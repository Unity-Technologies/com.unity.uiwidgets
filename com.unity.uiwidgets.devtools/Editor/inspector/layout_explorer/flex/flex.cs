/*using System;
using System.Collections.Generic;
using System.Linq;
using Unity.UIWidgets.animation;
using Unity.UIWidgets.async2;
using Unity.UIWidgets.DevTools.inspector.layout_explorer.ui;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.material;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.rendering;
using Unity.UIWidgets.ui;
using Unity.UIWidgets.widgets;
using UnityEngine;
using UnityEngine.UIElements;
using Align = Unity.UIWidgets.widgets.Align;
using Color = Unity.UIWidgets.ui.Color;
using FontStyle = Unity.UIWidgets.ui.FontStyle;
using Image = Unity.UIWidgets.widgets.Image;
using Object = System.Object;
using TextStyle = Unity.UIWidgets.painting.TextStyle;
using ThemeUtils = Unity.UIWidgets.DevTools.inspector.layout_explorer.ui.ThemeUtils;

namespace Unity.UIWidgets.DevTools.inspector.layout_explorer.flex
{
  public class FlexLayoutExplorerWidget : LayoutExplorerWidget {
        public FlexLayoutExplorerWidget(
        InspectorController inspectorController, 
        Key key = null
        ) : base(inspectorController, key: key){}

        public static bool shouldDisplay(RemoteDiagnosticsNode node) {
          return (node?.isFlex ?? false) || (node?.parent?.isFlex ?? false);
        }

  
        public override State createState()
        {
          return new _FlexLayoutExplorerWidgetState();
        }
    }

    public class _FlexLayoutExplorerWidgetState : LayoutExplorerWidgetState<FlexLayoutExplorerWidget, FlexLayoutProperties> {
        ScrollController scrollController = new ScrollController();

        public Axis? direction
        {
            get
            {
                return properties.direction;
            }
        }

  Color horizontalColor(ColorScheme colorScheme)
  {
    return properties.isMainAxisHorizontal
      ? ThemeUtils.mainAxisColor
      : ThemeUtils.crossAxisColor;
  }

  Color verticalColor(ColorScheme colorScheme)
  {
    return properties.isMainAxisVertical
      ? ThemeUtils.mainAxisColor
      : ThemeUtils.crossAxisColor;
  }

  Color horizontalTextColor(ColorScheme colorScheme)
  {
    return properties.isMainAxisHorizontal
      ? ThemeUtils.mainAxisTextColor
      : ThemeUtils.crossAxisTextColor;
  }

  Color verticalTextColor(ColorScheme colorScheme)
  {
    return properties.isMainAxisVertical
      ? ThemeUtils.mainAxisTextColor
      : ThemeUtils.crossAxisTextColor;
  }

  string flexType
  {
    get
    {
      return properties.type;
    }
  }
  
  public override RemoteDiagnosticsNode getRoot(RemoteDiagnosticsNode node) {
    if (!shouldDisplay(node)) return null;
    if (node.isFlex) return node;
    return node.parent;
  }
  
  public override bool shouldDisplay(RemoteDiagnosticsNode node) {
    return FlexLayoutExplorerWidget.shouldDisplay(selectedNode);
  }
  
  public override AnimatedLayoutProperties<FlexLayoutProperties> computeAnimatedProperties(
      FlexLayoutProperties nextProperties) {
    return new AnimatedFlexLayoutProperties(
      (FlexLayoutProperties)animatedProperties?.copyWith() ?? properties,
      nextProperties,
      changeAnimation
    );
  }
  
  public override FlexLayoutProperties computeLayoutProperties(RemoteDiagnosticsNode node)
  {
    return FlexLayoutProperties.fromDiagnostics(node);
  }
  
  public override void updateHighlighted(FlexLayoutProperties newProperties) {
    setState(() => {
      if (selectedNode.isFlex) {
        highlighted = newProperties;
      } else {
        var idx = selectedNode.parent.childrenNow.IndexOf(selectedNode);
        if (newProperties == null || newProperties.children == null) return;
        if (idx != -1) highlighted = newProperties.children[idx];
      }
    });
  }

  Widget _buildAxisAlignmentDropdown(Axis axis, ColorScheme colorScheme) {
    Color color = axis == direction
        ? ThemeUtils.mainAxisTextColor
        : ThemeUtils.crossAxisTextColor;
    List<object> alignmentEnumEntries;
    Object selected;
    if (axis == direction)
    {
      alignmentEnumEntries = new List<object>
      {
        Enum.GetValues(typeof(MainAxisAlignment))
          .Cast<MainAxisAlignment>().ToList()
      };
        
      selected = properties.mainAxisAlignment;
    } else {
      alignmentEnumEntries = alignmentEnumEntries = new List<object>
      {
        Enum.GetValues(typeof(CrossAxisAlignment))
          .Cast<CrossAxisAlignment>().ToList()
      };
      if (properties.textBaseline == null) {
        // TODO(albertusangga): Look for ways to visualize baseline when it is null
        alignmentEnumEntries.Remove(CrossAxisAlignment.baseline);
      }
      selected = properties.crossAxisAlignment;
    }

    List<DropdownMenuItem<object>> dropdownMenuItems = new List<DropdownMenuItem<object>>();
    foreach (var alignment in alignmentEnumEntries)
    {
      dropdownMenuItems.Add(new DropdownMenuItem<object>(
        value: alignment,
        child: new Container(
          padding: EdgeInsets.symmetric(vertical: ThemeUtils.margin),
          child: new Row(
            mainAxisAlignment: MainAxisAlignment.end,
            children: new List<Widget>{
            new Expanded(
              child: new Text(
                alignment.ToString(),
                style: new TextStyle(color: color),
                textAlign: TextAlign.center,
                overflow: TextOverflow.ellipsis
              )
            ),
            new Flexible(
              child: Image.asset(
                (axis == direction)
                  ? FlexUtils.mainAxisAssetImageUrl(direction.Value, (MainAxisAlignment)alignment)
                  : FlexUtils.crossAxisAssetImageUrl(direction.Value, (CrossAxisAlignment)alignment),
                fit: BoxFit.fitHeight,
                color: color
              )
            )
          }
          )
        )
        ));
    }
    
      
    
    return new RotatedBox(
      quarterTurns: axis == Axis.vertical ? 3 : 0,
      child: new Container(
        constraints: new BoxConstraints(
          maxWidth: ThemeUtils.dropdownMaxSize,
          maxHeight: ThemeUtils.dropdownMaxSize
        ),
        child: new DropdownButton<object>(
          value: selected,
          isExpanded: true,
          // Avoid showing an underline for the main axis and cross-axis drop downs.
          underline: new SizedBox(),
          iconEnabledColor: axis == properties.direction
              ? ThemeUtils.mainAxisColor
              : ThemeUtils.crossAxisColor,
          selectedItemBuilder: (context) =>
          {
            List<Widget> widgets = new List<Widget>();

            foreach (var alignment in alignmentEnumEntries)
            {
              widgets.Add(new Row(
                mainAxisAlignment: MainAxisAlignment.center,
                children: new List<Widget>{
                  new Expanded(
                    flex: 2,
                    child: new Text(
                      alignment.ToString(),
                      style: new TextStyle(color: color),
                      textAlign: TextAlign.center,
                      overflow: TextOverflow.ellipsis
                    )
                  ),
                  new Flexible(
                    child: Image.asset(
                      (axis == direction)
                        ? FlexUtils.mainAxisAssetImageUrl(direction.Value, (MainAxisAlignment)alignment)
                        : FlexUtils.crossAxisAssetImageUrl(direction.Value, (CrossAxisAlignment)alignment),
                      height: ThemeUtils.axisAlignmentAssetImageHeight,
                      fit: BoxFit.fitHeight,
                      color: color
                    )
                  )
                }
              ));
            }
            return widgets;
          },
          items: dropdownMenuItems,
          onChanged: (object newSelection) => {
            FlexLayoutProperties changedProperties;
            if (axis == direction) {
              changedProperties =
                  properties.copyWith(mainAxisAlignment: (MainAxisAlignment)newSelection);
            } else {
              changedProperties =
                  properties.copyWith(crossAxisAlignment: (CrossAxisAlignment)newSelection);
            }
            //[!!!] not sure about this
            var service = properties.node.inspectorService;
            var valueRef = properties.node.valueRef;
            markAsDirty();
            // service.invokeSetFlexProperties(
            //   valueRef,
            //   changedProperties.mainAxisAlignment,
            //   changedProperties.crossAxisAlignment
            // );
            
          }
        )
      )
    );
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

  Widget _buildLayout(BuildContext context, BoxConstraints constraints) {
    var colorScheme = Theme.of(context).colorScheme;
    var maxHeight = constraints.maxHeight;
    var maxWidth = constraints.maxWidth;
    var flexDescription = new Align(
      alignment: Alignment.centerLeft,
      child: new Container(
        margin: EdgeInsets.only(
          top: ThemeUtils.mainAxisArrowIndicatorSize,
          left: ThemeUtils.crossAxisArrowIndicatorSize + ThemeUtils.margin
        ),
        child: new InkWell(
          onTap: () => onTap(properties),
          child: new WidgetVisualizer(
            title: flexType,
            layoutProperties: properties,
            isSelected: highlighted == properties,
            overflowSide: properties.overflowSide,
            hint: new Container(
              padding: EdgeInsets.all(4.0f),
              child: new Text(
                $"Total Flex Factor: {properties?.totalFlex}",
                textScaleFactor: ThemeUtils.largeTextScaleFactor,
                style: new TextStyle(
                  color: ThemeUtils.emphasizedTextColor,
                  fontWeight: FontWeight.bold
                ),
                overflow: TextOverflow.ellipsis
              )
            ),
            child: new VisualizeFlexChildren(
              state: this,
              properties: properties,
              children: children,
              highlighted: highlighted,
              scrollController: scrollController,
              direction: direction
            )
          )
        )
      )
    );

    var verticalAxisDescription = new Align(
      alignment: Alignment.bottomLeft,
      child: new Container(
        margin: EdgeInsets.only(top: ThemeUtils.mainAxisArrowIndicatorSize + ThemeUtils.margin),
        width: ThemeUtils.crossAxisArrowIndicatorSize,
        child: new Column(
          children: new List<Widget>{
            new Expanded(
              child: new ArrowWrapper(
                arrowColor: verticalColor(colorScheme),
                child: new Truncateable(
                  truncate: maxHeight <= ThemeUtils.minHeightToAllowTruncating,
                  child: new RotatedBox(
                    quarterTurns: 3,
                    child: new Text(
                      properties.verticalDirectionDescription,
                      overflow: TextOverflow.ellipsis,
                      textAlign: TextAlign.center,
                      textScaleFactor: ThemeUtils.largeTextScaleFactor,
                      style: new TextStyle(
                        color: verticalTextColor(colorScheme)
                      )
                    )
                  )
                ),
                type: ArrowType.down
              )
            ),
            new Truncateable(
              truncate: maxHeight <= ThemeUtils.minHeightToAllowTruncating,
              child: _buildAxisAlignmentDropdown(Axis.vertical, colorScheme)
            ),
          }
        )
      )
    );

    var horizontalAxisDescription = new Align(
      alignment: Alignment.topRight,
      child: new Container(
        margin: EdgeInsets.only(left: ThemeUtils.crossAxisArrowIndicatorSize + ThemeUtils.margin),
        height: ThemeUtils.mainAxisArrowIndicatorSize,
        child: new Row(
          children: new List<Widget>{
            new Expanded(
              child: new ArrowWrapper(
                arrowColor: horizontalColor(colorScheme),
                child: new Truncateable(
                  truncate: maxWidth <= ThemeUtils.minWidthToAllowTruncating,
                  child: new Text(
                    properties.horizontalDirectionDescription,
                    overflow: TextOverflow.ellipsis,
                    textAlign: TextAlign.center,
                    textScaleFactor: ThemeUtils.largeTextScaleFactor,
                    style: new TextStyle(color: horizontalTextColor(colorScheme))
                  )
                ),
                type: ArrowType.right
              )
            ),
            new Truncateable(
              truncate: maxWidth <= ThemeUtils.minWidthToAllowTruncating,
              child: _buildAxisAlignmentDropdown(Axis.horizontal, colorScheme)
            ),
          }
        )
      )
    );

    return new Container(
      constraints: new BoxConstraints(maxWidth: maxWidth, maxHeight: maxHeight),
      child: new Stack(
        children: new List<Widget>{
          flexDescription,
          verticalAxisDescription,
          horizontalAxisDescription
        }
      )
    );
  }
}

public class VisualizeFlexChildren : StatefulWidget {
    public VisualizeFlexChildren(
        Key key = null,
        _FlexLayoutExplorerWidgetState state = null,
        FlexLayoutProperties properties = null, 
        List<LayoutProperties> children = null,
        LayoutProperties highlighted = null, 
        ScrollController scrollController = null,
        Axis? direction = null
    ) : base(key: key)
    {
      this.state = state;
      this.properties = properties;
      this.children = children;
      this.highlighted = highlighted;
      this.scrollController = scrollController;
      this.direction = direction;
    }

    public readonly FlexLayoutProperties properties;
    public readonly List<LayoutProperties> children;
    public readonly LayoutProperties highlighted;
    public readonly ScrollController scrollController;
    public readonly Axis? direction;
    public readonly _FlexLayoutExplorerWidgetState state;
  
    public override State createState()
    {
      return new _VisualizeFlexChildrenState();
    }
}

public class _VisualizeFlexChildrenState : State<VisualizeFlexChildren> {
  LayoutProperties lastHighlighted;
  
  public static readonly GlobalKey selectedChildKey = GlobalKey.key(debugLabel: "selectedChild");
  
  public override Widget build(BuildContext context) {
    if (lastHighlighted != widget.highlighted) {
      lastHighlighted = widget.highlighted;
      if (widget.highlighted != null) {
        WidgetsBinding.instance.addPostFrameCallback((_) => {
          var selectedRenderObject =
              selectedChildKey.currentContext?.findRenderObject();
          if (selectedRenderObject != null &&
              widget.scrollController.hasClients) {
            widget.scrollController.position.ensureVisible(
              selectedRenderObject,
              alignment: 0.5f,
              duration: CommonThemeUtils.defaultDuration
            );
          }
        });
      }
    }
    if (!widget.properties.hasChildren) {
      return new Center(child: new Text("No Children"));
    }

    var theme = Theme.of(context);
    var colorScheme = theme.colorScheme;

    var contents = new Container(
      decoration: new BoxDecoration(
        border: Border.all(
          color: theme.primaryColorLight
        )
      ),
      margin: EdgeInsets.only(top: ThemeUtils.margin, left: ThemeUtils.margin),
      child: new LayoutBuilder(builder: (context2, constraints) => {
        var maxWidth = constraints.maxWidth;
        var maxHeight = constraints.maxHeight;

        float maxSizeAvailable(Axis axis) {
          return axis == Axis.horizontal ? maxWidth : maxHeight;
        }

        var childrenAndMainAxisSpacesRenderProps =
            widget.properties.childrenRenderProperties(
          smallestRenderWidth: ThemeUtils.minRenderWidth,
          largestRenderWidth: ThemeUtils.defaultMaxRenderWidth,
          smallestRenderHeight: ThemeUtils.minRenderHeight,
          largestRenderHeight: ThemeUtils.defaultMaxRenderHeight,
          maxSizeAvailable: maxSizeAvailable
        );

        List<RenderProperties> renderProperties = new List<RenderProperties>();
        List<RenderProperties> mainAxisSpaces = new List<RenderProperties>();
        foreach (var prop in childrenAndMainAxisSpacesRenderProps)
        {
          if (!prop.isFreeSpace)
          {
            renderProperties.Add(prop);
          }
          else
          {
            mainAxisSpaces.Add(prop);
          }
        }
        
        var crossAxisSpaces = widget.properties.crossAxisSpaces(
          childrenRenderProperties: renderProperties,
          maxSizeAvailable: maxSizeAvailable
        );

        var childrenRenderWidgets = new List<Widget>();
        for (var i = 0; i < widget.children.Count; i++) {
          var child = widget.children[i];
          var isSelected = widget.highlighted == child;

          childrenRenderWidgets.Add( new FlexChildVisualizer(
            key: isSelected ? selectedChildKey : null,
            state: widget.state,
            layoutProperties: child,
            isSelected: isSelected,
            renderProperties: renderProperties[i]
          ));
        }

        List<Widget> freeSpacesWidgets = new List<Widget>();
        var propertiesList = new List<RenderProperties>(mainAxisSpaces.Union(crossAxisSpaces));
        foreach (var property in propertiesList)
        {
          freeSpacesWidgets.Add(new FreeSpaceVisualizerWidget(property));
        }

        List<Widget> widgets = new List<Widget>();
        widgets.Add(new LayoutExplorerBackground(colorScheme: colorScheme)); 
        widgets = widgets.Union(freeSpacesWidgets).Union(childrenRenderWidgets).ToList();

        float sum_width = 0;
        float sum_height = 0;
        foreach (var prop in childrenAndMainAxisSpacesRenderProps)
        {
          sum_width += prop.width;
          sum_height += prop.height;
        }
        
        return new Scrollbar(
          isAlwaysShown: true,
          controller: widget.scrollController,
          child: new SingleChildScrollView(
            scrollDirection: widget.properties.direction.Value,
            controller: widget.scrollController,
            child: new ConstrainedBox(
              constraints: new BoxConstraints(
                minWidth: maxWidth,
                minHeight: maxHeight,
                maxWidth: widget.direction == Axis.horizontal
                    ? sum_width
                    : maxWidth,
                maxHeight: widget.direction == Axis.vertical
                    ? sum_height
                    : maxHeight
              ).normalize(),
              child: new Stack(
                children: widgets
              )
            )
          )
        );
      })
    );
    return new VisualizeWidthAndHeightWithConstraints(
      child: contents,
      properties: widget.properties
    );
  }
}

/// Widget that represents and visualize a direct child of Flex widget.
public class FlexChildVisualizer : StatelessWidget {
  
  public readonly int maximumFlexFactorOptions = 5;
  public FlexChildVisualizer(
    Key key = null,
    _FlexLayoutExplorerWidgetState state = null,
    LayoutProperties layoutProperties = null,
    RenderProperties renderProperties = null,
    bool? isSelected = null
  ) : base(key: key)
  {
    this.state = state;
    this.layoutProperties = layoutProperties;
    this.renderProperties = renderProperties;
    this.isSelected = isSelected;
  }

  public readonly _FlexLayoutExplorerWidgetState state;

  public readonly bool? isSelected;

  public readonly LayoutProperties layoutProperties;

  public readonly RenderProperties renderProperties;

  FlexLayoutProperties root
  {
    get
    {
      return state.properties;
    }
  }

  LayoutProperties properties
  {
    get
    {
      return renderProperties.layoutProperties;
    }
  }

  void onChangeFlexFactor(int newFlexFactor) {
    // var node = properties.node;
    // var inspectorService = await node.inspectorService;
    // state.markAsDirty();
    // await inspectorService.invokeSetFlexFactor(
    //   node.valueRef,
    //   newFlexFactor
    // );
  }

  void onChangeFlexFit(FlexFit newFlexFit) {
    // var node = properties.node;
    // var inspectorService = node.inspectorService;
    // state.markAsDirty();
    // inspectorService.invokeSetFlexFit(
    //   node.valueRef,
    //   newFlexFit
    // );
  }

  Widget _buildFlexFactorChangerDropdown(int maximumFlexFactor) {
    Widget buildMenuitemChild(int flexFactor) {
      return new Text(
        $"flex: {flexFactor}",
        style: flexFactor == properties.flexFactor
            ? new TextStyle(
                fontWeight: FontWeight.bold,
                color: ThemeUtils.emphasizedTextColor
              )
            : new TextStyle(color: ThemeUtils.emphasizedTextColor)
      );
    }

    DropdownMenuItem<int> buildMenuItem(int flexFactor) {
      return new DropdownMenuItem<int>(
        value: flexFactor,
        child: buildMenuitemChild(flexFactor)
      );
    }

    List<DropdownMenuItem<int>> items = new List<DropdownMenuItem<int>>();
    items.Add(buildMenuItem(default)); // May has porblems
    for (var i = 0; i <= maximumFlexFactor; ++i)
    {
      items.Add(buildMenuItem(i));
    }
    return new DropdownButton<int>(
      value: (int)properties.flexFactor?.clamp(0, maximumFlexFactor),
      onChanged: onChangeFlexFactor,
      iconEnabledColor: ThemeUtils.textColor,
      underline: ThemeUtils.buildUnderline(),
      items: items
    );
  }

  Widget _buildFlexFitChangerDropdown() {
    Widget flexFitDescription(FlexFit flexFit)
    {
      return new Text(
        $"fit: {flexFit.ToString()}",
        style: new TextStyle(color: ThemeUtils.emphasizedTextColor)
        );
    }
    

    // Disable FlexFit changer if widget is Expanded.
    if (properties.description == "Expanded") {
      return flexFitDescription(FlexFit.tight);
    }

    DropdownMenuItem<FlexFit> buildMenuItem(FlexFit flexFit) {
      return new DropdownMenuItem<FlexFit>(
        value: flexFit,
        child: flexFitDescription(flexFit)
      );
    }

    List<DropdownMenuItem<FlexFit>> items = new List<DropdownMenuItem<FlexFit>>();
    items.Add(buildMenuItem(FlexFit.loose));
    if (properties.description != "Expanded") items.Add(buildMenuItem(FlexFit.tight));
    
    return new DropdownButton<FlexFit>(
      value: properties.flexFit.Value,
      onChanged: onChangeFlexFit,
      underline: ThemeUtils.buildUnderline(),
      iconEnabledColor: ThemeUtils.emphasizedTextColor,
      items: items
    );
  }

  Widget _buildContent(ColorScheme colorScheme)
  {
    List<Widget> widgets = new List<Widget>();
    
    widgets.Add(new Flexible(
      child: _buildFlexFactorChangerDropdown(maximumFlexFactorOptions)
    ));
    if (!properties.hasFlexFactor)
    {
      widgets.Add(new Text(
        root.isMainAxisHorizontal ?"unconstrained horizontal" : "unconstrained vertical",
        style: new TextStyle(
          color: ThemeUtils.unconstrainedColor,
          fontStyle: FontStyle.italic
        ),
        maxLines: 2,
        softWrap: true,
        overflow: TextOverflow.ellipsis,
        textScaleFactor: ThemeUtils.smallTextScaleFactor,
        textAlign: TextAlign.center
      ));
    }
    widgets.Add(_buildFlexFitChangerDropdown());
    
    return new Container(
      margin: EdgeInsets.only(
        top: ThemeUtils.margin,
        left: ThemeUtils.margin
      ),
      child: new Column(
        crossAxisAlignment: CrossAxisAlignment.end,
        children: widgets
      )
    );
  }
  
  public override Widget build(BuildContext context) {
    var renderSize = renderProperties.size;
    var renderOffset = renderProperties.offset;

    Widget buildEntranceAnimation(BuildContext context2, Widget child) {
      var vertical = root.isMainAxisVertical;
      var horizontal = root.isMainAxisHorizontal;
      Size size = renderSize;
      if (properties.hasFlexFactor) {
        size = new SizeTween(
          begin: new Size(
            horizontal ? ThemeUtils.minRenderWidth - ThemeUtils.entranceMargin : renderSize.width,
            vertical ? ThemeUtils.minRenderHeight - ThemeUtils.entranceMargin : renderSize.height
          ),
          end: renderSize
        ).evaluate(state.entranceCurve);
      }
      // Not-expanded widgets enter much faster.
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

    var colorScheme = Theme.of(context).colorScheme;

    return new Positioned(
      top: renderOffset.dy,
      left: renderOffset.dx,
      child: new InkWell(
        onTap: () => state.onTap(properties),
        onDoubleTap: () => state.onDoubleTap(properties),
        onLongPress: () => state.onDoubleTap(properties),
        child: new SizedBox(
          width: renderSize.width,
          height: renderSize.height,
          child: new AnimatedBuilder(
            animation: state.entranceController,
            builder: buildEntranceAnimation,
            child: new WidgetVisualizer(
              isSelected: isSelected.Value,
              layoutProperties: layoutProperties,
              title: properties.description,
              overflowSide: properties.overflowSide,
              child: new VisualizeWidthAndHeightWithConstraints(
                arrowHeadSize: ThemeUtils.arrowHeadSize,
                child: new Align(
                  alignment: Alignment.topRight,
                  child: _buildContent(colorScheme)
                ),
                properties: properties
              )
            )
          )
        )
      )
    );
  }

}

}*/