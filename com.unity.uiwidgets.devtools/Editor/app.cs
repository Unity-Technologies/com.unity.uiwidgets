using System;
using System.Collections.Generic;
using System.Linq;
using Unity.UIWidgets.async2;
using Unity.UIWidgets.DevTools;
using Unity.UIWidgets.DevTools.analytics;
using Unity.UIWidgets.DevTools.config_specific.ide_theme;
using Unity.UIWidgets.DevTools.inspector;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.material;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.rendering;
using Unity.UIWidgets.ui;
using Unity.UIWidgets.widgets;
using UnityEngine;
using Screen = Unity.UIWidgets.DevTools.Screen;
namespace Unity.UIWidgets.DevTools
{
    public class appUtils
    {
      public static readonly bool showVmDeveloperMode = false;

      /// Whether this DevTools build is external.
      public bool isExternalBuild = true;
      
      public static List<DevToolsScreen<object>> defaultScreens {
        get
        {
          return new List<DevToolsScreen<object>>
          {
            new DevToolsScreen<object>(
              new InspectorScreen(),
              createController: () => new InspectorSettingsController()
            )

          };
        }
  
      }
    }
    
    
public class DevToolsApp : StatefulWidget {
  public DevToolsApp(
    List<DevToolsScreen<object>> screens,
    IdeTheme ideTheme,
    AnalyticsProvider analyticsProvider
  )
  {
    this.screens = screens;
    this.ideTheme = ideTheme;
    this.analyticsProvider = analyticsProvider;
  }

  public readonly List<DevToolsScreen<object>> screens;
  public readonly IdeTheme ideTheme;
  public readonly AnalyticsProvider analyticsProvider;
  
  public override State createState()
  {
    return new DevToolsAppState();
  }
}

// TODO(https://github.com/flutter/devtools/issues/1146): Introduce tests that
// navigate the full app.
public class DevToolsAppState : State<DevToolsApp> {
  List<Screen> _screens
  {
    get
    {
      List<Screen> screensList = new List<Screen>();
      foreach (var s in widget.screens)
      {
        screensList.Add(s.screen);
      }

      return screensList;
    }
  }

  IdeTheme ideTheme
  {
    get
    {
      return widget.ideTheme;
    }
  }

  bool isDarkThemeEnabled
  {
    get
    {
      return _isDarkThemeEnabled;
    }
  }

  bool _isDarkThemeEnabled;

  bool vmDeveloperModeEnabled
  {
    get
    {
      return _vmDeveloperModeEnabled;
    }
  }

  bool _vmDeveloperModeEnabled;
  
   public override void initState() {
     base.initState();
  
     // ga.setupDimensions();
     //
     // serviceManager.isolateManager.onSelectedIsolateChanged.listen((_) => {
     //   setState(() => {
     //     _clearCachedRoutes();
     //   });
     // });
     //
     // _isDarkThemeEnabled = preferences.darkModeTheme.value;
     // preferences.darkModeTheme.addListener(() => {
     //   setState(() => {
     //     _isDarkThemeEnabled = preferences.darkModeTheme.value;
     //   });
     // });
     //
     // _vmDeveloperModeEnabled = preferences.vmDeveloperModeEnabled.value;
     // preferences.vmDeveloperModeEnabled.addListener(() => {
     //   setState(() => {
     //     _vmDeveloperModeEnabled = preferences.vmDeveloperModeEnabled.value;
     //   });
     // });
   }

  
   public void didUpdateWidget(DevToolsApp oldWidget) {
     base.didUpdateWidget(oldWidget);
     //_clearCachedRoutes();
   }
  
    // Gets the page for a given page/path and args.
   Page _getPage(BuildContext context2, string page, Dictionary<string, string> args) {
     // Provide the appropriate page route.
     if (pages.ContainsKey(page)) {
       Widget widget = pages[page](
         context2,
         page,
         args
       );
       D.assert(() => {
         widget = new _AlternateCheckedModeBanner(
           builder: (context) => pages[page](
             context,
             page,
             args
           )
         );
         return true;
       });
       return MaterialPage(child: widget);
     }
  
     // Return a page not found.
     return MaterialPage(
       child: DevToolsScaffold.withChild(
         key: Key.key("not-found"),
         child: CenteredMessage("'$page' not found."),
         ideTheme: ideTheme,
         analyticsProvider: widget.analyticsProvider
       )
     );
   }
  
