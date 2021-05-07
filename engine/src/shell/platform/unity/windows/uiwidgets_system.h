#pragma once

#include <flutter/fml/closure.h>

#include <chrono>
#include <cstdarg>
#include <set>
#include <unordered_map>
#include <mutex>

#include "Unity/IUnityInterface.h"
#include "Unity/IUnityUIWidgets.h"
#include "flutter/fml/macros.h"
#include "runtime/mono_api.h"

namespace uiwidgets {

using TimePoint = std::chrono::steady_clock::time_point;

class UIWidgetsPanel;

class UIWidgetsSystem {
 public:
  UIWidgetsSystem();
  ~UIWidgetsSystem();

  void RegisterPanel(UIWidgetsPanel* panel);
  void UnregisterPanel(UIWidgetsPanel* panel);

  void PostTaskToGfxWorker(const fml::closure& task);
  void printf_console(const char* log, ...) {
    /*va_list vl;
    va_start(vl, log);
    unity_uiwidgets_->printf_consolev(log, vl);
    va_end(vl);*/
  }

  void BindUnityInterfaces(IUnityInterfaces* unity_interfaces);
  void UnBindUnityInterfaces();
  IUnityInterfaces* GetUnityInterfaces() { return unity_interfaces_; }

  static UIWidgetsSystem* GetInstancePtr();

  FML_DISALLOW_COPY_AND_ASSIGN(UIWidgetsSystem);

 private:
  UIWIDGETS_CALLBACK(void) _Update() { GetInstancePtr()->Update(); }

  UIWIDGETS_CALLBACK(void) _Wait(long max_duration) {
    GetInstancePtr()->Wait(std::chrono::nanoseconds(max_duration));
  }

  UIWIDGETS_CALLBACK(void) _VSync() { GetInstancePtr()->VSync(); }

  UIWIDGETS_CALLBACK(void) _WakeUp() { GetInstancePtr()->WakeUp(); }

  UIWIDGETS_CALLBACK(void) _GfxWorkerCallback(int eventId, void* data) {
    GetInstancePtr()->GfxWorkerCallback(eventId, data);
  }

  void Update();
  void Wait(std::chrono::nanoseconds max_duration);
  void VSync();
  void WakeUp();
  void GfxWorkerCallback(int eventId, void* data);

  IUnityInterfaces* unity_interfaces_ = nullptr;
  UnityUIWidgets::IUnityUIWidgets* unity_uiwidgets_ = nullptr;

  std::unordered_map<int, fml::closure> gfx_worker_tasks_;
  int last_task_id_ = 0;

  TimePoint next_uiwidgets_event_time_ = TimePoint::clock::now();
  std::set<UIWidgetsPanel*> uiwidgets_panels_;

  std::mutex task_mutex_;
};

}  // namespace uiwidgets
