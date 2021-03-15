using System;
using System.Collections.Generic;
using Unity.UIWidgets.animation;
using Unity.UIWidgets.DevTools;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.material;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.rendering;
using Unity.UIWidgets.ui;
using Unity.UIWidgets.widgets;
using Image = Unity.UIWidgets.widgets.Image;
using TextStyle = Unity.UIWidgets.painting.TextStyle;

namespace Unity.UIWidgets.DevTools.inspector.layout_explorer.ui
{
    public class BorderLayout : StatelessWidget {
      public BorderLayout(
        Key key = null,
        Widget left= null,
        float? leftWidth = null,
        Widget top= null,
        float? topHeight = null,
        Widget right= null,
        float? rightWidth = null,
        Widget bottom = null,
        float? bottomHeight = null,
        Widget center = null
      ) : base(key: key)
      {
        D.assert(left != null ||
                 top != null ||
                 right != null ||
                 bottom != null ||
                 center != null);
        this.left = left;
        this.leftWidth = leftWidth;
        this.top = top;
        this.topHeight = topHeight;
        this.right = right;
        this.rightWidth = rightWidth;
        this.bottom = bottom;
        this.bottomHeight = bottomHeight;
        this.center = center;
      }

  public readonly Widget center;
  public readonly Widget top;
  public readonly Widget left;
  public readonly Widget right;
  public readonly Widget bottom;

  public readonly float? leftWidth;
  public readonly float? rightWidth;
  public readonly float? topHeight;
  public readonly float? bottomHeight;

  CrossAxisAlignment crossAxisAlignment {
    get
    {
      if (left != null && right != null) {
        return CrossAxisAlignment.center;
      } else if (left == null && right != null) {
        return CrossAxisAlignment.start;
      } else if (left != null && right == null) {
        return CrossAxisAlignment.end;
      } else {
        return CrossAxisAlignment.start;
      }
    }
    
  }

  
  public override Widget build(BuildContext context)
  {
    List<Widget> widgets = new List<Widget>();
    widgets.Add(new Center(
      child: new Container(
        margin: EdgeInsets.only(
          left: leftWidth ?? 0,
          right: rightWidth ?? 0,
          top: topHeight ?? 0,
          bottom: bottomHeight ?? 0
        ),
        child: center
      )
    ));
    if (top != null)
    {
      widgets.Add(new Align(
        alignment: Alignment.topCenter,
        child: new Container(height: topHeight, child: top)
      ));
    }

    if (left != null)
    {
      widgets.Add(new Align(
        alignment: Alignment.centerLeft,
        child: new Container(width: leftWidth, child: left)
      ));
    }

    if (right != null)
    {
      widgets.Add(new Align(
        alignment: Alignment.centerRight,
        child: new Container(width: rightWidth, child: right)
      ));
    }

    if (bottom != null)
    {
      widgets.Add(new Align(
        alignment: Alignment.bottomCenter,
        child: new Container(height: bottomHeight, child: bottom)
      ));
    }
    return new Stack(
      children: widgets
    );
  }
}
    
public class Truncateable : StatelessWidget {
  public Truncateable(Key key = null , bool truncate = false, Widget child = null) : base(key: key)
  {
    this.truncate = truncate;
    this.child = child;
  }

  public readonly Widget child;
  public readonly bool truncate;

  
  public override Widget build(BuildContext context) {
    return new Flexible(flex: truncate ? 1 : 0, child: child);
  }
}

public class WidgetVisualizer : StatelessWidget {
  public WidgetVisualizer(
    Key key = null,
    bool isSelected = false,
    string title = null,
    Widget hint = null,
    LayoutProperties layoutProperties = null,
    Widget child = null,
    OverflowSide? overflowSide = null,
    bool largeTitle = false
  ) : base(key: key)
  {
    D.assert(title != null);
    this.child = child;
    this.isSelected = isSelected;
    this.title = title;
    this.hint = hint;
    this.layoutProperties = layoutProperties;
    this.overflowSide = overflowSide;
    this.largeTitle = largeTitle;
  }

  public readonly LayoutProperties layoutProperties;
  public readonly string title;
  public readonly Widget child;
  public readonly Widget hint;
  public readonly bool isSelected;
  public readonly bool largeTitle;

  public readonly OverflowSide? overflowSide;

  public static readonly float overflowIndicatorSize = 20.0f;

