using System;
using System.Collections.Generic;
using Unity.UIWidgets.animation;
using Unity.UIWidgets.async;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.gestures;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.rendering;
using Unity.UIWidgets.scheduler;
using Unity.UIWidgets.service;
using Unity.UIWidgets.ui;
using UnityEngine;
using Object = System.Object;
using Rect = Unity.UIWidgets.ui.Rect;
using TextRange = Unity.UIWidgets.ui.TextRange;

namespace Unity.UIWidgets.widgets {
    static class TextSelectionUtils {
        public static TimeSpan _kDragSelectionUpdateThrottle = new TimeSpan(0, 0, 0, 0, 50);
    }

    public enum TextSelectionHandleType {
        left,
        right,
        collapsed,
    }

    enum _TextSelectionHandlePosition {
        start,
        end
    }

    public delegate void TextSelectionOverlayChanged(TextEditingValue value, Rect caretRect);

    public delegate void DragSelectionUpdateCallback(DragStartDetails startDetails, DragUpdateDetails updateDetails);

    public abstract class TextSelectionControls {
        public abstract Widget buildHandle(BuildContext context, TextSelectionHandleType type, float textLineHeight);
        public abstract Offset getHandleAnchor(TextSelectionHandleType type, float textLineHeight);
        public abstract Widget buildToolbar(
            BuildContext context, 
            Rect globalEditableRegion, 
            float textLineHeight,
            Offset position,
            List<TextSelectionPoint> endpoints,
            TextSelectionDelegate selectionDelegate
            );
        public abstract Size getHandleSize(float textLineHeight);

        public virtual bool canCut(TextSelectionDelegate _delegate) {
            return _delegate.cutEnabled && !_delegate.textEditingValue.selection.isCollapsed;
        }


        public bool canCopy(TextSelectionDelegate _delegate) {
            return _delegate.copyEnabled && !_delegate.textEditingValue.selection.isCollapsed;
        }

        public bool canPaste(TextSelectionDelegate _delegate) {
            // TODO(goderbauer): return false when clipboard is empty, https://github.com/flutter/flutter/issues/11254
            return _delegate.pasteEnabled;
        }

        public bool canSelectAll(TextSelectionDelegate _delegate) {
            return _delegate.selectAllEnabled 
                   && _delegate.textEditingValue.text.isNotEmpty() 
                   && _delegate.textEditingValue.selection.isCollapsed;
        }


        public void handleCut(TextSelectionDelegate selectionDelegate) {
            TextEditingValue value = selectionDelegate.textEditingValue;
            Clipboard.setData(new ClipboardData(
                text: value.selection.textInside(value.text)
            ));
            selectionDelegate.textEditingValue = new TextEditingValue(
                text: value.selection.textBefore(value.text)
                      + value.selection.textAfter(value.text),
                selection: TextSelection.collapsed(
                    offset: value.selection.start
                )
            );
            selectionDelegate.bringIntoView(selectionDelegate.textEditingValue.selection.extendPos);
            selectionDelegate.hideToolbar();
        }

        public void handleCopy(TextSelectionDelegate selectionDelegate) {
            TextEditingValue value = selectionDelegate.textEditingValue;
            Clipboard.setData(new ClipboardData(
                text: value.selection.textInside(value.text)
            ));
            selectionDelegate.textEditingValue = new TextEditingValue(
                text: value.text,
                selection: TextSelection.collapsed(offset: value.selection.end)
            );
            selectionDelegate.bringIntoView(selectionDelegate.textEditingValue.selection.extendPos);
            selectionDelegate.hideToolbar();
        }

        public void handlePaste(TextSelectionDelegate selectionDelegate) {
            TextEditingValue value = selectionDelegate.textEditingValue; // Snapshot the input before using `await`.
            Clipboard.getData(Clipboard.kTextPlain).then_((data) => {
                if (data != null) {
                    selectionDelegate.textEditingValue = new TextEditingValue(
                        text: value.selection.textBefore(value.text)
                              + data.text
                              + value.selection.textAfter(value.text),
                        selection: TextSelection.collapsed(
                            offset: value.selection.start + data.text.Length
                        )
                    );

                    selectionDelegate.bringIntoView(selectionDelegate.textEditingValue.selection.extendPos);
                    selectionDelegate.hideToolbar();
                }
            });
        }

