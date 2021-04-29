using System;
using System.Collections.Generic;
using Unity.UIWidgets.animation;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.rendering;

namespace Unity.UIWidgets.widgets {
    public enum CrossFadeState {
        showFirst,
        showSecond
    }

    public delegate Widget AnimatedCrossFadeBuilder(Widget topChild, Key topChildKey, Widget bottomChild, Key bottomChildKey);


    public class AnimatedCrossFade : StatefulWidget {
        public AnimatedCrossFade(
            Key key = null,
            Widget firstChild = null,
            Widget secondChild = null,
            Curve firstCurve = null,
            Curve secondCurve = null,
            Curve sizeCurve = null,
            AlignmentGeometry alignment = null,
            CrossFadeState? crossFadeState = null,
            TimeSpan? duration = null,
            TimeSpan? reverseDuration = null,
            AnimatedCrossFadeBuilder layoutBuilder = null
        ) : base(key: key) {
            D.assert(firstChild != null);
            D.assert(secondChild != null);
            D.assert(crossFadeState != null);
            D.assert(duration != null);
            this.firstChild = firstChild;
            this.secondChild = secondChild;
            this.firstCurve = firstCurve ?? Curves.linear;
            this.secondCurve = secondCurve ?? Curves.linear;
            this.sizeCurve = sizeCurve ?? Curves.linear;
            this.alignment = alignment ?? Alignment.topCenter;
            this.crossFadeState = crossFadeState ?? CrossFadeState.showFirst;
            this.duration = duration;
            this.reverseDuration = reverseDuration;
            this.layoutBuilder = layoutBuilder ?? defaultLayoutBuilder;
        }

        public readonly Widget firstChild;

        public readonly Widget secondChild;

        public readonly CrossFadeState crossFadeState;

        public readonly TimeSpan? duration;

        public readonly TimeSpan? reverseDuration;
        
        public readonly Curve firstCurve;

        public readonly Curve secondCurve;

        public readonly Curve sizeCurve;

        public readonly AlignmentGeometry alignment;

        public readonly AnimatedCrossFadeBuilder layoutBuilder;

        static Widget defaultLayoutBuilder(Widget topChild, Key topChildKey, Widget bottomChild, Key bottomChildKey) {
            return new Stack(
                overflow: Overflow.visible,
                children: new List<Widget> {
                    new Positioned(
                        key: bottomChildKey,
                        left: 0.0f,
                        top: 0.0f,
                        right: 0.0f,
                        child: bottomChild),
                    new Positioned(
                        key: topChildKey,
                        child: topChild)
                }
            );
        }

        public override State createState() {
            return new _AnimatedCrossFadeState();
        }

        public override void debugFillProperties(DiagnosticPropertiesBuilder properties) {
            base.debugFillProperties(properties);
            properties.add(new EnumProperty<CrossFadeState>("crossFadeState", crossFadeState));
            properties.add(new DiagnosticsProperty<AlignmentGeometry>("alignment", alignment,
                defaultValue: Alignment.topCenter));
            properties.add(new IntProperty("duration", duration?.Milliseconds, unit: "ms"));
            properties.add(new IntProperty("reverseDuration", reverseDuration?.Milliseconds, unit: "ms", defaultValue: null));
        }
    }


    public class _AnimatedCrossFadeState : TickerProviderStateMixin<AnimatedCrossFade> {
        
        AnimationController _controller;
        Animation<float> _firstAnimation;
        Animation<float> _secondAnimation;

        public override void initState() {
            base.initState();
            _controller = new AnimationController(
                duration: widget.duration, 
                reverseDuration: widget.reverseDuration,
                vsync: this);
            if (widget.crossFadeState == CrossFadeState.showSecond) {
                _controller.setValue(1.0f);
            }

            _firstAnimation = _initAnimation(widget.firstCurve, true);
            _secondAnimation = _initAnimation(widget.secondCurve, false);
            _controller.addStatusListener((AnimationStatus status) => { setState(() => { }); });
        }

