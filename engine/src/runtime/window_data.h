#pragma once

#include <memory>
#include <string>
#include <vector>

#include "lib/ui/window/viewport_metrics.h"

namespace uiwidgets {

struct WindowData {
  WindowData();

  ~WindowData();

  ViewportMetrics viewport_metrics;
  std::string language_code;
  std::string country_code;
  std::string script_code;
  std::string variant_code;
  std::vector<std::string> locale_data;
  std::string user_settings_data = "{}";
  std::string lifecycle_state;
  int32_t accessibility_feature_flags_ = 0;
};

}  // namespace uiwidgets
