using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using com.unity.uiwidgets.Runtime.rendering;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.rendering;
using Unity.UIWidgets.external;
using Unity.UIWidgets.ui;
    

namespace Unity.UIWidgets.widgets {
    public abstract class SliverChildDelegate {
        protected SliverChildDelegate() {
        }

        public abstract Widget build(BuildContext context, int index);

        public virtual int? estimatedChildCount {
            get { return null; }
        }

        public virtual float? estimateMaxScrollOffset(
            int firstIndex,
            int lastIndex,
            float leadingScrollOffset,
            float trailingScrollOffset
        ) {
            return null;
        }

        public virtual void didFinishLayout(int firstIndex, int lastIndex) {
        }

        public abstract bool shouldRebuild(SliverChildDelegate oldDelegate);

        public virtual int? findIndexByKey(Key key) {
            return null;
        }

        public override string ToString() {
            var description = new List<string>();
            debugFillDescription(description);
            return $"{foundation_.describeIdentity(this)}({string.Join(", ", description.ToArray())})";
        }

        protected virtual void debugFillDescription(List<string> description) {
            try {
                var children = estimatedChildCount;
                if (children != null) {
                    description.Add("estimated child count: " + children);
                }
            }
            catch (Exception ex) {
                description.Add("estimated child count: EXCEPTION (" + ex.GetType() + ")");
            }
        }
    }

    public delegate int ChildIndexGetter(Key key);
    
    public class _SaltedValueKey : ValueKey<Key>{
        public _SaltedValueKey(Key key) : base(key) {
            D.assert(key != null);
        }
    }


    public class SliverChildBuilderDelegate : SliverChildDelegate {
        public SliverChildBuilderDelegate(
            IndexedWidgetBuilder builder,
            ChildIndexGetter findChildIndexCallback = null,
            int? childCount = null,
            bool addAutomaticKeepAlives = true,
            bool addRepaintBoundaries = true
        ) {
            D.assert(builder != null);
            this.builder = builder;
            this.findChildIndexCallback = findChildIndexCallback;
            this.childCount = childCount;
            this.addAutomaticKeepAlives = addAutomaticKeepAlives;
            this.addRepaintBoundaries = addRepaintBoundaries;
        }

        public readonly IndexedWidgetBuilder builder;

        public readonly int? childCount;

        public readonly bool addAutomaticKeepAlives;

        public readonly bool addRepaintBoundaries;
        
        public readonly ChildIndexGetter findChildIndexCallback;
        
        public override int? findIndexByKey(Key key) {
            if (findChildIndexCallback == null)
                return null;
            D.assert(key != null);
            Key childKey;
            if (key is _SaltedValueKey) {
                _SaltedValueKey saltedValueKey = (_SaltedValueKey)key;
                childKey = saltedValueKey.value;
            } else {
                childKey = key;
            }
            return findChildIndexCallback(childKey);
        }

        public override Widget build(BuildContext context, int index) {
            D.assert(builder != null);
            if (index < 0 || (childCount != null && index >= childCount))
                return null;
            Widget child = builder(context, index);
            if (child == null)
                return null;
            Key key = child.key != null ? new _SaltedValueKey(child.key) : null;
            if (addRepaintBoundaries)
                child = new RepaintBoundary(child: child);
            if (addAutomaticKeepAlives)
                child = new AutomaticKeepAlive(child: child);
            return new KeyedSubtree(child: child, key: key);
        }

        public override int? estimatedChildCount {
            get { return childCount; }
        }

        public override bool shouldRebuild(SliverChildDelegate oldDelegate) {
            return true;
        }
    }

    public class SliverChildListDelegate : SliverChildDelegate {
        static readonly Key _defaultNullKey = Key.key("SliverChildListDelegate##DefaultNullKey");
        
