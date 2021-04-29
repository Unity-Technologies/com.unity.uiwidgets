using System.Collections.Generic;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.material;
using Unity.UIWidgets.widgets;

namespace Unity.UIWidgets.material {
    public partial class material_ {
        public static bool debugCheckHasMaterial(BuildContext context) {
            D.assert(() => {
                if (!(context.widget is Material) && context.findAncestorWidgetOfExactType<Material>() == null) {
                    var list = new List<DiagnosticsNode>() {
                        new ErrorSummary("No Material widget found."),
                        new ErrorDescription(
                            $"{context.widget.GetType()} widgets require a Material " +
                            "widget ancestor.\n" +
                            "In material design, most widgets are conceptually \"printed\" on " +
                            "a sheet of material. In Flutter\"s material library, that " +
                            "material is represented by the Material widget. It is the " +
                            "Material widget that renders ink splashes, for instance. " +
                            "Because of this, many material library widgets require that " +
                            "there be a Material widget in the tree above them."
                        ),
                        new ErrorHint(
                            "To introduce a Material widget, you can either directly " +
                            "include one, or use a widget that contains Material itself, " +
                            "such as a Card, Dialog, Drawer, or Scaffold."
                        )
                        // ...context.describeMissingAncestor(expectedAncestorType: Material)
                    };
                    list.AddRange(context.describeMissingAncestor(expectedAncestorType: typeof(Material)));
                    throw new UIWidgetsError(list);
                }

                return true;
            });
            return true;
        }


        public static bool debugCheckHasMaterialLocalizations(BuildContext context) {
            D.assert(() => {
                if (Localizations.of<MaterialLocalizations>(context, typeof(MaterialLocalizations)) == null) {
                    var list = new List<DiagnosticsNode>() {
                        new ErrorSummary("No MaterialLocalizations found."),
                        new ErrorDescription(
                            "${context.widget.runtimeType} widgets require MaterialLocalizations " +
                            "to be provided by a Localizations widget ancestor."
                        ),
                        new ErrorDescription(
                            "Localizations are used to generate many different messages, labels, " +
                            "and abbreviations which are used by the material library."
                        ),
                        new ErrorHint(
                            "To introduce a MaterialLocalizations, either use a " +
                            "MaterialApp at the root of your application to include them " +
                            "automatically, or add a Localization widget with a " +
                            "MaterialLocalizations delegate."
                        ),
                    };
                    list.AddRange(context.describeMissingAncestor(expectedAncestorType: typeof(MaterialLocalizations)));
                    throw new UIWidgetsError(list);
                }

                return true;
            });
            return true;
        }

        public static bool debugCheckHasScaffold(BuildContext context) {
            D.assert(() => {
                if (!(context.widget is Scaffold) && context.findAncestorWidgetOfExactType<Scaffold>() == null) {
                    var list = new List<DiagnosticsNode>() {
                        new ErrorSummary("No Scaffold widget found."),
                        new ErrorDescription(
                            "${context.widget.runtimeType} widgets require a Scaffold widget ancestor."),

                        new ErrorHint(
                            "Typically, the Scaffold widget is introduced by the MaterialApp or " +
                            "WidgetsApp widget at the top of your application widget tree."
                        )
                    };
                    list.AddRange(context.describeMissingAncestor(expectedAncestorType: typeof(Scaffold)));
                    throw new UIWidgetsError(list);
                }

                return true;
            });
            return true;
        }
    }
}