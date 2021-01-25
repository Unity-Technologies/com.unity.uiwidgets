using Unity.UIWidgets.foundation;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.rendering;
using Unity.UIWidgets.ui;
using TextStyle = Unity.UIWidgets.painting.TextStyle;

namespace Unity.UIWidgets.widgets {
    public class Icon : StatelessWidget {
        public Icon(
            IconData icon,
            Key key = null,
            float? size = null,
            Color color = null,
            TextDirection? textDirection = null
        ) : base(key: key) {
            this.icon = icon;
            this.size = size;
            this.color = color;
            this.textDirection = textDirection;
        }

        public readonly IconData icon;

        public readonly float? size;

        public readonly Color color;

        public readonly TextDirection? textDirection;
        public override Widget build(BuildContext context) {
            D.assert(this.textDirection != null || WidgetsD.debugCheckHasDirectionality(context)); 
            TextDirection textDirection = this.textDirection ?? Directionality.of(context);
            IconThemeData iconTheme = IconTheme.of(context);
            float iconSize = size ?? iconTheme.size.Value;

            if (icon == null) {
                return new SizedBox(width: iconSize, height: iconSize);
            }

            float iconOpacity = iconTheme.opacity.Value;
            Color iconColor = color ?? iconTheme.color;
            if (iconOpacity != 1.0) {
                iconColor = iconColor.withOpacity(iconColor.opacity * iconOpacity);
            }
            Widget iconWidget = new RichText(
                overflow: TextOverflow.visible, // Never clip.
                textDirection: textDirection, // Since we already fetched it for the assert...
                text: new TextSpan(
                    text: new string(new[] {(char) icon.codePoint}),
                    style: new TextStyle(
                        inherit: false,
                        color: iconColor,
                        fontSize: iconSize,
                        fontFamily: icon.fontFamily
                    )
                )
            );

            var matrix = Matrix4.identity();
            matrix.scale(-1.0f, 1.0f, 1.0f);
            if (icon.matchTextDirection) {
                switch (textDirection) {
                    case TextDirection.rtl:
                        iconWidget = new Transform(
                            transform: matrix,
                            alignment: Alignment.center,
                            transformHitTests: false,
                            child: iconWidget
                        );
                        break;
                    case TextDirection.ltr:
                        break;
                }
            }
            return new SizedBox(
                width: iconSize,
                height: iconSize,
                child: new Center(
                    child: iconWidget
                )
            );
        }
        
        public override void debugFillProperties(DiagnosticPropertiesBuilder properties) {
            base.debugFillProperties(properties);
            properties.add(new IconDataProperty("icon", icon, ifNull: "<empty>", showName: false));
            properties.add(new FloatProperty("size", size, defaultValue: null));
            properties.add(new ColorProperty("color", color, defaultValue: null));
        }
    }
}