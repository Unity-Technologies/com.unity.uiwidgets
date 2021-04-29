#pragma once

#include "flow/layers/layer.h"
#include "include/core/SkPoint.h"
#include "include/core/SkSize.h"

namespace uiwidgets {

class TextureLayer : public Layer {
 public:
  TextureLayer(const SkPoint& offset, const SkSize& size, int64_t texture_id,
               bool freeze);

  void Preroll(PrerollContext* context, const SkMatrix& matrix) override;
  void Paint(PaintContext& context) const override;

 private:
  SkPoint offset_;
  SkSize size_;
  int64_t texture_id_;
  bool freeze_;

  FML_DISALLOW_COPY_AND_ASSIGN(TextureLayer);
};

}  // namespace uiwidgets
