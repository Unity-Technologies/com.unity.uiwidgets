using System.Collections.Generic;
using uiwidgets;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.rendering;
using Unity.UIWidgets.ui;
using Unity.UIWidgets.widgets;
using UnityEngine;
using Color = Unity.UIWidgets.ui.Color;
using TextStyle = Unity.UIWidgets.painting.TextStyle;

namespace Unity.UIWidgets.material {
    public partial class material_ {
        public static readonly BoxConstraints _kSizeConstraints = BoxConstraints.tightFor(width: 56.0f, height: 56.0f);

        public static readonly BoxConstraints _kMiniSizeConstraints =
            BoxConstraints.tightFor(width: 40.0f, height: 40.0f);

        public static readonly BoxConstraints _kExtendedSizeConstraints =
            new BoxConstraints(minHeight: 48.0f, maxHeight: 48.0f);
    }


    class _DefaultHeroTag {
        public _DefaultHeroTag() {
        }

        public override string ToString() {
            return "<default FloatingActionButton tag>";
        }
    }

    public class FloatingActionButton : StatelessWidget {
        public FloatingActionButton(
            Key key = null,
            Widget child = null,
            string tooltip = null,
            Color foregroundColor = null,
            Color backgroundColor = null,
            Color focusColor = null,
            Color hoverColor = null,
            Color splashColor = null,
            object heroTag = null,
            float? elevation = null,
            float? focusElevation = null,
            float? hoverElevation = null,
            float? highlightElevation = null,
            float? disabledElevation = null,
            VoidCallback onPressed = null,
            bool mini = false,
            ShapeBorder shape = null,
            Clip clipBehavior = Clip.none,
            FocusNode focusNode = null,
            bool autofocus = false,
            MaterialTapTargetSize? materialTapTargetSize = null,
            bool isExtended = false,
            BoxConstraints _sizeConstraints = null
        ) : base(key: key) {
            D.assert(elevation == null || elevation >= 0.0f);
            D.assert(focusElevation == null || focusElevation >= 0.0);
            D.assert(hoverElevation == null || hoverElevation >= 0.0);
            D.assert(highlightElevation == null || highlightElevation >= 0.0f);
            D.assert(disabledElevation == null || disabledElevation >= 0.0f);
            heroTag = heroTag ?? new _DefaultHeroTag();
            this.child = child;
            this.tooltip = tooltip;
            this.foregroundColor = foregroundColor;
            this.backgroundColor = backgroundColor;
            this.focusColor = focusColor;
            this.hoverColor = hoverColor;
            this.splashColor = splashColor;
            this.heroTag = heroTag;
            this.elevation = elevation;
            this.focusElevation = focusElevation;
            this.hoverElevation = hoverElevation;
            this.highlightElevation = highlightElevation;
            this.onPressed = onPressed;
            this.mini = mini;
            this.shape = shape;
            this.clipBehavior = clipBehavior;
            this.focusNode = focusNode;
            this.autofocus = autofocus;
            this.materialTapTargetSize = materialTapTargetSize;
            this.isExtended = isExtended;
            this.disabledElevation = disabledElevation;
            this._sizeConstraints = _sizeConstraints ?? (mini
                ? material_._kMiniSizeConstraints
                : material_._kSizeConstraints);
        }

        public static FloatingActionButton extended(
            Key key = null,
            string tooltip = null,
            Color foregroundColor = null,
            Color backgroundColor = null,
            Color focusColor = null,
            Color hoverColor = null,
            object heroTag = null,
            float? elevation = null,
            float? focusElevation = null,
            float? hoverElevation = null,
            float? splashColor = null,
            float? highlightElevation = null,
            float? disabledElevation = null,
            VoidCallback onPressed = null,
            ShapeBorder shape = null,
            bool isExtended = true,
            MaterialTapTargetSize? materialTapTargetSize = null,
            Clip clipBehavior = Clip.none,
            FocusNode focusNode = null,
            bool autofocus = false,
            Widget icon = null,
            Widget label = null
        ) {
            D.assert(elevation == null || elevation >= 0.0f);
            D.assert(focusElevation == null || focusElevation >= 0.0);
            D.assert(hoverElevation == null || hoverElevation >= 0.0);
            D.assert(highlightElevation == null || highlightElevation >= 0.0f);
            D.assert(disabledElevation == null || disabledElevation >= 0.0f);
            D.assert(label != null);
            heroTag = heroTag ?? new _DefaultHeroTag();

            BoxConstraints _sizeConstraints = material_._kExtendedSizeConstraints;
            bool mini = false;
            Widget child = new _ChildOverflowBox(
                child: new Row(
                    mainAxisSize: MainAxisSize.min,
                    children: icon == null
                        ? new List<Widget> {
                            new SizedBox(width: 20.0f),
                            label,
                            new SizedBox(width: 20.0f),
                        }
                        : new List<Widget> {
                            new SizedBox(width: 16.0f),
                            icon,
                            new SizedBox(width: 8.0f),
                            label,
                            new SizedBox(width: 20.0f)
                        }));

            return new FloatingActionButton(
                key: key,
                child: child,
                tooltip: tooltip,
                foregroundColor: foregroundColor,
                backgroundColor: backgroundColor,
                heroTag: heroTag,
                elevation: elevation,
                highlightElevation: highlightElevation,
                disabledElevation: disabledElevation,
                onPressed: onPressed,
                mini: mini,
                shape: shape,
                clipBehavior: clipBehavior,
                materialTapTargetSize: materialTapTargetSize,
                isExtended: isExtended,
                _sizeConstraints: _sizeConstraints
            );
        }

