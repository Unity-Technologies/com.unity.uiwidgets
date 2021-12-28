using System.Collections.Generic;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.rendering;
using Unity.UIWidgets.service;
using Unity.UIWidgets.ui;
using Unity.UIWidgets.widgets;
using UnityEngine;
using Canvas = Unity.UIWidgets.ui.Canvas;
using Color = Unity.UIWidgets.ui.Color;
using Rect = Unity.UIWidgets.ui.Rect;
using TextStyle = Unity.UIWidgets.painting.TextStyle;
using Transform = Unity.UIWidgets.widgets.Transform;

namespace Unity.UIWidgets.cupertino {
    public static class CupertinoTextSelectionUtils {
       // public static readonly TextSelectionControls cupertinoTextSelectionControls = new _CupertinoTextSelectionControls();

        public const float _kSelectionHandleOverlap = 1.5f;

        public const float _kSelectionHandleRadius = 6;

        public const float _kArrowScreenPadding = 26.0f;


        public const float _kToolbarContentDistance = 8.0f;
        
        public const float _kHandlesPadding = 18.0f;

        public const float _kToolbarScreenPadding = 8.0f;

        public const float _kToolbarHeight = 43.0f;

        public static readonly Size _kToolbarArrowSize = new Size(14.0f, 7.0f);
        public static readonly Radius _kToolbarBorderRadius = Radius.circular(8);
       

        public static readonly Color _kToolbarBackgroundColor = new Color(0xEB202020);
        public static readonly Color _kToolbarDividerColor = new Color(0xFF808080);

        public static readonly Color _kHandlesColor = new Color(0xFF136FE0);

        public static readonly Size _kSelectionOffset = new Size(20.0f, 30.0f);

        public static readonly Size _kToolbarTriangleSize = new Size(18.0f, 9.0f);

        public static readonly EdgeInsets _kToolbarButtonPadding =
            EdgeInsets.symmetric(vertical: 10.0f, horizontal: 18.0f);


        public static readonly TextStyle _kToolbarButtonFontStyle = new TextStyle(
            inherit: false,
            fontSize: 14.0f,
            letterSpacing: -0.15f,
            fontWeight: FontWeight.w400,
            color: CupertinoColors.white
        );
    }
    class CupertinoTextSelectionToolbar : SingleChildRenderObjectWidget {
        public CupertinoTextSelectionToolbar(
        Key key = null,
        float barTopY = 0.0f,
        float arrowTipX = 0.0f,
        bool isArrowPointingDown = false, 
            Widget child = null
        ) : base(key: key, child: child) {
            _barTopY = barTopY;
            _arrowTipX = arrowTipX;
            _isArrowPointingDown = isArrowPointingDown;

        }

        public readonly float _barTopY;

        public readonly float _arrowTipX;

        public readonly bool _isArrowPointingDown;

        public override RenderObject createRenderObject(BuildContext context) => new _ToolbarRenderBox(_barTopY, _arrowTipX, _isArrowPointingDown, null);

        public void updateRenderObject(BuildContext context, _ToolbarRenderBox renderObject) {
            renderObject.barTopY = _barTopY;
            renderObject.arrowTipX = _arrowTipX;
            renderObject.isArrowPointingDown = _isArrowPointingDown;
        }
    }

    class _ToolbarParentData : BoxParentData {
        public float arrowXOffsetFromCenter;
        public override string ToString() => $"offset = {offset}," + $"arrowXOffsetFromCenter={arrowXOffsetFromCenter}";
    }
    
    class _ToolbarRenderBox : RenderShiftedBox { 
        public _ToolbarRenderBox(
            float _barTopY = 0.0f,
            float _arrowTipX = 0.0f,
            bool _isArrowPointingDown = false,
            RenderBox child = null) : base(child) {
              this._barTopY = _barTopY;
              this._arrowTipX = _arrowTipX;
              this._isArrowPointingDown = _isArrowPointingDown;
              
        }

        public override bool isRepaintBoundary {
            get { return true; }
        }

        float _barTopY;
        public float barTopY {
            set {
                if (_barTopY == value) {
                    return;
                }
                _barTopY = value;
                markNeedsLayout();
                //markNeedsSemanticsUpdate();
            }
        }

