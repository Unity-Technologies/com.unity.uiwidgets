using System;
using System.Collections.Generic;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.rendering;
using Unity.UIWidgets.ui;
using Unity.UIWidgets.widgets;

namespace Unity.UIWidgets.material {
    public class RaisedButton : MaterialButton {
        public RaisedButton(
            Key key = null,
            VoidCallback onPressed = null,
            VoidCallback onLongPress = null,
            ValueChanged<bool> onHighlightChanged = null,
            ButtonTextTheme? textTheme = null,
            Color textColor = null,
            Color disabledTextColor = null,
            Color color = null,
            Color disabledColor = null,
            Color focusColor = null,
            Color hoverColor = null,
            Color highlightColor = null,
            Color splashColor = null,
            Brightness? colorBrightness = null,
            float? elevation = null,
            float? focusElevation = null,
            float? hoverElevation = null,
            float? highlightElevation = null,
            float? disabledElevation = null,
            EdgeInsetsGeometry padding = null,
            VisualDensity visualDensity = null,
            ShapeBorder shape = null,
            Clip? clipBehavior = Clip.none,
            FocusNode focusNode = null,
            bool autofocus = false,
            MaterialTapTargetSize? materialTapTargetSize = null,
            TimeSpan? animationDuration = null,
            Widget child = null
        ) : base(
            key: key,
            onPressed: onPressed,
            onLongPress: onLongPress,
            onHighlightChanged: onHighlightChanged,
            textTheme: textTheme,
            textColor: textColor,
            disabledTextColor: disabledTextColor,
            color: color,
            disabledColor: disabledColor,
            focusColor: focusColor,
            hoverColor: hoverColor,
            highlightColor: highlightColor,
            splashColor: splashColor,
            colorBrightness: colorBrightness,
            elevation: elevation,
            focusElevation: focusElevation,
            hoverElevation: hoverElevation,
            highlightElevation: highlightElevation,
            disabledElevation: disabledElevation,
            padding: padding,
            visualDensity: visualDensity,
            shape: shape,
            clipBehavior: clipBehavior,
            focusNode: focusNode,
            autofocus: autofocus,
            materialTapTargetSize: materialTapTargetSize,
            animationDuration: animationDuration,
            child: child) {
            D.assert(elevation == null || elevation >= 0.0);
            D.assert(focusElevation == null || focusElevation >= 0.0);
            D.assert(hoverElevation == null || hoverElevation >= 0.0);
            D.assert(highlightElevation == null || highlightElevation >= 0.0);
            D.assert(disabledElevation == null || disabledElevation >= 0.0);
            D.assert(clipBehavior != null);
        }

        public static RaisedButton icon(
            Key key = null,
            VoidCallback onPressed = null,
            VoidCallback onLongPress = null,
            ValueChanged<bool> onHighlightChanged = null,
            ButtonTextTheme? textTheme = null,
            Color textColor = null,
            Color disabledTextColor = null,
            Color color = null,
            Color disabledColor = null,
            Color focusColor = null,
            Color hoverColor = null,
            Color highlightColor = null,
            Color splashColor = null,
            Brightness? colorBrightness = null,
            float? elevation = null,
            float? highlightElevation = null,
            float? disabledElevation = null,
            ShapeBorder shape = null,
            Clip? clipBehavior = null,
            FocusNode focusNode = null,
            bool autofocus = false,
            EdgeInsetsGeometry padding = null,
            MaterialTapTargetSize? materialTapTargetSize = null,
            TimeSpan? animationDuration = null,
            Widget icon = null,
            Widget label = null) {
            D.assert(icon != null);
            D.assert(label != null);

            clipBehavior = clipBehavior ?? Clip.none;

            return new _RaisedButtonWithIcon(
                key: key,
                onPressed: onPressed,
                onLongPress: onLongPress,
                onHighlightChanged: onHighlightChanged,
                textTheme: textTheme,
                textColor: textColor,
                disabledTextColor: disabledTextColor,
                color: color,
                disabledColor: disabledColor,
                focusColor: focusColor,
                hoverColor: hoverColor,
                highlightColor: highlightColor,
                splashColor: splashColor,
                colorBrightness: colorBrightness,
                elevation: elevation,
                highlightElevation: highlightElevation,
                disabledElevation: disabledElevation,
                padding: padding,
                shape: shape,
                clipBehavior: clipBehavior,
                focusNode: focusNode,
                autofocus: autofocus,
                materialTapTargetSize: materialTapTargetSize,
                animationDuration: animationDuration,
                icon: icon,
                label: label);
        }

