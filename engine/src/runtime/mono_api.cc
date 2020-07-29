#include "mono_api.h"

#include <flutter/fml/logging.h>
#include <flutter/fml/thread_local.h>

#include "mono_state.h"

namespace uiwidgets {

FML_THREAD_LOCAL fml::ThreadLocalUniquePtr<_Mono_Isolate> tls_isolate;

Mono_Isolate Mono_CreateIsolate(void* isolate_data) {
  return new _Mono_Isolate{isolate_data};
}

_Mono_Isolate::~_Mono_Isolate() {
  if (data_) {
    delete static_cast<MonoState*>(data_);
  }
}

Mono_Isolate Mono_CurrentIsolate() { return tls_isolate.get(); }

void Mono_EnterIsolate(Mono_Isolate isolate) {
  FML_DCHECK(tls_isolate.get() == nullptr);
  tls_isolate.reset(isolate);
}

void Mono_ExitIsolate() {
  FML_DCHECK(tls_isolate.get() != nullptr);
  tls_isolate.reset(nullptr);
}

void Mono_ShutdownIsolate() {
  Mono_Isolate current = tls_isolate.get();
  FML_DCHECK(current != nullptr);
  tls_isolate.reset(nullptr);
  delete current;
}

void* Mono_IsolateData(Mono_Isolate isolate) {
  FML_DCHECK(isolate != nullptr && isolate->data() != nullptr);
  return isolate->data();
}

void* Mono_CurrentIsolateData() {
  return Mono_IsolateData(Mono_CurrentIsolate());
}

void Mono_ThrowException(const char* exception) {}

}  // namespace uiwidgets