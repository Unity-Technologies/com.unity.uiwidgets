#pragma once

#include <mutex>
#include <queue>
#include <map>

#include <OpenGL/gl3.h>
#include <AppKit/AppKit.h>
#include <Metal/Metal.h>
#include <CoreVideo/CoreVideo.h>

#include <flutter/fml/memory/ref_counted.h>
#include "shell/platform/unity/gfx_worker_task_runner.h"
#include "runtime/mono_api.h"

namespace uiwidgets {

struct MouseState {
  bool state_is_down = false;
  bool state_is_added = false;
  uint64_t buttons = 0;
};

// ------- task runner -------
using TaskTimePoint = std::chrono::steady_clock::time_point;

struct Task {
  uint64_t order;
  TaskTimePoint fire_time;
  UIWidgetsTask task;

  struct Comparer {
    bool operator()(const Task& a, const Task& b) {
      if (a.fire_time == b.fire_time) {
        return a.order > b.order;
      }
      return a.fire_time > b.fire_time;
    }
  };
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

  void OnRenderTexture(void* native_texture_ptr, size_t width, size_t height,
                       float dpi);

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

  void CreateRenderingContext(size_t width, size_t height);

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

  //renderer apis
  bool ClearCurrentContext();

  bool MakeCurrentContext();

  bool MakeCurrentResourceContext();
  
  uint32_t GetFbo();

  //task runner
  void PostTask(UIWidgetsTask uiwidgets_task,
                               uint64_t uiwidgets_target_time_nanos);

  static TaskTimePoint TimePointFromUIWidgetsTime(
    uint64_t uiwidgets_target_time_nanos);

  std::chrono::nanoseconds ProcessTasks();

  void AddTaskObserver(intptr_t key, const fml::closure& callback);

  void RemoveTaskObserver(intptr_t key);

  Mono_Handle handle_;
  EntrypointCallback entrypoint_callback_;

  //pixel buffer handles
  CVPixelBufferRef pixelbuffer_ref;

  //openGL handlers
  GLuint default_fbo_ = 0;
  GLuint gl_tex_ = 0;
  NSOpenGLContext *gl_context_ = NULL;
  NSOpenGLContext *gl_resource_context_ = NULL;
  CVOpenGLTextureCacheRef gl_tex_cache_ref_;
  CVOpenGLTextureRef gl_tex_ref_;
  CGLPixelFormatObj gl_pixelformat_;

  //metal handlers
  id<MTLDevice> metal_device_;
  id<MTLTexture> metal_tex_;
  CVMetalTextureRef metal_tex_ref_;
  CVMetalTextureCacheRef metal_tex_cache_ref_;

  //task runner
  using TaskObservers = std::map<intptr_t, fml::closure>;
  TaskObservers task_observers_;
  std::mutex task_queue_mutex_;
  std::priority_queue<Task, std::deque<Task>, Task::Comparer> task_queue_;

  std::unique_ptr<GfxWorkerTaskRunner> gfx_worker_task_runner_;
  UIWidgetsEngine engine_ = nullptr;

  std::vector<intptr_t> vsync_batons_;

  MouseState mouse_state_;
  bool process_events_ = false;
};

}  // namespace uiwidgets
