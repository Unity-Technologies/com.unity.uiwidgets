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
using Canvas = Unity.UIWidgets.ui.Canvas;
using Color = Unity.UIWidgets.ui.Color;
using Material = Unity.UIWidgets.material.Material;
using Rect = Unity.UIWidgets.ui.Rect;

namespace UIWidgetsGallery.demo.material
{
    internal class BottomAppBarDemo : StatefulWidget
    {
        public static readonly string routeName = "/material/bottom_app_bar";

        public override State createState()
        {
            return new _BottomAppBarDemoState();
        }
    }

    internal class _BottomAppBarDemoState : State<BottomAppBarDemo>
    {
        private static GlobalKey<ScaffoldState> _scaffoldKey = GlobalKey<ScaffoldState>.key();

        // FAB shape

        private static readonly _ChoiceValue<Widget> kNoFab = new _ChoiceValue<Widget>(
            title: "None",
            label: "do not show a floating action button",
            value: null
        );

        private static readonly _ChoiceValue<Widget> kCircularFab = new _ChoiceValue<Widget>(
            title: "Circular",
            label: "circular floating action button",
            value: new FloatingActionButton(
                onPressed: _showSnackbar,
                child: new Icon(Icons.add),
                backgroundColor: Colors.orange
            )
        );

        private static readonly _ChoiceValue<Widget> kDiamondFab = new _ChoiceValue<Widget>(
            title: "Diamond",
            label: "diamond shape floating action button",
            value: new _DiamondFab(
                onPressed: _showSnackbar,
                child: new Icon(Icons.add)
            )
        );

        // Notch
        private static readonly _ChoiceValue<bool> kShowNotchTrue = new _ChoiceValue<bool>(
            title: "On",
            label: "show bottom appbar notch",
            value: true
        );

        private static readonly _ChoiceValue<bool> kShowNotchFalse = new _ChoiceValue<bool>(
            title: "Off",
            label: "do not show bottom appbar notch",
            value: false
        );

        // FAB Position

        private static readonly _ChoiceValue<FloatingActionButtonLocation> kFabEndDocked =
            new _ChoiceValue<FloatingActionButtonLocation>(
                title: "Attached - End",
                label: "floating action button is docked at the end of the bottom app bar",
                value: FloatingActionButtonLocation.endDocked
            );

        private static readonly _ChoiceValue<FloatingActionButtonLocation> kFabCenterDocked =
            new _ChoiceValue<FloatingActionButtonLocation>(
                title: "Attached - Center",
                label: "floating action button is docked at the center of the bottom app bar",
                value: FloatingActionButtonLocation.centerDocked
            );

        private static readonly _ChoiceValue<FloatingActionButtonLocation> kFabEndFloat =
            new _ChoiceValue<FloatingActionButtonLocation>(
                title: "Free - End",
                label: "floating action button floats above the end of the bottom app bar",
                value: FloatingActionButtonLocation.endFloat
            );

        private static readonly _ChoiceValue<FloatingActionButtonLocation> kFabCenterFloat =
            new _ChoiceValue<FloatingActionButtonLocation>(
                title: "Free - Center",
                label: "floating action button is floats above the center of the bottom app bar",
                value: FloatingActionButtonLocation.centerFloat
            );

        private static void _showSnackbar()
        {
            string text =
                "When the Scaffold\"s floating action button location changes, " +
                "the floating action button animates to its new position. " +
                "The BottomAppBar adapts its shape appropriately.";

            _scaffoldKey.currentState.showSnackBar(
                new SnackBar(content: new Text(text))
            );
        }

        // App bar color
        private static List<_NamedColor> kBabColors = new List<_NamedColor>
        {
            new _NamedColor(null, "Clear"),
            new _NamedColor(new Color(0xFFFFC100), "Orange"),
            new _NamedColor(new Color(0xFF91FAFF), "Light Blue"),
            new _NamedColor(new Color(0xFF00D1FF), "Cyan"),
            new _NamedColor(new Color(0xFF00BCFF), "Cerulean"),
            new _NamedColor(new Color(0xFF009BEE), "Blue"),
        };

        private _ChoiceValue<Widget> _fabShape = kCircularFab;
        private _ChoiceValue<bool> _showNotch = kShowNotchTrue;
        private _ChoiceValue<FloatingActionButtonLocation> _fabLocation = kFabEndDocked;
        private Color _babColor = kBabColors.First().color;

