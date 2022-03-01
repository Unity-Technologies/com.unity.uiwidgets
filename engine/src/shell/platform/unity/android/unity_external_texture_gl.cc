#include "unity_external_texture_gl.h"

#include <EGL/egl.h>
#include <GLES2/gl2.h>

#include "flutter/fml/logging.h"
#include "include/gpu/GrBackendSurface.h"
#include "include/gpu/GrContext.h"
#include "include/gpu/gl/GrGLTypes.h"
#include "src/gpu/gl/GrGLDefines.h"
#include "uiwidgets_system.h"

namespace uiwidgets {

UnityExternalTextureGL::UnityExternalTextureGL(
    int64_t texture_identifier)
    : Texture(texture_identifier){
  auto* graphics = UIWidgetsSystem::GetInstancePtr()
                       ->GetUnityInterfaces()
                       ->Get<IUnityGraphics>();
  UnityGfxRenderer renderer = graphics->GetRenderer();
  FML_DCHECK(renderer == kUnityGfxRendererOpenGLES20 || renderer == kUnityGfxRendererOpenGLES30);
  gl_texture_ = texture_identifier;
}

UnityExternalTextureGL::~UnityExternalTextureGL() {
  last_image_ = nullptr;
  if (gl_texture_) {
    glDeleteTextures(1, &gl_texture_);
    gl_texture_ = 0;
  }
}

// |flutter::Texture|
void UnityExternalTextureGL::Paint(SkCanvas& canvas, const SkRect& bounds,
                                   bool freeze, GrContext* context) {
  if (!last_image_) {
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
