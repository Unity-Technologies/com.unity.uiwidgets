using uiwidgets;
using Unity.UIWidgets.material;
using Unity.UIWidgets.ui;

namespace UIWidgetsGallery.gallery
{
    public static class GalleyThemes
    {
        public static readonly ThemeData kLightGalleryTheme = _buildLightTheme();
        public static readonly ThemeData kDarkGalleryTheme = _buildDarkTheme();

        private static string _defaultGalleryFontFamily = "";

        private static TextTheme _buildTextTheme(TextTheme baseTheme)
        {
            if (_defaultGalleryFontFamily != "")
                return baseTheme.copyWith(
                    headline6: baseTheme.headline6.copyWith(
                        fontFamily: _defaultGalleryFontFamily
                    )
                );

            return baseTheme;
        }

        private static ThemeData _buildDarkTheme()
        {
            Color primaryColor = new Color(0xFF0175c2);
            Color secondaryColor = new Color(0xFF13B9FD);
            ColorScheme colorScheme = ColorScheme.dark().copyWith(
                primary: primaryColor,
                secondary: secondaryColor
            );
            ThemeData baseTheme = new ThemeData(
                brightness: Brightness.dark,
                accentColorBrightness: Brightness.dark,
                primaryColor: primaryColor,
                primaryColorDark: new Color(0xFF0050a0),
                primaryColorLight: secondaryColor,
                buttonColor: primaryColor,
                indicatorColor: Colors.white,
                toggleableActiveColor: new Color(0xFF6997DF),
                accentColor: secondaryColor,
                canvasColor: new Color(0xFF202124),
                scaffoldBackgroundColor: new Color(0xFF202124),
                backgroundColor: new Color(0xFF202124),
                errorColor: new Color(0xFFB00020),
                buttonTheme: new ButtonThemeData(
                    colorScheme: colorScheme,
                    textTheme: ButtonTextTheme.primary
                )
            );
            return baseTheme.copyWith(
                textTheme: _buildTextTheme(baseTheme.textTheme),
                primaryTextTheme: _buildTextTheme(baseTheme.primaryTextTheme),
                accentTextTheme: _buildTextTheme(baseTheme.accentTextTheme)
            );
        }

        private static ThemeData _buildLightTheme()
        {
            Color primaryColor = new Color(0xFF0175c2);
            Color secondaryColor = new Color(0xFF13B9FD);
            ColorScheme colorScheme = ColorScheme.light().copyWith(
                primary: primaryColor,
                secondary: secondaryColor
            );
            ThemeData baseTheme = new ThemeData(
                brightness: Brightness.light,
                accentColorBrightness: Brightness.dark,
                colorScheme: colorScheme,
                primaryColor: primaryColor,
                buttonColor: primaryColor,
                indicatorColor: Colors.white,
                toggleableActiveColor: new Color(0xFF1E88E5),
                splashColor: Colors.white24,
                splashFactory: InkRipple.splashFactory,
                accentColor: secondaryColor,
                canvasColor: Colors.white,
                scaffoldBackgroundColor: Colors.white,
                backgroundColor: Colors.white,
                errorColor: new Color(0xFFB00020),
                buttonTheme: new ButtonThemeData(
                    colorScheme: colorScheme,
                    textTheme: ButtonTextTheme.primary
                )
            );
            return baseTheme.copyWith(
                textTheme: _buildTextTheme(baseTheme.textTheme),
                primaryTextTheme: _buildTextTheme(baseTheme.primaryTextTheme),
                accentTextTheme: _buildTextTheme(baseTheme.accentTextTheme)
            );
        }
    }
}