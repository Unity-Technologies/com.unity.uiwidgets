using System.Collections.Generic;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.rendering;
using Unity.UIWidgets.ui;

namespace Unity.UIWidgets.widgets {
    public delegate Widget SliverLayoutWidgetBuilder(BuildContext context, SliverConstraints constraints) ;
    
    public class SliverLayoutBuilder : ConstrainedLayoutBuilder<SliverConstraints> {
        public SliverLayoutBuilder(
            Key key = null,
            ConstraintBuilder builder = null
        ) : base(key: key, builder: builder) {
            
        }

        public override RenderObject createRenderObject(BuildContext context) {
            return new _RenderSliverLayoutBuilder();
        }
    }
    public class _RenderSliverLayoutBuilder : RenderConstrainedLayoutBuilderMixinRenderSliver<SliverConstraints, RenderSliver> { 
        public override float? childMainAxisPosition(RenderObject child) {
            D.assert(child != null);
            D.assert(child == this.child);
            return 0;
        }

        protected override void performLayout() {
            layoutAndBuildChild();
            child?.layout(constraints, parentUsesSize: true);
            geometry = child?.geometry ?? SliverGeometry.zero;
        }
        public override void applyPaintTransform(RenderObject child, Matrix4 transform) {
            D.assert(child != null);
            D.assert(child == this.child);
        }
        public override void paint(PaintingContext context, Offset offset) {
            if (child?.geometry?.visible == true)
                context.paintChild(child, offset);
        }

        protected override bool hitTestChildren(SliverHitTestResult result,float mainAxisPosition = 0f, float crossAxisPosition = 0f) {
            return child != null
                   && child.geometry.hitTestExtent > 0
                   && child.hitTest(result, mainAxisPosition: mainAxisPosition, crossAxisPosition: crossAxisPosition);
        }

        public override bool debugValidateChild(RenderObject child) {
            D.assert(() => {
                if (!(child is RenderSliver)) {
                    throw new UIWidgetsError(
                       new List<DiagnosticsNode>() {
                           new ErrorSummary(
                               $"A {GetType()} expected a child of type {typeof(RenderSliver)} but received a " +
                           $"child of type {child.GetType()}."
                           ),
                           new ErrorDescription(
                               "RenderObjects expect specific types of children because they " + 
                           "coordinate with their children during layout and paint. For " + 
                           "example, a RenderSliver cannot be the child of a RenderBox because " + 
                           "a RenderSliver does not understand the RenderBox layout protocol."
                           ),
                           new ErrorSpacer(),
                           new DiagnosticsProperty<object>(
                               $"The {GetType()} that expected a {typeof(RenderSliver)} child was created by",
                               debugCreator,
                               style: DiagnosticsTreeStyle.errorProperty
                           ),
                           new ErrorSpacer(),
                           new DiagnosticsProperty<dynamic>(
                               $"The {child.GetType()} that did not match the expected child type " +
                                    "was created by",
                                    child.debugCreator,
                                    style: DiagnosticsTreeStyle.errorProperty
                           ),
                           
                       }
                    );
                }

                return true;
            });
            return true;
        }

        
        internal new RenderSliver _child;

        public new RenderSliver child {
            get { return _child; }
            set {
                if (_child != null) {
                    dropChild(_child);
                }

                _child = value;
                if (_child != null) {
                    adoptChild(_child);
                }
            }
        }

        public override LayoutCallback<SliverConstraints> _callback { get; set; }
        
        public override void updateCallback(LayoutCallback<SliverConstraints> value) {
            if (value == _callback)
                return;
            _callback = value;
            markNeedsLayout();
        }

        public override void layoutAndBuildChild() {
            D.assert(_callback != null);
            invokeLayoutCallback(_callback);
        }
    }

}