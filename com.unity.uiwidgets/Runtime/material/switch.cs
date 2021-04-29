using System.Collections.Generic;
using uiwidgets;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.gestures;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.rendering;
using Unity.UIWidgets.ui;
using Unity.UIWidgets.widgets;

namespace Unity.UIWidgets.material {
    enum _SwitchType {
        material,
        adaptive
    }

    public class Switch : StatefulWidget {
        internal const float _kTrackHeight = 14.0f;
        internal const float _kTrackWidth = 33.0f;
        internal const float _kTrackRadius = _kTrackHeight / 2.0f;
        internal const float _kThumbRadius = 10.0f;
        internal const float _kSwitchWidth = _kTrackWidth - 2 * _kTrackRadius + 2 * material_.kRadialReactionRadius;
        internal const float _kSwitchHeight = 2 * material_.kRadialReactionRadius + 8.0f;
        internal const float _kSwitchHeightCollapsed = 2 * material_.kRadialReactionRadius;

        public Switch(
            Key key = null,
            bool? value = null,
            ValueChanged<bool?> onChanged = null,
            Color activeColor = null,
            Color activeTrackColor = null,
            Color inactiveThumbColor = null,
            Color inactiveTrackColor = null,
            ImageProvider activeThumbImage = null,
            ImageErrorListener onActiveThumbImageError = null,
            ImageProvider inactiveThumbImage = null,
            ImageErrorListener onInactiveThumbImageError = null,
            MaterialTapTargetSize? materialTapTargetSize = null,
            DragStartBehavior dragStartBehavior = DragStartBehavior.start,
            Color focusColor = null,
            Color hoverColor = null,
            FocusNode focusNode = null,
            bool autofocus = false
        ) : this(
            key: key,
            value: value,
            onChanged: onChanged,
            activeColor: activeColor,
            activeTrackColor: activeTrackColor,
            inactiveThumbColor: inactiveThumbColor,
            inactiveTrackColor: inactiveTrackColor,
            activeThumbImage: activeThumbImage,
            onActiveThumbImageError: onActiveThumbImageError,
            inactiveThumbImage: inactiveThumbImage,
            onInactiveThumbImageError: onInactiveThumbImageError,
            materialTapTargetSize: materialTapTargetSize,
            switchType: _SwitchType.material,
            dragStartBehavior: dragStartBehavior,
            focusColor: focusColor,
            hoverColor: hoverColor,
            focusNode: focusNode,
            autofocus: autofocus
        ) {
        }

        Switch(
            Key key = null,
            bool? value = null,
            ValueChanged<bool?> onChanged = null,
            Color activeColor = null,
            Color activeTrackColor = null,
            Color inactiveThumbColor = null,
            Color inactiveTrackColor = null,
            ImageProvider activeThumbImage = null,
            ImageErrorListener onActiveThumbImageError = null,
            ImageProvider inactiveThumbImage = null,
            ImageErrorListener onInactiveThumbImageError = null,
            MaterialTapTargetSize? materialTapTargetSize = null,
            _SwitchType switchType = _SwitchType.material,
            DragStartBehavior dragStartBehavior = DragStartBehavior.start,
            Color focusColor = null,
            Color hoverColor = null,
            FocusNode focusNode = null,
            bool autofocus = false
        ) : base(key: key) {
            D.assert(value != null);
            this.value = value.Value;
            D.assert(activeThumbImage != null || onActiveThumbImageError == null);
            D.assert(inactiveThumbImage != null || onInactiveThumbImageError == null);
            this.onChanged = onChanged;
            this.activeColor = activeColor;
            this.activeTrackColor = activeTrackColor;
            this.inactiveThumbColor = inactiveThumbColor;
            this.inactiveTrackColor = inactiveTrackColor;
            this.activeThumbImage = activeThumbImage;
            this.onActiveThumbImageError = onActiveThumbImageError;
            this.inactiveThumbImage = inactiveThumbImage;
            this.onInactiveThumbImageError = onInactiveThumbImageError;
            this.materialTapTargetSize = materialTapTargetSize;
            _switchType = switchType;
            this.dragStartBehavior = dragStartBehavior;
            this.focusColor = focusColor;
            this.hoverColor = hoverColor;
            this.focusNode = focusNode;
            this.autofocus = autofocus;
        }

