using System;
using Unity.UIWidgets.animation;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.rendering;
using Unity.UIWidgets.ui;
using UnityEngine;
using Canvas = Unity.UIWidgets.ui.Canvas;
using Color = Unity.UIWidgets.ui.Color;
using Rect = Unity.UIWidgets.ui.Rect;

namespace Unity.UIWidgets.material {
    static class InkSplashUtils {
        public static readonly TimeSpan _kUnconfirmedSplashDuration = new TimeSpan(0, 0, 0, 1, 0);

        public static readonly TimeSpan _kSplashFadeDuration = new TimeSpan(0, 0, 0, 0, 200);

        public const float _kSplashInitialSize = 0.0f;

        public const float _kSplashConfirmedVelocity = 1.0f;

        public static RectCallback _getClipCallback(RenderBox referenceBox, bool containedInkWell,
            RectCallback rectCallback) {
            if (rectCallback != null) {
                D.assert(containedInkWell);
                return rectCallback;
            }

            if (containedInkWell) {
                return () => Offset.zero & referenceBox.size;
            }

            return null;
        }

        public static float _getTargetRadius(RenderBox referenceBox, bool containedInkWell, RectCallback rectCallback,
            Offset position) {
            if (containedInkWell) {
                Size size = rectCallback != null ? rectCallback().size : referenceBox.size;
                return _getSplashRadiusForPositionInSize(size, position);
            }

            return Material.defaultSplashRadius;
        }

        static float _getSplashRadiusForPositionInSize(Size bounds, Offset position) {
            float d1 = (position - bounds.topLeft(Offset.zero)).distance;
            float d2 = (position - bounds.topRight(Offset.zero)).distance;
            float d3 = (position - bounds.bottomLeft(Offset.zero)).distance;
            float d4 = (position - bounds.bottomRight(Offset.zero)).distance;
            return Mathf.Max(Mathf.Max(d1, d2), Mathf.Max(d3, d4)).ceil();
        }
    }

    public class _InkSplashFactory : InteractiveInkFeatureFactory {
        public _InkSplashFactory() {
        }

        public override InteractiveInkFeature create(
            MaterialInkController controller = null,
            RenderBox referenceBox = null,
            Offset position = null,
            Color color = null,
            bool containedInkWell = false,
            RectCallback rectCallback = null,
            BorderRadius borderRadius = null,
            ShapeBorder customBorder = null,
            float? radius = null,
            VoidCallback onRemoved = null
        ) {
            D.assert(controller != null);
            D.assert(referenceBox != null);
            D.assert(position != null);
            D.assert(color != null);
            return new InkSplash(
                controller: controller,
                referenceBox: referenceBox,
                position: position,
                color: color,
                containedInkWell: containedInkWell,
                rectCallback: rectCallback,
                borderRadius: borderRadius,
                customBorder: customBorder,
                radius: radius,
                onRemoved: onRemoved);
        }
    }

    public class InkSplash : InteractiveInkFeature {
        public InkSplash(
            MaterialInkController controller = null,
            RenderBox referenceBox = null,
            Offset position = null,
            Color color = null,
            bool containedInkWell = false,
            RectCallback rectCallback = null,
            BorderRadius borderRadius = null,
            ShapeBorder customBorder = null,
            float? radius = null,
            VoidCallback onRemoved = null
        ) : base(
            controller: controller,
            referenceBox: referenceBox,
            color: color,
            onRemoved: onRemoved) {
            D.assert(controller != null);
            D.assert(referenceBox != null);
            _position = position;
            _borderRadius = borderRadius ?? BorderRadius.zero;
            _customBorder = customBorder;
            _targetRadius =
                radius ?? InkSplashUtils._getTargetRadius(referenceBox, containedInkWell, rectCallback, position);
            _clipCallback = InkSplashUtils._getClipCallback(referenceBox, containedInkWell, rectCallback);
            _repositionToReferenceBox = !containedInkWell;

            D.assert(_borderRadius != null);
            _radiusController = new AnimationController(
                duration: InkSplashUtils._kUnconfirmedSplashDuration,
                vsync: controller.vsync);
            _radiusController.addListener(controller.markNeedsPaint);
            _radiusController.forward();
            _radius = _radiusController.drive(new FloatTween(
                begin: InkSplashUtils._kSplashInitialSize,
                end: _targetRadius));

            _alphaController = new AnimationController(
                duration: InkSplashUtils._kSplashFadeDuration,
                vsync: controller.vsync);
            _alphaController.addListener(controller.markNeedsPaint);
            _alphaController.addStatusListener(_handleAlphaStatusChanged);
            _alpha = _alphaController.drive(new IntTween(
                begin: color.alpha,
                end: 0));

            controller.addInkFeature(this);
        }

        readonly Offset _position;

        readonly BorderRadius _borderRadius;

        readonly ShapeBorder _customBorder;

        readonly float _targetRadius;

        readonly RectCallback _clipCallback;

        readonly bool _repositionToReferenceBox;

        Animation<float> _radius;
        AnimationController _radiusController;

        Animation<int> _alpha;
        AnimationController _alphaController;

        public static InteractiveInkFeatureFactory splashFactory = new _InkSplashFactory();

        public override void confirm() {
            int duration = (_targetRadius / InkSplashUtils._kSplashConfirmedVelocity).floor();
            _radiusController.duration = new TimeSpan(0, 0, 0, 0, duration);
            _radiusController.forward();
            _alphaController.forward();
        }

        public override void cancel() {
            _alphaController?.forward();
        }

        void _handleAlphaStatusChanged(AnimationStatus status) {
            if (status == AnimationStatus.completed) {
                dispose();
            }
        }

        public override void dispose() {
            _radiusController.dispose();
            _alphaController.dispose();
            _alphaController = null;
            base.dispose();
        }

        protected override void paintFeature(Canvas canvas, Matrix4 transform) {
            Paint paint = new Paint {color = this.color.withAlpha(_alpha.value)};
            Offset center = _position;
            if (_repositionToReferenceBox) {
                center = Offset.lerp(center, referenceBox.size.center(Offset.zero), _radiusController.value);
            }

            Offset originOffset = transform.getAsTranslation();
            canvas.save();
            if (originOffset == null) {
                canvas.concat(transform.toMatrix3());
            }
            else {
                canvas.translate(originOffset.dx, originOffset.dy);
            }

            if (_clipCallback != null) {
                Rect rect = _clipCallback();
                if (_customBorder != null) {
                    canvas.clipPath(_customBorder.getOuterPath(rect));
                }
                else if (_borderRadius != BorderRadius.zero) {
                    canvas.clipRRect(RRect.fromRectAndCorners(
                        rect,
                        topLeft: _borderRadius.topLeft,
                        topRight: _borderRadius.topRight,
                        bottomLeft: _borderRadius.bottomLeft,
                        bottomRight: _borderRadius.bottomRight));
                }
                else {
                    canvas.clipRect(rect);
                }
            }

            canvas.drawCircle(center, _radius.value, paint);
            canvas.restore();
        }
    }
}