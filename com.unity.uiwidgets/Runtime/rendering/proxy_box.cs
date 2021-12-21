using System;
using System.Collections.Generic;
using Unity.UIWidgets.animation;
using Unity.UIWidgets.async;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.gestures;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.ui;
using Gradient = Unity.UIWidgets.ui.Gradient;
using TextStyle = Unity.UIWidgets.painting.TextStyle;

namespace Unity.UIWidgets.rendering {
    public class RenderProxyBox : RenderProxyBoxMixinRenderObjectWithChildMixinRenderBox<RenderBox> {
        public RenderProxyBox(RenderBox child = null) {
            this.child = child;
        }
    }

    public enum HitTestBehavior {
        deferToChild,
        opaque,
        translucent,
    }

    public delegate Shader ShaderCallback(Rect bounds);

    class RenderShaderMask : RenderProxyBox {
        public RenderShaderMask(
            RenderBox child = null,
            ShaderCallback shaderCallback = null,
            BlendMode blendMode = BlendMode.modulate
        ) : base(child) {
            D.assert(shaderCallback != null);
            _shaderCallback = shaderCallback;
            _blendMode = blendMode;
        }

        public new ShaderMaskLayer layer {
            get { return base.layer as ShaderMaskLayer; }
            set { }
        }

        public ShaderCallback shaderCallback {
            get { return _shaderCallback; }
            set {
                D.assert(value != null);
                if (_shaderCallback == value) {
                    return;
                }

                _shaderCallback = value;
                markNeedsPaint();
            }
        }

        ShaderCallback _shaderCallback;

        public BlendMode blendMode {
            get { return _blendMode; }
            set {
                if (_blendMode == value) {
                    return;
                }

                _blendMode = value;
                markNeedsPaint();
            }
        }

        BlendMode _blendMode;

        protected override bool alwaysNeedsCompositing {
            get { return child != null; }
        }

        public override void paint(PaintingContext context, Offset offset) {
            if (child != null) {
                D.assert(needsCompositing);
                layer = layer ?? new ShaderMaskLayer();
                layer.shader = _shaderCallback(Offset.zero & size);
                layer.maskRect = offset & size;
                layer.blendMode = _blendMode;
                context.pushLayer(layer, base.paint, offset);
            }
            else {
                layer = null;
            }
        }
    }

    public interface RenderAnimatedOpacityMixin<T> : RenderObjectWithChildMixin<T> where T : RenderObject {
        int _alpha { get; set; }
        bool _currentlyNeedsCompositing { get; set; }
        Animation<float> _opacity { get; set; }

        void attach(PipelineOwner owner);
        void detach();

        void _updateOpacity();
        void paint(PaintingContext context, Offset offset);
        void visitChildrenForSemantics(RenderObjectVisitor visitor);
        void debugFillProperties(DiagnosticPropertiesBuilder properties);
    }


    public abstract class RenderProxyBoxWithHitTestBehavior : RenderProxyBox {
        protected RenderProxyBoxWithHitTestBehavior(
            HitTestBehavior behavior = HitTestBehavior.deferToChild,
            RenderBox child = null
        ) : base(child) {
            this.behavior = behavior;
        }

        public HitTestBehavior behavior;

        public override bool hitTest(BoxHitTestResult result, Offset position = null) {
            bool hitTarget = false;
            if (size.contains(position)) {
                hitTarget = hitTestChildren(result, position: position) || hitTestSelf(position);
                if (hitTarget || behavior == HitTestBehavior.translucent) {
                    result.add(new BoxHitTestEntry(this, position));
                }
            }

            return hitTarget;
        }

        protected override bool hitTestSelf(Offset position) {
            return behavior == HitTestBehavior.opaque;
        }

        public override void debugFillProperties(DiagnosticPropertiesBuilder properties) {
            base.debugFillProperties(properties);
            properties.add(new EnumProperty<HitTestBehavior>(
                "behavior", behavior, defaultValue: foundation_.kNullDefaultValue));
        }
    }

    public class RenderConstrainedBox : RenderProxyBox {
        public RenderConstrainedBox(
            RenderBox child = null,
            BoxConstraints additionalConstraints = null) : base(child) {
            D.assert(additionalConstraints != null);
            D.assert(additionalConstraints.debugAssertIsValid());

            _additionalConstraints = additionalConstraints;
        }

        public BoxConstraints additionalConstraints {
            get { return _additionalConstraints; }
            set {
                D.assert(value != null);
                D.assert(value.debugAssertIsValid());

                if (_additionalConstraints == value) {
                    return;
                }

                _additionalConstraints = value;
                markNeedsLayout();
            }
        }

        BoxConstraints _additionalConstraints;

        protected internal override float computeMinIntrinsicWidth(float height) {
            if (_additionalConstraints.hasBoundedWidth && _additionalConstraints.hasTightWidth) {
                return _additionalConstraints.minWidth;
            }

            float width = base.computeMinIntrinsicWidth(height);
            D.assert(width.isFinite());

            if (!_additionalConstraints.hasInfiniteWidth) {
                return _additionalConstraints.constrainWidth(width);
            }

            return width;
        }

        protected internal override float computeMaxIntrinsicWidth(float height) {
            if (_additionalConstraints.hasBoundedWidth && _additionalConstraints.hasTightWidth) {
                return _additionalConstraints.minWidth;
            }

            float width = base.computeMaxIntrinsicWidth(height);
            D.assert(width.isFinite());

            if (!_additionalConstraints.hasInfiniteWidth) {
                return _additionalConstraints.constrainWidth(width);
            }

            return width;
        }

        protected internal override float computeMinIntrinsicHeight(float width) {
            if (_additionalConstraints.hasBoundedHeight && _additionalConstraints.hasTightHeight) {
                return _additionalConstraints.minHeight;
            }

            float height = base.computeMinIntrinsicHeight(width);
            D.assert(height.isFinite());

            if (!_additionalConstraints.hasInfiniteHeight) {
                return _additionalConstraints.constrainHeight(height);
            }

            return height;
        }

        protected internal override float computeMaxIntrinsicHeight(float width) {
            if (_additionalConstraints.hasBoundedHeight && _additionalConstraints.hasTightHeight) {
                return _additionalConstraints.minHeight;
            }

            float height = base.computeMaxIntrinsicHeight(width);
            D.assert(height.isFinite());

            if (!_additionalConstraints.hasInfiniteHeight) {
                return _additionalConstraints.constrainHeight(height);
            }

            return height;
        }

        protected override void performLayout() {
            BoxConstraints constraints = this.constraints;
            if (child != null) {
                child.layout(_additionalConstraints.enforce(constraints), parentUsesSize: true);
                size = child.size;
            }
            else {
                size = _additionalConstraints.enforce(constraints).constrain(Size.zero);
            }
        }

        protected override void debugPaintSize(PaintingContext context, Offset offset) {
            base.debugPaintSize(context, offset);
            D.assert(() => {
                if (child == null || child.size.isEmpty) {
                    var paint = new Paint {
                        color = new Color(0x90909090)
                    };
                    context.canvas.drawRect(offset & size, paint);
                }

                return true;
            });
        }

        public override void debugFillProperties(DiagnosticPropertiesBuilder properties) {
            base.debugFillProperties(properties);
            properties.add(
                new DiagnosticsProperty<BoxConstraints>("additionalConstraints", additionalConstraints));
        }
    }

    public class RenderLimitedBox : RenderProxyBox {
        public RenderLimitedBox(
            RenderBox child = null,
            float maxWidth = float.PositiveInfinity,
            float maxHeight = float.PositiveInfinity
        ) : base(child) {
            D.assert(maxWidth >= 0.0);
            D.assert(maxHeight >= 0.0);

            _maxWidth = maxWidth;
            _maxHeight = maxHeight;
        }

        public float maxWidth {
            get { return _maxWidth; }
            set {
                D.assert(value >= 0.0);
                if (_maxWidth == value) {
                    return;
                }

                _maxWidth = value;
                markNeedsLayout();
            }
        }

        float _maxWidth;

        public float maxHeight {
            get { return _maxHeight; }
            set {
                D.assert(value >= 0.0);
                if (_maxHeight == value) {
                    return;
                }

                _maxHeight = value;
                markNeedsLayout();
            }
        }

        float _maxHeight;

        BoxConstraints _limitConstraints(BoxConstraints constraints) {
            return new BoxConstraints(
                minWidth: constraints.minWidth,
                maxWidth: constraints.hasBoundedWidth
                    ? constraints.maxWidth
                    : constraints.constrainWidth(maxWidth),
                minHeight: constraints.minHeight,
                maxHeight: constraints.hasBoundedHeight
                    ? constraints.maxHeight
                    : constraints.constrainHeight(maxHeight)
            );
        }

        protected override void performLayout() {
            if (child != null) {
                BoxConstraints constraints = this.constraints;
                child.layout(_limitConstraints(constraints), parentUsesSize: true);
                size = constraints.constrain(child.size);
            }
            else {
                size = _limitConstraints(constraints).constrain(Size.zero);
            }
        }

        public override void debugFillProperties(DiagnosticPropertiesBuilder properties) {
            base.debugFillProperties(properties);
            properties.add(new FloatProperty("maxWidth", maxWidth, defaultValue: float.PositiveInfinity));
            properties.add(new FloatProperty("maxHeight", maxHeight, defaultValue: float.PositiveInfinity));
        }
    }

    public class RenderAspectRatio : RenderProxyBox {
        public RenderAspectRatio(
            float aspectRatio,
            RenderBox child = null) : base(child) {
            _aspectRatio = aspectRatio;
        }

        public float aspectRatio {
            get { return _aspectRatio; }
            set {
                if (_aspectRatio == value) {
                    return;
                }

                _aspectRatio = value;
                markNeedsLayout();
            }
        }

