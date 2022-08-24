using System;
using System.Collections.Generic;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.gestures;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.rendering;
using Unity.UIWidgets.ui;
using Unity.UIWidgets.widgets;
using UnityEngine;
using Color = Unity.UIWidgets.ui.Color;
using TextStyle = Unity.UIWidgets.painting.TextStyle;

namespace Unity.UIWidgets.material {
    public class RawMaterialButton : StatefulWidget {
        public RawMaterialButton(
            Key key = null,
            VoidCallback onPressed = null,
            GestureLongPressCallback onLongPress = null,
            ValueChanged<bool> onHighlightChanged = null,
            TextStyle textStyle = null,
            Color fillColor = null,
            Color focusColor = null,
            Color hoverColor = null,
            Color highlightColor = null,
            Color splashColor = null,
            float elevation = 2.0f,
            float focusElevation = 4.0f,
            float hoverElevation = 4.0f,
            float highlightElevation = 8.0f,
            float disabledElevation = 0.0f,
            EdgeInsetsGeometry padding = null,
            VisualDensity visualDensity = null,
            BoxConstraints constraints = null,
            ShapeBorder shape = null,
            TimeSpan? animationDuration = null,
            Clip clipBehavior = Clip.none,
            FocusNode focusNode = null,
            bool autofocus = false,
            MaterialTapTargetSize? materialTapTargetSize = null,
            Widget child = null,
            bool enableFeedback = true) : base(key: key) {
            D.assert(elevation >= 0.0f);
            D.assert(focusElevation >= 0.0f);
            D.assert(focusElevation >= 0.0f);
            D.assert(highlightElevation >= 0.0f);
            D.assert(disabledElevation >= 0.0f);

            MaterialTapTargetSize _materialTapTargetSize = materialTapTargetSize ?? MaterialTapTargetSize.padded;
            shape = shape ?? new RoundedRectangleBorder();
            visualDensity = visualDensity ?? new VisualDensity();
            padding = padding ?? EdgeInsets.zero;
            constraints = constraints ?? new BoxConstraints(minWidth: 88.0f, minHeight: 36.0f);
            TimeSpan _animationDuration = animationDuration ?? material_.kThemeChangeDuration;

            this.onPressed = onPressed;
            this.onLongPress = onLongPress;
            this.onHighlightChanged = onHighlightChanged;
            this.textStyle = textStyle;
            this.fillColor = fillColor;
            this.focusColor = focusColor;
            this.hoverColor = hoverColor;
            this.highlightColor = highlightColor;
            this.splashColor = splashColor;
            this.elevation = elevation;
            this.focusElevation = focusElevation;
            this.hoverElevation = hoverElevation;
            this.highlightElevation = highlightElevation;
            this.disabledElevation = disabledElevation;
            this.padding = padding;
            this.visualDensity = visualDensity;
            this.constraints = constraints;
            this.shape = shape;
            this.animationDuration = _animationDuration;
            this.clipBehavior = clipBehavior;
            this.focusNode = focusNode;
            this.autofocus = autofocus;
            this.enableFeedback = enableFeedback;
            this.materialTapTargetSize = _materialTapTargetSize;
            this.child = child;
        }

        public readonly VoidCallback onPressed;

        public readonly GestureLongPressCallback onLongPress;

        public readonly ValueChanged<bool> onHighlightChanged;

        public readonly TextStyle textStyle;

        public readonly Color fillColor;

        public readonly Color focusColor;

        public readonly Color hoverColor;

        public readonly Color highlightColor;

        public readonly Color splashColor;

        public readonly float elevation;

        public readonly float hoverElevation;

        public readonly float focusElevation;

        public readonly float highlightElevation;

        public readonly float disabledElevation;

        public readonly EdgeInsetsGeometry padding;

        public readonly VisualDensity visualDensity;

        public readonly BoxConstraints constraints;

        public readonly ShapeBorder shape;

        public readonly TimeSpan animationDuration;

        public readonly Widget child;

        public bool enabled {
            get { return onPressed != null || onLongPress != null; }
        }

        public readonly MaterialTapTargetSize materialTapTargetSize;

        public readonly FocusNode focusNode;

        public readonly bool autofocus;

        public readonly Clip clipBehavior;

        public readonly bool enableFeedback;

        public override State createState() {
            return new _RawMaterialButtonState();
        }
    }


