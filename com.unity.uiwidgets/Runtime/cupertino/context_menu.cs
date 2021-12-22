using System.Collections.Generic;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.animation;
using Unity.UIWidgets.rendering;
using Unity.UIWidgets.ui;
using Unity.UIWidgets.widgets;
using Unity.UIWidgets.scheduler;
using System;
using Unity.UIWidgets.gestures;
using Unity.UIWidgets.painting;
using UnityEngine;
using Color = Unity.UIWidgets.ui.Color;
using Rect = Unity.UIWidgets.ui.Rect;
using Transform = Unity.UIWidgets.widgets.Transform;

namespace Unity.UIWidgets.cupertino {
    
    class CupertinoContextMenuUtils {

        public static float  _kOpenScale = 1.1f;
        public static float kMinFlingVelocity = 50.0f;

        public static Rect _getRect(GlobalKey globalKey) {
            D.assert(globalKey.currentContext != null);
            RenderBox renderBoxContainer = globalKey.currentContext.findRenderObject() as RenderBox;
            Offset containerOffset = renderBoxContainer.localToGlobal(
                renderBoxContainer.paintBounds.topLeft
            );
            return containerOffset & renderBoxContainer.paintBounds.size;
        }

        public static Rect fromCenter(Offset center, float width, float height) {
            return Rect.fromLTRB(
                center.dx - width / 2,
                center.dy - height / 2,
                center.dx + width / 2,
                center.dy + height / 2
            );
        }
    
    }
    public delegate void _DismissCallback(
        BuildContext context,
        float? scale,
        float opacity
    );

    public delegate Widget ContextMenuPreviewBuilder(
        BuildContext context,
        Animation<float> animation,
        Widget child
    );

    public delegate Widget _ContextMenuPreviewBuilderChildless (
        BuildContext context,
        Animation<float> animation
    );

    public enum _ContextMenuLocation {
        center,
        left,
        right,
    }

    public class CupertinoContextMenu : StatefulWidget {
        public CupertinoContextMenu(
            Key key = null,
            List<Widget> actions = null,
            Widget child = null,
            ContextMenuPreviewBuilder previewBuilder = null
        ) : base(key: key) {
            D.assert(actions != null && actions.isNotEmpty());
            D.assert(child != null);
            this.actions = actions;
            this.child = child;
            this.previewBuilder = previewBuilder;
        }

        public readonly Widget child;

        public readonly List<Widget> actions;

        public readonly ContextMenuPreviewBuilder previewBuilder;

        public override State createState() {
            return new _CupertinoContextMenuState();
        }

    }

    public class _CupertinoContextMenuState :  TickerProviderStateMixin<CupertinoContextMenu> { 

        public readonly GlobalKey _childGlobalKey = GlobalKey<State<StatefulWidget>>.key();
        static readonly TimeSpan  kLongPressTimeout = TimeSpan.FromMilliseconds(500);
        public bool _childHidden = false;
        public AnimationController _openController;
        public Rect _decoyChildEndRect;
        public OverlayEntry _lastOverlayEntry;
        public _ContextMenuRoute _route;

        public override void initState() {
            base.initState();
            _openController = new AnimationController(
              duration:  kLongPressTimeout,
              vsync: this
            );
            _openController.addStatusListener(_onDecoyAnimationStatusChange);
        }

        public _ContextMenuLocation _contextMenuLocation {
            get {
                Rect childRect = CupertinoContextMenuUtils._getRect(_childGlobalKey);
                float screenWidth = MediaQuery.of(context).size.width;
                float center = screenWidth / 2;
                bool centerDividesChild = childRect.left < center
                                                && childRect.right > center;
                float distanceFromCenter = (center - childRect.center.dx).abs();
                if (centerDividesChild && distanceFromCenter <= childRect.width / 4) {
                    return _ContextMenuLocation.center;
                }

                if (childRect.center.dx > center) {
                    return _ContextMenuLocation.right;
                }

                return _ContextMenuLocation.left;
            }
        } 
        public void _openContextMenu() {
            setState(()=>{
              _childHidden = true;
            });

            _route = new _ContextMenuRoute(
                actions: widget.actions,
                barrierLabel: "Dismiss",
                filter: ImageFilter.blur(
                sigmaX: 5.0f,
                sigmaY: 5.0f
                ),
                contextMenuLocation: _contextMenuLocation,
                previousChildRect: _decoyChildEndRect,
                builder: (BuildContext _context, Animation<float> animation)=>{
                    if (widget.previewBuilder == null) {
                        return widget.child;
                    }
                    return widget.previewBuilder(_context, animation, widget.child);
                }
            );
            Navigator.of(context, rootNavigator: true).push(_route);
            _route.animation.addStatusListener(_routeAnimationStatusListener);
        }