        public void handleSelectAll(TextSelectionDelegate selectionDelegate) {
            selectionDelegate.textEditingValue = new TextEditingValue(
                text: selectionDelegate.textEditingValue.text,
                selection: new TextSelection(
                    baseOffset: 0,
                    extentOffset: selectionDelegate.textEditingValue.text.Length
                )
            );
            selectionDelegate.bringIntoView(selectionDelegate.textEditingValue.selection.extendPos);
        }
    }

    public class TextSelectionOverlay {
        public TextSelectionOverlay(
            TextEditingValue value = null,
            BuildContext context = null, 
            Widget debugRequiredFor = null,
            LayerLink toolbarLayerLink = null,
            LayerLink startHandleLayerLink = null,
            LayerLink endHandleLayerLink = null,
            RenderEditable renderObject = null,
            TextSelectionControls selectionControls = null,
            bool? handlesVisible = false,
            TextSelectionDelegate selectionDelegate = null,
            DragStartBehavior dragStartBehavior = DragStartBehavior.start, 
            VoidCallback onSelectionHandleTapped = null) {
            D.assert(value != null);
            D.assert(context != null);
            D.assert(handlesVisible != null);
            _handlesVisible = handlesVisible.Value;
            this.context = context;
            this.debugRequiredFor = debugRequiredFor;
            this.toolbarLayerLink = toolbarLayerLink;
            this.startHandleLayerLink = startHandleLayerLink;
            this.endHandleLayerLink = endHandleLayerLink;
            this.renderObject = renderObject;
            this.selectionControls = selectionControls;
            this.selectionDelegate = selectionDelegate;
            this.onSelectionHandleTapped = onSelectionHandleTapped;
            _value = value;
            OverlayState overlay = Overlay.of(context, rootOverlay: true);
            D.assert(overlay != null, () => $"No Overlay widget exists above {context}.\n" +
                                            "Usually the Navigator created by WidgetsApp provides the overlay. Perhaps your " +
                                            "app content was created above the Navigator with the WidgetsApp builder parameter.");
            _toolbarController = new AnimationController(duration: fadeDuration, vsync: overlay);
            this.dragStartBehavior = dragStartBehavior;
        }

        public readonly BuildContext context;
        public readonly Widget debugRequiredFor;
        public readonly LayerLink toolbarLayerLink;
        public readonly LayerLink startHandleLayerLink;
        public readonly LayerLink endHandleLayerLink;
        public readonly RenderEditable renderObject;
        public readonly TextSelectionControls selectionControls;
        public readonly TextSelectionDelegate selectionDelegate;
        public readonly DragStartBehavior dragStartBehavior;
        public readonly VoidCallback onSelectionHandleTapped;

        public static readonly TimeSpan fadeDuration = TimeSpan.FromMilliseconds(150);
        AnimationController _toolbarController;

        Animation<float> _toolbarOpacity {
            get { return _toolbarController.view; }
        }

        TextEditingValue  value {
            get { return _value; }
        }

        TextEditingValue _value;

        List<OverlayEntry> _handles;

        OverlayEntry _toolbar;

        TextSelection _selection {
            get { return _value.selection; }
        }

        bool _handlesVisible = false;
        public bool handlesVisible {
            get {
                return _handlesVisible;
            }
            set {
                if (_handlesVisible == value)
                    return;
                _handlesVisible = value;
                // If we are in build state, it will be too late to update visibility.
                // We will need to schedule the build in next frame.
                if (SchedulerBinding.instance.schedulerPhase == SchedulerPhase.persistentCallbacks) {
                    SchedulerBinding.instance.addPostFrameCallback((TimeSpan timespan) => {
                        _markNeedsBuild();
                    });
                } else {
                    _markNeedsBuild();
                }
            }
            
        }
        
