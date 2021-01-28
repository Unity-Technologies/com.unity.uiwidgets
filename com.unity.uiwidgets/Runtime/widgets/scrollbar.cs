using Unity.UIWidgets.animation;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.ui;
using UnityEngine;
using Canvas = Unity.UIWidgets.ui.Canvas;
using Color = Unity.UIWidgets.ui.Color;
using Rect = Unity.UIWidgets.ui.Rect;

namespace Unity.UIWidgets.widgets {

    static class ScrollbarPainterUtils {
        
        public const float _kMinThumbExtent = 18.0f;
        public const float _kMinInteractiveSize = 48.0f;
    }
    public class ScrollbarPainter : ChangeNotifier, CustomPainter {
        public ScrollbarPainter(
            Color color,
            TextDirection textDirection,
            float thickness,
            Animation<float> fadeoutOpacityAnimation,
            EdgeInsets padding = null,
            float mainAxisMargin = 0.0f,
            float crossAxisMargin = 0.0f,
            Radius radius = null,
            float minLength = ScrollbarPainterUtils._kMinThumbExtent,
            float? minOverscrollLength = null
        ) {
            D.assert(color != null);
            D.assert(fadeoutOpacityAnimation != null);
            D.assert(minLength >= 0);
            D.assert(minOverscrollLength == null || minOverscrollLength.Value <= minLength);
            D.assert(minOverscrollLength == null || minOverscrollLength.Value >= 0);
            padding = padding ?? EdgeInsets.zero;
            
            D.assert(padding.isNonNegative);
            _color = color;
            _textDirection = textDirection;
            _padding = padding;
            this.thickness = thickness;
            this.fadeoutOpacityAnimation = fadeoutOpacityAnimation;
            this.mainAxisMargin = mainAxisMargin;
            this.crossAxisMargin = crossAxisMargin;
            this.radius = radius;
            this.minLength = minLength;
            this.minOverscrollLength = minOverscrollLength ?? minLength;
            fadeoutOpacityAnimation.addListener(notifyListeners);
        }

        public Color color {
            get { return _color; }
            set {
                D.assert(value != null);
                if (color == value) {
                    return;
                }

                _color = value;
                notifyListeners();
            }
        }
        Color _color;

        public TextDirection textDirection {
            get { return _textDirection; }
            set {
                if (textDirection == value) {
                    return;
                }

                _textDirection = value;
                notifyListeners();
            }
        }

        TextDirection _textDirection;
        
        public float thickness;
        public readonly Animation<float> fadeoutOpacityAnimation;
        public readonly float mainAxisMargin;
        public readonly float crossAxisMargin;
        public Radius radius;

        public EdgeInsets padding {
            get { return _padding; }
            set {
                D.assert(value != null);
                if (padding == value) {
                    return;
                }

                _padding = value;
                notifyListeners();
            }
        }
        EdgeInsets _padding;
        
        public readonly float minLength;
        public readonly float minOverscrollLength;

        ScrollMetrics _lastMetrics;
        AxisDirection? _lastAxisDirection;
        Rect _thumbRect;

        public void update(ScrollMetrics metrics, AxisDirection axisDirection) {
            _lastMetrics = metrics;
            _lastAxisDirection = axisDirection;
            notifyListeners();
        }

        public void updateThickness(float nextThickness, Radius nextRadius) {
            thickness = nextThickness;
            radius = nextRadius;
            notifyListeners();
        }

        Paint _paint {
            get {
                var paint = new Paint();
                paint.color = color.withOpacity(color.opacity * fadeoutOpacityAnimation.value);
                return paint;
            }
        }
        
        void _paintThumbCrossAxis(Canvas canvas, Size size, float thumbOffset, float thumbExtent, AxisDirection direction) {
            float x = 0;
            float y = 0;
            Size thumbSize = Size.zero;

            switch (direction) {
                case AxisDirection.down:
                    thumbSize = new Size(thickness, thumbExtent);
                    x = textDirection == TextDirection.rtl
                        ? crossAxisMargin + padding.left
                        : size.width - thickness - crossAxisMargin - padding.right;
                    y = thumbOffset;
                    break;
                case AxisDirection.up:
                    thumbSize = new Size(thickness, thumbExtent);
                    x = textDirection == TextDirection.rtl
                        ? crossAxisMargin + padding.left
                        : size.width - thickness - crossAxisMargin - padding.right;
                    y = thumbOffset;
                    break;
                case AxisDirection.left:
                    thumbSize = new Size(thumbExtent, thickness);
                    x = thumbOffset;
                    y = size.height - thickness - crossAxisMargin - padding.bottom;
                    break;
                case AxisDirection.right:
                    thumbSize = new Size(thumbExtent, thickness);
                    x = thumbOffset;
                    y = size.height - thickness - crossAxisMargin - padding.bottom;
                    break;
            }

            _thumbRect = new Offset(x, y) & thumbSize;
            if (radius == null) {
                canvas.drawRect(_thumbRect, _paint);
            }
            else {
                canvas.drawRRect(RRect.fromRectAndRadius(_thumbRect, radius), _paint);
            }
        }
        
