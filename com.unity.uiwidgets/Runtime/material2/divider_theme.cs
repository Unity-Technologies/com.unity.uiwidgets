using System;
using JetBrains.Annotations;
using Unity.UIWidgets.animation;
using Unity.UIWidgets.async2;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.gestures;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.rendering;
using Unity.UIWidgets.service;
using Unity.UIWidgets.ui;
using Unity.UIWidgets.widgets;
using UnityEditor.Graphs;
using Brightness = Unity.UIWidgets.ui.Brightness;
using TextStyle = Unity.UIWidgets.painting.TextStyle;


namespace Unity.UIWidgets.material {
    public class DividerThemeData : Diagnosticable,IEquatable<DividerThemeData> {

        public DividerThemeData(
             Color color = null,
             float? space = null,
             float? thickness = null,
             float? indent = null,
             float? endIndent = null) {
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
            float? endIndent = null) {
            return new DividerThemeData(
              color: color ?? this.color,
              space: space ?? this.space,
              thickness: thickness ?? this.thickness,
              indent: indent ?? this.indent,
              endIndent: endIndent ?? this.endIndent
            );
        }
        public static DividerThemeData lerp(DividerThemeData a, DividerThemeData b, float t) {
            D.assert(t != null);
            return new DividerThemeData(
              color: Color.lerp(a?.color, b?.color, t),
              space: MathUtils.lerpFloat((float)a?.space, (float)b?.space, t),
              thickness: MathUtils.lerpFloat((float)a?.thickness, (float)b?.thickness, t),
              indent: MathUtils.lerpFloat((float)a?.indent, (float)b?.indent, t),
              endIndent: MathUtils.lerpFloat((float)a?.endIndent, (float)b?.endIndent, t)
            );
        }

        public  override  void debugFillProperties(DiagnosticPropertiesBuilder properties) {
            base.debugFillProperties(properties);
            properties.add( new ColorProperty("color", color, defaultValue: null));
            properties.add( new FloatProperty("space", space, defaultValue: null));
            properties.add( new FloatProperty("thickness", thickness, defaultValue: null));
            properties.add( new FloatProperty("indent", indent, defaultValue: null));
            properties.add( new FloatProperty("endIndent", endIndent, defaultValue: null));
        }

        public bool Equals(DividerThemeData other)
        {
            if (ReferenceEquals(null, other)) {
                return false;
            }

            if (ReferenceEquals(this, other)) {
                return true;
            }

            return Equals(color, other.color) && Nullable.Equals(space, other.space) && Nullable.Equals(thickness, other.thickness) && Nullable.Equals(indent, other.indent) && Nullable.Equals(endIndent, other.endIndent);
        }

        public override bool Equals(object obj)
        {
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
        
        public static bool operator ==(DividerThemeData left, DividerThemeData right) {
            return Equals(left, right);
        }

        public static bool operator !=(DividerThemeData left, DividerThemeData right) {
            return !Equals(left, right);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (color != null ? color.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ space.GetHashCode();
                hashCode = (hashCode * 397) ^ thickness.GetHashCode();
                hashCode = (hashCode * 397) ^ indent.GetHashCode();
                hashCode = (hashCode * 397) ^ endIndent.GetHashCode();
                return hashCode;
            }
        }
    }
}