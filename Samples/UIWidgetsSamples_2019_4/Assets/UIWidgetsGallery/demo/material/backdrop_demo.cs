using System;
using System.Collections.Generic;
using System.Linq;
using uiwidgets;
using Unity.UIWidgets.animation;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.gestures;
using Unity.UIWidgets.material;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.rendering;
using Unity.UIWidgets.ui;
using Unity.UIWidgets.widgets;
using UnityEngine;
using Image = Unity.UIWidgets.widgets.Image;
using Material = Unity.UIWidgets.material.Material;

namespace UIWidgetsGallery.demo.material
{
    public class Category
    {
        public Category(string title = null, List<string> assets = null)
        {
            this.title = title;
            this.assets = assets;
        }

        public readonly string title;
        public readonly List<string> assets;

        public override string ToString()
        {
            return $"{this.GetType()}({this.title})";
        }

        public static readonly List<Category> allCategories = new List<Category>
        {
            new Category(
                title: "Accessories",
                assets: new List<string>
                {
                    "gallery/products/belt.png",
                    "gallery/products/earrings.png",
                    "gallery/products/backpack.png",
                    "gallery/products/hat.png",
                    "gallery/products/scarf.png",
                    "gallery/products/sunnies.png"
                }
            ),
            new Category(
                title: "Blue",
                assets: new List<string>
                {
                    "gallery/products/backpack.png",
                    "gallery/products/cup.png",
                    "gallery/products/napkins.png",
                    "gallery/products/top.png"
                }
            ),
            new Category(
                title: "Cold Weather",
                assets: new List<string>
                {
                    "gallery/products/jacket.png",
                    "gallery/products/jumper.png",
                    "gallery/products/scarf.png",
                    "gallery/products/sweater.png",
                    "gallery/products/sweats.png"
                }
            ),
            new Category(
                title: "Home",
                assets: new List<string>
                {
                    "gallery/products/cup.png",
                    "gallery/products/napkins.png",
                    "gallery/products/planters.png",
                    "gallery/products/table.png",
                    "gallery/products/teaset.png"
                }
            ),
            new Category(
                title: "Tops",
                assets: new List<string>
                {
                    "gallery/products/jumper.png",
                    "gallery/products/shirt.png",
                    "gallery/products/sweater.png",
                    "gallery/products/top.png"
                }
            ),
            new Category(
                title: "Everything",
                assets: new List<string>
                {
                    "gallery/products/backpack.png",
                    "gallery/products/belt.png",
                    "gallery/products/cup.png",
                    "gallery/products/dress.png",
                    "gallery/products/earrings.png",
                    "gallery/products/flatwear.png",
                    "gallery/products/hat.png",
                    "gallery/products/jacket.png",
                    "gallery/products/jumper.png",
                    "gallery/products/napkins.png",
                    "gallery/products/planters.png",
                    "gallery/products/scarf.png",
                    "gallery/products/shirt.png",
                    "gallery/products/sunnies.png",
                    "gallery/products/sweater.png",
                    "gallery/products/sweats.png",
                    "gallery/products/table.png",
                    "gallery/products/teaset.png",
                    "gallery/products/top.png"
                }
            )
        };
    }


    public class CategoryView : StatelessWidget
    {
        public CategoryView(Key key = null, Category category = null) : base(key: key)
        {
            this.category = category;
        }

        public readonly Category category;

        public override Widget build(BuildContext context)
        {
            ThemeData theme = Theme.of(context);
            return new Scrollbar(
                child: new ListView(
                    key: new PageStorageKey<Category>(this.category),
                    padding: EdgeInsets.symmetric(
                        vertical: 16.0f,
                        horizontal: 64.0f
                    ),
                    children: this.category.assets.Select<string, Widget>((string asset) =>
                    {
                        return new Column(
                            crossAxisAlignment: CrossAxisAlignment.stretch,
                            children: new List<Widget>
                            {
                                new Card(
                                    child: new Container(
                                        width: 144.0f,
                                        alignment: Alignment.center,
                                        child: new Column(
                                            children: new List<Widget>
                                            {
                                                Image.file(
                                                    asset,
                                                    fit: BoxFit.contain
                                                ),
                                                new Container(
                                                    padding: EdgeInsets.only(bottom: 16.0f),
                                                    alignment: AlignmentDirectional.center,
                                                    child: new Text(
                                                        asset,
                                                        style: theme.textTheme.caption
                                                    )
                                                )
                                            }
                                        )
                                    )
                                ),
                                new SizedBox(height: 24.0f)
                            }
                        );
                    }).ToList()
                )
            );
        }
    }

