#pragma once

#include "flutter/fml/macros.h"
#include "flutter/fml/memory/weak_ptr.h"
#include "shell/common/surface.h"
#include "shell/gpu/gpu_surface_software_delegate.h"

namespace uiwidgets {

class GPUSurfaceSoftware : public Surface {
 public:
  GPUSurfaceSoftware(GPUSurfaceSoftwareDelegate* delegate,
                     bool render_to_surface);

  ~GPUSurfaceSoftware() override;

  // |Surface|
  bool IsValid() override;

  // |Surface|
  std::unique_ptr<SurfaceFrame> AcquireFrame(const SkISize& size) override;

  // |Surface|
  SkMatrix GetRootTransformation() const override;

  // |Surface|
  GrContext* GetContext() override;
    
  void ClearContext() override;

  // |Surface|
  ExternalViewEmbedder* GetExternalViewEmbedder() override;

 private:
  GPUSurfaceSoftwareDelegate* delegate_;
  // TODO(38466): Refactor GPU surface APIs take into account the fact that an
  // external view embedder may want to render to the root surface. This is a
  // hack to make avoid allocating resources for the root surface when an
  // external view embedder is present.
  const bool render_to_surface_;
  fml::WeakPtrFactory<GPUSurfaceSoftware> weak_factory_;

  FML_DISALLOW_COPY_AND_ASSIGN(GPUSurfaceSoftware);
};

}  // namespace uiwidgets
