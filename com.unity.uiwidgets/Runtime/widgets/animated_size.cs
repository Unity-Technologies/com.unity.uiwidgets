using System;
using Unity.UIWidgets.animation;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.rendering;
using Unity.UIWidgets.scheduler;

namespace Unity.UIWidgets.widgets {
    public class AnimatedSize : SingleChildRenderObjectWidget {
        public AnimatedSize(
            Key key = null,
            Widget child = null,
            AlignmentGeometry alignment = null,
            Curve curve = null,
            TimeSpan? duration = null,
            TimeSpan? reverseDuration = null,
            TickerProvider vsync = null) : base(key: key, child: child) {
            D.assert(duration != null);
            D.assert(vsync != null);
            this.alignment = alignment ?? Alignment.center;
            this.curve = curve ?? Curves.linear;
            this.duration = duration;
            this.reverseDuration = reverseDuration;
            this.vsync = vsync;
        }

        public readonly AlignmentGeometry alignment;

        public readonly Curve curve;

        public readonly TimeSpan? duration;

        public readonly TimeSpan? reverseDuration;
        
        public readonly TickerProvider vsync;

        public override RenderObject createRenderObject(BuildContext context) {
            return new RenderAnimatedSize(
                alignment: alignment,
                duration: duration,
                reverseDuration: reverseDuration,
                curve: curve,
                vsync: vsync,
                textDirection: Directionality.of(context)
                );
        }

        public override void updateRenderObject(BuildContext context, RenderObject renderObject) {
            RenderAnimatedSize _renderObject = (RenderAnimatedSize) renderObject;
            _renderObject.alignment = alignment;
            _renderObject.duration = duration;
            _renderObject.reverseDuration = reverseDuration;
            _renderObject.curve = curve;
            _renderObject.vsync = vsync;
            _renderObject.textDirection = Directionality.of(context);
        }
        
        public override void debugFillProperties(DiagnosticPropertiesBuilder properties) {
            base.debugFillProperties(properties);

            properties.add(new DiagnosticsProperty<AlignmentGeometry>("alignment", alignment, defaultValue: Alignment.topCenter));
            
            properties.add(new IntProperty("duration", duration?.Milliseconds, unit: "ms"));
            properties.add(new IntProperty("reverseDuration", reverseDuration?.Milliseconds, unit: "ms", defaultValue: null));
        }
    }
}