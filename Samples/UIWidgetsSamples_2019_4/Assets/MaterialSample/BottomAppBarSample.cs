using System.Collections.Generic;
using uiwidgets;
using Unity.UIWidgets.engine;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.material;
using Unity.UIWidgets.rendering;
using Unity.UIWidgets.ui;
using Unity.UIWidgets.widgets;
using UnityEngine;
using Color = Unity.UIWidgets.ui.Color;
using ui_ = Unity.UIWidgets.widgets.ui_;

namespace UIWidgetsSample {
    
    public class BottomAppBarSample : UIWidgetsPanel {

        protected override void main() {
            ui_.runApp(new MaterialApp(
                showPerformanceOverlay: false,
                home: new BottomAppBarWidget()));
        }

        protected new void OnEnable() {
            base.OnEnable();
        }
    }
    
    public class BottomAppBarWidget : StatelessWidget {
        public BottomAppBarWidget(Key key = null) : base(key) {

        }

        public override Widget build(BuildContext context) {
            return new Scaffold(
                backgroundColor: Colors.grey,
                bottomNavigationBar: new BottomAppBar(
                    child: new Row(
                        mainAxisSize: MainAxisSize.max,
                        mainAxisAlignment: MainAxisAlignment.spaceBetween,
                        children: new List<Widget> {
                            new IconButton(icon: new Icon(Unity.UIWidgets.material.Icons.menu), onPressed: () => { }),
                            new IconButton(icon: new Icon(Unity.UIWidgets.material.Icons.account_balance),
                                onPressed: () => { })
                        })));
        }
    }
}