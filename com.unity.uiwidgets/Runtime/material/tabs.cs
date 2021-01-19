using System.Collections.Generic;
using RSG;
using Unity.UIWidgets.animation;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.gestures;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.rendering;
using Unity.UIWidgets.ui;
using Unity.UIWidgets.widgets;
using TextStyle = Unity.UIWidgets.painting.TextStyle;

namespace Unity.UIWidgets.material {
    static class TabsUtils {
        public const float _kTabHeight = 46.0f;
        public const float _kTextAndIconTabHeight = 72.0f;

        public static float _indexChangeProgress(TabController controller) {
            float controllerValue = controller.animation.value;
            float previousIndex = controller.previousIndex;
            float currentIndex = controller.index;

            if (!controller.indexIsChanging) {
                return (currentIndex - controllerValue).abs().clamp(0.0f, 1.0f);
            }

            return (controllerValue - currentIndex).abs() / (currentIndex - previousIndex).abs();
        }

        public static readonly PageScrollPhysics _kTabBarViewPhysics =
            (PageScrollPhysics) new PageScrollPhysics().applyTo(new ClampingScrollPhysics());
    }

    public enum TabBarIndicatorSize {
        tab,
        label
    }

    public class Tab : StatelessWidget {
        public Tab(
            Key key = null,
            string text = null,
            Widget icon = null,
            Widget child = null
        ) : base(key: key) {
            D.assert(text != null || child != null || icon != null);
            D.assert(!(text != null && child != null));
            this.text = text;
            this.icon = icon;
            this.child = child;
        }

        public readonly string text;

        public readonly Widget child;

        public readonly Widget icon;

        Widget _buildLabelText() {
            return child ?? new Text(text, softWrap: false, overflow: TextOverflow.fade);
        }

        public override Widget build(BuildContext context) {
            D.assert(MaterialD.debugCheckHasMaterial(context));

            float height = 0f;
            Widget label = null;

            if (icon == null) {
                height = TabsUtils._kTabHeight;
                label = _buildLabelText();
            }
            else if (text == null && child == null) {
                height = TabsUtils._kTabHeight;
                label = icon;
            }
            else {
                height = TabsUtils._kTextAndIconTabHeight;
                label = new Column(
                    mainAxisAlignment: MainAxisAlignment.center,
                    crossAxisAlignment: CrossAxisAlignment.center,
                    children: new List<Widget> {
                        new Container(
                            child: icon,
                            margin: EdgeInsets.only(bottom: 10.0f)
                        ),
                        _buildLabelText()
                    }
                );
            }

            return new SizedBox(
                height: height,
                child: new Center(
                    child: label,
                    widthFactor: 1.0f)
            );
        }

        public override void debugFillProperties(DiagnosticPropertiesBuilder properties) {
            base.debugFillProperties(properties);
            properties.add(new StringProperty("text", text, defaultValue: Diagnostics.kNullDefaultValue));
            properties.add(new DiagnosticsProperty<Widget>("icon", icon,
                defaultValue: Diagnostics.kNullDefaultValue));
        }
    }


    class _TabStyle : AnimatedWidget {
        public _TabStyle(
            Key key = null,
            Animation<float> animation = null,
            bool? selected = null,
            Color labelColor = null,
            Color unselectedLabelColor = null,
            TextStyle labelStyle = null,
            TextStyle unselectedLabelStyle = null,
            Widget child = null
        ) : base(key: key, listenable: animation) {
            D.assert(child != null);
            D.assert(selected != null);
            this.selected = selected.Value;
            this.labelColor = labelColor;
            this.unselectedLabelColor = unselectedLabelColor;
            this.labelStyle = labelStyle;
            this.unselectedLabelStyle = unselectedLabelStyle;
            this.child = child;
        }

        public readonly TextStyle labelStyle;

        public readonly TextStyle unselectedLabelStyle;

        public readonly bool selected;

        public readonly Color labelColor;

        public readonly Color unselectedLabelColor;

        public readonly Widget child;

        protected internal override Widget build(BuildContext context) {
            ThemeData themeData = Theme.of(context);
            TabBarTheme tabBarTheme = TabBarTheme.of(context);
            Animation<float> animation = (Animation<float>) listenable;
            
            TextStyle defaultStyle = (labelStyle
                                      ?? tabBarTheme.labelStyle
                                      ?? themeData.primaryTextTheme.body2).copyWith(inherit: true);
            TextStyle defaultUnselectedStyle = (unselectedLabelStyle
                                               ?? tabBarTheme.unselectedLabelStyle
                                               ?? labelStyle
                                               ?? themeData.primaryTextTheme.body2).copyWith(inherit: true);
            TextStyle textStyle = selected
                ? TextStyle.lerp(defaultStyle, defaultUnselectedStyle, animation.value)
                : TextStyle.lerp(defaultUnselectedStyle, defaultStyle, animation.value);

            Color selectedColor = labelColor ?? tabBarTheme.labelColor ?? themeData.primaryTextTheme.body2.color;
            Color unselectedColor = unselectedLabelColor ??
                                    tabBarTheme.unselectedLabelColor ?? selectedColor.withAlpha(0xB2);
            Color color = selected
                ? Color.lerp(selectedColor, unselectedColor, animation.value)
                : Color.lerp(unselectedColor, selectedColor, animation.value);

            return new DefaultTextStyle(
                style: textStyle.copyWith(color: color),
                child: IconTheme.merge(
                    data: new IconThemeData(
                        size: 24.0f,
                        color: color),
                    child: child
                )
            );
        }
    }

