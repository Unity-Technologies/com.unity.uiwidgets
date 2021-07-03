using System.Collections;
using System.Collections.Generic;
using Unity.UIWidgets.async;

namespace developer {
    public static class developer_ {
        public static void postEvent(string eventKind, Hashtable eventData) {
        }
        
        static Dictionary<string, DeveloperExtensionFunc> extensions = new Dictionary<string, DeveloperExtensionFunc>();
        
        public delegate Future<IDictionary<string, object>> DeveloperExtensionFunc(string method, IDictionary<string, string> parameters);

        public static void registerExtension(string methodName, DeveloperExtensionFunc func) {
            extensions[methodName] = func;
        }

        public static Future<IDictionary<string, object>> callExtension(string method,
            IDictionary<string, string> parameters) {
            if (extensions.ContainsKey(method)) {
                return extensions[method].Invoke(method, parameters);
            }

            return null;
        }
    }
}