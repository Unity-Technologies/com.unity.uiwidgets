using System;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.ui;
using Unity.UIWidgets.widgets;
using UnityEngine;
using Color = Unity.UIWidgets.ui.Color;

namespace Unity.UIWidgets.material {
    public class DividerThemeData : Diagnosticable, IEquatable<DividerThemeData> {
        public DividerThemeData(
            Color color = null,
            float? space = null,
            float? thickness = null,
            float? indent = null,
            float? endIndent = null
        ) {
            this.color = color;
            this.space = space;
            this.thickness = thickness;
            this.indent = indent;
            this.endIndent = endIndent;
        }

        public readonly Color color;

        public readonly float? space;

        public readonly float? thickness;

        public readonly float? indent;

        public readonly float? endIndent;

        public DividerThemeData copyWith(
            Color color = null,
            float? space = null,
            float? thickness = null,
            float? indent = null,
            float? endIndent = null
        ) {
            return new DividerThemeData(
                color: color ?? this.color,
                space: space ?? this.space,
                thickness: thickness ?? this.thickness,
                indent: indent ?? this.indent,
                endIndent: endIndent ?? this.endIndent
            );
        }

        public static DividerThemeData lerp(DividerThemeData a, DividerThemeData b, float t) {
            return new DividerThemeData(
                color: Color.lerp(a?.color, b?.color, t),
                space: MathUtils.lerpNullableFloat(a?.space, b?.space, t),
                thickness: MathUtils.lerpNullableFloat(a?.thickness, b?.thickness, t),
                indent: MathUtils.lerpNullableFloat(a?.indent, b?.indent, t),
                endIndent: MathUtils.lerpNullableFloat(a?.endIndent, b?.endIndent, t)
            );
        }

        public override void debugFillProperties(DiagnosticPropertiesBuilder properties) {
            base.debugFillProperties(properties);
            properties.add(new ColorProperty("color", color, defaultValue: null));
            properties.add(new FloatProperty("space", space, defaultValue: null));
            properties.add(new FloatProperty("thickness", thickness, defaultValue: null));
            properties.add(new FloatProperty("indent", indent, defaultValue: null));
            properties.add(new FloatProperty("endIndent", endIndent, defaultValue: null));
        }

        public static bool operator ==(DividerThemeData self, DividerThemeData other) {
            return Equals(self, other);
        }

        public static bool operator !=(DividerThemeData self, DividerThemeData other) {
            return !Equals(self, other);
        }

        public bool Equals(DividerThemeData other) {
            if (ReferenceEquals(null, other)) {
                return false;
            }

            if (ReferenceEquals(this, other)) {
                return true;
            }

            return Equals(color, other.color) && Nullable.Equals(space, other.space) &&
                   Nullable.Equals(thickness, other.thickness) && Nullable.Equals(indent, other.indent) &&
                   Nullable.Equals(endIndent, other.endIndent);
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

            return Equals((DividerThemeData) obj);
        }

        public override int GetHashCode() {
            unchecked {
                var hashCode = (color != null ? color.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ space.GetHashCode();
                hashCode = (hashCode * 397) ^ thickness.GetHashCode();
                hashCode = (hashCode * 397) ^ indent.GetHashCode();
                hashCode = (hashCode * 397) ^ endIndent.GetHashCode();
                return hashCode;
            }
        }
    }

    public class DividerTheme : InheritedTheme {
        public DividerTheme(
            Key key = null,
            DividerThemeData data = null,
            Widget child = null
        ) : base(key: key, child: child) {
            D.assert(data != null);
            this.data = data;
        }

        public readonly DividerThemeData data;

        public static DividerThemeData of(BuildContext context) {
            DividerTheme dividerTheme = context.dependOnInheritedWidgetOfExactType<DividerTheme>();
            return dividerTheme?.data ?? Theme.of(context).dividerTheme;
        }

        public override Widget wrap(BuildContext context, Widget child) {
            DividerTheme ancestorTheme = context.findAncestorWidgetOfExactType<DividerTheme>();
            return ReferenceEquals(this, ancestorTheme) ? child : new DividerTheme(data: data, child: child);
        }

        public override bool updateShouldNotify(InheritedWidget oldWidget) =>
            oldWidget is DividerTheme dividerTheme && data != dividerTheme.data;
    }
}