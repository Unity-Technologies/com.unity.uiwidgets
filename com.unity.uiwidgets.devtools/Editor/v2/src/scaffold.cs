using System.Collections.Generic;
using Unity.UIWidgets.DevTools;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.ui;
using Unity.UIWidgets.widgets;
using Unity.UIWidgets.DevTools.config_specific.drag_and_drop;
using Unity.UIWidgets.DevTools.config_specific.import_export;
using Unity.UIWidgets.material;
using Unity.UIWidgets.rendering;
using Unity.UIWidgets.scheduler;

namespace Unity.UIWidgets.DevTools
{
    class DevToolsScaffold : StatefulWidget
    {
        
        public DevToolsScaffold(
            Key key = null,
            List<Screen> tabs = null,
            string initialPage = null,
            List<Widget> actions = null,
            bool embed = false
        )  : 
        base(key: key)
        {
            D.assert(tabs != null);
            this.tabs = tabs;
            this.initialPage = initialPage;
            this.actions = actions;
            this.embed = embed;
        }

        public static readonly EdgeInsets appPadding =
            EdgeInsets.fromLTRB(16.0f, 16.0f, 16.0f, 8.0f);
        
        public readonly List<Screen> tabs;
        
        public readonly string initialPage;
        
        public readonly bool embed;

        public readonly List<Widget> actions;

        public override State createState()
        {
            return new DevToolsScaffoldState();
        }

        public static DevToolsScaffold withChild(Key key = null, Widget child = null)
        {
            return new DevToolsScaffold(key: key, tabs: new List<Screen>()  {new SimpleScreen(child)});
        }
    }

    class DevToolsScaffoldState : TickerProviderStateMixin<DevToolsScaffold>
    {
        ValueNotifier<Screen> _currentScreen = new ValueNotifier<Screen>(null);
        ImportController _importController;
        TabController _tabController;
        List<Ticker> _tickers;
        
        public override void didChangeDependencies() {
            base.didChangeDependencies();

            _importController = new ImportController(
                Notifications.of(context),
                _pushSnapshotScreenForImport
            );
        }
        
        
        public override void initState() {
            base.initState();

            _setupTabController();

            // _connectVmSubscription =
            //     frameworkController.onConnectVmEvent.listen(_connectVm);
            // _showPageSubscription =
            //     frameworkController.onShowPageId.listen(_showPageById);
        }
        
        public override void didUpdateWidget(StatefulWidget oldWidget) {
            base.didUpdateWidget(oldWidget);

            if (widget.tabs.Count != ((DevToolsScaffold)oldWidget).tabs.Count) {
                var newIndex = 0;
                if (_tabController != null &&
                    widget.tabs.Contains(((DevToolsScaffold)oldWidget).tabs[_tabController.index])) {
                    newIndex = widget.tabs.IndexOf(((DevToolsScaffold)oldWidget).tabs[_tabController.index]);
                }

                _setupTabController();
                _tabController.index = newIndex;
            }
        }
        
        void _setupTabController() {
            _tabController?.dispose();
            _tabController = new TabController(length: widget.tabs.Count, vsync: this);
            if (widget.initialPage != null) {
                int initialIndex = -1;
                for (int i = 0; i< widget.tabs.Count; i++)
                {
                    if (widget.tabs[i].screenId == widget.initialPage)
                    {
                        initialIndex = i;
                    }
                }
                if (initialIndex != -1) {
                    _tabController.index = initialIndex;
                }
            }

            _currentScreen.value = widget.tabs[_tabController.index];
            _tabController.addListener(() => {
                var screen = widget.tabs[_tabController.index];

                if (_currentScreen.value != screen) {
                    _currentScreen.value = screen;
                    
                    Globals.frameworkController.notifyPageChange(screen?.screenId);
                }
            });

            Globals.frameworkController.notifyPageChange(_currentScreen.value.screenId);
        }
        
        
        
        public void _pushSnapshotScreenForImport(string screenId) {
            // var args = new SnapshotArguments(screenId);
            // if (offlineMode) {
            //     if (ModalRoute.of(context).settings.name == snapshotRoute) {
            //         Navigator.popAndPushNamed(context, snapshotRoute, arguments: args);
            //     }
            // } else {
            //     Navigator.pushNamed(context, snapshotRoute, arguments: args);
            // }
            // setState(() => {
            //     enterOfflineMode();
            // });
        }
        
        public override Widget build(BuildContext context)
        {
            var tabBodies = new List<Widget>();
            if (widget.tabs != null)
            {
                foreach (var screen in widget.tabs)
                {
                    tabBodies.Add(new Container(
                        padding: DevToolsScaffold.appPadding,
                        alignment: Alignment.topLeft,
                        child: new FocusScope(
                            child: new BannerMessages(
                                screen: screen
                            )
                        )
                    ));
                };
            }

            return new ValueListenableProvider<Screen>(
                value: _currentScreen,
                child: new Provider<BannerMessagesController>(
                    create: (_) => new BannerMessagesController(),
                    // child: new Container(height:100,width:200,color:Color.white)
                    child: new DragAndDrop(
                        handleDrop: null,
                        child: new Scaffold(
                            appBar: null, // widget.embed ? null : _buildAppBar(),
                            body: new TabBarView(
                                physics: ThemeUtils.defaultTabBarViewPhysics,
                                controller: _tabController,
                                children: tabBodies
                            ),
                            bottomNavigationBar: 
                            widget.embed ? null : _buildStatusLine(context)
                        )
                    )
                )
            );
        }
        
        Widget _buildStatusLine(BuildContext context) {
            var appPadding = DevToolsScaffold.appPadding;

            return new Container(
                height: 48.0f,
                child: new Column(
                    crossAxisAlignment: CrossAxisAlignment.stretch,
                    children: new List<Widget>{
                        // new PaddedDivider(padding: EdgeInsets.zero), [attention]
                        new Padding(
                            padding: EdgeInsets.only(
                                left: appPadding.left,
                                right: appPadding.right,
                                bottom: appPadding.bottom
                            ),
                            child: new Container()// StatusLine()
                ),
                }
                )
                );
        }

        public Ticker createTicker(TickerCallback onTick)
        {
            // if (_tickers == null)
            // {
            //     _tickers = new List<Ticker>();
            // }
            // _WidgetTicker<DevToolsScaffold> result = new _WidgetTicker<DevToolsScaffold>(onTick, this, debugLabel: $"created by {this}");
            // _tickers.Add(result);
            // return result;
            return null;
        }
    }
    
    
    class SimpleScreen : Screen {
        public SimpleScreen(Widget child) : base(id)
        {
            this.child = child;
        }

        public static readonly string id = "simple";

        public readonly Widget child;

        public override Widget build(BuildContext context)
        {
            return child;
        }
    }
}