using System;
using System.Collections;
using System.Collections.Generic;
using Unity.UIWidgets.engine;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.gestures;
using Unity.UIWidgets.material;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.rendering;
using Unity.UIWidgets.ui;
using Unity.UIWidgets.widgets;
using UnityEngine;

public class LocalPositionSample : UIWidgetsPanel
{
    protected override void OnEnable()
    {
        base.OnEnable();
    }

    protected override Widget createWidget()
    {
        {
            return new MaterialApp(
                title: "Flutter Demo",
                theme: new ThemeData(
                    primarySwatch: Colors.blue
                ),
                home: new MyHomePage(title: "Local Position Test"));
        }
    }

    public class MyHomePage : StatefulWidget
    {
        public MyHomePage(string title, Key key = null) : base(key: key)
        {
            this.title = title;
        }

        public readonly string title;

        public override State createState()
        {
            return new _MyHomePageState();
        }
    }

    public class _MyHomePageState : State<MyHomePage>
    {
        Offset _globalPostion = Offset.zero;
        Offset _localPostion = Offset.zero;

        void _checkPosition(TapUpDetails tapUpDetails)
        {
            setState(() =>
            {
                _globalPostion = tapUpDetails.globalPosition;
                _localPostion = tapUpDetails.localPosition;
            });
        }

        void _checkPosition(TapDownDetails tapUpDetails)
        {
            setState(() =>
            {
                _globalPostion = tapUpDetails.globalPosition;
                _localPostion = tapUpDetails.localPosition;
            });
        }

        public override Widget build(BuildContext context)
        {
            return new Scaffold(
                appBar: new AppBar(
                    title: new Text(widget.title)
                ),
                body: new Center(
                    child: new Column(
                        mainAxisAlignment: MainAxisAlignment.center,
                        children: new List<Widget>()
                        {
                            new Text(
                                "Tap to get global position and local position:"
                            ),
                            new Text(
                                $"global position: {_globalPostion}",
                                style: Theme.of(context).textTheme.headline
                            ),
                            new Text(
                                $"local position: {_localPostion}",
                                style: Theme.of(context).textTheme.headline
                            ),
                            new GestureDetector(
                                onTapUp: _checkPosition,
                                child: new Container(
                                    margin: EdgeInsets.all(10),
                                    padding: EdgeInsets.all(10),
                                    decoration: new BoxDecoration(color: Colors.red),
                                    child: new Text("On Tap Up")
                                )
                            ),
                            new GestureDetector(
                                onTapDown: _checkPosition,
                                child: new Container(
                                    margin: EdgeInsets.all(10),
                                    padding: EdgeInsets.all(30),
                                    decoration: new BoxDecoration(Colors.orange),
                                    child: new Text("on Tap Down")
                                )
                            ),
                            new GestureDetector(
                                onTapDown: _checkPosition,
                                child: new Container(
                                    padding: EdgeInsets.all(30),
                                    decoration: new BoxDecoration(Colors.pink),
                                    child: new Text("on Tap Down with 0 margin")
                                )
                            )
                        }
                    )
                )
            );
        }
    }
}