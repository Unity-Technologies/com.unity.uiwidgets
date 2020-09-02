#include "window.h"

#include "lib/ui/compositing/scene.h"
#include "lib/ui/ui_mono_state.h"

namespace uiwidgets {
namespace {

typedef Mono_Handle (*Window_constructorCallback)(Window* window);

Window_constructorCallback Window_constructor_;

typedef void (*Window_disposeCallback)(Mono_Handle handle);

Window_disposeCallback Window_dispose_;

typedef void (*Window_updateWindowMetricsCallback)(
    float devicePixelRatio, float width, float height, float depth,
    float viewPaddingTop, float viewPaddingRight, float viewPaddingBottom,
    float viewPaddingLeft, float viewInsetTop, float viewInsetRight,
    float viewInsetBottom, float viewInsetLeft, float systemGestureInsetTop,
    float systemGestureInsetRight, float systemGestureInsetBottom,
    float systemGestureInsetLeft);

Window_updateWindowMetricsCallback Window_updateWindowMetrics_;

typedef void (*Window_beginFrameCallback)(long microseconds);
Window_beginFrameCallback Window_beginFrame_;

typedef void (*Window_drawFrameCallback)();
Window_drawFrameCallback Window_drawFrame_;

UIWIDGETS_API(void)
Window_hook(Window_constructorCallback Window_constructor,
            Window_disposeCallback Window_dispose,
            Window_updateWindowMetricsCallback Window_updateWindowMetrics,
            Window_beginFrameCallback Window_beginFrame,
            Window_drawFrameCallback Window_drawFrame) {
  Window_constructor_ = Window_constructor;
  Window_dispose_ = Window_dispose;
  Window_updateWindowMetrics_ = Window_updateWindowMetrics;
  Window_beginFrame_ = Window_beginFrame;
  Window_drawFrame_ = Window_drawFrame;
}

UIWIDGETS_API(Mono_Handle) Window_instance() {
  return UIMonoState::Current()->window()->mono_window();
}

UIWIDGETS_API(void) Window_setNeedsReportTimings(Window* ptr, bool value) {
  ptr->client()->SetNeedsReportTimings(value);
}

UIWIDGETS_API(char*) Window_defaultRouteName(Window* ptr) {
  const std::string routeName = ptr->client()->DefaultRouteName();
  size_t size = routeName.length() + 1;
  char* result = static_cast<char*>(malloc(size));
  routeName.copy(result, size);
  return result;
}

UIWIDGETS_API(void) Window_freeDefaultRouteName(char* routeName) {
  free(routeName);
}

UIWIDGETS_API(void) Window_scheduleFrame(Window* ptr) {
  ptr->client()->ScheduleFrame();
}

UIWIDGETS_API(void) Window_render(Window* ptr, Scene* scene) {
  ptr->client()->Render(scene);
}

}  // namespace

WindowClient::~WindowClient() {}

Window::Window(WindowClient* client)
    : client_(client), mono_window_(Window_constructor_(this)) {}

Window::~Window() { Window_dispose_(mono_window_); }

void Window::DidCreateIsolate() {
  mono_state_ = MonoState::Current()->GetWeakPtr();
}

void Window::UpdateWindowMetrics(const ViewportMetrics& metrics) {
  viewport_metrics_ = metrics;

  std::shared_ptr<MonoState> mono_state = mono_state_.lock();
  if (!mono_state) return;
  MonoState::Scope scope(mono_state);
  Window_updateWindowMetrics_(
      metrics.device_pixel_ratio, metrics.physical_width,
      metrics.physical_height, metrics.physical_depth,
      metrics.physical_padding_top, metrics.physical_padding_right,
      metrics.physical_padding_bottom, metrics.physical_padding_left,
      metrics.physical_view_inset_top, metrics.physical_view_inset_right,
      metrics.physical_view_inset_bottom, metrics.physical_view_inset_left,
      metrics.physical_system_gesture_inset_top,
      metrics.physical_system_gesture_inset_right,
      metrics.physical_system_gesture_inset_bottom,
      metrics.physical_system_gesture_inset_left);
}

void Window::UpdateLocales(const std::vector<std::string>& locales) {}

void Window::UpdateUserSettingsData(const std::string& data) {}

void Window::UpdateLifecycleState(const std::string& data) {}

void Window::UpdateAccessibilityFeatures(int32_t values) {}

void Window::DispatchPlatformMessage(fml::RefPtr<PlatformMessage> message) {}

void Window::DispatchPointerDataPacket(const PointerDataPacket& packet) {}

void Window::BeginFrame(fml::TimePoint frameTime) {
  std::shared_ptr<MonoState> mono_state = mono_state_.lock();
  if (!mono_state) return;
  MonoState::Scope scope(mono_state);

  int64_t microseconds = (frameTime - fml::TimePoint()).ToMicroseconds();
  Window_beginFrame_(microseconds);

  UIMonoState::Current()->FlushMicrotasksNow();

  Window_drawFrame_();
}

void Window::ReportTimings(std::vector<int64_t> timings) {}

void Window::CompletePlatformMessageEmptyResponse(int response_id) {
  if (!response_id) return;
  auto it = pending_responses_.find(response_id);
  if (it == pending_responses_.end()) return;
  auto response = std::move(it->second);
  pending_responses_.erase(it);
  response->CompleteEmpty();
}

void Window::CompletePlatformMessageResponse(int response_id,
                                             std::vector<uint8_t> data) {
  if (!response_id) return;
  auto it = pending_responses_.find(response_id);
  if (it == pending_responses_.end()) return;
  auto response = std::move(it->second);
  pending_responses_.erase(it);
  response->Complete(std::make_unique<fml::DataMapping>(std::move(data)));
}

}  // namespace uiwidgets
