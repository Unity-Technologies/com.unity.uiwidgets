using System;
using System.Collections.Generic;
using Unity.UIWidgets.animation;
using Unity.UIWidgets.async;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.gestures;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.physics;
using Unity.UIWidgets.rendering;
using Unity.UIWidgets.scheduler;
using Unity.UIWidgets.ui;

namespace Unity.UIWidgets.widgets {
    public enum ScrollPositionAlignmentPolicy {
        explicitPolicy,
        keepVisibleAtEnd,
        keepVisibleAtStart,
    }

    public abstract class ScrollPosition : ViewportOffset, ScrollMetrics {
        protected ScrollPosition(
            ScrollPhysics physics = null,
            ScrollContext context = null,
            bool keepScrollOffset = true,
            ScrollPosition oldPosition = null,
            string debugLabel = null,
            object coordinator = null
        ) {
            D.assert(physics != null);
            D.assert(context != null);
            D.assert(context.vsync != null);

            this.physics = physics;
            this.context = context;
            this.keepScrollOffset = keepScrollOffset;
            this.debugLabel = debugLabel;
            _coordinator = coordinator;
            if (oldPosition != null) {
                absorb(oldPosition);
            }

            if (keepScrollOffset) {
                restoreScrollOffset();
            }
        }

        public readonly ScrollPhysics physics;

        public readonly ScrollContext context;

        public readonly bool keepScrollOffset;

        public readonly string debugLabel;

        internal readonly object _coordinator;

        public float minScrollExtent {
            get { return _minScrollExtent.Value; }
        }

        float? _minScrollExtent;

        public float maxScrollExtent {
            get { return _maxScrollExtent.Value; }
        }

        float? _maxScrollExtent;

        public bool hasMinScrollExtent {
            get { return _minScrollExtent != null; }
        }

        public bool hasMaxScrollExtent {
            get { return _maxScrollExtent != null; }
        }
        
        public override float pixels {
            get {
                D.assert(_pixels != null);
                return _pixels ?? 0.0f;
            }
        }

        public bool havePixels {
            get { return _pixels != null; }
        }

        internal float? _pixels;

        public float viewportDimension {
            get { return _viewportDimension.Value; }
        }

        float? _viewportDimension;

        public bool haveViewportDimension {
            get { return _viewportDimension != null; }
        }

        public bool haveDimensions {
            get { return _haveDimensions; }
        }

        bool _haveDimensions = false;

        public abstract AxisDirection axisDirection { get; }

        protected virtual void absorb(ScrollPosition other) {
            D.assert(other != null);
            D.assert(other.context == context);
            D.assert(_pixels == null);
            _minScrollExtent = other._minScrollExtent;
            _maxScrollExtent = other._maxScrollExtent;
            _pixels = other._pixels;
            _viewportDimension = other._viewportDimension;

            D.assert(activity == null);
            D.assert(other.activity != null);
            _activity = other.activity;
            other._activity = null;
            if (other.GetType() != GetType()) {
                activity.resetActivity();
            }

            context.setIgnorePointer(activity.shouldIgnorePointer);
            isScrollingNotifier.value = activity.isScrolling;
        }

        public virtual float setPixels(float newPixels) {
            D.assert(_pixels != null);
            D.assert(SchedulerBinding.instance.schedulerPhase <= SchedulerPhase.transientCallbacks);
            if (newPixels != pixels) {
                float overscroll = applyBoundaryConditions(newPixels);
                D.assert(() => {
                    float delta = newPixels - pixels;
                    if (overscroll.abs() > delta.abs()) {
                        throw new UIWidgetsError(
                            string.Format(
                                "{0}.applyBoundaryConditions returned invalid overscroll value.\n" +
                                "setPixels() was called to change the scroll offset from {1} to {2}.\n" +
                                "That is a delta of {3} units.\n" +
                                "{0}.applyBoundaryConditions reported an overscroll of {4} units."
                                , GetType(), pixels, newPixels, delta, overscroll));
                    }

                    return true;
                });

                float oldPixels = pixels;
                _pixels = newPixels - overscroll;
                if (pixels != oldPixels) {
                    notifyListeners();
                    didUpdateScrollPositionBy(pixels - oldPixels);
                }

                if (overscroll != 0.0) {
                    didOverscrollBy(overscroll);
                    return overscroll;
                }
            }

            return 0.0f;
        }

