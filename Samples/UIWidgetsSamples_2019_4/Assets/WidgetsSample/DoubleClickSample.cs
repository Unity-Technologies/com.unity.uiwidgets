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
    public class DoubleClickSample : UIWidgetsPanel {
        protected override void main() {
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

        class DoubleClickState : State<DoubleClickApp> {
            int _doubleClickCount = 0;

            void _toggleDoubleClick()
            {
                Debug.Log("ON DOUBLECLICK ...");
                setState(() =>
                {
                    _doubleClickCount += 1;
                });
                print("double tap");
            }

            public override Widget build(BuildContext context) {
                var entries = new List<OverlayEntry>();
                var entry = new OverlayEntry(
                    inner_context => new Center(
                        child: new GestureDetector(
                            onTap:() => {},
                            onDoubleTap: _toggleDoubleClick,
                            child: new Container(
                                    color: Colors.green,
                                    width: 40.0f,
                                    height: 40.0f,
                                    constraints: BoxConstraints.tight(new Size(40, 40)),
                                    child: new Center(child: new Text(_doubleClickCount.ToString()))
                            )

                        )
                    )
                );
                
                entries.Add(entry);

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