﻿using System;
using System.Collections.Generic;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.gestures;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.service;
using Unity.UIWidgets.ui;
using UnityEngine;
using Canvas = Unity.UIWidgets.ui.Canvas;
using Color = Unity.UIWidgets.ui.Color;
using Rect = Unity.UIWidgets.ui.Rect;
using StrutStyle = Unity.UIWidgets.painting.StrutStyle;

namespace Unity.UIWidgets.rendering {
    class EditableUtils {
        public static readonly float _kCaretGap = 1.0f;
        public static readonly float _kCaretHeightOffset = 2.0f;
        public static readonly Offset _kFloatingCaretSizeIncrease = new Offset(0.5f, 1.0f);
        public static readonly float _kFloatingCaretRadius = 1.0f;
    }

    public delegate void SelectionChangedHandler(TextSelection selection, RenderEditable renderObject,
        SelectionChangedCause cause);

    public delegate void CaretChangedHandler(Rect caretRect);

    public enum SelectionChangedCause {
        tap,
        doubleTap,
        longPress,
        forcePress,
        keyboard,
        drag
    }

    public class TextSelectionPoint {
        public readonly Offset point;
        public readonly TextDirection? direction;

        public TextSelectionPoint(Offset point, TextDirection? direction) {
            D.assert(point != null);
            this.point = point;
            this.direction = direction;
        }

        public override string ToString() {
            return $"Point: {point}, Direction: {direction}";
        }
    }

    public class RenderEditable : RenderBox {
        public RenderEditable(
            TextSpan text,
            TextDirection textDirection,
            TextAlign textAlign = TextAlign.left,
            Color cursorColor = null,
            Color backgroundCursorColor = null,
            ValueNotifier<bool> showCursor = null,
            bool? hasFocus = null,
            int? maxLines = 1,
            int? minLines = null,
            bool expands = false,
            StrutStyle strutStyle = null,
            Color selectionColor = null,
            float textScaleFactor = 1.0f,
            TextSelection selection = null,
            ViewportOffset offset = null,
            SelectionChangedHandler onSelectionChanged = null,
            CaretChangedHandler onCaretChanged = null,
            bool ignorePointer = false,
            bool obscureText = false,
            float cursorWidth = 1.0f,
            Radius cursorRadius = null,
            bool paintCursorAboveText = false,
            Offset cursorOffset = null,
            float devicePixelRatio = 1.0f,
            bool? enableInteractiveSelection = null,
            EdgeInsets floatingCursorAddedMargin = null,
            TextSelectionDelegate textSelectionDelegate = null,
            GlobalKeyEventHandlerDelegate globalKeyEventHandler = null) {
            floatingCursorAddedMargin = floatingCursorAddedMargin ?? EdgeInsets.fromLTRB(4, 4, 4, 5);
            D.assert(textSelectionDelegate != null);
            D.assert(minLines == null || minLines > 0);
            D.assert(maxLines == null || maxLines > 0);
            D.assert((maxLines == null) || (minLines == null) || maxLines >= minLines,
                () => "minLines can't be greater than maxLines");
            D.assert(offset != null);
            D.assert(cursorWidth >= 0.0f);
            _textPainter = new TextPainter(
                text: text,
                textAlign: textAlign,
                textDirection: textDirection,
                textScaleFactor: textScaleFactor,
                strutStyle: strutStyle);
            _cursorColor = cursorColor;
            _backgroundCursorColor = backgroundCursorColor;
            _showCursor = showCursor ?? new ValueNotifier<bool>(false);
            _hasFocus = hasFocus ?? false;
            _maxLines = maxLines;
            _minLines = minLines;
            _expands = expands;
            _selectionColor = selectionColor;
            _selection = selection;
            _obscureText = obscureText;
            _offset = offset;
            _cursorWidth = cursorWidth;
            _cursorRadius = cursorRadius;
            _enableInteractiveSelection = enableInteractiveSelection;
            this.ignorePointer = ignorePointer;
            this.onCaretChanged = onCaretChanged;
            this.onSelectionChanged = onSelectionChanged;
            this.textSelectionDelegate = textSelectionDelegate;
            this.globalKeyEventHandler = globalKeyEventHandler;

            D.assert(_maxLines == null || _maxLines > 0);
            D.assert(_showCursor != null);
            D.assert(!_showCursor.value || cursorColor != null);

            _tap = new TapGestureRecognizer(this);
            _doubleTap = new DoubleTapGestureRecognizer(this);
            _tap.onTapDown = _handleTapDown;
            _tap.onTap = _handleTap;
            _doubleTap.onDoubleTap = _handleDoubleTap;
            _longPress = new LongPressGestureRecognizer(debugOwner: this);
            _longPress.onLongPress = _handleLongPress;

            _paintCursorOnTop = paintCursorAboveText;
            _cursorOffset = cursorOffset;
            _floatingCursorAddedMargin = floatingCursorAddedMargin;
            _devicePixelRatio = devicePixelRatio;
        }

        public static readonly char obscuringCharacter = '•';
        public SelectionChangedHandler onSelectionChanged;
        float? _textLayoutLastWidth;
        public CaretChangedHandler onCaretChanged;
        public bool ignorePointer;

        float _devicePixelRatio;

        public float devicePixelRatio {
            get { return _devicePixelRatio; }
            set {
                if (devicePixelRatio == value) {
                    return;
                }

                _devicePixelRatio = value;
                markNeedsTextLayout();
            }
        }

        bool _obscureText;

        public bool obscureText {
            get { return _obscureText; }
            set {
                if (_obscureText == value) {
                    return;
                }

                _obscureText = value;
                markNeedsSemanticsUpdate();
            }
        }

        public TextSelectionDelegate textSelectionDelegate;
        public GlobalKeyEventHandlerDelegate globalKeyEventHandler;
        Rect _lastCaretRect;


        public ValueListenable<bool> selectionStartInViewport {
            get { return _selectionStartInViewport; }
        }

