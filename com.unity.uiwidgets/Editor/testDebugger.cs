using System;
using System.Collections.Generic;
using System.Text;
using developer;
using UnityEditor;
using UnityEngine;

namespace Unity.UIWidgets.Editor {
    public class testDebugger : EditorWindow{
        [MenuItem("Window/TestInspector")]
        static void Init() {
            var myWindow = (testDebugger) EditorWindow.GetWindow(typeof(testDebugger));
            myWindow.Show();
;        }

        static void ShowJsonMap(IDictionary<string, object> jsonMap) {
            StringBuilder sb = new StringBuilder();
            foreach (var key in jsonMap.Keys) {
                if (key == "result" && jsonMap[key] is Dictionary<string ,object>) {
                    var resultMap = jsonMap[key] as Dictionary<string, object>;
                    foreach (var ckey in resultMap.Keys) {
                        sb.Append($"result.{ckey} : {resultMap[ckey] ?? "Null"}\n");
                    }
                }
                sb.Append($"{key} : {jsonMap[key] ?? "Null"}\n");
            }
            Debug.Log(sb.ToString());
        }

        void OnGUI() {
            if (GUILayout.Button("getRootWidget")) {
                var result = developer_.callExtension("inspector.getRootWidget", new Dictionary<string, string> {
                    {"objectGroup", "root"}
                });

                result.then(o => {
                    var res = (IDictionary<string, object>) o;
                    ShowJsonMap(res);
                });
            }
            if (GUILayout.Button("getRootRenderObject")) {
                var result = developer_.callExtension("inspector.getRootRenderObject", new Dictionary<string, string> {
                    {"objectGroup", "root"}
                });

                result.then(o => {
                    var res = (IDictionary<string, object>) o;
                    ShowJsonMap(res);
                });
            }
            if (GUILayout.Button("getRootWidgetSummaryTree")) {
                var result = developer_.callExtension("inspector.getRootWidgetSummaryTree", new Dictionary<string, string> {
                    {"objectGroup", "root"}
                });

                result.then(o => {
                    var res = (IDictionary<string, object>) o;
                    ShowJsonMap(res);
                });
            }
            if (GUILayout.Button("getChildren")) {
                var result = developer_.callExtension("inspector.getChildren", new Dictionary<string, string> {
                    {"objectGroup", "root"},
                    {"arg", "inspector-0"}
                });

                result.then(o => {
                    var res = (IDictionary<string, object>) o;
                    ShowJsonMap(res);
                });
            }
        }
    }
}