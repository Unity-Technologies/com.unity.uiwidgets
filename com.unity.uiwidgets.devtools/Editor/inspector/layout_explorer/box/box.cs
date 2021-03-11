using System;
using System.Collections.Generic;
using System.Linq;
using Unity.UIWidgets.DevTools.inspector.layout_explorer.ui;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.material;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.rendering;
using Unity.UIWidgets.ui;
using Unity.UIWidgets.widgets;
using UnityEngine;
using ThemeUtils = Unity.UIWidgets.DevTools.inspector.layout_explorer.ui.ThemeUtils;

namespace Unity.UIWidgets.DevTools.inspector.layout_explorer.box{

  public class BoxUtils
  {
    public static string describeBoxName(LayoutProperties properties) {

      var title = properties.node.description;
      var renderDescription = properties.node?.renderObject?.description;
      // TODO(jacobr): consider de-emphasizing the render object name by putting it
      // in more transparent text or just calling the widget Parent instead of
      // surfacing a widget name.
      if(renderDescription != null) {
        title += " - $renderDescription";
      }
      return title;
    }
  }
  
    public class BoxLayoutExplorerWidget : LayoutExplorerWidget {
      public BoxLayoutExplorerWidget(
        InspectorController inspectorController = null,
        Key key = null
      ) : base(inspectorController, key: key)
      {
        
      }

      public static bool shouldDisplay(RemoteDiagnosticsNode node) {
        // TODO(jacobr) pass a RemoteDiagnosticsNode to this method that contains
        // the layout explorer related supplemental properties so that we can
        // accurately determine whether the widget uses box layout.
        return node != null;
      }


