using System;
using System.Collections.Generic;
using Unity.UIWidgets.animation;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.gestures;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.physics;
using Unity.UIWidgets.rendering;
using Unity.UIWidgets.ui;
using Unity.UIWidgets.widgets;

namespace Unity.UIWidget.material {
    public delegate Widget ScrollableWidgetBuilder(
        BuildContext context,
        ScrollController scrollController
    );

    public class DraggableScrollableSheet : StatefulWidget {
        public DraggableScrollableSheet(
            Key key = null,
            float initialChildSize = 0.5f,
            float minChildSize = 0.25f,
            bool expand = true,
            ScrollableWidgetBuilder builder = null
        ) : base(key: key) {
            D.assert(builder != null);
            this.initialChildSize = initialChildSize;
            this.minChildSize = minChildSize;
            this.expand = expand;
            this.builder = builder;
        }

        public readonly float initialChildSize;

        public readonly float minChildSize;

        public readonly float maxChildSize;

        public readonly bool expand;

        public readonly ScrollableWidgetBuilder builder;

        public override State createState() {
            return new _DraggableScrollableSheetState();
        }
    }

    public class DraggableScrollableNotification : ViewportNotificationMixinNotification {
        public DraggableScrollableNotification(
            float extent,
            float minExtent,
            float maxExtent,
            float initialExtent,
            BuildContext context
        ) {
            D.assert(0.0f <= minExtent);
            D.assert(maxExtent <= 1.0f);
            D.assert(minExtent <= extent);
            D.assert(minExtent <= initialExtent);
            D.assert(extent <= maxExtent);
            D.assert(initialExtent <= maxExtent);
            D.assert(context != null);

            this.extent = extent;
            this.minExtent = minExtent;
            this.maxExtent = maxExtent;
            this.initialExtent = initialExtent;
            this.context = context;
        }

        public readonly float extent;

        public readonly float minExtent;

        public readonly float maxExtent;

        public readonly float initialExtent;

        public readonly BuildContext context;

        protected override void debugFillDescription(List<string> description) {
            base.debugFillDescription(description);
            description.Add(
                $"minExtent: {minExtent}, extent: {extent}, maxExtent: {maxExtent}, initialExtent: {initialExtent}");
        }
    }

    class _DraggableSheetExtent {
        public _DraggableSheetExtent(
            float minExtent,
            float maxExtent,
            float initialExtent,
            VoidCallback listener
        ) {
            D.assert(minExtent >= 0);
            D.assert(maxExtent <= 1);
            D.assert(minExtent <= initialExtent);
            D.assert(initialExtent <= maxExtent);

            this.minExtent = minExtent;
            this.maxExtent = maxExtent;
            this.initialExtent = initialExtent;
            _currentExtent = new ValueNotifier<float>(initialExtent);
            _currentExtent.addListener(listener);
            availablePixels = float.PositiveInfinity;
        }

        float minExtent;
        float maxExtent;
        internal float initialExtent;
        internal ValueNotifier<float> _currentExtent;
        internal float availablePixels;

        public bool isAtMin {
            get { return minExtent >= _currentExtent.value; }
        }

        public bool isAtMax {
            get { return maxExtent <= _currentExtent.value; }
        }

        public float currentExtent {
            get { return _currentExtent.value; }
            set { _currentExtent.value = value.clamp(minExtent, maxExtent); }
        }

        public float additionalMinExtent {
            get { return isAtMin ? 0.0f : 1.0f; }
        }

        public float additionalMaxExtent {
            get { return isAtMax ? 0.0f : 1.0f; }
        }

        public void addPixelDelta(float delta, BuildContext context) {
            if (availablePixels == 0) {
                return;
            }

            currentExtent += delta / availablePixels * maxExtent;
            new DraggableScrollableNotification(
                minExtent: minExtent,
                maxExtent: maxExtent,
                extent: currentExtent,
                initialExtent: initialExtent,
                context: context
            ).dispatch(context);
        }
    }

    class _DraggableScrollableSheetState : State<DraggableScrollableSheet> {
        _DraggableScrollableSheetScrollController _scrollController;
        _DraggableSheetExtent _extent;

        public override void initState() {
            base.initState();
            _extent = new _DraggableSheetExtent(
                minExtent: widget.minChildSize,
                maxExtent: widget.maxChildSize,
                initialExtent: widget.initialChildSize,
                listener: _setExtent
            );
            _scrollController = new _DraggableScrollableSheetScrollController(extent: _extent);
        }

        public override void didChangeDependencies() {
            base.didChangeDependencies();
            if (_InheritedResetNotifier.shouldReset(context)) {
                if (_scrollController.offset != 0.0f) {
                    _scrollController.animateTo(
                        0.0f,
                        duration: new TimeSpan(0, 0, 0, 0, 1),
                        curve: Curves.linear
                    );
                }

                _extent._currentExtent.value = _extent.initialExtent;
            }
        }

        void _setExtent() {
            setState(() => { });
        }

        public override Widget build(BuildContext context) {
            return new LayoutBuilder(
                builder: (BuildContext subContext, BoxConstraints subConstraints) => {
                    _extent.availablePixels = widget.maxChildSize * subConstraints.biggest.height;
                    Widget sheet = new FractionallySizedBox(
                        heightFactor: _extent.currentExtent,
                        child: widget.builder(subContext, _scrollController),
                        alignment: Alignment.bottomCenter
                    );
                    return widget.expand ? SizedBox.expand(child: sheet) : sheet;
                }
            );
        }

        public override void dispose() {
            _scrollController.dispose();
            base.dispose();
        }
    }

