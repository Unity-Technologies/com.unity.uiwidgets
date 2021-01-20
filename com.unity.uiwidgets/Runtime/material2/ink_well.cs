using System;
using System.Collections.Generic;
using System.Text;
//using RSG;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.gestures;
using Unity.UIWidgets.material;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.ui;
using Unity.UIWidgets.widgets;
using Unity.UIWidgets.rendering;
using Color = Unity.UIWidgets.ui.Color;
namespace Unity.UIWidgets.material {
    public abstract class InteractiveInkFeature : InkFeature {

        public InteractiveInkFeature(
            MaterialInkController controller,
            RenderBox referenceBox,
            Color color,
            VoidCallback onRemoved 
        ) : base(controller: controller, referenceBox: referenceBox, onRemoved: onRemoved) {
            D.assert(controller != null);
            D.assert(referenceBox != null);
            _color = color;
            
            //super(controller: controller, referenceBox: referenceBox, onRemoved: onRemoved);
        }

        public void confirm() { }

        public void cancel() { }

        public Color color {
            get { return _color; }
            set {  
                if (value == _color)
                    return;
                _color = value;
                controller.markNeedsPaint();
                
            }
        }
        Color _color;
 

        protected void paintInkCircle(
            Canvas canvas,
            Matrix4 transform,
            Paint paint,
            Offset center,
            float radius,
            TextDirection textDirection,
            ShapeBorder customBorder,
            BorderRadius borderRadius ,
            RectCallback clipCallback) {
            
            borderRadius = borderRadius ?? BorderRadius.zero;
            D.assert(canvas != null);
            D.assert(transform != null);
            D.assert(paint != null);
            D.assert(center != null);
            D.assert(radius != null);
            D.assert(borderRadius != null);

            Offset originOffset = MatrixUtils.getAsTranslation(transform);
            canvas.save();
            if (originOffset == null) {
              canvas.transform(transform.storage);
            } else {
              canvas.translate(originOffset.dx, originOffset.dy);
            }
            if (clipCallback != null) {
              Rect rect = clipCallback();
              if (customBorder != null) {
                canvas.clipPath(customBorder.getOuterPath(rect));//, textDirection: textDirection)); 
              } 
              else if (borderRadius != BorderRadius.zero) {
                canvas.clipRRect(RRect.fromRectAndCorners(
                  rect,
                  topLeft: borderRadius.topLeft, topRight: borderRadius.topRight,
                  bottomLeft: borderRadius.bottomLeft, bottomRight: borderRadius.bottomRight
                ));
              } 
              else {
                canvas.clipRect(rect);
              }
            }
            canvas.drawCircle(center, radius, paint);
            canvas.restore();
          }
    }

    public abstract class InteractiveInkFeatureFactory {
       
        public InteractiveInkFeatureFactory() {
        }
        public abstract InteractiveInkFeature create(MaterialInkController controller,
          RenderBox referenceBox,
          Offset position,
          Color color,
          TextDirection textDirection,
          RectCallback rectCallback,
          BorderRadius borderRadius,
          ShapeBorder customBorder,
          float radius,
          VoidCallback onRemoved,
          bool containedInkWell = false);
    }

    class InkResponse : StatefulWidget {
     
