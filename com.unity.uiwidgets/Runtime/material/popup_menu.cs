using System;
using System.Collections.Generic;
using Unity.UIWidgets.animation;
using Unity.UIWidgets.async;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.gestures;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.rendering;
using Unity.UIWidgets.ui;
using Unity.UIWidgets.widgets;
using UnityEngine;
using Color = Unity.UIWidgets.ui.Color;
using Rect = Unity.UIWidgets.ui.Rect;
using TextStyle = Unity.UIWidgets.painting.TextStyle;

namespace Unity.UIWidgets.material {
    public partial class material_ {
        internal static readonly TimeSpan _kMenuDuration = new TimeSpan(0, 0, 0, 0, 300);
        internal const float _kMenuCloseIntervalEnd = 2.0f / 3.0f;
        internal const float _kMenuHorizontalPadding = 16.0f;
        internal const float _kMenuDividerHeight = 16.0f;
        internal const float _kMenuMaxWidth = 5.0f * _kMenuWidthStep;
        internal const float _kMenuMinWidth = 2.0f * _kMenuWidthStep;
        internal const float _kMenuVerticalPadding = 8.0f;
        internal const float _kMenuWidthStep = 56.0f;
        internal const float _kMenuScreenPadding = 8.0f;
    }

    public abstract class PopupMenuEntry<T> : StatefulWidget {
        protected PopupMenuEntry(Key key = null) : base(key: key) {
        }

        public abstract float height { get; }

        public abstract bool represents(T value);
    }


    public class PopupMenuDivider<T> : PopupMenuEntry<T> {
        public PopupMenuDivider(Key key = null, float height = material_._kMenuDividerHeight) : base(key: key) {
            _height = height;
        }

        readonly float _height;

        public override float height {
            get { return _height; }
        }

        public override bool represents(T value) {
            return false;
        }

        public override State createState() {
            return new _PopupMenuDividerState<T>();
        }
    }

    class _PopupMenuDividerState<T> : State<PopupMenuDivider<T>> {
        public override Widget build(BuildContext context) {
            return new Divider(height: widget.height);
        }
    }

    class _MenuItem : SingleChildRenderObjectWidget {
        internal _MenuItem(
            Key key = null,
            ValueChanged<Size> onLayout = null,
            Widget child = null
        ) : base(key: key, child: child) {
            this.onLayout = onLayout;
        }

        public readonly ValueChanged<Size> onLayout;

        public override RenderObject createRenderObject(BuildContext context) {
            return new _RenderMenuItemDuplicated(onLayout);
        }

        public override void updateRenderObject(BuildContext context, RenderObject renderObject) {
            if (renderObject is _RenderMenuItemDuplicated renderMenuItemDuplicated) {
                renderMenuItemDuplicated.onLayout = onLayout;
            }
        }
    }

    class _RenderMenuItemDuplicated : RenderShiftedBox {
        internal _RenderMenuItemDuplicated(
            ValueChanged<Size> onLayout,
            RenderBox child = null
        ) : base(child) {
            D.assert(onLayout != null);
            this.onLayout = onLayout;
        }

        public ValueChanged<Size> onLayout;

        protected override void performLayout() {
            if (child == null) {
                size = Size.zero;
            }
            else {
                child.layout(constraints, parentUsesSize: true);
                size = constraints.constrain(child.size);
            }

            BoxParentData childParentData = child.parentData as BoxParentData;
            childParentData.offset = Offset.zero;
            onLayout(size);
        }
    }

    public class PopupMenuItem<T> : PopupMenuEntry<T> {
        public PopupMenuItem(
            Key key = null,
            T value = default,
            bool enabled = true,
            float height = material_.kMinInteractiveDimension,
            TextStyle textStyle = null,
            Widget child = null
        ) : base(key: key) {
            this.value = value;
            this.enabled = enabled;
            _height = height;
            this.textStyle = textStyle;
            this.child = child;
        }

        public readonly T value;

        public readonly bool enabled;

        readonly float _height;

        public readonly TextStyle textStyle;

        public override float height {
            get { return _height; }
        }

        public readonly Widget child;

        public override bool represents(T value) {
            return Equals(value, this.value);
        }

        public override State createState() {
            return new PopupMenuItemState<T, PopupMenuItem<T>>();
        }
    }

    public class PopupMenuItemState<T, W> : State<W> where W : PopupMenuItem<T> {
        protected virtual Widget buildChild() {
            return widget.child;
        }

        protected virtual void handleTap() {
            Navigator.pop(context, widget.value);
        }

