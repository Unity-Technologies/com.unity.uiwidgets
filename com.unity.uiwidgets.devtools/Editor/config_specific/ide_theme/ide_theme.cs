using Unity.UIWidgets.ui;

namespace Unity.UIWidgets.DevTools.config_specific.ide_theme
{
    
    public class IdeTheme {
        public IdeTheme(Color backgroundColor = null, Color foregroundColor = null, float? fontSize = null)
        {
            this.backgroundColor = backgroundColor;
            this.foregroundColor = foregroundColor;
            this.fontSize = fontSize;
        }

        public readonly Color backgroundColor;
        public readonly Color foregroundColor;
        public readonly float? fontSize;
    }
}