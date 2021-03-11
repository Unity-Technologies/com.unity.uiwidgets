using System;
using System.Collections.Generic;
using uiwidgets;
using Unity.UIWidgets.DevTools.ui;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.material;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.ui;
using Unity.UIWidgets.widgets;
using Image = Unity.UIWidgets.widgets.Image;
using TextStyle = Unity.UIWidgets.painting.TextStyle;

namespace Unity.UIWidgets.DevTools.ui
{

  public class IconsUtils
  {
    public static Image createImageIcon(string url, float? size = null) {
      return new Image(
        image: new AssetImage(url),
        height: size?? CommonThemeUtils.defaultIconSize,
        width: size?? CommonThemeUtils.defaultIconSize
      );
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

  public Image baseIcon
  {
    get
    {
      return kind.icon;
    }
  }
  
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
  public readonly Dictionary<string, CustomIcon> iconCache = new Dictionary<string, CustomIcon>();

  public CustomIcon getCustomIcon(string fromText, IconKind kind = null, bool isAbstract = false) 
  {
      kind = kind ?? IconKind.classIcon;
      if (fromText?.isEmpty() != false) 
      {
        return null;
      }

    string text = char.ToUpper(fromText[0]).ToString();
    string mapKey = $"{text}_${kind.name}_{isAbstract}";

    return iconCache.putIfAbsent(mapKey, () => {
      return new CustomIcon(kind: kind, text: text, isAbstract: isAbstract);
    });
  }

  CustomIcon fromWidgetName(string name) {
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

  CustomIcon fromInfo(String name) {
    if (name == null) {
      return null;
    }

    if (name.isEmpty()) {
      return null;
    }

    return getCustomIcon(name, kind: IconKind.info);
  }

  bool isAlphabetic(int ch) {
    return (ch < '0' || ch > '9') &&
        ch != '_' &&
        ch != '$';
  }
}

public class IconKind {
  public IconKind(string name, Image icon, Image abstractIcon = null)
  {
    this.name = name;
    this.icon = icon;
    this.abstractIcon = abstractIcon ?? icon;
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
      size: new Size(CommonThemeUtils.defaultIconSize, CommonThemeUtils.defaultIconSize)
    );
  }
}

public class ColorIconMaker {
  public readonly Dictionary<Color, ColorIcon> iconCache = new Dictionary<Color, ColorIcon>();

  ColorIcon getCustomIcon(Color color) {
    return iconCache.putIfAbsent(color, () => new ColorIcon(color));
  }
}

public class _ColorIconPainter : CustomPainter {
  public _ColorIconPainter(Color color, ColorScheme colorScheme)
  {
    this.color = color;
    this.colorScheme = colorScheme;
  }

  public readonly Color color;

  public readonly ColorScheme colorScheme;
  public const float iconMargin = 1;

  
  public void paint(Canvas canvas, Size size)
  {
    var greyPaint = new Paint();
    greyPaint.color = Colors.grey;
    var iconRect = Rect.fromLTRB(
      iconMargin,
      iconMargin,
      size.width - iconMargin,
      size.height - iconMargin
    );

    Paint bgPaint = new Paint();
    bgPaint.color = CommonThemeUtils.defaultBackground;

    canvas.drawRect(
      Rect.fromLTRB(
        iconMargin,
        iconMargin,
        size.width - iconMargin,
        size.height - iconMargin
      ),
      bgPaint
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
      new Paint()
      {
        color = color
      }
    );
    Paint temp2 = new Paint();
    
      
      canvas.drawRect(
        iconRect,
        new Paint()
          {
            style = PaintingStyle.stroke,
            color = CommonThemeUtils.defaultForeground
          }
      );
  }
  
  public bool shouldRepaint(CustomPainter oldDelegate) {
    // if (oldDelegate is _ColorIconPainter) {
    //   return ((_ColorIconPainter)oldDelegate).colorScheme.isLight != CommonThemeUtils.isLight;
    // }
    return true;
  }

  public bool? hitTest(Offset position)
  {
    throw new NotImplementedException();
  }

  public void addListener(VoidCallback listener)
  {
    throw new NotImplementedException();
  }

  public void removeListener(VoidCallback listener)
  {
    throw new NotImplementedException();
  }
}

public class FlutterMaterialIcons {
  public FlutterMaterialIcons(){}

  static Icon getIconForCodePoint(int charCode, ColorScheme colorScheme) {
    return new Icon(new IconData(charCode), color: CommonThemeUtils.defaultForeground);
  }
}



public class Octicons {
  public static readonly IconData bug = new IconData(61714, fontFamily: "Octicons");
  public static readonly IconData info = new IconData(61778, fontFamily: "Octicons");
  public static readonly IconData deviceMobile = new IconData(61739, fontFamily: "Octicons");
  public static readonly IconData fileZip = new IconData(61757, fontFamily: "Octicons");
  public static readonly IconData clippy = new IconData(61724, fontFamily: "Octicons");
  public static readonly IconData package = new IconData(61812, fontFamily: "Octicons");
  public static readonly IconData dashboard = new IconData(61733, fontFamily: "Octicons");
  public static readonly IconData pulse = new IconData(61823, fontFamily: "Octicons");
}
}