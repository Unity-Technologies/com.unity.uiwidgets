using System;
using System.Collections.Generic;
using Unity.UIWidgets.foundation;

namespace Unity.UIWidgets.widgets {
    public static class WidgetsD {
        public static bool debugPrintRebuildDirtyWidgets = false;

        public static bool debugPrintBuildScope = false;

        public static bool debugPrintGlobalKeyedWidgetLifecycle = false;

        public static bool debugPrintScheduleBuildForStacks = false;
        
        public static bool debugProfileBuildsEnabled = false;

        public static bool debugHighlightDeprecatedWidgets = false;

        static Key _firstNonUniqueKey(IEnumerable<Widget> widgets) {
            var keySet = new HashSet<Key>();
            foreach (Widget widget in widgets) {
                D.assert(widget != null);
                if (widget.key == null) {
                    continue;
                }

                if (!keySet.Add(widget.key)) {
                    return widget.key;
                }
            }

            return null;
        }

        public static bool debugChildrenHaveDuplicateKeys(Widget parent, IEnumerable<Widget> children) {
            D.assert(() => {
                Key nonUniqueKey = _firstNonUniqueKey(children);
                if (nonUniqueKey != null) {
                    throw new UIWidgetsError(
                        "Duplicate keys found.\n" +
                        "If multiple keyed nodes exist as children of another node, they must have unique keys.\n" +
                        parent + " has multiple children with key " + nonUniqueKey + "."
                    );
                }

                return true;
            });
            return false;
        }

        public static bool debugItemsHaveDuplicateKeys(IEnumerable<Widget> items) {
            D.assert(() => {
                Key nonUniqueKey = _firstNonUniqueKey(items);
                if (nonUniqueKey != null) {
                    throw new UIWidgetsError($"Duplicate key found: {nonUniqueKey}.");
                }

                return true;
            });
            return false;
        }

        public static bool debugCheckHasTable(BuildContext context) {
            D.assert(() => {
                if (!(context.widget is Table) && context.findAncestorWidgetOfExactType<Table>() == null) {
                    throw new UIWidgetsError(new List<DiagnosticsNode> {
                        new ErrorSummary("No Table widget found."),
                        new ErrorDescription($"{context.widget.GetType()} widgets require a Table widget ancestor."),
                        context.describeWidget("The specific widget that could not find a Table ancestor was"),
                        context.describeOwnershipChain("The ownership chain for the affected widget is")
                    });
                }

                return true;
            });
            return true;
        }

        public static void debugWidgetBuilderValue(Widget widget, Widget built) {
            D.assert(() => {
                if (built == null) {
                    throw new UIWidgetsError(
                        "A build function returned null.\n" +
                        "The offending widget is: " + widget + "\n" +
                        "Build functions must never return null. " +
                        "To return an empty space that causes the building widget to fill available room, return \"new Container()\". " +
                        "To return an empty space that takes as little room as possible, return \"new Container(width: 0.0, height: 0.0)\".");
                }

                if (widget == built) {
                    throw new UIWidgetsError(
                        "A build function returned context.widget.\n" +
                        $"The offending widget is: {widget}\n" +
                        "Build functions must never return their BuildContext parameter\'s widget or a child that contains 'context.widget'. " +
                        "Doing so introduces a loop in the widget tree that can cause the app to crash."
                    );
                }

                return true;
            });
        }

        public static bool debugCheckHasMediaQuery(BuildContext context) {
            D.assert(() => {
                if (!(context.widget is MediaQuery) && context.findAncestorWidgetOfExactType<MediaQuery>() == null) {
                    throw new UIWidgetsError(new List<DiagnosticsNode> {
                        new ErrorSummary("No MediaQuery widget found."),
                        new ErrorDescription(
                            $"{context.widget.GetType()} widgets require a MediaQuery widget ancestor."),
                        context.describeWidget("The specific widget that could not find a MediaQuery ancestor was"),
                        context.describeOwnershipChain("The ownership chain for the affected widget is"),
                        new ErrorHint(
                            "Typically, the MediaQuery widget is introduced by the MaterialApp or " +
                            "WidgetsApp widget at the top of your application widget tree."
                        )
                    });
                }

                return true;
            });
            return true;
        }

        public static bool debugCheckHasDirectionality(BuildContext context) {
            D.assert(() => {
                if (!(context.widget is Directionality) &&
                    context.findAncestorWidgetOfExactType<Directionality>() == null) {
                    throw new UIWidgetsError(new List<DiagnosticsNode> {
                        new ErrorSummary("No Directionality widget found."),
                        new ErrorDescription(
                            $"{context.widget.GetType()} widgets require a Directionality widget ancestor.\n"),
                        context.describeWidget("The specific widget that could not find a Directionality ancestor was"),
                        context.describeOwnershipChain("The ownership chain for the affected widget is"),
                        new ErrorHint(
                            "Typically, the Directionality widget is introduced by the MaterialApp " +
                            "or WidgetsApp widget at the top of your application widget tree. It " +
                            "determines the ambient reading direction and is used, for example, to " +
                            "determine how to lay out text, how to interpret \"start\" and \"end\" " +
                            "values, and to resolve EdgeInsetsDirectional, " +
                            "AlignmentDirectional, and other *Directional objects."
                        )
                    });
                }

                return true;
            });
            return true;
        }

        internal static UIWidgetsErrorDetails _debugReportException(
            DiagnosticsNode context,
            Exception exception,
            InformationCollector informationCollector = null
        ) {
            var details = new UIWidgetsErrorDetails(
                exception: exception,
                library: "widgets library",
                context: context,
                informationCollector: informationCollector
            );
            UIWidgetsError.reportError(details);
            return details;
        }

        internal static UIWidgetsErrorDetails _debugReportException(
            string context,
            Exception exception,
            InformationCollector informationCollector = null
        ) {
            var details = new UIWidgetsErrorDetails(
                exception: exception,
                library: "widgets library",
                context: new ErrorDescription(context),
                informationCollector: informationCollector
            );
            UIWidgetsError.reportError(details);
            return details;
        }

        /// See [the widgets library](widgets/widgets-library.html) for a complete list.
        public static bool debugAssertAllWidgetVarsUnset(string reason) {
            D.assert(() => {
                if (debugPrintRebuildDirtyWidgets ||
                    debugPrintBuildScope ||
                    debugPrintScheduleBuildForStacks ||
                    debugPrintGlobalKeyedWidgetLifecycle ||
                    debugProfileBuildsEnabled ||
                    debugHighlightDeprecatedWidgets) {
                    throw new UIWidgetsError(reason);
                }
                return true;
            });
            return true;
        }
    }
}