        public SliverChildListDelegate(
            List<Widget> children,
            bool addAutomaticKeepAlives = true,
            bool addRepaintBoundaries = true
        ) {
            D.assert(children != null);
            this.children = children;
            this.addAutomaticKeepAlives = addAutomaticKeepAlives;
            this.addRepaintBoundaries = addRepaintBoundaries;
            _keyToIndex = new Dictionary<Key, int>{{_defaultNullKey, 0}};
        }

        public readonly bool addAutomaticKeepAlives;

        public readonly bool addRepaintBoundaries;

        public readonly List<Widget> children;
            
        public readonly Dictionary<Key, int> _keyToIndex;

        public bool _isConstantInstance {
            get {
                return _keyToIndex == null;
            }
        }
        public int? _findChildIndex(Key key) {
            if (_isConstantInstance) {
                return null;
            }
            if (!_keyToIndex.ContainsKey(key)) {
                int index = _keyToIndex.getOrDefault(_defaultNullKey);
                while (index < children.Count) {
                    Widget child = children[index];
                    if (child.key != null && child.key != _defaultNullKey) {
                        _keyToIndex[child.key] = index;
                    }
                    if (child.key == key) {
                        // Record current index for next function call.
                        _keyToIndex[_defaultNullKey] = index + 1;
                        return index;
                    }
                    index += 1;
                }
                _keyToIndex[_defaultNullKey] = index;
            } else {
                return _keyToIndex[key];
            }
            return null;
        }
        public  override int? findIndexByKey(Key key) {
            D.assert(key != null);
            Key childKey;
            if (key is _SaltedValueKey) {
                _SaltedValueKey saltedValueKey = (_SaltedValueKey)key;
                childKey = saltedValueKey.value;
            } else {
                childKey = key;
            }
            return _findChildIndex(childKey);
        }


        public override Widget build(BuildContext context, int index) {
            D.assert(children != null);
            if (index < 0 || index >= children.Count)
                return null;
            Widget child = children[index];
            
            D.assert(
                child != null,()=>
                    "The sliver's children must not contain null values, but a null value was found at index $index"
            );
            
            Key key = child.key != null? new _SaltedValueKey(child.key) : null;
            if (addRepaintBoundaries)
                child = new RepaintBoundary(child: child);
            if (addAutomaticKeepAlives)
                child = new AutomaticKeepAlive(child: child);
            return new KeyedSubtree(child: child, key: key);
        }

        public override int? estimatedChildCount {
            get { return children.Count; }
        }

        public override bool shouldRebuild(SliverChildDelegate oldDelegate) {
            return children != ((SliverChildListDelegate) oldDelegate).children;
        }
    }

    public abstract class SliverWithKeepAliveWidget : RenderObjectWidget {
        /// Initializes fields for subclasses.
        public SliverWithKeepAliveWidget(Key key = null) : base(key: key) {
        }

        public abstract override RenderObject createRenderObject(BuildContext context);
    }
    public abstract class SliverMultiBoxAdaptorWidget : SliverWithKeepAliveWidget {
        protected SliverMultiBoxAdaptorWidget(
            Key key = null,
            SliverChildDelegate del = null
        ) : base(key: key) {
            D.assert(del != null);
            this.del = del;
        }

        public readonly SliverChildDelegate del;

        public override Element createElement() {
            return new SliverMultiBoxAdaptorElement(this);
        }
        public abstract override RenderObject createRenderObject(BuildContext context);
        public virtual float? estimateMaxScrollOffset(
            SliverConstraints constraints,
            int firstIndex,
            int lastIndex,
            float leadingScrollOffset,
            float trailingScrollOffset
        ) {
            D.assert(lastIndex >= firstIndex);
            return del.estimateMaxScrollOffset(
                firstIndex,
                lastIndex,
                leadingScrollOffset,
                trailingScrollOffset
            );
        }

        public override void debugFillProperties(DiagnosticPropertiesBuilder properties) {
            base.debugFillProperties(properties);
            properties.add(new DiagnosticsProperty<SliverChildDelegate>("del", del));
        }
    }
    public class SliverFixedExtentList : SliverMultiBoxAdaptorWidget {
        public SliverFixedExtentList(
            Key key = null,
            SliverChildDelegate del = null,
            float itemExtent = 0
        ) : base(key: key, del: del) {
            this.itemExtent = itemExtent;
        }

