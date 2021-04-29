#pragma once

#include "include/effects/SkGradientShader.h"
#include "lib/ui/painting/matrix.h"
#include "lib/ui/painting/shader.h"

namespace uiwidgets {

// TODO: update this if/when Skia adds Decal mode skbug.com/7638
static_assert(kSkTileModeCount >= 3, "Need to update tile mode enum");

class CanvasGradient : public Shader {
  FML_FRIEND_MAKE_REF_COUNTED(CanvasGradient);

 public:
  ~CanvasGradient() override;

  static fml::RefPtr<CanvasGradient> Create();

  void initLinear(const float* end_points, int end_points_length,
                  const int* colors, int colors_length,
                  const float* color_stops, int color_stops_length,
                  SkTileMode tile_mode, const float* matrix4);

  void initRadial(float center_x, float center_y, float radius,
                  const int* colors, int colors_length,
                  const float* color_stops, int color_stops_length,
                  SkTileMode tile_mode, const float* matrix4);

  void initSweep(float center_x, float center_y, const int* colors,
                 int colors_length, const float* color_stops,
                 int color_stops_length, SkTileMode tile_mode,
                 float start_angle, float end_angle, const float* matrix4);

  void initTwoPointConical(float start_x, float start_y, float start_radius,
                           float end_x, float end_y, float end_radius,
                           const int* colors, int colors_length,
                           const float* color_stops, int color_stops_length,
                           SkTileMode tile_mode, const float* matrix4);

 private:
  CanvasGradient();
};

}  // namespace uiwidgets
