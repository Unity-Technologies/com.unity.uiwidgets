// Copyright 2013 The UIWidgets Authors. All rights reserved.
// Use of this source code is governed by a BSD-style license that can be
// found in the LICENSE file.

#ifndef UIWIDGETS_SHELL_PLATFORM_WINDOWS_PUBLIC_UIWIDGETS_H_
#define UIWIDGETS_SHELL_PLATFORM_WINDOWS_PUBLIC_UIWIDGETS_H_

#include <stddef.h>
#include <stdint.h>
#include "shell/platform/common/cpp/public/uiwidgets_export.h"
#include "shell/platform/common/cpp/public/uiwidgets_messenger.h"
#include "shell/platform/common/cpp/public/uiwidgets_plugin_registrar.h"

#include "Windows.h"

#if defined(__cplusplus)
extern "C" {
#endif

// Opaque reference to a UIWidgets window controller.
typedef struct UIWidgetsDesktopViewControllerState*
    UIWidgetsDesktopViewControllerRef;

// Opaque reference to a UIWidgets window.
typedef struct UIWidgetsDesktopView* UIWidgetsDesktopViewRef;

// Opaque reference to a UIWidgets engine instance.
typedef struct UIWidgetsDesktopEngineState* UIWidgetsDesktopEngineRef;

// Properties for configuring a UIWidgets engine instance.
typedef struct {
  // The path to the uiwidgets_assets folder for the application to be run.
  // This can either be an absolute path or a path relative to the directory
  // containing the executable.
  const wchar_t* assets_path;

  // The path to the icudtl.dat file for the version of UIWidgets you are using.
  // This can either be an absolute path or a path relative to the directory
  // containing the executable.
  const wchar_t* icu_data_path;

  // The switches to pass to the UIWidgets engine.
  //
  // See: https://github.com/uiwidgets/engine/blob/master/shell/common/switches.h
  // for details. Not all arguments will apply to desktop.
  const char** switches;

  // The number of elements in |switches|.
  size_t switches_count;
} UIWidgetsDesktopEngineProperties;

// Creates a View with the given dimensions running a UIWidgets Application.
//
// This will set up and run an associated UIWidgets engine using the settings in
// |engine_properties|.
//
// Returns a null pointer in the event of an error.
UIWIDGETS_EXPORT UIWidgetsDesktopViewControllerRef
UIWidgetsDesktopCreateViewController(
    int width,
    int height,
    const UIWidgetsDesktopEngineProperties& engine_properties);

// DEPRECATED. Will be removed soon; switch to the version above.
UIWIDGETS_EXPORT UIWidgetsDesktopViewControllerRef
UIWidgetsDesktopCreateViewControllerLegacy(int initial_width,
                                         int initial_height,
                                         const char* assets_path,
                                         const char* icu_data_path,
                                         const char** arguments,
                                         size_t argument_count);

// Shuts down the engine instance associated with |controller|, and cleans up
// associated state.
//
// |controller| is no longer valid after this call.
UIWIDGETS_EXPORT void UIWidgetsDesktopDestroyViewController(
    UIWidgetsDesktopViewControllerRef controller);

// Returns the plugin registrar handle for the plugin with the given name.
//
// The name must be unique across the application.
UIWIDGETS_EXPORT UIWidgetsDesktopPluginRegistrarRef
UIWidgetsDesktopGetPluginRegistrar(UIWidgetsDesktopViewControllerRef controller,
                                 const char* plugin_name);

// Returns the view managed by the given controller.
UIWIDGETS_EXPORT UIWidgetsDesktopViewRef
UIWidgetsDesktopGetView(UIWidgetsDesktopViewControllerRef controller);

// Processes any pending events in the UIWidgets engine, and returns the
// number of nanoseconds until the next scheduled event (or  max, if none).
//
// This should be called on every run of the application-level runloop, and
// a wait for native events in the runloop should never be longer than the
// last return value from this function.
UIWIDGETS_EXPORT uint64_t
UIWidgetsDesktopProcessMessages(UIWidgetsDesktopViewControllerRef controller);

// Return backing HWND for manipulation in host application.
UIWIDGETS_EXPORT HWND UIWidgetsDesktopViewGetHWND(UIWidgetsDesktopViewRef view);

// Gets the DPI for a given |hwnd|, depending on the supported APIs per
// windows version and DPI awareness mode. If nullptr is passed, returns the DPI
// of the primary monitor.
UIWIDGETS_EXPORT UINT UIWidgetsDesktopGetDpiForHWND(HWND hwnd);

// Gets the DPI for a given |monitor|. If the API is not available, a default
// DPI of 96 is returned.
UIWIDGETS_EXPORT UINT UIWidgetsDesktopGetDpiForMonitor(HMONITOR monitor);

// Runs an instance of a headless UIWidgets engine.
//
// Returns a null pointer in the event of an error.
UIWIDGETS_EXPORT UIWidgetsDesktopEngineRef UIWidgetsDesktopRunEngine(
    const UIWidgetsDesktopEngineProperties& engine_properties);

// Shuts down the given engine instance. Returns true if the shutdown was
// successful. |engine_ref| is no longer valid after this call.
UIWIDGETS_EXPORT bool UIWidgetsDesktopShutDownEngine(
    UIWidgetsDesktopEngineRef engine_ref);

// Returns the view associated with this registrar's engine instance
// This is a Windows-specific extension to uiwidgets_plugin_registrar.h.
UIWIDGETS_EXPORT UIWidgetsDesktopViewRef
UIWidgetsDesktopRegistrarGetView(UIWidgetsDesktopPluginRegistrarRef registrar);

#if defined(__cplusplus)
}  // extern "C"
#endif

#endif  // UIWIDGETS_SHELL_PLATFORM_WINDOWS_PUBLIC_UIWIDGETS_WINDOWS_H_
