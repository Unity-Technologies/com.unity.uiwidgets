using System;
using System.Collections.Generic;
using uiwidgets;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.rendering;
using Unity.UIWidgets.scheduler2;
using Unity.UIWidgets.ui;
using Unity.UIWidgets.widgets;

namespace Unity.UIWidgets.material {
    class RadioUtils {
        public const float _kOuterRadius = 8.0f;
        public const float _kInnerRadius = 4.5f;
    }

    public class Radio<T> : StatefulWidget {
        public Radio(
            Key key = null,
            T value = default,
            T groupValue = default,
            ValueChanged<T> onChanged = null,
            bool toggleable = false,
            Color activeColor = null,
            Color focusColor = null,
            Color hoverColor = null,
            MaterialTapTargetSize? materialTapTargetSize = null,
            VisualDensity visualDensity = null,
            FocusNode focusNode = null,
            bool autofocus = false
        ) : base(key: key) {
            D.assert(value != null);
            D.assert(groupValue != null);
            D.assert(onChanged != null);
            this.value = value;
            this.groupValue = groupValue;
            this.onChanged = onChanged;
            this.toggleable = toggleable;
            this.activeColor = activeColor;
            this.focusColor = focusColor;
            this.hoverColor = hoverColor;
            this.materialTapTargetSize = materialTapTargetSize;
            this.visualDensity = visualDensity;
            this.focusNode = focusNode;
            this.autofocus = autofocus;
        }

        public readonly T value;

        public readonly T groupValue;

        public readonly ValueChanged<T> onChanged;
        
        public readonly  bool toggleable;

        public readonly Color activeColor;

        public readonly MaterialTapTargetSize? materialTapTargetSize;
        
        public readonly VisualDensity visualDensity;

        public readonly Color focusColor;
        
        public readonly Color hoverColor;

        public readonly FocusNode focusNode;

        public readonly bool autofocus;

        public override State createState() {
            return new _RadioState<T>();
        }
    }

    class _RadioState<T> : TickerProviderStateMixin<Radio<T>> {
        bool enabled {
            get { return widget.onChanged != null; }
        }
        
        Dictionary<LocalKey, ActionFactory> _actionMap;

        public override void initState() {
            base.initState();
            _actionMap = new Dictionary<LocalKey, ActionFactory>();
            _actionMap[ActivateAction.key] = _createAction;
        }

        void _actionHandler(FocusNode node, Intent intent) {
            if (widget.onChanged != null) {
                widget.onChanged(widget.value);
            }
            RenderObject renderObject = node.context.findRenderObject();
        }

        UiWidgetAction _createAction() {
            return new CallbackAction(
                ActivateAction.key,
                onInvoke: _actionHandler
            );
        }

        bool _focused = false;
        void _handleHighlightChanged(bool focused) {
            if (_focused != focused) {
                setState(() =>{ _focused = focused; });
            }
        }

        bool _hovering = false;
        void _handleHoverChanged(bool hovering) {
            if (_hovering != hovering) {
                setState(() => { _hovering = hovering; });
            }
        }

        Color _getInactiveColor(ThemeData themeData) {
            return enabled ? themeData.unselectedWidgetColor : themeData.disabledColor;
        }

        void _handleChanged(bool? selected) {
            if (selected == null) {
                widget.onChanged(default);
                return;
            }
            if (selected == true) {
                widget.onChanged(widget.value);
            }
        }

        public override Widget build(BuildContext context) {
            D.assert(material_.debugCheckHasMaterial(context));
            ThemeData themeData = Theme.of(context);
            Size size;
            switch (widget.materialTapTargetSize ?? themeData.materialTapTargetSize) {
                case MaterialTapTargetSize.padded:
                    size = new Size(2 * material_.kRadialReactionRadius + 8.0f,
                        2 * material_.kRadialReactionRadius + 8.0f);
                    break;
                case MaterialTapTargetSize.shrinkWrap:
                    size = new Size(2 * material_.kRadialReactionRadius, 2 * material_.kRadialReactionRadius);
                    break;
                default:
                    throw new Exception("Unknown material tap target size");
            }
            size += (widget.visualDensity ?? themeData.visualDensity).baseSizeAdjustment;
            BoxConstraints additionalConstraints = BoxConstraints.tight(size);
            return new FocusableActionDetector(
                actions: _actionMap,
                focusNode: widget.focusNode,
                autofocus: widget.autofocus,
                enabled: enabled,
                onShowFocusHighlight: _handleHighlightChanged,
                onShowHoverHighlight: _handleHoverChanged,
                child: new Builder(
                    builder: (BuildContext subContext) => {
                    return new _RadioRenderObjectWidget(
                    selected: widget.value.Equals(widget.groupValue),
                    activeColor: widget.activeColor ?? themeData.toggleableActiveColor,
                    inactiveColor: _getInactiveColor(themeData),
                    focusColor: widget.focusColor ?? themeData.focusColor,
                    hoverColor: widget.hoverColor ?? themeData.hoverColor,
                    onChanged: enabled ? _handleChanged : (ValueChanged<bool?>)null,
                    toggleable: widget.toggleable,
                    additionalConstraints: additionalConstraints,
                    vsync: this,
                    hasFocus: _focused,
                    hovering: _hovering
                );
            }
            )
            );
        }
    }

