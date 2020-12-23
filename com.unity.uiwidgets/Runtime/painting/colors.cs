using System.Collections.Generic;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.ui;

namespace Unity.UIWidgets.painting {
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

            return ColorUtils._colorFromHue(alpha, hue, chroma, secondary, match);
        }

        public readonly float alpha;
        public readonly float hue;
        public readonly float saturation;
        public readonly float value;
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
            D.assert(showName != null);
            D.assert(style != null);
            D.assert(level != null);
            
        } 
        public override Dictionary<string, object> toJsonMap(DiagnosticsSerializationDelegate  Delegate) {
            Dictionary<string, object> json = base.toJsonMap(Delegate);
            if (value != null) {
                json["valueProperties"] = new Dictionary<string, object> {
                    {"red", value.red},
                    {"green", value.green},
                    {"blue", value.blue},
                    {"alpha", value.alpha}
                };
            }
            return json;
            }
        }
    public static class ColorUtils {
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
            if (hue < 60.0) {
                red = chroma;
                green = secondary;
                blue = 0.0f;
            }
            else if (hue < 120.0) {
                red = secondary;
                green = chroma;
                blue = 0.0f;
            }
            else if (hue < 180.0) {
                red = 0.0f;
                green = chroma;
                blue = secondary;
            }
            else if (hue < 240.0) {
                red = 0.0f;
                green = secondary;
                blue = chroma;
            }
            else if (hue < 300.0) {
                red = secondary;
                green = 0.0f;
                blue = chroma;
            }
            else {
                red = chroma;
                green = 0.0f;
                blue = secondary;
            }

            return Color.fromARGB((alpha * 0xFF).round(),
                ((red + match) * 0xFF).round(),
                ((green + match) * 0xFF).round(), ((blue + match) * 0xFF).round());
        }
    }
}