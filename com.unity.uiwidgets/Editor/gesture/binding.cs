using System;
using System.Collections.Generic;
using Unity.UIWidgets.gestures;
using Unity.UIWidgets.ui;

namespace Unity.UIWidgets.widgets {
    public class UiWidgetsEditorBinding : UiWidgetsBinding {
        public new static UiWidgetsEditorBinding instance {
            get { return (UiWidgetsEditorBinding) UiWidgetsBinding.instance; }
            set { UiWidgetsBinding.instance = value; }
        }
        
        public static UiWidgetsEditorBinding ensureInitializedForEditor() {
            if (UiWidgetsEditorBinding.instance == null) {
                return new UiWidgetsEditorBinding();
            }
            
            return UiWidgetsEditorBinding.instance;
        }
        
        public EditorMouseTracker editorMouseTracker {
            get { return _editorMouseTracker; }
        }

        EditorMouseTracker _editorMouseTracker;

        private void initEditorMouseTracker(EditorMouseTracker tracker  = null) {
            _editorMouseTracker?.dispose();
            _editorMouseTracker = tracker ?? new EditorMouseTracker(pointerRouter, hitTestMouseTrackers);
        }

        public EditorMouseTrackerAnnotation hitTestMouseTrackers(Offset position) {
            List<EditorMouseTrackerAnnotation> annotations = 
                new List<EditorMouseTrackerAnnotation>(renderView.layer.findAllAnnotations<EditorMouseTrackerAnnotation>(
                position * renderView.configuration.devicePixelRatio
            ).annotations);

            if (annotations is null || annotations.Count == 0) {
                return null;
            }

            return annotations[0];
        }

        protected override void initInstances() {
            base.initInstances();
            
            addPersistentFrameCallback(_handlePersistentFrameCallbackForEditor);
            initEditorMouseTracker();
        }

        private void _handlePersistentFrameCallbackForEditor(TimeSpan timeStamp) {
            _editorMouseTracker.schedulePostFrameCheck();
        }
    }
    
    public static class editor_ui_ {
        public static void runEditorApp(Widget app) {
            var instance = UiWidgetsEditorBinding.ensureInitializedForEditor();
            instance.scheduleAttachRootWidget(app);
            instance.scheduleWarmUpFrame();
        }
    }
}