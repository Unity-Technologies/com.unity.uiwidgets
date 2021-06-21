using Unity.UIWidgets.foundation;
using Unity.UIWidgets.widgets;

namespace Unity.UIWidgets.DevTools.inspector
{
    class InspectorDetailsTabController : StatefulWidget
    {
        public InspectorDetailsTabController(
            Widget detailsTree = null,
            Widget actionButtons = null,
            InspectorController controller = null,
            bool? layoutExplorerSupported = null,
            Key key = null
        ) : base(key: key)
        {
            this.detailsTree = detailsTree;
            this.actionButtons = actionButtons;
            this.controller = controller;
            this.layoutExplorerSupported = layoutExplorerSupported;
        }

        public readonly Widget detailsTree;
        public readonly Widget actionButtons;
        public readonly InspectorController controller;
        public readonly bool? layoutExplorerSupported;

        public override State createState()
        {
            return new _InspectorDetailsTabControllerState();
        }
    }

    class _InspectorDetailsTabControllerState : State<InspectorDetailsTabController>
    {
        public override Widget build(BuildContext context)
        {
            return new Container(child: new Text("this is a text!"));
        }
    }
    
}