        public void _onDecoyAnimationStatusChange(AnimationStatus animationStatus) { 
            switch (animationStatus) { 
                case AnimationStatus.dismissed: 
                    if (_route == null) { 
                        setState(()=> { 
                            _childHidden = false; 
                        });
                    }
                    _lastOverlayEntry?.remove();
                    _lastOverlayEntry = null;
                    break;
                case AnimationStatus.completed:
                    setState(()=>{
                        _childHidden = true;
                    });
                    _openContextMenu();
                    SchedulerBinding.instance.addPostFrameCallback((TimeSpan timestamp) =>{
                        _lastOverlayEntry?.remove();
                        _lastOverlayEntry = null;
                        _openController.reset();
                    });
                    break;
                default:
                    return;
            }
        }


        public void _routeAnimationStatusListener(AnimationStatus status) { 
          if (status != AnimationStatus.dismissed) {
            return;
          }
          setState(()=>{
            _childHidden = false;
          });
          _route.animation.removeStatusListener(_routeAnimationStatusListener);
          _route = null;
        }

        public void _onTap() {
          if (_openController.isAnimating && _openController.value < 0.5f) {
            _openController.reverse();
          }
        }
        
        public void _onTapCancel() {
          if (_openController.isAnimating && _openController.value < 0.5f) {
            _openController.reverse();
          }
        }

        public void _onTapUp(TapUpDetails details) {
          if(_openController.isAnimating && _openController.value < 0.5f) {
            _openController.reverse();
          }
        }

        public void _onTapDown(TapDownDetails details) {
          setState(()=>{ 
            _childHidden = true;
          });
          Rect childRect = CupertinoContextMenuUtils._getRect(_childGlobalKey);
          _decoyChildEndRect = CupertinoContextMenuUtils.fromCenter(
            center: childRect.center,
            width: childRect.width * CupertinoContextMenuUtils._kOpenScale,
            height: childRect.height * CupertinoContextMenuUtils._kOpenScale
          );
          _lastOverlayEntry = new OverlayEntry(
            opaque: false,
            builder: (BuildContext context) => {
              return new _DecoyChild(
                beginRect: childRect,
                child: widget.child,
                controller: _openController,
                endRect: _decoyChildEndRect
              );
            }
          );
          Overlay.of(context).insert(_lastOverlayEntry);
          _openController.forward();
        }

        public override Widget build(BuildContext context) {
          return new GestureDetector(
            onTapCancel: _onTapCancel,
            onTapDown: _onTapDown,
            onTapUp: _onTapUp,
            onTap: _onTap,
            child: new TickerMode(
              enabled: !_childHidden,
              child: new Opacity(
                key: _childGlobalKey,
                opacity: _childHidden ? 0.0f : 1.0f,
                child: widget.child
              )
            )
          );
        }
        public override void dispose() {
          _openController.dispose();
          base.dispose();
        }
    }
    public class _DecoyChild : StatefulWidget { 
        public _DecoyChild(
            Key key = null,
            Rect beginRect = null,
            AnimationController controller = null,
            Rect endRect = null,
            Widget child = null
        ) : base(key: key) {
            this.beginRect = beginRect;
            this.controller = controller;
            this.endRect = endRect;
            this.child = child;
        }

        public readonly Rect beginRect;
        public readonly AnimationController controller;
        public readonly Rect endRect;
        public readonly  Widget child;

        public override State createState() {
            return new _DecoyChildState();
        }
    }
    public class _DecoyChildState : TickerProviderStateMixin<_DecoyChild> {
  
        public static readonly Color _lightModeMaskColor = new Color(0xFF888888);
        public static readonly Color _masklessColor = new Color(0xFFFFFFFF);
        public readonly GlobalKey _childGlobalKey = GlobalKey<State<StatefulWidget>>.key();
        public Animation<Color> _mask;
        public Animation<Rect> _rect;

