using System.Collections.Generic;
using System.Linq;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.rendering;
using Unity.UIWidgets.ui;
using Unity.UIWidgets.widgets;
using UnityEngine;
using Color = Unity.UIWidgets.ui.Color;
using Rect = Unity.UIWidgets.ui.Rect;
using TextStyle = Unity.UIWidgets.painting.TextStyle;

namespace Unity.UIWidgets.material {
    public delegate void ToggleButtonOnPressed(int index);

    public class ToggleButtons : StatelessWidget {
        public ToggleButtons(
            Key key = null,
            List<Widget> children = null,
            List<bool> isSelected = null,
            ToggleButtonOnPressed onPressed = null,
            TextStyle textStyle = null,
            BoxConstraints constraints = null,
            Color color = null,
            Color selectedColor = null,
            Color disabledColor = null,
            Color fillColor = null,
            Color focusColor = null,
            Color highlightColor = null,
            Color hoverColor = null,
            Color splashColor = null,
            List<FocusNode> focusNodes = null,
            bool renderBorder = false,
            Color borderColor = null,
            Color selectedBorderColor = null,
            Color disabledBorderColor = null,
            BorderRadius borderRadius = null,
            float? borderWidth = null
        ) : base(key: key) {
            D.assert(children != null);
            D.assert(isSelected != null);
            D.assert(children.Count == isSelected.Count);

            this.children = children;
            this.isSelected = isSelected;
            this.onPressed = onPressed;
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
            this.focusNodes = focusNodes;
            this.renderBorder = renderBorder;
            this.borderColor = borderColor;
            this.selectedBorderColor = selectedBorderColor;
            this.disabledBorderColor = disabledBorderColor;
            this.borderRadius = borderRadius;
            this.borderWidth = borderWidth;
        }
        
        const float _defaultBorderWidth = 1.0f;

        public readonly List<Widget> children;

        public readonly List<bool> isSelected;

        public readonly ToggleButtonOnPressed onPressed;

        public readonly TextStyle textStyle;

        public readonly BoxConstraints constraints;

        public readonly Color color;

        public readonly Color selectedColor;

        public readonly Color disabledColor;

        public readonly Color fillColor;

        public readonly Color focusColor;

        public readonly Color highlightColor;

        public readonly Color splashColor;

        public readonly Color hoverColor;

        public readonly List<FocusNode> focusNodes;

        public readonly bool renderBorder;

        public readonly Color borderColor;

        public readonly Color selectedBorderColor;

        public readonly Color disabledBorderColor;

        public readonly float? borderWidth;

        public readonly BorderRadius borderRadius;

        bool _isFirstIndex(int index, int length, TextDirection textDirection) {
            return index == 0 && textDirection == TextDirection.ltr
                   || index == length - 1 && textDirection == TextDirection.rtl;
        }

        bool _isLastIndex(int index, int length, TextDirection textDirection) {
            return index == length - 1 && textDirection == TextDirection.ltr
                   || index == 0 && textDirection == TextDirection.rtl;
        }

        BorderRadius _getEdgeBorderRadius(
            int index,
            int length,
            TextDirection textDirection,
            ToggleButtonsThemeData toggleButtonsTheme
        ) {
            BorderRadius resultingBorderRadius = borderRadius
                                                 ?? toggleButtonsTheme.borderRadius
                                                 ?? BorderRadius.zero;

            if (_isFirstIndex(index, length, textDirection)) {
                return BorderRadius.only(
                    topLeft: resultingBorderRadius.topLeft,
                    bottomLeft: resultingBorderRadius.bottomLeft
                );
            }
            else if (_isLastIndex(index, length, textDirection)) {
                return BorderRadius.only(
                    topRight: resultingBorderRadius.topRight,
                    bottomRight: resultingBorderRadius.bottomRight
                );
            }

            return BorderRadius.zero;
        }

