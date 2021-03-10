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
// #define VERTEX_SHADER_SRC(ver, attr, varying)                           \
//   ver                                                                   \
//       attr " highp vec3 pos;\n" attr " lowp vec4 color;\n"              \
//            "\n" varying " lowp vec4 ocolor;\n"                          \
//            "\n"                                                         \
//            "uniform highp mat4 worldMatrix;\n"                          \
//            "uniform highp mat4 projMatrix;\n"                           \
//            "\n"                                                         \
//            "void main()\n"                                              \
//            "{\n"                                                        \
//            "	gl_Position = (projMatrix * worldMatrix) * vec4(pos,1);\n" \
//            "	ocolor = color;\n"                                         \
//            "}\n"

//   static const char *kGlesVProgTextGLES2 = VERTEX_SHADER_SRC("\n", "attribute", "varying");
//   static const char *kGlesVProgTextGLES3 = VERTEX_SHADER_SRC("#version 300 es\n", "in", "out");

// #define FRAGMENT_SHADER_SRC(ver, varying, outDecl, outVar) \
//   ver                                                      \
//       outDecl                                              \
//           varying " lowp vec4 ocolor;\n"                   \
//                   "\n"                                     \
//                   "void main()\n"                          \
//                   "{\n"                                    \
//                   "	" outVar " = ocolor;\n"                \
//                   "}\n"

//   static const char *kGlesFShaderTextGLES2 = FRAGMENT_SHADER_SRC("\n", "varying", "\n", "gl_FragColor");
//   static const char *kGlesFShaderTextGLES3 = FRAGMENT_SHADER_SRC("#version 300 es\n", "in", "out lowp vec4 fragColor;\n", "fragColor");

  // enum VertexInputs
  // {
  //   kVertexInputPosition = 0,
  //   kVertexInputColor = 1
  // };

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
        egl_resource_context_(EGL_NO_CONTEXT)
  // egl_config_(nullptr)
