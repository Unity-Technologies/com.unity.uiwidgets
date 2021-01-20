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
    public class ToggleButtonsThemeData : Diagnosticable,IEquatable<ToggleButtonsThemeData> {
        public  ToggleButtonsThemeData(
            TextStyle textStyle = null,
            BoxConstraints constraints = null,
            Color color = null,
            Color selectedColor = null,
            Color disabledColor = null,
            Color fillColor = null,
            Color focusColor = null, 
            Color highlightColor = null, 
            Color splashColor = null, 
            Color hoverColor = null, 
            Color borderColor = null, 
            Color selectedBorderColor = null, 
            Color disabledBorderColor = null,
            BorderRadius borderRadius = null,
            float? borderWidth = null
        ) {
            this.textStyle = textStyle;
            this.constraints = constraints;
            this.color = color;
            this.selectedColor = selectedColor;
            this.disabledColor = disabledColor;
            this.fillColor = fillColor;
            this.focusColor = focusColor;
            this.highlightColor = highlightColor;
            this.hoverColor = hoverColor;
            this.splashColor = splashColor;
            this.borderColor = borderColor;
            this.selectedBorderColor = selectedBorderColor;
            this.disabledBorderColor = disabledBorderColor;
            this.borderRadius = borderRadius;
            this.borderWidth = borderWidth;

        }
        public readonly  TextStyle textStyle;
        public readonly  BoxConstraints constraints;
        public readonly  Color color;
        public readonly  Color selectedColor; 
        public readonly  Color disabledColor; 
        public readonly  Color fillColor; 
        public readonly  Color focusColor; 
        public readonly  Color highlightColor; 
        public readonly  Color splashColor; 
        public readonly  Color hoverColor; 
        public readonly  Color borderColor; 
        public readonly  Color selectedBorderColor; 
        public readonly  Color disabledBorderColor; 
        public readonly  float? borderWidth; 
        public readonly  BorderRadius borderRadius;

  
        public ToggleButtonsThemeData copyWith(
            TextStyle textStyle = null,
            BoxConstraints constraints = null,
            Color color = null,
            Color selectedColor = null,
            Color disabledColor = null,
            Color fillColor = null,
            Color focusColor = null, 
            Color highlightColor = null, 
            Color splashColor = null, 
            Color hoverColor = null, 
            Color borderColor = null, 
            Color selectedBorderColor = null, 
            Color disabledBorderColor = null,
            BorderRadius borderRadius = null,
            float? borderWidth = null) {
            
            return new ToggleButtonsThemeData(
              textStyle: textStyle ?? this.textStyle,
              constraints: constraints ?? this.constraints,
              color: color ?? this.color,
              selectedColor: selectedColor ?? this.selectedColor,
              disabledColor: disabledColor ?? this.disabledColor,
              fillColor: fillColor ?? this.fillColor,
              focusColor: focusColor ?? this.focusColor,
              highlightColor: highlightColor ?? this.highlightColor,
              hoverColor: hoverColor ?? this.hoverColor,
              splashColor: splashColor ?? this.splashColor,
              borderColor: borderColor ?? this.borderColor,
              selectedBorderColor: selectedBorderColor ?? this.selectedBorderColor,
              disabledBorderColor: disabledBorderColor ?? this.disabledBorderColor,
              borderRadius: borderRadius ?? this.borderRadius,
              borderWidth: borderWidth ?? this.borderWidth
            );
        }
        public static ToggleButtonsThemeData lerp(ToggleButtonsThemeData a, ToggleButtonsThemeData b, float t) {
            D.assert (t != null);
            if (a == null && b == null)
              return null;
            return new ToggleButtonsThemeData(
              textStyle: TextStyle.lerp(a?.textStyle, b?.textStyle, t),
              constraints: BoxConstraints.lerp(a?.constraints, b?.constraints, t),
              color: Color.lerp(a?.color, b?.color, t),
              selectedColor: Color.lerp(a?.selectedColor, b?.selectedColor, t),
              disabledColor: Color.lerp(a?.disabledColor, b?.disabledColor, t),
              fillColor: Color.lerp(a?.fillColor, b?.fillColor, t),
              focusColor: Color.lerp(a?.focusColor, b?.focusColor, t),
              highlightColor: Color.lerp(a?.highlightColor, b?.highlightColor, t),
              hoverColor: Color.lerp(a?.hoverColor, b?.hoverColor, t),
              splashColor: Color.lerp(a?.splashColor, b?.splashColor, t),
              borderColor: Color.lerp(a?.borderColor, b?.borderColor, t),
              selectedBorderColor: Color.lerp(a?.selectedBorderColor, b?.selectedBorderColor, t),
              disabledBorderColor: Color.lerp(a?.disabledBorderColor, b?.disabledBorderColor, t),
              borderRadius: BorderRadius.lerp(a?.borderRadius, b?.borderRadius, t),
              borderWidth: MathUtils.lerpFloat((float)a?.borderWidth.Value, (float)b?.borderWidth.Value, t)
            );
        }
        public bool Equals(ToggleButtonsThemeData other) {
            if (ReferenceEquals(null, other)) {
                return false;
            }

            if (ReferenceEquals(this, other)) {
                return true;
            }
            return  Equals( textStyle, other.textStyle)
                    && Equals( constraints, other.constraints)
                    && Equals( color, other.color)
                    && Equals( selectedColor, other.selectedColor)
                    && Equals( disabledColor, other.disabledColor)
                    && Equals( fillColor, other.fillColor)
                    && Equals( focusColor, other.focusColor)
                    && Equals( highlightColor, other.highlightColor)
                    && Equals( hoverColor, other.hoverColor)
                    && Equals( splashColor, other.splashColor)
                    && Equals( borderColor, other.borderColor)
                    && Equals( selectedBorderColor, other.selectedBorderColor)
                    && Equals( disabledBorderColor, other.disabledBorderColor)
                    && Equals( borderRadius, other.borderRadius)
                    && Equals( borderWidth, other.borderWidth);
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

            return Equals((ToggleButtonsThemeData) obj);
        }
        public override int GetHashCode() {
            unchecked {
                var hashCode = (textStyle != null ? textStyle.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (constraints != null ? constraints.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (color != null ? color.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (selectedColor != null ? selectedColor.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (disabledColor != null ? disabledColor.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (fillColor != null ? fillColor.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (focusColor != null ? focusColor.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (highlightColor != null ? highlightColor.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (hoverColor != null ? hoverColor.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (splashColor != null ? splashColor.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (borderColor != null ? borderColor.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (selectedBorderColor != null ? selectedBorderColor.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (disabledBorderColor != null ? disabledBorderColor.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (borderRadius != null ? borderRadius.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (borderWidth != null ? borderWidth.GetHashCode() : 0);
                return hashCode;
            }

        }
        public static bool operator ==(ToggleButtonsThemeData left, ToggleButtonsThemeData right) {
            return Equals(left, right);
        }

        public static bool operator !=(ToggleButtonsThemeData left, ToggleButtonsThemeData right) {
            return !Equals(left, right);
        }


        public override void debugFillProperties(DiagnosticPropertiesBuilder properties) {
            base.debugFillProperties(properties);
            textStyle?.debugFillProperties(properties);//prefix: "textStyle.");
            properties.add( new DiagnosticsProperty<BoxConstraints>("constraints", constraints, defaultValue: null));
             properties.add( new ColorProperty("color", color, defaultValue: null));
             properties.add( new ColorProperty("selectedColor", selectedColor, defaultValue: null));
             properties.add( new ColorProperty("disabledColor", disabledColor, defaultValue: null));
             properties.add( new ColorProperty("fillColor", fillColor, defaultValue: null));
             properties.add( new ColorProperty("focusColor", focusColor, defaultValue: null));
             properties.add( new ColorProperty("highlightColor", highlightColor, defaultValue: null));
             properties.add( new ColorProperty("hoverColor", hoverColor, defaultValue: null));
             properties.add( new ColorProperty("splashColor", splashColor, defaultValue: null));
             properties.add( new ColorProperty("borderColor", borderColor, defaultValue: null));
             properties.add( new ColorProperty("selectedBorderColor", selectedBorderColor, defaultValue: null));
             properties.add( new ColorProperty("disabledBorderColor", disabledBorderColor, defaultValue: null));
             properties.add( new DiagnosticsProperty<BorderRadius>("borderRadius", borderRadius, defaultValue: null));
             properties.add( new FloatProperty("borderWidth", borderWidth, defaultValue: null));
        }
    }
}