    delegate void _LayoutCallback(List<float> xOffsets, float width);


    class _TabLabelBarRenderer : RenderFlex {
        public _TabLabelBarRenderer(
            List<RenderBox> children = null,
            Axis? direction = null,
            MainAxisSize? mainAxisSize = null,
            MainAxisAlignment? mainAxisAlignment = null,
            CrossAxisAlignment? crossAxisAlignment = null,
            VerticalDirection? verticalDirection = null,
            _LayoutCallback onPerformLayout = null
        ) : base(
            children: children,
            direction: direction.Value,
            mainAxisSize: mainAxisSize.Value,
            mainAxisAlignment: mainAxisAlignment.Value,
            crossAxisAlignment: crossAxisAlignment.Value,
            verticalDirection: verticalDirection.Value
        ) {
            D.assert(direction != null);
            D.assert(mainAxisSize != null);
            D.assert(mainAxisAlignment != null);
            D.assert(crossAxisAlignment != null);
            D.assert(verticalDirection != null);

            D.assert(onPerformLayout != null);
            this.onPerformLayout = onPerformLayout;
        }

        public _LayoutCallback onPerformLayout;

        protected override void performLayout() {
            base.performLayout();

            RenderBox child = firstChild;
            List<float> xOffsets = new List<float>();

            while (child != null) {
                FlexParentData childParentData = (FlexParentData) child.parentData;
                xOffsets.Add(childParentData.offset.dx);
                D.assert(child.parentData == childParentData);
                child = childParentData.nextSibling;
            }

            xOffsets.Add(size.width);
            onPerformLayout(xOffsets, size.width);
        }
    }


    class _TabLabelBar : Flex {
        public _TabLabelBar(
            Key key = null,
            List<Widget> children = null,
            _LayoutCallback onPerformLayout = null
        ) : base(
            key: key,
            children: children ?? new List<Widget>(),
            direction: Axis.horizontal,
            mainAxisSize: MainAxisSize.max,
            mainAxisAlignment: MainAxisAlignment.start,
            crossAxisAlignment: CrossAxisAlignment.center,
            verticalDirection: VerticalDirection.down
        ) {
            this.onPerformLayout = onPerformLayout;
        }

        public readonly _LayoutCallback onPerformLayout;

        public override RenderObject createRenderObject(BuildContext context) {
            return new _TabLabelBarRenderer(
                direction: direction,
                mainAxisAlignment: mainAxisAlignment,
                mainAxisSize: mainAxisSize,
                crossAxisAlignment: crossAxisAlignment,
                verticalDirection: verticalDirection,
                onPerformLayout: onPerformLayout
            );
        }

        public override void updateRenderObject(BuildContext context, RenderObject renderObject) {
            base.updateRenderObject(context, renderObject);
            _TabLabelBarRenderer _renderObject = (_TabLabelBarRenderer) renderObject;
            _renderObject.onPerformLayout = onPerformLayout;
        }
    }

    class _IndicatorPainter : AbstractCustomPainter {
        public _IndicatorPainter(
            TabController controller = null,
            Decoration indicator = null,
            TabBarIndicatorSize? indicatorSize = null,
            List<GlobalKey> tabKeys = null,
            _IndicatorPainter old = null
        ) : base(repaint: controller.animation) {
            D.assert(controller != null);
            D.assert(indicator != null);
            this.controller = controller;
            this.indicator = indicator;
            this.indicatorSize = indicatorSize;
            this.tabKeys = tabKeys;
            if (old != null) {
                saveTabOffsets(old._currentTabOffsets);
            }
        }

        public readonly TabController controller;

        public readonly Decoration indicator;

        public readonly TabBarIndicatorSize? indicatorSize;

        public readonly List<GlobalKey> tabKeys;

        List<float> _currentTabOffsets;
        Rect _currentRect;
        BoxPainter _painter;
        bool _needsPaint = false;

        void markNeedsPaint() {
            _needsPaint = true;
        }

        public void dispose() {
            _painter?.Dispose();
        }

        public void saveTabOffsets(List<float> tabOffsets) {
            _currentTabOffsets = tabOffsets;
        }

        public int maxTabIndex {
            get { return _currentTabOffsets.Count - 2; }
        }

