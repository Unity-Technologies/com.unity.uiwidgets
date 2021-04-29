using System.Collections.Generic;
using Unity.UIWidgets.cupertino;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.gestures;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.rendering;
using Unity.UIWidgets.service;
using Unity.UIWidgets.ui;
using Unity.UIWidgets.widgets;
using StrutStyle = Unity.UIWidgets.painting.StrutStyle;
using TextStyle = Unity.UIWidgets.painting.TextStyle;

namespace Unity.UIWidgets.material {
    public delegate Widget InputCounterWidgetBuilder(
        BuildContext buildContext,
        int? currentLength,
        int? maxLength,
        bool? isFocused);

    class _TextFieldSelectionGestureDetectorBuilder : TextSelectionGestureDetectorBuilder {
        public _TextFieldSelectionGestureDetectorBuilder(
            _TextFieldState state) : base(_delegate: state) {
            _state = state;
        }

        public readonly _TextFieldState _state;

        protected override void onForcePressStart(ForcePressDetails details) {
            base.onForcePressStart(details);
            if (_delegate.selectionEnabled && shouldShowSelectionToolbar) {
                editableText.showToolbar();
            }
        }

        protected override void onForcePressEnd(ForcePressDetails details) {
            // Not required.
        }

        protected override void onSingleTapUp(TapUpDetails details) {
            editableText.hideToolbar();
            if (_delegate.selectionEnabled) {
                //use android by default
                renderEditable.selectPosition(cause: SelectionChangedCause.tap);
            }

            _state._requestKeyboard();
            if (_state.widget.onTap != null) {
                _state.widget.onTap();
            }
        }

