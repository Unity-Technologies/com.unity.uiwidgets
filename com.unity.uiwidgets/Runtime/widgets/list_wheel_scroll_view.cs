using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using Unity.UIWidgets.animation;
using Unity.UIWidgets.async;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.physics;
using Unity.UIWidgets.rendering;
using Unity.UIWidgets.external;
using Unity.UIWidgets.scheduler;
using Unity.UIWidgets.ui;
using UnityEngine;

namespace Unity.UIWidgets.widgets {
    public interface ListWheelChildDelegate {
        Widget build(BuildContext context, int index);
        int? estimatedChildCount { get; }
        int trueIndexOf(int index);
        bool shouldRebuild(ListWheelChildDelegate oldDelegate);
    }

    public class ListWheelChildListDelegate : ListWheelChildDelegate {
        public ListWheelChildListDelegate(
            List<Widget> children
        ) {
            D.assert(children != null);
            this.children = children;
        }

        public readonly List<Widget> children;

        public int? estimatedChildCount {
            get { return children.Count; }
        }

        public Widget build(BuildContext context, int index) {
            if (index < 0 || index >= children.Count) {
                return null;
            }

            return new IndexedSemantics(child: children[index], index: index);
        }

        public int trueIndexOf(int index) {
            return index;
        }

        public bool shouldRebuild(ListWheelChildDelegate oldDelegate) {
            return children != ((ListWheelChildListDelegate) oldDelegate).children;
        }
    }

    public class ListWheelChildLoopingListDelegate : ListWheelChildDelegate {
        public ListWheelChildLoopingListDelegate(
            List<Widget> children
        ) {
            D.assert(children != null);
            this.children = children;
        }

        public readonly List<Widget> children;

        public int? estimatedChildCount {
            get { return null; }
        }

        public int trueIndexOf(int index) {
            var result = index;
            if (index < 0) {
                if (index % children.Count == 0)
                    result = 0;
                else {
                    result = index % children.Count + children.Count;
                }
            }
            else 
                result = index % children.Count;
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
        public ListWheelChildBuilderDelegate(
            IndexedWidgetBuilder builder,
            int? childCount = null
        ) {
            D.assert(builder != null);
            this.builder = builder;
            this.childCount = childCount;
        }

        public readonly IndexedWidgetBuilder builder;

        public readonly int? childCount;

        public int? estimatedChildCount {
            get { return childCount; }
        }

        public Widget build(BuildContext context, int index) {
            if (childCount == null) {
                Widget child = builder(context, index);
                return child == null ? null : new IndexedSemantics(child: child);
            }

            if (index < 0 || index >= childCount) {
                return null;
            }

            return new IndexedSemantics(child: builder(context, index));
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
            float offset ,
            float itemExtent ,
            float minScrollExtent ,
            float maxScrollExtent 
        ) {
            return (_clipOffsetToScrollableRange(offset, minScrollExtent, maxScrollExtent) / itemExtent).round();
        }
        public static float _clipOffsetToScrollableRange(
            float offset,
            float minScrollExtent,
            float maxScrollExtent
        ) {
            return Mathf.Min(Mathf.Max(offset, minScrollExtent), maxScrollExtent);
        }
    }

    public class FixedExtentScrollController : ScrollController {
        public FixedExtentScrollController(
            int initialItem = 0
        ) {
            this.initialItem = initialItem;
        }

        public readonly int initialItem;

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
                _FixedExtentScrollPosition position = (_FixedExtentScrollPosition) this.position; 
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
            List<Future> futures = new List<Future>();
            
            foreach (_FixedExtentScrollPosition position in positions.Cast<_FixedExtentScrollPosition>()) {
                futures.Add(position.animateTo(
                    itemIndex * position.itemExtent,
                    duration: duration,
                    curve: curve
                ));
            }
            return Future.wait<object>(futures);
        }

