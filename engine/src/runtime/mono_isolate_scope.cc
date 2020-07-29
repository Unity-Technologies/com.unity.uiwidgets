#include "mono_isolate_scope.h"

#include <flutter/fml/logging.h>

#include "mono_api.h"

namespace uiwidgets {

MonoIsolateScope::MonoIsolateScope(Mono_Isolate isolate) {
  isolate_ = isolate;
  previous_ = Mono_CurrentIsolate();
  if (previous_ == isolate_) return;
  if (previous_) Mono_ExitIsolate();
  Mono_EnterIsolate(isolate_);
}

MonoIsolateScope::~MonoIsolateScope() {
  Mono_Isolate current = Mono_CurrentIsolate();
  FML_DCHECK(!current || current == isolate_);
  if (previous_ == isolate_) return;
  if (current) Mono_ExitIsolate();
  if (previous_) Mono_EnterIsolate(previous_);
}

}  // namespace uiwidgets
