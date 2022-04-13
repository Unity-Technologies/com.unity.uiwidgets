#pragma once

#include <memory>

#include "flow/compositor_context.h"
#include "flow/embedded_views.h"
#include "flutter/fml/macros.h"
#include "include/core/SkCanvas.h"

namespace uiwidgets {

/// Represents a Frame that has been fully configured for the underlying client
/// rendering API. A frame may only be submitted once.
class SurfaceFrame {
 public:
  using SubmitCallback =
      std::function<bool(const SurfaceFrame& surface_frame, SkCanvas* canvas)>;

  SurfaceFrame(sk_sp<SkSurface> surface,
               bool supports_readback,
               const SubmitCallback& submit_callback);

  ~SurfaceFrame();

  bool Submit();

  SkCanvas* SkiaCanvas();

  sk_sp<SkSurface> SkiaSurface() const;

  bool supports_readback() { return supports_readback_; }

 private:
  bool submitted_;
  sk_sp<SkSurface> surface_;
  bool supports_readback_;
  SubmitCallback submit_callback_;

  bool PerformSubmit();

  FML_DISALLOW_COPY_AND_ASSIGN(SurfaceFrame);
};

/// Abstract Base Class that represents where we will be rendering content.
class Surface {
 public:
  Surface();

  virtual ~Surface();

  virtual bool IsValid() = 0;

  virtual std::unique_ptr<SurfaceFrame> AcquireFrame(const SkISize& size) = 0;

  //In UIWidgets, we add this new API to Surface which will eventually call the method UnitySurfaceManager.ClearCurrentContext on the specific platform
  //Since UIWidgets engine will always change the current OpenGL state before emitting its own OpenGL commands to the GPU by calling UnitySurfaceManager.MakeCurrentContext, we need to guarantee to restore the original OpenGL state after the job is done. Otherwise the OpenGL context outside will be permanentally changed and bad things would happen. Altough this situation won't exist for flutter (since it doesn't need to deal with any OpenGL contexts outside), it causes issues on UIWidgets engine. For instance, on Mac (OpenGLCore backend) the OpenGL context of Unity will be eventually lost and nothing can be rendered out properly.
  //To address this issue, we add this API so that we can call this method to force restoring the original OpenGL state in each code path after it is changed by UIWidget engine
  virtual void ClearContext() = 0;

  virtual SkMatrix GetRootTransformation() const = 0;

  virtual GrContext* GetContext() = 0;

  virtual ExternalViewEmbedder* GetExternalViewEmbedder();

  virtual bool MakeRenderContextCurrent();

 private:
  FML_DISALLOW_COPY_AND_ASSIGN(Surface);
};

}  // namespace uiwidgets