        public readonly Widget child;

        public readonly string tooltip;

        public readonly Color foregroundColor;

        public readonly Color backgroundColor;

        public readonly Color focusColor;

        public readonly Color hoverColor;

        public readonly Color splashColor;

        public readonly object heroTag;

        public readonly VoidCallback onPressed;

        public readonly float? elevation;

        public readonly float? focusElevation;

        public readonly float? hoverElevation;

        public readonly float? highlightElevation;

        public readonly float? disabledElevation;

        public readonly bool mini;

        public readonly ShapeBorder shape;

        public readonly Clip clipBehavior;

        public readonly bool isExtended;

        public readonly FocusNode focusNode;

        public readonly bool autofocus;

        public readonly MaterialTapTargetSize? materialTapTargetSize;

        readonly BoxConstraints _sizeConstraints;

        const float _defaultFocusElevation = 8;
        const float _defaultHoverElevation = 10;
        const float _defaultElevation = 6;
        const float _defaultHighlightElevation = 12;
        readonly ShapeBorder _defaultShape = new CircleBorder();
        readonly ShapeBorder _defaultExtendedShape = new StadiumBorder();

        public override Widget build(BuildContext context) {
            ThemeData theme = Theme.of(context);
            FloatingActionButtonThemeData floatingActionButtonTheme = theme.floatingActionButtonTheme;

            if (this.foregroundColor == null && floatingActionButtonTheme.foregroundColor == null) {
                bool accentIsDark = theme.accentColorBrightness == Brightness.dark;
                Color defaultAccentIconThemeColor = accentIsDark ? Colors.white : Colors.black;
                if (theme.accentIconTheme.color != defaultAccentIconThemeColor) {
                    Debug.Log(
                        "Warning: " +
                        "The support for configuring the foreground color of " +
                        "FloatingActionButtons using ThemeData.accentIconTheme " +
                        "has been deprecated. Please use ThemeData.floatingActionButtonTheme " +
                        "instead. "
                    );
                }
            }


            Color foregroundColor = this.foregroundColor
                                    ?? floatingActionButtonTheme.foregroundColor
                                    ?? theme.colorScheme.onSecondary;
            Color backgroundColor = this.backgroundColor
                                    ?? floatingActionButtonTheme.backgroundColor
                                    ?? theme.colorScheme.secondary;
            float elevation = this.elevation
                              ?? floatingActionButtonTheme.elevation
                              ?? _defaultElevation;
            Color focusColor = this.focusColor
                               ?? floatingActionButtonTheme.focusColor
                               ?? theme.focusColor;
            Color hoverColor = this.hoverColor
                               ?? floatingActionButtonTheme.hoverColor
                               ?? theme.hoverColor;
            Color splashColor = this.splashColor
                                ?? floatingActionButtonTheme.splashColor
                                ?? theme.splashColor;
            float disabledElevation = this.disabledElevation
                                      ?? floatingActionButtonTheme.disabledElevation
                                      ?? elevation;
            float focusElevation = this.focusElevation
                                   ?? floatingActionButtonTheme.focusElevation
                                   ?? _defaultFocusElevation;
            float hoverElevation = this.hoverElevation
                                   ?? floatingActionButtonTheme.hoverElevation
                                   ?? _defaultHoverElevation;
            float highlightElevation = this.highlightElevation
                                       ?? floatingActionButtonTheme.highlightElevation
                                       ?? _defaultHighlightElevation;
            MaterialTapTargetSize materialTapTargetSize = this.materialTapTargetSize
                                                          ?? theme.materialTapTargetSize;
            TextStyle textStyle = theme.textTheme.button.copyWith(
                color: foregroundColor,
                letterSpacing: 1.2f
            );
            ShapeBorder shape = this.shape
                                ?? floatingActionButtonTheme.shape
                                ?? (isExtended ? _defaultExtendedShape : _defaultShape);

            Widget result = new RawMaterialButton(
                onPressed: onPressed,
                elevation: elevation,
                focusElevation: focusElevation,
                hoverElevation: hoverElevation,
                highlightElevation: highlightElevation,
                disabledElevation: disabledElevation,
                constraints: _sizeConstraints,
                materialTapTargetSize: materialTapTargetSize,
                fillColor: backgroundColor,
                focusColor: focusColor,
                hoverColor: hoverColor,
                splashColor: splashColor,
                textStyle: textStyle,
                shape: shape,
                clipBehavior: clipBehavior,
                focusNode: focusNode,
                autofocus: autofocus,
                child: child
            );

            if (tooltip != null) {
                result = new Tooltip(
                    message: tooltip,
                    child: result
                );
            }

            if (heroTag != null) {
                result = new Hero(
                    tag: heroTag,
                    child: result
                );
            }

            return result;
        }

