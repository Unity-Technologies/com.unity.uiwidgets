#include "engine_layer.h"

namespace uiwidgets {

EngineLayer::EngineLayer(std::shared_ptr<ContainerLayer> layer)
    : layer_(layer) {}

EngineLayer::~EngineLayer() = default;

size_t EngineLayer::GetAllocationSize() { return 3000; };

}  // namespace uiwidgets
