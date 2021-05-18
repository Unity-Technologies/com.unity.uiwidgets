#include "unity_surface_manager.h"

#include <flutter/fml/logging.h>
#include <EGL/egl.h>
#include <EGL/eglext.h>
#include <GLES2/gl2.h>
#include <GLES2/gl2ext.h>

#include <vulkan/vulkan.h>
#include <vulkan/vulkan_core.h>
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

  GLuint UnitySurfaceManager::CreateRenderSurface(void *native_texture_ptr)
  {
    if (m_UnityVulkan != nullptr)
    {
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

      int width = 100;
      int height = 100;
      GrBackendTexture backendTex(width, height, info);

      m_SkSurface = SkSurface::MakeFromBackendTexture(
          gr_context_.get(), backendTex, kBottomLeft_GrSurfaceOrigin, 1,
          kRGBA_8888_SkColorType, nullptr, nullptr);
      return 0;
    }
    else
    {
      GLint old_framebuffer_binding;
      glGetIntegerv(GL_FRAMEBUFFER_BINDING, &old_framebuffer_binding);

      glGenFramebuffers(1, &fbo_);
      glBindFramebuffer(GL_FRAMEBUFFER, fbo_);

      GLuint gltex = (GLuint)(size_t)(native_texture_ptr);
      glFramebufferTexture2D(GL_FRAMEBUFFER, GL_COLOR_ATTACHMENT0, GL_TEXTURE_2D, gltex, 0);
      FML_CHECK(glCheckFramebufferStatus(GL_FRAMEBUFFER) == GL_FRAMEBUFFER_COMPLETE);

      glBindFramebuffer(GL_FRAMEBUFFER, old_framebuffer_binding);

      return fbo_;
    }
  }

  void UnitySurfaceManager::DestroyRenderSurface()
  {
    FML_DCHECK(fbo_ != 0);
    glDeleteFramebuffers(1, &fbo_);
    fbo_ = 0;
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

  static const int DEV_W = 16, DEV_H = 16;

  bool getMemoryTypeIndex(uint32_t typeBits, VkPhysicalDevice device, VkFlags quirementsMaks,
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
      // auto VkPhysicalDevice = m_Instance.physicalDevice;
      // VkPhysicalDeviceMemoryProperties mempryProperties;
      // vkGetPhysicalDeviceMemoryProperties(VkPhysicalDevice, &mempryProperties);

      auto vkDevice = m_Instance.device;
      // auto memoryPropertys = m_Instance.memoryTypeIndex;
      VkImage vkImage;
      bool useExternalFormat = true;

      AHardwareBuffer *buffer = nullptr;

      AHardwareBuffer_Desc usage = {};
      // filling in the usage for HardwareBuffer
      usage.format = AHARDWAREBUFFER_FORMAT_R8G8B8A8_UNORM;
      usage.height = 10000; //format.height;
      usage.width = 10000;  //format.width;
      usage.layers = 1;
      usage.rfu0 = 0;
      usage.rfu1 = 0;
      usage.stride = 0;
      usage.usage = AHARDWAREBUFFER_USAGE_CPU_READ_NEVER |
                    AHARDWAREBUFFER_USAGE_CPU_WRITE_NEVER |
                    AHARDWAREBUFFER_USAGE_GPU_SAMPLED_IMAGE |
                    AHARDWAREBUFFER_USAGE_GPU_COLOR_OUTPUT;
      auto error = AHardwareBuffer_allocate(&usage, &buffer);
      VkResult err;

      //  GrVkImageInfo info(
      //       image.image, GrVkAlloc(image.memory.memory, image.memory.offset, image.memory.size, image.memory.flags),
      //       image.tiling,
      //       image.layout,
      //       image.format,
      //       image.mipCount);

      // AHardwareBuffer_Desc bufferDesc;
      // AHardwareBuffer_describe(buffer, &bufferDesc);

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
      bool check = false;
      check = VK_FORMAT_R8G8B8A8_UNORM == formatInfo.format;
      check = VK_FORMAT_UNDEFINED == formatInfo.format;

      VkExternalFormatANDROID externalFormat{
          .sType = VK_STRUCTURE_TYPE_EXTERNAL_FORMAT_ANDROID,
          .pNext = nullptr,
          .externalFormat = formatInfo.externalFormat,
      };
      VkExternalMemoryImageCreateInfo externalCreateInfo{
          .sType = VK_STRUCTURE_TYPE_EXTERNAL_MEMORY_IMAGE_CREATE_INFO,
          .pNext = useExternalFormat ? &externalFormat : nullptr,
          .handleTypes =
              VK_EXTERNAL_MEMORY_HANDLE_TYPE_ANDROID_HARDWARE_BUFFER_BIT_ANDROID,
      };
      VkImageUsageFlags usageFlags = VK_IMAGE_USAGE_SAMPLED_BIT |
                                     VK_IMAGE_USAGE_TRANSFER_SRC_BIT |
                                     VK_IMAGE_USAGE_TRANSFER_DST_BIT;
      if (true)
      { //forWrite) {
        usageFlags |= VK_IMAGE_USAGE_COLOR_ATTACHMENT_BIT;
      }

      VkImageCreateInfo imageInfo = {};
      imageInfo.sType = VK_STRUCTURE_TYPE_IMAGE_CREATE_INFO;
      imageInfo.pNext = &externalCreateInfo;
      imageInfo.flags = 0;
      imageInfo.imageType = VK_IMAGE_TYPE_2D;
      imageInfo.format =
          formatInfo.format;
      imageInfo.extent = {
          100, //bufferDesc.width,
          100, //bufferDesc.height,
          1,
      };
      imageInfo.mipLevels = 1, imageInfo.arrayLayers = 1;
      imageInfo.samples = VK_SAMPLE_COUNT_1_BIT;
      imageInfo.tiling = VK_IMAGE_TILING_OPTIMAL;
      imageInfo.usage = usageFlags;
      imageInfo.sharingMode = VK_SHARING_MODE_EXCLUSIVE;
      imageInfo.queueFamilyIndexCount = 0;
      imageInfo.pQueueFamilyIndices = 0;
      imageInfo.initialLayout = VK_IMAGE_LAYOUT_UNDEFINED;
      auto result = vkCreateImage(vkDevice, &imageInfo, nullptr, &vkImage);

      VkImportAndroidHardwareBufferInfoANDROID androidHardwareBufferInfo{
          .sType = VK_STRUCTURE_TYPE_IMPORT_ANDROID_HARDWARE_BUFFER_INFO_ANDROID,
          .pNext = nullptr,
          .buffer = buffer,
      };
      VkMemoryDedicatedAllocateInfo memoryAllocateInfo{
          .sType = VK_STRUCTURE_TYPE_MEMORY_DEDICATED_ALLOCATE_INFO,
          .pNext = &androidHardwareBufferInfo,
          .image = vkImage,
          .buffer = VK_NULL_HANDLE,
      };
      // android的hardbuffer位置(properties)
      VkMemoryRequirements requires;
      vkGetImageMemoryRequirements(vkDevice, vkImage, &requires);

      VkPhysicalDeviceMemoryProperties2 phyDevMemProps;
      phyDevMemProps.sType = VK_STRUCTURE_TYPE_PHYSICAL_DEVICE_MEMORY_PROPERTIES_2;
      phyDevMemProps.pNext = nullptr;

      uint32_t typeIndex = 0;
      uint32_t heapIndex = 0;
      bool foundHeap = false;
      vkGetPhysicalDeviceMemoryProperties2(m_Instance.physicalDevice, &phyDevMemProps);
      uint32_t memTypeCnt = phyDevMemProps.memoryProperties.memoryTypeCount;
      for (uint32_t i = 0; i < memTypeCnt && !foundHeap; ++i)
      {
        if (properties.memoryTypeBits & (1 << i))
        {
          const VkPhysicalDeviceMemoryProperties &pdmp = phyDevMemProps.memoryProperties;
          uint32_t supportedFlags = pdmp.memoryTypes[i].propertyFlags &
                                    VK_MEMORY_PROPERTY_DEVICE_LOCAL_BIT;
          if (supportedFlags == VK_MEMORY_PROPERTY_DEVICE_LOCAL_BIT)
          {
            typeIndex = i;
            heapIndex = pdmp.memoryTypes[i].heapIndex;
            foundHeap = true;
          }
        }
      }

      uint32_t memoryTypeIndex = 0;
      // VkPhysicalDeviceMemoryProperties memoryPropertys; //???

      bool getIndex =
          getMemoryTypeIndex(properties.memoryTypeBits, m_Instance.physicalDevice, 0, memoryTypeIndex); //??
      assert(getIndex);
      VkMemoryAllocateInfo memoryInfo = {};
      memoryInfo.sType = VK_STRUCTURE_TYPE_MEMORY_ALLOCATE_INFO;
      memoryInfo.pNext = &memoryAllocateInfo;
      memoryInfo.memoryTypeIndex = memoryTypeIndex;
      memoryInfo.allocationSize = properties.allocationSize;

      VkDeviceMemory memory; //???

      vkAllocateMemory(vkDevice, &memoryInfo, nullptr, &memory);

      VkBindImageMemoryInfo bindImageInfo;
      bindImageInfo.sType = VK_STRUCTURE_TYPE_BIND_IMAGE_MEMORY_INFO;
      bindImageInfo.pNext = nullptr;
      bindImageInfo.image = vkImage;
      bindImageInfo.memory = memory;
      bindImageInfo.memoryOffset = 0;
      // vkBindImageMemory2KHR(vkDevice, 1, &bindImageInfo);
      vkBindImageMemory2(vkDevice, 1, &bindImageInfo);

      // android绑定AHardwareBuffer与egl image
      EGLClientBuffer native_buffer = eglGetNativeClientBufferANDROID(buffer);
      assert(native_buffer);
      auto success = false;

      egl_display_ = eglGetDisplay(EGL_DEFAULT_DISPLAY); // eglGetCurrentDisplay(); //
      auto xx2 = egl_display_ == EGL_NO_DISPLAY;
      auto xxxx = eglInitialize(egl_display_, nullptr, nullptr) == EGL_TRUE;
      eglBindAPI(EGL_OPENGL_ES_API);
      std::tie(success, egl_config_) = ChooseEGLConfiguration(egl_display_);

      std::tie(success, egl_context_) = CreateContext(egl_display_, egl_config_, EGL_NO_CONTEXT);
      std::tie(success, egl_resource_context_) = CreateContext(egl_display_, egl_config_, egl_context_);
      eglMakeCurrent(egl_display_, EGL_NO_SURFACE, EGL_NO_SURFACE, egl_context_);
      error = eglGetError();
      GLuint mTexture = 0;
      glGenTextures(1, &mTexture);
      error = eglGetError();
      glGenFramebuffers(1, &fbo_);
      error = eglGetError();
      glBindFramebuffer(GL_FRAMEBUFFER, fbo_);
      error = eglGetError();

      glFramebufferTexture2D(GL_FRAMEBUFFER, GL_COLOR_ATTACHMENT0, GL_TEXTURE_2D, mTexture, 0);
      error = eglGetError();

      EGLint attrs[] = {EGL_NONE};
      EGLImageKHR image = eglCreateImageKHR(egl_display_, EGL_NO_CONTEXT,
                                            EGL_NATIVE_BUFFER_ANDROID, native_buffer, attrs);
      // assert(image != EGL_NO_IMAGE_KHR);
      if (image == EGL_NO_IMAGE_KHR)
      {
        int32_t errorId = eglGetError();
        // logMessage(LogLevel::error, "not create image,error id" + errorId);
      }

      glBindTexture(GL_TEXTURE_2D, mTexture);
      error = eglGetError();

      glEGLImageTargetTexture2DOES(GL_TEXTURE_2D, image);
      error = eglGetError();
      // glBindTexture(bindType, 0);

      glFramebufferTexture2D(GL_FRAMEBUFFER, GL_COLOR_ATTACHMENT0, GL_TEXTURE_2D, mTexture, 0);
      error = eglGetError();

      GLenum x = glCheckFramebufferStatus(GL_FRAMEBUFFER);
      auto xb = x == GL_FRAMEBUFFER_COMPLETE;

      return success;
    }
    else
    {
      // Make sure Vulkan API functions are loaded

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
      return success;
    }
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
