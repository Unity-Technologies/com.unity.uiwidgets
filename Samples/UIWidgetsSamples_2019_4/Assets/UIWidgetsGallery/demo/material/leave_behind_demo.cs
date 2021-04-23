using System;
using System.Collections.Generic;
using System.Linq;
using uiwidgets;
using UIWidgetsGallery.gallery;
using Unity.UIWidgets.async;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.material;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.widgets;
using UnityEngine;

namespace UIWidgetsGallery.demo.material
{
    internal enum LeaveBehindDemoAction
    {
        reset,
        horizontalSwipe,
        leftSwipe,
        rightSwipe,
        confirmDismiss,
    }

    internal class LeaveBehindItem : IComparable<LeaveBehindItem>
    {
        public LeaveBehindItem(
            int index = 0,
            string name = null,
            string subject = null,
            string body = null)
        {
            this.index = index;
            this.name = name;
            this.subject = subject;
            this.body = body;
        }

        public static LeaveBehindItem from(LeaveBehindItem item)
        {
            return new LeaveBehindItem(
                index: item.index,
                name: item.name,
                subject: item.subject,
                body: item.body
            );
        }

        public readonly int index;
        public readonly string name;
        public readonly string subject;
        public readonly string body;

        public int CompareTo(LeaveBehindItem other)
        {
            return index.CompareTo(other.index);
        }
    }

    internal class LeaveBehindDemo : StatefulWidget
    {
        public LeaveBehindDemo(Key key = null) : base(key: key)
        {
        }

        public static readonly string routeName = "/material/leave-behind";

        public override State createState()
        {
            return new LeaveBehindDemoState();
        }
    }

    internal class LeaveBehindDemoState : State<LeaveBehindDemo>
    {
        private static readonly GlobalKey<ScaffoldState> _scaffoldKey = GlobalKey<ScaffoldState>.key();
        private DismissDirection _dismissDirection = DismissDirection.horizontal;
        private bool _confirmDismiss = true;
        private List<LeaveBehindItem> leaveBehindItems;

        private void initListItems()
        {
            leaveBehindItems = new List<LeaveBehindItem>();

            for (int i = 0; i < 16; i++)
            {
                var index = i;
                leaveBehindItems.Add(new LeaveBehindItem(
                    index: index,
                    name: $"Item {index} Sender",
                    subject: $"Subject: {index}",
                    body: $"[{index}] first line of the message's body..."
                ));
            }
        }


        public override void initState()
        {
            base.initState();
            initListItems();
        }

        private void handleDemoAction(LeaveBehindDemoAction action)
        {
            setState(() =>
            {
                switch (action)
                {
                    case LeaveBehindDemoAction.reset:
                        initListItems();
                        break;
                    case LeaveBehindDemoAction.horizontalSwipe:
                        _dismissDirection = DismissDirection.horizontal;
                        break;
                    case LeaveBehindDemoAction.leftSwipe:
                        _dismissDirection = DismissDirection.endToStart;
                        break;
                    case LeaveBehindDemoAction.rightSwipe:
                        _dismissDirection = DismissDirection.startToEnd;
                        break;
                    case LeaveBehindDemoAction.confirmDismiss:
                        _confirmDismiss = !_confirmDismiss;
                        break;
                }
            });
        }

        private int lowerBound(List<LeaveBehindItem> items, LeaveBehindItem item)
        {
            items.Sort();
            for (int i = 0; i < items.Count; i++)
                if (item.CompareTo(items[i]) >= 0)
                    return i;
            return items.Count - 1;
        }

        private void handleUndo(LeaveBehindItem item)
        {
            int insertionIndex = lowerBound(leaveBehindItems, item);
            setState(() => { leaveBehindItems.Insert(insertionIndex, item); });
        }

        private void _handleArchive(LeaveBehindItem item)
        {
            setState(() => { leaveBehindItems.Remove(item); });
            _scaffoldKey.currentState.showSnackBar(new SnackBar(
                content: new Text($"You archived item {item.index}"),
                action: new SnackBarAction(
                    label: "UNDO",
                    onPressed: () => { handleUndo(item); }
                )
            ));
        }

        private void _handleDelete(LeaveBehindItem item)
        {
            setState(() => { leaveBehindItems.Remove(item); });
            _scaffoldKey.currentState.showSnackBar(new SnackBar(
                content: new Text($"You deleted item {item.index}"),
                action: new SnackBarAction(
                    label: "UNDO",
                    onPressed: () => { handleUndo(item); }
                )
            ));
        }