        BorderRadius _getClipBorderRadius(
            int index,
            int length,
            TextDirection textDirection,
            ToggleButtonsThemeData toggleButtonsTheme
        ) {
            BorderRadius resultingBorderRadius = borderRadius
                                                 ?? toggleButtonsTheme.borderRadius
                                                 ?? BorderRadius.zero;
            float resultingBorderWidth = borderWidth
                                         ?? toggleButtonsTheme.borderWidth
                                         ?? _defaultBorderWidth;

            if (_isFirstIndex(index, length, textDirection)) {
                return BorderRadius.only(
                    topLeft: resultingBorderRadius.topLeft - Radius.circular(resultingBorderWidth / 2.0f),
                    bottomLeft: resultingBorderRadius.bottomLeft - Radius.circular(resultingBorderWidth / 2.0f)
                );
            }
            else if (_isLastIndex(index, length, textDirection)) {
                return BorderRadius.only(
                    topRight: resultingBorderRadius.topRight - Radius.circular(resultingBorderWidth / 2.0f),
                    bottomRight: resultingBorderRadius.bottomRight - Radius.circular(resultingBorderWidth / 2.0f)
                );
            }

            return BorderRadius.zero;
        }

        BorderSide _getLeadingBorderSide(
            int index,
            ThemeData theme,
            ToggleButtonsThemeData toggleButtonsTheme
        ) {
            if (!renderBorder) {
                return BorderSide.none;
            }

            float resultingBorderWidth = borderWidth
                                         ?? toggleButtonsTheme.borderWidth
                                         ?? _defaultBorderWidth;
            if (onPressed != null && (isSelected[index] || (index != 0 && isSelected[index - 1]))) {
                return new BorderSide(
                    color: selectedBorderColor
                           ?? toggleButtonsTheme.selectedBorderColor
                           ?? theme.colorScheme.onSurface.withOpacity(0.12f),
                    width: resultingBorderWidth
                );
            }
            else if (onPressed != null && !isSelected[index]) {
                return new BorderSide(
                    color: borderColor
                           ?? toggleButtonsTheme.borderColor
                           ?? theme.colorScheme.onSurface.withOpacity(0.12f),
                    width: resultingBorderWidth
                );
            }
            else {
                return new BorderSide(
                    color: disabledBorderColor
                           ?? toggleButtonsTheme.disabledBorderColor
                           ?? theme.colorScheme.onSurface.withOpacity(0.12f),
                    width: resultingBorderWidth
                );
            }
        }

        BorderSide _getHorizontalBorderSide(
            int index,
            ThemeData theme,
            ToggleButtonsThemeData toggleButtonsTheme
        ) {
            if (!renderBorder) {
                return BorderSide.none;
            }

            float resultingBorderWidth = borderWidth
                                         ?? toggleButtonsTheme.borderWidth
                                         ?? _defaultBorderWidth;
            if (onPressed != null && isSelected[index]) {
                return new BorderSide(
                    color: selectedBorderColor
                           ?? toggleButtonsTheme.selectedBorderColor
                           ?? theme.colorScheme.onSurface.withOpacity(0.12f),
                    width: resultingBorderWidth
                );
            }
            else if (onPressed != null && !isSelected[index]) {
                return new BorderSide(
                    color: borderColor
                           ?? toggleButtonsTheme.borderColor
                           ?? theme.colorScheme.onSurface.withOpacity(0.12f),
                    width: resultingBorderWidth
                );
            }
            else {
                return new BorderSide(
                    color: disabledBorderColor
                           ?? toggleButtonsTheme.disabledBorderColor
                           ?? theme.colorScheme.onSurface.withOpacity(0.12f),
                    width: resultingBorderWidth
                );
            }
        }

        BorderSide _getTrailingBorderSide(
            int index,
            ThemeData theme,
            ToggleButtonsThemeData toggleButtonsTheme
        ) {
            if (!renderBorder) {
                return BorderSide.none;
            }

            if (index != children.Count - 1) {
                return null;
            }

            float resultingBorderWidth = borderWidth
                                         ?? toggleButtonsTheme.borderWidth
                                         ?? _defaultBorderWidth;
            if (onPressed != null && (isSelected[index])) {
                return new BorderSide(
                    color: selectedBorderColor
                           ?? toggleButtonsTheme.selectedBorderColor
                           ?? theme.colorScheme.onSurface.withOpacity(0.12f),
                    width: resultingBorderWidth
                );
            }
            else if (onPressed != null && !isSelected[index]) {
                return new BorderSide(
                    color: borderColor
                           ?? toggleButtonsTheme.borderColor
                           ?? theme.colorScheme.onSurface.withOpacity(0.12f),
                    width: resultingBorderWidth
                );
            }
            else {
                return new BorderSide(
                    color: disabledBorderColor
                           ?? toggleButtonsTheme.disabledBorderColor
                           ?? theme.colorScheme.onSurface.withOpacity(0.12f),
                    width: resultingBorderWidth
                );
            }
        }

