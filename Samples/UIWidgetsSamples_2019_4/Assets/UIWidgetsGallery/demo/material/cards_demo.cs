using System.Collections.Generic;
using System.Linq;
using uiwidgets;
using UIWidgetsGallery.gallery;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.material;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.rendering;
using Unity.UIWidgets.ui;
using Unity.UIWidgets.widgets;
using UnityEngine;
using TextStyle = Unity.UIWidgets.painting.TextStyle;

namespace UIWidgetsGallery.demo.material
{
    public static class CardsDemoUtils
    {
        public static readonly string _kGalleryAssetsPackage = "gallery/";
    }

    internal enum CardDemoType
    {
        standard,
        tappable,
        selectable,
    }

    internal class TravelDestination
    {
        public TravelDestination(
            string assetName = null,
            string assetPackage = null,
            string title = null,
            string description = null,
            string city = null,
            string location = null,
            CardDemoType type = CardDemoType.standard
        )
        {
            D.assert(assetName != null);
            D.assert(assetPackage != null);
            D.assert(title != null);
            D.assert(description != null);
            D.assert(city != null);
            D.assert(location != null);

            this.assetName = assetName;
            this.assetPackage = assetPackage;
            this.title = title;
            this.description = description;
            this.city = city;
            this.location = location;
            this.type = type;
        }

        public readonly string assetName;
        public readonly string assetPackage;
        public readonly string title;
        public readonly string description;
        public readonly string city;
        public readonly string location;
        public readonly CardDemoType type;

        public static readonly List<TravelDestination> destinations = new List<TravelDestination>
        {
            new TravelDestination(
                assetName: "places/india_thanjavur_market.png",
                assetPackage: CardsDemoUtils._kGalleryAssetsPackage,
                title: "Top 10 Cities to Visit in Tamil Nadu",
                description: "Number 10",
                city: "Thanjavur",
                location: "Thanjavur, Tamil Nadu"
            ),
            new TravelDestination(
                assetName: "places/india_chettinad_silk_maker.png",
                assetPackage: CardsDemoUtils._kGalleryAssetsPackage,
                title: "Artisans of Southern India",
                description: "Silk Spinners",
                city: "Chettinad",
                location: "Sivaganga, Tamil Nadu",
                type: CardDemoType.tappable
            ),
            new TravelDestination(
                assetName: "places/india_tanjore_thanjavur_temple.png",
                assetPackage: CardsDemoUtils._kGalleryAssetsPackage,
                title: "Brihadisvara Temple",
                description: "Temples",
                city: "Thanjavur",
                location: "Thanjavur, Tamil Nadu",
                type: CardDemoType.selectable
            ),
        };
    }

    internal class TravelDestinationItem : StatelessWidget
    {
        public TravelDestinationItem(Key key = null, TravelDestination destination = null, ShapeBorder shape = null)
            : base(key: key)
        {
            D.assert(destination != null);
            this.destination = destination;
            this.shape = shape;
        }

        // This height will allow for all the Card's content to fit comfortably within the card.
        private const float height = 338.0f;
        public readonly TravelDestination destination;
        public readonly ShapeBorder shape;

        public override Widget build(BuildContext context)
        {
            return new SafeArea(
                top: false,
                bottom: false,
                child: new Padding(
                    padding: EdgeInsets.all(8.0f),
                    child: new Column(
                        children: new List<Widget>
                        {
                            new SectionTitle(title: "Normal"),
                            new SizedBox(
                                height: height,
                                child: new Card(
                                    // This ensures that the Card's children are clipped correctly.
                                    clipBehavior: Clip.antiAlias,
                                    shape: this.shape,
                                    child: new TravelDestinationContent(destination: this.destination)
                                )
                            )
                        }
                    )
                )
            );
        }
    }

    internal class TappableTravelDestinationItem : StatelessWidget
    {
        public TappableTravelDestinationItem(Key key = null, TravelDestination destination = null,
            ShapeBorder shape = null)
            : base(key: key)
        {
            D.assert(destination != null);
            this.destination = destination;
            this.shape = shape;
        }

        // This height will allow for all the Card's content to fit comfortably within the card.
        private const float height = 298.0f;
        public readonly TravelDestination destination;
        public readonly ShapeBorder shape;

