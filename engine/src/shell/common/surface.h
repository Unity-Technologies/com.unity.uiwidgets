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

  virtual SkMatrix GetRootTransformation() const = 0;

  virtual GrContext* GetContext() = 0;

  virtual ExternalViewEmbedder* GetExternalViewEmbedder();

  virtual bool MakeRenderContextCurrent();

 private:
  FML_DISALLOW_COPY_AND_ASSIGN(Surface);
};

}  // namespace uiwidgets
