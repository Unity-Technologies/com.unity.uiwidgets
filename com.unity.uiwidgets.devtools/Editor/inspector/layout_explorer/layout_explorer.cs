using Unity.UIWidgets.DevTools.inspector.layout_explorer.box;
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

    public class _LayoutExplorerTabState : State<LayoutExplorerTab> //, AutomaticKeepAliveClientMixin<LayoutExplorerTab>, AutoDisposeMixin 
    {
    InspectorController controller
    {
        get
        {
            return widget.controller;
        }
    }

    RemoteDiagnosticsNode selected
    {
        get
        {
            return null; //controller?.selectedNode?.value?.diagnostic;
        }
    }

    RemoteDiagnosticsNode previousSelection;

    Widget rootWidget(RemoteDiagnosticsNode node) {
        if (FlexLayoutExplorerWidget.shouldDisplay(node)) {
            return new FlexLayoutExplorerWidget(controller);
        }
        if (BoxLayoutExplorerWidget.shouldDisplay(node)) {
            return new BoxLayoutExplorerWidget(controller);
        }
        return new Center(
            child: new Text(
                node != null
                    ? "Currently, Layout Explorer only supports Box and Flex-based widgets." 
                    : "Select a widget to view its layout.",
                textAlign: TextAlign.center,
                overflow: TextOverflow.clip
            )
        );
    }

    void onSelectionChanged() {
        if (rootWidget(previousSelection).GetType() !=
            rootWidget(selected).GetType()) {
            setState(() => previousSelection = selected);
        }
    }

    
    public override void initState() {
        base.initState();
        //addAutoDisposeListener(controller.selectedNode, onSelectionChanged);
    }

    
    public override Widget build(BuildContext context) {
        //base.build(context);
        return rootWidget(selected);
    }

    
    public new bool wantKeepAlive
    {
        get
        {
            return true;
        }
    }
    }

    
}