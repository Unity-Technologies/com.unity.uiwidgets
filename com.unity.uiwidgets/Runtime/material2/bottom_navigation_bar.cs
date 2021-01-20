using System;
using System.Collections.Generic;
using System.Linq;
using Unity.UIWidgets.animation;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.gestures;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.rendering;
using Unity.UIWidgets.scheduler2;
using Unity.UIWidgets.service;
using Unity.UIWidgets.ui;
using Unity.UIWidgets.widgets;
using UnityEngine;
using Canvas = Unity.UIWidgets.ui.Canvas;
using Color = Unity.UIWidgets.ui.Color;
using Rect = Unity.UIWidgets.ui.Rect;
using TextStyle = Unity.UIWidgets.painting.TextStyle;
using Transform = Unity.UIWidgets.widgets.Transform;
using Brightness = Unity.UIWidgets.ui.Brightness;

namespace Unity.UIWidgets.material {
    public enum BottomNavigationBarType {
        fix,
        shifting
    }

    public class BottomNavigationBar : StatefulWidget {
        public BottomNavigationBar(
            Key key = null,
            List<BottomNavigationBarItem> items = null,
            ValueChanged<int> onTap = null,
            int currentIndex = 0,
            float elevation = 8.0f,
            BottomNavigationBarType? type = null,
            Color fixedColor = null,
            Color backgroundColor = null,
            float iconSize = 24.0f,
            Color selectedItemColor = null,
            Color unselectedItemColor = null,
            IconThemeData selectedIconTheme = null,
            IconThemeData unselectedIconTheme = null,
            float selectedFontSize = 14.0f,
            float unselectedFontSize = 12.0f,
            TextStyle selectedLabelStyle = null,
            TextStyle unselectedLabelStyle = null,
            bool showSelectedLabels = true,
            bool? showUnselectedLabels = null
        ) : base(key: key) {
            selectedIconTheme = selectedIconTheme ?? new IconThemeData();
            unselectedIconTheme = unselectedIconTheme ?? new IconThemeData();
            
            D.assert(items != null);
            D.assert(items.Count >= 2);
            D.assert(items.All((BottomNavigationBarItem item) => item.title != null) == true,
                () => "Every item must have a non-null title"
            );
            D.assert(0 <= currentIndex && currentIndex < items.Count);
            D.assert(elevation >= 0.0f);
            D.assert(iconSize >= 0.0f);
            D.assert(selectedItemColor == null || fixedColor == null,
                () => "Either selectedItemColor or fixedColor can be specified, but not both!");
            D.assert(selectedFontSize >= 0.0f);
            D.assert(unselectedFontSize >= 0.0f);
           
            type = _type(type, items);
            this.items = items;
            this.onTap = onTap;
            this.currentIndex = currentIndex;
            this.elevation = elevation;
            this.type = type ?? (items.Count <= 3 ? BottomNavigationBarType.fix : BottomNavigationBarType.shifting);
            this.backgroundColor = backgroundColor;
            this.iconSize = iconSize;
            this.selectedItemColor = selectedItemColor ?? fixedColor;
            this.unselectedItemColor = unselectedItemColor;
            this.selectedIconTheme = selectedIconTheme;
            this.unselectedIconTheme = unselectedIconTheme;
            this.selectedFontSize = selectedFontSize;
            this.unselectedFontSize = unselectedFontSize;
            this.selectedLabelStyle = selectedLabelStyle;
            this.unselectedLabelStyle = unselectedLabelStyle;
            this.showSelectedLabels = showSelectedLabels;
            this.showUnselectedLabels = showUnselectedLabels ?? _defaultShowUnselected(_type(type, items));
        }

        public readonly List<BottomNavigationBarItem> items;

        public readonly ValueChanged<int> onTap;

        public readonly int currentIndex;

        public readonly float elevation;

        public readonly BottomNavigationBarType? type;
        

        public Color fixedColor {
            get { return selectedItemColor; }
        }

        public readonly Color backgroundColor;

        public readonly float iconSize;


        public readonly Color selectedItemColor;

        public readonly Color unselectedItemColor;
        
        public readonly IconThemeData selectedIconTheme;
        
        public readonly IconThemeData unselectedIconTheme;

        public readonly float selectedFontSize;

        public readonly float unselectedFontSize;
        
        public readonly  TextStyle selectedLabelStyle;

        public readonly  TextStyle unselectedLabelStyle;

