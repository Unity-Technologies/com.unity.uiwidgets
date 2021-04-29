#pragma once

#include "include/core/SkRRect.h"

namespace uiwidgets {

class RRect {
 public:
  explicit RRect(float* data);

  SkRRect sk_rrect;
  bool is_null;
};
}  // namespace uiwidgets
