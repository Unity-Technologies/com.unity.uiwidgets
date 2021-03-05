using System.Collections.Generic;
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

        protected override void loadConfiguration()
        {
            AddFont("Material Icons", new List<string>{"MaterialIcons-Regular.ttf"}, new List<int>{0});
            AddFont("CupertinoIcons", new List<string>{"CupertinoIcons.ttf"}, new List<int>{0});
            AddFont("GalleryIcons", new List<string>{"gallery/GalleryIcons.ttf"}, new List<int>{0,1});
        }

        protected override void main()
        {
            ui_.runApp(new GalleryApp());
        }
    }
}