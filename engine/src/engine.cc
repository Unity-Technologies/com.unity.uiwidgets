#include "shell/platform/unity/uiwidgets_system.h"

extern "C" void UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API
UnityPluginLoad(IUnityInterfaces* unityInterfaces) {
	uiwidgets::UIWidgetsSystem::GetInstancePtr()->BindUnityInterfaces(unityInterfaces);
}

extern "C" void UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API UnityPluginUnload() {
  uiwidgets::UIWidgetsSystem::GetInstancePtr()->UnBindUnityInterfaces();
}
