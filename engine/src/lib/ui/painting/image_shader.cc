#include "image_shader.h"

#include "lib/ui/ui_mono_state.h"

namespace uiwidgets {

fml::RefPtr<ImageShader> ImageShader::Create() {
  return fml::MakeRefCounted<ImageShader>();
}

void ImageShader::initWithImage(CanvasImage* image, SkTileMode tmx,
                                SkTileMode tmy, const float* matrix4) {
  if (!image) {
    Mono_ThrowException(
        "ImageShader constructor called with non-genuine Image.");
  }
  SkMatrix sk_matrix = ToSkMatrix(matrix4);
  set_shader(UIMonoState::CreateGPUObject(
      image->image()->makeShader(tmx, tmy, &sk_matrix)));
}

ImageShader::ImageShader() = default;

ImageShader::~ImageShader() = default;

}  // namespace uiwidgets