        float _arrowTipX;

        public float arrowTipX {
            set {
                if (_arrowTipX == value) {
                    return;
                }
                _arrowTipX = value;
                markNeedsLayout();
                //markNeedsSemanticsUpdate();
            }
        }

        bool _isArrowPointingDown;

        public bool isArrowPointingDown {
          set {
              if (_isArrowPointingDown == value) {
                  return;
              }
              _isArrowPointingDown = value;
              markNeedsLayout();
              //markNeedsSemanticsUpdate();
          }
      }

      public readonly BoxConstraints heightConstraint =  BoxConstraints.tightFor(height: CupertinoTextSelectionUtils._kToolbarHeight);

      
      public override void setupParentData(RenderObject child) {
        if (!(child.parentData is _ToolbarParentData)) {
          child.parentData = new _ToolbarParentData();
        }
      }
      
      protected override void performLayout() {
        BoxConstraints constraints = this.constraints;
        size = constraints.biggest;

        if (child == null) {
          return;
        }
        BoxConstraints enforcedConstraint = constraints
          .deflate( EdgeInsets.symmetric(horizontal: CupertinoTextSelectionUtils._kToolbarScreenPadding))
          .loosen();

        child.layout(heightConstraint.enforce(enforcedConstraint), parentUsesSize: true);
         _ToolbarParentData childParentData = child.parentData as _ToolbarParentData;

       
        float lowerBound = child.size.width/2 + CupertinoTextSelectionUtils._kToolbarScreenPadding;
        float upperBound = size.width - child.size.width/2 - CupertinoTextSelectionUtils._kToolbarScreenPadding;
        float adjustedCenterX = _arrowTipX.clamp(lowerBound, upperBound) ;

        childParentData.offset = new Offset(adjustedCenterX - child.size.width / 2, _barTopY );
        childParentData.arrowXOffsetFromCenter = _arrowTipX - adjustedCenterX ;
      }

  
    Path _clipPath() {
        _ToolbarParentData childParentData = child.parentData as _ToolbarParentData;
        Path rrect = new Path();
        rrect.addRRect(
        RRect.fromRectAndRadius(
          new Offset(0, _isArrowPointingDown ? 0 : CupertinoTextSelectionUtils._kToolbarArrowSize.height)
          & new Size(child.size.width, child.size.height - CupertinoTextSelectionUtils._kToolbarArrowSize.height),
          CupertinoTextSelectionUtils._kToolbarBorderRadius
        ));

        float arrowTipX = child.size.width / 2 + childParentData.arrowXOffsetFromCenter;

        float arrowBottomY = _isArrowPointingDown
        ? child.size.height - CupertinoTextSelectionUtils._kToolbarArrowSize.height
        : CupertinoTextSelectionUtils._kToolbarArrowSize.height;

        float arrowTipY = _isArrowPointingDown ? child.size.height : 0;

        Path arrow = new Path();
        arrow.moveTo(arrowTipX, arrowTipY);
        arrow.lineTo(arrowTipX - CupertinoTextSelectionUtils._kToolbarArrowSize.width / 2, arrowBottomY);
        arrow.lineTo(arrowTipX + CupertinoTextSelectionUtils._kToolbarArrowSize.width / 2, arrowBottomY);
        arrow.close();

        return Path.combine(PathOperation.union, rrect, arrow);
    }

    public override void paint(PaintingContext context, Offset offset) {
        if (child == null) {
            return;
        }

        _ToolbarParentData childParentData = child.parentData as _ToolbarParentData;
        context.pushClipPath(
            needsCompositing,
            offset + childParentData.offset,
            Offset.zero & child.size,
            _clipPath(),
            (PaintingContext innerContext, Offset innerOffset) => innerContext.paintChild(child, innerOffset)
        );
    }

    Paint _debugPaint;


