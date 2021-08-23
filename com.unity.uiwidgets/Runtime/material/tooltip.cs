using System;
using uiwidgets;
using Unity.UIWidgets.animation;
using Unity.UIWidgets.async;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.gestures;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.rendering;
using Unity.UIWidgets.service;
using Unity.UIWidgets.ui;
using Unity.UIWidgets.widgets;
using TextStyle = Unity.UIWidgets.painting.TextStyle;

namespace Unity.UIWidgets.material {
    static class TooltipUtils {
        public static readonly TimeSpan _kFadeDuration = new TimeSpan(0, 0, 0, 0, 200);
        public static readonly TimeSpan _kShowDuration = new TimeSpan(0, 0, 0, 0, 1500);
    }


    public class Tooltip : StatefulWidget {
        public Tooltip(
            Key key = null,
            string message = null,
            float? height = null,
            EdgeInsetsGeometry padding = null,
            EdgeInsetsGeometry margin = null,
            float? verticalOffset = null,
            bool? preferBelow = null,
            bool? excludeFromSemantics = null,
            Decoration decoration = null,
            TextStyle textStyle = null,
            TimeSpan? waitDuration = null,
            TimeSpan? showDuration = null,
            Widget child = null
        ) : base(key: key) {
            D.assert(message != null);
            this.message = message;
            this.height = height;
            this.padding = padding ;
            this.margin = margin;
            this.verticalOffset = verticalOffset ;
            this.preferBelow = preferBelow ;
            this.excludeFromSemantics = excludeFromSemantics;
            this.decoration = decoration;
            this.textStyle = textStyle;
            this.waitDuration = waitDuration;
            this.showDuration = showDuration;
            this.child = child;
        }


        public readonly string message;

        public readonly float? height;

        public readonly EdgeInsetsGeometry padding;
        
        public readonly EdgeInsetsGeometry margin;
        
        public readonly float? verticalOffset;

        public readonly bool? preferBelow;

        public bool? excludeFromSemantics ;
        
        public Decoration decoration ;
        
        public TextStyle textStyle ;
        
        public TimeSpan? waitDuration ;
        
        public TimeSpan? showDuration ;

        public readonly Widget child;


        public override State createState() {
            return new _TooltipState();
        }

        public override void debugFillProperties(DiagnosticPropertiesBuilder properties) {
            base.debugFillProperties(properties);
            properties.add( new StringProperty("message", message, showName: false));
            properties.add( new FloatProperty("height", height, defaultValue: null));
            properties.add( new DiagnosticsProperty<EdgeInsetsGeometry>("padding", padding, defaultValue: null));
            properties.add( new DiagnosticsProperty<EdgeInsetsGeometry>("margin", margin, defaultValue: null));
            properties.add( new FloatProperty("vertical offset", verticalOffset, defaultValue: null));
            properties.add( new FlagProperty("position", value: preferBelow, ifTrue: "below", ifFalse: "above", showName: true, defaultValue: null));
            properties.add( new FlagProperty("semantics", value: excludeFromSemantics, ifTrue: "excluded", showName: true, defaultValue: null));
            properties.add( new DiagnosticsProperty<TimeSpan?>("wait duration", waitDuration, defaultValue: null));
            properties.add( new DiagnosticsProperty<TimeSpan?>("show duration", showDuration, defaultValue: null));
        }
    }


    public class _TooltipState : SingleTickerProviderStateMixin<Tooltip> {
        static float _defaultTooltipHeight = 32.0f;
        static float _defaultVerticalOffset = 24.0f;
        static bool _defaultPreferBelow = true;
        static EdgeInsetsGeometry _defaultPadding = EdgeInsets.symmetric(horizontal: 16.0f);
        static EdgeInsetsGeometry _defaultMargin = EdgeInsets.all(0.0f);
        static TimeSpan _fadeInDuration = new TimeSpan(0,0,0,0, 150);
        static TimeSpan _fadeOutDuration =new TimeSpan(0,0,0,0, 75);
        static TimeSpan _defaultShowDuration =new TimeSpan(0,0,0,0,1500);
        static TimeSpan _defaultWaitDuration =new TimeSpan(0,0,0,0, 0);
        static bool _defaultExcludeFromSemantics = false;
        
        float height;
        
        EdgeInsetsGeometry padding;
        
        EdgeInsetsGeometry margin;
        
        Decoration decoration;
        
        TextStyle textStyle;
        
        float verticalOffset;
        
        bool preferBelow;
        
        bool excludeFromSemantics;
        
        AnimationController _controller;
        
        OverlayEntry _entry;
        
        Timer _hideTimer;
        
        Timer _showTimer;
        
        TimeSpan showDuration;
        
        TimeSpan waitDuration;
        
        bool _mouseIsConnected;
        
        bool _longPressActivated = false;
        
