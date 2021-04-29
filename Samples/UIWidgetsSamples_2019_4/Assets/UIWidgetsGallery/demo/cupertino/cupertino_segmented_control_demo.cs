using System;
using System.Collections.Generic;
using uiwidgets;
using Unity.UIWidgets.async;
using Unity.UIWidgets.cupertino;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.rendering;
using Unity.UIWidgets.ui;
using Unity.UIWidgets.widgets;
using TextStyle = Unity.UIWidgets.painting.TextStyle;
using Random = System.Random;
using Unity.UIWidgets.material;

namespace UIWidgetsGallery.gallery
{
  public class CupertinoSegmentedControlDemo : StatefulWidget {
    public static string routeName = "cupertino/segmented_control";

    public override State createState()
    {
      return new _CupertinoSegmentedControlDemoState();
      
    }
  } 
  public class _CupertinoSegmentedControlDemoState : State<CupertinoSegmentedControlDemo> { 
    Color _kKeyUmbraOpacity = new Color(0x33000000); // alpha = 0.2
    Color _kKeyPenumbraOpacity = new Color(0x24000000); // alpha = 0.14
    Color _kAmbientShadowOpacity = new Color(0x1F000000); // alpha = 0.12
    public readonly Dictionary<int, Widget> children = new Dictionary<int, Widget>{
      {0, new Text("Midnight")},
      {1, new Text("Viridian")},
      {2, new Text("Cerulean")},
    };
    public readonly Dictionary<int, Widget> icons = new Dictionary<int, Widget>{
      {0, new Center(
        child: new Icon(
          CupertinoIcons.home,
          color: Colors.indigo,
          size: 200.0f
          
        )
      )},
      {1, new Center(
        child: new Icon(
          CupertinoIcons.bell,
          color: Colors.teal,
          size: 200.0f
        )
      )},
      {2, new Center(
        child: new Icon(
          CupertinoIcons.back,
          color: Colors.cyan,
          size: 200.0f
        )
      )},
    };
    int currentSegment = 0;
    void onValueChanged(int newValue) {
      setState(()=> {
        currentSegment = newValue;
      });
    }
    public override Widget build(BuildContext context) {
      return new CupertinoPageScaffold(
        navigationBar: new CupertinoNavigationBar(
          middle: new Text("Segmented Control"),
          previousPageTitle: "Cupertino"
         
        ),
        child: new DefaultTextStyle(
          style: CupertinoTheme.of(context).textTheme.textStyle.copyWith(fontSize: 13),
          child: new SafeArea(
            child: new Column(
              children: new List<Widget>{
                new Padding(padding: EdgeInsets.all(16.0f)),
                new SizedBox(
                  width: 500.0f,
                  child: new CupertinoSegmentedControl<int>(
                    children: children,
                    onValueChanged: onValueChanged,
                    groupValue: currentSegment
                  )
                ),
                new SizedBox(
                  width: 500,
                  child: new Padding(
                    padding: EdgeInsets.all(16.0f),
                    child: new CupertinoSlidingSegmentedControl<int>(
                      children: children,
                      onValueChanged: onValueChanged,
                      groupValue: currentSegment
                    )
                  )
                ),
                new Expanded(
                  child:  new Padding(
                    padding: EdgeInsets.symmetric(
                      vertical: 32.0f,
                      horizontal: 16.0f
                    ),
                    child: new CupertinoUserInterfaceLevel(
                      data: CupertinoUserInterfaceLevelData.elevatedlayer,
                      child: new Builder(
                        builder: (BuildContext context1) =>{
                          return new Container(
                            padding: EdgeInsets.symmetric(
                              vertical: 64.0f,
                              horizontal: 16.0f
                            ),
                            decoration:  new BoxDecoration(
                              color: CupertinoTheme.of(context1).scaffoldBackgroundColor,
                              borderRadius: BorderRadius.circular(3.0f),
                              boxShadow: new List<BoxShadow>{
                                new BoxShadow(
                                  offset: new Offset(0.0f, 3.0f),
                                  blurRadius: 5.0f,
                                  spreadRadius: -1.0f,
                                  color: _kKeyUmbraOpacity
                                ),
                                new BoxShadow(
                                  offset: new Offset(0.0f, 6.0f),
                                  blurRadius: 10.0f,
                                  spreadRadius: 0.0f,
                                  color: _kKeyPenumbraOpacity
                                ),
                                new BoxShadow(
                                  offset: new Offset(0.0f, 1.0f),
                                  blurRadius: 18.0f,
                                  spreadRadius: 0.0f,
                                  color: _kAmbientShadowOpacity
                                )
                              }
                            ),
                            child: icons[currentSegment]
                          );
                        }
                      )
                    )
                  )
                )
              }
            )
          )
        )
      );
    }
  }

    
}