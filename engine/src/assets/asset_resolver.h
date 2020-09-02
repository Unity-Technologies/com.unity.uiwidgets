#pragma once

#include <string>
#include <vector>

#include "flutter/fml/macros.h"
#include "flutter/fml/mapping.h"

namespace uiwidgets {

class AssetResolver {
 public:
  AssetResolver() = default;

  virtual ~AssetResolver() = default;

  virtual bool IsValid() const = 0;

  [[nodiscard]] virtual std::unique_ptr<fml::Mapping> GetAsMapping(
      const std::string& asset_name) const = 0;

 private:
  FML_DISALLOW_COPY_AND_ASSIGN(AssetResolver);
};

}  // namespace uiwidgets