        public override void initState() { 
            base.initState();
            _mask = new _OnOffAnimationColor(
                controller: widget.controller,
                onValue: _lightModeMaskColor,
                offValue: _masklessColor,
                intervalOn: 0.0f,
                intervalOff: 0.5f
            );
            Rect midRect =  widget.beginRect.deflate(
                widget.beginRect.width * (CupertinoContextMenuUtils._kOpenScale - 1.0f) / 2f
            );
            List<TweenSequenceItem<Rect>> tweenSequenceItems = new List<TweenSequenceItem<Rect>>();
            tweenSequenceItems.Add(
                new TweenSequenceItem<Rect>(
                    tween: new RectTween(
                    begin: widget.beginRect,
                    end: midRect
              ).chain(new CurveTween(curve: Curves.easeInOutCubic)),
              weight: 1.0f
          ));

          tweenSequenceItems.Add(
            new TweenSequenceItem<Rect>(
              tween: new RectTween(
                begin: midRect,
                end: widget.endRect
            ).chain(new CurveTween(curve: Curves.easeOutCubic)),
            weight: 1.0f
          ));

          _rect = new TweenSequence<Rect>(tweenSequenceItems).animate(widget.controller);
          _rect.addListener(_rectListener);

        }
        public void _rectListener() {
          if (widget.controller.value < 0.5f) {
            return;
          }
          //HapticFeedback.selectionClick();????
          /// tbc ???
          _rect.removeListener(_rectListener);
        }
        public override void dispose() {
          _rect.removeListener(_rectListener);
          base.dispose();
        }

        public Widget _buildAnimation(BuildContext context, Widget child) { 
            Color color = widget.controller.status == AnimationStatus.reverse ? _masklessColor : _mask.value;
            List<Color> colors = new List<Color>();
            colors.Add(color);
            colors.Add(color);
            return Positioned.fromRect(
                rect: _rect.value,
                child: foundation_.kIsWeb 
                    ? (Widget) new Container(key: _childGlobalKey, child: widget.child)
                    : new ShaderMask(
                        key: _childGlobalKey, 
                        shaderCallback: (Rect bounds) => { 
                            return new LinearGradient(
                                begin: Alignment.topLeft, 
                                end: Alignment.bottomRight, 
                                colors: colors).createShader(bounds); 
                        }, 
                        child: widget.child
                    )
            );
        }
        public override Widget build(BuildContext context) { 
            List<Widget> widgets = new List<Widget>();
            widgets.Add(new  AnimatedBuilder(
                builder: _buildAnimation,
                animation: widget.controller)
            ); 
            return new Stack(
                children: widgets
                );
        }
    }
    public class _ContextMenuRoute : PopupRoute { 
        public _ContextMenuRoute(
            List<Widget> actions = null, 
            _ContextMenuLocation? contextMenuLocation = null, 
            string barrierLabel = null, 
            _ContextMenuPreviewBuilderChildless builder = null, 
            ImageFilter filter = null, 
            Rect previousChildRect = null, 
            RouteSettings settings = null
            ) : base(filter: filter, settings: settings){ 
            
            D.assert(actions != null && actions.isNotEmpty());
            D.assert(contextMenuLocation != null);
            this.barrierLabel = barrierLabel;
            _actions = actions;
            _builder = builder;
            _contextMenuLocation = contextMenuLocation.Value;
            _previousChildRect = previousChildRect;
        } 
        
        public readonly static Color _kModalBarrierColor = new Color(0x6604040F);
        public readonly TimeSpan _kModalPopupTransitionDuration =new TimeSpan(0, 0, 0, 0, 335);
        public readonly List<Widget> _actions;
        public readonly _ContextMenuPreviewBuilderChildless _builder;
        public readonly GlobalKey _childGlobalKey = new LabeledGlobalKey<State<StatefulWidget>>();
        public readonly _ContextMenuLocation _contextMenuLocation;
        public bool _externalOffstage = false;
        public bool _internalOffstage = false;
        public Orientation _lastOrientation;
        public readonly Rect _previousChildRect;
        public float? _scale = 1.0f;
        public readonly GlobalKey _sheetGlobalKey = new LabeledGlobalKey<State<StatefulWidget>>();
        public static readonly CurveTween _curve = new CurveTween(
            curve: Curves.easeOutBack
        ); 
        public static readonly  CurveTween _curveReverse = new CurveTween(
            curve: Curves.easeInBack
        );
        public static readonly RectTween _rectTween = new RectTween();
        public static readonly Animatable<Rect> _rectAnimatable = _rectTween.chain(_curve);
        public static readonly RectTween _rectTweenReverse = new RectTween();
        public static readonly Animatable<Rect> _rectAnimatableReverse = _rectTweenReverse.chain(_curveReverse);

