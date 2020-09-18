#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using RSG.Promises;
using Unity.UIWidgets.editor;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.widgets;
using UnityEditor;
using UnityEngine;

namespace Unity.UIWidgets.debugger {
    public class WidgetsInpsectorWindow : EditorWindow {

        const float debugPaintToggleGroupWidth = 120;
        
        const float debugPaintToggleGroupHeight = 100;
        
        InspectorService m_InspectorService;
        bool m_ShowInspect;
        
        [SerializeField]
        bool m_DebugPaint;
        
        [SerializeField]
        bool m_DebugPaintSize;
        
        [SerializeField]
        bool m_DebugPaintBaseline;
        
        [SerializeField]
        bool m_DebugPaintPointer;
        
        [SerializeField]
        bool m_DebugPaintLayer;

        bool m_ShowDebugPaintToggles;

        GUIStyle m_MessageStyle;
        readonly List<InspectorPanel> m_Panels = new List<InspectorPanel>();

        Rect m_DebugPaintTogglesRect;
        int m_PanelIndex = 0;
        [SerializeField] List<PanelState> m_PanelStates = new List<PanelState>();

        List<Action> m_UpdateActions = new List<Action>();
        
        [MenuItem("Window/Analysis/UIWidgets Inspector")]
        public static void Init() {
            WidgetsInpsectorWindow window =
                (WidgetsInpsectorWindow) GetWindow(typeof(WidgetsInpsectorWindow));
            window.Show();
        }

        void OnEnable() {
            titleContent = new GUIContent("UIWidgets Inspector");
        }

        void OnGUI() {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar, GUILayout.ExpandWidth(true));
            DoSelectDropDown();
            bool needDebugPaintUpdate = false;
            
            if (m_InspectorService != null && m_InspectorService.debugEnabled) {
                if (GUILayout.Button("Refersh", EditorStyles.toolbarButton)) {
                    foreach (var panel in m_Panels) {
                        panel.MarkNeedReload();
                    }
                }
                
                EditorGUI.BeginChangeCheck();
                var newShowInspect = GUILayout.Toggle(m_ShowInspect, new GUIContent("Inspect Element"),
                    EditorStyles.toolbarButton);
                if (EditorGUI.EndChangeCheck()) {
                    m_InspectorService.setShowInspect(newShowInspect);
                }

                var style = (GUIStyle) "GV Gizmo DropDown";
                Rect r = GUILayoutUtility.GetRect(new GUIContent("Debug Paint"), style);
                Rect rightRect = new Rect(r.xMax - style.border.right, r.y, style.border.right, r.height);
                if (EditorGUI.DropdownButton(rightRect, GUIContent.none, FocusType.Passive, GUIStyle.none))
                {
                    ScheduleUpdateAction(() => {
                        m_ShowDebugPaintToggles = !m_ShowDebugPaintToggles;
                        Repaint();
                    });
                }

                if (Event.current.type == EventType.Repaint) {
                    m_DebugPaintTogglesRect = new Rect(r.xMax - debugPaintToggleGroupWidth, r.yMax + 2, 
                        debugPaintToggleGroupWidth, debugPaintToggleGroupHeight);
                }

                EditorGUI.BeginChangeCheck();
                m_DebugPaint = GUI.Toggle(r, m_DebugPaint, new GUIContent("Debug Paint"), style);
                if (EditorGUI.EndChangeCheck()) {
                    if (m_DebugPaint) {
                        if (!m_DebugPaintSize && !m_DebugPaintLayer
                                                   && !m_DebugPaintPointer && !m_DebugPaintBaseline) {
                            m_DebugPaintSize = true;
                        }
                    }
                    needDebugPaintUpdate = true;
                }
            }

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space();

            if (m_InspectorService != null && m_InspectorService .debugEnabled) {
                EditorGUILayout.BeginHorizontal(EditorStyles.toolbar, GUILayout.ExpandWidth(false));
                m_Panels.Each((pannel, index) => {
                    if (GUILayout.Toggle(m_PanelIndex == index, pannel.title, EditorStyles.toolbarButton,
                        GUILayout.ExpandWidth(false), GUILayout.Width(100))) {
                        m_PanelIndex = index;
                    }
                });
                EditorGUILayout.EndHorizontal();

                bool shouldHandleGUI = true;
                if (Event.current.type == EventType.MouseDown || Event.current.type == EventType.MouseUp) {
                    if (m_ShowDebugPaintToggles && m_DebugPaintTogglesRect.Contains(Event.current.mousePosition)) {
                        shouldHandleGUI = false;
                    }
                }

                if (shouldHandleGUI) {
                    m_Panels[m_PanelIndex].OnGUI();
                }
            } else if (m_InspectorService != null) { // debug not enabled
                if (m_MessageStyle == null) {
                    m_MessageStyle = new GUIStyle(GUI.skin.label);
                    m_MessageStyle.fontSize = 16;
                    m_MessageStyle.alignment = TextAnchor.MiddleCenter;
                    m_MessageStyle.padding = new RectOffset(20, 20, 40, 0);
                }
                GUILayout.Label("You're not in UIWidgets Debug Mode.\nPlease define UIWidgets_DEBUG " +
                                "symbols at \"Player Settings => Scripting Define Symbols\".",
                    m_MessageStyle, GUILayout.ExpandWidth(true));
            }
            
           if (m_ShowDebugPaintToggles) {
               DebugPaintToggles(ref needDebugPaintUpdate);
           }