        readonly ValueNotifier<bool> _selectionStartInViewport = new ValueNotifier<bool>(true);

        public ValueListenable<bool> selectionEndInViewport {
            get { return _selectionEndInViewport; }
        }

        readonly ValueNotifier<bool> _selectionEndInViewport = new ValueNotifier<bool>(true);


        DoubleTapGestureRecognizer _doubleTap;

        void _updateSelectionExtentsVisibility(Offset effectiveOffset) {
            Rect visibleRegion = Offset.zero & size;
            Offset startOffset = _textPainter.getOffsetForCaret(
                new TextPosition(offset: _selection.start, affinity: _selection.affinity),
                Rect.zero
            );

            float visibleRegionSlop = 0.5f;
            _selectionStartInViewport.value = visibleRegion
                .inflate(visibleRegionSlop)
                .contains(startOffset + effectiveOffset);

            Offset endOffset = _textPainter.getOffsetForCaret(
                new TextPosition(offset: _selection.end, affinity: _selection.affinity),
                Rect.zero
            );
            _selectionEndInViewport.value = visibleRegion
                .inflate(visibleRegionSlop)
                .contains(endOffset + effectiveOffset);
        }

        int _extentOffset = -1;

        int _baseOffset = -1;

        int _previousCursorLocation = -1;

        bool _resetCursor = false;

        void _handleKeyEvent(RawKeyEvent keyEvent) {
            if (keyEvent is RawKeyUpEvent) {
                return;
            }

            if (selection.isCollapsed) {
                _extentOffset = selection.extentOffset;
                _baseOffset = selection.baseOffset;
            }
            
            if (globalKeyEventHandler?.Invoke(keyEvent, false)?.swallow ?? false) {
                return;
            }

            KeyCode pressedKeyCode = keyEvent.data.unityEvent.keyCode;
            int modifiers = (int) keyEvent.data.unityEvent.modifiers;
            bool shift = (modifiers & (int) EventModifiers.Shift) > 0;
            bool ctrl = (modifiers & (int) EventModifiers.Control) > 0;
            bool alt = (modifiers & (int) EventModifiers.Alt) > 0;
            bool cmd = (modifiers & (int) EventModifiers.Command) > 0;

            bool rightArrow = pressedKeyCode == KeyCode.RightArrow;
            bool leftArrow = pressedKeyCode == KeyCode.LeftArrow;
            bool upArrow = pressedKeyCode == KeyCode.UpArrow;
            bool downArrow = pressedKeyCode == KeyCode.DownArrow;
            bool arrow = leftArrow || rightArrow || upArrow || downArrow;
            bool aKey = pressedKeyCode == KeyCode.A;
            bool xKey = pressedKeyCode == KeyCode.X;
            bool vKey = pressedKeyCode == KeyCode.V;
            bool cKey = pressedKeyCode == KeyCode.C;
            bool del = pressedKeyCode == KeyCode.Delete;
            bool isMac = SystemInfo.operatingSystemFamily == OperatingSystemFamily.MacOSX;

            if (keyEvent is RawKeyCommandEvent) {
                // editor case
                _handleShortcuts(((RawKeyCommandEvent) keyEvent).command);
                return;
            }

            if ((ctrl || (isMac && cmd)) && (xKey || vKey || cKey || aKey)) {
                // runtime case
                if (xKey) {
                    _handleShortcuts(KeyCommand.Cut);
                }
                else if (aKey) {
                    _handleShortcuts(KeyCommand.SelectAll);
                }
                else if (vKey) {
                    _handleShortcuts(KeyCommand.Paste);
                }
                else if (cKey) {
                    _handleShortcuts(KeyCommand.Copy);
                }

                return;
            }

            if (arrow) {
                int newOffset = _extentOffset;
                var word = (isMac && alt) || ctrl;
                if (word) {
                    newOffset = _handleControl(rightArrow, leftArrow, word, newOffset);
                }

                newOffset = _handleHorizontalArrows(rightArrow, leftArrow, shift, newOffset);
                if (downArrow || upArrow) {
                    newOffset = _handleVerticalArrows(upArrow, downArrow, shift, newOffset);
                }

                newOffset = _handleShift(rightArrow, leftArrow, shift, newOffset);

                _extentOffset = newOffset;
            }

            if (del) {
                _handleDelete();
            }
        }

        int _handleControl(bool rightArrow, bool leftArrow, bool ctrl, int newOffset) {
            // If control is pressed, we will decide which way to look for a word
            // based on which arrow is pressed.
            if (leftArrow && _extentOffset > 2) {
                TextSelection textSelection =
                    _selectWordAtOffset(new TextPosition(offset: _extentOffset - 2));
                newOffset = textSelection.baseOffset + 1;
            }
            else if (rightArrow && _extentOffset < text.text.Length - 2) {
                TextSelection textSelection =
                    _selectWordAtOffset(new TextPosition(offset: _extentOffset + 1));
                newOffset = textSelection.extentOffset - 1;
            }

            return newOffset;
        }

        int _handleHorizontalArrows(bool rightArrow, bool leftArrow, bool shift, int newOffset) {
            if (rightArrow && _extentOffset < text.text.Length) {
                if (newOffset < text.text.Length - 1 && char.IsHighSurrogate(text.text[newOffset])) {
                    // handle emoji, which takes 2 bytes
                    newOffset += 2;
                    if (shift) {
                        _previousCursorLocation += 2;
                    }
                }
                else {
                    newOffset += 1;
                    if (shift) {
                        _previousCursorLocation += 1;
                    }
                }
            }

            if (leftArrow && _extentOffset > 0) {
                if (newOffset > 1 && char.IsLowSurrogate(text.text[newOffset - 1])) {
                    // handle emoji, which takes 2 bytes
                    newOffset -= 2;
                    if (shift) {
                        _previousCursorLocation -= 2;
                    }
                }
                else {
                    newOffset -= 1;
                    if (shift) {
                        _previousCursorLocation -= 1;
                    }
                }
            }

            return newOffset;
        }

