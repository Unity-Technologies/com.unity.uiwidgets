using System;
using UIWidgetsGallery.demo.shrine;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.widgets;

namespace UIWidgetsGallery.demo
{
    class ShrineDemo : StatelessWidget 
    {
        public ShrineDemo(Key key = null) : base(key: key){}

        public static readonly string routeName = "/shrine";

        public override Widget build(BuildContext context)
        {
            return new ShrineApp();
        }
    }
}