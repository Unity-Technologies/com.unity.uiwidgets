using System;
using System.Collections.Generic;
using System.Linq;
using uiwidgets;
using Unity.UIWidgets.animation;
using Unity.UIWidgets.async;
using Unity.UIWidgets.external;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.gestures;
using Unity.UIWidgets.material;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.rendering;
using Unity.UIWidgets.service;
using Unity.UIWidgets.services;
using Unity.UIWidgets.ui;
using Unity.UIWidgets.widgets;
using UnityEngine;
using Canvas = Unity.UIWidgets.ui.Canvas;
using Color = Unity.UIWidgets.ui.Color;
using Material = UnityEngine.Material;
using Rect = Unity.UIWidgets.ui.Rect;
using TextStyle = Unity.UIWidgets.painting.TextStyle;

namespace Unity.UIWidgets.material {
    public partial class material_ {
        public static readonly TimeSpan _kDropdownMenuDuration = new TimeSpan(0, 0, 0, 0, 300);
        public const float _kMenuItemHeight = kMinInteractiveDimension;
        public const float _kDenseButtonHeight = 24.0f;
        public static readonly EdgeInsets _kMenuItemPadding = EdgeInsets.symmetric(horizontal: 16.0f);
        public static readonly EdgeInsetsGeometry _kAlignedButtonPadding = EdgeInsets.only(left: 16.0f, right: 4.0f);
        public static readonly EdgeInsets _kUnalignedButtonPadding = EdgeInsets.zero;
        public static readonly EdgeInsets _kAlignedMenuMargin = EdgeInsets.zero;
        public static readonly EdgeInsetsGeometry _kUnalignedMenuMargin = EdgeInsets.only(left: 16.0f, right: 24.0f);

        public delegate List<Widget> DropdownButtonBuilder(BuildContext context);
    }

    class _DropdownMenuPainter : AbstractCustomPainter {
        public _DropdownMenuPainter(
            Color color = null,
            int? elevation = null,
            int? selectedIndex = null,
            Animation<float> resize = null,
            ValueGetter<float> getSelectedItemOffset = null
        ) : base(repaint: resize) {
            D.assert(elevation != null);
            _painter = new BoxDecoration(
                color: color,
                borderRadius: BorderRadius.circular(2.0f),
                boxShadow: material_.kElevationToShadow[elevation ?? 0]
            ).createBoxPainter();
            this.color = color;
            this.elevation = elevation;
            this.selectedIndex = selectedIndex;
            this.resize = resize;
            this.getSelectedItemOffset = getSelectedItemOffset;
        }

        public readonly Color color;
        public readonly int? elevation;
        public readonly int? selectedIndex;
        public readonly Animation<float> resize;
        public readonly ValueGetter<float> getSelectedItemOffset;
        public readonly BoxPainter _painter;

        public override void paint(Canvas canvas, Size size) {
            float selectedItemOffset = getSelectedItemOffset();

            FloatTween top = new FloatTween(
                begin: selectedItemOffset.clamp(0.0f, size.height - material_._kMenuItemHeight),
                end: 0.0f
            );

            FloatTween bottom = new FloatTween(
                begin: (top.begin + material_._kMenuItemHeight).clamp(material_._kMenuItemHeight,
                    size.height),
                end: size.height
            );

            Rect rect = Rect.fromLTRB(0.0f, top.evaluate(resize), size.width, bottom.evaluate(resize));

            _painter.paint(canvas, rect.topLeft, new ImageConfiguration(size: rect.size));
        }

        public override bool shouldRepaint(CustomPainter painter) {
            _DropdownMenuPainter oldPainter = painter as _DropdownMenuPainter;
            return oldPainter.color != color
                   || oldPainter.elevation != elevation
                   || oldPainter.selectedIndex != selectedIndex
                   || oldPainter.resize != resize;
        }
    }

    class _DropdownScrollBehavior : ScrollBehavior {
        public _DropdownScrollBehavior() {
        }

        public override Widget buildViewportChrome(BuildContext context, Widget child, AxisDirection axisDirection) {
            return child;
        }

        public override ScrollPhysics getScrollPhysics(BuildContext context) {
            return new ClampingScrollPhysics();
        }
    }

    class _DropdownMenuItemButton<T> : StatefulWidget {
        internal _DropdownMenuItemButton(
            Key key = null,
            _DropdownRoute<T> route = null,
            EdgeInsets padding = null,
            Rect buttonRect = null,
            BoxConstraints constraints = null,
            int? itemIndex = null
        ) : base(key: key) {
            this.route = route;
            this.padding = padding;
            this.buttonRect = buttonRect;
            this.constraints = constraints;
            this.itemIndex = itemIndex;
        }

        public _DropdownRoute<T> route;
        public EdgeInsets padding;
        public Rect buttonRect;
        public BoxConstraints constraints;
        public int? itemIndex;

        public override State createState() => new _DropdownMenuItemButtonState<T>();
    }

