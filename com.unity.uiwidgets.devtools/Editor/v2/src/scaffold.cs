using System.Collections.Generic;
using Unity.UIWidgets.DevTools;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.widgets;

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
            return new DevToolsScaffold(key: key, tabs: new List<Screen>(){new SimpleScreen(child)});
        }
    }

    class DevToolsScaffoldState : State<DevToolsScaffold>
    {
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
                
            
            // return ValueListenableProvider.value(
            //     value: _currentScreen,
            //     child: Provider<BannerMessagesController>(
            //         create: (_) => BannerMessagesController(),
            //         child: DragAndDrop(
            //             // TODO(kenz): we are handling drops from multiple scaffolds. We need
            //             // to make sure we are only handling drops from the active scaffold.
            //             handleDrop: _importController.importData,
            //             child: Scaffold(
            //                 appBar: widget.embed ? null : _buildAppBar(),
            //                 body: TabBarView(
            //                     physics: defaultTabBarViewPhysics,
            //                     controller: _tabController,
            //                     children: tabBodies
            //                 ),
            //                 bottomNavigationBar:
            //                 widget.embed ? null : _buildStatusLine(context)
            //             )
            //         )
            //     )
            // );
            return new Container(child:new Text("enter here"));
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