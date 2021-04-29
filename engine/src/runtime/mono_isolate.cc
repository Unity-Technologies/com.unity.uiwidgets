#include "mono_isolate.h"

#include <cstdlib>
#include <tuple>

#include "flutter/fml/paths.h"
#include "flutter/fml/posix_wrappers.h"
#include "flutter/fml/trace_event.h"

namespace uiwidgets {

std::weak_ptr<MonoIsolate> MonoIsolate::CreateRootIsolate(
    const Settings& settings, TaskRunners task_runners,
    std::unique_ptr<Window> window,
    fml::WeakPtr<SnapshotDelegate> snapshot_delegate,
    fml::WeakPtr<IOManager> io_manager, fml::RefPtr<SkiaUnrefQueue> unref_queue,
    fml::WeakPtr<ImageDecoder> image_decoder) {
  TRACE_EVENT0("uiwidgets", "Isolate::CreateRootIsolate");

  auto isolate_data = new std::shared_ptr<MonoIsolate>(new MonoIsolate(
      settings,                      // settings
      task_runners,                  // task runners
      std::move(snapshot_delegate),  // snapshot delegate
      std::move(io_manager),         // IO manager
      std::move(unref_queue),        // Skia unref queue
      std::move(image_decoder)       // Image Decoder
      ));

  Mono_Isolate isolate = Mono_CreateIsolate(isolate_data);
  (*isolate_data)->SetIsolate(isolate);

  (*isolate_data)->SetMessageHandlingTaskRunner(task_runners.GetUITaskRunner());
  (*isolate_data)->SetWindow(std::move(window));

  return (*isolate_data)->GetWeakIsolatePtr();
}

MonoIsolate::MonoIsolate(const Settings& settings, TaskRunners task_runners,
                         fml::WeakPtr<SnapshotDelegate> snapshot_delegate,
                         fml::WeakPtr<IOManager> io_manager,
                         fml::RefPtr<SkiaUnrefQueue> unref_queue,
                         fml::WeakPtr<ImageDecoder> image_decoder)
    : UIMonoState(std::move(task_runners), settings.task_observer_add,
                  settings.task_observer_remove, std::move(snapshot_delegate),
                  std::move(io_manager), std::move(unref_queue),
                  std::move(image_decoder)) {}

MonoIsolate::~MonoIsolate() {
  if (GetMessageHandlingTaskRunner()) {
    FML_DCHECK(GetMessageHandlingTaskRunner()->RunsTasksOnCurrentThread());
  }
}

fml::RefPtr<fml::TaskRunner> MonoIsolate::GetMessageHandlingTaskRunner() const {
  return message_handling_task_runner_;
}

void MonoIsolate::SetMessageHandlingTaskRunner(
    fml::RefPtr<fml::TaskRunner> runner) {
  if (!runner) {
    return;
  }

  message_handling_task_runner_ = runner;
}

std::weak_ptr<MonoIsolate> MonoIsolate::GetWeakIsolatePtr() {
  return std::static_pointer_cast<MonoIsolate>(shared_from_this());
}

void MonoIsolate::AddIsolateShutdownCallback(const fml::closure& closure) {
  shutdown_callbacks_.emplace_back(std::make_unique<AutoFireClosure>(closure));
}

bool MonoIsolate::Shutdown() {
  TRACE_EVENT0("uiwidgets", "MonoIsolate::Shutdown");

  Mono_Isolate mono_isolate = isolate();
  // The isolate can be nullptr if this instance is the stub isolate data used
  // during root isolate creation.
  if (mono_isolate != nullptr) {
    // We need to enter the isolate because Mono_ShutdownIsolate does not take
    // the isolate to shutdown as a parameter.
    FML_DCHECK(Mono_CurrentIsolate() == nullptr);
    Mono_EnterIsolate(mono_isolate);
    Mono_ShutdownIsolate();
    FML_DCHECK(Mono_CurrentIsolate() == nullptr);
  }

  return true;
}

MonoIsolate::AutoFireClosure::AutoFireClosure(const fml::closure& closure)
    : closure_(closure) {}

MonoIsolate::AutoFireClosure::~AutoFireClosure() {
  if (closure_) {
    closure_();
  }
}

}  // namespace uiwidgets
