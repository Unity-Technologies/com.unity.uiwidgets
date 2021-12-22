using System;
using System.Collections.Generic;
using System.Linq;
using Unity.UIWidgets.animation;
using Unity.UIWidgets.async;
using Unity.UIWidgets.external;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.physics;
using Unity.UIWidgets.rendering;
using Unity.UIWidgets.scheduler;
using Unity.UIWidgets.ui;
using UnityEngine;

namespace Unity.UIWidgets.widgets {
    public interface ListWheelChildDelegate {
        int? estimatedChildCount { get; }
        Widget build(BuildContext context, int index);
        int trueIndexOf(int index);
        bool shouldRebuild(ListWheelChildDelegate oldDelegate);
    }

    public class ListWheelChildListDelegate : ListWheelChildDelegate {
        public readonly List<Widget> children;

        public ListWheelChildListDelegate(
            List<Widget> children
        ) {
            D.assert(children != null);
            this.children = children;
        }

        public int? estimatedChildCount {
            get { return children.Count; }
        }

        public Widget build(BuildContext context, int index) {
            if (index < 0 || index >= children.Count) {
                return null;
            }

            return new IndexedSemantics(child: children[index: index], index: index);
        }

        public int trueIndexOf(int index) {
            return index;
        }

        public bool shouldRebuild(ListWheelChildDelegate oldDelegate) {
            return children != ((ListWheelChildListDelegate) oldDelegate).children;
        }
    }

    public class ListWheelChildLoopingListDelegate : ListWheelChildDelegate {
        public readonly List<Widget> children;

        public ListWheelChildLoopingListDelegate(
            List<Widget> children
        ) {
            D.assert(children != null);
            this.children = children;
        }

        public int? estimatedChildCount {
            get { return null; }
        }

        public int trueIndexOf(int index) {
            var result = index % children.Count;
            if (index < 0) {result = result + children.Count; }
            return result;
        }

        public Widget build(BuildContext context, int index) {
            if (children.isEmpty()) {
                return null;
            }

            return new IndexedSemantics(child: children[Mathf.Abs(index % children.Count)]);
        }

        public bool shouldRebuild(ListWheelChildDelegate oldDelegate) {
            return children != ((ListWheelChildLoopingListDelegate) oldDelegate).children;
        }
    }

    public class ListWheelChildBuilderDelegate : ListWheelChildDelegate {
        public readonly IndexedWidgetBuilder builder;

        public readonly int? childCount;

        public ListWheelChildBuilderDelegate(
            IndexedWidgetBuilder builder,
            int? childCount = null
        ) {
            D.assert(builder != null);
            this.builder = builder;
            this.childCount = childCount;
        }

        public int? estimatedChildCount {
            get { return childCount; }
        }

        public Widget build(BuildContext context, int index) {
            if (childCount == null) {
                var child = builder(context: context, index: index);
                return child == null ? null : new IndexedSemantics(child: child);
            }

            if (index < 0 || index >= childCount) {
                return null;
            }

            return new IndexedSemantics(child: builder(context: context, index: index));
        }

        public int trueIndexOf(int index) {
            return index;
        }

        public bool shouldRebuild(ListWheelChildDelegate oldDelegate) {
            return builder != ((ListWheelChildBuilderDelegate) oldDelegate).builder ||
                   childCount != ((ListWheelChildBuilderDelegate) oldDelegate).childCount;
        }
    }

    class ListWheelScrollViewUtils {
        public static int _getItemFromOffset(
            float offset,
            float itemExtent,
            float minScrollExtent,
            float maxScrollExtent
        ) {
            return (_clipOffsetToScrollableRange(offset: offset, minScrollExtent: minScrollExtent,
                maxScrollExtent: maxScrollExtent) / itemExtent).round();
        }

        public static float _clipOffsetToScrollableRange(
            float offset,
            float minScrollExtent,
            float maxScrollExtent
        ) {
            return Mathf.Min(Mathf.Max(a: offset, b: minScrollExtent), b: maxScrollExtent);
        }
    }