        public void showHandles() {
            D.assert(_handles == null);
            _handles = new List<OverlayEntry> {
                new OverlayEntry(builder: (BuildContext context) =>
                    _buildHandle(context, _TextSelectionHandlePosition.start)),
                new OverlayEntry(builder: (BuildContext context) =>
                    _buildHandle(context, _TextSelectionHandlePosition.end)),
            };
            Overlay.of(this.context, rootOverlay: true, debugRequiredFor: debugRequiredFor).insertAll(_handles);
        }

        public void hideHandles() {
            if (_handles != null) {
                _handles[0].remove();
                _handles[1].remove();
                _handles = null;
            }
        }
        
        public void showToolbar() {
            D.assert(_toolbar == null);
            _toolbar = new OverlayEntry(builder: _buildToolbar);
            Overlay.of(context, rootOverlay: true, debugRequiredFor: debugRequiredFor).insert(_toolbar);
            _toolbarController.forward(from: 0.0f);
        }

        public void update(TextEditingValue newValue) {
            if (_value == newValue) {
                return;
            }

            _value = newValue;
            if (SchedulerBinding.instance.schedulerPhase == SchedulerPhase.persistentCallbacks) {
                SchedulerBinding.instance.addPostFrameCallback((duration) => _markNeedsBuild());
            }
            else {
                _markNeedsBuild();
            }
        }

        public void updateForScroll() {
            _markNeedsBuild();
        }

        void _markNeedsBuild() {
            if (_handles != null) {
                _handles[0].markNeedsBuild();
                _handles[1].markNeedsBuild();
            }

            _toolbar?.markNeedsBuild();
        }

        public bool handlesAreVisible {
            get { return _handles != null && handlesVisible; }
        }


        public bool toolbarIsVisible {
            get { return _toolbar != null; }
        }

        public void hide() {
            if (_handles != null) {
                _handles[0].remove();
                _handles[1].remove();
                _handles = null;
            }
            if (_toolbar != null) {
                hideToolbar();
            }
        }

        void hideToolbar() {
            D.assert(_toolbar != null);
            _toolbarController.stop();
            _toolbar.remove();
            _toolbar = null;
        }

        public void dispose() {
            hide();
            _toolbarController.dispose();
        }

        Widget _buildHandle(BuildContext context, _TextSelectionHandlePosition position) {
            if ((_selection.isCollapsed && position == _TextSelectionHandlePosition.end) ||
                selectionControls == null) {
                return new Container(); // hide the second handle when collapsed
            }

            return new Visibility(
                visible: handlesVisible,
                child: new _TextSelectionHandleOverlay(
                    onSelectionHandleChanged: (TextSelection newSelection) => {
                        _handleSelectionHandleChanged(newSelection, position);
                    },
                    onSelectionHandleTapped: onSelectionHandleTapped,
                    startHandleLayerLink: startHandleLayerLink,
                    endHandleLayerLink: endHandleLayerLink,
                    renderObject: renderObject,
                    selection: _selection,
                    selectionControls: selectionControls,
                    position: position,
                    dragStartBehavior: dragStartBehavior
                )
            );
        }

        Widget _buildToolbar(BuildContext context) {
            if (selectionControls == null) {
                return new Container();
            }

            // Find the horizontal midpoint, just above the selected text.
            List<TextSelectionPoint> endpoints = renderObject.getEndpointsForSelection(_selection);

            Rect editingRegion = Rect.fromPoints(renderObject.localToGlobal(Offset.zero),
                renderObject.localToGlobal(renderObject.size.bottomRight(Offset.zero))
            );

            bool isMultiline = endpoints.last().point.dy - endpoints.first().point.dy >
                                     renderObject.preferredLineHeight / 2;
            
            float midX = isMultiline
                ? editingRegion.width / 2
                : (endpoints.first().point.dx + endpoints.last().point.dx) / 2;

            Offset midpoint = new Offset(
                midX,
                endpoints[0].point.dy - renderObject.preferredLineHeight
            );
            
            return new FadeTransition(
                opacity: _toolbarOpacity,
                child: new CompositedTransformFollower(
                    link: toolbarLayerLink,
                    showWhenUnlinked: false,
                    offset: -editingRegion.topLeft,
                    child: selectionControls.buildToolbar(
                        context,
                        editingRegion,
                        renderObject.preferredLineHeight,
                        midpoint,
                        endpoints,
                        selectionDelegate)
                )
            );
        }

