using System;
using Unity.UIWidgets.animation;
using Unity.UIWidgets.async;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.gestures;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.physics;
using Unity.UIWidgets.scheduler;
using Unity.UIWidgets.ui;
using UnityEngine;

namespace Unity.UIWidgets.widgets {
    public interface ScrollActivityDelegate {
        AxisDirection axisDirection { get; }

        float setPixels(float pixels);

        void applyUserOffset(float delta);

        void applyUserScrollOffset(float delta);

        void goIdle();

        void goBallistic(float velocity);
    }

    public abstract class ScrollActivity {
        public ScrollActivity(ScrollActivityDelegate del) {
            _del = del;
        }

        public ScrollActivityDelegate del {
            get { return _del; }
        }

        ScrollActivityDelegate _del;

        public void updateDelegate(ScrollActivityDelegate value) {
            D.assert(_del != value);
            _del = value;
        }

        public virtual void resetActivity() {
        }

        public virtual void dispatchScrollStartNotification(ScrollMetrics metrics, BuildContext context) {
            new ScrollStartNotification(metrics: metrics, context: context).dispatch(context);
        }

        public virtual void dispatchScrollUpdateNotification(ScrollMetrics metrics, BuildContext context,
            float scrollDelta) {
            new ScrollUpdateNotification(metrics: metrics, context: context, scrollDelta: scrollDelta)
                .dispatch(context);
        }

        public virtual void dispatchOverscrollNotification(ScrollMetrics metrics, BuildContext context,
            float overscroll) {
            new OverscrollNotification(metrics: metrics, context: context, overscroll: overscroll).dispatch(context);
        }

        public virtual void dispatchScrollEndNotification(ScrollMetrics metrics, BuildContext context) {
            new ScrollEndNotification(metrics: metrics, context: context).dispatch(context);
        }

        public virtual void applyNewDimensions() {
        }

        public abstract bool shouldIgnorePointer { get; }

        public abstract bool isScrolling { get; }

        public abstract float velocity { get; }

        public virtual void dispose() {
            _del = null;
        }

        public override string ToString() {
            return foundation_.describeIdentity(this);
        }
    }

    public class IdleScrollActivity : ScrollActivity {
        public IdleScrollActivity(ScrollActivityDelegate del) : base(del) {
        }

        public override void applyNewDimensions() {
            del.goBallistic(0.0f);
        }

        public override bool shouldIgnorePointer {
            get { return false; }
        }

        public override bool isScrolling {
            get { return false; }
        }

        public override float velocity {
            get { return 0.0f; }
        }
    }

    public interface ScrollHoldController {
        void cancel();
    }

    public class HoldScrollActivity : ScrollActivity, ScrollHoldController {
        public HoldScrollActivity(
            ScrollActivityDelegate del = null,
            VoidCallback onHoldCanceled = null
        ) : base(del) {
            this.onHoldCanceled = onHoldCanceled;
        }

        public readonly VoidCallback onHoldCanceled;

        public override bool shouldIgnorePointer {
            get { return false; }
        }

        public override bool isScrolling {
            get { return false; }
        }

        public override float velocity {
            get { return 0.0f; }
        }

        public void cancel() {
            del.goBallistic(0.0f);
        }

        public override void dispose() {
            if (onHoldCanceled != null) {
                onHoldCanceled();
            }

            base.dispose();
        }
    }

    public class ScrollDragController : Drag {
        public ScrollDragController(
            ScrollActivityDelegate del = null,
            DragStartDetails details = null,
            VoidCallback onDragCanceled = null,
            float? carriedVelocity = null,
            float? motionStartDistanceThreshold = null
        ) {
            D.assert(del != null);
            D.assert(details != null);
            D.assert(
                motionStartDistanceThreshold == null || motionStartDistanceThreshold > 0.0,
                () => "motionStartDistanceThreshold must be a positive number or null"
            );

            _del = del;
            _lastDetails = details;
            _retainMomentum = carriedVelocity != null && carriedVelocity != 0.0;
            _lastNonStationaryTimestamp = details.sourceTimeStamp;
            _offsetSinceLastStop = motionStartDistanceThreshold == null ? (float?) null : 0.0f;

            this.onDragCanceled = onDragCanceled;
            this.carriedVelocity = carriedVelocity;
            this.motionStartDistanceThreshold = motionStartDistanceThreshold;
        }

        public ScrollActivityDelegate del {
            get { return _del; }
        }

        ScrollActivityDelegate _del;

        public readonly VoidCallback onDragCanceled;