        public override void debugFillProperties(DiagnosticPropertiesBuilder properties) {
            base.debugFillProperties(properties);
            properties.add(new ObjectFlagProperty<VoidCallback>("onPressed", onPressed, ifNull: "disabled"));
            properties.add(new StringProperty("tooltip", tooltip, defaultValue: null));
            properties.add(new ColorProperty("foregroundColor", foregroundColor, defaultValue: null));
            properties.add(new ColorProperty("backgroundColor", backgroundColor, defaultValue: null));
            properties.add(new ColorProperty("focusColor", focusColor, defaultValue: null));
            properties.add(new ColorProperty("hoverColor", hoverColor, defaultValue: null));
            properties.add(new ColorProperty("splashColor", splashColor, defaultValue: null));
            properties.add(new ObjectFlagProperty<object>("heroTag", heroTag, ifPresent: "hero"));
            properties.add(new FloatProperty("elevation", elevation, defaultValue: null));
            properties.add(new FloatProperty("focusElevation", focusElevation, defaultValue: null));
            properties.add(new FloatProperty("hoverElevation", hoverElevation, defaultValue: null));
            properties.add(new FloatProperty("highlightElevation", highlightElevation, defaultValue: null));
            properties.add(new FloatProperty("disabledElevation", disabledElevation, defaultValue: null));
            properties.add(new DiagnosticsProperty<ShapeBorder>("shape", shape, defaultValue: null));
            properties.add(new DiagnosticsProperty<FocusNode>("focusNode", focusNode, defaultValue: null));
            properties.add(new FlagProperty("isExtended", value: isExtended, ifTrue: "extended"));
            properties.add(new DiagnosticsProperty<MaterialTapTargetSize?>("materialTapTargetSize",
                materialTapTargetSize, defaultValue: null));
        }
    }

    class _ChildOverflowBox : SingleChildRenderObjectWidget {
        public _ChildOverflowBox(
            Key key = null,
            Widget child = null) : base(key: key, child: child) {
        }

        public override RenderObject createRenderObject(BuildContext context) {
            return new _RenderChildOverflowBox();
        }

        public override void updateRenderObject(BuildContext context, RenderObject renderObject) {
            if (renderObject is _RenderChildOverflowBox renderChildOverflowBox) {
                renderChildOverflowBox.textDirection = Directionality.of(context);
            }
        }
    }


    class _RenderChildOverflowBox : RenderAligningShiftedBox {
        public _RenderChildOverflowBox(
            RenderBox child = null) : base(child: child, alignment: Alignment.center) {
        }

        protected internal override float computeMinIntrinsicWidth(float height) {
            return 0.0f;
        }

        protected internal override float computeMinIntrinsicHeight(float width) {
            return 0.0f;
        }

        protected override void performLayout() {
            BoxConstraints constraints = this.constraints;
            if (child != null) {
                child.layout(new BoxConstraints(), parentUsesSize: true);
                size = new Size(
                    Mathf.Max(constraints.minWidth, Mathf.Min(constraints.maxWidth, child.size.width)),
                    Mathf.Max(constraints.minHeight, Mathf.Min(constraints.maxHeight, child.size.height))
                );
                alignChild();
            }
            else {
                size = constraints.biggest;
            }
        }
    }
}