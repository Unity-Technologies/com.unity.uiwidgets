#include "unity_surface_manager.h"

#include <d3d11.h>
#include <dxgi.h>
#include <flutter/fml/logging.h>

#include "Unity/IUnityGraphics.h"
#include "Unity/IUnityGraphicsD3D11.h"

namespace uiwidgets {

UnitySurfaceManager::UnitySurfaceManager(IUnityInterfaces* unity_interfaces)
    : egl_display_(EGL_NO_DISPLAY),
      egl_context_(EGL_NO_CONTEXT),
      egl_resource_context_(EGL_NO_CONTEXT),
      egl_config_(nullptr) {
  initialize_succeeded_ = Initialize(unity_interfaces);
}

UnitySurfaceManager::~UnitySurfaceManager() { CleanUp(); }

void UnitySurfaceManager::CreateRenderTexture(size_t width, size_t height) {
  D3D11_TEXTURE2D_DESC desc = {0};
  desc.Width = width;
  desc.Height = height;
  desc.MipLevels = 1;
  desc.ArraySize = 1;
  desc.Format = DXGI_FORMAT_R8G8B8A8_UNORM;
  desc.SampleDesc.Count = 1;
  desc.SampleDesc.Quality = 0;
  desc.Usage = D3D11_USAGE_DEFAULT;
  desc.BindFlags = D3D11_BIND_RENDER_TARGET | D3D11_BIND_SHADER_RESOURCE;
  desc.CPUAccessFlags = 0;
  desc.MiscFlags = D3D11_RESOURCE_MISC_SHARED;

  HRESULT hr = d3d11_device_->CreateTexture2D(&desc, nullptr, &d3d11_texture);
  FML_CHECK(SUCCEEDED(hr)) << "UnitySurfaceManager: Create Native d3d11Texture2D failed";
}

GLuint UnitySurfaceManager::CreateRenderSurface(size_t width, size_t height) {
  CreateRenderTexture(width, height);

  IDXGIResource* image_resource;
  HRESULT hr = d3d11_texture->QueryInterface(
      __uuidof(IDXGIResource), reinterpret_cast<void**>(&image_resource));
  FML_CHECK(SUCCEEDED(hr)) << "UnitySurfaceManager: QueryInterface() failed";

  HANDLE shared_image_handle;
  hr = image_resource->GetSharedHandle(&shared_image_handle);
  FML_CHECK(SUCCEEDED(hr)) << "UnitySurfaceManager: GetSharedHandle() failed";

  image_resource->Release();

  FML_CHECK(shared_image_handle != nullptr)
      << "UnitySurfaceManager: shared_image_handle is nullptr, miscFlags "
         "D3D11_RESOURCE_MISC_SHARED is needed";

  IDXGIResource* dxgi_resource;
  hr = d3d11_angle_device_->OpenSharedResource(
      shared_image_handle, __uuidof(ID3D11Resource),
      reinterpret_cast<void**>(&dxgi_resource));
  FML_CHECK(SUCCEEDED(hr))
      << "UnitySurfaceManager: failed to open shared resource";

  ID3D11Texture2D* image_texture;
  hr = dxgi_resource->QueryInterface(__uuidof(ID3D11Texture2D),
                                     reinterpret_cast<void**>(&image_texture));
  FML_CHECK(SUCCEEDED(hr))
      << "UnitySurfaceManager: failed to query interface ID3D11Texture2D";

  dxgi_resource->Release();

  MakeCurrent(EGL_NO_DISPLAY);

  const EGLint attribs[] = {EGL_NONE};
  FML_DCHECK(fbo_egl_image_ == nullptr);
  fbo_egl_image_ =
      eglCreateImageKHR(egl_display_, EGL_NO_CONTEXT, EGL_D3D11_TEXTURE_ANGLE,
                        static_cast<EGLClientBuffer>(image_texture), attribs);
  FML_CHECK(fbo_egl_image_ != EGL_NO_IMAGE_KHR);

  image_texture->Release();

  GLint old_texture_binding_2d;
  glGetIntegerv(GL_TEXTURE_BINDING_2D, &old_texture_binding_2d);

  FML_DCHECK(fbo_texture_ == 0);
  glGenTextures(1, &fbo_texture_);
  glBindTexture(GL_TEXTURE_2D, fbo_texture_);
  glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_MIN_FILTER, GL_NEAREST);
  glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_MAG_FILTER, GL_NEAREST);
  glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_WRAP_S, GL_CLAMP_TO_EDGE);
  glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_WRAP_T, GL_CLAMP_TO_EDGE);
  glEGLImageTargetTexture2DOES(GL_TEXTURE_2D, fbo_egl_image_);
  glBindTexture(GL_TEXTURE_2D, old_texture_binding_2d);

  GLint old_framebuffer_binding;
  glGetIntegerv(GL_FRAMEBUFFER_BINDING, &old_framebuffer_binding);

  FML_DCHECK(fbo_ == 0);
  glGenFramebuffers(1, &fbo_);
  glBindFramebuffer(GL_FRAMEBUFFER, fbo_);
  glFramebufferTexture2D(GL_FRAMEBUFFER, GL_COLOR_ATTACHMENT0, GL_TEXTURE_2D,
                         fbo_texture_, 0);

  FML_CHECK(glCheckFramebufferStatus(GL_FRAMEBUFFER) ==
            GL_FRAMEBUFFER_COMPLETE);
  glBindFramebuffer(GL_FRAMEBUFFER, old_framebuffer_binding);

  return fbo_;
}

