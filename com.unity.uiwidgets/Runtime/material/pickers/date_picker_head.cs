using System.Collections.Generic;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.rendering;
using Unity.UIWidgets.ui;
using Unity.UIWidgets.widgets;
using TextStyle = Unity.UIWidgets.painting.TextStyle;

namespace Unity.UIWidgets.material {
    public static class DatePickerHeaderUtils {
        public const float _datePickerHeaderLandscapeWidth = 152.0f;
        public const float _datePickerHeaderPortraitHeight = 120.0f;
        public const float _headerPaddingLandscape = 16.0f;
    }

    class DatePickerHeader : StatelessWidget {
        public DatePickerHeader(
            Key key = null,
            string helpText = null,
            string titleText = null,
            TextStyle titleStyle = null,
            Orientation? orientation = null,
            bool isShort = false,
            IconData icon = null,
            string iconTooltip = null,
            VoidCallback onIconPressed = null
        ) : base(key: key) {
            D.assert(helpText != null);
            D.assert(orientation != null);
            this.helpText = helpText;
            this.titleText = titleText;
            this.titleStyle = titleStyle;
            this.orientation = orientation.Value;
            this.isShort = isShort;
            this.icon = icon;
            this.iconTooltip = iconTooltip;
            this.onIconPressed = onIconPressed;
        }

        public readonly string helpText;

        public readonly string titleText;

        public readonly TextStyle titleStyle;

        public readonly Orientation orientation;

        public readonly bool isShort;

        public readonly IconData icon;

        public readonly string iconTooltip;

        public readonly VoidCallback onIconPressed;

        public override Widget build(BuildContext context) {
            ThemeData theme = Theme.of(context);
            ColorScheme colorScheme = theme.colorScheme;
            TextTheme textTheme = theme.textTheme;

            bool isDark = colorScheme.brightness == Brightness.dark;
            Color primarySurfaceColor = isDark ? colorScheme.surface : colorScheme.primary;
            Color onPrimarySurfaceColor = isDark ? colorScheme.onSurface : colorScheme.onPrimary;

            TextStyle helpStyle = textTheme.overline?.copyWith(
                color: onPrimarySurfaceColor
            );

            Text help = new Text(
                helpText,
                style: helpStyle,
                maxLines: 1,
                overflow: TextOverflow.ellipsis
            );

            Text title = new Text(
                titleText,
                style: titleStyle,
                maxLines: (isShort || orientation == Orientation.portrait) ? 1 : 2,
                overflow: TextOverflow.ellipsis
            );
            IconButton icon = new IconButton(
                icon: new Icon(this.icon),
                color: onPrimarySurfaceColor,
                tooltip: iconTooltip,
                onPressed: onIconPressed
            );

            switch (orientation) {
                case Orientation.portrait:
                    return new Column(
                        crossAxisAlignment: CrossAxisAlignment.start,
                        children: new List<Widget> {
                            new Container(
                                height: DatePickerHeaderUtils._datePickerHeaderPortraitHeight,
                                color: primarySurfaceColor,
                                padding: EdgeInsetsDirectional.only(
                                    start: 24f,
                                    end: 12f
                                ),
                                child: new Column(
                                    crossAxisAlignment: CrossAxisAlignment.start,
                                    children: new List<Widget> {
                                        new SizedBox(height: 16f),
                                        new Flexible(child: help),
                                        new SizedBox(height: 38),
                                        new Row(
                                            children: new List<Widget> {
                                                new Expanded(child: title),
                                                icon,
                                            }
                                        ),
                                    }
                                )
                            )
                        }
                    );
                case Orientation.landscape:
                    return new Row(
                        crossAxisAlignment: CrossAxisAlignment.start,
                        children: new List<Widget> {
                            new Container(
                                width: DatePickerHeaderUtils._datePickerHeaderLandscapeWidth,
                                color: primarySurfaceColor,
                                child: new Column(
                                    crossAxisAlignment: CrossAxisAlignment.start,
                                    children: new List<Widget> {
                                        new SizedBox(height: 16),
                                        new Padding(
                                            padding: EdgeInsets.symmetric(
                                                horizontal: DatePickerHeaderUtils._headerPaddingLandscape
                                            ),
                                            child: help
                                        ),
                                        new SizedBox(height: isShort ? 16 : 56),
                                        new Padding(
                                            padding: EdgeInsets.symmetric(
                                                horizontal: DatePickerHeaderUtils._headerPaddingLandscape
                                            ),
                                            child: title
                                        ),
                                        new Spacer(),
                                        new Padding(
                                            padding: EdgeInsets.symmetric(
                                                horizontal: 4
                                            ),
                                            child: icon
                                        ),
                                    }
                                )
                            ),
                        }
                    );
            }

            return null;
        }
    }
}