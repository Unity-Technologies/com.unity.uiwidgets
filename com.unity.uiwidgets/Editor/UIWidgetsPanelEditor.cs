using Unity.UIWidgets.engine;
using Unity.UIWidgets.engine2;
using Unity.UIWidgets.foundation;
using UnityEditor;
using UnityEditor.UI;
using UnityEngine;

namespace Unity.UIWidgets.Editor {
    [CustomEditor(typeof(UIWidgetsPanel), true)]
    [CanEditMultipleObjects]
    public class UIWidgetsPanelEditor : RawImageEditor {
        
        public override void OnInspectorGUI() {
            base.OnInspectorGUI();
            EditorGUILayout.PropertyField(serializedObject.FindProperty("fonts"), true);
            EditorGUI.BeginChangeCheck();
            UIWidgetsPanel panel = (UIWidgetsPanel)target;
            panel.enableDebugAtRuntime = EditorGUILayout.Toggle(new GUIContent("Enable Debug At Runtime", 
                    "This configuration only works in standalone players. \n\n" +
                    "If your scene contains multiple UIWidgets panels," +
                    " the DEBUG MODE is enabled for all panels if this is checked on any one panel.\n\n" +
                    "In editor the DEBUG MODE can be set using UIWidgets/EnableDebug."),
                panel.enableDebugAtRuntime);
            serializedObject.ApplyModifiedProperties(); 
        }

        [MenuItem("UIWidgets/EnableDebug")]
        public static void ToggleDebugMode(){
            D.enableDebug = !D.enableDebug;
        }
        
        [MenuItem("UIWidgets/EnableDebug",true)]
        public static bool CurrentDebugModeState() {
            Menu.SetChecked("UIWidgets/EnableDebug", D.enableDebug );
            return true;
        }
    }
}