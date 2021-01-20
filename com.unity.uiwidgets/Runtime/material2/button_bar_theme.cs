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
    public class ButtonBarThemeData : Diagnosticable,IEquatable<ButtonBarThemeData> {
        public ButtonBarThemeData(
            MainAxisAlignment? alignment = null,
            MainAxisSize? mainAxisSize = null, 
            ButtonTextTheme? buttonTextTheme = null,
            float? buttonMinWidth = null,
            float? buttonHeight = null,
            [CanBeNull] EdgeInsetsGeometry buttonPadding = null,
            bool? buttonAlignedDropdown = null,
            ButtonBarLayoutBehavior? layoutBehavior = null,
            VerticalDirection? overflowDirection = null
            ) { 
            D.assert(buttonMinWidth == null || buttonMinWidth >= 0.0);
            D.assert(buttonHeight == null || buttonHeight >= 0.0);
            this.alignment = alignment;
            this.mainAxisSize = mainAxisSize;
            this.buttonTextTheme = buttonTextTheme;
            this.buttonMinWidth = buttonMinWidth;
            this.buttonHeight = buttonHeight;
            this.buttonPadding = buttonPadding;
            this.buttonAlignedDropdown = buttonAlignedDropdown;
            this.layoutBehavior = layoutBehavior;
            this.overflowDirection = overflowDirection;
        }
        public readonly MainAxisAlignment? alignment;
        public readonly MainAxisSize? mainAxisSize; 
        public readonly ButtonTextTheme? buttonTextTheme;
        public readonly float? buttonMinWidth;
        public readonly float? buttonHeight;
        public readonly EdgeInsetsGeometry buttonPadding;
        public readonly bool? buttonAlignedDropdown;
        public readonly ButtonBarLayoutBehavior? layoutBehavior;
        public readonly VerticalDirection? overflowDirection;

        public ButtonBarThemeData copyWith(
            MainAxisAlignment? alignment = null,
            MainAxisSize? mainAxisSize = null, 
            ButtonTextTheme? buttonTextTheme = null,
            float? buttonMinWidth = null,
            float? buttonHeight = null,
            [CanBeNull] EdgeInsetsGeometry buttonPadding = null,
            bool? buttonAlignedDropdown = null,
            ButtonBarLayoutBehavior? layoutBehavior = null,
            VerticalDirection? overflowDirection = null
            ) { 
            return new ButtonBarThemeData(
              alignment: alignment ?? this.alignment,
              mainAxisSize: mainAxisSize ?? this.mainAxisSize,
              buttonTextTheme: buttonTextTheme ?? this.buttonTextTheme,
              buttonMinWidth: buttonMinWidth ?? this.buttonMinWidth,
              buttonHeight: buttonHeight ?? this.buttonHeight,
              buttonPadding: buttonPadding ?? this.buttonPadding,
              buttonAlignedDropdown: buttonAlignedDropdown ?? this.buttonAlignedDropdown,
              layoutBehavior: layoutBehavior ?? this.layoutBehavior,
              overflowDirection: overflowDirection ?? this.overflowDirection
            );
        }
        public static ButtonBarThemeData lerp(ButtonBarThemeData a, ButtonBarThemeData b, float t) {
            D.assert(t != null);
            if (a == null && b == null)
              return null;
            return new ButtonBarThemeData(
              alignment: t < 0.5 ? a.alignment : b.alignment,
              mainAxisSize: t < 0.5 ? a.mainAxisSize : b.mainAxisSize,
              buttonTextTheme: t < 0.5 ? a.buttonTextTheme : b.buttonTextTheme,
              buttonMinWidth: MathUtils.lerpFloat((float)a?.buttonMinWidth, (float)b?.buttonMinWidth, t),
              buttonHeight: MathUtils.lerpFloat((float)a?.buttonHeight, (float)b?.buttonHeight, t),
              buttonPadding: EdgeInsetsGeometry.lerp(a?.buttonPadding, b?.buttonPadding, t),
              buttonAlignedDropdown: t < 0.5 ? a.buttonAlignedDropdown : b.buttonAlignedDropdown,
              layoutBehavior: t < 0.5 ? a.layoutBehavior : b.layoutBehavior,
              overflowDirection: t < 0.5 ? a.overflowDirection : b.overflowDirection
            );
        }

        public override void debugFillProperties(DiagnosticPropertiesBuilder properties) {
            base.debugFillProperties(properties);
            properties.add( new DiagnosticsProperty<MainAxisAlignment>("alignment", alignment.Value, defaultValue: null));
            properties.add( new DiagnosticsProperty<MainAxisSize>("mainAxisSize", mainAxisSize.Value, defaultValue: null));
            properties.add( new DiagnosticsProperty<ButtonTextTheme>("textTheme", buttonTextTheme.Value, defaultValue: null));
            properties.add( new FloatProperty("minWidth", buttonMinWidth, defaultValue: null));
            properties.add( new FloatProperty("height", buttonHeight, defaultValue: null));
            properties.add( new DiagnosticsProperty<EdgeInsetsGeometry>("padding", buttonPadding, defaultValue: null));
            properties.add( new FlagProperty(
                "buttonAlignedDropdown",
                value: buttonAlignedDropdown,
                ifTrue: "dropdown width matches button",
                defaultValue: null));
            properties.add(new DiagnosticsProperty<ButtonBarLayoutBehavior>("layoutBehavior", layoutBehavior.Value, defaultValue: null));
            properties.add(new DiagnosticsProperty<VerticalDirection>("overflowDirection", overflowDirection.Value, defaultValue: null));
        }

        public bool Equals(ButtonBarThemeData other)
        {
            if (ReferenceEquals(null, other)) {
                return false;
            }

            if (ReferenceEquals(this, other)) {
                return true;
            }

            return alignment == other.alignment 
                   && mainAxisSize == other.mainAxisSize 
                   && buttonTextTheme == other.buttonTextTheme 
                   && Nullable.Equals(buttonMinWidth, other.buttonMinWidth) 
                   && Nullable.Equals(buttonHeight, other.buttonHeight) 
                   && Equals(buttonPadding, other.buttonPadding) 
                   && buttonAlignedDropdown == other.buttonAlignedDropdown 
                   && layoutBehavior == other.layoutBehavior 
                   && overflowDirection == other.overflowDirection;
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

            return Equals((ButtonBarThemeData) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = alignment.GetHashCode();
                hashCode = (hashCode * 397) ^ mainAxisSize.GetHashCode();
                hashCode = (hashCode * 397) ^ buttonTextTheme.GetHashCode();
                hashCode = (hashCode * 397) ^ buttonMinWidth.GetHashCode();
                hashCode = (hashCode * 397) ^ buttonHeight.GetHashCode();
                hashCode = (hashCode * 397) ^ (buttonPadding != null ? buttonPadding.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ buttonAlignedDropdown.GetHashCode();
                hashCode = (hashCode * 397) ^ layoutBehavior.GetHashCode();
                hashCode = (hashCode * 397) ^ overflowDirection.GetHashCode();
                return hashCode;
            }
        }
    }
}