        public float centerOf(int tabIndex) {
            D.assert(_currentTabOffsets != null);
            D.assert(_currentTabOffsets.isNotEmpty());
            D.assert(tabIndex >= 0);
            D.assert(tabIndex <= maxTabIndex);
            return (_currentTabOffsets[tabIndex] + _currentTabOffsets[tabIndex + 1]) / 2.0f;
        }

        public Rect indicatorRect(Size tabBarSize, int tabIndex) {
            D.assert(_currentTabOffsets != null);
            D.assert(_currentTabOffsets.isNotEmpty());
            D.assert(tabIndex >= 0);
            D.assert(tabIndex <= maxTabIndex);
            float tabLeft = _currentTabOffsets[tabIndex];
            float tabRight = _currentTabOffsets[tabIndex + 1];

            if (indicatorSize == TabBarIndicatorSize.label) {
                float tabWidth = tabKeys[tabIndex].currentContext.size.width;
                float delta = ((tabRight - tabLeft) - tabWidth) / 2.0f;
                tabLeft += delta;
                tabRight -= delta;
            }

            return Rect.fromLTWH(tabLeft, 0.0f, tabRight - tabLeft, tabBarSize.height);
        }

        public override void paint(Canvas canvas, Size size) {
            _needsPaint = false;
            _painter = _painter ?? indicator.createBoxPainter(markNeedsPaint);

            if (controller.indexIsChanging) {
                Rect targetRect = indicatorRect(size, controller.index);
                _currentRect = Rect.lerp(targetRect, _currentRect ?? targetRect,
                    TabsUtils._indexChangeProgress(controller));
            }
            else {
                int currentIndex = controller.index;
                Rect previous = currentIndex > 0 ? indicatorRect(size, currentIndex - 1) : null;
                Rect middle = indicatorRect(size, currentIndex);
                Rect next = currentIndex < maxTabIndex ? indicatorRect(size, currentIndex + 1) : null;
                float index = controller.index;
                float value = controller.animation.value;
                if (value == index - 1.0f) {
                    _currentRect = previous ?? middle;
                }
                else if (value == index + 1.0f) {
                    _currentRect = next ?? middle;
                }
                else if (value == index) {
                    _currentRect = middle;
                }
                else if (value < index) {
                    _currentRect = previous == null ? middle : Rect.lerp(middle, previous, index - value);
                }
                else {
                    _currentRect = next == null ? middle : Rect.lerp(middle, next, value - index);
                }
            }

            D.assert(_currentRect != null);

            ImageConfiguration configuration = new ImageConfiguration(
                size: _currentRect.size
            );

            _painter.paint(canvas, _currentRect.topLeft, configuration);
        }

        static bool _tabOffsetsEqual(List<float> a, List<float> b) {
            if (a?.Count != b?.Count) {
                return false;
            }

            for (int i = 0; i < a.Count; i++) {
                if (a[i] != b[i]) {
                    return false;
                }
            }

            return true;
        }

        public override bool shouldRepaint(CustomPainter old) {
            _IndicatorPainter _old = (_IndicatorPainter) old;
            return _needsPaint
                   || controller != _old.controller
                   || indicator != _old.indicator
                   || tabKeys.Count != _old.tabKeys.Count
                   || !_tabOffsetsEqual(_currentTabOffsets, _old._currentTabOffsets);
        }
    }

    class _ChangeAnimation : AnimationWithParentMixin<float, float> {
        public _ChangeAnimation(
            TabController controller) {
            this.controller = controller;
        }

        public readonly TabController controller;

        public override Animation<float> parent {
            get { return controller.animation; }
        }

        public override float value {
            get { return TabsUtils._indexChangeProgress(controller); }
        }
    }


    class _DragAnimation : AnimationWithParentMixin<float, float> {
        public _DragAnimation(
            TabController controller,
            int index) {
            this.controller = controller;
            this.index = index;
        }

        public readonly TabController controller;

        public readonly int index;

        public override Animation<float> parent {
            get { return controller.animation; }
        }

        public override float value {
            get {
                D.assert(!controller.indexIsChanging);
                return (controller.animation.value - index).abs().clamp(0.0f, 1.0f);
            }
        }
    }


    class _TabBarScrollPosition : ScrollPositionWithSingleContext {
        public _TabBarScrollPosition(
            ScrollPhysics physics = null,
            ScrollContext context = null,
            ScrollPosition oldPosition = null,
            _TabBarState tabBar = null
        ) : base(
            physics: physics,
            context: context,
            initialPixels: null,
            oldPosition: oldPosition) {
            this.tabBar = tabBar;
        }

        public readonly _TabBarState tabBar;

        bool _initialViewportDimensionWasZero;

        public override bool applyContentDimensions(float minScrollExtent, float maxScrollExtent) {
            bool result = true;
            if (_initialViewportDimensionWasZero != true) {
                _initialViewportDimensionWasZero = viewportDimension != 0.0;
                correctPixels(tabBar._initialScrollOffset(viewportDimension, minScrollExtent,
                    maxScrollExtent));
                result = false;
            }

            return base.applyContentDimensions(minScrollExtent, maxScrollExtent) && result;
        }
    }


