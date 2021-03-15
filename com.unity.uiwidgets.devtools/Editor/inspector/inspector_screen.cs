using System;
using System.Collections.Generic;
using Unity.UIWidgets.DevTools.inspector;
using Unity.UIWidgets.DevTools.ui;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.material;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.rendering;
using Unity.UIWidgets.widgets;
using UnityEngine;

namespace Unity.UIWidgets.DevTools.inspector
{
    public class InspectorScreen : Screen {
      public InspectorScreen():base(
          id: id,
          requiresLibrary: null,//flutterLibraryUri,
          requiresDebugBuild: true,
          title: "Flutter Inspector",
          icon: Octicons.deviceMobile
        )
        {
          
        }
      
      

  public static readonly string id = "inspector";

  
  public new string docPageId
  {
    get
    {
      return screenId;
    }
  }
  
  public override Widget build(BuildContext context)
  {
    return new InspectorScreenBody();
  }

  
}

public class InspectorScreenBody : StatefulWidget {
  public InspectorScreenBody(){}
  
  public override State createState()
  {
    return new InspectorScreenBodyState();
  }
}

public class InspectorScreenBodyState : State<InspectorScreenBody>, BlockingActionMixin, AutoDisposeMixin
{
  bool _expandCollapseSupported = false;
  bool _layoutExplorerSupported = false;
  bool connectionInProgress = false;
  InspectorService inspectorService;

  InspectorController inspectorController;
  InspectorTreeControllerFlutter summaryTreeController;
  InspectorTreeControllerFlutter detailsTreeController;
  bool displayedWidgetTrackingNotice = false;

  // bool  enableButtons
  // {
  //   get
  //   {
  //     return actionInProgress == false && connectionInProgress == false;
  //   }
  // }

  public static readonly Key summaryTreeKey = Key.key("Summary Tree");
  public static readonly Key detailsTreeKey = Key.key("Details Tree");
  
  public override void initState() {
    base.initState();
    // ga.screen(InspectorScreen.id);
    // autoDispose(
    //     serviceManager.onConnectionAvailable.listen(_handleConnectionStart));
    // if (serviceManager.hasConnection) {
    //   _handleConnectionStart(serviceManager.service);
    // }
    // autoDispose(
    //     serviceManager.onConnectionClosed.listen(_handleConnectionStop));
  }
  
  public override void dispose() {
    inspectorService?.dispose();
    inspectorController?.dispose();
    base.dispose();
  }

  // void _onExpandClick() {
  //   blockWhileInProgress(inspectorController.expandAllNodesInDetailsTree);
  // }

  // void _onResetClick() {
  //   blockWhileInProgress(inspectorController.collapseDetailsToSelected);
  // }

  
  public override Widget build(BuildContext context) {
    var summaryTree = _buildSummaryTreeColumn();

    var detailsTree = InspectorTree(
      key: detailsTreeKey,
      controller: detailsTreeController
    );

    var splitAxis = Split.axisFor(context, 0.85);
    return new Column(
      children: new List<Widget>{
        new Row(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: new List<Widget>{
            new ValueListenableBuilder(
              valueListenable: serviceManager.serviceExtensionManager
                  .hasServiceExtension(
                      extensions.toggleSelectWidgetMode.extension),
              builder: (_, selectModeSupported, __) => {
                return ServiceExtensionButtonGroup(
                  extensions: 
                    selectModeSupported
                        ? extensions.toggleSelectWidgetMode
                        : extensions.toggleOnDeviceWidgetInspector,
                  minIncludeTextWidth: 650
                );
              }
            ),
            new SizedBox(width: denseSpacing),
            IconLabelButton(
              onPressed: _refreshInspector,
              icon: Icons.refresh,
              label: "Refresh Tree",
              includeTextWidth: 750
            ),
            new Spacer(),
            new Row(children: getServiceExtensionWidgets()),
          }
        ),
        new SizedBox(height: denseRowSpacing),
        new Expanded(
          child: Split(
            axis: splitAxis,
            initialFractions: const [0.33, 0.67],
            children: [
              summaryTree,
              InspectorDetailsTabController(
                detailsTree: detailsTree,
                controller: inspectorController,
                actionButtons: _expandCollapseButtons(),
                layoutExplorerSupported: _layoutExplorerSupported,
              ),
            ],
          ),
        ),
      },
    );
  }

  Widget _buildSummaryTreeColumn() => OutlineDecoration(
        child: ValueListenableBuilder(
          valueListenable: serviceManager.errorBadgeManager
              .erroredItemsForPage(InspectorScreen.id),
          builder: (_, LinkedHashMap<String, DevToolsError> errors, __) {
            final inspectableErrors = errors.map(
                (key, value) => MapEntry(key, value as InspectableWidgetError));
            return Stack(
              children: [
                InspectorTree(
                  key: summaryTreeKey,
                  controller: summaryTreeController,
                  isSummaryTree: true,
                  widgetErrors: inspectableErrors,
                ),
                if (errors.isNotEmpty && inspectorController != null)
                  ValueListenableBuilder(
                    valueListenable: inspectorController.selectedErrorIndex,
                    builder: (_, selectedErrorIndex, __) => Positioned(
                      top: 0,
                      right: 0,
                      child: ErrorNavigator(
                        errors: inspectableErrors,
                        errorIndex: selectedErrorIndex,
                        onSelectError: inspectorController.selectErrorByIndex,
                      ),
                    ),
                  )
              ],
            );
          },
        ),
      );

