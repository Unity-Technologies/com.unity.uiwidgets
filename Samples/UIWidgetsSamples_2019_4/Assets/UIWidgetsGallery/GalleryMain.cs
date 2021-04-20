using System.Collections.Generic;
using UIWidgetsGallery.gallery;
using Unity.UIWidgets.engine;
using Unity.UIWidgets.widgets;

namespace UIWidgetsGallery
{
    public class GalleryMain : UIWidgetsPanel
    {
        protected new void OnEnable()
        {
            base.OnEnable();
        }

        protected override void main()
        {
            ui_.runApp(new GalleryApp());
        }

        protected override void onEnable()
        {
            AddFont("Material Icons", new List<string> {"MaterialIcons-Regular.ttf"}, new List<int> {0});
            AddFont("CupertinoIcons", new List<string> {"CupertinoIcons.ttf"}, new List<int> {0});
            AddFont("GalleryIcons", new List<string> {"gallery/GalleryIcons.ttf"}, new List<int> {0});
        }
    }
}