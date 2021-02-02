// Copyright 2013 The UIWidgets Authors. All rights reserved.
// Use of this source code is governed by a BSD-style license that can be
// found in the LICENSE file.

#include "include/uiwidgets/plugin_registrar.h"

#include <iostream>
#include <map>

#include "include/uiwidgets/engine_method_result.h"
#include "include/uiwidgets/method_channel.h"

namespace uiwidgets {

namespace {

// Passes |message| to |user_data|, which must be a BinaryMessageHandler, along
// with a BinaryReply that will send a response on |message|'s response handle.
//
// This serves as an adaptor between the function-pointer-based message callback
// interface provided by the C API and the std::function-based message handler
// interface of BinaryMessenger.
void ForwardToHandler(UIWidgetsDesktopMessengerRef messenger,
                      const UIWidgetsDesktopMessage* message,
                      void* user_data) {
  auto* response_handle = message->response_handle;
  BinaryReply reply_handler = [messenger, response_handle](
                                  const uint8_t* reply,
                                  const size_t reply_size) mutable {
    if (!response_handle) {
      std::cerr << "Error: Response can be set only once. Ignoring "
                   "duplicate response."
                << std::endl;
      return;
    }
    UIWidgetsDesktopMessengerSendResponse(messenger, response_handle, reply,
                                        reply_size);
    // The engine frees the response handle once
    // UIWidgetsDesktopSendMessageResponse is called.
    response_handle = nullptr;
  };

  const BinaryMessageHandler& message_handler =
      *static_cast<BinaryMessageHandler*>(user_data);

  message_handler(message->message, message->message_size,
                  std::move(reply_handler));
}

}  // namespace

// Wrapper around a UIWidgetsDesktopMessengerRef that implements the
// BinaryMessenger API.
class BinaryMessengerImpl : public BinaryMessenger {
 public:
  explicit BinaryMessengerImpl(UIWidgetsDesktopMessengerRef core_messenger)
      : messenger_(core_messenger) {}

  virtual ~BinaryMessengerImpl() = default;

  // Prevent copying.
  BinaryMessengerImpl(BinaryMessengerImpl const&) = delete;
  BinaryMessengerImpl& operator=(BinaryMessengerImpl const&) = delete;

  // |uiwidgets::BinaryMessenger|
  void Send(const std::string& channel,
            const uint8_t* message,
            const size_t message_size) const override;

  // |uiwidgets::BinaryMessenger|
  void Send(const std::string& channel,
            const uint8_t* message,
            const size_t message_size,
            BinaryReply reply) const override;

  // |uiwidgets::BinaryMessenger|
  void SetMessageHandler(const std::string& channel,
                         BinaryMessageHandler handler) override;

 private:
  // Handle for interacting with the C API.
  UIWidgetsDesktopMessengerRef messenger_;

  // A map from channel names to the BinaryMessageHandler that should be called
  // for incoming messages on that channel.
  std::map<std::string, BinaryMessageHandler> handlers_;
};

void BinaryMessengerImpl::Send(const std::string& channel,
                               const uint8_t* message,
                               const size_t message_size) const {
  UIWidgetsDesktopMessengerSend(messenger_, channel.c_str(), message,
                              message_size);
}

void BinaryMessengerImpl::Send(const std::string& channel,
                               const uint8_t* message,
                               const size_t message_size,
                               BinaryReply reply) const {
  if (reply == nullptr) {
    UIWidgetsDesktopMessengerSend(messenger_, channel.c_str(), message,
                                message_size);
    return;
  }
  struct Captures {
    BinaryReply reply;
  };
  auto captures = new Captures();
  captures->reply = reply;

  auto message_reply = [](const uint8_t* data, size_t data_size,
                          void* user_data) {
    auto captures = reinterpret_cast<Captures*>(user_data);
    captures->reply(data, data_size);
    delete captures;
  };
  bool result = UIWidgetsDesktopMessengerSendWithReply(
      messenger_, channel.c_str(), message, message_size, message_reply,
      captures);
  if (!result) {
    delete captures;
  }
}

void BinaryMessengerImpl::SetMessageHandler(const std::string& channel,
                                            BinaryMessageHandler handler) {
  if (!handler) {
    handlers_.erase(channel);
    UIWidgetsDesktopMessengerSetCallback(messenger_, channel.c_str(), nullptr,
                                       nullptr);
    return;
  }
  // Save the handler, to keep it alive.
  handlers_[channel] = std::move(handler);
  BinaryMessageHandler* message_handler = &handlers_[channel];
  // Set an adaptor callback that will invoke the handler.
  UIWidgetsDesktopMessengerSetCallback(messenger_, channel.c_str(),
                                     ForwardToHandler, message_handler);
}

// PluginRegistrar:

PluginRegistrar::PluginRegistrar(UIWidgetsDesktopPluginRegistrarRef registrar)
    : registrar_(registrar) {
  auto core_messenger = UIWidgetsDesktopRegistrarGetMessenger(registrar_);
  messenger_ = std::make_unique<BinaryMessengerImpl>(core_messenger);
}

PluginRegistrar::~PluginRegistrar() {}

void PluginRegistrar::AddPlugin(std::unique_ptr<Plugin> plugin) {
  plugins_.insert(std::move(plugin));
}

void PluginRegistrar::EnableInputBlockingForChannel(
    const std::string& channel) {
  UIWidgetsDesktopRegistrarEnableInputBlocking(registrar_, channel.c_str());
}

}  // namespace uiwidgets
