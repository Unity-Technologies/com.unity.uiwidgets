using System;
using System.Collections.Generic;
using Unity.UIWidgets.animation;
using Unity.UIWidgets.async;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.gestures;
using Unity.UIWidgets.material;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.rendering;
using Unity.UIWidgets.scheduler;
using Unity.UIWidgets.service;
using Unity.UIWidgets.ui;
using UnityEditor;
using UnityEngine;
using Brightness = Unity.UIWidgets.ui.Brightness;
using Color = Unity.UIWidgets.ui.Color;
using MathUtils = Unity.UIWidgets.ui.MathUtils;
using Rect = Unity.UIWidgets.ui.Rect;
using StrutStyle = Unity.UIWidgets.painting.StrutStyle;
using TextStyle = Unity.UIWidgets.painting.TextStyle;

namespace Unity.UIWidgets.widgets {
    public delegate void SelectionChangedCallback(TextSelection selection, SelectionChangedCause cause);

    public class TextEditingController : ValueNotifier<TextEditingValue> {
        public TextEditingController(string text = null) : base(text == null
            ? TextEditingValue.empty
            : new TextEditingValue(text)) {
        }

        TextEditingController(TextEditingValue value) : base(value ?? TextEditingValue.empty) {
        }

        public static TextEditingController fromValue(TextEditingValue value) {
            return new TextEditingController(value);
        }

        public string text {
            get { return value.text; }

            set {
                this.value = this.value.copyWith(
                    text: value,
                    selection: TextSelection.collapsed(-1),
                    composing: TextRange.empty);
            }
        }
        public virtual TextSpan buildTextSpan(TextStyle style = null , bool withComposing = false) {
            if (!value.composing.isValid || !withComposing) {
                return new TextSpan(style: style, text: text);
            }
            TextStyle composingStyle = style.merge(
            new TextStyle(decoration: TextDecoration.underline)
                );
            var spans = new List<InlineSpan>();
            spans.Add(new TextSpan(text: value.composing.textBefore(value.text)));
            spans.Add(new TextSpan(
                style: composingStyle,
                text: value.composing.textInside(value.text)));
            spans.Add(new TextSpan(text: value.composing.textAfter(value.text)));
            return new TextSpan(
                style: style,
                children: spans 
                );
        }
        public TextSelection selection {
            get { return value.selection; }

            set {
                if (value.start > text.Length || value.end > text.Length) {
                    throw new UIWidgetsError($"invalid text selection: {value}");
                }

                this.value = this.value.copyWith(selection: value, composing: TextRange.empty);
            }
        }

        public void clear() {
            value = TextEditingValue.empty;
        }

        public void clearComposing() {
            value = value.copyWith(composing: TextRange.empty);
        }
        
        public bool isSelectionWithinTextBounds(TextSelection selection) {
            return selection.start <= text.Length && selection.end <= text.Length;
        }
    }
    public class ToolbarOptions {
        
        public ToolbarOptions(
            bool copy = false,
            bool cut = false,
            bool paste = false,
            bool selectAll = false
        ) {
            this.copy = copy;
            this.cut = cut;
            this.paste = paste;
            this.selectAll = selectAll;

        }
        public readonly  bool copy;
        public readonly  bool cut;
        public readonly  bool paste; 
        public readonly  bool selectAll;
    }

    
    public class EditableText : StatefulWidget {
        public EditableText(
            Key key = null,
            TextEditingController controller = null,
            FocusNode focusNode = null,
            bool readOnly = false,
            bool obscureText = false,
            bool autocorrect = true,
            SmartDashesType? smartDashesType = null,
            SmartQuotesType? smartQuotesType = null,
            bool enableSuggestions = true,
            TextStyle style = null,
            StrutStyle strutStyle = null,
            Color cursorColor = null,
            Color backgroundCursorColor = null,
            TextAlign textAlign = TextAlign.start,
            TextDirection? textDirection = null,
            Locale locale = null,
            float? textScaleFactor = null,
            int? maxLines = 1,
            int? minLines = null,
            bool expands = false,
            bool forceLine = true,
            TextWidthBasis textWidthBasis = TextWidthBasis.parent,
            bool autofocus = false,
            bool? showCursor = null,
            bool showSelectionHandles = false,
            Color selectionColor = null,
            TextSelectionControls selectionControls = null,
            TextInputType keyboardType = null,
            TextInputAction? textInputAction = null,
            TextCapitalization textCapitalization = TextCapitalization.none,
            ValueChanged<string> onChanged = null,
            VoidCallback onEditingComplete = null,
            ValueChanged<string> onSubmitted = null,
            SelectionChangedCallback onSelectionChanged = null,
            VoidCallback onSelectionHandleTapped = null,
            List<TextInputFormatter> inputFormatters = null,
            bool rendererIgnoresPointer = false,
            float cursorWidth = 2.0f,
            Radius cursorRadius = null,
            bool cursorOpacityAnimates = false,
            Offset cursorOffset = null,
            bool paintCursorAboveText = false,
            BoxHeightStyle selectionHeightStyle = BoxHeightStyle.tight,
            BoxWidthStyle selectionWidthStyle = BoxWidthStyle.tight,
            EdgeInsets scrollPadding = null,
            Brightness? keyboardAppearance = Brightness.light,
            DragStartBehavior dragStartBehavior = DragStartBehavior.start,
            bool? enableInteractiveSelection = true,
            ScrollController scrollController = null,
            ScrollPhysics scrollPhysics = null,
            ToolbarOptions toolbarOptions = null,
            bool unityTouchKeyboard = false
        ) : base(key) {
            D.assert(controller != null);
            D.assert(focusNode != null);
            smartDashesType = smartDashesType ?? (obscureText ? SmartDashesType.disabled : SmartDashesType.enabled);
            smartQuotesType = smartQuotesType ?? (obscureText ? SmartQuotesType.disabled : SmartQuotesType.enabled);
            D.assert(enableInteractiveSelection != null);
            D.assert(style != null);
            D.assert(cursorColor != null);
            D.assert(backgroundCursorColor != null);
            D.assert(maxLines == null || maxLines > 0);
            D.assert(minLines == null || minLines > 0);
            D.assert(
                (maxLines == null) || (minLines == null) || (maxLines >= minLines),
                ()=>"minLines can't be greater than maxLines"
            );
            D.assert(
                !expands || (maxLines == null && minLines == null),()=>
                "minLines and maxLines must be null when expands is true."
            );
            D.assert(!obscureText || maxLines == 1, () => "Obscured fields cannot be multiline.");
            
            scrollPadding = scrollPadding ?? EdgeInsets.all(20.0f);
            D.assert(scrollPadding != null);
            toolbarOptions = toolbarOptions ?? new ToolbarOptions(
                copy: true,
                cut: true,
                paste: true,
                selectAll: true
            );
            keyboardType = keyboardType ?? (maxLines == 1 ? TextInputType.text : TextInputType.multiline);

            this.inputFormatters = inputFormatters?? new List<TextInputFormatter>();
            if (maxLines == 1) {
                this.inputFormatters.Add(BlacklistingTextInputFormatter.singleLineFormatter);
            }
            
            showCursor = showCursor ?? !readOnly;

            this.readOnly = readOnly;
            this.keyboardType = keyboardType ?? (maxLines == 1 ? TextInputType.text : TextInputType.multiline);
            this.locale = locale;
            this.scrollPadding = scrollPadding;
            this.controller = controller;
            this.focusNode = focusNode;
            this.obscureText = obscureText;
            this.autocorrect = autocorrect;
            this.style = style;
            this.smartDashesType = smartDashesType.Value;
            this.smartQuotesType = smartQuotesType.Value;
            this.showCursor = showCursor.Value;
            this.textWidthBasis = textWidthBasis;
            this.onSelectionHandleTapped = onSelectionHandleTapped;
            this.scrollController = scrollController;
            this.selectionHeightStyle = selectionHeightStyle;
            this.selectionWidthStyle = selectionWidthStyle;
            this.enableSuggestions = enableSuggestions;
            this.forceLine = forceLine;
            this.showSelectionHandles = showSelectionHandles;
            this.toolbarOptions = toolbarOptions;
            _strutStyle = strutStyle;
            this.textAlign = textAlign;
            this.textDirection = textDirection;
            this.textScaleFactor = textScaleFactor;
            this.textInputAction = textInputAction;
            this.textCapitalization = textCapitalization;
            this.cursorColor = cursorColor;
            this.backgroundCursorColor = backgroundCursorColor ; // TODO: remove ??
            this.maxLines = maxLines;
            this.minLines = minLines;
            this.expands = expands;
            this.autofocus = autofocus;
            this.selectionColor = selectionColor;
            this.onChanged = onChanged;
            this.onSubmitted = onSubmitted;
            this.onSelectionChanged = onSelectionChanged;
            this.onEditingComplete = onEditingComplete;
            this.rendererIgnoresPointer = rendererIgnoresPointer;
            this.selectionControls = selectionControls;
            this.unityTouchKeyboard = unityTouchKeyboard;
            if (maxLines == 1) {
                this.inputFormatters = new List<TextInputFormatter>();
                this.inputFormatters.Add(BlacklistingTextInputFormatter.singleLineFormatter);
                if (inputFormatters != null) {
                    this.inputFormatters.AddRange(inputFormatters);
                }
            }
            else {
                this.inputFormatters = inputFormatters;
            }

            this.cursorWidth = cursorWidth;
            this.cursorRadius = cursorRadius;
            this.cursorOpacityAnimates = cursorOpacityAnimates;
            this.cursorOffset = cursorOffset;
            this.paintCursorAboveText = paintCursorAboveText;
            this.keyboardAppearance = keyboardAppearance;
            this.enableInteractiveSelection = enableInteractiveSelection;
            this.dragStartBehavior = dragStartBehavior;
            this.scrollPhysics = scrollPhysics; 
        }

