using System;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.material;
using Unity.UIWidgets.painting;
using UnityEngine;
using Color = Unity.UIWidgets.ui.Color;
using Object = UnityEngine.Object;
using TextStyle = Unity.UIWidgets.painting.TextStyle;

namespace Unity.UIWidgets.material {
    public enum SnackBarBehavior {
        fix,
        floating,
    }

    public class SnackBarThemeData : Diagnosticable, IEquatable<SnackBarThemeData> {
        public SnackBarThemeData(
            Color backgroundColor,
            Color actionTextColor,
            Color disabledActionTextColor,
            TextStyle contentTextStyle,
            float? elevation,
            ShapeBorder shape,
            SnackBarBehavior? behavior
        ) {
            D.assert(elevation == null || elevation >= 0.0f);

            this.backgroundColor = backgroundColor;
            this.actionTextColor = actionTextColor;
            this.disabledActionTextColor = disabledActionTextColor;
            this.contentTextStyle = contentTextStyle;
            this.elevation = elevation;
            this.shape = shape;
            this.behavior = behavior;
        }

        public readonly Color backgroundColor;

        public readonly Color actionTextColor;

        public readonly Color disabledActionTextColor;

        public readonly TextStyle contentTextStyle;

        public readonly float? elevation;

        public readonly ShapeBorder shape;

        public readonly SnackBarBehavior? behavior;

        public SnackBarThemeData copyWith(
            Color backgroundColor,
            Color actionTextColor,
            Color disabledActionTextColor,
            TextStyle contentTextStyle,
            float? elevation,
            ShapeBorder shape,
            SnackBarBehavior? behavior
        ) {
            return new SnackBarThemeData(
                backgroundColor: backgroundColor ?? this.backgroundColor,
                actionTextColor: actionTextColor ?? this.actionTextColor,
                disabledActionTextColor: disabledActionTextColor ?? this.disabledActionTextColor,
                contentTextStyle: contentTextStyle ?? this.contentTextStyle,
                elevation: elevation ?? this.elevation,
                shape: shape ?? this.shape,
                behavior: behavior ?? this.behavior
            );
        }

        static SnackBarThemeData lerp(SnackBarThemeData a, SnackBarThemeData b, float t) {
            return new SnackBarThemeData(
                backgroundColor: Color.lerp(a?.backgroundColor, b?.backgroundColor, t),
                actionTextColor: Color.lerp(a?.actionTextColor, b?.actionTextColor, t),
                disabledActionTextColor: Color.lerp(a?.disabledActionTextColor, b?.disabledActionTextColor, t),
                contentTextStyle: TextStyle.lerp(a?.contentTextStyle, b?.contentTextStyle, t),
                elevation: Mathf.Lerp(a?.elevation ?? 0, b?.elevation ?? 0, t),
                shape: ShapeBorder.lerp(a?.shape, b?.shape, t),
                behavior: t < 0.5 ? a.behavior : b.behavior
            );
        }

        public override void debugFillProperties(DiagnosticPropertiesBuilder properties) {
            base.debugFillProperties(properties);
            properties.add(new ColorProperty("backgroundColor", backgroundColor, defaultValue: null));
            properties.add(new ColorProperty("actionTextColor", actionTextColor, defaultValue: null));
            properties.add(new ColorProperty("disabledActionTextColor", disabledActionTextColor, defaultValue: null));
            properties.add(
                new DiagnosticsProperty<TextStyle>("contentTextStyle", contentTextStyle, defaultValue: null));
            properties.add(new FloatProperty("elevation", elevation, defaultValue: null));
            properties.add(new DiagnosticsProperty<ShapeBorder>("shape", shape, defaultValue: null));
            properties.add(new DiagnosticsProperty<SnackBarBehavior?>("behavior", behavior, defaultValue: null));
        }

        public static bool operator ==(SnackBarThemeData self, SnackBarThemeData other) {
            return Equals(self, other);
        }

        public static bool operator !=(SnackBarThemeData self, SnackBarThemeData other) {
            return Equals(self, other);
        }

        public bool Equals(SnackBarThemeData other) {
            if (ReferenceEquals(null, other)) {
                return false;
            }

            if (ReferenceEquals(this, other)) {
                return true;
            }

            return Equals(backgroundColor, other.backgroundColor) && Equals(actionTextColor, other.actionTextColor) &&
                   Equals(disabledActionTextColor, other.disabledActionTextColor) &&
                   Equals(contentTextStyle, other.contentTextStyle) && Nullable.Equals(elevation, other.elevation) &&
                   Equals(shape, other.shape) && behavior == other.behavior;
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

            return Equals((SnackBarThemeData) obj);
        }

        public override int GetHashCode() {
            unchecked {
                var hashCode = (backgroundColor != null ? backgroundColor.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (actionTextColor != null ? actionTextColor.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^
                           (disabledActionTextColor != null ? disabledActionTextColor.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (contentTextStyle != null ? contentTextStyle.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ elevation.GetHashCode();
                hashCode = (hashCode * 397) ^ (shape != null ? shape.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (int) behavior;
                return hashCode;
            }
        }
    }
}