bool UnitySurfaceManager::ClearCurrent() {
  return eglMakeCurrent(egl_display_, EGL_NO_SURFACE, EGL_NO_SURFACE,
                        EGL_NO_CONTEXT) == EGL_TRUE;
}

bool UnitySurfaceManager::MakeCurrent(const EGLSurface surface) {
  return eglMakeCurrent(egl_display_, surface, surface, egl_context_) ==
         EGL_TRUE;
}

bool UnitySurfaceManager::MakeResourceCurrent() {
  return eglMakeCurrent(egl_display_, EGL_NO_SURFACE, EGL_NO_SURFACE,
                        egl_resource_context_) == EGL_TRUE;
}

bool UnitySurfaceManager::Initialize(IUnityInterfaces* unity_interfaces) {
  IUnityGraphics* graphics = unity_interfaces->Get<IUnityGraphics>();
  FML_CHECK(graphics->GetRenderer() == kUnityGfxRendererD3D11)
      << "Renderer type is invalid";

  IUnityGraphicsD3D11* d3d11 = unity_interfaces->Get<IUnityGraphicsD3D11>();
  ID3D11Device* d3d11_device = d3d11->GetDevice();

  d3d11_device_ = d3d11_device;

  IDXGIDevice* dxgi_device;
  HRESULT hr = d3d11_device->QueryInterface(
      __uuidof(IDXGIDevice), reinterpret_cast<void**>(&dxgi_device));

  FML_CHECK(SUCCEEDED(hr)) << "d3d11_device->QueryInterface(...) failed";

  IDXGIAdapter* dxgi_adapter;
  hr = dxgi_device->GetAdapter(&dxgi_adapter);
  FML_CHECK(SUCCEEDED(hr)) << "dxgi_adapter->GetAdapter(...) failed";

  dxgi_device->Release();

  DXGI_ADAPTER_DESC adapter_desc;
  hr = dxgi_adapter->GetDesc(&adapter_desc);
  FML_CHECK(SUCCEEDED(hr)) << "dxgi_adapter->GetDesc(...) failed";

  dxgi_adapter->Release();

  EGLint displayAttribs[] = {EGL_PLATFORM_ANGLE_TYPE_ANGLE,
                             EGL_PLATFORM_ANGLE_TYPE_D3D11_ANGLE,
                            //  EGL_PLATFORM_ANGLE_D3D_LUID_HIGH_ANGLE,
                             adapter_desc.AdapterLuid.HighPart,
                            //  EGL_PLATFORM_ANGLE_D3D_LUID_LOW_ANGLE,
                             adapter_desc.AdapterLuid.LowPart,
                             EGL_PLATFORM_ANGLE_ENABLE_AUTOMATIC_TRIM_ANGLE,
                             EGL_TRUE,
                             EGL_NONE};

  egl_display_ = eglGetPlatformDisplayEXT(EGL_PLATFORM_ANGLE_ANGLE,
                                          EGL_DEFAULT_DISPLAY, displayAttribs);

  FML_CHECK(egl_display_ != EGL_NO_DISPLAY)
      << "EGL: Failed to get a compatible EGLdisplay";

  if (eglInitialize(egl_display_, nullptr, nullptr) == EGL_FALSE) {
    FML_CHECK(false) << "EGL: Failed to initialize EGL";
  }

  EGLAttrib egl_device = 0;
  eglQueryDisplayAttribEXT(egl_display_, EGL_DEVICE_EXT, &egl_device);
  EGLAttrib angle_device = 0;
  eglQueryDeviceAttribEXT(reinterpret_cast<EGLDeviceEXT>(egl_device),
                          EGL_D3D11_DEVICE_ANGLE, &angle_device);
  d3d11_angle_device_ = reinterpret_cast<ID3D11Device*>(angle_device);

  const EGLint configAttributes[] = {EGL_RED_SIZE,   8, EGL_GREEN_SIZE,   8,
                                     EGL_BLUE_SIZE,  8, EGL_ALPHA_SIZE,   8,
                                     EGL_DEPTH_SIZE, 8, EGL_STENCIL_SIZE, 8,
                                     EGL_NONE};

  EGLint numConfigs = 0;
  if (eglChooseConfig(egl_display_, configAttributes, &egl_config_, 1,
                      &numConfigs) == EGL_FALSE ||
      numConfigs == 0) {
    FML_CHECK(false) << "EGL: Failed to choose first context";
  }

  const EGLint display_context_attributes[] = {EGL_CONTEXT_CLIENT_VERSION, 2,
                                               EGL_NONE};

  egl_context_ = eglCreateContext(egl_display_, egl_config_, EGL_NO_CONTEXT,
                                  display_context_attributes);
  if (egl_context_ == EGL_NO_CONTEXT) {
    FML_CHECK(false) << "EGL: Failed to create EGL context";
  }

  egl_resource_context_ = eglCreateContext(
      egl_display_, egl_config_, egl_context_, display_context_attributes);

  if (egl_resource_context_ == EGL_NO_CONTEXT) {
    FML_CHECK(false) << "EGL: Failed to create EGL resource context";
  }

  return true;
}

