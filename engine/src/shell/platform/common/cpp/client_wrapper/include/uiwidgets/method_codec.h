

#ifndef UIWIDGETS_SHELL_PLATFORM_COMMON_CPP_CLIENT_WRAPPER_INCLUDE_UIWIDGETS_METHOD_CODEC_H_
#define UIWIDGETS_SHELL_PLATFORM_COMMON_CPP_CLIENT_WRAPPER_INCLUDE_UIWIDGETS_METHOD_CODEC_H_

#include <memory>
#include <string>
#include <vector>

#include "method_call.h"

namespace uiwidgets {

template <typename T>
class MethodCodec {
 public:
  MethodCodec() = default;

  virtual ~MethodCodec() = default;

  MethodCodec(MethodCodec<T> const&) = delete;
  MethodCodec& operator=(MethodCodec<T> const&) = delete;

  std::unique_ptr<MethodCall<T>> DecodeMethodCall(
      const uint8_t* message, const size_t message_size) const {
    return std::move(DecodeMethodCallInternal(message, message_size));
  }

  std::unique_ptr<MethodCall<T>> DecodeMethodCall(
      const std::vector<uint8_t>& message) const {
    size_t size = message.size();
    const uint8_t* data = size > 0 ? &message[0] : nullptr;
    return std::move(DecodeMethodCallInternal(data, size));
  }

  std::unique_ptr<std::vector<uint8_t>> EncodeMethodCall(
      const MethodCall<T>& method_call) const {
    return std::move(EncodeMethodCallInternal(method_call));
  }

  std::unique_ptr<std::vector<uint8_t>> EncodeSuccessEnvelope(
      const T* result = nullptr) const {
    return std::move(EncodeSuccessEnvelopeInternal(result));
  }

  std::unique_ptr<std::vector<uint8_t>> EncodeErrorEnvelope(
      const std::string& error_code, const std::string& error_message = "",
      const T* error_details = nullptr) const {
    return std::move(
        EncodeErrorEnvelopeInternal(error_code, error_message, error_details));
  }

 protected:
  virtual std::unique_ptr<MethodCall<T>> DecodeMethodCallInternal(
      const uint8_t* message, const size_t message_size) const = 0;

  virtual std::unique_ptr<std::vector<uint8_t>> EncodeMethodCallInternal(
      const MethodCall<T>& method_call) const = 0;

  virtual std::unique_ptr<std::vector<uint8_t>> EncodeSuccessEnvelopeInternal(
      const T* result) const = 0;

  virtual std::unique_ptr<std::vector<uint8_t>> EncodeErrorEnvelopeInternal(
      const std::string& error_code, const std::string& error_message,
      const T* error_details) const = 0;
};

}  // namespace uiwidgets

#endif
