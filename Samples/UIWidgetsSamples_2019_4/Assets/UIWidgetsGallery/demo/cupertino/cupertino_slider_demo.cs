using System.Collections.Generic;
using Unity.UIWidgets.cupertino;
using Unity.UIWidgets.rendering;
using Unity.UIWidgets.widgets;

namespace UIWidgetsGallery.gallery {
    public class CupertinoSliderDemo : StatefulWidget {
        public static string routeName = "/cupertino/slider";

        public override State createState() {
            return new _CupertinoSliderDemoState();
        }
    }

    public class _CupertinoSliderDemoState : State<CupertinoSliderDemo> {
      float _value = 25.0f;
      float _discreteValue = 20.0f;

      public override Widget build(BuildContext context)
      {
        return new CupertinoPageScaffold(
          navigationBar: new CupertinoNavigationBar(
            middle: new Text("Sliders"),
            previousPageTitle: "Cupertino"
          ),
          child: new DefaultTextStyle(
            style: CupertinoTheme.of(context).textTheme.textStyle,
            child: new SafeArea(
              child: new Center(
                child: new Column(
                  mainAxisAlignment: MainAxisAlignment.spaceAround,
                  children: new List< Widget >{
                  new Column(
                    mainAxisSize: MainAxisSize.min,
                    children: new List< Widget >{
                      new CupertinoSlider(
                        value: _value,
                        min: 0.0f,
                        max: 100.0f,
                        onChanged: (float value) => { setState(() => { _value = value; }); }
                      ),
                      new Text($"Cupertino Continuous: {_value:F1}"),
                    }
                  ),
                  new Column(
                    mainAxisSize: MainAxisSize.min,
                    children: new List< Widget >{
                      new CupertinoSlider(
                        value: _discreteValue,
                        min: 0.0f,
                        max: 100.0f,
                        divisions: 5,
                        onChanged: (float value) => { setState(() => { _discreteValue = value; }); }
                      ),
                      new Text($"Cupertino Discrete: {_discreteValue}")
                    }
                  )
                }
              )
            )
          )
        )
      );
    }
  }

}