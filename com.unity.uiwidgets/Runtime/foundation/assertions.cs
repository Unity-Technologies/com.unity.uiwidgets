using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Unity.UIWidgets.external;
using UnityEngine;

namespace Unity.UIWidgets.foundation {
    public delegate void UIWidgetsExceptionHandler(UIWidgetsErrorDetails details);

    public delegate IEnumerable<DiagnosticsNode> DiagnosticPropertiesTransformer(IEnumerable<DiagnosticsNode> properties);

    public delegate IEnumerable<DiagnosticsNode> InformationCollector();

    public abstract class _ErrorDiagnostic : DiagnosticsProperty<List<object>> {
        internal _ErrorDiagnostic(
            string message,
            DiagnosticsTreeStyle style = DiagnosticsTreeStyle.flat,
            DiagnosticLevel level = DiagnosticLevel.info
        ) : base(
            name: null,
            value: new List<object>() {message},
            showName: false,
            showSeparator: false,
            defaultValue: null,
            style: style,
            level: level) {
            D.assert(message != null);
        }
        
        internal _ErrorDiagnostic(
            List<object> messageParts,
            DiagnosticsTreeStyle style = DiagnosticsTreeStyle.flat,
            DiagnosticLevel level = DiagnosticLevel.info
        ) : base(
            name: null,
            value: messageParts,
            showName: false,
            showSeparator: false,
            defaultValue: null,
            style: style,
            level: level) {
            D.assert(messageParts != null);
        }

        protected override string valueToString(TextTreeConfiguration parentConfiguration = null) {
            return string.Join("", valueT);
        }
    }

    public class ErrorDescription : _ErrorDiagnostic {
        public ErrorDescription(string message) : base(message, level: DiagnosticLevel.info) {
        }

        public ErrorDescription(List<object> messageParts) : base(messageParts, level: DiagnosticLevel.info) {
        }
    }

    public class ErrorSummary : _ErrorDiagnostic {
        public ErrorSummary(string message) : base(message, level: DiagnosticLevel.summary) {
        }

        public ErrorSummary(List<object> messageParts) : base(messageParts, level: DiagnosticLevel.summary) {
        }
    }
    
    internal class ErrorHint : _ErrorDiagnostic {
        public ErrorHint(string message) : base(message, level: DiagnosticLevel.hint) {
        }

        public ErrorHint(List<object> messageParts) : base(messageParts, level: DiagnosticLevel.hint) {
        }
    }
    
    public class ErrorSpacer : DiagnosticsProperty<object> {
        public ErrorSpacer() : base(
            "",
            null,
            description: "",
            showName: false
        ) {
        }
    }

    public class UIWidgetsErrorDetails : Diagnosticable {
        public UIWidgetsErrorDetails(
            Exception exception = null,
            string library = "UIWidgets framework",
            DiagnosticsNode context = null,
            IterableFilter<string> stackFilter = null,
            InformationCollector informationCollector = null,
            bool silent = false
        ) {
            this.exception = exception;
            this.library = library;
            this.context = context;
            this.stackFilter = stackFilter;
            this.informationCollector = informationCollector;
            this.silent = silent;
        }

        public static readonly List<DiagnosticPropertiesTransformer> propertiesTransformers =
            new List<DiagnosticPropertiesTransformer>();

        public readonly Exception exception;

        public readonly string library;

        public readonly DiagnosticsNode context;

        public readonly IterableFilter<string> stackFilter;

        public readonly InformationCollector informationCollector;

        public readonly bool silent;

        public string exceptionAsString() {
            string longMessage = null;

            if (exception != null) {
                longMessage = exception.Message;
            }

            if (longMessage != null) {
                longMessage = longMessage.TrimEnd();
            }

            if (longMessage.isEmpty()) {
                longMessage = "<no message available>";
            }

            return longMessage;
        }

        Diagnosticable _exceptionToDiagnosticable() {
            return null;
        }

        public DiagnosticsNode summary {
            get {
                string formatException() {
                    return exceptionAsString().Split('\n')[0].TrimStart();
                }

                if (foundation_.kReleaseMode) {
                    return DiagnosticsNode.message(formatException());
                }

                Diagnosticable diagnosticable = _exceptionToDiagnosticable();
                DiagnosticsNode summary = null;
                if (diagnosticable != null) {
                    DiagnosticPropertiesBuilder builder = new DiagnosticPropertiesBuilder();
                    debugFillProperties(builder);
                    summary = builder.properties.First((DiagnosticsNode node) => node.level == DiagnosticLevel.summary);
                }

                return summary ?? new ErrorSummary(formatException());
            }
        }

