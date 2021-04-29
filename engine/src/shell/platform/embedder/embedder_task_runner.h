#pragma once

#include <mutex>
#include <unordered_map>

#include "flutter/fml/macros.h"
#include "flutter/fml/task_runner.h"

namespace uiwidgets {

class EmbedderTaskRunner final : public fml::TaskRunner {
 public:
  struct DispatchTable {
    std::function<void(EmbedderTaskRunner* task_runner, uint64_t task_baton,
                       fml::TimePoint target_time)>
        post_task_callback;

    std::function<bool(void)> runs_task_on_current_thread_callback;
  };

  EmbedderTaskRunner(DispatchTable table, size_t embedder_identifier);

  ~EmbedderTaskRunner() override;

  size_t GetEmbedderIdentifier() const;

  bool PostTask(uint64_t baton);

  void Terminate();

 private:
  const size_t embedder_identifier_;
  DispatchTable dispatch_table_;
  std::mutex tasks_mutex_;
  uint64_t last_baton_;
  std::unordered_map<uint64_t, fml::closure> pending_tasks_;
  fml::TaskQueueId placeholder_id_;
  bool terminated_ = false;

  void PostTask(const fml::closure& task) override;

  void PostTaskForTime(const fml::closure& task,
                       fml::TimePoint target_time) override;

  void PostDelayedTask(const fml::closure& task, fml::TimeDelta delay) override;

  bool RunsTasksOnCurrentThread() override;

  fml::TaskQueueId GetTaskQueueId() override;

  FML_DISALLOW_COPY_AND_ASSIGN(EmbedderTaskRunner);
};

}  // namespace uiwidgets
