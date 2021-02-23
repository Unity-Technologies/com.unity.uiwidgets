/*using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using AOT;
using uiwidgets;
using Unity.UIWidgets.engine2;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.services;
using Unity.UIWidgets.ui;
using Unity.UIWidgets.widgets;
using UnityEditor;
using UnityEngine;
using Rect = UnityEngine.Rect;
using ui_ = Unity.UIWidgets.widgets.ui_;

namespace Unity.UIWidgets.editor2 {
#if UNITY_EDITOR
    
    class MyApp : StatelessWidget
    {
        public override Widget build(BuildContext context)
        {
            return new WidgetsApp(
                home: new Container(color: Colors.blue),
                pageRouteBuilder: (settings, builder) =>
                    new PageRouteBuilder(
                        settings: settings,
                        pageBuilder: (Buildcontext, animation, secondaryAnimation) => builder(context)
                    )
            );
        }
    }

    public class UIWidgetsEditorPanel : EditorWindow, IUIWidgetsPanel {
        RenderTexture _renderTexture;
        
        void _createRenderTexture(int width, int height, float devicePixelRatio) { 
            D.assert(_renderTexture == null);

            var desc = new RenderTextureDescriptor(
                width, height, RenderTextureFormat.ARGB32, 0) {
                useMipMap = false,
                autoGenerateMips = false,
            };

            _renderTexture = new RenderTexture(desc) {hideFlags = HideFlags.HideAndDontSave};
            _renderTexture.Create();

            _width = width;
            _height = height;
            _devicePixelRatio = devicePixelRatio;
        }

        void _destroyRenderTexture() {
            D.assert(_renderTexture != null);
            ObjectUtils.SafeDestroy(_renderTexture);
            _renderTexture = null;
        }

        void _enableUIWidgetsPanel(string font_settings) {
            UIWidgetsPanel_onEnable(_ptr, _renderTexture.GetNativeTexturePtr(),
                _width, _height, _devicePixelRatio, Application.streamingAssetsPath, font_settings);
        }

        void _resizeUIWidgetsPanel() {
            UIWidgetsPanel_onRenderTexture(_ptr,
                _renderTexture.GetNativeTexturePtr(),
                _width, _height, _devicePixelRatio);
        }

        void _disableUIWidgetsPanel() {
            _renderTexture = null;
        }

        public Isolate isolate { get; private set; }

        IntPtr _ptr;
        GCHandle _handle;

        int _width;
        int _height;
        float _devicePixelRatio;

        int _currentWidth {
            get { return Mathf.RoundToInt(position.size.x); }
        }

        int _currentHeight {
            get { return Mathf.RoundToInt(position.size.y); }
        }

        float _currentDevicePixelRatio {
            get { return EditorGUIUtility.pixelsPerPoint; }
        }

        void Awake() {
            D.assert(_renderTexture == null);
            _recreateRenderTexture(_currentWidth, _currentHeight, _currentDevicePixelRatio);

            _handle = GCHandle.Alloc(this);
            _ptr = UIWidgetsPanel_constructor((IntPtr) _handle, UIWidgetsPanel_entrypoint);
            var settings = new Dictionary<string, object>();

            _enableUIWidgetsPanel(JSONMessageCodec.instance.toJson(settings));
            NativeConsole.OnEnable();
        }
        
        void OnGUI()
        {
            GUI.DrawTexture(new Rect(0.0f, 0.0f, position.width, position.height), _renderTexture);
        }

        protected virtual void main() {
            ui_.runApp(new MyApp());
        }

        void _entryPoint() {
            try {
                isolate = Isolate.current;
                Window.instance._panel = this;

                main();
            }
            catch (Exception ex) {
                Debug.LogException(new Exception("exception in main", ex));
            }
        }
        
        delegate void UIWidgetsPanel_EntrypointCallback(IntPtr handle);

        [MonoPInvokeCallback(typeof(UIWidgetsPanel_EntrypointCallback))]
        static void UIWidgetsPanel_entrypoint(IntPtr handle) {
            GCHandle gcHandle = (GCHandle) handle;
            UIWidgetsEditorPanel panel = (UIWidgetsEditorPanel) gcHandle.Target;
            panel._entryPoint();
        }
        
        bool _recreateRenderTexture(int width, int height, float devicePixelRatio) {
            if (_renderTexture != null && _width == width && _height == height &&
                _devicePixelRatio == devicePixelRatio) {
                return false;
            }

            if (_renderTexture) {
                _destroyRenderTexture();
            }

            _createRenderTexture(width, height, devicePixelRatio);
            return true;
        }
        
        [DllImport(NativeBindings.dllName)]
        static extern IntPtr UIWidgetsPanel_constructor(IntPtr handle,
            UIWidgetsPanel_EntrypointCallback entrypointCallback);


        [DllImport(NativeBindings.dllName)]
        static extern void UIWidgetsPanel_onEnable(IntPtr ptr,
            IntPtr nativeTexturePtr, int width, int height, float dpi, string streamingAssetsPath,
            string font_settings);

        [DllImport(NativeBindings.dllName)]
        static extern void UIWidgetsPanel_onRenderTexture(
            IntPtr ptr, IntPtr nativeTexturePtr, int width, int height, float dpi);

        [MenuItem("UIWidgets/NewWindow")]
        public static void OnItem() {
            EditorWindow.CreateWindow<UIWidgetsEditorPanel>();
        }

        public float cur_devicePixelRatioOverride => _currentDevicePixelRatio;
    }
    
    

#endif

}*/