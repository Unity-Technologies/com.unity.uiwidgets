using System;
using System.Collections.Generic;
using Unity.UIWidgets.async;
using Unity.UIWidgets.external.simplejson;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.gestures;
using Unity.UIWidgets.services;
using Unity.UIWidgets.ui;
using UnityEngine;

namespace Unity.UIWidgets.service {

    public enum SmartDashesType {
        disabled,
        enabled
    }

    public enum SmartQuotesType {
        disabled,
        enabled
    }
    
    public class TextInputType : IEquatable<TextInputType> {
        public readonly int index;
        public readonly bool? signed;
        public readonly bool? decimalNum;

        TextInputType(int index, bool? signed = null, bool? decimalNum = null) {
            this.index = index;
            this.signed = signed;
            this.decimalNum = decimalNum;
        }

        public static TextInputType numberWithOptions(bool signed = false, bool decimalNum = false) {
            return new TextInputType(2, signed: signed, decimalNum: decimalNum);
        }

        public static readonly TextInputType text = new TextInputType(0);
        
        public static readonly TextInputType multiline = new TextInputType(1);

        public static readonly TextInputType number = numberWithOptions();

        public static readonly TextInputType phone = new TextInputType(3);

        public static readonly TextInputType datetime = new TextInputType(4);

        public static readonly TextInputType emailAddress = new TextInputType(5);

        public static readonly TextInputType url = new TextInputType(6);

        public static readonly TextInputType visiblePassword = new TextInputType(7);

        public static readonly List<TextInputType> values = new List<TextInputType> {
            text, multiline, number, phone, datetime, emailAddress, url, visiblePassword
        };

        public static List<string> _names = new List<string> {
            "text", "multiline", "number", "phone", "datetime", "emailAddress", "url", "visiblePassword"
        };

        public JSONNode toJson() {
            JSONObject jsonObject = new JSONObject();
            jsonObject["name"] = _name;
            jsonObject["signed"] = signed;
            jsonObject["decimal"] = decimalNum;
            return jsonObject;
        }

        string _name {
            get { return $"TextInputType.{_names[index]}"; }
        }

        public bool Equals(TextInputType other) {
            if (ReferenceEquals(null, other)) {
                return false;
            }

            if (ReferenceEquals(this, other)) {
                return true;
            }

            return index == other.index && signed == other.signed && decimalNum == other.decimalNum;
        }

        public override bool Equals(object obj) {
            if (ReferenceEquals(null, obj)) {
                return false;
            }

            if (ReferenceEquals(this, obj)) {
                return true;
            }

            if (obj.GetType() != GetType()) {
                return false;
            }

            return Equals((TextInputType) obj);
        }

        public override int GetHashCode() {
            unchecked {
                var hashCode = index;
                hashCode = (hashCode * 397) ^ signed.GetHashCode();
                hashCode = (hashCode * 397) ^ decimalNum.GetHashCode();
                return hashCode;
            }
        }

        public static bool operator ==(TextInputType left, TextInputType right) {
            return Equals(left, right);
        }

        public static bool operator !=(TextInputType left, TextInputType right) {
            return !Equals(left, right);
        }

        public override string ToString() {
            return $"{GetType().FullName}(name: {_name}, signed: {signed}, decimal: {decimalNum})";
        }
    }
    
    public enum TextInputAction {
        none,
        unspecified,
        done,
        go,
        search,
        send,
        next,
        previous,
        continueAction,
        join,
        route,
        emergencyCall,
        newline
    }
    
       
    public enum TextCapitalization {
        words,
        sentences,
        characters,
        none
    }

