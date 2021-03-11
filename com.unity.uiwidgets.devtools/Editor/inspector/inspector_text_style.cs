using uiwidgets;
using Unity.UIWidgets.material;
using Unity.UIWidgets.ui;
using TextStyle = Unity.UIWidgets.painting.TextStyle;
using ThemeUtils = Unity.UIWidgets.DevTools.inspector.layout_explorer.ui.ThemeUtils;

namespace Unity.UIWidgets.DevTools.inspector
{
    public class inspector_text_styles
    {
        public static TextStyle unimportant(ColorScheme colorScheme)
        {
            
            return new TextStyle(
                color: CommonThemeUtils.isLight ? Colors.grey.shade500 : Colors.grey.shade600);
        }

        public static TextStyle regular = new TextStyle();
        public static TextStyle warning(ColorScheme colorScheme) => new TextStyle(
            color:
            CommonThemeUtils.isLight ? Colors.orange.shade900 : Colors.orange.shade400);
        public static TextStyle error(ColorScheme colorScheme) => new TextStyle(
            color: CommonThemeUtils.isLight ? Colors.red.shade500 : Colors.red.shade400
        );
        TextStyle link(ColorScheme colorScheme) => new TextStyle(
            color: CommonThemeUtils.isLight ? Colors.blue.shade700 : Colors.blue.shade300,
            decoration: TextDecoration.underline
        );

        TextStyle regularBold(ColorScheme colorScheme) => new TextStyle(
            color: CommonThemeUtils.defaultForeground,
            fontWeight: FontWeight.w700
        );
        TextStyle regularItalic(ColorScheme colorScheme) => new TextStyle(
            color: CommonThemeUtils.defaultForeground,
            fontStyle: FontStyle.italic
        );
        TextStyle unimportantItalic(ColorScheme colorScheme) =>
            unimportant(colorScheme).merge(new TextStyle(
        fontStyle: FontStyle.italic
        ));
    }
}