#include "unity_surface_manager.h"

// #include <d3d11.h>
// #include <dxgi.h>
#include <flutter/fml/logging.h>

#include "Unity/IUnityGraphics.h"
// #include "Unity/IUnityGraphicsD3D11.h"
#include "src/shell/platform/unity/unity_console.h"

namespace uiwidgets
{

  UnitySurfaceManager::UnitySurfaceManager(IUnityInterfaces *unity_interfaces)
      : egl_display_(EGL_NO_DISPLAY),
        egl_context_(EGL_NO_CONTEXT),
        egl_resource_context_(EGL_NO_CONTEXT)
  // ,
  // egl_config_(nullptr)
  {
    initialize_succeeded_ = Initialize(unity_interfaces);
  }

  UnitySurfaceManager::~UnitySurfaceManager() { CleanUp(); }

  GLuint UnitySurfaceManager::CreateRenderSurface(void *native_texture_ptr)
  {

    UnityConsole::WriteLine("test c++");
    int i = 0;
    EGLContext current_ctx = eglGetCurrentContext();
    UnityConsole::WriteLine(("test c++" + std::to_string(i++)).c_str());

    EGLDisplay hdc = eglGetCurrentDisplay();
    // wglGetCurrentDC();
    UnityConsole::WriteLine(("test c++" + std::to_string(i++)).c_str());

    void *texture1_ptr_ = native_texture_ptr;
    UnityConsole::WriteLine(("test c++" + std::to_string(i++)).c_str());

    void *surface_texture1_ptr_ = texture1_ptr_;
    textureInfo.fTarget = GR_GL_TEXTURE_2D;
    textureInfo.fID = GrGLuint((long)texture1_ptr_);
    textureInfo.fFormat = GR_GL_RGBA8;
    UnityConsole::WriteLine(("test c++" + std::to_string(i++)).c_str());

     m_backendTex =
        GrBackendTexture(100, 100, GrMipMapped::kNo, textureInfo);
    UnityConsole::WriteLine(("test c++??" + std::to_string(i++)).c_str());

    // sk_sp<GrContext> gr_context_ = GrContext::MakeGL();
    //       UnityConsole::WriteLine(("test c++" +  std::to_string(i++)).c_str());

    // sk_sp<SkSurface> m_SkSurface = SkSurface::MakeFromBackendTexture(
    //     gr_context_.get(), m_backendTex, kBottomLeft_GrSurfaceOrigin, 1,
    //     kRGBA_8888_SkColorType, nullptr, nullptr);
    //       UnityConsole::WriteLine(("test c++" +  std::to_string(i++)).c_str());

    // SkCanvas* canvas = m_SkSurface->getCanvas();
    //       UnityConsole::WriteLine(("test c++" +  std::to_string(i++)).c_str());

    // canvas->drawColor(SK_ColorBLUE);
    // //   SkPaint paint;
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

    // width_ = width;
    // height_ = height;

    // ID3D11Texture2D* d3d11_texture =
    //     static_cast<ID3D11Texture2D*>(native_texture_ptr);
    // IDXGIResource* image_resource;
    // HRESULT hr = d3d11_texture->QueryInterface(
    //     __uuidof(IDXGIResource), reinterpret_cast<void**>(&image_resource));
    // FML_CHECK(SUCCEEDED(hr)) << "UnitySurfaceManager: QueryInterface() failed";

    // HANDLE shared_image_handle;
    // hr = image_resource->GetSharedHandle(&shared_image_handle);
    // FML_CHECK(SUCCEEDED(hr)) << "UnitySurfaceManager: GetSharedHandle() failed";

    // image_resource->Release();

    // FML_CHECK(shared_image_handle != nullptr)
    //     << "UnitySurfaceManager: shared_image_handle is nullptr, miscFlags "
    //        "D3D11_RESOURCE_MISC_SHARED is needed";

    // IDXGIResource* dxgi_resource;
    // hr = d3d11_device_->OpenSharedResource(
    //     shared_image_handle, __uuidof(ID3D11Resource),
    //     reinterpret_cast<void**>(&dxgi_resource));
    // FML_CHECK(SUCCEEDED(hr))
    //     << "UnitySurfaceManager: failed to open shared resource";

    // ID3D11Texture2D* image_texture;
    // hr = dxgi_resource->QueryInterface(__uuidof(ID3D11Texture2D),
    //                                    reinterpret_cast<void**>(&image_texture));
    // FML_CHECK(SUCCEEDED(hr))
    //     << "UnitySurfaceManager: failed to query interface ID3D11Texture2D";

    // dxgi_resource->Release();

    // const EGLint attribs[] = {EGL_NONE};
    // FML_DCHECK(fbo_egl_image_ == nullptr);
    // fbo_egl_image_ =
    //     eglCreateImageKHR(egl_display_, EGL_NO_CONTEXT, EGL_D3D11_TEXTURE_ANGLE,
    //                       static_cast<EGLClientBuffer>(image_texture), attribs);
    // FML_CHECK(fbo_egl_image_ != EGL_NO_IMAGE_KHR);

    // image_texture->Release();

    // GLint old_texture_binding_2d;
    // glGetIntegerv(GL_TEXTURE_BINDING_2D, &old_texture_binding_2d);

    // FML_DCHECK(fbo_texture_ == 0);
    // glGenTextures(1, &fbo_texture_);
    // glBindTexture(GL_TEXTURE_2D, fbo_texture_);
    // glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_MIN_FILTER, GL_NEAREST);
    // glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_MAG_FILTER, GL_NEAREST);
    // glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_WRAP_S, GL_CLAMP_TO_EDGE);
    // glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_WRAP_T, GL_CLAMP_TO_EDGE);
    // glEGLImageTargetTexture2DOES(GL_TEXTURE_2D, fbo_egl_image_);
    // glBindTexture(GL_TEXTURE_2D, old_texture_binding_2d);

    // GLint old_framebuffer_binding;
    // glGetIntegerv(GL_FRAMEBUFFER_BINDING, &old_framebuffer_binding);

    // FML_DCHECK(fbo_ == 0);
    // glGenFramebuffers(1, &fbo_);
    // glBindFramebuffer(GL_FRAMEBUFFER, fbo_);
    // glFramebufferTexture2D(GL_FRAMEBUFFER, GL_COLOR_ATTACHMENT0, GL_TEXTURE_2D,
    //                        fbo_texture_, 0);
    // FML_CHECK(glCheckFramebufferStatus(GL_FRAMEBUFFER) ==
    //           GL_FRAMEBUFFER_COMPLETE);
    // glBindFramebuffer(GL_FRAMEBUFFER, old_framebuffer_binding);

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
    return eglMakeCurrent(egl_display_, EGL_NO_SURFACE, EGL_NO_SURFACE,
                          EGL_NO_CONTEXT) == EGL_TRUE;
  }

