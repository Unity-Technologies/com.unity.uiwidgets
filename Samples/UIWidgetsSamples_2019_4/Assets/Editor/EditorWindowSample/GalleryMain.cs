using System.Collections.Generic;
using UIWidgetsGallery.gallery;
using Unity.UIWidgets.Editor;
using Unity.UIWidgets.engine2;
using Unity.UIWidgets.widgets;
using UnityEditor;
using UnityEngine;

namespace UIWidgetsEditorWindowSample
{
    public class EditorWindowGallery : UIWidgetsEditorPanel
    {

        [MenuItem("UIWidgets/EditorSample/GalleryMain")]
        public static void CountDemo()
        {
            CreateWindow<EditorWindowGallery>();
        }

        protected override void onEnable()
        {
            AddFont("Material Icons", new List<string> {"MaterialIcons-Regular.ttf"}, new List<int> {0});
            AddFont("CupertinoIcons", new List<string> {"CupertinoIcons.ttf"}, new List<int> {0});
            AddFont("GalleryIcons", new List<string> {"gallery/GalleryIcons.ttf"}, new List<int> {0});
        }

        protected override void main()
        {
            ui_.runApp(new GalleryApp());
        }
    }
}