        public override Widget build(BuildContext context) {
            D.assert(
                focusNodes == null || !focusNodes.Any((FocusNode val) => val == null),
                () => "All elements of focusNodes must be non-null.\n" +
                      "The current list of focus node values is as follows:\n" +
                      $"{focusNodes}"
            );
            D.assert(
                () => {
                    if (focusNodes != null) {
                        return focusNodes.Count == children.Count;
                    }

                    return true;
                },
                () => "focusNodes.length must match children.length.\n" +
                      $"There are {focusNodes.Count} focus nodes, while " +
                      $"there are {children.Count} children."
            );

            ThemeData theme = Theme.of(context);
            ToggleButtonsThemeData toggleButtonsTheme = ToggleButtonsTheme.of(context);
            TextDirection textDirection = Directionality.of(context);

            var childrenList = new List<Widget>(children.Count);
            for (var i = 0; i < children.Count; i++) {
                var index = i;
                BorderRadius edgeBorderRadius =
                    _getEdgeBorderRadius(index, children.Count, textDirection, toggleButtonsTheme);
                BorderRadius clipBorderRadius =
                    _getClipBorderRadius(index, children.Count, textDirection, toggleButtonsTheme);

                BorderSide leadingBorderSide = _getLeadingBorderSide(index, theme, toggleButtonsTheme);
                BorderSide horizontalBorderSide = _getHorizontalBorderSide(index, theme, toggleButtonsTheme);
                BorderSide trailingBorderSide = _getTrailingBorderSide(index, theme, toggleButtonsTheme);

                childrenList.Add(new _ToggleButton(
                    selected: isSelected[index],
                    textStyle: textStyle,
                    constraints: constraints,
                    color: color,
                    selectedColor: selectedColor,
                    disabledColor: disabledColor,
                    fillColor: fillColor ?? toggleButtonsTheme.fillColor,
                    focusColor: focusColor ?? toggleButtonsTheme.focusColor,
                    highlightColor: highlightColor ?? toggleButtonsTheme.highlightColor,
                    hoverColor: hoverColor ?? toggleButtonsTheme.hoverColor,
                    splashColor: splashColor ?? toggleButtonsTheme.splashColor,
                    focusNode: focusNodes != null ? focusNodes[index] : null,
                    onPressed: onPressed != null
                        ? () => { onPressed(index); }
                        : (VoidCallback) null,
                    leadingBorderSide: leadingBorderSide,
                    horizontalBorderSide: horizontalBorderSide,
                    trailingBorderSide: trailingBorderSide,
                    borderRadius: edgeBorderRadius,
                    clipRadius: clipBorderRadius,
                    isFirstButton: index == 0,
                    isLastButton: index == children.Count - 1,
                    child: children[index]
                ));
            }

            return new IntrinsicHeight(
                child: new Row(
                    crossAxisAlignment: CrossAxisAlignment.stretch,
                    mainAxisSize: MainAxisSize.min,
                    children: childrenList)
            );
        }

        public override void debugFillProperties(DiagnosticPropertiesBuilder properties) {
            base.debugFillProperties(properties);
            properties.add(new FlagProperty("disabled",
                value: onPressed == null,
                ifTrue: "Buttons are disabled",
                ifFalse: "Buttons are enabled"
            ));
            textStyle?.debugFillProperties(properties, prefix: "textStyle.");
            properties.add(new ColorProperty("color", color, defaultValue: null));
            properties.add(new ColorProperty("selectedColor", selectedColor, defaultValue: null));
            properties.add(new ColorProperty("disabledColor", disabledColor, defaultValue: null));
            properties.add(new ColorProperty("fillColor", fillColor, defaultValue: null));
            properties.add(new ColorProperty("focusColor", focusColor, defaultValue: null));
            properties.add(new ColorProperty("highlightColor", highlightColor, defaultValue: null));
            properties.add(new ColorProperty("hoverColor", hoverColor, defaultValue: null));
            properties.add(new ColorProperty("splashColor", splashColor, defaultValue: null));
            properties.add(new ColorProperty("borderColor", borderColor, defaultValue: null));
            properties.add(new ColorProperty("selectedBorderColor", selectedBorderColor, defaultValue: null));
            properties.add(new ColorProperty("disabledBorderColor", disabledBorderColor, defaultValue: null));
            properties.add(new DiagnosticsProperty<BorderRadius>("borderRadius", borderRadius, defaultValue: null));
            properties.add(new FloatProperty("borderWidth", borderWidth, defaultValue: null));
        }
    }


