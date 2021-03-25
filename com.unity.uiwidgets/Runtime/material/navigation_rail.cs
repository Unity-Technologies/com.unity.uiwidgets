using System.Collections.Generic;
using System.Linq;
using Unity.UIWidgets.animation;
using Unity.UIWidgets.external;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.rendering;
using Unity.UIWidgets.ui;
using Unity.UIWidgets.widgets;
using UnityEngine;
using Color = Unity.UIWidgets.ui.Color;
using TextStyle = Unity.UIWidgets.painting.TextStyle;

namespace Unity.UIWidgets.material {
    class NavigationRail : StatefulWidget {
        public NavigationRail(
            Color backgroundColor = null,
            bool? extended = null,
            Widget leading = null,
            Widget trailing = null,
            List<NavigationRailDestination> destinations = null,
            int? selectedIndex = null,
            ValueChanged<int> onDestinationSelected = null,
            float? elevation = null,
            float? groupAlignment = null,
            NavigationRailLabelType? labelType = null,
            TextStyle unselectedLabelTextStyle = null,
            TextStyle selectedLabelTextStyle = null,
            IconThemeData unselectedIconTheme = null,
            IconThemeData selectedIconTheme = null,
            float? minWidth = null,
            float? minExtendedWidth = null
        ) {
            D.assert(destinations != null && destinations.Count >= 2);
            D.assert(selectedIndex != null);
            D.assert(0 <= selectedIndex && selectedIndex < destinations.Count);
            D.assert(elevation == null || elevation > 0);
            D.assert(minWidth == null || minWidth > 0);
            D.assert(minExtendedWidth == null || minExtendedWidth > 0);
            D.assert((minWidth == null || minExtendedWidth == null) || minExtendedWidth >= minWidth);
            D.assert(extended != null);
            D.assert(!extended.Value || (labelType == null || labelType == NavigationRailLabelType.none));
            this.backgroundColor = backgroundColor;
            this.extended = extended;
            this.leading = leading;
            this.trailing = trailing;
            this.destinations = destinations;
            this.selectedIndex = selectedIndex;
            this.onDestinationSelected = onDestinationSelected;
            this.elevation = elevation;
            this.groupAlignment = groupAlignment;
            this.labelType = labelType;
            this.unselectedLabelTextStyle = unselectedLabelTextStyle;
            this.selectedLabelTextStyle = selectedLabelTextStyle;
            this.unselectedIconTheme = unselectedIconTheme;
            this.selectedIconTheme = selectedIconTheme;
            this.minWidth = minWidth;
            this.minExtendedWidth = minExtendedWidth;
        }

        public readonly Color backgroundColor;

        public readonly bool? extended;

        public readonly Widget leading;

        public readonly Widget trailing;

        public readonly List<NavigationRailDestination> destinations;

        public readonly int? selectedIndex;

        public readonly ValueChanged<int> onDestinationSelected;

        public readonly float? elevation;

        public readonly float? groupAlignment;

        public readonly NavigationRailLabelType? labelType;

        public readonly TextStyle unselectedLabelTextStyle;

        public readonly TextStyle selectedLabelTextStyle;

        public readonly IconThemeData unselectedIconTheme;

        public readonly IconThemeData selectedIconTheme;

        public readonly float? minWidth;

        public readonly float? minExtendedWidth;

        public static Animation<float> extendedAnimation(BuildContext context) {
            return context.dependOnInheritedWidgetOfExactType<_ExtendedNavigationRailAnimation>().animation;
        }

        public override State createState() => new _NavigationRailState();
    }

    class _NavigationRailState : TickerProviderStateMixin<NavigationRail> {
        List<AnimationController> _destinationControllers = new List<AnimationController>();
        List<Animation<float>> _destinationAnimations;
        AnimationController _extendedController;
        Animation<float> _extendedAnimation;

        public override void initState() {
            base.initState();
            _initControllers();
        }

