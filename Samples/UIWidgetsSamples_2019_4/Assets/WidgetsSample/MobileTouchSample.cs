using System;
using System.Collections.Generic;
using uiwidgets;
using Unity.UIWidgets.engine;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.material;
using Unity.UIWidgets.rendering;
using Unity.UIWidgets.ui;
using Unity.UIWidgets.widgets;
using UnityEngine;
using UnityEngine.Networking;
using Image = Unity.UIWidgets.widgets.Image;
using ui_ = Unity.UIWidgets.widgets.ui_;

namespace UIWidgetsSample
{
    public class MobileTouchSample : UIWidgetsPanel
    {
        protected override void main()
        {
            ui_.runApp(new MaterialApp(
                title: "Http Request Sample",
                home: new Scaffold(
                    body: new MobileTouchWidget()
                )
            ));
        }
    }

    public class MobileTouchWidget : StatefulWidget
    {
        public MobileTouchWidget(Key key = null) : base(key)
        {
        }

        public override State createState()
        {
            return new MobileTouchWidgetState();
        }
    }

    class MobileTouchWidgetState : State<MobileTouchWidget>
    {
        private float scale = 1;
        private int frameNo = 0;
        private float rotation = 0;

        public override Widget build(BuildContext context)
        {
            return new Column(
                crossAxisAlignment: CrossAxisAlignment.center,
                children: new List<Widget>()
                {
                    new Text("Frame: " + frameNo),
                    new Text("Scale: " + scale),
                    new Text("Rotation: " + rotation),
                    new GestureDetector(
                        child: new Container(height: 300, color: Colors.blue),
                        onScaleStart: details => { },
                        onScaleUpdate: details =>
                        {
                            scale = details.scale;
                            rotation = details.rotation;
                            frameNo += 1;
                            setState(() => { });
                        },
                        onScaleEnd: details => { }
                    )
                });
        }
    }
}