using System.Collections.Generic;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.rendering;

namespace Unity.UIWidgets.widgets {
    
    static class sliver_persistent_header_utils {
       public static _SliverPersistentHeaderElement _element { get; set; }
    }
    
    public abstract class SliverPersistentHeaderDelegate {
        public SliverPersistentHeaderDelegate() {
        }

        public abstract Widget build(BuildContext context, float shrinkOffset, bool overlapsContent);

        public abstract float? minExtent { get; }

        public abstract float? maxExtent { get; }

        public virtual FloatingHeaderSnapConfiguration snapConfiguration {
            get { return null; }
        }

        public virtual OverScrollHeaderStretchConfiguration stretchConfiguration {
            get { return null; }
        }

        public abstract bool shouldRebuild(SliverPersistentHeaderDelegate oldDelegate);
    }

    public class SliverPersistentHeader : StatelessWidget {
        public SliverPersistentHeader(
            Key key = null,
            SliverPersistentHeaderDelegate del = null,
            bool pinned = false,
            bool floating = false
        ) : base(key: key) {
            D.assert(del != null);
            layoutDelegate = del;
            this.pinned = pinned;
            this.floating = floating;
        }

        public readonly SliverPersistentHeaderDelegate layoutDelegate;

        public readonly bool pinned;

        public readonly bool floating;

        public override Widget build(BuildContext context) {
            if (floating && pinned) {
                return new _SliverFloatingPinnedPersistentHeader(layoutDelegate: layoutDelegate);
            }

            if (pinned) {
                return new _SliverPinnedPersistentHeader(layoutDelegate: layoutDelegate);
            }

            if (floating) {
                return new _SliverFloatingPersistentHeader(layoutDelegate: layoutDelegate);
            }

            return new _SliverScrollingPersistentHeader(layoutDelegate: layoutDelegate);
        }

        public override void debugFillProperties(DiagnosticPropertiesBuilder properties) {
            base.debugFillProperties(properties);
            properties.add(
                new DiagnosticsProperty<SliverPersistentHeaderDelegate>(
                    "delegate",
                    layoutDelegate
                )
            );
            List<string> flags = new List<string>();
            if (pinned) 
                flags.Add("pinned");
            if (floating)
                flags.Add("floating");
            if (flags.isEmpty()) {
                flags.Add("normal");
            }
            properties.add(new EnumerableProperty<string>("mode", flags));
        }
    }

    public class _SliverPersistentHeaderElement : RenderObjectElement {
        public _SliverPersistentHeaderElement(_SliverPersistentHeaderRenderObjectWidget widget) : base(widget) {
        }

        public new _SliverPersistentHeaderRenderObjectWidget widget {
            get { return (_SliverPersistentHeaderRenderObjectWidget) base.widget; }
        }

        public new _RenderSliverPersistentHeaderForWidgetsMixin renderObject {
            get { return base.renderObject as _RenderSliverPersistentHeaderForWidgetsMixin; }
        }

        public override void mount(Element parent, object newSlot) {
            base.mount(parent, newSlot);
            sliver_persistent_header_utils._element = this;
        }

        public override void unmount() {
            base.unmount();
            sliver_persistent_header_utils._element = null;
        }

        public override void update(Widget _newWidget) {
            base.update(_newWidget);
            _SliverPersistentHeaderRenderObjectWidget newWidget =
                _newWidget as _SliverPersistentHeaderRenderObjectWidget;
            _SliverPersistentHeaderRenderObjectWidget oldWidget = widget;
            SliverPersistentHeaderDelegate newDelegate = newWidget.layoutDelegate;
            SliverPersistentHeaderDelegate oldDelegate = oldWidget.layoutDelegate;
            if (newDelegate != oldDelegate &&
                (newDelegate.GetType() != oldDelegate.GetType() || newDelegate.shouldRebuild(oldDelegate))) {
                (renderObject as _RenderSliverPersistentHeaderForWidgetsMixin).triggerRebuild();
            }
        }

        protected override void performRebuild() {
            base.performRebuild();
            (renderObject as _RenderSliverPersistentHeaderForWidgetsMixin).triggerRebuild();
        }

        Element child;

        public void _build(float shrinkOffset, bool overlapsContent) {
            owner.buildScope(this, () =>{
                child = updateChild(
                    child,
                    widget.layoutDelegate.build(
                    this,
                    shrinkOffset,
                    overlapsContent
                ),
                null
                    );
            });
        }

        public override void forgetChild(Element child) {
            D.assert(child == this.child);
            this.child = null;
            base.forgetChild(child);
        }

        protected override void insertChildRenderObject(RenderObject child, object slot) {
            D.assert((bool) ((RenderSliverPersistentHeader)renderObject).debugValidateChild(child));
            ((RenderSliverPersistentHeader)renderObject).child = (RenderBox) child;
        }

