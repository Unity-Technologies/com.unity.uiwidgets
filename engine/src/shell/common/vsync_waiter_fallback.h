#pragma once

#include "flutter/fml/macros.h"
#include "flutter/fml/memory/weak_ptr.h"
#include "flutter/fml/time/time_point.h"
#include "vsync_waiter.h"

namespace uiwidgets {

/// A |VsyncWaiter| that will fire at 60 fps irrespective of the vsync.
class VsyncWaiterFallback final : public VsyncWaiter {
 public:
  VsyncWaiterFallback(TaskRunners task_runners);

  ~VsyncWaiterFallback() override;

 private:
  fml::TimePoint phase_;

  // |VsyncWaiter|
  void AwaitVSync() override;

  FML_DISALLOW_COPY_AND_ASSIGN(VsyncWaiterFallback);
};

}  // namespace uiwidgets
