using Unity.UIWidgets.animation;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.ui;
using Unity.UIWidgets.widgets;
using UnityEngine;
using Canvas = Unity.UIWidgets.ui.Canvas;
using Color = Unity.UIWidgets.ui.Color;

/**
 * In the animated icon data files, we have made some changes to optimize the compilation speed for il2cpp
 * Specifically, we change all instance of Offsets to 2 separate floats throughout the file
 *
 * In this file, we change the codes a bit to support these changes. Please think twice and test on the Gallery sample
 * if you want to make further changes here
 */

namespace Unity.UIWidgets.material {
    
    static class AnimatedIconUtils {
        public static float _interpolate(float[] values, float progress) {
            D.assert(progress <= 1.0f);
            D.assert(progress >= 0.0f);
            if (values.Length == 2) {
                return values[0];
            }

            float targetIdx = Mathf.Lerp(0, values.Length - 1, progress);
            int lowIdx = targetIdx.floor();
            int highIdx = targetIdx.ceil();
            float t = targetIdx - lowIdx;
            return Mathf.Lerp(values[lowIdx], values[highIdx], t);
        }

        public static void _interpolateOffset(float[] values, float progress, ref float x, ref float y) {
            D.assert(progress <= 1.0f);
            D.assert(progress >= 0.0f);
            D.assert(values.Length % 2 == 0);
            if (values.Length == 2) {
                x = values[0];
                y = values[1];
                return;
            }

            float targetIdx = Mathf.Lerp(0, values.Length / 2 - 1, progress);
            int lowIdx = targetIdx.floor();
            int highIdx = targetIdx.ceil();
            float t = targetIdx - lowIdx;
            x = Mathf.Lerp(values[lowIdx * 2], values[highIdx * 2], t);
            y = Mathf.Lerp(values[lowIdx * 2 + 1], values[highIdx * 2 + 1], t);
        }
    }

    public class AnimatedIcon : StatelessWidget {
        public AnimatedIcon(
            Key key = null,
            AnimatedIconData icon = null,
            Animation<float> progress = null,
            Color color = null,
            float? size = null
        ) : base(key: key) {
            D.assert(progress != null);
            D.assert(icon != null);
            this.progress = progress;
            this.color = color;
            this.size = size;
            this.icon = icon;
        }

        public readonly Animation<float> progress;

        public readonly Color color;

        public readonly float? size;

        public readonly AnimatedIconData icon;

        public static readonly _UiPathFactory _pathFactory = () => new Path();

        public override Widget build(BuildContext context) {
            _AnimatedIconData iconData = (_AnimatedIconData) icon;
            IconThemeData iconTheme = IconTheme.of(context);
            float iconSize = size ?? iconTheme.size ?? 0.0f;
            float? iconOpacity = iconTheme.opacity;
            Color iconColor = color ?? iconTheme.color;
            if (iconOpacity != 1.0f) {
                iconColor = iconColor.withOpacity(iconColor.opacity * (iconOpacity ?? 1.0f));
            }

            return new CustomPaint(
                size: new Size(iconSize, iconSize),
                painter: new _AnimatedIconPainter(
                    paths: iconData.paths,
                    progress: progress,
                    color: iconColor,
                    scale: iconSize / iconData.size.width,
                    uiPathFactory: _pathFactory
                )
            );
        }
    }

    public delegate Path _UiPathFactory();

    class _AnimatedIconPainter : AbstractCustomPainter {
        public _AnimatedIconPainter(
            _PathFrames[] paths = null,
            Animation<float> progress = null,
            Color color = null,
            float? scale = null,
            bool? shouldMirror = null,
            _UiPathFactory uiPathFactory = null
        ) : base(repaint: progress) {
            this.paths = paths;
            this.progress = progress;
            this.color = color;
            this.scale = scale;
            this.shouldMirror = shouldMirror;
            this.uiPathFactory = uiPathFactory;
        }

        public readonly _PathFrames[] paths;
        public readonly Animation<float> progress;
        public readonly Color color;
        public readonly float? scale;
        public readonly bool? shouldMirror;
        public readonly _UiPathFactory uiPathFactory;

