#pragma once

#include <memory>
#include <vector>

#include "assets/asset_manager.h"
#include "flutter/fml/macros.h"
#include "flutter/fml/memory/ref_ptr.h"
#include "txt/font_collection.h"

namespace uiwidgets {

    class FontCollection {
    public:
        FontCollection();

        ~FontCollection();

        //static void RegisterNatives(tonic::DartLibraryNatives* natives);

        std::shared_ptr<txt::FontCollection> GetFontCollection() const;

        void SetupDefaultFontManager();

        void RegisterFonts(std::shared_ptr<AssetManager> asset_manager);

        void LoadFontFromList(const uint8_t* font_data,
            int length,
            std::string family_name);

    private:
        std::shared_ptr<txt::FontCollection> collection_;
        sk_sp<txt::DynamicFontManager> dynamic_font_manager_;

        FML_DISALLOW_COPY_AND_ASSIGN(FontCollection);
    };

}  // namespace flutter