    public class FixedExtentScrollController : ScrollController {
        public readonly int initialItem;

        public FixedExtentScrollController(
            int initialItem = 0
        ) {
            this.initialItem = initialItem;
        }

        public int selectedItem {
            get {
                D.assert(positions.isNotEmpty(),
                    () =>
                        "FixedExtentScrollController.selectedItem cannot be accessed before a scroll view is built with it."
                );
                D.assert(positions.Count == 1,
                    () =>
                        "The selectedItem property cannot be read when multiple scroll views are attached to the same FixedExtentScrollController."
                );
                var position = (_FixedExtentScrollPosition) this.position;
                return position.itemIndex;
            }
        }

        public Future animateToItem(
            int itemIndex,
            TimeSpan duration,
            Curve curve
        ) {
            if (!hasClients) {
                return null;
            }

            var futures = new List<Future>();

            foreach (var position in positions.Cast<_FixedExtentScrollPosition>()) {
                futures.Add(position.animateTo(
                    itemIndex * position.itemExtent,
                    duration: duration,
                    curve: curve
                ));
            }

            return Future.wait<object>(futures: futures);
        }

        public void jumpToItem(int itemIndex) {
            foreach (var position in positions.Cast<_FixedExtentScrollPosition>()) {
                position.jumpTo(itemIndex * position.itemExtent);
            }
        }

        public override ScrollPosition createScrollPosition(
            ScrollPhysics physics,
            ScrollContext context,
            ScrollPosition oldPosition) {
            return new _FixedExtentScrollPosition(
                physics: physics,
                context: context,
                initialItem: initialItem,
                oldPosition: oldPosition
            );
        }
    }

    public interface IFixedExtentMetrics {
        int itemIndex { set; get; }

        FixedExtentMetrics copyWith(
            float? minScrollExtent = null,
            float? maxScrollExtent = null,
            float? pixels = null,
            float? viewportDimension = null,
            AxisDirection? axisDirection = null,
            int? itemIndex = null
        );
    }

    public class FixedExtentMetrics : FixedScrollMetrics, IFixedExtentMetrics {
        public FixedExtentMetrics(
            float minScrollExtent,
            float maxScrollExtent,
            float pixels,
            float viewportDimension,
            AxisDirection axisDirection,
            int itemIndex
        ) : base(
            minScrollExtent: minScrollExtent,
            maxScrollExtent: maxScrollExtent,
            pixels: pixels,
            viewportDimension: viewportDimension,
            axisDirection: axisDirection
        ) {
            this.itemIndex = itemIndex;
        }

        public int itemIndex { get; set; }

        public FixedExtentMetrics copyWith(
            float? minScrollExtent = null,
            float? maxScrollExtent = null,
            float? pixels = null,
            float? viewportDimension = null,
            AxisDirection? axisDirection = null,
            int? itemIndex = null
        ) {
            return new FixedExtentMetrics(
                minScrollExtent ?? this.minScrollExtent,
                maxScrollExtent ?? this.maxScrollExtent,
                pixels ?? this.pixels,
                viewportDimension ?? this.viewportDimension,
                axisDirection ?? this.axisDirection,
                itemIndex ?? this.itemIndex
            );
        }
    }

    class _FixedExtentScrollPosition : ScrollPositionWithSingleContext, IFixedExtentMetrics {
        public _FixedExtentScrollPosition(
            ScrollPhysics physics = null,
            ScrollContext context = null,
            int? initialItem = null,
            bool keepScrollOffset = true,
            ScrollPosition oldPosition = null,
            string debugLabel = null
        ) : base(
            physics: physics,
            context: context,
            _getItemExtentFromScrollContext(context: context) * (initialItem ?? 0.0f),
            keepScrollOffset: keepScrollOffset,
            oldPosition: oldPosition,
            debugLabel: debugLabel
        ) {
            D.assert(
                context is _FixedExtentScrollableState,
                () => "FixedExtentScrollController can only be used with ListWheelScrollViews"
            );
        }

