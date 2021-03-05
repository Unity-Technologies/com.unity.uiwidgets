using System.Collections.Generic;
using UIWidgetsGallery.gallery;
using Unity.UIWidgets.engine2;
using ui_ = Unity.UIWidgets.widgets.ui_;

namespace UIWidgetsGallery {
    public class GalleryMain : UIWidgetsPanel {
        protected override void main() {
            ui_.runApp(new GalleryApp());
        }

        protected override void loadConfiguration()
        {
            AddFont("Material Icons", new List<string>{"MaterialIcons-Regular.ttf"}, new List<int>{0});
            AddFont("CupertinoIcons", new List<string>{"CupertinoIcons.ttf"}, new List<int>{0});
            AddFont("GalleryIcons", new List<string>{"gallery/GalleryIcons.ttf"}, new List<int>{0,1});
            AddFont("1", new List<string>{"gallery/GalleryIcons.ttf"}, new List<int>{0});
        }
        
        protected new void OnEnable() {
            base.OnEnable();
        }
    }
}
