using System;
using System.Collections.Generic;
using Unity.UIWidgets.animation;
using Unity.UIWidgets.DevTools.inspector.layout_explorer.ui;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.rendering;
using Unity.UIWidgets.ui;

namespace Unity.UIWidgets.DevTools.inspector.layout_explorer.flex
{
  public class FlexUtils
  {
    public static string crossAxisAssetImageUrl(Axis direction, CrossAxisAlignment alignment)
    {
      return "assets/img/layout_explorer/cross_axis_alignment/" +
             $"{flexType(direction)}_{alignment.ToString()}.png";
    }

    public static string mainAxisAssetImageUrl(Axis direction, MainAxisAlignment alignment)
    {
      return "assets/img/layout_explorer/main_axis_alignment/" +
             $"{flexType(direction)}_{alignment.ToString()}.png";
    }
    
    public static string flexType(Axis direction)
    {
      switch (direction)
      {
        case Axis.horizontal:
          return "row";
        case Axis.vertical:
        default:
          return "column";
      }
    }
    
  }


  public delegate float MaxSizeAvailable(Axis axis);
  
  public class AnimatedFlexLayoutProperties
    : AnimatedLayoutProperties<FlexLayoutProperties>//, FlexLayoutProperties
  {
    
    public AnimatedFlexLayoutProperties(FlexLayoutProperties begin,
      FlexLayoutProperties end, Animation<float> animation)
      : base(begin, end, animation) { }

    public new CrossAxisAlignment? crossAxisAlignment
    {
      get { return end.crossAxisAlignment; }
    }

    public new MainAxisAlignment? mainAxisAlignment
    {
      get { return end.mainAxisAlignment; }
    }

  
    

    public List<RenderProperties> childrenRenderProperties( // public override List<RenderProperties> childrenRenderProperties
      float smallestRenderWidth,
      float largestRenderWidth,
      float smallestRenderHeight,
      float largestRenderHeight,
      MaxSizeAvailable maxSizeAvailable
    )
    {
      
      var beginRenderProperties = begin.childrenRenderProperties(
        smallestRenderHeight: smallestRenderHeight,
        smallestRenderWidth: smallestRenderWidth,
        largestRenderHeight: largestRenderHeight,
        largestRenderWidth: largestRenderWidth,
        maxSizeAvailable: maxSizeAvailable
      );
      var endRenderProperties = end.childrenRenderProperties(
        smallestRenderHeight: smallestRenderHeight,
        smallestRenderWidth: smallestRenderWidth,
        largestRenderHeight: largestRenderHeight,
        largestRenderWidth: largestRenderWidth,
        maxSizeAvailable: maxSizeAvailable
      );
      var result = new List<RenderProperties>();
      for (var i = 0; i < children?.Count; i++)
      {
        var beginProps = beginRenderProperties[i];
        var endProps = endRenderProperties[i];
        var t = animation.value;
        result.Add(
          new RenderProperties(
            axis: endProps.axis,
            offset: Offset.lerp(beginProps.offset, endProps.offset, t),
            size: Size.lerp(beginProps.size, endProps.size, t),
            realSize: Size.lerp(beginProps.realSize, endProps.realSize, t),
            layoutProperties: new AnimatedLayoutProperties<LayoutProperties>(
              beginProps.layoutProperties,
              endProps.layoutProperties,
              animation
            )
          )
        );
      }

      // Add in the free space from the end.
      // TODO(djshuckerow): We should make free space a part of
      // RenderProperties so that we can animate between those.
      foreach (var property in endRenderProperties)
      {
        if (property.isFreeSpace)
        {
          result.Add(property);
        }

      }

      return result;
    }


    public new float? crossAxisDimension
    {
      get
      {
        return utils.lerpFloat(
          begin.crossAxisDimension,
          end.crossAxisDimension,
          animation.value
        );
      }
    }

    public new Axis crossAxisDirection
    {
      get { return end.crossAxisDirection; }
    }


    public List<RenderProperties> crossAxisSpaces( // public override List<RenderProperties> crossAxisSpaces
      List<RenderProperties> childrenRenderProperties,
      MaxSizeAvailable maxSizeAvailable
      ) {
      return end.crossAxisSpaces(
        childrenRenderProperties: childrenRenderProperties,
        maxSizeAvailable: maxSizeAvailable
      );
    }

    public new Axis? direction
    {
      get { return end.direction; }
    }

    public new string horizontalDirectionDescription
    {
      get { return end.horizontalDirectionDescription; }
    }

    public new bool isMainAxisHorizontal
    {
      get { return end.isMainAxisHorizontal; }
    }


    public new bool isMainAxisVertical
    {
      get { return end.isMainAxisVertical; }
    }

    public new float? mainAxisDimension
    {
      get
      {
        return utils.lerpFloat(
          begin.mainAxisDimension,
          end.mainAxisDimension,
          animation.value
        );
      }
    }

    public new MainAxisSize? mainAxisSize
    {
      get { return end.mainAxisSize; }
    }

    public new TextBaseline? textBaseline
    {
      get { return end.textBaseline; }
    }

    public new TextDirection? textDirection
    {
      get { return end.textDirection; }
    }


    public new float? totalFlex
    {
      get { return utils.lerpFloat(begin.totalFlex, end.totalFlex, animation.value); }
    }


    public new string type
    {
      get { return end.type; }
    }

    public new VerticalDirection? verticalDirection
    {
      get { return end.verticalDirection; }
    }


    public new string verticalDirectionDescription
    {
      get { return end.verticalDirectionDescription; }
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
    )
    {
      return new FlexLayoutProperties(
        size: size ?? this.size,
        children: children ?? this.children,
        node: node,
        constraints: constraints ?? this.constraints,
        isFlex: isFlex ?? this.isFlex,
        description: description ?? this.description,
        flexFactor: flexFactor ?? this.flexFactor,
        direction: direction ?? this.direction,
        mainAxisAlignment: mainAxisAlignment ?? this.mainAxisAlignment,
        mainAxisSize: mainAxisSize ?? this.mainAxisSize,
        crossAxisAlignment: crossAxisAlignment ?? this.crossAxisAlignment,
        textDirection: textDirection ?? this.textDirection,
        verticalDirection: verticalDirection ?? this.verticalDirection,
        textBaseline: textBaseline ?? this.textBaseline
      );
    }

    public new bool startIsTopLeft
    {
      get { return end.startIsTopLeft; }
    }
  }

  
  
}

