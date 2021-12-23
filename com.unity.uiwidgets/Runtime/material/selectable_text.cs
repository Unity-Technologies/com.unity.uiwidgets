using System.Collections.Generic;
using Unity.UIWidgets.cupertino;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.gestures;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.rendering;
using Unity.UIWidgets.service;
using Unity.UIWidgets.ui;
using Unity.UIWidgets.widgets;
using UnityEngine;
using Color = Unity.UIWidgets.ui.Color;
using StrutStyle = Unity.UIWidgets.painting.StrutStyle;
using TextStyle = Unity.UIWidgets.painting.TextStyle;

namespace Unity.UIWidgets.material {
    public static class SelectableTextUtils {
        internal const int iOSHorizontalOffset = -2;
    }

    class _TextSpanEditingController : TextEditingController {
        public _TextSpanEditingController(TextSpan textSpan = null) : base(text: textSpan?.toPlainText()) {
            D.assert(textSpan != null);
            _textSpan = textSpan;
        }

        public readonly TextSpan _textSpan;

        public override TextSpan buildTextSpan(TextStyle style = null, bool withComposing = false) {
            return new TextSpan(
                style: style,
                children: new List<InlineSpan> {_textSpan}
            );
        }

        public new string text {
            get { return value.text; }
            set { }
        }
    }

    class _SelectableTextSelectionGestureDetectorBuilder : TextSelectionGestureDetectorBuilder {
        public _SelectableTextSelectionGestureDetectorBuilder(
            _SelectableTextState state
        ) : base(_delegate: state) {
            _state = state;
        }

        public readonly _SelectableTextState _state;

        protected override void onForcePressStart(ForcePressDetails details) {
            base.onForcePressStart(details);
            if (_delegate.selectionEnabled && shouldShowSelectionToolbar) {
                editableText.showToolbar();
            }
        }

        protected override void onForcePressEnd(ForcePressDetails details) {
            // Not required.
        }

        protected override void onSingleLongTapMoveUpdate(LongPressMoveUpdateDetails details) {
            if (_delegate.selectionEnabled) {
                switch (Theme.of(_state.context).platform) {
                    case RuntimePlatform.IPhonePlayer:
                    case RuntimePlatform.OSXEditor:
                    case RuntimePlatform.OSXPlayer:
                        renderEditable.selectPositionAt(
                            from: details.globalPosition,
                            cause: SelectionChangedCause.longPress
                        );
                        break;
                    default:
                        renderEditable.selectWordsInRange(
                            from: details.globalPosition - details.offsetFromOrigin,
                            to: details.globalPosition,
                            cause: SelectionChangedCause.longPress
                        );
                        break;
                }
            }
        }

        protected override void onSingleTapUp(TapUpDetails details) {
            editableText.hideToolbar();
            if (_delegate.selectionEnabled) {
                switch (Theme.of(_state.context).platform) {
                    case RuntimePlatform.IPhonePlayer:
                    case RuntimePlatform.OSXEditor:
                    case RuntimePlatform.OSXPlayer:
                        renderEditable.selectWordEdge(cause: SelectionChangedCause.tap);
                        break;
                    default:
                        renderEditable.selectPosition(cause: SelectionChangedCause.tap);
                        break;
                }
            }

            if (_state.widget.onTap != null)
                _state.widget.onTap();
        }

        protected override void onSingleLongTapStart(LongPressStartDetails details) {
            if (_delegate.selectionEnabled) {
                switch (Theme.of(_state.context).platform) {
                    case RuntimePlatform.IPhonePlayer:
                    case RuntimePlatform.OSXEditor:
                    case RuntimePlatform.OSXPlayer:
                        renderEditable.selectPositionAt(
                            from: details.globalPosition,
                            cause: SelectionChangedCause.longPress
                        );
                        break;
                    default:
                        renderEditable.selectWord(cause: SelectionChangedCause.longPress);
                        Feedback.forLongPress(_state.context);
                        break;
                }
            }
        }
    }

