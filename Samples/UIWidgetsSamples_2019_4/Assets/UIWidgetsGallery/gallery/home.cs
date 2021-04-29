using System;
using System.Collections.Generic;
using System.Linq;
using uiwidgets;
using Unity.UIWidgets.animation;
using Unity.UIWidgets.async;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.gestures;
using Unity.UIWidgets.material;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.rendering;
using Unity.UIWidgets.service;
using Unity.UIWidgets.ui;
using Unity.UIWidgets.widgets;
using UnityEngine;
using Color = Unity.UIWidgets.ui.Color;

namespace UIWidgetsGallery.gallery
{
    public static class GalleryHomeUtils
    {
        public static readonly string _kGalleryAssetsPackage = "uiwdigets_gallery_assets";
        public static readonly Color _kFlutterBlue = new Color(0xFF003D75);
        public const float _kDemoItemHeight = 64.0f;
        public static readonly TimeSpan _kFrontLayerSwitchDuration = new TimeSpan(0, 0, 0, 0, 300);
    }

    internal class _CategoryItem : StatelessWidget
    {
        public _CategoryItem(
            Key key = null,
            GalleryDemoCategory category = null,
            VoidCallback onTap = null
        ) : base(key: key)
        {
            this.category = category;
            this.onTap = onTap;
        }

        public readonly GalleryDemoCategory category;
        public readonly VoidCallback onTap;

        public override Widget build(BuildContext context)
        {
            ThemeData theme = Theme.of(context);
            bool isDark = theme.brightness == Brightness.dark;

            // This repaint boundary prevents the entire _CategoriesPage from being
            // repainted when the button's ink splash animates.
            return new RepaintBoundary(
                child: new RawMaterialButton(
                    padding: EdgeInsets.zero,
                    hoverColor: theme.primaryColor.withOpacity(0.05f),
                    splashColor: theme.primaryColor.withOpacity(0.12f),
                    highlightColor: Colors.transparent,
                    onPressed: this.onTap,
                    child: new Column(
                        mainAxisAlignment: MainAxisAlignment.end,
                        crossAxisAlignment: CrossAxisAlignment.center,
                        children: new List<Widget>
                        {
                            new Padding(
                                padding: EdgeInsets.all(6.0f),
                                child: new Icon(this.category.icon,
                                    size: 60.0f,
                                    color: isDark ? Colors.white : GalleryHomeUtils._kFlutterBlue
                                )
                            ),
                            new SizedBox(height: 10.0f),
                            new Container(
                                height: 48.0f,
                                alignment: Alignment.center,
                                child: new Text(this.category.name,
                                    textAlign: TextAlign.center,
                                    style: theme.textTheme.subtitle1.copyWith(
                                        color: isDark ? Colors.white : GalleryHomeUtils._kFlutterBlue
                                    )
                                )
                            )
                        }
                    )
                )
            );
        }
    }

    internal class _CategoriesPage : StatelessWidget
    {
        public _CategoriesPage(
            Key key = null,
            IEnumerable<GalleryDemoCategory> categories = null,
            ValueChanged<GalleryDemoCategory> onCategoryTap = null
        ) : base(key: key)
        {
            this.categories = categories;
            this.onCategoryTap = onCategoryTap;
        }

        public readonly IEnumerable<GalleryDemoCategory> categories;
        public readonly ValueChanged<GalleryDemoCategory> onCategoryTap;

        public override Widget build(BuildContext context)
        {
            float aspectRatio = 160.0f / 180.0f;
            List<GalleryDemoCategory> categoriesList = this.categories.ToList();
            int columnCount = (MediaQuery.of(context).orientation == Orientation.portrait) ? 2 : 3;

            return new SingleChildScrollView(
                key: Key.key("categories"),
                child: new LayoutBuilder(
                    builder: (BuildContext subContext, BoxConstraints constraints) =>
                    {
                        float columnWidth = constraints.biggest.width / columnCount;
                        float rowHeight = Mathf.Min(225.0f, columnWidth * aspectRatio);
                        int rowCount = (this.categories.Count() + columnCount - 1) / columnCount;

                        var children = new List<Widget>();

                        for (var i = 0; i < rowCount; i++)
                        {
                            var rowIndex = i;
                            int columnCountForRow = rowIndex == rowCount - 1
                                ? this.categories.Count() - columnCount * Mathf.Max(0, rowCount - 1)
                                : columnCount;

                            var subChildren = new List<Widget>();

                            for (var j = 0; j < columnCountForRow; j++)
                            {
                                var columnIndex = j;
                                int index = rowIndex * columnCount + columnIndex;
                                GalleryDemoCategory category = categoriesList[index];

                                subChildren.Add(new SizedBox(width: columnWidth,
                                    height: rowHeight,
                                    child: new _CategoryItem(
                                        category: category,
                                        onTap: () => { this.onCategoryTap(category); }
                                    )
                                ));
                            }

                            children.Add(new Row(
                                children: subChildren
                            ));
                        }

                        return new RepaintBoundary(
                            child: new Column(
                                mainAxisSize: MainAxisSize.min,
                                crossAxisAlignment: CrossAxisAlignment.stretch,
                                children: children
                            )
                        );
                    }));
        }
    }

