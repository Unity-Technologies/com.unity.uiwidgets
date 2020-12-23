using System;
using System.Collections.Generic;
using Unity.UIWidgets.cupertino;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.rendering;
using Unity.UIWidgets.service;
using Unity.UIWidgets.ui;
using Unity.UIWidgets.widgets;
using TextStyle = Unity.UIWidgets.painting.TextStyle;
using Brightness = Unity.UIWidgets.ui.Brightness;

namespace UIWidgetsGallery.gallery {
    class CupertinoPickerDemoUtils {
        public const float _kPickerSheetHeight = 216.0f;
        public const float _kPickerItemHeight = 32.0f;
    }

    class CupertinoPickerDemo : StatefulWidget {
        public const string routeName = "/cupertino/picker";

        public override State createState() {
            return new _CupertinoPickerDemoState();
        }
    }

    class _CupertinoPickerDemoState : State<CupertinoPickerDemo> {
        int _selectedColorIndex = 0;
        TimeSpan timer = new TimeSpan();

        // Value that is shown in the date picker in date mode.
        DateTime date = DateTime.Now;

        // Value that is shown in the date picker in time mode.
        DateTime time = DateTime.Now;

        // Value that is shown in the date picker in dateAndTime mode.
        DateTime dateTime = DateTime.Now;

        Widget _buildMenu(List<Widget> children) {
            return new Container(
                decoration: new BoxDecoration(
                    color: CupertinoTheme.of(context).scaffoldBackgroundColor,
                    border: new Border(
                        top: new BorderSide(color: new Color(0xFFBCBBC1), width: 0.0f),
                        bottom: new BorderSide(color: new Color(0xFFBCBBC1), width: 0.0f)
                    )
                ),
                height: 44.0f,
                child: new Padding(
                    padding: EdgeInsets.symmetric(horizontal: 16.0f),
                    child: new SafeArea(
                        top: false,
                        bottom: false,
                        child: new Row(
                            mainAxisAlignment: MainAxisAlignment.spaceBetween,
                            children: children
                        )
                    )
                )
            );
        }

        Widget _buildBottomPicker(Widget picker) {
            return new Container(
                height: CupertinoPickerDemoUtils._kPickerSheetHeight,
                padding: EdgeInsets.only(top: 6.0f),
                color: CupertinoColors.white,
                child: new DefaultTextStyle(
                    style: new TextStyle(
                        color: CupertinoColors.black,
                        fontSize: 22.0f
                    ),
                    child: new GestureDetector(
                        // Blocks taps from propagating to the modal sheet and popping.
                        onTap: () => { },
                        child: new SafeArea(
                            top: false,
                            child: picker
                        )
                    )
                )
            );
        }

        Widget _buildColorPicker(BuildContext context) {
            FixedExtentScrollController scrollController =
                new FixedExtentScrollController(initialItem: _selectedColorIndex);

            List<Widget> generateList() {
                var list = new List<Widget>();
                foreach (var item in CupertinoNavigationDemoUtils.coolColorNames) {
                    list.Add(new Center(child:
                        new Text(item)
                    ));
                }

                return list;
            }


            return new GestureDetector(
                onTap: () => {
                    CupertinoRouteUtils.showCupertinoModalPopup(
                        context: context,
                        builder: (BuildContext _context) => {
                            return _buildBottomPicker(
                                new CupertinoPicker(
                                    scrollController: scrollController,
                                    itemExtent: CupertinoPickerDemoUtils._kPickerItemHeight,
                                    backgroundColor: CupertinoColors.white,
                                    onSelectedItemChanged: (int index) => {
                                        setState(() => _selectedColorIndex = index);
                                    },
                                    children: generateList()
                                )
                            );
                        }
                    );
                },
                child: _buildMenu(new List<Widget> {
                        new Text("Favorite Color"),
                        new Text(
                            CupertinoNavigationDemoUtils.coolColorNames[_selectedColorIndex],
                            style: new TextStyle(
                                color: CupertinoColors.inactiveGray
                            )
                        )
                    }
                )
            );
        }

