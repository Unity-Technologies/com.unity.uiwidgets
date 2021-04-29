#pragma once

#include <memory>
#include <vector>

#include "common/task_runners.h"
#include "flow/layers/layer_tree.h"
#include "flutter/fml/macros.h"
#include "lib/ui/io_manager.h"

#include "lib/ui/ui_mono_state.h"
#include "lib/ui/window/pointer_data_packet.h"
#include "lib/ui/window/window.h"
#include "mono_isolate.h"
#include "rapidjson/document.h"
#include "rapidjson/stringbuffer.h"
#include "runtime/window_data.h"

namespace uiwidgets {

class Scene;
class RuntimeDelegate;
class View;
class Window;

class RuntimeController final : public WindowClient {
 public:
  RuntimeController(RuntimeDelegate& client, const Settings& settings,
                    TaskRunners task_runners,
                    fml::WeakPtr<SnapshotDelegate> snapshot_delegate,
                    fml::WeakPtr<IOManager> io_manager,
                    fml::RefPtr<SkiaUnrefQueue> unref_queue,
                    fml::WeakPtr<ImageDecoder> image_decoder,
                    const WindowData& window_data);

  ~RuntimeController() override;

  std::unique_ptr<RuntimeController> Clone() const;

  bool SetViewportMetrics(const ViewportMetrics& metrics);

  bool SetLocales(const std::vector<std::string>& locale_data);

  bool SetUserSettingsData(const std::string& data);

  bool SetLifecycleState(const std::string& data);

  bool SetAccessibilityFeatures(int32_t flags);

  bool BeginFrame(fml::TimePoint frame_time);

  bool ReportTimings(std::vector<int64_t> timings);

  bool NotifyIdle(int64_t deadline);

  bool DispatchPlatformMessage(fml::RefPtr<PlatformMessage> message);

  bool DispatchPointerDataPacket(const PointerDataPacket& packet);

  std::weak_ptr<MonoIsolate> GetRootIsolate();

 private:
  struct Locale {
    Locale(std::string language_code_, std::string country_code_,
           std::string script_code_, std::string variant_code_);

    ~Locale();

    std::string language_code;
    std::string country_code;
    std::string script_code;
    std::string variant_code;
  };

  RuntimeDelegate& client_;
  const Settings& settings_;
  TaskRunners task_runners_;
  fml::WeakPtr<SnapshotDelegate> snapshot_delegate_;
  fml::WeakPtr<IOManager> io_manager_;
  fml::RefPtr<SkiaUnrefQueue> unref_queue_;
  fml::WeakPtr<ImageDecoder> image_decoder_;
  WindowData window_data_;
  std::weak_ptr<MonoIsolate> root_isolate_;

  Window* GetWindowIfAvailable();

  bool FlushRuntimeStateToIsolate();

  // |WindowClient|
  std::string DefaultRouteName() override;

  // |WindowClient|
  void ScheduleFrame() override;

  // |WindowClient|
  void Render(Scene* scene) override;

  // |WindowClient|
  void HandlePlatformMessage(fml::RefPtr<PlatformMessage> message) override;

  // |WindowClient|
  FontCollection& GetFontCollection() override;

  // |WindowClient|
  void SetNeedsReportTimings(bool value) override;

  FML_DISALLOW_COPY_AND_ASSIGN(RuntimeController);
};

}  // namespace uiwidgets
