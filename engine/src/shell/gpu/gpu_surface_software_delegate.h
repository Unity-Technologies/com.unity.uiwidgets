#pragma once

#include "flow/embedded_views.h"
#include "flutter/fml/macros.h"
#include "include/core/SkSurface.h"
#include "shell/gpu/gpu_surface_delegate.h"

namespace uiwidgets {

class GPUSurfaceSoftwareDelegate : public GPUSurfaceDelegate {
 public:
  ~GPUSurfaceSoftwareDelegate() override;

  // |GPUSurfaceDelegate|
  ExternalViewEmbedder* GetExternalViewEmbedder() override;

  virtual sk_sp<SkSurface> AcquireBackingStore(const SkISize& size) = 0;

  virtual bool PresentBackingStore(sk_sp<SkSurface> backing_store) = 0;
};

}  // namespace uiwidgets