  bool UnitySurfaceManager::MakeCurrent(const EGLSurface surface)
  {
    int width_ = 100;
    int height_ = 100;
    UnityConsole::WriteLine("test c++: clear current");

    EGLBoolean a =  eglMakeCurrent(egl_display_, surface, surface, egl_context_) ==
           EGL_TRUE;

int i = 0;
      sk_sp<GrContext> gr_context_ = GrContext::MakeGL();
    UnityConsole::WriteLine(("test c++" +  std::to_string(i++)).c_str());

    sk_sp<SkSurface> m_SkSurface = SkSurface::MakeFromBackendTexture(
        gr_context_.get(), m_backendTex, kBottomLeft_GrSurfaceOrigin, 1,
        kRGBA_8888_SkColorType, nullptr, nullptr);
          UnityConsole::WriteLine(("test c++" +  std::to_string(i++)).c_str());

    SkCanvas* canvas = m_SkSurface->getCanvas();
          UnityConsole::WriteLine(("test c++" +  std::to_string(i++)).c_str());

    canvas->drawColor(SK_ColorBLUE);
      SkPaint paint;
          UnityConsole::WriteLine(("test c++" +  std::to_string(i++)).c_str());

    SkRect rect = SkRect::MakeXYWH(50, 50, 40, 60);
    canvas->drawRect(rect, paint);

    SkPaint paint2;
    auto text = SkTextBlob::MakeFromString("Hello, Skia!", SkFont(nullptr, 18));
    canvas->drawTextBlob(text.get(), 50, 25, paint2);

    GrBackendTexture  m_backendTex =
          GrBackendTexture(width_, height_, GrMipMapped::kNo, textureInfo);

      m_SkSurface = SkSurface::MakeFromBackendTexture(
          gr_context_.get(), m_backendTex, kBottomLeft_GrSurfaceOrigin, 1,
          kRGBA_8888_SkColorType, nullptr, nullptr);
    return a;
  }

  bool UnitySurfaceManager::MakeResourceCurrent()
  {
        UnityConsole::WriteLine("test c++: make resource");

    return eglMakeCurrent(egl_display_, EGL_NO_SURFACE, EGL_NO_SURFACE,
                          egl_resource_context_) == EGL_TRUE;
  }

  bool UnitySurfaceManager::Initialize(IUnityInterfaces *unity_interfaces)
  {
    // IUnityGraphics* graphics = unity_interfaces->Get<IUnityGraphics>();
    // FML_CHECK(graphics->GetRenderer() == kUnityGfxRendererD3D11)
    //     << "Renderer type is invalid";

    // IUnityGraphicsD3D11* d3d11 = unity_interfaces->Get<IUnityGraphicsD3D11>();
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
    // EGLBoolean result = EGL_FALSE;

    // if (egl_display_ != EGL_NO_DISPLAY && egl_context_ != EGL_NO_CONTEXT) {
    //   result = eglDestroyContext(egl_display_, egl_context_);
    //   egl_context_ = EGL_NO_CONTEXT;

    //   if (result == EGL_FALSE) {
    //     FML_LOG(ERROR) << "EGL: Failed to destroy context";
    //   }
    // }

    // d3d11_device_ = nullptr;

    // if (egl_display_ != EGL_NO_DISPLAY &&
    //     egl_resource_context_ != EGL_NO_CONTEXT) {
    //   result = eglDestroyContext(egl_display_, egl_resource_context_);
    //   egl_resource_context_ = EGL_NO_CONTEXT;

    //   if (result == EGL_FALSE) {
    //     FML_LOG(ERROR) << "EGL : Failed to destroy resource context";
    //   }
    // }

    // if (egl_display_ != EGL_NO_DISPLAY) {
    //   result = eglTerminate(egl_display_);
    //   egl_display_ = EGL_NO_DISPLAY;

    //   if (result == EGL_FALSE) {
    //     FML_LOG(ERROR) << "EGL : Failed to terminate EGL";
    //   }
    // }
  }

} // namespace uiwidgets
