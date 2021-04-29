#include "ui_mono_state.h"

#include "common/settings.h"
#include "common/task_runners.h"
#include "flutter/fml/message_loop.h"

namespace uiwidgets {

UIMonoState::UIMonoState(TaskRunners task_runners, TaskObserverAdd add_callback,
                         TaskObserverRemove remove_callback,
                         fml::WeakPtr<SnapshotDelegate> snapshot_delegate,
                         fml::WeakPtr<IOManager> io_manager,
                         fml::RefPtr<SkiaUnrefQueue> skia_unref_queue,
                         fml::WeakPtr<ImageDecoder> image_decoder)
    : task_runners_(std::move(task_runners)),
      add_callback_(std::move(add_callback)),
      remove_callback_(std::move(remove_callback)),
      snapshot_delegate_(std::move(snapshot_delegate)),
      io_manager_(std::move(io_manager)),
      skia_unref_queue_(std::move(skia_unref_queue)),
      image_decoder_(std::move(image_decoder)) {
  AddOrRemoveTaskObserver(true /* add */);
}

UIMonoState::~UIMonoState() { AddOrRemoveTaskObserver(false /* remove */); }

UIMonoState* UIMonoState::Current() {
  return static_cast<UIMonoState*>(MonoState::Current());
}

bool UIMonoState::EnsureCurrentIsolate() {
  return MonoState::EnsureCurrentIsolate();
}

void UIMonoState::SetWindow(std::unique_ptr<Window> window) {
  window_ = std::move(window);
}

const TaskRunners& UIMonoState::GetTaskRunners() const { return task_runners_; }

fml::WeakPtr<IOManager> UIMonoState::GetIOManager() const {
  return io_manager_;
}

fml::RefPtr<SkiaUnrefQueue> UIMonoState::GetSkiaUnrefQueue() const {
  return skia_unref_queue_;
}

void UIMonoState::ScheduleMicrotask(MonoMicrotaskQueue::CallbackFunc callback,
                                    Mono_Handle handle) {
  microtask_queue_.ScheduleMicrotask(callback, handle);
}

void UIMonoState::FlushMicrotasksNow() { microtask_queue_.RunMicrotasks(); }

void UIMonoState::AddOrRemoveTaskObserver(bool add) {
  auto task_runner = task_runners_.GetUITaskRunner();
  if (!task_runner) {
    // This may happen in case the isolate has no thread affinity (for example,
    // the service isolate).
    return;
  }
  FML_DCHECK(add_callback_ && remove_callback_);
  if (add) {
    add_callback_(reinterpret_cast<intptr_t>(this),
                  [this]() { this->FlushMicrotasksNow(); });
  } else {
    remove_callback_(reinterpret_cast<intptr_t>(this));
  }
}

fml::WeakPtr<SnapshotDelegate> UIMonoState::GetSnapshotDelegate() const {
  return snapshot_delegate_;
}

fml::WeakPtr<GrContext> UIMonoState::GetResourceContext() const {
  if (!io_manager_) {
    return {};
  }
  return io_manager_->GetResourceContext();
}

fml::WeakPtr<ImageDecoder> UIMonoState::GetImageDecoder() const {
  return image_decoder_;
}

UIWIDGETS_API(void)
UIMonoState_scheduleMicrotask(MonoMicrotaskQueue::CallbackFunc callback,
                              Mono_Handle handle) {
  UIMonoState::Current()->ScheduleMicrotask(callback, handle);
}

UIWIDGETS_API(void)
UIMonoState_postTaskForTime(MonoMicrotaskQueue::CallbackFunc callback,
                            Mono_Handle handle, int64_t target_time_nanos) {
  auto const weak_mono_state = MonoState::Current()->GetWeakPtr();

  UIMonoState::Current()->GetTaskRunners().GetUITaskRunner()->PostTaskForTime(
      [callback, handle, weak_mono_state]() -> void {
        if (auto mono_state = weak_mono_state.lock()) {
          MonoState::Scope mono_scope(mono_state.get());

          callback(handle);
        }
      },
      fml::TimePoint::FromEpochDelta(
          fml::TimeDelta::FromNanoseconds(target_time_nanos)));
}

UIWIDGETS_API(int64_t)
UIMonoState_timerMillisecondClock() { return Mono_TimelineGetMicros() / 1000; }

}  // namespace uiwidgets
