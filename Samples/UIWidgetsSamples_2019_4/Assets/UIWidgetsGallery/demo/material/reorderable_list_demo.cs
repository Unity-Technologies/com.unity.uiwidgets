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
    internal enum _ReorderableListType
    {
        /// A list tile that contains a [CircleAvatar].
        horizontalAvatar,

        /// A list tile that contains a [CircleAvatar].
        verticalAvatar,

        /// A list tile that contains three lines of text and a checkbox.
        threeLine,
    }

    internal class ReorderableListDemo : StatefulWidget
    {
        public ReorderableListDemo(Key key = null) : base(key: key)
        {
        }

        public static readonly string routeName = "/material/reorderable-list";

        public override State createState()
        {
            return new _ReorderableListDemoState();
        }
    }

    internal class _ListItem
    {
        public _ListItem(string value, bool? checkState)
        {
            this.value = value;
            this.checkState = checkState;
        }

        public readonly string value;

        internal bool? checkState;
    }

    internal class _ReorderableListDemoState : State<ReorderableListDemo>
    {
        private static readonly GlobalKey<ScaffoldState> scaffoldKey = GlobalKey<ScaffoldState>.key();

        private PersistentBottomSheetController<object> _bottomSheet;
        private _ReorderableListType _itemType = _ReorderableListType.threeLine;
        private bool _reverse = false;
        private bool _reverseSort = false;

        private static readonly List<_ListItem> _items = new List<string>
        {
            "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N",
        }.Select<string, _ListItem>((string item) => new _ListItem(item, false)).ToList();

        private void changeItemType(_ReorderableListType type)
        {
            setState(() => { _itemType = type; });
            // Rebuild the bottom sheet to reflect the selected list view.
            _bottomSheet?.setState(() =>
            {
                // Trigger a rebuild.
            });
            // Close the bottom sheet to give the user a clear view of the list.
            _bottomSheet?.close();
        }

        private void changeReverse(bool? newValue)
        {
            setState(() => { _reverse = newValue.Value; });
            // Rebuild the bottom sheet to reflect the selected list view.
            _bottomSheet?.setState(() =>
            {
                // Trigger a rebuild.
            });
            // Close the bottom sheet to give the user a clear view of the list.
            _bottomSheet?.close();
        }

        private void _showConfigurationSheet()
        {
            setState(() =>
            {
                _bottomSheet = scaffoldKey.currentState.showBottomSheet((BuildContext bottomSheetContext) =>
                {
                    return new DecoratedBox(
                        decoration: new BoxDecoration(
                            border: new Border(top: new BorderSide(color: Colors.black26))
                        ),
                        child: new ListView(
                            shrinkWrap: true,
                            primary: false,
                            children: new List<Widget>
                            {
                                new CheckboxListTile(
                                    dense: true,
                                    title: new Text("Reverse"),
                                    value: _reverse,
                                    onChanged: changeReverse
                                ),
                                new RadioListTile<_ReorderableListType>(
                                    dense: true,
                                    title: new Text("Horizontal Avatars"),
                                    value: _ReorderableListType.horizontalAvatar,
                                    groupValue: _itemType,
                                    onChanged: changeItemType
                                ),
                                new RadioListTile<_ReorderableListType>(
                                    dense: true,
                                    title: new Text("Vertical Avatars"),
                                    value: _ReorderableListType.verticalAvatar,
                                    groupValue: _itemType,
                                    onChanged: changeItemType
                                ),
                                new RadioListTile<_ReorderableListType>(
                                    dense: true,
                                    title: new Text("Three-line"),
                                    value: _ReorderableListType.threeLine,
                                    groupValue: _itemType,
                                    onChanged: changeItemType
                                )
                            }
                        )
                    );
                });

                // Garbage collect the bottom sheet when it closes.
                _bottomSheet.closed.whenComplete(() =>
                {
                    if (mounted)
                        setState(() => { _bottomSheet = null; });
                });
            });
        }

        private Widget buildListTile(_ListItem item)
        {
            Widget secondary = new Text(
                "Even more additional list item information appears on line three."
            );
            Widget listTile = null;
            switch (_itemType)
            {
                case _ReorderableListType.threeLine:
                    listTile = new CheckboxListTile(
                        key: Key.key(item.value),
                        isThreeLine: true,
                        value: item.checkState ?? false,
                        onChanged: (bool? newValue) => { setState(() => { item.checkState = newValue.Value; }); },
                        title: new Text($"This item represents {item.value}."),
                        subtitle: secondary,
                        secondary: new Icon(Icons.drag_handle)
                    );
                    break;
                case _ReorderableListType.horizontalAvatar:
                case _ReorderableListType.verticalAvatar:
                    listTile = new Container(
                        key: Key.key(item.value),
                        height: 100.0f,
                        width: 100.0f,
                        child: new CircleAvatar(child: new Text(item.value),
                            backgroundColor: Colors.green
                        )
                    );
                    break;
            }

            return listTile;
        }

        private void _onReorder(int oldIndex, int newIndex)
        {
            setState(() =>
            {
                if (newIndex > oldIndex) newIndex -= 1;
                _ListItem item = _items[oldIndex];
                _items.RemoveAt(oldIndex);
                _items.Insert(newIndex, item);
            });
        }


        public override Widget build(BuildContext context)
        {
            return new Scaffold(
                key: scaffoldKey,
                appBar: new AppBar(
                    title: new Text("Reorderable list"),
                    actions: new List<Widget>
                    {
                        new MaterialDemoDocumentationButton(ReorderableListDemo.routeName),
                        new IconButton(
                            icon: new Icon(Icons.sort_by_alpha),
                            tooltip: "Sort",
                            onPressed: () =>
                            {
                                setState(() =>
                                {
                                    _reverseSort = !_reverseSort;
                                    _items.Sort((_ListItem a, _ListItem b) =>
                                        _reverseSort ? b.value.CompareTo(a.value) : a.value.CompareTo(b.value));
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
                    child: new ReorderableListView(
                        header: _itemType != _ReorderableListType.threeLine
                            ? new Padding(
                                padding: EdgeInsets.all(8.0f),
                                child: new Text("Header of the list", style: Theme.of(context).textTheme.headline5))
                            : null,
                        onReorder: _onReorder,
                        reverse: _reverse,
                        scrollDirection: _itemType == _ReorderableListType.horizontalAvatar
                            ? Axis.horizontal
                            : Axis.vertical,
                        padding: EdgeInsets.symmetric(vertical: 8.0f),
                        children: _items.Select<_ListItem, Widget>(buildListTile).ToList()
                    )
                )
            );
        }
    }
}