        public static readonly RectTween _sheetRectTween = new RectTween();
        public readonly Animatable<Rect> _sheetRectAnimatable = _sheetRectTween.chain(_curve);
        public readonly Animatable<Rect> _sheetRectAnimatableReverse = _sheetRectTween.chain(_curveReverse);
        public static readonly  Tween<float?> _sheetScaleTween = new NullableFloatTween(0.0f,0.0f);
        public static readonly  Animatable<float?> _sheetScaleAnimatable = _sheetScaleTween.chain(_curve);
        public static readonly  Animatable<float?> _sheetScaleAnimatableReverse = _sheetScaleTween.chain(_curveReverse);
        public readonly Tween< float> _opacityTween = new FloatTween(begin: 0.0f, end: 1.0f);
        public Animation< float> _sheetOpacity;
      
        //public readonly string barrierLabel;
        public override string barrierLabel { get; }

        public override Color barrierColor {
            get { return _kModalBarrierColor; }
        }
        public override bool barrierDismissible{ 
            get { return false; }
        }

        public override bool semanticsDismissible { 
            get { return true; }
        }

        public override TimeSpan transitionDuration {
            get {
                return _kModalPopupTransitionDuration;
            }
        }
        public static AlignmentDirectional getSheetAlignment(_ContextMenuLocation contextMenuLocation) {
            switch (contextMenuLocation) {
                case _ContextMenuLocation.center:
                    return AlignmentDirectional.topCenter;
                case _ContextMenuLocation.right:
                    return AlignmentDirectional.topEnd;
                default:
                    return AlignmentDirectional.topStart;
            }
        }
        public static Rect _getScaledRect(GlobalKey globalKey,  float scale) {
            Rect childRect = CupertinoContextMenuUtils._getRect(globalKey);
            Size sizeScaled = childRect.size * scale;
            Offset offsetScaled = new Offset(
            childRect.left + (childRect.size.width - sizeScaled.width) / 2,
            childRect.top + (childRect.size.height - sizeScaled.height) / 2
            );
            return offsetScaled & sizeScaled;
        }
        public static Rect _getSheetRectBegin(Orientation orientation, _ContextMenuLocation contextMenuLocation, Rect childRect, Rect sheetRect) { 
            switch (contextMenuLocation) { 
                case _ContextMenuLocation.center: 
                    Offset target1 = (orientation == Orientation.portrait) 
                        ? childRect.bottomCenter
                        : childRect.topCenter;
                    Offset centered = target1 - new Offset(sheetRect.width / 2, 0.0f);
                    return centered & sheetRect.size;
                case _ContextMenuLocation.right: 
                    Offset target2 = ((orientation == Orientation.portrait) 
                        ? childRect.bottomRight 
                        : childRect.topRight); 
                    return (target2 - new Offset(sheetRect.width, 0.0f)) & sheetRect.size; 
                default: 
                    Offset target3 = orientation == Orientation.portrait 
                        ? childRect.bottomLeft 
                        : childRect.topLeft; 
                    return target3 & sheetRect.size;
            }
        } 
        public void _onDismiss(BuildContext context, float? scale, float opacity) { 
            _scale = scale;
            _opacityTween.end = opacity;
            _sheetOpacity = _opacityTween.animate(
                new CurvedAnimation(
                    parent: animation, 
                    curve: new Interval(0.9f, 1.0f)
                  ));
            Navigator.of(context).pop<object>();
        }
        public void _updateTweenRects() { 
            Rect childRect = _scale == null 
                ? CupertinoContextMenuUtils._getRect(_childGlobalKey) 
                : _getScaledRect(_childGlobalKey, _scale.Value);
            _rectTween.begin = _previousChildRect; 
            _rectTween.end = childRect; 
            Rect childRectOriginal = CupertinoContextMenuUtils.fromCenter(
                center: _previousChildRect.center,
                width: _previousChildRect.width / CupertinoContextMenuUtils._kOpenScale,
                height: _previousChildRect.height / CupertinoContextMenuUtils._kOpenScale
            ); 
            Rect sheetRect = CupertinoContextMenuUtils._getRect(_sheetGlobalKey);
            Rect sheetRectBegin = _getSheetRectBegin(
                _lastOrientation,
                _contextMenuLocation,
                childRectOriginal,
                sheetRect
            ); 
            _sheetRectTween.begin = sheetRectBegin;
            _sheetRectTween.end = sheetRect;
            _sheetScaleTween.begin = 0.0f;
            _sheetScaleTween.end = _scale;
            _rectTweenReverse.begin = childRectOriginal; 
            _rectTweenReverse.end = childRect;
        }
        public void _setOffstageInternally() {
            base.offstage = _externalOffstage || _internalOffstage;
            changedInternalState();
        }

