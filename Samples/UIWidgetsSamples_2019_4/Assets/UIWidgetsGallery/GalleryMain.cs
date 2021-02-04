using UIWidgetsGallery.gallery;
using Unity.UIWidgets.engine2;
using ui_ = Unity.UIWidgets.widgets.ui_;

namespace UIWidgetsGallery {
    public class GalleryMain : UIWidgetsPanel {
        protected override void main() {
            ui_.runApp(new GalleryApp());
        }

        protected new void OnEnable() {
            base.OnEnable();
        }
    }
}