  bool drawOverflow
  {
    get
    {
      return overflowSide != null;
    }
  }

 
  public override Widget build(BuildContext context) {
    var theme = Theme.of(context);
    var colorScheme = theme.colorScheme;
    var properties = layoutProperties;
    Color borderColor = ThemeUtils.regularWidgetColor;
    if (properties is FlexLayoutProperties) {
      borderColor =
          ((FlexLayoutProperties)properties)?.direction == Axis.horizontal ? ThemeUtils.rowColor : ThemeUtils.columnColor;
    }
    if (isSelected) {
      borderColor = ThemeUtils.selectedWidgetColor;
    }

    List<Widget> widgets1 = new List<Widget>();
    widgets1.Add(new Flexible(
      child: new Container(
        constraints: new BoxConstraints(
          maxWidth: largeTitle
            ? ThemeUtils.defaultMaxRenderWidth
            : ThemeUtils.minRenderWidth *
              ThemeUtils.widgetTitleMaxWidthPercentage),
        child: new Center(
          child: new Text(
            title,
            style:
            new TextStyle(color: ThemeUtils.widgetNameColor),
            overflow: TextOverflow.ellipsis
          )
        ),
        decoration: new BoxDecoration(color: borderColor),
        padding: EdgeInsets.all(4.0f)
      )
    ));
    if (hint != null)
    {
      widgets1.Add(new Flexible(child: hint)); 
    }
    
    List<Widget> widgets2 = new List<Widget>();
    widgets2.Add(new IntrinsicHeight(
      child: new Row(
        crossAxisAlignment: CrossAxisAlignment.stretch,
        mainAxisAlignment: MainAxisAlignment.spaceBetween,
        children: widgets1
      )
    ));
    if (child != null) widgets2.Add(new Expanded(child: child)); 
    
    List<Widget> widgets3 = new List<Widget>();
    if (drawOverflow)
    {
      widgets3.Add(Positioned.fill(
        child: new CustomPaint(
          painter: new OverflowIndicatorPainter(
            overflowSide.Value,
            overflowIndicatorSize
          )
        )
      ));
      widgets3.Add(new Container(
        margin: EdgeInsets.only(
          right: overflowSide == OverflowSide.right
            ? overflowIndicatorSize
            : 0.0f,
          bottom: overflowSide == OverflowSide.bottom
            ? overflowIndicatorSize
            : 0.0f
        ),
        color: isSelected ? ThemeUtils.backgroundColorSelected : null,
        child: new Column(
          crossAxisAlignment: CrossAxisAlignment.stretch,
          mainAxisSize: MainAxisSize.min,
          children: widgets2
        )
        ));
    }
    
    return new Container(
      child: new Stack(
        children: widgets3
      ),
      decoration: new BoxDecoration(
        border: Border.all(
          color: borderColor
        ),
        color: Color.black //theme.canvasColor.darken()
      )
    );
  }
}

public class AnimatedLayoutProperties<T> : LayoutProperties where T : LayoutProperties
{
  public AnimatedLayoutProperties(T begin, T end, Animation<float> animation)
  {
    D.assert(begin != null);
    D.assert(end != null);
    D.assert(begin.children?.Count == end.children?.Count);
    for (var i = 0; i < begin.children.Count; i++)
      _children.Add(new AnimatedLayoutProperties<LayoutProperties>(
        begin.children[i],
        end.children[i],
        animation
      ));
    this.begin = begin;
    this.end = end;
    this.animation = animation;
  }

  public readonly T begin;
  public readonly T end;
  public readonly Animation<float> animation;
  public readonly List<LayoutProperties> _children;

  
  public new LayoutProperties parent
  {
    get
    {
      return end.parent;
    }
    set 
    {
      end.parent = value;
    }
    
  }
  
  
  
  public new List<LayoutProperties> children {
    get
    {
      return _children;
    }
    
  }

  List<float?> _lerpList(List<float?> l1, List<float?> l2) {
    D.assert(l1.Count == l2.Count);
    List<float?> result = new List<float?>();
    for (var i = 0; i < children.Count; i++)
    {
      result.Add(utils.lerpFloat(l1[i], l2[i], animation.value));
    }
    return result;
  }

  public List<float?> childrenDimensions(Axis axis) { // public override List<float?> childrenDimensions(Axis axis)
    var beginDimensions = begin.childrenDimensions(axis);
    var endDimensions = end.childrenDimensions(axis);
    return _lerpList(beginDimensions, endDimensions);
  }
  
