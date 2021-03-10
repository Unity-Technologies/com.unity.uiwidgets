using Unity.UIWidgets.material;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.rendering;
using Unity.UIWidgets.ui;
using Unity.UIWidgets.widgets;

namespace Unity.UIWidgets.DevTools.inspector.layout_explorer.ui
{
    public class Dimension
    {
        public static Widget dimensionDescription(
            TextSpan description,
            bool overflow,
            ColorScheme colorScheme
        ) {
            var text = Text.rich(
                description,
                textAlign: TextAlign.center,
                style: 
                     ThemeUtils.dimensionIndicatorTextStyle,
                overflow: TextOverflow.ellipsis
            );
            if (overflow) {
                return new Container(
                    padding:  EdgeInsets.symmetric(
                    vertical: ThemeUtils.minPadding,
                    horizontal: ThemeUtils.overflowTextHorizontalPadding
                ),
                decoration: new BoxDecoration(
                    color: ThemeUtils.overflowBackgroundColor,
                    borderRadius: BorderRadius.circular(4.0f)
                ),
                child: new Center(child: text)
                    );
            }
            return text;
        }
    }
}