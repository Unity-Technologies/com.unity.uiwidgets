using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using AOT;
using Unity.UIWidgets.async;
using Unity.UIWidgets.engine;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.ui;
using UnityEngine;
using UnityEngine.UI;

namespace Unity.UIWidgets.ui {
    public delegate void VoidCallback();

    public delegate void FrameCallback(TimeSpan duration);

    public delegate void TimingsCallback(List<FrameTiming> timings);

    public delegate void PointerDataPacketCallback(PointerDataPacket packet);

    public delegate void PlatformMessageResponseCallback(byte[] data);

    public delegate Future PlatformMessageCallback(
        string name, byte[] data, PlatformMessageResponseCallback callback);

    delegate void _SetNeedsReportTimingsFunc(IntPtr ptr, bool value);

    public enum FramePhase {
        buildStart,
        buildFinish,
        rasterStart,
        rasterFinish,
    }

    public class FrameTiming {
        public FrameTiming(List<long> timestamps) {
            D.assert(timestamps.Count == Enum.GetNames(typeof(FramePhase)).Length);
            _timestamps = timestamps;
        }

        public long timestampInMicroseconds(FramePhase phase) => _timestamps[(int) phase];

        TimeSpan _rawDuration(FramePhase phase) => TimeSpan.FromMilliseconds(_timestamps[(int) phase] / 1000.0);

        public TimeSpan buildDuration => _rawDuration(FramePhase.buildFinish) - _rawDuration(FramePhase.buildStart);

        public TimeSpan rasterDuration => _rawDuration(FramePhase.rasterFinish) - _rawDuration(FramePhase.rasterStart);

        public TimeSpan totalSpan => _rawDuration(FramePhase.rasterFinish) - _rawDuration(FramePhase.buildStart);

        List<long> _timestamps; // in microseconds

        string _formatMS(TimeSpan duration) => $"{duration.Milliseconds}ms";

        public override string ToString() {
            return
                $"{GetType()}(buildDuration: {_formatMS(buildDuration)}, rasterDuration: {_formatMS(rasterDuration)}, totalSpan: {_formatMS(totalSpan)})";
        }
    }

    public enum AppLifecycleState {
        resumed,
        inactive,
        paused,
        detached,
    }

    public class WindowPadding {
        internal WindowPadding(float left, float top, float right, float bottom) {
            this.left = left;
            this.top = top;
            this.right = right;
            this.bottom = bottom;
        }

        public readonly float left;

        public readonly float top;

        public readonly float right;

        public readonly float bottom;

        public static readonly WindowPadding zero = new WindowPadding(left: 0.0f, top: 0.0f, right: 0.0f, bottom: 0.0f);

        public override string ToString() {
            return $"{GetType()}(left: {left}, top: {top}, right: {right}, bottom: {bottom})";
        }
    }


    public class Locale : IEquatable<Locale> {
        public Locale(string languageCode, string countryCode = null) {
            D.assert(languageCode != null);
            _languageCode = languageCode;
            _countryCode = countryCode;
        }

        public Locale(string languageCode, string countryCode = null, string scriptCode = null) {
            D.assert(languageCode != null);
            _languageCode = languageCode;
            _countryCode = countryCode;
            _scriptCode = scriptCode;
        }

        public static Locale fromSubtags(
            string languageCode = "und",
            string scriptCode = null,
            string countryCode = null
        ) {
            D.assert(languageCode != null); // ignore: unnecessary_null_comparison
            D.assert(languageCode != "");
            D.assert(scriptCode != "");
            D.assert(countryCode != "");
            return new Locale(languageCode, countryCode, scriptCode);
        }

        readonly string _languageCode;

        public string languageCode {
            get { return _languageCode; }
        }

        readonly string _countryCode;

        public string countryCode {
            get { return _countryCode; }
        }

        readonly string _scriptCode;

        public string scriptCode { get; }

