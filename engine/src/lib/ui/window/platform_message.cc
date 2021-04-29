#include "platform_message.h"

#include <utility>

namespace uiwidgets {

PlatformMessage::PlatformMessage(std::string channel, std::vector<uint8_t> data,
                                 fml::RefPtr<PlatformMessageResponse> response)
    : channel_(std::move(channel)),
      data_(std::move(data)),
      hasData_(true),
      response_(std::move(response)) {}
PlatformMessage::PlatformMessage(std::string channel,
                                 fml::RefPtr<PlatformMessageResponse> response)
    : channel_(std::move(channel)),
      data_(),
      hasData_(false),
      response_(std::move(response)) {}

PlatformMessage::~PlatformMessage() = default;

}  // namespace uiwidgets
