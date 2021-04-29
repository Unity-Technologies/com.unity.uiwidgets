#pragma once

#include "flow/embedded_views.h"

namespace uiwidgets {

class GPUSurfaceDelegate {
 public:
  virtual ~GPUSurfaceDelegate() {}

  virtual ExternalViewEmbedder* GetExternalViewEmbedder() = 0;
};

}  // namespace uiwidgets
