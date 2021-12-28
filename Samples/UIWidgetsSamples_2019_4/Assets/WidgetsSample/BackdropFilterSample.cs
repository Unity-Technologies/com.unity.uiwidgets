using uiwidgets;
using Unity.UIWidgets.engine;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.ui;
using Unity.UIWidgets.widgets;
using Color = Unity.UIWidgets.ui.Color;
using ui_ = Unity.UIWidgets.widgets.ui_;

namespace UIWidgetsSample {
    public class BackdropFilterSample : UIWidgetsPanel {
        protected override void main() {
            ui_.runApp(new MyApp());
        }
        
        class MyApp : StatelessWidget
        {
            public override Widget build(BuildContext context)
            {
                return new WidgetsApp(
                    color: Color.white,
                    home: new BackdropFilterApp(),
                    pageRouteBuilder: (settings, builder) =>
                        new PageRouteBuilder(
                            settings: settings,
                            pageBuilder: (Buildcontext, animation, secondaryAnimation) => builder(context)
                        )
                );
            }
        }

        public class BackdropFilterApp : StatefulWidget {
            public BackdropFilterApp(Key key = null) : base(key) {
            }

            public override State createState() {
                return new BackdropFilterAppState();
            }
        }

        public class BackdropFilterAppState : State<BackdropFilterApp> {
            public override Widget build(BuildContext context)
            {
                var item = new Container(
                    width: 350,
                    height: 300,
                    decoration: new BoxDecoration(
                        image: new DecorationImage(
                            image: new FileImage("shrine_images/0-0.jpg"),
                            fit: BoxFit.cover
                        )
                    ),
                    child: new BackdropFilter(
                        filter: ImageFilter.blur(sigmaX: 5f, sigmaY: 5f),
                        child: new Container(
                            color: Colors.black.withOpacity(0.1f)
                        )
                    )
                );
                
                return new Container(color: Colors.blue, child:
                    new Center(
                        child: item
                    ));
            }
        }
    }
}