        float _aspectRatio;


        protected internal override float computeMinIntrinsicWidth(float height) {
            if (height.isFinite()) {
                return height * _aspectRatio;
            }

            if (child != null) {
                return child.getMinIntrinsicWidth(height);
            }

            return 0.0f;
        }

        protected internal override float computeMaxIntrinsicWidth(float height) {
            if (height.isFinite()) {
                return height * _aspectRatio;
            }

            if (child != null) {
                return child.getMaxIntrinsicWidth(height);
            }

            return 0.0f;
        }

        protected internal override float computeMinIntrinsicHeight(float width) {
            if (width.isFinite()) {
                return width / _aspectRatio;
            }

            if (child != null) {
                return child.getMinIntrinsicHeight(width);
            }

            return 0.0f;
        }

        protected internal override float computeMaxIntrinsicHeight(float width) {
            if (width.isFinite()) {
                return width / _aspectRatio;
            }

            if (child != null) {
                return child.getMaxIntrinsicHeight(width);
            }

            return 0.0f;
        }

        Size _applyAspectRatio(BoxConstraints constraints) {
            D.assert(constraints.debugAssertIsValid());
            if (constraints.isTight) {
                return constraints.smallest;
            }

            float width = constraints.maxWidth;
            float height = width / _aspectRatio;

            if (width.isFinite()) {
                height = width / _aspectRatio;
            }
            else {
                height = constraints.maxHeight;
                width = height * _aspectRatio;
            }

            if (width > constraints.maxWidth) {
                width = constraints.maxWidth;
                height = width / _aspectRatio;
            }

            if (height > constraints.maxHeight) {
                height = constraints.maxHeight;
                width = height * _aspectRatio;
            }

            if (width < constraints.minWidth) {
                width = constraints.minWidth;
                height = width / _aspectRatio;
            }

            if (height < constraints.minHeight) {
                height = constraints.minHeight;
                width = height * _aspectRatio;
            }

            return constraints.constrain(new Size(width, height));
        }

        protected override void performLayout() {
            size = _applyAspectRatio(constraints);
            if (child != null) {
                child.layout(BoxConstraints.tight(size));
            }
        }

        public override void debugFillProperties(DiagnosticPropertiesBuilder properties) {
            base.debugFillProperties(properties);
            properties.add(new FloatProperty("aspectRatio", aspectRatio));
        }
    }

    public class RenderIntrinsicWidth : RenderProxyBox {
        public RenderIntrinsicWidth(
            float? stepWidth = null,
            float? stepHeight = null,
            RenderBox child = null
        ) : base(child) {
            D.assert(stepWidth == null || stepWidth > 0.0f);
            D.assert(stepHeight == null || stepHeight > 0.0f);
            _stepWidth = stepWidth;
            _stepHeight = stepHeight;
        }

        float? _stepWidth;

        public float? stepWidth {
            get { return _stepWidth; }
            set {
                D.assert(value == null || value > 0.0f);
                if (value == _stepWidth) {
                    return;
                }

                _stepWidth = value;
                markNeedsLayout();
            }
        }

        float? _stepHeight;

        public float? stepHeight {
            get { return _stepHeight; }
            set {
                D.assert(value == null || value > 0.0f);
                if (value == _stepHeight) {
                    return;
                }

                _stepHeight = value;
                markNeedsLayout();
            }
        }

        static float _applyStep(float input, float? step) {
            D.assert(input.isFinite());
            if (step == null) {
                return input;
            }

            return (input / step.Value).ceil() * step.Value;
        }

        protected internal override float computeMinIntrinsicWidth(float height) {
            return computeMaxIntrinsicWidth(height);
        }

        protected internal override float computeMaxIntrinsicWidth(float height) {
            if (child == null) {
                return 0.0f;
            }

            float width = child.getMaxIntrinsicWidth(height);
            return _applyStep(width, _stepWidth);
        }

        protected internal override float computeMinIntrinsicHeight(float width) {
            if (child == null) {
                return 0.0f;
            }

            if (!width.isFinite()) {
                width = computeMaxIntrinsicWidth(float.PositiveInfinity);
            }

            D.assert(width.isFinite());
            float height = child.getMinIntrinsicHeight(width);
            return _applyStep(height, _stepHeight);
        }

        protected internal override float computeMaxIntrinsicHeight(float width) {
            if (child == null) {
                return 0.0f;
            }

            if (!width.isFinite()) {
                width = computeMaxIntrinsicWidth(float.PositiveInfinity);
            }

            D.assert(width.isFinite());
            float height = child.getMaxIntrinsicHeight(width);
            return _applyStep(height, _stepHeight);
        }

        protected override void performLayout() {
            if (child != null) {
                BoxConstraints childConstraints = constraints;
                if (!childConstraints.hasTightWidth) {
                    float width = child.getMaxIntrinsicWidth(childConstraints.maxHeight);
                    D.assert(width.isFinite());
                    childConstraints = childConstraints.tighten(width: _applyStep(width, _stepWidth));
                }

                if (_stepHeight != null) {
                    float height = child.getMaxIntrinsicHeight(childConstraints.maxWidth);
                    D.assert(height.isFinite());
                    childConstraints = childConstraints.tighten(height: _applyStep(height, _stepHeight));
                }

                child.layout(childConstraints, parentUsesSize: true);
                size = child.size;
            }
            else {
                performResize();
            }
        }

        public override void debugFillProperties(DiagnosticPropertiesBuilder properties) {
            base.debugFillProperties(properties);
            properties.add(new FloatProperty("stepWidth", stepWidth));
            properties.add(new FloatProperty("stepHeight", stepHeight));
        }
    }

    public class RenderIntrinsicHeight : RenderProxyBox {
        public RenderIntrinsicHeight(
            RenderBox child = null
        ) : base(child) {
        }

        protected internal override float computeMinIntrinsicWidth(float height) {
            if (child == null) {
                return 0.0f;
            }

            if (!height.isFinite()) {
                height = child.getMaxIntrinsicHeight(float.PositiveInfinity);
            }

            D.assert(height.isFinite());
            return child.getMinIntrinsicWidth(height);
        }

        protected internal override float computeMaxIntrinsicWidth(float height) {
            if (child == null) {
                return 0.0f;
            }

            if (!height.isFinite()) {
                height = child.getMaxIntrinsicHeight(float.PositiveInfinity);
            }

            D.assert(height.isFinite());
            return child.getMaxIntrinsicWidth(height);
        }

        protected internal override float computeMinIntrinsicHeight(float width) {
            return computeMaxIntrinsicHeight(width);
        }

        protected override void performLayout() {
            if (child != null) {
                BoxConstraints childConstraints = constraints;
                if (!childConstraints.hasTightHeight) {
                    float height = child.getMaxIntrinsicHeight(childConstraints.maxWidth);
                    D.assert(height.isFinite());
                    childConstraints = childConstraints.tighten(height: height);
                }

                child.layout(childConstraints, parentUsesSize: true);
                size = child.size;
            }
            else {
                performResize();
            }
        }
    }

    public class RenderOpacity : RenderProxyBox {
        public RenderOpacity(float opacity = 1.0f, RenderBox child = null) : base(child) {
            D.assert(opacity >= 0.0 && opacity <= 1.0);
            _opacity = opacity;
            _alpha = _getAlphaFromOpacity(opacity);
        }

        protected override bool alwaysNeedsCompositing {
            get { return child != null && (_alpha != 0 && _alpha != 255); }
        }

        int _alpha;

        internal static int _getAlphaFromOpacity(float opacity) {
            return (opacity * 255).round();
        }

        float _opacity;

        public float opacity {
            get { return _opacity; }
            set {
                D.assert(value >= 0.0 && value <= 1.0);
                if (_opacity == value) {
                    return;
                }

                bool didNeedCompositing = alwaysNeedsCompositing;

                _opacity = value;
                _alpha = _getAlphaFromOpacity(_opacity);

                if (didNeedCompositing != alwaysNeedsCompositing) {
                    markNeedsCompositingBitsUpdate();
                }

                markNeedsPaint();
            }
        }

        public override void paint(PaintingContext context, Offset offset) {
            if (child != null) {
                if (_alpha == 0) {
                    layer = null;
                    return;
                }
            }

            if (_alpha == 255) {
                layer = null;
                context.paintChild(child, offset);
                return;
            }

            D.assert(needsCompositing);
            layer = context.pushOpacity(offset, _alpha, base.paint, oldLayer: layer as OpacityLayer);
        }

        public override void debugFillProperties(DiagnosticPropertiesBuilder properties) {
            base.debugFillProperties(properties);
            properties.add(new FloatProperty("opacity", opacity));
        }
    }

    public class RenderAnimatedOpacity : RenderProxyBox {
        public RenderAnimatedOpacity(
            Animation<float> opacity = null,
            RenderBox child = null
        ) : base(child) {
            D.assert(opacity != null);
            this.opacity = opacity;
        }


        int _alpha;

        protected override bool alwaysNeedsCompositing {
            get { return child != null && _currentlyNeedsCompositing; }
        }

        bool _currentlyNeedsCompositing;

        Animation<float> _opacity;

        public Animation<float> opacity {
            get { return _opacity; }
            set {
                D.assert(value != null);
                if (_opacity == value) {
                    return;
                }

                if (attached && _opacity != null) {
                    _opacity.removeListener(_updateOpacity);
                }

                _opacity = value;
                if (attached) {
                    _opacity.addListener(_updateOpacity);
                }

                _updateOpacity();
            }
        }


        public override void attach(object owner) {
            base.attach(owner);
            _opacity.addListener(_updateOpacity);
            _updateOpacity(); // in case it changed while we weren't listening
        }

