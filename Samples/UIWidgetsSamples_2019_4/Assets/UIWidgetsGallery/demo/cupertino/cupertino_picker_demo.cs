using System;
using System.Collections.Generic;
using uiwidgets;
using Unity.UIWidgets.cupertino;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.rendering;
using Unity.UIWidgets.service;
using Unity.UIWidgets.ui;
using Unity.UIWidgets.widgets;
using UnityEngine;
using TextStyle = Unity.UIWidgets.painting.TextStyle;
using Brightness = Unity.UIWidgets.ui.Brightness;

namespace UIWidgetsGallery.gallery {
    class CupertinoPickerDemoUtils {
        public const float _kPickerSheetHeight = 216.0f;
        public const float _kPickerItemHeight = 32.0f;
        public static List<string> coolColorNames = new List<string>{
        "Sarcoline", "Coquelicot", "Smaragdine", "Mikado", "Glaucous", "Wenge",
        "Fulvous", "Xanadu", "Falu", "Eburnean", "Amaranth", "Australien",
        "Banan", "Falu", "Gingerline", "Incarnadine", "Labrador", "Nattier",
        "Pervenche", "Sinoper", "Verditer", "Watchet", "Zaffre",
        };
    }

    public class CupertinoPickerDemo : StatefulWidget {
      public const string routeName = "/cupertino/picker";

      public override State createState() {
        return new _CupertinoPickerDemoState();
      }
      
    }
    public class _CupertinoPickerDemoState : State<CupertinoPickerDemo> { 
      int _selectedColorIndex = 0;
      TimeSpan timer = new TimeSpan();
      DateTime date = DateTime.Now;
      DateTime time = DateTime.Now;
      DateTime dateTime = DateTime.Now;
      Widget _buildColorPicker(BuildContext context) {
        FixedExtentScrollController scrollController = new FixedExtentScrollController(initialItem: _selectedColorIndex);
        
        List<Widget> widgets = new List<Widget>();
        for (int i = 0; i < CupertinoPickerDemoUtils.coolColorNames.Count; i++)
        {
          widgets.Add(new Center(
            child: new Text(CupertinoPickerDemoUtils.coolColorNames[i])
          ));
        }

        return new GestureDetector(
          onTap: () => {
            CupertinoRouteUtils.showCupertinoModalPopup(
              context: context,
              semanticsDismissible: true,
              builder: (BuildContext context1) =>{
                return new _BottomPicker(
                  child: new CupertinoPicker(
                    scrollController: scrollController,
                    itemExtent: CupertinoPickerDemoUtils._kPickerItemHeight,
                    backgroundColor: CupertinoColors.systemBackground.resolveFrom(context1),
                    onSelectedItemChanged: (int index)=> {
                      setState(() => _selectedColorIndex = index);
                    },
                    children: widgets
                  )
                );
              }
            );
          },
          child: new _Menu(
            children: new List<Widget>{
              new Text("Favorite Color"),
              new Text(
                CupertinoPickerDemoUtils.coolColorNames[_selectedColorIndex],
                style: new TextStyle(color: CupertinoDynamicColor.resolve(CupertinoColors.inactiveGray, context))
              )
            }
          )
        );
      }
      Widget _buildCountdownTimerPicker(BuildContext context) {
        return new GestureDetector(
          onTap: () =>
          {
            CupertinoRouteUtils.showCupertinoModalPopup(
              context: context,
              semanticsDismissible: true,
              builder: (BuildContext context1) =>
              {
                return new _BottomPicker(
                  child: new CupertinoTimerPicker(
                    backgroundColor: CupertinoColors.systemBackground.resolveFrom(context1),
                    initialTimerDuration: timer,
                    onTimerDurationChanged: (TimeSpan newTimer) =>{
                      setState(() => timer = newTimer);
                    }
                  )
                );
              }
            );
          },
          child: new _Menu(
            children: new List<Widget>{
              new Text("Countdown Timer"),
              new Text(
                $"{timer.Hours}:" +
                $"{(timer.Minutes % 60).ToString("00")}:" +
                $"{(timer.Seconds % 60).ToString("00")}",
                style: new TextStyle(color: CupertinoDynamicColor.resolve(CupertinoColors.inactiveGray, context))
              ),
            }
          )
        );
      }

      Widget _buildDatePicker(BuildContext context)
      {
        return new GestureDetector(
          onTap: () =>
          {
            CupertinoRouteUtils.showCupertinoModalPopup(
              context: context,
              semanticsDismissible: true,
              builder: (BuildContext context1) =>
              {
                return new _BottomPicker(
                  child:  new  CupertinoDatePicker(
                    backgroundColor: CupertinoColors.systemBackground.resolveFrom(context1),
                    mode: CupertinoDatePickerMode.date,
                    initialDateTime: date,
                    onDateTimeChanged: (DateTime newDateTime) =>{
                      setState(() => date = newDateTime);
                    }
                  )
                );
              }
            );
          },
          child: new _Menu(
            children: new List<Widget>{
              new Text("Date"),
              new Text(
                date.ToString("MMMM dd, yyyy"),
                style: new TextStyle(color: CupertinoDynamicColor.resolve(CupertinoColors.inactiveGray, context))
              ),
            }
          )
        );
      }

