using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using Unity.UIWidgets.animation;
using Unity.UIWidgets.cupertino;
using Unity.UIWidgets.ui;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.gestures;
using Unity.UIWidgets.widgets;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.physics;
using Unity.UIWidgets.rendering;
using UnityEngine;
using Color = Unity.UIWidgets.ui.Color;
using Rect = Unity.UIWidgets.ui.Rect;
using TextStyle = Unity.UIWidgets.painting.TextStyle;

namespace Unity.UIWidgets.cupertino {
    public class CupertinoSlidingSegmentedControlsUtils {
        public static readonly EdgeInsetsGeometry _kHorizontalItemPadding = EdgeInsets.symmetric(vertical: 2, horizontal: 3);
        public static readonly Radius _kThumbRadius = Radius.circular(6.93f);

        public static readonly EdgeInsets _kThumbInsets = EdgeInsets.symmetric(horizontal: 1);


        public const float _kMinSegmentedControlHeight = 28.0f;

        public static readonly Color _kSeparatorColor = new Color(0x4D8E8E93);

        public static readonly CupertinoDynamicColor _kThumbColor = CupertinoDynamicColor.withBrightness(
            color: new Color(0xFFFFFFFF),
            darkColor: new Color(0xFF636366)
        );

        public static readonly EdgeInsets _kSeparatorInset = EdgeInsets.symmetric(vertical: 6);
        public const float _kSeparatorWidth = 1;
        public static readonly Radius _kSeparatorRadius = Radius.circular(_kSeparatorWidth/2);


        public const float _kMinThumbScale = 0.95f;

        public const float _kSegmentMinPadding = 9.25f;

        public const float _kTouchYDistanceThreshold = 50.0f * 50.0f;

        public const float _kCornerRadius = 8;

        public static readonly SpringSimulation _kThumbSpringAnimationSimulation = new SpringSimulation(
          new SpringDescription(mass: 1, stiffness: 503.551f, damping: 44.8799f),
          0,
          1,
          0 // Everytime a new spring animation starts the previous animation stops.
        );

        public static readonly TimeSpan _kSpringAnimationDuration= TimeSpan.FromMilliseconds(412);

        public static readonly TimeSpan _kOpacityAnimationDuration = TimeSpan.FromMilliseconds(470);

        public static readonly TimeSpan _kHighlightAnimationDuration = TimeSpan.FromMilliseconds(200);
    }
    
    class _FontWeightTween : Tween<FontWeight> {
        public _FontWeightTween(FontWeight begin = null, FontWeight end = null) : base(begin: begin, end: end) {
        }

        public override FontWeight lerp(float t) => FontWeight.lerp(begin, end, t);
    }

    public class CupertinoSlidingSegmentedControl<T> : StatefulWidget {

        public CupertinoSlidingSegmentedControl( 
            Key key = null,
            Dictionary<T, Widget> children = null,
            ValueChanged<T> onValueChanged = null,
            T groupValue = default,
            Color thumbColor = null,
            EdgeInsetsGeometry padding = null,
            Color backgroundColor = null
        ) :base(key: key)
        {
            D.assert(children != null);
            D.assert(children.Count >= 2);
            D.assert(onValueChanged != null);
            D.assert(
                groupValue == null || children.Keys.Contains(groupValue),()=>
                "The groupValue must be either null or one of the keys in the children map."
            );
            this.children = children;
            this.onValueChanged = onValueChanged;
            this.groupValue = groupValue;
            this.thumbColor = thumbColor ?? CupertinoSlidingSegmentedControlsUtils._kThumbColor;
            this.padding = padding ??CupertinoSlidingSegmentedControlsUtils._kHorizontalItemPadding;
            this.backgroundColor = backgroundColor ?? CupertinoColors.tertiarySystemFill;
        }
        public readonly Dictionary<T, Widget> children;
        public readonly T groupValue;
        public readonly ValueChanged<T> onValueChanged;
        public readonly Color backgroundColor;
        public readonly Color thumbColor; 
        public readonly EdgeInsetsGeometry padding;

