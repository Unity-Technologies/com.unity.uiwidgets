using System;
using System.Collections.Generic;
using Unity.UIWidgets.animation;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.rendering;
using Unity.UIWidgets.scheduler;
using Unity.UIWidgets.ui;
using Unity.UIWidgets.widgets;
using TextStyle = Unity.UIWidgets.painting.TextStyle;

namespace Unity.UIWidgets.material {
    static class MaterialConstantsUtils {
        public static readonly Dictionary<MaterialType, BorderRadius> kMaterialEdges =
            new Dictionary<MaterialType, BorderRadius> {
                {MaterialType.canvas, null},
                {MaterialType.card, BorderRadius.circular(2.0f)},
                {MaterialType.circle, null},
                {MaterialType.button, BorderRadius.circular(2.0f)},
                {MaterialType.transparency, null}
            };
    }


    public delegate Rect RectCallback();

    public enum MaterialType {
        canvas,
        card,
        circle,
        button,
        transparency
    }


    public interface MaterialInkController {
        Color color { get; set; }

        TickerProvider vsync { get; }

        void addInkFeature(InkFeature feature);

        void markNeedsPaint();
    }


    public class Material : StatefulWidget {
        public Material(
            Key key = null,
            MaterialType type = MaterialType.canvas,
            float elevation = 0.0f,
            Color color = null,
            Color shadowColor = null,
            TextStyle textStyle = null,
            BorderRadius borderRadius = null,
            ShapeBorder shape = null,
            bool borderOnForeground = true,
            Clip clipBehavior = Clip.none,
            TimeSpan? animationDuration = null,
            Widget child = null
        ) : base(key: key) {
            D.assert(elevation >= 0.0f);
            D.assert(!(shape != null && borderRadius != null));
            D.assert(!(type == MaterialType.circle && (borderRadius != null || shape != null)));

            this.type = type;
            this.elevation = elevation;
            this.color = color;
            this.shadowColor = shadowColor ?? new Color(0xFF000000);
            this.textStyle = textStyle;
            this.borderRadius = borderRadius;
            this.shape = shape;
            this.borderOnForeground = borderOnForeground;
            this.clipBehavior = clipBehavior;
            this.animationDuration = animationDuration ?? material_.kThemeChangeDuration;
            this.child = child;
        }

        public readonly Widget child;

        public readonly MaterialType type;

        public readonly float elevation;

        public readonly Color color;

        public readonly Color shadowColor;

        public readonly TextStyle textStyle;

        public readonly ShapeBorder shape;

        public readonly bool borderOnForeground;

        public readonly Clip clipBehavior;

        public readonly TimeSpan animationDuration;

        public readonly BorderRadius borderRadius;


        public static MaterialInkController of(BuildContext context) {
            _RenderInkFeatures result =
                (_RenderInkFeatures) context.findAncestorRenderObjectOfType<_RenderInkFeatures>();
            return result;
        }

        public override State createState() {
            return new _MaterialState();
        }


        public override void debugFillProperties(DiagnosticPropertiesBuilder properties) {
            base.debugFillProperties(properties);
            properties.add(new EnumProperty<MaterialType>("type", type));
            properties.add(new FloatProperty("elevation", elevation, defaultValue: 0.0f));
            properties.add(new ColorProperty("color", color, defaultValue: null));
            properties.add(new ColorProperty("shadowColor", shadowColor, defaultValue: new Color(0xFF000000)));
            textStyle?.debugFillProperties(properties);
            properties.add(new DiagnosticsProperty<ShapeBorder>("shape", shape, defaultValue: null));
            properties.add(new DiagnosticsProperty<bool>("borderOnForeground", borderOnForeground,
                defaultValue: null));
            properties.add(
                new DiagnosticsProperty<BorderRadius>("borderRadius", borderRadius, defaultValue: null));
        }

        public const float defaultSplashRadius = 35.0f;
    }


    class _MaterialState : TickerProviderStateMixin<Material> {
        readonly GlobalKey _inkFeatureRenderer = GlobalKey.key(debugLabel: "ink renderer");

