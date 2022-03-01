#pragma once

#include "flow/texture.h"
#include "flutter/fml/macros.h"
#include "include/core/SkImage.h"
#include "include/core/SkSize.h"
#include "unity_surface_manager.h"

namespace uiwidgets {

class UnityExternalTextureGL : public Texture {
 public:
  UnityExternalTextureGL(int64_t texture_identifier);

  ~UnityExternalTextureGL() override;

 private:
  GLuint gl_texture_;
  sk_sp<SkImage> last_image_;

  // |flutter::Texture|
  void Paint(SkCanvas& canvas, const SkRect& bounds, bool freeze,
             GrContext* context) override;

  // |flutter::Texture|
  void OnGrContextCreated() override;

  // |flutter::Texture|
  void OnGrContextDestroyed() override;

  // |flutter::Texture|
  void MarkNewFrameAvailable() override;

  // |flutter::Texture|
  void OnTextureUnregistered() override;

  FML_DISALLOW_COPY_AND_ASSIGN(UnityExternalTextureGL);
};

}  // namespace uiwidgets
