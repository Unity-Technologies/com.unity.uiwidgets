#include "platform_message_response_mono.h"

#include <utility>

#include "common/task_runners.h"
#include "flutter/fml/make_copyable.h"
#include "lib/ui/window/window.h"

namespace uiwidgets {

PlatformMessageResponseMono::PlatformMessageResponseMono(
    std::weak_ptr<MonoState> mono_state_weak, PlatformMessageCallback callback,
    Mono_Handle handle, fml::RefPtr<fml::TaskRunner> ui_task_runner)
    : mono_state_weak_(mono_state_weak),
      callback_(std::move(callback)),
      handle_(handle),
      ui_task_runner_(std::move(ui_task_runner)) {}

PlatformMessageResponseMono::~PlatformMessageResponseMono() {}

void PlatformMessageResponseMono::Complete(std::unique_ptr<fml::Mapping> data) {
  if (callback_ == nullptr) return;
  FML_DCHECK(!is_complete_);
  is_complete_ = true;
  ui_task_runner_->PostTask(fml::MakeCopyable(
      [mono_state_weak = mono_state_weak_, callback = callback_,
       handle = handle_, data = std::move(data)]() {
        const std::shared_ptr<MonoState> mono_state = mono_state_weak.lock();
        if (!mono_state) {
          callback(handle, nullptr, 0);
          return;
        }
      	
        MonoState::Scope scope(mono_state);
        callback(handle, data->GetMapping(), static_cast<int>(data->GetSize()));
      }));
}

void PlatformMessageResponseMono::CompleteEmpty() {
  if (callback_ == nullptr) return;
  FML_DCHECK(!is_complete_);
  is_complete_ = true;
  ui_task_runner_->PostTask([mono_state_weak = mono_state_weak_,
                             callback = callback_, handle = handle_]() {
    const std::shared_ptr<MonoState> mono_state = mono_state_weak.lock();
    if (!mono_state) {
      callback(handle, nullptr, 0);
      return;
    }
  	
    MonoState::Scope scope(mono_state);
    callback(handle, nullptr, 0);
  });
}

}  // namespace uiwidgets
