using System.Collections;
using System.Collections.Generic;
using System.IO;
using Unity.UIWidgets.engine2;
using Unity.UIWidgets.services;
using UnityEngine;

namespace Unity.UIWidgets.foundation {
    public class UIWidgetsConfig {
        const string configFile = "UIWidgetsConfig.json";

        public static void Init() {
            var path = Path.Combine(Application.streamingAssetsPath, configFile);
            if (File.Exists(path)) {
                var bytes = File.ReadAllBytes(path);
                var config = JSONMessageCodec.instance.decodeMessage(bytes);
                if (config is IDictionary dict) {
                    UIWidgetsPanel.Configs = dict;
                    UIWidgetsPanel.ShowDebugLog = (bool)dict["ShowDebugLog"];
                    return;
                }
            }

            UIWidgetsPanel.Configs = new Dictionary<string, object> {
                {nameof(UIWidgetsPanel.ShowDebugLog), UIWidgetsPanel.ShowDebugLog}
            };
            var output = JSONMessageCodec.instance.encodeMessage(UIWidgetsPanel.Configs);
            File.WriteAllBytes(path, output);
        }

        public static void updateConfig(IDictionary config) {
            var path = Path.Combine(Application.streamingAssetsPath, configFile);
            var output = JSONMessageCodec.instance.encodeMessage(UIWidgetsPanel.Configs);
            File.WriteAllBytes(path, output);
        }
    }
}