        float _thumbExtent() {
            float fractionVisible = ((_lastMetrics.extentInside() - _mainAxisPadding) / (_totalContentExtent - _mainAxisPadding))
                .clamp(0.0f, 1.0f);

            float thumbExtent = Mathf.Max(
                Mathf.Min(_trackExtent, minOverscrollLength),
                _trackExtent * fractionVisible
            );

            float fractionOverscrolled = 1.0f - _lastMetrics.extentInside() / _lastMetrics.viewportDimension;
            float safeMinLength = Mathf.Min(minLength, _trackExtent);
            float newMinLength = (_beforeExtent > 0 && _afterExtent > 0)
                ? safeMinLength
                : safeMinLength * (1.0f - fractionOverscrolled.clamp(0.0f, 0.2f) / 0.2f);
            
            return thumbExtent.clamp(newMinLength, _trackExtent);
        }

        public override void dispose() {
            fadeoutOpacityAnimation.removeListener(notifyListeners);
            base.dispose();
        }
        
        bool _isVertical => _lastAxisDirection == AxisDirection.down || _lastAxisDirection == AxisDirection.up;
        bool _isReversed => _lastAxisDirection == AxisDirection.up || _lastAxisDirection == AxisDirection.left;
        float _beforeExtent => _isReversed ? _lastMetrics.extentAfter() : _lastMetrics.extentBefore();
        float _afterExtent => _isReversed ? _lastMetrics.extentBefore() : _lastMetrics.extentAfter();
        float _mainAxisPadding => _isVertical ? padding.vertical : padding.horizontal;
        float _trackExtent => _lastMetrics.viewportDimension - 2 * mainAxisMargin - _mainAxisPadding;
        
        float _totalContentExtent {
            get {
                return _lastMetrics.maxScrollExtent
                       - _lastMetrics.minScrollExtent
                       + _lastMetrics.viewportDimension;
            }
        }

        public float getTrackToScroll(float thumbOffsetLocal) {
            float scrollableExtent = _lastMetrics.maxScrollExtent - _lastMetrics.minScrollExtent;
            float thumbMovableExtent = _trackExtent - _thumbExtent();

            return scrollableExtent * thumbOffsetLocal / thumbMovableExtent;
        }
        
        float _getScrollToTrack(ScrollMetrics metrics, float thumbExtent) {
            float scrollableExtent = metrics.maxScrollExtent - metrics.minScrollExtent;

            float fractionPast = (scrollableExtent > 0)
                ? ((metrics.pixels - metrics.minScrollExtent) / scrollableExtent).clamp(0.0f, 1.0f)
                : 0;

            return (_isReversed ? 1 - fractionPast : fractionPast) * (_trackExtent - thumbExtent);
        }

        public void paint(Canvas canvas, Size size) {
            if (_lastAxisDirection == null
                || _lastMetrics == null
                || fadeoutOpacityAnimation.value == 0.0f) {
                return;
            }

            if (_lastMetrics.viewportDimension <= _mainAxisPadding || _trackExtent <= 0) {
                return;
            }
            
            float beforePadding = _isVertical ? padding.top : padding.left;
            float thumbExtent = _thumbExtent();
            float thumbOffsetLocal = _getScrollToTrack(_lastMetrics, thumbExtent);
            float thumbOffset = thumbOffsetLocal + mainAxisMargin + beforePadding;

            _paintThumbCrossAxis(canvas, size, thumbOffset, thumbExtent, _lastAxisDirection.Value);
        }

        public bool hitTestInteractive(Offset position) {
            if (_thumbRect == null) {
                return false;
            }

            if (fadeoutOpacityAnimation.value == 0.0f) {
                return false;
            }
            Rect interactiveThumbRect = _thumbRect.expandToInclude(
                Rect.fromCircle(center: _thumbRect.center, radius: ScrollbarPainterUtils._kMinInteractiveSize / 2)
            );
            return interactiveThumbRect.contains(position);
        }

        public bool? hitTest(Offset position) {
            if (_thumbRect == null) {
                return null;
            }

            if (fadeoutOpacityAnimation.value == 0.0f) {
                return false;
            }

            return _thumbRect.contains(position);
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
                       || minLength != old.minLength
                       || padding != old.padding;
            }

            return false;
        }
    }
}