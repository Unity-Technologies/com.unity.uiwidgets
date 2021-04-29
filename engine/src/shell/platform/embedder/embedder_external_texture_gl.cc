#include "embedder_external_texture_gl.h"

#include "flutter/fml/logging.h"

namespace uiwidgets {

EmbedderExternalTextureGL::EmbedderExternalTextureGL(
    int64_t texture_identifier, const ExternalTextureCallback& callback)
    : Texture(texture_identifier), external_texture_callback_(callback) {
  FML_DCHECK(external_texture_callback_);
}

EmbedderExternalTextureGL::~EmbedderExternalTextureGL() = default;

// |flutter::Texture|
void EmbedderExternalTextureGL::Paint(SkCanvas& canvas, const SkRect& bounds,
                                      bool freeze, GrContext* context) {
  if (auto image = external_texture_callback_(
          Id(),                                           //
          canvas.getGrContext(),                          //
          SkISize::Make(bounds.width(), bounds.height())  //
          )) {
    last_image_ = image;
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
void EmbedderExternalTextureGL::OnGrContextCreated() {}

// |flutter::Texture|
void EmbedderExternalTextureGL::OnGrContextDestroyed() {}

// |flutter::Texture|
void EmbedderExternalTextureGL::MarkNewFrameAvailable() {}

// |flutter::Texture|
void EmbedderExternalTextureGL::OnTextureUnregistered() {}

}  // namespace uiwidgets
