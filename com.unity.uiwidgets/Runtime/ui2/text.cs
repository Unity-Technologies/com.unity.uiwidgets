using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using AOT;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.ui;
using UnityEngine;
using Unity.UIWidgets.async2;
using Rect = Unity.UIWidgets.ui.Rect;

namespace Unity.UIWidgets.ui2 {
    public enum FontStyle {
        normal,

        italic,
    }

    public class FontWeight {
        public FontWeight(int index) {
            this.index = index;
        }

        public readonly int index;

        public static readonly FontWeight w100 = new FontWeight(0);

        public static readonly FontWeight w200 = new FontWeight(1);

        public static readonly FontWeight w300 = new FontWeight(2);

        public static readonly FontWeight w400 = new FontWeight(3);

        public static readonly FontWeight w500 = new FontWeight(4);

        public static readonly FontWeight w600 = new FontWeight(5);

        public static readonly FontWeight w700 = new FontWeight(6);

        public static readonly FontWeight w800 = new FontWeight(7);

        public static readonly FontWeight w900 = new FontWeight(8);

        public static readonly FontWeight normal = w400;

        public static readonly FontWeight bold = w700;

        public static readonly List<FontWeight> values = new List<FontWeight> {
            w100, w200, w300, w400, w500, w600, w700, w800, w900
        };

        public static FontWeight lerp(FontWeight a, FontWeight b, float t) {
            D.assert(t != null);
            if (a == null && b == null)
                return null;
            return values[Mathf.Lerp(a?.index ?? normal.index, b?.index ?? normal.index, t).round().clamp(0, 8)];
        }

        public static readonly Dictionary<int, String> map = new Dictionary<int, String> {
            {0, "FontWeight.w100"},
            {1, "FontWeight.w200"},
            {2, "FontWeight.w300"},
            {3, "FontWeight.w400"},
            {4, "FontWeight.w500"},
            {5, "FontWeight.w600"},
            {6, "FontWeight.w700"},
            {7, "FontWeight.w800"},
            {8, "FontWeight.w900"},
        };

        public override string ToString() {
            return map[index];
        }
    }

    public class FontFeature {
        public FontFeature(string feature, int value = 1) {
            D.assert(feature != null);
            D.assert(feature.Length == 4);
            D.assert(value != null);
            D.assert(value >= 0);
            this.feature = feature;
            this.value = value;
        }

        public FontFeature enable(String feature) {
            return new FontFeature(feature, 1);
        }

        public FontFeature disable(String feature) {
            return new FontFeature(feature, 0);
        }

        public FontFeature randomize() {
            return new FontFeature("rand", 1);
        }

        public static FontFeature stylisticSet(int value) {
            D.assert(value >= 1);
            D.assert(value <= 20);
            return new FontFeature($"ss{value.ToString().PadLeft(2, '0')}");
        }

        public FontFeature slashedZero() {
            return new FontFeature("zero", 1);
        }

        public FontFeature oldstyleFigures() {
            return new FontFeature("onum", 1);
        }

        public FontFeature proportionalFigures() {
            return new FontFeature("pnum", 1);
        }

        public FontFeature tabularFigures() {
            return new FontFeature("tnum", 1);
        }

        public readonly String feature;

        public readonly int value;

        public static readonly int _kEncodedSize = 8;

        public void _encode(byte[] byteData) {
            D.assert(() => {
                bool result = true;
                foreach (var charUnit in feature.ToCharArray()) {
                    if (charUnit <= 0x20 && charUnit >= 0x7F) {
                        return false;
                    }
                }

                return true;
            });
            for (int i = 0; i < 4; i++) {
                byteData[i] = (byte) feature[i];
            }

            byteData[4] = (byte) (value >> 0);
            byteData[5] = (byte) (value >> 8);
            byteData[6] = (byte) (value >> 16);
            byteData[7] = (byte) (value >> 24);
        }


        public override bool Equals(object obj) {
            if (ReferenceEquals(this, obj)) {
                return true;
            }

            return obj is FontFeature fontFeature && fontFeature.feature == feature &&
                   fontFeature.value == value;
        }

        public override int GetHashCode() {
            int hashcode = value.GetHashCode();
            hashcode = (hashcode ^ 397) ^ feature.GetHashCode();
            return hashcode;
        }

        public override string ToString() {
            return $"FontFeature({feature}, {value})";
        }
    }

    public enum TextAlign {
        left,

        right,

        center,

        justify,

        start,

        end,
    }

    public enum TextBaseline {
        alphabetic,

        ideographic,
    }

    public class TextDecoration {
        public TextDecoration(int _mask) {
            this._mask = _mask;
        }

        public TextDecoration combine(List<TextDecoration> decorations) {
            int mask = 0;
            foreach (TextDecoration decoration in decorations) {
                mask |= decoration._mask;
            }

            return new TextDecoration(mask);
        }

        public readonly int _mask;

        bool contains(TextDecoration other) {
            return (_mask | other._mask) == _mask;
        }

        public static readonly TextDecoration none = new TextDecoration(0x0);

        public static readonly TextDecoration underline = new TextDecoration(0x1);

        public static readonly TextDecoration overline = new TextDecoration(0x2);

        public static readonly TextDecoration lineThrough = new TextDecoration(0x4);

        public override bool Equals(object obj) {
            return obj is TextDecoration textDecoration
                   && textDecoration._mask == _mask;
        }

        public override int GetHashCode() {
            return _mask.GetHashCode();
        }


        public override string ToString() {
            if (_mask == 0) {
                return "TextDecoration.none";
            }

            List<String> values = new List<string>();
            if ((_mask & TextDecoration.underline._mask) != 0)
                values.Add("underline");
            if ((_mask & TextDecoration.overline._mask) != 0)
                values.Add("overline");
            if ((_mask & TextDecoration.lineThrough._mask) != 0)
                values.Add("lineThrough");
            if (values.Count == 1)
                return $"TextDecoration.{values[0]}";
            return $"TextDecoration.combine([{String.Join(", ", values)}])";
        }
    }


    public enum TextDecorationStyle {
        solid,

        doubleLine,

        dotted,

        dashed,

        wavy
    }

    public class TextHeightBehavior {
        public TextHeightBehavior(
            bool applyHeightToFirstAscent = true,
            bool applyHeightToLastDescent = true
        ) {
            this.applyHeightToFirstAscent = applyHeightToFirstAscent;
            this.applyHeightToLastDescent = applyHeightToLastDescent;
        }

        public static TextHeightBehavior fromEncoded(int encoded) {
            return new TextHeightBehavior((encoded & 0x1) == 0, (encoded & 0x2) == 0);
        }


        public readonly bool applyHeightToFirstAscent;

