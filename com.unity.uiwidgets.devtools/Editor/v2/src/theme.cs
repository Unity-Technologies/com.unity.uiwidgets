using System.Collections.Generic;
using uiwidgets;
using Unity.UIWidgets.material;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.ui;

namespace Unity.UIWidgets.DevTools
{
    public class ThemeUtils
    {
        public static ThemeData themeFor(bool isDarkTheme) {
            return isDarkTheme ? _darkTheme() : _lightTheme();
        }
        
        
        static ThemeData _darkTheme() {
            var theme = ThemeData.dark();
            return theme.copyWith(
                primaryColor: devtoolsGrey[900],
                primaryColorDark: devtoolsBlue[700],
                primaryColorLight: devtoolsBlue[400],
                indicatorColor: devtoolsBlue[400],
                accentColor: devtoolsBlue[400],
                backgroundColor: devtoolsGrey[600],
                toggleableActiveColor: devtoolsBlue[400],
                selectedRowColor: devtoolsGrey[600],
                buttonTheme: theme.buttonTheme.copyWith(minWidth: buttonMinWidth)
            );
        }

        static ThemeData _lightTheme() {
            var theme = ThemeData.light();
            return theme.copyWith(
                primaryColor: devtoolsBlue[600],
                primaryColorDark: devtoolsBlue[700],
                primaryColorLight: devtoolsBlue[400],
                indicatorColor: Colors.yellowAccent[400],
                accentColor: devtoolsBlue[400],
                backgroundColor: devtoolsGrey[600],
                toggleableActiveColor: devtoolsBlue[400],
                selectedRowColor: devtoolsBlue[600],
                buttonTheme: theme.buttonTheme.copyWith(minWidth: buttonMinWidth)
            );
        }
        
        public const float defaultIconSize = 16.0f;
        public const float buttonMinWidth = 36.0f;
        
        static ColorSwatch<int> devtoolsGrey = new ColorSwatch<int>(0xFF202124, new Dictionary<int, Color>(){
            {900, new Color(0xFF202124)},
            {600, new Color(0xFF60646B)},
            {100, new Color(0xFFD5D7Da)},
            {50, new Color(0xFFEAEBEC)}
        });
    
    static ColorSwatch<int> devtoolsYellow = new ColorSwatch<int>(700, new Dictionary<int, Color>(){
        {700, new Color(0xFFFFC108)}
    });
    
    static ColorSwatch<int> devtoolsBlue = new ColorSwatch<int>(600, new Dictionary<int, Color>(){
        {700, new Color(0xFF02569B)},
            {600, new Color(0xFF0175C2)},
            {400, new Color(0xFF13B9FD)},
    });
    }
}