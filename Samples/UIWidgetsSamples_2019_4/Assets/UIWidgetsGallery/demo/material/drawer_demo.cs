using System;
using System.Collections.Generic;
using System.Linq;
using UIWidgetsGallery.gallery;
using Unity.UIWidgets.animation;
using Unity.UIWidgets.gestures;
using Unity.UIWidgets.material;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.rendering;
using Unity.UIWidgets.ui;
using Unity.UIWidgets.widgets;
using UnityEngine;

namespace UIWidgetsGallery.demo.material
{
    public static class DrawerDemoUtils
    {
        public static readonly string _kAsset0 = "gallery/people/square/trevor.png";
        public static readonly string _kAsset1 = "gallery/people/square/stella.png";
        public static readonly string _kAsset2 = "gallery/people/square/sandra.png";
        public static readonly string _kGalleryAssetsPackage = "flutter_gallery_assets";
    }

    internal class DrawerDemo : StatefulWidget
    {
        public static readonly string routeName = "/material/drawer";

        public override State createState()
        {
            return new _DrawerDemoState();
        }
    }

    internal class _DrawerDemoState : TickerProviderStateMixin<DrawerDemo>
    {
        private GlobalKey<ScaffoldState> _scaffoldKey = GlobalKey<ScaffoldState>.key();

        private static readonly List<string> _drawerContents = new List<string>
        {
            "A", "B", "C", "D", "E",
        };

        private static readonly Animatable<Offset> _drawerDetailsTween = new OffsetTween(
            begin: new Offset(0.0f, -1.0f),
            end: Offset.zero
        ).chain(new CurveTween(
            curve: Curves.fastOutSlowIn
        ));

        private AnimationController _controller;
        private Animation<float> _drawerContentsOpacity;
        private Animation<Offset> _drawerDetailsPosition;
        private bool _showDrawerContents = true;

        public override void initState()
        {
            base.initState();
            this._controller = new AnimationController(
                vsync: this,
                duration: new TimeSpan(0, 0, 0, 0, 200)
            );
            this._drawerContentsOpacity = new CurvedAnimation(
                parent: new ReverseAnimation(this._controller),
                curve: Curves.fastOutSlowIn
            );

            this._drawerDetailsPosition = this._controller.drive(_drawerDetailsTween);
        }

        public override void dispose()
        {
            this._controller.dispose();
            base.dispose();
        }

        private IconData _backIcon()
        {
            switch (Theme.of(this.context).platform)
            {
                case RuntimePlatform.IPhonePlayer:
                case RuntimePlatform.OSXEditor:
                case RuntimePlatform.OSXPlayer:
                    return Icons.arrow_back_ios;
                default:
                    return Icons.arrow_back_ios;
            }
        }

        private void _showNotImplementedMessage()
        {
            Navigator.pop<object>(this.context); // Dismiss the drawer.
            this._scaffoldKey.currentState.showSnackBar(new SnackBar(
                content: new Text("The drawer\"s items don\"t do anything")
            ));
        }

