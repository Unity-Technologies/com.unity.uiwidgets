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
using Texture = Unity.UIWidgets.widgets.Texture;
using ui_ = Unity.UIWidgets.widgets.ui_;

namespace UIWidgetsSample
{
    public class SceneTest : UIWidgetsPanel
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
                    color: Color.white,
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
            private float frame = 0;

            public override Widget build(BuildContext context)
            {
                var text = GameObject.Find("CameraTarget")?.GetComponent<UnityEngine.UI.RawImage>()?.texture;
                return new Container(
                    child: new Column(
                        children: new List<Widget>
                        {
                            AnimatedLottie.file("wine.json", frame: frame, curve: Curves.linear),
                            new Container(width: 100, height: 100, child:new Texture(texture: text)),
                            new GestureDetector(
                                onTap: () => { setState(() => { frame += 1; }); },
                                child: new Container(
                                    color: Color.black,
                                    padding: EdgeInsets.symmetric(20, 20),
                                    child: new Text("Click Me",
                                        style: new TextStyle(fontWeight: FontWeight.w700))
                                )
                            )
                        }
                    )
                );
            }
        }
    }
}