    class _DropdownMenuItemButtonState<T> : State<_DropdownMenuItemButton<T>>  {
        void _handleFocusChange(bool focused) {
            bool inTraditionalMode = false;
            switch (FocusManager.instance.highlightMode) {
                case FocusHighlightMode.touch:
                    inTraditionalMode = false;
                    break;
                case FocusHighlightMode.traditional:
                    inTraditionalMode = true;
                    break;
            }

            if (focused && inTraditionalMode) {
                _MenuLimits menuLimits = widget.route.getMenuLimits(
                    widget.buttonRect, widget.constraints.maxHeight, widget.itemIndex ?? 0);
                widget.route.scrollController.animateTo(
                    menuLimits.scrollOffset ?? 0,
                    curve: Curves.easeInOut,
                    duration: new TimeSpan(0, 0, 0, 0, 100)
                );
            }
        }

        void _handleOnTap() {
            DropdownMenuItem<T> dropdownMenuItem = widget.route.items[widget.itemIndex ?? 0].item;

            if (dropdownMenuItem.onTap != null) {
                dropdownMenuItem.onTap();
            }

            Navigator.pop(
                context,
                new _DropdownRouteResult<T>(dropdownMenuItem.value)
            );
        }

        static readonly Dictionary<LogicalKeySet, Intent> _webShortcuts = new Dictionary<LogicalKeySet, Intent> {
            {new LogicalKeySet(LogicalKeyboardKey.enter), new Intent(ActivateAction.key)}
        };

        public override Widget build(BuildContext context) {
            CurvedAnimation opacity;

            float unit = 0.5f / (widget.route.items.Count + 1.5f);
            if (widget.itemIndex == widget.route.selectedIndex) {
                opacity = new CurvedAnimation(parent: widget.route.animation, curve: new Threshold(0.0f));
            }
            else {
                float start = ((0.5f + (widget.itemIndex + 1) * unit) ?? 0).clamp(0.0f, 1.0f);
                float end = (start + 1.5f * unit).clamp(0.0f, 1.0f);
                opacity = new CurvedAnimation(parent: widget.route.animation, curve: new Interval(start, end));
            }

            Widget child = new FadeTransition(
                opacity: opacity,
                child: new InkWell(
                    autofocus: widget.itemIndex == widget.route.selectedIndex,
                    child: new Container(
                        padding: widget.padding,
                        child: widget.route.items[widget.itemIndex ?? 0]
                    ),
                    onTap: _handleOnTap,
                    onFocusChange: _handleFocusChange
                )
            );

            return child;
        }
    }

    class _DropdownMenu<T> : StatefulWidget {
        public _DropdownMenu(
            Key key = null,
            EdgeInsets padding = null,
            _DropdownRoute<T> route = null,
            Rect buttonRect = null,
            BoxConstraints constraints = null,
            Color dropdownColor = null
        ) : base(key: key) {
            this.route = route;
            this.padding = padding;
            this.buttonRect = buttonRect;
            this.constraints = constraints;
            this.dropdownColor = dropdownColor;
        }

        public readonly _DropdownRoute<T> route;
        public readonly EdgeInsets padding;
        public readonly Rect buttonRect;
        public readonly BoxConstraints constraints;
        public readonly Color dropdownColor;


        public override State createState() {
            return new _DropdownMenuState<T>();
        }
    }

    class _DropdownMenuState<T> : State<_DropdownMenu<T>> {
        CurvedAnimation _fadeOpacity;
        CurvedAnimation _resize;

        public _DropdownMenuState() {
        }

        public override void initState() {
            base.initState();
            _fadeOpacity = new CurvedAnimation(
                parent: widget.route.animation,
                curve: new Interval(0.0f, 0.25f),
                reverseCurve: new Interval(0.75f, 1.0f)
            );
            _resize = new CurvedAnimation(
                parent: widget.route.animation,
                curve: new Interval(0.25f, 0.5f),
                reverseCurve: new Threshold(0.0f)
            );
        }

        public override Widget build(BuildContext context) {
            D.assert(material_.debugCheckHasMaterialLocalizations(context));
            MaterialLocalizations localizations = MaterialLocalizations.of(context);
            _DropdownRoute<T> route = widget.route;
            List<Widget> children = new List<Widget>();
            for (int itemIndex = 0; itemIndex < route.items.Count; ++itemIndex) {
                children.Add(new _DropdownMenuItemButton<T>(
                    route: widget.route,
                    padding: widget.padding,
                    buttonRect: widget.buttonRect,
                    constraints: widget.constraints,
                    itemIndex: itemIndex
                ));
            }

            return new FadeTransition(
                opacity: _fadeOpacity,
                child: new CustomPaint(
                    painter: new _DropdownMenuPainter(
                        color: widget.dropdownColor ?? Theme.of(context).canvasColor,
                        elevation: route.elevation,
                        selectedIndex: route.selectedIndex,
                        resize: _resize,
                        getSelectedItemOffset: () => route.getItemOffset(route.selectedIndex ?? 0)
                    ),
                    child: new Material(
                        type: MaterialType.transparency,
                        textStyle: route.style,
                        child: new ScrollConfiguration(
                            behavior: new _DropdownScrollBehavior(),
                            child: new Scrollbar(
                                child: new ListView(
                                    controller: widget.route.scrollController,
                                    padding: material_.kMaterialListPadding,
                                    shrinkWrap: true,
                                    children: children
                                )
                            )
                        )
                    )
                )
            );
        }
    }