        public bool Equals(Locale other) {
            if (ReferenceEquals(null, other)) {
                return false;
            }

            if (ReferenceEquals(this, other)) {
                return true;
            }

            return string.Equals(_languageCode, other._languageCode) &&
                   string.Equals(_countryCode, other._countryCode);
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

            return Equals((Locale) obj);
        }

        public override int GetHashCode() {
            unchecked {
                return ((_languageCode != null ? _languageCode.GetHashCode() : 0) * 397) ^
                       (_countryCode != null ? _countryCode.GetHashCode() : 0);
            }
        }

        public static bool operator ==(Locale left, Locale right) {
            return Equals(left, right);
        }

        public static bool operator !=(Locale left, Locale right) {
            return !Equals(left, right);
        }

        public override string ToString() {
            if (countryCode == null) {
                return languageCode;
            }

            return $"{languageCode}_{countryCode}";
        }
    }

    public class Window {
        internal IntPtr _ptr;
        internal object _binding;
        internal UIWidgetsPanelWrapper _panel;
        internal _AsyncCallbackState _asyncCallbackState;

        internal Window() {
            _setNeedsReportTimings = Window_setNeedsReportTimings;
        }

        public static Window instance {
            get {
                IntPtr ptr = Window_instance();
                if (ptr == IntPtr.Zero) {
                    D.assert(false, () => "AssertionError: Window.instance is null. Please enclose your code with window scope (detailed examples can be found in the README file)");
                    return null;
                }
                
                GCHandle gcHandle = (GCHandle) ptr;
                return (Window) gcHandle.Target;
            }
        }

        public float devicePixelRatio { get; internal set; } = 1.0f;

        public Size physicalSize { get; internal set; } = Size.zero;

        public float physicalDepth { get; internal set; } = float.MaxValue;

        public WindowPadding viewInsets { get; internal set; } = WindowPadding.zero;

        public WindowPadding viewPadding { get; internal set; } = WindowPadding.zero;

        public WindowPadding systemGestureInsets { get; internal set; } = WindowPadding.zero;

        public WindowPadding padding { get; internal set; } = WindowPadding.zero;

        public VoidCallback onMetricsChanged { get; set; }

        public Locale locale {
            get {
                if (_locales != null && _locales.isNotEmpty()) {
                    return _locales.first();
                }

                return null;
            }
        }

        List<Locale> _locales;

        public List<Locale> locales { get; }

        Locale computePlatformResolvedLocale(List<Locale> supportedLocales) {
            List<string> supportedLocalesData = new List<string>();
            foreach (Locale locale in supportedLocales) {
                supportedLocalesData.Add(locale.languageCode);
                supportedLocalesData.Add(locale.countryCode);
                supportedLocalesData.Add(locale.scriptCode);
            }

            List<string> result = _computePlatformResolvedLocale(supportedLocalesData);

            if (result.isNotEmpty()) {
                return Locale.fromSubtags(
                    languageCode: result[0],
                    countryCode: result[1] == "" ? null : result[1],
                    scriptCode: result[2] == "" ? null : result[2]);
            }

            return null;
        }

        // Window_computePlatformResolvedLocale not implement yet, TODO _computePlatformResolvedLocale
        List<string> _computePlatformResolvedLocale(List<string> supportedLocalesData) {
            Window_computePlatformResolvedLocale(supportedLocalesData);
            return new List<string>();
        }

        /// A callback that is invoked whenever [locale] changes value.
        ///
        /// The framework invokes this callback in the same zone in which the
        /// callback was set.
        ///
        /// See also:
        ///
        ///  * [WidgetsBindingObserver], for a mechanism at the widgets layer to
        ///    observe when this callback is invoked.
        public VoidCallback onLocaleChanged {
            get { return _onLocaleChanged; }
            set {
                _onLocaleChanged = value;
            }
        }

        VoidCallback _onLocaleChanged;

        public string initialLifecycleState {
            get {
                return _initialLifecycleState;
            }
        }