        public override State createState() {
            return new _SlidingSegmentedControlState<T>();
        }
    }
    public class _SlidingSegmentedControlState<T> : TickerProviderStateMixin<CupertinoSlidingSegmentedControl<T>> {

        public readonly Dictionary<T, AnimationController> _highlightControllers = new Dictionary<T, AnimationController>();
        public readonly Tween<FontWeight> _highlightTween = new _FontWeightTween(begin: FontWeight.normal, end: FontWeight.w500);
        public readonly Dictionary<T, AnimationController> _pressControllers = new Dictionary<T, AnimationController>(); 
        public readonly Tween<float> _pressTween = new FloatTween(begin: 1, end: 0.2f);

        List<T> keys;

        public AnimationController thumbController;
        public AnimationController separatorOpacityController;
        public AnimationController thumbScaleController;

          public readonly TapGestureRecognizer tap = new TapGestureRecognizer();
          public readonly HorizontalDragGestureRecognizer drag = new HorizontalDragGestureRecognizer();
          public readonly LongPressGestureRecognizer longPress = new LongPressGestureRecognizer();

          AnimationController _createHighlightAnimationController( bool isCompleted = false ) {
            return new AnimationController(
              duration: CupertinoSlidingSegmentedControlsUtils._kHighlightAnimationDuration,
              value: isCompleted ? 1 : 0,
              vsync: this
            );
          }
        AnimationController _createFadeoutAnimationController() {
            return new AnimationController(
              duration:CupertinoSlidingSegmentedControlsUtils._kOpacityAnimationDuration,
              vsync: this
            );
        }
        public override void initState() {
            base.initState();

            GestureArenaTeam team = new GestureArenaTeam();
            
            longPress.team = team;
            drag.team = team;
            team.captain = drag;

            _highlighted = widget.groupValue;

            thumbController = new AnimationController(
              duration: CupertinoSlidingSegmentedControlsUtils._kSpringAnimationDuration,
              value: 0,
              vsync: this
            );

            thumbScaleController = new AnimationController(
              duration:    CupertinoSlidingSegmentedControlsUtils._kSpringAnimationDuration,
              value: 1,
              vsync: this
            );

            separatorOpacityController = new AnimationController(
              duration:    CupertinoSlidingSegmentedControlsUtils._kSpringAnimationDuration,
              value: 0,
              vsync: this
            );

            foreach (T currentKey in widget.children.Keys) {
              _highlightControllers[currentKey] = _createHighlightAnimationController(
                isCompleted: currentKey.Equals(widget.groupValue)  // Highlight the current selection.
              );
              _pressControllers[currentKey] = _createFadeoutAnimationController();
            }
        }
        
        public override void didUpdateWidget(StatefulWidget oldWidget) {
            oldWidget = (CupertinoSlidingSegmentedControl<T>)oldWidget;
            base.didUpdateWidget(oldWidget);

            // Update animation controllers.
            foreach( T oldKey in ((CupertinoSlidingSegmentedControl<T>)oldWidget).children.Keys) {
              if (!widget.children.ContainsKey(oldKey)) {
                _highlightControllers[oldKey].dispose();
                _pressControllers[oldKey].dispose();

                _highlightControllers.Remove(oldKey);
                _pressControllers.Remove(oldKey);
              }
            }

            foreach( T newKey in widget.children.Keys) {
              if (!_highlightControllers.Keys.Contains(newKey)) {
                _highlightControllers[newKey] = _createHighlightAnimationController();
                _pressControllers[newKey] = _createFadeoutAnimationController();
              }
            }

            highlighted = widget.groupValue;
          }
        public override void dispose() {
            foreach( AnimationController animationController in _highlightControllers.Values) {
              animationController.dispose();
            }

            foreach( AnimationController animationController in _pressControllers.Values) {
              animationController.dispose();
            }

            thumbScaleController.dispose();
            thumbController.dispose();
            separatorOpacityController.dispose();

            drag.dispose();
            tap.dispose();
            longPress.dispose();

            base.dispose();
        }

