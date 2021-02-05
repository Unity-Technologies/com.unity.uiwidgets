using System.Collections.Generic;
using uiwidgets;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.material;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.ui;
using Unity.UIWidgets.widgets;
using Image = Unity.UIWidgets.widgets.Image;

namespace UIWidgetsGallery.demo.shrine
{
    public class LoginPage : StatefulWidget {
  
  public override State createState() => new _LoginPageState();
}

public class _LoginPageState : State<LoginPage> {
  public readonly TextEditingController _usernameController = new TextEditingController();
  public readonly TextEditingController _passwordController = new TextEditingController();
  static ShapeDecoration _decoration =  new ShapeDecoration(
    shape: new BeveledRectangleBorder(
      side: new BorderSide(color: shrineColorsUtils.kShrineBrown900, width: 0.5f),
      borderRadius: BorderRadius.all(Radius.circular(7.0f))
    )
  );
  
  public override Widget build(BuildContext context) {
    return new Scaffold(
      appBar: new AppBar(
        elevation: 0.0f,
        backgroundColor: Colors.white,
        brightness: Brightness.light,
        leading: new IconButton(
          icon: new BackButtonIcon(),
          tooltip: MaterialLocalizations.of(context).backButtonTooltip,
          onPressed: () => {
            Navigator.of(context, rootNavigator: true).pop<object>();
          }
        )
      ),
      body: new SafeArea(
        child: new ListView(
          padding: EdgeInsets.symmetric(horizontal: 24.0f),
          children: new List<Widget>{
            new SizedBox(height: 80.0f),
            new Column(
              children: new List<Widget>
              {
                Image.file("shrine_images/diamond.png"),
                new SizedBox(height: 16.0f),
                new Text(
                  "SHRINE",
                  style: Theme.of(context).textTheme.headline5
                ),
              }
            ),
            new SizedBox(height: 120.0f),
            new PrimaryColorOverride(
              color: shrineColorsUtils.kShrineBrown900,
              child: new Container(
                decoration: _decoration,
                child: new TextField(
                  controller: _usernameController,
                  decoration: new InputDecoration(
                    labelText: "Username"
                  )
                )
              )
            ),
            new SizedBox(height: 12.0f),
            new PrimaryColorOverride(
              color: shrineColorsUtils.kShrineBrown900,
              child: new Container(
                decoration: _decoration,
                child: new TextField(
                  controller: _passwordController,
                  decoration: new InputDecoration(
                    labelText: "Password"
                  )
                )
              )
            ),
            new Wrap(
              children: new List<Widget>{
                new ButtonBar(
                  children:new List<Widget>{
                    new FlatButton(
                      child: new Text("CANCEL"),
                      shape: new BeveledRectangleBorder(
                        borderRadius: BorderRadius.all(Radius.circular(7.0f))
                      ),
                      onPressed: () => {
                        Navigator.of(context, rootNavigator: true).pop<object>();
                      }
                    ),
                    new RaisedButton(
                      child: new Text("NEXT"),
                      elevation: 8.0f,
                      shape: new BeveledRectangleBorder(
                        borderRadius: BorderRadius.all(Radius.circular(7.0f))
                      ),
                      onPressed: () => {
                        Navigator.pop(context);
                      }
                    )
                  }
                ),
              }
            ),
          }
        )
      )
    );
  }
}

      public class PrimaryColorOverride : StatelessWidget {
      public PrimaryColorOverride(Key key = null, Color color = null, Widget child = null) : base(key: key)
      {
        this.child = child;
        this.color = color;
      }

      public readonly Color color;
      public readonly Widget child;

     
      public override Widget build(BuildContext context) {
        return new Theme(
          child: child,
          data: Theme.of(context).copyWith(primaryColor: color)
        );
      }
    }

}