using System.Collections.Generic;
using RSG;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.material;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.rendering;
using Unity.UIWidgets.ui;
using Unity.UIWidgets.widgets;
using UnityEngine;

namespace UIWidgetsSample {
    
    public class MaterialNavigationBarSample : UIWidgetsSamplePanel {

        protected override Widget createWidget() {
            return new MaterialApp(
                showPerformanceOverlay: false,
                home: new MaterialNavigationBarWidget());
        }

        protected override void OnEnable() {
            FontManager.instance.addFont(Resources.Load<Font>(path: "fonts/MaterialIcons-Regular"), "Material Icons");
            base.OnEnable();
        }
    }
    
    class MaterialNavigationBarWidget : StatefulWidget {
        public MaterialNavigationBarWidget(Key key = null) : base(key) {
        }

        public override State createState() {
            return new MaterialNavigationBarWidgetState();
        }
    }

    class MaterialNavigationBarWidgetState : SingleTickerProviderStateMixin<MaterialNavigationBarWidget> {
        int _currentIndex = 0;

        public MaterialNavigationBarWidgetState() {
        }

        public override Widget build(BuildContext context) {
            return new Scaffold(
                bottomNavigationBar: new Container(
                    height: 100,
                    color: Colors.blue,
                    child: new Center(
                        child: new BottomNavigationBar(
                            type: BottomNavigationBarType.shifting,
                            // type: BottomNavigationBarType.fix,
                            items: new List<BottomNavigationBarItem> {
                                new BottomNavigationBarItem(
                                    icon: new Icon(icon: Unity.UIWidgets.material.Icons.work, size: 30),
                                    title: new Text("Work"),
                                    activeIcon: new Icon(icon: Unity.UIWidgets.material.Icons.work, size: 50),
                                    backgroundColor: Colors.blue
                                ),
                                new BottomNavigationBarItem(
                                    icon: new Icon(icon: Unity.UIWidgets.material.Icons.home, size: 30),
                                    title: new Text("Home"),
                                    activeIcon: new Icon(icon: Unity.UIWidgets.material.Icons.home, size: 50),
                                    backgroundColor: Colors.blue
                                ),
                                new BottomNavigationBarItem(
                                    icon: new Icon(icon: Unity.UIWidgets.material.Icons.shop, size: 30),
                                    title: new Text("Shop"),
                                    activeIcon: new Icon(icon: Unity.UIWidgets.material.Icons.shop, size: 50),
                                    backgroundColor: Colors.blue
                                ),
                                new BottomNavigationBarItem(
                                    icon: new Icon(icon: Unity.UIWidgets.material.Icons.school, size: 30),
                                    title: new Text("School"),
                                    activeIcon: new Icon(icon: Unity.UIWidgets.material.Icons.school, size: 50),
                                    backgroundColor: Colors.blue
                                ),
                            },
                            currentIndex: this._currentIndex,
                            onTap: (value) => { this.setState(() => { this._currentIndex = value; }); }
                        )
                    )
                )
            );
        }
    }
}