using uiwidgets;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.service;
using Unity.UIWidgets.ui;
using Unity.UIWidgets.widgets;
using UnityEngine;
using Color = Unity.UIWidgets.ui.Color;
using TextStyle = Unity.UIWidgets.painting.TextStyle;

namespace Unity.UIWidgets.material {
    public class ChipTheme : InheritedTheme {
        public ChipTheme(
            Key key = null,
            ChipThemeData data = null,
            Widget child = null
        ) : base(key: key, child: child) {
            D.assert(child != null);
            D.assert(data != null);
            this.data = data;
        }

        public readonly ChipThemeData data;

        public static ChipThemeData of(BuildContext context) {
            ChipTheme inheritedTheme = (ChipTheme) context.dependOnInheritedWidgetOfExactType<ChipTheme>();
            return inheritedTheme?.data ?? Theme.of(context).chipTheme;
        }

        public override Widget wrap(BuildContext context, Widget child) {
            ChipTheme ancestorTheme = context.findAncestorWidgetOfExactType<ChipTheme>();
            return ReferenceEquals(this, ancestorTheme) ? child : new ChipTheme(data: data, child: child);
        }

        public override bool updateShouldNotify(InheritedWidget _oldWidget) {
            ChipTheme oldWidget = _oldWidget as ChipTheme;
            return data != oldWidget.data;
        }
    }

    public class ChipThemeData : Diagnosticable {
        public ChipThemeData(
            Color backgroundColor = null,
            Color deleteIconColor = null,
            Color disabledColor = null,
            Color selectedColor = null,
            Color secondarySelectedColor = null,
            Color shadowColor = null,
            Color selectedShadowColor = null,
            bool? showCheckmark = null,
            Color checkmarkColor = null,
            EdgeInsetsGeometry labelPadding = null,
            EdgeInsetsGeometry padding = null,
            ShapeBorder shape = null,
            TextStyle labelStyle = null,
            TextStyle secondaryLabelStyle = null,
            Brightness? brightness = null,
            float? elevation = null,
            float? pressElevation = null
        ) {
            D.assert(backgroundColor != null);
            D.assert(disabledColor != null);
            D.assert(selectedColor != null);
            D.assert(secondarySelectedColor != null);
            D.assert(labelPadding != null);
            D.assert(padding != null);
            D.assert(shape != null);
            D.assert(labelStyle != null);
            D.assert(secondaryLabelStyle != null);
            D.assert(brightness != null);
            this.backgroundColor = backgroundColor;
            this.deleteIconColor = deleteIconColor;
            this.disabledColor = disabledColor;
            this.selectedColor = selectedColor;
            this.secondarySelectedColor = secondarySelectedColor;
            this.shadowColor = shadowColor;
            this.selectedShadowColor = selectedShadowColor;
            this.showCheckmark = showCheckmark;
            this.checkmarkColor = checkmarkColor;
            this.labelPadding = labelPadding;
            this.padding = padding;
            this.shape = shape;
            this.labelStyle = labelStyle;
            this.secondaryLabelStyle = secondaryLabelStyle;
            this.brightness = brightness;
            this.elevation = elevation;
            this.pressElevation = pressElevation;
        }

        public static ChipThemeData fromDefaults(
            Brightness? brightness = null,
            Color primaryColor = null,
            Color secondaryColor = null,
            TextStyle labelStyle = null
        ) {
            D.assert(primaryColor != null || brightness != null,
                () => "One of primaryColor or brightness must be specified");
            D.assert(primaryColor == null || brightness == null,
                () => "Only one of primaryColor or brightness may be specified");
            D.assert(secondaryColor != null);
            D.assert(labelStyle != null);

            if (primaryColor != null) {
                brightness = ThemeData.estimateBrightnessForColor(primaryColor);
            }

            const int backgroundAlpha = 0x1f; // 12%
            const int deleteIconAlpha = 0xde; // 87%
            const int disabledAlpha = 0x0c; // 38% * 12% = 5%
            const int selectAlpha = 0x3d; // 12% + 12% = 24%
            const int textLabelAlpha = 0xde; // 87%
            ShapeBorder shape = new StadiumBorder();
            EdgeInsetsGeometry labelPadding = EdgeInsets.symmetric(horizontal: 8.0f);
            EdgeInsetsGeometry padding = EdgeInsets.all(4.0f);

            primaryColor = primaryColor ?? (brightness == Brightness.light ? Colors.black : Colors.white);
            Color backgroundColor = primaryColor.withAlpha(backgroundAlpha);
            Color deleteIconColor = primaryColor.withAlpha(deleteIconAlpha);
            Color disabledColor = primaryColor.withAlpha(disabledAlpha);
            Color selectedColor = primaryColor.withAlpha(selectAlpha);
            Color secondarySelectedColor = secondaryColor.withAlpha(selectAlpha);
            TextStyle secondaryLabelStyle = labelStyle.copyWith(
                color: secondaryColor.withAlpha(textLabelAlpha)
            );
            labelStyle = labelStyle.copyWith(color: primaryColor.withAlpha(textLabelAlpha));

            return new ChipThemeData(
                backgroundColor: backgroundColor,
                deleteIconColor: deleteIconColor,
                disabledColor: disabledColor,
                selectedColor: selectedColor,
                secondarySelectedColor: secondarySelectedColor,
                labelPadding: labelPadding,
                padding: padding,
                shape: shape,
                labelStyle: labelStyle,
                secondaryLabelStyle: secondaryLabelStyle,
                brightness: brightness
            );
        }

