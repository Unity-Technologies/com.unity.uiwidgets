using Unity.UIWidgets.DevTools.inspector.layout_explorer.flex;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.rendering;
using Unity.UIWidgets.ui;
using Unity.UIWidgets.widgets;

namespace Unity.UIWidgets.DevTools.inspector.layout_explorer
{
    public class LayoutExplorerTab : StatefulWidget {
        public LayoutExplorerTab(Key key = null, InspectorController controller = null) : base(key: key)
        {
            this.controller = controller;
        }

        public readonly InspectorController controller;
        
        public override State createState()
        {
            return new _LayoutExplorerTabState();
        }
    }

    public class _LayoutExplorerTabState : State<LayoutExplorerTab>
    {
        
        InspectorController controller => widget.controller;

        RemoteDiagnosticsNode selected => controller?.selectedNode?.diagnostic;
        
        Widget rootWidget(RemoteDiagnosticsNode node) {
            if (FlexLayoutExplorerWidget.shouldDisplay(node))
                return new FlexLayoutExplorerWidget(controller);
            return new Center(
                child: new Text(
                    "Currently, Layout Explorer only supports Flex-based widgets" +
                    "(e.g., Row, Column, Flex) or their direct children.",
                    textAlign: TextAlign.center,
                    overflow: TextOverflow.clip
                )
            );
        }
        
        public override Widget build(BuildContext context)
        {
            return rootWidget(selected);
        }
    }
    
    
}