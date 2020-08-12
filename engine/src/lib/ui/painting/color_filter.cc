#include "color_filter.h"

namespace uiwidgets {

fml::RefPtr<ColorFilter> ColorFilter::Create() {
  return fml::MakeRefCounted<ColorFilter>();
}

void ColorFilter::initMode(int color, int blend_mode) {
  filter_ = SkColorFilters::Blend(static_cast<SkColor>(color),
                                  static_cast<SkBlendMode>(blend_mode));
}

sk_sp<SkColorFilter> ColorFilter::MakeColorMatrixFilter255(
    const float array[20]) {
  float tmp[20];
  memcpy(tmp, array, sizeof(tmp));
  tmp[4] *= 1.0f / 255;
  tmp[9] *= 1.0f / 255;
  tmp[14] *= 1.0f / 255;
  tmp[19] *= 1.0f / 255;
  return SkColorFilters::Matrix(tmp);
}

void ColorFilter::initMatrix(const float* color_matrix) {
  filter_ = MakeColorMatrixFilter255(color_matrix);
}

void ColorFilter::initLinearToSrgbGamma() {
  filter_ = SkColorFilters::LinearToSRGBGamma();
}

void ColorFilter::initSrgbToLinearGamma() {
  filter_ = SkColorFilters::SRGBToLinearGamma();
}

ColorFilter::~ColorFilter() = default;

}  // namespace uiwidgets