        public float itemExtent {
            get { return _getItemExtentFromScrollContext(context: context); }
        }


        public int itemIndex {
            get {
                return ListWheelScrollViewUtils._getItemFromOffset(
                    offset: pixels,
                    itemExtent: itemExtent,
                    minScrollExtent: minScrollExtent,
                    maxScrollExtent: maxScrollExtent
                );
            }
            set { }
        }

        public FixedExtentMetrics copyWith(
            float? minScrollExtent = null,
            float? maxScrollExtent = null,
            float? pixels = null,
            float? viewportDimension = null,
            AxisDirection? axisDirection = null,
            int? itemIndex = null
        ) {
            return new FixedExtentMetrics(
                minScrollExtent ?? this.minScrollExtent,
                maxScrollExtent ?? this.maxScrollExtent,
                pixels ?? this.pixels,
                viewportDimension ?? this.viewportDimension,
                axisDirection ?? this.axisDirection,
                itemIndex ?? this.itemIndex
            );
        }

        static float _getItemExtentFromScrollContext(ScrollContext context) {
            var scrollable = (_FixedExtentScrollableState) context;
            return scrollable.itemExtent;
        }
    }

    class _FixedExtentScrollable : Scrollable {
        public readonly float itemExtent;

        public _FixedExtentScrollable(
            float itemExtent,
            Key key = null,
            AxisDirection axisDirection = AxisDirection.down,
            ScrollController controller = null,
            ScrollPhysics physics = null,
            ViewportBuilder viewportBuilder = null
        ) : base(
            key: key,
            axisDirection: axisDirection,
            controller: controller,
            physics: physics,
            viewportBuilder: viewportBuilder
        ) {
            this.itemExtent = itemExtent;
        }

        public override State createState() {
            return new _FixedExtentScrollableState();
        }
    }

    class _FixedExtentScrollableState : ScrollableState {
        public float itemExtent {
            get {
                var actualWidget = (_FixedExtentScrollable) widget;
                return actualWidget.itemExtent;
            }
        }
    }


    public class FixedExtentScrollPhysics : ScrollPhysics {
        public FixedExtentScrollPhysics(
            ScrollPhysics parent = null
        ) : base(parent: parent) {
        }

        public override ScrollPhysics applyTo(ScrollPhysics ancestor) {
            return new FixedExtentScrollPhysics(buildParent(ancestor: ancestor));
        }

        public override Simulation createBallisticSimulation(ScrollMetrics position, float velocity) {
            D.assert(
                position is _FixedExtentScrollPosition,
                () => "FixedExtentScrollPhysics can only be used with Scrollables that uses " +
                      "the FixedExtentScrollController"
            );

            var metrics = (_FixedExtentScrollPosition) position;

            if (velocity <= 0.0f && metrics.pixels <= metrics.minScrollExtent ||
                velocity >= 0.0f && metrics.pixels >= metrics.maxScrollExtent) {
                return base.createBallisticSimulation(position: metrics, velocity: velocity);
            }

            var testFrictionSimulation =
                base.createBallisticSimulation(position: metrics, velocity: velocity);

            if (testFrictionSimulation != null
                && (testFrictionSimulation.x(time: float.PositiveInfinity) == metrics.minScrollExtent
                    || testFrictionSimulation.x(time: float.PositiveInfinity) == metrics.maxScrollExtent)) {
                return base.createBallisticSimulation(position: metrics, velocity: velocity);
            }

            var settlingItemIndex = ListWheelScrollViewUtils._getItemFromOffset(
                testFrictionSimulation?.x(time: float.PositiveInfinity) ?? metrics.pixels,
                itemExtent: metrics.itemExtent,
                minScrollExtent: metrics.minScrollExtent,
                maxScrollExtent: metrics.maxScrollExtent
            );

            var settlingPixels = settlingItemIndex * metrics.itemExtent;

            if (velocity.abs() < tolerance.velocity
                && (settlingPixels - metrics.pixels).abs() < tolerance.distance) {
                return null;
            }

            if (settlingItemIndex == metrics.itemIndex) {
                return new SpringSimulation(
                    spring: spring,
                    start: metrics.pixels,
                    end: settlingPixels,
                    velocity: velocity,
                    tolerance: tolerance
                );
            }

            return FrictionSimulation.through(
                startPosition: metrics.pixels,
                endPosition: settlingPixels,
                startVelocity: velocity,
                tolerance.velocity * velocity.sign()
            );
        }
    }