    class _DropdownMenuRouteLayout<T> : SingleChildLayoutDelegate {
        public _DropdownMenuRouteLayout(
            Rect buttonRect,
            _DropdownRoute<T> route = null,
            TextDirection? textDirection = null
        ) {
            this.buttonRect = buttonRect;
            this.route = route;
            this.textDirection = textDirection;
        }

        public readonly Rect buttonRect;
        public readonly _DropdownRoute<T> route;
        public readonly TextDirection? textDirection;

        public override BoxConstraints getConstraintsForChild(BoxConstraints constraints) {
            float maxHeight = Mathf.Max(0.0f, constraints.maxHeight - 2 * material_._kMenuItemHeight);
            float width = Mathf.Min(constraints.maxWidth, buttonRect.width);
            return new BoxConstraints(
                minWidth: width,
                maxWidth: width,
                minHeight: 0.0f,
                maxHeight: maxHeight
            );
        }

        public override Offset getPositionForChild(Size size, Size childSize) {
            _MenuLimits menuLimits = route.getMenuLimits(buttonRect, size.height, route.selectedIndex ?? 0);
            D.assert(() => {
                Rect container = Offset.zero & size;
                if (container.intersect(buttonRect) == buttonRect) {
                    D.assert(menuLimits.top >= 0.0f);
                    D.assert(menuLimits.top + menuLimits.height <= size.height);
                }

                return true;
            });
            D.assert(textDirection != null);
            float left = 0;
            switch (textDirection) {
                case TextDirection.rtl:
                    left = (buttonRect.right.clamp(0.0f, size.width)) - childSize.width;
                    break;
                case TextDirection.ltr:
                    left = buttonRect.left.clamp(0.0f, size.width - childSize.width);
                    break;
            }

            return new Offset(left, menuLimits.top ?? 0);
        }

        public override bool shouldRelayout(SingleChildLayoutDelegate _oldDelegate) {
            _DropdownMenuRouteLayout<T> oldDelegate = _oldDelegate as _DropdownMenuRouteLayout<T>;
            return buttonRect != oldDelegate.buttonRect || textDirection != oldDelegate.textDirection;
        }
    }

    class _DropdownRouteResult<T> {
        public _DropdownRouteResult(T result) {
            this.result = result;
        }

        public readonly T result;

        public static bool operator ==(_DropdownRouteResult<T> left, _DropdownRouteResult<T> right) {
            return left.Equals(right);
        }

        public static bool operator !=(_DropdownRouteResult<T> left, _DropdownRouteResult<T> right) {
            return !left.Equals(right);
        }

        public bool Equals(_DropdownRouteResult<T> other) {
            return result.Equals(other.result);
        }

        public override bool Equals(object obj) {
            if (ReferenceEquals(null, obj)) {
                return false;
            }

            if (ReferenceEquals(this, obj)) {
                return true;
            }

            if (obj.GetType() != GetType()) {
                return false;
            }

            return Equals((_DropdownRouteResult<T>) obj);
        }

        public override int GetHashCode() {
            return result.GetHashCode();
        }
    }

    class _MenuLimits {
        internal _MenuLimits(float? top, float? bottom, float? height, float? scrollOffset) {
            this.top = top;
            this.bottom = bottom;
            this.height = height;
            this.scrollOffset = scrollOffset;
        }

        public readonly float? top;
        public readonly float? bottom;
        public readonly float? height;
        public readonly float? scrollOffset;
    }

    class _DropdownRoute<T> : PopupRoute<_DropdownRouteResult<T>>  {
        public _DropdownRoute(
            List<_MenuItem<T>> items = null,
            EdgeInsetsGeometry padding = null,
            Rect buttonRect = null,
            int? selectedIndex = null,
            int elevation = 8,
            ThemeData theme = null,
            TextStyle style = null,
            string barrierLabel = null,
            float? itemHeight = null,
            Color dropdownColor = null
        ) {
            D.assert(style != null);
            this.items = items;
            this.padding = padding;
            this.buttonRect = buttonRect;
            this.selectedIndex = selectedIndex;
            this.elevation = elevation;
            this.theme = theme;
            this.style = style;
            this.barrierLabel = barrierLabel;
            this.itemHeight = itemHeight;
            this.dropdownColor = dropdownColor;
            itemHeights = Enumerable.Repeat(itemHeight ?? material_.kMinInteractiveDimension, items.Count).ToList();
        }

