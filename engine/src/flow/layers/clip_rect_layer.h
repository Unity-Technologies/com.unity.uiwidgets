#pragma once

#include "flow/layers/container_layer.h"

namespace uiwidgets {

class ClipRectLayer : public ContainerLayer {
 public:
  ClipRectLayer(const SkRect& clip_rect, Clip clip_behavior);

  void Preroll(PrerollContext* context, const SkMatrix& matrix) override;
  void Paint(PaintContext& context) const override;

  bool UsesSaveLayer() const {
    return clip_behavior_ == Clip::antiAliasWithSaveLayer;
  }

 private:
  SkRect clip_rect_;
  Clip clip_behavior_;
  bool children_inside_clip_ = false;

  FML_DISALLOW_COPY_AND_ASSIGN(ClipRectLayer);
};

}  // namespace uiwidgets