        public override void paint(Canvas canvas, Size size) {
            canvas.scale(scale ?? 1.0f, scale ?? 1.0f);
            if (shouldMirror == true) {
                canvas.rotate(Mathf.PI);
                canvas.translate(-size.width, -size.height);
            }

            float clampedProgress = progress.value.clamp(0.0f, 1.0f);
            foreach (_PathFrames path in paths) {
                path.paint(canvas, color, uiPathFactory, clampedProgress);
            }
        }


        public override bool shouldRepaint(CustomPainter _oldDelegate) {
            _AnimatedIconPainter oldDelegate = _oldDelegate as _AnimatedIconPainter;
            return oldDelegate.progress.value != progress.value
                   || oldDelegate.color != color
                   || oldDelegate.paths != paths
                   || oldDelegate.scale != scale
                   || oldDelegate.uiPathFactory != uiPathFactory;
        }

        public override bool? hitTest(Offset position) {
            return null;
        }
    }

    struct _PathOffset {

        public float dx;
        public float dy;
        public _PathOffset(float x, float y) {
            dx = x;
            dy = y;
        }

        public static _PathOffset lerp(_PathOffset a, _PathOffset b, float t) {
            return new _PathOffset(Mathf.Lerp(a.dx, b.dx, t), Mathf.Lerp(a.dy, b.dy, t));
        }
    }

    class _PathFrames {
        public _PathFrames(
            _PathCommand[] commands,
            float[] opacities
        ) {
            this.commands = commands;
            this.opacities = opacities;
        }

        public readonly _PathCommand[] commands;
        public readonly float[] opacities;

        public void paint(Canvas canvas, Color color, _UiPathFactory uiPathFactory, float progress) {
            float opacity = AnimatedIconUtils._interpolate(opacities, progress);
            Paint paint = new Paint();
            paint.style = PaintingStyle.fill;
            paint.color = color.withOpacity(color.opacity * opacity);
            Path path = uiPathFactory();
            foreach (_PathCommand command in commands) {
                command.apply(path, progress);
            }

            canvas.drawPath(path, paint);
        }
    }

    abstract class _PathCommand {
        public _PathCommand() {
        }

        public abstract void apply(Path path, float progress);
    }

    class _PathMoveTo : _PathCommand {
        public _PathMoveTo(float[] points) {
            this.points = points;
        }

        public readonly float[] points;

        public override void apply(Path path, float progress) {
            float dx = 0f, dy = 0f;
            AnimatedIconUtils._interpolateOffset(points, progress, ref dx, ref dy);
            path.moveTo(dx, dy);
        }
    }

    class _PathCubicTo : _PathCommand {
        public _PathCubicTo(float[] controlPoints1, float[] controlPoints2, float[] targetPoints) {
            this.controlPoints1 = controlPoints1;
            this.controlPoints2 = controlPoints2;
            this.targetPoints = targetPoints;
        }

        public readonly float[] controlPoints2;
        public readonly float[] controlPoints1;
        public readonly float[] targetPoints;

        public override void apply(Path path, float progress) {
            float dx1 = 0f, dy1 = 0f, dx2 = 0f, dy2 = 0f, dx = 0f, dy = 0f;
            AnimatedIconUtils._interpolateOffset(controlPoints1, progress, ref dx1, ref dy1);
            AnimatedIconUtils._interpolateOffset(controlPoints2, progress, ref dx2, ref dy2);
            AnimatedIconUtils._interpolateOffset(targetPoints, progress, ref dx, ref dy);
            path.cubicTo(
                dx1, dy1,
                dx2, dy2,
                dx, dy
            );
        }
    }

    class _PathLineTo : _PathCommand {
        public _PathLineTo(float[] points) {
            this.points = points;
        }

        float[] points;

        public override void apply(Path path, float progress) {
            float dx = 0f, dy = 0f;
            AnimatedIconUtils._interpolateOffset(points, progress, ref dx, ref dy);
            path.lineTo(dx, dy);
        }
    }

    class _PathClose : _PathCommand {
        public _PathClose() {
        }

        public override void apply(Path path, float progress) {
            path.close();
        }
    }

    public delegate T _Interpolator<T>(T a, T b, float progress);
}