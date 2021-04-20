using System;
using Unity.UIWidgets.animation;
using Unity.UIWidgets.async;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.rendering;
using Unity.UIWidgets.ui;
using UnityEngine;
using Path = System.IO.Path;

namespace Unity.UIWidgets.widgets {
    public class Lottie : StatefulWidget {
        public Skottie _skottie = null;
        public float _frame = 0;

        public Lottie(string path, float frame) {
            D.assert(path != null);
            _skottie = new Skottie(Path.Combine(Application.streamingAssetsPath, path));
            _frame = frame;
        }

        public override State createState() {
            return new LottieState();
        }
    }

    public class LottieState : State<Lottie> {
        public override Widget build(BuildContext context) {
            return new LottieRenderObjectWidget(widget._skottie, widget._frame);
        }
    }

    public class LottieRenderObjectWidget : LeafRenderObjectWidget {
        Skottie _anime;
        float _frame;
        float _duration;

        public override void updateRenderObject(BuildContext context, RenderObject renderObject) {
            base.updateRenderObject(context, renderObject);
            var a = (RenderLottie) renderObject;
            a.frame = _frame*_duration;
        }

        public LottieRenderObjectWidget(Skottie anime, float frame) {
            _anime = anime;
            _frame = frame;
            _duration = anime.duration();
        }

        public override RenderObject createRenderObject(BuildContext context) {
            return new RenderLottie(_anime, 100, 100, frame: _frame);
        }
    }

    public class AnimatedLottie : ImplicitlyAnimatedWidget {
        public Skottie _skottie = null;
        public float _frame = 0;

        public AnimatedLottie(
            string path,
            Key key = null,
            Curve curve = null,
            TimeSpan? duration = null,
            float frame = 0
        ) :base(key: key, curve: curve, duration: duration){
            _skottie = new Skottie(Path.Combine(Application.streamingAssetsPath, path));
            _frame = frame;
        }

        AnimatedLottie(
            Skottie skottie,
            Key key = null,
            Curve curve = null,
            TimeSpan? duration = null,
            float frame = 0
        ) :base(key: key, curve: curve, duration: duration){
            _skottie = skottie;
            _frame = frame;
        }

        public static AnimatedLottie file(string path, Key key = null, Curve curve = null, TimeSpan? duration = null,
            float frame = 0) {
            var skottie = new Skottie(Path.Combine(Application.streamingAssetsPath, path));
            duration = duration ?? TimeSpan.FromSeconds(skottie.duration());
            return new AnimatedLottie(skottie, key, curve, duration, frame);
        }

        public override State createState() {
            return new _AnimatedLottieState();
        }
    }

    class _AnimatedLottieState : AnimatedWidgetBaseState<AnimatedLottie> {
        FloatTween frame;

        protected override void forEachTween(TweenVisitor visitor) {
            frame = (FloatTween) visitor.visit(this, frame, widget._frame,
                (value) => new FloatTween(begin: value, value));
        }

        public override Widget build(BuildContext context) {
            return new LottieRenderObjectWidget(widget._skottie, frame.lerp(animation.value));
        }
    }
}