        public readonly bool readOnly;
        public readonly Locale locale;
        public readonly TextEditingController controller;
        public readonly FocusNode focusNode;
        public readonly bool obscureText;
        public readonly bool autocorrect;
        public readonly SmartDashesType smartDashesType;
        public readonly SmartQuotesType smartQuotesType;
        public readonly TextStyle style;
        public readonly bool enableSuggestions;
        public readonly TextWidthBasis textWidthBasis;
        public readonly VoidCallback onSelectionHandleTapped;
        public readonly ScrollController scrollController;
        public readonly bool showCursor;
        public readonly BoxHeightStyle selectionHeightStyle;
        public readonly BoxWidthStyle selectionWidthStyle;
        public readonly bool forceLine;
        public readonly bool showSelectionHandles ;
        public readonly ToolbarOptions toolbarOptions;
        public StrutStyle strutStyle {
            get {
                if (_strutStyle == null) {
                    return style != null
                        ? StrutStyle.fromTextStyle(style, forceStrutHeight: true)
                        :  new StrutStyle();
                }

                return _strutStyle.inheritFromTextStyle(style);
            }
        }

        readonly StrutStyle _strutStyle;
        public readonly TextAlign textAlign;
        public readonly TextDirection? textDirection;
        public readonly TextCapitalization textCapitalization;
        public readonly float? textScaleFactor;
        public readonly Color cursorColor;
        public readonly Color backgroundCursorColor;
        public readonly int? maxLines;
        public readonly int? minLines;
        public readonly bool expands;
        public readonly bool autofocus;
        public readonly Color selectionColor;
        public readonly TextSelectionControls selectionControls;
        public readonly TextInputType keyboardType;
        public readonly TextInputAction? textInputAction;
        public readonly ValueChanged<string> onChanged;
        public readonly VoidCallback onEditingComplete;
        public readonly ValueChanged<string> onSubmitted;
        public readonly SelectionChangedCallback onSelectionChanged;
        public readonly List<TextInputFormatter> inputFormatters;
        public readonly bool rendererIgnoresPointer;
        public readonly bool unityTouchKeyboard;
        public readonly float? cursorWidth;
        public readonly Radius cursorRadius;
        public readonly bool cursorOpacityAnimates;
        public readonly Offset cursorOffset;
        public readonly bool paintCursorAboveText;
        public readonly Brightness? keyboardAppearance;
        public readonly EdgeInsets scrollPadding;
        public readonly DragStartBehavior dragStartBehavior;
        public readonly bool? enableInteractiveSelection;
        public readonly ScrollPhysics scrollPhysics;
        public readonly GlobalKeyEventHandlerDelegate globalKeyEventHandler;

        public bool selectionEnabled {
            get { return enableInteractiveSelection ?? !obscureText; }
        }

        public override State createState() {
            return new EditableTextState();
        }

        public static bool debugDeterministicCursor = false;

