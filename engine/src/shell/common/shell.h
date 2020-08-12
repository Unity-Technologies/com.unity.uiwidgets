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
                    public Rasterizer::Delegate,
                    public ServiceProtocol::Handler {
 public:
  template <class T>
  using CreateCallback = std::function<std::unique_ptr<T>(Shell&)>;

  //----------------------------------------------------------------------------
  /// @brief      Creates a shell instance using the provided settings. The
  ///             callbacks to create the various shell subcomponents will be
  ///             called on the appropriate threads before this method returns.
  ///             If this is the first instance of a shell in the process, this
  ///             call also bootstraps the Dart VM.
  ///
  /// @param[in]  task_runners             The task runners
  /// @param[in]  settings                 The settings
  /// @param[in]  on_create_platform_view  The callback that must return a
  ///                                      platform view. This will be called on
  ///                                      the platform task runner before this
  ///                                      method returns.
  /// @param[in]  on_create_rasterizer     That callback that must provide a
  ///                                      valid rasterizer. This will be called
  ///                                      on the render task runner before this
  ///                                      method returns.
  ///
  /// @return     A full initialized shell if the settings and callbacks are
  ///             valid. The root isolate has been created but not yet launched.
  ///             It may be launched by obtaining the engine weak pointer and
  ///             posting a task onto the UI task runner with a valid run
  ///             configuration to run the isolate. The embedder must always
  ///             check the validity of the shell (using the IsSetup call)
  ///             immediately after getting a pointer to it.
  ///
  static std::unique_ptr<Shell> Create(
      TaskRunners task_runners,
      Settings settings,
      const CreateCallback<PlatformView>& on_create_platform_view,
      const CreateCallback<Rasterizer>& on_create_rasterizer);

  //----------------------------------------------------------------------------
  /// @brief      Creates a shell instance using the provided settings. The
  ///             callbacks to create the various shell subcomponents will be
  ///             called on the appropriate threads before this method returns.
  ///             Unlike the simpler variant of this factory method, this method
  ///             allows for specification of window data. If this is the first
  ///             instance of a shell in the process, this call also bootstraps
  ///             the Dart VM.
  ///
  /// @param[in]  task_runners             The task runners
  /// @param[in]  window_data              The default data for setting up
  ///                                      ui.Window that attached to this
  ///                                      intance.
  /// @param[in]  settings                 The settings
  /// @param[in]  on_create_platform_view  The callback that must return a
  ///                                      platform view. This will be called on
  ///                                      the platform task runner before this
  ///                                      method returns.
  /// @param[in]  on_create_rasterizer     That callback that must provide a
  ///                                      valid rasterizer. This will be called
  ///                                      on the render task runner before this
  ///                                      method returns.
  ///
  /// @return     A full initialized shell if the settings and callbacks are
  ///             valid. The root isolate has been created but not yet launched.
  ///             It may be launched by obtaining the engine weak pointer and
  ///             posting a task onto the UI task runner with a valid run
  ///             configuration to run the isolate. The embedder must always
  ///             check the validity of the shell (using the IsSetup call)
  ///             immediately after getting a pointer to it.
  ///
  static std::unique_ptr<Shell> Create(
      TaskRunners task_runners,
      const WindowData window_data,
      Settings settings,
      CreateCallback<PlatformView> on_create_platform_view,
      CreateCallback<Rasterizer> on_create_rasterizer);

  //----------------------------------------------------------------------------
  /// @brief      Creates a shell instance using the provided settings. The
  ///             callbacks to create the various shell subcomponents will be
  ///             called on the appropriate threads before this method returns.
  ///             Unlike the simpler variant of this factory method, this method
  ///             allows for the specification of an isolate snapshot that
  ///             cannot be adequately described in the settings. This call also
  ///             requires the specification of a running VM instance.
  ///
  /// @param[in]  task_runners             The task runners
  /// @param[in]  window_data              The default data for setting up
  ///                                      ui.Window that attached to this
  ///                                      intance.
  /// @param[in]  settings                 The settings
  /// @param[in]  isolate_snapshot         A custom isolate snapshot. Takes
  ///                                      precedence over any snapshots
  ///                                      specified in the settings.
  /// @param[in]  on_create_platform_view  The callback that must return a
  ///                                      platform view. This will be called on
  ///                                      the platform task runner before this
  ///                                      method returns.
  /// @param[in]  on_create_rasterizer     That callback that must provide a
  ///                                      valid rasterizer. This will be called
  ///                                      on the render task runner before this
  ///                                      method returns.
  /// @param[in]  vm                       A running VM instance.
  ///
  /// @return     A full initialized shell if the settings and callbacks are
  ///             valid. The root isolate has been created but not yet launched.
  ///             It may be launched by obtaining the engine weak pointer and
  ///             posting a task onto the UI task runner with a valid run
  ///             configuration to run the isolate. The embedder must always
  ///             check the validity of the shell (using the IsSetup call)
  ///             immediately after getting a pointer to it.
  ///
  static std::unique_ptr<Shell> Create(
      TaskRunners task_runners,
      const WindowData window_data,
      Settings settings,
      fml::RefPtr<const DartSnapshot> isolate_snapshot,
      const CreateCallback<PlatformView>& on_create_platform_view,
      const CreateCallback<Rasterizer>& on_create_rasterizer,
      DartVMRef vm);

  //----------------------------------------------------------------------------
  /// @brief      Destroys the shell. This is a synchronous operation and
  ///             synchronous barrier blocks are introduced on the various
  ///             threads to ensure shutdown of all shell sub-components before
  ///             this method returns.
  ///
  ~Shell();

  //----------------------------------------------------------------------------
  /// @brief      Starts an isolate for the given RunConfiguration.
  ///
  void RunEngine(RunConfiguration run_configuration);

  //----------------------------------------------------------------------------
  /// @brief      Starts an isolate for the given RunConfiguration. The
  ///             result_callback will be called with the status of the
  ///             operation.
  ///
  void RunEngine(RunConfiguration run_configuration,
                 const std::function<void(Engine::RunStatus)>& result_callback);

  //------------------------------------------------------------------------------
  /// @return     The settings used to launch this shell.
  ///
  const Settings& GetSettings() const;

  //------------------------------------------------------------------------------
  /// @brief      If callers wish to interact directly with any shell
  ///             subcomponents, they must (on the platform thread) obtain a
  ///             task runner that the component is designed to run on and a
  ///             weak pointer to that component. They may then post a task to
  ///             that task runner, do the validity check on that task runner
  ///             before performing any operation on that component. This
  ///             accessor allows callers to access the task runners for this
  ///             shell.
  ///
  /// @return     The task runners current in use by the shell.
  ///
  const TaskRunners& GetTaskRunners() const;

  //----------------------------------------------------------------------------
  /// @brief      Rasterizers may only be accessed on the GPU task runner.
  ///
  /// @return     A weak pointer to the rasterizer.
  ///
  fml::WeakPtr<Rasterizer> GetRasterizer() const;

  //------------------------------------------------------------------------------
  /// @brief      Engines may only be accessed on the UI thread. This method is
  ///             deprecated, and implementers should instead use other API
  ///             available on the Shell or the PlatformView.
  ///
  /// @return     A weak pointer to the engine.
  ///
  fml::WeakPtr<Engine> GetEngine();

  //----------------------------------------------------------------------------
  /// @brief      Platform views may only be accessed on the platform task
  ///             runner.
  ///
  /// @return     A weak pointer to the platform view.
  ///
  fml::WeakPtr<PlatformView> GetPlatformView();

  // Embedders should call this under low memory conditions to free up
  // internal caches used.
  //
  // This method posts a task to the raster threads to signal the Rasterizer to
  // free resources.

  //----------------------------------------------------------------------------
  /// @brief      Used by embedders to notify that there is a low memory
  ///             warning. The shell will attempt to purge caches. Current, only
  ///             the rasterizer cache is purged.
  void NotifyLowMemoryWarning() const;

  //----------------------------------------------------------------------------
  /// @brief      Used by embedders to check if all shell subcomponents are
  ///             initialized. It is the embedder's responsibility to make this
  ///             call before accessing any other shell method. A shell that is
  ///             not setup must be discarded and another one created with
  ///             updated settings.
  ///
  /// @return     Returns if the shell has been setup. Once set up, this does
  ///             not change for the life-cycle of the shell.
  ///
  bool IsSetup() const;

  //----------------------------------------------------------------------------
  /// @brief      Captures a screenshot and optionally Base64 encodes the data
  ///             of the last layer tree rendered by the rasterizer in this
  ///             shell.
  ///
  /// @param[in]  type           The type of screenshot to capture.
  /// @param[in]  base64_encode  If the screenshot data should be base64
  ///                            encoded.
  ///
  /// @return     The screenshot result.
  ///
  Rasterizer::Screenshot Screenshot(Rasterizer::ScreenshotType type,
                                    bool base64_encode);

  //----------------------------------------------------------------------------
  /// @brief   Pauses the calling thread until the first frame is presented.
  ///
  /// @return  'kOk' when the first frame has been presented before the timeout
  ///          successfully, 'kFailedPrecondition' if called from the GPU or UI
  ///          thread, 'kDeadlineExceeded' if there is a timeout.
  ///
  fml::Status WaitForFirstFrame(fml::TimeDelta timeout);

  //----------------------------------------------------------------------------
  /// @brief      Used by embedders to reload the system fonts in
  /// FontCollection.
  ///             It also clears the cached font families and send system
  ///             channel message to framework to rebuild affected widgets.
  ///
  /// @return     Returns if shell reloads system fonts successfully.
  ///
  bool ReloadSystemFonts();

  //----------------------------------------------------------------------------
  /// @brief      Used by embedders to get the last error from the Dart UI
  ///             Isolate, if one exists.
  ///
  /// @return     Returns the last error code from the UI Isolate.
  ///
  std::optional<DartErrorCode> GetUIIsolateLastError() const;

  //----------------------------------------------------------------------------
  /// @brief      Used by embedders to check if the Engine is running and has
  ///             any live ports remaining. For example, the Flutter tester uses
  ///             this method to check whether it should continue to wait for
  ///             a running test or not.
  ///
  /// @return     Returns if the shell has an engine and the engine has any live
  ///             Dart ports.
  ///
  bool EngineHasLivePorts() const;

  //----------------------------------------------------------------------------
  /// @brief     Accessor for the disable GPU SyncSwitch
  std::shared_ptr<fml::SyncSwitch> GetIsGpuDisabledSyncSwitch() const;

  //----------------------------------------------------------------------------
  /// @brief      Get a pointer to the Dart VM used by this running shell
  ///             instance.
  ///
  /// @return     The Dart VM pointer.
  ///
  DartVM* GetDartVM();

 private:
  using ServiceProtocolHandler =
      std::function<bool(const ServiceProtocol::Handler::ServiceProtocolMap&,
                         rapidjson::Document&)>;

  const TaskRunners task_runners_;
  const Settings settings_;
  DartVMRef vm_;
  std::unique_ptr<PlatformView> platform_view_;  // on platform task runner
  std::unique_ptr<Engine> engine_;               // on UI task runner
  std::unique_ptr<Rasterizer> rasterizer_;       // on GPU task runner
  std::unique_ptr<ShellIOManager> io_manager_;   // on IO task runner
  std::shared_ptr<fml::SyncSwitch> is_gpu_disabled_sync_switch_;

  fml::WeakPtr<Engine> weak_engine_;          // to be shared across threads
  fml::WeakPtr<Rasterizer> weak_rasterizer_;  // to be shared across threads
  fml::WeakPtr<PlatformView>
      weak_platform_view_;  // to be shared across threads

  std::unordered_map<std::string_view,  // method
                     std::pair<fml::RefPtr<fml::TaskRunner>,
                               ServiceProtocolHandler>  // task-runner/function
                                                        // pair
                     >
      service_protocol_handlers_;
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

  Shell(DartVMRef vm, TaskRunners task_runners, Settings settings);

  static std::unique_ptr<Shell> CreateShellOnPlatformThread(
      DartVMRef vm,
      TaskRunners task_runners,
      const WindowData window_data,
      Settings settings,
      fml::RefPtr<const DartSnapshot> isolate_snapshot,
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

  // |PlatformView::Delegate|
  void OnPlatformViewDispatchSemanticsAction(
      int32_t id,
      SemanticsAction action,
      std::vector<uint8_t> args) override;

  // |PlatformView::Delegate|
  void OnPlatformViewSetSemanticsEnabled(bool enabled) override;

  // |shell:PlatformView::Delegate|
  void OnPlatformViewSetAccessibilityFeatures(int32_t flags) override;

  // |PlatformView::Delegate|
  void OnPlatformViewRegisterTexture(
      std::shared_ptr<flutter::Texture> texture) override;

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
  void OnAnimatorDraw(
      fml::RefPtr<Pipeline<flutter::LayerTree>> pipeline) override;

  // |Animator::Delegate|
  void OnAnimatorDrawLastLayerTree() override;

  // |Engine::Delegate|
  void OnEngineUpdateSemantics(
      SemanticsNodeUpdates update,
      CustomAccessibilityActionUpdates actions) override;

  // |Engine::Delegate|
  void OnEngineHandlePlatformMessage(
      fml::RefPtr<PlatformMessage> message) override;

  void HandleEngineSkiaMessage(fml::RefPtr<PlatformMessage> message);

  // |Engine::Delegate|
  void OnPreEngineRestart() override;

  // |Engine::Delegate|
  void UpdateIsolateDescription(const std::string isolate_name,
                                int64_t isolate_port) override;

  // |Engine::Delegate|
  void SetNeedsReportTimings(bool value) override;

  // |Rasterizer::Delegate|
  void OnFrameRasterized(const FrameTiming&) override;

  // |Rasterizer::Delegate|
  fml::Milliseconds GetFrameBudget() override;

  // |ServiceProtocol::Handler|
  fml::RefPtr<fml::TaskRunner> GetServiceProtocolHandlerTaskRunner(
      std::string_view method) const override;

  // |ServiceProtocol::Handler|
  bool HandleServiceProtocolMessage(
      std::string_view method,  // one if the extension names specified above.
      const ServiceProtocolMap& params,
      rapidjson::Document& response) override;

  // |ServiceProtocol::Handler|
  ServiceProtocol::Handler::Description GetServiceProtocolDescription()
      const override;

  // Service protocol handler
  bool OnServiceProtocolScreenshot(
      const ServiceProtocol::Handler::ServiceProtocolMap& params,
      rapidjson::Document& response);

  // Service protocol handler
  bool OnServiceProtocolScreenshotSKP(
      const ServiceProtocol::Handler::ServiceProtocolMap& params,
      rapidjson::Document& response);

  // Service protocol handler
  bool OnServiceProtocolRunInView(
      const ServiceProtocol::Handler::ServiceProtocolMap& params,
      rapidjson::Document& response);

  // Service protocol handler
  bool OnServiceProtocolFlushUIThreadTasks(
      const ServiceProtocol::Handler::ServiceProtocolMap& params,
      rapidjson::Document& response);

  // Service protocol handler
  bool OnServiceProtocolSetAssetBundlePath(
      const ServiceProtocol::Handler::ServiceProtocolMap& params,
      rapidjson::Document& response);

  // Service protocol handler
  bool OnServiceProtocolGetDisplayRefreshRate(
      const ServiceProtocol::Handler::ServiceProtocolMap& params,
      rapidjson::Document& response);

  // Service protocol handler
  //
  // The returned SkSLs are base64 encoded. Decode before storing them to files.
  bool OnServiceProtocolGetSkSLs(
      const ServiceProtocol::Handler::ServiceProtocolMap& params,
      rapidjson::Document& response);

  fml::WeakPtrFactory<Shell> weak_factory_;

  // For accessing the Shell via the raster thread, necessary for various
  // rasterizer callbacks.
  std::unique_ptr<fml::WeakPtrFactory<Shell>> weak_factory_gpu_;

  friend class testing::ShellTest;

  FML_DISALLOW_COPY_AND_ASSIGN(Shell);
};

}  // namespace flutter

#endif  // SHELL_COMMON_SHELL_H_
