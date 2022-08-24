using System.Collections.Generic;
using developer;
using uiwidgets;
using Unity.UIWidgets.async;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.material;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.rendering;
using Unity.UIWidgets.scheduler;
using Unity.UIWidgets.ui;
using Unity.UIWidgets.widgets;
using TextStyle = Unity.UIWidgets.painting.TextStyle;

namespace Unity.UIWidgets.material {
    public class AboutListTile : StatelessWidget {
        public AboutListTile(
            Key key,
            Widget icon = null,
            Widget child = null,
            string applicationName = null,
            string applicationVersion = null,
            Widget applicationIcon = null,
            string applicationLegalese = null,
            List<Widget> aboutBoxChildren = null,
            bool dense = false
        ) : base(key: key) {
            this.icon = icon;
            this.child = child;
            this.applicationName = applicationName;
            this.applicationVersion = applicationVersion;
            this.applicationIcon = applicationIcon;
            this.applicationLegalese = applicationLegalese;
            this.aboutBoxChildren = aboutBoxChildren;
            this.dense = dense;
        }

        public readonly Widget icon;

        public readonly Widget child;

        public readonly string applicationName;

        public readonly string applicationVersion;

        public readonly Widget applicationIcon;

        public readonly string applicationLegalese;

        public readonly List<Widget> aboutBoxChildren;

        public readonly bool dense;

        public override Widget build(BuildContext context) {
            D.assert(material_.debugCheckHasMaterial(context));
            D.assert(material_.debugCheckHasMaterialLocalizations(context));
            return new ListTile(
                leading: icon,
                title: child ?? new Text(MaterialLocalizations.of(context).aboutListTileTitle(
                    applicationName ?? material_._defaultApplicationName(context)
                )),
                dense: dense,
                onTap: () => {
                    material_.showAboutDialog(
                        context: context,
                        applicationName: applicationName,
                        applicationVersion: applicationVersion,
                        applicationIcon: applicationIcon,
                        applicationLegalese: applicationLegalese,
                        children: aboutBoxChildren
                    );
                }
            );
        }
    }

    public partial class material_ {
        public static void showAboutDialog(
            BuildContext context,
            string applicationName = null,
            string applicationVersion = null,
            Widget applicationIcon = null,
            string applicationLegalese = null,
            List<Widget> children = null,
            bool useRootNavigator = true,
            RouteSettings routeSettings = null
        ) {
            D.assert(context != null);
            showDialog<object>(
                context: context,
                useRootNavigator: useRootNavigator,
                builder: (BuildContext buildContext) => {
                    return new AboutDialog(
                        applicationName: applicationName,
                        applicationVersion: applicationVersion,
                        applicationIcon: applicationIcon,
                        applicationLegalese: applicationLegalese,
                        children: children
                    );
                }
                ,
                routeSettings: routeSettings
            );
        }


        public static void showLicensePage(
            BuildContext context,
            string applicationName = null,
            string applicationVersion = null,
            Widget applicationIcon = null,
            string applicationLegalese = null,
            bool useRootNavigator = false
        ) {
            D.assert(context != null);
            Navigator.of(context, rootNavigator: useRootNavigator).push(new MaterialPageRoute(
                builder: (BuildContext buildContext) => new LicensePage(
                    applicationName: applicationName,
                    applicationVersion: applicationVersion,
                    applicationIcon: applicationIcon,
                    applicationLegalese: applicationLegalese
                )
            ));
        }
    }

    public class AboutDialog : StatelessWidget {
        public AboutDialog(
            Key key = null,
            string applicationName = null,
            string applicationVersion = null,
            Widget applicationIcon = null,
            string applicationLegalese = null,
            List<Widget> children = null
        ) : base(key: key) {
            this.applicationName = applicationName;
            this.applicationVersion = applicationVersion;
            this.applicationIcon = applicationIcon;
            this.applicationLegalese = applicationLegalese;
            this.children = children;
        }

        public readonly string applicationName;

        public readonly string applicationVersion;

        public readonly Widget applicationIcon;

        public readonly string applicationLegalese;

        public readonly List<Widget> children;