        protected override void onSingleLongTapStart(LongPressStartDetails details) {
            if (_delegate.selectionEnabled) {
                //use android by default
                renderEditable.selectWord(cause: SelectionChangedCause.longPress);
                Feedback.forLongPress(_state.context);
            }
        }
    }

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
            TextAlignVertical textAlignVertical = null,
            TextDirection textDirection = TextDirection.ltr,
            bool readOnly = false,
            ToolbarOptions toolbarOptions = null,
            bool? showCursor = null,
            bool autofocus = false,
            bool obscureText = false,
            bool autocorrect = false,
            SmartDashesType? smartDashesType = null,
            SmartQuotesType? smartQuotesType = null,
            bool enableSuggestions = true,
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
            BoxHeightStyle selectionHeightStyle = BoxHeightStyle.tight,
            BoxWidthStyle selectionWidthStyle = BoxWidthStyle.tight,
            Brightness? keyboardAppearance = null,
            EdgeInsets scrollPadding = null,
            DragStartBehavior dragStartBehavior = DragStartBehavior.start,
            bool enableInteractiveSelection = true,
            GestureTapCallback onTap = null,
            InputCounterWidgetBuilder buildCounter = null,
            ScrollController scrollController = null,
            ScrollPhysics scrollPhysics = null
        ) : base(key: key) {
            D.assert(maxLines == null || maxLines > 0);
            D.assert(minLines == null || minLines > 0);
            D.assert((maxLines == null) || (minLines == null) || (maxLines >= minLines),
                () => "minLines can't be greater than maxLines");
            D.assert(!expands || (maxLines == null && minLines == null),
                () => "minLines and maxLines must be null when expands is true.");
            D.assert(maxLength == null || maxLength == noMaxLength || maxLength > 0);
            D.assert(!obscureText || maxLines == 1, () => "Obscured fields cannot be multiline.");

            this.smartDashesType =
                smartDashesType ?? (obscureText ? SmartDashesType.disabled : SmartDashesType.enabled);
            this.smartQuotesType =
                smartQuotesType ?? (obscureText ? SmartQuotesType.disabled : SmartQuotesType.enabled);

            this.controller = controller;
            this.focusNode = focusNode;
            this.decoration = noDecoration ? null : (decoration ?? new InputDecoration());
            this.textInputAction = textInputAction;
            this.textCapitalization = textCapitalization;
            this.style = style;
            this.strutStyle = strutStyle;
            this.textAlign = textAlign;
            this.textAlignVertical = textAlignVertical;
            this.textDirection = textDirection;
            this.readOnly = readOnly;
            this.showCursor = showCursor;
            this.autofocus = autofocus;
            this.obscureText = obscureText;
            this.autocorrect = autocorrect;
            this.enableSuggestions = enableSuggestions;
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
            this.selectionHeightStyle = selectionHeightStyle;
            this.selectionWidthStyle = selectionWidthStyle;
            this.cursorRadius = cursorRadius;
            this.onSubmitted = onSubmitted;
            this.keyboardAppearance = keyboardAppearance;
            this.enableInteractiveSelection = enableInteractiveSelection;
            this.onTap = onTap;
            this.keyboardType = keyboardType ?? (maxLines == 1 ? TextInputType.text : TextInputType.multiline);
            this.toolbarOptions = toolbarOptions ?? (obscureText
                                      ? new ToolbarOptions(
                                          selectAll: true,
                                          paste: true
                                      )
                                      : new ToolbarOptions(
                                          copy: true,
                                          cut: true,
                                          selectAll: true,
                                          paste: true
                                      ));

            this.scrollPadding = scrollPadding ?? EdgeInsets.all(20.0f);
            this.dragStartBehavior = dragStartBehavior;
            this.enableInteractiveSelection = enableInteractiveSelection;
            this.buildCounter = buildCounter;
            this.scrollPhysics = scrollPhysics;
            this.scrollController = scrollController;
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

        public readonly TextAlignVertical textAlignVertical;

        public readonly TextDirection textDirection;

        public readonly bool autofocus;

        public readonly bool obscureText;

        public readonly bool autocorrect;

        public readonly SmartDashesType smartDashesType;

        public readonly SmartQuotesType smartQuotesType;

        public readonly bool enableSuggestions;

        public readonly int? maxLines;

        public readonly int? minLines;

        public readonly bool expands;

        public readonly bool readOnly;

        public readonly ToolbarOptions toolbarOptions;

        public readonly bool? showCursor;

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

        public readonly BoxHeightStyle selectionHeightStyle;

        public readonly BoxWidthStyle selectionWidthStyle;

        public readonly Brightness? keyboardAppearance;

        public readonly EdgeInsets scrollPadding;

        public readonly bool enableInteractiveSelection;

        public readonly DragStartBehavior dragStartBehavior;

        public readonly ScrollPhysics scrollPhysics;

        public readonly ScrollController scrollController;

        public bool selectionEnabled {
            get { return enableInteractiveSelection; }
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
            properties.add(new DiagnosticsProperty<InputDecoration>("decoration", decoration,
                defaultValue: new InputDecoration()));
            properties.add(new DiagnosticsProperty<TextInputType>("keyboardType", keyboardType,
                defaultValue: TextInputType.text));
            properties.add(new DiagnosticsProperty<TextStyle>("style", style, defaultValue: null));
            properties.add(new DiagnosticsProperty<bool>("autofocus", autofocus, defaultValue: false));
            properties.add(new DiagnosticsProperty<bool>("obscureText", obscureText, defaultValue: false));
            properties.add(new DiagnosticsProperty<bool>("autocorrect", autocorrect, defaultValue: true));
            properties.add(new EnumProperty<SmartDashesType>("smartDashesType", smartDashesType,
                defaultValue: obscureText ? SmartDashesType.disabled : SmartDashesType.enabled));
            properties.add(new EnumProperty<SmartQuotesType>("smartQuotesType", smartQuotesType,
                defaultValue: obscureText ? SmartQuotesType.disabled : SmartQuotesType.enabled));
            properties.add(new DiagnosticsProperty<bool>("enableSuggestions", enableSuggestions, defaultValue: true));
            properties.add(new IntProperty("maxLines", maxLines, defaultValue: 1));
            properties.add(new IntProperty("minLines", minLines, defaultValue: null));
            properties.add(new DiagnosticsProperty<bool>("expands", expands, defaultValue: false));
            properties.add(new IntProperty("maxLength", maxLength, defaultValue: null));
            properties.add(new FlagProperty("maxLengthEnforced", value: maxLengthEnforced, defaultValue: true,
                ifFalse: "maxLength not enforced"));
            properties.add(new EnumProperty<TextInputAction?>("textInputAction", textInputAction, defaultValue: null));
            properties.add(new EnumProperty<TextCapitalization>("textCapitalization", textCapitalization,
                defaultValue: TextCapitalization.none));
            properties.add(new EnumProperty<TextAlign>("textAlign", textAlign, defaultValue: TextAlign.left));
            properties.add(new DiagnosticsProperty<TextAlignVertical>("textAlignVertical", textAlignVertical,
                defaultValue: null));
            properties.add(new EnumProperty<TextDirection>("textDirection", textDirection, defaultValue: null));
            properties.add(new FloatProperty("cursorWidth", cursorWidth, defaultValue: 2.0f));
            properties.add(new DiagnosticsProperty<Radius>("cursorRadius", cursorRadius, defaultValue: null));
            properties.add(new ColorProperty("cursorColor", cursorColor, defaultValue: null));
            properties.add(new DiagnosticsProperty<Brightness?>("keyboardAppearance", keyboardAppearance,
                defaultValue: null));
            properties.add(new DiagnosticsProperty<EdgeInsetsGeometry>("scrollPadding", scrollPadding,
                defaultValue: EdgeInsets.all(20.0f)));
            properties.add(new FlagProperty("selectionEnabled", value: selectionEnabled, defaultValue: true,
                ifFalse: "selection disabled"));
            properties.add(
                new DiagnosticsProperty<ScrollController>("scrollController", scrollController, defaultValue: null));
            properties.add(new DiagnosticsProperty<ScrollPhysics>("scrollPhysics", scrollPhysics, defaultValue: null));
        }
    }

    class _TextFieldState : State<TextField>, TextSelectionGestureDetectorBuilderDelegate {
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

        bool _isHovering = false;

        bool needsCounter {
            get {
                return widget.maxLength != null
                       && widget.decoration != null
                       && widget.decoration.counterText == null;
            }
        }

        bool _showSelectionHandles = false;

        _TextFieldSelectionGestureDetectorBuilder _selectionGestureDetectorBuilder;

        bool _forcePressEnabled;

        public bool forcePressEnabled {
            get { return _forcePressEnabled; }
        }

        readonly GlobalKey<EditableTextState> _editableTextKey = new LabeledGlobalKey<EditableTextState>();

        public GlobalKey<EditableTextState> editableTextKey {
            get { return _editableTextKey; }
        }

        public bool selectionEnabled {
            get { return widget.selectionEnabled; }
        }

        bool _isEnabled {
            get { return widget.enabled ?? widget.decoration?.enabled ?? true; }
        }

        int _currentLength {
            get { return _effectiveController.value.text.Length; }
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

            Widget counter = null;
            int currentLength = _currentLength;
            if (effectiveDecoration.counter == null
                && effectiveDecoration.counterText == null
                && widget.buildCounter != null) {
                bool isFocused = _effectiveFocusNode.hasFocus;
                Widget builtCounter = widget.buildCounter(
                    context,
                    currentLength: currentLength,
                    maxLength: widget.maxLength,
                    isFocused: isFocused
                );
                if (builtCounter != null) {
                    counter = builtCounter;
                }

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

            return effectiveDecoration.copyWith(
                counterText: counterText
            );
        }

        public override void initState() {
            base.initState();
            _selectionGestureDetectorBuilder = new _TextFieldSelectionGestureDetectorBuilder(state: this);
            if (widget.controller == null) {
                _controller = new TextEditingController();
            }

            _effectiveFocusNode.canRequestFocus = _isEnabled;
        }

        public override void didUpdateWidget(StatefulWidget oldWidget) {
            base.didUpdateWidget(oldWidget);
            if (widget.controller == null && ((TextField) oldWidget).controller != null) {
                _controller = TextEditingController.fromValue(((TextField) oldWidget).controller.value);
            }
            else if (widget.controller != null && ((TextField) oldWidget).controller == null) {
                _controller = null;
            }

            _effectiveFocusNode.canRequestFocus = _isEnabled;
            if (_effectiveFocusNode.hasFocus && widget.readOnly != ((TextField) oldWidget).readOnly) {
                if (_effectiveController.selection.isCollapsed) {
                    _showSelectionHandles = !widget.readOnly;
                }
            }
        }

        public override void dispose() {
            _focusNode?.dispose();
            base.dispose();
        }

        EditableTextState _editableText {
            get { return editableTextKey.currentState; }
        }

        internal void _requestKeyboard() {
            _editableText?.requestKeyboard();
        }

        bool _shouldShowSelectionHandles(SelectionChangedCause cause) {
            if (!_selectionGestureDetectorBuilder.shouldShowSelectionToolbar) {
                return false;
            }

            if (cause == SelectionChangedCause.keyboard) {
                return false;
            }

            if (widget.readOnly && _effectiveController.selection.isCollapsed) {
                return false;
            }

            if (cause == SelectionChangedCause.longPress) {
                return true;
            }

            if (_effectiveController.text.isNotEmpty()) {
                return true;
            }

            return false;
        }

        void _handleSelectionChanged(TextSelection selection, SelectionChangedCause cause) {
            bool willShowSelectionHandles = _shouldShowSelectionHandles(cause);
            if (willShowSelectionHandles != _showSelectionHandles) {
                setState(() => { _showSelectionHandles = willShowSelectionHandles; });
            }

            //use the code path for android by default
            return;
        }

        void _handleSelectionHandleTapped() {
            if (_effectiveController.selection.isCollapsed) {
                _editableText.toggleToolbar();
            }
        }

        void _handleHover(bool hovering) {
            if (hovering != _isHovering) {
                setState(() => { _isHovering = hovering; });
            }
        }

        public override Widget build(BuildContext context) {
            D.assert(material_.debugCheckHasMaterial(context));
            D.assert(WidgetsD.debugCheckHasDirectionality(context));
            D.assert(
                !(widget.style != null && widget.style.inherit == false &&
                  (widget.style.fontSize == null || widget.style.textBaseline == null)),
                () => "inherit false style must supply fontSize and textBaseline"
            );
            ThemeData themeData = Theme.of(context);
            TextStyle style = themeData.textTheme.subtitle1.merge(widget.style);
            Brightness keyboardAppearance = widget.keyboardAppearance ?? themeData.primaryColorBrightness;
            TextEditingController controller = _effectiveController;
            FocusNode focusNode = _effectiveFocusNode;
            List<TextInputFormatter> formatters = widget.inputFormatters ?? new List<TextInputFormatter>();
            if (widget.maxLength != null && widget.maxLengthEnforced) {
                formatters.Add(new LengthLimitingTextInputFormatter(widget.maxLength));
            }

            TextSelectionControls textSelectionControls = MaterialUtils.materialTextSelectionControls;
            ;
            bool paintCursorAboveText = false;
            bool cursorOpacityAnimates = false;
            Offset cursorOffset = null;
            Color cursorColor = widget.cursorColor ?? themeData.cursorColor;
            Radius cursorRadius = widget.cursorRadius;

            _forcePressEnabled = false;
            textSelectionControls = _MaterialTextSelectionControls.materialTextSelectionControls;
            paintCursorAboveText = false;
            cursorOpacityAnimates = false;
            cursorColor = cursorColor ?? themeData.cursorColor;

            Widget child = new RepaintBoundary(
                child: new EditableText(
                    key: editableTextKey,
                    readOnly: widget.readOnly,
                    toolbarOptions: widget.toolbarOptions,
                    showCursor: widget.showCursor,
                    showSelectionHandles: _showSelectionHandles,
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
                    smartDashesType: widget.smartDashesType,
                    smartQuotesType: widget.smartQuotesType,
                    enableSuggestions: widget.enableSuggestions,
                    maxLines: widget.maxLines,
                    minLines: widget.minLines,
                    expands: widget.expands,
                    selectionColor: themeData.textSelectionColor,
                    selectionControls: widget.selectionEnabled ? textSelectionControls : null,
                    onChanged: widget.onChanged,
                    onSelectionChanged: _handleSelectionChanged,
                    onEditingComplete: widget.onEditingComplete,
                    onSubmitted: widget.onSubmitted,
                    onSelectionHandleTapped: _handleSelectionHandleTapped,
                    inputFormatters: formatters,
                    rendererIgnoresPointer: true,
                    cursorWidth: widget.cursorWidth.Value,
                    cursorRadius: cursorRadius,
                    cursorColor: cursorColor,
                    selectionHeightStyle: widget.selectionHeightStyle,
                    selectionWidthStyle: widget.selectionWidthStyle,
                    cursorOpacityAnimates: cursorOpacityAnimates,
                    cursorOffset: cursorOffset,
                    paintCursorAboveText: paintCursorAboveText,
                    backgroundCursorColor: CupertinoColors.inactiveGray,
                    scrollPadding: widget.scrollPadding,
                    keyboardAppearance: keyboardAppearance,
                    enableInteractiveSelection: widget.enableInteractiveSelection == true,
                    dragStartBehavior: widget.dragStartBehavior,
                    scrollController: widget.scrollController,
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
                            textAlignVertical: widget.textAlignVertical,
                            isHovering: _isHovering,
                            isFocused: focusNode.hasFocus,
                            isEmpty: controller.value.text.isEmpty(),
                            expands: widget.expands,
                            child: _child
                        );
                    },
                    child: child
                );
            }

            void onEnter(PointerEnterEvent pEvent) {
                _handleHover(true);
            }

            void onExit(PointerExitEvent pEvent) {
                _handleHover(false);
            }

            return new IgnorePointer(
                ignoring: !_isEnabled,
                child: new MouseRegion(
                    onEnter: onEnter,
                    onExit: onExit,
                    child: new AnimatedBuilder(
                        animation: controller,
                        builder: (BuildContext buildContext, Widget buildChild) => { return buildChild; },
                        child: _selectionGestureDetectorBuilder.buildGestureDetector(
                            behavior: HitTestBehavior.translucent,
                            child: child
                        )
                    )
                )
            );
        }
    }
}