        protected internal override bool didPop(object result) {
            _updateTweenRects();
            return base.didPop(result);
        }

        public override bool offstage{
           set{
               _externalOffstage = value;
               _setOffstageInternally();
           }
        }

        protected internal override TickerFuture didPush() {
            _internalOffstage = true;
            _setOffstageInternally();
            SchedulerBinding.instance.addPostFrameCallback((TimeSpan timeSpan)=>{
                _updateTweenRects();
                _internalOffstage = false;
                _setOffstageInternally();
            });
            return base.didPush();
        }
        public override Animation<float> createAnimation() { 
            Animation< float> animation = base.createAnimation();
            _sheetOpacity = _opacityTween.animate(new CurvedAnimation(
                parent: animation,
                curve: Curves.linear
            ));
            return animation;
        }
        public override Widget buildPage(BuildContext context, Animation< float> animation, Animation< float> secondaryAnimation) {
            return null;
        }
        public override Widget buildTransitions(BuildContext context1, Animation< float> animation, Animation< float> secondaryAnimation, Widget child) {
            return new OrientationBuilder(
                builder: (BuildContext context2, Orientation orientation)=>{ 
                    _lastOrientation = orientation; 
                    if (!animation.isCompleted) { 
                        bool reverse = animation.status == AnimationStatus.reverse;
                        Rect rect = reverse 
                            ? _rectAnimatableReverse.evaluate(animation) 
                            : _rectAnimatable.evaluate(animation);
                        Rect sheetRect = reverse
                            ? _sheetRectAnimatableReverse.evaluate(animation)
                            : _sheetRectAnimatable.evaluate(animation);
                        float? sheetScale = reverse
                            ? _sheetScaleAnimatableReverse.evaluate(animation)
                            : _sheetScaleAnimatable.evaluate(animation); 
                        List<Widget> widgets = new List<Widget>(); 
                        widgets.Add(
                            Positioned.fromRect(
                                rect: sheetRect, 
                                child: new Opacity(
                                    opacity: _sheetOpacity.value, 
                                    child: Transform.scale( 
                                        alignment: getSheetAlignment(_contextMenuLocation),
                                        scale: sheetScale ?? 1.0f, 
                                        child: new _ContextMenuSheet(
                                            key: _sheetGlobalKey, 
                                            actions: _actions, 
                                            contextMenuLocation: _contextMenuLocation, 
                                            orientation: orientation
                                            )
                                        )
                                    )
                                )
                            ); 
                        widgets.Add(
                            Positioned.fromRect(
                                key: _childGlobalKey, 
                                rect: rect, 
                                child: _builder(context2, animation)
                                )); 
                        return new Stack(
                            children: widgets
                            ); 
                    } 
                    return new _ContextMenuRouteStatic(
                        actions: _actions,
                        child: _builder(context1, animation),
                        childGlobalKey: _childGlobalKey,
                        contextMenuLocation: _contextMenuLocation,
                        onDismiss: _onDismiss,
                        orientation: orientation,
                        sheetGlobalKey: _sheetGlobalKey
                        );
                }
            );
        }
    }
    public class _ContextMenuRouteStatic : StatefulWidget { 
        public _ContextMenuRouteStatic(
            Key key = null,
            List<Widget> actions = null,
            Widget child = null,
            GlobalKey childGlobalKey = null,
            _ContextMenuLocation contextMenuLocation = default,
            _DismissCallback onDismiss =default,
            Orientation orientation = default,
            GlobalKey sheetGlobalKey = null
            ) : base(key: key) {
            
            D.assert(contextMenuLocation != default);
            D.assert(orientation != default);
            this.actions = actions;
            this.child = child;
            this.childGlobalKey = childGlobalKey; 
            this.contextMenuLocation = contextMenuLocation;
            this.onDismiss = onDismiss;
            this.orientation = orientation;
            this.sheetGlobalKey = sheetGlobalKey;
        }
        public readonly List<Widget> actions;
        public readonly Widget child;
        public readonly GlobalKey childGlobalKey;
        public readonly _ContextMenuLocation contextMenuLocation;
        public readonly _DismissCallback onDismiss;
        public readonly Orientation orientation;
        public readonly GlobalKey sheetGlobalKey;