    public class TextInputConfiguration {
        public TextInputConfiguration(
            TextInputType inputType = null,
            bool obscureText = false, 
            bool autocorrect = true, 
            SmartDashesType? smartDashesType = null,
            SmartQuotesType? smartQuotesType = null,
            bool enableSuggestions = true,
            string actionLabel = null,
            TextInputAction inputAction = TextInputAction.done,
            ui.Brightness keyboardAppearance = ui.Brightness.light,
            TextCapitalization textCapitalization = TextCapitalization.none,
            bool unityTouchKeyboard = false
            ) {
            D.assert(inputType != null);
            this.smartDashesType =
                smartDashesType ?? (obscureText ? SmartDashesType.disabled : SmartDashesType.enabled);
            this.smartQuotesType =
                smartQuotesType ?? (obscureText ? SmartQuotesType.disabled : SmartQuotesType.enabled);
            this.inputType = inputType ?? TextInputType.text;
            this.obscureText = obscureText;
            this.autocorrect = autocorrect;
            this.enableSuggestions = enableSuggestions;
            this.actionLabel = actionLabel ?? "";
            this.inputAction = inputAction;
            this.textCapitalization = textCapitalization;
            this.keyboardAppearance = keyboardAppearance;
            this.unityTouchKeyboard = unityTouchKeyboard;
        }
        
        public readonly TextInputType inputType;
        public readonly bool obscureText;
        public readonly bool autocorrect;
        public readonly SmartDashesType smartDashesType;
        public readonly SmartQuotesType smartQuotesType;
        public readonly bool enableSuggestions;
        public readonly string actionLabel;
        public readonly TextInputAction inputAction;
        public readonly TextCapitalization textCapitalization;
        public readonly ui.Brightness keyboardAppearance;
        
        public readonly bool unityTouchKeyboard;

        public JSONNode toJson() {
            var json = new JSONObject();
            json["inputType"] = inputType.toJson();
            json["obscureText"] = obscureText;
            json["autocorrect"] = autocorrect;
            json["smartDashesType"] = smartDashesType.ToString();
            json["smartQuotesType"] = smartQuotesType.ToString();
            json["enableSuggestions"] = enableSuggestions;
            json["actionLabel"] = actionLabel;
            json["inputAction"] = $"TextInputAction.{inputAction.ToString()}";
            json["unityTouchKeyboard"] = unityTouchKeyboard;
            json["textCapitalization"] = textCapitalization.ToString();
            json["keyboardAppearance"] = keyboardAppearance.ToString();
            return json;
        }
    }
    
    static partial class TextInputUtils {
        internal static TextAffinity? _toTextAffinity(string affinity) {
            switch (affinity) {
                case "TextAffinity.downstream":
                    return TextAffinity.downstream;
                case "TextAffinity.upstream":
                    return TextAffinity.upstream;
            }

            return null;
        }

        internal static TextInputAction _toTextInputAction(string action) {
            switch (action) {
                case "TextInputAction.none":
                    return TextInputAction.none;
                case "TextInputAction.unspecified":
                    return TextInputAction.unspecified;
                case "TextInputAction.go":
                    return TextInputAction.go;
                case "TextInputAction.search":
                    return TextInputAction.search;
                case "TextInputAction.send":
                    return TextInputAction.send;
                case "TextInputAction.next":
                    return TextInputAction.next;
                case "TextInputAction.previuos":
                    return TextInputAction.previous;
                case "TextInputAction.continue_action":
                    return TextInputAction.continueAction;
                case "TextInputAction.join":
                    return TextInputAction.join;
                case "TextInputAction.route":
                    return TextInputAction.route;
                case "TextInputAction.emergencyCall":
                    return TextInputAction.emergencyCall;
                case "TextInputAction.done":
                    return TextInputAction.done;
                case "TextInputAction.newline":
                    return TextInputAction.newline;
            }

            throw new UIWidgetsError($"Unknown text input action: {action}");
        }

        public static FloatingCursorDragState _toTextCursorAction(string state) {
            switch (state) {
                case "FloatingCursorDragState.start":
                    return FloatingCursorDragState.Start;
                case "FloatingCursorDragState.update":
                    return FloatingCursorDragState.Update;
                case "FloatingCursorDragState.end":
                    return FloatingCursorDragState.End;
            }

            throw new UIWidgetsError(new List<DiagnosticsNode>() {new ErrorSummary($"Unknown text cursor action: {state}")});
        }

