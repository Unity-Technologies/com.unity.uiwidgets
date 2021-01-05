using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Unity.UIWidgets.ui;
using Unity.UIWidgets.widgets;
using UnityEditor.Experimental.GraphView;

namespace Unity.UIWidgets.foundation {
    public enum DiagnosticLevel {
        hidden,
        fine,
        debug,
        info,
        warning,
        error,
        off,
    }

    public enum DiagnosticsTreeStyle {
        
        sparse,
        offstage,
        dense,
        transition,
        error,
        whitespace,
        flat,
        singleLine,
        errorProperty,
    }

    public class DiagnosticUtils {
        public static bool _isSingleLine(DiagnosticsTreeStyle style) {
            return style == DiagnosticsTreeStyle.singleLine;
        }
        public static string describeEnum(object enumEntry) {
            string description = enumEntry.ToString();
            int indexOfDot = description.IndexOf(".");
            D.assert(indexOfDot != -1 && indexOfDot < description.Length - 1);
            return description.Substring(indexOfDot + 1);
        }
    }

    public class TextTreeConfiguration {
        public TextTreeConfiguration(
            string prefixLineOne = null,
            string prefixOtherLines = null,
            string prefixLastChildLineOne = null,
            string prefixOtherLinesRootNode = null,
            string linkCharacter = null,
            string propertyPrefixIfChildren = null,
            string propertyPrefixNoChildren = null,
            string lineBreak = "\n",
            bool lineBreakProperties = true,
            string afterName = ":",
            string afterDescriptionIfBody = "",
            string beforeProperties = "",
            string afterProperties = "",
            string propertySeparator = "",
            string bodyIndent = "",
            string footer = "",
            bool showChildren = true,
            bool addBlankLineIfNoChildren = true,
            bool isNameOnOwnLine = false,
            bool isBlankLineBetweenPropertiesAndChildren = true
        ) {
            D.assert(prefixLineOne != null);
            D.assert(prefixOtherLines != null);
            D.assert(prefixLastChildLineOne != null);
            D.assert(prefixOtherLinesRootNode != null);
            D.assert(linkCharacter != null);
            D.assert(propertyPrefixIfChildren != null);
            D.assert(propertyPrefixNoChildren != null);
            D.assert(lineBreak != null);
            D.assert(afterName != null);
            D.assert(afterDescriptionIfBody != null);
            D.assert(beforeProperties != null);
            D.assert(afterProperties != null);
            D.assert(propertySeparator != null);
            D.assert(bodyIndent != null);
            D.assert(footer != null);

            this.prefixLineOne = prefixLineOne;
            this.prefixOtherLines = prefixOtherLines;
            this.prefixLastChildLineOne = prefixLastChildLineOne;
            this.prefixOtherLinesRootNode = prefixOtherLinesRootNode;
            this.propertyPrefixIfChildren = propertyPrefixIfChildren;
            this.propertyPrefixNoChildren = propertyPrefixNoChildren;
            this.linkCharacter = linkCharacter;
            childLinkSpace = new string(' ', linkCharacter.Length);
            this.lineBreak = lineBreak;
            this.lineBreakProperties = lineBreakProperties;
            this.afterName = afterName;
            this.afterDescriptionIfBody = afterDescriptionIfBody;
            this.beforeProperties = beforeProperties;
            this.afterProperties = afterProperties;
            this.propertySeparator = propertySeparator;
            this.bodyIndent = bodyIndent;
            this.showChildren = showChildren;
            this.addBlankLineIfNoChildren = addBlankLineIfNoChildren;
            this.isNameOnOwnLine = isNameOnOwnLine;
            this.footer = footer;
            this.isBlankLineBetweenPropertiesAndChildren = isBlankLineBetweenPropertiesAndChildren;
        }

        public readonly string prefixLineOne;

        public readonly string prefixOtherLines;

        public readonly string prefixLastChildLineOne;

        public readonly string prefixOtherLinesRootNode;

        public readonly string propertyPrefixIfChildren;

        public readonly string propertyPrefixNoChildren;

        public readonly string linkCharacter;

        public readonly string childLinkSpace;

        public readonly string lineBreak;

        public readonly bool lineBreakProperties;

        public readonly string afterName;

        public readonly string afterDescriptionIfBody;

        public readonly string beforeProperties;

        public readonly string afterProperties;

        public readonly string propertySeparator;

        public readonly string bodyIndent;

        public readonly bool showChildren;

        public readonly bool addBlankLineIfNoChildren;

        public readonly bool isNameOnOwnLine;

        public readonly string footer;

        public readonly bool isBlankLineBetweenPropertiesAndChildren;
    }


    public static partial class foundation_ {
        public static readonly TextTreeConfiguration sparseTextConfiguration = new TextTreeConfiguration(
            prefixLineOne: "├─",
            prefixOtherLines: " ",
            prefixLastChildLineOne: "└─",
            linkCharacter: "│",
            propertyPrefixIfChildren: "│ ",
            propertyPrefixNoChildren: "  ",
            prefixOtherLinesRootNode: " "
        );

        public static readonly TextTreeConfiguration dashedTextConfiguration = new TextTreeConfiguration(
            prefixLineOne: "╎╌",
            prefixLastChildLineOne: "└╌",
            prefixOtherLines: " ",
            linkCharacter: "╎",
            propertyPrefixIfChildren: "│ ",
            propertyPrefixNoChildren: "  ",
            prefixOtherLinesRootNode: " "
        );

        public static readonly TextTreeConfiguration denseTextConfiguration = new TextTreeConfiguration(
            propertySeparator: ", ",
            beforeProperties: "(",
            afterProperties: ")",
            lineBreakProperties: false,
            prefixLineOne: "├",
            prefixOtherLines: "",
            prefixLastChildLineOne: "└",
            linkCharacter: "│",
            propertyPrefixIfChildren: "│",
            propertyPrefixNoChildren: " ",
            prefixOtherLinesRootNode: "",
            addBlankLineIfNoChildren: false,
            isBlankLineBetweenPropertiesAndChildren: false
        );

        public static readonly TextTreeConfiguration transitionTextConfiguration = new TextTreeConfiguration(
            prefixLineOne: "╞═╦══ ",
            prefixLastChildLineOne: "╘═╦══ ",
            prefixOtherLines: " ║ ",
            footer: " ╚═══════════\n",
            linkCharacter: "│",
            propertyPrefixIfChildren: "",
            propertyPrefixNoChildren: "",
            prefixOtherLinesRootNode: "",
            afterName: " ═══",
            afterDescriptionIfBody: ":",
            bodyIndent: "  ",
            isNameOnOwnLine: true,
            addBlankLineIfNoChildren: false,
            isBlankLineBetweenPropertiesAndChildren: false
        );