    internal class _DemoItem : StatelessWidget
    {
        public _DemoItem(Key key = null, GalleryDemo demo = null) : base(key: key)
        {
            this.demo = demo;
        }

        public readonly GalleryDemo demo;

        private void _launchDemo(BuildContext context)
        {
            if (this.demo.routeName != null) Navigator.pushNamed(context, this.demo.routeName);
        }

        public override Widget build(BuildContext context)
        {
            ThemeData theme = Theme.of(context);
            bool isDark = theme.brightness == Brightness.dark;
            float textScaleFactor = MediaQuery.textScaleFactorOf(context);

            var children = new List<Widget>
            {
                new Text(this.demo.title,
                    style: theme.textTheme.subtitle1.copyWith(
                        color: isDark ? Colors.white : new Color(0xFF202124)
                    )
                )
            };

            if (this.demo.subtitle != null)
                children.Add(new Text(this.demo.subtitle,
                    style: theme.textTheme.bodyText2.copyWith(
                        color: isDark ? Colors.white : new Color(0xFF60646B)
                    )
                ));

            return new RawMaterialButton(
                padding: EdgeInsets.zero,
                splashColor: theme.primaryColor.withOpacity(0.12f),
                highlightColor: Colors.transparent,
                onPressed: () => { this._launchDemo(context); },
                child: new Container(
                    constraints: new BoxConstraints(minHeight: GalleryHomeUtils._kDemoItemHeight * textScaleFactor),
                    child: new Row(
                        children: new List<Widget>
                        {
                            new Container(
                                width: 56.0f,
                                height: 56.0f,
                                alignment: Alignment.center,
                                child: new Icon(this.demo.icon,
                                    size: 24.0f,
                                    color: isDark ? Colors.white : GalleryHomeUtils._kFlutterBlue
                                )
                            ),
                            new Expanded(
                                child: new Column(
                                    mainAxisAlignment: MainAxisAlignment.center,
                                    crossAxisAlignment: CrossAxisAlignment.stretch,
                                    children: children
                                )
                            ),
                            new SizedBox(width: 44.0f)
                        }
                    )
                )
            );
        }
    }

    internal class _DemosPage : StatelessWidget
    {
        public _DemosPage(GalleryDemoCategory category)
        {
            this.category = category;
        }

        public readonly GalleryDemoCategory category;

        public override Widget build(BuildContext context)
        {
            // When overriding ListView.padding, it is necessary to manually handle
            // safe areas.
            float windowBottomPadding = MediaQuery.of(context).padding.bottom;
            return new KeyedSubtree(
                key: Key.key("GalleryDemoList"), // So the tests can find this ListView
                child: new ListView(
                    dragStartBehavior: DragStartBehavior.down,
                    key: Key.key(this.category.name),
                    padding: EdgeInsets.only(top: 8.0f, bottom: windowBottomPadding),
                    children: GalleryDemo.kGalleryCategoryToDemos[this.category]
                        .Select<GalleryDemo, Widget>((GalleryDemo demo) => { return new _DemoItem(demo: demo); })
                        .ToList()
                )
            );
        }
    }

    internal class GalleryHome : StatefulWidget
    {
        public GalleryHome(
            Key key = null,
            bool testMode = false,
            Widget optionsPage = null
        ) : base(key: key)
        {
            this.testMode = testMode;
            this.optionsPage = optionsPage;
        }

        public readonly Widget optionsPage;
        public readonly bool testMode;

        // In checked mode our MaterialApp will show the default "debug" banner.
        // Otherwise show the "preview" banner.
        public static bool showPreviewBanner = true;

        public override State createState()
        {
            return new _GalleryHomeState();
        }
    }

