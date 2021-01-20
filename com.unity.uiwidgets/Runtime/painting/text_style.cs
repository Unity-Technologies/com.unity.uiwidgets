using System;
using System.Collections.Generic;
using System.Linq;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.ui;
using UnityEngine;
using Color = Unity.UIWidgets.ui.Color;
using FontStyle = Unity.UIWidgets.ui.FontStyle;

namespace Unity.UIWidgets.painting {
    public class TextStyle : Diagnosticable, IEquatable<TextStyle> {
        public static readonly float _defaultFontSize = 14.0f;
        public readonly bool inherit;
        public readonly Color color;
        public readonly Color backgroundColor;
        public readonly float? fontSize;
        public readonly FontWeight fontWeight;
        public readonly FontStyle? fontStyle;
        public readonly float? letterSpacing;
        public readonly float? wordSpacing;
        public readonly TextBaseline? textBaseline;
        public readonly float? height;
        public readonly TextDecoration decoration;
        public readonly Color decorationColor;
        public readonly TextDecorationStyle? decorationStyle;
        public readonly float? decorationThickness;
        public readonly Paint foreground;
        public readonly Paint background;
        public readonly string fontFamily;
        public readonly List<BoxShadow> shadows;
        public readonly List<FontFeature> fontFeatures;

        public List<string> fontFamilyFallback {
            get { return _fontFamilyFallback; }
        }

        readonly List<string> _fontFamilyFallback;
        public readonly string debugLabel;

        const string _kDefaultDebugLabel = "unknown";

        const string _kColorForegroundWarning = "Cannot provide both a color and a foreground\n" +
                                                "The color argument is just a shorthand for 'foreground: new Paint()..color = color'.";

        const string _kColorBackgroundWarning = "Cannot provide both a backgroundColor and a background\n" +
                                                "The backgroundColor argument is just a shorthand for 'background: new Paint()..color = color'.";

        public TextStyle(bool inherit = true,
            Color color = null,
            Color backgroundColor = null,
            float? fontSize = null,
            FontWeight fontWeight = null,
            FontStyle? fontStyle = null,
            float? letterSpacing = null,
            float? wordSpacing = null,
            TextBaseline? textBaseline = null,
            float? height = null,
            Paint foreground = null,
            Paint background = null,
            TextDecoration decoration = null,
            Color decorationColor = null,
            TextDecorationStyle? decorationStyle = null,
            float? decorationThickness = null,
            string fontFamily = null,
            List<string> fontFamilyFallback = null,
            List<BoxShadow> shadows = null,
            List<FontFeature> fontFeatures = null,
            string debugLabel = null) {
            D.assert(color == null || foreground == null, () => _kColorForegroundWarning);
            D.assert(backgroundColor == null || background == null, () => _kColorBackgroundWarning);
            this.inherit = inherit;
            this.color = color;
            this.backgroundColor = backgroundColor;
            this.fontSize = fontSize;
            this.fontWeight = fontWeight;
            this.fontStyle = fontStyle;
            this.letterSpacing = letterSpacing;
            this.wordSpacing = wordSpacing;
            this.textBaseline = textBaseline;
            this.height = height;
            this.decoration = decoration;
            this.decorationColor = decorationColor;
            this.decorationStyle = decorationStyle;
            this.decorationThickness = decorationThickness;
            this.fontFamily = fontFamily;
            _fontFamilyFallback = fontFamilyFallback;
            this.debugLabel = debugLabel;
            this.foreground = foreground;
            this.background = background;
            this.shadows = shadows;
            this.fontFeatures = fontFeatures;
        }

