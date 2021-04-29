using Unity.UIWidgets.painting;

namespace Unity.UIWidgets.rendering {
    public static class RenderingDebugUtils {
        public static bool debugDisableShadows = false;
        public static bool debugCheckElevationsEnabled = false;
        public static bool debugRepaintTextRainbowEnabled = false;
        static readonly HSVColor _kDebugDefaultRepaintColor = HSVColor.fromAHSV(0.4f, 60.0f, 1.0f, 1.0f);
        public static HSVColor debugCurrentRepaintColor = _kDebugDefaultRepaintColor;
    }
}