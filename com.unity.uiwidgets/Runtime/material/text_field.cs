using System.Collections.Generic;
using System.Text;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.gestures;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.rendering;
using Unity.UIWidgets.service;
using Unity.UIWidgets.ui;
using Unity.UIWidgets.widgets;
using UnityEngine;
using Color = Unity.UIWidgets.ui.Color;
using TextStyle = Unity.UIWidgets.painting.TextStyle;

namespace Unity.UIWidgets.material {

    public delegate Widget InputCounterWidgetBuilder(
        BuildContext buildContext,
        int? currentLength,
        int? maxLength,
        bool? isFocused);
        
    public class TextField : StatefulWidget {
        public TextField(Key key = null,
            TextEditingController controller = null,
            FocusNode focusNode = null,
            InputDecoration decoration = null,
            bool noDecoration = false,
            TextInputType keyboardType = null,
            TextInputAction? textInputAction = null,
            TextCapitalization textCapitalization = TextCapitalization.none,
            TextStyle style = null,
            StrutStyle strutStyle = null,
            TextAlign textAlign = TextAlign.left,
            TextDirection textDirection = TextDirection.ltr,
            bool autofocus = false,
            bool obscureText = false,
            bool autocorrect = false,
            int? maxLines = 1,
            int? minLines = null,
            bool expands = false,
            int? maxLength = null,
            bool maxLengthEnforced = true,
            ValueChanged<string> onChanged = null,
            VoidCallback onEditingComplete = null,
            ValueChanged<string> onSubmitted = null,
            List<TextInputFormatter> inputFormatters = null,
            bool? enabled = null,
            float? cursorWidth = 2.0f,
            Radius cursorRadius = null,
            Color cursorColor = null,
            Brightness? keyboardAppearance = null,
            EdgeInsets scrollPadding = null,
            DragStartBehavior dragStartBehavior = DragStartBehavior.start,
            bool? enableInteractiveSelection = null,
            GestureTapCallback onTap = null,
            InputCounterWidgetBuilder buildCounter = null,
            ScrollPhysics scrollPhysics = null
        ) : base(key: key) {
            D.assert(maxLines == null || maxLines > 0);
            D.assert(minLines == null || minLines > 0);
            D.assert((maxLines == null) || (minLines == null) || (maxLines >= minLines),
                () => "minLines can't be greater than maxLines");
            D.assert(!expands || (maxLines == null && minLines == null),
                () => "minLines and maxLines must be null when expands is true.");
            D.assert(maxLength == null || maxLength == noMaxLength || maxLength > 0);

            this.controller = controller;
            this.focusNode = focusNode;
            this.decoration = noDecoration ? null : (decoration ?? new InputDecoration());
            this.textInputAction = textInputAction;
            this.textCapitalization = textCapitalization;
            this.style = style;
            this.strutStyle = strutStyle;
            this.textAlign = textAlign;
            this.textDirection = textDirection;
            this.autofocus = autofocus;
            this.obscureText = obscureText;
            this.autocorrect = autocorrect;
            this.maxLines = maxLines;
            this.minLines = minLines;
            this.expands = expands;
            this.maxLength = maxLength;
            this.maxLengthEnforced = maxLengthEnforced;
            this.onChanged = onChanged;
            this.onEditingComplete = onEditingComplete;
            this.onSubmitted = onSubmitted;
            this.inputFormatters = inputFormatters;
            this.enabled = enabled;
            this.cursorWidth = cursorWidth;
            this.cursorColor = cursorColor;
            this.cursorRadius = cursorRadius;
            this.onSubmitted = onSubmitted;
            this.keyboardAppearance = keyboardAppearance;
            this.enableInteractiveSelection = enableInteractiveSelection;
            this.onTap = onTap;
            this.keyboardType = keyboardType ?? (maxLines == 1 ? TextInputType.text : TextInputType.multiline);
            this.scrollPadding = scrollPadding ?? EdgeInsets.all(20.0f);
            this.dragStartBehavior = dragStartBehavior;
            this.buildCounter = buildCounter;
            this.scrollPhysics = scrollPhysics;
        }

        public readonly TextEditingController controller;

        public readonly FocusNode focusNode;

        public readonly InputDecoration decoration;

        public readonly TextInputType keyboardType;

        public readonly TextInputAction? textInputAction;

        public readonly TextCapitalization textCapitalization;

        public readonly TextStyle style;

        public readonly StrutStyle strutStyle;

        public readonly TextAlign textAlign;

        public readonly TextDirection textDirection;

