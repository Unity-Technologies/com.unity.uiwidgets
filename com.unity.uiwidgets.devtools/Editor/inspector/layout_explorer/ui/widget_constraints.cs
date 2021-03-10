using System.Collections.Generic;
using Unity.UIWidgets.material;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.widgets;

namespace Unity.UIWidgets.DevTools.inspector.layout_explorer.ui
{
    public class VisualizeWidthAndHeightWithConstraints : StatelessWidget {
      public VisualizeWidthAndHeightWithConstraints(
        LayoutProperties properties = null,
        float? arrowHeadSize = null,
        Widget child = null,
        bool warnIfUnconstrained = true
      )
      {
        this.properties = properties;
        this.child = child;
        this.arrowHeadSize = arrowHeadSize?? CommonThemeUtils.defaultIconSize;
        this.warnIfUnconstrained = warnIfUnconstrained;
      }

  public readonly Widget child;
  public readonly LayoutProperties properties;
  public readonly float? arrowHeadSize;
  public readonly bool warnIfUnconstrained;
  
  public override Widget build(BuildContext context) {
    var showChildrenWidthsSum =
        properties is FlexLayoutProperties && properties.isOverflowWidth;
    var bottomHeight = ThemeUtils.widthAndConstraintIndicatorSize;
    var rightWidth = ThemeUtils.heightAndConstraintIndicatorSize;
    var colorScheme = Theme.of(context).colorScheme;

    var showOverflowHeight =
        properties is FlexLayoutProperties && properties.isOverflowHeight;

    List<InlineSpan> spans = new List<InlineSpan>();
    spans.Add(new TextSpan(
      text: $"{properties.describeHeight()}"
    ));
    if (properties.constraints != null)
    {
      if (!showOverflowHeight) spans.Add(new TextSpan(text: "\n")); 
      spans.Add(new TextSpan(
        text: $" ({properties.describeHeightConstraints()})",
        style: properties.constraints.hasBoundedHeight ||
               !warnIfUnconstrained
          ? null
          : new TextStyle(
            color: ThemeUtils.unconstrainedColor
          )
      ));
      if (showOverflowHeight)
      {
        spans.Add(new TextSpan(
          text: "\nchildren take: " + 
        $"{InspectorDataModelsUtils.sum(properties.childrenHeights)}"
          ));
      }
        
    }


    var heightDescription = new RotatedBox(
      quarterTurns: 1,
      child: Dimension.dimensionDescription(
          new TextSpan(
            children: spans
          ),
          properties.isOverflowHeight,
          colorScheme)
    );
    var right = new Container(
      margin: EdgeInsets.only(
        top: ThemeUtils.margin,
        left: ThemeUtils.margin,
        bottom: bottomHeight,
        right: ThemeUtils.minPadding // custom margin for not sticking to the corner
      ),
      child: new LayoutBuilder(builder: (context2, constraints) => {
        var displayHeightOutsideArrow =
            constraints.maxHeight < ThemeUtils.minHeightToDisplayHeightInsideArrow;

        List<Widget> widgets = new List<Widget>();
        widgets.Add(new Truncateable(
          truncate: !displayHeightOutsideArrow,
          child: new Container(
            margin: EdgeInsets.symmetric(horizontal: ThemeUtils.arrowMargin),
            child: new ArrowWrapper(
                arrowColor: ThemeUtils.heightIndicatorColor,
                arrowStrokeWidth: ThemeUtils.arrowStrokeWidth,
                arrowHeadSize: arrowHeadSize,
                direction: Axis.vertical,
                child: displayHeightOutsideArrow ? null : heightDescription
              )
            )
        ));
        if (displayHeightOutsideArrow)
        {
          widgets.Add(new Flexible(
            child: heightDescription
          ));
        }
          
        
        return new Row(
          children: widgets
        );
      })
    );

    List<InlineSpan> spams2 = new List<InlineSpan>();
    spams2.Add(new TextSpan(text: $"{properties.describeWidth()}; "));
    if (properties.constraints != null)
    {
      if (!showChildrenWidthsSum) spams2.Add(new TextSpan(text: "\n"));
      spams2.Add(new TextSpan(
        text: $"({properties.describeWidthConstraints()})",
        style:
        properties.constraints.hasBoundedWidth || !warnIfUnconstrained
          ? null
          : new TextStyle(color: ThemeUtils.unconstrainedColor)
      ));
    }

    if (showChildrenWidthsSum)
    {
      spams2.Add(new TextSpan(
        text: "\nchildren take " +
      $"{InspectorDataModelsUtils.sum(properties.childrenWidths)}"
        ));
    }
      
    
    var widthDescription = Dimension.dimensionDescription(
      new TextSpan(
        children: spams2
        ),
      properties.isOverflowWidth,
      colorScheme
    );
    var bottom = new Container(
      margin: EdgeInsets.only(
        top: ThemeUtils.margin,
        left: ThemeUtils.margin,
        right: rightWidth,
        bottom: ThemeUtils.minPadding
      ),
      child: new LayoutBuilder(builder: (context3, constraints) => {
        var maxWidth = constraints.maxWidth;
        var displayWidthOutsideArrow =
            maxWidth < ThemeUtils.minWidthToDisplayWidthInsideArrow;
        List<Widget> widgets = new List<Widget>();
        widgets.Add(new Truncateable(
          truncate: !displayWidthOutsideArrow,
          child: new Container(
            margin: EdgeInsets.symmetric(vertical: ThemeUtils.arrowMargin),
            child: new ArrowWrapper(
                arrowColor: ThemeUtils.widthIndicatorColor,
                arrowHeadSize: arrowHeadSize,
                arrowStrokeWidth: ThemeUtils.arrowStrokeWidth,
                direction: Axis.horizontal,
                child: displayWidthOutsideArrow ? null : widthDescription
              )
            )
        ));
        if (displayWidthOutsideArrow)
        {
          widgets.Add(new Flexible(
            child: new Container(
              padding: EdgeInsets.only(top: ThemeUtils.minPadding),
              child: widthDescription
            )
          ));
        }
          
        
        return new Column(
          children: widgets
        );
      })
    );
    return new BorderLayout(
      center: child,
      right: right,
      rightWidth: rightWidth,
      bottom: bottom,
      bottomHeight: bottomHeight
    );
  }
}

}