using System;
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
    public enum TextOverflow {
        /// Clip the overflowing text to fix its container.
        clip,

        /// Fade the overflowing text to transparent.
        fade,

        /// Use an ellipsis to indicate that the text has overflowed.
        ellipsis,
        
        /// Render overflowing text outside of its container.
        visible,
    }


    public class RenderParagraph : RenderBox {
        static readonly string _kEllipsis = "\u2026";

        bool _softWrap;

        TextOverflow _overflow;
        readonly TextPainter _textPainter;
        bool _needsClipping = false;

        List<TextBox> _selectionRects;

        public RenderParagraph(TextSpan text,
            TextAlign textAlign = TextAlign.left,
            TextDirection textDirection = TextDirection.ltr,
            bool softWrap = true,
            TextOverflow overflow = TextOverflow.clip,
            float textScaleFactor = 1.0f,
            int? maxLines = null,
            StrutStyle strutStyle = null,
            Action onSelectionChanged = null,
            Color selectionColor = null
        ) {
            D.assert(maxLines == null || maxLines > 0);
            _softWrap = softWrap;
            _overflow = overflow;
            _textPainter = new TextPainter(
                text,
                textAlign,
                textDirection,
                textScaleFactor,
                maxLines,
                overflow == TextOverflow.ellipsis ? _kEllipsis : "",
                strutStyle: strutStyle
            );

            _selection = null;
            this.onSelectionChanged = onSelectionChanged;
            this.selectionColor = selectionColor;

            _resetHoverHandler();
        }

        public Action onSelectionChanged;
        public Color selectionColor;

        public TextSelection selection {
            get { return _selection; }
            set {
                if (_selection == value) {
                    return;
                }

                _selection = value;
                _selectionRects = null;
                markNeedsPaint();
            }
        }

        public InlineSpan text {
            get { return _textPainter.text; }

            set {
                Debug.Assert(value != null);
                switch (_textPainter.text.compareTo(value)) {
                    case RenderComparison.identical:
                    case RenderComparison.metadata:
                        return;
                    case RenderComparison.function:
                        _textPainter.text = value;
                        markNeedsPaint();
                        break;
                    case RenderComparison.paint:
                        _textPainter.text = value;
                        markNeedsPaint();
                        break;
                    case RenderComparison.layout:
                        _textPainter.text = value;
                        markNeedsLayout();
                        break;
                }

                _resetHoverHandler();
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

                _textPainter.textDirection = textDirection;
                markNeedsLayout();
            }
        }

        protected Offset getOffsetForCaret(TextPosition position, Rect caretPrototype) {
            D.assert(_textPainter != null);
            return _textPainter.getOffsetForCaret(position, caretPrototype);
        }

        public bool softWrap {
            get { return _softWrap; }
            set {
                if (_softWrap == value) {
                    return;
                }

                _softWrap = value;
                markNeedsLayout();
            }
        }

        public TextOverflow overflow {
            get { return _overflow; }
            set {
                if (_overflow == value) {
                    return;
                }

                _overflow = value;
                _textPainter.ellipsis = value == TextOverflow.ellipsis ? _kEllipsis : null;
                // _textPainter.e
                markNeedsLayout();
            }
        }

        public float textScaleFactor {
            get { return _textPainter.textScaleFactor; }
            set {
                if (Mathf.Abs(_textPainter.textScaleFactor - value) < 0.00000001) {
                    return;
                }

                _textPainter.textScaleFactor = value;
                markNeedsLayout();
            }
        }

        public int? maxLines {
            get { return _textPainter.maxLines; }
            set {
                D.assert(maxLines == null || maxLines > 0);
                if (_textPainter.maxLines == value) {
                    return;
                }

                _textPainter.maxLines = value;
                markNeedsLayout();
            }
        }

        public Size textSize {
            get { return _textPainter.size; }
        }

        protected override float computeMinIntrinsicWidth(float height) {
            _layoutText();
            return _textPainter.minIntrinsicWidth;
        }

        protected override float computeMaxIntrinsicWidth(float height) {
            _layoutText();
            return _textPainter.maxIntrinsicWidth;
        }

        float _computeIntrinsicHeight(float width) {
            _layoutText(minWidth: width, maxWidth: width);
            return _textPainter.height;
        }

        protected override float computeMinIntrinsicHeight(float width) {
            return _computeIntrinsicHeight(width);
        }

        protected internal override float computeMaxIntrinsicHeight(float width) {
            return _computeIntrinsicHeight(width);
        }

        protected override float? computeDistanceToActualBaseline(TextBaseline baseline) {
            _layoutTextWithConstraints(constraints);
            return _textPainter.computeDistanceToActualBaseline(baseline);
        }


        protected override bool hitTestSelf(Offset position) {
            return true;
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
                    selection = null;
                    D.assert(_listenerAttached);
                    RawKeyboard.instance.removeListener(_handleKeyEvent);
                    _listenerAttached = false;
                }
            }
        }

        InlineSpan _previousHoverSpan;

