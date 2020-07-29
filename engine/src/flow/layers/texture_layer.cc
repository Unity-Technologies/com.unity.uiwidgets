#include "flow/layers/texture_layer.h"

#include "flow/texture.h"

namespace uiwidgets {

TextureLayer::TextureLayer(const SkPoint& offset, const SkSize& size,
                           int64_t texture_id, bool freeze)
    : offset_(offset), size_(size), texture_id_(texture_id), freeze_(freeze) {}

void TextureLayer::Preroll(PrerollContext* context, const SkMatrix& matrix) {
  TRACE_EVENT0("uiwidgets", "TextureLayer::Preroll");

  set_paint_bounds(SkRect::MakeXYWH(offset_.x(), offset_.y(), size_.width(),
                                    size_.height()));
}

void TextureLayer::Paint(PaintContext& context) const {
  TRACE_EVENT0("uiwidgets", "TextureLayer::Paint");

  std::shared_ptr<Texture> texture =
      context.texture_registry.GetTexture(texture_id_);
  if (!texture) {
    TRACE_EVENT_INSTANT0("uiwidgets", "null texture");
    return;
  }
  texture->Paint(*context.leaf_nodes_canvas, paint_bounds(), freeze_,
                 context.gr_context);
}

}  // namespace uiwidgets