        public static readonly TextTreeConfiguration whitespaceTextConfiguration = new TextTreeConfiguration(
            prefixLineOne: "",
            prefixLastChildLineOne: "",
            prefixOtherLines: " ",
            prefixOtherLinesRootNode: "  ",
            bodyIndent: "",
            propertyPrefixIfChildren: "",
            propertyPrefixNoChildren: "",
            linkCharacter: " ",
            addBlankLineIfNoChildren: false,
            afterDescriptionIfBody: ":",
            isBlankLineBetweenPropertiesAndChildren: false
        );

        public static readonly TextTreeConfiguration singleLineTextConfiguration = new TextTreeConfiguration(
            propertySeparator: ", ",
            beforeProperties: "(",
            afterProperties: ")",
            prefixLineOne: "",
            prefixOtherLines: "",
            prefixLastChildLineOne: "",
            lineBreak: "",
            lineBreakProperties: false,
            addBlankLineIfNoChildren: false,
            showChildren: false,
            propertyPrefixIfChildren: "",
            propertyPrefixNoChildren: "",
            linkCharacter: "",
            prefixOtherLinesRootNode: ""
        );
    }

    class _PrefixedStringBuilder {
        internal _PrefixedStringBuilder(string prefixLineOne, string prefixOtherLines) {
            this.prefixLineOne = prefixLineOne;
            this.prefixOtherLines = prefixOtherLines;
        }

        public readonly string prefixLineOne;

        public string prefixOtherLines;

        readonly StringBuilder _buffer = new StringBuilder();
        bool _atLineStart = true;
        bool _hasMultipleLines = false;

        public bool hasMultipleLines {
            get { return _hasMultipleLines; }
        }

        public void write(string s) {
            if (s.isEmpty()) {
                return;
            }

            if (s == "\n") {
                if (_buffer.Length == 0) {
                    _buffer.Append(prefixLineOne.TrimEnd());
                }
                else if (_atLineStart) {
                    _buffer.Append(prefixOtherLines.TrimEnd());
                    _hasMultipleLines = true;
                }

                _buffer.Append("\n");
                _atLineStart = true;
                return;
            }

            if (_buffer.Length == 0) {
                _buffer.Append(prefixLineOne);
            }
            else if (_atLineStart) {
                _buffer.Append(prefixOtherLines);
                _hasMultipleLines = true;
            }

            bool lineTerminated = false;

            if (s.EndsWith("\n")) {
                s = s.Substring(0, s.Length - 1);
                lineTerminated = true;
            }

            var parts = s.Split('\n');
            _buffer.Append(parts[0]);
            for (int i = 1; i < parts.Length; ++i) {
                _buffer.Append("\n")
                    .Append(prefixOtherLines)
                    .Append(parts[i]);
            }

            if (lineTerminated) {
                _buffer.Append("\n");
            }

            _atLineStart = lineTerminated;
        }

        public void writeRaw(string text) {
            if (text.isEmpty()) {
                return;
            }

            _buffer.Append(text);
            _atLineStart = text.EndsWith("\n");
        }


        public void writeRawLine(string line) {
            if (line.isEmpty()) {
                return;
            }

            _buffer.Append(line);
            if (!line.EndsWith("\n")) {
                _buffer.Append('\n');
            }

            _atLineStart = true;
        }

        public override string ToString() {
            return _buffer.ToString();
        }
    }

    class _NoDefaultValue {
        internal _NoDefaultValue() {
        }
    }

    class _NullDefaultValue {
        internal _NullDefaultValue() {
        }
    }

    public static partial class foundation_ {
        public static readonly object kNoDefaultValue = new _NoDefaultValue();

        public static readonly object kNullDefaultValue = new _NullDefaultValue();
    }


    public abstract class DiagnosticsNode {
        protected DiagnosticsNode(
            string name = null,
            DiagnosticsTreeStyle? style = null,
            String linePrefix = null,
            bool showName = true,
            bool showSeparator = true
        ) {
            D.assert(name == null || !name.EndsWith(":"),
                () => "Names of diagnostic nodes must not end with colons.");
            this.name = name;
            _style = style;
            _showName = showName;
            this.showSeparator = showSeparator;
            this.linePrefix = linePrefix;
        }

        public static DiagnosticsNode message(
            string message,
            DiagnosticsTreeStyle style = DiagnosticsTreeStyle.singleLine,
            DiagnosticLevel level = DiagnosticLevel.info,
            bool allowWrap = true
        ) {
            D.assert(style != null);
            D.assert(level != null);
            return new DiagnosticsProperty<object>(
                "",
                null,
                description: message,
                style: style,
                showName: false,
                level: level
            );
        }

        public readonly string name;

        public abstract string toDescription(
            TextTreeConfiguration parentConfiguration = null
        );

        public readonly bool showSeparator;

        public object value { get; }

        public bool isFiltered(DiagnosticLevel minLevel) {
            return level < minLevel;
        }
        
        

        public virtual DiagnosticLevel level {
            get { return DiagnosticLevel.info; }
        }

        public virtual bool showName {
            get { return _showName; }
        }

        readonly bool _showName;

        public virtual string emptyBodyDescription {
            get { return null; }
        }

        readonly string linePrefix;

        public abstract object valueObject { get; }

        public virtual DiagnosticsTreeStyle? style {
            get { return _style; }
        }

        readonly DiagnosticsTreeStyle? _style;

        public bool allowWrap {
            get { return false; }
        }

        public bool allowNameWrap {
            get { return false; }
        }

        public bool allowTruncate {
            get { return false; }
        }

        public abstract List<DiagnosticsNode> getProperties();

        public abstract List<DiagnosticsNode> getChildren();