        int _handleVerticalArrows(bool upArrow, bool downArrow, bool shift, int newOffset) {
            float plh = _textPainter.preferredLineHeight;
            float verticalOffset = upArrow ? -0.5f * plh : 1.5f * plh;

            Offset caretOffset =
                _textPainter.getOffsetForCaret(new TextPosition(offset: _extentOffset), _caretPrototype);
            Offset caretOffsetTranslated = caretOffset.translate(0.0f, verticalOffset);
            TextPosition position = _textPainter.getPositionForOffset(caretOffsetTranslated);

            if (position.offset == _extentOffset) {
                if (downArrow) {
                    newOffset = text.text.Length;
                }
                else if (upArrow) {
                    newOffset = 0;
                }

                _resetCursor = shift;
            }
            else if (_resetCursor && shift) {
                newOffset = _previousCursorLocation;
                _resetCursor = false;
            }
            else {
                newOffset = position.offset;
                _previousCursorLocation = newOffset;
            }

            return newOffset;
        }

        int _handleShift(bool rightArrow, bool leftArrow, bool shift, int newOffset) {
            if (onSelectionChanged == null) {
                return newOffset;
            }

            if (shift) {
                if (_baseOffset < newOffset) {
                    onSelectionChanged(
                        new TextSelection(
                            baseOffset: _baseOffset,
                            extentOffset: newOffset
                        ),
                        this,
                        SelectionChangedCause.keyboard
                    );
                }
                else {
                    onSelectionChanged(
                        new TextSelection(
                            baseOffset: newOffset,
                            extentOffset: _baseOffset
                        ),
                        this,
                        SelectionChangedCause.keyboard
                    );
                }
            }
            else {
                if (!selection.isCollapsed) {
                    if (leftArrow) {
                        newOffset = _baseOffset < _extentOffset ? _baseOffset : _extentOffset;
                    }
                    else if (rightArrow) {
                        newOffset = _baseOffset > _extentOffset ? _baseOffset : _extentOffset;
                    }
                }

                onSelectionChanged(
                    TextSelection.fromPosition(
                        new TextPosition(
                            offset: newOffset
                        )
                    ),
                    this,
                    SelectionChangedCause.keyboard
                );
            }

            return newOffset;
        }

        void _handleShortcuts(KeyCommand cmd) {
            switch (cmd) {
                case KeyCommand.Copy:
                    if (!selection.isCollapsed) {
                        Clipboard.setData(
                            new ClipboardData(text: selection.textInside(text.text)));
                    }

                    break;
                case KeyCommand.Cut:
                    if (!selection.isCollapsed) {
                        Clipboard.setData(
                            new ClipboardData(text: selection.textInside(text.text)));
                        textSelectionDelegate.textEditingValue = new TextEditingValue(
                            text: selection.textBefore(text.text)
                                  + selection.textAfter(text.text),
                            selection: TextSelection.collapsed(offset: selection.start)
                        );
                    }

                    break;
                case KeyCommand.Paste:
                    TextEditingValue value = textSelectionDelegate.textEditingValue;
                    Clipboard.getData(Clipboard.kTextPlain).then_(data => {
                        if (data != null) {
                            textSelectionDelegate.textEditingValue = new TextEditingValue(
                                text: value.selection.textBefore(value.text)
                                      + data.text
                                      + value.selection.textAfter(value.text),
                                selection: TextSelection.collapsed(
                                    offset: value.selection.start + data.text.Length
                                )
                            );
                        }
                    });

                    break;
                case KeyCommand.SelectAll:
                    _baseOffset = 0;
                    _extentOffset = textSelectionDelegate.textEditingValue.text.Length;
                    onSelectionChanged(
                        new TextSelection(
                            baseOffset: 0,
                            extentOffset: textSelectionDelegate.textEditingValue.text.Length
                        ),
                        this,
                        SelectionChangedCause.keyboard
                    );
                    break;
                default:
                    D.assert(false);
                    break;
            }
        }

        void _handleDelete() {
            var selection = this.selection;
            if (selection.textAfter(text.text).isNotEmpty()) {
                if (char.IsHighSurrogate(text.text[selection.end])) {
                    textSelectionDelegate.textEditingValue = new TextEditingValue(
                        text: selection.textBefore(text.text)
                              + selection.textAfter(text.text).Substring(2),
                        selection: TextSelection.collapsed(offset: selection.start)
                    );
                }
                else {
                    textSelectionDelegate.textEditingValue = new TextEditingValue(
                        text: selection.textBefore(text.text)
                              + selection.textAfter(text.text).Substring(1),
                        selection: TextSelection.collapsed(offset: selection.start)
                    );
                }
            }
            else {
                textSelectionDelegate.textEditingValue = new TextEditingValue(
                    text: selection.textBefore(text.text),
                    selection: TextSelection.collapsed(offset: selection.start)
                );
            }
        }

        protected void markNeedsTextLayout() {
            _textLayoutLastWidth = null;
            markNeedsLayout();
        }

        TextPainter _textPainter;

        public TextSpan text {
            get { return _textPainter.text as TextSpan; }
            set {
                if (_textPainter.text == value) {
                    return;
                }

                _textPainter.text = value;
                markNeedsTextLayout();
                markNeedsSemanticsUpdate();
            }
        }

        public TextAlign textAlign {
            get { return _textPainter.textAlign; }
            set {
                if (_textPainter.textAlign == value) {
                    return;
                }

                _textPainter.textAlign = value;
                markNeedsPaint();
            }
        }

        public TextDirection? textDirection {
            get { return _textPainter.textDirection; }
            set {
                if (_textPainter.textDirection == value) {
                    return;
                }

                _textPainter.textDirection = value;
                markNeedsTextLayout();
                markNeedsSemanticsUpdate();
            }
        }

