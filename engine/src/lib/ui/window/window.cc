#include "window.h"

#include "lib/ui/compositing/scene.h"
#include "lib/ui/ui_mono_state.h"
#include "platform_message_response_mono.h"

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

typedef void (*Window_beginFrameCallback)(int64_t microseconds);
Window_beginFrameCallback Window_beginFrame_;

typedef void (*Window_drawFrameCallback)();
Window_drawFrameCallback Window_drawFrame_;

typedef void (*Window_dispatchPlatformMessageCallback)(const char* name,
                                                       const uint8_t* data,
                                                       int data_length,
                                                       int response_id);
Window_dispatchPlatformMessageCallback Window_dispatchPlatformMessage_;

typedef void (*Window_dispatchPointerDataPacketCallback)(const uint8_t* data,
                                                         int data_length);
Window_dispatchPointerDataPacketCallback Window_dispatchPointerDataPacket_;

UIWIDGETS_API(void)
Window_hook(
    Window_constructorCallback Window_constructor,
    Window_disposeCallback Window_dispose,
    Window_updateWindowMetricsCallback Window_updateWindowMetrics,
    Window_beginFrameCallback Window_beginFrame,
    Window_drawFrameCallback Window_drawFrame,
    Window_dispatchPlatformMessageCallback Window_dispatchPlatformMessage,
    Window_dispatchPointerDataPacketCallback Window_dispatchPointerDataPacket) {
  Window_constructor_ = Window_constructor;
  Window_dispose_ = Window_dispose;
  Window_updateWindowMetrics_ = Window_updateWindowMetrics;
  Window_beginFrame_ = Window_beginFrame;
  Window_drawFrame_ = Window_drawFrame;
  Window_dispatchPlatformMessage_ = Window_dispatchPlatformMessage;
  Window_dispatchPointerDataPacket_ = Window_dispatchPointerDataPacket;
}

UIWIDGETS_API(Mono_Handle) Window_instance() {
  if (!UIMonoState::EnsureCurrentIsolate()) {
    return nullptr;
  }
  return UIMonoState::Current()->window()->mono_window();
}

UIWIDGETS_API(void) Window_setNeedsReportTimings(Window* ptr, bool value) {
  ptr->client()->SetNeedsReportTimings(value);
}

UIWIDGETS_API(char*) Window_defaultRouteName(Window* ptr) {
  const std::string routeName = ptr->client()->DefaultRouteName();
  size_t size = routeName.length() + 1;
  char* result = static_cast<char*>(malloc(size));
  strcpy(result, routeName.c_str());
  return result;
}

UIWIDGETS_API(void) Window_freeDefaultRouteName(char* routeName) {
  free(routeName);
}

UIWIDGETS_API(void) Window_scheduleFrame(Window* ptr) {
  ptr->client()->ScheduleFrame();
}

typedef void (*Window_sendPlatformMessageCallback)(Mono_Handle callback_handle,
                                                   const uint8_t* data,
                                                   int data_length);

UIWIDGETS_API(const char*)
Window_sendPlatformMessage(char* name,
                           Window_sendPlatformMessageCallback callback,
                           Mono_Handle callback_handle, const uint8_t* data,
                           int data_length) {
  UIMonoState* dart_state = UIMonoState::Current();

  if (!dart_state->window()) {
    return "Platform messages can only be sent from the main isolate";
  }

  fml::RefPtr<PlatformMessageResponse> response;
  if (callback != nullptr) {
    response = fml::MakeRefCounted<PlatformMessageResponseMono>(
        dart_state->GetWeakPtr(), callback, callback_handle,
        dart_state->GetTaskRunners().GetUITaskRunner());
  }

  if (data == nullptr) {
    dart_state->window()->client()->HandlePlatformMessage(
        fml::MakeRefCounted<PlatformMessage>(name, response));
  } else {
    dart_state->window()->client()->HandlePlatformMessage(
        fml::MakeRefCounted<PlatformMessage>(
            name, std::vector<uint8_t>(data, data + data_length), response));
  }

  return nullptr;
}

UIWIDGETS_API(void)
Window_respondToPlatformMessage(Window* ptr, int response_id,
                                const uint8_t* data, int data_length) {
  if (data == nullptr || data_length == 0) {
    ptr->CompletePlatformMessageEmptyResponse(response_id);
  } else {
    ptr->CompletePlatformMessageResponse(
        response_id, std::vector<uint8_t>(data, data + data_length));
  }
}

UIWIDGETS_API(void) Window_render(Window* ptr, Scene* scene) {
  ptr->client()->Render(scene);
}

// TODO: ComputePlatformResolvedLocale
UIWIDGETS_API(void) Window_computePlatformResolvedLocale(Window* ptr, Scene* scene) {
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

void Window::DispatchPlatformMessage(fml::RefPtr<PlatformMessage> message) {
  std::shared_ptr<MonoState> mono_state = mono_state_.lock();
  if (!mono_state) {
    FML_DLOG(WARNING)
        << "Dropping platform message for lack of MonoState on channel: "
        << message->channel();
    return;
  }

  MonoState::Scope scope(mono_state);

  int response_id = 0;
  if (auto response = message->response()) {
    response_id = next_response_id_++;
    pending_responses_[response_id] = response;
  }

  const uint8_t* data = nullptr;
  int data_length = 0;
  if (message->hasData()) {
    data = message->data().data();
    data_length = static_cast<int>(message->data().size());
  }

  Window_dispatchPlatformMessage_(message->channel().c_str(), data, data_length,
                                  response_id);
}

void Window::DispatchPointerDataPacket(const PointerDataPacket& packet) {
  std::shared_ptr<MonoState> mono_state = mono_state_.lock();
  if (!mono_state) return;
  MonoState::Scope scope(mono_state);

  const auto& buffer = packet.data();
  Window_dispatchPointerDataPacket_(buffer.data(), buffer.size());
}

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
