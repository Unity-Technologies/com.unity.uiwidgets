using Unity.UIWidgets.foundation;
using Unity.UIWidgets.rendering;

namespace Unity.UIWidgets.widgets {
    public class AnnotatedRegion<T> : SingleChildRenderObjectWidget where T : class
    {
        public AnnotatedRegion(
            Key key = null,
            Widget child = null,
            T value = default,
            bool sized = true
        ) : base(key: key, child: child) {
            D.assert(value != null);
            D.assert(child != null);
            this.value = value;
            this.sized = sized;
        }

        public readonly T value;

        public readonly bool sized;

        public override RenderObject createRenderObject(BuildContext context) {
            return new RenderAnnotatedRegion<T>(value: value, sized: sized);
        }

        public override void updateRenderObject(BuildContext context, RenderObject renderObject) {
            RenderAnnotatedRegion<T> _renderObject = (RenderAnnotatedRegion<T>) renderObject;
            _renderObject.value = value;
            _renderObject.sized = sized;
        }
    }
}