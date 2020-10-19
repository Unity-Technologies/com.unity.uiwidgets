 using System.Collections.Generic;
 using Unity.UIWidgets.animation;
 using Unity.UIWidgets.engine;
 using Unity.UIWidgets.engine2;
 using Unity.UIWidgets.foundation;
 using Unity.UIWidgets.material;
 using Unity.UIWidgets.painting;
 using Unity.UIWidgets.ui;
 using Unity.UIWidgets.widgets;
 using UnityEngine;
 using FontStyle = Unity.UIWidgets.ui.FontStyle;
 using ui_ = Unity.UIWidgets.widgets.ui_;

 namespace UIWidgetsSample {
     public class UIWidgetsExample : UIWidgetsPanel {
        
         
         protected void OnEnable() {
             // if you want to use your own font or font icons.
             // FontManager.instance.addFont(Resources.Load<Font>(path: "path to your font"), "font family name");

             // load custom font with weight & style. The font weight & style corresponds to fontWeight, fontStyle of
             // a TextStyle object
             // FontManager.instance.addFont(Resources.Load<Font>(path: "path to your font"), "Roboto", FontWeight.w500,
             //    FontStyle.italic);

             // add material icons, familyName must be "Material Icons"
             // FontManager.instance.addFont(Resources.Load<Font>(path: "path to material icons"), "Material Icons");

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
                     pageRouteBuilder: (RouteSettings settings, WidgetBuilder builder) =>
                         new PageRouteBuilder(
                             settings: settings,
                             pageBuilder: (BuildContext Buildcontext, Animation<float> animation,
                                 Animation<float> secondaryAnimation) => builder(context)
                         )
                 );
             }
         }
         // protected override Widget createWidget() {
         //     return new WidgetsApp(
         //         home: new ExampleApp(),
         //         pageRouteBuilder: (RouteSettings settings, WidgetBuilder builder) =>
         //             new PageRouteBuilder(
         //                 settings: settings,
         //                 pageBuilder: (BuildContext context, Animation<float> animation,
         //                     Animation<float> secondaryAnimation) => builder(context)
         //             )
         //     );
         // }

         class ExampleApp : StatefulWidget {
             public ExampleApp(Key key = null) : base(key) {
             }

             public override State createState() {
                 return new ExampleState();
             }
         }

         class ExampleState : State<ExampleApp> {
             int counter = 0;

             public override Widget build(BuildContext context) {
                 return new Column(
                     children: new List<Widget> {
                         new Text("Counter: " + this.counter),
                         new GestureDetector(
                             onTap: () => {
                                 this.setState(()
                                     => {
                                     this.counter++;
                                 });
                             },
                             child: new Container(
                                 padding: EdgeInsets.symmetric(20, 20),
                                 color: Colors.blue,
                                 child: new Text("Click Me")
                             )
                         )
                     }
                 );
             }
         }
     }
 }