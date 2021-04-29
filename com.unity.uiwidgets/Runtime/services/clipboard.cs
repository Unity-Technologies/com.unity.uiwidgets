using System.Runtime.InteropServices;
using Unity.UIWidgets.async;
using UnityEngine;

namespace Unity.UIWidgets.service {
    public class ClipboardData {
        public ClipboardData(string text = null) {
            this.text = text;
        }

        public readonly string text;
    }

    public abstract class Clipboard {
        static readonly Clipboard _instance = new UnityGUIClipboard();

        public static readonly string kTextPlain = "text/plain";

        public static Future setData(ClipboardData data) {
            return _instance.setClipboardData(data);
        }

        public static Future<ClipboardData> getData(string format) {
            return _instance.getClipboardData(format);
        }

        protected abstract Future setClipboardData(ClipboardData data);
        protected abstract Future<ClipboardData> getClipboardData(string format);
    }

    public class UnityGUIClipboard : Clipboard {
        protected override Future setClipboardData(ClipboardData data) {
#if UNITY_WEBGL
            UIWidgetsCopyTextToClipboard(data.text);
#else
            GUIUtility.systemCopyBuffer = data.text;
#endif
            
            return Future.value();
        }

        protected override Future<ClipboardData> getClipboardData(string format) {
            var data = new ClipboardData(text: GUIUtility.systemCopyBuffer);
            return Future.value(FutureOr.value(data)).to<ClipboardData>();
        }
        
#if UNITY_WEBGL
        [DllImport ("__Internal")]
        internal static extern void UIWidgetsCopyTextToClipboard(string text);
#endif
    }
}