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

    public static class DiagnosticsNodeUtils
    {
        
        public static EnumUtils<DiagnosticLevel> diagnosticLevelUtils
        {
            get
            {
                List<DiagnosticLevel> styles = new List<DiagnosticLevel>();
                foreach (DiagnosticLevel style in Enum.GetValues(typeof(DiagnosticLevel)))
                {
                    styles.Add(style);
                }
                return new EnumUtils<DiagnosticLevel>(styles);
            }
        }
        
    }
    
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

        public RemoteDiagnosticsNode parent;

        Future<string> propertyDocFuture;

        List<RemoteDiagnosticsNode> cachedProperties;

        public readonly FutureOr inspectorService; // FutureOr<ObjectGroup>

        public readonly Dictionary<string, object> json;
        
        List<RemoteDiagnosticsNode> _children;
        static readonly CustomIconMaker iconMaker = new CustomIconMaker();

        public bool isFlex => new List<string>{"Row", "Column", "Flex"}.Contains(widgetRuntimeType);
        
        // Future<Dictionary<string, InstanceRef>> _valueProperties;

        public readonly bool isProperty;
        
        bool hasCreationLocation {
            get
            {
                return _creationLocation != null || json.ContainsKey("creationLocation");
            }
            
        }

        public string separator => showSeparator ? ":" : "";
        
        public bool showSeparator => getBooleanMember("showSeparator", true);
        public bool hasDefaultValue => json.ContainsKey("defaultValue");
        
        public bool isSummaryTree => getBooleanMember("summaryTree", false);
        public bool isCreatedByLocalProject {
            get
            {
                return getBooleanMember("createdByLocalProject", false);
            }
        }
        
        public string description => getStringMember("description");
        
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
        
        public bool childrenReady => json.ContainsKey("children") || _children != null || !hasChildren;

        public Widget icon {
            get
            {
                if (isProperty) return null;

                return iconMaker.fromWidgetName(widgetRuntimeType);
            }

        }
        
        InspectorSourceLocation _creationLocation;

        public InspectorSourceLocation creationLocation {
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
            return style1;  // ?? defaultValue
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
        
        public Dictionary<string, object>  valuePropertiesJson
        {
            get
            {
                if (json.ContainsKey("valueProperties"))
                {
                    return (Dictionary<string, object>) json["valueProperties"];
                }
                return new Dictionary<string, object>(){{"valueProperties","null"}};
            }
        }

        public bool hasChildren {
            get
            {
                if (json.ContainsKey("children"))
                {
                    List<Dictionary<string, object>> children = (List<Dictionary<string, object>>)json["children"];
                    if (children != null) {
                        return children.isNotEmpty();
                    }
                    
                }
                return getBooleanMember("hasChildren", false);
            }
            
        }
        
        public bool isDiagnosticableValue {
            get
            {
                return getBooleanMember("isDiagnosticableValue", false);
            }
            
        }
        public bool showName => getBooleanMember("showName", true);
        public DiagnosticLevel level => getLevelMember("level", DiagnosticLevel.info);
        
        public string propertyType => getStringMember("propertyType");
        
        bool getBooleanMember(string memberName, bool defaultValue)
        {
            if (json.ContainsKey(memberName))
            {
                if (json[memberName] == null) {
                    return defaultValue;
                }
                return (bool)json[memberName];
            }

            return defaultValue;
        }
            
        DiagnosticLevel getLevelMember(
            string memberName, DiagnosticLevel defaultValue)
        {
            string value = null;
            if (json.ContainsKey(memberName))
            {
                value = (string)json[memberName];
            }
            if (value == null) {
                return defaultValue;
            }
            var level = DiagnosticsNodeUtils.diagnosticLevelUtils.enumEntry(value);
            D.assert(level != null, () => $"Unabled to find level for {value}");
            if (level != null)
            {
                return level;
            }
            return defaultValue;
        }
        
        List<RemoteDiagnosticsNode> _childrenFuture;
        
        public List<RemoteDiagnosticsNode> children {
            get
            {
                // _computeChildren();
                return _childrenFuture;
            }
            
        }
        
        public InspectorInstanceRef dartDiagnosticRef {
            get
            {
                if (json.ContainsKey("objectId"))
                {
                    return new InspectorInstanceRef((string)json["objectId"]);
                }
                return new InspectorInstanceRef("The key 'objectId' doesn't exist");
            }
            
        }
        
        // void _computeChildren() {
        //     _maybePopulateChildren();
        //     if (!hasChildren || _children != null) {
        //         return;
        //     }
        //
        //     if (_childrenFuture != null) {
        //         return;
        //     }
        //
        //     _childrenFuture = _getChildrenHelper();
        //     try {
        //         _children = _childrenFuture;
        //     } finally {
        //         if (_children == null)
        //         {
        //             _children = new List<RemoteDiagnosticsNode>();
        //         }
        //     }
        // }
        
        public List<RemoteDiagnosticsNode> inlineProperties {
            get
            {
                if (cachedProperties == null) {
                    cachedProperties = new List<RemoteDiagnosticsNode>();
                    if (json.ContainsKey("properties")) {
                        List<Dictionary<string,object>> jsonArray = (List<Dictionary<string,object>>)json["properties"];
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
                _maybePopulateChildren();
                return _children;
            }
            
        }
        
        public void _maybePopulateChildren() {
            if (!hasChildren || _children != null) {
                return;
            }
            
            List<Dictionary<string, object>> jsonArray = (List<Dictionary<string, object>>)json["children"];
            if (jsonArray?.isNotEmpty() == true) {
                List<RemoteDiagnosticsNode> nodes = new List<RemoteDiagnosticsNode>();
                foreach (Dictionary<string, object> element in jsonArray) {
                    var child =
                        new RemoteDiagnosticsNode(element, inspectorService, false, parent);
                    child.parent = this;
                    nodes.Add(child);
                }
                _children = nodes;
            }
        }
        
    }
    
    public class InspectorSourceLocation {
        public InspectorSourceLocation(Dictionary<string, object> json, InspectorSourceLocation parent)
        {
            this.json = json;
            this.parent = parent;
        }

        public readonly Dictionary<string, object> json;
        public readonly InspectorSourceLocation parent;

        public string path
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