        void _animateHighlightController( T at = default, bool forward = false) {
            if (at == null)
              return;
            AnimationController controller = _highlightControllers[at];
            D.assert(!forward || controller != null);
            controller?.animateTo(forward ? 1 : 0, duration: CupertinoSlidingSegmentedControlsUtils._kHighlightAnimationDuration, curve: Curves.ease);
    }
        T _highlighted; 
        public T highlighted{
            set {
                if (_highlighted.Equals(value))
                    return;
                _animateHighlightController(at: value, forward: true);
                _animateHighlightController(at: _highlighted, forward: false);
                _highlighted = value;
            }

        }
        T _pressed;
        public T pressed{
            set {
                if (_pressed.Equals(value))
                    return;

                if (_pressed != null) {
                    _pressControllers[_pressed]?.animateTo(0, duration:    CupertinoSlidingSegmentedControlsUtils._kOpacityAnimationDuration, curve: Curves.ease);
                }
                if (!value.Equals(_highlighted)&& value != null) {
                    _pressControllers[value].animateTo(1, duration:    CupertinoSlidingSegmentedControlsUtils._kOpacityAnimationDuration, curve: Curves.ease);
                }
                _pressed = value;
            }
        }

        public void didChangeSelectedViaGesture() {
            widget.onValueChanged(_highlighted);
        }

        public T indexToKey(int? index) {
            return index == null ? default : keys[index.Value];
        }

        public override Widget build(BuildContext context) {
            WidgetsD.debugCheckHasDirectionality(context);

            switch (Directionality.of(context)) {
              case TextDirection.ltr:
                keys = widget.children.Keys.ToList();
                break;
              case TextDirection.rtl:
                  widget.children.Keys.ToList().Reverse();
                  keys = widget.children.Keys.ToList();
                  break;
            }
            List<Listenable> results = new List<Listenable>();
            results.AddRange(_highlightControllers.Values);
            results.AddRange(_pressControllers.Values);
            return new AnimatedBuilder(
              animation: ListenableUtils.merge(results),
              builder: (BuildContext context1, Widget child1) =>{
                List<Widget> children = new List<Widget>();
                foreach( T currentKey in keys){
                  TextStyle textStyle = DefaultTextStyle.of(context1).style.copyWith(
                    fontWeight: _highlightTween.evaluate(_highlightControllers[currentKey])
                  );

                  Widget child = new DefaultTextStyle(
                    style: textStyle,
                    child: 
                    new Opacity(
                        opacity: _pressTween.evaluate(_pressControllers[currentKey]),
                        child: new MetaData(
                            behavior: HitTestBehavior.opaque,
                            child: new Center(child: widget.children[currentKey])
                        )
                    )
                  );

                  children.Add(child);
                }

                int selectedIndex = widget.groupValue.Equals(default) ? 0 : keys.IndexOf(widget.groupValue);

                Widget box = new _SlidingSegmentedControlRenderWidget<T>(
                  children: children,
                  selectedIndex: selectedIndex,
                  thumbColor: CupertinoDynamicColor.resolve(widget.thumbColor, context),
                  state: this
                );
                return new UnconstrainedBox(
                  constrainedAxis: Axis.horizontal,
                  child: new Container(
                    padding: widget.padding.resolve(Directionality.of(context)),
                    decoration: new BoxDecoration(
                      borderRadius: BorderRadius.all(Radius.circular(CupertinoSlidingSegmentedControlsUtils._kCornerRadius)),
                      color: CupertinoDynamicColor.resolve(widget.backgroundColor, context)
                    ),
                    child: box
                  )
                );
              }
            );
          }
    }
    public class _SlidingSegmentedControlRenderWidget<T> : MultiChildRenderObjectWidget { 
        public _SlidingSegmentedControlRenderWidget(
            Key key = null,
            List<Widget> children = null,
            int? selectedIndex = null,
            Color thumbColor = null,
            _SlidingSegmentedControlState<T> state = null
          ) : base(key: key, children: children ?? new List<Widget>()) {
            this.selectedIndex = selectedIndex;
            this.thumbColor = thumbColor;
            this.state = state;
        }