void UnitySurfaceManager::CleanUp() {
  EGLBoolean result = EGL_FALSE;

  if (egl_display_ != EGL_NO_DISPLAY &&
      egl_resource_context_ != EGL_NO_CONTEXT) {
    result = eglDestroyContext(egl_display_, egl_resource_context_);
    egl_resource_context_ = EGL_NO_CONTEXT;

    if (result == EGL_FALSE) {
      FML_LOG(ERROR) << "EGL : Failed to destroy resource context";
    }
  }

  if (egl_display_ != EGL_NO_DISPLAY && egl_context_ != EGL_NO_CONTEXT) {
    result = eglDestroyContext(egl_display_, egl_context_);
    egl_context_ = EGL_NO_CONTEXT;

    if (result == EGL_FALSE) {
      FML_LOG(ERROR) << "EGL: Failed to destroy context";
    }
  }

  //TODO: investigate a bit more on the possible memory leak here since egl_display_ will never be released
  //refer to the commit log (9b39afd879d06626f5049cfae2a9cb852044c518) for the details
  if (egl_display_ != EGL_NO_DISPLAY) {
    egl_display_ = EGL_NO_DISPLAY;
  }
  
  d3d11_device_ = nullptr;
  d3d11_angle_device_ = nullptr;
}

bool UnitySurfaceManager::ReleaseNativeRenderTexture() {
  if (d3d11_texture == nullptr) {
    return true;  
  }
  FML_DCHECK(fbo_ != 0);
  glDeleteFramebuffers(1, &fbo_);
  fbo_ = 0;

  FML_DCHECK(fbo_texture_ != 0);
  glDeleteTextures(1, &fbo_texture_);
  fbo_texture_ = 0;

  FML_DCHECK(fbo_egl_image_ != nullptr);
  eglDestroyImageKHR(egl_display_, fbo_egl_image_);
  fbo_egl_image_ = nullptr;

  d3d11_texture->Release();
  d3d11_texture = nullptr;

  return true;
}

}  // namespace uiwidgets
 