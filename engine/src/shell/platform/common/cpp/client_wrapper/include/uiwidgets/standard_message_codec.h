

#ifndef UIWIDGETS_SHELL_PLATFORM_COMMON_CPP_CLIENT_WRAPPER_INCLUDE_UIWIDGETS_STANDARD_MESSAGE_CODEC_H_
#define UIWIDGETS_SHELL_PLATFORM_COMMON_CPP_CLIENT_WRAPPER_INCLUDE_UIWIDGETS_STANDARD_MESSAGE_CODEC_H_

#include "encodable_value.h"
#include "message_codec.h"

namespace uiwidgets {

class StandardMessageCodec : public MessageCodec<EncodableValue> {
 public:
  static const StandardMessageCodec& GetInstance();

  ~StandardMessageCodec();

  StandardMessageCodec(StandardMessageCodec const&) = delete;
  StandardMessageCodec& operator=(StandardMessageCodec const&) = delete;

 protected:
  StandardMessageCodec();

  std::unique_ptr<EncodableValue> DecodeMessageInternal(
      const uint8_t* binary_message, const size_t message_size) const override;

  std::unique_ptr<std::vector<uint8_t>> EncodeMessageInternal(
      const EncodableValue& message) const override;
};

}  // namespace uiwidgets

#endif
