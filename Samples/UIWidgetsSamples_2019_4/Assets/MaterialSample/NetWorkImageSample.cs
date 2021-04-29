using System.Collections;
using System.Collections.Generic;
using Unity.UIWidgets.engine;
using Unity.UIWidgets.material;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.widgets;
using UnityEngine;

namespace UIWidgetsSample
{
    public class NetWorkImageSample : UIWidgetsPanel
    {
        protected override void main()
        {
            ui_.runApp(new MaterialApp(
                showPerformanceOverlay: false,
                home: new NetWorkeImageWidget()));
        }

        class NetWorkeImageWidget : StatelessWidget { 
                public override Widget build(BuildContext context) {
                var title = "NetWork Image";

                return new MaterialApp(
                    title: title,
                    home: new Scaffold(
                        appBar: new AppBar(
                            title: new Text(title)
                        ),
                        body: Image.network("https://picsum.photos/250?image=2")
                    )
                );
            }
        }
    }

}



