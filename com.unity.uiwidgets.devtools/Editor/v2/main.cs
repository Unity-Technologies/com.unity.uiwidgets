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
        protected override void main()
        {
            var preferences = new PreferencesController();
            ui_.runApp(
                new DevToolsApp(AppUtils.defaultScreens, preferences)
            );
        }
    }
}