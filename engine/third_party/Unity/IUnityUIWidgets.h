#pragma once

#ifdef __APPLE__
#import "stdarg.h"
#endif

#include "IUnityGraphics.h"
#include "IUnityInterface.h"

namespace UnityUIWidgets {
typedef void (*VoidCallback)();
typedef void (*VoidCallbackLong)(long);

UNITY_DECLARE_INTERFACE(IUnityUIWidgets) {
  virtual ~IUnityUIWidgets() {}

  virtual void SetUpdateCallback(VoidCallback callback) = 0;
  virtual void SetVSyncCallback(VoidCallback callback) = 0;
  virtual void SetWaitCallback(VoidCallbackLong callback) = 0;
  virtual void SetWakeUpCallback(VoidCallback callback) = 0;
  virtual void IssuePluginEventAndData(UnityRenderingEventAndData callback,
                                       int eventId, void* data) = 0;
  //TODO zxw: this API is not provided in 2019.4.26f1c1 due to a mistake :(
  //we should consider adding it back later (remember to enable all callers too)
  //virtual void printf_consolev(const char* log, va_list alist) = 0;
};
}  // namespace UnityUIWidgets

UNITY_REGISTER_INTERFACE_GUID_IN_NAMESPACE(0x4C8BE8056B3C41D7ULL,
                                           0xBC8BF5F2F0AC3532ULL,
                                           IUnityUIWidgets, UnityUIWidgets)