        public readonly float itemExtent;

        public override RenderObject createRenderObject(BuildContext context) {
            SliverMultiBoxAdaptorElement element = (SliverMultiBoxAdaptorElement) context;
            return new RenderSliverFixedExtentList(childManager: element, itemExtent: itemExtent);
        }

        public override void updateRenderObject(BuildContext context, RenderObject renderObjectRaw) {
            var renderObject = (RenderSliverFixedExtentList) renderObjectRaw;
            renderObject.itemExtent = itemExtent;
        }
    }
    
    public class SliverGrid : SliverMultiBoxAdaptorWidget {
        public SliverGrid(
            Key key = null,
            SliverChildDelegate layoutDelegate = null,
            SliverGridDelegate gridDelegate = null
        ) : base(key: key, del: layoutDelegate) {
            D.assert(layoutDelegate != null);
            D.assert(gridDelegate != null);
            this.gridDelegate = gridDelegate;
        }

        public static SliverGrid count(
            Key key = null,
            int? crossAxisCount = null,
            float mainAxisSpacing = 0.0f,
            float crossAxisSpacing = 0.0f,
            float childAspectRatio = 1.0f,
            List<Widget> children = null
        ) {
            SliverGridDelegate gridDelegate = new SliverGridDelegateWithFixedCrossAxisCount(
                crossAxisCount: crossAxisCount ?? 0,
                mainAxisSpacing: mainAxisSpacing,
                crossAxisSpacing: crossAxisSpacing,
                childAspectRatio: childAspectRatio
            );
            return new SliverGrid(
                key: key,
                layoutDelegate: new SliverChildListDelegate(children ?? new List<Widget>()),
                gridDelegate: gridDelegate
                
            );
        }

        public static SliverGrid extent(
            Key key = null,
            float? maxCrossAxisExtent = null,
            float mainAxisSpacing = 0.0f,
            float crossAxisSpacing = 0.0f,
            float childAspectRatio = 1.0f,
            List<Widget> children = null
        ) {
            return new SliverGrid(key: key,
                layoutDelegate: new SliverChildListDelegate(new List<Widget> { }),
                gridDelegate: new SliverGridDelegateWithMaxCrossAxisExtent(
                    maxCrossAxisExtent: maxCrossAxisExtent ?? 0,
                    mainAxisSpacing: mainAxisSpacing,
                    crossAxisSpacing: crossAxisSpacing,
                    childAspectRatio: childAspectRatio
                ));
        }

        public readonly SliverGridDelegate gridDelegate;

        public override RenderObject createRenderObject(BuildContext context) {
            SliverMultiBoxAdaptorElement element = context as SliverMultiBoxAdaptorElement;
            return new RenderSliverGrid(childManager: element, gridDelegate: gridDelegate);
        }

        public override void updateRenderObject(BuildContext context, RenderObject renderObject) {
            (renderObject as RenderSliverGrid).gridDelegate = gridDelegate;
        }

        public override float? estimateMaxScrollOffset(
            SliverConstraints constraints,
            int firstIndex,
            int lastIndex,
            float leadingScrollOffset,
            float trailingScrollOffset
        ) {
            return base.estimateMaxScrollOffset(
                constraints,
                firstIndex,
                lastIndex,
                leadingScrollOffset,
                trailingScrollOffset
            ) ?? gridDelegate.getLayout(constraints)
                .computeMaxScrollOffset(del.estimatedChildCount ?? 0);
        }
    }


    public class SliverIgnorePointer : SingleChildRenderObjectWidget {
        public SliverIgnorePointer(
            Key key = null,
            bool ignoring = true,
            Widget sliver = null
        ) : base(key: key, child: sliver) {
            this.ignoring = ignoring;

        }

        public readonly bool ignoring;

