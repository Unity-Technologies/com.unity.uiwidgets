#pragma once

#include <stdint.h>

#include <memory>

#include "flow/layers/layer_tree.h"
#include "lib/ui/painting/picture.h"

namespace uiwidgets {

class Scene : public fml::RefCountedThreadSafe<Scene> {
  FML_FRIEND_MAKE_REF_COUNTED(Scene);

 public:
  ~Scene();
  static fml::RefPtr<Scene> create(std::shared_ptr<Layer> rootLayer,
                                   uint32_t rasterizerTracingThreshold,
                                   bool checkerboardRasterCacheImages,
                                   bool checkerboardOffscreenLayers);

  std::unique_ptr<LayerTree> takeLayerTree();

  const char* toImage(uint32_t width, uint32_t height,
                      Picture::RawImageCallback raw_image_callback,
                      Mono_Handle callback_handle);

  void dispose();

 private:
  explicit Scene(std::shared_ptr<Layer> rootLayer,
                 uint32_t rasterizerTracingThreshold,
                 bool checkerboardRasterCacheImages,
                 bool checkerboardOffscreenLayers);

  std::unique_ptr<LayerTree> layer_tree_;
};

}  // namespace uiwidgets
