#pragma once

#include <flutter/fml/memory/ref_counted.h>
#include <flutter/fml/memory/ref_ptr.h>

#include "include/core/SkCanvas.h"
#include "picture.h"
#include "picture_recorder.h"
#include "paint.h"
#include "path.h"
#include "picture.h"
#include "picture_recorder.h"
#include "rrect.h"
#include "vertices.h"

namespace uiwidgets {

class Canvas : public fml::RefCountedThreadSafe<Canvas> {
  FML_FRIEND_MAKE_REF_COUNTED(Canvas);

 public:
  static fml::RefPtr<Canvas> Create(PictureRecorder* recorder, float left,
                                    float top, float right, float bottom);

  ~Canvas();

  void save();
  void saveLayerWithoutBounds(const Paint& paint);
  void saveLayer(float left, float top, float right, float bottom,
                 const Paint& paint);
  void restore();
  int getSaveCount();

  void translate(float dx, float dy);
  void scale(float sx, float sy);
  void rotate(float radians);
  void skew(float sx, float sy);
  void transform(float* matrix4);

  void clipRect(float left, float top, float right, float bottom,
                SkClipOp clipOp, bool doAntiAlias = true);
  void clipRRect(const RRect& rrect, bool doAntiAlias = true);
  void clipPath(const CanvasPath* path, bool doAntiAlias = true);

  void drawColor(SkColor color, SkBlendMode blend_mode);
  void drawLine(float x1, float y1, float x2, float y2, const Paint& paint);
  void drawPaint(const Paint& paint);
  void drawRect(float left, float top, float right, float bottom,
                const Paint& paint);
  void drawRRect(const RRect& rrect, const Paint& paint);
  void drawDRRect(const RRect& outer, const RRect& inner, const Paint& paint);
  void drawOval(float left, float top, float right, float bottom,
                const Paint& paint);
  void drawCircle(float x, float y, float radius, const Paint& paint);
  void drawArc(float left, float top, float right, float bottom,
               float startAngle, float sweepAngle, bool useCenter,
               const Paint& paint);
  void drawPath(const CanvasPath* path, const Paint& paint);
  void drawImage(const CanvasImage* image, float x, float y,
                 const Paint& paint);
  void drawImageRect(const CanvasImage* image, float src_left, float src_top,
                     float src_right, float src_bottom, float dst_left,
                     float dst_top, float dst_right, float dst_bottom,
                     const Paint& paint);
  void drawImageNine(const CanvasImage* image, float center_left,
                     float center_top, float center_right,
                     float center_bottom, float dst_left, float dst_top,
                     float dst_right, float dst_bottom, const Paint& paint);
  void drawPicture(Picture* picture);

  // The paint argument is first for the following functions because Paint
  // unwraps a number of C++ objects. Once we create a view unto a
  // Float32List, we cannot re-enter the VM to unwrap objects. That means we
  // either need to process the paint argument first.

  void drawPoints(const Paint& paint, SkCanvas::PointMode point_mode,
                  float* points, int points_length);

  void drawVertices(const Vertices* vertices, SkBlendMode blend_mode,
                    const Paint& paint);

  void drawAtlas(const Paint& paint, CanvasImage* atlas, float* transforms,
                 int transforms_length, float* rects, int rects_length,
                 int32_t* colors, int colors_length, SkBlendMode blend_mode,
                 float* cull_rect);

  void drawShadow(const CanvasPath* path, SkColor color, float elevation,
                  bool transparentOccluder);

  SkCanvas* canvas() const { return canvas_; }
  void Clear();
  bool IsRecording() const;

 private:
  explicit Canvas(SkCanvas* canvas);

  SkCanvas* canvas_;
};

}  // namespace uiwidgets
