using System;
using System.Text.RegularExpressions;
using Unity.UIWidgets.async;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.material;
using Unity.UIWidgets.ui;
using UnityEngine;
using TextStyle = Unity.UIWidgets.painting.TextStyle;

namespace Unity.UIWidgets.DevTools.inspector
{

    public static class InspectorControllerUtils
    {
        public static TextStyle textStyleForLevel(DiagnosticLevel level, ColorScheme colorScheme) {
            switch (level) {
                case DiagnosticLevel.hidden:
                    return inspector_text_styles.unimportant(colorScheme);
                case DiagnosticLevel.warning:
                    return inspector_text_styles.warning(colorScheme);
                case DiagnosticLevel.error:
                    return inspector_text_styles.error(colorScheme);
                case DiagnosticLevel.debug:
                case DiagnosticLevel.info:
                case DiagnosticLevel.fine:
                default:
                    return inspector_text_styles.regular;
            }
        }
    }
    
    public class InspectorController
    {
        public InspectorController(
            InspectorService inspectorService,
            InspectorTreeController inspectorTree = null,
            InspectorTreeController detailsTree = null,
            FlutterTreeType treeType= FlutterTreeType.widget,
            InspectorController parent = null,
            bool isSummaryTree = true,
            VoidCallback onExpandCollapseSupported = null,
            VoidCallback onLayoutExplorerSupported = null
        )
        {
            this.inspectorService = inspectorService;
            this.inspectorTree = inspectorTree;
            this.parent = parent;
            this.isSummaryTree = isSummaryTree;
            this.onExpandCollapseSupported = onExpandCollapseSupported;
            this.onLayoutExplorerSupported = onLayoutExplorerSupported;
            _treeGroups = new InspectorObjectGroupManager(inspectorService, "tree");
            // _selectionGroups =
            //     new InspectorObjectGroupManager(inspectorService, "selection");
            //     _refreshRateLimiter = RateLimiter(refreshFramesPerSecond, refresh);
            //     _refreshRateLimiter = RateLimiter(refreshFramesPerSecond, refresh); 
            //
            //     D.assert(inspectorTree != null);
            inspectorTree.config = new InspectorTreeConfig(
                summaryTree: isSummaryTree,
                treeType: treeType,
                // onNodeAdded: _onNodeAdded,
                // onHover: highlightShowNode,
                // onSelectionChange: selectionChanged,
                onExpand: _onExpand,
                onClientActiveChange: _onClientChange
            );
                if (isSummaryTree) {
                    details = new InspectorController(
                        inspectorService: inspectorService,
                        inspectorTree: detailsTree,
                        treeType: treeType,
                        parent: this,
                        isSummaryTree: false
                    );
                } else {
                    details = null;
                }

                // flutterIsolateSubscription = serviceManager.isolateManager
                //     .getSelectedIsolate((IsolateRef flutterIsolate) => {
                //     // Any time we have a new isolate it means the previous isolate stopped.
                //     onIsolateStopped();
                // });
                //
                // _checkForExpandCollapseSupport();
                // _checkForLayoutExplorerSupport();
                //
                // // This logic only needs to be run once so run it in the outermost
                // // controller.
                // if (parent == null) {
                //     // If select mode is available, enable the on device inspector as it
                //     // won't interfere with users.
                //     addAutoDisposeListener(_supportsToggleSelectWidgetMode, () => {
                //         if (_supportsToggleSelectWidgetMode.value) {
                //             serviceManager.serviceExtensionManager.setServiceExtensionState(
                //                 extensions.enableOnDeviceInspector.extension,
                //                 true,
                //                 true
                //             );
                //         }
                //     });
                // }
            

        }
        
        InspectorObjectGroupManager _treeGroups;
        
        InspectorController details;
        
        InspectorTreeController inspectorTree;
        public readonly FlutterTreeType treeType;

        public readonly InspectorService inspectorService;

        bool _disposed = false;
        
        InspectorController parent;
        int _clientCount = 0;
        
        public readonly bool isSummaryTree;

        public readonly VoidCallback onExpandCollapseSupported;

        public readonly VoidCallback onLayoutExplorerSupported;
        
        public InspectorTreeNode selectedNode;
        
        
        bool isActive = false;
        bool visibleToUser = true;
        bool flutterAppFrameReady = false;
        bool detailsSubtree => parent != null;
        
