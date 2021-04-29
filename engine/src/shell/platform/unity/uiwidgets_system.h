#pragma once

#ifdef __ANDROID__
#include "shell/platform/unity/android/uiwidgets_system.h"
#elif __APPLE__
    #ifdef TARGET_OS_IOS
        #include "shell/platform/unity/darwin/ios/uiwidgets_system.h"
    #else
        #include "shell/platform/unity/darwin/macos/uiwidgets_system.h"
    #endif
#elif _WIN64
    #include "shell/platform/unity/windows/uiwidgets_system.h"
#endif