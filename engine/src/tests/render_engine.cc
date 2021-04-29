#include <assert.h>

#include "Unity/IUnityGraphics.h"
#include "Unity/IUnityUIWidgets.h"
#include "include/core/SkSurface.h"
#include "include/gpu/GrBackendSurface.h"


static IUnityInterfaces* s_UnityInterfaces = NULL;
static IUnityGraphics* s_Graphics = NULL;
static UnityGfxRenderer s_DeviceType = kUnityGfxRendererNull;
static RenderAPI* s_CurrentAPI = NULL;

static void UNITY_INTERFACE_API
OnGraphicsDeviceEvent(UnityGfxDeviceEventType eventType) {
  if (eventType == kUnityGfxDeviceEventInitialize) {
    assert(s_CurrentAPI == NULL);
    s_DeviceType = s_Graphics->GetRenderer();
    s_CurrentAPI = CreateRenderAPI(s_DeviceType);
  }

  if (s_CurrentAPI) {
    s_CurrentAPI->ProcessDeviceEvent(eventType, s_UnityInterfaces);
  }

  if (eventType == kUnityGfxDeviceEventShutdown) {
    delete s_CurrentAPI;
    s_CurrentAPI = NULL;
    s_DeviceType = kUnityGfxRendererNull;
  }
}

extern "C" void UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API
UnityPluginLoad(IUnityInterfaces* unityInterfaces) {
  //do nothing
}

extern "C" void UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API UnityPluginUnload() {
  //do nothing
}

// --------------------------------------------------------------------------
// SetTextureFromUnity, an example function we export which is called by one of
// the scripts.


extern "C" UNITY_INTERFACE_EXPORT void* UNITY_INTERFACE_API
CreateTexture(int w, int h) {
  if (s_CurrentAPI == NULL) {
    return NULL;
  }

  return s_CurrentAPI->CreateTexture(w, h);
}

extern "C" UNITY_INTERFACE_EXPORT void UNITY_INTERFACE_API
CreateTexture1(void *ptr, int w, int h) {
  if (s_CurrentAPI == NULL) {
    return;
  }

  s_CurrentAPI->CreateTexture1(ptr, w, h);
}

extern "C" UNITY_INTERFACE_EXPORT void UNITY_INTERFACE_API
SetImageTexture(void *ptr) {
  if (s_CurrentAPI == NULL) {
    return;
  }

  return s_CurrentAPI->SetImageTexture(ptr);
}

extern "C" void UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API Draw123() {
  if (s_CurrentAPI == NULL) {
    return;
  }

  s_CurrentAPI->Draw();
}

static void UNITY_INTERFACE_API OnRenderEvent(int eventID) {
  // Unknown / unsupported graphics device type? Do nothing
  if (s_CurrentAPI == NULL) {
    return;
  }

  if (eventID == 1) {
    s_CurrentAPI->Draw();
  } else if (eventID == 2) {
    s_CurrentAPI->PreDraw();
  } else if (eventID == 3) {
    s_CurrentAPI->PostDraw();
  }
}

extern "C" UnityRenderingEvent UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API
GetRenderEventFunc() {
  return OnRenderEvent;
}

