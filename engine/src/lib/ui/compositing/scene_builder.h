#pragma once

#include <stdint.h>

#include <memory>
#include <vector>

#include "flow/layers/container_layer.h"
#include "lib/ui/compositing/scene.h"
#include "lib/ui/painting/color_filter.h"
#include "lib/ui/painting/engine_layer.h"
#include "lib/ui/painting/image_filter.h"
#include "lib/ui/painting/path.h"
#include "lib/ui/painting/picture.h"
#include "lib/ui/painting/rrect.h"
#include "lib/ui/painting/shader.h"

namespace uiwidgets {

class SceneBuilder : public fml::RefCountedThreadSafe<SceneBuilder> {
  FML_FRIEND_MAKE_REF_COUNTED(SceneBuilder);

 public:
  static fml::RefPtr<SceneBuilder> create() {
    return fml::MakeRefCounted<SceneBuilder>();
  }
  ~SceneBuilder();

  fml::RefPtr<EngineLayer> pushTransform(const float* matrix4);

  fml::RefPtr<EngineLayer> pushOffset(float dx, float dy);
  fml::RefPtr<EngineLayer> pushClipRect(float left, float right, float top,
                                        float bottom, int clipBehavior);
  fml::RefPtr<EngineLayer> pushClipRRect(const RRect& rrect, int clipBehavior);
  fml::RefPtr<EngineLayer> pushClipPath(const CanvasPath* path,
                                        int clipBehavior);
  fml::RefPtr<EngineLayer> pushOpacity(int alpha, float dx = 0, float dy = 0);
  fml::RefPtr<EngineLayer> pushColorFilter(const ColorFilter* color_filter);
  fml::RefPtr<EngineLayer> pushImageFilter(const ImageFilter* image_filter);
  fml::RefPtr<EngineLayer> pushBackdropFilter(ImageFilter* filter);
  fml::RefPtr<EngineLayer> pushShaderMask(Shader* shader, float maskRectLeft,
                                          float maskRectRight,
                                          float maskRectTop,
                                          float maskRectBottom, int blendMode);
  fml::RefPtr<EngineLayer> pushPhysicalShape(const CanvasPath* path,
                                             float elevation, int color,
                                             int shadowColor, int clipBehavior);

  void addRetained(fml::RefPtr<EngineLayer> retainedLayer);

  void pop();

  void addPerformanceOverlay(uint64_t enabledOptions, float left, float right,
                             float top, float bottom);

  void addPicture(float dx, float dy, Picture* picture, int hints);

  void addTexture(float dx, float dy, float width, float height,
                  int64_t textureId, bool freeze);

  void addPlatformView(float dx, float dy, float width, float height,
                       int64_t viewId);

  void setRasterizerTracingThreshold(uint32_t frameInterval);
  void setCheckerboardRasterCacheImages(bool checkerboard);
  void setCheckerboardOffscreenLayers(bool checkerboard);

  fml::RefPtr<Scene> build();

 private:
  SceneBuilder();

  void AddLayer(std::shared_ptr<Layer> layer);
  void PushLayer(std::shared_ptr<ContainerLayer> layer);
  void PopLayer();

  std::vector<std::shared_ptr<ContainerLayer>> layer_stack_;
  int rasterizer_tracing_threshold_ = 0;
  bool checkerboard_raster_cache_images_ = false;
  bool checkerboard_offscreen_layers_ = false;

  FML_DISALLOW_COPY_AND_ASSIGN(SceneBuilder);
};

}  // namespace uiwidgets
