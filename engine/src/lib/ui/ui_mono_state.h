#pragma once

#include <flutter/fml/memory/weak_ptr.h>

#include "common/settings.h"
#include "common/task_runners.h"
#include "io_manager.h"
#include "runtime/mono_microtask_queue.h"
#include "runtime/mono_state.h"
#include "snapshot_delegate.h"
#include "lib/ui/window/window.h"
#include "lib/ui/painting/image_decoder.h"

namespace uiwidgets {
class UIMonoState : public MonoState {
 public:
  static UIMonoState* Current();

  static bool EnsureCurrentIsolate();

  Window* window() const { return window_.get(); }

  const TaskRunners& GetTaskRunners() const;

  void ScheduleMicrotask(MonoMicrotaskQueue::CallbackFunc callback,
                         Mono_Handle handle);

  void FlushMicrotasksNow();

  fml::WeakPtr<IOManager> GetIOManager() const;

  fml::RefPtr<SkiaUnrefQueue> GetSkiaUnrefQueue() const;

  fml::WeakPtr<SnapshotDelegate> GetSnapshotDelegate() const;

  fml::WeakPtr<GrContext> GetResourceContext() const;

  fml::WeakPtr<ImageDecoder> GetImageDecoder() const;

  template <class T>
  static SkiaGPUObject<T> CreateGPUObject(sk_sp<T> object) {
    if (!object) {
      return {};
    }
    auto* state = UIMonoState::Current();
    FML_DCHECK(state);
    auto queue = state->GetSkiaUnrefQueue();
    return {std::move(object), std::move(queue)};
  };

 protected:
  UIMonoState(TaskRunners task_runners, TaskObserverAdd add_callback,
              TaskObserverRemove remove_callback,
              fml::WeakPtr<SnapshotDelegate> snapshot_delegate,
              fml::WeakPtr<IOManager> io_manager,
              fml::RefPtr<SkiaUnrefQueue> skia_unref_queue,
              fml::WeakPtr<ImageDecoder> image_decoder);

  ~UIMonoState();

  void SetWindow(std::unique_ptr<Window> window);

 private:
  const TaskRunners task_runners_;
  const TaskObserverAdd add_callback_;
  const TaskObserverRemove remove_callback_;
  fml::WeakPtr<SnapshotDelegate> snapshot_delegate_;
  fml::WeakPtr<IOManager> io_manager_;
  fml::RefPtr<SkiaUnrefQueue> skia_unref_queue_;
  fml::WeakPtr<ImageDecoder> image_decoder_;
  std::unique_ptr<Window> window_;
  MonoMicrotaskQueue microtask_queue_;

  void AddOrRemoveTaskObserver(bool add);
};

}  // namespace uiwidgets