    class _ToggleButton : StatelessWidget {
        public _ToggleButton(
            Key key = null,
            bool selected = false,
            TextStyle textStyle = null,
            BoxConstraints constraints = null,
            Color color = null,
            Color selectedColor = null,
            Color disabledColor = null,
            Color fillColor = null,
            Color focusColor = null,
            Color highlightColor = null,
            Color hoverColor = null,
            Color splashColor = null,
            FocusNode focusNode = null,
            VoidCallback onPressed = null,
            BorderSide leadingBorderSide = null,
            BorderSide horizontalBorderSide = null,
            BorderSide trailingBorderSide = null,
            BorderRadius borderRadius = null,
            BorderRadius clipRadius = null,
            bool? isFirstButton = null,
            bool? isLastButton = null,
            Widget child = null
        ) : base(key: key) {
            D.assert(isFirstButton != null);
            D.assert(isLastButton != null);

            this.selected = selected;
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
            this.focusNode = focusNode;
            this.onPressed = onPressed;
            this.leadingBorderSide = leadingBorderSide;
            this.horizontalBorderSide = horizontalBorderSide;
            this.trailingBorderSide = trailingBorderSide;
            this.borderRadius = borderRadius;
            this.clipRadius = clipRadius;
            this.isFirstButton = isFirstButton.Value;
            this.isLastButton = isLastButton.Value;
            this.child = child;
        }

        public readonly bool selected;

        public readonly TextStyle textStyle;

        public readonly BoxConstraints constraints;

        public readonly Color color;

        public readonly Color selectedColor;

        public readonly Color disabledColor;

        public readonly Color fillColor;

        public readonly Color focusColor;

        public readonly Color hoverColor;

        public readonly Color highlightColor;

        public readonly Color splashColor;

        public readonly FocusNode focusNode;

        public readonly VoidCallback onPressed;

        public readonly BorderSide leadingBorderSide;

        public readonly BorderSide horizontalBorderSide;

        public readonly BorderSide trailingBorderSide;

        public readonly BorderRadius borderRadius;

        public readonly BorderRadius clipRadius;

        public readonly bool isFirstButton;

        public readonly bool isLastButton;

        public readonly Widget child;

        public override Widget build(BuildContext context) {
            D.assert(material_.debugCheckHasMaterial(context));
            Color currentColor = null;
            Color currentFillColor = null;
            Color currentFocusColor = null;
            Color currentHoverColor = null;
            Color currentSplashColor = null;
            ThemeData theme = Theme.of(context);
            ToggleButtonsThemeData toggleButtonsTheme = ToggleButtonsTheme.of(context);

            if (onPressed != null && selected) {
                currentColor = selectedColor
                               ?? toggleButtonsTheme.selectedColor
                               ?? theme.colorScheme.primary;
                currentFillColor = fillColor
                                   ?? theme.colorScheme.primary.withOpacity(0.12f);
                currentFocusColor = focusColor
                                    ?? toggleButtonsTheme.focusColor
                                    ?? theme.colorScheme.primary.withOpacity(0.12f);
                currentHoverColor = hoverColor
                                    ?? toggleButtonsTheme.hoverColor
                                    ?? theme.colorScheme.primary.withOpacity(0.04f);
                currentSplashColor = splashColor
                                     ?? toggleButtonsTheme.splashColor
                                     ?? theme.colorScheme.primary.withOpacity(0.16f);
            }
            else if (onPressed != null && !selected) {
                currentColor = color
                               ?? toggleButtonsTheme.color
                               ?? theme.colorScheme.onSurface.withOpacity(0.87f);
                currentFillColor = theme.colorScheme.surface.withOpacity(0.0f);
                currentFocusColor = focusColor
                                    ?? toggleButtonsTheme.focusColor
                                    ?? theme.colorScheme.onSurface.withOpacity(0.12f);
                currentHoverColor = hoverColor
                                    ?? toggleButtonsTheme.hoverColor
                                    ?? theme.colorScheme.onSurface.withOpacity(0.04f);
                currentSplashColor = splashColor
                                     ?? toggleButtonsTheme.splashColor
                                     ?? theme.colorScheme.onSurface.withOpacity(0.16f);
            }
            else {
                currentColor = disabledColor
                               ?? toggleButtonsTheme.disabledColor
                               ?? theme.colorScheme.onSurface.withOpacity(0.38f);
                currentFillColor = theme.colorScheme.surface.withOpacity(0.0f);
            }

            TextStyle currentTextStyle = textStyle ?? toggleButtonsTheme.textStyle ?? theme.textTheme.bodyText2;
            BoxConstraints currentConstraints = constraints ?? toggleButtonsTheme.constraints ??
                                                new BoxConstraints(minWidth: material_.kMinInteractiveDimension,
                                                    minHeight: material_.kMinInteractiveDimension);

            Widget result = new ClipRRect(
                borderRadius: clipRadius,
                child: new RawMaterialButton(
                    textStyle: currentTextStyle.copyWith(
                        color: currentColor
                    ),
                    constraints: currentConstraints,
                    elevation: 0.0f,
                    highlightElevation: 0.0f,
                    fillColor: currentFillColor,
                    focusColor: currentFocusColor,
                    highlightColor: highlightColor
                                    ?? theme.colorScheme.surface.withOpacity(0.0f),
                    hoverColor: currentHoverColor,
                    splashColor: currentSplashColor,
                    focusNode: focusNode,
                    materialTapTargetSize: MaterialTapTargetSize.shrinkWrap,
                    onPressed: onPressed,
                    child: child
                )
            );

            return new _SelectToggleButton(
                key: key,
                leadingBorderSide: leadingBorderSide,
                horizontalBorderSide: horizontalBorderSide,
                trailingBorderSide: trailingBorderSide,
                borderRadius: borderRadius,
                isFirstButton: isFirstButton,
                isLastButton: isLastButton,
                child: result
            );
        }

