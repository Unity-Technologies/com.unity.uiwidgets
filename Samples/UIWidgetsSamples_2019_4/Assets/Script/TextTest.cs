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
                    home: new HomeScreen()//new DetailScreen1("ok")
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
                return new CupertinoPickerDemo();
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
