using System.Collections.Generic;
using Unity.UIWidgets.animation;
using Unity.UIWidgets.engine2;
using Unity.UIWidgets.foundation;
//using Unity.UIWidgets.material;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.ui;
using Unity.UIWidgets.widgets;
using FontStyle = Unity.UIWidgets.ui.FontStyle;
using Image = Unity.UIWidgets.widgets.Image;
using TextStyle = Unity.UIWidgets.painting.TextStyle;
using ui_ = Unity.UIWidgets.widgets.ui_;
using Unity.UIWidgets.cupertino;
using Unity.UIWidgets.rendering;
//using UIWidgetsGallery.gallery;
using Unity.UIWidgets.service;
using Brightness = Unity.UIWidgets.ui.Brightness;
using UnityEngine;
using System;
using uiwidgets;
using UIWidgetsGallery.gallery;
using Color = Unity.UIWidgets.ui.Color;
using Random = UnityEngine.Random;

namespace UIWidgetsSample
{
    public class TextField : UIWidgetsPanel
    {
        protected void OnEnable()
        {
            base.OnEnable();
        }

        protected override void main()
        {
            ui_.runApp(new MyApp());
        }

        class MyApp : StatelessWidget
        {
            public override Widget build(BuildContext context)
            {
                return new CupertinoApp(
                    home: new HomeScreen() //new DetailScreen1("ok")
                    //color: Color.white
                );
            }
        }

        class HomeScreen : StatelessWidget
        {
            public override Widget build(BuildContext context)
            {
                //return new CupertinoNavigationDemo();
                //return new CupertinoAlertDemo();
                //return new CupertinoPickerDemo();
                return new HomeScreen3();
            }
        }

        public class HomeScreen3 : StatelessWidget
        {
            public override Widget build(BuildContext context)
            {
                return new CupertinoPageScaffold(
                    child: new Column(
                        children: new List<Widget>()
                        {
                            new Container(
                                height: 300,
                                width: 200,
                                color: Colors.blue
                            ),
                            new MyPrefilledText()
                        }
                    )
                );
            }
        }

        public class HomeScreen2 : StatelessWidget
        {
            public override Widget build(BuildContext context)
            {
                // TODO: implement build
                List<BottomNavigationBarItem> items = new List<BottomNavigationBarItem>();
                items.Add(new BottomNavigationBarItem(
                    icon: new Icon(CupertinoIcons.bell), title: new Text("articles")));
                items.Add(new BottomNavigationBarItem(
                    icon: new Icon(CupertinoIcons.eye_solid), title: new Text("articles")));
                return new CupertinoTabScaffold(
                    tabBar: new CupertinoTabBar(
                        items: items
                    ),
                    tabBuilder: (_context, index) =>
                    {
                        return new CupertinoTabView(builder: (__context) =>
                        {
                            return new CupertinoPageScaffold(
                                navigationBar: new CupertinoNavigationBar(
                                    middle: index == 0 ? new Text("views") : new Text("detail")
                                ),
                                child: new Center(
                                    child: new MyPrefilledText()
//                    child: CupertinoButton(
//                        child: CupertinoButton(
//                            child: new Container(
//                                width: 200,
//                                height: 100,
//                                decoration: new BoxDecoration(
//                                    borderRadius:
//                                        BorderRadius.all(Radius.circular(8))),
//                                child: Image.network(
//                                    "https://unity-cn-cms-prd-1254078910.cos.ap-shanghai.myqcloud.com/assetstore-cms-media/img-7dfe215f-0075-4f9c-9b5a-be5ee88b866b",
//                                    gaplessPlayback: true)))
//                    )
                                )
                            );
                        });
                    });
            }
        }

        public class MyPrefilledText : StatefulWidget
        {
            public override State createState() => new _MyPrefilledTextState();
        }

        class _MyPrefilledTextState : State<MyPrefilledText>
        {
            TextEditingController _textController;

            public override void initState()
            {
                base.initState();
                _textController = new TextEditingController(text: "initial text");
            }

            public override Widget build(BuildContext context)
            {
                return new CupertinoTextField(controller: _textController);
            }
        }
    }
}