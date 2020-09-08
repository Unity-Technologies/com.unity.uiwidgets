#pragma once

#include <map>
#include <unordered_map>

#include "flow/embedded_views.h"
#include "flutter/fml/hash_combine.h"
#include "flutter/fml/macros.h"
#include "shell/platform/embedder/embedder_external_view.h"
#include "shell/platform/embedder/embedder_render_target_cache.h"

namespace uiwidgets {

class EmbedderExternalViewEmbedder final : public ExternalViewEmbedder {
 public:
  using CreateRenderTargetCallback =
      std::function<std::unique_ptr<EmbedderRenderTarget>(
          GrContext* context, const UIWidgetsBackingStoreConfig& config)>;
  using PresentCallback =
      std::function<bool(const std::vector<const UIWidgetsLayer*>& layers)>;
  using SurfaceTransformationCallback = std::function<SkMatrix(void)>;

  EmbedderExternalViewEmbedder(
      const CreateRenderTargetCallback& create_render_target_callback,
      const PresentCallback& present_callback);

  ~EmbedderExternalViewEmbedder() override;

  void SetSurfaceTransformationCallback(
      SurfaceTransformationCallback surface_transformation_callback);

 private:
  // |ExternalViewEmbedder|
  void CancelFrame() override;

  // |ExternalViewEmbedder|
  void BeginFrame(SkISize frame_size, GrContext* context,
                  double device_pixel_ratio) override;

  // |ExternalViewEmbedder|
  void PrerollCompositeEmbeddedView(
      int view_id, std::unique_ptr<EmbeddedViewParams> params) override;

  // |ExternalViewEmbedder|
  std::vector<SkCanvas*> GetCurrentCanvases() override;

  // |ExternalViewEmbedder|
  SkCanvas* CompositeEmbeddedView(int view_id) override;

  // |ExternalViewEmbedder|
  bool SubmitFrame(GrContext* context, SkCanvas* background_canvas) override;

  // |ExternalViewEmbedder|
  void FinishFrame() override;

  // |ExternalViewEmbedder|
  SkCanvas* GetRootCanvas() override;

 private:
  const CreateRenderTargetCallback create_render_target_callback_;
  const PresentCallback present_callback_;
  SurfaceTransformationCallback surface_transformation_callback_;
  SkISize pending_frame_size_ = SkISize::Make(0, 0);
  double pending_device_pixel_ratio_ = 1.0;
  SkMatrix pending_surface_transformation_;
  EmbedderExternalView::PendingViews pending_views_;
  std::vector<EmbedderExternalView::ViewIdentifier> composition_order_;
  EmbedderRenderTargetCache render_target_cache_;

  void Reset();

  SkMatrix GetSurfaceTransformation() const;

  FML_DISALLOW_COPY_AND_ASSIGN(EmbedderExternalViewEmbedder);
};

}  // namespace uiwidgets
