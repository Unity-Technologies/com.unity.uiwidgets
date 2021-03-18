/*using System;
using uiwidgets;
using Unity.UIWidgets.DevTools.config_specific.ide_theme;
using Unity.UIWidgets.DevTools.inspector.layout_explorer.ui;
using Unity.UIWidgets.material;
using Unity.UIWidgets.ui;
using Unity.UIWidgets.widgets;

namespace Unity.UIWidgets.DevTools
{
    public class CommonThemeUtils
    {
        public static readonly float denseSpacing = 8.0f;
        public static readonly float defaultIconSize = 16.0f;
        public static readonly TimeSpan defaultDuration = TimeSpan.FromMilliseconds(200);
        public static bool isLight = false;
        public static readonly NeverScrollableScrollPhysics defaultTabBarViewPhysics = new NeverScrollableScrollPhysics();
        public static readonly Color devtoolsError = new Color(0xFFAF4054);
        
        
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
        
        // ThemeData themeFor(
        //     bool isDarkTheme,
        //     IdeTheme ideTheme
        // ) {
        //     // If the theme specifies a background color, use it to infer a theme.
        //     if (isValidDarkColor(ideTheme?.backgroundColor)) {
        //         return _darkTheme(ideTheme);
        //     } else if (isValidLightColor(ideTheme?.backgroundColor)) {
        //         return _lightTheme(ideTheme);
        //     }
        //
        //     return isDarkTheme ? _darkTheme(ideTheme) : _lightTheme(ideTheme);
        // }
        
        // bool isValidDarkColor(Color color) {
        //     if (color == null) {
        //         return false;
        //     }
        //     return color.computeLuminance() <= _lightDarkLuminanceThreshold;
        // }
        
    }
}*/