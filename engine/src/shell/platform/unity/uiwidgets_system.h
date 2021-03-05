#pragma once

#ifdef __ANDROID__
#include "shell/platform/unity/darwin/android/uiwidgets_system.h"
#elif __APPLE__
#include "shell/platform/unity/darwin/macos/uiwidgets_system.h"
#elif _WIN64
#include "shell/platform/unity/windows/uiwidgets_system.h"
#endif