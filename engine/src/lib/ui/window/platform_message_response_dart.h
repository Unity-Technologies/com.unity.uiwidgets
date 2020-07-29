#pragma once

#include "flutter/fml/message_loop.h"
#include "platform_message_response.h"

namespace uiwidgets {

class PlatformMessageResponseDart : public PlatformMessageResponse {
  FML_FRIEND_MAKE_REF_COUNTED(PlatformMessageResponseDart);

 public:
  // Callable on any thread.
  void Complete(std::unique_ptr<fml::Mapping> data) override;
  void CompleteEmpty() override;

 protected:
  explicit PlatformMessageResponseDart(
      tonic::DartPersistentValue callback,
      fml::RefPtr<fml::TaskRunner> ui_task_runner);
  ~PlatformMessageResponseDart() override;

  tonic::DartPersistentValue callback_;
  fml::RefPtr<fml::TaskRunner> ui_task_runner_;
};

}  // namespace uiwidgets
