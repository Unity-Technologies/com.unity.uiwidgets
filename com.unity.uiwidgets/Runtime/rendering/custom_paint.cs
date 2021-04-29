using System.Collections.Generic;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.gestures;
using Unity.UIWidgets.rendering;
using Unity.UIWidgets.ui;

namespace Unity.UIWidgets.widgets {
    public interface CustomPainter : Listenable {
        
        void paint(Canvas canvas, Size size);

        bool shouldRepaint(CustomPainter oldDelegate);

        bool? hitTest(Offset position);
    }

    public abstract class AbstractCustomPainter : CustomPainter {
        public AbstractCustomPainter(Listenable repaint = null) {
            _repaint = repaint;
        }

        readonly Listenable _repaint;

        public void addListener(VoidCallback listener) {
            _repaint?.addListener(listener);
        }

        public void removeListener(VoidCallback listener) {
            _repaint?.removeListener(listener);
        }

        public abstract void paint(Canvas canvas, Size size);

        public abstract bool shouldRepaint(CustomPainter oldDelegate);

        public virtual bool? hitTest(Offset position) {
            return null;
        }

        public override string ToString() {
            return $"{foundation_.describeIdentity(this)}({_repaint?.ToString() ?? ""})";
        }
    }

    public class RenderCustomPaint : RenderProxyBox {
        public RenderCustomPaint(
            CustomPainter painter = null,
            CustomPainter foregroundPainter = null,
            Size preferredSize = null,
            bool isComplex = false,
            bool willChange = false,
            RenderBox child = null
        ) : base(child) {
            preferredSize = preferredSize ?? Size.zero;
            this.preferredSize = preferredSize;
            _painter = painter;
            _foregroundPainter = foregroundPainter;
            this.isComplex = isComplex;
            this.willChange = willChange;
        }

        CustomPainter _painter;

        public CustomPainter painter {
            get { return _painter; }
            set {
                if (_painter == value) {
                    return;
                }

                CustomPainter oldPainter = _painter;
                _painter = value;
                _didUpdatePainter(_painter, oldPainter);
            }
        }

        CustomPainter _foregroundPainter;

        public CustomPainter foregroundPainter {
            get { return _foregroundPainter; }
            set {
                if (_foregroundPainter == value) {
                    return;
                }

                CustomPainter oldPainter = _foregroundPainter;
                _foregroundPainter = value;
                _didUpdatePainter(_foregroundPainter, oldPainter);
            }
        }

        void _didUpdatePainter(CustomPainter newPainter, CustomPainter oldPainter) {
            if (newPainter == null) {
                D.assert(oldPainter != null);
                markNeedsPaint();
            }
            else if (oldPainter == null ||
                     newPainter.GetType() != oldPainter.GetType() ||
                     newPainter.shouldRepaint(oldPainter)) {
                markNeedsPaint();
            }

            if (attached) {
                oldPainter?.removeListener(markNeedsPaint);
                newPainter?.addListener(markNeedsPaint);
            }
        }

        Size _preferredSize;

        public Size preferredSize {
            get { return _preferredSize; }
            set {
                D.assert(value != null);
                if (preferredSize == value) {
                    return;
                }

                _preferredSize = value;
                markNeedsLayout();
            }
        }

        public bool isComplex;

        public bool willChange;

        public override void attach(object owner) {
            base.attach(owner);
            _painter?.addListener(markNeedsPaint);
            _foregroundPainter?.addListener(markNeedsPaint);
        }

        public override void detach() {
            _painter?.removeListener(markNeedsPaint);
            _foregroundPainter?.removeListener(markNeedsPaint);
            base.detach();
        }

        protected override bool hitTestChildren(BoxHitTestResult result, Offset position = null) {
            if (_foregroundPainter != null && ((_foregroundPainter.hitTest(position)) ?? false)) {
                return true;
            }

            return base.hitTestChildren(result, position: position);
        }


        protected override bool hitTestSelf(Offset position) {
            return _painter != null && (_painter.hitTest(position) ?? true);
        }

        protected override void performResize() {
            size = constraints.constrain(preferredSize);
        }

        void _paintWithPainter(Canvas canvas, Offset offset, CustomPainter painter) {
            int debugPreviousCanvasSaveCount = 0;
            canvas.save();
            D.assert(() => {
                debugPreviousCanvasSaveCount = canvas.getSaveCount();
                return true;
            });
            if (offset != Offset.zero) {
                canvas.translate(offset.dx, offset.dy);
            }

            painter.paint(canvas, size);
            D.assert(() => {
                int debugNewCanvasSaveCount = canvas.getSaveCount();
                if (debugNewCanvasSaveCount > debugPreviousCanvasSaveCount) {
                    throw new UIWidgetsError(new List<DiagnosticsNode>{
                        new ErrorSummary(
                            $"The {painter} custom painter called canvas.save() or canvas.saveLayer() at least " +
                            $"{debugNewCanvasSaveCount - debugPreviousCanvasSaveCount} more " +
                            "times than it called canvas.restore()."
                        ),
                        new ErrorDescription("This leaves the canvas in an inconsistent state and will probably result in a broken display."),
                        new ErrorHint("You must pair each call to save()/saveLayer() with a later matching call to restore().")
                    });
                }

                if (debugNewCanvasSaveCount < debugPreviousCanvasSaveCount) {
                    throw new UIWidgetsError(new List<DiagnosticsNode>{
                        new ErrorSummary(
                            $"The {painter} custom painter called canvas.restore() " +
                            $"{debugPreviousCanvasSaveCount - debugNewCanvasSaveCount} more " +
                            "times than it called canvas.save() or canvas.saveLayer()."
                        ),
                        new ErrorDescription("This leaves the canvas in an inconsistent state and will result in a broken display."),
                        new ErrorHint("You should only call restore() if you first called save() or saveLayer().")
                    });
                }

                return debugNewCanvasSaveCount == debugPreviousCanvasSaveCount;
            });
            canvas.restore();
        }

        public override void paint(PaintingContext context, Offset offset) {
            if (_painter != null) {
                _paintWithPainter(context.canvas, offset, _painter);
                _setRasterCacheHints(context);
            }

            base.paint(context, offset);
            if (_foregroundPainter != null) {
                _paintWithPainter(context.canvas, offset, _foregroundPainter);
                _setRasterCacheHints(context);
            }
        }

        void _setRasterCacheHints(PaintingContext context) {
            if (isComplex) {
                context.setIsComplexHint();
            }

            if (willChange) {
                context.setWillChangeHint();
            }
        }
    }
}