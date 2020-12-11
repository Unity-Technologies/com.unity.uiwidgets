#include "font_collection.h"

#include <mutex>

#include "include/core/SkFontMgr.h"
#include "include/core/SkGraphics.h"
#include "include/core/SkStream.h"
#include "include/core/SkTypeface.h"
#include "asset_manager_font_provider.h"
#include "lib/ui/ui_mono_state.h"
#include "lib/ui/window/window.h"
#include "txt/asset_font_manager.h"
#include "txt/test_font_manager.h"

namespace uiwidgets {
namespace {
typedef void (*LoadFontCallback)(Mono_Handle callback_handle);

UIWIDGETS_API(void)
Font_LoadFontFromList(uint8_t* font_data, int size,
                 LoadFontCallback loadFontCallback, Mono_Handle callbackHandle,
                 char* family_name) {
  FontCollection& font_collection =
      UIMonoState::Current()->window()->client()->GetFontCollection();
  font_collection.LoadFontFromList(font_data, size, family_name);
  loadFontCallback(callbackHandle);
}

}  // namespace

FontCollection::FontCollection()
    : collection_(std::make_shared<txt::FontCollection>()) {
  dynamic_font_manager_ = sk_make_sp<txt::DynamicFontManager>();
  collection_->SetDynamicFontManager(dynamic_font_manager_);
}

FontCollection::~FontCollection() {
  collection_.reset();
  SkGraphics::PurgeFontCache();
}

std::shared_ptr<txt::FontCollection> FontCollection::GetFontCollection() const {
  return collection_;
}

void FontCollection::SetupDefaultFontManager() {
  collection_->SetupDefaultFontManager();
}

void FontCollection::RegisterFonts(
    std::shared_ptr<AssetManager> asset_manager) {
  std::unique_ptr<fml::Mapping> manifest_mapping =
      asset_manager->GetAsMapping("FontManifest.json");
  if (manifest_mapping == nullptr) {
    FML_DLOG(WARNING) << "Could not find the font manifest in the asset store.";
    return;
  }

  rapidjson::Document document;
  static_assert(sizeof(decltype(document)::Ch) == sizeof(uint8_t), "");
  document.Parse(reinterpret_cast<const decltype(document)::Ch*>(
                     manifest_mapping->GetMapping()),
                 manifest_mapping->GetSize());

  if (document.HasParseError()) {
    FML_DLOG(WARNING) << "Error parsing the font manifest in the asset store.";
    return;
  }

  if (document.IsArray()) {
    RegisterFonts(asset_manager, document.GetArray());
  }
}

void FontCollection::RegisterFonts(
    std::shared_ptr<AssetManager> asset_manager, rapidjson::Value::Array fonts) {
  
  auto font_provider =
      std::make_unique<AssetManagerFontProvider>(asset_manager);

  for (const auto& family : fonts) {
    auto family_name = family.FindMember("family");
    if (family_name == family.MemberEnd() || !family_name->value.IsString()) {
      continue;
    }

    auto family_fonts = family.FindMember("fonts");
    if (family_fonts == family.MemberEnd() || !family_fonts->value.IsArray()) {
      continue;
    }

    for (const auto& family_font : family_fonts->value.GetArray()) {
      if (!family_font.IsObject()) {
        continue;
      }

      auto font_asset = family_font.FindMember("asset");
      if (font_asset == family_font.MemberEnd() ||
          !font_asset->value.IsString()) {
        continue;
      }

      // TODO: Handle weights and styles.
      font_provider->RegisterAsset(family_name->value.GetString(),
                                   font_asset->value.GetString());
    }
  }

  collection_->SetAssetFontManager(
      sk_make_sp<txt::AssetFontManager>(std::move(font_provider)));
}

void FontCollection::LoadFontFromList(const uint8_t* font_data, int length,
                                      std::string family_name) {
  std::unique_ptr<SkStreamAsset> font_stream =
      std::make_unique<SkMemoryStream>(font_data, length, true);
  sk_sp<SkTypeface> typeface =
      SkTypeface::MakeFromStream(std::move(font_stream));
  txt::TypefaceFontAssetProvider& font_provider =
      dynamic_font_manager_->font_provider();
  if (family_name.empty()) {
    font_provider.RegisterTypeface(typeface);
  } else {
    font_provider.RegisterTypeface(typeface, family_name);
  }
  collection_->ClearFontFamilyCache();
}

}  // namespace uiwidgets
