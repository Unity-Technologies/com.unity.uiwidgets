using System;
using System.Collections.Generic;
using System.Linq;
using UIWidgetsGallery.gallery;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.material;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.rendering;
using Unity.UIWidgets.ui;
using Unity.UIWidgets.widgets;
using TextStyle = Unity.UIWidgets.painting.TextStyle;

namespace UIWidgetsGallery.demo.material
{
    enum DialogDemoAction {
        cancel,
        discard,
        disagree,
        agree,
    }

    public static class GalleryDialogDemoUtils
    {
        public static readonly string _alertWithoutTitleText = "Discard draft?";

        public static readonly string _alertWithTitleText =
            "Let Google help apps determine location. This means sending anonymous location " +
                "data to Google, even when no apps are running.";
    }
    
    class DialogDemoItem : StatelessWidget {
    public DialogDemoItem(Key key = null, IconData icon = null, Color color = null, string text = null, VoidCallback onPressed = null) : base(key: key)
    {
        this.icon = icon;
        this.color = color;
        this.text = text;
        this.onPressed = onPressed;
    }

    public readonly IconData icon;
    public readonly Color color;
    public readonly string text;
    public readonly VoidCallback onPressed;

    public override Widget build(BuildContext context) {
        return new SimpleDialogOption(
            onPressed: onPressed,
            child: new Row(
                       mainAxisAlignment: MainAxisAlignment.start,
                       crossAxisAlignment: CrossAxisAlignment.center,
                       children: new List<Widget>{
            new Icon(icon, size: 36.0f, color: color),
            new Padding(
                padding: EdgeInsets.only(left: 16.0f),
            child: new Text(text)
            )
            }
            )
            );
    }
    }
    
    class DialogDemo : StatefulWidget {
    public static readonly string routeName = "/material/dialog";

    public override State createState() => new DialogDemoState();
    }
    
    class DialogDemoState : State<DialogDemo> {
  readonly GlobalKey<ScaffoldState> _scaffoldKey = GlobalKey<ScaffoldState>.key();

  TimeOfDay _selectedTime;

  public override void initState() {
    base.initState();
    DateTime now = DateTime.Now;
    _selectedTime = new TimeOfDay(hour: now.Hour, minute: now.Minute);
  }

  void showDemoDialog<T>(BuildContext context = null, Widget child = null) {
    material_.showDialog<T>(
      context: context,
      builder: (BuildContext subContext) => child
    )
    .then((object value) => { // The value passed to Navigator.pop() or null.
      if (value != null) {
        _scaffoldKey.currentState.showSnackBar(new SnackBar(
          content: new Text($"You selected: {value}")
        ));
      }
    });
  }

  public override Widget build(BuildContext context) {
     ThemeData theme = Theme.of(context);
     TextStyle dialogTextStyle = theme.textTheme.subtitle1.copyWith(color: theme.textTheme.caption.color);

     var children = new List<Widget>
     {
       new RaisedButton(
            child: new Text("ALERT"),
            onPressed: () => {
              showDemoDialog<DialogDemoAction>(
                context: context,
                child: new AlertDialog(
                  content: new Text(
                    GalleryDialogDemoUtils._alertWithoutTitleText,
                    style: dialogTextStyle
                  ),
                  actions: new List<Widget>{
                    new FlatButton(
                      child: new Text("CANCEL"),
                      onPressed: () => { Navigator.pop(context, DialogDemoAction.cancel); }
                    ),
                    new FlatButton(
                      child: new Text("DISCARD"),
                      onPressed: () => { Navigator.pop(context, DialogDemoAction.discard); }
                    )
                  }
                )
              );
            }
          ),
          new RaisedButton(
            child: new Text("ALERT WITH TITLE"),
            onPressed: () => {
              showDemoDialog<DialogDemoAction>(
                context: context,
                child: new AlertDialog(
                  title: new Text("Use location service?"),
                  content: new Text(
                    GalleryDialogDemoUtils._alertWithTitleText,
                    style: dialogTextStyle
                  ),
                  actions: new List<Widget>{
                    new FlatButton(
                      child: new Text("DISAGREE"),
                      onPressed: () => { Navigator.pop(context, DialogDemoAction.disagree); }
                    ),
                new FlatButton(
                      child: new Text("AGREE"),
                      onPressed: () => { Navigator.pop(context, DialogDemoAction.agree); }
                    )
                  }
                )
              );
            }
          ),
          new RaisedButton(
            child: new Text("SIMPLE"),
            onPressed: () => {
              showDemoDialog<String>(
                context: context,
                child: new SimpleDialog(
                  title: new Text("Set backup account"),
                  children: new List<Widget>{
                    new DialogDemoItem(
                      icon: Icons.account_circle,
                      color: theme.primaryColor,
                      text: "username@gmail.com",
                      onPressed: () => { Navigator.pop(context, "username@gmail.com"); }
                    ),
                    new DialogDemoItem(
                      icon: Icons.account_circle,
                      color: theme.primaryColor,
                      text: "user02@gmail.com",
                      onPressed: () => { Navigator.pop(context, "user02@gmail.com"); }
                    ),
                    new DialogDemoItem(
                      icon: Icons.add_circle,
                      text: "add account",
                      color: theme.disabledColor
                    )
                  }
                )
              );
            }
          ),
          new RaisedButton(
            child: new Text("CONFIRMATION"),
            onPressed: () => {
              TimePickerUtils.showTimePicker(
                context: context,
                initialTime: _selectedTime
              )
              .then((object value) => {
                var time = (TimeOfDay) value;
                if (time != null && time != _selectedTime) {
                  _selectedTime = time;
                  _scaffoldKey.currentState.showSnackBar(new SnackBar(
                    content: new Text($"You selected: {time.format(context)}")
                  ));
                }
              });
            }
          ),
          new RaisedButton(
            child: new Text("FULLSCREEN"),
            onPressed: () => {
              /*Navigator.push(context, new MaterialPageRoute<DismissDialogAction>(
                builder: (BuildContext subContext) => new FullScreenDialogDemo(),
                fullscreenDialog: true
              ));*/
              
              D.assert(false, () => "TO DO >>>");
            }
          )
     };

    return new Scaffold(
      key: _scaffoldKey,
      appBar: new AppBar(
        title: new Text("Dialogs"),
        actions: new List<Widget>{new MaterialDemoDocumentationButton(DialogDemo.routeName)}
      ),
      body: new ListView(
        padding: EdgeInsets.symmetric(vertical: 24.0f, horizontal: 72.0f),
        children: children.Select<Widget, Widget>((Widget button) => {
          return new Container(
            padding: EdgeInsets.symmetric(vertical: 8.0f),
            child: button
          );
        })
        .ToList()
      )
    );
  }
}
}