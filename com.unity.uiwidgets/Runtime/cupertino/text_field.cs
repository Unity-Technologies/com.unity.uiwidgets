using System.Collections.Generic;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.gestures;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.rendering;
using Unity.UIWidgets.service;
using Unity.UIWidgets.ui;
using Unity.UIWidgets.widgets;
using UnityEngine;
using StrutStyle = Unity.UIWidgets.painting.StrutStyle;
using TextStyle = Unity.UIWidgets.painting.TextStyle;
using Brightness = Unity.UIWidgets.ui.Brightness;
using Color = Unity.UIWidgets.ui.Color;

namespace Unity.UIWidgets.cupertino {
    class CupertinoTextFieldUtils {
        public static readonly BorderSide _kDefaultRoundedBorderSide = new BorderSide(
            color: CupertinoDynamicColor.withBrightness(
                color: new Color(0x33000000),
                darkColor: new Color(0x33FFFFFF)
            ),
            style: BorderStyle.solid,
            width: 0.0f
        );
        public static readonly Border _kDefaultRoundedBorder = new Border(
            top: _kDefaultRoundedBorderSide,
            bottom: _kDefaultRoundedBorderSide,
            left: _kDefaultRoundedBorderSide,
            right: _kDefaultRoundedBorderSide
        );

        public static readonly BoxDecoration _kDefaultRoundedBorderDecoration = new BoxDecoration(
            color: CupertinoDynamicColor.withBrightness(
                color: CupertinoColors.white,
                darkColor: CupertinoColors.black
            ),
            border: _kDefaultRoundedBorder,
            borderRadius: BorderRadius.all(Radius.circular(5.0f))
        );

        public static readonly Color _kDisabledBackground = CupertinoDynamicColor.withBrightness(
            color: new Color(0xFFFAFAFA),
            darkColor: new Color(0xFF050505)
        );


        public static readonly CupertinoDynamicColor _kClearButtonColor = CupertinoDynamicColor.withBrightness(
            color: new Color(0xFF636366),
            darkColor: new Color(0xFFAEAEB2)
        );

      

        public const int _iOSHorizontalCursorOffsetPixels = -2;
        
        public static TextSelectionControls cupertinoTextSelectionControls = new _CupertinoTextSelectionControls();
        
    }

    public enum OverlayVisibilityMode {
        never,
        editing,
        notEditing,
        always
    }
    
    public class _CupertinoTextFieldSelectionGestureDetectorBuilder : TextSelectionGestureDetectorBuilder { 
        public _CupertinoTextFieldSelectionGestureDetectorBuilder(
            _CupertinoTextFieldState state = null) : base(_delegate: state) {
            _state = state;
        }

        public readonly _CupertinoTextFieldState _state;

        protected override void onSingleTapUp(TapUpDetails details) {
            if (_state._clearGlobalKey.currentContext != null) {
                RenderBox renderBox = _state._clearGlobalKey.currentContext.findRenderObject() as RenderBox;
                Offset localOffset = renderBox.globalToLocal(details.globalPosition);
                if (renderBox.hitTest(new BoxHitTestResult(), position: localOffset)) {
                    return;
                }
            }
            base.onSingleTapUp(details);
            _state._requestKeyboard();
            if (_state.widget.onTap != null)
                _state.widget.onTap();
        }

        
        protected  override void onDragSelectionEnd(DragEndDetails details) {
            //(DragStartDetails details)
            _state._requestKeyboard();
        }
    }