        public StrutStyle strutStyle {
            get { return _textPainter.strutStyle; }
            set {
                if (_textPainter.strutStyle == value) {
                    return;
                }

                _textPainter.strutStyle = value;
                markNeedsTextLayout();
            }
        }

        Color _cursorColor;

        public Color cursorColor {
            get { return _cursorColor; }
            set {
                if (_cursorColor == value) {
                    return;
                }

                _cursorColor = value;
                markNeedsPaint();
            }
        }

        Color _backgroundCursorColor;

        public Color backgroundCursorColor {
            get { return _backgroundCursorColor; }
            set {
                if (backgroundCursorColor == value) {
                    return;
                }

                _backgroundCursorColor = value;
                markNeedsPaint();
            }
        }


        ValueNotifier<bool> _showCursor;

        public ValueNotifier<bool> showCursor {
            get { return _showCursor; }
            set {
                D.assert(value != null);
                if (_showCursor == value) {
                    return;
                }

                if (attached) {
                    _showCursor.removeListener(markNeedsPaint);
                }

                _showCursor = value;
                if (attached) {
                    _showCursor.addListener(markNeedsPaint);
                }

                markNeedsPaint();
            }
        }

        bool _hasFocus = false;
        bool _listenerAttached = false;

        public bool hasFocus {
            get { return _hasFocus; }
            set {
                if (_hasFocus == value) {
                    return;
                }

                _hasFocus = value;
                if (_hasFocus) {
                    D.assert(!_listenerAttached);
                    RawKeyboard.instance.addListener(_handleKeyEvent);
                    _listenerAttached = true;
                }
                else {
                    D.assert(_listenerAttached);
                    RawKeyboard.instance.removeListener(_handleKeyEvent);
                    _listenerAttached = false;
                }

                markNeedsSemanticsUpdate();
            }
        }

        int? _maxLines;

        public int? maxLines {
            get { return _maxLines; }
            set {
                D.assert(value == null || value > 0);
                if (_maxLines == value) {
                    return;
                }

                _maxLines = value;
                markNeedsTextLayout();
            }
        }

        int? _minLines;

        public int? minLines {
            get { return _minLines; }
            set {
                D.assert(value == null || value > 0);
                if (_minLines == value) {
                    return;
                }

                _minLines = value;
                markNeedsTextLayout();
            }
        }

        bool _expands;

        public bool expands {
            get { return _expands; }
            set {
                if (expands == value) {
                    return;
                }

                _expands = value;
                markNeedsTextLayout();
            }
        }

        Color _selectionColor;

        public Color selectionColor {
            get { return _selectionColor; }
            set {
                if (_selectionColor == value) {
                    return;
                }

                _selectionColor = value;
                markNeedsPaint();
            }
        }

        public float textScaleFactor {
            get { return _textPainter.textScaleFactor; }
            set {
                if (_textPainter.textScaleFactor == value) {
                    return;
                }

                _textPainter.textScaleFactor = value;
                markNeedsTextLayout();
            }
        }

        List<TextBox> _selectionRects;

        TextSelection _selection;

        public TextSelection selection {
            get { return _selection; }
            set {
                if (_selection == value) {
                    return;
                }

                _selection = value;
                _selectionRects = null;
                markNeedsPaint();
                markNeedsSemanticsUpdate();
            }
        }

        ViewportOffset _offset;

        public ViewportOffset offset {
            get { return _offset; }
            set {
                D.assert(offset != null);
                if (_offset == value) {
                    return;
                }

                if (attached) {
                    _offset.removeListener(markNeedsPaint);
                }

                _offset = value;
                if (attached) {
                    _offset.addListener(markNeedsPaint);
                }

                markNeedsLayout();
            }
        }

        float _cursorWidth = 1.0f;

        public float cursorWidth {
            get { return _cursorWidth; }
            set {
                if (_cursorWidth == value) {
                    return;
                }

                _cursorWidth = value;
                markNeedsLayout();
            }
        }


        bool _paintCursorOnTop;

        public bool paintCursorAboveText {
            get { return _paintCursorOnTop; }
            set {
                if (_paintCursorOnTop == value) {
                    return;
                }

                _paintCursorOnTop = value;
                markNeedsLayout();
            }
        }

        Offset _cursorOffset;

        public Offset cursorOffset {
            get { return _cursorOffset; }
            set {
                if (_cursorOffset == value) {
                    return;
                }

                _cursorOffset = value;
                markNeedsLayout();
            }
        }

        Radius _cursorRadius;

        public Radius cursorRadius {
            get { return _cursorRadius; }
            set {
                if (_cursorRadius == value) {
                    return;
                }

                _cursorRadius = value;
                markNeedsLayout();
            }
        }

        public EdgeInsets floatingCursorAddedMargin {
            get { return _floatingCursorAddedMargin; }
            set {
                if (_floatingCursorAddedMargin == value) {
                    return;
                }

                _floatingCursorAddedMargin = value;
                markNeedsPaint();
            }
        }

        EdgeInsets _floatingCursorAddedMargin;

        bool _floatingCursorOn = false;
        Offset _floatingCursorOffset;
        TextPosition _floatingCursorTextPosition;


        bool? _enableInteractiveSelection;

        public bool? enableInteractiveSelection {
            get { return _enableInteractiveSelection; }
            set {
                if (_enableInteractiveSelection == value) {
                    return;
                }

                _enableInteractiveSelection = value;
                markNeedsTextLayout();
                markNeedsSemanticsUpdate();
            }
        }

        public bool selectionEnabled {
            get { return enableInteractiveSelection ?? !obscureText; }
        }


        public override void attach(object ownerObject) {
            base.attach(ownerObject);
            _offset.addListener(markNeedsLayout);
            _showCursor.addListener(markNeedsPaint);
        }