        Widget _buildCountdownTimerPicker(BuildContext context) {
            return new GestureDetector(
                onTap: () => {
                    CupertinoRouteUtils.showCupertinoModalPopup(
                        context: context,
                        builder: (BuildContext _context) => {
                            return _buildBottomPicker(
                                new CupertinoTimerPicker(
                                    initialTimerDuration: timer,
                                    onTimerDurationChanged: (TimeSpan newTimer) => {
                                        setState(() => timer = newTimer);
                                    }
                                )
                            );
                        }
                    );
                },
                child: _buildMenu(new List<Widget> {
                        new Text("Countdown Timer"),
                        new Text(
                            $"{timer.Hours}:" +
                            $"{(timer.Minutes % 60).ToString("00")}:" +
                            $"{(timer.Seconds % 60).ToString("00")}",
                            style: new TextStyle(color: CupertinoColors.inactiveGray)
                        )
                    }
                )
            );
        }

        Widget _buildDatePicker(BuildContext context) {
            return new GestureDetector(
                onTap: () => {
                    CupertinoRouteUtils.showCupertinoModalPopup(
                        context: context,
                        builder: (BuildContext _context) => {
                            return _buildBottomPicker(
                                new CupertinoDatePicker(
                                    mode: CupertinoDatePickerMode.date,
                                    initialDateTime: date,
                                    onDateTimeChanged: (DateTime newDateTime) => {
                                        setState(() => date = newDateTime);
                                    }
                                )
                            );
                        }
                    );
                },
                child: _buildMenu(new List<Widget> {
                        new Text("Date"),
                        new Text(
                            date.ToString("MMMM dd, yyyy"),
                            style: new TextStyle(color: CupertinoColors.inactiveGray)
                        )
                    }
                )
            );
        }

        Widget _buildTimePicker(BuildContext context) {
            return new GestureDetector(
                onTap: () => {
                    CupertinoRouteUtils.showCupertinoModalPopup(
                        context: context,
                        builder: (BuildContext _context) => {
                            return _buildBottomPicker(
                                new CupertinoDatePicker(
                                    mode: CupertinoDatePickerMode.time,
                                    initialDateTime: time,
                                    onDateTimeChanged: (DateTime newDateTime) => {
                                        setState(() => time = newDateTime);
                                    }
                                )
                            );
                        }
                    );
                },
                child: _buildMenu(new List<Widget> {
                        new Text("Time"),
                        new Text(
                            time.ToString("h:mm tt"),
                            style: new TextStyle(color: CupertinoColors.inactiveGray)
                        )
                    }
                )
            );
        }

        Widget _buildDateAndTimePicker(BuildContext context) {
            return new GestureDetector(
                onTap: () => {
                    CupertinoRouteUtils.showCupertinoModalPopup(
                        context: context,
                        builder: (BuildContext _context) => {
                            return _buildBottomPicker(
                                new CupertinoDatePicker(
                                    mode: CupertinoDatePickerMode.dateAndTime,
                                    initialDateTime: dateTime,
                                    onDateTimeChanged: (DateTime newDateTime) => {
                                        setState(() => dateTime = newDateTime);
                                    }
                                )
                            );
                        }
                    );
                },
                child: _buildMenu(new List<Widget> {
                        new Text("Date and Time"),
                        new Text(
                            dateTime.ToString("MMMM dd, yyyy h:mm tt"),
                            style: new TextStyle(color: CupertinoColors.inactiveGray)
                        )
                    }
                )
            );
        }

        public override Widget build(BuildContext context) {
            return new CupertinoPageScaffold(
                navigationBar: new CupertinoNavigationBar(
                    middle: new Text("Picker"),
                    // We"re specifying a back label here because the previous page is a
                    // Material page. CupertinoPageRoutes could auto-populate these back
                    // labels.
                    previousPageTitle: "Cupertino"
                    //, trailing: new CupertinoDemoDocumentationButton(CupertinoPickerDemo.routeName)
                ),
                child: new DefaultTextStyle(
                    style: CupertinoTheme.of(context).textTheme.textStyle,
                    child: new DecoratedBox(
                        decoration: new BoxDecoration(
                            color: CupertinoTheme.of(context).brightness == Brightness.light
                                ? CupertinoColors.extraLightBackgroundGray
                                : CupertinoColors.darkBackgroundGray
                        ),
                        child: new SafeArea(
                            child: new ListView(
                                children: new List<Widget> {
                                    new Padding(padding: EdgeInsets.only(top: 32.0f)),
                                    _buildColorPicker(context),
                                    _buildCountdownTimerPicker(context),
                                    _buildDatePicker(context),
                                    _buildTimePicker(context),
                                    _buildDateAndTimePicker(context)
                                }
                            )
                        )
                    )
                )
            );
        }
    }
}