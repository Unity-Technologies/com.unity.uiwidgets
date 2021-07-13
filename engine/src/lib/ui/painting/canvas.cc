#include "canvas.h"

#define _USE_MATH_DEFINES
#include <math.h>

#include "flow/layers/physical_shape_layer.h"
#include "image.h"
#include "include/core/SkBitmap.h"
#include "include/core/SkCanvas.h"
#include "include/core/SkRSXform.h"
#include "lib/ui/ui_mono_state.h"
#include "lib/ui/window/window.h"
#include "matrix.h"
#include "picture_recorder.h"

namespace uiwidgets {

fml::RefPtr<Canvas> Canvas::Create(PictureRecorder* recorder, float left,
                                   float top, float right, float bottom) {
  if (!recorder)
    Mono_ThrowException(
        "Canvas constructor called with non-genuine PictureRecorder.");

  FML_DCHECK(!recorder->isRecording());
  fml::RefPtr<Canvas> canvas = fml::MakeRefCounted<Canvas>(
      recorder->BeginRecording(SkRect::MakeLTRB(left, top, right, bottom)));
  recorder->set_canvas(canvas);
  return canvas;
}

Canvas::Canvas(SkCanvas* canvas) : canvas_(canvas) {}

Canvas::~Canvas() {}

void Canvas::save() {
  if (!canvas_) return;
  canvas_->save();
}

void Canvas::saveLayerWithoutBounds(const Paint& paint) {
  if (!canvas_) return;
  canvas_->saveLayer(nullptr, paint.paint());
}

void Canvas::saveLayer(float left, float top, float right, float bottom,
                       const Paint& paint) {
  if (!canvas_) return;
  SkRect bounds = SkRect::MakeLTRB(left, top, right, bottom);
  canvas_->saveLayer(&bounds, paint.paint());
}

void Canvas::restore() {
  if (!canvas_) return;
  canvas_->restore();
}

int Canvas::getSaveCount() {
  if (!canvas_) return 0;
  return canvas_->getSaveCount();
}

void Canvas::translate(float dx, float dy) {
  if (!canvas_) return;
  canvas_->translate(dx, dy);
}

void Canvas::scale(float sx, float sy) {
  if (!canvas_) return;
  canvas_->scale(sx, sy);
}

void Canvas::rotate(float radians) {
  if (!canvas_) return;
  canvas_->rotate(radians * 180.0 / M_PI);
}

void Canvas::skew(float sx, float sy) {
  if (!canvas_) return;
  canvas_->skew(sx, sy);
}

void Canvas::transform(float* matrix4) {
  if (!canvas_) return;
  canvas_->concat(ToSkMatrix(matrix4));
}

void Canvas::clipRect(float left, float top, float right, float bottom,
                      SkClipOp clipOp, bool doAntiAlias) {
  if (!canvas_) return;
  canvas_->clipRect(SkRect::MakeLTRB(left, top, right, bottom), clipOp,
                    doAntiAlias);
}

void Canvas::clipRRect(const RRect& rrect, bool doAntiAlias) {
  if (!canvas_) return;
  canvas_->clipRRect(rrect.sk_rrect, doAntiAlias);
}

void Canvas::clipPath(const CanvasPath* path, bool doAntiAlias) {
  if (!canvas_) return;
  if (!path)
    Mono_ThrowException("Canvas.clipPath called with non-genuine Path.");
  canvas_->clipPath(path->path(), doAntiAlias);
}

void Canvas::drawColor(SkColor color, SkBlendMode blend_mode) {
  if (!canvas_) return;
  canvas_->drawColor(color, blend_mode);
}

void Canvas::drawLine(float x1, float y1, float x2, float y2,
                      const Paint& paint) {
  if (!canvas_) return;
  canvas_->drawLine(x1, y1, x2, y2, *paint.paint());
}

void Canvas::drawPaint(const Paint& paint) {
  if (!canvas_) return;
  canvas_->drawPaint(*paint.paint());
}

void Canvas::drawRect(float left, float top, float right, float bottom,
                      const Paint& paint) {
  if (!canvas_) return;
  canvas_->drawRect(SkRect::MakeLTRB(left, top, right, bottom), *paint.paint());
}

void Canvas::drawRRect(const RRect& rrect, const Paint& paint) {
  if (!canvas_) return;
  canvas_->drawRRect(rrect.sk_rrect, *paint.paint());
}

void Canvas::drawDRRect(const RRect& outer, const RRect& inner,
                        const Paint& paint) {
  if (!canvas_) return;
  canvas_->drawDRRect(outer.sk_rrect, inner.sk_rrect, *paint.paint());
}

void Canvas::drawOval(float left, float top, float right, float bottom,
                      const Paint& paint) {
  if (!canvas_) return;
  canvas_->drawOval(SkRect::MakeLTRB(left, top, right, bottom), *paint.paint());
}

void Canvas::drawCircle(float x, float y, float radius, const Paint& paint) {
  if (!canvas_) return;
  canvas_->drawCircle(x, y, radius, *paint.paint());
}

void Canvas::drawArc(float left, float top, float right, float bottom,
                     float startAngle, float sweepAngle, bool useCenter,
                     const Paint& paint) {
  if (!canvas_) return;
  canvas_->drawArc(SkRect::MakeLTRB(left, top, right, bottom),
                   startAngle * 180.0 / M_PI, sweepAngle * 180.0 / M_PI,
                   useCenter, *paint.paint());
}

void Canvas::drawPath(const CanvasPath* path, const Paint& paint) {
  if (!canvas_) return;
  if (!path)
    Mono_ThrowException("Canvas.drawPath called with non-genuine Path.");
  canvas_->drawPath(path->path(), *paint.paint());
}

void Canvas::drawImage(const CanvasImage* image, float x, float y,
                       const Paint& paint) {
  if (!canvas_) return;
  if (!image)
    Mono_ThrowException("Canvas.drawImage called with non-genuine Image.");
  canvas_->drawImage(image->image(), x, y, paint.paint());
}

void Canvas::drawImageRect(const CanvasImage* image, float src_left,
                           float src_top, float src_right, float src_bottom,
                           float dst_left, float dst_top, float dst_right,
                           float dst_bottom, const Paint& paint) {
  if (!canvas_) return;
  if (!image)
    Mono_ThrowException("Canvas.drawImageRect called with non-genuine Image.");
  SkRect src = SkRect::MakeLTRB(src_left, src_top, src_right, src_bottom);
  SkRect dst = SkRect::MakeLTRB(dst_left, dst_top, dst_right, dst_bottom);
  canvas_->drawImageRect(image->image(), src, dst, paint.paint(),
                         SkCanvas::kFast_SrcRectConstraint);
}

void Canvas::drawImageNine(const CanvasImage* image, float center_left,
                           float center_top, float center_right,
                           float center_bottom, float dst_left, float dst_top,
                           float dst_right, float dst_bottom,
                           const Paint& paint) {
  if (!canvas_) return;
  if (!image)
    Mono_ThrowException("Canvas.drawImageNine called with non-genuine Image.");
  SkRect center =
      SkRect::MakeLTRB(center_left, center_top, center_right, center_bottom);
  SkIRect icenter;
  center.round(&icenter);
  SkRect dst = SkRect::MakeLTRB(dst_left, dst_top, dst_right, dst_bottom);
  canvas_->drawImageNine(image->image(), icenter, dst, paint.paint());
}

void Canvas::drawPicture(Picture* picture) {
  if (!canvas_) return;
  if (!picture)
    Mono_ThrowException("Canvas.drawPicture called with non-genuine Picture.");
  canvas_->drawPicture(picture->picture().get());
}

void Canvas::drawPoints(const Paint& paint, SkCanvas::PointMode point_mode,
                        float* points, int points_length) {
  if (!canvas_) return;

  static_assert(sizeof(SkPoint) == sizeof(float) * 2,
                "SkPoint doesn't use floats.");

  canvas_->drawPoints(point_mode,
                      points_length / 2,  // SkPoints have two floats.
                      reinterpret_cast<const SkPoint*>(points), *paint.paint());
}

void Canvas::drawVertices(const Vertices* vertices, SkBlendMode blend_mode,
                          const Paint& paint) {
  if (!canvas_) return;
  if (!vertices)
    Mono_ThrowException(
        "Canvas.drawVertices called with non-genuine Vertices.");

  canvas_->drawVertices(vertices->vertices(), blend_mode, *paint.paint());
}

void Canvas::drawAtlas(const Paint& paint, CanvasImage* atlas,
                       float* transforms, int transforms_length, float* rects,
                       int rects_length, int32_t* colors, int colors_length,
                       SkBlendMode blend_mode, float* cull_rect) {
  if (!canvas_) return;
  if (!atlas)
    Mono_ThrowException(
        "Canvas.drawAtlas or Canvas.drawRawAtlas called with "
        "non-genuine Image.");

  sk_sp<SkImage> skImage = atlas->image();

  static_assert(sizeof(SkRSXform) == sizeof(float) * 4,
                "SkRSXform doesn't use floats.");
  static_assert(sizeof(SkRect) == sizeof(float) * 4,
                "SkRect doesn't use floats.");

  canvas_->drawAtlas(
      skImage.get(), reinterpret_cast<const SkRSXform*>(transforms),
      reinterpret_cast<const SkRect*>(rects),
      reinterpret_cast<const SkColor*>(colors),
      rects_length / 4,  // SkRect have four floats.
      blend_mode, reinterpret_cast<const SkRect*>(cull_rect), paint.paint());
}

void Canvas::drawShadow(const CanvasPath* path, SkColor color, float elevation,
                        bool transparentOccluder) {
  if (!path)
    Mono_ThrowException("Canvas.drawShader called with non-genuine Path.");
  SkScalar dpr = 1.f;
  // TODO:
  // UIMonoState::Current()->window()->viewport_metrics().device_pixel_ratio;

  PhysicalShapeLayer::DrawShadow(canvas_, path->path(), color, elevation,
                                 transparentOccluder, dpr);
}

void Canvas::Clear() { canvas_ = nullptr; }

bool Canvas::IsRecording() const { return !!canvas_; }

UIWIDGETS_API(Canvas*)
Canvas_constructor(PictureRecorder* recorder, float left, float top,
                   float right, float bottom) {
  const auto canvas = Canvas::Create(recorder, left, top, right, bottom);
  canvas->AddRef();
  return canvas.get();
}

UIWIDGETS_API(void) Canvas_dispose(Canvas* ptr) { ptr->Release(); }

UIWIDGETS_API(void) Canvas_save(Canvas* ptr) { ptr->save(); }

UIWIDGETS_API(void)
Canvas_saveLayerWithoutBounds(Canvas* ptr, void** paint_objects,
                              uint8_t* paint_data) {
  const Paint paint(paint_objects, paint_data);
  ptr->saveLayerWithoutBounds(paint);
}

UIWIDGETS_API(void)
Canvas_saveLayer(Canvas* ptr, float left, float top, float right, float bottom,
                 void** paint_objects, uint8_t* paint_data) {
  ptr->saveLayer(left, top, right, bottom, Paint(paint_objects, paint_data));
}

UIWIDGETS_API(void) Canvas_restore(Canvas* ptr) { ptr->restore(); }

UIWIDGETS_API(int) Canvas_getSaveCount(Canvas* ptr) {
  return ptr->getSaveCount();
}

UIWIDGETS_API(void) Canvas_translate(Canvas* ptr, float x, float y) {
  ptr->translate(x, y);
}

UIWIDGETS_API(void)
Canvas_scale(Canvas* ptr, float sx, float sy) { ptr->scale(sx, sy); }

UIWIDGETS_API(void)
Canvas_rotate(Canvas* ptr, float radians) { ptr->rotate(radians); }

UIWIDGETS_API(void)
Canvas_skew(Canvas* ptr, float sx, float sy) { ptr->skew(sx, sy); }

UIWIDGETS_API(void)
Canvas_transform(Canvas* ptr, float* matrix4) { ptr->transform(matrix4); }

UIWIDGETS_API(void)
Canvas_clipRect(Canvas* ptr, float left, float top, float right, float bottom,
                SkClipOp clipOp, bool doAntiAlias) {
  ptr->clipRect(left, top, right, bottom, clipOp, doAntiAlias);
}

UIWIDGETS_API(void)
Canvas_clipRRect(Canvas* ptr, float* rrect, bool doAntiAlias) {
  ptr->clipRRect(RRect(rrect), doAntiAlias);
}

UIWIDGETS_API(void)
Canvas_clipPath(Canvas* ptr, CanvasPath* path, bool doAntiAlias) {
  ptr->clipPath(path, doAntiAlias);
}

UIWIDGETS_API(void)
Canvas_drawColor(Canvas* ptr, SkColor color, SkBlendMode blendMode) {
  ptr->drawColor(color, blendMode);
}

UIWIDGETS_API(void)
Canvas_drawLine(Canvas* ptr, float x1, float y1, float x2, float y2,
                void** paint_objects, uint8_t* paint_data) {
  ptr->drawLine(x1, y1, x2, y2, Paint(paint_objects, paint_data));
}

UIWIDGETS_API(void)
Canvas_drawPaint(Canvas* ptr, void** paint_objects, uint8_t* paint_data) {
  ptr->drawPaint(Paint(paint_objects, paint_data));
}

UIWIDGETS_API(void)
Canvas_drawRect(Canvas* ptr, float left, float top, float right, float bottom,
                void** paint_objects, uint8_t* paint_data) {
  ptr->drawRect(left, top, right, bottom, Paint(paint_objects, paint_data));
}

UIWIDGETS_API(void)
Canvas_drawRRect(Canvas* ptr, float* rrect, void** paint_objects,
                 uint8_t* paint_data) {
  ptr->drawRRect(RRect(rrect), Paint(paint_objects, paint_data));
}

UIWIDGETS_API(void)
Canvas_drawDRRect(Canvas* ptr, float* outer, float* inner, void** paint_objects,
                  uint8_t* paint_data) {
  ptr->drawDRRect(RRect(outer), RRect(inner), Paint(paint_objects, paint_data));
}

UIWIDGETS_API(void)
Canvas_drawOval(Canvas* ptr, float left, float top, float right, float bottom,
                void** paint_objects, uint8_t* paint_data) {
  ptr->drawOval(left, top, right, bottom, Paint(paint_objects, paint_data));
}

UIWIDGETS_API(void)
Canvas_drawCircle(Canvas* ptr, float x, float y, float radius,
                  void** paint_objects, uint8_t* paint_data) {
  ptr->drawCircle(x, y, radius, Paint(paint_objects, paint_data));
}

UIWIDGETS_API(void)
Canvas_drawArc(Canvas* ptr, float left, float top, float right, float bottom,
               float startAngle, float sweepAngle, bool useCenter,
               void** paint_objects, uint8_t* paint_data) {
  ptr->drawArc(left, top, right, bottom, startAngle, sweepAngle, useCenter,
               Paint(paint_objects, paint_data));
}

UIWIDGETS_API(void)
Canvas_drawPath(Canvas* ptr, CanvasPath* path, void** paint_objects,
                uint8_t* paint_data) {
  ptr->drawPath(path, Paint(paint_objects, paint_data));
}

UIWIDGETS_API(void)
Canvas_drawImage(Canvas* ptr, CanvasImage* image, float x, float y,
                 void** paint_objects, uint8_t* paint_data) {
  ptr->drawImage(image, x, y, Paint(paint_objects, paint_data));
}

UIWIDGETS_API(void)
Canvas_drawImageRect(Canvas* ptr, CanvasImage* image, float srcLeft,
                     float srcTop, float srcRight, float srcBottom,
                     float dstLeft, float dstTop, float dstRight,
                     float dstBottom, void** paint_objects,
                     uint8_t* paint_data) {
  ptr->drawImageRect(image, srcLeft, srcTop, srcRight, srcBottom, dstLeft,
                     dstTop, dstRight, dstBottom,
                     Paint(paint_objects, paint_data));
}

UIWIDGETS_API(void)
Canvas_drawImageNine(Canvas* ptr, CanvasImage* image, float centerLeft,
                     float centerTop, float centerRight, float centerBottom,
                     float dstLeft, float dstTop, float dstRight,
                     float dstBottom, void** paint_objects,
                     uint8_t* paint_data) {
  ptr->drawImageNine(image, centerLeft, centerTop, centerRight, centerBottom,
                     dstLeft, dstTop, dstRight, dstBottom,
                     Paint(paint_objects, paint_data));
}

UIWIDGETS_API(void)
Canvas_drawPicture(Canvas* ptr, Picture* picture) { ptr->drawPicture(picture); }

UIWIDGETS_API(void)
Canvas_drawPoints(Canvas* ptr, void** paint_objects, uint8_t* paint_data,
                  SkCanvas::PointMode pointMode, float* points,
                  int pointsLength) {
  ptr->drawPoints(Paint(paint_objects, paint_data), pointMode, points,
                  pointsLength);
}

UIWIDGETS_API(void)
Canvas_drawVertices(Canvas* ptr, Vertices* vertices, SkBlendMode blendMode,
                    void** paint_objects, uint8_t* paint_data) {
  ptr->drawVertices(vertices, blendMode, Paint(paint_objects, paint_data));
}

UIWIDGETS_API(void)
Canvas_drawAtlas(Canvas* ptr, void** paint_objects, uint8_t* paint_data,
                 CanvasImage* atlas, float* rstTransforms,
                 int rstTransformsLength, float* rects, int rectsLength,
                 int32_t* colors, int colorsLength, SkBlendMode blendMode,
                 float* cullRect) {
  ptr->drawAtlas(Paint(paint_objects, paint_data), atlas, rstTransforms,
                 rstTransformsLength, rects, rectsLength, colors, colorsLength,
                 blendMode, cullRect);
}

UIWIDGETS_API(void)
Canvas_drawShadow(Canvas* ptr, CanvasPath* path, SkColor color, float elevation,
                  bool transparentOccluder) {
  ptr->drawShadow(path, color, elevation, transparentOccluder);
}

}  // namespace uiwidgets
