using System.Collections.Generic;
using Unity.UIWidgets.engine2;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.material;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.ui;
using Unity.UIWidgets.widgets;
using TextStyle = Unity.UIWidgets.painting.TextStyle;
using ui_ = Unity.UIWidgets.widgets.ui_;
using Unity.UIWidgets.cupertino;
using Unity.UIWidgets.rendering;

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
                 return new CupertinoApp(
                     home: new HomeScreen()
                 );
                 /*return new WidgetsApp(
                     home: new HomeScreen(),
                     
                     pageRouteBuilder: (RouteSettings settings, WidgetBuilder builder) =>
                         new PageRouteBuilder(
                             settings: settings,
                             pageBuilder: (BuildContext Buildcontext, Animation<float> animation,
                                 Animation<float> secondaryAnimation) => builder(context)
                         )
                 );*/
                
             }
         }

         

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
    }
}