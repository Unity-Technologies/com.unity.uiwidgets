using System.Collections.Generic;
using Unity.UIWidgets.engine2;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.material;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.ui;
using Unity.UIWidgets.widgets;
using TextStyle = Unity.UIWidgets.painting.TextStyle;
using ui_ = Unity.UIWidgets.widgets.ui_;

namespace UIWidgetsSample
{
    public class UIWidgetsExample : UIWidgetsPanel
    {
        protected void OnEnable()
        {
            base.OnEnable();
        }

        protected override void main()
        {
            ui_.runApp(new MyApp());
        }

        class MyApp : StatelessWidget
        {
            public override Widget build(BuildContext context)
            {
                return new WidgetsApp(
                    home: new ExampleApp(),
                    pageRouteBuilder: (settings, builder) =>
                        new PageRouteBuilder(
                            settings: settings,
                            pageBuilder: (Buildcontext, animation, secondaryAnimation) => builder(context)
                        )
                );
            }
        }

        class ExampleApp : StatefulWidget
        {
            public ExampleApp(Key key = null) : base(key)
            {
            }

            public override State createState()
            {
                return new ExampleState();
            }
        }

        class ExampleState : State<ExampleApp>
        {
            int counter;

            public override Widget build(BuildContext context)
            {
                return new Container(
                    color: Colors.green,
                    child: new Column(
                        children: new List<Widget>
                        {
                            new Text("Counter: " + counter,
                                style: new TextStyle(fontSize: 18, fontWeight: FontWeight.w100)),
                            new Text("Counter: " + counter,
                                style: new TextStyle(fontFamily: "racher", fontSize: 18, fontWeight: FontWeight.w100)),
                            new Text("Counter: " + counter,
                                style: new TextStyle(fontFamily: "roboto", fontSize: 18, fontWeight: FontWeight.w200)),
                            new Text("Counter: " + counter,
                                style: new TextStyle(fontFamily: "roboto", fontSize: 18, fontWeight: FontWeight.w300)),
                            new Text("Counter: " + counter,
                                style: new TextStyle(fontFamily: "roboto", fontSize: 18, fontWeight: FontWeight.w400)),
                            new Text("Counter: " + counter,
                                style: new TextStyle(fontFamily: "roboto", fontSize: 18, fontWeight: FontWeight.w500)),
                            new Text("Counter: " + counter,
                                style: new TextStyle(fontFamily: "roboto", fontSize: 18, fontWeight: FontWeight.w600)),
                            new Text("Counter: " + counter,
                                style: new TextStyle(fontFamily: "roboto", fontSize: 18, fontWeight: FontWeight.w700)),
                            new Text("Counter: " + counter,
                                style: new TextStyle(fontFamily: "roboto", fontSize: 18, fontWeight: FontWeight.w800)),
                            new Text("Counter: " + counter,
                                style: new TextStyle(fontFamily: "roboto", fontSize: 18, fontWeight: FontWeight.w900)),
                            new Text("Counter: " + counter,
                                style: new TextStyle(fontFamily: "roboto", fontSize: 18, fontWeight: FontWeight.w900,
                                    fontStyle: FontStyle.italic)),
                            new Text("Counter: " + counter,
                                style: new TextStyle(fontFamily: "roboto", fontSize: 18, fontWeight: FontWeight.w100)),
                            new Text("Counter: " + counter,
                                style: new TextStyle(fontFamily: "roboto", fontSize: 18, fontWeight: FontWeight.w900)),
                            new Text("-----"),
                            new Text("Counter: " + counter,
                                style: new TextStyle(fontFamily: "robotox", fontSize: 18, fontWeight: FontWeight.w200)),
                            new Text("Counter: " + counter,
                                style: new TextStyle(fontFamily: "robotox", fontSize: 18, fontWeight: FontWeight.w300)),
                            new Text("Counter: " + counter,
                                style: new TextStyle(fontFamily: "robotox", fontSize: 18, fontWeight: FontWeight.w400)),
                            new Text("Counter: " + counter,
                                style: new TextStyle(fontFamily: "robotox", fontSize: 18, fontWeight: FontWeight.w500)),
                            new Text("Counter: " + counter,
                                style: new TextStyle(fontFamily: "robotox", fontSize: 18, fontWeight: FontWeight.w600)),
                            new Text("Counter: " + counter,
                                style: new TextStyle(fontFamily: "robotox", fontSize: 18, fontWeight: FontWeight.w700)),
                            new Text("Counter: " + counter,
                                style: new TextStyle(fontFamily: "robotox", fontSize: 18, fontWeight: FontWeight.w800)),
                            new Text("Counter: " + counter,
                                style: new TextStyle(fontFamily: "robotox", fontSize: 18, fontWeight: FontWeight.w900)),
                            new Text("Counter: " + counter + (char) 0xf472 + (char) 0xf442 + (char) 0xf43b,
                                style: new TextStyle(fontFamily: "robotox", fontSize: 18, fontWeight: FontWeight.w900,
                                    fontStyle: FontStyle.italic)),
                            new Text("Counter: " + counter + (char) 0xf472 + (char) 0xf442 + (char) 0xf43b,
                                style: new TextStyle(fontFamily: "CupertinoIcons", fontSize: 18)),

                            new GestureDetector(
                                onTap: () =>
                                {
                                    setState(()
                                        =>
                                    {
                                        counter++;
                                    });
                                },
                                child: new Container(
                                    padding: EdgeInsets.symmetric(20, 20),
                                    color: counter % 2 == 0 ? Colors.blue : Colors.red,
                                    child: new Text("Click Me",
                                        style: new TextStyle(fontFamily: "racher", fontWeight: FontWeight.w100))
                                )
                            )
                        }
                    )
                );
            }
        }
    }
}