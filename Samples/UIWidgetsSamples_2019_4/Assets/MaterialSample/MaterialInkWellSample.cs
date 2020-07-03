using Unity.UIWidgets.foundation;
using Unity.UIWidgets.material;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.ui;
using Unity.UIWidgets.widgets;
using UnityEngine;
using Color = Unity.UIWidgets.ui.Color;
using Material = Unity.UIWidgets.material.Material;

namespace UIWidgetsSample {
    
    public class MaterialInkWellSample : UIWidgetsSamplePanel {

        protected override Widget createWidget() {
            return new MaterialApp(
                showPerformanceOverlay: false,
                home: new MaterialInkWellWidget());
        }

        protected override void OnEnable() {
            FontManager.instance.addFont(Resources.Load<Font>(path: "fonts/MaterialIcons-Regular"), "Material Icons");
            base.OnEnable();
        }
    }
    
    public class MaterialInkWellWidget : StatefulWidget {
        public MaterialInkWellWidget(Key key = null) : base(key) {
        }

        public override State createState() {
            return new MaterialInkWidgetState();
        }
    }

    public class MaterialInkWidgetState : State<MaterialInkWellWidget> {
        public override Widget build(BuildContext context) {
            return new Material(
                //color: Colors.blue,
                child: new Center(
                    child: new Container(
                        width: 200,
                        height: 200,
                        child: new InkWell(
                            borderRadius: BorderRadius.circular(2.0f),
                            highlightColor: new Color(0xAAFF0000),
                            splashColor: new Color(0xAA0000FF),
                            onTap: () => { Debug.Log("on tap"); }
                        )
                    )
                )
            );
        }
    }

}