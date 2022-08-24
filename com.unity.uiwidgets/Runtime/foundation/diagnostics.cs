using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Unity.UIWidgets.external;
using Unity.UIWidgets.ui;
using UnityEngine;

namespace Unity.UIWidgets.foundation {
    public enum DiagnosticLevel {
        hidden,
        fine,
        debug,
        info,
        warning,
        hint,
        summary,
        error,
        off,
    }

    public enum DiagnosticsTreeStyle {
        none,
        sparse,
        offstage,
        dense,
        transition,
        error,
        whitespace,
        flat,
        singleLine,
        errorProperty,
        shallow,
        truncateChildren
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
            string afterDescription = "",
            string beforeProperties = "",
            string afterProperties = "",
            string mandatoryAfterProperties = "",
            string propertySeparator = "",
            string bodyIndent = "",
            string footer = "",
            bool showChildren = true,
            bool addBlankLineIfNoChildren = true,
            bool isNameOnOwnLine = false,
            bool isBlankLineBetweenPropertiesAndChildren = true,
            string beforeName = "",
            string suffixLineOne = "",
            string mandatoryFooter = ""
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
            D.assert(afterDescription != null);
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

        public readonly string suffixLineOne;

        public readonly string prefixOtherLines;

        public readonly string prefixLastChildLineOne;

        public readonly string prefixOtherLinesRootNode;

        public readonly string propertyPrefixIfChildren;

        public readonly string propertyPrefixNoChildren;

        public readonly string linkCharacter;

        public readonly string childLinkSpace;

        public readonly string lineBreak;

        public readonly bool lineBreakProperties;

        public readonly string beforeName;

        public readonly string afterName;

        public readonly string afterDescriptionIfBody;

        public readonly string afterDescription;

        public readonly string beforeProperties;

        public readonly string afterProperties;

        public readonly string mandatoryAfterProperties;

        public readonly string propertySeparator;

        public readonly string bodyIndent;

        public readonly bool showChildren;

        public readonly bool addBlankLineIfNoChildren;

        public readonly bool isNameOnOwnLine;

        public readonly string footer;

        public readonly string mandatoryFooter;

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
            footer: " ╚═══════════",
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

        public static readonly TextTreeConfiguration errorTextConfiguration = new TextTreeConfiguration(
            prefixLineOne: "╞═╦",
            prefixLastChildLineOne: "╘═╦",
            prefixOtherLines: " ║ ",
            footer: " ╚═══════════",
            linkCharacter: "│",
            propertyPrefixIfChildren: "",
            propertyPrefixNoChildren: "",
            prefixOtherLinesRootNode: "",
            beforeName: "══╡ ",
            suffixLineOne: " ╞══",
            mandatoryFooter: "═════",
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

        public static readonly TextTreeConfiguration flatTextConfiguration = new TextTreeConfiguration(
            prefixLineOne: "",
            prefixLastChildLineOne: "",
            prefixOtherLines: "",
            prefixOtherLinesRootNode: "",
            bodyIndent: "",
            propertyPrefixIfChildren: "",
            propertyPrefixNoChildren: "",
            linkCharacter: "",
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
            propertyPrefixIfChildren: "  ",
            propertyPrefixNoChildren: "  ",
            linkCharacter: "",
            prefixOtherLinesRootNode: ""
        );

        public static readonly TextTreeConfiguration errorPropertyTextConfiguration = new TextTreeConfiguration(
            propertySeparator: ", ",
            beforeProperties: "(",
            afterProperties: ")",
            prefixLineOne: "",
            prefixOtherLines: "",
            prefixLastChildLineOne: "",
            lineBreak: "\n",
            lineBreakProperties: false,
            addBlankLineIfNoChildren: false,
            showChildren: false,
            propertyPrefixIfChildren: "  ",
            propertyPrefixNoChildren: "  ",
            linkCharacter: "",
            prefixOtherLinesRootNode: "",
            afterName: ":",
            isNameOnOwnLine: true
        );

        public static readonly TextTreeConfiguration shallowTextConfiguration = new TextTreeConfiguration(
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
            isBlankLineBetweenPropertiesAndChildren: false,
            showChildren: false
        );
        
        public static string shortHash(object o) {
            return (o.GetHashCode() & 0xFFFFF).ToString("X").PadLeft(5, '0');
        }

        public static string describeIdentity(object o) {
            return $"{o.GetType()}#{shortHash(o)}";
        }
    }

    enum _WordWrapParseMode {
        inSpace,
        inWord,
        atBreak
    }

    class _PrefixedStringBuilder {
        internal _PrefixedStringBuilder(string prefixLineOne, string prefixOtherLines, int? wrapWidth = null) {
            this.prefixLineOne = prefixLineOne;
            _prefixOtherLines = prefixOtherLines;
            this.wrapWidth = wrapWidth;
        }

        public readonly string prefixLineOne;

        public string prefixOtherLines {
            get { return _nextPrefixOtherLines ?? _prefixOtherLines; }
            set {
                _prefixOtherLines = value;
                _nextPrefixOtherLines = null;
            }
        }

        string _prefixOtherLines;

        string _nextPrefixOtherLines;

        public void incrementPrefixOtherLines(string suffix, bool updateCurrentLine) {
            if (_currentLine.Length == 0 || updateCurrentLine) {
                _prefixOtherLines = prefixOtherLines + suffix;
                _nextPrefixOtherLines = null;
            }
            else {
                _nextPrefixOtherLines = prefixOtherLines + suffix;
            }
        }

        public readonly int? wrapWidth;

        readonly StringBuilder _buffer = new StringBuilder();

        readonly StringBuilder _currentLine = new StringBuilder();

        readonly List<int> _wrappableRanges = new List<int>();

        public bool requiresMultipleLines {
            get {
                return _numLines > 1 || (_numLines == 1 && _currentLine.Length != 0) ||
                       (_currentLine.Length + _getCurrentPrefix(true).Length > wrapWidth);
            }
        }

        public bool isCurrentLineEmpty {
            get { return _currentLine.Length == 0; }
        }

        int _numLines = 0;

