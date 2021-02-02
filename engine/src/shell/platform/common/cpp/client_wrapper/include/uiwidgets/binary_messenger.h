

#ifndef UIWIDGETS_SHELL_PLATFORM_COMMON_CPP_CLIENT_WRAPPER_INCLUDE_UIWIDGETS_BINARY_MESSENGER_H_
#define UIWIDGETS_SHELL_PLATFORM_COMMON_CPP_CLIENT_WRAPPER_INCLUDE_UIWIDGETS_BINARY_MESSENGER_H_

#include <functional>
#include <string>

namespace uiwidgets {

typedef std::function<void(const uint8_t* reply, const size_t reply_size)>
    BinaryReply;

typedef std::function<void(const uint8_t* message, const size_t message_size,
                           BinaryReply reply)>
    BinaryMessageHandler;

class BinaryMessenger {
 public:
  virtual ~BinaryMessenger() = default;

  virtual void Send(const std::string& channel, const uint8_t* message,
                    const size_t message_size) const = 0;

  virtual void Send(const std::string& channel, const uint8_t* message,
                    const size_t message_size, BinaryReply reply) const = 0;

  virtual void SetMessageHandler(const std::string& channel,
                                 BinaryMessageHandler handler) = 0;
};

}  // namespace uiwidgets

#endif