  public override State createState()
  {
    return new _BoxLayoutExplorerWidgetState();
  }
      
}

public class _BoxLayoutExplorerWidgetState : 
  LayoutExplorerWidgetState<BoxLayoutExplorerWidget, LayoutProperties> 
{
  
  public override RemoteDiagnosticsNode getRoot(RemoteDiagnosticsNode node) {
    if (!shouldDisplay(node)) return null;
    return node;
  }

  
  public override bool shouldDisplay(RemoteDiagnosticsNode node) {
    return BoxLayoutExplorerWidget.shouldDisplay(selectedNode);
  }

  
  public override AnimatedLayoutProperties<LayoutProperties> computeAnimatedProperties(
    LayoutProperties nextProperties
  ) {
    return new AnimatedLayoutProperties<LayoutProperties>(
      animatedProperties?.copyWith() ?? properties,
      nextProperties,
      changeAnimation
    );
  }

  
  public override LayoutProperties computeLayoutProperties(RemoteDiagnosticsNode node)
  {
    return new LayoutProperties(node);
  }
  
  public override void updateHighlighted(LayoutProperties newProperties) {
    setState(() =>{
      // This implementation will need to change if we support showing more than
      // a single widget in the box visualization for the layout explorer.
      if (newProperties != null && selectedNode == newProperties.node) {
        highlighted = newProperties;
      } else {
        highlighted = null;
      }
    });
  }
  
  public override Widget build(BuildContext context) {
    if (properties == null) return new SizedBox();
    return new Container(
      margin: EdgeInsets.all(ThemeUtils.margin),
      padding: EdgeInsets.only(bottom: ThemeUtils.margin, right: ThemeUtils.margin),
      child: new AnimatedBuilder(
        animation: changeController,
        builder: (context2, _) => {
          return new LayoutBuilder(builder: _buildLayout);
        }
      )
    );
  }
  
  /// TODO(jacobr): see if we can unify with the stylized version of the overall
  /// layout used for Flex. Our constraints are quite different as we can
  /// guarantee that the entire layout fits without scrolling while in the Flex
  /// case that would be difficult.
  
  static List<float?> minFractionLayout(
    float? availableSize,
    List<float?> sizes,
    List<float?> minFractions
  ) {
    D.assert(sizes.Count == minFractions.Count);
    var length = sizes.Count;
    float total = 1.0f;
    var fractions = minFractions.ToList();
    foreach (var size in sizes) {
      if (size != null) {
        total += Mathf.Max(0, size.Value);
      }
    }

    float totalFraction = 0.0f;
    for (int i = 0; i < length; i++) {
      var size = sizes[i];
      if (size != null) {
        fractions[i] = Mathf.Max(size.Value / total, minFractions[i]?? 0.0f);
        totalFraction += fractions[i].Value;
      } else {
        fractions[i] = 0.0f;
      }
    }
    if (Mathf.Abs(totalFraction - 1.0f) > 1E-10) {
      for (int i = 0; i < length; i++) {
        fractions[i] = fractions[i] / totalFraction;
      }
    }
    var output = new List<float?>();
    foreach (var fraction in fractions) {
      output.Add(fraction * availableSize);
    }
    return output;
  }

  public Widget _buildChild(BuildContext context) {
    var theme = Theme.of(context);
    var colorScheme = theme.colorScheme;
    var parentProperties = this.parentProperties ?? properties; 

    var parentSize = parentProperties.size;
    var boxParentData = new BoxParentData();
    boxParentData.offset = new Offset(0, 0);
    var offset = properties.node?.parentData ?? boxParentData;

    if (properties.size == null) {
      return new Center(
        child: new Text(
          "Visualizing layouts for ${properties.description} widgets is not yet supported."
        )
      );
    }
    return new LayoutBuilder(
      builder: (BuildContext context2, BoxConstraints constraints) =>
      {
        // Subtract out one pixel border on each side.
        var availableHeight = constraints.maxHeight - 2;
        var availableWidth = constraints.maxWidth - 2;

        var minFractions = new List<float?> {0.2f, 0.5f, 0.2f};

        float? nullOutZero(float value)
        {
          return value != 0.0f ? value : (float?) null;
        }

        var widths = new List<float?>
        {
          nullOutZero(offset.offset.dx),
          properties.size.width,
          nullOutZero(parentSize != null
            ? parentSize.width - (properties.size.width + offset.offset.dx)
            : 0.0f)
        };
        var heights = new List<float?>
        {
          nullOutZero(offset.offset.dy),
          properties.size.height,
          nullOutZero(parentSize != null
            ? parentSize.height - (properties.size.height + offset.offset.dy)
            : 0.0f)
        };
        // 3 element array with [left padding, widget width, right padding].
        var displayWidths = minFractionLayout(
          availableSize: availableWidth,
          sizes: widths,
          minFractions: minFractions
        );
        // 3 element array with [top padding, widget height, bottom padding].
        var displayHeights = minFractionLayout(
          availableSize: availableHeight,
          sizes: heights,
          minFractions: minFractions
        );
        var widgetWidth = displayWidths[1];
        var widgetHeight = displayHeights[1];
        var safeParentSize = parentSize ?? properties.size;

        List<Widget> widgets = new List<Widget>();
        widgets.Add(new LayoutExplorerBackground(colorScheme: colorScheme));
        if (widths[0] != null)
        {
          widgets.Add(new PaddingVisualizerWidget(
            new RenderProperties(
              axis: Axis.horizontal,
              size: new Size(displayWidths[0].Value, widgetHeight.Value),
              offset: new Offset(0, displayHeights[0].Value),
              realSize: new Size(widths[0].Value, safeParentSize.height),
              layoutProperties: properties,
              isFreeSpace: true
            ),
            horizontal: true
            ));
        }

        if (heights[0] != null)
        {
          widgets.Add(new PaddingVisualizerWidget(
            new RenderProperties(
              axis: Axis.horizontal,
              size: new Size(widgetWidth.Value, displayHeights[0].Value),
              offset: new Offset(displayWidths[0].Value, 0),
              realSize: new Size(safeParentSize.width, heights[0].Value),
              layoutProperties: properties,
              isFreeSpace: true
            ),
            horizontal: false
          ));
        }

        if (widths[2] != null)
        {
          widgets.Add(new PaddingVisualizerWidget(
            new RenderProperties(
              axis: Axis.horizontal,
              size: new Size(displayWidths[2].Value, widgetHeight.Value),
              offset: new Offset(
                displayWidths[0].Value + displayWidths[1].Value, displayHeights[0].Value),
              realSize: new Size(widths[2].Value, safeParentSize.height),
              layoutProperties: properties,
              isFreeSpace: true
            ),
            horizontal: true
          ));
        }

        if (heights[2] != null)
        {
          widgets.Add(new PaddingVisualizerWidget(
            new RenderProperties(
              axis: Axis.horizontal,
              size: new Size(widgetWidth.Value, displayHeights[2].Value),
              offset: new Offset(displayWidths[0].Value,
                displayHeights[0].Value + displayHeights[1].Value),
              realSize: new Size(safeParentSize.width, heights[2].Value),
              layoutProperties: properties,
              isFreeSpace: true
            ),
            horizontal: false
          ));
        }
        widgets.Add(new BoxChildVisualizer(
            isSelected: true,
            state: this,
            layoutProperties: properties,
            renderProperties: 
            new RenderProperties(
              axis: Axis.horizontal,
              size: new Size(widgetWidth.Value, widgetHeight.Value),
              offset: new Offset(displayWidths[0].Value, displayHeights[0].Value),
              realSize: properties.size,
              layoutProperties: properties
            )
          ));
          
          
        return new Container(
          width: constraints.maxWidth,
          height: constraints.maxHeight,
          decoration: new BoxDecoration(
            border: Border.all(
              color: ThemeUtils.regularWidgetColor
            )
          ),
          child: new Stack(
            children: widgets
          )
        );
      }
    );
  }

  LayoutProperties parentProperties {
    get
    {
      var parentElement = properties?.node?.parentRenderElement;
      if (parentElement == null) return null;
      var parentProperties = computeLayoutProperties(parentElement);
      if (parentProperties.size == null) return null;
      return parentProperties;
    }
    
  }

  Widget _buildLayout(BuildContext context, BoxConstraints constraints) {
    var maxHeight = constraints.maxHeight;
    var maxWidth = constraints.maxWidth;

    Widget widget = _buildChild(context);
    var parentProperties = this.parentProperties;
    if (parentProperties != null) {
      widget = new WidgetVisualizer(
        // TODO(jacobr): this node's name can be misleading more often than
        // in the flex case the widget doesn't have its own RenderObject.
        // Consider showing the true ancestor for the summary tree that first
        // has a different render object.
        title: BoxUtils.describeBoxName(parentProperties),
        largeTitle: true,
        layoutProperties: parentProperties,
        isSelected: false,
        child: new VisualizeWidthAndHeightWithConstraints(
          properties: parentProperties,
          warnIfUnconstrained: false,
          child: new Padding(
            padding: EdgeInsets.all(CommonThemeUtils.denseSpacing),
            child: widget
          )
        )
      );
    }
    return new Container(
      constraints: new BoxConstraints(maxWidth: maxWidth, maxHeight: maxHeight),
      child: widget
    );
  }

}

  

public class BoxChildVisualizer : StatelessWidget {
  public BoxChildVisualizer(
    Key key = null,
    _BoxLayoutExplorerWidgetState state = null,
    LayoutProperties layoutProperties = null,
    RenderProperties renderProperties = null,
    bool isSelected = false
  ) : base(key: key)
  {
    this.state = state;
    this.layoutProperties = layoutProperties;
    this.renderProperties = renderProperties;
    this.isSelected = isSelected;
  }

