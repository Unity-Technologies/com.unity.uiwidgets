#pragma once

#include "flow/layers/container_layer.h"

namespace uiwidgets {

// Be careful that SkMatrix's default constructor doesn't initialize the matrix
// at all. Hence |set_transform| must be called with an initialized SkMatrix.
class TransformLayer : public ContainerLayer {
 public:
  TransformLayer(const SkMatrix& transform);

  void Preroll(PrerollContext* context, const SkMatrix& matrix) override;

  void Paint(PaintContext& context) const override;

 private:
  SkMatrix transform_;

  FML_DISALLOW_COPY_AND_ASSIGN(TransformLayer);
};

}  // namespace uiwidgets