        public readonly bool showUnselectedLabels;

        public readonly bool showSelectedLabels;

        static BottomNavigationBarType _type(
            BottomNavigationBarType? type,
            List<BottomNavigationBarItem> items
        ) {
            if (type != null) {
                return type.Value;
            }

            return items.Count <= 3 ? BottomNavigationBarType.fix : BottomNavigationBarType.shifting;
        }

        static bool _defaultShowUnselected(BottomNavigationBarType type) {
            switch (type) {
                case BottomNavigationBarType.shifting:
                    return false;
                case BottomNavigationBarType.fix:
                    return true;
            }

            D.assert(false);
            return false;
        }

        public override State createState() {
            return new _BottomNavigationBarState();
        }
    }

    class _BottomNavigationTile : StatelessWidget {
        public _BottomNavigationTile(
            BottomNavigationBarType? type,
            BottomNavigationBarItem item,
            Animation<float> animation,
            float? iconSize = null,
            VoidCallback onTap = null,
            ColorTween colorTween = null,
            float? flex = null,
            bool selected = false,
            TextStyle selectedLabelStyle = null,
            TextStyle unselectedLabelStyle = null,
            IconThemeData selectedIconTheme = null,
            IconThemeData unselectedIconTheme = null,
            bool? showSelectedLabels = null,
            bool? showUnselectedLabels = null,
            string indexLabel = null
        ) {
            D.assert(type != null);
            D.assert(item != null);
            D.assert(animation != null);
            D.assert(selected != null);
            D.assert(selectedLabelStyle != null);
            D.assert(unselectedLabelStyle != null);
            this.type = type;
            this.item = item;
            this.animation = animation;
            this.iconSize = iconSize;
            this.onTap = onTap;
            this.colorTween = colorTween;
            this.flex = flex;
            this.selected = selected;
            //this.selectedFontSize = selectedFontSize.Value;
            //this.unselectedFontSize = unselectedFontSize.Value;
            this.selectedLabelStyle = selectedLabelStyle;
            this.unselectedLabelStyle = unselectedLabelStyle;
            this.selectedIconTheme = selectedIconTheme;
            this.unselectedIconTheme = unselectedIconTheme;
            this.showSelectedLabels = showSelectedLabels ?? false;
            this.showUnselectedLabels = showUnselectedLabels ?? false;
            this.indexLabel = indexLabel;
        }

        public readonly BottomNavigationBarType? type;
        public readonly BottomNavigationBarItem item;
        public readonly Animation<float> animation;
        public readonly float? iconSize;
        public readonly VoidCallback onTap;
        public readonly ColorTween colorTween;
        public readonly float? flex;
        public readonly bool selected;
        public readonly string indexLabel;
        
        public readonly  IconThemeData selectedIconTheme;
        public readonly  IconThemeData unselectedIconTheme;
        public readonly  TextStyle selectedLabelStyle;
        public readonly  TextStyle unselectedLabelStyle;
        public readonly  bool showSelectedLabels;
        public readonly  bool showUnselectedLabels;
        
        //public readonly float selectedFontSize;
        //public readonly float unselectedFontSize;
        //public readonly bool showSelectedLabels;
        //public readonly bool showUnselectedLabels;

