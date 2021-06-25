
using System.Collections.Generic;
using Unity.UIWidgets.material;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.rendering;
using Unity.UIWidgets.widgets;
using UnityEngine;

namespace Unity.UIWidgets.DevTools.inspector
{
    class InspectorScreen : Screen {
        public InspectorScreen() : base(
                "inspector",
                title: "Flutter Inspector",
                icon: Octicons.deviceMobile
            )
        {
            
        }
        public override Widget build(BuildContext context)
        {
            // var isFlutterApp = serviceManager.connectedApp.isFlutterAppNow;
            // var isProfileBuild = serviceManager.connectedApp.isProfileBuildNow;
            // if (!isFlutterApp || isProfileBuild) {
            //     return !isFlutterApp
            //         ? new DisabledForNonFlutterAppMessage()
            //         : new DisabledForProfileBuildMessage();
            // }
            return new InspectorScreenBody();
        }
        
        public class InspectorScreenBody : StatefulWidget {
            public InspectorScreenBody(){}

            public override State createState()
            {
                return new _InspectorScreenBodyState();
            }
        }

        class _InspectorScreenBodyState : State<InspectorScreenBody>
        {
            bool _layoutExplorerSupported = false;
            bool _expandCollapseSupported = false;
            InspectorController inspectorController;
            InspectorTreeControllerFlutter summaryTreeController;
            InspectorTreeControllerFlutter detailsTreeController;

            
            
            public override void initState() {
                base.initState();
                _handleConnectionStart();
            }
            
            void _onExpandCollapseSupported() {
                setState(() => {
                    _expandCollapseSupported = true;
                });
            }

            void _onLayoutExplorerSupported() {
                setState(() => {
                    _layoutExplorerSupported = true;
                });
            }

            void _handleConnectionStart()
            {
                setState(() => {
                    // inspectorController?.dispose();
                    summaryTreeController = new InspectorTreeControllerFlutter();
                    detailsTreeController = new InspectorTreeControllerFlutter();
                    inspectorController = new InspectorController(
                        inspectorTree: summaryTreeController,
                        detailsTree: detailsTreeController,
                        // inspectorService: inspectorService,
                        // treeType: FlutterTreeType.widget,
                        onExpandCollapseSupported: _onExpandCollapseSupported,
                        onLayoutExplorerSupported: _onLayoutExplorerSupported
                    );
                    
                });
            }
            

            public override Widget build(BuildContext context)
            {
                var summaryTree = new Container(
                    decoration: new BoxDecoration(
                        border: Border.all(color: Theme.of(context).focusColor)
                    ),
                    child: new InspectorTree(
                        controller: summaryTreeController,
                        isSummaryTree: true
                    )
                );
                var detailsTree = new InspectorTree(
                    controller: detailsTreeController
                );

                var splitAxis = Split.axisFor(context, 0.85f);
                return new Column(
                    children: new List<Widget>
                    {
                        new Expanded(
                            child: new Split(
                                axis: splitAxis,
                                initialFractions: new List<float?>{0.33f, 0.67f},
                                children: new List<Widget>
                                {
                                    summaryTree,
                                    new InspectorDetailsTabController(
                                        detailsTree: detailsTree,
                                        controller: inspectorController,
                                        actionButtons: null,
                                        layoutExplorerSupported: _layoutExplorerSupported
                                    )
                                }
                            )
                        )
                    }
                );
            } 
        }
    }
}