using System.Collections.Generic;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.material;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.ui;
using Unity.UIWidgets.widgets;
using UnityEngine;

namespace UIWidgetsSample {
    
    public class MaterialTabBarSample : UIWidgetsSamplePanel {

        protected override Widget createWidget() {
            return new MaterialApp(
                showPerformanceOverlay: false,
                home: new MaterialTabBarWidget());
        }

        protected override void OnEnable() {
            FontManager.instance.addFont(Resources.Load<Font>(path: "fonts/MaterialIcons-Regular"), "Material Icons");
            base.OnEnable();
        }
    }
    
    public class MaterialTabBarWidget : StatefulWidget {
        public MaterialTabBarWidget(Key key = null) : base(key) {
        }

        public override State createState() {
            return new MaterialTabBarWidgetState();
        }
    }
    
    public class MaterialTabBarWidgetState : SingleTickerProviderStateMixin<MaterialTabBarWidget> {
        TabController _tabController;

        public override void initState() {
            base.initState();
            this._tabController = new TabController(vsync: this, length: Choice.choices.Count);
        }

        public override void dispose() {
            this._tabController.dispose();
            base.dispose();
        }

        void _nextPage(int delta) {
            int newIndex = this._tabController.index + delta;
            if (newIndex < 0 || newIndex >= this._tabController.length) {
                return;
            }

            this._tabController.animateTo(newIndex);
        }

        public override Widget build(BuildContext context) {
            List<Widget> tapChildren = new List<Widget>();
            foreach (Choice choice in Choice.choices) {
                tapChildren.Add(
                    new Padding(
                        padding: EdgeInsets.all(16.0f),
                        child: new ChoiceCard(choice: choice)));
            }

            return new Scaffold(
                appBar: new AppBar(
                    title: new Center(
                        child: new Text("AppBar Bottom Widget")
                    ),
                    leading: new IconButton(
                        tooltip: "Previous choice",
                        icon: new Icon(Unity.UIWidgets.material.Icons.arrow_back),
                        onPressed: () => { this._nextPage(-1); }
                    ),
                    actions: new List<Widget> {
                        new IconButton(
                            icon: new Icon(Unity.UIWidgets.material.Icons.arrow_forward),
                            tooltip: "Next choice",
                            onPressed: () => { this._nextPage(1); })
                    },
                    bottom: new PreferredSize(
                        preferredSize: Size.fromHeight(48.0f),
                        child: new Theme(
                            data: Theme.of(context).copyWith(accentColor: Colors.white),
                            child: new Container(
                                height: 48.0f,
                                alignment: Alignment.center,
                                child: new TabPageSelector(
                                    controller: this._tabController))))
                ),
                body: new TabBarView(
                    controller: this._tabController,
                    children: tapChildren
                ));
        }
    }
}