  public new List<float?> childrenHeights
  {
    get
    {
      return _lerpList(begin.childrenHeights, end.childrenHeights);
    }
  }

  public new List<float?> childrenWidths
  {
    get
    {
      return _lerpList(begin.childrenWidths, end.childrenWidths);
    }
  }

  public new BoxConstraints constraints {
    get
    {
      try {
        return BoxConstraints.lerp(
          begin.constraints, end.constraints, animation.value);
      } catch (Exception e) {
        return end.constraints;
      }
    }
    
  }

  public string describeWidthConstraints() { // public override string describeWidthConstraints()
    return constraints.hasBoundedWidth
        ? LayoutProperties.describeAxis(
            constraints.minWidth, constraints.maxWidth, "w")
        : "w=unconstrained";
  }

  public string describeHeightConstraints() { // public override string describeHeightConstraints()
    return constraints.hasBoundedHeight
        ? LayoutProperties.describeAxis(
            constraints.minHeight, constraints.maxHeight, "h")
        : "h=unconstrained";
  }

  public string describeWidth()  //public override string describeWidth()
  {
    return $"w={size.width}";
  }
  
  public new string describeHeight()
  {
    
    return $"h={size.height}";
  }
  
  public new string description
  {
    get
    {
      return end.description;
    }
  }
  
  public float? dimension(Axis axis) { // public override float? dimension(Axis axis)
    return utils.lerpFloat(
      begin.dimension(axis),
      end.dimension(axis),
      animation.value
    );
  }

  
  public new float? flexFactor
  {
    get
    {
      return utils.lerpFloat(begin.flexFactor, end.flexFactor, animation.value);
    }
  }

  
  public new bool hasChildren
  {
    get
    {
      return children.isNotEmpty();
    }
  }
  
  public new float? height
  {
    get
    {
      return size.height;
    }
  }

  
  public new bool isFlex
  {
    get
    {
      return begin.isFlex.Value && end.isFlex.Value;
    }
  }
  
  public new RemoteDiagnosticsNode node
  {
    get
    {
      return end.node;
    }
  }
  
  public new Size size
  {
    get
    {
      return Size.lerp(begin.size, end.size, animation.value);
    }
  }

  
  public new int totalChildren
  {
    get
    {
      return end.totalChildren;
    }
  }
  
  public new float? width
  {
    get
    {
      return size.width;
    }
  }
  
  public new bool hasFlexFactor
  {
    get
    {
      return begin.hasFlexFactor && end.hasFlexFactor;
    }
  }


  public LayoutProperties copyWith( //public override LayoutProperties copyWith
    List<LayoutProperties> children = null,
    BoxConstraints constraints = null,
    string description = null,
    float? flexFactor = null,
    FlexFit? flexFit = null,
    bool? isFlex = null,
    Size size = null
  )
  {
    return new LayoutProperties();
    // return LayoutProperties.values(
    //   node: node,
    //   children: children ?? this.children,
    //   constraints: constraints ?? this.constraints,
    //   description: description ?? this.description,
    //   flexFactor: flexFactor ?? this.flexFactor,
    //   flexFit: flexFit ?? this.flexFit,
    //   isFlex: isFlex ?? this.isFlex,
    //   size: size ?? this.size
    // );
  }
  
  public new bool isOverflowWidth
  {
    get
    {
      return end.isOverflowWidth;
    }
  }
  
  public new bool isOverflowHeight
  {
    get
    {
      return end.isOverflowHeight;
    }
  }
  
  public new FlexFit? flexFit
  {
    get
    {
      return end.flexFit;
    }
  }

  
  public new List<LayoutProperties> displayChildren
  {
    get
    {
      return end.displayChildren;
    }
  }
}

public class LayoutExplorerBackground : StatelessWidget {
  public LayoutExplorerBackground(
    Key key = null,
    ColorScheme colorScheme = null
  ) : base(key: key)
  {
    this.colorScheme = colorScheme;
  }

  public readonly ColorScheme colorScheme;
  
  public override Widget build(BuildContext context) {
    return Positioned.fill(
      child: new Opacity(
        opacity: CommonThemeUtils.isLight ? 0.3f : 0.2f,
        child: Image.asset(
          CommonThemeUtils.isLight
              ? ThemeUtils.negativeSpaceLightAssetName
              : ThemeUtils.negativeSpaceDarkAssetName,
          fit: BoxFit.none,
          repeat: ImageRepeat.repeat,
          alignment: Alignment.topLeft
        )
      )
    );
  }
}

}