        public override void debugFillProperties(DiagnosticPropertiesBuilder properties) {
            base.debugFillProperties(properties);
            DiagnosticsNode verb = context != null
                ? new ErrorDescription($"thrown {new ErrorDescription($" {context}")}")
                : new ErrorDescription("");

            Diagnosticable diagnosticable = _exceptionToDiagnosticable();
            if (exception is NullReferenceException) {
                properties.add(new ErrorDescription($"The null value was {verb}."));
            }
            else {
                DiagnosticsNode errorName;
                errorName = new ErrorDescription($"{exception.GetType()}");
                properties.add(new ErrorDescription($"The following {errorName} was {verb}:"));
                if (diagnosticable != null) {
                    diagnosticable.debugFillProperties(properties);
                }
                else {
                    string prefix = $"{exception.GetType()}";
                    string message = exceptionAsString();
                    if (message.StartsWith(prefix)) {
                        message = message.Substring(prefix.Length);
                    }

                    properties.add(new ErrorSummary(message));
                }
            }

            if (informationCollector != null) {
                properties.add(new ErrorSpacer());
                foreach (var diagnosticsNode in informationCollector()) {
                    properties.add(diagnosticsNode);
                }
            }
        }

        public override string toStringShort() {
            return library != null ? $"Exception caught by {library}" : "Exception caught";
        }

        public override string toString(DiagnosticLevel minLevel = DiagnosticLevel.info) {
            return toDiagnosticsNode(style: DiagnosticsTreeStyle.error).toStringDeep(minLevel: minLevel);
        }

        public override DiagnosticsNode toDiagnosticsNode(string name = null,
            DiagnosticsTreeStyle style = DiagnosticsTreeStyle.sparse) {
            return new _UIWIdgetsErrorDetailsNode(
                name: name,
                value: this,
                style: style
            );
        }
    }


    internal class _UIWIdgetsErrorDetailsNode : DiagnosticableNode<UIWidgetsErrorDetails> {
        public _UIWIdgetsErrorDetailsNode(
            string name,
            UIWidgetsErrorDetails value,
            DiagnosticsTreeStyle style
        ) : base(
            name: name,
            value: value,
            style: style
        ) {
            D.assert(value != null);
        }

        new DiagnosticPropertiesBuilder builder {
            get {
                DiagnosticPropertiesBuilder builder = base.builder;
                if (builder == null) {
                    return null;
                }

                IEnumerable<DiagnosticsNode> properties = builder.properties;
                foreach (DiagnosticPropertiesTransformer transformer in UIWidgetsErrorDetails.propertiesTransformers) {
                    properties = transformer(properties);
                }

                return new DiagnosticPropertiesBuilder(properties.ToList());
            }
        }
    }
    class DiagnosticsStackTrace : DiagnosticsBlock {
        public DiagnosticsStackTrace(
            string name,
            IterableFilter<string> stackFilter = null,
            bool showSeparator = true
            ) : base(
            name: name,
            properties: new List<DiagnosticsNode>(),
            style: DiagnosticsTreeStyle.flat,
            showSeparator: showSeparator,
            allowTruncate: true
        ) {
        }

        public DiagnosticsStackTrace(
            string name,
            string frame,
            bool showSeparator = true
        ) : base(
            name: name,
            properties: new List<DiagnosticsNode>() {_createStackFrame(frame)},
            style: DiagnosticsTreeStyle.whitespace,
            showSeparator: showSeparator
        ) {
            
        }
        public static DiagnosticsNode _createStackFrame(string frame) {
            return DiagnosticsNode.message(frame, allowWrap: false);
        }
    }


    public class UIWidgetsError : Exception {
        public UIWidgetsError(string message) {
            string[] lines = message.Split('\n');
            diagnostics = new List<DiagnosticsNode>() {
                new ErrorSummary(lines[0])
            };

            for (var i = 1; i < lines.Length; i++) {
                diagnostics.Add(new ErrorDescription(lines[i]));
            }
        }

