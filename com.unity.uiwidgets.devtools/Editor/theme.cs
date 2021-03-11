using System;
using uiwidgets;
using Unity.UIWidgets.DevTools.inspector.layout_explorer.ui;
using Unity.UIWidgets.ui;

namespace Unity.UIWidgets.DevTools
{
    public class CommonThemeUtils
    {
        public static readonly float denseSpacing = 8.0f;
        public static readonly float defaultIconSize = 16.0f;
        public static readonly TimeSpan defaultDuration = TimeSpan.FromMilliseconds(200);
        public static bool isLight = false;
        
        public static Color defaultBackground
        {
            get
            {
                return isLight ? Colors.white : Colors.black;
            }
        }

        public static Color defaultForeground
        {
            get
            {
                return isLight ? Colors.black : Color.fromARGB(255, 187, 187, 187);
            }
        }
        
    }
}