    class _RadioRenderObjectWidget : LeafRenderObjectWidget {
        public _RadioRenderObjectWidget(
            Key key = null,
            bool? selected = null,
            Color activeColor = null,
            Color inactiveColor = null,
            Color focusColor = null,
            Color hoverColor = null,
            BoxConstraints additionalConstraints = null,
            ValueChanged<bool?> onChanged = null,
            bool? toggleable = null,
            TickerProvider vsync = null,
            bool hasFocus = false,
            bool hovering = false
        ) : base(key: key) {
            D.assert(selected != null);
            D.assert(activeColor != null);
            D.assert(inactiveColor != null);
            D.assert(additionalConstraints != null);
            D.assert(vsync != null);
            D.assert(toggleable != null);
            this.selected = selected.Value;
            this.activeColor = activeColor;
            this.inactiveColor = inactiveColor;
            this.focusColor = focusColor;
            this.hoverColor = hoverColor;
            this.additionalConstraints = additionalConstraints;
            this.onChanged = onChanged;
            this.toggleable = toggleable.Value;
            this.vsync = vsync;
            this.hasFocus = hasFocus;
            this.hovering = hovering;
        }

        public readonly bool selected;
        public readonly  bool hasFocus;
        public readonly  bool hovering;
        public readonly Color activeColor;
        public readonly Color inactiveColor;
        public readonly  Color focusColor;
        public readonly  Color hoverColor;
        public readonly ValueChanged<bool?> onChanged;
        public readonly  bool toggleable;
        public readonly TickerProvider vsync;
        public readonly BoxConstraints additionalConstraints;

        public override RenderObject createRenderObject(BuildContext context) {
            return new _RenderRadio(
                value: selected,
                activeColor: activeColor,
                inactiveColor: inactiveColor,
                focusColor: focusColor,
                hoverColor: hoverColor,
                onChanged: onChanged,
                tristate: toggleable,
                vsync: vsync,
                additionalConstraints: additionalConstraints,
                hasFocus: hasFocus,
                hovering: hovering
            );
        }

        public override void updateRenderObject(BuildContext context, RenderObject _renderObject) {
            _RenderRadio renderObject = _renderObject as _RenderRadio;
            renderObject.value = selected;
            renderObject.activeColor = activeColor;
            renderObject.inactiveColor = inactiveColor;
            renderObject.focusColor = focusColor;
            renderObject.hoverColor = hoverColor;
            renderObject.onChanged = onChanged;
            renderObject.tristate = toggleable;
            renderObject.additionalConstraints = additionalConstraints;
            renderObject.vsync = vsync;
            renderObject.hasFocus = hasFocus;
            renderObject.hovering = hovering;
        }
    }

    class _RenderRadio : RenderToggleable {
        public _RenderRadio(
            bool? value,
            Color activeColor,
            Color inactiveColor,
            Color focusColor,
            Color hoverColor,
            ValueChanged<bool?> onChanged,
            bool tristate,
            BoxConstraints additionalConstraints,
            TickerProvider vsync,
            bool hasFocus,
            bool hovering
        ) : base(
            value: value,
            activeColor: activeColor,
            inactiveColor: inactiveColor,
            focusColor: focusColor,
            hoverColor: hoverColor,
            onChanged: onChanged,
            tristate: tristate,
            additionalConstraints: additionalConstraints,
            vsync: vsync,
            hasFocus: hasFocus,
            hovering: hovering
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