        void _finalizeLine(bool addTrailingLineBreak) {
            bool firstLine = _buffer.Length == 0;
            string text = _currentLine.ToString();
            _currentLine.Clear();

            if (_wrappableRanges.Count == 0) {
                _writeLine(
                    text,
                    includeLineBreak: addTrailingLineBreak,
                    firstLine: firstLine);
                return;
            }

            IEnumerable<string> lines = _wordWrapLine(
                text,
                _wrappableRanges,
                wrapWidth,
                startOffset: firstLine ? prefixLineOne.Length : _prefixOtherLines.Length,
                otherLineOffset: firstLine ? _prefixOtherLines.Length : _prefixOtherLines.Length);

            int i = 0;
            int length = lines.Count();
            foreach (string line in lines) {
                i++;
                _writeLine(
                    line,
                    includeLineBreak: addTrailingLineBreak || i < length,
                    firstLine: firstLine);
            }

            _wrappableRanges.Clear();
        }

        static IEnumerable<string> _wordWrapLine(string message, List<int> wrapRanges, int? width, int startOffset = 0,
            int otherLineOffset = 0) {
            if (message.Length + startOffset < width) {
                yield return message;
            }

            int startForLengthCalculations = -startOffset;
            int index = 0;
            _WordWrapParseMode mode = _WordWrapParseMode.inSpace;
            int? lastWordStart = null;
            int? lastWordEnd = null;
            int start = 0;

            int currentChunk = 0;

            bool noWrap(int index2) {
                while (true) {
                    if (currentChunk >= wrapRanges.Count) {
                        return true;
                    }

                    if (index2 < wrapRanges[currentChunk + 1]) {
                        break;
                    }

                    currentChunk += 2;
                }

                return index2 < wrapRanges[currentChunk];
            }

            while (true) {
                switch (mode) {
                    case _WordWrapParseMode.inSpace: {
                        while ((index < message.Length) && (message[index] == ' ')) {
                            index += 1;
                        }

                        lastWordStart = index;
                        mode = _WordWrapParseMode.inWord;
                        break;
                    }

                    case _WordWrapParseMode.inWord: {
                        while ((index < message.Length) && (message[index] != ' ' || noWrap(index))) {
                            index += 1;
                        }

                        mode = _WordWrapParseMode.atBreak;
                        break;
                    }

                    case _WordWrapParseMode.atBreak: {
                        if ((index - startForLengthCalculations > width) || (index == message.Length)) {
                            if ((index - startForLengthCalculations <= width) || (lastWordEnd == null)) {
                                lastWordEnd = index;
                            }
                            string line = message.Substring(start, lastWordEnd.Value - start);

                            yield return line;
                            if (lastWordEnd.Value >= message.Length) {
                                yield break;
                            }

                            if (lastWordEnd.Value == index) {
                                while ((index < message.Length) && (message[index] == ' ')) {
                                    index += 1;
                                }

                                start = index;
                                mode = _WordWrapParseMode.inWord;
                            }
                            else {
                                D.assert(lastWordStart.Value > lastWordEnd.Value);
                                start = lastWordStart.Value;
                                mode = _WordWrapParseMode.atBreak;
                            }

                            startForLengthCalculations = start - otherLineOffset;
                            lastWordEnd = null;
                        }
                        else {
                            lastWordEnd = index;
                            mode = _WordWrapParseMode.inSpace;
                        }

                        break;
                    }
                }
            }
        }

        public void write(string s, bool allowWrap = false) {
            if (s.isEmpty()) {
                return;
            }

            string[] lines = s.Split('\n');
            for (int i = 0; i < lines.Length; i++) {
                if (i > 0) {
                    _finalizeLine(true);
                    _updatePrefix();
                }

                string line = lines[i];
                if (line.isNotEmpty()) {
                    if (allowWrap && wrapWidth != null) {
                        int wrapStart = _currentLine.Length;
                        int wrapEnd = wrapStart + line.Length;
                        if (_wrappableRanges.isNotEmpty() && _wrappableRanges.last() == wrapStart) {
                            _wrappableRanges[_wrappableRanges.Count - 1] = wrapEnd;
                        }
                        else {
                            _wrappableRanges.Add(wrapStart);
                            _wrappableRanges.Add(wrapEnd);
                        }
                    }

                    _currentLine.Append(line);
                }
            }
        }

        void _updatePrefix() {
            if (_nextPrefixOtherLines != null) {
                _prefixOtherLines = _nextPrefixOtherLines;
                _nextPrefixOtherLines = null;
            }
        }

        void _writeLine(string line, bool includeLineBreak, bool firstLine) {
            line = $"{_getCurrentPrefix(firstLine)}{line}";
            _buffer.Append(line.TrimEnd());
            if (includeLineBreak) {
                _buffer.Append("\n");
            }

            _numLines++;
        }

        string _getCurrentPrefix(bool firstLine) {
            return _buffer.Length == 0 ? prefixLineOne : (firstLine ? _prefixOtherLines : _prefixOtherLines);
        }

        public void writeRawLines(string lines) {
            if (lines.isEmpty()) {
                return;
            }

            if (_currentLine.Length != 0) {
                _finalizeLine(true);
            }

            D.assert(_currentLine.Length == 0);

            _buffer.Append(lines);
            if (!lines.EndsWith("\n")) {
                _buffer.Append("\n");
            }

            _numLines++;
            _updatePrefix();
        }

        public void writeStretched(string text, int targetLineLength) {
            write(text);
            int currentLineLength = _currentLine.Length + _getCurrentPrefix(_buffer.Length == 0).Length;
            D.assert(_currentLine.Length > 0);
            int targetLength = targetLineLength - currentLineLength;
            if (targetLength > 0) {
                D.assert(text.isNotEmpty());
                char lastChar = text[text.Length - 1];
                D.assert(lastChar != '\n');
                _currentLine.Append(new string(lastChar, targetLength));
            }

            _wrappableRanges.Clear();
        }

        public string build() {
            if (_currentLine.Length != 0) {
                _finalizeLine(false);
            }

            return _buffer.ToString();
        }
    }

    public class _NoDefaultValue {
        internal _NoDefaultValue() {
        }
    }

    public class _NullDefaultValue {
        internal _NullDefaultValue() {
        }
    }

    public static partial class foundation_ {
        public static readonly _NoDefaultValue kNoDefaultValue = new _NoDefaultValue();

        public static readonly _NullDefaultValue kNullDefaultValue = new _NullDefaultValue();

        public static bool _isSingleLine(DiagnosticsTreeStyle? style) {
            return style == DiagnosticsTreeStyle.singleLine;
        }