        Color _getBackgroundColor(BuildContext context) {
            ThemeData theme = Theme.of(context);
            Color color = widget.color;
            if (color == null) {
                switch (widget.type) {
                    case MaterialType.canvas:
                        color = theme.canvasColor;
                        break;
                    case MaterialType.card:
                        color = theme.cardColor;
                        break;
                    default:
                        break;
                }
            }

            return color;
        }

        public override Widget build(BuildContext context) {
            Color backgroundColor = _getBackgroundColor(context);
            D.assert(backgroundColor != null || widget.type == MaterialType.transparency,
                () => "If Material type is not MaterialType.transparency, a color must" +
                      "either be passed in through the 'color' property, or be defined " +
                      "in the theme (ex. canvasColor != null if type is set to " +
                      "MaterialType.canvas");
            Widget contents = widget.child;
            if (contents != null) {
                contents = new AnimatedDefaultTextStyle(
                    style: widget.textStyle ?? Theme.of(context).textTheme.bodyText2,
                    duration: widget.animationDuration,
                    child: contents
                );
            }

            contents = new NotificationListener<LayoutChangedNotification>(
                onNotification: (LayoutChangedNotification notification) => {
                    _RenderInkFeatures renderer =
                        (_RenderInkFeatures) _inkFeatureRenderer.currentContext.findRenderObject();
                    renderer._didChangeLayout();
                    return false;
                },
                child: new _InkFeatures(
                    key: _inkFeatureRenderer,
                    color: backgroundColor,
                    child: contents,
                    vsync: this
                )
            );

            if (widget.type == MaterialType.canvas && widget.shape == null &&
                widget.borderRadius == null) {
                return new AnimatedPhysicalModel(
                    curve: Curves.fastOutSlowIn,
                    duration: widget.animationDuration,
                    shape: BoxShape.rectangle,
                    clipBehavior: widget.clipBehavior,
                    borderRadius: BorderRadius.zero,
                    elevation: widget.elevation,
                    color: ElevationOverlay.applyOverlay(context, backgroundColor, widget.elevation),
                    shadowColor: widget.shadowColor,
                    animateColor: false,
                    child: contents
                );
            }

            ShapeBorder shape = _getShape();

            if (widget.type == MaterialType.transparency) {
                return _transparentInterior(
                    context: context,
                    shape: shape,
                    clipBehavior: widget.clipBehavior,
                    contents: contents);
            }

            return new _MaterialInterior(
                curve: Curves.fastOutSlowIn,
                duration: widget.animationDuration,
                shape: shape,
                borderOnForeground: widget.borderOnForeground,
                clipBehavior: widget.clipBehavior,
                elevation: widget.elevation,
                color: backgroundColor,
                shadowColor: widget.shadowColor,
                child: contents
            );
        }


        static Widget _transparentInterior(
            BuildContext context = null,
            ShapeBorder shape = null,
            Clip? clipBehavior = null,
            Widget contents = null) {
            _ShapeBorderPaint child = new _ShapeBorderPaint(
                child: contents,
                shape: shape);

            if (clipBehavior == Clip.none) {
                return child;
            }

            return new ClipPath(
                child: child,
                clipper: new ShapeBorderClipper(shape: shape),
                clipBehavior: clipBehavior ?? Clip.none
            );
        }


        ShapeBorder _getShape() {
            if (widget.shape != null) {
                return widget.shape;
            }

            if (widget.borderRadius != null) {
                return new RoundedRectangleBorder(borderRadius: widget.borderRadius);
            }

            switch (widget.type) {
                case MaterialType.canvas:
                case MaterialType.transparency:
                    return new RoundedRectangleBorder();
                case MaterialType.card:
                case MaterialType.button:
                    return new RoundedRectangleBorder(
                        borderRadius: widget.borderRadius ??
                                      MaterialConstantsUtils.kMaterialEdges[widget.type]);
                case MaterialType.circle:
                    return new CircleBorder();
            }

            return new RoundedRectangleBorder();
        }
    }


    public class _RenderInkFeatures : RenderProxyBox, MaterialInkController {
        public _RenderInkFeatures(
            RenderBox child = null,
            TickerProvider vsync = null,
            Color color = null) : base(child: child) {
            D.assert(vsync != null);
            _vsync = vsync;
            _color = color;
        }

        public TickerProvider vsync {
            get { return _vsync; }
        }

        readonly TickerProvider _vsync;

