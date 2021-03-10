using System.Collections.Generic;
using Unity.UIWidgets.ui;
using Unity.UIWidgets.widgets;

namespace Unity.UIWidgets.DevTools.inspector.layout_explorer.ui
{
    public class OverflowIndicatorPainter : CustomPainter {
        public  OverflowIndicatorPainter(OverflowSide side, float size)
        {
            this.side = side;
            this.size = size;
            indicatorPaint.shader = Gradient.linear(
                new Offset(0.0f, 0.0f),
                new Offset(10.0f, 10.0f),
    
                new List<Color>{
                    black, yellow, yellow, black
                },
    
                new List<float>{
                    0.25f, 0.25f, 0.75f, 0.75f
                },
                TileMode.repeated
            );
        }

        public readonly OverflowSide side;
        public readonly float size;

        Color black = new Color(0xBF000000);
        Color yellow = new Color(0xBFFFFF00);
        Paint indicatorPaint = new Paint();
        

    
        public void paint(Canvas canvas, Size size) {
            var bottomOverflow = OverflowSide.bottom == side;
            var width = bottomOverflow ? size.width : this.size;
            var height = !bottomOverflow ? size.height : this.size;

            var left = bottomOverflow ? 0.0f : size.width - width;
            var top = side == OverflowSide.right ? 0.0f : size.height - height;
            var rect = Rect.fromLTWH(left, top, width, height);
            
            canvas.drawRect(rect, indicatorPaint);
        }

    
        public bool shouldRepaint(CustomPainter oldDelegate) {
            return oldDelegate is OverflowIndicatorPainter &&
                   (side != ((OverflowIndicatorPainter)oldDelegate).side || size != ((OverflowIndicatorPainter)oldDelegate).size);
        }

        public bool? hitTest(Offset position)
        {
            throw new System.NotImplementedException();
        }

        public void addListener(VoidCallback listener)
        {
            throw new System.NotImplementedException();
        }

        public void removeListener(VoidCallback listener)
        {
            throw new System.NotImplementedException();
        }
    }

}