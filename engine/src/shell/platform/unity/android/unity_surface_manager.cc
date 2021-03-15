#include "unity_surface_manager.h"

// #include <d3d11.h>
// #include <dxgi.h>
#include <flutter/fml/logging.h>
#include <GLES2/gl2.h>
// #	include <OpenGL/gl3.h>

#include "include/gpu/vk/GrVkBackendContext.h"
// #include "src/gpu/gl/GrGLDefines.h"
#include "src/shell/common/shell_io_manager.h"
#include "src/shell/gpu/gpu_surface_delegate.h"
#include "src/shell/gpu/gpu_surface_gl_delegate.h"
namespace uiwidgets
{

  template <class T>
  using EGLResult = std::pair<bool, T>;

  UnitySurfaceManager::UnitySurfaceManager(IUnityInterfaces *unity_interfaces)
      :
        egl_display_(EGL_NO_DISPLAY),
        egl_context_(EGL_NO_CONTEXT),
        egl_resource_context_(EGL_NO_CONTEXT),
        surface(EGL_NO_SURFACE)
  {
    initialize_succeeded_ = Initialize(unity_interfaces);
  }

  UnitySurfaceManager::~UnitySurfaceManager() { CleanUp(); }

  GLuint UnitySurfaceManager::CreateRenderSurface(void *native_texture_ptr, size_t width, size_t height)
  {
    // unsigned int texture;
    // glGenTextures(1, &texture);
    int rowPitch = width * 4;
    unsigned char *data = new unsigned char[rowPitch * height];

    unsigned char *dst = (unsigned char *)data;
    for (size_t y = 0; y < height; ++y)
    {
      unsigned char *ptr = dst;
      for (size_t x = 0; x < width; ++x)
      {
        // Simple "plasma effect": several combined sine waves
        int vv = 255;

        // Write the texture pixel
        ptr[0] = vv;
        // ptr[1] = vv;
        // ptr[2] = vv;
        ptr[3] = vv;

        // To next pixel (our pixels are 4 bpp)
        ptr += 4;
      }

      // To next image row
      dst += rowPitch;
    }
    GLuint gltex = (GLuint)(size_t)(native_texture_ptr);
    glBindTexture(GL_TEXTURE_2D, gltex);
    glTexSubImage2D(GL_TEXTURE_2D, 0, 0, 0, width, height, GL_RGBA, GL_UNSIGNED_BYTE, data);
    delete[](unsigned char *) data;

    return fbo_;
  }

  void UnitySurfaceManager::DestroyRenderSurface()
  {
    // FML_DCHECK(fbo_ != 0);
    // glDeleteFramebuffers(1, &fbo_);
    // fbo_ = 0;

    // FML_DCHECK(fbo_texture_ != 0);
    // glDeleteTextures(1, &fbo_texture_);
    // fbo_texture_ = 0;

    // FML_DCHECK(fbo_egl_image_ != nullptr);
    // eglDestroyImageKHR(egl_display_, fbo_egl_image_);
    // fbo_egl_image_ = nullptr;
  }

  bool UnitySurfaceManager::ClearCurrent()
  {
    return eglMakeCurrent(egl_display_, EGL_NO_SURFACE, EGL_NO_SURFACE,
                          EGL_NO_CONTEXT) == EGL_TRUE;
  }

