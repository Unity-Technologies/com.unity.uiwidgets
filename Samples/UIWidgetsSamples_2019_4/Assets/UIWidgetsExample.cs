using System.Collections.Generic;
using Unity.UIWidgets.engine2;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.material;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.ui;
using Unity.UIWidgets.widgets;
using TextStyle = Unity.UIWidgets.painting.TextStyle;
using ui_ = Unity.UIWidgets.widgets.ui_;
<<<<<<< HEAD
using Unity.UIWidgets.cupertino;
using Unity.UIWidgets.rendering;
=======
>>>>>>> parent of f5669e93... add cupertino and sth goes wrong

namespace UIWidgetsSample
{
    public class UIWidgetsExample : UIWidgetsPanel
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
                return new WidgetsApp(
                    home: new ExampleApp(),
                    pageRouteBuilder: (settings, builder) =>
                        new PageRouteBuilder(
                            settings: settings,
                            pageBuilder: (Buildcontext, animation, secondaryAnimation) => builder(context)
                        )
                );
            }
        }

        class ExampleApp : StatefulWidget
        {
            public ExampleApp(Key key = null) : base(key)
            {
            }

            public override State createState()
            {
                return new ExampleState();
            }
        }

        class ExampleState : State<ExampleApp>
        {
            int counter;

<<<<<<< HEAD
         class HomeScreen : StatelessWidget
         {
             public override Widget build(BuildContext context)
             {
                 /*return new CupertinoPageScaffold(
                     child: new Center(
                         child: new Text("hello world!",
                         style: CupertinoTheme.of(context).textTheme.navTitleTextStyle)
                     ),
                     backgroundColor: Colors.brown
                 );*/
                 List<BottomNavigationBarItem> items = new List<BottomNavigationBarItem>();
                 items.Add( new BottomNavigationBarItem(
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
                             builder:(contex1) =>
                             {
                                 return new CupertinoPageScaffold(
                                     navigationBar: new CupertinoNavigationBar(
                                         middle:(index==0)? new Text("views") : new Text("articles") 
                                         ), 
                                     child: new Center(
                                         /*child: new Text(
                                             "THIS IS TAB #" + index, 
                                             style: CupertinoTheme.of(contex1)
                                                 .textTheme
                                                 .navTitleTextStyle
                                                 //.copyWith(fontSize:32)
                                         )*/
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
                                                         return new DetailScreen1(index==0? "views" : "articles" );
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
             public DetailScreen1(string topic)
             {
                 this.topic = topic;
                 
             }

             public string topic;
             public override Widget build(BuildContext context)
             {
                 return new CupertinoPageScaffold(
                     navigationBar: new CupertinoNavigationBar(
                         //middle: new Text("Details")
                     ),
                     child: new Center(
                         child: new Text("hello world")
                     )
                 );
             }
         }
         public class DetailScreen : StatefulWidget
         {
             public DetailScreen(string topic)
             {
                 this.topic = topic;
             }
             public string topic;
            

             public override State createState()
             {
                 return new DetailScreenState();
             }
         }

         public class DetailScreenState : State<DetailScreen>
         {
             public bool switchValue = false;
             public override Widget build(BuildContext context)
             {
                 var widgets = new List<Widget>();
                 widgets.Add( new Expanded(child : new Text("a switch")));
                 widgets.Add(new CupertinoSwitch(
                     value: switchValue,
                     onChanged: value =>
                     {
                         setState(() => switchValue = value);
                     }
                 ));
                 var rowWidgtes = new List<Widget>();
                 var row = new Row(children:widgets);
                 //rowWidgtes.Add(row);
                 
                 var cupBtn = new CupertinoButton(
                     child: new Text("launch action sheet"),
                     onPressed: () =>
                     {
                         CupertinoRouteUtils.showCupertinoModalPopup(
                             context: context,
                             builder: (context1) =>
                             {
                                 return new CupertinoActionSheet(
                                     title: new Text("some choices"),
                                     actions: new List<Widget>(){ 
                                         new CupertinoActionSheetAction(
                                             child: new Text("one"),
                                             onPressed: () =>
                                             {
                                                 Navigator.pop(context1, 1);
                                             },
                                             isDefaultAction: true
                                         ),
                                         new CupertinoActionSheetAction(
                                             child: new Text("two"),
                                             onPressed: () =>
                                             {
                                                 Navigator.pop(context1, 2);
                                             }
                                         ),
                                         new CupertinoActionSheetAction(
                                             child: new Text("three"),
                                             onPressed: () =>
                                             {
                                                 Navigator.pop(context1, 3);
                                             }
                                         )
                                     }
                                 );
                             }
                         );
                     }
                 );
                rowWidgtes.Add(cupBtn);
                 
                 return new CupertinoPageScaffold(
                     child: new Center(
                         child: new Column(
                             mainAxisSize: MainAxisSize.min,
                             crossAxisAlignment: CrossAxisAlignment.center,
                             children:rowWidgtes
                         )
                     )
                     /*,
                     navigationBar: new CupertinoNavigationBar(
                         middle: new Text("hello world") 
                     )*/
                     
                     
                     
                     
                 );
             }
         }
=======
            public override Widget build(BuildContext context)
            {
                return new Container(
                    color: Colors.green,
                    child: new Column(
                        children: new List<Widget>
                        {
                            new Text("Counter: " + counter,
                                style: new TextStyle(fontSize: 18, fontWeight: FontWeight.w100)),
                            new Text("Counter: " + counter,
                                style: new TextStyle(fontFamily: "racher", fontSize: 18, fontWeight: FontWeight.w100)),
                            new Text("Counter: " + counter,
                                style: new TextStyle(fontFamily: "roboto", fontSize: 18, fontWeight: FontWeight.w200)),
                            new Text("Counter: " + counter,
                                style: new TextStyle(fontFamily: "roboto", fontSize: 18, fontWeight: FontWeight.w300)),
                            new Text("Counter: " + counter,
                                style: new TextStyle(fontFamily: "roboto", fontSize: 18, fontWeight: FontWeight.w400)),
                            new Text("Counter: " + counter,
                                style: new TextStyle(fontFamily: "roboto", fontSize: 18, fontWeight: FontWeight.w500)),
                            new Text("Counter: " + counter,
                                style: new TextStyle(fontFamily: "roboto", fontSize: 18, fontWeight: FontWeight.w600)),
                            new Text("Counter: " + counter,
                                style: new TextStyle(fontFamily: "roboto", fontSize: 18, fontWeight: FontWeight.w700)),
                            new Text("Counter: " + counter,
                                style: new TextStyle(fontFamily: "roboto", fontSize: 18, fontWeight: FontWeight.w800)),
                            new Text("Counter: " + counter,
                                style: new TextStyle(fontFamily: "roboto", fontSize: 18, fontWeight: FontWeight.w900)),
                            new Text("Counter: " + counter,
                                style: new TextStyle(fontFamily: "roboto", fontSize: 18, fontWeight: FontWeight.w900,
                                    fontStyle: FontStyle.italic)),
                            new Text("Counter: " + counter,
                                style: new TextStyle(fontFamily: "roboto", fontSize: 18, fontWeight: FontWeight.w100)),
                            new Text("Counter: " + counter,
                                style: new TextStyle(fontFamily: "roboto", fontSize: 18, fontWeight: FontWeight.w900)),
                            new Text("-----"),
                            new Text("Counter: " + counter,
                                style: new TextStyle(fontFamily: "robotox", fontSize: 18, fontWeight: FontWeight.w200)),
                            new Text("Counter: " + counter,
                                style: new TextStyle(fontFamily: "robotox", fontSize: 18, fontWeight: FontWeight.w300)),
                            new Text("Counter: " + counter,
                                style: new TextStyle(fontFamily: "robotox", fontSize: 18, fontWeight: FontWeight.w400)),
                            new Text("Counter: " + counter,
                                style: new TextStyle(fontFamily: "robotox", fontSize: 18, fontWeight: FontWeight.w500)),
                            new Text("Counter: " + counter,
                                style: new TextStyle(fontFamily: "robotox", fontSize: 18, fontWeight: FontWeight.w600)),
                            new Text("Counter: " + counter,
                                style: new TextStyle(fontFamily: "robotox", fontSize: 18, fontWeight: FontWeight.w700)),
                            new Text("Counter: " + counter,
                                style: new TextStyle(fontFamily: "robotox", fontSize: 18, fontWeight: FontWeight.w800)),
                            new Text("Counter: " + counter,
                                style: new TextStyle(fontFamily: "robotox", fontSize: 18, fontWeight: FontWeight.w900)),
                            new Text("Counter: " + counter,
                                style: new TextStyle(fontFamily: "robotox", fontSize: 18, fontWeight: FontWeight.w900,
                                    fontStyle: FontStyle.italic)),
                            new Text("Counter: " + counter + (char) 0xf472 + (char) 0xf442 + (char) 0xf43b,
                                style: new TextStyle(fontFamily: "CupertinoIcons", fontSize: 18)),

                            new GestureDetector(
                                onTap: () =>
                                {
                                    setState(()
                                        =>
                                    {
                                        counter++;
                                    });
                                },
                                child: new Container(
                                    padding: EdgeInsets.symmetric(20, 20),
                                    color: counter % 2 == 0 ? Colors.blue : Colors.red,
                                    child: new Text("Click Me",
                                        style: new TextStyle(fontFamily: "racher", fontWeight: FontWeight.w100))
                                )
                            )
                        }
                    )
                );
            }
        }
>>>>>>> parent of f5669e93... add cupertino and sth goes wrong
    }
}