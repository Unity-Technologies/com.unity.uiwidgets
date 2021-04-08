using System.Runtime.InteropServices;
using UnityEngine;

namespace Unity.UIWidgets.engine2 {
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

#if UNITY_IOS
        [DllImport("__Internal")]
        static extern float IOSDeviceScaleFactor();

//         [DllImport(“__Internal”)]
//         static extern viewMetrics IOSGetViewportPadding();
#endif

    }
}