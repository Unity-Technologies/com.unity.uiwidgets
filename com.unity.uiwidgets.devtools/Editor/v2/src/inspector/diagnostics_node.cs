using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Unity.UIWidgets.async;
using Unity.UIWidgets.DevTools.ui;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.widgets;

namespace Unity.UIWidgets.DevTools.inspector
{
    public class RemoteDiagnosticsNode : DiagnosticableTree {
        public RemoteDiagnosticsNode(
            Dictionary<string, object> json,
            FutureOr inspectorService,
            bool isProperty,
            RemoteDiagnosticsNode parent
        )
        {
            this.json = json;
            this.inspectorService = inspectorService;
            this.isProperty = isProperty;
            this.parent = parent;
        }

        RemoteDiagnosticsNode parent;

        Future<string> propertyDocFuture;

        List<RemoteDiagnosticsNode> cachedProperties;

        public readonly FutureOr inspectorService; // FutureOr<ObjectGroup>

        public readonly Dictionary<string, object> json;
        
        List<RemoteDiagnosticsNode> _children;
        static readonly CustomIconMaker iconMaker = new CustomIconMaker();

        // Future<Dictionary<string, InstanceRef>> _valueProperties;

        public readonly bool isProperty;
        
        bool hasCreationLocation {
            get
            {
                return _creationLocation != null || json.ContainsKey("creationLocation");
            }
            
        }

        public string name
        {
            get
            {
                return getStringMember("name");
            }
        }
        
        public string type => getStringMember("type");
        public string widgetRuntimeType => getStringMember("widgetRuntimeType");

        string getStringMember(string memberName) {
            return JsonUtils.getStringMember(json, memberName);
        }
        
        public bool childrenReady {
            get
            {
                return json.ContainsKey("children") || _children != null || !hasChildren;
            }
            
        }
        
        public Widget icon {
            get
            {
                if (isProperty) return null;

                return iconMaker.fromWidgetName(widgetRuntimeType);
            }

        }
        
        InspectorSourceLocation _creationLocation;

        InspectorSourceLocation creationLocation {
            get
            {
                if (_creationLocation != null) {
                    return _creationLocation;
                }
                if (!hasCreationLocation) {
                    return null;
                }
                _creationLocation = new InspectorSourceLocation((Dictionary<string,object>)json["creationLocation"], null);
                return _creationLocation;
            }
            set 
            {
            _creationLocation = value;
            }
            
        }
        
        
        public DiagnosticsTreeStyle? style {
            get
            {
                return _style != null ?_style: getStyleMember("style", DiagnosticsTreeStyle.sparse);
            }
            set {
            _style = value;
        }
            
        }
        DiagnosticsTreeStyle? _style;
        
        DiagnosticsTreeStyle getStyleMember(
            string memberName, DiagnosticsTreeStyle defaultValue) {
            if (!json.ContainsKey(memberName)) {
                return defaultValue;
            }
            string value = json[memberName].ToString();
            if (value == null) {
                return defaultValue;
            }
            var style1 = treeStyleUtils.enumEntry(value);
            D.assert(style1 != null);
            return style ?? defaultValue;
        }

        private EnumUtils<DiagnosticsTreeStyle> _treeStyleUtils;

        EnumUtils<DiagnosticsTreeStyle> treeStyleUtils
        {
            get
            {
                List<DiagnosticsTreeStyle> styles = new List<DiagnosticsTreeStyle>();
                foreach (DiagnosticsTreeStyle style in Enum.GetValues(typeof(DiagnosticsTreeStyle)))
                {
                    styles.Add(style);
                }
                return new EnumUtils<DiagnosticsTreeStyle>(styles);
            }
        }
        
        public bool hasChildren {
            get
            {
                List<object> children = json["children"] as List<object>;
                if (children != null) {
                    return children.isNotEmpty();
                }
                return getBooleanMember("hasChildren", false);
            }
            
        }
        
        bool getBooleanMember(string memberName, bool defaultValue) {
            if (json[memberName] == null) {
                return defaultValue;
            }
            return (bool)json[memberName];
        }
            
        public List<RemoteDiagnosticsNode> inlineProperties {
            get
            {
                if (cachedProperties == null) {
                    cachedProperties = new List<RemoteDiagnosticsNode>();
                    if (json.ContainsKey("properties")) {
                        List<object> jsonArray = (List<object>)json["properties"];
                        foreach (Dictionary<string, object> element in jsonArray) {
                            cachedProperties.Add(
                                new RemoteDiagnosticsNode(element, inspectorService, true, parent));
                        }
                        trackPropertiesMatchingParameters(cachedProperties);
                    }
                }
                return cachedProperties;
            }
            
        }
        
        List<RemoteDiagnosticsNode> trackPropertiesMatchingParameters(
            List<RemoteDiagnosticsNode> nodes) {
            List<InspectorSourceLocation> parameterLocations =
                creationLocation?.getParameterLocations();
            if (parameterLocations != null) {
                Dictionary<string, InspectorSourceLocation> names = new Dictionary<string, InspectorSourceLocation>();
                foreach (InspectorSourceLocation location in parameterLocations) {
                    string name = location.getName();
                    if (name != null) {
                        names[name] = location;
                    }
                }
                foreach (RemoteDiagnosticsNode node in nodes) {
                    node.parent = this;
                    string name = node.name;
                    if (name != null) {
                        InspectorSourceLocation parameterLocation = names[name];
                        if (parameterLocation != null) {
                            node.creationLocation = parameterLocation;
                        }
                    }
                }
            }
            return nodes;
        }
        
        public List<RemoteDiagnosticsNode> childrenNow {
            get
            {
                // _maybePopulateChildren();
                return _children;
            }
            
        }
        
    }
    
    class InspectorSourceLocation {
        public InspectorSourceLocation(Dictionary<string, object> json, InspectorSourceLocation parent)
        {
            this.json = json;
            this.parent = parent;
        }

        public readonly Dictionary<string, object> json;
        public readonly InspectorSourceLocation parent;

        string path
        {
            get
            {
                return JsonUtils.getStringMember(json, "file");
            }
        }

        string getFile() {
            var fileName = path;
            if (fileName == null) {
                return parent != null ? parent.getFile() : null;
            }

            return fileName;
        }

        int getLine() => JsonUtils.getIntMember(json, "line");

        public string getName() => JsonUtils.getStringMember(json, "name");

        int getColumn() => JsonUtils.getIntMember(json, "column");

        // SourcePosition getXSourcePosition() {
        //     var file = getFile();
        //     if (file == null) {
        //         return null;
        //     }
        //     int line = getLine();
        //     int column = getColumn();
        //     if (line < 0 || column < 0) {
        //         return null;
        //     }
        //     return SourcePosition(file: file, line: line - 1, column: column - 1);
        // }

        public List<InspectorSourceLocation> getParameterLocations() {
            if (json.ContainsKey("parameterLocations")) {
                List<object> parametersJson = (List<object>)json["parameterLocations"];
                List<InspectorSourceLocation> ret = new List<InspectorSourceLocation>();
                for (int i = 0; i < parametersJson.Count; ++i) {
                    ret.Add(new InspectorSourceLocation((Dictionary<string, object>)parametersJson[i], this));
                }
                return ret;
            }
            return null;
        }
    }
    
    
}