    class _RawMaterialButtonState : State<RawMaterialButton> {
        readonly HashSet<MaterialState> _states = new HashSet<MaterialState>();

        bool _hovered {
            get { return _states.Contains(MaterialState.hovered); }
        }

        bool _focused {
            get { return _states.Contains(MaterialState.focused); }
        }

        bool _pressed {
            get { return _states.Contains(MaterialState.pressed); }
        }

        bool _disabled {
            get { return _states.Contains(MaterialState.disabled); }
        }

        void _updateState(MaterialState state, bool value) {
            if (value) {
                _states.Add(state);
            }
            else {
                _states.Remove(state);
            }
        }

        void _handleHighlightChanged(bool value) {
            if (_pressed != value) {
                setState(() => {
                    _updateState(MaterialState.pressed, value);
                    if (widget.onHighlightChanged != null) {
                        widget.onHighlightChanged(value);
                    }
                });
            }
        }

        void _handleHoveredChanged(bool value) {
            if (_hovered != value) {
                setState(() => { _updateState(MaterialState.hovered, value); });
            }
        }

        void _handleFocusedChanged(bool value) {
            if (_focused != value) {
                setState(() => { _updateState(MaterialState.focused, value); });
            }
        }

        public override void initState() {
            base.initState();
            _updateState(MaterialState.disabled, !widget.enabled);
        }

        public override void didUpdateWidget(StatefulWidget _oldWidget) {
            RawMaterialButton oldWidget = _oldWidget as RawMaterialButton;
            base.didUpdateWidget(oldWidget);
            _updateState(MaterialState.disabled, !widget.enabled);
            if (_disabled && _pressed) {
                _handleHighlightChanged(false);
            }
        }

        float _effectiveElevation {
            get {
                if (_disabled) {
                    return widget.disabledElevation;
                }

                if (_pressed) {
                    return widget.highlightElevation;
                }

                if (_hovered) {
                    return widget.hoverElevation;
                }

                if (_focused) {
                    return widget.focusElevation;
                }

                return widget.elevation;
            }
        }

        public override Widget build(BuildContext context) {
            Color effectiveTextColor = MaterialStateProperty<Color>.resolveAsMaterialStateProperty<Color>(widget.textStyle?.color, _states);
            ShapeBorder effectiveShape = MaterialStateProperty<Color>.resolveAsMaterialStateProperty<ShapeBorder>(widget.shape, _states);
            Offset densityAdjustment = widget.visualDensity.baseSizeAdjustment;
            BoxConstraints effectiveConstraints = widget.visualDensity.effectiveConstraints(widget.constraints);
            EdgeInsetsGeometry padding = widget.padding.add(
                EdgeInsets.only(
                    left: densityAdjustment.dx,
                    top: densityAdjustment.dy,
                    right: densityAdjustment.dx,
                    bottom: densityAdjustment.dy
                )
            ).clamp(EdgeInsets.zero, EdgeInsetsGeometry.infinityEdgeInsetsGeometry) as EdgeInsets;

            Widget result = new ConstrainedBox(
                constraints: effectiveConstraints,
                child: new Material(
                    elevation: _effectiveElevation,
                    textStyle: widget.textStyle?.copyWith(color: effectiveTextColor),
                    shape: effectiveShape,
                    color: widget.fillColor,
                    type: widget.fillColor == null ? MaterialType.transparency : MaterialType.button,
                    animationDuration: widget.animationDuration,
                    clipBehavior: widget.clipBehavior,
                    child: new InkWell(
                        focusNode: widget.focusNode,
                        canRequestFocus: widget.enabled,
                        onFocusChange: _handleFocusedChanged,
                        autofocus: widget.autofocus,
                        onHighlightChanged: _handleHighlightChanged,
                        splashColor: widget.splashColor,
                        highlightColor: widget.highlightColor,
                        focusColor: widget.focusColor,
                        hoverColor: widget.hoverColor,
                        onHover: _handleHoveredChanged,
                        onTap: widget.onPressed == null
                            ? (GestureTapCallback) null
                            : () => {
                                if (widget.onPressed != null) {
                                    widget.onPressed();
                                }
                            },
                        onLongPress: widget.onLongPress,
                        enableFeedback: widget.enableFeedback,
                        customBorder: effectiveShape,
                        child: IconTheme.merge(
                            data: new IconThemeData(color: effectiveTextColor),
                            child: new Container(
                                padding: padding,
                                child: new Center(
                                    widthFactor: 1.0f,
                                    heightFactor: 1.0f,
                                    child: widget.child)
                            )
                        )
                    )
                )
            );

            Size minSize = null;
            switch (widget.materialTapTargetSize) {
                case MaterialTapTargetSize.padded:
                    minSize = new Size(
                        material_.kMinInteractiveDimension + densityAdjustment.dx,
                        material_.kMinInteractiveDimension + densityAdjustment.dy
                    );
                    D.assert(minSize.width >= 0.0f);
                    D.assert(minSize.height >= 0.0f);
                    break;
                case MaterialTapTargetSize.shrinkWrap:
                    minSize = Size.zero;
                    break;
            }

            return new _InputPadding(
                minSize: minSize,
                child: result
            );
        }
    }

