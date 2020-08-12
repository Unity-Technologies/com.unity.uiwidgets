#include "path.h"

#define _USE_MATH_DEFINES
#include <math.h>

#include "matrix.h"
#include "runtime/mono_state.h"

namespace uiwidgets {

typedef CanvasPath Path;

CanvasPath::CanvasPath() {}

CanvasPath::~CanvasPath() {}

int CanvasPath::getFillType() { return static_cast<int>(path_.getFillType()); }

void CanvasPath::setFillType(int fill_type) {
  path_.setFillType(static_cast<SkPathFillType>(fill_type));
}

void CanvasPath::moveTo(float x, float y) { path_.moveTo(x, y); }

void CanvasPath::relativeMoveTo(float x, float y) { path_.rMoveTo(x, y); }

void CanvasPath::lineTo(float x, float y) { path_.lineTo(x, y); }

void CanvasPath::relativeLineTo(float x, float y) { path_.rLineTo(x, y); }

void CanvasPath::quadraticBezierTo(float x1, float y1, float x2, float y2) {
  path_.quadTo(x1, y1, x2, y2);
}

void CanvasPath::relativeQuadraticBezierTo(float x1, float y1, float x2,
                                           float y2) {
  path_.rQuadTo(x1, y1, x2, y2);
}

void CanvasPath::cubicTo(float x1, float y1, float x2, float y2, float x3,
                         float y3) {
  path_.cubicTo(x1, y1, x2, y2, x3, y3);
}

void CanvasPath::relativeCubicTo(float x1, float y1, float x2, float y2,
                                 float x3, float y3) {
  path_.rCubicTo(x1, y1, x2, y2, x3, y3);
}

void CanvasPath::conicTo(float x1, float y1, float x2, float y2, float w) {
  path_.conicTo(x1, y1, x2, y2, w);
}

void CanvasPath::relativeConicTo(float x1, float y1, float x2, float y2,
                                 float w) {
  path_.rConicTo(x1, y1, x2, y2, w);
}

void CanvasPath::arcTo(float left, float top, float right, float bottom,
                       float startAngle, float sweepAngle, bool forceMoveTo) {
  path_.arcTo(SkRect::MakeLTRB(left, top, right, bottom),
              startAngle * 180.0f / M_PI, sweepAngle * 180.0f / M_PI,
              forceMoveTo);
}

void CanvasPath::arcToPoint(float arcEndX, float arcEndY, float radiusX,
                            float radiusY, float xAxisRotation, bool isLargeArc,
                            bool isClockwiseDirection) {
  const auto arcSize = isLargeArc ? SkPath::ArcSize::kLarge_ArcSize
                                  : SkPath::ArcSize::kSmall_ArcSize;
  const auto direction =
      isClockwiseDirection ? SkPathDirection::kCW : SkPathDirection::kCCW;

  path_.arcTo(radiusX, radiusY, xAxisRotation, arcSize, direction, arcEndX,
              arcEndY);
}

void CanvasPath::relativeArcToPoint(float arcEndDeltaX, float arcEndDeltaY,
                                    float radiusX, float radiusY,
                                    float xAxisRotation, bool isLargeArc,
                                    bool isClockwiseDirection) {
  const auto arcSize = isLargeArc ? SkPath::ArcSize::kLarge_ArcSize
                                  : SkPath::ArcSize::kSmall_ArcSize;
  const auto direction =
      isClockwiseDirection ? SkPathDirection::kCW : SkPathDirection::kCCW;
  path_.rArcTo(radiusX, radiusY, xAxisRotation, arcSize, direction,
               arcEndDeltaX, arcEndDeltaY);
}

void CanvasPath::addRect(float left, float top, float right, float bottom) {
  path_.addRect(SkRect::MakeLTRB(left, top, right, bottom));
}

void CanvasPath::addOval(float left, float top, float right, float bottom) {
  path_.addOval(SkRect::MakeLTRB(left, top, right, bottom));
}

void CanvasPath::addArc(float left, float top, float right, float bottom,
                        float startAngle, float sweepAngle) {
  path_.addArc(SkRect::MakeLTRB(left, top, right, bottom),
               startAngle * 180.0f / M_PI, sweepAngle * 180.0 / M_PI);
}

void CanvasPath::addPolygon(float* points, int points_length, bool close) {
  path_.addPoly(reinterpret_cast<const SkPoint*>(points), points_length / 2,
                close);
}

void CanvasPath::addRRect(const RRect& rrect) {
  path_.addRRect(rrect.sk_rrect);
}

void CanvasPath::addPath(CanvasPath* path, float dx, float dy) {
  if (!path) Mono_ThrowException("Path.addPath called with non-genuine Path.");
  path_.addPath(path->path(), dx, dy, SkPath::kAppend_AddPathMode);
}

void CanvasPath::addPathWithMatrix(CanvasPath* path, float dx, float dy,
                                   float* matrix4) {
  if (!path) {
    Mono_ThrowException("Path.addPathWithMatrix called with non-genuine Path.");
  }

  SkMatrix matrix = ToSkMatrix(matrix4);
  matrix.setTranslateX(matrix.getTranslateX() + dx);
  matrix.setTranslateY(matrix.getTranslateY() + dy);
  path_.addPath(path->path(), matrix, SkPath::kAppend_AddPathMode);
}

void CanvasPath::extendWithPath(CanvasPath* path, float dx, float dy) {
  if (!path)
    Mono_ThrowException("Path.extendWithPath called with non-genuine Path.");
  path_.addPath(path->path(), dx, dy, SkPath::kExtend_AddPathMode);
}

void CanvasPath::extendWithPathAndMatrix(CanvasPath* path, float dx, float dy,
                                         float* matrix4) {
  if (!path) {
    Mono_ThrowException("Path.addPathWithMatrix called with non-genuine Path.");
  }

  SkMatrix matrix = ToSkMatrix(matrix4);
  matrix.setTranslateX(matrix.getTranslateX() + dx);
  matrix.setTranslateY(matrix.getTranslateY() + dy);
  path_.addPath(path->path(), matrix, SkPath::kExtend_AddPathMode);
}

void CanvasPath::close() { path_.close(); }

void CanvasPath::reset() { path_.reset(); }

bool CanvasPath::contains(float x, float y) { return path_.contains(x, y); }

fml::RefPtr<CanvasPath> CanvasPath::shift(float dx, float dy) {
  fml::RefPtr<CanvasPath> path = CanvasPath::CreateNew();
  path_.offset(dx, dy, &path->path_);
  return path;
}

fml::RefPtr<CanvasPath> CanvasPath::transform(float* matrix4) {
  fml::RefPtr<CanvasPath> path = CanvasPath::CreateNew();
  path_.transform(ToSkMatrix(matrix4), &path->path_);
  return path;
}

void CanvasPath::getBounds(float* rect) {
  const SkRect& bounds = path_.getBounds();
  rect[0] = bounds.left();
  rect[1] = bounds.top();
  rect[2] = bounds.right();
  rect[3] = bounds.bottom();
}

bool CanvasPath::op(CanvasPath* path1, CanvasPath* path2, int operation) {
  return Op(path1->path(), path2->path(), (SkPathOp)operation, &path_);
}

fml::RefPtr<CanvasPath> CanvasPath::clone() {
  fml::RefPtr<CanvasPath> path = CanvasPath::CreateNew();
  // per Skia docs, this will create a fast copy
  // data is shared until the source path or dest path are mutated
  path->path_ = path_;
  return path;
}

UIWIDGETS_API(Path*) Path_constructor() {
  const auto path = Path::CreateNew();
  path->AddRef();
  return path.get();
}

UIWIDGETS_API(void) Path_dispose(Path* ptr) { ptr->Release(); }

UIWIDGETS_API(Path*) Path_clone(Path* ptr) {
  const auto path = ptr->clone();
  path->AddRef();
  return path.get();
}

UIWIDGETS_API(int) Path_getFillType(Path* ptr) { return ptr->getFillType(); }

UIWIDGETS_API(void) Path_setFillType(Path* ptr, int fillType) {
  ptr->setFillType(fillType);
}

UIWIDGETS_API(void) Path_moveTo(Path* ptr, float x, float y) {
  ptr->moveTo(x, y);
}

UIWIDGETS_API(void) Path_relativeMoveTo(Path* ptr, float dx, float dy) {
  ptr->relativeMoveTo(dx, dy);
}

UIWIDGETS_API(void) Path_lineTo(Path* ptr, float x, float y) {
  ptr->lineTo(x, y);
}

UIWIDGETS_API(void) Path_relativeLineTo(Path* ptr, float dx, float dy) {
  ptr->relativeLineTo(dx, dy);
}

UIWIDGETS_API(void)
Path_quadraticBezierTo(Path* ptr, float x1, float y1, float x2, float y2) {
  ptr->quadraticBezierTo(x1, y1, x2, y2);
}

UIWIDGETS_API(void)
Path_relativeQuadraticBezierTo(Path* ptr, float x1, float y1, float x2,
                               float y2) {
  ptr->relativeQuadraticBezierTo(x1, y1, x2, y2);
}

UIWIDGETS_API(void)
Path_cubicTo(Path* ptr, float x1, float y1, float x2, float y2, float x3,
             float y3) {
  ptr->cubicTo(x1, y1, x2, y2, x3, y3);
}

UIWIDGETS_API(void)
Path_relativeCubicTo(Path* ptr, float x1, float y1, float x2, float y2,
                     float x3, float y3) {
  ptr->relativeCubicTo(x1, y1, x2, y2, x3, y3);
}

UIWIDGETS_API(void)
Path_conicTo(Path* ptr, float x1, float y1, float x2, float y2, float w) {
  ptr->conicTo(x1, y1, x2, y2, w);
}

UIWIDGETS_API(void)
Path_relativeConicTo(Path* ptr, float x1, float y1, float x2, float y2,
                     float w) {
  ptr->relativeConicTo(x1, y1, x2, y2, w);
}

UIWIDGETS_API(void)
Path_arcTo(Path* ptr, float left, float top, float right, float bottom,
           float startAngle, float sweepAngle, bool forceMoveTo) {
  ptr->arcTo(left, top, right, bottom, startAngle, sweepAngle, forceMoveTo);
}

UIWIDGETS_API(void)
Path_arcToPoint(Path* ptr, float arcEndX, float arcEndY, float radiusX,
                float radiusY, float rotation, bool largeArc, bool clockwise) {
  ptr->arcToPoint(arcEndX, arcEndY, radiusX, radiusY, rotation, largeArc,
                  clockwise);
}

UIWIDGETS_API(void)
Path_relativeArcToPoint(Path* ptr, float arcEndX, float arcEndY, float radiusX,
                        float radiusY, float rotation, bool largeArc,
                        bool clockwise) {
  ptr->relativeArcToPoint(arcEndX, arcEndY, radiusX, radiusY, rotation,
                          largeArc, clockwise);
}

UIWIDGETS_API(void)
Path_addRect(Path* ptr, float left, float top, float right, float bottom) {
  ptr->addRect(left, top, right, bottom);
}

UIWIDGETS_API(void)
Path_addOval(Path* ptr, float left, float top, float right, float bottom) {
  ptr->addOval(left, top, right, bottom);
}

UIWIDGETS_API(void)
Path_addArc(Path* ptr, float left, float top, float right, float bottom,
            float startAngle, float sweepAngle) {
  ptr->addArc(left, top, right, bottom, startAngle, sweepAngle);
}

UIWIDGETS_API(void)
Path_addPolygon(Path* ptr, float* points, int pointsLength, bool close) {
  ptr->addPolygon(points, pointsLength, close);
}

UIWIDGETS_API(void)
Path_addRRect(Path* ptr, float* value32) { ptr->addRRect(RRect(value32)); }

UIWIDGETS_API(void)
Path_addPathWithMatrix(Path* ptr, Path* path, float dx, float dy,
                       float* matrix4) {
  ptr->addPathWithMatrix(path, dx, dy, matrix4);
}

UIWIDGETS_API(void)
Path_addPath(Path* ptr, Path* path, float dx, float dy) {
  ptr->addPath(path, dx, dy);
}

UIWIDGETS_API(void)
Path_extendWithPathAndMatrix(Path* ptr, Path* path, float dx, float dy,
                             float* matrix4) {
  ptr->extendWithPathAndMatrix(path, dx, dy, matrix4);
}

UIWIDGETS_API(void)
Path_extendWithPath(Path* ptr, Path* path, float dx, float dy) {
  ptr->extendWithPath(path, dx, dy);
}

UIWIDGETS_API(void)
Path_close(Path* ptr) { ptr->close(); }

UIWIDGETS_API(void)
Path_reset(Path* ptr) { ptr->reset(); }

UIWIDGETS_API(bool)
Path_contains(Path* ptr, float x, float y) { return ptr->contains(x, y); }

UIWIDGETS_API(Path*) Path_shift(Path* ptr, float dx, float dy) {
  const auto new_path = ptr->shift(dx, dy);
  new_path->AddRef();
  return new_path.get();
}

UIWIDGETS_API(Path*) Path_transform(Path* ptr, float* matrix4) {
  const auto new_path = ptr->transform(matrix4);
  new_path->AddRef();
  return new_path.get();
}

UIWIDGETS_API(void) Path_getBounds(Path* ptr, float* rect) {
  ptr->getBounds(rect);
}

UIWIDGETS_API(bool)
Path_op(Path* ptr, Path* path1, Path* path2, int operation) {
  return ptr->op(path1, path2, operation);
}

}  // namespace uiwidgets