        public readonly bool autofocus;

        public readonly bool obscureText;

        public readonly bool autocorrect;

        public readonly int? maxLines;

        public readonly int? minLines;

        public readonly bool expands;
        
        public const long noMaxLength = -1;

        public readonly int? maxLength;

        public readonly bool maxLengthEnforced;

        public readonly ValueChanged<string> onChanged;

        public readonly VoidCallback onEditingComplete;

        public readonly ValueChanged<string> onSubmitted;

        public readonly List<TextInputFormatter> inputFormatters;

        public readonly bool? enabled;

        public readonly float? cursorWidth;

        public readonly Radius cursorRadius;

        public readonly Color cursorColor;

        public readonly Brightness? keyboardAppearance;

        public readonly EdgeInsets scrollPadding;

        public readonly bool? enableInteractiveSelection;
        
        public readonly DragStartBehavior dragStartBehavior;

        public readonly ScrollPhysics scrollPhysics;
        
        public bool selectionEnabled {
            get {
                return enableInteractiveSelection ?? !obscureText;
            }
        }

        public readonly GestureTapCallback onTap;

        public readonly InputCounterWidgetBuilder buildCounter;

        public override State createState() {
            return new _TextFieldState();
        }

        public override void debugFillProperties(DiagnosticPropertiesBuilder properties) {
            base.debugFillProperties(properties);
            properties.add(
                new DiagnosticsProperty<TextEditingController>("controller", controller, defaultValue: null));
            properties.add(new DiagnosticsProperty<FocusNode>("focusNode", focusNode, defaultValue: null));
            properties.add(new DiagnosticsProperty<bool?>("enabled", enabled, defaultValue: null));
            properties.add(new DiagnosticsProperty<InputDecoration>("decoration", decoration, defaultValue: new InputDecoration()));
            properties.add(new DiagnosticsProperty<TextInputType>("keyboardType", keyboardType,
                defaultValue: TextInputType.text));
            properties.add(new DiagnosticsProperty<TextStyle>("style", style, defaultValue: null));
            properties.add(new DiagnosticsProperty<bool>("autofocus", autofocus, defaultValue: false));
            properties.add(new DiagnosticsProperty<bool>("obscureText", obscureText, defaultValue: false));
            properties.add(new DiagnosticsProperty<bool>("autocorrect", autocorrect, defaultValue: true));
            properties.add(new IntProperty("maxLines", maxLines, defaultValue: 1));
            properties.add(new IntProperty("minLines", minLines, defaultValue: null));
            properties.add(new DiagnosticsProperty<bool>("expands", expands, defaultValue: false));
            properties.add(new IntProperty("maxLength", maxLength, defaultValue: null));
            properties.add(new FlagProperty("maxLengthEnforced", value: maxLengthEnforced, defaultValue: true,
                ifFalse: "maxLength not enforced"));
            properties.add(new EnumProperty<TextInputAction?>("textInputAction", textInputAction, defaultValue: null));
            properties.add(new EnumProperty<TextCapitalization>("textCapitalization", textCapitalization, defaultValue: TextCapitalization.none));
            properties.add(new EnumProperty<TextAlign>("textAlign", textAlign, defaultValue: TextAlign.left));
            properties.add(new EnumProperty<TextDirection>("textDirection", textDirection, defaultValue: null));
            properties.add(new FloatProperty("cursorWidth", cursorWidth, defaultValue: 2.0f));
            properties.add(new DiagnosticsProperty<Radius>("cursorRadius", cursorRadius, defaultValue: null));
            properties.add(new DiagnosticsProperty<Color>("cursorColor", cursorColor, defaultValue: null));
            properties.add(new DiagnosticsProperty<Brightness?>("keyboardAppearance", keyboardAppearance, defaultValue: null));
            properties.add(new DiagnosticsProperty<EdgeInsets>("scrollPadding", scrollPadding, defaultValue: EdgeInsets.all(20.0f)));
            properties.add(new FlagProperty("selectionEnabled", value: selectionEnabled, defaultValue: true, ifFalse: "selection disabled"));
            properties.add(new DiagnosticsProperty<ScrollPhysics>("scrollPhysics", scrollPhysics, defaultValue: null));
        }
    }

    class _TextFieldState : AutomaticKeepAliveClientMixin<TextField> {
        readonly GlobalKey<EditableTextState> _editableTextKey = new LabeledGlobalKey<EditableTextState>();

        HashSet<InteractiveInkFeature> _splashes;
        InteractiveInkFeature _currentSplash;

        TextEditingController _controller;