        public RenderComparison compareTo(TextStyle other) {
            if (ReferenceEquals(this, other))
                return RenderComparison.identical;
            if (inherit != other.inherit ||
                fontFamily != other.fontFamily ||
                fontSize != other.fontSize ||
                fontWeight != other.fontWeight ||
                fontStyle != other.fontStyle ||
                letterSpacing != other.letterSpacing ||
                wordSpacing != other.wordSpacing ||
                textBaseline != other.textBaseline ||
                height != other.height ||
                foreground != other.foreground ||
                background != other.background ||
                !shadows.equalsList(other.shadows) ||
                !fontFeatures.equalsList(other.fontFeatures) ||
                !fontFamilyFallback.equalsList(other.fontFamilyFallback)) {
                return RenderComparison.layout;
            }

            if (color != other.color ||
                backgroundColor != other.backgroundColor ||
                decoration != other.decoration ||
                decorationColor != other.decorationColor ||
                decorationStyle != other.decorationStyle) {
                return RenderComparison.paint;
            }

            return RenderComparison.identical;
        }

        public ParagraphStyle getParagraphStyle(
            TextAlign textAlign,
            TextDirection textDirection,
            float? textScaleFactor = 1.0f,
            string ellipsis = null,
            int? maxLines = null,
            TextHeightBehavior textHeightBehavior = null,
            Locale locale = null,
            String fontFamily = null,
            float? fontSize = null,
            FontWeight fontWeight = null,
            FontStyle? fontStyle = null,
            float? height = null,
            StrutStyle strutStyle = null
        ) {
            D.assert(textScaleFactor != null);
            D.assert(maxLines == null || maxLines > 0);
            return new ui.ParagraphStyle(
                textAlign: textAlign,
                textDirection: textDirection,
                fontWeight: fontWeight ?? this.fontWeight,
                fontStyle: fontStyle ?? this.fontStyle,
                fontFamily: fontFamily ?? this.fontFamily,
                fontSize: (fontSize ?? this.fontSize ?? _defaultFontSize) * textScaleFactor,
                height: height ?? this.height,
                textHeightBehavior: textHeightBehavior,
                strutStyle: strutStyle == null
                    ? null
                    : new ui.StrutStyle(
                        fontFamily: strutStyle.fontFamily,
                        fontFamilyFallback: strutStyle.fontFamilyFallback,
                        fontSize: strutStyle.fontSize == null ? null : strutStyle.fontSize * textScaleFactor,
                        height: strutStyle.height,
                        leading: strutStyle.leading,
                        fontWeight: strutStyle.fontWeight,
                        fontStyle: strutStyle.fontStyle,
                        forceStrutHeight: strutStyle.forceStrutHeight
                    ),
                maxLines: maxLines,
                ellipsis: ellipsis,
                locale: locale
            );
        }


        public TextStyle apply(
            Color color = null,
            Color backgroundColor = null,
            TextDecoration decoration = null,
            Color decorationColor = null,
            TextDecorationStyle? decorationStyle = null,
            float decorationThicknessFactor = 1.0f,
            float decorationThicknessDelta = 0.0f,
            string fontFamily = null,
            List<string> fontFamilyFallback = null,
            List<BoxShadow> shadows = null,
            float fontSizeFactor = 1.0f,
            float fontSizeDelta = 0.0f,
            int fontWeightDelta = 0,
            float letterSpacingFactor = 1.0f,
            float letterSpacingDelta = 0.0f,
            float wordSpacingFactor = 1.0f,
            float wordSpacingDelta = 0.0f,
            float heightFactor = 1.0f,
            float heightDelta = 0.0f
        ) {
            D.assert(fontSize != null || (fontSizeFactor == 1.0f && fontSizeDelta == 0.0f));
            D.assert(fontWeight != null || fontWeightDelta == 0.0f);
            D.assert(letterSpacing != null || (letterSpacingFactor == 1.0f && letterSpacingDelta == 0.0f));
            D.assert(wordSpacing != null || (wordSpacingFactor == 1.0f && wordSpacingDelta == 0.0f));
            D.assert(height != null || (heightFactor == 1.0f && heightDelta == 0.0f));
            D.assert(decorationThickness != null ||
                     (decorationThicknessFactor == 1.0f && decorationThicknessDelta == 0.0f));

            string modifiedDebugLabel = "";
            D.assert(() => {
                if (debugLabel != null) {
                    modifiedDebugLabel = debugLabel + ".apply";
                }

                return true;
            });

            return new TextStyle(
                inherit: inherit,
                color: foreground == null ? color ?? this.color : null,
                backgroundColor: background == null ? backgroundColor ?? this.backgroundColor : null,
                fontFamily: fontFamily ?? this.fontFamily,
                fontFamilyFallback: fontFamilyFallback ?? this.fontFamilyFallback,
                fontSize: fontSize == null ? null : fontSize * fontSizeFactor + fontSizeDelta,
                fontWeight: fontWeight == null ? null : fontWeight,
                fontStyle: fontStyle,
                letterSpacing: letterSpacing == null
                    ? null
                    : letterSpacing * letterSpacingFactor + letterSpacingDelta,
                wordSpacing: wordSpacing == null ? null : wordSpacing * wordSpacingFactor + wordSpacingDelta,
                textBaseline: textBaseline,
                height: height == null ? null : height * heightFactor + heightDelta,
                foreground: foreground,
                background: background,
                decoration: decoration ?? this.decoration,
                decorationColor: decorationColor ?? this.decorationColor,
                decorationStyle: decorationStyle ?? this.decorationStyle,
                decorationThickness: decorationThickness == null
                    ? null
                    : decorationThickness * decorationThicknessFactor + decorationThicknessDelta,
                shadows: shadows ?? this.shadows,
                fontFeatures: fontFeatures,
                debugLabel: modifiedDebugLabel
            );
        }

