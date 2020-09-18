using System.Collections.Generic;
using System.Linq;
using Unity.UIWidgets.editor;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.widgets;

namespace Unity.UIWidgets.debugger {
    public delegate void SelectionChanged();

    public class InspectorService {
        public readonly WindowAdapter window;

        public SelectionChanged selectionChanged;

        readonly WidgetInspectorService _widgetInspectorService;

        public InspectorService(WindowAdapter window) {
            this.window = window;
            _widgetInspectorService = window.widgetInspectorService;
            _widgetInspectorService.developerInspect += developerInspect; // todo dispose
        }

        public bool active {
            get { return window.alive; }
        }

        public bool debugEnabled {
            get { return foundation_.kDebugMode; }
        }

        public void close() {
            setShowInspect(false);
            _widgetInspectorService.developerInspect -= developerInspect;
        }

        public DiagnosticsNode getRootWidgetSummaryTree(string groupName) {
            return toNode(window.withBindingFunc(() =>
                _widgetInspectorService.getRootWidgetSummaryTree(groupName)));
        }

        public DiagnosticsNode getRootWidget(string groupName) {
            return toNode(window.withBindingFunc(() =>
                _widgetInspectorService.getRootWidget(groupName)));
        }

        public DiagnosticsNode getRootRenderObject(string groupName) {
            return toNode(window.withBindingFunc(() =>
                _widgetInspectorService.getRootRenderObject(groupName)));
        }

        public DiagnosticsNode getDetailsSubtree(InspectorInstanceRef instanceRef, string groupName) {
            return toNode(window.withBindingFunc(() =>
                _widgetInspectorService.getDetailsSubtree(instanceRef.id, groupName)));
        }

        public DiagnosticsNode getSelection(DiagnosticsNode previousSelection, WidgetTreeType treeType, bool localOnly,
            string groupName) {
            InspectorInstanceRef previousSelectionRef =
                previousSelection == null ? null : previousSelection.diagnosticRef;
            string previousSelectionId = previousSelectionRef == null ? null : previousSelectionRef.id;
            DiagnosticsNode result = null;

            window.withBinding(() => {
                switch (treeType) {
                    case WidgetTreeType.Widget:
                        result = localOnly
                            ? toNode(
                                _widgetInspectorService.getSelectedSummaryWidget(previousSelectionId, groupName))
                            : toNode(
                                _widgetInspectorService.getSelectedWidget(previousSelectionId, groupName));
                        break;
                    case WidgetTreeType.Render:
                        result = toNode(
                            _widgetInspectorService.getSelectedRenderObject(previousSelectionId, groupName));
                        break;
                }
            });


            if (result != null && result.diagnosticRef == previousSelectionRef) {
                return previousSelection;
            }
            else {
                return result;
            }
        }

        public bool setSelection(InspectorInstanceRef inspectorInstanceRef, string groupName) {
            return window.withBindingFunc(() =>
                _widgetInspectorService.setSelectionById(inspectorInstanceRef.id, groupName));
        }

        public void setShowInspect(bool show) {
            window.withBinding(() => { _widgetInspectorService.debugShowInspector = show; });
        }

        public bool getShowInspect() {
            return window.withBindingFunc(() => _widgetInspectorService.debugShowInspector);
        }

        public List<DiagnosticsNode> getProperties(InspectorInstanceRef inspectorInstanceRef, string groupName) {
            var list = window.withBindingFunc(() =>
                _widgetInspectorService.getProperties(inspectorInstanceRef.id, groupName));
            return list.Select(json => toNode(json, isProperty: true)).ToList();
        }

        public void disposeGroup(string groupName) {
            window.withBinding(() => _widgetInspectorService.disposeGroup(groupName));
        }

        DiagnosticsNode toNode(Dictionary<string, object> json, bool isProperty = false) {
            if (json == null) {
                return null;
            }

            return new DiagnosticsNode(json, isProperty);
        }


        void developerInspect() {
            if (selectionChanged != null) {
                selectionChanged();
            }
        }
    }

    public enum WidgetTreeType {
        Widget,
        Render
    }
}