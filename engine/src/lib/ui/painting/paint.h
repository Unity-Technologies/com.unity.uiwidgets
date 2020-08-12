#pragma once

#include "include/core/SkPaint.h"
#include "runtime/mono_api.h"

namespace uiwidgets {

class Paint {
 public:
  Paint() = default;
  Paint(void** paint_objects, uint8_t* paint_data);

  const SkPaint* paint() const { return is_null_ ? nullptr : &paint_; }

 private:
  SkPaint paint_;
  bool is_null_ = true;
};

}  // namespace uiwidgets