    public class CupertinoTextField : StatefulWidget {
        public CupertinoTextField(
            Key key = null,
            TextEditingController controller = null,
            FocusNode focusNode = null,
            BoxDecoration decoration = null,
            EdgeInsets padding = null,
            string placeholder = null,
            TextStyle placeholderStyle = null,
            Widget prefix = null,
            OverlayVisibilityMode prefixMode = OverlayVisibilityMode.always,
            Widget suffix = null,
            OverlayVisibilityMode suffixMode = OverlayVisibilityMode.always,
            OverlayVisibilityMode clearButtonMode = OverlayVisibilityMode.never,
            TextInputType keyboardType = null,
            TextInputAction? textInputAction = null,
            TextCapitalization textCapitalization = TextCapitalization.none,
            TextStyle style = null,
            StrutStyle strutStyle = null,
            TextAlign textAlign = TextAlign.left,
            TextAlignVertical textAlignVertical = null,
            bool readOnly = false,
            ToolbarOptions toolbarOptions = null,
            bool? showCursor = null,
            bool autofocus = false,
            bool obscureText = false,
            bool autocorrect = true,
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
            float cursorWidth = 2.0f,
            Radius cursorRadius = null,
            Color cursorColor = null,
            ui.BoxHeightStyle selectionHeightStyle = ui.BoxHeightStyle.tight,
            ui.BoxWidthStyle selectionWidthStyle = ui.BoxWidthStyle.tight,
            Brightness? keyboardAppearance = null,
            EdgeInsets scrollPadding = null,
            DragStartBehavior dragStartBehavior = DragStartBehavior.start,
            bool enableInteractiveSelection = true,
            GestureTapCallback onTap = null,
            ScrollController scrollController = null,
            ScrollPhysics scrollPhysics = null
            ) : base(key: key) {
            this.smartDashesType = smartDashesType ?? (obscureText ? SmartDashesType.disabled : SmartDashesType.enabled);
            this.smartQuotesType = smartQuotesType ?? (obscureText ? SmartQuotesType.disabled : SmartQuotesType.enabled);
            D.assert(maxLines == null || maxLines > 0);
            D.assert(minLines == null || minLines > 0);
            D.assert(maxLines == null || minLines == null || maxLines >= minLines,
                () => "minLines can't be greater than maxLines");
            D.assert(!expands || (maxLines == null && minLines == null),
                () => "minLines and maxLines must be null when expands is true.");
            D.assert(maxLength == null || maxLength > 0);

            this.controller = controller;
            this.focusNode = focusNode;
            this.decoration = decoration ?? CupertinoTextFieldUtils._kDefaultRoundedBorderDecoration;
            this.padding = padding ?? EdgeInsets.all(6.0f);
            this.placeholder = placeholder;
            this.placeholderStyle = placeholderStyle ?? new TextStyle(
                fontWeight: FontWeight.w400,
                color: CupertinoColors.placeholderText
                                    );
            this.prefix = prefix;
            this.prefixMode = prefixMode;
            this.suffix = suffix;
            this.suffixMode = suffixMode;
            this.clearButtonMode = clearButtonMode;
            this.textInputAction = textInputAction;
            this.textCapitalization = textCapitalization;
            this.style = style;
            this.strutStyle = strutStyle;
            this.textAlign = textAlign;
            this.textAlignVertical = textAlignVertical;
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
            this.cursorRadius = cursorRadius ?? Radius.circular(2.0f);
            this.cursorColor = cursorColor;
            this.selectionHeightStyle = selectionHeightStyle;
            this.selectionWidthStyle = selectionWidthStyle;
            this.keyboardAppearance = keyboardAppearance;
            this.scrollPadding = scrollPadding ?? EdgeInsets.all(20.0f);
            this.dragStartBehavior = dragStartBehavior ;
            this.enableInteractiveSelection = enableInteractiveSelection;
            this.scrollPhysics = scrollPhysics;
            this.onTap = onTap;
            this.scrollController = scrollController;
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
        }

        public readonly TextEditingController controller;

        public readonly FocusNode focusNode;
        
        public readonly ToolbarOptions toolbarOptions;

        public readonly BoxDecoration decoration;

        public readonly EdgeInsetsGeometry padding;

        public readonly string placeholder;

        public readonly TextStyle placeholderStyle;

        public readonly Widget prefix;

        public readonly OverlayVisibilityMode prefixMode;

        public readonly Widget suffix;

        public readonly OverlayVisibilityMode suffixMode;

        public readonly OverlayVisibilityMode clearButtonMode;

        public readonly TextInputType keyboardType;

        public readonly TextInputAction? textInputAction;

        public readonly TextCapitalization textCapitalization;

        public readonly TextStyle style;

        public readonly StrutStyle strutStyle;

        public readonly TextAlign textAlign;

        public readonly bool autofocus;

        public readonly bool obscureText;

        public readonly bool autocorrect;

        public readonly int? maxLines;

        public readonly int? minLines;

        public readonly bool expands;

        public readonly int? maxLength;

        public readonly bool maxLengthEnforced;

        public readonly ValueChanged<string> onChanged;

        public readonly VoidCallback onEditingComplete;

        public readonly ValueChanged<string> onSubmitted;

        public readonly List<TextInputFormatter> inputFormatters;

        public readonly bool? enabled;

        public readonly float cursorWidth;

        public readonly Radius cursorRadius;

        public readonly Color cursorColor;

        public readonly Brightness? keyboardAppearance;

