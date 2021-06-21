using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.UIWidgets.async;
using Unity.UIWidgets.foundation;

namespace Unity.UIWidgets.DevTools.inspector
{
    class RemoteDiagnosticsNode : DiagnosticableTree {
        public RemoteDiagnosticsNode(
            Dictionary<string, Object> json,
            FutureOr inspectorService,
            bool isProperty,
            RemoteDiagnosticsNode parent
        )
        {
            this.json = json;
            this.inspectorService = inspectorService;
            this.isProperty = isProperty;
        }

        RemoteDiagnosticsNode parent;

        Future<string> propertyDocFuture;

        List<RemoteDiagnosticsNode> cachedProperties;

        public readonly FutureOr inspectorService; // FutureOr<ObjectGroup>

        public readonly Dictionary<string, object> json;

        // Future<Dictionary<string, InstanceRef>> _valueProperties;

        public readonly bool isProperty;
        
        
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
        
            
        
    }
}