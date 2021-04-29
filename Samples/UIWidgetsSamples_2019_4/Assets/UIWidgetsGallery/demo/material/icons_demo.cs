using System.Collections.Generic;
using uiwidgets;
using UIWidgetsGallery.gallery;
using Unity.UIWidgets.material;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.rendering;
using Unity.UIWidgets.ui;
using Unity.UIWidgets.widgets;
using TextStyle = Unity.UIWidgets.painting.TextStyle;

namespace UIWidgetsGallery.demo.material
{
    internal class IconsDemo : StatefulWidget
    {
        public static readonly string routeName = "/material/icons";


        public override State createState()
        {
            return new IconsDemoState();
        }
    }

    internal class IconsDemoState : State<IconsDemo>
    {
        public static readonly List<MaterialColor> iconColors = new List<MaterialColor>
        {
            Colors.red,
            Colors.pink,
            Colors.purple,
            Colors.deepPurple,
            Colors.indigo,
            Colors.blue,
            Colors.lightBlue,
            Colors.cyan,
            Colors.teal,
            Colors.green,
            Colors.lightGreen,
            Colors.lime,
            Colors.yellow,
            Colors.amber,
            Colors.orange,
            Colors.deepOrange,
            Colors.brown,
            Colors.grey,
            Colors.blueGrey
        };

        private int iconColorIndex = 8; // teal

        private Color iconColor => iconColors[iconColorIndex];

        private void handleIconButtonPress()
        {
            setState(() => { iconColorIndex = (iconColorIndex + 1) % iconColors.Count; });
        }

        public override Widget build(BuildContext context)
        {
            return new Scaffold(
                appBar: new AppBar(
                    title: new Text("Icons"),
                    actions: new List<Widget> {new MaterialDemoDocumentationButton(IconsDemo.routeName)}
                ),
                body: new IconTheme(
                    data: new IconThemeData(color: iconColor),
                    child: new SafeArea(
                        top: false,
                        bottom: false,
                        child: new Scrollbar(
                            child: new ListView(
                                padding: EdgeInsets.all(24.0f),
                                children: new List<Widget>
                                {
                                    new _IconsDemoCard(handleIconButtonPress, Icons.face), // direction-agnostic icon
                                    new SizedBox(height: 24.0f),
                                    new _IconsDemoCard(handleIconButtonPress,
                                        Icons.battery_unknown) // direction-aware icon
                                }
                            )
                        )
                    )
                )
            );
        }
    }

    internal class _IconsDemoCard : StatelessWidget
    {
        public _IconsDemoCard(VoidCallback handleIconButtonPress, IconData icon)
        {
            this.handleIconButtonPress = handleIconButtonPress;
            this.icon = icon;
        }

        public readonly VoidCallback handleIconButtonPress;
        public readonly IconData icon;

        private Widget _buildIconButton(float iconSize, IconData icon, bool enabled)
        {
            var prefix = enabled ? "Enabled" : "Disabled";
            return new IconButton(
                icon: new Icon(icon),
                iconSize: iconSize,
                tooltip: $"{prefix} icon button",
                onPressed: enabled ? handleIconButtonPress : null
            );
        }

        private Widget _centeredText(string label)
        {
            return new Padding(
                padding: EdgeInsets.all(8.0f),
                child: new Text(label, textAlign: TextAlign.center)
            );
        }

        private TableRow _buildIconRow(float size)
        {
            return new TableRow(
                children: new List<Widget>
                {
                    _centeredText(size.floor().ToString()),
                    _buildIconButton(size, icon, true),
                    _buildIconButton(size, icon, false)
                }
            );
        }

        public override Widget build(BuildContext context)
        {
            ThemeData theme = Theme.of(context);
            TextStyle textStyle = theme.textTheme.subtitle1.copyWith(color: theme.textTheme.caption.color);
            return new Card(
                child: new DefaultTextStyle(
                    style: textStyle,
                    child: new Table(
                        defaultVerticalAlignment: TableCellVerticalAlignment.middle,
                        children: new List<TableRow>
                        {
                            new TableRow(
                                children: new List<Widget>
                                {
                                    _centeredText("Size"),
                                    _centeredText("Enabled"),
                                    _centeredText("Disabled")
                                }
                            ),
                            _buildIconRow(18.0f),
                            _buildIconRow(24.0f),
                            _buildIconRow(36.0f),
                            _buildIconRow(48.0f)
                        }
                    )
                )
            );
        }
    }
}