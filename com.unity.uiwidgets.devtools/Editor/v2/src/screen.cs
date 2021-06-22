using System.Collections.Generic;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.material;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.widgets;

namespace Unity.UIWidgets.DevTools
{
    public abstract class Screen {
      public Screen(
        string screenId,
        string title = null,
        IconData icon = null,
        Key tabKey = null,
        string conditionalLibrary = null
      )
      {
        this.screenId = screenId;
        this.title = title;
        this.icon = icon;
        this.tabKey = tabKey;
        this.conditionalLibrary = conditionalLibrary;
      }

      public readonly string screenId;
  
  public readonly string title;

  public readonly IconData icon;

  public readonly Key tabKey;

  public readonly string conditionalLibrary;
  
  
  public bool showIsolateSelector => false;
  
  public string docPageId => null;
  
  Widget buildTab(BuildContext context) {
    return new Tab(
      key: tabKey,
      child: new Row(
        children: new List<Widget>{
          new Icon(icon, size: ThemeUtils.defaultIconSize),
          new Padding(
            padding: EdgeInsets.only(left: 8.0f),
            child: new Text(title)
          ),
        }
      )
    );
  }
  
  public abstract Widget build(BuildContext context);
  
  Widget buildStatus(BuildContext context, TextTheme textTheme) {
    return null;
  }
}
}