#endif
  {
    initialize_succeeded_ = Initialize(unity_interfaces);
  }

  UnitySurfaceManager::~UnitySurfaceManager() { CleanUp(); }

  GLuint UnitySurfaceManager::CreateRenderSurface(void *native_texture_ptr)
  {
    int width = 100;
    int height = 100;
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
    gr_context_ = ShellIOManager::CreateCompatibleResourceLoadingContext(
        GrBackend::kOpenGL_GrBackend,
        GPUSurfaceGLDelegate::GetDefaultPlatformGLInterface());
    // gr_context_ = GrContext::MakeGL();

    GrGLTextureInfo textureInfo;
    textureInfo.fTarget = GR_GL_TEXTURE_2D;
    textureInfo.fID = GrGLuint((long)native_texture_ptr);
    textureInfo.fFormat = GR_GL_RGBA8;

    GrBackendTexture m_backendTex =
        GrBackendTexture(width, height, GrMipMapped::kNo, textureInfo);

    m_SkSurface = SkSurface::MakeFromBackendTexture(
        gr_context_.get(), m_backendTex, kBottomLeft_GrSurfaceOrigin, 1,
        kRGBA_8888_SkColorType, nullptr, nullptr);
        
    SkCanvas *canvas = m_SkSurface->getCanvas();

    canvas->drawColor(SK_ColorBLUE);
    SkPaint paint;

    SkRect rect = SkRect::MakeXYWH(50, 50, 40, 60);
    canvas->drawRect(rect, paint);
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
    if (egl_display_ == EGL_NO_DISPLAY)
    {
      FML_CHECK(false)
          << "Renderer type is invalid";
    }

    // Initialize the display connection.
    if (eglInitialize(egl_display_, nullptr, nullptr) != EGL_TRUE)
    {
      FML_CHECK(false)
          << "Renderer type is invalid";
    }

    auto valid_ = true;

    bool success = false;

    std::tie(success, egl_config_) = ChooseEGLConfiguration(egl_display_);
    if (!success)
    {
      FML_CHECK(false) << "Could not choose an EGL configuration.";
      // LogLastEGLError();
      // return;
    }

    EGLDisplay display = egl_display_;

    const EGLint attribs[] = {EGL_WIDTH, 1, EGL_HEIGHT, 1, EGL_NONE};

    EGLSurface surface_ = eglCreatePbufferSurface(display, egl_config_, attribs);
    success = surface_ != EGL_NO_SURFACE;

    EGLint attributes[] = {EGL_CONTEXT_CLIENT_VERSION, 2, EGL_NONE};
    egl_context_ = eglCreateContext(display, egl_config_, EGL_NO_CONTEXT, attributes);

    EGLDisplay display2 = egl_display_;

    const EGLint attribs2[] = {EGL_WIDTH, 1, EGL_HEIGHT, 1, EGL_NONE};

    surface_ = eglCreatePbufferSurface(display2, egl_config_, attribs2);
    auto xxx = surface_ != EGL_NO_SURFACE;

    //  auto egl_display_ = eglGetPlatformDisplayEXT(EGL_PLATFORM_ANGLE_ANGLE,
    //                                         EGL_DEFAULT_DISPLAY, displayAttribs);
    // IUnityGraphics *graphics = unity_interfaces->Get<IUnityGraphics>();
    // UnityGfxRenderer renderer = graphics->GetRenderer();
    // if (renderer == kUnityGfxRendererOpenGLES20)
    // {
    //   m_VertexShader = CreateShader(GL_VERTEX_SHADER, kGlesVProgTextGLES2);
    //   m_FragmentShader = CreateShader(GL_FRAGMENT_SHADER, kGlesFShaderTextGLES2);
    // }
    // else if (renderer == kUnityGfxRendererOpenGLES30)
    // {
    //   m_VertexShader = CreateShader(GL_VERTEX_SHADER, kGlesVProgTextGLES3);
    //   m_FragmentShader = CreateShader(GL_FRAGMENT_SHADER, kGlesFShaderTextGLES3);
    // }
    // else
    // {
    //   FML_CHECK(false)
    //       << "Renderer type is invalid";
    // }

    // m_Program = glCreateProgram();
    // glBindAttribLocation(m_Program, kVertexInputPosition, "pos");
    // glBindAttribLocation(m_Program, kVertexInputColor, "color");
    // glAttachShader(m_Program, m_VertexShader);
    // glAttachShader(m_Program, m_FragmentShader);
    // glLinkProgram(m_Program);
    // GLint status = 0;
    // glGetProgramiv(m_Program, GL_LINK_STATUS, &status);
    // assert(status == GL_TRUE);

    // m_UniformWorldMatrix = glGetUniformLocation(m_Program, "worldMatrix");
    // m_UniformProjMatrix = glGetUniformLocation(m_Program, "projMatrix");

    // // Create vertex buffer
    // glGenBuffers(1, &m_VertexBuffer);
    // glBindBuffer(GL_ARRAY_BUFFER, m_VertexBuffer);
    // glBufferData(GL_ARRAY_BUFFER, 1024, NULL, GL_STREAM_DRAW);

    // FML_CHECK(glGetError() == GL_NO_ERROR);
    // UnityVulkanInstance instance = vulkan->Instance();
    // VkDevice device = instance.device;

    // ID3D11Device* d3d11_device = d3d11->GetDevice();

    // IDXGIDevice* dxgi_device;
    // HRESULT hr = d3d11_device->QueryInterface(
    //     __uuidof(IDXGIDevice), reinterpret_cast<void**>(&dxgi_device));

    // FML_CHECK(SUCCEEDED(hr)) << "d3d11_device->QueryInterface(...) failed";

    // IDXGIAdapter* dxgi_adapter;
    // hr = dxgi_device->GetAdapter(&dxgi_adapter);
    // FML_CHECK(SUCCEEDED(hr)) << "dxgi_adapter->GetAdapter(...) failed";

    // dxgi_device->Release();

    // DXGI_ADAPTER_DESC adapter_desc;
    // hr = dxgi_adapter->GetDesc(&adapter_desc);
    // FML_CHECK(SUCCEEDED(hr)) << "dxgi_adapter->GetDesc(...) failed";

    // dxgi_adapter->Release();

    // EGLint displayAttribs[] = {EGL_PLATFORM_ANGLE_TYPE_ANGLE,
    //                            EGL_PLATFORM_ANGLE_TYPE_D3D11_ANGLE,
    //                            EGL_PLATFORM_ANGLE_D3D_LUID_HIGH_ANGLE,
    //                            adapter_desc.AdapterLuid.HighPart,
    //                            EGL_PLATFORM_ANGLE_D3D_LUID_LOW_ANGLE,
    //                            adapter_desc.AdapterLuid.LowPart,
    //                            EGL_PLATFORM_ANGLE_ENABLE_AUTOMATIC_TRIM_ANGLE,
    //                            EGL_TRUE,
    //                            EGL_NONE};

    // egl_display_ = eglGetPlatformDisplayEXT(EGL_PLATFORM_ANGLE_ANGLE,
    //                                         EGL_DEFAULT_DISPLAY, displayAttribs);

    // FML_CHECK(egl_display_ != EGL_NO_DISPLAY)
    //     << "EGL: Failed to get a compatible EGLdisplay";

    // if (eglInitialize(egl_display_, nullptr, nullptr) == EGL_FALSE) {
    //   FML_CHECK(false) << "EGL: Failed to initialize EGL";
    // }

    // EGLAttrib egl_device = 0;
    // eglQueryDisplayAttribEXT(egl_display_, EGL_DEVICE_EXT, &egl_device);
    // EGLAttrib angle_device = 0;
    // eglQueryDeviceAttribEXT(reinterpret_cast<EGLDeviceEXT>(egl_device),
    //                         EGL_D3D11_DEVICE_ANGLE, &angle_device);
    // d3d11_device_ = reinterpret_cast<ID3D11Device*>(angle_device);

    // const EGLint configAttributes[] = {EGL_RED_SIZE,   8, EGL_GREEN_SIZE,   8,
    //                                    EGL_BLUE_SIZE,  8, EGL_ALPHA_SIZE,   8,
    //                                    EGL_DEPTH_SIZE, 8, EGL_STENCIL_SIZE, 8,
    //                                    EGL_NONE};

    // EGLint numConfigs = 0;
    // if (eglChooseConfig(egl_display_, configAttributes, &egl_config_, 1,
    //                     &numConfigs) == EGL_FALSE ||
    //     numConfigs == 0) {
    //   FML_CHECK(false) << "EGL: Failed to choose first context";
    // }

    // const EGLint display_context_attributes[] = {EGL_CONTEXT_CLIENT_VERSION, 2,
    //                                              EGL_NONE};

    // egl_context_ = eglCreateContext(egl_display_, egl_config_, EGL_NO_CONTEXT,
    //                                 display_context_attributes);
    // if (egl_context_ == EGL_NO_CONTEXT) {
    //   FML_CHECK(false) << "EGL: Failed to create EGL context";
    // }

    // egl_resource_context_ = eglCreateContext(
    //     egl_display_, egl_config_, egl_context_, display_context_attributes);

    // if (egl_resource_context_ == EGL_NO_CONTEXT) {
    //   FML_CHECK(false) << "EGL: Failed to create EGL resource context";
    // }

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
} // namespace uiwidgets
