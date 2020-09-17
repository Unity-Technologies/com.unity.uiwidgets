using UnityEngine;

namespace Unity.UIWidgets.foundation {
    public static partial class foundation_ {
        public static readonly bool kReleaseMode = !Debug.isDebugBuild;

#if UIWidgets_PROFILE
        public const bool kProfileMode = true;
#else
        public const bool kProfileMode = false;
#endif

        public static readonly bool kDebugMode = !kReleaseMode && !kProfileMode;

        public const float precisionErrorTolerance = 1e-10f;

#if UNITY_WEBGL
        public const bool kIsWeb = true;
#else
        public const bool kIsWeb = false;
#endif
    }
}