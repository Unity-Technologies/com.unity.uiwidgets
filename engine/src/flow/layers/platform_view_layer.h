#pragma once

#include "flow/layers/layer.h"
#include "include/core/SkPoint.h"
#include "include/core/SkSize.h"

namespace uiwidgets {

class PlatformViewLayer : public Layer {
 public:
  PlatformViewLayer(const SkPoint& offset, const SkSize& size, int64_t view_id);

  void Preroll(PrerollContext* context, const SkMatrix& matrix) override;
  void Paint(PaintContext& context) const override;

 private:
  SkPoint offset_;
  SkSize size_;
  int64_t view_id_;

  FML_DISALLOW_COPY_AND_ASSIGN(PlatformViewLayer);
};

}  // namespace uiwidgets
