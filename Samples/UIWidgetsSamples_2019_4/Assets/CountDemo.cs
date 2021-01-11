using System.Collections.Generic;
using Unity.UIWidgets.cupertino;
using Unity.UIWidgets.engine2;
using Unity.UIWidgets.ui;
using Unity.UIWidgets.widgets;
using UnityEngine.UI;
using Text = Unity.UIWidgets.widgets.Text;
using ui_ = Unity.UIWidgets.widgets.ui_;

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
            ui_.runApp(new MyApp());
        }

        class MyApp : StatelessWidget
        {
            public override Widget build(BuildContext context)
            {
                return new CupertinoApp(
                    home: new CounterApp()
                );
            }
        }
    }

    internal class CounterApp : StatefulWidget
    {
        public override State createState()
        {
            return new CountDemoState();
        }
    }

    internal class CountDemoState : State<CounterApp>
    {
        private int count = 0;

        public override Widget build(BuildContext context)
        {
            return new Container(
                color: Color.white,
                child: new Column(children: new List<Widget>()
                    {
                        new Text($"count: {count}"),
                        new CupertinoButton(
                            onPressed: () =>
                            {
                                setState(() =>
                                {
                                    count++;
                                });
                            },
                            child: new Container(
                                color: Color.black,
                                width: 100,
                                height: 40
                            )
                        ),
                    }
                )
            );
        }
    }
}