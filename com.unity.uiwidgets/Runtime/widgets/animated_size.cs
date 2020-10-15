using System;
using Unity.UIWidgets.animation;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.rendering;
using Unity.UIWidgets.scheduler2;

namespace Unity.UIWidgets.widgets {
    public class AnimatedSize : SingleChildRenderObjectWidget {
        public AnimatedSize(
            Key key = null,
            Widget child = null,
            Alignment alignment = null,
            Curve curve = null,
            TimeSpan? duration = null,
            TickerProvider vsync = null) : base(key: key, child: child) {
            D.assert(duration != null);
            D.assert(vsync != null);
            this.alignment = alignment ?? Alignment.center;
            this.curve = curve ?? Curves.linear;
            this.duration = duration ?? TimeSpan.Zero;
            this.vsync = vsync;
        }

        public readonly Alignment alignment;

        public readonly Curve curve;

        public readonly TimeSpan duration;

        public readonly TickerProvider vsync;

        public override RenderObject createRenderObject(BuildContext context) {
            return new RenderAnimatedSize(
                alignment: alignment,
                duration: duration,
                curve: curve,
                vsync: vsync);
        }

        public override void updateRenderObject(BuildContext context, RenderObject renderObject) {
            RenderAnimatedSize _renderObject = (RenderAnimatedSize) renderObject;
            _renderObject.alignment = alignment;
            _renderObject.duration = duration;
            _renderObject.curve = curve;
            _renderObject.vsync = vsync;
        }
    }
}