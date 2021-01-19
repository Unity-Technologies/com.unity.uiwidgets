#include "unity_external_texture_gl.h"

#include <EGL/egl.h>
#include <EGL/eglext.h>
#include <EGL/eglext_angle.h>
#include <GLES2/gl2.h>
#include <GLES2/gl2ext.h>
#include <d3d11.h>

#include "flutter/fml/logging.h"
#include "include/gpu/GrBackendSurface.h"
#include "include/gpu/GrContext.h"
#include "include/gpu/gl/GrGLTypes.h"
#include "src/gpu/gl/GrGLDefines.h"
#include "uiwidgets_system.h"

namespace uiwidgets {

UnityExternalTextureGL::UnityExternalTextureGL(
    int64_t texture_identifier, void* native_texture_ptr,
    UnitySurfaceManager* unity_surface_manager)
    : Texture(texture_identifier),
      unity_surface_manager_(unity_surface_manager) {
  auto* graphics = UIWidgetsSystem::GetInstancePtr()
                       ->GetUnityInterfaces()
                       ->Get<IUnityGraphics>();
  FML_DCHECK(graphics->GetRenderer() == kUnityGfxRendererD3D11);

  auto* src_d3d11_texture = static_cast<ID3D11Texture2D*>(native_texture_ptr);

  IDXGIResource* src_d3d11_resource;
  HRESULT hr = src_d3d11_texture->QueryInterface(
      __uuidof(IDXGIResource), reinterpret_cast<void**>(&src_d3d11_resource));

  FML_CHECK(SUCCEEDED(hr));

  HANDLE shared_image_handle;
  hr = src_d3d11_resource->GetSharedHandle(&shared_image_handle);
  FML_CHECK(SUCCEEDED(hr));

  src_d3d11_resource->Release();

  IDXGIResource* d3d11_resource;

  unity_surface_manager_->GetD3D11Device()->OpenSharedResource(
      shared_image_handle, __uuidof(ID3D11Resource),
      reinterpret_cast<void**>(&d3d11_resource));

  d3d11_resource->QueryInterface(__uuidof(ID3D11Texture2D),
                                 reinterpret_cast<void**>(&d3d11_texture_));

  d3d11_resource->Release();

  const EGLint attribs[] = {EGL_NONE};

  egl_image_ =
      eglCreateImageKHR(unity_surface_manager_->GetEGLDisplay(), EGL_NO_CONTEXT,
                        EGL_D3D11_TEXTURE_ANGLE,
                        static_cast<EGLClientBuffer>(d3d11_texture_), attribs);

  gl_texture_ = 0;
}

UnityExternalTextureGL::~UnityExternalTextureGL() {
  last_image_ = nullptr;
  if (gl_texture_) {
    glDeleteTextures(1, &gl_texture_);
    gl_texture_ = 0;
  }

  eglDestroyImageKHR(unity_surface_manager_->GetEGLDisplay(), egl_image_);
  d3d11_texture_->Release();
}

// |flutter::Texture|
void UnityExternalTextureGL::Paint(SkCanvas& canvas, const SkRect& bounds,
                                   bool freeze, GrContext* context) {
  if (!last_image_) {
    if (!gl_texture_) {
      glGenTextures(1, &gl_texture_);
      glBindTexture(GL_TEXTURE_2D, gl_texture_);
      glEGLImageTargetTexture2DOES(GL_TEXTURE_2D, egl_image_);
    }

    GrGLTextureInfo texture_info;
    texture_info.fTarget = GR_GL_TEXTURE_2D;
    texture_info.fID = gl_texture_;
    texture_info.fFormat = GR_GL_RGBA8;

    GrBackendTexture backend_tex = GrBackendTexture(
        bounds.width(), bounds.height(), GrMipMapped::kNo, texture_info);

    last_image_ = SkImage::MakeFromTexture(
        context, backend_tex, kBottomLeft_GrSurfaceOrigin,
        kRGBA_8888_SkColorType, kOpaque_SkAlphaType, nullptr);
  }

  if (last_image_) {
    if (bounds != SkRect::Make(last_image_->bounds())) {
      canvas.drawImageRect(last_image_, bounds, nullptr);
    } else {
      canvas.drawImage(last_image_, bounds.x(), bounds.y());
    }
  }
}

// |flutter::Texture|
void UnityExternalTextureGL::OnGrContextCreated() {}

// |flutter::Texture|
void UnityExternalTextureGL::OnGrContextDestroyed() {}

// |flutter::Texture|
void UnityExternalTextureGL::MarkNewFrameAvailable() {}

// |flutter::Texture|
void UnityExternalTextureGL::OnTextureUnregistered() {}

}  // namespace uiwidgets
