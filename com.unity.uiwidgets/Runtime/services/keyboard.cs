using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Unity.UIWidgets.async;
using Unity.UIWidgets.engine;
using Unity.UIWidgets.external.simplejson;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.gestures;
using Unity.UIWidgets.ui;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;

#endif

namespace Unity.UIWidgets.service {
    public delegate RawInputKeyResponse GlobalKeyEventHandlerDelegate(RawKeyEvent rawEvt,
        bool enableCustomAction = false);

    public class RawInputKeyResponse {
        public readonly bool swallow;
        public readonly char input;
        public readonly TextInputAction? inputAction;

        public RawInputKeyResponse(bool swallow, int input = 0, TextInputAction? inputAction = null) {
            this.swallow = swallow;
            this.input = (char) input;
            this.inputAction = inputAction;
        }

        public static RawInputKeyResponse convert(RawKeyEvent evt) {
            return new RawInputKeyResponse(
                false,
                evt.data.unityEvent.character,
                null);
        }

        public static readonly RawInputKeyResponse swallowResponse = new RawInputKeyResponse(true, 0, null);
    }

    interface KeyboardDelegate : IDisposable {
        void show();
        void hide();
        void setEditingState(TextEditingValue value);

        void setEditableSizeAndTransform(Dictionary<string, object> args);
        
        void setStyle(Dictionary<string, object> args);
        void setIMEPos(Offset imeGlobalPos);

        void setClient(int client, TextInputConfiguration configuration);
        void clearClient();

        bool imeRequired();
    }

    public interface TextInputUpdateListener {
        void Update();
    }

    public interface TextInputOnGUIListener {
        void OnGUI();
    }

    class DefaultKeyboardDelegate : KeyboardDelegate, TextInputOnGUIListener {
        int _client;

        TextEditingValue _value;

        TextInputConfiguration _textInputConfiguration;

        public void show() {
        }

        public void hide() {
        }

        public void setEditingState(TextEditingValue value) {
            _value = value;
        }

        public void setEditableSizeAndTransform(Dictionary<string, object> args) {
            //todo
        }

        public void setStyle(Dictionary<string, object> args) {
            //todo 
        }

        Offset _editorWindowPosToScreenPos(Offset position) {
#if UNITY_EDITOR_WIN
            return position * EditorGUIUtility.pixelsPerPoint;
#elif UNITY_EDITOR_OSX
            //locate the IME 30 logical pixels lower than the caret
            var offsetY = 30f;
            return new Offset(position.dx, position.dy + offsetY);
#else
            return position;
#endif
        }

        public void setIMEPos(Offset imeGlobalPos) {
            var uiWidgetWindowAdapter = Window.instance;
            Offset screenPos = uiWidgetWindowAdapter != null
                ? uiWidgetWindowAdapter.windowPosToScreenPos(imeGlobalPos)
                : _editorWindowPosToScreenPos(imeGlobalPos);

            Input.compositionCursorPos = new Vector2(screenPos.dx, screenPos.dy);
        }

        public void setClient(int client, TextInputConfiguration configuration) {
            _client = client;
            _textInputConfiguration = configuration;
        }

        public void clearClient() {
            _client = 0;
            _textInputConfiguration = null;
        }

        public bool imeRequired() {
            return true;
        }

        bool isIMEInput = false;

