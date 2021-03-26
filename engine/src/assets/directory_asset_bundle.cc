#include "directory_asset_bundle.h"

#include <utility>

#include "flutter/fml/file.h"
#include "flutter/fml/mapping.h"
#if __ANDROID__
#include "shell/platform/unity/android_unpack_streaming_asset.h"
#endif
namespace uiwidgets {

DirectoryAssetBundle::DirectoryAssetBundle(fml::UniqueFD descriptor)
    : descriptor_(std::move(descriptor)) {
  if (!fml::IsDirectory(descriptor_)) {
    return;
  }
  is_valid_ = true;
}

DirectoryAssetBundle::~DirectoryAssetBundle() = default;

// |AssetResolver|
bool DirectoryAssetBundle::IsValid() const { return is_valid_; }

// |AssetResolver|
std::unique_ptr<fml::Mapping> DirectoryAssetBundle::GetAsMapping(
    const std::string &asset_name) const
{
  if (!is_valid_)
  {
    FML_DLOG(WARNING) << "Asset bundle was not valid.";
    return std::unique_ptr<fml::Mapping>(nullptr);
  }

#if __ANDROID__
  AndroidUnpackStreamingAsset::Unpack(asset_name.c_str());
#endif

  auto mapping = std::make_unique<fml::FileMapping>(fml::OpenFile(
      descriptor_, asset_name.c_str(), false, fml::FilePermission::kRead));

  if (!mapping->IsValid()) {
    return std::unique_ptr<fml::Mapping>(nullptr);
  }

  return mapping;
}

} // namespace uiwidgets
