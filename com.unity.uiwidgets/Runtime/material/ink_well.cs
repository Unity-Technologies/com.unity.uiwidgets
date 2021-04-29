using System;
using System.Collections.Generic;
using System.Linq;
using uiwidgets;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.gestures;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.rendering;
using Unity.UIWidgets.ui;
using Unity.UIWidgets.widgets;

namespace Unity.UIWidgets.material {
    public abstract class InteractiveInkFeature : InkFeature {
        public InteractiveInkFeature(
            MaterialInkController controller = null,
            RenderBox referenceBox = null,
            Color color = null,
            VoidCallback onRemoved = null
        ) : base(controller: controller, referenceBox: referenceBox, onRemoved: onRemoved) {
            D.assert(controller != null);
            D.assert(referenceBox != null);
            _color = color;
        }

        public virtual void confirm() {
        }

        public virtual void cancel() {
        }

        public Color color {
            get { return _color; }
            set {
                if (value == _color) {
                    return;
                }

                _color = value;
                controller.markNeedsPaint();
            }
        }

        Color _color;

        protected void paintInkCircle(
            Canvas canvas,
            Matrix4 transform,
            Paint paint,
            Offset center,
            float radius,
            TextDirection? textDirection = null,
            ShapeBorder customBorder = null,
            BorderRadius borderRadius = null,
            RectCallback clipCallback = null) {
            borderRadius = borderRadius ?? BorderRadius.zero;
            D.assert(canvas != null);
            D.assert(transform != null);
            D.assert(paint != null);
            D.assert(center != null);
            D.assert(borderRadius != null);

            Offset originOffset = MatrixUtils.getAsTranslation(transform);
            canvas.save();
            if (originOffset == null) {
                canvas.transform(transform.storage);
            }
            else {
                canvas.translate(originOffset.dx, originOffset.dy);
            }

            if (clipCallback != null) {
                Rect rect = clipCallback();
                if (customBorder != null) {
                    canvas.clipPath(customBorder.getOuterPath(rect, textDirection: textDirection));
                }
                else if (borderRadius != BorderRadius.zero) {
                    canvas.clipRRect(RRect.fromRectAndCorners(
                        rect,
                        topLeft: borderRadius.topLeft, topRight: borderRadius.topRight,
                        bottomLeft: borderRadius.bottomLeft, bottomRight: borderRadius.bottomRight
                    ));
                }
                else {
                    canvas.clipRect(rect);
                }
            }

            canvas.drawCircle(center, radius, paint);
            canvas.restore();
        }
    }

    public abstract class InteractiveInkFeatureFactory {
        public InteractiveInkFeatureFactory() {
        }

        public abstract InteractiveInkFeature create(
            MaterialInkController controller = null,
            RenderBox referenceBox = null,
            Offset position = null,
            Color color = null,
            TextDirection? textDirection = null,
            bool containedInkWell = false,
            RectCallback rectCallback = null,
            BorderRadius borderRadius = null,
            ShapeBorder customBorder = null,
            float? radius = null,
            VoidCallback onRemoved = null);
    }


    public class InkResponse : StatefulWidget {
        public InkResponse(
            Key key = null,
            Widget child = null,
            GestureTapCallback onTap = null,
            GestureTapDownCallback onTapDown = null,
            GestureTapCancelCallback onTapCancel = null,
            GestureTapCallback onDoubleTap = null,
            GestureLongPressCallback onLongPress = null,
            ValueChanged<bool> onHighlightChanged = null,
            ValueChanged<bool> onHover = null,
            bool containedInkWell = false,
            BoxShape highlightShape = BoxShape.circle,
            float? radius = null,
            BorderRadius borderRadius = null,
            ShapeBorder customBorder = null,
            Color focusColor = null,
            Color hoverColor = null,
            Color highlightColor = null,
            Color splashColor = null,
            InteractiveInkFeatureFactory splashFactory = null,
            bool enableFeedback = true,
            FocusNode focusNode = null,
            bool canRequestFocus = true,
            ValueChanged<bool> onFocusChange = null,
            bool autofocus = false) : base(key: key) {
            this.child = child;
            this.onTap = onTap;
            this.onTapDown = onTapDown;
            this.onTapCancel = onTapCancel;
            this.onDoubleTap = onDoubleTap;
            this.onLongPress = onLongPress;
            this.onHighlightChanged = onHighlightChanged;
            this.onHover = onHover;
            this.containedInkWell = containedInkWell;
            this.highlightShape = highlightShape;
            this.radius = radius;
            this.borderRadius = borderRadius;
            this.customBorder = customBorder;
            this.focusColor = focusColor;
            this.hoverColor = hoverColor;
            this.highlightColor = highlightColor;
            this.splashColor = splashColor;
            this.splashFactory = splashFactory;
            this.enableFeedback = enableFeedback;
            this.focusNode = focusNode;
            this.canRequestFocus = canRequestFocus;
            this.onFocusChange = onFocusChange;
            this.autofocus = autofocus;
        }

