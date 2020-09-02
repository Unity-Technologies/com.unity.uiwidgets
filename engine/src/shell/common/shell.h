#pragma once

#include <functional>
#include <string_view>
#include <unordered_map>

#include "common/settings.h"
#include "common/task_runners.h"
#include "flow/texture.h"
#include "flutter/fml/closure.h"
#include "flutter/fml/macros.h"
#include "flutter/fml/memory/ref_ptr.h"
#include "flutter/fml/memory/thread_checker.h"
#include "flutter/fml/memory/weak_ptr.h"
#include "flutter/fml/status.h"
#include "flutter/fml/synchronization/sync_switch.h"
#include "flutter/fml/synchronization/waitable_event.h"
#include "flutter/fml/thread.h"
#include "lib/ui/window/platform_message.h"
#include "shell/common/animator.h"
#include "shell/common/engine.h"
#include "shell/common/platform_view.h"
#include "shell/common/rasterizer.h"
#include "shell/common/shell_io_manager.h"
#include "shell/common/surface.h"

namespace uiwidgets {

class Shell final : public PlatformView::Delegate,
                    public Animator::Delegate,
                    public Engine::Delegate,
                    public Rasterizer::Delegate {
 public:
  template <class T>
  using CreateCallback = std::function<std::unique_ptr<T>(Shell&)>;

  static std::unique_ptr<Shell> Create(
      TaskRunners task_runners, Settings settings,
      const CreateCallback<PlatformView>& on_create_platform_view,
      const CreateCallback<Rasterizer>& on_create_rasterizer);

  static std::unique_ptr<Shell> Create(
      TaskRunners task_runners, const WindowData window_data, Settings settings,
      const CreateCallback<PlatformView>& on_create_platform_view,
      const CreateCallback<Rasterizer>& on_create_rasterizer);

  ~Shell();

  void RunEngine(RunConfiguration run_configuration);

  void RunEngine(RunConfiguration run_configuration,
                 const std::function<void(Engine::RunStatus)>& result_callback);

  const Settings& GetSettings() const;

  const TaskRunners& GetTaskRunners() const;

  fml::WeakPtr<Rasterizer> GetRasterizer() const;

  fml::WeakPtr<Engine> GetEngine();

  fml::WeakPtr<PlatformView> GetPlatformView();

  void NotifyLowMemoryWarning() const;

  bool IsSetup() const;

  Rasterizer::Screenshot Screenshot(Rasterizer::ScreenshotType type,
                                    bool base64_encode);

  fml::Status WaitForFirstFrame(fml::TimeDelta timeout);

  bool ReloadSystemFonts();

  std::shared_ptr<fml::SyncSwitch> GetIsGpuDisabledSyncSwitch() const;

 private:
  const TaskRunners task_runners_;
  const Settings settings_;
  std::unique_ptr<PlatformView> platform_view_;  // on platform task runner
  std::unique_ptr<Engine> engine_;               // on UI task runner
  std::unique_ptr<Rasterizer> rasterizer_;       // on GPU task runner
  std::unique_ptr<ShellIOManager> io_manager_;   // on IO task runner
  std::shared_ptr<fml::SyncSwitch> is_gpu_disabled_sync_switch_;

  fml::WeakPtr<Engine> weak_engine_;          // to be shared across threads
  fml::WeakPtr<Rasterizer> weak_rasterizer_;  // to be shared across threads
  fml::WeakPtr<PlatformView>
      weak_platform_view_;  // to be shared across threads

  bool is_setup_ = false;
  uint64_t next_pointer_flow_id_ = 0;

  bool first_frame_rasterized_ = false;
  std::atomic<bool> waiting_for_first_frame_ = true;
  std::mutex waiting_for_first_frame_mutex_;
  std::condition_variable waiting_for_first_frame_condition_;

  // Written in the UI thread and read from the raster thread. Hence make it
  // atomic.
  std::atomic<bool> needs_report_timings_{false};

  // Whether there's a task scheduled to report the timings to Dart through
  // ui.Window.onReportTimings.
  bool frame_timings_report_scheduled_ = false;

  // Vector of FrameTiming::kCount * n timestamps for n frames whose timings
  // have not been reported yet. Vector of ints instead of FrameTiming is stored
  // here for easier conversions to Dart objects.
  std::vector<int64_t> unreported_timings_;

  // A cache of `Engine::GetDisplayRefreshRate` (only callable in the UI thread)
  // so we can access it from `Rasterizer` (in the raster thread).
  //
  // The atomic is for extra thread safety as this is written in the UI thread
  // and read from the raster thread.
  std::atomic<float> display_refresh_rate_ = 0.0f;

  // How many frames have been timed since last report.
  size_t UnreportedFramesCount() const;

  Shell(TaskRunners task_runners, Settings settings);

  static std::unique_ptr<Shell> CreateShellOnPlatformThread(
      TaskRunners task_runners, const WindowData window_data, Settings settings,
      const Shell::CreateCallback<PlatformView>& on_create_platform_view,
      const Shell::CreateCallback<Rasterizer>& on_create_rasterizer);

  bool Setup(std::unique_ptr<PlatformView> platform_view,
             std::unique_ptr<Engine> engine,
             std::unique_ptr<Rasterizer> rasterizer,
             std::unique_ptr<ShellIOManager> io_manager);

  void ReportTimings();

  // |PlatformView::Delegate|
  void OnPlatformViewCreated(std::unique_ptr<Surface> surface) override;

  // |PlatformView::Delegate|
  void OnPlatformViewDestroyed() override;

  // |PlatformView::Delegate|
  void OnPlatformViewSetViewportMetrics(
      const ViewportMetrics& metrics) override;

  // |PlatformView::Delegate|
  void OnPlatformViewDispatchPlatformMessage(
      fml::RefPtr<PlatformMessage> message) override;

  // |PlatformView::Delegate|
  void OnPlatformViewDispatchPointerDataPacket(
      std::unique_ptr<PointerDataPacket> packet) override;

  // |shell:PlatformView::Delegate|
  void OnPlatformViewSetAccessibilityFeatures(int32_t flags) override;

  // |PlatformView::Delegate|
  void OnPlatformViewRegisterTexture(std::shared_ptr<Texture> texture) override;

  // |PlatformView::Delegate|
  void OnPlatformViewUnregisterTexture(int64_t texture_id) override;

  // |PlatformView::Delegate|
  void OnPlatformViewMarkTextureFrameAvailable(int64_t texture_id) override;

  // |PlatformView::Delegate|
  void OnPlatformViewSetNextFrameCallback(const fml::closure& closure) override;

  // |Animator::Delegate|
  void OnAnimatorBeginFrame(fml::TimePoint frame_time) override;

  // |Animator::Delegate|
  void OnAnimatorNotifyIdle(int64_t deadline) override;

  // |Animator::Delegate|
  void OnAnimatorDraw(fml::RefPtr<Pipeline<LayerTree>> pipeline) override;

  // |Animator::Delegate|
  void OnAnimatorDrawLastLayerTree() override;

  // |Engine::Delegate|
  void OnEngineHandlePlatformMessage(
      fml::RefPtr<PlatformMessage> message) override;

  void HandleEngineSkiaMessage(fml::RefPtr<PlatformMessage> message);

  // |Engine::Delegate|
  void OnPreEngineRestart() override;

  // |Engine::Delegate|
  void SetNeedsReportTimings(bool value) override;

  // |Rasterizer::Delegate|
  void OnFrameRasterized(const FrameTiming&) override;

  // |Rasterizer::Delegate|
  fml::Milliseconds GetFrameBudget() override;

  fml::WeakPtrFactory<Shell> weak_factory_;

  // For accessing the Shell via the raster thread, necessary for various
  // rasterizer callbacks.
  std::unique_ptr<fml::WeakPtrFactory<Shell>> weak_factory_gpu_;

  FML_DISALLOW_COPY_AND_ASSIGN(Shell);
};

}  // namespace uiwidgets