  List<Widget> getServiceExtensionWidgets() {
    return [
      ServiceExtensionButtonGroup(
        minIncludeTextWidth: 1050,
        extensions: [extensions.slowAnimations],
      ),
      const SizedBox(width: denseSpacing),
      ServiceExtensionButtonGroup(
        minIncludeTextWidth: 1050,
        extensions: [extensions.debugPaint, extensions.debugPaintBaselines],
      ),
      const SizedBox(width: denseSpacing),
      ServiceExtensionButtonGroup(
        minIncludeTextWidth: 1250,
        extensions: [
          extensions.repaintRainbow,
          extensions.invertOversizedImages,
        ],
      ),
      // TODO(jacobr): implement TogglePlatformSelector.
      //  TogglePlatformSelector().selector
    ];
  }

  Widget _expandCollapseButtons() {
    if (!_expandCollapseSupported) return null;

    return Align(
      alignment: Alignment.centerRight,
      child: Row(
        mainAxisAlignment: MainAxisAlignment.end,
        mainAxisSize: MainAxisSize.min,
        children: [
          Flexible(
            child: FixedHeightOutlinedButton(
              onPressed: enableButtons ? _onExpandClick : null,
              child: const Text(
                'Expand all',
                overflow: TextOverflow.ellipsis,
              ),
            ),
          ),
          const SizedBox(width: denseSpacing),
          Flexible(
            child: FixedHeightOutlinedButton(
              onPressed: enableButtons ? _onResetClick : null,
              child: const Text(
                'Collapse to selected',
                overflow: TextOverflow.ellipsis,
              ),
            ),
          )
        ],
      ),
    );
  }

  void _onExpandCollapseSupported() {
    setState(() {
      _expandCollapseSupported = true;
    });
  }

  void _onLayoutExplorerSupported() {
    setState(() {
      _layoutExplorerSupported = true;
    });
  }

  void _handleConnectionStart(VmService service) async {
    setState(() {
      connectionInProgress = true;
    });

    try {
      // Init the inspector service, or return null.
      await ensureInspectorServiceDependencies();
      inspectorService =
          await InspectorService.create(service).catchError((e) => null);
    } finally {
      setState(() {
        connectionInProgress = false;
      });
    }

    if (inspectorService == null) {
      return;
    }

    setState(() {
      inspectorController?.dispose();
      summaryTreeController = InspectorTreeControllerFlutter();
      detailsTreeController = InspectorTreeControllerFlutter();
      inspectorController = InspectorController(
        inspectorTree: summaryTreeController,
        detailsTree: detailsTreeController,
        inspectorService: inspectorService,
        treeType: FlutterTreeType.widget,
        onExpandCollapseSupported: _onExpandCollapseSupported,
        onLayoutExplorerSupported: _onLayoutExplorerSupported,
      );

      // Clear any existing badge/errors for older errors that were collected.
      serviceManager.errorBadgeManager.clearErrors(InspectorScreen.id);
      inspectorController.filterErrors();

      // TODO(jacobr): move this notice display to once a day.
      if (!displayedWidgetTrackingNotice) {
        // ignore: unawaited_futures
        inspectorService.isWidgetCreationTracked().then((bool value) {
          if (value) {
            return;
          }

          displayedWidgetTrackingNotice = true;
          // TODO(jacobr): implement showMessage.
          // framework.showMessage(
          //  message: trackWidgetCreationWarning,
          //  screenId: inspectorScreenId,
          //);
        });
      }
    });
  }

  void _handleConnectionStop(dynamic event) {
    inspectorController?.setActivate(false);
    inspectorController?.dispose();
    setState(() {
      inspectorController = null;
    });
  }

  void _refreshInspector() {
    ga.select(inspector, refresh);
    blockWhileInProgress(() async {
      await inspectorController?.onForceRefresh();
    });
  }
}

class ErrorNavigator extends StatelessWidget {
  const ErrorNavigator({
    Key key,
    @required this.errors,
    @required this.errorIndex,
    @required this.onSelectError,
  }) : super(key: key);

  final LinkedHashMap<String, InspectableWidgetError> errors;

  final int errorIndex;

  final Function(int) onSelectError;

  @override
  Widget build(BuildContext context) {
    final label = errorIndex != null
        ? 'Error ${errorIndex + 1}/${errors.length}'
        : 'Errors: ${errors.length}';
    return Container(
      color: devtoolsError,
      child: Padding(
        padding: const EdgeInsets.symmetric(
          horizontal: defaultSpacing,
          vertical: denseSpacing,
        ),
        child: Row(
          children: [
            Padding(
              padding: const EdgeInsets.only(right: denseSpacing),
              child: Text(label),
            ),
            IconButton(
              padding: EdgeInsets.zero,
              constraints: const BoxConstraints(),
              splashRadius: defaultIconSize,
              icon: const Icon(Icons.keyboard_arrow_up),
              onPressed: _previousError,
            ),
            IconButton(
              padding: EdgeInsets.zero,
              constraints: const BoxConstraints(),
              splashRadius: defaultIconSize,
              icon: const Icon(Icons.keyboard_arrow_down),
              onPressed: _nextError,
            ),
          ],
        ),
      ),
    );
  }

  void _previousError() {
    var newIndex = errorIndex == null ? errors.length - 1 : errorIndex - 1;
    while (newIndex < 0) {
      newIndex += errors.length;
    }

    onSelectError(newIndex);
  }

  void _nextError() {
    final newIndex = errorIndex == null ? 0 : (errorIndex + 1) % errors.length;

    onSelectError(newIndex);
  }
}

}