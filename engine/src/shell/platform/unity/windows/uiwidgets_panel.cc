#include "uiwidgets_panel.h"

#include <Windows.h>
#include <flutter/fml/synchronization/waitable_event.h>
#include <include\utils\SkBase64.h>

#include <fstream>
#include <iostream>

#include "lib/ui/window/viewport_metrics.h"
#include "runtime/mono_api.h"
#include "shell/common/switches.h"
#include "shell/platform/embedder/embedder_engine.h"
#include "uiwidgets_system.h"
#include "unity_external_texture_gl.h"

namespace uiwidgets {

fml::RefPtr<UIWidgetsPanel> UIWidgetsPanel::Create(
    Mono_Handle handle, UIWidgetsWindowType window_type, EntrypointCallback entrypoint_callback) {
  return fml::MakeRefCounted<UIWidgetsPanel>(handle, window_type, entrypoint_callback);
}

UIWidgetsPanel::UIWidgetsPanel(Mono_Handle handle,
                               UIWidgetsWindowType window_type, 
                               EntrypointCallback entrypoint_callback)
    : handle_(handle), window_type_(window_type), entrypoint_callback_(entrypoint_callback) {}

UIWidgetsPanel::~UIWidgetsPanel() = default;

bool UIWidgetsPanel::NeedUpdateByPlayerLoop() {
  return window_type_ == GameObjectPanel;
}

bool UIWidgetsPanel::NeedUpdateByEditorLoop() {
  return window_type_ == EditorWindowPanel;
}

void* UIWidgetsPanel::OnEnable(size_t width,
                              size_t height, float device_pixel_ratio,
                              const char* streaming_assets_path,
                              const char* settings) {

	fml::AutoResetWaitableEvent latch;
  std::thread::id gfx_worker_thread_id;
  UIWidgetsSystem::GetInstancePtr()->PostTaskToGfxWorker(
      [&latch, &gfx_worker_thread_id]() -> void {
        gfx_worker_thread_id = std::this_thread::get_id();
        latch.Signal();
      });
  latch.Wait();

  surface_manager_ = std::make_unique<UnitySurfaceManager>(
      UIWidgetsSystem::GetInstancePtr()->GetUnityInterfaces());

  FML_DCHECK(fbo_ == 0);
  surface_manager_->MakeCurrent(EGL_NO_DISPLAY);
  fbo_ = surface_manager_->CreateRenderSurface(width, height);
  void* d3dtexture = surface_manager_->GetD3DInnerTexture();
  
  surface_manager_->ClearCurrent();
  
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
  args.font_asset = settings;

  // std::string icu_path = std::string(streaming_assets_path) + "/icudtl.dat";
  // args.icu_data_path = icu_path.c_str();

  args.icu_mapper = GetICUStaticMapping;

  // Used for IOS build
  // std::string icu_symbol_prefix = "_binary_icudtl_dat_start";
  // std::string native_lib_path =
  // "pathtodll/Plugins/x86_64/libUIWidgets_d.dll"; args.icu_mapper =
  // [icu_symbol_prefix, native_lib_path] {
  //  return GetSymbolMapping(icu_symbol_prefix, native_lib_path);
  // };
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
    return nullptr;
  }

  engine_ = engine;
  UIWidgetsEngineRunInitialized(engine);

  UIWidgetsSystem::GetInstancePtr()->RegisterPanel(this);

  process_events_ = true;

  return static_cast<void*>(d3dtexture);
}

void UIWidgetsPanel::MonoEntrypoint() { entrypoint_callback_(handle_); }

void UIWidgetsPanel::OnDisable() {
  // drain pending messages
  ProcessMessages();

  // drain pending vsync batons
  ProcessVSync();

  process_events_ = false;

  UIWidgetsSystem::GetInstancePtr()->UnregisterPanel(this);

  if (engine_) {
    UIWidgetsEngineShutdown(engine_);
    engine_ = nullptr;
  }

  gfx_worker_task_runner_ = nullptr;
  task_runner_ = nullptr;

  if (fbo_) {
    surface_manager_->MakeCurrent(EGL_NO_DISPLAY);

    surface_manager_->ReleaseNativeRenderTexture();
    fbo_ = 0;

    surface_manager_->ClearCurrent();
  }

  surface_manager_ = nullptr;
}