        public readonly int? selectedIndex;
        public readonly Color thumbColor;
        public readonly _SlidingSegmentedControlState<T> state;
        public override RenderObject createRenderObject(BuildContext context) {
            return new _RenderSlidingSegmentedControl<T>(
              selectedIndex: selectedIndex,
              thumbColor: CupertinoDynamicColor.resolve(thumbColor, context),
              state: state
            );
        }
        public override void updateRenderObject(BuildContext context,RenderObject renderObject) {
            renderObject = (_RenderSlidingSegmentedControl<T>) renderObject;
            ((_RenderSlidingSegmentedControl<T>)renderObject).thumbColor = CupertinoDynamicColor.resolve(thumbColor, context);
            ((_RenderSlidingSegmentedControl<T>)renderObject).guardedSetHighlightedIndex(selectedIndex ?? 0);
        }
    }
    public class _ChildAnimationManifest { 
        public _ChildAnimationManifest(
            float opacity = 1f,
            float separatorOpacity = 0f
            ){
                separatorTween = new FloatTween(begin: separatorOpacity, end: separatorOpacity);
                opacityTween = new FloatTween(begin: opacity, end: opacity);
            }
        
        float opacity; 
        Tween<float> opacityTween;
        public float separatorOpacity;
        public Tween<float> separatorTween;
    }
    class _SlidingSegmentedControlContainerBoxParentData : ContainerBoxParentData<RenderBox> { }

    public class _RenderSlidingSegmentedControl<T> : RenderBoxContainerDefaultsMixinContainerRenderObjectMixinRenderBox<RenderBox, ContainerBoxParentData<RenderBox>> { 
        public _RenderSlidingSegmentedControl(
            int? selectedIndex = null,
            Color thumbColor = null,
            _SlidingSegmentedControlState<T> state = null 
          ) { 
            D.assert(state != null);
            _highlightedIndex = selectedIndex;
            _thumbColor = thumbColor;
            this.state = state;
            this.state.drag.onDown = _onDown;
            this.state.drag.onUpdate = _onUpdate;
            this.state.drag.onEnd = _onEnd;
            this.state.drag.onCancel = _onCancel;
            this.state.tap.onTapUp = _onTapUp;
            this.state.longPress.onLongPress = ()=> { };
        }
        public _SlidingSegmentedControlState<T> state;
        Dictionary<RenderBox, _ChildAnimationManifest> _childAnimations = new Dictionary<RenderBox, _ChildAnimationManifest>{};

  
  Rect currentThumbRect;

  Tween<Rect> _currentThumbTween;

  Tween<float> _thumbScaleTween = new FloatTween(begin:    CupertinoSlidingSegmentedControlsUtils._kMinThumbScale, end: 1);
  float currentThumbScale = 1;

  
  Offset _localDragOffset;
  
  bool _startedOnSelectedSegment;

  public override void insert(RenderBox child, RenderBox after  = null) {
    base.insert(child, after: after);
    if (_childAnimations == null)
      return;
    D.assert(_childAnimations.getOrDefault(child) == null);
    _childAnimations[child] = new _ChildAnimationManifest(separatorOpacity: 1);
  }

  public override void remove(RenderBox child) {
    base.remove(child);
    _childAnimations?.Remove(child);
  }

  public override void attach(object owner) {
      owner = (PipelineOwner) owner;
      base.attach(owner); 
      state.thumbController.addListener(markNeedsPaint);
        state.thumbScaleController.addListener(markNeedsPaint);
        state.separatorOpacityController.addListener(markNeedsPaint);
  }

  public override void detach() {
    state.thumbController.removeListener(markNeedsPaint);
    state.thumbScaleController.removeListener(markNeedsPaint);
    state.separatorOpacityController.removeListener(markNeedsPaint);
    base.detach();
  }

  
  bool _needsThumbAnimationUpdate = false;

