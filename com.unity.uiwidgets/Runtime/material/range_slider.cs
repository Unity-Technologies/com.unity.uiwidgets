using System;
using System.Collections.Generic;
using System.Linq;
using Unity.UIWidgets.animation;
using Unity.UIWidgets.async;
using Unity.UIWidgets.external;
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
    public class RangeSlider : StatefulWidget {
        public RangeSlider(
            Key key = null,
            RangeValues values = null,
            ValueChanged<RangeValues> onChanged = null,
            ValueChanged<RangeValues> onChangeStart = null,
            ValueChanged<RangeValues> onChangeEnd = null,
            float min = 0.0f,
            float max = 1.0f,
            int? divisions = null,
            RangeLabels labels = null,
            Color activeColor = null,
            Color inactiveColor = null
        ) : base(key: key) {
            D.assert(values != null);
            D.assert(min <= max);
            D.assert(values.start <= values.end);
            D.assert(values.start >= min && values.start <= max);
            D.assert(values.end >= min && values.end <= max);
            D.assert(divisions == null || divisions > 0);
            this.values = values;
            this.onChanged = onChanged;
            this.onChangeStart = onChangeStart;
            this.onChangeEnd = onChangeEnd;
            this.min = min;
            this.max = max;
            this.divisions = divisions;
            this.labels = labels;
            this.activeColor = activeColor;
            this.inactiveColor = inactiveColor;
        }

        public readonly RangeValues values;

        public readonly ValueChanged<RangeValues> onChanged;
        
        public readonly ValueChanged<RangeValues> onChangeStart;

        public readonly ValueChanged<RangeValues> onChangeEnd;

        public readonly float min;

        public readonly float max;

        public readonly int? divisions;

        public readonly RangeLabels labels;

        public readonly Color activeColor;

        public readonly Color inactiveColor;

        public const float _minTouchTargetWidth = material_.kMinInteractiveDimension;

        public override State createState() {
            return new _RangeSliderState();
        }

        public override void debugFillProperties(DiagnosticPropertiesBuilder properties) {
            base.debugFillProperties(properties);
            properties.add(new FloatProperty("valueStart", values.start));
            properties.add(new FloatProperty("valueEnd", values.end));
            properties.add(
                new ObjectFlagProperty<ValueChanged<RangeValues>>("onChanged", onChanged, ifNull: "disabled"));
            properties.add(ObjectFlagProperty<ValueChanged<RangeValues>>.has("onChangeStart", onChangeStart));
            properties.add(ObjectFlagProperty<ValueChanged<RangeValues>>.has("onChangeEnd", onChangeEnd));
            properties.add(new FloatProperty("min", min));
            properties.add(new FloatProperty("max", max));
            properties.add(new IntProperty("divisions", divisions));
            properties.add(new StringProperty("labelStart", labels?.start));
            properties.add(new StringProperty("labelEnd", labels?.end));
            properties.add(new ColorProperty("activeColor", activeColor));
            properties.add(new ColorProperty("inactiveColor", inactiveColor));
        }
    }


    class _RangeSliderState : TickerProviderStateMixin<RangeSlider> {
        static readonly TimeSpan enableAnimationDuration = new TimeSpan(0, 0, 0, 0, 75);
        static readonly TimeSpan valueIndicatorAnimationDuration = new TimeSpan(0, 0, 0, 0, 100);

        internal AnimationController overlayController;

        internal AnimationController valueIndicatorController;

        internal AnimationController enableController;
        internal AnimationController startPositionController;
        internal AnimationController endPositionController;
        internal Timer interactionTimer;

        public override void initState() {
            base.initState();
            overlayController = new AnimationController(
                duration: material_.kRadialReactionDuration,
                vsync: this
            );
            valueIndicatorController = new AnimationController(
                duration: valueIndicatorAnimationDuration,
                vsync: this
            );
            enableController = new AnimationController(
                duration: enableAnimationDuration,
                vsync: this,
                value: widget.onChanged != null ? 1.0f : 0.0f
            );
            startPositionController = new AnimationController(
                duration: TimeSpan.Zero,
                vsync: this,
                value: _unlerp(widget.values.start)
            );
            endPositionController = new AnimationController(
                duration: TimeSpan.Zero,
                vsync: this,
                value: _unlerp(widget.values.end)
            );
        }

        public override void didUpdateWidget(StatefulWidget oldWidget) {
            var _oldWidget = (RangeSlider) oldWidget;
            base.didUpdateWidget(oldWidget);
            if (_oldWidget.onChanged == widget.onChanged) {
                return;
            }

            bool wasEnabled = _oldWidget.onChanged != null;
            bool isEnabled = widget.onChanged != null;
            if (wasEnabled != isEnabled) {
                if (isEnabled) {
                    enableController.forward();
                }
                else {
                    enableController.reverse();
                }
            }
        }

        public override void dispose() {
            interactionTimer?.cancel();
            overlayController.dispose();
            valueIndicatorController.dispose();
            enableController.dispose();
            startPositionController.dispose();
            endPositionController.dispose();
            base.dispose();
        }

        void _handleChanged(RangeValues values) {
            D.assert(widget.onChanged != null);
            RangeValues lerpValues = _lerpRangeValues(values);
            if (lerpValues != widget.values) {
                widget.onChanged(lerpValues);
            }
        }

        void _handleDragStart(RangeValues values) {
            D.assert(widget.onChangeStart != null);
            widget.onChangeStart(_lerpRangeValues(values));
        }

        void _handleDragEnd(RangeValues values) {
            D.assert(widget.onChangeEnd != null);
            widget.onChangeEnd(_lerpRangeValues(values));
        }

        float _lerp(float value) {
            return MathUtils.lerpNullableFloat(widget.min, widget.max, value);
        }

        RangeValues _lerpRangeValues(RangeValues values) {
            return new RangeValues(_lerp(values.start), _lerp(values.end));
        }

        float _unlerp(float value) {
            D.assert(value <= widget.max);
            D.assert(value >= widget.min);
            return widget.max > widget.min ? (value - widget.min) / (widget.max - widget.min) : 0.0f;
        }

        RangeValues _unlerpRangeValues(RangeValues values) {
            return new RangeValues(_unlerp(values.start), _unlerp(values.end));
        }

        static Thumb? _defaultRangeThumbSelector(
            TextDirection textDirection,
            RangeValues values,
            float tapValue,
            Size thumbSize,
            Size trackSize,
            float dx
        ) {
            float touchRadius = Mathf.Max(thumbSize.width, RangeSlider._minTouchTargetWidth) / 2f;
            bool inStartTouchTarget = (tapValue - values.start).abs() * trackSize.width < touchRadius;
            bool inEndTouchTarget = (tapValue - values.end).abs() * trackSize.width < touchRadius;

            if (inStartTouchTarget && inEndTouchTarget) {
                bool towardsStart = false;
                bool towardsEnd = false;
                switch (textDirection) {
                    case TextDirection.ltr:
                        towardsStart = dx < 0;
                        towardsEnd = dx > 0;
                        break;
                    case TextDirection.rtl:
                        towardsStart = dx > 0;
                        towardsEnd = dx < 0;
                        break;
                }

                if (towardsStart) {
                    return Thumb.start;
                }

                if (towardsEnd) {
                    return Thumb.end;
                }
            }
            else {
                if (tapValue < values.start || inStartTouchTarget) {
                    return Thumb.start;
                }

                if (tapValue > values.end || inEndTouchTarget) {
                    return Thumb.end;
                }
            }

            return null;
        }

        const float _defaultTrackHeight = 2;
        static readonly RangeSliderTrackShape _defaultTrackShape = new RoundedRectRangeSliderTrackShape();
        static readonly RangeSliderTickMarkShape _defaultTickMarkShape = new RoundRangeSliderTickMarkShape();
        static readonly SliderComponentShape _defaultOverlayShape = new RoundSliderOverlayShape();
        static readonly RangeSliderThumbShape _defaultThumbShape = new RoundRangeSliderThumbShape();

        static readonly RangeSliderValueIndicatorShape _defaultValueIndicatorShape =
            new PaddleRangeSliderValueIndicatorShape();

        const ShowValueIndicator _defaultShowValueIndicator = ShowValueIndicator.onlyForDiscrete;
        const float _defaultMinThumbSeparation = 8;

        public override Widget build(BuildContext context) {
            D.assert(material_.debugCheckHasMaterial(context));
            D.assert(WidgetsD.debugCheckHasMediaQuery(context));

            ThemeData theme = Theme.of(context);
            SliderThemeData sliderTheme = SliderTheme.of(context);

            sliderTheme = sliderTheme.copyWith(
                trackHeight: sliderTheme.trackHeight ?? _defaultTrackHeight,
                activeTrackColor: widget.activeColor ?? sliderTheme.activeTrackColor ?? theme.colorScheme.primary,
                inactiveTrackColor: widget.inactiveColor ??
                                    sliderTheme.inactiveTrackColor ?? theme.colorScheme.primary.withOpacity(0.24f),
                disabledActiveTrackColor: sliderTheme.disabledActiveTrackColor ??
                                          theme.colorScheme.onSurface.withOpacity(0.32f),
                disabledInactiveTrackColor: sliderTheme.disabledInactiveTrackColor ??
                                            theme.colorScheme.onSurface.withOpacity(0.12f),
                activeTickMarkColor: widget.inactiveColor ??
                                     sliderTheme.activeTickMarkColor ?? theme.colorScheme.onPrimary.withOpacity(0.54f),
                inactiveTickMarkColor: widget.activeColor ??
                                       sliderTheme.inactiveTickMarkColor ??
                                       theme.colorScheme.primary.withOpacity(0.54f),
                disabledActiveTickMarkColor: sliderTheme.disabledActiveTickMarkColor ??
                                             theme.colorScheme.onPrimary.withOpacity(0.12f),
                disabledInactiveTickMarkColor: sliderTheme.disabledInactiveTickMarkColor ??
                                               theme.colorScheme.onSurface.withOpacity(0.12f),
                thumbColor: widget.activeColor ?? sliderTheme.thumbColor ?? theme.colorScheme.primary,
                overlappingShapeStrokeColor: sliderTheme.overlappingShapeStrokeColor ?? theme.colorScheme.surface,
                disabledThumbColor: sliderTheme.disabledThumbColor ?? theme.colorScheme.onSurface.withOpacity(0.38f),
                overlayColor: widget.activeColor?.withOpacity(0.12f) ??
                              sliderTheme.overlayColor ?? theme.colorScheme.primary.withOpacity(0.12f),
                valueIndicatorColor: widget.activeColor ?? sliderTheme.valueIndicatorColor ?? theme.colorScheme.primary,
                rangeTrackShape: sliderTheme.rangeTrackShape ?? _defaultTrackShape,
                rangeTickMarkShape: sliderTheme.rangeTickMarkShape ?? _defaultTickMarkShape,
                rangeThumbShape: sliderTheme.rangeThumbShape ?? _defaultThumbShape,
                overlayShape: sliderTheme.overlayShape ?? _defaultOverlayShape,
                rangeValueIndicatorShape: sliderTheme.rangeValueIndicatorShape ?? _defaultValueIndicatorShape,
                showValueIndicator: sliderTheme.showValueIndicator ?? _defaultShowValueIndicator,
                valueIndicatorTextStyle: sliderTheme.valueIndicatorTextStyle ?? theme.textTheme.bodyText1.copyWith(
                                             color: theme.colorScheme.onPrimary
                                         ),
                minThumbSeparation: sliderTheme.minThumbSeparation ?? _defaultMinThumbSeparation,
                thumbSelector: sliderTheme.thumbSelector ?? _defaultRangeThumbSelector
            );

            return new _RangeSliderRenderObjectWidget(
                values: _unlerpRangeValues(widget.values),
                divisions: widget.divisions,
                labels: widget.labels,
                sliderTheme: sliderTheme,
                textScaleFactor: MediaQuery.of(context).textScaleFactor,
                onChanged: (widget.onChanged != null) && (widget.max > widget.min)
                    ? _handleChanged
                    : (ValueChanged<RangeValues>) null,
                onChangeStart: widget.onChangeStart != null ? _handleDragStart : (ValueChanged<RangeValues>) null,
                onChangeEnd: widget.onChangeEnd != null ? _handleDragEnd : (ValueChanged<RangeValues>) null,
                state: this
            );
        }
    }

    class _RangeSliderRenderObjectWidget : LeafRenderObjectWidget {
        public _RangeSliderRenderObjectWidget(
            Key key = null,
            RangeValues values = null,
            int? divisions = null,
            RangeLabels labels = null,
            SliderThemeData sliderTheme = null,
            float textScaleFactor = 0f,
            ValueChanged<RangeValues> onChanged = null,
            ValueChanged<RangeValues> onChangeStart = null,
            ValueChanged<RangeValues> onChangeEnd = null,
            _RangeSliderState state = null) : base(key: key) {
            this.values = values;
            this.divisions = divisions;
            this.labels = labels;
            this.sliderTheme = sliderTheme;
            this.textScaleFactor = textScaleFactor;
            this.onChanged = onChanged;
            this.onChangeStart = onChangeStart;
            this.onChangeEnd = onChangeEnd;
            this.state = state;
        }

        public readonly RangeValues values;
        public readonly int? divisions;
        public readonly RangeLabels labels;
        public readonly SliderThemeData sliderTheme;
        public readonly float textScaleFactor;
        public readonly ValueChanged<RangeValues> onChanged;
        public readonly ValueChanged<RangeValues> onChangeStart;
        public readonly ValueChanged<RangeValues> onChangeEnd;
        public readonly _RangeSliderState state;

        public override RenderObject createRenderObject(BuildContext context) {
            return new _RenderRangeSlider(
                values: values,
                divisions: divisions,
                labels: labels,
                sliderTheme: sliderTheme,
                theme: Theme.of(context),
                textScaleFactor: textScaleFactor,
                onChanged: onChanged,
                onChangeStart: onChangeStart,
                onChangeEnd: onChangeEnd,
                state: state,
                textDirection: Directionality.of(context),
                platform: Theme.of(context).platform
            );
        }

        public override void updateRenderObject(BuildContext context, RenderObject renderObject) {
            var _renderObject = (_RenderRangeSlider) renderObject;
            _renderObject.values = values;
            _renderObject.divisions = divisions;
            _renderObject.labels = labels;
            _renderObject.sliderTheme = sliderTheme;
            _renderObject.theme = Theme.of(context);
            _renderObject.textScaleFactor = textScaleFactor;
            _renderObject.onChanged = onChanged;
            _renderObject.onChangeStart = onChangeStart;
            _renderObject.onChangeEnd = onChangeEnd;
            _renderObject.textDirection = Directionality.of(context);
            _renderObject.platform = Theme.of(context).platform;
        }
    }

    class _RenderRangeSlider : RelayoutWhenSystemFontsChangeMixinRenderBox {
        public _RenderRangeSlider(
            RangeValues values,
            int? divisions,
            RangeLabels labels,
            SliderThemeData sliderTheme,
            ThemeData theme,
            float textScaleFactor,
            RuntimePlatform platform,
            ValueChanged<RangeValues> onChanged,
            ValueChanged<RangeValues> onChangeStart,
            ValueChanged<RangeValues> onChangeEnd,
            _RangeSliderState state,
            TextDirection? textDirection) {
            D.assert(values != null);
            D.assert(values.start >= 0.0 && values.start <= 1.0);
            D.assert(values.end >= 0.0 && values.end <= 1.0);
            D.assert(state != null);
            D.assert(textDirection != null);

            this.onChangeStart = onChangeStart;
            this.onChangeEnd = onChangeEnd;

            _platform = platform;
            _labels = labels;
            _values = values;
            _divisions = divisions;
            _sliderTheme = sliderTheme;
            _theme = theme;
            _textScaleFactor = textScaleFactor;
            _onChanged = onChanged;
            _state = state;
            _textDirection = textDirection;

            _updateLabelPainters();

            GestureArenaTeam team = new GestureArenaTeam();
            _drag = new HorizontalDragGestureRecognizer();
            _drag.team = team;
            _drag.onStart = _handleDragStart;
            _drag.onUpdate = _handleDragUpdate;
            _drag.onEnd = _handleDragEnd;
            _drag.onCancel = _handleDragCancel;

            _tap = new TapGestureRecognizer();
            _tap.team = team;
            _tap.onTapDown = _handleTapDown;
            _tap.onTapUp = _handleTapUp;
            _tap.onTapCancel = _handleTapCancel;

            _overlayAnimation = new CurvedAnimation(
                parent: _state.overlayController,
                curve: Curves.fastOutSlowIn
            );
            _valueIndicatorAnimation = new CurvedAnimation(
                parent: _state.valueIndicatorController,
                curve: Curves.fastOutSlowIn
            );
            _enableAnimation = new CurvedAnimation(
                parent: _state.enableController,
                curve: Curves.easeInOut
            );
        }

        Thumb? _lastThumbSelection;

        static readonly TimeSpan _positionAnimationDuration = new TimeSpan(0, 0, 0, 0, 75);

        const float _minPreferredTrackWidth = 144.0f;

        float _maxSliderPartWidth {
            get {
                return Mathf.Max(LinqUtils<float, Size>.SelectArray(_sliderPartSizes,((Size size) => size.width)));
            }
        }

        float _maxSliderPartHeight {
            get {
                return Mathf.Max(LinqUtils<float,Size>.SelectArray(_sliderPartSizes,((Size size) => size.height)));
            }
        }

        List<Size> _sliderPartSizes {
            get {
                return new List<Size> {
                    _sliderTheme.overlayShape.getPreferredSize(isEnabled, isDiscrete),
                    _sliderTheme.rangeThumbShape.getPreferredSize(isEnabled, isDiscrete),
                    _sliderTheme.rangeTickMarkShape.getPreferredSize(isEnabled: isEnabled, sliderTheme: sliderTheme),
                };
            }
        }

        float _minPreferredTrackHeight {
            get { return _sliderTheme.trackHeight.Value; }
        }

        Rect _trackRect {
            get {
                return _sliderTheme.rangeTrackShape.getPreferredRect(
                    parentBox: this,
                    offset: Offset.zero,
                    sliderTheme: _sliderTheme,
                    isDiscrete: false
                );
            }
        }

        static readonly TimeSpan _minimumInteractionTime = new TimeSpan(0, 0, 0, 0, 500);

        readonly _RangeSliderState _state;
        Animation<float> _overlayAnimation;
        Animation<float> _valueIndicatorAnimation;
        Animation<float> _enableAnimation;
        TextPainter _startLabelPainter = new TextPainter();
        TextPainter _endLabelPainter = new TextPainter();
        HorizontalDragGestureRecognizer _drag;
        TapGestureRecognizer _tap;
        
        bool _active = false;
        RangeValues _newValues;

        bool isEnabled {
            get { return onChanged != null; }
        }

        bool isDiscrete {
            get { return divisions != null && divisions > 0; }
        }

        RangeValues _values;
        public RangeValues values {
            get { return _values; }
            set {
                D.assert(value != null);
                D.assert(value.start >= 0.0 && value.start <= 1.0f);
                D.assert(value.end >= 0.0 && value.end <= 1.0f);
                D.assert(value.start <= value.end);

                RangeValues convertedValues = isDiscrete ? _discretizeRangeValues(value) : value;
                if (convertedValues == _values) {
                    return;
                }

                _values = convertedValues;
                if (isDiscrete) {
                    float startDistance = (_values.start - _state.startPositionController.value).abs();
                    _state.startPositionController.duration = startDistance != 0.0f
                        ? new TimeSpan(0, 0, 0, 0,
                            (int) (_positionAnimationDuration.TotalMilliseconds * (1.0f / startDistance)))
                        : TimeSpan.Zero;
                    _state.startPositionController.animateTo(_values.start, curve: Curves.easeInOut);

                    float endDistance = (_values.end - _state.endPositionController.value).abs();

                    _state.endPositionController.duration = endDistance != 0.0f
                        ? new TimeSpan(0, 0, 0, 0,
                            (int) (_positionAnimationDuration.TotalMilliseconds * (1.0f / endDistance)))
                        : TimeSpan.Zero;
                    _state.endPositionController.animateTo(_values.end, curve: Curves.easeInOut);
                }
                else {
                    _state.startPositionController.setValue(convertedValues.start);
                    _state.endPositionController.setValue(convertedValues.end);
                }
            }
        }

        RuntimePlatform _platform;
        public RuntimePlatform platform {
            get { return _platform; }

            set {
                if (_platform == value) {
                    return;
                }

                _platform = value;
            }
        }


        int? _divisions;
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

        RangeLabels _labels;
        public RangeLabels labels {
            get { return _labels; }
            set {
                if (labels == _labels) {
                    return;
                }

                _labels = labels;
                _updateLabelPainters();
            }
        }

        SliderThemeData _sliderTheme;
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

        ThemeData _theme;
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

        float _textScaleFactor;
        public float textScaleFactor {
            get { return _textScaleFactor; }
            set {
                if (value == _textScaleFactor) {
                    return;
                }

                _textScaleFactor = value;
                _updateLabelPainters();
            }
        }

        ValueChanged<RangeValues> _onChanged;
        public ValueChanged<RangeValues> onChanged {
            get { return _onChanged; }
            set {
                if (value == _onChanged) {
                    return;
                }

                bool wasEnabled = isEnabled;
                _onChanged = value;
                if (wasEnabled != isEnabled) {
                    markNeedsPaint();
                }
            }
        }

        public ValueChanged<RangeValues> onChangeStart;
        public ValueChanged<RangeValues> onChangeEnd;

        TextDirection? _textDirection;
        public TextDirection? textDirection {
            get { return _textDirection; }
            set {
                D.assert(value != null);
                if (value == _textDirection) {
                    return;
                }

                _textDirection = value;
                _updateLabelPainters();
            }
        }

        bool showValueIndicator {
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

        Size _thumbSize {
            get { return _sliderTheme.rangeThumbShape.getPreferredSize(isEnabled, isDiscrete); }
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

        void _updateLabelPainters() {
            _updateLabelPainter(Thumb.start);
            _updateLabelPainter(Thumb.end);
        }

        void _updateLabelPainter(Thumb thumb) {
            if (labels == null) {
                return;
            }

            string text = null;
            TextPainter labelPainter = null;
            switch (thumb) {
                case Thumb.start:
                    text = labels.start;
                    labelPainter = _startLabelPainter;
                    break;
                case Thumb.end:
                    text = labels.end;
                    labelPainter = _endLabelPainter;
                    break;
            }

            if (labels != null) {
                labelPainter.text = new TextSpan(
                    style: _sliderTheme.valueIndicatorTextStyle,
                    text: text
                );
                labelPainter.textDirection = textDirection;
                labelPainter.textScaleFactor = textScaleFactor;
                labelPainter.layout();
            }
            else {
                labelPainter.text = null;
            }

            markNeedsLayout();
        }

        public override void systemFontsDidChange() {
            base.systemFontsDidChange();
            _startLabelPainter.markNeedsLayout();
            _endLabelPainter.markNeedsLayout();
            _updateLabelPainters();
        }

        public override void attach(object owner) {
            base.attach(owner);
            _overlayAnimation.addListener(markNeedsPaint);
            _valueIndicatorAnimation.addListener(markNeedsPaint);
            _enableAnimation.addListener(markNeedsPaint);
            _state.startPositionController.addListener(markNeedsPaint);
            _state.endPositionController.addListener(markNeedsPaint);
        }

        public override void detach() {
            _overlayAnimation.removeListener(markNeedsPaint);
            _valueIndicatorAnimation.removeListener(markNeedsPaint);
            _enableAnimation.removeListener(markNeedsPaint);
            _state.startPositionController.removeListener(markNeedsPaint);
            _state.endPositionController.removeListener(markNeedsPaint);
            base.detach();
        }

        float _getValueFromVisualPosition(float visualPosition) {
            switch (textDirection) {
                case TextDirection.rtl:
                    return 1.0f - visualPosition;
                case TextDirection.ltr:
                    return visualPosition;
            }

            D.assert(false);
            return 0;
        }

        float _getValueFromGlobalPosition(Offset globalPosition) {
            float visualPosition = (globalToLocal(globalPosition).dx - _trackRect.left) / _trackRect.width;
            return _getValueFromVisualPosition(visualPosition);
        }

        float _discretize(float value) {
            float result = value.clamp(0.0f, 1.0f);
            if (isDiscrete) {
                result = (result * divisions.Value).round() * 1.0f / divisions.Value;
            }

            return result;
        }

        RangeValues _discretizeRangeValues(RangeValues values) {
            return new RangeValues(_discretize(values.start), _discretize(values.end));
        }

        void _startInteraction(Offset globalPosition) {
            float tapValue = _getValueFromGlobalPosition(globalPosition).clamp(0.0f, 1.0f);
            _lastThumbSelection = sliderTheme.thumbSelector(textDirection.Value, values, tapValue, _thumbSize, size, 0);

            if (_lastThumbSelection != null) {
                _active = true;
                RangeValues currentValues = _discretizeRangeValues(values);
                if (_lastThumbSelection == Thumb.start) {
                    _newValues = new RangeValues(tapValue, currentValues.end);
                }
                else if (_lastThumbSelection == Thumb.end) {
                    _newValues = new RangeValues(currentValues.start, tapValue);
                }

                _updateLabelPainter(_lastThumbSelection.Value);

                if (onChangeStart != null) {
                    onChangeStart(currentValues);
                }

                onChanged(_discretizeRangeValues(_newValues));

                _state.overlayController.forward();
                if (showValueIndicator) {
                    _state.valueIndicatorController.forward();
                    _state.interactionTimer?.cancel();
                    _state.interactionTimer =
                        Timer.create(
                            new TimeSpan(0, 0, 0, 0,
                                (int) (_minimumInteractionTime.TotalMilliseconds * scheduler_.timeDilation)), () => {
                                _state.interactionTimer = null;
                                if (!_active && _state.valueIndicatorController.status == AnimationStatus.completed) {
                                    _state.valueIndicatorController.reverse();
                                }
                            });
                }
            }
        }

        void _handleDragUpdate(DragUpdateDetails details) {
            float dragValue = _getValueFromGlobalPosition(details.globalPosition);

            bool shouldCallOnChangeStart = false;
            if (_lastThumbSelection == null) {
                _lastThumbSelection = sliderTheme.thumbSelector(textDirection.Value, values, dragValue, _thumbSize,
                    size, details.delta.dx);
                if (_lastThumbSelection != null) {
                    shouldCallOnChangeStart = true;
                    _active = true;
                    _state.overlayController.forward();
                    if (showValueIndicator) {
                        _state.valueIndicatorController.forward();
                    }
                }
            }

            if (isEnabled && _lastThumbSelection != null) {
                RangeValues currentValues = _discretizeRangeValues(values);
                if (onChangeStart != null && shouldCallOnChangeStart) {
                    onChangeStart(currentValues);
                }

                float currentDragValue = _discretize(dragValue);

                float minThumbSeparationValue =
                    isDiscrete ? 0 : sliderTheme.minThumbSeparation.Value / _trackRect.width;
                if (_lastThumbSelection == Thumb.start) {
                    _newValues =
                        new RangeValues(Mathf.Min(currentDragValue, currentValues.end - minThumbSeparationValue),
                            currentValues.end);
                }
                else if (_lastThumbSelection == Thumb.end) {
                    _newValues = new RangeValues(currentValues.start,
                        Mathf.Max(currentDragValue, currentValues.start + minThumbSeparationValue));
                }

                onChanged(_newValues);
            }
        }

        void _endInteraction() {
            _state.overlayController.reverse();
            if (showValueIndicator && _state.interactionTimer == null) {
                _state.valueIndicatorController.reverse();
            }

            if (_active && _state.mounted && _lastThumbSelection != null) {
                RangeValues discreteValues = _discretizeRangeValues(_newValues);
                if (onChangeEnd != null) {
                    onChangeEnd(discreteValues);
                }

                _active = false;
            }
        }

        void _handleDragStart(DragStartDetails details) {
            _startInteraction(details.globalPosition);
        }

        void _handleDragEnd(DragEndDetails details) {
            _endInteraction();
        }

        void _handleDragCancel() {
            _endInteraction();
        }

        void _handleTapDown(TapDownDetails details) {
            _startInteraction(details.globalPosition);
        }

        void _handleTapUp(TapUpDetails details) {
            _endInteraction();
        }

        void _handleTapCancel() {
            _endInteraction();
        }

        protected override bool hitTestSelf(Offset position) {
            return true;
        }

        public override void handleEvent(PointerEvent evt, HitTestEntry entry) {
            D.assert(debugHandleEvent(evt, entry));
            if (evt is PointerDownEvent && isEnabled) {
                _drag.addPointer((PointerDownEvent) evt);
                _tap.addPointer((PointerDownEvent) evt);
            }
        }


        protected internal override float computeMinIntrinsicWidth(float height) {
            return _minPreferredTrackWidth + _maxSliderPartWidth;
        }


        protected internal override float computeMaxIntrinsicWidth(float height) {
            return _minPreferredTrackWidth + _maxSliderPartWidth;
        }


        protected internal override float computeMinIntrinsicHeight(float width) {
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
                constraints.hasBoundedWidth ? constraints.maxWidth : _minPreferredTrackWidth + _maxSliderPartWidth,
                constraints.hasBoundedHeight
                    ? constraints.maxHeight
                    : Mathf.Max(_minPreferredTrackHeight, _maxSliderPartHeight)
            );
        }

        public override void paint(PaintingContext context, Offset offset) {
            float startValue = _state.startPositionController.value;
            float endValue = _state.endPositionController.value;

            float startVisualPosition = 0f;
            float endVisualPosition = 0f;
            switch (textDirection) {
                case TextDirection.rtl:
                    startVisualPosition = 1.0f - startValue;
                    endVisualPosition = 1.0f - endValue;
                    break;
                case TextDirection.ltr:
                    startVisualPosition = startValue;
                    endVisualPosition = endValue;
                    break;
            }

            Rect trackRect = _sliderTheme.rangeTrackShape.getPreferredRect(
                parentBox: this,
                offset: offset,
                sliderTheme: _sliderTheme,
                isDiscrete: isDiscrete
            );
            Offset startThumbCenter =
                new Offset(trackRect.left + startVisualPosition * trackRect.width, trackRect.center.dy);
            Offset endThumbCenter =
                new Offset(trackRect.left + endVisualPosition * trackRect.width, trackRect.center.dy);

            _sliderTheme.rangeTrackShape.paint(
                context,
                offset,
                parentBox: this,
                sliderTheme: _sliderTheme,
                enableAnimation: _enableAnimation,
                textDirection: _textDirection,
                startThumbCenter: startThumbCenter,
                endThumbCenter: endThumbCenter,
                isDiscrete: isDiscrete,
                isEnabled: isEnabled
            );

            if (!_overlayAnimation.isDismissed) {
                if (_lastThumbSelection == Thumb.start) {
                    _sliderTheme.overlayShape.paint(
                        context,
                        startThumbCenter,
                        activationAnimation: _overlayAnimation,
                        enableAnimation: _enableAnimation,
                        isDiscrete: isDiscrete,
                        labelPainter: _startLabelPainter,
                        parentBox: this,
                        sliderTheme: _sliderTheme,
                        textDirection: _textDirection,
                        value: startValue
                    );
                }

                if (_lastThumbSelection == Thumb.end) {
                    _sliderTheme.overlayShape.paint(
                        context,
                        endThumbCenter,
                        activationAnimation: _overlayAnimation,
                        enableAnimation: _enableAnimation,
                        isDiscrete: isDiscrete,
                        labelPainter: _endLabelPainter,
                        parentBox: this,
                        sliderTheme: _sliderTheme,
                        textDirection: _textDirection,
                        value: endValue
                    );
                }
            }

            if (isDiscrete) {
                float tickMarkWidth = _sliderTheme.rangeTickMarkShape.getPreferredSize(
                    isEnabled: isEnabled,
                    sliderTheme: _sliderTheme
                ).width;
                float adjustedTrackWidth = trackRect.width - tickMarkWidth;

                if (adjustedTrackWidth / divisions >= 3.0f * tickMarkWidth) {
                    float dy = trackRect.center.dy;
                    for (int i = 0; i <= divisions; i++) {
                        float value = i / divisions.Value;
                        float dx = trackRect.left + value * adjustedTrackWidth + tickMarkWidth / 2;
                        Offset tickMarkOffset = new Offset(dx, dy);
                        _sliderTheme.rangeTickMarkShape.paint(
                            context,
                            tickMarkOffset,
                            parentBox: this,
                            sliderTheme: _sliderTheme,
                            enableAnimation: _enableAnimation,
                            textDirection: _textDirection,
                            startThumbCenter: startThumbCenter,
                            endThumbCenter: endThumbCenter,
                            isEnabled: isEnabled
                        );
                    }
                }
            }

            float thumbDelta = (endThumbCenter.dx - startThumbCenter.dx).abs();

            bool isLastThumbStart = _lastThumbSelection == Thumb.start;
            Thumb bottomThumb = isLastThumbStart ? Thumb.end : Thumb.start;
            Thumb topThumb = isLastThumbStart ? Thumb.start : Thumb.end;
            Offset bottomThumbCenter = isLastThumbStart ? endThumbCenter : startThumbCenter;
            Offset topThumbCenter = isLastThumbStart ? startThumbCenter : endThumbCenter;
            TextPainter bottomLabelPainter = isLastThumbStart ? _endLabelPainter : _startLabelPainter;
            TextPainter topLabelPainter = isLastThumbStart ? _startLabelPainter : _endLabelPainter;
            float bottomValue = isLastThumbStart ? endValue : startValue;
            float topValue = isLastThumbStart ? startValue : endValue;
            bool shouldPaintValueIndicators = isEnabled && labels != null && !_valueIndicatorAnimation.isDismissed &&
                                              showValueIndicator;

            if (shouldPaintValueIndicators) {
                _sliderTheme.rangeValueIndicatorShape.paint(
                    context,
                    bottomThumbCenter,
                    activationAnimation: _valueIndicatorAnimation,
                    enableAnimation: _enableAnimation,
                    isDiscrete: isDiscrete,
                    isOnTop: false,
                    labelPainter: bottomLabelPainter,
                    parentBox: this,
                    sliderTheme: _sliderTheme,
                    textDirection: _textDirection,
                    thumb: bottomThumb,
                    value: bottomValue
                );
            }

            _sliderTheme.rangeThumbShape.paint(
                context,
                bottomThumbCenter,
                activationAnimation: _valueIndicatorAnimation,
                enableAnimation: _enableAnimation,
                isDiscrete: isDiscrete,
                isOnTop: false,
                textDirection: textDirection,
                sliderTheme: _sliderTheme,
                thumb: bottomThumb
            );

            if (shouldPaintValueIndicators) {
                float startOffset = sliderTheme.rangeValueIndicatorShape.getHorizontalShift(
                    parentBox: this,
                    center: startThumbCenter,
                    labelPainter: _startLabelPainter,
                    activationAnimation: _valueIndicatorAnimation
                );
                float endOffset = sliderTheme.rangeValueIndicatorShape.getHorizontalShift(
                    parentBox: this,
                    center: endThumbCenter,
                    labelPainter: _endLabelPainter,
                    activationAnimation: _valueIndicatorAnimation
                );
                float startHalfWidth = sliderTheme.rangeValueIndicatorShape
                                           .getPreferredSize(isEnabled, isDiscrete, labelPainter: _startLabelPainter)
                                           .width / 2;
                float endHalfWidth = sliderTheme.rangeValueIndicatorShape
                                         .getPreferredSize(isEnabled, isDiscrete, labelPainter: _endLabelPainter)
                                         .width / 2;
                float innerOverflow = startHalfWidth + endHalfWidth;
                switch (textDirection) {
                    case TextDirection.ltr:
                        innerOverflow += startOffset;
                        innerOverflow -= endOffset;
                        break;
                    case TextDirection.rtl:
                        innerOverflow -= startOffset;
                        innerOverflow += endOffset;
                        break;
                }

                _sliderTheme.rangeValueIndicatorShape.paint(
                    context,
                    topThumbCenter,
                    activationAnimation: _valueIndicatorAnimation,
                    enableAnimation: _enableAnimation,
                    isDiscrete: isDiscrete,
                    isOnTop: thumbDelta < innerOverflow,
                    labelPainter: topLabelPainter,
                    parentBox: this,
                    sliderTheme: _sliderTheme,
                    textDirection: _textDirection,
                    thumb: topThumb,
                    value: topValue
                );
            }

            _sliderTheme.rangeThumbShape.paint(
                context,
                topThumbCenter,
                activationAnimation: _valueIndicatorAnimation,
                enableAnimation: _enableAnimation,
                isDiscrete: isDiscrete,
                isOnTop: thumbDelta < sliderTheme.rangeThumbShape.getPreferredSize(isEnabled, isDiscrete).width,
                textDirection: textDirection,
                sliderTheme: _sliderTheme,
                thumb: topThumb
            );
        }
    }
}