        public readonly EdgeInsets scrollPadding;

        public readonly DragStartBehavior dragStartBehavior;

        public readonly ScrollPhysics scrollPhysics;

        public readonly ui.BoxHeightStyle selectionHeightStyle;
        public readonly ui.BoxWidthStyle selectionWidthStyle;
        public readonly TextAlignVertical textAlignVertical;
        public readonly bool readOnly;
        public readonly bool enableInteractiveSelection;
        public readonly bool? showCursor;
        public readonly bool enableSuggestions;
        public readonly GestureTapCallback onTap;
        public readonly ScrollController scrollController;
        public readonly SmartDashesType smartDashesType; 
        public readonly SmartQuotesType smartQuotesType;

        public bool selectionEnabled {
            get {
                return enableInteractiveSelection;
            }
        }

        public override State createState() {
            return new _CupertinoTextFieldState();
        }

        public override void debugFillProperties(DiagnosticPropertiesBuilder properties) {
            base.debugFillProperties(properties);
            properties.add( new DiagnosticsProperty<TextEditingController>("controller", controller, defaultValue: null));
            properties.add( new DiagnosticsProperty<FocusNode>("focusNode", focusNode, defaultValue: null));
            properties.add( new DiagnosticsProperty<BoxDecoration>("decoration", decoration));
            properties.add( new DiagnosticsProperty<EdgeInsetsGeometry>("padding", padding));
            properties.add( new StringProperty("placeholder", placeholder));
            properties.add( new DiagnosticsProperty<TextStyle>("placeholderStyle", placeholderStyle));
            properties.add( new DiagnosticsProperty<OverlayVisibilityMode>("prefix", prefix == null ? default(OverlayVisibilityMode) : prefixMode));
            properties.add( new DiagnosticsProperty<OverlayVisibilityMode>("suffix", suffix == null ? default(OverlayVisibilityMode) : suffixMode));
            properties.add( new DiagnosticsProperty<OverlayVisibilityMode>("clearButtonMode", clearButtonMode));
            properties.add( new DiagnosticsProperty<TextInputType>("keyboardType", keyboardType, defaultValue: TextInputType.text));
            properties.add( new DiagnosticsProperty<TextStyle>("style", style, defaultValue: null));
            properties.add( new DiagnosticsProperty<bool>("autofocus", autofocus, defaultValue: false));
            properties.add( new DiagnosticsProperty<bool>("obscureText", obscureText, defaultValue: false));
            properties.add( new DiagnosticsProperty<bool>("autocorrect", autocorrect, defaultValue: true));
            properties.add( new EnumProperty<SmartDashesType>("smartDashesType", smartDashesType, defaultValue: obscureText ? SmartDashesType.disabled : SmartDashesType.enabled));
            properties.add( new EnumProperty<SmartQuotesType>("smartQuotesType", smartQuotesType, defaultValue: obscureText ? SmartQuotesType.disabled : SmartQuotesType.enabled));
            properties.add( new DiagnosticsProperty<bool>("enableSuggestions", enableSuggestions, defaultValue: true));
            properties.add( new IntProperty("maxLines", maxLines, defaultValue: 1));
            properties.add( new IntProperty("minLines", minLines, defaultValue: null));
            properties.add( new DiagnosticsProperty<bool>("expands", expands, defaultValue: false));
            properties.add( new IntProperty("maxLength", maxLength, defaultValue: null));
            properties.add( new FlagProperty("maxLengthEnforced", value: maxLengthEnforced, ifTrue: "max length enforced"));
           // properties.add( new createCupertinoColorProperty("cursorColor", cursorColor, defaultValue: null));
            properties.add( new FlagProperty("selectionEnabled", value: selectionEnabled, defaultValue: true, ifFalse: "selection disabled"));
            properties.add( new DiagnosticsProperty<ScrollController>("scrollController", scrollController, defaultValue: null));
            properties.add( new DiagnosticsProperty<ScrollPhysics>("scrollPhysics", scrollPhysics, defaultValue: null));
            properties.add( new EnumProperty<TextAlign>("textAlign", textAlign, defaultValue: TextAlign.start));
            properties.add( new DiagnosticsProperty<TextAlignVertical>("textAlignVertical", textAlignVertical, defaultValue: null));

            
        }
    }

    public class _CupertinoTextFieldState : AutomaticKeepAliveClientMixin<CupertinoTextField>, TextSelectionGestureDetectorBuilderDelegate{
        public GlobalKey _clearGlobalKey = GlobalKey.key();