        void _handleSelectionHandleChanged(TextSelection newSelection, _TextSelectionHandlePosition position) {
            TextPosition textPosition = null;
            switch (position) {
                case _TextSelectionHandlePosition.start:
                    textPosition = newSelection.basePos;
                    break;
                case _TextSelectionHandlePosition.end:
                    textPosition = newSelection.extendPos;
                    break;
            }

            selectionDelegate.textEditingValue =
                _value.copyWith(selection: newSelection, composing: TextRange.empty);
            selectionDelegate.bringIntoView(textPosition);
        }
    }

    class _TextSelectionHandleOverlay : StatefulWidget {
        internal _TextSelectionHandleOverlay(
            Key key = null,
            TextSelection selection = null,
            _TextSelectionHandlePosition position = _TextSelectionHandlePosition.start,
            LayerLink startHandleLayerLink = null,
            LayerLink endHandleLayerLink = null,
            RenderEditable renderObject = null,
            ValueChanged<TextSelection> onSelectionHandleChanged = null,
            VoidCallback onSelectionHandleTapped = null,
            TextSelectionControls selectionControls = null,
            DragStartBehavior dragStartBehavior = DragStartBehavior.start
        ) : base(key: key) {
            this.selection = selection;
            this.position = position;
            this.startHandleLayerLink = startHandleLayerLink;
            this.endHandleLayerLink = endHandleLayerLink;
            this.renderObject = renderObject;
            this.onSelectionHandleChanged = onSelectionHandleChanged;
            this.onSelectionHandleTapped = onSelectionHandleTapped;
            this.selectionControls = selectionControls;
            this.dragStartBehavior = dragStartBehavior;
        }

        public readonly TextSelection selection;
        public readonly _TextSelectionHandlePosition position;
        public readonly LayerLink startHandleLayerLink;
        public readonly LayerLink endHandleLayerLink;
        public readonly RenderEditable renderObject;
        public readonly ValueChanged<TextSelection> onSelectionHandleChanged;
        public readonly VoidCallback onSelectionHandleTapped;
        public readonly TextSelectionControls selectionControls;
        public readonly DragStartBehavior dragStartBehavior;

        public override State createState() {
            return new _TextSelectionHandleOverlayState();
        }

        internal ValueListenable<bool> _visibility {
            get {
                switch (position) {
                    case _TextSelectionHandlePosition.start:
                        return renderObject.selectionStartInViewport;
                    case _TextSelectionHandlePosition.end:
                        return renderObject.selectionEndInViewport;
                }

                return null;
            }
        }
    }

    class _TextSelectionHandleOverlayState : SingleTickerProviderStateMixin<_TextSelectionHandleOverlay> {
        Offset _dragPosition;

        AnimationController _controller;

        const float kMinInteractiveDimension = 48.0f;
        Animation<float> _opacity {
            get { return _controller.view; }
        }

        public override void initState() {
            base.initState();
            _controller = new AnimationController(duration: TextSelectionOverlay.fadeDuration, vsync: this);
            _handleVisibilityChanged();
            widget._visibility.addListener(_handleVisibilityChanged);
        }

        void _handleVisibilityChanged() {
            if (widget._visibility.value) {
                _controller.forward();
            }
            else {
                _controller.reverse();
            }
        }

        public override void didUpdateWidget(StatefulWidget oldWidget) {
            base.didUpdateWidget(oldWidget);
            (oldWidget as _TextSelectionHandleOverlay)._visibility.removeListener(_handleVisibilityChanged);
            _handleVisibilityChanged();
            widget._visibility.addListener(_handleVisibilityChanged);
        }

        public override void dispose() {
            widget._visibility.removeListener(_handleVisibilityChanged);
            _controller.dispose();
            base.dispose();
        }