        public override State createState() { 
            return new _ContextMenuRouteStaticState();
        }
    }

    public class _ContextMenuRouteStaticState : TickerProviderStateMixin<_ContextMenuRouteStatic> {
        public readonly static  float _kMinScale = 0.8f;
        public readonly static  float _kSheetScaleThreshold = 0.9f;
        public readonly static  float _kPadding = 20.0f;
        public readonly static  float _kDamping = 400.0f;
        public readonly static TimeSpan _kMoveControllerDuration = TimeSpan.FromMilliseconds(600);
        Offset _dragOffset;
        public float _lastScale = 1.0f;
        public AnimationController _moveController;
        public AnimationController _sheetController;
        public Animation<Offset> _moveAnimation;
        public Animation< float> _sheetScaleAnimation;
        public Animation< float> _sheetOpacityAnimation; 
        public static float _getScale(Orientation orientation,  float maxDragDistance,  float dy) { 
            float dyDirectional = dy <= 0.0f ? dy : -dy;
            return Mathf.Max(
                _kMinScale, 
                (maxDragDistance + dyDirectional) / maxDragDistance
            );
        } 
        void _onPanStart(DragStartDetails details) {
            _moveController.setValue(1.0f) ;
            _setDragOffset(Offset.zero);
        } 
        void _onPanUpdate(DragUpdateDetails details) {
            _setDragOffset(_dragOffset + details.delta);
        } 
        void _onPanEnd(DragEndDetails details) { 
            if (details.velocity.pixelsPerSecond.dy.abs() >= CupertinoContextMenuUtils.kMinFlingVelocity) { 
                bool flingIsAway = details.velocity.pixelsPerSecond.dy > 0; 
                float finalPosition = flingIsAway 
                    ? _moveAnimation.value.dy + 100.0f
                    : 0.0f; 
                if (flingIsAway && _sheetController.status != AnimationStatus.forward) { 
                    _sheetController.forward();
                } else if (!flingIsAway && _sheetController.status != AnimationStatus.reverse) {
                    _sheetController.reverse();
                }

                _moveAnimation = new OffsetTween(
                    begin: new Offset(0.0f, _moveAnimation.value.dy),
                    end:new  Offset(0.0f, finalPosition)
                ).animate(_moveController);
                _moveController.reset();
                _moveController.duration = TimeSpan.FromMilliseconds(64);
                _moveController.forward();
                _moveController.addStatusListener(_flingStatusListener);
                return; 
            } 
            if (_lastScale == _kMinScale) {
                widget.onDismiss(context, _lastScale, _sheetOpacityAnimation.value);
                return;
            }
            _moveController.addListener(_moveListener);
            _moveController.reverse();
    }

    void _moveListener() { 
        if (_lastScale > _kSheetScaleThreshold) {
            _moveController.removeListener(_moveListener); 
            if (_sheetController.status != AnimationStatus.dismissed) { 
                _sheetController.reverse();
            }
        }
    }

    void _flingStatusListener(AnimationStatus status) { 
        if (status != AnimationStatus.completed) { 
            return;
        }
        _moveController.duration = _kMoveControllerDuration;
        _moveController.removeStatusListener(_flingStatusListener);
        if (_moveAnimation.value.dy == 0.0) {
            return;
        }
        widget.onDismiss(context, _lastScale, _sheetOpacityAnimation.value);
    }
    Alignment _getChildAlignment(Orientation orientation, _ContextMenuLocation contextMenuLocation) { 
        switch (contextMenuLocation) { 
            case _ContextMenuLocation.center: 
                return orientation == Orientation.portrait 
                    ? Alignment.bottomCenter 
                    : Alignment.topRight;
            case _ContextMenuLocation.right: 
                return orientation == Orientation.portrait 
                    ? Alignment.bottomCenter
                    : Alignment.topLeft;
            default:
                return orientation == Orientation.portrait
                    ? Alignment.bottomCenter
                    : Alignment.topRight;
        }
    }

