#pragma once

#include <memory>

namespace uiwidgets {

typedef void* Mono_Handle;

class _Mono_Isolate final {
 public:
  _Mono_Isolate(void* data) : data_(data) {}
  ~_Mono_Isolate();

  void* data() const { return data_; }

 private:
  void* data_;
};

typedef _Mono_Isolate* Mono_Isolate;

Mono_Isolate Mono_CreateIsolate(void* isolate_data);
Mono_Isolate Mono_CurrentIsolate();
void Mono_EnterIsolate(Mono_Isolate isolate);
void Mono_ExitIsolate();
void Mono_ShutdownIsolate();

void* Mono_IsolateData(Mono_Isolate isolate);
void* Mono_CurrentIsolateData();

void Mono_ThrowException(const char* exception);
	
}  // namespace uiwidgets
