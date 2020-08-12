#pragma once

#include <flutter/fml/memory/ref_counted.h>

#include "include/core/SkColorFilter.h"

namespace uiwidgets {

class ColorFilter : public fml::RefCountedThreadSafe<ColorFilter> {
  FML_FRIEND_MAKE_REF_COUNTED(ColorFilter);

 public:
  static fml::RefPtr<ColorFilter> Create();

  static sk_sp<SkColorFilter> MakeColorMatrixFilter255(const float array[20]);

  void initMode(int color, int blend_mode);
  void initMatrix(const float* color_matrix);
  void initSrgbToLinearGamma();
  void initLinearToSrgbGamma();

  ~ColorFilter();

  sk_sp<SkColorFilter> filter() const { return filter_; }

 private:
  sk_sp<SkColorFilter> filter_;
};

}  // namespace uiwidgets
