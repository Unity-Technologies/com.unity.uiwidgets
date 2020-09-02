#pragma once

#include "flutter/fml/closure.h"
#include "flutter/fml/macros.h"
#include "include/core/SkCanvas.h"
#include "include/core/SkSurface.h"
#include "shell/platform/embedder/embedder.h"

namespace uiwidgets {

class EmbedderRenderTarget {
 public:
  EmbedderRenderTarget(UIWidgetsBackingStore backing_store,
                       sk_sp<SkSurface> render_surface,
                       fml::closure on_release);
  ~EmbedderRenderTarget();

  sk_sp<SkSurface> GetRenderSurface() const;

  const UIWidgetsBackingStore* GetBackingStore() const;

 private:
  UIWidgetsBackingStore backing_store_;
  sk_sp<SkSurface> render_surface_;
  fml::closure on_release_;

  FML_DISALLOW_COPY_AND_ASSIGN(EmbedderRenderTarget);
};

}  // namespace uiwidgets
