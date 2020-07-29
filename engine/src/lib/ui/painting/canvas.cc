#include "canvas.h"

#define _USE_MATH_DEFINES
#include <math.h>

#include "flow/layers/physical_shape_layer.h"
#include "include/core/SkBitmap.h"
#include "include/core/SkCanvas.h"
#include "include/core/SkRSXform.h"
#include "lib/ui/painting/image.h"
#include "lib/ui/window/window.h"
#include "picture_recorder.h"

namespace uiwidgets {

fml::RefPtr<Canvas> Canvas::Create(PictureRecorder* recorder, double left,
                                   double top, double right, double bottom) {
  if (!recorder)
    Mono_ThrowException(
        "Canvas constructor called with non-genuine PictureRecorder.");

  FML_DCHECK(!recorder->isRecording());  // verified by Dart code
  fml::RefPtr<Canvas> canvas = fml::MakeRefCounted<Canvas>(
      recorder->BeginRecording(SkRect::MakeLTRB(left, top, right, bottom)));
  recorder->set_canvas(canvas);
  return canvas;
}

Canvas::Canvas(SkCanvas* canvas) : canvas_(canvas) {}

Canvas::~Canvas() {}

}  // namespace uiwidgets