        public UIWidgetsError(List<DiagnosticsNode> diagnostics) {
            this.diagnostics = diagnostics;
            D.assert(diagnostics != null && diagnostics.isNotEmpty(), () => new UIWidgetsError(new List<DiagnosticsNode>() {new ErrorSummary("Empty FlutterError")}).ToString());
            D.assert(diagnostics.first().level == DiagnosticLevel.summary,
                () => new UIWidgetsError(new List<DiagnosticsNode>() {
                    new ErrorSummary("UIWidgetsError is missing a summary."),
                    new ErrorDescription("All UIWidgetsError objects should start with a short (one line) " + 
                                         "summary description of the problem that was detected."),
                    new DiagnosticsProperty<UIWidgetsError>("Malformed", this, expandableValue: true, showSeparator: false, style : DiagnosticsTreeStyle.whitespace),
                    new ErrorDescription(
                        "\nThis error should still help you solve your problem, " +
                        "however please also report this malformed error in the " +
                        "framework by filing a bug on GitHub:\n" +
                        "  https://https://github.com/Unity-Technologies/com.unity.uiwidgets"
                    )
                }).ToString());
            
            D.assert(() => {
                IEnumerable<DiagnosticsNode> summaries =
                    LinqUtils<DiagnosticsNode>.WhereList(diagnostics,((DiagnosticsNode node) => node.level == DiagnosticLevel.summary));
                if (summaries.Count() > 1) {
                    return false;
                }
                return true;
            }, () => {
                IEnumerable<DiagnosticsNode> summaries =
                    LinqUtils<DiagnosticsNode>.WhereList(diagnostics,((DiagnosticsNode node) => node.level == DiagnosticLevel.summary));
                List<DiagnosticsNode> message = new List<DiagnosticsNode>() {
                    new ErrorSummary("UIWidgetsError contained multiple error summaries."),
                    new ErrorDescription(
                        "All UIWidgetsError objects should have only a single short " + 
                        "(one line) summary description of the problem that was " +
                        "detected."
                        ),
                    new DiagnosticsProperty<UIWidgetsError>("Malformed", this, expandableValue: true, showSeparator: false, style : DiagnosticsTreeStyle.whitespace) 
                };

                int i = 0;
                foreach (DiagnosticsNode summary in summaries) {
                    message.Add(new DiagnosticsProperty<DiagnosticsNode>($"Summary {i}", summary, expandableValue : true));
                    i += 1;
                }
                message.Add(new ErrorDescription(
                    "\nThis error should still help you solve your problem, " +
                    "however please also report this malformed error in the " +
                    "framework by filing a bug on GitHub:\n" +
                    "  https://https://github.com/Unity-Technologies/com.unity.uiwidgets"
                ));
                
                return new UIWidgetsError(message).ToString();
            });
        }
        
        public readonly List<DiagnosticsNode> diagnostics;

        public override string Message => ToString();

        public static UIWidgetsExceptionHandler onError = dumpErrorToConsole;

        static int _errorCount = 0;

        public static void resetErrorCount() {
            _errorCount = 0;
        }

        const int wrapWidth = 100;

        public static void dumpErrorToConsole(UIWidgetsErrorDetails details) {
            dumpErrorToConsole(details, forceReport: false);
        }

        public static void dumpErrorToConsole(UIWidgetsErrorDetails details, bool forceReport = false) {
            D.assert(details != null);
            D.assert(details.exception != null);
            bool reportError = !details.silent;
            D.assert(() => {
                reportError = true;
                return true;
            });
            if (!reportError && !forceReport) {
                return;
            }

            if (_errorCount == 0 || forceReport) {
                D.logError(new TextTreeRenderer(
                    wrapWidth: wrapWidth,
                    wrapWidthProperties: wrapWidth,
                    maxDescendentsTruncatableNode: 5).render(
                    details.toDiagnosticsNode(style: DiagnosticsTreeStyle.error)).TrimEnd(), details.exception);   
            }
            
            D.logError($"Another exception was thrown: {details.summary}", details.exception);

            _errorCount += 1;
        }

        public static IEnumerable<string> defaultStackFilter(IEnumerable<string> frames) {
            return frames;
        }

        public void debugFillProperties(DiagnosticPropertiesBuilder properties) {
            foreach (var diagnosticsNode in diagnostics)
            {
                properties.add(diagnosticsNode);
            }
        }

        public string toStringShort() => "UIWidgetsError";

        public override string ToString() {
            TextTreeRenderer renderer = new TextTreeRenderer(wrapWidth: 400000000);
            return string.Join("\n", LinqUtils<string,DiagnosticsNode>.SelectList(diagnostics,((DiagnosticsNode node) => renderer.render(node).TrimEnd())));
        }

        public static void reportError(UIWidgetsErrorDetails details) {
            D.assert(details != null);
            D.assert(details.exception != null);
            if (onError != null) {
                onError(details);
            }
        }
    }
}