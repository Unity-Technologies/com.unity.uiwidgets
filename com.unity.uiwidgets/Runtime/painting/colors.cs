using System;
using System.Collections.Generic;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.ui;
using UnityEngine;
using Color = Unity.UIWidgets.ui.Color;

namespace Unity.UIWidgets.painting {
    public partial class painting_ {
        internal static float _getHue(float red, float green, float blue, float max, float delta) {
            float hue = 0;
            if (max == 0.0f || delta == 0.0f) {
                hue = 0.0f;
            }
            else if (foundation_.FloatEqual(max,red)) {
                hue = 60.0f * (((green - blue) / delta + 6) % 6);
            }
            else if (foundation_.FloatEqual(max,green)) {
                hue = 60.0f * (((blue - red) / delta) + 2);
            }
            else if (foundation_.FloatEqual(max,blue)) {
                hue = 60.0f * (((red - green) / delta) + 4);
            }

            return hue;
        }

        internal static Color _colorFromHue(
            float alpha,
            float hue,
            float chroma,
            float secondary,
            float match
        ) {
            float red;
            float green;
            float blue;
            if (hue < 60.0f) {
                red = chroma;
                green = secondary;
                blue = 0.0f;
            }
            else if (hue < 120.0f) {
                red = secondary;
                green = chroma;
                blue = 0.0f;
            }
            else if (hue < 180.0f) {
                red = 0.0f;
                green = chroma;
                blue = secondary;
            }
            else if (hue < 240.0f) {
                red = 0.0f;
                green = secondary;
                blue = chroma;
            }
            else if (hue < 300.0f) {
                red = secondary;
                green = 0.0f;
                blue = chroma;
            }
            else {
                red = chroma;
                green = 0.0f;
                blue = secondary;
            }

            return Color.fromARGB((alpha * 0xFF).round(), ((red + match) * 0xFF).round(),
                ((green + match) * 0xFF).round(), ((blue + match) * 0xFF).round());
        }
    }

    public class HSVColor {
        HSVColor(float alpha, float hue, float saturation, float value) {
            D.assert(this.alpha >= 0);
            D.assert(this.alpha <= 1);
            D.assert(hue >= 0);
            D.assert(hue <= 360);
            D.assert(saturation >= 0);
            D.assert(saturation <= 1);
            D.assert(value >= 0);
            D.assert(value <= 1);
            this.alpha = alpha;
            this.hue = hue;
            this.saturation = saturation;
            this.value = value;
        }

        public static HSVColor fromAHSV(float alpha, float hue, float saturation, float value) {
            return new HSVColor(alpha, hue, saturation, value);
        }
        
        public static HSVColor fromColor(Color color) {
            float red = (float)color.red / 0xFF;
            float green = (float)color.green / 0xFF;
            float blue = (float)color.blue / 0xFF;

            float max = Mathf.Max(red, Mathf.Max(green, blue));
            float min = Mathf.Min(red, Mathf.Min(green, blue));
            float delta = max - min;
            float alpha = (float)color.alpha / 0xFF;
            float hue = painting_._getHue(red, green, blue, max, delta);
            float saturation = max == 0.0f ? 0.0f : delta / max;
            
            return new HSVColor(alpha, hue, saturation, max);
        }

        public HSVColor withAlpha(float alpha) {
            return fromAHSV(alpha, hue, saturation, value);
        }

        public HSVColor withHue(float hue) {
            return fromAHSV(alpha, hue, saturation, value);
        }

        public HSVColor withSaturation(float saturation) {
            return fromAHSV(alpha, hue, saturation, value);
        }

        public HSVColor withValue(float value) {
            return fromAHSV(alpha, hue, saturation, value);
        }

        public Color toColor() {
            float chroma = saturation * value;
            float secondary = chroma * (1.0f - (((hue / 60.0f) % 2.0f) - 1.0f).abs());
            float match = value - chroma;

            return painting_._colorFromHue(alpha, hue, chroma, secondary, match);
        }

        public readonly float alpha;
        public readonly float hue;
        public readonly float saturation;
        public readonly float value;

        public override string ToString() {
            return $"{foundation_.objectRuntimeType(this, "HSVColor")}({alpha}, {hue}, {saturation},{value})";
        }
    }

    public class HSLColor : IEquatable<HSLColor> {
        private HSLColor(float alpha,
            float hue,
            float saturation,
            float lightness) {
            this.alpha = alpha;
            this.hue = hue;
            this.saturation = saturation;
            this.lightness = lightness;
        }

        public static HSLColor fromAHSL(float alpha, float hue, float saturation, float lightness) {
            D.assert(alpha >= 0.0f);
            D.assert(alpha <= 1.0f);
            D.assert(hue >= 0.0f);
            D.assert(hue <= 360.0f);
            D.assert(saturation >= 0.0f);
            D.assert(saturation <= 1.0f);
            D.assert(lightness >= 0.0f);
            D.assert(lightness <= 1.0f);
            return new HSLColor(alpha, hue, saturation, lightness);
        }

        public static HSLColor fromColor(Color color) {
            float red = (float)color.red / 0xFF;
            float green = (float)color.green / 0xFF;
            float blue = (float)color.blue / 0xFF;

            float max = Mathf.Max(red, Mathf.Max(green, blue));
            float min = Mathf.Min(red, Mathf.Min(green, blue));
            float delta = max - min;

            float alpha = (float)color.alpha / 0xFF;
            float hue = painting_._getHue(red, green, blue, max, delta);
            float lightness = (max + min) / 2.0f;
            float saturation = foundation_.FloatEqual(lightness, 1.0f)
                ? 0.0f
                : ((delta / (1.0f - Mathf.Abs(2.0f * lightness - 1.0f))).clamp(0.0f, 1.0f));
            return HSLColor.fromAHSL(alpha, hue, saturation, lightness);
        }

