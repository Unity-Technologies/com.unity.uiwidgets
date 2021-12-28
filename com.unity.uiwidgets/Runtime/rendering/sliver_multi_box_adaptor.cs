using System.Collections.Generic;
using System.Linq;
using Unity.UIWidgets.external;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.gestures;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.ui;

namespace Unity.UIWidgets.rendering {
    public interface IKeepAliveParentDataMixin : IParentData {
        bool keptAlive { get; }
        
        bool keepAlive { get; set; }
    }
    
    public class KeepAliveParentDataMixin : ParentData, IKeepAliveParentDataMixin { 
        bool _keepAlive = false;

        public bool keepAlive {
            get { return _keepAlive; }
            set { _keepAlive = value; }
        }

        public bool keptAlive { get; }
    } 

    public interface RenderSliverBoxChildManager {
        void createChild(int index, RenderBox after = null);

        void removeChild(RenderBox child);

        float estimateMaxScrollOffset(
            SliverConstraints constraints,
            int firstIndex = 0,
            int lastIndex = 0,
            float leadingScrollOffset = 0,
            float trailingScrollOffset = 0);

        int? childCount { get; }

        void didAdoptChild(RenderBox child);

        void setDidUnderflow(bool value);

        void didStartLayout();

        void didFinishLayout();

        bool debugAssertChildListLocked();
    }

    public class SliverMultiBoxAdaptorParentData : ContainerParentDataMixinSliverLogicalParentData<RenderBox>, IKeepAliveParentDataMixin {
        public int index;

        bool _keepAlive = false;

        public bool keepAlive {
            get { return _keepAlive; }
            set { _keepAlive = value; }
        }

        public bool keptAlive {
            get { return _keptAlive; }
        }

        internal bool _keptAlive = false;
        
        public override string ToString() {
            return $"index={index}; {(keepAlive ? "keeyAlive; " : "")}{base.ToString()}";
        }
    }

