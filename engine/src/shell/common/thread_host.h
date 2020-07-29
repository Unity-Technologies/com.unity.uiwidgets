#pragma once

#include <memory>

#include "flutter/fml/macros.h"
#include "flutter/fml/thread.h"

namespace uiwidgets {

/// The collection of all the threads used by the engine.
struct ThreadHost {
  enum Type {
    Platform = 1 << 0,
    UI = 1 << 1,
    GPU = 1 << 2,
    IO = 1 << 3,
  };

  std::unique_ptr<fml::Thread> platform_thread;
  std::unique_ptr<fml::Thread> ui_thread;
  std::unique_ptr<fml::Thread> raster_thread;
  std::unique_ptr<fml::Thread> io_thread;

  ThreadHost();

  ThreadHost(ThreadHost&&);

  ThreadHost& operator=(ThreadHost&&) = default;

  ThreadHost(std::string name_prefix, uint64_t type_mask);

  ~ThreadHost();

  void Reset();
};

}  // namespace uiwidgets