    class _DraggableScrollableSheetScrollController : ScrollController {
        public _DraggableScrollableSheetScrollController(
            float initialScrollOffset = 0.0f,
            string debugLabel = null,
            _DraggableSheetExtent extent = null
        ) : base(debugLabel: debugLabel, initialScrollOffset: initialScrollOffset) {
            D.assert(extent != null);
            this.extent = extent;
        }

        public readonly _DraggableSheetExtent extent;

        public override ScrollPosition createScrollPosition(
            ScrollPhysics physics,
            ScrollContext context,
            ScrollPosition oldPosition
        ) {
            return new _DraggableScrollableSheetScrollPosition(
                physics: physics,
                context: context,
                oldPosition: oldPosition,
                extent: extent
            );
        }

        protected override void debugFillDescription(List<string> description) {
            base.debugFillDescription(description);
            description.Add($"extent: {extent}");
        }
    }

    class _DraggableScrollableSheetScrollPosition : ScrollPositionWithSingleContext {
        public _DraggableScrollableSheetScrollPosition(
            ScrollPhysics physics = null,
            ScrollContext context = null,
            float initialPixels = 0.0f,
            bool keepScrollOffset = true,
            ScrollPosition oldPosition = null,
            string debugLabel = null,
            _DraggableSheetExtent extent = null
        ) : base(physics: physics,
            context: context,
            initialPixels: initialPixels,
            keepScrollOffset: keepScrollOffset,
            oldPosition: oldPosition,
            debugLabel: debugLabel) {
            D.assert(extent != null);
            this.extent = extent;
        }

        VoidCallback _dragCancelCallback;
        public readonly _DraggableSheetExtent extent;

        bool listShouldScroll {
            get { return pixels > 0.0f; }
        }

        public override bool applyContentDimensions(float minScrollExtent, float maxScrollExtent) {
            return base.applyContentDimensions(
                minScrollExtent - extent.additionalMinExtent,
                maxScrollExtent + extent.additionalMaxExtent
            );
        }

        public override void applyUserOffset(float delta) {
            if (!listShouldScroll &&
                (!(extent.isAtMin || extent.isAtMax) ||
                 (extent.isAtMin && delta < 0) ||
                 (extent.isAtMax && delta > 0))) {
                extent.addPixelDelta(-delta, context.notificationContext);
            }
            else {
                base.applyUserOffset(delta);
            }
        }

        public override void goBallistic(float velocity) {
            if (velocity == 0.0f ||
                (velocity < 0.0f && listShouldScroll) ||
                (velocity > 0.0f && extent.isAtMax)) {
                base.goBallistic(velocity);
                return;
            }

            _dragCancelCallback?.Invoke();
            _dragCancelCallback = null;

            Simulation simulation = new ClampingScrollSimulation(
                position: extent.currentExtent,
                velocity: velocity,
                tolerance: physics.tolerance
            );

            AnimationController ballisticController = AnimationController.unbounded(
                debugLabel: $"{GetType()}",
                vsync: context.vsync
            );

            float lastDelta = 0;

            void _tick() {
                float delta = ballisticController.value - lastDelta;
                lastDelta = ballisticController.value;
                extent.addPixelDelta(delta, context.notificationContext);
                if ((velocity > 0 && extent.isAtMax) || (velocity < 0 && extent.isAtMin)) {
                    velocity = ballisticController.velocity +
                               (physics.tolerance.velocity * ballisticController.velocity.sign());
                    base.goBallistic(velocity);
                    ballisticController.stop();
                }
                else if (ballisticController.isCompleted) {
                    base.goBallistic(0);
                }
            }

            ballisticController.addListener(_tick);
            ballisticController.animateWith(simulation).whenCompleteOrCancel(
                ballisticController.dispose
            );
        }

        public override Drag drag(DragStartDetails details, VoidCallback dragCancelCallback) {
            _dragCancelCallback = dragCancelCallback;
            return base.drag(details, dragCancelCallback);
        }
    }

    public class DraggableScrollableActuator : StatelessWidget {
        public DraggableScrollableActuator(
            Key key = null,
            Widget child = null
        ) : base(key: key) {
            D.assert(child != null);
            this.child = child;
        }

        public readonly Widget child;

        readonly _ResetNotifier _notifier = new _ResetNotifier();

        public static bool reset(BuildContext context) {
            _InheritedResetNotifier notifier = context.dependOnInheritedWidgetOfExactType<_InheritedResetNotifier>();
            if (notifier == null) {
                return false;
            }

            return notifier._sendReset();
        }

        public override Widget build(BuildContext context) {
            return new _InheritedResetNotifier(child: child, notifier: _notifier);
        }
    }

    class _ResetNotifier : ChangeNotifier {
        internal bool _wasCalled = false;

        internal bool sendReset() {
            if (!hasListeners) {
                return false;
            }

            _wasCalled = true;
            notifyListeners();
            return true;
        }
    }

    class _InheritedResetNotifier : InheritedNotifier<_ResetNotifier> {
        public _InheritedResetNotifier(
            Key key = null,
            Widget child = null,
            _ResetNotifier notifier = null
        ) : base(key: key, child: child, notifier: notifier) {
        }

        internal bool _sendReset() {
            return notifier.sendReset();
        }

        public static bool shouldReset(BuildContext context) {
            InheritedWidget widget = context.dependOnInheritedWidgetOfExactType<_InheritedResetNotifier>();
            if (widget == null) {
                return false;
            }

            _InheritedResetNotifier inheritedNotifier = widget as _InheritedResetNotifier;
            bool wasCalled = inheritedNotifier.notifier._wasCalled;
            inheritedNotifier.notifier._wasCalled = false;
            return wasCalled;
        }
    }
}