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
            var ShowDebugLog = serializedObject.FindProperty("m_ShowDebugLog");
            
            EditorGUILayout.PropertyField(serializedObject.FindProperty("fonts"), true);

            EditorGUILayout.PropertyField(pixelRatioProperty);
            EditorGUILayout.PropertyField(antiAliasingProperty);
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(ShowDebugLog);
            if (EditorGUI.EndChangeCheck()) {
                UIWidgetsPanel.ShowDebugLog = ShowDebugLog.boolValue;
            }
            UIWidgetsPanel panel = (UIWidgetsPanel)target;
            
            serializedObject.ApplyModifiedProperties(); 
        }
    }
}