        public override Widget build(BuildContext context) {
            ThemeData theme = Theme.of(context);
            PopupMenuThemeData popupMenuTheme = PopupMenuTheme.of(context);
            TextStyle style = widget.textStyle ?? popupMenuTheme.textStyle ?? theme.textTheme.subtitle1;
            if (!widget.enabled) {
                style = style.copyWith(color: theme.disabledColor);
            }

            Widget item = new AnimatedDefaultTextStyle(
                style: style,
                duration: material_.kThemeChangeDuration,
                child: new Container(
                    alignment: AlignmentDirectional.centerStart,
                    constraints: new BoxConstraints(minHeight: widget.height),
                    padding: EdgeInsets.symmetric(horizontal: material_._kMenuHorizontalPadding),
                    child: buildChild()
                )
            );

            if (!widget.enabled) {
                bool isDark = theme.brightness == Brightness.dark;
                item = IconTheme.merge(
                    data: new IconThemeData(opacity: isDark ? 0.5f : 0.38f),
                    child: item
                );
            }

            return new InkWell(
                onTap: widget.enabled ? handleTap : (GestureTapCallback) null,
                canRequestFocus: widget.enabled,
                child: item
            );
        }
    }

    public class PopupMenuItemSingleTickerProviderState<T, W> : SingleTickerProviderStateMixin<W>
        where W : PopupMenuItem<T> {
        protected virtual Widget buildChild() {
            return widget.child;
        }

        protected virtual void handleTap() {
            Navigator.pop(context, widget.value);
        }

        public override Widget build(BuildContext context) {
            ThemeData theme = Theme.of(context);
            PopupMenuThemeData popupMenuTheme = PopupMenuTheme.of(context);
            TextStyle style = widget.textStyle ?? popupMenuTheme.textStyle ?? theme.textTheme.subtitle1;
            if (!widget.enabled) {
                style = style.copyWith(color: theme.disabledColor);
            }

            Widget item = new AnimatedDefaultTextStyle(
                style: style,
                duration: material_.kThemeChangeDuration,
                child: new Container(
                    alignment: AlignmentDirectional.centerStart,
                    constraints: new BoxConstraints(minHeight: widget.height),
                    padding: EdgeInsets.symmetric(horizontal: material_._kMenuHorizontalPadding),
                    child: buildChild()
                )
            );

            if (!widget.enabled) {
                bool isDark = theme.brightness == Brightness.dark;
                item = IconTheme.merge(
                    data: new IconThemeData(opacity: isDark ? 0.5f : 0.38f),
                    child: item
                );
            }

            return new InkWell(
                onTap: widget.enabled ? handleTap : (GestureTapCallback) null,
                canRequestFocus: widget.enabled,
                child: item
            );
        }
    }

    public class CheckedPopupMenuItem<T> : PopupMenuItem<T> {
        public CheckedPopupMenuItem(
            Key key = null,
            T value = default,
            bool isChecked = false,
            bool enabled = true,
            Widget child = null
        ) : base(
            key: key,
            value: value,
            enabled: enabled,
            child: child
        ) {
            this.isChecked = isChecked;
        }

        public readonly bool isChecked;

        public override State createState() {
            return new _CheckedPopupMenuItemState<T>();
        }
    }

    class _CheckedPopupMenuItemState<T> : PopupMenuItemSingleTickerProviderState<T, CheckedPopupMenuItem<T>> {
        static readonly TimeSpan _fadeDuration = new TimeSpan(0, 0, 0, 0, 150);

        AnimationController _controller;

        Animation<float> _opacity {
            get { return _controller.view; }
        }

        public override void initState() {
            base.initState();
            _controller = new AnimationController(duration: _fadeDuration, vsync: this);
            _controller.setValue(widget.isChecked ? 1.0f : 0.0f);
            _controller.addListener(() => setState(() => {
                /* animation changed */
            }));
        }

        protected override void handleTap() {
            if (widget.isChecked) {
                _controller.reverse();
            }
            else {
                _controller.forward();
            }

            base.handleTap();
        }

        protected override Widget buildChild() {
            return new ListTile(
                enabled: widget.enabled,
                leading: new FadeTransition(
                    opacity: _opacity,
                    child: new Icon(_controller.isDismissed ? null : Icons.done)
                ),
                title: widget.child
            );
        }
    }

    class _PopupMenu<T> : StatelessWidget {
        public _PopupMenu(
            Key key = null,
            _PopupMenuRoute<T> route = null
        ) : base(key: key) {
            this.route = route;
        }

        public readonly _PopupMenuRoute<T> route;

