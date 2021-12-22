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
        clip,
        fade,
        ellipsis,
        visible
    }

    public class TextParentData : ContainerBoxParentData<RenderBox> {
        public float scale;
        public override string ToString() {
            List<string> values = new List<string>();
            if (offset != null) values.Add($"offset={offset}");
            values.Add($"scale={scale}");
            values.Add(base.ToString());
            return string.Join("; ", values);
        }
    }
    public class RenderParagraph : RelayoutWhenSystemFontsChangeMixinRenderBoxContainerDefaultsMixinContainerRenderObjectMixinRenderBox<RenderBox, TextParentData> {
        static readonly string _kEllipsis = "\u2026";

        bool _softWrap;

        TextOverflow _overflow;
        readonly TextPainter _textPainter;
        bool _needsClipping = false;
        ui.Shader _overflowShader;

        List<TextBox> _selectionRects;

        public RenderParagraph(
            InlineSpan text = null,
            TextAlign textAlign = TextAlign.start,
            TextDirection textDirection = TextDirection.ltr,
            bool softWrap = true,
            TextOverflow overflow = TextOverflow.clip,
            float textScaleFactor = 1.0f,
            int? maxLines = null,
            Locale locale = null,
            StrutStyle strutStyle = null,
            TextWidthBasis textWidthBasis = TextWidthBasis.parent,
            ui.TextHeightBehavior textHeightBehavior = null,
            List<RenderBox> children = null
        ) {
            D.assert(maxLines == null || maxLines > 0);
            D.assert(text != null);
            D.assert(text.debugAssertIsValid());
            D.assert(maxLines == null || maxLines > 0);
            _softWrap = softWrap;
            _overflow = overflow;
            _textPainter = new TextPainter(
                text: text,
                textAlign: textAlign,
                textDirection: textDirection,
                textScaleFactor: textScaleFactor,
                maxLines: maxLines,
                ellipsis: overflow == TextOverflow.ellipsis ? _kEllipsis : null,
                locale: locale,
                strutStyle: strutStyle,
                textWidthBasis: textWidthBasis,
                textHeightBehavior: textHeightBehavior
            );
            addAll(children);
            _extractPlaceholderSpans(text);
        }

        public override void setupParentData(RenderObject child) {
            if (!(child.parentData is TextParentData))
                child.parentData = new TextParentData();
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
                    case RenderComparison.paint:
                        _textPainter.text = value;
                        _extractPlaceholderSpans(value);
                        markNeedsPaint();
                        break;
                    case RenderComparison.layout:
                        _textPainter.text = value;
                        _overflowShader = null;
                        _extractPlaceholderSpans(value);
                        markNeedsLayout();
                        break;
                }

                
            }
        }
        List<PlaceholderSpan> _placeholderSpans;
        void _extractPlaceholderSpans(InlineSpan span) {
            _placeholderSpans = new List<PlaceholderSpan>();
            span.visitChildren((InlineSpan inlinespan)=> {
                if (inlinespan is PlaceholderSpan) {
                    PlaceholderSpan placeholderSpan =(PlaceholderSpan)inlinespan;
                    _placeholderSpans.Add(placeholderSpan);
                }
                return true;
            });
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
                markNeedsLayout();
            }
        }

        protected Offset getOffsetForCaret(TextPosition position, Rect caretPrototype) {
            D.assert(_textPainter != null);
            return _textPainter.getOffsetForCaret(position, caretPrototype);
        }
        
        List<ui.TextBox> getBoxesForSelection(TextSelection selection) {
            D.assert(!debugNeedsLayout);
            _layoutTextWithConstraints(constraints);
            return _textPainter.getBoxesForSelection(selection);
        }
        
        TextPosition getPositionForOffset(Offset offset) {
            D.assert(!debugNeedsLayout);
            _layoutTextWithConstraints(constraints);
            return _textPainter.getPositionForOffset(offset);
        }
        
        TextRange getWordBoundary(TextPosition position) {
            D.assert(!debugNeedsLayout);
            _layoutTextWithConstraints(constraints);
            return _textPainter.getWordBoundary(position);
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
                _overflowShader = null;
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
                _overflowShader = null;
                markNeedsLayout();
            }
        }

        public Locale locale {
            get {
                return _textPainter.locale;
            }
            set {
                if (_textPainter.locale == value)
                    return;
                _textPainter.locale = value;
                _overflowShader = null;
                markNeedsLayout();
            }
        }

        public StrutStyle strutStyle {
            get { return  _textPainter.strutStyle;}
            set {
                if (_textPainter.strutStyle == value)
                    return;
                _textPainter.strutStyle = value;
                _overflowShader = null;
                markNeedsLayout();
            }
        }

        public TextWidthBasis textWidthBasis {
            get { return _textPainter.textWidthBasis; }
            set {
                if (_textPainter.textWidthBasis == value)
                    return;
                _textPainter.textWidthBasis = value;
                _overflowShader = null;
                markNeedsLayout();
            }
        }


        /// {@macro flutter.dart:ui.textHeightBehavior}
        public ui.TextHeightBehavior textHeightBehavior {
            get {
                return   _textPainter.textHeightBehavior;
            }
            set {
                if (_textPainter.textHeightBehavior == value)
                    return;
                _textPainter.textHeightBehavior = value;
                _overflowShader = null;
                markNeedsLayout();
            }
        }

        public Size textSize {
            get { return _textPainter.size; }
        }

        protected internal override float computeMinIntrinsicWidth(float height) {
            if (!_canComputeIntrinsics()) {
                return 0.0f;
            }
            _computeChildrenWidthWithMinIntrinsics(height);
            _layoutText(); // layout with infinite width.
            return _textPainter.minIntrinsicWidth;
        }

        protected internal override float computeMaxIntrinsicWidth(float height) {
            if (!_canComputeIntrinsics()) {
                return 0.0f;
            }
            _computeChildrenWidthWithMaxIntrinsics(height);
            _layoutText(); // layout with infinite width.
            return _textPainter.maxIntrinsicWidth;
        }

        float _computeIntrinsicHeight(float width) {
            if (!_canComputeIntrinsics()) {
                return 0.0f;
            }
            _computeChildrenHeightWithMinIntrinsics(width);
            _layoutText(minWidth: width, maxWidth: width);
            return _textPainter.height;
        }

        protected internal override float computeMinIntrinsicHeight(float width) {
            return _computeIntrinsicHeight(width);
        }

        protected internal override float computeMaxIntrinsicHeight(float width) {
            return _computeIntrinsicHeight(width);
        }

        public override float? computeDistanceToActualBaseline(TextBaseline baseline) {
            D.assert(!debugNeedsLayout);
            D.assert(constraints != null);
            D.assert(constraints.debugAssertIsValid());
            _layoutTextWithConstraints(constraints);
            return _textPainter.computeDistanceToActualBaseline(TextBaseline.alphabetic);
        }

        bool _canComputeIntrinsics() {
            foreach (PlaceholderSpan span in _placeholderSpans) {
                switch (span.alignment) {
                    case ui.PlaceholderAlignment.baseline:
                    case ui.PlaceholderAlignment.aboveBaseline:
                    case ui.PlaceholderAlignment.belowBaseline: {
                        D.assert(RenderObject.debugCheckingIntrinsics,
                        () => "Intrinsics are not available for PlaceholderAlignment.baseline, " +
                        "PlaceholderAlignment.aboveBaseline, or PlaceholderAlignment.belowBaseline,");
                        return false;
                    }
                    case ui.PlaceholderAlignment.top:
                    case ui.PlaceholderAlignment.middle:
                    case ui.PlaceholderAlignment.bottom: {
                        continue;
                    }
                }
            }
            return true;
        }

        void _computeChildrenWidthWithMaxIntrinsics(float height) {
            RenderBox child = firstChild;
            List<PlaceholderDimensions> placeholderDimensions = new List<PlaceholderDimensions>(new PlaceholderDimensions[childCount]);
            int childIndex = 0;
            while (child != null) {
                placeholderDimensions[childIndex] = new PlaceholderDimensions(
                size: new Size(child.getMaxIntrinsicWidth(height), height),
                alignment: _placeholderSpans[childIndex].alignment,
                baseline: _placeholderSpans[childIndex].baseline,
                baselineOffset:0.0f
            );
            child = childAfter(child);
            childIndex += 1;
            }
            _textPainter.setPlaceholderDimensions(placeholderDimensions);
        }

        void _computeChildrenWidthWithMinIntrinsics(float height) {
            RenderBox child = firstChild;
            List<PlaceholderDimensions> placeholderDimensions = new List<PlaceholderDimensions>(new PlaceholderDimensions[childCount]);
            int childIndex = 0;
            while (child != null) {
                float intrinsicWidth = child.getMinIntrinsicWidth(height);
                float intrinsicHeight = child.getMinIntrinsicHeight(intrinsicWidth);
                placeholderDimensions[childIndex] = new PlaceholderDimensions(
                    size: new Size(intrinsicWidth, intrinsicHeight),
                    alignment: _placeholderSpans[childIndex].alignment,
                    baseline: _placeholderSpans[childIndex].baseline,
                    baselineOffset:0.0f
                );
                child = childAfter(child);
                childIndex += 1;
            }
            _textPainter.setPlaceholderDimensions(placeholderDimensions);
        }

        void _computeChildrenHeightWithMinIntrinsics(float width) {
            RenderBox child = firstChild;
            List<PlaceholderDimensions> placeholderDimensions = new List<PlaceholderDimensions>(new PlaceholderDimensions[childCount]);
            int childIndex = 0;
            while (child != null) {
                float intrinsicHeight = child.getMinIntrinsicHeight(width);
                float intrinsicWidth = child.getMinIntrinsicWidth(intrinsicHeight);
                placeholderDimensions[childIndex] = new PlaceholderDimensions(
                    size: new Size(intrinsicWidth, intrinsicHeight),
                    alignment: _placeholderSpans[childIndex].alignment,
                    baseline: _placeholderSpans[childIndex].baseline,
                    baselineOffset:0.0f
                );
                child = childAfter(child);
                childIndex += 1;
            }
            _textPainter.setPlaceholderDimensions(placeholderDimensions);
        }

        protected override bool hitTestSelf(Offset position) {
            return true;
        }
        
        protected override bool hitTestChildren(BoxHitTestResult boxHitTestResult, Offset position = null) {
            RenderBox child = firstChild;
            while (child != null) {
                TextParentData textParentData = child.parentData as TextParentData;
                Matrix4 transform = Matrix4.translationValues(
                    textParentData.offset.dx,
                    textParentData.offset.dy,
                    0.0f
                );
                    transform.scale(
                    textParentData.scale,
                    textParentData.scale,
                    textParentData.scale
                );
                bool isHit = boxHitTestResult.addWithPaintTransform(
                    transform: transform,
                    position: position,
                    hitTest: (BoxHitTestResult result, Offset transformed) => {
                        D.assert(() => {
                            Offset manualPosition = (position - textParentData.offset) / textParentData.scale;
                            return (transformed.dx - manualPosition.dx).abs() < foundation_.precisionErrorTolerance
                                   && (transformed.dy - manualPosition.dy).abs() < foundation_.precisionErrorTolerance;
                        });
                        return child.hitTest(result, position: transformed);
                    }
                );
                if (isHit) {
                    return true;
                }
                child = childAfter(child);
            }
            return false;
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
            /*if (_hoverAnnotation != null) {
                RendererBinding.instance.mouseTracker.attachAnnotation(_hoverAnnotation);
            }*/
        }

        public override void detach() {
            if (_listenerAttached) {
                RawKeyboard.instance.removeListener(_handleKeyEvent);
            }

            base.detach();
          
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

      

        public override void handleEvent(PointerEvent evt, HitTestEntry entry) {
            D.assert(debugHandleEvent(evt, entry));
            if (!(evt is PointerDownEvent)) {
                return;
            }
            
            _layoutTextWithConstraints(constraints);
            Offset offset = ((BoxHitTestEntry) entry).localPosition;
            TextPosition position = _textPainter.getPositionForOffset(offset);
            InlineSpan span = _textPainter.text.getSpanForPosition(position);
            if (span == null) {
                return;
            }
            if (span is TextSpan) {
                TextSpan textSpan = (TextSpan)span;
                textSpan.recognizer?.addPointer(evt as PointerDownEvent);
            }
        }
        
        bool debugHasOverflowShader {
            get { return _overflowShader != null; }
        }


        void _layoutChildren(BoxConstraints constraints) {
            if (childCount == 0) {
                return;
            }
            RenderBox child = firstChild;
            _placeholderDimensions = new List<PlaceholderDimensions>(new PlaceholderDimensions[childCount]);
            int childIndex = 0;
            while (child != null) {
                child.layout(
                    new BoxConstraints(
                        maxWidth: constraints.maxWidth
                    ),
                    parentUsesSize: true
                );
                float baselineOffset;
                switch (_placeholderSpans[childIndex].alignment) {
                    case ui.PlaceholderAlignment.baseline: {
                        baselineOffset = child.getDistanceToBaseline(
                        _placeholderSpans[childIndex].baseline
                        ) ?? 0.0f;
                        break;
                    }
                    default: {
                        baselineOffset = 0.0f;
                        break;
                    }
                }
                _placeholderDimensions[childIndex] = new PlaceholderDimensions(
                    size: child.size,
                    alignment: _placeholderSpans[childIndex].alignment,
                    baseline: _placeholderSpans[childIndex].baseline,
                    baselineOffset: baselineOffset
                );
                child = childAfter(child);
                childIndex += 1;
            }
        }
        
        void _setParentData() {
            RenderBox child = firstChild;
            int childIndex = 0;
            while (child != null && childIndex < _textPainter.inlinePlaceholderBoxes.Count) {
                TextParentData textParentData = child.parentData as TextParentData;
                textParentData.offset = new Offset(
                    _textPainter.inlinePlaceholderBoxes[childIndex].left,
                    _textPainter.inlinePlaceholderBoxes[childIndex].top
                );
                textParentData.scale = _textPainter.inlinePlaceholderScales[childIndex];
                child = childAfter(child);
                childIndex += 1;
            }
        }
        
        protected override void performLayout() {
             BoxConstraints constraints = this.constraints;
             _layoutChildren(constraints);
            _layoutTextWithConstraints(constraints);
             _setParentData();
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
                        _overflowShader = null;
                        break;
                    case TextOverflow.clip:
                    case TextOverflow.ellipsis:
                        _needsClipping = true;
                        _overflowShader = null;
                        break;
                    case TextOverflow.fade:
                        D.assert(textDirection != null);
                        _needsClipping = true;
                        TextPainter fadeSizePainter = new TextPainter(
                            text: new TextSpan(style: _textPainter.text.style, text: "\u2026"),
                            textDirection: textDirection.Value,
                            textScaleFactor: textScaleFactor,
                            locale: locale
                        );
                        fadeSizePainter.layout();
                        if (didOverflowWidth) {
                            float fadeEnd = 0, fadeStart = 0;
                            switch (textDirection) {
                                case TextDirection.rtl:
                                    fadeEnd = 0.0f;
                                    fadeStart = fadeSizePainter.width;
                                    break;
                                case TextDirection.ltr:
                                    fadeEnd = size.width;
                                    fadeStart = fadeEnd - fadeSizePainter.width;
                                    break;
                            }
                            _overflowShader = ui.Gradient.linear(
                                    new Offset(fadeStart, 0.0f),
                                    new Offset(fadeEnd, 0.0f),
                                    new List<Color>{new Color(0xFFFFFFFF), new Color(0x00FFFFFF)}
                                );
                        } else {
                            float fadeEnd = size.height;
                            float fadeStart = fadeEnd - fadeSizePainter.height / 2.0f;
                            _overflowShader = ui.Gradient.linear(
                                new Offset(0.0f, fadeStart),
                                new Offset(0.0f, fadeEnd),
                                new List<Color> {new Color(0xFFFFFFFF), new Color(0x00FFFFFF)}
                            );
                        }
                        break;
                }
            }
            else {
                _needsClipping = false;
                _overflowShader = null;
            }
        }
        
        public override void paint(PaintingContext context, Offset offset) {
              _layoutTextWithConstraints(constraints);

            D.assert(() => {
              if (RenderingDebugUtils.debugRepaintTextRainbowEnabled) {
                  Paint paint = new Paint();
                  paint.color = RenderingDebugUtils.debugCurrentRepaintColor.toColor();
                context.canvas.drawRect(offset & size, paint);
              }
              return true;
            });

            if (_needsClipping) {
              Rect bounds = offset & size;
              if (_overflowShader != null) {
                  context.canvas.saveLayer(bounds, new Paint());
              } else {
                context.canvas.save();
              }
              context.canvas.clipRect(bounds);
            }
            _textPainter.paint(context.canvas, offset);

            RenderBox child = firstChild;
            int childIndex = 0;
            while (child != null && childIndex < _textPainter.inlinePlaceholderBoxes.Count) {
                TextParentData textParentData = child.parentData as TextParentData;

                float scale = textParentData.scale;
                context.pushTransform(
                    needsCompositing,
                    offset + textParentData.offset,
                    Matrix4.diagonal3Values(scale, scale, scale),
                    (PaintingContext context2, Offset offset2) => {
                    context2.paintChild(
                        child,
                        offset2
                    );
                });
                child = childAfter(child);
                childIndex += 1;
            }
            if (_needsClipping) {
                if (_overflowShader != null) {
                    context.canvas.translate(offset.dx, offset.dy);
                    Paint paint = new Paint();
                    paint.blendMode = BlendMode.modulate;
                    paint.shader = _overflowShader;
                    context.canvas.drawRect(Offset.zero & size, paint);
                }
                context.canvas.restore();
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

        void _layoutText(float minWidth = 0.0f, float maxWidth = float.PositiveInfinity) {
            var widthMatters = softWrap || overflow == TextOverflow.ellipsis;
            _textPainter.layout(minWidth, widthMatters ? maxWidth : float.PositiveInfinity);
        }

        public override void systemFontsDidChange() {
            base.systemFontsDidChange();
            _textPainter.markNeedsLayout();
        }
        
        List<PlaceholderDimensions> _placeholderDimensions;
        
        void _layoutTextWithConstraints(BoxConstraints constraints) {
            _textPainter.setPlaceholderDimensions(_placeholderDimensions);
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
            properties.add(new DiagnosticsProperty<Locale>("locale", locale, defaultValue: null));
            properties.add(new IntProperty("maxLines", maxLines, ifNull: "unlimited"));
        }
    }
}