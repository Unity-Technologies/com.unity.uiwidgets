using uiwidgets;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.ui;

namespace Unity.UIWidgets.material {
    public class UnderlineTabIndicator : Decoration {
        public UnderlineTabIndicator(
            BorderSide borderSide = null,
            EdgeInsetsGeometry insets = null) {
            borderSide = borderSide ?? new BorderSide(width: 2.0f, color: Colors.white);
            insets = insets ?? EdgeInsets.zero;
            this.borderSide = borderSide;
            this.insets = insets;
        }

        public readonly BorderSide borderSide;

        public readonly EdgeInsetsGeometry insets;

        public override Decoration lerpFrom(Decoration a, float t) {
            if (a is UnderlineTabIndicator) {
                UnderlineTabIndicator _a = (UnderlineTabIndicator) a;
                return new UnderlineTabIndicator(
                    borderSide: BorderSide.lerp(_a.borderSide, borderSide, t),
                    insets: EdgeInsetsGeometry.lerp(_a.insets, insets, t)
                );
            }

            return base.lerpFrom(a, t);
        }

        public override Decoration lerpTo(Decoration b, float t) {
            if (b is UnderlineTabIndicator) {
                UnderlineTabIndicator _b = (UnderlineTabIndicator) b;
                return new UnderlineTabIndicator(
                    borderSide: BorderSide.lerp(borderSide, _b.borderSide, t),
                    insets: EdgeInsetsGeometry.lerp(insets, _b.insets, t)
                );
            }

            return base.lerpTo(b, t);
        }


        public override BoxPainter createBoxPainter(VoidCallback onChanged) {
            return new _UnderlinePainter(this, onChanged);
        }
    }


    class _UnderlinePainter : BoxPainter {
        public _UnderlinePainter(
            UnderlineTabIndicator decoration = null,
            VoidCallback onChanged = null
        ) : base(onChanged: onChanged) {
            D.assert(decoration != null);
            this.decoration = decoration;
        }

        public readonly UnderlineTabIndicator decoration;

        public BorderSide borderSide {
            get { return decoration.borderSide; }
        }

        public EdgeInsetsGeometry insets {
            get { return decoration.insets; }
        }

        Rect _indicatorRectFor(Rect rect, TextDirection textDirection) {
            D.assert(rect != null);
            Rect indicator = insets.resolve(textDirection).deflateRect(rect);
            return Rect.fromLTWH(
                indicator.left,
                indicator.bottom - borderSide.width,
                indicator.width,
                borderSide.width);
        }

        public override void paint(Canvas canvas, Offset offset, ImageConfiguration configuration) {
            D.assert(configuration != null);
            D.assert(configuration.size != null);
            Rect rect = offset & configuration.size;
            TextDirection textDirection = configuration.textDirection;
            Rect indicator = _indicatorRectFor(rect, textDirection).deflate(borderSide.width / 2.0f);
            Paint paint = borderSide.toPaint();
            paint.strokeCap = StrokeCap.square;
            canvas.drawLine(indicator.bottomLeft, indicator.bottomRight, paint);
        }
    }
}