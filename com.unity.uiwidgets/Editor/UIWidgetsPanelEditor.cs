using Unity.UIWidgets.engine;
using Unity.UIWidgets.engine2;
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
            var updateVale = EditorGUILayout.Toggle("Show Debug Log", UIWidgetsPanel.ShowDebugLog);

            if (EditorGUI.EndChangeCheck()) {
                UIWidgetsPanel.ShowDebugLog = updateVale;
            }
            UIWidgetsPanel panel = (UIWidgetsPanel)target;
            
            serializedObject.ApplyModifiedProperties(); 
        }
    }
}