        TextEditingController _controller;
        TextEditingController _effectiveController {
            get { return widget.controller ?? _controller; }
        }

        FocusNode _focusNode;
        FocusNode _effectiveFocusNode {
            get { return widget.focusNode ?? _focusNode ?? (_focusNode = new FocusNode()); }
        }

        bool _showSelectionHandles = false;

        _CupertinoTextFieldSelectionGestureDetectorBuilder _selectionGestureDetectorBuilder;

        public GlobalKey<EditableTextState> editableTextKey {
            get {
                return _editableTextKey;
            }
        }

        public readonly GlobalKey<EditableTextState>  _editableTextKey =  GlobalKey<EditableTextState>.key();

        public bool forcePressEnabled {
            get { return true; }
        }

        public bool selectionEnabled {
            get { return widget.selectionEnabled;}
        }

        public override void initState() {
            base.initState();
            _selectionGestureDetectorBuilder = new _CupertinoTextFieldSelectionGestureDetectorBuilder(state: this);
            if (widget.controller == null) {
                _controller = new TextEditingController();
                _controller.addListener(updateKeepAlive);
            }
        }

        public override void didUpdateWidget(StatefulWidget oldWidget) {
            oldWidget = (CupertinoTextField) oldWidget;
            base.didUpdateWidget(oldWidget);
            if (widget.controller == null && ((CupertinoTextField)oldWidget).controller != null) {
                _controller = TextEditingController.fromValue(((CupertinoTextField)oldWidget).controller.value);
                _controller.addListener(updateKeepAlive);
            } else if (widget.controller != null &&((CupertinoTextField)oldWidget).controller == null) {
                _controller = null;
            }
            bool isEnabled = widget.enabled ?? true;
            bool wasEnabled = ((CupertinoTextField)oldWidget).enabled ?? true;
            if (wasEnabled && !isEnabled) {
                _effectiveFocusNode.unfocus();
            }
        }
        
        public override void dispose() {
            _focusNode?.dispose();
            _controller?.removeListener(updateKeepAlive);
            base.dispose();
        }


        public EditableTextState _editableText {
            get {
                return editableTextKey.currentState;
            }
        }

        public void _requestKeyboard() {
            _editableText?.requestKeyboard();
        }

        bool _shouldShowSelectionHandles(SelectionChangedCause cause) {
            // When the text field is activated by something that doesn't trigger the
            // selection overlay, we shouldn't show the handles either.
            if (!_selectionGestureDetectorBuilder.shouldShowSelectionToolbar)
                return false;

            // On iOS, we don't show handles when the selection is collapsed.
            if (_effectiveController.selection.isCollapsed)
                return false;

            if (cause == SelectionChangedCause.keyboard)
                return false;

            if (_effectiveController.text.isNotEmpty())
                return true;

            return false;
        }


        
        void _handleSelectionChanged(TextSelection selection, SelectionChangedCause cause) {
            if (cause == SelectionChangedCause.longPress) {
                _editableText?.bringIntoView(selection._base);
            }
            bool willShowSelectionHandles = _shouldShowSelectionHandles(cause);
            if (willShowSelectionHandles != _showSelectionHandles) {
                setState(()=> {
                    _showSelectionHandles = willShowSelectionHandles;
                });
            }
        }

        protected override  bool wantKeepAlive {
            get {
                return _controller?.text?.isNotEmpty() == true;
            }
        }

        bool _shouldShowAttachment(
            OverlayVisibilityMode attachment,
            bool hasText
        ) {
            switch (attachment) {
                case OverlayVisibilityMode.never:
                    return false;
                case OverlayVisibilityMode.always:
                    return true;
                case OverlayVisibilityMode.editing:
                    return hasText;
                case OverlayVisibilityMode.notEditing:
                    return !hasText;
            }
            D.assert(false);
            return false;
        }

        bool _showPrefixWidget(TextEditingValue text) {
            return widget.prefix != null && _shouldShowAttachment(
                attachment: widget.prefixMode,
                hasText: text.text.isNotEmpty()
            );
        }
        bool _showSuffixWidget(TextEditingValue text) {
            return widget.suffix != null && _shouldShowAttachment(
                attachment: widget.suffixMode,
                hasText: text.text.isNotEmpty()
            );
        }

        bool _showClearButton(TextEditingValue text) {
            return _shouldShowAttachment(
                attachment: widget.clearButtonMode,
                hasText: text.text.isNotEmpty()
            );
        }

