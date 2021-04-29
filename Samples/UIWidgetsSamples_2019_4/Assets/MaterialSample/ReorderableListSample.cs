using System.Collections.Generic;
using System.Linq;
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
using Material = Unity.UIWidgets.material.Material;
using TextStyle = Unity.UIWidgets.painting.TextStyle;
using ui_ = Unity.UIWidgets.widgets.ui_;

namespace UIWidgetsSample {
    public class ReorderableListSample : UIWidgetsPanel {
        protected override void main() {
            ui_.runApp(new MaterialApp(
                showPerformanceOverlay: false,
                home: new MaterialReorderableListViewWidget()));
        }

        protected new void OnEnable() {
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
        List<string> _list = new List<string> {"Apple", "Ball", "Cat", "Dog", "Elephant"};

        public override Widget build(BuildContext context) {
            return new Scaffold(
                body: new ReorderableListView(
                    children: this._list.Select<string, Widget>((item) =>
                    {
                        return new ListTile(
                            Key.key(item),
                            title: new Text(item),
                            trailing: new Icon(Icons.menu));
                    }).ToList(),
                    onReorder: (int start, int current) =>
                    {
                        if (start < current) {
                            int end = current - 1;
                            string startItem = _list[start];
                            int i = 0;
                            int local = start;
                            do {
                                _list[local] = _list[++local];
                                i++;
                            } while (i < end - start);
                            _list[end] = startItem;
                        }
                        // dragging from bottom to top
                        else if (start > current) {
                            string startItem = _list[start];
                            for (int i = start; i > current; i--) {
                                _list[i] = _list[i - 1];
                            }
                            _list[current] = startItem;
                        }
                        setState(() => {});
                    }
                    )
            );
        }
    }
}