        public TextStyle merge(TextStyle other) {
            if (other == null) {
                return this;
            }

            if (!other.inherit) {
                return other;
            }

            string mergedDebugLabel = null;
            D.assert(() => {
                if (other.debugLabel != null || debugLabel != null) {
                    mergedDebugLabel =
                        $"({debugLabel ?? _kDefaultDebugLabel}).merge({other.debugLabel ?? _kDefaultDebugLabel})";
                }

                return true;
            });

            return copyWith(
                color: other.color,
                backgroundColor: other.backgroundColor,
                fontFamily: other.fontFamily,
                fontFamilyFallback: other.fontFamilyFallback,
                fontSize: other.fontSize,
                fontWeight: other.fontWeight,
                fontStyle: other.fontStyle,
                letterSpacing: other.letterSpacing,
                wordSpacing: other.wordSpacing,
                textBaseline: other.textBaseline,
                height: other.height,
                foreground: other.foreground,
                background: other.background,
                decoration: other.decoration,
                decorationColor: other.decorationColor,
                decorationStyle: other.decorationStyle,
                decorationThickness: other.decorationThickness,
                shadows: other.shadows,
                fontFeatures: other.fontFeatures,
                debugLabel: mergedDebugLabel
            );
        }

        public TextStyle copyWith(
            bool? inherit = null,
            Color color = null,
            Color backgroundColor = null,
            string fontFamily = null,
            List<string> fontFamilyFallback = null,
            float? fontSize = null,
            FontWeight fontWeight = null,
            FontStyle? fontStyle = null,
            float? letterSpacing = null,
            float? wordSpacing = null,
            TextBaseline? textBaseline = null,
            float? height = null,
            Paint foreground = null,
            Paint background = null,
            TextDecoration decoration = null,
            Color decorationColor = null,
            TextDecorationStyle? decorationStyle = null,
            float? decorationThickness = null,
            List<BoxShadow> shadows = null,
            List<FontFeature> fontFeatures = null,
            string debugLabel = null) {
            D.assert(color == null || foreground == null, () => _kColorForegroundWarning);
            D.assert(backgroundColor == null || background == null, () => _kColorBackgroundWarning);
            string newDebugLabel = null;
            D.assert(() => {
                if (this.debugLabel != null) {
                    newDebugLabel = debugLabel ?? $"({this.debugLabel}).copyWith";
                }

                return true;
            });

            return new TextStyle(
                inherit: inherit ?? this.inherit,
                color: this.foreground == null && foreground == null ? color ?? this.color : null,
                backgroundColor: this.background == null && background == null ? backgroundColor ?? this.backgroundColor : null,
                fontFamily: fontFamily ?? this.fontFamily,
                fontFamilyFallback: fontFamilyFallback ?? this.fontFamilyFallback,
                fontSize: fontSize ?? this.fontSize,
                fontWeight: fontWeight ?? this.fontWeight,
                fontStyle: fontStyle ?? this.fontStyle,
                letterSpacing: letterSpacing ?? this.letterSpacing,
                wordSpacing: wordSpacing ?? this.wordSpacing,
                textBaseline: textBaseline ?? this.textBaseline,
                height: height ?? this.height,
                decoration: decoration ?? this.decoration,
                decorationColor: decorationColor ?? this.decorationColor,
                decorationStyle: decorationStyle ?? this.decorationStyle,
                decorationThickness: decorationThickness ?? this.decorationThickness,
                foreground: foreground ?? this.foreground,
                background: background ?? this.background,
                shadows: shadows ?? this.shadows,
                fontFeatures: fontFeatures ?? this.fontFeatures,
                debugLabel: newDebugLabel
            );
        }

