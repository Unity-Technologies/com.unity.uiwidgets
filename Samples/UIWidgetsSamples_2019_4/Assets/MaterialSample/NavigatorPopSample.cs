using System.Collections.Generic;
using Unity.UIWidgets.async;
using Unity.UIWidgets.engine;
using Unity.UIWidgets.material;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.rendering;
using Unity.UIWidgets.widgets;

namespace UIWidgetsSample
{
    public class NavigatorPopSample : UIWidgetsPanel
    {
        protected override void main()
        {
            ui_.runApp(new MaterialApp(
                title: "Returning Data",
                home: new PopDemo()));
        }

        protected new void OnEnable()
        {
            base.OnEnable();
        }
    }

    public class PopDemo : StatelessWidget
    {
        public override Widget build(BuildContext context)
        {
            return new Scaffold(
                appBar: new AppBar(
                    title: new Text("Returning Data Demo")
                ),
                body: new Center(child: new SelectionButton())
            );
        }
    }

    internal class SelectionButton : StatelessWidget
    {
        public override Widget build(BuildContext context)
        {
            return new RaisedButton(
                onPressed: () => { _navigateAndDisplaySelection(context); },
                child: new Text("Pick an option, any option!")
            );
        }

        // A method that launches the SelectionScreen and awaits the result from
        // Navigator.pop!
        private void _navigateAndDisplaySelection(BuildContext context)
        {
            // Navigator.push returns a Future that will complete after we call
            // Navigator.pop on the Selection Screen!
            Future<bool> result = Navigator.push<bool>(
                context,
                new MaterialPageRoute(builder: (subContext) => new SelectionScreen())
            );

            // After the Selection Screen returns a result, show it in a Snackbar!
            result.then(value =>
            {
                bool res = (bool) value;
                Scaffold
                    .of(context)
                    .showSnackBar(new SnackBar(content: new Text($"{res}")));
            });
        }
    }

    internal class SelectionScreen : StatelessWidget
    {
        public override Widget build(BuildContext context)
        {
            return new Scaffold(
                appBar: new AppBar(
                    title: new Text("Pick an option")
                ),
                body: new Center(
                    child: new Column(
                        mainAxisAlignment: MainAxisAlignment.center,
                        children: new List<Widget>
                        {
                            new Padding(
                                padding: EdgeInsets.all(8.0f),
                                child: new RaisedButton(
                                    onPressed: () =>
                                    {
                                        // Close the screen and return "Yep!" as the result
                                        Navigator.pop(context, true);
                                    },
                                    child: new Text("Yep!")
                                )
                            ),
                            new Padding(
                                padding: EdgeInsets.all(8.0f),
                                child: new RaisedButton(
                                    onPressed: () =>
                                    {
                                        // Close the screen and return "Nope!" as the result
                                        Navigator.pop(context, false);
                                    },
                                    child: new Text("Nope.")
                                )
                            )
                        }
                    )
                )
            );
        }
    }
}