        public override Widget build(BuildContext context) {
            ThemeData theme = Theme.of(context);
            ButtonThemeData buttonTheme = ButtonTheme.of(context);

            return new RawMaterialButton(
                onPressed: onPressed,
                onLongPress: () => onLongPress?.Invoke(),
                onHighlightChanged: onHighlightChanged,
                clipBehavior: clipBehavior.Value,
                fillColor: buttonTheme.getFillColor(this),
                textStyle: theme.textTheme.button.copyWith(color: buttonTheme.getTextColor(this)),
                focusColor: buttonTheme.getFocusColor(this),
                hoverColor: buttonTheme.getHoverColor(this),
                highlightColor: buttonTheme.getHighlightColor(this),
                splashColor: buttonTheme.getSplashColor(this),
                elevation: buttonTheme.getElevation(this),
                focusElevation: buttonTheme.getFocusElevation(this),
                hoverElevation: buttonTheme.getHoverElevation(this),
                highlightElevation: buttonTheme.getHighlightElevation(this),
                disabledElevation: buttonTheme.getDisabledElevation(this),
                padding: buttonTheme.getPadding(this),
                visualDensity: visualDensity ?? theme.visualDensity,
                constraints: buttonTheme.getConstraints(this),
                shape: buttonTheme.getShape(this),
                focusNode: focusNode,
                autofocus: autofocus.Value,
                animationDuration: buttonTheme.getAnimationDuration(this),
                materialTapTargetSize: buttonTheme.getMaterialTapTargetSize(this),
                child: child
            );
        }

        public override void debugFillProperties(DiagnosticPropertiesBuilder properties) {
            base.debugFillProperties(properties);
            properties.add(new ObjectFlagProperty<VoidCallback>("onPressed", onPressed, ifNull: "disabled"));
            properties.add(new DiagnosticsProperty<ButtonTextTheme?>("textTheme", textTheme, defaultValue: null));
            properties.add(new DiagnosticsProperty<Color>("textColor", textColor, defaultValue: null));
            properties.add(new DiagnosticsProperty<Color>("disabledTextColor", disabledTextColor,
                defaultValue: null));
            properties.add(new DiagnosticsProperty<Color>("color", color, defaultValue: null));
            properties.add(new DiagnosticsProperty<Color>("disabledColor", disabledColor, defaultValue: null));
            properties.add(new DiagnosticsProperty<Color>("highlightColor", highlightColor, defaultValue: null));
            properties.add(new DiagnosticsProperty<Color>("splashColor", splashColor, defaultValue: null));
            properties.add(new DiagnosticsProperty<Brightness?>("colorBrightness", colorBrightness,
                defaultValue: null));
            properties.add(new DiagnosticsProperty<float?>("elevation", elevation, defaultValue: null));
            properties.add(new DiagnosticsProperty<float?>("focusElevation", focusElevation, defaultValue: null));
            properties.add(new DiagnosticsProperty<float?>("hoverElevation", hoverElevation, defaultValue: null));
            properties.add(new DiagnosticsProperty<float?>("highlightElevation", highlightElevation,
                defaultValue: null));
            properties.add(new DiagnosticsProperty<float?>("disabledElevation", disabledElevation,
                defaultValue: null));
            properties.add(new DiagnosticsProperty<EdgeInsetsGeometry>("padding", padding, defaultValue: null));
            properties.add(new DiagnosticsProperty<ShapeBorder>("shape", shape, defaultValue: null));
            properties.add(new DiagnosticsProperty<MaterialTapTargetSize?>("materialTapTargetSize",
                materialTapTargetSize, defaultValue: null));
        }
    }

    class _RaisedButtonWithIcon : RaisedButton, MaterialButtonWithIconMixin {
        public _RaisedButtonWithIcon(
            Key key = null,
            VoidCallback onPressed = null,
            VoidCallback onLongPress = null,
            ValueChanged<bool> onHighlightChanged = null,
            ButtonTextTheme? textTheme = null,
            Color textColor = null,
            Color disabledTextColor = null,
            Color color = null,
            Color disabledColor = null,
            Color focusColor = null,
            Color hoverColor = null,
            Color highlightColor = null,
            Color splashColor = null,
            Brightness? colorBrightness = null,
            float? elevation = null,
            float? highlightElevation = null,
            float? disabledElevation = null,
            ShapeBorder shape = null,
            Clip? clipBehavior = Clip.none,
            FocusNode focusNode = null,
            bool autofocus = false,
            EdgeInsetsGeometry padding = null,
            MaterialTapTargetSize? materialTapTargetSize = null,
            TimeSpan? animationDuration = null,
            Widget icon = null,
            Widget label = null
        ) : base(
            key: key,
            onPressed: onPressed,
            onLongPress: onLongPress,
            onHighlightChanged: onHighlightChanged,
            textTheme: textTheme,
            textColor: textColor,
            disabledTextColor: disabledTextColor,
            color: color,
            disabledColor: disabledColor,
            focusColor: focusColor,
            hoverColor: hoverColor,
            highlightColor: highlightColor,
            splashColor: splashColor,
            colorBrightness: colorBrightness,
            elevation: elevation,
            highlightElevation: highlightElevation,
            disabledElevation: disabledElevation,
            shape: shape,
            clipBehavior: clipBehavior,
            focusNode: focusNode,
            autofocus: autofocus,
            padding: padding,
            materialTapTargetSize: materialTapTargetSize,
            animationDuration: animationDuration,
            child: new Row(
                mainAxisSize: MainAxisSize.min,
                children: new List<Widget> {
                    icon,
                    new SizedBox(width: 8.0f),
                    label
                }
            )) {
            D.assert(elevation == null || elevation >= 0.0);
            D.assert(highlightElevation == null || highlightElevation >= 0.0);
            D.assert(disabledElevation == null || disabledElevation >= 0.0);
            D.assert(icon != null);
            D.assert(label != null);
            D.assert(clipBehavior != null);
        }
    }
}