        public static TextStyle lerp(TextStyle a, TextStyle b, float t) {
            D.assert(a == null || b == null || a.inherit == b.inherit);
            if (a == null && b == null) {
                return null;
            }

            string lerpDebugLabel = "";
            D.assert(() => {
                lerpDebugLabel = "lerp" + (a?.debugLabel ?? _kDefaultDebugLabel) + "-" + t + "-" +
                                 (b?.debugLabel ?? _kDefaultDebugLabel);
                return true;
            });

            if (a == null) {
                return new TextStyle(
                    inherit: b.inherit,
                    color: Color.lerp(null, b.color, t),
                    backgroundColor: Color.lerp(null, b.backgroundColor, t),
                    fontFamily: t < 0.5f ? null : b.fontFamily,
                    fontFamilyFallback: t < 0.5f ? null : b.fontFamilyFallback,
                    fontSize: t < 0.5f ? null : b.fontSize,
                    fontWeight: t < 0.5f ? null : b.fontWeight,
                    fontStyle: t < 0.5f ? null : b.fontStyle,
                    letterSpacing: t < 0.5f ? null : b.letterSpacing,
                    wordSpacing: t < 0.5f ? null : b.wordSpacing,
                    textBaseline: t < 0.5f ? null : b.textBaseline,
                    height: t < 0.5f ? null : b.height,
                    foreground: t < 0.5f ? null : b.foreground,
                    background: t < 0.5f ? null : b.background,
                    decoration: t < 0.5f ? null : b.decoration,
                    decorationColor: Color.lerp(null, b.decorationColor, t),
                    decorationStyle: t < 0.5f ? null : b.decorationStyle,
                    decorationThickness: t < 0.5f ? null : b.decorationThickness,
                    shadows: t < 0.5f ? null : b.shadows,
                    fontFeatures: t < 0.5 ? null : b.fontFeatures,
                    debugLabel: lerpDebugLabel
                );
            }

            if (b == null) {
                return new TextStyle(
                    inherit: a.inherit,
                    color: Color.lerp(a.color, null, t),
                    backgroundColor: Color.lerp(a.backgroundColor, null, t),
                    fontFamily: t < 0.5f ? a.fontFamily : null,
                    fontFamilyFallback: t < 0.5f ? a.fontFamilyFallback : null,
                    fontSize: t < 0.5f ? a.fontSize : null,
                    fontWeight: t < 0.5f ? a.fontWeight : null,
                    fontStyle: t < 0.5f ? a.fontStyle : null,
                    letterSpacing: t < 0.5f ? a.letterSpacing : null,
                    wordSpacing: t < 0.5f ? a.wordSpacing : null,
                    textBaseline: t < 0.5f ? a.textBaseline : null,
                    height: t < 0.5f ? a.height : null,
                    foreground: t < 0.5f ? a.foreground : null,
                    background: t < 0.5f ? a.background : null,
                    decoration: t < 0.5f ? a.decoration : null,
                    decorationColor: Color.lerp(a.decorationColor, null, t),
                    decorationStyle: t < 0.5f ? a.decorationStyle : null,
                    decorationThickness: t < 0.5f ? a.decorationThickness : null,
                    shadows: t < 0.5f ? a.shadows : null,
                    fontFeatures: t < 0.5 ? a.fontFeatures : null,
                    debugLabel: lerpDebugLabel
                );
            }

            return new TextStyle(
                inherit: b.inherit,
                color: a.foreground == null && b.foreground == null ? Color.lerp(a.color, b.color, t) : null,
                backgroundColor: a.background == null && b.background == null
                    ? Color.lerp(a.backgroundColor, b.backgroundColor, t)
                    : null,
                fontFamily: t < 0.5 ? a.fontFamily : b.fontFamily,
                fontFamilyFallback: t < 0.5 ? a.fontFamilyFallback : b.fontFamilyFallback,
                fontSize: MathUtils.lerpNullableFloat(a.fontSize ?? b.fontSize, b.fontSize ?? a.fontSize, t),
                fontWeight: t < 0.5 ? a.fontWeight : b.fontWeight,
                fontStyle: t < 0.5 ? a.fontStyle : b.fontStyle,
                letterSpacing: MathUtils.lerpNullableFloat(a.letterSpacing ?? b.letterSpacing,
                    b.letterSpacing ?? a.letterSpacing, t),
                wordSpacing: MathUtils.lerpNullableFloat(a.wordSpacing ?? b.wordSpacing,
                    b.wordSpacing ?? a.wordSpacing, t),
                textBaseline: t < 0.5 ? a.textBaseline : b.textBaseline,
                height: MathUtils.lerpNullableFloat(a.height ?? b.height, b.height ?? a.height, t),
                foreground: (a.foreground != null || b.foreground != null)
                    ? t < 0.5
                        ? a.foreground ?? new Paint() {color = a.color}
                        : b.foreground ?? new Paint() {color = b.color}
                    : null,
                background: (a.background != null || b.background != null)
                    ? t < 0.5
                        ? a.background ?? new Paint() {color = a.backgroundColor}
                        : b.background ?? new Paint() {color = b.backgroundColor}
                    : null,
                decoration: t < 0.5 ? a.decoration : b.decoration,
                decorationColor: Color.lerp(a.decorationColor, b.decorationColor, t),
                decorationStyle: t < 0.5 ? a.decorationStyle : b.decorationStyle,
                decorationThickness: Mathf.Lerp(
                    a.decorationThickness ?? b.decorationThickness ?? 0.0f,
                    b.decorationThickness ?? a.decorationThickness ?? 0.0f, t),
                shadows: t < 0.5f ? a.shadows : b.shadows,
                fontFeatures: t < 0.5 ? a.fontFeatures : b.fontFeatures,
                debugLabel: lerpDebugLabel
            );
        }

