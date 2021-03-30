using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using AOT;
using Unity.UIWidgets.async2;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.services;
using UnityEngine;

namespace Unity.UIWidgets.ui {
    public enum FontStyle {
        normal,

        italic
    }

    public class FontWeight {
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

        public static readonly Dictionary<int, string> map = new Dictionary<int, string> {
            {0, "FontWeight.w100"},
            {1, "FontWeight.w200"},
            {2, "FontWeight.w300"},
            {3, "FontWeight.w400"},
            {4, "FontWeight.w500"},
            {5, "FontWeight.w600"},
            {6, "FontWeight.w700"},
            {7, "FontWeight.w800"},
            {8, "FontWeight.w900"}
        };

        public readonly int index;

        public FontWeight(int index) {
            this.index = index;
        }

        public static FontWeight lerp(FontWeight a, FontWeight b, float t) {
            if (a == null && b == null) {
                return null;
            }

            return values[
                MathUtils.lerpNullableFloat(a?.index ?? normal.index, b?.index ?? normal.index, t: t).round()
                    .clamp(0, 8)];
        }

        public override string ToString() {
            return map[key: index];
        }
    }

    public class FontFeature : IEquatable<FontFeature> {
        internal static readonly int _kEncodedSize = 8;

        public readonly string feature;

        public readonly int value;

        public FontFeature(string feature, int value = 1) {
            D.assert(feature != null);
            D.assert(feature.Length == 4);
            D.assert(value >= 0);
            this.feature = feature;
            this.value = value;
        }

        public bool Equals(FontFeature other) {
            if (ReferenceEquals(null, objB: other)) {
                return false;
            }

            if (ReferenceEquals(this, objB: other)) {
                return true;
            }

            return feature == other.feature && value == other.value;
        }

        public FontFeature enable(string feature) {
            return new FontFeature(feature: feature);
        }

        public FontFeature disable(string feature) {
            return new FontFeature(feature: feature, 0);
        }

        public FontFeature randomize() {
            return new FontFeature("rand");
        }

        public static FontFeature stylisticSet(int value) {
            D.assert(value >= 1);
            D.assert(value <= 20);
            return new FontFeature($"ss{value.ToString().PadLeft(2, '0')}");
        }

        public FontFeature slashedZero() {
            return new FontFeature("zero");
        }

        public FontFeature oldstyleFigures() {
            return new FontFeature("onum");
        }

        public FontFeature proportionalFigures() {
            return new FontFeature("pnum");
        }

        public FontFeature tabularFigures() {
            return new FontFeature("tnum");
        }

        internal void _encode(byte[] byteData) {
            D.assert(() => {
                foreach (var charUnit in feature.ToCharArray()) {
                    if (charUnit <= 0x20 && charUnit >= 0x7F) {
                        return false;
                    }
                }

                return true;
            });
            for (var i = 0; i < 4; i++) {
                byteData[i] = (byte) feature[index: i];
            }

            byteData[4] = (byte) (value >> 0);
            byteData[5] = (byte) (value >> 8);
            byteData[6] = (byte) (value >> 16);
            byteData[7] = (byte) (value >> 24);
        }

        public override string ToString() {
            return $"FontFeature({feature}, {value})";
        }

        public override bool Equals(object obj) {
            if (ReferenceEquals(null, objB: obj)) {
                return false;
            }

            if (ReferenceEquals(this, objB: obj)) {
                return true;
            }

            if (obj.GetType() != GetType()) {
                return false;
            }

            return Equals((FontFeature) obj);
        }

        public override int GetHashCode() {
            unchecked {
                return ((feature != null ? feature.GetHashCode() : 0) * 397) ^ value;
            }
        }
    }

    public enum TextAlign {
        left,

        right,

        center,

        justify,

        start,

        end
    }

    public enum TextBaseline {
        alphabetic,

        ideographic
    }

    public class TextDecoration : IEquatable<TextDecoration> {
        public static readonly TextDecoration none = new TextDecoration(0x0);

        public static readonly TextDecoration underline = new TextDecoration(0x1);

        public static readonly TextDecoration overline = new TextDecoration(0x2);

        public static readonly TextDecoration lineThrough = new TextDecoration(0x4);

        internal readonly int _mask;

        public TextDecoration(int _mask) {
            this._mask = _mask;
        }

        public bool Equals(TextDecoration other) {
            if (ReferenceEquals(null, objB: other)) {
                return false;
            }

            if (ReferenceEquals(this, objB: other)) {
                return true;
            }

            return _mask == other._mask;
        }

        public TextDecoration combine(List<TextDecoration> decorations) {
            var mask = 0;
            foreach (var decoration in decorations) {
                mask |= decoration._mask;
            }

            return new TextDecoration(_mask: mask);
        }

        public bool contains(TextDecoration other) {
            return (_mask | other._mask) == _mask;
        }

        public override string ToString() {
            if (_mask == 0) {
                return "TextDecoration.none";
            }

            var values = new List<string>();
            if ((_mask & underline._mask) != 0) {
                values.Add("underline");
            }

            if ((_mask & overline._mask) != 0) {
                values.Add("overline");
            }

            if ((_mask & lineThrough._mask) != 0) {
                values.Add("lineThrough");
            }

            if (values.Count == 1) {
                return $"TextDecoration.{values[0]}";
            }

            return $"TextDecoration.combine([{string.Join(", ", values: values)}])";
        }

        public override bool Equals(object obj) {
            if (ReferenceEquals(null, objB: obj)) {
                return false;
            }

            if (ReferenceEquals(this, objB: obj)) {
                return true;
            }

            if (obj.GetType() != GetType()) {
                return false;
            }

            return Equals((TextDecoration) obj);
        }

        public override int GetHashCode() {
            return _mask;
        }
    }


    public enum TextDecorationStyle {
        solid,

        doubleLine,

        dotted,

        dashed,

        wavy
    }

    public class TextHeightBehavior : IEquatable<TextHeightBehavior> {
        public readonly bool applyHeightToFirstAscent;

        public readonly bool applyHeightToLastDescent;