      Widget _buildTimePicker(BuildContext context) {
        return new GestureDetector(
          onTap: () =>{
            CupertinoRouteUtils.showCupertinoModalPopup(
              context: context,
              semanticsDismissible: true,
              builder: (BuildContext context1) =>{
                return new _BottomPicker(
                  child: new CupertinoDatePicker(
                    backgroundColor: CupertinoColors.systemBackground.resolveFrom(context1),
                    mode: CupertinoDatePickerMode.time,
                    initialDateTime: time,
                    onDateTimeChanged: (newDateTime) => {
                      setState(() => time = newDateTime);
                    }
                  )
                );
              }
            );
          },
          child:new _Menu(
            children: new List<Widget>{
              new Text("Time"),
              new Text(
                time.ToString("h:mm tt"),
                style: new TextStyle(color: CupertinoDynamicColor.resolve(CupertinoColors.inactiveGray, context))
              ),
            }
          )
        );
      }

      Widget _buildDateAndTimePicker(BuildContext context) {
        return new GestureDetector(
          onTap: ()=> {
            CupertinoRouteUtils.showCupertinoModalPopup(
              context: context,
              semanticsDismissible: true,
              builder: (BuildContext context1) =>{
                return new _BottomPicker(
                  child: new CupertinoDatePicker(
                    backgroundColor: CupertinoColors.systemBackground.resolveFrom(context1),
                    mode: CupertinoDatePickerMode.dateAndTime,
                    initialDateTime: dateTime,
                    onDateTimeChanged: (newDateTime) => {
                      setState(() => dateTime = newDateTime);
                    }
                  )
                );
              }
            );
          },
          child: new _Menu(
            children:new List<Widget>{
              new Text("Date and Time"),
              new Text(
                dateTime.ToString("MMMM dd, yyyy") + " " + dateTime.ToString("HH:mm tt"),
                style: new TextStyle(color: CupertinoDynamicColor.resolve(CupertinoColors.inactiveGray, context))
              ),
            }
          )
        );
      }

      public override Widget build(BuildContext context) {
        return new CupertinoPageScaffold(
          navigationBar: new CupertinoNavigationBar(
            middle: new  Text("Picker"),
            // We're specifying a back label here because the previous page is a
            // Material page. CupertinoPageRoutes could auto-populate these back
            // labels.
            previousPageTitle: "Cupertino"
            //trailing: CupertinoDemoDocumentationButton(CupertinoPickerDemo.routeName)
          ),
          child: new DefaultTextStyle(
            style: CupertinoTheme.of(context).textTheme.textStyle,
            child: new ListView(
              children: new List<Widget>{
                new Padding(padding: EdgeInsets.only(top: 32.0f)),
                _buildColorPicker(context),
                _buildCountdownTimerPicker(context),
                _buildDatePicker(context),
                _buildTimePicker(context),
                _buildDateAndTimePicker(context),
              }
            )
          )
        );
      }
    }
  
public class _BottomPicker : StatelessWidget {
  public _BottomPicker(
    Key key = null,
    Widget child = null
  )  : base(key: key){
    D.assert(child != null);
    this.child = child;
  }
  public readonly Widget child;
  public override Widget build(BuildContext context)
  {
    return new Container(
      height: CupertinoPickerDemoUtils._kPickerSheetHeight,
      padding: EdgeInsets.only(top: 6.0f),
      color: CupertinoColors.label.resolveFrom(context).darkColor,
      child: new DefaultTextStyle(
        style: new TextStyle(
          color: CupertinoColors.label.resolveFrom(context),
          fontSize: 22.0f
        ),
        child: new GestureDetector(
          onTap: () =>{ },
          child: new SafeArea(
              top: false,
              child: child
            )
        )
      )
    );
  }

}

public class _Menu : StatelessWidget {
  public  _Menu(
    Key key = null,
    List<Widget> children = null
  )  :base(key: key) {
    D.assert(children != null);
    this.children = children;
  }

  public readonly List<Widget> children;

  public override Widget build(BuildContext context) {
    return new Container(
      decoration: new BoxDecoration(
        border: new Border(
          top: new BorderSide(color: CupertinoColors.inactiveGray, width: 0),
          bottom: new BorderSide(color: CupertinoColors.inactiveGray, width: 0)
        )
      ),
      height: 44,
      child: new Padding(
        padding: EdgeInsets.symmetric(horizontal: 16),
        child: new Row(
          mainAxisAlignment: MainAxisAlignment.spaceBetween,
          children: children
        )
      )
    );
  }
}
}
