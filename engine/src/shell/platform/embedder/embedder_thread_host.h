#pragma once

#include <map>
#include <memory>
#include <set>

#include "common/task_runners.h"
#include "embedder.h"
#include "embedder_task_runner.h"
#include "flutter/fml/macros.h"
#include "shell/common/thread_host.h"

namespace uiwidgets {

class EmbedderThreadHost {
 public:
  static std::unique_ptr<EmbedderThreadHost> CreateEmbedderManagedThreadHost(
      const UIWidgetsCustomTaskRunners* custom_task_runners);

  EmbedderThreadHost(
      ThreadHost host, TaskRunners runners,
      const std::set<fml::RefPtr<EmbedderTaskRunner>>& embedder_task_runners);

  ~EmbedderThreadHost();

  bool IsValid() const;

  const TaskRunners& GetTaskRunners() const;

  bool PostTask(int64_t runner, uint64_t task) const;

 private:
  ThreadHost host_;
  TaskRunners runners_;
  std::map<int64_t, fml::RefPtr<EmbedderTaskRunner>> runners_map_;

  FML_DISALLOW_COPY_AND_ASSIGN(EmbedderThreadHost);
};

}  // namespace uiwidgets
