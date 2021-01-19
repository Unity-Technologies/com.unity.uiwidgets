using System.Collections.Generic;
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
    }

    public abstract class InteractiveInkFeatureFactory {
        public InteractiveInkFeatureFactory() {
        }

        public abstract InteractiveInkFeature create(
            MaterialInkController controller = null,
            RenderBox referenceBox = null,
            Offset position = null,
            Color color = null,
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
            bool containedInkWell = false,
            BoxShape highlightShape = BoxShape.circle,
            float? radius = null,
            BorderRadius borderRadius = null,
            ShapeBorder customBorder = null,
            Color highlightColor = null,
            Color splashColor = null,
            InteractiveInkFeatureFactory splashFactory = null) : base(key: key) {
            this.child = child;
            this.onTap = onTap;
            this.onTapDown = onTapDown;
            this.onTapCancel = onTapCancel;
            this.onDoubleTap = onDoubleTap;
            this.onLongPress = onLongPress;
            this.onHighlightChanged = onHighlightChanged;
            this.containedInkWell = containedInkWell;
            this.highlightShape = highlightShape;
            this.radius = radius;
            this.borderRadius = borderRadius;
            this.customBorder = customBorder;
            this.highlightColor = highlightColor;
            this.splashColor = splashColor;
            this.splashFactory = splashFactory;
        }

        public readonly Widget child;

        public readonly GestureTapCallback onTap;

        public readonly GestureTapDownCallback onTapDown;

        public readonly GestureTapCancelCallback onTapCancel;

        public readonly GestureTapCallback onDoubleTap;

        public readonly GestureLongPressCallback onLongPress;

        public readonly ValueChanged<bool> onHighlightChanged;

        public readonly bool containedInkWell;

        public readonly BoxShape highlightShape;

        public readonly float? radius;

        public readonly BorderRadius borderRadius;

        public readonly ShapeBorder customBorder;

        public readonly Color highlightColor;

        public readonly Color splashColor;

        public readonly InteractiveInkFeatureFactory splashFactory;

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


    public class _InkResponseState<T> : AutomaticKeepAliveClientMixin<T> where T : InkResponse {
        HashSet<InteractiveInkFeature> _splashes;
        InteractiveInkFeature _currentSplash;
        InkHighlight _lastHighlight;

        protected override bool wantKeepAlive {
            get { return _lastHighlight != null || (_splashes != null && _splashes.isNotEmpty()); }
        }

        public void updateHighlight(bool value) {
            if (value == (_lastHighlight != null && _lastHighlight.active)) {
                return;
            }

            if (value) {
                if (_lastHighlight == null) {
                    RenderBox referenceBox = (RenderBox) context.findRenderObject();
                    _lastHighlight = new InkHighlight(
                        controller: Material.of(context),
                        referenceBox: referenceBox,
                        color: widget.highlightColor ?? Theme.of(context).highlightColor,
                        shape: widget.highlightShape,
                        borderRadius: widget.borderRadius,
                        customBorder: widget.customBorder,
                        rectCallback: widget.getRectCallback(referenceBox),
                        onRemoved: _handleInkHighlightRemoval);
                    updateKeepAlive();
                }
                else {
                    _lastHighlight.activate();
                }
            }
            else {
                _lastHighlight.deactivate();
            }

            D.assert(value == (_lastHighlight != null && _lastHighlight.active));
            if (widget.onHighlightChanged != null) {
                widget.onHighlightChanged(value);
            }
        }

        void _handleInkHighlightRemoval() {
            D.assert(_lastHighlight != null);
            _lastHighlight = null;
            updateKeepAlive();
        }

        InteractiveInkFeature _createInkFeature(TapDownDetails details) {
            MaterialInkController inkController = Material.of(context);
            RenderBox referenceBox = (RenderBox) context.findRenderObject();
            Offset position = referenceBox.globalToLocal(details.globalPosition);
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
                onRemoved: OnRemoved);

            return splash;
        }


        void _handleTapDown(TapDownDetails details) {
            InteractiveInkFeature splash = _createInkFeature(details);
            _splashes = _splashes ?? new HashSet<InteractiveInkFeature>();
            _splashes.Add(splash);
            _currentSplash = splash;
            if (widget.onTapDown != null) {
                widget.onTapDown(details);
            }

            updateKeepAlive();
            updateHighlight(true);
        }

        void _handleTap(BuildContext context) {
            _currentSplash?.confirm();
            _currentSplash = null;
            updateHighlight(false);
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

            updateHighlight(false);
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
            _lastHighlight?.dispose();
            _lastHighlight = null;
            base.deactivate();
        }

        public override Widget build(BuildContext context) {
            D.assert(widget.debugCheckContext(context));
            base.build(context);
            ThemeData themeData = Theme.of(context);
            if (_lastHighlight != null) {
                _lastHighlight.color = widget.highlightColor ?? themeData.highlightColor;
            }

            if (_currentSplash != null) {
                _currentSplash.color = widget.splashColor ?? themeData.splashColor;
            }

            bool enabled = widget.onTap != null || widget.onDoubleTap != null ||
                           widget.onLongPress != null;

            return new GestureDetector(
                onTapDown: enabled ? (GestureTapDownCallback) _handleTapDown : null,
                onTap: enabled ? (GestureTapCallback) (() => _handleTap(context)) : null,
                onTapCancel: enabled ? (GestureTapCancelCallback) _handleTapCancel : null,
                onDoubleTap: widget.onDoubleTap != null
                    ? (GestureDoubleTapCallback) (details => _handleDoubleTap())
                    : null,
                onLongPress: widget.onLongPress != null
                    ? (GestureLongPressCallback) (() => _handleLongPress(context))
                    : null,
                behavior: HitTestBehavior.opaque,
                child: widget.child
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
            Color highlightColor = null,
            Color splashColor = null,
            InteractiveInkFeatureFactory splashFactory = null,
            float? radius = null,
            BorderRadius borderRadius = null,
            ShapeBorder customBorder = null
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
            containedInkWell: true,
            highlightShape: BoxShape.rectangle,
            highlightColor: highlightColor,
            splashColor: splashColor,
            splashFactory: splashFactory,
            radius: radius,
            borderRadius: borderRadius,
            customBorder: customBorder) {
        }
    }
}