        string _initialLifecycleState = "AppLifecycleState.resumed";
        public float textScaleFactor { get; internal set; } = 1.0f;

        public VoidCallback onTextScaleFactorChanged { get; set; }

        public bool alwaysUse24HourFormat { get; internal set; } = false;

        public Brightness platformBrightness { get; internal set; } = Brightness.light;

        public VoidCallback onPlatformBrightnessChanged { get; set; }

        public FrameCallback onBeginFrame { get; set; }

        public VoidCallback onDrawFrame { get; set; }

        TimingsCallback _onReportTimings;
        _SetNeedsReportTimingsFunc _setNeedsReportTimings;

        public TimingsCallback onReportTimings {
            get { return _onReportTimings; }
            set {
                if ((value == null) != (_onReportTimings == null)) {
                    _setNeedsReportTimings(_ptr, value != null);
                }

                _onReportTimings = value;
            }
        }

        protected float queryDevicePixelRatio() {
            return _panel.devicePixelRatio;
        }
        
        public Offset windowPosToScreenPos(Offset offset) {
            return _panel.window.windowPosToScreenPos(offset);
        }

        public PointerDataPacketCallback onPointerDataPacket {
            get { return _onPointerDataPacket; }
            set {
                _onPointerDataPacket = value;
                _onPointerDataPacketZone = Zone.current;
            }
        }

        PointerDataPacketCallback _onPointerDataPacket;
        internal Zone _onPointerDataPacketZone;

        public string defaultRouteName {
            get {
                IntPtr routeNamePtr = Window_defaultRouteName(_ptr);
                string routeName = Marshal.PtrToStringAnsi(routeNamePtr);
                Window_freeDefaultRouteName(routeNamePtr);
                return routeName;
            }
        }

        public void updateSafeArea() {
            padding = _panel.displayMetrics.viewPadding;
            viewInsets = _panel.displayMetrics.viewInsets;
        }
        
        public void scheduleFrame() {
            Window_scheduleFrame(_ptr);
            _panel.window.onNewFrameScheduled();
        }

        public void scheduleMicrotask(Action callback) {
            async_.scheduleMicrotask(() => {
                callback.Invoke();
                return null;
            });
        }

        public void render(Scene scene) {
            Window_render(_ptr, scene._ptr);
        }

        public AccessibilityFeatures accessibilityFeatures { get; internal set; } = AccessibilityFeatures.zero;

        public VoidCallback onAccessibilityFeaturesChanged { get; set; }

        public unsafe void sendPlatformMessage(string name,
            byte[] data, PlatformMessageResponseCallback callback) {
            fixed (byte* bytes = data) {
                var callbackHandle = GCHandle.Alloc(callback);
                IntPtr errorPtr = Window_sendPlatformMessage(name, _sendPlatformMessageCallback,
                    (IntPtr) callbackHandle, bytes, data?.Length ?? 0);

                if (errorPtr != IntPtr.Zero) {
                    callbackHandle.Free();
                    throw new Exception(Marshal.PtrToStringAnsi(errorPtr));
                }
            }
        }

        public PlatformMessageCallback onPlatformMessage {
            get { return _onPlatformMessage; }
            set {
                _onPlatformMessage = value;
                _onPlatformMessageZone = Zone.current;
            }
        }

        PlatformMessageCallback _onPlatformMessage;
        internal Zone _onPlatformMessageZone;

        internal unsafe void _respondToPlatformMessage(int responseId, byte[] data) {
            fixed (byte* bytes = data) {
                Window_respondToPlatformMessage(_ptr, responseId, bytes, data?.Length ?? 0);
            }
        }

        [DllImport(NativeBindings.dllName)]
        static extern IntPtr Window_instance();

        [DllImport(NativeBindings.dllName)]
        static extern void Window_setNeedsReportTimings(IntPtr ptr, bool value);

        [DllImport(NativeBindings.dllName)]
        static extern IntPtr Window_defaultRouteName(IntPtr ptr);

