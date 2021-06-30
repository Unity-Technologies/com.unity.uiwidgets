using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.UIWidgets.animation;
using Unity.UIWidgets.DevTools.ui;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.gestures;
using Unity.UIWidgets.material;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.rendering;
using Unity.UIWidgets.service;
using Unity.UIWidgets.services;
using Unity.UIWidgets.ui;
using Unity.UIWidgets.widgets;
using UnityEditor.Graphs;
using UnityEngine;
using Canvas = Unity.UIWidgets.ui.Canvas;
using Color = Unity.UIWidgets.ui.Color;

namespace Unity.UIWidgets.DevTools.inspector
{

    public static class _InspectorTreeRowWidgetUtils
    {
        public const float rowHeight = 24.0f;
        public const float columnWidth = 16.0f;
        public const float chartLineStrokeWidth = 1.0f;
        
        public static Paint _defaultPaint(ColorScheme colorScheme)
        {
            Paint res = new Paint();
            res.color = ColorsUtils.treeGuidelineColor;
            res.strokeWidth = chartLineStrokeWidth;
            return res;
        }
    }
    
    class _InspectorTreeRowWidget : StatefulWidget {
        public _InspectorTreeRowWidget(
            Key key = null,
            InspectorTreeRow row = null,
            _InspectorTreeState inspectorTreeState = null
        ) : base(key: key)
        {
            this.row = row;
            this.inspectorTreeState = inspectorTreeState;
        }

        public readonly _InspectorTreeState inspectorTreeState;

        InspectorTreeNode node
        {
            get
            {
                return row.node;
            }
        }

            
        
        public readonly InspectorTreeRow row;

        public override State createState()
        {
           return new _InspectorTreeRowState();
        }
    }

    class _InspectorTreeRowState : TickerProviderStateMixin<_InspectorTreeRowWidget>
    {
        
        public override Widget build(BuildContext context)
        {
            return new SizedBox(
                height: _InspectorTreeRowWidgetUtils.rowHeight,
                child: new InspectorRowContent(
                    row: widget.row,
                    expandArrowAnimation: expandArrowAnimation,
                    controller: widget.inspectorTreeState.controller,
                    onToggle: () => {
                        // setExpanded(!isExpanded);
                    }
                )
            );
        }
    }
    
    class InspectorTreeControllerFlutter : InspectorTreeController
    {
        public readonly float rowWidth = 1200;
        float _maxIndent;
        // InspectorControllerClient client
        // {
        //     get
        //     {
        //         return _client;
        //     }
        // }
        // InspectorControllerClient _client;
        
        
        public float maxRowIndent {
            get
            {
                if (lastContentWidth == null) {
                    float maxIndent = 0;
                    for (int i = 0; i < numRows; i++) {
                        var row = getCachedRow(i);
                        if (row != null) {
                            maxIndent = Mathf.Max(maxIndent, getDepthIndent(row.depth));
                        }
                    }
                    lastContentWidth = maxIndent + maxIndent;
                    _maxIndent = maxIndent;
                }
                return _maxIndent;
            }
        }

        public override void setState(VoidCallback fn)
        {
            fn();
            // client?.onChanged();
        }

        public override InspectorTreeNode createNode()
        {
            return new InspectorTreeNode();
        }
    }

    class InspectorTree : StatefulWidget
    {
        public InspectorTree(
            Key key = null,
            InspectorTreeController controller = null,
            bool isSummaryTree = false
        ) : base(key: key)
        {
            this.controller = controller;
            this.isSummaryTree = isSummaryTree;
        }

        public readonly InspectorTreeController controller;
        public bool isSummaryTree;

        public override State createState()
        {
            return new _InspectorTreeState();
        }
    }

    class _InspectorTreeState : State<InspectorTree>
    {
        ScrollController _scrollControllerY;
        ScrollController _scrollControllerX;
        FocusNode _focusNode;
        public InspectorTreeControllerFlutter controller
        {
            get
            {
                return (InspectorTreeControllerFlutter)widget.controller;
            }
        }
        
        bool _handleKeyEvent(FocusNode _, RawKeyEvent event_) {
            if (!(event_ is RawKeyDownEvent)) return false;

            // if (event_.logicalKey == LogicalKeyboardKey.arrowDown) {
            //     controller.navigateDown();
            //     return true;
            // } else if (event_.logicalKey == LogicalKeyboardKey.arrowUp) {
            //     controller.navigateUp();
            //     return true;
            // } else if (event_.logicalKey == LogicalKeyboardKey.arrowLeft) {
            //     controller.navigateLeft();
            //     return true;
            // } else if (event_.logicalKey == LogicalKeyboardKey.arrowRight) {
            //     controller.navigateRight();
            //     return true;
            // }

            return false;
        }
        