    public class ListWheelScrollView : StatefulWidget {
        public readonly bool clipToSize;

        public readonly ScrollController controller;
        public readonly float diameterRatio;
        public readonly float itemExtent;
        public readonly float magnification;
        public readonly float offAxisFraction;
        public readonly ValueChanged<int> onSelectedItemChanged;
        public readonly float overAndUnderCenterOpacity;
        public readonly float perspective;
        public readonly ScrollPhysics physics;
        public readonly bool renderChildrenOutsideViewport;
        public readonly float squeeze;
        public readonly bool useMagnifier;
        public ListWheelChildDelegate childDelegate;

        public ListWheelScrollView(
            List<Widget> children,
            float itemExtent,
            Key key = null,
            ScrollController controller = null,
            ScrollPhysics physics = null,
            float diameterRatio = RenderListWheelViewport.defaultDiameterRatio,
            float perspective = RenderListWheelViewport.defaultPerspective,
            float offAxisFraction = 0.0f,
            bool useMagnifier = false,
            float magnification = 1.0f,
            float overAndUnderCenterOpacity = 1.0f,
            float squeeze = 1.0f,
            ValueChanged<int> onSelectedItemChanged = null,
            bool clipToSize = true,
            bool renderChildrenOutsideViewport = false
        ) : base(key: key) {
            //D.assert(children != null);
            D.assert(diameterRatio > 0.0, () => RenderListWheelViewport.diameterRatioZeroMessage);
            D.assert(perspective > 0);
            D.assert(perspective <= 0.01f, () => RenderListWheelViewport.perspectiveTooHighMessage);
            D.assert(magnification > 0);
            D.assert(overAndUnderCenterOpacity >= 0 && overAndUnderCenterOpacity <= 1);
            D.assert(itemExtent > 0);
            D.assert(squeeze > 0);
            D.assert(
                !renderChildrenOutsideViewport || !clipToSize,
                () => RenderListWheelViewport.clipToSizeAndRenderChildrenOutsideViewportConflict
            );

            childDelegate = new ListWheelChildListDelegate(children: children);
            this.overAndUnderCenterOpacity = overAndUnderCenterOpacity;
            this.itemExtent = itemExtent;
            this.squeeze = squeeze;
            this.controller = controller;
            this.physics = physics;
            this.diameterRatio = diameterRatio;
            this.perspective = perspective;
            this.offAxisFraction = offAxisFraction;
            this.useMagnifier = useMagnifier;
            this.magnification = magnification;
            this.onSelectedItemChanged = onSelectedItemChanged;
            this.clipToSize = clipToSize;
            this.renderChildrenOutsideViewport = renderChildrenOutsideViewport;
        }

