using System.Collections.Generic;
using Unity.UIWidgets.async;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.ui;

namespace Unity.UIWidgets.painting {
    public abstract class ShaderWarmUp {
        protected ShaderWarmUp() {
        }

        public virtual Size size => new Size(100.0f, 100.0f);
        protected abstract Future warmUpOnCanvas(Canvas canvas);

        public Future execute() {
            PictureRecorder recorder = new PictureRecorder();
            Canvas canvas = new Canvas(recorder);

            return warmUpOnCanvas(canvas).then(_ => {
                Picture picture = recorder.endRecording();
                //TimelineTask shaderWarmUpTask = TimelineTask();
                //shaderWarmUpTask.start('Warm-up shader');

                picture.toImage(size.width.ceil(), size.height.ceil()).then(__ => {
                    //shaderWarmUpTask.finish();
                    return FutureOr.nil;
                });
                return FutureOr.nil;
            });
        }
    }

    public class DefaultShaderWarmUp : ShaderWarmUp {
        public DefaultShaderWarmUp(
            float drawCallSpacing = 0.0f,
            Size canvasSize = null
        ) {
            canvasSize = canvasSize ?? new Size(100.0f, 100.0f);
            this.drawCallSpacing = drawCallSpacing;
            this.canvasSize = canvasSize;
        }

        public readonly float drawCallSpacing;

        public readonly Size canvasSize;

        public override Size size => canvasSize;

        /// Trigger common draw operations on a canvas to warm up GPU shader
        /// compilation cache.
        protected override Future warmUpOnCanvas(Canvas canvas) {
            RRect rrect = RRect.fromLTRBXY(20.0f, 20.0f, 60.0f, 60.0f, 10.0f, 10.0f);
            Path rrectPath = new Path();
            rrectPath.addRRect(rrect);

            Path circlePath = new Path();
            circlePath.addOval(
                Rect.fromCircle(center: new Offset(40.0f, 40.0f), radius: 20.0f)
            );

            // The following path is based on
            // https://skia.org/user/api/SkCanvas_Reference#SkCanvas_drawPath
            Path path = new Path();
            path.moveTo(20.0f, 60.0f);
            path.quadraticBezierTo(60.0f, 20.0f, 60.0f, 60.0f);
            path.close();
            path.moveTo(60.0f, 20.0f);
            path.quadraticBezierTo(60.0f, 60.0f, 20.0f, 60.0f);

            Path convexPath = new Path();
            convexPath.moveTo(20.0f, 30.0f);
            convexPath.lineTo(40.0f, 20.0f);
            convexPath.lineTo(60.0f, 30.0f);
            convexPath.lineTo(60.0f, 60.0f);
            convexPath.lineTo(20.0f, 60.0f);
            convexPath.close();

            // Skia uses different shaders based on the kinds of paths being drawn and
            // the associated paint configurations. According to our experience and
            // tracing, drawing the following paths/paints generates various of
            // shaders that are commonly used.
            List<Path> paths = new List<Path> {rrectPath, circlePath, path, convexPath};

            List<Paint> paints = new List<Paint> {
                new Paint {
                    isAntiAlias = true,
                    style = PaintingStyle.fill
                },
                new Paint {
                    isAntiAlias = false,
                    style = PaintingStyle.fill
                },
                new Paint {
                    isAntiAlias = true,
                    style = PaintingStyle.stroke,
                    strokeWidth = 10
                },
                new Paint {
                    isAntiAlias = true,
                    style = PaintingStyle.stroke,
                    strokeWidth = 0.1f, // hairline
                }
            };

            // Warm up path stroke and fill shaders.
            for (int i = 0; i < paths.Count; i += 1) {
                canvas.save();
                foreach (var paint in paints) {
                    canvas.drawPath(paths[i], paint);
                    canvas.translate(drawCallSpacing, 0.0f);
                }

                canvas.restore();
                canvas.translate(0.0f, drawCallSpacing);
            }

            // Warm up shadow shaders.
            Color black = new Color(0xFF000000);
            canvas.save();
            canvas.drawShadow(rrectPath, black, 10.0f, true);
            canvas.translate(drawCallSpacing, 0.0f);
            canvas.drawShadow(rrectPath, black, 10.0f, false);
            canvas.restore();

            // Warm up text shaders.
            canvas.translate(0.0f, drawCallSpacing);

            // final ui.ParagraphBuilder paragraphBuilder = ui.ParagraphBuilder(
            //     ui.ParagraphStyle(textDirection: ui.TextDirection.ltr),
            // )..pushStyle(ui.TextStyle(color: black))..addText('_');
            // final ui.Paragraph paragraph = paragraphBuilder.build()
            //     ..layout(const ui.ParagraphConstraints  (width: 60.0));
            // canvas.drawParagraph(paragraph,  const ui.Offset  (20.0, 20.0));

            // Draw a rect inside a rrect with a non-trivial intersection. If the
            // intersection is trivial (e.g., equals the rrect clip), Skia will optimize
            // the clip out.
            //
            // Add an integral or fractional translation to trigger Skia's non-AA or AA
            // optimizations (as did before in normal FillRectOp in rrect clip cases).
            foreach (var fraction in new[] {0.0f, 0.5f}) {
                canvas.save();
                canvas.translate(fraction, fraction);
                canvas.clipRRect(RRect.fromLTRBR(8, 8, 328, 248, Radius.circular(16)));
                canvas.drawRect(Rect.fromLTRB(10, 10, 320, 240), new Paint());
                canvas.restore();
                canvas.translate(drawCallSpacing, 0.0f);
            }

            canvas.translate(0.0f, drawCallSpacing);
            return Future.value();
        }
    }
}