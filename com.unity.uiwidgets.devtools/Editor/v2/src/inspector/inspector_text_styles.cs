using uiwidgets;
using Unity.UIWidgets.material;
using Unity.UIWidgets.ui;
using TextStyle = Unity.UIWidgets.painting.TextStyle;

namespace Unity.UIWidgets.DevTools.inspector
{
    public static class inspector_text_styles
    {
        public static TextStyle unimportant(ColorScheme colorScheme) => new TextStyle(
            color: InspectorTreeUtils.isLight ? Colors.grey.shade500 : Colors.grey.shade600);
        
        public static TextStyle regular = new TextStyle();
        public static TextStyle warning(ColorScheme colorScheme) => new TextStyle(
            color:
            InspectorTreeUtils.isLight ? Colors.orange.shade900 : Colors.orange.shade400);
        public static TextStyle error(ColorScheme colorScheme) => new TextStyle(
            color: InspectorTreeUtils.isLight ? Colors.red.shade500 : Colors.red.shade400
        );
        
        public static TextStyle regularBold(ColorScheme colorScheme) => new TextStyle(
            color: ThemeUtils.defaultForeground,
            fontWeight: FontWeight.w700
        );
        public static TextStyle regularItalic(ColorScheme colorScheme) => new TextStyle(
            color: ThemeUtils.defaultForeground,
            fontStyle: FontStyle.italic
        );
        public static TextStyle unimportantItalic(ColorScheme colorScheme) =>
            unimportant(colorScheme).merge(new TextStyle(
        fontStyle: FontStyle.italic
        ));
    }
}