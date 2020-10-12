using System;
using System.Collections.Generic;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.painting;

namespace Unity.UIWidgets.uiOld{
    public enum FontStyle {
        normal,
        italic,
    }

    public enum TextBaseline {
        alphabetic,
        ideographic,
    }

    public enum TextAlign {
        left,
        right,
        center,
        justify,
    }

    public class ParagraphConstraints : IEquatable<ParagraphConstraints> {
        public readonly float width;

        public ParagraphConstraints(float width) {
            this.width = width;
        }

        public bool Equals(ParagraphConstraints other) {
            if (ReferenceEquals(null, other)) {
                return false;
            }

            if (ReferenceEquals(this, other)) {
                return true;
            }

            return width.Equals(other.width);
        }

        public override bool Equals(object obj) {
            if (ReferenceEquals(null, obj)) {
                return false;
            }

            if (ReferenceEquals(this, obj)) {
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

        public static bool operator ==(ParagraphConstraints left, ParagraphConstraints right) {
            return Equals(left, right);
        }

        public static bool operator !=(ParagraphConstraints left, ParagraphConstraints right) {
            return !Equals(left, right);
        }

        public override string ToString() {
            return $"Width: {width}";
        }
    }

    class TextStyle : IEquatable<TextStyle> {
        public static readonly Color kDefaultColor = Color.fromARGB(255, 0, 0, 0);
        public const float kDefaultFontSize = 14.0f;
        public static readonly FontWeight kDefaultFontWeight = FontWeight.w400;
        public const FontStyle kDefaultfontStyle = FontStyle.normal;
        public const float kDefaultLetterSpacing = 0.0f;
        public const float kDefaultWordSpacing = 0.0f;
        public const TextBaseline kDefaultTextBaseline = TextBaseline.alphabetic;
        public const float kDefaultHeight = 1.0f;
        public static readonly TextDecoration kDefaultDecoration = TextDecoration.none;
        public const TextDecorationStyle kDefaultDecorationStyle = TextDecorationStyle.solid;
        public const string kDefaultFontFamily = "Helvetica";

        public readonly Color color = kDefaultColor;
        public readonly float fontSize = kDefaultFontSize;
        public readonly FontWeight fontWeight = kDefaultFontWeight;
        public readonly FontStyle fontStyle = kDefaultfontStyle;
        public readonly float letterSpacing = kDefaultLetterSpacing;
        public readonly float wordSpacing = kDefaultWordSpacing;
        public readonly TextBaseline textBaseline = kDefaultTextBaseline;
        public readonly float height = kDefaultHeight;
        public readonly TextDecoration decoration = kDefaultDecoration;
        public readonly Color decorationColor;
        public readonly TextDecorationStyle decorationStyle = kDefaultDecorationStyle;
        public readonly string fontFamily = kDefaultFontFamily;
        public readonly Paint foreground;
        public readonly Paint background;
        public readonly List<BoxShadow> shadows;

        internal UnityEngine.Color UnityColor {
            get { return color.toColor(); }
        }

        internal UnityEngine.FontStyle UnityFontStyle {
            get {
                bool isBold = fontWeight.index == FontWeight.bold.index;
                if (fontStyle == FontStyle.italic) {
                    if (isBold) {
                        return UnityEngine.FontStyle.BoldAndItalic;
                    }
                    else {
                        return UnityEngine.FontStyle.Italic;
                    }
                }
                else if (isBold) {
                    return UnityEngine.FontStyle.Bold;
                }

                return UnityEngine.FontStyle.Normal;
            }
        }

        internal int UnityFontSize {
            get { return (int) fontSize; }
        }

        public static TextStyle applyStyle(TextStyle currentStyle, painting.TextStyle style, float textScaleFactor) {
            if (currentStyle != null) {
                return new TextStyle(
                    color: style.color ?? currentStyle.color,
                    fontSize: style.fontSize != null ? style.fontSize * textScaleFactor : currentStyle.fontSize,
                    fontWeight: style.fontWeight ?? currentStyle.fontWeight,
                    fontStyle: style.fontStyle ?? currentStyle.fontStyle,
                    letterSpacing: style.letterSpacing ?? currentStyle.letterSpacing,
                    wordSpacing: style.wordSpacing ?? currentStyle.wordSpacing,
                    textBaseline: style.textBaseline ?? currentStyle.textBaseline,
                    height: style.height ?? currentStyle.height,
                    decoration: style.decoration ?? currentStyle.decoration,
                    decorationColor: style.decorationColor ?? currentStyle.decorationColor,
                    fontFamily: style.fontFamily ?? currentStyle.fontFamily,
                    foreground: style.foreground ?? currentStyle.foreground,
                    background: style.background ?? currentStyle.background,
                    shadows: style.shadows ?? currentStyle.shadows
                );
            }

            return new TextStyle(
                color: style.color,
                fontSize: style.fontSize * textScaleFactor,
                fontWeight: style.fontWeight,
                fontStyle: style.fontStyle,
                letterSpacing: style.letterSpacing,
                wordSpacing: style.wordSpacing,
                textBaseline: style.textBaseline,
                height: style.height,
                decoration: style.decoration,
                decorationColor: style.decorationColor,
                fontFamily: style.fontFamily,
                foreground: style.foreground,
                background: style.background,
                shadows: style.shadows
            );
        }

        public bool Equals(TextStyle other) {
            if (ReferenceEquals(null, other)) {
                return false;
            }

            if (ReferenceEquals(this, other)) {
                return true;
            }

            return Equals(color, other.color) && fontSize.Equals(other.fontSize) &&
                   fontWeight == other.fontWeight && fontStyle == other.fontStyle &&
                   letterSpacing.Equals(other.letterSpacing) && wordSpacing.Equals(other.wordSpacing) &&
                   textBaseline == other.textBaseline && height.Equals(other.height) &&
                   Equals(decoration, other.decoration) &&
                   Equals(decorationColor, other.decorationColor) &&
                   decorationStyle == other.decorationStyle &&
                   string.Equals(fontFamily, other.fontFamily) &&
                   shadows.equalsList(other.shadows);
        }

        public override bool Equals(object obj) {
            if (ReferenceEquals(null, obj)) {
                return false;
            }

            if (ReferenceEquals(this, obj)) {
                return true;
            }

            if (obj.GetType() != GetType()) {
                return false;
            }

            return Equals((TextStyle) obj);
        }

        public override int GetHashCode() {
            unchecked {
                var hashCode = (color != null ? color.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ fontSize.GetHashCode();
                hashCode = (hashCode * 397) ^ (fontWeight != null ? fontWeight.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ fontStyle.GetHashCode();
                hashCode = (hashCode * 397) ^ letterSpacing.GetHashCode();
                hashCode = (hashCode * 397) ^ wordSpacing.GetHashCode();
                hashCode = (hashCode * 397) ^ textBaseline.GetHashCode();
                hashCode = (hashCode * 397) ^ height.GetHashCode();
                hashCode = (hashCode * 397) ^ (decoration != null ? decoration.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (decorationColor != null ? decorationColor.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ decorationStyle.GetHashCode();
                hashCode = (hashCode * 397) ^ (fontFamily != null ? fontFamily.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (shadows != null ? shadows.GetHashCode() : 0);
                return hashCode;
            }
        }

        public static bool operator ==(TextStyle left, TextStyle right) {
            return Equals(left, right);
        }

        public static bool operator !=(TextStyle left, TextStyle right) {
            return !Equals(left, right);
        }


        public TextStyle(Color color = null,
            float? fontSize = null,
            FontWeight fontWeight = null,
            FontStyle? fontStyle = null,
            float? letterSpacing = null,
            float? wordSpacing = null,
            TextBaseline? textBaseline = null,
            float? height = null,
            TextDecoration decoration = null,
            TextDecorationStyle? decorationStyle = null,
            Color decorationColor = null,
            string fontFamily = null,
            Paint foreground = null,
            Paint background = null,
            List<BoxShadow> shadows = null
        ) {
            this.color = color ?? this.color;
            this.fontSize = fontSize ?? this.fontSize;
            this.fontWeight = fontWeight ?? this.fontWeight;
            this.fontStyle = fontStyle ?? this.fontStyle;
            this.letterSpacing = letterSpacing ?? this.letterSpacing;
            this.wordSpacing = wordSpacing ?? this.wordSpacing;
            this.fontSize = fontSize ?? this.fontSize;
            this.textBaseline = textBaseline ?? this.textBaseline;
            this.height = height ?? this.height;
            this.decoration = decoration ?? this.decoration;
            this.decorationStyle = decorationStyle ?? this.decorationStyle;
            this.decorationColor = decorationColor ?? this.decorationColor;
            this.fontFamily = fontFamily ?? this.fontFamily;
            this.foreground = foreground ?? this.foreground;
            this.background = background ?? this.background;
            this.shadows = shadows ?? this.shadows;
        }
    }

    public class ParagraphStyle : IEquatable<ParagraphStyle> {
        public ParagraphStyle(TextAlign? textAlign = null,
            TextDirection? textDirection = null,
            FontWeight fontWeight = null,
            FontStyle? fontStyle = null,
            int? maxLines = null,
            float? fontSize = null,
            string fontFamily = null,
            float? height = null, // todo  
            string ellipsis = null,
            StrutStyle strutStyle = null) {
            this.textAlign = textAlign;
            this.textDirection = textDirection;
            this.fontWeight = fontWeight;
            this.fontStyle = fontStyle;
            this.maxLines = maxLines;
            this.fontSize = fontSize;
            this.fontFamily = fontFamily;
            this.height = height;
            this.ellipsis = ellipsis;
            this.strutStyle = strutStyle;
        }

        public bool Equals(ParagraphStyle other) {
            if (ReferenceEquals(null, other)) {
                return false;
            }

            if (ReferenceEquals(this, other)) {
                return true;
            }

            return textAlign == other.textAlign && textDirection == other.textDirection &&
                   fontWeight == other.fontWeight && fontStyle == other.fontStyle &&
                   maxLines == other.maxLines && fontSize.Equals(other.fontSize) &&
                   string.Equals(fontFamily, other.fontFamily) && height.Equals(other.height) &&
                   string.Equals(ellipsis, other.ellipsis);
        }

        public override bool Equals(object obj) {
            if (ReferenceEquals(null, obj)) {
                return false;
            }

            if (ReferenceEquals(this, obj)) {
                return true;
            }

            if (obj.GetType() != GetType()) {
                return false;
            }

            return Equals((ParagraphStyle) obj);
        }

        public static bool operator ==(ParagraphStyle a, ParagraphStyle b) {
            return Equals(a, b);
        }

        public static bool operator !=(ParagraphStyle a, ParagraphStyle b) {
            return !(a == b);
        }

        public override int GetHashCode() {
            unchecked {
                var hashCode = textAlign.GetHashCode();
                hashCode = (hashCode * 397) ^ textDirection.GetHashCode();
                hashCode = (hashCode * 397) ^ (fontWeight != null ? fontWeight.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ fontStyle.GetHashCode();
                hashCode = (hashCode * 397) ^ maxLines.GetHashCode();
                hashCode = (hashCode * 397) ^ fontSize.GetHashCode();
                hashCode = (hashCode * 397) ^ (fontFamily != null ? fontFamily.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ height.GetHashCode();
                hashCode = (hashCode * 397) ^ (ellipsis != null ? ellipsis.GetHashCode() : 0);
                return hashCode;
            }
        }

        internal TextStyle getTextStyle() {
            return new TextStyle(
                fontWeight: fontWeight,
                fontStyle: fontStyle,
                fontFamily: fontFamily,
                fontSize: fontSize,
                height: height
            );
        }

        public TextAlign TextAlign {
            get { return textAlign ?? TextAlign.left; }
        }

        public readonly TextAlign? textAlign;
        public readonly TextDirection? textDirection;
        public readonly FontWeight fontWeight;
        public readonly FontStyle? fontStyle;
        public readonly int? maxLines;
        public readonly float? fontSize;
        public readonly string fontFamily;
        public readonly float? height;
        public readonly string ellipsis;
        public readonly StrutStyle strutStyle;

        public bool ellipsized() {
            return !string.IsNullOrEmpty(ellipsis);
        }
    }

    public enum TextDecorationStyle {
        solid,
        doubleLine,
    }

    public class TextDecoration : IEquatable<TextDecoration> {
        public bool Equals(TextDecoration other) {
            if (ReferenceEquals(null, other)) {
                return false;
            }

            if (ReferenceEquals(this, other)) {
                return true;
            }

            return mask == other.mask;
        }

        public override bool Equals(object obj) {
            if (ReferenceEquals(null, obj)) {
                return false;
            }

            if (ReferenceEquals(this, obj)) {
                return true;
            }

            if (obj.GetType() != GetType()) {
                return false;
            }

            return Equals((TextDecoration) obj);
        }

        public override int GetHashCode() {
            return mask;
        }


        public static bool operator ==(TextDecoration a, TextDecoration b) {
            return Equals(a, b);
        }

        public static bool operator !=(TextDecoration a, TextDecoration b) {
            return !(a == b);
        }

        public static readonly TextDecoration none = new TextDecoration(0);

        public static readonly TextDecoration underline = new TextDecoration(1);

        public static readonly TextDecoration overline = new TextDecoration(2);

        public static readonly TextDecoration lineThrough = new TextDecoration(4);

        public readonly int mask;

        public TextDecoration(int mask) {
            this.mask = mask;
        }

        public bool contains(TextDecoration other) {
            return (mask | other.mask) == mask;
        }
    }

    public enum TextDirection {
        rtl,
        ltr,
    }

    public enum TextAffinity {
        upstream,
        downstream,
    }

    public class FontWeight : IEquatable<FontWeight> {
        FontWeight(int index) {
            this.index = index;
        }

        public readonly int index;

        public static readonly FontWeight w100 = new FontWeight(0); // Ultralight
        public static readonly FontWeight w200 = new FontWeight(1); // Thin
        public static readonly FontWeight w300 = new FontWeight(2); // Light
        public static readonly FontWeight w400 = new FontWeight(3); // Regular
        public static readonly FontWeight w500 = new FontWeight(4); // Medium
        public static readonly FontWeight w600 = new FontWeight(5); // Semibold
        public static readonly FontWeight w700 = new FontWeight(6); // Bold
        public static readonly FontWeight w800 = new FontWeight(7); // Heavy
        public static readonly FontWeight w900 = new FontWeight(8); // Black
        
        public static readonly FontWeight normal = w400;

        public static readonly FontWeight bold = w700;

        public static readonly List<FontWeight> values = new List<FontWeight> {
            w100, w200, w300, w400, w500, w600, w700, w800, w900
        };

        public static readonly Dictionary<int, int> indexToFontWeight = new Dictionary<int, int> {
            {0, 100},
            {1, 200},
            {2, 300},
            {3, 400},
            {4, 500},
            {5, 600},
            {6, 700},
            {7, 800},
            {8, 900},
        };

        public bool Equals(FontWeight other) {
            if (ReferenceEquals(null, other)) {
                return false;
            }

            if (ReferenceEquals(this, other)) {
                return true;
            }

            return index == other.index;
        }

        public override bool Equals(object obj) {
            if (ReferenceEquals(null, obj)) {
                return false;
            }

            if (ReferenceEquals(this, obj)) {
                return true;
            }

            if (obj.GetType() != GetType()) {
                return false;
            }

            return Equals((FontWeight) obj);
        }

        public override int GetHashCode() {
            return index;
        }

        public static bool operator ==(FontWeight left, FontWeight right) {
            return Equals(left, right);
        }

        public static bool operator !=(FontWeight left, FontWeight right) {
            return !Equals(left, right);
        }

        public override string ToString() {
            return $"FontWeight.w{weightValue}";
        }

        public int weightValue {
            get { return indexToFontWeight[index]; }
        }
    }


    public class TextPosition : IEquatable<TextPosition> {
        public readonly int offset;
        public readonly TextAffinity affinity;

        public TextPosition(int offset, TextAffinity affinity = TextAffinity.downstream) {
            this.offset = offset;
            this.affinity = affinity;
        }

        public override string ToString() {
            return $"Offset: {offset}, Affinity: {affinity}";
        }

        public bool Equals(TextPosition other) {
            if (ReferenceEquals(null, other)) {
                return false;
            }

            if (ReferenceEquals(this, other)) {
                return true;
            }

            return offset == other.offset && affinity == other.affinity;
        }

        public override bool Equals(object obj) {
            if (ReferenceEquals(null, obj)) {
                return false;
            }

            if (ReferenceEquals(this, obj)) {
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

        public static bool operator ==(TextPosition left, TextPosition right) {
            return Equals(left, right);
        }

        public static bool operator !=(TextPosition left, TextPosition right) {
            return !Equals(left, right);
        }
    }

    public struct TextBox : IEquatable<TextBox> {
        public readonly float left;

        public readonly float top;

        public readonly float right;

        public readonly float bottom;

        public readonly TextDirection direction;

        TextBox(float left, float top, float right, float bottom, TextDirection direction) {
            this.left = left;
            this.top = top;
            this.right = right;
            this.bottom = bottom;
            this.direction = direction;
        }

        public static TextBox fromLTBD(float left, float top, float right, float bottom, TextDirection direction) {
            return new TextBox(left, top, right, bottom, direction);
        }

        public Rect toRect() {
            return Rect.fromLTRB(left, top, right, bottom);
        }

        public float start {
            get { return direction == TextDirection.ltr ? left : right; }
        }

        public float end {
            get { return direction == TextDirection.ltr ? right : left; }
        }

        public bool Equals(TextBox other) {
            return left.Equals(other.left) && top.Equals(other.top) && right.Equals(other.right) &&
                   bottom.Equals(other.bottom) && direction == other.direction;
        }

        public override bool Equals(object obj) {
            if (ReferenceEquals(null, obj)) {
                return false;
            }

            if (obj.GetType() != GetType()) {
                return false;
            }

            return Equals((TextBox) obj);
        }

        public override string ToString() {
            return
                $"Left: {left}, Top: {top}, Right: {right}, Bottom: {bottom}, Direction: {direction}";
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

        public static bool operator ==(TextBox left, TextBox right) {
            return Equals(left, right);
        }

        public static bool operator !=(TextBox left, TextBox right) {
            return !Equals(left, right);
        }
    }
}