        void _handleDragStart(DragStartDetails details) {
            Size handleSize = widget.selectionControls.getHandleSize(
                widget.renderObject.preferredLineHeight
            );
            _dragPosition = details.globalPosition + new Offset(0.0f, -handleSize.height);
        }

        void _handleDragUpdate(DragUpdateDetails details) {
            _dragPosition += details.delta;
            TextPosition position = widget.renderObject.getPositionForPoint(_dragPosition);

            if (widget.selection.isCollapsed) {
                widget.onSelectionHandleChanged(TextSelection.fromPosition(position));
                return;
            }

            TextSelection newSelection = null;
            switch (widget.position) {
                case _TextSelectionHandlePosition.start:
                    newSelection = new TextSelection(
                        baseOffset: position.offset,
                        extentOffset: widget.selection.extentOffset
                    );
                    break;
                case _TextSelectionHandlePosition.end:
                    newSelection = new TextSelection(
                        baseOffset: widget.selection.baseOffset,
                        extentOffset: position.offset
                    );
                    break;
            }

            if (newSelection.baseOffset >= newSelection.extentOffset) {
                return; // don't allow order swapping.
            }

            widget.onSelectionHandleChanged(newSelection);
        }

        void _handleTap() {
            if (widget.onSelectionHandleTapped != null)
                widget.onSelectionHandleTapped();
        }

        public override Widget build(BuildContext context) {
            LayerLink layerLink = null;
            TextSelectionHandleType type = TextSelectionHandleType.left;

            switch (widget.position) {
                case _TextSelectionHandlePosition.start:
                    layerLink = widget.startHandleLayerLink;
                    type = _chooseType(
                        widget.renderObject.textDirection,
                        TextSelectionHandleType.left,
                        TextSelectionHandleType.right
                    );
                    break;
                case _TextSelectionHandlePosition.end:
                    D.assert(!widget.selection.isCollapsed);
                    layerLink = widget.endHandleLayerLink;
                    type = _chooseType(
                        widget.renderObject.textDirection,
                        TextSelectionHandleType.right,
                        TextSelectionHandleType.left
                    );
                    break;
            }

            Offset handleAnchor = widget.selectionControls.getHandleAnchor(
                type,
                widget.renderObject.preferredLineHeight
            );
            Size handleSize = widget.selectionControls.getHandleSize(
                widget.renderObject.preferredLineHeight
            );

            Rect handleRect = Rect.fromLTWH(
                -handleAnchor.dx,
                -handleAnchor.dy,
                handleSize.width,
                handleSize.height
            );
            
            Rect interactiveRect = handleRect.expandToInclude(
                Rect.fromCircle(center: handleRect.center, radius: kMinInteractiveDimension/ 2)
            );
            RelativeRect padding = RelativeRect.fromLTRB(
                Mathf.Max((interactiveRect.width - handleRect.width) / 2, 0),
                Mathf.Max((interactiveRect.height - handleRect.height) / 2, 0),
                Mathf.Max((interactiveRect.width - handleRect.width) / 2, 0),
                Mathf.Max((interactiveRect.height - handleRect.height) / 2, 0)
            );

            return new CompositedTransformFollower(
                link: layerLink,
                offset: interactiveRect.topLeft,
                showWhenUnlinked: false,
                child: new FadeTransition(
                    opacity: _opacity,
                    child: new Container(
                        alignment: Alignment.topLeft,
                        width: interactiveRect.width,
                        height: interactiveRect.height,
                        child: new GestureDetector(
                            behavior: HitTestBehavior.translucent,
                            dragStartBehavior: widget.dragStartBehavior,
                            onPanStart: _handleDragStart,
                            onPanUpdate: _handleDragUpdate,
                            onTap: _handleTap,
                            child: new Padding(
                                padding: EdgeInsets.only(
                                    left: padding.left,
                                    top: padding.top,
                                    right: padding.right,
                                    bottom: padding.bottom
                                ),
                                child: widget.selectionControls.buildHandle(context, type,
                                    widget.renderObject.preferredLineHeight)
                            )
                        )
                    )
                )
            );

        }

