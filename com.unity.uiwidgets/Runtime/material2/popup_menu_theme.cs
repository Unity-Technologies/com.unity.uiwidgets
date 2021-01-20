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
    public class PopupMenuThemeData : Diagnosticable,IEquatable<PopupMenuThemeData> {
        public PopupMenuThemeData(
        Color color = null,
        ShapeBorder shape = null,
        float? elevation = null,
        TextStyle textStyle = null) {
            this.color = color;
            this.shape = shape;
            this.elevation = elevation;
            this.textStyle = textStyle;
        }
        public readonly Color color;
        public readonly ShapeBorder shape;
        public readonly float? elevation;
        public readonly TextStyle textStyle;
        
        public PopupMenuThemeData copyWith(
            Color color = null,
            ShapeBorder shape = null,
            float? elevation = null,
            TextStyle textStyle = null) {
            return new PopupMenuThemeData(
              color: color ?? this.color,
              shape: shape ?? this.shape,
              elevation: elevation ?? this.elevation,
              textStyle: textStyle ?? this.textStyle
            );
        }
        public static PopupMenuThemeData lerp(PopupMenuThemeData a, PopupMenuThemeData b, float t) {
            D.assert(t != null);
            if (a == null && b == null)
              return null;
            return new PopupMenuThemeData(
              color: Color.lerp(a?.color, b?.color, t),
              shape: ShapeBorder.lerp(a?.shape, b?.shape, t),
              elevation: MathUtils.lerpFloat((float)a?.elevation.Value, (float)b?.elevation.Value, t),
              textStyle: TextStyle.lerp(a?.textStyle, b?.textStyle, t)
            );
        }
        public override void debugFillProperties(DiagnosticPropertiesBuilder properties) {
            base.debugFillProperties(properties);
            properties.add( new ColorProperty("color", color, defaultValue: null));
            properties.add( new DiagnosticsProperty<ShapeBorder>("shape", shape, defaultValue: null));
            properties.add( new FloatProperty("elevation", elevation, defaultValue: null));
            properties.add( new DiagnosticsProperty<TextStyle>("text style", textStyle, defaultValue: null));
        }

        public bool Equals(PopupMenuThemeData other)
        {
            if (ReferenceEquals(null, other)) {
                return false;
            }

            if (ReferenceEquals(this, other)) {
                return true;
            }

            return Equals(color, other.color) 
                   && Equals(shape, other.shape) 
                   && Nullable.Equals(elevation, other.elevation) 
                   && Equals(textStyle, other.textStyle);
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

            return Equals((PopupMenuThemeData) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (color != null ? color.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (shape != null ? shape.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ elevation.GetHashCode();
                hashCode = (hashCode * 397) ^ (textStyle != null ? textStyle.GetHashCode() : 0);
                return hashCode;
            }
        }
        public static bool operator ==(PopupMenuThemeData left, PopupMenuThemeData right) {
            return Equals(left, right);
        }

        public static bool operator !=(PopupMenuThemeData left, PopupMenuThemeData right) {
            return !Equals(left, right);
        }

    }
    public class PopupMenuTheme : InheritedTheme {
        public PopupMenuTheme(
            Key key ,
            PopupMenuThemeData data,
            Widget child = null) : base(key: key, child: child) {
            D.assert(data != null);
            this.data = data;
        }
        public readonly  PopupMenuThemeData data;
        
        public static PopupMenuThemeData of(BuildContext context) { 
            PopupMenuTheme popupMenuTheme = context.dependOnInheritedWidgetOfExactType<PopupMenuTheme>();
            return popupMenuTheme?.data ?? Theme.of(context).popupMenuTheme;
        }

        public  override Widget wrap(BuildContext context, Widget child) { 
            PopupMenuTheme ancestorTheme = context.findAncestorWidgetOfExactType<PopupMenuTheme>();
            return this == ancestorTheme ? child : new PopupMenuTheme(data: data, child: child);
        }

        public override bool updateShouldNotify(InheritedWidget oldWidget) {
            oldWidget = (PopupMenuTheme)oldWidget;
            return data != ((PopupMenuTheme)oldWidget).data;
        }
    }

}