    class _TabBarScrollController : ScrollController {
        public _TabBarScrollController(_TabBarState tabBar) {
            this.tabBar = tabBar;
        }

        public readonly _TabBarState tabBar;

        public override ScrollPosition createScrollPosition(ScrollPhysics physics, ScrollContext context,
            ScrollPosition oldPosition) {
            return new _TabBarScrollPosition(
                physics: physics,
                context: context,
                oldPosition: oldPosition,
                tabBar: tabBar
            );
        }
    }


    public class TabBar : PreferredSizeWidget {
        public TabBar(
            Key key = null,
            List<Widget> tabs = null,
            TabController controller = null,
            bool isScrollable = false,
            Color indicatorColor = null,
            float indicatorWeight = 2.0f,
            EdgeInsets indicatorPadding = null,
            Decoration indicator = null,
            TabBarIndicatorSize? indicatorSize = null,
            Color labelColor = null,
            TextStyle labelStyle = null,
            EdgeInsets labelPadding = null,
            Color unselectedLabelColor = null,
            TextStyle unselectedLabelStyle = null,
            DragStartBehavior dragStartBehavior = DragStartBehavior.start,
            ValueChanged<int> onTap = null
        ) : base(key: key) {
            indicatorPadding = indicatorPadding ?? EdgeInsets.zero;
            D.assert(tabs != null);
            D.assert(indicator != null || indicatorWeight > 0.0f);
            D.assert(indicator != null || indicatorPadding != null);
            this.tabs = tabs;
            this.controller = controller;
            this.isScrollable = isScrollable;
            this.indicatorColor = indicatorColor;
            this.indicatorWeight = indicatorWeight;
            this.indicatorPadding = indicatorPadding;
            this.indicator = indicator;
            this.indicatorSize = indicatorSize;
            this.labelColor = labelColor;
            this.labelStyle = labelStyle;
            this.labelPadding = labelPadding;
            this.unselectedLabelColor = unselectedLabelColor;
            this.unselectedLabelStyle = unselectedLabelStyle;
            this.dragStartBehavior = dragStartBehavior;
            this.onTap = onTap;
        }

        public readonly List<Widget> tabs;

        public readonly TabController controller;

        public readonly bool isScrollable;

        public readonly Color indicatorColor;

        public readonly float indicatorWeight;

        public readonly EdgeInsets indicatorPadding;

        public readonly Decoration indicator;

        public readonly TabBarIndicatorSize? indicatorSize;

        public readonly Color labelColor;

        public readonly Color unselectedLabelColor;

        public readonly TextStyle labelStyle;

        public readonly EdgeInsets labelPadding;

        public readonly TextStyle unselectedLabelStyle;

        public readonly DragStartBehavior dragStartBehavior;

        public readonly ValueChanged<int> onTap;

        public override Size preferredSize {
            get {
                foreach (Widget item in tabs) {
                    if (item is Tab) {
                        Tab tab = (Tab) item;
                        if (tab.text != null && tab.icon != null) {
                            return Size.fromHeight(TabsUtils._kTextAndIconTabHeight + indicatorWeight);
                        }
                    }
                }

                return Size.fromHeight(TabsUtils._kTabHeight + indicatorWeight);
            }
        }


        public override State createState() {
            return new _TabBarState();
        }
    }


    class _TabBarState : State<TabBar> {
        ScrollController _scrollController;
        TabController _controller;
        _IndicatorPainter _indicatorPainter;
        int _currentIndex;
        List<GlobalKey> _tabKeys;


        public override void initState() {
            base.initState();
            _tabKeys = new List<GlobalKey>();
            foreach (Widget tab in widget.tabs) {
                _tabKeys.Add(GlobalKey.key());
            }
        }

        Decoration _indicator {
            get {
                if (widget.indicator != null) {
                    return widget.indicator;
                }

                TabBarTheme tabBarTheme = TabBarTheme.of(context);
                if (tabBarTheme.indicator != null) {
                    return tabBarTheme.indicator;
                }

                Color color = widget.indicatorColor ?? Theme.of(context).indicatorColor;
                if (color.value == Material.of(context).color?.value) {
                    color = Colors.white;
                }

                return new UnderlineTabIndicator(
                    insets: widget.indicatorPadding,
                    borderSide: new BorderSide(
                        width: widget.indicatorWeight,
                        color: color));
            }
        }