        public override void detach() {
            _offset.removeListener(markNeedsLayout);
            _showCursor.removeListener(markNeedsPaint);
            if (_listenerAttached) {
                RawKeyboard.instance.removeListener(_handleKeyEvent);
            }

            base.detach();
        }

        bool _isMultiline {
            get { return _maxLines != 1; }
        }

        Axis _viewportAxis {
            get { return _isMultiline ? Axis.vertical : Axis.horizontal; }
        }

        Offset _paintOffset {
            get {
                switch (_viewportAxis) {
                    case Axis.horizontal:
                        return new Offset(-offset.pixels, 0.0f);
                    case Axis.vertical:
                        return new Offset(0.0f, -offset.pixels);
                }

                return null;
            }
        }

        float _viewportExtent {
            get {
                D.assert(hasSize);
                switch (_viewportAxis) {
                    case Axis.horizontal:
                        return size.width;
                    case Axis.vertical:
                        return size.height;
                }

                return 0.0f;
            }
        }

        float _getMaxScrollExtent(Size contentSize) {
            D.assert(hasSize);
            switch (_viewportAxis) {
                case Axis.horizontal:
                    return Mathf.Max(0.0f, contentSize.width - size.width);
                case Axis.vertical:
                    return Mathf.Max(0.0f, contentSize.height - size.height);
            }

            return 0.0f;
        }

        float _maxScrollExtent = 0;

        bool _hasVisualOverflow {
            get { return _maxScrollExtent > 0 || _paintOffset != Offset.zero; }
        }


        /// Returns the local coordinates of the endpoints of the given selection.
        ///
        /// If the selection is collapsed (and therefore occupies a single point), the
        /// returned list is of length one. Otherwise, the selection is not collapsed
        /// and the returned list is of length two. In this case, however, the two
        /// points might actually be co-located (e.g., because of a bidirectional
        /// selection that contains some text but whose ends meet in the middle).
        ///
        public List<TextSelectionPoint> getEndpointsForSelection(TextSelection selection) {
            D.assert(constraints != null);
            _layoutText(constraints.maxWidth);
            var paintOffset = _paintOffset;
            if (selection.isCollapsed) {
                var caretOffset = _textPainter.getOffsetForCaret(selection.extendPos, _caretPrototype);
                var start = new Offset(0.0f, preferredLineHeight) + caretOffset + paintOffset;
                return new List<TextSelectionPoint> {new TextSelectionPoint(start, null)};
            }
            else {
                var boxes = _textPainter.getBoxesForSelection(selection);
                var start = new Offset(boxes[0].start, boxes[0].bottom) + paintOffset;
                var last = boxes.Count - 1;
                var end = new Offset(boxes[last].end, boxes[last].bottom) + paintOffset;
                return new List<TextSelectionPoint> {
                    new TextSelectionPoint(start, boxes[0].direction),
                    new TextSelectionPoint(end, boxes[last].direction),
                };
            }
        }

        public TextPosition getPositionForPoint(Offset globalPosition) {
            _layoutText(constraints.maxWidth);
            globalPosition -= _paintOffset;
            return _textPainter.getPositionForOffset(globalToLocal(globalPosition));
        }

        public Rect getLocalRectForCaret(TextPosition caretPosition) {
            _layoutText(constraints.maxWidth);
            var caretOffset = _textPainter.getOffsetForCaret(caretPosition, _caretPrototype);
            Rect rect = Rect.fromLTWH(0.0f, 0.0f, cursorWidth, preferredLineHeight)
                .shift(caretOffset + _paintOffset);
            if (_cursorOffset != null) {
                rect = rect.shift(_cursorOffset);
            }

            return rect.shift(_getPixelPerfectCursorOffset(rect));
        }

        protected override float computeMinIntrinsicWidth(float height) {
            _layoutText(float.PositiveInfinity);
            return _textPainter.minIntrinsicWidth;
        }

        protected override float computeMaxIntrinsicWidth(float height) {
            _layoutText(float.PositiveInfinity);
            return _textPainter.maxIntrinsicWidth + cursorWidth;
        }

        public float preferredLineHeight {
            get { return _textPainter.preferredLineHeight; }
        }

        float _preferredHeight(float width) {
            bool lockedMax = maxLines != null && minLines == null;
            bool lockedBoth = maxLines != null && minLines == maxLines;
            bool singleLine = maxLines == 1;
            if (singleLine || lockedMax || lockedBoth) {
                return preferredLineHeight * maxLines.Value;
            }

            bool minLimited = minLines != null && minLines > 1;
            bool maxLimited = maxLines != null;
            if (minLimited || maxLimited) {
                _layoutText(width);
                if (minLimited && _textPainter.height < preferredLineHeight * minLines.Value) {
                    return preferredLineHeight * minLines.Value;
                }

                if (maxLimited && _textPainter.height > preferredLineHeight * maxLines.Value) {
                    return preferredLineHeight * maxLines.Value;
                }
            }

            if (!width.isFinite()) {
                var text = _textPainter.text.toPlainText();
                int lines = 1;
                for (int index = 0; index < text.Length; ++index) {
                    if (text[index] == 0x0A) {
                        lines += 1;
                    }
                }

                return preferredLineHeight * lines;
            }

            _layoutText(width);
            return Mathf.Max(preferredLineHeight, _textPainter.height);
        }

        protected override float computeMinIntrinsicHeight(float width) {
            return _preferredHeight(width);
        }

        protected internal override float computeMaxIntrinsicHeight(float width) {
            return _preferredHeight(width);
        }

        protected override float? computeDistanceToActualBaseline(TextBaseline baseline) {
            _layoutText(constraints.maxWidth);
            return _textPainter.computeDistanceToActualBaseline(baseline);
        }

        protected override bool hitTestSelf(Offset position) {
            return true;
        }

        TapGestureRecognizer _tap;
        LongPressGestureRecognizer _longPress;

