

#ifndef UIWIDGETS_SHELL_PLATFORM_COMMON_CPP_CLIENT_WRAPPER_INCLUDE_UIWIDGETS_JSON_METHOD_CODEC_H_
#define UIWIDGETS_SHELL_PLATFORM_COMMON_CPP_CLIENT_WRAPPER_INCLUDE_UIWIDGETS_JSON_METHOD_CODEC_H_

#include "json_type.h"
#include "method_call.h"
#include "method_codec.h"

namespace uiwidgets {

class JsonMethodCodec : public MethodCodec<JsonValueType> {
 public:
  static const JsonMethodCodec& GetInstance();

  ~JsonMethodCodec() = default;

  JsonMethodCodec(JsonMethodCodec const&) = delete;
  JsonMethodCodec& operator=(JsonMethodCodec const&) = delete;

 protected:
  JsonMethodCodec() = default;

  std::unique_ptr<MethodCall<JsonValueType>> DecodeMethodCallInternal(
      const uint8_t* message, const size_t message_size) const override;

  std::unique_ptr<std::vector<uint8_t>> EncodeMethodCallInternal(
      const MethodCall<JsonValueType>& method_call) const override;

  std::unique_ptr<std::vector<uint8_t>> EncodeSuccessEnvelopeInternal(
      const JsonValueType* result) const override;

  std::unique_ptr<std::vector<uint8_t>> EncodeErrorEnvelopeInternal(
      const std::string& error_code, const std::string& error_message,
      const JsonValueType* error_details) const override;
};

}  // namespace uiwidgets

#endif
