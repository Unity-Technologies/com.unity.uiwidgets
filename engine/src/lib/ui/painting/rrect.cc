#include "rrect.h"

namespace uiwidgets {

RRect::RRect(float* buffer) {
  is_null = true;

  if (buffer == nullptr) return;

  SkVector radii[4] = {{buffer[4], buffer[5]},
                       {buffer[6], buffer[7]},
                       {buffer[8], buffer[9]},
                       {buffer[10], buffer[11]}};

  sk_rrect.setRectRadii(
      SkRect::MakeLTRB(buffer[0], buffer[1], buffer[2], buffer[3]), radii);

  is_null = false;
}

}  // namespace uiwidgets