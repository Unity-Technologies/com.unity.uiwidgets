#pragma once

#if defined(__UIWIDGETS_MAC__) || defined(__UIWIDGETS_IOS__)
#include "shell/platform/unity/darwin/common/uiwidgets_system.h"
#elif __UIWIDGETS_WIN__
#include "shell/platform/unity/windows/uiwidgets_system.h"
#endif