        public override void debugFillProperties(DiagnosticPropertiesBuilder properties) {
            base.debugFillProperties(properties);
            properties.add( new DiagnosticsProperty<TextEditingController>("controller", controller));
            properties.add( new DiagnosticsProperty<FocusNode>("focusNode", focusNode));
            properties.add( new DiagnosticsProperty<bool>("obscureText", obscureText, defaultValue: false));
            properties.add( new DiagnosticsProperty<bool>("autocorrect", autocorrect, defaultValue: true));
            properties.add( new EnumProperty<SmartDashesType>("smartDashesType", smartDashesType, defaultValue: obscureText ? SmartDashesType.disabled : SmartDashesType.enabled));
            properties.add( new EnumProperty<SmartQuotesType>("smartQuotesType", smartQuotesType, defaultValue: obscureText ? SmartQuotesType.disabled : SmartQuotesType.enabled));
            properties.add( new DiagnosticsProperty<bool>("enableSuggestions", enableSuggestions, defaultValue: true));
            style?.debugFillProperties(properties);
            properties.add( new EnumProperty<TextAlign>("textAlign", textAlign, defaultValue: null));
            properties.add( new EnumProperty<TextDirection?>("textDirection", textDirection, defaultValue: null));
            properties.add( new DiagnosticsProperty<Locale>("locale", locale, defaultValue: null));
            properties.add( new FloatProperty("textScaleFactor", textScaleFactor, defaultValue: null));
            properties.add( new IntProperty("maxLines", maxLines, defaultValue: 1));
            properties.add( new IntProperty("minLines", minLines, defaultValue: null));
            properties.add( new DiagnosticsProperty<bool>("expands", expands, defaultValue: false));
            properties.add( new DiagnosticsProperty<bool>("autofocus", autofocus, defaultValue: false));
            properties.add( new DiagnosticsProperty<TextInputType>("keyboardType", keyboardType, defaultValue: null));
            properties.add( new DiagnosticsProperty<ScrollController>("scrollController", scrollController, defaultValue: null));
            properties.add( new DiagnosticsProperty<ScrollPhysics>("scrollPhysics", scrollPhysics, defaultValue: null));
        }
    }

