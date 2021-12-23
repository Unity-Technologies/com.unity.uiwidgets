using Unity.UIWidgets.foundation;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.rendering;

namespace Unity.UIWidgets.widgets {
    public class SliverPrototypeExtentList : SliverMultiBoxAdaptorWidget {
        public SliverPrototypeExtentList(
            Key key = null, 
            SliverChildDelegate del = null, 
            Widget prototypeItem = null
            ) : base(key: key, del: del) {
            this.prototypeItem = prototypeItem;
        }


        public readonly Widget prototypeItem;
        public override RenderObject createRenderObject(BuildContext context) { 
            _SliverPrototypeExtentListElement element = context as _SliverPrototypeExtentListElement; 
            return new _RenderSliverPrototypeExtentList(childManager: element);
        }
        public override Element createElement() {
            return new _SliverPrototypeExtentListElement(this);
        }
    }
    public class _SliverPrototypeExtentListElement : SliverMultiBoxAdaptorElement {
        public _SliverPrototypeExtentListElement(SliverPrototypeExtentList widget) : base(widget) {
        }

        public new SliverPrototypeExtentList widget {
            get {
                return base.widget as SliverPrototypeExtentList;
            }
        }
        public new _RenderSliverPrototypeExtentList renderObject {
            get { return base.renderObject as _RenderSliverPrototypeExtentList;}
        }

        Element _prototype;
        public readonly static object _prototypeSlot = null;

        protected override void insertChildRenderObject(RenderObject child, object slot) {
            if (slot == _prototypeSlot) {
              D.assert(child is RenderBox);
              renderObject.child = child as RenderBox;
            } else {
              base.insertChildRenderObject(child, (int)slot );
            }
        }
        public override void didAdoptChild(RenderBox child) {
            if (child != renderObject.child)
                base.didAdoptChild(child);
        }

        protected override void moveChildRenderObject(RenderObject child, object slot) {
            child = (RenderBox) child;
            if (slot == _prototypeSlot)
                D.assert(false); 
            else
                base.moveChildRenderObject(child, (int)slot );
        }

        protected override void removeChildRenderObject(RenderObject child) {
            child = (RenderBox) child;
            if (renderObject.child == child)
              renderObject.child = null;
            else
              base.removeChildRenderObject(child);
        }
        public override void visitChildren(ElementVisitor visitor) {
            if (_prototype != null)
              visitor(_prototype);
            base.visitChildren(visitor);
        }
        public override void mount(Element parent, object newSlot) {
            base.mount(parent, newSlot);
            _prototype = updateChild(_prototype, widget.prototypeItem, _prototypeSlot);
        }
        public override void update(Widget newWidget) {
            newWidget = (SliverPrototypeExtentList) newWidget;
            base.update(newWidget);
            D.assert(widget == newWidget);
            _prototype = updateChild(_prototype, widget.prototypeItem, _prototypeSlot);
        }
    }
    public class _RenderSliverPrototypeExtentList : RenderSliverFixedExtentBoxAdaptor {
        public _RenderSliverPrototypeExtentList(
            _SliverPrototypeExtentListElement childManager = null
        ) : base(childManager: childManager) {
        }

        RenderBox _child;

        public RenderBox child {
            get { return _child;}
            set {
                if (_child != null)
                    dropChild(_child);
                _child = value;
                if (_child != null)
                    adoptChild(_child);
                markNeedsLayout();
            }
        }
        protected override void performLayout() {
            child.layout(constraints.asBoxConstraints(), parentUsesSize: true);
            base.performLayout();
        }
        public override void attach(object owner) {
            owner = (PipelineOwner) owner;
            base.attach(owner);
            if (_child != null)
              _child.attach(owner);
        }
        public override void detach() {
            base.detach();
            if (_child != null)
              _child.detach();
        }
        public override void redepthChildren() {
            if (_child != null)
              redepthChild(_child);
            base.redepthChildren();
        }
        public override void visitChildren(RenderObjectVisitor visitor) {
            if (_child != null)
              visitor(_child);
            base.visitChildren(visitor);
        }

        public override float itemExtent {
            get {
                D.assert(child != null && child.hasSize);
                return constraints.axis == Axis.vertical ? child.size.height : child.size.width;
            }
            set {
            }
        }
    }

}