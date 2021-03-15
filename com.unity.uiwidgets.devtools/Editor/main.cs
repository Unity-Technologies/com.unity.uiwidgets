

using Unity.UIWidgets.DevTools.analytics;
using Unity.UIWidgets.DevTools.config_specific.ide_theme;
using Unity.UIWidgets.Editor;
using Unity.UIWidgets.widgets;

namespace Unity.UIWidgets.DevTools
{
    public class Devetool : UIWidgetsEditorPanel
    {
        
        public IdeTheme ideTheme = null;
        protected override void main()
        {
            
            ui_.runApp(new DevToolsApp(appUtils.defaultScreens, ideTheme, stub_provider.analyticsProvider));
        }
    }
}