using System;
using System.Collections.Generic;
using Unity.UIWidgets.animation;
using Unity.UIWidgets.async;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.gestures;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.rendering;
using Unity.UIWidgets.scheduler;
using Unity.UIWidgets.ui;
using Unity.UIWidgets.widgets;
using UnityEngine;
using Color = Unity.UIWidgets.ui.Color;
using Rect = Unity.UIWidgets.ui.Rect;

namespace Unity.UIWidgets.material {
    public class Slider : StatefulWidget {
        public Slider(
            Key key = null,
            float? value = null,
            ValueChanged<float> onChanged = null,
            ValueChanged<float> onChangeStart = null,
            ValueChanged<float> onChangeEnd = null,
            float min = 0.0f,
            float max = 1.0f,
            int? divisions = null,
            string label = null,
            Color activeColor = null,
            Color inactiveColor = null
        ) : base(key: key) {
            D.assert(value != null);
            D.assert(min <= max);
            D.assert(value >= min && value <= max);
            D.assert(divisions == null || divisions > 0);
            this.value = value.Value;
            this.onChanged = onChanged;
            this.onChangeStart = onChangeStart;
            this.onChangeEnd = onChangeEnd;
            this.min = min;
            this.max = max;
            this.divisions = divisions;
            this.label = label;
            this.activeColor = activeColor;
            this.inactiveColor = inactiveColor;
        }

        public readonly float value;

        public readonly ValueChanged<float> onChanged;

        public readonly ValueChanged<float> onChangeStart;

        public readonly ValueChanged<float> onChangeEnd;

        public readonly float min;

        public readonly float max;

        public readonly int? divisions;

        public readonly string label;

        public readonly Color activeColor;

        public readonly Color inactiveColor;

        public override State createState() {
            return new _SliderState();
        }


        public override void debugFillProperties(DiagnosticPropertiesBuilder properties) {
            base.debugFillProperties(properties);
            properties.add(new FloatProperty("value", value));
            properties.add(new FloatProperty("min", min));
            properties.add(new FloatProperty("max", max));
        }
    }


    class _SliderState : TickerProviderStateMixin<Slider> {
        static TimeSpan enableAnimationDuration = new TimeSpan(0, 0, 0, 0, 75);
        static TimeSpan valueIndicatorAnimationDuration = new TimeSpan(0, 0, 0, 0, 100);

        public AnimationController overlayController;
        public AnimationController valueIndicatorController;
        public AnimationController enableController;
        public AnimationController positionController;
        public Timer interactionTimer;

        public override void initState() {
            base.initState();
            overlayController = new AnimationController(
                duration: Constants.kRadialReactionDuration,
                vsync: this
            );
            valueIndicatorController = new AnimationController(
                duration: valueIndicatorAnimationDuration,
                vsync: this
            );
            enableController = new AnimationController(
                duration: enableAnimationDuration,
                vsync: this
            );
            positionController = new AnimationController(
                duration: TimeSpan.Zero,
                vsync: this
            );
            enableController.setValue(widget.onChanged != null ? 1.0f : 0.0f);
            positionController.setValue(_unlerp(widget.value));
        }

        public override void dispose() {
            interactionTimer?.cancel();
            overlayController.dispose();
            valueIndicatorController.dispose();
            enableController.dispose();
            positionController.dispose();
            base.dispose();
        }

        void _handleChanged(float value) {
            D.assert(widget.onChanged != null);
            float lerpValue = _lerp(value);
            if (lerpValue != widget.value) {
                widget.onChanged(lerpValue);
            }
        }

        void _handleDragStart(float value) {
            D.assert(widget.onChangeStart != null);
            widget.onChangeStart(_lerp(value));
        }

        void _handleDragEnd(float value) {
            D.assert(widget.onChangeEnd != null);
            widget.onChangeEnd(_lerp(value));
        }

