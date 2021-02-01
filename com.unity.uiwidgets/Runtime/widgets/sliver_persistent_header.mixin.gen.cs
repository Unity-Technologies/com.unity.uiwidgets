using Unity.UIWidgets.foundation;
using Unity.UIWidgets.rendering;

namespace Unity.UIWidgets.widgets {

    public abstract class _RenderSliverPersistentHeaderForWidgetsMixinOnRenderSliverPersistentHeaderRenderSliverFloatingPersistentHeader : RenderSliverFloatingPersistentHeader, _RenderSliverPersistentHeaderForWidgetsMixin {
         
        public _RenderSliverPersistentHeaderForWidgetsMixinOnRenderSliverPersistentHeaderRenderSliverFloatingPersistentHeader(
            RenderBox child = null,
            FloatingHeaderSnapConfiguration snapConfiguration = null,
            OverScrollHeaderStretchConfiguration stretchConfiguration = null
        ) : base(
            child: child,
            stretchConfiguration: stretchConfiguration) {
            _snapConfiguration = snapConfiguration;
        }

         public _SliverPersistentHeaderElement _element {
             get {
                 return sliver_persistent_header_utils._element;
             }
             set {
                 if (value == sliver_persistent_header_utils._element) {
                     return;
                 }
 
                 sliver_persistent_header_utils._element = value;
             }
             
         }
         
         public override float? minExtent {
             get {
                 return _element.widget.layoutDelegate.minExtent;
             }
         }
 
         public override float? maxExtent {
             get {
                 return _element.widget.layoutDelegate.minExtent;
             }
         }
 
         protected override void updateChild(float shrinkOffset, bool overlapsContent) {
             D.assert(_element != null);
             _element._build(shrinkOffset, overlapsContent);
         }
 
        void _RenderSliverPersistentHeaderForWidgetsMixin.triggerRebuild() {
            triggerRebuild();
        }
        
        void triggerRebuild() {
            markNeedsLayout();
        }
        
        protected override void performLayout() {
            base.performLayout();
        }
    }





    public abstract class _RenderSliverPersistentHeaderForWidgetsMixinOnRenderSliverPersistentHeaderRenderSliverFloatingPinnedPersistentHeader : RenderSliverFloatingPinnedPersistentHeader, _RenderSliverPersistentHeaderForWidgetsMixin {
         
        public _RenderSliverPersistentHeaderForWidgetsMixinOnRenderSliverPersistentHeaderRenderSliverFloatingPinnedPersistentHeader(
            RenderBox child = null,
            FloatingHeaderSnapConfiguration snapConfiguration = null,
            OverScrollHeaderStretchConfiguration stretchConfiguration = null
        ) : base(child: child,
            snapConfiguration: snapConfiguration,
            stretchConfiguration: stretchConfiguration) {
        }

         public _SliverPersistentHeaderElement _element {
             get {
                 return sliver_persistent_header_utils._element;
             }
             set {
                 if (value == sliver_persistent_header_utils._element) {
                     return;
                 }
 
                 sliver_persistent_header_utils._element = value;
             }
             
         }
         
         public override float? minExtent {
             get {
                 return _element.widget.layoutDelegate.minExtent;
             }
         }
 
         public override float? maxExtent {
             get {
                 return _element.widget.layoutDelegate.minExtent;
             }
         }
 
         protected override void updateChild(float shrinkOffset, bool overlapsContent) {
             D.assert(_element != null);
             _element._build(shrinkOffset, overlapsContent);
         }
 
        void _RenderSliverPersistentHeaderForWidgetsMixin.triggerRebuild() {
            triggerRebuild();
        }
        
        void triggerRebuild() {
            markNeedsLayout();
        }
        
        protected override void performLayout() {
            base.performLayout();
        }
    }



    public abstract class _RenderSliverPersistentHeaderForWidgetsMixinOnRenderSliverPersistentHeaderRenderSliverPinnedPersistentHeader : RenderSliverPinnedPersistentHeader, _RenderSliverPersistentHeaderForWidgetsMixin {
         
        public _RenderSliverPersistentHeaderForWidgetsMixinOnRenderSliverPersistentHeaderRenderSliverPinnedPersistentHeader(
            RenderBox child = null,
            OverScrollHeaderStretchConfiguration stretchConfiguration = null
        ) : base(child: child,
            stretchConfiguration: stretchConfiguration) {
        }

         public _SliverPersistentHeaderElement _element {
             get {
                 return sliver_persistent_header_utils._element;
             }
             set {
                 if (value == sliver_persistent_header_utils._element) {
                     return;
                 }
 
                 sliver_persistent_header_utils._element = value;
             }
             
         }
         
         public override float? minExtent {
             get {
                 return _element.widget.layoutDelegate.minExtent;
             }
         }
 
         public override float? maxExtent {
             get {
                 return _element.widget.layoutDelegate.minExtent;
             }
         }
 
         protected override void updateChild(float shrinkOffset, bool overlapsContent) {
             D.assert(_element != null);
             _element._build(shrinkOffset, overlapsContent);
         }
 
        void _RenderSliverPersistentHeaderForWidgetsMixin.triggerRebuild() {
            triggerRebuild();
        }
        
        void triggerRebuild() {
            markNeedsLayout();
        }
        
        protected override void performLayout() {
            base.performLayout();
        }
    }



    public abstract class _RenderSliverPersistentHeaderForWidgetsMixinOnRenderSliverPersistentHeaderRenderSliverScrollingPersistentHeader : RenderSliverScrollingPersistentHeader, _RenderSliverPersistentHeaderForWidgetsMixin {
         
        public _RenderSliverPersistentHeaderForWidgetsMixinOnRenderSliverPersistentHeaderRenderSliverScrollingPersistentHeader(
            RenderBox child = null,
            OverScrollHeaderStretchConfiguration stretchConfiguration = null
        ) : base(child: child,
            stretchConfiguration: stretchConfiguration) {
        }

         public _SliverPersistentHeaderElement _element {
             get {
                 return sliver_persistent_header_utils._element;
             }
             set {
                 if (value == sliver_persistent_header_utils._element) {
                     return;
                 }
 
                 sliver_persistent_header_utils._element = value;
             }
             
         }
         
         public override float? minExtent {
             get {
                 return _element.widget.layoutDelegate.minExtent;
             }
         }
 
         public override float? maxExtent {
             get {
                 return _element.widget.layoutDelegate.minExtent;
             }
         }
 
         protected override void updateChild(float shrinkOffset, bool overlapsContent) {
             D.assert(_element != null);
             _element._build(shrinkOffset, overlapsContent);
         }
 
        void _RenderSliverPersistentHeaderForWidgetsMixin.triggerRebuild() {
            triggerRebuild();
        }
        
        void triggerRebuild() {
            markNeedsLayout();
        }
        
        protected override void performLayout() {
            base.performLayout();
        }
    }


}
