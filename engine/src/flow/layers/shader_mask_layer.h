#pragma once

#include "flow/layers/container_layer.h"
#include "include/core/SkShader.h"

namespace uiwidgets {

class ShaderMaskLayer : public ContainerLayer {
 public:
  ShaderMaskLayer(sk_sp<SkShader> shader, const SkRect& mask_rect,
                  SkBlendMode blend_mode);

  void Preroll(PrerollContext* context, const SkMatrix& matrix) override;

  void Paint(PaintContext& context) const override;

 private:
  sk_sp<SkShader> shader_;
  SkRect mask_rect_;
  SkBlendMode blend_mode_;

  FML_DISALLOW_COPY_AND_ASSIGN(ShaderMaskLayer);
};

}  // namespace uiwidgets
