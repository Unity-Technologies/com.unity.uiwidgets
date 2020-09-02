#include "asset_manager.h"

#include "flutter/fml/trace_event.h"

namespace uiwidgets {

AssetManager::AssetManager() = default;

AssetManager::~AssetManager() = default;

void AssetManager::PushFront(std::unique_ptr<AssetResolver> resolver) {
  if (resolver == nullptr || !resolver->IsValid()) {
    return;
  }

  resolvers_.push_front(std::move(resolver));
}

void AssetManager::PushBack(std::unique_ptr<AssetResolver> resolver) {
  if (resolver == nullptr || !resolver->IsValid()) {
    return;
  }

  resolvers_.push_back(std::move(resolver));
}

// |AssetResolver|
std::unique_ptr<fml::Mapping> AssetManager::GetAsMapping(
    const std::string& asset_name) const {
  if (asset_name.empty()) {
    return std::unique_ptr<fml::Mapping>(nullptr);
  }
  TRACE_EVENT1("uiwidgets", "AssetManager::GetAsMapping", "name",
               asset_name.c_str());
  for (const auto& resolver : resolvers_) {
    auto mapping = resolver->GetAsMapping(asset_name);
    if (mapping != nullptr) {
      return mapping;
    }
  }
  FML_DLOG(WARNING) << "Could not find asset: " << asset_name;
  return std::unique_ptr<fml::Mapping>(nullptr);
}

// |AssetResolver|
bool AssetManager::IsValid() const { return !resolvers_.empty(); }

}  // namespace uiwidgets
