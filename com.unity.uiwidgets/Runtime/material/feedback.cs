using Unity.UIWidgets.async;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.gestures;
using Unity.UIWidgets.widgets;
using UnityEngine;

namespace Unity.UIWidgets.material {
    public class Feedback {
        Feedback() {
        }

        public static Future forTap(BuildContext context) {
            switch (_platform(context)) {
                case RuntimePlatform.Android:
                    return Future.value(); // SystemSound.play(SystemSoundType.click); TODO: replace with unity equivalent
                case RuntimePlatform.IPhonePlayer:
                case RuntimePlatform.LinuxEditor:
                case RuntimePlatform.LinuxPlayer:
                case RuntimePlatform.OSXEditor:
                case RuntimePlatform.OSXPlayer:
                case RuntimePlatform.WindowsEditor:
                case RuntimePlatform.WindowsPlayer:
                    return Future.value();
            }

            D.assert(false, () => $"Unhandled TargetPlatform {_platform(context)}");
            return Future.value();
        }

        public static GestureTapCallback wrapForTap(GestureTapCallback callback, BuildContext context) {
            if (callback == null) {
                return null;
            }

            return () => {
                forTap(context);
                callback();
            };
        }

        public static Future forLongPress(BuildContext context) {
            switch (_platform(context)) {
                case RuntimePlatform.Android:
                    return Future.value(); // HapticFeedback.vibrate(); TODO
                case RuntimePlatform.IPhonePlayer:
                case RuntimePlatform.LinuxEditor:
                case RuntimePlatform.LinuxPlayer:
                case RuntimePlatform.OSXEditor:
                case RuntimePlatform.OSXPlayer:
                case RuntimePlatform.WindowsEditor:
                case RuntimePlatform.WindowsPlayer:
                    return Future.value();
            }
            D.assert(false, ()=>$"Unhandled TargetPlatform {_platform(context)}");
            return Future.value();
        }

        public static GestureLongPressCallback
            wrapForLongPress(GestureLongPressCallback callback, BuildContext context) {
            if (callback == null) {
                return null;
            }

            return () => {
                forLongPress(context);
                callback();
            };
        }

        static RuntimePlatform _platform(BuildContext context) {
            return Theme.of(context).platform;
        }
    }
}