        // True if any surrounding decoration widgets will be shown.
        bool _hasDecoration {
            get {
                return widget.placeholder != null ||
                       widget.clearButtonMode != OverlayVisibilityMode.never ||
                       widget.prefix != null ||
                       widget.suffix != null;
            }
        }
        TextAlignVertical  _textAlignVertical {
            get {
                if (widget.textAlignVertical != null) {
                    return widget.textAlignVertical;
                }
                return _hasDecoration ? TextAlignVertical.center : TextAlignVertical.top;
            }
        }


        Widget _addTextDependentAttachments(Widget editableText, TextStyle textStyle, TextStyle placeholderStyle) {
            D.assert(editableText != null);
            D.assert(textStyle != null);
            D.assert(placeholderStyle != null);

            if (widget.placeholder == null &&
                widget.clearButtonMode == OverlayVisibilityMode.never &&
                widget.prefix == null &&
                widget.suffix == null) {
                return editableText;
            }

            return new ValueListenableBuilder<TextEditingValue>(
                valueListenable: _effectiveController,
                child: editableText,
                builder: (BuildContext context, TextEditingValue text, Widget child) => {
                    List<Widget> rowChildren = new List<Widget>();

                    if (_showPrefixWidget(text)) {
                        rowChildren.Add(widget.prefix);
                    }

                    List<Widget> stackChildren = new List<Widget>();
                    if (widget.placeholder != null && text.text.isEmpty()) {
                        stackChildren.Add(
                            new SizedBox(
                                width  : float.PositiveInfinity,
                                child: new Padding(
                                    padding: widget.padding,
                                    child: new Text(
                                        widget.placeholder,
                                        maxLines: widget.maxLines,
                                        overflow: TextOverflow.ellipsis,
                                        style: placeholderStyle,
                                        textAlign: widget.textAlign
                                    )
                                )
                                )
                        );
                    }

                    stackChildren.Add(child);
                    rowChildren.Add(
                        new Expanded
                        (child: new Stack
                            (children: stackChildren)));

                    if (_showSuffixWidget(text)) {
                        rowChildren.Add(widget.suffix);
                    }
                    else if (_showClearButton(text)) {
                        rowChildren.Add(
                            new GestureDetector(
                                onTap: widget.enabled ?? true
                                    ? () => {
                                        bool textChanged = _effectiveController.text.isNotEmpty();
                                        _effectiveController.clear();
                                        if (widget.onChanged != null && textChanged) {
                                            widget.onChanged(_effectiveController.text);
                                        }
                                    }
                                    : (GestureTapCallback) null,
                                child: new Padding(
                                    padding: EdgeInsets.symmetric(horizontal: 6.0f),
                                    child: new Icon(
                                        CupertinoIcons.clear_thick_circled,
                                        size: 18.0f,
                                        color: CupertinoDynamicColor.resolve(CupertinoTextFieldUtils._kClearButtonColor, context)
                                    )
                                )
                            )
                        );
                    }

                    return new Row(children: rowChildren);
                }
            );
        }