        void _updateTabController() {
            TabController newController = widget.controller ?? DefaultTabController.of(context);
            D.assert(() => {
                if (newController == null) {
                    throw new UIWidgetsError(
                        "No TabController for " + widget.GetType() + ".\n" +
                        "When creating a " + widget.GetType() + ", you must either provide an explicit " +
                        "TabController using the \"controller\" property, or you must ensure that there " +
                        "is a DefaultTabController above the " + widget.GetType() + ".\n" +
                        "In this case, there was neither an explicit controller nor a default controller."
                    );
                }

                return true;
            });
            D.assert(() => {
                if (newController.length != widget.tabs.Count) {
                    throw new UIWidgetsError(
                        $"Controller's length property {newController.length} does not match the\n" +
                        $"number of tab elements {widget.tabs.Count} present in TabBar's tabs property."
                    );
                }

                return true;
            });
            if (newController == _controller) {
                return;
            }

            if (_controller != null) {
                _controller.animation.removeListener(_handleTabControllerAnimationTick);
                _controller.removeListener(_handleTabControllerTick);
            }

            _controller = newController;
            if (_controller != null) {
                _controller.animation.addListener(_handleTabControllerAnimationTick);
                _controller.addListener(_handleTabControllerTick);
                _currentIndex = _controller.index;
            }
        }

        void _initIndicatorPainter() {
            _indicatorPainter = _controller == null
                ? null
                : new _IndicatorPainter(
                    controller: _controller,
                    indicator: _indicator,
                    indicatorSize: widget.indicatorSize ?? TabBarTheme.of(context).indicatorSize,
                    tabKeys: _tabKeys,
                    old: _indicatorPainter
                );
        }

        public override void didChangeDependencies() {
            base.didChangeDependencies();
            D.assert(MaterialD.debugCheckHasMaterial(context));
            _updateTabController();
            _initIndicatorPainter();
        }

        public override void didUpdateWidget(StatefulWidget oldWidget) {
            base.didUpdateWidget(oldWidget);
            TabBar _oldWidget = (TabBar) oldWidget;
            if (widget.controller != _oldWidget.controller) {
                _updateTabController();
                _initIndicatorPainter();
            }
            else if (widget.indicatorColor != _oldWidget.indicatorColor ||
                     widget.indicatorWeight != _oldWidget.indicatorWeight ||
                     widget.indicatorSize != _oldWidget.indicatorSize ||
                     widget.indicator != _oldWidget.indicator) {
                _initIndicatorPainter();
            }

            if (widget.tabs.Count > _oldWidget.tabs.Count) {
                int delta = widget.tabs.Count - _oldWidget.tabs.Count;
                for (int i = 0; i < delta; i++) {
                    _tabKeys.Add(GlobalKey.key());
                }
            }
            else if (widget.tabs.Count < _oldWidget.tabs.Count) {
                int delta = _oldWidget.tabs.Count - widget.tabs.Count;
                _tabKeys.RemoveRange(widget.tabs.Count, delta);
            }
        }

        public override void dispose() {
            _indicatorPainter.dispose();
            if (_controller != null) {
                _controller.animation.removeListener(_handleTabControllerAnimationTick);
                _controller.removeListener(_handleTabControllerTick);
            }

            base.dispose();
        }

        public int maxTabIndex {
            get { return _indicatorPainter.maxTabIndex; }
        }

        float _tabScrollOffset(int index, float viewportWidth, float minExtent, float maxExtent) {
            if (!widget.isScrollable) {
                return 0.0f;
            }

            float tabCenter = _indicatorPainter.centerOf(index);
            return (tabCenter - viewportWidth / 2.0f).clamp(minExtent, maxExtent);
        }

        float _tabCenteredScrollOffset(int index) {
            ScrollPosition position = _scrollController.position;
            return _tabScrollOffset(index, position.viewportDimension, position.minScrollExtent,
                position.maxScrollExtent);
        }

        internal float _initialScrollOffset(float viewportWidth, float minExtent, float maxExtent) {
            return _tabScrollOffset(_currentIndex, viewportWidth, minExtent, maxExtent);
        }

        void _scrollToCurrentIndex() {
            float offset = _tabCenteredScrollOffset(_currentIndex);
            _scrollController.animateTo(offset, duration: Constants.kTabScrollDuration, curve: Curves.ease);
        }

        void _scrollToControllerValue() {
            float? leadingPosition = _currentIndex > 0
                ? (float?) _tabCenteredScrollOffset(_currentIndex - 1)
                : null;
            float middlePosition = _tabCenteredScrollOffset(_currentIndex);
            float? trailingPosition = _currentIndex < maxTabIndex
                ? (float?) _tabCenteredScrollOffset(_currentIndex + 1)
                : null;

            float index = _controller.index;
            float value = _controller.animation.value;
            float offset = 0.0f;
            if (value == index - 1.0f) {
                offset = leadingPosition ?? middlePosition;
            }
            else if (value == index + 1.0f) {
                offset = trailingPosition ?? middlePosition;
            }
            else if (value == index) {
                offset = middlePosition;
            }
            else if (value < index) {
                offset = leadingPosition == null
                    ? middlePosition
                    : MathUtils.lerpNullableFloat(middlePosition, leadingPosition, index - value).Value;
            }
            else {
                offset = trailingPosition == null
                    ? middlePosition
                    : MathUtils.lerpNullableFloat(middlePosition, trailingPosition, value - index).Value;
            }

            _scrollController.jumpTo(offset);
        }


