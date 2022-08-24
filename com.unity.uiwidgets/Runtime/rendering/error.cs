using System;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.ui;

namespace Unity.UIWidgets.rendering {
    public static partial class rendering_ {
        public static float _kMaxWidth = 100000.0f;
        public static float _kMaxHeight = 100000.0f;
    }

    public class RenderErrorBox : RenderBox {

        protected override bool sizedByParent
        {
            get { return true; }
        }
        public RenderErrorBox(string message = "") {
            this.message = message;
            if (message == "") {
                return;
            }

            ParagraphBuilder builder = new ParagraphBuilder(paragraphStyle);
            builder.pushStyle(textStyle);
            builder.addText(message);
            _paragraph = builder.build();
        }

        string message;
        Paragraph _paragraph;

        protected internal override float computeMaxIntrinsicWidth(float height) {
            return rendering_._kMaxWidth;
        }

        protected internal override float computeMaxIntrinsicHeight(float width) {
            return rendering_._kMaxHeight;
        }

        protected override bool hitTestSelf(Offset position) => true;

        protected override void performResize() {
            size = constraints.constrain(new Size(rendering_._kMaxWidth, rendering_._kMaxHeight));
        }

        public static EdgeInsets padding = EdgeInsets.fromLTRB(64, 96, 64, 12);

        public static float minimumWidth = 200;

        public static Color backgroundColor = _initBackgroundColor();

        public static Color _initBackgroundColor() {
            Color result = new Color(0xF0C0C0C0);
            D.assert(() => {
                result = new Color(0xF0900000);
                return true;
            });
            return result;
        }

        public static ui.TextStyle textStyle = _initTextStyle();

        public static ui.TextStyle _initTextStyle() {
            ui.TextStyle result = new ui.TextStyle(
                color: new Color(0xFF303030),
                fontFamily: "sans-serif",
                fontSize: 18.0f
            );
            D.assert(() => {
                result = new ui.TextStyle(
                    color: new Color(0xFFFFFF66),
                    fontFamily: "monospace",
                    fontSize: 14.0f,
                    fontWeight: FontWeight.bold
                );
                return true;
            });
            return result;
        }

        public static ParagraphStyle paragraphStyle = new ParagraphStyle(
            textDirection: TextDirection.ltr,
            textAlign: TextAlign.left
        );

        public override void paint(PaintingContext context, Offset offset) {
            try {
                context.canvas.drawRect(offset & size, new Paint() {color = backgroundColor});
                if (_paragraph != null) {
                    float width = size.width;
                    float left = 0.0f;
                    float top = 0.0f;
                    if (width > padding.left + minimumWidth + padding.right) {
                        width -= padding.left + padding.right;
                        left += padding.left;
                    }

                    _paragraph.layout(new ParagraphConstraints(width: width));
                    if (size.height > padding.top + _paragraph.height() + padding.bottom) {
                        top += padding.top;
                    }

                    context.canvas.drawParagraph(_paragraph, offset + new Offset(left, top));
                }
            }
            catch (Exception) {
                // Intentionally left empty.
            }
        }
    }
}