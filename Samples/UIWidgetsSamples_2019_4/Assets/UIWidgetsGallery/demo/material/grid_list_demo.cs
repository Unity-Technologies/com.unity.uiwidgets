using System.Collections.Generic;
using System.Linq;
using uiwidgets;
using UIWidgetsGallery.gallery;
using Unity.UIWidgets.animation;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.gestures;
using Unity.UIWidgets.material;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.ui;
using Unity.UIWidgets.widgets;
using Image = Unity.UIWidgets.widgets.Image;

namespace UIWidgetsGallery.demo.material
{
    internal enum GridDemoTileStyle
    {
        imageOnly,
        oneLine,
        twoLine
    }

    internal delegate void BannerTapCallback(Photo photo);

    internal static class GridListDemoUtils
    {
        public const float _kMinFlingVelocity = 800.0f;
        public static readonly string _kGalleryAssetsPackage = "gallery/";
    }

    internal class Photo
    {
        public Photo(
            string assetName = null,
            string assetPackage = null,
            string title = null,
            string caption = null,
            bool isFavorite = false
        )
        {
            this.assetName = assetName;
            this.assetPackage = assetPackage;
            this.title = title;
            this.caption = caption;
            this.isFavorite = isFavorite;
        }

        public readonly string assetName;
        public readonly string assetPackage;
        public readonly string title;
        public readonly string caption;

        internal bool isFavorite;
        internal string tag => assetName; // Assuming that all asset names are unique.

        internal bool isValid => assetName != null && title != null && caption != null;
    }

    internal class GridPhotoViewer : StatefulWidget
    {
        public GridPhotoViewer(
            Key key = null,
            Photo photo = null) : base(key: key)
        {
            this.photo = photo;
        }

        public readonly Photo photo;

        public override State createState()
        {
            return new _GridPhotoViewerState();
        }
    }

    internal class _GridTitleText : StatelessWidget
    {
        public _GridTitleText(string text)
        {
            this.text = text;
        }

        public readonly string text;


        public override Widget build(BuildContext context)
        {
            return new FittedBox(
                fit: BoxFit.scaleDown,
                alignment: Alignment.centerLeft,
                child: new Text(text)
            );
        }
    }

    internal class _GridPhotoViewerState : SingleTickerProviderStateMixin<GridPhotoViewer>
    {
        private AnimationController _controller;
        private Animation<Offset> _flingAnimation;
        private Offset _offset = Offset.zero;
        private float _scale = 1.0f;
        private Offset _normalizedOffset;
        private float _previousScale;


        public override void initState()
        {
            base.initState();
            _controller = new AnimationController(vsync: this);
            _controller.addListener(_handleFlingAnimation);
        }


        public override void dispose()
        {
            _controller.dispose();
            base.dispose();
        }

        private Offset _clampOffset(Offset offset)
        {
            Size size = context.size;
            Offset minOffset = new Offset(size.width, size.height) * (1.0f - _scale);
            return new Offset(
                offset.dx.clamp(minOffset.dx, 0.0f),
                offset.dy.clamp(minOffset.dy, 0.0f)
            );
        }

        private void _handleFlingAnimation()
        {
            setState(() => { _offset = _flingAnimation.value; });
        }

        private void _handleOnScaleStart(ScaleStartDetails details)
        {
            setState(() =>
            {
                _previousScale = _scale;
                _normalizedOffset = (details.focalPoint - _offset) / _scale;
                _controller.stop();
            });
        }

        private void _handleOnScaleUpdate(ScaleUpdateDetails details)
        {
            setState(() =>
            {
                _scale = (_previousScale * details.scale).clamp(1.0f, 4.0f);
                _offset = _clampOffset(details.focalPoint - _normalizedOffset * _scale);
            });
        }

        private void _handleOnScaleEnd(ScaleEndDetails details)
        {
            float magnitude = details.velocity.pixelsPerSecond.distance;
            if (magnitude < GridListDemoUtils._kMinFlingVelocity)
                return;
            Offset direction = details.velocity.pixelsPerSecond / magnitude;
            float distance = (Offset.zero & context.size).shortestSide;
            _flingAnimation = _controller.drive(new OffsetTween(
                begin: _offset,
                end: _clampOffset(_offset + direction * distance)
            ));
            _controller.setValue(0.0f);
            _controller.fling(velocity: magnitude / 1000.0f);
        }


