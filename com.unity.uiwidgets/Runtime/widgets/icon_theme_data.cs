using System;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.ui;

namespace Unity.UIWidgets.widgets {
    public class IconThemeData : Diagnosticable, IEquatable<IconThemeData> {
        public IconThemeData(
            Color color = null,
            float? opacity = null,
            float? size = null) {
            this.color = color;
            _opacity = opacity;
            this.size = size;
        }

        public static IconThemeData fallback() {
            return new IconThemeData(
                color: new Color(0xFF000000),
                opacity: 1.0f,
                size: 24.0f);
        }

        public IconThemeData copyWith(
            Color color = null,
            float? opacity = null,
            float? size = null) {
            return new IconThemeData(
                color: color ?? this.color,
                opacity: opacity ?? this.opacity,
                size: size ?? this.size
            );
        }

        public IconThemeData merge(IconThemeData other) {
            if (other == null) {
                return this;
            }

            return copyWith(
                color: other.color,
                opacity: other.opacity,
                size: other.size
            );
        }

        public IconThemeData resolve(BuildContext context) => this;
        
        public bool isConcrete {
            get { return color != null && opacity != null && size != null; }
        }

        public readonly Color color;

        public float? opacity {
            get { return _opacity == null ? (float?) null : _opacity.Value.clamp(0.0f, 1.0f); }
        }

        readonly float? _opacity;

        public readonly float? size;

        public static IconThemeData lerp(IconThemeData a, IconThemeData b, float t) {
            return new IconThemeData(
                color: Color.lerp(a?.color, b?.color, t),
                opacity: MathUtils.lerpNullableFloat(a?.opacity, b?.opacity, t),
                size: MathUtils.lerpNullableFloat(a?.size, b?.size, t));
        }


        public bool Equals(IconThemeData other) {
            if (ReferenceEquals(null, other)) {
                return false;
            }

            if (ReferenceEquals(this, other)) {
                return true;
            }

            return Equals(color, other.color) &&
                   _opacity.Equals(other._opacity) &&
                   size.Equals(other.size);
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

            return Equals((IconThemeData) obj);
        }

        public override int GetHashCode() {
            unchecked {
                var hashCode = (color != null ? color.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ _opacity.GetHashCode();
                hashCode = (hashCode * 397) ^ size.GetHashCode();
                return hashCode;
            }
        }

        public static bool operator ==(IconThemeData left, IconThemeData right) {
            return Equals(left, right);
        }

        public static bool operator !=(IconThemeData left, IconThemeData right) {
            return !Equals(left, right);
        }

        public override void debugFillProperties(DiagnosticPropertiesBuilder properties) {
            base.debugFillProperties(properties);
            properties.add(new ColorProperty("color", color,
                defaultValue: foundation_.kNullDefaultValue));
            properties.add(new FloatProperty("opacity", opacity,
                defaultValue: foundation_.kNullDefaultValue));
            properties.add(new FloatProperty("size", size,
                defaultValue: foundation_.kNullDefaultValue));
        }
    }
}