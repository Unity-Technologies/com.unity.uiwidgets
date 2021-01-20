using System;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.gestures;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.rendering;
using Unity.UIWidgets.ui;
using Unity.UIWidgets.widgets;
using TextStyle = Unity.UIWidgets.painting.TextStyle;

namespace Unity.UIWidgets.material {
    public class RawMaterialButton : StatefulWidget {
        public RawMaterialButton(
            Key key = null,
            VoidCallback onPressed = null,
            ValueChanged<bool> onHighlightChanged = null,
            TextStyle textStyle = null,
            Color fillColor = null,
            Color highlightColor = null,
            Color splashColor = null,
            float elevation = 2.0f,
            float highlightElevation = 8.0f,
            float disabledElevation = 0.0f,
            EdgeInsets padding = null,
            BoxConstraints constraints = null,
            ShapeBorder shape = null,
            TimeSpan? animationDuration = null,
            Clip clipBehavior = Clip.none,
            MaterialTapTargetSize? materialTapTargetSize = null,
            Widget child = null) : base(key: key) {
            D.assert(elevation >= 0.0);
            D.assert(highlightElevation >= 0.0);
            D.assert(disabledElevation >= 0.0);

            MaterialTapTargetSize _materialTapTargetSize = materialTapTargetSize ?? MaterialTapTargetSize.padded;
            shape = shape ?? new RoundedRectangleBorder();
            padding = padding ?? EdgeInsets.zero;
            constraints = constraints ?? new BoxConstraints(minWidth: 88.0f, minHeight: 36.0f);
            TimeSpan _animationDuration = animationDuration ?? Constants.kThemeChangeDuration;

            this.onPressed = onPressed;
            this.onHighlightChanged = onHighlightChanged;
            this.textStyle = textStyle;
            this.fillColor = fillColor;
            this.highlightColor = highlightColor;
            this.splashColor = splashColor;
            this.elevation = elevation;
            this.highlightElevation = highlightElevation;
            this.disabledElevation = disabledElevation;
            this.padding = padding;
            this.constraints = constraints;
            this.shape = shape;
            this.animationDuration = _animationDuration;
            this.clipBehavior = clipBehavior;
            this.materialTapTargetSize = _materialTapTargetSize;
            this.child = child;
        }

        public readonly VoidCallback onPressed;

        public readonly ValueChanged<bool> onHighlightChanged;

        public readonly TextStyle textStyle;

        public readonly Color fillColor;

        public readonly Color highlightColor;

        public readonly Color splashColor;

        public readonly float elevation;

        public readonly float highlightElevation;

        public readonly float disabledElevation;

        public readonly EdgeInsets padding;

        public readonly BoxConstraints constraints;

        public readonly ShapeBorder shape;

        public readonly TimeSpan animationDuration;

        public readonly Widget child;

        public bool enabled {
            get { return onPressed != null; }
        }

        public readonly MaterialTapTargetSize materialTapTargetSize;

        public readonly Clip clipBehavior;

        public override State createState() {
            return new _RawMaterialButtonState();
        }
    }


    class _RawMaterialButtonState : State<RawMaterialButton> {
        bool _highlight = false;

        void _handleHighlightChanged(bool value) {
            if (_highlight != value) {
                setState(() => {
                    _highlight = value;
                    if (widget.onHighlightChanged != null) {
                        widget.onHighlightChanged(value);
                    }
                });
            }
        }

        public override void didUpdateWidget(StatefulWidget _oldWidget) {
            RawMaterialButton oldWidget = _oldWidget as RawMaterialButton;
            base.didUpdateWidget(oldWidget);
            if (_highlight && !widget.enabled) {
                _highlight = false;
                if (widget.onHighlightChanged != null) {
                    widget.onHighlightChanged(false);
                }
            }
        }

        public override Widget build(BuildContext context) {
            float elevation = widget.enabled
                ? (_highlight ? widget.highlightElevation : widget.elevation)
                : widget.disabledElevation;

            Widget result = new ConstrainedBox(
                constraints: widget.constraints,
                child: new Material(
                    elevation: elevation,
                    textStyle: widget.textStyle,
                    shape: widget.shape,
                    color: widget.fillColor,
                    type: widget.fillColor == null ? MaterialType.transparency : MaterialType.button,
                    animationDuration: widget.animationDuration,
                    clipBehavior: widget.clipBehavior,
                    child: new InkWell(
                        onHighlightChanged: this._handleHighlightChanged,
                        splashColor: widget.splashColor,
                        highlightColor: widget.highlightColor,
                        onTap: widget.onPressed == null
                            ? (GestureTapCallback) null
                            : () => {
                                if (widget.onPressed != null) {
                                    widget.onPressed();
                                }
                            },
                        customBorder: widget.shape,
                        child: IconTheme.merge(
                            data: new IconThemeData(color: widget.textStyle?.color),
                            child: new Container(
                                padding: widget.padding,
                                child: new Center(
                                    widthFactor: 1.0f,
                                    heightFactor: 1.0f,
                                    child: widget.child)
                            )
                        )
                    )
                )
            );

            return result;
        }
    }
}