        public override void handleEvent(PointerEvent evt, HitTestEntry entry) {
            if (ignorePointer) {
                return;
            }

            D.assert(debugHandleEvent(evt, entry));
            if (evt is PointerDownEvent && onSelectionChanged != null) {
                _tap.addPointer((PointerDownEvent) evt);
                _doubleTap.addPointer((PointerDownEvent) evt);
                _longPress.addPointer((PointerDownEvent) evt);
            }
        }

        Offset _lastTapDownPosition;

        public void handleTapDown(TapDownDetails details) {
            _lastTapDownPosition = details.globalPosition;
            if (!Application.isMobilePlatform) {
                selectPosition(SelectionChangedCause.tap);
            }
        }

        void _handleTapDown(TapDownDetails details) {
            D.assert(!ignorePointer);
            handleTapDown(details);
        }

        public void handleTap() {
            selectPosition(cause: SelectionChangedCause.tap);
        }

        void _handleTap() {
            D.assert(!ignorePointer);
            handleTap();
        }

        void _handleDoubleTap(DoubleTapDetails details) {
            D.assert(!ignorePointer);
            handleDoubleTap(details);
        }

        public void handleDoubleTap(DoubleTapDetails details) {
            // need set _lastTapDownPosition, otherwise it would be last single tap position
            _lastTapDownPosition = details.firstGlobalPosition - _paintOffset;
            selectWord(cause: SelectionChangedCause.doubleTap);
        }

        void _handleLongPress() {
            D.assert(!ignorePointer);
            handleLongPress();
        }

        public void handleLongPress() {
            selectWord(cause: SelectionChangedCause.longPress);
        }

        public void selectPositionAt(Offset from = null, Offset to = null, SelectionChangedCause? cause = null) {
            D.assert(cause != null);
            D.assert(from != null);

            _layoutText(constraints.maxWidth);
            if (onSelectionChanged != null) {
                TextPosition fromPosition =
                    _textPainter.getPositionForOffset(globalToLocal(from - _paintOffset));
                TextPosition toPosition = to == null
                    ? null
                    : _textPainter.getPositionForOffset(globalToLocal(to - _paintOffset));

                int baseOffset = fromPosition.offset;
                int extentOffset = fromPosition.offset;
                if (toPosition != null) {
                    baseOffset = Mathf.Min(fromPosition.offset, toPosition.offset);
                    extentOffset = Mathf.Max(fromPosition.offset, toPosition.offset);
                }

                TextSelection newSelection = new TextSelection(
                    baseOffset: baseOffset,
                    extentOffset: extentOffset,
                    affinity: fromPosition.affinity);

                onSelectionChanged(newSelection, this, cause.Value);
            }
        }

        public void selectPosition(SelectionChangedCause? cause = null) {
            selectPositionAt(from: _lastTapDownPosition, cause: cause);
        }

        public void selectWord(SelectionChangedCause? cause = null) {
            selectWordsInRange(from: _lastTapDownPosition, cause: cause);
        }

        public void selectWordsInRange(Offset from = null, Offset to = null, SelectionChangedCause? cause = null) {
            D.assert(cause != null);
            D.assert(from != null);
            _layoutText(constraints.maxWidth);
            if (onSelectionChanged != null) {
                TextPosition firstPosition =
                    _textPainter.getPositionForOffset(globalToLocal(from - _paintOffset));
                TextSelection firstWord = _selectWordAtOffset(firstPosition);
                TextSelection lastWord = to == null
                    ? firstWord
                    : _selectWordAtOffset(
                        _textPainter.getPositionForOffset(globalToLocal(to - _paintOffset)));

                onSelectionChanged(
                    new TextSelection(
                        baseOffset: firstWord.baseOffset,
                        extentOffset: lastWord.extentOffset,
                        affinity: firstWord.affinity),
                    this,
                    cause.Value);
            }
        }

        public void selectWordEdge(SelectionChangedCause cause) {
            _layoutText(constraints.maxWidth);
            D.assert(_lastTapDownPosition != null);
            if (onSelectionChanged != null) {
                TextPosition position =
                    _textPainter.getPositionForOffset(
                        globalToLocal(_lastTapDownPosition - _paintOffset));
                TextRange word = _textPainter.getWordBoundary(position);
                if (position.offset - word.start <= 1) {
                    onSelectionChanged(
                        TextSelection.collapsed(offset: word.start, affinity: TextAffinity.downstream),
                        this,
                        cause
                    );
                }
                else {
                    onSelectionChanged(
                        TextSelection.collapsed(offset: word.end, affinity: TextAffinity.upstream),
                        this,
                        cause
                    );
                }
            }
        }

        TextSelection _selectWordAtOffset(TextPosition position) {
            D.assert(_textLayoutLastWidth == constraints.maxWidth);
            var word = _textPainter.getWordBoundary(position);
            if (position.offset >= word.end) {
                return TextSelection.fromPosition(position);
            }

            return new TextSelection(baseOffset: word.start, extentOffset: word.end);
        }

        Rect _caretPrototype;

        void _layoutText(float constraintWidth) {
            if (_textLayoutLastWidth == constraintWidth) {
                return;
            }

            var caretMargin = EditableUtils._kCaretGap + cursorWidth;
            var avialableWidth = Mathf.Max(0.0f, constraintWidth - caretMargin);
            var maxWidth = _isMultiline ? avialableWidth : float.PositiveInfinity;
            _textPainter.layout(minWidth: avialableWidth, maxWidth: maxWidth);
            _textLayoutLastWidth = constraintWidth;
        }

        Rect _getCaretPrototype {
            get {
                switch (Application.platform) {
                    case RuntimePlatform.IPhonePlayer:
                        return Rect.fromLTWH(0.0f, 0.0f, cursorWidth,
                            preferredLineHeight + 2.0f);
                    default:
                        return Rect.fromLTWH(0.0f, EditableUtils._kCaretHeightOffset, cursorWidth,
                            preferredLineHeight - 2.0f * EditableUtils._kCaretHeightOffset);
                }
            }
        }


