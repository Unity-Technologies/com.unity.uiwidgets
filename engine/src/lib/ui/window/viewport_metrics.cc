#include "viewport_metrics.h"

#include "flutter/fml/logging.h"

namespace uiwidgets {

ViewportMetrics::ViewportMetrics(
    float p_device_pixel_ratio, float p_physical_width, float p_physical_height,
    float p_physical_padding_top, float p_physical_padding_right,
    float p_physical_padding_bottom, float p_physical_padding_left,
    float p_physical_view_inset_top, float p_physical_view_inset_right,
    float p_physical_view_inset_bottom, float p_physical_view_inset_left,
    float p_physical_system_gesture_inset_top,
    float p_physical_system_gesture_inset_right,
    float p_physical_system_gesture_inset_bottom,
    float p_physical_system_gesture_inset_left)
    : device_pixel_ratio(p_device_pixel_ratio),
      physical_width(p_physical_width),
      physical_height(p_physical_height),
      physical_padding_top(p_physical_padding_top),
      physical_padding_right(p_physical_padding_right),
      physical_padding_bottom(p_physical_padding_bottom),
      physical_padding_left(p_physical_padding_left),
      physical_view_inset_top(p_physical_view_inset_top),
      physical_view_inset_right(p_physical_view_inset_right),
      physical_view_inset_bottom(p_physical_view_inset_bottom),
      physical_view_inset_left(p_physical_view_inset_left),
      physical_system_gesture_inset_top(p_physical_system_gesture_inset_top),
      physical_system_gesture_inset_right(
          p_physical_system_gesture_inset_right),
      physical_system_gesture_inset_bottom(
          p_physical_system_gesture_inset_bottom),
      physical_system_gesture_inset_left(p_physical_system_gesture_inset_left) {
  // Ensure we don't have nonsensical dimensions.
  FML_DCHECK(physical_width >= 0);
  FML_DCHECK(physical_height >= 0);
  FML_DCHECK(device_pixel_ratio > 0);
}

ViewportMetrics::ViewportMetrics(
    float p_device_pixel_ratio, float p_physical_width, float p_physical_height,
    float p_physical_depth, float p_physical_padding_top,
    float p_physical_padding_right, float p_physical_padding_bottom,
    float p_physical_padding_left, float p_physical_view_inset_front,
    float p_physical_view_inset_back, float p_physical_view_inset_top,
    float p_physical_view_inset_right, float p_physical_view_inset_bottom,
    float p_physical_view_inset_left)
    : device_pixel_ratio(p_device_pixel_ratio),
      physical_width(p_physical_width),
      physical_height(p_physical_height),
      physical_depth(p_physical_depth),
      physical_padding_top(p_physical_padding_top),
      physical_padding_right(p_physical_padding_right),
      physical_padding_bottom(p_physical_padding_bottom),
      physical_padding_left(p_physical_padding_left),
      physical_view_inset_top(p_physical_view_inset_top),
      physical_view_inset_right(p_physical_view_inset_right),
      physical_view_inset_bottom(p_physical_view_inset_bottom),
      physical_view_inset_left(p_physical_view_inset_left),
      physical_view_inset_front(p_physical_view_inset_front),
      physical_view_inset_back(p_physical_view_inset_back) {
  // Ensure we don't have nonsensical dimensions.
  FML_DCHECK(physical_width >= 0);
  FML_DCHECK(physical_height >= 0);
  FML_DCHECK(device_pixel_ratio > 0);
}

}  // namespace uiwidgets