        public override void dispose() {
            _disposeControllers();
            base.dispose();
        }

        public override void didUpdateWidget(StatefulWidget oldWidget) {
            base.didUpdateWidget(oldWidget);
            var checkOldWidget = (NavigationRail) oldWidget;
            if (oldWidget is NavigationRail navigationRail) {
                if (widget.extended != navigationRail.extended) {
                    if (widget.extended ?? false) {
                        _extendedController.forward();
                    }
                    else {
                        _extendedController.reverse();
                    }
                }

                if (widget.destinations.Count != navigationRail.destinations.Count) {
                    _resetState();
                    return;
                }

                if (widget.selectedIndex != navigationRail.selectedIndex) {
                    _destinationControllers[navigationRail.selectedIndex.Value].reverse();
                    _destinationControllers[widget.selectedIndex.Value].forward();
                    return;
                }
            }
        }

        public override Widget build(BuildContext context) {
            ThemeData theme = Theme.of(context);
            NavigationRailThemeData navigationRailTheme = NavigationRailTheme.of(context);
            MaterialLocalizations localizations = MaterialLocalizations.of(context);

            Color backgroundColor =
                widget.backgroundColor ?? navigationRailTheme.backgroundColor ?? theme.colorScheme.surface;
            float elevation = widget.elevation ?? navigationRailTheme.elevation ?? 0;
            float minWidth = widget.minWidth ?? material_._minRailWidth;
            float minExtendedWidth = widget.minExtendedWidth ?? material_._minExtendedRailWidth;
            Color baseSelectedColor = theme.colorScheme.primary;
            Color baseColor = theme.colorScheme.onSurface.withOpacity(0.64f);
            IconThemeData defaultUnselectedIconTheme =
                widget.unselectedIconTheme ?? navigationRailTheme.unselectedIconTheme;
            IconThemeData unselectedIconTheme = new IconThemeData(
                size: defaultUnselectedIconTheme?.size ?? 24.0f,
                color: defaultUnselectedIconTheme?.color ?? theme.colorScheme.onSurface,
                opacity: defaultUnselectedIconTheme?.opacity ?? 1.0f
            );
            IconThemeData defaultSelectedIconTheme = widget.selectedIconTheme ?? navigationRailTheme.selectedIconTheme;
            IconThemeData selectedIconTheme = new IconThemeData(
                size: defaultSelectedIconTheme?.size ?? 24.0f,
                color: defaultSelectedIconTheme?.color ?? theme.colorScheme.primary,
                opacity: defaultSelectedIconTheme?.opacity ?? 0.64f
            );
            TextStyle unselectedLabelTextStyle = theme.textTheme.bodyText1.copyWith(color: baseColor)
                .merge(widget.unselectedLabelTextStyle ?? navigationRailTheme.unselectedLabelTextStyle);
            TextStyle selectedLabelTextStyle = theme.textTheme.bodyText1.copyWith(color: baseSelectedColor)
                .merge(widget.selectedLabelTextStyle ?? navigationRailTheme.selectedLabelTextStyle);
            float groupAlignment = widget.groupAlignment ?? navigationRailTheme.groupAlignment ?? -1.0f;
            NavigationRailLabelType labelType =
                widget.labelType ?? navigationRailTheme.labelType ?? NavigationRailLabelType.none;

            var materialChildren = new List<Widget>();
            materialChildren.Add(material_._verticalSpacer);

            if (widget.leading != null) {
                materialChildren.AddRange(new List<Widget>() {
                    new ConstrainedBox(
                        constraints: new BoxConstraints(
                            minWidth: MathUtils.lerpNullableFloat(minWidth, minExtendedWidth, _extendedAnimation.value)
                        ),
                        child: widget.leading
                    ),
                    material_._verticalSpacer,
                });
            }

            var alignChildren = new List<Widget>();
            for (int i = 0; i < widget.destinations.Count; i += 1) {
                alignChildren.Add(new _RailDestination(
                    minWidth: minWidth,
                    minExtendedWidth: minExtendedWidth,
                    extendedTransitionAnimation: _extendedAnimation,
                    selected: widget.selectedIndex == i,
                    icon: widget.selectedIndex == i ? widget.destinations[i].selectedIcon : widget.destinations[i].icon,
                    label: widget.destinations[i].label,
                    destinationAnimation: _destinationAnimations[i],
                    labelType: labelType,
                    iconTheme: widget.selectedIndex == i ? selectedIconTheme : unselectedIconTheme,
                    labelTextStyle: widget.selectedIndex == i ? selectedLabelTextStyle : unselectedLabelTextStyle,
                    onTap: () => { widget.onDestinationSelected(i); },
                    indexLabel: localizations.tabLabel(
                        tabIndex: i + 1,
                        tabCount: widget.destinations.Count
                    )
                ));
            }

            if (widget.trailing != null) {
                alignChildren.Add(new ConstrainedBox(
                    constraints: new BoxConstraints(
                        minWidth: MathUtils.lerpNullableFloat(minWidth, minExtendedWidth, _extendedAnimation.value)
                    ),
                    child: widget.trailing
                ));
            }

            materialChildren.Add(new Expanded(
                child: new Align(
                    alignment: new Alignment(0, groupAlignment),
                    child: new Column(
                        mainAxisSize: MainAxisSize.min,
                        children: alignChildren
                    )
                )
            ));

            return new _ExtendedNavigationRailAnimation(
                animation: _extendedAnimation,
                child: new Material(
                    elevation: elevation,
                    color: backgroundColor,
                    child: new Column(
                        children: materialChildren
                    )
                )
            );
        }

