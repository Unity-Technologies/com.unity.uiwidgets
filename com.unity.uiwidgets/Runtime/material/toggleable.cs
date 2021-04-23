using System;
using Unity.UIWidgets.animation;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.gestures;
using Unity.UIWidgets.rendering;
using Unity.UIWidgets.scheduler;
using Unity.UIWidgets.ui;

namespace Unity.UIWidgets.material {
    static class ToggleableUtils {
        public static readonly TimeSpan _kToggleDuration = new TimeSpan(0, 0, 0, 0, 200);

        public static readonly Animatable<float> _kRadialReactionRadiusTween =
            new FloatTween(begin: 0.0f, end: material_.kRadialReactionRadius);
        
        public static readonly TimeSpan _kReactionFadeDuration = new TimeSpan(0, 0, 0, 0, 50);
    }

    public abstract class RenderToggleable : RenderConstrainedBox {
        protected RenderToggleable(
            bool? value = null,
            bool tristate = false,
            Color activeColor = null,
            Color inactiveColor = null,
            Color hoverColor = null,
            Color focusColor = null,
            ValueChanged<bool?> onChanged = null,
            BoxConstraints additionalConstraints = null,
            TickerProvider vsync = null,
            bool hasFocus = false,
            bool hovering = false
        ) : base(additionalConstraints: additionalConstraints) {
            D.assert(tristate || value != null);
            D.assert(activeColor != null);
            D.assert(inactiveColor != null);
            D.assert(vsync != null);
            _value = value;
            _tristate = tristate;
            _activeColor = activeColor;
            _inactiveColor = inactiveColor;
            _hoverColor = hoverColor ?? activeColor.withAlpha(material_.kRadialReactionAlpha);
            _focusColor = focusColor ?? activeColor.withAlpha(material_.kRadialReactionAlpha);
            _onChanged = onChanged;
            _hasFocus = hasFocus;
            _hovering = hovering;
            _vsync = vsync;

            _tap = new TapGestureRecognizer {
                onTapDown = _handleTapDown,
                onTap = _handleTap,
                onTapUp = _handleTapUp,
                onTapCancel = _handleTapCancel
            };

            _positionController = new AnimationController(
                duration: ToggleableUtils._kToggleDuration,
                value: value == false ? 0.0f : 1.0f,
                vsync: vsync);

            _position = new CurvedAnimation(
                parent: _positionController,
                curve: Curves.linear);
            _position.addListener(markNeedsPaint);

            _reactionController = new AnimationController(
                duration: material_.kRadialReactionDuration,
                vsync: vsync);
            _reaction = new CurvedAnimation(
                parent: _reactionController,
                curve: Curves.fastOutSlowIn);
            _reaction.addListener(markNeedsPaint);
            
            _reactionHoverFadeController = new AnimationController(
                duration: ToggleableUtils._kReactionFadeDuration,
                value: hovering || hasFocus ? 1.0f : 0.0f,
                vsync: vsync
            );
            _reactionHoverFade = new CurvedAnimation(
                parent: _reactionHoverFadeController,
                curve: Curves.fastOutSlowIn
            );
            _reactionHoverFade.addListener(markNeedsPaint);
            
            _reactionFocusFadeController = new AnimationController(
                duration: ToggleableUtils._kReactionFadeDuration,
                value: hovering || hasFocus ? 1.0f : 0.0f,
                vsync: vsync
            );
            _reactionFocusFade = new CurvedAnimation(
                parent: _reactionFocusFadeController,
                curve: Curves.fastOutSlowIn
            );
            _reactionFocusFade.addListener(markNeedsPaint);
        }

        protected AnimationController positionController {
            get { return _positionController; }
        }

        AnimationController _positionController;

        public CurvedAnimation position {
            get { return _position; }
        }

        CurvedAnimation _position;

        protected AnimationController reactionController {
            get { return _reactionController; }
        }

        AnimationController _reactionController;
        Animation<float> _reaction;

        protected AnimationController reactionFocusFadeController {
            get { return _reactionFocusFadeController; }
        }
        
        AnimationController _reactionFocusFadeController;
        Animation<float> _reactionFocusFade;

        protected AnimationController reactionHoverFadeController {
            get { return _reactionHoverFadeController; }
        }
        
        AnimationController _reactionHoverFadeController;
        Animation<float> _reactionHoverFade;

        public bool hasFocus {
            get { return _hasFocus; }
            set {
                if (value == _hasFocus) {
                    return;
                }

                _hasFocus = value;
                if (_hasFocus) {
                    _reactionFocusFadeController.forward();
                }
                else {
                    _reactionFocusFadeController.reverse();
                }
                markNeedsPaint();
            }
        }

        bool _hasFocus;

        public bool hovering {
            get { return _hovering; }
            set {
                if (value == _hovering) {
                    return;
                }

                _hovering = value;
                if (_hovering) {
                    _reactionHoverFadeController.forward();
                }
                else {
                    _reactionHoverFadeController.reverse();
                }
                markNeedsPaint();
            }
        }
        bool _hovering;

        public TickerProvider vsync {
            get { return _vsync; }
            set {
                D.assert(value != null);
                if (value == _vsync) {
                    return;
                }

                _vsync = value;
                positionController.resync(vsync);
                reactionController.resync(vsync);
            }
        }

        TickerProvider _vsync;

