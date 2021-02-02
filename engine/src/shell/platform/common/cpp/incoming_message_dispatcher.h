// Copyright 2013 The UIWidgets Authors. All rights reserved.
// Use of this source code is governed by a BSD-style license that can be
// found in the LICENSE file.

#ifndef UIWIDGETS_SHELL_PLATFORM_CPP_INCOMING_MESSAGE_DISPATCHER_H_
#define UIWIDGETS_SHELL_PLATFORM_CPP_INCOMING_MESSAGE_DISPATCHER_H_

#include <functional>
#include <map>
#include <set>
#include <string>
#include <utility>

#include "shell/platform/common/cpp/public/uiwidgets_messenger.h"

namespace uiwidgets {

// Manages per-channel registration of callbacks for handling messages from the
// UIWidgets engine, and dispatching incoming messages to those handlers.
class IncomingMessageDispatcher {
 public:
  // Creates a new IncomingMessageDispatcher. |messenger| must remain valid as
  // long as this object exists.
  explicit IncomingMessageDispatcher(UIWidgetsDesktopMessengerRef messenger);

  virtual ~IncomingMessageDispatcher();

  // Prevent copying.
  IncomingMessageDispatcher(IncomingMessageDispatcher const&) = delete;
  IncomingMessageDispatcher& operator=(IncomingMessageDispatcher const&) =
      delete;

  // Routes |message| to to the registered handler for its channel, if any.
  //
  // If input blocking has been enabled on that channel, wraps the call to the
  // handler with calls to the given callbacks to block and then unblock input.
  //
  // If no handler is registered for the message's channel, sends a
  // NotImplemented response to the engine.
  void HandleMessage(
      const UIWidgetsDesktopMessage& message,
      const std::function<void(void)>& input_block_cb = [] {},
      const std::function<void(void)>& input_unblock_cb = [] {});

  // Registers a message callback for incoming messages from the UIWidgets
  // side on the specified channel. |callback| will be called with the message
  // and |user_data| any time a message arrives on that channel.
  //
  // Replaces any existing callback. Pass a null callback to unregister the
  // existing callback.
  void SetMessageCallback(const std::string& channel,
                          UIWidgetsDesktopMessageCallback callback,
                          void* user_data);

  // Enables input blocking on the given channel name.
  //
  // If set, then the parent window should disable input callbacks
  // while waiting for the handler for messages on that channel to run.
  void EnableInputBlockingForChannel(const std::string& channel);

 private:
  // Handle for interacting with the C messaging API.
  UIWidgetsDesktopMessengerRef messenger_;

  // A map from channel names to the UIWidgetsDesktopMessageCallback that should
  // be called for incoming messages on that channel, along with the void* user
  // data to pass to it.
  std::map<std::string, std::pair<UIWidgetsDesktopMessageCallback, void*>>
      callbacks_;

  // Channel names for which input blocking should be enabled during the call to
  // that channel's handler.
  std::set<std::string> input_blocking_channels_;
};

}  // namespace uiwidgets

#endif  // UIWIDGETS_SHELL_PLATFORM_CPP_INCOMING_MESSAGE_DISPATCHER_H_
