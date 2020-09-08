#pragma once

#include <memory>
#include <string>

#include "assets/asset_manager.h"
#include "assets/asset_resolver.h"
#include "common/settings.h"
#include "flutter/fml/macros.h"

namespace uiwidgets {

class RunConfiguration {
 public:
  static RunConfiguration InferFromSettings(const Settings& settings);

  RunConfiguration();

  RunConfiguration(std::shared_ptr<AssetManager> asset_manager, fml::closure mono_entrypoint_callback);

  RunConfiguration(RunConfiguration&& config);

  ~RunConfiguration();

  bool IsValid() const;

  bool AddAssetResolver(std::unique_ptr<AssetResolver> resolver);

  std::shared_ptr<AssetManager> GetAssetManager() const;

	void SetMonoEntrypointCallback(fml::closure mono_entrypoint_callback);
	
  fml::closure GetMonoEntrypointCallback() const;

 private:
  std::shared_ptr<AssetManager> asset_manager_;
  fml::closure mono_entrypoint_callback_;

  FML_DISALLOW_COPY_AND_ASSIGN(RunConfiguration);
};

}  // namespace uiwidgets
