using System.Collections.Generic;
using uiwidgets;
using UIWidgetsGallery.gallery;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.material;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.ui;
using Unity.UIWidgets.widgets;

namespace UIWidgetsGallery.demo.material
{
    internal class MenuDemo : StatefulWidget
    {
        public MenuDemo(Key key = null) : base(key: key)
        {
        }

        public static readonly string routeName = "/material/menu";

        public override State createState()
        {
            return new MenuDemoState();
        }
    }

    internal class MenuDemoState : State<MenuDemo>
    {
        private readonly GlobalKey<ScaffoldState> _scaffoldKey = GlobalKey<ScaffoldState>.key();

        private static readonly string _simpleValue1 = "Menu item value one";
        private static readonly string _simpleValue2 = "Menu item value two";
        private static readonly string _simpleValue3 = "Menu item value three";
        private string _simpleValue;

        private readonly string _checkedValue1 = "One";
        private readonly string _checkedValue2 = "Two";
        private readonly string _checkedValue3 = "Free";
        private readonly string _checkedValue4 = "Four";
        private List<string> _checkedValues;


        public override void initState()
        {
            base.initState();
            _simpleValue = _simpleValue2;
            _checkedValues = new List<string> {_checkedValue3};
        }

        private void showInSnackBar(string value)
        {
            _scaffoldKey.currentState.showSnackBar(new SnackBar(
                content: new Text(value)
            ));
        }

        private List<string> _showSelection = new List<string> {_simpleValue1, _simpleValue2, _simpleValue3};

        private void showMenuSelection(string value)
        {
            if (_showSelection.Contains(value))
                _simpleValue = value;
            showInSnackBar($"You selected: {value}");
        }

        private void showCheckedMenuSelections(string value)
        {
            if (_checkedValues.Contains(value))
                _checkedValues.Remove(value);
            else
                _checkedValues.Add(value);

            showInSnackBar($"Checked {_checkedValues.Count} items");
        }

        private bool isChecked(string value)
        {
            return _checkedValues.Contains(value);
        }


