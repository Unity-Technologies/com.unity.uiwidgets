#include "scene.h"

#include "flutter/fml/trace_event.h"
#include "lib/ui/painting/image.h"
#include "lib/ui/painting/picture.h"
#include "lib/ui/ui_mono_state.h"
#include "lib/ui/window/window.h"

namespace uiwidgets {

fml::RefPtr<Scene> Scene::create(std::shared_ptr<Layer> rootLayer,
                                 uint32_t rasterizerTracingThreshold,
                                 bool checkerboardRasterCacheImages,
                                 bool checkerboardOffscreenLayers) {
  return fml::MakeRefCounted<Scene>(
      std::move(rootLayer), rasterizerTracingThreshold,
      checkerboardRasterCacheImages, checkerboardOffscreenLayers);
}

Scene::Scene(std::shared_ptr<Layer> rootLayer,
             uint32_t rasterizerTracingThreshold,
             bool checkerboardRasterCacheImages,
             bool checkerboardOffscreenLayers) {
  auto viewport_metrics = UIMonoState::Current()->window()->viewport_metrics();

  layer_tree_ = std::make_unique<LayerTree>(
      SkISize::Make(viewport_metrics.physical_width,
                    viewport_metrics.physical_height),
      static_cast<float>(viewport_metrics.physical_depth),
      static_cast<float>(viewport_metrics.device_pixel_ratio));
  layer_tree_->set_root_layer(std::move(rootLayer));
  layer_tree_->set_rasterizer_tracing_threshold(rasterizerTracingThreshold);
  layer_tree_->set_checkerboard_raster_cache_images(
      checkerboardRasterCacheImages);
  layer_tree_->set_checkerboard_offscreen_layers(checkerboardOffscreenLayers);
}

Scene::~Scene() {}

void Scene::dispose() {}

const char* Scene::toImage(uint32_t width, uint32_t height,
                           Picture::RawImageCallback raw_image_callback,
                           Mono_Handle callback_handle) {
  TRACE_EVENT0("uiwidgets", "Scene::toImage");

  if (!layer_tree_) {
    return "Scene did not contain a layer tree.";
  }

  auto picture = layer_tree_->Flatten(SkRect::MakeWH(width, height));
  if (!picture) {
    return "Could not flatten scene into a layer tree.";
  }

  return Picture::RasterizeToImage(picture, width, height, raw_image_callback,
                                   callback_handle);
}

std::unique_ptr<LayerTree> Scene::takeLayerTree() {
  return std::move(layer_tree_);
}

UIWIDGETS_API(void) Scene_dispose(Scene* ptr) { ptr->Release(); }

}  // namespace uiwidgets
