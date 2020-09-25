#pragma once

#include <memory>
#include <string>
#include <unordered_map>
#include <vector>

#include "assets/asset_manager.h"
#include "flutter/fml/macros.h"
#include "third_party/skia/include/core/SkFontMgr.h"
#include "third_party/skia/include/core/SkTypeface.h"
#include "txt/font_asset_provider.h"

namespace uiwidgets {

class AssetManagerFontStyleSet : public SkFontStyleSet {
 public:
  AssetManagerFontStyleSet(std::shared_ptr<AssetManager> asset_manager,
                           std::string family_name);

  ~AssetManagerFontStyleSet() override;

  void registerAsset(std::string asset);

  // |SkFontStyleSet|
  int count() override;

  // |SkFontStyleSet|
  void getStyle(int index, SkFontStyle*, SkString* style) override;

  // |SkFontStyleSet|
  SkTypeface* createTypeface(int index) override;

  // |SkFontStyleSet|
  SkTypeface* matchStyle(const SkFontStyle& pattern) override;

 private:
  std::shared_ptr<AssetManager> asset_manager_;
  std::string family_name_;

  struct TypefaceAsset {
    TypefaceAsset(std::string a);

    TypefaceAsset(const TypefaceAsset& other);

    ~TypefaceAsset();

    std::string asset;
    sk_sp<SkTypeface> typeface;
  };
  std::vector<TypefaceAsset> assets_;

  FML_DISALLOW_COPY_AND_ASSIGN(AssetManagerFontStyleSet);
};

class AssetManagerFontProvider : public txt::FontAssetProvider {
 public:
  AssetManagerFontProvider(std::shared_ptr<AssetManager> asset_manager);

  ~AssetManagerFontProvider() override;

  void RegisterAsset(std::string family_name, std::string asset);

  // |FontAssetProvider|
  size_t GetFamilyCount() const override;

  // |FontAssetProvider|
  std::string GetFamilyName(int index) const override;

  // |FontAssetProvider|
  SkFontStyleSet* MatchFamily(const std::string& family_name) override;

 private:
  std::shared_ptr<AssetManager> asset_manager_;
  std::unordered_map<std::string, sk_sp<AssetManagerFontStyleSet>>
      registered_families_;
  std::vector<std::string> family_names_;

  FML_DISALLOW_COPY_AND_ASSIGN(AssetManagerFontProvider);
};

}  // namespace uiwidgets
