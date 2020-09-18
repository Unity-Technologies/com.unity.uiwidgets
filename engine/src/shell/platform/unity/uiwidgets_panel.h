#pragma once

#include <flutter/fml/memory/ref_counted.h>

#include "gfx_worker_task_runner.h"
#include "runtime/mono_api.h"
#include "unity_surface_manager.h"
#include "win32_task_runner.h"

namespace uiwidgets {

class UIWidgetsPanel : public fml::RefCountedThreadSafe<UIWidgetsPanel> {
  FML_FRIEND_MAKE_REF_COUNTED(UIWidgetsPanel);

 public:
  typedef void (*EntrypointCallback)(Mono_Handle handle);

  static fml::RefPtr<UIWidgetsPanel> Create(
      Mono_Handle handle, EntrypointCallback entrypoint_callback);

  ~UIWidgetsPanel();

  void OnEnable(void* native_texture_ptr, size_t width, size_t height,
                float device_pixel_ratio, const char* streaming_assets_path);

  void MonoEntrypoint();

  void OnDisable();

  void OnRenderTexture(void* native_texture_ptr, size_t width, size_t height,
                       float dpi);

  std::chrono::nanoseconds ProcessMessages();

  void ProcessVSync();

  void VSyncCallback(intptr_t baton);

 private:
  UIWidgetsPanel(Mono_Handle handle, EntrypointCallback entrypoint_callback);

  Mono_Handle handle_;
  EntrypointCallback entrypoint_callback_;

  std::unique_ptr<UnitySurfaceManager> surface_manager_;
  GLuint fbo_ = 0;

  std::unique_ptr<GfxWorkerTaskRunner> gfx_worker_task_runner_;
  std::unique_ptr<Win32TaskRunner> task_runner_;
  UIWidgetsEngine engine_ = nullptr;

  std::vector<intptr_t> vsync_batons_;
};

}  // namespace uiwidgets
