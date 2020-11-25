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
//using UIWidgetsGallery.gallery;
using Unity.UIWidgets.service;


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
                                                         return
                                                             new DetailScreen1(index==0? "views" : "articles" );
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
         
         class CupertinoTextFieldDemo : StatefulWidget {
            public const string routeName = "/cupertino/text_fields";

            public override State createState() {
                return new _CupertinoTextFieldDemoState();
        }
    }

    class _CupertinoTextFieldDemoState : State<CupertinoTextFieldDemo> {
        TextEditingController _chatTextController;
        TextEditingController _locationTextController;

        public override void initState() {
            base.initState();
            this._chatTextController = new TextEditingController();
            this._locationTextController = new TextEditingController(text: "Montreal, Canada");
        }

        Widget _buildChatTextField() {
            return new CupertinoTextField(
                controller: this._chatTextController,
                textCapitalization: TextCapitalization.sentences,
                placeholder: "Text Message",
                decoration: new BoxDecoration(
                    border: Border.all(
                        width: 0.0f,
                        color: CupertinoColors.inactiveGray
                    ),
                    borderRadius: BorderRadius.circular(15.0f)
                ),
                maxLines: null,
                keyboardType: TextInputType.multiline,
                prefix: new Padding(padding: EdgeInsets.symmetric(horizontal: 4.0f)),
                suffix:
                new Padding(
                    padding: EdgeInsets.symmetric(horizontal: 4.0f),
                    child: new CupertinoButton(
                        color: CupertinoColors.activeGreen,
                        minSize: 0.0f,
                        child: new Icon(
                            CupertinoIcons.up_arrow,
                            size: 21.0f,
                            color: CupertinoColors.white
                        ),
                        padding: EdgeInsets.all(2.0f),
                        borderRadius:
                        BorderRadius.circular(15.0f),
                        onPressed: () => this.setState(() => this._chatTextController.clear())
                    )
                ),
                autofocus: true,
                suffixMode: OverlayVisibilityMode.editing,
                onSubmitted: (string text) => this.setState(() => this._chatTextController.clear())
            );
        }

        Widget _buildNameField() {
            return new CupertinoTextField(
                prefix: new Icon(
                    CupertinoIcons.person_solid,
                    color: CupertinoColors.lightBackgroundGray,
                    size: 28.0f
                ),
                padding: EdgeInsets.symmetric(horizontal: 6.0f, vertical: 12.0f),
                clearButtonMode: OverlayVisibilityMode.editing,
                textCapitalization: TextCapitalization.words,
                autocorrect: false,
                decoration: new BoxDecoration(
                    border: new Border(bottom: new BorderSide(width: 0.0f, color: CupertinoColors.inactiveGray))
                ),
                placeholder: "Name"
            );
        }

        Widget _buildEmailField() {
            return new CupertinoTextField(
                prefix: new Icon(
                    CupertinoIcons.mail_solid,
                    color: CupertinoColors.lightBackgroundGray,
                    size: 28.0f
                ),
                padding: EdgeInsets.symmetric(horizontal: 6.0f, vertical: 12.0f),
                clearButtonMode: OverlayVisibilityMode.editing,
                keyboardType: TextInputType.emailAddress,
                autocorrect: false,
                decoration: new BoxDecoration(
                    border: new Border(bottom: new BorderSide(width: 0.0f, color: CupertinoColors.inactiveGray))
                ),
                placeholder: "Email"
            );
        }

        Widget _buildLocationField() {
            return new CupertinoTextField(
                controller: this._locationTextController,
                prefix: new Icon(
                    CupertinoIcons.location_solid,
                    color: CupertinoColors.lightBackgroundGray,
                    size: 28.0f
                ),
                padding: EdgeInsets.symmetric(horizontal: 6.0f, vertical: 12.0f),
                clearButtonMode: OverlayVisibilityMode.editing,
                textCapitalization: TextCapitalization.words,
                decoration: new BoxDecoration(
                    border: new Border(bottom: new BorderSide(width: 0.0f, color: CupertinoColors.inactiveGray))
                ),
                placeholder: "Location"
            );
        }

        Widget _buildPinField() {
            return new CupertinoTextField(
                prefix: new Icon(
                    CupertinoIcons.padlock_solid,
                    color: CupertinoColors.lightBackgroundGray,
                    size: 28.0f
                ),
                padding: EdgeInsets.symmetric(horizontal: 6.0f, vertical: 12.0f),
                clearButtonMode: OverlayVisibilityMode.editing,
                keyboardType: TextInputType.number,
                autocorrect: false,
                obscureText: true,
                decoration: new BoxDecoration(
                    border: new Border(bottom: new BorderSide(width: 0.0f, color: CupertinoColors.inactiveGray))
                ),
                placeholder: "Create a PIN"
            );
        }

        Widget _buildTagsField() {
            return new CupertinoTextField(
                controller: new TextEditingController(text: "colleague, reading club"),
                prefix: new Icon(
                    CupertinoIcons.tags_solid,
                    color: CupertinoColors.lightBackgroundGray,
                    size: 28.0f
                ),
                enabled: false,
                padding: EdgeInsets.symmetric(horizontal: 6.0f, vertical: 12.0f),
                decoration: new BoxDecoration(
                    border: new Border(bottom: new BorderSide(width: 0.0f, color: CupertinoColors.inactiveGray))
                )
            );
        }

        public override Widget build(BuildContext context) {
            return new DefaultTextStyle(
                style: new TextStyle(
                    fontFamily: ".SF Pro Text", // ".SF UI Text",
                    inherit: false,
                    fontSize: 17.0f,
                    color: CupertinoColors.black
                ),
                child: new CupertinoPageScaffold(
                    
                    child: new SafeArea(
                        child: new ListView(
                                children: new List<Widget> 
                                {
                                    /*new Padding(
                                        padding: EdgeInsets.symmetric(vertical: 32.0f, horizontal: 16.0f),
                                        child: new Column(
                                            children: new List<Widget> {
                                                this._buildNameField(),
                                                this._buildEmailField(),
                                                this._buildLocationField(),
                                                this._buildPinField(),
                                                this._buildTagsField(),
                                            }
                                        )
                                    ),*/
                                    new Padding(
                                        padding: EdgeInsets.symmetric(vertical: 32.0f, horizontal: 16.0f)//,
                                        //child: this._buildChatTextField()
                                    ),
                                }
                            )
                                
                        )
                    )
                
            );
        }
    }
    }
}