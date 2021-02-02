

#ifndef UIWIDGETS_SHELL_PLATFORM_COMMON_CPP_CLIENT_WRAPPER_INCLUDE_UIWIDGETS_STANDARD_METHOD_CODEC_H_
#define UIWIDGETS_SHELL_PLATFORM_COMMON_CPP_CLIENT_WRAPPER_INCLUDE_UIWIDGETS_STANDARD_METHOD_CODEC_H_

#include "encodable_value.h"
#include "method_call.h"
#include "method_codec.h"

namespace uiwidgets {

class StandardMethodCodec : public MethodCodec<EncodableValue> {
 public:
  static const StandardMethodCodec& GetInstance();

  ~StandardMethodCodec() = default;

  StandardMethodCodec(StandardMethodCodec const&) = delete;
  StandardMethodCodec& operator=(StandardMethodCodec const&) = delete;

 protected:
  StandardMethodCodec() = default;

  std::unique_ptr<MethodCall<EncodableValue>> DecodeMethodCallInternal(
      const uint8_t* message, const size_t message_size) const override;

  std::unique_ptr<std::vector<uint8_t>> EncodeMethodCallInternal(
      const MethodCall<EncodableValue>& method_call) const override;

  std::unique_ptr<std::vector<uint8_t>> EncodeSuccessEnvelopeInternal(
      const EncodableValue* result) const override;

  std::unique_ptr<std::vector<uint8_t>> EncodeErrorEnvelopeInternal(
      const std::string& error_code, const std::string& error_message,
      const EncodableValue* error_details) const override;
};

}  // namespace uiwidgets

#endif
