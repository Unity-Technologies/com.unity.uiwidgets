/*using System.Collections.Generic;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.material;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.rendering;
using Unity.UIWidgets.widgets;

namespace Unity.UIWidgets.DevTools.inspector.layout_explorer.ui
{
    public class FreeSpaceVisualizerWidget : StatelessWidget {
      public FreeSpaceVisualizerWidget(
        RenderProperties renderProperties,
        Key key = null
      ) : base(key: key)
      {
        
      }

  public readonly RenderProperties renderProperties;
  
  public override Widget build(BuildContext context) {
    var colorScheme = Theme.of(context).colorScheme;
    var heightDescription =
        $"h={renderProperties.realHeight}";
    var widthDescription = $"w={renderProperties.realWidth}";
    var showWidth = renderProperties.realWidth !=
        (renderProperties.layoutProperties?.width);
    var widthWidget = new Container(
      child: new Column(
        mainAxisAlignment: MainAxisAlignment.center,
        children: new List<Widget>{
          new Flexible(
            child: Dimension.dimensionDescription(
              new TextSpan(
                text: widthDescription
              ),
              false,
              colorScheme
            )
          ),
          new Container(
            margin: EdgeInsets.symmetric(vertical: ThemeUtils.arrowMargin),
            child: new ArrowWrapper(
              arrowColor: ThemeUtils.widthIndicatorColor,
              direction: Axis.horizontal,
              arrowHeadSize: ThemeUtils.arrowHeadSize
            )
          ),
        }
      )
      );
    var heightWidget = new Container(
      width: ThemeUtils.heightOnlyIndicatorSize,
      child: new Row(
        mainAxisAlignment: MainAxisAlignment.center,
        children: new List<Widget>{
          new Flexible(
            child: Dimension.dimensionDescription(
              new TextSpan(text: heightDescription),
              false,
              colorScheme
            )
          ),
          new Container(
            margin: EdgeInsets.symmetric(horizontal: ThemeUtils.arrowMargin),
            child: new ArrowWrapper(
              arrowColor: ThemeUtils.heightIndicatorColor,
              direction: Axis.vertical,
              arrowHeadSize: ThemeUtils.arrowHeadSize,
              childMarginFromArrow: 0.0f
            )
          )
        }
      )
    );
    return new Positioned(
      top: renderProperties.offset.dy,
      left: renderProperties.offset.dx,
      child: new Container(
        width: renderProperties.width,
        height: renderProperties.height,
        child: new Tooltip(
          message: $"{widthDescription}\n{heightDescription}",
          child: showWidth ? widthWidget : heightWidget
        )
      )
    );
  }
}

public class PaddingVisualizerWidget : StatelessWidget {
  public PaddingVisualizerWidget(
    RenderProperties renderProperties,
    bool horizontal,
    Key key = null
  ) : base(key: key)
  {
    this.renderProperties = renderProperties;
    this.horizontal = horizontal;
  }

  public readonly RenderProperties renderProperties;
  public readonly bool horizontal;
  
  public override Widget build(BuildContext context) {
    var colorScheme = Theme.of(context).colorScheme;
    var heightDescription =
        $"h={renderProperties.realHeight}";
    var widthDescription = $"w={renderProperties.realWidth}";
    var widthWidget = new Container(child:
        new Column(
          mainAxisAlignment: MainAxisAlignment.center,
          children: new List<Widget>{
            new Flexible(
              child: Dimension.dimensionDescription(
                new TextSpan(
                  text: widthDescription
                ),
                false,
                colorScheme
              )
            ),
            new Container(
              margin: EdgeInsets.symmetric(vertical: ThemeUtils.arrowMargin),
              child: new ArrowWrapper(
                arrowColor: ThemeUtils.widthIndicatorColor,
                direction: Axis.horizontal,
                arrowHeadSize: ThemeUtils.arrowHeadSize
              )
            )
          }
        )
      );
    var heightWidget = new Container(
      width: ThemeUtils.heightOnlyIndicatorSize,
      child: new Row(
        mainAxisAlignment: MainAxisAlignment.center,
        children: new List<Widget>{
          new Flexible(
            child: Dimension.dimensionDescription(
              new TextSpan(text: heightDescription),
              false,
              colorScheme
            )
          ),
          new Container(
            margin: EdgeInsets.symmetric(horizontal: ThemeUtils.arrowMargin),
            child: new ArrowWrapper(
              arrowColor: ThemeUtils.heightIndicatorColor,
              direction: Axis.vertical,
              arrowHeadSize: ThemeUtils.arrowHeadSize,
              childMarginFromArrow: 0.0f
            )
          )
        }
      )
    );
    return new Positioned(
      top: renderProperties.offset.dy,
      left: renderProperties.offset.dx,
      child: new Container(
        width: utils.safePositiveFloat(renderProperties.width),
        height: utils.safePositiveFloat(renderProperties.height),
        child: horizontal ? widthWidget : heightWidget
      )
    );
  }
}

}*/