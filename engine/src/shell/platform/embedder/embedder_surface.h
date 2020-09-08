#pragma once

#include "flow/embedded_views.h"
#include "flutter/fml/macros.h"
#include "shell/common/surface.h"

namespace uiwidgets {

class EmbedderSurface {
 public:
  EmbedderSurface();

  virtual ~EmbedderSurface();

  virtual bool IsValid() const = 0;

  virtual std::unique_ptr<Surface> CreateGPUSurface() = 0;

  virtual sk_sp<GrContext> CreateResourceContext() const = 0;

 private:
  FML_DISALLOW_COPY_AND_ASSIGN(EmbedderSurface);
};

}  // namespace uiwidgets