        public override Widget build(BuildContext context)
        {
            // base.build(context);
            if (controller == null) {
                // Indicate the tree is loading.
                return new Center(child: new CircularProgressIndicator());
            }

            GestureTapCallback gestureTapCallback = () =>  _focusNode.requestFocus();
            return new Scrollbar(
                child: new SingleChildScrollView(
                    scrollDirection: Axis.horizontal,
                    controller: _scrollControllerX,
                    child: new SizedBox(
                        width: controller.rowWidth + controller.maxRowIndent,
                        child: new Scrollbar(
                            child: new GestureDetector(
                                onTap:  null,// gestureTapCallback,
                                child: new Focus(
                                    onKey: _handleKeyEvent,
                                    autofocus: widget.isSummaryTree,
                                    focusNode: _focusNode,
                                    child: ListView.custom(
                                        itemExtent: InspectorTreeUtils.rowHeight,
                                        childrenDelegate: new SliverChildBuilderDelegate(
                                            (context2, index) =>{
                                                        InspectorTreeRow row = controller.root?.getRow(index);
                                                        return new _InspectorTreeRowWidget(
                                                            key: new PageStorageKey<InspectorTreeNode>(row?.node),
                                                            inspectorTreeState: this,
                                                            row: row
                                                        );
                                                    },
                                        childCount: controller.numRows
                                        ),
                                        controller: _scrollControllerY
                                    )
                                )
                            )
                        )
                    )
                )
            );
        }
    }
    
    
    class InspectorRowContent : StatelessWidget {
      public InspectorRowContent(
          InspectorTreeRow row = null,
          InspectorTreeControllerFlutter controller = null,
          VoidCallback onToggle = null,
          Animation<float> expandArrowAnimation = null
      )
      {
          this.row = row;
          this.controller = controller;
          this.onToggle = onToggle;
          this.expandArrowAnimation = expandArrowAnimation;
      }

      public readonly InspectorTreeRow row;
      public readonly InspectorTreeControllerFlutter controller;
      public readonly VoidCallback onToggle;
      public readonly Animation<float> expandArrowAnimation;
  
      public override Widget build(BuildContext context) {
        float currentX = controller.getDepthIndent(row.depth) - _InspectorTreeRowWidgetUtils.columnWidth;
        var colorScheme = Theme.of(context).colorScheme;

        if (row == null) {
          return new SizedBox();
        }
        Color backgroundColor = null;
        if (row.isSelected || row.node == controller.hover) {
          backgroundColor = row.isSelected
              ? InspectorTreeUtils.selectedRowBackgroundColor
              : InspectorTreeUtils.hoverColor;
        }

        var node = row.node;

        return new CustomPaint(
            painter: new _RowPainter(row, controller, colorScheme),
            size: new Size(currentX, _InspectorTreeRowWidgetUtils.rowHeight),
            child: new Padding(
            padding: EdgeInsets.only(left: currentX),
            child: new ClipRect(
              child: new Row(
                mainAxisSize: MainAxisSize.min,
                textBaseline: TextBaseline.alphabetic,
                children: new List<Widget>{
                  node.showExpandCollapse
                      ? new SizedBox(child:
                          new InkWell(
                          onTap: () => { onToggle();},
                          child: new RotationTransition(
                              turns: expandArrowAnimation,
                              child: new Icon(
                                  Icons.expand_more,
                                  size: ThemeUtils.defaultIconSize
                              )
                          )
                      ))
                      : new SizedBox(
                          width: ThemeUtils.defaultSpacing, height: ThemeUtils.defaultSpacing),
                  new DecoratedBox(
                    decoration: new BoxDecoration(
                      color: backgroundColor
                    ),
                    child: new InkWell(
                      onTap: () => {
                        // controller.onSelectRow(row);
                        // controller.requestFocus();
                      },
                      child: new Container(
                        height: _InspectorTreeRowWidgetUtils.rowHeight,
                        padding: EdgeInsets.symmetric(horizontal: 4.0f),
                        child: new Text(node.ToString())// new DiagnosticsNodeDescription(node.diagnostic)
                      )
                    )
                  ),
                }
              )
            )
          )
        );
      }
    }
    
    class _RowPainter : AbstractCustomPainter {
        public _RowPainter(InspectorTreeRow row, InspectorTreeController controller, ColorScheme colorScheme)
        {
            this.row = row;
            this.controller = controller;
            this.colorScheme = colorScheme;
        }

        public readonly InspectorTreeController controller;
        public readonly InspectorTreeRow row;
        public readonly ColorScheme colorScheme;
        
        public override void paint(Canvas canvas, Size size) {
            float currentX = 0;
            var paint = _InspectorTreeRowWidgetUtils._defaultPaint(colorScheme);

            if (row == null) {
                return;
            }
            InspectorTreeNode node = row.node;
            bool showExpandCollapse = node.showExpandCollapse;
            foreach (int tick in row.ticks) {
                currentX = controller.getDepthIndent(tick) - _InspectorTreeRowWidgetUtils.columnWidth * 0.5f;
                // Draw a vertical line for each tick identifying a connection between
                // an ancestor of this node and some other node in the tree.
                canvas.drawLine(
                    new Offset(currentX, 0.0f),
                    new Offset(currentX, _InspectorTreeRowWidgetUtils.rowHeight),
                    paint
                );
            }
            // If this row is itself connected to a parent then draw the L shaped line
            // to make that connection.
            if (row.lineToParent.Value) {
                currentX = controller.getDepthIndent(row.depth - 1) - _InspectorTreeRowWidgetUtils.columnWidth * 0.5f;
                float width = showExpandCollapse ? _InspectorTreeRowWidgetUtils.columnWidth * 0.5f : _InspectorTreeRowWidgetUtils.columnWidth;
                canvas.drawLine(
                    new Offset(currentX, 0.0f),
                    new Offset(currentX, _InspectorTreeRowWidgetUtils.rowHeight * 0.5f),
                    paint
                );
                canvas.drawLine(
                    new Offset(currentX, _InspectorTreeRowWidgetUtils.rowHeight * 0.5f),
                    new Offset(currentX + width, _InspectorTreeRowWidgetUtils.rowHeight * 0.5f),
                    paint
                );
            }
        }
        
        public override bool shouldRepaint(CustomPainter oldDelegate) {
            // if (oldDelegate is _RowPainter) {
            //     return ((_RowPainter)oldDelegate).colorScheme.isLight != colorScheme.isLight;
            // }
            return true;
        }
        
    }
    
    
}