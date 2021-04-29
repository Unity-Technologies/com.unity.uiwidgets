using System;
using System.Collections.Generic;
using UIWidgetsGallery.gallery;
using Unity.UIWidgets.async;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.material;
using Unity.UIWidgets.widgets;

namespace UIWidgetsGallery.demo.material
{
    internal enum IndicatorType
    {
        overscroll,
        refresh
    }

    internal class OverscrollDemo : StatefulWidget
    {
        public OverscrollDemo(Key key = null) : base(key: key)
        {
        }

        public static readonly string routeName = "/material/overscroll";

        public override State createState()
        {
            return new OverscrollDemoState();
        }
    }

    internal class OverscrollDemoState : State<OverscrollDemo>
    {
        private readonly GlobalKey<ScaffoldState> _scaffoldKey = GlobalKey<ScaffoldState>.key();
        private readonly GlobalKey<RefreshIndicatorState> _refreshIndicatorKey = GlobalKey<RefreshIndicatorState>.key();

        private static readonly List<string> _items = new List<string>
        {
            "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N",
        };

        private Future _handleRefresh()
        {
            Completer completer = Completer.create();
            Timer.create(new TimeSpan(0, 0, 0, 3), () => { completer.complete(); });
            return completer.future.then((_) =>
            {
                _scaffoldKey.currentState?.showSnackBar(new SnackBar(
                    content: new Text("Refresh complete"),
                    action: new SnackBarAction(
                        label: "RETRY",
                        onPressed: () => { _refreshIndicatorKey.currentState.show(); }
                    )
                ));
            });
        }

        public override Widget build(BuildContext context)
        {
            return new Scaffold(
                key: _scaffoldKey,
                appBar: new AppBar(
                    title: new Text("Pull to refresh"),
                    actions: new List<Widget>
                    {
                        new MaterialDemoDocumentationButton(OverscrollDemo.routeName),
                        new IconButton(
                            icon: new Icon(Icons.refresh),
                            tooltip: "Refresh",
                            onPressed: () => { _refreshIndicatorKey.currentState.show(); }
                        )
                    }
                ),
                body: new RefreshIndicator(
                    key: _refreshIndicatorKey,
                    onRefresh: _handleRefresh,
                    child: new Scrollbar(
                        child: ListView.builder(
                            padding: material_.kMaterialListPadding,
                            itemCount: _items.Count,
                            itemBuilder: (BuildContext subContext, int index) =>
                            {
                                string item = _items[index];
                                return new ListTile(
                                    isThreeLine: true,
                                    leading: new CircleAvatar(child: new Text(item)),
                                    title: new Text($"This item represents {item}."),
                                    subtitle: new Text(
                                        "Even more additional list item information appears on line three.")
                                );
                            }
                        )
                    )
                )
            );
        }
    }
}