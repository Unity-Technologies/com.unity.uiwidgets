#pragma once

#include <string>
#include <vector>

#include "flutter/fml/memory/ref_counted.h"
#include "flutter/fml/memory/ref_ptr.h"
#include "platform_message_response.h"

namespace uiwidgets {

class PlatformMessage : public fml::RefCountedThreadSafe<PlatformMessage> {
  FML_FRIEND_REF_COUNTED_THREAD_SAFE(PlatformMessage);
  FML_FRIEND_MAKE_REF_COUNTED(PlatformMessage);

 public:
  const std::string& channel() const { return channel_; }
  const std::vector<uint8_t>& data() const { return data_; }
  bool hasData() { return hasData_; }

  const fml::RefPtr<PlatformMessageResponse>& response() const {
    return response_;
  }

 private:
  PlatformMessage(std::string channel, std::vector<uint8_t> data,
                  fml::RefPtr<PlatformMessageResponse> response);
  PlatformMessage(std::string channel,
                  fml::RefPtr<PlatformMessageResponse> response);
  ~PlatformMessage();

  std::string channel_;
  std::vector<uint8_t> data_;
  bool hasData_;
  fml::RefPtr<PlatformMessageResponse> response_;
};

}  // namespace uiwidgets