        public readonly List<_MenuItem<T>> items;
        public readonly EdgeInsetsGeometry padding;
        public readonly Rect buttonRect;
        public readonly int? selectedIndex;
        public readonly int elevation;
        public readonly ThemeData theme;
        public readonly TextStyle style;
        public readonly float? itemHeight;
        public readonly Color dropdownColor;

        public readonly List<float> itemHeights;
        public ScrollController scrollController;

        public override TimeSpan transitionDuration {
            get { return material_._kDropdownMenuDuration; }
        }

        public override bool barrierDismissible {
            get { return true; }
        }

        public override Color barrierColor {
            get { return null; }
        }

        public override string barrierLabel { get; }

        public override Widget buildPage(BuildContext context, Animation<float> animation,
            Animation<float> secondaryAnimation) {
            return new LayoutBuilder(
                builder: (ctx, constraints) => {
                    return new _DropdownRoutePage<T>(
                        route: this,
                        constraints: constraints,
                        items: items,
                        padding: padding,
                        buttonRect: buttonRect,
                        selectedIndex: selectedIndex,
                        elevation: elevation,
                        theme: theme,
                        style: style,
                        dropdownColor: dropdownColor
                    );
                }
            );
        }

        internal void _dismiss() {
            navigator?.removeRoute(this);
        }

        public float getItemOffset(int index) {
            float offset = material_.kMaterialListPadding.top;
            if (items.isNotEmpty() && index > 0) {
                D.assert(items.Count == itemHeights?.Count);
                offset += itemHeights
                    .Aggregate(0, (float total, float height) => total + height);
            }

            return offset;
        }

        public _MenuLimits getMenuLimits(Rect buttonRect, float availableHeight, int index) {
            float maxMenuHeight = availableHeight - 2.0f * material_._kMenuItemHeight;
            float buttonTop = buttonRect.top;
            float buttonBottom = Mathf.Min(buttonRect.bottom, availableHeight);
            float selectedItemOffset = getItemOffset(index);

            float topLimit = Mathf.Min(material_._kMenuItemHeight, buttonTop);
            float bottomLimit = Mathf.Max(availableHeight - material_._kMenuItemHeight, buttonBottom);

            float menuTop = (buttonTop - selectedItemOffset) -
                            (itemHeights[selectedIndex ?? 0] - buttonRect.height) / 2.0f;
            float preferredMenuHeight = material_.kMaterialListPadding.vertical;
            if (items.isNotEmpty())
                preferredMenuHeight += itemHeights.Aggregate((float total, float height) => total + height);

            float menuHeight = Mathf.Min(maxMenuHeight, preferredMenuHeight);
            float menuBottom = menuTop + menuHeight;

            if (menuTop < topLimit)
                menuTop = Mathf.Min(buttonTop, topLimit);

            if (menuBottom > bottomLimit) {
                menuBottom = Mathf.Max(buttonBottom, bottomLimit);
                menuTop = menuBottom - menuHeight;
            }

            float scrollOffset = preferredMenuHeight <= maxMenuHeight
                ? 0
                : Mathf.Max(0.0f, selectedItemOffset - (buttonTop - menuTop));

            return new _MenuLimits(menuTop, menuBottom, menuHeight, scrollOffset);
        }
    }

    class _DropdownRoutePage<T> : StatelessWidget {
        public _DropdownRoutePage(
            Key key = null,
            _DropdownRoute<T> route = null,
            BoxConstraints constraints = null,
            List<_MenuItem<T>> items = null,
            EdgeInsetsGeometry padding = null,
            Rect buttonRect = null,
            int? selectedIndex = null,
            int elevation = 0,
            ThemeData theme = null,
            TextStyle style = null,
            Color dropdownColor = null
        ) : base(key: key) {
            this.route = route;
            this.constraints = constraints;
            this.items = items;
            this.padding = padding;
            this.buttonRect = buttonRect;
            this.selectedIndex = selectedIndex;
            this.elevation = elevation;
            this.theme = theme;
            this.style = style;
            this.dropdownColor = dropdownColor;
        }

        public readonly _DropdownRoute<T> route;
        public readonly BoxConstraints constraints;
        public readonly List<_MenuItem<T>> items;
        public readonly EdgeInsetsGeometry padding;
        public readonly Rect buttonRect;
        public readonly int? selectedIndex;
        public readonly int elevation;
        public readonly ThemeData theme;
        public readonly TextStyle style;
        public readonly Color dropdownColor;

        public override Widget build(BuildContext context) {
            D.assert(WidgetsD.debugCheckHasDirectionality(context));
            if (route.scrollController == null) {
                _MenuLimits menuLimits = route.getMenuLimits(buttonRect, constraints.maxHeight, selectedIndex ?? 0);
                route.scrollController = new ScrollController(initialScrollOffset: menuLimits.scrollOffset ?? 0);
            }

            TextDirection textDirection = Directionality.of(context);
            Widget menu = new _DropdownMenu<T>(
                route: route,
                padding: padding.resolve(textDirection),
                buttonRect: buttonRect,
                constraints: constraints,
                dropdownColor: dropdownColor
            );

            if (theme != null) {
                menu = new Theme(data: theme, child: menu);
            }

            return MediaQuery.removePadding(
                context: context,
                removeTop: true,
                removeBottom: true,
                removeLeft: true,
                removeRight: true,
                child: new Builder(
                    builder: (BuildContext _context) => {
                        return new CustomSingleChildLayout(
                            layoutDelegate: new _DropdownMenuRouteLayout<T>(
                                buttonRect: buttonRect,
                                route: route,
                                textDirection: textDirection
                            ),
                            child: menu
                        );
                    }
                )
            );
        }
    }