        void _disposeControllers() {
            foreach (AnimationController controller in _destinationControllers) {
                controller.dispose();
            }

            _extendedController.dispose();
        }

        void _initControllers() {
            _destinationControllers = LinqUtils<AnimationController, NavigationRailDestination>.SelectList(widget.destinations, ((destination) => {
                    var result = new AnimationController(
                        duration: ThemeUtils.kThemeAnimationDuration,
                        vsync: this
                    );
                    result.addListener(_rebuild);
                    return result;
                }));
            _destinationAnimations = LinqUtils<Animation<float>, AnimationController>.SelectList(_destinationControllers,((AnimationController controller) => controller.view));
            _destinationControllers[widget.selectedIndex ?? 0].setValue(1.0f);
            _extendedController = new AnimationController(
                duration: ThemeUtils.kThemeAnimationDuration,
                vsync: this,
                value: widget.extended ?? false ? 1.0f : 0.0f
            );
            _extendedAnimation = new CurvedAnimation(
                parent: _extendedController,
                curve: Curves.easeInOut
            );
            _extendedController.addListener(() => { _rebuild(); });
        }

        void _resetState() {
            _disposeControllers();
            _initControllers();
        }

        void _rebuild() {
            setState(() => {
                // Rebuilding when any of the controllers tick, i.e. when the items are
                // animating.
            });
        }
    }

    internal class _RailDestination : StatelessWidget {
        internal _RailDestination(
            float? minWidth = null,
            float? minExtendedWidth = null,
            Widget icon = null,
            Widget label = null,
            Animation<float> destinationAnimation = null,
            Animation<float> extendedTransitionAnimation = null,
            NavigationRailLabelType? labelType = null,
            bool? selected = null,
            IconThemeData iconTheme = null,
            TextStyle labelTextStyle = null,
            VoidCallback onTap = null,
            string indexLabel = null
        ) {
            D.assert(minWidth != null);
            D.assert(minExtendedWidth != null);
            D.assert(icon != null);
            D.assert(label != null);
            D.assert(destinationAnimation != null);
            D.assert(extendedTransitionAnimation != null);
            D.assert(labelType != null);
            D.assert(selected != null);
            D.assert(iconTheme != null);
            D.assert(labelTextStyle != null);
            D.assert(onTap != null);
            D.assert(indexLabel != null);
            this.minWidth = minWidth;

            this.minExtendedWidth = minExtendedWidth;

            this.icon = icon;

            this.label = label;

            this.destinationAnimation = destinationAnimation;

            this.extendedTransitionAnimation = extendedTransitionAnimation;

            this.labelType = labelType;

            this.selected = selected;

            this.iconTheme = iconTheme;

            this.labelTextStyle = labelTextStyle;

            this.onTap = onTap;

            this.indexLabel = indexLabel;

            _positionAnimation = new CurvedAnimation(
                parent: new ReverseAnimation(destinationAnimation),
                curve: Curves.easeInOut,
                reverseCurve: Curves.easeInOut.flipped
            );
        }

