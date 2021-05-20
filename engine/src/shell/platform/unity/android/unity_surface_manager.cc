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

#define UNITY_USED_VULKAN_API_FUNCTIONS(apply)        \
  apply(vkGetDeviceProcAddr);                         \
  apply(vkCreateInstance);                            \
  apply(vkCmdBeginRenderPass);                        \
  apply(vkCreateBuffer);                              \
  apply(vkGetPhysicalDeviceMemoryProperties);         \
  apply(vkGetBufferMemoryRequirements);               \
  apply(vkMapMemory);                                 \
  apply(vkBindBufferMemory);                          \
  apply(vkAllocateMemory);                            \
  apply(vkDestroyBuffer);                             \
  apply(vkFreeMemory);                                \
  apply(vkUnmapMemory);                               \
  apply(vkQueueWaitIdle);                             \
  apply(vkDeviceWaitIdle);                            \
  apply(vkCmdCopyBufferToImage);                      \
  apply(vkFlushMappedMemoryRanges);                   \
  apply(vkCreatePipelineLayout);                      \
  apply(vkCreateShaderModule);                        \
  apply(vkDestroyShaderModule);                       \
  apply(vkCreateGraphicsPipelines);                   \
  apply(vkCmdBindPipeline);                           \
  apply(vkCmdDraw);                                   \
  apply(vkCmdPushConstants);                          \
  apply(vkCmdBindVertexBuffers);                      \
  apply(vkDestroyPipeline);                           \
  apply(vkGetAndroidHardwareBufferPropertiesANDROID); \
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

