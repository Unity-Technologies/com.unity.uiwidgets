using System;
using System.Collections.Generic;
using Unity.UIWidgets.animation;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.gestures;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.ui;

namespace Unity.UIWidgets.rendering
{
    public abstract class RenderProxySliver : RenderSliver, RenderObjectWithChildMixin<RenderSliver>
    {
        public RenderProxySliver(RenderSliver child = null)
        {
            this.child = child;
        }

        RenderSliver _child;
        public RenderSliver child
        {
            get { return _child; }
            set
            {
                if (_child != null)
                    dropChild(_child);
                _child = value;
                if (_child != null)
                    adoptChild(_child);
            }
        }
        public override void setupParentData(RenderObject child)
        {
            if (!(child.parentData is SliverPhysicalParentData))
                child.parentData = new SliverPhysicalParentData();
        }

        protected override void performLayout()
        {
            D.assert(child != null);
            child.layout(constraints, parentUsesSize: true);
            geometry = child.geometry;
        }
        public override void paint(PaintingContext context, Offset offset)
        {
            if (child != null)
                context.paintChild(child, offset);
        }

        protected override bool hitTestChildren(SliverHitTestResult result, float mainAxisPosition = 0, float crossAxisPosition = 0)
        {
            return child != null
                   && child.geometry.hitTestExtent > 0
                   && child.hitTest(
                       result,
                       mainAxisPosition: mainAxisPosition,
                       crossAxisPosition: crossAxisPosition);
        }
        public override float? childMainAxisPosition(RenderObject child)
        {
            child = (RenderSliver)child;
            D.assert(child != null);
            D.assert(child == this.child);
            return 0.0f;
        }
        public override void applyPaintTransform(RenderObject child, Matrix4 transform)
        {
            D.assert(child != null);
            SliverPhysicalParentData childParentData = child.parentData as SliverPhysicalParentData;
            childParentData.applyPaintTransform(transform);
        }

        public virtual bool debugValidateChild(RenderObject child)
        {
            D.assert(() =>
            {
                if (!(child is RenderSliver))
                {
                    string result = "";
                    result += new ErrorDescription(
                        $"A {GetType()} expected a child of type $ChildType but received a " +
                        $"child of type {child.GetType()}.");
                    result += new ErrorDescription(
                        "RenderObjects expect specific types of children because they " +
                        "coordinate with their children during layout and paint. For " +
                        "example, a RenderSliver cannot be the child of a RenderBox because " +
                        "a RenderSliver does not understand the RenderBox layout protocol."
                    );
                    result += new ErrorSpacer();
                    result += new DiagnosticsProperty<dynamic>(
                        $"The {GetType()} that expected a $ChildType child was created by",
                        debugCreator,
                        style: DiagnosticsTreeStyle.errorProperty
                    );
                    result += new ErrorSpacer();
                    result += new DiagnosticsProperty<dynamic>(
                        $"The {child.GetType()} that did not match the expected child type " +
                        "was created by",
                        child.debugCreator,
                        style: DiagnosticsTreeStyle.errorProperty
                    );
                    throw new UIWidgetsError(result);
                }

                return true;
            });
            return true;

        }

        RenderObject RenderObjectWithChildMixin.child
        {
            get { return child; }
            set { child = (RenderSliver)value; }
        }
    }
    public class RenderSliverAnimatedOpacity : RenderAnimatedOpacityMixinRenderSliver<RenderSliver>
    {
        public RenderSliverAnimatedOpacity(
            Animation<float> opacity,
            RenderSliver sliver = null
        )
        {
            D.assert(opacity != null);
            this.opacity = opacity;
            child = sliver;
        }
    }
    public class RenderSliverOpacity : RenderProxySliver
    {
        public RenderSliverOpacity(
            RenderSliver sliver = null,
            float opacity = 1.0f
        ) : base(child: sliver)
        {
            D.assert(opacity >= 0.0 && opacity <= 1.0);
            _opacity = opacity;
            _alpha = ui.Color.getAlphaFromOpacity(opacity);
        }


        protected override bool alwaysNeedsCompositing
        {
            get { return child != null && (_alpha != 0 && _alpha != 255); }
        }
        int _alpha;

