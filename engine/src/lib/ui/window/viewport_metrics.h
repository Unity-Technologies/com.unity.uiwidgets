#pragma once

namespace uiwidgets {

static const float kUnsetDepth = 3.402823E+38f;

struct ViewportMetrics {
  ViewportMetrics() = default;
  ViewportMetrics(const ViewportMetrics& other) = default;

  // Create a 2D ViewportMetrics instance.
  ViewportMetrics(float p_device_pixel_ratio, float p_physical_width,
                  float p_physical_height, float p_physical_padding_top,
                  float p_physical_padding_right,
                  float p_physical_padding_bottom,
                  float p_physical_padding_left,
                  float p_physical_view_inset_top,
                  float p_physical_view_inset_right,
                  float p_physical_view_inset_bottom,
                  float p_physical_view_inset_left,
                  float p_physical_system_gesture_inset_top,
                  float p_physical_system_gesture_inset_right,
                  float p_physical_system_gesture_inset_bottom,
                  float p_physical_system_gesture_inset_left);

  // Create a ViewportMetrics instance that contains z information.
  ViewportMetrics(
      float p_device_pixel_ratio, float p_physical_width,
      float p_physical_height, float p_physical_depth,
      float p_physical_padding_top, float p_physical_padding_right,
      float p_physical_padding_bottom, float p_physical_padding_left,
      float p_physical_view_inset_front, float p_physical_view_inset_back,
      float p_physical_view_inset_top, float p_physical_view_inset_right,
      float p_physical_view_inset_bottom, float p_physical_view_inset_left);

  float device_pixel_ratio = 1.0f;
  float physical_width = 0;
  float physical_height = 0;
  float physical_depth = kUnsetDepth;
  float physical_padding_top = 0;
  float physical_padding_right = 0;
  float physical_padding_bottom = 0;
  float physical_padding_left = 0;
  float physical_view_inset_top = 0;
  float physical_view_inset_right = 0;
  float physical_view_inset_bottom = 0;
  float physical_view_inset_left = 0;
  float physical_view_inset_front = kUnsetDepth;
  float physical_view_inset_back = kUnsetDepth;
  float physical_system_gesture_inset_top = 0;
  float physical_system_gesture_inset_right = 0;
  float physical_system_gesture_inset_bottom = 0;
  float physical_system_gesture_inset_left = 0;
};

struct LogicalSize {
  float width = 0.0f;
  float height = 0.0f;
  float depth = kUnsetDepth;
};

struct LogicalInset {
  float left = 0.0f;
  float top = 0.0f;
  float right = 0.0f;
  float bottom = 0.0f;
  float front = kUnsetDepth;
  float back = kUnsetDepth;
};

struct LogicalMetrics {
  LogicalSize size;
  float scale = 1.0f;
  float scale_z = 1.0f;
  LogicalInset padding;
  LogicalInset view_inset;
};

}  // namespace uiwidgets