    void _setDragOffset(Offset dragOffset) {
        float endX = _kPadding * dragOffset.dx / _kDamping;
        float endY = dragOffset.dy >= 0.0 
            ? dragOffset.dy 
            : _kPadding * dragOffset.dy / _kDamping; 
        setState(() =>{ 
            _dragOffset = dragOffset;
            _moveAnimation = new OffsetTween(
                begin: Offset.zero,
                end: new Offset(
                    endX.clamp(-_kPadding, _kPadding) , 
                    endY
                    )).animate(
                new CurvedAnimation(
                    parent: _moveController, 
                    curve: Curves.elasticIn
                    )
        );
        if (_lastScale <= _kSheetScaleThreshold && _sheetController.status != AnimationStatus.forward
                                                && _sheetScaleAnimation.value != 0.0f) { 
            _sheetController.forward();
        } else if (_lastScale > _kSheetScaleThreshold && _sheetController.status != AnimationStatus.reverse
                                                      && _sheetScaleAnimation.value != 1.0f) { 
            _sheetController.reverse();
        } 
        });
    }
    List<Widget> _getChildren(Orientation orientation, _ContextMenuLocation contextMenuLocation) { 
        Expanded child = new Expanded(
            child: new Align(
                alignment: _getChildAlignment(
                    widget.orientation, 
                    widget.contextMenuLocation
                ), 
                child: new AnimatedBuilder(
                    animation: _moveController,
                    builder: _buildChildAnimation,
                    child: widget.child
                )
            )
        );
        Container spacer = new Container(
            width: _kPadding,
            height: _kPadding
            );
        Expanded sheet = new Expanded(
            child: new AnimatedBuilder(
                animation: _sheetController,
                builder: _buildSheetAnimation,
                child: new _ContextMenuSheet(
                    key: widget.sheetGlobalKey,
                    actions: widget.actions,
                    contextMenuLocation: widget.contextMenuLocation,
                    orientation: widget.orientation
                )
            )
        ); 
        List<Widget> centerWidgets = new List<Widget>();
        centerWidgets.Add(child);
        centerWidgets.Add(spacer);
        centerWidgets.Add(sheet);
        List<Widget> rightWidgets = new List<Widget>();
        rightWidgets.Add(sheet);
        rightWidgets.Add(spacer);
        rightWidgets.Add(child);
      switch (contextMenuLocation) {
        case _ContextMenuLocation.center:
            return new List<Widget>{child, spacer, sheet};
        case _ContextMenuLocation.right:
          return orientation == Orientation.portrait
            ? new List<Widget>{child, spacer, sheet}
            : new List<Widget>{sheet, spacer, child};
        default:
          return new List<Widget>{child, spacer, sheet};
      }
    }

    // Build the animation for the _ContextMenuSheet.
    Widget _buildSheetAnimation(BuildContext context, Widget child) {
      return Transform.scale(
        alignment: _ContextMenuRoute.getSheetAlignment(widget.contextMenuLocation),
        scale: _sheetScaleAnimation.value,
        child: new Opacity(
          opacity: _sheetOpacityAnimation.value,
          child: child
        )
      );
    }

    // Build the animation for the child.
    Widget _buildChildAnimation(BuildContext context, Widget child) {
      _lastScale = _getScale(
        widget.orientation,
        MediaQuery.of(context).size.height,
        _moveAnimation.value.dy
      );
      return Transform.scale(
        key: widget.childGlobalKey,
        scale: _lastScale,
        child: child
      );
    }

    // Build the animation for the overall draggable dismissable content.
    Widget _buildAnimation(BuildContext context, Widget child) {
      return Transform.translate(
        offset: _moveAnimation.value,
        child: child
      );
    }

      
    public override void initState() {
      base.initState();
      _moveController = new AnimationController(
        duration: _kMoveControllerDuration,
        value: 1.0f,
        vsync: this
      );
      _sheetController = new AnimationController(
        duration: TimeSpan.FromMilliseconds(100),
        reverseDuration: TimeSpan.FromMilliseconds(300),
        vsync: this
      );
      _sheetScaleAnimation = new FloatTween(
        begin: 1.0f,
        end: 0.0f
      ).animate(
        new CurvedAnimation(
          parent: _sheetController,
          curve: Curves.linear,
          reverseCurve: Curves.easeInBack
        )
      );
      _sheetOpacityAnimation = new FloatTween(
        begin: 1.0f,
        end: 0.0f
      ).animate(_sheetController);
      _setDragOffset(Offset.zero);
    }

