#include "win32_task_runner.h"

#include <flutter/fml/time/time_point.h>

#include <atomic>
#include <utility>

namespace uiwidgets {

Win32TaskRunner::Win32TaskRunner(DWORD main_thread_id,
                                 const TaskExpiredCallback& on_task_expired)
    : main_thread_id_(main_thread_id),
      on_task_expired_(std::move(on_task_expired)) {}

Win32TaskRunner::~Win32TaskRunner() = default;

bool Win32TaskRunner::RunsTasksOnCurrentThread() const {
  return GetCurrentThreadId() == main_thread_id_;
}

std::chrono::nanoseconds Win32TaskRunner::ProcessTasks() {
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

Win32TaskRunner::TaskTimePoint Win32TaskRunner::TimePointFromUIWidgetsTime(
    uint64_t uiwidgets_target_time_nanos) {
  const auto fml_now = fml::TimePoint::Now().ToEpochDelta().ToNanoseconds();
  if (uiwidgets_target_time_nanos <= fml_now) {
    return {};
  }
  const auto uiwidgets_duration = uiwidgets_target_time_nanos - fml_now;
  const auto now = TaskTimePoint::clock::now();
  return now + std::chrono::nanoseconds(uiwidgets_duration);
}

void Win32TaskRunner::PostTask(UIWidgetsTask uiwidgets_task,
                               uint64_t uiwidgets_target_time_nanos) {
  static std::atomic_uint64_t sGlobalTaskOrder(0);

  Task task;
  task.order = ++sGlobalTaskOrder;
  task.fire_time = TimePointFromUIWidgetsTime(uiwidgets_target_time_nanos);
  task.task = uiwidgets_task;

  {
    std::lock_guard<std::mutex> lock(task_queue_mutex_);
    task_queue_.push(task);

    // Make sure the queue mutex is unlocked before waking up the loop. In case
    // the wake causes this thread to be descheduled for the primary thread to
    // process tasks, the acquisition of the lock on that thread while holding
    // the lock here momentarily till the end of the scope is a pessimization.
  }

  if (!PostThreadMessage(main_thread_id_, WM_NULL, 0, 0)) {
    OutputDebugString(L"Failed to post message to main thread.");
  }
}

void Win32TaskRunner::AddTaskObserver(intptr_t key,
                                      const fml::closure& callback) {
  task_observers_[key] = callback;
}

void Win32TaskRunner::RemoveTaskObserver(intptr_t key) {
  task_observers_.erase(key);
}
}  // namespace uiwidgets