        public static bool FloatEqual(float left, float right, float precisionTolerance = precisionErrorTolerance) {
            return Mathf.Abs(left - right) < precisionTolerance;
        }
    }


    public class TextTreeRenderer {
        public TextTreeRenderer(
            DiagnosticLevel minLevel = DiagnosticLevel.debug,
            int wrapWidth = 100,
            int wrapWidthProperties = 65,
            int maxDescendentsTruncatableNode = -1) {
            _minLevel = minLevel;
            _wrapWidth = wrapWidth;
            _wrapWidthProperties = wrapWidthProperties;
            _maxDescendentsTruncatableNode = maxDescendentsTruncatableNode;
        }


        readonly int _wrapWidth;
        readonly int _wrapWidthProperties;
        readonly DiagnosticLevel _minLevel;
        readonly int _maxDescendentsTruncatableNode;

        TextTreeConfiguration _childTextConfiguration(
            DiagnosticsNode child,
            TextTreeConfiguration textStyle) {
            DiagnosticsTreeStyle? childStyle = child?.style;
            return (foundation_._isSingleLine(childStyle) || childStyle == DiagnosticsTreeStyle.errorProperty)
                ? textStyle
                : child.textTreeConfiguration;
        }

        public string render(
            DiagnosticsNode node,
            string prefixLineOne = "",
            string prefixOtherLines = "",
            TextTreeConfiguration parentConfiguration = null
        ) {
            if (foundation_.kReleaseMode) {
                return "";
            }
            else {
                return _debugRender(
                    node,
                    prefixLineOne: prefixLineOne,
                    prefixOtherLines: prefixOtherLines,
                    parentConfiguration: parentConfiguration);
            }
        }

