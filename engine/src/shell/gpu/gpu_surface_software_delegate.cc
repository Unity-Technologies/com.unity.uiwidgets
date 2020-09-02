#include "gpu_surface_software_delegate.h"

namespace uiwidgets {

GPUSurfaceSoftwareDelegate::~GPUSurfaceSoftwareDelegate() = default;

ExternalViewEmbedder* GPUSurfaceSoftwareDelegate::GetExternalViewEmbedder() {
  return nullptr;
}

}  // namespace uiwidgets
