using System.Collections.Generic;
using uiwidgets;
using Unity.UIWidgets.engine;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.ui;
using Unity.UIWidgets.widgets;
using Image = Unity.UIWidgets.widgets.Image;
using ui_ = Unity.UIWidgets.widgets.ui_;

namespace UIWidgetsSample
{
    public class ImageFormatSample : UIWidgetsPanel
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
                    home: new ImageFormatApp(),
                    color: Color.white,
                    pageRouteBuilder: (settings, builder) =>
                        new PageRouteBuilder(
                            settings: settings,
                            pageBuilder: (buildContext, animation, secondaryAnimation) => builder(context)
                        )
                );
            }
        }

        class ImageFormatApp : StatefulWidget
        {
            public override State createState()
            {
                return new ImageFormatAppState();
            }
        }

        class ImageFormatAppState : State<ImageFormatApp>
        {
            private int imageId;

            private static List<string> imagePath = new List<string>
            {
                "shrine_images/0-0.jpg",
                "shrine_images/2.0x/1-0.jpg",
                "shrine_images/diamond.png",
                "gallery/people/square/ali.png",
                "gallery/10-0.jpg",
                "gallery/glasses.jpg"
            };


            public override Widget build(BuildContext context)
            {
                return new Container(
                    child: new Column(
                        children: new List<Widget>
                        {
                            new Container(
                                width: 100,
                                height: 100,
                                decoration: new BoxDecoration(
                                    borderRadius: BorderRadius.all(Radius.circular(8))
                                ),
                                child: Image.file(imagePath[imageId])
                            ),
                            new Text(imagePath[imageId]),
                            new GestureDetector(
                                onTap: () => { setState(() => { imageId = (imageId + 1) % imagePath.Count; }); },
                                child: new Container(
                                    color: Color.black,
                                    padding: EdgeInsets.symmetric(20, 20),
                                    child: new Text("Next"
                                )
                            ))
                        }
                        ));
            }
        }
    }
    
    
}