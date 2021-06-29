using System;
using System.Collections.Generic;
using System.ComponentModel;
using Unity.UIWidgets.async;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.widgets;

namespace Unity.UIWidgets.DevTools.inspector
{
    
    public enum FlutterTreeType {
        widget, 
        renderObject

    }
    
    public class ObjectGroup
    {
        public ObjectGroup(
            string debugName,
            InspectorService inspectorService
        )
        {
            groupName = $"{debugName}_${InspectorService.nextGroupId}";
            InspectorService.nextGroupId++;
            
        }
    
        /// Object group all objects in this arena are allocated with.
        public readonly string groupName;
        public readonly InspectorService inspectorService;
        public bool disposed = false;
        
        public Future dispose() {
            // var disposeComplete = invokeVoidServiceMethod("disposeGroup", groupName);
            // disposed = true;
            // return disposeComplete;
            return new SynchronousFuture(null);
        }
        
        
        public Future<RemoteDiagnosticsNode> getRoot(FlutterTreeType type) {
            // There is no excuse to call this method on a disposed group.
            D.assert(!disposed);
            switch (type) {
                case FlutterTreeType.widget:
                    return getRootWidget();
                case FlutterTreeType.renderObject:
                    return getRootRenderObject();
            }
            throw new Exception("Unexpected FlutterTreeType");
        }
        
        public RemoteDiagnosticsNode _getRoot(FlutterTreeType type) {
            // There is no excuse to call this method on a disposed group.
            D.assert(!disposed);
            switch (type) {
                case FlutterTreeType.widget:
                    return _getRootWidget();
                case FlutterTreeType.renderObject:
                    return null;
            }
            throw new Exception("Unexpected FlutterTreeType");
        }
        
        RemoteDiagnosticsNode _getRootWidget() {
            // return invokeServiceMethodReturningNode('getRootWidgetSummaryTree');

            Dictionary<string, object> widgetTree = new Dictionary<string, object>();
            widgetTree["hasChildren"] = true;
            widgetTree["name"] = "inspector";
            widgetTree["children"] = new List<Widget>()
            {
                new Text("text1"),
                new Text("text2"),
                new Text("text3"),
                new Text("text4"),
                new Text("text5"),
            };
            widgetTree["properties"] = new List<object>()
            {
                "properties_1",
                "properties_2",
                "properties_3",
                "properties_4",
                "properties_5",
            };
            return new RemoteDiagnosticsNode(widgetTree,
                inspectorService: FutureOr.value(inspectorService),  
                true,
                null);
        }
        
        
        Future<RemoteDiagnosticsNode> getRootWidget() {
            // return invokeServiceMethodReturningNode('getRootWidgetSummaryTree');

            Dictionary<string, object> widgetTree = new Dictionary<string, object>();
            widgetTree["children"] = new List<Widget>()
            {
                new Text("text1"),
                new Text("text2"),
                new Text("text3"),
                new Text("text4"),
                new Text("text5"),
            };
            widgetTree["properties"] = new List<object>()
            {
                "properties_1",
                "properties_2",
                "properties_3",
                "properties_4",
                "properties_5",
            };
            return Future.value(FutureOr.value(new RemoteDiagnosticsNode(widgetTree,
                inspectorService: FutureOr.value(inspectorService),  
                true,
                null))).to<RemoteDiagnosticsNode>();
        }


        Future<RemoteDiagnosticsNode> getRootRenderObject()
        {
            return Future.value(FutureOr.value(null)).to<RemoteDiagnosticsNode>();
        }
        
        
        public Future<RemoteDiagnosticsNode> getDetailsSubtree(
            RemoteDiagnosticsNode node, 
            int subtreeDepth = 2
        ) {
            if (node == null) return null;
            // var args = new Dictionary<string,object>(){
            //     {"objectGroup", groupName},
            //     {"arg", node.dartDiagnosticRef.id},
            //     {"subtreeDepth", subtreeDepth.ToString()},
            // };
            // return parseDiagnosticsNodeDaemon(invokeServiceMethodDaemonParams(
            //     "getDetailsSubtree",
            //     args
            // ));
            return Future.value(FutureOr.value("enpty")).to<RemoteDiagnosticsNode>();
        }
        
    }
    