    public class SelectableText : StatefulWidget {
        public SelectableText(
            string data,
            Key key = null,
            FocusNode focusNode = null,
            TextStyle style = null,
            StrutStyle strutStyle = null,
            TextAlign? textAlign = null,
            TextDirection? textDirection = null,
            float? textScaleFactor = null,
            bool showCursor = false,
            bool autofocus = false,
            ToolbarOptions toolbarOptions = null,
            int? minLines = null,
            int? maxLines = null,
            float cursorWidth = 2.0f,
            Radius cursorRadius = null,
            Color cursorColor = null,
            DragStartBehavior dragStartBehavior = DragStartBehavior.start,
            bool enableInteractiveSelection = true,
            GestureTapCallback onTap = null,
            ScrollPhysics scrollPhysics = null,
            TextWidthBasis? textWidthBasis = null
        ) : this(
            data: data,
            textSpan: null,
            key: key,
            focusNode: focusNode,
            style: style,
            strutStyle: strutStyle,
            textAlign: textAlign,
            textDirection: textDirection,
            textScaleFactor: textScaleFactor,
            showCursor: showCursor,
            autofocus: autofocus,
            toolbarOptions: toolbarOptions,
            minLines: minLines,
            maxLines: maxLines,
            cursorWidth: cursorWidth,
            cursorRadius: cursorRadius,
            cursorColor: cursorColor,
            dragStartBehavior: dragStartBehavior,
            enableInteractiveSelection: enableInteractiveSelection,
            onTap: onTap,
            scrollPhysics: scrollPhysics,
            textWidthBasis: textWidthBasis
        ) {
            D.assert(
                data != null,
                () => "A non-null String must be provided to a SelectableText widget."
            );
        }

        SelectableText(
            string data,
            TextSpan textSpan,
            Key key = null,
            FocusNode focusNode = null,
            TextStyle style = null,
            StrutStyle strutStyle = null,
            TextAlign? textAlign = null,
            TextDirection? textDirection = null,
            float? textScaleFactor = null,
            bool showCursor = false,
            bool autofocus = false,
            ToolbarOptions toolbarOptions = null,
            int? minLines = null,
            int? maxLines = null,
            float cursorWidth = 2.0f,
            Radius cursorRadius = null,
            Color cursorColor = null,
            DragStartBehavior dragStartBehavior = DragStartBehavior.start,
            bool enableInteractiveSelection = true,
            GestureTapCallback onTap = null,
            ScrollPhysics scrollPhysics = null,
            TextWidthBasis? textWidthBasis = null
        ) : base(key: key) {
            D.assert(maxLines == null || maxLines > 0);
            D.assert(minLines == null || minLines > 0);
            D.assert(
                (maxLines == null) || (minLines == null) || (maxLines >= minLines),
                () => "minLines can\'t be greater than maxLines"
            );
            toolbarOptions = toolbarOptions ??
                             new ToolbarOptions(
                                 selectAll: true,
                                 copy: true
                             );

            this.data = data;
            this.textSpan = textSpan;
            this.focusNode = focusNode;
            this.style = style;
            this.strutStyle = strutStyle;
            this.textAlign = textAlign;
            this.textDirection = textDirection;
            this.textScaleFactor = textScaleFactor;
            this.showCursor = showCursor;
            this.autofocus = autofocus;
            this.toolbarOptions = toolbarOptions;
            this.minLines = minLines;
            this.maxLines = maxLines;
            this.cursorWidth = cursorWidth;
            this.cursorRadius = cursorRadius;
            this.cursorColor = cursorColor;
            this.dragStartBehavior = dragStartBehavior;
            this.enableInteractiveSelection = enableInteractiveSelection;
            this.onTap = onTap;
            this.scrollPhysics = scrollPhysics;
            this.textWidthBasis = textWidthBasis;
        }

