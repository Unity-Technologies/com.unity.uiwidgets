#pragma once

#include <memory>
#include <string>

#include "assets/asset_manager.h"
#include "common/task_runners.h"
#include "flutter/fml/macros.h"
#include "flutter/fml/memory/weak_ptr.h"
#include "lib/ui/painting/image_decoder.h"
#include "lib/ui/snapshot_delegate.h"
#include "lib/ui/text/font_collection.h"
#include <flutter/fml/concurrent_message_loop.h>

#include "include/core/SkPicture.h"
#include "lib/ui/window/platform_message.h"
#include "lib/ui/window/viewport_metrics.h"
#include "runtime/runtime_controller.h"
#include "runtime/runtime_delegate.h"
#include "shell/common/animator.h"
#include "shell/common/platform_view.h"
#include "shell/common/pointer_data_dispatcher.h"
#include "shell/common/rasterizer.h"
#include "shell/common/run_configuration.h"
#include "shell/common/shell_io_manager.h"

namespace uiwidgets {

class Engine final : public RuntimeDelegate, PointerDataDispatcher::Delegate {
 public:
  enum class RunStatus {
    Success,
    FailureAlreadyRunning,
    Failure,
  };

  class Delegate {
   public:
    virtual void OnEngineHandlePlatformMessage(
        fml::RefPtr<PlatformMessage> message) = 0;
    virtual void OnPreEngineRestart() = 0;
    virtual void SetNeedsReportTimings(bool needs_reporting) = 0;
  };

  Engine(Delegate& delegate, const PointerDataDispatcherMaker& dispatcher_maker,
         TaskRunners task_runners, const WindowData window_data,
         Settings settings, std::unique_ptr<Animator> animator,
         fml::WeakPtr<IOManager> io_manager,
         fml::RefPtr<SkiaUnrefQueue> unref_queue,
         fml::WeakPtr<SnapshotDelegate> snapshot_delegate);

  ~Engine() override;

  float GetDisplayRefreshRate() const;

  fml::WeakPtr<Engine> GetWeakPtr() const;

  [[nodiscard]] RunStatus Run(RunConfiguration configuration);

  [[nodiscard]] bool Restart(RunConfiguration configuration);

  void SetupDefaultFontManager();

  bool UpdateAssetManager(std::shared_ptr<AssetManager> asset_manager);

  void BeginFrame(fml::TimePoint frame_time);

  void NotifyIdle(int64_t deadline);

  void ReportTimings(std::vector<int64_t> timings);

  void OnOutputSurfaceCreated();

  void OnOutputSurfaceDestroyed();

  void SetViewportMetrics(const ViewportMetrics& metrics);

  void DispatchPlatformMessage(fml::RefPtr<PlatformMessage> message);

  void DispatchPointerDataPacket(std::unique_ptr<PointerDataPacket> packet,
                                 uint64_t trace_flow_id);
  void SetAccessibilityFeatures(int32_t flags);

  void ScheduleFrame(bool regenerate_layer_tree = true) override;

  // |RuntimeDelegate|
  FontCollection& GetFontCollection() override;

  // |PointerDataDispatcher::Delegate|
  void DoDispatchPacket(std::unique_ptr<PointerDataPacket> packet,
                        uint64_t trace_flow_id) override;

  // |PointerDataDispatcher::Delegate|
  void ScheduleSecondaryVsyncCallback(const fml::closure& callback) override;

	std::shared_ptr<fml::ConcurrentMessageLoop> GetConcurrentMessageLoop();

 private:
  Delegate& delegate_;
  const Settings settings_;
  std::shared_ptr<fml::ConcurrentMessageLoop> concurrent_message_loop_;
  std::unique_ptr<Animator> animator_;
  std::unique_ptr<RuntimeController> runtime_controller_;

  // The pointer_data_dispatcher_ depends on animator_ and runtime_controller_.
  // So it should be defined after them to ensure that pointer_data_dispatcher_
  // is destructed first.
  std::unique_ptr<PointerDataDispatcher> pointer_data_dispatcher_;

  std::string initial_route_;
  ViewportMetrics viewport_metrics_;
  std::shared_ptr<AssetManager> asset_manager_;
  bool activity_running_;
  bool have_surface_;
  FontCollection font_collection_;
  ImageDecoder image_decoder_;
  TaskRunners task_runners_;
  fml::WeakPtrFactory<Engine> weak_factory_;

  // |RuntimeDelegate|
  std::string DefaultRouteName() override;

  // |RuntimeDelegate|
  void Render(std::unique_ptr<LayerTree> layer_tree) override;

  // |RuntimeDelegate|
  void HandlePlatformMessage(fml::RefPtr<PlatformMessage> message) override;

  void SetNeedsReportTimings(bool value) override;

  void StopAnimator();

  void StartAnimatorIfPossible();

  bool HandleLifecyclePlatformMessage(PlatformMessage* message);

  bool HandleNavigationPlatformMessage(fml::RefPtr<PlatformMessage> message);

  bool HandleLocalizationPlatformMessage(PlatformMessage* message);

  void HandleSettingsPlatformMessage(PlatformMessage* message);

  void HandleAssetPlatformMessage(fml::RefPtr<PlatformMessage> message);

  bool GetAssetAsBuffer(const std::string& name, std::vector<uint8_t>* data);

  RunStatus PrepareAndLaunchIsolate(RunConfiguration configuration);

  FML_DISALLOW_COPY_AND_ASSIGN(Engine);
};

}  // namespace uiwidgets