        public override Widget build(BuildContext context) {
            float unit = 1.0f / (route.items.Count + 1.5f);
            List<Widget> children = new List<Widget>();
            PopupMenuThemeData popupMenuTheme = PopupMenuTheme.of(context);

            for (int i = 0; i < route.items.Count; i += 1) {
                int index = i;
                float start = (index + 1) * unit;
                float end = (start + 1.5f * unit).clamp(0.0f, 1.0f);
                CurvedAnimation opacityCurvedAnimation = new CurvedAnimation(
                    parent: route.animation,
                    curve: new Interval(start, end)
                );
                Widget item = route.items[index];
                if (route.initialValue != null && route.items[index].represents((T) route.initialValue)) {
                    item = new Container(
                        color: Theme.of(context).highlightColor,
                        child: item
                    );
                }

                children.Add(
                    new _MenuItem(
                        onLayout: (Size size) => { route.itemSizes[index] = size; },
                        child: new FadeTransition(
                            opacity: opacityCurvedAnimation,
                            child: item
                        )
                    )
                );
            }

            CurveTween opacity = new CurveTween(curve: new Interval(0.0f, 1.0f / 3.0f));
            CurveTween width = new CurveTween(curve: new Interval(0.0f, unit));
            CurveTween height = new CurveTween(curve: new Interval(0.0f, unit * route.items.Count));

            Widget child = new ConstrainedBox(
                constraints: new BoxConstraints(
                    minWidth: material_._kMenuMinWidth,
                    maxWidth: material_._kMenuMaxWidth
                ),
                child: new IntrinsicWidth(
                    stepWidth: material_._kMenuWidthStep,
                    child: new SingleChildScrollView(
                        padding: EdgeInsets.symmetric(
                            vertical: material_._kMenuVerticalPadding
                        ),
                        child: new ListBody(children: children)
                    )
                )
            );

            return new AnimatedBuilder(
                animation: route.animation,
                builder: (_, builderChild) => {
                    return new Opacity(
                        opacity: opacity.evaluate(route.animation),
                        child: new Material(
                            shape: route.shape ?? popupMenuTheme.shape,
                            color: route.color ?? popupMenuTheme.color,
                            type: MaterialType.card,
                            elevation: route.elevation ?? popupMenuTheme.elevation ?? 8.0f,
                            child: new Align(
                                alignment: Alignment.topRight,
                                widthFactor: width.evaluate(route.animation),
                                heightFactor: height.evaluate(route.animation),
                                child: builderChild
                            )
                        )
                    );
                },
                child: child
            );
        }
    }

    class _PopupMenuRouteLayout : SingleChildLayoutDelegate {
        public _PopupMenuRouteLayout(RelativeRect position, List<Size> itemSizes, int selectedItemIndex,
            TextDirection? textDirection) {
            this.position = position;
            this.itemSizes = itemSizes;
            this.selectedItemIndex = selectedItemIndex;
            this.textDirection = textDirection;
        }

        public readonly RelativeRect position;

        public List<Size> itemSizes;

        public readonly int selectedItemIndex;

        public readonly TextDirection? textDirection;

        public override BoxConstraints getConstraintsForChild(BoxConstraints constraints) {
            return BoxConstraints.loose(
                constraints.biggest - new Offset(material_._kMenuScreenPadding * 2.0f,
                    material_._kMenuScreenPadding * 2.0f)
            );
        }

        public override Offset getPositionForChild(Size size, Size childSize) {
            float y = position.top;
            if (itemSizes != null) {
                float selectedItemOffset = material_._kMenuVerticalPadding;
                for (int index = 0; index < selectedItemIndex; index += 1)
                    selectedItemOffset += itemSizes[index].height;
                selectedItemOffset += itemSizes[selectedItemIndex].height / 2;
                y = position.top + (size.height - position.top - position.bottom) / 2.0f - selectedItemOffset;
            }

            float x = 0;
            if (position.left > position.right) {
                x = size.width - position.right - childSize.width;
            }
            else if (position.left < position.right) {
                x = position.left;
            }
            else {
                D.assert(textDirection != null);
                switch (textDirection) {
                    case TextDirection.rtl:
                        x = size.width - position.right - childSize.width;
                        break;
                    case TextDirection.ltr:
                        x = position.left;
                        break;
                }
            }

            if (x < material_._kMenuScreenPadding) {
                x = material_._kMenuScreenPadding;
            }
            else if (x + childSize.width > size.width - material_._kMenuScreenPadding) {
                x = size.width - childSize.width - material_._kMenuScreenPadding;
            }

            if (y < material_._kMenuScreenPadding) {
                y = material_._kMenuScreenPadding;
            }
            else if (y + childSize.height > size.height - material_._kMenuScreenPadding) {
                y = size.height - childSize.height - material_._kMenuScreenPadding;
            }

            return new Offset(x, y);
        }

