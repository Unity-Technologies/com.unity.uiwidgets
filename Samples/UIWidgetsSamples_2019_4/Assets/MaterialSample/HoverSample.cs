using Unity.UIWidgets.engine;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.widgets;
using TextStyle = Unity.UIWidgets.painting.TextStyle;
using ui_ = Unity.UIWidgets.widgets.ui_;
using uiwidgets;
using Unity.UIWidgets.gestures;
using Unity.UIWidgets.material;
using Color = Unity.UIWidgets.ui.Color;

namespace UIWidgetsSample
{
  public class HoverSample : UIWidgetsPanel
  {
    protected override void main()
    {
      ui_.runApp(new MyApp());
    }

    class MyApp : StatelessWidget
    {
      public override Widget build(BuildContext context)
      {
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

    class MyStatefulWidget : StatefulWidget
    {
      public MyStatefulWidget(Key key = null) : base(key: key)
      {
      }

      public override State createState() => new _MyStatefulWidgetState();
    }

    class _MyStatefulWidgetState : State<MyStatefulWidget>
    {
      Color textColor = Colors.blue;
      int _enterCounter = 0;
      int _exitCounter = 0;
      float x = 0.0f;
      float y = 0.0f;

      void _incrementEnter(PointerEvent details)
      {
        UnityEngine.Debug.Log("enter");
        setState(() => { _enterCounter++; });
      }

      void _incrementExit(PointerEvent details)
      {
        setState(() =>
        {
          textColor = Colors.blue;
          _exitCounter++;
        });
      }

      void _updateLocation(PointerEvent details)
      {
        setState(() =>
        {
          textColor = Colors.red;
          x = details.position.dx;
          y = details.position.dy;
        });
      }


      public override Widget build(BuildContext context)
      {
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
  }
}
