#include "gfx_worker_task_runner.h"

#include <flutter/fml/logging.h>
#include <flutter/fml/synchronization/waitable_event.h>
#include <flutter/fml/time/time_point.h>

#include <atomic>
#include <utility>

#include "uiwidgets_system.h"

namespace uiwidgets {

GfxWorkerTaskRunner::GfxWorkerTaskRunner(std::thread::id gfx_worker_thread_id,
                                         TaskExpiredCallback on_task_expired)
    : gfx_worker_thread_id_(gfx_worker_thread_id),
      on_task_expired_(std::move(on_task_expired)),
      weak_factory_(this) {}

GfxWorkerTaskRunner::~GfxWorkerTaskRunner() {
  // drain pending tasks.
  fml::AutoResetWaitableEvent latch;
  UIWidgetsSystem::GetInstancePtr()->PostTaskToGfxWorker(
      [&latch]() -> void { latch.Signal(); });
  latch.Wait();
}

bool GfxWorkerTaskRunner::RunsTasksOnCurrentThread() const {
  return std::this_thread::get_id() == gfx_worker_thread_id_;
}

void GfxWorkerTaskRunner::PostTask(UIWidgetsTask uiwidgets_task,
                                   uint64_t uiwidgets_target_time_nanos) {
  FML_DCHECK(uiwidgets_target_time_nanos <=
             (uint64_t)fml::TimePoint::Now().ToEpochDelta().ToNanoseconds());

  UIWidgetsSystem::GetInstancePtr()->PostTaskToGfxWorker(
      [&on_task_expired = on_task_expired_, uiwidgets_task]() -> void {
        on_task_expired(&uiwidgets_task);
      });
}

}  // namespace uiwidgets
