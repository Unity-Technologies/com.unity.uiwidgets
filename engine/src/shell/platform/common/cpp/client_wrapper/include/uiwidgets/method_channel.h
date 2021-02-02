

#ifndef UIWIDGETS_SHELL_PLATFORM_COMMON_CPP_CLIENT_WRAPPER_INCLUDE_UIWIDGETS_METHOD_CHANNEL_H_
#define UIWIDGETS_SHELL_PLATFORM_COMMON_CPP_CLIENT_WRAPPER_INCLUDE_UIWIDGETS_METHOD_CHANNEL_H_

#include <iostream>
#include <string>

#include "binary_messenger.h"
#include "engine_method_result.h"
#include "method_call.h"
#include "method_codec.h"
#include "method_result.h"

namespace uiwidgets {

template <typename T>
using MethodCallHandler = std::function<void(
    const MethodCall<T>& call, std::unique_ptr<MethodResult<T>> result)>;

template <typename T>
class MethodChannel {
 public:
  MethodChannel(BinaryMessenger* messenger, const std::string& name,
                const MethodCodec<T>* codec)
      : messenger_(messenger), name_(name), codec_(codec) {}

  ~MethodChannel() = default;

  MethodChannel(MethodChannel const&) = delete;
  MethodChannel& operator=(MethodChannel const&) = delete;

  void InvokeMethod(const std::string& method, std::unique_ptr<T> arguments) {
    MethodCall<T> method_call(method, std::move(arguments));
    std::unique_ptr<std::vector<uint8_t>> message =
        codec_->EncodeMethodCall(method_call);
    messenger_->Send(name_, message->data(), message->size(), nullptr);
  }

  void InvokeMethod(const std::string& method, std::unique_ptr<T> arguments,
                    uiwidgets::BinaryReply reply) {
    MethodCall<T> method_call(method, std::move(arguments));
    std::unique_ptr<std::vector<uint8_t>> message =
        codec_->EncodeMethodCall(method_call);
    messenger_->Send(name_, message->data(), message->size(), reply);
  }

  void SetMethodCallHandler(MethodCallHandler<T> handler) const {
    if (!handler) {
      messenger_->SetMessageHandler(name_, nullptr);
      return;
    }
    const auto* codec = codec_;
    std::string channel_name = name_;
    BinaryMessageHandler binary_handler = [handler, codec, channel_name](
                                              const uint8_t* message,
                                              const size_t message_size,
                                              BinaryReply reply) {
      auto result =
          std::make_unique<EngineMethodResult<T>>(std::move(reply), codec);
      std::unique_ptr<MethodCall<T>> method_call =
          codec->DecodeMethodCall(message, message_size);
      if (!method_call) {
        std::cerr << "Unable to construct method call from message on channel "
                  << channel_name << std::endl;
        result->NotImplemented();
        return;
      }
      handler(*method_call, std::move(result));
    };
    messenger_->SetMessageHandler(name_, std::move(binary_handler));
  }

 private:
  BinaryMessenger* messenger_;
  std::string name_;
  const MethodCodec<T>* codec_;
};

}  // namespace uiwidgets

#endif