        string _separator {
            get { return showSeparator ? ":" : ""; }
        }
        public virtual Dictionary<string, object> toJsonMap(DiagnosticsSerializationDelegate Delegate) {
            Dictionary<string, object> result = new Dictionary<string, object>();
            D.assert(()=> {
                bool hasChildren = getChildren().isNotEmpty();
                result = new Dictionary<string, object>{ };
                result["description"] = toDescription();
                result["type"] = GetType().ToString();
                if (name != null)
                    result["name"] = name;
                if (!showSeparator)
                    result["showSeparator"] = showSeparator;
                if (level != DiagnosticLevel.info)
                    result["level"] = DiagnosticUtils.describeEnum(level);
                if (showName == false)
                    result["showName"] = showName;
                if (emptyBodyDescription != null)
                    result["emptyBodyDescription"] = emptyBodyDescription;
                if (style != DiagnosticsTreeStyle.sparse)
                    result["style"] = DiagnosticUtils.describeEnum(style);
                if (allowTruncate)
                    result["allowTruncate"] = allowTruncate;
                if (hasChildren)
                    result["hasChildren"] = hasChildren;
                if (linePrefix?.isNotEmpty() == true)
                    result["linePrefix"] = linePrefix;
                if (!allowWrap)
                    result["allowWrap"] = allowWrap;
                if (allowNameWrap)
                    result["allowNameWrap"] = allowNameWrap;
                Delegate.additionalNodeProperties(this);
                if (Delegate.includeProperties)
                    result["properties"] = toJsonList(
                        Delegate.filterProperties(getProperties(), this),
                        this,
                        Delegate
                    );
                if (Delegate.subtreeDepth > 0)
                    result["children"] = toJsonList(
                    Delegate.filterChildren(getChildren(), this),
                    this,
                    Delegate
                );
                return true;
            });
            return result;
        }

        
        public static List<Dictionary<string, object>> toJsonList(
            List<DiagnosticsNode> nodes,
            DiagnosticsNode parent,
            DiagnosticsSerializationDelegate Delegate
            ) {
            bool truncated = false;
            if (nodes == null)
                return new List<Dictionary<string, object>>();
            int originalNodeCount = nodes.Count;
            nodes = Delegate.truncateNodesList(nodes, parent);
            if (nodes.Count != originalNodeCount) {
                nodes.Add(DiagnosticsNode.message("..."));
                truncated = true;
            }
            List<Dictionary<string, object>> json = new List<Dictionary<string, object>>();
            foreach (var node in nodes) {
                json.Add(node.toJsonMap(Delegate.delegateForNode(node)));
            }
            json.ToList();
            if (truncated)
                json.Last()["truncated"] = true;
            return json;
        }

        public override string ToString() {
            return toString();
        }

        public virtual string toString(
            TextTreeConfiguration parentConfiguration = null,
            DiagnosticLevel minLevel = DiagnosticLevel.info
        ) {
            D.assert(style != null);
            if (style == DiagnosticsTreeStyle.singleLine) {
                return toStringDeep(parentConfiguration: parentConfiguration, minLevel: minLevel);
            }

            var description = toDescription(parentConfiguration: parentConfiguration);
            if (name.isEmpty() || !showName) {
                return description;
            }

            return description.Contains("\n")
                ? name + _separator + "\n" + description
                : name + _separator + description;
        }

        protected TextTreeConfiguration textTreeConfiguration {
            get {
                D.assert(style != null);
                switch (style) {
                    case DiagnosticsTreeStyle.dense:
                        return foundation_.denseTextConfiguration;
                    case DiagnosticsTreeStyle.sparse:
                        return foundation_.sparseTextConfiguration;
                    case DiagnosticsTreeStyle.offstage:
                        return foundation_.dashedTextConfiguration;
                    case DiagnosticsTreeStyle.whitespace:
                        return foundation_.whitespaceTextConfiguration;
                    case DiagnosticsTreeStyle.transition:
                        return foundation_.transitionTextConfiguration;
                    case DiagnosticsTreeStyle.singleLine:
                        return foundation_.singleLineTextConfiguration;
                }

                return null;
            }
        }

        TextTreeConfiguration _childTextConfiguration(
            DiagnosticsNode child,
            TextTreeConfiguration textStyle
        ) {
            return child != null && child.style != DiagnosticsTreeStyle.singleLine
                ? child.textTreeConfiguration
                : textStyle;
        }

        public string toStringDeep(
            string prefixLineOne = "",
            string prefixOtherLines = null,
            TextTreeConfiguration parentConfiguration = null,
            DiagnosticLevel minLevel = DiagnosticLevel.debug
        ) {
            prefixOtherLines = prefixOtherLines ?? prefixLineOne;

            var children = getChildren();
            var config = textTreeConfiguration;
            if (prefixOtherLines.isEmpty()) {
                prefixOtherLines += config.prefixOtherLinesRootNode;
            }

            var builder = new _PrefixedStringBuilder(
                prefixLineOne,
                prefixOtherLines
            );

            var description = toDescription(parentConfiguration: parentConfiguration);
            if (description.isEmpty()) {
                if (name.isNotEmpty() && showName) {
                    builder.write(name);
                }
            }
            else {
                if (name.isNotEmpty() && showName) {
                    builder.write(name);
                    if (showSeparator) {
                        builder.write(config.afterName);
                    }

                    builder.write(
                        config.isNameOnOwnLine || description.Contains("\n") ? "\n" : "");
                    if (description.Contains("\n") && style == DiagnosticsTreeStyle.singleLine) {
                        builder.prefixOtherLines += "  ";
                    }
                }

                builder.prefixOtherLines +=
                    children.isEmpty() ? config.propertyPrefixNoChildren : config.propertyPrefixIfChildren;
                builder.write(description);
            }

            var properties = getProperties().Where(n => !n.isFiltered(minLevel)).ToList();

            if (properties.isNotEmpty() || children.isNotEmpty() || emptyBodyDescription != null) {
                builder.write(config.afterDescriptionIfBody);
            }

            if (config.lineBreakProperties) {
                builder.write(config.lineBreak);
            }

            if (properties.isNotEmpty()) {
                builder.write(config.beforeProperties);
            }

            builder.prefixOtherLines += config.bodyIndent;
            if (emptyBodyDescription != null &&
                properties.isEmpty() &&
                children.isEmpty() &&
                prefixLineOne.isNotEmpty()) {
                builder.write(emptyBodyDescription);
                if (config.lineBreakProperties) {
                    builder.write(config.lineBreak);
                }
            }

            for (int i = 0; i < properties.Count; ++i) {
                DiagnosticsNode property = properties[i];
                if (i > 0) {
                    builder.write(config.propertySeparator);
                }

                const int kWrapWidth = 65;
                if (property.style != DiagnosticsTreeStyle.singleLine) {
                    TextTreeConfiguration propertyStyle = property.textTreeConfiguration;
                    builder.writeRaw(property.toStringDeep(
                        prefixLineOne: builder.prefixOtherLines + propertyStyle.prefixLineOne,
                        prefixOtherLines: builder.prefixOtherLines + propertyStyle.linkCharacter +
                                          propertyStyle.prefixOtherLines,
                        parentConfiguration: config,
                        minLevel: minLevel
                    ));
                    continue;
                }

                D.assert(property.style == DiagnosticsTreeStyle.singleLine);
                string message = property.toString(parentConfiguration: config, minLevel: minLevel);
                if (!config.lineBreakProperties || message.Length < kWrapWidth) {
                    builder.write(message);
                }
                else {
                    var lines = message.Split('\n');
                    for (int j = 0; j < lines.Length; ++j) {
                        string line = lines[j];
                        if (j > 0) {
                            builder.write(config.lineBreak);
                        }

                        builder.write(string.Join("\n",
                            DebugPrint.debugWordWrap(line, kWrapWidth, wrapIndent: "  ").ToArray()));
                    }
                }

                if (config.lineBreakProperties) {
                    builder.write(config.lineBreak);
                }
            }

            if (properties.isNotEmpty()) {
                builder.write(config.afterProperties);
            }

            if (!config.lineBreakProperties) {
                builder.write(config.lineBreak);
            }

            var prefixChildren = prefixOtherLines + config.bodyIndent;
            if (children.isEmpty() &&
                config.addBlankLineIfNoChildren &&
                builder.hasMultipleLines) {
                string prefix = prefixChildren.TrimEnd();
                if (prefix.isNotEmpty()) {
                    builder.writeRaw(prefix + config.lineBreak);
                }
            }

            if (children.isNotEmpty() && config.showChildren) {
                if (config.isBlankLineBetweenPropertiesAndChildren &&
                    properties.isNotEmpty() &&
                    children.First().textTreeConfiguration.isBlankLineBetweenPropertiesAndChildren) {
                    builder.write(config.lineBreak);
                }

                for (int i = 0; i < children.Count; i++) {
                    DiagnosticsNode child = children[i];
                    D.assert(child != null);
                    TextTreeConfiguration childConfig = _childTextConfiguration(child, config);
                    if (i == children.Count - 1) {
                        string lastChildPrefixLineOne = prefixChildren + childConfig.prefixLastChildLineOne;
                        builder.writeRawLine(child.toStringDeep(
                            prefixLineOne: lastChildPrefixLineOne,
                            prefixOtherLines: prefixChildren + childConfig.childLinkSpace +
                                              childConfig.prefixOtherLines,
                            parentConfiguration: config,
                            minLevel: minLevel
                        ));
                        if (childConfig.footer.isNotEmpty()) {
                            builder.writeRaw(prefixChildren + childConfig.childLinkSpace + childConfig.footer);
                        }
                    }
                    else {
                        TextTreeConfiguration nextChildStyle = _childTextConfiguration(children[i + 1], config);
                        string childPrefixLineOne = prefixChildren + childConfig.prefixLineOne;
                        string childPrefixOtherLines =
                            prefixChildren + nextChildStyle.linkCharacter + childConfig.prefixOtherLines;
                        builder.writeRawLine(child.toStringDeep(
                            prefixLineOne: childPrefixLineOne,
                            prefixOtherLines: childPrefixOtherLines,
                            parentConfiguration: config,
                            minLevel: minLevel
                        ));
                        if (childConfig.footer.isNotEmpty()) {
                            builder.writeRaw(prefixChildren + nextChildStyle.linkCharacter + childConfig.footer);
                        }
                    }
                }
            }

            return builder.ToString();
        }
    }