    internal class BackdropPanel : StatelessWidget
    {
        public BackdropPanel(
            Key key = null,
            VoidCallback onTap = null,
            GestureDragUpdateCallback onVerticalDragUpdate = null,
            GestureDragEndCallback onVerticalDragEnd = null,
            Widget title = null,
            Widget child = null
        ) : base(key: key)
        {
            this.onTap = onTap;
            this.onVerticalDragUpdate = onVerticalDragUpdate;
            this.onVerticalDragEnd = onVerticalDragEnd;
            this.title = title;
            this.child = child;
        }

        public readonly VoidCallback onTap;
        public readonly GestureDragUpdateCallback onVerticalDragUpdate;
        public readonly GestureDragEndCallback onVerticalDragEnd;
        public readonly Widget title;
        public readonly Widget child;

        public override Widget build(BuildContext context)
        {
            ThemeData theme = Theme.of(context);
            return new Material(
                elevation: 2.0f,
                borderRadius: BorderRadius.only(
                    topLeft: Radius.circular(16.0f),
                    topRight: Radius.circular(16.0f)
                ),
                child: new Column(
                    crossAxisAlignment: CrossAxisAlignment.stretch,
                    children: new List<Widget>
                    {
                        new GestureDetector(
                            behavior: HitTestBehavior.opaque,
                            onVerticalDragUpdate: this.onVerticalDragUpdate,
                            onVerticalDragEnd: this.onVerticalDragEnd,
                            onTap: () => { this.onTap?.Invoke(); },
                            child: new Container(
                                height: 48.0f,
                                padding: EdgeInsetsDirectional.only(start: 16.0f),
                                alignment: AlignmentDirectional.centerStart,
                                child: new DefaultTextStyle(
                                    style: theme.textTheme.subtitle1,
                                    child: new Tooltip(
                                        message: "Tap to dismiss",
                                        child: this.title
                                    )
                                )
                            )
                        ),
                        new Divider(height: 1.0f),
                        new Expanded(child: this.child)
                    }
                )
            );
        }
    }

    internal class BackdropTitle : AnimatedWidget
    {
        public BackdropTitle(
            Key key = null,
            Animation<float> listenable = null
        ) : base(key: key, listenable: listenable)
        {
        }

        protected override Widget build(BuildContext context)
        {
            Animation<float> animation = this.listenable as Animation<float>;
            return new DefaultTextStyle(
                style: Theme.of(context).primaryTextTheme.headline6,
                softWrap: false,
                overflow: TextOverflow.ellipsis,
                child: new Stack(
                    children: new List<Widget>
                    {
                        new Opacity(
                            opacity: new CurvedAnimation(
                                parent: new ReverseAnimation(animation),
                                curve: new Interval(0.5f, 1.0f)
                            ).value,
                            child: new Text("Select a Category")
                        ),
                        new Opacity(
                            opacity: new CurvedAnimation(
                                parent: animation,
                                curve: new Interval(0.5f, 1.0f)
                            ).value,
                            child: new Text("Asset Viewer")
                        )
                    }
                )
            );
        }
    }

    internal class BackdropDemo : StatefulWidget
    {
        public static readonly string routeName = "/material/backdrop";

        public override State createState()
        {
            return new _BackdropDemoState();
        }
    }

    internal class _BackdropDemoState : SingleTickerProviderStateMixin<BackdropDemo>
    {
        private GlobalKey _backdropKey = GlobalKey.key(debugLabel: "Backdrop");
        private AnimationController _controller;
        private Category _category = Category.allCategories[0];


        public override void initState()
        {
            base.initState();
            this._controller = new AnimationController(
                duration: new TimeSpan(0, 0, 0, 0, 300),
                value: 1.0f,
                vsync: this
            );
        }

        public override void dispose()
        {
            this._controller.dispose();
            base.dispose();
        }

        private void _changeCategory(Category category)
        {
            this.setState(() =>
            {
                this._category = category;
                this._controller.fling(velocity: 2.0f);
            });
        }

        private bool _backdropPanelVisible
        {
            get
            {
                AnimationStatus status = this._controller.status;
                return status == AnimationStatus.completed || status == AnimationStatus.forward;
            }
        }

        private void _toggleBackdropPanelVisibility()
        {
            this._controller.fling(velocity: this._backdropPanelVisible ? -2.0f : 2.0f);
        }

