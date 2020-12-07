#pragma once

#ifdef __APPLE__
#include "shell/platform/unity/darwin/macos/uiwidgets_system.h"
#elif _WIN64
#include "shell/platform/unity/windows/uiwidgets_system.h"
#endif