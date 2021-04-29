#pragma once

#include "gradient.h"
#include "image.h"
#include "include/core/SkMatrix.h"
#include "include/core/SkShader.h"
#include "matrix.h"
#include "shader.h"

namespace uiwidgets {

class ImageShader : public Shader {
  FML_FRIEND_MAKE_REF_COUNTED(ImageShader);

 public:
  ~ImageShader() override;
  static fml::RefPtr<ImageShader> Create();

  void initWithImage(CanvasImage* image, SkTileMode tmx, SkTileMode tmy,
                     const float* matrix4);

 private:
  ImageShader();
};

}  // namespace uiwidgets