        public readonly float alpha;

        public readonly float hue;

        public readonly float saturation;

        public readonly float lightness;

        HSLColor withAlpha(float alpha) {
            return HSLColor.fromAHSL(alpha, hue, saturation, lightness);
        }

        HSLColor withHue(float hue) {
            return HSLColor.fromAHSL(alpha, hue, saturation, lightness);
        }

        public HSLColor withSaturation(float saturation) {
            return HSLColor.fromAHSL(alpha, hue, saturation, lightness);
        }

        public HSLColor withLightness(float lightness) {
            return HSLColor.fromAHSL(alpha, hue, saturation, lightness);
        }

        public Color toColor() {
            float chroma = Mathf.Abs(1.0f - (2.0f * lightness - 1.0f) * saturation);
            float secondary = chroma * (1.0f - Mathf.Abs(((hue / 60.0f) % 2.0f) - 1.0f));
            float match = lightness - chroma / 2.0f;

            return painting_._colorFromHue(alpha, hue, chroma, secondary, match);
        }

        HSLColor _scaleAlpha(float factor) {
            return withAlpha(alpha * factor);
        }

        static HSLColor lerp(HSLColor a, HSLColor b, float t) {
            if (a == null && b == null)
                return null;
            if (a == null)
                return b._scaleAlpha(t);
            if (b == null)
                return a._scaleAlpha(1.0f - t);
            return HSLColor.fromAHSL(
                MathUtils.lerpNullableFloat(a.alpha, b.alpha, t).clamp(0.0f, 1.0f),
                MathUtils.lerpNullableFloat(a.hue, b.hue, t) % 360.0f,
                MathUtils.lerpNullableFloat(a.saturation, b.saturation, t).clamp(0.0f, 1.0f),
                MathUtils.lerpNullableFloat(a.lightness, b.lightness, t).clamp(0.0f, 1.0f)
            );
        }

        public override string ToString() {
            return $"{foundation_.objectRuntimeType(this, "HSLColor")}({alpha}, {hue}, {saturation}, {lightness})";
        }


        public static bool operator ==(HSLColor left, HSLColor right) {
            return Equals(left, right);
        }

        public static bool operator !=(HSLColor left, HSLColor right) {
            return !Equals(left, right);
        }

        public bool Equals(HSLColor other) {
            if (ReferenceEquals(null, other)) {
                return false;
            }

            if (ReferenceEquals(this, other)) {
                return true;
            }

            return alpha.Equals(other.alpha) && hue.Equals(other.hue) && saturation.Equals(other.saturation) &&
                   lightness.Equals(other.lightness);
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

            return Equals((HSLColor) obj);
        }

        public override int GetHashCode() {
            unchecked {
                var hashCode = alpha.GetHashCode();
                hashCode = (hashCode * 397) ^ hue.GetHashCode();
                hashCode = (hashCode * 397) ^ saturation.GetHashCode();
                hashCode = (hashCode * 397) ^ lightness.GetHashCode();
                return hashCode;
            }
        }
    }


    public class ColorSwatch<T> : Color {
        public ColorSwatch(
            uint primary,
            Dictionary<T, Color> swatch) : base(primary) {
            _swatch = swatch;
        }

        protected readonly Dictionary<T, Color> _swatch;

        public Color this[T index] {
            get { return _swatch[index]; }
        }


        public bool Equals(ColorSwatch<T> other) {
            if (ReferenceEquals(null, other)) {
                return false;
            }

            if (ReferenceEquals(this, other)) {
                return true;
            }

            return value == other.value && _swatch == other._swatch;
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

            return Equals((ColorSwatch<T>) obj);
        }

        public static bool operator ==(ColorSwatch<T> left, ColorSwatch<T> right) {
            return Equals(left, right);
        }

        public static bool operator !=(ColorSwatch<T> left, ColorSwatch<T> right) {
            return !Equals(left, right);
        }

        public override int GetHashCode() {
            unchecked {
                var hashCode = (int) value;
                hashCode = (hashCode * 397) ^ _swatch.GetHashCode();
                return hashCode;
            }
        }

        public override string ToString() {
            return GetType() + "(primary value: " + base.ToString() + ")";
        }
    }

    public class ColorProperty : DiagnosticsProperty<Color> {
        public ColorProperty(
            string name = "",
            Color value = null,
            bool showName = true,
            object defaultValue = null,
            DiagnosticsTreeStyle style = DiagnosticsTreeStyle.singleLine,
            DiagnosticLevel level = DiagnosticLevel.info
        ) : base(name, value,
            defaultValue: defaultValue,
            showName: showName,
            style: style,
            level: level
        ) {
        }

        public override Dictionary<string, object> toJsonMap(DiagnosticsSerializationDelegate Delegate) {
            Dictionary<string, object> json = base.toJsonMap(Delegate);
            if (value != null) {
                json["valueProperties"] = new Dictionary<string, object> {
                    {"red", valueT.red},
                    {"green", valueT.green},
                    {"blue", valueT.blue},
                    {"alpha", valueT.alpha}
                };
            }

            return json;
        }
    }
}