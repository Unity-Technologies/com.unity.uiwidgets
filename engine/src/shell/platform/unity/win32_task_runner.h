#pragma once

#include <flutter/fml/closure.h>
#include <windows.h>

#include <chrono>
#include <deque>
#include <functional>
#include <map>
#include <mutex>
#include <queue>
#include <thread>

#include "shell/platform/embedder/embedder.h"

namespace uiwidgets {

class Win32TaskRunner {
 public:
  using TaskExpiredCallback = std::function<void(const UIWidgetsTask*)>;

  Win32TaskRunner(DWORD main_thread_id,
                  const TaskExpiredCallback& on_task_expired);

  ~Win32TaskRunner();

  // Returns if the current thread is the thread used by the win32 event loop.
  bool RunsTasksOnCurrentThread() const;

  std::chrono::nanoseconds ProcessTasks();

  // Post a Flutter engine tasks to the event loop for delayed execution.
  void PostTask(UIWidgetsTask uiwidgets_task,
                uint64_t uiwidgets_target_time_nanos);

  void AddTaskObserver(intptr_t key, const fml::closure& callback);

  void RemoveTaskObserver(intptr_t key);

  FML_DISALLOW_COPY_AND_ASSIGN(Win32TaskRunner);

 private:
  using TaskTimePoint = std::chrono::steady_clock::time_point;

  struct Task {
    uint64_t order;
    TaskTimePoint fire_time;
    UIWidgetsTask task;

    struct Comparer {
      bool operator()(const Task& a, const Task& b) {
        if (a.fire_time == b.fire_time) {
          return a.order > b.order;
        }
        return a.fire_time > b.fire_time;
      }
    };
  };

  DWORD main_thread_id_;
  TaskExpiredCallback on_task_expired_;
  std::mutex task_queue_mutex_;
  std::priority_queue<Task, std::deque<Task>, Task::Comparer> task_queue_;
  std::condition_variable task_queue_cv_;

  using TaskObservers = std::map<intptr_t, fml::closure>;
  TaskObservers task_observers_;

  static TaskTimePoint TimePointFromUIWidgetsTime(
      uint64_t uiwidgets_target_time_nanos);
};

}  // namespace uiwidgets
