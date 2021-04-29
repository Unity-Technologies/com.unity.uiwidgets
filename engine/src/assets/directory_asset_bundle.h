#pragma once

#include "asset_resolver.h"
#include "flutter/fml/macros.h"
#include "flutter/fml/memory/ref_counted.h"
#include "flutter/fml/unique_fd.h"

namespace uiwidgets {

class DirectoryAssetBundle : public AssetResolver {
 public:
  explicit DirectoryAssetBundle(fml::UniqueFD descriptor);

  ~DirectoryAssetBundle() override;

 private:
  const fml::UniqueFD descriptor_;
  bool is_valid_ = false;

  // |AssetResolver|
  bool IsValid() const override;

  // |AssetResolver|
  std::unique_ptr<fml::Mapping> GetAsMapping(
      const std::string& asset_name) const override;

  FML_DISALLOW_COPY_AND_ASSIGN(DirectoryAssetBundle);
};

}  // namespace uiwidgets