        public void OnGUI() {
            if (TouchScreenKeyboard.isSupported) {
                return;
            }

            if (_client == 0) {
                return;
            }


            while (!PointerEventConverter.KeyEvent.isEmpty()) {
                var currentEvent = PointerEventConverter.KeyEvent.Dequeue();
                var oldValue = _value;

                if (currentEvent != null && currentEvent.type == EventType.KeyDown) {
                    var response = TextInput._handleGlobalInputKey(_client,
                        new RawKeyDownEvent(new RawKeyEventData(currentEvent)));

                    if (response.swallow) {
                        if (response.inputAction != null) {
                            Timer.create(TimeSpan.Zero,
                                () => { TextInput._performAction(_client, response.inputAction.Value); });
                        }

                        if (_validateCharacter(response.input)) {
                            _value = _value.insert(new string(response.input, 1));
                        }
                    }
                    else if (currentEvent.keyCode == KeyCode.Backspace) {
                        if (_value.selection.isValid) {
                            _value = _value.deleteSelection(true);
                        }
                    }
                    else if (currentEvent.character != 0) {
                        _value = _value.clearCompose();
                        char ch = currentEvent.character;
                        if (ch == '\r' || ch == 3) {
                            ch = '\n';
                        }

                        if (ch == '\n') {
                            Timer.create(TimeSpan.Zero, () => { TextInput._performAction(_client, 
                                _textInputConfiguration?.inputAction ?? TextInputAction.newline); });
                        }

                        if (_validateCharacter(ch)) {
                            _value = _value.insert(new string(ch, 1));
                        }
                    }
                    else if (!string.IsNullOrEmpty(currentEvent.keyCode.ToString())) {
                        isIMEInput = true;
                        _value = _value.compose(currentEvent.keyCode.ToString());
                    }

                    currentEvent.Use();
                }

                if (_value != oldValue) {
                    if (this.isIMEInput) {
                        var isIMEInput = this.isIMEInput;
                        Timer.create(TimeSpan.Zero,
                            () => { TextInput._updateEditingState(_client, _value, isIMEInput); });
                        this.isIMEInput = false;
                    }
                    else {
                        Timer.create(TimeSpan.Zero, () => { TextInput._updateEditingState(_client, _value, isIMEInput); });
                    }
                }
            }
        }

        public void Dispose() {
        }

        static bool _validateCharacter(char ch) {
            return ch >= ' ' || ch == '\t' || ch == '\r' || ch == 10 || ch == '\n';
        }
    }

    class UnityTouchScreenKeyboardDelegate : KeyboardDelegate, TextInputUpdateListener {
        int _client;
        string _lastCompositionString;
        TextInputConfiguration _configuration;
        TextEditingValue _value;
        TouchScreenKeyboard _keyboard;
        RangeInt? _pendingSelection;
        bool _screenKeyboardDone;
        readonly TextInput _textInput;

        public void Update() {
            if (_client == 0 || _keyboard == null) {
                return;
            }

            if (_keyboard.canSetSelection && _pendingSelection != null) {
                _keyboard.selection = _pendingSelection.Value;
                _pendingSelection = null;
            }

            if (_keyboard.status == TouchScreenKeyboard.Status.Done) {
                if (!_screenKeyboardDone) {
                    _screenKeyboardDone = true;
                    Timer.create(TimeSpan.Zero, () => {
                        TextInput._performAction(_client,
                            TextInputAction.done);
                    });
                }
            }
            else if (_keyboard.status == TouchScreenKeyboard.Status.Visible) {
                var keyboardSelection = _keyboard.selection;
                var newValue = new TextEditingValue(
                    _keyboard.text,
                    _keyboard.canGetSelection
                        ? new TextSelection(keyboardSelection.start, keyboardSelection.end)
                        : TextSelection.collapsed(0)
                );
                var changed = _value != newValue;

                _value = newValue;
                if (changed) {
                    Timer.create(TimeSpan.Zero, () => {
                        TextInput._updateEditingState(_client,
                            _value);
                    });
                }
            }
        }

        public void show() {
            var secure = _configuration.obscureText;
            var multiline = _configuration.inputType == TextInputType.multiline;
            var autocorrection = _configuration.autocorrect;
            _keyboard = TouchScreenKeyboard.Open(_value.text,
                getKeyboardTypeForConfiguration(_configuration),
                autocorrection, multiline, secure, false, "", 0);
            _pendingSelection = null;
            _screenKeyboardDone = false;
            if (_value.selection != null && _value.selection.isValid) {
                int start = _value.selection.start;
                int end = _value.selection.end;
                _pendingSelection = new RangeInt(start, end - start);
            }
        }

        public void clearClient() {
            _client = 0;
        }

        public bool imeRequired() {
            return false;
        }

        public void setStyle(Dictionary<string, object> args) {
            throw new NotImplementedException();
        }