        public readonly float? minWidth;
        public readonly float? minExtendedWidth;
        public readonly Widget icon;
        public readonly Widget label;
        public readonly Animation<float> destinationAnimation;
        public readonly NavigationRailLabelType? labelType;
        public readonly bool? selected;
        public readonly Animation<float> extendedTransitionAnimation;
        public readonly IconThemeData iconTheme;
        public readonly TextStyle labelTextStyle;
        public readonly VoidCallback onTap;
        public readonly string indexLabel;

        public readonly Animation<float> _positionAnimation;

        public override Widget build(BuildContext context) {
            Widget themedIcon = new IconTheme(
                data: iconTheme,
                child: icon
            );

            Widget styledLabel = new DefaultTextStyle(
                style: labelTextStyle,
                child: label
            );
            Widget content = null;
            switch (labelType) {
                case NavigationRailLabelType.none:

                    Widget iconPart = new SizedBox(
                        width: minWidth,
                        height: minWidth,
                        child: new Align(
                            alignment: Alignment.center,
                            child: themedIcon
                        )
                    );
                    if (extendedTransitionAnimation.value == 0) {
                        content = new Stack(
                                children: new List<Widget>() {
                                    iconPart,
                                    new SizedBox(
                                        width: 0,
                                        height: 0,
                                        child: new Opacity(
                                            opacity: 0.0f,
                                            child: label
                                        )
                                    )
                                }
                            )
                            ;
                    }
                    else {
                        content = new ConstrainedBox(
                            constraints: new BoxConstraints(
                                minWidth: MathUtils.lerpNullableFloat(minWidth, minExtendedWidth,
                                    extendedTransitionAnimation.value) ?? 0
                            ),
                            child: new ClipRect(
                                child: new Row(
                                    children: new List<Widget> {
                                        iconPart,
                                        new Align(
                                            heightFactor: 1.0f,
                                            widthFactor: extendedTransitionAnimation.value,
                                            alignment: AlignmentDirectional.centerStart,
                                            child: new Opacity(
                                                opacity: _extendedLabelFadeValue(),
                                                child: styledLabel
                                            )
                                        ),
                                        new SizedBox(width: material_._horizontalDestinationPadding),
                                    }
                                )
                            )
                        );
                    }

                    break;
                case NavigationRailLabelType.selected:

                    float appearingAnimationValue = 1 - _positionAnimation.value;

                    float verticalPadding = MathUtils.lerpNullableFloat(material_._verticalDestinationPaddingNoLabel,
                        material_._verticalDestinationPaddingWithLabel, appearingAnimationValue);
                    content = new Container(
                        constraints: new BoxConstraints(
                            minWidth: minWidth ?? 0,
                            minHeight: minWidth ?? 0
                        ),
                        padding: EdgeInsets.symmetric(horizontal: material_._horizontalDestinationPadding),
                        child:
                        new ClipRect(
                            child: new Column(
                                mainAxisSize: MainAxisSize.min,
                                mainAxisAlignment: MainAxisAlignment.center,
                                children: new List<Widget>() {
                                    new SizedBox(height: verticalPadding),
                                    themedIcon,
                                    new Align(
                                        alignment: Alignment.topCenter,
                                        heightFactor: appearingAnimationValue,
                                        widthFactor: 1.0f,
                                        child: new Opacity(
                                            opacity: selected ?? false
                                                ? _normalLabelFadeInValue()
                                                : _normalLabelFadeOutValue(),
                                            child: styledLabel
                                        )
                                    ),
                                    new SizedBox(height: verticalPadding)
                                }
                            )
                        )
                    );
                    break;
                case NavigationRailLabelType.all:
                    content = new Container(
                        constraints: new BoxConstraints(
                            minWidth: minWidth ?? 0,
                            minHeight: minWidth ?? 0
                        ),
                        padding: EdgeInsets.symmetric(horizontal: material_._horizontalDestinationPadding),
                        child:
                        new Column(
                            children: new List<Widget>() {
                                new SizedBox(height: material_._verticalDestinationPaddingWithLabel),
                                themedIcon,
                                styledLabel,
                                new SizedBox(height: material_._verticalDestinationPaddingWithLabel),
                            }
                        ));
                    break;
            }

            ColorScheme colors = Theme.of(context).colorScheme;
            return new Material(
                type: MaterialType.transparency,
                clipBehavior: Clip.none,
                child: new InkResponse(
                    onTap: () => onTap(),
                    onHover: (_) => { },
                    highlightShape:
                    BoxShape.rectangle,
                    borderRadius:
                    BorderRadius.all(Radius.circular((minWidth ?? 0) / 2.0f)),
                    containedInkWell:
                    true,
                    splashColor: colors.primary.withOpacity(0.12f),
                    hoverColor: colors.primary.withOpacity(0.04f),
                    child: content
                )
            );
        }