    class _MenuItem<T> : SingleChildRenderObjectWidget  {
        internal _MenuItem(
            Key key = null,
            ValueChanged<Size> onLayout = null,
            DropdownMenuItem<T> item = null
        ) : base(key: key, child: item) {
            D.assert(onLayout != null);
            this.onLayout = onLayout;
            this.item = item;
        }

        public readonly ValueChanged<Size> onLayout;
        public readonly DropdownMenuItem<T> item;

        public override RenderObject createRenderObject(BuildContext context) {
            return new _RenderMenuItem(onLayout);
        }

        public override void updateRenderObject(BuildContext context, RenderObject renderObject) {
            if (renderObject is _RenderMenuItem renderMenuItem) {
                renderMenuItem.onLayout = onLayout;
            }
        }
    }

    class _RenderMenuItem : RenderProxyBox {
        internal _RenderMenuItem(ValueChanged<Size> onLayout = null, RenderBox child = null) : base(child) {
            D.assert(onLayout != null);
            this.onLayout = onLayout;
        }

        public ValueChanged<Size> onLayout;

        protected override void performLayout() {
            base.performLayout();
            onLayout(size);
        }
    }

    public class _DropdownMenuItemContainer : StatelessWidget {
        internal _DropdownMenuItemContainer(
            Key key = null,
            Widget child = null
        ) : base(key: key) {
            D.assert(child != null);
            this.child = child;
        }

        public readonly Widget child;

        public override Widget build(BuildContext context) {
            return new Container(
                constraints: new BoxConstraints(minHeight: material_._kMenuItemHeight),
                alignment: AlignmentDirectional.centerStart,
                child: child
            );
        }
    }

    public class DropdownMenuItem<T> : _DropdownMenuItemContainer {
        public DropdownMenuItem(
            Key key = null,
            VoidCallback onTap = null,
            T value = default,
            Widget child = null
        ) : base(key: key, child: child) {
            D.assert(child != null);
            this.onTap = onTap;
            this.value = value;
        }

        public readonly VoidCallback onTap;
        public readonly T value;
    }

    public class DropdownButtonHideUnderline : InheritedWidget {
        public DropdownButtonHideUnderline(
            Key key = null,
            Widget child = null
        ) : base(key: key, child: child) {
            D.assert(child != null);
        }

        public static bool at(BuildContext context) {
            return context.dependOnInheritedWidgetOfExactType<DropdownButtonHideUnderline>() != null;
        }

        public override bool updateShouldNotify(InheritedWidget oldWidget) {
            return false;
        }
    }

    public class DropdownButton<T> : StatefulWidget{
        public DropdownButton(
            Key key = null,
            List<DropdownMenuItem<T>> items = null,
            material_.DropdownButtonBuilder selectedItemBuilder = null,
            T value = default,
            Widget hint = null,
            Widget disabledHint = null,
            ValueChanged<T> onChanged = null,
            VoidCallback onTap = null,
            int elevation = 8,
            TextStyle style = null,
            Widget underline = null,
            Widget icon = null,
            Color iconDisabledColor = null,
            Color iconEnabledColor = null,
            float iconSize = 24.0f,
            bool isDense = false,
            bool isExpanded = false,
            float? itemHeight = material_.kMinInteractiveDimension,
            Color focusColor = null,
            FocusNode focusNode = null,
            bool autofocus = false,
            Color dropdownColor = null
        ) :
            base(key: key) {
            D.assert(items == null || items.isEmpty() || value == null ||
                     LinqUtils<DropdownMenuItem<T>>.WhereList(items,((DropdownMenuItem<T> item) => { return item.value.Equals(value); })).Count() == 1,
                () => "There should be exactly one item with [DropdownButton]'s value: " +
                      $"{value}. \n" +
                      "Either zero or 2 or more [DropdownMenuItem]s were detected " +
                      "with the same value"
            );
            D.assert(itemHeight == null || itemHeight >= material_.kMinInteractiveDimension);

            this.items = items;
            this.selectedItemBuilder = selectedItemBuilder;
            this.value = value;
            this.hint = hint;
            this.disabledHint = disabledHint;
            this.onChanged = onChanged;
            this.onTap = onTap;
            this.elevation = elevation;
            this.style = style;
            this.underline = underline;
            this.icon = icon;
            this.iconDisabledColor = iconDisabledColor;
            this.iconEnabledColor = iconEnabledColor;
            this.iconSize = iconSize;
            this.isDense = isDense;
            this.isExpanded = isExpanded;
            this.itemHeight = itemHeight;
            this.focusColor = focusColor;
            this.focusNode = focusNode;
            this.autofocus = autofocus;
            this.dropdownColor = dropdownColor;
        }

