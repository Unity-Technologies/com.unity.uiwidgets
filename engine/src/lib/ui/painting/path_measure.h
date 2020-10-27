#pragma once

#include <flutter/fml/memory/ref_counted.h>
#include <vector>

#include "include/core/SkContourMeasure.h"
#include "path.h"

namespace uiwidgets {

class CanvasPathMeasure : public fml::RefCountedThreadSafe<CanvasPathMeasure> {
  FML_FRIEND_MAKE_REF_COUNTED(CanvasPathMeasure);

 public:
  ~CanvasPathMeasure();

  static fml::RefPtr<CanvasPathMeasure> Create(const CanvasPath* path,
                                               bool forcedClosed);

  void setPath(const CanvasPath* path, bool isClosed);
  float getLength(int contour_index);
  bool isClosed(int contour_index);
  bool nextContour();

 private:
  CanvasPathMeasure();

  std::unique_ptr<SkContourMeasureIter> path_measure_;
  std::vector<sk_sp<SkContourMeasure>> measures_;
};

}  // namespace uiwidgets
