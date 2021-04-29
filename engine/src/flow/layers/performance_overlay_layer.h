#pragma once

#include <string>

#include "flow/instrumentation.h"
#include "flow/layers/layer.h"
#include "flutter/fml/macros.h"

class SkTextBlob;

namespace uiwidgets {

const int kDisplayRasterizerStatistics = 1 << 0;
const int kVisualizeRasterizerStatistics = 1 << 1;
const int kDisplayEngineStatistics = 1 << 2;
const int kVisualizeEngineStatistics = 1 << 3;

class PerformanceOverlayLayer : public Layer {
 public:
  static sk_sp<SkTextBlob> MakeStatisticsText(const Stopwatch& stopwatch,
                                              const std::string& label_prefix,
                                              const std::string& font_path);

  explicit PerformanceOverlayLayer(uint64_t options,
                                   const char* font_path = nullptr);

  void Paint(PaintContext& context) const override;

 private:
  int options_;
  std::string font_path_;

  FML_DISALLOW_COPY_AND_ASSIGN(PerformanceOverlayLayer);
};

}  // namespace uiwidgets
