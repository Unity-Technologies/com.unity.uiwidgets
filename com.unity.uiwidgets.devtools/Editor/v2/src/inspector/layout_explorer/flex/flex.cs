using Unity.UIWidgets.foundation;
using Unity.UIWidgets.widgets;

namespace Unity.UIWidgets.DevTools.inspector.layout_explorer.flex
{
    public class FlexLayoutExplorerWidget : StatefulWidget {
        public FlexLayoutExplorerWidget(
            InspectorController inspectorController, 
            Key key = null
        ) : base(key: key)
        {
            this.inspectorController = inspectorController;
        }

        public readonly InspectorController inspectorController;

        public static bool shouldDisplay(RemoteDiagnosticsNode node) {
            return (node?.isFlex ?? false) || (node?.parent?.isFlex ?? false);
        }
        
        public override State createState()
        {
            return new _FlexLayoutExplorerWidgetState();
        }
    }

    public class _FlexLayoutExplorerWidgetState : State<FlexLayoutExplorerWidget>
    {
        public override Widget build(BuildContext context)
        {
            return new Container(child: new Text("asdfafafa dakfiwejo"));
        }
    }
}