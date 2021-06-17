using System.Collections.Generic;
using uiwidgets;
using Unity.UIWidgets.engine;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.material;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.rendering;
using Unity.UIWidgets.ui;
using Unity.UIWidgets.widgets;
using UnityEngine;
using Color = Unity.UIWidgets.ui.Color;
using ui_ = Unity.UIWidgets.widgets.ui_;

namespace UIWidgetsSample {
    
    public class TableSample : UIWidgetsPanel {

        protected override void main() {
            ui_.runApp(new MaterialApp(
                showPerformanceOverlay: false,
                home: new TableWidget()));
        }

        protected new void OnEnable() {
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