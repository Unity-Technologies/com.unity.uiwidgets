#pragma once

#include "flutter/fml/macros.h"
#include "shell/gpu/gpu_surface_software.h"
#include "shell/platform/embedder/embedder_external_view_embedder.h"
#include "shell/platform/embedder/embedder_surface.h"

namespace uiwidgets {

class EmbedderSurfaceSoftware final : public EmbedderSurface,
                                      public GPUSurfaceSoftwareDelegate {
 public:
  struct SoftwareDispatchTable {
    std::function<bool(const void* allocation, size_t row_bytes, size_t height)>
        software_present_backing_store;  // required
  };

  EmbedderSurfaceSoftware(
      SoftwareDispatchTable software_dispatch_table,
      std::unique_ptr<EmbedderExternalViewEmbedder> external_view_embedder);

  ~EmbedderSurfaceSoftware() override;

 private:
  bool valid_ = false;
  SoftwareDispatchTable software_dispatch_table_;
  sk_sp<SkSurface> sk_surface_;
  std::unique_ptr<EmbedderExternalViewEmbedder> external_view_embedder_;

  // |EmbedderSurface|
  bool IsValid() const override;

  // |EmbedderSurface|
  std::unique_ptr<Surface> CreateGPUSurface() override;

  // |EmbedderSurface|
  sk_sp<GrContext> CreateResourceContext() const override;

  // |GPUSurfaceSoftwareDelegate|
  sk_sp<SkSurface> AcquireBackingStore(const SkISize& size) override;

  // |GPUSurfaceSoftwareDelegate|
  bool PresentBackingStore(sk_sp<SkSurface> backing_store) override;

  // |GPUSurfaceSoftwareDelegate|
  ExternalViewEmbedder* GetExternalViewEmbedder() override;

  FML_DISALLOW_COPY_AND_ASSIGN(EmbedderSurfaceSoftware);
};

}  // namespace uiwidgets
