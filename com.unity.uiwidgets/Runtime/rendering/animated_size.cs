using System;
using Unity.UIWidgets.animation;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.scheduler;
using Unity.UIWidgets.ui;

namespace Unity.UIWidgets.rendering {
    public enum RenderAnimatedSizeState {
        start,
        stable,
        changed,
        unstable
    }

    public class RenderAnimatedSize : RenderAligningShiftedBox {
        public RenderAnimatedSize(
            TickerProvider vsync = null,
            TimeSpan? duration = null,
            TimeSpan? reverseDuration = null,
            Curve curve = null,
            AlignmentGeometry alignment = null,
            TextDirection? textDirection = null,
            RenderBox child = null
        ) : base(child: child, alignment: alignment ?? Alignment.center, textDirection: textDirection) {
            curve = curve ?? Curves.linear;
            D.assert(vsync != null);
            D.assert(duration != null);
            _vsync = vsync;
            _controller = new AnimationController(
                vsync: this.vsync,
                duration: duration,
                reverseDuration: reverseDuration);
            _controller.addListener(() => {
                if (_controller.value != _lastValue) {
                    markNeedsLayout();
                }
            });
            _animation = new CurvedAnimation(
                parent: _controller,
                curve: curve);
        }

        AnimationController _controller;
        CurvedAnimation _animation;
        readonly SizeTween _sizeTween = new SizeTween();
        bool _hasVisualOverflow;
        float _lastValue;

        public RenderAnimatedSizeState state {
            get { return _state; }
        }

        RenderAnimatedSizeState _state = RenderAnimatedSizeState.start;

        public TimeSpan? duration {
            get { return _controller.duration; }
            set {
                D.assert(value != null);
                if (value == _controller.duration) {
                    return;
                }

                _controller.duration = value;
            }
        }

        /// The duration of the animation when running in reverse.
        public TimeSpan? reverseDuration {
            get { return _controller.reverseDuration; }
            set {
                if (value == _controller.reverseDuration) {
                    return;
                }
 
                _controller.reverseDuration = value;
            }
        }
        
        public Curve curve {
            get { return _animation.curve; }
            set {
                D.assert(value != null);
                if (value == _animation.curve) {
                    return;
                }

                _animation.curve = value;
            }
        }

        public bool isAnimating {
            get { return _controller.isAnimating; }
        }

        public TickerProvider vsync {
            get { return _vsync; }
            set {
                D.assert(value != null);
                if (value == _vsync) {
                    return;
                }

                _vsync = value;
                _controller.resync(vsync);
            }
        }

        TickerProvider _vsync;

        public override void detach() {
            _controller.stop();
            base.detach();
        }

        Size _animatedSize {
            get { return _sizeTween.evaluate(_animation); }
        }

        protected override void performLayout() {
            _lastValue = _controller.value;
            _hasVisualOverflow = false;
            BoxConstraints constraints = this.constraints;
            if (child == null || constraints.isTight) {
                _controller.stop();
                size = _sizeTween.begin = _sizeTween.end = constraints.smallest;
                _state = RenderAnimatedSizeState.start;
                child?.layout(constraints);
                return;
            }

            child.layout(constraints, parentUsesSize: true);
            
            switch (_state) {
                case RenderAnimatedSizeState.start:
                    _layoutStart();
                    break;
                case RenderAnimatedSizeState.stable:
                    _layoutStable();
                    break;
                case RenderAnimatedSizeState.changed:
                    _layoutChanged();
                    break;
                case RenderAnimatedSizeState.unstable:
                    _layoutUnstable();
                    break;
            }

            size = constraints.constrain(_animatedSize);
            alignChild();

            if (size.width < _sizeTween.end.width ||
                size.height < _sizeTween.end.height) {
                _hasVisualOverflow = true;
            }
        }

        void _restartAnimation() {
            _lastValue = 0.0f;
            _controller.forward(from: 0.0f);
        }

        void _layoutStart() {
            _sizeTween.begin = _sizeTween.end = debugAdoptSize(child.size);
            _state = RenderAnimatedSizeState.stable;
        }

        void _layoutStable() {
            if (_sizeTween.end != child.size) {
                _sizeTween.begin = size;
                _sizeTween.end = debugAdoptSize(child.size);
                _restartAnimation();
                _state = RenderAnimatedSizeState.changed;
            }
            else if (_controller.value == _controller.upperBound) {
                _sizeTween.begin = _sizeTween.end = debugAdoptSize(child.size);
            }
            else if (!_controller.isAnimating) {
                _controller.forward();
            }
        }

        void _layoutChanged() {
            if (_sizeTween.end != child.size) {
                _sizeTween.begin = _sizeTween.end = debugAdoptSize(child.size);
                _restartAnimation();
                _state = RenderAnimatedSizeState.unstable;
            }
            else {
                _state = RenderAnimatedSizeState.stable;
                if (!_controller.isAnimating) {
                    _controller.forward();
                }
            }
        }

        void _layoutUnstable() {
            if (_sizeTween.end != child.size) {
                _sizeTween.begin = _sizeTween.end = debugAdoptSize(child.size);
                _restartAnimation();
            }
            else {
                _controller.stop();
                _state = RenderAnimatedSizeState.stable;
            }
        }

        public override void paint(PaintingContext context, Offset offset) {
            if (child != null && _hasVisualOverflow) {
                Rect rect = Offset.zero & size;
                context.pushClipRect(needsCompositing, offset, rect, base.paint);
            }
            else {
                base.paint(context, offset);
            }
        }
    }
}