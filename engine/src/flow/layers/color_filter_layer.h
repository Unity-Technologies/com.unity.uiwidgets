#pragma once
#include "flow/layers/container_layer.h"
#include "include/core/SkColorFilter.h"

namespace uiwidgets {

class ColorFilterLayer : public ContainerLayer {
 public:
  ColorFilterLayer(sk_sp<SkColorFilter> filter);

  void Preroll(PrerollContext* context, const SkMatrix& matrix) override;

  void Paint(PaintContext& context) const override;

 private:
  sk_sp<SkColorFilter> filter_;

  FML_DISALLOW_COPY_AND_ASSIGN(ColorFilterLayer);
};

}  // namespace uiwidgets
