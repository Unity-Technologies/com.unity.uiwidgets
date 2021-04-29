#define _USE_MATH_DEFINES

#include "gradient.h"

namespace uiwidgets {

typedef CanvasGradient Gradient;

fml::RefPtr<CanvasGradient> CanvasGradient::Create() {
  return fml::MakeRefCounted<CanvasGradient>();
}

void CanvasGradient::initLinear(const float* end_points, int end_points_length,
                                const int* colors, int colors_length,
                                const float* color_stops,
                                int color_stops_length, SkTileMode tile_mode,
                                const float* matrix4) {
  FML_DCHECK(end_points_length == 4);
  FML_DCHECK(colors_length == color_stops_length || color_stops == nullptr);

  static_assert(sizeof(SkPoint) == sizeof(float) * 2,
                "SkPoint doesn't use floats.");
  static_assert(sizeof(SkColor) == sizeof(int32_t),
                "SkColor doesn't use int32_t.");

  SkMatrix sk_matrix;
  bool has_matrix = matrix4 != nullptr;
  if (has_matrix) {
    sk_matrix = ToSkMatrix(matrix4);
  }

  set_shader(UIMonoState::CreateGPUObject(SkGradientShader::MakeLinear(
      reinterpret_cast<const SkPoint*>(end_points),
      reinterpret_cast<const SkColor*>(colors), color_stops, colors_length,
      tile_mode, 0, has_matrix ? &sk_matrix : nullptr)));
}

void CanvasGradient::initRadial(float center_x, float center_y, float radius,
                                const int* colors, int colors_length,
                                const float* color_stops,
                                int color_stops_length, SkTileMode tile_mode,
                                const float* matrix4) {
  FML_DCHECK(colors_length == color_stops_length || color_stops == nullptr);

  static_assert(sizeof(SkColor) == sizeof(int32_t),
                "SkColor doesn't use int32_t.");

  SkMatrix sk_matrix;
  bool has_matrix = matrix4 != nullptr;
  if (has_matrix) {
    sk_matrix = ToSkMatrix(matrix4);
  }

  set_shader(UIMonoState::CreateGPUObject(SkGradientShader::MakeRadial(
      SkPoint::Make(center_x, center_y), radius,
      reinterpret_cast<const SkColor*>(colors), color_stops, colors_length,
      tile_mode, 0, has_matrix ? &sk_matrix : nullptr)));
}

void CanvasGradient::initSweep(float center_x, float center_y,
                               const int* colors, int colors_length,
                               const float* color_stops, int color_stops_length,
                               SkTileMode tile_mode, float start_angle,
                               float end_angle, const float* matrix4) {
  FML_DCHECK(colors_length == color_stops_length || color_stops == nullptr);

  static_assert(sizeof(SkColor) == sizeof(int32_t),
                "SkColor doesn't use int32_t.");

  SkMatrix sk_matrix;
  bool has_matrix = matrix4 != nullptr;
  if (has_matrix) {
    sk_matrix = ToSkMatrix(matrix4);
  }

  set_shader(UIMonoState::CreateGPUObject(SkGradientShader::MakeSweep(
      center_x, center_y, reinterpret_cast<const SkColor*>(colors), color_stops,
      colors_length, tile_mode, start_angle * 180.0 / M_PI,
      end_angle * 180.0 / M_PI, 0, has_matrix ? &sk_matrix : nullptr)));
}

void CanvasGradient::initTwoPointConical(
    float start_x, float start_y, float start_radius, float end_x, float end_y,
    float end_radius, const int* colors, int colors_length,
    const float* color_stops, int color_stops_length, SkTileMode tile_mode,
    const float* matrix4) {
  FML_DCHECK(colors_length == color_stops_length || color_stops == nullptr);

  static_assert(sizeof(SkColor) == sizeof(int32_t),
                "SkColor doesn't use int32_t.");

  SkMatrix sk_matrix;
  bool has_matrix = matrix4 != nullptr;
  if (has_matrix) {
    sk_matrix = ToSkMatrix(matrix4);
  }

  set_shader(UIMonoState::CreateGPUObject(SkGradientShader::MakeTwoPointConical(
      SkPoint::Make(start_x, start_y), start_radius,
      SkPoint::Make(end_x, end_y), end_radius,
      reinterpret_cast<const SkColor*>(colors), color_stops, colors_length,
      tile_mode, 0, has_matrix ? &sk_matrix : nullptr)));
}

CanvasGradient::CanvasGradient() = default;

CanvasGradient::~CanvasGradient() = default;

UIWIDGETS_API(CanvasGradient*)
Gradient_constructor() {
  const auto gradiant = CanvasGradient::Create();
  gradiant->AddRef();
  return gradiant.get();
}

UIWIDGETS_API(void) Gradient_dispose(CanvasGradient* ptr) { ptr->Release(); }

UIWIDGETS_API(void)
Gradient_initLinear(CanvasGradient* ptr, const float* end_points,
                    int end_points_length, const int* colors, int colors_length,
                    const float* color_stops, int color_stops_length,
                    SkTileMode tile_mode, const float* matrix4) {
  ptr->initLinear(end_points, end_points_length, colors, colors_length,
                  color_stops, color_stops_length, tile_mode, matrix4);
}

UIWIDGETS_API(void)
Gradient_initRadial(CanvasGradient* ptr, float center_x, float center_y,
                    float radius, const int* colors, int colors_length,
                    const float* color_stops, int color_stops_length,
                    SkTileMode tile_mode, const float* matrix4) {
  ptr->initRadial(center_x, center_y, radius, colors, colors_length,
                  color_stops, color_stops_length, tile_mode, matrix4);
}

UIWIDGETS_API(void)
Gradient_initConical(CanvasGradient* ptr, float start_x, float start_y,
                     float start_radius, float end_x, float end_y,
                     float end_radius, const int* colors, int colors_length,
                     const float* color_stops, int color_stops_length,
                     SkTileMode tile_mode, const float* matrix4) {
  ptr->initTwoPointConical(start_x, start_y, start_radius, end_x, end_y,
                           end_radius, colors, colors_length, color_stops,
                           color_stops_length, tile_mode, matrix4);
}

UIWIDGETS_API(void)
Gradient_initSweep(CanvasGradient* ptr, float center_x, float center_y,
                   const int* colors, int colors_length,
                   const float* color_stops, int color_stops_length,
                   SkTileMode tile_mode, float start_angle, float end_angle,
                   const float* matrix4) {
  ptr->initSweep(center_x, center_y, colors, colors_length, color_stops,
                 color_stops_length, tile_mode, start_angle, end_angle,
                 matrix4);
}

}  // namespace uiwidgets
