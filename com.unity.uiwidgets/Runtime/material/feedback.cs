using Unity.UIWidgets.async2;
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
                    return
                        Future.value(); // SystemSound.play(SystemSoundType.click); TODO: replace with unity equivalent
                default:
                    return Future.value();
            }
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
                default:
                    return Future.value();
            }
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