  int? highlightedIndex {
      get {
          return _highlightedIndex;
      }
      set {
          if (_highlightedIndex == value) {
              return;
          }

          _needsThumbAnimationUpdate = true;
          _highlightedIndex = value;

          state.thumbController.animateWith(CupertinoSlidingSegmentedControlsUtils._kThumbSpringAnimationSimulation);

          state.separatorOpacityController.reset();
          state.separatorOpacityController.animateTo(
              1,
              duration:    CupertinoSlidingSegmentedControlsUtils._kSpringAnimationDuration,
              curve: Curves.ease
          );

          state.highlighted = state.indexToKey(value??0);
          markNeedsPaint();
          //markNeedsSemanticsUpdate();
      }
  }
  int? _highlightedIndex;


  public void guardedSetHighlightedIndex(int value) {
    // Ignore set highlightedIndex when the user is dragging the thumb around.
    if (_startedOnSelectedSegment == true)
      return;
    highlightedIndex = value;
  }

  int? pressedIndex {
      get {
          return _pressedIndex;
      }
      set {
          if (_pressedIndex == value) {
              return;
          }

          D.assert(value == null || (value >= 0 && value < childCount));

          _pressedIndex = value;
          state.pressed = state.indexToKey(value ?? 0);
      }
  }
  int? _pressedIndex;


  public Color thumbColor {
      get {
          return _thumbColor;
      }
      set {
          if (_thumbColor == value) {
              return;
          }
          _thumbColor = value;
          markNeedsPaint(); 
      }
  }
  Color _thumbColor;

  float totalSeparatorWidth {
      get {
          return (CupertinoSlidingSegmentedControlsUtils._kSeparatorInset.horizontal +    CupertinoSlidingSegmentedControlsUtils._kSeparatorWidth) * (childCount - 1);
      }
  }

  public override void handleEvent(PointerEvent _event, HitTestEntry entry) {
      entry = (BoxHitTestEntry) entry;
     
    D.assert(debugHandleEvent(_event, entry));
    if (_event is PointerDownEvent) {
      state.tap.addPointer((PointerDownEvent)_event);
      state.longPress.addPointer((PointerDownEvent)_event);
      state.drag.addPointer((PointerDownEvent)_event);
    }
  }

  int? indexFromLocation(Offset location) {
    return childCount == 0
      ? (int?) null
      // This assumes all children have the same width.
      : (int)((location.dx / (size.width / childCount))
        .floor()
        .clamp(0, childCount - 1) );
  }

  void _onTapUp(TapUpDetails details) {
    highlightedIndex = indexFromLocation(details.localPosition);
    state.didChangeSelectedViaGesture();
  }

  void _onDown(DragDownDetails details) {
    D.assert(size.contains(details.localPosition));
    _localDragOffset = details.localPosition;
    int? index = indexFromLocation(_localDragOffset);
    _startedOnSelectedSegment = index == highlightedIndex;
    pressedIndex = index;

    if (_startedOnSelectedSegment) {
      _playThumbScaleAnimation(isExpanding: false);
    }
  }

  void _onUpdate(DragUpdateDetails details) {
    _localDragOffset = details.localPosition;
    int? newIndex = indexFromLocation(_localDragOffset);

    if (_startedOnSelectedSegment) {
      highlightedIndex = newIndex;
      pressedIndex = newIndex;
    } else {
      pressedIndex = _hasDraggedTooFar(details) ? 0 : newIndex;
    }
  }

  void _onEnd(DragEndDetails details) {
    if (_startedOnSelectedSegment) {
      _playThumbScaleAnimation(isExpanding: true);
      state.didChangeSelectedViaGesture();
    }

    if (pressedIndex != null) {
      highlightedIndex = pressedIndex;
      state.didChangeSelectedViaGesture();
    }
    pressedIndex = null;
    _localDragOffset = null;
    _startedOnSelectedSegment = false;
  }

  void _onCancel() {
    if (_startedOnSelectedSegment) {
      _playThumbScaleAnimation(isExpanding: true);
    }

    _localDragOffset = null;
    pressedIndex = null;
    _startedOnSelectedSegment = false;
  }