        public override Widget build(BuildContext context)
        {
            return new SafeArea(
                top: false,
                bottom: false,
                child: new Padding(
                    padding: EdgeInsets.all(8.0f),
                    child: new Column(
                        children: new List<Widget>
                        {
                            new SectionTitle(title: "Tappable"),
                            new SizedBox(
                                height: height,
                                child: new Card(
                                    // This ensures that the Card's children (including the ink splash) are clipped correctly.
                                    clipBehavior: Clip.antiAlias,
                                    shape: this.shape,
                                    child: new InkWell(
                                        onTap: () => { Debug.Log("Card was tapped"); },
                                        // Generally, material cards use onSurface with 12% opacity for the pressed state.
                                        splashColor: Theme.of(context).colorScheme.onSurface.withOpacity(0.12f),
                                        // Generally, material cards do not have a highlight overlay.
                                        highlightColor: Colors.transparent,
                                        child: new TravelDestinationContent(destination: this.destination)
                                    )
                                )
                            )
                        }
                    )
                )
            );
        }
    }

    internal class SelectableTravelDestinationItem : StatefulWidget
    {
        public SelectableTravelDestinationItem(Key key = null, TravelDestination destination = null,
            ShapeBorder shape = null)
            : base(key: key)
        {
            D.assert(destination != null);
            this.destination = destination;
            this.shape = shape;
        }

        public readonly TravelDestination destination;
        public readonly ShapeBorder shape;


        public override State createState()
        {
            return new _SelectableTravelDestinationItemState();
        }
    }

    internal class _SelectableTravelDestinationItemState : State<SelectableTravelDestinationItem>
    {
        // This height will allow for all the Card's content to fit comfortably within the card.
        private const float height = 298.0f;
        private bool _isSelected = false;

        public override Widget build(BuildContext context)
        {
            ColorScheme colorScheme = Theme.of(context).colorScheme;

            return new SafeArea(
                top: false,
                bottom: false,
                child: new Padding(
                    padding: EdgeInsets.all(8.0f),
                    child: new Column(
                        children: new List<Widget>
                        {
                            new SectionTitle(title: "Selectable (long press)"),
                            new SizedBox(
                                height: height,
                                child: new Card(
                                    // This ensures that the Card's children (including the ink splash) are clipped correctly.
                                    clipBehavior: Clip.antiAlias,
                                    shape: this.widget.shape,
                                    child: new InkWell(
                                        onLongPress: () =>
                                        {
                                            Debug.Log("Selectable card state changed");
                                            this.setState(() => { this._isSelected = !this._isSelected; });
                                        },
                                        // Generally, material cards use onSurface with 12% opacity for the pressed state.
                                        splashColor: colorScheme.onSurface.withOpacity(0.12f),
                                        // Generally, material cards do not have a highlight overlay.
                                        highlightColor: Colors.transparent,
                                        child: new Stack(
                                            children: new List<Widget>
                                            {
                                                new Container(
                                                    color: this._isSelected
                                                        // Generally, material cards use primary with 8% opacity for the selected state.
                                                        // See: https://material.io/design/interaction/states.html#anatomy
                                                        ? colorScheme.primary.withOpacity(0.08f)
                                                        : Colors.transparent
                                                ),
                                                new TravelDestinationContent(destination: this.widget.destination),
                                                new Align(
                                                    alignment: Alignment.topRight,
                                                    child: new Padding(
                                                        padding: EdgeInsets.all(8.0f),
                                                        child: new Icon(
                                                            Icons.check_circle,
                                                            color: this._isSelected
                                                                ? colorScheme.primary
                                                                : Colors.transparent
                                                        )
                                                    )
                                                )
                                            }
                                        )
                                    )
                                )
                            )
                        }
                    )
                )
            );
        }
    }

    internal class SectionTitle : StatelessWidget
    {
        public SectionTitle(
            Key key = null,
            string title = null
        ) : base(key: key)
        {
            this.title = title;
        }

        public readonly string title;


        public override Widget build(BuildContext context)
        {
            return new Padding(
                padding: EdgeInsets.fromLTRB(4.0f, 4.0f, 4.0f, 12.0f),
                child: new Align(
                    alignment: Alignment.centerLeft,
                    child: new Text(this.title, style: Theme.of(context).textTheme.subtitle1)
                )
            );
        }
    }

    internal class TravelDestinationContent : StatelessWidget
    {
        public TravelDestinationContent(Key key = null, TravelDestination destination = null)
            : base(key: key)
        {
            D.assert(destination != null);
            this.destination = destination;
        }

        public readonly TravelDestination destination;


