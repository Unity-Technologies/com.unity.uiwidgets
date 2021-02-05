using System.Collections.Generic;
using System.Linq;
using UIWidgetsGallery.gallery;
using Unity.UIWidgets.material;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.widgets;

namespace UIWidgetsGallery.demo.material
{
    internal class ElevationDemo : StatefulWidget
    {
        public static readonly string routeName = "/material/elevation";

        public override State createState()
        {
            return new _ElevationDemoState();
        }
    }

    internal class _ElevationDemoState : State<ElevationDemo>
    {
        private bool _showElevation = true;

        private List<Widget> buildCards()
        {
            List<float> elevations = new List<float>
            {
                0.0f,
                1.0f,
                2.0f,
                3.0f,
                4.0f,
                5.0f,
                8.0f,
                16.0f,
                24.0f
            };

            return elevations.Select<float, Widget>((float elevation) =>
            {
                return new Center(
                    child: new Card(
                        margin: EdgeInsets.all(20.0f),
                        elevation: this._showElevation ? elevation : 0.0f,
                        child: new SizedBox(
                            height: 100.0f,
                            width: 100.0f,
                            child: new Center(
                                child: new Text($"{elevation:F0} pt")
                            )
                        )
                    )
                );
            }).ToList();
        }

        public override Widget build(BuildContext context)
        {
            return new Scaffold(
                appBar: new AppBar(
                    title: new Text("Elevation"),
                    actions: new List<Widget>
                    {
                        new MaterialDemoDocumentationButton(ElevationDemo.routeName),
                        new IconButton(
                            icon: new Icon(Icons.sentiment_very_satisfied),
                            onPressed: () => { this.setState(() => this._showElevation = !this._showElevation); }
                        )
                    }
                ),
                body: new Scrollbar(child: new ListView(children: this.buildCards()))
            );
        }
    }
}