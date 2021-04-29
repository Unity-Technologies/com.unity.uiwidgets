#pragma once

#include "flow/texture.h"
#include "flutter/fml/macros.h"
#include "include/core/SkImage.h"
#include "include/core/SkSize.h"

namespace uiwidgets {

class EmbedderExternalTextureGL : public Texture {
 public:
  using ExternalTextureCallback = std::function<sk_sp<SkImage>(
      int64_t texture_identifier, GrContext*, const SkISize&)>;

  EmbedderExternalTextureGL(int64_t texture_identifier,
                            const ExternalTextureCallback& callback);

  ~EmbedderExternalTextureGL();

 private:
  ExternalTextureCallback external_texture_callback_;
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

  FML_DISALLOW_COPY_AND_ASSIGN(EmbedderExternalTextureGL);
};

}  // namespace uiwidgets