        public override Widget build(BuildContext context)
        {
            ThemeData theme = Theme.of(context);
            TextStyle titleStyle = theme.textTheme.headline5.copyWith(color: Colors.white);
            TextStyle descriptionStyle = theme.textTheme.subtitle1;

            var children = new List<Widget>
            {
                // Photo and title.
                new SizedBox(
                    height: 184.0f,
                    child: new Stack(
                        children: new List<Widget>
                        {
                            Positioned.fill(
                                // In order to have the ink splash appear above the image, you
                                // must use Ink.image. This allows the image to be painted as part
                                // of the Material and display ink effects above it. Using a
                                // standard Image will obscure the ink splash.
                                child: Ink.image(
                                    image: new FileImage(System.IO.Path.Combine(destination.assetPackage,
                                                         destination.assetName)),
                                    fit: BoxFit.cover,
                                    child: new Container()
                                )
                            ),
                            new Positioned(
                                bottom: 16.0f,
                                left: 16.0f,
                                right: 16.0f,
                                child: new FittedBox(
                                    fit: BoxFit.scaleDown,
                                    alignment: Alignment.centerLeft,
                                    child: new Text(this.destination.title,
                                        style: titleStyle
                                    )
                                )
                            )
                        }
                    )
                ),
                // Description and share/explore buttons.
                new Padding(
                    padding: EdgeInsets.fromLTRB(16.0f, 16.0f, 16.0f, 0.0f),
                    child: new DefaultTextStyle(
                        softWrap: false,
                        overflow: TextOverflow.ellipsis,
                        style: descriptionStyle,
                        child: new Column(
                            crossAxisAlignment: CrossAxisAlignment.start,
                            children: new List<Widget>
                            {
                                // three line description
                                new Padding(
                                    padding: EdgeInsets.only(bottom: 8.0f),
                                    child: new Text(this.destination.description,
                                        style: descriptionStyle.copyWith(color: Colors.black54)
                                    )
                                ),
                                new Text(this.destination.city),
                                new Text(this.destination.location)
                            }
                        )
                    )
                )
            };

            if (this.destination.type == CardDemoType.standard)
                // share, explore buttons
                children.Add(new ButtonBar(
                    alignment: MainAxisAlignment.start,
                    children: new List<Widget>
                    {
                        new FlatButton(
                            child: new Text("SHARE"),
                            textColor: Colors.amber.shade500,
                            onPressed: () => { Debug.Log("pressed"); }
                        ),
                        new FlatButton(
                            child: new Text("EXPLORE"),
                            textColor: Colors.amber.shade500,
                            onPressed: () => { Debug.Log("pressed"); }
                        )
                    }
                ));

            return new Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: children
            );
        }
    }

    internal class CardsDemo : StatefulWidget
    {
        public static readonly string routeName = "/material/cards";

        public override State createState()
        {
            return new _CardsDemoState();
        }
    }

    internal class _CardsDemoState : State<CardsDemo>
    {
        private ShapeBorder _shape;

        public override Widget build(BuildContext context)
        {
            return new Scaffold(
                appBar: new AppBar(
                    title: new Text("Cards"),
                    actions: new List<Widget>
                    {
                        new MaterialDemoDocumentationButton(CardsDemo.routeName),
                        new IconButton(
                            icon: new Icon(
                                Icons.sentiment_very_satisfied
                            ),
                            onPressed: () =>
                            {
                                this.setState(() =>
                                {
                                    this._shape = this._shape != null
                                        ? null
                                        : new RoundedRectangleBorder(
                                            borderRadius: BorderRadius.only(
                                                topLeft: Radius.circular(16.0f),
                                                topRight: Radius.circular(16.0f),
                                                bottomLeft: Radius.circular(2.0f),
                                                bottomRight: Radius.circular(2.0f)
                                            )
                                        );
                                });
                            }
                        )
                    }
                ),
                body: new Scrollbar(
                    child: new ListView(
                        padding: EdgeInsets.only(top: 8.0f, left: 8.0f, right: 8.0f),
                        children: TravelDestination.destinations.Select<TravelDestination, Widget>(
                            (TravelDestination destination) =>
                            {
                                Widget child = null;
                                switch (destination.type)
                                {
                                    case CardDemoType.standard:
                                        child = new TravelDestinationItem(destination: destination, shape: this._shape);
                                        break;
                                    case CardDemoType.tappable:
                                        child = new TappableTravelDestinationItem(destination: destination,
                                            shape: this._shape);
                                        break;
                                    case CardDemoType.selectable:
                                        child = new SelectableTravelDestinationItem(destination: destination,
                                            shape: this._shape);
                                        break;
                                }

                                return new Container(
                                    margin: EdgeInsets.only(bottom: 8.0f),
                                    child: child
                                );
                            }).ToList()
                    )
                )
            );
        }
    }
}