        public static RawFloatingCursorPoint _toTextPoint(FloatingCursorDragState state,
            Dictionary<string, float?>  encoded) {
            D.assert(encoded.getOrDefault("X") != null,
                () => "You must provide a value for the horizontal location of the floating cursor.");
            D.assert(encoded.getOrDefault("Y") != null,
                () => "You must provide a value for the vertical location of the floating cursor.");
            Offset offset = state == FloatingCursorDragState.Update
                ? new Offset(encoded["X"] ?? 0.0f, encoded["Y"] ?? 0.0f)
                : new Offset(0, 0);
            return new RawFloatingCursorPoint(offset: offset, state: state);
        }
    }

    
    public enum FloatingCursorDragState {
        Start,
        Update,
        End
    }

    public class RawFloatingCursorPoint {
        public RawFloatingCursorPoint( 
            Offset offset = null,
            FloatingCursorDragState? state = null
        ) {
            D.assert(state != null);
            D.assert(state != FloatingCursorDragState.Update || offset != null);
            this.offset = offset;
            this.state = state;
        }

        public readonly Offset offset;

        public readonly FloatingCursorDragState? state;
    }

    public class TextEditingValue : IEquatable<TextEditingValue> {
        static JSONNode defaultIndexNode = new JSONNumber(-1);
        static JSONNode defaultBoolNode = new JSONBool(false);

        public TextEditingValue(
            string text = "",
            TextSelection selection = null,
            TextRange composing = null) {
            selection = selection ?? TextSelection.collapsed(offset: -1);
            
            D.assert(text != null);
            this.text = text;
            this.composing = composing ?? TextRange.empty;

            if (selection != null && selection.start >= 0 && selection.end >= 0) {
                // handle surrogate pair emoji, which takes 2 utf16 chars
                // if selection cuts in the middle of the emoji, move it to the end
                int start = selection.start, end = selection.end;
                if (start < text.Length && char.IsLowSurrogate(text[start])) {
                    start++;
                }
                if (end < text.Length && char.IsLowSurrogate(text[end])) {
                    end++;
                }
                this.selection = selection.copyWith(start, end);
            }
            else {
                this.selection = selection ?? TextSelection.collapsed(-1);
            }
        }

        public static TextEditingValue fromJSON(JSONNode json) {
            TextAffinity? affinity =
                TextInputUtils._toTextAffinity(json["selectionAffinity"].Value);
            return new TextEditingValue(
                text: json["text"].Value,
                selection: new TextSelection(
                    baseOffset: json.GetValueOrDefault("selectionBase", defaultIndexNode).AsInt,
                    extentOffset: json.GetValueOrDefault("selectionExtent", defaultIndexNode).AsInt,
                    affinity: affinity != null ? affinity.Value : TextAffinity.downstream,
                    isDirectional: json.GetValueOrDefault("selectionIsDirectional", defaultBoolNode).AsBool
                ),
                composing: new TextRange(
                    start: json.GetValueOrDefault("composingBase", defaultIndexNode).AsInt,
                    end: json.GetValueOrDefault("composingExtent", defaultIndexNode).AsInt
                )
            );
        }

        public JSONNode toJSON() {
            var json = new JSONObject();
            json["text"] = text;
            json["selectionBase"] = selection.baseOffset;
            json["selectionExtent"] = selection.extentOffset;
            json["selectionAffinity"] = selection.affinity.ToString();
            json["selectionIsDirectional"] = selection.isDirectional;
            json["composingBase"] = composing.start;
            json["composingExtent"] = composing.end;
            return json;
        }
        
        public readonly string text;
        public readonly TextSelection selection;
        public readonly TextRange composing;
        
        public static readonly TextEditingValue empty = new TextEditingValue();

        public TextEditingValue copyWith(
            string text = null,
            TextSelection selection = null,
            TextRange composing = null) {
            return new TextEditingValue(
                text: text ?? this.text, 
                selection: selection ?? this.selection, 
                composing: composing ?? this.composing
            );
        }