    internal class _GalleryHomeState : SingleTickerProviderStateMixin<GalleryHome>
    {
        private static readonly GlobalKey<ScaffoldState> _scaffoldKey = GlobalKey<ScaffoldState>.key();
        private AnimationController _controller;
        private GalleryDemoCategory _category;

        private static Widget _topHomeLayout(Widget currentChild, List<Widget> previousChildren)
        {
            var children = new List<Widget>();
            children.AddRange(previousChildren);
            if (currentChild != null) children.Add(currentChild);

            return new Stack(
                children: children,
                alignment: Alignment.topCenter
            );
        }

        private static readonly AnimatedSwitcherLayoutBuilder _centerHomeLayout = AnimatedSwitcher.defaultLayoutBuilder;

        public override void initState()
        {
            base.initState();
            this._controller = new AnimationController(
                duration: new TimeSpan(0, 0, 0, 0, 600),
                debugLabel: "preview banner",
                vsync: this
            );
            this._controller.forward();
        }

        public override void dispose()
        {
            this._controller.dispose();
            base.dispose();
        }


        public override Widget build(BuildContext context)
        {
            ThemeData theme = Theme.of(context);
            bool isDark = theme.brightness == Brightness.dark;
            MediaQueryData media = MediaQuery.of(context);
            bool centerHome = media.orientation == Orientation.portrait && media.size.height < 800.0;

            Curve switchOutCurve = new Interval(0.4f, 1.0f, curve: Curves.fastOutSlowIn);
            Curve switchInCurve = new Interval(0.4f, 1.0f, curve: Curves.fastOutSlowIn);

            Widget home = new Scaffold(
                key: _scaffoldKey,
                backgroundColor: isDark ? GalleryHomeUtils._kFlutterBlue : theme.primaryColor,
                body: new SafeArea(
                    bottom: false,
                    child: new WillPopScope(
                        onWillPop: () =>
                        {
                            // Pop the category page if Android back button is pressed.
                            if (this._category != null)
                            {
                                this.setState(() => this._category = null);
                                return Future.value(false).to<bool>();
                            }

                            return Future.value(true).to<bool>();
                        },
                        child: new Backdrop(
                            backTitle: new Text("Options"),
                            backLayer: this.widget.optionsPage,
                            frontAction: new AnimatedSwitcher(
                                duration: GalleryHomeUtils._kFrontLayerSwitchDuration,
                                switchOutCurve: switchOutCurve,
                                switchInCurve: switchInCurve,
                                child: this._category == null
                                    ? (Widget) new Container()
                                    : new IconButton(
                                        icon: new BackButtonIcon(),
                                        tooltip: "Back",
                                        onPressed: () => this.setState(() => this._category = null)
                                    )
                            ),
                            frontTitle: new AnimatedSwitcher(
                                duration: GalleryHomeUtils._kFrontLayerSwitchDuration,
                                child: this._category == null
                                    ? new Text("UIWidgets gallery")
                                    : new Text(this._category.name)
                            ),
                            frontHeading: this.widget.testMode ? null : new Container(height: 24.0f),
                            frontLayer: new AnimatedSwitcher(
                                duration: GalleryHomeUtils._kFrontLayerSwitchDuration,
                                switchOutCurve: switchOutCurve,
                                switchInCurve: switchInCurve,
                                layoutBuilder: centerHome ? _centerHomeLayout : _topHomeLayout,
                                child: this._category != null
                                    ? (Widget) new _DemosPage(this._category)
                                    : new _CategoriesPage(
                                        categories: GalleryDemo.kAllGalleryDemoCategories,
                                        onCategoryTap: (GalleryDemoCategory category) =>
                                        {
                                            this.setState(() => this._category = category);
                                        }
                                    )
                            )
                        )
                    )
                )
            );

            D.assert(() =>
            {
                GalleryHome.showPreviewBanner = false;
                return true;
            });

            if (GalleryHome.showPreviewBanner)
                home = new Stack(
                    fit: StackFit.expand,
                    children: new List<Widget>
                    {
                        home,
                        new FadeTransition(
                            opacity: new CurvedAnimation(parent: this._controller, curve: Curves.easeInOut),
                            child: new Banner(
                                message: "PREVIEW",
                                location: BannerLocation.topEnd
                            )
                        )
                    }
                );
            home = new AnnotatedRegion<SystemUiOverlayStyle>(
                child: home,
                value: SystemUiOverlayStyle.light
            );

            return home;
        }
    }
}