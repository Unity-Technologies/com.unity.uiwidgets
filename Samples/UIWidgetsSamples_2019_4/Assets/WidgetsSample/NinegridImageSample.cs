using System.Collections.Generic;
using Unity.UIWidgets.engine;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.ui;
using Unity.UIWidgets.widgets;
using Color = Unity.UIWidgets.ui.Color;
using Image = Unity.UIWidgets.widgets.Image;
using ui_ = Unity.UIWidgets.widgets.ui_;

namespace UIWidgetsSample {
    public class NinegridImageSample : UIWidgetsPanel {
        protected override void main() {
            ui_.runApp(new NinegridWidgetApp());
        }
    }
    
    class NinegridWidgetApp : StatelessWidget
    {
        public override Widget build(BuildContext context)
        {
            return new WidgetsApp(
                color: Color.white,
                home: new NinegridWidget(),
                pageRouteBuilder: (settings, builder) =>
                    new PageRouteBuilder(
                        settings: settings,
                        pageBuilder: (Buildcontext, animation, secondaryAnimation) => builder(context)
                    )
            );
        }
    }

    public class NinegridWidget : StatefulWidget {
        public override State createState() {
            return new NinegridWidgetState();
        }
    }

    class NinegridWidgetState : State<NinegridWidget> {
        public override Widget build(BuildContext context) {
            return new Center(
                child: new Column(
                        children: new List<Widget>
                        {
                            Image.file(
                                "button.png",
                                height: 100,
                                width: 350,
                                centerSlice: Rect.fromLTRB(15, 15, 35, 35)
                                ),
                            Image.file(
                                "button.png",
                                height: 100,
                                width: 350,
                                fit: BoxFit.fill
                                )
                        }
                    )
                );
        }
    }
}