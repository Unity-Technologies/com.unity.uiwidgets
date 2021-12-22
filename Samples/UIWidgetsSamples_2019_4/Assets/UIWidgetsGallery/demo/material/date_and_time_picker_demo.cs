using System;
using System.Collections.Generic;
using System.Linq;
using uiwidgets;
using UIWidgetsGallery.gallery;
using Unity.UIWidgets.async;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.material;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.rendering;
using Unity.UIWidgets.ui;
using Unity.UIWidgets.widgets;
using TextStyle = Unity.UIWidgets.painting.TextStyle;

namespace UIWidgetsGallery.demo.material
{
    internal class _InputDropdown : StatelessWidget
    {
        public _InputDropdown(
            Key key = null,
            Widget child = null,
            string labelText = null,
            string valueText = null,
            TextStyle valueStyle = null,
            VoidCallback onPressed = null
        ) : base(key: key)
        {
            this.labelText = labelText;
            this.valueStyle = valueStyle;
            this.valueText = valueText;
            this.onPressed = onPressed;
            this.child = child;
        }

        public readonly string labelText;
        public readonly string valueText;
        public readonly TextStyle valueStyle;
        public readonly VoidCallback onPressed;
        public readonly Widget child;


        public override Widget build(BuildContext context)
        {
            return new InkWell(
                onTap: () => this.onPressed?.Invoke(),
                child: new InputDecorator(
                    decoration: new InputDecoration(
                        labelText: this.labelText
                    ),
                    baseStyle: this.valueStyle,
                    child: new Row(
                        mainAxisAlignment: MainAxisAlignment.spaceBetween,
                        mainAxisSize: MainAxisSize.min,
                        children: new List<Widget>
                        {
                            new Text(this.valueText, style: this.valueStyle),
                            new Icon(Icons.arrow_drop_down,
                                color: Theme.of(context).brightness == Brightness.light
                                    ? Colors.grey.shade700
                                    : Colors.white70
                            )
                        }
                    )
                )
            );
        }
    }

    internal class _DateTimePicker : StatelessWidget
    {
        public _DateTimePicker(
            Key key = null,
            string labelText = null,
            DateTime? selectedDate = null,
            TimeOfDay selectedTime = null,
            ValueChanged<DateTime?> selectDate = null,
            ValueChanged<TimeOfDay> selectTime = null
        ) : base(key: key)
        {
            this.labelText = labelText;
            this.selectDate = selectDate;
            this.selectedTime = selectedTime;
            this.selectedDate = selectedDate;
            this.selectTime = selectTime;
        }

        public readonly string labelText;
        public readonly DateTime? selectedDate;
        public readonly TimeOfDay selectedTime;
        public readonly ValueChanged<DateTime?> selectDate;
        public readonly ValueChanged<TimeOfDay> selectTime;

        private Future _selectDate(BuildContext context)
        {
            material_.showDatePicker(
                context: context,
                initialDate: this.selectedDate.Value,
                firstDate: new DateTime(2015, 8, 1),
                lastDate: new DateTime(2030, 1, 1)
            ).then((object value) =>
            {
                var picked = (DateTime) value;
                if (picked != null && picked != this.selectedDate) this.selectDate(picked);
            });

            return null;
        }

        private Future _selectTime(BuildContext context)
        {
            TimePickerUtils.showTimePicker(
                context: context,
                initialTime: this.selectedTime
            ).then((object value) =>
            {
                var picked = (TimeOfDay) value;
                if (picked != null && picked != this.selectedTime) this.selectTime(picked);
            });

            return null;
        }

