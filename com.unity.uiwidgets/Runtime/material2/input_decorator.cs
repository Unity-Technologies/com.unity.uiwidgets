using System;
using System.Collections.Generic;

using Unity.UIWidgets.animation;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.gestures;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.rendering;
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
    class InputDecoratorConstants {
        public static readonly TimeSpan _kTransitionDuration = new TimeSpan(0, 0, 0, 0, 200);
        public static readonly Curve _kTransitionCurve = Curves.fastOutSlowIn;
    }


    class _InputBorderGap : ChangeNotifier, IEquatable<_InputBorderGap> {
        float _start;

        public float start {
            get { return _start; }
            set {
                if (value != _start) {
                    _start = value;
                    notifyListeners();
                }
            }
        }

        float _extent = 0.0f;

        public float extent {
            get { return _extent; }
            set {
                if (value != _extent) {
                    _extent = value;
                    notifyListeners();
                }
            }
        }

        public bool Equals(_InputBorderGap other) {
            if (ReferenceEquals(null, other)) {
                return false;
            }

            if (ReferenceEquals(this, other)) {
                return true;
            }

            return start == other.start && extent == other._extent;
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

            return Equals((_InputBorderGap) obj);
        }

        public override int GetHashCode() {
            unchecked {
                return (_start.GetHashCode() * 397) ^ _extent.GetHashCode();
            }
        }

        public static bool operator ==(_InputBorderGap left, _InputBorderGap right) {
            return Equals(left, right);
        }

        public static bool operator !=(_InputBorderGap left, _InputBorderGap right) {
            return !Equals(left, right);
        }
    }

    class _InputBorderTween : Tween<InputBorder> {
        public _InputBorderTween(InputBorder begin = null, InputBorder end = null) : base(begin: begin, end: end) {
        }

        public override InputBorder lerp(float t) {
            return (InputBorder) ShapeBorder.lerp(begin, end, t);
        }
    }

    class _InputBorderPainter : AbstractCustomPainter {
        public _InputBorderPainter(
            Listenable repaint,
            Animation<float> borderAnimation = null,
            _InputBorderTween border = null,
            Animation<float> gapAnimation = null,
            _InputBorderGap gap = null,
            Color fillColor = null
        ) : base(repaint: repaint) {
            this.borderAnimation = borderAnimation;
            this.border = border;
            this.gapAnimation = gapAnimation;
            this.gap = gap;
            this.fillColor = fillColor;
        }

        public readonly Animation<float> borderAnimation;
        public readonly _InputBorderTween border;
        public readonly Animation<float> gapAnimation;
        public readonly _InputBorderGap gap;
        public readonly Color fillColor;

        public override void paint(Canvas canvas, Size size) {
            InputBorder borderValue = border.evaluate(borderAnimation);
            Rect canvasRect = Offset.zero & size;

            if (fillColor.alpha > 0) {
                Paint paint = new Paint();
                paint.color = fillColor;
                paint.style = PaintingStyle.fill;
                canvas.drawPath(
                    borderValue.getOuterPath(canvasRect),
                    paint
                );
            }

            borderValue.paint(
                canvas,
                canvasRect,
                gapStart: gap.start,
                gapExtent: gap.extent,
                gapPercentage: gapAnimation.value
            );
        }

        public override bool shouldRepaint(CustomPainter _oldPainter) {
            _InputBorderPainter oldPainter = _oldPainter as _InputBorderPainter;
            return borderAnimation != oldPainter.borderAnimation
                   || gapAnimation != oldPainter.gapAnimation
                   || border != oldPainter.border
                   || gap != oldPainter.gap;
        }
    }

    class _BorderContainer : StatefulWidget {
        public _BorderContainer(
            Key key = null,
            InputBorder border = null,
            _InputBorderGap gap = null,
            Animation<float> gapAnimation = null,
            Color fillColor = null,
            Widget child = null
        ) : base(key: key) {
            D.assert(border != null);
            D.assert(gap != null);
            D.assert(fillColor != null);
            this.border = border;
            this.gap = gap;
            this.gapAnimation = gapAnimation;
            this.fillColor = fillColor;
            this.child = child;
        }

        public readonly InputBorder border;
        public readonly _InputBorderGap gap;
        public readonly Animation<float> gapAnimation;
        public readonly Color fillColor;
        public readonly Widget child;

        public override State createState() {
            return new _BorderContainerState();
        }
    }

    class _BorderContainerState : SingleTickerProviderStateMixin<_BorderContainer> {
        AnimationController _controller;
        Animation<float> _borderAnimation;
        _InputBorderTween _border;

        public override void initState() {
            base.initState();
            _controller = new AnimationController(
                duration: InputDecoratorConstants._kTransitionDuration,
                vsync: this
            );
            _borderAnimation = new CurvedAnimation(
                parent: _controller,
                curve: InputDecoratorConstants._kTransitionCurve
            );
            _border = new _InputBorderTween(
                begin: widget.border,
                end: widget.border
            );
        }

        public override void dispose() {
            _controller.dispose();
            base.dispose();
        }

        public override void didUpdateWidget(StatefulWidget _oldWidget) {
            _BorderContainer oldWidget = _oldWidget as _BorderContainer;
            base.didUpdateWidget(oldWidget);
            if (widget.border != oldWidget.border) {
                _border = new _InputBorderTween(
                    begin: oldWidget.border,
                    end: widget.border
                );
                _controller.setValue(0.0f);
                _controller.forward();
            }
        }

        public override Widget build(BuildContext context) {
            return new CustomPaint(
                foregroundPainter: new _InputBorderPainter(
                    repaint: ListenableUtils.merge(new List<Listenable> {_borderAnimation, widget.gap}),
                    borderAnimation: _borderAnimation,
                    border: _border,
                    gapAnimation: widget.gapAnimation,
                    gap: widget.gap,
                    fillColor: widget.fillColor
                ),
                child: widget.child
            );
        }
    }

    class _Shaker : AnimatedWidget {
        public _Shaker(
            Key key = null,
            Animation<float> animation = null,
            Widget child = null
        ) : base(key: key, listenable: animation) {
            this.child = child;
        }

        public readonly Widget child;

        public Animation<float> animation {
            get { return (Animation<float>) listenable; }
        }

        public float translateX {
            get {
                const float shakeDelta = 4.0f;
                float t = animation.value;
                if (t <= 0.25f) {
                    return -t * shakeDelta;
                }
                else if (t < 0.75f) {
                    return (t - 0.5f) * shakeDelta;
                }
                else {
                    return (1.0f - t) * 4.0f * shakeDelta;
                }
            }
        }

        protected internal override Widget build(BuildContext context) {
            return new Transform(
                transform: Matrix4.translationValues(translateX, 0, 0),
                child: child
            );
        }
    }

    class _HelperError : StatefulWidget {
        public _HelperError(
            Key key = null,
            TextAlign? textAlign = null,
            string helperText = null,
            TextStyle helperStyle = null,
            string errorText = null,
            TextStyle errorStyle = null,
            int? errorMaxLines = null
        ) : base(key: key) {
            this.textAlign = textAlign;
            this.helperText = helperText;
            this.helperStyle = helperStyle;
            this.errorText = errorText;
            this.errorStyle = errorStyle;
            this.errorMaxLines = errorMaxLines;
        }

        public readonly TextAlign? textAlign;
        public readonly string helperText;
        public readonly TextStyle helperStyle;
        public readonly string errorText;
        public readonly TextStyle errorStyle;
        public readonly int? errorMaxLines;

        public override State createState() {
            return new _HelperErrorState();
        }
    }

    class _HelperErrorState : SingleTickerProviderStateMixin<_HelperError> {
        static readonly Widget empty = new SizedBox();

        AnimationController _controller;
        Widget _helper;
        Widget _error;

        public override void initState() {
            base.initState();
            _controller = new AnimationController(
                duration: InputDecoratorConstants._kTransitionDuration,
                vsync: this
            );
            if (widget.errorText != null) {
                _error = _buildError();
                _controller.setValue(1.0f);
            }
            else if (widget.helperText != null) {
                _helper = _buildHelper();
            }

            _controller.addListener(_handleChange);
        }

        public override void dispose() {
            _controller.dispose();
            base.dispose();
        }

        void _handleChange() {
            setState(() => { });
        }

        public override void didUpdateWidget(StatefulWidget _old) {
            base.didUpdateWidget(_old);

            _HelperError old = _old as _HelperError;

            string newErrorText = widget.errorText;
            string newHelperText = widget.helperText;
            string oldErrorText = old.errorText;
            string oldHelperText = old.helperText;

            bool errorTextStateChanged = (newErrorText != null) != (oldErrorText != null);
            bool helperTextStateChanged = newErrorText == null && (newHelperText != null) != (oldHelperText != null);

            if (errorTextStateChanged || helperTextStateChanged) {
                if (newErrorText != null) {
                    _error = _buildError();
                    _controller.forward();
                }
                else if (newHelperText != null) {
                    _helper = _buildHelper();
                    _controller.reverse();
                }
                else {
                    _controller.reverse();
                }
            }
        }

        Widget _buildHelper() {
            D.assert(widget.helperText != null);
            return new Opacity(
                opacity: 1.0f - _controller.value,
                child: new Text(
                    widget.helperText,
                    style: widget.helperStyle,
                    textAlign: widget.textAlign,
                    overflow: TextOverflow.ellipsis
                )
            );
        }

        Widget _buildError() {
            D.assert(widget.errorText != null);
            return new Opacity(
                opacity: _controller.value,
                child: new FractionalTranslation(
                    translation: new OffsetTween(
                        begin: new Offset(0.0f, -0.25f),
                        end: new Offset(0.0f, 0.0f)
                    ).evaluate(_controller.view),
                    child: new Text(
                        widget.errorText,
                        style: widget.errorStyle,
                        textAlign: widget.textAlign,
                        overflow: TextOverflow.ellipsis,
                        maxLines: widget.errorMaxLines
                    )
                )
            );
        }

        public override Widget build(BuildContext context) {
            if (_controller.isDismissed) {
                _error = null;
                if (widget.helperText != null) {
                    return _helper = _buildHelper();
                }
                else {
                    _helper = null;
                    return empty;
                }
            }

            if (_controller.isCompleted) {
                _helper = null;
                if (widget.errorText != null) {
                    return _error = _buildError();
                }
                else {
                    _error = null;
                    return empty;
                }
            }

            if (_helper == null && widget.errorText != null) {
                return _buildError();
            }

            if (_error == null && widget.helperText != null) {
                return _buildHelper();
            }

            if (widget.errorText != null) {
                return new Stack(
                    children: new List<Widget> {
                        new Opacity(
                            opacity: 1.0f - _controller.value,
                            child: _helper
                        ),
                        _buildError(),
                    }
                );
            }

            if (widget.helperText != null) {
                return new Stack(
                    children: new List<Widget> {
                        _buildHelper(),
                        new Opacity(
                            opacity: _controller.value,
                            child: _error
                        )
                    }
                );
            }

            return empty;
        }
    }

    enum _DecorationSlot {
        icon,
        input,
        label,
        hint,
        prefix,
        suffix,
        prefixIcon,
        suffixIcon,
        helperError,
        counter,
        container
    }

    class _Decoration : IEquatable<_Decoration> {
        public _Decoration(
            EdgeInsets contentPadding,
            bool isCollapsed,
            float floatingLabelHeight,
            float floatingLabelProgress,
            InputBorder border = null,
            _InputBorderGap borderGap = null,
            Widget icon = null,
            Widget input = null,
            Widget label = null,
            Widget hint = null,
            Widget prefix = null,
            Widget suffix = null,
            Widget prefixIcon = null,
            Widget suffixIcon = null,
            Widget helperError = null,
            Widget counter = null,
            Widget container = null,
            bool? alignLabelWithHint = null
        ) {
            D.assert(contentPadding != null);
            this.contentPadding = contentPadding;
            this.isCollapsed = isCollapsed;
            this.floatingLabelHeight = floatingLabelHeight;
            this.floatingLabelProgress = floatingLabelProgress;
            this.border = border;
            this.borderGap = borderGap;
            this.icon = icon;
            this.input = input;
            this.label = label;
            this.hint = hint;
            this.prefix = prefix;
            this.suffix = suffix;
            this.prefixIcon = prefixIcon;
            this.suffixIcon = suffixIcon;
            this.helperError = helperError;
            this.counter = counter;
            this.container = container;
            this.alignLabelWithHint = alignLabelWithHint;
        }

        public readonly EdgeInsets contentPadding;
        public readonly bool isCollapsed;
        public readonly float floatingLabelHeight;
        public readonly float floatingLabelProgress;
        public readonly InputBorder border;
        public readonly _InputBorderGap borderGap;
        public readonly bool? alignLabelWithHint;
        public readonly Widget icon;
        public readonly Widget input;
        public readonly Widget label;
        public readonly Widget hint;
        public readonly Widget prefix;
        public readonly Widget suffix;
        public readonly Widget prefixIcon;
        public readonly Widget suffixIcon;
        public readonly Widget helperError;
        public readonly Widget counter;
        public readonly Widget container;

        public bool Equals(_Decoration other) {
            if (ReferenceEquals(null, other)) {
                return false;
            }

            if (ReferenceEquals(this, other)) {
                return true;
            }

            return Equals(contentPadding, other.contentPadding) && isCollapsed == other.isCollapsed &&
                   floatingLabelHeight.Equals(other.floatingLabelHeight) &&
                   floatingLabelProgress.Equals(other.floatingLabelProgress) &&
                   Equals(border, other.border) && Equals(borderGap, other.borderGap) &&
                   Equals(icon, other.icon) && Equals(input, other.input) &&
                   Equals(label, other.label) && Equals(hint, other.hint) &&
                   Equals(prefix, other.prefix) && Equals(suffix, other.suffix) &&
                   Equals(prefixIcon, other.prefixIcon) && Equals(suffixIcon, other.suffixIcon) &&
                   Equals(helperError, other.helperError) && Equals(counter, other.counter) &&
                   Equals(container, other.container) && Equals(alignLabelWithHint, other.alignLabelWithHint);
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

            return Equals((_Decoration) obj);
        }

        public override int GetHashCode() {
            unchecked {
                var hashCode = (contentPadding != null ? contentPadding.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ isCollapsed.GetHashCode();
                hashCode = (hashCode * 397) ^ floatingLabelHeight.GetHashCode();
                hashCode = (hashCode * 397) ^ floatingLabelProgress.GetHashCode();
                hashCode = (hashCode * 397) ^ (border != null ? border.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (borderGap != null ? borderGap.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (icon != null ? icon.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (input != null ? input.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (label != null ? label.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (hint != null ? hint.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (prefix != null ? prefix.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (suffix != null ? suffix.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (prefixIcon != null ? prefixIcon.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (suffixIcon != null ? suffixIcon.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (helperError != null ? helperError.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (counter != null ? counter.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (container != null ? container.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^
                           (alignLabelWithHint != null ? alignLabelWithHint.GetHashCode() : 0);
                return hashCode;
            }
        }

        public static bool operator ==(_Decoration left, _Decoration right) {
            return Equals(left, right);
        }

        public static bool operator !=(_Decoration left, _Decoration right) {
            return !Equals(left, right);
        }
    }

    class _RenderDecorationLayout {
        public _RenderDecorationLayout(
            Dictionary<RenderBox, float> boxToBaseline = null,
            float? inputBaseline = null,
            float? outlineBaseline = null,
            float? subtextBaseline = null,
            float? containerHeight = null,
            float? subtextHeight = null
        ) {
            this.boxToBaseline = boxToBaseline;
            this.inputBaseline = inputBaseline;
            this.outlineBaseline = outlineBaseline;
            this.subtextBaseline = subtextBaseline;
            this.containerHeight = containerHeight;
            this.subtextHeight = subtextHeight;
        }

        public readonly Dictionary<RenderBox, float> boxToBaseline;
        public readonly float? inputBaseline;
        public readonly float? outlineBaseline;
        public readonly float? subtextBaseline;
        public readonly float? containerHeight;
        public readonly float? subtextHeight;
    }

    class _RenderDecoration : RenderBox {
        public _RenderDecoration(
            _Decoration decoration,
            TextBaseline? textBaseline,
            bool isFocused,
            bool expands
        ) {
            D.assert(decoration != null);
            D.assert(textBaseline != null);
            _decoration = decoration;
            _textBaseline = textBaseline;
            _isFocused = isFocused;
            _expands = expands;
        }

        public const float subtextGap = 8.0f;

        public readonly Dictionary<_DecorationSlot, RenderBox> slotToChild =
            new Dictionary<_DecorationSlot, RenderBox>();

        public readonly Dictionary<RenderBox, _DecorationSlot> childToSlot =
            new Dictionary<RenderBox, _DecorationSlot>();

        RenderBox _updateChild(RenderBox oldChild, RenderBox newChild, _DecorationSlot slot) {
            if (oldChild != null) {
                dropChild(oldChild);
                childToSlot.Remove(oldChild);
                slotToChild.Remove(slot);
            }

            if (newChild != null) {
                childToSlot[newChild] = slot;
                slotToChild[slot] = newChild;
                adoptChild(newChild);
            }

            return newChild;
        }

        RenderBox _icon;

        public RenderBox icon {
            get { return _icon; }
            set { _icon = _updateChild(_icon, value, _DecorationSlot.icon); }
        }

        RenderBox _input;

        public RenderBox input {
            get { return _input; }
            set { _input = _updateChild(_input, value, _DecorationSlot.input); }
        }

        RenderBox _label;

        public RenderBox label {
            get { return _label; }
            set { _label = _updateChild(_label, value, _DecorationSlot.label); }
        }

        RenderBox _hint;

        public RenderBox hint {
            get { return _hint; }
            set { _hint = _updateChild(_hint, value, _DecorationSlot.hint); }
        }

        RenderBox _prefix;

        public RenderBox prefix {
            get { return _prefix; }
            set { _prefix = _updateChild(_prefix, value, _DecorationSlot.prefix); }
        }

        RenderBox _suffix;

        public RenderBox suffix {
            get { return _suffix; }
            set { _suffix = _updateChild(_suffix, value, _DecorationSlot.suffix); }
        }

        RenderBox _prefixIcon;

        public RenderBox prefixIcon {
            get { return _prefixIcon; }
            set { _prefixIcon = _updateChild(_prefixIcon, value, _DecorationSlot.prefixIcon); }
        }

        RenderBox _suffixIcon;

        public RenderBox suffixIcon {
            get { return _suffixIcon; }
            set { _suffixIcon = _updateChild(_suffixIcon, value, _DecorationSlot.suffixIcon); }
        }

        RenderBox _helperError;

        public RenderBox helperError {
            get { return _helperError; }
            set { _helperError = _updateChild(_helperError, value, _DecorationSlot.helperError); }
        }

        RenderBox _counter;

        public RenderBox counter {
            get { return _counter; }
            set { _counter = _updateChild(_counter, value, _DecorationSlot.counter); }
        }

        RenderBox _container;

        public RenderBox container {
            get { return _container; }
            set { _container = _updateChild(_container, value, _DecorationSlot.container); }
        }

        IEnumerable<RenderBox> _children {
            get {
                if (icon != null) {
                    yield return icon;
                }

                if (input != null) {
                    yield return input;
                }

                if (prefixIcon != null) {
                    yield return prefixIcon;
                }

                if (suffixIcon != null) {
                    yield return suffixIcon;
                }

                if (prefix != null) {
                    yield return prefix;
                }

                if (suffix != null) {
                    yield return suffix;
                }

                if (label != null) {
                    yield return label;
                }

                if (hint != null) {
                    yield return hint;
                }

                if (helperError != null) {
                    yield return helperError;
                }

                if (counter != null) {
                    yield return counter;
                }

                if (container != null) {
                    yield return container;
                }
            }
        }

        public _Decoration decoration {
            get { return _decoration; }
            set {
                D.assert(value != null);
                if (_decoration == value) {
                    return;
                }

                _decoration = value;
                markNeedsLayout();
            }
        }

        _Decoration _decoration;

        public TextBaseline? textBaseline {
            get { return _textBaseline; }
            set {
                D.assert(value != null);
                if (_textBaseline == value) {
                    return;
                }

                _textBaseline = value;
                markNeedsLayout();
            }
        }

        TextBaseline? _textBaseline;

        public bool isFocused {
            get { return _isFocused; }
            set {
                if (_isFocused == value) {
                    return;
                }

                _isFocused = value;
            }
        }

        bool _isFocused;

        public bool expands {
            get { return _expands; }
            set {
                if (_expands == value) {
                    return;
                }

                _expands = value;
                markNeedsLayout();
            }
        }

        bool _expands = false;

        public override void attach(object owner) {
            base.attach(owner);
            foreach (RenderBox child in _children) {
                child.attach(owner);
            }
        }

        public override void detach() {
            base.detach();
            foreach (RenderBox child in _children) {
                child.detach();
            }
        }

        public override void redepthChildren() {
            _children.Each(redepthChild);
        }

        public override void visitChildren(RenderObjectVisitor visitor) {
            _children.Each((child) => { visitor(child); });
        }

        public override List<DiagnosticsNode> debugDescribeChildren() {
            List<DiagnosticsNode> value = new List<DiagnosticsNode> { };

            void add(RenderBox child, string name) {
                if (child != null) {
                    value.Add(child.toDiagnosticsNode(name: name));
                }
            }

            add(icon, "icon");
            add(input, "input");
            add(label, "label");
            add(hint, "hint");
            add(prefix, "prefix");
            add(suffix, "suffix");
            add(prefixIcon, "prefixIcon");
            add(suffixIcon, "suffixIcon");
            add(helperError, "helperError");
            add(counter, "counter");
            add(container, "container");
            return value;
        }

        protected override bool sizedByParent {
            get { return false; }
        }

        static float _minWidth(RenderBox box, float height) {
            return box == null ? 0.0f : box.getMinIntrinsicWidth(height);
        }

        static float _maxWidth(RenderBox box, float height) {
            return box == null ? 0.0f : box.getMaxIntrinsicWidth(height);
        }

        static float _minHeight(RenderBox box, float width) {
            return box == null ? 0.0f : box.getMinIntrinsicHeight(width);
        }

        static Size _boxSize(RenderBox box) {
            return box == null ? Size.zero : box.size;
        }

        static BoxParentData _boxParentData(RenderBox box) {
            return (BoxParentData) box.parentData;
        }

        public EdgeInsets contentPadding {
            get { return decoration.contentPadding; }
        }

        float _layoutLineBox(RenderBox box, BoxConstraints constraints) {
            if (box == null) {
                return 0.0f;
            }

            box.layout(constraints, parentUsesSize: true);
            float baseline = box.getDistanceToBaseline(textBaseline.Value).Value;
            D.assert(baseline >= 0.0f);
            return baseline;
        }

        _RenderDecorationLayout _layout(BoxConstraints layoutConstraints) {
            Dictionary<RenderBox, float> boxToBaseline = new Dictionary<RenderBox, float>();
            BoxConstraints boxConstraints = layoutConstraints.loosen();
            if (prefix != null) {
                boxToBaseline[prefix] = _layoutLineBox(prefix, boxConstraints);
            }

            if (suffix != null) {
                boxToBaseline[suffix] = _layoutLineBox(suffix, boxConstraints);
            }

            if (icon != null) {
                boxToBaseline[icon] = _layoutLineBox(icon, boxConstraints);
            }

            if (prefixIcon != null) {
                boxToBaseline[prefixIcon] = _layoutLineBox(prefixIcon, boxConstraints);
            }

            if (suffixIcon != null) {
                boxToBaseline[suffixIcon] = _layoutLineBox(suffixIcon, boxConstraints);
            }

            float inputWidth = Math.Max(0.0f, constraints.maxWidth - (
                _boxSize(icon).width
                + contentPadding.left
                + _boxSize(prefixIcon).width
                + _boxSize(prefix).width
                + _boxSize(suffix).width
                + _boxSize(suffixIcon).width
                + contentPadding.right));
            if (label != null) {
                boxToBaseline[label] = _layoutLineBox(label,
                    boxConstraints.copyWith(maxWidth: inputWidth)
                );
            }

            if (hint != null) {
                boxToBaseline[hint] = _layoutLineBox(hint,
                    boxConstraints.copyWith(minWidth: inputWidth, maxWidth: inputWidth)
                );
            }

            if (counter != null) {
                boxToBaseline[counter] = _layoutLineBox(counter, boxConstraints);
            }

            if (helperError != null) {
                boxToBaseline[helperError] = _layoutLineBox(helperError,
                    boxConstraints.copyWith(
                        maxWidth: Math.Max(0.0f, boxConstraints.maxWidth
                                                 - _boxSize(icon).width
                                                 - _boxSize(counter).width
                                                 - contentPadding.horizontal
                        )
                    )
                );
            }

            float labelHeight = label == null
                ? 0
                : decoration.floatingLabelHeight;
            float topHeight = decoration.border.isOutline
                ? Math.Max(labelHeight - boxToBaseline.getOrDefault(label, 0), 0)
                : labelHeight;
            float counterHeight = counter == null
                ? 0
                : boxToBaseline.getOrDefault(counter, 0) + subtextGap;
            bool helperErrorExists = helperError?.size != null
                                     && helperError.size.height > 0;
            float helperErrorHeight = !helperErrorExists
                ? 0
                : helperError.size.height + subtextGap;
            float bottomHeight = Math.Max(
                counterHeight,
                helperErrorHeight
            );
            if (input != null) {
                boxToBaseline[input] = _layoutLineBox(input,
                    boxConstraints.deflate(EdgeInsets.only(
                        top: contentPadding.top + topHeight,
                        bottom: contentPadding.bottom + bottomHeight
                    )).copyWith(
                        minWidth: inputWidth,
                        maxWidth: inputWidth
                    )
                );
            }

            // The field can be occupied by a hint or by the input itself
            float hintHeight = hint == null ? 0 : hint.size.height;
            float inputDirectHeight = input == null ? 0 : input.size.height;
            float inputHeight = Math.Max(hintHeight, inputDirectHeight);
            float inputInternalBaseline = Math.Max(
                boxToBaseline.getOrDefault(input, 0.0f),
                boxToBaseline.getOrDefault(hint, 0.0f)
            );

            // Calculate the amount that prefix/suffix affects height above and below
            // the input.
            float prefixHeight = prefix == null ? 0 : prefix.size.height;
            float suffixHeight = suffix == null ? 0 : suffix.size.height;
            float fixHeight = Math.Max(
                boxToBaseline.getOrDefault(prefix, 0.0f),
                boxToBaseline.getOrDefault(suffix, 0.0f)
            );
            float fixAboveInput = Math.Max(0, fixHeight - inputInternalBaseline);
            float fixBelowBaseline = Math.Max(
                prefixHeight - boxToBaseline.getOrDefault(prefix, 0.0f),
                suffixHeight - boxToBaseline.getOrDefault(suffix, 0.0f)
            );
            float fixBelowInput = Math.Max(
                0,
                fixBelowBaseline - (inputHeight - inputInternalBaseline)
            );

            // Calculate the height of the input text container.
            float prefixIconHeight = prefixIcon == null ? 0 : prefixIcon.size.height;
            float suffixIconHeight = suffixIcon == null ? 0 : suffixIcon.size.height;
            float fixIconHeight = Math.Max(prefixIconHeight, suffixIconHeight);
            float contentHeight = Math.Max(
                fixIconHeight,
                topHeight
                + contentPadding.top
                + fixAboveInput
                + inputHeight
                + fixBelowInput
                + contentPadding.bottom
            );
            float maxContainerHeight = boxConstraints.maxHeight - bottomHeight;
            float containerHeight = expands
                ? maxContainerHeight
                : Math.Min(contentHeight, maxContainerHeight);

            // Always position the prefix/suffix in the same place (baseline).
            float overflow = Math.Max(0, contentHeight - maxContainerHeight);
            float baselineAdjustment = fixAboveInput - overflow;

            // The baselines that will be used to draw the actual input text content.
            float inputBaseline = contentPadding.top
                                  + topHeight
                                  + inputInternalBaseline
                                  + baselineAdjustment;
            // The text in the input when an outline border is present is centered
            // within the container less 2.0 dps at the top to account for the vertical
            // space occupied by the floating label.
            float outlineBaseline = inputInternalBaseline
                                    + baselineAdjustment / 2
                                    + (containerHeight - (2.0f + inputHeight)) / 2.0f;

            // Find the positions of the text below the input when it exists.
            float subtextCounterBaseline = 0;
            float subtextHelperBaseline = 0;
            float subtextCounterHeight = 0;
            float subtextHelperHeight = 0;
            if (counter != null) {
                subtextCounterBaseline =
                    containerHeight + subtextGap + boxToBaseline.getOrDefault(counter, 0.0f);
                subtextCounterHeight = counter.size.height + subtextGap;
            }

            if (helperErrorExists) {
                subtextHelperBaseline =
                    containerHeight + subtextGap + boxToBaseline.getOrDefault(helperError, 0.0f);
                subtextHelperHeight = helperErrorHeight;
            }

            float subtextBaseline = Math.Max(
                subtextCounterBaseline,
                subtextHelperBaseline
            );
            float subtextHeight = Math.Max(
                subtextCounterHeight,
                subtextHelperHeight
            );

            return new _RenderDecorationLayout(
                boxToBaseline: boxToBaseline,
                containerHeight: containerHeight,
                inputBaseline: inputBaseline,
                outlineBaseline: outlineBaseline,
                subtextBaseline: subtextBaseline,
                subtextHeight: subtextHeight
            );
        }

        protected override float computeMinIntrinsicWidth(float height) {
            return _minWidth(icon, height)
                   + contentPadding.left
                   + _minWidth(prefixIcon, height)
                   + _minWidth(prefix, height)
                   + Mathf.Max(_minWidth(input, height), _minWidth(hint, height))
                   + _minWidth(suffix, height)
                   + _minWidth(suffixIcon, height)
                   + contentPadding.right;
        }

        protected override float computeMaxIntrinsicWidth(float height) {
            return _maxWidth(icon, height)
                   + contentPadding.left
                   + _maxWidth(prefixIcon, height)
                   + _maxWidth(prefix, height)
                   + Mathf.Max(_maxWidth(input, height), _maxWidth(hint, height))
                   + _maxWidth(suffix, height)
                   + _maxWidth(suffixIcon, height)
                   + contentPadding.right;
        }

        float _lineHeight(float width, List<RenderBox> boxes) {
            float height = 0.0f;
            foreach (RenderBox box in boxes) {
                if (box == null) {
                    continue;
                }

                height = Mathf.Max(_minHeight(box, width), height);
            }

            return height;
        }

        protected override float computeMinIntrinsicHeight(float width) {
            float subtextHeight = _lineHeight(width, new List<RenderBox> {helperError, counter});
            if (subtextHeight > 0.0f) {
                subtextHeight += subtextGap;
            }

            return contentPadding.top
                   + (label == null ? 0.0f : decoration.floatingLabelHeight)
                   + _lineHeight(width, new List<RenderBox> {prefix, input, suffix})
                   + subtextHeight
                   + contentPadding.bottom;
        }

        protected internal override float computeMaxIntrinsicHeight(float width) {
            return computeMinIntrinsicHeight(width);
        }

        protected override float? computeDistanceToActualBaseline(TextBaseline baseline) {
            return _boxParentData(input).offset.dy + input.getDistanceToActualBaseline(baseline);
        }

        Matrix4 _labelTransform;

        protected override void performLayout() {
            _labelTransform = null;
            _RenderDecorationLayout layout = _layout(constraints);

            float overallWidth = constraints.maxWidth;
            float? overallHeight = layout.containerHeight + layout.subtextHeight;

            if (container != null) {
                BoxConstraints containerConstraints = BoxConstraints.tightFor(
                    height: layout.containerHeight,
                    width: overallWidth - _boxSize(icon).width
                );
                container.layout(containerConstraints, parentUsesSize: true);
                float x = _boxSize(icon).width;
                _boxParentData(container).offset = new Offset(x, 0.0f);
            }

            float height;

            float centerLayout(RenderBox box, float x) {
                _boxParentData(box).offset = new Offset(x, (height - box.size.height) / 2.0f);
                return box.size.width;
            }

            float baseline;

            float baselineLayout(RenderBox box, float x) {
                _boxParentData(box).offset = new Offset(x, baseline - layout.boxToBaseline[box]);
                return box.size.width;
            }

            float left = contentPadding.left;
            float right = overallWidth - contentPadding.right;

            height = layout.containerHeight ?? 0.0f;
            baseline = (decoration.isCollapsed || !decoration.border.isOutline
                ? layout.inputBaseline
                : layout.outlineBaseline) ?? 0.0f;

            if (icon != null) {
                float x = 0.0f;

                centerLayout(icon, x);
            }

            float start = left + _boxSize(icon).width;
            float end = right;
            if (prefixIcon != null) {
                start -= contentPadding.left;
                start += centerLayout(prefixIcon, start);
            }

            if (label != null) {
                if (decoration.alignLabelWithHint == true) {
                    baselineLayout(label, start);
                }
                else {
                    centerLayout(label, start);
                }
            }

            if (prefix != null) {
                start += baselineLayout(prefix, start);
            }

            if (input != null) {
                baselineLayout(input, start);
            }

            if (hint != null) {
                baselineLayout(hint, start);
            }

            if (suffixIcon != null) {
                end += contentPadding.right;
                end -= centerLayout(suffixIcon, end - suffixIcon.size.width);
            }

            if (suffix != null) {
                end -= baselineLayout(suffix, end - suffix.size.width);
            }

            if (helperError != null || counter != null) {
                height = layout.subtextHeight ?? 0.0f;
                baseline = layout.subtextBaseline ?? 0.0f;

                if (helperError != null) {
                    baselineLayout(helperError, left + _boxSize(icon).width);
                }

                if (counter != null) {
                    baselineLayout(counter, right - counter.size.width);
                }
            }

            if (label != null) {
                float labelX = _boxParentData(label).offset.dx;
                decoration.borderGap.start = labelX - _boxSize(icon).width;

                decoration.borderGap.extent = label.size.width * 0.75f;
            }
            else {
                decoration.borderGap.start = 0.0f;
                decoration.borderGap.extent = 0.0f;
            }

            size = constraints.constrain(new Size(overallWidth, overallHeight ?? 0.0f));
            D.assert(size.width == constraints.constrainWidth(overallWidth));
            D.assert(size.height == constraints.constrainHeight(overallHeight ?? 0.0f));
        }

        void _paintLabel(PaintingContext context, Offset offset) {
            context.paintChild(label, offset);
        }

        public override void paint(PaintingContext context, Offset offset) {
            void doPaint(RenderBox child) {
                if (child != null) {
                    context.paintChild(child, _boxParentData(child).offset + offset);
                }
            }

            doPaint(container);

            if (label != null) {
                Offset labelOffset = _boxParentData(label).offset;
                float labelHeight = label.size.height;
                float t = decoration.floatingLabelProgress;
                bool isOutlineBorder = decoration.border != null && decoration.border.isOutline;
                float floatingY = isOutlineBorder ? -labelHeight * 0.25f : contentPadding.top;
                float scale = MathUtils.lerpFloat(1.0f, 0.75f, t);
                float dx = labelOffset.dx;
                float dy = MathUtils.lerpFloat(0.0f, floatingY - labelOffset.dy, t);
                _labelTransform = Matrix4.identity();
                _labelTransform.translate(dx, labelOffset.dy + dy);
                _labelTransform.scale(scale, scale, 1);
                context.pushTransform(needsCompositing, offset, _labelTransform, _paintLabel);
            }

            doPaint(icon);
            doPaint(prefix);
            doPaint(suffix);
            doPaint(prefixIcon);
            doPaint(suffixIcon);
            doPaint(hint);
            doPaint(input);
            doPaint(helperError);
            doPaint(counter);
        }

        protected override bool hitTestSelf(Offset position) {
            return true;
        }

        protected override bool hitTestChildren(BoxHitTestResult result, Offset position) {
            D.assert(position != null);
            foreach (RenderBox child in _children) {
                Offset offset = _boxParentData(child).offset;
                bool isHit = result.addWithPaintOffset(
                    offset: offset,
                    position: position,
                    hitTest: (BoxHitTestResult resultIn, Offset transformed) => {
                        D.assert(transformed == position - offset);
                        return child.hitTest(resultIn, position: transformed);
                    }
                );
                if (isHit) {
                    return true;
                }
            }

            return false;
        }

        public override void applyPaintTransform(RenderObject child, Matrix4 transform) {
            if (child == label && _labelTransform != null) {
                Offset labelOffset = _boxParentData(label).offset;
                transform.multiply(_labelTransform);
                transform.translate(-labelOffset.dx, -labelOffset.dy);
            }

            base.applyPaintTransform(child, transform);
        }
    }

    class _RenderDecorationElement : RenderObjectElement {
        public _RenderDecorationElement(_Decorator widget) : base(widget) {
        }

        Dictionary<_DecorationSlot, Element> slotToChild = new Dictionary<_DecorationSlot, Element>();
        Dictionary<Element, _DecorationSlot> childToSlot = new Dictionary<Element, _DecorationSlot>();

        public new _Decorator widget {
            get { return (_Decorator) base.widget; }
        }

        public new _RenderDecoration renderObject {
            get { return (_RenderDecoration) base.renderObject; }
        }

        public override void visitChildren(ElementVisitor visitor) {
            slotToChild.Values.Each((child) => { visitor(child); });
        }

        internal override void forgetChild(Element child) {
            D.assert(slotToChild.ContainsValue(child));
            D.assert(childToSlot.ContainsKey(child));
            _DecorationSlot slot = childToSlot[child];
            childToSlot.Remove(child);
            slotToChild.Remove(slot);
        }

        void _mountChild(Widget widget, _DecorationSlot slot) {
            Element oldChild = slotToChild.getOrDefault(slot);
            Element newChild = updateChild(oldChild, widget, slot);
            if (oldChild != null) {
                slotToChild.Remove(slot);
                childToSlot.Remove(oldChild);
            }

            if (newChild != null) {
                slotToChild[slot] = newChild;
                childToSlot[newChild] = slot;
            }
        }

        public override void mount(Element parent, object newSlot) {
            base.mount(parent, newSlot);
            _mountChild(widget.decoration.icon, _DecorationSlot.icon);
            _mountChild(widget.decoration.input, _DecorationSlot.input);
            _mountChild(widget.decoration.label, _DecorationSlot.label);
            _mountChild(widget.decoration.hint, _DecorationSlot.hint);
            _mountChild(widget.decoration.prefix, _DecorationSlot.prefix);
            _mountChild(widget.decoration.suffix, _DecorationSlot.suffix);
            _mountChild(widget.decoration.prefixIcon, _DecorationSlot.prefixIcon);
            _mountChild(widget.decoration.suffixIcon, _DecorationSlot.suffixIcon);
            _mountChild(widget.decoration.helperError, _DecorationSlot.helperError);
            _mountChild(widget.decoration.counter, _DecorationSlot.counter);
            _mountChild(widget.decoration.container, _DecorationSlot.container);
        }

        void _updateChild(Widget widget, _DecorationSlot slot) {
            Element oldChild = slotToChild.getOrDefault(slot);
            Element newChild = updateChild(oldChild, widget, slot);
            if (oldChild != null) {
                childToSlot.Remove(oldChild);
                slotToChild.Remove(slot);
            }

            if (newChild != null) {
                slotToChild[slot] = newChild;
                childToSlot[newChild] = slot;
            }
        }

        public override void update(Widget newWidget) {
            base.update(newWidget);
            D.assert(widget == newWidget);
            _updateChild(widget.decoration.icon, _DecorationSlot.icon);
            _updateChild(widget.decoration.input, _DecorationSlot.input);
            _updateChild(widget.decoration.label, _DecorationSlot.label);
            _updateChild(widget.decoration.hint, _DecorationSlot.hint);
            _updateChild(widget.decoration.prefix, _DecorationSlot.prefix);
            _updateChild(widget.decoration.suffix, _DecorationSlot.suffix);
            _updateChild(widget.decoration.prefixIcon, _DecorationSlot.prefixIcon);
            _updateChild(widget.decoration.suffixIcon, _DecorationSlot.suffixIcon);
            _updateChild(widget.decoration.helperError, _DecorationSlot.helperError);
            _updateChild(widget.decoration.counter, _DecorationSlot.counter);
            _updateChild(widget.decoration.container, _DecorationSlot.container);
        }

        void _updateRenderObject(RenderObject child, _DecorationSlot slot) {
            switch (slot) {
                case _DecorationSlot.icon:
                    renderObject.icon = (RenderBox) child;
                    break;
                case _DecorationSlot.input:
                    renderObject.input = (RenderBox) child;
                    break;
                case _DecorationSlot.label:
                    renderObject.label = (RenderBox) child;
                    break;
                case _DecorationSlot.hint:
                    renderObject.hint = (RenderBox) child;
                    break;
                case _DecorationSlot.prefix:
                    renderObject.prefix = (RenderBox) child;
                    break;
                case _DecorationSlot.suffix:
                    renderObject.suffix = (RenderBox) child;
                    break;
                case _DecorationSlot.prefixIcon:
                    renderObject.prefixIcon = (RenderBox) child;
                    break;
                case _DecorationSlot.suffixIcon:
                    renderObject.suffixIcon = (RenderBox) child;
                    break;
                case _DecorationSlot.helperError:
                    renderObject.helperError = (RenderBox) child;
                    break;
                case _DecorationSlot.counter:
                    renderObject.counter = (RenderBox) child;
                    break;
                case _DecorationSlot.container:
                    renderObject.container = (RenderBox) child;
                    break;
            }
        }

        protected override void insertChildRenderObject(RenderObject child, object slotValue) {
            D.assert(child is RenderBox);
            D.assert(slotValue is _DecorationSlot);
            _DecorationSlot slot = (_DecorationSlot) slotValue;
            _updateRenderObject(child, slot);
            D.assert(renderObject.childToSlot.ContainsKey((RenderBox) child));
            D.assert(renderObject.slotToChild.ContainsKey(slot));
        }

        protected override void removeChildRenderObject(RenderObject child) {
            D.assert(child is RenderBox);
            D.assert(renderObject.childToSlot.ContainsKey((RenderBox) child));
            var slot = renderObject.childToSlot[(RenderBox) child];
            _updateRenderObject(null, renderObject.childToSlot[(RenderBox) child]);
            D.assert(!renderObject.childToSlot.ContainsKey((RenderBox) child));
            D.assert(!renderObject.slotToChild.ContainsKey(slot));
        }

        protected override void moveChildRenderObject(RenderObject child, object slotValue) {
            D.assert(false, () => "not reachable");
        }
    }

    class _Decorator : RenderObjectWidget {
        public _Decorator(
            Key key = null,
            _Decoration decoration = null,
            TextBaseline? textBaseline = null,
            bool isFocused = false,
            bool? expands = null
        ) : base(key: key) {
            D.assert(decoration != null);
            D.assert(textBaseline != null);
            D.assert(expands != null);
            this.decoration = decoration;
            this.textBaseline = textBaseline;
            this.isFocused = isFocused;
            this.expands = expands.Value;
        }

        public readonly _Decoration decoration;
        public readonly TextBaseline? textBaseline;
        public readonly bool isFocused;
        public readonly bool expands;

        public override Element createElement() {
            return new _RenderDecorationElement(this);
        }

        public override RenderObject createRenderObject(BuildContext context) {
            return new _RenderDecoration(
                decoration: decoration,
                textBaseline: textBaseline,
                isFocused: isFocused,
                expands: expands
            );
        }

        public override void updateRenderObject(BuildContext context, RenderObject _renderObject) {
            _RenderDecoration renderObject = _renderObject as _RenderDecoration;
            renderObject.decoration = decoration;
            renderObject.textBaseline = textBaseline;
            renderObject.isFocused = isFocused;
            renderObject.expands = expands;
        }
    }

    class _AffixText : StatelessWidget {
        public _AffixText(
            bool labelIsFloating = false,
            string text = null,
            TextStyle style = null,
            Widget child = null
        ) {
            this.labelIsFloating = labelIsFloating;
            this.text = text;
            this.style = style;
            this.child = child;
        }

        public readonly bool labelIsFloating;
        public readonly string text;
        public readonly TextStyle style;
        public readonly Widget child;

        public override Widget build(BuildContext context) {
            return DefaultTextStyle.merge(
                style: style,
                child: new AnimatedOpacity(
                    duration: InputDecoratorConstants._kTransitionDuration,
                    curve: InputDecoratorConstants._kTransitionCurve,
                    opacity: labelIsFloating ? 1.0f : 0.0f,
                    child: child ?? new Text(text, style: style)
                )
            );
        }
    }

    public class InputDecorator : StatefulWidget {
        public InputDecorator(
            Key key = null,
            InputDecoration decoration = null,
            TextStyle baseStyle = null,
            TextAlign? textAlign = null,
            bool isFocused = false,
            bool expands = false,
            bool isEmpty = false,
            Widget child = null
        ) : base(key: key) {
            this.decoration = decoration;
            this.baseStyle = baseStyle;
            this.textAlign = textAlign;
            this.isFocused = isFocused;
            this.expands = expands;
            this.isEmpty = isEmpty;
            this.child = child;
        }

        public readonly InputDecoration decoration;

        public readonly TextStyle baseStyle;

        public readonly TextAlign? textAlign;

        public readonly bool isFocused;

        public readonly bool expands;

        public readonly bool isEmpty;

        public readonly Widget child;

        public bool _labelShouldWithdraw {
            get { return !isEmpty || isFocused; }
        }

        public override State createState() {
            return new _InputDecoratorState();
        }

        internal static RenderBox containerOf(BuildContext context) {
            _RenderDecoration result =
                (_RenderDecoration) context.ancestorRenderObjectOfType(new TypeMatcher<_RenderDecoration>());
            return result?.container;
        }

        public override void debugFillProperties(DiagnosticPropertiesBuilder properties) {
            base.debugFillProperties(properties);
            properties.add(new DiagnosticsProperty<InputDecoration>("decoration", decoration));
            properties.add(new DiagnosticsProperty<TextStyle>("baseStyle", baseStyle, defaultValue: null));
            properties.add(new DiagnosticsProperty<bool>("isFocused", isFocused));
            properties.add(new DiagnosticsProperty<bool>("expands", expands));
            properties.add(new DiagnosticsProperty<bool>("isEmpty", isEmpty));
        }
    }

    class _InputDecoratorState : TickerProviderStateMixin<InputDecorator> {
        AnimationController _floatingLabelController;
        AnimationController _shakingLabelController;
        _InputBorderGap _borderGap = new _InputBorderGap();

        public override void initState() {
            base.initState();
            _floatingLabelController = new AnimationController(
                duration: InputDecoratorConstants._kTransitionDuration,
                vsync: this,
                value: (widget.decoration.hasFloatingPlaceholder == true && widget._labelShouldWithdraw)
                    ? 1.0f
                    : 0.0f
            );
            _floatingLabelController.addListener(_handleChange);

            _shakingLabelController = new AnimationController(
                duration: InputDecoratorConstants._kTransitionDuration,
                vsync: this
            );
        }

        public override void didChangeDependencies() {
            base.didChangeDependencies();
            _effectiveDecoration = null;
        }

        public override void dispose() {
            _floatingLabelController.dispose();
            _shakingLabelController.dispose();
            base.dispose();
        }

        void _handleChange() {
            setState(() => { });
        }

        InputDecoration _effectiveDecoration;

        public InputDecoration decoration {
            get {
                _effectiveDecoration = _effectiveDecoration ?? widget.decoration.applyDefaults(
                    Theme.of(context).inputDecorationTheme
                );
                return _effectiveDecoration;
            }
        }

        TextAlign? textAlign {
            get { return widget.textAlign; }
        }

        bool isFocused {
            get { return widget.isFocused; }
        }

        bool isEmpty {
            get { return widget.isEmpty; }
        }

        public override void didUpdateWidget(StatefulWidget _old) {
            base.didUpdateWidget(_old);
            InputDecorator old = _old as InputDecorator;
            if (widget.decoration != old.decoration) {
                _effectiveDecoration = null;
            }

            if (widget._labelShouldWithdraw != old._labelShouldWithdraw &&
                widget.decoration.hasFloatingPlaceholder == true) {
                if (widget._labelShouldWithdraw) {
                    _floatingLabelController.forward();
                }
                else {
                    _floatingLabelController.reverse();
                }
            }

            string errorText = decoration.errorText;
            string oldErrorText = old.decoration.errorText;

            if (_floatingLabelController.isCompleted && errorText != null && errorText != oldErrorText) {
                _shakingLabelController.setValue(0.0f);
                _shakingLabelController.forward();
            }
        }

        Color _getActiveColor(ThemeData themeData) {
            if (isFocused) {
                switch (themeData.brightness) {
                    case Brightness.dark:
                        return themeData.accentColor;
                    case Brightness.light:
                        return themeData.primaryColor;
                }
            }

            return themeData.hintColor;
        }

        Color _getFillColor(ThemeData themeData) {
            if (decoration.filled != true) {
                return Colors.transparent;
            }

            if (decoration.fillColor != null) {
                return decoration.fillColor;
            }

            Color darkEnabled = new Color(0x1AFFFFFF);
            Color darkDisabled = new Color(0x0DFFFFFF);
            Color lightEnabled = new Color(0x0A000000);
            Color lightDisabled = new Color(0x05000000);

            switch (themeData.brightness) {
                case Brightness.dark:
                    return decoration.enabled == true ? darkEnabled : darkDisabled;
                case Brightness.light:
                    return decoration.enabled == true ? lightEnabled : lightDisabled;
            }

            return lightEnabled;
        }

        Color _getDefaultIconColor(ThemeData themeData) {
            if (!decoration.enabled == true) {
                return themeData.disabledColor;
            }

            switch (themeData.brightness) {
                case Brightness.dark:
                    return Colors.white70;
                case Brightness.light:
                    return Colors.black45;
                default:
                    return themeData.iconTheme.color;
            }
        }

        bool _hasInlineLabel {
            get { return !widget._labelShouldWithdraw && decoration.labelText != null; }
        }

        bool _shouldShowLabel {
            get { return _hasInlineLabel || decoration.hasFloatingPlaceholder == true; }
        }


        TextStyle _getInlineStyle(ThemeData themeData) {
            return themeData.textTheme.subhead.merge(widget.baseStyle)
                .copyWith(color: decoration.enabled == true ? themeData.hintColor : themeData.disabledColor);
        }

        TextStyle _getFloatingLabelStyle(ThemeData themeData) {
            Color color = decoration.errorText != null
                ? decoration.errorStyle?.color ?? themeData.errorColor
                : _getActiveColor(themeData);
            TextStyle style = themeData.textTheme.subhead.merge(widget.baseStyle);
            return style
                .copyWith(color: decoration.enabled == true ? color : themeData.disabledColor)
                .merge(decoration.labelStyle);
        }

        TextStyle _getHelperStyle(ThemeData themeData) {
            Color color = decoration.enabled == true ? themeData.hintColor : Colors.transparent;
            return themeData.textTheme.caption.copyWith(color: color).merge(decoration.helperStyle);
        }

        TextStyle _getErrorStyle(ThemeData themeData) {
            Color color = decoration.enabled == true ? themeData.errorColor : Colors.transparent;
            return themeData.textTheme.caption.copyWith(color: color).merge(decoration.errorStyle);
        }

        InputBorder _getDefaultBorder(ThemeData themeData) {
            if (decoration.border?.borderSide == BorderSide.none) {
                return decoration.border;
            }

            Color borderColor;
            if (decoration.enabled == true) {
                borderColor = decoration.errorText == null
                    ? _getActiveColor(themeData)
                    : themeData.errorColor;
            }
            else {
                borderColor = (decoration.filled == true && decoration.border?.isOutline != true)
                    ? Colors.transparent
                    : themeData.disabledColor;
            }

            float borderWeight;
            if (decoration.isCollapsed || decoration?.border == InputBorder.none ||
                !decoration.enabled == true) {
                borderWeight = 0.0f;
            }
            else {
                borderWeight = isFocused ? 2.0f : 1.0f;
            }

            InputBorder border = decoration.border ?? new UnderlineInputBorder();
            return border.copyWith(borderSide: new BorderSide(color: borderColor, width: borderWeight));
        }

        public override Widget build(BuildContext context) {
            ThemeData themeData = Theme.of(context);
            TextStyle inlineStyle = _getInlineStyle(themeData);
            TextBaseline? textBaseline = inlineStyle.textBaseline;

            TextStyle hintStyle = inlineStyle.merge(decoration.hintStyle);
            Widget hint = decoration.hintText == null
                ? null
                : new AnimatedOpacity(
                    opacity: (isEmpty && !_hasInlineLabel) ? 1.0f : 0.0f,
                    duration: InputDecoratorConstants._kTransitionDuration,
                    curve: InputDecoratorConstants._kTransitionCurve,
                    child: new Text(decoration.hintText,
                        style: hintStyle,
                        overflow: TextOverflow.ellipsis,
                        textAlign: textAlign,
                        maxLines: decoration.hintMaxLines
                    )
                );

            bool isError = decoration.errorText != null;
            InputBorder border;
            if (!decoration.enabled == true) {
                border = isError ? decoration.errorBorder : decoration.disabledBorder;
            }
            else if (isFocused) {
                border = isError ? decoration.focusedErrorBorder : decoration.focusedBorder;
            }
            else {
                border = isError ? decoration.errorBorder : decoration.enabledBorder;
            }

            border = border ?? _getDefaultBorder(themeData);

            Widget container = new _BorderContainer(
                border: border,
                gap: _borderGap,
                gapAnimation: _floatingLabelController.view,
                fillColor: _getFillColor(themeData)
            );

            TextStyle inlineLabelStyle = inlineStyle.merge(decoration.labelStyle);
            Widget label = decoration.labelText == null
                ? null
                : new _Shaker(
                    animation: _shakingLabelController.view,
                    child: new AnimatedOpacity(
                        duration: InputDecoratorConstants._kTransitionDuration,
                        curve: InputDecoratorConstants._kTransitionCurve,
                        opacity: _shouldShowLabel ? 1.0f : 0.0f,
                        child: new AnimatedDefaultTextStyle(
                            duration: InputDecoratorConstants._kTransitionDuration,
                            curve: InputDecoratorConstants._kTransitionCurve,
                            style: widget._labelShouldWithdraw
                                ? _getFloatingLabelStyle(themeData)
                                : inlineLabelStyle,
                            child: new Text(decoration.labelText,
                                overflow: TextOverflow.ellipsis,
                                textAlign: textAlign
                            )
                        )
                    )
                );

            Widget prefix = decoration.prefix == null && decoration.prefixText == null
                ? null
                : new _AffixText(
                    labelIsFloating: widget._labelShouldWithdraw,
                    text: decoration.prefixText,
                    style: decoration.prefixStyle ?? hintStyle,
                    child: decoration.prefix
                );

            Widget suffix = decoration.suffix == null && decoration.suffixText == null
                ? null
                : new _AffixText(
                    labelIsFloating: widget._labelShouldWithdraw,
                    text: decoration.suffixText,
                    style: decoration.suffixStyle ?? hintStyle,
                    child: decoration.suffix
                );

            Color activeColor = _getActiveColor(themeData);
            bool decorationIsDense = decoration.isDense == true;
            float iconSize = decorationIsDense ? 18.0f : 24.0f;
            Color iconColor = isFocused ? activeColor : _getDefaultIconColor(themeData);

            Widget icon = decoration.icon == null
                ? null
                : new Padding(
                    padding: EdgeInsets.only(right: 16.0f),
                    child: IconTheme.merge(
                        data: new IconThemeData(
                            color: iconColor,
                            size: iconSize
                        ),
                        child: decoration.icon
                    )
                );

            Widget prefixIcon = decoration.prefixIcon == null
                ? null
                : new Center(
                    widthFactor: 1.0f,
                    heightFactor: 1.0f,
                    child: new ConstrainedBox(
                        constraints: new BoxConstraints(minWidth: 48.0f, minHeight: 48.0f),
                        child: IconTheme.merge(
                            data: new IconThemeData(
                                color: iconColor,
                                size: iconSize
                            ),
                            child: decoration.prefixIcon
                        )
                    )
                );

            Widget suffixIcon = decoration.suffixIcon == null
                ? null
                : new Center(
                    widthFactor: 1.0f,
                    heightFactor: 1.0f,
                    child: new ConstrainedBox(
                        constraints: new BoxConstraints(minWidth: 48.0f, minHeight: 48.0f),
                        child: IconTheme.merge(
                            data: new IconThemeData(
                                color: iconColor,
                                size: iconSize
                            ),
                            child: decoration.suffixIcon
                        )
                    )
                );

            Widget helperError = new _HelperError(
                textAlign: textAlign,
                helperText: decoration.helperText,
                helperStyle: _getHelperStyle(themeData),
                errorText: decoration.errorText,
                errorStyle: _getErrorStyle(themeData),
                errorMaxLines: decoration.errorMaxLines
            );

            Widget counter = null;
            if (decoration.counter != null) {
                counter = decoration.counter;
            }
            else if (decoration.counterText != null && decoration.counterText != "") {
                counter = new Text(decoration.counterText,
                    style: _getHelperStyle(themeData).merge(decoration.counterStyle),
                    overflow: TextOverflow.ellipsis
                );
            }

            EdgeInsets decorationContentPadding = decoration.contentPadding;
            EdgeInsets contentPadding;
            float? floatingLabelHeight;
            if (decoration.isCollapsed) {
                floatingLabelHeight = 0.0f;
                contentPadding = decorationContentPadding ?? EdgeInsets.zero;
            }
            else if (!border.isOutline) {
                floatingLabelHeight =
                    (4.0f + 0.75f * inlineLabelStyle.fontSize) * MediaQuery.textScaleFactorOf(context);
                if (decoration.filled == true) {
                    contentPadding = decorationContentPadding ?? (decorationIsDense
                        ? EdgeInsets.fromLTRB(12.0f, 8.0f, 12.0f, 8.0f)
                        : EdgeInsets.fromLTRB(12.0f, 12.0f, 12.0f, 12.0f));
                }
                else {
                    contentPadding = decorationContentPadding ?? (decorationIsDense
                        ? EdgeInsets.fromLTRB(0.0f, 8.0f, 0.0f, 8.0f)
                        : EdgeInsets.fromLTRB(0.0f, 12.0f, 0.0f, 12.0f));
                }
            }
            else {
                floatingLabelHeight = 0.0f;
                contentPadding = decorationContentPadding ?? (decorationIsDense
                    ? EdgeInsets.fromLTRB(12.0f, 20.0f, 12.0f, 12.0f)
                    : EdgeInsets.fromLTRB(12.0f, 24.0f, 12.0f, 16.0f));
            }

            return new _Decorator(
                decoration: new _Decoration(
                    contentPadding: contentPadding,
                    isCollapsed: decoration.isCollapsed,
                    floatingLabelHeight: floatingLabelHeight ?? 0.0f,
                    floatingLabelProgress: _floatingLabelController.value,
                    border: border,
                    borderGap: _borderGap,
                    icon: icon,
                    input: widget.child,
                    label: label,
                    alignLabelWithHint: decoration.alignLabelWithHint,
                    hint: hint,
                    prefix: prefix,
                    suffix: suffix,
                    prefixIcon: prefixIcon,
                    suffixIcon: suffixIcon,
                    helperError: helperError,
                    counter: counter,
                    container: container
                ),
                textBaseline: textBaseline,
                isFocused: isFocused,
                expands: widget.expands
            );
        }
    }

    public class InputDecoration {
        public InputDecoration(
            Widget icon = null,
            string labelText = null,
            TextStyle labelStyle = null,
            string helperText = null,
            TextStyle helperStyle = null,
            string hintText = null,
            TextStyle hintStyle = null,
            int? hintMaxLines = null,
            string errorText = null,
            TextStyle errorStyle = null,
            int? errorMaxLines = null,
            bool? hasFloatingPlaceholder = true,
            bool? isDense = null,
            EdgeInsets contentPadding = null,
            Widget prefixIcon = null,
            Widget prefix = null,
            string prefixText = null,
            TextStyle prefixStyle = null,
            Widget suffixIcon = null,
            Widget suffix = null,
            string suffixText = null,
            TextStyle suffixStyle = null,
            Widget counter = null,
            string counterText = null,
            TextStyle counterStyle = null,
            bool? filled = null,
            Color fillColor = null,
            InputBorder errorBorder = null,
            InputBorder focusedBorder = null,
            InputBorder focusedErrorBorder = null,
            InputBorder disabledBorder = null,
            InputBorder enabledBorder = null,
            InputBorder border = null,
            bool? enabled = true,
            bool? alignLabelWithHint = null
        ) {
            D.assert(enabled != null);
            D.assert(!(prefix != null && prefixText != null),
                () => "Declaring both prefix and prefixText is not supported");
            D.assert(!(suffix != null && suffixText != null),
                () => "Declaring both suffix and suffixText is not supported");
            isCollapsed = false;
            this.icon = icon;
            this.labelText = labelText;
            this.labelStyle = labelStyle;
            this.helperText = helperText;
            this.helperStyle = helperStyle;
            this.hintText = hintText;
            this.hintStyle = hintStyle;
            this.hintMaxLines = hintMaxLines;
            this.errorText = errorText;
            this.errorStyle = errorStyle;
            this.errorMaxLines = errorMaxLines;
            this.hasFloatingPlaceholder = hasFloatingPlaceholder;
            this.isDense = isDense;
            this.contentPadding = contentPadding;
            this.prefix = prefix;
            this.prefixText = prefixText;
            this.prefixIcon = prefixIcon;
            this.prefixStyle = prefixStyle;
            this.suffix = suffix;
            this.suffixText = suffixText;
            this.suffixIcon = suffixIcon;
            this.suffixStyle = suffixStyle;
            this.counter = counter;
            this.counterText = counterText;
            this.counterStyle = counterStyle;
            this.filled = filled;
            this.fillColor = fillColor;
            this.errorBorder = errorBorder;
            this.focusedBorder = focusedBorder;
            this.focusedErrorBorder = focusedErrorBorder;
            this.disabledBorder = disabledBorder;
            this.enabledBorder = enabledBorder;
            this.border = border;
            this.enabled = enabled;
            this.alignLabelWithHint = alignLabelWithHint;
        }

        public static InputDecoration collapsed(
            string hintText = null,
            bool hasFloatingPlaceholder = true,
            TextStyle hintStyle = null,
            bool filled = false,
            Color fillColor = null,
            InputBorder border = null,
            bool enabled = true
        ) {
            border = border ?? InputBorder.none;
            InputDecoration decoration = new InputDecoration(
                icon: null,
                labelText: null,
                labelStyle: null,
                helperText: null,
                helperStyle: null,
                hintMaxLines: null,
                errorText: null,
                errorStyle: null,
                errorMaxLines: null,
                isDense: false,
                contentPadding: EdgeInsets.zero,
                prefixIcon: null,
                prefix: null,
                prefixText: null,
                prefixStyle: null,
                suffix: null,
                suffixIcon: null,
                suffixText: null,
                suffixStyle: null,
                counter: null,
                counterText: null,
                counterStyle: null,
                errorBorder: null,
                focusedBorder: null,
                focusedErrorBorder: null,
                disabledBorder: null,
                enabledBorder: null,
                hintText: hintText,
                hasFloatingPlaceholder: hasFloatingPlaceholder,
                hintStyle: hintStyle,
                filled: filled,
                fillColor: fillColor,
                border: border,
                enabled: enabled,
                alignLabelWithHint: false
            );
            decoration.isCollapsed = true;
            return decoration;
        }

        public readonly Widget icon;

        public readonly string labelText;

        public readonly TextStyle labelStyle;

        public readonly string helperText;

        public readonly TextStyle helperStyle;

        public readonly string hintText;

        public readonly TextStyle hintStyle;

        public readonly int? hintMaxLines;

        public readonly string errorText;

        public readonly TextStyle errorStyle;

        public readonly int? errorMaxLines;

        public readonly bool? hasFloatingPlaceholder;

        public readonly bool? isDense;

        public readonly EdgeInsets contentPadding;

        public bool isCollapsed;

        public readonly Widget prefixIcon;

        public readonly Widget prefix;

        public readonly string prefixText;

        public readonly TextStyle prefixStyle;

        public readonly Widget suffixIcon;

        public readonly Widget suffix;

        public readonly string suffixText;

        public readonly TextStyle suffixStyle;

        public readonly Widget counter;

        public readonly string counterText;

        public readonly TextStyle counterStyle;

        public readonly bool? filled;

        public readonly Color fillColor;

        public readonly InputBorder errorBorder;

        public readonly InputBorder focusedBorder;

        public readonly InputBorder focusedErrorBorder;

        public readonly InputBorder disabledBorder;

        public readonly InputBorder enabledBorder;

        public readonly InputBorder border;

        public readonly bool? enabled;

        public readonly bool? alignLabelWithHint;

        public InputDecoration copyWith(
            Widget icon = null,
            string labelText = null,
            TextStyle labelStyle = null,
            string helperText = null,
            TextStyle helperStyle = null,
            string hintText = null,
            TextStyle hintStyle = null,
            int? hintMaxLines = null,
            string errorText = null,
            TextStyle errorStyle = null,
            int? errorMaxLines = null,
            bool? hasFloatingPlaceholder = null,
            bool? isDense = null,
            EdgeInsets contentPadding = null,
            Widget prefixIcon = null,
            Widget prefix = null,
            string prefixText = null,
            TextStyle prefixStyle = null,
            Widget suffixIcon = null,
            Widget suffix = null,
            string suffixText = null,
            TextStyle suffixStyle = null,
            Widget counter = null,
            string counterText = null,
            TextStyle counterStyle = null,
            bool? filled = null,
            Color fillColor = null,
            InputBorder errorBorder = null,
            InputBorder focusedBorder = null,
            InputBorder focusedErrorBorder = null,
            InputBorder disabledBorder = null,
            InputBorder enabledBorder = null,
            InputBorder border = null,
            bool? enabled = null,
            bool? alignLabelWithHint = null
        ) {
            return new InputDecoration(
                icon: icon ?? this.icon,
                labelText: labelText ?? this.labelText,
                labelStyle: labelStyle ?? this.labelStyle,
                helperText: helperText ?? this.helperText,
                helperStyle: helperStyle ?? this.helperStyle,
                hintText: hintText ?? this.hintText,
                hintStyle: hintStyle ?? this.hintStyle,
                hintMaxLines: hintMaxLines ?? this.hintMaxLines,
                errorText: errorText ?? this.errorText,
                errorStyle: errorStyle ?? this.errorStyle,
                errorMaxLines: errorMaxLines ?? this.errorMaxLines,
                hasFloatingPlaceholder: hasFloatingPlaceholder ?? this.hasFloatingPlaceholder,
                isDense: isDense ?? this.isDense,
                contentPadding: contentPadding ?? this.contentPadding,
                prefixIcon: prefixIcon ?? this.prefixIcon,
                prefix: prefix ?? this.prefix,
                prefixText: prefixText ?? this.prefixText,
                prefixStyle: prefixStyle ?? this.prefixStyle,
                suffixIcon: suffixIcon ?? this.suffixIcon,
                suffix: suffix ?? this.suffix,
                suffixText: suffixText ?? this.suffixText,
                suffixStyle: suffixStyle ?? this.suffixStyle,
                counter: counter ?? this.counter,
                counterText: counterText ?? this.counterText,
                counterStyle: counterStyle ?? this.counterStyle,
                filled: filled ?? this.filled,
                fillColor: fillColor ?? this.fillColor,
                errorBorder: errorBorder ?? this.errorBorder,
                focusedBorder: focusedBorder ?? this.focusedBorder,
                focusedErrorBorder: focusedErrorBorder ?? this.focusedErrorBorder,
                disabledBorder: disabledBorder ?? this.disabledBorder,
                enabledBorder: enabledBorder ?? this.enabledBorder,
                border: border ?? this.border,
                enabled: enabled ?? this.enabled,
                alignLabelWithHint: alignLabelWithHint ?? this.alignLabelWithHint
            );
        }

        public InputDecoration applyDefaults(InputDecorationTheme theme) {
            return copyWith(
                labelStyle: labelStyle ?? theme.labelStyle,
                helperStyle: helperStyle ?? theme.helperStyle,
                hintStyle: hintStyle ?? theme.hintStyle,
                errorStyle: errorStyle ?? theme.errorStyle,
                errorMaxLines: errorMaxLines ?? theme.errorMaxLines,
                hasFloatingPlaceholder: hasFloatingPlaceholder ?? theme.hasFloatingPlaceholder,
                isDense: isDense ?? theme.isDense,
                contentPadding: contentPadding ?? theme.contentPadding,
                prefixStyle: prefixStyle ?? theme.prefixStyle,
                suffixStyle: suffixStyle ?? theme.suffixStyle,
                counterStyle: counterStyle ?? theme.counterStyle,
                filled: filled ?? theme.filled,
                fillColor: fillColor ?? theme.fillColor,
                errorBorder: errorBorder ?? theme.errorBorder,
                focusedBorder: focusedBorder ?? theme.focusedBorder,
                focusedErrorBorder: focusedErrorBorder ?? theme.focusedErrorBorder,
                disabledBorder: disabledBorder ?? theme.disabledBorder,
                enabledBorder: enabledBorder ?? theme.enabledBorder,
                border: border ?? theme.border,
                alignLabelWithHint: alignLabelWithHint ?? theme.alignLabelWithHint
            );
        }

        public static bool operator ==(InputDecoration left, InputDecoration right) {
            return Equals(left, right);
        }

        public static bool operator !=(InputDecoration left, InputDecoration right) {
            return !Equals(left, right);
        }

        public bool Equals(InputDecoration other) {
            return Equals(other.icon, icon)
                   && Equals(other.labelText, labelText)
                   && Equals(other.labelStyle, labelStyle)
                   && Equals(other.helperText, helperText)
                   && Equals(other.helperStyle, helperStyle)
                   && Equals(other.hintText, hintText)
                   && Equals(other.hintStyle, hintStyle)
                   && Equals(other.hintMaxLines, hintMaxLines)
                   && Equals(other.errorText, errorText)
                   && Equals(other.errorStyle, errorStyle)
                   && Equals(other.errorMaxLines, errorMaxLines)
                   && Equals(other.hasFloatingPlaceholder, hasFloatingPlaceholder)
                   && Equals(other.isDense, isDense)
                   && Equals(other.contentPadding, contentPadding)
                   && Equals(other.isCollapsed, isCollapsed)
                   && Equals(other.prefixIcon, prefixIcon)
                   && Equals(other.prefix, prefix)
                   && Equals(other.prefixText, prefixText)
                   && Equals(other.prefixStyle, prefixStyle)
                   && Equals(other.suffixIcon, suffixIcon)
                   && Equals(other.suffix, suffix)
                   && Equals(other.suffixText, suffixText)
                   && Equals(other.suffixStyle, suffixStyle)
                   && Equals(other.counter, counter)
                   && Equals(other.counterText, counterText)
                   && Equals(other.counterStyle, counterStyle)
                   && Equals(other.filled, filled)
                   && Equals(other.fillColor, fillColor)
                   && Equals(other.errorBorder, errorBorder)
                   && Equals(other.focusedBorder, focusedBorder)
                   && Equals(other.focusedErrorBorder, focusedErrorBorder)
                   && Equals(other.disabledBorder, disabledBorder)
                   && Equals(other.enabledBorder, enabledBorder)
                   && Equals(other.border, border)
                   && Equals(other.enabled, enabled)
                   && Equals(other.alignLabelWithHint, alignLabelWithHint);
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

            return Equals((InputDecoration) obj);
        }

        public override int GetHashCode() {
            unchecked {
                var hashCode = icon.GetHashCode();
                hashCode = (hashCode * 397) ^ labelText.GetHashCode();
                hashCode = (hashCode * 397) ^ labelStyle.GetHashCode();
                hashCode = (hashCode * 397) ^ helperText.GetHashCode();
                hashCode = (hashCode * 397) ^ helperStyle.GetHashCode();
                hashCode = (hashCode * 397) ^ hintText.GetHashCode();
                hashCode = (hashCode * 397) ^ hintStyle.GetHashCode();
                hashCode = (hashCode * 397) ^ hintMaxLines.GetHashCode();
                hashCode = (hashCode * 397) ^ errorText.GetHashCode();
                hashCode = (hashCode * 397) ^ errorStyle.GetHashCode();
                hashCode = (hashCode * 397) ^ errorMaxLines.GetHashCode();
                hashCode = (hashCode * 397) ^ hasFloatingPlaceholder.GetHashCode();
                hashCode = (hashCode * 397) ^ isDense.GetHashCode();
                hashCode = (hashCode * 397) ^ contentPadding.GetHashCode();
                hashCode = (hashCode * 397) ^ isCollapsed.GetHashCode();
                hashCode = (hashCode * 397) ^ filled.GetHashCode();
                hashCode = (hashCode * 397) ^ fillColor.GetHashCode();
                hashCode = (hashCode * 397) ^ border.GetHashCode();
                hashCode = (hashCode * 397) ^ enabled.GetHashCode();
                hashCode = (hashCode * 397) ^ prefixIcon.GetHashCode();
                hashCode = (hashCode * 397) ^ prefix.GetHashCode();
                hashCode = (hashCode * 397) ^ prefixText.GetHashCode();
                hashCode = (hashCode * 397) ^ prefixStyle.GetHashCode();
                hashCode = (hashCode * 397) ^ suffixIcon.GetHashCode();
                hashCode = (hashCode * 397) ^ suffix.GetHashCode();
                hashCode = (hashCode * 397) ^ suffixText.GetHashCode();
                hashCode = (hashCode * 397) ^ suffixStyle.GetHashCode();
                hashCode = (hashCode * 397) ^ counter.GetHashCode();
                hashCode = (hashCode * 397) ^ counterText.GetHashCode();
                hashCode = (hashCode * 397) ^ counterStyle.GetHashCode();
                hashCode = (hashCode * 397) ^ filled.GetHashCode();
                hashCode = (hashCode * 397) ^ fillColor.GetHashCode();
                hashCode = (hashCode * 397) ^ errorBorder.GetHashCode();
                hashCode = (hashCode * 397) ^ focusedBorder.GetHashCode();
                hashCode = (hashCode * 397) ^ focusedErrorBorder.GetHashCode();
                hashCode = (hashCode * 397) ^ disabledBorder.GetHashCode();
                hashCode = (hashCode * 397) ^ enabledBorder.GetHashCode();
                hashCode = (hashCode * 397) ^ border.GetHashCode();
                hashCode = (hashCode * 397) ^ enabled.GetHashCode();
                hashCode = (hashCode * 397) ^ alignLabelWithHint.GetHashCode();
                return hashCode;
            }
        }

        public override string ToString() {
            List<string> description = new List<string> { };
            if (icon != null) {
                description.Add($"icon: ${icon}");
            }

            if (labelText != null) {
                description.Add($"labelText: ${labelText}");
            }

            if (helperText != null) {
                description.Add($"helperText: ${helperText}");
            }

            if (hintMaxLines != null) {
                description.Add($"hintMaxLines: ${hintMaxLines}");
            }

            if (hintText != null) {
                description.Add($"hintText: ${hintText}");
            }

            if (errorText != null) {
                description.Add($"errorText: ${errorText}");
            }

            if (errorStyle != null) {
                description.Add($"errorStyle: ${errorStyle}");
            }

            if (errorMaxLines != null) {
                description.Add($"errorMaxLines: ${errorMaxLines}");
            }

            if (hasFloatingPlaceholder == false) {
                description.Add($"hasFloatingPlaceholder: false");
            }

            if (isDense ?? false) {
                description.Add($"isDense: ${isDense}");
            }

            if (contentPadding != null) {
                description.Add($"contentPadding: ${contentPadding}");
            }

            if (isCollapsed) {
                description.Add($"isCollapsed: ${isCollapsed}");
            }

            if (prefixIcon != null) {
                description.Add($"prefixIcon: ${prefixIcon}");
            }

            if (prefix != null) {
                description.Add($"prefix: ${prefix}");
            }

            if (prefixText != null) {
                description.Add($"prefixText: ${prefixText}");
            }

            if (prefixStyle != null) {
                description.Add($"prefixStyle: ${prefixStyle}");
            }

            if (suffixIcon != null) {
                description.Add($"suffixIcon: ${suffixIcon}");
            }

            if (suffix != null) {
                description.Add($"suffix: ${suffix}");
            }

            if (suffixText != null) {
                description.Add($"suffixText: ${suffixText}");
            }

            if (suffixStyle != null) {
                description.Add($"suffixStyle: ${suffixStyle}");
            }

            if (counter != null) {
                description.Add($"counter: ${counter}");
            }

            if (counterText != null) {
                description.Add($"counterText: ${counterText}");
            }

            if (counterStyle != null) {
                description.Add($"counterStyle: ${counterStyle}");
            }

            if (filled == true) {
                description.Add($"filled: true");
            }

            if (fillColor != null) {
                description.Add($"fillColor: ${fillColor}");
            }

            if (errorBorder != null) {
                description.Add($"errorBorder: ${errorBorder}");
            }

            if (focusedBorder != null) {
                description.Add($"focusedBorder: ${focusedBorder}");
            }

            if (focusedErrorBorder != null) {
                description.Add($"focusedErrorBorder: ${focusedErrorBorder}");
            }

            if (disabledBorder != null) {
                description.Add($"disabledBorder: ${disabledBorder}");
            }

            if (enabledBorder != null) {
                description.Add($"enabledBorder: ${enabledBorder}");
            }

            if (border != null) {
                description.Add($"border: ${border}");
            }

            if (enabled != true) {
                description.Add("enabled: false");
            }

            if (alignLabelWithHint != null) {
                description.Add($"alignLabelWithHint: {alignLabelWithHint}");
            }

            return $"InputDecoration(${string.Join(", ", description)})";
        }
    }

    public class InputDecorationTheme : Diagnosticable {
        public InputDecorationTheme(
            TextStyle labelStyle = null,
            TextStyle helperStyle = null,
            TextStyle hintStyle = null,
            TextStyle errorStyle = null,
            int? errorMaxLines = null,
            bool? hasFloatingPlaceholder = true,
            bool? isDense = false,
            EdgeInsets contentPadding = null,
            bool? isCollapsed = false,
            TextStyle prefixStyle = null,
            TextStyle suffixStyle = null,
            TextStyle counterStyle = null,
            bool? filled = false,
            Color fillColor = null,
            InputBorder errorBorder = null,
            InputBorder focusedBorder = null,
            InputBorder focusedErrorBorder = null,
            InputBorder disabledBorder = null,
            InputBorder enabledBorder = null,
            InputBorder border = null,
            bool alignLabelWithHint = false
        ) {
            D.assert(isDense != null);
            D.assert(isCollapsed != null);
            D.assert(filled != null);
            this.labelStyle = labelStyle;
            this.helperStyle = helperStyle;
            this.hintStyle = hintStyle;
            this.errorStyle = errorStyle;
            this.errorMaxLines = errorMaxLines;
            this.hasFloatingPlaceholder = hasFloatingPlaceholder;
            this.isDense = isDense;
            this.contentPadding = contentPadding;
            this.isCollapsed = isCollapsed;
            this.prefixStyle = prefixStyle;
            this.suffixStyle = suffixStyle;
            this.counterStyle = counterStyle;
            this.filled = filled;
            this.fillColor = fillColor;
            this.errorBorder = errorBorder;
            this.focusedBorder = focusedBorder;
            this.focusedErrorBorder = focusedErrorBorder;
            this.disabledBorder = disabledBorder;
            this.enabledBorder = enabledBorder;
            this.border = border;
            this.alignLabelWithHint = alignLabelWithHint;
        }

        public readonly TextStyle labelStyle;

        public readonly TextStyle helperStyle;

        public readonly TextStyle hintStyle;

        public readonly TextStyle errorStyle;

        public readonly int? errorMaxLines;

        public readonly bool? hasFloatingPlaceholder;

        public readonly bool? isDense;

        public readonly EdgeInsets contentPadding;

        public readonly bool? isCollapsed;

        public readonly TextStyle prefixStyle;

        public readonly TextStyle suffixStyle;

        public readonly TextStyle counterStyle;

        public readonly bool? filled;

        public readonly Color fillColor;

        public readonly InputBorder errorBorder;

        public readonly InputBorder focusedBorder;

        public readonly InputBorder focusedErrorBorder;

        public readonly InputBorder disabledBorder;

        public readonly InputBorder enabledBorder;

        public readonly InputBorder border;

        public readonly bool alignLabelWithHint;

        public override void debugFillProperties(DiagnosticPropertiesBuilder properties) {
            base.debugFillProperties(properties);
            InputDecorationTheme defaultTheme = new InputDecorationTheme();
            properties.add(new DiagnosticsProperty<TextStyle>("labelStyle", labelStyle,
                defaultValue: defaultTheme.labelStyle));
            properties.add(new DiagnosticsProperty<TextStyle>("helperStyle", helperStyle,
                defaultValue: defaultTheme.helperStyle));
            properties.add(new DiagnosticsProperty<TextStyle>("hintStyle", hintStyle,
                defaultValue: defaultTheme.hintStyle));
            properties.add(new DiagnosticsProperty<TextStyle>("errorStyle", errorStyle,
                defaultValue: defaultTheme.errorStyle));
            properties.add(new DiagnosticsProperty<int?>("errorMaxLines", errorMaxLines,
                defaultValue: defaultTheme.errorMaxLines));
            properties.add(new DiagnosticsProperty<bool?>("hasFloatingPlaceholder", hasFloatingPlaceholder,
                defaultValue: defaultTheme.hasFloatingPlaceholder));
            properties.add(new DiagnosticsProperty<bool?>("isDense", isDense, defaultValue: defaultTheme.isDense));
            properties.add(new DiagnosticsProperty<EdgeInsets>("contentPadding", contentPadding,
                defaultValue: defaultTheme.contentPadding));
            properties.add(new DiagnosticsProperty<bool?>("isCollapsed", isCollapsed,
                defaultValue: defaultTheme.isCollapsed));
            properties.add(new DiagnosticsProperty<TextStyle>("prefixStyle", prefixStyle,
                defaultValue: defaultTheme.prefixStyle));
            properties.add(new DiagnosticsProperty<TextStyle>("suffixStyle", suffixStyle,
                defaultValue: defaultTheme.suffixStyle));
            properties.add(new DiagnosticsProperty<TextStyle>("counterStyle", counterStyle,
                defaultValue: defaultTheme.counterStyle));
            properties.add(new DiagnosticsProperty<bool?>("filled", filled, defaultValue: defaultTheme.filled));
            properties.add(new DiagnosticsProperty<Color>("fillColor", fillColor,
                defaultValue: defaultTheme.fillColor));
            properties.add(new DiagnosticsProperty<InputBorder>("errorBorder", errorBorder,
                defaultValue: defaultTheme.errorBorder));
            properties.add(new DiagnosticsProperty<InputBorder>("focusedBorder", focusedBorder,
                defaultValue: defaultTheme.focusedErrorBorder));
            properties.add(new DiagnosticsProperty<InputBorder>("focusedErrorBorder", focusedErrorBorder,
                defaultValue: defaultTheme.focusedErrorBorder));
            properties.add(new DiagnosticsProperty<InputBorder>("disabledBorder", disabledBorder,
                defaultValue: defaultTheme.disabledBorder));
            properties.add(new DiagnosticsProperty<InputBorder>("enabledBorder", enabledBorder,
                defaultValue: defaultTheme.enabledBorder));
            properties.add(
                new DiagnosticsProperty<InputBorder>("border", border, defaultValue: defaultTheme.border));
            properties.add(new DiagnosticsProperty<bool>("alignLabelWithHint", alignLabelWithHint,
                defaultValue: defaultTheme.alignLabelWithHint));
        }
    }
}