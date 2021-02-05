using System.Collections.Generic;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.ui;
using Unity.UIWidgets.widgets;
using TextStyle = Unity.UIWidgets.painting.TextStyle;

namespace Unity.UIWidgets.material {
    public class MaterialBanner : StatelessWidget {
        public MaterialBanner(
            Key key = null,
            Widget content = null,
            TextStyle contentTextStyle = null,
            List<Widget> actions = null,
            Widget leading = null,
            Color backgroundColor = null,
            EdgeInsetsGeometry padding = null,
            EdgeInsetsGeometry leadingPadding = null,
            bool forceActionsBelow = false
        ) : base(key: key) {
            D.assert(content != null);
            D.assert(actions != null);
            this.content = content;
            this.contentTextStyle = contentTextStyle;
            this.actions = actions;
            this.leading = leading;
            this.backgroundColor = backgroundColor;
            this.padding = padding;
            this.leadingPadding = leadingPadding;
            this.forceActionsBelow = forceActionsBelow;
        }

        public readonly Widget content;

        public readonly TextStyle contentTextStyle;

        public readonly List<Widget> actions;

        public readonly Widget leading;

        public readonly Color backgroundColor;

        public readonly EdgeInsetsGeometry padding;

        public readonly EdgeInsetsGeometry leadingPadding;

        public readonly bool forceActionsBelow;

        public override Widget build(BuildContext context) {
            D.assert(actions.isNotEmpty);

            ThemeData theme = Theme.of(context);
            MaterialBannerThemeData bannerTheme = MaterialBannerTheme.of(context);

            bool isSingleRow = actions.Count == 1 && !forceActionsBelow;
            EdgeInsetsGeometry padding = this.padding ?? bannerTheme?.padding ?? (isSingleRow
                ? EdgeInsetsDirectional.only(start: 16.0f, top: 2.0f)
                : EdgeInsetsDirectional.only(start: 16.0f, top: 24.0f, end: 16.0f,
                    bottom: 4.0f));
            EdgeInsetsGeometry leadingPadding = this.leadingPadding
                                                ?? bannerTheme?.padding
                                                ?? EdgeInsetsDirectional.only(end: 16.0f);

            Widget buttonBar = new ButtonBar(
                layoutBehavior: ButtonBarLayoutBehavior.constrained,
                children: actions
            );

            Color backgroundColor = this.backgroundColor
                                    ?? bannerTheme?.backgroundColor
                                    ?? theme.colorScheme.surface;
            TextStyle textStyle = contentTextStyle
                                  ?? bannerTheme?.contentTextStyle
                                  ?? theme.textTheme.bodyText2;

            var rowList = new List<Widget>();
            if (leading != null) {
                rowList.Add(new Padding(
                    padding: leadingPadding,
                    child: leading
                ));
            }

            rowList.Add(new Expanded(
                child: new DefaultTextStyle(
                    style: textStyle,
                    child: content
                )
            ));
            if (isSingleRow) {
                rowList.Add(buttonBar);
            }

            var columnList = new List<Widget>();
            columnList.Add(new Padding(
                padding: padding,
                child: new Row(
                    children: rowList
                )
            ));
            if (!isSingleRow) {
                columnList.Add(buttonBar);
            }

            columnList.Add(new Divider(height: 0));

            return new Container(
                color: backgroundColor,
                child: new Column(
                    children: columnList
                )
            );
        }
    }
}