    public class DiagnosticsBlock : DiagnosticsNode {
        public DiagnosticsBlock(
            string name,
            DiagnosticsTreeStyle style = DiagnosticsTreeStyle.whitespace,
            bool showName = true,
            bool showSeparator = true,
            string linePrefix = null,
            object value = null,
            string description = null,
            DiagnosticLevel level = DiagnosticLevel.info,
            bool allowTruncate = false,
            List<DiagnosticsNode> children = null,
            List<DiagnosticsNode> properties = null
        ) : base(
            name: name,
            style: style,
            showName: showName && name != null,
            showSeparator: showSeparator,
            linePrefix: linePrefix
        ) {
            _description = description;
            _children = children ?? new List<DiagnosticsNode>();
            _properties = properties ?? new List<DiagnosticsNode>();
            this.level = level;
            valueObject = value;
            this.allowTruncate = allowTruncate;
        }

        readonly List<DiagnosticsNode> _children;
        readonly List<DiagnosticsNode> _properties;


        public override DiagnosticLevel level {
            get;
        }
        
        readonly string _description;
        
        public override object valueObject { get; }

        public readonly bool allowTruncate;

        public override List<DiagnosticsNode> getChildren() => _children;

        public override List<DiagnosticsNode> getProperties() => _properties;

        public override string toDescription(TextTreeConfiguration parentConfiguration = null) => _description;
    }

    public class MessageProperty : DiagnosticsProperty<object> {
        public MessageProperty(string name, string message,
            DiagnosticLevel level = DiagnosticLevel.info
        ) : base(name, null, description: message, level: level) {
            D.assert(name != null);
            D.assert(message != null);
        }
    }

    public class StringProperty : DiagnosticsProperty<string> {
        public StringProperty(string name, string value,
            string description = null,
            string tooltip = null,
            bool showName = true,
            object defaultValue = null,
            bool quoted = true,
            string ifEmpty = null,
            DiagnosticLevel level = DiagnosticLevel.info
        ) : base(name,
            value,
            description: description,
            defaultValue: defaultValue,
            tooltip: tooltip,
            showName: showName,
            ifEmpty: ifEmpty,
            level: level) {
            this.quoted = quoted;
        }

        public readonly bool quoted;

        public override Dictionary<string, object> toJsonMap(DiagnosticsSerializationDelegate Delegate) {
            var json = base.toJsonMap(Delegate);
            json["quoted"] = quoted;
            return json;
        }

        protected override string valueToString(TextTreeConfiguration parentConfiguration = null) {
            string text = _description ?? value;
            if (parentConfiguration != null &&
                !parentConfiguration.lineBreakProperties &&
                text != null) {
                text = text.Replace("\n", "\\n");
            }

            if (quoted && text != null) {
                if (ifEmpty != null && text.isEmpty()) {
                    return ifEmpty;
                }

                return "\"" + text + "\"";
            }

            return text ?? "null";
        }
    }

    public abstract class _NumProperty<T> : DiagnosticsProperty<T> {
        internal _NumProperty(string name,
            T value,
            string ifNull = null,
            string unit = null,
            bool showName = true,
            object defaultValue = null,
            string tooltip = null,
            DiagnosticLevel level = DiagnosticLevel.info
        ) : base(
            name,
            value,
            ifNull: ifNull,
            showName: showName,
            defaultValue: defaultValue,
            tooltip: tooltip,
            level: level
        ) {
            this.unit = unit;
        }

        internal _NumProperty(string name,
            ComputePropertyValueCallback<T> computeValue,
            string ifNull = null,
            string unit = null,
            bool showName = true,
            object defaultValue = null,
            string tooltip = null,
            DiagnosticLevel level = DiagnosticLevel.info
        ) : base(
            name,
            computeValue,
            ifNull: ifNull,
            showName: showName,
            defaultValue: defaultValue,
            tooltip: tooltip,
            level: level
        ) {
            this.unit = unit;
        }

