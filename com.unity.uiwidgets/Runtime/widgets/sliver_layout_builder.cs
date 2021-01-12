using System.Collections.Generic;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.painting;
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

        public static SliverLayoutBuilder Creat(
            Key key = null,
            SliverLayoutWidgetBuilder builder = null
            ) {
            ConstraintBuilder _builder = (context, constraints) => {
                return builder(context, (SliverConstraints)constraints);
            };
            return new SliverLayoutBuilder(key,_builder);
        }

        public new SliverLayoutWidgetBuilder builder {
            get {
                //return base.builder;
                SliverLayoutWidgetBuilder _builder = (context, constraints) => {
                    return base.builder(context, (SliverConstraints) constraints);
                };
                return _builder;
            }
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

        public bool debugValidateChild(RenderObject child) {
            D.assert(() => {
                if (!(child is RenderSliver)) {
                    throw new UIWidgetsError(
                        "A " + GetType() + " expected a child of type " + typeof(RenderSliver) + " but received a " +
                        "child of type " + child.GetType() + ".\n" +
                        "RenderObjects expect specific types of children because they " +
                        "coordinate with their children during layout and paint. For " +
                        "example, a RenderSliver cannot be the child of a RenderBox because " +
                        "a RenderSliver does not understand the RenderBox layout protocol.\n" +
                        "\n" +
                        "The " + GetType() + " that expected a " + typeof(RenderSliver) + " child was created by:\n" +
                        "  " + debugCreator + "\n" +
                        "\n" +
                        "The " + child.GetType() + " that did not match the expected child type " +
                        "was created by:\n" +
                        "  " + child.debugCreator + "\n"
                    );
                }

                return true;
            });
            return true;
        }

        
        internal RenderSliver _child;

        public RenderSliver child {
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

        public LayoutCallback<SliverConstraints> _callback { get; set; }
        public void updateCallback(LayoutCallback<SliverConstraints> value) {
            if (value == _callback)
                return;
            _callback = value;
            markNeedsLayout();
        }

        public void layoutAndBuildChild() {
            D.assert(_callback != null);
            invokeLayoutCallback(_callback);
        }
    }

}