        public override Widget build(BuildContext context)
        {
            var transform = Matrix4.identity();
            transform.translate(_offset.dx, _offset.dy);
            transform.scale(_scale);
            return new GestureDetector(
                onScaleStart: _handleOnScaleStart,
                onScaleUpdate: _handleOnScaleUpdate,
                onScaleEnd: _handleOnScaleEnd,
                child: new ClipRect(
                    child: new Transform(
                        transform: transform,
                        child: Image.file(
                            widget.photo.assetPackage + widget.photo.assetName,
                            fit: BoxFit.cover
                        )
                    )
                )
            );
        }
    }

    internal class GridDemoPhotoItem : StatelessWidget
    {
        public GridDemoPhotoItem(
            Key key = null,
            Photo photo = null,
            GridDemoTileStyle? tileStyle = null,
            BannerTapCallback onBannerTap = null
        ) : base(key: key)
        {
            D.assert(photo != null && photo.isValid);
            D.assert(tileStyle != null);
            D.assert(onBannerTap != null);

            this.photo = photo;
            this.tileStyle = tileStyle.Value;
            this.onBannerTap = onBannerTap;
        }

        public readonly Photo photo;
        public readonly GridDemoTileStyle tileStyle;
        public readonly BannerTapCallback onBannerTap; // User taps on the photo's header or footer.

        private void showPhoto(BuildContext context)
        {
            Navigator.push<object>(context, new MaterialPageRoute(
                builder: (BuildContext subContext) =>
                {
                    return new Scaffold(
                        appBar: new AppBar(
                            title: new Text(photo.title)
                        ),
                        body: SizedBox.expand(
                            child: new Hero(
                                tag: photo.tag,
                                child: new GridPhotoViewer(photo: photo)
                            )
                        )
                    );
                }
            ));
        }

        public override Widget build(BuildContext context)
        {
            Widget image = new GestureDetector(
                onTap: () => { showPhoto(context); },
                child: new Hero(
                    key: Key.key(photo.assetName),
                    tag: photo.tag,
                    child: Image.file(
                        photo.assetPackage + photo.assetName,
                        fit: BoxFit.cover
                    )
                )
            );

            IconData icon = photo.isFavorite ? Icons.star : Icons.star_border;

            switch (tileStyle)
            {
                case GridDemoTileStyle.imageOnly:
                    return image;

                case GridDemoTileStyle.oneLine:
                    return new GridTile(
                        header: new GestureDetector(
                            onTap: () => { onBannerTap(photo); },
                            child: new GridTileBar(
                                title: new _GridTitleText(photo.title),
                                backgroundColor: Colors.black45,
                                leading: new Icon(
                                    icon,
                                    color: Colors.white
                                )
                            )
                        ),
                        child: image
                    );

                case GridDemoTileStyle.twoLine:
                    return new GridTile(
                        footer: new GestureDetector(
                            onTap: () => { onBannerTap(photo); },
                            child: new GridTileBar(
                                backgroundColor: Colors.black45,
                                title: new _GridTitleText(photo.title),
                                subtitle: new _GridTitleText(photo.caption),
                                trailing: new Icon(
                                    icon,
                                    color: Colors.white
                                )
                            )
                        ),
                        child: image
                    );
            }
            return null;
        }
    }

    internal class GridListDemo : StatefulWidget
    {
        public GridListDemo(Key key = null) : base(key: key)
        {
        }

        public static readonly string routeName = "/material/grid-list";


        public override State createState()
        {
            return new GridListDemoState();
        }
    }

    internal class GridListDemoState : State<GridListDemo>
    {
        private GridDemoTileStyle _tileStyle = GridDemoTileStyle.twoLine;