        protected override void moveChildRenderObject(RenderObject child, object slot) {
            D.assert(false);
        }

        protected override void removeChildRenderObject(RenderObject child) {
            (renderObject as RenderSliverPersistentHeader).child = null;
        }

        public override void visitChildren(ElementVisitor visitor) {
            if (child != null) {
                visitor(child);
            }
        }
    }

    public abstract class _SliverPersistentHeaderRenderObjectWidget : RenderObjectWidget {
        public _SliverPersistentHeaderRenderObjectWidget(
            Key key = null,
            SliverPersistentHeaderDelegate layoutDelegate = null
        ) : base(key: key) {
            D.assert(layoutDelegate != null);
            this.layoutDelegate = layoutDelegate;
        }

        public readonly SliverPersistentHeaderDelegate layoutDelegate;

        public override Element createElement() {
            return new _SliverPersistentHeaderElement(this);
        }

        public abstract override RenderObject createRenderObject(BuildContext context);

        public override void debugFillProperties(DiagnosticPropertiesBuilder description) {
            base.debugFillProperties(description);
            description.add(new DiagnosticsProperty<SliverPersistentHeaderDelegate>("layoutDelegate", layoutDelegate));
        }
    }

    public interface _RenderSliverPersistentHeaderForWidgetsMixin {

        void triggerRebuild();
    }

    class _SliverScrollingPersistentHeader : _SliverPersistentHeaderRenderObjectWidget {
        public _SliverScrollingPersistentHeader(
            Key key = null,
            SliverPersistentHeaderDelegate layoutDelegate = null
        ) : base(key: key, layoutDelegate: layoutDelegate) {
        }

        public override RenderObject createRenderObject(BuildContext context) {
            return new _RenderSliverScrollingPersistentHeaderForWidgets(
                stretchConfiguration: layoutDelegate.stretchConfiguration);
        }
    }

    abstract class _RenderSliverScrollingPersistentHeader : RenderSliverScrollingPersistentHeader {
    }

    class _RenderSliverScrollingPersistentHeaderForWidgets : _RenderSliverPersistentHeaderForWidgetsMixinOnRenderSliverPersistentHeaderRenderSliverScrollingPersistentHeader {
        
        public _RenderSliverScrollingPersistentHeaderForWidgets(
            RenderBox child = null,
            OverScrollHeaderStretchConfiguration stretchConfiguration = null
        ) : base(
            child: child,
            stretchConfiguration: stretchConfiguration
        ) {
        }
    }

    class _SliverPinnedPersistentHeader : _SliverPersistentHeaderRenderObjectWidget {
        public _SliverPinnedPersistentHeader(
            Key key = null,
            SliverPersistentHeaderDelegate layoutDelegate = null
        ) : base(key: key, layoutDelegate: layoutDelegate) {
        }

        public override RenderObject createRenderObject(BuildContext context) {
            return new _RenderSliverPinnedPersistentHeaderForWidgets(
                stretchConfiguration: layoutDelegate.stretchConfiguration
                );
        }
    }

    abstract class _RenderSliverPinnedPersistentHeader : RenderSliverPinnedPersistentHeader {
    }

    class _RenderSliverPinnedPersistentHeaderForWidgets : _RenderSliverPersistentHeaderForWidgetsMixinOnRenderSliverPersistentHeaderRenderSliverPinnedPersistentHeader {

        public _RenderSliverPinnedPersistentHeaderForWidgets(
            RenderBox child = null,
            OverScrollHeaderStretchConfiguration stretchConfiguration = null
            ) : base(
            child: child,
            stretchConfiguration: stretchConfiguration
        ) {
        }

        public override _SliverPersistentHeaderElement _element {
            get { return sliver_persistent_header_utils._element; }
            set { sliver_persistent_header_utils._element = value; }
        }

        _SliverPersistentHeaderElement _ele;

        public override float? minExtent {
            get { return _element.widget.layoutDelegate.minExtent; }
        }

        public override float? maxExtent {
            get { return _element.widget.layoutDelegate.maxExtent; }
        }

        protected override void updateChild(float shrinkOffset, bool overlapsContent) {
            D.assert(_element != null);
            _element._build(shrinkOffset, overlapsContent);
        }

        protected override void triggerRebuild() {
            markNeedsLayout();
        }
    }

    class _SliverFloatingPersistentHeader : _SliverPersistentHeaderRenderObjectWidget {
        public _SliverFloatingPersistentHeader(
            Key key = null,
            SliverPersistentHeaderDelegate layoutDelegate = null
        ) : base(key: key, layoutDelegate: layoutDelegate) {
        }