        public override void detach() {
            _opacity.removeListener(_updateOpacity);
            base.detach();
        }

        public void _updateOpacity() {
            var oldAlpha = _alpha;
            _alpha = RenderOpacity._getAlphaFromOpacity(_opacity.value.clamp(0.0f, 1.0f));
            if (oldAlpha != _alpha) {
                bool didNeedCompositing = _currentlyNeedsCompositing;
                _currentlyNeedsCompositing = _alpha > 0 && _alpha < 255;
                if (child != null && didNeedCompositing != _currentlyNeedsCompositing) {
                    markNeedsCompositingBitsUpdate();
                }

                markNeedsPaint();
            }
        }

        public override void paint(PaintingContext context, Offset offset) {
            if (child != null) {
                if (_alpha == 0) {
                    return;
                }

                if (_alpha == 255) {
                    context.paintChild(child, offset);
                    return;
                }

                D.assert(needsCompositing);
                context.pushOpacity(offset, _alpha, base.paint);
            }
        }

        public override void debugFillProperties(DiagnosticPropertiesBuilder properties) {
            base.debugFillProperties(properties);
            properties.add(new DiagnosticsProperty<Animation<float>>("opacity", opacity));
        }
    }

    public class RenderBackdropFilter : RenderProxyBox {
        public RenderBackdropFilter(
            RenderBox child = null,
            ImageFilter filter = null
        ) : base(child) {
            D.assert(filter != null);
            _filter = filter;
        }

        ImageFilter _filter;

        public ImageFilter filter {
            get { return _filter; }
            set {
                D.assert(value != null);
                if (_filter == value) {
                    return;
                }

                markNeedsPaint();
            }
        }

        protected override bool alwaysNeedsCompositing {
            get { return child != null; }
        }

        public override void paint(PaintingContext context, Offset offset) {
            if (child != null) {
                D.assert(needsCompositing);
                layer = layer ?? new BackdropFilterLayer(_filter);
                context.pushLayer(layer, base.paint, offset);
            }
            else {
                layer = null;
            }
        }
    }

    public abstract class CustomClipper<T> {
        public CustomClipper(Listenable reclip = null) {
            _reclip = reclip;
        }

        public readonly Listenable _reclip;

        public abstract T getClip(Size size);

        public virtual Rect getApproximateClipRect(Size size) {
            return Offset.zero & size;
        }

        public abstract bool shouldReclip(CustomClipper<T> oldClipper);

        public override string ToString() {
            return GetType() + "";
        }
    }

    public class ShapeBorderClipper : CustomClipper<Path> {
        public ShapeBorderClipper(
            ShapeBorder shape = null,
            TextDirection? textDirection = null
        ) {
            D.assert(shape != null);
            this.shape = shape;
            this.textDirection = textDirection;
        }

        public readonly ShapeBorder shape;
        public readonly TextDirection? textDirection;

        public override Path getClip(Size size) {
            return shape.getOuterPath(Offset.zero & size, textDirection: textDirection);
        }

        public override bool shouldReclip(CustomClipper<Path> oldClipper) {
            if (oldClipper.GetType() != typeof(ShapeBorderClipper)) {
                return true;
            }

            ShapeBorderClipper typedOldClipper = oldClipper as ShapeBorderClipper;
            return typedOldClipper.shape != shape
                   || typedOldClipper.textDirection != textDirection;
        }
    }

    public abstract class _RenderCustomClip<T> : RenderProxyBox where T : class {
        protected _RenderCustomClip(
            RenderBox child = null,
            CustomClipper<T> clipper = null,
            Clip clipBehavior = Clip.antiAlias) : base(child: child) {
            this.clipBehavior = clipBehavior;
            _clipper = clipper;
        }

        public CustomClipper<T> clipper {
            get { return _clipper; }
            set {
                if (_clipper == value) {
                    return;
                }

                CustomClipper<T> oldClipper = _clipper;
                _clipper = value;
                D.assert(value != null || oldClipper != null);
                if (value == null || oldClipper == null ||
                    value.GetType() != oldClipper.GetType() ||
                    value.shouldReclip(oldClipper)) {
                    _markNeedsClip();
                }

                if (attached) {
                    oldClipper?._reclip?.removeListener(_markNeedsClip);
                    value?._reclip?.addListener(_markNeedsClip);
                }
            }
        }

        protected CustomClipper<T> _clipper;

        public override void attach(object owner) {
            base.attach(owner);
            _clipper?._reclip?.addListener(_markNeedsClip);
        }

        public override void detach() {
            _clipper?._reclip?.removeListener(_markNeedsClip);
            base.detach();
        }

        protected void _markNeedsClip() {
            _clip = null;
            markNeedsPaint();
        }

        protected abstract T _defaultClip { get; }
        protected T _clip;

        protected Clip _clipBehavior;


        public Clip clipBehavior {
            get { return _clipBehavior; }
            set {
                if (_clipBehavior == value) {
                    return;
                }

                _clipBehavior = value;
                markNeedsPaint();
            }
        }

        protected override void performLayout() {
            Size oldSize = hasSize ? size : null;
            base.performLayout();
            if (oldSize != size) {
                _clip = null;
            }
        }

        protected void _updateClip() {
            _clip = _clip ?? _clipper?.getClip(size) ?? _defaultClip;
        }

        public override Rect describeApproximatePaintClip(RenderObject child) {
            return _clipper?.getApproximateClipRect(size) ?? Offset.zero & size;
        }

        protected Paint _debugPaint;
        protected TextPainter _debugText;

        protected override void debugPaintSize(PaintingContext context, Offset offset) {
            D.assert(() => {
                if (_debugPaint == null) {
                    _debugPaint = new Paint();
                    _debugPaint.shader = Gradient.linear(
                        new Offset(0.0f, 0.0f),
                        new Offset(10.0f, 10.0f),
                        new List<Color> {
                            new Color(0x00000000),
                            new Color(0xFFFF00FF),
                            new Color(0xFFFF00FF),
                            new Color(0x00000000)
                        },
                        new List<float> {0.25f, 0.25f, 0.75f, 0.75f},
                        TileMode.repeated);
                    _debugPaint.strokeWidth = 2.0f;
                    _debugPaint.style = PaintingStyle.stroke;
                }

                if (_debugText == null) {
                    _debugText = new TextPainter(
                        text: new TextSpan(
                            text: "✂",
                            style: new TextStyle(
                                color: new Color(0xFFFF00FF),
                                fontSize: 14.0f)
                        ),
                        textDirection: TextDirection.rtl
                    );
                    _debugText.layout();
                }

                return true;
            });
        }
    }

    public class RenderClipRect : _RenderCustomClip<Rect> {
        public RenderClipRect(
            RenderBox child = null,
            CustomClipper<Rect> clipper = null,
            Clip clipBehavior = Clip.antiAlias
        ) : base(
            child: child,
            clipper: clipper,
            clipBehavior: clipBehavior) {
            D.assert(clipBehavior != Clip.none);
        }

        protected override Rect _defaultClip {
            get { return Offset.zero & size; }
        }

        public override bool hitTest(BoxHitTestResult result, Offset position = null) {
            if (_clipper != null) {
                _updateClip();
                D.assert(_clip != null);
                if (!_clip.contains(position)) {
                    return false;
                }
            }

            return base.hitTest(result, position: position);
        }

        public override void paint(PaintingContext context, Offset offset) {
            if (child != null) {
                _updateClip();
                layer = context.pushClipRect(
                    needsCompositing,
                    offset,
                    _clip,
                    base.paint,
                    clipBehavior: clipBehavior,
                    oldLayer: layer as ClipRectLayer
                );
            }
            else {
                layer = null;
            }
        }

        protected override void debugPaintSize(PaintingContext context, Offset offset) {
            D.assert(() => {
                if (child != null) {
                    base.debugPaintSize(context, offset);
                    context.canvas.drawRect(_clip.shift(offset), _debugPaint);
                    _debugText.paint(context.canvas,
                        offset + new Offset(_clip.width / 8.0f,
                            -(_debugText.text.style.fontSize ?? 0.0f) * 1.1f));
                }

                return true;
            });
        }
    }


    public class RenderClipRRect : _RenderCustomClip<RRect> {
        public RenderClipRRect(
            RenderBox child = null,
            BorderRadius borderRadius = null,
            CustomClipper<RRect> clipper = null,
            Clip clipBehavior = Clip.antiAlias
        ) : base(child: child, clipper: clipper, clipBehavior: clipBehavior) {
            D.assert(clipBehavior != Clip.none);
            _borderRadius = borderRadius ?? BorderRadius.zero;
            D.assert(_borderRadius != null || clipper != null);
        }

        public BorderRadius borderRadius {
            get { return _borderRadius; }
            set {
                D.assert(value != null);
                if (_borderRadius == value) {
                    return;
                }

                _borderRadius = value;
                _markNeedsClip();
            }
        }

        BorderRadius _borderRadius;

        protected override RRect _defaultClip {
            get { return _borderRadius.toRRect(Offset.zero & size); }
        }

        public override bool hitTest(BoxHitTestResult result, Offset position = null) {
            if (_clipper != null) {
                _updateClip();
                D.assert(_clip != null);
                if (!_clip.contains(position)) {
                    return false;
                }
            }

            return base.hitTest(result, position: position);
        }

        public override void paint(PaintingContext context, Offset offset) {
            if (child != null) {
                _updateClip();
                layer = context.pushClipRRect(
                    needsCompositing,
                    offset,
                    _clip.outerRect,
                    _clip,
                    base.paint,
                    clipBehavior: clipBehavior,
                    oldLayer: layer as ClipRRectLayer
                );
            }
            else {
                layer = null;
            }
        }

