#include <cassert>

#include "platform_base.h"
#include "render_api.h"
#include "shell/platform/unity/uiwidgets_system.h"

// Direct3D 11 implementation of RenderAPI.

#if SUPPORT_D3D11

#include <EGL/egl.h>
#include <EGL/eglext.h>
#include <GLES2/gl2.h>
#include <GLES2/gl2ext.h>
#include <assert.h>
#include <d3d11.h>
#include <string.h>

#include <vector>

#include "TestLoadICU.h"
#include "Unity/IUnityGraphicsD3D11.h"
#include "flutter/fml/message_loop.h"
#include "txt/paragraph.h"
#include "txt/paragraph_builder.h"
#include "txt/font_collection.h"

#include "flutter/fml/synchronization/waitable_event.h"
#include "include/core/SkCanvas.h"
#include "include/core/SkSurface.h"
#include "include/core/SkTextBlob.h"
#include "include/effects/SkPerlinNoiseShader.h"
#include "include/gpu/GrBackendSurface.h"
#include "include/gpu/GrContext.h"
#include "include/gpu/gl/GrGLTypes.h"
#include "modules/skottie/include/Skottie.h"
#include "src/gpu/gl/GrGLDefines.h"
#include "third_party/dart/runtime/include/dart_tools_api.h"
#include "third_party/icu/SkLoadICU.h"

static std::shared_ptr<txt::FontCollection> _fontCollection;


class RenderAPI_D3D11 : public RenderAPI {
 public:
  RenderAPI_D3D11();
  virtual ~RenderAPI_D3D11() {}

  virtual void ProcessDeviceEvent(UnityGfxDeviceEventType type,
                                  IUnityInterfaces* interfaces);

  void* CreateTexture(int width, int height) override;
  void CreateTexture1(void* ptr1, int width, int height) override;
  void SetImageTexture(void* ptr) override;

  void Draw() override;
  void PreDraw() override;
  void PostDraw() override;

 private:
  void CreateResources();
  void ReleaseResources();
  void draw(SkCanvas* canvas);

 private:
  ID3D11Device* m_Device;
  ID3D11Device* m_DeviceSkia;

  EGLDeviceEXT m_eglDevice;
  EGLDisplay m_eglDisplay;
  EGLConfig m_eglConfig;
  EGLContext m_eglContext;

  ID3D11Texture2D* m_TextureHandle = NULL;
  int m_TextureWidth = 0;
  int m_TextureHeight = 0;

  sk_sp<GrContext> m_GrContext;
  GrBackendTexture m_backendTex;
  sk_sp<SkSurface> m_SkSurface;

  IDXGIKeyedMutex* mutex_unity;
  IDXGIKeyedMutex* mutex_skia;

  sk_sp<skottie::Animation> animation_;
  ID3D11Texture2D* image_texture_ = NULL;

  sk_sp<SkImage> image_;
};

RenderAPI* CreateRenderAPI_D3D11() { return new RenderAPI_D3D11(); }

RenderAPI_D3D11::RenderAPI_D3D11() : m_Device(NULL) {}

void RenderAPI_D3D11::ProcessDeviceEvent(UnityGfxDeviceEventType type,
                                         IUnityInterfaces* interfaces) {
  fml::MessageLoop* loop1 = nullptr;
  fml::AutoResetWaitableEvent latch1;
  fml::AutoResetWaitableEvent term1;
  std::thread thread1([&loop1, &latch1, &term1]() {
    fml::MessageLoop::EnsureInitializedForCurrentThread();
    loop1 = &fml::MessageLoop::GetCurrent();
    latch1.Signal();
    term1.Wait();
  });

  fml::MessageLoop* loop2 = nullptr;
  fml::AutoResetWaitableEvent latch2;
  fml::AutoResetWaitableEvent term2;
  std::thread thread2([&loop2, &latch2, &term2]() {
    fml::MessageLoop::EnsureInitializedForCurrentThread();
    loop2 = &fml::MessageLoop::GetCurrent();
    latch2.Signal();
    term2.Wait();
  });

  latch1.Wait();
  latch2.Wait();

  term1.Signal();
  term2.Signal();
  thread1.join();
  thread2.join();

  switch (type) {
    case kUnityGfxDeviceEventInitialize: {
      IUnityGraphicsD3D11* d3d = interfaces->Get<IUnityGraphicsD3D11>();
      m_Device = d3d->GetDevice();
      CreateResources();
      break;
    }
    case kUnityGfxDeviceEventShutdown:
      ReleaseResources();
      break;
  }
}

