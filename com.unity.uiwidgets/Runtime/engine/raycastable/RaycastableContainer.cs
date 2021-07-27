using Unity.UIWidgets.foundation;
using Unity.UIWidgets.rendering;
using Unity.UIWidgets.ui;
using Unity.UIWidgets.widgets;

namespace Unity.UIWidgets.engine {
    class RaycastableBox : SingleChildRenderObjectWidget {
        public RaycastableBox(
            Key key = null,
            Widget child = null
        ) : base(key, child) {
            windowHashCode = Isolate.current.GetHashCode();
        }

        readonly int windowHashCode;

        public override RenderObject createRenderObject(BuildContext context) {
            return new RenderRaycastableBox(
                windowHashCode: windowHashCode,
                widget: this
            );
        }

        public override Element createElement() {
            return new _RaycastableBoxRenderElement(windowHashCode: windowHashCode, widget: this);
        }
    }

    class RenderRaycastableBox : RenderProxyBox {
        public RenderRaycastableBox(
            int windowHashCode,
            RaycastableBox widget
        ) {
            widgetHashCode = widget.GetHashCode();
            this.windowHashCode = windowHashCode;
        }

        readonly int widgetHashCode;
        readonly int windowHashCode;

        public override void paint(PaintingContext context, Offset offset) {
            RaycastManager.UpdateSizeOffset(widgetHashCode, windowHashCode, size, offset);

            base.paint(context, offset);
        }
    }

    class _RaycastableBoxRenderElement : SingleChildRenderObjectElement {
        public _RaycastableBoxRenderElement(
            int windowHashCode,
            RaycastableBox widget
        ) : base(widget) {
            this.windowHashCode = windowHashCode;
        }

        public new RaycastableBox widget {
            get { return base.widget as RaycastableBox; }
        }

        int widgetHashCode;
        int windowHashCode;

        public override void mount(Element parent, object newSlot) {
            widgetHashCode = widget.GetHashCode();
            RaycastManager.AddToList(widgetHashCode, windowHashCode);
            base.mount(parent, newSlot);
        }

        public override void update(Widget newWidget) {
            RaycastManager.MarkDirty(widgetHashCode, windowHashCode);
            base.update(newWidget);
        }

        public override void unmount() {
            RaycastManager.RemoveFromList(widgetHashCode, windowHashCode);
            base.unmount();
        }
    }

    public class RaycastableContainer : StatelessWidget {
        public RaycastableContainer(
            Widget child = null,
            Key key = null
        ) : base(key) {
            this.child = child;
        }

        public readonly Widget child;

        public override Widget build(BuildContext context) {
            Widget current = child;

            if (child == null) {
                current = new LimitedBox(
                    maxWidth: 0.0f,
                    maxHeight: 0.0f,
                    child: new ConstrainedBox(constraints: BoxConstraints.expand())
                );
            }

            current = new RaycastableBox(child: current);

            return current;
        }
    }
}