        public readonly float? carriedVelocity;

        public readonly float? motionStartDistanceThreshold;

        TimeSpan? _lastNonStationaryTimestamp;

        bool _retainMomentum;

        float? _offsetSinceLastStop;

        public static readonly TimeSpan momentumRetainStationaryDurationThreshold = new TimeSpan(0, 0, 0, 0, 20);

        public static readonly TimeSpan motionStoppedDurationThreshold = new TimeSpan(0, 0, 0, 0, 50);

        const float _bigThresholdBreakDistance = 24.0f;

        bool _reversed {
            get { return AxisUtils.axisDirectionIsReversed(del.axisDirection); }
        }

        public void updateDelegate(ScrollActivityDelegate value) {
            D.assert(_del != value);
            _del = value;
        }

        void _maybeLoseMomentum(float offset, TimeSpan? timestamp) {
            if (_retainMomentum &&
                offset == 0.0 &&
                (timestamp == null ||
                 timestamp - _lastNonStationaryTimestamp >
                 momentumRetainStationaryDurationThreshold)) {
                _retainMomentum = false;
            }
        }

        float _adjustForScrollStartThreshold(float offset, TimeSpan? timestamp) {
            if (timestamp == null) {
                return offset;
            }

            if (offset == 0.0) {
                if (motionStartDistanceThreshold != null &&
                    _offsetSinceLastStop == null &&
                    timestamp - _lastNonStationaryTimestamp >
                    motionStoppedDurationThreshold) {
                    _offsetSinceLastStop = 0.0f;
                }

                return 0.0f;
            }
            else {
                if (_offsetSinceLastStop == null) {
                    return offset;
                }
                else {
                    _offsetSinceLastStop += offset;
                    if (_offsetSinceLastStop.Value.abs() > motionStartDistanceThreshold) {
                        _offsetSinceLastStop = null;
                        if (offset.abs() > _bigThresholdBreakDistance) {
                            return offset;
                        }
                        else {
                            return Mathf.Min(
                                       motionStartDistanceThreshold.Value / 3.0f,
                                       offset.abs()
                                   ) * offset.sign();
                        }
                    }
                    else {
                        return 0.0f;
                    }
                }
            }
        }

        public void update(DragUpdateDetails details) {
            D.assert(details.primaryDelta != null);
            _lastDetails = details;
            float offset = details.primaryDelta.Value;

            if (details.isScroll) {
                if (offset == 0.0) {
                    return;
                }

                if (_reversed) {
                    offset = -offset;
                }


                del.applyUserScrollOffset(offset);
                return;
            }


            if (offset != 0.0) {
                _lastNonStationaryTimestamp = details.sourceTimeStamp;
            }

            _maybeLoseMomentum(offset, details.sourceTimeStamp);
            offset = _adjustForScrollStartThreshold(offset, details.sourceTimeStamp);
            if (offset == 0.0) {
                return;
            }

            if (_reversed) {
                offset = -offset;
            }

            del.applyUserOffset(offset);
        }

        public void end(DragEndDetails details) {
            D.assert(details.primaryVelocity != null);
            float velocity = -details.primaryVelocity.Value;
            if (_reversed) {
                velocity = -velocity;
            }

            _lastDetails = details;

            if (_retainMomentum && velocity.sign() == carriedVelocity.Value.sign()) {
                velocity += carriedVelocity.Value;
            }

            del.goBallistic(velocity);
        }

        public void cancel() {
            del.goBallistic(0.0f);
        }

        public virtual void dispose() {
            _lastDetails = null;
            if (onDragCanceled != null) {
                onDragCanceled();
            }
        }

        public object lastDetails {
            get { return _lastDetails; }
        }

        object _lastDetails;

        public override string ToString() {
            return foundation_.describeIdentity(this);
        }
    }

    public class DragScrollActivity : ScrollActivity {
        public DragScrollActivity(
            ScrollActivityDelegate del,
            ScrollDragController controller
        ) : base(del) {
            _controller = controller;
        }

        ScrollDragController _controller;

        public override void dispatchScrollStartNotification(ScrollMetrics metrics, BuildContext context) {
            object lastDetails = _controller.lastDetails;
            D.assert(lastDetails is DragStartDetails);
            new ScrollStartNotification(metrics: metrics, context: context, dragDetails: (DragStartDetails) lastDetails)
                .dispatch(context);
        }

