using Unity.UIWidgets.foundation;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.rendering;
using Unity.UIWidgets.ui;
using TextStyle = Unity.UIWidgets.painting.TextStyle;

namespace Unity.UIWidgets.widgets {
    public class Icon : StatelessWidget {
        public Icon(IconData icon,
            Key key = null,
            float? size = null,
            Color color = null
        ) : base(key: key) {
            this.icon = icon;
            this.size = size;
            this.color = color;
        }

        public readonly IconData icon;

        public readonly float? size;

        public readonly Color color;

        public override Widget build(BuildContext context) {
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
                overflow: TextOverflow.visible,
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