  void _playThumbScaleAnimation(bool isExpanding = false) {
      _thumbScaleTween = new FloatTween(begin: currentThumbScale, end: isExpanding ? 1 : CupertinoSlidingSegmentedControlsUtils._kMinThumbScale);
    state.thumbScaleController.animateWith(CupertinoSlidingSegmentedControlsUtils._kThumbSpringAnimationSimulation);
  }

  bool _hasDraggedTooFar(DragUpdateDetails details) {
    Offset offCenter = details.localPosition - new Offset(size.width/2, size.height/2);
    return Mathf.Pow(Mathf.Max(0, offCenter.dx.abs() - size.width/2), 2) + Mathf.Pow(Mathf.Max(0, offCenter.dy.abs() - size.height/2), 2) > CupertinoSlidingSegmentedControlsUtils._kTouchYDistanceThreshold;
  }

  protected internal override float computeMinIntrinsicWidth(float height) {
    RenderBox child = firstChild;
    float maxMinChildWidth = 0;
    while (child != null) {
      _SlidingSegmentedControlContainerBoxParentData childParentData =
        child.parentData as _SlidingSegmentedControlContainerBoxParentData;
      float childWidth = child.getMinIntrinsicWidth(height);
      maxMinChildWidth = Math.Max(maxMinChildWidth, childWidth);
      child = childParentData.nextSibling;
    }
    return (maxMinChildWidth + 2 *    CupertinoSlidingSegmentedControlsUtils._kSegmentMinPadding) * childCount + totalSeparatorWidth;
  }

  protected internal override float computeMaxIntrinsicWidth(float height) {
    RenderBox child = firstChild;
    float maxMaxChildWidth = 0;
    while (child != null) {
       _SlidingSegmentedControlContainerBoxParentData childParentData =
        child.parentData as _SlidingSegmentedControlContainerBoxParentData;
      float childWidth = child.getMaxIntrinsicWidth(height);
      maxMaxChildWidth = Mathf.Max(maxMaxChildWidth, childWidth);
      child = childParentData.nextSibling;
    }
    return (maxMaxChildWidth + 2 *    CupertinoSlidingSegmentedControlsUtils._kSegmentMinPadding) * childCount + totalSeparatorWidth;
  }

  protected internal override float computeMinIntrinsicHeight(float width) {
    RenderBox child = firstChild;
    float maxMinChildHeight = 0;
    while (child != null) {
      _SlidingSegmentedControlContainerBoxParentData childParentData =
        child.parentData as _SlidingSegmentedControlContainerBoxParentData;
      float childHeight = child.getMinIntrinsicHeight(width);
      maxMinChildHeight = Mathf.Max(maxMinChildHeight, childHeight);
      child = childParentData.nextSibling;
    }
    return maxMinChildHeight;
  }

  protected internal override float computeMaxIntrinsicHeight(float width) {
    RenderBox child = firstChild;
    float maxMaxChildHeight = 0;
    while (child != null) {
       _SlidingSegmentedControlContainerBoxParentData childParentData =
        child.parentData as _SlidingSegmentedControlContainerBoxParentData;
      float childHeight = child.getMaxIntrinsicHeight(width);
      maxMaxChildHeight = Mathf.Max(maxMaxChildHeight, childHeight);
      child = childParentData.nextSibling;
    }
    return maxMaxChildHeight;
  }

  public override float? computeDistanceToActualBaseline(TextBaseline baseline) {
    return defaultComputeDistanceToHighestActualBaseline(baseline);
  }

  public override void setupParentData(RenderObject child) {
      child = (RenderBox) child; 
    if (!(child.parentData is _SlidingSegmentedControlContainerBoxParentData)) {
      child.parentData = new _SlidingSegmentedControlContainerBoxParentData();
    }
  }

