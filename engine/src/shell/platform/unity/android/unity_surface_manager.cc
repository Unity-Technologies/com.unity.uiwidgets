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
#ifdef VulkanX
#define UNITY_USED_VULKAN_API_FUNCTIONS(apply) \
  apply(vkGetDeviceProcAddr);                  \
  apply(vkCreateInstance);                     \
  apply(vkCmdBeginRenderPass);                 \
  apply(vkCreateBuffer);                       \
  apply(vkGetPhysicalDeviceMemoryProperties);  \
  apply(vkGetBufferMemoryRequirements);        \
  apply(vkMapMemory);                          \
  apply(vkBindBufferMemory);                   \
  apply(vkAllocateMemory);                     \
  apply(vkDestroyBuffer);                      \
  apply(vkFreeMemory);                         \
  apply(vkUnmapMemory);                        \
  apply(vkQueueWaitIdle);                      \
  apply(vkDeviceWaitIdle);                     \
  apply(vkCmdCopyBufferToImage);               \
  apply(vkFlushMappedMemoryRanges);            \
  apply(vkCreatePipelineLayout);               \
  apply(vkCreateShaderModule);                 \
  apply(vkDestroyShaderModule);                \
  apply(vkCreateGraphicsPipelines);            \
  apply(vkCmdBindPipeline);                    \
  apply(vkCmdDraw);                            \
  apply(vkCmdPushConstants);                   \
  apply(vkCmdBindVertexBuffers);               \
  apply(vkDestroyPipeline);                    \
  apply(vkDestroyPipelineLayout);

#define VULKAN_DEFINE_API_FUNCPTR(func) static PFN_##func my_##func
  VULKAN_DEFINE_API_FUNCPTR(vkGetInstanceProcAddr);
  UNITY_USED_VULKAN_API_FUNCTIONS(VULKAN_DEFINE_API_FUNCPTR);