        float _lerp(float value) {
            D.assert(value >= 0.0f);
            D.assert(value <= 1.0f);
            return value * (widget.max - widget.min) + widget.min;
        }

        float _unlerp(float value) {
            D.assert(value <= widget.max);
            D.assert(value >= widget.min);
            return widget.max > widget.min
                ? (value - widget.min) / (widget.max - widget.min)
                : 0.0f;
        }

        public override Widget build(BuildContext context) {
            D.assert(MaterialD.debugCheckHasMaterial(context));
            D.assert(WidgetsD.debugCheckHasMediaQuery(context));

            SliderThemeData sliderTheme = SliderTheme.of(context);

            if (widget.activeColor != null || widget.inactiveColor != null) {
                sliderTheme = sliderTheme.copyWith(
                    activeTrackColor: widget.activeColor,
                    inactiveTrackColor: widget.inactiveColor,
                    activeTickMarkColor: widget.inactiveColor,
                    inactiveTickMarkColor: widget.activeColor,
                    thumbColor: widget.activeColor,
                    valueIndicatorColor: widget.activeColor,
                    overlayColor: widget.activeColor?.withAlpha(0x29)
                );
            }

            return new _SliderRenderObjectWidget(
                value: _unlerp(widget.value),
                divisions: widget.divisions,
                label: widget.label,
                sliderTheme: sliderTheme,
                mediaQueryData: MediaQuery.of(context),
                onChanged: (widget.onChanged != null) && (widget.max > widget.min)
                    ? _handleChanged
                    : (ValueChanged<float>) null,
                onChangeStart: widget.onChangeStart != null ? _handleDragStart : (ValueChanged<float>) null,
                onChangeEnd: widget.onChangeEnd != null ? _handleDragEnd : (ValueChanged<float>) null,
                state: this
            );
        }
    }

    class _SliderRenderObjectWidget : LeafRenderObjectWidget {
        public _SliderRenderObjectWidget(
            Key key = null,
            float? value = null,
            int? divisions = null,
            string label = null,
            SliderThemeData sliderTheme = null,
            MediaQueryData mediaQueryData = null,
            ValueChanged<float> onChanged = null,
            ValueChanged<float> onChangeStart = null,
            ValueChanged<float> onChangeEnd = null,
            _SliderState state = null
        ) : base(key: key) {
            this.value = value.Value;
            this.divisions = divisions;
            this.label = label;
            this.sliderTheme = sliderTheme;
            this.mediaQueryData = mediaQueryData;
            this.onChanged = onChanged;
            this.onChangeStart = onChangeStart;
            this.onChangeEnd = onChangeEnd;
            this.state = state;
        }


        public readonly float value;
        public readonly int? divisions;
        public readonly string label;
        public readonly SliderThemeData sliderTheme;
        public readonly MediaQueryData mediaQueryData;
        public readonly ValueChanged<float> onChanged;
        public readonly ValueChanged<float> onChangeStart;
        public readonly ValueChanged<float> onChangeEnd;
        public readonly _SliderState state;

        public override RenderObject createRenderObject(BuildContext context) {
            return new _RenderSlider(
                value: value,
                divisions: divisions,
                label: label,
                sliderTheme: sliderTheme,
                theme: Theme.of(context),
                mediaQueryData: mediaQueryData,
                onChanged: onChanged,
                onChangeStart: onChangeStart,
                onChangeEnd: onChangeEnd,
                state: state,
                platform: Theme.of(context).platform
            );
        }


        public override void updateRenderObject(BuildContext context, RenderObject renderObject) {
            _RenderSlider _renderObject = (_RenderSlider) renderObject;
            _renderObject.value = value;
            _renderObject.divisions = divisions;
            _renderObject.label = label;
            _renderObject.sliderTheme = sliderTheme;
            _renderObject.theme = Theme.of(context);
            _renderObject.mediaQueryData = mediaQueryData;
            _renderObject.onChanged = onChanged;
            _renderObject.onChangeStart = onChangeStart;
            _renderObject.onChangeEnd = onChangeEnd;
            _renderObject.platform = Theme.of(context).platform;
        }
    }

