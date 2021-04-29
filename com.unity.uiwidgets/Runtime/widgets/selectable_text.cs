using System;
using System.Collections.Generic;
using Unity.UIWidgets.async;
using Unity.UIWidgets.cupertino;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.gestures;
//using Unity.UIWidgets.material;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.rendering;
using Unity.UIWidgets.service;
using Unity.UIWidgets.ui;
using Color = Unity.UIWidgets.ui.Color;
using Constants = Unity.UIWidgets.gestures.Constants;
using TextStyle = Unity.UIWidgets.painting.TextStyle;

namespace Unity.UIWidgets.widgets {
    /*public class SelectableText : StatefulWidget {
        public SelectableText(string data,
            Key key = null,
            TextStyle style = null,
            TextAlign? textAlign = null,
            bool? softWrap = null,
            TextOverflow? overflow = null,
            float? textScaleFactor = null,
            int? maxLines = null,
            FocusNode focusNode = null,
            Color selectionColor = null,
            GestureTapDownCallback onTapDown = null,
            GestureTapUpCallback onTapUp = null,
            GestureTapCancelCallback onTapCancel = null) : base(key) {
            D.assert(data != null);
            textSpan = null;
            this.data = data;
            this.style = style;
            this.textAlign = textAlign;
            this.softWrap = softWrap;
            this.overflow = overflow;
            this.textScaleFactor = textScaleFactor;
            this.maxLines = maxLines;
            this.focusNode = focusNode ?? new FocusNode();
            this.selectionColor = selectionColor;
            this.onTapDown = onTapDown;
            this.onTapUp = onTapUp;
            this.onTapCancel = onTapCancel;
        }

        public SelectableText(TextSpan textSpan,
            Key key = null,
            TextStyle style = null,
            TextAlign? textAlign = null,
            bool? softWrap = null,
            TextOverflow? overflow = null,
            float? textScaleFactor = null,
            int? maxLines = null,
            FocusNode focusNode = null,
            Color selectionColor = null,
            GestureTapDownCallback onTapDown = null,
            GestureTapUpCallback onTapUp = null,
            GestureTapCancelCallback onTapCancel = null) : base(key) {
            D.assert(textSpan != null);
            this.textSpan = textSpan;
            data = null;
            this.style = style;
            this.textAlign = textAlign;
            this.softWrap = softWrap;
            this.overflow = overflow;
            this.textScaleFactor = textScaleFactor;
            this.maxLines = maxLines;
            this.focusNode = focusNode ?? new FocusNode();
            this.selectionColor = selectionColor;
            this.onTapDown = onTapDown;
            this.onTapUp = onTapUp;
            this.onTapCancel = onTapCancel;
        }

        public static SelectableText rich(TextSpan textSpan,
            Key key = null,
            TextStyle style = null,
            TextAlign? textAlign = null,
            bool? softWrap = null,
            TextOverflow? overflow = null,
            float? textScaleFactor = null,
            int? maxLines = null,
            FocusNode focusNode = null,
            Color selectionColor = null,
            GestureTapDownCallback onTapDown = null,
            GestureTapUpCallback onTapUp = null,
            GestureTapCancelCallback onTapCancel = null) {
            return new SelectableText(
                textSpan, key,
                style,
                textAlign,
                softWrap,
                overflow,
                textScaleFactor,
                maxLines,
                focusNode,
                selectionColor,
                onTapDown,
                onTapUp,
                onTapCancel);
        }

        public readonly string data;

        public readonly FocusNode focusNode;

        public readonly TextSpan textSpan;

        public readonly TextStyle style;

        public readonly TextAlign? textAlign;

        public readonly bool? softWrap;

        public readonly TextOverflow? overflow;

        public readonly float? textScaleFactor;

        public readonly int? maxLines;

        public readonly Color selectionColor;

        public readonly GestureTapDownCallback onTapDown;

        public readonly GestureTapUpCallback onTapUp;

        public readonly GestureTapCancelCallback onTapCancel;

        public override State createState() {
            return new _SelectableTextState();
        }
    }


    class _SelectableTextState : State<SelectableText>, WidgetsBindingObserver {
        readonly GlobalKey _richTextKey = GlobalKey.key();

        RenderParagraph _renderParagragh {
            get { return (RenderParagraph) _richTextKey.currentContext.findRenderObject(); }
        }

        public override void initState() {
            base.initState();
            widget.focusNode.addListener(_handleFocusChanged);
        }


        public override void didUpdateWidget(StatefulWidget old) {
            SelectableText oldWidget = (SelectableText) old;
            base.didUpdateWidget(oldWidget);

            if (oldWidget.focusNode != widget.focusNode) {
                oldWidget.focusNode.removeListener(_handleFocusChanged);
                widget.focusNode.addListener(_handleFocusChanged);
            }
        }

        public override void dispose() {
            widget.focusNode.removeListener(_handleFocusChanged);
            base.dispose();
        }

        bool _hasFocus {
            get { return widget.focusNode.hasFocus; }
        }

        void _handleFocusChanged() {
            if (_hasFocus) {
                WidgetsBinding.instance.addObserver(this);
                _renderParagragh.hasFocus = true;
            }
            else {
                WidgetsBinding.instance.removeObserver(this);
                _renderParagragh.hasFocus = false;
            }
        }


        public void didChangeMetrics() {
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

        void _handleTapDown(TapDownDetails details) {
            widget.onTapDown?.Invoke(details);
        }

        void _handleSingleTapUp(TapUpDetails details) {
            widget.onTapUp?.Invoke(details);
        }

        void _handleSingleTapCancel() {
            widget.onTapCancel?.Invoke();
        }

        void _handleLongPress() {
        }

        void _handleDragSelectionStart(DragStartDetails details) {
            _renderParagragh.selectPositionAt(
                from: details.globalPosition,
                cause: SelectionChangedCause.drag);
        }

        void _handleDragSelectionUpdate(DragStartDetails startDetails,
            DragUpdateDetails updateDetails) {
            _renderParagragh.selectPositionAt(
                from: startDetails.globalPosition,
                to: updateDetails.globalPosition,
                cause: SelectionChangedCause.drag);
        }

        public override Widget build(BuildContext context) {
            
            FocusScope.of(context).reparentIfNeeded(widget.focusNode);

            DefaultTextStyle defaultTextStyle = DefaultTextStyle.of(context);
            TextStyle effectiveTextStyle = widget.style;
            if (widget.style == null || widget.style.inherit) {
                effectiveTextStyle = defaultTextStyle.style.merge(widget.style);
            }

            Widget child = new RichText(
                key: _richTextKey,
                textAlign: widget.textAlign ?? defaultTextStyle.textAlign ?? TextAlign.left,
                softWrap: widget.softWrap ?? defaultTextStyle.softWrap,
                overflow: widget.overflow ?? defaultTextStyle.overflow,
                textScaleFactor: widget.textScaleFactor ?? MediaQuery.textScaleFactorOf(context),
                maxLines: widget.maxLines ?? defaultTextStyle.maxLines,
                text: new TextSpan(
                    style: effectiveTextStyle,
                    text: widget.data,
                    children: widget.textSpan != null ? new List<InlineSpan> {widget.textSpan} : null
                ),
                onSelectionChanged: () => {
                    if (_hasFocus) {
                        return;
                    }

                    FocusScope.of(this.context).requestFocus(widget.focusNode);
                },
                selectionColor: widget.selectionColor ?? Colors.blue);

            return new IgnorePointer(
                ignoring: false,
                child: new RichTextSelectionGestureDetector(
                    onTapDown: _handleTapDown,
                    onSingleTapUp: _handleSingleTapUp,
                    onSingleTapCancel: _handleSingleTapCancel,
                    onSingleLongTapStart: _handleLongPress,
                    onDragSelectionStart: _handleDragSelectionStart,
                    onDragSelectionUpdate: _handleDragSelectionUpdate,
                    behavior: HitTestBehavior.translucent,
                    child: child
                )
            );
        }
    }

    public class RichTextSelectionGestureDetector : StatefulWidget {
        public RichTextSelectionGestureDetector(
            Key key = null,
            GestureTapDownCallback onTapDown = null,
            GestureTapUpCallback onSingleTapUp = null,
            GestureTapCancelCallback onSingleTapCancel = null,
            GestureLongPressCallback onSingleLongTapStart = null,
            GestureTapDownCallback onDoubleTapDown = null,
            GestureDragStartCallback onDragSelectionStart = null,
            DragSelectionUpdateCallback onDragSelectionUpdate = null,
            GestureDragEndCallback onDragSelectionEnd = null,
            HitTestBehavior? behavior = null,
            Widget child = null
        ) : base(key: key) {
            D.assert(child != null);
            this.onTapDown = onTapDown;
            this.onSingleTapUp = onSingleTapUp;
            this.onSingleTapCancel = onSingleTapCancel;
            this.onSingleLongTapStart = onSingleLongTapStart;
            this.onDoubleTapDown = onDoubleTapDown;
            this.onDragSelectionStart = onDragSelectionStart;
            this.onDragSelectionUpdate = onDragSelectionUpdate;
            this.onDragSelectionEnd = onDragSelectionEnd;
            this.behavior = behavior;
            this.child = child;
        }

        public readonly GestureTapDownCallback onTapDown;

        public readonly GestureTapUpCallback onSingleTapUp;

        public readonly GestureTapCancelCallback onSingleTapCancel;

        public readonly GestureLongPressCallback onSingleLongTapStart;

        public readonly GestureTapDownCallback onDoubleTapDown;

        public readonly GestureDragStartCallback onDragSelectionStart;

        public readonly DragSelectionUpdateCallback onDragSelectionUpdate;

        public readonly GestureDragEndCallback onDragSelectionEnd;

        public HitTestBehavior? behavior;

        public readonly Widget child;

        public override State createState() {
            return new _RichTextSelectionGestureDetectorState();
        }
    }

    class _RichTextSelectionGestureDetectorState : State<RichTextSelectionGestureDetector> {
        Timer _doubleTapTimer;
        Offset _lastTapOffset;

        bool _isDoubleTap = false;

        public override void dispose() {
            _doubleTapTimer?.cancel();
            _dragUpdateThrottleTimer?.cancel();
            base.dispose();
        }

        void _handleTapDown(TapDownDetails details) {
            if (widget.onTapDown != null) {
                widget.onTapDown(details);
            }

            if (_doubleTapTimer != null &&
                _isWithinDoubleTapTolerance(details.globalPosition)) {
                if (widget.onDoubleTapDown != null) {
                    widget.onDoubleTapDown(details);
                }

                _doubleTapTimer.cancel();
                _doubleTapTimeout();
                _isDoubleTap = true;
            }
        }

        void _handleTapUp(TapUpDetails details) {
            if (!_isDoubleTap) {
                if (widget.onSingleTapUp != null) {
                    widget.onSingleTapUp(details);
                }

                _lastTapOffset = details.globalPosition;
                _doubleTapTimer = Timer.create(Constants.kDoubleTapTimeout, _doubleTapTimeout);
            }

            _isDoubleTap = false;
        }

        void _handleTapCancel() {
            if (widget.onSingleTapCancel != null) {
                widget.onSingleTapCancel();
            }
        }

        DragStartDetails _lastDragStartDetails;
        DragUpdateDetails _lastDragUpdateDetails;
        Timer _dragUpdateThrottleTimer;

        void _handleDragStart(DragStartDetails details) {
            D.assert(_lastDragStartDetails == null);
            _lastDragStartDetails = details;
            if (widget.onDragSelectionStart != null) {
                widget.onDragSelectionStart(details);
            }
        }

        void _handleDragUpdate(DragUpdateDetails details) {
            _lastDragUpdateDetails = details; 
            _dragUpdateThrottleTimer = _dragUpdateThrottleTimer ?? Timer.create(TextSelectionUtils._kDragSelectionUpdateThrottle, _handleDragUpdateThrottled);
        }

        object _handleDragUpdateThrottled() {
            D.assert(_lastDragStartDetails != null);
            D.assert(_lastDragUpdateDetails != null);
            if (widget.onDragSelectionUpdate != null) {
                widget.onDragSelectionUpdate(_lastDragStartDetails, _lastDragUpdateDetails);
            }

            _dragUpdateThrottleTimer = null;
            _lastDragUpdateDetails = null;
            return null;
        }

        void _handleDragEnd(DragEndDetails details) {
            D.assert(_lastDragStartDetails != null);
            if (_lastDragUpdateDetails != null) {
                _dragUpdateThrottleTimer.cancel();
                _handleDragUpdateThrottled();
            }

            if (widget.onDragSelectionEnd != null) {
                widget.onDragSelectionEnd(details);
            }

            _dragUpdateThrottleTimer = null;
            _lastDragStartDetails = null;
            _lastDragUpdateDetails = null;
        }

        void _handleLongPressStart() {
            if (!_isDoubleTap && widget.onSingleLongTapStart != null) {
                widget.onSingleLongTapStart();
            }
        }

        object _doubleTapTimeout() {
            _doubleTapTimer = null;
            _lastTapOffset = null;
            return null;
        }

        bool _isWithinDoubleTapTolerance(Offset secondTapOffset) {
            D.assert(secondTapOffset != null);
            if (_lastTapOffset == null) {
                return false;
            }

            Offset difference = secondTapOffset - _lastTapOffset;
            return difference.distance <= Constants.kDoubleTapSlop;
        }

        public override Widget build(BuildContext context) {
            Dictionary<Type, GestureRecognizerFactory> gestures = new Dictionary<Type, GestureRecognizerFactory>();

            gestures.Add(typeof(TapGestureRecognizer), new GestureRecognizerFactoryWithHandlers<TapGestureRecognizer>(
                    () => new TapGestureRecognizer(debugOwner: this),
                    instance => {
                        instance.onTapDown = _handleTapDown;
                        instance.onTapUp = _handleTapUp;
                        instance.onTapCancel = _handleTapCancel;
                    }
                )
            );

            if (widget.onSingleLongTapStart != null) {
                gestures[typeof(LongPressGestureRecognizer)] =
                    new GestureRecognizerFactoryWithHandlers<LongPressGestureRecognizer>(
                        () => new LongPressGestureRecognizer(debugOwner: this, kind: PointerDeviceKind.touch),
                        instance => { instance.onLongPress = _handleLongPressStart; });
            }

            if (widget.onDragSelectionStart != null ||
                widget.onDragSelectionUpdate != null ||
                widget.onDragSelectionEnd != null) {
                gestures.Add(typeof(PanGestureRecognizer),
                    new GestureRecognizerFactoryWithHandlers<PanGestureRecognizer>(
                        () => new PanGestureRecognizer(debugOwner: this, kind: PointerDeviceKind.mouse),
                        instance => {
                            instance.dragStartBehavior = DragStartBehavior.down;
                            instance.onStart = _handleDragStart;
                            instance.onUpdate = _handleDragUpdate;
                            instance.onEnd = _handleDragEnd;
                        }
                    )
                );
            }

            return new RawGestureDetector(
                gestures: gestures,
                behavior: widget.behavior,
                child: widget.child
            );
        }
    }*/
}