        private float _backdropHeight
        {
            get
            {
                RenderBox renderBox = this._backdropKey.currentContext.findRenderObject() as RenderBox;
                return renderBox.size.height;
            }
        }

        // By design: the panel can only be opened with a swipe. To close the panel
        // the user must either tap its heading or the backdrop's menu icon.

        private void _handleDragUpdate(DragUpdateDetails details)
        {
            if (this._controller.isAnimating || this._controller.status == AnimationStatus.completed)
                return;

            this._controller.setValue(this._controller.value - details.primaryDelta.Value / (this._backdropHeight));
        }

        private void _handleDragEnd(DragEndDetails details)
        {
            if (this._controller.isAnimating || this._controller.status == AnimationStatus.completed)
                return;

            float flingVelocity = details.velocity.pixelsPerSecond.dy / this._backdropHeight;
            if (flingVelocity < 0.0f)
                this._controller.fling(velocity: Mathf.Max(2.0f, -flingVelocity));
            else if (flingVelocity > 0.0f)
                this._controller.fling(velocity: Mathf.Min(-2.0f, -flingVelocity));
            else
                this._controller.fling(velocity: this._controller.value < 0.5 ? -2.0f : 2.0f);
        }

        // Stacks a BackdropPanel, which displays the selected category, on top
        // of the backdrop. The categories are displayed with ListTiles. Just one
        // can be selected at a time. This is a LayoutWidgetBuild function because
        // we need to know how big the BackdropPanel will be to set up its
        // animation.
        private Widget _buildStack(BuildContext context, BoxConstraints constraints)
        {
            float panelTitleHeight = 48.0f;
            Size panelSize = constraints.biggest;
            float panelTop = panelSize.height - panelTitleHeight;

            Animation<RelativeRect> panelAnimation = this._controller.drive(
                new RelativeRectTween(
                    begin: RelativeRect.fromLTRB(
                        0.0f,
                        panelTop - MediaQuery.of(context).padding.bottom,
                        0.0f,
                        panelTop - panelSize.height
                    ),
                    end: RelativeRect.fromLTRB(0.0f, 0.0f, 0.0f, 0.0f)
                )
            );

            ThemeData theme = Theme.of(context);
            List<Widget> backdropItems = Category.allCategories.Select<Category, Widget>((Category category) =>
            {
                bool selected = category == this._category;
                return new Material(
                    shape: new RoundedRectangleBorder(
                        borderRadius: BorderRadius.all(Radius.circular(4.0f))
                    ),
                    color: selected
                        ? Colors.white.withOpacity(0.25f)
                        : Colors.transparent,
                    child: new ListTile(
                        title: new Text(category.title),
                        selected: selected,
                        onTap: () => { this._changeCategory(category); }
                    )
                );
            }).ToList();

            return new Container(
                key: this._backdropKey,
                color: theme.primaryColor,
                child: new Stack(
                    children: new List<Widget>
                    {
                        new ListTileTheme(
                            iconColor: theme.primaryIconTheme.color,
                            textColor: theme.primaryTextTheme.headline6.color.withOpacity(0.6f),
                            selectedColor: theme.primaryTextTheme.headline6.color,
                            child: new Padding(
                                padding: EdgeInsets.symmetric(horizontal: 16.0f),
                                child: new Column(
                                    crossAxisAlignment: CrossAxisAlignment.stretch,
                                    children: backdropItems
                                )
                            )
                        ),
                        new PositionedTransition(
                            rect: panelAnimation,
                            child: new BackdropPanel(
                                onTap: this._toggleBackdropPanelVisibility,
                                onVerticalDragUpdate: this._handleDragUpdate,
                                onVerticalDragEnd: this._handleDragEnd,
                                title: new Text(this._category.title),
                                child: new CategoryView(category: this._category)
                            )
                        )
                    }
                )
            );
        }

        public override Widget build(BuildContext context)
        {
            return new Scaffold(
                appBar: new AppBar(
                    elevation: 0.0f,
                    title: new BackdropTitle(
                        listenable: this._controller.view
                    ),
                    actions: new List<Widget>
                    {
                        new IconButton(
                            onPressed: this._toggleBackdropPanelVisibility,
                            icon: new AnimatedIcon(
                                icon: AnimatedIcons.close_menu,
                                progress: this._controller.view
                            )
                        )
                    }
                ),
                body: new LayoutBuilder(
                    builder: this._buildStack
                )
            );
        }
    }
}