#pragma once

#include "flutter/fml/message_loop.h"
#include "platform_message_response.h"
#include "runtime/mono_state.h"

namespace uiwidgets {

class PlatformMessageResponseMono : public PlatformMessageResponse {
  FML_FRIEND_MAKE_REF_COUNTED(PlatformMessageResponseMono);

 public:
  void Complete(std::unique_ptr<fml::Mapping> data) override;
  void CompleteEmpty() override;

  typedef void (*PlatformMessageCallback)(Mono_Handle callback_handle,
                                          const uint8_t* data, int data_length);

 protected:
  explicit PlatformMessageResponseMono(
      std::weak_ptr<MonoState> mono_state_weak,
      PlatformMessageCallback callback, Mono_Handle handle,
      fml::RefPtr<fml::TaskRunner> ui_task_runner);
  ~PlatformMessageResponseMono() override;

  std::weak_ptr<MonoState> mono_state_weak_;
  PlatformMessageCallback callback_;
  Mono_Handle handle_;

  fml::RefPtr<fml::TaskRunner> ui_task_runner_;
};

}  // namespace uiwidgets