        private void _onShowNotchChanged(_ChoiceValue<bool> value)
        {
            this.setState(() => { this._showNotch = value; });
        }

        private void _onFabShapeChanged(_ChoiceValue<Widget> value)
        {
            this.setState(() => { this._fabShape = value; });
        }

        private void _onFabLocationChanged(_ChoiceValue<FloatingActionButtonLocation> value)
        {
            this.setState(() => { this._fabLocation = value; });
        }

        private void _onBabColorChanged(Color value)
        {
            this.setState(() => { this._babColor = value; });
        }

        public override Widget build(BuildContext context)
        {
            return new Scaffold(
                key: _scaffoldKey,
                appBar: new AppBar(
                    title: new Text("Bottom app bar"),
                    elevation: 0.0f,
                    actions: new List<Widget>
                    {
                        new MaterialDemoDocumentationButton(BottomAppBarDemo.routeName),
                        new IconButton(
                            icon: new Icon(Icons.sentiment_very_satisfied),
                            onPressed: () =>
                            {
                                this.setState(() =>
                                {
                                    this._fabShape = this._fabShape == kCircularFab ? kDiamondFab : kCircularFab;
                                });
                            }
                        )
                    }
                ),
                body: new Scrollbar(
                    child: new ListView(
                        padding: EdgeInsets.only(bottom: 88.0f),
                        children: new List<Widget>
                        {
                            new _Heading("FAB Shape"),

                            new _RadioItem<Widget>(kCircularFab, this._fabShape, this._onFabShapeChanged),
                            new _RadioItem<Widget>(kDiamondFab, this._fabShape, this._onFabShapeChanged),
                            new _RadioItem<Widget>(kNoFab, this._fabShape, this._onFabShapeChanged),

                            new Divider(),
                            new _Heading("Notch"),

                            new _RadioItem<bool>(kShowNotchTrue, this._showNotch, this._onShowNotchChanged),
                            new _RadioItem<bool>(kShowNotchFalse, this._showNotch, this._onShowNotchChanged),

                            new Divider(),
                            new _Heading("FAB Position"),

                            new _RadioItem<FloatingActionButtonLocation>(kFabEndDocked, this._fabLocation,
                                this._onFabLocationChanged),
                            new _RadioItem<FloatingActionButtonLocation>(kFabCenterDocked, this._fabLocation,
                                this._onFabLocationChanged),
                            new _RadioItem<FloatingActionButtonLocation>(kFabEndFloat, this._fabLocation,
                                this._onFabLocationChanged),
                            new _RadioItem<FloatingActionButtonLocation>(kFabCenterFloat, this._fabLocation,
                                this._onFabLocationChanged),

                            new Divider(),
                            new _Heading("App bar color"),

                            new _ColorsItem(kBabColors, this._babColor, this._onBabColorChanged)
                        }
                    )
                ),
                floatingActionButton: this._fabShape.value,
                floatingActionButtonLocation: this._fabLocation.value,
                bottomNavigationBar: new _DemoBottomAppBar(
                    color: this._babColor,
                    fabLocation: this._fabLocation.value,
                    shape: this._selectNotch()
                )
            );
        }

        private NotchedShape _selectNotch()
        {
            if (!this._showNotch.value)
                return null;
            if (this._fabShape == kCircularFab)
                return new CircularNotchedRectangle();
            if (this._fabShape == kDiamondFab)
                return new _DiamondNotchedRectangle();
            return null;
        }
    }

    internal class _ChoiceValue<T>
    {
        public _ChoiceValue(T value, string title, string label)
        {
            this.value = value;
            this.title = title;
            this.label = label;
        }

        public readonly T value;
        public readonly string title;
        public readonly string label; // For the Semantics widget that contains title

        public override string ToString()
        {
            return $"{this.GetType()}(\"{this.title}\")";
        }
    }

    internal class _RadioItem<T> : StatelessWidget
    {
        public _RadioItem(_ChoiceValue<T> value, _ChoiceValue<T> groupValue, ValueChanged<_ChoiceValue<T>> onChanged)
        {
            this.value = value;
            this.groupValue = groupValue;
            this.onChanged = onChanged;
        }

        public readonly _ChoiceValue<T> value;
        public readonly _ChoiceValue<T> groupValue;
        public readonly ValueChanged<_ChoiceValue<T>> onChanged;