        public override Dictionary<string, object> toJsonMap(DiagnosticsSerializationDelegate Delegate) {
            var json = base.toJsonMap(Delegate);
            if (unit != null) {
                json["unit"] = unit;
            }

            json["numberToString"] = numberToString();
            return json;
        }

        public readonly string unit;

        protected abstract string numberToString();

        protected override string valueToString(TextTreeConfiguration parentConfiguration = null) {
            if (value == null) {
                return "null";
            }

            return unit != null ? numberToString() + unit : numberToString();
        }
    }

    public class IntProperty : _NumProperty<int?> {
        public IntProperty(string name, int? value,
            string ifNull = null,
            bool showName = true,
            string unit = null,
            object defaultValue = null,
            DiagnosticLevel level = DiagnosticLevel.info
        ) : base(
            name,
            value,
            ifNull: ifNull,
            showName: showName,
            unit: unit,
            defaultValue: defaultValue,
            level: level
        ) {
        }

        protected override string numberToString() {
            if (value == null) {
                return "null";
            }

            return value.Value.ToString();
        }
    }

    public class FloatProperty : _NumProperty<float?> {
        public FloatProperty(string name, float? value,
            string ifNull = null,
            string unit = null,
            string tooltip = null,
            object defaultValue = null,
            bool showName = true,
            DiagnosticLevel level = DiagnosticLevel.info
        ) : base(
            name,
            value,
            ifNull: ifNull,
            unit: unit,
            tooltip: tooltip,
            defaultValue: defaultValue,
            showName: showName,
            level: level
        ) {
        }

        FloatProperty(
            string name,
            ComputePropertyValueCallback<float?> computeValue,
            string ifNull = null,
            bool showName = true,
            string unit = null,
            string tooltip = null,
            object defaultValue = null,
            DiagnosticLevel level = DiagnosticLevel.info
        ) : base(
            name,
            computeValue,
            showName: showName,
            ifNull: ifNull,
            unit: unit,
            tooltip: tooltip,
            defaultValue: defaultValue,
            level: level
        ) {
        }

        public static FloatProperty lazy(
            string name,
            ComputePropertyValueCallback<float?> computeValue,
            string ifNull = null,
            bool showName = true,
            string unit = null,
            string tooltip = null,
            object defaultValue = null,
            DiagnosticLevel level = DiagnosticLevel.info
        ) {
            return new FloatProperty(
                name,
                computeValue,
                showName: showName,
                ifNull: ifNull,
                unit: unit,
                tooltip: tooltip,
                defaultValue: defaultValue,
                level: level
            );
        }

        protected override string numberToString() {
            if (value != null) {
                return value.Value.ToString("F1");
            }

            return "null";
        }
    }

    public class PercentProperty : FloatProperty {
        public PercentProperty(string name, float fraction,
            string ifNull = null,
            bool showName = true,
            string tooltip = null,
            string unit = null,
            DiagnosticLevel level = DiagnosticLevel.info
        ) : base(
            name,
            fraction,
            ifNull: ifNull,
            showName: showName,
            tooltip: tooltip,
            unit: unit,
            level: level
        ) {
        }

        protected override string valueToString(TextTreeConfiguration parentConfiguration = null) {
            if (value == null) {
                return "null";
            }

            return unit != null ? numberToString() + " " + unit : numberToString();
        }

        protected override string numberToString() {
            if (value == null) {
                return "null";
            }

            return (value.Value.clamp(0.0f, 1.0f) * 100).ToString("F1") + "%";
        }
    }

    public class FlagProperty : DiagnosticsProperty<bool?> {
        public FlagProperty(string name,
            bool? value,
            string ifTrue = null,
            string ifFalse = null,
            bool showName = false,
            object defaultValue = null,
            DiagnosticLevel level = DiagnosticLevel.info
        ) : base(name,
            value,
            showName: showName,
            defaultValue: defaultValue,
            level: level
        ) {
            D.assert(ifTrue != null || ifFalse != null);
        }

        public override Dictionary<string, object> toJsonMap(DiagnosticsSerializationDelegate Delegate) {
            var json = base.toJsonMap(Delegate);
            if (ifTrue != null) {
                json["ifTrue"] = ifTrue;
            }

            if (ifFalse != null) {
                json["ifFalse"] = ifFalse;
            }

            return json;
        }

        public readonly string ifTrue;

        public readonly string ifFalse;

        protected override string valueToString(TextTreeConfiguration parentConfiguration = null) {
            if (value == true) {
                if (ifTrue != null) {
                    return ifTrue;
                }
            }
            else if (value == false) {
                if (ifFalse != null) {
                    return ifFalse;
                }
            }

            return base.valueToString(parentConfiguration: parentConfiguration);
        }

        public override bool showName {
            get {
                if (value == null || value == true && ifTrue == null ||
                    value == false && ifFalse == null) {
                    return true;
                }

                return base.showName;
            }
        }

        public override DiagnosticLevel level {
            get {
                if (value == true) {
                    if (ifTrue == null) {
                        return DiagnosticLevel.hidden;
                    }
                }

                if (value == false) {
                    if (ifFalse == null) {
                        return DiagnosticLevel.hidden;
                    }
                }

                return base.level;
            }
        }
    }

    public class EnumerableProperty<T> : DiagnosticsProperty<IEnumerable<T>> {
        public EnumerableProperty(
            string name,
            IEnumerable<T> value,
            object defaultValue = null,
            string ifNull = null,
            string ifEmpty = "[]",
            DiagnosticsTreeStyle style = DiagnosticsTreeStyle.singleLine,
            bool showName = true,
            DiagnosticLevel level = DiagnosticLevel.info
        ) : base(
            name,
            value,
            defaultValue: defaultValue,
            ifNull: ifNull,
            ifEmpty: ifEmpty,
            style: style,
            showName: showName,
            level: level
        ) {
        }

        protected override string valueToString(TextTreeConfiguration parentConfiguration = null) {
            if (value == null) {
                return "null";
            }

            if (!value.Any()) {
                return ifEmpty ?? "[]";
            }

            if (parentConfiguration != null && !parentConfiguration.lineBreakProperties) {
                return string.Join(", ", value.Select(v => v.ToString()).ToArray());
            }

            return string.Join(style == DiagnosticsTreeStyle.singleLine ? ", " : "\n",
                value.Select(v => v.ToString()).ToArray());
        }

        public override DiagnosticLevel level {
            get {
                if (ifEmpty == null &&
                    value != null && !value.Any()
                    && base.level != DiagnosticLevel.hidden) {
                    return DiagnosticLevel.fine;
                }

                return base.level;
            }
        }