void* UIWidgetsPanel::OnRenderTexture(size_t width,
                                     size_t height, float device_pixel_ratio) {
  ViewportMetrics metrics;
  metrics.physical_width = static_cast<float>(width);
  metrics.physical_height = static_cast<float>(height);
  metrics.device_pixel_ratio = device_pixel_ratio;
  reinterpret_cast<EmbedderEngine*>(engine_)->SetViewportMetrics(metrics);

  fbo_ = surface_manager_->CreateRenderSurface(width, height);
  return static_cast<void*>(surface_manager_->GetD3DInnerTexture());
}

bool UIWidgetsPanel::ReleaseNativeRenderTexture() { return surface_manager_->ReleaseNativeRenderTexture(); }

int UIWidgetsPanel::RegisterTexture(void* native_texture_ptr) {
  static int texture_identifier = 0;
  texture_identifier++;

  auto* engine = reinterpret_cast<EmbedderEngine*>(engine_);

  engine->GetShell().GetPlatformView()->RegisterTexture(
      std::make_unique<UnityExternalTextureGL>(
          texture_identifier, native_texture_ptr, surface_manager_.get()));
  return texture_identifier;
}

void UIWidgetsPanel::UnregisterTexture(int texture_id) {
  auto* engine = reinterpret_cast<EmbedderEngine*>(engine_);
  engine->GetShell().GetPlatformView()->UnregisterTexture(texture_id);
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

void UIWidgetsPanel::SetEventLocationFromCursorPosition(
    UIWidgetsPointerEvent* event_data) {
  POINT point;
  GetCursorPos(&point);

  // TODO: this is a native method, use unity position instead.
  // ScreenToClient(GetWindowHandle(), &point);

  event_data->x = point.x;
  event_data->y = point.y;
}

void UIWidgetsPanel::SetEventPhaseFromCursorButtonState(
    UIWidgetsPointerEvent* event_data) {
  MouseState state = GetMouseState();
  event_data->phase = state.buttons == 0
                          ? state.state_is_down ? UIWidgetsPointerPhase::kUp
                                                : UIWidgetsPointerPhase::kHover
                          : state.state_is_down ? UIWidgetsPointerPhase::kMove
                                                : UIWidgetsPointerPhase::kDown;
}

void UIWidgetsPanel::SendMouseMove(float x, float y) {
  UIWidgetsPointerEvent event = {};
  event.x = x;
  event.y = y;
  SetEventPhaseFromCursorButtonState(&event);
  SendPointerEventWithData(event);
}

void UIWidgetsPanel::SendMouseDown(float x, float y) {
  UIWidgetsPointerEvent event = {};
  SetEventPhaseFromCursorButtonState(&event);
  event.x = x;
  event.y = y;
  SendPointerEventWithData(event);
  SetMouseStateDown(true);
}

void UIWidgetsPanel::SendMouseUp(float x, float y) {
  UIWidgetsPointerEvent event = {};
  SetEventPhaseFromCursorButtonState(&event);
  event.x = x;
  event.y = y;
  SendPointerEventWithData(event);
  if (event.phase == UIWidgetsPointerPhase::kUp) {
    SetMouseStateDown(false);
  }
}

void UIWidgetsPanel::SendMouseLeave() {
  UIWidgetsPointerEvent event = {};
  event.phase = UIWidgetsPointerPhase::kRemove;
  SendPointerEventWithData(event);
}

void UIWidgetsPanel::SendScroll(float delta_x, float delta_y, float px, float py) {
  UIWidgetsPointerEvent event = {};
 // TODO: this is a native method, use unity position instead.
  event.x = px;
  event.y = py;
  //SetEventLocationFromCursorPosition(&event);
  SetEventPhaseFromCursorButtonState(&event);
  event.signal_kind = UIWidgetsPointerSignalKind::kUIWidgetsPointerSignalKindScroll;
  // TODO: See if this can be queried from the OS; this value is chosen
  // arbitrarily to get something that feels reasonable.
  const int kScrollOffsetMultiplier = 20;
  event.scroll_delta_x = delta_x * kScrollOffsetMultiplier;
  event.scroll_delta_y = delta_y * kScrollOffsetMultiplier;
  SendPointerEventWithData(event);
}

void UIWidgetsPanel::SendPointerEventWithData(
    const UIWidgetsPointerEvent& event_data) {
  MouseState mouse_state = GetMouseState();
  // If sending anything other than an add, and the pointer isn't already added,
  // synthesize an add to satisfy Flutter's expectations about events.
  if (!mouse_state.state_is_added &&
      event_data.phase != UIWidgetsPointerPhase::kAdd) {
    UIWidgetsPointerEvent event = {};
    event.phase = UIWidgetsPointerPhase::kAdd;
    event.x = event_data.x;
    event.y = event_data.y;
    event.buttons = 0;
    SendPointerEventWithData(event);
  }
  // Don't double-add (e.g., if events are delivered out of order, so an add has
  // already been synthesized).
  if (mouse_state.state_is_added &&
      event_data.phase == UIWidgetsPointerPhase::kAdd) {
    return;
  }

  UIWidgetsPointerEvent event = event_data;
  event.device_kind = kUIWidgetsPointerDeviceKindMouse;
  event.buttons = mouse_state.buttons;

  // Set metadata that's always the same regardless of the event.
  event.struct_size = sizeof(event);
  event.timestamp =
      std::chrono::duration_cast<std::chrono::microseconds>(
          std::chrono::high_resolution_clock::now().time_since_epoch())
          .count();

  UIWidgetsEngineSendPointerEvent(engine_, &event, 1);

  if (event_data.phase == UIWidgetsPointerPhase::kAdd) {
    SetMouseStateAdded(true);
  } else if (event_data.phase == UIWidgetsPointerPhase::kRemove) {
    SetMouseStateAdded(false);
    ResetMouseState();
  }
}

void UIWidgetsPanel::OnKeyDown(int keyCode, bool isKeyDown, int64_t modifier) {
  if (process_events_) {
    UIWidgetsPointerEvent event = {};
    event.phase = isKeyDown ? UIWidgetsPointerPhase::kMouseDown : UIWidgetsPointerPhase::kMouseUp;
    event.device_kind =
        UIWidgetsPointerDeviceKind::kUIWidgetsPointerDeviceKindKeyboard;
    event.buttons = keyCode;
    event.struct_size = sizeof(event);
    event.timestamp =
        std::chrono::duration_cast<std::chrono::microseconds>(
            std::chrono::high_resolution_clock::now().time_since_epoch())
            .count();
    event.modifier = modifier;

    UIWidgetsEngineSendPointerEvent(engine_, &event, 1);
  }
}

void UIWidgetsPanel::OnMouseMove(float x, float y) {
  if (process_events_) {
    SendMouseMove(x, y);
  }
}

void UIWidgetsPanel::OnScroll(float x, float y, float px, float py) {
  if (process_events_) {
    SendScroll(x, y, px, py);
  }
}

void UIWidgetsPanel::OnDragUpdateInEditor(float x, float y)
{
  if (process_events_) {
    UIWidgetsPointerEvent event = {};
    event.x = x;
    event.y = y;
    SetEventPhaseFromCursorButtonState(&event);
    event.signal_kind = UIWidgetsPointerSignalKind::kUIWidgetsPointerSignalKindEditorDragUpdate;

    SendPointerEventWithData(event);
  }
}

void UIWidgetsPanel::OnDragReleaseInEditor(float x, float y)
{
  if (process_events_) {
    UIWidgetsPointerEvent event = {};
    event.x = x;
    event.y = y;
    SetEventPhaseFromCursorButtonState(&event);
    event.signal_kind = UIWidgetsPointerSignalKind::kUIWidgetsPointerSignalKindEditorDragRelease;

    SendPointerEventWithData(event);
  }
}

static uint64_t ConvertToUIWidgetsButton(int button) {
  switch (button) {
    case -1:
      return kUIWidgetsPointerButtonMousePrimary;
    case -2:
      return kUIWidgetsPointerButtonMouseSecondary;
    case -3:
      return kUIWidgetsPointerButtonMouseMiddle;
  }
  std::cerr << "Mouse button not recognized: " << button << std::endl;
  return 0;
}

void UIWidgetsPanel::OnMouseDown(float x, float y, int button) {
  if (process_events_) {
    uint64_t uiwidgets_button = ConvertToUIWidgetsButton(button);
    if (uiwidgets_button != 0) {
      uint64_t mouse_buttons = GetMouseState().buttons | uiwidgets_button;
      SetMouseButtons(mouse_buttons);
      SendMouseDown(x, y);
    }
  }
}

void UIWidgetsPanel::OnMouseUp(float x, float y, int button) {
  if (process_events_) {
    uint64_t uiwidgets_button = ConvertToUIWidgetsButton(button);
    if (uiwidgets_button != 0) {
      uint64_t mouse_buttons = GetMouseState().buttons & ~uiwidgets_button;
      SetMouseButtons(mouse_buttons);
      SendMouseUp(x, y);
    }
  }
}

void UIWidgetsPanel::OnMouseLeave() {
  if (process_events_) {
    SendMouseLeave();
  }
}

UIWIDGETS_API(UIWidgetsPanel*)
UIWidgetsPanel_constructor(
    Mono_Handle handle, int windowType,
    UIWidgetsPanel::EntrypointCallback entrypoint_callback) {
  UIWidgetsWindowType window_type = static_cast<UIWidgetsWindowType>(windowType);
  const auto panel = UIWidgetsPanel::Create(handle, window_type, entrypoint_callback);
  panel->AddRef();
  return panel.get();
}

UIWIDGETS_API(void) UIWidgetsPanel_dispose(UIWidgetsPanel* panel) {
  panel->Release();
}

UIWIDGETS_API(void*)
UIWidgetsPanel_onEnable(UIWidgetsPanel* panel,
                        size_t width, size_t height, float device_pixel_ratio,
                        const char* streaming_assets_path,
                        const char* settings) {
  return panel->OnEnable(width, height, device_pixel_ratio,
                  streaming_assets_path, settings);
}

UIWIDGETS_API(void) UIWidgetsPanel_onDisable(UIWidgetsPanel* panel) {
  panel->OnDisable();
}

UIWIDGETS_API(bool) UIWidgetsPanel_releaseNativeTexture(UIWidgetsPanel* panel) {
  return panel->ReleaseNativeRenderTexture();
}

UIWIDGETS_API(void*)
UIWidgetsPanel_onRenderTexture(UIWidgetsPanel* panel,
                               int width, int height, float dpi) {
  return panel->OnRenderTexture(width, height, dpi);
}

UIWIDGETS_API(int)
UIWidgetsPanel_registerTexture(UIWidgetsPanel* panel,
                               void* native_texture_ptr) {
  return panel->RegisterTexture(native_texture_ptr);
}

UIWIDGETS_API(void)
UIWidgetsPanel_unregisterTexture(UIWidgetsPanel* panel, int texture_id) {
  panel->UnregisterTexture(texture_id);
}


UIWIDGETS_API(void)
UIWidgetsPanel_onKey(UIWidgetsPanel* panel, int keyCode, bool isKeyDown, int64_t modifier) {
  panel->OnKeyDown(keyCode, isKeyDown, modifier);
}

UIWIDGETS_API(void)
UIWidgetsPanel_onMouseDown(UIWidgetsPanel* panel, float x, float y,
                           int button) {
  panel->OnMouseDown(x, y, button);
}

UIWIDGETS_API(void)
UIWidgetsPanel_onMouseUp(UIWidgetsPanel* panel, float x, float y, int button) {
  panel->OnMouseUp(x, y, button);
}

UIWIDGETS_API(void)
UIWidgetsPanel_onMouseMove(UIWidgetsPanel* panel, float x, float y, int button) {
  //button id is not useful for desktop since the motions happens on the mouse (including all buttons)
  panel->OnMouseMove(x, y);
}

UIWIDGETS_API(void)
UIWidgetsPanel_onMouseLeave(UIWidgetsPanel* panel) { panel->OnMouseLeave(); }

UIWIDGETS_API(void)
UIWidgetsPanel_onEditorUpdate(UIWidgetsPanel* panel) {
  if (!panel->NeedUpdateByEditorLoop()) {
    std::cerr << "only EditorWindowPanel can be updated using onEditorUpdate" << std::endl;
    return;
  }

  //_Update
  panel->ProcessMessages();

  //_ProcessVSync
  panel->ProcessVSync();

  //_Wait
  panel->ProcessMessages();
}

UIWIDGETS_API(void)
UIWidgetsPanel_onScroll(UIWidgetsPanel* panel, float x, float y, float px, float py) {
  panel->OnScroll(x, y, px, py);
}

UIWIDGETS_API(void)
UIWidgetsPanel_onDragUpdateInEditor(UIWidgetsPanel* panel, float x, float y) {
  if (!panel->NeedUpdateByEditorLoop()) {
    return;
  }
  panel->OnDragUpdateInEditor(x, y);
}

UIWIDGETS_API(void)
UIWidgetsPanel_onDragReleaseInEditor(UIWidgetsPanel* panel, float x, float y) {
  if (!panel->NeedUpdateByEditorLoop()) {
    return;
  }
  panel->OnDragReleaseInEditor(x, y);
}

}  // namespace uiwidgets