        public override RenderObject  createRenderObject(BuildContext context) {
            return new RenderSliverIgnorePointer(
              ignoring: ignoring
            );
        }
        public override void updateRenderObject(BuildContext context, RenderObject renderObject) {
            renderObject = (RenderSliverIgnorePointer) renderObject;
            ((RenderSliverIgnorePointer)renderObject).ignoring = ignoring;
        }
        public override void debugFillProperties(DiagnosticPropertiesBuilder properties) {
            base.debugFillProperties(properties);
            properties.add(new DiagnosticsProperty<bool>("ignoring", ignoring));
        }
    }
    public class SliverOffstage : SingleChildRenderObjectWidget {
        public SliverOffstage(
        Key key = null,
        bool offstage = true,
        Widget sliver = null) : base(key: key, child: sliver) {
            this.offstage = offstage;
        }


        public readonly bool offstage;

        public override RenderObject createRenderObject(BuildContext context) {
            return new RenderSliverOffstage(offstage: offstage);
        }

        public override void updateRenderObject(BuildContext context, RenderObject renderObject) {
            renderObject = (RenderSliverOffstage) renderObject;
            ((RenderSliverOffstage)renderObject).offstage = offstage;
        }
        public override void debugFillProperties(DiagnosticPropertiesBuilder properties) {
            base.debugFillProperties(properties);
            properties.add(new DiagnosticsProperty<bool>("offstage", offstage));
        }

        public override Element createElement() {
            return new _SliverOffstageElement(this);
        }
    }


    
    public class SliverList : SliverMultiBoxAdaptorWidget {
        public SliverList(
            Key key = null,
            SliverChildDelegate del = null
        ) : base(key: key, del: del) {
        }

        public override RenderObject createRenderObject(BuildContext context) {
            SliverMultiBoxAdaptorElement element = (SliverMultiBoxAdaptorElement) context;
            return new RenderSliverList(childManager: element);
        }
    }
    public class SliverOpacity : SingleChildRenderObjectWidget {
        public SliverOpacity(
            Key key = null,
            float opacity = 0f,
            Widget sliver = null) : base(key: key, child: sliver) { 
            D.assert(opacity >= 0.0 && opacity <= 1.0);
            this.opacity = opacity;
        }

        public readonly float opacity;
        public override RenderObject createRenderObject(BuildContext context) {
            return new RenderSliverOpacity(
              opacity: opacity
            );
        }
        public override void updateRenderObject(BuildContext context, RenderObject renderObject) {
            renderObject = (RenderSliverOpacity) renderObject;
            ((RenderSliverOpacity) renderObject).opacity = opacity;
        }
        public override void debugFillProperties(DiagnosticPropertiesBuilder properties) { 
            base.debugFillProperties(properties);
            properties.add(new DiagnosticsProperty<float>("opacity", opacity));
        }
    }

    
    public class _SliverOffstageElement : SingleChildRenderObjectElement {
        public _SliverOffstageElement(SliverOffstage widget) : base(widget) {
        }
        
        public new SliverOffstage widget {
            get { return base.widget as SliverOffstage; }
        }

        public override void debugVisitOnstageChildren(ElementVisitor visitor) {
            if (!widget.offstage)
                base.debugVisitOnstageChildren(visitor);
        }
    }

    public class SliverMultiBoxAdaptorElement : RenderObjectElement, RenderSliverBoxChildManager {
        public SliverMultiBoxAdaptorElement(SliverMultiBoxAdaptorWidget widget) : base(widget) {
        }

        public new SliverMultiBoxAdaptorWidget widget {
            get { return (SliverMultiBoxAdaptorWidget) base.widget; }
        }

        public new RenderSliverMultiBoxAdaptor renderObject {
            get { return (RenderSliverMultiBoxAdaptor) base.renderObject; }
        }