        void setVisibleToUser(bool visible) {
            if (visibleToUser == visible) {
                return;
            }
            visibleToUser = visible;
            details?.setVisibleToUser(visible);

            if (visibleToUser) {
                if (parent == null) {
                    maybeLoadUI();
                }
            } else {
                shutdownTree(false);
            }
        }
        
        
       
        void _onExpand(InspectorTreeNode node) {
            inspectorTree.maybePopulateChildren(node);
        }
        
        
        
        void _onClientChange(bool added) {
            _clientCount += added ? 1 : -1;
            D.assert(_clientCount >= 0);
            if (_clientCount == 1) {
                setVisibleToUser(true);
                setActivate(true);
            } else if (_clientCount == 0) {
                setVisibleToUser(false);
            }
        }
        
        Future recomputeTreeRoot(
            RemoteDiagnosticsNode newSelection,
            RemoteDiagnosticsNode detailsSelection,
            bool setSubtreeRoot,
            int subtreeDepth = 2
        )
        {
            D.assert(!_disposed);
            if (_disposed)
            {
                return new SynchronousFuture(null);
            }

            _treeGroups.cancelNext();
            try
            {

                var group = _treeGroups.next;
                RemoteDiagnosticsNode node = null;
                group.getRoot(treeType).then_<RemoteDiagnosticsNode>((v) =>
                {
                    node = v;
                    if (node == null || group.disposed) {
                        return new SynchronousFuture(null);
                    }
                    _treeGroups.promoteNext();
                    // clearValueToInspectorTreeNodeMapping();
                    if (node != null) {
                        InspectorTreeNode rootNode = inspectorTree.setupInspectorTreeNode(
                            node:inspectorTree.createNode(),
                            diagnosticsNode: node,
                            expandChildren: true,
                            expandProperties: false
                        );
                        inspectorTree.root = rootNode;
                    } else {
                        inspectorTree.root = inspectorTree.createNode();
                    }
                    return FutureOr.nil;
                });

                // refreshSelection(newSelection, detailsSelection, setSubtreeRoot);
            } catch (Exception e) {
                Debug.Log(e);
                _treeGroups.cancelNext();
                return new SynchronousFuture(null);
            }
            return new SynchronousFuture(null);
        }
        
        
        
        public Future onForceRefresh() {
            D.assert(!_disposed);
            if (!visibleToUser || _disposed) {
                return Future.value();
            }
            recomputeTreeRoot(null, null, false);

            return Future.value();
            // return getPendingUpdateDone();
        }
        
        void setActivate(bool enabled) {
            if (!enabled) {
                // onIsolateStopped();
                isActive = false;
                return;
            }
            if (isActive) {
                return;
            }
            
            isActive = true;
            // inspectorService.addClient(this);
            maybeLoadUI();
        }

        void maybeLoadUI() {
            if (!visibleToUser || !isActive) {
                return;
            }

            if (flutterAppFrameReady) {
                inspectorService.inferPubRootDirectoryIfNeeded();
                // updateSelectionFromService(firstFrame: true);
            } else {
                var ready = inspectorService.isWidgetTreeReady();
                ready.then_<bool>((v) =>
                {
                    flutterAppFrameReady = v;
                    if (isActive && v) {
                        maybeLoadUI();
                    }
                    
                    return FutureOr.value(null);
                });
            }
        }
        
        void shutdownTree(bool isolateStopped) {
            // programaticSelectionChangeInProgress = true;
            // _treeGroups?.clear(isolateStopped);
            // _selectionGroups?.clear(isolateStopped);
            //
            // currentShowNode = null;
            // selectedNode = null;
            // lastExpanded = null;
            //
            // selectedNode = null;
            // subtreeRoot = null;
            //
            // inspectorTree?.root = inspectorTree?.createNode();
            // details?.shutdownTree(isolateStopped);
            // programaticSelectionChangeInProgress = false;
            // valueToInspectorTreeNode?.clear();
        }
        
        
        // public override void dispose() {
        //     D.assert(!_disposed);
        //     _disposed = true;
        //     flutterIsolateSubscription.cancel();
        //     if (inspectorService != null) {
        //         shutdownTree(false);
        //     }
        //     _treeGroups?.clear(false);
        //     _treeGroups = null;
        //     _selectionGroups?.clear(false);
        //     _selectionGroups = null;
        //     details?.dispose();
        //     base.dispose();
        // }
        
        
    }
}