        public readonly Color backgroundColor;

        public readonly Color deleteIconColor;

        public readonly Color disabledColor;

        public readonly Color selectedColor;

        public readonly Color secondarySelectedColor;

        public readonly Color shadowColor;

        public readonly Color selectedShadowColor;

        public readonly bool? showCheckmark;

        public readonly Color checkmarkColor;

        public readonly EdgeInsetsGeometry labelPadding;

        public readonly EdgeInsetsGeometry padding;

        public readonly ShapeBorder shape;

        public readonly TextStyle labelStyle;

        public readonly TextStyle secondaryLabelStyle;

        public readonly Brightness? brightness;

        public readonly float? elevation;

        public readonly float? pressElevation;

        public ChipThemeData copyWith(
            Color backgroundColor = null,
            Color deleteIconColor = null,
            Color disabledColor = null,
            Color selectedColor = null,
            Color secondarySelectedColor = null,
            Color shadowColor = null,
            Color selectedShadowColor = null,
            Color checkmarkColor = null,
            EdgeInsetsGeometry labelPadding = null,
            EdgeInsetsGeometry padding = null,
            ShapeBorder shape = null,
            TextStyle labelStyle = null,
            TextStyle secondaryLabelStyle = null,
            Brightness? brightness = null,
            float? elevation = null,
            float? pressElevation = null
        ) {
            return new ChipThemeData(
                backgroundColor: backgroundColor ?? this.backgroundColor,
                deleteIconColor: deleteIconColor ?? this.deleteIconColor,
                disabledColor: disabledColor ?? this.disabledColor,
                selectedColor: selectedColor ?? this.selectedColor,
                secondarySelectedColor: secondarySelectedColor ?? this.secondarySelectedColor,
                shadowColor: shadowColor ?? this.shadowColor,
                selectedShadowColor: selectedShadowColor ?? this.selectedShadowColor,
                checkmarkColor: checkmarkColor ?? this.checkmarkColor,
                labelPadding: labelPadding ?? this.labelPadding,
                padding: padding ?? this.padding,
                shape: shape ?? this.shape,
                labelStyle: labelStyle ?? this.labelStyle,
                secondaryLabelStyle: secondaryLabelStyle ?? this.secondaryLabelStyle,
                brightness: brightness ?? this.brightness,
                elevation: elevation ?? this.elevation,
                pressElevation: pressElevation ?? this.pressElevation
            );
        }

        public static ChipThemeData lerp(ChipThemeData a, ChipThemeData b, float t) {
            if (a == null && b == null) {
                return null;
            }

            return new ChipThemeData(
                backgroundColor: Color.lerp(a?.backgroundColor, b?.backgroundColor, t),
                deleteIconColor: Color.lerp(a?.deleteIconColor, b?.deleteIconColor, t),
                disabledColor: Color.lerp(a?.disabledColor, b?.disabledColor, t),
                selectedColor: Color.lerp(a?.selectedColor, b?.selectedColor, t),
                secondarySelectedColor: Color.lerp(a?.secondarySelectedColor, b?.secondarySelectedColor, t),
                shadowColor: Color.lerp(a?.shadowColor, b?.shadowColor, t),
                selectedShadowColor: Color.lerp(a?.selectedShadowColor, b?.selectedShadowColor, t),
                checkmarkColor: Color.lerp(a?.checkmarkColor, b?.checkmarkColor, t),
                labelPadding: EdgeInsetsGeometry.lerp(a?.labelPadding, b?.labelPadding, t),
                padding: EdgeInsetsGeometry.lerp(a?.padding, b?.padding, t),
                shape: ShapeBorder.lerp(a?.shape, b?.shape, t),
                labelStyle: TextStyle.lerp(a?.labelStyle, b?.labelStyle, t),
                secondaryLabelStyle: TextStyle.lerp(a?.secondaryLabelStyle, b?.secondaryLabelStyle, t),
                brightness: t < 0.5f ? a?.brightness ?? Brightness.light : b?.brightness ?? Brightness.light,
                elevation: MathUtils.lerpNullableFloat(a?.elevation, b?.elevation, t),
                pressElevation: MathUtils.lerpNullableFloat(a?.pressElevation, b?.pressElevation, t)
            );
        }

