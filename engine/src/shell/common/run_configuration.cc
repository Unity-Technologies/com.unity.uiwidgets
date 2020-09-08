#include "run_configuration.h"

#include <sstream>
#include <utility>

#include "assets/directory_asset_bundle.h"
#include "flutter/fml/file.h"
#include "flutter/fml/unique_fd.h"

namespace uiwidgets {

RunConfiguration RunConfiguration::InferFromSettings(const Settings& settings) {
  auto asset_manager = std::make_shared<AssetManager>();

  if (fml::UniqueFD::traits_type::IsValid(settings.assets_dir)) {
    asset_manager->PushBack(std::make_unique<DirectoryAssetBundle>(
        fml::Duplicate(settings.assets_dir)));
  }

  asset_manager->PushBack(
      std::make_unique<DirectoryAssetBundle>(fml::OpenDirectory(
          settings.assets_path.c_str(), false, fml::FilePermission::kRead)));

  return {asset_manager, settings.mono_entrypoint_callback};
}

RunConfiguration::RunConfiguration()
    : RunConfiguration(std::make_shared<AssetManager>(), fml::closure()) {}

RunConfiguration::RunConfiguration(std::shared_ptr<AssetManager> asset_manager,
                                   fml::closure mono_entrypoint_callback)
    : asset_manager_(std::move(asset_manager)),
      mono_entrypoint_callback_(std::move(mono_entrypoint_callback)) {}

RunConfiguration::RunConfiguration(RunConfiguration&&) = default;

RunConfiguration::~RunConfiguration() = default;

bool RunConfiguration::IsValid() const {
  return static_cast<bool>(asset_manager_);
}

bool RunConfiguration::AddAssetResolver(
    std::unique_ptr<AssetResolver> resolver) {
  if (!resolver || !resolver->IsValid()) {
    return false;
  }

  asset_manager_->PushBack(std::move(resolver));
  return true;
}

std::shared_ptr<AssetManager> RunConfiguration::GetAssetManager() const {
  return asset_manager_;
}

void RunConfiguration::SetMonoEntrypointCallback(fml::closure mono_entrypoint_callback) {
  mono_entrypoint_callback_ = std::move(mono_entrypoint_callback);
}

fml::closure RunConfiguration::GetMonoEntrypointCallback() const {
  return mono_entrypoint_callback_;
}
}  // namespace uiwidgets