        Animation<float> _initAnimation(Curve curve, bool inverted) {
            Animation<float> result = _controller.drive(new CurveTween(curve: curve));
            if (inverted) {
                result = result.drive(new FloatTween(begin: 1.0f, end: 0.0f));
            }

            return result;
        }

        public override void dispose() {
            _controller.dispose();
            base.dispose();
        }

        public override void didUpdateWidget(StatefulWidget oldWidget) {
            base.didUpdateWidget(oldWidget);
            AnimatedCrossFade _oldWidget = (AnimatedCrossFade) oldWidget;
            if (widget.duration != _oldWidget.duration) {
                _controller.duration = widget.duration;
            }

            if (widget.reverseDuration != _oldWidget.reverseDuration)
                _controller.reverseDuration = widget.reverseDuration;
            
            if (widget.firstCurve != _oldWidget.firstCurve) {
                _firstAnimation = _initAnimation(widget.firstCurve, true);
            }

            if (widget.secondCurve != _oldWidget.secondCurve) {
                _secondAnimation = _initAnimation(widget.secondCurve, false);
            }

            if (widget.crossFadeState != _oldWidget.crossFadeState) {
                switch (widget.crossFadeState) {
                    case CrossFadeState.showFirst:
                        _controller.reverse();
                        break;
                    case CrossFadeState.showSecond:
                        _controller.forward();
                        break;
                }
            }
        }

        bool _isTransitioning {
            get {
                return _controller.status == AnimationStatus.forward ||
                       _controller.status == AnimationStatus.reverse;
            }
        }

        public override Widget build(BuildContext context) {
            Key kFirstChildKey = new ValueKey<CrossFadeState>(CrossFadeState.showFirst);
            Key kSecondChildKey = new ValueKey<CrossFadeState>(CrossFadeState.showSecond);
            bool transitioningForwards = _controller.status == AnimationStatus.completed || _controller.status == AnimationStatus.forward;

            Key topKey;
            Widget topChild;
            Animation<float> topAnimation;
            Key bottomKey;
            Widget bottomChild;
            Animation<float> bottomAnimation;
            
            if (transitioningForwards) {
                topKey = kSecondChildKey;
                topChild = widget.secondChild;
                topAnimation = _secondAnimation;
                bottomKey = kFirstChildKey;
                bottomChild = widget.firstChild;
                bottomAnimation = _firstAnimation;
            }
            else {
                topKey = kFirstChildKey;
                topChild = widget.firstChild;
                topAnimation = _firstAnimation;
                bottomKey = kSecondChildKey;
                bottomChild = widget.secondChild;
                bottomAnimation = _secondAnimation;
            }

            bottomChild = new TickerMode(
                key: bottomKey,
                enabled: _isTransitioning,
                child: new FadeTransition(
                    opacity: bottomAnimation,
                    child: bottomChild
                )
            );

            topChild = new TickerMode(
                key: topKey,
                enabled: true,
                child: new FadeTransition(
                    opacity: topAnimation,
                    child: topChild
                )
            );

            return new ClipRect(
                child: new AnimatedSize(
                    alignment: widget.alignment,
                    duration: widget.duration,
                    reverseDuration: widget.reverseDuration,
                    curve: widget.sizeCurve,
                    vsync: this,
                    child: widget.layoutBuilder(topChild, topKey, bottomChild, bottomKey)
                )
            );
        }


        public override void debugFillProperties(DiagnosticPropertiesBuilder description) {
            base.debugFillProperties(description);
            description.add(new EnumProperty<CrossFadeState>("crossFadeState", widget.crossFadeState));
            description.add(
                new DiagnosticsProperty<AnimationController>("controller", _controller, showName: false));
            description.add(new DiagnosticsProperty<AlignmentGeometry>("alignment", widget.alignment,
                defaultValue: Alignment.topCenter));
        }
    }
}