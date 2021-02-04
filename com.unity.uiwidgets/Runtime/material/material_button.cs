using System;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.service;
using Unity.UIWidgets.ui;
using Unity.UIWidgets.widgets;

namespace Unity.UIWidgets.material {
    public class MaterialButton : StatelessWidget {
        public MaterialButton(
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
            bool? autofocus = false,
            MaterialTapTargetSize? materialTapTargetSize = null,
            TimeSpan? animationDuration = null,
            float? minWidth = null,
            float? height = null,
            bool? enableFeedback = true,
            Widget child = null
        ) : base(key: key) {
            D.assert(clipBehavior != null);
            D.assert(autofocus != null);
            D.assert(elevation == null || elevation >= 0.0f);
            D.assert(focusElevation == null || focusElevation >= 0.0f);
            D.assert(hoverElevation == null || hoverElevation >= 0.0f);
            D.assert(highlightElevation == null || highlightElevation >= 0.0f);
            D.assert(disabledElevation == null || disabledElevation >= 0.0f);
            this.onPressed = onPressed;
            this.onLongPress = onLongPress;
            this.onHighlightChanged = onHighlightChanged;
            this.textTheme = textTheme;
            this.textColor = textColor;
            this.disabledTextColor = disabledTextColor;
            this.color = color;
            this.disabledColor = disabledColor;
            this.focusColor = focusColor;
            this.hoverColor = hoverColor;
            this.highlightColor = highlightColor;
            this.splashColor = splashColor;
            this.colorBrightness = colorBrightness;
            this.elevation = elevation;
            this.focusElevation = focusElevation;
            this.hoverElevation = hoverElevation;
            this.highlightElevation = highlightElevation;
            this.disabledElevation = disabledElevation;
            this.padding = padding;
            this.visualDensity = visualDensity;
            this.shape = shape;
            this.clipBehavior = clipBehavior;
            this.focusNode = focusNode;
            this.materialTapTargetSize = materialTapTargetSize;
            this.animationDuration = animationDuration;
            this.minWidth = minWidth;
            this.height = height;
            this.enableFeedback = enableFeedback;
            this.child = child;
            this.autofocus = autofocus;
        }

        public readonly VoidCallback onPressed;

        public readonly VoidCallback onLongPress;

        public readonly ValueChanged<bool> onHighlightChanged;

        public readonly ButtonTextTheme? textTheme;

        public readonly Color textColor;

        public readonly Color disabledTextColor;

        public readonly Color color;

        public readonly Color disabledColor;

        public readonly Color splashColor;

        public readonly Color focusColor;

        public readonly Color hoverColor;

        public readonly Color highlightColor;

        public readonly float? elevation;

        public readonly float? hoverElevation;

        public readonly float? focusElevation;

        public readonly float? highlightElevation;

        public readonly float? disabledElevation;

        public readonly Brightness? colorBrightness;

        public readonly Widget child;

        public bool enabled {
            get { return onPressed != null || onLongPress != null; }
        }

        public readonly EdgeInsetsGeometry padding;

        public readonly VisualDensity visualDensity;

        public readonly ShapeBorder shape;

        public readonly Clip? clipBehavior;

        public readonly FocusNode focusNode;

        public readonly bool? autofocus;

        public readonly TimeSpan? animationDuration;

        public readonly MaterialTapTargetSize? materialTapTargetSize;

        public readonly float? minWidth;

        public readonly float? height;

        public readonly bool? enableFeedback;

        public override Widget build(BuildContext context) {
            ThemeData theme = Theme.of(context);
            ButtonThemeData buttonTheme = ButtonTheme.of(context);

            return new RawMaterialButton(
                onPressed: onPressed,
                onLongPress:() => onLongPress?.Invoke(),
                enableFeedback: enableFeedback?? true,
                onHighlightChanged: onHighlightChanged,
                fillColor: buttonTheme.getFillColor(this),
                textStyle: theme.textTheme.button.copyWith(color: buttonTheme.getTextColor(this)),
                focusColor: focusColor ?? buttonTheme.getFocusColor(this) ?? theme.focusColor,
                hoverColor: hoverColor ?? buttonTheme.getHoverColor(this) ?? theme.hoverColor,
                highlightColor: highlightColor ?? theme.highlightColor,
                splashColor: splashColor ?? theme.splashColor,
                elevation: buttonTheme.getElevation(this),
                focusElevation: buttonTheme.getFocusElevation(this),
                hoverElevation: buttonTheme.getHoverElevation(this),
                highlightElevation: buttonTheme.getHighlightElevation(this),
                padding: buttonTheme.getPadding(this),
                visualDensity: visualDensity ?? theme.visualDensity,
                constraints: buttonTheme.getConstraints(this).copyWith(
                    minWidth: minWidth,
                    minHeight: height),
                shape: buttonTheme.getShape(this),
                clipBehavior: clipBehavior ?? Clip.none,
                focusNode: focusNode,
                autofocus: autofocus ?? false,
                animationDuration: buttonTheme.getAnimationDuration(this),
                child: child,
                materialTapTargetSize: materialTapTargetSize ?? theme.materialTapTargetSize);
        }


        public override void debugFillProperties(DiagnosticPropertiesBuilder properties) {
            base.debugFillProperties(properties);
            properties.add(new FlagProperty("enabled", value: enabled, ifFalse: "disabled"));
            properties.add(new DiagnosticsProperty<ButtonTextTheme?>("textTheme", textTheme, defaultValue: null));
            properties.add(new ColorProperty("textColor", textColor, defaultValue: null));
            properties.add(new ColorProperty("disabledTextColor", disabledTextColor, defaultValue: null));
            properties.add(new ColorProperty("color", color, defaultValue: null));
            properties.add(new ColorProperty("disabledColor", disabledColor, defaultValue: null));
            properties.add(new ColorProperty("focusColor", focusColor, defaultValue: null));
            properties.add(new ColorProperty("hoverColor", hoverColor, defaultValue: null));
            properties.add(new ColorProperty("highlightColor", highlightColor, defaultValue: null));
            properties.add(new ColorProperty("splashColor", splashColor, defaultValue: null));
            properties.add(
                new DiagnosticsProperty<Brightness?>("colorBrightness", colorBrightness, defaultValue: null));
            properties.add(new DiagnosticsProperty<EdgeInsetsGeometry>("padding", padding, defaultValue: null));
            properties.add(new DiagnosticsProperty<VisualDensity>("visualDensity", visualDensity, defaultValue: null));
            properties.add(new DiagnosticsProperty<ShapeBorder>("shape", shape, defaultValue: null));
            properties.add(new DiagnosticsProperty<FocusNode>("focusNode", focusNode, defaultValue: null));
            properties.add(new DiagnosticsProperty<MaterialTapTargetSize?>("materialTapTargetSize",
                materialTapTargetSize, defaultValue: null));
        }
    }


    public interface MaterialButtonWithIconMixin {
    }
}