        public float opacity
        {
            get { return _opacity; }
            set
            {
                D.assert(value >= 0.0 && value <= 1.0);
                if (_opacity == value)
                    return;
                bool didNeedCompositing = alwaysNeedsCompositing;
                _opacity = value;
                _alpha = ui.Color.getAlphaFromOpacity(_opacity);
                if (didNeedCompositing != alwaysNeedsCompositing)
                    markNeedsCompositingBitsUpdate();
                markNeedsPaint();
            }
        }
        float _opacity;

        public override void paint(PaintingContext context, Offset offset)
        {
            if (child != null && child.geometry.visible)
            {
                if (_alpha == 0)
                {
                    layer = null;
                    return;
                }
                if (_alpha == 255)
                {
                    layer = null;
                    context.paintChild(child, offset);
                    return;
                }
                D.assert(needsCompositing);
                layer = context.pushOpacity(
                    offset,
                    _alpha,
                    base.paint,
                    oldLayer: layer as OpacityLayer
                );
            }
        }

        public override void debugFillProperties(DiagnosticPropertiesBuilder properties)
        {
            base.debugFillProperties(properties);
            properties.add(new FloatProperty("opacity", opacity));
        }
    }

    public class RenderSliverIgnorePointer : RenderProxySliver
    {
        public RenderSliverIgnorePointer(
            RenderSliver sliver = null,
            bool ignoring = true
            )
        {
            _ignoring = ignoring;
            child = sliver;
        }


        public bool ignoring
        {
            get { return _ignoring; }
            set
            {
                if (value == _ignoring)
                    return;
                _ignoring = value;
            }
        }
        bool _ignoring;

        public override bool hitTest(SliverHitTestResult result, float mainAxisPosition = 0, float crossAxisPosition = 0)
        {
            return !ignoring && base.hitTest(
                result,
                mainAxisPosition: mainAxisPosition,
                crossAxisPosition: crossAxisPosition
              );
        }

        public override void debugFillProperties(DiagnosticPropertiesBuilder properties)
        {
            base.debugFillProperties(properties);
            properties.add(new DiagnosticsProperty<bool>("ignoring", ignoring));
        }
    }
    public class RenderSliverOffstage : RenderProxySliver
    {
        public RenderSliverOffstage(
            RenderSliver sliver = null,
            bool offstage = true) : base(child: sliver)
        {
            _offstage = offstage;
        }

        public bool offstage
        {
            get { return _offstage; }
            set
            {
                if (value == _offstage)
                    return;
                _offstage = value;
                markNeedsLayoutForSizedByParentChange();
            }
        }
        bool _offstage;

        protected override void performLayout()
        {
            D.assert(child != null);
            child.layout(constraints, parentUsesSize: true);
            if (!offstage)
                geometry = child.geometry;
            else
                geometry = new SliverGeometry(
                scrollExtent: 0.0f,
                visible: false,
                maxPaintExtent: 0.0f);
        }
        public override bool hitTest(SliverHitTestResult result, float mainAxisPosition = 0, float crossAxisPosition = 0)
        {
            return !offstage && base.hitTest(result, mainAxisPosition: mainAxisPosition, crossAxisPosition: crossAxisPosition);
        }
        protected override bool hitTestChildren(SliverHitTestResult result, float mainAxisPosition = 0, float crossAxisPosition = 0)
        {
            return !offstage
              && child != null
              && child.geometry.hitTestExtent > 0
              && child.hitTest(
                result,
                mainAxisPosition: mainAxisPosition,
                crossAxisPosition: crossAxisPosition
              );
        }
        public override void paint(PaintingContext context, Offset offset)
        {
            if (offstage)
                return;
            context.paintChild(child, offset);
        }

        public override void debugFillProperties(DiagnosticPropertiesBuilder properties)
        {
            base.debugFillProperties(properties);
            properties.add(new DiagnosticsProperty<bool>("offstage", offstage));
        }
        public override List<DiagnosticsNode> debugDescribeChildren()
        {
            if (child == null)
                return new List<DiagnosticsNode>();
            return new List<DiagnosticsNode>{
              child.toDiagnosticsNode(
                name: "child",
                style: offstage ? DiagnosticsTreeStyle.offstage : DiagnosticsTreeStyle.sparse
              ),
            };
        }
    }
    
}