        ui.ParagraphStyle getParagraphStyle(
            TextAlign? textAlign = null,
            TextDirection? textDirection = null,
            float textScaleFactor = 1.0f,
            String ellipsis = null,
            int? maxLines = null,
            ui.TextHeightBehavior textHeightBehavior = null,
            Locale locale = null,
            String fontFamily = null,
            float? fontSize = null,
            FontWeight fontWeight = null,
            FontStyle? fontStyle = null,
            float? height = null,
            StrutStyle strutStyle = null
        ) {
            D.assert(maxLines == null || maxLines > 0);
            return new ui.ParagraphStyle(
                textAlign: textAlign,
                textDirection: textDirection,
                fontWeight: fontWeight ?? this.fontWeight,
                fontStyle: fontStyle ?? this.fontStyle,
                fontFamily: fontFamily ?? this.fontFamily,
                fontSize: (fontSize ?? this.fontSize ?? _defaultFontSize) * textScaleFactor,
                height: height ?? this.height,
                textHeightBehavior: textHeightBehavior,
                strutStyle: strutStyle == null
                    ? null
                    : new ui.StrutStyle(
                        fontFamily: strutStyle.fontFamily,
                        fontFamilyFallback: strutStyle.fontFamilyFallback,
                        fontSize: strutStyle.fontSize == null ? null : strutStyle.fontSize * textScaleFactor,
                        height: strutStyle.height,
                        leading: strutStyle.leading,
                        fontWeight: strutStyle.fontWeight,
                        fontStyle: strutStyle.fontStyle,
                        forceStrutHeight: strutStyle.forceStrutHeight
                    ),
                maxLines: maxLines,
                ellipsis: ellipsis,
                locale: locale
            );
        }

