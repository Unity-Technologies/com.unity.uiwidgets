#pragma once

#include "flow/layers/container_layer.h"
#include "include/core/SkImageFilter.h"

namespace uiwidgets {

class ImageFilterLayer : public ContainerLayer {
 public:
  ImageFilterLayer(sk_sp<SkImageFilter> filter);

  void Preroll(PrerollContext* context, const SkMatrix& matrix) override;

  void Paint(PaintContext& context) const override;

 private:
  sk_sp<SkImageFilter> filter_;
  SkRect child_paint_bounds_;

  FML_DISALLOW_COPY_AND_ASSIGN(ImageFilterLayer);
};

}  // namespace uiwidgets
