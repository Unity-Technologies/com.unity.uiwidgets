using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using AOT;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.ui;
using UnityEngine;
using RSG;
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
            return map[this.index];
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
                foreach (var charUnit in this.feature.ToCharArray()) {
                    if (charUnit <= 0x20 && charUnit >= 0x7F) {
                        return false;
                    }
                }

                return true;
            });
            for (int i = 0; i < 4; i++) {
                byteData[i] = (byte) this.feature[i];
            }

            byteData[4] = (byte) (this.value >> 0);
            byteData[5] = (byte) (this.value >> 8);
            byteData[6] = (byte) (this.value >> 16);
            byteData[7] = (byte) (this.value >> 24);
        }


        public override bool Equals(object obj) {
            if (ReferenceEquals(this, obj)) {
                return true;
            }

            return obj is FontFeature fontFeature && fontFeature.feature == this.feature &&
                   fontFeature.value == this.value;
        }

        public override int GetHashCode() {
            int hashcode = this.value.GetHashCode();
            hashcode = (hashcode ^ 397) ^ this.feature.GetHashCode();
            return hashcode;
        }

        public override string ToString() {
            return $"FontFeature({this.feature}, {this.value})";
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
            return (this._mask | other._mask) == this._mask;
        }

        public static readonly TextDecoration none = new TextDecoration(0x0);

        public static readonly TextDecoration underline = new TextDecoration(0x1);

        public static readonly TextDecoration overline = new TextDecoration(0x2);

        public static readonly TextDecoration lineThrough = new TextDecoration(0x4);

        public override bool Equals(object obj) {
            return obj is TextDecoration textDecoration
                   && textDecoration._mask == this._mask;
        }

        public override int GetHashCode() {
            return this._mask.GetHashCode();
        }


        public override string ToString() {
            if (this._mask == 0) {
                return "TextDecoration.none";
            }

            List<String> values = new List<string>();
            if ((this._mask & TextDecoration.underline._mask) != 0)
                values.Add("underline");
            if ((this._mask & TextDecoration.overline._mask) != 0)
                values.Add("overline");
            if ((this._mask & TextDecoration.lineThrough._mask) != 0)
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
            return (this.applyHeightToFirstAscent ? 0 : 1 << 0) | (this.applyHeightToLastDescent ? 0 : 1 << 1);
        }

        public override bool Equals(object other) {
            return other is TextHeightBehavior otherText
                   && otherText.applyHeightToFirstAscent == this.applyHeightToFirstAscent
                   && otherText.applyHeightToLastDescent == this.applyHeightToLastDescent;
        }


        public override int GetHashCode() {
            int hashcode = this.applyHeightToFirstAscent.GetHashCode();
            hashcode = (hashcode ^ 397) ^ this.applyHeightToLastDescent.GetHashCode();
            return hashcode;
        }

        public override string ToString() {
            return "TextHeightBehavior(\n" +
                   " applyHeightToFirstAscent: $applyHeightToFirstAscent, \n" +
                   " applyHeightToLastDescent: $applyHeightToLastDescent \n" +
                   ")";
        }
    }

    public class Utils {
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
            double? decorationThickness,
            FontWeight fontWeight,
            FontStyle? fontStyle,
            TextBaseline? textBaseline,
            String fontFamily,
            List<String> fontFamilyFallback,
            double? fontSize,
            double? letterSpacing,
            double? wordSpacing,
            double? height,
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
            double? fontSize,
            double? height,
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
            double fontSize,
            double height,
            double leading,
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

        public Promise loadFontFromList(int[] list, string fontFamily = null) {
            var completer = new Promise(true);
            GCHandle completerHandle = GCHandle.Alloc(completer);
            loadFontFromList(list, _toImageCallback, (IntPtr) completerHandle, fontFamily);
            return completer;
        }

        [MonoPInvokeCallback(typeof(_loadFontFromListCallback))]
        static void _toImageCallback(IntPtr callbackHandle) {
            GCHandle completerHandle = (GCHandle) callbackHandle;
            var completer = (Promise) completerHandle.Target;
            completerHandle.Free();
            _sendFontChangeMessage();
        }

        static byte[] _fontChangeMessage =
            Encoding.ASCII.GetBytes(JsonUtility.ToJson(new Dictionary<string, dynamic>() {{"type", "fontsChange"}}));

        public static async void _sendFontChangeMessage() {
            unsafe {
                fixed (byte* data = _fontChangeMessage) {
                    Window window = new Window();
                    window.onPlatformMessage?.Invoke("flutter/system", data, _fontChangeMessage.Length,
                        (byte* data, int dataLength) => { });
                }
            }
        }

        delegate void _loadFontFromListCallback(IntPtr callbackHandle);

        [DllImport(NativeBindings.dllName)]
        static extern IntPtr loadFontFromList(int[] list, _loadFontFromListCallback callback, IntPtr callbackHandle,
            string fontFamily);

        public struct Float32List {
            public unsafe float* data;
            public int Length;
        }
    }

    public class TextStyle {
        public TextStyle(
            Color color,
            TextDecoration decoration = null,
            Color decorationColor = null,
            TextDecorationStyle? decorationStyle = null,
            double? decorationThickness = null,
            FontWeight fontWeight = null,
            FontStyle? fontStyle = null,
            TextBaseline? textBaseline = null,
            String fontFamily = null,
            List<String> fontFamilyFallback = null,
            double? fontSize = null,
            double? letterSpacing = null,
            double? wordSpacing = null,
            double? height = null,
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
            this._encoded = Utils._encodeTextStyle(
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

            this._fontFamily = fontFamily ?? "";
            this._fontFamilyFallback = fontFamilyFallback;
            this._fontSize = fontSize;
            this._letterSpacing = letterSpacing;
            this._wordSpacing = wordSpacing;
            this._height = height;
            this._decorationThickness = decorationThickness;
            this._locale = locale;
            this._background = background;
            this._foreground = foreground;
            this._shadows = shadows;
            this._fontFeatures = fontFeatures;
        }

        public readonly List<int> _encoded;
        public readonly String _fontFamily;
        public readonly List<String> _fontFamilyFallback;
        public readonly double? _fontSize;
        public readonly double? _letterSpacing;
        public readonly double? _wordSpacing;
        public readonly double? _height;
        public readonly double? _decorationThickness;
        public readonly Locale _locale;
        public readonly Paint _background;
        public readonly Paint _foreground;
        public readonly List<Shadow> _shadows;
        public readonly List<FontFeature> _fontFeatures;

        public override bool Equals(object obj) {
            if (ReferenceEquals(this, obj))
                return true;
            return obj is TextStyle textStyle
                   && textStyle._fontFamily == this._fontFamily
                   && textStyle._fontSize == this._fontSize
                   && textStyle._letterSpacing == this._letterSpacing
                   && textStyle._wordSpacing == this._wordSpacing
                   && textStyle._height == this._height
                   && textStyle._decorationThickness == this._decorationThickness
                   && textStyle._locale == this._locale
                   && textStyle._background == this._background
                   && textStyle._foreground == this._foreground
                   && Utils._listEquals<int>(textStyle._encoded, this._encoded)
                   && Utils._listEquals<Shadow>(textStyle._shadows, this._shadows)
                   && Utils._listEquals<String>(textStyle._fontFamilyFallback, this._fontFamilyFallback)
                   && Utils._listEquals<FontFeature>(textStyle._fontFeatures, this._fontFeatures);
        }

        public override int GetHashCode() {
            int hashcode = this._encoded.hashList();
            hashcode = (hashcode ^ 397) ^ (this._fontFamily.GetHashCode());

            hashcode = (hashcode ^ 397) ^ this._fontFamilyFallback.hashList();
            hashcode = (hashcode ^ 397) ^ this._fontSize.GetHashCode();
            hashcode = (hashcode ^ 397) ^ this._letterSpacing.GetHashCode();
            hashcode = (hashcode ^ 397) ^ this._wordSpacing.GetHashCode();
            hashcode = (hashcode ^ 397) ^ this._height.GetHashCode();
            hashcode = (hashcode ^ 397) ^ this._locale.GetHashCode();
            hashcode = (hashcode ^ 397) ^ this._background.GetHashCode();
            hashcode = (hashcode ^ 397) ^ this._foreground.GetHashCode();
            hashcode = (hashcode ^ 397) ^ this._shadows.hashList();
            hashcode = (hashcode ^ 397) ^ this._decorationThickness.GetHashCode();
            hashcode = (hashcode ^ 397) ^ this._fontFeatures.hashList();
            return hashcode;
        }

        public override string ToString() {
            string color = (this._encoded[0] & 0x00002) == 0x00002
                ? (new Color((uint) this._encoded[1])).ToString()
                : "unspecified";
            string decoration = (this._encoded[0] & 0x00004) == 0x00004
                ? (new TextDecoration(this._encoded[2])).ToString()
                : "unspecified";
            string decorationColor = (this._encoded[0] & 0x00008) == 0x00008
                ? (new Color((uint) this._encoded[3])).ToString()
                : "unspecified";
            string decorationStyle = (this._encoded[0] & 0x00010) == 0x00010
                ? ((TextDecorationStyle) this._encoded[4]).ToString()
                : "unspecified";
            // The decorationThickness is not in encoded order in order to keep it near the other decoration properties.
            string decorationThickness = (this._encoded[0] & 0x00100) == 0x00100
                ? this._decorationThickness.ToString()
                : "unspecified";
            string fontWeight = (this._encoded[0] & 0x00020) == 0x00020
                ? FontWeight.values[this._encoded[5]].ToString()
                : "unspecified";
            string fontStyle = (this._encoded[0] & 0x00040) == 0x00040
                ? ((FontStyle) this._encoded[6]).ToString()
                : "unspecified";
            string textBaseline = (this._encoded[0] & 0x00080) == 0x00080
                ? ((TextBaseline) this._encoded[7]).ToString()
                : "unspecified";
            string fontFamily = (this._encoded[0] & 0x00200) == 0x00200
                                && this._fontFamily != null
                ? this._fontFamily
                : "unspecified";
            string fontFamilyFallback = (this._encoded[0] & 0x00200) == 0x00200
                                        && this._fontFamilyFallback != null
                                        && this._fontFamilyFallback.isNotEmpty()
                ? this._fontFamilyFallback.ToString()
                : "unspecified";
            string fontSize = (this._encoded[0] & 0x00400) == 0x00400 ? this._fontSize.ToString() : "unspecified";
            string letterSpacing = (this._encoded[0] & 0x00800) == 0x00800 ? "_letterSpacing}x" : "unspecified";
            string wordSpacing = (this._encoded[0] & 0x01000) == 0x01000 ? "_wordSpacing}x" : "unspecified";
            string height = (this._encoded[0] & 0x02000) == 0x02000 ? "_height}x" : "unspecified";
            string locale = (this._encoded[0] & 0x04000) == 0x04000 ? this._locale.ToString() : "unspecified";
            string background = (this._encoded[0] & 0x08000) == 0x08000 ? this._locale.ToString() : "unspecified";
            string foreground = (this._encoded[0] & 0x10000) == 0x10000 ? this._locale.ToString() : "unspecified";
            string shadows = (this._encoded[0] & 0x20000) == 0x20000 ? this._locale.ToString() : "unspecified";

            string fontFeatures =
                (this._encoded[0] & 0x40000) == 0x40000 ? this._fontFeatures.ToString() : "unspecified";

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
            double? fontSize = null,
            double? height = null,
            TextHeightBehavior textHeightBehavior = null,
            FontWeight fontWeight = null,
            FontStyle? fontStyle = null,
            StrutStyle strutStyle = null,
            string ellipsis = null,
            Locale locale = null
        ) {
            this._encoded = Utils._encodeParagraphStyle(
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
            this._fontFamily = fontFamily;
            this._fontSize = fontSize;
            this._height = height;
            this._strutStyle = strutStyle;
            this._ellipsis = ellipsis;
            this._locale = locale;
        }

        public readonly List<int> _encoded;
        public readonly String _fontFamily;
        public readonly double? _fontSize;
        public readonly double? _height;
        public readonly StrutStyle _strutStyle;
        public readonly String _ellipsis;
        public readonly Locale _locale;

        public override bool Equals(object other) {
            if (ReferenceEquals(this, other))
                return true;
            return other is ParagraphStyle otherParagraph
                   && otherParagraph._fontFamily == this._fontFamily
                   && otherParagraph._fontSize == this._fontSize
                   && otherParagraph._height == this._height
                   && otherParagraph._strutStyle == this._strutStyle
                   && otherParagraph._ellipsis == this._ellipsis
                   && otherParagraph._locale == this._locale
                   && Utils._listEquals<int>(otherParagraph._encoded, this._encoded);
        }

        public override int GetHashCode() {
            int hashcode = this._encoded.hashList();
            hashcode = (hashcode ^ 397) ^ this._fontFamily.GetHashCode();
            hashcode = (hashcode ^ 397) ^ this._fontSize.GetHashCode();
            hashcode = (hashcode ^ 397) ^ this._height.GetHashCode();
            hashcode = (hashcode ^ 397) ^ this._ellipsis.GetHashCode();
            hashcode = (hashcode ^ 397) ^ this._locale.GetHashCode();

            return hashcode;
        }

        public override string ToString() {
            string textAlign = (this._encoded[0] & 0x002) == 0x002
                ? ((TextAlign) this._encoded[1]).ToString()
                : "unspecified";
            string textDirection = (this._encoded[0] & 0x004) == 0x004
                ? ((TextDirection) this._encoded[2]).ToString()
                : "unspecified";
            string fontWeight = (this._encoded[0] & 0x008) == 0x008
                ? FontWeight.values[this._encoded[3]].ToString()
                : "unspecified";
            string fontStyle = (this._encoded[0] & 0x010) == 0x010
                ? ((FontStyle) this._encoded[4]).ToString()
                : "unspecified";
            string maxLines = (this._encoded[0] & 0x020) == 0x020 ? this._encoded[5].ToString() : "unspecified";
            string textHeightBehavior = (this._encoded[0] & 0x040) == 0x040
                ? TextHeightBehavior.fromEncoded(this._encoded[6]).ToString()
                : "unspecified";
            string fontFamily = (this._encoded[0] & 0x080) == 0x080 ? this._fontFamily : "unspecified";
            string fontSize = (this._encoded[0] & 0x100) == 0x100 ? this._fontSize.ToString() : "unspecified";
            string height = (this._encoded[0] & 0x200) == 0x200 ? $"{this._height}x" : "unspecified";
            string ellipsis = (this._encoded[0] & 0x400) == 0x400 ? this._ellipsis : "unspecified";
            string locale = (this._encoded[0] & 0x800) == 0x800 ? this._locale.ToString() : "unspecified";
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
            double fontSize,
            double height,
            double leading,
            FontWeight fontWeight,
            FontStyle fontStyle,
            bool forceStrutHeight
        ) {
            this._encoded = Utils._encodeStrut(
                fontFamily,
                fontFamilyFallback,
                fontSize,
                height,
                leading,
                fontWeight,
                fontStyle,
                forceStrutHeight
            );
            this._fontFamily = fontFamily;
            this._fontFamilyFallback = fontFamilyFallback;
        }

        public readonly List<byte> _encoded; // Most of the data for strut is encoded.
        public readonly String _fontFamily;
        public readonly List<String> _fontFamilyFallback;

        public override bool Equals(object obj) {
            if (ReferenceEquals(this, obj)) {
                return true;
            }

            return obj is StrutStyle other
                   && other._fontFamily == this._fontFamily
                   && Utils._listEquals<String>(other._fontFamilyFallback, this._fontFamilyFallback)
                   && Utils._listEquals<byte>(other._encoded, this._encoded);
        }

        public override int GetHashCode() {
            int hashcode = this._encoded.hashList();
            hashcode = (hashcode ^ 397) ^ this._fontFamily.GetHashCode();
            return hashcode;
        }
    }

    public enum TextDirection {
        rtl,

        ltr,
    }

    public class TextBox {
        public TextBox(
            double left,
            double top,
            double right,
            double bottom,
            TextDirection direction) {
            this.left = left;
            this.top = top;
            this.right = right;
            this.bottom = bottom;
            this.direction = direction;
        }

        public static TextBox fromLTRBD(
            double left,
            double top,
            double right,
            double bottom,
            TextDirection direction) {
            return new TextBox(left, top, right, bottom, direction);
        }

        public readonly double left;

        public readonly double top;

        public readonly double right;

        public readonly double bottom;

        public readonly TextDirection direction;

        Rect toRect() => Rect.fromLTRB((float) this.left, (float) this.top, (float) this.right, (float) this.bottom);

        double start {
            get { return (this.direction == TextDirection.ltr) ? this.left : this.right; }
        }

        double end {
            get { return (this.direction == TextDirection.ltr) ? this.right : this.left; }
        }

        public override bool Equals(object other) {
            if (ReferenceEquals(this, other))
                return true;
            return other is TextBox textBox
                   && textBox.left == this.left
                   && textBox.top == this.top
                   && textBox.right == this.right
                   && textBox.bottom == this.bottom
                   && textBox.direction == this.direction;
        }

        public override int GetHashCode() {
            int hashcode = this.left.GetHashCode();
            hashcode = (hashcode ^ 397) ^ this.top.GetHashCode();
            hashcode = (hashcode ^ 397) ^ this.right.GetHashCode();
            hashcode = (hashcode ^ 397) ^ this.bottom.GetHashCode();
            hashcode = (hashcode ^ 397) ^ this.direction.GetHashCode();
            return hashcode;
        }

        public override string ToString() {
            return
                $"TextBox.fromLTRBD({this.left.ToString("N1")}, {this.top.ToString("N1")}, {this.right.ToString("N1")}, {this.bottom.ToString("N1")}, {this.direction})";
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
                   && other.offset == this.offset
                   && other.affinity == this.affinity;
        }

        public override int GetHashCode() {
            int hashcode = this.offset.GetHashCode();
            hashcode = (hashcode ^ 397) ^ this.affinity.GetHashCode();
            return hashcode;
        }

        public override string ToString() {
            return $"TextPosition(offset: {this.offset}, affinity: {this.affinity})";
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
            get { return this.start >= 0 && this.end >= 0; }
        }

        bool isCollapsed {
            get { return this.start == this.end; }
        }

        bool isNormalized {
            get { return this.end >= this.start; }
        }

        String textBefore(String text) {
            D.assert(this.isNormalized);
            return text.Substring(0, this.start);
        }

        String textAfter(String text) {
            D.assert(this.isNormalized);
            return text.Substring(this.end);
        }

        String textInside(String text) {
            D.assert(this.isNormalized);
            return text.Substring(this.start, this.end);
        }

        public override bool Equals(object obj) {
            if (ReferenceEquals(this, obj)) {
                return true;
            }

            return obj is TextRange other && other.start == this.start && other.end == this.end;
        }

        public override int GetHashCode() {
            return (this.start.GetHashCode() ^ 397) ^ this.end.GetHashCode();
        }

        public override string ToString() {
            return $"TextRange(start: {this.start}, end: {this.end})";
        }
    }

    public class ParagraphConstraints {
        public ParagraphConstraints(double width) {
            this.width = width;
        }

        public readonly double width;

        public override bool Equals(object obj) {
            return obj is ui.ParagraphConstraints other && other.width == this.width;
        }

        public override int GetHashCode() {
            return this.width.GetHashCode();
        }

        public override string ToString() {
            return $"ParagraphConstraints(width: {this.width})";
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
            double ascent,
            double descent,
            double unscaledAscent,
            double height,
            double width,
            double left,
            double baseline,
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

        public readonly double ascent;

        public readonly double descent;

        public readonly double unscaledAscent;

        public readonly double height;

        public readonly double width;

        public readonly double left;

        public readonly double baseline;

        public readonly int lineNumber;

        public override bool Equals(object obj) {
            return obj is LineMetrics other
                   && other.hardBreak == this.hardBreak
                   && other.ascent == this.ascent
                   && other.descent == this.descent
                   && other.unscaledAscent == this.unscaledAscent
                   && other.height == this.height
                   && other.width == this.width
                   && other.left == this.left
                   && other.baseline == this.baseline
                   && other.lineNumber == this.lineNumber;
        }

        public override int GetHashCode() {
            unchecked {
                var hashCode = this.hardBreak.GetHashCode();
                hashCode = (hashCode * 397) ^ this.ascent.GetHashCode();
                hashCode = (hashCode * 397) ^ this.descent.GetHashCode();
                hashCode = (hashCode * 397) ^ this.unscaledAscent.GetHashCode();
                hashCode = (hashCode * 397) ^ this.height.GetHashCode();
                hashCode = (hashCode * 397) ^ this.width.GetHashCode();
                hashCode = (hashCode * 397) ^ this.left.GetHashCode();
                hashCode = (hashCode * 397) ^ this.baseline.GetHashCode();
                hashCode = (hashCode * 397) ^ this.lineNumber;
                return hashCode;
            }
        }

        public override string ToString() {
            return $"LineMetrics(hardBreak: {this.hardBreak}, \n" +
                   $"ascent: {this.ascent}, " +
                   $"descent: {this.descent}, " +
                   $"unscaledAscent: {this.unscaledAscent}, " +
                   $"height: {this.height}, " +
                   $"width: {this.width}, " +
                   $"left: {this.left}, " +
                   $"baseline: {this.baseline}, " +
                   $"lineNumber: {this.lineNumber})";
        }
    }

    public class Paragraph : NativeWrapper {
        internal Paragraph(IntPtr ptr) {
            this._ptr = ptr;
        }

        [DllImport(NativeBindings.dllName)]
        static extern double Paragraph_width(IntPtr ptr);

        [DllImport(NativeBindings.dllName)]
        static extern double Paragraph_height(IntPtr ptr);

        [DllImport(NativeBindings.dllName)]
        static extern double Paragraph_longestLine(IntPtr ptr);

        [DllImport(NativeBindings.dllName)]
        static extern double Paragraph_minIntrinsicWidth(IntPtr ptr);

        [DllImport(NativeBindings.dllName)]
        static extern double Paragraph_maxIntrinsicWidth(IntPtr ptr);

        [DllImport(NativeBindings.dllName)]
        static extern double Paragraph_alphabeticBaseline(IntPtr ptr);

        [DllImport(NativeBindings.dllName)]
        static extern double Paragraph_ideographicBaseline(IntPtr ptr);

        [DllImport(NativeBindings.dllName)]
        static extern bool Paragraph_didExceedMaxLines(IntPtr ptr);

        [DllImport(NativeBindings.dllName)]
        static extern void Paragraph_layout(IntPtr ptr, double width);

        [DllImport(NativeBindings.dllName)]
        static extern Utils.Float32List Paragraph_getRectsForRange(IntPtr ptr, int start, int end, int boxHeightStyle,
            int boxWidthStyle);

        [DllImport(NativeBindings.dllName)]
        static extern Utils.Float32List Paragraph_getRectsForPlaceholders(IntPtr ptr);

        [DllImport(NativeBindings.dllName)]
        static extern List<int> Paragraph_getPositionForOffset(IntPtr ptr, double dx, double dy);

        [DllImport(NativeBindings.dllName)]
        static extern List<int> Paragraph_getWordBoundary(IntPtr ptr, int offset);

        [DllImport(NativeBindings.dllName)]
        static extern List<int> Paragraph_getLineBoundary(IntPtr ptr, int offset);

        [DllImport(NativeBindings.dllName)]
        static extern void Paragraph_paint(IntPtr ptr, Canvas canvas, double x, double y);

        [DllImport(NativeBindings.dllName)]
        static extern Utils.Float32List Paragraph_computeLineMetrics(IntPtr ptr);

        [DllImport(NativeBindings.dllName)]
        static extern void Paragraph_dispose(IntPtr ptr);

        public void layout(ParagraphConstraints constraints) => _layout(constraints.width);
        void _layout(double width) => Paragraph_layout(_ptr, width);

        List<TextBox> _decodeTextBoxes(Utils.Float32List encoded) {
            int count = encoded.Length / 5;
            List<TextBox> boxes = new List<TextBox>();
            int position = 0;
            for (int index = 0; index < count; index += 1) {
                unsafe {
                    boxes.Add(TextBox.fromLTRBD(
                        encoded.data[position++],
                        encoded.data[position++],
                        encoded.data[position++],
                        encoded.data[position++],
                        (TextDirection) encoded.data[position++]
                    ));
                }
            }

            return boxes;
        }

        public double width() {
            return Paragraph_width(this._ptr);
        }

        public double height() {
            return Paragraph_height(this._ptr);
        }

        public double longestLine() {
            return Paragraph_longestLine(this._ptr);
        }

        public double minIntrinsicWidth() {
            return Paragraph_minIntrinsicWidth(this._ptr);
        }

        public double maxIntrinsicWidth() {
            return Paragraph_maxIntrinsicWidth(this._ptr);
        }

        public double alphabeticBaseline() {
            return Paragraph_alphabeticBaseline(this._ptr);
        }

        public double ideographicBaseline() {
            return Paragraph_ideographicBaseline(this._ptr);
        }

        public bool didExceedMaxLines() {
            return Paragraph_didExceedMaxLines(this._ptr);
        }

        public List<TextBox> getBoxesForRange(int start, int end, BoxHeightStyle boxHeightStyle = BoxHeightStyle.tight,
            BoxWidthStyle boxWidthStyle = BoxWidthStyle.tight) {
            return _decodeTextBoxes(_getBoxesForRange(start, end, (int) boxHeightStyle, (int) boxWidthStyle));
        }

        // See paragraph.cc for the layout of this return value.
        Utils.Float32List _getBoxesForRange(int start, int end, int boxHeightStyle, int boxWidthStyle) =>
            Paragraph_getRectsForRange(_ptr, start, end, boxHeightStyle, boxWidthStyle);

        public List<TextBox> getBoxesForPlaceholders() {
            return _decodeTextBoxes(_getBoxesForPlaceholders());
        }

        Utils.Float32List _getBoxesForPlaceholders() => Paragraph_getRectsForPlaceholders(_ptr);

        public TextPosition getPositionForOffset(Offset offset) {
            List<int> encoded = _getPositionForOffset(offset.dx, offset.dy);
            return new TextPosition(offset: encoded[0], affinity: (TextAffinity) encoded[1]);
        }

        List<int> _getPositionForOffset(double dx, double dy) => Paragraph_getPositionForOffset(_ptr, dx, dy);

        public TextRange getWordBoundary(TextPosition position) {
            List<int> boundary = _getWordBoundary(position.offset);
            return new TextRange(start: boundary[0], end: boundary[1]);
        }

        List<int> _getWordBoundary(int offset) => Paragraph_getWordBoundary(_ptr, offset);

        public TextRange getLineBoundary(TextPosition position) {
            List<int> boundary = _getLineBoundary(position.offset);
            return new TextRange(start: boundary[0], end: boundary[1]);
        }

        List<int> _getLineBoundary(int offset) => Paragraph_getLineBoundary(_ptr, offset);

        void _paint(Canvas canvas, double x, double y) => Paragraph_paint(_ptr, canvas, x, y);

        public List<LineMetrics> computeLineMetrics() {
            Utils.Float32List encoded = _computeLineMetrics();
            int count = encoded.Length / 9;
            int position = 0;
            List<LineMetrics> metrics = new List<LineMetrics>();

            for (int index = 0; index < count; index += 1) {
                unsafe {
                    metrics.Add(new LineMetrics(
                        hardBreak: encoded.data[position++] != 0,
                        ascent: encoded.data[position++],
                        descent: encoded.data[position++],
                        unscaledAscent: encoded.data[position++],
                        height: encoded.data[position++],
                        width: encoded.data[position++],
                        left: encoded.data[position++],
                        baseline: encoded.data[position++],
                        lineNumber: (int) encoded.data[position++]
                    ));
                }
            }

            return metrics;
        }

        Utils.Float32List _computeLineMetrics() => Paragraph_computeLineMetrics(_ptr);

        protected override void DisposePtr(IntPtr ptr) {
            Paragraph_dispose(ptr);
        }
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
            double fontSize,
            double height,
            [MarshalAs(UnmanagedType.LPWStr)] string ellipsis,
            string locale);

        [DllImport(NativeBindings.dllName, CallingConvention = CallingConvention.Cdecl)]
        static unsafe extern void ParagraphBuilder_pushStyle(
            IntPtr ptr,
            int[] encoded,
            int encodedSize,
            [In] string[] fontFamilies,
            int fontFamiliesSize,
            double? fontSize,
            double? letterSpacing,
            double? wordSpacing,
            double? height,
            double? decorationThickness,
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
        static extern IntPtr ParagraphBuilder_build(IntPtr ptr /*, IntPtr outParagraph*/);

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

            this._ptr = _constructor(
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
            double fontSize,
            double height,
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

        public int placeholderCount {
            get { return _placeholderCount; }
        }

        int _placeholderCount = 0;

        public List<double> placeholderScales {
            get { return _placeholderScales; }
        }

        List<double> _placeholderScales = new List<double>();

        public void pushStyle(TextStyle style) {
            List<String> fullFontFamilies = new List<string>();

            fullFontFamilies.Add(style._fontFamily);
            if (style._fontFamilyFallback != null)
                fullFontFamilies.AddRange(style._fontFamilyFallback!);

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
            double? fontSize,
            double? letterSpacing,
            double? wordSpacing,
            double? height,
            double? decorationThickness,
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
                    fontSize,
                    letterSpacing,
                    wordSpacing,
                    height,
                    decorationThickness,
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

        static string _encodeLocale(Locale? locale) => locale?.ToString() ?? "";

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

        IntPtr _build( /*IntPtr outParagraph*/) => ParagraphBuilder_build(_ptr /*, outParagraph*/);

        protected override void DisposePtr(IntPtr ptr) {
            ParagraphBuilder_dispose(ptr);
        }
    }
}