        public override void initState() {
            base.initState();
            _mouseIsConnected = RendererBinding.instance.mouseTracker.mouseIsConnected;
            _controller = new AnimationController(duration: _fadeInDuration, reverseDuration: _fadeOutDuration, vsync: this);
            _controller.addStatusListener(_handleStatusChanged);
            
            RendererBinding.instance.mouseTracker.addListener(_handleMouseTrackerChange);
            GestureBinding.instance.pointerRouter.addGlobalRoute(_handlePointerEvent);
        }
        
        public void _handleMouseTrackerChange() {
            if (!mounted) {
                return;
            }
            bool mouseIsConnected = RendererBinding.instance.mouseTracker.mouseIsConnected;
            if (mouseIsConnected != _mouseIsConnected) {
                setState(()=>{
                    _mouseIsConnected = mouseIsConnected;
                });
            }
        }

        void _handleStatusChanged(AnimationStatus status) {
            if (status == AnimationStatus.dismissed) {
                _hideTooltip(immediately: true);
            }
        }
        
        void _hideTooltip( bool immediately = false ) {
            _showTimer?.cancel();
            _showTimer = null;
            if (immediately) {
                _removeEntry();
                return;
            }
            if (_longPressActivated) {
                _hideTimer = _hideTimer ?? Timer.create(showDuration, () => {
                    _controller.reverse();
                });
            } else {
                _controller.reverse();
            }
            _longPressActivated = false;
        }
        
        void _showTooltip( bool immediately = false ) {
            _hideTimer?.cancel();
            _hideTimer = null;
            if (immediately) {
                ensureTooltipVisible();
                return;
            }
            _showTimer = _showTimer ?? Timer.create(waitDuration, () => {
                ensureTooltipVisible();
            });
        }
        
        bool ensureTooltipVisible() {
            _showTimer?.cancel();
            _showTimer = null;
            
            if (_entry != null) {
                _hideTimer?.cancel();
                _hideTimer = null;
                _controller.forward();
                return false;
            }
            
            _createNewEntry();
            _controller.forward();
            return true;
        }
        
        void _createNewEntry() {
            RenderBox box = context.findRenderObject() as RenderBox;
            Offset target = box.localToGlobal(box.size.center(Offset.zero));
            
            Widget overlay = new Directionality(
                textDirection: Directionality.of(context),
                child: new _TooltipOverlay(
                    message: widget.message,
                    height: height,
                    padding: padding,
                    margin: margin,
                    decoration: decoration,
                    textStyle: textStyle,
                    animation: new CurvedAnimation(
                        parent: _controller,
                        curve: Curves.fastOutSlowIn
                    ),
                    target: target,
                    verticalOffset: verticalOffset,
                    preferBelow: preferBelow
                )
            );
            
            _entry = new OverlayEntry(builder: (BuildContext context) => overlay);
            Overlay.of(context, debugRequiredFor: widget).insert(_entry);
        }
        
        
        void _removeEntry() {
            _hideTimer?.cancel();
            _hideTimer = null;
            _showTimer?.cancel();
            _showTimer = null;
            _entry?.remove();
            _entry = null;
        }

        void _handlePointerEvent(PointerEvent pEvent) {
           
            if (_entry == null) {
                return;
            }
            if (pEvent is PointerUpEvent || pEvent is PointerCancelEvent) {
                _hideTooltip();
            } else if (pEvent is PointerDownEvent) {
                _hideTooltip(immediately: true);
            }
        }

        public override void deactivate() {
            if (_entry != null) {
                _hideTooltip(immediately: true);
            }

            base.deactivate();
        }

        public override void dispose() {
            GestureBinding.instance.pointerRouter.removeGlobalRoute(_handlePointerEvent);
            RendererBinding.instance.mouseTracker.removeListener(_handleMouseTrackerChange);
            
            if (_entry != null) {
                _removeEntry();
            }

            _controller.dispose();
            base.dispose();
        }

        void _handleLongPress() {
            _longPressActivated = true;
            bool tooltipCreated = ensureTooltipVisible();
            if (tooltipCreated) {
                Feedback.forLongPress(context);
            }
        }