        public static SelectableText rich(
            TextSpan textSpan,
            Key key = null,
            FocusNode focusNode = null,
            TextStyle style = null,
            StrutStyle strutStyle = null,
            TextAlign? textAlign = null,
            TextDirection? textDirection = null,
            float? textScaleFactor = null,
            bool showCursor = false,
            bool autofocus = false,
            ToolbarOptions toolbarOptions = null,
            int? minLines = null,
            int? maxLines = null,
            float cursorWidth = 2.0f,
            Radius cursorRadius = null,
            Color cursorColor = null,
            DragStartBehavior dragStartBehavior = DragStartBehavior.start,
            bool enableInteractiveSelection = true,
            GestureTapCallback onTap = null,
            ScrollPhysics scrollPhysics = null,
            TextWidthBasis? textWidthBasis = null
        ) {
            D.assert(
                textSpan != null,
                () => "A non-null TextSpan must be provided to a SelectableText.rich widget."
            );

            return new SelectableText(
                data: null,
                textSpan: textSpan,
                key: key,
                focusNode: focusNode,
                style: style,
                strutStyle: strutStyle,
                textAlign: textAlign,
                textDirection: textDirection,
                textScaleFactor: textScaleFactor,
                showCursor: showCursor,
                autofocus: autofocus,
                toolbarOptions: toolbarOptions,
                minLines: minLines,
                maxLines: maxLines,
                cursorWidth: cursorWidth,
                cursorRadius: cursorRadius,
                cursorColor: cursorColor,
                dragStartBehavior: dragStartBehavior,
                enableInteractiveSelection: enableInteractiveSelection,
                onTap: onTap,
                scrollPhysics: scrollPhysics,
                textWidthBasis: textWidthBasis
            );
        }

        public readonly string data;

        public readonly TextSpan textSpan;

        public readonly FocusNode focusNode;

        public readonly TextStyle style;

        public readonly StrutStyle strutStyle;

        public readonly TextAlign? textAlign;

        public readonly TextDirection? textDirection;

        public readonly float? textScaleFactor;


        public readonly bool autofocus;


        public readonly int? minLines;


        public readonly int? maxLines;


        public readonly bool showCursor;


        public readonly float cursorWidth;


        public readonly Radius cursorRadius;

        public readonly Color cursorColor;


        public readonly bool enableInteractiveSelection;


        public readonly DragStartBehavior dragStartBehavior;

        public readonly ToolbarOptions toolbarOptions;


        internal bool selectionEnabled {
            get { return enableInteractiveSelection; }
        }

        public readonly GestureTapCallback onTap;


        public readonly ScrollPhysics scrollPhysics;


        public readonly TextWidthBasis? textWidthBasis;

        public override State createState() => new _SelectableTextState();

        public override void debugFillProperties(DiagnosticPropertiesBuilder properties) {
            base.debugFillProperties(properties);
            properties.add(new DiagnosticsProperty<string>("data", data, defaultValue: null));
            properties.add(new DiagnosticsProperty<FocusNode>("focusNode", focusNode, defaultValue: null));
            properties.add(new DiagnosticsProperty<TextStyle>("style", style, defaultValue: null));
            properties.add(new DiagnosticsProperty<bool>("autofocus", autofocus, defaultValue: false));
            properties.add(new DiagnosticsProperty<bool>("showCursor", showCursor, defaultValue: false));
            properties.add(new IntProperty("minLines", minLines, defaultValue: null));
            properties.add(new IntProperty("maxLines", maxLines, defaultValue: null));
            properties.add(new EnumProperty<TextAlign?>("textAlign", textAlign, defaultValue: null));
            properties.add(new EnumProperty<TextDirection?>("textDirection", textDirection, defaultValue: null));
            properties.add(new FloatProperty("textScaleFactor", textScaleFactor, defaultValue: null));
            properties.add(new FloatProperty("cursorWidth", cursorWidth, defaultValue: 2.0f));
            properties.add(new DiagnosticsProperty<Radius>("cursorRadius", cursorRadius, defaultValue: null));
            properties.add(new DiagnosticsProperty<Color>("cursorColor", cursorColor, defaultValue: null));
            properties.add(new FlagProperty("selectionEnabled", value: selectionEnabled, defaultValue: true,
                ifFalse: "selection disabled"));
            properties.add(new DiagnosticsProperty<ScrollPhysics>("scrollPhysics", scrollPhysics, defaultValue: null));
        }
    }