        public override void update(Widget newWidgetRaw) {
            var newWidget = (SliverMultiBoxAdaptorWidget) newWidgetRaw;
            SliverMultiBoxAdaptorWidget oldWidget = widget;
            base.update(newWidget);
            SliverChildDelegate newDelegate = newWidget.del;
            SliverChildDelegate oldDelegate = oldWidget.del;
            if (newDelegate != oldDelegate &&
                (newDelegate.GetType() != oldDelegate.GetType() || newDelegate.shouldRebuild(oldDelegate))) {
                performRebuild();
            }
        }

        Dictionary<int, Widget> _childWidgets = new Dictionary<int, Widget>();
        SplayTree<int, Element> _childElements = new SplayTree<int, Element>();
        RenderBox _currentBeforeChild;

        protected override void performRebuild() {
            _childWidgets.Clear();
            base.performRebuild();
            _currentBeforeChild = null;
            D.assert(_currentlyUpdatingChildIndex == null);
            
            try {
                SplayTree<int, Element> newChildren = new SplayTree<int, Element>();
                
                Dictionary<int, float> indexToLayoutOffset = new Dictionary<int, float>();
                
                void processElement(int index) {
                    _currentlyUpdatingChildIndex = index;
                    if(_childElements.getOrDefault(index) != null && 
                       _childElements.getOrDefault(index) != newChildren.getOrDefault(index))
                    {
                        _childElements[index] = updateChild(_childElements[index], null, index);
                    }
                    Element newChild = updateChild(newChildren.getOrDefault(index), _build(index), index);
                    if (newChild != null) {
                        _childElements[index] = newChild;
                        SliverMultiBoxAdaptorParentData parentData = newChild.renderObject.parentData as SliverMultiBoxAdaptorParentData;
                        if (index == 0) {
                            parentData.layoutOffset = 0.0f;
                        } else if (indexToLayoutOffset.ContainsKey(index)) {
                            parentData.layoutOffset = indexToLayoutOffset[index];
                        }
                        if (!parentData.keptAlive)
                            _currentBeforeChild = newChild.renderObject as RenderBox;
                    } else {
                        _childElements.Remove(index);
                    }
                }
                
                foreach ( int index in _childElements.Keys.ToList()) {
                     Key key = _childElements[index].widget.key;
                     int? newIndex = key == null ? null : widget.del.findIndexByKey(key);
                     SliverMultiBoxAdaptorParentData childParentData =
                        _childElements[index].renderObject?.parentData as SliverMultiBoxAdaptorParentData;

                    if (childParentData != null && childParentData.layoutOffset != null)
                        indexToLayoutOffset[index] = (float)childParentData.layoutOffset;

                    if (newIndex != null && newIndex != index) {

                        if (childParentData != null)
                            childParentData.layoutOffset = null;

                        newChildren[(int)newIndex] = _childElements[index];
                        
                        newChildren.putIfAbsent(index, () => null);
                      
                        _childElements.Remove(index);
                    } 
                    else {
                        newChildren.putIfAbsent(index, () => _childElements[index]);
                    }
                }

                renderObject.debugChildIntegrityEnabled = false;  // Moving children will temporary violate the integrity.
                foreach (var key in newChildren.Keys) {
                    processElement(key);
                }
                if (_didUnderflow) { 
                    int lastKey = _childElements.Count == 0 ? -1 : _childElements.Keys.Last();
                    int rightBoundary = lastKey + 1;
                    if(newChildren.ContainsKey(rightBoundary))
                        newChildren[rightBoundary] = _childElements.getOrDefault(rightBoundary);
                    processElement(rightBoundary);
                }
            } finally {
                _currentlyUpdatingChildIndex = null;
                renderObject.debugChildIntegrityEnabled = true;
            }
        }

        Widget _build(int index) {
            return _childWidgets.putIfAbsent(index, () => widget.del.build(this, index));
        }