        public override Widget build(BuildContext context) {
            int size = 0;
            float selectedFontSize = selectedLabelStyle.fontSize.Value;
            float selectedIconSize = (selectedIconTheme?.size ?? iconSize).Value;
            float unselectedIconSize = (unselectedIconTheme?.size ?? iconSize).Value;
            float selectedIconDiff = Mathf.Max(selectedIconSize - unselectedIconSize, 0);
            float unselectedIconDiff = Mathf.Max(unselectedIconSize - selectedIconSize, 0);
            float bottomPadding = 0.0f;
            float topPadding = 0.0f;
            if (showSelectedLabels && !showUnselectedLabels) {
              bottomPadding = new FloatTween(
                begin: selectedIconDiff / 2.0f,
                end: selectedFontSize / 2.0f - unselectedIconDiff / 2.0f
              ).evaluate(animation);
              topPadding = new FloatTween(
                begin: selectedFontSize + selectedIconDiff / 2.0f,
                end: selectedFontSize / 2.0f - unselectedIconDiff / 2.0f
              ).evaluate(animation);
            } else if (!showSelectedLabels && !showUnselectedLabels) {
              bottomPadding = new FloatTween(
                begin: selectedIconDiff / 2.0f,
                end: unselectedIconDiff / 2.0f
              ).evaluate(animation);
              topPadding = new FloatTween(
                begin: selectedFontSize + selectedIconDiff / 2.0f,
                end: selectedFontSize + unselectedIconDiff / 2.0f
              ).evaluate(animation);
            } else {
              bottomPadding = new FloatTween(
                begin: selectedFontSize / 2.0f + selectedIconDiff / 2.0f,
                end: selectedFontSize / 2.0f + unselectedIconDiff / 2.0f
              ).evaluate(animation);
              topPadding = new FloatTween(
                begin: selectedFontSize / 2.0f + selectedIconDiff / 2.0f,
                end: selectedFontSize / 2.0f + unselectedIconDiff / 2.0f
              ).evaluate(animation);
            }

            switch (type) {
              case BottomNavigationBarType.fix:
                size = 1;
                break;
              case BottomNavigationBarType.shifting:
                size = (flex.Value * 1000.0f).Round();
                break;
            }
            return new Expanded(
                flex: size,
                child: new Stack(
                    children: new List<Widget>{
                        new InkResponse(
                            onTap: onTap == null ? (GestureTapCallback) null : () => { onTap(); },
                            child: new Padding(
                                padding:EdgeInsets.only(top: topPadding, bottom: bottomPadding),
                                child:new Column(
                                    crossAxisAlignment: CrossAxisAlignment.center,
                                    mainAxisAlignment: MainAxisAlignment.spaceBetween,
                                    mainAxisSize: MainAxisSize.min,
                                    children: new List<Widget> {
                                        new _TileIcon(
                                            colorTween: colorTween,
                                            animation: animation,
                                            iconSize: iconSize,
                                            selected: selected,
                                            item: item,
                                            selectedIconTheme: selectedIconTheme,
                                            unselectedIconTheme: unselectedIconTheme
                                        ),
                                        new _Label(
                                            colorTween: colorTween,
                                            animation: animation,
                                            item: item,
                                            selectedLabelStyle: selectedLabelStyle,
                                            unselectedLabelStyle: unselectedLabelStyle,
                                            showSelectedLabels: showSelectedLabels,
                                            showUnselectedLabels: showUnselectedLabels
                                        )
                                    }
                                )
                            )
                        )
                        
                    }
                )
            );
                    
        }

        /*float bottomPadding = selectedFontSize / 2.0f;
        float topPadding = selectedFontSize / 2.0f;
        if (showSelectedLabels && !showUnselectedLabels) {
            bottomPadding = new FloatTween(
                begin: 0.0f,
                end: selectedFontSize / 2.0f
            ).evaluate(animation);
            topPadding = new FloatTween(
                begin: selectedFontSize,
                end: selectedFontSize / 2.0f
            ).evaluate(animation);
        }

        if (!showSelectedLabels && !showUnselectedLabels) {
            bottomPadding = 0.0f;
            topPadding = selectedFontSize;
        }
        switch (type) {
            case BottomNavigationBarType.fix:
                size = 1;
                break;
            case BottomNavigationBarType.shifting:
                size = ((flex * 1000.0f) ?? 0.0f).round();
                break;
            default:
                throw new Exception("Unknown BottomNavigationBarType: " + type);
        }

        return new Expanded(
            flex: size,
            child: new Stack(
                children: new List<Widget> {
                    new InkResponse(
                        onTap: onTap == null ? (GestureTapCallback) null : () => { onTap(); },
                        child: 
                        new Padding(
                            padding: EdgeInsets.only(top: topPadding, bottom: bottomPadding),
                            child: new Column(
                                crossAxisAlignment: CrossAxisAlignment.center,
                                mainAxisAlignment: MainAxisAlignment.spaceBetween,
                                mainAxisSize: MainAxisSize.min,
                                children:
                                new List<Widget> {
                                    new _TileIcon(
                                        colorTween: colorTween,
                                        animation: animation,
                                        iconSize: iconSize,
                                        selected: selected,
                                        item: item
                                    ),
                                    new _Label(
                                        colorTween: colorTween,
                                        animation: animation,
                                        item: item,
                                        selectedFontSize: selectedFontSize,
                                        unselectedFontSize: unselectedFontSize,
                                        showSelectedLabels: showSelectedLabels,
                                        showUnselectedLabels: showUnselectedLabels
                                    )
                                }
                            )
                        )
                    )
                }
            )
        );
    }*/
    }


