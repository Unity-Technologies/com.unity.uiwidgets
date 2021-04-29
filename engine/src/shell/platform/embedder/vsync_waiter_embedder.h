#pragma once

#include "flutter/fml/macros.h"
#include "shell/common/vsync_waiter.h"

namespace uiwidgets {

class VsyncWaiterEmbedder final : public VsyncWaiter {
 public:
  using VsyncCallback = std::function<void(intptr_t)>;

  VsyncWaiterEmbedder(const VsyncCallback& callback, TaskRunners task_runners);

  ~VsyncWaiterEmbedder() override;

  static bool OnEmbedderVsync(intptr_t baton, fml::TimePoint frame_start_time,
                              fml::TimePoint frame_target_time);

 private:
  const VsyncCallback vsync_callback_;

  // |VsyncWaiter|
  void AwaitVSync() override;

  FML_DISALLOW_COPY_AND_ASSIGN(VsyncWaiterEmbedder);
};

}  // namespace uiwidgets