        public override Dictionary<string, object> toJsonMap(DiagnosticsSerializationDelegate Delegate) {
            var json = base.toJsonMap(Delegate);
            if (value != null) {
                json["values"] = value.Select(v => v.ToString()).ToList();
            }

            return json;
        }
    }


    public class EnumProperty<T> : DiagnosticsProperty<T> {
        public EnumProperty(string name, T value,
            object defaultValue = null,
            DiagnosticLevel level = DiagnosticLevel.info
        ) : base(
            name,
            value,
            defaultValue: defaultValue,
            level: level
        ) {
        }

        protected override string valueToString(TextTreeConfiguration parentConfiguration = null) {
            if (value == null) {
                return "null";
            }

            return value.ToString();
        }
    }

    public class ObjectFlagProperty<T> : DiagnosticsProperty<T> {
        public ObjectFlagProperty(string name, T value,
            string ifPresent = null,
            string ifNull = null,
            bool showName = false,
            DiagnosticLevel level = DiagnosticLevel.info
        ) : base(
            name,
            value,
            showName: showName,
            ifNull: ifNull,
            level: level
        ) {
            D.assert(ifPresent != null || ifNull != null);
        }

        ObjectFlagProperty(
            string name,
            T value,
            DiagnosticLevel level = DiagnosticLevel.info
        ) : base(
            name,
            value,
            showName: false,
            level: level
        ) {
            D.assert(name != null);
            ifPresent = "has " + name;
        }

        public static ObjectFlagProperty<T> has(
            string name,
            T value,
            DiagnosticLevel level = DiagnosticLevel.info
        ) {
            return new ObjectFlagProperty<T>(name, value, level);
        }

        public readonly string ifPresent;

        protected override string valueToString(
            TextTreeConfiguration parentConfiguration = null) {
            if (value != null) {
                if (ifPresent != null) {
                    return ifPresent;
                }
            }
            else {
                if (ifNull != null) {
                    return ifNull;
                }
            }

            return base.valueToString(parentConfiguration: parentConfiguration);
        }

        public override bool showName {
            get {
                if ((value != null && ifPresent == null) || (value == null && ifNull == null)) {
                    return true;
                }

                return base.showName;
            }
        }

        public override DiagnosticLevel level {
            get {
                if (value != null) {
                    if (ifPresent == null) {
                        return DiagnosticLevel.hidden;
                    }
                }
                else {
                    if (ifNull == null) {
                        return DiagnosticLevel.hidden;
                    }
                }

                return base.level;
            }
        }

        public override Dictionary<string, object> toJsonMap(DiagnosticsSerializationDelegate Delegate) {
            var json = base.toJsonMap(Delegate);
            if (ifPresent != null) {
                json["ifPresent"] = ifPresent;
            }

            return json;
        }
    }
    public abstract class DiagnosticsSerializationDelegate {
        public DiagnosticsSerializationDelegate(
            int subtreeDepth = 0,
            bool includeProperties = false
        ) { 
            new _DefaultDiagnosticsSerializationDelegate(includeProperties,subtreeDepth);
        }
        public abstract Dictionary<string, object> additionalNodeProperties(DiagnosticsNode node);
        public abstract List<DiagnosticsNode> filterChildren(List<DiagnosticsNode> nodes, DiagnosticsNode owner);

        public abstract List<DiagnosticsNode> filterProperties(List<DiagnosticsNode> nodes, DiagnosticsNode owner);

        public abstract List<DiagnosticsNode> truncateNodesList(List<DiagnosticsNode> nodes, DiagnosticsNode owner);

        public abstract DiagnosticsSerializationDelegate delegateForNode(DiagnosticsNode node);

        public int subtreeDepth {
            get { return 0; }
        }
        public bool includeProperties {
            get { return false; }
        }
        public bool expandPropertyValues {

            get { return false; }
        }
        public abstract DiagnosticsSerializationDelegate copyWith(
            int? subtreeDepth = null,
            bool? includeProperties = null
          );
    }

    class _DefaultDiagnosticsSerializationDelegate : DiagnosticsSerializationDelegate {
        public _DefaultDiagnosticsSerializationDelegate(
            bool includeProperties = false,
            int subtreeDepth = 0
        ) {
            this.includeProperties = includeProperties;
            this.subtreeDepth = subtreeDepth;
        }
        public override Dictionary<string, object> additionalNodeProperties(DiagnosticsNode node) {
            return new Dictionary<string, object>();
        }
        public  override  DiagnosticsSerializationDelegate delegateForNode(DiagnosticsNode node) {
            return subtreeDepth > 0 ? copyWith(subtreeDepth: subtreeDepth - 1) : this;
        }
        bool  expandPropertyValues  {
            get { return false; }
        }
        public  override List<DiagnosticsNode> filterChildren(List<DiagnosticsNode> nodes, DiagnosticsNode owner) {
            return nodes;
        }
        public override List<DiagnosticsNode> filterProperties(List<DiagnosticsNode> nodes, DiagnosticsNode owner) {
            return nodes;
        }

        public readonly  bool includeProperties;

        public readonly int subtreeDepth;

        public override List<DiagnosticsNode> truncateNodesList(List<DiagnosticsNode> nodes, DiagnosticsNode owner) {
            return nodes;
        }

        public override DiagnosticsSerializationDelegate copyWith( int? subtreeDepth = null, bool? includeProperties = null) {
            return new _DefaultDiagnosticsSerializationDelegate(
                subtreeDepth: subtreeDepth ?? this.subtreeDepth,
            includeProperties: includeProperties ?? this.includeProperties
            );
        }
    }

    class FlagsSummary<T> : DiagnosticsProperty<Dictionary<String, T>> { 
        public FlagsSummary(
            string name = null,
            Dictionary<string, T> value = null, 
            string ifEmpty = null,
            bool showName = true,
            bool showSeparator = true,
            DiagnosticLevel level  = DiagnosticLevel.info
        ) : base(
            name,
            value,
            ifEmpty: ifEmpty,
            showName: showName,
            showSeparator: showSeparator,
            level: level
        ){
            D.assert(value != null);
            D.assert(showName != null);
            D.assert(showSeparator != null);
            D.assert(level != null);
            
        }

        protected override string valueToString(TextTreeConfiguration parentConfiguration = null) {
            D.assert(value != null);
            if (!_hasNonNullEntry() && ifEmpty != null)
              return ifEmpty;

            IEnumerable<string> formattedValues = _formattedValues();
            if (parentConfiguration != null && !parentConfiguration.lineBreakProperties) {
              
              return $"{string.Join(",",formattedValues)}";
            }
            return string.Join((DiagnosticUtils._isSingleLine((DiagnosticsTreeStyle)style) ? "," : "\n"),formattedValues);
        }

        
        DiagnosticLevel level {
            get{
                if (!_hasNonNullEntry() && ifEmpty == null)
                    return DiagnosticLevel.hidden;
                return base.level;
            }
        }
        public override Dictionary<string, object> toJsonMap(DiagnosticsSerializationDelegate Delegate) 
        {
            Dictionary<string, object> json = base.toJsonMap(Delegate);
            if (value.isNotEmpty())
              json["values"] = _formattedValues().ToList();
            return json;
        }

