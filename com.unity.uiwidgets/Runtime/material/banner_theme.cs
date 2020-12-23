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
    public class MaterialBannerThemeData :Diagnosticable,IEquatable<MaterialBannerThemeData> {
        public MaterialBannerThemeData(
            Color backgroundColor = null,
            TextStyle contentTextStyle = null,
            EdgeInsetsGeometry padding = null,
            EdgeInsetsGeometry leadingPadding = null
        ) {
            this.backgroundColor = backgroundColor;
            this.contentTextStyle = contentTextStyle;
            this.padding = padding;
            this.leadingPadding = leadingPadding;
        }
        public readonly Color backgroundColor;
        public readonly TextStyle contentTextStyle;
        public readonly EdgeInsetsGeometry padding;
        public readonly EdgeInsetsGeometry leadingPadding;

        public MaterialBannerThemeData copyWith(
            Color backgroundColor = null,
            TextStyle contentTextStyle = null,
            EdgeInsetsGeometry padding = null,
            EdgeInsetsGeometry leadingPadding = null) {
            
            return new MaterialBannerThemeData(
              backgroundColor: backgroundColor ?? this.backgroundColor,
              contentTextStyle: contentTextStyle ?? this.contentTextStyle,
              padding: padding ?? this.padding,
              leadingPadding: leadingPadding ?? this.leadingPadding
            );
        }
        public static MaterialBannerThemeData lerp(MaterialBannerThemeData a, MaterialBannerThemeData b, float t) {
            D.assert(t != null);
            return new MaterialBannerThemeData(
              backgroundColor: Color.lerp(a?.backgroundColor, b?.backgroundColor, t),
              contentTextStyle: TextStyle.lerp(a?.contentTextStyle, b?.contentTextStyle, t),
              padding: EdgeInsetsGeometry.lerp(a?.padding, b?.padding, t),
              leadingPadding: EdgeInsetsGeometry.lerp(a?.leadingPadding, b?.leadingPadding, t)
            );
        }

        public override void debugFillProperties(DiagnosticPropertiesBuilder properties) {
            base.debugFillProperties(properties);
            properties.add( new ColorProperty("backgroundColor", backgroundColor, defaultValue: null));
            properties.add( new DiagnosticsProperty<TextStyle>("contentTextStyle", contentTextStyle, defaultValue: null));
            properties.add( new DiagnosticsProperty<EdgeInsetsGeometry>("padding", padding, defaultValue: null));
            properties.add( new DiagnosticsProperty<EdgeInsetsGeometry>("leadingPadding", leadingPadding, defaultValue: null));
        }

        public bool Equals(MaterialBannerThemeData other)
        {
            if (ReferenceEquals(null, other)) {
                return false;
            }

            if (ReferenceEquals(this, other)) {
                return true;
            }

            return Equals(backgroundColor, other.backgroundColor) 
                   && Equals(contentTextStyle, other.contentTextStyle) 
                   && Equals(padding, other.padding) 
                   && Equals(leadingPadding, other.leadingPadding);
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

            return Equals((MaterialBannerThemeData) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (backgroundColor != null ? backgroundColor.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (contentTextStyle != null ? contentTextStyle.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (padding != null ? padding.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (leadingPadding != null ? leadingPadding.GetHashCode() : 0);
                return hashCode;
            }
        }
    }
}