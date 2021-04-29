using System;
using System.Collections.Generic;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.ui;

namespace Unity.UIWidgets.animation {
    public abstract class Animatable<T> {
        public Animatable() {
        }

        public abstract T transform(float t);


        public  T evaluate(Animation<float> animation) {
            return transform(animation.value);
        }

        public Animation<T> animate(Animation<float> parent) {
            return new _AnimatedEvaluation<T>(parent, this);
        }

        public Animatable<T> chain(Animatable<float> parent) {
            return new _ChainedEvaluation<T>(parent, this);
        }
    }

    class _AnimatedEvaluation<T> : AnimationWithParentMixin<float, T> {
        internal _AnimatedEvaluation(Animation<float> parent, Animatable<T> evaluatable) {
            _parent = parent;
            _evaluatable = evaluatable;
        }

        public override Animation<float> parent {
            get { return _parent; }
        }

        readonly Animation<float> _parent;

        readonly Animatable<T> _evaluatable;

        public override T value {
            get {
                return  _evaluatable.evaluate(parent);
            }
        }

        public override string ToString() {
            return $"{parent}\u27A9{_evaluatable}\u27A9{value}";
        }

        public override string toStringDetails() {
            return base.toStringDetails() + " " + _evaluatable;
        }
    }


    class _ChainedEvaluation<T> : Animatable<T> {
        internal _ChainedEvaluation(Animatable<float> parent, Animatable<T> evaluatable) {
            _parent = parent;
            _evaluatable = evaluatable;
        }

        readonly Animatable<float> _parent;

        readonly Animatable<T> _evaluatable;
        public override T transform(float t) {
            return _evaluatable.transform(_parent.transform(t));
        }
        
        /*public override T evaluate(Animation<float> animation) {
            float value = _parent.evaluate(animation);
            return _evaluatable.evaluate(new AlwaysStoppedAnimation<float>(value));
        }*/

        public override string ToString() {
            return $"{_parent}\u27A9{_evaluatable}";
        }
    }
    
    /**
     * We make Tween<T> a abstract class by design here (while it is not a abstract class in flutter
     * The reason to do so is, in C# we cannot use arithmetic between generic types, therefore the
     * lerp method cannot be implemented in Tween<T>
     *
     * To solve this problem, we make each Tween<T1>, Tween<T2> an explicit subclass T1Tween and T2Tween and
     * implement the lerp method specifically
     *
     * See the implementations in "_OnOffAnimationColor" for some specific workarounds on this issue
     * 
     */

    public abstract class Tween<T> : Animatable<T>, IEquatable<Tween<T>> {
        public Tween(T begin, T end) {
            this.begin = begin;
            this.end = end;
        }

        public virtual T begin { get; set; }

        public virtual T end { get; set; }

        public abstract T lerp(float t);

        public override T transform(float t) {
            if (t == 0.0)
                return begin;
            if (t == 1.0)
                return end;
            return lerp(t);
        }

        public override string ToString() {
            return $"{GetType()}({begin} \u2192 {end})";
        }

        public bool Equals(Tween<T> other) {
            if (ReferenceEquals(null, other)) {
                return false;
            }

            if (ReferenceEquals(this, other)) {
                return true;
            }

            return EqualityComparer<T>.Default.Equals(begin, other.begin) &&
                   EqualityComparer<T>.Default.Equals(end, other.end);
        }

        public override bool Equals(object obj) {
            if (ReferenceEquals(null, obj)) {
                return false;
            }

            if (ReferenceEquals(this, obj)) {
                return true;
            }

            if (obj.GetType() != GetType()) {
                return false;
            }

            return Equals((Tween<T>) obj);
        }

        public override int GetHashCode() {
            unchecked {
                return (EqualityComparer<T>.Default.GetHashCode(begin) * 397) ^
                       EqualityComparer<T>.Default.GetHashCode(end);
            }
        }

        public static bool operator ==(Tween<T> left, Tween<T> right) {
            return Equals(left, right);
        }

        public static bool operator !=(Tween<T> left, Tween<T> right) {
            return !Equals(left, right);
        }
    }

    public class ReverseTween<T> : Tween<T> {
        public ReverseTween(Tween<T> parent) : base(begin: parent.end, end: parent.begin) {
            this.parent = parent;
        }

        public readonly Tween<T> parent;

        public override T lerp(float t) {
            return parent.lerp(1.0f - t);
        }
    }

    public class ColorTween : Tween<Color> {
        public ColorTween(Color begin = null, Color end = null) : base(begin: begin, end: end) {
        }

        public override Color lerp(float t) {
            return Color.lerp(begin, end, t);
        }
    }

    public class SizeTween : Tween<Size> {
        public SizeTween(Size begin = null, Size end = null) : base(begin: begin, end: end) {
        }

        public override Size lerp(float t) {
            return Size.lerp(begin, end, t);
        }
    }

    public class RectTween : Tween<Rect> {
        public RectTween(Rect begin = null, Rect end = null) : base(begin: begin, end: end) {
        }

        public override Rect lerp(float t) {
            return Rect.lerp(begin, end, t);
        }
    }

    public class IntTween : Tween<int> {
        public IntTween(int begin, int end) : base(begin: begin, end: end) {
        }

        public override int lerp(float t) {
            return (begin + (end - begin) * t).round();
        }
    }

    public class NullableFloatTween : Tween<float?> {
        public NullableFloatTween(float? begin = null, float? end = null) : base(begin: begin, end: end) {
        }

        public override float? lerp(float t) {
            D.assert(begin != null);
            D.assert(end != null);
            return begin + (end - begin) * t;
        }
    }

    public class FloatTween : Tween<float> {
        public FloatTween(float begin, float end) : base(begin: begin, end: end) {
        }

        public override float lerp(float t) {
            return begin + (end - begin) * t;
        }
    }

    public class StepTween : Tween<int> {
        public StepTween(int begin, int end) : base(begin: begin, end: end) {
        }

        public override int lerp(float t) {
            return (begin + (end - begin) * t).floor();
        }
    }

    public class OffsetTween : Tween<Offset> {
        public OffsetTween(Offset begin, Offset end) : base(begin: begin, end: end) {
        }

        public override Offset lerp(float t) {
            return (begin + (end - begin) * t);
        }
    }

    class ConstantTween<T> : Tween<T> {
        public ConstantTween(T value) : base(begin: value, end: value) {
        }

        public override T lerp(float t) {
            return begin;
        }

        public override string ToString() {
            return $"{GetType()}(value: {begin})";
        }
    }

    public class CurveTween : Animatable<float> {
        public CurveTween(Curve curve = null) {
            D.assert(curve != null);
            this.curve = curve;
        }

        public readonly Curve curve;
        
        public override float transform(float t) {
            if (t == 0.0 || t == 1.0) {
                D.assert(curve.transform(t).round() == t);
                return t;
            }
            return curve.transform(t);
        }

        /*public override float evaluate(Animation<float> animation) {
            float t = animation.value;
            if (t == 0.0 || t == 1.0) {
                D.assert(curve.transform(t).round() == t);
                return t;
            }

            return curve.transform(t);
        }*/

        public override string ToString() {
            return $"{GetType()}(curve: {curve})";
        }
    }
}