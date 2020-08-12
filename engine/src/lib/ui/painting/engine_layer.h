#pragma once

#include "flow/layers/container_layer.h"

namespace uiwidgets {

class EngineLayer : public fml::RefCountedThreadSafe<EngineLayer> {
 public:
  ~EngineLayer();

  size_t GetAllocationSize();

  static fml::RefPtr<EngineLayer> MakeRetained(
      std::shared_ptr<ContainerLayer> layer) {
    return fml::MakeRefCounted<EngineLayer>(layer);
  }

  std::shared_ptr<ContainerLayer> Layer() const { return layer_; }

 private:
  explicit EngineLayer(std::shared_ptr<ContainerLayer> layer);
  std::shared_ptr<ContainerLayer> layer_;

  FML_FRIEND_MAKE_REF_COUNTED(EngineLayer);
};

}  // namespace uiwidgets
