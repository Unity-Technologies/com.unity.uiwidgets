using System;
using System.Collections.Generic;
using Unity.UIWidgets.DevTools.inspector;
using Unity.UIWidgets.DevTools.ui;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.material;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.ui;
using Unity.UIWidgets.widgets;
using UnityEditor;
using Image = Unity.UIWidgets.widgets.Image;
using TextStyle = Unity.UIWidgets.painting.TextStyle;

namespace Unity.UIWidgets.DevTools.ui
{

    public static class IconsUtils
    {
        public static Image createImageIcon(string url, float size = ThemeUtils.defaultIconSize) {
            return new Image(
                image: new FileImage(url),
                height: size,
                width: size
            );
        }
    }
    
    public class ColorIconMaker {
        
         Dictionary<Color, ColorIcon> iconCache = new Dictionary<Color, ColorIcon>();
         public ColorIcon getCustomIcon(Color color) {
            return iconCache.putIfAbsent(color, () => new ColorIcon(color));
        }
    }
    
    
    public class CustomIcon : StatelessWidget {
        public CustomIcon(
            IconKind kind = null,
            string text = null,
            bool isAbstract = false
        )
        {
            this.kind = kind;
            this.text = text;
            this.isAbstract = isAbstract;
        }

        public readonly IconKind kind;
        public readonly string text;
        public readonly bool isAbstract;

    Image baseIcon => kind.icon;
    
    public override Widget build(BuildContext context) {
    return new Container(
    width: baseIcon.width,
    height: baseIcon.height,
    child: new Stack(
    alignment: AlignmentDirectional.center,
    children: new List<Widget>{
        baseIcon,
        new Text(
            text,
            textAlign: TextAlign.center,
            style: new TextStyle(fontSize: 9, color: new Color(0xFF231F20))
    ),
    }
    )
    );
}
}
    
    
    public class CustomIconMaker {
        Dictionary<string, CustomIcon> iconCache = new Dictionary<string, CustomIcon>();

        CustomIcon getCustomIcon(string fromText,
        IconKind kind = null, bool isAbstract = false)
        {
            if (kind == null)
            {
                kind = IconKind.classIcon;
            }
            if (fromText?.isEmpty() != false) {
                return null;
            }
            
            string text = toUpperCase(fromText[0].ToString());
            string mapKey = $"{text}_{kind.name}_{isAbstract}";

            return iconCache.putIfAbsent(mapKey, () => {
                return new CustomIcon(kind: kind, text: text, isAbstract: isAbstract);
            });
        }

        string toUpperCase(string str)
        {
            if (str != null)
            {
                string retStr = string.Empty;
                for (int i = 0; i < str.Length; i++)
                {
                    if (str[i]>='a'&&str[i]<='z')
                    {
                        retStr += (char)(str[i] - 'a' + 'A');
                        continue;
                    }
                    retStr += str[i];
                }
                return retStr;
            }

            return "str is null";
        }


        public CustomIcon fromWidgetName(string name) {
            if (name == null) {
                return null;
            }

            bool isPrivate = name.StartsWith("_");
            while (name.isNotEmpty() && !isAlphabetic(name[0])) {
                name = name.Substring(1);
            }

            if (name.isEmpty()) {
                return null;
            }

            return getCustomIcon(
                name,
                kind: isPrivate ? IconKind.method : IconKind.classIcon
            );
        }

        public CustomIcon fromInfo(String name) {
            if (name == null) {
                return null;
            }

            if (name.isEmpty()) {
                return null;
            }

            return getCustomIcon(name, kind: IconKind.info);
        }

        bool isAlphabetic(int _char) {
            return (_char < '0' || _char > '9') &&
                   _char != '_' &&
                   _char != '$';
        }
    }   
    
    
    public class IconKind {
        public IconKind(string name, Image icon, Image abstractIcon = null)
        {
            this.name = name;
            this.icon = icon;
            abstractIcon = abstractIcon ?? icon;
        }

