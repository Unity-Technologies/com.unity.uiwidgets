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
    static class InkHighlightUtils {
        public static readonly TimeSpan _kHighlightFadeDuration = new TimeSpan(0, 0, 0, 0, 200);
    }

    public class InkHighlight : InteractiveInkFeature {
        public InkHighlight(
            MaterialInkController controller = null,
            RenderBox referenceBox = null,
            Color color = null,
            TextDirection textDirection = TextDirection.ltr,
            BoxShape shape = BoxShape.rectangle,
            BorderRadius borderRadius = null,
            ShapeBorder customBorder = null,
            RectCallback rectCallback = null,
            VoidCallback onRemoved = null,
            TimeSpan? fadeDuration = null
            ) : base(
            controller: controller,
            referenceBox: referenceBox,
            color: color,
            onRemoved: onRemoved) {
            if(fadeDuration == null)
                fadeDuration = InkHighlightUtils._kHighlightFadeDuration;
            D.assert(color != null);
            D.assert(controller != null);
            D.assert(referenceBox != null);
            _shape = shape;
            _borderRadius = borderRadius ?? BorderRadius.zero;
            _customBorder = customBorder;
            _textDirection = textDirection;
            _rectCallback = rectCallback;
            _alphaController = new AnimationController(
                duration: fadeDuration,
                vsync: controller.vsync);
            _alphaController.addListener(controller.markNeedsPaint);
            _alphaController.addStatusListener(_handleAlphaStatusChanged);
            _alphaController.forward();

            _alpha = _alphaController.drive(new IntTween(
                begin: 0, 
                end: color.alpha));

            this.controller.addInkFeature(this);
        }

        readonly BoxShape _shape;

        readonly BorderRadius _borderRadius;

        readonly ShapeBorder _customBorder;

        readonly RectCallback _rectCallback;
        public readonly TextDirection _textDirection;

        Animation<int> _alpha;
        AnimationController _alphaController;

        public bool active {
            get { return _active; }
        }
        bool _active = true;

        public void activate() {
            _active = true;
            _alphaController.forward();
        }

        public void deactivate() {
            _active = false;
            _alphaController.reverse();
        }

        void _handleAlphaStatusChanged(AnimationStatus status) {
            if (status == AnimationStatus.dismissed && !_active) {
                dispose();
            }
        }

        public override void dispose() {
            _alphaController.dispose();
            base.dispose();
        }

        void _paintHighlight(Canvas canvas, Rect rect, Paint paint) {
            D.assert(_shape != null);
            canvas.save();
            if (_customBorder != null) {
                canvas.clipPath(_customBorder.getOuterPath(rect));
                //canvas.clipPath(_customBorder.getOuterPath(rect, textDirection: _textDirection));
            }

            switch (_shape) {
                case BoxShape.circle: {
                    canvas.drawCircle(rect.center, Material.defaultSplashRadius, paint);
                    break;
                }
                case BoxShape.rectangle: {
                    if (_borderRadius != BorderRadius.zero) {
                        RRect clipRRect = RRect.fromRectAndCorners(
                            rect,
                            topLeft: _borderRadius.topLeft,
                            topRight: _borderRadius.topRight,
                            bottomLeft: _borderRadius.bottomLeft,
                            bottomRight: _borderRadius.bottomRight);
                        canvas.drawRRect(clipRRect, paint);
                    }
                    else {
                        canvas.drawRect(rect, paint);
                    }

                    break;
                }
            }

            canvas.restore();
        }

        protected override void paintFeature(Canvas canvas, Matrix4 transform) {
            Paint paint = new Paint {color = color.withAlpha(_alpha.value)};
            Offset originOffset = transform.getAsTranslation();
            Rect rect = _rectCallback != null ? _rectCallback() : Offset.zero & referenceBox.size;
            if (originOffset == null) {
                canvas.save();
                canvas.transform(transform.storage);
                _paintHighlight(canvas, rect, paint);
                canvas.restore();
            }
            else {
                _paintHighlight(canvas, rect.shift(originOffset), paint);
            }
        }
    }
}