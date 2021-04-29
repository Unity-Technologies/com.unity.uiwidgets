using System.Collections.Generic;
using System.Linq;
using UIWidgetsGallery.gallery;
using Unity.UIWidgets.material;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.widgets;

namespace UIWidgetsGallery.demo.material
{
    internal static class TooltipsDemoUtils
    {
        public static readonly string _introText =
            "Tooltips are short identifying messages that briefly appear in response to " +
            "a long press. Tooltip messages are also used by services that make Flutter " +
            "apps accessible, like screen readers.";
    }

    internal class TooltipDemo : StatelessWidget
    {
        public static readonly string routeName =
            "/material/tooltips";

        public override Widget build(BuildContext context)
        {
            ThemeData theme = Theme.of(context);
            return new Scaffold(
                appBar: new AppBar(
                    title: new Text("Tooltips"),
                    actions: new List<Widget> {new MaterialDemoDocumentationButton(routeName)}
                ),
                body: new Builder(
                    builder: (BuildContext subContext) =>
                    {
                        return new SafeArea(
                            top: false,
                            bottom: false,
                            child: new ListView(
                                children: new List<Widget>
                                    {
                                        new Text(TooltipsDemoUtils._introText, style: theme.textTheme.subtitle1),
                                        new Row(
                                            children: new List<Widget>
                                            {
                                                new Text("Long press the ", style: theme.textTheme.subtitle1),
                                                new Tooltip(
                                                    message: "call icon",
                                                    child: new Icon(
                                                        Icons.call,
                                                        size: 18.0f,
                                                        color: theme.iconTheme.color
                                                    )
                                                ),
                                                new Text(" icon.", style: theme.textTheme.subtitle1)
                                            }
                                        ),
                                        new Center(
                                            child: new IconButton(
                                                iconSize: 48.0f,
                                                icon: new Icon(Icons.call),
                                                color: theme.iconTheme.color,
                                                tooltip: "Place a phone call",
                                                onPressed: () =>
                                                {
                                                    Scaffold.of(context).showSnackBar(new SnackBar(
                                                        content: new Text("That was an ordinary tap.")
                                                    ));
                                                }
                                            )
                                        )
                                    }
                                    .Select<Widget, Widget>((Widget widget) =>
                                    {
                                        return new Padding(
                                            padding: EdgeInsets.only(top: 16.0f, left: 16.0f, right: 16.0f),
                                            child: widget
                                        );
                                    })
                                    .ToList()
                            )
                        );
                    }
                )
            );
        }
    }
}