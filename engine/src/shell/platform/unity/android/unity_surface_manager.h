#pragma once

// OpenGL ES and EGL includes
#include <EGL/egl.h>
#include <EGL/eglext.h>
#include <EGL/eglplatform.h>
#include <GLES2/gl2.h>
#include <GLES2/gl2ext.h>

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
#include "include/gpu/gl/GrGLTypes.h"

#include "Unity/IUnityGraphics.h"

#include "Unity/IUnityGraphicsVulkan.h"
#include "include/gpu/vk/GrVkBackendContext.h"

namespace uiwidgets
{

  class UnitySurfaceManager
  {
  public:
    static void GetUnityContext();

    UnitySurfaceManager(IUnityInterfaces *unity_interfaces);
    ~UnitySurfaceManager();

    GLuint CreateRenderSurface(void *native_texture_ptr);
    GLuint CreateRenderSurfaceVK(int width, int height);

    void DestroyRenderSurface();

    void* GetInnerTexture() const { return static_cast<void*>(vk_Image_); }

    bool ClearCurrent();

    bool MakeCurrent(const EGLSurface surface);

    bool MakeResourceCurrent();

    FML_DISALLOW_COPY_AND_ASSIGN(UnitySurfaceManager);

  private:
    bool Initialize(IUnityInterfaces *unity_interfaces);
    void CleanUp();

    EGLContext egl_context_;
    EGLContext egl_resource_context_;
    EGLConfig egl_config_;
    
    IUnityGraphicsVulkan* m_UnityVulkan;
    UnityVulkanInstance m_Instance;
    sk_sp<GrContext> gr_context_;
    sk_sp<SkSurface> m_SkSurface;
    VkImage* vk_Image_;

    bool initialize_succeeded_;

    GLuint fbo_ = 0;
  };

} // namespace uiwidgets