    class _RenderSlider : RenderBox {
        static float _positionAnimationDurationMilliSeconds = 75;
        static float _minimumInteractionTimeMilliSeconds = 500;

        const float _minPreferredTrackWidth = 144.0f;

        public _RenderSlider(
            float? value = null,
            int? divisions = null,
            string label = null,
            SliderThemeData sliderTheme = null,
            ThemeData theme = null,
            MediaQueryData mediaQueryData = null,
            RuntimePlatform? platform = null,
            ValueChanged<float> onChanged = null,
            ValueChanged<float> onChangeStart = null,
            ValueChanged<float> onChangeEnd = null,
            _SliderState state = null
        ) {
            D.assert(value != null && value >= 0.0 && value <= 1.0);
            D.assert(state != null);
            this.onChangeStart = onChangeStart;
            this.onChangeEnd = onChangeEnd;
            _platform = platform;
            _label = label;
            _value = value.Value;
            _divisions = divisions;
            _sliderTheme = sliderTheme;
            _theme = theme;
            _mediaQueryData = mediaQueryData;
            _onChanged = onChanged;
            _state = state;

            _updateLabelPainter();
            GestureArenaTeam team = new GestureArenaTeam();
            _drag = new HorizontalDragGestureRecognizer {
                team = team,
                onStart = _handleDragStart,
                onUpdate = _handleDragUpdate,
                onEnd = _handleDragEnd,
                onCancel = _endInteraction
            };

            _tap = new TapGestureRecognizer {
                team = team,
                onTapDown = _handleTapDown,
                onTapUp = _handleTapUp,
                onTapCancel = _endInteraction
            };

            _overlayAnimation = new CurvedAnimation(
                parent: _state.overlayController,
                curve: Curves.fastOutSlowIn);

            _valueIndicatorAnimation = new CurvedAnimation(
                parent: _state.valueIndicatorController,
                curve: Curves.fastOutSlowIn);

            _enableAnimation = new CurvedAnimation(
                parent: _state.enableController,
                curve: Curves.easeInOut);
        }

        float _maxSliderPartWidth {
            get {
                float maxValue = 0;
                foreach (Size size in _sliderPartSizes) {
                    if (size.width > maxValue) {
                        maxValue = size.width;
                    }
                }

                return maxValue;
            }
        }

        float _maxSliderPartHeight {
            get {
                float maxValue = 0;
                foreach (Size size in _sliderPartSizes) {
                    if (size.width > maxValue) {
                        maxValue = size.width;
                    }
                }

                return maxValue;
            }
        }

        List<Size> _sliderPartSizes {
            get {
                return new List<Size> {
                    _sliderTheme.overlayShape.getPreferredSize(isInteractive, isDiscrete),
                    _sliderTheme.thumbShape.getPreferredSize(isInteractive, isDiscrete),
                    _sliderTheme.tickMarkShape.getPreferredSize(isEnabled: isInteractive,
                        sliderTheme: sliderTheme)
                };
            }
        }

        float _minPreferredTrackHeight {
            get { return _sliderTheme.trackHeight; }
        }

        _SliderState _state;
        Animation<float> _overlayAnimation;
        Animation<float> _valueIndicatorAnimation;
        Animation<float> _enableAnimation;
        TextPainter _labelPainter = new TextPainter();
        HorizontalDragGestureRecognizer _drag;
        TapGestureRecognizer _tap;
        bool _active = false;
        float _currentDragValue = 0.0f;

        Rect _trackRect {
            get {
                return _sliderTheme.trackShape.getPreferredRect(
                    parentBox: this,
                    offset: Offset.zero,
                    sliderTheme: _sliderTheme,
                    isDiscrete: false
                );
            }
        }

