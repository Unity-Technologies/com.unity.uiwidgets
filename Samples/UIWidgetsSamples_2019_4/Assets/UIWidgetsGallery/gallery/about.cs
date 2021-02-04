using System.Collections.Generic;
using System.Diagnostics;
using Unity.UIWidgets.gestures;
using Unity.UIWidgets.material;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.widgets;

namespace UIWidgetsGallery.gallery
{
    public static class GalleryAboutUtils
    {
        public static void showGalleryAboutDialog(BuildContext context)
        {
            ThemeData themeData = Theme.of(context);
            TextStyle aboutTextStyle = themeData.textTheme.bodyText1;
            TextStyle linkStyle = themeData.textTheme.bodyText1.copyWith(color: themeData.accentColor);

            material_.showAboutDialog(
                context: context,
                applicationVersion: "January 2021",
                children: new List<Widget>
                {
                    new Padding(
                        padding: EdgeInsets.only(top: 24.0f),
                        child: new RichText(
                            text: new TextSpan(
                                children: new List<InlineSpan>
                                {
                                    new TextSpan(
                                        style: aboutTextStyle,
                                        text: "uiwidgets is an open-source project to help developers " +
                                              "build high-performance, high-fidelity, mobile apps for multiple platforms using " +
                                              "Unity Editor" +
                                              "from a single codebase. This design lab is a playground " +
                                              "and showcase of Flutter's many widgets, behaviors, " +
                                              "animations, layouts, and more. Learn more about Flutter at "
                                    ),
                                    new _LinkTextSpan(
                                        style: linkStyle,
                                        url: "https://github.com/Unity-Technologies/com.unity.uiwidgets",
                                        text: "uiWidgets github repo"
                                    ),
                                    new TextSpan(
                                        style: aboutTextStyle,
                                        text: "."
                                    )
                                }
                            )
                        )
                    )
                }
            );
        }
    }
    public class _LinkTextSpan : TextSpan
    {
        public _LinkTextSpan(TextStyle style = null, string url = null, string text = null) : base(
            style: style,
            text: text ?? url,
            recognizer: new TapGestureRecognizer {onTap = () => { BrowserUtils.launch(url); }}
        )
        {
        }
    }
}