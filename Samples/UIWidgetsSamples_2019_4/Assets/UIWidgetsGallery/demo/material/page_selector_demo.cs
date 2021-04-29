using System.Collections.Generic;
using System.Linq;
using UIWidgetsGallery.gallery;
using Unity.UIWidgets.material;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.rendering;
using Unity.UIWidgets.ui;
using Unity.UIWidgets.widgets;

namespace UIWidgetsGallery.demo.material
{
    class _PageSelector : StatelessWidget {
  public _PageSelector(List<Icon> icons)
  {
    this.icons = icons;
  }

  public readonly List<Icon> icons;

  void _handleArrowButtonPress(BuildContext context, int delta) {
    TabController controller = DefaultTabController.of(context);
    if (!controller.indexIsChanging)
      controller.animateTo((controller.index + delta).clamp(0, icons.Count - 1));
  }

  public override Widget build(BuildContext context) {
     TabController controller = DefaultTabController.of(context);
     Color color = Theme.of(context).accentColor;
    return new SafeArea(
      top: false,
      bottom: false,
      child: new Column(
        children: new List<Widget>{
          new Container(
            margin: EdgeInsets.only(top: 16.0f),
            child: new Row(
              children: new List<Widget>{
                new IconButton(
                  icon: new  Icon(Icons.chevron_left),
                  color: color,
                  onPressed: () => { _handleArrowButtonPress(context, -1); },
                  tooltip: "Page back"
                ),
                new TabPageSelector(controller: controller),
                new IconButton(
                  icon: new  Icon(Icons.chevron_right),
                  color: color,
                  onPressed: () => { _handleArrowButtonPress(context, 1); },
                  tooltip: "Page forward"
                )
              },
              mainAxisAlignment: MainAxisAlignment.spaceBetween
            )
          ),
          new Expanded(
            child: new IconTheme(
              data: new IconThemeData(
                size: 128.0f,
                color: color
              ),
              child: new TabBarView(
                children: icons.Select<Icon, Widget>((Icon icon) => {
                  return new Container(
                    padding: EdgeInsets.all(12.0f),
                    child: new Card(
                      child: new Center(
                        child: icon
                      )
                    )
                  );
                }).ToList()
              )
            )
          )
        }
      )
    );
  }
}
  
  class PageSelectorDemo : StatelessWidget {
  public static readonly string routeName = "/material/page-selector";
  static readonly List<Icon> icons = new List<Icon>{
    new Icon(Icons.event_icon),
  new Icon(Icons.home),
  new Icon(Icons.android),
  new Icon(Icons.alarm),
  new Icon(Icons.face),
  new Icon(Icons.language)
  };

    public override Widget build(BuildContext context) {
    return new Scaffold(
      appBar: new AppBar(
        title: new Text("Page selector"),
    actions: new List<Widget>{new MaterialDemoDocumentationButton(routeName)}),

    body: new DefaultTabController(
        length: icons.Count,
        child: new _PageSelector(icons: icons)
      )
      );
  }
  }
}