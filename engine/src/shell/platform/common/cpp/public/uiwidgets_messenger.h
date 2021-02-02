

#ifndef UIWIDGETS_SHELL_PLATFORM_COMMON_CPP_PUBLIC_UIWIDGETS_MESSENGER_H_
#define UIWIDGETS_SHELL_PLATFORM_COMMON_CPP_PUBLIC_UIWIDGETS_MESSENGER_H_

#include <stddef.h>
#include <stdint.h>

#include "uiwidgets_export.h"

#if defined(__cplusplus)
extern "C" {
#endif

typedef struct UIWidgetsDesktopMessenger* UIWidgetsDesktopMessengerRef;

typedef struct _UIWidgetsPlatformMessageResponseHandle
    UIWidgetsDesktopMessageResponseHandle;

typedef void (*UIWidgetsDesktopBinaryReply)(const uint8_t* data, size_t data_size,
                                          void* user_data);

typedef struct {
  size_t struct_size;

  const char* channel;

  const uint8_t* message;

  size_t message_size;

  const UIWidgetsDesktopMessageResponseHandle* response_handle;
} UIWidgetsDesktopMessage;

typedef void (*UIWidgetsDesktopMessageCallback)(
    UIWidgetsDesktopMessengerRef /* messenger */,
    const UIWidgetsDesktopMessage* /* message*/, void* /* user data */);

UIWIDGETS_EXPORT bool UIWidgetsDesktopMessengerSend(
    UIWidgetsDesktopMessengerRef messenger, const char* channel,
    const uint8_t* message, const size_t message_size);

UIWIDGETS_EXPORT bool UIWidgetsDesktopMessengerSendWithReply(
    UIWidgetsDesktopMessengerRef messenger, const char* channel,
    const uint8_t* message, const size_t message_size,
    const UIWidgetsDesktopBinaryReply reply, void* user_data);

UIWIDGETS_EXPORT void UIWidgetsDesktopMessengerSendResponse(
    UIWidgetsDesktopMessengerRef messenger,
    const UIWidgetsDesktopMessageResponseHandle* handle, const uint8_t* data,
    size_t data_length);

UIWIDGETS_EXPORT void UIWidgetsDesktopMessengerSetCallback(
    UIWidgetsDesktopMessengerRef messenger, const char* channel,
    UIWidgetsDesktopMessageCallback callback, void* user_data);

#if defined(__cplusplus)
}
#endif

#endif