        public override bool shouldRelayout(SingleChildLayoutDelegate oldDelegate) {
            if (oldDelegate is _PopupMenuRouteLayout popupMenu) {
                D.assert(itemSizes.Count == popupMenu.itemSizes.Count);

                return position != popupMenu.position
                       || selectedItemIndex != popupMenu.selectedItemIndex
                       || textDirection != popupMenu.textDirection
                       || !itemSizes.equalsList(popupMenu.itemSizes);
            }
            else {
                return false;
            }
        }
    }

    class _PopupMenuRoute<T> : PopupRoute {
        public _PopupMenuRoute(
            RelativeRect position = null,
            List<PopupMenuEntry<T>> items = null,
            T initialValue = default,
            float? elevation = 8.0f,
            ThemeData theme = null,
            PopupMenuThemeData popupMenuTheme = null,
            string barrierLabel = null,
            ShapeBorder shape = null,
            Color color = null,
            BuildContext showMenuContext = null,
            bool? captureInheritedThemes = null
        ) {
            this.position = position;
            this.items = items;
            this.initialValue = initialValue;
            this.elevation = elevation;
            this.theme = theme;
            this.popupMenuTheme = popupMenuTheme;
            this.barrierLabel = barrierLabel;
            this.shape = shape;
            this.color = color;
            this.showMenuContext = showMenuContext;
            this.captureInheritedThemes = captureInheritedThemes;
            itemSizes = new List<Size>(new Size[items.Count]);
        }

        public readonly RelativeRect position;
        public readonly List<PopupMenuEntry<T>> items;
        public readonly List<Size> itemSizes;
        public readonly T initialValue;
        public readonly float? elevation;
        public readonly ThemeData theme;
        public readonly ShapeBorder shape;
        public readonly Color color;
        public readonly PopupMenuThemeData popupMenuTheme;
        public readonly BuildContext showMenuContext;
        public readonly bool? captureInheritedThemes;

        public override Animation<float> createAnimation() {
            return new CurvedAnimation(
                parent: base.createAnimation(),
                curve: Curves.linear,
                reverseCurve: new Interval(0.0f, material_._kMenuCloseIntervalEnd)
            );
        }

        public override TimeSpan transitionDuration {
            get { return material_._kMenuDuration; }
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
            int? selectedItemIndex = null;
            if (initialValue != null) {
                for (int index = 0; selectedItemIndex == null && index < items.Count; index += 1) {
                    if (items[index].represents(initialValue))
                        selectedItemIndex = index;
                }
            }

            Widget menu = new _PopupMenu<T>(route: this);
            if (captureInheritedThemes ?? false) {
                menu = InheritedTheme.captureAll(showMenuContext, menu);
            }
            else {
                if (theme != null)
                    menu = new Theme(data: theme, child: menu);
            }

            return MediaQuery.removePadding(
                context: context,
                removeTop: true,
                removeBottom: true,
                removeLeft: true,
                removeRight: true,
                child: new Builder(
                    builder: _ => new CustomSingleChildLayout(
                        layoutDelegate: new _PopupMenuRouteLayout(
                            position,
                            itemSizes,
                            selectedItemIndex ?? 0,
                            Directionality.of(context)
                        ),
                        child: menu
                    ))
            );
        }
    }

    public partial class material_ {
        public static Future<T> showMenu<T>(
            BuildContext context,
            RelativeRect position,
            List<PopupMenuEntry<T>> items,
            T initialValue,
            float? elevation,
            ShapeBorder shape,
            Color color,
            bool captureInheritedThemes = true,
            bool useRootNavigator = false
        ) {
            D.assert(context != null);
            D.assert(position != null);
            D.assert(items != null && items.isNotEmpty());
            D.assert(material_.debugCheckHasMaterialLocalizations(context));

            return Navigator.of(context, rootNavigator: useRootNavigator).push(new _PopupMenuRoute<T>(
                position: position,
                items: items,
                initialValue: initialValue,
                elevation: elevation,
                theme: Theme.of(context, shadowThemeOnly: true),
                popupMenuTheme: PopupMenuTheme.of(context),
                barrierLabel: MaterialLocalizations.of(context).modalBarrierDismissLabel,
                shape: shape,
                color: color,
                showMenuContext: context,
                captureInheritedThemes: captureInheritedThemes
            )).to<T>();
        }
    }

    public delegate void PopupMenuItemSelected<T>(T value);

    public delegate void PopupMenuCanceled();

    public delegate List<PopupMenuEntry<T>> PopupMenuItemBuilder<T>(BuildContext context);

