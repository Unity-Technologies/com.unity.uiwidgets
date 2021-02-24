using System.Collections.Generic;
using System.Linq;
using UIWidgetsGallery.gallery;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.material;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.widgets;
using UnityEngine;

namespace UIWidgetsGallery.demo.material
{
    internal static class SnackBarDemoUtils
    {
        public static readonly string _text1 =
            "Snackbars provide lightweight feedback about an operation by " +
            "showing a brief message at the bottom of the screen. Snackbars " +
            "can contain an action.";

        public static readonly string _text2 =
            "Snackbars should contain a single line of text directly related " +
            "to the operation performed. They cannot contain icons.";

        public static readonly string _text3 =
            "By default snackbars automatically disappear after a few seconds ";
    }

    internal class SnackBarDemo : StatefulWidget
    {
        public SnackBarDemo(Key key = null) : base(key: key)
        {
        }

        public static readonly string routeName = "/material/snack-bar";


        public override State createState()
        {
            return new _SnackBarDemoState();
        }
    }

    internal class _SnackBarDemoState : State<SnackBarDemo>
    {
        private int _snackBarIndex = 1;

        private Widget buildBody(BuildContext context)
        {
            return new SafeArea(
                top: false,
                bottom: false,
                child: new ListView(
                    padding: EdgeInsets.all(24.0f),
                    children: new List<Widget>
                        {
                            new Text(SnackBarDemoUtils._text1),
                            new Text(SnackBarDemoUtils._text2),
                            new Center(
                                child: new RaisedButton(
                                    child: new Text("SHOW A SNACKBAR"),
                                    onPressed: () =>
                                    {
                                        int thisSnackBarIndex = _snackBarIndex++;
                                        Scaffold.of(context).showSnackBar(new SnackBar(
                                            content: new Text($"This is snackbar #{thisSnackBarIndex}."),
                                            action: new SnackBarAction(
                                                label: "ACTION",
                                                onPressed: () =>
                                                {
                                                    Scaffold.of(context).showSnackBar(new SnackBar(
                                                        content: new Text(
                                                            $"You pressed snackbar {thisSnackBarIndex}'s action.")
                                                    ));
                                                }
                                            )
                                        ));
                                    }
                                )
                            ),
                            new Text(SnackBarDemoUtils._text3)
                        }
                        .Select<Widget, Widget>((Widget child) =>
                        {
                            return new Container(
                                margin: EdgeInsets.symmetric(vertical: 12.0f),
                                child: child
                            );
                        })
                        .ToList()
                )
            );
        }

        public override Widget build(BuildContext context)
        {
            return new Scaffold(
                appBar: new AppBar(
                    title: new Text("Snackbar"),
                    actions: new List<Widget> {new MaterialDemoDocumentationButton(SnackBarDemo.routeName)}
                ),
                body: new Builder(
                    builder: buildBody
                ),
                floatingActionButton: new FloatingActionButton(
                    child: new Icon(Icons.add),
                    tooltip: "Create",
                    onPressed: () => { Debug.Log("Floating Action Button was pressed"); }
                )
            );
        }
    }
}