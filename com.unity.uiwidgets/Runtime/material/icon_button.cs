using uiwidgets;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.rendering;
using Unity.UIWidgets.ui;
using Unity.UIWidgets.widgets;
using UnityEngine;
using Color = Unity.UIWidgets.ui.Color;

namespace Unity.UIWidgets.material {
    public partial class material_ {
        public const float _kMinButtonSize = kMinInteractiveDimension;
    }


    public class IconButton : StatelessWidget {
        public IconButton(
            Key key = null,
            float iconSize = 24.0f,
            VisualDensity visualDensity = null,
            EdgeInsetsGeometry padding = null,
            AlignmentGeometry alignment = null,
            Widget icon = null,
            Color color = null,
            Color focusColor = null,
            Color hoverColor = null,
            Color highlightColor = null,
            Color splashColor = null,
            Color disabledColor = null,
            VoidCallback onPressed = null,
            FocusNode focusNode = null,
            bool autofocus = false,
            string tooltip = null,
            bool? enableFeedback = true,
            BoxConstraints constraints = null
        ) : base(key: key) {
            D.assert(icon != null);
            this.iconSize = iconSize;
            this.visualDensity = visualDensity;
            this.padding = padding ?? EdgeInsets.all(8.0f);
            this.alignment = alignment ?? Alignment.center;
            this.icon = icon;
            this.color = color;
            this.focusColor = focusColor;
            this.hoverColor = hoverColor;
            this.highlightColor = highlightColor;
            this.splashColor = splashColor;
            this.disabledColor = disabledColor;
            this.onPressed = onPressed;
            this.focusNode = focusNode;
            this.autofocus = autofocus;
            this.tooltip = tooltip;
            this.enableFeedback = enableFeedback;
            this.constraints = constraints;
        }

        public readonly float iconSize;

        public readonly VisualDensity visualDensity;

        public readonly EdgeInsetsGeometry padding;

        public readonly AlignmentGeometry alignment;

        public readonly Widget icon;

        public readonly Color focusColor;

        public readonly Color hoverColor;

        public readonly Color color;

        public readonly Color splashColor;

        public readonly Color highlightColor;

        public readonly Color disabledColor;

        public readonly VoidCallback onPressed;

        public readonly FocusNode focusNode;

        public readonly bool autofocus;

        public readonly string tooltip;

        public readonly bool? enableFeedback;

        public readonly BoxConstraints constraints;

        public override Widget build(BuildContext context) {
            D.assert(material_.debugCheckHasMaterial(context));
            ThemeData theme = Theme.of(context);
            Color currentColor;
            if (onPressed != null) {
                currentColor = color;
            }
            else {
                currentColor = disabledColor ?? Theme.of(context).disabledColor;
            }

            VisualDensity effectiveVisualDensity = visualDensity ?? theme.visualDensity;

            BoxConstraints unadjustedConstraints = constraints ?? new BoxConstraints(
                minWidth: material_._kMinButtonSize,
                minHeight: material_._kMinButtonSize
            );
            BoxConstraints adjustedConstraints = effectiveVisualDensity.effectiveConstraints(unadjustedConstraints);


            Widget result = new ConstrainedBox(
                constraints: adjustedConstraints,
                child: new Padding(
                    padding: padding,
                    child: new SizedBox(
                        height: iconSize,
                        width: iconSize,
                        child: new Align(
                            alignment: alignment,
                            child: IconTheme.merge(
                                data: new IconThemeData(
                                    size: iconSize,
                                    color: currentColor
                                ),
                                child: icon
                            )
                        )
                    )
                )
            );

            if (tooltip != null) {
                result = new Tooltip(
                    message: tooltip,
                    child: result);
            }

            return new InkResponse(
                focusNode: focusNode,
                autofocus: autofocus,
                canRequestFocus: onPressed != null,
                onTap: () => {
                    if (onPressed != null) {
                        onPressed();
                    }
                },
                enableFeedback: enableFeedback ?? false,
                child: result,
                focusColor: focusColor ?? Theme.of(context).focusColor,
                hoverColor: hoverColor ?? Theme.of(context).hoverColor,
                highlightColor: highlightColor ?? Theme.of(context).highlightColor,
                splashColor: splashColor ?? Theme.of(context).splashColor,
                radius: Mathf.Max(
                    Material.defaultSplashRadius,
                    (iconSize + Mathf.Min(padding.horizontal, padding.vertical)) * 0.7f)
            );
        }


        public override void debugFillProperties(DiagnosticPropertiesBuilder properties) {
            base.debugFillProperties(properties);
            properties.add(new DiagnosticsProperty<Widget>("icon", icon, showName: false));
            properties.add(new StringProperty("tooltip", tooltip, defaultValue: null, quoted: false));
            properties.add(new ObjectFlagProperty<VoidCallback>("onPressed", onPressed, ifNull: "disabled"));
            properties.add(new StringProperty("tooltip", tooltip, defaultValue: null, quoted: false));
            properties.add(new ColorProperty("color", color, defaultValue: null));
            properties.add(new ColorProperty("disabledColor", disabledColor, defaultValue: null));
            properties.add(new ColorProperty("focusColor", focusColor, defaultValue: null));
            properties.add(new ColorProperty("hoverColor", hoverColor, defaultValue: null));
            properties.add(new ColorProperty("highlightColor", highlightColor, defaultValue: null));
            properties.add(new ColorProperty("splashColor", splashColor, defaultValue: null));
            properties.add(new DiagnosticsProperty<EdgeInsetsGeometry>("padding", padding, defaultValue: null));
            properties.add(new DiagnosticsProperty<FocusNode>("focusNode", focusNode, defaultValue: null));
        }
    }
}