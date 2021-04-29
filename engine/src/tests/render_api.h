#pragma once

#include "Unity/IUnityGraphics.h"
class RenderAPI {
 public:
  virtual ~RenderAPI() {}

  virtual void ProcessDeviceEvent(UnityGfxDeviceEventType type,
                                  IUnityInterfaces* interfaces) = 0;

	virtual void CreateTexture1(void* ptr, int width, int height) = 0;
  virtual void* CreateTexture(int width, int height) = 0;

	virtual void SetImageTexture(void* ptr) = 0;

	virtual void Draw() = 0;

	virtual void PreDraw() = 0;

	virtual void PostDraw() = 0;
};

RenderAPI* CreateRenderAPI(UnityGfxRenderer apiType);