void RenderAPI_D3D11::CreateResources() {
  IDXGIDevice* dxgi_device;
  HRESULT hr = m_Device->QueryInterface(__uuidof(IDXGIDevice),
                                        reinterpret_cast<void**>(&dxgi_device));
  assert(SUCCEEDED(hr) && "UgpDXGISwapChain: QueryInterface(...) failed");

  IDXGIAdapter* adapter;
  hr = dxgi_device->GetAdapter(&adapter);
  assert(SUCCEEDED(hr) && "UgpDXGISwapChain: GetAdapter(...) failed");

  DXGI_ADAPTER_DESC adapter_desc;
  hr = adapter->GetDesc(&adapter_desc);
  assert(SUCCEEDED(hr) && "UgpDXGISwapChain: GetDesc(...) failed");

  adapter->Release();
  dxgi_device->Release();

  EGLint displayAttribs[] = {EGL_PLATFORM_ANGLE_TYPE_ANGLE,
                             EGL_PLATFORM_ANGLE_TYPE_D3D11_ANGLE,
                             EGL_PLATFORM_ANGLE_D3D_LUID_HIGH_ANGLE,
                             adapter_desc.AdapterLuid.HighPart,
                             EGL_PLATFORM_ANGLE_D3D_LUID_LOW_ANGLE,
                             adapter_desc.AdapterLuid.LowPart,
                             EGL_NONE};

  m_eglDisplay = eglGetPlatformDisplayEXT(EGL_PLATFORM_ANGLE_ANGLE,
                                          EGL_DEFAULT_DISPLAY, displayAttribs);

  // EGLDeviceEXT eglDevice = eglCreateDeviceANGLE(
  //    EGL_D3D11_DEVICE_ANGLE, reinterpret_cast<void*>(m_Device), nullptr);

  // m_eglDisplay = eglGetPlatformDisplayEXT(EGL_PLATFORM_DEVICE_EXT,
  //                                                eglDevice, nullptr);

  eglInitialize(m_eglDisplay, nullptr, nullptr);

  EGLAttrib angleDevice = 0;
  eglQueryDisplayAttribEXT(m_eglDisplay, EGL_DEVICE_EXT, &angleDevice);

  EGLAttrib device = 0;
  eglQueryDeviceAttribEXT(reinterpret_cast<EGLDeviceEXT>(angleDevice),
                          EGL_D3D11_DEVICE_ANGLE, &device);

  m_DeviceSkia = reinterpret_cast<ID3D11Device*>(device);

  EGLConfig config;
  int num_config;
  eglGetConfigs(m_eglDisplay, &config, 1, &num_config);

  const EGLint contextAttributes[] = {EGL_CONTEXT_CLIENT_VERSION, 2, EGL_NONE};
  m_eglContext =
      eglCreateContext(m_eglDisplay, config, nullptr, contextAttributes);

  eglMakeCurrent(m_eglDisplay, EGL_NO_SURFACE, EGL_NO_SURFACE, m_eglContext);

  m_GrContext = GrContext::MakeGL();

  animation_ =
      skottie::Animation::MakeFromFile(R"(C:\Users\kgdev\skottie.txt)");

  // if (rdoc_api) rdoc_api->StartFrameCapture(m_DeviceSkia, NULL);
}

void RenderAPI_D3D11::ReleaseResources() {}

