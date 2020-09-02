#pragma once

#include <memory>
#include <vector>

#include "flow/embedded_views.h"
#include "flutter/fml/macros.h"
#include "include/core/SkMatrix.h"
#include "include/core/SkSize.h"
#include "shell/platform/embedder/embedder.h"

namespace uiwidgets {

class EmbedderLayers {
 public:
  EmbedderLayers(SkISize frame_size, double device_pixel_ratio,
                 SkMatrix root_surface_transformation);

  ~EmbedderLayers();

  void PushBackingStoreLayer(const UIWidgetsBackingStore* store);

  void PushPlatformViewLayer(UIWidgetsPlatformViewIdentifier identifier,
                             const EmbeddedViewParams& params);

  using PresentCallback =
      std::function<bool(const std::vector<const UIWidgetsLayer*>& layers)>;
  void InvokePresentCallback(const PresentCallback& callback) const;

 private:
  const SkISize frame_size_;
  const double device_pixel_ratio_;
  const SkMatrix root_surface_transformation_;
  std::vector<std::unique_ptr<UIWidgetsPlatformView>>
      platform_views_referenced_;
  std::vector<std::unique_ptr<UIWidgetsPlatformViewMutation>>
      mutations_referenced_;
  std::vector<
      std::unique_ptr<std::vector<const UIWidgetsPlatformViewMutation*>>>
      mutations_arrays_referenced_;
  std::vector<UIWidgetsLayer> presented_layers_;

  FML_DISALLOW_COPY_AND_ASSIGN(EmbedderLayers);
};

}  // namespace uiwidgets