    class InspectorObjectGroupManager {
        public InspectorObjectGroupManager(
            InspectorService inspectorService, 
            string debugName)
        {
            this.inspectorService = inspectorService;
            this.debugName = debugName;
        }

        public InspectorService inspectorService;
        public string debugName;
         ObjectGroup _current;
         ObjectGroup _next;
        
        Completer _pendingNext;
        
        // Future<void> pendingUpdateDone {
        //     if (_pendingNext != null) {
        //         return _pendingNext.future;
        //     }
        //     if (_next == null) {
        //         // There is no pending update.
        //         return Future.value();
        //     }
        //
        //     _pendingNext = Completer();
        //     return _pendingNext.future;
        // }
        //
        public ObjectGroup current {
            get
            {
                // _current ??= inspectorService.createObjectGroup(debugName);
                return _current;
            }
            
        }
        
        public ObjectGroup next {
            get
            {
                // _next ??= inspectorService.createObjectGroup(debugName);
                return _next;
            }
            
        }
        //
        // void clear(bool isolateStopped) {
        //     if (isolateStopped) {
        //         // The Dart VM will handle GCing the underlying memory.
        //         _current = null;
        //         _setNextNull();
        //     } else {
        //         clearCurrent();
        //         cancelNext();
        //     }
        // }
        
        public void promoteNext() {
            clearCurrent();
            _current = _next;
            _setNextNull();
        }
        
        void clearCurrent() {
            if (_current != null) {
                _current.dispose();
                _current = null;
            }
        }
        
        public void cancelNext() {
            if (_next != null) {
                _next.dispose();
                _setNextNull();
            }
        }
        
        void _setNextNull() {
            _next = null;
            if (_pendingNext != null) {
                _pendingNext.complete();
                _pendingNext = null;
            }
        }
    }

    public class InspectorService
    {

        public static int nextGroupId = 0;
        public Future<string> inferPubRootDirectoryIfNeeded()
        {
            var group = createObjectGroup("temp");
            var root =  group.getRoot(FlutterTreeType.widget);
            return Future.value("aaa").to<string>();
        }

        ObjectGroup createObjectGroup(string debugName) {
            return new ObjectGroup(debugName, this);
        }
        
        // static Future<InspectorService> create(VmService vmService) async {
        //     assert(_inspectorDependenciesLoaded);
        //     assert(serviceManager.hasConnection);
        //     assert(serviceManager.service != null);
        //     final inspectorLibrary = EvalOnDartLibrary(
        //         inspectorLibraryUriCandidates,
        //         vmService,
        //     );
        //
        //     final libraryRef = await inspectorLibrary.libraryRef.catchError(
        //         (_) => throw FlutterInspectorLibraryNotFound(),
        //         test: (e) => e is LibraryNotFound,
        //     );
        //     final libraryFuture = inspectorLibrary.getLibrary(libraryRef, null);
        //     final library = await libraryFuture;
        //     Future<Set<String>> lookupFunctionNames() async {
        //         for (ClassRef classRef in library.classes) {
        //             if ('WidgetInspectorService' == classRef.name) {
        //                 final classObj = await inspectorLibrary.getClass(classRef, null);
        //                 final functionNames = <String>{};
        //                 for (FuncRef funcRef in classObj.functions) {
        //                     functionNames.add(funcRef.name);
        //                 }
        //                 return functionNames;
        //             }
        //         }
        //         // WidgetInspectorService is not available. Either this is not a Flutter
        //         // application or it is running in profile mode.
        //         return null;
        //     }
        //
        //     final supportedServiceMethods = await lookupFunctionNames();
        //     if (supportedServiceMethods == null) return null;
        //     return InspectorService(
        //         vmService,
        //         inspectorLibrary,
        //         supportedServiceMethods,
        //     );
        // }
        //
        
        
    }
    
    
}