        public override Widget build(BuildContext context)
        {
            ThemeData theme = Theme.of(context);
            return new Container(
                height: 56.0f,
                //TODO: uncomment this when fixes on EdgeInsetsDirectional lands
                //padding: const EdgeInsetsDirectional.only(start: 16.0),
                padding: EdgeInsets.only(left: 16.0f),
                alignment: AlignmentDirectional.centerStart,
                child: new Row(
                    children: new List<Widget>
                    {
                        new Radio<_ChoiceValue<T>>(
                            value: this.value,
                            groupValue: this.groupValue,
                            onChanged: this.onChanged
                        ),
                        new Expanded(
                            child: new GestureDetector(
                                behavior: HitTestBehavior.opaque,
                                onTap: () =>
                                {
                                    this.onChanged(this.value);
                                },
                                child: new Text(this.value.title,
                                    style: theme.textTheme.subtitle1
                                )
                            )
                        )
                    }
                )
            );
        }
    }

    internal class _NamedColor
    {
        public _NamedColor(Color color, string name)
        {
            this.color = color;
            this.name = name;
        }

        public readonly Color color;
        public readonly string name;
    }

    internal class _ColorsItem : StatelessWidget
    {
        public _ColorsItem(List<_NamedColor> colors, Color selectedColor, ValueChanged<Color> onChanged)
        {
            this.colors = colors;
            this.selectedColor = selectedColor;
            this.onChanged = onChanged;
        }

        public readonly List<_NamedColor> colors;
        public readonly Color selectedColor;
        public readonly ValueChanged<Color> onChanged;

        public override Widget build(BuildContext context)
        {
            return new Row(
                mainAxisAlignment: MainAxisAlignment.spaceEvenly,
                children: this.colors.Select<_NamedColor, Widget>((_NamedColor namedColor) =>
                {
                    return new RawMaterialButton(
                        onPressed: () => { this.onChanged(namedColor.color); },
                        constraints: BoxConstraints.tightFor(
                            width: 32.0f,
                            height: 32.0f
                        ),
                        fillColor: namedColor.color,
                        shape: new CircleBorder(
                            side: new BorderSide(
                                color: namedColor.color == this.selectedColor ? Colors.black : new Color(0xFFD5D7DA),
                                width: 2.0f
                            )
                        ),
                        child: new Container(
                        )
                    );
                }).ToList()
            );
        }
    }

    internal class _Heading : StatelessWidget
    {
        public _Heading(string text)
        {
            this.text = text;
        }

        public readonly string text;

        public override Widget build(BuildContext context)
        {
            ThemeData theme = Theme.of(context);
            return new Container(
                height: 48.0f,
                //TODO: uncomment this when fixes on EdgeInsetsDirectional lands
                //padding:  EdgeInsetsDirectional.only(start: 56.0),
                padding: EdgeInsets.only(left: 56.0f),
                alignment: AlignmentDirectional.centerStart,
                child: new Text(this.text,
                    style: theme.textTheme.bodyText2.copyWith(
                        color: theme.primaryColor
                    )
                )
            );
        }
    }

    internal class _DemoBottomAppBar : StatelessWidget
    {
        public _DemoBottomAppBar(
            Color color = null,
            FloatingActionButtonLocation fabLocation = null,
            NotchedShape shape = null
        )
        {
            this.color = color;
            this.fabLocation = fabLocation;
            this.shape = shape;
        }

        public readonly Color color;
        public readonly FloatingActionButtonLocation fabLocation;
        public readonly NotchedShape shape;

        private static readonly List<FloatingActionButtonLocation> kCenterLocations =
            new List<FloatingActionButtonLocation>
            {
                FloatingActionButtonLocation.centerDocked,
                FloatingActionButtonLocation.centerFloat
            };


        public override Widget build(BuildContext context)
        {
            var children = new List<Widget>
            {
                new IconButton(
                    icon: new Icon(Icons.menu),
                    onPressed: () =>
                    {
                        material_.showModalBottomSheet<object>(
                            context: context,
                            builder: (BuildContext subContext) => new _DemoDrawer()
                        );
                    }
                )
            };

            if (kCenterLocations.Contains(this.fabLocation)) children.Add(new Expanded(child: new SizedBox()));

            children.Add(
                new IconButton(
                    icon: new Icon(Icons.search),
                    onPressed: () =>
                    {
                        Scaffold.of(context).showSnackBar(
                            new SnackBar(content: new Text("This is a dummy search action."))
                        );
                    }
                )
            );

            children.Add(
                new IconButton(
                    icon: new Icon(
                        Theme.of(context).platform == RuntimePlatform.IPhonePlayer
                            ? Icons.more_horiz
                            : Icons.more_vert
                    ),
                    onPressed: () =>
                    {
                        Scaffold.of(context).showSnackBar(
                            new SnackBar(content: new Text("This is a dummy menu action."))
                        );
                    }
                )
            );


            return new BottomAppBar(
                color: this.color,
                shape: this.shape,
                child: new Row(children: children)
            );
        }
    }

