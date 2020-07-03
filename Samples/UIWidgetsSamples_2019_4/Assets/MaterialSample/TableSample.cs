using System.Collections.Generic;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.material;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.rendering;
using Unity.UIWidgets.ui;
using Unity.UIWidgets.widgets;
using UnityEngine;
using Color = Unity.UIWidgets.ui.Color;

namespace UIWidgetsSample {
    
    public class TableSample : UIWidgetsSamplePanel {

        protected override Widget createWidget() {
            return new MaterialApp(
                showPerformanceOverlay: false,
                home: new TableWidget());
        }

        protected override void OnEnable() {
            FontManager.instance.addFont(Resources.Load<Font>(path: "fonts/MaterialIcons-Regular"), "Material Icons");
            base.OnEnable();
        }
    }
    
    public class TableWidget : StatelessWidget {
        public TableWidget(Key key = null) : base(key) {
        }

        public override Widget build(BuildContext context) {
            return new Scaffold(
                body: new Table(
                    children: new List<TableRow> {
                        new TableRow(
                            decoration: new BoxDecoration(color: Colors.blue),
                            children: new List<Widget> {
                                new Text("item 1"),
                                new Text("item 2")
                            }
                        ),
                        new TableRow(children: new List<Widget> {
                                new Text("item 3"),
                                new Text("item 4")
                            }
                        )
                    },
                    defaultVerticalAlignment: TableCellVerticalAlignment.middle));
        }
    }
}