        bool _hasNonNullEntry() {
            bool result = false;
            foreach (var o in value.Values) {
                if (o != null) {
                    result = true;
                    return true;
                }

            }
            return result;
        }
       
        IEnumerable<string> _formattedValues() {
            List<string> results = new List<string>();
            foreach (var o in value.Keys) {
                if (value[o] != null) {
                    results.Add(o);
                }
            }
            return results;
        }
    }
    public delegate T ComputePropertyValueCallback<T>();

    public class DiagnosticsProperty<T> : DiagnosticsNode {
        public DiagnosticsProperty(
            string name,
            T value,
            string description = null,
            string ifNull = null,
            string ifEmpty = null,
            bool showName = true,
            bool showSeparator = true,
            object defaultValue = null,
            string tooltip = null,
            bool missingIfNull = false,
            DiagnosticsTreeStyle style = DiagnosticsTreeStyle.singleLine,
            DiagnosticLevel level = DiagnosticLevel.info
        ) : base(
            name: name,
            showName: showName,
            showSeparator: showSeparator,
            style: style
        ) {
            defaultValue = defaultValue ?? foundation_.kNoDefaultValue;
            if (defaultValue == foundation_.kNullDefaultValue) {
                defaultValue = null;
            }

            D.assert(defaultValue == null || defaultValue == foundation_.kNoDefaultValue || defaultValue is T);
            _description = description;
            _valueComputed = true;
            _value = value;
            _computeValue = null;
            this.ifNull = ifNull ?? (missingIfNull ? "MISSING" : null);
            _defaultLevel = level;
            this.ifEmpty = ifEmpty;
            this.defaultValue = defaultValue;
            this.tooltip = tooltip;
            this.missingIfNull = missingIfNull;
        }

        internal DiagnosticsProperty(
            string name,
            ComputePropertyValueCallback<T> computeValue,
            string description = null,
            string ifNull = null,
            string ifEmpty = null,
            bool showName = true,
            bool showSeparator = true,
            object defaultValue = null,
            string tooltip = null,
            bool missingIfNull = false,
            DiagnosticsTreeStyle style = DiagnosticsTreeStyle.singleLine,
            DiagnosticLevel level = DiagnosticLevel.info
        ) : base(
            name: name,
            showName: showName,
            showSeparator: showSeparator,
            style: style
        ) {
            defaultValue = defaultValue ?? foundation_.kNoDefaultValue;
            if (defaultValue == foundation_.kNullDefaultValue) {
                defaultValue = null;
            }

            D.assert(defaultValue == null || defaultValue == foundation_.kNoDefaultValue || defaultValue is T);
            _description = description;
            _valueComputed = false;
            _value = default(T);
            _computeValue = computeValue;
            _defaultLevel = level;
            this.ifNull = ifNull ?? (missingIfNull ? "MISSING" : null);
            this.ifEmpty = ifEmpty;
            this.defaultValue = defaultValue;
            this.tooltip = tooltip;
            this.missingIfNull = missingIfNull;
        }

        public static DiagnosticsProperty<T> lazy(
            string name,
            ComputePropertyValueCallback<T> computeValue,
            string description = null,
            string ifNull = null,
            string ifEmpty = null,
            bool showName = true,
            bool showSeparator = true,
            object defaultValue = null,
            string tooltip = null,
            bool missingIfNull = false,
            DiagnosticsTreeStyle style = DiagnosticsTreeStyle.singleLine,
            DiagnosticLevel level = DiagnosticLevel.info
        ) {
            return new DiagnosticsProperty<T>(
                name,
                computeValue,
                description,
                ifNull,
                ifEmpty,
                showName,
                showSeparator,
                defaultValue,
                tooltip,
                missingIfNull,
                style,
                level);
        }

        internal readonly string _description;

        public override Dictionary<string, object> toJsonMap(DiagnosticsSerializationDelegate  Delegate){
            T v = value;
            List<Dictionary<string, object>> properties = new List<Dictionary<string, object>>();
            if (Delegate.expandPropertyValues && Delegate.includeProperties && v is Diagnosticable vDiagnosticable && getProperties().isEmpty()) {
                // Exclude children for expanded nodes to avoid cycles.
                Delegate = Delegate.copyWith(subtreeDepth: 0, includeProperties: false);
                properties = DiagnosticsNode.toJsonList(
                    Delegate.filterProperties(vDiagnosticable.toDiagnosticsNode().getProperties(), this),
                    this,
                    Delegate
                );
            }
            var json = base.toJsonMap(Delegate);
            if (properties != null) {
                json["properties"] = properties;
            }
            if (defaultValue != foundation_.kNoDefaultValue) {
                json["defaultValue"] = Convert.ToString(defaultValue);
            }

            if (ifEmpty != null) {
                json["ifEmpty"] = ifEmpty;
            }

            if (ifNull != null) {
                json["ifNull"] = ifNull;
            }

            if (tooltip != null) {
                json["tooltip"] = tooltip;
            }
            json["missingIfNull"] = missingIfNull;
            if (exception != null) {
                json["exception"] = exception.ToString();
            }
            json["propertyType"] = propertyType.ToString();
            json["defaultLevel"] = DiagnosticUtils.describeEnum(_defaultLevel);
            
            if (value is Diagnosticable || value is DiagnosticsNode)
                json["isDiagnosticableValue"] = true;
            if (v is int)
                json["value"] = Convert.ToInt32(v);
            else if (v is float) {
                json["value"] = float.Parse(v.ToString());
            }

            if (value is string || value is bool || value == null)
                json["value"] = value;

            return json;
        }

        protected virtual string valueToString(
            TextTreeConfiguration parentConfiguration = null
        ) {
            var v = value;
            var tree = v as DiagnosticableTree;
            return tree != null ? tree.toStringShort() : v != null ? v.ToString() : "null";
        }

        public override string toDescription(
            TextTreeConfiguration parentConfiguration = null
        ) {
            if (_description != null) {
                return _addTooltip(_description);
            }

            if (exception != null) {
                return "EXCEPTION (" + exception.GetType() + ")";
            }

            if (ifNull != null && value == null) {
                return _addTooltip(ifNull);
            }

            string result = valueToString(parentConfiguration: parentConfiguration);
            if (result.isEmpty() && ifEmpty != null) {
                result = ifEmpty;
            }

            return _addTooltip(result);
        }