        public void setIMEPos(Offset imeGlobalPos) {
        }

        public void setClient(int client, TextInputConfiguration configuration) {
            _client = client;
            _configuration = configuration;
        }

        public void hide() {
            if (_keyboard != null) {
                _keyboard.active = false;
                _keyboard = null;
            }
        }

        public void setEditingState(TextEditingValue state) {
            _value = state;
            if (_keyboard != null && _keyboard.active) {
                _keyboard.text = state.text;
                if (_value.selection != null && _value.selection.isValid) {
                    int start = _value.selection.start;
                    int end = _value.selection.end;
                    _pendingSelection = new RangeInt(start, end - start);
                    RangeInt selection = new RangeInt(state.selection.start, end - start);

                    if (_keyboard.canGetSelection) {
                        _pendingSelection = null;
                        _keyboard.selection = selection;
                    }
                    else {
                        _pendingSelection = selection;
                    }
                }
            }
        }

        public void setEditableSizeAndTransform(Dictionary<string, object> args) {
            throw new NotImplementedException();
        }

        static TouchScreenKeyboardType getKeyboardTypeForConfiguration(TextInputConfiguration config) {
            var inputType = config.inputType;

            if (inputType.index == TextInputType.url.index) {
                return TouchScreenKeyboardType.URL;
            }

            if (inputType.index == TextInputType.emailAddress.index) {
                return TouchScreenKeyboardType.EmailAddress;
            }

            if (inputType.index == TextInputType.phone.index) {
                return TouchScreenKeyboardType.PhonePad;
            }

            if (inputType.index == TextInputType.number.index) {
                return TouchScreenKeyboardType.NumberPad;
            }

            return TouchScreenKeyboardType.Default;
        }

        public void Dispose() {
        }
    }

    abstract class AbstractUIWidgetsKeyboardDelegate : KeyboardDelegate {
        protected AbstractUIWidgetsKeyboardDelegate() {
            UIWidgetsMessageManager.instance.AddChannelMessageDelegate("TextInput", _handleMethodCall);
        }

        public void Dispose() {
            UIWidgetsMessageManager.instance.RemoveChannelMessageDelegate("TextInput", _handleMethodCall);
        }

        public abstract void show();

        public abstract void hide();

        public abstract void setEditingState(TextEditingValue value);

        public void setEditableSizeAndTransform(Dictionary<string, object> args) {
            Debug.LogError("not implemented yet");
        }

        public void setStyle(Dictionary<string, object> args) {
            Debug.LogError("not implemented yet");
        }

        public abstract void setIMEPos(Offset imeGlobalPos);
        public abstract void setClient(int client, TextInputConfiguration configuration);

        public abstract void clearClient();

        public virtual bool imeRequired() {
            return false;
        }

        void _handleMethodCall(string method, List<JSONNode> args) {
            if (TextInput._currentConnection == null) {
                return;
            }
            int client = args[0].AsInt;
            if (client != TextInput._currentConnection._id) {
                return;
            }

            using (Isolate.getScope(TextInput._currentConnection.isolate)) {
                switch (method) {
                    case "TextInputClient.updateEditingState":
                        TextInput._updateEditingState(client, TextEditingValue.fromJSON(args[1].AsObject));
                        break;
                    case "TextInputClient.performAction":
                        TextInput._performAction(client, TextInputUtils._toTextInputAction(args[1].Value));
                        break;
                    default:
                        throw new UIWidgetsError($"unknown method ${method}");
                }
            }
        }
    }

#if UNITY_WEBGL
    class UIWidgetsWebGLKeyboardDelegate : AbstractUIWidgetsKeyboardDelegate {
        
        public override void show() {
            Input.imeCompositionMode = IMECompositionMode.On;
        }

        public override void hide() {
        }

        public override void setEditingState(TextEditingValue value) {
            UIWidgetsTextInputSetTextInputEditingState(value.toJson().ToString());
        }

