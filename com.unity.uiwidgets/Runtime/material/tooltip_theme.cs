using System;
using System.Collections.Generic;
using Unity.UIWidgets.animation;
using Unity.UIWidgets.async2;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.gestures;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.rendering;
using Unity.UIWidgets.service;
using Unity.UIWidgets.ui;
using Unity.UIWidgets.widgets;
using UnityEngine;
using Brightness = Unity.UIWidgets.ui.Brightness;
using Object = System.Object;
using TextStyle = Unity.UIWidgets.painting.TextStyle;

namespace Unity.UIWidgets.material {
    public class TooltipThemeData : Diagnosticable, IEquatable<TooltipThemeData> {
        public TooltipThemeData(
            float? height = null,
            EdgeInsetsGeometry padding = null,
            EdgeInsetsGeometry margin = null,
            float? verticalOffset = null,
            bool? preferBelow = null,
            bool? excludeFromSemantics = null,
            Decoration decoration = null,
            TextStyle textStyle = null,
            TimeSpan? waitDuration = null,
            TimeSpan? showDuration = null
        ) {
            this.height = height;
            this.padding = padding;
            this.margin = margin;
            this.verticalOffset = verticalOffset;
            this.preferBelow = preferBelow;
            this.excludeFromSemantics = excludeFromSemantics;
            this.decoration = decoration;
            this.textStyle = textStyle;
            this.waitDuration = waitDuration;
            this.showDuration = showDuration;
        }
        public readonly float? height;
        public readonly EdgeInsetsGeometry padding;
        public readonly EdgeInsetsGeometry margin;
        public readonly float? verticalOffset;
        public readonly bool? preferBelow;
        public readonly bool? excludeFromSemantics;
        public readonly Decoration decoration;
        public readonly TextStyle textStyle;
        public readonly TimeSpan? waitDuration;
        public readonly TimeSpan? showDuration;

        public TooltipThemeData copyWith(
            float? height = null,
            EdgeInsetsGeometry padding = null,
            EdgeInsetsGeometry margin = null,
            float? verticalOffset = null,
            bool? preferBelow = null,
            bool? excludeFromSemantics = null,
            Decoration decoration = null,
            TextStyle textStyle = null,
            TimeSpan? waitDuration = null,
            TimeSpan? showDuration = null) { 
            return new TooltipThemeData(
              height: height ?? this.height,
              padding: padding ?? this.padding,
              margin: margin ?? this.margin,
              verticalOffset: verticalOffset ?? this.verticalOffset,
              preferBelow: preferBelow ?? this.preferBelow,
              excludeFromSemantics: excludeFromSemantics ?? this.excludeFromSemantics,
              decoration: decoration ?? this.decoration,
              textStyle: textStyle ?? this.textStyle,
              waitDuration: waitDuration ?? this.waitDuration,
              showDuration: showDuration ?? this.showDuration
            );
        }
        public static TooltipThemeData lerp(TooltipThemeData a, TooltipThemeData b, float t) {
            if (a == null && b == null)
              return null;
            D.assert(t != null);
            return new TooltipThemeData(
              height: MathUtils.lerpFloat((float)a?.height.Value, (float)b?.height.Value, t),
              padding: EdgeInsetsGeometry.lerp(a?.padding, b?.padding, t),
              margin: EdgeInsetsGeometry.lerp(a?.margin, b?.margin, t),
              verticalOffset: MathUtils.lerpFloat((float)a?.verticalOffset.Value, (float)b?.verticalOffset.Value, t),
              preferBelow: t < 0.5 ? a.preferBelow: b.preferBelow,
              excludeFromSemantics: t < 0.5 ? a.excludeFromSemantics : b.excludeFromSemantics,
              decoration: Decoration.lerp(a?.decoration, b?.decoration, t),
              textStyle: TextStyle.lerp(a?.textStyle, b?.textStyle, t)
            );
        }
        public override void debugFillProperties(DiagnosticPropertiesBuilder properties) {
            base.debugFillProperties(properties);
            properties.add( new FloatProperty("height", height, defaultValue: null));
            properties.add( new DiagnosticsProperty<EdgeInsetsGeometry>("padding", padding, defaultValue: null));
            properties.add( new DiagnosticsProperty<EdgeInsetsGeometry>("margin", margin, defaultValue: null));
            properties.add( new FloatProperty("vertical offset", verticalOffset, defaultValue: null));
            properties.add( new FlagProperty("position", value: preferBelow, ifTrue: "below", ifFalse: "above", showName: true, defaultValue: null));
            properties.add( new FlagProperty("semantics", value: excludeFromSemantics, ifTrue: "excluded", showName: true, defaultValue: null));
            properties.add( new DiagnosticsProperty<Decoration>("decoration", decoration, defaultValue: null));
            properties.add( new DiagnosticsProperty<TextStyle>("textStyle", textStyle, defaultValue: null));
            properties.add( new DiagnosticsProperty<TimeSpan>("wait duration", (TimeSpan)waitDuration, defaultValue: TimeSpan.Zero));
            properties.add( new DiagnosticsProperty<TimeSpan>("show duration", (TimeSpan)showDuration, defaultValue: TimeSpan.Zero));
        }
        public bool Equals(TooltipThemeData other) 
        { 
            if (ReferenceEquals(null, other)) { 
                return false;
            }

            if (ReferenceEquals(this, other)) { 
                return true;
            }

            return Nullable.Equals(height, other.height) 
                   && Equals(padding, other.padding) 
                   && Equals(margin, other.margin) 
                   && Nullable.Equals(verticalOffset, other.verticalOffset) 
                   && preferBelow == other.preferBelow 
                   && excludeFromSemantics == other.excludeFromSemantics 
                   && Equals(decoration, other.decoration) 
                   && Equals(textStyle, other.textStyle) 
                   && Nullable.Equals(waitDuration, other.waitDuration) 
                   && Nullable.Equals(showDuration, other.showDuration);
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
            return Equals((TooltipThemeData) obj);
        }
        
        public static bool operator ==(TooltipThemeData left, TooltipThemeData right) {
            return Equals(left, right);
        }

        public static bool operator !=(TooltipThemeData left, TooltipThemeData right) {
            return !Equals(left, right);
        }
        
        public override int GetHashCode()
        { 
            unchecked{ 
                var hashCode = height.GetHashCode();
                hashCode = (hashCode * 397) ^ (padding != null ? padding.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (margin != null ? margin.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ verticalOffset.GetHashCode();
                hashCode = (hashCode * 397) ^ preferBelow.GetHashCode();
                hashCode = (hashCode * 397) ^ excludeFromSemantics.GetHashCode();
                hashCode = (hashCode * 397) ^ (decoration != null ? decoration.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (textStyle != null ? textStyle.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ waitDuration.GetHashCode();
                hashCode = (hashCode * 397) ^ showDuration.GetHashCode();
                return hashCode;
            }
        }
    }
}