        public override void dispatchScrollUpdateNotification(ScrollMetrics metrics, BuildContext context,
            float scrollDelta) {
            object lastDetails = _controller.lastDetails;
            D.assert(lastDetails is DragUpdateDetails);
            new ScrollUpdateNotification(metrics: metrics, context: context, scrollDelta: scrollDelta,
                dragDetails: (DragUpdateDetails) lastDetails).dispatch(context);
        }

        public override void dispatchOverscrollNotification(ScrollMetrics metrics, BuildContext context,
            float overscroll) {
            object lastDetails = _controller.lastDetails;
            D.assert(lastDetails is DragUpdateDetails);
            new OverscrollNotification(metrics: metrics, context: context, overscroll: overscroll,
                dragDetails: (DragUpdateDetails) lastDetails).dispatch(context);
        }

        public override void dispatchScrollEndNotification(ScrollMetrics metrics, BuildContext context) {
            object lastDetails = _controller.lastDetails;
            new ScrollEndNotification(
                metrics: metrics,
                context: context,
                dragDetails: lastDetails as DragEndDetails
            ).dispatch(context);
        }

        public override bool shouldIgnorePointer {
            get { return true; }
        }

        public override bool isScrolling {
            get { return true; }
        }

        public override float velocity {
            get { return 0.0f; }
        }

        public override void dispose() {
            _controller = null;
            base.dispose();
        }

        public override string ToString() {
            return $"{foundation_.describeIdentity(this)}({_controller})";
        }
    }

    public class BallisticScrollActivity : ScrollActivity {
        public BallisticScrollActivity(
            ScrollActivityDelegate del,
            Simulation simulation,
            TickerProvider vsync
        ) : base(del) {
            _controller = AnimationController.unbounded(
                debugLabel: GetType().ToString(),
                vsync: vsync
            );

            _controller.addListener(_tick);
            _controller.animateWith(simulation).then(o => _end());
        }

        public override float velocity {
            get { return _controller.velocity; }
        }

        readonly AnimationController _controller;

        public override void resetActivity() {
            del.goBallistic(velocity);
        }

        public override void applyNewDimensions() {
            del.goBallistic(velocity);
        }

        void _tick() {
            if (!applyMoveTo(_controller.value)) {
                del.goIdle();
            }
        }

        protected virtual bool applyMoveTo(float value) {
            return del.setPixels(value) == 0.0;
        }

        void _end() {
            if (del != null) {
                del.goBallistic(0.0f);
            }
        }

        public override void dispatchOverscrollNotification(
            ScrollMetrics metrics, BuildContext context, float overscroll) {
            new OverscrollNotification(metrics: metrics, context: context, overscroll: overscroll,
                velocity: velocity).dispatch(context);
        }

        public override bool shouldIgnorePointer {
            get { return true; }
        }

        public override bool isScrolling {
            get { return true; }
        }

        public override void dispose() {
            _controller.dispose();
            base.dispose();
        }

        public override string ToString() {
            return $"{foundation_.describeIdentity(this)}({_controller})";
        }
    }

    public class DrivenScrollActivity : ScrollActivity {
        public DrivenScrollActivity(
            ScrollActivityDelegate del,
            float from,
            float to,
            TimeSpan duration,
            Curve curve,
            TickerProvider vsync
        ) : base(del) {
            D.assert(duration > TimeSpan.Zero);
            D.assert(curve != null);

            _completer = Completer.create();
            _controller = AnimationController.unbounded(
                value: from,
                debugLabel: GetType().ToString(),
                vsync: vsync
            );
            _controller.addListener(_tick);
            _controller.animateTo(to, duration: duration, curve: curve)
                .then(o => _end());
        }

        readonly Completer _completer;
        readonly AnimationController _controller;

        public Future done {
            get { return _completer.future; }
        }

        public override float velocity {
            get { return _controller.velocity; }
        }

        void _tick() {
            if (del.setPixels(_controller.value) != 0.0) {
                del.goIdle();
            }
        }

        void _end() {
            if (del != null) {
                del.goBallistic(velocity);
            }
        }

        public override void dispatchOverscrollNotification(
            ScrollMetrics metrics, BuildContext context, float overscroll) {
            new OverscrollNotification(metrics: metrics, context: context, overscroll: overscroll,
                velocity: velocity).dispatch(context);
        }

        public override bool shouldIgnorePointer {
            get { return true; }
        }

        public override bool isScrolling {
            get { return true; }
        }

        public override void dispose() {
            _completer.complete();
            _controller.dispose();
            base.dispose();
        }

        public override string ToString() {
            return $"{foundation_.describeIdentity(this)}({_controller})";
        }
    }
}