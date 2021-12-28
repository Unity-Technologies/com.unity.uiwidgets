using System.Collections.Generic;
using System.Linq;
using uiwidgets;
using UIWidgetsGallery.gallery;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.material;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.ui;
using Unity.UIWidgets.widgets;

namespace UIWidgetsGallery.demo.material
{
    internal enum TabsDemoStyle
    {
        iconsAndText,
        iconsOnly,
        textOnly
    }

    internal class _Page
    {
        public _Page(IconData icon = null, string text = null)
        {
            this.icon = icon;
            this.text = text;
        }

        public readonly IconData icon;
        public readonly string text;

        public static readonly List<_Page> _allPages = new List<_Page>
        {
            new _Page(icon: Icons.grade, text: "TRIUMPH"),
            new _Page(icon: Icons.playlist_add, text: "NOTE"),
            new _Page(icon: Icons.check_circle, text: "SUCCESS"),
            new _Page(icon: Icons.question_answer, text: "OVERSTATE"),
            new _Page(icon: Icons.sentiment_very_satisfied, text: "SATISFACTION"),
            new _Page(icon: Icons.camera, text: "APERTURE"),
            new _Page(icon: Icons.assignment_late, text: "WE MUST"),
            new _Page(icon: Icons.assignment_turned_in, text: "WE CAN"),
            new _Page(icon: Icons.group, text: "ALL"),
            new _Page(icon: Icons.block, text: "EXCEPT"),
            new _Page(icon: Icons.sentiment_very_dissatisfied, text: "CRYING"),
            new _Page(icon: Icons.error, text: "MISTAKE"),
            new _Page(icon: Icons.loop, text: "TRYING"),
            new _Page(icon: Icons.cake, text: "CAKE")
        };
    }


    internal class ScrollableTabsDemo : StatefulWidget
    {
        public static readonly string routeName = "/material/scrollable-tabs";

        public override State createState()
        {
            return new ScrollableTabsDemoState();
        }
    }

    internal class ScrollableTabsDemoState : SingleTickerProviderStateMixin<ScrollableTabsDemo>
    {
        private TabController _controller;
        private TabsDemoStyle _demoStyle = TabsDemoStyle.iconsAndText;
        private bool _customIndicator = false;

        public override void initState()
        {
            base.initState();
            _controller = new TabController(vsync: this, length: _Page._allPages.Count);
        }

        public override void dispose()
        {
            _controller.dispose();
            base.dispose();
        }

        private void changeDemoStyle(TabsDemoStyle style)
        {
            setState(() => { _demoStyle = style; });
        }

        private Decoration getIndicator()
        {
            if (!_customIndicator)
                return new UnderlineTabIndicator();

            switch (_demoStyle)
            {
                case TabsDemoStyle.iconsAndText:
                    return new ShapeDecoration(
                        shape: new RoundedRectangleBorder(
                                   borderRadius: BorderRadius.all(Radius.circular(4.0f)),
                                   side: new BorderSide(
                                       color: Colors.white24,
                                       width: 2.0f
                                   )
                               ) + new RoundedRectangleBorder(
                                   borderRadius: BorderRadius.all(Radius.circular(4.0f)),
                                   side: new BorderSide(
                                       color: Colors.transparent,
                                       width: 4.0f
                                   )
                               )
                    );

                case TabsDemoStyle.iconsOnly:
                    return new ShapeDecoration(
                        shape: new CircleBorder(
                                   side: new BorderSide(
                                       color: Colors.white24,
                                       width: 4.0f
                                   )
                               ) + new CircleBorder(
                                   side: new BorderSide(
                                       color: Colors.transparent,
                                       width: 4.0f
                                   )
                               )
                    );

                case TabsDemoStyle.textOnly:
                    return new ShapeDecoration(
                        shape: new StadiumBorder(
                                   side: new BorderSide(
                                       color: Colors.white24,
                                       width: 2.0f
                                   )
                               ) + new StadiumBorder(
                                   side: new BorderSide(
                                       color: Colors.transparent,
                                       width: 4.0f
                                   )
                               )
                    );
            }

            return null;
        }


        public override Widget build(BuildContext context)
        {
            Color iconColor = Theme.of(context).accentColor;
            return new Scaffold(
                appBar: new AppBar(
                    title: new Text("Scrollable tabs"),
                    actions: new List<Widget>
                    {
                        new MaterialDemoDocumentationButton(ScrollableTabsDemo.routeName),
                        new IconButton(
                            icon: new Icon(Icons.sentiment_very_satisfied),
                            onPressed: () => { setState(() => { _customIndicator = !_customIndicator; }); }
                        ),
                        new PopupMenuButton<TabsDemoStyle>(
                            onSelected: changeDemoStyle,
                            itemBuilder: (BuildContext subContext) => new List<PopupMenuEntry<TabsDemoStyle>>
                            {
                                new PopupMenuItem<TabsDemoStyle>(
                                    value: TabsDemoStyle.iconsAndText,
                                    child: new Text("Icons and text")
                                ),
                                new PopupMenuItem<TabsDemoStyle>(
                                    value: TabsDemoStyle.iconsOnly,
                                    child: new Text("Icons only")
                                ),
                                new PopupMenuItem<TabsDemoStyle>(
                                    value: TabsDemoStyle.textOnly,
                                    child: new Text("Text only")
                                )
                            }
                        )
                    },
                    bottom: new TabBar(
                        controller: _controller,
                        isScrollable: true,
                        indicator: getIndicator(),
                        tabs: _Page._allPages.Select<_Page, Widget>((_Page page) =>
                        {
                            switch (_demoStyle)
                            {
                                case TabsDemoStyle.iconsAndText:
                                    return new Tab(text: page.text, icon: new Icon(page.icon));
                                case TabsDemoStyle.iconsOnly:
                                    return new Tab(icon: new Icon(page.icon));
                                case TabsDemoStyle.textOnly:
                                    return new Tab(text: page.text);
                            }
                            return null;
                        }).ToList()
                    )
                ),
                body: new TabBarView(
                    controller: _controller,
                    children: _Page._allPages.Select<_Page, Widget>((_Page page) =>
                    {
                        return new SafeArea(
                            top: false,
                            bottom: false,
                            child: new Container(
                                key: new ObjectKey(page.icon),
                                padding: EdgeInsets.all(12.0f),
                                child: new Card(
                                    child: new Center(
                                        child: new Icon(
                                            page.icon,
                                            color: iconColor,
                                            size: 128.0f
                                        )
                                    )
                                )
                            )
                        );
                    }).ToList()
                )
            );
        }
    }
}