        public override Widget build(BuildContext context) {
            D.assert(Overlay.of(context, debugRequiredFor: widget) != null);
            ThemeData theme = Theme.of(context);
            TooltipThemeData tooltipTheme = TooltipTheme.of(context);
            TextStyle defaultTextStyle;
            BoxDecoration defaultDecoration;
            
            if (theme.brightness == Brightness.dark) {
              defaultTextStyle = theme.textTheme.bodyText2.copyWith(
                color: Colors.black
              );
              defaultDecoration = new BoxDecoration(
                color: Colors.white.withOpacity(0.9f),
                borderRadius: BorderRadius.all(Radius.circular(4))
              );
            } else {
              defaultTextStyle = theme.textTheme.bodyText2.copyWith(
                color: Colors.white
              );
              defaultDecoration = new BoxDecoration(
                color: Colors.grey[700].withOpacity(0.9f),
                borderRadius: BorderRadius.all(Radius.circular(4))
              );
            }

            height = widget.height ?? tooltipTheme?.height ?? _defaultTooltipHeight;
            padding = widget.padding ?? tooltipTheme?.padding ?? _defaultPadding;
            margin = widget.margin ?? tooltipTheme?.margin ?? _defaultMargin;
            verticalOffset = widget.verticalOffset ?? tooltipTheme?.verticalOffset ?? _defaultVerticalOffset;
            preferBelow = widget.preferBelow ?? tooltipTheme?.preferBelow ?? _defaultPreferBelow;
            excludeFromSemantics = widget.excludeFromSemantics ?? tooltipTheme?.excludeFromSemantics ?? _defaultExcludeFromSemantics;
            decoration = widget.decoration ?? tooltipTheme?.decoration ?? defaultDecoration;
            textStyle = widget.textStyle ?? tooltipTheme?.textStyle ?? defaultTextStyle;
            waitDuration = widget.waitDuration ?? tooltipTheme?.waitDuration ?? _defaultWaitDuration;
            showDuration = widget.showDuration ?? tooltipTheme?.showDuration ?? _defaultShowDuration;

            Widget result = new GestureDetector(
              behavior: HitTestBehavior.opaque,
              onLongPress: _handleLongPress,
              child: widget.child
            );
            
            if (_mouseIsConnected) {
              result = new MouseRegion(
                onEnter: (PointerEnterEvent _event) => _showTooltip(),
                onExit: (PointerExitEvent _event) => _hideTooltip(),
                child: result
              );
            }

            return result;
        }
    }


    public class _TooltipPositionDelegate : SingleChildLayoutDelegate {
        public _TooltipPositionDelegate(
            Offset target = null,
            float? verticalOffset = null,
            bool? preferBelow = null) {
            D.assert(target != null);
            D.assert(verticalOffset != null);
            D.assert(preferBelow != null);
            this.target = target;
            this.verticalOffset = verticalOffset ?? 0.0f;
            this.preferBelow = preferBelow ?? true;
        }

        public readonly Offset target;

        public readonly float verticalOffset;

        public readonly bool preferBelow;

        public override BoxConstraints getConstraintsForChild(BoxConstraints constraints) {
            return constraints.loosen();
        }

        public override Offset getPositionForChild(Size size, Size childSize) {
            return Geometry.positionDependentBox(
                size: size,
                childSize: childSize,
                target: target,
                verticalOffset: verticalOffset,
                preferBelow: preferBelow);
        }

        public override bool shouldRelayout(SingleChildLayoutDelegate oldDelegate) {
            _TooltipPositionDelegate _oldDelegate = (_TooltipPositionDelegate) oldDelegate;
            return target != _oldDelegate.target ||
                   verticalOffset != _oldDelegate.verticalOffset ||
                   preferBelow != _oldDelegate.preferBelow;
        }
    }


    class _TooltipOverlay : StatelessWidget {
        public _TooltipOverlay(
            Key key = null,
            string message = null,
            float? height = null,
            EdgeInsetsGeometry padding = null,
            EdgeInsetsGeometry margin = null,
            Decoration decoration = null,
            TextStyle textStyle = null,
            Animation<float> animation = null,
            Offset target = null,
            float? verticalOffset = null,
            bool? preferBelow = null
        ) : base(key: key) {
            this.message = message;
            this.height = height;
            this.padding = padding;
            this.margin = margin;
            this.decoration = decoration;
            this.textStyle = textStyle;
            this.animation = animation;
            this.target = target;
            this.verticalOffset = verticalOffset;
            this.preferBelow = preferBelow;
        }

        public readonly string message;

        public readonly float? height;

        public readonly EdgeInsetsGeometry padding;

        public readonly EdgeInsetsGeometry margin;

        public readonly Decoration decoration;

        public readonly TextStyle textStyle;

        public readonly Animation<float> animation;

        public readonly Offset target;

        public readonly float? verticalOffset;

        public readonly bool? preferBelow;


        public override Widget build(BuildContext context) {
            return Positioned.fill(
                child: new IgnorePointer(
                    child: new CustomSingleChildLayout(
                        layoutDelegate: new _TooltipPositionDelegate(
                            target: target,
                            verticalOffset: verticalOffset,
                            preferBelow: preferBelow),
                        child: new FadeTransition(
                            opacity: animation,
                            child: new ConstrainedBox(
                                constraints: new BoxConstraints(minHeight: height.Value),
                                child: new DefaultTextStyle(
                                    style: Theme.of(context).textTheme.bodyText2,
                                    child: new Container(
                                        decoration: decoration,
                                        padding: padding,
                                        margin: margin,
                                        child: new Center(
                                            widthFactor: 1.0f,
                                            heightFactor: 1.0f,
                                            child: new Text(message, style: textStyle)
                                            )
                                        ))
                                )
                        )
                    )
                )
            );
        }
    }
}