        bool isInteractive {
            get { return onChanged != null; }
        }

        bool isDiscrete {
            get { return divisions != null && divisions.Value > 0; }
        }

        public float value {
            get { return _value; }
            set {
                D.assert(value >= 0.0f && value <= 1.0f);
                float convertedValue = isDiscrete ? _discretize(value) : value;
                if (convertedValue == _value) {
                    return;
                }

                _value = convertedValue;
                if (isDiscrete) {
                    float distance = (_value - _state.positionController.value).abs();
                    _state.positionController.duration = distance != 0.0f
                        ? new TimeSpan(0, 0, 0, 0, (int) (_positionAnimationDurationMilliSeconds * (1.0f / distance)))
                        : TimeSpan.Zero;
                    _state.positionController.animateTo(convertedValue, curve: Curves.easeInOut);
                }
                else {
                    _state.positionController.setValue(convertedValue);
                }
            }
        }

        float _value;

        public RuntimePlatform? platform {
            get { return _platform; }
            set {
                if (_platform == value) {
                    return;
                }

                _platform = value;
            }
        }

        RuntimePlatform? _platform;

        public int? divisions {
            get { return _divisions; }
            set {
                if (value == _divisions) {
                    return;
                }

                _divisions = value;
                markNeedsPaint();
            }
        }

        int? _divisions;

        public string label {
            get { return _label; }
            set {
                if (value == _label) {
                    return;
                }

                _label = value;
                _updateLabelPainter();
            }
        }

        string _label;

        public SliderThemeData sliderTheme {
            get { return _sliderTheme; }
            set {
                if (value == _sliderTheme) {
                    return;
                }

                _sliderTheme = value;
                markNeedsPaint();
            }
        }

        SliderThemeData _sliderTheme;

        public ThemeData theme {
            get { return _theme; }
            set {
                if (value == _theme) {
                    return;
                }

                _theme = value;
                markNeedsPaint();
            }
        }

        ThemeData _theme;

        public MediaQueryData mediaQueryData {
            get { return _mediaQueryData; }
            set {
                if (value == _mediaQueryData) {
                    return;
                }

                _mediaQueryData = value;
                _updateLabelPainter();
            }
        }

        MediaQueryData _mediaQueryData;

        public ValueChanged<float> onChanged {
            get { return _onChanged; }
            set {
                if (value == _onChanged) {
                    return;
                }

                bool wasInteractive = isInteractive;
                _onChanged = value;
                if (wasInteractive != isInteractive) {
                    if (isInteractive) {
                        _state.enableController.forward();
                    }
                    else {
                        _state.enableController.reverse();
                    }

                    markNeedsPaint();
                }
            }
        }

        ValueChanged<float> _onChanged;

        public ValueChanged<float> onChangeStart;
        public ValueChanged<float> onChangeEnd;

        public bool showValueIndicator {
            get {
                bool showValueIndicator = false;
                switch (_sliderTheme.showValueIndicator) {
                    case ShowValueIndicator.onlyForDiscrete:
                        showValueIndicator = isDiscrete;
                        break;
                    case ShowValueIndicator.onlyForContinuous:
                        showValueIndicator = !isDiscrete;
                        break;
                    case ShowValueIndicator.always:
                        showValueIndicator = true;
                        break;
                    case ShowValueIndicator.never:
                        showValueIndicator = false;
                        break;
                }

                return showValueIndicator;
            }
        }

        float _adjustmentUnit {
            get {
                switch (_platform) {
                    case RuntimePlatform.IPhonePlayer:
                        return 0.1f;
                    default:
                        return 0.05f;
                }
            }
        }


        void _updateLabelPainter() {
            if (label != null) {
                _labelPainter.text = new TextSpan(
                    style: _sliderTheme.valueIndicatorTextStyle,
                    text: label
                );
                _labelPainter.textScaleFactor = _mediaQueryData.textScaleFactor;
                _labelPainter.layout();
            }
            else {
                _labelPainter.text = null;
            }

            markNeedsLayout();
        }

