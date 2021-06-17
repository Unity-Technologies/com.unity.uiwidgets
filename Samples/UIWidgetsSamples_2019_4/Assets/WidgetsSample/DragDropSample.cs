using System.Collections.Generic;
using uiwidgets;
using Unity.UIWidgets.engine;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.rendering;
using Unity.UIWidgets.ui;
using Unity.UIWidgets.widgets;
using UnityEngine;
using Color = Unity.UIWidgets.ui.Color;
using ui_ = Unity.UIWidgets.widgets.ui_;

namespace UIWidgetsSample {
    public class DragDropSample : UIWidgetsPanel {
        protected override void main() {
            ui_.runApp(new MyApp());
        }
        
        class MyApp : StatelessWidget
        {
            public override Widget build(BuildContext context)
            {
                return new WidgetsApp(
                    color: Color.white,
                    home: new DragDropApp(),
                    pageRouteBuilder: (settings, builder) =>
                        new PageRouteBuilder(
                            settings: settings,
                            pageBuilder: (Buildcontext, animation, secondaryAnimation) => builder(context)
                        )
                );
            }
        }

        class DragDropApp : StatefulWidget {
            public DragDropApp(Key key = null) : base(key) {
            }

            public override State createState() {
                return new DragDropState();
            }
        }

        class DragTargetWidget : StatefulWidget {
            public DragTargetWidget(Key key = null) : base(key) {
            }

            public override State createState() {
                return new DragTargetWidgetState();
            }
        }

        class DragTargetWidgetState : State<DragTargetWidget> {
            int value;

            public override Widget build(BuildContext context)
            {
                return new Positioned(
                    left: 40.0f,
                    bottom: 40.0f,
                    child: new DragTarget<int>(
                        onWillAccept: (o) => true,
                        onAccept: obj => {
                            Debug.Log("ON ACCEPTED ..." + obj);
                            this.setState(() => { this.value += obj; });
                        },
                        builder: (inner_context2, accepted, rejected) => {
                            return new Container(
                                width: 40.0f,
                                height: 40.0f,
                                constraints: BoxConstraints.tight(new Size(40, 40)),
                                color: Colors.red,
                                child: new Center(child: new Text("" + this.value))
                            );
                        }
                    )
                );
            }
        }

        class DragDropState : State<DragDropApp> {
            public override Widget build(BuildContext context) {
                var entries = new List<OverlayEntry>();

                var entry_bg = new OverlayEntry(
                    inner_context => new Container(
                        color: Colors.white
                    ));

                var entry = new OverlayEntry(
                    inner_context => new Positioned(
                        left: 0.0f,
                        bottom: 0.0f,
                        child: new GestureDetector(
                            onTap: () => { },
                            child: new Draggable<int>(
                                data: 5,
                                child: new Container(
                                    color: Colors.blue,
                                    width: 30.0f,
                                    height: 30.0f,
                                    constraints: BoxConstraints.tight(new Size(30, 30)),
                                    child: new Center(child: new Text("5"))
                                ),
                                feedback: new Container(
                                    color: Colors.green,
                                    width: 30.0f,
                                    height: 30.0f),
                                //maxSimultaneousDrags: 1,
                                childWhenDragging: new Container(
                                    color: Colors.black,
                                    width: 30.0f,
                                    height: 30.0f,
                                    constraints: BoxConstraints.tight(new Size(30, 30))
                                )
                            )
                        )
                    )
                );

                var entry3 = new OverlayEntry(
                    inner_context => new Positioned(
                        left: 0.0f,
                        bottom: 40.0f,
                        child: new GestureDetector(
                            onTap: () => { },
                            child:
                            new Draggable<int>(
                                data: 8,
                                child: new Container(
                                    color: Colors.grey,
                                    width: 30.0f,
                                    height: 30.0f,
                                    constraints: BoxConstraints.tight(new Size(30, 30)),
                                    child: new Center(child: new Text("8")))
                                ,
                                feedback: new Container(
                                    color: Colors.green,
                                    width: 30.0f,
                                    height: 30.0f),
                                maxSimultaneousDrags: 1,
                                childWhenDragging: new Container(
                                    color: Colors.black,
                                    width: 30.0f,
                                    height: 30.0f,
                                    constraints: BoxConstraints.tight(new Size(30, 30))
                                )
                            )
                        )
                    )
                );

                var entry2 = new OverlayEntry(
                    inner_context => new DragTargetWidget()
                );

                entries.Add(entry_bg);
                entries.Add(entry);
                entries.Add(entry2);
                entries.Add(entry3);

                return new Container(
                    color: Colors.white,
                    child: new Overlay(
                        initialEntries: entries
                    )
                );
            }
        }
    }
}