    public class EditableTextState : AutomaticKeepAliveClientWithTickerProviderStateMixin<EditableText>,
        WidgetsBindingObserver, TextInputClient,
        TextSelectionDelegate 
    {
        const int _kObscureShowLatestCharCursorTicks = 3;
        static TimeSpan _kCursorBlinkHalfPeriod = TimeSpan.FromMilliseconds(500);
        static TimeSpan _kCursorBlinkWaitForStart = TimeSpan.FromMilliseconds(150);
        const float kMinInteractiveDimension = 48.0f;
        Timer _cursorTimer;
        bool _targetCursorVisibility = false;
        ValueNotifier<bool> _cursorVisibilityNotifier = new ValueNotifier<bool>(false);
        public readonly LayerLink _toolbarLayerLink = new LayerLink();
        public readonly LayerLink _startHandleLayerLink = new LayerLink();
        public readonly LayerLink _endHandleLayerLink = new LayerLink();
        GlobalKey _editableKey = GlobalKey.key();

        TextInputConnection _textInputConnection;
        TextSelectionOverlay _selectionOverlay;

        public ScrollController _scrollController = new ScrollController();
        AnimationController _cursorBlinkOpacityController;
        
        bool _didAutoFocus = false;
        FocusAttachment _focusAttachment;
        
        TextEditingValue _lastFormattedUnmodifiedTextEditingValue;
        TextEditingValue _lastFormattedValue;
        TextEditingValue _receivedRemoteTextEditingValue;


        static readonly TimeSpan _fadeDuration = TimeSpan.FromMilliseconds(250);

        static readonly TimeSpan _floatingCursorResetTime = TimeSpan.FromMilliseconds(125);

        AnimationController _floatingCursorResetController;

        protected override bool wantKeepAlive {
            get { return widget.focusNode.hasFocus; }
        }


        Color _cursorColor {
            get { return widget.cursorColor.withOpacity(_cursorBlinkOpacityController.value); }
        }

        public override void initState() {
            base.initState();
            widget.controller.addListener(_didChangeTextEditingValue);
            _focusAttachment = widget.focusNode.attach(context);
            widget.focusNode.addListener(_handleFocusChanged);
            _scrollController = widget.scrollController ?? new ScrollController();
            _scrollController.addListener(() => { _selectionOverlay?.updateForScroll(); });
            _cursorBlinkOpacityController = new AnimationController(vsync: this, duration: _fadeDuration);
            _cursorBlinkOpacityController.addListener(_onCursorColorTick);
            _floatingCursorResetController = new AnimationController(vsync: this);
            _floatingCursorResetController.addListener(_onFloatingCursorResetTick);
            _cursorVisibilityNotifier.value = widget.showCursor;
        }

        public override void didChangeDependencies() {
            base.didChangeDependencies();
            if (!_didAutoFocus && widget.autofocus) {
                _didAutoFocus = true;
                SchedulerBinding.instance.addPostFrameCallback((_) => {
                    if (mounted) {
                        FocusScope.of(context).autofocus(widget.focusNode);
                    }
                });
            }
        }

        public override void didUpdateWidget(StatefulWidget old) {
            EditableText oldWidget = (EditableText) old;
            base.didUpdateWidget(oldWidget);
            if (widget.controller != oldWidget.controller) {
                oldWidget.controller.removeListener(_didChangeTextEditingValue);
                widget.controller.addListener(_didChangeTextEditingValue);
                _updateRemoteEditingValueIfNeeded();
            }
            if (widget.controller.selection != oldWidget.controller.selection) {
                _selectionOverlay?.update(_value);
            }

            if (_selectionOverlay != null) {
                _selectionOverlay.handlesVisible = widget.showSelectionHandles;
            }

            if (widget.focusNode != oldWidget.focusNode) {
                oldWidget.focusNode.removeListener(_handleFocusChanged);
                _focusAttachment?.detach();
                _focusAttachment = widget.focusNode.attach(context);
                widget.focusNode.addListener(_handleFocusChanged);
                updateKeepAlive();
            }
            if (widget.readOnly) {
                _closeInputConnectionIfNeeded();
            } else {
                if (oldWidget.readOnly && _hasFocus)
                    _openInputConnection();
            }
            if (widget.style != oldWidget.style) {
                TextStyle style = widget.style;
                if (_textInputConnection != null && _textInputConnection.attached) {
                    _textInputConnection.setStyle(
                        fontFamily: style.fontFamily,
                        fontSize: style.fontSize,
                        fontWeight: style.fontWeight,
                        textDirection: _textDirection.Value,
                        textAlign: widget.textAlign
                    );
                }
            }
        }

        public override void dispose() {
            widget.controller.removeListener(_didChangeTextEditingValue);
            _cursorBlinkOpacityController.removeListener(_onCursorColorTick);
            _floatingCursorResetController.removeListener(_onFloatingCursorResetTick);
            _closeInputConnectionIfNeeded();
            D.assert(!_hasInputConnection);
            _stopCursorTimer();
            D.assert(_cursorTimer == null);
            _selectionOverlay?.dispose();
            _selectionOverlay = null;
            _focusAttachment.detach();
            widget.focusNode.removeListener(_handleFocusChanged);
            base.dispose();
        }
        TextEditingValue _lastKnownRemoteTextEditingValue;

        public void updateEditingValue(TextEditingValue value, bool isIMEInput) {
            if (widget.readOnly) {
                return;
            }
            _receivedRemoteTextEditingValue = value;
            if (value.text != _value.text) {
                hideToolbar();
                _showCaretOnScreen();
                if (widget.obscureText && value.text.Length == _value.text.Length + 1) {
                    _obscureShowCharTicksPending = !_unityKeyboard() ? _kObscureShowLatestCharCursorTicks : 0;
                    _obscureLatestCharIndex = _value.selection.baseOffset;
                }
            }
            _formatAndSetValue(value, isIMEInput);
            _stopCursorTimer(resetCharTicks: false);
            _startCursorTimer();
        }

        public void performAction(TextInputAction action) {
            switch (action) {
                case TextInputAction.newline:
                    if (!_isMultiline) {
                        _finalizeEditing(true);
                    }

                    break;
                case TextInputAction.done:
                case TextInputAction.go:
                case TextInputAction.send:
                case TextInputAction.search:
                    _finalizeEditing(true);
                    break;
                default:
                    _finalizeEditing(false);
                    break;
            }
        }

        Rect _startCaretRect;

        TextPosition _lastTextPosition;

        Offset _pointOffsetOrigin;

        Offset _lastBoundedOffset;

        Offset _floatingCursorOffset {
            get { return new Offset(0, renderEditable.preferredLineHeight / 2); }
        }

        public void updateFloatingCursor(RawFloatingCursorPoint point) {
            switch (point.state) {
                case FloatingCursorDragState.Start:
                    if (_floatingCursorResetController.isAnimating) {
                        _floatingCursorResetController.stop();
                        _onFloatingCursorResetTick();
                    }
                    TextPosition currentTextPosition =
                        new TextPosition(offset: renderEditable.selection.baseOffset);
                    _startCaretRect = renderEditable.getLocalRectForCaret(currentTextPosition);
                    renderEditable.setFloatingCursor(point.state,
                        _startCaretRect.center - _floatingCursorOffset, currentTextPosition);
                    break;
                case FloatingCursorDragState.Update:
                    // We want to send in points that are centered around a (0,0) origin, so we cache the
                    // position on the first update call.
                    if (_pointOffsetOrigin != null) {
                        Offset centeredPoint = point.offset - _pointOffsetOrigin;
                        Offset rawCursorOffset =
                            _startCaretRect.center + centeredPoint - _floatingCursorOffset;
                        _lastBoundedOffset =
                            renderEditable.calculateBoundedFloatingCursorOffset(rawCursorOffset);
                        _lastTextPosition = renderEditable.getPositionForPoint(
                            renderEditable.localToGlobal(_lastBoundedOffset + _floatingCursorOffset));
                        renderEditable.setFloatingCursor(point.state, _lastBoundedOffset,
                            _lastTextPosition);
                    }
                    else {
                        _pointOffsetOrigin = point.offset;
                    }

                    break;
                case FloatingCursorDragState.End:
                    if (_lastTextPosition != null && _lastBoundedOffset != null) {
                        _floatingCursorResetController.setValue(0.0f);
                        _floatingCursorResetController.animateTo(1.0f, duration: _floatingCursorResetTime,
                            curve: Curves.decelerate);
                    }

                    break;
            }
        }

        public RawInputKeyResponse globalInputKeyHandler(RawKeyEvent evt) {
            return widget.globalKeyEventHandler?.Invoke(evt, true) ?? RawInputKeyResponse.convert(evt);
        }

        public TextEditingValue currentTextEditingValue {
            get {
                return _value;
            }
        }
        public void connectionClosed() {
            if (_hasInputConnection) {
                _textInputConnection.connectionClosedReceived();
                _textInputConnection = null;
                _lastFormattedUnmodifiedTextEditingValue = null;
                _receivedRemoteTextEditingValue = null;
                _finalizeEditing(true);
            }
        }

        void _onFloatingCursorResetTick() {
            Offset finalPosition = renderEditable.getLocalRectForCaret(_lastTextPosition).centerLeft -
                                   _floatingCursorOffset;
            if (_floatingCursorResetController.isCompleted) {
                renderEditable.setFloatingCursor(FloatingCursorDragState.End, finalPosition,
                    _lastTextPosition);
                if (_lastTextPosition.offset != renderEditable.selection.baseOffset) {
                    _handleSelectionChanged(TextSelection.collapsed(offset: _lastTextPosition.offset),
                        renderEditable, SelectionChangedCause.forcePress);
                }

                _startCaretRect = null;
                _lastTextPosition = null;
                _pointOffsetOrigin = null;
                _lastBoundedOffset = null;
            }
            else {
                float lerpValue = _floatingCursorResetController.value;
                float lerpX = MathUtils.lerpNullableFloat(_lastBoundedOffset.dx, finalPosition.dx, lerpValue);
                float lerpY = MathUtils.lerpNullableFloat(_lastBoundedOffset.dy, finalPosition.dy, lerpValue);

                renderEditable.setFloatingCursor(FloatingCursorDragState.Update, new Offset(lerpX, lerpY),
                    _lastTextPosition, resetLerpValue: lerpValue);
            }
        }

        void _finalizeEditing(bool shouldUnfocus) {
            if (widget.onEditingComplete != null) {
                widget.onEditingComplete();
            }
            else {
                widget.controller.clearComposing();
                if (shouldUnfocus) {
                    widget.focusNode.unfocus();
                }
            }

            if (widget.onSubmitted != null) {
                widget.onSubmitted(_value.text);
            }
        }

        void _updateRemoteEditingValueIfNeeded() {
            if (!_hasInputConnection) {
                return;
            }

            var localValue = _value;
            if (localValue == _receivedRemoteTextEditingValue) {
                return;
            }
            
            _textInputConnection.setEditingState(localValue);
        }

        TextEditingValue _value {
            get { return widget.controller.value; }
            set { widget.controller.value = value; }
        }
        

        bool _hasFocus {
            get { return widget.focusNode.hasFocus; }
        }

        bool _isMultiline {
            get { return widget.maxLines != 1; }
        }

        // Calculate the new scroll offset so the cursor remains visible.
        float _getScrollOffsetForCaret(Rect caretRect) {
            float caretStart;
            float caretEnd;
            if (_isMultiline) {
                float lineHeight = renderEditable.preferredLineHeight;
                float caretOffset = (lineHeight - caretRect.height) / 2;
                caretStart = caretRect.top - caretOffset;
                caretEnd = caretRect.bottom + caretOffset;
            }
            else {
                caretStart = caretRect.left;
                caretEnd = caretRect.right;
            }

            float scrollOffset = _scrollController.offset;
            float viewportExtent = _scrollController.position.viewportDimension;
            if (caretStart < 0.0) {
                scrollOffset += caretStart;
            }
            else if (caretEnd >= viewportExtent) {
                scrollOffset += caretEnd - viewportExtent;
            }
            if (_isMultiline) {
                scrollOffset = scrollOffset.clamp(0.0f, renderEditable.maxScrollExtent);
            }
            return scrollOffset;
        }

        // Calculates where the `caretRect` would be if `_scrollController.offset` is set to `scrollOffset`.
        Rect _getCaretRectAtScrollOffset(Rect caretRect, float scrollOffset) {
            float offsetDiff = _scrollController.offset - scrollOffset;
            return _isMultiline ? caretRect.translate(0.0f, offsetDiff) : caretRect.translate(offsetDiff, 0.0f);
        }

        bool _hasInputConnection {
            get { return _textInputConnection != null && _textInputConnection.attached; }
        }

        void _openInputConnection() {
            if (widget.readOnly) {
                return;
            }
            if (!_hasInputConnection) {
                TextEditingValue localValue = _value;
                _lastFormattedUnmodifiedTextEditingValue = localValue;
                _textInputConnection = TextInput.attach(
                    this,
                    new TextInputConfiguration(
                        inputType: widget.keyboardType,
                        obscureText: widget.obscureText,
                        autocorrect: widget.autocorrect,
                        smartDashesType: widget.smartDashesType,
                        smartQuotesType: widget.smartQuotesType,
                        enableSuggestions: widget.enableSuggestions,
                        inputAction: widget.textInputAction ?? (widget.keyboardType == TextInputType.multiline
                            ? TextInputAction.newline
                            : TextInputAction.done
                        ),
                        textCapitalization: widget.textCapitalization,
                        keyboardAppearance: widget.keyboardAppearance.Value
                    )
                );
                _textInputConnection.show();

                _updateSizeAndTransform();
                TextStyle style = widget.style;
                _textInputConnection.setStyle(
                    fontFamily: style.fontFamily,
                    fontSize: style.fontSize,
                    fontWeight: style.fontWeight,
                    textDirection: _textDirection.Value,
                    textAlign: widget.textAlign
                );
                _textInputConnection.setEditingState(localValue);
            } else {
                _textInputConnection.show();
            }
        }
        void _updateSizeAndTransform() {
            if (_hasInputConnection) {
                Size size = renderEditable.size;
                Matrix4 transform = renderEditable.getTransformTo(null);
                _textInputConnection.setEditableSizeAndTransform(size, transform);
                SchedulerBinding.instance
                    .addPostFrameCallback((TimeSpan _) => _updateSizeAndTransform());
            }
        }
        
        void _closeInputConnectionIfNeeded() {
            if (_hasInputConnection) {
                _textInputConnection.close();
                _textInputConnection = null;
                _lastFormattedUnmodifiedTextEditingValue = null;
                _receivedRemoteTextEditingValue = null;
            }
        }

        void _openOrCloseInputConnectionIfNeeded() {
            if (_hasFocus && widget.focusNode.consumeKeyboardToken()) {
                _openInputConnection();
            }
            else if (!_hasFocus) {
                _closeInputConnectionIfNeeded();
                widget.controller.clearComposing();
            }
        }
        public void requestKeyboard() {
            if (_hasFocus) {
                _openInputConnection();
            }
            else {
                widget.focusNode.requestFocus();
            }
        }

        void _hideSelectionOverlayIfNeeded() {
            _selectionOverlay?.hide();
            _selectionOverlay = null;
        }

        void _updateOrDisposeSelectionOverlayIfNeeded() {
            if (_selectionOverlay != null) {
                if (_hasFocus) {
                    _selectionOverlay.update(_value);
                }
                else {
                    _selectionOverlay.dispose();
                    _selectionOverlay = null;
                }
            }
        }


        void _handleSelectionChanged(TextSelection selection, RenderEditable renderObject,
            SelectionChangedCause cause) {
            if (!widget.controller.isSelectionWithinTextBounds(selection))
                return;
            widget.controller.selection = selection;
            requestKeyboard();

            _selectionOverlay?.hide();
            _selectionOverlay = null;

            if (widget.selectionControls != null && Application.isMobilePlatform && !_unityKeyboard()) {
                _selectionOverlay = new TextSelectionOverlay(
                    context: context,
                    value: _value,
                    debugRequiredFor: widget,
                    toolbarLayerLink: _toolbarLayerLink,
                    startHandleLayerLink: _startHandleLayerLink,
                    endHandleLayerLink: _endHandleLayerLink,
                    renderObject: renderObject,
                    selectionControls: widget.selectionControls,
                    selectionDelegate: this,
                    dragStartBehavior: widget.dragStartBehavior,
                    onSelectionHandleTapped: widget.onSelectionHandleTapped
                );
                _selectionOverlay.handlesVisible = widget.showSelectionHandles;
                _selectionOverlay.showHandles();
                if (widget.onSelectionChanged != null) {
                    widget.onSelectionChanged(selection, cause);
                }
            }
        }

        bool _textChangedSinceLastCaretUpdate = false;
        Rect _currentCaretRect;

        void _handleCaretChanged(Rect caretRect) {
            _currentCaretRect = caretRect;
            // If the caret location has changed due to an update to the text or
            // selection, then scroll the caret into view.
            if (_textChangedSinceLastCaretUpdate) {
                _textChangedSinceLastCaretUpdate = false;
                _showCaretOnScreen();
            }
        }

        // Animation configuration for scrolling the caret back on screen.
        static readonly TimeSpan _caretAnimationDuration = TimeSpan.FromMilliseconds(100);
        static readonly Curve _caretAnimationCurve = Curves.fastOutSlowIn;
        bool _showCaretOnScreenScheduled = false;

        void _showCaretOnScreen() {
            if (_showCaretOnScreenScheduled) {
                return;
            }

            _showCaretOnScreenScheduled = true;
            SchedulerBinding.instance.addPostFrameCallback(_ => {
                _showCaretOnScreenScheduled = false;
                if (_currentCaretRect == null || !_scrollController.hasClients) {
                    return;
                }

                float scrollOffsetForCaret = _getScrollOffsetForCaret(_currentCaretRect);
                _scrollController.animateTo(scrollOffsetForCaret,
                    duration: _caretAnimationDuration,
                    curve: _caretAnimationCurve);

                Rect newCaretRect = _getCaretRectAtScrollOffset(_currentCaretRect, scrollOffsetForCaret);
                float bottomSpacing = widget.scrollPadding.bottom;
                if (_selectionOverlay?.selectionControls != null) {
                    float handleHeight = _selectionOverlay.selectionControls
                        .getHandleSize(renderEditable.preferredLineHeight).height;
                    float interactiveHandleHeight = Mathf.Max(
                        handleHeight,
                        kMinInteractiveDimension
                    );
                    Offset anchor = _selectionOverlay.selectionControls
                        .getHandleAnchor(
                            TextSelectionHandleType.collapsed,
                            renderEditable.preferredLineHeight
                        );
                    float handleCenter = handleHeight / 2 - anchor.dy;
                    bottomSpacing = Mathf.Max(
                        handleCenter + interactiveHandleHeight / 2,
                        bottomSpacing
                    );
                }
                Rect inflatedRect = Rect.fromLTRB(
                    newCaretRect.left - widget.scrollPadding.left,
                    newCaretRect.top - widget.scrollPadding.top,
                    newCaretRect.right + widget.scrollPadding.right,
                    newCaretRect.bottom + bottomSpacing
                );
                _editableKey.currentContext.findRenderObject().showOnScreen(
                    rect: inflatedRect,
                    duration: _caretAnimationDuration,
                    curve: _caretAnimationCurve
                );
            });
        }


        double _lastBottomViewInset;

        public void didChangeMetrics() {
            if (_lastBottomViewInset < Window.instance.viewInsets.bottom) {
                _showCaretOnScreen();
            }

            _lastBottomViewInset = Window.instance.viewInsets.bottom;
        }

        public void didChangeTextScaleFactor() {
        }

        public void didChangePlatformBrightness() {
        }

        public void didChangeLocales(List<Locale> locale) {
        }

        public Future<bool> didPopRoute() {
            return Future.value(false).to<bool>();
        }

        public Future<bool> didPushRoute(string route) {
            return Future.value(false).to<bool>();
        }


        public void didChangeAccessibilityFeatures() {}


        //_WhitespaceDirectionalityFormatter _whitespaceFormatter;

        void _formatAndSetValue(TextEditingValue value, bool isIMEInput = false) {
            //whitespaceFormatter ??= new _WhitespaceDirectionalityFormatter(textDirection: _textDirection);

            bool textChanged = _value?.text != value?.text;
            bool isRepeatText = value?.text == _lastFormattedUnmodifiedTextEditingValue?.text;
            bool isRepeatSelection = value?.selection == _lastFormattedUnmodifiedTextEditingValue?.selection; 
            bool isRepeatComposing = value?.composing == _lastFormattedUnmodifiedTextEditingValue?.composing;
           
            if (!isRepeatText && textChanged && widget.inputFormatters != null && widget.inputFormatters.isNotEmpty()) {
                foreach (TextInputFormatter formatter in widget.inputFormatters) {
                    value = formatter.formatEditUpdate(_value, value);
                }
                //value = _whitespaceFormatter.formatEditUpdate(_value, value);
                _lastFormattedValue = value;
            }

            _value = value;
           
            if (isRepeatText && isRepeatSelection && isRepeatComposing && textChanged && _lastFormattedValue != null) {
                _value = _lastFormattedValue;
            }

          
            _updateRemoteEditingValueIfNeeded();

            if (textChanged && widget.onChanged != null)
                widget.onChanged(value.text);
            _lastFormattedUnmodifiedTextEditingValue = _receivedRemoteTextEditingValue;
            
        }

        void _onCursorColorTick() {
            renderEditable.cursorColor =
                widget.cursorColor.withOpacity(_cursorBlinkOpacityController.value);
            _cursorVisibilityNotifier.value = widget.showCursor && _cursorBlinkOpacityController.value > 0;
        }

        public bool cursorCurrentlyVisible {
            get { return _cursorBlinkOpacityController.value > 0; }
        }

        public TimeSpan cursorBlinkInterval {
            get { return _kCursorBlinkHalfPeriod; }
        }

        public TextSelectionOverlay selectionOverlay {
            get { return _selectionOverlay; }
        }

        int _obscureShowCharTicksPending = 0;
        int _obscureLatestCharIndex;

        object _cursorTick(object timer) {
            _targetCursorVisibility = !_unityKeyboard() && !_targetCursorVisibility;
            float targetOpacity = _targetCursorVisibility ? 1.0f : 0.0f;
            if (widget.cursorOpacityAnimates) {
                _cursorBlinkOpacityController.animateTo(targetOpacity, curve: Curves.easeOut);
            }
            else {
                _cursorBlinkOpacityController.setValue(targetOpacity);
            }

            if (_obscureShowCharTicksPending > 0) {
                setState(() => { _obscureShowCharTicksPending--; });
            }

            return null;
        }

        object _cursorWaitForStart(object timer) {
            D.assert(_kCursorBlinkHalfPeriod > _fadeDuration);
            _cursorTimer?.cancel();
            _cursorTimer = Timer.periodic(_kCursorBlinkHalfPeriod, _cursorTick);
            return null;
        }

        void _startCursorTimer() {
            _targetCursorVisibility = true;
            _cursorBlinkOpacityController.setValue(1.0f);
            if (EditableText.debugDeterministicCursor) {
                return;
            }

            if (widget.cursorOpacityAnimates) {
                _cursorTimer =
                    Timer.periodic(_kCursorBlinkWaitForStart, _cursorWaitForStart);
            }
            else {
                _cursorTimer = Timer.periodic(_kCursorBlinkHalfPeriod, _cursorTick);
            }
        }

        void _stopCursorTimer(bool resetCharTicks = true) {
            _cursorTimer?.cancel();
            _cursorTimer = null;
            _targetCursorVisibility = false;
            _cursorBlinkOpacityController.setValue(0.0f);
            if (EditableText.debugDeterministicCursor) {
                return;
            }

            if (resetCharTicks) {
                _obscureShowCharTicksPending = 0;
            }

            if (widget.cursorOpacityAnimates) {
                _cursorBlinkOpacityController.stop();
                _cursorBlinkOpacityController.setValue(0.0f);
            }
        }

        void _startOrStopCursorTimerIfNeeded() {
            if (_cursorTimer == null && _hasFocus && _value.selection.isCollapsed) {
                _startCursorTimer();
            }
            else if (_cursorTimer != null && (!_hasFocus || !_value.selection.isCollapsed)) {
                _stopCursorTimer();
            }
        }


        void _didChangeTextEditingValue() {
            _updateRemoteEditingValueIfNeeded();
            _updateImePosIfNeed();
            _startOrStopCursorTimerIfNeeded();
            _updateOrDisposeSelectionOverlayIfNeeded();
            _textChangedSinceLastCaretUpdate = true;
            setState(() => { });
        }

        void _handleFocusChanged() {
            _openOrCloseInputConnectionIfNeeded();
            _startOrStopCursorTimerIfNeeded();
            _updateOrDisposeSelectionOverlayIfNeeded();
            if (_hasFocus) {
                WidgetsBinding.instance.addObserver(this);
                _lastBottomViewInset = Window.instance.viewInsets.bottom;
                _showCaretOnScreen();
                if (!_value.selection.isValid) {
                    widget.controller.selection = TextSelection.collapsed(offset: _value.text.Length);
                }
            }
            else {
                WidgetsBinding.instance.removeObserver(this);
                _value = new TextEditingValue(text: _value.text);
            }

            updateKeepAlive();
        }

        TextDirection? _textDirection {
            get {
                TextDirection? result = widget.textDirection ?? Directionality.of(context);
                D.assert(result != null,
                    () =>
                        $"{GetType().FullName} created without a textDirection and with no ambient Directionality.");
                return result;
            }
        }

        public RenderEditable renderEditable {
            get { return (RenderEditable) _editableKey.currentContext.findRenderObject(); }
        }

        public TextEditingValue textEditingValue {
            get { return _value; }
            set {
                _selectionOverlay?.update(value);
                _formatAndSetValue(value);
            }
        }

        float _devicePixelRatio {
            get { return MediaQuery.of(context).devicePixelRatio; }
        }

        public void bringIntoView(TextPosition position) {
            _scrollController.jumpTo(
                _getScrollOffsetForCaret(renderEditable.getLocalRectForCaret(position)));
        }

        public bool cutEnabled {
            get { return widget.toolbarOptions.cut && !widget.readOnly; }
        }
        public bool copyEnabled {
            get {
                return widget.toolbarOptions.copy;
            }
        }
        public bool pasteEnabled {
            get {
                return widget.toolbarOptions.paste && !widget.readOnly;
            }
        }
        public bool selectAllEnabled {
            get {
                return widget.toolbarOptions.selectAll;
            }
        }

        public void toggleToolbar() {
            D.assert(_selectionOverlay != null);
            if (_selectionOverlay.toolbarIsVisible) {
                hideToolbar();
            } else {
                showToolbar();
            }
        }
        public bool showToolbar() {
#pragma warning disable CS0162
            if (foundation_.kIsWeb) {
                return false;
            }
#pragma warning restore CS0162

            if (_selectionOverlay == null || _selectionOverlay.toolbarIsVisible) {
                return false;
            }

            _selectionOverlay.showToolbar();
            return true;
        }

        public void hideToolbar() {
            _selectionOverlay?.hide();
        }

        public override Widget build(BuildContext context) {
            D.assert(WidgetsD.debugCheckHasMediaQuery(context));
            _focusAttachment.reparent();
            base.build(context);

            return new Scrollable(
                axisDirection: _isMultiline ? AxisDirection.down : AxisDirection.right,
                controller: _scrollController,
                physics: widget.scrollPhysics,
                dragStartBehavior: widget.dragStartBehavior,
                viewportBuilder: (BuildContext _context, ViewportOffset offset) =>
                    new CompositedTransformTarget(
                        link: _toolbarLayerLink,
                        child: new _Editable(
                            key: _editableKey,
                              startHandleLayerLink: _startHandleLayerLink,
                              endHandleLayerLink: _endHandleLayerLink,
                              textSpan: buildTextSpan(),
                              value: _value,
                              cursorColor: _cursorColor,
                              backgroundCursorColor: widget.backgroundCursorColor,
                              showCursor: EditableText.debugDeterministicCursor
                                  ? new ValueNotifier<bool>(widget.showCursor)
                                  : _cursorVisibilityNotifier,
                              forceLine: widget.forceLine,
                              readOnly: widget.readOnly,
                              hasFocus: _hasFocus,
                              maxLines: widget.maxLines,
                              minLines: widget.minLines,
                              expands: widget.expands,
                              strutStyle: widget.strutStyle,
                              selectionColor: widget.selectionColor,
                              textScaleFactor: widget.textScaleFactor ?? MediaQuery.textScaleFactorOf(context),
                              textAlign: widget.textAlign,
                              textDirection: _textDirection,
                              locale: widget.locale,
                              textWidthBasis: widget.textWidthBasis,
                              obscureText: widget.obscureText,
                              autocorrect: widget.autocorrect,
                              smartDashesType: widget.smartDashesType,
                              smartQuotesType: widget.smartQuotesType,
                              enableSuggestions: widget.enableSuggestions,
                              offset: offset,
                              onSelectionChanged: _handleSelectionChanged,
                              onCaretChanged: _handleCaretChanged,
                              rendererIgnoresPointer: widget.rendererIgnoresPointer,
                              cursorWidth: widget.cursorWidth,
                              cursorRadius: widget.cursorRadius,
                              cursorOffset: widget.cursorOffset,
                              selectionHeightStyle: widget.selectionHeightStyle,
                              selectionWidthStyle: widget.selectionWidthStyle,
                              paintCursorAboveText: widget.paintCursorAboveText,
                              enableInteractiveSelection: widget.enableInteractiveSelection ?? true,
                              textSelectionDelegate: this,
                              devicePixelRatio: _devicePixelRatio

                        )
                    )
            );
        }

        public TextSpan buildTextSpan() {
            if (widget.obscureText) {
                var text = _value.text;
                if (widget.obscureText) {
                    text = new string(RenderEditable.obscuringCharacter, text.Length);
                    int o = _obscureShowCharTicksPending > 0 ? _obscureLatestCharIndex : -1;
                    if (o >= 0 && o < text.Length) {
                        text = text.Substring(0, o) + _value.text.Substring(o, 1) + text.Substring(o + 1);
                    }
                }
                return new TextSpan(style: widget.style, text: text);
            }
            
            return widget.controller.buildTextSpan(
                style: widget.style,
                withComposing: !widget.readOnly
            );
        }

       
        bool _unityKeyboard() {
            return TouchScreenKeyboard.isSupported && widget.unityTouchKeyboard;
        }

        Offset _getImePos() {
            if (_hasInputConnection && _textInputConnection.imeRequired()) {
                var localPos = renderEditable.getLocalRectForCaret(_value.selection.basePos).bottomLeft;
                return renderEditable.localToGlobal(localPos);
            }

            return null;
        }

        bool _imePosUpdateScheduled = false;

        void _updateImePosIfNeed() {
            if (!_hasInputConnection || !_textInputConnection.imeRequired()) {
                return;
            }

            if (_imePosUpdateScheduled) {
                return;
            }

            _imePosUpdateScheduled = true;
            SchedulerBinding.instance.addPostFrameCallback(_ => {
                _imePosUpdateScheduled = false;
                if (!_hasInputConnection) {
                    return;
                }

                _textInputConnection.setIMEPos(_getImePos());
            });
        }
        
    }
    

