using Unity.UIWidgets.foundation;
using Unity.UIWidgets.ui;
using UnityEngine;
using Canvas = Unity.UIWidgets.ui.Canvas;
using Path = System.IO.Path;

namespace Unity.UIWidgets.widgets {
    public class Lottie : StatefulWidget {
        public Skottie _skottie = null;
        public int _round = 0;
        public float _frame = 0;
        public float _duration = 0;
        public Size _size = null;

        public Lottie(string path, float frame = 0, Size size = null, int round = -1) {
            D.assert(path != null);
            _skottie = new Skottie(Path.Combine(Application.streamingAssetsPath, path));
            _duration = _skottie.duration();
            _round = round;
            _frame = frame * _duration;
            _size = size;
        }

        public override State createState() {
            return new LottieState(_frame, _round);
        }
    }

    public class LottieState : State<Lottie> {
        float _frame = 0;
        int _round = 0;

        public LottieState(float frame, int round) {
            _frame = frame;
            _round = round;
        }

        public override Widget build(BuildContext context) {
            if (_round != 0) {
                WidgetsBinding.instance.addPostFrameCallback((_) => {
                    if (!mounted) {
                        return;
                    }
                    setState(() => {
                        _frame += Time.deltaTime;
                        if (_frame > widget._duration) {
                            _frame -= widget._duration;
                            if (_round > 0) {
                                _round--;
                            }
                        }
                    });
                });
            }
            
            return new CustomPaint(
                size: widget._size,
                painter: new LottiePainter(
                    skottie: widget._skottie,
                    frame: _frame
                )
            );
        }
    }

    public class LottiePainter : AbstractCustomPainter {
        public Skottie _skottie = null;
        float _frame = 0;

        public LottiePainter(Skottie skottie, float frame) {
            _skottie = skottie;
            _frame = frame;
        }

        public override void paint(Canvas canvas, Size size) {
            _skottie.paint(canvas, Offset.zero, size.width, size.height, _frame);
        }

        public override bool shouldRepaint(CustomPainter oldDelegate) {
            if (oldDelegate is LottiePainter oldLottiePainter) {
                return _frame != oldLottiePainter._frame;
            }
            return true;
        }
    }
}