    internal class _DemoDrawer : StatelessWidget
    {
        public _DemoDrawer()
        {
        }

        public override Widget build(BuildContext context)
        {
            return new Drawer(
                child: new Column(
                    children: new List<Widget>
                    {
                        new ListTile(
                            leading: new Icon(Icons.search),
                            title: new Text("Search")
                        ),
                        new ListTile(
                            leading: new Icon(Icons.threed_rotation),
                            title: new Text("3D")
                        )
                    }
                )
            );
        }
    }

    internal class _DiamondFab : StatelessWidget
    {
        public _DiamondFab(
            Widget child = null,
            VoidCallback onPressed = null
        )
        {
            this.child = child;
            this.onPressed = onPressed;
        }

        public readonly Widget child;
        public readonly VoidCallback onPressed;

        public override Widget build(BuildContext context)
        {
            return new Material(
                shape: new _DiamondBorder(),
                color: Colors.orange,
                child: new InkWell(
                    onTap: () => this.onPressed?.Invoke(),
                    child: new Container(
                        width: 56.0f,
                        height: 56.0f,
                        child: IconTheme.merge(
                            data: new IconThemeData(color: Theme.of(context).accentIconTheme.color),
                            child: this.child
                        )
                    )
                ),
                elevation: 6.0f
            );
        }
    }

    internal class _DiamondNotchedRectangle : NotchedShape
    {
        public _DiamondNotchedRectangle()
        {
        }

        public override Path getOuterPath(Rect host, Rect guest)
        {
            if (!host.overlaps(guest))
            {
                var path = new Path();
                path.addRect(host);
                return path;
            }

            D.assert(guest.width > 0.0f);

            Rect intersection = guest.intersect(host);
            // We are computing a "V" shaped notch, as in this diagram:
            //    -----\****   /-----
            //          \     /
            //           \   /
            //            \ /
            //
            //  "-" marks the top edge of the bottom app bar.
            //  "\" and "/" marks the notch outline
            //
            //  notchToCenter is the horizontal distance between the guest's center and
            //  the host's top edge where the notch starts (marked with "*").
            //  We compute notchToCenter by similar triangles:
            float notchToCenter =
                intersection.height * (guest.height / 2.0f)
                / (guest.width / 2.0f);

            var retPath = new Path();
            retPath.moveTo(host.left, host.top);
            retPath.lineTo(guest.center.dx - notchToCenter, host.top);
            retPath.lineTo(guest.left + guest.width / 2.0f, guest.bottom);
            retPath.lineTo(guest.center.dx + notchToCenter, host.top);
            retPath.lineTo(host.right, host.top);
            retPath.lineTo(host.right, host.bottom);
            retPath.lineTo(host.left, host.bottom);
            retPath.close();

            return retPath;
        }
    }

    internal class _DiamondBorder : ShapeBorder
    {
        public _DiamondBorder()
        {
        }

        public override EdgeInsetsGeometry dimensions => EdgeInsets.only();


        public override Path getInnerPath(Rect rect, TextDirection? textDirection = null)
        {
            return this.getOuterPath(rect, textDirection: textDirection);
        }


        public override Path getOuterPath(Rect rect, TextDirection? textDirection = null)
        {
            var path = new Path();
            path.moveTo(rect.left + rect.width / 2.0f, rect.top);
            path.lineTo(rect.right, rect.top + rect.height / 2.0f);
            path.lineTo(rect.left + rect.width / 2.0f, rect.bottom);
            path.lineTo(rect.left, rect.top + rect.height / 2.0f);
            path.close();
            return path;
        }

        public override void paint(Canvas canvas, Rect rect, TextDirection? textDirection = null)
        {
        }


        public override ShapeBorder scale(float t)
        {
            return null;
        }
    }
}