        TextEditingController _effectiveController {
            get { return widget.controller ?? _controller; }
        }

        FocusNode _focusNode;

        FocusNode _effectiveFocusNode {
            get {
                if (widget.focusNode != null) {
                    return widget.focusNode;
                }

                if (_focusNode != null) {
                    return _focusNode;
                }

                _focusNode = new FocusNode();
                return _focusNode;
            }
        }

        bool needsCounter {
            get {
                return widget.maxLength != null
                       && widget.decoration != null
                       && widget.decoration.counterText == null;
            }
        }

        InputDecoration _getEffectiveDecoration() {
            MaterialLocalizations localizations = MaterialLocalizations.of(context);
            ThemeData themeData = Theme.of(context);
            InputDecoration effectiveDecoration = (widget.decoration ?? new InputDecoration())
                .applyDefaults(themeData.inputDecorationTheme)
                .copyWith(
                    enabled: widget.enabled,
                    hintMaxLines: widget.decoration?.hintMaxLines ?? widget.maxLines
                );

            if (effectiveDecoration.counter != null || effectiveDecoration.counterText != null) {
                return effectiveDecoration;
            }

            Widget counter;
            int currentLength = _effectiveController.value.text.Length;
            if (effectiveDecoration.counter == null
                && effectiveDecoration.counterText == null
                && widget.buildCounter != null) {
                bool isFocused = _effectiveFocusNode.hasFocus;
                counter = widget.buildCounter(
                    context,
                    currentLength: currentLength,
                    maxLength: widget.maxLength,
                    isFocused: isFocused
                );
                return effectiveDecoration.copyWith(counter: counter);
            }

            if (widget.maxLength == null) {
                return effectiveDecoration;
            }

            string counterText = $"{currentLength}";

            if (widget.maxLength > 0) {
                counterText += $"/{widget.maxLength}";
                if (_effectiveController.value.text.Length > widget.maxLength) {
                    return effectiveDecoration.copyWith(
                        errorText: effectiveDecoration.errorText ?? "",
                        counterStyle: effectiveDecoration.errorStyle
                                      ?? themeData.textTheme.caption.copyWith(color: themeData.errorColor),
                        counterText: counterText
                    );
                }
            }

            // Handle length exceeds maxLength

            return effectiveDecoration.copyWith(
                counterText: counterText
            );
        }

        public override void initState() {
            base.initState();
            if (widget.controller == null) {
                _controller = new TextEditingController();
            }
        }

        public override void didUpdateWidget(StatefulWidget oldWidget) {
            base.didUpdateWidget(oldWidget);
            if (widget.controller == null && ((TextField) oldWidget).controller != null) {
                _controller = TextEditingController.fromValue(((TextField) oldWidget).controller.value);
            }
            else if (widget.controller != null && ((TextField) oldWidget).controller == null) {
                _controller = null;
            }

            bool isEnabled = widget.enabled ?? widget.decoration?.enabled ?? true;
            bool wasEnabled = ((TextField) oldWidget).enabled ?? ((TextField) oldWidget).decoration?.enabled ?? true;
            if (wasEnabled && !isEnabled) {
                _effectiveFocusNode.unfocus();
            }
        }

        public override void dispose() {
            _focusNode?.dispose();
            base.dispose();
        }

        void _requestKeyboard() {
            _editableTextKey.currentState?.requestKeyboard();
        }

        void _handleSelectionChanged(TextSelection selection, SelectionChangedCause cause) {
            switch (Theme.of(context).platform) {
                case RuntimePlatform.IPhonePlayer:
                    if (cause == SelectionChangedCause.longPress) {
                        _editableTextKey.currentState?.bringIntoView(selection.basePos);
                    }
                    return;
                case RuntimePlatform.Android:
                    break;
            }
        }

        InteractiveInkFeature _createInkFeature(Offset globalPosition) {
            MaterialInkController inkController = Material.of(context);
            ThemeData themeData = Theme.of(context);
            BuildContext editableContext = _editableTextKey.currentContext;
            RenderBox referenceBox =
                (RenderBox) (InputDecorator.containerOf(editableContext) ?? editableContext.findRenderObject());
            Offset position = referenceBox.globalToLocal(globalPosition);
            Color color = themeData.splashColor;

            InteractiveInkFeature splash = null;

            void handleRemoved() {
                if (_splashes != null) {
                    D.assert(_splashes.Contains(splash));
                    _splashes.Remove(splash);
                    if (_currentSplash == splash) {
                        _currentSplash = null;
                    }

                    updateKeepAlive();
                } // else we're probably in deactivate()
            }

            splash = themeData.splashFactory.create(
                controller: inkController,
                referenceBox: referenceBox,
                position: position,
                color: color,
                containedInkWell: true,
                borderRadius: BorderRadius.zero,
                onRemoved: handleRemoved
            );

            return splash;
        }

