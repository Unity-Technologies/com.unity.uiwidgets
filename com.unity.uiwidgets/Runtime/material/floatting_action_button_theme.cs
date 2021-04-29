using System;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.ui;
using UnityEngine;
using Color = Unity.UIWidgets.ui.Color;

namespace Unity.UIWidgets.material {
    public class FloatingActionButtonThemeData : Diagnosticable, IEquatable<FloatingActionButtonThemeData> {
        public FloatingActionButtonThemeData(
            Color foregroundColor = null,
            Color backgroundColor = null,
            Color focusColor = null,
            Color hoverColor = null,
            Color splashColor = null,
            float? elevation = null,
            float? focusElevation = null,
            float? hoverElevation = null,
            float? disabledElevation = null,
            float? highlightElevation = null,
            ShapeBorder shape = null
        ) {
            this.foregroundColor = foregroundColor;
            this.backgroundColor = backgroundColor;
            this.focusColor = focusColor;
            this.hoverColor = hoverColor;
            this.splashColor = splashColor;
            this.elevation = elevation;
            this.focusElevation = focusElevation;
            this.hoverElevation = hoverElevation;
            this.disabledElevation = disabledElevation;
            this.highlightElevation = highlightElevation;
            this.shape = shape;
        }

        public readonly Color foregroundColor;

        public readonly Color backgroundColor;

        public readonly Color focusColor;

        public readonly Color hoverColor;

        public readonly Color splashColor;

        public readonly float? elevation;

        public readonly float? focusElevation;

        public readonly float? hoverElevation;

        public readonly float? disabledElevation;

        public readonly float? highlightElevation;

        public readonly ShapeBorder shape;

        public FloatingActionButtonThemeData copyWith(
            Color foregroundColor = null,
            Color backgroundColor = null,
            Color focusColor = null,
            Color hoverColor = null,
            Color splashColor = null,
            float? elevation = null,
            float? focusElevation = null,
            float? hoverElevation = null,
            float? disabledElevation = null,
            float? highlightElevation = null,
            ShapeBorder shape = null
        ) {
            return new FloatingActionButtonThemeData(
                foregroundColor: foregroundColor ?? this.foregroundColor,
                backgroundColor: backgroundColor ?? this.backgroundColor,
                focusColor: focusColor ?? this.focusColor,
                hoverColor: hoverColor ?? this.hoverColor,
                splashColor: splashColor ?? this.splashColor,
                elevation: elevation ?? this.elevation,
                focusElevation: focusElevation ?? this.focusElevation,
                hoverElevation: hoverElevation ?? this.hoverElevation,
                disabledElevation: disabledElevation ?? this.disabledElevation,
                highlightElevation: highlightElevation ?? this.highlightElevation,
                shape: shape ?? this.shape
            );
        }

        public static FloatingActionButtonThemeData lerp(FloatingActionButtonThemeData a,
            FloatingActionButtonThemeData b,
            float t) {
            if (a == null && b == null) {
                return null;
            }

            return new FloatingActionButtonThemeData(
                foregroundColor: Color.lerp(a?.foregroundColor, b?.foregroundColor, t),
                backgroundColor: Color.lerp(a?.backgroundColor, b?.backgroundColor, t),
                focusColor: Color.lerp(a?.focusColor, b?.focusColor, t),
                hoverColor: Color.lerp(a?.hoverColor, b?.hoverColor, t),
                splashColor: Color.lerp(a?.splashColor, b?.splashColor, t),
                elevation: MathUtils.lerpNullableFloat(a?.elevation, b?.elevation, t),
                focusElevation: MathUtils.lerpNullableFloat(a?.focusElevation, b?.focusElevation, t),
                hoverElevation: MathUtils.lerpNullableFloat(a?.hoverElevation, b?.hoverElevation, t),
                disabledElevation: MathUtils.lerpNullableFloat(a?.disabledElevation, b?.disabledElevation, t),
                highlightElevation: MathUtils.lerpNullableFloat(a?.highlightElevation, b?.highlightElevation, t),
                shape: ShapeBorder.lerp(a?.shape, b?.shape, t)
            );
        }

        public override int GetHashCode() {
            var hashCode = (foregroundColor != null ? foregroundColor.GetHashCode() : 0);
            hashCode = (hashCode * 397) ^ (backgroundColor != null ? backgroundColor.GetHashCode() : 0);
            hashCode = (hashCode * 397) ^ (focusColor != null ? focusColor.GetHashCode() : 0);
            hashCode = (hashCode * 397) ^ (hoverColor != null ? hoverColor.GetHashCode() : 0);
            hashCode = (hashCode * 397) ^ (splashColor != null ? splashColor.GetHashCode() : 0);
            hashCode = (hashCode * 397) ^ elevation.GetHashCode();
            hashCode = (hashCode * 397) ^ focusElevation.GetHashCode();
            hashCode = (hashCode * 397) ^ hoverElevation.GetHashCode();
            hashCode = (hashCode * 397) ^ disabledElevation.GetHashCode();
            hashCode = (hashCode * 397) ^ highlightElevation.GetHashCode();
            hashCode = (hashCode * 397) ^ (shape != null ? shape.GetHashCode() : 0);
            return hashCode;
        }

        public bool Equals(FloatingActionButtonThemeData other) {
            if (ReferenceEquals(null, other)) {
                return false;
            }

            if (ReferenceEquals(this, other)) {
                return true;
            }

            return Equals(foregroundColor, other.foregroundColor) && Equals(backgroundColor, other.backgroundColor) &&
                   Equals(focusColor, other.focusColor) && Equals(hoverColor, other.hoverColor) &&
                   Equals(splashColor, other.splashColor) && Nullable.Equals(elevation, other.elevation) &&
                   Nullable.Equals(focusElevation, other.focusElevation) &&
                   Nullable.Equals(hoverElevation, other.hoverElevation) &&
                   Nullable.Equals(disabledElevation, other.disabledElevation) &&
                   Nullable.Equals(highlightElevation, other.highlightElevation) && Equals(shape, other.shape);
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

            return Equals((FloatingActionButtonThemeData) obj);
        }

        public override void debugFillProperties(DiagnosticPropertiesBuilder properties) {
            base.debugFillProperties(properties);
            FloatingActionButtonThemeData defaultData = new FloatingActionButtonThemeData();

            properties.add(new ColorProperty("foregroundColor", foregroundColor,
                defaultValue: defaultData.foregroundColor));
            properties.add(new ColorProperty("backgroundColor", backgroundColor,
                defaultValue: defaultData.backgroundColor));
            properties.add(new ColorProperty("focusColor", focusColor, defaultValue: defaultData.focusColor));
            properties.add(new ColorProperty("hoverColor", hoverColor, defaultValue: defaultData.hoverColor));
            properties.add(new ColorProperty("splashColor", splashColor, defaultValue: defaultData.splashColor));
            properties.add(new FloatProperty("elevation", elevation, defaultValue: defaultData.elevation));
            properties.add(
                new FloatProperty("focusElevation", focusElevation, defaultValue: defaultData.focusElevation));
            properties.add(
                new FloatProperty("hoverElevation", hoverElevation, defaultValue: defaultData.hoverElevation));
            properties.add(new FloatProperty("disabledElevation", disabledElevation,
                defaultValue: defaultData.disabledElevation));
            properties.add(new FloatProperty("highlightElevation", highlightElevation,
                defaultValue: defaultData.highlightElevation));
            properties.add(new DiagnosticsProperty<ShapeBorder>("shape", shape, defaultValue: defaultData.shape));
        }
    }
}