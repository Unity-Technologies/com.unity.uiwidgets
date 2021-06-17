using System;
using System.Collections.Generic;
using Unity.UIWidgets.animation;
using Unity.UIWidgets.async;
using Unity.UIWidgets.foundation;

namespace Unity.UIWidgets.rendering {
    public enum ScrollDirection {
        idle,
        forward,
        reverse,
    }

    public static class ScrollDirectionUtils {
        public static ScrollDirection flipScrollDirection(ScrollDirection direction) {
            switch (direction) {
                case ScrollDirection.idle:
                    return ScrollDirection.idle;
                case ScrollDirection.forward:
                    return ScrollDirection.reverse;
                case ScrollDirection.reverse:
                    return ScrollDirection.forward;
            }

            D.assert(false);
            return default(ScrollDirection);
        }
    }

    public abstract class ViewportOffset : ChangeNotifier {
        protected ViewportOffset() {
        }

        public static ViewportOffset @fixed(float value) {
            return new _FixedViewportOffset(value);
        }

        public static ViewportOffset zero() {
            return _FixedViewportOffset.zero();
        }

        public abstract float pixels { get; }
        public abstract bool applyViewportDimension(float viewportDimension);
        public abstract bool applyContentDimensions(float minScrollExtent, float maxScrollExtent);

        public abstract void correctBy(float correction);
        public abstract void jumpTo(float pixels);

        public abstract Future animateTo(float to, TimeSpan duration, Curve curve);

        public virtual Future moveTo(float to, TimeSpan? duration, Curve curve = null, bool clamp = true) {
            if (duration == null || duration.Value == TimeSpan.Zero) {
                jumpTo(to);
                return Future.value();
            } else {
                return animateTo(to, duration: duration??TimeSpan.Zero , curve: curve ?? Curves.ease);
            }
        }

        public abstract ScrollDirection userScrollDirection { get; }

        public abstract bool allowImplicitScrolling { get; }

        public override string ToString() {
            var description = new List<string>();
            debugFillDescription(description);
            return foundation_.describeIdentity(this) + "(" + string.Join(", ", description.ToArray()) + ")";
        }

        protected virtual void debugFillDescription(List<string> description) {
            description.Add("offset: " + pixels.ToString("F1"));
        }
    }

    class _FixedViewportOffset : ViewportOffset {
        internal _FixedViewportOffset(float _pixels) {
            this._pixels = _pixels;
        }

        internal new static _FixedViewportOffset zero() {
            return new _FixedViewportOffset(0.0f);
        }

        float _pixels;

        public override float pixels {
            get { return _pixels; }
        }

        public override bool applyViewportDimension(float viewportDimension) {
            return true;
        }

        public override bool applyContentDimensions(float minScrollExtent, float maxScrollExtent) {
            return true;
        }

        public override void correctBy(float correction) {
            _pixels += correction;
        }

        public override void jumpTo(float pixels) {
        }

        public override Future animateTo(float to, TimeSpan duration, Curve curve) {
            return Future.value();
        }

        public override ScrollDirection userScrollDirection {
            get { return ScrollDirection.idle; }
        }

        public override bool allowImplicitScrolling {
            get { return false; }
        }
    }
}