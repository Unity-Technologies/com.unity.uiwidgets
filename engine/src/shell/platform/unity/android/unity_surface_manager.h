#pragma once

// OpenGL ES and EGL includes
#include <EGL/egl.h>
#include <EGL/eglext.h>
#include <EGL/eglplatform.h>
#include <GLES2/gl2.h>
#include <GLES2/gl2ext.h>
// #include <d3d11.h>
// #include <windows.h>

#include <cstdint>

#include "Unity/IUnityInterface.h"
#include "flutter/fml/macros.h"

#include <include/gpu/GrContext.h>
#include <src/gpu/gl/GrGLDefines.h>

#include "cassert"
#include "include/core/SkCanvas.h"
#include "include/core/SkSurface.h"
#include "include/core/SkTextBlob.h"
#include "include/effects/SkPerlinNoiseShader.h"
#include "include/gpu/GrBackendSurface.h"
// #include "include/gpu/GrContext.h"
#include "include/gpu/gl/GrGLTypes.h"
// #include "modules/skottie/include/Skottie.h"
// #include "platform_base.h"
// #include "render_api.h"
// #include "src/gpu/gl/GrGLDefines.h"
// #include "windows.h"
// #include <wingdi.h>

#include "Unity/IUnityGraphics.h"
#include "Unity/IUnityGraphicsVulkan.h"
#include "src/shell/platform/unity/unity_console.h"

// #define VulkanX

namespace uiwidgets
{

  class UnitySurfaceManager
  {
  public:
    UnitySurfaceManager(IUnityInterfaces *unity_interfaces);
    ~UnitySurfaceManager();

    GLuint CreateRenderSurface(void *native_texture_ptr);
    void DestroyRenderSurface();

    bool ClearCurrent();

    bool MakeCurrent(const EGLSurface surface);

    bool MakeResourceCurrent();

    // EGLDisplay GetEGLDisplay() const { return egl_display_; }

    // ID3D11Device* GetD3D11Device() const { return d3d11_device_; }

    FML_DISALLOW_COPY_AND_ASSIGN(UnitySurfaceManager);

  private:
    void draw();
    bool Initialize(IUnityInterfaces *unity_interfaces);
    void CleanUp();

#ifdef VulkanX
    IUnityGraphicsVulkan *m_UnityVulkan;
    UnityVulkanInstance m_Instance;

#endif
    GrBackendTexture m_backendTex;
    GrGLTextureInfo textureInfo;
    sk_sp<SkSurface> m_SkSurface;

    sk_sp<GrContext> gr_context_;

    EGLDisplay egl_display_;
    EGLContext egl_context_;
#ifndef VulkanX
    EGLContext egl_resource_context_;
    EGLConfig egl_config_;

#endif
    // EGLConfig egl_config_;
    bool initialize_succeeded_;
    // ID3D11Device* d3d11_device_;

    // EGLImage fbo_egl_image_ = nullptr;
    // GLuint fbo_texture_ = 0;
    GLuint fbo_ = 0;

    // GLuint m_VertexShader;
  // GLuint m_FragmentShader;
  // GLuint m_Program;
  // GLuint m_VertexArray;
  // GLuint m_VertexBuffer;
  // int m_UniformWorldMatrix;
  // int m_UniformProjMatrix;
  };

} // namespace uiwidgets
