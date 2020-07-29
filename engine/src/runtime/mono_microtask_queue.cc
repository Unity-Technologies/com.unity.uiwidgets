#include "mono_microtask_queue.h"

#include <utility>

namespace uiwidgets {

MonoMicrotaskQueue::MonoMicrotaskQueue() {}

MonoMicrotaskQueue::~MonoMicrotaskQueue() = default;

void MonoMicrotaskQueue::ScheduleMicrotask(CallbackFunc func,
                                           Mono_Handle handle) {
  queue_.emplace_back(
      Callback{MonoState::Current()->GetWeakPtr(), func, handle});
}

void MonoMicrotaskQueue::RunMicrotasks() {
  while (!queue_.empty()) {
    Queue local;
    std::swap(queue_, local);

    for (auto& callback : local) {
      if (auto mono_state = callback.mono_state.lock()) {
        MonoState::Scope mono_scope(mono_state.get());

        callback.func(callback.handle);
      }
    }
  }
}

}  // namespace uiwidgets