        public void createChild(int index, RenderBox after = null) {
            D.assert(_currentlyUpdatingChildIndex == null);
            owner.buildScope(this, () => {
                bool insertFirst = after == null;
                D.assert(insertFirst || _childElements[index - 1] != null);
                _currentBeforeChild = insertFirst ? null : (RenderBox) _childElements[index - 1].renderObject;
                Element newChild;
                try {
                    _currentlyUpdatingChildIndex = index;
                    _childElements.TryGetValue(index, out newChild);
                    newChild = updateChild(newChild, _build(index), index);
                }
                finally {
                    _currentlyUpdatingChildIndex = null;
                }

                if (newChild != null) {
                    _childElements[index] = newChild;
                }
                else {
                    _childElements.Remove(index);
                }
            });
        }

        protected override Element updateChild(Element child, Widget newWidget, object newSlot) {
            
            SliverMultiBoxAdaptorParentData oldParentData = child?.renderObject?.parentData as SliverMultiBoxAdaptorParentData;
            Element newChild = base.updateChild(child, newWidget, newSlot);
            SliverMultiBoxAdaptorParentData newParentData = newChild?.renderObject?.parentData as SliverMultiBoxAdaptorParentData;

            // Preserve the old layoutOffset if the renderObject was swapped out.
            if (oldParentData != newParentData && oldParentData != null && newParentData != null) {
                newParentData.layoutOffset = oldParentData.layoutOffset;
            }
            return newChild;
            
        }

        public override void forgetChild(Element child) {
            D.assert(child != null);
            D.assert(child.slot != null);
            D.assert(_childElements.ContainsKey((int) child.slot));
            _childElements.Remove((int) child.slot);
            base.forgetChild(child);
        }

        public void removeChild(RenderBox child) {
            int index = renderObject.indexOf(child);
            D.assert(_currentlyUpdatingChildIndex == null);
            D.assert(index >= 0);
            owner.buildScope(this, () => {
                D.assert(_childElements.ContainsKey(index));
                try {
                    _currentlyUpdatingChildIndex = index;
                    Element result = updateChild(_childElements[index], null, index);
                    D.assert(result == null);
                }
                finally {
                    _currentlyUpdatingChildIndex = null;
                }

                _childElements.Remove(index);
                D.assert(!_childElements.ContainsKey(index));
            });
        }

        static float _extrapolateMaxScrollOffset(
            int firstIndex,
            int lastIndex,
            float leadingScrollOffset,
            float trailingScrollOffset,
            int childCount
        ) {
            if (lastIndex == childCount - 1) {
                return trailingScrollOffset;
            }

            int reifiedCount = lastIndex - firstIndex + 1;
            float averageExtent = (trailingScrollOffset - leadingScrollOffset) / reifiedCount;
            int remainingCount = childCount - lastIndex - 1;
            return trailingScrollOffset + averageExtent * remainingCount;
        }

        public float estimateMaxScrollOffset(SliverConstraints constraints,
            int firstIndex = 0,
            int lastIndex = 0,
            float leadingScrollOffset = 0,
            float trailingScrollOffset = 0
        ) {
            int? childCount = this.childCount;
            if (childCount == null) {
                return float.PositiveInfinity;
            }

            return widget.estimateMaxScrollOffset(
                constraints,
                firstIndex,
                lastIndex,
                leadingScrollOffset,
                trailingScrollOffset
            ) ?? _extrapolateMaxScrollOffset(
                firstIndex,
                lastIndex,
                leadingScrollOffset,
                trailingScrollOffset,
                childCount.Value
            );
        }

        public int? childCount {
            get { return widget.del.estimatedChildCount; }
        }

        public void didStartLayout() {
            D.assert(debugAssertChildListLocked());
        }

        public void didFinishLayout() {
            D.assert(debugAssertChildListLocked());
            int firstIndex = _childElements.FirstOrDefault().Key;
            int lastIndex = _childElements.LastOrDefault().Key;
            widget.del.didFinishLayout(firstIndex, lastIndex);
        }

        int? _currentlyUpdatingChildIndex;

        public bool debugAssertChildListLocked() {
            D.assert(_currentlyUpdatingChildIndex == null);
            return true;
        }

