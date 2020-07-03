using UIWidgetsGallery.gallery;
using Unity.UIWidgets.editor;
using Unity.UIWidgets.ui;
using Unity.UIWidgets.widgets;
using UnityEditor;
using UnityEngine;

namespace UIWidgetsGallery {
    public class GalleryMainEditor : UIWidgetsEditorWindow {
        
        [MenuItem("UIWidgetsTests/Gallery")]
        public static void gallery() {
            EditorWindow.GetWindow<GalleryMainEditor>();
        }
        
        protected override Widget createWidget() {
            return new GalleryApp();
        }
        
        protected override void OnEnable() {
            FontManager.instance.addFont(Resources.Load<Font>("fonts/MaterialIcons-Regular"), "Material Icons");
            FontManager.instance.addFont(Resources.Load<Font>("fonts/GalleryIcons"), "GalleryIcons");
            
            FontManager.instance.addFont(Resources.Load<Font>("fonts/CupertinoIcons"), "CupertinoIcons");
            FontManager.instance.addFont(Resources.Load<Font>(path: "fonts/SF-Pro-Text-Regular"), ".SF Pro Text", FontWeight.w400);
            FontManager.instance.addFont(Resources.Load<Font>(path: "fonts/SF-Pro-Text-Semibold"), ".SF Pro Text", FontWeight.w600);
            FontManager.instance.addFont(Resources.Load<Font>(path: "fonts/SF-Pro-Text-Bold"), ".SF Pro Text", FontWeight.w700);
            base.OnEnable();
        }
    }
}