        protected override void debugPaintSize(PaintingContext context, Offset offset) {
            D.assert(() => {
                if (child != null) {
                    base.debugPaintSize(context, offset);
                    context.canvas.drawRRect(_clip.shift(offset), _debugPaint);
                    _debugText.paint(context.canvas,
                        offset + new Offset(_clip.tlRadiusX,
                            -(_debugText.text.style.fontSize ?? 0.0f) * 1.1f));
                }

                return true;
            });
        }
    }

    public class RenderClipOval : _RenderCustomClip<Rect> {
        public RenderClipOval(
            RenderBox child = null,
            CustomClipper<Rect> clipper = null,
            Clip clipBehavior = Clip.antiAlias
        ) : base(child: child, clipper: clipper, clipBehavior: clipBehavior) {
            D.assert(clipBehavior != Clip.none);
        }

        Rect _cachedRect;
        Path _cachedPath;

        Path _getClipPath(Rect rect) {
            if (rect != _cachedRect) {
                _cachedRect = rect;
                _cachedPath = new Path();
                _cachedPath.addOval(_cachedRect);
            }

            return _cachedPath;
        }

        protected override Rect _defaultClip {
            get { return Offset.zero & size; }
        }

        public override bool hitTest(BoxHitTestResult result, Offset position = null) {
            _updateClip();
            D.assert(_clip != null);
            Offset center = _clip.center;
            Offset offset = new Offset((position.dx - center.dx) / _clip.width,
                (position.dy - center.dy) / _clip.height);
            if (offset.distanceSquared > 0.25f) {
                return false;
            }

            return base.hitTest(result, position: position);
        }

        public override void paint(PaintingContext context, Offset offset) {
            if (child != null) {
                _updateClip();
                layer = context.pushClipPath(
                    needsCompositing,
                    offset,
                    _clip,
                    _getClipPath(_clip),
                    base.paint,
                    clipBehavior: clipBehavior,
                    oldLayer: layer as ClipPathLayer
                );
            }
            else {
                layer = null;
            }
        }

        protected override void debugPaintSize(PaintingContext context, Offset offset) {
            D.assert(() => {
                if (child != null) {
                    base.debugPaintSize(context, offset);
                    context.canvas.drawPath(_getClipPath(_clip).shift(offset), _debugPaint);
                    _debugText.paint(context.canvas,
                        offset + new Offset((_clip.width - _debugText.width) / 2.0f,
                            -_debugText.text.style.fontSize * 1.1f ?? 0.0f));
                }

                return true;
            });
        }
    }

    public class RenderClipPath : _RenderCustomClip<Path> {
        public RenderClipPath(
            RenderBox child = null,
            CustomClipper<Path> clipper = null,
            Clip clipBehavior = Clip.antiAlias
        ) : base(child: child, clipper: clipper, clipBehavior: clipBehavior) {
            D.assert(clipBehavior != Clip.none);
        }

        protected override Path _defaultClip {
            get {
                var path = new Path();
                path.addRect(Offset.zero & size);
                return path;
            }
        }

        public override bool hitTest(BoxHitTestResult result, Offset position = null) {
            D.assert(position != null);

            if (_clipper != null) {
                _updateClip();
                D.assert(_clip != null);
                if (!_clip.contains(position)) {
                    return false;
                }
            }

            return base.hitTest(result, position: position);
        }


        public override void paint(PaintingContext context, Offset offset) {
            if (child != null) {
                _updateClip();
                layer = context.pushClipPath(
                    needsCompositing,
                    offset,
                    Offset.zero & size,
                    _clip,
                    base.paint,
                    clipBehavior: clipBehavior,
                    oldLayer: layer as ClipPathLayer
                );
            }
            else {
                layer = null;
            }
        }

        protected override void debugPaintSize(PaintingContext context, Offset offset) {
            D.assert(() => {
                if (child != null) {
                    base.debugPaintSize(context, offset);
                    context.canvas.drawPath(_clip.shift(offset), _debugPaint);
                    _debugText.paint(context.canvas, offset);
                }

                return true;
            });
        }
    }

    public abstract class _RenderPhysicalModelBase<T> : _RenderCustomClip<T> where T : class {
        public _RenderPhysicalModelBase(
            RenderBox child = null,
            float? elevation = null,
            Color color = null,
            Color shadowColor = null,
            Clip clipBehavior = Clip.none,
            CustomClipper<T> clipper = null
        ) : base(child: child, clipBehavior: clipBehavior, clipper: clipper) {
            D.assert(elevation != null && elevation >= 0.0f);
            D.assert(color != null);
            D.assert(shadowColor != null);
            _elevation = elevation ?? 0.0f;
            _color = color;
            _shadowColor = shadowColor;
        }

        public float elevation {
            get { return _elevation; }
            set {
                D.assert(value >= 0.0f);
                if (elevation == value) {
                    return;
                }

                bool didNeedCompositing = alwaysNeedsCompositing;
                _elevation = value;
                if (didNeedCompositing != alwaysNeedsCompositing) {
                    markNeedsCompositingBitsUpdate();
                }

                markNeedsPaint();
            }
        }

        float _elevation;

        public Color shadowColor {
            get { return _shadowColor; }
            set {
                D.assert(value != null);
                if (shadowColor == value) {
                    return;
                }

                _shadowColor = value;
                markNeedsPaint();
            }
        }

        Color _shadowColor;

        public Color color {
            get { return _color; }
            set {
                D.assert(value != null);
                if (color == value) {
                    return;
                }

                _color = value;
                markNeedsPaint();
            }
        }

        Color _color;

        protected override bool alwaysNeedsCompositing {
            get { return true; }
        }

        public override void debugFillProperties(DiagnosticPropertiesBuilder description) {
            base.debugFillProperties(description);
            description.add(new FloatProperty("elevation", elevation));
            description.add(new DiagnosticsProperty<Color>("color", color));
            description.add(new DiagnosticsProperty<Color>("shadowColor", shadowColor));
        }
    }

    public class RenderPhysicalModel : _RenderPhysicalModelBase<RRect> {
        public RenderPhysicalModel(
            RenderBox child = null,
            BoxShape shape = BoxShape.rectangle,
            Clip clipBehavior = Clip.none,
            BorderRadius borderRadius = null,
            float? elevation = 0.0f,
            Color color = null,
            Color shadowColor = null
        ) : base(clipBehavior: clipBehavior, child: child, elevation: elevation, color: color,
            shadowColor: shadowColor ?? new Color(0xFF000000)) {
            D.assert(color != null);
            D.assert(elevation != null && elevation >= 0.0f);
            _shape = shape;
            _borderRadius = borderRadius;
        }

        public new PhysicalModelLayer layer {
            get { return base.layer as PhysicalModelLayer; }
            set {
                if (layer == value) {
                    return;
                }

                base.layer = value;
            }
        }

        public BoxShape shape {
            get { return _shape; }
            set {
                if (shape == value) {
                    return;
                }

                _shape = value;
                _markNeedsClip();
            }
        }

        BoxShape _shape;

        public BorderRadius borderRadius {
            get { return _borderRadius; }
            set {
                if (borderRadius == value) {
                    return;
                }

                _borderRadius = value;
                _markNeedsClip();
            }
        }

        BorderRadius _borderRadius;

        protected override RRect _defaultClip {
            get {
                D.assert(hasSize);
                switch (_shape) {
                    case BoxShape.rectangle:
                        return (borderRadius ?? BorderRadius.zero).toRRect(Offset.zero & size);
                    case BoxShape.circle:
                        Rect rect = Offset.zero & size;
                        return RRect.fromRectXY(rect, rect.width / 2, rect.height / 2);
                }

                return null;
            }
        }

        public override bool hitTest(BoxHitTestResult result, Offset position = null) {
            if (_clipper != null) {
                _updateClip();
                D.assert(_clip != null);
                if (!_clip.contains(position)) {
                    return false;
                }
            }

            return base.hitTest(result, position: position);
        }


        public override void paint(PaintingContext context, Offset offset) {
            if (child != null) {
                _updateClip();
                RRect offsetRRect = _clip.shift(offset);
                Rect offsetBounds = offsetRRect.outerRect;
                Path offsetRRectAsPath = new Path();
                offsetRRectAsPath.addRRect(offsetRRect);
                bool paintShadows = true;
                D.assert(() => {
                    if (RenderingDebugUtils.debugDisableShadows) {
                        if (elevation > 0.0) {
                            Paint paint = new Paint();
                            paint.color = shadowColor;
                            paint.style = PaintingStyle.stroke;
                            paint.strokeWidth = elevation * 2.0f;
                            context.canvas.drawRRect(
                                offsetRRect,
                                paint
                            );
                        }

                        paintShadows = false;
                    }

                    return true;
                });
                layer = layer ?? new PhysicalModelLayer();
                layer.clipPath = offsetRRectAsPath;
                layer.clipBehavior = clipBehavior;
                layer.elevation = paintShadows ? elevation : 0.0f;
                layer.color = color;
                layer.shadowColor = shadowColor;
                context.pushLayer(layer, base.paint, offset, childPaintBounds: offsetBounds);
                D.assert(() => {
                    layer.debugCreator = debugCreator;
                    return true;
                });
            }
            else {
                layer = null;
            }
        }

        public override void debugFillProperties(DiagnosticPropertiesBuilder description) {
            base.debugFillProperties(description);
            description.add(new DiagnosticsProperty<BoxShape>("shape", shape));
            description.add(new DiagnosticsProperty<BorderRadius>("borderRadius", borderRadius));
        }
    }


