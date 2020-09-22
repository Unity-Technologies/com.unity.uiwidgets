#include "uiwidgets_panel.h"

#include <Windows.h>
#include <flutter/fml/synchronization/waitable_event.h>

#include <iostream>

#include "lib/ui/window/viewport_metrics.h"
#include "runtime/mono_api.h"
#include "shell/platform/embedder/embedder_engine.h"
#include "uiwidgets_system.h"

namespace uiwidgets {

fml::RefPtr<UIWidgetsPanel> UIWidgetsPanel::Create(
    Mono_Handle handle, EntrypointCallback entrypoint_callback) {
  return fml::MakeRefCounted<UIWidgetsPanel>(handle, entrypoint_callback);
}

UIWidgetsPanel::UIWidgetsPanel(Mono_Handle handle,
                               EntrypointCallback entrypoint_callback)
    : handle_(handle), entrypoint_callback_(entrypoint_callback) {}

UIWidgetsPanel::~UIWidgetsPanel() = default;

void UIWidgetsPanel::OnEnable(void* native_texture_ptr, size_t width,
                              size_t height, float device_pixel_ratio,
                              const char* streaming_assets_path) {
  surface_manager_ = std::make_unique<UnitySurfaceManager>(
      UIWidgetsSystem::GetInstancePtr()->GetUnityInterfaces());

  FML_DCHECK(fbo_ == 0);
  surface_manager_->MakeCurrent(EGL_NO_DISPLAY);
  fbo_ = surface_manager_->CreateRenderSurface(native_texture_ptr);
  surface_manager_->ClearCurrent();

  fml::AutoResetWaitableEvent latch;
  std::thread::id gfx_worker_thread_id;
  UIWidgetsSystem::GetInstancePtr()->PostTaskToGfxWorker(
      [&latch, &gfx_worker_thread_id]() -> void {
        gfx_worker_thread_id = std::this_thread::get_id();
        latch.Signal();
      });
  latch.Wait();

  gfx_worker_task_runner_ = std::make_unique<GfxWorkerTaskRunner>(
      gfx_worker_thread_id, [this](const auto* task) {
        if (UIWidgetsEngineRunTask(engine_, task) != kSuccess) {
          std::cerr << "Could not post an gfx worker task." << std::endl;
        }
      });

  UIWidgetsTaskRunnerDescription render_task_runner = {};
  render_task_runner.struct_size = sizeof(UIWidgetsTaskRunnerDescription);
  render_task_runner.identifier = 1;
  render_task_runner.user_data = gfx_worker_task_runner_.get();
  render_task_runner.runs_task_on_current_thread_callback =
      [](void* user_data) -> bool {
    return static_cast<GfxWorkerTaskRunner*>(user_data)
        ->RunsTasksOnCurrentThread();
  };
  render_task_runner.post_task_callback = [](UIWidgetsTask task,
                                             uint64_t target_time_nanos,
                                             void* user_data) -> void {
    static_cast<GfxWorkerTaskRunner*>(user_data)->PostTask(task,
                                                           target_time_nanos);
  };

  UIWidgetsRendererConfig config = {};
  config.type = kOpenGL;
  config.open_gl.struct_size = sizeof(config.open_gl);
  config.open_gl.clear_current = [](void* user_data) -> bool {
    auto* panel = static_cast<UIWidgetsPanel*>(user_data);
    return panel->surface_manager_->ClearCurrent();
  };
  config.open_gl.make_current = [](void* user_data) -> bool {
    auto* panel = static_cast<UIWidgetsPanel*>(user_data);
    return panel->surface_manager_->MakeCurrent(EGL_NO_DISPLAY);
  };
  config.open_gl.make_resource_current = [](void* user_data) -> bool {
    auto* panel = static_cast<UIWidgetsPanel*>(user_data);
    return panel->surface_manager_->MakeResourceCurrent();
  };
  config.open_gl.fbo_callback = [](void* user_data) -> uint32_t {
    auto* panel = static_cast<UIWidgetsPanel*>(user_data);
    return panel->fbo_;
  };
  config.open_gl.present = [](void* user_data) -> bool { return true; };
  config.open_gl.gl_proc_resolver = [](void* user_data,
                                       const char* what) -> void* {
    return reinterpret_cast<void*>(eglGetProcAddress(what));
  };
  config.open_gl.fbo_reset_after_present = true;

  task_runner_ = std::make_unique<Win32TaskRunner>(
      GetCurrentThreadId(), [this](const auto* task) {
        if (UIWidgetsEngineRunTask(engine_, task) != kSuccess) {
          std::cerr << "Could not post an engine task." << std::endl;
        }
      });

  UIWidgetsTaskRunnerDescription ui_task_runner = {};
  ui_task_runner.struct_size = sizeof(UIWidgetsTaskRunnerDescription);
  ui_task_runner.identifier = 2;
  ui_task_runner.user_data = task_runner_.get();
  ui_task_runner.runs_task_on_current_thread_callback =
      [](void* user_data) -> bool {
    return static_cast<Win32TaskRunner*>(user_data)->RunsTasksOnCurrentThread();
  };
  ui_task_runner.post_task_callback = [](UIWidgetsTask task,
                                         uint64_t target_time_nanos,
                                         void* user_data) -> void {
    static_cast<Win32TaskRunner*>(user_data)->PostTask(task, target_time_nanos);
  };

  UIWidgetsCustomTaskRunners custom_task_runners = {};
  custom_task_runners.struct_size = sizeof(UIWidgetsCustomTaskRunners);
  custom_task_runners.platform_task_runner = &ui_task_runner;
  custom_task_runners.ui_task_runner = &ui_task_runner;
  custom_task_runners.render_task_runner = &render_task_runner;

  UIWidgetsProjectArgs args = {};
  args.struct_size = sizeof(UIWidgetsProjectArgs);

  args.assets_path = streaming_assets_path;
  std::string icu_path = std::string(streaming_assets_path) + "/icudtl.dat";
  args.icu_data_path = icu_path.c_str();

  args.command_line_argc = 0;
  args.command_line_argv = nullptr;

  args.platform_message_callback =
      [](const UIWidgetsPlatformMessage* engine_message,
         void* user_data) -> void {};

  args.custom_task_runners = &custom_task_runners;
  args.task_observer_add = [](intptr_t key, void* callback,
                              void* user_data) -> void {
    auto* panel = static_cast<UIWidgetsPanel*>(user_data);
    panel->task_runner_->AddTaskObserver(key,
                                         *static_cast<fml::closure*>(callback));
  };
  args.task_observer_remove = [](intptr_t key, void* user_data) -> void {
    auto* panel = static_cast<UIWidgetsPanel*>(user_data);
    panel->task_runner_->RemoveTaskObserver(key);
  };

  args.custom_mono_entrypoint = [](void* user_data) -> void {
    auto* panel = static_cast<UIWidgetsPanel*>(user_data);
    panel->MonoEntrypoint();
  };

  args.vsync_callback = [](void* user_data, intptr_t baton) -> void {
    auto* panel = static_cast<UIWidgetsPanel*>(user_data);
    panel->VSyncCallback(baton);
  };

  args.initial_window_metrics.width = width;
  args.initial_window_metrics.height = height;
  args.initial_window_metrics.pixel_ratio = device_pixel_ratio;

  UIWidgetsEngine engine = nullptr;
  auto result = UIWidgetsEngineInitialize(&config, &args, this, &engine);

  if (result != kSuccess || engine == nullptr) {
    std::cerr << "Failed to start UIWidgets engine: error " << result
              << std::endl;
    return;
  }

  engine_ = engine;
  UIWidgetsEngineRunInitialized(engine);

  UIWidgetsSystem::GetInstancePtr()->RegisterPanel(this);
}

void UIWidgetsPanel::MonoEntrypoint() { entrypoint_callback_(handle_); }

void UIWidgetsPanel::OnDisable() {
  // drain pending messages
  ProcessMessages();

  // drain pending vsync batons
  ProcessVSync();

  UIWidgetsSystem::GetInstancePtr()->UnregisterPanel(this);

  if (engine_) {
    UIWidgetsEngineShutdown(engine_);
    engine_ = nullptr;
  }

  gfx_worker_task_runner_ = nullptr;
  task_runner_ = nullptr;

  if (fbo_) {
    surface_manager_->MakeCurrent(EGL_NO_DISPLAY);

    surface_manager_->DestroyRenderSurface();
    fbo_ = 0;

    surface_manager_->ClearCurrent();
  }

  surface_manager_ = nullptr;
}

void UIWidgetsPanel::OnRenderTexture(void* native_texture_ptr, size_t width,
                                     size_t height, float device_pixel_ratio) {
  reinterpret_cast<EmbedderEngine*>(engine_)->PostRenderThreadTask(
      [this, native_texture_ptr]() -> void {
        surface_manager_->MakeCurrent(EGL_NO_DISPLAY);

        if (fbo_) {
          surface_manager_->DestroyRenderSurface();
          fbo_ = 0;
        }
        fbo_ = surface_manager_->CreateRenderSurface(native_texture_ptr);

        surface_manager_->ClearCurrent();
      });

  ViewportMetrics metrics;
  metrics.physical_width = static_cast<float>(width);
  metrics.physical_height = static_cast<float>(height);
  metrics.device_pixel_ratio = device_pixel_ratio;
  reinterpret_cast<EmbedderEngine*>(engine_)->SetViewportMetrics(metrics);
}

std::chrono::nanoseconds UIWidgetsPanel::ProcessMessages() {
  return std::chrono::nanoseconds(task_runner_->ProcessTasks().count());
}

void UIWidgetsPanel::ProcessVSync() {
  std::vector<intptr_t> batons;
  vsync_batons_.swap(batons);

  for (intptr_t baton : batons) {
    reinterpret_cast<EmbedderEngine*>(engine_)->OnVsyncEvent(
        baton, fml::TimePoint::Now(),
        fml::TimePoint::Now() +
            fml::TimeDelta::FromNanoseconds(1000000000 / 60));
  }
}

void UIWidgetsPanel::VSyncCallback(intptr_t baton) {
  vsync_batons_.push_back(baton);
}

using TimePoint = std::chrono::steady_clock::time_point;

TimePoint next_flutter_event_time = TimePoint::clock::now();

UIWIDGETS_API(UIWidgetsPanel*)
UIWidgetsPanel_constructor(
    Mono_Handle handle,
    UIWidgetsPanel::EntrypointCallback entrypoint_callback) {
  const auto panel = UIWidgetsPanel::Create(handle, entrypoint_callback);
  panel->AddRef();
  return panel.get();
}

UIWIDGETS_API(void) UIWidgetsPanel_dispose(UIWidgetsPanel* panel) {
  panel->Release();
}

UIWIDGETS_API(void)
UIWidgetsPanel_onEnable(UIWidgetsPanel* panel, void* native_texture_ptr,
                        size_t width, size_t height, float device_pixel_ratio,
                        const char* streaming_assets_path) {
  panel->OnEnable(native_texture_ptr, width, height, device_pixel_ratio,
                  streaming_assets_path);
}

UIWIDGETS_API(void) UIWidgetsPanel_onDisable(UIWidgetsPanel* panel) {
  panel->OnDisable();
}

UIWIDGETS_API(void)
UIWidgetsPanel_onRenderTexture(UIWidgetsPanel* panel, void* native_texture_ptr,
                               int width, int height, float dpi) {
  panel->OnRenderTexture(native_texture_ptr, width, height, dpi);
}

}  // namespace uiwidgets