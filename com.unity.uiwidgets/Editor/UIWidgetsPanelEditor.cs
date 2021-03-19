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
            var pixelRatioProperty = serializedObject.FindProperty("devicePixelRatioOverride");
            var antiAliasingProperty = serializedObject.FindProperty("hardwareAntiAliasing");
            EditorGUILayout.PropertyField(serializedObject.FindProperty("fonts"), true);
            EditorGUILayout.PropertyField(pixelRatioProperty);
            EditorGUILayout.PropertyField(antiAliasingProperty);
            EditorGUI.BeginChangeCheck();
            UIWidgetsPanel panel = (UIWidgetsPanel)target; 
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