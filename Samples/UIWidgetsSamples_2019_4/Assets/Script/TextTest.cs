﻿using System.Collections.Generic;
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
using Unity.UIWidgets.gestures;
using Unity.UIWidgets.material;
using UnityEngine.Networking;
using Color = Unity.UIWidgets.ui.Color;
using Random = UnityEngine.Random;
using Transform = Unity.UIWidgets.widgets.Transform;

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
        class MyApp : StatelessWidget {
          public override Widget build(BuildContext context) {
            return new MaterialApp(
              debugShowCheckedModeBanner: false,
              title: "Flutter Demo",
              home: new Scaffold(
                appBar: new AppBar(title: new Text("good")),
                body: new Center(
                    child: new MyStatefulWidget()
                  )
                )
            );
          }
        } 
        class MyStatefulWidget : StatefulWidget {
          public MyStatefulWidget(Key key = null) : base(key: key)
          {
          }

          public override State createState() => new _MyStatefulWidgetState();
        }

        class _MyStatefulWidgetState : State<MyStatefulWidget> {
          Color textColor = Colors.blue;
          int _enterCounter = 0;
          int _exitCounter = 0;
          float x = 0.0f;
          float y = 0.0f;

          void _incrementEnter(PointerEvent details) {
              UnityEngine.Debug.Log("enter");
            setState(() =>{
                
              _enterCounter++;
            });
          }

          void _incrementExit(PointerEvent details) {
            setState(() =>{
              textColor = Colors.blue;
              _exitCounter++;
            });
          }

          void _updateLocation(PointerEvent details) {
            setState(() =>{
              textColor = Colors.red;
              x = details.position.dx;
              y = details.position.dy;
            });
          }


          public override Widget build(BuildContext context) {
              return new MouseRegion(
              onEnter: _incrementEnter,
              onHover: _updateLocation,
              onExit: _incrementExit,
              child: new FlatButton(
                color: Colors.white,
                textColor: Colors.teal[700], //when hovered text color change
                shape: new RoundedRectangleBorder(
                  borderRadius: BorderRadius.circular(5),
                  side: new BorderSide(
                    color: Colors.teal[700]
                  )
                ),
                onPressed: () => { },
                child: new Text("Log in", style: new TextStyle(color: textColor))
              )
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
                return new CupertinoSwitchDemo();
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
                        //backgroundColor: Color.white,
                        child: new Center(
                            child: new Text(
                                "hello world"
                                 //style : new TextStyle(color: CupertinoColors.activeBlue)
                                //style : new TextStyle(color: Color.white)
                            )
                        )
                    );
                }
            }
        }
    }