        string _debugRender(
            DiagnosticsNode node,
            string prefixLineOne = "",
            string prefixOtherLines = "",
            TextTreeConfiguration parentConfiguration = null
        ) {
            bool isSingleLine = foundation_._isSingleLine(node.style) &&
                                parentConfiguration?.lineBreakProperties != true;
            prefixOtherLines = prefixOtherLines ?? prefixLineOne;
            if (node.linePrefix != null) {
                prefixLineOne += node.linePrefix;
                prefixOtherLines += node.linePrefix;
            }

            TextTreeConfiguration config = node.textTreeConfiguration;
            if (prefixOtherLines.isEmpty()) {
                prefixOtherLines += config.prefixOtherLinesRootNode;
            }

            if (node.style == DiagnosticsTreeStyle.truncateChildren) {
                List<string> descendants = new List<string>();
                const int maxDepth = 5;
                int depth = 0;
                const int maxLines = 25;
                int lines = 0;

                void visitor(DiagnosticsNode node2) {
                    foreach (DiagnosticsNode child in node2.getChildren()) {
                        if (lines < maxLines) {
                            depth += 1;
                            descendants.Add($"{prefixOtherLines}{new string(' ', depth)}{child}");
                            if (depth < maxDepth) {
                                visitor(child);
                            }

                            depth -= 1;
                        }
                        else if (lines == maxLines) {
                            descendants.Add($"{prefixOtherLines}  ...(descendants list truncated after {lines} lines)");
                        }

                        lines += 1;
                    }
                }

                visitor(node);
                StringBuilder information = new StringBuilder(prefixLineOne);
                if (lines > 1) {
                    information.Append(
                        $"This {node.name} had the following descendants (showing up to depth {maxDepth}):");
                }
                else if (descendants.Count == 1) {
                    information.Append($"This {node.name} had the following child:");
                }
                else {
                    information.Append($"This {node.name} has no descendants.");
                }

                foreach (var line in descendants) {
                    information.AppendLine(line);
                }

                return information.ToString();
            }

            _PrefixedStringBuilder builder = new _PrefixedStringBuilder(
                prefixLineOne: prefixLineOne,
                prefixOtherLines: prefixOtherLines,
                wrapWidth: Mathf.Max(_wrapWidth, prefixOtherLines.Length + _wrapWidthProperties)
            );

            List<DiagnosticsNode> children = node.getChildren();
            string description = node.toDescription(parentConfiguration: parentConfiguration);
            if (config.beforeName.isNotEmpty()) {
                builder.write(config.beforeName);
            }

            bool wrapName = !isSingleLine && node.allowNameWrap;
            bool wrapDescription = !isSingleLine && node.allowWrap;
            bool uppercaseTitle = node.style == DiagnosticsTreeStyle.error;
            string name = node.name;

            if (uppercaseTitle) {
                name = name?.ToUpper();
            }

            if (description == null || description.isEmpty()) {
                if (node.showName && name != null) {
                    builder.write(name, allowWrap: wrapName);
                }
            }
            else {
                bool includeName = false;
                if (name != null && name.isNotEmpty() && node.showName) {
                    includeName = true;
                    builder.write(name, allowWrap: wrapName);
                    if (node.showSeparator) {
                        builder.write(config.afterName, allowWrap: wrapName);
                    }

                    builder.write(
                        config.isNameOnOwnLine || description.Contains('\n') ? "\n" : " ",
                        allowWrap: wrapName
                    );
                }

                if (!isSingleLine && builder.requiresMultipleLines && !builder.isCurrentLineEmpty) {
                    builder.write("\n");
                }

                if (includeName) {
                    builder.incrementPrefixOtherLines(
                        children.isEmpty() ? config.propertyPrefixNoChildren : config.propertyPrefixIfChildren,
                        updateCurrentLine: true
                    );
                }

                if (uppercaseTitle) {
                    description = description.ToUpper();
                }

                builder.write(description.TrimEnd(), allowWrap: wrapDescription);

                if (!includeName) {
                    builder.incrementPrefixOtherLines(
                        children.isEmpty() ? config.propertyPrefixNoChildren : config.propertyPrefixIfChildren,
                        updateCurrentLine: false
                    );
                }
            }

            if (config.suffixLineOne.isNotEmpty()) {
                builder.writeStretched(config.suffixLineOne, builder.wrapWidth.Value);
            }

            IEnumerable<DiagnosticsNode> propertiesIterable = LinqUtils<DiagnosticsNode>.WhereList(node.getProperties(), (
                (DiagnosticsNode n) => !n.isFiltered(_minLevel)
            ));
            List<DiagnosticsNode> properties;
            if (_maxDescendentsTruncatableNode >= 0 && node.allowTruncate) {
                if (propertiesIterable.Count() < _maxDescendentsTruncatableNode) {
                    properties =
                        propertiesIterable.Take(_maxDescendentsTruncatableNode).ToList();
                    properties.Add(DiagnosticsNode.message("..."));
                }
                else {
                    properties = propertiesIterable.ToList();
                }

                if (_maxDescendentsTruncatableNode < children.Count) {
                    children = children.Take(_maxDescendentsTruncatableNode).ToList();
                    children.Add(DiagnosticsNode.message("..."));
                }
            }
            else {
                properties = propertiesIterable.ToList();
            }

            if ((properties.isNotEmpty() || children.isNotEmpty() || node.emptyBodyDescription != null) &&
                (node.showSeparator || description?.isNotEmpty() == true)) {
                builder.write(config.afterDescriptionIfBody);
            }

            if (config.lineBreakProperties) {
                builder.write(config.lineBreak);
            }

            if (properties.isNotEmpty()) {
                builder.write(config.beforeProperties);
            }

            builder.incrementPrefixOtherLines(config.bodyIndent, updateCurrentLine: false);

            if (node.emptyBodyDescription != null &&
                properties.isEmpty() &&
                children.isEmpty() &&
                prefixLineOne.isNotEmpty()) {
                builder.write(node.emptyBodyDescription);
                if (config.lineBreakProperties) {
                    builder.write(config.lineBreak);
                }
            }

            for (int i = 0; i < properties.Count; ++i) {
                DiagnosticsNode property = properties[i];
                if (i > 0) {
                    builder.write(config.propertySeparator);
                }

                TextTreeConfiguration propertyStyle = property.textTreeConfiguration;
                if (foundation_._isSingleLine(property.style)) {
                    string propertyRender = render(property,
                        prefixLineOne: propertyStyle.prefixLineOne,
                        prefixOtherLines: $"{propertyStyle.childLinkSpace}{propertyStyle.prefixOtherLines}",
                        parentConfiguration: config
                    );
                    string[] propertyLines = propertyRender.Split('\n');
                    if (propertyLines.Length == 1 && !config.lineBreakProperties) {
                        builder.write(propertyLines.first());
                    }
                    else {
                        builder.write(propertyRender, allowWrap: false);
                        if (!propertyRender.EndsWith("\n")) {
                            builder.write("\n");
                        }
                    }
                }
                else {
                    string propertyRender = render(property,
                        prefixLineOne: $"{builder.prefixOtherLines}{propertyStyle.prefixLineOne}",
                        prefixOtherLines:
                        $"{builder.prefixOtherLines}{propertyStyle.childLinkSpace}{propertyStyle.prefixOtherLines}",
                        parentConfiguration: config
                    );
                    builder.writeRawLines(propertyRender);
                }
            }

            if (properties.isNotEmpty()) {
                builder.write(config.afterProperties);
            }

            builder.write(config.mandatoryAfterProperties);

            if (!config.lineBreakProperties) {
                builder.write(config.lineBreak);
            }

            string prefixChildren = config.bodyIndent;
            string prefixChildrenRaw = $"{prefixOtherLines}{prefixChildren}";
            if (children.isEmpty() &&
                config.addBlankLineIfNoChildren &&
                builder.requiresMultipleLines &&
                builder.prefixOtherLines.TrimEnd().isNotEmpty()
            ) {
                builder.write(config.lineBreak);
            }

            if (children.isNotEmpty() && config.showChildren) {
                if (config.isBlankLineBetweenPropertiesAndChildren &&
                    properties.isNotEmpty() &&
                    children.first().textTreeConfiguration.isBlankLineBetweenPropertiesAndChildren) {
                    builder.write(config.lineBreak);
                }

                builder.prefixOtherLines = prefixOtherLines;

                for (int i = 0; i < children.Count; i++) {
                    DiagnosticsNode child = children[i];
                    D.assert(child != null);
                    TextTreeConfiguration childConfig = _childTextConfiguration(child, config);
                    if (i == children.Count - 1) {
                        string lastChildPrefixLineOne = $"{prefixChildrenRaw}{childConfig.prefixLastChildLineOne}";
                        string childPrefixOtherLines =
                            $"{prefixChildrenRaw}{childConfig.childLinkSpace}{childConfig.prefixOtherLines}";
                        builder.writeRawLines(render(
                            child,
                            prefixLineOne: lastChildPrefixLineOne,
                            prefixOtherLines: childPrefixOtherLines,
                            parentConfiguration: config
                        ));
                        if (childConfig.footer.isNotEmpty()) {
                            builder.prefixOtherLines = prefixChildrenRaw;
                            builder.write($"{childConfig.childLinkSpace}${childConfig.footer}");
                            if (childConfig.mandatoryFooter.isNotEmpty()) {
                                builder.writeStretched(
                                    childConfig.mandatoryFooter,
                                    Mathf.Max(builder.wrapWidth.Value,
                                        _wrapWidthProperties + childPrefixOtherLines.Length)
                                );
                            }

                            builder.write(config.lineBreak);
                        }
                    }
                    else {
                        TextTreeConfiguration nextChildStyle = _childTextConfiguration(children[i + 1], config);
                        string childPrefixLineOne = $"{prefixChildrenRaw}{childConfig.prefixLineOne}";
                        string childPrefixOtherLines =
                            $"{prefixChildrenRaw}{nextChildStyle.linkCharacter}{childConfig.prefixOtherLines}";
                        builder.writeRawLines(render(
                            child,
                            prefixLineOne: childPrefixLineOne,
                            prefixOtherLines: childPrefixOtherLines,
                            parentConfiguration: config
                        ));
                        if (childConfig.footer.isNotEmpty()) {
                            builder.prefixOtherLines = prefixChildrenRaw;
                            builder.write($"{childConfig.linkCharacter}{childConfig.footer}");
                            if (childConfig.mandatoryFooter.isNotEmpty()) {
                                builder.writeStretched(
                                    childConfig.mandatoryFooter,
                                    Mathf.Max(builder.wrapWidth.Value,
                                        _wrapWidthProperties + childPrefixOtherLines.Length)
                                );
                            }

                            builder.write(config.lineBreak);
                        }
                    }
                }
            }

            if (parentConfiguration == null && config.mandatoryFooter.isNotEmpty()) {
                builder.writeStretched(config.mandatoryFooter, builder.wrapWidth.Value);
                builder.write(config.lineBreak);
            }

            return builder.build();
        }
    }


