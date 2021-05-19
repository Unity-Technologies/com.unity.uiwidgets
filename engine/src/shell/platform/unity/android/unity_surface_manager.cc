#include "unity_surface_manager.h"

#include <flutter/fml/logging.h>
#include <EGL/egl.h>
#include <EGL/eglext.h>
#include <GLES2/gl2.h>
#include <GLES2/gl2ext.h>

#include <android/hardware_buffer.h>

#include "src/shell/common/shell_io_manager.h"
#include "src/shell/gpu/gpu_surface_delegate.h"
#include "src/shell/gpu/gpu_surface_gl_delegate.h"
namespace uiwidgets
{

  static EGLDisplay egl_display_;
  static EGLContext egl_unity_context_;

  template <class T>
  using EGLResult = std::pair<bool, T>;

  UnitySurfaceManager::UnitySurfaceManager(IUnityInterfaces *unity_interfaces)
      : egl_context_(EGL_NO_CONTEXT),
        egl_resource_context_(EGL_NO_CONTEXT)
  {
    initialize_succeeded_ = Initialize(unity_interfaces);
  }

  UnitySurfaceManager::~UnitySurfaceManager() { CleanUp(); }

  GLuint UnitySurfaceManager::CreateRenderSurface(size_t width, size_t height)
  {
    buffer = nullptr;


    AHardwareBuffer_Desc usage = {};
    // filling in the usage for HardwareBuffer
    usage.format = AHARDWAREBUFFER_FORMAT_R8G8B8A8_UNORM;
    usage.height = height; //format.height;
    usage.width = width;  //format.width;
    usage.layers = 1;
    usage.rfu0 = 0;
    usage.rfu1 = 0;
    usage.stride = 10;
    usage.usage = AHARDWAREBUFFER_USAGE_CPU_READ_OFTEN |
                AHARDWAREBUFFER_USAGE_CPU_WRITE_NEVER |
                AHARDWAREBUFFER_USAGE_GPU_COLOR_OUTPUT;
    FML_CHECK(AHardwareBuffer_allocate(&usage, &buffer) == 0);
    
    EGLClientBuffer native_buffer = eglGetNativeClientBufferANDROID(buffer);
    assert(native_buffer);
    auto success = false;
    EGLint attrs[] = {EGL_NONE};

    image = eglCreateImageKHR(egl_display_, EGL_NO_CONTEXT,
                                        EGL_NATIVE_BUFFER_ANDROID, native_buffer, attrs);
    
    glBindTexture(GL_TEXTURE_2D, egl_texture_);
    glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_MIN_FILTER, GL_NEAREST);
    glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_MAG_FILTER, GL_NEAREST);
    glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_WRAP_S, GL_CLAMP_TO_EDGE);
    glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_WRAP_T, GL_CLAMP_TO_EDGE);

    GLint old_framebuffer_binding;
    glGetIntegerv(GL_FRAMEBUFFER_BINDING, &old_framebuffer_binding);

    glGenFramebuffers(1, &fbo_);
    glBindFramebuffer(GL_FRAMEBUFFER, fbo_);
   
    glEGLImageTargetTexture2DOES(GL_TEXTURE_2D, image);

    glFramebufferTexture2D(GL_FRAMEBUFFER, GL_COLOR_ATTACHMENT0, GL_TEXTURE_2D, egl_texture_, 0);

    // GLuint gltex = (GLuint)(size_t)(native_texture_ptr);
    // glFramebufferTexture2D(GL_FRAMEBUFFER, GL_COLOR_ATTACHMENT0, GL_TEXTURE_2D, gltex, 0);
    FML_CHECK(glCheckFramebufferStatus(GL_FRAMEBUFFER) == GL_FRAMEBUFFER_COMPLETE);

    glBindFramebuffer(GL_FRAMEBUFFER, old_framebuffer_binding);

    return fbo_;
  }

  void UnitySurfaceManager::DestroyRenderSurface()
  {
    if(buffer != nullptr){
      AHardwareBuffer_release(buffer);
    }
    if(image){
      eglDestroyImageKHR(egl_display_, image);
    }
    FML_DCHECK(fbo_ != 0);
    glDeleteFramebuffers(1, &fbo_);
    
    fbo_ = 0;
    
    eglDestroyImageKHR(egl_display_, image);

  }

  bool UnitySurfaceManager::ClearCurrent()
  {
    return eglMakeCurrent(egl_display_, EGL_NO_SURFACE, EGL_NO_SURFACE,
                          EGL_NO_CONTEXT) == EGL_TRUE;
  }

  bool UnitySurfaceManager::MakeCurrent(const EGLSurface surface)
  {
    return eglMakeCurrent(egl_display_, surface, surface, egl_context_) ==
           EGL_TRUE;
  }

  bool UnitySurfaceManager::MakeResourceCurrent()
  {
    return eglMakeCurrent(egl_display_, EGL_NO_SURFACE, EGL_NO_SURFACE,
                          egl_resource_context_) == EGL_TRUE;
  }

  static EGLResult<EGLConfig> ChooseEGLConfiguration(EGLDisplay display)
  {
    EGLint attributes[] = {
        // clang-format off
      EGL_RENDERABLE_TYPE, EGL_OPENGL_ES2_BIT,
      EGL_SURFACE_TYPE,    EGL_WINDOW_BIT,
      EGL_RED_SIZE,        8,
      EGL_GREEN_SIZE,      8,
      EGL_BLUE_SIZE,       8,
      EGL_ALPHA_SIZE,      8,
      EGL_DEPTH_SIZE,      0,
      EGL_STENCIL_SIZE,    0,
      EGL_NONE,            // termination sentinel
        // clang-format on
    };

    EGLint config_count = 0;
    EGLConfig egl_config = nullptr;

    if (eglChooseConfig(display, attributes, &egl_config, 1, &config_count) !=
        EGL_TRUE)
    {
      return {false, nullptr};
    }

    bool success = config_count > 0 && egl_config != nullptr;

    return {success, success ? egl_config : nullptr};
  }

  static EGLResult<EGLSurface> CreateContext(EGLDisplay display,
                                             EGLConfig config,
                                             EGLContext share = EGL_NO_CONTEXT)
  {
    EGLint attributes[] = {EGL_CONTEXT_CLIENT_VERSION, 2, EGL_NONE};

    EGLContext context = eglCreateContext(display, config, share, attributes);

    return {context != EGL_NO_CONTEXT, context};
  }

  void UnitySurfaceManager::GetUnityContext()
  {
    if(egl_unity_context_ != nullptr){
      return;
    }
    egl_display_ = eglGetCurrentDisplay();
    egl_unity_context_ = eglGetCurrentContext();
    FML_CHECK(egl_display_ != EGL_NO_DISPLAY)
        << "Renderer type is invalid";
  }

  bool UnitySurfaceManager::Initialize(IUnityInterfaces *unity_interfaces)
  {
    FML_CHECK(egl_display_ != EGL_NO_DISPLAY)
        << "Renderer type is invalid";

    // Initialize the display connection.
    FML_CHECK(eglInitialize(egl_display_, nullptr, nullptr) == EGL_TRUE)
        << "Renderer type is invalid";

    auto valid_ = true;

    bool success = false;

    std::tie(success, egl_config_) = ChooseEGLConfiguration(egl_display_);
    FML_CHECK(success) << "Could not choose an EGL configuration.";

    std::tie(success, egl_context_) = CreateContext(egl_display_, egl_config_, egl_unity_context_);

    std::tie(success, egl_resource_context_) = CreateContext(egl_display_, egl_config_, egl_context_);
    MakeCurrent(EGL_NO_DISPLAY);

    egl_texture_ = 0;
    glGenTextures(1, &egl_texture_);
    return success;
  }

  void UnitySurfaceManager::CleanUp()
  {
    if (egl_display_ != EGL_NO_DISPLAY &&
        egl_resource_context_ != EGL_NO_CONTEXT)
    {
      eglDestroyContext(egl_display_, egl_resource_context_);
      egl_resource_context_ = EGL_NO_CONTEXT;
    }
    if (egl_display_ != EGL_NO_DISPLAY && egl_context_ != EGL_NO_CONTEXT)
    {
      eglDestroyContext(egl_display_, egl_context_);
      egl_context_ = EGL_NO_CONTEXT;
    }
  }

} // namespace uiwidgets