    public class RenderPhysicalShape : _RenderPhysicalModelBase<Path> {
        public RenderPhysicalShape(
            RenderBox child = null,
            CustomClipper<Path> clipper = null,
            Clip clipBehavior = Clip.none,
            float elevation = 0.0f,
            Color color = null,
            Color shadowColor = null
        ) : base(child: child,
            elevation: elevation,
            color: color,
            shadowColor: shadowColor ?? new Color(0xFF000000),
            clipper: clipper,
            clipBehavior: clipBehavior) {
            D.assert(clipper != null);
            D.assert(color != null);
            D.assert(elevation >= 0.0);
        }

        public new PhysicalModelLayer layer {
            get { return base.layer as PhysicalModelLayer; }
            set {
                if (layer == value) {
                    return;
                }

                base.layer = value;
            }
        }

        protected override Path _defaultClip {
            get {
                Path path = new Path();
                path.addRect(Offset.zero & size);
                return path;
            }
        }

        public override bool hitTest(BoxHitTestResult result, Offset position = null) {
            if (_clipper != null) {
                _updateClip();
                D.assert(_clip != null);
                if (!_clip.contains(position)) {
                    return false;
                }
            }

            return base.hitTest(result, position: position);
        }


        public override void paint(PaintingContext context, Offset offset) {
            if (child != null) {
                _updateClip();
                Rect offsetBounds = offset & size;
                Path offsetPath = _clip.shift(offset);
                bool paintShadows = true;
                D.assert(() => {
                    if (RenderingDebugUtils.debugDisableShadows) {
                        if (elevation > 0.0) {
                            Paint paint = new Paint();
                            paint.color = shadowColor;
                            paint.style = PaintingStyle.stroke;
                            paint.strokeWidth = elevation * 2.0f;
                            context.canvas.drawPath(
                                offsetPath,
                                paint
                            );
                        }

                        paintShadows = false;
                    }

                    return true;
                });
                layer = layer ?? new PhysicalModelLayer();
                layer.clipPath = offsetPath;
                layer.clipBehavior = clipBehavior;
                layer.elevation = paintShadows ? elevation : 0.0f;
                layer.color = color;
                layer.shadowColor = shadowColor;
                context.pushLayer(layer, base.paint, offset, childPaintBounds: offsetBounds);
                D.assert(() => {
                    layer.debugCreator = debugCreator;
                    return true;
                });
            }
            else {
                layer = null;
            }
        }


        public override void debugFillProperties(DiagnosticPropertiesBuilder description) {
            base.debugFillProperties(description);
            description.add(new DiagnosticsProperty<CustomClipper<Path>>("clipper", clipper));
        }
    }

    public enum DecorationPosition {
        background,
        foreground,
    }

    public class RenderDecoratedBox : RenderProxyBox {
        public RenderDecoratedBox(
            Decoration decoration = null,
            DecorationPosition position = DecorationPosition.background,
            ImageConfiguration configuration = null,
            RenderBox child = null
        ) : base(child) {
            D.assert(decoration != null);
            _decoration = decoration;
            _position = position;
            _configuration = configuration ?? ImageConfiguration.empty;
        }

        BoxPainter _painter;

        public Decoration decoration {
            get { return _decoration; }
            set {
                D.assert(value != null);
                if (value == _decoration) {
                    return;
                }

                if (_painter != null) {
                    _painter.Dispose();
                    _painter = null;
                }

                _decoration = value;
                markNeedsPaint();
            }
        }

        Decoration _decoration;

        public DecorationPosition position {
            get { return _position; }
            set {
                if (value == _position) {
                    return;
                }

                _position = value;
                markNeedsPaint();
            }
        }

        DecorationPosition _position;

        public ImageConfiguration configuration {
            get { return _configuration; }
            set {
                D.assert(value != null);
                if (value == _configuration) {
                    return;
                }

                _configuration = value;
                markNeedsPaint();
            }
        }

        ImageConfiguration _configuration;

        public override void detach() {
            if (_painter != null) {
                _painter.Dispose();
                _painter = null;
            }

            base.detach();
            markNeedsPaint();
        }

        protected override bool hitTestSelf(Offset position) {
            return _decoration.hitTest(size, position, textDirection: configuration.textDirection);
        }

        public override void paint(PaintingContext context, Offset offset) {
            _painter = _painter ?? _decoration.createBoxPainter(markNeedsPaint);
            var filledConfiguration = configuration.copyWith(size: size);

            if (position == DecorationPosition.background) {
                int debugSaveCount = 0;
                D.assert(() => {
                    debugSaveCount = context.canvas.getSaveCount();
                    return true;
                });

                _painter.paint(context.canvas, offset, filledConfiguration);

                D.assert(() => {
                    if (debugSaveCount != context.canvas.getSaveCount()) {
                        throw new UIWidgetsError(new List<DiagnosticsNode> {
                            new ErrorSummary(
                                $"{_decoration.GetType()} painter had mismatching save and restore calls."),
                            new ErrorDescription(
                                "Before painting the decoration, the canvas save count was $debugSaveCount. " +
                                "After painting it, the canvas save count was ${context.canvas.getSaveCount()}. " +
                                "Every call to save() or saveLayer() must be matched by a call to restore()."
                            ),
                            new DiagnosticsProperty<Decoration>("The decoration was", decoration,
                                style: DiagnosticsTreeStyle.errorProperty),
                            new DiagnosticsProperty<BoxPainter>("The painter was", _painter,
                                style: DiagnosticsTreeStyle.errorProperty),
                        });
                    }

                    return true;
                });

                if (decoration.isComplex) {
                    context.setIsComplexHint();
                }
            }

            base.paint(context, offset);

            if (position == DecorationPosition.foreground) {
                _painter.paint(context.canvas, offset, filledConfiguration);
                if (decoration.isComplex) {
                    context.setIsComplexHint();
                }
            }
        }

        public override void debugFillProperties(DiagnosticPropertiesBuilder properties) {
            base.debugFillProperties(properties);
            properties.add(_decoration.toDiagnosticsNode(name: "decoration"));
            properties.add(new DiagnosticsProperty<ImageConfiguration>("configuration", configuration));
        }
    }

    public class RenderTransform : RenderProxyBox {
        public RenderTransform(
            Matrix4 transform,
            Offset origin = null,
            AlignmentGeometry alignment = null,
            TextDirection? textDirection = null,
            bool transformHitTests = true,
            RenderBox child = null
        ) : base(child) {
            D.assert(transform != null);
            this.transform = transform;
            this.origin = origin;
            this.alignment = alignment;
            this.textDirection = textDirection;
            this.transformHitTests = transformHitTests;
        }

        public Offset origin {
            get { return _origin; }
            set {
                if (_origin == value) {
                    return;
                }

                _origin = value;
                markNeedsPaint();
            }
        }

        Offset _origin;

        public AlignmentGeometry alignment {
            get { return _alignment; }
            set {
                if (_alignment == value) {
                    return;
                }

                _alignment = value;
                markNeedsPaint();
            }
        }

        AlignmentGeometry _alignment;


        public TextDirection? textDirection {
            get { return _textDirection; }
            set {
                if (_textDirection == value) {
                    return;
                }

                _textDirection = value;
                markNeedsPaint();
                //markNeedsSemanticsUpdate();
            }
        }

        TextDirection? _textDirection;


        public bool transformHitTests;

        public Matrix4 transform {
            set {
                if (_transform == value) {
                    return;
                }

                _transform = value;
                //_transform = Matrix4.copy(value);
                markNeedsPaint();
            }
        }

        Matrix4 _transform;

        public void setIdentity() {
            _transform.setIdentity();
            _transform = Matrix4.identity();
            markNeedsPaint();
        }

        public void rotateX(float degrees) {
            _transform.rotateX(degrees);
            markNeedsPaint();
        }

        public void rotateY(float degrees) {
            _transform.rotateY(degrees);
            markNeedsPaint();
        }

        public void rotateZ(float degrees) {
            _transform.rotateZ(degrees);
            markNeedsPaint();
        }

        public void translate(float x, float y = 0.0f, float z = 0.0f) {
            _transform.translate(x, y, z);
            markNeedsPaint();
        }

        public void scale(float x, float y, float z) {
            _transform.scale(x, y, 1);
            markNeedsPaint();
        }

        Matrix4 _effectiveTransform {
            get {
                Alignment resolvedAlignment = alignment?.resolve(textDirection);
                if (_origin == null && resolvedAlignment == null) {
                    return _transform;
                }

                var result = Matrix4.identity();
                if (_origin != null) {
                    result.translate(_origin.dx, _origin.dy);
                }

                Offset translation = null;
                if (resolvedAlignment != null) {
                    translation = resolvedAlignment.alongSize(size);
                    result.translate(translation.dx, translation.dy);
                }

                result.multiply(_transform);

                if (resolvedAlignment != null) {
                    result.translate(-translation.dx, -translation.dy);
                }

                if (_origin != null) {
                    result.translate(-_origin.dx, -_origin.dy);
                }

                return result;
            }
        }

        public override bool hitTest(BoxHitTestResult result, Offset position = null) {
            return hitTestChildren(result, position: position);
        }

        protected override bool hitTestChildren(BoxHitTestResult result, Offset position = null) {
            D.assert(!transformHitTests || _effectiveTransform != null);
            return result.addWithPaintTransform(
                transform: transformHitTests ? _effectiveTransform : null,
                position: position,
                hitTest: (BoxHitTestResult resultIn, Offset positionIn) => {
                    return base.hitTestChildren(resultIn, position: positionIn);
                }
            );
        }

        public override void paint(PaintingContext context, Offset offset) {
            if (child != null) {
                var transform = _effectiveTransform;
                Offset childOffset = transform.getAsTranslation();

                if (childOffset == null) {
                    layer = context.pushTransform(
                        needsCompositing,
                        offset,
                        transform,
                        base.paint,
                        oldLayer: layer as TransformLayer
                    );
                }
                else {
                    base.paint(context, offset + childOffset);
                    layer = null;
                }
            }
        }