        public override int GetHashCode() {
            var hashCode = backgroundColor.GetHashCode();
            hashCode = (hashCode * 397) ^ deleteIconColor.GetHashCode();
            hashCode = (hashCode * 397) ^ disabledColor.GetHashCode();
            hashCode = (hashCode * 397) ^ selectedColor.GetHashCode();
            hashCode = (hashCode * 397) ^ secondarySelectedColor.GetHashCode();
            hashCode = (hashCode * 397) ^ shadowColor?.GetHashCode() ?? 0;
            hashCode = (hashCode * 397) ^ selectedShadowColor?.GetHashCode() ?? 0;
            hashCode = (hashCode * 397) ^ checkmarkColor?.GetHashCode() ?? 0;
            hashCode = (hashCode * 397) ^ labelPadding.GetHashCode();
            hashCode = (hashCode * 397) ^ padding.GetHashCode();
            hashCode = (hashCode * 397) ^ shape.GetHashCode();
            hashCode = (hashCode * 397) ^ labelStyle.GetHashCode();
            hashCode = (hashCode * 397) ^ secondaryLabelStyle.GetHashCode();
            hashCode = (hashCode * 397) ^ brightness.GetHashCode();
            hashCode = (hashCode * 397) ^ elevation.GetHashCode();
            hashCode = (hashCode * 397) ^ pressElevation.GetHashCode();
            return hashCode;
        }

        public bool Equals(ChipThemeData other) {
            return other.backgroundColor == backgroundColor
                   && other.deleteIconColor == deleteIconColor
                   && other.disabledColor == disabledColor
                   && other.selectedColor == selectedColor
                   && other.secondarySelectedColor == secondarySelectedColor
                   && other.shadowColor == shadowColor
                   && other.selectedShadowColor == selectedShadowColor
                   && other.checkmarkColor == checkmarkColor
                   && other.labelPadding == labelPadding
                   && other.padding == padding
                   && other.shape == shape
                   && other.labelStyle == labelStyle
                   && other.secondaryLabelStyle == secondaryLabelStyle
                   && other.brightness == brightness
                   && other.elevation == elevation
                   && other.pressElevation == pressElevation;
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

            return Equals((ChipThemeData) obj);
        }

        public static bool operator ==(ChipThemeData left, ChipThemeData right) {
            return Equals(left, right);
        }

        public static bool operator !=(ChipThemeData left, ChipThemeData right) {
            return !Equals(left, right);
        }

        public override void debugFillProperties(DiagnosticPropertiesBuilder properties) {
            base.debugFillProperties(properties);
            ThemeData defaultTheme = ThemeData.fallback();
            ChipThemeData defaultData = fromDefaults(
                secondaryColor: defaultTheme.primaryColor,
                brightness: defaultTheme.brightness,
                labelStyle: defaultTheme.textTheme.body2
            );
            properties.add(new ColorProperty("backgroundColor", backgroundColor,
                defaultValue: defaultData.backgroundColor));
            properties.add(new ColorProperty("deleteIconColor", deleteIconColor,
                defaultValue: defaultData.deleteIconColor));
            properties.add(new ColorProperty("disabledColor", disabledColor,
                defaultValue: defaultData.disabledColor));
            properties.add(new ColorProperty("selectedColor", selectedColor,
                defaultValue: defaultData.selectedColor));
            properties.add(new ColorProperty("secondarySelectedColor", secondarySelectedColor,
                defaultValue: defaultData.secondarySelectedColor));
            properties.add(new ColorProperty("shadowColor", shadowColor,
                defaultValue: defaultData.shadowColor));
            properties.add(new ColorProperty("selectedShadowColor", selectedShadowColor,
                defaultValue: defaultData.selectedShadowColor));
            properties.add(new ColorProperty("checkMarkColor", checkmarkColor,
                defaultValue: defaultData.checkmarkColor));
            properties.add(new DiagnosticsProperty<EdgeInsetsGeometry>("labelPadding", labelPadding,
                defaultValue: defaultData.labelPadding));
            properties.add(
                new DiagnosticsProperty<EdgeInsetsGeometry>("padding", padding, defaultValue: defaultData.padding));
            properties.add(new DiagnosticsProperty<ShapeBorder>("shape", shape, defaultValue: defaultData.shape));
            properties.add(new DiagnosticsProperty<TextStyle>("labelStyle", labelStyle,
                defaultValue: defaultData.labelStyle));
            properties.add(new DiagnosticsProperty<TextStyle>("secondaryLabelStyle", secondaryLabelStyle,
                defaultValue: defaultData.secondaryLabelStyle));
            properties.add(new EnumProperty<Brightness?>("brightness", brightness,
                defaultValue: defaultData.brightness));
            properties.add(new FloatProperty("elevation", elevation, defaultValue: defaultData.elevation));
            properties.add(new FloatProperty("pressElevation", pressElevation,
                defaultValue: defaultData.pressElevation));
        }
    }
}