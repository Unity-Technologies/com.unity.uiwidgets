using System;
using Unity.UIWidgets.animation;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.ui;
using Unity.UIWidgets.widgets;
using UnityEngine;
using Canvas = Unity.UIWidgets.ui.Canvas;
using Color = Unity.UIWidgets.ui.Color;

namespace Unity.UIWidgets.cupertino {
    static class CupertinoActivityIndicatorUtils {
        public const float _kDefaultIndicatorRadius = 10.0f;
        public const float _kTwoPI = Mathf.PI * 2.0f;
        public const int _kTickCount = 12;
        public const int _kHalfTickCount = _kTickCount / 2;
        public static readonly Color _kTickColor = CupertinoColors.lightBackgroundGray;
        public static readonly Color _kActiveTickColor = new Color(0xFF9D9D9D);
    }

    public class CupertinoActivityIndicator : StatefulWidget {
        public CupertinoActivityIndicator(
            Key key = null,
            bool animating = true,
            float radius = CupertinoActivityIndicatorUtils._kDefaultIndicatorRadius
        ) : base(key: key) {
            D.assert(radius > 0);
            this.animating = animating;
            this.radius = radius;
        }

        public readonly bool animating;
        public readonly float radius;

        public override State createState() {
            return new _CupertinoActivityIndicatorState();
        }
    }

    class _CupertinoActivityIndicatorState : TickerProviderStateMixin<CupertinoActivityIndicator> {
        AnimationController _controller;

        public override void initState() {
            base.initState();
            _controller = new AnimationController(
                duration: TimeSpan.FromSeconds(1),
                vsync: this
            );

            if (widget.animating) {
                _controller.repeat();
            }
        }

        public override void didUpdateWidget(StatefulWidget oldWidget) {
            base.didUpdateWidget(oldWidget: oldWidget);
            if (oldWidget is CupertinoActivityIndicator _oldWidget) {
                if (widget.animating != _oldWidget.animating) {
                    if (widget.animating) {
                        _controller.repeat();
                    }
                    else {
                        _controller.stop();
                    }
                }
            }
        }

        public override void dispose() {
            _controller.dispose();
            base.dispose();
        }

        public override Widget build(BuildContext context) {
            return new SizedBox(
                height: widget.radius * 2,
                width: widget.radius * 2,
                child: new CustomPaint(
                    painter: new _CupertinoActivityIndicatorPainter(
                        position: _controller,
                        radius: widget.radius
                    )
                )
            );
        }
    }

    class _CupertinoActivityIndicatorPainter : AbstractCustomPainter {
        public _CupertinoActivityIndicatorPainter(
            Animation<float> position,
            float radius
        ) : base(repaint: position) {
            tickFundamentalRRect = RRect.fromLTRBXY(
                left: -radius,
                top: 1.0f * radius / CupertinoActivityIndicatorUtils._kDefaultIndicatorRadius,
                right: -radius / 2.0f,
                bottom: -1.0f * radius / CupertinoActivityIndicatorUtils._kDefaultIndicatorRadius,
                radiusX: 1.0f,
                radiusY: 1.0f
            );
            this.position = position;
        }

        readonly Animation<float> position;
        readonly RRect tickFundamentalRRect;

        public override void paint(Canvas canvas, Size size) {
            Paint paint = new Paint();

            canvas.save();
            canvas.translate(size.width / 2.0f, size.height / 2.0f);

            int activeTick = (CupertinoActivityIndicatorUtils._kTickCount * position.value).floor();

            for (int i = 0; i < CupertinoActivityIndicatorUtils._kTickCount; ++i) {
                float t = (((i + activeTick) % CupertinoActivityIndicatorUtils._kTickCount) /
                           CupertinoActivityIndicatorUtils._kHalfTickCount).clamp(0, 1);
                paint.color = Color.lerp(a: CupertinoActivityIndicatorUtils._kActiveTickColor,
                    b: CupertinoActivityIndicatorUtils._kTickColor, t: t);
                canvas.drawRRect(rect: tickFundamentalRRect, paint: paint);
                canvas.rotate(-CupertinoActivityIndicatorUtils._kTwoPI / CupertinoActivityIndicatorUtils._kTickCount);
            }

            canvas.restore();
        }

        public override bool shouldRepaint(CustomPainter oldPainter) {
            return (oldPainter as _CupertinoActivityIndicatorPainter).position != position;
        }
    }
}