        public override void attach(object owner) {
            base.attach(owner);
            _overlayAnimation.addListener(markNeedsPaint);
            _valueIndicatorAnimation.addListener(markNeedsPaint);
            _enableAnimation.addListener(markNeedsPaint);
            _state.positionController.addListener(markNeedsPaint);
        }

        public override void detach() {
            _overlayAnimation.removeListener(markNeedsPaint);
            _valueIndicatorAnimation.removeListener(markNeedsPaint);
            _enableAnimation.removeListener(markNeedsPaint);
            _state.positionController.removeListener(markNeedsPaint);
            base.detach();
        }

        float _getValueFromVisualPosition(float visualPosition) {
            return visualPosition;
        }

        float _getValueFromGlobalPosition(Offset globalPosition) {
            float visualPosition =
                (globalToLocal(globalPosition).dx - _trackRect.left) / _trackRect.width;
            return _getValueFromVisualPosition(visualPosition);
        }

        float _discretize(float value) {
            float result = value.clamp(0.0f, 1.0f);
            if (isDiscrete) {
                result = (result * divisions.Value).round() * 1.0f / divisions.Value;
            }

            return result;
        }

        void _startInteraction(Offset globalPosition) {
            if (isInteractive) {
                _active = true;

                if (onChangeStart != null) {
                    onChangeStart(_discretize(value));
                }

                _currentDragValue = _getValueFromGlobalPosition(globalPosition);
                onChanged(_discretize(_currentDragValue));
                _state.overlayController.forward();
                if (showValueIndicator) {
                    _state.valueIndicatorController.forward();
                    _state.interactionTimer?.cancel();
                    _state.interactionTimer = Window.instance.run(
                        new TimeSpan(0, 0, 0, 0,
                            (int) (_minimumInteractionTimeMilliSeconds * SchedulerBinding.instance.timeDilation)),
                        () => {
                            _state.interactionTimer = null;
                            if (!_active &&
                                _state.valueIndicatorController.status == AnimationStatus.completed) {
                                _state.valueIndicatorController.reverse();
                            }
                        }
                    );
                }
            }
        }

        void _endInteraction() {
            if (_active && _state.mounted) {
                if (onChangeEnd != null) {
                    onChangeEnd(_discretize(_currentDragValue));
                }

                _active = false;
                _currentDragValue = 0.0f;
                _state.overlayController.reverse();
                if (showValueIndicator && _state.interactionTimer == null) {
                    _state.valueIndicatorController.reverse();
                }
            }
        }

        void _handleDragStart(DragStartDetails details) {
            _startInteraction(details.globalPosition);
        }

        void _handleDragUpdate(DragUpdateDetails details) {
            if (isInteractive) {
                float valueDelta = details.primaryDelta.Value / _trackRect.width;
                _currentDragValue += valueDelta;
                onChanged(_discretize(_currentDragValue));
            }
        }

        void _handleDragEnd(DragEndDetails details) {
            _endInteraction();
        }

        void _handleTapDown(TapDownDetails details) {
            _startInteraction(details.globalPosition);
        }

        void _handleTapUp(TapUpDetails details) {
            _endInteraction();
        }

        protected override bool hitTestSelf(Offset position) {
            return true;
        }

        public override void handleEvent(PointerEvent evt, HitTestEntry entry) {
            D.assert(debugHandleEvent(evt, entry));
            if (evt is PointerDownEvent && isInteractive) {
                _drag.addPointer((PointerDownEvent) evt);
                _tap.addPointer((PointerDownEvent) evt);
            }
        }


        protected override float computeMinIntrinsicWidth(float height) {
            return _minPreferredTrackWidth + _maxSliderPartWidth;
        }

        protected override float computeMaxIntrinsicWidth(float height) {
            return _minPreferredTrackWidth + _maxSliderPartWidth;
        }