       public InkResponse(
        Key key = null,
        Widget child= null,
        GestureTapCallback onTap= null,
        GestureTapDownCallback onTapDown= null,
        GestureTapCallback onTapCancel= null,
        GestureTapCallback onDoubleTap= null,
        GestureLongPressCallback onLongPress= null,
        ValueChanged<bool> onHighlightChanged= null,
        ValueChanged<bool> onHover= null,
        ValueChanged<bool> onFocusChange= null,
        BoxShape highlightShape = BoxShape.circle,
        float radius = 0f,
        BorderRadius borderRadius = null,
        ShapeBorder customBorder = null,
        Color focusColor = null,
        Color hoverColor = null,
        Color highlightColor = null,
        Color splashColor = null, 
        InteractiveInkFeatureFactory splashFactory = null,
        FocusNode focusNode = null,
        bool containedInkWell = false,
        bool enableFeedback = true,
        bool excludeFromSemantics = false,
        bool canRequestFocus = true,
        bool autofocus = false
        ) : base(key : key)
       {
           D.assert(containedInkWell != null);
           D.assert(highlightShape != null);
           D.assert(enableFeedback != null);
           D.assert(excludeFromSemantics != null);
           D.assert(autofocus != null);
           D.assert(canRequestFocus != null);
           this.child = child;
           this.onTap = onTap;
           this.onTapDown = onTapDown;
           this.onTapCancel = onTapCancel;
           this.onDoubleTap = onDoubleTap;
           this.onLongPress = onLongPress;
           this.onHighlightChanged = onHighlightChanged;
           this.onHover = onHover;
           this.onFocusChange = onFocusChange;
           this.highlightShape = BoxShape.circle;
           this.radius = radius;
           this.borderRadius = borderRadius;
           this.customBorder = customBorder;
           this.focusColor = focusColor;
           this.hoverColor = hoverColor;
           this.highlightColor = highlightColor;
           this.splashColor = splashColor;
           this.splashFactory = splashFactory;
           this.focusNode = focusNode;
           this.containedInkWell = containedInkWell;
           this.enableFeedback = enableFeedback;
           this.excludeFromSemantics = excludeFromSemantics;
           this.canRequestFocus = canRequestFocus;
           this.autofocus = autofocus;

       }


       public readonly Widget child;
        public readonly GestureTapCallback onTap;
        public readonly GestureTapDownCallback onTapDown;
        public readonly GestureTapCallback onTapCancel;
        public readonly GestureTapCallback onDoubleTap;
        public readonly GestureLongPressCallback onLongPress;
        public readonly ValueChanged<bool> onHighlightChanged;
        public readonly ValueChanged<bool> onHover;
        public readonly bool containedInkWell;
        public readonly BoxShape highlightShape;
        public readonly float radius;
        public readonly BorderRadius borderRadius;
        public readonly ShapeBorder customBorder;
        public readonly Color focusColor;
        public readonly Color hoverColor;
        public readonly Color highlightColor;
        public readonly Color splashColor;
        public readonly InteractiveInkFeatureFactory splashFactory;
        public readonly bool enableFeedback;
        readonly bool excludeFromSemantics;
        public readonly ValueChanged<bool> onFocusChange;
        readonly bool autofocus;
        public readonly FocusNode focusNode;
        public readonly bool canRequestFocus;

        public RectCallback getRectCallback(RenderBox referenceBox) {

            return null;
        }

        public bool debugCheckContext(BuildContext context) { 
            D.assert(MaterialD.debugCheckHasMaterial(context));
            D.assert(WidgetsD.debugCheckHasDirectionality(context));
            return true;
        }

        public override State createState() {
            return new _InkResponseState<InkResponse>();
        }

        public override void debugFillProperties(DiagnosticPropertiesBuilder properties) {
            base.debugFillProperties(properties);
            List<string> gestures = new List<string>();
            string gesture = "";
            if (onTap != null) {
                gesture = "tap";
                gestures.Add(gesture);
            }

            if (onDoubleTap != null) {
                gesture = "double tap";
                gestures.Add(gesture);   
            }
            if (onLongPress != null){
                gesture = "long press";
                gestures.Add(gesture);
            } 
           
            if (onTapDown != null) {
                gesture = "tap down";
                gestures.Add(gesture);
            }
      
            if (onTapCancel != null){
                gesture = "tap cancel"; 
                gestures.Add(gesture);
            } 
                    
            properties.add(new IterableProperty<string>("gestures", gestures, ifEmpty: "<none>"));
            properties.add(new DiagnosticsProperty<bool>("containedInkWell", containedInkWell, level: DiagnosticLevel.fine));
            properties.add(new DiagnosticsProperty<BoxShape>(
                "highlightShape",
                highlightShape,
                description: $"{containedInkWell}" + " ? clipped to  : " + $"{highlightShape}",
                showName: false
            ));
         }
    }


    public enum _HighlightType {
        pressed,
        hover,
        focus,
    }

    class _InkResponseState<T> : State<T> , AutomaticKeepAliveClientMixin<T> where T : InkResponse {
        HashSet<InteractiveInkFeature> _splashes;
        InteractiveInkFeature _currentSplash;
        bool _hovering = false;
        readonly Dictionary<_HighlightType, InkHighlight> _highlights = new Dictionary<_HighlightType, InkHighlight>();
        Dictionary<LocalKey, ActionFactory> _actionMap;