        public override Widget build(BuildContext context)
        {
            Widget body = null;
            if (leaveBehindItems.isEmpty())
                body = new Center(
                    child: new RaisedButton(
                        onPressed: () => handleDemoAction(LeaveBehindDemoAction.reset),
                        child: new Text("Reset the list")
                    )
                );
            else
                body = new Scrollbar(
                    child: new ListView(
                        children: leaveBehindItems.Select<LeaveBehindItem, Widget>((LeaveBehindItem item) =>
                        {
                            return new _LeaveBehindListItem(
                                confirmDismiss: _confirmDismiss,
                                item: item,
                                onArchive: _handleArchive,
                                onDelete: _handleDelete,
                                dismissDirection: _dismissDirection
                            );
                        }).ToList()
                    )
                );

            return new Scaffold(
                key: _scaffoldKey,
                appBar: new AppBar(
                    title: new Text("Swipe to dismiss"),
                    actions: new List<Widget>
                    {
                        new MaterialDemoDocumentationButton(LeaveBehindDemo.routeName),
                        new PopupMenuButton<LeaveBehindDemoAction>(
                            onSelected: handleDemoAction,
                            itemBuilder: (BuildContext subContext) => new List<PopupMenuEntry<LeaveBehindDemoAction>>
                            {
                                new PopupMenuItem<LeaveBehindDemoAction>(
                                    value: LeaveBehindDemoAction.reset,
                                    child: new Text("Reset the list")
                                ),
                                new PopupMenuDivider<LeaveBehindDemoAction>(),
                                new CheckedPopupMenuItem<LeaveBehindDemoAction>(
                                    value: LeaveBehindDemoAction.horizontalSwipe,
                                    isChecked: _dismissDirection == DismissDirection.horizontal,
                                    child: new Text("Horizontal swipe")
                                ),
                                new CheckedPopupMenuItem<LeaveBehindDemoAction>(
                                    value: LeaveBehindDemoAction.leftSwipe,
                                    isChecked: _dismissDirection == DismissDirection.endToStart,
                                    child: new Text("Only swipe left")
                                ),
                                new CheckedPopupMenuItem<LeaveBehindDemoAction>(
                                    value: LeaveBehindDemoAction.rightSwipe,
                                    isChecked: _dismissDirection == DismissDirection.startToEnd,
                                    child: new Text("Only swipe right")
                                ),
                                new CheckedPopupMenuItem<LeaveBehindDemoAction>(
                                    value: LeaveBehindDemoAction.confirmDismiss,
                                    isChecked: _confirmDismiss,
                                    child: new Text("Confirm dismiss")
                                )
                            }
                        )
                    }
                ),
                body: body
            );
        }
    }

    internal delegate void onArchiveFunc(LeaveBehindItem item);

    internal delegate void onDeleteFunc(LeaveBehindItem item);

    internal class _LeaveBehindListItem : StatelessWidget
    {
        public _LeaveBehindListItem(
            Key key = null,
            LeaveBehindItem item = null,
            onArchiveFunc onArchive = null,
            onDeleteFunc onDelete = null,
            DismissDirection? dismissDirection = null,
            bool? confirmDismiss = null
        ) : base(key: key)
        {
            this.item = item;
            this.onArchive = onArchive;
            this.onDelete = onDelete;
            this.dismissDirection = dismissDirection;
            this.confirmDismiss = confirmDismiss;
        }

        public readonly LeaveBehindItem item;
        public readonly DismissDirection? dismissDirection;
        public readonly onArchiveFunc onArchive;
        public readonly onDeleteFunc onDelete;
        public readonly bool? confirmDismiss;

        private void _handleArchive()
        {
            Debug.Log("handle archived >>>>");
            onArchive(item);
        }

        private void _handleDelete()
        {
            Debug.Log("handle deleted >>>>");
            onDelete(item);
        }


        public override Widget build(BuildContext context)
        {
            ThemeData theme = Theme.of(context);

            Future<bool> dismissDirectionFunc(DismissDirection? dismissDirection)
            {
                switch (dismissDirection)
                {
                    case DismissDirection.endToStart:
                        return _showConfirmationDialog(context, "archive");
                    case DismissDirection.startToEnd:
                        return _showConfirmationDialog(context, "delete");
                    case DismissDirection.horizontal:
                    case DismissDirection.vertical:
                    case DismissDirection.up:
                    case DismissDirection.down:
                        D.assert(false);
                        break;
                }

                return Future.value(false).to<bool>();
            }

            return new Dismissible(
                key: new ObjectKey(item),
                direction: dismissDirection.Value,
                onDismissed: (DismissDirection? direction) =>
                {
                    if (direction == DismissDirection.endToStart)
                        _handleArchive();
                    else
                        _handleDelete();
                },
                confirmDismiss: !confirmDismiss.Value ? (ConfirmDismissCallback) null : dismissDirectionFunc,
                background: new Container(
                    color: theme.primaryColor,
                    child: new Center(
                        child: new ListTile(
                            leading: new Icon(Icons.delete, color: Colors.white, size: 36.0f)
                        )
                    )
                ),
                secondaryBackground: new Container(
                    color: theme.primaryColor,
                    child: new Center(
                        child: new ListTile(
                            trailing: new Icon(Icons.archive, color: Colors.white, size: 36.0f)
                        )
                    )
                ),
                child: new Container(
                    decoration: new BoxDecoration(
                        color: theme.canvasColor,
                        border: new Border(bottom: new BorderSide(color: theme.dividerColor))
                    ),
                    child: new ListTile(
                        title: new Text(item.name),
                        subtitle: new Text($"{item.subject}\n{item.body}"),
                        isThreeLine: true
                    )
                )
            );
        }

        private Future<bool> _showConfirmationDialog(BuildContext context, string action)
        {
            return material_.showDialog<bool>(
                context: context,
                barrierDismissible: true,
                builder: (BuildContext subContext) =>
                {
                    return new AlertDialog(
                        title: new Text($"Do you want to {action} this item?"),
                        actions: new List<Widget>
                        {
                            new FlatButton(
                                child: new Text("Yes"),
                                onPressed: () =>
                                {
                                    Navigator.pop(context, true); // showDialog() returns true
                                }
                            ),
                            new FlatButton(
                                child: new Text("No"),
                                onPressed: () =>
                                {
                                    Navigator.pop(context, false); // showDialog() returns false
                                }
                            )
                        }
                    );
                }
            );
        }
    }
}