#include "flow/layers/shader_mask_layer.h"

namespace uiwidgets {

ShaderMaskLayer::ShaderMaskLayer(sk_sp<SkShader> shader,
                                 const SkRect& mask_rect,
                                 SkBlendMode blend_mode)
    : shader_(shader), mask_rect_(mask_rect), blend_mode_(blend_mode) {}

void ShaderMaskLayer::Preroll(PrerollContext* context, const SkMatrix& matrix) {
  Layer::AutoPrerollSaveLayerState save =
      Layer::AutoPrerollSaveLayerState::Create(context);
  ContainerLayer::Preroll(context, matrix);
}

void ShaderMaskLayer::Paint(PaintContext& context) const {
  TRACE_EVENT0("uiwidgets", "ShaderMaskLayer::Paint");
  FML_DCHECK(needs_painting());

  Layer::AutoSaveLayer save =
      Layer::AutoSaveLayer::Create(context, paint_bounds(), nullptr);
  PaintChildren(context);

  SkPaint paint;
  paint.setBlendMode(blend_mode_);
  paint.setShader(shader_);
  context.leaf_nodes_canvas->translate(mask_rect_.left(), mask_rect_.top());
  context.leaf_nodes_canvas->drawRect(
      SkRect::MakeWH(mask_rect_.width(), mask_rect_.height()), paint);
}

}  // namespace uiwidgets
