using System;
using Unity.UIWidgets.animation;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.ui;

namespace Unity.UIWidgets.widgets {
    public class TweenAnimationBuilder<T> : ImplicitlyAnimatedWidget {
        public TweenAnimationBuilder(
            Key key = null,
            Tween<T> tween = null,
            TimeSpan? duration = null,
            Curve curve = null,
            ValueWidgetBuilder<T> builder = null,
            VoidCallback onEnd = null,
            Widget child = null
        ) : base(key: key, duration: duration, curve: curve, onEnd: onEnd) {
            curve = curve ?? Curves.linear;
            D.assert(tween != null);
            D.assert(curve != null);
            D.assert(builder != null);
            this.tween = tween;
            this.child = child;
            this.builder = builder;
        }


        public readonly Tween<T> tween;
        public readonly ValueWidgetBuilder<T> builder;
        public readonly Widget child;
        public override State createState() {
            return new _TweenAnimationBuilderState<T>();
        }
    }
    public class _TweenAnimationBuilderState<T> : AnimatedWidgetBaseState<TweenAnimationBuilder<T>> { 
        Tween<T> _currentTween;

        public override void initState() {
            _currentTween = widget.tween;
            if (_currentTween.begin == null) {
                _currentTween.begin = _currentTween.end;
            }

            
            base.initState();
            if (!_currentTween.begin.Equals( _currentTween.end)) {
                controller.forward();
            }
        }

        protected override void forEachTween(TweenVisitor visitor) { 
            D.assert(
                widget.tween.end != null,()=> 
                    "Tween provided to TweenAnimationBuilder must have non-null Tween.end value.");
            _currentTween = visitor.visit(
                this,
                _currentTween, 
                widget.tween.end, 
                (value) =>{
                    D.assert(false);
                    return null; 
                }) as Tween<T>;
        }

        public override Widget build(BuildContext context) {
            return widget.builder(context, _currentTween.evaluate(animation), widget.child);
        }
    }

}