        public readonly bool applyHeightToLastDescent;

        public int encode() {
            return (applyHeightToFirstAscent ? 0 : 1 << 0) | (applyHeightToLastDescent ? 0 : 1 << 1);
        }

        public override bool Equals(object other) {
            return other is TextHeightBehavior otherText
                   && otherText.applyHeightToFirstAscent == applyHeightToFirstAscent
                   && otherText.applyHeightToLastDescent == applyHeightToLastDescent;
        }


        public override int GetHashCode() {
            int hashcode = applyHeightToFirstAscent.GetHashCode();
            hashcode = (hashcode ^ 397) ^ applyHeightToLastDescent.GetHashCode();
            return hashcode;
        }

        public override string ToString() {
            return "TextHeightBehavior(\n" +
                   " applyHeightToFirstAscent: $applyHeightToFirstAscent, \n" +
                   " applyHeightToLastDescent: $applyHeightToLastDescent \n" +
                   ")";
        }
    }

    public partial class ui_ {
        public static void SetFloat(List<byte> data, int offSet, float value) {
            var bytes = BitConverter.GetBytes(value);
            data[offSet] = bytes[0];
            data[offSet + 1] = bytes[1];
            data[offSet + 2] = bytes[2];
            data[offSet + 3] = bytes[3];
        }

        public static bool _listEquals<T>(List<T> a, List<T> b) {
            if (a == null)
                return b == null;
            if (b == null || a.Count != b.Count)
                return false;
            for (int index = 0; index < a.Count; index += 1) {
                if (!a[index].Equals(b[index]))
                    return false;
            }

            return true;
        }

        public static List<int> _encodeTextStyle(
            Color color,
            TextDecoration decoration,
            Color decorationColor,
            TextDecorationStyle? decorationStyle,
            float? decorationThickness,
            FontWeight fontWeight,
            FontStyle? fontStyle,
            TextBaseline? textBaseline,
            String fontFamily,
            List<String> fontFamilyFallback,
            float? fontSize,
            float? letterSpacing,
            float? wordSpacing,
            float? height,
            Locale locale,
            Paint background,
            Paint foreground,
            List<Shadow> shadows,
            List<FontFeature> fontFeatures
        ) {
            List<int> result = new List<int>(new int[8]);
            if (color != null) {
                result[0] |= 1 << 1;
                result[1] = (int) color.value;
            }

            if (decoration != null) {
                result[0] |= 1 << 2;
                result[2] = decoration._mask;
            }

            if (decorationColor != null) {
                result[0] |= 1 << 3;
                result[3] = (int) decorationColor.value;
            }

            if (decorationStyle != null) {
                result[0] |= 1 << 4;
                result[4] = (int) decorationStyle;
            }

            if (fontWeight != null) {
                result[0] |= 1 << 5;
                result[5] = fontWeight.index;
            }

            if (fontStyle != null) {
                result[0] |= 1 << 6;
                result[6] = (int) fontStyle;
            }

            if (textBaseline != null) {
                result[0] |= 1 << 7;
                result[7] = (int) textBaseline;
            }

            if (decorationThickness != null) {
                result[0] |= 1 << 8;
            }

            if (fontFamily != null || (fontFamilyFallback != null && fontFamilyFallback.isNotEmpty())) {
                result[0] |= 1 << 9;
                // Passed separately to native.
            }

            if (fontSize != null) {
                result[0] |= 1 << 10;
                // Passed separately to native.
            }

            if (letterSpacing != null) {
                result[0] |= 1 << 11;
                // Passed separately to native.
            }

            if (wordSpacing != null) {
                result[0] |= 1 << 12;
                // Passed separately to native.
            }

            if (height != null) {
                result[0] |= 1 << 13;
                // Passed separately to native.
            }

            if (locale != null) {
                result[0] |= 1 << 14;
                // Passed separately to native.
            }

            if (background != null) {
                result[0] |= 1 << 15;
                // Passed separately to native.
            }

            if (foreground != null) {
                result[0] |= 1 << 16;
                // Passed separately to native.
            }

            if (shadows != null) {
                result[0] |= 1 << 17;
                // Passed separately to native.
            }

            if (fontFeatures != null) {
                result[0] |= 1 << 18;
                // Passed separately to native.
            }

            return result;
        }

        public static List<int> _encodeParagraphStyle(
            TextAlign? textAlign,
            TextDirection? textDirection,
            int? maxLines,
            String fontFamily,
            float? fontSize,
            float? height,
            TextHeightBehavior textHeightBehavior,
            FontWeight fontWeight,
            FontStyle? fontStyle,
            StrutStyle strutStyle,
            String ellipsis,
            Locale locale
        ) {
            List<int> result = new List<int>(new int[7]);
            if (textAlign != null) {
                result[0] |= 1 << 1;
                result[1] = (int) textAlign;
            }

            if (textDirection != null) {
                result[0] |= 1 << 2;
                result[2] = (int) textDirection;
            }

            if (fontWeight != null) {
                result[0] |= 1 << 3;
                result[3] = fontWeight.index;
            }

            if (fontStyle != null) {
                result[0] |= 1 << 4;
                result[4] = (int) fontStyle;
            }

            if (maxLines != null) {
                result[0] |= 1 << 5;
                result[5] = maxLines.Value;
            }

            if (textHeightBehavior != null) {
                result[0] |= 1 << 6;
                result[6] = textHeightBehavior.encode();
            }

            if (fontFamily != null) {
                result[0] |= 1 << 7;
                // Passed separately to native.
            }

            if (fontSize != null) {
                result[0] |= 1 << 8;
                // Passed separately to native.
            }

            if (height != null) {
                result[0] |= 1 << 9;
                // Passed separately to native.
            }

            if (strutStyle != null) {
                result[0] |= 1 << 10;
                // Passed separately to native.
            }

            if (ellipsis != null) {
                result[0] |= 1 << 11;
                // Passed separately to native.
            }

            if (locale != null) {
                result[0] |= 1 << 12;
                // Passed separately to native.
            }

            return result;
        }

