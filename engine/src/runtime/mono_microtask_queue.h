#pragma once

#include <vector>

#include "mono_state.h"
#include "runtime/mono_api.h"

namespace uiwidgets {

class MonoMicrotaskQueue {
 public:
  MonoMicrotaskQueue();
  ~MonoMicrotaskQueue();

  typedef void (*CallbackFunc)(Mono_Handle handle);

  void ScheduleMicrotask(CallbackFunc func, Mono_Handle handle);

  void RunMicrotasks();

  bool HasMicrotasks() const { return !queue_.empty(); }

 private:
  struct Callback {
    std::weak_ptr<MonoState> mono_state;
    CallbackFunc func;
    Mono_Handle handle;
  };

  typedef std::vector<Callback> Queue;

  Queue queue_;
};

}  // namespace uiwidgets
