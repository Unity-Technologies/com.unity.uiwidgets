#pragma once

#include <flutter/fml/memory/ref_counted.h>

#include "include/core/SkPath.h"
#include "include/pathops/SkPathOps.h"
#include "rrect.h"

namespace uiwidgets {

class CanvasPath : public fml::RefCountedThreadSafe<CanvasPath> {
  FML_FRIEND_MAKE_REF_COUNTED(CanvasPath);

 public:
  ~CanvasPath();

  static fml::RefPtr<CanvasPath> CreateNew() {
    return fml::MakeRefCounted<CanvasPath>();
  }

  static fml::RefPtr<CanvasPath> CreateFrom(const SkPath& src) {
    fml::RefPtr<CanvasPath> path = CanvasPath::CreateNew();
    path->path_ = src;
    return path;
  }

  int getFillType();
  void setFillType(int fill_type);

  void moveTo(float x, float y);
  void relativeMoveTo(float x, float y);
  void lineTo(float x, float y);
  void relativeLineTo(float x, float y);
  void quadraticBezierTo(float x1, float y1, float x2, float y2);
  void relativeQuadraticBezierTo(float x1, float y1, float x2, float y2);
  void cubicTo(float x1, float y1, float x2, float y2, float x3, float y3);
  void relativeCubicTo(float x1, float y1, float x2, float y2, float x3,
                       float y3);
  void conicTo(float x1, float y1, float x2, float y2, float w);
  void relativeConicTo(float x1, float y1, float x2, float y2, float w);
  void arcTo(float left, float top, float right, float bottom, float startAngle,
             float sweepAngle, bool forceMoveTo);
  void arcToPoint(float arcEndX, float arcEndY, float radiusX, float radiusY,
                  float xAxisRotation, bool isLargeArc,
                  bool isClockwiseDirection);
  void relativeArcToPoint(float arcEndDeltaX, float arcEndDeltaY, float radiusX,
                          float radiusY, float xAxisRotation, bool isLargeArc,
                          bool isClockwiseDirection);
  void addRect(float left, float top, float right, float bottom);
  void addOval(float left, float top, float right, float bottom);
  void addArc(float left, float top, float right, float bottom,
              float startAngle, float sweepAngle);
  void addPolygon(float* points, int points_length, bool close);
  void addRRect(const RRect& rrect);
  void addPath(CanvasPath* path, float dx, float dy);
  void addPathWithMatrix(CanvasPath* path, float dx, float dy, float* matrix4);
  void extendWithPath(CanvasPath* path, float dx, float dy);
  void extendWithPathAndMatrix(CanvasPath* path, float dx, float dy,
                               float* matrix4);
  void close();
  void reset();
  bool contains(float x, float y);
  fml::RefPtr<CanvasPath> shift(float dx, float dy);
  fml::RefPtr<CanvasPath> transform(float* matrix4);
  void getBounds(float* bounds);
  bool op(CanvasPath* path1, CanvasPath* path2, int operation);
  fml::RefPtr<CanvasPath> clone();

  const SkPath& path() const { return path_; }

 private:
  CanvasPath();

  SkPath path_;
};

}  // namespace uiwidgets
