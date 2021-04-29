#include "color_filter.h"
#include "runtime/mono_api.h"

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

UIWIDGETS_API(ColorFilter*)
ColorFilter_constructor() {
  const auto colorFilter = ColorFilter::Create();
  colorFilter->AddRef();
  return colorFilter.get();
}

UIWIDGETS_API(void)
ColorFilter_dispose(ColorFilter* ptr) {
  ptr->Release();
}

UIWIDGETS_API(void)
ColorFilter_initMode(ColorFilter* ptr, int color, int blend_mode) {
  ptr->initMode(color, blend_mode);
}

UIWIDGETS_API(void)
ColorFilter_initMatrix(ColorFilter* ptr, const float* color_matrix) {
  ptr->initMatrix(color_matrix);
}

UIWIDGETS_API(void)
ColorFilter_initLinearToSrgbGamma(ColorFilter* ptr) {
  ptr->initLinearToSrgbGamma();
}

UIWIDGETS_API(void)
ColorFilter_initSrgbToLinearGamma(ColorFilter* ptr) {
  ptr->initSrgbToLinearGamma();
}

}  // namespace uiwidgets