        TextSelectionHandleType _chooseType(
            TextDirection? textDirection,
            TextSelectionHandleType ltrType,
            TextSelectionHandleType rtlType
        ) {
            if (widget.selection.isCollapsed) {
                return TextSelectionHandleType.collapsed;
            }

            D.assert(textDirection != null);
            switch (textDirection) {
                case TextDirection.ltr:
                    return ltrType;
                case TextDirection.rtl:
                    return rtlType;
            }

            D.assert(() => throw new UIWidgetsError($"invalid endpoint.direction {textDirection}"));
            return ltrType;
        }
    }
    public class TextSelectionGestureDetectorBuilder {
        public TextSelectionGestureDetectorBuilder(TextSelectionGestureDetectorBuilderDelegate _delegate = null ) {
            D.assert(_delegate != null);
            this._delegate = _delegate;
        }

        protected readonly TextSelectionGestureDetectorBuilderDelegate _delegate;

        public bool shouldShowSelectionToolbar {
            get { return  _shouldShowSelectionToolbar;  }
        }
        bool _shouldShowSelectionToolbar = true;

        protected EditableTextState editableText {
            get { return _delegate.editableTextKey.currentState; }
        }
        protected RenderEditable renderEditable {
            get { return editableText.renderEditable; }
        }
        protected void onTapDown(TapDownDetails details) {
            renderEditable.handleTapDown(details);
            PointerDeviceKind? kind = details.kind;
            _shouldShowSelectionToolbar = kind == null
                              || kind == PointerDeviceKind.touch
                              || kind == PointerDeviceKind.stylus;
        }
        protected virtual void onForcePressStart(ForcePressDetails details) {
            D.assert(_delegate.forcePressEnabled);
            _shouldShowSelectionToolbar = true;
            if (_delegate.selectionEnabled) {
              renderEditable.selectWordsInRange(
                from: details.globalPosition,
                cause: SelectionChangedCause.forcePress
              );
            }
        } 
        protected virtual void onForcePressEnd(ForcePressDetails details) {
            D.assert(_delegate.forcePressEnabled);
            renderEditable.selectWordsInRange(
              from: details.globalPosition,
              cause: SelectionChangedCause.forcePress
            );
            if (shouldShowSelectionToolbar)
              editableText.showToolbar();
        }
        protected virtual void onSingleTapUp(TapUpDetails details) {
            if (_delegate.selectionEnabled) {
                renderEditable.selectWordEdge(cause: SelectionChangedCause.tap);
            }
        }
        protected void onSingleTapCancel() {/* Subclass should override this method if needed. */}
        protected virtual void onSingleLongTapStart(LongPressStartDetails details) {
            if (_delegate.selectionEnabled) {
              renderEditable.selectPositionAt(
                from: details.globalPosition,
                cause: SelectionChangedCause.longPress
              );
            }
        }
        protected virtual void onSingleLongTapMoveUpdate(LongPressMoveUpdateDetails details) {
        if (_delegate.selectionEnabled) {
          renderEditable.selectPositionAt(
            from: details.globalPosition,
            cause: SelectionChangedCause.longPress
          );
        }
      }

      protected void onSingleLongTapEnd(LongPressEndDetails details) {
        if (shouldShowSelectionToolbar)
          editableText.showToolbar();
      } 

      protected void onDoubleTapDown(TapDownDetails details) {
        if (_delegate.selectionEnabled) {
          renderEditable.selectWord(cause: SelectionChangedCause.tap);
          if (shouldShowSelectionToolbar)
            editableText.showToolbar();
        }
      }

      protected void onDragSelectionStart(DragStartDetails details) {
        renderEditable.selectPositionAt(
          from: details.globalPosition,
          cause: SelectionChangedCause.drag
        );
      }

      
      protected void onDragSelectionUpdate(DragStartDetails startDetails, DragUpdateDetails updateDetails) {
        renderEditable.selectPositionAt(
          from: startDetails.globalPosition,
          to: updateDetails.globalPosition,
          cause: SelectionChangedCause.drag
        );
      }