        public virtual bool? value {
            get { return _value; }
            set {
                D.assert(tristate || value != null);
                if (value == _value) {
                    return;
                }

                _value = value;
                _position.curve = Curves.easeIn;
                _position.reverseCurve = Curves.easeOut;
                if (tristate) {
                    switch (_positionController.status) {
                        case AnimationStatus.forward:
                        case AnimationStatus.completed: {
                            _positionController.reverse();
                            break;
                        }
                        default: {
                            _positionController.forward();
                            break;
                        }
                    }
                }
                else {
                    if (value == true) {
                        _positionController.forward();
                    }
                    else {
                        _positionController.reverse();
                    }
                }
            }
        }

        bool? _value;

        public bool tristate {
            get { return _tristate; }
            set {
                if (value == _tristate) {
                    return;
                }

                _tristate = value;
            }
        }

        bool _tristate;

        public Color activeColor {
            get { return _activeColor; }
            set {
                D.assert(value != null);
                if (value == _activeColor) {
                    return;
                }

                _activeColor = value;
                markNeedsPaint();
            }
        }

        Color _activeColor;

        public Color inactiveColor {
            get { return _inactiveColor; }
            set {
                D.assert(value != null);
                if (value == _inactiveColor) {
                    return;
                }

                _inactiveColor = value;
                markNeedsPaint();
            }
        }

        Color _inactiveColor;

        public ValueChanged<bool?> onChanged {
            get { return _onChanged; }
            set {
                if (value == _onChanged) {
                    return;
                }

                bool wasInteractive = isInteractive;
                _onChanged = value;
                if (wasInteractive != isInteractive) {
                    markNeedsPaint();
                }
            }
        }

        public Color hoverColor {
            get { return _hoverColor; }
            set {
                if (value == _hoverColor) {
                    return;
                }

                _hoverColor = value;
                markNeedsPaint();
            }
        }
        Color _hoverColor;

        public Color focusColor {
            get { return _focusColor; }
            set {
                if (value == _focusColor) {
                    return;
                }

                _focusColor = value;
                markNeedsPaint();
            }
        }
        
        Color _focusColor;

        public Color reactionColor {
            get { return _reactionColor; }
            set {
                if (value == _reactionColor) {
                    return;
                }

                _reactionColor = value;
                markNeedsPaint();
            }
        }
        Color _reactionColor;
        
        ValueChanged<bool?> _onChanged;

        public bool isInteractive {
            get { return onChanged != null; }
        }

        TapGestureRecognizer _tap;
        Offset _downPosition;

        public override void attach(object owner) {
            base.attach(owner);
            if (value == false) {
                _positionController.reverse();
            }
            else {
                _positionController.forward();
            }

            if (isInteractive) {
                switch (_reactionController.status) {
                    case AnimationStatus.forward: {
                        _reactionController.forward();
                        break;
                    }
                    case AnimationStatus.reverse: {
                        _reactionController.reverse();
                        break;
                    }
                    case AnimationStatus.dismissed:
                    case AnimationStatus.completed: {
                        break;
                    }
                }
            }
        }

        public override void detach() {
            _positionController.stop();
            _reactionController.stop();
            base.detach();
        }

        void _handleTapDown(TapDownDetails details) {
            if (isInteractive) {
                _downPosition = globalToLocal(details.globalPosition);
                _reactionController.forward();
            }
        }

        void _handleTap() {
            if (!isInteractive) {
                return;
            }

            switch (value) {
                case false:
                    onChanged(true);
                    break;
                case true:
                    onChanged(tristate ? (bool?) null : false);
                    break;
                default:
                    onChanged(false);
                    break;
            }
        }

        void _handleTapUp(TapUpDetails details) {
            _downPosition = null;
            if (isInteractive) {
                _reactionController.reverse();
            }
        }

        void _handleTapCancel() {
            _downPosition = null;
            if (isInteractive) {
                _reactionController.reverse();
            }
        }

        protected override bool hitTestSelf(Offset position) {
            return true;
        }

        public override void handleEvent(PointerEvent pEvent, HitTestEntry entry) {
            D.assert(debugHandleEvent(pEvent, entry));
            if (pEvent is PointerDownEvent && isInteractive) {
                _tap.addPointer((PointerDownEvent) pEvent);
            }
        }

        public void paintRadialReaction(Canvas canvas, Offset offset, Offset origin) {
            if (!_reaction.isDismissed || !_reactionFocusFade.isDismissed || !_reactionHoverFade.isDismissed) {
                Paint reactionPaint = new Paint();
                reactionPaint.color = Color.lerp(
                    Color.lerp(activeColor.withAlpha(material_.kRadialReactionAlpha), hoverColor,
                        _reactionHoverFade.value),
                    focusColor,
                    _reactionFocusFade.value);
                Offset center = Offset.lerp(_downPosition ?? origin, origin, _reaction.value);
                float reactionRadius = hasFocus || hovering
                    ? material_.kRadialReactionRadius
                    : ToggleableUtils._kRadialReactionRadiusTween.evaluate(_reaction);

                if (reactionRadius > 0.0f) {
                    canvas.drawCircle(center + offset, reactionRadius, reactionPaint);
                }
            }
        }

        public override void debugFillProperties(DiagnosticPropertiesBuilder properties) {
            base.debugFillProperties(properties);
            properties.add(new FlagProperty("value", value: value, ifTrue: "checked", ifFalse: "unchecked",
                showName: true));
            properties.add(new FlagProperty("isInteractive", value: isInteractive, ifTrue: "enabled",
                ifFalse: "disabled", defaultValue: true));
        }
    }
}