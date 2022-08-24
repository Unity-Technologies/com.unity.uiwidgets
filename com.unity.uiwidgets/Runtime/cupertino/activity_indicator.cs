using System;
using System.Collections.Generic;
using Unity.UIWidgets.animation;
using Unity.UIWidgets.cupertino;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.ui;
using Unity.UIWidgets.widgets;
using UnityEngine;
using Canvas = Unity.UIWidgets.ui.Canvas;
using Color = Unity.UIWidgets.ui.Color;

namespace Unity.UIWidgets.cupertino {
    static class CupertinoActivityIndicatorUtils {
        public const float _kDefaultIndicatorRadius = 10.0f;

        public static readonly Color _kActiveTickColor = CupertinoDynamicColor.withBrightness(
            color: new Color(0xFF3C3C44),
            darkColor: new Color(0xFFEBEBF5)
        );

        public const float _kTwoPI = Mathf.PI * 2.0f;
        public const int _kTickCount = 12;
        // list
        public static int[] _alphaValues = {    
            147, 131, 114, 97, 81, 64, 47, 47, 47, 47, 47, 47
        };
        public const int _kHalfTickCount = _kTickCount / 2;
        public static readonly Color _kTickColor = CupertinoColors.lightBackgroundGray;
        
    }

    public class CupertinoActivityIndicator : StatefulWidget {
        public CupertinoActivityIndicator(
            Key key = null,
            bool? animating = true,
            float? radius = CupertinoActivityIndicatorUtils._kDefaultIndicatorRadius
        ) : base(key: key) {
            D.assert(animating != null);
            D.assert(radius != null);
            D.assert(radius > 0);
            this.animating = animating.Value;
            this.radius = radius.Value;
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
                        activeColor: CupertinoDynamicColor.resolve(CupertinoActivityIndicatorUtils._kActiveTickColor, context),
                        radius: widget.radius
                    )
                )
            );
        }
    }

    class _CupertinoActivityIndicatorPainter : AbstractCustomPainter
    {
        public _CupertinoActivityIndicatorPainter(
            Animation<float> position = null,
            Color activeColor = null,
            float radius = 0f
        ) : base(repaint: position) {
            
            tickFundamentalRRect = RRect.fromLTRBXY(
                left: -radius,
                top: 1.0f * radius / CupertinoActivityIndicatorUtils._kDefaultIndicatorRadius,
                right: -radius / 2.0f,
                bottom: -1.0f * radius / CupertinoActivityIndicatorUtils._kDefaultIndicatorRadius,
                radiusX: 1.0f * radius / CupertinoActivityIndicatorUtils._kDefaultIndicatorRadius,
                radiusY: 1.0f * radius / CupertinoActivityIndicatorUtils._kDefaultIndicatorRadius
            );
            this.position = position;
            this.activeColor = activeColor;
        }
        
        public readonly Animation<float> position;
        public readonly RRect tickFundamentalRRect;
        public readonly Color activeColor;

        public override void paint(Canvas canvas, Size size) {
            Paint paint = new Paint();

            canvas.save();
            canvas.translate(size.width / 2.0f, size.height / 2.0f);

            int activeTick = (CupertinoActivityIndicatorUtils._kTickCount * position.value).floor();

            for (int i = 0; i < CupertinoActivityIndicatorUtils._kTickCount; ++i) {
                int t = ((i + activeTick) % CupertinoActivityIndicatorUtils._kTickCount);
                paint.color = activeColor.withAlpha(CupertinoActivityIndicatorUtils._alphaValues[t]);
                canvas.drawRRect(tickFundamentalRRect,paint);
                canvas.rotate(-CupertinoActivityIndicatorUtils._kTwoPI / CupertinoActivityIndicatorUtils._kTickCount);
            }

            canvas.restore();
        }

        public override bool shouldRepaint(CustomPainter oldPainter) {
            return ((oldPainter as _CupertinoActivityIndicatorPainter).position != position) || 
                   ((oldPainter as _CupertinoActivityIndicatorPainter).activeColor != activeColor);
        }
    }
}