        //unity-specific
        public TextEditingValue insert(string text) {
            string newText;
            TextSelection newSelection;
            if (string.IsNullOrEmpty(text)) {
                return this;
            }

            var selection = this.selection;
            if (selection.start < 0) {
                selection = TextSelection.collapsed(0, this.selection.affinity);
            }

            newText = selection.textBefore(this.text) + text + selection.textAfter(this.text);
            newSelection = TextSelection.collapsed(selection.start + text.Length);
            return new TextEditingValue(
                text: newText, 
                selection: newSelection,
                composing: TextRange.empty
            );
        }

        //unity-specific
        public TextEditingValue deleteSelection(bool backDelete = true) {
            if (selection.isCollapsed) {
                if (backDelete) {
                    if (selection.start == 0) {
                        return this;
                    }

                    if (char.IsHighSurrogate(text[selection.start - 1])) {
                        return copyWith(
                            text: text.Substring(0, selection.start - 1) +
                                  text.Substring(selection.start + 1),
                            selection: TextSelection.collapsed(selection.start - 1),
                            composing: TextRange.empty);
                    }

                    if (char.IsLowSurrogate(text[selection.start - 1])) {
                        D.assert(selection.start > 1);
                        return copyWith(
                            text: text.Substring(0, selection.start - 2) +
                                  selection.textAfter(text),
                            selection: TextSelection.collapsed(selection.start - 2),
                            composing: TextRange.empty);
                    }

                    return copyWith(
                        text: text.Substring(0, selection.start - 1) + selection.textAfter(text),
                        selection: TextSelection.collapsed(selection.start - 1),
                        composing: TextRange.empty);
                }

                if (selection.start >= text.Length) {
                    return this;
                }

                return copyWith(text: text.Substring(0, selection.start) +
                                           text.Substring(selection.start + 1),
                    composing: TextRange.empty);
            }
            else {
                var newText = selection.textBefore(text) + selection.textAfter(text);
                return copyWith(text: newText, selection: TextSelection.collapsed(selection.start),
                    composing: TextRange.empty);
            }
        }

        //unity-specific
        public TextEditingValue moveLeft() {
            return moveSelection(-1);
        }

        //unity-specific
        public TextEditingValue moveRight() {
            return moveSelection(1);
        }

        //unity-specific
        public TextEditingValue extendLeft() {
            return moveExtent(-1);
        }

        //unity-specific
        public TextEditingValue extendRight() {
            return moveExtent(1);
        }

        //unity-specific
        public TextEditingValue moveExtent(int move) {
            int offset = selection.extentOffset + move;
            offset = Mathf.Max(0, offset);
            offset = Mathf.Min(offset, text.Length);
            return copyWith(selection: selection.copyWith(extentOffset: offset));
        }

        //unity-specific
        public TextEditingValue moveSelection(int move) {
            int offset = selection.baseOffset + move;
            offset = Mathf.Max(0, offset);
            offset = Mathf.Min(offset, text.Length);
            return copyWith(selection: TextSelection.collapsed(offset, affinity: selection.affinity));
        }

        //unity-specific
        public TextEditingValue compose(string composeText) {
            D.assert(!string.IsNullOrEmpty(composeText));
            var composeStart = composing == TextRange.empty ? selection.start : composing.start;
            var lastComposeEnd = composing == TextRange.empty ? selection.end : composing.end;
            
            composeStart = Mathf.Clamp(composeStart, 0, text.Length);
            lastComposeEnd = Mathf.Clamp(lastComposeEnd, 0, text.Length);
            var newText = text.Substring(0, composeStart) + composeText + text.Substring(lastComposeEnd);
            var componseEnd = composeStart + composeText.Length;
            return new TextEditingValue(
                text: newText, selection: TextSelection.collapsed(componseEnd),
                composing: new TextRange(composeStart, componseEnd)
            );
        }

        //unity-specific
        public TextEditingValue clearCompose() {
            if (composing == TextRange.empty) {
                return this;
            }

            return new TextEditingValue(
                text: text.Substring(0, composing.start) + text.Substring(composing.end),
                selection: TextSelection.collapsed(composing.start),
                composing: TextRange.empty
            );
        }

