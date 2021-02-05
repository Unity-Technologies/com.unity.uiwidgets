using System;
using UIWidgetsGallery.demo.shrine;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.widgets;

namespace UIWidgetsGallery.demo
{
    class ShrineDemo : StatelessWidget {
    public ShrineDemo(Key key = null) : base(key: key){}

    public static readonly string routeName = "/shrine"; // Used by the Gallery app.

    public override Widget build(BuildContext context) => new ShrineApp();
    }
}