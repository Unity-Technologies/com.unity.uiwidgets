

#ifndef UIWIDGETS_SHELL_PLATFORM_COMMON_CPP_CLIENT_WRAPPER_INCLUDE_UIWIDGETS_MESSAGE_CODEC_H_
#define UIWIDGETS_SHELL_PLATFORM_COMMON_CPP_CLIENT_WRAPPER_INCLUDE_UIWIDGETS_MESSAGE_CODEC_H_

#include <memory>
#include <string>
#include <vector>

namespace uiwidgets {

template <typename T>
class MessageCodec {
 public:
  MessageCodec() = default;

  virtual ~MessageCodec() = default;

  MessageCodec(MessageCodec<T> const&) = delete;
  MessageCodec& operator=(MessageCodec<T> const&) = delete;

  std::unique_ptr<T> DecodeMessage(const uint8_t* binary_message,
                                   const size_t message_size) const {
    return std::move(DecodeMessageInternal(binary_message, message_size));
  }

  std::unique_ptr<T> DecodeMessage(
      const std::vector<uint8_t>& binary_message) const {
    size_t size = binary_message.size();
    const uint8_t* data = size > 0 ? &binary_message[0] : nullptr;
    return std::move(DecodeMessageInternal(data, size));
  }

  std::unique_ptr<std::vector<uint8_t>> EncodeMessage(const T& message) const {
    return std::move(EncodeMessageInternal(message));
  }

 protected:
  virtual std::unique_ptr<T> DecodeMessageInternal(
      const uint8_t* binary_message, const size_t message_size) const = 0;

  virtual std::unique_ptr<std::vector<uint8_t>> EncodeMessageInternal(
      const T& message) const = 0;
};

}  // namespace uiwidgets

#endif