        protected override float computeMinIntrinsicHeight(float width) {
            return Mathf.Max(_minPreferredTrackHeight, _maxSliderPartHeight);
        }

        protected internal override float computeMaxIntrinsicHeight(float width) {
            return Mathf.Max(_minPreferredTrackHeight, _maxSliderPartHeight);
        }

        protected override bool sizedByParent {
            get { return true; }
        }

        protected override void performResize() {
            size = new Size(
                constraints.hasBoundedWidth
                    ? constraints.maxWidth
                    : _minPreferredTrackWidth + _maxSliderPartWidth,
                constraints.hasBoundedHeight
                    ? constraints.maxHeight
                    : Mathf.Max(_minPreferredTrackHeight, _maxSliderPartHeight)
            );
        }

        public override void paint(PaintingContext context, Offset offset) {
            float value = _state.positionController.value;
            float visualPosition = value;

            Rect trackRect = _sliderTheme.trackShape.getPreferredRect(
                parentBox: this,
                offset: offset,
                sliderTheme: _sliderTheme,
                isDiscrete: isDiscrete
            );

            Offset thumbCenter = new Offset(trackRect.left + visualPosition * trackRect.width, trackRect.center.dy);

            _sliderTheme.trackShape.paint(
                context,
                offset,
                parentBox: this,
                sliderTheme: _sliderTheme,
                enableAnimation: _enableAnimation,
                thumbCenter: thumbCenter,
                isDiscrete: isDiscrete,
                isEnabled: isInteractive
            );

            if (!_overlayAnimation.isDismissed) {
                _sliderTheme.overlayShape.paint(
                    context,
                    thumbCenter,
                    activationAnimation: _overlayAnimation,
                    enableAnimation: _enableAnimation,
                    isDiscrete: isDiscrete,
                    labelPainter: _labelPainter,
                    parentBox: this,
                    sliderTheme: _sliderTheme,
                    value: _value
                );
            }

            if (isDiscrete) {
                float tickMarkWidth = _sliderTheme.tickMarkShape.getPreferredSize(
                    isEnabled: isInteractive,
                    sliderTheme: _sliderTheme
                ).width;

                float adjustedTrackWidth = trackRect.width - tickMarkWidth;
                if (adjustedTrackWidth / divisions.Value >= 3.0f * tickMarkWidth) {
                    float dy = trackRect.center.dy;
                    for (int i = 0; i <= divisions; i++) {
                        float tickValue = i / divisions.Value;
                        float dx = trackRect.left + tickValue * adjustedTrackWidth + tickMarkWidth / 2;
                        Offset tickMarkOffset = new Offset(dx, dy);
                        _sliderTheme.tickMarkShape.paint(
                            context,
                            tickMarkOffset,
                            parentBox: this,
                            sliderTheme: _sliderTheme,
                            enableAnimation: _enableAnimation,
                            thumbCenter: thumbCenter,
                            isEnabled: isInteractive
                        );
                    }
                }
            }

            if (isInteractive && label != null && !_valueIndicatorAnimation.isDismissed) {
                if (showValueIndicator) {
                    _sliderTheme.valueIndicatorShape.paint(
                        context,
                        thumbCenter,
                        activationAnimation: _valueIndicatorAnimation,
                        enableAnimation: _enableAnimation,
                        isDiscrete: isDiscrete,
                        labelPainter: _labelPainter,
                        parentBox: this,
                        sliderTheme: _sliderTheme,
                        value: _value
                    );
                }
            }

            _sliderTheme.thumbShape.paint(
                context,
                thumbCenter,
                activationAnimation: _valueIndicatorAnimation,
                enableAnimation: _enableAnimation,
                isDiscrete: isDiscrete,
                labelPainter: _labelPainter,
                parentBox: this,
                sliderTheme: _sliderTheme,
                value: _value
            );
        }
    }
}