// Copyright 2013 The UIWidgets Authors. All rights reserved.
// Use of this source code is governed by a BSD-style license that can be
// found in the LICENSE file.

#ifndef UIWIDGETS_SHELL_PLATFORM_WINDOWS_UIWIDGETS_WINDOW_STATE_H_
#define UIWIDGETS_SHELL_PLATFORM_WINDOWS_UIWIDGETS_WINDOW_STATE_H_

#include "shell/platform/common/cpp/client_wrapper/include/uiwidgets/plugin_registrar.h"
#include "shell/platform/common/cpp/incoming_message_dispatcher.h"
#include "shell/platform/embedder/embedder.h"
//#include "shell/platform/unity/windows/key_event_handler.h"
//#include "shell/platform/unity/windows/keyboard_hook_handler.h"
//#include "shell/platform/unity/windows/platform_handler.h"
#include "shell/platform/unity/windows/text_input_plugin.h"
#include "shell/platform/unity/windows/win32_task_runner.h"

namespace uiwidgets {
struct Win32UIWidgetsWindow;
}

// Struct for storing state within an instance of the windows native (HWND or
// CoreWindow) Window.
struct UIWidgetsDesktopViewControllerState {
  // The win32 window that owns this state object.
  std::unique_ptr<uiwidgets::Win32UIWidgetsWindow> view;

  // The state associate with the engine backing the view.
  std::unique_ptr<UIWidgetsDesktopEngineState> engine_state;

  // The window handle given to API clients.
  std::unique_ptr<UIWidgetsDesktopView> view_wrapper;
};

// Opaque reference for the native windows itself. This is separate from the
// controller so that it can be provided to plugins without giving them access
// to all of the controller-based functionality.
struct UIWidgetsDesktopView {
  // The window that (indirectly) owns this state object.
  uiwidgets::Win32UIWidgetsWindow* window;
};

// Struct for storing state of a UIWidgets engine instance.
struct UIWidgetsDesktopEngineState {
  // The handle to the UIWidgets engine instance.
  UIWIDGETS_API_SYMBOL(UIWidgetsEngine) engine;

  // Task runner for tasks posted from the engine.
  std::unique_ptr<uiwidgets::Win32TaskRunner> task_runner;
};

// State associated with the plugin registrar.
struct UIWidgetsDesktopPluginRegistrar {
  // The plugin messenger handle given to API clients.
  std::unique_ptr<UIWidgetsDesktopMessenger> messenger;

  // The handle for the window associated with this registrar.
  UIWidgetsDesktopView* window;
};

// State associated with the messenger used to communicate with the engine.
struct UIWidgetsDesktopMessenger {
  // The UIWidgets engine this messenger sends outgoing messages to.
  UIWIDGETS_API_SYMBOL(UIWidgetsEngine) engine;

  // The message dispatcher for handling incoming messages.
  uiwidgets::IncomingMessageDispatcher* dispatcher;
};

#endif  // UIWIDGETS_SHELL_PLATFORM_WINDOWS_UIWIDGETS_WINDOW_STATE_H_
