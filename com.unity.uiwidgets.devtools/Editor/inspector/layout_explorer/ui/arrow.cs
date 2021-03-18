/*using System.Collections.Generic;
using uiwidgets;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.ui;
using Unity.UIWidgets.widgets;
using UnityEngine;
using Canvas = Unity.UIWidgets.ui.Canvas;
using Color = Unity.UIWidgets.ui.Color;

namespace Unity.UIWidgets.DevTools.inspector.layout_explorer.ui
{
  
  public class ArrowUtils{
    public static Color defaultArrowColor = Colors.white;
    public static float defaultArrowStrokeWidth = 2.0f;
    public static float defaultDistanceToArrow = 4.0f;
    
    public static Axis axis(ArrowType? type)
    {
      return (type == ArrowType.up || type == ArrowType.down)
        ? Axis.vertical
        : Axis.horizontal;
    }
  }
    

  public enum ArrowType {
    up,
    left,
    right,
    down,
  }

  
public class ArrowWrapper : StatelessWidget {

  public  ArrowWrapper(
    Key key = null, 
    Widget child = null,
    ArrowType? type = null,
    Color arrowColor = null,
    float? arrowHeadSize = null,
    float? arrowStrokeWidth = null,
    float? childMarginFromArrow = null
  )
  {
    D.assert(type != null);
    D.assert(arrowHeadSize != null && arrowHeadSize > 0.0);
    D.assert(arrowStrokeWidth != null && arrowHeadSize > 0.0);
    D.assert(childMarginFromArrow != null && childMarginFromArrow > 0.0);
    direction = ArrowUtils.axis(type);
    isBidirectional = false;
    startArrowType = type;
    endArrowType = type;
    this.child = child;
   
    this.arrowColor = arrowColor?? ArrowUtils.defaultArrowColor;
    this.arrowHeadSize = arrowHeadSize?? CommonThemeUtils.defaultIconSize;
    this.arrowStrokeWidth = arrowStrokeWidth ?? ArrowUtils.defaultArrowStrokeWidth;
    this.childMarginFromArrow = childMarginFromArrow ?? ArrowUtils.defaultDistanceToArrow;
    
  }

  public ArrowWrapper(
    Key key = null,
    Widget child = null,
    Axis? direction = null,
    Color arrowColor = null,
    float? arrowHeadSize = null,
    float? arrowStrokeWidth = null,
    float? childMarginFromArrow = null
  )
  {
    D.assert(direction != null);
    D.assert(arrowColor != null);
    D.assert(arrowHeadSize != null && arrowHeadSize >= 0.0);
    D.assert(arrowStrokeWidth != null && arrowHeadSize >= 0.0);
    D.assert(childMarginFromArrow != null && childMarginFromArrow >= 0.0);
    isBidirectional = true;
    startArrowType =
      direction == Axis.horizontal ? ArrowType.left : ArrowType.up;
    endArrowType =
      direction == Axis.horizontal ? ArrowType.right : ArrowType.down;
    this.child = child;
    this.direction = direction;
    this.arrowColor = arrowColor?? ArrowUtils.defaultArrowColor;
    this.arrowHeadSize = arrowHeadSize?? CommonThemeUtils.defaultIconSize;
    this.arrowStrokeWidth = arrowStrokeWidth ?? ArrowUtils.defaultArrowStrokeWidth;
    this.childMarginFromArrow = childMarginFromArrow ?? ArrowUtils.defaultDistanceToArrow;

  }

  public readonly Color arrowColor;
  public readonly float? arrowHeadSize;
  public readonly float? arrowStrokeWidth;
  public readonly Widget child;
  public readonly float childMarginFromArrow;
  
  public readonly Axis? direction;
  public readonly bool isBidirectional;
  public readonly ArrowType? startArrowType;
  public readonly ArrowType? endArrowType;

  public float verticalMarginFromArrow {
    get
    {
      if (child == null || direction == Axis.horizontal) return 0.0f;
      return childMarginFromArrow;
    }
   
  }

  public float horizontalMarginFromArrow {
    get
    {
      if (child == null || direction == Axis.vertical) return 0.0f;
      return childMarginFromArrow;
    }
    
  }
  
  public override Widget build(BuildContext context)
  {

    List<Widget> widgets = new List<Widget>();
    widgets.Add(new Expanded(
      child: new Container(
        margin: EdgeInsets.only(
          bottom: verticalMarginFromArrow,
          right: horizontalMarginFromArrow
        ),
        child: new ArrowWidget(
          color: arrowColor,
          headSize: arrowHeadSize,
          strokeWidth: arrowStrokeWidth,
          type: startArrowType,
          shouldDrawHead: isBidirectional
            ? true
            : (startArrowType == ArrowType.left ||
               startArrowType == ArrowType.up)
        )
      )
    ));
    if(child!= null)widgets.Add(child);
    widgets.Add(new Expanded(
      child: new Container(
        margin: EdgeInsets.only(
          top: verticalMarginFromArrow,
          left: horizontalMarginFromArrow
        ),
        child: new ArrowWidget(
          color: arrowColor,
          headSize: arrowHeadSize,
          strokeWidth: arrowStrokeWidth,
          type: endArrowType,
          shouldDrawHead: isBidirectional
            ? true
            : (endArrowType == ArrowType.right ||
               endArrowType == ArrowType.down)
        )
      )
    ));
    
    return new Flex(
      direction: direction.Value,
      children: widgets
    );
  }
}


public class ArrowWidget : StatelessWidget {
  public ArrowWidget(
    Color color = null,
    float? headSize = null,
    Key key = null,
    bool shouldDrawHead = true,
    float? strokeWidth = null,
    ArrowType? type = null
  ) : base(key: key)
  {
    this.headSize = headSize ?? CommonThemeUtils.defaultIconSize;
    this.strokeWidth = strokeWidth ?? ArrowUtils.defaultArrowStrokeWidth;
    D.assert(headSize != null && headSize > 0.0);
    D.assert(strokeWidth != null && strokeWidth > 0.0);
    D.assert(type != null);
    this.color = color ?? ArrowUtils.defaultArrowColor;
    this.shouldDrawHead = shouldDrawHead;
    this.type = type;
    
    _painter = new _ArrowPainter(
      headSize: headSize,
      color: color,
      strokeWidth: strokeWidth,
      type: type,
      shouldDrawHead: shouldDrawHead
    );
  }

  public readonly Color color;

  /// The arrow head is a Equilateral triangle
  public readonly float? headSize;

  public readonly float? strokeWidth;

  public readonly ArrowType? type;

  public readonly CustomPainter _painter;

  public readonly bool shouldDrawHead;
  
  public override Widget build(BuildContext context) {
    return new CustomPaint(
      painter: _painter,
      child: new Container()
    );
  }
}

public class _ArrowPainter : CustomPainter {
  public _ArrowPainter(
    float? headSize = null,
    float? strokeWidth = null,
    Color color = null,
    bool shouldDrawHead = true,
    ArrowType? type = null
  )
  {
    D.assert(headSize != null);
    D.assert(color != null);
    D.assert(strokeWidth != null);
    D.assert(type != null);
    D.assert(shouldDrawHead != null);
    // the height of an equilateral triangle
    headHeight = 0.5f * Mathf.Sqrt(3) * headSize.Value;
    this.headSize = headSize?? CommonThemeUtils.defaultIconSize;
    this.strokeWidth = strokeWidth?? ArrowUtils.defaultArrowStrokeWidth;
    this.color = color?? ArrowUtils.defaultArrowColor;
    this.shouldDrawHead = shouldDrawHead;
    this.type = type;
  }

  public readonly Color color;
  public readonly float headSize;
  public readonly bool shouldDrawHead;
  public readonly float strokeWidth;
  public readonly ArrowType? type;

  public readonly float headHeight;

  bool headIsGreaterThanConstraint(Size size) {
    if (type == ArrowType.left || type == ArrowType.right) {
      return headHeight >= (size.width);
    }
    return headHeight >= (size.height);
  }
  
  public bool shouldRepaint(CustomPainter oldDelegate)
  {
    return !(oldDelegate is _ArrowPainter &&
             headSize == ((_ArrowPainter)oldDelegate).headSize &&
             strokeWidth == ((_ArrowPainter)oldDelegate).strokeWidth &&
             color == ((_ArrowPainter)oldDelegate).color &&
             type == ((_ArrowPainter)oldDelegate).type);
  }

  public void paint(Canvas canvas, Size size)
  {
    Paint paint = new Paint();
    paint.color = color;
    paint.strokeWidth = strokeWidth;

    var originX = size.width / 2;
    var originY = size.height / 2;
    Offset lineStartingPoint = Offset.zero;
    Offset lineEndingPoint = Offset.zero;

    if (!headIsGreaterThanConstraint(size) && shouldDrawHead) {
      Offset p1 = null, p2 = null, p3 = null;
      var headSizeDividedBy2 = headSize / 2;
      switch (type) {
        case ArrowType.up:
          p1 = new Offset(originX, 0);
          p2 = new Offset(originX - headSizeDividedBy2, headHeight);
          p3 = new Offset(originX + headSizeDividedBy2, headHeight);
          break;
        case ArrowType.left:
          p1 = new Offset(0, originY);
          p2 = new Offset(headHeight, originY - headSizeDividedBy2);
          p3 = new Offset(headHeight, originY + headSizeDividedBy2);
          break;
        case ArrowType.right:
          var startingX = size.width - headHeight;
          p1 = new Offset(size.width, originY);
          p2 = new Offset(startingX, originY - headSizeDividedBy2);
          p3 = new Offset(startingX, originY + headSizeDividedBy2);
          break;
        case ArrowType.down:
          var startingY = size.height - headHeight;
          p1 = new Offset(originX, size.height);
          p2 = new Offset(originX - headSizeDividedBy2, startingY);
          p3 = new Offset(originX + headSizeDividedBy2, startingY);
          break;
      }

      Path path = new Path();

      path.moveTo(p1.dx, p1.dy);
      path.lineTo(p2.dx, p2.dy);
      path.lineTo(p3.dx, p3.dy);
      path.close();
      canvas.drawPath(path, paint);

      switch (type) {
        case ArrowType.up:
          lineStartingPoint = new Offset(originX, headHeight);
          lineEndingPoint = new Offset(originX, size.height);
          break;
        case ArrowType.left:
          lineStartingPoint = new Offset(headHeight, originY);
          lineEndingPoint = new Offset(size.width, originY);
          break;
        case ArrowType.right:
          var arrowHeadStartingX = size.width - headHeight;
          lineStartingPoint = new Offset(0, originY);
          lineEndingPoint = new Offset(arrowHeadStartingX, originY);
          break;
        case ArrowType.down:
          var headStartingY = size.height - headHeight;
          lineStartingPoint = new Offset(originX, 0);
          lineEndingPoint = new Offset(originX, headStartingY);
          break;
      }
    } else {
      // draw full line
      switch (type) {
        case ArrowType.up:
        case ArrowType.down:
          lineStartingPoint = new Offset(originX, 0);
          lineEndingPoint = new Offset(originX, size.height);
          break;
        case ArrowType.left:
        case ArrowType.right:
          lineStartingPoint = new Offset(0, originY);
          lineEndingPoint = new Offset(size.width, originY);
          break;
      }
    }
    canvas.drawLine(
      lineStartingPoint,
      lineEndingPoint,
      paint
    );
  }

  public bool? hitTest(Offset position)
  {
    throw new System.NotImplementedException();
  }
  
  public void addListener(VoidCallback listener)
  {
    throw new System.NotImplementedException();
  }

  public void removeListener(VoidCallback listener)
  {
    throw new System.NotImplementedException();
  }
}

}*/