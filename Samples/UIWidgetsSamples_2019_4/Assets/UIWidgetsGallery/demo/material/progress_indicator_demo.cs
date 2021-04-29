using System;
using System.Collections.Generic;
using System.Linq;
using UIWidgetsGallery.gallery;
using Unity.UIWidgets.animation;
using Unity.UIWidgets.material;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.rendering;
using Unity.UIWidgets.ui;
using Unity.UIWidgets.widgets;

namespace UIWidgetsGallery.demo.material
{
    class ProgressIndicatorDemo : StatefulWidget {
    public static readonly string routeName = "/material/progress-indicator";

        public override State createState() => new _ProgressIndicatorDemoState();
    }
    
    class _ProgressIndicatorDemoState : SingleTickerProviderStateMixin<ProgressIndicatorDemo> {
  AnimationController _controller;
  Animation<float> _animation;

  public override void initState() {
    base.initState();
    _controller = new AnimationController(
      duration: new TimeSpan(0, 0, 0, 0, 1500),
      vsync: this,
      animationBehavior: AnimationBehavior.preserve
    );
    _controller.forward();

    _animation = new CurvedAnimation(
      parent: _controller,
      curve:  new Interval(0.0f, 0.9f, curve: Curves.fastOutSlowIn),
      reverseCurve: Curves.fastOutSlowIn
    );
      _animation.addStatusListener((AnimationStatus status) => {
      if (status == AnimationStatus.dismissed)
        _controller.forward();
      else if (status == AnimationStatus.completed)
        _controller.reverse();
    });
  }

  public override void dispose() {
    _controller.stop();
    base.dispose();
  }

  void _handleTap() {
    setState(() => {
      // valueAnimation.isAnimating is part of our build state
      if (_controller.isAnimating) {
        _controller.stop();
      } else {
        switch (_controller.status) {
          case AnimationStatus.dismissed:
          case AnimationStatus.forward:
            _controller.forward();
            break;
          case AnimationStatus.reverse:
          case AnimationStatus.completed:
            _controller.reverse();
            break;
        }
      }
    });
  }

  Widget _buildIndicators(BuildContext context, Widget child)
  {
    List<Widget> indicators = new List<Widget>
    {
      new SizedBox(
        width: 200.0f,
        child: new LinearProgressIndicator()
      ),
      new LinearProgressIndicator(),
      new LinearProgressIndicator(),
      new LinearProgressIndicator(value: _animation.value),
      new Row(
        mainAxisAlignment: MainAxisAlignment.spaceEvenly,
        children: new List<Widget>
        {
          new CircularProgressIndicator(),
          new SizedBox(
            width: 20.0f,
            height: 20.0f,
            child: new CircularProgressIndicator(value: _animation.value)
          ),
          new SizedBox(
            width: 100.0f,
            height: 20.0f,
            child: new Text($"{(_animation.value * 100.0):F1}%",
              textAlign: TextAlign.right
            )
          )
        }
      )
    };
    return new Column(
      children: indicators
        .Select<Widget, Widget>((Widget c) => new Container(child: c, margin: EdgeInsets.symmetric(vertical: 15.0f, horizontal: 20.0f)))
        .ToList()
    );
  }

  public override Widget build(BuildContext context) {
    return new Scaffold(
      appBar: new AppBar(
        title: new Text("Progress indicators"),
        actions: new List<Widget>{new MaterialDemoDocumentationButton(ProgressIndicatorDemo.routeName)}
      ),
      body: new Center(
        child: new SingleChildScrollView(
          child: new DefaultTextStyle(
            style: Theme.of(context).textTheme.headline6,
            child: new GestureDetector(
              onTap: _handleTap,
              behavior: HitTestBehavior.opaque,
              child: new SafeArea(
                top: false,
                bottom: false,
                child: new Container(
                  padding: EdgeInsets.symmetric(vertical: 12.0f, horizontal: 8.0f),
                  child: new AnimatedBuilder(
                    animation: _animation,
                    builder: _buildIndicators
                  )
                )
              )
            )
          )
        )
      )
    );
  }
}
}