        public readonly Widget child;

        public readonly GestureTapCallback onTap;

        public readonly GestureTapDownCallback onTapDown;

        public readonly GestureTapCancelCallback onTapCancel;

        public readonly GestureTapCallback onDoubleTap;

        public readonly GestureLongPressCallback onLongPress;

        public readonly ValueChanged<bool> onHighlightChanged;

        public readonly ValueChanged<bool> onHover;

        public readonly bool containedInkWell;

        public readonly BoxShape highlightShape;

        public readonly float? radius;

        public readonly BorderRadius borderRadius;

        public readonly ShapeBorder customBorder;

        public readonly Color focusColor;

        public readonly Color hoverColor;

        public readonly Color highlightColor;

        public readonly Color splashColor;

        public readonly InteractiveInkFeatureFactory splashFactory;

        public readonly bool enableFeedback;

        public readonly ValueChanged<bool> onFocusChange;

        public readonly bool autofocus;

        public readonly FocusNode focusNode;

        public readonly bool canRequestFocus;

        public virtual RectCallback getRectCallback(RenderBox referenceBox) {
            return null;
        }


        public virtual bool debugCheckContext(BuildContext context) {
            D.assert(material_.debugCheckHasMaterial(context));
            return true;
        }

        public override State createState() {
            return new _InkResponseState<InkResponse>();
        }


        public override void debugFillProperties(DiagnosticPropertiesBuilder properties) {
            base.debugFillProperties(properties);
            List<string> gestures = new List<string>();
            if (onTap != null) {
                gestures.Add("tap");
            }

            if (onDoubleTap != null) {
                gestures.Add("double tap");
            }

            if (onLongPress != null) {
                gestures.Add("long press");
            }

            if (onTapDown != null) {
                gestures.Add("tap down");
            }

            if (onTapCancel != null) {
                gestures.Add("tap cancel");
            }

            properties.add(new EnumerableProperty<string>("gestures", gestures, ifEmpty: "<none>"));
            properties.add(new DiagnosticsProperty<bool>("containedInkWell", containedInkWell,
                level: DiagnosticLevel.fine));
            properties.add(new DiagnosticsProperty<BoxShape>(
                "highlightShape",
                highlightShape,
                description: (containedInkWell ? "clipped to" : "") + highlightShape,
                showName: false
            ));
        }
    }

    public enum _HighlightType {
        pressed,
        hover,
        focus
    }

    public class _InkResponseState<T> : AutomaticKeepAliveClientMixin<T> where T : InkResponse {
        HashSet<InteractiveInkFeature> _splashes;
        InteractiveInkFeature _currentSplash;

        bool _hovering = false;
        readonly Dictionary<_HighlightType, InkHighlight> _highlights = new Dictionary<_HighlightType, InkHighlight>();
        Dictionary<LocalKey, ActionFactory> _actionMap;

        bool highlightsExist => _highlights.Values.Count(highlight => highlight != null) != 0;

        void _handleAction(FocusNode node, Intent intent) {
            _startSplash(context: node.context);
            _handleTap(node.context);
        }