    protected override void debugPaintSize(PaintingContext context, Offset offset) {
        D.assert(() => {
            if (child == null) {
                return true;
            }

            //_debugPaint ??= new Paint()
            if (_debugPaint == null) {
                _debugPaint = new Paint();
            }

            _debugPaint.shader = ui.Gradient.linear(
                new Offset(0.0f, 0.0f),
                new Offset(10.0f, 10.0f),
                new List<Color> {
                    new Color(0x00000000), new Color(0xFFFF00FF), new Color(0xFFFF00FF),
                    new Color(0x00000000)
                },
                new List<float> {0.25f, 0.25f, 0.75f, 0.75f},
                TileMode.repeated
            );
            _debugPaint.strokeWidth = 2.0f;
            _debugPaint.style = PaintingStyle.stroke;
            _ToolbarParentData childParentData = child.parentData as _ToolbarParentData;
            context.canvas.drawPath(_clipPath().shift(offset + childParentData.offset), _debugPaint);
            return true;
        });
        }
    }

    public  class _TextSelectionHandlePainter : AbstractCustomPainter {
        public _TextSelectionHandlePainter(Color color) {
            this.color = color;
        }

        public readonly Color color;

        public override void paint(Canvas canvas, Size size) {
            float halfStrokeWidth = 1.0f;
            Paint paint = new Paint();
            paint.color = color;
            Rect circle = Rect.fromCircle(
                center: new Offset(CupertinoTextSelectionUtils._kSelectionHandleRadius, CupertinoTextSelectionUtils._kSelectionHandleRadius),
                radius: CupertinoTextSelectionUtils._kSelectionHandleRadius
            );
            Rect line = Rect.fromPoints(
            new Offset(
                CupertinoTextSelectionUtils._kSelectionHandleRadius - halfStrokeWidth,
            2 * CupertinoTextSelectionUtils._kSelectionHandleRadius - CupertinoTextSelectionUtils._kSelectionHandleOverlap
            ),
            new Offset(CupertinoTextSelectionUtils._kSelectionHandleRadius + halfStrokeWidth, size.height)
            );
            Path path = new Path();
            path.addOval(circle);
            // Draw line so it slightly overlaps the circle.
            path.addRect(line);
            canvas.drawPath(path, paint);
        }

        public override bool shouldRepaint(CustomPainter oldPainter) {
            oldPainter = (_TextSelectionHandlePainter)oldPainter;
            return color != ((_TextSelectionHandlePainter)oldPainter).color;
        }
    }

    public delegate bool Predicate(TextSelectionDelegate _delegate);

    public delegate void OnPressed(TextSelectionDelegate _delegate);
    class _CupertinoTextSelectionControls : TextSelectionControls {

