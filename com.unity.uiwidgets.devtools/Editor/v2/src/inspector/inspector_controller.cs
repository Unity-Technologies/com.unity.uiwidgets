using Unity.UIWidgets.foundation;
using Unity.UIWidgets.ui;

namespace Unity.UIWidgets.DevTools.inspector
{
    class InspectorController
    {
        public InspectorController(
            // @required this.inspectorService,
            InspectorTreeController inspectorTree = null,
            InspectorTreeController detailsTree = null,
                // @required this.treeType,
            InspectorController parent = null,
            bool isSummaryTree = true,
            VoidCallback onExpandCollapseSupported = null,
            VoidCallback onLayoutExplorerSupported = null
        )
        {
            this.inspectorTree = inspectorTree;
            this.parent = parent;
            this.isSummaryTree = isSummaryTree;
            this.onExpandCollapseSupported = onExpandCollapseSupported;
            this.onLayoutExplorerSupported = onLayoutExplorerSupported;
        }
        
        InspectorTreeController inspectorTree;
        // public readonly FlutterTreeType treeType;

        // public readonly InspectorService inspectorService;

        bool _disposed = false;

        InspectorController parent;
        
        public readonly bool isSummaryTree;

        public readonly VoidCallback onExpandCollapseSupported;

        public readonly VoidCallback onLayoutExplorerSupported;
        
       
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