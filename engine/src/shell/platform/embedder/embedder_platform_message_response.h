#pragma once

#include "flutter/fml/macros.h"
#include "flutter/fml/task_runner.h"
#include "lib/ui/window/platform_message_response.h"

namespace uiwidgets {

class EmbedderPlatformMessageResponse : public PlatformMessageResponse {
 public:
  using Callback = std::function<void(const uint8_t* data, size_t size)>;

  EmbedderPlatformMessageResponse(fml::RefPtr<fml::TaskRunner> runner,
                                  const Callback& callback);

  ~EmbedderPlatformMessageResponse() override;

 private:
  fml::RefPtr<fml::TaskRunner> runner_;
  Callback callback_;

  // |PlatformMessageResponse|
  void Complete(std::unique_ptr<fml::Mapping> data) override;

  // |PlatformMessageResponse|
  void CompleteEmpty() override;

  FML_DISALLOW_COPY_AND_ASSIGN(EmbedderPlatformMessageResponse);
};

}  // namespace uiwidgets
