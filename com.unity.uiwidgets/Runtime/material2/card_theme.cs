using Unity.UIWidgets.foundation;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.ui;
using Unity.UIWidgets.widgets;

namespace Unity.UIWidgets.material {
    public class CardTheme : Diagnosticable {
        public CardTheme(
            Clip? clipBehavior = null,
            Color color = null,
            float? elevation = null,
            EdgeInsets margin = null,
            ShapeBorder shape = null
        ) {
            D.assert(elevation == null || elevation >= 0.0f);
            this.clipBehavior = clipBehavior;
            this.color = color;
            this.elevation = elevation;
            this.margin = margin;
            this.shape = shape;
        }

        public readonly Clip? clipBehavior;

        public readonly Color color;

        public readonly float? elevation;

        public readonly EdgeInsets margin;

        public readonly ShapeBorder shape;

        CardTheme copyWith(
            Clip? clipBehavior = null,
            Color color = null,
            float? elevation = null,
            EdgeInsets margin = null,
            ShapeBorder shape = null
        ) {
            return new CardTheme(
                clipBehavior: clipBehavior ?? this.clipBehavior,
                color: color ?? this.color,
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
                elevation: MathUtils.lerpFloat(a?.elevation ?? 0.0f, b?.elevation ?? 0.0f, t),
                margin: EdgeInsets.lerp(a?.margin, b?.margin, t),
                shape: ShapeBorder.lerp(a?.shape, b?.shape, t)
            );
        }

        public override int GetHashCode() {
            var hashCode = clipBehavior?.GetHashCode() ?? 0;
            hashCode = (hashCode * 397) ^ color?.GetHashCode() ?? 0;
            hashCode = (hashCode * 397) ^ elevation?.GetHashCode() ?? 0;
            hashCode = (hashCode * 397) ^ margin?.GetHashCode() ?? 0;
            hashCode = (hashCode * 397) ^ shape?.GetHashCode() ?? 0;
            return hashCode;
        }

        public bool Equals(CardTheme other) {
            return other.clipBehavior == clipBehavior
                   && other.color == color
                   && other.elevation == elevation
                   && other.margin == margin
                   && other.shape == shape;
        }

        public override void debugFillProperties(DiagnosticPropertiesBuilder properties) {
            base.debugFillProperties(properties);
            properties.add(new DiagnosticsProperty<Clip?>("clipBehavior", clipBehavior, defaultValue: null));
            properties.add(new DiagnosticsProperty<Color>("color", color, defaultValue: null));
            properties.add(new DiagnosticsProperty<float?>("elevation", elevation, defaultValue: null));
            properties.add(new DiagnosticsProperty<EdgeInsets>("margin", margin, defaultValue: null));
            properties.add(new DiagnosticsProperty<ShapeBorder>("shape", shape, defaultValue: null));
        }
    }
}