        float _normalLabelFadeInValue() {
            if (destinationAnimation.value < 0.25f) {
                return 0;
            }
            else if (destinationAnimation.value < 0.75f) {
                return (destinationAnimation.value - 0.25f) * 2;
            }
            else {
                return 1;
            }
        }

        float _normalLabelFadeOutValue() {
            if (destinationAnimation.value > 0.75f) {
                return (destinationAnimation.value - 0.75f) * 4.0f;
            }
            else {
                return 0;
            }
        }

        float _extendedLabelFadeValue() {
            return extendedTransitionAnimation.value < 0.25f ? extendedTransitionAnimation.value * 4.0f : 1.0f;
        }
    }

    public enum NavigationRailLabelType {
        none,

        selected,

        all,
    }

    class NavigationRailDestination {
        public NavigationRailDestination(
            Widget icon,
            Widget selectedIcon = null,
            Widget label = null
        ) {
            D.assert(icon != null);
            selectedIcon = selectedIcon ?? icon;
            this.icon = icon;
            this.selectedIcon = selectedIcon;
            this.label = label;
        }

        public readonly Widget icon;

        public readonly Widget selectedIcon;

        public readonly Widget label;
    }

    class _ExtendedNavigationRailAnimation : InheritedWidget {
        public _ExtendedNavigationRailAnimation(
            Key key = null,
            Animation<float> animation = null,
            Widget child = null
        ) : base(key: key, child: child) {
            D.assert(child != null);
            this.animation = animation;
        }

        public readonly Animation<float> animation;

        public override bool updateShouldNotify(InheritedWidget oldWidget) =>
            oldWidget is _ExtendedNavigationRailAnimation extendedNavigationRailAnimation
            && animation != extendedNavigationRailAnimation.animation;
    }


    public partial class material_ {
        public static readonly float _minRailWidth = 72.0f;
        public static readonly float _minExtendedRailWidth = 256.0f;
        public static readonly float _horizontalDestinationPadding = 8.0f;
        public static readonly float _verticalDestinationPaddingNoLabel = 24.0f;
        public static readonly float _verticalDestinationPaddingWithLabel = 16.0f;
        public static readonly Widget _verticalSpacer = new SizedBox(height: 8.0f);
    }
}