static bool getMemoryTypeIndex(uint32_t typeBits, VkPhysicalDevice device, VkFlags quirementsMaks,
                               uint32_t &index)
{
  // const auto& memoryPropertys =
  //     VulkanManager::Get().physical->mempryProperties;
  VkPhysicalDeviceMemoryProperties memoryPropertys;
  vkGetPhysicalDeviceMemoryProperties(device, &memoryPropertys);
  for (uint32_t i = 0; i < memoryPropertys.memoryTypeCount; i++)
  {
    if ((typeBits & 1) == 1)
    {
      // Type is available, does it match user properties?
      if ((memoryPropertys.memoryTypes[i].propertyFlags &
           quirementsMaks) == quirementsMaks)
      {
        index = i;
        return true;
      }
    }
    typeBits >>= 1;
  }
  return false;
}
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
    usage.width = width;   //format.width;
    usage.layers = 1;
    usage.rfu0 = 0;
    usage.rfu1 = 0;
    usage.stride = 10;
    usage.usage = AHARDWAREBUFFER_USAGE_CPU_READ_OFTEN |
                  AHARDWAREBUFFER_USAGE_CPU_WRITE_NEVER |
                  AHARDWAREBUFFER_USAGE_GPU_COLOR_OUTPUT;
    FML_CHECK(AHardwareBuffer_allocate(&usage, &buffer) == 0);

    if (m_UnityVulkan != nullptr)
    {
      auto vkDevice = m_Instance.device;
      // auto memoryPropertys = m_Instance.memoryTypeIndex;
      // VkImage vkImage;
      bool useExternalFormat = true;

      VkResult err;

      AHardwareBuffer_Desc bufferDesc;
      AHardwareBuffer_describe(buffer, &bufferDesc);

      VkAndroidHardwareBufferFormatPropertiesANDROID formatInfo = {
          .sType =
              VK_STRUCTURE_TYPE_ANDROID_HARDWARE_BUFFER_FORMAT_PROPERTIES_ANDROID,
          .pNext = nullptr,
      };
      VkAndroidHardwareBufferPropertiesANDROID properties = {
          .sType = VK_STRUCTURE_TYPE_ANDROID_HARDWARE_BUFFER_PROPERTIES_ANDROID,
          .pNext = &formatInfo,
      };
      err = vkGetAndroidHardwareBufferPropertiesANDROID(vkDevice, buffer, &properties);

      VkExternalFormatANDROID externalFormat{
          .sType = VK_STRUCTURE_TYPE_EXTERNAL_FORMAT_ANDROID,
          .pNext = nullptr,
          .externalFormat = formatInfo.externalFormat,
      };
      VkExternalMemoryImageCreateInfo externalCreateInfo{
          .sType = VK_STRUCTURE_TYPE_EXTERNAL_MEMORY_IMAGE_CREATE_INFO,
          .pNext = &externalFormat,
          .handleTypes =
              VK_EXTERNAL_MEMORY_HANDLE_TYPE_ANDROID_HARDWARE_BUFFER_BIT_ANDROID,
      };


      VkImageCreateInfo imageInfo = {};
      imageInfo.sType = VK_STRUCTURE_TYPE_IMAGE_CREATE_INFO;
      imageInfo.pNext = &externalCreateInfo;
      imageInfo.flags = 0;
      imageInfo.imageType = VK_IMAGE_TYPE_2D;
      imageInfo.format =
          VK_FORMAT_UNDEFINED;
      imageInfo.extent = {
          bufferDesc.width,
          bufferDesc.height,
          1,
      };
      imageInfo.mipLevels = 1, imageInfo.arrayLayers = 1;
      imageInfo.samples = VK_SAMPLE_COUNT_1_BIT;
      imageInfo.tiling = VK_IMAGE_TILING_OPTIMAL;
      imageInfo.usage = VK_IMAGE_USAGE_SAMPLED_BIT;
      imageInfo.sharingMode = VK_SHARING_MODE_EXCLUSIVE;
      imageInfo.queueFamilyIndexCount = 0;
      imageInfo.pQueueFamilyIndices = nullptr;
      imageInfo.initialLayout = VK_IMAGE_LAYOUT_UNDEFINED;
      auto result = vkCreateImage(vkDevice, &imageInfo, nullptr, &vk_Image_);

      VkImportAndroidHardwareBufferInfoANDROID androidHardwareBufferInfo{
          .sType = VK_STRUCTURE_TYPE_IMPORT_ANDROID_HARDWARE_BUFFER_INFO_ANDROID,
          .pNext = nullptr,
          .buffer = buffer,
      };
      VkMemoryDedicatedAllocateInfo memoryAllocateInfo{
          .sType = VK_STRUCTURE_TYPE_MEMORY_DEDICATED_ALLOCATE_INFO,
          .pNext = &androidHardwareBufferInfo,
          .image = vk_Image_,
          .buffer = VK_NULL_HANDLE,
      };
      // android的hardbuffer位置(properties)
      VkMemoryRequirements requires;
      vkGetImageMemoryRequirements(vkDevice, vk_Image_, &requires);

      uint32_t memoryTypeIndex = 0;
      bool getIndex =
        getMemoryTypeIndex(properties.memoryTypeBits, m_Instance.physicalDevice, 0, memoryTypeIndex);
    assert(getIndex);

      VkMemoryAllocateInfo memoryInfo = {};
      memoryInfo.sType = VK_STRUCTURE_TYPE_MEMORY_ALLOCATE_INFO;
      memoryInfo.pNext = &memoryAllocateInfo;
      memoryInfo.memoryTypeIndex = memoryTypeIndex;
      memoryInfo.allocationSize = properties.allocationSize;

      auto al = vkAllocateMemory(vkDevice, &memoryInfo, nullptr, &memory);

      VkBindImageMemoryInfo bindImageInfo;
      bindImageInfo.sType = VK_STRUCTURE_TYPE_BIND_IMAGE_MEMORY_INFO;
      bindImageInfo.pNext = nullptr;
      bindImageInfo.image = vk_Image_;
      bindImageInfo.memory = memory;
      bindImageInfo.memoryOffset = 0;
      // vkBindImageMemory2KHR(vkDevice, 1, &bindImageInfo);
      vkBindImageMemory2(vkDevice, 1, &bindImageInfo);
    }

    EGLClientBuffer native_buffer = eglGetNativeClientBufferANDROID(buffer);
    assert(native_buffer);
    auto success = false;
    EGLint attrs[] = {EGL_NONE};

    image = eglCreateImageKHR(egl_display_, EGL_NO_CONTEXT,
                              EGL_NATIVE_BUFFER_ANDROID, native_buffer, attrs);

    egl_texture_ = 0;
    glGenTextures(1, &egl_texture_);
    glBindTexture(GL_TEXTURE_2D, egl_texture_);
    glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_MIN_FILTER, GL_NEAREST);
    glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_MAG_FILTER, GL_NEAREST);
    glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_WRAP_S, GL_CLAMP_TO_EDGE);
    glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_WRAP_T, GL_CLAMP_TO_EDGE);

    glGenFramebuffers(1, &fbo_);
    glBindFramebuffer(GL_FRAMEBUFFER, fbo_);

    glEGLImageTargetTexture2DOES(GL_TEXTURE_2D, image);

    glFramebufferTexture2D(GL_FRAMEBUFFER, GL_COLOR_ATTACHMENT0, GL_TEXTURE_2D, egl_texture_, 0);

    // GLuint gltex = (GLuint)(size_t)(native_texture_ptr);
    // glFramebufferTexture2D(GL_FRAMEBUFFER, GL_COLOR_ATTACHMENT0, GL_TEXTURE_2D, gltex, 0);
    FML_CHECK(glCheckFramebufferStatus(GL_FRAMEBUFFER) == GL_FRAMEBUFFER_COMPLETE);
      
    return fbo_;
  }

  void UnitySurfaceManager::DestroyRenderSurface()
  {
    if (buffer != nullptr)
    {
      AHardwareBuffer_release(buffer);
    }
    if (image)
    {
      eglDestroyImageKHR(egl_display_, image);
    }
    FML_DCHECK(fbo_ != 0);
    glDeleteFramebuffers(1, &fbo_);

    fbo_ = 0;

    eglDestroyImageKHR(egl_display_, image);

    glDeleteTextures(1, &egl_texture_);
    egl_texture_ = 0;
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
    if (egl_unity_context_ != nullptr)
    {
      return;
    }
    egl_display_ = eglGetCurrentDisplay();
    egl_unity_context_ = eglGetCurrentContext();
  }

  bool UnitySurfaceManager::Initialize(IUnityInterfaces *unity_interfaces)
  {
    auto *graphics = unity_interfaces->Get<IUnityGraphics>();
    UnityGfxRenderer renderer = graphics->GetRenderer();
    FML_DCHECK(renderer == kUnityGfxRendererOpenGLES30 ||
               renderer == kUnityGfxRendererOpenGLES20 ||
               renderer == kUnityGfxRendererVulkan);

    if (renderer == kUnityGfxRendererVulkan)
    {
      m_UnityVulkan = unity_interfaces->Get<IUnityGraphicsVulkan>();
      m_Instance = m_UnityVulkan->Instance();
      LoadVulkanAPI(m_Instance.getInstanceProcAddr, m_Instance.instance);

      egl_display_ = eglGetDisplay(EGL_DEFAULT_DISPLAY); // eglGetCurrentDisplay(); //
      
      FML_CHECK(egl_display_ != EGL_NO_DISPLAY);
      
      eglBindAPI(EGL_OPENGL_ES_API);

    }

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