        public override Widget build(BuildContext context) {
            D.assert(material_.debugCheckHasMaterialLocalizations(context));
            string name = applicationName ?? material_._defaultApplicationName(context);
            string version = applicationVersion ?? material_._defaultApplicationVersion(context);
            Widget icon = applicationIcon ?? material_._defaultApplicationIcon(context);
            var listChildren = new List<Widget>();
            var rowChildren = new List<Widget>();
            if (icon != null) {
                rowChildren.Add(new IconTheme(data: Theme.of(context).iconTheme, child: icon));
            }

            rowChildren.Add(new Expanded(
                child: new Padding(
                    padding: EdgeInsets.symmetric(horizontal: 24.0f),
                    child: new ListBody(
                        children: new List<Unity.UIWidgets.widgets.Widget>() {
                            new Text(name, style: Theme.of(context).textTheme.headline5),
                            new Text(version, style: Theme.of(context).textTheme.bodyText2),
                            new Container(height: 18.0f),
                            new Text(applicationLegalese ?? "", style: Theme.of(context).textTheme.caption)
                        }
                    )
                )
            ));
            listChildren.Add(new Row(
                    crossAxisAlignment: CrossAxisAlignment.start,
                    children: rowChildren
                )
            );
            listChildren.AddRange(children);
            return new AlertDialog(
                content: new ListBody(
                    children: listChildren
                ),
                actions: new List<Unity.UIWidgets.widgets.Widget>(){
                    new FlatButton(
                        child: new Text(MaterialLocalizations.of(context).viewLicensesButtonLabel),
                        onPressed: () => {
                            material_.showLicensePage(
                                context: context,
                                applicationName: applicationName,
                                applicationVersion: applicationVersion,
                                applicationIcon: applicationIcon,
                                applicationLegalese: applicationLegalese
                            );
                        }
                    ),
                    new FlatButton(
                        child: new Text(MaterialLocalizations.of(context).closeButtonLabel),
                        onPressed: () => { Navigator.pop<object>(context); }
                    )
                },
                scrollable: true
            );
        }
    }

    public class LicensePage : StatefulWidget {
        public LicensePage(
            Key key = null,
            string applicationName = null,
            string applicationVersion = null,
            Widget applicationIcon = null,
            string applicationLegalese = null
        ) : base(key: key) {
            this.applicationName = applicationName;
            this.applicationVersion = applicationVersion;
            this.applicationIcon = applicationIcon;
            this.applicationLegalese = applicationLegalese;
        }

        public readonly string applicationName;

        public readonly string applicationVersion;

        public readonly Widget applicationIcon;

        public readonly string applicationLegalese;

        public override State createState() => new _LicensePageState();
    }

    public class _LicensePageState : State<LicensePage> {
        public override void initState() {
            base.initState();
            _initLicenses();
        }

        readonly List<Widget> _licenses = new List<Widget>();
        bool _loaded = false;

        Future _initLicenses() {
            // int debugFlowId = -1;
            // D.assert(() => {
            //   Flow flow = Flow.begin();
            //   Timeline.timeSync("_initLicenses()", () { }, flow: flow);
            //   debugFlowId = flow.id;
            //   return true;
            // }());
            // Future.forEach(LicenseRegistry.licenses, license => { });
            // await for (final LicenseEntry license in LicenseRegistry.licenses) {
            //   if (!mounted) {
            //     return;
            //   }
            //   D.assert(() {
            //     Timeline.timeSync("_initLicenses()", () { }, flow: Flow.step(debugFlowId));
            //     return true;
            //   }());
            //   final List<LicenseParagraph> paragraphs =
            //     await SchedulerBinding.instance.scheduleTask<List<LicenseParagraph>>(
            //       license.paragraphs.toList,
            //       Priority.animation,
            //       debugLabel: "License",
            //     );
            //   if (!mounted) {
            //     return;
            //   }
            //   setState(() {
            //     _licenses.add(const Padding(
            //       padding: EdgeInsets.symmetric(vertical: 18.0),
            //       child: Text(
            //         "🍀‬", // That"s U+1F340. Could also use U+2766 (❦) if U+1F340 doesn"t work everywhere.
            //         textAlign: TextAlign.center,
            //       ),
            //     ));
            //     _licenses.add(Container(
            //       decoration: const BoxDecoration(
            //         border: Border(bottom: BorderSide(width: 0.0))
            //       ),
            //       child: Text(
            //         license.packages.join(", "),
            //         style: const TextStyle(fontWeight: FontWeight.bold),
            //         textAlign: TextAlign.center,
            //       ),
            //     ));
            //     for (final LicenseParagraph paragraph in paragraphs) {
            //       if (paragraph.indent == LicenseParagraph.centeredIndent) {
            //         _licenses.add(Padding(
            //           padding: const EdgeInsets.only(top: 16.0),
            //           child: Text(
            //             paragraph.text,
            //             style: const TextStyle(fontWeight: FontWeight.bold),
            //             textAlign: TextAlign.center,
            //           ),
            //         ));
            //       } else {
            //         D.assert(paragraph.indent >= 0);
            //         _licenses.add(Padding(
            //           padding: EdgeInsetsDirectional.only(top: 8.0, start: 16.0 * paragraph.indent),
            //           child: Text(paragraph.text),
            //         ));
            //       }
            //     }
            //   });
            // }
            //TODO: implement
            setState(() => { _loaded = true; });
            return null;
            // D.assert(() {
            //   Timeline.timeSync("Build scheduled", () { }, flow: Flow.end(debugFlowId));
            //   return true;
            // }());
        }