        UiWidgetAction _createAction() {
            return new CallbackAction(
                ActivateAction.key,
                onInvoke: _handleAction
            );
        }

        public override void initState() {
            base.initState();
            _actionMap = new Dictionary<LocalKey, ActionFactory>();
            _actionMap[ActivateAction.key] = _createAction;
            FocusManager.instance.addHighlightModeListener(_handleFocusHighlightModeChange);
        }

        public override void didUpdateWidget(StatefulWidget oldWidget) {
            var _oldWidget = (InkResponse) oldWidget;
            base.didUpdateWidget(oldWidget);
            if (_isWidgetEnabled(widget) != _isWidgetEnabled(_oldWidget)) {
                _handleHoverChange(_hovering);
                _updateFocusHighlights();
            }
        }

        public override void dispose() {
            FocusManager.instance.removeHighlightModeListener(_handleFocusHighlightModeChange);
            base.dispose();
        }

        protected override bool wantKeepAlive {
            get { return highlightsExist || (_splashes != null && _splashes.isNotEmpty()); }
        }

        Color getHighlightColorForType(_HighlightType type) {
            switch (type) {
                case _HighlightType.pressed:
                    return widget.highlightColor ?? Theme.of(context).highlightColor;
                case _HighlightType.focus:
                    return widget.focusColor ?? Theme.of(context).focusColor;
                case _HighlightType.hover:
                    return widget.hoverColor ?? Theme.of(context).hoverColor;
            }

            D.assert(false, () => $"Unhandled {typeof(_HighlightType)} {type}");
            return null;
        }

        TimeSpan getFadeDurationForType(_HighlightType type) {
            switch (type) {
                case _HighlightType.pressed:
                    return new TimeSpan(0, 0, 0, 0, 200);
                case _HighlightType.hover:
                case _HighlightType.focus:
                    return new TimeSpan(0, 0, 0, 0, 50);
            }

            D.assert(false, () => $"Unhandled {typeof(_HighlightType)} {type}");
            return TimeSpan.Zero;
        }

        public void updateHighlight(_HighlightType type, bool value) {
            InkHighlight highlight = _highlights.getOrDefault(type);

            void handleInkRemoval() {
                D.assert(_highlights.getOrDefault(type) != null);
                _highlights[type] = null;
                updateKeepAlive();
            }

            if (value == (highlight != null && highlight.active)) {
                return;
            }

            if (value) {
                if (highlight == null) {
                    RenderBox referenceBox = (RenderBox) context.findRenderObject();
                    _highlights[type] = new InkHighlight(
                        controller: Material.of(context),
                        referenceBox: referenceBox,
                        color: getHighlightColorForType(type),
                        shape: widget.highlightShape,
                        borderRadius: widget.borderRadius,
                        customBorder: widget.customBorder,
                        rectCallback: widget.getRectCallback(referenceBox),
                        onRemoved: handleInkRemoval,
                        textDirection: Directionality.of(context),
                        fadeDuration: getFadeDurationForType(type)
                    );
                    updateKeepAlive();
                }
                else {
                    highlight.activate();
                }
            }
            else {
                highlight.deactivate();
            }

            D.assert(value == (_highlights.getOrDefault(type) != null && _highlights[type].active));
            switch (type) {
                case _HighlightType.pressed: {
                    if (widget.onHighlightChanged != null)
                        widget.onHighlightChanged(value);
                    break;
                }
                case _HighlightType.hover: {
                    if (widget.onHover != null)
                        widget.onHover(value);
                    break;
                }
                case _HighlightType.focus:
                    break;
            }
        }

