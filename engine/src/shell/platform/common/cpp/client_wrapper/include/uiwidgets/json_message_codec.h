

#ifndef UIWIDGETS_SHELL_PLATFORM_COMMON_CPP_CLIENT_WRAPPER_INCLUDE_UIWIDGETS_JSON_MESSAGE_CODEC_H_
#define UIWIDGETS_SHELL_PLATFORM_COMMON_CPP_CLIENT_WRAPPER_INCLUDE_UIWIDGETS_JSON_MESSAGE_CODEC_H_

#include "json_type.h"
#include "message_codec.h"

namespace uiwidgets {

class JsonMessageCodec : public MessageCodec<JsonValueType> {
 public:
  static const JsonMessageCodec& GetInstance();

  ~JsonMessageCodec() = default;

  JsonMessageCodec(JsonMessageCodec const&) = delete;
  JsonMessageCodec& operator=(JsonMessageCodec const&) = delete;

 protected:
  JsonMessageCodec() = default;

  std::unique_ptr<JsonValueType> DecodeMessageInternal(
      const uint8_t* binary_message, const size_t message_size) const override;

  std::unique_ptr<std::vector<uint8_t>> EncodeMessageInternal(
      const JsonValueType& message) const override;
};

}  // namespace uiwidgets

#endif
