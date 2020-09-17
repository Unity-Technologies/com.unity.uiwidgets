using Unity.UIWidgets.animation;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.ui;
using UnityEngine;
using Canvas = Unity.UIWidgets.ui.Canvas;
using Color = Unity.UIWidgets.ui.Color;
using Rect = Unity.UIWidgets.ui.Rect;

namespace Unity.UIWidgets.widgets {
    public class ScrollbarPainter : ChangeNotifier, CustomPainter {
        public ScrollbarPainter(
            Color color,
            TextDirection textDirection,
            float thickness,
            Animation<float> fadeoutOpacityAnimation,
            float mainAxisMargin = 0.0f,
            float crossAxisMargin = 0.0f,
            Radius radius = null,
            float minLength = _kMinThumbExtent,
            float minOverscrollLength = _kMinThumbExtent
        ) {
            this.color = color;
            this.textDirection = textDirection;
            this.thickness = thickness;
            this.fadeoutOpacityAnimation = fadeoutOpacityAnimation;
            this.mainAxisMargin = mainAxisMargin;
            this.crossAxisMargin = crossAxisMargin;
            this.radius = radius;
            this.minLength = minLength;
            this.minOverscrollLength = minOverscrollLength;
            fadeoutOpacityAnimation.addListener(notifyListeners);
        }

        const float _kMinThumbExtent = 18.0f;

        public Color color;
        public TextDirection? textDirection;
        public float thickness;
        public Animation<float> fadeoutOpacityAnimation;
        public float mainAxisMargin;
        public float crossAxisMargin;
        public Radius radius;
        public float minLength;
        public float minOverscrollLength;

        ScrollMetrics _lastMetrics;
        AxisDirection? _lastAxisDirection;

        public void update(ScrollMetrics metrics, AxisDirection axisDirection) {
            _lastMetrics = metrics;
            _lastAxisDirection = axisDirection;
            notifyListeners();
        }

        Paint _paint {
            get {
                var paint = new Paint();
                paint.color = color.withOpacity(color.opacity * fadeoutOpacityAnimation.value);
                return paint;
            }
        }

        float _getThumbX(Size size) {
            D.assert(textDirection != null);
            switch (textDirection) {
                case TextDirection.rtl:
                    return crossAxisMargin;
                case TextDirection.ltr:
                    return size.width - thickness - crossAxisMargin;
            }

            return 0;
        }

        void _paintVerticalThumb(Canvas canvas, Size size, float thumbOffset, float thumbExtent) {
            Offset thumbOrigin = new Offset(_getThumbX(size), thumbOffset);
            Size thumbSize = new Size(thickness, thumbExtent);
            Rect thumbRect = thumbOrigin & thumbSize;
            if (radius == null) {
                canvas.drawRect(thumbRect, _paint);
            }
            else {
                canvas.drawRRect(RRect.fromRectAndRadius(thumbRect, radius), _paint);
            }
        }

        void _paintHorizontalThumb(Canvas canvas, Size size, float thumbOffset, float thumbExtent) {
            Offset thumbOrigin = new Offset(thumbOffset, size.height - thickness);
            Size thumbSize = new Size(thumbExtent, thickness);
            Rect thumbRect = thumbOrigin & thumbSize;
            if (radius == null) {
                canvas.drawRect(thumbRect, _paint);
            }
            else {
                canvas.drawRRect(RRect.fromRectAndRadius(thumbRect, radius), _paint);
            }
        }

        public delegate void painterDelegate(Canvas canvas, Size size, float thumbOffset, float thumbExtent);

        void _paintThumb(
            float before,
            float inside,
            float after,
            float viewport,
            Canvas canvas,
            Size size,
            painterDelegate painter
        ) {
            float thumbExtent = Mathf.Min(viewport, minOverscrollLength);

            if (before + inside + after > 0.0) {
                float fractionVisible = inside / (before + inside + after);
                thumbExtent = Mathf.Max(
                    thumbExtent,
                    viewport * fractionVisible - 2 * mainAxisMargin
                );

                if (before != 0.0 && after != 0.0) {
                    thumbExtent = Mathf.Max(
                        minLength,
                        thumbExtent
                    );
                }
                else {
                    thumbExtent = Mathf.Max(
                        thumbExtent,
                        minLength * (((inside / viewport) - 0.8f) / 0.2f)
                    );
                }

                float fractionPast = before / (before + after);
                float thumbOffset = (before + after > 0.0)
                    ? fractionPast * (viewport - thumbExtent - 2 * mainAxisMargin) + mainAxisMargin
                    : mainAxisMargin;

                painter(canvas, size, thumbOffset, thumbExtent);
            }
        }

        public override void dispose() {
            fadeoutOpacityAnimation.removeListener(notifyListeners);
            base.dispose();
        }


        public void paint(Canvas canvas, Size size) {
            if (_lastAxisDirection == null
                || _lastMetrics == null
                || fadeoutOpacityAnimation.value == 0.0) {
                return;
            }

            switch (_lastAxisDirection) {
                case AxisDirection.down:
                    _paintThumb(_lastMetrics.extentBefore(), _lastMetrics.extentInside(),
                        _lastMetrics.extentAfter(), size.height, canvas, size, _paintVerticalThumb);
                    break;
                case AxisDirection.up:
                    _paintThumb(_lastMetrics.extentAfter(), _lastMetrics.extentInside(),
                        _lastMetrics.extentBefore(), size.height, canvas, size, _paintVerticalThumb);
                    break;
                case AxisDirection.right:
                    _paintThumb(_lastMetrics.extentBefore(), _lastMetrics.extentInside(),
                        _lastMetrics.extentAfter(), size.width, canvas, size, _paintHorizontalThumb);
                    break;
                case AxisDirection.left:
                    _paintThumb(_lastMetrics.extentAfter(), _lastMetrics.extentInside(),
                        _lastMetrics.extentBefore(), size.width, canvas, size, _paintHorizontalThumb);
                    break;
            }
        }

        public bool? hitTest(Offset position) {
            return false;
        }

        public bool shouldRepaint(CustomPainter oldRaw) {
            if (oldRaw is ScrollbarPainter old) {
                return color != old.color
                       || textDirection != old.textDirection
                       || thickness != old.thickness
                       || fadeoutOpacityAnimation != old.fadeoutOpacityAnimation
                       || mainAxisMargin != old.mainAxisMargin
                       || crossAxisMargin != old.crossAxisMargin
                       || radius != old.radius
                       || minLength != old.minLength;
            }

            return false;
        }
    }
}