        internal static List<byte> _encodeStrut(
            String fontFamily,
            List<String> fontFamilyFallback,
            float fontSize,
            float height,
            float leading,
            FontWeight fontWeight,
            FontStyle fontStyle,
            bool forceStrutHeight) {
            if (fontFamily == null &&
                fontSize == null &&
                height == null &&
                leading == null &&
                fontWeight == null &&
                fontStyle == null &&
                forceStrutHeight == null) {
                return new List<byte>(0);
            }

            List<byte> data = new List<byte>(15); // Max size is 15 bytes
            int bitmask = 0; // 8 bit mask
            int byteCount = 1;
            if (fontWeight != null) {
                bitmask |= 1 << 0;
                data[byteCount] = (byte) fontWeight.index;
                byteCount += 1;
            }

            if (fontStyle != null) {
                bitmask |= 1 << 1;
                data[byteCount] = (byte) fontStyle;
                byteCount += 1;
            }

            if (fontFamily != null || (fontFamilyFallback != null && fontFamilyFallback.isNotEmpty())) {
                bitmask |= 1 << 2;
                // passed separately to native
            }

            if (fontSize != null) {
                bitmask |= 1 << 3;
                SetFloat(data, byteCount, (float) fontSize);
                byteCount += 4;
            }

            if (height != null) {
                bitmask |= 1 << 4;
                SetFloat(data, byteCount, (float) height);
                byteCount += 4;
            }

            if (leading != null) {
                bitmask |= 1 << 5;
                SetFloat(data, byteCount, (float) leading);
                byteCount += 4;
            }

            if (forceStrutHeight != null) {
                bitmask |= 1 << 6;
                // We store this boolean directly in the bitmask since there is
                // extra space in the 16 bit int.
                bitmask |= (forceStrutHeight ? 1 : 0) << 7;
            }

            data[0] = (byte) bitmask;
            return data.GetRange(0, byteCount);
        }

        public static unsafe Future loadFontFromList(byte[] list, string fontFamily = null) {
            return ui_._futurize((_Callback<object> callback) => {
                // var completer = new Promise(true);
                GCHandle completerHandle = GCHandle.Alloc(callback);
                fixed (byte* listPtr = list) {
                    LoadFontFromList(listPtr, list.Length, _loadFontCallback, (IntPtr) completerHandle, fontFamily);
                }

                return null;
            });
        }

        [MonoPInvokeCallback(typeof(_loadFontFromListCallback))]
        static void _loadFontCallback(IntPtr callbackHandle) {
            GCHandle completerHandle = (GCHandle) callbackHandle;
            var callback = (_Callback<object>) completerHandle.Target;
            completerHandle.Free();
            _sendFontChangeMessage();
            try {
                callback(new object());
            }
            catch (Exception ex) {
                Debug.LogException(ex);
            }
        }

        static byte[] _fontChangeMessage =
            Encoding.ASCII.GetBytes(JsonUtility.ToJson(new Dictionary<string, dynamic>() {{"type", "fontsChange"}}));

        public static async void _sendFontChangeMessage() {
            unsafe {
                Window window = new Window();
                window.onPlatformMessage?.Invoke("uiwidgets/system", _fontChangeMessage,
                    (byte[] dataIn) => { });
            }
        }

        delegate void _loadFontFromListCallback(IntPtr callbackHandle);

        [DllImport(NativeBindings.dllName)]
        static extern unsafe void LoadFontFromList(byte* list, int size, _loadFontFromListCallback callback,
            IntPtr callbackHandle,
            string fontFamily);

        public struct Float32List {
            public unsafe float* data;
            public int Length;
        }
    }

    public class TextStyle {
        public TextStyle(
            Color color = null,
            TextDecoration decoration = null,
            Color decorationColor = null,
            TextDecorationStyle? decorationStyle = null,
            float? decorationThickness = null,
            FontWeight fontWeight = null,
            FontStyle? fontStyle = null,
            TextBaseline? textBaseline = null,
            String fontFamily = null,
            List<String> fontFamilyFallback = null,
            float? fontSize = null,
            float? letterSpacing = null,
            float? wordSpacing = null,
            float? height = null,
            Locale locale = null,
            Paint background = null,
            Paint foreground = null,
            List<Shadow> shadows = null,
            List<FontFeature> fontFeatures = null
        ) {
            D.assert(color == null || foreground == null
                // "Cannot provide both a color and a foreground\n"+
                // "The color argument is just a shorthand for \"foreground: Paint()..color = color\"."
            );
            _encoded = ui_._encodeTextStyle(
                color,
                decoration,
                decorationColor,
                decorationStyle,
                decorationThickness,
                fontWeight,
                fontStyle,
                textBaseline,
                fontFamily,
                fontFamilyFallback,
                fontSize,
                letterSpacing,
                wordSpacing,
                height,
                locale,
                background,
                foreground,
                shadows,
                fontFeatures
            );

            _fontFamily = fontFamily ?? "";
            _fontFamilyFallback = fontFamilyFallback;
            _fontSize = fontSize;
            _letterSpacing = letterSpacing;
            _wordSpacing = wordSpacing;
            _height = height;
            _decorationThickness = decorationThickness;
            _locale = locale;
            _background = background;
            _foreground = foreground;
            _shadows = shadows;
            _fontFeatures = fontFeatures;
        }

        public readonly List<int> _encoded;
        public readonly String _fontFamily;
        public readonly List<String> _fontFamilyFallback;
        public readonly float? _fontSize;
        public readonly float? _letterSpacing;
        public readonly float? _wordSpacing;
        public readonly float? _height;
        public readonly float? _decorationThickness;
        public readonly Locale _locale;
        public readonly Paint _background;
        public readonly Paint _foreground;
        public readonly List<Shadow> _shadows;
        public readonly List<FontFeature> _fontFeatures;

        public override bool Equals(object obj) {
            if (ReferenceEquals(this, obj))
                return true;
            return obj is TextStyle textStyle
                   && textStyle._fontFamily == _fontFamily
                   && textStyle._fontSize == _fontSize
                   && textStyle._letterSpacing == _letterSpacing
                   && textStyle._wordSpacing == _wordSpacing
                   && textStyle._height == _height
                   && textStyle._decorationThickness == _decorationThickness
                   && textStyle._locale == _locale
                   && textStyle._background == _background
                   && textStyle._foreground == _foreground
                   && ui_._listEquals<int>(textStyle._encoded, _encoded)
                   && ui_._listEquals<Shadow>(textStyle._shadows, _shadows)
                   && ui_._listEquals<String>(textStyle._fontFamilyFallback, _fontFamilyFallback)
                   && ui_._listEquals<FontFeature>(textStyle._fontFeatures, _fontFeatures);
        }

        public override int GetHashCode() {
            int hashcode = _encoded.hashList();
            hashcode = (hashcode ^ 397) ^ (_fontFamily.GetHashCode());

            hashcode = (hashcode ^ 397) ^ _fontFamilyFallback.hashList();
            hashcode = (hashcode ^ 397) ^ _fontSize.GetHashCode();
            hashcode = (hashcode ^ 397) ^ _letterSpacing.GetHashCode();
            hashcode = (hashcode ^ 397) ^ _wordSpacing.GetHashCode();
            hashcode = (hashcode ^ 397) ^ _height.GetHashCode();
            hashcode = (hashcode ^ 397) ^ _locale.GetHashCode();
            hashcode = (hashcode ^ 397) ^ _background.GetHashCode();
            hashcode = (hashcode ^ 397) ^ _foreground.GetHashCode();
            hashcode = (hashcode ^ 397) ^ _shadows.hashList();
            hashcode = (hashcode ^ 397) ^ _decorationThickness.GetHashCode();
            hashcode = (hashcode ^ 397) ^ _fontFeatures.hashList();
            return hashcode;
        }