        public void correctPixels(float value) {
            _pixels = value;
        }

        public override void correctBy(float correction) {
            D.assert(
                _pixels != null,
                () => "An initial pixels value must exist by caling correctPixels on the ScrollPosition"
            );

            _pixels += correction;
            _didChangeViewportDimensionOrReceiveCorrection = true;
        }

        protected void forcePixels(float value) {
            D.assert(_pixels != null);
            _pixels = value;
            notifyListeners();
        }

        protected virtual void saveScrollOffset() {
            var pageStorage = PageStorage.of(context.storageContext);
            if (pageStorage != null) {
                pageStorage.writeState(context.storageContext, pixels);
            }
        }

        protected virtual void restoreScrollOffset() {
            if (_pixels == null) {
                var pageStorage = PageStorage.of(context.storageContext);
                if (pageStorage != null) {
                    object valueRaw = pageStorage.readState(context.storageContext);
                    if (valueRaw != null) {
                        correctPixels((float) valueRaw);
                    }
                }
            }
        }

        protected float applyBoundaryConditions(float value) {
            float result = physics.applyBoundaryConditions(this, value);
            D.assert(() => {
                float delta = value - pixels;
                if (result.abs() > delta.abs()) {
                    throw new UIWidgetsError(
                        $"{physics.GetType()}.applyBoundaryConditions returned invalid overscroll value.\n" +
                        $"The method was called to consider a change from {pixels} to {value}, which is a " +
                        $"delta of {delta:F1} units. However, it returned an overscroll of " +
                        $"${result:F1} units, which has a greater magnitude than the delta. " +
                        "The applyBoundaryConditions method is only supposed to reduce the possible range " +
                        "of movement, not increase it.\n" +
                        $"The scroll extents are {minScrollExtent} .. {maxScrollExtent}, and the " +
                        $"viewport dimension is {viewportDimension}.");
                }

                return true;
            });

            return result;
        }

        bool _didChangeViewportDimensionOrReceiveCorrection = true;

        public override bool applyViewportDimension(float viewportDimension) {
            if (_viewportDimension != viewportDimension) {
                _viewportDimension = viewportDimension;
                _didChangeViewportDimensionOrReceiveCorrection = true;
            }

            return true;
        }

        public override bool applyContentDimensions(float minScrollExtent, float maxScrollExtent) {
            if (!PhysicsUtils.nearEqual(_minScrollExtent, minScrollExtent, Tolerance.defaultTolerance.distance) ||
                !PhysicsUtils.nearEqual(_maxScrollExtent, maxScrollExtent, Tolerance.defaultTolerance.distance) ||
                _didChangeViewportDimensionOrReceiveCorrection) {
                D.assert(minScrollExtent <= maxScrollExtent);
                _minScrollExtent = minScrollExtent;
                _maxScrollExtent = maxScrollExtent;
                _haveDimensions = true;
                applyNewDimensions();
                _didChangeViewportDimensionOrReceiveCorrection = false;
            }

            return true;
        }

        protected virtual void applyNewDimensions() {
            D.assert(_pixels != null);
            activity.applyNewDimensions();
        }

        public Future ensureVisible(
            RenderObject renderObject,
            float alignment = 0.0f,
            TimeSpan? duration = null,
            Curve curve = null,
            ScrollPositionAlignmentPolicy alignmentPolicy = ScrollPositionAlignmentPolicy.explicitPolicy
        ) {
            D.assert(renderObject.attached);
            RenderAbstractViewport viewport = RenderViewportUtils.of(renderObject);
            D.assert(viewport != null);

            float target = 0f;
            switch (alignmentPolicy) {
                case ScrollPositionAlignmentPolicy.explicitPolicy:
                    target = viewport.getOffsetToReveal(renderObject, alignment).offset.clamp(minScrollExtent, maxScrollExtent);
                    break;
                case ScrollPositionAlignmentPolicy.keepVisibleAtEnd:
                    target = viewport.getOffsetToReveal(renderObject, 1.0f).offset.clamp(minScrollExtent, maxScrollExtent) ;
                    if (target < pixels) {
                        target = pixels;
                    }
                    break;
                case ScrollPositionAlignmentPolicy.keepVisibleAtStart:
                    target = viewport.getOffsetToReveal(renderObject, 0.0f).offset.clamp(minScrollExtent, maxScrollExtent) ;
                    if (target > pixels) {
                        target = pixels;
                    }
                    break;
            }
            if (foundation_.FloatEqual(target,pixels)) {
                return Future.value();
            }

            duration = duration ?? TimeSpan.Zero;
            if (duration == TimeSpan.Zero) {
                jumpTo(target);
                return Future.value();
            }

            curve = curve ?? Curves.ease;
            return animateTo(target, duration: duration.Value, curve: curve);
        }