        public TextHeightBehavior(
            bool applyHeightToFirstAscent = true,
            bool applyHeightToLastDescent = true
        ) {
            this.applyHeightToFirstAscent = applyHeightToFirstAscent;
            this.applyHeightToLastDescent = applyHeightToLastDescent;
        }

        public bool Equals(TextHeightBehavior other) {
            if (ReferenceEquals(null, objB: other)) {
                return false;
            }

            if (ReferenceEquals(this, objB: other)) {
                return true;
            }

            return applyHeightToFirstAscent == other.applyHeightToFirstAscent &&
                   applyHeightToLastDescent == other.applyHeightToLastDescent;
        }

        public static TextHeightBehavior fromEncoded(int encoded) {
            return new TextHeightBehavior((encoded & 0x1) == 0, (encoded & 0x2) == 0);
        }

        public int encode() {
            return (applyHeightToFirstAscent ? 0 : 1 << 0) | (applyHeightToLastDescent ? 0 : 1 << 1);
        }

        public override string ToString() {
            return "TextHeightBehavior(\n" +
                   $" applyHeightToFirstAscent: {applyHeightToFirstAscent}, \n" +
                   $" applyHeightToLastDescent: {applyHeightToLastDescent} \n" +
                   ")";
        }

        public override bool Equals(object obj) {
            if (ReferenceEquals(null, objB: obj)) {
                return false;
            }

            if (ReferenceEquals(this, objB: obj)) {
                return true;
            }

            if (obj.GetType() != GetType()) {
                return false;
            }

            return Equals((TextHeightBehavior) obj);
        }

        public override int GetHashCode() {
            unchecked {
                return (applyHeightToFirstAscent.GetHashCode() * 397) ^ applyHeightToLastDescent.GetHashCode();
            }
        }
    }

    public struct Float32List {
        public IntPtr data;
        public int length;
    }

    public static partial class ui_ {
        static readonly object VoidObject = new object();

        static readonly byte[] _fontChangeMessage = JSONMessageCodec.instance.encodeMessage(
            new Dictionary<string, string> {
                {"type", "fontsChange"}
            });

