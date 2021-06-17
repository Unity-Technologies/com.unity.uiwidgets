using uiwidgets;
using Unity.UIWidgets.engine;
using Unity.UIWidgets.widgets;
using UnityEngine;
using Color = Unity.UIWidgets.ui.Color;

namespace UIWidgetsSample {
    public class LongPressSample : UIWidgetsPanel {
        protected override void main() {
            ui_.runApp(new MyApp());
        }
    }
    
    class MyApp : StatelessWidget
    {
        public override Widget build(BuildContext context)
        {
            return new WidgetsApp(
                color: Color.white,
                home: new LongPressSampleWidget(),
                pageRouteBuilder: (settings, builder) =>
                    new PageRouteBuilder(
                        settings: settings,
                        pageBuilder: (Buildcontext, animation, secondaryAnimation) => builder(context)
                    )
            );
        }
    }

    public class LongPressSampleWidget : StatefulWidget {
        public override State createState() {
            return new _LongPressSampleWidgetState();
        }
    }

    class _LongPressSampleWidgetState : State<LongPressSampleWidget> {
        public override Widget build(BuildContext context) {
            return new GestureDetector(
                onLongPressStart: (value) => { Debug.Log($"Long Press Drag Start: {value}"); },
                onLongPressMoveUpdate: (value) => { Debug.Log($"Long Press Drag Update: {value}"); },
                onLongPressEnd: (value) => { Debug.Log($"Long Press Drag Up: {value}"); },
                onLongPressUp: () => { Debug.Log($"Long Press Up"); },
                onLongPress: () => { Debug.Log($"Long Press"); },
                child: new Center(
                    child: new Container(
                        width: 200,
                        height: 200,
                        color: Colors.blue
                    )
                )
            );
        }
    }
}