      protected virtual void onDragSelectionEnd(DragEndDetails details)
      {/* Subclass should override this method if needed. */}

      public Widget buildGestureDetector(
        Key key = null,
        HitTestBehavior behavior = default,
        Widget child = null) {
            GestureForcePressStartCallback PressStart = null;
            GestureForcePressEndCallback PressEnd = null;
            if (_delegate.forcePressEnabled) {
                PressStart = onForcePressStart;
                PressEnd = onForcePressEnd;
            }
            return new TextSelectionGestureDetector(
            key: key,
            onTapDown: onTapDown,
            onForcePressStart:PressStart,
            onForcePressEnd:PressEnd,
            onSingleTapUp: onSingleTapUp,
            onSingleTapCancel: onSingleTapCancel,
            onSingleLongTapStart: onSingleLongTapStart,
            onSingleLongTapMoveUpdate: onSingleLongTapMoveUpdate,
            onSingleLongTapEnd: onSingleLongTapEnd,
            onDoubleTapDown: onDoubleTapDown,
            onDragSelectionStart: onDragSelectionStart,
            onDragSelectionUpdate: onDragSelectionUpdate,
            onDragSelectionEnd: onDragSelectionEnd,
            behavior: behavior,
            child: child
            );
        }
    }

    public interface TextSelectionGestureDetectorBuilderDelegate {

        GlobalKey<EditableTextState> editableTextKey { get; }
        bool forcePressEnabled {
            get;
        }
        bool selectionEnabled { get; }
    }
    
    public class TextSelectionGestureDetector : StatefulWidget {
        public TextSelectionGestureDetector(
            Key key = null,
            GestureTapDownCallback onTapDown = null,
            GestureForcePressStartCallback onForcePressStart = null,
            GestureForcePressEndCallback onForcePressEnd = null,
            GestureTapUpCallback onSingleTapUp = null,
            GestureTapCancelCallback onSingleTapCancel = null,
            GestureLongPressStartCallback onSingleLongTapStart = null,
            GestureLongPressMoveUpdateCallback onSingleLongTapMoveUpdate = null,
            GestureLongPressEndCallback onSingleLongTapEnd = null,
            GestureTapDownCallback onDoubleTapDown = null,
            GestureDragStartCallback onDragSelectionStart = null,
            DragSelectionUpdateCallback onDragSelectionUpdate = null,
            GestureDragEndCallback onDragSelectionEnd = null,
            HitTestBehavior? behavior = null,
            Widget child = null
        ) : base(key: key) {
            D.assert(child != null);
            this.onTapDown = onTapDown;
            this.onForcePressEnd = onForcePressEnd;
            this.onForcePressStart = onForcePressStart;
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
        
        public readonly GestureForcePressStartCallback onForcePressStart;
        
        public readonly GestureForcePressEndCallback onForcePressEnd;

        public readonly GestureTapUpCallback onSingleTapUp;

        public readonly GestureTapCancelCallback onSingleTapCancel;

        public readonly GestureLongPressStartCallback onSingleLongTapStart;

        public readonly GestureLongPressMoveUpdateCallback onSingleLongTapMoveUpdate;

        public readonly GestureLongPressEndCallback onSingleLongTapEnd;

        public readonly GestureTapDownCallback onDoubleTapDown;

        public readonly GestureDragStartCallback onDragSelectionStart;

        public readonly DragSelectionUpdateCallback onDragSelectionUpdate;

        public readonly GestureDragEndCallback onDragSelectionEnd;

        public HitTestBehavior? behavior;

        public readonly Widget child;

        public override State createState() {
            return new _TextSelectionGestureDetectorState();
        }
    }

    class _TextSelectionGestureDetectorState : State<TextSelectionGestureDetector> {
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
            _dragUpdateThrottleTimer =
                _dragUpdateThrottleTimer ?? Timer.create(TextSelectionUtils._kDragSelectionUpdateThrottle, _handleDragUpdateThrottled);
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

        void _forcePressStarted(ForcePressDetails details) {
            _doubleTapTimer?.cancel();
            _doubleTapTimer = null;
            if (widget.onForcePressStart != null)
                widget.onForcePressStart(details);
        }

