// Copyright 2013 The UIWidgets Authors. All rights reserved.
// Use of this source code is governed by a BSD-style license that can be
// found in the LICENSE file.

#include "shell/platform/unity/windows/public/uiwidgets_windows.h"

#include <assert.h>

#include <algorithm>
#include <chrono>
#include <cstdlib>
#include <filesystem>
#include <iostream>
#include <vector>

#include "shell/platform/common/cpp/client_wrapper/include/uiwidgets/plugin_registrar.h"
//#include "shell/platform/common/cpp/incoming_message_dispatcher.h"
//#include "shell/platform/common/cpp/path_utils.h"
#include "shell/platform/embedder/embedder.h"
//#include "shell/platform/unity/windows/dpi_utils.h"
//#include "shell/platform/unity/windows/key_event_handler.h"
//#include "shell/platform/unity/windows/keyboard_hook_handler.h"
//#include "shell/platform/unity/windows/platform_handler.h"
#include "shell/platform/unity/windows/text_input_plugin.h"
//#include "shell/platform/unity/windows/win32_uiwidgets_window.h"
#include "shell/platform/unity/windows/win32_task_runner.h"
#include "shell/platform/unity/windows/window_state.h"

//static_assert(UIWIDGETS_ENGINE_VERSION == 1, "");

