

#ifndef UIWIDGETS_SHELL_PLATFORM_COMMON_CPP_PUBLIC_UIWIDGETS_EXPORT_H_
#define UIWIDGETS_SHELL_PLATFORM_COMMON_CPP_PUBLIC_UIWIDGETS_EXPORT_H_

#ifdef UIWIDGETS_DESKTOP_LIBRARY

#ifdef _WIN32
#define UIWIDGETS_EXPORT __declspec(dllexport)
#else
#define UIWIDGETS_EXPORT __attribute__((visibility("default")))
#endif

#else

#ifdef _WIN32
#define UIWIDGETS_EXPORT __declspec(dllimport)
#else
#define UIWIDGETS_EXPORT
#endif

#endif

#endif
