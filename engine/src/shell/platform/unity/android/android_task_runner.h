#pragma once
#include <flutter/fml/closure.h>

#include <chrono>
#include <deque>
#include <map>
#include <mutex>
#include <queue>

#include "shell/platform/embedder/embedder.h"

#include <flutter/fml/memory/ref_counted.h>
#include "runtime/mono_api.h"

namespace uiwidgets {

class CocoaTaskRunner {
 public:
  using TaskExpiredCallback = std::function<void(const UIWidgetsTask*)>;

  CocoaTaskRunner(pid_t threadId, const TaskExpiredCallback& on_task_expired);

  ~CocoaTaskRunner();

  std::chrono::nanoseconds ProcessTasks();

  // Post a Flutter engine tasks to the event loop for delayed execution.
  void PostTask(UIWidgetsTask uiwidgets_task,
                uint64_t uiwidgets_target_time_nanos);

  void AddTaskObserver(intptr_t key, const fml::closure& callback);

  void RemoveTaskObserver(intptr_t key);

  bool RunsTasksOnCurrentThread();

  FML_DISALLOW_COPY_AND_ASSIGN(CocoaTaskRunner);

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

  TaskExpiredCallback on_task_expired_;
  std::mutex task_queue_mutex_;
  std::priority_queue<Task, std::deque<Task>, Task::Comparer> task_queue_;
  pid_t threadId;

  using TaskObservers = std::map<intptr_t, fml::closure>;
  TaskObservers task_observers_;

  static TaskTimePoint TimePointFromUIWidgetsTime(
      uint64_t uiwidgets_target_time_nanos);
};

}  // namespace uiwidgets