        public virtual void didAdoptChild(RenderBox child) {
            D.assert(_currentlyUpdatingChildIndex != null);
            SliverMultiBoxAdaptorParentData childParentData = (SliverMultiBoxAdaptorParentData) child.parentData;
            childParentData.index = _currentlyUpdatingChildIndex.Value;
        }

        public bool _didUnderflow = false;

        public void setDidUnderflow(bool value) {
            _didUnderflow = value;
        }

        protected override void insertChildRenderObject(RenderObject child, object slotRaw) {
            D.assert(slotRaw != null);
            int slot = (int) slotRaw;
            D.assert(_currentlyUpdatingChildIndex == slot);
            D.assert(renderObject.debugValidateChild(child));
            renderObject.insert((RenderBox) child, after: _currentBeforeChild);
            D.assert(() => {
                SliverMultiBoxAdaptorParentData childParentData = (SliverMultiBoxAdaptorParentData) child.parentData;
                D.assert(slot == childParentData.index);
                return true;
            });
        }

        protected override void moveChildRenderObject(RenderObject child, object slotRaw) {
            //D.assert(false);
            D.assert(slotRaw != null);
            int slot = (int) slotRaw;
            D.assert(_currentlyUpdatingChildIndex == slot);
            renderObject.move(child as RenderBox, after: _currentBeforeChild);
        }

        protected override void removeChildRenderObject(RenderObject child) {
            D.assert(_currentlyUpdatingChildIndex != null);
            renderObject.remove((RenderBox) child);
        }

        public override void visitChildren(ElementVisitor visitor) {
            D.assert(!_childElements.Values.Any(child => child == null));
            foreach (var e in _childElements.Values) {
                visitor(e);
            }
        }

        public override void debugVisitOnstageChildren(ElementVisitor visitor) {
            LinqUtils<Element>.WhereList(_childElements.Values, (child => {
                SliverMultiBoxAdaptorParentData parentData =
                    (SliverMultiBoxAdaptorParentData) child.renderObject.parentData;
                float itemExtent = 0;
                switch (renderObject.constraints.axis) {
                    case Axis.horizontal:
                        itemExtent = child.renderObject.paintBounds.width;
                        break;
                    case Axis.vertical:
                        itemExtent = child.renderObject.paintBounds.height;
                        break;
                }

                return parentData.layoutOffset != null &&
                       parentData.layoutOffset < renderObject.constraints.scrollOffset + renderObject.constraints.remainingPaintExtent &&
                       parentData.layoutOffset + itemExtent > renderObject.constraints.scrollOffset;

            })).ForEach(e => visitor(e));
        }
    }

    public class KeepAlive : ParentDataWidget<IKeepAliveParentDataMixin> {
        public KeepAlive(
            Key key = null,
            bool keepAlive = true,
            Widget child = null
        ) : base(key: key, child: child) {
            D.assert(child != null);
            this.keepAlive = keepAlive;
        }

        public readonly bool keepAlive;

        public override void applyParentData(RenderObject renderObject) {
            D.assert(renderObject.parentData is IKeepAliveParentDataMixin);
            IKeepAliveParentDataMixin parentData = (IKeepAliveParentDataMixin) renderObject.parentData ;
   
            if (((IKeepAliveParentDataMixin)parentData).keepAlive != keepAlive) {
                parentData.keepAlive = keepAlive;
                var targetParent = renderObject.parent;
                if (targetParent is RenderObject && !keepAlive) {
                    ((RenderObject) targetParent).markNeedsLayout();
                }
            }
        }

        public override bool debugCanApplyOutOfTurn() {
            return keepAlive;
        }

        public override Type debugTypicalAncestorWidgetClass {
            get { return typeof(SliverWithKeepAliveWidget); }
        }

        public override void debugFillProperties(DiagnosticPropertiesBuilder properties) {
            base.debugFillProperties(properties);
            properties.add(new DiagnosticsProperty<bool>("keepAlive", keepAlive));
        }
    }
}