        void _forcePressEnded(ForcePressDetails details) {
            if (widget.onForcePressEnd != null)
                widget.onForcePressEnd(details);
        }
        void _handleLongPressStart(LongPressStartDetails details) {
            if (!_isDoubleTap && widget.onSingleLongTapStart != null) {
                widget.onSingleLongTapStart(details);
            }
        }

        void _handleLongPressMoveUpdate(LongPressMoveUpdateDetails details) {
            if (!_isDoubleTap && widget.onSingleLongTapMoveUpdate != null) {
                widget.onSingleLongTapMoveUpdate(details);
            }
        }

        void _handleLongPressEnd(LongPressEndDetails details) {
            if (!_isDoubleTap && widget.onSingleLongTapEnd != null) {
                widget.onSingleLongTapEnd(details);
            }

            _isDoubleTap = false;
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

            gestures.Add(typeof(_TransparentTapGestureRecognizer), new GestureRecognizerFactoryWithHandlers<_TransparentTapGestureRecognizer>(
                    () => new _TransparentTapGestureRecognizer(debugOwner: this),
                    (_TransparentTapGestureRecognizer instance) => {
                        instance.onTapDown = _handleTapDown;
                        instance.onTapUp = _handleTapUp;
                        instance.onTapCancel = _handleTapCancel;
                    }
                )
            );

            if (widget.onSingleLongTapStart != null ||
                widget.onSingleLongTapMoveUpdate != null ||
                widget.onSingleLongTapEnd != null
            ) {
                gestures[typeof(LongPressGestureRecognizer)] =
                    new GestureRecognizerFactoryWithHandlers<LongPressGestureRecognizer>(
                        () => new LongPressGestureRecognizer(debugOwner: this, kind: PointerDeviceKind.touch),
                        instance => {
                            instance.onLongPressStart = _handleLongPressStart;
                            instance.onLongPressMoveUpdate = _handleLongPressMoveUpdate;
                            instance.onLongPressEnd = _handleLongPressEnd;
                        });
            }

            if (widget.onDragSelectionStart != null ||
                widget.onDragSelectionUpdate != null ||
                widget.onDragSelectionEnd != null) {
                gestures.Add(typeof(HorizontalDragGestureRecognizer),
                    new GestureRecognizerFactoryWithHandlers<HorizontalDragGestureRecognizer>(
                        () => new HorizontalDragGestureRecognizer(debugOwner: this, kind: PointerDeviceKind.mouse),
                        instance => {
                            instance.dragStartBehavior = DragStartBehavior.down;
                            instance.onStart = _handleDragStart;
                            instance.onUpdate = _handleDragUpdate;
                            instance.onEnd = _handleDragEnd;
                        }
                    )
                );
            }

            if (widget.onForcePressStart != null || widget.onForcePressEnd != null) {
                GestureForcePressStartCallback startInstance = null;
                if (widget.onForcePressStart != null)
                    startInstance = _forcePressStarted;
                GestureForcePressEndCallback endInstance = null;
                if (widget.onForcePressEnd != null)
                    endInstance = _forcePressEnded;
                gestures[typeof(ForcePressGestureRecognizer)] = new GestureRecognizerFactoryWithHandlers<ForcePressGestureRecognizer>(
                    () => new ForcePressGestureRecognizer(debugOwner: this),
                    (ForcePressGestureRecognizer instance) => {
                        instance.onStart = startInstance;
                        instance.onEnd = endInstance;
                    }
                );
            }

            return new RawGestureDetector(
                gestures: gestures,
                behavior: widget.behavior,
                child: widget.child
            );
        }
    }
    public class _TransparentTapGestureRecognizer : TapGestureRecognizer {
        public _TransparentTapGestureRecognizer(Object debugOwner = default) : base(debugOwner: debugOwner) {}
        
        public override void rejectGesture(int pointer) {
            if (state == GestureRecognizerState.ready) {
                acceptGesture(pointer);
            } else {
                base.rejectGesture(pointer);
            }
        }
    }
}