        InteractiveInkFeature _createInkFeature(Offset globalPosition) {
            MaterialInkController inkController = Material.of(context);
            RenderBox referenceBox = context.findRenderObject() as RenderBox;
            Offset position = referenceBox.globalToLocal(globalPosition);
            Color color = widget.splashColor ?? Theme.of(context).splashColor;
            RectCallback rectCallback = widget.containedInkWell ? widget.getRectCallback(referenceBox) : null;
            BorderRadius borderRadius = widget.borderRadius;
            ShapeBorder customBorder = widget.customBorder;

            InteractiveInkFeature splash = null;

            void OnRemoved() {
                if (_splashes != null) {
                    D.assert(_splashes.Contains(splash));
                    _splashes.Remove(splash);
                    if (_currentSplash == splash) {
                        _currentSplash = null;
                    }

                    updateKeepAlive();
                }
            }

            splash = (widget.splashFactory ?? Theme.of(context).splashFactory).create(
                controller: inkController,
                referenceBox: referenceBox,
                position: position,
                color: color,
                containedInkWell: widget.containedInkWell,
                rectCallback: rectCallback,
                radius: widget.radius,
                borderRadius: borderRadius,
                customBorder: customBorder,
                onRemoved: OnRemoved,
                textDirection: Directionality.of(context));

            return splash;
        }

        void _handleFocusHighlightModeChange(FocusHighlightMode mode) {
            if (!mounted) {
                return;
            }

            setState(() => { _updateFocusHighlights(); });
        }

        void _updateFocusHighlights() {
            bool showFocus = false;
            switch (FocusManager.instance.highlightMode) {
                case FocusHighlightMode.touch: {
                    showFocus = false;
                    break;
                }
                case FocusHighlightMode.traditional: {
                    showFocus = enabled && _hasFocus;
                    break;
                }
            }

            updateHighlight(_HighlightType.focus, value: showFocus);
        }

        bool _hasFocus = false;

        void _handleFocusUpdate(bool hasFocus) {
            _hasFocus = hasFocus;
            _updateFocusHighlights();
            if (widget.onFocusChange != null) {
                widget.onFocusChange(hasFocus);
            }
        }


        void _handleTapDown(TapDownDetails details) {
            _startSplash(details: details);
            if (widget.onTapDown != null) {
                widget.onTapDown(details);
            }
        }

        void _startSplash(TapDownDetails details = null, BuildContext context = null) {
            D.assert(details != null || context != null);

            Offset globalPosition;
            if (context != null) {
                RenderBox referenceBox = context.findRenderObject() as RenderBox;
                D.assert(referenceBox.hasSize, () => "InkResponse must be done with layout before starting a splash.");
                globalPosition = referenceBox.localToGlobal(referenceBox.paintBounds.center);
            }
            else {
                globalPosition = details.globalPosition;
            }

            InteractiveInkFeature splash = _createInkFeature(globalPosition);
            _splashes = _splashes ?? new HashSet<InteractiveInkFeature>();
            _splashes.Add(splash);
            _currentSplash = splash;
            updateKeepAlive();
            updateHighlight(_HighlightType.pressed, value: true);
        }

        void _handleTap(BuildContext context) {
            _currentSplash?.confirm();
            _currentSplash = null;
            updateHighlight(_HighlightType.pressed, value: false);
            if (widget.onTap != null) {
                widget.onTap();
            }
        }

        void _handleTapCancel() {
            _currentSplash?.cancel();
            _currentSplash = null;
            if (widget.onTapCancel != null) {
                widget.onTapCancel();
            }

            updateHighlight(_HighlightType.pressed, value: false);
        }

        void _handleDoubleTap() {
            _currentSplash?.confirm();
            _currentSplash = null;
            if (widget.onDoubleTap != null) {
                widget.onDoubleTap();
            }
        }

        void _handleLongPress(BuildContext context) {
            _currentSplash?.confirm();
            _currentSplash = null;
            if (widget.onLongPress != null) {
                widget.onLongPress();
            }
        }

        public override void deactivate() {
            if (_splashes != null) {
                HashSet<InteractiveInkFeature> splashes = _splashes;
                _splashes = null;
                foreach (InteractiveInkFeature splash in splashes) {
                    splash.dispose();
                }

                _currentSplash = null;
            }

            D.assert(_currentSplash == null);
            foreach (_HighlightType highlight in _highlights.Keys.ToList()) {
                _highlights[highlight]?.dispose();
                _highlights[highlight] = null;
            }

            base.deactivate();
        }