    public class PopupMenuButton<T> : StatefulWidget {
        public PopupMenuButton(
            Key key = null,
            PopupMenuItemBuilder<T> itemBuilder = null,
            T initialValue = default,
            PopupMenuItemSelected<T> onSelected = null,
            PopupMenuCanceled onCanceled = null,
            string tooltip = null,
            float? elevation = null,
            EdgeInsetsGeometry padding = null,
            Widget child = null,
            Icon icon = null,
            Offset offset = null,
            bool enabled = true,
            ShapeBorder shape = null,
            Color color = null,
            bool captureInheritedThemes = true
        ) : base(key: key) {
            offset = offset ?? Offset.zero;
            D.assert(itemBuilder != null);
            D.assert(offset != null);
            D.assert(!(child != null && icon != null), () => "You can only pass [child] or [icon], not both.");

            this.itemBuilder = itemBuilder;
            this.initialValue = initialValue;
            this.onSelected = onSelected;
            this.onCanceled = onCanceled;
            this.tooltip = tooltip;
            this.elevation = elevation;
            this.padding = padding ?? EdgeInsets.all(8.0f);
            this.child = child;
            this.icon = icon;
            this.offset = offset;
            this.enabled = enabled;
            this.shape = shape;
            this.color = color;
            this.captureInheritedThemes = captureInheritedThemes;
        }


        public readonly PopupMenuItemBuilder<T> itemBuilder;

        public readonly T initialValue;

        public readonly PopupMenuItemSelected<T> onSelected;

        public readonly PopupMenuCanceled onCanceled;

        public readonly string tooltip;

        public readonly float? elevation;

        public readonly EdgeInsetsGeometry padding;

        public readonly Widget child;

        public readonly Widget icon;

        public readonly Offset offset;

        public readonly bool enabled;

        public readonly ShapeBorder shape;

        public readonly Color color;

        public readonly bool captureInheritedThemes;

        public override State createState() {
            return new PopupMenuButtonState<T>();
        }
    }

    public class PopupMenuButtonState<T> : State<PopupMenuButton<T>> {
        void showButtonMenu() {
            PopupMenuThemeData popupMenuTheme = PopupMenuTheme.of(context);
            RenderBox button = (RenderBox) context.findRenderObject();
            RenderBox overlay = (RenderBox) Overlay.of(context).context.findRenderObject();
            RelativeRect position = RelativeRect.fromRect(
                Rect.fromPoints(
                    button.localToGlobal(widget.offset, ancestor: overlay),
                    button.localToGlobal(button.size.bottomRight(Offset.zero), ancestor: overlay)
                ),
                Offset.zero & overlay.size
            );
            List<PopupMenuEntry<T>> items = widget.itemBuilder(context);
            if (items.isNotEmpty()) {
                material_.showMenu<T>(
                        context: context,
                        elevation: widget.elevation ?? popupMenuTheme.elevation,
                        items: items,
                        initialValue: widget.initialValue,
                        position: position,
                        shape: widget.shape ?? popupMenuTheme.shape,
                        color: widget.color ?? popupMenuTheme.color,
                        captureInheritedThemes: widget.captureInheritedThemes
                    )
                    .then(newValue => {
                        if (!mounted)
                            return;
                        if (newValue == null) {
                            if (widget.onCanceled != null)
                                widget.onCanceled();
                            return;
                        }

                        if (widget.onSelected != null)
                            widget.onSelected((T) newValue);
                    });
            }
        }

        Icon _getIcon(RuntimePlatform platform) {
            switch (platform) {
                case RuntimePlatform.IPhonePlayer:
                case RuntimePlatform.OSXEditor:
                case RuntimePlatform.OSXPlayer:
                    return new Icon(Icons.more_horiz);
                default:
                    return new Icon(Icons.more_vert);
            }
        }

        public override Widget build(BuildContext context) {
            D.assert(material_.debugCheckHasMaterialLocalizations(context));
            if (widget.child != null)
                return new Tooltip(
                    message: widget.tooltip ?? MaterialLocalizations.of(context).showMenuTooltip,
                    child: new InkWell(
                        onTap: widget.enabled ? showButtonMenu : (GestureTapCallback) null,
                        canRequestFocus: widget.enabled,
                        child: widget.child
                    )
                );


            return new IconButton(
                icon: widget.icon ?? _getIcon(Theme.of(context).platform),
                padding: widget.padding,
                tooltip: widget.tooltip ?? MaterialLocalizations.of(context).showMenuTooltip,
                onPressed: widget.enabled ? showButtonMenu : (VoidCallback) null
            );
        }
    }
}