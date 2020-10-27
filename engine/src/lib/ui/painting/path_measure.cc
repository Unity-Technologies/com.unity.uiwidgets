#include "path_measure.h"

#include "runtime/mono_state.h"

namespace uiwidgets {
typedef CanvasPathMeasure PathMeasure;

CanvasPathMeasure::CanvasPathMeasure() {}

CanvasPathMeasure::~CanvasPathMeasure() {}

fml::RefPtr<CanvasPathMeasure> CanvasPathMeasure::Create(const CanvasPath* path,
                                                         bool forceClosed) {
  fml::RefPtr<CanvasPathMeasure> pathMeasure =
      fml::MakeRefCounted<CanvasPathMeasure>();
  if (path) {
    const SkPath skPath = path->path();
    SkScalar resScale = 1;
    pathMeasure->path_measure_ =
        std::make_unique<SkContourMeasureIter>(skPath, forceClosed, resScale);
  } else {
    pathMeasure->path_measure_ = std::make_unique<SkContourMeasureIter>();
  }

  return pathMeasure;
}

void CanvasPathMeasure::setPath(const CanvasPath* path, bool isClosed) {
  const SkPath& skPath = path->path();
  path_measure_->reset(skPath, isClosed);
}

float CanvasPathMeasure::getLength(int contour_index) {
  if (static_cast<std::vector<sk_sp<SkContourMeasure>>::size_type>(
          contour_index) < measures_.size()) {
    return measures_[contour_index]->length();
  }
  return -1;
}

void CanvasPathMeasure::getPosTan(int contour_index, float distance, float* posTan) {
  posTan[0] = 0;
  if (static_cast<std::vector<sk_sp<SkContourMeasure>>::size_type>(
          contour_index) >= measures_.size()) {
    return;
  }

  SkPoint pos;
  SkVector tan;
  bool success = measures_[contour_index]->getPosTan(distance, &pos, &tan);

  if (success) {
    posTan[0] = 1;  // dart code will check for this for success
    posTan[1] = pos.x();
    posTan[2] = pos.y();
    posTan[3] = tan.x();
    posTan[4] = tan.y();
  }
}

fml::RefPtr<CanvasPath> CanvasPathMeasure::getSegment(int contour_index,
                                   float start_d, float stop_d,
                                   bool start_with_move_to) {
  if (static_cast<std::vector<sk_sp<SkContourMeasure>>::size_type>(
          contour_index) >= measures_.size()) {
		return CanvasPath::CreateNew();
  }
  SkPath dst;
  bool success = measures_[contour_index]->getSegment(start_d, stop_d, &dst,
                                                      start_with_move_to);
  if (!success) {
    return CanvasPath::CreateNew();
  } else {
    return CanvasPath::CreateFrom(dst);
  }
}

bool CanvasPathMeasure::isClosed(int contour_index) {
  if (static_cast<std::vector<sk_sp<SkContourMeasure>>::size_type>(
          contour_index) < measures_.size()) {
    return measures_[contour_index]->isClosed();
  }
  return false;
}

bool CanvasPathMeasure::nextContour() {
  auto measure = path_measure_->next();
  if (measure) {
    measures_.push_back(std::move(measure));
    return true;
  }
  return false;
}

UIWIDGETS_API(PathMeasure*)
PathMeasure_constructor(CanvasPath* path, bool forceClosed) {
  const auto pathMeasure = PathMeasure::Create(path, forceClosed);
  pathMeasure->AddRef();
  return pathMeasure.get();
}

UIWIDGETS_API(void) PathMeasure_dispose(PathMeasure* ptr) { ptr->Release(); }

UIWIDGETS_API(float) PathMeasure_length(PathMeasure* ptr, int contourIndex) {
  return ptr->getLength(contourIndex);
}

UIWIDGETS_API(void)
PathMeasure_getPosTan(PathMeasure* ptr, int contour_index, float distance,
                      float* posTan) {
  ptr->getPosTan(contour_index, distance, posTan);
}

UIWIDGETS_API(CanvasPath*)
PathMeasure_getSegment(PathMeasure* ptr, int contour_index, float start_d,
	float stop_d,
	bool start_with_move_to) {
  const auto path = ptr->getSegment(contour_index, start_d, stop_d, start_with_move_to);
  path->AddRef();
  return path.get();
}

UIWIDGETS_API(bool) PathMeasure_isClosed(PathMeasure* ptr, int contourIndex) {
  return ptr->isClosed(contourIndex);
}

UIWIDGETS_API(bool) PathMeasure_nativeNextContour(PathMeasure* ptr) {
  return ptr->nextContour();
}

}  // namespace uiwidgets