        public static IconKind classIcon = new IconKind(
            "class",
            IconsUtils.createImageIcon("icons/custom/class.png"),
            IconsUtils.createImageIcon("icons/custom/class_abstract.png")
        );
        public static IconKind field = new IconKind(
            "fields",
            IconsUtils.createImageIcon("icons/custom/fields.png")
        );
        public static IconKind _interface = new IconKind(
            "interface",
            IconsUtils.createImageIcon("icons/custom/interface.png")
            );
        public static IconKind method = new IconKind(
            "method",
            IconsUtils.createImageIcon("icons/custom/method.png"),
            IconsUtils.createImageIcon("icons/custom/method_abstract.png")
        );
        public static IconKind property = new IconKind(
            "property",
            IconsUtils.createImageIcon("icons/custom/property.png")
        );
        public static IconKind info = new IconKind(
            "info",
            IconsUtils.createImageIcon("icons/custom/info.png")
        );

        public readonly string name;
        public readonly Image icon;
        public readonly Image abstractIcon;
    }
    
    public class ColorIcon : StatelessWidget {
        public ColorIcon(Color color)
        {
            this.color = color;
        }

        public readonly Color color;
    
        public override Widget build(BuildContext context) {
            var colorScheme = Theme.of(context).colorScheme;
            return new CustomPaint(
                painter: new _ColorIconPainter(color, colorScheme),
                size: new Size(ThemeUtils.defaultIconSize, ThemeUtils.defaultIconSize)
                );
        }
    }
    
    public static class FlutterMaterialIcons {

        public static Icon getIconForCodePoint(int charCode, ColorScheme colorScheme) {
            return new Icon(new IconData(charCode), color: ThemeUtils.defaultForeground);
        }
    }
    
    
    class _ColorIconPainter : AbstractCustomPainter {
        public _ColorIconPainter(Color color, ColorScheme colorScheme)
        {
            this.color = color;
            this.colorScheme = colorScheme;
        }

        public readonly Color color;

        public readonly ColorScheme colorScheme;
        public static readonly float iconMargin = 1.0f;

    
        public override void paint(Canvas canvas, Size size) {
            // draw a black and gray grid to use as the background to disambiguate
            // opaque colors from translucent colors.
            Paint greyPaint = new Paint();
            Paint defaultPaint = new Paint();
            Paint paint = new Paint();
            Paint paint2 = new Paint();
            paint2.style = PaintingStyle.stroke;
            paint2.color = ThemeUtils.defaultForeground;
            paint.color = color;
            defaultPaint.color = ThemeUtils.defaultBackground;
            greyPaint.color = ThemeUtils.grey;
            var iconRect = Rect.fromLTRB(
                iconMargin,
                iconMargin,
                size.width - iconMargin,
                size.height - iconMargin
            );
            canvas.drawRect(
                Rect.fromLTRB(
                    iconMargin,
                    iconMargin,
                    size.width - iconMargin,
                    size.height - iconMargin
                ),
                defaultPaint
            );
            canvas.drawRect(
                Rect.fromLTRB(
                    iconMargin,
                    iconMargin,
                    size.width * 0.5f,
                    size.height * 0.5f
                ),
                greyPaint
            );
            canvas.drawRect(
                Rect.fromLTRB(
                    size.width * 0.5f,
                    size.height * 0.5f,
                    size.width - iconMargin,
                    size.height - iconMargin
                ),
                greyPaint
            );
            canvas.drawRect(
                iconRect,
                paint
            );
            canvas.drawRect(
                    iconRect,
                    paint2
                );
        }
    
        public override bool shouldRepaint(CustomPainter oldDelegate) {
            // if (oldDelegate is _ColorIconPainter) {
            //     return ((_ColorIconPainter)oldDelegate).colorScheme.isLight != InspectorTreeUtils.isLight;
            // }
            return true;
        }
    }
    
}