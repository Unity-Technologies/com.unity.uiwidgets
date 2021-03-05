using UIWidgetsGallery.gallery;
using Unity.UIWidgets.Editor;
using UnityEditor;
using ui_ = Unity.UIWidgets.widgets.ui_;

namespace UIWidgetsEditorWindowSample
{
    public class EditorWindowGallery : UIWidgetsEditorPanel
    {
        [MenuItem("UIWidgets/EditorSample")]
        public static void CountDemo()
        {
            CreateWindow<EditorWindowGallery>();
        }
        
        protected override void main()
        {
            ui_.runApp(new GalleryApp());
        }
    }
}