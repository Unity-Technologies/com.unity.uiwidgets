using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Unity.UIWidgets.foundation {
    public delegate void UIWidgetsExceptionHandler(UIWidgetsErrorDetails details);

    public delegate void InformationCollector(StringBuilder information);
    
    public class ErrorSpacer : DiagnosticsProperty<object> {
        public ErrorSpacer() : base(
            "",
            null,
            description: "",
            showName: false
        ) {
        }
    }

    public class UIWidgetsErrorDetails {
        public UIWidgetsErrorDetails(
            Exception exception = null,
            string library = "UIWidgets framework",
            string context = null,
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

        public readonly Exception exception;
        public readonly string library;
        public readonly string context;
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

        public override string ToString() {
            var buffer = new StringBuilder();
            if (library.isNotEmpty() || context.isNotEmpty()) {
                if (library.isNotEmpty()) {
                    buffer.AppendFormat("Error caught by {0}", library);
                    if (context.isNotEmpty()) {
                        buffer.Append(", ");
                    }
                }
                else {
                    buffer.Append("Exception ");
                }

                if (context.isNotEmpty()) {
                    buffer.AppendFormat("thrown {0}", context);
                }

                buffer.Append(". ");
            }
            else {
                buffer.Append("An error was caught. ");
            }

            buffer.AppendLine(exceptionAsString());
            if (informationCollector != null) {
                informationCollector(buffer);
            }

            if (exception.StackTrace != null) {
                IEnumerable<string> stackLines = exception.StackTrace.TrimEnd().Split('\n');
                if (stackFilter != null) {
                    stackLines = stackFilter(stackLines);
                }
                else {
                    stackLines = UIWidgetsError.defaultStackFilter(stackLines);
                }

                buffer.Append(string.Join("\n", stackLines.ToArray()));
            }

            return buffer.ToString().TrimEnd();
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
        public UIWidgetsError(string message) : base(message) {
        }

        public static UIWidgetsExceptionHandler onError = dumpErrorToConsole;

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

            D.logError(details.ToString(), details.exception);
        }

        public static IEnumerable<string> defaultStackFilter(IEnumerable<string> frames) {
            return frames;
        }

        public static void reportError(UIWidgetsErrorDetails details) {
            D.assert(details != null);
            D.assert(details.exception != null);
            if (onError != null) {
                onError(details);
            }
        }
    }

    internal abstract class _ErrorDiagnostic : DiagnosticsProperty<List<object>> {
        /// This constructor provides a reliable hook for a kernel transformer to find
        /// error messages that need to be rewritten to include object references for
        /// interactive display of errors.
        internal _ErrorDiagnostic(
            String message,
            DiagnosticsTreeStyle style = DiagnosticsTreeStyle.sparse, //  DiagnosticsTreeStyle.flat
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

        /// In debug builds, a kernel transformer rewrites calls to the default
        /// constructors for [ErrorSummary], [ErrorDetails], and [ErrorHint] to use
        /// this constructor.
        //
        // ```dart
        // _ErrorDiagnostic('Element $element must be $color')
        // ```
        // Desugars to:
        // ```dart
        // _ErrorDiagnostic.fromParts(<Object>['Element ', element, ' must be ', color])
        // ```
        //
        // Slightly more complex case:
        // ```dart
        // _ErrorDiagnostic('Element ${element.runtimeType} must be $color')
        // ```
        // Desugars to:
        //```dart
        // _ErrorDiagnostic.fromParts(<Object>[
        //   'Element ',
        //   DiagnosticsProperty(null, element, description: element.runtimeType?.toString()),
        //   ' must be ',
        //   color,
        // ])
        // ```
        internal _ErrorDiagnostic(
            List<object> messageParts,
            DiagnosticsTreeStyle style = DiagnosticsTreeStyle.sparse,
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
            return String.Join("", value);
        }
    }

    internal class ErrorDescription : _ErrorDiagnostic {
        /// A lint enforces that this constructor can only be called with a string
        /// literal to match the limitations of the Dart Kernel transformer that
        /// optionally extracts out objects referenced using string interpolation in
        /// the message passed in.
        ///
        /// The message will display with the same text regardless of whether the
        /// kernel transformer is used. The kernel transformer is required so that
        /// debugging tools can provide interactive displays of objects described by
        /// the error.
        public ErrorDescription(string message) : base(message, level: DiagnosticLevel.info) {
        }

        /// Calls to the default constructor may be rewritten to use this constructor
        /// in debug mode using a kernel transformer.
        // ignore: unused_element
        public ErrorDescription(List<object> messageParts) : base(messageParts, level: DiagnosticLevel.info) {
        }
    }
}