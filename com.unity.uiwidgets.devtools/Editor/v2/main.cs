using System.Collections.Generic;
using Unity.UIWidgets.DevTools.config_specific.framework_initialize;
using Unity.UIWidgets.Editor;
using Unity.UIWidgets.widgets;
using UnityEditor;

namespace Unity.UIWidgets.DevTools
{
    public class EditorWindowDevtools : UIWidgetsEditorPanel
    {
        
        [MenuItem("UIWidgets/DevTools")]
        public static void CountDemo()
        {
            CreateWindow<EditorWindowDevtools>();
        }
        
        protected override void onEnable()
        {
            AddFont("Material Icons", new List<string> {"MaterialIcons-Regular.ttf"}, new List<int> {0});
        }
        
        protected override void main()
        {
            var preferences = new PreferencesController();
            FrameworkInitializeUtils.initializeFramework();
            ui_.runApp(
                new DevToolsApp(AppUtils.defaultScreens, preferences)
            );
        }
    }
}