    public abstract class DiagnosticsNode {
        protected DiagnosticsNode(
            string name = null,
            DiagnosticsTreeStyle? style = null,
            bool showName = true,
            bool showSeparator = true,
            string linePrefix = null
        ) {
            D.assert(
                name == null || !name.EndsWith(":"),
                () => "Names of diagnostic nodes must not end with colons.\n" +
                      "name:\n" +
                      $"  {name}");
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
            return new DiagnosticsProperty<object>(
                "",
                null,
                description: message,
                style: style,
                showName: false,
                allowWrap: allowWrap,
                level: level
            );
        }

        public readonly string name;

        public abstract string toDescription(
            TextTreeConfiguration parentConfiguration = null
        );

        public readonly bool showSeparator;

        public virtual object value { get; }

        public bool isFiltered(DiagnosticLevel minLevel) {
            return foundation_.kReleaseMode || level < minLevel;
        }


        public virtual DiagnosticLevel level {
            get { return foundation_.kReleaseMode ? DiagnosticLevel.hidden : DiagnosticLevel.info; }
        }

        public virtual bool showName {
            get { return _showName; }
        }

        readonly bool _showName;

        public readonly string linePrefix;

        public virtual string emptyBodyDescription {
            get { return null; }
        }

        public abstract object valueObject { get; }

        public virtual DiagnosticsTreeStyle? style {
            get { return _style; }
        }

        readonly DiagnosticsTreeStyle? _style;

        public virtual bool allowWrap {
            get { return false; }
        }

        public virtual bool allowNameWrap {
            get { return false; }
        }

        public virtual bool allowTruncate {
            get { return false; }
        }

        public abstract List<DiagnosticsNode> getProperties();

        public abstract List<DiagnosticsNode> getChildren();

        string _separator {
            get { return showSeparator ? ":" : ""; }
        }

