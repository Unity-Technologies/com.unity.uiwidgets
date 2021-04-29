#include "runtime_controller.h"

#include <utility>

#include "flutter/fml/message_loop.h"
#include "flutter/fml/trace_event.h"
#include "lib/ui/compositing/scene.h"
#include "lib/ui/ui_mono_state.h"
#include "lib/ui/window/window.h"
#include "runtime_delegate.h"

namespace uiwidgets {

RuntimeController::RuntimeController(
    RuntimeDelegate& client, const Settings& settings, TaskRunners task_runners,
    fml::WeakPtr<SnapshotDelegate> snapshot_delegate,
    fml::WeakPtr<IOManager> io_manager, fml::RefPtr<SkiaUnrefQueue> unref_queue,
    fml::WeakPtr<ImageDecoder> image_decoder, const WindowData& window_data)
    : client_(client),
      settings_(settings),
      task_runners_(task_runners),
      snapshot_delegate_(std::move(snapshot_delegate)),
      io_manager_(std::move(io_manager)),
      unref_queue_(std::move(unref_queue)),
      image_decoder_(std::move(image_decoder)),
      window_data_(window_data) {
  auto strong_root_isolate =
      MonoIsolate::CreateRootIsolate(settings_,                       //
                                     task_runners_,                   //
                                     std::make_unique<Window>(this),  //
                                     snapshot_delegate_,              //
                                     io_manager_,                     //
                                     unref_queue_,                    //
                                     image_decoder_                   //
                                     )
          .lock();

  FML_CHECK(strong_root_isolate) << "Could not create root isolate.";

  // The root isolate ivar is weak.
  root_isolate_ = strong_root_isolate;

  if (auto* window = GetWindowIfAvailable()) {
    MonoState::Scope scope(strong_root_isolate);
    window->DidCreateIsolate();
    if (!FlushRuntimeStateToIsolate()) {
      FML_DLOG(ERROR) << "Could not setup initial isolate state.";
    }
  } else {
    FML_DCHECK(false) << "RuntimeController created without window binding.";
  }

  FML_DCHECK(Mono_CurrentIsolate() == nullptr);
}

RuntimeController::~RuntimeController() {
  FML_DCHECK(Mono_CurrentIsolate() == nullptr);
  std::shared_ptr<MonoIsolate> root_isolate = root_isolate_.lock();
  if (root_isolate) {
    auto result = root_isolate->Shutdown();
    if (!result) {
      FML_DLOG(ERROR) << "Could not shutdown the root isolate.";
    }
    root_isolate_ = {};
  }
}

std::unique_ptr<RuntimeController> RuntimeController::Clone() const {
  return std::make_unique<RuntimeController>(client_,             //
                                             settings_,           //
                                             task_runners_,       //
                                             snapshot_delegate_,  //
                                             io_manager_,         //
                                             unref_queue_,        //
                                             image_decoder_,      //
                                             window_data_         //
  );
}

bool RuntimeController::FlushRuntimeStateToIsolate() {
  return SetViewportMetrics(window_data_.viewport_metrics) &&
         SetLocales(window_data_.locale_data) &&
         SetAccessibilityFeatures(window_data_.accessibility_feature_flags_) &&
         SetUserSettingsData(window_data_.user_settings_data) &&
         SetLifecycleState(window_data_.lifecycle_state);
}

bool RuntimeController::SetViewportMetrics(const ViewportMetrics& metrics) {
  window_data_.viewport_metrics = metrics;

  if (auto* window = GetWindowIfAvailable()) {
    window->UpdateWindowMetrics(metrics);
    return true;
  }
  return false;
}

bool RuntimeController::SetLocales(
    const std::vector<std::string>& locale_data) {
  window_data_.locale_data = locale_data;

  if (auto* window = GetWindowIfAvailable()) {
    window->UpdateLocales(locale_data);
    return true;
  }

  return false;
}

bool RuntimeController::SetUserSettingsData(const std::string& data) {
  window_data_.user_settings_data = data;

  if (auto* window = GetWindowIfAvailable()) {
    window->UpdateUserSettingsData(window_data_.user_settings_data);
    return true;
  }

  return false;
}

bool RuntimeController::SetLifecycleState(const std::string& data) {
  window_data_.lifecycle_state = data;

  if (auto* window = GetWindowIfAvailable()) {
    window->UpdateLifecycleState(window_data_.lifecycle_state);
    return true;
  }

  return false;
}

bool RuntimeController::SetAccessibilityFeatures(int32_t flags) {
  window_data_.accessibility_feature_flags_ = flags;
  if (auto* window = GetWindowIfAvailable()) {
    window->UpdateAccessibilityFeatures(
        window_data_.accessibility_feature_flags_);
    return true;
  }

  return false;
}

bool RuntimeController::BeginFrame(fml::TimePoint frame_time) {
  if (auto* window = GetWindowIfAvailable()) {
    window->BeginFrame(frame_time);
    return true;
  }
  return false;
}

bool RuntimeController::ReportTimings(std::vector<int64_t> timings) {
  if (auto* window = GetWindowIfAvailable()) {
    window->ReportTimings(std::move(timings));
    return true;
  }
  return false;
}

bool RuntimeController::NotifyIdle(int64_t deadline) {
  std::shared_ptr<MonoIsolate> root_isolate = root_isolate_.lock();
  if (!root_isolate) {
    return false;
  }

  MonoState::Scope scope(root_isolate);
  Mono_NotifyIdle(deadline);

  return true;
}

bool RuntimeController::DispatchPlatformMessage(
    fml::RefPtr<PlatformMessage> message) {
  if (auto* window = GetWindowIfAvailable()) {
    TRACE_EVENT1("uiwidgets", "RuntimeController::DispatchPlatformMessage",
                 "mode", "basic");
    window->DispatchPlatformMessage(std::move(message));
    return true;
  }
  return false;
}

bool RuntimeController::DispatchPointerDataPacket(
    const PointerDataPacket& packet) {
  if (auto* window = GetWindowIfAvailable()) {
    TRACE_EVENT1("uiwidgets", "RuntimeController::DispatchPointerDataPacket",
                 "mode", "basic");
    window->DispatchPointerDataPacket(packet);
    return true;
  }
  return false;
}

Window* RuntimeController::GetWindowIfAvailable() {
  std::shared_ptr<MonoIsolate> root_isolate = root_isolate_.lock();
  return root_isolate ? root_isolate->window() : nullptr;
}

// |WindowClient|
std::string RuntimeController::DefaultRouteName() {
  return client_.DefaultRouteName();
}

// |WindowClient|
void RuntimeController::ScheduleFrame() { client_.ScheduleFrame(); }

// |WindowClient|
void RuntimeController::Render(Scene* scene) {
  client_.Render(scene->takeLayerTree());
}

// |WindowClient|
void RuntimeController::HandlePlatformMessage(
    fml::RefPtr<PlatformMessage> message) {
  client_.HandlePlatformMessage(std::move(message));
}

// |WindowClient|
FontCollection& RuntimeController::GetFontCollection() {
  return client_.GetFontCollection();
}

// |WindowClient|
void RuntimeController::SetNeedsReportTimings(bool value) {
  client_.SetNeedsReportTimings(value);
}

std::weak_ptr<MonoIsolate> RuntimeController::GetRootIsolate() {
  return root_isolate_;
}

RuntimeController::Locale::Locale(std::string language_code_,
                                  std::string country_code_,
                                  std::string script_code_,
                                  std::string variant_code_)
    : language_code(language_code_),
      country_code(country_code_),
      script_code(script_code_),
      variant_code(variant_code_) {}

RuntimeController::Locale::~Locale() = default;

}  // namespace uiwidgets