        public override Size getHandleSize(float textLineHeight) {
            return new Size(
                CupertinoTextSelectionUtils._kSelectionHandleRadius * 2,
              textLineHeight + CupertinoTextSelectionUtils._kSelectionHandleRadius * 2 - CupertinoTextSelectionUtils._kSelectionHandleOverlap
            );
        } 
        public override Widget buildToolbar(
            BuildContext context,
            Rect globalEditableRegion,
            float textLineHeight,
            Offset position,
            List<TextSelectionPoint> endpoints,
            TextSelectionDelegate _delegate
            ) 
          { 
              D.assert(WidgetsD.debugCheckHasMediaQuery(context));
              MediaQueryData mediaQuery = MediaQuery.of(context);

              float toolbarHeightNeeded = mediaQuery.padding.top
                  + CupertinoTextSelectionUtils._kToolbarScreenPadding
                  + CupertinoTextSelectionUtils._kToolbarHeight
                  + CupertinoTextSelectionUtils._kToolbarContentDistance;
              float availableHeight = globalEditableRegion.top + endpoints.first().point.dy - textLineHeight;
              bool isArrowPointingDown = toolbarHeightNeeded <= availableHeight;

              float arrowTipX = (position.dx + globalEditableRegion.left).clamp(
                    CupertinoTextSelectionUtils._kArrowScreenPadding + mediaQuery.padding.left,
                  mediaQuery.size.width - mediaQuery.padding.right - CupertinoTextSelectionUtils._kArrowScreenPadding
                ) ;
              float localBarTopY = isArrowPointingDown
                  ? endpoints.first().point.dy - textLineHeight - CupertinoTextSelectionUtils._kToolbarContentDistance - CupertinoTextSelectionUtils._kToolbarHeight
                  : endpoints.last().point.dy + CupertinoTextSelectionUtils._kToolbarContentDistance;

              List<Widget> items = new List<Widget>{};
              Widget onePhysicalPixelVerticalDivider = new SizedBox(width: 1.0f / MediaQuery.of(context).devicePixelRatio);
              CupertinoLocalizations localizations = CupertinoLocalizations.of(context);
              EdgeInsets arrowPadding = isArrowPointingDown
                  ? EdgeInsets.only(bottom: CupertinoTextSelectionUtils._kToolbarArrowSize.height)
                  : EdgeInsets.only(top: CupertinoTextSelectionUtils._kToolbarArrowSize.height);
              void addToolbarButtonIfNeeded(
                  string text,
                  Predicate predicate,
                  OnPressed onPressed) {
                  if (!predicate(_delegate)) {
                    return;
                  }

                  if (items.isNotEmpty()) {
                    items.Add(onePhysicalPixelVerticalDivider);
                  }

                  items.Add(new CupertinoButton(
                    child: new Text(text, style:CupertinoTextSelectionUtils. _kToolbarButtonFontStyle),
                    color: CupertinoTextSelectionUtils._kToolbarBackgroundColor,
                    minSize: CupertinoTextSelectionUtils._kToolbarHeight,
                    padding: CupertinoTextSelectionUtils._kToolbarButtonPadding.add(arrowPadding),
                    borderRadius: null,
                    pressedOpacity: 0.7f,
                    onPressed: () => onPressed(_delegate)
                  ));
              }
              addToolbarButtonIfNeeded(localizations.cutButtonLabel, canCut, handleCut);
              addToolbarButtonIfNeeded(localizations.copyButtonLabel, canCopy, handleCopy);
              addToolbarButtonIfNeeded(localizations.pasteButtonLabel, canPaste, handlePaste);
              addToolbarButtonIfNeeded(localizations.selectAllButtonLabel, canSelectAll, handleSelectAll);
              return new CupertinoTextSelectionToolbar(
              barTopY: localBarTopY + globalEditableRegion.top,
              arrowTipX: arrowTipX,
              isArrowPointingDown: isArrowPointingDown,
              child: items.isEmpty() ? null : new DecoratedBox(
                decoration: new BoxDecoration(color: CupertinoTextSelectionUtils._kToolbarDividerColor),
                child: new Row(mainAxisSize: MainAxisSize.min, children: items)
                  )
              ); 
      } 
      public override Widget buildHandle(BuildContext context, TextSelectionHandleType type, float textLineHeight) {
          Size desiredSize = getHandleSize(textLineHeight);
          Widget handle = SizedBox.fromSize(
              size: desiredSize, 
              child: new CustomPaint(
                  painter: new _TextSelectionHandlePainter(CupertinoTheme.of(context).primaryColor)
                  )
              );
          var result = Matrix4.identity();
          result.translate(desiredSize.width / 2, desiredSize.height / 2);
          result.rotateZ(Mathf.PI);
          result.translate(-desiredSize.width / 2, -desiredSize.height / 2);
          switch (type) {
            case TextSelectionHandleType.left:
                return handle;
            case TextSelectionHandleType.right:
                return new Transform(
                transform: result,
                child: handle
                );
            case TextSelectionHandleType.collapsed:
                return new SizedBox();
          }
          return null;
        }
        public override Offset getHandleAnchor(TextSelectionHandleType type, float textLineHeight) {
            Size handleSize = getHandleSize(textLineHeight);
            switch (type) {
            
              case TextSelectionHandleType.left:
                return new Offset(
                  handleSize.width / 2,
                  handleSize.height
                );
             
              case TextSelectionHandleType.right:
                return new Offset(
                  handleSize.width / 2,
                  handleSize.height - 2 * CupertinoTextSelectionUtils._kSelectionHandleRadius + CupertinoTextSelectionUtils._kSelectionHandleOverlap
                );
              
              default:
                return new Offset(
                  handleSize.width / 2,
                  textLineHeight + (handleSize.height - textLineHeight) / 2
                );
            }
        }
    }



}