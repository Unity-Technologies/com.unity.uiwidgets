
#include "scene_builder.h"

#include "flow/layers/backdrop_filter_layer.h"
#include "flow/layers/clip_path_layer.h"
#include "flow/layers/clip_rect_layer.h"
#include "flow/layers/clip_rrect_layer.h"
#include "flow/layers/color_filter_layer.h"
#include "flow/layers/container_layer.h"
#include "flow/layers/image_filter_layer.h"
#include "flow/layers/layer.h"
#include "flow/layers/layer_tree.h"
#include "flow/layers/opacity_layer.h"
#include "flow/layers/performance_overlay_layer.h"
#include "flow/layers/physical_shape_layer.h"
#include "flow/layers/picture_layer.h"
#include "flow/layers/platform_view_layer.h"
#include "flow/layers/shader_mask_layer.h"
#include "flow/layers/texture_layer.h"
#include "flow/layers/transform_layer.h"
#include "include/core/SkColorFilter.h"
#include "lib/ui/painting/matrix.h"
#include "lib/ui/painting/shader.h"

namespace uiwidgets {

SceneBuilder::SceneBuilder() { PushLayer(std::make_shared<ContainerLayer>()); }

SceneBuilder::~SceneBuilder() = default;

fml::RefPtr<EngineLayer> SceneBuilder::pushTransform(const float* matrix4) {
  SkMatrix sk_matrix = ToSkMatrix(matrix4);
  auto layer = std::make_shared<TransformLayer>(sk_matrix);
  PushLayer(layer);
  return EngineLayer::MakeRetained(layer);
}

fml::RefPtr<EngineLayer> SceneBuilder::pushOffset(float dx, float dy) {
  SkMatrix sk_matrix = SkMatrix::MakeTrans(dx, dy);
  auto layer = std::make_shared<TransformLayer>(sk_matrix);
  PushLayer(layer);
  return EngineLayer::MakeRetained(layer);
}

fml::RefPtr<EngineLayer> SceneBuilder::pushClipRect(float left, float right,
                                                    float top, float bottom,
                                                    int clipBehavior) {
  SkRect clipRect = SkRect::MakeLTRB(left, top, right, bottom);
  Clip clip_behavior = static_cast<Clip>(clipBehavior);
  auto layer = std::make_shared<ClipRectLayer>(clipRect, clip_behavior);
  PushLayer(layer);
  return EngineLayer::MakeRetained(layer);
}

fml::RefPtr<EngineLayer> SceneBuilder::pushClipRRect(const RRect& rrect,
                                                     int clipBehavior) {
  Clip clip_behavior = static_cast<Clip>(clipBehavior);
  auto layer = std::make_shared<ClipRRectLayer>(rrect.sk_rrect, clip_behavior);
  PushLayer(layer);
  return EngineLayer::MakeRetained(layer);
}

fml::RefPtr<EngineLayer> SceneBuilder::pushClipPath(const CanvasPath* path,
                                                    int clipBehavior) {
  Clip clip_behavior = static_cast<Clip>(clipBehavior);
  FML_DCHECK(clip_behavior != Clip::none);
  auto layer = std::make_shared<ClipPathLayer>(path->path(), clip_behavior);
  PushLayer(layer);
  return EngineLayer::MakeRetained(layer);
}

fml::RefPtr<EngineLayer> SceneBuilder::pushOpacity(int alpha, float dx,
                                                   float dy) {
  auto layer = std::make_shared<OpacityLayer>(alpha, SkPoint::Make(dx, dy));
  PushLayer(layer);
  return EngineLayer::MakeRetained(layer);
}

fml::RefPtr<EngineLayer> SceneBuilder::pushColorFilter(

    const ColorFilter* color_filter) {
  auto layer = std::make_shared<ColorFilterLayer>(color_filter->filter());
  PushLayer(layer);
  return EngineLayer::MakeRetained(layer);
}

fml::RefPtr<EngineLayer> SceneBuilder::pushImageFilter(
    const ImageFilter* image_filter) {
  auto layer = std::make_shared<ImageFilterLayer>(image_filter->filter());
  PushLayer(layer);
  return EngineLayer::MakeRetained(layer);
}

fml::RefPtr<EngineLayer> SceneBuilder::pushBackdropFilter(ImageFilter* filter) {
  auto layer = std::make_shared<BackdropFilterLayer>(filter->filter());
  PushLayer(layer);
  return EngineLayer::MakeRetained(layer);
}

fml::RefPtr<EngineLayer> SceneBuilder::pushShaderMask(
    Shader* shader, float maskRectLeft, float maskRectRight, float maskRectTop,
    float maskRectBottom, int blendMode) {
  SkRect rect = SkRect::MakeLTRB(maskRectLeft, maskRectTop, maskRectRight,
                                 maskRectBottom);
  auto layer = std::make_shared<ShaderMaskLayer>(
      shader->shader(), rect, static_cast<SkBlendMode>(blendMode));
  PushLayer(layer);
  return EngineLayer::MakeRetained(layer);
}

fml::RefPtr<EngineLayer> SceneBuilder::pushPhysicalShape(const CanvasPath* path,
                                                         float elevation,
                                                         int color,
                                                         int shadow_color,
                                                         int clipBehavior) {
  auto layer = std::make_shared<PhysicalShapeLayer>(
      static_cast<SkColor>(color), static_cast<SkColor>(shadow_color),
      static_cast<float>(elevation), path->path(),
      static_cast<Clip>(clipBehavior));
  PushLayer(layer);
  return EngineLayer::MakeRetained(layer);
}

void SceneBuilder::addRetained(fml::RefPtr<EngineLayer> retainedLayer) {
  AddLayer(retainedLayer->Layer());
}

void SceneBuilder::pop() { PopLayer(); }

void SceneBuilder::addPicture(float dx, float dy, Picture* picture, int hints) {
  SkPoint offset = SkPoint::Make(dx, dy);
  SkRect pictureRect = picture->picture()->cullRect();
  pictureRect.offset(offset.x(), offset.y());
  auto layer = std::make_unique<PictureLayer>(
      offset, UIMonoState::CreateGPUObject(picture->picture()), !!(hints & 1),
      !!(hints & 2));
  AddLayer(std::move(layer));
}

void SceneBuilder::addTexture(float dx, float dy, float width, float height,
                              int64_t textureId, bool freeze) {
  auto layer = std::make_unique<TextureLayer>(
      SkPoint::Make(dx, dy), SkSize::Make(width, height), textureId, freeze);
  AddLayer(std::move(layer));
}

void SceneBuilder::addPlatformView(float dx, float dy, float width,
                                   float height, int64_t viewId) {
  auto layer = std::make_unique<PlatformViewLayer>(
      SkPoint::Make(dx, dy), SkSize::Make(width, height), viewId);
  AddLayer(std::move(layer));
}

void SceneBuilder::addPerformanceOverlay(uint64_t enabledOptions, float left,
                                         float right, float top, float bottom) {
  SkRect rect = SkRect::MakeLTRB(left, top, right, bottom);
  auto layer = std::make_unique<PerformanceOverlayLayer>(enabledOptions);
  layer->set_paint_bounds(rect);
  AddLayer(std::move(layer));
}

void SceneBuilder::setRasterizerTracingThreshold(uint32_t frameInterval) {
  rasterizer_tracing_threshold_ = frameInterval;
}

void SceneBuilder::setCheckerboardRasterCacheImages(bool checkerboard) {
  checkerboard_raster_cache_images_ = checkerboard;
}

void SceneBuilder::setCheckerboardOffscreenLayers(bool checkerboard) {
  checkerboard_offscreen_layers_ = checkerboard;
}

fml::RefPtr<Scene> SceneBuilder::build() {
  FML_DCHECK(layer_stack_.size() >= 1);

  return Scene::create(layer_stack_[0], rasterizer_tracing_threshold_,
                       checkerboard_raster_cache_images_,
                       checkerboard_offscreen_layers_);
}

void SceneBuilder::AddLayer(std::shared_ptr<Layer> layer) {
  FML_DCHECK(layer);

  if (!layer_stack_.empty()) {
    layer_stack_.back()->Add(std::move(layer));
  }
}

void SceneBuilder::PushLayer(std::shared_ptr<ContainerLayer> layer) {
  AddLayer(layer);
  layer_stack_.push_back(std::move(layer));
}

void SceneBuilder::PopLayer() {
  // We never pop the root layer, so that AddLayer operations are always valid.
  if (layer_stack_.size() > 1) {
    layer_stack_.pop_back();
  }
}

UIWIDGETS_API(SceneBuilder*) SceneBuilder_constructor() {
  const auto builder = fml::MakeRefCounted<SceneBuilder>();
  builder->AddRef();
  return builder.get();
}

UIWIDGETS_API(void) SceneBuilder_dispose(SceneBuilder* ptr) { ptr->Release(); }

UIWIDGETS_API(EngineLayer*)
SceneBuilder_pushTransform(SceneBuilder* ptr, const float* matrix4) {
  const auto layer = ptr->pushTransform(matrix4);
  layer->AddRef();
  return layer.get();
}

UIWIDGETS_API(EngineLayer*)
SceneBuilder_pushOffset(SceneBuilder* ptr, float dx, float dy) {
  const auto layer = ptr->pushOffset(dx, dy);
  layer->AddRef();
  return layer.get();
}

UIWIDGETS_API(EngineLayer*)
SceneBuilder_pushClipRect(SceneBuilder* ptr, float left, float right, float top,
                          float bottom, int clipBehavior) {
  const auto layer = ptr->pushClipRect(left, right, top, bottom, clipBehavior);
  layer->AddRef();
  return layer.get();
}

UIWIDGETS_API(EngineLayer*)
SceneBuilder_pushClipRRect(SceneBuilder* ptr, float* rrect, int clipBehavior) {
  const auto layer = ptr->pushClipRRect(RRect(rrect), clipBehavior);
  layer->AddRef();
  return layer.get();
}

UIWIDGETS_API(EngineLayer*)
SceneBuilder_pushClipPath(SceneBuilder* ptr, CanvasPath* path,
                          int clipBehavior) {
  const auto layer = ptr->pushClipPath(path, clipBehavior);
  layer->AddRef();
  return layer.get();
}

UIWIDGETS_API(EngineLayer*)
SceneBuilder_pushOpacity(SceneBuilder* ptr, int alpha, float dx, float dy) {
  const auto layer = ptr->pushOpacity(alpha, dx, dy);
  layer->AddRef();
  return layer.get();
}

UIWIDGETS_API(EngineLayer*)
SceneBuilder_pushColorFilter(SceneBuilder* ptr, ColorFilter* filter) {
  const auto layer = ptr->pushColorFilter(filter);
  layer->AddRef();
  return layer.get();
}

UIWIDGETS_API(EngineLayer*)
SceneBuilder_pushImageFilter(SceneBuilder* ptr, ImageFilter* filter) {
  const auto layer = ptr->pushImageFilter(filter);
  layer->AddRef();
  return layer.get();
}

UIWIDGETS_API(EngineLayer*)
SceneBuilder_pushBackdropFilter(SceneBuilder* ptr, ImageFilter* filter) {
  const auto layer = ptr->pushBackdropFilter(filter);
  layer->AddRef();
  return layer.get();
}

UIWIDGETS_API(EngineLayer*)
SceneBuilder_pushShaderMask(SceneBuilder* ptr,
    Shader* shader, float maskRectLeft, float maskRectRight, float maskRectTop,
    float maskRectBottom, int blendMode) {
  const auto layer = ptr->pushShaderMask(
    shader, maskRectLeft, maskRectRight, maskRectTop,
    maskRectBottom, blendMode);
  layer->AddRef();
  return layer.get();
}

UIWIDGETS_API(EngineLayer*)
SceneBuilder_pushPhysicalShape(SceneBuilder* ptr, CanvasPath* path,
                               float elevation, int color, int shadowColor,
                               int clipBehavior) {
  const auto layer =
      ptr->pushPhysicalShape(path, elevation, color, shadowColor, clipBehavior);
  layer->AddRef();
  return layer.get();
}

UIWIDGETS_API(void)
SceneBuilder_addPerformanceOverlay(SceneBuilder* ptr, int enabledOptions,
                                   float left, float right, float top,
                                   float bottom) {
  ptr->addPerformanceOverlay(enabledOptions, left, right, top, bottom);
}

UIWIDGETS_API(void)
SceneBuilder_addRetained(SceneBuilder* ptr, EngineLayer* retainedLayer) {
  ptr->addRetained(static_cast<fml::RefPtr<EngineLayer>>(retainedLayer));
}

UIWIDGETS_API(void)
SceneBuilder_pop(SceneBuilder* ptr) { ptr->pop(); }

UIWIDGETS_API(Scene*) SceneBuilder_build(SceneBuilder* ptr) {
  const auto scene = ptr->build();
  scene->AddRef();
  return scene.get();
}

UIWIDGETS_API(void)
SceneBuilder_addPicture(SceneBuilder* ptr, float dx, float dy, Picture* picture,
                        int hints) {
  ptr->addPicture(dx, dy, picture, hints);
}

UIWIDGETS_API(void)
SceneBuilder_addTexture(SceneBuilder* ptr, float dx, float dy, float width,
                        float height, int texture_id, bool freeze) {
  ptr->addTexture(dx, dy, width, height, texture_id, freeze);
}

}  // namespace uiwidgets