        public ListWheelScrollView(
            float itemExtent,
            Key key = null,
            ScrollController controller = null,
            ScrollPhysics physics = null,
            float diameterRatio = RenderListWheelViewport.defaultDiameterRatio,
            float perspective = RenderListWheelViewport.defaultPerspective,
            float offAxisFraction = 0.0f,
            bool useMagnifier = false,
            float magnification = 1.0f,
            float squeeze = 1.0f,
            ValueChanged<int> onSelectedItemChanged = null,
            float overAndUnderCenterOpacity = 1.0f,
            bool clipToSize = true,
            bool renderChildrenOutsideViewport = false,
            ListWheelChildDelegate childDelegate = null
        ) : base(key: key) {
            D.assert(childDelegate != null);
            D.assert(diameterRatio > 0.0, () => RenderListWheelViewport.diameterRatioZeroMessage);
            D.assert(perspective > 0);
            D.assert(perspective <= 0.01, () => RenderListWheelViewport.perspectiveTooHighMessage);
            D.assert(magnification > 0);
            D.assert(overAndUnderCenterOpacity >= 0 && overAndUnderCenterOpacity <= 1);
            D.assert(itemExtent > 0);
            D.assert(squeeze > 0);
            D.assert(
                !renderChildrenOutsideViewport || !clipToSize, () =>
                    RenderListWheelViewport.clipToSizeAndRenderChildrenOutsideViewportConflict
            );
            this.controller = controller;
            this.physics = physics;
            this.diameterRatio = diameterRatio;
            this.perspective = perspective;
            this.offAxisFraction = offAxisFraction;
            this.useMagnifier = useMagnifier;
            this.magnification = magnification;
            this.overAndUnderCenterOpacity = overAndUnderCenterOpacity;
            this.itemExtent = itemExtent;
            this.squeeze = squeeze;
            this.onSelectedItemChanged = onSelectedItemChanged;
            this.clipToSize = clipToSize;
            this.renderChildrenOutsideViewport = renderChildrenOutsideViewport;
            this.childDelegate = childDelegate;
        }

        public override State createState() {
            return new _ListWheelScrollViewState();
        }
    }

    class _ListWheelScrollViewState : State<ListWheelScrollView> {
        int _lastReportedItemIndex;
        ScrollController scrollController;

        public override void initState() {
            base.initState();
            scrollController = widget.controller ?? new FixedExtentScrollController();
            if (widget.controller is FixedExtentScrollController controller) {
                _lastReportedItemIndex = controller.initialItem;
            }
        }

        public override void didUpdateWidget(StatefulWidget oldWidget) {
            base.didUpdateWidget(oldWidget: oldWidget);
            if (widget.controller != null && widget.controller != scrollController) {
                var oldScrollController = scrollController;
                SchedulerBinding.instance.addPostFrameCallback(_ => { oldScrollController.dispose(); });
                scrollController = widget.controller;
            }
        }

        public override Widget build(BuildContext context) {
            return new NotificationListener<ScrollNotification>(
                onNotification: notification => {
                    if (notification.depth == 0
                        && widget.onSelectedItemChanged != null
                        && notification is ScrollUpdateNotification
                        && notification.metrics is FixedExtentMetrics metrics) {
                        var currentItemIndex = metrics.itemIndex;
                        if (currentItemIndex != _lastReportedItemIndex) {
                            _lastReportedItemIndex = currentItemIndex;
                            var trueIndex = widget.childDelegate.trueIndexOf(index: currentItemIndex);
                            widget.onSelectedItemChanged(value: trueIndex);
                        }
                    }

                    return false;
                },
                child: new _FixedExtentScrollable(
                    controller: scrollController,
                    physics: widget.physics,
                    itemExtent: widget.itemExtent,
                    viewportBuilder: (_context, _offset) => {
                        return new ListWheelViewport(
                            diameterRatio: widget.diameterRatio,
                            perspective: widget.perspective,
                            offAxisFraction: widget.offAxisFraction,
                            useMagnifier: widget.useMagnifier,
                            magnification: widget.magnification,
                            overAndUnderCenterOpacity: widget.overAndUnderCenterOpacity,
                            itemExtent: widget.itemExtent,
                            squeeze: widget.squeeze,
                            clipToSize: widget.clipToSize,
                            renderChildrenOutsideViewport: widget.renderChildrenOutsideViewport,
                            offset: _offset,
                            childDelegate: widget.childDelegate
                        );
                    }
                )
            );
        }
    }

    public class ListWheelElement : RenderObjectElement, IListWheelChildManager {
        readonly SplayTree<int, Element> _childElements = new SplayTree<int, Element>();


        readonly Dictionary<int, Widget> _childWidgets = new Dictionary<int, Widget>();

        public ListWheelElement(ListWheelViewport widget) : base(widget: widget) {
        }

        public new ListWheelViewport widget {
            get { return (ListWheelViewport) base.widget; }
        }