        public Color color {
            get { return _color; }
            set { _color = value; }
        }

        Color _color;

        List<InkFeature> _inkFeatures;

        public void addInkFeature(InkFeature feature) {
            D.assert(!feature._debugDisposed);
            D.assert(feature._controller == this);
            _inkFeatures = _inkFeatures ?? new List<InkFeature>();
            D.assert(!_inkFeatures.Contains(feature));
            _inkFeatures.Add(feature);
            markNeedsPaint();
        }

        public void _removeFeature(InkFeature feature) {
            D.assert(_inkFeatures != null);
            _inkFeatures.Remove(feature);
            markNeedsPaint();
        }

        public void _didChangeLayout() {
            if (_inkFeatures != null && _inkFeatures.isNotEmpty()) {
                markNeedsPaint();
            }
        }

        protected override bool hitTestSelf(Offset position) {
            return true;
        }

        public override void paint(PaintingContext context, Offset offset) {
            if (_inkFeatures != null && _inkFeatures.isNotEmpty()) {
                Canvas canvas = context.canvas;
                canvas.save();
                canvas.translate(offset.dx, offset.dy);
                canvas.clipRect(Offset.zero & size);
                foreach (InkFeature inkFeature in _inkFeatures) {
                    inkFeature._paint(canvas);
                }

                canvas.restore();
            }

            base.paint(context, offset);
        }
    }


    public class _InkFeatures : SingleChildRenderObjectWidget {
        public _InkFeatures(
            Key key = null,
            Color color = null,
            TickerProvider vsync = null,
            Widget child = null) : base(key: key, child: child) {
            D.assert(vsync != null);
            this.color = color;
            this.vsync = vsync;
        }

        public readonly Color color;

        public readonly TickerProvider vsync;

        public override RenderObject createRenderObject(BuildContext context) {
            return new _RenderInkFeatures(
                color: color,
                vsync: vsync);
        }

        public override void updateRenderObject(BuildContext context, RenderObject renderObject) {
            _RenderInkFeatures _renderObject = (_RenderInkFeatures) renderObject;
            _renderObject.color = color;
            D.assert(vsync == _renderObject.vsync);
        }
    }

    public abstract class InkFeature {
        public InkFeature(
            MaterialInkController controller = null,
            RenderBox referenceBox = null,
            VoidCallback onRemoved = null) {
            D.assert(controller != null);
            D.assert(referenceBox != null);
            _controller = (_RenderInkFeatures) controller;
            this.referenceBox = referenceBox;
            this.onRemoved = onRemoved;
        }

        public MaterialInkController controller {
            get { return _controller; }
        }

        public readonly _RenderInkFeatures _controller;

        public readonly RenderBox referenceBox;

        public readonly VoidCallback onRemoved;

        public bool _debugDisposed = false;

        public virtual void dispose() {
            D.assert(!_debugDisposed);
            D.assert(() => {
                _debugDisposed = true;
                return true;
            });
            _controller._removeFeature(this);
            if (onRemoved != null) {
                onRemoved();
            }
        }

        public void _paint(Canvas canvas) {
            D.assert(referenceBox.attached);
            D.assert(!_debugDisposed);

            List<RenderObject> descendants = new List<RenderObject> {referenceBox};
            RenderObject node = referenceBox;
            while (node != _controller) {
                node = (RenderObject) node.parent;
                D.assert(node != null);
                descendants.Add(node);
            }

            Matrix4 transform = Matrix4.identity();
            D.assert(descendants.Count >= 2);
            for (int index = descendants.Count - 1; index > 0; index -= 1) {
                descendants[index].applyPaintTransform(descendants[index - 1], transform);
            }

            paintFeature(canvas, transform);
        }

        protected abstract void paintFeature(Canvas canvas, Matrix4 transform);

        public override string ToString() {
            return GetType() + "";
        }
    }

    public class ShapeBorderTween : Tween<ShapeBorder> {
        public ShapeBorderTween(
            ShapeBorder begin = null,
            ShapeBorder end = null) : base(begin: begin, end: end) {
        }

        public override ShapeBorder lerp(float t) {
            return ShapeBorder.lerp(begin, end, t);
        }
    }