        private List<Photo> photos = new List<Photo>
        {
            new Photo(
                assetName: "places/india_chennai_flower_market.png",
                assetPackage: GridListDemoUtils._kGalleryAssetsPackage,
                title: "Chennai",
                caption: "Flower Market"
            ),
            new Photo(
                assetName: "places/india_tanjore_bronze_works.png",
                assetPackage: GridListDemoUtils._kGalleryAssetsPackage,
                title: "Tanjore",
                caption: "Bronze Works"
            ),
            new Photo(
                assetName: "places/india_tanjore_market_merchant.png",
                assetPackage: GridListDemoUtils._kGalleryAssetsPackage,
                title: "Tanjore",
                caption: "Market"
            ),
            new Photo(
                assetName: "places/india_tanjore_thanjavur_temple.png",
                assetPackage: GridListDemoUtils._kGalleryAssetsPackage,
                title: "Tanjore",
                caption: "Thanjavur Temple"
            ),
            new Photo(
                assetName: "places/india_tanjore_thanjavur_temple_carvings.png",
                assetPackage: GridListDemoUtils._kGalleryAssetsPackage,
                title: "Tanjore",
                caption: "Thanjavur Temple"
            ),
            new Photo(
                assetName: "places/india_pondicherry_salt_farm.png",
                assetPackage: GridListDemoUtils._kGalleryAssetsPackage,
                title: "Pondicherry",
                caption: "Salt Farm"
            ),
            new Photo(
                assetName: "places/india_chennai_highway.png",
                assetPackage: GridListDemoUtils._kGalleryAssetsPackage,
                title: "Chennai",
                caption: "Scooters"
            ),
            new Photo(
                assetName: "places/india_chettinad_silk_maker.png",
                assetPackage: GridListDemoUtils._kGalleryAssetsPackage,
                title: "Chettinad",
                caption: "Silk Maker"
            ),
            new Photo(
                assetName: "places/india_chettinad_produce.png",
                assetPackage: GridListDemoUtils._kGalleryAssetsPackage,
                title: "Chettinad",
                caption: "Lunch Prep"
            ),
            new Photo(
                assetName: "places/india_tanjore_market_technology.png",
                assetPackage: GridListDemoUtils._kGalleryAssetsPackage,
                title: "Tanjore",
                caption: "Market"
            ),
            new Photo(
                assetName: "places/india_pondicherry_beach.png",
                assetPackage: GridListDemoUtils._kGalleryAssetsPackage,
                title: "Pondicherry",
                caption: "Beach"
            ),
            new Photo(
                assetName: "places/india_pondicherry_fisherman.png",
                assetPackage: GridListDemoUtils._kGalleryAssetsPackage,
                title: "Pondicherry",
                caption: "Fisherman"
            )
        };

        private void changeTileStyle(GridDemoTileStyle value)
        {
            setState(() => { _tileStyle = value; });
        }


        public override Widget build(BuildContext context)
        {
            Orientation? orientation = MediaQuery.of(context).orientation;
            return new Scaffold(
                appBar: new AppBar(
                    title: new Text("Grid list"),
                    actions: new List<Widget>
                    {
                        new MaterialDemoDocumentationButton(GridListDemo.routeName),
                        new PopupMenuButton<GridDemoTileStyle>(
                            onSelected: changeTileStyle,
                            itemBuilder: (BuildContext subContext) => new List<PopupMenuEntry<GridDemoTileStyle>>
                            {
                                new PopupMenuItem<GridDemoTileStyle>(
                                    value: GridDemoTileStyle.imageOnly,
                                    child: new Text("Image only")
                                ),
                                new PopupMenuItem<GridDemoTileStyle>(
                                    value: GridDemoTileStyle.oneLine,
                                    child: new Text("One line")
                                ),
                                new PopupMenuItem<GridDemoTileStyle>(
                                    value: GridDemoTileStyle.twoLine,
                                    child: new Text("Two line")
                                )
                            }
                        )
                    }
                ),
                body: new Column(
                    children: new List<Widget>
                    {
                        new Expanded(
                            child: new SafeArea(
                                top: false,
                                bottom: false,
                                child: GridView.count(
                                    crossAxisCount: (orientation == Orientation.portrait) ? 2 : 3,
                                    mainAxisSpacing: 4.0f,
                                    crossAxisSpacing: 4.0f,
                                    padding: EdgeInsets.all(4.0f),
                                    childAspectRatio: (orientation == Orientation.portrait) ? 1.0f : 1.3f,
                                    children: photos.Select<Photo, Widget>((Photo photo) =>
                                    {
                                        return new GridDemoPhotoItem(
                                            photo: photo,
                                            tileStyle: _tileStyle,
                                            onBannerTap: (Photo curPhoto) =>
                                            {
                                                setState(() => { curPhoto.isFavorite = !curPhoto.isFavorite; });
                                            }
                                        );
                                    }).ToList()
                                )
                            )
                        )
                    }
                )
            );
        }
    }
}