        public override Widget build(BuildContext context) {
            base.build(context);
            D.assert(WidgetsD.debugCheckHasDirectionality(context));
            TextEditingController controller = _effectiveController;
            List<TextInputFormatter> formatters = widget.inputFormatters ?? new List<TextInputFormatter>();
            bool enabled = widget.enabled ?? true;
            Offset cursorOffset =
                new Offset(
                    CupertinoTextFieldUtils._iOSHorizontalCursorOffsetPixels / MediaQuery.of(context).devicePixelRatio,
                    0);
            if (widget.maxLength != null && widget.maxLengthEnforced) {
                formatters.Add(new LengthLimitingTextInputFormatter(widget.maxLength));
            }

            CupertinoThemeData themeData = CupertinoTheme.of(context);
            TextStyle resolvedStyle = widget.style?.copyWith(
                color: CupertinoDynamicColor.resolve(widget.style?.color, context),
                backgroundColor: CupertinoDynamicColor.resolve(widget.style?.backgroundColor, context)
            );
            TextStyle textStyle = themeData.textTheme.textStyle.merge(resolvedStyle);
            TextStyle resolvedPlaceholderStyle = widget.placeholderStyle?.copyWith(
                color: CupertinoDynamicColor.resolve(widget.placeholderStyle?.color, context),
                backgroundColor: CupertinoDynamicColor.resolve(widget.placeholderStyle?.backgroundColor, context)
            );
            TextStyle placeholderStyle = textStyle.merge(resolvedPlaceholderStyle);
            Brightness? keyboardAppearance = widget.keyboardAppearance ?? CupertinoTheme.brightnessOf(context);
            
            Color cursorColor =  CupertinoDynamicColor.resolve(widget.cursorColor, context) ?? themeData.primaryColor;
            Color disabledColor = CupertinoDynamicColor.resolve(CupertinoTextFieldUtils._kDisabledBackground, context);

            Color decorationColor = CupertinoDynamicColor.resolve(widget.decoration?.color, context);
            BoxBorder border = widget.decoration?.border;
            Border resolvedBorder = border as Border;
            if (border is Border) {
                BorderSide resolveBorderSide(BorderSide side) {
                    return side == BorderSide.none
                        ? side
                        : side.copyWith(color: CupertinoDynamicColor.resolve(side.color, context));
                }
                resolvedBorder = (Border) (border == null || border.GetType() != typeof(Border)
                    ? border
                    : new Border(
                        top: resolveBorderSide(((Border)border).top),
                        left: resolveBorderSide(((Border)border).left),
                        bottom: resolveBorderSide(((Border)border).bottom),
                        right: resolveBorderSide(((Border)border).right)
                    ));
            }

            BoxDecoration effectiveDecoration = widget.decoration?.copyWith(
                border: resolvedBorder,
                color: enabled ? decorationColor : (decorationColor == null ? disabledColor : decorationColor)
            );
            
            Widget paddedEditable = new Padding(
                  padding: widget.padding,
                  child: new RepaintBoundary(
                    child: new EditableText(
                      key: editableTextKey,
                      controller: controller,
                      readOnly: widget.readOnly,
                      toolbarOptions: widget.toolbarOptions,
                      showCursor: widget.showCursor,
                      showSelectionHandles: _showSelectionHandles,
                      focusNode: _effectiveFocusNode,
                      keyboardType: widget.keyboardType,
                      textInputAction: widget.textInputAction,
                      textCapitalization: widget.textCapitalization,
                      style: textStyle,
                      strutStyle: widget.strutStyle,
                      textAlign: widget.textAlign,
                      autofocus: widget.autofocus,
                      obscureText: widget.obscureText,
                      autocorrect: widget.autocorrect,
                      smartDashesType: widget.smartDashesType,
                      smartQuotesType: widget.smartQuotesType,
                      enableSuggestions: widget.enableSuggestions,
                      maxLines: widget.maxLines,
                      minLines: widget.minLines,
                      expands: widget.expands,
                      selectionColor: CupertinoTheme.of(context).primaryColor.withOpacity(0.2f),
                      selectionControls: widget.selectionEnabled
                        ? CupertinoTextFieldUtils.cupertinoTextSelectionControls : null,
                      onChanged: widget.onChanged,
                      onSelectionChanged: _handleSelectionChanged,
                      onEditingComplete: widget.onEditingComplete,
                      onSubmitted: widget.onSubmitted,
                      inputFormatters: formatters,
                      rendererIgnoresPointer: true,
                      cursorWidth: widget.cursorWidth,
                      cursorRadius: widget.cursorRadius,
                      cursorColor: cursorColor,
                      cursorOpacityAnimates: true,
                      cursorOffset: cursorOffset,
                      paintCursorAboveText: true,
                      backgroundCursorColor: CupertinoDynamicColor.resolve(CupertinoColors.inactiveGray, context),
                      selectionHeightStyle: widget.selectionHeightStyle,
                      selectionWidthStyle: widget.selectionWidthStyle,
                      scrollPadding: widget.scrollPadding,
                      keyboardAppearance: keyboardAppearance,
                      dragStartBehavior: widget.dragStartBehavior,
                      scrollController: widget.scrollController,
                      scrollPhysics: widget.scrollPhysics,
                      enableInteractiveSelection: widget.enableInteractiveSelection
                    )
                  )
                );
            return new IgnorePointer(
                ignoring: !enabled,
                child: new Container(
                    decoration: effectiveDecoration,
                    child: _selectionGestureDetectorBuilder.buildGestureDetector(
                        behavior: HitTestBehavior.translucent,
                        child: new Align(
                            alignment: new Alignment(-1.0f, _textAlignVertical.y),
                            widthFactor: 1.0f,
                            heightFactor: 1.0f,
                            child: _addTextDependentAttachments(paddedEditable, textStyle, placeholderStyle)
                        )
                    )
                )
                
            );
        }
    }
}