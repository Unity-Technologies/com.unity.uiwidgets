using System.Collections.Generic;
using uiwidgets;
using Unity.UIWidgets.cupertino;
using Unity.UIWidgets.engine;
using Unity.UIWidgets.ui;
using Unity.UIWidgets.widgets;
using Text = Unity.UIWidgets.widgets.Text;
using ui_ = Unity.UIWidgets.widgets.ui_;
using TextStyle = Unity.UIWidgets.painting.TextStyle;

namespace UIWidgetsSample
{
    public class CountDemo : UIWidgetsPanel
    {
        protected void OnEnable()
        {
            base.OnEnable();
        }

        protected override void main()
        {
            ui_.runApp(new MyCountApp());
        }
    }

    public class MyCountApp : StatelessWidget
    {
        public override Widget build(BuildContext context)
        {
            return new CupertinoApp(
                home: new CounterApp()
            );
        }
    }

    internal class CounterApp : StatefulWidget
    {
        public override State createState()
        {
            return new CountDemoState();
        }
    }

    class TestButton : StatefulWidget
    {
        public readonly Color onPressed;
        public readonly Color onReleased;
        public readonly string text;

        public TestButton(Color onPressed, Color onReleased, string text)
        {
            this.onPressed = onPressed;
            this.onReleased = onReleased;
            this.text = text;
        }

        public override State createState()
        {
            return new TestButtonState();
        }
    }

    class TestButtonState : State<TestButton>
    {
        private Color myColor;

        public override void initState()
        {
            base.initState();
            myColor = widget.onReleased;
        }

        public override Widget build(BuildContext context)
        {
            return new GestureDetector(
                child: new Container(
                    color: myColor,
                    width: 100,
                    height: 40,
                    child: new Text(widget.text)
                ),
                onTapDown: evt =>
                {
                    setState(() =>
                    {
                        myColor = widget.onPressed;
                    });
                },
                onTapUp: evt =>
                {
                    setState(() =>
                    {
                        myColor = widget.onReleased;
                    });
                }
            );
        }
    }

    internal class CountDemoState : State<CounterApp>
    {
        private bool useComposite = true;
        private int count = 0;

        private Color myColor = Colors.grey;

        public override Widget build(BuildContext context)
        {
            return new Container(
                color: Color.fromARGB(255, 255, 0, 0),
                child: new Column(children: new List<Widget>()
                    {
                        new Text($"count: {count}", style: new TextStyle(color: Color.fromARGB(255, 0 ,0 ,255))),
                        new Icon(CupertinoIcons.battery_charging, color: Colors.yellow),
                        new Text($"count: {count}", style: new TextStyle(fontFamily: "CupertixnoIcons", color: Color.fromARGB(255, 0 ,0 ,255))),
                        new Text($"count: {count}", style: new TextStyle(fontFamily: "CupertinoIcons", color: Color.fromARGB(255, 0 ,0 ,255))),
                        useComposite ? (Widget)new TestButton(Colors.green, Colors.red, "TestButton") :
                            new GestureDetector(
                                child: new Container(
                                    color: myColor,
                                    width: 100,
                                    height: 40,
                                    child: new Text("TestButton2")
                                ),
                                onTapDown: evt =>
                                {
                                    setState(() =>
                                    {
                                        myColor = Colors.blue;
                                    });
                                },
                                onTapUp: evt =>
                                {
                                    setState(() =>
                                    {
                                        myColor = Colors.grey;
                                    });
                                }
                            ) 
                    }
                )
            );
        }
    }
}