        public ui.TextStyle getTextStyle(float textScaleFactor = 1.0f) {
            var backgroundPaint = new Paint();
            if (background != null) {
                backgroundPaint = new Paint();
                backgroundPaint.color = backgroundColor;
            }

            return new ui.TextStyle(
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
                fontSize: fontSize == null ? null : fontSize * textScaleFactor,
                letterSpacing: letterSpacing,
                wordSpacing: wordSpacing,
                height: height,
                // locale: locale,
                foreground: foreground,
                background: background ?? (backgroundColor != null
                    ? backgroundPaint
                    : null
                ),
                shadows: shadows?.Cast<Shadow>().ToList(),
                fontFeatures: fontFeatures
            );
        }

        public override void debugFillProperties(DiagnosticPropertiesBuilder properties) { 
            debugFillProperties(properties, "");
        }

        public void debugFillProperties(DiagnosticPropertiesBuilder properties, string prefix = "") {
            base.debugFillProperties(properties);

            List<DiagnosticsNode> styles = new List<DiagnosticsNode>();
            styles.Add(new ColorProperty($"{prefix}color", color,
                defaultValue: foundation_.kNullDefaultValue));
            styles.Add(new ColorProperty($"{prefix}backgroundColor", backgroundColor,
                defaultValue: foundation_.kNullDefaultValue));
            styles.Add(new StringProperty($"{prefix}family", fontFamily, defaultValue: foundation_.kNullDefaultValue,
                quoted: false));
            styles.Add(new EnumerableProperty<string>($"{prefix}familyFallback", fontFamilyFallback,
                defaultValue: foundation_.kNullDefaultValue));
            styles.Add(new DiagnosticsProperty<float?>($"{prefix}size", fontSize,
                defaultValue: foundation_.kNullDefaultValue));
            string weightDescription = "";
            if (fontWeight != null) {
                weightDescription = $"{fontWeight.index + 1}00";
            }

            styles.Add(new DiagnosticsProperty<FontWeight>(
                "weight", fontWeight,
                description: weightDescription,
                defaultValue: foundation_.kNullDefaultValue
            ));
            styles.Add(new EnumProperty<FontStyle?>($"{prefix}style", fontStyle,
                defaultValue: foundation_.kNullDefaultValue));
            styles.Add(new DiagnosticsProperty<float?>($"{prefix}letterSpacing", letterSpacing,
                defaultValue: foundation_.kNullDefaultValue));
            styles.Add(new DiagnosticsProperty<float?>($"{prefix}wordSpacing", wordSpacing,
                defaultValue: foundation_.kNullDefaultValue));
            styles.Add(new EnumProperty<TextBaseline?>($"{prefix}baseline", textBaseline,
                defaultValue: foundation_.kNullDefaultValue));
            styles.Add(new DiagnosticsProperty<float?>($"{prefix}height", height,
                defaultValue: foundation_.kNullDefaultValue));
            styles.Add(new StringProperty($"{prefix}foreground", foreground == null ? null : foreground.ToString(),
                defaultValue: foundation_.kNullDefaultValue, quoted: false));
            styles.Add(new StringProperty($"{prefix}background", background == null ? null : background.ToString(),
                defaultValue: foundation_.kNullDefaultValue, quoted: false));
            if (decoration != null) {
                List<string> decorationDescription = new List<string>();
                if (decorationStyle != null) {
                    decorationDescription.Add(decorationStyle.ToString());
                }

                styles.Add(new ColorProperty($"{prefix}decorationColor", decorationColor,
                    defaultValue: foundation_.kNullDefaultValue,
                    level: DiagnosticLevel.fine));
                if (decorationColor != null) {
                    decorationDescription.Add(decorationColor.ToString());
                }

                styles.Add(new DiagnosticsProperty<TextDecoration>($"{prefix}decoration", decoration,
                    defaultValue: foundation_.kNullDefaultValue,
                    level: DiagnosticLevel.hidden));
                if (decoration != null) {
                    decorationDescription.Add($"{decoration}");
                }

                D.assert(decorationDescription.isNotEmpty);
                styles.Add(new MessageProperty($"{prefix}decoration", string.Join(" ", decorationDescription.ToArray())));
                styles.Add(new FloatProperty($"{prefix}decorationThickness", decorationThickness, unit: "x",
                    defaultValue: foundation_.kNoDefaultValue));
            }

            bool styleSpecified = styles.Any((DiagnosticsNode n) => !n.isFiltered(DiagnosticLevel.info));
            properties.add(new DiagnosticsProperty<bool>("inherit", inherit,
                level: (!styleSpecified && inherit) ? DiagnosticLevel.fine : DiagnosticLevel.info));
            foreach (var style in styles) {
                properties.add(style);
            }

            if (!styleSpecified) {
                properties.add(new FlagProperty("inherit", value: inherit, ifTrue: $"{prefix}<all styles inherited>",
                    ifFalse: $"{prefix}<no style specified>"));
            }
        }

