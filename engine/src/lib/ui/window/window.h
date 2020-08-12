#pragma once

#include <string>
#include <unordered_map>
#include <vector>

#include "flutter/fml/time/time_point.h"
#include "include/gpu/GrContext.h"
#include "lib/ui/window/platform_message.h"
#include "lib/ui/window/pointer_data_packet.h"
#include "lib/ui/window/viewport_metrics.h"
#include "runtime/mono_api.h"
#include "runtime/mono_state.h"

namespace uiwidgets {
class FontCollection;
class Scene;

enum class AccessibilityFeatureFlag : int32_t {
  kAccessibleNavigation = 1 << 0,
  kInvertColors = 1 << 1,
  kDisableAnimations = 1 << 2,
  kBoldText = 1 << 3,
  kReduceMotion = 1 << 4,
  kHighContrast = 1 << 5,
};

class WindowClient {
 public:
  virtual std::string DefaultRouteName() = 0;
  virtual void ScheduleFrame() = 0;
  virtual void Render(Scene* scene) = 0;
  virtual void HandlePlatformMessage(fml::RefPtr<PlatformMessage> message) = 0;
  virtual FontCollection& GetFontCollection() = 0;
  virtual void SetNeedsReportTimings(bool value) = 0;

 protected:
  virtual ~WindowClient();
};

class Window final {
 public:
  explicit Window(WindowClient* client);

  ~Window();

  WindowClient* client() const { return client_; }

  const ViewportMetrics& viewport_metrics() { return viewport_metrics_; }

	Mono_Handle mono_window() const { return mono_window_; }

	void DidCreateIsolate();
  void UpdateWindowMetrics(const ViewportMetrics& metrics);
  void UpdateLocales(const std::vector<std::string>& locales);
  void UpdateUserSettingsData(const std::string& data);
  void UpdateLifecycleState(const std::string& data);
  void UpdateAccessibilityFeatures(int32_t flags);
  void DispatchPlatformMessage(fml::RefPtr<PlatformMessage> message);
  void DispatchPointerDataPacket(const PointerDataPacket& packet);
  void BeginFrame(fml::TimePoint frameTime);
  void ReportTimings(std::vector<int64_t> timings);

  void CompletePlatformMessageResponse(int response_id,
                                       std::vector<uint8_t> data);
  void CompletePlatformMessageEmptyResponse(int response_id);

 private:
  WindowClient* client_;
  ViewportMetrics viewport_metrics_;
  Mono_Handle mono_window_;
  std::weak_ptr<MonoState> mono_state_;

  // We use id 0 to mean that no response is expected.
  int next_response_id_ = 1;
  std::unordered_map<int, fml::RefPtr<PlatformMessageResponse>>
      pending_responses_;
};

}  // namespace uiwidgets