  protected override void performLayout() {
    BoxConstraints constraints = this.constraints;
    float childWidth = (constraints.minWidth - totalSeparatorWidth) / childCount;
    float maxHeight =    CupertinoSlidingSegmentedControlsUtils._kMinSegmentedControlHeight;

    foreach( RenderBox child1 in getChildrenAsList()) {
      childWidth = Mathf.Max(childWidth, child1.getMaxIntrinsicWidth(float.PositiveInfinity) + 2 * CupertinoSlidingSegmentedControlsUtils._kSegmentMinPadding);
    }

    childWidth = Mathf.Min(
      childWidth,
      (constraints.maxWidth - totalSeparatorWidth) / childCount
    );

    RenderBox child = firstChild;
    while (child != null) {
      float boxHeight = child.getMaxIntrinsicHeight(childWidth);
      maxHeight = Mathf.Max(maxHeight, boxHeight);
      child = childAfter(child);
    }

    constraints.constrainHeight(maxHeight);

    BoxConstraints childConstraints = BoxConstraints.tightFor(
      width: childWidth,
      height: maxHeight
    );

    // Layout children.
    child = firstChild;
    while (child != null) {
      child.layout(childConstraints, parentUsesSize: true);
      child = childAfter(child);
    }

    float start = 0;
    child = firstChild;

    while (child != null) {
      _SlidingSegmentedControlContainerBoxParentData childParentData =
        child.parentData as _SlidingSegmentedControlContainerBoxParentData;
       Offset childOffset = new Offset(start, 0);
      childParentData.offset = childOffset;
      start += child.size.width +    CupertinoSlidingSegmentedControlsUtils._kSeparatorWidth +    CupertinoSlidingSegmentedControlsUtils._kSeparatorInset.horizontal;
      child = childAfter(child);
    }

    size = constraints.constrain(new Size(childWidth * childCount + totalSeparatorWidth, maxHeight));
  }

  public override void paint(PaintingContext context, Offset offset) {
    List<RenderBox> children = getChildrenAsList();

    // Paint thumb if highlightedIndex is not null.
    if (highlightedIndex != null) {
      if (_childAnimations == null) {
        _childAnimations = new Dictionary<RenderBox, _ChildAnimationManifest>();//<RenderBox, _ChildAnimationManifest> { };
        for (int i = 0; i < childCount - 1; i += 1) {
            bool shouldFadeOut = i == highlightedIndex || i == highlightedIndex - 1;
            RenderBox child = children[i];
            _childAnimations[child] = new _ChildAnimationManifest(separatorOpacity: shouldFadeOut ? 0 : 1);
        }
      }

      RenderBox selectedChild = children[highlightedIndex ?? 0];

      _SlidingSegmentedControlContainerBoxParentData childParentData =
        selectedChild.parentData as _SlidingSegmentedControlContainerBoxParentData;
      Rect unscaledThumbTargetRect =    CupertinoSlidingSegmentedControlsUtils._kThumbInsets.inflateRect(childParentData.offset & selectedChild.size);

      // Update related Tweens before animation update phase.
      if (_needsThumbAnimationUpdate) {
        // Needs to ensure _currentThumbRect is valid.
        _currentThumbTween = new RectTween(begin: currentThumbRect ?? unscaledThumbTargetRect, end: unscaledThumbTargetRect);

        for (int i = 0; i < childCount - 1; i += 1) {
         
          bool shouldFadeOut = i == highlightedIndex || i == highlightedIndex - 1;
          RenderBox child = children[i];
          _ChildAnimationManifest manifest = _childAnimations[child];
          D.assert(manifest != null);
          manifest.separatorTween = new FloatTween(
            begin: manifest.separatorOpacity,
            end: shouldFadeOut ? 0 : 1
          );
        }

        _needsThumbAnimationUpdate = false;
      } else if (_currentThumbTween != null && unscaledThumbTargetRect != _currentThumbTween.begin) {
        _currentThumbTween = new RectTween(begin: _currentThumbTween.begin, end: unscaledThumbTargetRect);
      }

      for (int index = 0; index < childCount - 1; index += 1) {
        _paintSeparator(context, offset, children[index]);
      }

      currentThumbRect = _currentThumbTween?.evaluate(state.thumbController)
                        ?? unscaledThumbTargetRect;

      currentThumbScale = _thumbScaleTween.evaluate(state.thumbScaleController);

      Rect thumbRect = Rect.fromCenter(
        center: currentThumbRect.center,
        width: currentThumbRect.width * currentThumbScale,
        height: currentThumbRect.height * currentThumbScale
      );

      _paintThumb(context, offset, thumbRect);
    } else {
      // Reset all animations when there"s no thumb.
      currentThumbRect = null;
      _childAnimations = null;

      for (int index = 0; index < childCount - 1; index += 1) {
        _paintSeparator(context, offset, children[index]);
      }
    }

    for (int index = 0; index < children.Count; index++) {
      _paintChild(context, offset, children[index], index);
    }
  }