        public static Switch adaptive(
            Key key = null,
            bool? value = null,
            ValueChanged<bool?> onChanged = null,
            Color activeColor = null,
            Color activeTrackColor = null,
            Color inactiveThumbColor = null,
            Color inactiveTrackColor = null,
            ImageProvider activeThumbImage = null,
            ImageErrorListener onActiveThumbImageError = null,
            ImageProvider inactiveThumbImage = null,
            ImageErrorListener onInactiveThumbImageError = null,
            MaterialTapTargetSize? materialTapTargetSize = null,
            DragStartBehavior dragStartBehavior = DragStartBehavior.start,
            Color focusColor = null,
            Color hoverColor = null,
            FocusNode focusNode = null,
            bool autofocus = false
        ) {
            return new Switch(key: key,
                value: value,
                onChanged: onChanged,
                activeColor: activeColor,
                activeTrackColor: activeTrackColor,
                inactiveThumbColor: inactiveThumbColor,
                inactiveTrackColor: inactiveTrackColor,
                activeThumbImage: activeThumbImage,
                onActiveThumbImageError: onActiveThumbImageError,
                inactiveThumbImage: inactiveThumbImage,
                onInactiveThumbImageError: onInactiveThumbImageError,
                materialTapTargetSize: materialTapTargetSize,
                switchType: _SwitchType.adaptive,
                dragStartBehavior: dragStartBehavior,
                focusColor: focusColor,
                hoverColor: hoverColor,
                focusNode: focusNode,
                autofocus: autofocus
            );
        }

        public readonly bool value;

        public readonly ValueChanged<bool?> onChanged;

        public readonly Color activeColor;

        public readonly Color activeTrackColor;

        public readonly Color inactiveThumbColor;

        public readonly Color inactiveTrackColor;

        public readonly ImageProvider activeThumbImage;

        public readonly ImageErrorListener onActiveThumbImageError;

        public readonly ImageProvider inactiveThumbImage;

        public readonly ImageErrorListener onInactiveThumbImageError;

        public readonly MaterialTapTargetSize? materialTapTargetSize;

        internal readonly _SwitchType _switchType;

        public readonly DragStartBehavior dragStartBehavior;

        public readonly Color focusColor;

        public readonly Color hoverColor;

        public readonly FocusNode focusNode;

        public readonly bool autofocus;

        public override State createState() {
            return new _SwitchState();
        }

        public override void debugFillProperties(DiagnosticPropertiesBuilder properties) {
            base.debugFillProperties(properties);
            properties.add(new FlagProperty("value", value: value, ifTrue: "on", ifFalse: "off", showName: true));
            properties.add(
                new ObjectFlagProperty<ValueChanged<bool?>>("onChanged", onChanged, ifNull: "disabled"));
        }
    }

    class _SwitchState : TickerProviderStateMixin<Switch> {
        Dictionary<LocalKey, ActionFactory> _actionMap;

        public override void initState() {
            base.initState();
            _actionMap = new Dictionary<LocalKey, ActionFactory>();
            _actionMap[ActivateAction.key] = _createAction;
        }

