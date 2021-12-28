using System;
using System.Collections.Generic;
using System.Linq;
using Unity.UIWidgets.external;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.rendering;

namespace Unity.UIWidgets.widgets {
    public class Viewport : MultiChildRenderObjectWidget {
        public Viewport(
            Key key = null,
            AxisDirection? axisDirection = AxisDirection.down,
            AxisDirection? crossAxisDirection = null,
            float anchor = 0.0f,
            ViewportOffset offset = null,
            Key center = null,
            float? cacheExtent = null,
            CacheExtentStyle cacheExtentStyle = CacheExtentStyle.pixel,
            List<Widget> slivers = null
        ) : base(key: key, children: slivers) {
            D.assert(offset != null);
            D.assert(slivers != null);
            D.assert(center == null || LinqUtils<Widget>.WhereList(slivers,((Widget child) => child.key == center)).Count() == 1);
            D.assert(cacheExtentStyle != CacheExtentStyle.viewport || cacheExtent != null);
            this.axisDirection = axisDirection;
            this.crossAxisDirection = crossAxisDirection;
            this.anchor = anchor;
            this.offset = offset;
            this.center = center;
            this.cacheExtent = cacheExtent;
            this.cacheExtentStyle = cacheExtentStyle;
        }

        public readonly AxisDirection? axisDirection;

        public readonly AxisDirection? crossAxisDirection;

        public readonly float anchor;

        public readonly ViewportOffset offset;

        public readonly Key center;

        public readonly float? cacheExtent;
        
        public readonly CacheExtentStyle cacheExtentStyle;

        public static AxisDirection? getDefaultCrossAxisDirection(BuildContext context, AxisDirection? axisDirection) {
            D.assert(axisDirection != null);
            switch (axisDirection) {
                case AxisDirection.up:
                    return AxisUtils.textDirectionToAxisDirection(Directionality.of(context));
                case AxisDirection.right:
                    return AxisDirection.down;
                case AxisDirection.down:
                    return AxisUtils.textDirectionToAxisDirection(Directionality.of(context));
                case AxisDirection.left:
                    return AxisDirection.down;
            }
            return null;
        }


        public override RenderObject createRenderObject(BuildContext context) {
            return new RenderViewport(
                axisDirection: axisDirection,
                crossAxisDirection: crossAxisDirection ?? getDefaultCrossAxisDirection(context, axisDirection),
                anchor: anchor,
                offset: offset,
                cacheExtent: cacheExtent ?? RenderViewportUtils.defaultCacheExtent,
                cacheExtentStyle: cacheExtentStyle);
        }

        public override void updateRenderObject(BuildContext context, RenderObject renderObjectRaw) {
            var renderObject = (RenderViewport) renderObjectRaw;
            renderObject.axisDirection = axisDirection.Value;
            renderObject.crossAxisDirection = (crossAxisDirection ?? getDefaultCrossAxisDirection(context, axisDirection)).Value;
            renderObject.anchor = anchor;
            renderObject.offset = offset;
            renderObject.cacheExtent = cacheExtent ?? RenderViewportUtils.defaultCacheExtent;
            renderObject.cacheExtentStyle = cacheExtentStyle;
        }

        public override Element createElement() {
            return new _ViewportElement(this);
        }

        public override void debugFillProperties(DiagnosticPropertiesBuilder properties) {
            base.debugFillProperties(properties);
            properties.add(new EnumProperty<AxisDirection?>("axisDirection", axisDirection));
            properties.add(new EnumProperty<AxisDirection?>("crossAxisDirection", crossAxisDirection,
                defaultValue: foundation_.kNullDefaultValue));
            properties.add(new FloatProperty("anchor", anchor));
            properties.add(new DiagnosticsProperty<ViewportOffset>("offset", offset));
            if (center != null) {
                properties.add(new DiagnosticsProperty<Key>("center", center));
            }
            else if (children.isNotEmpty() && children.First().key != null) {
                properties.add(new DiagnosticsProperty<Key>("center", children.First().key, tooltip: "implicit"));
            }
            properties.add(new DiagnosticsProperty<float>("cacheExtent", (float)cacheExtent));
            properties.add(new DiagnosticsProperty<CacheExtentStyle>("cacheExtentStyle", cacheExtentStyle));
        }
    }


    class _ViewportElement : MultiChildRenderObjectElement {
        internal _ViewportElement(Viewport widget) : base(widget) {
        }

        public new Viewport widget {
            get { return (Viewport) base.widget; }
        }

        public new RenderViewport renderObject {
            get { return (RenderViewport) base.renderObject; }
        }

        public override void mount(Element parent, object newSlot) {
            base.mount(parent, newSlot);
            _updateCenter();
        }

        public override void update(Widget newWidget) {
            base.update(newWidget);
            _updateCenter();
        }

        void _updateCenter() {
            if (widget.center != null) {
                renderObject.center = (RenderSliver) children.Single(
                    element => element.widget.key == widget.center).renderObject;
            }
            else if (children.Any()) {
                renderObject.center = (RenderSliver) children.First().renderObject;
            }
            else {
                renderObject.center = null;
            }
        }

        public override void debugVisitOnstageChildren(ElementVisitor visitor) {
            LinqUtils<Element>.WhereList(children, (e => {
                RenderSliver renderSliver = (RenderSliver) e.renderObject;
                return renderSliver.geometry.visible;
            })).ForEach(e => visitor(e));
        }
    }


    public class ShrinkWrappingViewport : MultiChildRenderObjectWidget {
        public ShrinkWrappingViewport(
            Key key = null,
            AxisDirection? axisDirection = AxisDirection.down,
            AxisDirection? crossAxisDirection = null,
            ViewportOffset offset = null,
            List<Widget> slivers = null
        ) : base(key: key, children: slivers ?? new List<Widget>()) {
            D.assert(offset != null);
            this.axisDirection = axisDirection;
            this.crossAxisDirection = crossAxisDirection;
            this.offset = offset;
        }

        public readonly AxisDirection? axisDirection;

        public readonly AxisDirection? crossAxisDirection;

        public readonly ViewportOffset offset;

        public override RenderObject createRenderObject(BuildContext context) {
            return new RenderShrinkWrappingViewport(
                axisDirection: axisDirection,
                crossAxisDirection: crossAxisDirection
                                    ?? Viewport.getDefaultCrossAxisDirection(context, axisDirection),
                offset: offset
            );
        }

        public override void updateRenderObject(BuildContext context, RenderObject renderObjectRaw) {
            var renderObject = (RenderShrinkWrappingViewport) renderObjectRaw;
            renderObject.axisDirection = axisDirection.Value;
            renderObject.crossAxisDirection = (crossAxisDirection ?? Viewport.getDefaultCrossAxisDirection(context, axisDirection)).Value;
            renderObject.offset = offset;
        }

        public override void debugFillProperties(DiagnosticPropertiesBuilder properties) {
            base.debugFillProperties(properties);
            properties.add(new EnumProperty<AxisDirection?>("axisDirection", axisDirection));
            properties.add(new EnumProperty<AxisDirection?>("crossAxisDirection", crossAxisDirection,
                defaultValue: foundation_.kNullDefaultValue));
            properties.add(new DiagnosticsProperty<ViewportOffset>("offset", offset));
        }
    }
}