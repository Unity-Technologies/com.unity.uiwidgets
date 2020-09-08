#pragma once

#include <deque>
#include <memory>
#include <string>

#include "asset_resolver.h"
#include "flutter/fml/macros.h"

namespace uiwidgets {

class AssetManager final : public AssetResolver {
 public:
  AssetManager();

  ~AssetManager() override;

  void PushFront(std::unique_ptr<AssetResolver> resolver);

  void PushBack(std::unique_ptr<AssetResolver> resolver);

  // |AssetResolver|
  bool IsValid() const override;

  // |AssetResolver|
  std::unique_ptr<fml::Mapping> GetAsMapping(
      const std::string& asset_name) const override;

 private:
  std::deque<std::unique_ptr<AssetResolver>> resolvers_;

  FML_DISALLOW_COPY_AND_ASSIGN(AssetManager);
};

}  // namespace uiwidgets
