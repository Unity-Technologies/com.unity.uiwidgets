#include "mono_api.h"

#include <flutter/fml/logging.h>
#include <flutter/fml/thread_local.h>

#include "mono_isolate.h"
#include "mono_state.h"

#include "shell/platform/unity/uiwidgets_system.h"

#ifdef __APPLE__
//https://stackoverflow.com/questions/28188258/how-do-i-get-the-current-pthread-id/28212486
const uint64_t GetCurrentThreadId() { 
  uint64_t tid;
  pthread_threadid_np(NULL, &tid);
  return tid;
}
#elif __ANDROID__
uint64_t GetCurrentThreadId() { 
  return gettid();
}
#endif
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
  Mono_Shutdown(current);

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

typedef void (*Mono_ShutdownCallback)(Mono_Isolate isolate);

static Mono_ShutdownCallback Mono_ShutdownCallback_;

void Mono_ThrowException(const char* exception) {
  Mono_ThrowExceptionCallback_(exception);
}

void Mono_Shutdown(Mono_Isolate isolate) { Mono_ShutdownCallback_(isolate); }

int64_t Mono_TimelineGetMicros() {
  return fml::TimePoint::Now().ToEpochDelta().ToMicroseconds();
}

void Mono_NotifyIdle(int64_t deadline) {}

UIWIDGETS_API(void)
Mono_hook(Mono_ThrowExceptionCallback throwException,
          Mono_ShutdownCallback shutdown) {
  Mono_ThrowExceptionCallback_ = throwException;
  Mono_ShutdownCallback_ = shutdown;
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

inline const char* TimelineEventToString(Dart_Timeline_Event_Type type) {
  switch (type) {
    case Dart_Timeline_Event_Begin:
      return "Begin";
    case Dart_Timeline_Event_End:
      return "End";
    case Dart_Timeline_Event_Instant:
      return "Instant";
    case Dart_Timeline_Event_Async_Begin:
      return "AsyncBegin";
    case Dart_Timeline_Event_Async_End:
      return "AsyncEnd";
    case Dart_Timeline_Event_Async_Instant:
      return "AsyncInstant";
    case Dart_Timeline_Event_Counter:
      return "Counter";
    case Dart_Timeline_Event_Flow_Begin:
      return "FlowBegin";
    case Dart_Timeline_Event_Flow_Step:
      return "FlowStep";
    case Dart_Timeline_Event_Flow_End:
      return "FlowEnd";
    default:
      return "";
  }
}

extern "C" void Dart_TimelineEvent(const char* label, int64_t timestamp0,
                                   int64_t timestamp1_or_async_id,
                                   Dart_Timeline_Event_Type type,
                                   intptr_t argument_count,
                                   const char** argument_names,
                                   const char** argument_values) {
  static int64_t timestamp_begin = timestamp0;

  if (timestamp1_or_async_id) {
    uiwidgets::UIWidgetsSystem::GetInstancePtr()->printf_console(
        "uiwidgets Timeline [Thread:%d] [%lld ms] [%lld] [%s]: %s\n",
        GetCurrentThreadId(), (timestamp0 - timestamp_begin) / 1000,
        timestamp1_or_async_id, TimelineEventToString(type), label);
  } else {
    uiwidgets::UIWidgetsSystem::GetInstancePtr()->printf_console(
        "uiwidgets Timeline [Thread:%d] [%d ms] [%s]: %s\n",
        GetCurrentThreadId(), (timestamp0 - timestamp_begin) / 1000,
        TimelineEventToString(type), label);
  }
}
