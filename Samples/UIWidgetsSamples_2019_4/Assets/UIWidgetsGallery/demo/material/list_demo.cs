using System.Collections.Generic;
using System.Linq;
using uiwidgets;
using UIWidgetsGallery.gallery;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.material;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.ui;
using Unity.UIWidgets.widgets;
using UnityEngine;

namespace UIWidgetsGallery.demo.material
{
    internal enum _MaterialListType
    {
        /// A list tile that contains a single line of text.
        oneLine,

        /// A list tile that contains a [CircleAvatar] followed by a single line of text.
        oneLineWithAvatar,

        /// A list tile that contains two lines of text.
        twoLine,

        /// A list tile that contains three lines of text.
        threeLine,
    }

    internal class ListDemo : StatefulWidget
    {
        public ListDemo(Key key = null) : base(key: key)
        {
        }

        public static readonly string routeName = "/material/list";

        public override State createState()
        {
            return new _ListDemoState();
        }
    }

    internal class _ListDemoState : State<ListDemo>
    {
        private static readonly GlobalKey<ScaffoldState> scaffoldKey = GlobalKey<ScaffoldState>.key();

        private PersistentBottomSheetController<object> _bottomSheet;
        private _MaterialListType _itemType = _MaterialListType.threeLine;
        private bool _dense = false;
        private bool _showAvatars = true;
        private bool _showIcons = false;
        private bool _showDividers = false;
        private bool _reverseSort = false;

        private List<string> items = new List<string>
        {
            "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N",
        };

        private void changeItemType(_MaterialListType type)
        {
            setState(() => { _itemType = type; });
            _bottomSheet?.setState(() => { });
        }

        private void _showConfigurationSheet()
        {
            PersistentBottomSheetController<object> bottomSheet = scaffoldKey.currentState.showBottomSheet(
                (BuildContext bottomSheetContext) =>
                {
                    return new Container(
                        decoration: new BoxDecoration(
                            border: new Border(top: new BorderSide(color: Colors.black26))
                        ),
                        child: new ListView(
                            shrinkWrap: true,
                            primary: false,
                            children: new List<Widget>
                            {
                                new ListTile(
                                    dense: true,
                                    title: new Text("One-line"),
                                    trailing: new Radio<_MaterialListType>(
                                        value: _showAvatars
                                            ? _MaterialListType.oneLineWithAvatar
                                            : _MaterialListType.oneLine,
                                        groupValue: _itemType,
                                        onChanged: changeItemType
                                    )
                                ),
                                new ListTile(
                                    dense: true,
                                    title: new Text("Two-line"),
                                    trailing: new Radio<_MaterialListType>(
                                        value: _MaterialListType.twoLine,
                                        groupValue: _itemType,
                                        onChanged: changeItemType
                                    )
                                ),
                                new ListTile(
                                    dense: true,
                                    title: new Text("Three-line"),
                                    trailing: new Radio<_MaterialListType>(
                                        value: _MaterialListType.threeLine,
                                        groupValue: _itemType,
                                        onChanged: changeItemType
                                    )
                                ),
                                new ListTile(
                                    dense: true,
                                    title: new Text("Show avatar"),
                                    trailing: new Checkbox(
                                        value: _showAvatars,
                                        onChanged: (bool? value) =>
                                        {
                                            setState(() => { _showAvatars = value.Value; });
                                            _bottomSheet?.setState(() => { });
                                        }
                                    )
                                ),
                                new ListTile(
                                    dense: true,
                                    title: new Text("Show icon"),
                                    trailing: new Checkbox(
                                        value: _showIcons,
                                        onChanged: (bool? value) =>
                                        {
                                            setState(() => { _showIcons = value.Value; });
                                            _bottomSheet?.setState(() => { });
                                        }
                                    )
                                ),
                                new ListTile(
                                    dense: true,
                                    title: new Text("Show dividers"),
                                    trailing: new Checkbox(
                                        value: _showDividers,
                                        onChanged: (bool? value) =>
                                        {
                                            setState(() => { _showDividers = value.Value; });
                                            _bottomSheet?.setState(() => { });
                                        }
                                    )
                                ),
                                new ListTile(
                                    dense: true,
                                    title: new Text("Dense layout"),
                                    trailing: new Checkbox(
                                        value: _dense,
                                        onChanged: (bool? value) =>
                                        {
                                            setState(() => { _dense = value.Value; });
                                            _bottomSheet?.setState(() => { });
                                        }
                                    )
                                )
                            }
                        )
                    );
                });

            setState(() => { _bottomSheet = bottomSheet; });

            _bottomSheet.closed.whenComplete(() =>
            {
                if (mounted)
                    setState(() => { _bottomSheet = null; });
            });
        }

        private Widget buildListTile(BuildContext context, string item)
        {
            Widget secondary = null;
            if (_itemType == _MaterialListType.twoLine)
                secondary = new Text("Additional item information.");
            else if (_itemType == _MaterialListType.threeLine)
                secondary = new Text(
                    "Even more additional list item information appears on line three."
                );
            return new ListTile(
                isThreeLine: _itemType == _MaterialListType.threeLine,
                dense: _dense,
                leading: _showAvatars ? new CircleAvatar(child: new Text(item)) : null,
                title: new Text($"This item represents {item}."),
                subtitle: secondary,
                trailing: _showIcons ? new Icon(Icons.info, color: Theme.of(context).disabledColor) : null
            );
        }

        public override Widget build(BuildContext context)
        {
            string layoutText = _dense ? " \u2013 Dense" : "";
            string itemTypeText = "";
            switch (_itemType)
            {
                case _MaterialListType.oneLine:
                case _MaterialListType.oneLineWithAvatar:
                    itemTypeText = "Single-line";
                    break;
                case _MaterialListType.twoLine:
                    itemTypeText = "Two-line";
                    break;
                case _MaterialListType.threeLine:
                    itemTypeText = "Three-line";
                    break;
            }

            IEnumerable<Widget> listTiles = items.Select<string, Widget>((string item) => buildListTile(context, item));
            if (_showDividers)
                listTiles = ListTile.divideTiles(context: context, tiles: listTiles);

            return new Scaffold(
                key: scaffoldKey,
                appBar: new AppBar(
                    title: new Text($"Scrolling list\n{itemTypeText}{layoutText}"),
                    actions: new List<Widget>
                    {
                        new MaterialDemoDocumentationButton(ListDemo.routeName),
                        new IconButton(
                            icon: new Icon(Icons.sort_by_alpha),
                            tooltip: "Sort",
                            onPressed: () =>
                            {
                                setState(() =>
                                {
                                    _reverseSort = !_reverseSort;
                                    items.Sort((string a, string b) => _reverseSort ? b.CompareTo(a) : a.CompareTo(b));
                                });
                            }
                        ),
                        new IconButton(
                            icon: new Icon(
                                Theme.of(context).platform == RuntimePlatform.IPhonePlayer
                                    ? Icons.more_horiz
                                    : Icons.more_vert
                            ),
                            tooltip: "Show menu",
                            onPressed: _bottomSheet == null ? _showConfigurationSheet : (VoidCallback) null
                        )
                    }
                ),
                body: new Scrollbar(
                    child: new ListView(
                        padding: EdgeInsets.symmetric(vertical: _dense ? 4.0f : 8.0f),
                        children: listTiles.ToList()
                    )
                )
            );
        }
    }
}