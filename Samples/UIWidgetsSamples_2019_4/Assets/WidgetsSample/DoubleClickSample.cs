using System.Collections;
using System.Collections.Generic;
using uiwidgets;
using Unity.UIWidgets.engine;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.rendering;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.ui;
using Unity.UIWidgets.widgets;
using UnityEngine;
using Color = Unity.UIWidgets.ui.Color;
using TextStyle = Unity.UIWidgets.painting.TextStyle;
using ui_ = Unity.UIWidgets.widgets.ui_;

namespace UIWidgetsSample
{
    public class DoubleClickSample : UIWidgetsPanel
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
                    color: Color.white,
                    home: new DoubleClickApp(),
                    pageRouteBuilder: (settings, builder) =>
                        new PageRouteBuilder(
                            settings: settings,
                            pageBuilder: (Buildcontext, animation, secondaryAnimation) => builder(context)
                        )
                );
            }

            class DoubleClickApp : StatefulWidget
            {
                public DoubleClickApp(Key key = null) : base(key)
                {
                }

                public override State createState()
                {
                    return new DoubleClickState();
                }
            }

            class DoubleClickState : State<DoubleClickApp>
            {
                int counter;

                public override Widget build(BuildContext context)
                {
                    return new Container(
                        color: Color.fromARGB(255, 0,0, 0),
                        child: new Column(
                            children: new List<Widget>
                            {
                                new Text("双击次数: " + counter,
                                    style: new TextStyle(fontSize: 18, fontWeight: FontWeight.w100)),
                                
                                // new Container(
                                //     margin: EdgeInsets.fromLTRB(200, 50, 200, 200),
                                //     padding: EdgeInsets.symmetric(20, 60),
                                //     color: Color.fromARGB(255, 0, 0, 0),
                                //     child: new Text("双击次数: " + counter,
                                //         style: new TextStyle(fontSize: 18, fontWeight: FontWeight.w100))
                                // ),

                                new GestureDetector(
                                    onDoubleTap: () => { setState(() => { counter++; }); },
                                    child: new Container(
                                        margin: EdgeInsets.fromLTRB(200, 150, 200, 200),
                                        padding: EdgeInsets.symmetric(40, 60),
                                        color: counter % 2 == 0
                                            ? Color.fromARGB(255, 0, 255, 0)
                                            : Color.fromARGB(255, 0, 0, 255),
                                        child: new Text("双击按钮",
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
}
