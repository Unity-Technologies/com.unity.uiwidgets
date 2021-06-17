using System.Runtime.InteropServices;
using Unity.UIWidgets.ui;
using UnityEngine;

namespace Unity.UIWidgets.engine {
    [StructLayout(LayoutKind.Sequential)]
    public struct viewMetrics {
        public float insets_top;
        public float insets_bottom;
        public float insets_left;
        public float insets_right;

        public float padding_top;
        public float padding_bottom;
        public float padding_left;
        public float padding_right;
    }

    public class DisplayMetrics {
        float _devicePixelRatioByDefault;

        public float DevicePixelRatioByDefault {
            get {
                if (_devicePixelRatioByDefault > 0) {
                    return _devicePixelRatioByDefault;
                }

#if UNITY_ANDROID
                _devicePixelRatioByDefault = AndroidDevicePixelRatio();
#endif

#if UNITY_IOS
                _devicePixelRatioByDefault = IOSDeviceScaleFactor();
#endif

                if (_devicePixelRatioByDefault <= 0) {
                    _devicePixelRatioByDefault = 1;
                }

                return _devicePixelRatioByDefault;
            }
        }
        
        public WindowPadding viewPadding {
            get {
                return new WindowPadding(viewMetrics.padding_left,
                    viewMetrics.padding_top,
                    viewMetrics.padding_right,
                    viewMetrics.padding_bottom);
            }
        }

        public WindowPadding viewInsets {
            get {
                return new WindowPadding(viewMetrics.insets_left,
                    viewMetrics.insets_top,
                    viewMetrics.insets_right,
                    viewMetrics.insets_bottom);
            }
        }

        private viewMetrics? _viewMetrics;

        public viewMetrics viewMetrics {
            get {
                if (_viewMetrics != null) {
                    return _viewMetrics.Value;
                }

#if UNITY_ANDROID && !UNITY_EDITOR
                using (
                    AndroidJavaClass viewController =
                        new AndroidJavaClass("com.unity.uiwidgets.plugin.UIWidgetsViewController")
                ) {
                    AndroidJavaObject metrics = viewController.CallStatic<AndroidJavaObject>("getMetrics");
                    float insets_bottom = metrics.Get<float>("insets_bottom");
                    float insets_top = metrics.Get<float>("insets_top");
                    float insets_left = metrics.Get<float>("insets_left");
                    float insets_right = metrics.Get<float>("insets_right");
                    float padding_bottom = metrics.Get<float>("padding_bottom");
                    float padding_top = metrics.Get<float>("padding_top");
                    float padding_left = metrics.Get<float>("padding_left");
                    float padding_right = metrics.Get<float>("padding_right");

                    _viewMetrics = new viewMetrics {
                        insets_bottom = insets_bottom,
                        insets_left = insets_left,
                        insets_right = insets_right,
                        insets_top = insets_top,
                        padding_left = padding_left,
                        padding_top = padding_top,
                        padding_right = padding_right,
                        padding_bottom = padding_bottom
                    };
                }
#elif !UNITY_EDITOR && UNITY_IOS
                viewMetrics metrics = IOSGetViewportPadding();
                this._viewMetrics = metrics;
#else
                _viewMetrics = new viewMetrics {
                    insets_bottom = 0,
                    insets_left = 0,
                    insets_right = 0,
                    insets_top = 0,
                    padding_left = 0,
                    padding_top = 0,
                    padding_right = 0,
                    padding_bottom = 0
                };
#endif
                return _viewMetrics.Value;
            }
        }
        
        public void onViewMetricsChanged() {
            //view metrics marks dirty
            _viewMetrics = null;
        }

#if UNITY_ANDROID
        static float AndroidDevicePixelRatio() {
            using (
                AndroidJavaClass unityPlayerClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer")
            ) {
                using (
                    AndroidJavaObject metricsInstance = new AndroidJavaObject("android.util.DisplayMetrics"),
                    activityInstance = unityPlayerClass.GetStatic<AndroidJavaObject>("currentActivity"),
                    windowManagerInstance = activityInstance.Call<AndroidJavaObject>("getWindowManager"),
                    displayInstance = windowManagerInstance.Call<AndroidJavaObject>("getDefaultDisplay")
                ) {
                    displayInstance.Call("getMetrics", metricsInstance);
                    return metricsInstance.Get<float>("density");
                }
            }
        }
#endif

#if UNITY_IOS
        [DllImport("__Internal")]
        static extern float IOSDeviceScaleFactor();

        [DllImport("__Internal")]
        static extern viewMetrics IOSGetViewportPadding();
#endif

    }
}