using UnityEngine;

namespace Unity.UIWidgets.uiOld{
    class Utils {
        public static float PixelCorrectRound(float v) {
            return Mathf.Round(v * Window.instance.devicePixelRatio) / Window.instance.devicePixelRatio;
        }
    }
}