        bool highlightsExist {
          get {
              //_highlights.Values.where((InkHighlight highlight) => highlight != null).isNotEmpty;
              int i = 0;
              foreach (var highlightsValue in _highlights.Values) {
                  if (highlightsValue != null) {
                      i++;
                  }
              }
              if (i != 0) {
                  return true;
              }
              else 
                  return false;
          }
        }

        void _handleAction(FocusNode node, Intent intent) {
            _startSplash(context: node.context);
            _handleTap(node.context);
        }

        Action _createAction() {
            return CallbackAction(
              ActivateAction.key,
              onInvoke:  _handleAction
            );
        }

       
        public override void initState() {
            base.initState();
            _actionMap = new Dictionary<LocalKey, ActionFactory>{
              ActivateAction.key: _createAction,
            };
            FocusManager.instance.addHighlightModeListener(_handleFocusHighlightModeChange);
        }

        public override void didUpdateWidget(StatefulWidget oldWidget) {
            oldWidget = (T) oldWidget;
            base.didUpdateWidget(oldWidget);
            if (_isWidgetEnabled(widget) != _isWidgetEnabled((InkResponse)oldWidget)) {
              _handleHoverChange(_hovering);
              _updateFocusHighlights();
            }
        }

        public override void dispose() {
            FocusManager.instance.removeHighlightModeListener(_handleFocusHighlightModeChange);
            base.dispose();
        }

        
        public  bool  wantKeepAlive {
            get {
                return highlightsExist || (_splashes != null && _splashes.isNotEmpty());
            }
        }

        Color getHighlightColorForType(_HighlightType type) {
            switch (type) {
              case _HighlightType.pressed:
                return widget.highlightColor ?? Theme.of(context).highlightColor;
              case _HighlightType.focus:
                return widget.focusColor ?? Theme.of(context).focusColor;
              case _HighlightType.hover:
                return widget.hoverColor ?? Theme.of(context).hoverColor;
            }

            D.assert(false, () => "Unhandled " + $"{typeof(_HighlightType)}" + $"{type}");
            
            return null;
        }

        TimeSpan? getFadeDurationForType(_HighlightType type) {
            switch (type) {
              case _HighlightType.pressed:
                return new TimeSpan(0,0,0,0,200);
              case _HighlightType.hover:
              case _HighlightType.focus:
                return new TimeSpan(0,0,0,0,50);
            }
            D.assert(false, ()=>$"Unhandled _HighlightType {type}");
            return null;
        }

        void updateHighlight(_HighlightType type,   bool value = false) {
            InkHighlight highlight = _highlights[type];
            void handleInkRemoval() {
               D.assert(_highlights[type] != null);
              _highlights[type] = null;
              updateKeepAlive();
            }
            if (value == (highlight != null && highlight.active))
              return;
            if (value) {
              if (highlight == null) {
                RenderBox referenceBox = context.findRenderObject() as RenderBox;
                _highlights[type] = new InkHighlight(
                  controller: Material.of(context),
                  referenceBox: referenceBox,
                  color: getHighlightColorForType(type),
                  shape: widget.highlightShape,
                  borderRadius: widget.borderRadius,
                  customBorder: widget.customBorder,
                  rectCallback: widget.getRectCallback(referenceBox),
                  onRemoved: handleInkRemoval,
                  textDirection: Directionality.of(context),
                  fadeDuration: getFadeDurationForType(type)
                );
                updateKeepAlive();
              } else {
                highlight.activate();
              }
            } else {
              highlight.deactivate();
            }
             D.assert(value == (_highlights[type] != null && _highlights[type].active));
             switch (type) {
              case _HighlightType.pressed:
                if (widget.onHighlightChanged != null)
                  widget.onHighlightChanged(value);
                break;
              case _HighlightType.hover:
                if (widget.onHover != null)
                  widget.onHover(value);
                break;
              case _HighlightType.focus:
                break;
            }
        }