        bool _isWidgetEnabled(InkResponse widget) {
            return widget.onTap != null || widget.onDoubleTap != null || widget.onLongPress != null;
        }

        bool enabled {
            get { return _isWidgetEnabled(widget); }
        }

        void _handleMouseEnter(PointerEnterEvent Event) {
            _handleHoverChange(true);
        }

        void _handleMouseExit(PointerExitEvent Event) {
            _handleHoverChange(false);
        }

        void _handleHoverChange(bool hovering) {
            if (_hovering != hovering) {
                _hovering = hovering;
                updateHighlight(_HighlightType.hover, value: enabled && _hovering);
            }
        }

        public override Widget build(BuildContext context) {
            D.assert(widget.debugCheckContext(context));
            base.build(context);
            foreach (_HighlightType type in _highlights.Keys) {
                if (_highlights[type] != null) {
                    _highlights[type].color = getHighlightColorForType(type);
                }
            }

            if (_currentSplash != null) {
                _currentSplash.color = widget.splashColor ?? Theme.of(context).splashColor;
            }

            bool canRequestFocus = enabled && widget.canRequestFocus;

            return new Actions(
                actions: _actionMap,
                child: new Focus(
                    focusNode: widget.focusNode,
                    canRequestFocus: canRequestFocus,
                    onFocusChange: _handleFocusUpdate,
                    autofocus: widget.autofocus,
                    child: new MouseRegion(
                        onEnter: enabled ? _handleMouseEnter : (PointerEnterEventListener) null,
                        onExit: enabled ? _handleMouseExit : (PointerExitEventListener) null,
                        child: new GestureDetector(
                            onTapDown: enabled ? _handleTapDown : (GestureTapDownCallback) null,
                            onTap: enabled ? () => _handleTap(context) : (GestureTapCallback) null,
                            onTapCancel: enabled ? _handleTapCancel : (GestureTapCancelCallback) null,
                            onDoubleTap: widget.onDoubleTap != null
                                ? _handleDoubleTap
                                : (GestureDoubleTapCallback) null,
                            onLongPress: widget.onLongPress != null
                                ? () => _handleLongPress(context)
                                : (GestureLongPressCallback) null,
                            behavior: HitTestBehavior.opaque,
                            child: widget.child
                        )
                    )
                )
            );
        }
    }


    public class InkWell : InkResponse {
        public InkWell(
            Key key = null,
            Widget child = null,
            GestureTapCallback onTap = null,
            GestureTapCallback onDoubleTap = null,
            GestureLongPressCallback onLongPress = null,
            GestureTapDownCallback onTapDown = null,
            GestureTapCancelCallback onTapCancel = null,
            ValueChanged<bool> onHighlightChanged = null,
            ValueChanged<bool> onHover = null,
            Color focusColor = null,
            Color hoverColor = null,
            Color highlightColor = null,
            Color splashColor = null,
            InteractiveInkFeatureFactory splashFactory = null,
            float? radius = null,
            BorderRadius borderRadius = null,
            ShapeBorder customBorder = null,
            bool enableFeedback = true,
            FocusNode focusNode = null,
            bool canRequestFocus = true,
            ValueChanged<bool> onFocusChange = null,
            bool autofocus = false
        ) : base(
            key: key,
            child: child,
            onTap: onTap,
            onDoubleTap: onDoubleTap,
            onLongPress: onLongPress,
            onTapDown: onTapDown,
            onTapCancel: () => {
                if (onTapCancel != null) {
                    onTapCancel();
                }
            },
            onHighlightChanged: onHighlightChanged,
            onHover: onHover,
            containedInkWell: true,
            highlightShape: BoxShape.rectangle,
            focusColor: focusColor,
            hoverColor: hoverColor,
            highlightColor: highlightColor,
            splashColor: splashColor,
            splashFactory: splashFactory,
            radius: radius,
            borderRadius: borderRadius,
            customBorder: customBorder,
            enableFeedback: enableFeedback,
            focusNode: focusNode,
            canRequestFocus: canRequestFocus,
            onFocusChange: onFocusChange,
            autofocus: autofocus) {
        }
    }
}