#undef VULKAN_DEFINE_API_FUNCPTR

  static void LoadVulkanAPI(PFN_vkGetInstanceProcAddr getInstanceProcAddr,
                            VkInstance instance)
  {
    if (!my_vkGetInstanceProcAddr && getInstanceProcAddr)
      my_vkGetInstanceProcAddr = getInstanceProcAddr;

    if (!my_vkCreateInstance)
      my_vkCreateInstance = (PFN_vkCreateInstance)my_vkGetInstanceProcAddr(
          VK_NULL_HANDLE, "vkCreateInstance");

#define LOAD_VULKAN_FUNC(fn) \
  if (!my_##fn)              \
  my_##fn = (PFN_##fn)my_vkGetInstanceProcAddr(instance, #fn)
    UNITY_USED_VULKAN_API_FUNCTIONS(LOAD_VULKAN_FUNC);
#undef LOAD_VULKAN_FUNC
  }
#else

  template <class T>
  using EGLResult = std::pair<bool, T>;
#endif

  UnitySurfaceManager::UnitySurfaceManager(IUnityInterfaces *unity_interfaces)
      :
#ifdef VulkanX
        m_UnityVulkan(NULL)
#else
        egl_display_(EGL_NO_DISPLAY),
        egl_context_(EGL_NO_CONTEXT),
        egl_resource_context_(EGL_NO_CONTEXT),
        surface(EGL_NO_SURFACE)
  // egl_config_(nullptr)
#endif
  {
    initialize_succeeded_ = Initialize(unity_interfaces);
  }

  UnitySurfaceManager::~UnitySurfaceManager() { CleanUp(); }

  GLuint UnitySurfaceManager::CreateRenderSurface(void *native_texture_ptr, size_t width, size_t height)
  {
#ifdef VulkanX
    UnityVulkanImage image;

    m_UnityVulkan->AccessTexture(
        native_texture_ptr, UnityVulkanWholeImage, VkImageLayout::VK_IMAGE_LAYOUT_UNDEFINED, 0,
        0, UnityVulkanResourceAccessMode::kUnityVulkanResourceAccess_ObserveOnly,
        &image);

    GrVkImageInfo info(
        image.image, GrVkAlloc(image.memory.memory, image.memory.offset, image.memory.size, image.memory.flags),
        image.tiling,
        image.layout,
        image.format,
        image.mipCount);

    GrBackendTexture backendTex(width, height, info);
    m_SkSurface = SkSurface::MakeFromBackendTexture(
        gr_context_.get(), backendTex, kBottomLeft_GrSurfaceOrigin, 1,
        kRGBA_8888_SkColorType, nullptr, nullptr);

    SkCanvas *canvas = m_SkSurface->getCanvas();

    canvas->drawColor(SK_ColorBLUE);
    SkPaint paint;

    SkRect rect = SkRect::MakeXYWH(50, 50, 40, 60);
    canvas->drawRect(rect, paint);

    // SkPaint paint2;
    // auto text = SkTextBlob::MakeFromString("Hello, Skia!", SkFont(nullptr, 18));
    // canvas->drawTextBlob(text.get(), 50, 25, paint2);
    canvas->flush();
#else
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

    // glTexSubImage2D(GL_TEXTURE_2D, 0, 0, 0, width, height, GL_BGRA, GL_UNSIGNED_BYTE, native_texture_ptr);
    // glGenerateMipmap(GL_TEXTURE_2D);
    delete[](unsigned char *) data;
    // glClearColor(1,0,0,1.0);
    // glClear(GL_COLOR_BUFFER_BIT);

    // gr_context_ = ShellIOManager::CreateCompatibleResourceLoadingContext(
    //     GrBackend::kOpenGL_GrBackend,
    //     GPUSurfaceGLDelegate::GetDefaultPlatformGLInterface());

    // GrGLTextureInfo textureInfo;
    // textureInfo.fTarget = GR_GL_TEXTURE_2D;
    // textureInfo.fID = GrGLuint((long)native_texture_ptr);
    // textureInfo.fFormat = GR_GL_RGBA8;

    // // egl_context_

    // GrBackendTexture m_backendTex =
    //     GrBackendTexture(width, height, GrMipMapped::kNo, textureInfo);

    // m_SkSurface = SkSurface::MakeFromBackendTexture(
    //     gr_context_.get(), m_backendTex, kBottomLeft_GrSurfaceOrigin, 1,
    //     kRGBA_8888_SkColorType, nullptr, nullptr);

    // SkCanvas *canvas = m_SkSurface->getCanvas();

    // canvas->drawColor(SK_ColorBLUE);
    // SkPaint paint;

    // SkRect rect = SkRect::MakeXYWH(50, 50, 40, 60);
    // canvas->drawRect(rect, paint);
#endif

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
    UnityConsole::WriteLine("test c++: make current");
#ifdef VulkanX
    return true;
#else
    return eglMakeCurrent(egl_display_, EGL_NO_SURFACE, EGL_NO_SURFACE,
                          EGL_NO_CONTEXT) == EGL_TRUE;
#endif
  }

  bool UnitySurfaceManager::MakeCurrent(const EGLSurface surface)
  {

    UnityConsole::WriteLine("test c++: clear current");
#ifdef Vulkanx
    return true;
#else
    return eglMakeCurrent(egl_display_, surface, surface, egl_context_) ==
           EGL_TRUE;
#endif
  }

  bool UnitySurfaceManager::MakeResourceCurrent()
  {
#ifdef VulkanX
    return true;
#else
    return eglMakeCurrent(egl_display_, EGL_NO_SURFACE, EGL_NO_SURFACE,
                          egl_resource_context_) == EGL_TRUE;
#endif
  }
#ifdef VulkanX
  static VKAPI_ATTR VkResult VKAPI_CALL Hook_vkCreateInstance(
      const VkInstanceCreateInfo *pCreateInfo,
      const VkAllocationCallbacks *pAllocator, VkInstance *pInstance)
  {
    my_vkCreateInstance = (PFN_vkCreateInstance)my_vkGetInstanceProcAddr(
        VK_NULL_HANDLE, "vkCreateInstance");
    VkResult result = my_vkCreateInstance(pCreateInfo, pAllocator, pInstance);
    if (result == VK_SUCCESS)
      LoadVulkanAPI(my_vkGetInstanceProcAddr, *pInstance);

    return result;
  }

  static VKAPI_ATTR PFN_vkVoidFunction VKAPI_CALL
  Hook_vkGetInstanceProcAddr(VkInstance device, const char *funcName)
  {
    if (!funcName)
      return NULL;

#define INTERCEPT(fn)             \
  if (strcmp(funcName, #fn) == 0) \
  return (PFN_vkVoidFunction)&Hook_##fn
    INTERCEPT(vkCreateInstance);
#undef INTERCEPT

    return NULL;
  }

  static PFN_vkGetInstanceProcAddr UNITY_INTERFACE_API
  InterceptVulkanInitialization(PFN_vkGetInstanceProcAddr getInstanceProcAddr,
                                void *)
  {
    my_vkGetInstanceProcAddr = getInstanceProcAddr;
    return Hook_vkGetInstanceProcAddr;
  }
#else
  // static GLuint CreateShader(GLenum type, const char *sourceText)
  // {
  //   GLuint ret = glCreateShader(type);
  //   glShaderSource(ret, 1, &sourceText, NULL);
  //   glCompileShader(ret);
  //   return ret;
  // }

  // static void updatePixel(void *textureDataPtr, int width, int height, int textureRowPitch)
  // {

  //   if (!textureDataPtr)
  //     return;

  //   unsigned char *dst = (unsigned char *)textureDataPtr;
  //   for (int y = 0; y < height; ++y)
  //   {
  //     unsigned char *ptr = dst;
  //     for (int x = 0; x < width; ++x)
  //     {
  //       // Simple "plasma effect": several combined sine waves
  //       int vv = 255;

  //       // Write the texture pixel
  //       ptr[0] = vv;
  //       // ptr[1] = vv;
  //       // ptr[2] = vv;
  //       ptr[3] = vv;

  //       // To next pixel (our pixels are 4 bpp)
  //       ptr += 4;
  //     }

  //     // To next image row
  //     dst += textureRowPitch;
  //   }
  //   return;
  // }

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
#endif

  bool UnitySurfaceManager::Initialize(IUnityInterfaces *unity_interfaces)
  {
#ifdef VulkanX
    IUnityGraphics *graphics = unity_interfaces->Get<IUnityGraphics>();
    auto result = graphics->GetRenderer();

    FML_CHECK(graphics->GetRenderer() == kUnityGfxRendererVulkan)
        << "Renderer type is invalid";

    m_UnityVulkan = unity_interfaces->Get<IUnityGraphicsVulkan>();
    m_UnityVulkan->InterceptInitialization(
        InterceptVulkanInitialization, NULL);
    m_Instance = m_UnityVulkan->Instance();
    LoadVulkanAPI(m_Instance.getInstanceProcAddr, m_Instance.instance);

    UnityVulkanPluginEventConfig config_1;
    config_1.graphicsQueueAccess = kUnityVulkanGraphicsQueueAccess_DontCare;
    config_1.renderPassPrecondition = kUnityVulkanRenderPass_EnsureInside;
    config_1.flags =
        kUnityVulkanEventConfigFlag_EnsurePreviousFrameSubmission |
        kUnityVulkanEventConfigFlag_ModifiesCommandBuffersState;
    m_UnityVulkan->ConfigureEvent(1, &config_1);

    // alternative way to intercept API
    // m_UnityVulkan->InterceptVulkanAPI("vkCmdBeginRenderPass",
    // (PFN_vkVoidFunction)Hook_vkCmdBeginRenderPass);

    GrVkBackendContext vk_backend_context;
    vk_backend_context.fInstance = m_Instance.instance;
    vk_backend_context.fPhysicalDevice = m_Instance.physicalDevice;
    vk_backend_context.fDevice = m_Instance.device;
    vk_backend_context.fQueue = m_Instance.graphicsQueue;
    vk_backend_context.fGraphicsQueueIndex = m_Instance.queueFamilyIndex;
    vk_backend_context.fGetProc =
        [getInstanceProc = m_Instance.getInstanceProcAddr,
         getDeviceProc = my_vkGetDeviceProcAddr](
            const char *proc_name, VkInstance instance, VkDevice device) {
          if (device != VK_NULL_HANDLE)
          {
            return getDeviceProc(device, proc_name);
          }
          return getInstanceProc(instance, proc_name);
        };

    gr_context_ = GrContext::MakeVulkan(vk_backend_context);

#endif
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
#ifdef VulkanX
#else
    // EGLBoolean result = EGL_FALSE;

    // if (egl_display_ != EGL_NO_DISPLAY && egl_context_ != EGL_NO_CONTEXT)
    // {
    //   result = eglDestroyContext(egl_display_, egl_context_);
    //   egl_context_ = EGL_NO_CONTEXT;

    //   if (result == EGL_FALSE)
    //   {
    //     FML_LOG(ERROR) << "EGL: Failed to destroy context";
    //   }
    // }

    // // d3d11_device_ = nullptr;

    // if (egl_display_ != EGL_NO_DISPLAY &&
    //     egl_resource_context_ != EGL_NO_CONTEXT)
    // {
    //   result = eglDestroyContext(egl_display_, egl_resource_context_);
    //   egl_resource_context_ = EGL_NO_CONTEXT;

    //   if (result == EGL_FALSE)
    //   {
    //     FML_LOG(ERROR) << "EGL : Failed to destroy resource context";
    //   }
    // }

    // if (egl_display_ != EGL_NO_DISPLAY)
    // {
    //   result = eglTerminate(egl_display_);
    //   egl_display_ = EGL_NO_DISPLAY;

    //   if (result == EGL_FALSE)
    //   {
    //     FML_LOG(ERROR) << "EGL : Failed to terminate EGL";
    //   }
    // }
#endif
  }

  void UnitySurfaceManager::draw()
  {
    // int width_ = 100;
    // int height_ = 100;
    // int i = 0;
    //   sk_sp<GrContext> gr_context_ = GrContext::MakeGL();
    // UnityConsole::WriteLine(("test c++" +  std::to_string(i++)).c_str());

    // sk_sp<SkSurface> m_SkSurface = SkSurface::MakeFromBackendTexture(
    //     gr_context_.get(), m_backendTex, kBottomLeft_GrSurfaceOrigin, 1,
    //     kRGBA_8888_SkColorType, nullptr, nullptr);
    //       UnityConsole::WriteLine(("test c++" +  std::to_string(i++)).c_str());

    // SkCanvas* canvas = m_SkSurface->getCanvas();
    //       UnityConsole::WriteLine(("test c++" +  std::to_string(i++)).c_str());

    // canvas->drawColor(SK_ColorBLUE);
    //   SkPaint paint;
    //       UnityConsole::WriteLine(("test c++" +  std::to_string(i++)).c_str());

    // SkRect rect = SkRect::MakeXYWH(50, 50, 40, 60);
    // canvas->drawRect(rect, paint);

    // SkPaint paint2;
    // auto text = SkTextBlob::MakeFromString("Hello, Skia!", SkFont(nullptr, 18));
    // canvas->drawTextBlob(text.get(), 50, 25, paint2);

    // GrBackendTexture  m_backendTex =
    //       GrBackendTexture(width_, height_, GrMipMapped::kNo, textureInfo);

    //   m_SkSurface = SkSurface::MakeFromBackendTexture(
    //       gr_context_.get(), m_backendTex, kBottomLeft_GrSurfaceOrigin, 1,
    //       kRGBA_8888_SkColorType, nullptr, nullptr);
  }
  static void *g_TextureHandle = NULL;
  static int wi = 0;
  static int he = 0;
  static EGLDisplay egl_display_s;
  static EGLConfig egl_config_s;
  static EGLSurface surfaces;
  static EGLContext egl_context_s;
  static EGLContext egl_context_s_old;
  static GLenum state;
  static bool inited = false;
  static GLuint fbo_s = 0;
  static EGLSurface read_s;
  static EGLSurface draw_s;

  void *UnitySurfaceManager::SetTextureFromUnity(void *tex, int w, int h)
  {
    g_TextureHandle = tex;
    wi = w;
    he = h;
    return nullptr;
  }
  static void StoreOldCurrent(){
        read_s = eglGetCurrentSurface(EGL_READ);
      draw_s = eglGetCurrentSurface(EGL_DRAW);
      egl_display_s = eglGetCurrentDisplay();
          egl_context_s_old = eglGetCurrentContext(); //????? for get context

  }
  static bool RestoreOldCurrent() {
  return eglMakeCurrent(egl_display_s, draw_s, read_s, egl_context_s_old) == GL_TRUE;
  }
  void UnitySurfaceManager::SetUp()
  {
  StoreOldCurrent();
    GLuint gltex = (GLuint)(size_t)(g_TextureHandle);

    // egl_display_s = eglGetDisplay(EGL_DEFAULT_DISPLAY);
    // FML_CHECK(egl_display_s != EGL_NO_DISPLAY)
    //     << "Renderer type is invalid";

    // // Initialize the display connection.
    // FML_CHECK(eglInitialize(egl_display_s, nullptr, nullptr) == EGL_TRUE)
    //     << "Renderer type is invalid";

    bool success = false;

    std::tie(success, egl_config_s) = ChooseEGLConfiguration(egl_display_s);
    // FML_CHECK(success) << "Could not choose an EGL configuration.";

    EGLint attributes[] = {EGL_CONTEXT_CLIENT_VERSION, 2, EGL_NONE};
    egl_context_s = eglCreateContext(egl_display_s, egl_config_s, egl_context_s_old, attributes);
    // auto state = eglMakeCurrent(egl_display_s, EGL_NO_SURFACE, EGL_NO_SURFACE, egl_config_s) == GL_TRUE;
    // int texture;
    // glGenTextures()

    // state = eglMakeCurrent(egl_display_s, EGL_NO_SURFACE, EGL_NO_SURFACE, EGL_NO_CONTEXT) == GL_TRUE;

    auto state = eglMakeCurrent(egl_display_s, EGL_NO_SURFACE, EGL_NO_SURFACE, egl_context_s) == GL_TRUE;
    // unsigned int texture;
    // glGenTextures(1, &texture);
    // glBindTexture(GL_TEXTURE_2D, texture);
    // int rowPitch = wi * 4;
    // unsigned char *data = new unsigned char[rowPitch * he];

    // glTexImage2D(GL_TEXTURE_2D, 0, GL_RGB, wi, he, 0, GL_RGB, GL_UNSIGNED_BYTE, data);
    // state = glGetError();
    GLint old_framebuffer_binding;
    glGetIntegerv(GL_FRAMEBUFFER_BINDING, &old_framebuffer_binding);

    glGenFramebuffers(1, &fbo_s);
    glBindFramebuffer(GL_FRAMEBUFFER, fbo_s);
    glFramebufferTexture2D(GL_FRAMEBUFFER, GL_COLOR_ATTACHMENT0, GL_TEXTURE_2D,
                           gltex, 0);
    FML_CHECK(glCheckFramebufferStatus(GL_FRAMEBUFFER) ==
              GL_FRAMEBUFFER_COMPLETE);
    glBindFramebuffer(GL_FRAMEBUFFER, old_framebuffer_binding);
    // glFramebufferTexture2D(GL_FRAMEBUFFER, GL_COLOR_ATTACHMENT0, GL_TEXTURE_2D, gltex, 0);
    state = glGetError();
    RestoreOldCurrent();
    // return nullptr;
  }

  // static void createContext()
  // {
  //   if (egl_display_s != nullptr)
  //   {
  //     return;
  //   }
  //   egl_display_s = eglGetDisplay(EGL_DEFAULT_DISPLAY);
  //   FML_CHECK(egl_display_s != EGL_NO_DISPLAY)
  //       << "Renderer type is invalid";

  //   // Initialize the display connection.
  //   FML_CHECK(eglInitialize(egl_display_s, nullptr, nullptr) == EGL_TRUE)
  //       << "Renderer type is invalid";

  //   bool success = false;

  //   std::tie(success, egl_config_s) = ChooseEGLConfiguration(egl_display_s);
  //   FML_CHECK(success) << "Could not choose an EGL configuration.";

  //   EGLint attributes[] = {EGL_CONTEXT_CLIENT_VERSION, 2, EGL_NONE};
  //   egl_context_s_old = eglGetCurrentContext(); //????? for get context

  //   egl_context_s = eglCreateContext(egl_display_s, egl_config_s, egl_context_s_old, attributes);
  // }
  static int siyao= 0;
  void UnitySurfaceManager::drawxxx()
  {
    // createContext();
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
        // Simple "plasma effect": several combined sine waves
        int vv = (244 + siyao )%244;

        // Write the texture pixel
        ptr[0] = vv;
        ptr[1] = (x + y + siyao) % 244;
        ptr[2] = (x * y * siyao) % 244;
        ptr[3] = vv;

        // To next pixel (our pixels are 4 bpp)
        ptr += 4;
      }

      // To next image row
      dst += rowPitch;
    }
    GLuint gltex = (GLuint)(size_t)(g_TextureHandle);
    auto read = eglGetCurrentSurface(EGL_READ);
    auto draw = eglGetCurrentSurface(EGL_DRAW);
    auto dis = eglGetCurrentDisplay();
    auto ctx = eglGetCurrentContext();
    auto state = eglMakeCurrent(egl_display_s, EGL_NO_SURFACE, EGL_NO_SURFACE, egl_context_s) == GL_TRUE;
    glBindTexture(GL_TEXTURE_2D, gltex);
    // glBegin(GL_LINES);
    //     glVertex2f(.25,0.25);
    //     glVertex2f(.75,.75);
    // glEnd();

    glTexSubImage2D(GL_TEXTURE_2D, 0, 0, 0, width, height, GL_RGBA, GL_UNSIGNED_BYTE, data);
    // glClearColor(1.0, 0, 0, 1.0);
    // glClear(GL_COLOR_BUFFER_BIT);
    state = eglMakeCurrent(dis,  draw, read, ctx) == GL_TRUE;
    // state = eglMakeCurrent(EGL_NO_DISPLAY,  EGL_NO_DISPLAY, EGL_NO_DISPLAY, EGL_NO_CONTEXT) == GL_TRUE;
    // state = eglMakeCurrent(egl_display_s, draw_s, read_s, egl_context_s_old) == GL_TRUE;

    // state = eglMakeCurrent(egl_display_s, EGL_NO_DISPLAY, EGL_NO_DISPLAY, egl_context_s_old) == GL_TRUE;
    // state = eglMakeCurrent(egl_display_s, draw, read, egl_context_s_old) == GL_TRUE;

    delete[](unsigned char *) data;
  }

} // namespace uiwidgets
