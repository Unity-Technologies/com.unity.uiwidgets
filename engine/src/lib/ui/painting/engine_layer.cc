#include "engine_layer.h"

#include "runtime/mono_api.h"

namespace uiwidgets {

EngineLayer::EngineLayer(std::shared_ptr<ContainerLayer> layer)
    : layer_(layer) {}

EngineLayer::~EngineLayer() = default;

size_t EngineLayer::GetAllocationSize() { return 3000; }

UIWIDGETS_API(void) EngineLayer_dispose(EngineLayer* ptr) { ptr->Release(); }

}  // namespace uiwidgets
