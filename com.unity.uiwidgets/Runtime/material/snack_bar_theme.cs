using System;
using Unity.UIWidgets.animation;
using Unity.UIWidgets.async2;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.gestures;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.rendering;
using Unity.UIWidgets.service;
using Unity.UIWidgets.ui;
using Unity.UIWidgets.widgets;
using Brightness = Unity.UIWidgets.ui.Brightness;
using TextStyle = Unity.UIWidgets.painting.TextStyle;


namespace Unity.UIWidgets.material {
    public enum SnackBarBehavior {
        fixing,
        floating,
    }


    public class SnackBarThemeData :  Diagnosticable,IEquatable<SnackBarThemeData> {
        public SnackBarThemeData (
          Color backgroundColor = null,
          Color actionTextColor = null,
          Color disabledActionTextColor = null,
          TextStyle contentTextStyle = null,
          float? elevation = null,
          ShapeBorder shape = null,
          SnackBarBehavior? behavior = null
        ) {
            D.assert(elevation == null || elevation >= 0.0);
            this.backgroundColor = backgroundColor;
            this.actionTextColor = actionTextColor;
            this.disabledActionTextColor = disabledActionTextColor;
            this.contentTextStyle = contentTextStyle;
            this.elevation = elevation;
            this.shape = shape;
            this.behavior = behavior;
        }
        public readonly  Color backgroundColor;
        public readonly  Color actionTextColor;
        public readonly  Color disabledActionTextColor;
        public readonly  TextStyle contentTextStyle;
        public readonly  float? elevation;
        public readonly  ShapeBorder shape;
        public readonly  SnackBarBehavior? behavior;
        
        public SnackBarThemeData copyWith( 
            Color backgroundColor = null,
            Color actionTextColor = null,
            Color disabledActionTextColor = null,
            TextStyle contentTextStyle = null,
            float? elevation = null,
            ShapeBorder shape = null,
            SnackBarBehavior? behavior = null ){
            
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
        public static SnackBarThemeData lerp(SnackBarThemeData a, SnackBarThemeData b, float t) {
            D.assert(t != null);
            return new SnackBarThemeData(
                backgroundColor: Color.lerp(a?.backgroundColor, b?.backgroundColor, t),
                actionTextColor: Color.lerp(a?.actionTextColor, b?.actionTextColor, t),
                disabledActionTextColor: Color.lerp(a?.disabledActionTextColor, b?.disabledActionTextColor, t),
                contentTextStyle: TextStyle.lerp(a?.contentTextStyle, b?.contentTextStyle, t),
                elevation: MathUtils.lerpFloat((float)a?.elevation.Value, (float)b?.elevation.Value, t),
                shape: ShapeBorder.lerp(a?.shape, b?.shape, t),
                behavior: t < 0.5 ? a.behavior : b.behavior
            );
         }

        public override void debugFillProperties(DiagnosticPropertiesBuilder properties) {
            base.debugFillProperties(properties);
            properties.add( new ColorProperty("backgroundColor", backgroundColor, defaultValue: null));
            properties.add( new ColorProperty("actionTextColor", actionTextColor, defaultValue: null));
            properties.add( new ColorProperty("disabledActionTextColor", disabledActionTextColor, defaultValue: null));
            properties.add( new DiagnosticsProperty<TextStyle>("contentTextStyle", contentTextStyle, defaultValue: null));
            properties.add( new FloatProperty("elevation", elevation, defaultValue: null));
            properties.add( new DiagnosticsProperty<ShapeBorder>("shape", shape, defaultValue: null));
            properties.add( new DiagnosticsProperty<SnackBarBehavior>("behavior", (SnackBarBehavior)behavior, defaultValue: null));
        }

        public bool Equals(SnackBarThemeData other)
        {
            if (ReferenceEquals(null, other)) {
                return false;
            }

            if (ReferenceEquals(this, other)) {
                return true;
            }

            return Equals(backgroundColor, other.backgroundColor) 
                   && Equals(actionTextColor, other.actionTextColor) 
                   && Equals(disabledActionTextColor, other.disabledActionTextColor) 
                   && Equals(contentTextStyle, other.contentTextStyle) 
                   && Nullable.Equals(elevation, other.elevation) 
                   && Equals(shape, other.shape) 
                   && behavior.Equals(other.behavior);
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

            return Equals((SnackBarThemeData) obj);
        }
        public static bool operator ==(SnackBarThemeData left, SnackBarThemeData right) {
            return Equals(left, right);
        }

        public static bool operator !=(SnackBarThemeData left, SnackBarThemeData right) {
            return !Equals(left, right);
        }


        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (backgroundColor != null ? backgroundColor.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (actionTextColor != null ? actionTextColor.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (disabledActionTextColor != null ? disabledActionTextColor.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (contentTextStyle != null ? contentTextStyle.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ elevation.GetHashCode();
                hashCode = (hashCode * 397) ^ (shape != null ? shape.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ behavior.GetHashCode();
                return hashCode;
            }
        }
    }

}