using System;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.rendering;
using Unity.UIWidgets.scheduler;
using Unity.UIWidgets.ui;
using Unity.UIWidgets.widgets;

namespace Unity.UIWidgets.material {
    class RadioUtils {
        public const float _kOuterRadius = 8.0f;
        public const float _kInnerRadius = 4.5f;
    }

    public class Radio<T> : StatefulWidget where T : class {
        public Radio(
            Key key = null,
            T value = null,
            T groupValue = null,
            ValueChanged<T> onChanged = null,
            Color activeColor = null,
            MaterialTapTargetSize? materialTapTargetSize = null
        ) : base(key: key) {
            D.assert(value != null);
            D.assert(groupValue != null);
            D.assert(onChanged != null);
            this.value = value;
            this.groupValue = groupValue;
            this.onChanged = onChanged;
            this.activeColor = activeColor;
            this.materialTapTargetSize = materialTapTargetSize;
        }

        public readonly T value;

        public readonly T groupValue;

        public readonly ValueChanged<T> onChanged;

        public readonly Color activeColor;

        public readonly MaterialTapTargetSize? materialTapTargetSize;

        public override State createState() {
            return new _RadioState<T>();
        }
    }

    class _RadioState<T> : TickerProviderStateMixin<Radio<T>> where T : class {
        bool _enabled {
            get { return widget.onChanged != null; }
        }

        Color _getInactiveColor(ThemeData themeData) {
            return _enabled ? themeData.unselectedWidgetColor : themeData.disabledColor;
        }

        void _handleChanged(bool? selected) {
            if (selected == true) {
                widget.onChanged(widget.value);
            }
        }

        public override Widget build(BuildContext context) {
            D.assert(MaterialD.debugCheckHasMaterial(context));
            ThemeData themeData = Theme.of(context);
            Size size;
            switch (widget.materialTapTargetSize ?? themeData.materialTapTargetSize) {
                case MaterialTapTargetSize.padded:
                    size = new Size(2 * Constants.kRadialReactionRadius + 8.0f,
                        2 * Constants.kRadialReactionRadius + 8.0f);
                    break;
                case MaterialTapTargetSize.shrinkWrap:
                    size = new Size(2 * Constants.kRadialReactionRadius, 2 * Constants.kRadialReactionRadius);
                    break;
                default:
                    throw new Exception("Unknown material tap target size");
            }

            BoxConstraints additionalConstraints = BoxConstraints.tight(size);
            return new _RadioRenderObjectWidget(
                selected: widget.value == widget.groupValue,
                activeColor: widget.activeColor ?? themeData.toggleableActiveColor,
                inactiveColor: _getInactiveColor(themeData),
                onChanged: _enabled ? _handleChanged : (ValueChanged<bool?>) null,
                additionalConstraints: additionalConstraints,
                vsync: this
            );
        }
    }

    class _RadioRenderObjectWidget : LeafRenderObjectWidget {
        public _RadioRenderObjectWidget(
            Key key = null,
            bool? selected = null,
            Color activeColor = null,
            Color inactiveColor = null,
            BoxConstraints additionalConstraints = null,
            ValueChanged<bool?> onChanged = null,
            TickerProvider vsync = null
        ) : base(key: key) {
            D.assert(selected != null);
            D.assert(activeColor != null);
            D.assert(inactiveColor != null);
            D.assert(additionalConstraints != null);
            D.assert(vsync != null);
            this.selected = selected;
            this.activeColor = activeColor;
            this.inactiveColor = inactiveColor;
            this.additionalConstraints = additionalConstraints;
            this.onChanged = onChanged;
            this.vsync = vsync;
        }

        public readonly bool? selected;
        public readonly Color activeColor;
        public readonly Color inactiveColor;
        public readonly BoxConstraints additionalConstraints;
        public readonly ValueChanged<bool?> onChanged;
        public readonly TickerProvider vsync;

        public override RenderObject createRenderObject(BuildContext context) {
            return new _RenderRadio(
                value: selected,
                activeColor: activeColor,
                inactiveColor: inactiveColor,
                onChanged: onChanged,
                vsync: vsync,
                additionalConstraints: additionalConstraints
            );
        }

        public override void updateRenderObject(BuildContext context, RenderObject _renderObject) {
            _RenderRadio renderObject = _renderObject as _RenderRadio;
            renderObject.value = selected;
            renderObject.activeColor = activeColor;
            renderObject.inactiveColor = inactiveColor;
            renderObject.onChanged = onChanged;
            renderObject.additionalConstraints = additionalConstraints;
            renderObject.vsync = vsync;
        }
    }

    class _RenderRadio : RenderToggleable {
        public _RenderRadio(
            bool? value,
            Color activeColor,
            Color inactiveColor,
            ValueChanged<bool?> onChanged,
            BoxConstraints additionalConstraints,
            TickerProvider vsync
        ) : base(
            value: value,
            tristate: false,
            activeColor: activeColor,
            inactiveColor: inactiveColor,
            onChanged: onChanged,
            additionalConstraints: additionalConstraints,
            vsync: vsync
        ) {
        }

        public override void paint(PaintingContext context, Offset offset) {
            Canvas canvas = context.canvas;

            paintRadialReaction(canvas, offset, size.center(Offset.zero));

            Offset center = (offset & size).center;
            Color radioColor = onChanged != null ? activeColor : inactiveColor;

            Paint paint = new Paint();
            paint.color = Color.lerp(inactiveColor, radioColor, position.value);
            paint.style = PaintingStyle.stroke;
            paint.strokeWidth = 2.0f;
            canvas.drawCircle(center, RadioUtils._kOuterRadius, paint);

            if (!position.isDismissed) {
                paint.style = PaintingStyle.fill;
                canvas.drawCircle(center, RadioUtils._kInnerRadius * position.value, paint);
            }
        }
    }
}