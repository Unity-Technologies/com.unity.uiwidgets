using System.Collections.Generic;
using Unity.UIWidgets.engine;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.ui;
using Unity.UIWidgets.widgets;
using UnityEngine;
using Color = Unity.UIWidgets.ui.Color;
using Image = Unity.UIWidgets.widgets.Image;
using ui_ = Unity.UIWidgets.widgets.ui_;

namespace UIWidgetsSample
{
    public class ImageTest : UIWidgetsPanel
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

            //user are responsible to get the right absolute image path for different platforms
            private string getAbsoluteImagePathForApp(string simplePath)
            {
                var absolutePath = System.IO.Path.Combine(Application.streamingAssetsPath, simplePath);
#if UNITY_ANDROID && !UNITY_EDITOR
                //do nothing
#else
                absolutePath = "file://" + absolutePath;
#endif
                return absolutePath;
            }
            
            public override Widget build(BuildContext context)
            {
                var imageSimplePath = "test.gif";
                var absolutePath = getAbsoluteImagePathForApp(imageSimplePath);

                return new Container(
                    child: new Column(
                        children: new List<Widget>
                        {
                            new Lottie("wine.json", size: new Size(100, 100)),
                            new Container(
                                width: 100,
                                height: 100,
                                decoration: new BoxDecoration(
                                    borderRadius: BorderRadius.all(Radius.circular(8))
                                ),
                                child: Image.file(absolutePath, gaplessPlayback: true, isAbsolutePath: true)
                            ),
                            new Container(width: 50f, height: 50f, child: Image.file(imageSimplePath, gaplessPlayback: true)),
                            new Container(
                                width: 200,
                                height: 100,
                                decoration: new BoxDecoration(
                                    borderRadius: BorderRadius.all(Radius.circular(8))
                                ),
                                child: Image.network(
                                    "https://unity-cn-cms-prd-1254078910.cos.ap-shanghai.myqcloud.com/assetstore-cms-media/img-7dfe215f-0075-4f9c-9b5a-be5ee88b866b",
                                    gaplessPlayback: true)
                            )
                        }
                    )
                );
            }
        }
    }
}