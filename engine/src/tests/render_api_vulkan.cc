#include <math.h>
#include <string.h>

#include <map>
#include <vector>

#include "include/gpu/GrContext.h"
#include "include/gpu/vk/GrVkBackendContext.h"
#include "include/core/SkCanvas.h"
#include "include/core/SkSurface.h"
#include "include/core/SkTextBlob.h"
#include "include/effects/SkPerlinNoiseShader.h"
#include "include/gpu/GrBackendSurface.h"
#include "modules/skottie/include/Skottie.h"

#include "platform_base.h"
#include "render_api.h"

const char data[] =
    R"({"assets":[{"id":"comp_1","layers":[{"ao":0,"bm":0,"ddd":0,"ind":0,"ip":0,"ks":{"a":{"a":0,"k":[-55.922,214.156,0]},"o":{"a":0,"k":100},"p":{"s":true,"x":{"a":0,"k":960},"y":{"a":1,"k":[{"e":[910.934],"i":{"x":[0.96],"y":[0.317]},"n":["0p96_0p317_0p453_0"],"o":{"x":[0.453],"y":[0]},"s":[372.934],"t":0},{"e":[910.934],"i":{"x":[0.491],"y":[0.491]},"n":["0p491_0p491_0p405_0p405"],"o":{"x":[0.405],"y":[0.405]},"s":[910.934],"t":14},{"e":[372.934],"i":{"x":[0.328],"y":[1]},"n":["0p328_1_0p02_0p674"],"o":{"x":[0.02],"y":[0.674]},"s":[910.934],"t":16},{"t":30}]}},"r":{"a":0,"k":0},"s":{"a":1,"k":[{"e":[36.8,54.9,100],"i":{"x":[0.833,0.833,0.833],"y":[0.833,0.833,0.833]},"n":["0p833_0p833_0p167_0p167","0p833_0p833_0p167_0p167","0p833_0p833_0p167_0p167"],"o":{"x":[0.167,0.167,0.167],"y":[0.167,0.167,0.167]},"s":[43,43,100],"t":0},{"e":[56.6,27.5,100],"i":{"x":[0.833,0.833,0.833],"y":[0.833,0.833,0.833]},"n":["0p833_0p833_0p167_0p167","0p833_0p833_0p167_0p167","0p833_0p833_0p167_0p167"],"o":{"x":[0.167,0.167,0.167],"y":[0.167,0.167,0.167]},"s":[36.8,54.9,100],"t":14},{"e":[56.6,27.5,100],"i":{"x":[0.833,0.833,0.833],"y":[0.833,0.833,0.833]},"n":["0p833_0p833_0p167_0p167","0p833_0p833_0p167_0p167","0p833_0p833_0p167_0p167"],"o":{"x":[0.167,0.167,0.167],"y":[0.167,0.167,0.167]},"s":[56.6,27.5,100],"t":15},{"e":[34.58,45.3,100],"i":{"x":[0.833,0.833,0.833],"y":[0.833,0.833,0.833]},"n":["0p833_0p833_0p167_0p167","0p833_0p833_0p167_0p167","0p833_0p833_0p167_0p167"],"o":{"x":[0.167,0.167,0.167],"y":[0.167,0.167,0.167]},"s":[56.6,27.5,100],"t":16},{"e":[47.692,41.258,100],"i":{"x":[0.833,0.833,0.833],"y":[0.833,0.833,0.833]},"n":["0p833_0p833_0p167_0p167","0p833_0p833_0p167_0p167","0p833_0p833_0p167_0p167"],"o":{"x":[0.167,0.167,0.167],"y":[0.167,0.167,0.167]},"s":[34.58,45.3,100],"t":18},{"e":[43,43,100],"i":{"x":[0.833,0.833,0.833],"y":[0.833,0.833,0.833]},"n":["0p833_0p833_0p167_0p167","0p833_0p833_0p167_0p167","0p833_0p833_0p167_0p167"],"o":{"x":[0.167,0.167,0.167],"y":[0.167,0.167,0.167]},"s":[47.692,41.258,100],"t":25},{"t":30}]}},"nm":"Shape Layer 4","op":150,"shapes":[{"it":[{"d":1,"mn":"ADBE Vector Shape - Ellipse","nm":"Ellipse Path 1","p":{"a":0,"k":[0,0]},"s":{"a":0,"k":[460.156,460.156]},"ty":"el"},{"c":{"a":0,"k":[0.2,0.77,0.61,1]},"lc":1,"lj":1,"ml":4,"mn":"ADBE Vector Graphic - Stroke","nm":"Stroke 1","o":{"a":0,"k":100},"ty":"st","w":{"a":0,"k":0}},{"c":{"a":0,"k":[0.95,0.31,0.56,1]},"mn":"ADBE Vector Graphic - Fill","nm":"Fill 1","o":{"a":0,"k":100},"ty":"fl"},{"a":{"a":0,"ix":1,"k":[0,0]},"nm":"Transform","o":{"a":0,"ix":7,"k":100},"p":{"a":0,"ix":2,"k":[-55.922,-15.922]},"r":{"a":0,"ix":6,"k":0},"s":{"a":0,"ix":3,"k":[100,100]},"sa":{"a":0,"ix":5,"k":0},"sk":{"a":0,"ix":4,"k":0},"ty":"tr"}],"mn":"ADBE Vector Group","nm":"Ellipse 1","np":3,"ty":"gr"}],"sr":1,"st":0,"td":1,"ty":4},{"ao":0,"bm":0,"ddd":0,"ind":1,"ip":0,"ks":{"a":{"a":0,"k":[7.73,-318.27,0]},"o":{"a":0,"k":12},"p":{"a":1,"k":[{"e":[59.033,-256.954,0],"i":{"x":0.667,"y":1},"n":"0p667_1_0p333_0","o":{"x":0.333,"y":0},"s":[23.697,-140.59,0],"t":0,"ti":[0,0,0],"to":[0,0,0]},{"e":[23.697,-140.59,0],"i":{"x":0.667,"y":1},"n":"0p667_1_0p333_0","o":{"x":0.333,"y":0},"s":[59.033,-256.954,0],"t":15,"ti":[0,0,0],"to":[0,0,0]},{"t":30}]},"r":{"a":0,"k":0},"s":{"a":0,"k":[168.239,178.473,100]}},"nm":"Shape Layer 3","op":150,"parent":2,"shapes":[{"it":[{"d":1,"mn":"ADBE Vector Shape - Ellipse","nm":"Ellipse Path 1","p":{"a":0,"k":[0,0]},"s":{"a":0,"k":[36.539,36.539]},"ty":"el"},{"c":{"a":0,"k":[0.2,0.77,0.61,1]},"lc":1,"lj":1,"ml":4,"mn":"ADBE Vector Graphic - Stroke","nm":"Stroke 1","o":{"a":0,"k":100},"ty":"st","w":{"a":0,"k":0}},{"c":{"a":0,"k":[1,1,1,1]},"mn":"ADBE Vector Graphic - Fill","nm":"Fill 1","o":{"a":0,"k":100},"ty":"fl"},{"a":{"a":0,"ix":1,"k":[0,0]},"nm":"Transform","o":{"a":0,"ix":7,"k":100},"p":{"a":0,"ix":2,"k":[7.73,-318.27]},"r":{"a":0,"ix":6,"k":0},"s":{"a":0,"ix":3,"k":[100,100]},"sa":{"a":0,"ix":5,"k":0},"sk":{"a":0,"ix":4,"k":0},"ty":"tr"}],"mn":"ADBE Vector Group","nm":"Ellipse 1","np":3,"ty":"gr"}],"sr":1,"st":0,"tt":1,"ty":4},{"ao":0,"bm":0,"ddd":0,"ind":2,"ip":0,"ks":{"a":{"a":0,"k":[-55.922,214.156,0]},"o":{"a":0,"k":100},"p":{"s":true,"x":{"a":0,"k":960},"y":{"a":1,"k":[{"e":[910.934],"i":{"x":[0.96],"y":[0.317]},"n":["0p96_0p317_0p453_0"],"o":{"x":[0.453],"y":[0]},"s":[372.934],"t":0},{"e":[910.934],"i":{"x":[0.491],"y":[0.491]},"n":["0p491_0p491_0p405_0p405"],"o":{"x":[0.405],"y":[0.405]},"s":[910.934],"t":14},{"e":[372.934],"i":{"x":[0.328],"y":[1]},"n":["0p328_1_0p02_0p674"],"o":{"x":[0.02],"y":[0.674]},"s":[910.934],"t":16},{"t":30}]}},"r":{"a":0,"k":0},"s":{"a":1,"k":[{"e":[36.8,54.9,100],"i":{"x":[0.833,0.833,0.833],"y":[0.833,0.833,0.833]},"n":["0p833_0p833_0p167_0p167","0p833_0p833_0p167_0p167","0p833_0p833_0p167_0p167"],"o":{"x":[0.167,0.167,0.167],"y":[0.167,0.167,0.167]},"s":[43,43,100],"t":0},{"e":[56.6,27.5,100],"i":{"x":[0.833,0.833,0.833],"y":[0.833,0.833,0.833]},"n":["0p833_0p833_0p167_0p167","0p833_0p833_0p167_0p167","0p833_0p833_0p167_0p167"],"o":{"x":[0.167,0.167,0.167],"y":[0.167,0.167,0.167]},"s":[36.8,54.9,100],"t":14},{"e":[56.6,27.5,100],"i":{"x":[0.833,0.833,0.833],"y":[0.833,0.833,0.833]},"n":["0p833_0p833_0p167_0p167","0p833_0p833_0p167_0p167","0p833_0p833_0p167_0p167"],"o":{"x":[0.167,0.167,0.167],"y":[0.167,0.167,0.167]},"s":[56.6,27.5,100],"t":15},{"e":[34.58,45.3,100],"i":{"x":[0.833,0.833,0.833],"y":[0.833,0.833,0.833]},"n":["0p833_0p833_0p167_0p167","0p833_0p833_0p167_0p167","0p833_0p833_0p167_0p167"],"o":{"x":[0.167,0.167,0.167],"y":[0.167,0.167,0.167]},"s":[56.6,27.5,100],"t":16},{"e":[47.692,41.258,100],"i":{"x":[0.833,0.833,0.833],"y":[0.833,0.833,0.833]},"n":["0p833_0p833_0p167_0p167","0p833_0p833_0p167_0p167","0p833_0p833_0p167_0p167"],"o":{"x":[0.167,0.167,0.167],"y":[0.167,0.167,0.167]},"s":[34.58,45.3,100],"t":18},{"e":[43,43,100],"i":{"x":[0.833,0.833,0.833],"y":[0.833,0.833,0.833]},"n":["0p833_0p833_0p167_0p167","0p833_0p833_0p167_0p167","0p833_0p833_0p167_0p167"],"o":{"x":[0.167,0.167,0.167],"y":[0.167,0.167,0.167]},"s":[47.692,41.258,100],"t":25},{"t":30}]}},"nm":"Shape Layer 1","op":150,"shapes":[{"it":[{"d":1,"mn":"ADBE Vector Shape - Ellipse","nm":"Ellipse Path 1","p":{"a":0,"k":[0,0]},"s":{"a":0,"k":[460.156,460.156]},"ty":"el"},{"c":{"a":0,"k":[0.2,0.77,0.61,1]},"lc":1,"lj":1,"ml":4,"mn":"ADBE Vector Graphic - Stroke","nm":"Stroke 1","o":{"a":0,"k":100},"ty":"st","w":{"a":0,"k":0}},{"c":{"a":0,"k":[0.95,0.31,0.56,1]},"mn":"ADBE Vector Graphic - Fill","nm":"Fill 1","o":{"a":0,"k":100},"ty":"fl"},{"a":{"a":0,"ix":1,"k":[0,0]},"nm":"Transform","o":{"a":0,"ix":7,"k":100},"p":{"a":0,"ix":2,"k":[-55.922,-15.922]},"r":{"a":0,"ix":6,"k":0},"s":{"a":0,"ix":3,"k":[100,100]},"sa":{"a":0,"ix":5,"k":0},"sk":{"a":0,"ix":4,"k":0},"ty":"tr"}],"mn":"ADBE Vector Group","nm":"Ellipse 1","np":3,"ty":"gr"}],"sr":1,"st":0,"ty":4},{"ao":0,"bm":0,"ddd":0,"ind":3,"ip":0,"ks":{"a":{"a":0,"k":[-2,400,0]},"o":{"a":0,"k":45},"p":{"a":0,"k":[958,908,0]},"r":{"a":0,"k":0},"s":{"a":1,"k":[{"e":[39.9,51.5,100],"i":{"x":[0.893,0.893,1],"y":[0.5,-0.195,1]},"n":["0p893_0p5_0p44_0","0p893_-0p195_0p44_0","1_1_0_0"],"o":{"x":[0.44,0.44,0],"y":[0,0,0]},"s":[117.1,83.8,100],"t":0},{"e":[117.1,83.8,100],"i":{"x":[0.539,0.539,1],"y":[1,1,1]},"n":["0p539_1_0p175_0p542","0p539_1_0p175_1p294","1_1_0_0"],"o":{"x":[0.175,0.175,0],"y":[0.542,1.294,0]},"s":[39.9,51.5,100],"t":16},{"t":30}]}},"nm":"Shape Layer 2","op":150,"shapes":[{"it":[{"d":1,"mn":"ADBE Vector Shape - Ellipse","nm":"Ellipse Path 1","p":{"a":0,"k":[0,0]},"s":{"a":0,"k":[467.797,27.125]},"ty":"el"},{"c":{"a":0,"k":[0.2,0.77,0.61,1]},"lc":1,"lj":1,"ml":4,"mn":"ADBE Vector Graphic - Stroke","nm":"Stroke 1","o":{"a":0,"k":100},"ty":"st","w":{"a":0,"k":0}},{"c":{"a":0,"k":[0.18,0.62,0.5,1]},"mn":"ADBE Vector Graphic - Fill","nm":"Fill 1","o":{"a":0,"k":100},"ty":"fl"},{"a":{"a":0,"ix":1,"k":[0,0]},"nm":"Transform","o":{"a":0,"ix":7,"k":100},"p":{"a":0,"ix":2,"k":[-2,400]},"r":{"a":0,"ix":6,"k":0},"s":{"a":0,"ix":3,"k":[69.545,100]},"sa":{"a":0,"ix":5,"k":0},"sk":{"a":0,"ix":4,"k":0},"ty":"tr"}],"mn":"ADBE Vector Group","nm":"Ellipse 1","np":3,"ty":"gr"}],"sr":1,"st":0,"ty":4}]}],"ddd":0,"fr":30,"h":600,"ip":0,"layers":[{"ao":0,"bm":0,"cl":"1920x1080","ddd":0,"h":1080,"ind":0,"ip":0,"ks":{"a":{"a":0,"k":[960,540,0]},"o":{"a":0,"k":100},"p":{"a":0,"k":[400,300,0]},"r":{"a":0,"k":0},"s":{"a":0,"k":[55.556,55.556,100]}},"nm":"bouncing ball.1920x1080","op":150,"refId":"comp_1","sr":1,"st":0,"ty":0,"w":1920}],"op":30,"v":"4.5.7","w":800})";

