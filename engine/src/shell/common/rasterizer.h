#pragma once

#include <memory>
#include <optional>

#include "common/settings.h"
#include "common/task_runners.h"
#include "flow/compositor_context.h"
#include "flow/layers/layer_tree.h"
#include "flutter/fml/closure.h"
#include "flutter/fml/memory/weak_ptr.h"
#include "flutter/fml/raster_thread_merger.h"
#include "flutter/fml/synchronization/waitable_event.h"
#include "lib/ui/snapshot_delegate.h"
#include "shell/common/pipeline.h"
#include "shell/common/surface.h"

namespace uiwidgets {

class Rasterizer final : public SnapshotDelegate {
 public:
  class Delegate {
   public:
    virtual void OnFrameRasterized(const FrameTiming& frame_timing) = 0;
    virtual fml::Milliseconds GetFrameBudget() = 0;
  };

  Rasterizer(Delegate& delegate, TaskRunners task_runners);

  Rasterizer(Delegate& delegate, TaskRunners task_runners,
             std::unique_ptr<CompositorContext> compositor_context);

  ~Rasterizer();

  void Setup(std::unique_ptr<Surface> surface);

  void Teardown();

  void NotifyLowMemoryWarning() const;

  fml::WeakPtr<Rasterizer> GetWeakPtr() const;

  fml::WeakPtr<SnapshotDelegate> GetSnapshotDelegate() const;

  LayerTree* GetLastLayerTree();

  void DrawLastLayerTree();

  TextureRegistry* GetTextureRegistry();

  void Draw(fml::RefPtr<Pipeline<LayerTree>> pipeline);

  enum class ScreenshotType {
    SkiaPicture,
    UncompressedImage,
    CompressedImage,
  };

  struct Screenshot {
    sk_sp<SkData> data;

    SkISize frame_size = SkISize::MakeEmpty();

    Screenshot();

    Screenshot(sk_sp<SkData> p_data, SkISize p_size);

    Screenshot(const Screenshot& other);

    ~Screenshot();
  };

  Screenshot ScreenshotLastLayerTree(ScreenshotType type, bool base64_encode);

  void SetNextFrameCallback(const fml::closure& callback);

  CompositorContext* compositor_context() { return compositor_context_.get(); }

  void SetResourceCacheMaxBytes(size_t max_bytes, bool from_user);

  std::optional<size_t> GetResourceCacheMaxBytes() const;

 private:
  Delegate& delegate_;
  TaskRunners task_runners_;
  std::unique_ptr<Surface> surface_;
  std::unique_ptr<CompositorContext> compositor_context_;
  // This is the last successfully rasterized layer tree.
  std::unique_ptr<LayerTree> last_layer_tree_;
  // Set when we need attempt to rasterize the layer tree again. This layer_tree
  // has not successfully rasterized. This can happen due to the change in the
  // thread configuration. This will be inserted to the front of the pipeline.
  std::unique_ptr<LayerTree> resubmitted_layer_tree_;
  fml::closure next_frame_callback_;
  bool user_override_resource_cache_bytes_;
  std::optional<size_t> max_cache_bytes_;
  fml::WeakPtrFactory<Rasterizer> weak_factory_;
  fml::RefPtr<fml::RasterThreadMerger> raster_thread_merger_;

  // |SnapshotDelegate|
  sk_sp<SkImage> MakeRasterSnapshot(sk_sp<SkPicture> picture,
                                    SkISize picture_size) override;

  // |SnapshotDelegate|
  sk_sp<SkImage> ConvertToRasterImage(sk_sp<SkImage> image) override;

  sk_sp<SkImage> DoMakeRasterSnapshot(
      SkISize size, std::function<void(SkCanvas*)> draw_callback);

  RasterStatus DoDraw(std::unique_ptr<LayerTree> layer_tree);

  RasterStatus DrawToSurface(LayerTree& layer_tree);

  void FireNextFrameCallbackIfPresent();

  FML_DISALLOW_COPY_AND_ASSIGN(Rasterizer);
};

}  // namespace uiwidgets