        public bool Equals(TextStyle other) {
            if (ReferenceEquals(null, other)) {
                return false;
            }

            if (ReferenceEquals(this, other)) {
                return true;
            }

            return inherit == other.inherit &&
                   Equals(color, other.color) &&
                   Equals(backgroundColor, other.backgroundColor) &&
                   fontSize.Equals(other.fontSize) &&
                   fontWeight == other.fontWeight &&
                   fontStyle == other.fontStyle &&
                   letterSpacing.Equals(other.letterSpacing) &&
                   wordSpacing.Equals(other.wordSpacing) &&
                   textBaseline == other.textBaseline &&
                   height.Equals(other.height) &&
                   Equals(decoration, other.decoration) &&
                   Equals(decorationColor, other.decorationColor) &&
                   decorationStyle == other.decorationStyle &&
                   decorationThickness == other.decorationThickness &&
                   Equals(foreground, other.foreground) &&
                   Equals(background, other.background) &&
                   fontFamilyFallback.equalsList(other.fontFamilyFallback) &&
                   shadows.equalsList(other.shadows) &&
                   fontFeatures.equalsList(other.fontFeatures) &&
                   string.Equals(fontFamily, other.fontFamily);
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
                var hashCode = inherit.GetHashCode();
                hashCode = (hashCode * 397) ^ (color != null ? color.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (backgroundColor != null ? backgroundColor.GetHashCode() : 0);
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
                hashCode = (hashCode * 397) ^ decorationThickness.GetHashCode();
                hashCode = (hashCode * 397) ^ (foreground != null ? foreground.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (background != null ? background.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (fontFamily != null ? fontFamily.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^
                           (fontFamilyFallback != null ? fontFamilyFallback.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (shadows != null ? shadows.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (fontFeatures != null ? fontFeatures.GetHashCode() : 0);
                return hashCode;
            }
        }

        public static bool operator ==(TextStyle left, TextStyle right) {
            return Equals(left, right);
        }

        public static bool operator !=(TextStyle left, TextStyle right) {
            return !Equals(left, right);
        }

        public override string toStringShort() {
            return GetType().FullName;
        }
    }
}