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
    static class InkRippleUtils {
        public static readonly TimeSpan _kUnconfirmedRippleDuration = new TimeSpan(0, 0, 1);
        public static readonly TimeSpan _kFadeInDuration = new TimeSpan(0, 0, 0, 0, 75);
        public static readonly TimeSpan _kRadiusDuration = new TimeSpan(0, 0, 0, 0, 225);
        public static readonly TimeSpan _kFadeOutDuration = new TimeSpan(0, 0, 0, 0, 375);
        public static readonly TimeSpan _kCancelDuration = new TimeSpan(0, 0, 0, 0, 75);

        public const float _kFadeOutIntervalStart = 225.0f / 375.0f;

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
            Size size = rectCallback != null ? rectCallback().size : referenceBox.size;
            float d1 = size.bottomRight(Offset.zero).distance;
            float d2 = (size.topRight(Offset.zero) - size.bottomLeft(Offset.zero)).distance;
            return (Mathf.Max(d1, d2) / 2.0f);
        }
    }

    public class _InkRippleFactory : InteractiveInkFeatureFactory {
        public _InkRippleFactory() {
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
            return new InkRipple(
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

    public class InkRipple : InteractiveInkFeature {
        public InkRipple(
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
            D.assert(color != null);
            D.assert(position != null);

            _position = position;
            _borderRadius = borderRadius ?? BorderRadius.zero;
            _customBorder = customBorder;
            _targetRadius =
                radius ?? InkRippleUtils._getTargetRadius(referenceBox, containedInkWell, rectCallback, position);
            _clipCallback = InkRippleUtils._getClipCallback(referenceBox, containedInkWell, rectCallback);

            D.assert(_borderRadius != null);

            _fadeInController =
                new AnimationController(duration: InkRippleUtils._kFadeInDuration, vsync: controller.vsync);
            _fadeInController.addListener(controller.markNeedsPaint);
            _fadeInController.forward();
            _fadeIn = _fadeInController.drive(new IntTween(
                begin: 0,
                end: color.alpha
            ));

            _radiusController = new AnimationController(
                duration: InkRippleUtils._kUnconfirmedRippleDuration,
                vsync: controller.vsync);
            _radiusController.addListener(controller.markNeedsPaint);
            _radiusController.forward();
            _radius = _radiusController.drive(new FloatTween(
                    begin: _targetRadius * 0.30f,
                    end: _targetRadius + 5.0f
                ).chain(_easeCurveTween)
            );

            _fadeOutController = new AnimationController(
                duration: InkRippleUtils._kFadeOutDuration,
                vsync: controller.vsync);
            _fadeOutController.addListener(controller.markNeedsPaint);
            _fadeOutController.addStatusListener(_handleAlphaStatusChanged);
            _fadeOut = _fadeOutController.drive(new IntTween(
                    begin: color.alpha,
                    end: 0
                ).chain(_fadeOutIntervalTween)
            );

            controller.addInkFeature(this);
        }

        readonly Offset _position;

        readonly BorderRadius _borderRadius;

        readonly ShapeBorder _customBorder;

        readonly float _targetRadius;

        readonly RectCallback _clipCallback;

        Animation<float> _radius;
        AnimationController _radiusController;

        Animation<int> _fadeIn;
        AnimationController _fadeInController;

        Animation<int> _fadeOut;
        AnimationController _fadeOutController;

        public static InteractiveInkFeatureFactory splashFactory = new _InkRippleFactory();

        static readonly Animatable<float> _easeCurveTween = new CurveTween(curve: Curves.ease);

        static readonly Animatable<float> _fadeOutIntervalTween =
            new CurveTween(curve: new Interval(InkRippleUtils._kFadeOutIntervalStart, 1.0f));

        public override void confirm() {
            _radiusController.duration = InkRippleUtils._kRadiusDuration;
            _radiusController.forward();
            _fadeInController.forward();
            _fadeOutController.animateTo(1.0f, duration: InkRippleUtils._kFadeOutDuration);
        }

        public override void cancel() {
            _fadeInController.stop();
            float fadeOutValue = 1.0f - _fadeInController.value;
            _fadeOutController.setValue(fadeOutValue);
            if (fadeOutValue < 1.0) {
                _fadeOutController.animateTo(1.0f, duration: InkRippleUtils._kCancelDuration);
            }
        }

        void _handleAlphaStatusChanged(AnimationStatus status) {
            if (status == AnimationStatus.completed) {
                dispose();
            }
        }

        public override void dispose() {
            _radiusController.dispose();
            _fadeInController.dispose();
            _fadeOutController.dispose();
            base.dispose();
        }

        protected override void paintFeature(Canvas canvas, Matrix4 transform) {
            int alpha = _fadeInController.isAnimating ? _fadeIn.value : _fadeOut.value;
            Paint paint = new Paint {color = this.color.withAlpha(alpha)};
            Offset center = Offset.lerp(
                _position,
                referenceBox.size.center(Offset.zero),
                Curves.ease.transform(_radiusController.value)
            );
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