        public override Widget build(BuildContext context)
        {
            return new Scaffold(
                drawerDragStartBehavior: DragStartBehavior.down,
                key: this._scaffoldKey,
                appBar: new AppBar(
                    leading: new IconButton(
                        icon: new Icon(this._backIcon()),
                        alignment: Alignment.centerLeft,
                        tooltip: "Back",
                        onPressed: () => { Navigator.pop<object>(context); }
                    ),
                    title: new Text("Navigation drawer"),
                    actions: new List<Widget> {new MaterialDemoDocumentationButton(DrawerDemo.routeName)}
                ),
                drawer: new Drawer(
                    child: new Column(
                        children: new List<Widget>
                        {
                            new UserAccountsDrawerHeader(
                                accountName: new Text("Trevor Widget"),
                                accountEmail: new Text("trevor.widget@example.com"),
                                currentAccountPicture: new CircleAvatar(
                                    backgroundImage: new FileImage(
                                        DrawerDemoUtils._kAsset0
                                    )
                                ),
                                otherAccountsPictures: new List<Widget>
                                {
                                    new GestureDetector(
                                        dragStartBehavior: DragStartBehavior.down,
                                        onTap: () => { this._onOtherAccountsTap(context); },
                                        child: new CircleAvatar(
                                            backgroundImage: new FileImage(
                                                DrawerDemoUtils._kAsset1
                                            )
                                        )
                                    ),
                                    new GestureDetector(
                                        dragStartBehavior: DragStartBehavior.down,
                                        onTap: () => { this._onOtherAccountsTap(context); },
                                        child: new CircleAvatar(
                                            backgroundImage: new FileImage(
                                                DrawerDemoUtils._kAsset2
                                            )
                                        )
                                    )
                                },
                                margin: EdgeInsets.zero,
                                onDetailsPressed: () =>
                                {
                                    this._showDrawerContents = !this._showDrawerContents;
                                    if (this._showDrawerContents)
                                        this._controller.reverse();
                                    else
                                        this._controller.forward();
                                }
                            ),
                            MediaQuery.removePadding(
                                context: context,
                                // DrawerHeader consumes top MediaQuery padding.
                                removeTop: true,
                                child: new Expanded(
                                    child: new ListView(
                                        dragStartBehavior: DragStartBehavior.down,
                                        padding: EdgeInsets.only(top: 8.0f),
                                        children: new List<Widget>
                                        {
                                            new Stack(
                                                children: new List<Widget>
                                                {
                                                    // The initial contents of the drawer.
                                                    new FadeTransition(
                                                        opacity: this._drawerContentsOpacity,
                                                        child: new Column(
                                                            mainAxisSize: MainAxisSize.min,
                                                            crossAxisAlignment: CrossAxisAlignment.stretch,
                                                            children: _drawerContents.Select<string, Widget>(
                                                                (string id) =>
                                                                {
                                                                    return new ListTile(
                                                                        leading: new CircleAvatar(child: new Text(id)),
                                                                        title: new Text($"Drawer item {id}"),
                                                                        onTap: this._showNotImplementedMessage
                                                                    );
                                                                }).ToList()
                                                        )
                                                    ),
                                                    // The drawer"s "details" view.
                                                    new SlideTransition(
                                                        position: this._drawerDetailsPosition,
                                                        child: new FadeTransition(
                                                            opacity: new ReverseAnimation(this._drawerContentsOpacity),
                                                            child: new Column(
                                                                mainAxisSize: MainAxisSize.min,
                                                                crossAxisAlignment: CrossAxisAlignment.stretch,
                                                                children: new List<Widget>
                                                                {
                                                                    new ListTile(
                                                                        leading: new Icon(Icons.add),
                                                                        title: new Text("Add account"),
                                                                        onTap: this._showNotImplementedMessage
                                                                    ),
                                                                    new ListTile(
                                                                        leading: new Icon(Icons.settings),
                                                                        title: new Text("Manage accounts"),
                                                                        onTap: this._showNotImplementedMessage
                                                                    )
                                                                }
                                                            )
                                                        )
                                                    )
                                                }
                                            )
                                        }
                                    )
                                )
                            )
                        }
                    )
                ),
                body: new Center(
                    child: new InkWell(
                        onTap: () => { this._scaffoldKey.currentState.openDrawer(); },
                        child: new Column(
                            mainAxisSize: MainAxisSize.min,
                            children: new List<Widget>
                            {
                                new Container(
                                    width: 100.0f,
                                    height: 100.0f,
                                    decoration: new BoxDecoration(
                                        shape: BoxShape.circle,
                                        image: new DecorationImage(
                                            image: new FileImage(
                                                DrawerDemoUtils._kAsset0
                                            )
                                        )
                                    )
                                ),
                                new Padding(
                                    padding: EdgeInsets.only(top: 8.0f),
                                    child: new Text("Tap here to open the drawer",
                                        style: Theme.of(context).textTheme.subtitle1
                                    )
                                )
                            }
                        )
                    )
                )
            );
        }

        private void _onOtherAccountsTap(BuildContext context)
        {
            material_.showDialog<object>(
                context: context,
                builder: (BuildContext subContext) =>
                {
                    return new AlertDialog(
                        title: new Text("Account switching not implemented."),
                        actions: new List<Widget>
                        {
                            new FlatButton(
                                child: new Text("OK"),
                                onPressed: () => { Navigator.pop<object>(context); }
                            )
                        }
                    );
                }
            );
        }
    }
}