        [DllImport(NativeBindings.dllName)]
        static extern void Window_freeDefaultRouteName(IntPtr routeNamePtr);

        [DllImport(NativeBindings.dllName)]
        static extern void Window_scheduleFrame(IntPtr ptr);

        [DllImport(NativeBindings.dllName)]
        static extern void Window_render(IntPtr ptr, IntPtr scene);

        [DllImport(NativeBindings.dllName)]
        static extern void Window_computePlatformResolvedLocale(List<string> supportedLocalesData);

        [MonoPInvokeCallback(typeof(Window_sendPlatformMessageCallback))]
        static unsafe void _sendPlatformMessageCallback(IntPtr callbackHandle, byte* data, int dataLength) {
            GCHandle handle = (GCHandle) callbackHandle;
            var callback = (PlatformMessageResponseCallback) handle.Target;
            handle.Free();

            if (!Isolate.checkExists()) {
                return;
            }

            byte[] bytes = null;
            if (data != null && dataLength != 0) {
                bytes = new byte[dataLength];
                Marshal.Copy((IntPtr) data, bytes, 0, dataLength);
            }

            try {
                callback(bytes);
            }
            catch (Exception ex) {
                Debug.LogException(ex);
            }
        }

        unsafe delegate void Window_sendPlatformMessageCallback(IntPtr callbackHandle, byte* data, int dataLength);

        [DllImport(NativeBindings.dllName)]
        static extern unsafe IntPtr Window_sendPlatformMessage(string name,
            Window_sendPlatformMessageCallback callback, IntPtr callbackHandle,
            byte* data, int dataLength);

        [DllImport(NativeBindings.dllName)]
        static extern unsafe void Window_respondToPlatformMessage(IntPtr ptr, int responseId,
            byte* data, int dataLength);
    }

    public class AccessibilityFeatures : IEquatable<AccessibilityFeatures> {
        internal AccessibilityFeatures(int index) {
            _index = index;
        }

        const int _kAccessibleNavigation = 1 << 0;
        const int _kInvertColorsIndex = 1 << 1;
        const int _kDisableAnimationsIndex = 1 << 2;
        const int _kBoldTextIndex = 1 << 3;
        const int _kReduceMotionIndex = 1 << 4;
        const int _kHighContrastIndex = 1 << 5;

        readonly int _index;

        public static readonly AccessibilityFeatures zero = new AccessibilityFeatures(0);

        public bool accessibleNavigation => (_kAccessibleNavigation & _index) != 0;

        public bool invertColors => (_kInvertColorsIndex & _index) != 0;

        public bool disableAnimations => (_kDisableAnimationsIndex & _index) != 0;

        public bool boldText => (_kBoldTextIndex & _index) != 0;

        public bool reduceMotion => (_kReduceMotionIndex & _index) != 0;

        public bool highContrast => (_kHighContrastIndex & _index) != 0;

        public override string ToString() {
            List<string> features = new List<string>();
            if (accessibleNavigation)
                features.Add("accessibleNavigation");
            if (invertColors)
                features.Add("invertColors");
            if (disableAnimations)
                features.Add("disableAnimations");
            if (boldText)
                features.Add("boldText");
            if (reduceMotion)
                features.Add("reduceMotion");
            if (highContrast)
                features.Add("highContrast");
            return $"AccessibilityFeatures{features}";
        }

        public bool Equals(AccessibilityFeatures other) {
            if (ReferenceEquals(null, other)) {
                return false;
            }

            if (ReferenceEquals(this, other)) {
                return true;
            }

            return _index == other._index;
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

            return Equals((AccessibilityFeatures) obj);
        }

        public override int GetHashCode() {
            return _index;
        }

        public static bool operator ==(AccessibilityFeatures left, AccessibilityFeatures right) {
            return Equals(left, right);
        }

        public static bool operator !=(AccessibilityFeatures left, AccessibilityFeatures right) {
            return !Equals(left, right);
        }
    }

    public enum Brightness {
        dark,
        light,
    }
}