        public override void applyPaintTransform(RenderObject child, Matrix4 transform) {
            transform.multiply(_effectiveTransform);
        }

        public override void debugFillProperties(DiagnosticPropertiesBuilder properties) {
            base.debugFillProperties(properties);
            properties.add(new DiagnosticsProperty<Matrix4>("transform matrix", _transform));
            properties.add(new DiagnosticsProperty<Offset>("origin", origin));
            properties.add(new DiagnosticsProperty<AlignmentGeometry>("alignment", alignment));
            properties.add(new DiagnosticsProperty<bool>("transformHitTests", transformHitTests));
            properties.add(new EnumProperty<TextDirection>("textDirection", (TextDirection) textDirection,
                defaultValue: null));
        }
    }

    public class RenderFittedBox : RenderProxyBox {
        public RenderFittedBox(
            BoxFit fit = BoxFit.contain,
            AlignmentGeometry alignment = null,
            TextDirection? textDirection = null,
            RenderBox child = null
        ) : base(child) {
            _fit = fit;
            _alignment = alignment ?? Alignment.center;
            _textDirection = textDirection;
        }

        Alignment _resolvedAlignment;

        void _resolve() {
            if (_resolvedAlignment != null) {
                return;
            }

            _resolvedAlignment = alignment.resolve(textDirection);
        }

        void _markNeedResolution() {
            _resolvedAlignment = null;
            markNeedsPaint();
        }

        public BoxFit fit {
            get { return _fit; }
            set {
                if (_fit == value) {
                    return;
                }

                _fit = value;
                _clearPaintData();
                markNeedsPaint();
            }
        }

        BoxFit _fit;

        public AlignmentGeometry alignment {
            get { return _alignment; }
            set {
                D.assert(value != null);
                if (_alignment == value) {
                    return;
                }

                _alignment = value;
                _clearPaintData();
                _markNeedResolution();
            }
        }

        AlignmentGeometry _alignment;

        public TextDirection? textDirection {
            get { return _textDirection; }
            set {
                if (_textDirection == value) {
                    return;
                }

                _textDirection = value;
                _clearPaintData();
                _markNeedResolution();
            }
        }

        TextDirection? _textDirection;


        protected override void performLayout() {
            if (child != null) {
                child.layout(new BoxConstraints(), parentUsesSize: true);
                size = constraints.constrainSizeAndAttemptToPreserveAspectRatio(child.size);
                _clearPaintData();
            }
            else {
                size = constraints.smallest;
            }
        }

        bool? _hasVisualOverflow;
        Matrix4 _transform;

        void _clearPaintData() {
            _hasVisualOverflow = null;
            _transform = null;
        }

        void _updatePaintData() {
            if (_transform != null) {
                return;
            }

            if (child == null) {
                _hasVisualOverflow = false;
                _transform = Matrix4.identity();
            }
            else {
                _resolve();
                Size childSize = child.size;
                FittedSizes sizes = FittedSizes.applyBoxFit(_fit, childSize, size);
                float scaleX = sizes.destination.width / sizes.source.width;
                float scaleY = sizes.destination.height / sizes.source.height;
                Rect sourceRect = _resolvedAlignment.inscribe(sizes.source, Offset.zero & childSize);
                Rect destinationRect = _resolvedAlignment.inscribe(sizes.destination, Offset.zero & size);
                _hasVisualOverflow = sourceRect.width < childSize.width || sourceRect.height < childSize.height;
                D.assert(scaleX.isFinite() && scaleY.isFinite());
                _transform = Matrix4.translationValues(destinationRect.left, destinationRect.top, 0);
                _transform.scale(scaleX, scaleY, 1);
                _transform.translate(-sourceRect.left, -sourceRect.top);
                bool result = true;
                foreach (var value in _transform.storage) {
                    if (!value.isFinite()) {
                        result = false;
                    }
                }

                D.assert(result);
            }
        }

        TransformLayer _paintChildWithTransform(PaintingContext context, Offset offset) {
            Offset childOffset = _transform.getAsTranslation();
            if (childOffset == null) {
                return context.pushTransform(needsCompositing, offset, _transform, base.paint,
                    oldLayer: layer is TransformLayer ? layer as TransformLayer : null);
            }
            else {
                base.paint(context, offset + childOffset);
                return null;
            }
        }

        public override void paint(PaintingContext context, Offset offset) {
            if (size.isEmpty || child.size.isEmpty) {
                return;
            }

            _updatePaintData();
            if (child != null) {
                if (_hasVisualOverflow == true) {
                    layer = context.pushClipRect(needsCompositing, offset, Offset.zero & size,
                        painter: (PaintingContext subContext, Offset subOffset) => {
                            _paintChildWithTransform(subContext, subOffset);
                        },
                        oldLayer: layer is ClipRectLayer ? layer as ClipRectLayer : null);
                }
                else {
                    _paintChildWithTransform(context, offset);
                }
            }
        }

        protected override bool hitTestChildren(BoxHitTestResult result, Offset position = null) {
            if (size.isEmpty || child?.size?.isEmpty == true) {
                return false;
            }

            _updatePaintData();
            return result.addWithPaintTransform(
                transform: _transform,
                position: position,
                hitTest: (BoxHitTestResult resultIn, Offset positionIn) => {
                    return base.hitTestChildren(resultIn, position: positionIn);
                }
            );
        }

        public override void applyPaintTransform(RenderObject child, Matrix4 transform) {
            if (size.isEmpty || ((RenderBox) child).size.isEmpty) {
                transform.setZero();
            }
            else {
                _updatePaintData();
                transform.multiply(_transform);
            }
        }

        public override void debugFillProperties(DiagnosticPropertiesBuilder properties) {
            base.debugFillProperties(properties);
            properties.add(new EnumProperty<BoxFit>("fit", fit));
            properties.add(new DiagnosticsProperty<AlignmentGeometry>("alignment", alignment));
            properties.add(new EnumProperty<TextDirection>("textDirection", (TextDirection) textDirection,
                defaultValue: null));
        }
    }

    public class RenderFractionalTranslation : RenderProxyBox {
        public RenderFractionalTranslation(
            Offset translation = null,
            bool transformHitTests = true,
            RenderBox child = null
        ) : base(child: child) {
            D.assert(translation != null);
            _translation = translation;
            this.transformHitTests = transformHitTests;
        }

        public Offset translation {
            get { return _translation; }

            set {
                D.assert(value != null);
                if (_translation == value) {
                    return;
                }

                _translation = value;
                markNeedsPaint();
            }
        }

        Offset _translation;

        public override bool hitTest(BoxHitTestResult result, Offset position = null) {
            return hitTestChildren(result, position: position);
        }

        public bool transformHitTests;

        protected override bool hitTestChildren(BoxHitTestResult result, Offset position = null) {
            D.assert(!debugNeedsLayout);
            return result.addWithPaintOffset(
                offset: transformHitTests
                    ? new Offset(translation.dx * size.width, translation.dy * size.height)
                    : null,
                position: position,
                hitTest: (BoxHitTestResult resultIn, Offset positionIn) => {
                    return base.hitTestChildren(resultIn, position: positionIn);
                }
            );
        }

        public override void paint(PaintingContext context, Offset offset) {
            D.assert(!debugNeedsLayout);
            if (child != null) {
                base.paint(context, new Offset(
                    offset.dx + translation.dx * size.width,
                    offset.dy + translation.dy * size.height
                ));
            }
        }

        public override void applyPaintTransform(RenderObject child, Matrix4 transform) {
            transform.translate(translation.dx * size.width,
                translation.dy * size.height);
        }

        public override void debugFillProperties(DiagnosticPropertiesBuilder properties) {
            base.debugFillProperties(properties);
            properties.add(new DiagnosticsProperty<Offset>("translation", translation));
            properties.add(new DiagnosticsProperty<bool>("transformHitTests", transformHitTests));
        }
    }

    public delegate void PointerDownEventListener(PointerDownEvent evt);

    public delegate void PointerMoveEventListener(PointerMoveEvent evt);

    public delegate void PointerUpEventListener(PointerUpEvent evt);

    public delegate void PointerCancelEventListener(PointerCancelEvent evt);

    public delegate void PointerSignalEventListener(PointerSignalEvent evt);

    public delegate void PointerScrollEventListener(PointerScrollEvent evt);


    
    class RenderPointerListener : RenderProxyBoxWithHitTestBehavior {
        public RenderPointerListener(
            PointerDownEventListener onPointerDown = null,
            PointerMoveEventListener onPointerMove = null,
            PointerUpEventListener onPointerUp = null,
            PointerCancelEventListener onPointerCancel = null,
            PointerSignalEventListener onPointerSignal = null,
            HitTestBehavior behavior = HitTestBehavior.deferToChild,
            RenderBox child = null
        ) : base(behavior: behavior, child: child) {
            this.onPointerDown = onPointerDown;
            this.onPointerMove = onPointerMove;
            this.onPointerUp = onPointerUp;
            this.onPointerCancel = onPointerCancel;
            this.onPointerSignal = onPointerSignal;
        }

        public PointerDownEventListener onPointerDown;
        public PointerMoveEventListener onPointerMove;
        public PointerUpEventListener onPointerUp;
        public PointerCancelEventListener onPointerCancel;
        public PointerSignalEventListener onPointerSignal;

        protected override void performResize() {
            size = constraints.biggest;
        }

