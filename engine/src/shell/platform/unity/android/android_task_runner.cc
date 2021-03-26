#include "android_task_runner.h"

#include <flutter/fml/time/time_point.h>

#include <atomic>
#include <utility>

namespace uiwidgets {

CocoaTaskRunner::CocoaTaskRunner(pid_t threadId, const TaskExpiredCallback& on_task_expired)
    : on_task_expired_(std::move(on_task_expired)),
    threadId(threadId)
     {}

CocoaTaskRunner::~CocoaTaskRunner() = default;

std::chrono::nanoseconds CocoaTaskRunner::ProcessTasks() {
  const TaskTimePoint now = TaskTimePoint::clock::now();

  std::vector<UIWidgetsTask> expired_tasks;

  // Process expired tasks.
  {
    std::lock_guard<std::mutex> lock(task_queue_mutex_);
    while (!task_queue_.empty()) {
      const auto& top = task_queue_.top();
      // If this task (and all tasks after this) has not yet expired, there is
      // nothing more to do. Quit iterating.
      if (top.fire_time > now) {
        break;
      }

      // Make a record of the expired task. Do NOT service the task here
      // because we are still holding onto the task queue mutex. We don't want
      // other threads to block on posting tasks onto this thread till we are
      // done processing expired tasks.
      expired_tasks.push_back(task_queue_.top().task);

      // Remove the tasks from the delayed tasks queue.
      task_queue_.pop();
    }
  }

  for (const auto& observer : task_observers_) {
    observer.second();
  }

  // Fire expired tasks.
  {
    // Flushing tasks here without holing onto the task queue mutex.
    for (const auto& task : expired_tasks) {
      on_task_expired_(&task);

      for (const auto& observer : task_observers_) {
        observer.second();
      }
    }
  }

  if (!expired_tasks.empty()) {
    return ProcessTasks();
  }

  // Calculate duration to sleep for on next iteration.
  {
    std::lock_guard<std::mutex> lock(task_queue_mutex_);
    const auto next_wake = task_queue_.empty() ? TaskTimePoint::max()
                                               : task_queue_.top().fire_time;

    return std::min(next_wake - now, std::chrono::nanoseconds::max());
  }
}

CocoaTaskRunner::TaskTimePoint CocoaTaskRunner::TimePointFromUIWidgetsTime(
    uint64_t uiwidgets_target_time_nanos) {
  const auto fml_now = fml::TimePoint::Now().ToEpochDelta().ToNanoseconds();
  if (uiwidgets_target_time_nanos <= (uint64_t)fml_now) {
    return {};
  }
  const auto uiwidgets_duration = uiwidgets_target_time_nanos - fml_now;
  const auto now = TaskTimePoint::clock::now();
  return now + std::chrono::nanoseconds(uiwidgets_duration);
}

void CocoaTaskRunner::PostTask(UIWidgetsTask uiwidgets_task,
                               uint64_t uiwidgets_target_time_nanos) {
  static std::atomic_uint64_t sGlobalTaskOrder(0);

  Task task;
  task.order = ++sGlobalTaskOrder;
  task.fire_time = TimePointFromUIWidgetsTime(uiwidgets_target_time_nanos);
  task.task = uiwidgets_task;

  {
    std::lock_guard<std::mutex> lock(task_queue_mutex_);
    task_queue_.push(task);
  }
}

bool CocoaTaskRunner::RunsTasksOnCurrentThread(){
  pid_t id = gettid();
  return threadId == id;
}

void CocoaTaskRunner::AddTaskObserver(intptr_t key,
                                      const fml::closure& callback) {
  task_observers_[key] = callback;
}

void CocoaTaskRunner::RemoveTaskObserver(intptr_t key) {
  task_observers_.erase(key);
}
}  // namespace uiwidgets
