using System.Collections.Generic;
using Unity.UIWidgets.animation;
using Unity.UIWidgets.engine2;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.material;
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
using UIWidgetsGallery.gallery;
using Color = Unity.UIWidgets.ui.Color;
using Random = UnityEngine.Random;

namespace UIWidgetsSample
{
    public class TextTest : UIWidgetsPanel
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
                    home: new HomeScreen()
                );

            }
        }



        class HomeScreen : StatelessWidget
        {
            public override Widget build(BuildContext context)
            {
                return new CupertinoPageScaffold(
                    child: new Center(
                        child: new CupertinoButton(
                            child: new Text(
                                "THIS IS TAB #"
                            ),
                            onPressed: () =>
                            {
                                Navigator.of(context).push(
                                    new CupertinoPageRoute(builder: (contex3) =>
                                    {
                                        return
                                            new CupertinoAlertDemo();
                                    })
                                );
                            }
                        )
                        //new Text("hello world!", style: CupertinoTheme.of(context).textTheme.navTitleTextStyle)
                    )
                    //backgroundColor: Colors.brown
                );

                List<BottomNavigationBarItem> items = new List<BottomNavigationBarItem>();
                items.Add(new BottomNavigationBarItem(
                    icon: new Icon(CupertinoIcons.bell),
                    title: new Text("views")
                ));
                items.Add(new BottomNavigationBarItem(
                    icon: new Icon(CupertinoIcons.eye_solid),
                    title: new Text("articles")
                ));
                return new CupertinoTabScaffold(
                    tabBar: new CupertinoTabBar(
                        items: items
                    ),
                    tabBuilder: ((contex, index) =>
                    {
                        //return new Center(child: new Text("hello"));
                        return new CupertinoTabView(
                            builder: (contex1) =>
                            {
                                return new CupertinoPageScaffold(
                                    navigationBar: new CupertinoNavigationBar(
                                        middle: (index == 0) ? new Text("views") : new Text("articles")
                                    ),
                                    child: new Center(
                                        
                                        child: new CupertinoButton(
                                            child: new Text(
                                                "THIS IS TAB #",
                                                style: CupertinoTheme.of(contex1)
                                                    .textTheme
                                                    .navTitleTextStyle
                                                //.copyWith(fontSize:32)
                                            ),

                                            onPressed: () =>
                                            {
                                                Navigator.of(contex1).push(
                                                    new CupertinoPageRoute(builder: (contex3) =>
                                                    {
                                                        return
                                                            new CupertinoAlertDemo(); //DetailScreen1(index == 0 ? "views" : "articles");
                                                    })
                                                );
                                            }
                                        )
                                    )
                                );
                            }
                        );
                    })

                );


            }
        }
        
        

        public class DetailScreen1 : StatelessWidget
        {
            public override Widget build(BuildContext context)
            {
                return new Container(
                    color: Colors.green,
                    child: new Column(
                        children: new List<Widget>
                        {
                            new Text("Text test",
                                style: new TextStyle(fontSize: 18, fontWeight: FontWeight.w100)),
                            new Text("Text test",
                                style: new TextStyle(fontFamily: "racher", fontSize: 18, fontWeight: FontWeight.w100)),
                            new Text("Text test",
                                style: new TextStyle(fontFamily: "roboto", fontSize: 18, fontWeight: FontWeight.w200)),
                            new Text("Text test",
                                style: new TextStyle(fontFamily: "roboto", fontSize: 18, fontWeight: FontWeight.w300)),
                            new Text("Text test",
                                style: new TextStyle(fontFamily: "roboto", fontSize: 18, fontWeight: FontWeight.w400)),
                            new Text("Text test",
                                style: new TextStyle(fontFamily: "roboto", fontSize: 18, fontWeight: FontWeight.w500)),
                            new Text("Text test",
                                style: new TextStyle(fontFamily: "roboto", fontSize: 18, fontWeight: FontWeight.w600)),
                            new Text("Text test",
                                style: new TextStyle(fontFamily: "roboto", fontSize: 18, fontWeight: FontWeight.w700)),
                            new Text("Text test",
                                style: new TextStyle(fontFamily: "roboto", fontSize: 18, fontWeight: FontWeight.w800)),
                            new Text("Text test",
                                style: new TextStyle(fontFamily: "roboto", fontSize: 18, fontWeight: FontWeight.w900)),
                            new Text("Text test",
                                style: new TextStyle(fontFamily: "roboto", fontSize: 18, fontWeight: FontWeight.w900,
                                    fontStyle: FontStyle.italic)),
                            new Text("Text test",
                                style: new TextStyle(fontFamily: "roboto", fontSize: 18, fontWeight: FontWeight.w100)),
                            new Text("Text test",
                                style: new TextStyle(fontFamily: "roboto", fontSize: 18, fontWeight: FontWeight.w900)),
                            new Text("-----"),
                            new Text("Text test",
                                style: new TextStyle(fontFamily: "robotox", fontSize: 18, fontWeight: FontWeight.w200)),
                            new Text("Text test",
                                style: new TextStyle(fontFamily: "robotox", fontSize: 18, fontWeight: FontWeight.w300)),
                            new Text("Text test",
                                style: new TextStyle(fontFamily: "robotox", fontSize: 18, fontWeight: FontWeight.w400)),
                            new Text("Text test",
                                style: new TextStyle(fontFamily: "robotox", fontSize: 18, fontWeight: FontWeight.w500)),
                            new Text("Text test",
                                style: new TextStyle(fontFamily: "robotox", fontSize: 18, fontWeight: FontWeight.w600)),
                            new Text("Text test",
                                style: new TextStyle(fontFamily: "robotox", fontSize: 18, fontWeight: FontWeight.w700)),
                            new Text("Text test",
                                style: new TextStyle(fontFamily: "robotox", fontSize: 18, fontWeight: FontWeight.w800)),
                            new Text("Text test",
                                style: new TextStyle(fontFamily: "robotox", fontSize: 18, fontWeight: FontWeight.w900)),
                            new Text("Text test" + (char) 0xf472 + (char) 0xf442 + (char) 0xf43b,
                                style: new TextStyle(fontFamily: "robotox", fontSize: 18, fontWeight: FontWeight.w900,
                                    fontStyle: FontStyle.italic)),
                            new Text("Text test" + (char) 0xf472 + (char) 0xf442 + (char) 0xf43b,
                                style: new TextStyle(fontFamily: "CupertinoIcons", fontSize: 18)),
                        }
                    )
                );
            }
        }

              
    }

    
}