        public override string ToString() {
            string color = (_encoded[0] & 0x00002) == 0x00002
                ? (new Color((uint) _encoded[1])).ToString()
                : "unspecified";
            string decoration = (_encoded[0] & 0x00004) == 0x00004
                ? (new TextDecoration(_encoded[2])).ToString()
                : "unspecified";
            string decorationColor = (_encoded[0] & 0x00008) == 0x00008
                ? (new Color((uint) _encoded[3])).ToString()
                : "unspecified";
            string decorationStyle = (_encoded[0] & 0x00010) == 0x00010
                ? ((TextDecorationStyle) _encoded[4]).ToString()
                : "unspecified";
            // The decorationThickness is not in encoded order in order to keep it near the other decoration properties.
            string decorationThickness = (_encoded[0] & 0x00100) == 0x00100
                ? _decorationThickness.ToString()
                : "unspecified";
            string fontWeight = (_encoded[0] & 0x00020) == 0x00020
                ? FontWeight.values[_encoded[5]].ToString()
                : "unspecified";
            string fontStyle = (_encoded[0] & 0x00040) == 0x00040
                ? ((FontStyle) _encoded[6]).ToString()
                : "unspecified";
            string textBaseline = (_encoded[0] & 0x00080) == 0x00080
                ? ((TextBaseline) _encoded[7]).ToString()
                : "unspecified";
            string fontFamily = (_encoded[0] & 0x00200) == 0x00200
                                && _fontFamily != null
                ? _fontFamily
                : "unspecified";
            string fontFamilyFallback = (_encoded[0] & 0x00200) == 0x00200
                                        && _fontFamilyFallback != null
                                        && _fontFamilyFallback.isNotEmpty()
                ? _fontFamilyFallback.ToString()
                : "unspecified";
            string fontSize = (_encoded[0] & 0x00400) == 0x00400 ? _fontSize.ToString() : "unspecified";
            string letterSpacing = (_encoded[0] & 0x00800) == 0x00800 ? "_letterSpacing}x" : "unspecified";
            string wordSpacing = (_encoded[0] & 0x01000) == 0x01000 ? "_wordSpacing}x" : "unspecified";
            string height = (_encoded[0] & 0x02000) == 0x02000 ? "_height}x" : "unspecified";
            string locale = (_encoded[0] & 0x04000) == 0x04000 ? _locale.ToString() : "unspecified";
            string background = (_encoded[0] & 0x08000) == 0x08000 ? _locale.ToString() : "unspecified";
            string foreground = (_encoded[0] & 0x10000) == 0x10000 ? _locale.ToString() : "unspecified";
            string shadows = (_encoded[0] & 0x20000) == 0x20000 ? _locale.ToString() : "unspecified";

            string fontFeatures =
                (_encoded[0] & 0x40000) == 0x40000 ? _fontFeatures.ToString() : "unspecified";

            return "TextStyle(\n" +
                   $"color: {color}\n" +
                   $"decoration: {decoration}\n" +
                   $"decorationColor: {decorationColor}\n" +
                   $"decorationStyle: {decorationStyle}\n" +
                   // The decorationThickness is not in encoded order in order to keep it near the other decoration properties.
                   $"decorationThickness: {decorationThickness}\n" +
                   $"fontWeight: {fontWeight}\n" +
                   $"fontStyle: {fontStyle}\n" +
                   $"textBaseline: {textBaseline}\n" +
                   $"fontFamily: {fontFamily}\n" +
                   $"fontFamilyFallback: {fontFamilyFallback}\n" +
                   $"fontSize: {fontSize}\n" +
                   $"letterSpacing: {letterSpacing}\n" +
                   $"wordSpacing: {wordSpacing}\n" +
                   $"height: {height}\n" +
                   $"locale: {locale}\n" +
                   $"background: {background}\n" +
                   $"foreground: {foreground}\n" +
                   $"shadows: {shadows}\n" +
                   $"fontFeatures: {fontFeatures}\n" +
                   ")";
        }
    }

    public class ParagraphStyle {
        public ParagraphStyle(
            TextAlign? textAlign = null,
            TextDirection? textDirection = null,
            int? maxLines = null,
            String fontFamily = null,
            float? fontSize = null,
            float? height = null,
            TextHeightBehavior textHeightBehavior = null,
            FontWeight fontWeight = null,
            FontStyle? fontStyle = null,
            StrutStyle strutStyle = null,
            string ellipsis = null,
            Locale locale = null
        ) {
            _encoded = ui_._encodeParagraphStyle(
                textAlign,
                textDirection,
                maxLines,
                fontFamily,
                fontSize,
                height,
                textHeightBehavior,
                fontWeight,
                fontStyle,
                strutStyle,
                ellipsis,
                locale
            );
            _fontFamily = fontFamily;
            _fontSize = fontSize;
            _height = height;
            _strutStyle = strutStyle;
            _ellipsis = ellipsis;
            _locale = locale;
        }

        public readonly List<int> _encoded;
        public readonly String _fontFamily;
        public readonly float? _fontSize;
        public readonly float? _height;
        public readonly StrutStyle _strutStyle;
        public readonly String _ellipsis;
        public readonly Locale _locale;

        public override bool Equals(object other) {
            if (ReferenceEquals(this, other))
                return true;
            return other is ParagraphStyle otherParagraph
                   && otherParagraph._fontFamily == _fontFamily
                   && otherParagraph._fontSize == _fontSize
                   && otherParagraph._height == _height
                   && otherParagraph._strutStyle == _strutStyle
                   && otherParagraph._ellipsis == _ellipsis
                   && otherParagraph._locale == _locale
                   && ui_._listEquals<int>(otherParagraph._encoded, _encoded);
        }

        public override int GetHashCode() {
            int hashcode = _encoded.hashList();
            hashcode = (hashcode ^ 397) ^ _fontFamily.GetHashCode();
            hashcode = (hashcode ^ 397) ^ _fontSize.GetHashCode();
            hashcode = (hashcode ^ 397) ^ _height.GetHashCode();
            hashcode = (hashcode ^ 397) ^ _ellipsis.GetHashCode();
            hashcode = (hashcode ^ 397) ^ _locale.GetHashCode();

            return hashcode;
        }

