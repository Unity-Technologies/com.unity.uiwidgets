using Unity.UIWidgets.foundation;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.ui;
using Unity.UIWidgets.widgets;
using UnityEngine;
using Color = Unity.UIWidgets.ui.Color;

namespace Unity.UIWidgets.material {
    public class CardTheme : Diagnosticable {
        public CardTheme(
            Clip? clipBehavior = null,
            Color color = null,
            Color shadowColor = null,
            float? elevation = null,
            EdgeInsetsGeometry margin = null,
            ShapeBorder shape = null
        ) {
            D.assert(elevation == null || elevation >= 0.0f);
            this.clipBehavior = clipBehavior;
            this.color = color;
            this.shadowColor = shadowColor;
            this.elevation = elevation;
            this.margin = margin;
            this.shape = shape;
        }

        public readonly Clip? clipBehavior;

        public readonly Color color;

        public readonly Color shadowColor;

        public readonly float? elevation;

        public readonly EdgeInsetsGeometry margin;

        public readonly ShapeBorder shape;

        CardTheme copyWith(
            Clip? clipBehavior = null,
            Color color = null,
            Color shadowColor = null,
            float? elevation = null,
            EdgeInsetsGeometry margin = null,
            ShapeBorder shape = null
        ) {
            return new CardTheme(
                clipBehavior: clipBehavior ?? this.clipBehavior,
                color: color ?? this.color,
                shadowColor: shadowColor ?? this.shadowColor,
                elevation: elevation ?? this.elevation,
                margin: margin ?? this.margin,
                shape: shape ?? this.shape
            );
        }

        public static CardTheme of(BuildContext context) {
            return Theme.of(context).cardTheme;
        }

        public static CardTheme lerp(CardTheme a, CardTheme b, float t) {
            return new CardTheme(
                clipBehavior: t < 0.5f ? a?.clipBehavior : b?.clipBehavior,
                color: Color.lerp(a?.color, b?.color, t),
                shadowColor: Color.lerp(a?.shadowColor, b?.shadowColor, t),
                elevation: MathUtils.lerpNullableFloat(a?.elevation, b?.elevation, t),
                margin: EdgeInsetsGeometry.lerp(a?.margin, b?.margin, t),
                shape: ShapeBorder.lerp(a?.shape, b?.shape, t)
            );
        }

        public override int GetHashCode() {
            var hashCode = clipBehavior?.GetHashCode() ?? 0;
            hashCode = (hashCode * 397) ^ color?.GetHashCode() ?? 0;
            hashCode = (hashCode * 397) ^ shadowColor?.GetHashCode() ?? 0;
            hashCode = (hashCode * 397) ^ elevation?.GetHashCode() ?? 0;
            hashCode = (hashCode * 397) ^ margin?.GetHashCode() ?? 0;
            hashCode = (hashCode * 397) ^ shape?.GetHashCode() ?? 0;
            return hashCode;
        }

        public bool Equals(CardTheme other) {
            return other.clipBehavior == clipBehavior
                   && other.color == color
                   && other.shadowColor == shadowColor
                   && other.elevation == elevation
                   && other.margin == margin
                   && other.shape == shape;
        }

        public override void debugFillProperties(DiagnosticPropertiesBuilder properties) {
            base.debugFillProperties(properties);
            properties.add(new DiagnosticsProperty<Clip?>("clipBehavior", clipBehavior, defaultValue: null));
            properties.add(new ColorProperty("color", color, defaultValue: null));
            properties.add(new ColorProperty("shadowColor", shadowColor, defaultValue: null));
            properties.add(new DiagnosticsProperty<float?>("elevation", elevation, defaultValue: null));
            properties.add(new DiagnosticsProperty<EdgeInsetsGeometry>("margin", margin, defaultValue: null));
            properties.add(new DiagnosticsProperty<ShapeBorder>("shape", shape, defaultValue: null));
        }
    }
}