    class _TileIcon : StatelessWidget {
        public _TileIcon(
            Key key = null,
            ColorTween colorTween = null,
            Animation<float> animation = null,
            float? iconSize = null,
            bool? selected = null,
            BottomNavigationBarItem item = null,
            IconThemeData selectedIconTheme = null,
            IconThemeData unselectedIconTheme = null
        ) : base(key: key) {
            D.assert(selected != null);
            D.assert(item != null);
            this.colorTween = colorTween;
            this.animation = animation;
            this.iconSize = iconSize;
            this.selected = selected;
            this.item = item;
            this.selectedIconTheme = selectedIconTheme;
            this.unselectedIconTheme = unselectedIconTheme;
        }

        public readonly ColorTween colorTween;
        public readonly Animation<float> animation;
        public readonly float? iconSize;
        public readonly bool? selected;
        public readonly BottomNavigationBarItem item;
        public readonly IconThemeData selectedIconTheme;
        public readonly IconThemeData unselectedIconTheme;

        public override Widget build(BuildContext context) {
            Color iconColor = colorTween.evaluate(animation);
            IconThemeData defaultIconTheme = new IconThemeData(
                color: iconColor,
                size: iconSize
            );
            IconThemeData iconThemeData = IconThemeData.lerp(
                defaultIconTheme.merge(unselectedIconTheme),
                defaultIconTheme.merge(selectedIconTheme),
                animation.value
            );
            return new Align(
                alignment: Alignment.topCenter,
                heightFactor: 1.0f,
                child: new Container(
                    child: new IconTheme(
                        data:iconThemeData,
                        //child: selected ? item.activeIcon : item.icon
                        /*data: new IconThemeData(
                            color: iconColor,
                            size:  iconSize
                        ),*/
                        child: selected == true ? item.activeIcon : item.icon
                    )
                )
            );
        }
    }

    class _Label : StatelessWidget {
        public _Label(
            Key key = null,
            ColorTween colorTween = null,
            Animation<float> animation = null,
            BottomNavigationBarItem item = null,
            TextStyle selectedLabelStyle = null,
            TextStyle unselectedLabelStyle = null,
            //float? selectedFontSize = null,
            //float? unselectedFontSize = null,
            bool? showSelectedLabels = null,
            bool? showUnselectedLabels = null
        ) : base(key: key) {
            D.assert(colorTween != null);
            D.assert(animation != null);
            D.assert(item != null);
            //D.assert(selectedFontSize != null);
            //D.assert(unselectedFontSize != null);
            D.assert(selectedLabelStyle != null);
            D.assert(unselectedLabelStyle != null);
            D.assert(showSelectedLabels != null);
            D.assert(showUnselectedLabels != null);
            this.colorTween = colorTween;
            this.animation = animation;
            this.item = item;
            this.selectedLabelStyle = selectedLabelStyle;
            this.unselectedLabelStyle = unselectedLabelStyle;
            //this.selectedFontSize = selectedFontSize.Value;
            //this.unselectedFontSize = unselectedFontSize.Value;
            this.showSelectedLabels = showSelectedLabels.Value;
            this.showUnselectedLabels = showUnselectedLabels.Value;
        }

        public readonly ColorTween colorTween;
        public readonly Animation<float> animation;
        public readonly BottomNavigationBarItem item;
        //public readonly float selectedFontSize;
        //public readonly float unselectedFontSize;
        public readonly TextStyle selectedLabelStyle;
        public readonly TextStyle unselectedLabelStyle;
        public readonly bool showSelectedLabels;
        public readonly bool showUnselectedLabels;

