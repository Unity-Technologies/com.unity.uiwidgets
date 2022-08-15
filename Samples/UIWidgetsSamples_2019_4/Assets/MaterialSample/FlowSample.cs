using System;
using System.Collections.Generic;
using System.Linq;
using uiwidgets;
using Unity.UIWidgets.animation;
using Unity.UIWidgets.engine;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.material;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.rendering;
using Unity.UIWidgets.ui;
using Unity.UIWidgets.widgets;
using ui_ = Unity.UIWidgets.widgets.ui_;

namespace UIWidgetsSample
{
    public class FlowSample : UIWidgetsPanel
    {
        protected override void main()
        {
            ui_.runApp(new MaterialApp(
                showPerformanceOverlay: false,
                home: new FlowDemo()));
        }

        protected new void OnEnable()
        {
            base.OnEnable();
        }
        
        protected override void onEnable()
        {
            AddFont("Material Icons", new List<string> {"MaterialIcons-Regular.ttf"}, new List<int> {0});
        }
    }

    public class FlowDemo : StatefulWidget
    {
        public FlowDemo(Key key = null) : base(key)
        {
        }

        public override State createState()
        {
            return new _FlowDemoState();
        }
    }

    class FlowDemoDelegate : FlowDelegate
    {
        public FlowDemoDelegate(Animation<float> myAnimation) : base(repaint: myAnimation)
        {
            this.myAnimation = myAnimation;
        }

        Animation<float> myAnimation;

        public override bool shouldRepaint(FlowDelegate oldDelegate)
        {
            var _oldDelegate = oldDelegate as FlowDemoDelegate;
            return myAnimation != _oldDelegate?.myAnimation;
        }

        public override void paintChildren(FlowPaintingContext context)
        {
            for (int i = context.childCount - 1; i >= 0; i--)
            {
                float dx = (context.getChildSize(i).height + 10) * i;
                context.paintChild(
                    i,
                    transform: Matrix4.translationValues(0, dx * myAnimation.value + 10, 0)
                );
            }
        }

        public override Size getSize(BoxConstraints constraints)
        {
            return new Size(70.0f, float.PositiveInfinity);
        }


        public override BoxConstraints getConstraintsForChild(int i, BoxConstraints constraints)
        {
            return i == 0 ? constraints : BoxConstraints.tight(new Size(50.0f, 50.0f));
        }
    }

    public class _FlowDemoState : SingleTickerProviderStateMixin<FlowDemo>
    {
        private AnimationController _myAnimation;

        List<IconData> _icons = new List<IconData>
        {
            Icons.menu,
            Icons.email,
            Icons.settings,
            Icons.notifications,
            Icons.person,
            Icons.wifi
        };

        public override void initState()
        {
            base.initState();

            _myAnimation = new AnimationController(
                duration: new TimeSpan(0, 0, 0, 1),
                vsync: this
            );
        }

        Widget _buildItem(IconData icon)
        {
            return new Padding(
                padding: EdgeInsets.symmetric(horizontal: 10.0f),
                child: new RawMaterialButton(
                    fillColor: Colors.cyan,
                    shape: new CircleBorder(),
                    constraints: BoxConstraints.tight(Size.square(50.0f)),
                    onPressed: () =>
                    {
                        if (_myAnimation.status == AnimationStatus.completed)
                        {
                            _myAnimation.reverse();
                        }
                        else
                        {
                            _myAnimation.forward();
                        }
                    },
                    child: new Icon(
                        icon,
                        color: Colors.white,
                        size: 30.0f
                    )
                )
            );
        }

        public override Widget build(BuildContext context)
        {
            return new Scaffold(
                appBar: new AppBar(
                    automaticallyImplyLeading: false,
                    title: new Text("Flutter Flow Widget Demo"),
                    backgroundColor: Colors.cyan
                ),
                body: new Stack(
                    children: new List<Widget>
                    {
                        new Container(color: Colors.grey[200]),
                        new Flow(
                            _delegate: new FlowDemoDelegate(myAnimation: _myAnimation),
                            children: _icons
                                .Select((IconData icon) => _buildItem(icon)).ToList()
                        ),
                    }
                )
            );
        }
    }
}