using Unity.UIWidgets.DevTools.config_specific.framework_initialize;
using Unity.UIWidgets.Editor;
using Unity.UIWidgets.widgets;

namespace Unity.UIWidgets.DevTools
{
    public class Devetool : UIWidgetsEditorPanel
    {
        protected override void main()
        {
            var preferences = new PreferencesController();
            
            preferences.init();
            FrameworkInitializeUtils.initializeFramework();
            
            ui_.runApp(
                new DevToolsApp(defaultScreens, preferences)
            );
        }
    }
}