        public override string ToString() {
            string textAlign = (_encoded[0] & 0x002) == 0x002
                ? ((TextAlign) _encoded[1]).ToString()
                : "unspecified";
            string textDirection = (_encoded[0] & 0x004) == 0x004
                ? ((TextDirection) _encoded[2]).ToString()
                : "unspecified";
            string fontWeight = (_encoded[0] & 0x008) == 0x008
                ? FontWeight.values[_encoded[3]].ToString()
                : "unspecified";
            string fontStyle = (_encoded[0] & 0x010) == 0x010
                ? ((FontStyle) _encoded[4]).ToString()
                : "unspecified";
            string maxLines = (_encoded[0] & 0x020) == 0x020 ? _encoded[5].ToString() : "unspecified";
            string textHeightBehavior = (_encoded[0] & 0x040) == 0x040
                ? TextHeightBehavior.fromEncoded(_encoded[6]).ToString()
                : "unspecified";
            string fontFamily = (_encoded[0] & 0x080) == 0x080 ? _fontFamily : "unspecified";
            string fontSize = (_encoded[0] & 0x100) == 0x100 ? _fontSize.ToString() : "unspecified";
            string height = (_encoded[0] & 0x200) == 0x200 ? $"{_height}x" : "unspecified";
            string ellipsis = (_encoded[0] & 0x400) == 0x400 ? _ellipsis : "unspecified";
            string locale = (_encoded[0] & 0x800) == 0x800 ? _locale.ToString() : "unspecified";
            return "ParagraphStyle(\n" +
                   $"textAlign: {textAlign}\n" +
                   $"textDirection: {textDirection}\n" +
                   $"fontWeight: {fontWeight}\n" +
                   $"fontStyle: {fontStyle}\n" +
                   $"maxLines: {maxLines}\n" +
                   $"textHeightBehavior: {textHeightBehavior}$\n" +
                   $"fontFamily: {fontFamily}\n" +
                   $"fontSize: {fontSize}\n" +
                   $"height: {height}\n" +
                   $"ellipsis: {ellipsis}\n" +
                   $"locale: {locale}\n" +
                   ")";
        }
    }

    public class StrutStyle {
        public StrutStyle(
            String fontFamily,
            List<String> fontFamilyFallback,
            float fontSize,
            float height,
            float leading,
            FontWeight fontWeight,
            FontStyle fontStyle,
            bool forceStrutHeight
        ) {
            _encoded = ui_._encodeStrut(
                fontFamily,
                fontFamilyFallback,
                fontSize,
                height,
                leading,
                fontWeight,
                fontStyle,
                forceStrutHeight
            );
            _fontFamily = fontFamily;
            _fontFamilyFallback = fontFamilyFallback;
        }

        public readonly List<byte> _encoded; // Most of the data for strut is encoded.
        public readonly String _fontFamily;
        public readonly List<String> _fontFamilyFallback;

        public override bool Equals(object obj) {
            if (ReferenceEquals(this, obj)) {
                return true;
            }

            return obj is StrutStyle other
                   && other._fontFamily == _fontFamily
                   && ui_._listEquals<String>(other._fontFamilyFallback, _fontFamilyFallback)
                   && ui_._listEquals<byte>(other._encoded, _encoded);
        }

        public override int GetHashCode() {
            int hashcode = _encoded.hashList();
            hashcode = (hashcode ^ 397) ^ _fontFamily.GetHashCode();
            return hashcode;
        }
    }

    public enum TextDirection {
        rtl,

        ltr,
    }

    public class TextBox {
        public TextBox(
            float left,
            float top,
            float right,
            float bottom,
            TextDirection direction) {
            this.left = left;
            this.top = top;
            this.right = right;
            this.bottom = bottom;
            this.direction = direction;
        }

        public static TextBox fromLTRBD(
            float left,
            float top,
            float right,
            float bottom,
            TextDirection direction) {
            return new TextBox(left, top, right, bottom, direction);
        }

        public readonly float left;

        public readonly float top;

        public readonly float right;

        public readonly float bottom;

        public readonly TextDirection direction;

        Rect toRect() => Rect.fromLTRB((float) left, (float) top, (float) right, (float) bottom);

        float start {
            get { return (direction == TextDirection.ltr) ? left : right; }
        }

        float end {
            get { return (direction == TextDirection.ltr) ? right : left; }
        }

        public override bool Equals(object other) {
            if (ReferenceEquals(this, other))
                return true;
            return other is TextBox textBox
                   && textBox.left == left
                   && textBox.top == top
                   && textBox.right == right
                   && textBox.bottom == bottom
                   && textBox.direction == direction;
        }

        public override int GetHashCode() {
            int hashcode = left.GetHashCode();
            hashcode = (hashcode ^ 397) ^ top.GetHashCode();
            hashcode = (hashcode ^ 397) ^ right.GetHashCode();
            hashcode = (hashcode ^ 397) ^ bottom.GetHashCode();
            hashcode = (hashcode ^ 397) ^ direction.GetHashCode();
            return hashcode;
        }

        public override string ToString() {
            return
                $"TextBox.fromLTRBD({left.ToString("N1")}, {top.ToString("N1")}, {right.ToString("N1")}, {bottom.ToString("N1")}, {direction})";
        }
    }

    public enum TextAffinity {
        upstream,

        downstream,
    }

    public class TextPosition {
        public TextPosition(
            int offset,
            TextAffinity affinity = TextAffinity.downstream) {
            this.offset = offset;
            this.affinity = affinity;
        }

        public readonly int offset;

        public readonly TextAffinity affinity;

        public override bool Equals(object obj) {
            return obj is TextPosition other
                   && other.offset == offset
                   && other.affinity == affinity;
        }

        public override int GetHashCode() {
            int hashcode = offset.GetHashCode();
            hashcode = (hashcode ^ 397) ^ affinity.GetHashCode();
            return hashcode;
        }

        public override string ToString() {
            return $"TextPosition(offset: {offset}, affinity: {affinity})";
        }
    }

    public class TextRange {
        public TextRange(
            int start, int end) {
            D.assert(start >= -1);
            D.assert(end >= -1);
            this.start = start;
            this.end = end;
        }

        public static TextRange collapsed(int offset) {
            D.assert(offset >= -1);
            return new TextRange(offset, offset);
        }

        static readonly TextRange empty = new TextRange(start: -1, end: -1);

        public readonly int start;

        public readonly int end;

        bool isValid {
            get { return start >= 0 && end >= 0; }
        }

        bool isCollapsed {
            get { return start == end; }
        }

        bool isNormalized {
            get { return end >= start; }
        }

        String textBefore(String text) {
            D.assert(isNormalized);
            return text.Substring(0, start);
        }

        String textAfter(String text) {
            D.assert(isNormalized);
            return text.Substring(end);
        }