        InteractiveInkFeature _createInkFeature(Offset globalPosition) {
            MaterialInkController inkController = Material.of(context);
            RenderBox referenceBox = context.findRenderObject() as RenderBox;
            Offset position = referenceBox.globalToLocal(globalPosition);
            Color color = widget.splashColor ?? Theme.of(context).splashColor;
            RectCallback rectCallback = widget.containedInkWell ? widget.getRectCallback(referenceBox) : null;
            BorderRadius borderRadius = widget.borderRadius;
            ShapeBorder customBorder = widget.customBorder;

            InteractiveInkFeature splash;
            void onRemoved() {
              if (_splashes != null) {
                 D.assert(_splashes.Contains(splash));
                _splashes.Remove(splash);
                if (_currentSplash == splash)
                  _currentSplash = null;
                updateKeepAlive();
              } // else we"re probably in deactivate()
            }

            splash = (widget.splashFactory ?? Theme.of(context).splashFactory).create(
              controller: inkController,
              referenceBox: referenceBox,
              position: position,
              color: color,
              containedInkWell: widget.containedInkWell,
              rectCallback: rectCallback,
              radius: widget.radius,
              borderRadius: borderRadius,
              customBorder: customBorder,
              onRemoved: onRemoved,
              textDirection: Directionality.of(context)
            );

            return splash;
        }

        void _handleFocusHighlightModeChange(FocusHighlightMode mode) {
            if (!mounted) {
              return;
            }
            setState(() =>{
              _updateFocusHighlights();
            });
        }

        void _updateFocusHighlights() {
            bool showFocus;
            switch (FocusManager.instance.highlightMode) {
              case FocusHighlightMode.touch:
                showFocus = false;
                break;
              case FocusHighlightMode.traditional:
                showFocus = enabled && _hasFocus;
                break;
            }
            updateHighlight(_HighlightType.focus, value: showFocus);
        }

        bool _hasFocus = false;
        void _handleFocusUpdate(bool hasFocus) {
            _hasFocus = hasFocus;
            _updateFocusHighlights();
            if (widget.onFocusChange != null) {
              widget.onFocusChange(hasFocus);
            }
        }

        void _handleTapDown(TapDownDetails details) {
            _startSplash(details: details);
            if (widget.onTapDown != null) {
              widget.onTapDown(details);
            }
        }

        void _startSplash(TapDownDetails details = null, BuildContext context = null) { 
            D.assert(details != null || context != null);

        Offset globalPosition;
        if (context != null) {
            RenderBox referenceBox = context.findRenderObject() as RenderBox;
           D.assert(referenceBox.hasSize, "InkResponse must be done with layout before starting a splash.");
          globalPosition = referenceBox.localToGlobal(referenceBox.paintBounds.center);
        } else {
          globalPosition = details.globalPosition;
        }
        readonly InteractiveInkFeature splash = _createInkFeature(globalPosition);
        _splashes ??= new HashSet<InteractiveInkFeature>();
        _splashes.Add(splash);
        _currentSplash = splash;
        updateKeepAlive();
        updateHighlight(_HighlightType.pressed, value: true);
        }

        void _handleTap(BuildContext context) {
            _currentSplash?.confirm();
            _currentSplash = null;
            updateHighlight(_HighlightType.pressed, value: false);
            if (widget.onTap != null) {
              if (widget.enableFeedback)
                Feedback.forTap(context);
              widget.onTap();
            }
        }

        void _handleTapCancel() {
            _currentSplash?.cancel();
            _currentSplash = null;
            if (widget.onTapCancel != null) {
              widget.onTapCancel();
            }
            updateHighlight(_HighlightType.pressed, value: false);
        }

        void _handleDoubleTap() {
            _currentSplash?.confirm();
            _currentSplash = null;
            if (widget.onDoubleTap != null)
              widget.onDoubleTap();
        }

        void _handleLongPress(BuildContext context) {
            _currentSplash?.confirm();
            _currentSplash = null;
            if (widget.onLongPress != null) {
              if (widget.enableFeedback)
                Feedback.forLongPress(context);
              widget.onLongPress();
            }
        }

       
        public override void deactivate() {
            if (_splashes != null) {
              HashSet<InteractiveInkFeature> splashes = _splashes;
              _splashes = null;
              foreach (InteractiveInkFeature splash in splashes)
                splash.dispose();
              _currentSplash = null;
            }
             D.assert(_currentSplash == null);
            foreach ( _HighlightType highlight in _highlights.Keys) {
              _highlights[highlight]?.dispose();
              _highlights[highlight] = null;
            }
            base.deactivate();
        }

