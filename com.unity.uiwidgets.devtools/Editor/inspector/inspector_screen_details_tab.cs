/*using System;
using System.Collections.Generic;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.material;
using Unity.UIWidgets.rendering;
using Unity.UIWidgets.widgets;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.DevTools.inspector.layout_explorer;

namespace Unity.UIWidgets.DevTools.inspector
{
    public class inspector_screen_details_tab
    {
        
    }
    
    public class InspectorDetailsTabController : StatefulWidget {
      public InspectorDetailsTabController(
        Widget detailsTree = null,
        Widget actionButtons = null,
        InspectorController controller = null,
        bool layoutExplorerSupported = false,
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
      public readonly bool layoutExplorerSupported;

  
      public override State createState()
      {
        return new _InspectorDetailsTabControllerState();
      }
    }

public class _InspectorDetailsTabControllerState : State<InspectorDetailsTabController>, TickerProviderStateMixin, AutoDisposeMixin 
{
  public static readonly int _detailsTreeTabIndex = 1;
  public static readonly int _tabsLengthWithLayoutExplorer = 2;
  public static readonly int _tabsLengthWithoutLayoutExplorer = 1;

  TabController _tabControllerWithLayoutExplorer;
  TabController _tabControllerWithoutLayoutExplorer;
  
  public override void initState() {
    base.initState();
    addAutoDisposeListener(
      _tabControllerWithLayoutExplorer =
          new TabController(length: _tabsLengthWithLayoutExplorer, vsync: this)
    );
    addAutoDisposeListener(
      _tabControllerWithoutLayoutExplorer =
          new TabController(length: _tabsLengthWithoutLayoutExplorer, vsync: this)
    );
  }
  
  public override Widget build(BuildContext context)
  {
    List<Widget> tabs = new List<Widget>();
    if (widget.layoutExplorerSupported)
      tabs.Add(_buildTab("Layout Explorer"));
    tabs.Add(_buildTab("Details Tree"));

    List<Widget> tabViews = new List<Widget>();
    if (widget.layoutExplorerSupported)
      tabViews.Add(new LayoutExplorerTab(controller: widget.controller));
    tabViews.Add(widget.detailsTree);
      var _tabController = widget.layoutExplorerSupported
        ? _tabControllerWithLayoutExplorer
        : _tabControllerWithoutLayoutExplorer;

    var theme = Theme.of(context);
    var focusColor = theme.focusColor;
    var borderSide = new BorderSide(color: focusColor);
    var hasActionButtons = widget.actionButtons != null &&
        _tabController.index == _detailsTreeTabIndex;

    return new Column(
      children: new List<Widget>{
        new SizedBox(
          height: 50.0f,
          child: new Row(
            crossAxisAlignment: CrossAxisAlignment.end,
            children: new List<Widget>{
              new Container(
                color: focusColor,
                child: new TabBar(
                  controller: _tabController,
                  labelColor: theme.textTheme.bodyText1.color,
                  tabs: tabs,
                  isScrollable: true
                )
              ),
              new Expanded(
                child: new Container(
                  decoration: new BoxDecoration(border: new Border(bottom: borderSide)),
                  child: hasActionButtons
                      ? widget.actionButtons
                      : new SizedBox()
                )
              )
            }
          )
        ),
        new Expanded(
          child: new Container(
            decoration: new BoxDecoration(
              border: new Border(
                left: borderSide,
                bottom: borderSide,
                right: borderSide
              )
            ),
            child: new TabBarView(
              physics: CommonThemeUtils.defaultTabBarViewPhysics,
              controller: _tabController,
              children: tabViews
            )
          )
        ),
      }
    );
  }

  Widget _buildTab(string tabName) {
    return new Tab(
      child: new Text(
        tabName,
        overflow: TextOverflow.ellipsis
      )
    );
  }
}

    
}*/