// This plugin does not link to the Vulkan loader, easier to support multiple
// APIs and systems that don't have Vulkan support
#define VK_NO_PROTOTYPES
#include "Unity/IUnityGraphicsVulkan.h"

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
                          VkInstance instance) {
  if (!my_vkGetInstanceProcAddr && getInstanceProcAddr)
    my_vkGetInstanceProcAddr = getInstanceProcAddr;

  if (!my_vkCreateInstance)
    my_vkCreateInstance = (PFN_vkCreateInstance)my_vkGetInstanceProcAddr(
        VK_NULL_HANDLE, "vkCreateInstance");

#define LOAD_VULKAN_FUNC(fn) \
  if (!my_##fn) my_##fn = (PFN_##fn)my_vkGetInstanceProcAddr(instance, #fn)
  UNITY_USED_VULKAN_API_FUNCTIONS(LOAD_VULKAN_FUNC);
#undef LOAD_VULKAN_FUNC
}

static VKAPI_ATTR VkResult VKAPI_CALL Hook_vkCreateInstance(
    const VkInstanceCreateInfo* pCreateInfo,
    const VkAllocationCallbacks* pAllocator, VkInstance* pInstance) {
  my_vkCreateInstance = (PFN_vkCreateInstance)my_vkGetInstanceProcAddr(
      VK_NULL_HANDLE, "vkCreateInstance");
  VkResult result = my_vkCreateInstance(pCreateInfo, pAllocator, pInstance);
  if (result == VK_SUCCESS) LoadVulkanAPI(my_vkGetInstanceProcAddr, *pInstance);

  return result;
}

static VKAPI_ATTR PFN_vkVoidFunction VKAPI_CALL
Hook_vkGetInstanceProcAddr(VkInstance device, const char* funcName) {
  if (!funcName) return NULL;

#define INTERCEPT(fn) \
  if (strcmp(funcName, #fn) == 0) return (PFN_vkVoidFunction)&Hook_##fn
  INTERCEPT(vkCreateInstance);
#undef INTERCEPT

  return NULL;
}

static PFN_vkGetInstanceProcAddr UNITY_INTERFACE_API
InterceptVulkanInitialization(PFN_vkGetInstanceProcAddr getInstanceProcAddr,
                              void*) {
  my_vkGetInstanceProcAddr = getInstanceProcAddr;
  return Hook_vkGetInstanceProcAddr;
}

extern "C" void RenderAPI_Vulkan_OnPluginLoad(IUnityInterfaces* interfaces) {
  interfaces->Get<IUnityGraphicsVulkan>()->InterceptInitialization(
      InterceptVulkanInitialization, NULL);
}

class RenderAPI_Vulkan : public RenderAPI {
 public:
  RenderAPI_Vulkan();
  virtual ~RenderAPI_Vulkan() {}

  virtual void ProcessDeviceEvent(UnityGfxDeviceEventType type,
                                  IUnityInterfaces* interfaces);

  void* CreateTexture(int width, int height) override;

  void CreateTexture1(void* ptr1, int width, int height) override;

  void SetImageTexture(void* ptr) override;

  void Draw() override;

  void PreDraw() override;

  void PostDraw() override;

 private:
 private:
 private:
  IUnityGraphicsVulkan* m_UnityVulkan;
  UnityVulkanInstance m_Instance;
  sk_sp<GrContext> gr_context_;
  sk_sp<SkSurface> m_SkSurface;
  sk_sp<skottie::Animation> animation_;
};

RenderAPI* CreateRenderAPI_Vulkan() { return new RenderAPI_Vulkan(); }

RenderAPI_Vulkan::RenderAPI_Vulkan() : m_UnityVulkan(NULL) {}

void RenderAPI_Vulkan::ProcessDeviceEvent(UnityGfxDeviceEventType type,
                                          IUnityInterfaces* interfaces) {
  switch (type) {
    case kUnityGfxDeviceEventInitialize: {
      m_UnityVulkan = interfaces->Get<IUnityGraphicsVulkan>();
      m_Instance = m_UnityVulkan->Instance();

      // Make sure Vulkan API functions are loaded
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
              const char* proc_name, VkInstance instance, VkDevice device) {
            if (device != VK_NULL_HANDLE) {
              return getDeviceProc(device, proc_name);
            }
            return getInstanceProc(instance, proc_name);
          };

      gr_context_ = GrContext::MakeVulkan(vk_backend_context);

       animation_ = skottie::Animation::Make(data, strlen(data));
    }

      break;
    case kUnityGfxDeviceEventShutdown:

      m_UnityVulkan = NULL;
      m_Instance = UnityVulkanInstance();

      break;
  }
}

void* RenderAPI_Vulkan::CreateTexture(int width, int height) {
   GrBackendTexture backendTex = gr_context_->createBackendTexture(
      width, height, kRGBA_8888_SkColorType, SkColors::kCyan, GrMipMapped::kNo,
      GrRenderable::kNo);
  
    m_SkSurface = SkSurface::MakeFromBackendTexture(
       gr_context_.get(), backendTex, kBottomLeft_GrSurfaceOrigin, 1,
       kRGBA_8888_SkColorType, nullptr, nullptr);


    GrVkImageInfo info;
    backendTex.getVkImageInfo(&info);


  return info.fImage;
}

void RenderAPI_Vulkan::CreateTexture1(void* ptr1, int width, int height) 
{
  UnityVulkanImage image;

  m_UnityVulkan->AccessTexture(
      ptr1, UnityVulkanWholeImage, VkImageLayout::VK_IMAGE_LAYOUT_UNDEFINED, 0,
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

}

void RenderAPI_Vulkan::SetImageTexture(void* ptr) {
  UnityVulkanImage image;

//  m_UnityVulkan->AccessTexture(ptr, UnityVulkanWholeImage,
 //                              VkImageLayout::VK_IMAGE_LAYOUT_UNDEFINED, 0, 0,
 //                              UnityVulkanResourceAccessMode::kUnityVulkanResourceAccess_ObserveOnly, &image);

}

void draw_test1(SkCanvas* canvas) {
  canvas->drawColor(SK_ColorWHITE);

  SkPaint paint;
  paint.setStyle(SkPaint::kStroke_Style);
  paint.setStrokeWidth(4);
  paint.setColor(SK_ColorRED);

  SkRect rect = SkRect::MakeXYWH(50, 50, 40, 60);
  canvas->drawRect(rect, paint);

  SkRRect oval;
  oval.setOval(rect);
  oval.offset(40, 60);
  paint.setColor(SK_ColorBLUE);
  canvas->drawRRect(oval, paint);

  paint.setColor(SK_ColorCYAN);
  canvas->drawCircle(180, 50, 25, paint);

  rect.offset(80, 0);
  paint.setColor(SK_ColorYELLOW);
  canvas->drawRoundRect(rect, 10, 10, paint);

  SkPath path;
  path.cubicTo(768, 0, -512, 256, 256, 256);
  paint.setColor(SK_ColorGREEN);
  canvas->drawPath(path, paint);

  // canvas->drawImage(image, 128, 128, &paint);

  // SkRect rect2 = SkRect::MakeXYWH(0, 0, 40, 60);
  // canvas->drawImageRect(image, rect2, &paint);

  SkPaint paint2;
  auto text = SkTextBlob::MakeFromString("Hello, Skia!", SkFont(nullptr, 18));
  canvas->drawTextBlob(text.get(), 50, 25, paint2);
}


double t1 = 0;

void RenderAPI_Vulkan::Draw() {
  SkCanvas* canvas = m_SkSurface->getCanvas();

  canvas->clear(SK_ColorWHITE);
  draw_test1(canvas);

  //t1 += 1.0 / 60;
  //if (t1 >= animation_->duration()) {
  //  t1 = 0.0;
  //}

  //animation_->seekFrameTime(t1);
  //animation_->render(canvas);

  canvas->flush();
  canvas->getGrContext()->submit(true);
}

void RenderAPI_Vulkan::PreDraw() {}

void RenderAPI_Vulkan::PostDraw() {}