        string _addTooltip(string text) {
            D.assert(text != null);
            return tooltip == null ? text : text + "(" + tooltip + ")";
        }

        public readonly string ifNull;
        public readonly string ifEmpty;
        public readonly string tooltip;
        public readonly bool missingIfNull;

        public Type propertyType {
            get { return typeof(T); }
        }

        public override object valueObject {
            get { return value; }
        }

        public T value {
            get {
                _maybeCacheValue();
                return _value;
            }
        }

        T _value;
        bool _valueComputed;
        Exception _exception;

        public Exception exception {
            get {
                _maybeCacheValue();
                return _exception;
            }
        }

        void _maybeCacheValue() {
            if (_valueComputed) {
                return;
            }

            _valueComputed = true;
            try {
                _value = _computeValue();
            }
            catch (Exception ex) {
                _exception = ex;
                _value = default(T);
            }
        }

        public readonly object defaultValue;
        DiagnosticLevel _defaultLevel;

        public override DiagnosticLevel level {
            get {
                if (_defaultLevel == DiagnosticLevel.hidden) {
                    return _defaultLevel;
                }

                if (exception != null) {
                    return DiagnosticLevel.error;
                }

                if (value == null && missingIfNull) {
                    return DiagnosticLevel.warning;
                }

                if (defaultValue != foundation_.kNoDefaultValue && Equals(value, defaultValue)) {
                    return DiagnosticLevel.fine;
                }

                return _defaultLevel;
            }
        }

        readonly ComputePropertyValueCallback<T> _computeValue;

        public override List<DiagnosticsNode> getProperties() {
            return new List<DiagnosticsNode>();
        }

        public override List<DiagnosticsNode> getChildren() {
            return new List<DiagnosticsNode>();
        }
    }

    public class DiagnosticableNode<T> : DiagnosticsNode where T : Diagnosticable {
        public DiagnosticableNode(
            string name = null,
            T value = null,
            DiagnosticsTreeStyle? style = null
        ) : base(name: name, style: style) {
            D.assert(value != null);
            _value = value;
        }

        public override object valueObject {
            get { return value; }
        }

        public T value {
            get { return _value; }
        }

        readonly T _value;
        DiagnosticPropertiesBuilder _cachedBuilder;

        DiagnosticPropertiesBuilder _builder {
            get {
                if (_cachedBuilder == null) {
                    _cachedBuilder = new DiagnosticPropertiesBuilder();
                    if (_value != null) {
                        _value.debugFillProperties(_cachedBuilder);
                    }
                }

                return _cachedBuilder;
            }
        }

        public override DiagnosticsTreeStyle? style {
            get { return base.style ?? _builder.defaultDiagnosticsTreeStyle; }
        }

        public override string emptyBodyDescription {
            get { return _builder.emptyBodyDescription; }
        }

        public override List<DiagnosticsNode> getProperties() {
            return _builder.properties;
        }

        public override List<DiagnosticsNode> getChildren() {
            return new List<DiagnosticsNode>();
        }

        public override string toDescription(
            TextTreeConfiguration parentConfiguration = null
        ) {
            return _value.toStringShort();
        }
    }

    class _DiagnosticableTreeNode : DiagnosticableNode<DiagnosticableTree> {
        internal _DiagnosticableTreeNode(
            string name,
            DiagnosticableTree value,
            DiagnosticsTreeStyle style
        ) : base(
            name: name,
            value: value,
            style: style
        ) {
        }

        public override List<DiagnosticsNode> getChildren() {
            if (value != null) {
                return value.debugDescribeChildren();
            }

            return new List<DiagnosticsNode>();
        }
    }

    public static partial class foundation_ {
        public static string shortHash(object o) {
            return (o.GetHashCode() & 0xFFFFF).ToString("X").PadLeft(5, '0');
        }

        public static string describeIdentity(object o) {
            return $"{o.GetType()}#{shortHash(o)}";
        }
    }

    public class DiagnosticPropertiesBuilder {
        public void add(DiagnosticsNode property) {
            properties.Add(property);
        }

        public readonly List<DiagnosticsNode> properties = new List<DiagnosticsNode>();
        public DiagnosticsTreeStyle defaultDiagnosticsTreeStyle = DiagnosticsTreeStyle.sparse;
        public string emptyBodyDescription;
    }

    public abstract class Diagnosticable {
        protected Diagnosticable() {
        }

        public virtual string toStringShort() {
            return foundation_.describeIdentity(this);
        }

        public override string ToString() {
            return toString();
        }

        public virtual string toString(DiagnosticLevel minLevel = DiagnosticLevel.debug) {
            string fullString = null;
            D.assert(() => {
                fullString = toDiagnosticsNode(style: DiagnosticsTreeStyle.singleLine)
                    .toString(minLevel: minLevel);
                return true;
            });
            return fullString ?? toStringShort();
        }

        public virtual DiagnosticsNode toDiagnosticsNode(
            string name = null,
            DiagnosticsTreeStyle style = DiagnosticsTreeStyle.sparse ) {
            return new DiagnosticableNode<Diagnosticable>(
                name: name, value: this, style: style
            );
        }

        public virtual void debugFillProperties(DiagnosticPropertiesBuilder properties) {
        }
    }

    public abstract class DiagnosticableTree : Diagnosticable {
        protected DiagnosticableTree() {
        }

        public virtual string toStringShallow(
            string joiner = ", ",
            DiagnosticLevel minLevel = DiagnosticLevel.debug
        ) {
            var result = new StringBuilder();
            result.Append(ToString());
            result.Append(joiner);
            DiagnosticPropertiesBuilder builder = new DiagnosticPropertiesBuilder();
            debugFillProperties(builder);
            result.Append(string.Join(joiner,
                builder.properties.Where(n => !n.isFiltered(minLevel)).Select(n => n.ToString()).ToArray())
            );
            return result.ToString();
        }

        public virtual string toStringDeep(
            string prefixLineOne = "",
            string prefixOtherLines = null,
            DiagnosticLevel minLevel = DiagnosticLevel.debug
        ) {
            return toDiagnosticsNode().toStringDeep(
                prefixLineOne: prefixLineOne,
                prefixOtherLines: prefixOtherLines,
                minLevel: minLevel);
        }

        public override string toStringShort() {
            return foundation_.describeIdentity(this);
        }

        public override DiagnosticsNode toDiagnosticsNode(
            string name = null,
            DiagnosticsTreeStyle style = DiagnosticsTreeStyle.sparse ) {
            
            return new _DiagnosticableTreeNode(
                name: name,
                value: this,
                style: style
            );
        }

        public virtual List<DiagnosticsNode> debugDescribeChildren() {
            return new List<DiagnosticsNode>();
        }
    }
}