        RenderEditable _renderEditable {
            get { return _editableTextKey.currentState.renderEditable; }
        }

        void _handleTapDown(TapDownDetails details) {
            _renderEditable.handleTapDown(details);
            _startSplash(details.globalPosition);
        }

        void _handleSingleTapUp(TapUpDetails details) {
            if (widget.enableInteractiveSelection == true) {
                _renderEditable.handleTap();
            }

            _requestKeyboard();
            _confirmCurrentSplash();
            if (widget.onTap != null) {
                widget.onTap();
            }
        }

        void _handleSingleTapCancel() {
            _cancelCurrentSplash();
        }

        void _handleSingleLongTapStart(LongPressStartDetails details) {
            if (widget.selectionEnabled) {
                switch (Theme.of(context).platform) {
                    case RuntimePlatform.IPhonePlayer:
                        _renderEditable.selectPositionAt(
                            from: details.globalPosition,
                            cause: SelectionChangedCause.longPress
                        );
                        break;
                    case RuntimePlatform.Android:
                        _renderEditable.selectWord(cause: SelectionChangedCause.longPress);
                        Feedback.forLongPress(context);
                        break;
                }
            }
            _confirmCurrentSplash();
        }

        void _handleSingleLongTapMoveUpdate(LongPressMoveUpdateDetails details) {
            if (widget.selectionEnabled) {
                switch (Theme.of(context).platform) {
                    case RuntimePlatform.IPhonePlayer:
                        _renderEditable.selectPositionAt(
                            from: details.globalPosition,
                            cause: SelectionChangedCause.longPress
                        );
                        break;
                    case RuntimePlatform.Android:
                        _renderEditable.selectWordsInRange(
                            from: details.globalPosition - details.offsetFromOrigin,
                            to: details.globalPosition,
                            cause: SelectionChangedCause.longPress);
                        Feedback.forLongPress(context);
                        break;
                }
            }
        }

        void _handleSingleLongTapEnd(LongPressEndDetails details) {
            _editableTextKey.currentState.showToolbar();
        }

        void _handleDoubleTapDown(TapDownDetails details) {
            if (widget.selectionEnabled) {
                _renderEditable.selectWord(cause: SelectionChangedCause.doubleTap);
                _editableTextKey.currentState.showToolbar();
            }
        }

        void _handleMouseDragSelectionStart(DragStartDetails details) {
            _renderEditable.selectPositionAt(
                from: details.globalPosition,
                cause: SelectionChangedCause.drag);

            _startSplash(details.globalPosition);
        }

        void _handleMouseDragSelectionUpdate(DragStartDetails startDetails,
            DragUpdateDetails updateDetails) {
            _renderEditable.selectPositionAt(
                from: startDetails.globalPosition,
                to: updateDetails.globalPosition,
                cause: SelectionChangedCause.drag);
        }


        void _startSplash(Offset globalPosition) {
            if (_effectiveFocusNode.hasFocus) {
                return;
            }

            InteractiveInkFeature splash = _createInkFeature(globalPosition);
            _splashes = _splashes ?? new HashSet<InteractiveInkFeature>();
            _splashes.Add(splash);
            _currentSplash = splash;
            updateKeepAlive();
        }

        void _confirmCurrentSplash() {
            _currentSplash?.confirm();
            _currentSplash = null;
        }

        void _cancelCurrentSplash() {
            _currentSplash?.cancel();
        }

        protected override bool wantKeepAlive {
            get { return _splashes != null && _splashes.isNotEmpty(); }
        }

        public override void deactivate() {
            if (_splashes != null) {
                HashSet<InteractiveInkFeature> splashes = _splashes;
                _splashes = null;
                foreach (InteractiveInkFeature splash in splashes) {
                    splash.dispose();
                }

                _currentSplash = null;
            }

            D.assert(_currentSplash == null);
            base.deactivate();
        }

