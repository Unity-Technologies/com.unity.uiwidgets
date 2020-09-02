#include "mono_api.h"

#include <flutter/fml/logging.h>
#include <flutter/fml/thread_local.h>

#include "mono_isolate.h"
#include "mono_state.h"

namespace uiwidgets {

FML_THREAD_LOCAL fml::ThreadLocalUniquePtr<Mono_Isolate> tls_isolate;

Mono_Isolate Mono_CreateIsolate(void* isolate_data) {
  return new _Mono_Isolate(isolate_data);
}

_Mono_Isolate::~_Mono_Isolate() {
  if (data_) {
    delete static_cast<std::shared_ptr<MonoIsolate>*>(data_);
  }
}

Mono_Isolate Mono_CurrentIsolate() {
  Mono_Isolate* ptr = tls_isolate.get();
  if (!ptr) {
    return nullptr;
  }
  return *ptr;
}

void Mono_EnterIsolate(Mono_Isolate isolate) {
  Mono_Isolate* ptr = tls_isolate.get();
  FML_DCHECK(ptr == nullptr || *ptr == nullptr);
  if (!ptr) {
    ptr = new Mono_Isolate();
    tls_isolate.reset(ptr);
  }
  *ptr = isolate;
}

void Mono_ExitIsolate() {
  Mono_Isolate* ptr = tls_isolate.get();
  FML_DCHECK(ptr != nullptr && *ptr != nullptr);
  *ptr = nullptr;
}

void Mono_ShutdownIsolate() {
  Mono_Isolate* ptr = tls_isolate.get();
  FML_DCHECK(ptr != nullptr);

  const Mono_Isolate current = *ptr;
  FML_DCHECK(current != nullptr);

  *ptr = nullptr;
  delete current;
}

void* Mono_IsolateData(Mono_Isolate isolate) {
  FML_DCHECK(isolate != nullptr && isolate->data() != nullptr);
  return isolate->data();
}

void* Mono_CurrentIsolateData() {
  return Mono_IsolateData(Mono_CurrentIsolate());
}

typedef void (*Mono_ThrowExceptionCallback)(const char* exception);

static Mono_ThrowExceptionCallback Mono_ThrowExceptionCallback_;

void Mono_ThrowException(const char* exception) {
  Mono_ThrowExceptionCallback_(exception);
}

int64_t Mono_TimelineGetMicros() {
  return fml::TimePoint::Now().ToEpochDelta().ToMicroseconds();
}

void Mono_NotifyIdle(int64_t deadline) {}

UIWIDGETS_API(void)
Mono_hook(Mono_ThrowExceptionCallback throwException) {
  Mono_ThrowExceptionCallback_ = throwException;
}

UIWIDGETS_API(Mono_Isolate)
Isolate_current() { return Mono_CurrentIsolate(); }

UIWIDGETS_API(void)
Isolate_enter(Mono_Isolate isolate) { return Mono_EnterIsolate(isolate); }

UIWIDGETS_API(void)
Isolate_exit() { Mono_ExitIsolate(); }

}  // namespace uiwidgets

extern "C" int64_t Dart_TimelineGetMicros() {
  return uiwidgets::Mono_TimelineGetMicros();
}