        public override Widget build(BuildContext context)
        {
            return new Scaffold(
                key: _scaffoldKey,
                appBar: new AppBar(
                    title: new Text("Menus"),
                    actions: new List<Widget>
                    {
                        new MaterialDemoDocumentationButton(MenuDemo.routeName),
                        new PopupMenuButton<string>(
                            onSelected: showMenuSelection,
                            itemBuilder: (BuildContext subContext) => new List<PopupMenuEntry<string>>
                            {
                                new PopupMenuItem<string>(
                                    value: "Toolbar menu",
                                    child: new Text("Toolbar menu")
                                ),
                                new PopupMenuItem<string>(
                                    value: "Right here",
                                    child: new Text("Right here")
                                ),
                                new PopupMenuItem<string>(
                                    value: "Hooray!",
                                    child: new Text("Hooray!")
                                )
                            }
                        )
                    }
                ),
                body: new ListTileTheme(
                    iconColor: Theme.of(context).brightness == Brightness.light
                        ? Colors.grey[600]
                        : Colors.grey[500],
                    child: new ListView(
                        padding: material_.kMaterialListPadding,
                        children: new List<Widget>
                        {
                            // Pressing the PopupMenuButton on the right of this item shows
                            // a simple menu with one disabled item. Typically the contents
                            // of this "contextual menu" would reflect the app's state.
                            new ListTile(
                                title: new Text("An item with a context menu button"),
                                trailing: new PopupMenuButton<string>(
                                    padding: EdgeInsets.zero,
                                    onSelected: showMenuSelection,
                                    itemBuilder: (BuildContext subContext) => new List<PopupMenuEntry<string>>
                                    {
                                        new PopupMenuItem<string>(
                                            value: _simpleValue1,
                                            child: new Text("Context menu item one")
                                        ),
                                        new PopupMenuItem<string>(
                                            enabled: false,
                                            child: new Text("A disabled menu item")
                                        ),
                                        new PopupMenuItem<string>(
                                            value: _simpleValue3,
                                            child: new Text("Context menu item three")
                                        )
                                    }
                                )
                            ),
                            // Pressing the PopupMenuButton on the right of this item shows
                            // a menu whose items have text labels and icons and a divider
                            // That separates the first three items from the last one.
                            new ListTile(
                                title: new Text("An item with a sectioned menu"),
                                trailing: new PopupMenuButton<string>(
                                    padding: EdgeInsets.zero,
                                    onSelected: showMenuSelection,
                                    itemBuilder: (BuildContext subContext) => new List<PopupMenuEntry<string>>
                                    {
                                        new PopupMenuItem<string>(
                                            value: "Preview",
                                            child: new ListTile(
                                                leading: new Icon(Icons.visibility),
                                                title: new Text("Preview")
                                            )
                                        ),
                                        new PopupMenuItem<string>(
                                            value: "Share",
                                            child: new ListTile(
                                                leading: new Icon(Icons.person_add),
                                                title: new Text("Share")
                                            )
                                        ),
                                        new PopupMenuItem<string>(
                                            value: "Get Link",
                                            child: new ListTile(
                                                leading: new Icon(Icons.link),
                                                title: new Text("Get link")
                                            )
                                        ),
                                        new PopupMenuDivider<string>(),
                                        new PopupMenuItem<string>(
                                            value: "Remove",
                                            child: new ListTile(
                                                leading: new Icon(Icons.delete),
                                                title: new Text("Remove")
                                            )
                                        )
                                    }
                                )
                            ),
                            // This entire list item is a PopupMenuButton. Tapping anywhere shows
                            // a menu whose current value is highlighted and aligned over the
                            // list item's center line.
                            new PopupMenuButton<string>(
                                padding: EdgeInsets.zero,
                                initialValue: _simpleValue,
                                onSelected: showMenuSelection,
                                child: new ListTile(
                                    title: new Text("An item with a simple menu"),
                                    subtitle: new Text(_simpleValue)
                                ),
                                itemBuilder: (BuildContext subContext) => new List<PopupMenuEntry<string>>
                                {
                                    new PopupMenuItem<string>(
                                        value: _simpleValue1,
                                        child: new Text(_simpleValue1)
                                    ),
                                    new PopupMenuItem<string>(
                                        value: _simpleValue2,
                                        child: new Text(_simpleValue2)
                                    ),
                                    new PopupMenuItem<string>(
                                        value: _simpleValue3,
                                        child: new Text(_simpleValue3)
                                    )
                                }
                            ),
                            // Pressing the PopupMenuButton on the right of this item shows a menu
                            // whose items have checked icons that reflect this app's state.
                            new ListTile(
                                title: new Text("An item with a checklist menu"),
                                trailing: new PopupMenuButton<string>(
                                    padding: EdgeInsets.zero,
                                    onSelected: showCheckedMenuSelections,
                                    itemBuilder: (BuildContext subContext) => new List<PopupMenuEntry<string>>
                                    {
                                        new CheckedPopupMenuItem<string>(
                                            value: _checkedValue1,
                                            isChecked: isChecked(_checkedValue1),
                                            child: new Text(_checkedValue1)
                                        ),
                                        new CheckedPopupMenuItem<string>(
                                            value: _checkedValue2,
                                            enabled: false,
                                            isChecked: isChecked(_checkedValue2),
                                            child: new Text(_checkedValue2)
                                        ),
                                        new CheckedPopupMenuItem<string>(
                                            value: _checkedValue3,
                                            isChecked: isChecked(_checkedValue3),
                                            child: new Text(_checkedValue3)
                                        ),
                                        new CheckedPopupMenuItem<string>(
                                            value: _checkedValue4,
                                            isChecked: isChecked(_checkedValue4),
                                            child: new Text(_checkedValue4)
                                        )
                                    }
                                )
                            )
                        }
                    )
                )
            );
        }
    }
}