        void _handleTabControllerAnimationTick() {
            D.assert(mounted);
            if (!_controller.indexIsChanging && widget.isScrollable) {
                _currentIndex = _controller.index;
                _scrollToControllerValue();
            }
        }

        void _handleTabControllerTick() {
            if (_controller.index != _currentIndex) {
                _currentIndex = _controller.index;
                if (widget.isScrollable) {
                    _scrollToCurrentIndex();
                }
            }

            setState(() => { });
        }

        void _saveTabOffsets(List<float> tabOffsets, float width) {
            _indicatorPainter?.saveTabOffsets(tabOffsets);
        }

        void _handleTap(int index) {
            D.assert(index >= 0 && index < widget.tabs.Count);
            _controller.animateTo(index);
            if (widget.onTap != null) {
                widget.onTap(index);
            }
        }

        Widget _buildStyledTab(Widget child, bool selected, Animation<float> animation) {
            return new _TabStyle(
                animation: animation,
                selected: selected,
                labelColor: widget.labelColor,
                unselectedLabelColor: widget.unselectedLabelColor,
                labelStyle: widget.labelStyle,
                unselectedLabelStyle: widget.unselectedLabelStyle,
                child: child
            );
        }

        public override Widget build(BuildContext context) {
            D.assert(MaterialD.debugCheckHasMaterialLocalizations(context));

            if (_controller.length == 0) {
                return new Container(
                    height: TabsUtils._kTabHeight + widget.indicatorWeight
                );
            }

            TabBarTheme tabBarTheme = TabBarTheme.of(context);

            List<Widget> wrappedTabs = new List<Widget>();
            for (int i = 0; i < widget.tabs.Count; i++) {
                wrappedTabs.Add(new Center(
                        heightFactor: 1.0f,
                        child: new Padding(
                            padding: widget.labelPadding ?? tabBarTheme.labelPadding ?? Constants.kTabLabelPadding,
                            child: new KeyedSubtree(
                                key: _tabKeys[i],
                                child: widget.tabs[i]
                            )
                        )
                    )
                );
            }

            if (_controller != null) {
                int previousIndex = _controller.previousIndex;

                if (_controller.indexIsChanging) {
                    D.assert(_currentIndex != previousIndex);
                    Animation<float> animation = new _ChangeAnimation(_controller);
                    wrappedTabs[_currentIndex] =
                        _buildStyledTab(wrappedTabs[_currentIndex], true, animation);
                    wrappedTabs[previousIndex] = _buildStyledTab(wrappedTabs[previousIndex], false, animation);
                }
                else {
                    int tabIndex = _currentIndex;
                    Animation<float> centerAnimation = new _DragAnimation(_controller, tabIndex);
                    wrappedTabs[tabIndex] = _buildStyledTab(wrappedTabs[tabIndex], true, centerAnimation);
                    if (_currentIndex > 0) {
                        int previousTabIndex = _currentIndex - 1;
                        Animation<float> previousAnimation =
                            new ReverseAnimation(new _DragAnimation(_controller, previousTabIndex));
                        wrappedTabs[previousTabIndex] =
                            _buildStyledTab(wrappedTabs[previousTabIndex], false, previousAnimation);
                    }

                    if (_currentIndex < widget.tabs.Count - 1) {
                        int nextTabIndex = _currentIndex + 1;
                        Animation<float> nextAnimation =
                            new ReverseAnimation(new _DragAnimation(_controller, nextTabIndex));
                        wrappedTabs[nextTabIndex] =
                            _buildStyledTab(wrappedTabs[nextTabIndex], false, nextAnimation);
                    }
                }
            }

            int tabCount = widget.tabs.Count;
            for (int index = 0; index < tabCount; index++) {
                int tabIndex = index;
                wrappedTabs[index] = new InkWell(
                    onTap: () => { _handleTap(tabIndex); },
                    child: new Padding(
                        padding: EdgeInsets.only(bottom: widget.indicatorWeight),
                        child: wrappedTabs[index]
                    )
                );
                if (!widget.isScrollable) {
                    wrappedTabs[index] = new Expanded(
                        child: wrappedTabs[index]);
                }
            }

            Widget tabBar = new CustomPaint(
                painter: _indicatorPainter,
                child: new _TabStyle(
                    animation: Animations.kAlwaysDismissedAnimation,
                    selected: false,
                    labelColor: widget.labelColor,
                    unselectedLabelColor: widget.unselectedLabelColor,
                    labelStyle: widget.labelStyle,
                    unselectedLabelStyle: widget.unselectedLabelStyle,
                    child: new _TabLabelBar(
                        onPerformLayout: _saveTabOffsets,
                        children: wrappedTabs
                    )
                )
            );

            if (widget.isScrollable) {
                _scrollController = _scrollController ?? new _TabBarScrollController(this);
                tabBar = new SingleChildScrollView(
                    dragStartBehavior: widget.dragStartBehavior,
                    scrollDirection: Axis.horizontal,
                    controller: _scrollController,
                    child: tabBar);
            }

            return tabBar;
        }
    }