  public readonly _BoxLayoutExplorerWidgetState state;

  public readonly bool isSelected;
  public readonly LayoutProperties layoutProperties;
  public readonly RenderProperties renderProperties;

  public LayoutProperties root
  {
    get
    {
      return state.properties;
    }
  }

  public LayoutProperties properties
  {
    get
    {
      return renderProperties.layoutProperties;
    }
  }

  
  public override Widget build(BuildContext context2) {
    var renderSize = renderProperties.size;
    var renderOffset = renderProperties.offset;

    Widget buildEntranceAnimation(BuildContext context3, Widget child) {
      var size = renderSize;
      // TODO(jacobr): does this entrance animation really add value.
      return new Opacity(
        opacity: Mathf.Min(state.entranceCurve.value * 5, 1.0f),
        child: new Padding(
          padding: EdgeInsets.symmetric(
            horizontal: Mathf.Max(0.0f, (renderSize.width - size.width) / 2),
            vertical: Mathf.Max(0.0f, (renderSize.height - size.height) / 2)
          ),
          child: child
        )
      );
    }

    return new Positioned(
      top: renderOffset.dy,
      left: renderOffset.dx,
      child: new InkWell(
        onTap: () => state.onTap(properties),
        onDoubleTap: () => state.onDoubleTap(properties),
        onLongPress: () => state.onDoubleTap(properties),
        child: new SizedBox(
          width: utils.safePositiveFloat(renderSize.width),
          height: utils.safePositiveFloat(renderSize.height),
          child: new AnimatedBuilder(
            animation: state.entranceController,
            builder: buildEntranceAnimation,
            child: new WidgetVisualizer(
              isSelected: isSelected,
              layoutProperties: layoutProperties,
              title: BoxUtils.describeBoxName(properties),
              // TODO(jacobr): consider surfacing the overflow size information
              // if we determine
              // overflowSide: properties.overflowSide,

              // We only show one child at a time so a large title is safe.
              largeTitle: true,
              child: new VisualizeWidthAndHeightWithConstraints(
                arrowHeadSize: ThemeUtils.arrowHeadSize,
                properties: properties,
                warnIfUnconstrained: false,
                child: null
              )
            )
          )
        )
      )
    );
  }
}
}