        public override void handleEvent(PointerEvent Event, HitTestEntry entry) {
            D.assert(debugHandleEvent(Event, entry));
            if (onPointerDown != null && Event is PointerDownEvent) {
                onPointerDown((PointerDownEvent) Event);
                return;
            }

            if (onPointerMove != null && Event is PointerMoveEvent) {
                onPointerMove((PointerMoveEvent) Event);
                return;
            }

            if (onPointerUp != null && Event is PointerUpEvent) {
                onPointerUp((PointerUpEvent) Event);
                return;
            }

            if (onPointerCancel != null && Event is PointerCancelEvent) {
                onPointerCancel((PointerCancelEvent) Event);
                return;
            }

            if (onPointerSignal != null && Event is PointerSignalEvent) {
                onPointerSignal((PointerSignalEvent) Event);
            }
        }

        public override void debugFillProperties(DiagnosticPropertiesBuilder properties) {
            base.debugFillProperties(properties);
            //new FlagsSummary<Delegate>() 
            properties.add(new FlagsSummary<Delegate>(
                "listeners",
                new Dictionary<string, Delegate> {
                    {"down", onPointerDown},
                    {"move", onPointerMove},
                    {"up", onPointerUp},
                    {"cancel", onPointerCancel},
                    {"signal", onPointerSignal}
                },
                ifEmpty: "<none>"
            ));
        }
    }

    public class RenderMouseRegion : RenderProxyBox {
        public RenderMouseRegion(
            PointerEnterEventListener onEnter = null,
            PointerHoverEventListener onHover = null,
            PointerExitEventListener onExit = null,
            bool opaque = true,
            RenderBox child = null
        ) : base(child) {
            _onEnter = onEnter;
            _onHover = onHover;
            _onExit = onExit;
            _opaque = opaque;
            _annotationIsActive = false;
            _hoverAnnotation = new MouseTrackerAnnotation(
                onEnter: _handleEnter,
                onHover: _handleHover,
                onExit: _handleExit
            );
        }

        public bool opaque {
            get { return _opaque; }
            set {
                if (_opaque != value) {
                    _opaque = value;
                    _markPropertyUpdated(mustRepaint: true);
                }
            }
        }

        bool _opaque;

        public PointerEnterEventListener onEnter {
            get { return _onEnter; }
            set {
                if (_onEnter != value) {
                    _onEnter = value;
                    _markPropertyUpdated(mustRepaint: false);
                }
            }
        }

        PointerEnterEventListener _onEnter;

        public void _handleEnter(PointerEnterEvent _event) {
            if (_onEnter != null) {
                _onEnter(_event);
            }
        }

        public PointerHoverEventListener onHover {
            get { return _onHover; }
            set {
                if (_onHover != value) {
                    _onHover = value;
                    _markPropertyUpdated(mustRepaint: false);
                }
            }
        }

        PointerHoverEventListener _onHover;

        void _handleHover(PointerHoverEvent _event) {
            if (_onHover != null) {
                _onHover(_event);
            }
        }

        public PointerExitEventListener onExit {
            get { return _onExit; }
            set {
                if (_onExit != value) {
                    _onExit = value;
                    _markPropertyUpdated(mustRepaint: false);
                }
            }
        }

        PointerExitEventListener _onExit;

        void _handleExit(PointerExitEvent _event) {
            if (_onExit != null) {
                _onExit(_event);
            }
        }

        MouseTrackerAnnotation _hoverAnnotation;

        MouseTrackerAnnotation hoverAnnotation {
            get { return _hoverAnnotation; }
        }

        public void _markPropertyUpdated(bool mustRepaint) {
            D.assert(owner == null || !owner.debugDoingPaint);
            bool newAnnotationIsActive = (
                                             _onEnter != null ||
                                             _onHover != null ||
                                             _onExit != null ||
                                             opaque) && RendererBinding.instance.mouseTracker.mouseIsConnected;
            _setAnnotationIsActive(newAnnotationIsActive);
            if (mustRepaint) {
                markNeedsPaint();
            }
        }

        void _setAnnotationIsActive(bool value) {
            bool annotationWasActive = _annotationIsActive;
            _annotationIsActive = value;
            if (annotationWasActive != value) {
                markNeedsPaint();
                markNeedsCompositingBitsUpdate();
            }
        }

        void _handleUpdatedMouseIsConnected() {
            _markPropertyUpdated(mustRepaint: false);
        }

        public override void attach(object owner) {
            owner = (PipelineOwner) owner;
            base.attach(owner);
            // todo
            RendererBinding.instance.mouseTracker.addListener(_handleUpdatedMouseIsConnected);
            _markPropertyUpdated(mustRepaint: false);
        }

        public override void detach() {
            RendererBinding.instance.mouseTracker.removeListener(_handleUpdatedMouseIsConnected);
            base.detach();
        }

        public bool _annotationIsActive;

        public override bool needsCompositing {
            get { return base.needsCompositing || _annotationIsActive; }
        }
        
        public override void paint(PaintingContext context, Offset offset) {
            if (_annotationIsActive) {
                var layer = new AnnotatedRegionLayer<MouseTrackerAnnotation>(
                    _hoverAnnotation,
                    size: size,
                    offset: offset,
                    opaque: opaque
                );
                context.pushLayer(layer, base.paint, offset);
            }
            else {
                base.paint(context, offset);
            }
        }

        protected override void performResize() {
            size = constraints.biggest;
        }

        public override void debugFillProperties(DiagnosticPropertiesBuilder properties) {
            base.debugFillProperties(properties);
            // todo
            properties.add(new FlagsSummary<Delegate>(
                "listeners",
                new Dictionary<string, Delegate> {
                    {"enter", onEnter},
                    {"hover", onHover},
                    {"exit", onExit},
                },
                ifEmpty: "<none>"
            ));
            properties.add(new DiagnosticsProperty<bool>("opaque", opaque, defaultValue: true));
        }
    }


    public class RenderRepaintBoundary : RenderProxyBox {
        public RenderRepaintBoundary(
            RenderBox child = null
        ) : base(child) {
        }

        public override bool isRepaintBoundary {
            get { return true; }
        }

        Future<Image> toImage(float pixelRatio = 1.0f) {
            D.assert(!debugNeedsPaint);
            OffsetLayer offsetLayer = layer as OffsetLayer;
            return offsetLayer.toImage(Offset.zero & size, pixelRatio: pixelRatio);
        }

        public int debugSymmetricPaintCount {
            get { return _debugSymmetricPaintCount; }
        }

        int _debugSymmetricPaintCount = 0;

        public int debugAsymmetricPaintCount {
            get { return _debugAsymmetricPaintCount; }
        }

        int _debugAsymmetricPaintCount = 0;

        public void debugResetMetrics() {
            D.assert(() => {
                _debugSymmetricPaintCount = 0;
                _debugAsymmetricPaintCount = 0;
                return true;
            });
        }

        public override void debugRegisterRepaintBoundaryPaint(bool includedParent = true, bool includedChild = false) {
            D.assert(() => {
                if (includedParent && includedChild) {
                    _debugSymmetricPaintCount += 1;
                }
                else {
                    _debugAsymmetricPaintCount += 1;
                }

                return true;
            });
        }

        public override void debugFillProperties(DiagnosticPropertiesBuilder properties) {
            base.debugFillProperties(properties);
            bool inReleaseMode = true;
            D.assert(() => {
                inReleaseMode = false;
                if (debugSymmetricPaintCount + debugAsymmetricPaintCount == 0) {
                    properties.add(new MessageProperty("usefulness ratio", "no metrics collected yet (never painted)"));
                }
                else {
                    float fraction = (float) debugAsymmetricPaintCount /
                                     (debugSymmetricPaintCount + debugAsymmetricPaintCount);

                    string diagnosis;
                    if (debugSymmetricPaintCount + debugAsymmetricPaintCount < 5) {
                        diagnosis = "insufficient data to draw conclusion (less than five repaints)";
                    }
                    else if (fraction > 0.9) {
                        diagnosis = "this is an outstandingly useful repaint boundary and should definitely be kept";
                    }
                    else if (fraction > 0.5) {
                        diagnosis = "this is a useful repaint boundary and should be kept";
                    }
                    else if (fraction > 0.30) {
                        diagnosis =
                            "this repaint boundary is probably useful, but maybe it would be more useful in tandem with adding more repaint boundaries elsewhere";
                    }
                    else if (fraction > 0.1) {
                        diagnosis = "this repaint boundary does sometimes show value, though currently not that often";
                    }
                    else if (debugAsymmetricPaintCount == 0) {
                        diagnosis = "this repaint boundary is astoundingly ineffectual and should be removed";
                    }
                    else {
                        diagnosis = "this repaint boundary is not very effective and should probably be removed";
                    }

                    properties.add(new PercentProperty("metrics", fraction, unit: "useful",
                        tooltip: debugSymmetricPaintCount + " bad vs " + debugAsymmetricPaintCount +
                                 " good"));
                    properties.add(new MessageProperty("diagnosis", diagnosis));
                }

                return true;
            });
            if (inReleaseMode) {
                properties.add(DiagnosticsNode.message("(run in checked mode to collect repaint boundary statistics)"));
            }
        }
    }

    public class RenderIgnorePointer : RenderProxyBox {
        public RenderIgnorePointer(
            RenderBox child = null,
            bool ignoring = true
        ) : base(child) {
            _ignoring = ignoring;
        }

        public bool ignoring {
            get { return _ignoring; }
            set {
                if (value == _ignoring) {
                    return;
                }

                _ignoring = value;
            }
        }

        bool _ignoring;

        public override bool hitTest(BoxHitTestResult result, Offset position = null) {
            return !ignoring && base.hitTest(result, position: position);
        }

        public override void debugFillProperties(DiagnosticPropertiesBuilder properties) {
            base.debugFillProperties(properties);
            properties.add(new DiagnosticsProperty<bool>("ignoring", ignoring));
        }
    }

