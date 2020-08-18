#pragma once

#include <flutter/fml/memory/ref_counted.h>
#include <src\lib\ui\painting\canvas.h>
#include <flutter\third_party\txt\src\txt\paragraph.h>
#include "flutter/third_party/txt/src/txt/paragraph_builder.h"
#include "utils.h"
#include "paragraph.h"

namespace uiwidgets {

	class ParagraphBuilder : public fml::RefCountedThreadSafe<ParagraphBuilder> {
        FML_FRIEND_MAKE_REF_COUNTED(ParagraphBuilder);

	public:
        static fml::RefPtr<ParagraphBuilder> create(int* encoded, uint8_t* structData, int structDataSize, const std::string& fontFamily, const std::vector<std::string>& strutFontFamilies, double fontSize, double height, const std::u16string& ellipsis, const std::string& locale);
       
        void pushStyle(
            int* encoded,
            int encodedSize,
            char** fontFamilies,
            int fontFamiliesSize,
            double fontSize,
            double letterSpacing,
            double wordSpacing,
            double height,
            double decorationThickness,
            const std::string& locale,
            void** background_objects,
            uint8_t* background_data,
            void** foreground_objects,
            uint8_t* foreground_data,
            uint8_t* shadows_data,
            int shadow_data_size,
            uint8_t* font_features_data,
            int font_feature_data_size);

        const char* addText(const std::u16string& text);
        const char* addPlaceholder(
            double width,
            double height,
            unsigned alignment,
            double baseline_offset,
            unsigned baseline);
        fml::RefPtr<Paragraph> build(/*Dart_Handle paragraph_handle*/);
        ~ParagraphBuilder();

        void pop();

	private:
        explicit ParagraphBuilder(
            int* encoded,
            uint8_t* strutData,
            int strutData_size,
            const std::string& fontFamily,
            const std::vector<std::string>& strutFontFamilies,
            double fontSize,
            double height,
            const std::u16string& ellipsis,
            const std::string& locale);

		std::unique_ptr<txt::ParagraphBuilder> m_paragraphBuilder;
    };
}