        public override Widget build(BuildContext context) {
            float selectedFontSize = selectedLabelStyle.fontSize.Value;
            float unselectedFontSize = unselectedLabelStyle.fontSize.Value;

            TextStyle customStyle = TextStyle.lerp(
                unselectedLabelStyle,
                selectedLabelStyle,
                animation.value
            );
            float t = new FloatTween(
                begin: unselectedFontSize / selectedFontSize,
                end: 1.0f
            ).evaluate(animation);
            Widget text = DefaultTextStyle.merge(
                style: customStyle.copyWith(
                    fontSize: selectedFontSize,
                    color: colorTween.evaluate(animation)
                ),
                child: new Transform(
                    transform: Matrix4.diagonal3Values(t, t, t),
                    /*Matrix4.diagonal3(
                        new Vector3(t,t,t)
                    ),*/
                    alignment: Alignment.bottomCenter,
                    child: item.title
                )
            );

            if (!showUnselectedLabels && !showSelectedLabels) {
                text = new Opacity(
                    //alwaysIncludeSemantics: true,
                    opacity: 0.0f,
                    child: text
                );
            } else if (!showUnselectedLabels) {
                text = new FadeTransition(
                    //alwaysIncludeSemantics: true,
                    opacity: animation,
                    child: text
                );
            } else if (!showSelectedLabels) {
                // Fade selected labels out.
                text = new FadeTransition(
                    //alwaysIncludeSemantics: true,
                    opacity: new FloatTween(begin: 1.0f, end: 0.0f).animate(animation),
                    child: text
                );
            }

            return new Align(
                alignment: Alignment.bottomCenter,
                heightFactor: 1.0f,
                child: new Container(child: text)
            );
            /*float t = new FloatTween(begin: unselectedFontSize / selectedFontSize, end: 1.0f)
                    .evaluate(animation);
            Widget text = DefaultTextStyle.merge(
                style: new TextStyle(
                    fontSize: selectedFontSize,
                    color: colorTween.evaluate(animation)
                ),
                child: new Transform(
                    transform: Matrix4.diagonal3Values(t, t, t),
                    alignment: Alignment.bottomCenter,
                    child: item.title
                )
            );
            if (!showUnselectedLabels && !showSelectedLabels) {
                text = new Opacity(
                    opacity: 0.0f,
                    child: text
                );
            }
            else if (!showUnselectedLabels) {
                text = new FadeTransition(
                    opacity: animation,
                    child: text
                );
            }
            else if (!showSelectedLabels) {
                text = new FadeTransition(
                    opacity: new FloatTween(begin: 1.0f, end: 0.0f).animate(animation),
                    child: text
                );
            }
            return new Align(
                alignment: Alignment.bottomCenter,
                heightFactor: 1.0f,
                child: new Container(child: text)
            );*/
        }
    }

    class _BottomNavigationBarState : TickerProviderStateMixin<BottomNavigationBar> {
        public List<AnimationController> _controllers = new List<AnimationController> { };
        public List<CurvedAnimation> _animations;

        Queue<_Circle> _circles = new Queue<_Circle>();

        Color _backgroundColor;

        static readonly Animatable<float> _flexTween = new FloatTween(begin: 1.0f, end: 1.5f);

        public _BottomNavigationBarState() {
        }

        void _resetState() {
            foreach (AnimationController controller in _controllers) {
                controller.dispose();
            }

            foreach (_Circle circle in _circles) {
                circle.dispose();
            }

            _circles.Clear();

            _controllers = new List<AnimationController>(capacity: widget.items.Count);
            for (int index = 0; index < widget.items.Count; index++) {
                AnimationController controller = new AnimationController(
                    duration: ThemeUtils.kThemeAnimationDuration,
                    vsync: this
                );
                controller.addListener(_rebuild);
                _controllers.Add(controller);
            }

            _animations = new List<CurvedAnimation>(capacity: widget.items.Count);
            for (int index = 0; index < widget.items.Count; index++) {
                _animations.Add(new CurvedAnimation(
                    parent: _controllers[index],
                    curve: Curves.fastOutSlowIn,
                    reverseCurve: Curves.fastOutSlowIn.flipped
                ));
            }

            _controllers[widget.currentIndex].setValue(1.0f);
            _backgroundColor = widget.items[widget.currentIndex].backgroundColor;
        }

        public override void initState() {
            base.initState();
            _resetState();
        }

        void _rebuild() {
            setState(() => { });
        }

        public override void dispose() {
            foreach (AnimationController controller in _controllers) {
                controller.dispose();
            }

            foreach (_Circle circle in _circles) {
                circle.dispose();
            }

            base.dispose();
        }

        public float _evaluateFlex(Animation<float> animation) {
            return _flexTween.evaluate(animation);
        }