        public override void debugFillProperties(DiagnosticPropertiesBuilder properties) {
            base.debugFillProperties(properties);
            properties.add(new FlagProperty("selected",
                value: selected,
                ifTrue: "Button is selected",
                ifFalse: "Button is unselected"
            ));
        }
    }


    class _SelectToggleButton : SingleChildRenderObjectWidget {
        public _SelectToggleButton(
            Key key = null,
            Widget child = null,
            BorderSide leadingBorderSide = null,
            BorderSide horizontalBorderSide = null,
            BorderSide trailingBorderSide = null,
            BorderRadius borderRadius = null,
            bool? isFirstButton = null,
            bool? isLastButton = null
        ) : base(key: key, child: child) {
            D.assert(isFirstButton != null);
            D.assert(isLastButton != null);

            this.leadingBorderSide = leadingBorderSide;
            this.horizontalBorderSide = horizontalBorderSide;
            this.trailingBorderSide = trailingBorderSide;
            this.borderRadius = borderRadius;
            this.isFirstButton = isFirstButton.Value;
            this.isLastButton = isLastButton.Value;
        }

        public readonly BorderSide leadingBorderSide;

        public readonly BorderSide horizontalBorderSide;

        public readonly BorderSide trailingBorderSide;

        public readonly BorderRadius borderRadius;

        public readonly bool isFirstButton;

        public readonly bool isLastButton;

        public override RenderObject createRenderObject(BuildContext context) {
            return new _SelectToggleButtonRenderObject(
                leadingBorderSide,
                horizontalBorderSide,
                trailingBorderSide,
                borderRadius,
                isFirstButton,
                isLastButton,
                Directionality.of(context)
            );
        }

        public override void updateRenderObject(BuildContext context, RenderObject renderObject) {
            var _renderObject = (_SelectToggleButtonRenderObject) renderObject;
            _renderObject.leadingBorderSide = leadingBorderSide;
            _renderObject.horizontalBorderSide = horizontalBorderSide;
            _renderObject.trailingBorderSide = trailingBorderSide;
            _renderObject.borderRadius = borderRadius;
            _renderObject.isFirstButton = isFirstButton;
            _renderObject.isLastButton = isLastButton;
            _renderObject.textDirection = Directionality.of(context);
        }
    }

    class _SelectToggleButtonRenderObject : RenderShiftedBox {
        public _SelectToggleButtonRenderObject(
            BorderSide leadingBorderSide,
            BorderSide horizontalBorderSide,
            BorderSide trailingBorderSide,
            BorderRadius borderRadius,
            bool isFirstButton,
            bool isLastButton,
            TextDirection textDirection,
            RenderBox child = null
        ) : base(child: child) {
            _leadingBorderSide = leadingBorderSide;
            _horizontalBorderSide = horizontalBorderSide;
            _trailingBorderSide = trailingBorderSide;
            _borderRadius = borderRadius;
            _isFirstButton = isFirstButton;
            _isLastButton = isLastButton;
            _textDirection = textDirection;
        }

