#include "paragraph_builder.h"

#include "lib/ui/ui_mono_state.h"
#include "unicode/ustring.h"

namespace uiwidgets {
namespace {
const int tsColorIndex = 1;
const int tsTextDecorationIndex = 2;
const int tsTextDecorationColorIndex = 3;
const int tsTextDecorationStyleIndex = 4;
const int tsFontWeightIndex = 5;
const int tsFontStyleIndex = 6;
const int tsTextBaselineIndex = 7;
const int tsTextDecorationThicknessIndex = 8;
const int tsFontFamilyIndex = 9;
const int tsFontSizeIndex = 10;
const int tsLetterSpacingIndex = 11;
const int tsWordSpacingIndex = 12;
const int tsHeightIndex = 13;
const int tsLocaleIndex = 14;
const int tsBackgroundIndex = 15;
const int tsForegroundIndex = 16;
const int tsTextShadowsIndex = 17;
const int tsFontFeaturesIndex = 18;

const int tsColorMask = 1 << tsColorIndex;
const int tsTextDecorationMask = 1 << tsTextDecorationIndex;
const int tsTextDecorationColorMask = 1 << tsTextDecorationColorIndex;
const int tsTextDecorationStyleMask = 1 << tsTextDecorationStyleIndex;
const int tsTextDecorationThicknessMask = 1 << tsTextDecorationThicknessIndex;
const int tsFontWeightMask = 1 << tsFontWeightIndex;
const int tsFontStyleMask = 1 << tsFontStyleIndex;
const int tsTextBaselineMask = 1 << tsTextBaselineIndex;
const int tsFontFamilyMask = 1 << tsFontFamilyIndex;
const int tsFontSizeMask = 1 << tsFontSizeIndex;
const int tsLetterSpacingMask = 1 << tsLetterSpacingIndex;
const int tsWordSpacingMask = 1 << tsWordSpacingIndex;
const int tsHeightMask = 1 << tsHeightIndex;
const int tsLocaleMask = 1 << tsLocaleIndex;
const int tsBackgroundMask = 1 << tsBackgroundIndex;
const int tsForegroundMask = 1 << tsForegroundIndex;
const int tsTextShadowsMask = 1 << tsTextShadowsIndex;
const int tsFontFeaturesMask = 1 << tsFontFeaturesIndex;

// ParagraphStyle

const int psTextAlignIndex = 1;
const int psTextDirectionIndex = 2;
const int psFontWeightIndex = 3;
const int psFontStyleIndex = 4;
const int psMaxLinesIndex = 5;
const int psTextHeightBehaviorIndex = 6;
const int psFontFamilyIndex = 7;
const int psFontSizeIndex = 8;
const int psHeightIndex = 9;
const int psStrutStyleIndex = 10;
const int psEllipsisIndex = 11;
const int psLocaleIndex = 12;

const int psTextAlignMask = 1 << psTextAlignIndex;
const int psTextDirectionMask = 1 << psTextDirectionIndex;
const int psFontWeightMask = 1 << psFontWeightIndex;
const int psFontStyleMask = 1 << psFontStyleIndex;
const int psMaxLinesMask = 1 << psMaxLinesIndex;
const int psFontFamilyMask = 1 << psFontFamilyIndex;
const int psFontSizeMask = 1 << psFontSizeIndex;
const int psHeightMask = 1 << psHeightIndex;
const int psTextHeightBehaviorMask = 1 << psTextHeightBehaviorIndex;
const int psStrutStyleMask = 1 << psStrutStyleIndex;
const int psEllipsisMask = 1 << psEllipsisIndex;
const int psLocaleMask = 1 << psLocaleIndex;

// TextShadows decoding

constexpr uint32_t kColorDefault = 0xFF000000;
constexpr uint32_t kBytesPerShadow = 16;
constexpr uint32_t kShadowPropertiesCount = 4;
constexpr uint32_t kColorOffset = 0;
constexpr uint32_t kXOffset = 1;
constexpr uint32_t kYOffset = 2;
constexpr uint32_t kBlurOffset = 3;

// FontFeature decoding
constexpr uint32_t kBytesPerFontFeature = 8;
constexpr uint32_t kFontFeatureTagLength = 4;

// Strut decoding
const int sFontWeightIndex = 0;
const int sFontStyleIndex = 1;
const int sFontFamilyIndex = 2;
const int sFontSizeIndex = 3;
const int sHeightIndex = 4;
const int sLeadingIndex = 5;
const int sForceStrutHeightIndex = 6;

const int sFontWeightMask = 1 << sFontWeightIndex;
const int sFontStyleMask = 1 << sFontStyleIndex;
const int sFontFamilyMask = 1 << sFontFamilyIndex;
const int sFontSizeMask = 1 << sFontSizeIndex;
const int sHeightMask = 1 << sHeightIndex;
const int sLeadingMask = 1 << sLeadingIndex;
const int sForceStrutHeightMask = 1 << sForceStrutHeightIndex;

}  // namespace

fml::RefPtr<ParagraphBuilder> ParagraphBuilder::create(
    int* encoded, uint8_t* strutData, int strutDataSize,
    const std::string& fontFamily,
    const std::vector<std::string>& strutFontFamilies, float fontSize,
    float height, const std::u16string& ellipsis, const std::string& locale) {
  return fml::MakeRefCounted<ParagraphBuilder>(
      encoded, strutData, strutDataSize, fontFamily, strutFontFamilies,
      fontSize, height, ellipsis, locale);
}

void decodeTextShadows(uint8_t* shadows_data, int shadow_data_size,
                       std::vector<txt::TextShadow>& decoded_shadows) {
  decoded_shadows.clear();

  FML_CHECK(shadow_data_size % kBytesPerShadow == 0);
  const uint32_t* uint_data = reinterpret_cast<const uint32_t*>(shadows_data);
  const float* float_data = reinterpret_cast<const float*>(shadows_data);

  size_t shadow_count = shadow_data_size / kBytesPerShadow;
  size_t shadow_count_offset = 0;
  for (size_t shadow_index = 0; shadow_index < shadow_count; ++shadow_index) {
    shadow_count_offset = shadow_index * kShadowPropertiesCount;
    SkColor color =
        uint_data[shadow_count_offset + kColorOffset] ^ kColorDefault;
    decoded_shadows.emplace_back(
        color,
        SkPoint::Make(float_data[shadow_count_offset + kXOffset],
                      float_data[shadow_count_offset + kYOffset]),
        float_data[shadow_count_offset + kBlurOffset]);
  }
}

void decodeStrut(uint8_t* strut_data, int struct_data_size,
                 const std::vector<std::string>& strut_font_families,
                 txt::ParagraphStyle& paragraph_style) {
  if (strut_data == nullptr) {
    return;
  }

  if (struct_data_size == 0) {
    return;
  }
  paragraph_style.strut_enabled = true;

  const uint8_t* uint8_data = static_cast<const uint8_t*>(strut_data);
  uint8_t mask = uint8_data[0];

  // Data is stored in order of increasing size, eg, 8 bit ints will be before
  // any 32 bit ints. In addition, the order of decoding is the same order
  // as it is encoded, and the order is used to maintain consistency.
  size_t byte_count = 1;
  if (mask & sFontWeightMask) {
    paragraph_style.strut_font_weight =
        static_cast<txt::FontWeight>(uint8_data[byte_count++]);
  }
  if (mask & sFontStyleMask) {
    paragraph_style.strut_font_style =
        static_cast<txt::FontStyle>(uint8_data[byte_count++]);
  }

  std::vector<float> float_data;
  float_data.resize((struct_data_size - byte_count) / 4);
  memcpy(float_data.data(),
         reinterpret_cast<const char*>(strut_data) + byte_count,
         struct_data_size - byte_count);
  size_t float_count = 0;
  if (mask & sFontSizeMask) {
    paragraph_style.strut_font_size = float_data[float_count++];
  }
  if (mask & sHeightMask) {
    paragraph_style.strut_height = float_data[float_count++];
    paragraph_style.strut_has_height_override = true;
  }
  if (mask & sLeadingMask) {
    paragraph_style.strut_leading = float_data[float_count++];
  }
  if (mask & sForceStrutHeightMask) {
    // The boolean is stored as the last bit in the bitmask.
    paragraph_style.force_strut_height = (mask & 1 << 7) != 0;
  }

  if (mask & sFontFamilyMask) {
    paragraph_style.strut_font_families = strut_font_families;
  } else {
    // Provide an empty font name so that the platform default font will be
    // used.
    paragraph_style.strut_font_families.push_back("");
  }
}

ParagraphBuilder::ParagraphBuilder(
    int* encoded, uint8_t* strutData, int strutData_size,
    const std::string& fontFamily,
    const std::vector<std::string>& strutFontFamilies, float fontSize,
    float height, const std::u16string& ellipsis, const std::string& locale) {
  int32_t mask = encoded[0];
  txt::ParagraphStyle style;

  if (mask & psTextAlignMask) {
    style.text_align = txt::TextAlign(encoded[psTextAlignIndex]);
  }

  if (mask & psTextDirectionMask) {
    style.text_direction = txt::TextDirection(encoded[psTextDirectionIndex]);
  }

  if (mask & psFontWeightMask) {
    style.font_weight =
        static_cast<txt::FontWeight>(encoded[psFontWeightIndex]);
  }

  if (mask & psFontStyleMask) {
    style.font_style = static_cast<txt::FontStyle>(encoded[psFontStyleIndex]);
  }

  if (mask & psFontFamilyMask) {
    style.font_family = fontFamily;
  }

  if (mask & psFontSizeMask) {
    style.font_size = fontSize;
  }

  if (mask & psHeightMask) {
    style.height = height;
    style.has_height_override = true;
  }

  if (mask & psTextHeightBehaviorMask) {
    style.text_height_behavior = encoded[psTextHeightBehaviorIndex];
  }

  if (mask & psStrutStyleMask) {
    decodeStrut(strutData, strutData_size, strutFontFamilies, style);
  }

  if (mask & psMaxLinesMask) {
    style.max_lines = encoded[psMaxLinesIndex];
  }

  if (mask & psEllipsisMask) {
    style.ellipsis = ellipsis;
  }

  if (mask & psLocaleMask) {
    style.locale = locale;
  }

  FontCollection& font_collection =
      UIMonoState::Current()->window()->client()->GetFontCollection();

  m_paragraphBuilder = txt::ParagraphBuilder::CreateTxtBuilder(
      style, font_collection.GetFontCollection());
}

ParagraphBuilder::~ParagraphBuilder() = default;

void decodeFontFeatures(uint8_t* font_features_data,
                        int font_features_data_size,
                        txt::FontFeatures& font_features) {
  uint8_t* byte_data(font_features_data);
  FML_CHECK(font_features_data_size % kBytesPerFontFeature == 0);

  size_t feature_count = font_features_data_size / kBytesPerFontFeature;
  const char* char_data = reinterpret_cast<const char*>(font_features_data);

  for (size_t feature_index = 0; feature_index < feature_count;
       ++feature_index) {
    size_t feature_offset = feature_index * kBytesPerFontFeature;
    const char* feature_bytes = char_data + feature_offset;
    std::string tag(feature_bytes, kFontFeatureTagLength);
    int32_t value = *(reinterpret_cast<const int32_t*>(feature_bytes +
                                                       kFontFeatureTagLength));
    font_features.SetFeature(tag, value);
  }
}

void ParagraphBuilder::pushStyle(
    int* encoded, int encodedSize, char** fontFamilies, int fontFamiliesSize,
    float fontSize, float letterSpacing, float wordSpacing, float height,
    float decorationThickness, const std::string& locale,
    void** background_objects, uint8_t* background_data,
    void** foreground_objects, uint8_t* foreground_data, uint8_t* shadows_data,
    int shadow_data_size, uint8_t* font_features_data,
    int font_feature_data_size) {
  FML_DCHECK(encodedSize == 8);

  int32_t mask = encoded[0];

  // Set to use the properties of the previous style if the property is not
  // explicitly given.
  txt::TextStyle style = m_paragraphBuilder->PeekStyle();

  // Only change the style property from the previous value if a new explicitly
  // set value is available
  if (mask & tsColorMask) {
    style.color = encoded[tsColorIndex];
  }

  if (mask & tsTextDecorationMask) {
    style.decoration =
        static_cast<txt::TextDecoration>(encoded[tsTextDecorationIndex]);
  }

  if (mask & tsTextDecorationColorMask) {
    style.decoration_color = encoded[tsTextDecorationColorIndex];
  }

  if (mask & tsTextDecorationStyleMask) {
    style.decoration_style = static_cast<txt::TextDecorationStyle>(
        encoded[tsTextDecorationStyleIndex]);
  }

  if (mask & tsTextDecorationThicknessMask) {
    style.decoration_thickness_multiplier = decorationThickness;
  }

  if (mask & tsTextBaselineMask) {
    // TODO(abarth): Implement TextBaseline. The CSS version of this
    // property wasn't wired up either.
  }

  if (mask & (tsFontWeightMask | tsFontStyleMask | tsFontSizeMask |
              tsLetterSpacingMask | tsWordSpacingMask)) {
    if (mask & tsFontWeightMask)
      style.font_weight =
          static_cast<txt::FontWeight>(encoded[tsFontWeightIndex]);

    if (mask & tsFontStyleMask)
      style.font_style = static_cast<txt::FontStyle>(encoded[tsFontStyleIndex]);

    if (mask & tsFontSizeMask) style.font_size = fontSize;

    if (mask & tsLetterSpacingMask) style.letter_spacing = letterSpacing;

    if (mask & tsWordSpacingMask) style.word_spacing = wordSpacing;
  }

  if (mask & tsHeightMask) {
    style.height = height;
    style.has_height_override = true;
  }

  if (mask & tsLocaleMask) {
    style.locale = locale;
  }

  if (mask & tsBackgroundMask) {
    Paint background(background_objects, background_data);
    if (background.paint()) {
      style.has_background = true;
      style.background = *background.paint();
    }
  }

  if (mask & tsForegroundMask) {
    Paint foreground(foreground_objects, foreground_data);
    if (foreground.paint()) {
      style.has_foreground = true;
      style.foreground = *foreground.paint();
    }
  }

  if (mask & tsTextShadowsMask) {
    decodeTextShadows(shadows_data, shadow_data_size, style.text_shadows);
  }

  if (mask & tsFontFamilyMask) {
    // The child style's font families override the parent's font families.
    // If the child's fonts are not available, then the font collection will
    // use the system fallback fonts (not the parent's fonts).
    style.font_families =
        std::vector<std::string>(fontFamilies, fontFamilies + fontFamiliesSize);
  }

  if (mask & tsFontFeaturesMask) {
    decodeFontFeatures(font_features_data, font_feature_data_size,
                       style.font_features);
  }

  m_paragraphBuilder->PushStyle(style);
}

void ParagraphBuilder::pop() { m_paragraphBuilder->Pop(); }

const char* ParagraphBuilder::addText(const std::u16string& text) {
  if (text.empty()) return nullptr;

  // Use ICU to validate the UTF-16 input.  Calling u_strToUTF8 with a null
  // output buffer will return U_BUFFER_OVERFLOW_ERROR if the input is well
  // formed.
  const UChar* text_ptr = reinterpret_cast<const UChar*>(text.data());
  UErrorCode error_code = U_ZERO_ERROR;
  u_strToUTF8(nullptr, 0, nullptr, text_ptr, text.size(), &error_code);
  if (error_code != U_BUFFER_OVERFLOW_ERROR)
    return "string is not well-formed UTF-16";

  m_paragraphBuilder->AddText(text);

  return nullptr;
}

fml::RefPtr<Paragraph> ParagraphBuilder::build(
    /*Dart_Handle paragraph_handle*/) {
  return Paragraph::Create(/*paragraph_handle,*/ m_paragraphBuilder->Build());
}

const char* ParagraphBuilder::addPlaceholder(float width, float height,
                                             unsigned alignment,
                                             float baseline_offset,
                                             unsigned baseline) {
  txt::PlaceholderRun placeholder_run(
      width, height, static_cast<txt::PlaceholderAlignment>(alignment),
      static_cast<txt::TextBaseline>(baseline), baseline_offset);

  m_paragraphBuilder->AddPlaceholder(placeholder_run);

  return nullptr;
}

UIWIDGETS_API(ParagraphBuilder*)
ParagraphBuilder_constructor(int* encoded, int encodedSize, uint8_t* structData,
                             int structDataSize, char* fontFamily,
                             char** structFontFamily, int structFontFamilySize,
                             float fontSize, float height, char16_t* ellipsis,
                             char* locale) {
  std::string fontFamily_s = fontFamily ? std::string(fontFamily) : "";
  auto structFamily_v =
      structFontFamily
          ? std::vector<std::string>(structFontFamily,
                                     structFontFamily + structFontFamilySize)
          : std::vector<std::string>();
  auto ellipsis_s = ellipsis ? std::u16string(ellipsis) : u"";
  auto local_s = locale ? std::string(locale) : "";
  fml::RefPtr<ParagraphBuilder> paragraphBuilder = ParagraphBuilder::create(
      encoded, structData, structDataSize, fontFamily_s, structFamily_v,
      fontSize, height, ellipsis_s, local_s);
  paragraphBuilder->AddRef();
  return paragraphBuilder.get();
}

UIWIDGETS_API(void) ParagraphBuilder_dispose(Canvas* ptr) { ptr->Release(); }

UIWIDGETS_API(void)
ParagraphBuilder_pushStyle(ParagraphBuilder* ptr, int* encoded, int encodedSize,
                           char** fontFamilies, int fontFamiliesSize,
                           float fontSize, float letterSpacing,
                           float wordSpacing, float height,
                           float decorationThickness, char* locale,
                           void** background_objects, uint8_t* background_data,
                           void** foreground_objects, uint8_t* foreground_data,
                           uint8_t* shadows_data, int shadow_data_size,
                           uint8_t* font_features_data,
                           int font_feature_data_size) {
  ptr->pushStyle(encoded, encodedSize, fontFamilies, fontFamiliesSize, fontSize,
                 letterSpacing, wordSpacing, height, decorationThickness,
                 locale, background_objects, background_data,
                 foreground_objects, foreground_data, shadows_data,
                 shadow_data_size, font_features_data, font_feature_data_size);
}
UIWIDGETS_API(void) ParagraphBuilder_pop(ParagraphBuilder* ptr) { ptr->pop(); }

UIWIDGETS_API(const char*)
ParagraphBuilder_addText(ParagraphBuilder* ptr, char16_t* text) {
  return ptr->addText(std::u16string(text));
}

UIWIDGETS_API(const char*)
ParagraphBuilder_addPlaceholder(ParagraphBuilder* ptr, float width,
                                float height, int alignment,
                                float baselineOffset, unsigned baseline) {
  return ptr->addPlaceholder(width, height, alignment, baselineOffset,
                             baseline);
}

UIWIDGETS_API(Paragraph*)
ParagraphBuilder_build(ParagraphBuilder* ptr) {
  auto paragraph = ptr->build();
  paragraph->AddRef();
  return paragraph.get();
}
}  // namespace uiwidgets