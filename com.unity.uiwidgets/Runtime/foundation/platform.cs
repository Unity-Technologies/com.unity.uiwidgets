using System.Collections.Generic;
using Unity.UIWidgets.animation;
using Unity.UIWidgets.cupertino;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.service;
using Unity.UIWidgets.ui;
using Unity.UIWidgets.widgets;
using Color = Unity.UIWidgets.ui.Color;
using Rect = Unity.UIWidgets.ui.Rect;
using TextStyle = Unity.UIWidgets.painting.TextStyle;
using Brightness = Unity.UIWidgets.ui.Brightness;

namespace Unity.UIWidgets.foundation {
    public enum TargetPlatform {
        /// Android: <https://www.android.com/>
        android,

        /// Fuchsia: <https://fuchsia.googlesource.com/>
        fuchsia,

        /// iOS: <https://www.apple.com/ios/>
        iOS,

        /// Linux: <https://www.linux.org>
        linux,

        /// macOS: <https://www.apple.com/macos>
        macOS,

        /// Windows: <https://www.windows.com>
        windows,
    }

    public class PlatformUtils {
       /* public TargetPlatform defaultTargetPlatform {
            get {
                return _platform.defaultTargetPlatform;
        
            }
        }*/
    }


}