        void _actionHandler(FocusNode node, Intent intent) {
            if (widget.onChanged != null) {
                widget.onChanged(!widget.value);
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

        void _handleFocusHighlightChanged(bool focused) {
            if (focused != _focused) {
                setState(() => { _focused = focused; });
            }
        }

        bool _hovering = false;

        void _handleHoverChanged(bool hovering) {
            if (hovering != _hovering) {
                setState(() => { _hovering = hovering; });
            }
        }

        Size getSwitchSize(ThemeData theme) {
            switch (widget.materialTapTargetSize ?? theme.materialTapTargetSize) {
                case MaterialTapTargetSize.padded:
                    return new Size(Switch._kSwitchWidth, Switch._kSwitchHeight);
                case MaterialTapTargetSize.shrinkWrap:
                    return new Size(Switch._kSwitchWidth, Switch._kSwitchHeightCollapsed);
            }

            D.assert(false);
            return null;
        }

        bool enabled {
            get { return widget.onChanged != null; }
        }

        internal void _didFinishDragging() {
            setState(() => { });
        }

        Widget buildMaterialSwitch(BuildContext context) {
            D.assert(material_.debugCheckHasMaterial(context));
            ThemeData theme = Theme.of(context);
            bool isDark = theme.brightness == Brightness.dark;

            Color activeThumbColor = widget.activeColor ?? theme.toggleableActiveColor;
            Color activeTrackColor = widget.activeTrackColor ?? activeThumbColor.withAlpha(0x80);
            Color hoverColor = widget.hoverColor ?? theme.hoverColor;
            Color focusColor = widget.focusColor ?? theme.focusColor;

            Color inactiveThumbColor;
            Color inactiveTrackColor;
            if (enabled) {
                Color black32 = new Color(0x52000000);
                inactiveThumbColor = widget.inactiveThumbColor ??
                                     (isDark ? Colors.grey.shade400 : Colors.grey.shade50);
                inactiveTrackColor = widget.inactiveTrackColor ?? (isDark ? Colors.white30 : black32);
            }
            else {
                inactiveThumbColor = widget.inactiveThumbColor ??
                                     (isDark ? Colors.grey.shade800 : Colors.grey.shade400);
                inactiveTrackColor = widget.inactiveTrackColor ?? (isDark ? Colors.white10 : Colors.black12);
            }

            return new FocusableActionDetector(
                actions: _actionMap,
                focusNode: widget.focusNode,
                autofocus: widget.autofocus,
                enabled: enabled,
                onShowFocusHighlight: _handleFocusHighlightChanged,
                onShowHoverHighlight: _handleHoverChanged,
                child: new Builder(
                    builder: (BuildContext subContext) => {
                        return new _SwitchRenderObjectWidget(
                            dragStartBehavior: widget.dragStartBehavior,
                            value: widget.value,
                            activeColor: activeThumbColor,
                            inactiveColor: inactiveThumbColor,
                            hoverColor: hoverColor,
                            focusColor: focusColor,
                            activeThumbImage: widget.activeThumbImage,
                            onActiveThumbImageError: widget.onActiveThumbImageError,
                            inactiveThumbImage: widget.inactiveThumbImage,
                            onInactiveThumbImageError: widget.onInactiveThumbImageError,
                            activeTrackColor: activeTrackColor,
                            inactiveTrackColor: inactiveTrackColor,
                            configuration: ImageUtils.createLocalImageConfiguration(subContext),
                            onChanged: widget.onChanged,
                            additionalConstraints: BoxConstraints.tight(getSwitchSize(theme)),
                            hasFocus: _focused,
                            hovering: _hovering,
                            state: this
                        );
                    }
                )
            );
        }

        public override Widget build(BuildContext context) {
            switch (widget._switchType) {
                case _SwitchType.material:
                    return buildMaterialSwitch(context);
                case _SwitchType.adaptive: {
                    return buildMaterialSwitch(context);
                }
            }

            D.assert(false);
            return null;
        }
    }

    class _SwitchRenderObjectWidget : LeafRenderObjectWidget {
        public _SwitchRenderObjectWidget(
            Key key = null,
            bool? value = null,
            Color activeColor = null,
            Color inactiveColor = null,
            Color hoverColor = null,
            Color focusColor = null,
            ImageProvider activeThumbImage = null,
            ImageErrorListener onActiveThumbImageError = null,
            ImageProvider inactiveThumbImage = null,
            ImageErrorListener onInactiveThumbImageError = null,
            Color activeTrackColor = null,
            Color inactiveTrackColor = null,
            ImageConfiguration configuration = null,
            ValueChanged<bool?> onChanged = null,
            BoxConstraints additionalConstraints = null,
            DragStartBehavior? dragStartBehavior = null,
            bool hasFocus = false,
            bool hovering = false,
            _SwitchState state = null
        ) : base(key: key) {
            D.assert(value != null);
            this.value = value.Value;
            this.activeColor = activeColor;
            this.inactiveColor = inactiveColor;
            this.hoverColor = hoverColor;
            this.focusColor = focusColor;
            this.activeThumbImage = activeThumbImage;
            this.onActiveThumbImageError = onActiveThumbImageError;
            this.inactiveThumbImage = inactiveThumbImage;
            this.onInactiveThumbImageError = onInactiveThumbImageError;
            this.activeTrackColor = activeTrackColor;
            this.inactiveTrackColor = inactiveTrackColor;
            this.configuration = configuration;
            this.onChanged = onChanged;
            this.additionalConstraints = additionalConstraints;
            this.dragStartBehavior = dragStartBehavior;
            this.hasFocus = hasFocus;
            this.hovering = hovering;
            this.state = state;
        }

        public readonly bool value;
        public readonly Color activeColor;
        public readonly Color inactiveColor;
        public readonly Color hoverColor;
        public readonly Color focusColor;
        public readonly ImageProvider activeThumbImage;
        public readonly ImageErrorListener onActiveThumbImageError;
        public readonly ImageProvider inactiveThumbImage;
        public readonly ImageErrorListener onInactiveThumbImageError;
        public readonly Color activeTrackColor;
        public readonly Color inactiveTrackColor;
        public readonly ImageConfiguration configuration;
        public readonly ValueChanged<bool?> onChanged;
        public readonly BoxConstraints additionalConstraints;
        public readonly DragStartBehavior? dragStartBehavior;
        public readonly bool hasFocus;
        public readonly bool hovering;
        public readonly _SwitchState state;

        public override RenderObject createRenderObject(BuildContext context) {
            return new _RenderSwitch(
                dragStartBehavior: dragStartBehavior,
                value: value,
                activeColor: activeColor,
                inactiveColor: inactiveColor,
                hoverColor: hoverColor,
                focusColor: focusColor,
                activeThumbImage: activeThumbImage,
                onActiveThumbImageError: onActiveThumbImageError,
                inactiveThumbImage: inactiveThumbImage,
                onInactiveThumbImageError: onInactiveThumbImageError,
                activeTrackColor: activeTrackColor,
                inactiveTrackColor: inactiveTrackColor,
                configuration: configuration,
                onChanged: onChanged,
                additionalConstraints: additionalConstraints,
                textDirection: Directionality.of(context),
                hasFocus: hasFocus,
                hovering: hovering,
                state: state
            );
        }

        public override void updateRenderObject(BuildContext context, RenderObject renderObjectRaw) {
            _RenderSwitch renderObject = (_RenderSwitch) renderObjectRaw;

            renderObject.value = value;
            renderObject.activeColor = activeColor;
            renderObject.inactiveColor = inactiveColor;
            renderObject.hoverColor = hoverColor;
            renderObject.focusColor = focusColor;
            renderObject.activeThumbImage = activeThumbImage;
            renderObject.onActiveThumbImageError = onActiveThumbImageError;
            renderObject.inactiveThumbImage = inactiveThumbImage;
            renderObject.onInactiveThumbImageError = onInactiveThumbImageError;
            renderObject.activeTrackColor = activeTrackColor;
            renderObject.inactiveTrackColor = inactiveTrackColor;
            renderObject.configuration = configuration;
            renderObject.onChanged = onChanged;
            renderObject.additionalConstraints = additionalConstraints;
            renderObject.textDirection = Directionality.of(context);
            renderObject.dragStartBehavior = dragStartBehavior;
            renderObject.hasFocus = hasFocus;
            renderObject.hovering = hovering;
            renderObject.vsync = state;
        }
    }

    class _RenderSwitch : RenderToggleable {
        public _RenderSwitch(
            bool? value = null,
            Color activeColor = null,
            Color inactiveColor = null,
            Color hoverColor = null,
            Color focusColor = null,
            ImageProvider activeThumbImage = null,
            ImageErrorListener onActiveThumbImageError = null,
            ImageProvider inactiveThumbImage = null,
            ImageErrorListener onInactiveThumbImageError = null,
            Color activeTrackColor = null,
            Color inactiveTrackColor = null,
            ImageConfiguration configuration = null,
            BoxConstraints additionalConstraints = null,
            TextDirection? textDirection = null,
            ValueChanged<bool?> onChanged = null,
            DragStartBehavior? dragStartBehavior = null,
            bool hasFocus = false,
            bool hovering = false,
            _SwitchState state = null
        ) : base(
            value: value,
            tristate: false,
            activeColor: activeColor,
            inactiveColor: inactiveColor,
            hoverColor: hoverColor,
            focusColor: focusColor,
            onChanged: onChanged,
            additionalConstraints: additionalConstraints,
            hasFocus: hasFocus,
            hovering: hovering,
            vsync: state
        ) {
            D.assert(textDirection != null);
            _activeThumbImage = activeThumbImage;
            _onActiveThumbImageError = onActiveThumbImageError;
            _inactiveThumbImage = inactiveThumbImage;
            _onInactiveThumbImageError = onInactiveThumbImageError;
            _activeTrackColor = activeTrackColor;
            _inactiveTrackColor = inactiveTrackColor;
            _configuration = configuration;
            _drag = new HorizontalDragGestureRecognizer {
                onStart = _handleDragStart,
                onUpdate = _handleDragUpdate,
                onEnd = _handleDragEnd,
                dragStartBehavior = dragStartBehavior ?? DragStartBehavior.down
            };
        }

        public ImageProvider activeThumbImage {
            get { return _activeThumbImage; }
            set {
                if (value == _activeThumbImage) {
                    return;
                }

                _activeThumbImage = value;
                markNeedsPaint();
            }
        }

        ImageProvider _activeThumbImage;

        public ImageErrorListener onActiveThumbImageError {
            get { return _onActiveThumbImageError; }
            set {
                if (value == _onActiveThumbImageError) {
                    return;
                }

                _onActiveThumbImageError = value;
                markNeedsPaint();
            }
        }

        ImageErrorListener _onActiveThumbImageError;

        public ImageProvider inactiveThumbImage {
            get { return _inactiveThumbImage; }
            set {
                if (value == _inactiveThumbImage) {
                    return;
                }

                _inactiveThumbImage = value;
                markNeedsPaint();
            }
        }

        ImageProvider _inactiveThumbImage;

        public ImageErrorListener onInactiveThumbImageError {
            get { return _onInactiveThumbImageError; }
            set {
                if (value == _onInactiveThumbImageError) {
                    return;
                }

                _onInactiveThumbImageError = value;
                markNeedsPaint();
            }
        }

        ImageErrorListener _onInactiveThumbImageError;

        public Color activeTrackColor {
            get { return _activeTrackColor; }
            set {
                D.assert(value != null);
                if (value == _activeTrackColor) {
                    return;
                }

                _activeTrackColor = value;
                markNeedsPaint();
            }
        }

        Color _activeTrackColor;

        public Color inactiveTrackColor {
            get { return _inactiveTrackColor; }
            set {
                D.assert(value != null);
                if (value == _inactiveTrackColor) {
                    return;
                }

                _inactiveTrackColor = value;
                markNeedsPaint();
            }
        }

        Color _inactiveTrackColor;

        public ImageConfiguration configuration {
            get { return _configuration; }
            set {
                D.assert(value != null);
                if (value == _configuration) {
                    return;
                }

                _configuration = value;
                markNeedsPaint();
            }
        }

        ImageConfiguration _configuration;

        public TextDirection textDirection {
            get { return _textDirection; }
            set {
                if (_textDirection == value) {
                    return;
                }

                _textDirection = value;
                markNeedsPaint();
            }
        }

        TextDirection _textDirection;

        public DragStartBehavior? dragStartBehavior {
            get { return _drag.dragStartBehavior; }
            set { _drag.dragStartBehavior = value ?? DragStartBehavior.down; }
        }

        _SwitchState state;

        public override bool? value {
            get { return base.value; }
            set {
                D.assert(value != null);
                base.value = value;

                if (_needsPositionAnimation) {
                    _needsPositionAnimation = false;
                    position.curve = null;
                    position.reverseCurve = null;

                    if (value == true) {
                        positionController.forward();
                    }
                    else {
                        positionController.reverse();
                    }
                }
            }
        }

        public override void detach() {
            _cachedThumbPainter?.Dispose();
            _cachedThumbPainter = null;
            base.detach();
        }

        float _trackInnerLength {
            get { return size.width - 2.0f * material_.kRadialReactionRadius; }
        }

        HorizontalDragGestureRecognizer _drag;

        bool _needsPositionAnimation = false;

        void _handleDragStart(DragStartDetails details) {
            if (isInteractive) {
                reactionController.forward();
            }
        }

        void _handleDragUpdate(DragUpdateDetails details) {
            if (isInteractive) {
                position.curve = null;
                position.reverseCurve = null;
                float delta = details.primaryDelta.Value / _trackInnerLength;
                positionController.setValue(positionController.value + delta);
            }
        }

        void _handleDragEnd(DragEndDetails details) {
            _needsPositionAnimation = true;

            if ((position.value >= 0.5f) != value) {
                onChanged(!value);
            }

            reactionController.reverse();
            state._didFinishDragging();
        }

        public override void handleEvent(PointerEvent evt, HitTestEntry entry) {
            D.assert(debugHandleEvent(evt, entry));
            if (evt is PointerDownEvent && onChanged != null) {
                _drag.addPointer((PointerDownEvent) evt);
            }

            base.handleEvent(evt, entry);
        }

        Color _cachedThumbColor;
        ImageProvider _cachedThumbImage;
        ImageErrorListener _cachedThumbErrorListener;
        BoxPainter _cachedThumbPainter;

        BoxDecoration _createDefaultThumbDecoration(Color color, ImageProvider image,
            ImageErrorListener errorListener) {
            return new BoxDecoration(
                color: color,
                image: image == null ? null : new DecorationImage(image: image, onError: errorListener),
                shape: BoxShape.circle,
                boxShadow: material_.kElevationToShadow[1]
            );
        }

        bool _isPainting = false;

        void _handleDecorationChanged() {
            if (!_isPainting) {
                markNeedsPaint();
            }
        }

        public override void paint(PaintingContext context, Offset offset) {
            Canvas canvas = context.canvas;

            bool isEnabled = onChanged != null;
            float currentValue = position.value;

            float visualPosition = currentValue;

            Color trackColor = isEnabled
                ? Color.lerp(inactiveTrackColor, activeTrackColor, currentValue)
                : inactiveTrackColor;

            Color thumbColor = isEnabled
                ? Color.lerp(inactiveColor, activeColor, currentValue)
                : inactiveColor;

            ImageProvider thumbImage = isEnabled
                ? (currentValue < 0.5 ? inactiveThumbImage : activeThumbImage)
                : inactiveThumbImage;

            ImageErrorListener thumbErrorListener = isEnabled
                ? (currentValue < 0.5f ? onInactiveThumbImageError : onActiveThumbImageError)
                : onInactiveThumbImageError;

            // Paint the track
            Paint paint = new Paint {color = trackColor};
            float trackHorizontalPadding = material_.kRadialReactionRadius - Switch._kTrackRadius;
            Rect trackRect = Rect.fromLTWH(
                offset.dx + trackHorizontalPadding,
                offset.dy + (size.height - Switch._kTrackHeight) / 2.0f,
                size.width - 2.0f * trackHorizontalPadding,
                Switch._kTrackHeight
            );
            RRect trackRRect = RRect.fromRectAndRadius(trackRect, Radius.circular(Switch._kTrackRadius));
            canvas.drawRRect(trackRRect, paint);

            Offset thumbPosition = new Offset(
                material_.kRadialReactionRadius + visualPosition * _trackInnerLength,
                size.height / 2.0f
            );

            paintRadialReaction(canvas, offset, thumbPosition);

            try {
                _isPainting = true;
                BoxPainter thumbPainter;
                if (_cachedThumbPainter == null || thumbColor != _cachedThumbColor ||
                    thumbImage != _cachedThumbImage || thumbErrorListener != _cachedThumbErrorListener) {
                    _cachedThumbColor = thumbColor;
                    _cachedThumbImage = thumbImage;
                    _cachedThumbErrorListener = thumbErrorListener;
                    _cachedThumbPainter = _createDefaultThumbDecoration(thumbColor, thumbImage, thumbErrorListener)
                        .createBoxPainter(_handleDecorationChanged);
                }

                thumbPainter = _cachedThumbPainter;

                float inset = 1.0f - (currentValue - 0.5f).abs() * 2.0f;
                float radius = Switch._kThumbRadius - inset;
                thumbPainter.paint(
                    canvas,
                    thumbPosition + offset - new Offset(radius, radius),
                    configuration.copyWith(size: Size.fromRadius(radius))
                );
            }
            finally {
                _isPainting = false;
            }
        }
    }
}