    public class TabBarView : StatefulWidget {
        public TabBarView(
            Key key = null,
            List<Widget> children = null,
            TabController controller = null,
            ScrollPhysics physics = null,
            DragStartBehavior dragStartBehavior = DragStartBehavior.start
        ) : base(key: key) {
            D.assert(children != null);
            this.children = children;
            this.controller = controller;
            this.physics = physics;
            this.dragStartBehavior = dragStartBehavior;
        }

        public readonly TabController controller;

        public readonly List<Widget> children;

        public readonly ScrollPhysics physics;

        public readonly DragStartBehavior dragStartBehavior;

        public override State createState() {
            return new _TabBarViewState();
        }
    }

    class _TabBarViewState : State<TabBarView> {
        TabController _controller;
        PageController _pageController;
        List<Widget> _children;
        int? _currentIndex = null;
        int _warpUnderwayCount = 0;

        void _updateTabController() {
            TabController newController = widget.controller ?? DefaultTabController.of(context);
            D.assert(() => {
                if (newController == null) {
                    throw new UIWidgetsError(
                        "No TabController for " + widget.GetType() + "\n" +
                        "When creating a " + widget.GetType() + ", you must either provide an explicit " +
                        "TabController using the \"controller\" property, or you must ensure that there " +
                        "is a DefaultTabController above the " + widget.GetType() + ".\n" +
                        "In this case, there was neither an explicit controller nor a default controller."
                    );
                }

                return true;
            });
            D.assert(() => {
                if (newController.length != widget.children.Count) {
                    throw new UIWidgetsError(
                        $"Controller's length property {newController.length} does not match the\n" +
                        $"number of elements {widget.children.Count} present in TabBarView's children property."
                    );
                }

                return true;
            });
            if (newController == _controller) {
                return;
            }

            if (_controller != null) {
                _controller.animation.removeListener(_handleTabControllerAnimationTick);
            }

            _controller = newController;
            if (_controller != null) {
                _controller.animation.addListener(_handleTabControllerAnimationTick);
            }
        }


        public override void initState() {
            base.initState();
            _children = widget.children;
        }

        public override void didChangeDependencies() {
            base.didChangeDependencies();
            _updateTabController();
            _currentIndex = _controller?.index;
            _pageController = new PageController(initialPage: _currentIndex ?? 0);
        }

        public override void didUpdateWidget(StatefulWidget oldWidget) {
            base.didUpdateWidget(oldWidget);
            TabBarView _oldWidget = (TabBarView) oldWidget;
            if (widget.controller != _oldWidget.controller) {
                _updateTabController();
            }

            if (widget.children != _oldWidget.children && _warpUnderwayCount == 0) {
                _children = widget.children;
            }
        }

        public override void dispose() {
            if (_controller != null) {
                _controller.animation.removeListener(_handleTabControllerAnimationTick);
            }

            base.dispose();
        }

        void _handleTabControllerAnimationTick() {
            if (_warpUnderwayCount > 0 || !_controller.indexIsChanging) {
                return;
            }

            if (_controller.index != _currentIndex) {
                _currentIndex = _controller.index;
                _warpToCurrentIndex();
            }
        }

        void _warpToCurrentIndex() {
            if (!mounted) {
                return;
            }

            if (_pageController.page == _currentIndex) {
                return;
            }

            int previousIndex = _controller.previousIndex;
            if ((_currentIndex.Value - previousIndex).abs() == 1) {
                _pageController.animateToPage(_currentIndex.Value, duration: Constants.kTabScrollDuration,
                    curve: Curves.ease);
                return;
            }

            D.assert((_currentIndex.Value - previousIndex).abs() > 1);
            int initialPage = 0;
            setState(() => {
                _warpUnderwayCount += 1;
                _children = new List<Widget>(widget.children);
                if (_currentIndex > previousIndex) {
                    _children[_currentIndex.Value - 1] = _children[previousIndex];
                    initialPage = _currentIndex.Value - 1;
                }
                else {
                    _children[_currentIndex.Value + 1] = _children[previousIndex];
                    initialPage = _currentIndex.Value + 1;
                }
            });

            _pageController.jumpToPage(initialPage);
            _pageController.animateToPage(_currentIndex.Value, duration: Constants.kTabScrollDuration,
                curve: Curves.ease).Then(() => {
                if (!mounted) {
                    return new Promise();
                }

                setState(() => {
                    _warpUnderwayCount -= 1;
                    _children = widget.children;
                });

                return new Promise();
            });
        }

        bool _handleScrollNotification(ScrollNotification notification) {
            if (_warpUnderwayCount > 0) {
                return false;
            }

            if (notification.depth != 0) {
                return false;
            }

            _warpUnderwayCount += 1;
            if (notification is ScrollUpdateNotification && !_controller.indexIsChanging) {
                if ((_pageController.page - _controller.index).abs() > 1.0) {
                    _controller.index = _pageController.page.floor();
                    _currentIndex = _controller.index;
                }

                _controller.offset = (_pageController.page - _controller.index).clamp(-1.0f, 1.0f);
            }
            else if (notification is ScrollEndNotification) {
                _controller.index = _pageController.page.round();
                _currentIndex = _controller.index;
            }

            _warpUnderwayCount -= 1;

            return false;
        }