// Spins up an instance of the UIWidgets Engine.
//
// This function launches the UIWidgets Engine in a background thread, supplying
// the necessary callbacks for rendering within a win32window (if one is
// provided).
//
// Returns the state object for the engine, or null on failure to start the
// engine.
//static std::unique_ptr<UIWidgetsDesktopEngineState> RunUIWidgetsEngine(
//    uiwidgets::Win32UIWidgetsWindow* window,
//    const UIWidgetsDesktopEngineProperties& engine_properties) {
//  auto state = std::make_unique<UIWidgetsDesktopEngineState>();
//
//  // UIWidgetsProjectArgs is expecting a full argv, so when processing it for
//  // flags the first item is treated as the executable and ignored. Add a dummy
//  // value so that all provided arguments are used.
//  std::vector<const char*> argv = {"placeholder"};
//  if (engine_properties.switches_count > 0) {
//    argv.insert(argv.end(), &engine_properties.switches[0],
//                &engine_properties.switches[engine_properties.switches_count]);
//  }
//
//  window->CreateRenderSurface();
//
//  // Provide the necessary callbacks for rendering within a win32 child window.
//  UIWidgetsRendererConfig config = {};
//  config.type = kOpenGL;
//  config.open_gl.struct_size = sizeof(config.open_gl);
//  config.open_gl.make_current = [](void* user_data) -> bool {
//    auto host = static_cast<uiwidgets::Win32UIWidgetsWindow*>(user_data);
//    return host->MakeCurrent();
//  };
//  config.open_gl.clear_current = [](void* user_data) -> bool {
//    auto host = static_cast<uiwidgets::Win32UIWidgetsWindow*>(user_data);
//    return host->ClearContext();
//  };
//  config.open_gl.present = [](void* user_data) -> bool {
//    auto host = static_cast<uiwidgets::Win32UIWidgetsWindow*>(user_data);
//    return host->SwapBuffers();
//  };
//  config.open_gl.fbo_callback = [](void* user_data) -> uint32_t { return 0; };
//  config.open_gl.gl_proc_resolver = [](void* user_data,
//                                       const char* what) -> void* {
//    return reinterpret_cast<void*>(eglGetProcAddress(what));
//  };
//  config.open_gl.make_resource_current = [](void* user_data) -> bool {
//    auto host = static_cast<uiwidgets::Win32UIWidgetsWindow*>(user_data);
//    return host->MakeResourceCurrent();
//  };
//
//  // Configure task runner interop.
//  auto state_ptr = state.get();
//  state->task_runner = std::make_unique<uiwidgets::Win32TaskRunner>(
//      GetCurrentThreadId(), [state_ptr](const auto* task) {
//        if (UIWidgetsEngineRunTask(state_ptr->engine, task) != kSuccess) {
//          std::cerr << "Could not post an engine task." << std::endl;
//        }
//      });
//  UIWidgetsTaskRunnerDescription platform_task_runner = {};
//  platform_task_runner.struct_size = sizeof(UIWidgetsTaskRunnerDescription);
//  platform_task_runner.user_data = state->task_runner.get();
//  platform_task_runner.runs_task_on_current_thread_callback =
//      [](void* user_data) -> bool {
//    return reinterpret_cast<uiwidgets::Win32TaskRunner*>(user_data)
//        ->RunsTasksOnCurrentThread();
//  };
//  platform_task_runner.post_task_callback = [](UIWidgetsTask task,
//                                               uint64_t target_time_nanos,
//                                               void* user_data) -> void {
//    reinterpret_cast<uiwidgets::Win32TaskRunner*>(user_data)->PostTask(
//        task, target_time_nanos);
//  };
//
//  UIWidgetsCustomTaskRunners custom_task_runners = {};
//  custom_task_runners.struct_size = sizeof(UIWidgetsCustomTaskRunners);
//  custom_task_runners.platform_task_runner = &platform_task_runner;
//
//  std::filesystem::path assets_path(engine_properties.assets_path);
//  std::filesystem::path icu_path(engine_properties.icu_data_path);
//  if (assets_path.is_relative() || icu_path.is_relative()) {
//    // Treat relative paths as relative to the directory of this executable.
//    std::filesystem::path executable_location =
//        uiwidgets::GetExecutableDirectory();
//    if (executable_location.empty()) {
//      std::cerr
//          << "Unable to find executable location to resolve resource paths."
//          << std::endl;
//      return nullptr;
//    }
//    assets_path = std::filesystem::path(executable_location) / assets_path;
//    icu_path = std::filesystem::path(executable_location) / icu_path;
//  }
//  std::string assets_path_string = assets_path.u8string();
//  std::string icu_path_string = icu_path.u8string();
//
//  UIWidgetsProjectArgs args = {};
//  args.struct_size = sizeof(UIWidgetsProjectArgs);
//  args.assets_path = assets_path_string.c_str();
//  args.icu_data_path = icu_path_string.c_str();
//  args.command_line_argc = static_cast<int>(argv.size());
//  args.command_line_argv = &argv[0];
//  args.platform_message_callback =
//      [](const UIWidgetsPlatformMessage* engine_message,
//         void* user_data) -> void {
//    auto window = reinterpret_cast<uiwidgets::Win32UIWidgetsWindow*>(user_data);
//    return window->HandlePlatformMessage(engine_message);
//  };
//  args.custom_task_runners = &custom_task_runners;
//
//  UIWIDGETS_API_SYMBOL(UIWidgetsEngine) engine = nullptr;
//  auto result =
//      UIWidgetsEngineRun(UIWIDGETS_ENGINE_VERSION, &config, &args, window, &engine);
//  if (result != kSuccess || engine == nullptr) {
//    std::cerr << "Failed to start UIWidgets engine: error " << result
//              << std::endl;
//    return nullptr;
//  }
//  state->engine = engine;
//  return state;
//}
//
//UIWidgetsDesktopViewControllerRef UIWidgetsDesktopCreateViewController(
//    int width,
//    int height,
//    const UIWidgetsDesktopEngineProperties& engine_properties) {
//  UIWidgetsDesktopViewControllerRef state =
//      uiwidgets::Win32UIWidgetsWindow::CreateWin32UIWidgetsWindow(width, height);
//
//  auto engine_state = RunUIWidgetsEngine(state->view.get(), engine_properties);
//
//  if (!engine_state) {
//    return nullptr;
//  }
//  state->view->SetState(engine_state->engine);
//  state->engine_state = std::move(engine_state);
//  return state;
//}
//
//UIWidgetsDesktopViewControllerRef UIWidgetsDesktopCreateViewControllerLegacy(
//    int initial_width,
//    int initial_height,
//    const char* assets_path,
//    const char* icu_data_path,
//    const char** arguments,
//    size_t argument_count) {
//  std::filesystem::path assets_path_fs = std::filesystem::u8path(assets_path);
//  std::filesystem::path icu_data_path_fs =
//      std::filesystem::u8path(icu_data_path);
//  UIWidgetsDesktopEngineProperties engine_properties = {};
//  engine_properties.assets_path = assets_path_fs.c_str();
//  engine_properties.icu_data_path = icu_data_path_fs.c_str();
//  engine_properties.switches = arguments;
//  engine_properties.switches_count = argument_count;
//
//  return UIWidgetsDesktopCreateViewController(initial_width, initial_height,
//                                            engine_properties);
//}
//
//uint64_t UIWidgetsDesktopProcessMessages(
//    UIWidgetsDesktopViewControllerRef controller) {
//  return controller->engine_state->task_runner->ProcessTasks().count();
//}
//
//void UIWidgetsDesktopDestroyViewController(
//    UIWidgetsDesktopViewControllerRef controller) {
//  UIWidgetsEngineShutdown(controller->engine_state->engine);
//  delete controller;
//}
//
//UIWidgetsDesktopPluginRegistrarRef UIWidgetsDesktopGetPluginRegistrar(
//    UIWidgetsDesktopViewControllerRef controller,
//    const char* plugin_name) {
//  // Currently, one registrar acts as the registrar for all plugins, so the
//  // name is ignored. It is part of the API to reduce churn in the future when
//  // aligning more closely with the UIWidgets registrar system.
//
//  return controller->view->GetRegistrar();
//}
//
//UIWidgetsDesktopViewRef UIWidgetsDesktopGetView(
//    UIWidgetsDesktopViewControllerRef controller) {
//  return controller->view_wrapper.get();
//}
//
//HWND UIWidgetsDesktopViewGetHWND(UIWidgetsDesktopViewRef view) {
//  return view->window->GetWindowHandle();
//}
//
//UINT UIWidgetsDesktopGetDpiForHWND(HWND hwnd) {
//  return uiwidgets::GetDpiForHWND(hwnd);
//}
//
//UINT UIWidgetsDesktopGetDpiForMonitor(HMONITOR monitor) {
//  return uiwidgets::GetDpiForMonitor(monitor);
//}
//
//UIWidgetsDesktopEngineRef UIWidgetsDesktopRunEngine(
//    const UIWidgetsDesktopEngineProperties& engine_properties) {
//  auto engine = RunUIWidgetsEngine(nullptr, engine_properties);
//  return engine.release();
//}
//
//bool UIWidgetsDesktopShutDownEngine(UIWidgetsDesktopEngineRef engine_ref) {
//  std::cout << "Shutting down uiwidgets engine process." << std::endl;
//  auto result = UIWidgetsEngineShutdown(engine_ref->engine);
//  delete engine_ref;
//  return (result == kSuccess);
//}

