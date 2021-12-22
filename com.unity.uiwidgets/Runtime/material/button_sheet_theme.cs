using System;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.ui;
using UnityEngine;
using Color = Unity.UIWidgets.ui.Color;

namespace Unity.UIWidgets.material {
    public class BottomSheetThemeData : Diagnosticable, IEquatable<BottomSheetThemeData> {
        public BottomSheetThemeData(
            Color backgroundColor = null,
            float? elevation = null,
            Color modalBackgroundColor = null,
            float? modalElevation = null,
            ShapeBorder shape = null,
            Clip? clipBehavior = null
        ) {
            this.backgroundColor = backgroundColor;
            this.elevation = elevation;
            this.modalBackgroundColor = modalBackgroundColor;
            this.modalElevation = modalElevation;
            this.shape = shape;
            this.clipBehavior = clipBehavior;
        }

        public readonly Color backgroundColor;

        public readonly float? elevation;

        public readonly Color modalBackgroundColor;

        public readonly float? modalElevation;

        public readonly ShapeBorder shape;

        public readonly Clip? clipBehavior;

        public BottomSheetThemeData copyWith(
            Color backgroundColor = null,
            float? elevation = null,
            Color modalBackgroundColor = null,
            float? modalElevation = null,
            ShapeBorder shape = null,
            Clip? clipBehavior = null
        ) {
            return new BottomSheetThemeData(
                backgroundColor: backgroundColor ?? this.backgroundColor,
                elevation: elevation ?? this.elevation,
                modalBackgroundColor: modalBackgroundColor ?? this.modalBackgroundColor,
                modalElevation: modalElevation ?? this.modalElevation,
                shape: shape ?? this.shape,
                clipBehavior: clipBehavior ?? this.clipBehavior
            );
        }

        public static BottomSheetThemeData lerp(BottomSheetThemeData a, BottomSheetThemeData b, float t) {
            if (a == null && b == null)
                return null;
            return new BottomSheetThemeData(
                backgroundColor: Color.lerp(a?.backgroundColor, b?.backgroundColor, t),
                elevation: MathUtils.lerpNullableFloat(a?.elevation, b?.elevation, t),
                modalBackgroundColor: Color.lerp(a?.modalBackgroundColor, b?.modalBackgroundColor, t),
                modalElevation: MathUtils.lerpNullableFloat(a?.modalElevation, b?.modalElevation, t),
                shape: ShapeBorder.lerp(a?.shape, b?.shape, t),
                clipBehavior: t < 0.5 ? a?.clipBehavior : b?.clipBehavior
            );
        }

        public override void debugFillProperties(DiagnosticPropertiesBuilder properties) {
            base.debugFillProperties(properties);
            properties.add(new ColorProperty("backgroundColor", backgroundColor, defaultValue: null));
            properties.add(new FloatProperty("elevation", elevation, defaultValue: null));
            properties.add(new ColorProperty("modalBackgroundColor", modalBackgroundColor, defaultValue: null));
            properties.add(new FloatProperty("modalElevation", modalElevation, defaultValue: null));
            properties.add(new DiagnosticsProperty<ShapeBorder>("shape", shape, defaultValue: null));
            properties.add(new DiagnosticsProperty<Clip?>("clipBehavior", clipBehavior, defaultValue: null));
        }

        public bool Equals(BottomSheetThemeData other) {
            if (ReferenceEquals(null, other)) {
                return false;
            }

            if (ReferenceEquals(this, other)) {
                return true;
            }

            return Equals(backgroundColor, other.backgroundColor) && Nullable.Equals(elevation, other.elevation) &&
                   Equals(modalBackgroundColor, other.modalBackgroundColor) &&
                   Nullable.Equals(modalElevation, other.modalElevation) && Equals(shape, other.shape) &&
                   clipBehavior == other.clipBehavior;
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

            return Equals((BottomSheetThemeData) obj);
        }

        public override int GetHashCode() {
            unchecked {
                var hashCode = (backgroundColor != null ? backgroundColor.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ elevation.GetHashCode();
                hashCode = (hashCode * 397) ^ (modalBackgroundColor != null ? modalBackgroundColor.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ modalElevation.GetHashCode();
                hashCode = (hashCode * 397) ^ (shape != null ? shape.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ clipBehavior.GetHashCode();
                return hashCode;
            }
        }
    }
}