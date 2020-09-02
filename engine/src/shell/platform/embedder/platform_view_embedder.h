#pragma once

#include <functional>

#include "flutter/fml/macros.h"
#include "shell/common/platform_view.h"
#include "shell/platform/embedder/embedder.h"
#include "shell/platform/embedder/embedder_surface.h"
#include "shell/platform/embedder/embedder_surface_gl.h"
#include "shell/platform/embedder/embedder_surface_software.h"
#include "shell/platform/embedder/vsync_waiter_embedder.h"

namespace uiwidgets {

class PlatformViewEmbedder final : public PlatformView {
 public:
  using PlatformMessageResponseCallback =
      std::function<void(fml::RefPtr<PlatformMessage>)>;

  struct PlatformDispatchTable {
    PlatformMessageResponseCallback
        platform_message_response_callback;             // optional
    VsyncWaiterEmbedder::VsyncCallback vsync_callback;  // optional
  };

  // Creates a platform view that sets up an OpenGL rasterizer.
  PlatformViewEmbedder(
      PlatformView::Delegate& delegate, TaskRunners task_runners,
      EmbedderSurfaceGL::GLDispatchTable gl_dispatch_table,
      bool fbo_reset_after_present,
      PlatformDispatchTable platform_dispatch_table,
      std::unique_ptr<EmbedderExternalViewEmbedder> external_view_embedder);

  // Create a platform view that sets up a software rasterizer.
  PlatformViewEmbedder(
      PlatformView::Delegate& delegate, TaskRunners task_runners,
      EmbedderSurfaceSoftware::SoftwareDispatchTable software_dispatch_table,
      PlatformDispatchTable platform_dispatch_table,
      std::unique_ptr<EmbedderExternalViewEmbedder> external_view_embedder);

  ~PlatformViewEmbedder() override;

  // |PlatformView|
  void HandlePlatformMessage(fml::RefPtr<PlatformMessage> message) override;

 private:
  std::unique_ptr<EmbedderSurface> embedder_surface_;
  PlatformDispatchTable platform_dispatch_table_;

  // |PlatformView|
  std::unique_ptr<Surface> CreateRenderingSurface() override;

  // |PlatformView|
  sk_sp<GrContext> CreateResourceContext() const override;

  // |PlatformView|
  std::unique_ptr<VsyncWaiter> CreateVSyncWaiter() override;

  FML_DISALLOW_COPY_AND_ASSIGN(PlatformViewEmbedder);
};

}  // namespace uiwidgets