        public bool Equals(TextEditingValue other) {
            if (ReferenceEquals(null, other)) {
                return false;
            }

            if (ReferenceEquals(this, other)) {
                return true;
            }

            return string.Equals(text, other.text) && Equals(selection, other.selection) &&
                   Equals(composing, other.composing);
        }

        public override bool Equals(object obj) {
            if (ReferenceEquals(null, obj)) {
                return false;
            }

            if (ReferenceEquals(this, obj)) {
                return true;
            }

            if (obj.GetType() != GetType()) {
                return false;
            }

            return Equals((TextEditingValue) obj);
        }

        public override int GetHashCode() {
            unchecked {
                var hashCode = (text != null ? text.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (selection != null ? selection.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (composing != null ? composing.GetHashCode() : 0);
                return hashCode;
            }
        }

        public static bool operator ==(TextEditingValue left, TextEditingValue right) {
            return Equals(left, right);
        }

        public static bool operator !=(TextEditingValue left, TextEditingValue right) {
            return !Equals(left, right);
        }

        public override string ToString() {
            return $"Text: {text}, Selection: {selection}, Composing: {composing}";
        }
    }

    public interface TextSelectionDelegate {
        TextEditingValue textEditingValue { get; set; }

        void hideToolbar();

        void bringIntoView(TextPosition textPosition);

        bool cutEnabled {
            get; 
        }
        bool copyEnabled {
            get;
        }
        bool  pasteEnabled {
            get;
        }
        bool  selectAllEnabled {
            get;
        }
    }

    public interface TextInputClient {
        void updateEditingValue(TextEditingValue value, bool isIMEInput);

        void performAction(TextInputAction action);

        void updateFloatingCursor(RawFloatingCursorPoint point);
        
        TextEditingValue currentTextEditingValue { get; }
        
        void connectionClosed();
        
        //unity-specific
        RawInputKeyResponse globalInputKeyHandler(RawKeyEvent evt);
    }

    public class TextInputConnection {
        internal TextInputConnection(TextInputClient client) {
            D.assert(client != null);
            _window = Window.instance;
            _client = client;
            _id = _nextId++;
        }

        public Isolate isolate {
            get { return _window._panel.isolate; }
        }
        
        internal Size _cachedSize;
        internal Matrix4 _cachedTransform;
        
        static int _nextId = 1;
        internal readonly int _id;
        
        public void setIMEPos(Offset imeGlobalPos) {
            D.assert(attached);
            D.assert(imeGlobalPos != null);
            D.assert(imeRequired());
            TextInput.keyboardDelegate.setIMEPos(imeGlobalPos);
        }
        
        public bool imeRequired() {
            return TextInput.keyboardDelegate != null && TextInput.keyboardDelegate.imeRequired();
        }
        
        public static void debugResetId(int to = 1) {
            D.assert(() =>{
                _nextId = to;
                return true;
            });
        }
        
        internal readonly TextInputClient _client;
        public bool attached { 
            get { return TextInput._currentConnection == this; }
        }
        
        public void show() {
            D.assert(attached);
            
            Input.imeCompositionMode = IMECompositionMode.On;
            TextInput.keyboardDelegate.show();
        }
        
        public void setEditingState(TextEditingValue value) {
            D.assert(attached);
            TextInput.keyboardDelegate.setEditingState(value);
        }
        
        public void setEditableSizeAndTransform(Size editableBoxSize, Matrix4 transform) {
            if (editableBoxSize != _cachedSize || transform != _cachedTransform) {
                _cachedSize = editableBoxSize;
                _cachedTransform = transform;

                Dictionary<string,object> dictionary = new Dictionary<string, object>();
                dictionary["width"] = editableBoxSize.width;
                dictionary["height"] = editableBoxSize.height;
                dictionary["transform"] = transform.storage;
                TextInput.keyboardDelegate.setEditableSizeAndTransform(
                   dictionary
                );
            }
        }
        
        public void setStyle(
             string fontFamily,
             float? fontSize,
             FontWeight fontWeight,
             TextDirection textDirection,
             TextAlign textAlign
        ) {
            D.assert(attached);
            Dictionary<string,object> dictionary = new Dictionary<string, object>();
            dictionary["fontFamily"] = fontFamily;
            dictionary["fontSize"] = fontSize;
            dictionary["fontWeightIndex"] = fontWeight?.index;
            dictionary["textAlignIndex"] = (int)textAlign;
            dictionary["textDirectionIndex"] = (int)textDirection;
            TextInput.keyboardDelegate.setStyle(
               dictionary
            );
        }
        
        public void close() {
            if (attached) {
                TextInput.keyboardDelegate.clearClient();
                TextInput._currentConnection = null;
                Input.imeCompositionMode = IMECompositionMode.Auto;
                TextInput._scheduleHide();
            }
            D.assert(!attached);
        }
        
        public void connectionClosedReceived() {
            TextInput._currentConnection = null;
            D.assert(!attached);
        }
        
        internal readonly Window _window;
        TouchScreenKeyboard _keyboard;
    }

    class TextInput {
        static internal TextInputConnection _currentConnection;

        static internal KeyboardDelegate keyboardDelegate;

        public TextInput() {
        }

        public static TextInputConnection attach(TextInputClient client, TextInputConfiguration configuration) {
            D.assert(client != null);
            D.assert(configuration != null);
            var connection = new TextInputConnection(client);
            _currentConnection = connection;
            if (keyboardDelegate != null) {
                keyboardDelegate.Dispose();
            }

            if (Application.isEditor) {
                keyboardDelegate = new DefaultKeyboardDelegate();
            }
            else {
#if UNITY_IOS || UNITY_ANDROID
                /*if (configuration.unityTouchKeyboard) {
                    keyboardDelegate = new UnityTouchScreenKeyboardDelegate();
                }
                else {
                    keyboardDelegate = new UIWidgetsTouchScreenKeyboardDelegate();
                }*/
                keyboardDelegate = new UIWidgetsTouchScreenKeyboardDelegate();
#elif UNITY_WEBGL
                keyboardDelegate = new UIWidgetsWebGLKeyboardDelegate();
#else
                keyboardDelegate = new DefaultKeyboardDelegate();
#endif
            }

            keyboardDelegate.setClient(connection._id, configuration);
            return connection;
        }

        internal static void Update() {
            if (_currentConnection != null && _currentConnection._window == Window.instance) {
                (keyboardDelegate as TextInputUpdateListener)?.Update();
            }
        }

        internal static bool OnGUI() {
            if (_currentConnection != null && _currentConnection._window == Window.instance) {
                (keyboardDelegate as TextInputOnGUIListener)?.OnGUI();

                return true;
            }
            else {
                //skip all the key events when no connection is attached
                while (!PointerEventConverter.KeyEvent.isEmpty()) {
                    PointerEventConverter.KeyEvent.Dequeue();
                }

                return false;
            }
        }

        internal static void _updateEditingState(int client, TextEditingValue value, bool isIMEInput = false) {
            if (_currentConnection == null) {
                return;
            }

            if (client != _currentConnection._id) {
                return;
            }

            _currentConnection._client.updateEditingValue(value, isIMEInput); 
        }

        internal static void _performAction(int client, TextInputAction action) {
            if (_currentConnection == null) {
                return;
            }

            if (client != _currentConnection._id) {
                return;
            }

            _currentConnection._client.performAction(action);
        }

        internal static RawInputKeyResponse _handleGlobalInputKey(int client, RawKeyEvent evt) {
            if (_currentConnection == null) {
                return RawInputKeyResponse.convert(evt);
            }

            if (client != _currentConnection._id) {
                return RawInputKeyResponse.convert(evt);
            }

            return _currentConnection._client.globalInputKeyHandler(evt);
        }

        static bool _hidePending = false;

        static internal void _scheduleHide() {
            if (_hidePending) {
                return;
            }

            _hidePending = true;

            Window.instance.scheduleMicrotask(() => {
                _hidePending = false;
                if (_currentConnection == null) {
                    keyboardDelegate.hide();
                }
            });
        }
    }
    }