using System;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.material;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.service;
using Unity.UIWidgets.services;
using Unity.UIWidgets.ui;
using Unity.UIWidgets.widgets;
using UnityEngine;

namespace Unity.UIWidgets.DevTools.inspector
{
    
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

    class _InspectorTreeRowState : State<_InspectorTreeRowWidget>
    {
        public override Widget build(BuildContext context)
        {
            // return new SizedBox(
            //     height: rowHeight,
            //     child: InspectorRowContent(
            //         row: widget.row,
            //         expandArrowAnimation: expandArrowAnimation,
            //         controller: widget.inspectorTreeState.controller,
            //         onToggle: () => {
            //             setExpanded(!isExpanded);
            //         }
            //     )
            // );
            return null;
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
        InspectorTreeControllerFlutter controller
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

            return new Scrollbar(
                child: new SingleChildScrollView(
                    scrollDirection: Axis.horizontal,
                    controller: _scrollControllerX,
                    child: new SizedBox(
                        width: controller.rowWidth + controller.maxRowIndent,
                        child: new Scrollbar(
                            child: new GestureDetector(
                                onTap: null,//_focusNode.requestFocus,
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
                                                            key: null,// new PageStorageKey(row?.node),
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
    
    
}