    public override void dispose() {
      _moveController.dispose();
      _sheetController.dispose();
      base.dispose();
    }

      
    public override Widget build(BuildContext context) {
      List<Widget> children = _getChildren(
        widget.orientation,
        widget.contextMenuLocation
      );

      return new SafeArea(
        child: new Padding(
          padding: EdgeInsets.all(_kPadding),
          child: new Align(
            alignment: Alignment.topLeft,
            child: new GestureDetector(
              onPanEnd: _onPanEnd,
              onPanStart: _onPanStart,
              onPanUpdate: _onPanUpdate,
              child: new AnimatedBuilder(
                animation: _moveController,
                builder: _buildAnimation,
                child: widget.orientation == Orientation.portrait ?
                    (Widget) new Column(
                        crossAxisAlignment: CrossAxisAlignment.start,
                        children: children
                    )
                  : new Row(
                    crossAxisAlignment: CrossAxisAlignment.start,
                    children: children
                  )
                )
              )
            )
          )
        );
      }
    }
  public class _ContextMenuSheet : StatelessWidget {
    public _ContextMenuSheet(
      Key key = null,
      List<Widget> actions = null,
      _ContextMenuLocation? contextMenuLocation = null,
      Orientation? orientation = null
    ) : base(key: key) {
      D.assert(actions != null && actions.isNotEmpty());
      D.assert(contextMenuLocation != null);
      D.assert(orientation != null);
      _contextMenuLocation = contextMenuLocation.Value;
      _orientation = orientation.Value;
      this.actions = actions;
    }
    public readonly List<Widget> actions;
    public readonly _ContextMenuLocation _contextMenuLocation;
    public readonly Orientation _orientation;
    public List<Widget> children { 
      get{
        Flexible menu = new Flexible(
          fit: FlexFit.tight,
          flex: 2,
          child: new IntrinsicHeight(
            child: new ClipRRect(
              borderRadius: BorderRadius.circular(13.0f),
              child: new Column(
                crossAxisAlignment: CrossAxisAlignment.stretch,
                children: actions
              )
            )
          )
        );
        List<Widget> spacers1 = new List<Widget>();
        List<Widget> spacers2 = new List<Widget>();
        spacers1.Add(new Spacer(
          flex: 1
        ));
        spacers1.Add(menu); 
        spacers1.Add(new Spacer(
          flex: 1
        ));
        spacers2.Add(menu); 
        spacers2.Add(new Spacer(
          flex: 1
        ));
        List<Widget> spacers3 = new List<Widget>();
        List<Widget> spacers4 = new List<Widget>();
        spacers3.Add(new Spacer(
          flex: 1
        ));
        spacers3.Add(menu);
        spacers4.Add(menu); 
        spacers4.Add(new Spacer(
          flex: 1
        ));
        switch (_contextMenuLocation) {
          case _ContextMenuLocation.center:
            return _orientation == Orientation.portrait
              ? spacers1
              : spacers2;
          case _ContextMenuLocation.right:
            return spacers3;
          default:
            return spacers4;
        }
                  
      }
    }
    public override Widget build(BuildContext context) {
      return new Row(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: children
      );
    }
  }

  
  /**
   * We cannot use _OnOffAnimation<T> here directly since Tween<T> is an abstract class in UIWidgets (while it is not in flutter)
   * Refer to "Twee<T>" for the detailed reasons why we do so
   *
   * Instead, we should explicitly define classes for each generic type here
   * 
   */
  public class _OnOffAnimationColor : CompoundAnimation<Color> { 
    public _OnOffAnimationColor(
      AnimationController controller = null,
      Color onValue = default,
      Color offValue = default,
      float? intervalOn = null,
      float? intervalOff = null
    ) : base(
        first: new ColorTween(begin: offValue, end: onValue).animate(
            new CurvedAnimation(
                parent: controller,
                curve: new Interval(intervalOn == null ? 0.0f : (float)intervalOn, intervalOn == null ? 0.0f : (float)intervalOn)
                )
            ),
        next: new ColorTween(begin: onValue, end: offValue).animate(
            new CurvedAnimation(
                parent: controller,
                curve: new Interval(intervalOff == null ? 0.0f : (float)intervalOff, intervalOff == null ? 0.0f : (float)intervalOff)
                )
            )
        )
    {
      D.assert(intervalOn != null && intervalOn >= 0.0 && intervalOn <= 1.0);
      D.assert(intervalOff !=null && intervalOff >= 0.0 && intervalOff <= 1.0);
      D.assert(intervalOn <= intervalOff);
    }
    public readonly Color _offValue;
    public override Color value {
      get {
        return next.value.Equals( _offValue) ? next.value : first.value; 
      }
    }

  }

        
}