        public override Widget build(BuildContext context)
        {
            TextStyle valueStyle = Theme.of(context).textTheme.headline6;
            return new Row(
                crossAxisAlignment: CrossAxisAlignment.end,
                children: new List<Widget>
                {
                    new Expanded(
                        flex: 4,
                        child: new _InputDropdown(
                            labelText: this.labelText,
                            valueText: this.selectedDate.Value.ToString("yyyyMMdd"),
                            valueStyle: valueStyle,
                            onPressed: () => { this._selectDate(context); }
                        )
                    ),
                    new SizedBox(width: 12.0f),
                    new Expanded(
                        flex: 3,
                        child: new _InputDropdown(
                            valueText: this.selectedTime.format(context),
                            valueStyle: valueStyle,
                            onPressed: () => { this._selectTime(context); }
                        )
                    )
                }
            );
        }
    }

    internal class DateAndTimePickerDemo : StatefulWidget
    {
        public static readonly string routeName = "/material/date-and-time-pickers";

        public override State createState()
        {
            return new _DateAndTimePickerDemoState();
        }
    }

    internal class _DateAndTimePickerDemoState : State<DateAndTimePickerDemo>
    {
        private DateTime _fromDate = DateTime.Now;
        private TimeOfDay _fromTime = new TimeOfDay(hour: 7, minute: 28);
        private DateTime _toDate = DateTime.Now;
        private TimeOfDay _toTime = new TimeOfDay(hour: 7, minute: 28);
        public readonly List<string> _allActivities = new List<string> {"hiking", "swimming", "boating", "fishing"};
        private string _activity = "fishing";

        public override Widget build(BuildContext context)
        {
            return new Scaffold(
                appBar: new AppBar(
                    title: new Text("Date and time pickers"),
                    actions: new List<Widget> {new MaterialDemoDocumentationButton(DateAndTimePickerDemo.routeName)}
                ),
                body: new DropdownButtonHideUnderline(
                    child: new SafeArea(
                        top: false,
                        bottom: false,
                        child: new ListView(
                            padding: EdgeInsets.all(16.0f),
                            children: new List<Widget>
                            {
                                new TextField(
                                    enabled: true,
                                    decoration: new InputDecoration(
                                        labelText: "Event name",
                                        border: new OutlineInputBorder()
                                    ),
                                    style: Theme.of(context).textTheme.headline4
                                ),
                                new TextField(
                                    decoration: new InputDecoration(
                                        labelText: "Location"
                                    ),
                                    style: Theme.of(context).textTheme.headline4.copyWith(fontSize: 20.0f)
                                ),
                                new _DateTimePicker(
                                    labelText: "From",
                                    selectedDate: this._fromDate,
                                    selectedTime: this._fromTime,
                                    selectDate: (DateTime? date) =>
                                    {
                                        this.setState(() => { this._fromDate = date.Value; });
                                    },
                                    selectTime: (TimeOfDay time) => { this.setState(() => { this._fromTime = time; }); }
                                ),
                                new _DateTimePicker(
                                    labelText: "To",
                                    selectedDate: this._toDate,
                                    selectedTime: this._toTime,
                                    selectDate: (DateTime? date) =>
                                    {
                                        this.setState(() => { this._toDate = date.Value; });
                                    },
                                    selectTime: (TimeOfDay time) => { this.setState(() => { this._toTime = time; }); }
                                ),
                                new SizedBox(height: 8.0f),
                                new InputDecorator(
                                    decoration: new InputDecoration(
                                        labelText: "Activity",
                                        hintText: "Choose an activity",
                                        contentPadding: EdgeInsets.zero
                                    ),
                                    isEmpty: this._activity == null,
                                    child: new DropdownButton<string>(
                                        value: this._activity,
                                        onChanged: (string newValue) =>
                                        {
                                            this.setState(() => { this._activity = newValue; });
                                        },
                                        items: this._allActivities.Select<string, DropdownMenuItem<string>>(
                                            (string value) =>
                                            {
                                                return new DropdownMenuItem<string>(
                                                    value: value,
                                                    child: new Text(value)
                                                );
                                            }).ToList()
                                    )
                                )
                            }
                        )
                    )
                )
            );
        }
    }
}