        public readonly List<DropdownMenuItem<T>> items;

        public readonly T value;

        public readonly Widget hint;

        public readonly Widget disabledHint;

        public readonly ValueChanged<T> onChanged;

        public readonly VoidCallback onTap;

        public readonly material_.DropdownButtonBuilder selectedItemBuilder;

        public readonly int elevation;

        public readonly TextStyle style;

        public readonly Widget underline;

        public readonly Widget icon;

        public readonly Color iconDisabledColor;

        public readonly Color iconEnabledColor;

        public readonly float iconSize;

        public readonly bool isDense;

        public readonly bool isExpanded;

        public readonly float? itemHeight;

        public readonly Color focusColor;

        public readonly FocusNode focusNode;

        public readonly bool autofocus;

        public readonly Color dropdownColor;

        public override State createState() {
            return new _DropdownButtonState<T>();
        }
    }

    class _DropdownButtonState<T> : State<DropdownButton<T>>, WidgetsBindingObserver{
        int? _selectedIndex;
        _DropdownRoute<T> _dropdownRoute;
        Orientation? _lastOrientation;
        FocusNode _internalNode;

        public FocusNode focusNode {
            get { return widget.focusNode ?? _internalNode; }
        }

        bool _hasPrimaryFocus = false;
        Dictionary<LocalKey, ActionFactory> _actionMap;
        FocusHighlightMode _focusHighlightMode;

        FocusNode _createFocusNode() {
            return new FocusNode(debugLabel: $"{widget.GetType()}");
        }

        public override void initState() {
            base.initState();
            _updateSelectedIndex();
            if (widget.focusNode == null) {
                _internalNode = _internalNode ?? _createFocusNode();
            }

            _actionMap = new Dictionary<LocalKey, ActionFactory>() {
                {ActivateAction.key, _createAction}
            };
            focusNode.addListener(_handleFocusChanged);
            FocusManager focusManager = WidgetsBinding.instance.focusManager;
            _focusHighlightMode = focusManager.highlightMode;
            focusManager.addHighlightModeListener(_handleFocusHighlightModeChange);
        }

        public override void dispose() {
            WidgetsBinding.instance.removeObserver(this);
            _removeDropdownRoute();
            WidgetsBinding.instance.focusManager.removeHighlightModeListener(_handleFocusHighlightModeChange);
            focusNode.removeListener(_handleFocusChanged);
            _internalNode?.dispose();
            base.dispose();
        }

        public void didChangeMetrics() {
            _removeDropdownRoute();
        }

        void _removeDropdownRoute() {
            _dropdownRoute?._dismiss();
            _dropdownRoute = null;
            _lastOrientation = null;
        }

        void _handleFocusChanged() {
            if (_hasPrimaryFocus != focusNode.hasPrimaryFocus) {
                setState(() => { _hasPrimaryFocus = focusNode.hasPrimaryFocus; });
            }
        }

        void _handleFocusHighlightModeChange(FocusHighlightMode mode) {
            if (!mounted) {
                return;
            }

            setState(() => { _focusHighlightMode = mode; });
        }

        public override void didUpdateWidget(StatefulWidget oldWidget) {
            base.didUpdateWidget(oldWidget);
            if (oldWidget is DropdownButton<T> dropdownButton && widget.focusNode != dropdownButton.focusNode) {
                dropdownButton.focusNode?.removeListener(_handleFocusChanged);
                if (widget.focusNode == null) {
                    _internalNode = _internalNode ?? _createFocusNode();
                }

                _hasPrimaryFocus = focusNode.hasPrimaryFocus;
                focusNode.addListener(_handleFocusChanged);
            }

            _updateSelectedIndex();
        }

        public void didChangeTextScaleFactor() {
        }

        public void didChangeLocales(List<Locale> locale) {
        }

        public Future<bool> didPopRoute() {
            return Future.value(false).to<bool>();
        }

        public Future<bool> didPushRoute(string route) {
            return Future.value(false).to<bool>();
        }

        public void didChangeAccessibilityFeatures() {
            //FIX ME!!!!
            throw new NotImplementedException();
        }

        public void didChangePlatformBrightness() {
        }


        void _updateSelectedIndex() {
            if (!_enabled) {
                return;
            }

            D.assert(widget.value == null ||
                     LinqUtils<DropdownMenuItem<T>>.WhereList(widget.items,((DropdownMenuItem<T> item) => item.value.Equals(widget.value))
                         ).Count == 1);
            _selectedIndex = null;
            for (int itemIndex = 0; itemIndex < widget.items.Count; itemIndex++) {
                if (widget.items[itemIndex].value.Equals(widget.value)) {
                    _selectedIndex = itemIndex;
                    return;
                }
            }
        }