  // Paint the separator to the right of the given child.
  void _paintSeparator(PaintingContext context, Offset offset, RenderBox child) {
    D.assert(child != null);
    _SlidingSegmentedControlContainerBoxParentData childParentData =
      child.parentData as _SlidingSegmentedControlContainerBoxParentData;

    Paint paint = new Paint();

    _ChildAnimationManifest manifest = _childAnimations == null ? null : _childAnimations[child];
    float opacity = manifest?.separatorTween?.evaluate(state.separatorOpacityController) ?? 1;
    if(manifest != null)
        manifest.separatorOpacity = opacity;
    paint.color = CupertinoSlidingSegmentedControlsUtils._kSeparatorColor.withOpacity(CupertinoSlidingSegmentedControlsUtils._kSeparatorColor.opacity * opacity);

    Rect childRect = (childParentData.offset + offset) & child.size;
    Rect separatorRect = CupertinoSlidingSegmentedControlsUtils._kSeparatorInset.deflateRect(
      childRect.topRight & new Size(CupertinoSlidingSegmentedControlsUtils._kSeparatorInset.horizontal + CupertinoSlidingSegmentedControlsUtils._kSeparatorWidth,
          child.size.height)
    );

    context.canvas.drawRRect(
      RRect.fromRectAndRadius(separatorRect,    CupertinoSlidingSegmentedControlsUtils._kSeparatorRadius),
      paint
    );
  }

  void _paintChild(PaintingContext context, Offset offset, RenderBox child, int childIndex) {
    D.assert(child != null);
    _SlidingSegmentedControlContainerBoxParentData childParentData =
      child.parentData as _SlidingSegmentedControlContainerBoxParentData;
    context.paintChild(child, childParentData.offset + offset);
  }

  void _paintThumb(PaintingContext context, Offset offset, Rect thumbRect) {
    List<BoxShadow> thumbShadow = new List<BoxShadow> {
      new BoxShadow(
        color: new Color(0x1F000000),
        offset: new Offset(0, 3),
        blurRadius: 8
      ),
      new BoxShadow(
        color: new Color(0x0A000000),
        offset: new Offset(0, 3),
        blurRadius: 1
      )
    };

   RRect thumbRRect = RRect.fromRectAndRadius(thumbRect.shift(offset), CupertinoSlidingSegmentedControlsUtils._kThumbRadius);

    foreach( BoxShadow shadow in thumbShadow) {
      context.canvas.drawRRect(thumbRRect.shift(shadow.offset), shadow.toPaint());
    }

    context.canvas.drawRRect(
      thumbRRect.inflate(0.5f),
      new Paint(){color = new Color(0x0A000000)}
    );

    context.canvas.drawRRect(
      thumbRRect,
        new Paint(){color = thumbColor}
      
    );
  }

  protected override bool hitTestChildren(BoxHitTestResult result, Offset position ) {
    D.assert(position != null);
    RenderBox child = lastChild;
    while (child != null) {
      _SlidingSegmentedControlContainerBoxParentData childParentData =
        child.parentData as _SlidingSegmentedControlContainerBoxParentData;
      if ((childParentData.offset & child.size).contains(position)) {
         Offset center = (Offset.zero & child.size).center;
        return result.addWithRawTransform(
          transform: MatrixUtils.forceToPoint(center),
          position: center,
          hitTest: (BoxHitTestResult result1, Offset position1)=> {
            D.assert(position1 == center);
            return child.hitTest(result1, position: center);
          }
        );
      }
      child = childParentData.previousSibling;
    }
    return false;
  }
}

    
}