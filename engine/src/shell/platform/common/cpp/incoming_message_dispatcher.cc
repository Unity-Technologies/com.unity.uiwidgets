// Copyright 2013 The UIWidgets Authors. All rights reserved.
// Use of this source code is governed by a BSD-style license that can be
// found in the LICENSE file.

#include "shell/platform/common/cpp/incoming_message_dispatcher.h"

namespace uiwidgets {

IncomingMessageDispatcher::IncomingMessageDispatcher(
    UIWidgetsDesktopMessengerRef messenger)
    : messenger_(messenger) {}

IncomingMessageDispatcher::~IncomingMessageDispatcher() = default;

/// @note Procedure doesn't copy all closures.
void IncomingMessageDispatcher::HandleMessage(
    const UIWidgetsDesktopMessage& message,
    const std::function<void(void)>& input_block_cb,
    const std::function<void(void)>& input_unblock_cb) {
  std::string channel(message.channel);

  // Find the handler for the channel; if there isn't one, report the failure.
  if (callbacks_.find(channel) == callbacks_.end()) {
    UIWidgetsDesktopMessengerSendResponse(messenger_, message.response_handle,
                                        nullptr, 0);
    return;
  }
  auto& callback_info = callbacks_[channel];
  UIWidgetsDesktopMessageCallback message_callback = callback_info.first;

  // Process the call, handling input blocking if requested.
  bool block_input = input_blocking_channels_.count(channel) > 0;
  if (block_input) {
    input_block_cb();
  }
  message_callback(messenger_, &message, callback_info.second);
  if (block_input) {
    input_unblock_cb();
  }
}

void IncomingMessageDispatcher::SetMessageCallback(
    const std::string& channel,
    UIWidgetsDesktopMessageCallback callback,
    void* user_data) {
  if (!callback) {
    callbacks_.erase(channel);
    return;
  }
  callbacks_[channel] = std::make_pair(callback, user_data);
}

void IncomingMessageDispatcher::EnableInputBlockingForChannel(
    const std::string& channel) {
  input_blocking_channels_.insert(channel);
}

}  // namespace uiwidgets