        TextStyle _textStyle {
            get { return widget.style ?? Theme.of(context).textTheme.subtitle1; }
        }

        void _handleTap() {
            RenderBox itemBox = (RenderBox) context.findRenderObject();
            Rect itemRect = itemBox.localToGlobal(Offset.zero) & itemBox.size;
            TextDirection textDirection = Directionality.of(context);
            EdgeInsetsGeometry menuMargin = ButtonTheme.of(context).alignedDropdown
                ? material_._kAlignedMenuMargin
                : material_._kUnalignedMenuMargin;

            List<_MenuItem<T>> menuItems = new List<_MenuItem<T>>(new _MenuItem<T>[widget.items.Count]);
            for (int index = 0; index < widget.items.Count; index += 1) {
                var pindex = index;
                menuItems[pindex] = new _MenuItem<T>(
                    item: widget.items[pindex],
                    onLayout: (Size size) => {
                        if (_dropdownRoute == null)
                            return;

                        _dropdownRoute.itemHeights[pindex] = size.height;
                    }
                );
            }

            D.assert(_dropdownRoute == null);
            _dropdownRoute = new _DropdownRoute<T>(
                items: menuItems,
                buttonRect: menuMargin.resolve(textDirection).inflateRect(itemRect),
                padding: material_._kMenuItemPadding.resolve(textDirection),
                selectedIndex: _selectedIndex ?? 0,
                elevation: widget.elevation,
                theme: Theme.of(context, shadowThemeOnly: true),
                style: _textStyle,
                barrierLabel: MaterialLocalizations.of(context).modalBarrierDismissLabel,
                itemHeight: widget.itemHeight,
                dropdownColor: widget.dropdownColor
            );

            Navigator.push<T>(context, _dropdownRoute).then(newValue => {
                _DropdownRouteResult<T> value = newValue as _DropdownRouteResult<T>;
                _removeDropdownRoute();
                if (!mounted || newValue == null) {
                    return;
                }

                if (widget.onChanged != null) {
                    widget.onChanged(value.result);
                }
            });
            if (widget.onTap != null) {
                widget.onTap();
            }
        }

        UiWidgetAction _createAction() {
            return new CallbackAction(
                ActivateAction.key,
                onInvoke: (FocusNode node, Intent intent) => { _handleTap(); }
            );
        }

        float? _denseButtonHeight {
            get {
                float fontSize = _textStyle.fontSize ?? Theme.of(context).textTheme.subtitle1.fontSize ?? 0;

                return Mathf.Max(fontSize, Mathf.Max(widget.iconSize, material_._kDenseButtonHeight));
            }
        }

        Color _iconColor {
            get {
                if (_enabled) {
                    if (widget.iconEnabledColor != null) {
                        return widget.iconEnabledColor;
                    }

                    switch (Theme.of(context).brightness) {
                        case Brightness.light:
                            return Colors.grey.shade700;
                        case Brightness.dark:
                            return Colors.white70;
                    }
                }
                else {
                    if (widget.iconDisabledColor != null) {
                        return widget.iconDisabledColor;
                    }

                    switch (Theme.of(context).brightness) {
                        case Brightness.light:
                            return Colors.grey.shade400;
                        case Brightness.dark:
                            return Colors.white10;
                    }
                }

                D.assert(false);
                return null;
            }
        }

        bool _enabled {
            get { return widget.items != null && widget.items.isNotEmpty() && widget.onChanged != null; }
        }

        Orientation? _getOrientation(BuildContext context) {
            Orientation? result = MediaQuery.of(context, nullOk: true)?.orientation;
            if (result == null) {
                // If there's no MediaQuery, then use the window aspect to determine
                // orientation.
                Size size = Window.instance.physicalSize;
                result = size.width > size.height ? Orientation.landscape : Orientation.portrait;
            }

            return result;
        }

        bool? _showHighlight {
            get {
                switch (_focusHighlightMode) {
                    case FocusHighlightMode.touch:
                        return false;
                    case FocusHighlightMode.traditional:
                        return _hasPrimaryFocus;
                }

                return null;
            }
        }