  bool UnitySurfaceManager::MakeCurrent(const EGLSurface surface)
  {

    UnityConsole::WriteLine("test c++: clear current");
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

  bool UnitySurfaceManager::Initialize(IUnityInterfaces *unity_interfaces)
  {
    egl_display_ = eglGetDisplay(EGL_DEFAULT_DISPLAY);
    FML_CHECK(egl_display_ != EGL_NO_DISPLAY)
        << "Renderer type is invalid";

    // Initialize the display connection.
    FML_CHECK(eglInitialize(egl_display_, nullptr, nullptr) == EGL_TRUE)
        << "Renderer type is invalid";

    auto valid_ = true;

    bool success = false;

    std::tie(success, egl_config_) = ChooseEGLConfiguration(egl_display_);
    FML_CHECK(success) << "Could not choose an EGL configuration.";

    const EGLint attribs[] = {EGL_WIDTH, 1, EGL_HEIGHT, 1, EGL_NONE};

    surface = eglCreatePbufferSurface(egl_display_, egl_config_, attribs);
    success = surface != EGL_NO_SURFACE;

    EGLint attributes[] = {EGL_CONTEXT_CLIENT_VERSION, 2, EGL_NONE};
    egl_context_ = eglCreateContext(egl_display_, egl_config_, EGL_NO_CONTEXT, attributes);
    return true;
  }

  void UnitySurfaceManager::CleanUp()
  {
  }

  void UnitySurfaceManager::draw()
  {
  }

  static void *g_TextureHandle = NULL;
  static int wi = 0;
  static int he = 0;
  static EGLDisplay egl_display_s;
  static EGLConfig egl_config_s;
  static EGLContext egl_context_s;
  static GLenum state;
  static GLuint fbo_s = 0;

  void *UnitySurfaceManager::SetTextureFromUnity(void *tex, int w, int h)
  {
    g_TextureHandle = tex;
    wi = w;
    he = h;
    return nullptr;
  }

  void UnitySurfaceManager::SetUp()
  {
    auto read_s = eglGetCurrentSurface(EGL_READ);
    auto draw_s = eglGetCurrentSurface(EGL_DRAW);
    egl_display_s = eglGetCurrentDisplay();
    auto egl_context_s_old = eglGetCurrentContext();
    GLuint gltex = (GLuint)(size_t)(g_TextureHandle);

    bool success = false;

    std::tie(success, egl_config_s) = ChooseEGLConfiguration(egl_display_s);

    EGLint attributes[] = {EGL_CONTEXT_CLIENT_VERSION, 2, EGL_NONE};
    egl_context_s = eglCreateContext(egl_display_s, egl_config_s, egl_context_s_old, attributes);

    auto state = eglMakeCurrent(egl_display_s, EGL_NO_SURFACE, EGL_NO_SURFACE, egl_context_s) == GL_TRUE;
    GLint old_framebuffer_binding;
    glGetIntegerv(GL_FRAMEBUFFER_BINDING, &old_framebuffer_binding);

    glGenFramebuffers(1, &fbo_s);
    glBindFramebuffer(GL_FRAMEBUFFER, fbo_s);
    glFramebufferTexture2D(GL_FRAMEBUFFER, GL_COLOR_ATTACHMENT0, GL_TEXTURE_2D, gltex, 0);
    FML_CHECK(glCheckFramebufferStatus(GL_FRAMEBUFFER) == GL_FRAMEBUFFER_COMPLETE);
    glBindFramebuffer(GL_FRAMEBUFFER, old_framebuffer_binding);
    eglMakeCurrent(egl_display_s, draw_s, read_s, egl_context_s_old);
  }

  static int siyao = 0;

  void UnitySurfaceManager::drawxxx()
  {
    int width = wi;
    int height = he;

    int rowPitch = width * 4;
    void *data = new unsigned char[rowPitch * height];

    unsigned char *dst = (unsigned char *)data;
    siyao++;

    for (int y = 0; y < height; ++y)
    {
      unsigned char *ptr = dst;
      for (int x = 0; x < width; ++x)
      {
        int vv = (244 + siyao) % 244;

        // Write the texture pixel
        ptr[0] = vv;
        ptr[1] = (x + y + siyao) % 244;
        ptr[2] = (x * y * siyao) % 244;
        ptr[3] = vv;

        ptr += 4;
      }

      dst += rowPitch;
    }
    GLuint gltex = (GLuint)(size_t)(g_TextureHandle);
    auto state = eglMakeCurrent(egl_display_s, EGL_NO_SURFACE, EGL_NO_SURFACE, egl_context_s) == GL_TRUE;
    glBindTexture(GL_TEXTURE_2D, gltex);

    glTexSubImage2D(GL_TEXTURE_2D, 0, 0, 0, width, height, GL_RGBA, GL_UNSIGNED_BYTE, data);
    // glClearColor(1.0, 0, 1.0, 1.0);
    // glClear(GL_COLOR_BUFFER_BIT);
    eglMakeCurrent(egl_display_s, EGL_NO_DISPLAY, EGL_NO_DISPLAY, EGL_NO_CONTEXT);
    delete[](unsigned char *) data;
  }

} // namespace uiwidgets