        public new RenderListWheelViewport renderObject {
            get { return (RenderListWheelViewport) base.renderObject; }
        }

        public int? childCount {
            get { return widget.childDelegate.estimatedChildCount; }
        }

        public bool childExistsAt(int index) {
            return retrieveWidget(index: index) != null;
        }

        public void createChild(int index, RenderBox after) {
            owner.buildScope(this, () => {
                var insertFirst = after == null;
                D.assert(insertFirst || _childElements.getOrDefault(index - 1) != null);
                var newChild = updateChild(_childElements.getOrDefault(key: index), retrieveWidget(index: index),
                    newSlot: index);
                if (newChild != null) {
                    _childElements[key: index] = newChild;
                }
                else {
                    _childElements.Remove(key: index);
                }
            });
        }

        public void removeChild(RenderBox child) {
            var index = renderObject.indexOf(child: child);
            owner.buildScope(this, () => {
                D.assert(_childElements.ContainsKey(key: index));
                var result = updateChild(_childElements[key: index], null, newSlot: index);
                D.assert(result == null);
                _childElements.Remove(key: index);
                D.assert(!_childElements.ContainsKey(key: index));
            });
        }

        public override void update(Widget newWidget) {
            var oldWidget = widget;
            base.update(newWidget: newWidget);
            var newDelegate = ((ListWheelViewport) newWidget).childDelegate;
            var oldDelegate = oldWidget.childDelegate;
            if (newDelegate != oldDelegate &&
                (newDelegate.GetType() != oldDelegate.GetType() ||
                 newDelegate.shouldRebuild(oldDelegate: oldDelegate))) {
                performRebuild();
            }
        }

        protected override void performRebuild() {
            _childWidgets.Clear();
            base.performRebuild();
            if (_childElements.isEmpty()) {
                return;
            }

            var firstIndex = _childElements.First()?.Key ?? 0;
            var lastIndex = _childElements.Last()?.Key ?? _childElements.Count;

            for (var index = firstIndex; index <= lastIndex; ++index) {
                var newChild = updateChild(_childElements[key: index], retrieveWidget(index: index), newSlot: index);
                if (newChild != null) {
                    _childElements[key: index] = newChild;
                }
                else {
                    _childElements.Remove(key: index);
                }
            }
        }

        Widget retrieveWidget(int index) {
            return _childWidgets.putIfAbsent(key: index,
                () => { return widget.childDelegate.build(this, index: index); });
        }

        protected override Element updateChild(Element child, Widget newWidget, object newSlot) {
            var oldParentData = (ListWheelParentData) child?.renderObject?.parentData;
            var newChild = base.updateChild(child: child, newWidget: newWidget, newSlot: newSlot);
            var newParentData = (ListWheelParentData) newChild?.renderObject?.parentData;
            if (newParentData != null) {
                newParentData.index = (int) newSlot;
                if (oldParentData != null) {
                    newParentData.offset = oldParentData.offset;
                }
            }

            return newChild;
        }

        protected override void insertChildRenderObject(RenderObject child, object slot) {
            var renderObject = this.renderObject;
            D.assert(renderObject.debugValidateChild(child: child));
            var slotNum = (int) slot;
            renderObject.insert(child as RenderBox,
                _childElements.getOrDefault(slotNum - 1)?.renderObject as RenderBox);
            D.assert(renderObject == this.renderObject);
        }

        protected override void moveChildRenderObject(RenderObject child, dynamic slot) {
            var moveChildRenderObjectErrorMessage =
                "Currently we maintain the list in contiguous increasing order, so " +
                "moving children around is not allowed.";
            D.assert(false, () => moveChildRenderObjectErrorMessage);
        }

        protected override void removeChildRenderObject(RenderObject child) {
            D.assert(child.parent == renderObject);
            renderObject.remove((RenderBox) child);
        }

        public override void visitChildren(ElementVisitor visitor) {
            foreach (var item in _childElements) {
                visitor(element: item.Value);
            }
        }

