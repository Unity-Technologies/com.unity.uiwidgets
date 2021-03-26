#include "embedder_render_target.h"

#include "flutter/fml/logging.h"

namespace uiwidgets {

EmbedderRenderTarget::EmbedderRenderTarget(UIWidgetsBackingStore backing_store,
                                           sk_sp<SkSurface> render_surface,
                                           fml::closure on_release)
    : backing_store_(backing_store),
      render_surface_(std::move(render_surface)),
      on_release_(on_release) {
  // TODO(38468): The optimization to elide backing store updates between frames
  // has not been implemented yet.
  backing_store_.did_update = true;
  FML_DCHECK(render_surface_);
}

EmbedderRenderTarget::~EmbedderRenderTarget() {
  if (on_release_) {
    on_release_();
  }
}

const UIWidgetsBackingStore* EmbedderRenderTarget::GetBackingStore() const {
  return &backing_store_;
}

sk_sp<SkSurface> EmbedderRenderTarget::GetRenderSurface() const {
  return render_surface_;
}

}  // namespace uiwidgets