        protected override void performLayout() {
            _layoutText(constraints.maxWidth);
            _caretPrototype = _getCaretPrototype;
            _selectionRects = null;

            var textPainterSize = _textPainter.size;
            size = new Size(constraints.maxWidth,
                constraints.constrainHeight(_preferredHeight(constraints.maxWidth)));
            var contentSize = new Size(textPainterSize.width + EditableUtils._kCaretGap + cursorWidth,
                textPainterSize.height);
            _maxScrollExtent = _getMaxScrollExtent(contentSize);
            offset.applyViewportDimension(_viewportExtent);
            offset.applyContentDimensions(0.0f, _maxScrollExtent);
        }

        Offset _getPixelPerfectCursorOffset(Rect caretRect) {
            Offset caretPosition = localToGlobal(caretRect.topLeft);
            float pixelMultiple = 1.0f / _devicePixelRatio;
            int quotientX = (caretPosition.dx / pixelMultiple).round();
            int quotientY = (caretPosition.dy / pixelMultiple).round();
            float pixelPerfectOffsetX = quotientX * pixelMultiple - caretPosition.dx;
            float pixelPerfectOffsetY = quotientY * pixelMultiple - caretPosition.dy;
            return new Offset(pixelPerfectOffsetX, pixelPerfectOffsetY);
        }

        void _paintCaret(Canvas canvas, Offset effectiveOffset, TextPosition textPosition) {
            D.assert(_textLayoutLastWidth == constraints.maxWidth);
            var paint = new Paint() {color = _floatingCursorOn ? backgroundCursorColor : _cursorColor};
            var caretOffset = _textPainter.getOffsetForCaret(textPosition, _caretPrototype) + effectiveOffset;
            Rect caretRect = _caretPrototype.shift(caretOffset);
            if (_cursorOffset != null) {
                caretRect = caretRect.shift(_cursorOffset);
            }
            
            float? caretHeight = _textPainter.getFullHeightForCaret(textPosition, _caretPrototype);
            if (caretHeight != null) {
                switch (Application.platform) {
                    case RuntimePlatform.IPhonePlayer:
                        float heightDiff = caretHeight.Value - caretRect.height;
                        caretRect = Rect.fromLTWH(
                            caretRect.left,
                            caretRect.top + heightDiff / 2f,
                            caretRect.width,
                            caretRect.height
                        );
                        break;
                    default:
                        caretRect = Rect.fromLTWH(
                            caretRect.left,
                            caretRect.top - EditableUtils._kCaretHeightOffset,
                            caretRect.width,
                            caretHeight.Value
                        );
                        break;
                }
            }

            caretRect = caretRect.shift(_getPixelPerfectCursorOffset(caretRect));

            if (cursorRadius == null) {
                canvas.drawRect(caretRect, paint);
            }
            else {
                RRect caretRRect = RRect.fromRectAndRadius(caretRect, cursorRadius);
                canvas.drawRRect(caretRRect, paint);
            }

            if (!caretRect.Equals(_lastCaretRect)) {
                _lastCaretRect = caretRect;
                if (onCaretChanged != null) {
                    onCaretChanged(caretRect);
                }
            }
        }

        public void setFloatingCursor(FloatingCursorDragState? state, Offset boundedOffset,
            TextPosition lastTextPosition,
            float? resetLerpValue = null) {
            D.assert(boundedOffset != null);
            D.assert(lastTextPosition != null);
            if (state == FloatingCursorDragState.Start) {
                _relativeOrigin = new Offset(0, 0);
                _previousOffset = null;
                _resetOriginOnBottom = false;
                _resetOriginOnTop = false;
                _resetOriginOnRight = false;
                _resetOriginOnBottom = false;
            }

            _floatingCursorOn = state != FloatingCursorDragState.End;
            _resetFloatingCursorAnimationValue = resetLerpValue;
            if (_floatingCursorOn) {
                _floatingCursorOffset = boundedOffset;
                _floatingCursorTextPosition = lastTextPosition;
            }

            markNeedsPaint();
        }

        // describeSemanticsConfiguration todo

        void _paintFloatingCaret(Canvas canvas, Offset effectiveOffset) {
            D.assert(_textLayoutLastWidth == constraints.maxWidth);
            D.assert(_floatingCursorOn);

            Paint paint = new Paint() {color = _cursorColor.withOpacity(0.75f)};

            float sizeAdjustmentX = EditableUtils._kFloatingCaretSizeIncrease.dx;
            float sizeAdjustmentY = EditableUtils._kFloatingCaretSizeIncrease.dy;

            if (_resetFloatingCursorAnimationValue != null) {
                sizeAdjustmentX =
                    MathUtils.lerpFloat(sizeAdjustmentX, 0f, _resetFloatingCursorAnimationValue.Value);
                sizeAdjustmentY =
                    MathUtils.lerpFloat(sizeAdjustmentY, 0f, _resetFloatingCursorAnimationValue.Value);
            }

            Rect floatingCaretPrototype = Rect.fromLTRB(
                _caretPrototype.left - sizeAdjustmentX,
                _caretPrototype.top - sizeAdjustmentY,
                _caretPrototype.right + sizeAdjustmentX,
                _caretPrototype.bottom + sizeAdjustmentY
            );

            Rect caretRect = floatingCaretPrototype.shift(effectiveOffset);
            Radius floatingCursorRadius = Radius.circular(EditableUtils._kFloatingCaretRadius);
            RRect caretRRect = RRect.fromRectAndRadius(caretRect, floatingCursorRadius);
            canvas.drawRRect(caretRRect, paint);
        }

        Offset _relativeOrigin = new Offset(0f, 0f);
        Offset _previousOffset;
        bool _resetOriginOnLeft = false;
        bool _resetOriginOnRight = false;
        bool _resetOriginOnTop = false;
        bool _resetOriginOnBottom = false;
        float? _resetFloatingCursorAnimationValue;

