using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using Unity.UIWidgets.async;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.widgets;
using UnityEngine;
using developer;

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
            this.inspectorService = inspectorService;
        }
    
        /// Object group all objects in this arena are allocated with.
        public readonly string groupName;
        public readonly InspectorService inspectorService;
        public bool disposed = false;
        
        // EvalOnDartLibrary inspectorLibrary => inspectorService.inspectorLibrary;
        
        public Future dispose() {
            // var disposeComplete = invokeVoidServiceMethod("disposeGroup", groupName);
            // disposed = true;
            // return disposeComplete;
            return new SynchronousFuture(null);
        }
        
        bool useDaemonApi => inspectorService.useDaemonApi;
        
        public Future<RemoteDiagnosticsNode> getRoot(FlutterTreeType type) {
            D.assert(!disposed);
            switch (type) {
                case FlutterTreeType.widget:
                    return getRootWidget();
                case FlutterTreeType.renderObject:
                    return getRootRenderObject();
            }
            throw new Exception("Unexpected FlutterTreeType");
        }
        
        Future<RemoteDiagnosticsNode> invokeServiceMethodReturningNode(string methodName) {
            if (disposed) return null;
            if (useDaemonApi)
            {

                return parseDiagnosticsNodeDaemon(invokeServiceMethodDaemon(methodName));
            }
            // else {
            //     return parseDiagnosticsNodeObservatory(
            //         invokeServiceMethodObservatory(methodName));
            // }
            return null;
        }
        
        Future<object> invokeServiceMethodDaemon(string methodName, string objectGroup = null) {
            return invokeServiceMethodDaemonParams(methodName,
            new Dictionary<string, object>(){
                    {"objectGroup",objectGroup ?? groupName}
                }
            );
        }
        
        Future<object> invokeServiceMethodDaemonParams(
            string methodName,
            Dictionary<string, object> _params
        ) {
            
            if (methodName == "getRootWidgetSummaryTree")
            {
                var result = developer_.callExtension("inspector.getRootWidgetSummaryTree", new Dictionary<string, string> {
                    {"objectGroup", "root"}
                });

                return result.then_<object>(o =>
                {
                    if (!o.ContainsKey("result"))
                    {
                        return new SynchronousFuture<object>(null);
                    }
                    var res = (Dictionary<string, object>) o["result"];
                    return new SynchronousFuture<object>(res);
                });
                
            }
            
            if (methodName == "getRootRenderObject")
            {
                return getRootRenderObjectFake();
            }

            
            var callMethodName = $"ext.flutter.inspector.{methodName}";
            if (!Globals.serviceManager.serviceExtensionManager
                .isServiceExtensionAvailable(callMethodName)) {
                return null;
            }
            
            return _callServiceExtension(callMethodName, _params);
        }
        
        Future<object> _callServiceExtension(
            string extension, Dictionary<string, object> args) {
            if (disposed)
            {
                return new SynchronousFuture<object>(null);
            }

            return null;
            // return inspectorLibrary.addRequest(this, () => {
            //     var r = inspectorService.vmService.callServiceExtension(
            //         extension,
            //         isolateId: inspectorService.inspectorLibrary.isolateId,
            //         args: args
            //     );
            //     if (disposed) return null;
            //     var json = r.json;
            //     if (json["errorMessage"] != null) {
            //         throw new Exception($"{extension} -- {json["errorMessage"]}");
            //     }
            //     return json["result"];
            // });
        }
        
        List<RemoteDiagnosticsNode> parseDiagnosticsNodesHelper(
            List<object> jsonObject, RemoteDiagnosticsNode parent) {
            if (disposed || jsonObject == null) return null;
            List<RemoteDiagnosticsNode> nodes = new List<RemoteDiagnosticsNode>();
            foreach (Dictionary<string, object> element in jsonObject) {
                nodes.Add(new RemoteDiagnosticsNode(element, FutureOr.value(this), false, parent));
            }
            return nodes;
        }
        
        Future<List<RemoteDiagnosticsNode>> parseDiagnosticsNodesDaemon(
            Future<object> jsonFuture, RemoteDiagnosticsNode parent) {
            if (disposed || jsonFuture == null) return null;

            return jsonFuture.then_<List<RemoteDiagnosticsNode>>((v) =>
            {
                return new SynchronousFuture<List<RemoteDiagnosticsNode>>(parseDiagnosticsNodesHelper((List<object>)v, parent));
            }); 
        }
        
        Future<object> invokeServiceMethodDaemonInspectorRef(
            string methodName, InspectorInstanceRef arg) {
            return invokeServiceMethodDaemonArg(methodName, arg?.id, groupName);
        }
        
        Future<object> invokeServiceMethodDaemonArg(
            string methodName, string arg, string objectGroup) {
            var args = new Dictionary<string, object>(){{"objectGroup", objectGroup}};
            if (arg != null) {
                args["arg"] = arg;
            }
            return invokeServiceMethodDaemonParams(methodName, args);
        }
        
        
        public Future<List<RemoteDiagnosticsNode>> getChildren(
            InspectorInstanceRef instanceRef,
            bool summaryTree,
            RemoteDiagnosticsNode parent
        ) {
            return getListHelper(
                instanceRef,
                summaryTree ? "getChildrenSummaryTree" : "getChildrenDetailsSubtree",
                parent
            );
        }
        
        Future<List<RemoteDiagnosticsNode>> getListHelper(
            InspectorInstanceRef instanceRef,
            String methodName,
            RemoteDiagnosticsNode parent
        ) {
            if (disposed) return null;
            if (useDaemonApi) {
                return parseDiagnosticsNodesDaemon(
                    invokeServiceMethodDaemonInspectorRef(methodName, instanceRef),
                    parent);
            } 
            // else {
            //     return parseDiagnosticsNodesObservatory(
            //         invokeServiceMethodObservatoryInspectorRef(methodName, instanceRef),
            //         parent);
            // }
            return null;
        }

        Future<object> getRootRenderObjectFake()
        {
            Debug.Log("At getRootRenderObjectFake : not implement yet");
            return new SynchronousFuture<object>("At getRootRenderObjectFake : not implement yet");
        }
        
        Future<RemoteDiagnosticsNode> parseDiagnosticsNodeDaemon(
            Future<object> json) {
            if (disposed) return null;
            
            return json.then_<RemoteDiagnosticsNode>((value) =>
            {
                return FutureOr.value(parseDiagnosticsNodeHelper((Dictionary<string,object>)value));
            });
        }
        
        RemoteDiagnosticsNode parseDiagnosticsNodeHelper(
            Dictionary<string, object> jsonElement) {
            if (disposed) return null;
            if (jsonElement == null) return null;
            return new RemoteDiagnosticsNode(jsonElement, FutureOr.value(this), false, null);
        }
        
        
        // Future<RemoteDiagnosticsNode> parseDiagnosticsNodeObservatory(
        //     FutureOr<InstanceRef> instanceRefFuture) {
        //     return parseDiagnosticsNodeHelper(instanceRefToJson(instanceRefFuture));
        // }
        //
        // Future<InstanceRef> invokeServiceMethodObservatory(string methodName) {
        //     return invokeServiceMethodObservatory1(methodName, groupName);
        // }
        //
        // Future<InstanceRef> invokeServiceMethodObservatory1(
        //     string methodName, string arg1) {
        //     return inspectorLibrary.eval(
        //         "WidgetInspectorService.instance.$methodName('$arg1')",
        //         isAlive: this
        //     );
        // }
        
        
        Future<RemoteDiagnosticsNode> getRootWidget() {
            return invokeServiceMethodReturningNode("getRootWidgetSummaryTree");
        }
        
        Future<RemoteDiagnosticsNode> getRootWidgetFullTree() {
            return invokeServiceMethodReturningNode("getRootWidget");
        }

        Future<RemoteDiagnosticsNode> getSummaryTreeWithoutIds() {
            return parseDiagnosticsNodeDaemon(
                invokeServiceMethodDaemon("getRootWidgetSummaryTree"));
        }   

        Future<RemoteDiagnosticsNode> getRootRenderObject() {
            D.assert(!disposed);
            return invokeServiceMethodReturningNode("getRootRenderObject");
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
                if (_next == null)
                {
                    _next = inspectorService.createObjectGroup(debugName);
                }
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

        // VmService vmService;
        public static int nextGroupId = 0;
        public Future<string> inferPubRootDirectoryIfNeeded()
        {
            var group = createObjectGroup("temp");
            var root =  group.getRoot(FlutterTreeType.widget);
            return Future.value("null").to<string>();
        }

        public ObjectGroup createObjectGroup(string debugName) {
            return new ObjectGroup(debugName, this);
        }
        
        public bool useDaemonApi {
            get
            {
                return true;
            }
        }
        
        public Future<bool> isWidgetTreeReady() {
            return invokeBoolServiceMethodNoArgs("isWidgetTreeReady");
        }
        
        Future<bool> invokeBoolServiceMethodNoArgs(string methodName) {
            if (useDaemonApi) {
                return invokeServiceMethodDaemonNoGroupArgs(methodName).then_<bool>((v)=>
                {
                    if (v == null)
                    {
                        return false;
                    }
                    return (bool)v;
                });
            } 
            else {
                // return (invokeServiceMethodObservatoryNoGroup(methodName))
                //        ?.valueAsString ==
                //        "true";
                return null;
            }
        }
        
        Future<object> invokeServiceMethodDaemonNoGroupArgs(string methodName,
            List<string> args = null) {
            Dictionary<string, object> _params = new Dictionary<string, object>();
            if (args != null) {
                for (int i = 0; i < args.Count; ++i) {
                    _params[$"arg{i}"] = args[i];
                }
            }
            return invokeServiceMethodDaemonNoGroup(methodName, _params);
        }
        
        Future<object> invokeServiceMethodDaemonNoGroup(
            string methodName, Dictionary<string, object> args) {

            if (methodName == "isWidgetTreeReady")
            {
                return new SynchronousFuture<object>(true);
            }
            
            Debug.Log("Function invokeServiceMethodDaemonNoGroup : not implement yet");
            return new SynchronousFuture<object>(false);
            // string callMethodName = $"ext.flutter.inspector.{methodName}";
            // if (!Globals.serviceManager.serviceExtensionManager
            //     .isServiceExtensionAvailable(callMethodName)) {
            //     return new SynchronousFuture<object>(new Dictionary<string,object>()
            //     {
            //         {"result", null}
            //     });
            // }
            //
            // var r = vmService.callServiceExtension(
            //     callMethodName,
            //     isolateId: inspectorLibrary.isolateId,
            //     args: args
            // );
            // var json = r.json;
            // if (json["errorMessage"] != null) {
            //     throw new Exception($"{methodName} -- {json["errorMessage"]}");
            // }
            // return json["result"];
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
    
    public class InspectorInstanceRef: IEquatable<InspectorInstanceRef> {
        public InspectorInstanceRef(string id)
        {
            this.id = id;
        }
        

        
        public new int hashCode => id.GetHashCode();
        
        public new string toString() => $"instance-{id}";

        public readonly string id;
        public bool Equals(InspectorInstanceRef other)
        {
            if (ReferenceEquals(null, other)) {
                return false;
            }

            if (ReferenceEquals(this, other)) {
                return true;
            }

            return  other is InspectorInstanceRef && Equals(id, other.id);
        }
    }
    
    
}