        public override void setIMEPos(Offset imeGlobalPos) {
            var window = Window.instance as UIWidgetWindowAdapter;
            var canvasPos = window.windowPosToScreenPos(imeGlobalPos);
            UIWidgetsTextInputSetIMEPos(canvasPos.dx, canvasPos.dy);
        }

        public override void setClient(int client, TextInputConfiguration configuration) {
            WebGLInput.captureAllKeyboardInput = false;
            Input.imeCompositionMode = IMECompositionMode.On;
            UIWidgetsTextInputSetClient(client, configuration.toJson().ToString());
        }

        public override void clearClient() {
            UIWidgetsTextInputClearTextInputClient();
        }

        public override bool imeRequired() {
            return true;
        }

        [DllImport ("__Internal")]
        internal static extern void UIWidgetsTextInputSetClient(int client, string configuration);
        
        [DllImport ("__Internal")]
        internal static extern void UIWidgetsTextInputSetTextInputEditingState(string jsonText);
        
        [DllImport ("__Internal")]
        internal static extern void UIWidgetsTextInputClearTextInputClient();
        
        [DllImport ("__Internal")]
        internal static extern void UIWidgetsTextInputSetIMEPos(float x, float y);


    }
#endif

#if UNITY_IOS || UNITY_ANDROID
    class UIWidgetsTouchScreenKeyboardDelegate : AbstractUIWidgetsKeyboardDelegate {

        public override void show() {
            UIWidgetsTextInputShow();
        }

        public override void hide() {
            UIWidgetsTextInputHide();
        }

        public override void setEditingState(TextEditingValue value) {
            UIWidgetsTextInputSetTextInputEditingState(value.toJSON().ToString());
        }

        public override void setIMEPos(Offset imeGlobalPos) {
        }

        public override void setClient(int client, TextInputConfiguration configuration) {
            UIWidgetsTextInputSetClient(client, configuration.toJson().ToString());
        }

        public override void clearClient() {
            UIWidgetsTextInputClearTextInputClient();
        }
        
#if UNITY_IOS
        [DllImport ("__Internal")]
        internal static extern void UIWidgetsTextInputShow();
        
        [DllImport ("__Internal")]
        internal static extern void UIWidgetsTextInputHide();
        
        [DllImport ("__Internal")]
        internal static extern void UIWidgetsTextInputSetClient(int client, string configuration);
        
        [DllImport ("__Internal")]
        internal static extern void UIWidgetsTextInputSetTextInputEditingState(string jsonText);
        
        [DllImport ("__Internal")]
        internal static extern void UIWidgetsTextInputClearTextInputClient();
        
#elif UNITY_ANDROID
        internal static void UIWidgetsTextInputShow() {
            using (
                AndroidJavaClass pluginClass =
 new AndroidJavaClass("com.unity.uiwidgets.plugin.editing.TextInputPlugin")
            ) {
                pluginClass.CallStatic("show");
            }
        }
        
        internal static void UIWidgetsTextInputHide() {
            using (
                AndroidJavaClass pluginClass =
 new AndroidJavaClass("com.unity.uiwidgets.plugin.editing.TextInputPlugin")
            ) {
                pluginClass.CallStatic("hide");
            }
        }
        
        internal static void UIWidgetsTextInputSetClient(int client, string configuration) {
            using (
                AndroidJavaClass pluginClass =
 new AndroidJavaClass("com.unity.uiwidgets.plugin.editing.TextInputPlugin")
            ) {
               
                pluginClass.CallStatic("setClient", client, configuration);
            }
        }
        
        internal static void UIWidgetsTextInputSetTextInputEditingState(string jsonText) {
            using (
                AndroidJavaClass pluginClass =
 new AndroidJavaClass("com.unity.uiwidgets.plugin.editing.TextInputPlugin")
            ) {
                pluginClass.CallStatic("setEditingState", jsonText);
            }
        }
        
        internal static void UIWidgetsTextInputClearTextInputClient() {
            using (
                AndroidJavaClass pluginClass =
 new AndroidJavaClass("com.unity.uiwidgets.plugin.editing.TextInputPlugin")
            ) {
                pluginClass.CallStatic("clearClient");
            }
        }
#endif
    }
#endif
}