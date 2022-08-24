using System.Collections.Generic;
using uiwidgets;
using Unity.UIWidgets.engine;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.ui;
using Unity.UIWidgets.widgets;
using UnityEngine;
using Color = Unity.UIWidgets.ui.Color;
using Image = Unity.UIWidgets.widgets.Image;
using TextStyle = Unity.UIWidgets.painting.TextStyle;
using Texture = Unity.UIWidgets.widgets.Texture;
using ui_ = Unity.UIWidgets.widgets.ui_;

namespace UIWidgetsSample
{
    namespace UIWidgetsSample
{
    public class Scene3DTest1 : UIWidgetsPanel
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
            public override Widget build(BuildContext context)
            {
                //var text = GameObject.Find("CameraTarget")?.GetComponent<UnityEngine.UI.RawImage>()?.texture;
                ModelViewHelper.Instance?.Load(1, "CubPrefab");
                var text = ModelViewHelper.Instance?.LoadModelViewRT(1);

                return new Container(
                    child: new Column(
                        children: new List<Widget>
                        {
                            new Text("External Texture is Only Available on OpenGLCore (Mac), OpenGLes(iOS and android) and D3d11(Windows)", style: new TextStyle(fontSize: 16f, decoration: TextDecoration.none, color: Colors.red)),
                            new Container(width: 120, height: 120, color: Colors.red, child: new Center(child: new Container(width: 100, height: 100, child:new Texture(texture: text)))),
                            /*new Container(
                                width: 100,
                                height: 100,
                                decoration: new BoxDecoration(
                                    borderRadius: BorderRadius.all(Radius.circular(8))
                                ),
                                child: Image.file("test.gif", gaplessPlayback: true)
                            ), 
                            new Container(
                                width: 200,
                                height: 100,
                                decoration: new BoxDecoration(
                                    borderRadius: BorderRadius.all(Radius.circular(8))
                                ),
                                child: Image.network(
                                    "https://unity-cn-cms-prd-1254078910.cos.ap-shanghai.myqcloud.com/assetstore-cms-media/img-7dfe215f-0075-4f9c-9b5a-be5ee88b866b",
                                    gaplessPlayback: true)
                            )*/
                        }
                    )
                );
            }
        }
    }
}
}