#pragma warning disable 0414
        bool _pointerHoverInside;
#pragma warning restore 0414
        bool _hasHoverRecognizer;
        MouseTrackerAnnotation _hoverAnnotation;

        void _resetHoverHandler() {
            _hasHoverRecognizer = (_textPainter.text as TextSpan)?.hasHoverRecognizer ?? false;
            _previousHoverSpan = null;
            _pointerHoverInside = false;

            if (_hoverAnnotation != null && attached) {
                RendererBinding.instance.mouseTracker.detachAnnotation(_hoverAnnotation);
            }

            if (_hasHoverRecognizer) {
                _hoverAnnotation = new MouseTrackerAnnotation(
                    onEnter: _onPointerEnter,
                    onHover: _onPointerHover,
                    onExit: _onPointerExit);

                if (attached) {
                    RendererBinding.instance.mouseTracker.attachAnnotation(_hoverAnnotation);
                }
            }
            else {
                _hoverAnnotation = null;
            }
        }

        void _handleKeyEvent(RawKeyEvent keyEvent) {
            //only allow KCommand.copy
            if (keyEvent is RawKeyUpEvent) {
                return;
            }

            if (selection.isCollapsed) {
                return;
            }

            KeyCode pressedKeyCode = keyEvent.data.unityEvent.keyCode;
            int modifiers = (int) keyEvent.data.unityEvent.modifiers;
            bool ctrl = (modifiers & (int) EventModifiers.Control) > 0;
            bool cmd = (modifiers & (int) EventModifiers.Command) > 0;
            bool cKey = pressedKeyCode == KeyCode.C;
            bool isMac = SystemInfo.operatingSystemFamily == OperatingSystemFamily.MacOSX;

            KeyCommand? kcmd = keyEvent is RawKeyCommandEvent
                ? ((RawKeyCommandEvent) keyEvent).command
                : ((ctrl || (isMac && cmd)) && cKey)
                    ? KeyCommand.Copy
                    : (KeyCommand?) null;

            if (kcmd == KeyCommand.Copy) {
                Clipboard.setData(
                    new ClipboardData(text: selection.textInside(text.toPlainText()))
                );
            }
        }

        public override void attach(object owner) {
            base.attach(owner);
            if (_hoverAnnotation != null) {
                RendererBinding.instance.mouseTracker.attachAnnotation(_hoverAnnotation);
            }
        }

        public override void detach() {
            if (_listenerAttached) {
                RawKeyboard.instance.removeListener(_handleKeyEvent);
            }

            base.detach();
            if (_hoverAnnotation != null) {
                RendererBinding.instance.mouseTracker.detachAnnotation(_hoverAnnotation);
            }
        }

        TextSelection _selection;

        public void selectPositionAt(Offset from = null, Offset to = null, SelectionChangedCause? cause = null) {
            D.assert(cause != null);
            D.assert(from != null);
            if (true) {
                TextPosition fromPosition =
                    _textPainter.getPositionForOffset(globalToLocal(from));
                TextPosition toPosition = to == null
                    ? null
                    : _textPainter.getPositionForOffset(globalToLocal(to));

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

                if (newSelection != _selection) {
                    _handleSelectionChanged(newSelection, cause.Value);
                }
            }
        }


        void _handleSelectionChanged(TextSelection selection,
            SelectionChangedCause cause) {
            this.selection = selection;
            onSelectionChanged?.Invoke();
        }

        void _onPointerEnter(PointerEvent evt) {
            _pointerHoverInside = true;
        }

        void _onPointerExit(PointerEvent evt) {
            _pointerHoverInside = false;
            (_previousHoverSpan as TextSpan)?.hoverRecognizer?.OnPointerLeave?.Invoke();
            _previousHoverSpan = null;
        }

        void _onPointerHover(PointerEvent evt) {
            _layoutTextWithConstraints(constraints);
            Offset offset = globalToLocal(evt.position);
            TextPosition position = _textPainter.getPositionForOffset(offset);
            InlineSpan span = _textPainter.text.getSpanForPosition(position);

            if (_previousHoverSpan != span) {
                (_previousHoverSpan as TextSpan)?.hoverRecognizer?.OnPointerLeave?.Invoke();
                (span as TextSpan)?.hoverRecognizer?.OnPointerEnter?.Invoke((PointerHoverEvent) evt);
                _previousHoverSpan = span;
            }
        }

        public override void handleEvent(PointerEvent evt, HitTestEntry entry) {
            D.assert(debugHandleEvent(evt, entry));
            if (!(evt is PointerDownEvent)) {
                return;
            }
            
            _layoutTextWithConstraints(constraints);
            Offset offset = ((BoxHitTestEntry) entry).localPosition;
            TextPosition position = _textPainter.getPositionForOffset(offset);
            InlineSpan span = _textPainter.text.getSpanForPosition(position);
            (span as TextSpan)?.recognizer?.addPointer((PointerDownEvent) evt);
        }

        protected override void performLayout() {
            _layoutTextWithConstraints(constraints);
            var textSize = _textPainter.size;
            var textDidExceedMaxLines = _textPainter.didExceedMaxLines;
            size = constraints.constrain(textSize);

            var didOverflowHeight = size.height < textSize.height || textDidExceedMaxLines;
            var didOverflowWidth = size.width < textSize.width;
            var hasVisualOverflow = didOverflowWidth || didOverflowHeight;
            if (hasVisualOverflow) {
                switch (_overflow) {
                case TextOverflow.visible:
                    _needsClipping = false;
                    break;
                case TextOverflow.clip:
                case TextOverflow.ellipsis:
                case TextOverflow.fade:
                    _needsClipping = true;
                    break;
                }
            }
            else {
                _needsClipping = false;
            }

            _selectionRects = null;
        }


        void paintParagraph(PaintingContext context, Offset offset) {
            _layoutTextWithConstraints(constraints);
            var canvas = context.canvas;

            if (_needsClipping) {
                var bounds = offset & size;
                canvas.save();
                canvas.clipRect(bounds);
            }

            if (_selection != null && selectionColor != null && _selection.isValid) {
                if (!_selection.isCollapsed) {
                    _selectionRects =
                        _selectionRects ?? _textPainter.getBoxesForSelection(_selection);
                    _paintSelection(canvas, offset);
                }
            }

            _textPainter.paint(canvas, offset);
            if (_needsClipping) {
                canvas.restore();
            }
        }

        public override void paint(PaintingContext context, Offset offset) {
            if (_hoverAnnotation != null) {
                AnnotatedRegionLayer<MouseTrackerAnnotation> layer = new AnnotatedRegionLayer<MouseTrackerAnnotation>(
                    _hoverAnnotation, size: size, offset: offset);

                context.pushLayer(layer, paintParagraph, offset);
            }
            else {
                paintParagraph(context, offset);
            }
        }


        void _paintSelection(Canvas canvas, Offset effectiveOffset) {
            D.assert(_selectionRects != null);
            D.assert(selectionColor != null);
            var paint = new Paint {color = selectionColor};

            Path barPath = new Path();
            foreach (var box in _selectionRects) {
                barPath.addRect(box.toRect().shift(effectiveOffset));
            }

            canvas.drawPath(barPath, paint);
        }

        public StrutStyle strutStyle {
            get { return _textPainter.strutStyle; }
            set {
                if (_textPainter.strutStyle == value) {
                    return;
                }

                _textPainter.strutStyle = value;
                markNeedsLayout();
            }
        }

        void _layoutText(float minWidth = 0.0f, float maxWidth = float.PositiveInfinity) {
            var widthMatters = softWrap || overflow == TextOverflow.ellipsis;
            _textPainter.layout(minWidth, widthMatters ? maxWidth : float.PositiveInfinity);
        }

        void _layoutTextWithConstraints(BoxConstraints constraints) {
            _layoutText(minWidth: constraints.minWidth, maxWidth: constraints.maxWidth);
        }

        public override List<DiagnosticsNode> debugDescribeChildren() {
            return new List<DiagnosticsNode> {
                text.toDiagnosticsNode(name: "text", style: DiagnosticsTreeStyle.transition)
            };
        }

        public override void debugFillProperties(DiagnosticPropertiesBuilder properties) {
            base.debugFillProperties(properties);
            properties.add(new EnumProperty<TextAlign>("textAlign", textAlign));
            properties.add(new EnumProperty<TextDirection?>("textDirection", textDirection));
            properties.add(new FlagProperty("softWrap", value: softWrap, ifTrue: "wrapping at box width",
                ifFalse: "no wrapping except at line break characters", showName: true));
            properties.add(new EnumProperty<TextOverflow>("overflow", overflow));
            properties.add(new FloatProperty("textScaleFactor", textScaleFactor, defaultValue: 1.0f));
            properties.add(new IntProperty("maxLines", maxLines, ifNull: "unlimited"));
        }
    }
}