        public virtual Dictionary<string, object> toJsonMap(DiagnosticsSerializationDelegate Delegate) {
            Dictionary<string, object> result = new Dictionary<string, object>();
            D.assert(() => {
                bool hasChildren = getChildren().isNotEmpty();
                result = new Dictionary<string, object> { };
                result["description"] = toDescription();
                result["type"] = GetType().ToString();
                if (name != null) {
                    result["name"] = name;
                }

                if (!showSeparator) {
                    result["showSeparator"] = showSeparator;
                }

                if (level != DiagnosticLevel.info) {
                    result["level"] = DiagnosticUtils.describeEnum(level);
                }

                if (showName == false) {
                    result["showName"] = showName;
                }

                if (emptyBodyDescription != null) {
                    result["emptyBodyDescription"] = emptyBodyDescription;
                }

                if (style != DiagnosticsTreeStyle.sparse) {
                    result["style"] = DiagnosticUtils.describeEnum(style);
                }

                if (allowTruncate) {
                    result["allowTruncate"] = allowTruncate;
                }

                if (hasChildren) {
                    result["hasChildren"] = hasChildren;
                }

                if (linePrefix?.isNotEmpty() == true) {
                    result["linePrefix"] = linePrefix;
                }

                if (!allowWrap) {
                    result["allowWrap"] = allowWrap;
                }

                if (allowNameWrap) {
                    result["allowNameWrap"] = allowNameWrap;
                }

                Delegate.additionalNodeProperties(this);
                if (Delegate.includeProperties) {
                    result["properties"] = toJsonList(
                        Delegate.filterProperties(getProperties(), this),
                        this,
                        Delegate
                    );
                }

                if (Delegate.subtreeDepth > 0) {
                    result["children"] = toJsonList(
                        Delegate.filterChildren(getChildren(), this),
                        this,
                        Delegate
                    );
                }

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
            if (nodes == null) {
                return new List<Dictionary<string, object>>();
            }

            int originalNodeCount = nodes.Count;
            nodes = Delegate.truncateNodesList(nodes, parent);
            if (nodes.Count != originalNodeCount) {
                nodes.Add(message("..."));
                truncated = true;
            }

            List<Dictionary<string, object>> json = new List<Dictionary<string, object>>();
            foreach (var node in nodes) {
                json.Add(node.toJsonMap(Delegate.delegateForNode(node)));
            }

            json.ToList();
            if (truncated) {
                json.Last()["truncated"] = true;
            }

            return json;
        }

        public override string ToString() {
            return toString();
        }

        public virtual string toString(
            TextTreeConfiguration parentConfiguration = null,
            DiagnosticLevel minLevel = DiagnosticLevel.info
        ) {
            string result = base.ToString();
            D.assert(style != null);
            D.assert(() => {
                if (foundation_._isSingleLine(style)) {
                    result = toStringDeep(parentConfiguration: parentConfiguration, minLevel: minLevel);
                }
                else {
                    var description = toDescription(parentConfiguration: parentConfiguration);
                    if (name == null || name.isEmpty() || !showName) {
                        result = description;
                    }
                    else {
                        result = description.Contains("\n")
                            ? $"{name}{_separator}\n{description}"
                            : $"{name}{_separator} {description}";
                    }
                }

                return true;
            });

            return result;
        }

        public TextTreeConfiguration textTreeConfiguration {
            get {
                D.assert(style != null);
                switch (style) {
                    case DiagnosticsTreeStyle.none:
                        return null;
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
                    case DiagnosticsTreeStyle.errorProperty:
                        return foundation_.errorPropertyTextConfiguration;
                    case DiagnosticsTreeStyle.shallow:
                        return foundation_.shallowTextConfiguration;
                    case DiagnosticsTreeStyle.error:
                        return foundation_.errorTextConfiguration;
                    case DiagnosticsTreeStyle.truncateChildren:
                        return foundation_.whitespaceTextConfiguration;
                    case DiagnosticsTreeStyle.flat:
                        return foundation_.flatTextConfiguration;
                }

                return null;
            }
        }

        public string toStringDeep(
            string prefixLineOne = "",
            string prefixOtherLines = null,
            TextTreeConfiguration parentConfiguration = null,
            DiagnosticLevel minLevel = DiagnosticLevel.debug) {
            string result = "";
            D.assert(() => {
                result = new TextTreeRenderer(
                    minLevel: minLevel,
                    wrapWidth: 65,
                    wrapWidthProperties: 65
                    ).render(
                    this,
                    prefixLineOne: prefixLineOne,
                    prefixOtherLines: prefixOtherLines,
                    parentConfiguration: parentConfiguration);

                return true;
            });
            return result;
        }
    }

    public class MessageProperty : DiagnosticsProperty<object> {
        public MessageProperty(string name, string message,
            DiagnosticsTreeStyle style = DiagnosticsTreeStyle.singleLine,
            DiagnosticLevel level = DiagnosticLevel.info
        ) : base(name, null, description: message, style: style, level: level) {
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
            DiagnosticsTreeStyle style = DiagnosticsTreeStyle.singleLine,
            DiagnosticLevel level = DiagnosticLevel.info
        ) : base(name,
            value,
            description: description,
            defaultValue: defaultValue,
            tooltip: tooltip,
            showName: showName,
            ifEmpty: ifEmpty,
            style: style,
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
            string text = _description ?? valueT;
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
            DiagnosticsTreeStyle style = DiagnosticsTreeStyle.singleLine,
            DiagnosticLevel level = DiagnosticLevel.info
        ) : base(
            name,
            value,
            ifNull: ifNull,
            showName: showName,
            defaultValue: defaultValue,
            tooltip: tooltip,
            level: level,
            style: style
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
            DiagnosticsTreeStyle style = DiagnosticsTreeStyle.singleLine,
            DiagnosticLevel level = DiagnosticLevel.info
        ) : base(
            name,
            computeValue,
            ifNull: ifNull,
            showName: showName,
            defaultValue: defaultValue,
            tooltip: tooltip,
            style: style,
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

    public class FloatProperty : _NumProperty<float?> {
        public FloatProperty(string name, float? value,
            string ifNull = null,
            string unit = null,
            string tooltip = null,
            object defaultValue = null,
            bool showName = true,
            DiagnosticsTreeStyle style = DiagnosticsTreeStyle.singleLine,
            DiagnosticLevel level = DiagnosticLevel.info
        ) : base(
            name,
            value,
            ifNull: ifNull,
            unit: unit,
            tooltip: tooltip,
            defaultValue: defaultValue,
            showName: showName,
            level: level,
            style: style
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
                return valueT.Value.ToString("F1");
            }

            return "null";
        }
    }
    
    
    public class IntProperty : _NumProperty<int?> {
        public IntProperty(string name, int? value,
            string ifNull = null,
            bool showName = true,
            string unit = null,
            object defaultValue = null,
            DiagnosticsTreeStyle style = DiagnosticsTreeStyle.singleLine,
            DiagnosticLevel level = DiagnosticLevel.info
        ) : base(
            name,
            value,
            ifNull: ifNull,
            showName: showName,
            unit: unit,
            defaultValue: defaultValue,
            style: style,
            level: level
        ) {
        }

        protected override string numberToString() {
            if (value == null) {
                return "null";
            }

            return valueT.Value.ToString();
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

            return (valueT.Value.clamp(0.0f, 1.0f) * 100).ToString("F1") + "%";
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
            if (valueT == true) {
                if (ifTrue != null) {
                    return ifTrue;
                }
            }
            else if (valueT == false) {
                if (ifFalse != null) {
                    return ifFalse;
                }
            }

            return base.valueToString(parentConfiguration: parentConfiguration);
        }

        public override bool showName {
            get {
                if (value == null || valueT == true && ifTrue == null ||
                    valueT == false && ifFalse == null) {
                    return true;
                }

                return base.showName;
            }
        }

        public override DiagnosticLevel level {
            get {
                if (valueT == true) {
                    if (ifTrue == null) {
                        return DiagnosticLevel.hidden;
                    }
                }

                if (valueT == false) {
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
            bool showSeparator = true,
            DiagnosticLevel level = DiagnosticLevel.info
        ) : base(
            name,
            value,
            defaultValue: defaultValue,
            ifNull: ifNull,
            ifEmpty: ifEmpty,
            style: style,
            showName: showName,
            showSeparator: showSeparator,
            level: level
        ) {
        }

        protected override string valueToString(TextTreeConfiguration parentConfiguration = null) {
            if (value == null) {
                return "null";
            }

            if (!valueT.Any()) {
                return ifEmpty ?? "[]";
            }

            if (parentConfiguration != null && !parentConfiguration.lineBreakProperties) {
               return string.Join(", ", LinqUtils<string, T>.SelectList(valueT, (v => v.ToString())));
            }
            return string.Join(style == DiagnosticsTreeStyle.singleLine ? ", " : "\n", 
                LinqUtils<string, T>.SelectList(valueT,  (v => v.ToString())));
        }

        public override DiagnosticLevel level {
            get {
                if (ifEmpty == null &&
                    value != null && !valueT.Any()
                    && base.level != DiagnosticLevel.hidden) {
                    return DiagnosticLevel.fine;
                }

                return base.level;
            }
        }

        public override Dictionary<string, object> toJsonMap(DiagnosticsSerializationDelegate Delegate) {
            var json = base.toJsonMap(Delegate);
            if (value != null) {
                json["values"] = LinqUtils<string, T>.SelectList(valueT, (v => v.ToString()));
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
    
    class FlagsSummary<T> : DiagnosticsProperty<Dictionary<string, T>> {
        public FlagsSummary(
            string name = null,
            Dictionary<string, T> value = null,
            string ifEmpty = null,
            bool showName = true,
            bool showSeparator = true,
            DiagnosticLevel level = DiagnosticLevel.info
        ) : base(
            name,
            value,
            ifEmpty: ifEmpty,
            showName: showName,
            showSeparator: showSeparator,
            level: level
        ) {
            D.assert(value != null);
        }

        protected override string valueToString(TextTreeConfiguration parentConfiguration = null) {
            D.assert(value != null);
            if (!_hasNonNullEntry() && ifEmpty != null) {
                return ifEmpty;
            }

            IEnumerable<string> formattedValues = _formattedValues();
            if (parentConfiguration != null && !parentConfiguration.lineBreakProperties) {
                return string.Join(",", formattedValues);
            }

            return string.Join((DiagnosticUtils._isSingleLine((DiagnosticsTreeStyle) style) ? "," : "\n"),
                formattedValues);
        }


        public override DiagnosticLevel level {
            get {
                if (!_hasNonNullEntry() && ifEmpty == null) {
                    return DiagnosticLevel.hidden;
                }
                return base.level;
            }
        }

        public override Dictionary<string, object> toJsonMap(DiagnosticsSerializationDelegate Delegate) {
            Dictionary<string, object> json = base.toJsonMap(Delegate);
            if (valueT.isNotEmpty()) {
                json["values"] = _formattedValues().ToList();
            }

            return json;
        }

        bool _hasNonNullEntry() {
            return valueT.Values.ToList().Any((T o) => o != null);
        }

        IEnumerable<string> _formattedValues() {
            List<string> results = new List<string>();
            foreach (string entry in valueT.Keys) {
                if (valueT[entry] != null) {
                    results.Add(entry);
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
            string linePrefix = null,
            bool expandableValue = false,
            bool allowWrap = true,
            bool allowNameWrap = true,
            DiagnosticsTreeStyle style = DiagnosticsTreeStyle.singleLine,
            DiagnosticLevel level = DiagnosticLevel.info
        ) : base(
            name: name,
            showName: showName,
            showSeparator: showSeparator,
            style: style,
            linePrefix: linePrefix
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
            this.expandableValue = expandableValue;
            this.allowWrap = allowWrap;
            this.allowNameWrap = allowNameWrap;
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
            bool expandableValue = false,
            bool allowWrap = true,
            bool allowNameWrap = true,
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
            this.expandableValue = expandableValue;
            this.allowWrap = allowWrap;
            this.allowNameWrap = allowNameWrap;
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
            bool expandableValue = false,
            bool allowWrap = true,
            bool allowNameWrap = true,
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
                expandableValue,
                allowWrap,
                allowNameWrap,
                style,
                level);
        }

        internal readonly string _description;

        public readonly bool expandableValue;

        public override bool allowWrap { get; }

        public override bool allowNameWrap { get; }

        public override Dictionary<string, object> toJsonMap(DiagnosticsSerializationDelegate Delegate) {
            T v = valueT;
            List<Dictionary<string, object>> properties = new List<Dictionary<string, object>>();
            if (Delegate.expandPropertyValues && Delegate.includeProperties && v is Diagnosticable vDiagnosticable &&
                getProperties().isEmpty()) {
                // Exclude children for expanded nodes to avoid cycles.
                Delegate = Delegate.copyWith(subtreeDepth: 0, includeProperties: false);
                properties = toJsonList(
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

            if (value is Diagnosticable || value is DiagnosticsNode) {
                json["isDiagnosticableValue"] = true;
            }

            if (v is int) {
                json["value"] = Convert.ToInt32(v);
            }
            else if (v is float) {
                json["value"] = float.Parse(v.ToString());
            }

            if (value is string || value is bool || value == null) {
                json["value"] = value;
            }

            return json;
        }

        protected virtual string valueToString(
            TextTreeConfiguration parentConfiguration = null
        ) {
            var v = value;
            var tree = v as DiagnosticableTree;
            return tree != null ? tree.toStringShort() : v?.ToString() ?? "";
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

        public override object value {
            get {
                _maybeCacheValue();
                return _value;
            }
        }

        public T valueT {
            get {
                return (T) value;
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
            if (expandableValue) {
                T obj = valueT;
                if (obj is DiagnosticsNode) {
                    return (obj as DiagnosticsNode).getProperties();
                }

                if (obj is Diagnosticable) {
                    return (obj as Diagnosticable).toDiagnosticsNode(style: style.Value).getProperties();
                }
            }
            return new List<DiagnosticsNode>();
        }

        public override List<DiagnosticsNode> getChildren() {
            if (expandableValue) {
                T obj = valueT;
                if (obj is DiagnosticsNode) {
                    return (obj as DiagnosticsNode).getChildren();
                }

                if (obj is Diagnosticable) {
                    return (obj as Diagnosticable).toDiagnosticsNode(style: style.Value).getChildren();
                }
            }
            return new List<DiagnosticsNode>();
        }
    }

    public class DiagnosticableNode<T> : DiagnosticsNode where T : IDiagnosticable {
        public DiagnosticableNode(
            string name = null,
            T value = default,
            DiagnosticsTreeStyle? style = null
        ) : base(name: name, style: style) {
            D.assert(value != null);
            _value = value;
        }

        public override object valueObject {
            get { return value; }
        }

        public override object value {
            get { return _value; }
        }

        public T valueT {
            get { return (T) value; }
        }

        readonly T _value;
        DiagnosticPropertiesBuilder _cachedBuilder;

        protected DiagnosticPropertiesBuilder builder {
            get {
                if (foundation_.kReleaseMode) {
                    return null;
                }
                else {
                    D.assert(() => {
                        if (_cachedBuilder == null) {
                            _cachedBuilder = new DiagnosticPropertiesBuilder();
                            _value?.debugFillProperties(_cachedBuilder);
                        }

                        return true;
                    });
                }
                
                return _cachedBuilder;
            }
        }

        public override DiagnosticsTreeStyle? style {
            get { return foundation_.kReleaseMode ? DiagnosticsTreeStyle.none : base.style ?? builder.defaultDiagnosticsTreeStyle; }
        }

        public override string emptyBodyDescription {
            get { return foundation_.kReleaseMode ? "" : builder.emptyBodyDescription; }
        }

        public override List<DiagnosticsNode> getProperties() {
            return foundation_.kReleaseMode ? new List<DiagnosticsNode>() : builder.properties;
        }

        public override List<DiagnosticsNode> getChildren() {
            return new List<DiagnosticsNode>();
        }

        public override string toDescription(
            TextTreeConfiguration parentConfiguration = null
        ) {
            string result = "";
            D.assert(() => {
                result = _value.toStringShort();
                return true;
            });
            return result;
        }
    }

    class DiagnosticableTreeNode : DiagnosticableNode<DiagnosticableTreeMixin> {
        internal DiagnosticableTreeNode(
            string name,
            DiagnosticableTreeMixin value,
            DiagnosticsTreeStyle style
        ) : base(
            name: name,
            value: value,
            style: style
        ) {
        }
        

        public override List<DiagnosticsNode> getChildren() {
            if (value != null) {
                return valueT.debugDescribeChildren();
            }

            return new List<DiagnosticsNode>();
        }
    }
    
    public class DiagnosticPropertiesBuilder {
        public DiagnosticPropertiesBuilder(List<DiagnosticsNode> properties) {
            this.properties = properties;
        }

        public DiagnosticPropertiesBuilder() {
            properties = new List<DiagnosticsNode>();
        }

        public void add(DiagnosticsNode property) {
            D.assert(() => {
                properties.Add(property);
                return true;
            });
        }

        public readonly List<DiagnosticsNode> properties;
        
        public DiagnosticsTreeStyle defaultDiagnosticsTreeStyle = DiagnosticsTreeStyle.sparse;
        
        public string emptyBodyDescription;
    }

    public class DiagnosticableMixin : Diagnosticable {
        public override string toStringShort() {
            return foundation_.describeIdentity(this);
        }

        public override string toString(DiagnosticLevel minLevel = DiagnosticLevel.debug) {
            string fullString = null;
            D.assert(() => {
                fullString = toDiagnosticsNode(style: DiagnosticsTreeStyle.singleLine).toString(minLevel: minLevel);
                return true;
            });
            return fullString ?? toStringShort();
        }

        public override DiagnosticsNode toDiagnosticsNode(string name = null, DiagnosticsTreeStyle style = DiagnosticsTreeStyle.sparse) {
            return new DiagnosticableNode<Diagnosticable>(
                      name: name,
                      value: this,
                      style: style
                );
        }

        public override void debugFillProperties(DiagnosticPropertiesBuilder properties) {
        }
    }

    public interface IDiagnosticable {
        string toStringShort();

        string toString(DiagnosticLevel minLevel = DiagnosticLevel.info);
        
        DiagnosticsNode toDiagnosticsNode(
            string name = null,
            DiagnosticsTreeStyle style = DiagnosticsTreeStyle.sparse);

        void debugFillProperties(DiagnosticPropertiesBuilder properties);
    }

    public abstract class Diagnosticable : IDiagnosticable {

        protected Diagnosticable() {
        }

        public virtual string toStringShort() {
            return foundation_.describeIdentity(this);
        }

        public override string ToString() {
            return toString();
        }

        public virtual string toString(DiagnosticLevel minLevel = DiagnosticLevel.info) {
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


    public interface DiagnosticableTreeMixin : IDiagnosticable {
        string toStringShallow(
            string joiner = ", ",
            DiagnosticLevel minLevel = DiagnosticLevel.debug
        );

        string toStringDeep(
            string prefixLineOne = "",
            string prefixOtherLines = null,
            DiagnosticLevel minLevel = DiagnosticLevel.debug);
        
        List<DiagnosticsNode> debugDescribeChildren();
    }

    public abstract class DiagnosticableTree : Diagnosticable,DiagnosticableTreeMixin {
        protected DiagnosticableTree() {
        }

        public virtual string toStringShallow(
            string joiner = ", ",
            DiagnosticLevel minLevel = DiagnosticLevel.debug
        ) {
            if (foundation_.kReleaseMode) {
                return toString();
            }

            string shallowString = "";
            D.assert(() => {
                var result = new StringBuilder();
                result.Append(toString());
                result.Append(joiner);
                DiagnosticPropertiesBuilder builder = new DiagnosticPropertiesBuilder();
                debugFillProperties(builder);
                result.Append(string.Join(joiner,LinqUtils<string,DiagnosticsNode>.SelectList(
                    LinqUtils<DiagnosticsNode>.WhereList(builder.properties, (n => !n.isFiltered(minLevel)))
                    ,(n => n.ToString())))
                );
                shallowString = result.ToString();
                return true;
            });

            return shallowString;
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

            DiagnosticsTreeStyle style = DiagnosticsTreeStyle.sparse) {
            return new DiagnosticableTreeNode(
                name: name,
                value: this,
                style: style
            );
        }

        public virtual List<DiagnosticsNode> debugDescribeChildren() {
            return new List<DiagnosticsNode>();
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


        public override DiagnosticLevel level { get; }
        readonly string _description;

        public override object valueObject { get; }

        public override bool allowTruncate { get; }

        public override List<DiagnosticsNode> getChildren() {
            return _children;
        }

        public override List<DiagnosticsNode> getProperties() {
            return _properties;
        }

        public override string toDescription(TextTreeConfiguration parentConfiguration = null) {
            return _description;
        }
    }

    public abstract class DiagnosticsSerializationDelegate {
        public DiagnosticsSerializationDelegate(
            int subtreeDepth = 0,
            bool includeProperties = false
        ) {
            new _DefaultDiagnosticsSerializationDelegate(includeProperties, subtreeDepth);
        }

        public abstract Dictionary<string, object> additionalNodeProperties(DiagnosticsNode node);
        public abstract List<DiagnosticsNode> filterChildren(List<DiagnosticsNode> nodes, DiagnosticsNode owner);

        public abstract List<DiagnosticsNode> filterProperties(List<DiagnosticsNode> nodes, DiagnosticsNode owner);

        public abstract List<DiagnosticsNode> truncateNodesList(List<DiagnosticsNode> nodes, DiagnosticsNode owner);

        public abstract DiagnosticsSerializationDelegate delegateForNode(DiagnosticsNode node);

        public virtual int subtreeDepth { get; }

        public virtual bool includeProperties { get; }

        public virtual bool expandPropertyValues { get; }

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

        public override DiagnosticsSerializationDelegate delegateForNode(DiagnosticsNode node) {
            return subtreeDepth > 0 ? copyWith(subtreeDepth: subtreeDepth - 1) : this;
        }

        public override bool expandPropertyValues {
            get { return false; }
        }

        public override List<DiagnosticsNode> filterChildren(List<DiagnosticsNode> nodes, DiagnosticsNode owner) {
            return nodes;
        }

        public override List<DiagnosticsNode> filterProperties(List<DiagnosticsNode> nodes, DiagnosticsNode owner) {
            return nodes;
        }

        public override bool includeProperties { get; }

        public override int subtreeDepth { get; }

        public override List<DiagnosticsNode> truncateNodesList(List<DiagnosticsNode> nodes, DiagnosticsNode owner) {
            return nodes;
        }

        public override DiagnosticsSerializationDelegate copyWith(int? subtreeDepth = null,
            bool? includeProperties = null) {
            return new _DefaultDiagnosticsSerializationDelegate(
                subtreeDepth: subtreeDepth ?? this.subtreeDepth,
                includeProperties: includeProperties ?? this.includeProperties
            );
        }
    }
}