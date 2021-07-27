using System.Collections.Generic;
using Unity.UIWidgets.animation;
using Unity.UIWidgets.engine;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.ui;
using Unity.UIWidgets.widgets;
using UnityEngine;
using Color = Unity.UIWidgets.ui.Color;
using FontStyle = Unity.UIWidgets.ui.FontStyle;
using Image = Unity.UIWidgets.widgets.Image;
using TextStyle = Unity.UIWidgets.painting.TextStyle;
using ui_ = Unity.UIWidgets.widgets.ui_;

namespace UIWidgetsSample
{
    public class RaycastPanelSample : UIWidgetsRaycastablePanel
    {
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
                    color: Color.white,
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
            private bool raycastable = true;
            
            public override Widget build(BuildContext context)
            {
                var child = new Container(
                    padding: EdgeInsets.symmetric(20, 20),
                    color: counter % 2 == 0 ? Color.fromARGB(255, 0, 255, 0) : Color.fromARGB(255, 0, 0, 255),
                    child: new Text("Click Me",
                        style: new TextStyle(fontFamily: "racher", fontWeight: FontWeight.w100))
                );
                
                
                return new Container(
                    color: Color.fromARGB(0, 0, 0, 0),
                    child: new Column(
                        children: new List<Widget>
                        {
                            new Text("Counter: " + counter,
                                style: new TextStyle(fontSize: 18, fontWeight: FontWeight.w100)),

                            new GestureDetector(
                                onTap: () =>
                                {
                                    setState(() =>
                                    {
                                        counter++;
                                    });
                                },
                                child: raycastable ? (Widget) new RaycastableContainer(
                                    child: child) : child
                            )
                        }
                    )
                );
            }
        }
    }
}