        void _pushCircle(int index) {
            if (widget.items[index].backgroundColor != null) {
                _Circle circle = new _Circle(
                    state: this,
                    index: index,
                    color: widget.items[index].backgroundColor,
                    vsync: this
                );
                circle.controller.addStatusListener(
                    (AnimationStatus status) => {
                        switch (status) {
                            case AnimationStatus.completed:
                                setState(() => {
                                    _Circle cir = _circles.Dequeue();
                                    _backgroundColor = cir.color;
                                    cir.dispose();
                                });
                                break;
                            case AnimationStatus.dismissed:
                            case AnimationStatus.forward:
                            case AnimationStatus.reverse:
                                break;
                        }
                    }
                );
                _circles.Enqueue(circle);
            }
        }

        public override void didUpdateWidget(StatefulWidget _oldWidget) {
            base.didUpdateWidget(_oldWidget);
            BottomNavigationBar oldWidget = _oldWidget as BottomNavigationBar;
            if (widget.items.Count != oldWidget.items.Count) {
                _resetState();
                return;
            }

            if (widget.currentIndex != oldWidget.currentIndex) {
                switch (widget.type) {
                    case BottomNavigationBarType.fix:
                        break;
                    case BottomNavigationBarType.shifting:
                        _pushCircle(widget.currentIndex);
                        break;
                }

                _controllers[oldWidget.currentIndex].reverse();
                _controllers[widget.currentIndex].forward();
            }
            else {
                if (_backgroundColor != widget.items[widget.currentIndex].backgroundColor) {
                    _backgroundColor = widget.items[widget.currentIndex].backgroundColor;
                }
            }
        }
        public static TextStyle _effectiveTextStyle(TextStyle textStyle, float fontSize) {
            textStyle = textStyle ?? new TextStyle();
            return textStyle.fontSize == null ? textStyle.copyWith(fontSize: fontSize) : textStyle;
        }
        List<Widget> _createTiles() {
            MaterialLocalizations localizations = MaterialLocalizations.of(context);
            D.assert(localizations != null);
            ThemeData themeData = Theme.of(context);
            TextStyle effectiveSelectedLabelStyle =
                _effectiveTextStyle(widget.selectedLabelStyle, widget.selectedFontSize);
            TextStyle effectiveUnselectedLabelStyle =
                _effectiveTextStyle(widget.unselectedLabelStyle, widget.unselectedFontSize);

            Color themeColor;
            switch (themeData.brightness) {
                case Brightness.light:
                    themeColor = themeData.primaryColor;
                    break;
                case Brightness.dark:
                    themeColor = themeData.accentColor;
                    break;
                default:
                    throw new Exception("Unknown brightness: " + themeData.brightness);
            }

            ColorTween colorTween;
            switch (widget.type) {
                case BottomNavigationBarType.fix:
                    colorTween = new ColorTween(
                        begin: widget.unselectedItemColor ?? themeData.textTheme.caption.color,
                        end: widget.selectedItemColor ?? widget.fixedColor ?? themeColor
                    );
                    break;
                case BottomNavigationBarType.shifting:
                    colorTween = new ColorTween(
                        begin: widget.unselectedItemColor ?? Colors.white,
                        end: widget.selectedItemColor ?? Colors.white
                    );
                    break;
                default:
                    throw new UIWidgetsError($"Unknown bottom navigation bar type: {widget.type}");
            }

            List<Widget> tiles = new List<Widget>();
            for (int i = 0; i < widget.items.Count; i++) {
                int index = i;
                tiles.Add(new _BottomNavigationTile(
                    widget.type,
                    widget.items[i],
                    _animations[i],
                    widget.iconSize,
                    //selectedFontSize: widget.selectedFontSize,
                    //unselectedFontSize: widget.unselectedFontSize,
                    selectedIconTheme: widget.selectedIconTheme,
                    unselectedIconTheme: widget.unselectedIconTheme,
                    selectedLabelStyle: effectiveSelectedLabelStyle,
                    unselectedLabelStyle: effectiveUnselectedLabelStyle,
                    onTap: () => {
                        if (widget.onTap != null) {
                            widget.onTap(index);
                        }
                    },
                    colorTween: colorTween,
                    flex: _evaluateFlex(_animations[i]),
                    selected: i == widget.currentIndex,
                    showSelectedLabels: widget.showSelectedLabels,
                    showUnselectedLabels: widget.showUnselectedLabels,
                    indexLabel: localizations.tabLabel(tabIndex: i+1, tabCount: widget.items.Count)
                ));
            }

            return tiles;
        }

        Widget _createContainer(List<Widget> tiles) {
            return DefaultTextStyle.merge(
                overflow: TextOverflow.ellipsis,
                child: new Row(
                    mainAxisAlignment: MainAxisAlignment.spaceBetween,
                    children: tiles
                )
            );
        }