        public override Widget build(BuildContext context) {
            base.build(context); // See AutomaticKeepAliveClientMixin.
            D.assert(MaterialD.debugCheckHasMaterial(context));
            D.assert(WidgetsD.debugCheckHasDirectionality(context));
            D.assert(
              !(widget.style != null && widget.style.inherit == false &&
                (widget.style.fontSize == null || widget.style.textBaseline == null)),
              () => "inherit false style must supply fontSize and textBaseline"
            );
            ThemeData themeData = Theme.of(context);
            TextStyle style = themeData.textTheme.subhead.merge(widget.style);
            Brightness keyboardAppearance = widget.keyboardAppearance ?? themeData.primaryColorBrightness;
            TextEditingController controller = _effectiveController;
            FocusNode focusNode = _effectiveFocusNode;
            List<TextInputFormatter> formatters = widget.inputFormatters ?? new List<TextInputFormatter>();
            if (widget.maxLength != null && widget.maxLengthEnforced) {
                formatters.Add(new LengthLimitingTextInputFormatter(widget.maxLength));
            }
            
            // bool forcePressEnabled = false; // TODO: wait for force press is ready
            TextSelectionControls textSelectionControls = MaterialUtils.materialTextSelectionControls;;
            bool paintCursorAboveText = false;
            bool cursorOpacityAnimates = false;
            Offset cursorOffset = null;
            Color cursorColor = widget.cursorColor ?? themeData.cursorColor;
            Radius cursorRadius = widget.cursorRadius;

            Widget child = new RepaintBoundary(
                child: new EditableText(
                    key: _editableTextKey,
                    controller: controller,
                    focusNode: focusNode,
                    keyboardType: widget.keyboardType,
                    textInputAction: widget.textInputAction,
                    textCapitalization: widget.textCapitalization,
                    style: style,
                    strutStyle: widget.strutStyle,
                    textAlign: widget.textAlign,
                    textDirection: widget.textDirection,
                    autofocus: widget.autofocus,
                    obscureText: widget.obscureText,
                    autocorrect: widget.autocorrect,
                    maxLines: widget.maxLines,
                    minLines: widget.minLines,
                    expands: widget.expands,
                    selectionColor: themeData.textSelectionColor,
                    selectionControls: widget.selectionEnabled ? textSelectionControls : null,
                    onChanged: widget.onChanged,
                    onSelectionChanged: _handleSelectionChanged,
                    onEditingComplete: widget.onEditingComplete,
                    onSubmitted: widget.onSubmitted,
                    inputFormatters: formatters,
                    rendererIgnoresPointer: true,
                    cursorWidth: widget.cursorWidth,
                    cursorRadius: cursorRadius,
                    cursorColor: cursorColor,
                    cursorOpacityAnimates: cursorOpacityAnimates,
                    cursorOffset: cursorOffset,
                    paintCursorAboveText: paintCursorAboveText,
                    backgroundCursorColor: new Color(0xFF8E8E93),// TODO: CupertinoColors.inactiveGray,
                    scrollPadding: widget.scrollPadding,
                    keyboardAppearance: keyboardAppearance,
                    enableInteractiveSelection: widget.enableInteractiveSelection == true,
                    dragStartBehavior: widget.dragStartBehavior,
                    scrollPhysics: widget.scrollPhysics
                )
            );

            if (widget.decoration != null) {
                child = new AnimatedBuilder(
                    animation: ListenableUtils.merge(new List<Listenable> {focusNode, controller}),
                    builder:
                    (_context, _child) => {
                        return new InputDecorator(
                            decoration: _getEffectiveDecoration(),
                            baseStyle: widget.style,
                            textAlign: widget.textAlign,
                            isFocused: focusNode.hasFocus,
                            isEmpty: controller.value.text.isEmpty(),
                            expands: widget.expands,
                            child: _child
                        );
                    },
                    child: child
                );
            }

            return new IgnorePointer(
                ignoring: !(widget.enabled ?? widget.decoration?.enabled ?? true),
                child: new TextSelectionGestureDetector(
                    onTapDown: _handleTapDown,
                    // onForcePressStart: forcePressEnabled ? this._handleForcePressStarted : null, // TODO: Remove this when force press is added
                    onSingleTapUp: _handleSingleTapUp,
                    onSingleTapCancel: _handleSingleTapCancel,
                    onSingleLongTapStart: _handleSingleLongTapStart,
                    onSingleLongTapMoveUpdate: _handleSingleLongTapMoveUpdate,
                    onSingleLongTapEnd: _handleSingleLongTapEnd,
                    onDoubleTapDown: _handleDoubleTapDown,
                    onDragSelectionStart: _handleMouseDragSelectionStart,
                    onDragSelectionUpdate: _handleMouseDragSelectionUpdate,
                    behavior: HitTestBehavior.translucent,
                    child: child
                )
            );
        }
    }
}