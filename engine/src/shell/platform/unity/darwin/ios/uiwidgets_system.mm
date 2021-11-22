#include "uiwidgets_system.h"

#include <algorithm>
#include <chrono>

#include "uiwidgets_panel.h"

namespace uiwidgets {
UIWidgetsSystem g_uiwidgets_system;

UIWidgetsSystem::UIWidgetsSystem() = default;

UIWidgetsSystem::~UIWidgetsSystem() = default;

void UIWidgetsSystem::RegisterPanel(UIWidgetsPanel* panel) {
  uiwidgets_panels_.insert(panel);
}

void UIWidgetsSystem::UnregisterPanel(UIWidgetsPanel* panel) {
  uiwidgets_panels_.erase(panel);
}

void UIWidgetsSystem::Wait(std::chrono::nanoseconds max_duration) {
  Update();

  std::chrono::nanoseconds wait_duration =
      std::max(std::chrono::nanoseconds(0),
               next_uiwidgets_event_time_ - TimePoint::clock::now());

  wait_duration = std::min(max_duration, wait_duration);

  //TODO: find a proper api similar to MsgWaitForMultipleObjects on Windows
  //      which will notify os to wait for the given period of time
}

void UIWidgetsSystem::Update() {
  TimePoint next_event_time = TimePoint::max();
  for (auto* uiwidgets_panel : uiwidgets_panels_) {
    if (!uiwidgets_panel->NeedUpdateByPlayerLoop()) {
      continue;
    }
    std::chrono::nanoseconds wait_duration = uiwidgets_panel->ProcessMessages();
    if (wait_duration != std::chrono::nanoseconds::max()) {
      next_event_time =
          std::min(next_event_time, TimePoint::clock::now() + wait_duration);
    }
  }
  next_uiwidgets_event_time_ = next_event_time;
}

void UIWidgetsSystem::VSync() {
  for (auto* uiwidgets_panel : uiwidgets_panels_) {
    if (!uiwidgets_panel->NeedUpdateByPlayerLoop()) {
      continue;
    }
    uiwidgets_panel->ProcessVSync();
  }
}

void UIWidgetsSystem::WakeUp() {}

void UIWidgetsSystem::GfxWorkerCallback(int eventId, void* data) {
  const fml::closure task(std::move(gfx_worker_tasks_[eventId]));

  {
    std::scoped_lock lock(task_mutex_);
    gfx_worker_tasks_.erase(eventId);
  }

  task();
}

void UIWidgetsSystem::PostTaskToGfxWorker(const fml::closure& task) {
  {
    std::scoped_lock lock(task_mutex_);
    last_task_id_++;
    gfx_worker_tasks_[last_task_id_] = task;
  }
  unity_uiwidgets_->IssuePluginEventAndData(&_GfxWorkerCallback, last_task_id_,
                                            nullptr);
}

void UIWidgetsSystem::BindUnityInterfaces(IUnityInterfaces* unity_interfaces) {
  unity_interfaces_ = unity_interfaces;

  unity_uiwidgets_ = unity_interfaces_->Get<UnityUIWidgets::IUnityUIWidgets>();
  unity_uiwidgets_->SetUpdateCallback(_Update);
  unity_uiwidgets_->SetVSyncCallback(_VSync);
  unity_uiwidgets_->SetWaitCallback(_Wait);
  unity_uiwidgets_->SetWakeUpCallback(_WakeUp);
}

void UIWidgetsSystem::UnBindUnityInterfaces() {
  unity_uiwidgets_->SetUpdateCallback(nullptr);
  unity_uiwidgets_->SetVSyncCallback(nullptr);
  unity_uiwidgets_->SetWaitCallback(nullptr);
  unity_uiwidgets_->SetWakeUpCallback(nullptr);
  unity_uiwidgets_ = nullptr;

  UnitySurfaceManager::ReleaseResource();

  unity_interfaces_ = nullptr;
}

UIWidgetsSystem* UIWidgetsSystem::GetInstancePtr() {
  return &g_uiwidgets_system;
}
}  // namespace uiwidgets