        public override Widget build(BuildContext context) {
            return new NotificationListener<ScrollNotification>(
                onNotification: _handleScrollNotification,
                child: new PageView(
                    dragStartBehavior: widget.dragStartBehavior,
                    controller: _pageController,
                    physics: widget.physics == null
                        ? TabsUtils._kTabBarViewPhysics
                        : TabsUtils._kTabBarViewPhysics.applyTo(widget.physics),
                    children: _children
                )
            );
        }
    }


    public class TabPageSelectorIndicator : StatelessWidget {
        public TabPageSelectorIndicator(
            Key key = null,
            Color backgroundColor = null,
            Color borderColor = null,
            float? size = null
        ) : base(key: key) {
            D.assert(backgroundColor != null);
            D.assert(borderColor != null);
            D.assert(size != null);

            this.backgroundColor = backgroundColor;
            this.borderColor = borderColor;
            this.size = size.Value;
        }

        public readonly Color backgroundColor;

        public readonly Color borderColor;

        public readonly float size;

        public override Widget build(BuildContext context) {
            return new Container(
                width: size,
                height: size,
                margin: EdgeInsets.all(4.0f),
                decoration: new BoxDecoration(
                    color: backgroundColor,
                    border: Border.all(color: borderColor),
                    shape: BoxShape.circle
                )
            );
        }
    }


    public class TabPageSelector : StatelessWidget {
        public TabPageSelector(
            Key key = null,
            TabController controller = null,
            float indicatorSize = 12.0f,
            Color color = null,
            Color selectedColor = null
        ) : base(key: key) {
            D.assert(indicatorSize > 0.0f);
            this.controller = controller;
            this.indicatorSize = indicatorSize;
            this.color = color;
            this.selectedColor = selectedColor;
        }

        public readonly TabController controller;

        public readonly float indicatorSize;

        public readonly Color color;

        public readonly Color selectedColor;

        Widget _buildTabIndicator(
            int tabIndex,
            TabController tabController,
            ColorTween selectedColorTween,
            ColorTween previousColorTween) {
            Color background = null;
            if (tabController.indexIsChanging) {
                float t = 1.0f - TabsUtils._indexChangeProgress(tabController);
                if (tabController.index == tabIndex) {
                    background = selectedColorTween.lerp(t);
                }
                else if (tabController.previousIndex == tabIndex) {
                    background = previousColorTween.lerp(t);
                }
                else {
                    background = selectedColorTween.begin;
                }
            }
            else {
                float offset = tabController.offset;
                if (tabController.index == tabIndex) {
                    background = selectedColorTween.lerp(1.0f - offset.abs());
                }
                else if (tabController.index == tabIndex - 1 && offset > 0.0) {
                    background = selectedColorTween.lerp(offset);
                }
                else if (tabController.index == tabIndex + 1 && offset < 0.0) {
                    background = selectedColorTween.lerp(-offset);
                }
                else {
                    background = selectedColorTween.begin;
                }
            }

            return new TabPageSelectorIndicator(
                backgroundColor: background,
                borderColor: selectedColorTween.end,
                size: indicatorSize
            );
        }

        public override Widget build(BuildContext context) {
            Color fixColor = color ?? Colors.transparent;
            Color fixSelectedColor = selectedColor ?? Theme.of(context).accentColor;
            ColorTween selectedColorTween = new ColorTween(begin: fixColor, end: fixSelectedColor);
            ColorTween previousColorTween = new ColorTween(begin: fixSelectedColor, end: fixColor);
            TabController tabController = controller ?? DefaultTabController.of(context);
            D.assert(() => {
                if (tabController == null) {
                    throw new UIWidgetsError(
                        "No TabController for " + GetType() + ".\n" +
                        "When creating a " + GetType() + ", you must either provide an explicit TabController " +
                        "using the \"controller\" property, or you must ensure that there is a " +
                        "DefaultTabController above the " + GetType() + ".\n" +
                        "In this case, there was neither an explicit controller nor a default controller."
                    );
                }

                return true;
            });

            Animation<float> animation = new CurvedAnimation(
                parent: tabController.animation,
                curve: Curves.fastOutSlowIn
            );

            return new AnimatedBuilder(
                animation: animation,
                builder: (BuildContext subContext, Widget child) => {
                    List<Widget> children = new List<Widget>();

                    for (int tabIndex = 0; tabIndex < tabController.length; tabIndex++) {
                        children.Add(_buildTabIndicator(
                            tabIndex,
                            tabController,
                            selectedColorTween,
                            previousColorTween)
                        );
                    }

                    return new Row(
                        mainAxisSize: MainAxisSize.min,
                        children: children
                    );
                }
            );
        }
    }
}