        public void jumpToItem(int itemIndex) {
            foreach (_FixedExtentScrollPosition position in positions.Cast<_FixedExtentScrollPosition>()) {
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
            float minScrollExtent ,
            float maxScrollExtent,
            float pixels ,
            float viewportDimension ,
            AxisDirection axisDirection ,
            int itemIndex 
        ) : base(
            minScrollExtent: minScrollExtent ,
            maxScrollExtent: maxScrollExtent ,
            pixels: pixels ,
            viewportDimension: viewportDimension ,
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
                minScrollExtent: minScrollExtent ?? this.minScrollExtent,
                maxScrollExtent: maxScrollExtent ?? this.maxScrollExtent,
                pixels: pixels ?? this.pixels,
                viewportDimension: viewportDimension ?? this.viewportDimension,
                axisDirection: axisDirection ?? this.axisDirection,
                itemIndex: itemIndex ?? this.itemIndex
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
            initialPixels: _getItemExtentFromScrollContext(context) * (initialItem ?? 0.0f),
            keepScrollOffset: keepScrollOffset,
            oldPosition: oldPosition,
            debugLabel: debugLabel
        ) {
            D.assert(
                context is _FixedExtentScrollableState,
                () => "FixedExtentScrollController can only be used with ListWheelScrollViews"
            );
        }

        static float _getItemExtentFromScrollContext(ScrollContext context) {
            _FixedExtentScrollableState scrollable = (_FixedExtentScrollableState) context;
            return scrollable.itemExtent;
        }

        public float itemExtent {
            get { return _getItemExtentFromScrollContext(context); }
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
                minScrollExtent: minScrollExtent ?? this.minScrollExtent,
                maxScrollExtent: maxScrollExtent ?? this.maxScrollExtent,
                pixels: pixels ?? this.pixels,
                viewportDimension: viewportDimension ?? this.viewportDimension,
                axisDirection: axisDirection ?? this.axisDirection,
                itemIndex: itemIndex ?? this.itemIndex
            );
        }
    }

    class _FixedExtentScrollable : Scrollable {
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

        public readonly float itemExtent;

        public override State createState() {
            return new _FixedExtentScrollableState();
        }
    }

    class _FixedExtentScrollableState : ScrollableState {
        public float itemExtent {
            get {
                _FixedExtentScrollable actualWidget = (_FixedExtentScrollable) widget;
                return actualWidget.itemExtent;
            }
        }
    }


    public class FixedExtentScrollPhysics : ScrollPhysics {
        public FixedExtentScrollPhysics(
            ScrollPhysics parent = null
        ) : base(parent: parent) { }

        public override ScrollPhysics applyTo(ScrollPhysics ancestor) {
            return new FixedExtentScrollPhysics(parent: buildParent(ancestor));
        }

        public override Simulation createBallisticSimulation(ScrollMetrics position, float velocity) {
            D.assert(
                position is _FixedExtentScrollPosition,
                () => "FixedExtentScrollPhysics can only be used with Scrollables that uses " +
                      "the FixedExtentScrollController"
            );

            _FixedExtentScrollPosition metrics = (_FixedExtentScrollPosition) position;

            if ((velocity <= 0.0f && metrics.pixels <= metrics.minScrollExtent) ||
                (velocity >= 0.0f && metrics.pixels >= metrics.maxScrollExtent)) {
                return base.createBallisticSimulation(metrics, velocity);
            }

            Simulation testFrictionSimulation =
                base.createBallisticSimulation(metrics, velocity);

            if (testFrictionSimulation != null
                && (testFrictionSimulation.x(float.PositiveInfinity) == metrics.minScrollExtent
                    || testFrictionSimulation.x(float.PositiveInfinity) == metrics.maxScrollExtent)) {
                return base.createBallisticSimulation(metrics, velocity);
            }

            int settlingItemIndex = ListWheelScrollViewUtils._getItemFromOffset(
                offset: testFrictionSimulation?.x(float.PositiveInfinity) ?? metrics.pixels,
                itemExtent: metrics.itemExtent,
                minScrollExtent: metrics.minScrollExtent,
                maxScrollExtent: metrics.maxScrollExtent
            );

            float settlingPixels = settlingItemIndex * metrics.itemExtent;

            if (velocity.abs() < tolerance.velocity
                && (settlingPixels - metrics.pixels).abs() < tolerance.distance) {
                return null;
            }

            if (settlingItemIndex == metrics.itemIndex) {
                return new SpringSimulation(
                    spring,
                    metrics.pixels,
                    settlingPixels,
                    velocity,
                    tolerance: tolerance
                );
            }

            return FrictionSimulation.through(
                metrics.pixels,
                settlingPixels,
                velocity, 
                tolerance.velocity * velocity.sign()
            );
        }
    }