        public override Widget build(BuildContext context) {
            D.assert(material_.debugCheckHasMaterial(context));
            D.assert(material_.debugCheckHasMaterialLocalizations(context));
            Orientation? newOrientation = _getOrientation(context);
            _lastOrientation = _lastOrientation ?? newOrientation;
            if (newOrientation != _lastOrientation) {
                _removeDropdownRoute();
                _lastOrientation = newOrientation;
            }

            List<Widget> items = null;
            if (_enabled) {
                items = widget.selectedItemBuilder == null
                    ? new List<Widget>(widget.items)
                    : widget.selectedItemBuilder(context);
            }
            else {
                items = widget.selectedItemBuilder == null
                    ? new List<Widget>()
                    : widget.selectedItemBuilder(context);
            }

            int hintIndex = 0;
            if (widget.hint != null || (!_enabled && widget.disabledHint != null)) {
                Widget displayedHint = _enabled ? widget.hint : widget.disabledHint ?? widget.hint;
                if (widget.selectedItemBuilder == null) {
                    displayedHint = new _DropdownMenuItemContainer(child: displayedHint);
                }

                hintIndex = items.Count;
                items.Add(new DefaultTextStyle(
                    style: _textStyle.copyWith(color: Theme.of(context).hintColor),
                    child: new IgnorePointer(
                        child: displayedHint
                    )
                ));
            }

            EdgeInsetsGeometry padding = ButtonTheme.of(context).alignedDropdown
                ? material_._kAlignedButtonPadding
                : material_._kUnalignedButtonPadding;

            int index = _enabled ? (_selectedIndex ?? hintIndex) : hintIndex;
            Widget innerItemsWidget = null;
            if (items.isEmpty()) {
                innerItemsWidget = new Container();
            }
            else {
                innerItemsWidget = new IndexedStack(
                    index: index,
                    alignment: AlignmentDirectional.centerStart,
                    children: widget.isDense
                        ? items
                        : LinqUtils<Widget>.SelectList(items,(Widget item) => {
                            return widget.itemHeight != null
                                ? new SizedBox(height: widget.itemHeight, child: item)
                                : (Widget) new Column(mainAxisSize: MainAxisSize.min,
                                    children: new List<Widget>() {item});
                        }));
            }

            Icon defaultIcon = new Icon(Icons.arrow_drop_down);
            Widget result = new DefaultTextStyle(
                style: _textStyle,
                child: new Container(
                    decoration: _showHighlight ?? false
                        ? new BoxDecoration(
                            color: widget.focusColor ?? Theme.of(context).focusColor,
                            borderRadius: BorderRadius.all(Radius.circular(4.0f))
                        )
                        : null,
                    padding: padding,
                    height: widget.isDense ? _denseButtonHeight : null,
                    child: new Row(
                        mainAxisAlignment: MainAxisAlignment.spaceBetween,
                        mainAxisSize: MainAxisSize.min,
                        children: new List<Widget> {
                            widget.isExpanded ? new Expanded(child: innerItemsWidget) : (Widget) innerItemsWidget,
                            new IconTheme(
                                data: new IconThemeData(
                                    color: _iconColor,
                                    size: widget.iconSize
                                ),
                                child: widget.icon ?? defaultIcon
                            )
                        }
                    )
                )
            );

            if (!DropdownButtonHideUnderline.at(context)) {
                float bottom = widget.isDense || widget.itemHeight == null ? 0.0f : 8.0f;
                result = new Stack(
                    children: new List<Widget> {
                        result,
                        new Positioned(
                            left: 0.0f,
                            right: 0.0f,
                            bottom: bottom,
                            child: widget.underline ?? new Container(
                                height: 1.0f,
                                decoration: new BoxDecoration(
                                    border: new Border(
                                        bottom: new BorderSide(color: new Color(0xFFBDBDBD), width: 0.0f))
                                )
                            )
                        )
                    }
                );
            }

            return new Focus(
                canRequestFocus: _enabled,
                focusNode: focusNode,
                autofocus: widget.autofocus,
                child: new GestureDetector(
                    onTap: _enabled ? (GestureTapCallback) _handleTap : null,
                    behavior: HitTestBehavior.opaque,
                    child: result
                )
            );
        }
    }

    public class DropdownButtonFormField<T> : FormField<T> where T : class {
        public DropdownButtonFormField(
            Key key = null,
            T value = null,
            List<DropdownMenuItem<T>> items = null,
            ValueChanged<T> onChanged = null,
            InputDecoration decoration = null,
            FormFieldSetter<T> onSaved = null,
            FormFieldValidator<T> validator = null,
            Widget hint = null
        ) : base(
            key: key,
            onSaved: onSaved,
            initialValue: value,
            validator: validator,
            builder: (FormFieldState<T> field) => {
                InputDecoration effectiveDecoration = (decoration ?? new InputDecoration())
                    .applyDefaults(Theme.of(field.context).inputDecorationTheme);
                return new InputDecorator(
                    decoration: effectiveDecoration.copyWith(errorText: field.errorText),
                    isEmpty: value == null,
                    child: new DropdownButtonHideUnderline(
                        child: new DropdownButton<T>(
                            isDense: true,
                            value: value,
                            items: items,
                            hint: hint,
                            onChanged: field.didChange
                        )
                    )
                );
            }
        ) {
            this.onChanged = onChanged;
        }

        public readonly ValueChanged<T> onChanged;

        public override State createState() {
            return new _DropdownButtonFormFieldState<T>();
        }
    }

    class _DropdownButtonFormFieldState<T> : FormFieldState<T> where T : class {
        public new DropdownButtonFormField<T> widget {
            get { return base.widget as DropdownButtonFormField<T>; }
        }

        public override void didChange(T value) {
            base.didChange(value);
            if (widget.onChanged != null) {
                widget.onChanged(value);
            }
        }
    }
}