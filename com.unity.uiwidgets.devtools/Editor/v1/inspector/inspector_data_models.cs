/*using System;
using System.Collections.Generic;
using System.Linq;
using Unity.UIWidgets.DevTools;
using Unity.UIWidgets.DevTools.inspector.layout_explorer.flex;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.rendering;
using Unity.UIWidgets.ui;
using UnityEngine;

namespace Unity.UIWidgets.DevTools.inspector
{
  public class InspectorDataModelsUtils
  {
      public const float overflowEpsilon = 0.1f;
      
      
      public static float sum(List<float?> list)
      {
        float sum_result = 0.0f;
        foreach (var res in list)
        {
          sum_result += (res?? 0.0f);
        }
        return sum_result;
      }
      
      
      List<float?> computeRenderSizes(
          List<float?> sizes,
          float? smallestSize,
          float? largestSize,
          float smallestRenderSize,
          float? largestRenderSize,
          float maxSizeAvailable,
        bool useMaxSizeAvailable = true
      ) {
        int n = sizes.Count;

        if (smallestSize == largestSize) {
          var renderSize = Mathf.Max(smallestRenderSize, maxSizeAvailable / n);
          //TODO might have error here
          List<float?> result = new List<float?>();
          foreach (var size in sizes)
          {
              result.Add(renderSize);
          }
          return result;
        }

        List<float?> transformToRenderSize(float? _largestRenderSize)
        {
          List<float?> renderSize = new List<float?>();
          foreach (var s in sizes)
          {
            renderSize.Add((s - smallestSize) *
                           (largestRenderSize - smallestRenderSize) /
                           (largestSize - smallestSize) +
                           smallestRenderSize);
          }

          return renderSize;
        }

        var renderSizes = transformToRenderSize(largestRenderSize);
        
        
        
      if (useMaxSizeAvailable && sum(renderSizes) < maxSizeAvailable)
      {
        List<float?> temp_list = new List<float?>();
        foreach (var s in sizes)
        {
          temp_list.Add(s-smallestSize);
        }
          largestRenderSize = (maxSizeAvailable - n * smallestRenderSize) *
                              (largestSize - smallestSize) /
                              sum(temp_list) +
            smallestRenderSize;
          renderSizes = transformToRenderSize(largestRenderSize);
        }
        return renderSizes;
      }
      
      //[!!!] precision no use
      bool _closeTo(float? a, float? b, int precision = 1) {
        return a.ToString() == b.ToString();
      }

      bool closeTo(Size a,Size b) {
        return _closeTo(a.width, b.width) && _closeTo(a.height, b.height);
      }
      
      bool closeTo(Offset a, Offset b) {
        return _closeTo(a.dx, b.dx) && _closeTo(a.dy, b.dy);
      }
      
      public readonly Expando<FlexLayoutProperties> _flexLayoutExpando = Expando();
      
  }
    




// TODO(albertusangga): Move this to [RemoteDiagnosticsNode] once dart:html app is removed
/// Represents parsed layout information for a specific [RemoteDiagnosticsNode].
public class LayoutProperties {
  
  public LayoutProperties(){}

  public LayoutProperties(RemoteDiagnosticsNode node, int copyLevel = 1)
  {
    description = node?.description;
    size = node?.size;
    constraints = node?.constraints;
    isFlex = node?.isFlex;
    flexFactor = node?.flexFactor;
    flexFit = node?.flexFit;
    if (copyLevel == 0)
    {
      children = new List<LayoutProperties>();
    }
    else
    {
      var children_temp = node?.childrenNow;
      var children_res = new List<LayoutProperties>();
      foreach (var child in children_temp)
      {
         children_res.Add(new LayoutProperties(child, copyLevel: copyLevel - 1));
      }

      children = children_res.ToList();
    }
    if (children?.isNotEmpty() ?? false) {
      foreach (var child in children) {
        child.parent = this;
      }
    }
  }

  public LayoutProperties values(
    RemoteDiagnosticsNode node,
    List<LayoutProperties> children,
    BoxConstraints constraints,
    string description,
    float? flexFactor,
    bool? isFlex,
    Size size,
    FlexFit? flexFit
  )
  {
    foreach (var child in children) {
      child.parent = this;
    }

    return null;
  }

  public LayoutProperties lerp(
    LayoutProperties begin,
    LayoutProperties end,
    float t
  ) {
    return LayoutProperties.values(
      node: end.node,
      children: end.children,
      constraints: BoxConstraints.lerp(begin.constraints, end.constraints, t),
      description: end.description,
      flexFactor: begin.flexFactor + (begin.flexFactor - end.flexFactor) * t,
      isFlex: begin.isFlex.Value && end.isFlex.Value,
      size: Size.lerp(begin.size, end.size, t),
      flexFit: end.flexFit
    );
  }

  public LayoutProperties parent;
  public readonly RemoteDiagnosticsNode node;
  public readonly List<LayoutProperties> children;
  public readonly BoxConstraints constraints;
  public readonly string description;
  public readonly float? flexFactor;
  public readonly FlexFit? flexFit;
  public readonly bool? isFlex;
  public readonly Size size;

  /// Represents the order of [children] to be displayed.
  public List<LayoutProperties> displayChildren {
    get
    {
      return children;
    }
  }

  public bool hasFlexFactor
  {
    get
    {
      return flexFactor != null && flexFactor > 0;
    }
  }

  public int totalChildren
  {
    get
    {
      return children?.Count ?? 0;
    }
  }

  public bool hasChildren
  {
    get
    {
      return children?.isNotEmpty() ?? false;
    }
  }

  public float? width
  {
    get
    {
      return size?.width;
    }
  }

  public float? height
  {
    get
    {
      return size?.height;
    }
  }

  public float? dimension(Axis? axis)
  {
    return axis == Axis.horizontal ? width : height;
  }

  public List<float?> childrenDimensions(Axis axis)
  {
    List<float?> res = new List<float?>();
    
    foreach (var child in displayChildren)
    {
      res.Add(child.dimension(axis));
    }
    return res?.ToList();
  }

  public List<float?> childrenWidths
  {
    get
    {
      return childrenDimensions(Axis.horizontal);
    }
  }

  public List<float?> childrenHeights
  {
    get
    {
      return childrenDimensions(Axis.vertical);
    }
  }

  public string describeWidthConstraints() {
    if (constraints == null) return "";
    return constraints.hasBoundedWidth
        ? describeAxis(constraints.minWidth, constraints.maxWidth, "w")
        : "width is unconstrained";
  }

  public string describeHeightConstraints() {
    if (constraints == null) return "";
    return constraints.hasBoundedHeight
        ? describeAxis(constraints.minHeight, constraints.maxHeight, "h")
        : "height is unconstrained";
  }

  public string describeWidth()
  {
    return $"w={size.width :F}";
  }

  public string describeHeight()
  {
    return $"h={size.height :F}";
  }

  public bool isOverflowWidth {
    get
    {
      var parentWidth = parent?.width;
      if (parentWidth == null) return false;
      var parentData = node.parentData;
      float? widthUsed = width;
      if (parentData != null) {
        widthUsed += parentData.offset.dx;
      }
      // TODO(jacobr): certain widgets may allow overflow so this may false
      // positive a bit for cases like Stack.
      return widthUsed > parentWidth + InspectorDataModelsUtils.overflowEpsilon;
    }
    
  }

  public bool isOverflowHeight {
    get
    {
      var parentHeight = parent?.height;
      if (parentHeight == null) return false;
      var parentData = node.parentData;
      float? heightUsed = height;
      if (parentData != null) {
        heightUsed += parentData.offset.dy;
      }
      return heightUsed > parentHeight + InspectorDataModelsUtils.overflowEpsilon;
    }
    
  }

  public static string describeAxis(float? min, float? max, string axis) {
    if (min == max) return $"axis={min :F1}";
    return $"{min :F1}<={axis}<={max :F1}";
  }

  LayoutProperties copyWith(
    List<LayoutProperties> children,
    BoxConstraints constraints,
    String description,
    float? flexFactor,
    FlexFit? flexFit,
    bool? isFlex,
    Size size
  ) {
    return LayoutProperties.values(
      node: node,
      children: children ?? this.children,
      constraints: constraints ?? this.constraints,
      description: description ?? this.description,
      flexFactor: flexFactor ?? this.flexFactor,
      isFlex: isFlex ?? this.isFlex,
      size: size ?? this.size,
      flexFit: flexFit ?? this.flexFit
    );
  }
  
  // TODO(jacobr): is it possible to overflow on multiple sides?
  // TODO(jacobr): do we need to worry about overflowing on the left side in RTL
  // layouts? We need to audit the Flutter semantics for determining overflow to
  // make sure we are consistent.
  
  public OverflowSide? overflowSide {
    get
    {
      if (isOverflowWidth) return OverflowSide.right;
      if (isOverflowHeight) return OverflowSide.bottom;
      return null;
    }
    
  }
  
  public MainAxisAlignment GetMainAxisAlignment(MainAxisAlignment mainAxisAlignment) {
    switch (mainAxisAlignment) {
      case MainAxisAlignment.start:
        return MainAxisAlignment.end;
      case MainAxisAlignment.end:
        return MainAxisAlignment.start;
      default:
        return mainAxisAlignment;
    }
  }
  
}

public enum OverflowSide {
  right,
  bottom,
}



/// TODO(albertusangga): Move this to [RemoteDiagnosticsNode] once dart:html app is removed
public class FlexLayoutProperties : LayoutProperties {
  public FlexLayoutProperties(
    Size size = null,
    List<LayoutProperties> children = null,
    RemoteDiagnosticsNode node = null,
    BoxConstraints constraints = null,
    bool? isFlex = null,
    string description = null,
    float? flexFactor = null,
    FlexFit? flexFit = null,
    Axis? direction = null,
    MainAxisAlignment? mainAxisAlignment = null,
    CrossAxisAlignment? crossAxisAlignment = null,
    MainAxisSize? mainAxisSize = null,
    TextDirection? textDirection = null,
    VerticalDirection? verticalDirection = null,
    TextBaseline? textBaseline = null
  )
  {
    base.values(
      size: size,
      children: children,
      node: node,
      constraints: constraints,
      isFlex: isFlex,
      description: description,
      flexFactor: flexFactor,
      flexFit: flexFit
    );
    this.direction = direction;
    this.mainAxisAlignment = mainAxisAlignment;
    this.crossAxisAlignment = crossAxisAlignment;
    this.mainAxisSize = mainAxisSize;
    this.textDirection = textDirection;
    this.verticalDirection = verticalDirection;
    this.textBaseline = textBaseline;
  }

  public FlexLayoutProperties(
    RemoteDiagnosticsNode node = null, 
    Axis? direction = null,
    MainAxisAlignment? mainAxisAlignment = null,
    MainAxisSize? mainAxisSize = null,
    CrossAxisAlignment? crossAxisAlignment = null,
    TextDirection? textDirection = null,
    VerticalDirection? verticalDirection = null,
    TextBaseline? textBaseline = null
  ) : base(node)
  {
    
  }

  public static FlexLayoutProperties fromDiagnostics(RemoteDiagnosticsNode node) {
    if (node == null) return null;
    // Cache the properties on an expando so that local tweaks to
    // FlexLayoutProperties persist across multiple lookups from an
    // RemoteDiagnosticsNode.
    return _flexLayoutExpando[node] = _flexLayoutExpando[node] ?? _buildNode(node);
  }

  
  public new FlexLayoutProperties copyWith(
    Size size = null,
    List<LayoutProperties> children = null,
    BoxConstraints constraints = null,
    bool? isFlex = null,
    string description = null,
    float? flexFactor = null,
    FlexFit? flexFit = null,
    Axis? direction = null,
    MainAxisAlignment? mainAxisAlignment = null,
    MainAxisSize? mainAxisSize = null,
    CrossAxisAlignment? crossAxisAlignment = null,
    TextDirection? textDirection = null,
    VerticalDirection? verticalDirection = null,
    TextBaseline? textBaseline = null
  ) {
    return new FlexLayoutProperties(
      size: size ?? this.size,
      children: children.isEmpty() ? this.children : children,
      node: node,
      constraints: constraints ?? this.constraints,
      isFlex: isFlex ?? this.isFlex,
      description: description ?? this.description,
      flexFactor: flexFactor ?? this.flexFactor,
      flexFit: flexFit ?? this.flexFit,
      direction: direction ?? this.direction,
      mainAxisAlignment: mainAxisAlignment ?? this.mainAxisAlignment,
      mainAxisSize: mainAxisSize ?? this.mainAxisSize,
      crossAxisAlignment: crossAxisAlignment ?? this.crossAxisAlignment,
      textDirection: textDirection ?? this.textDirection,
      verticalDirection: verticalDirection ?? this.verticalDirection,
      textBaseline: textBaseline ?? this.textBaseline
    );
  }

  static FlexLayoutProperties _buildNode(RemoteDiagnosticsNode node) {
    Dictionary<string, object> renderObjectJson = node?.renderObject?.json;
    if (renderObjectJson == null) return null;
    List<object> properties = renderObjectJson["properties"];
    Dictionary<string, object> data = Dictionary<string, object>.fromIterable(
      properties,
      key: (property) => property["name"],
      value: (property) => property["description"]
    );
    return FlexLayoutProperties._fromNode(
      node,
      direction: _directionUtils.enumEntry(data["direction"]),
      mainAxisAlignment:
          _mainAxisAlignmentUtils.enumEntry(data["mainAxisAlignment"]),
      mainAxisSize: _mainAxisSizeUtils.enumEntry(data["mainAxisSize"]),
      crossAxisAlignment:
          _crossAxisAlignmentUtils.enumEntry(data["crossAxisAlignment"]),
      textDirection: _textDirectionUtils.enumEntry(data["textDirection"]),
      verticalDirection:
          _verticalDirectionUtils.enumEntry(data["verticalDirection"]),
      textBaseline: _textBaselineUtils.enumEntry(data["textBaseline"])
    );
  }

  public readonly Axis? direction;
  public readonly MainAxisAlignment? mainAxisAlignment;
  public readonly CrossAxisAlignment? crossAxisAlignment;
  public readonly MainAxisSize? mainAxisSize;
  public readonly TextDirection? textDirection;
  public readonly VerticalDirection? verticalDirection;
  public readonly TextBaseline? textBaseline;

  List<LayoutProperties> _displayChildren;

  
  public override List<LayoutProperties> displayChildren {
    get
    {
      if (_displayChildren != null) return _displayChildren;
      return _displayChildren =
        startIsTopLeft ? children : children.Reverse();
    }
    
  }

  int _totalFlex;

  public bool isMainAxisHorizontal
  {
    get
    {
      return direction == Axis.horizontal;
    }
  }

  public bool isMainAxisVertical
  {
    get
    {
      return direction == Axis.vertical;
    }
  }

  public string horizontalDirectionDescription {
    get
    {
      return direction == Axis.horizontal ? "Main Axis" : "Cross Axis";
    }
    
  }

  public string verticalDirectionDescription {
    get
    {
      return direction == Axis.vertical ? "Main Axis" : "Cross Axis";  
    }
    
  }

  public string type
  {
    get
    {
      return direction.flexType;
    }
  }

  public int totalFlex {
    get
    {
      if (children?.isEmpty() ?? true) return 0;
      _totalFlex = _totalFlex ?? children
        .map((child) => child.flexFactor ?? 0)
        .reduce((value, element) => value + element)
        .toInt();
      return _totalFlex;
    }
    
  }

  public Axis crossAxisDirection {
    get
    {
      return direction == Axis.horizontal ? Axis.vertical : Axis.horizontal;
    }
    
  }

  public float? mainAxisDimension
  {
    get
    {
      return dimension(direction);
    }
  }

  public float? crossAxisDimension
  {
    get
    {
      return dimension(crossAxisDirection);
    }
  }

  
  public new bool isOverflowWidth {
    get
    {
      if (direction == Axis.horizontal) {
        return width + overflowEpsilon < sum(childrenWidths);
      }
      return width + overflowEpsilon < max(childrenWidths);
    }
  }

  
  public new bool isOverflowHeight {
    get
    {
      if (direction == Axis.vertical) {
        return height + overflowEpsilon < sum(childrenHeights);
      }
      return height + overflowEpsilon < max(childrenHeights);  
    }
  }

  public bool startIsTopLeft {
    get
    {
      D.assert(direction != null);
      switch (direction) {
        case Axis.horizontal:
          switch (textDirection) {
            case TextDirection.ltr:
              return true;
            case TextDirection.rtl:
              return false;
          }
          break;
        case Axis.vertical:
          switch (verticalDirection) {
            case VerticalDirection.down:
              return true;
            case VerticalDirection.up:
              return false;
          }
          break;
      }
      return true;
    }
    
  }
  
  public List<RenderProperties> childrenRenderProperties(
    float? smallestRenderWidth,
    float? largestRenderWidth,
    float? smallestRenderHeight,
    float? largestRenderHeight,
    MaxSizeAvailable maxSizeAvailable
  ) {
    var freeSpace = dimension(direction) - sum(childrenDimensions(direction));
    var displayMainAxisAlignment =
        startIsTopLeft ? mainAxisAlignment : mainAxisAlignment.reversed;

    float? leadingSpace(float? leadingfreeSpace) {
      if (children.isEmpty()) return 0.0f;
      switch (displayMainAxisAlignment) {
        case MainAxisAlignment.start:
        case MainAxisAlignment.end:
          return leadingfreeSpace;
        case MainAxisAlignment.center:
          return leadingfreeSpace * 0.5f;
        case MainAxisAlignment.spaceBetween:
          return 0.0f;
        case MainAxisAlignment.spaceAround:
          var spaceBetweenChildren = leadingfreeSpace / children.Count;
          return spaceBetweenChildren * 0.5f;
        case MainAxisAlignment.spaceEvenly:
          return leadingfreeSpace / (children.Count + 1);
        default:
          return 0.0f;
      }
    }

    float? betweenSpace(float? betweenfreeSpace) {
      if (children.isEmpty()) return 0.0f;
      switch (displayMainAxisAlignment) {
        case MainAxisAlignment.start:
        case MainAxisAlignment.end:
        case MainAxisAlignment.center:
          return 0.0f;
        case MainAxisAlignment.spaceBetween:
          if (children.Count == 1) return betweenfreeSpace;
          return betweenfreeSpace / (children.Count - 1);
        case MainAxisAlignment.spaceAround:
          return betweenfreeSpace / children.Count;
        case MainAxisAlignment.spaceEvenly:
          return betweenfreeSpace / (children.Count + 1);
        default:
          return 0.0f;
      }
    }

    float? smallestRenderSize(Axis axis) {
      return axis == Axis.horizontal
          ? smallestRenderWidth
          : smallestRenderHeight;
    }

    float? largestRenderSize(Axis axis) {
      var lrs =
          axis == Axis.horizontal ? largestRenderWidth : largestRenderHeight;
      // use all the space when visualizing cross axis
      return (axis == direction) ? lrs : maxSizeAvailable(axis);
    }

    List<float?> renderSizes(Axis axis) {
      var sizes = childrenDimensions(axis);
      if (freeSpace > 0.0 && axis == direction) {
        /// include free space in the computation
        sizes.Add(freeSpace);
      }
      var smallestSize = min(sizes);
      var largestSize = max(sizes);
      if (axis == direction ||
          (crossAxisAlignment != CrossAxisAlignment.stretch &&
              smallestSize != largestSize)) {
        return computeRenderSizes(
          sizes: sizes,
          smallestSize: smallestSize,
          largestSize: largestSize,
          smallestRenderSize: smallestRenderSize(axis),
          largestRenderSize: largestRenderSize(axis),
          maxSizeAvailable: maxSizeAvailable(axis)
        );
      } else {
        // uniform cross axis sizes.
        float? size = crossAxisAlignment == CrossAxisAlignment.stretch
            ? maxSizeAvailable(axis)
            : largestSize /
                Mathf.Max(dimension(axis).Value, 1.0f) *
                maxSizeAvailable(axis);
        size = Mathf.Max(size.Value, smallestRenderSize(axis).Value);
        return sizes.ToList();
      }
    }

    var widths = renderSizes(Axis.horizontal);
    var heights = renderSizes(Axis.vertical);

    var renderFreeSpace = freeSpace > 0.0f
        ? (isMainAxisHorizontal ? widths.Last() : heights.Last())
        : 0.0f;

    var renderLeadingSpace = leadingSpace(renderFreeSpace);
    var renderBetweenSpace = betweenSpace(renderFreeSpace);

    var childrenRenderProps = new List<RenderProperties>();

    float? lastMainAxisOffset() {
      if (childrenRenderProps.isEmpty()) return 0.0f;
      return childrenRenderProps.Last().mainAxisOffset;
    }

    float? lastMainAxisDimension() {
      if (childrenRenderProps.isEmpty()) return 0.0f;
      return childrenRenderProps.Last().mainAxisDimension;
    }

    float? space(int index) {
      if (index == 0) {
        if (displayMainAxisAlignment == MainAxisAlignment.start) return 0.0f;
        return renderLeadingSpace;
      }
      return renderBetweenSpace;
    }

    float? calculateMainAxisOffset(int i) {
      return lastMainAxisOffset() + lastMainAxisDimension() + space(i);
    }

    float? calculateCrossAxisOffset(int i) {
      var maxDimension = maxSizeAvailable(crossAxisDirection);
      var usedDimension =
          crossAxisDirection == Axis.horizontal ? widths[i] : heights[i];

      if (crossAxisAlignment == CrossAxisAlignment.start ||
          crossAxisAlignment == CrossAxisAlignment.stretch ||
          maxDimension == usedDimension) return 0.0f;
      var emptySpace = Mathf.Max(0.0f, maxDimension - usedDimension.Value);
      if (crossAxisAlignment == CrossAxisAlignment.end) return emptySpace;
      return emptySpace * 0.5f;
    }

    for (var i = 0; i < children.Count; ++i)
    {
      RenderProperties renderProperties = new RenderProperties(
        axis: direction,
        size: new Size(widths[i].Value, heights[i].Value),
        offset: Offset.zero,
        realSize: displayChildren[i].size
      );
      renderProperties.mainAxisOffset = calculateMainAxisOffset(i);
      renderProperties.crossAxisOffset = calculateCrossAxisOffset(i);
      renderProperties.layoutProperties = displayChildren[i];
      

      childrenRenderProps.Add(renderProperties);
    }

    var spaces = new List<RenderProperties>();
    var actualLeadingSpace = leadingSpace(freeSpace);
    var actualBetweenSpace = betweenSpace(freeSpace);
    var renderPropsWithFullCrossAxisDimension = new RenderProperties(axis: direction);
    renderPropsWithFullCrossAxisDimension.crossAxisDimension = maxSizeAvailable(crossAxisDirection);
    renderPropsWithFullCrossAxisDimension.crossAxisRealDimension = dimension(crossAxisDirection);
    renderPropsWithFullCrossAxisDimension.crossAxisOffset = 0.0f;
    renderPropsWithFullCrossAxisDimension.isFreeSpace = true;
    renderPropsWithFullCrossAxisDimension.layoutProperties = this;
    if (actualLeadingSpace > 0.0 &&
        displayMainAxisAlignment != MainAxisAlignment.start)
    {
      var renderProps = renderPropsWithFullCrossAxisDimension.clone();
      renderProps.mainAxisOffset = 0.0f;
      renderProps.mainAxisDimension = renderLeadingSpace;
      renderProps.mainAxisRealDimension = actualLeadingSpace;
      spaces.Add(renderProps);

    }
    if (actualBetweenSpace > 0.0) {
      for (var i = 0; i < childrenRenderProps.Count - 1; ++i) {
        var child = childrenRenderProps[i];
        var renderProps = renderPropsWithFullCrossAxisDimension.clone();
        renderProps.mainAxisDimension = renderBetweenSpace;
        renderProps.mainAxisRealDimension = actualBetweenSpace;
        renderProps.mainAxisOffset = child.mainAxisOffset + child.mainAxisDimension;
        spaces.Add(renderProps);
      }
    }
    if (actualLeadingSpace > 0.0 &&
        displayMainAxisAlignment != MainAxisAlignment.end)
    {
      var renderProps = renderPropsWithFullCrossAxisDimension.clone();
      renderProps.mainAxisOffset = childrenRenderProps.Last().mainAxisDimension +
                                   childrenRenderProps.Last().mainAxisOffset;
      renderProps.mainAxisDimension = renderLeadingSpace;
      renderProps.mainAxisRealDimension = actualLeadingSpace;
      spaces.Add(renderProps);
    }

    var res = new List<RenderProperties>(childrenRenderProps.Union(spaces));
    return res;
  }

  public List<RenderProperties> crossAxisSpaces(
    List<RenderProperties> childrenRenderProperties,
    MaxSizeAvailable maxSizeAvailable
  ) {
    if (crossAxisAlignment == CrossAxisAlignment.stretch) return new List<RenderProperties>();
    var spaces = new List<RenderProperties>();
    for (var i = 0; i < children.Count; ++i) {
      if (dimension(crossAxisDirection) ==
              displayChildren[i].dimension(crossAxisDirection) ||
          childrenRenderProperties[i].crossAxisDimension ==
              maxSizeAvailable(crossAxisDirection)) continue;

      var renderProperties = childrenRenderProperties[i];
      var space = renderProperties.clone();
      space.isFreeSpace = true;

      space.crossAxisRealDimension =
          crossAxisDimension - space.crossAxisRealDimension;
      space.crossAxisDimension =
          maxSizeAvailable(crossAxisDirection) - space.crossAxisDimension;
      if (space.crossAxisDimension <= 0.0) continue;
      if (crossAxisAlignment == CrossAxisAlignment.center) {
        space.crossAxisDimension *= 0.5f;
        space.crossAxisRealDimension *= 0.5f;
        var tempSpace = space.clone();
        tempSpace.crossAxisOffset = 0.0f;
        spaces.Add(tempSpace);
        var tempSpace2 = space.clone();
        tempSpace2.crossAxisOffset = renderProperties.crossAxisDimension +
                                     renderProperties.crossAxisOffset;
        spaces.Add(tempSpace2);
      } else {
        space.crossAxisOffset = crossAxisAlignment == CrossAxisAlignment.end
            ? 0
            : renderProperties.crossAxisDimension;
        spaces.Add(space);
      }
    }
    return spaces;
  }

  public static readonly Axis _directionUtils = EnumUtils<Axis>(Axis.values);
  public static readonly MainAxisAlignment _mainAxisAlignmentUtils =
      EnumUtils<MainAxisAlignment>(MainAxisAlignment.values);
  public static readonly MainAxisSize _mainAxisSizeUtils =
      EnumUtils<MainAxisSize>(MainAxisSize.values);
  public static readonly CrossAxisAlignment _crossAxisAlignmentUtils =
      EnumUtils<CrossAxisAlignment>(CrossAxisAlignment.values);
  public static readonly TextDirection _textDirectionUtils =
      EnumUtils<TextDirection>(TextDirection.values);
  public static readonly VerticalDirection _verticalDirectionUtils =
      EnumUtils<VerticalDirection>(VerticalDirection.values);
  public static readonly TextBaseline _textBaselineUtils =
      EnumUtils<TextBaseline>(TextBaseline.values);
}

/// RenderProperties contains information for rendering a [LayoutProperties] node
  public class RenderProperties {
  public RenderProperties(
    Axis? axis = null,
    Size size = null,
    Offset offset = null,
    Size realSize = null,
    LayoutProperties layoutProperties = null,
    bool isFreeSpace = false
  )
  {
    width = size?.width ?? 0.0f;
    height = size?.height ?? 0.0f;
    realWidth = realSize?.width ?? 0.0f;
    realHeight = realSize?.height ?? 0.0f;
    dx = offset?.dx ?? 0.0f;
    dy = offset?.dy ?? 0.0f;
    this.axis = axis;
  }

    public readonly Axis? axis;

  /// represents which node is rendered for this object.
  public LayoutProperties layoutProperties;

  float? dx, dy;
  public float width, height;
  public float realWidth, realHeight;

  public bool isFreeSpace;

  public Size size
  {
    get
    {
      return new Size(width, height);
    }
  }

  public Size realSize
  {
    get
    {
      return new Size(realWidth, realHeight);
    }
  }

  public Offset offset
  {
    get
    {
      return new Offset(dx, dy);
    }
  }

  public float? mainAxisDimension
  {
    get
    {
      return axis == Axis.horizontal ? width : height;
    }
    set 
    {
    if (axis == Axis.horizontal) {
      width = value;
    } else {
      height = value;
    }
  }
  }

  

  public float? crossAxisDimension
  {
    get
    {
      return axis == Axis.horizontal ? height : width;
    }
    set 
    {
      if (axis == Axis.horizontal) {
        height = value;
      } else {
        width = value;
      }
    }
  }

  

  public float? mainAxisOffset
  {
      get
      {
        return axis == Axis.horizontal ? dx : dy;
      }
      set 
      {
        if (axis == Axis.horizontal) {
          dx = value;
        } else {
          dy = value;
        }
      }
  }

  

  public float? crossAxisOffset
  {
    get
    {
      return axis == Axis.horizontal ? dy : dx;
    }
    set
    {
      if (axis == Axis.horizontal) {
        dy = value;
      } else {
        dx = value;
      }
    }
  }

  

  public float? mainAxisRealDimension
  {
    get
    {
      return axis == Axis.horizontal ? realWidth : realHeight;
    }
    set 
    {
      if (axis == Axis.horizontal) {
        realWidth = value;
      } else {
        realHeight = value;
      }
    }
  }

  

  public float? crossAxisRealDimension
  {
    get
    {
      return axis == Axis.horizontal ? realHeight : realWidth;
    }
    set 
    {
      if (axis == Axis.horizontal) {
        realHeight = value;
      } else {
        realWidth = value;
      }
    }
  }

  

  public RenderProperties clone() {
    return new RenderProperties(
      axis: axis,
      size: size,
      offset: offset,
      realSize: realSize,
      layoutProperties: layoutProperties,
      isFreeSpace: isFreeSpace
    );
  }

  
  public override int hashCode
  {
    get
    {
      return axis.GetHashCode() ^
             size.GetHashCode() ^
             offset.GetHashCode() ^
             realSize.GetHashCode() ^
             isFreeSpace.GetHashCode();
    }
  }
  
  public override bool operator ==(Object other) {
    return other is RenderProperties &&
        axis == other.axis &&
        size.closeTo(other.size) &&
        offset.closeTo(other.offset) &&
        realSize.closeTo(other.realSize) &&
        isFreeSpace == other.isFreeSpace;
  }

  
  public override string ToString() {
    return $"[ axis: {axis}, size: {size}, offset: {offset}, realSize: {realSize}, isFreeSpace: {isFreeSpace} ]";
  }
}


}*/