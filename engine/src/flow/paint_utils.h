#pragma once

#include "include/core/SkCanvas.h"
#include "include/core/SkColor.h"
#include "include/core/SkRect.h"

namespace uiwidgets {

void DrawCheckerboard(SkCanvas* canvas, SkColor c1, SkColor c2, int size);

void DrawCheckerboard(SkCanvas* canvas, const SkRect& rect);

}  // namespace uiwidgets