        public readonly ValueNotifier<bool> isScrollingNotifier = new ValueNotifier<bool>(false);

        public override Future moveTo(float to, TimeSpan? duration, Curve curve = null, bool clamp = true) {
            if (clamp) {
                to = to.clamp(minScrollExtent, maxScrollExtent);
            }

            return base.moveTo(to, duration: duration, curve: curve, clamp: clamp);
        }
        
        public override bool allowImplicitScrolling {
            get { return physics.allowImplicitScrolling; }
        }

        public abstract ScrollHoldController hold(VoidCallback holdCancelCallback);

        public abstract Drag drag(DragStartDetails details, VoidCallback dragCancelCallback);

        protected ScrollActivity activity {
            get { return _activity; }
        }

        ScrollActivity _activity;

        public virtual void beginActivity(ScrollActivity newActivity) {
            if (newActivity == null) {
                return;
            }

            bool wasScrolling, oldIgnorePointer;
            if (_activity != null) {
                oldIgnorePointer = _activity.shouldIgnorePointer;
                wasScrolling = _activity.isScrolling;
                if (wasScrolling && !newActivity.isScrolling) {
                    didEndScroll();
                }

                _activity.dispose();
            }
            else {
                oldIgnorePointer = false;
                wasScrolling = false;
            }

            _activity = newActivity;
            if (oldIgnorePointer != activity.shouldIgnorePointer) {
                context.setIgnorePointer(activity.shouldIgnorePointer);
            }

            isScrollingNotifier.value = activity.isScrolling;
            if (!wasScrolling && _activity.isScrolling) {
                didStartScroll();
            }
        }

        public void didStartScroll() {
            activity.dispatchScrollStartNotification(
                ScrollMetricsUtils.copyWith(this), context.notificationContext);
        }

        public void didUpdateScrollPositionBy(float delta) {
            activity.dispatchScrollUpdateNotification(
                ScrollMetricsUtils.copyWith(this), context.notificationContext, delta);
        }

        public void didEndScroll() {
            activity.dispatchScrollEndNotification(
                ScrollMetricsUtils.copyWith(this), context.notificationContext);
            if (keepScrollOffset) {
                saveScrollOffset();
            }
        }

        public void didOverscrollBy(float value) {
            D.assert(activity.isScrolling);
            activity.dispatchOverscrollNotification(
                ScrollMetricsUtils.copyWith(this), context.notificationContext, value);
        }

        public void didUpdateScrollDirection(ScrollDirection direction) {
            new UserScrollNotification(metrics:
                ScrollMetricsUtils.copyWith(this), context: context.notificationContext, direction: direction
            ).dispatch(context.notificationContext);
        }

        public bool recommendDeferredLoading(BuildContext context) {
            D.assert(context != null);
            D.assert(activity != null);
            return physics.recommendDeferredLoading(activity.velocity, ScrollMetricsUtils.copyWith(this), context);
        }

        public override void dispose() {
            D.assert(_pixels != null);
            if (activity != null) {
                activity.dispose();
                _activity = null;
            }

            base.dispose();
        }

        protected override void debugFillDescription(List<string> description) {
            if (debugLabel != null) {
                description.Add(debugLabel);
            }

            base.debugFillDescription(description);
            description.Add($"range: {_minScrollExtent:F1}..{_maxScrollExtent:F1}");
            description.Add($"viewport: {_viewportDimension:F1}");
        }
    }
}