        public Offset calculateBoundedFloatingCursorOffset(Offset rawCursorOffset) {
            Offset deltaPosition = new Offset(0f, 0f);
            float topBound = -floatingCursorAddedMargin.top;
            float bottomBound = _textPainter.height - preferredLineHeight +
                                floatingCursorAddedMargin.bottom;
            float leftBound = -floatingCursorAddedMargin.left;
            float rightBound = _textPainter.width + floatingCursorAddedMargin.right;

            if (_previousOffset != null) {
                deltaPosition = rawCursorOffset - _previousOffset;
            }

            if (_resetOriginOnLeft && deltaPosition.dx > 0) {
                _relativeOrigin = new Offset(rawCursorOffset.dx - leftBound, _relativeOrigin.dy);
                _resetOriginOnLeft = false;
            }
            else if (_resetOriginOnRight && deltaPosition.dx < 0) {
                _relativeOrigin = new Offset(rawCursorOffset.dx - rightBound, _relativeOrigin.dy);
                _resetOriginOnRight = false;
            }

            if (_resetOriginOnTop && deltaPosition.dy > 0) {
                _relativeOrigin = new Offset(_relativeOrigin.dx, rawCursorOffset.dy - topBound);
                _resetOriginOnTop = false;
            }
            else if (_resetOriginOnBottom && deltaPosition.dy < 0) {
                _relativeOrigin = new Offset(_relativeOrigin.dx, rawCursorOffset.dy - bottomBound);
                _resetOriginOnBottom = false;
            }

            float currentX = rawCursorOffset.dx - _relativeOrigin.dx;
            float currentY = rawCursorOffset.dy - _relativeOrigin.dy;
            float adjustedX = Mathf.Min(Mathf.Max(currentX, leftBound), rightBound);
            float adjustedY = Mathf.Min(Mathf.Max(currentY, topBound), bottomBound);
            Offset adjustedOffset = new Offset(adjustedX, adjustedY);

            if (currentX < leftBound && deltaPosition.dx < 0) {
                _resetOriginOnLeft = true;
            }
            else if (currentX > rightBound && deltaPosition.dx > 0) {
                _resetOriginOnRight = true;
            }

            if (currentY < topBound && deltaPosition.dy < 0) {
                _resetOriginOnTop = true;
            }
            else if (currentY > bottomBound && deltaPosition.dy > 0) {
                _resetOriginOnBottom = true;
            }

            _previousOffset = rawCursorOffset;

            return adjustedOffset;
        }

        void _paintSelection(Canvas canvas, Offset effectiveOffset) {
            D.assert(_textLayoutLastWidth == constraints.maxWidth);
            D.assert(_selectionRects != null);
            var paint = new Paint() {color = _selectionColor};

            foreach (var box in _selectionRects) {
                canvas.drawRect(box.toRect().shift(effectiveOffset), paint);
            }
        }

        void _paintContents(PaintingContext context, Offset offset) {
            D.assert(_textLayoutLastWidth == constraints.maxWidth);
            var effectiveOffset = offset + _paintOffset;

            bool showSelection = false;
            bool showCaret = false;

            if (_selection != null && !_floatingCursorOn) {
                if (_selection.isCollapsed && _showCursor.value && cursorColor != null) {
                    showCaret = true;
                }
                else if (!_selection.isCollapsed && _selectionColor != null) {
                    showSelection = true;
                }
                _updateSelectionExtentsVisibility(effectiveOffset);
            }

            if (showSelection) {
                _selectionRects = _selectionRects ?? _textPainter.getBoxesForSelection(_selection);
                _paintSelection(context.canvas, effectiveOffset);
            }

            if (paintCursorAboveText) {
                _textPainter.paint(context.canvas, effectiveOffset);
            }

            if (showCaret) {
                _paintCaret(context.canvas, effectiveOffset, _selection.extendPos);
            }

            if (!paintCursorAboveText) {
                _textPainter.paint(context.canvas, effectiveOffset);
            }

            if (_floatingCursorOn) {
                if (_resetFloatingCursorAnimationValue == null) {
                    _paintCaret(context.canvas, effectiveOffset, _floatingCursorTextPosition);
                }

                _paintFloatingCaret(context.canvas, _floatingCursorOffset);
            }
        }

        void markNeedsSemanticsUpdate() {
            // todo
        }

        public override Rect describeApproximatePaintClip(RenderObject child) {
            return _hasVisualOverflow ? Offset.zero & size : null;
        }

        public override void paint(PaintingContext context, Offset offset) {
            _layoutText(constraints.maxWidth);
            if (_hasVisualOverflow) {
                context.pushClipRect(needsCompositing, offset, Offset.zero & size, _paintContents);
            }
            else {
                _paintContents(context, offset);
            }
        }

        public override void debugFillProperties(DiagnosticPropertiesBuilder properties) {
            base.debugFillProperties(properties);
            properties.add(new DiagnosticsProperty<Color>("cursorColor", cursorColor));
            properties.add(new DiagnosticsProperty<ValueNotifier<bool>>("showCursor", showCursor));
            properties.add(new DiagnosticsProperty<int?>("maxLines", maxLines));
            properties.add(new DiagnosticsProperty<int?>("minLines", minLines));
            properties.add(new DiagnosticsProperty<bool>("expands", expands));
            properties.add(new DiagnosticsProperty<Color>("selectionColor", selectionColor));
            properties.add(new DiagnosticsProperty<float>("textScaleFactor", textScaleFactor));
            properties.add(new DiagnosticsProperty<TextSelection>("selection", selection));
            properties.add(new DiagnosticsProperty<ViewportOffset>("offset", offset));
        }

        public override List<DiagnosticsNode> debugDescribeChildren() {
            return new List<DiagnosticsNode> {
                text.toDiagnosticsNode(
                    name: "text",
                    style: DiagnosticsTreeStyle.transition
                ),
            };
        }
    }
}