    class _InputPadding : SingleChildRenderObjectWidget {
        public _InputPadding(
            Key key = null,
            Widget child = null,
            Size minSize = null) : base(key: key, child: child) {
            this.minSize = minSize;
        }

        public readonly Size minSize;

        public override RenderObject createRenderObject(BuildContext context) {
            return new _RenderInputPadding(minSize);
        }

        public override void updateRenderObject(BuildContext context, RenderObject renderObject) {
            var _renderObject = (_RenderInputPadding) renderObject;
            _renderObject.minSize = minSize;
        }
    }

    class _RenderInputPadding : RenderShiftedBox {
        public _RenderInputPadding(
            Size minSize,
            RenderBox child = null
        ) : base(child: child) {
            _minSize = minSize;
        }

        public Size minSize {
            get { return _minSize; }
            set {
                if (_minSize == value) {
                    return;
                }

                _minSize = value;
                markNeedsLayout();
            }
        }

        Size _minSize;

        protected internal override float computeMinIntrinsicWidth(float height) {
            if (child != null) {
                return Mathf.Max(child.getMinIntrinsicWidth(height), minSize.width);
            }

            return 0.0f;
        }

        protected internal override float computeMinIntrinsicHeight(float width) {
            if (child != null) {
                return Mathf.Max(child.getMinIntrinsicHeight(width), minSize.height);
            }

            return 0.0f;
        }

        protected internal override float computeMaxIntrinsicWidth(float height) {
            if (child != null) {
                return Mathf.Max(child.getMaxIntrinsicWidth(height), minSize.width);
            }

            return 0.0f;
        }

        protected internal override float computeMaxIntrinsicHeight(float width) {
            if (child != null) {
                return Mathf.Max(child.getMaxIntrinsicHeight(width), minSize.height);
            }

            return 0.0f;
        }

        protected override void performLayout() {
            BoxConstraints constraints = this.constraints;
            if (child != null) {
                child.layout(constraints, parentUsesSize: true);
                float height = Mathf.Max(child.size.width, minSize.width);
                float width = Mathf.Max(child.size.height, minSize.height);
                size = constraints.constrain(new Size(height, width));
                BoxParentData childParentData = child.parentData as BoxParentData;
                childParentData.offset = Alignment.center.alongOffset(size - child.size as Offset);
            }
            else {
                size = Size.zero;
            }
        }

        public override bool hitTest(BoxHitTestResult result, Offset position = null) {
            if (base.hitTest(result, position: position)) {
                return true;
            }

            Offset center = child.size.center(Offset.zero);
            return result.addWithRawTransform(
                transform: MatrixUtils.forceToPoint(center),
                position: center,
                hitTest: (BoxHitTestResult boxHitTest, Offset offsetPosition) => {
                    D.assert(offsetPosition == center);
                    //WARNING: inconsistent with flutter (zxw): I believe that there is a bug here in flutter
                    //in flutter, the following line is "return child.hitTest(boxHitTest, position: center); ". This is nonsense since it will always return true regardless of the value of the input parameter: position.
                    //we have tested a bit in flutter and found that, since an inputPadding has a Semantics as it parent which shares the same size, the Semantics's hitTest can hide this bug in flutter
                    //Therefore this bug only occurs in UIWidgets
                    //We are not very clear whether this is the best fix though. Please feel free to optimize it
                    return child.hitTest(boxHitTest, position: position);
                }
            );
        }
    }
}