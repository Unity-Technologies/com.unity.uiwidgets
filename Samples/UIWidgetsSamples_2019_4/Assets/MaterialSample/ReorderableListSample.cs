using System.Collections.Generic;
using System.Linq;
using RSG;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.material;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.rendering;
using Unity.UIWidgets.ui;
using Unity.UIWidgets.widgets;
using UnityEngine;
using Color = Unity.UIWidgets.ui.Color;
using Material = Unity.UIWidgets.material.Material;
using TextStyle = Unity.UIWidgets.painting.TextStyle;

namespace UIWidgetsSample {
    public class ReorderableListSample : UIWidgetsSamplePanel {
        protected override Widget createWidget() {
            return new MaterialApp(
                showPerformanceOverlay: false,
                home: new MaterialReorderableListViewWidget());
        }

        protected override void OnEnable() {
            FontManager.instance.addFont(Resources.Load<Font>(path: "fonts/MaterialIcons-Regular"), "Material Icons");
            base.OnEnable();
        }
    }

    class MaterialReorderableListViewWidget : StatefulWidget {
        public MaterialReorderableListViewWidget(Key key = null) : base(key) {
        }

        public override State createState() {
            return new MaterialReorderableListViewWidgetState();
        }
    }

    class MaterialReorderableListViewWidgetState : State<MaterialReorderableListViewWidget> {
        List<string> items = new List<string> {"First", "Second", "Third"};

        public override Widget build(BuildContext context) {
            return new Scaffold(
                body: new Scrollbar(
                    child: new ReorderableListView(
                        header: new Text("Header of list"),
                        children: this.items.Select<string, Widget>((item) => {
                            return new Container(
                                key: Key.key(item),
                                width: 300.0f,
                                height: 50.0f,
                                decoration: new BoxDecoration(
                                    color: Colors.blue,
                                    border: Border.all(
                                        color: Colors.black
                                    )
                                ),
                                child: new Center(
                                    child: new Text(
                                        item,
                                        style: new TextStyle(
                                            fontSize: 32
                                        )
                                    )
                                )
                            );
                        }).ToList(),
                        onReorder: (int oldIndex, int newIndex) => {
                            this.setState(() => {
                                if (newIndex > oldIndex) {
                                    newIndex -= 1;
                                }
                                string item = this.items[oldIndex];
                                this.items.RemoveAt(oldIndex);
                                this.items.Insert(newIndex, item);
                            });
                        }
                    )
                )
            );
        }
    }
}