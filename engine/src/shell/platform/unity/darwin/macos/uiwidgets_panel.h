#pragma once

#include <mutex>
#include <queue>
#include <map>

#include <flutter/fml/memory/ref_counted.h>
#include "shell/platform/unity/gfx_worker_task_runner.h"
#include "runtime/mono_api.h"
#include "cocoa_task_runner.h"
#include "unity_surface_manager.h"

namespace uiwidgets {

struct MouseState {
  bool state_is_down = false;
  bool state_is_added = false;
  uint64_t buttons = 0;
};

class UIWidgetsPanel : public fml::RefCountedThreadSafe<UIWidgetsPanel> {
  FML_FRIEND_MAKE_REF_COUNTED(UIWidgetsPanel);

 public:
  typedef void (*EntrypointCallback)(Mono_Handle handle);

  static fml::RefPtr<UIWidgetsPanel> Create(
      Mono_Handle handle, EntrypointCallback entrypoint_callback);

  ~UIWidgetsPanel();

  void* OnEnable(size_t width, size_t height, float device_pixel_ratio, const char* streaming_assets_path);

  void MonoEntrypoint();

  void OnDisable();

  void* OnRenderTexture(size_t width, size_t height,
                       float dpi);
  
  bool ReleaseNativeRenderTexture();

  int RegisterTexture(void* native_texture_ptr);

  void UnregisterTexture(int texture_id);

  std::chrono::nanoseconds ProcessMessages();

  void ProcessVSync();

  void VSyncCallback(intptr_t baton);

  void OnMouseMove(float x, float y);

  void OnMouseDown(float x, float y, int button);

  void OnMouseUp(float x, float y, int button);

  void OnMouseLeave();

 private:
  UIWidgetsPanel(Mono_Handle handle, EntrypointCallback entrypoint_callback);

  void CreateInternalUIWidgetsEngine(size_t width, size_t height, float device_pixel_ratio, const char* streaming_assets_path);

  MouseState GetMouseState() { return mouse_state_; }

  void ResetMouseState() { mouse_state_ = MouseState(); }

  void SetMouseStateDown(bool is_down) { mouse_state_.state_is_down = is_down; }

  void SetMouseStateAdded(bool is_added) {
    mouse_state_.state_is_added = is_added;
  }

  void SetMouseButtons(uint64_t buttons) { mouse_state_.buttons = buttons; }

  void SendMouseMove(float x, float y);

  void SendMouseDown(float x, float y);

  void SendMouseUp(float x, float y);

  void SendMouseLeave();

  void SetEventPhaseFromCursorButtonState(UIWidgetsPointerEvent* event_data);

  void SendPointerEventWithData(const UIWidgetsPointerEvent& event_data);

  Mono_Handle handle_;
  EntrypointCallback entrypoint_callback_;

  std::unique_ptr<UnitySurfaceManager> surface_manager_;

  std::unique_ptr<GfxWorkerTaskRunner> gfx_worker_task_runner_;
  std::unique_ptr<CocoaTaskRunner> task_runner_;
  UIWidgetsEngine engine_ = nullptr;

  std::vector<intptr_t> vsync_batons_;

  MouseState mouse_state_;
  bool process_events_ = false;
};

}  // namespace uiwidgets
