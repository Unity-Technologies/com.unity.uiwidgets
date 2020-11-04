#include "engine.h"

#include <memory>
#include <string>
#include <utility>
#include <vector>

#include "common/settings.h"
#include "flutter/fml/eintr_wrapper.h"
#include "flutter/fml/file.h"
#include "flutter/fml/make_copyable.h"
#include "flutter/fml/paths.h"
#include "flutter/fml/trace_event.h"
#include "flutter/fml/unique_fd.h"
#include "include/core/SkCanvas.h"
#include "include/core/SkPictureRecorder.h"
#include "lib/ui/text/font_collection.h"
#include "rapidjson/document.h"
#include "shell/common/animator.h"
#include "shell/common/platform_view.h"
#include "shell/common/shell.h"

namespace uiwidgets {

static constexpr char kAssetChannel[] = "uiwidgets/assets";
static constexpr char kLifecycleChannel[] = "uiwidgets/lifecycle";
static constexpr char kNavigationChannel[] = "uiwidgets/navigation";
static constexpr char kLocalizationChannel[] = "uiwidgets/localization";
static constexpr char kSettingsChannel[] = "uiwidgets/settings";

Engine::Engine(Delegate& delegate,
               const PointerDataDispatcherMaker& dispatcher_maker,
               TaskRunners task_runners, const WindowData window_data,
               Settings settings, std::unique_ptr<Animator> animator,
               fml::WeakPtr<IOManager> io_manager,
               fml::RefPtr<SkiaUnrefQueue> unref_queue,
               fml::WeakPtr<SnapshotDelegate> snapshot_delegate)
    : delegate_(delegate),
      settings_(std::move(settings)),
      concurrent_message_loop_(fml::ConcurrentMessageLoop::Create()),
      animator_(std::move(animator)),
      activity_running_(true),
      have_surface_(false),
      image_decoder_(task_runners, concurrent_message_loop_->GetTaskRunner(),
                     io_manager),
      task_runners_(std::move(task_runners)),
      weak_factory_(this) {
  // Runtime controller is initialized here because it takes a reference to this
  // object as its delegate. The delegate may be called in the constructor and
  // we want to be fully initilazed by that point.
  runtime_controller_ = std::make_unique<RuntimeController>(
      *this,          // runtime delegate
      settings_,      // settings
      task_runners_,  // task runners
      std::move(snapshot_delegate),
      std::move(io_manager),        // io manager
      std::move(unref_queue),       // Skia unref queue
      image_decoder_.GetWeakPtr(),  // image decoder
      window_data                   // window data
  );

  pointer_data_dispatcher_ = dispatcher_maker(*this);
}

Engine::~Engine() = default;

float Engine::GetDisplayRefreshRate() const {
  return animator_->GetDisplayRefreshRate();
}

fml::WeakPtr<Engine> Engine::GetWeakPtr() const {
  return weak_factory_.GetWeakPtr();
}

bool Engine::UpdateAssetManager(
    std::shared_ptr<AssetManager> new_asset_manager) {
  if (asset_manager_ == new_asset_manager) {
    return false;
  }

  asset_manager_ = new_asset_manager;

  if (!asset_manager_) {
    return false;
  }
  rapidjson::Document document;
  document.Parse(settings_.font_data);
  if (!document.HasParseError()) {
    auto settings = document.GetObject();
    auto fontSettings = settings.FindMember("fonts");
    if (fontSettings != settings.MemberEnd() && fontSettings->value.IsArray()) {
      rapidjson::Value::Array fontArray = fontSettings->value.GetArray();
      font_collection_.RegisterFonts(asset_manager_, fontArray);
      return true;
    }
  } 
  font_collection_.RegisterFonts(asset_manager_);
  
  return true;
}

bool Engine::Restart(RunConfiguration configuration) {
  TRACE_EVENT0("uiwidgets", "Engine::Restart");
  if (!configuration.IsValid()) {
    FML_LOG(ERROR) << "Engine run configuration was invalid.";
    return false;
  }
  delegate_.OnPreEngineRestart();
  runtime_controller_ = runtime_controller_->Clone();
  UpdateAssetManager(nullptr);
  return Run(std::move(configuration)) == RunStatus::Success;
}

void Engine::SetupDefaultFontManager() {
  TRACE_EVENT0("uiwidgets", "Engine::SetupDefaultFontManager");
  font_collection_.SetupDefaultFontManager();
}

Engine::RunStatus Engine::Run(RunConfiguration configuration) {
  if (!configuration.IsValid()) {
    FML_LOG(ERROR) << "Engine run configuration was invalid.";
    return RunStatus::Failure;
  }

  auto isolate_launch_status =
      PrepareAndLaunchIsolate(std::move(configuration));
  if (isolate_launch_status == RunStatus::Failure) {
    FML_LOG(ERROR) << "Engine not prepare and launch isolate.";
    return isolate_launch_status;
  } else if (isolate_launch_status ==
             Engine::RunStatus::FailureAlreadyRunning) {
    return isolate_launch_status;
  }

  return RunStatus::Success;
}

Engine::RunStatus Engine::PrepareAndLaunchIsolate(
    RunConfiguration configuration) {
  TRACE_EVENT0("uiwidgets", "Engine::PrepareAndLaunchIsolate");

  UpdateAssetManager(configuration.GetAssetManager());

  std::shared_ptr<MonoIsolate> isolate =
      runtime_controller_->GetRootIsolate().lock();

  if (!isolate) {
    return RunStatus::Failure;
  }

  MonoState::Scope scope(isolate);
  const fml::closure entrypoint_callback =
      configuration.GetMonoEntrypointCallback();
  entrypoint_callback();

  return RunStatus::Success;
}

void Engine::BeginFrame(fml::TimePoint frame_time) {
  TRACE_EVENT0("uiwidgets", "Engine::BeginFrame");
  runtime_controller_->BeginFrame(frame_time);
}

void Engine::ReportTimings(std::vector<int64_t> timings) {
  TRACE_EVENT0("uiwidgets", "Engine::ReportTimings");
  runtime_controller_->ReportTimings(std::move(timings));
}

void Engine::NotifyIdle(int64_t deadline) {
  auto trace_event = std::to_string(deadline - Mono_TimelineGetMicros());
  TRACE_EVENT1("uiwidgets", "Engine::NotifyIdle", "deadline_now_delta",
               trace_event.c_str());
  runtime_controller_->NotifyIdle(deadline);
}

void Engine::OnOutputSurfaceCreated() {
  have_surface_ = true;
  StartAnimatorIfPossible();
  ScheduleFrame();
}

void Engine::OnOutputSurfaceDestroyed() {
  have_surface_ = false;
  StopAnimator();
}

void Engine::SetViewportMetrics(const ViewportMetrics& metrics) {
  bool dimensions_changed =
      viewport_metrics_.physical_height != metrics.physical_height ||
      viewport_metrics_.physical_width != metrics.physical_width ||
      viewport_metrics_.physical_depth != metrics.physical_depth;
  viewport_metrics_ = metrics;
  runtime_controller_->SetViewportMetrics(viewport_metrics_);
  if (animator_) {
    if (dimensions_changed) animator_->SetDimensionChangePending();
    if (have_surface_) ScheduleFrame();
  }
}

void Engine::DispatchPlatformMessage(fml::RefPtr<PlatformMessage> message) {
  if (message->channel() == kLifecycleChannel) {
    if (HandleLifecyclePlatformMessage(message.get())) return;
  } else if (message->channel() == kLocalizationChannel) {
    if (HandleLocalizationPlatformMessage(message.get())) return;
  } else if (message->channel() == kSettingsChannel) {
    HandleSettingsPlatformMessage(message.get());
    return;
  }

  if (runtime_controller_->DispatchPlatformMessage(std::move(message))) {
    return;
  }

  // If there's no runtime_, we may still need to set the initial route.
  if (message->channel() == kNavigationChannel) {
    HandleNavigationPlatformMessage(std::move(message));
    return;
  }

  FML_DLOG(WARNING) << "Dropping platform message on channel: "
                    << message->channel();
}

bool Engine::HandleLifecyclePlatformMessage(PlatformMessage* message) {
  const auto& data = message->data();
  std::string state(reinterpret_cast<const char*>(data.data()), data.size());
  if (state == "AppLifecycleState.paused" ||
      state == "AppLifecycleState.detached") {
    activity_running_ = false;
    StopAnimator();
  } else if (state == "AppLifecycleState.resumed" ||
             state == "AppLifecycleState.inactive") {
    activity_running_ = true;
    StartAnimatorIfPossible();
  }

  // Always schedule a frame when the app does become active as per API
  // recommendation
  // https://developer.apple.com/documentation/uikit/uiapplicationdelegate/1622956-applicationdidbecomeactive?language=objc
  if (state == "AppLifecycleState.resumed" && have_surface_) {
    ScheduleFrame();
  }
  runtime_controller_->SetLifecycleState(state);
  // Always forward these messages to the framework by returning false.
  return false;
}

bool Engine::HandleNavigationPlatformMessage(
    fml::RefPtr<PlatformMessage> message) {
  const auto& data = message->data();

  rapidjson::Document document;
  document.Parse(reinterpret_cast<const char*>(data.data()), data.size());
  if (document.HasParseError() || !document.IsObject()) return false;
  auto root = document.GetObject();
  auto method = root.FindMember("method");
  if (method->value != "setInitialRoute") return false;
  auto route = root.FindMember("args");
  initial_route_ = std::move(route->value.GetString());
  return true;
}

bool Engine::HandleLocalizationPlatformMessage(PlatformMessage* message) {
  const auto& data = message->data();

  rapidjson::Document document;
  document.Parse(reinterpret_cast<const char*>(data.data()), data.size());
  if (document.HasParseError() || !document.IsObject()) return false;
  auto root = document.GetObject();
  auto method = root.FindMember("method");
  if (method == root.MemberEnd() || method->value != "setLocale") return false;

  auto args = root.FindMember("args");
  if (args == root.MemberEnd() || !args->value.IsArray()) return false;

  const size_t strings_per_locale = 4;
  if (args->value.Size() % strings_per_locale != 0) return false;
  std::vector<std::string> locale_data;
  for (size_t locale_index = 0; locale_index < args->value.Size();
       locale_index += strings_per_locale) {
    if (!args->value[locale_index].IsString() ||
        !args->value[locale_index + 1].IsString())
      return false;
    locale_data.push_back(args->value[locale_index].GetString());
    locale_data.push_back(args->value[locale_index + 1].GetString());
    locale_data.push_back(args->value[locale_index + 2].GetString());
    locale_data.push_back(args->value[locale_index + 3].GetString());
  }

  return runtime_controller_->SetLocales(locale_data);
}

void Engine::HandleSettingsPlatformMessage(PlatformMessage* message) {
  const auto& data = message->data();
  std::string jsonData(reinterpret_cast<const char*>(data.data()), data.size());
  if (runtime_controller_->SetUserSettingsData(std::move(jsonData)) &&
      have_surface_) {
    ScheduleFrame();
  }
}

void Engine::DispatchPointerDataPacket(
    std::unique_ptr<PointerDataPacket> packet, uint64_t trace_flow_id) {
  TRACE_EVENT0("uiwidgets", "Engine::DispatchPointerDataPacket");
  TRACE_FLOW_STEP("uiwidgets", "PointerEvent", trace_flow_id);
  pointer_data_dispatcher_->DispatchPacket(std::move(packet), trace_flow_id);
}

void Engine::SetAccessibilityFeatures(int32_t flags) {
  runtime_controller_->SetAccessibilityFeatures(flags);
}

void Engine::StopAnimator() { animator_->Stop(); }

void Engine::StartAnimatorIfPossible() {
  if (activity_running_ && have_surface_) animator_->Start();
}

std::string Engine::DefaultRouteName() {
  if (!initial_route_.empty()) {
    return initial_route_;
  }
  return "/";
}

void Engine::ScheduleFrame(bool regenerate_layer_tree) {
  animator_->RequestFrame(regenerate_layer_tree);
}

void Engine::Render(std::unique_ptr<uiwidgets::LayerTree> layer_tree) {
  if (!layer_tree) return;

  // Ensure frame dimensions are sane.
  if (layer_tree->frame_size().isEmpty() ||
      layer_tree->frame_physical_depth() <= 0.0f ||
      layer_tree->frame_device_pixel_ratio() <= 0.0f)
    return;

  animator_->Render(std::move(layer_tree));
}

void Engine::HandlePlatformMessage(fml::RefPtr<PlatformMessage> message) {
  if (message->channel() == kAssetChannel) {
    HandleAssetPlatformMessage(std::move(message));
  } else {
    delegate_.OnEngineHandlePlatformMessage(std::move(message));
  }
}

void Engine::SetNeedsReportTimings(bool needs_reporting) {
  delegate_.SetNeedsReportTimings(needs_reporting);
}

FontCollection& Engine::GetFontCollection() { return font_collection_; }

void Engine::DoDispatchPacket(std::unique_ptr<PointerDataPacket> packet,
                              uint64_t trace_flow_id) {
  animator_->EnqueueTraceFlowId(trace_flow_id);
  if (runtime_controller_) {
    runtime_controller_->DispatchPointerDataPacket(*packet);
  }
}

void Engine::ScheduleSecondaryVsyncCallback(const fml::closure& callback) {
  animator_->ScheduleSecondaryVsyncCallback(callback);
}

std::shared_ptr<fml::ConcurrentMessageLoop> Engine::GetConcurrentMessageLoop() {
  return concurrent_message_loop_;
}

void Engine::HandleAssetPlatformMessage(fml::RefPtr<PlatformMessage> message) {
  fml::RefPtr<PlatformMessageResponse> response = message->response();
  if (!response) {
    return;
  }
  const auto& data = message->data();
  std::string asset_name(reinterpret_cast<const char*>(data.data()),
                         data.size());

  if (asset_manager_) {
    std::unique_ptr<fml::Mapping> asset_mapping =
        asset_manager_->GetAsMapping(asset_name);
    if (asset_mapping) {
      response->Complete(std::move(asset_mapping));
      return;
    }
  }

  response->CompleteEmpty();
}

}  // namespace uiwidgets
