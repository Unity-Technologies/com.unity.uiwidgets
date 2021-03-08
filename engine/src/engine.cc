#include "shell/platform/unity/uiwidgets_system.h"

extern "C" void UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API
UnityPluginLoad(IUnityInterfaces* unityInterfaces) {
	uiwidgets::UIWidgetsSystem::GetInstancePtr()->BindUnityInterfaces(unityInterfaces);
}

extern "C" void UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API UnityPluginUnload() {
  uiwidgets::UIWidgetsSystem::GetInstancePtr()->UnBindUnityInterfaces();
}

extern "C" UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API float TestFloat()
{
#if defined(__CYGWIN32__)
  return 0.3f;
#elif defined(WIN32) || defined(_WIN32) || defined(__WIN32__) || defined(_WIN64) || defined(WINAPI_FAMILY)
  return 0.2f;
#elif defined(__MACH__) || defined(__ANDROID__) || defined(__linux__) || defined(LUMIN)
  return 0.1f;
#endif
}