        internal static int[] _encodeTextStyle(
            Color color,
            TextDecoration decoration,
            Color decorationColor,
            TextDecorationStyle? decorationStyle,
            float? decorationThickness,
            FontWeight fontWeight,
            FontStyle? fontStyle,
            TextBaseline? textBaseline,
            string fontFamily,
            List<string> fontFamilyFallback,
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
            var result = new int[8];
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

            if (fontFamily != null || fontFamilyFallback != null && fontFamilyFallback.isNotEmpty()) {
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

        internal static List<int> _encodeParagraphStyle(
            TextAlign? textAlign,
            TextDirection? textDirection,
            int? maxLines,
            string fontFamily,
            float? fontSize,
            float? height,
            TextHeightBehavior textHeightBehavior,
            FontWeight fontWeight,
            FontStyle? fontStyle,
            StrutStyle strutStyle,
            string ellipsis,
            Locale locale
        ) {
            var result = new List<int>(new int[7]);
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

        internal static unsafe void setFloat(this byte[] bytes, int byteOffset, float value) {
            D.assert(byteOffset >= 0 && byteOffset + 4 < bytes.Length);
            var intVal = *(int*) &value;
            fixed (byte* b = &bytes[byteOffset]) {
                *(int*) b = intVal;
            }
        }

        internal static byte[] _encodeStrut(
            string fontFamily,
            List<string> fontFamilyFallback,
            float? fontSize,
            float? height,
            float? leading,
            FontWeight fontWeight,
            FontStyle? fontStyle,
            bool? forceStrutHeight) {
            if (fontFamily == null &&
                fontSize == null &&
                height == null &&
                leading == null &&
                fontWeight == null &&
                fontStyle == null &&
                forceStrutHeight == null) {
                return new byte[0];
            }

            var data = new byte[15]; // Max size is 15 bytes
            var bitmask = 0; // 8 bit mask
            var byteCount = 1;
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

            if (fontFamily != null || fontFamilyFallback != null && fontFamilyFallback.isNotEmpty()) {
                bitmask |= 1 << 2;
                // passed separately to native
            }

            if (fontSize != null) {
                bitmask |= 1 << 3;
                data.setFloat(byteOffset: byteCount, (float) fontSize);
                byteCount += 4;
            }

            if (height != null) {
                bitmask |= 1 << 4;
                data.setFloat(byteOffset: byteCount, (float) height);
                byteCount += 4;
            }

            if (leading != null) {
                bitmask |= 1 << 5;
                data.setFloat(byteOffset: byteCount, (float) leading);
                byteCount += 4;
            }

            if (forceStrutHeight != null) {
                bitmask |= 1 << 6;
                // We store this boolean directly in the bitmask since there is
                // extra space in the 16 bit int.
                bitmask |= (forceStrutHeight.Value ? 1 : 0) << 7;
            }

            data[0] = (byte) bitmask;
            return data.range(0, length: byteCount);
        }

        public static unsafe Future loadFontFromList(byte[] list, string fontFamily = null) {
            return _futurize((_Callback<object> callback) => {
                // var completer = new Promise(true);
                var completerHandle = GCHandle.Alloc(value: callback);
                fixed (byte* listPtr = list) {
                    Font_LoadFontFromList(list: listPtr, size: list.Length, callback: _loadFontCallback,
                        (IntPtr) completerHandle,
                        fontFamily: fontFamily);
                }

                return null;
            });
        }

        [MonoPInvokeCallback(typeof(_loadFontFromListCallback))]
        static void _loadFontCallback(IntPtr callbackHandle) {
            var completerHandle = (GCHandle) callbackHandle;
            var callback = (_Callback<object>) completerHandle.Target;
            completerHandle.Free();
            _sendFontChangeMessage();
            try {
                callback(result: VoidObject);
            }
            catch (Exception ex) {
                Debug.LogException(exception: ex);
            }
        }

        static void _sendFontChangeMessage() {
            Window.instance.onPlatformMessage?.Invoke("uiwidgets/system", data: _fontChangeMessage,
                _ => { });
        }

        [DllImport(dllName: NativeBindings.dllName)]
        static extern unsafe void Font_LoadFontFromList(byte* list, int size, _loadFontFromListCallback callback,
            IntPtr callbackHandle,
            string fontFamily);


        public static float[] toFloatArrayAndFree(this Float32List data) {
            var result = new float[data.length];
            Marshal.Copy(source: data.data, destination: result, 0, length: data.length);
            Lists_Free(data: data.data);
            return result;
        }

        [DllImport(dllName: NativeBindings.dllName)]
        public static extern void Lists_Free(IntPtr data);

        delegate void _loadFontFromListCallback(IntPtr callbackHandle);
    }

    public class TextStyle : IEquatable<TextStyle> {
        internal readonly Paint _background;
        internal readonly float? _decorationThickness;

        internal readonly int[] _encoded;
        internal readonly string _fontFamily;
        internal readonly List<string> _fontFamilyFallback;
        internal readonly List<FontFeature> _fontFeatures;
        internal readonly float? _fontSize;
        internal readonly Paint _foreground;
        internal readonly float? _height;
        internal readonly float? _letterSpacing;
        internal readonly Locale _locale;
        internal readonly List<Shadow> _shadows;
        internal readonly float? _wordSpacing;

        public TextStyle(
            Color color = null,
            TextDecoration decoration = null,
            Color decorationColor = null,
            TextDecorationStyle? decorationStyle = null,
            float? decorationThickness = null,
            FontWeight fontWeight = null,
            FontStyle? fontStyle = null,
            TextBaseline? textBaseline = null,
            string fontFamily = null,
            List<string> fontFamilyFallback = null,
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
            D.assert(color == null || foreground == null, () =>
                "Cannot provide both a color and a foreground\n" +
                "The color argument is just a shorthand for \"foreground: Paint()..color = color\"."
            );
            _encoded = ui_._encodeTextStyle(
                color: color,
                decoration: decoration,
                decorationColor: decorationColor,
                decorationStyle: decorationStyle,
                decorationThickness: decorationThickness,
                fontWeight: fontWeight,
                fontStyle: fontStyle,
                textBaseline: textBaseline,
                fontFamily: fontFamily,
                fontFamilyFallback: fontFamilyFallback,
                fontSize: fontSize,
                letterSpacing: letterSpacing,
                wordSpacing: wordSpacing,
                height: height,
                locale: locale,
                background: background,
                foreground: foreground,
                shadows: shadows,
                fontFeatures: fontFeatures
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

        public bool Equals(TextStyle other) {
            if (ReferenceEquals(null, objB: other)) {
                return false;
            }

            if (ReferenceEquals(this, objB: other)) {
                return true;
            }

            return _encoded.equalsList(list: other._encoded) && Equals(objA: _fontFamily, objB: other._fontFamily) &&
                   _fontFamilyFallback.equalsList(list: other._fontFamilyFallback) &&
                   Nullable.Equals(n1: _fontSize, n2: other._fontSize) &&
                   Nullable.Equals(n1: _letterSpacing, n2: other._letterSpacing) &&
                   Nullable.Equals(n1: _wordSpacing, n2: other._wordSpacing) &&
                   Nullable.Equals(n1: _height, n2: other._height) &&
                   Nullable.Equals(n1: _decorationThickness, n2: other._decorationThickness) &&
                   Equals(objA: _locale, objB: other._locale) && Equals(objA: _background, objB: other._background) &&
                   Equals(objA: _foreground, objB: other._foreground) && _shadows.equalsList(list: other._shadows) &&
                   _fontFeatures.equalsList(list: other._fontFeatures);
        }

        public override string ToString() {
            var color = (_encoded[0] & 0x00002) == 0x00002
                ? new Color((uint) _encoded[1]).ToString()
                : "unspecified";
            var decoration = (_encoded[0] & 0x00004) == 0x00004
                ? new TextDecoration(_encoded[2]).ToString()
                : "unspecified";
            var decorationColor = (_encoded[0] & 0x00008) == 0x00008
                ? new Color((uint) _encoded[3]).ToString()
                : "unspecified";
            var decorationStyle = (_encoded[0] & 0x00010) == 0x00010
                ? ((TextDecorationStyle) _encoded[4]).ToString()
                : "unspecified";
            // The decorationThickness is not in encoded order in order to keep it near the other decoration properties.
            var decorationThickness = (_encoded[0] & 0x00100) == 0x00100
                ? _decorationThickness.ToString()
                : "unspecified";
            var fontWeight = (_encoded[0] & 0x00020) == 0x00020
                ? FontWeight.values[_encoded[5]].ToString()
                : "unspecified";
            var fontStyle = (_encoded[0] & 0x00040) == 0x00040
                ? ((FontStyle) _encoded[6]).ToString()
                : "unspecified";
            var textBaseline = (_encoded[0] & 0x00080) == 0x00080
                ? ((TextBaseline) _encoded[7]).ToString()
                : "unspecified";
            var fontFamily = (_encoded[0] & 0x00200) == 0x00200
                             && _fontFamily != null
                ? _fontFamily
                : "unspecified";
            var fontFamilyFallback = (_encoded[0] & 0x00200) == 0x00200
                                     && _fontFamilyFallback != null
                                     && _fontFamilyFallback.isNotEmpty()
                ? _fontFamilyFallback.ToString()
                : "unspecified";
            var fontSize = (_encoded[0] & 0x00400) == 0x00400 ? _fontSize.ToString() : "unspecified";
            var letterSpacing = (_encoded[0] & 0x00800) == 0x00800 ? "_letterSpacing}x" : "unspecified";
            var wordSpacing = (_encoded[0] & 0x01000) == 0x01000 ? "_wordSpacing}x" : "unspecified";
            var height = (_encoded[0] & 0x02000) == 0x02000 ? "_height}x" : "unspecified";
            var locale = (_encoded[0] & 0x04000) == 0x04000 ? _locale.ToString() : "unspecified";
            var background = (_encoded[0] & 0x08000) == 0x08000 ? _locale.ToString() : "unspecified";
            var foreground = (_encoded[0] & 0x10000) == 0x10000 ? _locale.ToString() : "unspecified";
            var shadows = (_encoded[0] & 0x20000) == 0x20000 ? _locale.ToString() : "unspecified";

            var fontFeatures =
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

        public override bool Equals(object obj) {
            if (ReferenceEquals(null, objB: obj)) {
                return false;
            }

            if (ReferenceEquals(this, objB: obj)) {
                return true;
            }

            if (obj.GetType() != GetType()) {
                return false;
            }

            return Equals((TextStyle) obj);
        }

        public override int GetHashCode() {
            unchecked {
                var hashCode = _encoded != null ? _encoded.GetHashCode() : 0;
                hashCode = (hashCode * 397) ^ (_fontFamily != null ? _fontFamily.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (_fontFamilyFallback != null ? _fontFamilyFallback.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ _fontSize.GetHashCode();
                hashCode = (hashCode * 397) ^ _letterSpacing.GetHashCode();
                hashCode = (hashCode * 397) ^ _wordSpacing.GetHashCode();
                hashCode = (hashCode * 397) ^ _height.GetHashCode();
                hashCode = (hashCode * 397) ^ _decorationThickness.GetHashCode();
                hashCode = (hashCode * 397) ^ (_locale != null ? _locale.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (_background != null ? _background.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (_foreground != null ? _foreground.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (_shadows != null ? _shadows.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (_fontFeatures != null ? _fontFeatures.GetHashCode() : 0);
                return hashCode;
            }
        }
    }

    public class ParagraphStyle : IEquatable<ParagraphStyle> {
        internal readonly string _ellipsis;

        internal readonly List<int> _encoded;
        internal readonly string _fontFamily;
        internal readonly float? _fontSize;
        internal readonly float? _height;
        internal readonly Locale _locale;
        internal readonly StrutStyle _strutStyle;

        public ParagraphStyle(
            TextAlign? textAlign = null,
            TextDirection? textDirection = null,
            int? maxLines = null,
            string fontFamily = null,
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
                textAlign: textAlign,
                textDirection: textDirection,
                maxLines: maxLines,
                fontFamily: fontFamily,
                fontSize: fontSize,
                height: height,
                textHeightBehavior: textHeightBehavior,
                fontWeight: fontWeight,
                fontStyle: fontStyle,
                strutStyle: strutStyle,
                ellipsis: ellipsis,
                locale: locale
            );
            _fontFamily = fontFamily;
            _fontSize = fontSize;
            _height = height;
            _strutStyle = strutStyle;
            _ellipsis = ellipsis;
            _locale = locale;
        }

        public bool Equals(ParagraphStyle other) {
            if (ReferenceEquals(null, objB: other)) {
                return false;
            }

            if (ReferenceEquals(this, objB: other)) {
                return true;
            }

            return _encoded.equalsList(list: other._encoded) && _fontFamily == other._fontFamily &&
                   Nullable.Equals(n1: _fontSize, n2: other._fontSize) &&
                   Nullable.Equals(n1: _height, n2: other._height) &&
                   Equals(objA: _strutStyle, objB: other._strutStyle) && _ellipsis == other._ellipsis &&
                   Equals(objA: _locale, objB: other._locale);
        }

        public override string ToString() {
            var textAlign = (_encoded[0] & 0x002) == 0x002
                ? ((TextAlign) _encoded[1]).ToString()
                : "unspecified";
            var textDirection = (_encoded[0] & 0x004) == 0x004
                ? ((TextDirection) _encoded[2]).ToString()
                : "unspecified";
            var fontWeight = (_encoded[0] & 0x008) == 0x008
                ? FontWeight.values[_encoded[3]].ToString()
                : "unspecified";
            var fontStyle = (_encoded[0] & 0x010) == 0x010
                ? ((FontStyle) _encoded[4]).ToString()
                : "unspecified";
            var maxLines = (_encoded[0] & 0x020) == 0x020 ? _encoded[5].ToString() : "unspecified";
            var textHeightBehavior = (_encoded[0] & 0x040) == 0x040
                ? TextHeightBehavior.fromEncoded(_encoded[6]).ToString()
                : "unspecified";
            var fontFamily = (_encoded[0] & 0x080) == 0x080 ? _fontFamily : "unspecified";
            var fontSize = (_encoded[0] & 0x100) == 0x100 ? _fontSize.ToString() : "unspecified";
            var height = (_encoded[0] & 0x200) == 0x200 ? $"{_height}x" : "unspecified";
            var ellipsis = (_encoded[0] & 0x400) == 0x400 ? _ellipsis : "unspecified";
            var locale = (_encoded[0] & 0x800) == 0x800 ? _locale.ToString() : "unspecified";
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

        public override bool Equals(object obj) {
            if (ReferenceEquals(null, objB: obj)) {
                return false;
            }

            if (ReferenceEquals(this, objB: obj)) {
                return true;
            }

            if (obj.GetType() != GetType()) {
                return false;
            }

            return Equals((ParagraphStyle) obj);
        }

        public override int GetHashCode() {
            unchecked {
                var hashCode = _encoded != null ? _encoded.GetHashCode() : 0;
                hashCode = (hashCode * 397) ^ (_fontFamily != null ? _fontFamily.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ _fontSize.GetHashCode();
                hashCode = (hashCode * 397) ^ _height.GetHashCode();
                hashCode = (hashCode * 397) ^ (_strutStyle != null ? _strutStyle.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (_ellipsis != null ? _ellipsis.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (_locale != null ? _locale.GetHashCode() : 0);
                return hashCode;
            }
        }
    }

    public class StrutStyle : IEquatable<StrutStyle> {
        internal readonly byte[] _encoded; // Most of the data for strut is encoded.
        internal readonly string _fontFamily;
        internal readonly List<string> _fontFamilyFallback;

        public StrutStyle(
            string fontFamily = null,
            List<string> fontFamilyFallback = null,
            float? fontSize = null,
            float? height = null,
            float? leading = null,
            FontWeight fontWeight = null,
            FontStyle? fontStyle = null,
            bool? forceStrutHeight = null
        ) {
            _encoded = ui_._encodeStrut(
                fontFamily: fontFamily,
                fontFamilyFallback: fontFamilyFallback,
                fontSize: fontSize,
                height: height,
                leading: leading,
                fontWeight: fontWeight,
                fontStyle: fontStyle,
                forceStrutHeight: forceStrutHeight
            );
            _fontFamily = fontFamily;
            _fontFamilyFallback = fontFamilyFallback;
        }

        public bool Equals(StrutStyle other) {
            if (ReferenceEquals(null, objB: other)) {
                return false;
            }

            if (ReferenceEquals(this, objB: other)) {
                return true;
            }

            return _encoded.equalsList(list: other._encoded) && _fontFamily == other._fontFamily &&
                   _fontFamilyFallback.equalsList(list: other._fontFamilyFallback);
        }

        public override bool Equals(object obj) {
            if (ReferenceEquals(null, objB: obj)) {
                return false;
            }

            if (ReferenceEquals(this, objB: obj)) {
                return true;
            }

            if (obj.GetType() != GetType()) {
                return false;
            }

            return Equals((StrutStyle) obj);
        }

        public override int GetHashCode() {
            unchecked {
                var hashCode = _encoded != null ? _encoded.GetHashCode() : 0;
                hashCode = (hashCode * 397) ^ (_fontFamily != null ? _fontFamily.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (_fontFamilyFallback != null ? _fontFamilyFallback.GetHashCode() : 0);
                return hashCode;
            }
        }
    }

    public enum TextDirection {
        rtl,
        ltr,
    }

    public class TextBox : IEquatable<TextBox> {
        public readonly float bottom;

        public readonly TextDirection direction;

        public readonly float left;

        public readonly float right;

        public readonly float top;

        TextBox(
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

        public float start {
            get { return direction == TextDirection.ltr ? left : right; }
        }

        public float end {
            get { return direction == TextDirection.ltr ? right : left; }
        }

        public bool Equals(TextBox other) {
            if (ReferenceEquals(null, objB: other)) {
                return false;
            }

            if (ReferenceEquals(this, objB: other)) {
                return true;
            }

            return left.Equals(obj: other.left) && top.Equals(obj: other.top) && right.Equals(obj: other.right) &&
                   bottom.Equals(obj: other.bottom) && direction == other.direction;
        }

        public static TextBox fromLTRBD(
            float left,
            float top,
            float right,
            float bottom,
            TextDirection direction) {
            return new TextBox(left: left, top: top, right: right, bottom: bottom, direction: direction);
        }

        public Rect toRect() {
            return Rect.fromLTRB(left: left, top: top, right: right, bottom);
        }

        public override string ToString() {
            return
                $"TextBox.fromLTRBD({left.ToString("N1")}, {top.ToString("N1")}, {right.ToString("N1")}, {bottom.ToString("N1")}, {direction})";
        }

        public override bool Equals(object obj) {
            if (ReferenceEquals(null, objB: obj)) {
                return false;
            }

            if (ReferenceEquals(this, objB: obj)) {
                return true;
            }

            if (obj.GetType() != GetType()) {
                return false;
            }

            return Equals((TextBox) obj);
        }

        public override int GetHashCode() {
            unchecked {
                var hashCode = left.GetHashCode();
                hashCode = (hashCode * 397) ^ top.GetHashCode();
                hashCode = (hashCode * 397) ^ right.GetHashCode();
                hashCode = (hashCode * 397) ^ bottom.GetHashCode();
                hashCode = (hashCode * 397) ^ (int) direction;
                return hashCode;
            }
        }
    }

    public enum TextAffinity {
        upstream,

        downstream
    }

    public class TextPosition : IEquatable<TextPosition> {
        public readonly TextAffinity affinity;

        public readonly int offset;

        public TextPosition(
            int offset,
            TextAffinity affinity = TextAffinity.downstream) {
            this.offset = offset;
            this.affinity = affinity;
        }

        public bool Equals(TextPosition other) {
            if (ReferenceEquals(null, objB: other)) {
                return false;
            }

            if (ReferenceEquals(this, objB: other)) {
                return true;
            }

            return offset == other.offset && affinity == other.affinity;
        }

        public override string ToString() {
            return $"TextPosition(offset: {offset}, affinity: {affinity})";
        }

        public override bool Equals(object obj) {
            if (ReferenceEquals(null, objB: obj)) {
                return false;
            }

            if (ReferenceEquals(this, objB: obj)) {
                return true;
            }

            if (obj.GetType() != GetType()) {
                return false;
            }

            return Equals((TextPosition) obj);
        }

        public override int GetHashCode() {
            unchecked {
                return (offset * 397) ^ (int) affinity;
            }
        }
    }

    public class TextRange : IEquatable<TextRange> {
        public static readonly TextRange empty = new TextRange(-1, -1);

        public readonly int end;

        public readonly int start;

        public TextRange(
            int start, int end) {
            D.assert(start >= -1);
            D.assert(end >= -1);
            this.start = start;
            this.end = end;
        }

        public bool isValid {
            get { return start >= 0 && end >= 0; }
        }

        public bool isCollapsed {
            get { return start == end; }
        }

        public bool isNormalized {
            get { return end >= start; }
        }

        public bool Equals(TextRange other) {
            if (ReferenceEquals(null, objB: other)) {
                return false;
            }

            if (ReferenceEquals(this, objB: other)) {
                return true;
            }

            return start == other.start && end == other.end;
        }

        public static TextRange collapsed(int offset) {
            D.assert(offset >= -1);
            return new TextRange(start: offset, end: offset);
        }

        public string textBefore(string text) {
            D.assert(result: isNormalized);
            return text.Substring(0, length: start);
        }

        public string textAfter(string text) {
            D.assert(result: isNormalized);
            return text.Substring(startIndex: end);
        }

        public string textInside(string text) {
            D.assert(result: isNormalized);
            return text.Substring(startIndex: start, end - start);
        }

        public override string ToString() {
            return $"TextRange(start: {start}, end: {end})";
        }

        public override bool Equals(object obj) {
            if (ReferenceEquals(null, objB: obj)) {
                return false;
            }

            if (ReferenceEquals(this, objB: obj)) {
                return true;
            }

            if (obj.GetType() != GetType()) {
                return false;
            }

            return Equals((TextRange) obj);
        }

        public override int GetHashCode() {
            unchecked {
                return (start * 397) ^ end;
            }
        }
    }

    public class ParagraphConstraints : IEquatable<ParagraphConstraints> {
        public readonly float width;

        public ParagraphConstraints(float width) {
            this.width = width;
        }

        public bool Equals(ParagraphConstraints other) {
            if (ReferenceEquals(null, objB: other)) {
                return false;
            }

            if (ReferenceEquals(this, objB: other)) {
                return true;
            }

            return width.Equals(obj: other.width);
        }

        public override string ToString() {
            return $"ParagraphConstraints(width: {width})";
        }

        public override bool Equals(object obj) {
            if (ReferenceEquals(null, objB: obj)) {
                return false;
            }

            if (ReferenceEquals(this, objB: obj)) {
                return true;
            }

            if (obj.GetType() != GetType()) {
                return false;
            }

            return Equals((ParagraphConstraints) obj);
        }

        public override int GetHashCode() {
            return width.GetHashCode();
        }
    }

    public enum BoxHeightStyle {
        tight,

        max,

        includeLineSpacingMiddle,

        includeLineSpacingTop,

        includeLineSpacingBottom,

        strut
    }

    public enum BoxWidthStyle {
        tight,

        max
    }

    public enum PlaceholderAlignment {
        baseline,

        aboveBaseline,

        belowBaseline,

        top,

        bottom,

        middle
    }

    public class LineMetrics : IEquatable<LineMetrics> {
        public readonly float ascent;

        public readonly float baseline;

        public readonly float descent;

        public readonly bool hardBreak;

        public readonly float height;

        public readonly float left;

        public readonly int lineNumber;

        public readonly float unscaledAscent;

        public readonly float width;

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

        public bool Equals(LineMetrics other) {
            if (ReferenceEquals(null, objB: other)) {
                return false;
            }

            if (ReferenceEquals(this, objB: other)) {
                return true;
            }

            return hardBreak == other.hardBreak && ascent.Equals(obj: other.ascent) &&
                   descent.Equals(obj: other.descent) &&
                   unscaledAscent.Equals(obj: other.unscaledAscent) && height.Equals(obj: other.height) &&
                   width.Equals(obj: other.width) && left.Equals(obj: other.left) &&
                   baseline.Equals(obj: other.baseline) &&
                   lineNumber == other.lineNumber;
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

        public override bool Equals(object obj) {
            if (ReferenceEquals(null, objB: obj)) {
                return false;
            }

            if (ReferenceEquals(this, objB: obj)) {
                return true;
            }

            if (obj.GetType() != GetType()) {
                return false;
            }

            return Equals((LineMetrics) obj);
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
    }

    public class Paragraph : NativeWrapper {
        internal Paragraph(IntPtr ptr) {
            _setPtr(ptr: ptr);
        }

        [DllImport(dllName: NativeBindings.dllName)]
        static extern float Paragraph_width(IntPtr ptr);

        [DllImport(dllName: NativeBindings.dllName)]
        static extern float Paragraph_height(IntPtr ptr);

        [DllImport(dllName: NativeBindings.dllName)]
        static extern float Paragraph_longestLine(IntPtr ptr);

        [DllImport(dllName: NativeBindings.dllName)]
        static extern float Paragraph_minIntrinsicWidth(IntPtr ptr);

        [DllImport(dllName: NativeBindings.dllName)]
        static extern float Paragraph_maxIntrinsicWidth(IntPtr ptr);

        [DllImport(dllName: NativeBindings.dllName)]
        static extern float Paragraph_alphabeticBaseline(IntPtr ptr);

        [DllImport(dllName: NativeBindings.dllName)]
        static extern float Paragraph_ideographicBaseline(IntPtr ptr);

        [DllImport(dllName: NativeBindings.dllName)]
        static extern bool Paragraph_didExceedMaxLines(IntPtr ptr);

        [DllImport(dllName: NativeBindings.dllName)]
        static extern void Paragraph_layout(IntPtr ptr, float width);

        [DllImport(dllName: NativeBindings.dllName)]
        static extern Float32List Paragraph_getRectsForRange(IntPtr ptr, int start, int end,
            int boxHeightStyle,
            int boxWidthStyle);

        [DllImport(dllName: NativeBindings.dllName)]
        static extern Float32List Paragraph_getRectsForPlaceholders(IntPtr ptr);

        [DllImport(dllName: NativeBindings.dllName)]
        static extern unsafe void Paragraph_getPositionForOffset(IntPtr ptr, float dx, float dy, int* encodedPtr);

        [DllImport(dllName: NativeBindings.dllName)]
        static extern unsafe void Paragraph_getWordBoundary(IntPtr ptr, int offset, int* boundaryPtr);

        [DllImport(dllName: NativeBindings.dllName)]
        static extern unsafe void Paragraph_getLineBoundary(IntPtr ptr, int offset, int* boundaryPtr);

        [DllImport(dllName: NativeBindings.dllName)]
        static extern void Paragraph_paint(IntPtr ptr, IntPtr canvas, float x, float y);

        [DllImport(dllName: NativeBindings.dllName)]
        static extern Float32List Paragraph_computeLineMetrics(IntPtr ptr);

        [DllImport(dllName: NativeBindings.dllName)]
        static extern void Paragraph_dispose(IntPtr ptr);

        public void layout(ParagraphConstraints constraints) {
            _layout(width: constraints.width);
        }

        void _layout(float width) {
            Paragraph_layout(ptr: _ptr, width: width);
        }

        List<TextBox> _decodeTextBoxes(float[] encoded, int size) {
            var count = size / 5;
            var boxes = new List<TextBox>();
            var position = 0;
            for (var index = 0; index < count; index += 1) {
                boxes.Add(TextBox.fromLTRBD(
                    encoded[position++],
                    encoded[position++],
                    encoded[position++],
                    encoded[position++],
                    (TextDirection) encoded[position++]
                ));
            }

            return boxes;
        }

        public override void DisposePtr(IntPtr ptr) {
            Paragraph_dispose(ptr: ptr);
        }

        public float width() {
            return Paragraph_width(ptr: _ptr);
        }

        public float height() {
            return Paragraph_height(ptr: _ptr);
        }

        public float longestLine() {
            return Paragraph_longestLine(ptr: _ptr);
        }

        public float minIntrinsicWidth() {
            return Paragraph_minIntrinsicWidth(ptr: _ptr);
        }

        public float maxIntrinsicWidth() {
            return Paragraph_maxIntrinsicWidth(ptr: _ptr);
        }

        public float alphabeticBaseline() {
            return Paragraph_alphabeticBaseline(ptr: _ptr);
        }

        public float ideographicBaseline() {
            return Paragraph_ideographicBaseline(ptr: _ptr);
        }

        public bool didExceedMaxLines() {
            return Paragraph_didExceedMaxLines(ptr: _ptr);
        }

        public List<TextBox> getBoxesForRange(int start, int end,
            BoxHeightStyle boxHeightStyle = BoxHeightStyle.tight,
            BoxWidthStyle boxWidthStyle = BoxWidthStyle.tight) {
            var data = _getBoxesForRange(start: start, end: end, (int) boxHeightStyle, (int) boxWidthStyle)
                .toFloatArrayAndFree();
            return _decodeTextBoxes(encoded: data, size: data.Length);
        }

        // See paragraph.cc for the layout of this return value.
        Float32List _getBoxesForRange(int start, int end, int boxHeightStyle, int boxWidthStyle) {
            return Paragraph_getRectsForRange(ptr: _ptr, start: start, end: end, boxHeightStyle: boxHeightStyle,
                boxWidthStyle: boxWidthStyle);
        }


        public List<TextBox> getBoxesForPlaceholders() {
            var data = _getBoxesForPlaceholders().toFloatArrayAndFree();
            return _decodeTextBoxes(encoded: data, size: data.Length);
        }

        Float32List _getBoxesForPlaceholders() {
            return Paragraph_getRectsForPlaceholders(ptr: _ptr);
        }

        public unsafe TextPosition getPositionForOffset(Offset offset) {
            var encoded = new int[2];
            fixed (int* encodedPtr = encoded) {
                _getPositionForOffset(dx: offset.dx, dy: offset.dy, encodedPtr: encodedPtr);
            }

            return new TextPosition(encoded[0], (TextAffinity) encoded[1]);
        }

        unsafe void _getPositionForOffset(float dx, float dy, int* encodedPtr) {
            Paragraph_getPositionForOffset(ptr: _ptr, dx: dx, dy: dy, encodedPtr: encodedPtr);
        }

        public unsafe TextRange getWordBoundary(TextPosition position) {
            var boundary = new int[2];
            fixed (int* boundaryPtr = boundary) {
                _getWordBoundary(offset: position.offset, boundaryPtr: boundaryPtr);
            }

            return new TextRange(boundary[0], boundary[1]);
        }

        unsafe void _getWordBoundary(int offset, int* boundaryPtr) {
            Paragraph_getWordBoundary(ptr: _ptr, offset: offset, boundaryPtr: boundaryPtr);
        }

        public unsafe TextRange getLineBoundary(TextPosition position) {
            var boundary = new int[2];
            fixed (int* boundaryPtr = boundary) {
                _getLineBoundary(offset: position.offset, boundaryPtr: boundaryPtr);
            }

            return new TextRange(boundary[0], boundary[1]);
        }

        unsafe void _getLineBoundary(int offset, int* boundaryPtr) {
            Paragraph_getLineBoundary(ptr: _ptr, offset: offset, boundaryPtr: boundaryPtr);
        }

        internal void _paint(Canvas canvas, float x, float y) {
            Paragraph_paint(ptr: _ptr, canvas: canvas._ptr, x: x, y: y);
        }

        public List<LineMetrics> computeLineMetrics() {
            var data = _computeLineMetrics().toFloatArrayAndFree();
            var count = data.Length / 9;
            var position = 0;
            var metrics = new List<LineMetrics>();

            for (var index = 0; index < count; index += 1) {
                metrics.Add(new LineMetrics(
                    data[position++] != 0,
                    data[position++],
                    data[position++],
                    data[position++],
                    data[position++],
                    data[position++],
                    data[position++],
                    data[position++],
                    (int) data[position++]
                ));
            }

            return metrics;
        }

        Float32List _computeLineMetrics() {
            return Paragraph_computeLineMetrics(ptr: _ptr);
        }
    }

    public class ParagraphBuilder : NativeWrapper {
        public ParagraphBuilder(ParagraphStyle style) {
            List<string> strutFontFamilies = null;
            var strutStyle = style._strutStyle;
            if (strutStyle != null) {
                strutFontFamilies = new List<string>();
                var fontFamily = strutStyle._fontFamily;
                if (fontFamily != null) {
                    strutFontFamilies.Add(item: fontFamily);
                }

                if (strutStyle._fontFamilyFallback != null) {
                    strutFontFamilies.AddRange(collection: strutStyle._fontFamilyFallback);
                }
            }

            _setPtr(_constructor(
                style._encoded.ToArray(),
                strutStyle?._encoded.ToArray(),
                fontFamily: style._fontFamily,
                strutFontFamilies?.ToArray(),
                style._fontSize ?? 0,
                style._height ?? 0,
                ellipsis: style._ellipsis,
                _encodeLocale(locale: style._locale)
            ));
        }

        public int placeholderCount { get; private set; }

        public List<float> placeholderScales { get; } = new List<float>();

        [DllImport(dllName: NativeBindings.dllName)]
        static extern IntPtr ParagraphBuilder_constructor(
            int[] encoded,
            int encodeSize,
            byte[] strutData,
            int structDataSize,
            string fontFamily,
            string[] structFontFamily,
            int structFontFamilySize,
            float fontSize,
            float height,
            [MarshalAs(unmanagedType: UnmanagedType.LPWStr)]
            string ellipsis,
            string locale);

        [DllImport(dllName: NativeBindings.dllName)]
        static extern unsafe void ParagraphBuilder_pushStyle(
            IntPtr ptr,
            int[] encoded,
            int encodedSize,
            string[] fontFamilies,
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

        [DllImport(dllName: NativeBindings.dllName)]
        static extern void ParagraphBuilder_pop(IntPtr ptr);

        [DllImport(dllName: NativeBindings.dllName)]
        static extern IntPtr ParagraphBuilder_addText(IntPtr ptr, [MarshalAs(unmanagedType: UnmanagedType.LPWStr)]
            string text);

        [DllImport(dllName: NativeBindings.dllName)]
        static extern IntPtr ParagraphBuilder_addPlaceholder(IntPtr ptr, float width, float height, int alignment,
            float baselineOffset, int baseline);

        [DllImport(dllName: NativeBindings.dllName)]
        static extern IntPtr ParagraphBuilder_build(IntPtr ptr);

        [DllImport(dllName: NativeBindings.dllName)]
        static extern void ParagraphBuilder_dispose(IntPtr ptr);

        IntPtr _constructor(
            int[] encoded,
            byte[] structData,
            string fontFamily,
            string[] structFontFamily,
            float fontSize,
            float height,
            string ellipsis,
            string locale
        ) {
            return ParagraphBuilder_constructor(
                encoded: encoded,
                encoded == null ? 0 : encoded.Length,
                strutData: structData,
                structData == null ? 0 : structData.Length,
                fontFamily: fontFamily,
                structFontFamily: structFontFamily,
                structFontFamily == null ? 0 : structFontFamily.Length,
                fontSize: fontSize,
                height: height,
                ellipsis: ellipsis,
                locale: locale
            );
        }

        public override void DisposePtr(IntPtr ptr) {
            ParagraphBuilder_dispose(ptr: ptr);
        }

        public void pushStyle(TextStyle style) {
            var fullFontFamilies = new List<string>();

            fullFontFamilies.Add(item: style._fontFamily);
            if (style._fontFamilyFallback != null) {
                fullFontFamilies.AddRange(collection: style._fontFamilyFallback);
            }

            byte[] encodedFontFeatures = null;
            var fontFeatures = style._fontFeatures;
            if (fontFeatures != null) {
                encodedFontFeatures = new byte[fontFeatures.Count * FontFeature._kEncodedSize];
                var byteOffset = 0;
                foreach (var feature in fontFeatures) {
                    var byteFeature = new byte[FontFeature._kEncodedSize - byteOffset];
                    Array.Copy(sourceArray: encodedFontFeatures, 0, destinationArray: byteFeature, 0,
                        length: FontFeature._kEncodedSize);
                    feature._encode(byteData: byteFeature);
                    byteOffset += FontFeature._kEncodedSize;
                }
            }

            _pushStyle(
                style._encoded.ToArray(),
                fullFontFamilies.ToArray(),
                fontSize: style._fontSize,
                letterSpacing: style._letterSpacing,
                wordSpacing: style._wordSpacing,
                height: style._height,
                decorationThickness: style._decorationThickness,
                _encodeLocale(locale: style._locale),
                backgroundObjects: style._background?._objectPtrs,
                backgroundData: style._background?._data,
                foregroundObjects: style._foreground?._objectPtrs,
                foregroundData: style._foreground?._data,
                Shadow._encodeShadows(shadows: style._shadows),
                fontFeaturesData: encodedFontFeatures
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
            fixed (IntPtr* foregroundObjectsPtr = foregroundObjects) {
                ParagraphBuilder_pushStyle(ptr: _ptr,
                    encoded: encoded,
                    encodedSize: encoded.Length,
                    fontFamilies: fontFamilies,
                    fontFamiliesSize: fontFamilies.Length,
                    fontSize.GetValueOrDefault(0),
                    letterSpacing.GetValueOrDefault(0),
                    wordSpacing.GetValueOrDefault(0),
                    height.GetValueOrDefault(0),
                    decorationThickness.GetValueOrDefault(0),
                    locale: locale,
                    backgroundObjects: backgroundObjectsPtr,
                    backgroundData: backgroundData,
                    foregroundObjects: foregroundObjectsPtr,
                    foregroundData: foregroundData,
                    shadowsData: shadowsData,
                    shadowsData?.Length ?? 0,
                    fontFeaturesData: fontFeaturesData,
                    fontFeaturesData?.Length ?? 0
                );
            }
        }

        static string _encodeLocale(Locale locale) {
            return locale?.ToString() ?? "";
        }

        public void pop() {
            ParagraphBuilder_pop(ptr: _ptr);
        }

        public void addText(string text) {
            var error = _addText(text: text);
            if (error != IntPtr.Zero) {
                throw new Exception(Marshal.PtrToStringAnsi(ptr: error));
            }
        }

        IntPtr _addText(string text) {
            return ParagraphBuilder_addText(ptr: _ptr, text: text);
        }

        public void addPlaceholder(float width, float height, PlaceholderAlignment alignment,
            TextBaseline baseline,
            float scale = 1.0f,
            float? baselineOffset = null
        ) {
            // Require a baseline to be specified if using a baseline-based alignment.
            D.assert(alignment == PlaceholderAlignment.aboveBaseline ||
                     alignment == PlaceholderAlignment.belowBaseline ||
                     alignment == PlaceholderAlignment.baseline);
            // Default the baselineOffset to height if null. This will place the placeholder
            // fully above the baseline, similar to [PlaceholderAlignment.aboveBaseline].
            var baselineOffsetFloat = baselineOffset ?? height;
            _addPlaceholder(width * scale, height * scale, (int) alignment, baselineOffsetFloat * scale,
                (int) baseline);
            placeholderCount++;
            placeholderScales.Add(item: scale);
        }

        IntPtr _addPlaceholder(float width, float height, int alignment, float baselineOffset,
            int baseline) {
            return ParagraphBuilder_addPlaceholder(ptr: _ptr, width: width, height: height, alignment: alignment,
                baselineOffset: baselineOffset, baseline: baseline);
        }

        public Paragraph build() {
            var paragraph = new Paragraph(_build());
            return paragraph;
        }

        IntPtr _build() {
            return ParagraphBuilder_build(ptr: _ptr);
        }
    }
}