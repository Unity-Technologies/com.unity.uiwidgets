#pragma once

#include <flutter/fml/memory/ref_counted.h>
#include <flutter/fml/memory/ref_ptr.h>

#include "include/core/SkCanvas.h"
#include "lib/ui/painting/picture.h"
#include "lib/ui/painting/picture_recorder.h"

namespace uiwidgets {
class PictureRecorder;
class CanvasImage;

class Canvas : public fml::RefCountedThreadSafe<Canvas> {
  FML_FRIEND_MAKE_REF_COUNTED(Canvas);

 public:
  static fml::RefPtr<Canvas> Create(PictureRecorder* recorder, double left,
                                    double top, double right, double bottom);

  ~Canvas();

 private:
  explicit Canvas(SkCanvas* canvas);

  SkCanvas* canvas_;
};

}  // namespace uiwidgets