        public override RenderObject createRenderObject(BuildContext context) {
            _RenderSliverFloatingPersistentHeaderForWidgets ret = 
                new _RenderSliverFloatingPersistentHeaderForWidgets(
                    snapConfiguration: layoutDelegate.snapConfiguration,
                    stretchConfiguration: layoutDelegate.stretchConfiguration
                    );
            //ret.snapConfiguration = layoutDelegate.snapConfiguration;
            return ret;
        }

        public override void updateRenderObject(BuildContext context, RenderObject _renderObject) {
            _RenderSliverFloatingPersistentHeaderForWidgets renderObject =
                _renderObject as _RenderSliverFloatingPersistentHeaderForWidgets;
            renderObject.snapConfiguration = layoutDelegate.snapConfiguration;
            renderObject.stretchConfiguration = layoutDelegate.stretchConfiguration;
        }
    }

    abstract class _RenderSliverFloatingPinnedPersistentHeader : RenderSliverFloatingPinnedPersistentHeader {
    }

    class _RenderSliverFloatingPinnedPersistentHeaderForWidgets : _RenderSliverPersistentHeaderForWidgetsMixinOnRenderSliverPersistentHeaderRenderSliverFloatingPinnedPersistentHeader {
        public _RenderSliverFloatingPinnedPersistentHeaderForWidgets(
            RenderBox child = null,
            FloatingHeaderSnapConfiguration snapConfiguration = null,
            OverScrollHeaderStretchConfiguration stretchConfiguration = null
        ) : base(
            child: child,
            snapConfiguration: snapConfiguration,
            stretchConfiguration: stretchConfiguration
        ) {
        }

        public override _SliverPersistentHeaderElement _element {
            get { return _ele; }
            set { _ele = value; }
        }

        _SliverPersistentHeaderElement _ele;

        public override float? minExtent {
            get { return _element.widget.layoutDelegate.minExtent; }
        }

        public override float? maxExtent {
            get { return _element.widget.layoutDelegate.maxExtent; }
        }

        protected override void updateChild(float shrinkOffset, bool overlapsContent) {
            D.assert(_element != null);
            _element._build(shrinkOffset, overlapsContent);
        }

        protected override void triggerRebuild() {
            markNeedsLayout();
        }
    }

    class _SliverFloatingPinnedPersistentHeader : _SliverPersistentHeaderRenderObjectWidget {
        public _SliverFloatingPinnedPersistentHeader(
            Key key = null,
            SliverPersistentHeaderDelegate layoutDelegate = null
        ) : base(key: key, layoutDelegate: layoutDelegate) {
        }

        public override RenderObject createRenderObject(BuildContext context) {
            _RenderSliverFloatingPinnedPersistentHeaderForWidgets ret =
                new _RenderSliverFloatingPinnedPersistentHeaderForWidgets(
                    snapConfiguration:  layoutDelegate.snapConfiguration,
                    stretchConfiguration:  layoutDelegate.stretchConfiguration
                    );
            //ret.snapConfiguration = layoutDelegate.snapConfiguration;
            return ret;
        }

        public override void updateRenderObject(BuildContext context, RenderObject _renderObject) {
            _RenderSliverFloatingPinnedPersistentHeaderForWidgets renderObject =
                _renderObject as _RenderSliverFloatingPinnedPersistentHeaderForWidgets;
            renderObject.snapConfiguration = layoutDelegate.snapConfiguration;
            renderObject.stretchConfiguration = layoutDelegate.stretchConfiguration;
        }
    }

    abstract class _RenderSliverFloatingPersistentHeader : RenderSliverFloatingPersistentHeader {
    }

    class _RenderSliverFloatingPersistentHeaderForWidgets : _RenderSliverPersistentHeaderForWidgetsMixinOnRenderSliverPersistentHeaderRenderSliverFloatingPersistentHeader {

        public _RenderSliverFloatingPersistentHeaderForWidgets(
            RenderBox child = null,
            FloatingHeaderSnapConfiguration snapConfiguration = null,
            OverScrollHeaderStretchConfiguration stretchConfiguration = null
        ) : base(
            child: child,
            snapConfiguration: snapConfiguration,
            stretchConfiguration: stretchConfiguration
        ) {
        }

        public override _SliverPersistentHeaderElement _element {
            get { return _ele; }
            set { _ele = value; }
        }

        _SliverPersistentHeaderElement _ele;

        public override float? minExtent {
            get { return _element.widget.layoutDelegate.minExtent; }
        }

        public override float? maxExtent {
            get { return _element.widget.layoutDelegate.maxExtent; }
        }

        protected override void updateChild(float shrinkOffset, bool overlapsContent) {
            D.assert(_element != null);
            _element._build(shrinkOffset, overlapsContent);
        }

        protected override void triggerRebuild() {
            markNeedsLayout();
        }
    }
}