   Widget _buildTabbedPage(
     BuildContext context,
     string page,
     Dictionary<string, string> _params
   ) {
     var vmServiceUri = _params["uri"];
  
     // Always return the landing screen if there's no VM service URI.
     if (vmServiceUri?.isEmpty() ?? true) {
       return DevToolsScaffold.withChild(
         key: Key.key("landing"),
         child: LandingScreenBody(),
         ideTheme: ideTheme,
         analyticsProvider: widget.analyticsProvider,
         actions: new List<Widget>{
           new OpenSettingsAction(),
           new OpenAboutAction()
         }
       );
     }
  
     // TODO(dantup): We should be able simplify this a little, removing params['page']
     // and only supporting /inspector (etc.) instead of also &page=inspector if
     // all IDEs switch over to those URLs.
     if (page?.isEmpty() ?? true) {
       page = _params["page"];
     }
     var embed = _params["embed"] == "true";
     var hide = _params["hide"]?.Split(',');
     return Initializer(
       url: vmServiceUri,
       allowConnectionScreenOnDisconnect: !embed,
       builder: (_) => {
         return new ValueListenableBuilder<bool>(
           valueListenable: preferences.vmDeveloperModeEnabled,
           builder: (_, __, ___) => {
             var tabs = _visibleScreens()
                 .where((p) => embed && page != null ? p.screenId == page : true)
                 .where((p) => !hide.Contains(p.screenId))
                 .toList();
             if (tabs.isEmpty) {
               return DevToolsScaffold.withChild(
                 child: CenteredMessage(
                     $"The \"{page}\" screen is not available for this application."),
                 ideTheme: ideTheme,
                 analyticsProvider: widget.analyticsProvider
               );
             }
  
             List<Widget> widgets = new List<Widget>();
             if (serviceManager.connectedApp.isFlutterAppNow)
             {
               widgets.Add(HotReloadButton());
               widgets.Add(HotRestartButton());
             }
             widgets.Add(new OpenSettingsAction());
             widgets.Add(new OpenAboutAction());
             
             return _providedControllers(
               child: DevToolsScaffold(
                 embed: embed,
                 ideTheme: ideTheme,
                 page: page,
                 tabs: tabs,
                 analyticsProvider: widget.analyticsProvider,
                 actions: widgets
               )
             );
           }
         );
       }
     );
   }
  
  // The pages that the app exposes.
   Dictionary<string, UrlParametersBuilder> pages {
     get
     {
       if (_routes == null)
       {
         return null;
       }
       return _routes ?? {
         homePageId: _buildTabbedPage,
         foreach (Screen screen in widget.screens)
         screen.screen.screenId: _buildTabbedPage,
         snapshotPageId: (_, __, args) => {
           final snapshotArgs = SnapshotArguments.fromArgs(args);
           return DevToolsScaffold.withChild(
             key: new UniqueKey(),
             analyticsProvider: widget.analyticsProvider,
             child: _providedControllers(
               offline: true,
               child: SnapshotScreenBody(snapshotArgs, _screens),
             ),
             ideTheme: ideTheme,
           );
         },
         appSizePageId: (_, __, ___) =>{
           return DevToolsScaffold.withChild(
               key: Key.key("appsize"),
               analyticsProvider: widget.analyticsProvider,
               child: _providedControllers(
                 child: AppSizeBody()
               ),
               ideTheme: ideTheme,
               actions: [
               OpenSettingsAction(),
               OpenAboutAction(),
             ],
             );
         },
       };
     }
     
   }

  Dictionary<string, UrlParametersBuilder> _routes;

  void _clearCachedRoutes() {
    _routes = null;
  }

  List<Screen> _visibleScreens() => _screens.where(shouldShowScreen).toList();
  
  Widget _providedControllers({@required Widget child, bool offline = false}) {
    var _providers = widget.screens
        .where((s) =>
            s.createController != null && (offline ? s.supportsOffline : true))
        .map((s) => s.controllerProvider)
        .toList();
  
    return MultiProvider(
      providers: _providers,
      child: child,
    );
  }

  
  public override Widget build(BuildContext context) {
    return new MaterialApp(
      debugShowCheckedModeBanner: false,
      theme: ThemeData.light(),//themeFor(isDarkTheme: isDarkThemeEnabled, ideTheme: ideTheme),
       builder: (context2, child) => null//Notifications(child: child),
       // routerDelegate: DevToolsRouterDelegate(_getPage),
       // routeInformationParser: DevToolsRouteInformationParser()
    );
  }
}

public class DevToolsScreen<C> {
  
  public delegate C CreateController();
  public DevToolsScreen(
    Screen screen,
    CreateController createController = null,
    bool supportsOffline = false
  )
  {
    this.screen = screen;
    this.createController = createController;
    this.supportsOffline = supportsOffline;
  }

  public readonly Screen screen;
  public readonly CreateController createController;
  public readonly bool supportsOffline;

  
  Provider<C> controllerProvider {
    get
    {
      D.assert(createController != null);
      return Provider<C>(create: (_) => createController());
    }
    
  }
}


