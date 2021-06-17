#include "mono_state.h"

namespace uiwidgets {

MonoState::Scope::Scope(MonoState* mono_state)
    : scope_(mono_state->isolate()) {}

MonoState::Scope::Scope(std::shared_ptr<MonoState> mono_state)
    : scope_(mono_state->isolate()) {}

MonoState::Scope::~Scope() {}

MonoState::MonoState() : isolate_(nullptr) {}

MonoState::~MonoState() {}

void MonoState::SetIsolate(Mono_Isolate isolate) { isolate_ = isolate; }

MonoState* MonoState::From(Mono_Isolate isolate) {
  auto isolate_data =
      static_cast<std::shared_ptr<MonoState>*>(Mono_IsolateData(isolate));
  return isolate_data->get();
}

bool MonoState::EnsureCurrentIsolate() {
  return Mono_CurrentIsolate() != nullptr;
}

MonoState* MonoState::Current() {
  auto isolate_data =
      static_cast<std::shared_ptr<MonoState>*>(Mono_CurrentIsolateData());
  return isolate_data->get();
}

std::weak_ptr<MonoState> MonoState::GetWeakPtr() { return shared_from_this(); }

}  // namespace uiwidgets