        public override Widget build(BuildContext context) {
            D.assert(WidgetsD.debugCheckHasDirectionality(context));
            D.assert(MaterialD.debugCheckHasMaterialLocalizations(context));

            float additionalBottomPadding =
                Mathf.Max(MediaQuery.of(context).padding.bottom - widget.selectedFontSize / 2.0f, 0.0f);
            Color backgroundColor = null;
            switch (widget.type) {
                case BottomNavigationBarType.fix:
                    backgroundColor = widget.backgroundColor;
                    break;
                case BottomNavigationBarType.shifting:
                    backgroundColor = _backgroundColor;
                    break;
            }


            return new Material(
                elevation: widget.elevation,
                color: backgroundColor,
                child: new ConstrainedBox(
                    constraints: new BoxConstraints(
                        minHeight: Constants.kBottomNavigationBarHeight + additionalBottomPadding),
                    child: new CustomPaint(
                        painter: new _RadialPainter(
                            circles: _circles.ToList()
                        ),
                        child: new Material( // Splashes.
                            type: MaterialType.transparency,
                            child: new Padding(
                                padding: EdgeInsets.only(bottom: additionalBottomPadding),
                                child: MediaQuery.removePadding(
                                    context: context,
                                    removeBottom: true,
                                    child: _createContainer(_createTiles())
                                )
                            )
                        )
                    )
                )
            );
        }
    }

    class _Circle {
        public _Circle(
            _BottomNavigationBarState state = null,
            int? index = null,
            Color color = null,
            TickerProvider vsync = null
        ) {
            D.assert(state != null);
            D.assert(index != null);
            D.assert(color != null);
            this.state = state;
            this.index = index;
            this.color = color;
            controller = new AnimationController(
                duration: ThemeUtils.kThemeAnimationDuration,
                vsync: vsync
            );
            animation = new CurvedAnimation(
                parent: controller,
                curve: Curves.fastOutSlowIn
            );
            controller.forward();
        }

        public readonly _BottomNavigationBarState state;
        public readonly int? index;
        public readonly Color color;
        public readonly AnimationController controller;
        public readonly CurvedAnimation animation;

        public float horizontalLeadingOffset {
            get {
                float weightSum(IEnumerable<Animation<float>> animations) {
                    return animations.Select(state._evaluateFlex).Sum();
                }

                float allWeights = weightSum(state._animations);
                float leadingWeights = weightSum(state._animations.GetRange(0, index ?? 0));

                return (leadingWeights + state._evaluateFlex(state._animations[index ?? 0]) / 2.0f) /
                       allWeights;
            }
        }

        public void dispose() {
            controller.dispose();
        }
    }

    class _RadialPainter : AbstractCustomPainter {
        public _RadialPainter(
            List<_Circle> circles
        ) {
            D.assert(circles != null);
            this.circles = circles;
        }

        public readonly List<_Circle> circles;

        static float _maxRadius(Offset center, Size size) {
            float maxX = Mathf.Max(center.dx, size.width - center.dx);
            float maxY = Mathf.Max(center.dy, size.height - center.dy);
            return Mathf.Sqrt(maxX * maxX + maxY * maxY);
        }

        public override bool shouldRepaint(CustomPainter _oldPainter) {
            _RadialPainter oldPainter = _oldPainter as _RadialPainter;
            if (circles == oldPainter.circles) {
                return false;
            }

            if (circles.Count != oldPainter.circles.Count) {
                return true;
            }

            for (int i = 0; i < circles.Count; i += 1) {
                if (circles[i] != oldPainter.circles[i]) {
                    return true;
                }
            }

            return false;
        }

        public override void paint(Canvas canvas, Size size) {
            foreach (_Circle circle in circles) {
                Paint paint = new Paint();
                paint.color = circle.color;
                Rect rect = Rect.fromLTWH(0.0f, 0.0f, size.width, size.height);
                canvas.clipRect(rect);
                float leftFraction = circle.horizontalLeadingOffset;
                Offset center = new Offset(leftFraction * size.width, size.height / 2.0f);
                FloatTween radiusTween = new FloatTween(
                    begin: 0.0f,
                    end: _maxRadius(center, size)
                );
                canvas.drawCircle(
                    center,
                    radiusTween.evaluate(circle.animation),
                    paint
                );
            }
        }
    }
}