    public class ListWheelScrollView : StatefulWidget {
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

        public ListWheelScrollView (
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
            D.assert(diameterRatio > 0.0, ()=>RenderListWheelViewport.diameterRatioZeroMessage);
            D.assert(perspective > 0);
            D.assert(perspective <= 0.01,()=> RenderListWheelViewport.perspectiveTooHighMessage);
            D.assert(magnification > 0);
            D.assert(overAndUnderCenterOpacity >= 0 && overAndUnderCenterOpacity <= 1);
            D.assert(itemExtent != null);
            D.assert(itemExtent > 0);
            D.assert(squeeze > 0);
            D.assert(
                !renderChildrenOutsideViewport || !clipToSize,()=>
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

        public readonly ScrollController controller;
        public readonly ScrollPhysics physics;
        public readonly float diameterRatio;
        public readonly float perspective;
        public readonly float offAxisFraction;
        public readonly bool useMagnifier;
        public readonly float magnification;
        public readonly float itemExtent;
        public readonly float overAndUnderCenterOpacity;
        public readonly float squeeze;
        public readonly ValueChanged<int> onSelectedItemChanged;
        public readonly bool clipToSize;
        public readonly bool renderChildrenOutsideViewport;
        public ListWheelChildDelegate childDelegate;

        public override State createState() {
            return new _ListWheelScrollViewState();
        }
    }

    class _ListWheelScrollViewState : State<ListWheelScrollView> {
        int _lastReportedItemIndex = 0;
        ScrollController scrollController;

        public override void initState() {
            base.initState();
            scrollController = widget.controller ?? new FixedExtentScrollController();
            if (widget.controller is FixedExtentScrollController controller) {
                _lastReportedItemIndex = controller.initialItem;
            }
        }

        public override void didUpdateWidget(StatefulWidget oldWidget) {
            base.didUpdateWidget(oldWidget);
            if (widget.controller != null && widget.controller != scrollController) {
                ScrollController oldScrollController = scrollController;
                SchedulerBinding.instance.addPostFrameCallback((_) => { oldScrollController.dispose(); });
                scrollController = widget.controller;
            }
        }

        public override Widget build(BuildContext context) {
            return new NotificationListener<ScrollNotification>(
                onNotification: (ScrollNotification notification) => {
                    if (notification.depth == 0
                        && widget.onSelectedItemChanged != null
                        && notification is ScrollUpdateNotification
                        && notification.metrics is FixedExtentMetrics metrics) {
                        int currentItemIndex = metrics.itemIndex;
                        if (currentItemIndex != _lastReportedItemIndex) {
                            _lastReportedItemIndex = currentItemIndex;
                            int trueIndex = widget.childDelegate.trueIndexOf(currentItemIndex);
                            widget.onSelectedItemChanged(trueIndex);
                        }
                    }

                    return false;
                },
                child: new _FixedExtentScrollable(
                    controller: scrollController,
                    physics: widget.physics,
                    itemExtent: widget.itemExtent,
                    viewportBuilder: (BuildContext _context, ViewportOffset _offset) => {
                        return new ListWheelViewport(
                            diameterRatio: widget.diameterRatio,
                            perspective: widget.perspective,
                            offAxisFraction: widget.offAxisFraction,
                            useMagnifier: widget.useMagnifier,
                            magnification: widget.magnification,
                            overAndUnderCenterOpacity: widget.overAndUnderCenterOpacity,
                            itemExtent: widget.itemExtent ,
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
        public ListWheelElement(ListWheelViewport widget) : base(widget) { }

        public new ListWheelViewport widget {
            get { return (ListWheelViewport) base.widget; }
        }

        public new RenderListWheelViewport renderObject {
            get { return (RenderListWheelViewport)base.renderObject; }
        }


        readonly Dictionary<int, Widget> _childWidgets = new Dictionary<int, Widget>();

        readonly SplayTree<int, Element> _childElements = new SplayTree<int, Element>();

        public override void update(Widget newWidget) {
            ListWheelViewport oldWidget = widget;
            base.update(newWidget);
            ListWheelChildDelegate newDelegate = ((ListWheelViewport) newWidget).childDelegate;
            ListWheelChildDelegate oldDelegate = oldWidget.childDelegate;
            if (newDelegate != oldDelegate &&
                (newDelegate.GetType() != oldDelegate.GetType() || newDelegate.shouldRebuild(oldDelegate))) {
                performRebuild();
            }
        }

        public int? childCount {
            get { return widget.childDelegate.estimatedChildCount; }
        }

        protected override void performRebuild() {
            _childWidgets.Clear();
            base.performRebuild();
            if (_childElements.isEmpty()) {
                return;
            }

            int firstIndex = _childElements.First()?.Key ?? 0;
            int lastIndex = _childElements.Last()?.Key ?? _childElements.Count;

            for (int index = firstIndex; index <= lastIndex; ++index) {
                Element newChild = updateChild(_childElements[index], retrieveWidget(index), index);
                if (newChild != null) {
                    _childElements[index] = newChild;
                }
                else {
                    _childElements.Remove(index);
                }
            }
        }

        Widget retrieveWidget(int index) {
            return _childWidgets.putIfAbsent(index,
                () => { return widget.childDelegate.build(this, index); });
        }

        public bool childExistsAt(int index) {
            return retrieveWidget(index) != null;
        }

        public void createChild(int index, RenderBox after) {
            owner.buildScope(this, () => {
                bool insertFirst = after == null;
                D.assert(insertFirst || _childElements.getOrDefault(index - 1) != null);
                Element newChild = updateChild(_childElements.getOrDefault(index), retrieveWidget(index), index);
                if (newChild != null) {
                    _childElements[index] = newChild;
                }
                else {
                    _childElements.Remove(index);
                }
            });
        }

        public void removeChild(RenderBox child) {
            int index = renderObject.indexOf(child);
            owner.buildScope(this, () => {
                D.assert(_childElements.ContainsKey(index));
                Element result = updateChild(_childElements[index], null, index);
                D.assert(result == null);
                _childElements.Remove(index);
                D.assert(!_childElements.ContainsKey(index));
            });
        }

        protected override Element updateChild(Element child, Widget newWidget, object newSlot) {
            ListWheelParentData oldParentData = (ListWheelParentData) child?.renderObject?.parentData;
            Element newChild = base.updateChild(child, newWidget, newSlot);
            ListWheelParentData newParentData = (ListWheelParentData) newChild?.renderObject?.parentData;
            if (newParentData != null) {
                newParentData.index = (int) newSlot;
                if (oldParentData != null) {
                    newParentData.offset = oldParentData.offset;
                }
            }

            return newChild;
        }

        protected override void insertChildRenderObject(RenderObject child, object slot) {
            RenderListWheelViewport renderObject = this.renderObject;
            D.assert(renderObject.debugValidateChild(child));
            int slotNum = (int) slot;
            renderObject.insert(child as RenderBox, after: _childElements.getOrDefault(slotNum - 1)?.renderObject as RenderBox);
            D.assert(renderObject == this.renderObject);
        }

        protected override void moveChildRenderObject(RenderObject child, dynamic slot) { 
            string moveChildRenderObjectErrorMessage =
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
                visitor(item.Value);
            }
        }

        internal override void forgetChild(Element child) {
            _childElements.Remove((int) (child.slot));
            base.forgetChild(child);
        }
    }

    public class ListWheelViewport : RenderObjectWidget {
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

        public readonly float diameterRatio;
        public readonly float perspective;
        public readonly float offAxisFraction;
        public readonly bool useMagnifier;
        public readonly float magnification;
        public readonly float overAndUnderCenterOpacity;
        public readonly float? itemExtent;
        public readonly float squeeze;
        public readonly bool clipToSize;
        public readonly bool renderChildrenOutsideViewport;
        public readonly ViewportOffset offset;
        public readonly ListWheelChildDelegate childDelegate;

        public override Element createElement() {
            return new ListWheelElement(this);
        }

        public override RenderObject createRenderObject(BuildContext context) {
            ListWheelElement childManager = (ListWheelElement) context;
            return new RenderListWheelViewport(
                childManager: childManager,
                offset: offset,
                diameterRatio: diameterRatio,
                perspective: perspective,
                offAxisFraction: offAxisFraction,
                useMagnifier: useMagnifier,
                magnification: magnification,
                overAndUnderCenterOpacity: overAndUnderCenterOpacity,
                itemExtent: itemExtent ?? 1.0f ,
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