void* RenderAPI_D3D11::CreateTexture(int width, int height) {
  ID3D11Texture2D* d3d11_texture = nullptr;  // todo: keep reference.
  CD3D11_TEXTURE2D_DESC desc(
      DXGI_FORMAT_R8G8B8A8_UNORM, width, height, 1, 1,
      D3D11_BIND_SHADER_RESOURCE | D3D11_BIND_RENDER_TARGET,
      D3D11_USAGE_DEFAULT, 0, 1, 0, D3D11_RESOURCE_MISC_SHARED);

  assert(
      SUCCEEDED(m_DeviceSkia->CreateTexture2D(&desc, nullptr, &d3d11_texture)));

  const EGLint attribs[] = {EGL_NONE};

  EGLImage image =
      eglCreateImageKHR(m_eglDisplay, EGL_NO_CONTEXT, EGL_D3D11_TEXTURE_ANGLE,
                        static_cast<EGLClientBuffer>(d3d11_texture), attribs);

  GLuint texture;
  glGenTextures(1, &texture);
  glBindTexture(GL_TEXTURE_2D, texture);
  glEGLImageTargetTexture2DOES(GL_TEXTURE_2D, image);

  GrGLTextureInfo textureInfo;
  textureInfo.fTarget = GR_GL_TEXTURE_2D;
  textureInfo.fID = texture;
  textureInfo.fFormat = GR_GL_RGBA8;

  m_backendTex = GrBackendTexture(width, height, GrMipMapped::kNo, textureInfo);

  m_GrContext = GrContext::MakeGL();
  m_SkSurface = SkSurface::MakeFromBackendTexture(
      m_GrContext.get(), m_backendTex, kBottomLeft_GrSurfaceOrigin, 1,
      kRGBA_8888_SkColorType, nullptr, nullptr);

  IDXGIResource* image_resource;
  HRESULT hr = d3d11_texture->QueryInterface(
      __uuidof(IDXGIResource), reinterpret_cast<void**>(&image_resource));
  assert(SUCCEEDED(hr) &&
         "UgpDXGISwapChain: QueryInterface(IDXGIResource1) failed");

  HANDLE shared_image_handle;
  hr = image_resource->GetSharedHandle(&shared_image_handle);
  assert(SUCCEEDED(hr) && "UgpDXGISwapChain: GetSharedHandle() failed");

  image_resource->Release();

  // hr = d3d11_texture->QueryInterface(__uuidof(IDXGIKeyedMutex),
  //                                   (void**)&mutex_skia);
  // assert(SUCCEEDED(hr));

  IDXGIResource* dxgiResource;
  m_Device->OpenSharedResource(shared_image_handle, __uuidof(ID3D11Resource),
                               (void**)(&dxgiResource));

  ID3D11Texture2D* texture_unity;
  dxgiResource->QueryInterface(__uuidof(ID3D11Texture2D),
                               (void**)(&texture_unity));

  dxgiResource->Release();

  // hr = texture_unity->QueryInterface(__uuidof(IDXGIKeyedMutex),
  //                                   (LPVOID*)&mutex_unity);
  // assert(SUCCEEDED(hr));

  return texture_unity;
}

void RenderAPI_D3D11::CreateTexture1(void* ptr1, int width, int height) {
  ID3D11Texture2D* d3d11_texture = (ID3D11Texture2D*)ptr1;
  IDXGIResource* image_resource;
  HRESULT hr = d3d11_texture->QueryInterface(
      __uuidof(IDXGIResource), reinterpret_cast<void**>(&image_resource));
  assert(SUCCEEDED(hr) &&
         "UgpDXGISwapChain: QueryInterface(IDXGIResource1) failed");

  HANDLE shared_image_handle;
  hr = image_resource->GetSharedHandle(&shared_image_handle);
  assert(SUCCEEDED(hr) && "UgpDXGISwapChain: GetSharedHandle() failed");

  image_resource->Release();

  IDXGIResource* dxgiResource;
  m_DeviceSkia->OpenSharedResource(
      shared_image_handle, __uuidof(ID3D11Resource), (void**)(&dxgiResource));

  ID3D11Texture2D* texture_unity;
  dxgiResource->QueryInterface(__uuidof(ID3D11Texture2D),
                               (void**)(&texture_unity));

  dxgiResource->Release();

  // image_texture_ = texture_unity;

  const EGLint attribs[] = {EGL_NONE};

  EGLImage image =
      eglCreateImageKHR(m_eglDisplay, EGL_NO_CONTEXT, EGL_D3D11_TEXTURE_ANGLE,
                        static_cast<EGLClientBuffer>(texture_unity), attribs);

  GLuint texture;
  glGenTextures(1, &texture);
  glBindTexture(GL_TEXTURE_2D, texture);
  glEGLImageTargetTexture2DOES(GL_TEXTURE_2D, image);

  GrGLTextureInfo textureInfo;
  textureInfo.fTarget = GR_GL_TEXTURE_2D;
  textureInfo.fID = texture;
  textureInfo.fFormat = GR_GL_RGBA8;

  m_backendTex = GrBackendTexture(width, height, GrMipMapped::kNo, textureInfo);

  m_SkSurface = SkSurface::MakeFromBackendTexture(
      m_GrContext.get(), m_backendTex, kBottomLeft_GrSurfaceOrigin, 1,
      kRGBA_8888_SkColorType, nullptr, nullptr);
}