        public BorderSide leadingBorderSide {
            get { return _leadingBorderSide; }
            set {
                if (_leadingBorderSide == value) {
                    return;
                }

                _leadingBorderSide = value;
                markNeedsLayout();
            }
        }

        BorderSide _leadingBorderSide;

        public BorderSide horizontalBorderSide {
            get { return _horizontalBorderSide; }
            set {
                if (_horizontalBorderSide == value) {
                    return;
                }

                _horizontalBorderSide = value;
                markNeedsLayout();
            }
        }

        BorderSide _horizontalBorderSide;

        public BorderSide trailingBorderSide {
            get { return _trailingBorderSide; }
            set {
                if (_trailingBorderSide == value) {
                    return;
                }

                _trailingBorderSide = value;
                markNeedsLayout();
            }
        }

        BorderSide _trailingBorderSide;

        public BorderRadius borderRadius {
            get { return _borderRadius; }
            set {
                if (_borderRadius == value) {
                    return;
                }

                _borderRadius = value;
                markNeedsLayout();
            }
        }

        BorderRadius _borderRadius;

        public bool isFirstButton {
            get { return _isFirstButton; }
            set {
                if (_isFirstButton == value) {
                    return;
                }

                _isFirstButton = value;
                markNeedsLayout();
            }
        }

        bool _isFirstButton;

        public bool isLastButton {
            get { return _isLastButton; }
            set {
                if (_isLastButton == value) {
                    return;
                }

                _isLastButton = value;
                markNeedsLayout();
            }
        }

        bool _isLastButton;

        public TextDirection textDirection {
            get { return _textDirection; }
            set {
                if (_textDirection == value) {
                    return;
                }

                _textDirection = value;
                markNeedsLayout();
            }
        }

        TextDirection _textDirection;

        static float _maxHeight(RenderBox box, float width) {
            return box == null ? 0.0f : box.getMaxIntrinsicHeight(width);
        }

        static float _minWidth(RenderBox box, float height) {
            return box == null ? 0.0f : box.getMinIntrinsicWidth(height);
        }

        static float _maxWidth(RenderBox box, float height) {
            return box == null ? 0.0f : box.getMaxIntrinsicWidth(height);
        }

        public override float? computeDistanceToActualBaseline(TextBaseline baseline) {
            return child.computeDistanceToActualBaseline(baseline).Value +
                   horizontalBorderSide.width;
        }

        protected internal override float computeMaxIntrinsicHeight(float width) {
            return horizontalBorderSide.width +
                   _maxHeight(child, width) +
                   horizontalBorderSide.width;
        }

        protected internal override float computeMinIntrinsicHeight(float width) {
            return computeMaxIntrinsicHeight(width);
        }

        protected internal override float computeMaxIntrinsicWidth(float height) {
            float trailingWidth = trailingBorderSide == null ? 0.0f : trailingBorderSide.width;
            return leadingBorderSide.width +
                   _maxWidth(child, height) +
                   trailingWidth;
        }

        protected internal override float computeMinIntrinsicWidth(float height) {
            float trailingWidth = trailingBorderSide == null ? 0.0f : trailingBorderSide.width;
            return leadingBorderSide.width +
                   _minWidth(child, height) +
                   trailingWidth;
        }

