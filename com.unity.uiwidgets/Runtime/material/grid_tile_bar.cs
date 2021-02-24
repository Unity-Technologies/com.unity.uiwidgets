using System.Collections.Generic;
using uiwidgets;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.rendering;
using Unity.UIWidgets.ui;
using Unity.UIWidgets.widgets;

namespace Unity.UIWidgets.material {
    public class GridTileBar : StatelessWidget {
        public GridTileBar(
            Key key = null,
            Color backgroundColor = null,
            Widget leading = null,
            Widget title = null,
            Widget subtitle = null,
            Widget trailing = null
        ) : base(key: key) {
            this.backgroundColor = backgroundColor;
            this.leading = leading;
            this.title = title;
            this.subtitle = subtitle;
            this.trailing = trailing;
        }

        public readonly Color backgroundColor;

        public readonly Widget leading;

        public readonly Widget title;

        public readonly Widget subtitle;

        public readonly Widget trailing;

        public override Widget build(BuildContext context) {
            BoxDecoration decoration = null;
            if (backgroundColor != null)
                decoration = new BoxDecoration(color: backgroundColor);

            EdgeInsetsDirectional padding = EdgeInsetsDirectional.only(
                start: leading != null ? 8.0f : 16.0f,
                end: trailing != null ? 8.0f : 16.0f
            );

            ThemeData theme = Theme.of(context);
            ThemeData darkTheme = new ThemeData(
                brightness: Brightness.dark,
                accentColor: theme.accentColor,
                accentColorBrightness: theme.accentColorBrightness
            );
            var expandChildren = new List<widgets.Widget>();

            if (leading != null) {
                expandChildren.Add(new Padding(
                    padding: EdgeInsetsDirectional.only(end: 8.0f), child: leading));
            }

            if (title != null && subtitle != null) {
                expandChildren.Add(new Expanded(
                    child: new Column(
                        mainAxisAlignment: MainAxisAlignment.center,
                        crossAxisAlignment: CrossAxisAlignment.start,
                        children: new List<Widget> {
                            new DefaultTextStyle(
                                style: darkTheme.textTheme.subtitle1,
                                softWrap: false,
                                overflow: TextOverflow.ellipsis,
                                child: title
                            ),
                            new DefaultTextStyle(
                                style: darkTheme.textTheme.caption,
                                softWrap: false,
                                overflow: TextOverflow.ellipsis,
                                child: subtitle
                            )
                        }
                    )
                ));
            }
            else if (title != null || subtitle != null)
                expandChildren.Add(new Expanded(
                    child: new DefaultTextStyle(
                        style: darkTheme.textTheme.subtitle1,
                        softWrap: false,
                        overflow: TextOverflow.ellipsis,
                        child: title ?? subtitle
                    )
                ));

            if (trailing != null) {
                expandChildren.Add(new Padding(
                    padding: EdgeInsetsDirectional.only(start: 8.0f),
                    child: trailing));
            }

            return new Container(
                padding: padding,
                decoration: decoration,
                height: (title != null && subtitle != null) ? 68.0f : 48.0f,
                child: new Theme(
                    data: darkTheme,
                    child: IconTheme.merge(
                        data: new IconThemeData(color: Colors.white),
                        child: new Row(
                            crossAxisAlignment: CrossAxisAlignment.center,
                            children: expandChildren
                        )
                    )
                )
            );
        }
    }
}