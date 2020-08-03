using System.Collections.Generic;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.material;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.ui;
using Unity.UIWidgets.widgets;
using UnityEngine;
using Color = Unity.UIWidgets.ui.Color;
using Material = Unity.UIWidgets.material.Material;

namespace UIWidgetsSample {
    
    public class MaterialButtonSample : UIWidgetsSamplePanel {

        protected override Widget createWidget() {
            return new MaterialApp(
                showPerformanceOverlay: false,
                home: new MaterialButtonWidget());
        }

        protected override void OnEnable() {
            FontManager.instance.addFont(Resources.Load<Font>(path: "fonts/MaterialIcons-Regular"), "Material Icons");
            base.OnEnable();
        }
    }
    
    public class MaterialButtonWidget : StatefulWidget {
        public MaterialButtonWidget(Key key = null) : base(key) {
        }

        public override State createState() {
            return new MaterialButtonWidgetState();
        }
    }

    public class MaterialButtonWidgetState : State<MaterialButtonWidget> {
        public override Widget build(BuildContext context) {
            return new Stack(
                children: new List<Widget> {
                    new Material(
                        child: new Center(
                            child: new Column(
                                children: new List<Widget> {
                                    new Padding(padding: EdgeInsets.only(top: 30f)),
                                    new MaterialButton(
                                        shape: new RoundedRectangleBorder(borderRadius: BorderRadius.all(20.0f)),
                                        color: new Color(0xFF00FF00),
                                        splashColor: new Color(0xFFFF0011),
                                        highlightColor: new Color(0x88FF0011),
                                        child: new Text("Click Me"),
                                        onPressed: () => { Debug.Log("pressed flat button"); }
                                    ),
                                    new Padding(padding: EdgeInsets.only(top: 30f)),
                                    new MaterialButton(
                                        shape: new RoundedRectangleBorder(borderRadius: BorderRadius.all(20.0f)),
                                        color: new Color(0xFFFF00FF),
                                        splashColor: new Color(0xFFFF0011),
                                        highlightColor: new Color(0x88FF0011),
                                        elevation: 4.0f,
                                        child: new Text("Click Me"),
                                        onPressed: () => { Debug.Log("pressed raised button"); }
                                    )
                                }
                            )
                        )
                    ),
                    new PerformanceOverlay()
                }
            );
        }
    }

}