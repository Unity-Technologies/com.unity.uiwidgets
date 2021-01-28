using System.Collections.Generic;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.ui;

namespace Unity.UIWidgets.cupertino {
    public class CupertinoThumbPainterUtils {
        public static Color _kThumbBorderColor = new Color(0x0A000000);

        public static List<BoxShadow> _kSwitchBoxShadows = new List<BoxShadow> {
            new BoxShadow(
                color: new Color(0x26000000),
                offset: new Offset(0, 3),
                blurRadius: 8.0f
            ),
            new BoxShadow(
                color: new Color(0x0F000000),
                offset: new Offset(0, 3),
                blurRadius: 1.0f
            ),
        };

        public static List<BoxShadow> _kSliderBoxShadows = new List<BoxShadow>(){
            new BoxShadow(
                color: new Color(0x26000000),
                offset: new Offset(0, 3),
                blurRadius: 8.0f
            ),
            new BoxShadow(
                color: new Color(0x29000000),
                offset: new Offset(0, 1),
                blurRadius: 1.0f
            ),
            new BoxShadow(
                color: new Color(0x1A000000),
                offset: new Offset(0, 3),
                blurRadius: 1.0f
            )
        };

    }

    public class CupertinoThumbPainter {
        public CupertinoThumbPainter(
            Color color = null,
            List<BoxShadow> shadows = null
        ) {

            this.color = color ?? CupertinoColors.white;
            this.shadows = shadows ?? CupertinoThumbPainterUtils._kSliderBoxShadows;
        }

        public static CupertinoThumbPainter switchThumb(
            Color color = null,
            List<BoxShadow> shadows = null
        ) {
            return new CupertinoThumbPainter(
                color : color ?? CupertinoColors.white,
                shadows  : shadows ?? CupertinoThumbPainterUtils._kSwitchBoxShadows);

        }

        public readonly Color color;

        public readonly List<BoxShadow> shadows;

        public readonly Paint _shadowPaint;

        public const float radius = 14.0f;

        public const float extension = 7.0f;
        
        

        public void paint(Canvas canvas, Rect rect) {
            RRect rrect = RRect.fromRectAndRadius(
                rect,
                Radius.circular(rect.shortestSide / 2.0f)
            );

            foreach (BoxShadow shadow in shadows)
                canvas.drawRRect(rrect.shift(shadow.offset), shadow.toPaint());
            canvas.drawRRect(
                rrect.inflate(0.5f),
                new Paint(){color = CupertinoThumbPainterUtils._kThumbBorderColor}
            );
            canvas.drawRRect(rrect, new Paint(){color = color});
        }
    }
}