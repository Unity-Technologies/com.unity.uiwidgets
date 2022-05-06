#pragma once

#include <functional>
#include <memory>

#include "flow/embedded_views.h"
#include "flutter/fml/macros.h"
#include "flutter/fml/memory/weak_ptr.h"
#include "include/gpu/GrContext.h"
#include "shell/common/surface.h"
#include "shell/gpu/gpu_surface_gl_delegate.h"

namespace uiwidgets {

class GPUSurfaceGL : public Surface {
 public:
  GPUSurfaceGL(GPUSurfaceGLDelegate* delegate, bool render_to_surface);

  // Creates a new GL surface reusing an existing GrContext.
  GPUSurfaceGL(sk_sp<GrContext> gr_context, GPUSurfaceGLDelegate* delegate,
               bool render_to_surface);

  // |Surface|
  ~GPUSurfaceGL() override;

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

  // |Surface|
  bool MakeRenderContextCurrent() override;

 private:
  GPUSurfaceGLDelegate* delegate_;
  sk_sp<GrContext> context_;
  sk_sp<SkSurface> onscreen_surface_;
  bool context_owner_;
  // TODO(38466): Refactor GPU surface APIs take into account the fact that an
  // external view embedder may want to render to the root surface. This is a
  // hack to make avoid allocating resources for the root surface when an
  // external view embedder is present.
  const bool render_to_surface_;
  bool valid_ = false;
  fml::WeakPtrFactory<GPUSurfaceGL> weak_factory_;

  bool CreateOrUpdateSurfaces(const SkISize& size);

  sk_sp<SkSurface> AcquireRenderSurface(
      const SkISize& untransformed_size,
      const SkMatrix& root_surface_transformation);

  bool PresentSurface(SkCanvas* canvas);

  FML_DISALLOW_COPY_AND_ASSIGN(GPUSurfaceGL);
};

}  // namespace uiwidgets
