using System.Collections.Generic;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.material;
using Unity.UIWidgets.rendering;
using Unity.UIWidgets.widgets;
using Unity.UIWidgets.DevTools.inspector.layout_explorer;
using Unity.UIWidgets.painting;

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
    private static readonly int _detailsTreeTabIndex = 0;
    TabController _tabControllerWithLayoutExplorer;
    TabController _tabControllerWithoutLayoutExplorer;


    public override Widget build(BuildContext context)
    {
      // var tabs = new List<Widget>();
      // tabs.Add(_buildTab("Details Tree"));
      // if (widget.layoutExplorerSupported.Value)
      // {
      //   tabs.Add(_buildTab("Layout Explorer"));
      // }
      //
      // var tabViews = new List<Widget>();
      // tabViews.Add(widget.detailsTree);
      // if (widget.layoutExplorerSupported.Value)
      // {
      //   tabViews.Add(new LayoutExplorerTab(controller: widget.controller));
      // }
      //
      // var _tabController = widget.layoutExplorerSupported.Value
      //   ? _tabControllerWithLayoutExplorer
      //   : _tabControllerWithoutLayoutExplorer;
      //
      // var theme = Theme.of(context);
      // var focusColor = theme.focusColor;
      // var borderSide = new BorderSide(color: focusColor);
      // var hasActionButtons = widget.actionButtons != null &&
      //                        _tabController.index == _detailsTreeTabIndex;
      //
      // return new Column(
      //   children: new List<Widget>
      //   {
      //     new SizedBox(
      //       height: 50.0f,
      //       child: new Row(
      //         crossAxisAlignment: CrossAxisAlignment.end,
      //         children: new List<Widget>
      //         {
      //           new Container(
      //             color: focusColor,
      //             child: new TabBar(
      //               controller: _tabController,
      //               labelColor: theme.textTheme.bodyText1.color,
      //               tabs: tabs,
      //               isScrollable: true
      //             )
      //           ),
      //           new Expanded(
      //             child: new Container(
      //               decoration: new BoxDecoration(border: new Border(bottom: borderSide)),
      //               child: hasActionButtons
      //                 ? widget.actionButtons
      //                 : new SizedBox()
      //             )
      //           ),
      //         }
      //       )
      //     ),
      //     new Expanded(
      //       child: new Container(
      //         decoration: new BoxDecoration(
      //           border: new Border(
      //             left: borderSide,
      //             bottom: borderSide,
      //             right: borderSide
      //           )
      //         ),
      //         child: new TabBarView(
      //           physics: ThemeUtils.defaultTabBarViewPhysics,
      //           controller: _tabController,
      //           children: tabViews
      //         )
      //       )
      //     )
      //   }
      // );
      //
      // Widget _buildTab(string tabName)
      // {
      //   return new Tab(
      //     child: new Text(
      //       tabName,
      //       overflow: TextOverflow.ellipsis
      //     )
      //   );
      // }
      return new Container(child: new Text("Not Implement yet"));
    }
  }
}