#include "flow/layers/color_filter_layer.h"

namespace uiwidgets {

ColorFilterLayer::ColorFilterLayer(sk_sp<SkColorFilter> filter)
    : filter_(std::move(filter)) {}

void ColorFilterLayer::Preroll(PrerollContext* context,
                               const SkMatrix& matrix) {
  Layer::AutoPrerollSaveLayerState save =
      Layer::AutoPrerollSaveLayerState::Create(context);
  ContainerLayer::Preroll(context, matrix);
}

void ColorFilterLayer::Paint(PaintContext& context) const {
  TRACE_EVENT0("uiwidgets", "ColorFilterLayer::Paint");
  FML_DCHECK(needs_painting());

  SkPaint paint;
  paint.setColorFilter(filter_);

  Layer::AutoSaveLayer save =
      Layer::AutoSaveLayer::Create(context, paint_bounds(), &paint);
  PaintChildren(context);
}

}  // namespace uiwidgets