        bool _isWidgetEnabled(InkResponse widget) {
            return widget.onTap != null || widget.onDoubleTap != null || widget.onLongPress != null;
        }

        bool enabled {
            get {
                return  _isWidgetEnabled(widget);
            }
        }

        void _handleMouseEnter(PointerEnterEvent Event) {
            _handleHoverChange(true);
        }

        void _handleMouseExit(PointerExitEvent Event) {
            _handleHoverChange(false);
        }

        void _handleHoverChange(bool hovering) {
            if (_hovering != hovering) {
              _hovering = hovering;
              updateHighlight(_HighlightType.hover, value: enabled && _hovering);
            }
        }


        public override Widget build(BuildContext context) { 
            D.assert(widget.debugCheckContext(context));
            base.build(context); // See AutomaticKeepAliveClientMixin.
            foreach ( _HighlightType type in _highlights.Keys) {
              _highlights[type]?.color = getHighlightColorForType(type);
            }
            _currentSplash?.color = widget.splashColor ?? Theme.of(context).splashColor;
            bool canRequestFocus = enabled && widget.canRequestFocus;
            return new Actions(
              actions: _actionMap,
              child: new Focus(
                focusNode: widget.focusNode,
                canRequestFocus: canRequestFocus,
                onFocusChange: _handleFocusUpdate,
                autofocus: widget.autofocus,
                child: new MouseRegion(
                  onEnter: enabled ? _handleMouseEnter : null,
                  onExit: enabled ? _handleMouseExit : null,
                  child: new GestureDetector(
                    onTapDown: enabled ? _handleTapDown : null,
                    onTap: enabled ? () => _handleTap(context) : null,
                    onTapCancel: enabled ? _handleTapCancel : null,
                    onDoubleTap: widget.onDoubleTap != null ? _handleDoubleTap : null,
                    onLongPress: widget.onLongPress != null ? () => _handleLongPress(context) : null,
                    behavior: HitTestBehavior.opaque,
                    //excludeFromSemantics: widget.excludeFromSemantics,
                    child: widget.child
                  )
                )
              )
            );
        }
    }

class InkWell : InkResponse {
 
  const InkWell({
    Key key,
    Widget child,
    GestureTapCallback onTap,
    GestureTapCallback onDoubleTap,
    GestureLongPressCallback onLongPress,
    GestureTapDownCallback onTapDown,
    GestureTapCancelCallback onTapCancel,
    ValueChanged<bool> onHighlightChanged,
    ValueChanged<bool> onHover,
    Color focusColor,
    Color hoverColor,
    Color highlightColor,
    Color splashColor,
    InteractiveInkFeatureFactory splashFactory,
    float radius,
    BorderRadius borderRadius,
    ShapeBorder customBorder,
    bool enableFeedback = true,
    bool excludeFromSemantics = false,
    FocusNode focusNode,
    bool canRequestFocus = true,
    ValueChanged<bool> onFocusChange,
    bool autofocus = false,
  }) : super(
    key: key,
    child: child,
    onTap: onTap,
    onDoubleTap: onDoubleTap,
    onLongPress: onLongPress,
    onTapDown: onTapDown,
    onTapCancel: onTapCancel,
    onHighlightChanged: onHighlightChanged,
    onHover: onHover,
    containedInkWell: true,
    highlightShape: BoxShape.rectangle,
    focusColor: focusColor,
    hoverColor: hoverColor,
    highlightColor: highlightColor,
    splashColor: splashColor,
    splashFactory: splashFactory,
    radius: radius,
    borderRadius: borderRadius,
    customBorder: customBorder,
    enableFeedback: enableFeedback ?? true,
    excludeFromSemantics: excludeFromSemantics ?? false,
    focusNode: focusNode,
    canRequestFocus: canRequestFocus ?? true,
    onFocusChange: onFocusChange,
    autofocus: autofocus ?? false,
  );
}

}