        public override Widget build(BuildContext context) {
            D.assert(material_.debugCheckHasMaterialLocalizations(context));
            string name = widget.applicationName ?? material_._defaultApplicationName(context);
            string version = widget.applicationVersion ?? material_._defaultApplicationVersion(context);
            Widget icon = widget.applicationIcon ?? material_._defaultApplicationIcon(context);
            MaterialLocalizations localizations = MaterialLocalizations.of(context);
            var list = new List<Widget>();
            list.Add(new Text(name, style: Theme.of(context).textTheme.headline5, textAlign: TextAlign.center));

            if (icon != null) {
                list.Add(new IconTheme(data: Theme.of(context).iconTheme, child: icon));
            }

            list.Add(new Text(version, style: Theme.of(context).textTheme.bodyText2, textAlign: TextAlign.center));
            list.Add(new Container(height: 18.0f));
            list.Add(new Text(widget.applicationLegalese ?? "", style: Theme.of(context).textTheme.caption,
                textAlign: TextAlign.center));
            list.Add(new Container(height: 18.0f));
            list.Add(new Text("Powered by Flutter", style: Theme.of(context).textTheme.bodyText2,
                textAlign: TextAlign.center));
            list.Add(new Container(height: 24.0f));
            list.AddRange(_licenses);
            if (!_loaded) {
                list.Add(new Padding(
                    padding: EdgeInsets.symmetric(vertical: 24.0f),
                    child: new Center(
                        child: new CircularProgressIndicator()
                    )
                ));
            }

            return new Scaffold(
                appBar: new AppBar(
                    title: new Text(localizations.licensesPageTitle)
                ),
                // All of the licenses page text is English. We don"t want localized text
                // or text direction.
                body: Localizations.overrides(
                    locale: new Locale("en", "US"),
                    context: context,
                    child: new DefaultTextStyle(
                        style: Theme.of(context).textTheme.caption,
                        child: new SafeArea(
                            bottom: false,
                            child: new Scrollbar(
                                child: new ListView(
                                    padding: EdgeInsets.symmetric(horizontal: 8.0f, vertical: 12.0f),
                                    children: list
                                )
                            )
                        )
                    )
                )
            );
        }
    }

    public partial class material_ {
        internal static string _defaultApplicationName(BuildContext context) {
            // This doesn"t handle the case of the application"s title dynamically
            // changing. In theory, we should make Title expose the current application
            // title using an InheritedWidget, and so forth. However, in practice, if
            // someone really wants their application title to change dynamically, they
            // can provide an explicit applicationName to the widgets defined in this
            // file, instead of relying on the default.
            Title ancestorTitle = context.findAncestorWidgetOfExactType<Title>();
            return ancestorTitle?.title ;//?? Platform.resolvedExecutable.split(Platform.pathSeparator).last;
        }

        internal static string _defaultApplicationVersion(BuildContext context) {
            // TODO(ianh): Get this from the embedder somehow.
            return "";
        }

        internal static Widget _defaultApplicationIcon(BuildContext context) {
            // TODO(ianh): Get this from the embedder somehow.
            return null;
        }
    }
}