           if (needDebugPaintUpdate) {
               D.setDebugPaint(
                   debugPaintSizeEnabled: m_DebugPaint && m_DebugPaintSize, 
                   debugPaintBaselinesEnabled: m_DebugPaint && m_DebugPaintBaseline,
                   debugPaintPointersEnabled: m_DebugPaint && m_DebugPaintPointer,
                   debugPaintLayerBordersEnabled: m_DebugPaint && m_DebugPaintLayer,
                   debugRepaintRainbowEnabled: m_DebugPaint && m_DebugPaintLayer
                   );
           }
        }

        void DebugPaintToggles(ref bool needUpdate) {
            GUILayout.BeginArea(m_DebugPaintTogglesRect, GUI.skin.box);
            GUILayout.BeginVertical();
            EditorGUI.BeginChangeCheck();
            GUILayout.Space(4);
            m_DebugPaintSize = GUILayout.Toggle(m_DebugPaintSize, new GUIContent("Paint Size"));
            m_DebugPaintBaseline = GUILayout.Toggle(m_DebugPaintBaseline, new GUIContent("Paint Baseline"));
            m_DebugPaintPointer = GUILayout.Toggle(m_DebugPaintPointer, new GUIContent("Paint Pointer"));
            m_DebugPaintLayer  = GUILayout.Toggle(m_DebugPaintLayer, new GUIContent("Paint Layer"));
            if (EditorGUI.EndChangeCheck()) {
                needUpdate = true;
            }
            GUILayout.EndVertical();
            GUILayout.EndArea();

            if (Event.current.type == EventType.MouseDown && 
                !m_DebugPaintTogglesRect.Contains(Event.current.mousePosition)) {
                ScheduleUpdateAction(() => {
                    m_ShowDebugPaintToggles = false;
                    Repaint();
                });
            }
        }
        
        void DoSelectDropDown() {
            var currentWindow = m_InspectorService == null ? null : m_InspectorService.window;
            if (currentWindow != null && !currentWindow.alive) {
                currentWindow = null;
            }
            
            var selectTitle = currentWindow != null ? currentWindow.titleContent : new GUIContent("<Please Select>");
            if (GUILayout.Button(selectTitle, EditorStyles.toolbarDropDown)) {
                var windows = new List<WindowAdapter>(WindowAdapter.windowAdapters.Where(w => {
                    return w.withBindingFunc(() => WidgetsBinding.instance.renderViewElement != null);
                }));
                Rect rect = EditorGUILayout.GetControlRect(false, EditorGUIUtility.singleLineHeight,
                    EditorStyles.toolbarDropDown);
                var menuPos = EditorGUI.IndentedRect(rect);
                menuPos.y += EditorGUIUtility.singleLineHeight / 2;

                int selectedIndex = 0;
                var labels = new GUIContent[windows.Count + 1];
                labels[0] = new GUIContent("none");
                for (int i = 0; i < windows.Count; i++) {
                    labels[i + 1] = windows[i].titleContent;
                    if (windows[i] == currentWindow) {
                        selectedIndex = i + 1;
                    }
                }

                EditorUtility.DisplayCustomMenu(menuPos, labels, selectedIndex, (data, options, selected) => {
                    if (selected > 0) {
                        var selectedWindow = windows[selected - 1];
                        if (selectedWindow != currentWindow) {
                            inspect(selectedWindow);
                        }
                    }
                    else {
                        if (m_InspectorService != null) {
                            closeInspect();
                        }
                    }
                }, null);
            }
        }

        void inspect(WindowAdapter window) {
            if (m_InspectorService != null) // stop previous inspect
            {
                closeInspect();
            }

            m_InspectorService = new InspectorService(window);
            m_PanelIndex = 0;

            var state = m_PanelStates.Find((s) => s.treeType == WidgetTreeType.Widget);
            m_Panels.Add(new InspectorPanel(this, WidgetTreeType.Widget, m_InspectorService,
                state == null ? (float?) null : state.splitOffset));

            state = m_PanelStates.Find((s) => s.treeType == WidgetTreeType.Render);
            m_Panels.Add(new InspectorPanel(this, WidgetTreeType.Render, m_InspectorService,
                state == null ? (float?) null : state.splitOffset));
        }

        void closeInspect() {
            if (m_InspectorService == null) {
                return;
            }

            m_InspectorService.close();
            m_InspectorService = null;
            foreach (var panel in m_Panels) {
                panel.Close();
            }

            m_Panels.Clear();
            m_ShowInspect = false;
            m_ShowDebugPaintToggles = false;
        }

        void ScheduleUpdateAction(Action action) {
            m_UpdateActions.Add(action);
        }

        void Update() {
            if (m_InspectorService != null && !m_InspectorService.active) {
                closeInspect();
                Repaint();
            }

            bool showInspect = false;
            if (m_InspectorService != null) {
                showInspect = m_InspectorService.getShowInspect();
            }

            if (showInspect != m_ShowInspect) {
                Repaint();
            }

            m_ShowInspect = showInspect;

            for (int i = 0; i < m_Panels.Count; i++) {
                m_Panels[i].visibleToUser = m_PanelIndex == i;
                m_Panels[i].Update();
            }

            if (m_Panels.Count > 0) {
                m_PanelStates = m_Panels.Select(p => p.PanelState).ToList();
            }

            while (m_UpdateActions.Count > 0) {
                m_UpdateActions[0]();
                m_UpdateActions.RemoveAt(0);
            }
        }


        void OnDestroy() {
            closeInspect();
        }
    }
}
#endif