    public abstract class RenderSliverMultiBoxAdaptor
        : ContainerRenderObjectMixinRenderSliver<RenderBox, SliverMultiBoxAdaptorParentData> {
        public RenderSliverMultiBoxAdaptor(
            RenderSliverBoxChildManager childManager = null
        ) {
            D.assert(childManager != null);
            _childManager = childManager;
            D.assert(()=> {
                _debugDanglingKeepAlives = new List<RenderBox>();
                return true;
            });
        }

        public override void setupParentData(RenderObject child) {
            if (!(child.parentData is SliverMultiBoxAdaptorParentData)) {
                child.parentData = new SliverMultiBoxAdaptorParentData();
            }
        }

        protected RenderSliverBoxChildManager childManager {
            get { return _childManager; }
        }
        readonly RenderSliverBoxChildManager _childManager;

        readonly Dictionary<int, RenderBox> _keepAliveBucket = new Dictionary<int, RenderBox>();
        List<RenderBox> _debugDanglingKeepAlives;

        public bool debugChildIntegrityEnabled {
            get { return _debugChildIntegrityEnabled;}
            set {
                D.assert(() =>{
                    _debugChildIntegrityEnabled = value;
                    return _debugVerifyChildOrder() &&
                           (!_debugChildIntegrityEnabled || _debugDanglingKeepAlives.isEmpty());
                });
            }
        }
        public void setDebugChildIntegrityEnabled(bool? enabled) {
            D.assert(enabled != null);
            D.assert(() => {
                _debugChildIntegrityEnabled = enabled.Value;
                return _debugVerifyChildOrder() &&
                       (!_debugChildIntegrityEnabled || _debugDanglingKeepAlives.isEmpty());
            });
        }
        bool _debugChildIntegrityEnabled = true;
        
        protected override void adoptChild(AbstractNodeMixinDiagnosticableTree childNode) {
            base.adoptChild(childNode);
            var child = (RenderBox) childNode;
            SliverMultiBoxAdaptorParentData childParentData = (SliverMultiBoxAdaptorParentData) child.parentData;
            if (!childParentData._keptAlive) {
                childManager.didAdoptChild(child);
            }
        }

        bool _debugAssertChildListLocked() {
            return childManager.debugAssertChildListLocked();
        }

        bool _debugVerifyChildOrder(){
            if (_debugChildIntegrityEnabled) {
                RenderBox child = firstChild;
                int index;
                while (child != null) {
                    index = indexOf(child);
                    child = childAfter(child);
                    D.assert(child == null || indexOf(child) > index);
                }
            }
            return true;
        }

        public override void insert(RenderBox child, RenderBox after = null) {
            D.assert(!_keepAliveBucket.ContainsValue(value: child));
            base.insert(child, after: after);
            D.assert(firstChild != null);
            D.assert(_debugVerifyChildOrder());
        }
        public new void move(RenderBox child, RenderBox after = null) {
            SliverMultiBoxAdaptorParentData childParentData = child.parentData as SliverMultiBoxAdaptorParentData;
            if (!childParentData.keptAlive) {
                base.move(child, after: after);
                childManager.didAdoptChild(child);
                markNeedsLayout();
            } else {
                if (_keepAliveBucket[childParentData.index] == child) {
                    _keepAliveBucket.Remove(childParentData.index);
                }
                D.assert(()=> {
                    _debugDanglingKeepAlives.Remove(child);
                    return true;
                });
                childManager.didAdoptChild(child);
                D.assert(()=> {
                    if (_keepAliveBucket.ContainsKey(childParentData.index))
                        _debugDanglingKeepAlives.Add(_keepAliveBucket.getOrDefault(childParentData.index));
                    return true;
                });
                _keepAliveBucket[childParentData.index] = child;
            }
        }
        public override void remove(RenderBox child) {
            SliverMultiBoxAdaptorParentData childParentData = (SliverMultiBoxAdaptorParentData) child.parentData;
            if (!childParentData._keptAlive) {
                base.remove(child);
                return;
            }
            D.assert(_keepAliveBucket[childParentData.index] == child);
            D.assert(()=> {
                _debugDanglingKeepAlives.Remove(child);
                return true;
            });
            _keepAliveBucket.Remove(childParentData.index);
            dropChild(child);
        }
        public override void removeAll() {
            base.removeAll();

            foreach (var child in _keepAliveBucket.Values) {
                dropChild(child);
            }

            _keepAliveBucket.Clear();
        }

        void _createOrObtainChild(int index, RenderBox after = null) {
            invokeLayoutCallback<SliverConstraints>(constraints => {
                D.assert(constraints == this.constraints);
                if (_keepAliveBucket.ContainsKey(index)) {
                    RenderBox child = _keepAliveBucket[index];
                    _keepAliveBucket.Remove(index);
                    SliverMultiBoxAdaptorParentData childParentData = (SliverMultiBoxAdaptorParentData) child.parentData;
                    D.assert(childParentData._keptAlive);
                    dropChild(child);
                    child.parentData = childParentData;
                    insert(child, after: after);
                    childParentData._keptAlive = false;
                }
                else {
                    _childManager.createChild(index, after: after);
                }
            });
        }

        public void _destroyOrCacheChild(RenderBox child) {
            SliverMultiBoxAdaptorParentData childParentData = (SliverMultiBoxAdaptorParentData) child.parentData;
            if (childParentData.keepAlive) {
                D.assert(!childParentData._keptAlive);
                remove(child);
                _keepAliveBucket[childParentData.index] = child;
                child.parentData = childParentData;
                base.adoptChild(child);
                childParentData._keptAlive = true;
            }
            else {
                D.assert(child.parent == this);
                _childManager.removeChild(child);
                D.assert(child.parent == null);
            }
        }

        public override void attach(object owner) {
            base.attach(owner);
            foreach (RenderBox child in _keepAliveBucket.Values) {
                child.attach(owner);
            }
        }

        public override void detach() {
            base.detach();
            foreach (RenderBox child in _keepAliveBucket.Values) {
                child.detach();
            }
        }

        public override void redepthChildren() {
            base.redepthChildren();
            foreach (var child in _keepAliveBucket.Values) {
                redepthChild(child);
            }
        }

        public override void visitChildren(RenderObjectVisitor visitor) {
            base.visitChildren(visitor);

            foreach (var child in _keepAliveBucket.Values) {
                visitor(child);
            }
        }

        protected bool addInitialChild(int index = 0, float layoutOffset = 0.0f) {
            D.assert(_debugAssertChildListLocked());
            D.assert(firstChild == null);
            _createOrObtainChild(index, after: null);
            if (firstChild != null) {
                D.assert(firstChild == lastChild);
                D.assert(indexOf(firstChild) == index);
                SliverMultiBoxAdaptorParentData firstChildParentData = (SliverMultiBoxAdaptorParentData) firstChild.parentData;
                firstChildParentData.layoutOffset = layoutOffset;
                return true;
            }
            childManager.setDidUnderflow(true);
            return false;
        }

        protected RenderBox insertAndLayoutLeadingChild(
            BoxConstraints childConstraints, 
            bool parentUsesSize = false) {
            
            D.assert(_debugAssertChildListLocked());
            int index = indexOf(firstChild) - 1;
            _createOrObtainChild(index, after: null);
            if (indexOf(firstChild) == index) {
                firstChild.layout(childConstraints, parentUsesSize: parentUsesSize);
                return firstChild;
            }
            childManager.setDidUnderflow(true);
            return null;
        }

        protected RenderBox insertAndLayoutChild(
            BoxConstraints childConstraints,
            RenderBox after = null,
            bool parentUsesSize = false
        ) {
            D.assert(_debugAssertChildListLocked());
            D.assert(after != null);

            int index = indexOf(after) + 1;
            _createOrObtainChild(index, after: after);
            RenderBox child = childAfter(after);
            if (child != null && indexOf(child) == index) {
                child.layout(childConstraints, parentUsesSize: parentUsesSize);
                return child;
            }

            childManager.setDidUnderflow(true);
            return null;
        }

        protected void collectGarbage(int leadingGarbage, int trailingGarbage) {
            D.assert(_debugAssertChildListLocked());
            D.assert(childCount >= leadingGarbage + trailingGarbage);

            invokeLayoutCallback<SliverConstraints>(constraints => {
                while (leadingGarbage > 0) {
                    _destroyOrCacheChild(firstChild);
                    leadingGarbage -= 1;
                }

                while (trailingGarbage > 0) {
                    _destroyOrCacheChild(lastChild);
                    trailingGarbage -= 1;
                }

                LinqUtils<RenderBox>.WhereList(_keepAliveBucket.Values, (child => {
                    SliverMultiBoxAdaptorParentData childParentData = (SliverMultiBoxAdaptorParentData) child.parentData;
                    return !childParentData.keepAlive;
                })).ForEach(_childManager.removeChild);

                D.assert(LinqUtils<RenderBox>.WhereList(_keepAliveBucket.Values, (child => {
                    SliverMultiBoxAdaptorParentData childParentData = (SliverMultiBoxAdaptorParentData) child.parentData;
                    return !childParentData.keepAlive;
                })).isEmpty());
            });
        }

        public int indexOf(RenderBox child) {
            D.assert(child != null);
            SliverMultiBoxAdaptorParentData childParentData = (SliverMultiBoxAdaptorParentData) child.parentData;
            return childParentData.index;
        }

        protected float paintExtentOf(RenderBox child) {
            D.assert(child != null);
            D.assert(child.hasSize);

            switch (constraints.axis) {
                case Axis.horizontal:
                    return child.size.width;
                case Axis.vertical:
                    return child.size.height;
            }

            return 0.0f;
        }

        protected override bool hitTestChildren(
            SliverHitTestResult result, 
            float mainAxisPosition = 0.0f,
            float crossAxisPosition = 0.0f) {
            RenderBox child = lastChild;
            BoxHitTestResult boxResult = new BoxHitTestResult(result);

            while (child != null) {
                if (this.hitTestBoxChild(boxResult, child, mainAxisPosition: mainAxisPosition,
                    crossAxisPosition: crossAxisPosition)) {
                    return true;
                }

                child = childBefore(child);
            }

            return false;
        }

        public override float? childMainAxisPosition(RenderObject child) {
            return childScrollOffset(child) - constraints.scrollOffset;
        }

        public override float? childScrollOffset(RenderObject child) {
            D.assert(child != null);
            D.assert(child.parent == this);
            SliverMultiBoxAdaptorParentData childParentData = (SliverMultiBoxAdaptorParentData) child.parentData;
            return (float)childParentData.layoutOffset;
        }

        public override void applyPaintTransform(RenderObject child, Matrix4 transform) {
            this.applyPaintTransformForBoxChild((RenderBox) child, transform);
        }

        public override void paint(PaintingContext context, Offset offset) {
            if (firstChild == null) {
                return;
            }

            Offset mainAxisUnit = null, crossAxisUnit = null, originOffset = null;
            bool addExtent = false;
            switch (GrowthDirectionUtils.applyGrowthDirectionToAxisDirection(constraints.axisDirection,
                constraints.growthDirection)) {
                case AxisDirection.up:
                    mainAxisUnit = new Offset(0.0f, -1.0f);
                    crossAxisUnit = new Offset(1.0f, 0.0f);
                    originOffset = offset + new Offset(0.0f, geometry.paintExtent);
                    addExtent = true;
                    break;
                case AxisDirection.right:
                    mainAxisUnit = new Offset(1.0f, 0.0f);
                    crossAxisUnit = new Offset(0.0f, 1.0f);
                    originOffset = offset;
                    addExtent = false;
                    break;
                case AxisDirection.down:
                    mainAxisUnit = new Offset(0.0f, 1.0f);
                    crossAxisUnit = new Offset(1.0f, 0.0f);
                    originOffset = offset;
                    addExtent = false;
                    break;
                case AxisDirection.left:
                    mainAxisUnit = new Offset(-1.0f, 0.0f);
                    crossAxisUnit = new Offset(0.0f, 1.0f);
                    originOffset = offset + new Offset(geometry.paintExtent, 0.0f);
                    addExtent = true;
                    break;
            }
            D.assert(mainAxisUnit != null);
            RenderBox child = firstChild;
            while (child != null) {
                float mainAxisDelta = childMainAxisPosition(child) ?? 0.0f;
                float crossAxisDelta = childCrossAxisPosition(child) ?? 0.0f;
                Offset childOffset = new Offset(
                    originOffset.dx + mainAxisUnit.dx * mainAxisDelta + crossAxisUnit.dx * crossAxisDelta,
                    originOffset.dy + mainAxisUnit.dy * mainAxisDelta + crossAxisUnit.dy * crossAxisDelta
                );
                if (addExtent) {
                    childOffset += mainAxisUnit * paintExtentOf(child);
                }

                if (mainAxisDelta < constraints.remainingPaintExtent &&
                    mainAxisDelta + paintExtentOf(child) > 0) {
                    context.paintChild(child, childOffset);
                }

                child = childAfter(child);
            }
        }

        public override void debugFillProperties(DiagnosticPropertiesBuilder properties) {
            base.debugFillProperties(properties);
            properties.add(DiagnosticsNode.message(firstChild != null
                ? $"currently live children: {indexOf(firstChild)} to {indexOf(lastChild)}" : "no children current live"));
        }

        public bool debugAssertChildListIsNonEmptyAndContiguous() {
            D.assert(() => {
                D.assert(firstChild != null);
                int index = indexOf(firstChild);
                RenderBox child = childAfter(firstChild);
                while (child != null) {
                    index += 1;
                    D.assert(indexOf(child) == index);
                    child = childAfter(child);
                }

                return true;
            });
            return true;
        }

        public override List<DiagnosticsNode> debugDescribeChildren() {
            var children = new List<DiagnosticsNode>();
            if (firstChild != null) {
                RenderBox child = firstChild;
                while (true) {
                   SliverMultiBoxAdaptorParentData childParentData = (SliverMultiBoxAdaptorParentData) child.parentData;
                    children.Add(child.toDiagnosticsNode(name: "child with index " + childParentData.index));
                    if (child == lastChild) {
                        break;
                    }

                    child = childParentData.nextSibling;
                }
            }

            if (_keepAliveBucket.isNotEmpty()) {
                List<int> indices = _keepAliveBucket.Keys.ToList();
                indices.Sort();
                foreach (int index in indices) {
                    children.Add(_keepAliveBucket[index].toDiagnosticsNode(
                        name: "child with index " + index + " (kept alive but not laid out)",
                        style: DiagnosticsTreeStyle.offstage
                    ));
                }
            }

            return children;
        }
    }
}