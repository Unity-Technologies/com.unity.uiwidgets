

#ifndef UIWIDGETS_SHELL_PLATFORM_COMMON_CPP_PUBLIC_UIWIDGETS_PLUGIN_REGISTRAR_H_
#define UIWIDGETS_SHELL_PLATFORM_COMMON_CPP_PUBLIC_UIWIDGETS_PLUGIN_REGISTRAR_H_

#include <stddef.h>
#include <stdint.h>

#include "uiwidgets_export.h"
#include "uiwidgets_messenger.h"

#if defined(__cplusplus)
extern "C" {
#endif

typedef struct UIWidgetsDesktopPluginRegistrar* UIWidgetsDesktopPluginRegistrarRef;

UIWIDGETS_EXPORT UIWidgetsDesktopMessengerRef
UIWidgetsDesktopRegistrarGetMessenger(UIWidgetsDesktopPluginRegistrarRef registrar);

UIWIDGETS_EXPORT void UIWidgetsDesktopRegistrarEnableInputBlocking(
    UIWidgetsDesktopPluginRegistrarRef registrar, const char* channel);

#if defined(__cplusplus)
}
#endif

#endif