    class _Editable : LeafRenderObjectWidget {
        public readonly TextSpan textSpan;
        public readonly TextEditingValue value;
        public readonly Color cursorColor;
        public readonly LayerLink startHandleLayerLink;
        public readonly LayerLink endHandleLayerLink;
        public readonly Color backgroundCursorColor;
        public readonly ValueNotifier<bool> showCursor;
        public readonly bool forceLine;
        public readonly bool readOnly;
        public readonly bool hasFocus;
        public readonly int? maxLines;
        public readonly int? minLines;
        public readonly bool expands;
        public readonly StrutStyle strutStyle;
        public readonly Color selectionColor;
        public readonly float textScaleFactor;
        public readonly TextAlign textAlign;
        public readonly TextDirection? textDirection;
        public readonly bool obscureText;
        public readonly TextWidthBasis textWidthBasis;
        public readonly bool autocorrect;
        public readonly SmartDashesType smartDashesType;
        public readonly SmartQuotesType smartQuotesType;
        public readonly bool? enableSuggestions;
        public readonly ViewportOffset offset;
        public readonly SelectionChangedHandler onSelectionChanged;
        public readonly CaretChangedHandler onCaretChanged;
        public readonly bool rendererIgnoresPointer;
        public readonly float? cursorWidth;
        public readonly Radius cursorRadius;
        public readonly Offset cursorOffset;
        public readonly BoxHeightStyle selectionHeightStyle;
        public readonly BoxWidthStyle selectionWidthStyle;
        public readonly bool enableInteractiveSelection;
        public readonly TextSelectionDelegate textSelectionDelegate;
        public readonly bool? paintCursorAboveText;
        public readonly float? devicePixelRatio;
        public readonly Locale locale;
        public _Editable(
            Key key = null,
            TextSpan textSpan = null,
            TextEditingValue value = null,
            LayerLink startHandleLayerLink = null,
            LayerLink endHandleLayerLink = null,
            Color cursorColor = null,
            Color backgroundCursorColor = null,
            ValueNotifier<bool> showCursor = null,
            bool forceLine = false,
            bool readOnly = false,
            TextWidthBasis textWidthBasis = TextWidthBasis.parent,
            bool hasFocus = false,
            int? maxLines = null,
            int? minLines = null,
            bool expands = false,
            StrutStyle strutStyle = null,
            Color selectionColor = null,
            float textScaleFactor = 1.0f,
            TextAlign textAlign = TextAlign.start,
            TextDirection? textDirection = null,
            Locale locale = null,
            bool obscureText = false,
            bool autocorrect = false,
            SmartDashesType? smartDashesType = null,
            SmartQuotesType? smartQuotesType = null,
            bool? enableSuggestions = null,
            ViewportOffset offset = null,
            SelectionChangedHandler onSelectionChanged = null,
            CaretChangedHandler onCaretChanged = null,
            bool rendererIgnoresPointer = false,
            float? cursorWidth = null,
            Radius cursorRadius = null,
            Offset cursorOffset = null,
            bool? paintCursorAboveText = null,
            BoxHeightStyle selectionHeightStyle = BoxHeightStyle.tight,
            BoxWidthStyle selectionWidthStyle = BoxWidthStyle.tight,
            bool enableInteractiveSelection = true,
            TextSelectionDelegate textSelectionDelegate = null,
            float? devicePixelRatio = null
        ) : base(key) {
            this.textSpan = textSpan;
            this.value = value;
            this.startHandleLayerLink = startHandleLayerLink;
            this.endHandleLayerLink = endHandleLayerLink;
            this.cursorColor = cursorColor;
            this.locale = locale;
            this.backgroundCursorColor = backgroundCursorColor;
            this.showCursor = showCursor;
            this.forceLine = forceLine;
            this.readOnly = readOnly;
            this.textWidthBasis = textWidthBasis;
            this.hasFocus = hasFocus;
            this.maxLines = maxLines;
            this.minLines = minLines;
            this.expands = expands;
            this.strutStyle = strutStyle;
            this.selectionColor = selectionColor;
            this.textScaleFactor = textScaleFactor;
            this.textAlign = textAlign;
            this.textDirection = textDirection;
            this.obscureText = obscureText;
            this.autocorrect = autocorrect;
            this.smartDashesType = smartDashesType.Value;
            this.smartQuotesType = smartQuotesType.Value;
            this.enableSuggestions = enableSuggestions;
            this.offset = offset;
            this.onSelectionChanged = onSelectionChanged;
            this.onCaretChanged = onCaretChanged;
            this.rendererIgnoresPointer = rendererIgnoresPointer;
            this.textSelectionDelegate = textSelectionDelegate;
            this.cursorWidth = cursorWidth;
            this.cursorRadius = cursorRadius;
            this.cursorOffset = cursorOffset;
            this.paintCursorAboveText = paintCursorAboveText;
            this.selectionHeightStyle = selectionHeightStyle;
            this.selectionWidthStyle = selectionWidthStyle;
            this.enableInteractiveSelection = enableInteractiveSelection;
            this.devicePixelRatio = devicePixelRatio;
        }