    public class _MaterialInterior : ImplicitlyAnimatedWidget {
        public _MaterialInterior(
            Key key = null,
            Widget child = null,
            ShapeBorder shape = null,
            bool borderOnForeground = true,
            Clip clipBehavior = Clip.none,
            float? elevation = null,
            Color color = null,
            Color shadowColor = null,
            Curve curve = null,
            TimeSpan? duration = null
        ) : base(key: key, curve: curve ?? Curves.linear, duration: duration) {
            D.assert(child != null);
            D.assert(shape != null);
            D.assert(elevation != null && elevation >= 0.0f);
            D.assert(color != null);
            D.assert(shadowColor != null);
            D.assert(duration != null);
            this.child = child;
            this.shape = shape;
            this.borderOnForeground = borderOnForeground;
            this.clipBehavior = clipBehavior;
            this.elevation = elevation ?? 0.0f;
            this.color = color;
            this.shadowColor = shadowColor;
        }

        public readonly Widget child;

        public readonly ShapeBorder shape;

        public readonly bool borderOnForeground;

        public readonly Clip clipBehavior;

        public readonly float elevation;

        public readonly Color color;

        public readonly Color shadowColor;

        public override State createState() {
            return new _MaterialInteriorState();
        }


        public override void debugFillProperties(DiagnosticPropertiesBuilder description) {
            base.debugFillProperties(description);
            description.add(new DiagnosticsProperty<ShapeBorder>("shape", shape));
            description.add(new FloatProperty("elevation", elevation));
            description.add(new ColorProperty("color", color));
            description.add(new ColorProperty("shadowColor", shadowColor));
        }
    }

    public class _MaterialInteriorState : AnimatedWidgetBaseState<_MaterialInterior> {
        FloatTween _elevation;
        ColorTween _shadowColor;
        ShapeBorderTween _border;

        protected override void forEachTween(TweenVisitor visitor) {
            _elevation = (FloatTween) visitor.visit(this, _elevation, widget.elevation,
                (float value) => new FloatTween(begin: value, end: value));
            _shadowColor = (ColorTween) visitor.visit(this, _shadowColor, widget.shadowColor,
                (Color value) => new ColorTween(begin: value));
            _border = (ShapeBorderTween) visitor.visit(this, _border, widget.shape,
                (ShapeBorder value) => new ShapeBorderTween(begin: value));
        }

        public override Widget build(BuildContext context) {
            ShapeBorder shape = _border.evaluate(animation);
            float elevation = _elevation.evaluate(animation);
            return new PhysicalShape(
                child: new _ShapeBorderPaint(
                    child: widget.child,
                    shape: shape,
                    borderOnForeground: widget.borderOnForeground),
                clipper: new ShapeBorderClipper(
                    shape: shape,
                    textDirection: Directionality.of(context)),
                clipBehavior: widget.clipBehavior,
                elevation: _elevation.evaluate(animation),
                color: ElevationOverlay.applyOverlay(context, widget.color, elevation),
                shadowColor: _shadowColor.evaluate(animation)
            );
        }
    }

    class _ShapeBorderPaint : StatelessWidget {
        public _ShapeBorderPaint(
            Widget child = null,
            ShapeBorder shape = null,
            bool borderOnForeground = true) {
            D.assert(child != null);
            D.assert(shape != null);
            this.child = child;
            this.shape = shape;
            this.borderOnForeground = borderOnForeground;
        }

        public readonly Widget child;

        public readonly ShapeBorder shape;

        public readonly bool borderOnForeground;

        public override Widget build(BuildContext context) {
            return new CustomPaint(
                child: child,
                painter: borderOnForeground ? null : new _ShapeBorderPainter(shape),
                foregroundPainter: borderOnForeground ? new _ShapeBorderPainter(shape) : null);
        }
    }

    class _ShapeBorderPainter : AbstractCustomPainter {
        public _ShapeBorderPainter(ShapeBorder border = null) : base(null) {
            this.border = border;
        }

        public readonly ShapeBorder border;


        public override void paint(Canvas canvas, Size size) {
            border.paint(canvas, Offset.zero & size, null);
        }

        public override bool shouldRepaint(CustomPainter oldDelegate) {
            _ShapeBorderPainter _oldDelegate = (_ShapeBorderPainter) oldDelegate;
            return _oldDelegate.border != border;
        }
    }
}