        public override void forgetChild(Element child) {
            _childElements.Remove((int) (child.slot));
            base.forgetChild(child);
        }
    }

    public class ListWheelViewport : RenderObjectWidget {
        public readonly ListWheelChildDelegate childDelegate;
        public readonly bool clipToSize;

        public readonly float diameterRatio;
        public readonly float? itemExtent;
        public readonly float magnification;
        public readonly float offAxisFraction;
        public readonly ViewportOffset offset;
        public readonly float overAndUnderCenterOpacity;
        public readonly float perspective;
        public readonly bool renderChildrenOutsideViewport;
        public readonly float squeeze;
        public readonly bool useMagnifier;

        public ListWheelViewport(
            Key key = null,
            float diameterRatio = RenderListWheelViewport.defaultDiameterRatio,
            float perspective = RenderListWheelViewport.defaultPerspective,
            float offAxisFraction = 0.0f,
            bool useMagnifier = false,
            float magnification = 1.0f,
            float overAndUnderCenterOpacity = 1.0f,
            float? itemExtent = null,
            float squeeze = 1.0f,
            bool clipToSize = true,
            bool renderChildrenOutsideViewport = false,
            ViewportOffset offset = null,
            ListWheelChildDelegate childDelegate = null
        ) : base(key: key) {
            D.assert(childDelegate != null);
            D.assert(offset != null);
            D.assert(diameterRatio > 0, () => RenderListWheelViewport.diameterRatioZeroMessage);
            D.assert(perspective > 0);
            D.assert(perspective <= 0.01, () => RenderListWheelViewport.perspectiveTooHighMessage);
            D.assert(overAndUnderCenterOpacity >= 0 && overAndUnderCenterOpacity <= 1);
            D.assert(itemExtent != null);
            D.assert(itemExtent > 0);
            D.assert(
                !renderChildrenOutsideViewport || !clipToSize,
                () => RenderListWheelViewport.clipToSizeAndRenderChildrenOutsideViewportConflict
            );
            this.diameterRatio = diameterRatio;
            this.perspective = perspective;
            this.offAxisFraction = offAxisFraction;
            this.useMagnifier = useMagnifier;
            this.magnification = magnification;
            this.overAndUnderCenterOpacity = overAndUnderCenterOpacity;
            this.itemExtent = itemExtent;
            this.squeeze = squeeze;
            this.clipToSize = clipToSize;
            this.renderChildrenOutsideViewport = renderChildrenOutsideViewport;
            this.offset = offset;
            this.childDelegate = childDelegate;
        }

        public override Element createElement() {
            return new ListWheelElement(this);
        }

        public override RenderObject createRenderObject(BuildContext context) {
            var childManager = (ListWheelElement) context;
            return new RenderListWheelViewport(
                childManager: childManager,
                offset: offset,
                diameterRatio: diameterRatio,
                perspective: perspective,
                offAxisFraction: offAxisFraction,
                useMagnifier: useMagnifier,
                magnification: magnification,
                overAndUnderCenterOpacity: overAndUnderCenterOpacity,
                itemExtent: itemExtent ?? 1.0f,
                squeeze: squeeze,
                clipToSize: clipToSize,
                renderChildrenOutsideViewport: renderChildrenOutsideViewport
            );
        }

        public override void updateRenderObject(BuildContext context, RenderObject renderObject) {
            var viewport = (RenderListWheelViewport) renderObject;
            viewport.offset = offset;
            viewport.diameterRatio = diameterRatio;
            viewport.perspective = perspective;
            viewport.offAxisFraction = offAxisFraction;
            viewport.useMagnifier = useMagnifier;
            viewport.magnification = magnification;
            viewport.overAndUnderCenterOpacity = overAndUnderCenterOpacity;
            viewport.itemExtent = itemExtent ?? 1.0f;
            viewport.squeeze = squeeze;
            viewport.clipToSize = clipToSize;
            viewport.renderChildrenOutsideViewport = renderChildrenOutsideViewport;
        }
    }
}