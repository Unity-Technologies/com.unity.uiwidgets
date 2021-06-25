using Unity.UIWidgets.DevTools.framework;
using UnityEngine;

namespace Unity.UIWidgets.DevTools.config_specific.framework_initialize
{
    public static class FrameworkInitializeUtils
    {
        public static void initializeFramework()
        {
            FrameworkCore.initGlobals();
        }
    }
}