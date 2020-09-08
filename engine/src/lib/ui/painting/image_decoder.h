#pragma once

#include <flutter/fml/concurrent_message_loop.h>
#include <flutter/fml/memory/weak_ptr.h>

#include "common/task_runners.h"
#include "lib/ui/io_manager.h"

namespace uiwidgets {

class ImageDecoder {
 public:
  ImageDecoder(
      TaskRunners runners,
      std::shared_ptr<fml::ConcurrentTaskRunner> concurrent_task_runner,
      fml::WeakPtr<IOManager> io_manager);

  fml::WeakPtr<ImageDecoder> GetWeakPtr() const;
};

}  // namespace uiwidgets