        String textInside(String text) {
            D.assert(isNormalized);
            return text.Substring(start, end);
        }

        public override bool Equals(object obj) {
            if (ReferenceEquals(this, obj)) {
                return true;
            }

            return obj is TextRange other && other.start == start && other.end == end;
        }

        public override int GetHashCode() {
            return (start.GetHashCode() ^ 397) ^ end.GetHashCode();
        }

        public override string ToString() {
            return $"TextRange(start: {start}, end: {end})";
        }
    }

    public class ParagraphConstraints {
        public ParagraphConstraints(float width) {
            this.width = width;
        }

        public readonly float width;

        public override bool Equals(object obj) {
            return obj is ui.ParagraphConstraints other && other.width == width;
        }

        public override int GetHashCode() {
            return width.GetHashCode();
        }

        public override string ToString() {
            return $"ParagraphConstraints(width: {width})";
        }
    }

    public enum BoxHeightStyle {
        tight,

        max,

        includeLineSpacingMiddle,

        includeLineSpacingTop,

        includeLineSpacingBottom,

        strut,
    }

    public enum BoxWidthStyle {
        tight,

        max,
    }

    public enum PlaceholderAlignment {
        baseline,

        aboveBaseline,

        belowBaseline,

        top,

        bottom,

        middle,
    }

    public class LineMetrics {
        public LineMetrics(
            bool hardBreak,
            float ascent,
            float descent,
            float unscaledAscent,
            float height,
            float width,
            float left,
            float baseline,
            int lineNumber) {
            this.hardBreak = hardBreak;
            this.ascent = ascent;
            this.descent = descent;
            this.unscaledAscent = unscaledAscent;
            this.height = height;
            this.width = width;
            this.left = left;
            this.baseline = baseline;
            this.lineNumber = lineNumber;
        }

        public readonly bool hardBreak;

        public readonly float ascent;

        public readonly float descent;

        public readonly float unscaledAscent;

        public readonly float height;

        public readonly float width;

        public readonly float left;

        public readonly float baseline;

        public readonly int lineNumber;

        public override bool Equals(object obj) {
            return obj is LineMetrics other
                   && other.hardBreak == hardBreak
                   && other.ascent == ascent
                   && other.descent == descent
                   && other.unscaledAscent == unscaledAscent
                   && other.height == height
                   && other.width == width
                   && other.left == left
                   && other.baseline == baseline
                   && other.lineNumber == lineNumber;
        }

        public override int GetHashCode() {
            unchecked {
                var hashCode = hardBreak.GetHashCode();
                hashCode = (hashCode * 397) ^ ascent.GetHashCode();
                hashCode = (hashCode * 397) ^ descent.GetHashCode();
                hashCode = (hashCode * 397) ^ unscaledAscent.GetHashCode();
                hashCode = (hashCode * 397) ^ height.GetHashCode();
                hashCode = (hashCode * 397) ^ width.GetHashCode();
                hashCode = (hashCode * 397) ^ left.GetHashCode();
                hashCode = (hashCode * 397) ^ baseline.GetHashCode();
                hashCode = (hashCode * 397) ^ lineNumber;
                return hashCode;
            }
        }

        public override string ToString() {
            return $"LineMetrics(hardBreak: {hardBreak}, \n" +
                   $"ascent: {ascent}, " +
                   $"descent: {descent}, " +
                   $"unscaledAscent: {unscaledAscent}, " +
                   $"height: {height}, " +
                   $"width: {width}, " +
                   $"left: {left}, " +
                   $"baseline: {baseline}, " +
                   $"lineNumber: {lineNumber})";
        }
    }

    public class Paragraph : NativeWrapper {
        internal Paragraph(IntPtr ptr) {
            _ptr = ptr;
        }

        [DllImport(NativeBindings.dllName)]
        static extern float Paragraph_width(IntPtr ptr);

        [DllImport(NativeBindings.dllName)]
        static extern float Paragraph_height(IntPtr ptr);

        [DllImport(NativeBindings.dllName)]
        static extern float Paragraph_longestLine(IntPtr ptr);

        [DllImport(NativeBindings.dllName)]
        static extern float Paragraph_minIntrinsicWidth(IntPtr ptr);

        [DllImport(NativeBindings.dllName)]
        static extern float Paragraph_maxIntrinsicWidth(IntPtr ptr);

        [DllImport(NativeBindings.dllName)]
        static extern float Paragraph_alphabeticBaseline(IntPtr ptr);

        [DllImport(NativeBindings.dllName)]
        static extern float Paragraph_ideographicBaseline(IntPtr ptr);

        [DllImport(NativeBindings.dllName)]
        static extern bool Paragraph_didExceedMaxLines(IntPtr ptr);

        [DllImport(NativeBindings.dllName)]
        static extern void Paragraph_layout(IntPtr ptr, float width);

        [DllImport(NativeBindings.dllName)]
        static extern int Paragraph_getRectsForRangeSize(IntPtr ptr, int start, int end,
            int boxHeightStyle,
            int boxWidthStyle);

        [DllImport(NativeBindings.dllName)]
        static extern unsafe void Paragraph_getRectsForRange(IntPtr ptr, float* data, int start, int end,
            int boxHeightStyle,
            int boxWidthStyle);

        [DllImport(NativeBindings.dllName)]
        static extern unsafe int Paragraph_getRectsForPlaceholdersSize(IntPtr ptr);

        [DllImport(NativeBindings.dllName)]
        static extern unsafe void Paragraph_getRectsForPlaceholders(IntPtr ptr, float* data);

        [DllImport(NativeBindings.dllName)]
        static extern unsafe void Paragraph_getPositionForOffset(IntPtr ptr, float dx, float dy, int* encodedPtr);

        [DllImport(NativeBindings.dllName)]
        static extern unsafe void Paragraph_getWordBoundary(IntPtr ptr, int offset, int* boundaryPtr);

        [DllImport(NativeBindings.dllName)]
        static extern unsafe void Paragraph_getLineBoundary(IntPtr ptr, int offset, int* boundaryPtr);

        [DllImport(NativeBindings.dllName)]
        static extern void Paragraph_paint(IntPtr ptr, IntPtr canvas, float x, float y);

        [DllImport(NativeBindings.dllName)]
        static extern int Paragraph_computeLineMetricsSize(IntPtr ptr);

        [DllImport(NativeBindings.dllName)]
        static extern unsafe void Paragraph_computeLineMetrics(IntPtr ptr, float* data);

        [DllImport(NativeBindings.dllName)]
        static extern void Paragraph_dispose(IntPtr ptr);

        public void layout(ParagraphConstraints constraints) => _layout(constraints.width);
        void _layout(float width) => Paragraph_layout(_ptr, width);