        public override RenderObject createRenderObject(BuildContext context) {
            return new RenderEditable(
                text: textSpan,
                cursorColor: cursorColor,
                startHandleLayerLink: startHandleLayerLink,
                endHandleLayerLink: endHandleLayerLink,
                backgroundCursorColor: backgroundCursorColor,
                showCursor: showCursor,
                forceLine: forceLine,
                readOnly: readOnly,
                hasFocus: hasFocus,
                maxLines: maxLines,
                minLines: minLines,
                expands: expands,
                strutStyle: strutStyle,
                selectionColor: selectionColor,
                textScaleFactor: textScaleFactor,
                textAlign: textAlign,
                textDirection: textDirection,
                locale: locale ?? Localizations.localeOf(context, nullOk: true),
                selection: value.selection,
                offset: offset,
                onSelectionChanged: onSelectionChanged,
                onCaretChanged: onCaretChanged,
                ignorePointer: rendererIgnoresPointer,
                obscureText: obscureText,
                textWidthBasis: textWidthBasis,
                cursorWidth: cursorWidth ?? 1.0f,
                cursorRadius: cursorRadius,
                cursorOffset: cursorOffset,
                paintCursorAboveText: paintCursorAboveText ?? false,
                selectionHeightStyle: selectionHeightStyle,
                selectionWidthStyle: selectionWidthStyle,
                enableInteractiveSelection: enableInteractiveSelection,
                textSelectionDelegate: textSelectionDelegate,
                devicePixelRatio: devicePixelRatio ?? 1.0f
            );
        }