    class _SelectableTextState : AutomaticKeepAliveClientMixin<SelectableText>,
        TextSelectionGestureDetectorBuilderDelegate {
        EditableTextState _editableText => editableTextKey.currentState;

        _TextSpanEditingController _controller;

        FocusNode _focusNode;
        FocusNode _effectiveFocusNode => widget.focusNode ?? (_focusNode = _focusNode ?? new FocusNode());

        bool _showSelectionHandles = false;

        _SelectableTextSelectionGestureDetectorBuilder _selectionGestureDetectorBuilder;

        public bool forcePressEnabled {
            get { return _forcePressEnabled; }
            set { _forcePressEnabled = value; }
        }

        bool _forcePressEnabled;

        public GlobalKey<EditableTextState> editableTextKey {
            get { return _editableTextKey; }
        }

        readonly GlobalKey<EditableTextState> _editableTextKey = GlobalKey<EditableTextState>.key();

        public bool selectionEnabled => widget.selectionEnabled;

        public override void initState() {
            base.initState();
            _selectionGestureDetectorBuilder = new _SelectableTextSelectionGestureDetectorBuilder(state: this);
            _controller = new _TextSpanEditingController(
                textSpan: widget.textSpan ?? new TextSpan(text: widget.data)
            );
        }

        public override void didUpdateWidget(StatefulWidget oldWidget) {
            var _oldWidget = (SelectableText) oldWidget;
            base.didUpdateWidget(oldWidget);
            if (widget.data != _oldWidget.data || widget.textSpan != _oldWidget.textSpan) {
                _controller = new _TextSpanEditingController(
                    textSpan: widget.textSpan ?? new TextSpan(text: widget.data)
                );
            }

            if (_effectiveFocusNode.hasFocus && _controller.selection.isCollapsed) {
                _showSelectionHandles = false;
            }
        }

        public override void dispose() {
            _focusNode?.dispose();
            base.dispose();
        }

        void _handleSelectionChanged(TextSelection selection, SelectionChangedCause cause) {
            bool willShowSelectionHandles = _shouldShowSelectionHandles(cause);
            if (willShowSelectionHandles != _showSelectionHandles) {
                setState(() => { _showSelectionHandles = willShowSelectionHandles; });
            }

            switch (Theme.of(context).platform) {
                case RuntimePlatform.IPhonePlayer:
                case RuntimePlatform.OSXEditor:
                case RuntimePlatform.OSXPlayer:
                    if (cause == SelectionChangedCause.longPress) {
                        _editableText?.bringIntoView(selection.basePos);
                    }

                    return;
                default:
                    // Do nothing.
                    break;
            }
        }

        void _handleSelectionHandleTapped() {
            if (_controller.selection.isCollapsed) {
                _editableText.toggleToolbar();
            }
        }

        bool _shouldShowSelectionHandles(SelectionChangedCause cause) {
            if (!_selectionGestureDetectorBuilder.shouldShowSelectionToolbar)
                return false;

            if (_controller.selection.isCollapsed)
                return false;

            if (cause == SelectionChangedCause.keyboard)
                return false;

            if (cause == SelectionChangedCause.longPress)
                return true;

            if (_controller.text.isNotEmpty())
                return true;

            return false;
        }

        protected override bool wantKeepAlive => true;

        public override Widget build(BuildContext context) {
            base.build(context);
            D.assert(() => { return _controller._textSpan.visitChildren((InlineSpan span) => span is TextSpan); },
                () => "SelectableText only supports TextSpan; Other type of InlineSpan is not allowed");
            D.assert(WidgetsD.debugCheckHasMediaQuery(context));
            D.assert(WidgetsD.debugCheckHasDirectionality(context));
            D.assert(
                !(widget.style != null && widget.style.inherit == false &&
                  (widget.style.fontSize == null || widget.style.textBaseline == null)),
                () => "inherit false style must supply fontSize and textBaseline"
            );

            ThemeData themeData = Theme.of(context);
            FocusNode focusNode = _effectiveFocusNode;

            TextSelectionControls textSelectionControls;
            bool paintCursorAboveText;
            bool cursorOpacityAnimates;
            Offset cursorOffset = Offset.zero;
            Color cursorColor = widget.cursorColor;
            Radius cursorRadius = widget.cursorRadius;

            switch (themeData.platform) {
                case RuntimePlatform.IPhonePlayer:
                case RuntimePlatform.OSXEditor:
                case RuntimePlatform.OSXPlayer:
                    forcePressEnabled = true;
                    textSelectionControls = CupertinoTextFieldUtils.cupertinoTextSelectionControls;
                    paintCursorAboveText = true;
                    cursorOpacityAnimates = true;
                    cursorColor = cursorColor ?? CupertinoTheme.of(context).primaryColor;
                    cursorRadius = cursorRadius ?? Radius.circular(2.0f);
                    cursorOffset =
                        new Offset(SelectableTextUtils.iOSHorizontalOffset / MediaQuery.of(context).devicePixelRatio,
                            0);
                    break;

                default:
                    forcePressEnabled = false;
                    textSelectionControls = _MaterialTextSelectionControls.materialTextSelectionControls;
                    paintCursorAboveText = false;
                    cursorOpacityAnimates = false;
                    cursorColor = cursorColor ?? themeData.cursorColor;
                    break;
            }

            DefaultTextStyle defaultTextStyle = DefaultTextStyle.of(context);
            TextStyle effectiveTextStyle = widget.style;
            if (widget.style == null || widget.style.inherit)
                effectiveTextStyle = defaultTextStyle.style.merge(widget.style);
            if (MediaQuery.boldTextOverride(context))
                effectiveTextStyle = effectiveTextStyle.merge(new TextStyle(fontWeight: FontWeight.bold));

            Widget child = new RepaintBoundary(
                child: new EditableText(
                    key: editableTextKey,
                    style: effectiveTextStyle,
                    readOnly: true,
                    textWidthBasis: widget.textWidthBasis ?? defaultTextStyle.textWidthBasis,
                    showSelectionHandles: _showSelectionHandles,
                    showCursor: widget.showCursor,
                    controller: _controller,
                    focusNode: focusNode,
                    strutStyle: widget.strutStyle ?? new StrutStyle(),
                    textAlign: widget.textAlign ?? defaultTextStyle.textAlign ?? TextAlign.start,
                    textDirection: widget.textDirection,
                    textScaleFactor: widget.textScaleFactor,
                    autofocus: widget.autofocus,
                    forceLine: false,
                    toolbarOptions: widget.toolbarOptions,
                    minLines: widget.minLines,
                    maxLines: widget.maxLines ?? defaultTextStyle.maxLines,
                    selectionColor: themeData.textSelectionColor,
                    selectionControls: widget.selectionEnabled ? textSelectionControls : null,
                    onSelectionChanged: _handleSelectionChanged,
                    onSelectionHandleTapped: _handleSelectionHandleTapped,
                    rendererIgnoresPointer: true,
                    cursorWidth: widget.cursorWidth,
                    cursorRadius: cursorRadius,
                    cursorColor: cursorColor,
                    cursorOpacityAnimates: cursorOpacityAnimates,
                    cursorOffset: cursorOffset,
                    paintCursorAboveText: paintCursorAboveText,
                    backgroundCursorColor: CupertinoColors.inactiveGray,
                    enableInteractiveSelection: widget.enableInteractiveSelection,
                    dragStartBehavior: widget.dragStartBehavior,
                    scrollPhysics: widget.scrollPhysics
                )
            );

            return _selectionGestureDetectorBuilder.buildGestureDetector(
                behavior: HitTestBehavior.translucent,
                child: child
            );
        }
    }
}