  public delegate Widget UrlParametersBuilder(
    BuildContext buildContext,
    string s,
    Dictionary<string, string> dictionary
  );

public class _AlternateCheckedModeBanner : StatelessWidget {
  public _AlternateCheckedModeBanner(Key key = null, WidgetBuilder builder = null) : base(key: key)
  {
    this.builder = builder;
  }
  public readonly WidgetBuilder builder;

  
  public override Widget build(BuildContext context) {
    return new Banner(
      message: "DEBUG",
      textDirection: TextDirection.ltr,
      location: BannerLocation.topStart,
      child: new Builder(
        builder: builder
      )
    );
  }
}

/*public class OpenAboutAction : StatelessWidget {
  
  public override Widget build(BuildContext context) {
    return DevToolsTooltip(
      tooltip: "About DevTools",
      child: new InkWell(
        onTap: () => {
          unawaited(showDialog(
            context: context,
            builder: (context2) => new DevToolsAboutDialog()
          ));
        },
        child: new Container(
          width: DevToolsScaffold.actionWidgetSize,
          height: DevToolsScaffold.actionWidgetSize,
          alignment: Alignment.center,
          child: new Icon(
            Icons.help_outline,
            size: actionsIconSize
          )
        )
      )
    );
  }
}

public class  OpenSettingsAction : StatelessWidget {
  
  public override Widget build(BuildContext context) {
    return DevToolsTooltip(
      tooltip: "Settings",
      child: new InkWell(
        onTap: () => {
          unawaited(showDialog(
            context: context,
            builder: (context2) => new SettingsDialog()
          ));
        },
        child: new Container(
          width: DevToolsScaffold.actionWidgetSize,
          height: DevToolsScaffold.actionWidgetSize,
          alignment: Alignment.center,
          child: new Icon(
            Icons.settings,
            size: actionsIconSize
          )
        )
      )
    );
  }
}

public class DevToolsAboutDialog : StatelessWidget {
  
  public override Widget build(BuildContext context) {
    var theme = Theme.of(context);

    List<Widget> widgets = new List<Widget>();
    widgets.Add(new SizedBox(height: defaultSpacing));
    List<Widget> temp = dialogSubHeader(theme, "Feedback");
    foreach (var widget in temp)
    {
      widgets.Add(widget);
    }
    widgets.Add(new Wrap(
      children: new List<Widget>{
        new Text("Encountered an issue? Let us know at "),
        _createFeedbackLink(context),
        new Text(".")
      }
    ));
    
    
    return DevToolsDialog(
      title: dialogTitleText(theme, "About DevTools"),
      content: new Column(
        mainAxisSize: MainAxisSize.min,
        crossAxisAlignment: CrossAxisAlignment.start,
        children: widgets
      ),
      actions: [
        DialogCloseButton(),
      ],
    );
  }

  Widget _aboutDevTools(BuildContext context) {
    return const SelectableText('DevTools version ${devtools.version}');
  }

  Widget _createFeedbackLink(BuildContext context) {
    const urlPath = 'github.com/flutter/devtools/issues';
    final colorScheme = Theme.of(context).colorScheme;
    return InkWell(
      onTap: () async {
        ga.select(devToolsMain, feedback);

        const reportIssuesUrl = 'https://$urlPath';
        await launchUrl(reportIssuesUrl, context);
      },
      child: Text(urlPath, style: linkTextStyle(colorScheme)),
    );
  }
}

TODO(kenz): merge the checkbox functionality here with [NotifierCheckbox]
class SettingsDialog extends StatelessWidget {
  @override
  Widget build(BuildContext context) {
    return DevToolsDialog(
      title: dialogTitleText(Theme.of(context), 'Settings'),
      content: Column(
        mainAxisSize: MainAxisSize.min,
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          _buildOption(
            label: const Text('Use a dark theme'),
            listenable: preferences.darkModeTheme,
            toggle: preferences.toggleDarkModeTheme,
          ),
          if (isExternalBuild && isDevToolsServerAvailable)
            _buildOption(
              label: const Text('Enable analytics'),
              listenable: ga.gaEnabledNotifier,
              toggle: ga.setAnalyticsEnabled,
            ),
          _buildOption(
            label: const Text('Enable VM developer mode'),
            listenable: preferences.vmDeveloperModeEnabled,
            toggle: preferences.toggleVmDeveloperMode,
          ),
        ],
      ),
      actions: [
        DialogCloseButton(),
      ],
    );
  }

  Widget _buildOption({
    Text label,
    ValueListenable<bool> listenable,
    Function(bool) toggle,
  }) {
    return InkWell(
      onTap: () => toggle(!listenable.value),
      child: Row(
        children: [
          ValueListenableBuilder<bool>(
            valueListenable: listenable,
            builder: (context, value, _) {
              return Checkbox(
                value: value,
                onChanged: toggle,
              );
            },
          ),
          label,
        ],
      ),
    );
 }*/
}