        protected override void performLayout() {
            if (child == null) {
                size = constraints.constrain(new Size(
                    leadingBorderSide.width + trailingBorderSide.width,
                    horizontalBorderSide.width * 2.0f
                ));
                return;
            }

            float trailingBorderOffset = isLastButton ? trailingBorderSide.width : 0.0f;
            float leftConstraint;
            float rightConstraint;

            BoxConstraints innerConstraints;
            BoxParentData childParentData;

            switch (textDirection) {
                case TextDirection.ltr: {
                    rightConstraint = trailingBorderOffset;
                    leftConstraint = leadingBorderSide.width;

                    innerConstraints = constraints.deflate(
                        EdgeInsets.only(
                            left: leftConstraint,
                            top: horizontalBorderSide.width,
                            right: rightConstraint,
                            bottom: horizontalBorderSide.width
                        )
                    );

                    child.layout(innerConstraints, parentUsesSize: true);

                    childParentData = child.parentData as BoxParentData;
                    childParentData.offset = new Offset(leadingBorderSide.width, leadingBorderSide.width);

                    size = constraints.constrain(new Size(
                        leftConstraint + child.size.width + rightConstraint,
                        horizontalBorderSide.width * 2.0f + child.size.height
                    ));
                    break;
                }

                case TextDirection.rtl: {
                    rightConstraint = leadingBorderSide.width;
                    leftConstraint = trailingBorderOffset;

                    innerConstraints = constraints.deflate(
                        EdgeInsets.only(
                            left: leftConstraint,
                            top: horizontalBorderSide.width,
                            right: rightConstraint,
                            bottom: horizontalBorderSide.width
                        )
                    );

                    child.layout(innerConstraints, parentUsesSize: true);
                    childParentData = child.parentData as BoxParentData;

                    if (isLastButton) {
                        childParentData.offset = new Offset(trailingBorderOffset, trailingBorderOffset);
                    }
                    else {
                        childParentData.offset = new Offset(0, horizontalBorderSide.width);
                    }

                    size = constraints.constrain(new Size(
                        leftConstraint + child.size.width + rightConstraint,
                        horizontalBorderSide.width * 2.0f + child.size.height
                    ));
                    break;
                }
            }
        }

