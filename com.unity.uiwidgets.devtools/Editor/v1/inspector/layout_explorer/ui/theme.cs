/*using uiwidgets;
using Unity.UIWidgets.material;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.ui;
using Unity.UIWidgets.widgets;
using TextStyle = Unity.UIWidgets.painting.TextStyle;

namespace Unity.UIWidgets.DevTools.inspector.layout_explorer.ui
{
    public class ThemeUtils
    {
        public static readonly float margin = 8.0f;

        public static readonly float arrowHeadSize = 8.0f;
        public static readonly float arrowMargin = 4.0f;
        public static readonly float arrowStrokeWidth = 1.5f;

        /// Hardcoded sizes for scaling the flex children widget properly.
        public static readonly float minRenderWidth = 250.0f;
        public static readonly float minRenderHeight = 250.0f;

        public static readonly float minPadding = 2.0f;
        public static readonly float overflowTextHorizontalPadding = 8.0f;

        /// The size to shrink a widget by when animating it in.
        public static readonly float entranceMargin = 50.0f;

        public static readonly float defaultMaxRenderWidth = 400.0f;
        public static readonly float defaultMaxRenderHeight = 400.0f;

        public static readonly float widgetTitleMaxWidthPercentage = 0.75f;

        /// Hardcoded arrow size respective to its cross axis (because it's unconstrained).
        public static readonly float heightAndConstraintIndicatorSize = 48.0f;
        public static readonly float widthAndConstraintIndicatorSize = 56.0f;
        public static readonly float mainAxisArrowIndicatorSize = 48.0f;
        public static readonly float crossAxisArrowIndicatorSize = 48.0f;

        public static readonly float heightOnlyIndicatorSize = 72.0f;
        public static readonly float widthOnlyIndicatorSize = 32.0f;

        /// Minimum size to display width/height inside the arrow
        public static readonly float minWidthToDisplayWidthInsideArrow = 200.0f;
        public static readonly float minHeightToDisplayHeightInsideArrow = 200.0f;

        public static readonly float largeTextScaleFactor = 1.2f;
        public static readonly float smallTextScaleFactor = 0.8f;

        /// Height for limiting asset image (selected one in the drop down).
        public static readonly float axisAlignmentAssetImageHeight = 24.0f;

        /// Width for limiting asset image (when drop down menu is open for the vertical).
        public static readonly float axisAlignmentAssetImageWidth = 96.0f;
        public static readonly float dropdownMaxSize = 220.0f;

        public static readonly float minHeightToAllowTruncating = 375.0f;
        public static readonly float minWidthToAllowTruncating = 375.0f;

        // Story of Layout colors
        public static readonly Color mainAxisLightColor = new Color(0xff2c5daa);
        public static readonly Color mainAxisDarkColor = new Color(0xff2c5daa);
        public static readonly Color rowColor = new Color(0xff2c5daa);
        public static readonly Color columnColor = new Color(0xff77974d);
        public static readonly Color regularWidgetColor = new Color(0xff88b1de);

        public static readonly Color selectedWidgetColor = new Color(0xff36c6f4);

        public static readonly Color textColor = new Color(0xff55767f);
        public static readonly Color emphasizedTextColor = new Color(0xff009aca);

        public static readonly Color crossAxisLightColor = new Color(0xff8ac652);
        public static readonly Color crossAxisDarkColor = new Color(0xff8ac652);

        public static readonly Color mainAxisTextColorLight = new Color(0xFF1375bc);
        public static readonly Color mainAxisTextColorDark = new Color(0xFF1375bc);

        public static readonly Color crossAxisTextColorLight = new Color(0xFF66672C);
        public static readonly Color crossAxisTextColorsDark = new Color(0xFFB3D25A);

        public static readonly Color overflowBackgroundColorDark = new Color(0xFFB00020);
        public static readonly Color overflowBackgroundColorLight = new Color(0xFFB00020);

        public static readonly Color overflowTextColorDark = new Color(0xfff5846b);
        public static readonly Color overflowTextColorLight = new Color(0xffdea089);

        public static readonly Color backgroundColorSelectedDark = new Color(
            0x4d474747); // TODO(jacobr): we would like Color(0x4dedeeef) but that makes the background show through.
        public static readonly Color backgroundColorSelectedLight = new Color(0x4dedeeef);
        
        
        public static Color mainAxisColor
        {
          get
          {
              return  CommonThemeUtils.isLight? mainAxisLightColor : mainAxisDarkColor;
          }
        }

        public static Color widgetNameColor
        {
          get
          {
              return CommonThemeUtils.isLight? Colors.white : Colors.black;
          }
        }

        public static Color crossAxisColor
        {
          get
          {
              return CommonThemeUtils.isLight? crossAxisLightColor : crossAxisDarkColor;
          }
        }

        public static Color mainAxisTextColor
        {
          get
          {
              return CommonThemeUtils.isLight? mainAxisTextColorLight : mainAxisTextColorDark;
          }
        }

        public static Color crossAxisTextColor
        {
          get
          {
              return CommonThemeUtils.isLight? crossAxisTextColorLight : crossAxisTextColorsDark;
          }
        }

        public static Color overflowBackgroundColor
        {
          get
          {
              return CommonThemeUtils.isLight? overflowBackgroundColorLight : overflowBackgroundColorDark;
          }
        }

        public static Color overflowTextColor
        {
          get
          {
              return CommonThemeUtils.isLight? overflowTextColorLight : overflowTextColorDark;
          }
        }

        public static Color backgroundColorSelected
        {
          get
          {
              return CommonThemeUtils.isLight? backgroundColorSelectedLight : backgroundColorSelectedDark;
          }
        }

        public static Color backgroundColor
        {
          get
          {
              return CommonThemeUtils.isLight? backgroundColorLight : backgroundColorDark;
          }
        }

        public static Color unconstrainedColor
        {
          get
          {
              return CommonThemeUtils.isLight? unconstrainedLightColor : unconstrainedDarkColor;
          }
        }
        

        public static readonly Color backgroundColorDark = new Color(0xff30302f);
        public static readonly Color backgroundColorLight = new Color(0xffffffff);

        public static readonly Color unconstrainedDarkColor = new Color(0xffdea089);
        public static readonly Color unconstrainedLightColor = new Color(0xfff5846b);

        public static readonly Color widthIndicatorColor = textColor;
        public static readonly Color heightIndicatorColor = textColor;

        public static readonly string negativeSpaceDarkAssetName =
            "assets/img/layout_explorer/negative_space_dark.png";
        public static readonly string negativeSpaceLightAssetName =
            "assets/img/layout_explorer/negative_space_light.png";

        public static TextStyle dimensionIndicatorTextStyle = new TextStyle(
          height: 1.0f,
          letterSpacing: 1.1f,
          color: emphasizedTextColor
        );

        public static TextStyle overflowingDimensionIndicatorTextStyle(ColorScheme colorScheme)
        {
            return dimensionIndicatorTextStyle.merge(
                new TextStyle(
                    fontWeight: FontWeight.bold,
                    color: overflowTextColor
                )
            );
        }

        public static Widget buildUnderline() {
          return new Container(
            height: 1.0f,
            decoration: new BoxDecoration(
              border: new Border(
                bottom: new BorderSide(
                  color: textColor,
                  width: 0.0f
                )
              )
            )
          );
        }
    }
    

}*/