        List<TextBox> _decodeTextBoxes(float[] encoded, int size) {
            int count = size / 5;
            List<TextBox> boxes = new List<TextBox>();
            int position = 0;
            for (int index = 0; index < count; index += 1) {
                unsafe {
                    boxes.Add(TextBox.fromLTRBD(
                        encoded[position++],
                        encoded[position++],
                        encoded[position++],
                        encoded[position++],
                        (TextDirection) encoded[position++]
                    ));
                }
            }

            return boxes;
        }

        protected override void DisposePtr(IntPtr ptr) {
            Paragraph_dispose(ptr);
        }

        public float width() {
            return Paragraph_width(_ptr);
        }

        public float height() {
            return Paragraph_height(_ptr);
        }

        public float longestLine() {
            return Paragraph_longestLine(_ptr);
        }

        public float minIntrinsicWidth() {
            return Paragraph_minIntrinsicWidth(_ptr);
        }

        public float maxIntrinsicWidth() {
            return Paragraph_maxIntrinsicWidth(_ptr);
        }

        public float alphabeticBaseline() {
            return Paragraph_alphabeticBaseline(_ptr);
        }

        public float ideographicBaseline() {
            return Paragraph_ideographicBaseline(_ptr);
        }

        public bool didExceedMaxLines() {
            return Paragraph_didExceedMaxLines(_ptr);
        }

        public unsafe List<TextBox> getBoxesForRange(int start, int end,
            BoxHeightStyle boxHeightStyle = BoxHeightStyle.tight,
            BoxWidthStyle boxWidthStyle = BoxWidthStyle.tight) {
            int size = Paragraph_getRectsForRangeSize(_ptr, start, end, (int) boxHeightStyle, (int) boxWidthStyle);
            float[] data = new float[size];
            fixed (float* dataPtr = data) {
                _getBoxesForRange(dataPtr, start, end, (int) boxHeightStyle, (int) boxWidthStyle);
            }

            return _decodeTextBoxes(data, size);
        }

        // See paragraph.cc for the layout of this return value.
        unsafe void _getBoxesForRange(float* data, int start, int end, int boxHeightStyle, int boxWidthStyle) =>
            Paragraph_getRectsForRange(_ptr, data, start, end, boxHeightStyle, boxWidthStyle);

        public unsafe List<TextBox> getBoxesForPlaceholders() {
            int size = Paragraph_getRectsForPlaceholdersSize(_ptr);
            float[] data = new float[size];
            fixed (float* dataPtr = data) {
                _getBoxesForPlaceholders(dataPtr);
            }

            return _decodeTextBoxes(data, size);
        }

        unsafe void _getBoxesForPlaceholders(float* data) => Paragraph_getRectsForPlaceholders(_ptr, data);

        public unsafe TextPosition getPositionForOffset(Offset offset) {
            int[] encoded = new int[2];
            fixed (int* encodedPtr = encoded) {
                _getPositionForOffset(offset.dx, offset.dy, encodedPtr);
            }

            return new TextPosition(offset: encoded[0], affinity: (TextAffinity) encoded[1]);
        }

        unsafe void _getPositionForOffset(float dx, float dy, int* encodedPtr) =>
            Paragraph_getPositionForOffset(_ptr, dx, dy, encodedPtr);

        public unsafe TextRange getWordBoundary(TextPosition position) {
            int[] boundary = new int[2];
            fixed (int* boundaryPtr = boundary) {
                _getWordBoundary(position.offset, boundaryPtr);
            }

            return new TextRange(start: boundary[0], end: boundary[1]);
        }

        unsafe void _getWordBoundary(int offset, int* boundaryPtr) =>
            Paragraph_getWordBoundary(_ptr, offset, boundaryPtr);

        public unsafe TextRange getLineBoundary(TextPosition position) {
            int[] boundary = new int[2];
            fixed (int* boundaryPtr = boundary) {
                _getLineBoundary(position.offset, boundaryPtr);
            }

            return new TextRange(start: boundary[0], end: boundary[1]);
        }

        unsafe void _getLineBoundary(int offset, int* boundaryPtr) =>
            Paragraph_getLineBoundary(_ptr, offset, boundaryPtr);

        public void _paint(Canvas canvas, float x, float y) {
            Paragraph_paint(_ptr, canvas._ptr, x, y);
        }

        public unsafe List<LineMetrics> computeLineMetrics() {
            int size = Paragraph_computeLineMetricsSize(_ptr);
            float[] data = new float[size];
            fixed (float* dataPtr = data) {
                _computeLineMetrics(dataPtr);
            }

            int count = size / 9;
            int position = 0;
            List<LineMetrics> metrics = new List<LineMetrics>();

            for (int index = 0; index < count; index += 1) {
                unsafe {
                    metrics.Add(new LineMetrics(
                        hardBreak: data[position++] != 0,
                        ascent: data[position++],
                        descent: data[position++],
                        unscaledAscent: data[position++],
                        height: data[position++],
                        width: data[position++],
                        left: data[position++],
                        baseline: data[position++],
                        lineNumber: (int) data[position++]
                    ));
                }
            }

            return metrics;
        }

        unsafe void _computeLineMetrics(float* data) => Paragraph_computeLineMetrics(_ptr, data);
    }

    public class ParagraphBuilder : NativeWrapper {
        [DllImport(NativeBindings.dllName, CallingConvention = CallingConvention.Cdecl)]
        static extern IntPtr ParagraphBuilder_constructor(
            int[] encoded,
            int encodeSize,
            byte[] strutData,
            int structDataSize,
            string fontFamily,
            [In] string[] structFontFamily,
            int structFontFamilySize,
            float fontSize,
            float height,
            [MarshalAs(UnmanagedType.LPWStr)] string ellipsis,
            string locale);

        [DllImport(NativeBindings.dllName, CallingConvention = CallingConvention.Cdecl)]
        static unsafe extern void ParagraphBuilder_pushStyle(
            IntPtr ptr,
            int[] encoded,
            int encodedSize,
            [In] string[] fontFamilies,
            int fontFamiliesSize,
            float fontSize,
            float letterSpacing,
            float wordSpacing,
            float height,
            float decorationThickness,
            string locale,
            IntPtr* backgroundObjects,
            byte[] backgroundData,
            IntPtr* foregroundObjects,
            byte[] foregroundData,
            byte[] shadowsData,
            int shadowDataSize,
            byte[] fontFeaturesData,
            int fontFeatureDataSize
        );

        [DllImport(NativeBindings.dllName)]
        static extern void ParagraphBuilder_pop(IntPtr ptr);

