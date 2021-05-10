#pragma once

// OpenGL ES and EGL includes
#include <EGL/egl.h>
#include <EGL/eglext.h>
#include <EGL/eglplatform.h>
#include <GLES2/gl2.h>
#include <GLES2/gl2ext.h>
#include <d3d11.h>
#include <windows.h>

#include <cstdint>

#include "Unity/IUnityInterface.h"
#include "flutter/fml/macros.h"

namespace uiwidgets {

class UnitySurfaceManager {
 public:
  UnitySurfaceManager(IUnityInterfaces* unity_interfaces);
  ~UnitySurfaceManager();

  GLuint CreateRenderSurface(size_t width, size_t height);

  bool ReleaseNativeRenderTexture();

  bool ClearCurrent();

  bool MakeCurrent(const EGLSurface surface);

  bool MakeResourceCurrent();

  EGLDisplay GetEGLDisplay() const { return egl_display_; }

  ID3D11Device* GetD3D11Device() const { return d3d11_angle_device_; }

  void* GetD3DInnerTexture() const { return static_cast<void*>(d3d11_texture); }

  FML_DISALLOW_COPY_AND_ASSIGN(UnitySurfaceManager);

 private:
  bool Initialize(IUnityInterfaces* unity_interfaces);
  void CreateRenderTexture(size_t width, size_t height);
  void CleanUp();

  EGLDisplay egl_display_;
  EGLContext egl_context_;
  EGLContext egl_resource_context_;
  EGLConfig egl_config_;
  bool initialize_succeeded_;
  ID3D11Device* d3d11_device_;
  ID3D11Device* d3d11_angle_device_;
  ID3D11Texture2D* d3d11_texture;

  EGLImage fbo_egl_image_ = nullptr;
  GLuint fbo_texture_ = 0;
  GLuint fbo_ = 0;
};

}  // namespace uiwidgets