        public override void updateRenderObject(BuildContext context, RenderObject renderObject) {
            var edit = (RenderEditable) renderObject;
            edit.text = textSpan;
            edit.cursorColor = cursorColor;
            edit.startHandleLayerLink = startHandleLayerLink;
            edit.endHandleLayerLink = endHandleLayerLink;
            edit.showCursor = showCursor;
            edit.forceLine = forceLine;
            edit.readOnly = readOnly;
            edit.hasFocus = hasFocus;
            edit.maxLines = maxLines;
            edit.minLines = minLines;
            edit.expands = expands;
            edit.strutStyle = strutStyle;
            edit.selectionColor = selectionColor;
            edit.textScaleFactor = textScaleFactor;
            edit.textAlign = textAlign;
            edit.textDirection = textDirection;
            edit.locale = locale ?? Localizations.localeOf(context, nullOk: true);
            edit.selection = value.selection;
            edit.offset = offset;
            edit.onSelectionChanged = onSelectionChanged;
            edit.onCaretChanged = onCaretChanged;
            edit.ignorePointer = rendererIgnoresPointer;
            edit.textWidthBasis = textWidthBasis;
            edit.obscureText = obscureText;
            edit.cursorWidth = cursorWidth ?? 0.0f;
            edit.cursorRadius = cursorRadius;
            edit.cursorOffset = cursorOffset;
            edit.selectionHeightStyle = selectionHeightStyle;
            edit.selectionWidthStyle = selectionWidthStyle;
            edit.textSelectionDelegate = textSelectionDelegate;
            edit.devicePixelRatio = devicePixelRatio ?? 1.0f;
            edit.paintCursorAboveText = paintCursorAboveText ?? false;
        }
    }
}