        [DllImport(NativeBindings.dllName, CallingConvention = CallingConvention.Cdecl)]
        static extern IntPtr ParagraphBuilder_addText(IntPtr ptr, [MarshalAs(UnmanagedType.LPWStr)] string text);

        [DllImport(NativeBindings.dllName)]
        static extern IntPtr ParagraphBuilder_addPlaceholder(IntPtr ptr, float width, float height, int alignment,
            float baselineOffset, int baseline);

        [DllImport(NativeBindings.dllName)]
        static extern IntPtr ParagraphBuilder_build(IntPtr ptr);

        [DllImport(NativeBindings.dllName)]
        static extern void ParagraphBuilder_dispose(IntPtr ptr);

        public ParagraphBuilder(ParagraphStyle style) {
            List<String> strutFontFamilies = null;
            StrutStyle strutStyle = style._strutStyle;
            if (strutStyle != null) {
                strutFontFamilies = new List<String>();
                String fontFamily = strutStyle._fontFamily;
                if (fontFamily != null)
                    strutFontFamilies.Add(fontFamily);
                if (strutStyle._fontFamilyFallback != null)
                    strutFontFamilies.AddRange(strutStyle._fontFamilyFallback);
            }

            _ptr = _constructor(
                style._encoded.ToArray(),
                strutStyle?._encoded.ToArray(),
                style._fontFamily,
                strutFontFamilies?.ToArray(),
                style._fontSize ?? 0,
                style._height ?? 0,
                style._ellipsis,
                _encodeLocale(style._locale)
            );
        }

        IntPtr _constructor(
            int[] encoded,
            byte[] structData,
            string fontFamily,
            string[] structFontFamily,
            float fontSize,
            float height,
            string ellipsis,
            string locale
        ) => ParagraphBuilder_constructor(
            encoded,
            encoded == null ? 0 : encoded.Length,
            structData,
            structData == null ? 0 : structData.Length,
            fontFamily,
            structFontFamily,
            structFontFamily == null ? 0 : structFontFamily.Length,
            fontSize,
            height,
            ellipsis,
            locale
        );

        protected override void DisposePtr(IntPtr ptr) {
            ParagraphBuilder_dispose(ptr);
        }

        public int placeholderCount {
            get { return _placeholderCount; }
        }

        int _placeholderCount = 0;

        public List<float> placeholderScales {
            get { return _placeholderScales; }
        }

        List<float> _placeholderScales = new List<float>();

        public void pushStyle(TextStyle style) {
            List<String> fullFontFamilies = new List<string>();

            fullFontFamilies.Add(style._fontFamily);
            if (style._fontFamilyFallback != null)
                fullFontFamilies.AddRange(style._fontFamilyFallback);

            byte[] encodedFontFeatures = null;
            List<FontFeature> fontFeatures = style._fontFeatures;
            if (fontFeatures != null) {
                encodedFontFeatures = new byte[fontFeatures.Count * FontFeature._kEncodedSize];
                int byteOffset = 0;
                foreach (FontFeature feature in fontFeatures) {
                    var byteFeature = new byte[FontFeature._kEncodedSize - byteOffset];
                    Array.Copy(encodedFontFeatures, 0, byteFeature, 0, FontFeature._kEncodedSize);
                    feature._encode(byteFeature);
                    byteOffset += FontFeature._kEncodedSize;
                }
            }

            _pushStyle(
                style._encoded.ToArray(),
                fullFontFamilies.ToArray(),
                style._fontSize,
                style._letterSpacing,
                style._wordSpacing,
                style._height,
                style._decorationThickness,
                _encodeLocale(style._locale),
                style._background?._objectPtrs,
                style._background?._data,
                style._foreground?._objectPtrs,
                style._foreground?._data,
                Shadow._encodeShadows(style._shadows),
                encodedFontFeatures
            );
        }

        unsafe void _pushStyle(
            int[] encoded,
            string[] fontFamilies,
            float? fontSize,
            float? letterSpacing,
            float? wordSpacing,
            float? height,
            float? decorationThickness,
            string locale,
            IntPtr[] backgroundObjects,
            byte[] backgroundData,
            IntPtr[] foregroundObjects,
            byte[] foregroundData,
            byte[] shadowsData,
            byte[] fontFeaturesData
        ) {
            fixed (IntPtr* backgroundObjectsPtr = backgroundObjects)
            fixed (IntPtr* foregroundObjectsPtr = foregroundObjects)
                ParagraphBuilder_pushStyle(_ptr,
                    encoded,
                    encoded.Length,
                    fontFamilies,
                    fontFamilies.Length,
                    fontSize.GetValueOrDefault(0),
                    letterSpacing.GetValueOrDefault(0),
                    wordSpacing.GetValueOrDefault(0),
                    height.GetValueOrDefault(0),
                    decorationThickness.GetValueOrDefault(0),
                    locale,
                    backgroundObjectsPtr,
                    backgroundData,
                    foregroundObjectsPtr,
                    foregroundData,
                    shadowsData,
                    shadowsData?.Length ?? 0,
                    fontFeaturesData,
                    fontFeaturesData?.Length ?? 0
                );
        }

        static string _encodeLocale(Locale locale) => locale?.ToString() ?? "";

        public void pop() => ParagraphBuilder_pop(_ptr);

        public void addText(String text) {
            IntPtr error = _addText(text);
            if (error != IntPtr.Zero) {
                throw new Exception(Marshal.PtrToStringAnsi(error));
            }
        }

        IntPtr _addText(String text) => ParagraphBuilder_addText(_ptr, text);

        public void addPlaceholder(float width, float height, PlaceholderAlignment alignment,
            TextBaseline baseline,
            float scale = 1.0f,
            float? baselineOffset = null
        ) {
            // Require a baseline to be specified if using a baseline-based alignment.
            D.assert((alignment == PlaceholderAlignment.aboveBaseline ||
                      alignment == PlaceholderAlignment.belowBaseline ||
                      alignment == PlaceholderAlignment.baseline)
                ? baseline != null
                : true);
            // Default the baselineOffset to height if null. This will place the placeholder
            // fully above the baseline, similar to [PlaceholderAlignment.aboveBaseline].
            float baselineOffsetFloat = baselineOffset ?? height;
            _addPlaceholder(width * scale, height * scale, (int) alignment, baselineOffsetFloat * scale,
                (int) baseline);
            _placeholderCount++;
            _placeholderScales.Add(scale);
        }

        IntPtr _addPlaceholder(float width, float height, int alignment, float baselineOffset,
            int baseline) => ParagraphBuilder_addPlaceholder(_ptr, width, height, alignment, baselineOffset, baseline);

        public Paragraph build() {
            Paragraph paragraph = new Paragraph(_build());
            return paragraph;
        }

        IntPtr _build() => ParagraphBuilder_build(_ptr);
    }
}