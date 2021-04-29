#pragma once

#include "flow/skia_gpu_object.h"
#include "flutter/fml/memory/weak_ptr.h"
#include "flutter/fml/synchronization/sync_switch.h"
#include "include/gpu/GrContext.h"

namespace uiwidgets {

class IOManager {
 public:
  virtual ~IOManager() = default;

  virtual fml::WeakPtr<IOManager> GetWeakIOManager() const = 0;

  virtual fml::WeakPtr<GrContext> GetResourceContext() const = 0;

  virtual fml::RefPtr<SkiaUnrefQueue> GetSkiaUnrefQueue() const = 0;

  virtual std::shared_ptr<fml::SyncSwitch> GetIsGpuDisabledSyncSwitch() = 0;
};

}  // namespace uiwidgets
