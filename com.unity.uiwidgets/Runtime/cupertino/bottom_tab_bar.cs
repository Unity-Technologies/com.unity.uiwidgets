using System.Collections.Generic;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.gestures;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.rendering;
using Unity.UIWidgets.ui;
using Unity.UIWidgets.widgets;
using TextStyle = Unity.UIWidgets.painting.TextStyle;

namespace Unity.UIWidgets.cupertino {
    class BottomAppBarUtils {
        public const float _kTabBarHeight = 50.0f;

        public static readonly Color _kDefaultTabBarBorderColor = 
            CupertinoDynamicColor.withBrightness(
                color: new Color(0x4C000000),
                darkColor: new Color(0x29000000)
            );
        public static readonly Color _kDefaultTabBarInactiveColor = CupertinoColors.inactiveGray;
    }


    public class CupertinoTabBar : StatelessWidget{ 

        public CupertinoTabBar(
            Key key = null,
            List<BottomNavigationBarItem> items = null,
            ValueChanged<int> onTap = null,
            int currentIndex = 0,
            Color backgroundColor = null,
            Color activeColor = null,
            Color inactiveColor = null,
            float iconSize = 30.0f,
            Border border = null
        ) : base(key: key) {

            D.assert(items != null);
            D.assert(items.Count >= 2,
                () => "Tabs need at least 2 items to conform to Apple's HIG"
            );
            D.assert(0 <= currentIndex && currentIndex < items.Count);

            this.items = items;
            this.onTap = onTap;
            this.currentIndex = currentIndex;
            this.backgroundColor = backgroundColor;
            this.activeColor = activeColor;
            this.inactiveColor = inactiveColor ?? BottomAppBarUtils._kDefaultTabBarInactiveColor;
            this.iconSize = iconSize;
            this.border = border ?? new Border(
                              top: new BorderSide(
                                  color: BottomAppBarUtils._kDefaultTabBarBorderColor,
                                  width: 0.0f, 
                                  style: BorderStyle.solid
                              )
                          );
        }

        public readonly List<BottomNavigationBarItem> items;

        public readonly ValueChanged<int> onTap;

        public readonly int currentIndex;

        public readonly Color backgroundColor;

        public readonly Color activeColor;

        public readonly Color inactiveColor;

        public readonly float iconSize;

        public readonly Border border;

        public Size preferredSize {
            get { return Size.fromHeight(BottomAppBarUtils._kTabBarHeight); }
        }

        public bool opaque(BuildContext context) {
            
            Color backgroundColor =
                this.backgroundColor ?? CupertinoTheme.of(context).barBackgroundColor;
           
            return CupertinoDynamicColor.resolve(backgroundColor, context).alpha == 0xFF;
        }

        public override Widget build(BuildContext context) {
            float bottomPadding = MediaQuery.of(context).padding.bottom;
            Color backgroundColor = CupertinoDynamicColor.resolve(
                this.backgroundColor ?? CupertinoTheme.of(context).barBackgroundColor,
                context
            );
            BorderSide resolveBorderSide(BorderSide side) {
                return side == BorderSide.none
                    ? side
                    : side.copyWith(color: CupertinoDynamicColor.resolve(side.color, context));
            }
             Border resolvedBorder = border == null || border.GetType() != typeof(Border)
                ? border
                : new Border(
                    top: resolveBorderSide(border.top),
                    left: resolveBorderSide(border.left),
                    bottom: resolveBorderSide(border.bottom),
                    right: resolveBorderSide(border.right)
                );

            Color inactive = CupertinoDynamicColor.resolve(inactiveColor, context);
            Widget result = new DecoratedBox(
                decoration: new BoxDecoration(
                    border: resolvedBorder,
                    color: backgroundColor 
                ),
                child: new SizedBox(
                    height: BottomAppBarUtils._kTabBarHeight + bottomPadding,
                    child: IconTheme.merge( // Default with the inactive state.
                        data: new IconThemeData(
                            color: inactiveColor,
                            size: iconSize
                        ),
                        child: new DefaultTextStyle( // Default with the inactive state.
                            style: CupertinoTheme.of(context).textTheme.tabLabelTextStyle.copyWith(color: inactive),
                            child: new Padding(
                                padding: EdgeInsets.only(bottom: bottomPadding),
                                child: new Row(
                                    crossAxisAlignment: CrossAxisAlignment.end,
                                    children: _buildTabItems(context)
                                )
                            )
                        )
                    )
                )
            );

            if (!opaque(context)) {
                result = new ClipRect(
                    child: new BackdropFilter(
                        filter: ImageFilter.blur(sigmaX: 10.0f, sigmaY: 10.0f),
                        child: result
                    )
                );
            }

            return result;
        }

        List<Widget> _buildTabItems(BuildContext context) {
            List<Widget> result = new List<Widget> { };

            for (int index = 0; index < items.Count; index += 1) {
                bool active = index == currentIndex;
                var tabIndex = index;
                result.Add(
                    _wrapActiveItem(
                        context,
                        new Expanded(
                            child: new GestureDetector(
                                behavior: HitTestBehavior.opaque,
                                onTap: onTap == null ? null : (GestureTapCallback) (() => { onTap(tabIndex); }),
                                child: new Padding(
                                    padding: EdgeInsets.only(bottom: 4.0f),
                                    child: new Column(
                                        mainAxisAlignment: MainAxisAlignment.end,
                                        children: _buildSingleTabItem(items[index], active)
                                    )
                                )
                            )
                        ),
                        active: active
                    )
                );
            }

            return result;
        }

        List<Widget> _buildSingleTabItem(BottomNavigationBarItem item, bool active) {
            List<Widget> components = new List<Widget> {
                new Expanded(
                    child: new Center(child: active ? item.activeIcon : item.icon)
                )
            };

            if (item.title != null) {
                components.Add(item.title);
            }

            return components;
        }

        Widget _wrapActiveItem(BuildContext context, Widget item, bool active) {
            if (!active) {
                return item;
            }
            Color activeColor = CupertinoDynamicColor.resolve(
                this.activeColor ?? CupertinoTheme.of(context).primaryColor,
                context
            );
            return IconTheme.merge(
                data: new IconThemeData(color: activeColor),
                child: DefaultTextStyle.merge(
                    style: new TextStyle(color: activeColor),
                    child: item
                )
            );
        }

        public CupertinoTabBar copyWith(
            Key key = null,
            List<BottomNavigationBarItem> items = null,
            Color backgroundColor = null,
            Color activeColor = null,
            Color inactiveColor = null,
            float? iconSize = null,
            Border border = null,
            int? currentIndex = null,
            ValueChanged<int> onTap = null
        ) {
            return new CupertinoTabBar(
                key: key ?? this.key,
                items: items ?? this.items,
                backgroundColor: backgroundColor ?? this.backgroundColor,
                activeColor: activeColor ?? this.activeColor,
                inactiveColor: inactiveColor ?? this.inactiveColor,
                iconSize: iconSize ?? this.iconSize,
                border: border ?? this.border,
                currentIndex: currentIndex ?? this.currentIndex,
                onTap: onTap ?? this.onTap
            );
        }
    }
}