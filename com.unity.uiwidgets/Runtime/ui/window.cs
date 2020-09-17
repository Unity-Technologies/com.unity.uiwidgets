using System;
using System.Collections.Generic;
using Unity.UIWidgets.async;
using Unity.UIWidgets.editor;
using Unity.UIWidgets.foundation;
using UnityEngine;
using UnityEngine.Rendering;

namespace Unity.UIWidgets.ui {
    public delegate void VoidCallback();

    public delegate void FrameCallback(TimeSpan duration);

    public delegate void PointerDataPacketCallback(PointerDataPacket packet);

    public class WindowPadding {
        public WindowPadding(float left, float top, float right, float bottom) {
            this.left = left;
            this.top = top;
            this.right = right;
            this.bottom = bottom;
        }

        public readonly float left;

        public readonly float top;

        public readonly float right;

        public readonly float bottom;

        public static WindowPadding zero = new WindowPadding(left: 0.0f, top: 0.0f, right: 0.0f, bottom: 0.0f);

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

        readonly string _languageCode;

        public string languageCode {
            get { return _languageCode; }
        }

        readonly string _countryCode;

        public string countryCode {
            get { return _countryCode; }
        }

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

    public abstract class Window {
        public static Window instance {
            get {
                D.assert(_instance != null,
                    () => "Window.instance is null. " +
                    "This usually happens when there is a callback from outside of UIWidgets. " +
                    "Try to use \"using (WindowProvider.of(BuildContext).getScope()) { ... }\" to wrap your code.");
                return _instance;
            }

            set {
                if (value == null) {
                    D.assert(_instance != null, () => "Window.instance is already cleared.");
                    _instance = null;
                }
                else {
                    D.assert(_instance == null, () => "Window.instance is already assigned.");
                    _instance = value;
                }
            }
        }

        public static bool hasInstance {
            get { return _instance != null; }
        }

        internal static Window _instance;
        
        public const int defaultAntiAliasing = 4;

        public float devicePixelRatio {
            get { return _devicePixelRatio; }
        }

        protected float _devicePixelRatio = 1.0f;
        
        public int antiAliasing {
            get { return _antiAliasing; }
        }

        protected int _antiAliasing = defaultAntiAliasing;

        public Size physicalSize {
            get { return _physicalSize; }
        }

        public WindowConfig windowConfig = WindowConfig.defaultConfig;

        protected Size _physicalSize = Size.zero;

        public WindowPadding viewInsets {
            get { return _viewInsets; }
        }

        protected WindowPadding _viewInsets = WindowPadding.zero;

        public WindowPadding padding {
            get { return _padding; }
        }

        protected WindowPadding _padding = WindowPadding.zero;

        public VoidCallback onMetricsChanged {
            get { return _onMetricsChanged; }
            set { _onMetricsChanged = value; }
        }

        VoidCallback _onMetricsChanged;

        public Locale locale {
            get {
                if (_locales != null && _locales.isNotEmpty()) {
                    return _locales[0];
                }

                return null;
            }
        }

        public List<Locale> locales {
            get { return _locales; }
        }

        protected List<Locale> _locales;

        public VoidCallback onLocaleChanged {
            get { return _onLocaleChanged; }
            set { _onLocaleChanged = value; }
        }

        VoidCallback _onLocaleChanged;

        public float textScaleFactor {
            get { return _textScaleFactor; }
        }

        protected float _textScaleFactor = 1.0f;

        public VoidCallback onTextScaleFactorChanged {
            get { return _onTextScaleFactorChanged; }
            set { _onTextScaleFactorChanged = value; }
        }

        VoidCallback _onTextScaleFactorChanged;

        public VoidCallback onPlatformBrightnessChanged {
            get { return _onPlatformBrightnessChanged; }
            set { _onPlatformBrightnessChanged = value; }
        }

        VoidCallback _onPlatformBrightnessChanged;

        public FrameCallback onBeginFrame {
            get { return _onBeginFrame; }
            set { _onBeginFrame = value; }
        }

        FrameCallback _onBeginFrame;

        public VoidCallback onDrawFrame {
            get { return _onDrawFrame; }
            set { _onDrawFrame = value; }
        }

        VoidCallback _onDrawFrame;

        public PointerDataPacketCallback onPointerEvent {
            get { return _onPointerEvent; }
            set { _onPointerEvent = value; }
        }

        PointerDataPacketCallback _onPointerEvent;

        public abstract void scheduleFrame(bool regenerateLayerTree = true);

        public abstract void render(Scene scene);

        public abstract void scheduleMicrotask(Action callback);

        public abstract void flushMicrotasks();

        public abstract Timer run(TimeSpan duration, Action callback, bool periodic = false);

        public Timer periodic(TimeSpan duration, Action callback) {
            return run(duration, callback, true);
        }

        public Timer run(Action callback) {
            return run(TimeSpan.Zero, callback);
        }

        public abstract Timer runInMain(Action callback);

        public abstract IDisposable getScope();


        float fpsDeltaTime;

        public void updateFPS(float unscaledDeltaTime) {
            fpsDeltaTime += (unscaledDeltaTime - fpsDeltaTime) * 0.1f;
        }

        public float getFPS() {
            return 1.0f / fpsDeltaTime;
        }

        public const int defaultMaxTargetFrameRate = 60;
        public const int defaultMinTargetFrameRate = 25;
        public const int defaultMaxRenderFrameInterval = 100;
        public const int defaultMinRenderFrameInterval = 1;

        static Action _onFrameRateSpeedUp = defaultFrameRateSpeedUp;

        public static Action onFrameRateSpeedUp {
            get { return _onFrameRateSpeedUp; }
            set {
                if (value == null) {
                    _onFrameRateSpeedUp = defaultFrameRateSpeedUp;
                }
                else {
                    _onFrameRateSpeedUp = value;
                }
            }
        }

        static void defaultFrameRateSpeedUp() {
#if UNITY_2019_3_OR_NEWER
            OnDemandRendering.renderFrameInterval = defaultMinRenderFrameInterval;
#else
            Application.targetFrameRate = defaultMaxTargetFrameRate;
#endif
        }

        static Action _onFrameRateCoolDown = defaultFrameRateCoolDown;

        public static Action onFrameRateCoolDown {
            get { return _onFrameRateCoolDown; }
            set {
                if (value == null) {
                    _onFrameRateCoolDown = defaultFrameRateCoolDown;
                }
                else {
                    _onFrameRateCoolDown = value;
                }
            }
        }

        static void defaultFrameRateCoolDown() {
#if UNITY_2019_3_OR_NEWER
            OnDemandRendering.renderFrameInterval = defaultMaxRenderFrameInterval;
#else
            Application.targetFrameRate = defaultMinTargetFrameRate;
#endif
        }
    }
}