#include <flutter/fml/synchronization/waitable_event.h>

#include <iostream>

#include "lib/ui/window/viewport_metrics.h"
#include "runtime/mono_api.h"
#include "shell/platform/embedder/embedder_engine.h"
#include "shell/platform/embedder/embedder.h"
#include "shell/common/switches.h"
#include "Unity/IUnityGraphics.h"
#include "Unity/IUnityGraphicsMetal.h"

#include "uiwidgets_system.h"
#include "uiwidgets_panel.h"

namespace uiwidgets {

fml::RefPtr<UIWidgetsPanel> UIWidgetsPanel::Create(
    Mono_Handle handle, EntrypointCallback entrypoint_callback) {
  return fml::MakeRefCounted<UIWidgetsPanel>(handle, entrypoint_callback);
}

UIWidgetsPanel::UIWidgetsPanel(Mono_Handle handle,
                               EntrypointCallback entrypoint_callback)
    : handle_(handle), entrypoint_callback_(entrypoint_callback) {}

UIWidgetsPanel::~UIWidgetsPanel() = default;


void UIWidgetsPanel::CreateRenderTexture(size_t width, size_t height)
{
  //Constants
  const MTLPixelFormat ConstMetalViewPixelFormat = MTLPixelFormatBGRA8Unorm_sRGB;
  const int ConstCVPixelFormat = kCVPixelFormatType_32BGRA;
  const GLuint ConstGLInternalFormat = GL_SRGB8_ALPHA8;
  const GLuint ConstGLFormat = GL_BGRA;
  const GLuint ConstGLType = GL_UNSIGNED_INT_8_8_8_8_REV;

  //render context must be available
  FML_DCHECK(metal_device_ != nullptr && gl_context_ != nullptr && gl_resource_context_ != nullptr);

  //render textures must be released already
  FML_DCHECK(pixelbuffer_ref == nullptr && default_fbo_ == 0 && gl_tex_ == 0 && gl_tex_cache_ref_ == nullptr && gl_tex_ref_ == nullptr && metal_tex_ == nullptr && metal_tex_ref_ == nullptr && metal_tex_cache_ref_ == nullptr);
  //create pixel buffer
  auto gl_pixelformat_ = gl_context_.pixelFormat.CGLPixelFormatObj;

  NSDictionary* cvBufferProperties = @{
    (__bridge NSString*)kCVPixelBufferOpenGLCompatibilityKey : @YES,
    (__bridge NSString*)kCVPixelBufferMetalCompatibilityKey : @YES,
  };

  CVReturn cvret = CVPixelBufferCreate(kCFAllocatorDefault,
            width, height,
            ConstCVPixelFormat,
            (__bridge CFDictionaryRef)cvBufferProperties,
            &pixelbuffer_ref);
  FML_DCHECK(cvret == kCVReturnSuccess);

  //create metal texture
  cvret = CVMetalTextureCacheCreate(
            kCFAllocatorDefault,
            nil,
            metal_device_,
            nil,
            &metal_tex_cache_ref_);
  FML_DCHECK(cvret == kCVReturnSuccess);

  cvret = CVMetalTextureCacheCreateTextureFromImage(
            kCFAllocatorDefault,
            metal_tex_cache_ref_,
            pixelbuffer_ref, nil,
            ConstMetalViewPixelFormat,
            width, height,
            0,
            &metal_tex_ref_);
  FML_DCHECK(cvret == kCVReturnSuccess);

  metal_tex_ = CVMetalTextureGetTexture(metal_tex_ref_);

  //create opengl texture
  cvret  = CVOpenGLTextureCacheCreate(
            kCFAllocatorDefault,
            nil,
            gl_context_.CGLContextObj,
            gl_pixelformat_,
            nil,
            &gl_tex_cache_ref_);
  FML_DCHECK(cvret == kCVReturnSuccess);

  cvret = CVOpenGLTextureCacheCreateTextureFromImage(
            kCFAllocatorDefault,
            gl_tex_cache_ref_,
            pixelbuffer_ref,
            nil,
            &gl_tex_ref_);
  FML_DCHECK(cvret == kCVReturnSuccess);

  gl_tex_ = CVOpenGLTextureGetName(gl_tex_ref_);

  //initialize gl renderer
  [gl_context_ makeCurrentContext];
  glGenFramebuffers(1, &default_fbo_);
  glBindFramebuffer(GL_FRAMEBUFFER, default_fbo_);

  const GLenum texType = GL_TEXTURE_RECTANGLE;
  glFramebufferTexture2D(GL_FRAMEBUFFER, GL_COLOR_ATTACHMENT0, texType, gl_tex_, 0);
}

void UIWidgetsPanel::CreateRenderingContext()
{
  FML_DCHECK(metal_device_ == nullptr);

  //get main gfx device (metal)
  auto* graphics = UIWidgetsSystem::GetInstancePtr()
                    ->GetUnityInterfaces()
                    ->Get<IUnityGraphics>();
    
  FML_DCHECK(graphics->GetRenderer() == kUnityGfxRendererMetal);

  auto* metalGraphics = UIWidgetsSystem::GetInstancePtr()
                    ->GetUnityInterfaces()
                    ->Get<IUnityGraphicsMetalV1>();

  metal_device_ = metalGraphics->MetalDevice();

  //create opengl context
  FML_DCHECK(!gl_context_);
  FML_DCHECK(!gl_resource_context_);

  NSOpenGLPixelFormatAttribute attrs[] =
    {
      NSOpenGLPFAAccelerated,
      0
    };

  NSOpenGLPixelFormat *pixelFormat = [[NSOpenGLPixelFormat alloc] initWithAttributes:attrs];
  gl_context_ = [[NSOpenGLContext alloc] initWithFormat:pixelFormat shareContext:nil];
  gl_resource_context_ = [[NSOpenGLContext alloc] initWithFormat:pixelFormat shareContext:gl_context_];

  FML_DCHECK(gl_context_ != nullptr && gl_resource_context_ != nullptr);
}

bool UIWidgetsPanel::ClearCurrentContext()
{
  [NSOpenGLContext clearCurrentContext];
  return true;
}

bool UIWidgetsPanel::MakeCurrentContext()
{
  [gl_context_ makeCurrentContext];
  return true;
}

bool UIWidgetsPanel::MakeCurrentResourceContext()
{
  [gl_resource_context_ makeCurrentContext];
  return true;
}

uint32_t UIWidgetsPanel::GetFbo()
{
  return default_fbo_;
}

void* UIWidgetsPanel::OnEnable(size_t width, size_t height, float device_pixel_ratio, const char* streaming_assets_path)
{
  CreateRenderingContext();
  CreateRenderTexture(width, height);
  CreateInternalUIWidgetsEngine(width, height, device_pixel_ratio, streaming_assets_path);

  return (__bridge void*)metal_tex_;
}

void UIWidgetsPanel::CreateInternalUIWidgetsEngine(size_t width, size_t height, float device_pixel_ratio, const char* streaming_assets_path)
{
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

  //render task runner configs
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

  //renderer config
  UIWidgetsRendererConfig config = {};
  config.type = kOpenGL;
  config.open_gl.struct_size = sizeof(config.open_gl);
  config.open_gl.clear_current = [](void* user_data) -> bool {
    auto* panel = static_cast<UIWidgetsPanel*>(user_data);
    return panel->ClearCurrentContext();
  };
  config.open_gl.make_current = [](void* user_data) -> bool {
    auto* panel = static_cast<UIWidgetsPanel*>(user_data);
    return panel->MakeCurrentContext();
  };
  config.open_gl.make_resource_current = [](void* user_data) -> bool {
    auto* panel = static_cast<UIWidgetsPanel*>(user_data);
    return panel->MakeCurrentResourceContext();
  };
  config.open_gl.fbo_callback = [](void* user_data) -> uint32_t {
    auto* panel = static_cast<UIWidgetsPanel*>(user_data);
    return panel->GetFbo();
  };
  config.open_gl.present = [](void* user_data) -> bool { return true; };
  config.open_gl.fbo_reset_after_present = true;

  //main thread task runner
  task_runner_ = std::make_unique<CocoaTaskRunner>(
    [this](const auto* task) {
        if (UIWidgetsEngineRunTask(engine_, task) != kSuccess) {
          std::cerr << "Could not post an engine task." << std::endl;
        }
  });

  UIWidgetsTaskRunnerDescription main_task_runner = {};
  main_task_runner.struct_size = sizeof(UIWidgetsTaskRunnerDescription);
  main_task_runner.identifier = 2;
  main_task_runner.user_data = task_runner_.get();
  main_task_runner.runs_task_on_current_thread_callback = [](void* user_data) -> bool {
    return [[NSThread currentThread] isMainThread];
  };
  main_task_runner.post_task_callback = [](UIWidgetsTask task, uint64_t target_time_nanos,
                            void* user_data) -> void {
    static_cast<CocoaTaskRunner*>(user_data)->PostTask(task, target_time_nanos);
  };

  //setup custom task runners
  UIWidgetsCustomTaskRunners custom_task_runners = {};
  custom_task_runners.struct_size = sizeof(UIWidgetsCustomTaskRunners);
  custom_task_runners.platform_task_runner = &main_task_runner;
  custom_task_runners.ui_task_runner = &main_task_runner;
  custom_task_runners.render_task_runner = &render_task_runner;

  UIWidgetsProjectArgs args = {};
  args.struct_size = sizeof(UIWidgetsProjectArgs);
  args.assets_path = streaming_assets_path;
  //TODO: not support icu yet
  //args.icu_mapper = GetICUStaticMapping;
  args.command_line_argc = 0;
  args.command_line_argv = nullptr;
  args.platform_message_callback =
    [](const UIWidgetsPlatformMessage* engine_message,
        void* user_data) -> void {};

  args.custom_task_runners = &custom_task_runners;
  args.task_observer_add = [](intptr_t key, void* callback,
                              void* user_data) -> void {
    auto* panel = static_cast<UIWidgetsPanel*>(user_data);
    panel->task_runner_->AddTaskObserver(key, *static_cast<fml::closure*>(callback));
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
  process_events_ = true;
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

  //release all resources
  if (default_fbo_) {
    ReleaseNativeRenderTexture();
    ReleaseNativeRenderContext();
  }
}

void UIWidgetsPanel::ReleaseNativeRenderContext()
{
  FML_DCHECK(gl_resource_context_);
  CGLReleaseContext(gl_resource_context_.CGLContextObj);
  gl_resource_context_ = nullptr;

  FML_DCHECK(gl_context_);
  CGLReleaseContext(gl_context_.CGLContextObj);
  gl_context_ = nullptr;

  FML_DCHECK(metal_device_ != nullptr);
  metal_device_ = nullptr;
}

bool UIWidgetsPanel::ReleaseNativeRenderTexture()
{
  //release gl resources
  FML_DCHECK(default_fbo_ != 0);
  glDeleteFramebuffers(1, &default_fbo_);
  default_fbo_ = 0;

  FML_DCHECK(gl_tex_ != 0);
  glDeleteTextures(1, &gl_tex_);
  gl_tex_ = 0;

  CFRelease(gl_tex_cache_ref_);
  gl_tex_cache_ref_ = nullptr;

  CFRelease(gl_tex_ref_);
  gl_tex_ref_ = nullptr;

  //release metal resources
  //since ARC is enabled by default, no need to release the texture
  metal_tex_ = nullptr;

  CFRelease(metal_tex_ref_);
  metal_tex_ref_ = nullptr;

  CFRelease(metal_tex_cache_ref_);
  metal_tex_cache_ref_ = nullptr;

  //release cv pixelbuffer
  CVPixelBufferRelease(pixelbuffer_ref);
  pixelbuffer_ref = nullptr;

  return true;
}

void* UIWidgetsPanel::OnRenderTexture(size_t width,
                                     size_t height, float device_pixel_ratio) {
  ViewportMetrics metrics;
  metrics.physical_width = static_cast<float>(width);
  metrics.physical_height = static_cast<float>(height);
  metrics.device_pixel_ratio = device_pixel_ratio;
  reinterpret_cast<EmbedderEngine*>(engine_)->SetViewportMetrics(metrics);

  CreateRenderTexture(width, height);
  return (__bridge void*)metal_tex_;
}

int UIWidgetsPanel::RegisterTexture(void* native_texture_ptr) {
  /*
  int texture_identifier = 0;
  texture_identifier++;

  auto* engine = reinterpret_cast<EmbedderEngine*>(engine_);

  engine->GetShell().GetPlatformView()->RegisterTexture(
      std::make_unique<UnityExternalTextureGL>(
          texture_identifier, native_texture_ptr, surface_manager_.get()));
  return texture_identifier;*/
    return 0;
}

void UIWidgetsPanel::UnregisterTexture(int texture_id) {
  /*
  auto* engine = reinterpret_cast<EmbedderEngine*>(engine_);
  engine->GetShell().GetPlatformView()->UnregisterTexture(texture_id);*/
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

void UIWidgetsPanel::OnMouseMove(float x, float y) {
  if (process_events_) {
    SendMouseMove(x, y);
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
    Mono_Handle handle,
    UIWidgetsPanel::EntrypointCallback entrypoint_callback) {
  const auto panel = UIWidgetsPanel::Create(handle, entrypoint_callback);
  panel->AddRef();
  return panel.get();
}

UIWIDGETS_API(void) UIWidgetsPanel_dispose(UIWidgetsPanel* panel) {
  panel->Release();
}

UIWIDGETS_API(void*)
UIWidgetsPanel_onEnable(UIWidgetsPanel* panel,
                        size_t width, size_t height, float device_pixel_ratio,
                        const char* streaming_assets_path) {
  return panel->OnEnable(width, height, device_pixel_ratio,
                  streaming_assets_path);
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
UIWidgetsPanel_onMouseDown(UIWidgetsPanel* panel, float x, float y,
                           int button) {
  panel->OnMouseDown(x, y, button);
}

UIWIDGETS_API(void)
UIWidgetsPanel_onMouseUp(UIWidgetsPanel* panel, float x, float y, int button) {
  panel->OnMouseUp(x, y, button);
}

UIWIDGETS_API(void)
UIWidgetsPanel_onMouseMove(UIWidgetsPanel* panel, float x, float y) {
  panel->OnMouseMove(x, y);
}

UIWIDGETS_API(void)
UIWidgetsPanel_onMouseLeave(UIWidgetsPanel* panel) { panel->OnMouseLeave(); }

}  // namespace uiwidgets