    public class RenderOffstage : RenderProxyBox {
        public RenderOffstage(bool offstage = true, RenderBox child = null) : base(child) {
            _offstage = offstage;
        }

        public bool offstage {
            get { return _offstage; }

            set {
                if (value == _offstage) {
                    return;
                }

                _offstage = value;
                markNeedsLayoutForSizedByParentChange();
            }
        }

        bool _offstage;

        protected internal override float computeMinIntrinsicWidth(float height) {
            if (offstage) {
                return 0.0f;
            }

            return base.computeMinIntrinsicWidth(height);
        }


        protected internal override float computeMaxIntrinsicWidth(float height) {
            if (offstage) {
                return 0.0f;
            }

            return base.computeMaxIntrinsicWidth(height);
        }

        protected internal override float computeMinIntrinsicHeight(float width) {
            if (offstage) {
                return 0.0f;
            }

            return base.computeMinIntrinsicHeight(width);
        }

        protected internal override float computeMaxIntrinsicHeight(float width) {
            if (offstage) {
                return 0.0f;
            }

            return base.computeMaxIntrinsicHeight(width);
        }

        public override float? computeDistanceToActualBaseline(TextBaseline baseline) {
            if (offstage) {
                return null;
            }

            return base.computeDistanceToActualBaseline(baseline);
        }

        protected override bool sizedByParent {
            get { return offstage; }
        }

        protected override void performResize() {
            D.assert(offstage);
            size = constraints.smallest;
        }

        protected override void performLayout() {
            if (offstage) {
                child?.layout(constraints);
            }
            else {
                base.performLayout();
            }
        }

        public override bool hitTest(BoxHitTestResult result, Offset position = null) {
            return !offstage && base.hitTest(result, position: position);
        }

        public override void paint(PaintingContext context, Offset offset) {
            if (offstage) {
                return;
            }

            base.paint(context, offset);
        }


        public override void debugFillProperties(DiagnosticPropertiesBuilder properties) {
            base.debugFillProperties(properties);
            properties.add(new DiagnosticsProperty<bool>("offstage", offstage));
        }


        public override List<DiagnosticsNode> debugDescribeChildren() {
            if (child == null) {
                return new List<DiagnosticsNode>();
            }

            return new List<DiagnosticsNode> {
                child.toDiagnosticsNode(
                    name: "child",
                    style: offstage ? DiagnosticsTreeStyle.offstage : DiagnosticsTreeStyle.sparse
                ),
            };
        }
    }

    public class RenderAbsorbPointer : RenderProxyBox {
        public RenderAbsorbPointer(
            RenderBox child = null,
            bool absorbing = true
        ) : base(child) {
            _absorbing = absorbing;
        }


        public bool absorbing {
            get { return _absorbing; }
            set {
                if (_absorbing == value) {
                    return;
                }

                _absorbing = value;
            }
        }

        bool _absorbing;

        public override bool hitTest(BoxHitTestResult result, Offset position = null) {
            return absorbing
                ? size.contains(position)
                : base.hitTest(result, position: position);
        }


        public override void debugFillProperties(DiagnosticPropertiesBuilder properties) {
            base.debugFillProperties(properties);
            properties.add(new DiagnosticsProperty<bool>("absorbing", absorbing));
        }
    }

    public class RenderMetaData : RenderProxyBoxWithHitTestBehavior {
        public RenderMetaData(
            object metaData,
            HitTestBehavior behavior = HitTestBehavior.deferToChild,
            RenderBox child = null
        ) : base(behavior, child) {
            this.metaData = metaData;
        }

        public object metaData;

        public override void debugFillProperties(DiagnosticPropertiesBuilder properties) {
            base.debugFillProperties(properties);
            properties.add(new DiagnosticsProperty<object>("metaData", metaData));
        }
    }

    class RenderIndexedSemantics : RenderProxyBox {
        public RenderIndexedSemantics(
            RenderBox child = null,
            int index = 0
        ) : base(child: child) {
            _index = index;
        }

        int _index;

        public int index {
            get { return _index; }
            set {
                if (value == index) {
                    return;
                }

                _index = value;
            }
        }

        public override void debugFillProperties(DiagnosticPropertiesBuilder properties) {
            base.debugFillProperties(properties);
            properties.add(new DiagnosticsProperty<int>("index", index));
        }
    }

    public class RenderLeaderLayer : RenderProxyBox {
        public RenderLeaderLayer(
            LayerLink link = null,
            RenderBox child = null
        ) : base(child: child) {
            D.assert(link != null);
            this.link = link;
        }

        public LayerLink link {
            get { return _link; }
            set {
                D.assert(value != null);
                if (_link == value) {
                    return;
                }

                _link = value;
                markNeedsPaint();
            }
        }

        LayerLink _link;

        protected override bool alwaysNeedsCompositing {
            get { return true; }
        }

        public override void paint(PaintingContext context, Offset offset) {
            if (layer == null) {
                layer = new LeaderLayer(link: link, offset: offset);
            }
            else {
                LeaderLayer leaderLayer = (LeaderLayer) layer;
                leaderLayer.link = link;
                leaderLayer.offset = offset;
            }

            context.pushLayer(layer, base.paint, Offset.zero);
            D.assert(layer != null);
        }

        public override void debugFillProperties(DiagnosticPropertiesBuilder properties) {
            base.debugFillProperties(properties);
            properties.add(new DiagnosticsProperty<LayerLink>("link", link));
        }
    }


    class RenderFollowerLayer : RenderProxyBox {
        public RenderFollowerLayer(LayerLink link,
            bool showWhenUnlinked = true,
            Offset offset = null,
            RenderBox child = null
        ) : base(child) {
            D.assert(link != null);
            this.link = link;
            this.showWhenUnlinked = showWhenUnlinked;
            this.offset = offset ?? Offset.zero;
        }

        public LayerLink link {
            get { return _link; }
            set {
                D.assert(value != null);
                if (_link == value) {
                    return;
                }

                _link = value;
                markNeedsPaint();
            }
        }

        LayerLink _link;

        public bool showWhenUnlinked {
            get { return _showWhenUnlinked; }
            set {
                if (_showWhenUnlinked == value) {
                    return;
                }

                _showWhenUnlinked = value;
                markNeedsPaint();
            }
        }

        bool _showWhenUnlinked;

        public Offset offset {
            get { return _offset; }
            set {
                D.assert(value != null);
                if (_offset == value) {
                    return;
                }

                _offset = value;
                markNeedsPaint();
            }
        }

        Offset _offset;

        public override void detach() {
            layer = null;
            base.detach();
        }

        protected override bool alwaysNeedsCompositing {
            get { return true; }
        }

        public new FollowerLayer layer {
            get { return base.layer as FollowerLayer; }
            set {
                if (layer == value) {
                    return;
                }

                base.layer = value;
            }
        }

        Matrix4 getCurrentTransform() {
            return layer?.getLastTransform() ?? Matrix4.identity();
        }

        public override bool hitTest(BoxHitTestResult result, Offset position = null) {
            return hitTestChildren(result, position: position);
        }

        protected override bool hitTestChildren(BoxHitTestResult result, Offset position = null) {
            return result.addWithPaintTransform(
                transform: getCurrentTransform(),
                position: position,
                hitTest: (BoxHitTestResult resultIn, Offset positionIn) => {
                    return base.hitTestChildren(resultIn, position: positionIn);
                }
            );
        }

        public override void paint(PaintingContext context, Offset offset) {
            if (layer == null) {
                layer = new FollowerLayer(
                    link: link,
                    showWhenUnlinked: showWhenUnlinked,
                    linkedOffset: this.offset,
                    unlinkedOffset: offset
                );
            }
            else {
                layer.link = link;
                layer.showWhenUnlinked = showWhenUnlinked;
                layer.linkedOffset = this.offset;
                layer.unlinkedOffset = offset;
            }

            context.pushLayer(
                layer,
                base.paint,
                Offset.zero,
                childPaintBounds: Rect.fromLTRB(
                    float.NegativeInfinity,
                    float.NegativeInfinity,
                    float.PositiveInfinity,
                    float.PositiveInfinity
                )
            );
        }

        public override void applyPaintTransform(RenderObject child, Matrix4 transform) {
            transform.multiply(getCurrentTransform());
        }

        public override void debugFillProperties(DiagnosticPropertiesBuilder properties) {
            base.debugFillProperties(properties);
            properties.add(new DiagnosticsProperty<LayerLink>("link", link));
            properties.add(new DiagnosticsProperty<bool>("showWhenUnlinked", showWhenUnlinked));
            properties.add(new DiagnosticsProperty<Offset>("offset", offset));
            properties.add(new TransformProperty("current transform matrix", getCurrentTransform()));
        }
    }

    public class RenderAnnotatedRegion<T> : RenderProxyBox
        where T : class {
        public RenderAnnotatedRegion(
            T value = null,
            bool? sized = null,
            RenderBox child = null
        ) : base(child: child) {
            D.assert(value != null);
            D.assert(sized != null);
            _value = value;
            _sized = sized.Value;
        }

        public T value {
            get { return _value; }
            set {
                if (_value == value) {
                    return;
                }

                _value = value;
                markNeedsPaint();
            }
        }

        T _value;

        public bool sized {
            get { return _sized; }
            set {
                if (_sized == value) {
                    return;
                }

                _sized = value;
                markNeedsPaint();
            }
        }

        bool _sized;

        protected override bool alwaysNeedsCompositing {
            get { return true; }
        }

        public override void paint(PaintingContext context, Offset offset) {
            AnnotatedRegionLayer<T> layer =
                new AnnotatedRegionLayer<T>(
                    value: value,
                    size: sized ? size : null,
                    offset: sized ? offset : null);

            context.pushLayer(layer, base.paint, offset);
        }
    }
}