void RenderAPI_D3D11::SetImageTexture(void* ptr) {
  ID3D11Texture2D* d3d11_texture = (ID3D11Texture2D*)ptr;

  IDXGIResource* image_resource;
  HRESULT hr = d3d11_texture->QueryInterface(
      __uuidof(IDXGIResource), reinterpret_cast<void**>(&image_resource));
  assert(SUCCEEDED(hr) &&
         "UgpDXGISwapChain: QueryInterface(IDXGIResource1) failed");

  HANDLE shared_image_handle;
  hr = image_resource->GetSharedHandle(&shared_image_handle);
  assert(SUCCEEDED(hr) && "UgpDXGISwapChain: GetSharedHandle() failed");

  image_resource->Release();

  IDXGIResource* dxgiResource;
  m_DeviceSkia->OpenSharedResource(
      shared_image_handle, __uuidof(ID3D11Resource), (void**)(&dxgiResource));

  ID3D11Texture2D* texture_unity;
  dxgiResource->QueryInterface(__uuidof(ID3D11Texture2D),
                               (void**)(&texture_unity));

  dxgiResource->Release();

  image_texture_ = texture_unity;

  const EGLint attribs[] = {EGL_NONE};

  EGLImage image =
      eglCreateImageKHR(m_eglDisplay, EGL_NO_CONTEXT, EGL_D3D11_TEXTURE_ANGLE,
                        static_cast<EGLClientBuffer>(image_texture_), attribs);

  GLuint texture;
  glGenTextures(1, &texture);
  glBindTexture(GL_TEXTURE_2D, texture);
  glEGLImageTargetTexture2DOES(GL_TEXTURE_2D, image);

  GrGLTextureInfo textureInfo;
  textureInfo.fTarget = GR_GL_TEXTURE_2D;
  textureInfo.fID = texture;
  textureInfo.fFormat = GR_GL_RGBA8;

  GrBackendTexture backendTex =
      GrBackendTexture(1000, 1000, GrMipMapped::kNo, textureInfo);

  image_ = SkImage::MakeFromTexture(
      m_GrContext.get(), backendTex, kBottomLeft_GrSurfaceOrigin,
      kRGBA_8888_SkColorType, kOpaque_SkAlphaType, nullptr);
}

std::shared_ptr<txt::FontCollection> GetTestFontCollection();
std::unique_ptr<txt::Paragraph> testParagraph;
void RenderAPI_D3D11::draw(SkCanvas* canvas) {
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

  canvas->drawImage(image_, 128, 128, &paint);

  SkRect rect2 = SkRect::MakeXYWH(0, 0, 40, 60);
  canvas->drawImageRect(image_, rect2, &paint);

  SkPaint paint2;
  auto text = SkTextBlob::MakeFromString("Hello, Skia!", SkFont(nullptr, 18));
  canvas->drawTextBlob(text.get(), 50, 25, paint2);

  if (testParagraph == NULL) {
    TestLoadICU();
    _fontCollection = GetTestFontCollection();

    txt::ParagraphStyle style;
    txt::FontCollection tf;
    txt::TextStyle ts;
    // style.
    style.font_family = "Arial";
    ts.font_size = 28;
    ts.height = 4.0;

    std::unique_ptr<txt::ParagraphBuilder> pb =
        txt::ParagraphBuilder::CreateTxtBuilder(style, _fontCollection);
    ts.font_families.clear();
    ts.font_families.push_back("Arial");
    ts.color = SK_ColorBLACK;

    pb->PushStyle(ts);
    std::u16string s16 = u"Hello, some text.你好！";
    pb->AddText(s16);
    testParagraph = pb->Build();
    testParagraph->Layout(500);
  }

  // canvas->drawColor(SK_ColorWHITE);
  testParagraph->Paint(canvas, 10, 200);
}

void draw1(SkCanvas* canvas) {
  canvas->clear(SK_ColorWHITE);
  SkPaint paint;
  paint.setShader(
      SkPerlinNoiseShader::MakeFractalNoise(0.05f, 0.05f, 4, 0.0f, nullptr));
  canvas->drawPaint(paint);
}

double t = 0;

void RenderAPI_D3D11::Draw() {
  // mutex_skia->AcquireSync(0, 5000);

  SkCanvas* canvas = m_SkSurface->getCanvas();
  draw(canvas);

  /* canvas->clear(SK_ColorWHITE);

   t += 1.0 / 60;
   if (t >= animation_->duration()) {
     t = 0.0;
   }

   animation_->seekFrameTime(t);
   animation_->render(canvas);*/

  // SkRect rect = SkRect::MakeLTRB(100, 100, 200, 200);
  // canvas->drawImageRect(image_, rect, nullptr);

  // mutex_skia->ReleaseSync(0);
  canvas->flush();
  canvas->getGrContext()->submit(true);

  // if (rdoc_api) rdoc_api->EndFrameCapture(m_DeviceSkia, NULL);
}

void RenderAPI_D3D11::PreDraw() {
  // mutex_unity->AcquireSync(0, 5000);
}

void RenderAPI_D3D11::PostDraw() {
  // mutex_unity->ReleaseSync(0);
}

using namespace txt;

std::shared_ptr<txt::FontCollection> GetTestFontCollection() {
  std::shared_ptr<txt::FontCollection> collection =
      std::make_shared<txt::FontCollection>();
  collection->SetupDefaultFontManager();
  return collection;
}

#endif  // #if SUPPORT_D3D11