void UIWidgetsDesktopRegistrarEnableInputBlocking(
   UIWidgetsDesktopPluginRegistrarRef registrar,
   const char* channel) {
 registrar->messenger->dispatcher->EnableInputBlockingForChannel(channel);
}

UIWidgetsDesktopMessengerRef UIWidgetsDesktopRegistrarGetMessenger(
   UIWidgetsDesktopPluginRegistrarRef registrar) {
 return registrar->messenger.get();
}

//UIWidgetsDesktopViewRef UIWidgetsDesktopRegistrarGetView(
//    UIWidgetsDesktopPluginRegistrarRef registrar) {
//  return registrar->window;
//}
//
bool UIWidgetsDesktopMessengerSendWithReply(UIWidgetsDesktopMessengerRef messenger,
                                          const char* channel,
                                          const uint8_t* message,
                                          const size_t message_size,
                                          const UIWidgetsDesktopBinaryReply reply,
                                          void* user_data) {
  UIWidgetsPlatformMessageResponseHandle* response_handle = nullptr;
  if (reply != nullptr && user_data != nullptr) {
    UIWidgetsEngineResult result = UIWidgetsPlatformMessageCreateResponseHandle(
        messenger->engine, reply, user_data, &response_handle);
    if (result != kSuccess) {
      std::cout << "Failed to create response handle\n";
      return false;
    }
  }

  UIWidgetsPlatformMessage platform_message = {
      sizeof(UIWidgetsPlatformMessage),
      channel,
      message,
      message_size,
      response_handle,
  };

  UIWidgetsEngineResult message_result =
      UIWidgetsEngineSendPlatformMessage(messenger->engine, &platform_message);

  if (response_handle != nullptr) {
    UIWidgetsPlatformMessageReleaseResponseHandle(messenger->engine,
                                                response_handle);
  }

  return message_result == kSuccess;
}

bool UIWidgetsDesktopMessengerSend(UIWidgetsDesktopMessengerRef messenger,
                                 const char* channel,
                                 const uint8_t* message,
                                 const size_t message_size) {
  return UIWidgetsDesktopMessengerSendWithReply(messenger, channel, message,
                                              message_size, nullptr, nullptr);
}

void UIWidgetsDesktopMessengerSendResponse(
    UIWidgetsDesktopMessengerRef messenger,
    const UIWidgetsDesktopMessageResponseHandle* handle,
    const uint8_t* data,
    size_t data_length) {
  UIWidgetsEngineSendPlatformMessageResponse(messenger->engine, handle, data,
                                           data_length);
}

void UIWidgetsDesktopMessengerSetCallback(UIWidgetsDesktopMessengerRef messenger,
                                        const char* channel,
                                        UIWidgetsDesktopMessageCallback callback,
                                        void* user_data) {
  messenger->dispatcher->SetMessageCallback(channel, callback, user_data);
}