        public override void paint(PaintingContext context, Offset offset) {
            base.paint(context, offset);
            Offset bottomRight = size.bottomRight(offset);
            Rect outer = Rect.fromLTRB(offset.dx, offset.dy, bottomRight.dx, bottomRight.dy);
            Rect center = outer.deflate(horizontalBorderSide.width / 2.0f);
            const float sweepAngle = Mathf.PI / 2.0f;

            RRect rrect = RRect.fromRectAndCorners(
                center,
                topLeft: borderRadius.topLeft,
                topRight: borderRadius.topRight,
                bottomLeft: borderRadius.bottomLeft,
                bottomRight: borderRadius.bottomRight
            ).scaleRadii();

            Rect tlCorner = Rect.fromLTWH(
                rrect.left,
                rrect.top,
                rrect.tlRadiusX * 2.0f,
                rrect.tlRadiusY * 2.0f
            );
            Rect blCorner = Rect.fromLTWH(
                rrect.left,
                rrect.bottom - (rrect.blRadiusY * 2.0f),
                rrect.blRadiusX * 2.0f,
                rrect.blRadiusY * 2.0f
            );
            Rect trCorner = Rect.fromLTWH(
                rrect.right - (rrect.trRadiusX * 2.0f),
                rrect.top,
                rrect.trRadiusX * 2.0f,
                rrect.trRadiusY * 2.0f
            );
            Rect brCorner = Rect.fromLTWH(
                rrect.right - (rrect.brRadiusX * 2.0f),
                rrect.bottom - (rrect.brRadiusY * 2.0f),
                rrect.brRadiusX * 2.0f,
                rrect.brRadiusY * 2.0f
            );

            Paint leadingPaint = leadingBorderSide.toPaint();
            switch (textDirection) {
                case TextDirection.ltr: {
                    if (isLastButton) {
                        Path leftPath = new Path();
                        leftPath.moveTo(rrect.left, rrect.bottom + leadingBorderSide.width / 2);
                        leftPath.lineTo(rrect.left, rrect.top - leadingBorderSide.width / 2);
                        context.canvas.drawPath(leftPath, leadingPaint);

                        Paint endingPaint = trailingBorderSide.toPaint();
                        Path endingPath = new Path();
                        endingPath.moveTo(rrect.left + horizontalBorderSide.width / 2.0f, rrect.top);
                        endingPath.lineTo(rrect.right - rrect.trRadiusX, rrect.top);
                        endingPath.addArc(trCorner, Mathf.PI * 3.0f / 2.0f, sweepAngle);
                        endingPath.lineTo(rrect.right, rrect.bottom - rrect.brRadiusY);
                        endingPath.addArc(brCorner, 0, sweepAngle);
                        endingPath.lineTo(rrect.left + horizontalBorderSide.width / 2.0f, rrect.bottom);
                        context.canvas.drawPath(endingPath, endingPaint);
                    }
                    else if (isFirstButton) {
                        Path leadingPath = new Path();
                        leadingPath.moveTo(outer.right, rrect.bottom);
                        leadingPath.lineTo(rrect.left + rrect.blRadiusX, rrect.bottom);
                        leadingPath.addArc(blCorner, Mathf.PI / 2.0f, sweepAngle);
                        leadingPath.lineTo(rrect.left, rrect.top + rrect.tlRadiusY);
                        leadingPath.addArc(tlCorner, Mathf.PI, sweepAngle);
                        leadingPath.lineTo(outer.right, rrect.top);
                        context.canvas.drawPath(leadingPath, leadingPaint);
                    }
                    else {
                        Path leadingPath = new Path();
                        leadingPath.moveTo(rrect.left, rrect.bottom + leadingBorderSide.width / 2);
                        leadingPath.lineTo(rrect.left, rrect.top - leadingBorderSide.width / 2);
                        context.canvas.drawPath(leadingPath, leadingPaint);

                        Paint horizontalPaint = horizontalBorderSide.toPaint();
                        Path horizontalPaths = new Path();
                        horizontalPaths.moveTo(rrect.left + horizontalBorderSide.width / 2.0f, rrect.top);
                        horizontalPaths.lineTo(outer.right - rrect.trRadiusX, rrect.top);
                        horizontalPaths.moveTo(rrect.left + horizontalBorderSide.width / 2.0f + rrect.tlRadiusX,
                            rrect.bottom);
                        horizontalPaths.lineTo(outer.right - rrect.trRadiusX, rrect.bottom);
                        context.canvas.drawPath(horizontalPaths, horizontalPaint);
                    }

                    break;
                }

                case TextDirection.rtl: {
                    if (isLastButton) {
                        Path leadingPath = new Path();
                        leadingPath.moveTo(rrect.right, rrect.bottom + leadingBorderSide.width / 2);
                        leadingPath.lineTo(rrect.right, rrect.top - leadingBorderSide.width / 2);
                        context.canvas.drawPath(leadingPath, leadingPaint);

                        Paint endingPaint = trailingBorderSide.toPaint();
                        Path endingPath = new Path();
                        endingPath.moveTo(rrect.right - horizontalBorderSide.width / 2.0f, rrect.top);
                        endingPath.lineTo(rrect.left + rrect.tlRadiusX, rrect.top);
                        endingPath.addArc(tlCorner, Mathf.PI * 3.0f / 2.0f, -sweepAngle);
                        endingPath.lineTo(rrect.left, rrect.bottom - rrect.blRadiusY);
                        endingPath.addArc(blCorner, Mathf.PI, -sweepAngle);
                        endingPath.lineTo(rrect.right - horizontalBorderSide.width / 2.0f, rrect.bottom);
                        context.canvas.drawPath(endingPath, endingPaint);
                    }
                    else if (isFirstButton) {
                        Path leadingPath = new Path();
                        leadingPath.moveTo(outer.left, rrect.bottom);
                        leadingPath.lineTo(rrect.right - rrect.brRadiusX, rrect.bottom);
                        leadingPath.addArc(brCorner, Mathf.PI / 2.0f, -sweepAngle);
                        leadingPath.lineTo(rrect.right, rrect.top + rrect.trRadiusY);
                        leadingPath.addArc(trCorner, 0, -sweepAngle);
                        leadingPath.lineTo(outer.left, rrect.top);
                        context.canvas.drawPath(leadingPath, leadingPaint);
                    }
                    else {
                        Path leadingPath = new Path();
                        leadingPath.moveTo(rrect.right, rrect.bottom + leadingBorderSide.width / 2);
                        leadingPath.lineTo(rrect.right, rrect.top - leadingBorderSide.width / 2);
                        context.canvas.drawPath(leadingPath, leadingPaint);

                        Paint horizontalPaint = horizontalBorderSide.toPaint();
                        Path horizontalPaths = new Path();
                        horizontalPaths.moveTo(rrect.right - horizontalBorderSide.width / 2.0f, rrect.top);
                        horizontalPaths.lineTo(outer.left - rrect.tlRadiusX, rrect.top);
                        horizontalPaths.moveTo(rrect.right - horizontalBorderSide.width / 2.0f + rrect.trRadiusX,
                            rrect.bottom);
                        horizontalPaths.lineTo(outer.left - rrect.tlRadiusX, rrect.bottom);
                        context.canvas.drawPath(horizontalPaths, horizontalPaint);
                    }

                    break;
                }
            }
        }
    }
}