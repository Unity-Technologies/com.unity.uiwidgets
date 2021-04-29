using System.Collections.Generic;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.rendering;
using Unity.UIWidgets.service;
using Unity.UIWidgets.ui;
using Unity.UIWidgets.widgets;

namespace Unity.UIWidgets.material {
    public class FlatButton : MaterialButton {
        public FlatButton(
            Key key = null,
            VoidCallback onPressed = null,
            ValueChanged<bool> onHighlightChanged = null,
            VoidCallback onLongPress = null,
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
            EdgeInsetsGeometry padding = null,
            VisualDensity visualDensity = null,
            ShapeBorder shape = null,
            Clip clipBehavior = Clip.none,
            FocusNode focusNode = null,
            bool autofocus = false,
            MaterialTapTargetSize? materialTapTargetSize = null,
            Widget child = null) : base(
            key: key,
            onPressed: onPressed,
            onLongPress: onLongPress,
            onHighlightChanged: onHighlightChanged,
            textTheme: textTheme,
            textColor: textColor,
            disabledTextColor: disabledTextColor,
            focusColor: focusColor,
            hoverColor: hoverColor,
            color: color,
            disabledColor: disabledColor,
            highlightColor: highlightColor,
            splashColor: splashColor,
            colorBrightness: colorBrightness,
            padding: padding,
            visualDensity: visualDensity,
            shape: shape,
            clipBehavior: clipBehavior,
            focusNode: focusNode,
            autofocus: autofocus,
            materialTapTargetSize: materialTapTargetSize,
            child: child) {
        }

        public static FlatButton icon(
            Key key = null,
            VoidCallback onPressed = null,
            ValueChanged<bool> onHighlightChanged = null,
            VoidCallback onLongPress = null,
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
            EdgeInsetsGeometry padding = null,
            ShapeBorder shape = null,
            Clip clipBehavior = Clip.none,
            FocusNode focusNode = null,
            bool autofocus = false,
            MaterialTapTargetSize? materialTapTargetSize = null,
            Widget icon = null,
            Widget label = null) {
            D.assert(icon != null);
            D.assert(label != null);

            return new _FlatButtonWithIcon(
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
                padding: padding,
                shape: shape,
                clipBehavior: clipBehavior,
                focusNode: focusNode,
                autofocus: autofocus,
                materialTapTargetSize: materialTapTargetSize,
                icon: icon,
                label: label
            );
        }

        public override Widget build(BuildContext context) {
            ThemeData theme = Theme.of(context);
            ButtonThemeData buttonTheme = ButtonTheme.of(context);

            return new RawMaterialButton(
                onPressed: onPressed,
                onLongPress: () => onLongPress?.Invoke(),
                onHighlightChanged: onHighlightChanged,
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
                clipBehavior: clipBehavior ?? Clip.none,
                focusNode: focusNode,
                autofocus: autofocus ?? false,
                materialTapTargetSize: buttonTheme.getMaterialTapTargetSize(this),
                animationDuration: buttonTheme.getAnimationDuration(this),
                child: child
            );
        }
    }

    class _FlatButtonWithIcon : FlatButton, MaterialButtonWithIconMixin {
        public _FlatButtonWithIcon(
            Key key = null,
            VoidCallback onPressed = null,
            ValueChanged<bool> onHighlightChanged = null,
            VoidCallback onLongPress = null,
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
            EdgeInsetsGeometry padding = null,
            ShapeBorder shape = null,
            Clip clipBehavior = Clip.none,
            FocusNode focusNode = null,
            bool autofocus = false,
            MaterialTapTargetSize? materialTapTargetSize = null,
            Widget icon = null,
            Widget label = null) : base(
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
            padding: padding,
            shape: shape,
            clipBehavior: clipBehavior,
            focusNode: focusNode,
            autofocus: autofocus,
            materialTapTargetSize: materialTapTargetSize,
            child: new Row(
                mainAxisSize: MainAxisSize.min,
                children: new List<Widget> {
                    icon,
                    new SizedBox(width: 8.0f),
                    label
                }
            )) {
            D.assert(icon != null);
            D.assert(label != null);
        }
    }
}