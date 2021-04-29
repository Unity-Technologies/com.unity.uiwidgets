#include "flow/layers/backdrop_filter_layer.h"

namespace uiwidgets {

BackdropFilterLayer::BackdropFilterLayer(sk_sp<SkImageFilter> filter)
    : filter_(std::move(filter)) {}

void BackdropFilterLayer::Preroll(PrerollContext* context,
                                  const SkMatrix& matrix) {
  Layer::AutoPrerollSaveLayerState save =
      Layer::AutoPrerollSaveLayerState::Create(context, true, bool(filter_));
  ContainerLayer::Preroll(context, matrix);
}

void BackdropFilterLayer::Paint(PaintContext& context) const {
  TRACE_EVENT0("uiwidgets", "BackdropFilterLayer::Paint");
  FML_DCHECK(needs_painting());

  Layer::AutoSaveLayer save = Layer::AutoSaveLayer::Create(
      context,
      SkCanvas::SaveLayerRec{&paint_bounds(), nullptr, filter_.get(), 0});
  PaintChildren(context);
}

}  // namespace uiwidgets
