using Unity.UIWidgets.foundation;
using Unity.UIWidgets.rendering;
using Unity.UIWidgets.ui;
using Unity.UIWidgets.painting;

namespace Unity.UIWidgets.widgets {
    public class ImageIcon: StatelessWidget {
        public ImageIcon(
            ImageProvider image , 
            Key key = null, 
            float? size = null,
            Color color = null
        ) : base(key: key) {

            this.image = image;
            this.size = size;
            this.color = color;
        }

        public readonly  ImageProvider image;
        public readonly  float? size;
        public readonly  Color color;

        public override Widget build(BuildContext context) { 
            IconThemeData iconTheme = IconTheme.of(context);
            float? iconSize = size == null ?  iconTheme.size : size;

            if (image == null)
                return new SizedBox(width: iconSize, height: iconSize);
            float? iconOpacity = iconTheme.opacity; 
            Color iconColor = color ?? iconTheme.color;
            if (iconOpacity != null && iconOpacity != 1.0) 
                iconColor = iconColor.withOpacity(iconColor.opacity * (iconOpacity ?? 1.0f));
            
            return new Image(
                image: image,
                width: iconSize,
                height: iconSize,
                color: iconColor,
                fit: BoxFit.scaleDown,
                alignment: Alignment.center
                //excludeFromSemantics: true
            );
        }
        public override void debugFillProperties(DiagnosticPropertiesBuilder properties) {
            base.debugFillProperties(properties);
            properties.add(new DiagnosticsProperty<ImageProvider>("image'", image, ifNull: "<empty>", showName: false));
            properties.add(new FloatProperty("size", size, defaultValue: null));
            properties.add(new ColorProperty("color", color, defaultValue: null));
        }
    }

}