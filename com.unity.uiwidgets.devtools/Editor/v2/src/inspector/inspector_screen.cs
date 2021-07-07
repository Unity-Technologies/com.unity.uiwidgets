
using System.Collections.Generic;
using Unity.UIWidgets.async;
using Unity.UIWidgets.material;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.rendering;
using Unity.UIWidgets.widgets;
using Unity.UIWidgets.DevTools.ui;
using UnityEngine;
using Color = Unity.UIWidgets.ui.Color;

namespace Unity.UIWidgets.DevTools.inspector
{
    class InspectorScreen : Screen
    {
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

        public class InspectorScreenBody : StatefulWidget
        {
            public InspectorScreenBody()
            {
            }

            public override State createState()
            {
                return new _InspectorScreenBodyState();
            }
        }

        class _InspectorScreenBodyState : State<InspectorScreenBody>
        {
            bool _layoutExplorerSupported = false;
            bool _expandCollapseSupported = false;
            bool connectionInProgress = false;
            InspectorController inspectorController;
            InspectorTreeControllerFlutter summaryTreeController;
            InspectorTreeControllerFlutter detailsTreeController;
            InspectorService inspectorService;



            public override void initState()
            {
                base.initState();
                _handleConnectionStart();
            }

            void _onExpandCollapseSupported()
            {
                setState(() => { _expandCollapseSupported = true; });
            }

            void _onLayoutExplorerSupported()
            {
                setState(() => { _layoutExplorerSupported = true; });
            }

            void _handleConnectionStart()
            {

                setState(() => { connectionInProgress = true; });

                try
                {
                    // Init the inspector service, or return null.
                    // ensureInspectorDependencies();
                    // ensureInspectorServiceDependencies();
                    inspectorService = new InspectorService();
                    // InspectorService.create(service).catchError((e) => null);
                }
                finally
                {
                    setState(() => { connectionInProgress = false; });
                }

                if (inspectorService == null)
                {
                    return;
                }


                setState(() =>
                {
                    // inspectorController?.dispose();
                    summaryTreeController = new InspectorTreeControllerFlutter();
                    detailsTreeController = new InspectorTreeControllerFlutter();
                    inspectorController = new InspectorController(
                        inspectorTree: summaryTreeController,
                        detailsTree: detailsTreeController,
                        inspectorService: inspectorService,
                        treeType: FlutterTreeType.widget,
                        onExpandCollapseSupported: _onExpandCollapseSupported,
                        onLayoutExplorerSupported: _onLayoutExplorerSupported
                    );

                });
                _refreshInspector();
            }

            void _refreshInspector()
            {
                inspectorController?.onForceRefresh().then((v) =>
                {
                    setState(()=>{});
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
                        new Row(
                            crossAxisAlignment: CrossAxisAlignment.start,
                            children: new List<Widget>
                            {
                                new SizedBox(width: ThemeUtils.denseSpacing),
                                new Container(
                                    height: Theme.of(context).buttonTheme.height + 0.01f,
                                    child: new OutlineButton(
                                        onPressed: _refreshInspector,
                                        child: new MaterialIconLabel(
                                            Icons.refresh,
                                            "Refresh Tree",
                                            includeTextWidth: 750
                                        )
                                    )
                                ),
                                new Spacer()
                            }),
                        new SizedBox(height: ThemeUtils.denseRowSpacing),
                        new Expanded(
                            child: new Split(
                                axis: splitAxis,
                                initialFractions: new List<float?> {0.33f, 0.67f},
                                children: new List<Widget>
                                {
                                    summaryTree,
                                    new InspectorDetailsTabController(
                                        detailsTree: detailsTree,
                                        controller: inspectorController,
                                        actionButtons: null,
                                        layoutExplorerSupported: _layoutExplorerSupported
                                    ),
                                }
                            )
                        )
                    }
                );
            }
        }
    }
}