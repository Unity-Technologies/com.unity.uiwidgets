#pragma once

#include <chrono>
#include <deque>
#include <functional>
#include <mutex>
#include <queue>
#include <thread>
#include <flutter/fml/memory/weak_ptr.h>


#include "flutter/fml/macros.h"
#include "shell/platform/embedder/embedder.h"

namespace uiwidgets {

class GfxWorkerTaskRunner {
 public:
  using TaskExpiredCallback = std::function<void(const UIWidgetsTask*)>;

  GfxWorkerTaskRunner(std::thread::id gfx_worker_thread_id,
                      TaskExpiredCallback on_task_expired);

  ~GfxWorkerTaskRunner();

  bool RunsTasksOnCurrentThread() const;

  void PostTask(UIWidgetsTask uiwidgets_task,
                uint64_t uiwidgets_target_time_nanos);

  FML_DISALLOW_COPY_AND_ASSIGN(GfxWorkerTaskRunner);

 private:
  using TaskTimePoint = std::chrono::steady_clock::time_point;

  std::thread::id gfx_worker_thread_id_;
  TaskExpiredCallback on_task_expired_;
  fml::WeakPtrFactory<GfxWorkerTaskRunner> weak_factory_;
};

}  // namespace uiwidgets
