using System;
using System.Collections.Generic;
using Unity.UIWidgets.animation;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.rendering;
using Unity.UIWidgets.scheduler2;
using Unity.UIWidgets.ui;
using Unity.UIWidgets.widgets;
using TextStyle = Unity.UIWidgets.painting.TextStyle;

namespace Unity.UIWidgets.cupertino {
    static class CupertinoDatePickerUtils {
        public const float _kItemExtent = 32.0f;
        public const float _kPickerWidth = 330.0f;
        public const bool _kUseMagnifier = true;
        public const float _kMagnification = 1.05f;
        public const float _kDatePickerPadSize = 12.0f;

        public static Color _kBackgroundColor = CupertinoColors.white;

        public static TextStyle _kDefaultPickerTextStyle = new TextStyle(
            letterSpacing: -0.83f
        );


        public delegate Widget listGenerateDelegate(int index);

        public static List<Widget> listGenerate(int count, listGenerateDelegate func) {
            var list = new List<Widget>();
            for (int i = 0; i < count; i++) {
                list.Add(func(i));
            }

            return list;
        }
    }

    public class _DatePickerLayoutDelegate : MultiChildLayoutDelegate {
        public _DatePickerLayoutDelegate(
            List<float> columnWidths
        ) {
            D.assert(columnWidths != null);
            this.columnWidths = columnWidths;
        }

        public readonly List<float> columnWidths;

        public override void performLayout(Size size) {
            float remainingWidth = size.width;
            for (int i = 0; i < columnWidths.Count; i++) {
                remainingWidth -= columnWidths[i] + CupertinoDatePickerUtils._kDatePickerPadSize * 2;
            }

            float currentHorizontalOffset = 0.0f;
            for (int i = 0; i < columnWidths.Count; i++) {
                float childWidth = columnWidths[i] + CupertinoDatePickerUtils._kDatePickerPadSize * 2;
                if (i == 0 || i == columnWidths.Count - 1) {
                    childWidth += remainingWidth / 2;
                }

                layoutChild(i, BoxConstraints.tight(new Size(childWidth, size.height)));
                positionChild(i, new Offset(currentHorizontalOffset, 0.0f));
                currentHorizontalOffset += childWidth;
            }
        }

        public override bool shouldRelayout(MultiChildLayoutDelegate oldDelegate) {
            return columnWidths != ((_DatePickerLayoutDelegate) oldDelegate).columnWidths;
        }
    }

    public enum CupertinoDatePickerMode {
        time,
        date,
        dateAndTime,
    }

    enum _PickerColumnType {
        dayOfMonth,
        month,
        year,
        date,
        hour,
        minute,
        dayPeriod,
    }

    public class CupertinoDatePicker : StatefulWidget {
        public CupertinoDatePicker(
            ValueChanged<DateTime> onDateTimeChanged,
            CupertinoDatePickerMode mode = CupertinoDatePickerMode.dateAndTime,
            DateTime? initialDateTime = null,
            DateTime? minimumDate = null,
            DateTime? maximumDate = null,
            int minimumYear = 1,
            int? maximumYear = null,
            int minuteInterval = 1,
            bool use24hFormat = false
        ) {
            this.initialDateTime = initialDateTime ?? DateTime.Now;
            D.assert(onDateTimeChanged != null);
            D.assert(
                minuteInterval > 0 && 60 % minuteInterval == 0,
                () => "minute interval is not a positive integer factor of 60"
            );
            D.assert(this.initialDateTime != null);
            D.assert(
                mode != CupertinoDatePickerMode.dateAndTime || minimumDate == null ||
                !(this.initialDateTime < minimumDate),
                () => "initial date is before minimum date"
            );
            D.assert(
                mode != CupertinoDatePickerMode.dateAndTime || maximumDate == null ||
                !(this.initialDateTime > maximumDate),
                () => "initial date is after maximum date"
            );
            D.assert(
                mode != CupertinoDatePickerMode.date || (minimumYear >= 1 && this.initialDateTime.Year >= minimumYear),
                () => "initial year is not greater than minimum year, or mininum year is not positive"
            );
            D.assert(
                mode != CupertinoDatePickerMode.date || maximumYear == null || this.initialDateTime.Year <= maximumYear,
                () => "initial year is not smaller than maximum year"
            );
            D.assert(
                this.initialDateTime.Minute % minuteInterval == 0,
                () => "initial minute is not divisible by minute interval"
            );
            this.onDateTimeChanged = onDateTimeChanged;
            this.mode = mode;
            this.minimumDate = minimumDate;
            this.maximumDate = maximumDate;
            this.minimumYear = minimumYear;
            this.maximumYear = maximumYear;
            this.minuteInterval = minuteInterval;
            this.use24hFormat = use24hFormat;
        }

        public readonly CupertinoDatePickerMode mode;
        public readonly DateTime initialDateTime;
        public readonly DateTime? minimumDate;
        public readonly DateTime? maximumDate;
        public readonly int minimumYear;
        public readonly int? maximumYear;
        public readonly int minuteInterval;
        public readonly bool use24hFormat;
        public readonly ValueChanged<DateTime> onDateTimeChanged;

        public override State createState() {
            if (mode == CupertinoDatePickerMode.time || mode == CupertinoDatePickerMode.dateAndTime) {
                return new _CupertinoDatePickerDateTimeState();
            }
            else {
                return new _CupertinoDatePickerDateState();
            }
        }

        internal static float _getColumnWidth(
            _PickerColumnType columnType,
            CupertinoLocalizations localizations,
            BuildContext context
        ) {
            string longestText = "";
            switch (columnType) {
                case _PickerColumnType.date:

                    for (int i = 1; i <= 12; i++) {
                        string date =
                            localizations.datePickerMediumDate(new DateTime(2018, i, 25));
                        if (longestText.Length < date.Length) {
                            longestText = date;
                        }
                    }

                    break;
                case _PickerColumnType.hour:
                    for (int i = 0; i < 24; i++) {
                        string hour = localizations.datePickerHour(i);
                        if (longestText.Length < hour.Length) {
                            longestText = hour;
                        }
                    }

                    break;
                case _PickerColumnType.minute:
                    for (int i = 0; i < 60; i++) {
                        string minute = localizations.datePickerMinute(i);
                        if (longestText.Length < minute.Length) {
                            longestText = minute;
                        }
                    }

                    break;
                case _PickerColumnType.dayPeriod:
                    longestText =
                        localizations.anteMeridiemAbbreviation.Length > localizations.postMeridiemAbbreviation.Length
                            ? localizations.anteMeridiemAbbreviation
                            : localizations.postMeridiemAbbreviation;
                    break;
                case _PickerColumnType.dayOfMonth:
                    for (int i = 1; i <= 31; i++) {
                        string dayOfMonth = localizations.datePickerDayOfMonth(i);
                        if (longestText.Length < dayOfMonth.Length) {
                            longestText = dayOfMonth;
                        }
                    }

                    break;
                case _PickerColumnType.month:
                    for (int i = 1; i <= 12; i++) {
                        string month = localizations.datePickerMonth(i);
                        if (longestText.Length < month.Length) {
                            longestText = month;
                        }
                    }

                    break;
                case _PickerColumnType.year:
                    longestText = localizations.datePickerYear(2018);
                    break;
            }

            D.assert(longestText != "", () => "column type is not appropriate");
            TextPainter painter = new TextPainter(
                text: new TextSpan(
                    style: DefaultTextStyle.of(context).style,
                    text: longestText
                )
            );


            painter.layout();
            return painter.maxIntrinsicWidth;
        }
    }

    delegate Widget _ColumnBuilder(float offAxisFraction, TransitionBuilder itemPositioningBuilder);

    class _CupertinoDatePickerDateTimeState : State<CupertinoDatePicker> {
        public CupertinoLocalizations localizations;
        public Alignment alignCenterLeft;
        public Alignment alignCenterRight;
        public DateTime initialDateTime;
        public int selectedDayFromInitial;
        public int selectedHour;
        public int previousHourIndex;
        public int selectedMinute;
        public int selectedAmPm;
        public FixedExtentScrollController amPmController;

        public static Dictionary<int, float> estimatedColumnWidths = new Dictionary<int, float>();

        public override void initState() {
            base.initState();
            initialDateTime = widget.initialDateTime;
            selectedDayFromInitial = 0;
            selectedHour = widget.initialDateTime.Hour;
            selectedMinute = widget.initialDateTime.Minute;
            selectedAmPm = 0;
            if (!widget.use24hFormat) {
                selectedAmPm = selectedHour / 12;
                selectedHour = selectedHour % 12;
                if (selectedHour == 0) {
                    selectedHour = 12;
                }

                amPmController = new FixedExtentScrollController(initialItem: selectedAmPm);
            }

            previousHourIndex = selectedHour;
        }

        public override void didUpdateWidget(StatefulWidget oldWidget) {
            base.didUpdateWidget(oldWidget);
            D.assert(
                ((CupertinoDatePicker) oldWidget).mode == widget.mode,
                () => "The CupertinoDatePicker's mode cannot change once it's built"
            );
        }

        public override void didChangeDependencies() {
            base.didChangeDependencies();

            localizations = CupertinoLocalizations.of(context);
            alignCenterLeft = Alignment.centerLeft;
            alignCenterRight = Alignment.centerRight;
            estimatedColumnWidths.Clear();
        }

        float _getEstimatedColumnWidth(_PickerColumnType columnType) {
            if (!estimatedColumnWidths.TryGetValue((int) columnType, out float _)) {
                estimatedColumnWidths[(int) columnType] =
                    CupertinoDatePicker._getColumnWidth(columnType, localizations, context);
            }

            return estimatedColumnWidths[(int) columnType];
        }

        DateTime _getDateTime() {
            DateTime date = new DateTime(initialDateTime.Year, initialDateTime.Month, initialDateTime.Day
            ).Add(new TimeSpan(selectedDayFromInitial, 0, 0, 0));
            return new DateTime(
                date.Year,
                date.Month,
                date.Day,
                widget.use24hFormat ? selectedHour : selectedHour % 12 + selectedAmPm * 12,
                selectedMinute,
                0
            );
        }

        Widget _buildMediumDatePicker(float offAxisFraction, TransitionBuilder itemPositioningBuilder) {
            return CupertinoPicker.builder(
                scrollController: new FixedExtentScrollController(initialItem: selectedDayFromInitial),
                offAxisFraction: offAxisFraction,
                itemExtent: CupertinoDatePickerUtils._kItemExtent,
                useMagnifier: CupertinoDatePickerUtils._kUseMagnifier,
                magnification: CupertinoDatePickerUtils._kMagnification,
                backgroundColor: CupertinoDatePickerUtils._kBackgroundColor,
                onSelectedItemChanged: (int index) => {
                    selectedDayFromInitial = index;
                    widget.onDateTimeChanged(_getDateTime());
                },
                itemBuilder: (BuildContext context, int index) => {
                    DateTime dateTime = new DateTime(initialDateTime.Year, initialDateTime.Month,
                        initialDateTime.Day
                    ).Add(new TimeSpan(index, 0, 0, 0));
                    if (widget.minimumDate != null && dateTime < widget.minimumDate) {
                        return null;
                    }

                    if (widget.maximumDate != null && dateTime > widget.maximumDate) {
                        return null;
                    }

                    return itemPositioningBuilder(
                        context,
                        new Text(localizations.datePickerMediumDate(dateTime))
                    );
                }
            );
        }

        Widget _buildHourPicker(float offAxisFraction, TransitionBuilder itemPositioningBuilder) {
            return new CupertinoPicker(
                scrollController: new FixedExtentScrollController(initialItem: selectedHour),
                offAxisFraction: offAxisFraction,
                itemExtent: CupertinoDatePickerUtils._kItemExtent,
                useMagnifier: CupertinoDatePickerUtils._kUseMagnifier,
                magnification: CupertinoDatePickerUtils._kMagnification,
                backgroundColor: CupertinoDatePickerUtils._kBackgroundColor,
                onSelectedItemChanged: (int index) => {
                    if (widget.use24hFormat) {
                        selectedHour = index;
                        widget.onDateTimeChanged(_getDateTime());
                    }
                    else {
                        selectedHour = index % 12;


                        bool wasAm = previousHourIndex >= 0 && previousHourIndex <= 11;
                        bool isAm = index >= 0 && index <= 11;
                        if (wasAm != isAm) {
                            amPmController.animateToItem(
                                1 - amPmController.selectedItem,
                                duration: new TimeSpan(0, 0, 0, 0, 300),
                                curve:
                                Curves.easeOut
                            );
                        }
                        else {
                            widget.onDateTimeChanged(_getDateTime());
                        }
                    }

                    previousHourIndex = index;
                },
                children: CupertinoDatePickerUtils.listGenerate(24, (index) => {
                    int hour = index;
                    if (!widget.use24hFormat) {
                        hour = hour % 12 == 0 ? 12 : hour % 12;
                    }

                    return itemPositioningBuilder(context,
                        new Text(localizations.datePickerHour(hour)
                        )
                    );
                }),
                looping: true
            );
        }

        Widget _buildMinutePicker(float offAxisFraction, TransitionBuilder itemPositioningBuilder) {
            return new CupertinoPicker(
                scrollController: new FixedExtentScrollController(
                    initialItem: selectedMinute / widget.minuteInterval),
                offAxisFraction: offAxisFraction,
                itemExtent: CupertinoDatePickerUtils._kItemExtent,
                useMagnifier: CupertinoDatePickerUtils._kUseMagnifier,
                magnification: CupertinoDatePickerUtils._kMagnification,
                backgroundColor: CupertinoDatePickerUtils._kBackgroundColor,
                onSelectedItemChanged: (int index) => {
                    selectedMinute = index * widget.minuteInterval;
                    widget.onDateTimeChanged(_getDateTime());
                },
                children: CupertinoDatePickerUtils.listGenerate(60 / widget.minuteInterval, (int index) => {
                    int minute = index * widget.minuteInterval;
                    return itemPositioningBuilder(context,
                        new Text(localizations.datePickerMinute(minute)
                        )
                    );
                }),
                looping: true
            );
        }

        Widget _buildAmPmPicker(float offAxisFraction, TransitionBuilder itemPositioningBuilder) {
            return new CupertinoPicker(
                scrollController: amPmController,
                offAxisFraction: offAxisFraction,
                itemExtent: CupertinoDatePickerUtils._kItemExtent,
                useMagnifier: CupertinoDatePickerUtils._kUseMagnifier,
                magnification: CupertinoDatePickerUtils._kMagnification,
                backgroundColor: CupertinoDatePickerUtils._kBackgroundColor,
                onSelectedItemChanged: (int index) => {
                    selectedAmPm = index;
                    widget.onDateTimeChanged(_getDateTime());
                },
                children: CupertinoDatePickerUtils.listGenerate(2, (int index) => {
                    return itemPositioningBuilder(context,
                        new Text(
                            index == 0
                                ? localizations.anteMeridiemAbbreviation
                                : localizations.postMeridiemAbbreviation
                        )
                    );
                })
            );
        }

        public override Widget build(BuildContext context) {
            List<float> columnWidths = new List<float>() {
                _getEstimatedColumnWidth(_PickerColumnType.hour),
                _getEstimatedColumnWidth(_PickerColumnType.minute),
            };

            List<_ColumnBuilder> pickerBuilders = new List<_ColumnBuilder> {
                _buildHourPicker, _buildMinutePicker,
            };

            if (!widget.use24hFormat) {
                if (localizations.datePickerDateTimeOrder == DatePickerDateTimeOrder.date_time_dayPeriod
                    || localizations.datePickerDateTimeOrder == DatePickerDateTimeOrder.time_dayPeriod_date) {
                    pickerBuilders.Add(_buildAmPmPicker);
                    columnWidths.Add(_getEstimatedColumnWidth(_PickerColumnType.dayPeriod));
                }
                else {
                    pickerBuilders.Insert(0, _buildAmPmPicker);
                    columnWidths.Insert(0, _getEstimatedColumnWidth(_PickerColumnType.dayPeriod));
                }
            }

            if (widget.mode == CupertinoDatePickerMode.dateAndTime) {
                if (localizations.datePickerDateTimeOrder == DatePickerDateTimeOrder.time_dayPeriod_date
                    || localizations.datePickerDateTimeOrder == DatePickerDateTimeOrder.dayPeriod_time_date) {
                    pickerBuilders.Add(_buildMediumDatePicker);
                    columnWidths.Add(_getEstimatedColumnWidth(_PickerColumnType.date));
                }
                else {
                    pickerBuilders.Insert(0, _buildMediumDatePicker);
                    columnWidths.Insert(0, _getEstimatedColumnWidth(_PickerColumnType.date));
                }
            }

            List<Widget> pickers = new List<Widget>();
            for (int i = 0; i < columnWidths.Count; i++) {
                var _i = i;
                float offAxisFraction = 0.0f;
                if (_i == 0) {
                    offAxisFraction = -0.5f;
                }
                else if (_i >= 2 || columnWidths.Count == 2) {
                    offAxisFraction = 0.5f;
                }

                EdgeInsets padding = EdgeInsets.only(right: CupertinoDatePickerUtils._kDatePickerPadSize);
                if (_i == columnWidths.Count - 1) {
                    padding = padding.flipped;
                }

                pickers.Add(new LayoutId(
                    id: _i,
                    child: pickerBuilders[_i](
                        offAxisFraction,
                        (BuildContext _context, Widget child) => {
                            return new Container(
                                alignment: _i == columnWidths.Count - 1
                                    ? alignCenterLeft
                                    : alignCenterRight,
                                padding: padding,
                                child: new Container(
                                    alignment: _i == columnWidths.Count - 1
                                        ? alignCenterLeft
                                        : alignCenterRight,
                                    width: _i == 0 || _i == columnWidths.Count - 1
                                        ? (float?) null
                                        : columnWidths[_i] + CupertinoDatePickerUtils._kDatePickerPadSize,
                                    child: child
                                )
                            );
                        }
                    )
                ));
            }

            return new MediaQuery(
                data: new MediaQueryData(textScaleFactor: 1.0f),
                child: DefaultTextStyle.merge(
                    style: CupertinoDatePickerUtils._kDefaultPickerTextStyle,
                    child: new CustomMultiChildLayout(
                        layoutDelegate: new _DatePickerLayoutDelegate(
                            columnWidths: columnWidths
                        ),
                        children: pickers
                    )
                )
            );
        }
    }

    class _CupertinoDatePickerDateState : State<CupertinoDatePicker> {
        CupertinoLocalizations localizations;

        Alignment alignCenterLeft;
        Alignment alignCenterRight;

        int selectedDay;
        int selectedMonth;
        int selectedYear;
        FixedExtentScrollController dayController;

        Dictionary<int, float> estimatedColumnWidths = new Dictionary<int, float>();

        public override void initState() {
            base.initState();
            selectedDay = widget.initialDateTime.Day;
            selectedMonth = widget.initialDateTime.Month;
            selectedYear = widget.initialDateTime.Year;
            dayController = new FixedExtentScrollController(initialItem: selectedDay - 1);
        }

        public override void didChangeDependencies() {
            base.didChangeDependencies();
            localizations = CupertinoLocalizations.of(context);
            alignCenterLeft = Alignment.centerLeft;
            alignCenterRight = Alignment.centerRight;
            estimatedColumnWidths[(int) _PickerColumnType.dayOfMonth] = CupertinoDatePicker._getColumnWidth(
                _PickerColumnType.dayOfMonth, localizations, context);

            estimatedColumnWidths[(int) _PickerColumnType.month] = CupertinoDatePicker._getColumnWidth(
                _PickerColumnType.month, localizations, context);

            estimatedColumnWidths[(int) _PickerColumnType.year] = CupertinoDatePicker._getColumnWidth(
                _PickerColumnType.year, localizations, context);
        }

        Widget _buildDayPicker(float offAxisFraction, TransitionBuilder itemPositioningBuilder) {
            int daysInCurrentMonth = DateTime.DaysInMonth(selectedYear, selectedMonth);
            return new CupertinoPicker(
                scrollController: dayController,
                offAxisFraction: offAxisFraction,
                itemExtent: CupertinoDatePickerUtils._kItemExtent,
                useMagnifier: CupertinoDatePickerUtils._kUseMagnifier,
                magnification: CupertinoDatePickerUtils._kMagnification,
                backgroundColor: CupertinoDatePickerUtils._kBackgroundColor,
                onSelectedItemChanged: (int index) => {
                    selectedDay = index + 1;
                    var validDay = selectedDay;
                    if (DateTime.DaysInMonth(selectedYear, selectedMonth) < selectedDay) {
                        validDay -= DateTime.DaysInMonth(selectedYear, selectedMonth);
                    }

                    if (selectedDay == validDay) {
                        widget.onDateTimeChanged(new DateTime(selectedYear, selectedMonth,
                            selectedDay));
                    }
                },
                children: CupertinoDatePickerUtils.listGenerate(31, (int index) => {
                    TextStyle disableTextStyle = null;

                    if (index >= daysInCurrentMonth) {
                        disableTextStyle = new TextStyle(color: CupertinoColors.inactiveGray);
                    }

                    return itemPositioningBuilder(context,
                        new Text(localizations.datePickerDayOfMonth(index + 1),
                            style: disableTextStyle
                        )
                    );
                }),
                looping: true
            );
        }

        Widget _buildMonthPicker(float offAxisFraction, TransitionBuilder itemPositioningBuilder) {
            return new CupertinoPicker(
                scrollController: new FixedExtentScrollController(initialItem: selectedMonth - 1),
                offAxisFraction: offAxisFraction,
                itemExtent: CupertinoDatePickerUtils._kItemExtent,
                useMagnifier: CupertinoDatePickerUtils._kUseMagnifier,
                magnification: CupertinoDatePickerUtils._kMagnification,
                backgroundColor: CupertinoDatePickerUtils._kBackgroundColor,
                onSelectedItemChanged: (int index) => {
                    selectedMonth = index + 1;
                    var validDay = selectedDay;
                    if (DateTime.DaysInMonth(selectedYear, selectedMonth) < selectedDay) {
                        validDay -= DateTime.DaysInMonth(selectedYear, selectedMonth);
                    }

                    if (selectedDay == validDay) {
                        widget.onDateTimeChanged(new DateTime(selectedYear, selectedMonth,
                            selectedDay));
                    }
                },
                children: CupertinoDatePickerUtils.listGenerate(12, (int index) => {
                    return itemPositioningBuilder(context,
                        new Text(localizations.datePickerMonth(index + 1))
                    );
                }),
                looping: true
            );
        }

        Widget _buildYearPicker(float offAxisFraction, TransitionBuilder itemPositioningBuilder) {
            return CupertinoPicker.builder(
                scrollController: new FixedExtentScrollController(initialItem: selectedYear),
                itemExtent: CupertinoDatePickerUtils._kItemExtent,
                offAxisFraction: offAxisFraction,
                useMagnifier: CupertinoDatePickerUtils._kUseMagnifier,
                magnification: CupertinoDatePickerUtils._kMagnification,
                backgroundColor: CupertinoDatePickerUtils._kBackgroundColor,
                onSelectedItemChanged: (int index) => {
                    selectedYear = index;
                    if (new DateTime(selectedYear, selectedMonth, selectedDay).Day == selectedDay) {
                        widget.onDateTimeChanged(new DateTime(selectedYear, selectedMonth,
                            selectedDay));
                    }
                },
                itemBuilder: (BuildContext context, int index) => {
                    if (index < widget.minimumYear) {
                        return null;
                    }

                    if (widget.maximumYear != null && index > widget.maximumYear) {
                        return null;
                    }

                    return itemPositioningBuilder(
                        context,
                        new Text(localizations.datePickerYear(index))
                    );
                }
            );
        }

        bool _keepInValidRange(ScrollEndNotification notification) {
            var desiredDay = selectedDay;
            if (DateTime.DaysInMonth(selectedYear, selectedMonth) < selectedDay) {
                desiredDay -= DateTime.DaysInMonth(selectedYear, selectedMonth);
            }

            if (desiredDay != selectedDay) {
                SchedulerBinding.instance.addPostFrameCallback((TimeSpan timestamp) => {
                    dayController.animateToItem(dayController.selectedItem - desiredDay,
                        duration: new TimeSpan(0, 0, 0, 0, 200),
                        curve:
                        Curves.easeOut
                    );
                });
            }

            setState(() => { });
            return false;
        }

        public override Widget build(BuildContext context) {
            List<_ColumnBuilder> pickerBuilders = new List<_ColumnBuilder>();
            List<float> columnWidths = new List<float>();
            switch (localizations.datePickerDateOrder) {
                case DatePickerDateOrder.mdy:
                    pickerBuilders = new List<_ColumnBuilder>()
                        {_buildMonthPicker, _buildDayPicker, _buildYearPicker};
                    columnWidths = new List<float>() {
                        estimatedColumnWidths[(int) _PickerColumnType.month],
                        estimatedColumnWidths[(int) _PickerColumnType.dayOfMonth],
                        estimatedColumnWidths[(int) _PickerColumnType.year]
                    };
                    break;
                case DatePickerDateOrder.dmy:
                    pickerBuilders = new List<_ColumnBuilder>
                        {_buildDayPicker, _buildMonthPicker, _buildYearPicker};
                    columnWidths = new List<float> {
                        estimatedColumnWidths[(int) _PickerColumnType.dayOfMonth],
                        estimatedColumnWidths[(int) _PickerColumnType.month],
                        estimatedColumnWidths[(int) _PickerColumnType.year]
                    };
                    break;
                case DatePickerDateOrder.ymd:
                    pickerBuilders = new List<_ColumnBuilder>
                        {_buildYearPicker, _buildMonthPicker, _buildDayPicker};
                    columnWidths = new List<float>() {
                        estimatedColumnWidths[(int) _PickerColumnType.year],
                        estimatedColumnWidths[(int) _PickerColumnType.month],
                        estimatedColumnWidths[(int) _PickerColumnType.dayOfMonth]
                    };
                    break;
                case DatePickerDateOrder.ydm:
                    pickerBuilders = new List<_ColumnBuilder>
                        {_buildYearPicker, _buildDayPicker, _buildMonthPicker};
                    columnWidths = new List<float> {
                        estimatedColumnWidths[(int) _PickerColumnType.year],
                        estimatedColumnWidths[(int) _PickerColumnType.dayOfMonth],
                        estimatedColumnWidths[(int) _PickerColumnType.month]
                    };
                    break;
                default:
                    D.assert(false, () => "date order is not specified");
                    break;
            }

            List<Widget> pickers = new List<Widget>();
            for (int i = 0; i < columnWidths.Count; i++) {
                var _i = i;
                float offAxisFraction = (_i - 1) * 0.3f;
                EdgeInsets padding = EdgeInsets.only(right: CupertinoDatePickerUtils._kDatePickerPadSize);

                pickers.Add(new LayoutId(
                    id: _i,
                    child: pickerBuilders[_i](
                        offAxisFraction,
                        (BuildContext _context, Widget child) => {
                            return new Container(
                                alignment: _i == columnWidths.Count - 1
                                    ? alignCenterLeft
                                    : alignCenterRight,
                                padding: _i == 0 ? null : padding,
                                child: new Container(
                                    alignment: _i == 0 ? alignCenterLeft : alignCenterRight,
                                    width: columnWidths[_i] + CupertinoDatePickerUtils._kDatePickerPadSize,
                                    child: child
                                )
                            );
                        }
                    )
                ));
            }

            return new MediaQuery(
                data: new MediaQueryData(textScaleFactor: 1.0f),
                child: new NotificationListener<ScrollEndNotification>(
                    onNotification: _keepInValidRange,
                    child: DefaultTextStyle.merge(
                        style: CupertinoDatePickerUtils._kDefaultPickerTextStyle,
                        child: new CustomMultiChildLayout(
                            layoutDelegate: new _DatePickerLayoutDelegate(
                                columnWidths: columnWidths
                            ),
                            children:
                            pickers
                        )
                    )
                )
            );
        }
    }

    public enum CupertinoTimerPickerMode {
        hm,
        ms,
        hms,
    }

    public class CupertinoTimerPicker : StatefulWidget {
        public CupertinoTimerPicker(
            ValueChanged<TimeSpan> onTimerDurationChanged,
            CupertinoTimerPickerMode mode = CupertinoTimerPickerMode.hms,
            TimeSpan initialTimerDuration = new TimeSpan(),
            int minuteInterval = 1,
            int secondInterval = 1
        ) {
            D.assert(onTimerDurationChanged != null);
            D.assert(initialTimerDuration >= TimeSpan.Zero);
            D.assert(initialTimerDuration < new TimeSpan(1, 0, 0, 0));
            D.assert(minuteInterval > 0 && 60 % minuteInterval == 0);
            D.assert(secondInterval > 0 && 60 % secondInterval == 0);
            D.assert((int) initialTimerDuration.TotalMinutes % minuteInterval == 0);
            D.assert((int) initialTimerDuration.TotalSeconds % secondInterval == 0);
            this.onTimerDurationChanged = onTimerDurationChanged;
            this.mode = mode;
            this.initialTimerDuration = initialTimerDuration;
            this.minuteInterval = minuteInterval;
            this.secondInterval = secondInterval;
        }

        public readonly CupertinoTimerPickerMode mode;
        public readonly TimeSpan initialTimerDuration;
        public readonly int minuteInterval;
        public readonly int secondInterval;
        public readonly ValueChanged<TimeSpan> onTimerDurationChanged;

        public override State createState() {
            return new _CupertinoTimerPickerState();
        }
    }

    class _CupertinoTimerPickerState : State<CupertinoTimerPicker> {
        CupertinoLocalizations localizations;
        Alignment alignCenterLeft;
        Alignment alignCenterRight;
        int selectedHour;
        int selectedMinute;
        int selectedSecond;

        public override void initState() {
            base.initState();
            selectedMinute = (int) widget.initialTimerDuration.TotalMinutes % 60;
            if (widget.mode != CupertinoTimerPickerMode.ms) {
                selectedHour = (int) widget.initialTimerDuration.TotalHours;
            }

            if (widget.mode != CupertinoTimerPickerMode.hm) {
                selectedSecond = (int) widget.initialTimerDuration.TotalSeconds % 60;
            }
        }

        Widget _buildLabel(string text) {
            return new Text(
                text,
                textScaleFactor: 0.8f,
                style: new TextStyle(fontWeight: FontWeight.w600)
            );
        }

        public override void didChangeDependencies() {
            base.didChangeDependencies();
            localizations = CupertinoLocalizations.of(context);
            alignCenterLeft = Alignment.centerLeft;
            alignCenterRight = Alignment.centerRight;
        }

        Widget _buildHourPicker() {
            return new CupertinoPicker(
                scrollController: new FixedExtentScrollController(initialItem: selectedHour),
                offAxisFraction: -0.5f,
                itemExtent: CupertinoDatePickerUtils._kItemExtent,
                backgroundColor: CupertinoDatePickerUtils._kBackgroundColor,
                onSelectedItemChanged: (int index) => {
                    setState(() => {
                        selectedHour = index;
                        widget.onTimerDurationChanged(
                            new TimeSpan(
                                hours: selectedHour,
                                minutes: selectedMinute,
                                seconds: selectedSecond));
                    });
                },
                children: CupertinoDatePickerUtils.listGenerate(24, (int index) => {
                    float hourLabelWidth = widget.mode == CupertinoTimerPickerMode.hm
                        ? CupertinoDatePickerUtils._kPickerWidth / 4
                        : CupertinoDatePickerUtils._kPickerWidth / 6;
                    return new Container(
                        alignment: alignCenterRight,
                        padding: EdgeInsets.only(right: hourLabelWidth),
                        child: new Container(
                            alignment: alignCenterRight,
                            padding: EdgeInsets.symmetric(horizontal: 2.0f),
                            child: new Text(localizations.timerPickerHour(index))
                        )
                    );
                })
            );
        }

        Widget _buildHourColumn() {
            Widget hourLabel = new IgnorePointer(
                child: new Container(
                    alignment: alignCenterRight,
                    child: new Container(
                        alignment: alignCenterLeft,
                        padding: EdgeInsets.symmetric(horizontal: 2.0f),
                        width: widget.mode == CupertinoTimerPickerMode.hm
                            ? CupertinoDatePickerUtils._kPickerWidth / 4
                            : CupertinoDatePickerUtils._kPickerWidth / 6,
                        child: _buildLabel(localizations.timerPickerHourLabel(selectedHour))
                    )
                )
            );
            return new Stack(
                children: new List<Widget> {
                    _buildHourPicker(),
                    hourLabel
                }
            );
        }

        Widget _buildMinutePicker() {
            float offAxisFraction;
            if (widget.mode == CupertinoTimerPickerMode.hm) {
                offAxisFraction = 0.5f;
            }
            else if (widget.mode == CupertinoTimerPickerMode.hms) {
                offAxisFraction = 0.0f;
            }
            else {
                offAxisFraction = -0.5f;
            }

            return new CupertinoPicker(
                scrollController: new FixedExtentScrollController(
                    initialItem: selectedMinute / widget.minuteInterval
                ),
                offAxisFraction: offAxisFraction,
                itemExtent: CupertinoDatePickerUtils._kItemExtent,
                backgroundColor: CupertinoDatePickerUtils._kBackgroundColor,
                onSelectedItemChanged: (int index) => {
                    setState(() => {
                        selectedMinute = index * widget.minuteInterval;
                        widget.onTimerDurationChanged(
                            new TimeSpan(
                                hours: selectedHour,
                                minutes: selectedMinute,
                                seconds: selectedSecond));
                    });
                },
                children: CupertinoDatePickerUtils.listGenerate(60 / widget.minuteInterval, (int index) => {
                    int minute = index * widget.minuteInterval;
                    if (widget.mode == CupertinoTimerPickerMode.ms) {
                        return new Container(
                            alignment: alignCenterRight,
                            padding: EdgeInsets.only(right: CupertinoDatePickerUtils._kPickerWidth / 4),
                            child: new Container(
                                alignment: alignCenterRight,
                                padding: EdgeInsets.symmetric(horizontal: 2.0f),
                                child: new Text(localizations.timerPickerMinute(minute))
                            )
                        );
                    }
                    else {
                        return new Container(
                            alignment: alignCenterLeft,
                            child: new Container(
                                alignment: alignCenterRight,
                                width: widget.mode == CupertinoTimerPickerMode.hm
                                    ? CupertinoDatePickerUtils._kPickerWidth / 10
                                    : CupertinoDatePickerUtils._kPickerWidth / 6,
                                padding: EdgeInsets.symmetric(horizontal: 2.0f),
                                child:
                                new Text(localizations.timerPickerMinute(minute))
                            )
                        );
                    }
                })
            );
        }

        Widget _buildMinuteColumn() {
            Widget minuteLabel;
            if (widget.mode == CupertinoTimerPickerMode.hm) {
                minuteLabel = new IgnorePointer(
                    child: new Container(
                        alignment: alignCenterLeft,
                        padding: EdgeInsets.only(left: CupertinoDatePickerUtils._kPickerWidth / 10),
                        child: new Container(
                            alignment: alignCenterLeft,
                            padding: EdgeInsets.symmetric(horizontal: 2.0f),
                            child: _buildLabel(localizations.timerPickerMinuteLabel(selectedMinute))
                        )
                    )
                );
            }
            else {
                minuteLabel = new IgnorePointer(
                    child: new Container(
                        alignment: alignCenterRight,
                        child: new Container(
                            alignment: alignCenterLeft,
                            width: widget.mode == CupertinoTimerPickerMode.ms
                                ? CupertinoDatePickerUtils._kPickerWidth / 4
                                : CupertinoDatePickerUtils._kPickerWidth / 6,
                            padding: EdgeInsets.symmetric(horizontal: 2.0f),
                            child: _buildLabel(localizations.timerPickerMinuteLabel(selectedMinute))
                        )
                    )
                );
            }

            return new Stack(
                children: new List<Widget> {
                    _buildMinutePicker(),
                    minuteLabel
                }
            );
        }

        Widget _buildSecondPicker() {
            float offAxisFraction = 0.5f;
            float secondPickerWidth = widget.mode == CupertinoTimerPickerMode.ms
                ? CupertinoDatePickerUtils._kPickerWidth / 10
                : CupertinoDatePickerUtils._kPickerWidth / 6;
            return new CupertinoPicker(
                scrollController: new FixedExtentScrollController(
                    initialItem: selectedSecond / widget.secondInterval
                ),
                offAxisFraction: offAxisFraction,
                itemExtent: CupertinoDatePickerUtils._kItemExtent,
                backgroundColor: CupertinoDatePickerUtils._kBackgroundColor,
                onSelectedItemChanged: (int index) => {
                    setState(() => {
                        selectedSecond = index * widget.secondInterval;
                        widget.onTimerDurationChanged(
                            new TimeSpan(
                                hours: selectedHour,
                                minutes: selectedMinute,
                                seconds: selectedSecond));
                    });
                },
                children: CupertinoDatePickerUtils.listGenerate(60 / widget.secondInterval, (int index) => {
                    int second = index * widget.secondInterval;
                    return new Container(
                        alignment: alignCenterLeft,
                        child: new Container(
                            alignment: alignCenterRight,
                            padding: EdgeInsets.symmetric(horizontal: 2.0f),
                            width: secondPickerWidth,
                            child: new Text(localizations.timerPickerSecond(second))
                        )
                    );
                })
            );
        }

        Widget _buildSecondColumn() {
            float secondPickerWidth = widget.mode == CupertinoTimerPickerMode.ms
                ? CupertinoDatePickerUtils._kPickerWidth / 10
                : CupertinoDatePickerUtils._kPickerWidth / 6;
            Widget secondLabel = new IgnorePointer(
                child: new Container(
                    alignment: alignCenterLeft,
                    padding: EdgeInsets.only(left: secondPickerWidth),
                    child: new Container(
                        alignment: alignCenterLeft,
                        padding: EdgeInsets.symmetric(horizontal: 2.0f),
                        child: _buildLabel(localizations.timerPickerSecondLabel(selectedSecond))
                    )
                )
            );
            return new Stack(
                children: new List<Widget> {
                    _buildSecondPicker(),
                    secondLabel
                }
            );
        }

        public override Widget build(BuildContext context) {
            Widget picker;
            if (widget.mode == CupertinoTimerPickerMode.hm) {
                picker = new Row(
                    children: new List<Widget> {
                        new Expanded(child: _buildHourColumn()),
                        new Expanded(child: _buildMinuteColumn()),
                    }
                );
            }
            else if (widget.mode == CupertinoTimerPickerMode.ms) {
                picker = new Row(
                    children: new List<Widget> {
                        new Expanded(child: _buildMinuteColumn()),
                        new Expanded(child: _buildSecondColumn()),
                    }
                );
            }
            else {
                picker = new Row(
                    children: new List<Widget> {
                        new Expanded(child: _buildHourColumn()),
                        new Container(
                            width: CupertinoDatePickerUtils._kPickerWidth / 3,
                            child: _buildMinuteColumn()
                        ),
                        new Expanded(child: _buildSecondColumn()),
                    }
                );
            }

            return new MediaQuery(
                data: new MediaQueryData(
                    textScaleFactor: 1.0f
                ),
                child: picker
            );
        }
    }
}
/*
using System;
using System.Collections.Generic;
using Unity.UIWidgets.animation;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.rendering;
using Unity.UIWidgets.scheduler2;
using Unity.UIWidgets.ui;
using Unity.UIWidgets.widgets;
using TextStyle = Unity.UIWidgets.painting.TextStyle;

namespace Unity.UIWidgets.cupertino {
    static class CupertinoDatePickerUtils {
        public const float _kItemExtent = 32.0f;
        public const float _kPickerWidth = 320.0f;
        public const float _kPickerHeight = 216.0f;
        public const bool _kUseMagnifier = true;
        public const float _kMagnification = 1.05f;
        public const float _kDatePickerPadSize = 12.0f;
        public const float _kSqueeze = 1.25f;
        public static Color _kBackgroundColor = CupertinoColors.white;

        public static TextStyle _kDefaultPickerTextStyle = new TextStyle(
            letterSpacing: -0.83f
        );
        public const float _kTimerPickerHalfColumnPadding = 2f;
        public const float _kTimerPickerLabelPadSize = 4.5f;
        public const float _kTimerPickerLabelFontSize = 17.0f;
        public const float _kTimerPickerColumnIntrinsicWidth = 106f;
        public const float _kTimerPickerNumberLabelFontSize = 23f;

        public static TextStyle _themeTextStyle(BuildContext context, bool isValid = true ) {
            TextStyle style = CupertinoTheme.of(context).textTheme.dateTimePickerTextStyle;
            return isValid ? style : style.copyWith(color: CupertinoDynamicColor.resolve(CupertinoColors.inactiveGray, context));
        }

        public static void _animateColumnControllerToItem(FixedExtentScrollController controller, int targetItem) {
            controller.animateToItem(
                targetItem,
                curve: Curves.easeInOut,
                duration: new TimeSpan(0,0,0,0,200)
                );
        }
        public delegate Widget listGenerateDelegate(int index);

        public static List<Widget> listGenerate(int count, listGenerateDelegate func) {
            var list = new List<Widget>();
            for (int i = 0; i < count; i++) {
                list.Add(func(i));
            }

            return list;
        }
    }

    public class _DatePickerLayoutDelegate : MultiChildLayoutDelegate {
        public _DatePickerLayoutDelegate(
            List<float> columnWidths,
            int textDirectionFactor
        ) {
            D.assert(columnWidths != null);
            D.assert(textDirectionFactor != null);
            this.columnWidths = columnWidths;
            this.textDirectionFactor = textDirectionFactor ;
        }

        public readonly List<float> columnWidths;
        public readonly int textDirectionFactor;

        public override void performLayout(Size size) {
            float remainingWidth = size.width;
            for (int i = 0; i < columnWidths.Count; i++) {
                remainingWidth -= columnWidths[i] + CupertinoDatePickerUtils._kDatePickerPadSize * 2;
            }

            float currentHorizontalOffset = 0.0f;
            for (int i = 0; i < columnWidths.Count; i++) {
                int index = textDirectionFactor == 1 ? i : columnWidths.Count - i - 1;
                float childWidth = columnWidths[index] + CupertinoDatePickerUtils._kDatePickerPadSize * 2;
                if (index == 0 || index == columnWidths.Count - 1) {
                    childWidth += remainingWidth / 2;
                }
                D.assert(()=>{
                    if (childWidth < 0) {
                        throw new UIWidgetsError(
                            "Insufficient horizontal space to render the " +
                            "CupertinoDatePicker because the parent is too narrow at " +
                            $"{size.width}px.\n" + "An additional ${-remainingWidth}px is needed to avoid " +
                            "overlapping columns."
                        );
                    }
                    return true;
                });
                layoutChild(index, BoxConstraints.tight(new Size(childWidth, size.height)));
                positionChild(index, new Offset(currentHorizontalOffset, 0.0f));
                currentHorizontalOffset += childWidth;
            }       
            
        }

        public override bool shouldRelayout(MultiChildLayoutDelegate oldDelegate) {
       
            return columnWidths != ((_DatePickerLayoutDelegate) oldDelegate).columnWidths
                || textDirectionFactor != ((_DatePickerLayoutDelegate) oldDelegate).textDirectionFactor;
        }
    }

    public enum CupertinoDatePickerMode {
        time,
        date,
        dateAndTime,
    }

    enum _PickerColumnType {
        dayOfMonth,
        month,
        year,
        date,
        hour,
        minute,
        dayPeriod,
    }

    public class CupertinoDatePicker : StatefulWidget {
        public CupertinoDatePicker(
            Key key = null,
            CupertinoDatePickerMode mode = CupertinoDatePickerMode.dateAndTime,
            ValueChanged<DateTime> onDateTimeChanged = null,
            DateTime? initialDateTime = null,
            DateTime? minimumDate = null,
            DateTime? maximumDate = null,
            int minimumYear = 1,
            int? maximumYear = null,
            int minuteInterval = 1,
            bool use24hFormat = false,
            Color backgroundColor = null
        ) {
            this.initialDateTime = initialDateTime ?? DateTime.Now;
            D.assert(onDateTimeChanged != null);
            D.assert(
                minuteInterval > 0 && 60 % minuteInterval == 0,
                () => "minute interval is not a positive integer factor of 60"
            );
            D.assert(this.initialDateTime != null);
            D.assert(
                mode != CupertinoDatePickerMode.dateAndTime || minimumDate == null ||
                !(this.initialDateTime < minimumDate),
                () => "initial date is before minimum date"
            );
            D.assert(
                mode != CupertinoDatePickerMode.dateAndTime || maximumDate == null ||
                !(this.initialDateTime > maximumDate),
                () => "initial date is after maximum date"
            );
            D.assert(
                mode != CupertinoDatePickerMode.date || (minimumYear >= 1 && this.initialDateTime.Year >= minimumYear),
                () => "initial year is not greater than minimum year, or mininum year is not positive"
            );
            D.assert(
                mode != CupertinoDatePickerMode.date || maximumYear == null || this.initialDateTime.Year <= maximumYear,
                () => "initial year is not smaller than maximum year"
            );
            D.assert(
                this.initialDateTime.Minute % minuteInterval == 0,
                () => "initial minute is not divisible by minute interval"
            );
            this.onDateTimeChanged = onDateTimeChanged;
            this.mode = mode;
            this.minimumDate = minimumDate;
            this.maximumDate = maximumDate;
            this.minimumYear = minimumYear ;
            this.maximumYear = maximumYear;
            this.minuteInterval = minuteInterval ;
            this.use24hFormat = use24hFormat;
            this.backgroundColor = backgroundColor;
        }

        public readonly Color backgroundColor;
        public readonly CupertinoDatePickerMode mode;
        public readonly DateTime initialDateTime;
        public readonly DateTime? minimumDate;
        public readonly DateTime? maximumDate;
        public readonly int minimumYear;
        public readonly int? maximumYear;
        public readonly int minuteInterval;
        public readonly bool use24hFormat;
        public readonly ValueChanged<DateTime> onDateTimeChanged;

        public override State createState() {
            if (mode == CupertinoDatePickerMode.time || mode == CupertinoDatePickerMode.dateAndTime) {
                return new _CupertinoDatePickerDateTimeState();
            }
            else {
                return new _CupertinoDatePickerDateState();
            }
        }

        internal static float _getColumnWidth(
            _PickerColumnType columnType,
            CupertinoLocalizations localizations,
            BuildContext context
        ) {
            string longestText = "";
            switch (columnType) {
                case _PickerColumnType.date:

                    for (int i = 1; i <= 12; i++) {
                        string date =
                            localizations.datePickerMediumDate(new DateTime(2018, i, 25));
                        if (longestText.Length < date.Length) {
                            longestText = date;
                        }
                    }

                    break;
                case _PickerColumnType.hour:
                    for (int i = 0; i < 24; i++) {
                        string hour = localizations.datePickerHour(i);
                        if (longestText.Length < hour.Length) {
                            longestText = hour;
                        }
                    }

                    break;
                case _PickerColumnType.minute:
                    for (int i = 0; i < 60; i++) {
                        string minute = localizations.datePickerMinute(i);
                        if (longestText.Length < minute.Length) {
                            longestText = minute;
                        }
                    }

                    break;
                case _PickerColumnType.dayPeriod:
                    longestText =
                        localizations.anteMeridiemAbbreviation.Length > localizations.postMeridiemAbbreviation.Length
                            ? localizations.anteMeridiemAbbreviation
                            : localizations.postMeridiemAbbreviation;
                    break;
                case _PickerColumnType.dayOfMonth:
                    for (int i = 1; i <= 31; i++) {
                        string dayOfMonth = localizations.datePickerDayOfMonth(i);
                        if (longestText.Length < dayOfMonth.Length) {
                            longestText = dayOfMonth;
                        }
                    }

                    break;
                case _PickerColumnType.month:
                    for (int i = 1; i <= 12; i++) {
                        string month = localizations.datePickerMonth(i);
                        if (longestText.Length < month.Length) {
                            longestText = month;
                        }
                    }

                    break;
                case _PickerColumnType.year:
                    longestText = localizations.datePickerYear(2018);
                    break;
            }

            D.assert(longestText != "", () => "column type is not appropriate");
            TextPainter painter = new TextPainter(
                text: new TextSpan(
                    style: DefaultTextStyle.of(context).style,
                    text: longestText
                ),
                textDirection: Directionality.of(context)
            );


            painter.layout();
            return painter.maxIntrinsicWidth;
        }
    }

    delegate Widget _ColumnBuilder(float offAxisFraction, TransitionBuilder itemPositioningBuilder);

    class _CupertinoDatePickerDateTimeState : State<CupertinoDatePicker> {
        public static readonly float _kMaximumOffAxisFraction = 0.45f;
        public CupertinoLocalizations localizations;
        public int textDirectionFactor;
        public Alignment alignCenterLeft;
        public Alignment alignCenterRight;
        public DateTime initialDateTime;

        public int selectedDayFromInitial
        {
            get {
                switch (widget.mode) {
                    case CupertinoDatePickerMode.dateAndTime:
                        return dateController.hasClients ? dateController.selectedItem : 0;
                    case CupertinoDatePickerMode.time:
                        return 0;
                    case CupertinoDatePickerMode.date:
                        break;
                }
                D.assert(
                    false,()=>"$runtimeType is only meant for dateAndTime mode or time mode");
                return 0;
            }
        }
        public FixedExtentScrollController dateController;

        public int selectedHour
        {
            get{
                return _selectedHour(selectedAmPm, _selectedHourIndex);               
            }
        }

        public int _selectedHourIndex {
            get {
                return hourController. hasClients ? hourController. selectedItem % 24 : initialDateTime.hour;
            }
        }
        public int _selectedHour(int selectedAmPm, int selectedHour) {
            return _isHourRegionFlipped(selectedAmPm) ? (selectedHour + 12) % 24 : selectedHour;
        }
        public FixedExtentScrollController hourController;

        public int selectedMinute
        {
            get {
                return minuteController.hasClients
                    ? minuteController.selectedItem * widget.minuteInterval % 60
                    : initialDateTime.minute;
            }
            
        }
        // The controller of the minute column.
        FixedExtentScrollController minuteController;
        
        public int previousHourIndex;

        public int selectedAmPm;

        public bool isHourRegionFlipped => _isHourRegionFlipped(selectedAmPm);
        bool _isHourRegionFlipped(int selectedAmPm) => selectedAmPm != meridiemRegion;
        
        int meridiemRegion;
        
        FixedExtentScrollController meridiemController;

        bool isDatePickerScrolling = false;
        bool isHourPickerScrolling = false;
        bool isMinutePickerScrolling = false;
        bool isMeridiemPickerScrolling = false;

        bool isScrolling { 
            get{return isDatePickerScrolling
                       || isHourPickerScrolling
                       || isMinutePickerScrolling
                       || isMeridiemPickerScrolling;
            }
        }
        
        public static Dictionary<int, float> estimatedColumnWidths = new Dictionary<int, float>();

        public override void initState() {
            base.initState();
            initialDateTime = widget.initialDateTime;

            
            selectedAmPm = (int)(initialDateTime.hour / 12);
            meridiemRegion = selectedAmPm;
            meridiemController = new FixedExtentScrollController(initialItem: selectedAmPm);
            hourController = new FixedExtentScrollController(initialItem: initialDateTime.hour);
            minuteController = new FixedExtentScrollController(initialItem: (int)(initialDateTime.minute / widget.minuteInterval));
            dateController = new FixedExtentScrollController(initialItem: 0);

            PaintingBinding.instance.systemFonts.addListener(_handleSystemFontsChange);
        }
        public void _handleSystemFontsChange () {
            setState(()=>{
                estimatedColumnWidths.Clear();
            });
        }
        public override void dispose() {
            dateController.dispose();
            hourController.dispose();
            minuteController.dispose();
            meridiemController.dispose();

            PaintingBinding.instance.systemFonts.removeListener(_handleSystemFontsChange);
            base.dispose();
         }
        public override void didUpdateWidget(StatefulWidget oldWidget) {
            base.didUpdateWidget(oldWidget);
            D.assert(
                ((CupertinoDatePicker) oldWidget).mode == widget.mode,
                () => "The CupertinoDatePicker's mode cannot change once it's built"
            );
            if (!widget.use24hFormat && oldWidget.use24hFormat) {
                meridiemController.dispose();
                meridiemController = FixedExtentScrollController(initialItem: selectedAmPm);
            }
        }

        public override void didChangeDependencies() {
            base.didChangeDependencies();
            textDirectionFactor = Directionality.of(context) == TextDirection.ltr ? 1 : -1;
            localizations = CupertinoLocalizations.of(context);

            alignCenterLeft = textDirectionFactor == 1 ? Alignment.centerLeft : Alignment.centerRight;
            alignCenterRight = textDirectionFactor == 1 ? Alignment.centerRight : Alignment.centerLeft;

            estimatedColumnWidths.Clear();
    
        }

        float _getEstimatedColumnWidth(_PickerColumnType columnType) {
            if (!estimatedColumnWidths.TryGetValue((int) columnType, out float _)) {
                estimatedColumnWidths[(int) columnType] =
                    CupertinoDatePicker._getColumnWidth(columnType, localizations, context);
            }

            return estimatedColumnWidths[(int) columnType];
        }

        public DateTime selectedDateTime {
            get{
                return new DateTime(
                        initialDateTime.year,
                            initialDateTime.month,
                            initialDateTime.day + selectedDayFromInitial,
                            selectedHour,
                            selectedMinute
                        );
            }
           
        }
        
        public void _onSelectedItemChange(int index) {
            DateTime selected = selectedDateTime;

            bool isDateInvalid = widget.minimumDate?.isAfter(selected) == true
                                    || widget.maximumDate?.isBefore(selected) == true;

            if (isDateInvalid)
                return;

            widget.onDateTimeChanged(selected);
        }
        
        Widget _buildMediumDatePicker(float offAxisFraction, TransitionBuilder itemPositioningBuilder) {
            //// tbc notification
            return 
                new NotificationListener<ScrollNotification>(
                    onNotification: (ScrollNotification notification)=> {
                        if (notification is ScrollStartNotification) {
                            isDatePickerScrolling = true;
                        } else if (notification is ScrollEndNotification) {
                            isDatePickerScrolling = false;
                            _pickerDidStopScrolling();
                        }

                        return false;
                    },
                    child: CupertinoPicker.builder(
                            scrollController: dateController,
                            offAxisFraction: offAxisFraction,
                            itemExtent:CupertinoDatePickerUtils. _kItemExtent,
                            useMagnifier:CupertinoDatePickerUtils. _kUseMagnifier,
                            magnification: CupertinoDatePickerUtils._kMagnification,
                            backgroundColor: widget.backgroundColor,
                            squeeze: CupertinoDatePickerUtils._kSqueeze,
                            onSelectedItemChanged: (int index) => {
                            _onSelectedItemChange(index);
                            },
                            itemBuilder: (BuildContext context, int index) => {
                                DateTime rangeStart = new DateTime(
                                    initialDateTime.year,
                                    initialDateTime.month,
                                    initialDateTime.day + index
                                );

                                // Exclusive.
                                DateTime rangeEnd = new DateTime(
                                    initialDateTime.year,
                                    initialDateTime.month,
                                    initialDateTime.day + index + 1
                                );

                                DateTime now = DateTime.Now;

                                if (widget.minimumDate?.isAfter(rangeEnd) == true)
                                return null;
                            if (widget.maximumDate?.isAfter(rangeStart) == false)
                                return null;

                            string dateText = rangeStart == new DateTime(now.year, now.month, now.day)
                                ? localizations.todayLabel
                                : localizations.datePickerMediumDate(rangeStart);

                            return itemPositioningBuilder(
                                context,
                                new Text(dateText, style: _themeTextStyle(context))
                            );
                            }
                        )
                    );
            
        }
         bool _isValidHour(int meridiemIndex, int hourIndex) {
            DateTime rangeStart = new DateTime(
                initialDateTime.year,
                initialDateTime.month,
                initialDateTime.day + selectedDayFromInitial,
                _selectedHour(meridiemIndex, hourIndex),
                0
            );

            // The end value of the range is exclusive, i.e. [rangeStart, rangeEnd).
            DateTime rangeEnd = rangeStart.Add( new TimeSpan(10,0,0));

            return (widget.minimumDate?.isBefore(rangeEnd) ?? true)
                && !(widget.maximumDate?.isBefore(rangeStart) ?? false);
        }

        Widget _buildHourPicker(float offAxisFraction, TransitionBuilder itemPositioningBuilder) { 
            return new NotificationListener<ScrollNotification>(
                    onNotification: (ScrollNotification notification) => {
                    if (notification is ScrollStartNotification) {
                        isHourPickerScrolling = true;
                    } else if (notification is ScrollEndNotification) {
                        isHourPickerScrolling = false;
                        _pickerDidStopScrolling();
                    }

                    return false;
                },
                child: 
                    new CupertinoPicker(
                    scrollController: dateController,//new FixedExtentScrollController(initialItem: selectedHour),
                    offAxisFraction: offAxisFraction,
                    itemExtent: CupertinoDatePickerUtils._kItemExtent,
                    useMagnifier: CupertinoDatePickerUtils._kUseMagnifier,
                    magnification: CupertinoDatePickerUtils._kMagnification,
                    backgroundColor: widget.backgroundColor,//CupertinoDatePickerUtils._kBackgroundColor,
                    squeeze: CupertinoDatePickerUtils._kSqueeze,
                    onSelectedItemChanged: (int index) => {
                        bool regionChanged = meridiemRegion != index % 12;
                        bool debugIsFlipped = isHourRegionFlipped;

                        if (regionChanged) {
                            meridiemRegion = index % 12;
                            selectedAmPm = 1 - selectedAmPm;
                        }

                        if (!widget.use24hFormat && regionChanged) {
                            meridiemController.animateToItem(
                            selectedAmPm,
                            duration: new TimeSpan(0,0,0,0, 300),
                            curve: Curves.easeOut
                            );
                        } 
                        else {
                            _onSelectedItemChange(index);
                        }

                        D.assert(debugIsFlipped == isHourRegionFlipped);
                       
                    },
                    children: CupertinoDatePickerUtils.listGenerate(24, (index) => {
                        int hour = isHourRegionFlipped ? (index + 12) % 24 : index;//hour = index;
                        int displayHour = widget.use24hFormat ? hour : (hour + 11) % 12 + 1;
                        return itemPositioningBuilder(context,
                            new Text(
                                localizations.datePickerHour(hour),
                                semanticsLabel: localizations.datePickerHourSemanticsLabel(displayHour),
                                style: _themeTextStyle(context, isValid: _isValidHour(selectedAmPm, index))
                            )
                        );
                    }
                        ),
                    looping: true
                );
        }

        Widget _buildMinutePicker(float offAxisFraction, TransitionBuilder itemPositioningBuilder) {
            return new NotificationListener<ScrollNotification>(
                onNotification: (ScrollNotification notification) => {
                if (notification is ScrollStartNotification) {
                    isMinutePickerScrolling = true;
                } else if (notification is ScrollEndNotification) {
                    isMinutePickerScrolling = false;
                    _pickerDidStopScrolling();
                }

                return false;
            },
            child:  
                new CupertinoPicker(
                scrollController:  minuteController,
                offAxisFraction: offAxisFraction,
                itemExtent: CupertinoDatePickerUtils._kItemExtent,
                useMagnifier: CupertinoDatePickerUtils._kUseMagnifier,
                magnification: CupertinoDatePickerUtils._kMagnification,
                backgroundColor: widget.backgroundColor,//CupertinoDatePickerUtils._kBackgroundColor,
                squeeze: CupertinoDatePickerUtils._kSqueeze,
               
                children: CupertinoDatePickerUtils.listGenerate((int)(60 / widget.minuteInterval), (int index) =>{
                    int minute = index * widget.minuteInterval;

                    DateTime date = new DateTime(
                        initialDateTime.year,
                        initialDateTime.month,
                        initialDateTime.day + selectedDayFromInitial,
                        selectedHour,
                        minute
                    );

                    bool isInvalidMinute = (widget.minimumDate?.isAfter(date) ?? false)
                                            || (widget.maximumDate?.isBefore(date) ?? false);

                    return itemPositioningBuilder(
                        context,
                        new Text(
                        localizations.datePickerMinute(minute),
                        semanticsLabel: localizations.datePickerMinuteSemanticsLabel(minute),
                        style: _themeTextStyle(context, isValid: !isInvalidMinute)
                        )
                    );
                }),
                looping: true
            );
        }

        Widget _buildAmPmPicker(float offAxisFraction, TransitionBuilder itemPositioningBuilder) {
            return new NotificationListener<ScrollNotification>(
                onNotification: (ScrollNotification notification) => {
                if (notification is ScrollStartNotification) {
                    isMeridiemPickerScrolling = true;
                } else if (notification is ScrollEndNotification) {
                    isMeridiemPickerScrolling = false;
                    _pickerDidStopScrolling();
                }

                return false;
            },
                child: new CupertinoPicker(
                    scrollController:  meridiemController,//amPmController,
                    offAxisFraction: offAxisFraction,
                    itemExtent: CupertinoDatePickerUtils._kItemExtent,
                    useMagnifier: CupertinoDatePickerUtils._kUseMagnifier,
                    magnification: CupertinoDatePickerUtils._kMagnification,
                    backgroundColor: widget.backgroundColor,
                    squeeze:CupertinoDatePickerUtils._kSqueeze,
                    onSelectedItemChanged: (int index) => {
                        //selectedAmPm = index;
                        //widget.onDateTimeChanged(_getDateTime());
                        selectedAmPm = index;
                        D.assert(selectedAmPm == 0 || selectedAmPm == 1);
                        _onSelectedItemChange(index);
                    },
                    children: CupertinoDatePickerUtils.listGenerate(2, (int index) => {
                        return itemPositioningBuilder(context,
                            new Text(
                                index == 0
                                    ? localizations.anteMeridiemAbbreviation
                                    : localizations.postMeridiemAbbreviation,
                                style: _themeTextStyle(context, isValid: _isValidHour(index, _selectedHourIndex))
                            )
                        );
                    })
            );
        }

        void _pickerDidStopScrolling() {
            setState(()=>{ });

            if (isScrolling)
                return;
            DateTime selectedDate = selectedDateTime;

            bool minCheck = widget.minimumDate?.isAfter(selectedDate) ?? false;
            bool maxCheck = widget.maximumDate?.isBefore(selectedDate) ?? false;

            if (minCheck || maxCheck) {
                DateTime targetDate = minCheck ? widget.minimumDate : widget.maximumDate;
                _scrollToDate(targetDate, selectedDate);
            }
        }

        void _scrollToDate(DateTime newDate, DateTime fromDate) {
            D.assert(newDate != null);
            SchedulerBinding.instance.addPostFrameCallback((stamp => {
                if (fromDate.year != newDate.year || fromDate.month != newDate.month || fromDate.day != newDate.day) {
                    _animateColumnControllerToItem(dateController, selectedDayFromInitial);
                }

                if (fromDate.hour != newDate.hour) {
                    bool needsMeridiemChange = !widget.use24hFormat
                                                && (int)(fromDate.hour / 12) != (int)(newDate.hour / 12);
                    // In AM/PM mode, the pickers should not scroll all the way to the other hour region.
                    if (needsMeridiemChange) { 
                        _animateColumnControllerToItem(meridiemController, 1 - meridiemController.selectedItem);

                    // Keep the target item index in the current 12-h region.
                      int newItem = ((int)(hourController.selectedItem / 12)) * 12
                                        + (hourController.selectedItem + newDate.hour - fromDate.hour) % 12;
                    _animateColumnControllerToItem(hourController, newItem);
                    } else {
                    _animateColumnControllerToItem(
                        hourController,
                        hourController.selectedItem + newDate.hour - fromDate.hour,
                    );
                    }
                }

                if (fromDate.minute != newDate.minute) {
                    _animateColumnControllerToItem(minuteController, newDate.minute);
                }
                });
        }



        public override Widget build(BuildContext context) {
            List<float> columnWidths = new List<float>() {
                _getEstimatedColumnWidth(_PickerColumnType.hour),
                _getEstimatedColumnWidth(_PickerColumnType.minute),
            };

            List<_ColumnBuilder> pickerBuilders = new List<_ColumnBuilder> {
                _buildHourPicker, _buildMinutePicker,
            };

            if (!widget.use24hFormat) {
                if (localizations.datePickerDateTimeOrder == DatePickerDateTimeOrder.date_time_dayPeriod
                    || localizations.datePickerDateTimeOrder == DatePickerDateTimeOrder.time_dayPeriod_date) {
                    pickerBuilders.Add(_buildAmPmPicker);
                    columnWidths.Add(_getEstimatedColumnWidth(_PickerColumnType.dayPeriod));
                }
                else {
                    pickerBuilders.Insert(0, _buildAmPmPicker);
                    columnWidths.Insert(0, _getEstimatedColumnWidth(_PickerColumnType.dayPeriod));
                }
            }

            if (widget.mode == CupertinoDatePickerMode.dateAndTime) {
                if (localizations.datePickerDateTimeOrder == DatePickerDateTimeOrder.time_dayPeriod_date
                    || localizations.datePickerDateTimeOrder == DatePickerDateTimeOrder.dayPeriod_time_date) {
                    pickerBuilders.Add(_buildMediumDatePicker);
                    columnWidths.Add(_getEstimatedColumnWidth(_PickerColumnType.date));
                }
                else {
                    pickerBuilders.Insert(0, _buildMediumDatePicker);
                    columnWidths.Insert(0, _getEstimatedColumnWidth(_PickerColumnType.date));
                }
            }

            List<Widget> pickers = new List<Widget>();
            for (int i = 0; i < columnWidths.Count; i++) {
                float offAxisFraction = 0.0f;
                if (i == 0)
                    offAxisFraction = -_kMaximumOffAxisFraction * textDirectionFactor;
                else if (i >= 2 || columnWidths.Count == 2)
                    offAxisFraction = _kMaximumOffAxisFraction * textDirectionFactor;

                EdgeInsets padding = EdgeInsets.only(right: CupertinoDatePickerUtils._kDatePickerPadSize);
                if (i == columnWidths.Count - 1)
                    padding = padding.flipped;
                if (textDirectionFactor == -1)
                    padding = padding.flipped;

                pickers.Add(new LayoutId(
                    id: i,
                    child: pickerBuilders[i](
                        offAxisFraction,
                        (BuildContext _context, Widget child) => {
                            return new Container(
                                alignment: i == columnWidths.Count - 1
                                    ? alignCenterLeft
                                    : alignCenterRight,
                                padding: padding,
                                child: new Container(
                                    alignment: i == columnWidths.Count - 1
                                        ? alignCenterLeft
                                        : alignCenterRight,
                                    width: i == 0 || i == columnWidths.Count - 1
                                        ? (float?) null
                                        : columnWidths[i] + CupertinoDatePickerUtils._kDatePickerPadSize,
                                    child: child
                                )
                            );
                        }
                    )
                ));
            }

            return new MediaQuery(
                data: new MediaQueryData(textScaleFactor: 1.0f),
                child: DefaultTextStyle.merge(
                    style: CupertinoDatePickerUtils._kDefaultPickerTextStyle,
                    child: new CustomMultiChildLayout(
                        layoutDelegate: new _DatePickerLayoutDelegate(
                            columnWidths: columnWidths,
                             textDirectionFactor: textDirectionFactor
                        ),
                        children: pickers
                    )
                )
            );
        }
    }

    class _CupertinoDatePickerDateState : State<CupertinoDatePicker> {
        int textDirectionFactor;
        CupertinoLocalizations localizations;

        Alignment alignCenterLeft;
        Alignment alignCenterRight;

        int selectedDay;
        int selectedMonth;
        int selectedYear;

        FixedExtentScrollController dayController;
        FixedExtentScrollController monthController;
        FixedExtentScrollController yearController;

        bool isDayPickerScrolling = false;
        bool isMonthPickerScrolling = false;
        bool isYearPickerScrolling = false;

        bool isScrolling 
        {
            get{
                return isDayPickerScrolling || isMonthPickerScrolling || isYearPickerScrolling;
            }
        }

        Dictionary<int, float> estimatedColumnWidths = new Dictionary<int, float>();

        public override void initState() {
            base.initState();
            selectedDay = widget.initialDateTime.Day;
            selectedMonth = widget.initialDateTime.Month;
            selectedYear = widget.initialDateTime.Year;
            dayController = new FixedExtentScrollController(initialItem: selectedDay - 1);
            monthController = new FixedExtentScrollController(initialItem: selectedMonth - 1);
            yearController = new FixedExtentScrollController(initialItem: selectedYear);
            /// tbc??? 
            PaintingBinding.instance.systemFonts.addListener(_handleSystemFontsChange);
        }

        void _handleSystemFontsChange() {
            setState(() => {
            // System fonts change might cause the text layout width to change.
            _refreshEstimatedColumnWidths();
            });
        }
        
        public override void dispose() {
            dayController.dispose();
            monthController.dispose();
            yearController.dispose();

            PaintingBinding.instance.systemFonts.removeListener(_handleSystemFontsChange);
            base.dispose();
        }
        public override void didChangeDependencies() {
            base.didChangeDependencies();

            textDirectionFactor = Directionality.of(context) == TextDirection.ltr ? 1 : -1;
            localizations = CupertinoLocalizations.of(context);
            
            alignCenterLeft = textDirectionFactor == 1 ? Alignment.centerLeft : Alignment.centerRight;
            alignCenterRight = textDirectionFactor == 1 ? Alignment.centerRight : Alignment.centerLeft;

            _refreshEstimatedColumnWidths();
            
        }
        public void  _refreshEstimatedColumnWidths(){
             estimatedColumnWidths[(int) _PickerColumnType.dayOfMonth] = CupertinoDatePicker._getColumnWidth(
                _PickerColumnType.dayOfMonth, localizations, context);

            estimatedColumnWidths[(int) _PickerColumnType.month] = CupertinoDatePicker._getColumnWidth(
                _PickerColumnType.month, localizations, context);

            estimatedColumnWidths[(int) _PickerColumnType.year] = CupertinoDatePicker._getColumnWidth(
                _PickerColumnType.year, localizations, context);

        }

        DateTime _lastDayInMonth(int year, int month) => new DateTime(year, month + 1, 0);

        Widget _buildDayPicker(float offAxisFraction, TransitionBuilder itemPositioningBuilder) {
            int daysInCurrentMonth = _lastDayInMonth(selectedYear, selectedMonth).day;//DateTime.DaysInMonth(selectedYear, selectedMonth);
            return new NotificationListener<ScrollNotification>(
                onNotification: (ScrollNotification notification)=> {
                if (notification is ScrollStartNotification) {
                    isDayPickerScrolling = true;
                } else if (notification is ScrollEndNotification) {
                    isDayPickerScrolling = false;
                    _pickerDidStopScrolling();
                }

                return false;
            },
            child:
                new CupertinoPicker(
                scrollController: dayController,
                offAxisFraction: offAxisFraction,
                itemExtent: CupertinoDatePickerUtils._kItemExtent,
                useMagnifier: CupertinoDatePickerUtils._kUseMagnifier,
                magnification: CupertinoDatePickerUtils._kMagnification,
                backgroundColor:  widget.backgroundColor,
                squeeze: CupertinoDatePickerUtils._kSqueeze,
                onSelectedItemChanged: (int index) => {
                    selectedDay = index + 1;
                     selectedDay = index + 1;
                    if (_isCurrentDateValid)
                        widget.onDateTimeChanged(new DateTime(selectedYear, selectedMonth, selectedDay));
                },
                children: CupertinoDatePickerUtils.listGenerate(31, (int index) => {
                    int day = index + 1;
                    return itemPositioningBuilder(
                        context,
                        new Text(
                        localizations.datePickerDayOfMonth(day),
                        style: _themeTextStyle(context, isValid: day <= daysInCurrentMonth)
                        )
                    );
                }),
                looping: true
            );
        }

        Widget _buildMonthPicker(float offAxisFraction, TransitionBuilder itemPositioningBuilder) {
            return new NotificationListener<ScrollNotification>(
                onNotification: (ScrollNotification notification) => {
                if (notification is ScrollStartNotification) {
                    isMonthPickerScrolling = true;
                } else if (notification is ScrollEndNotification) {
                    isMonthPickerScrolling = false;
                    _pickerDidStopScrolling();
                }

                return false;
            },
            child:
                new CupertinoPicker(
                scrollController: monthController,//new FixedExtentScrollController(initialItem: selectedMonth - 1),
                offAxisFraction: offAxisFraction,
                itemExtent: CupertinoDatePickerUtils._kItemExtent,
                useMagnifier: CupertinoDatePickerUtils._kUseMagnifier,
                magnification: CupertinoDatePickerUtils._kMagnification,
                backgroundColor:  widget.backgroundColor,
                squeeze: CupertinoDatePickerUtils._kSqueeze,
                onSelectedItemChanged: (int index) => {
                   selectedMonth = index + 1;
                    if (_isCurrentDateValid)
                        widget.onDateTimeChanged(new DateTime(selectedYear, selectedMonth, selectedDay));
                },
                children: CupertinoDatePickerUtils.listGenerate(12, (int index) => {
                    int month = index + 1;
                    bool isInvalidMonth = (widget.minimumDate?.year == selectedYear && widget.minimumDate.month > month)
                                   || (widget.maximumDate?.year == selectedYear && widget.maximumDate.month < month);

                    return itemPositioningBuilder(
                        context,
                        new Text(
                        localizations.datePickerMonth(month),
                        style: _themeTextStyle(context, isValid: !isInvalidMonth)
                        )
                    );
                }),
                looping: true
            );
        }

        Widget _buildYearPicker(float offAxisFraction, TransitionBuilder itemPositioningBuilder) {
            return new NotificationListener<ScrollNotification>(
                onNotification: (ScrollNotification notification)=> {
                if (notification is ScrollStartNotification) {
                    isYearPickerScrolling = true;
                } else if (notification is ScrollEndNotification) {
                    isYearPickerScrolling = false;
                    _pickerDidStopScrolling();
                }

                return false;
            },
            child:
                CupertinoPicker.builder(
                scrollController: yearController,//new FixedExtentScrollController(initialItem: selectedYear),
                itemExtent: CupertinoDatePickerUtils._kItemExtent,
                offAxisFraction: offAxisFraction,
                useMagnifier: CupertinoDatePickerUtils._kUseMagnifier,
                magnification: CupertinoDatePickerUtils._kMagnification,
                backgroundColor: widget.backgroundColor,/// tbc????
                onSelectedItemChanged: (int index) => {
                    selectedYear = index;
                     if (_isCurrentDateValid)
                        widget.onDateTimeChanged(new DateTime(selectedYear, selectedMonth, selectedDay));
                   
                },
                itemBuilder: (BuildContext context, int index) => {
                    if (index < widget.minimumYear) {
                        return null;
                    }

                    if (widget.maximumYear != null && index > widget.maximumYear) {
                        return null;
                    }
                    bool isValidYear = (widget.minimumDate == null || widget.minimumDate.year <= year)
                                && (widget.maximumDate == null || widget.maximumDate.year >= year);

                    return itemPositioningBuilder(
                        context,
                        new Text(
                        localizations.datePickerYear(year),
                        style: _themeTextStyle(context, isValid: isValidYear)
                        )
                    );

                }
            );
        }

        bool  _isCurrentDateValid {
            get{
                    DateTime minSelectedDate = new DateTime(selectedYear, selectedMonth, selectedDay);
                    DateTime maxSelectedDate = new DateTime(selectedYear, selectedMonth, selectedDay + 1);
                    bool minCheck = widget.minimumDate?.isBefore(maxSelectedDate) ?? true;
                    bool maxCheck = widget.maximumDate?.isBefore(minSelectedDate) ?? false;
                    return minCheck && !maxCheck && minSelectedDate.day == selectedDay;
            }
            
           
        }

        void _pickerDidStopScrolling() {
    
            setState(()=>{ });

            if (isScrolling) {
                return;
            }
            DateTime minSelectDate = new DateTime(selectedYear, selectedMonth, selectedDay);
            DateTime maxSelectDate = new DateTime(selectedYear, selectedMonth, selectedDay + 1);

              bool minCheck = widget.minimumDate?.isBefore(maxSelectDate) ?? true;
              bool maxCheck = widget.maximumDate?.isBefore(minSelectDate) ?? false;

            if (!minCheck || maxCheck) {
                DateTime targetDate = minCheck ? widget.maximumDate : widget.minimumDate;
                _scrollToDate(targetDate);
                return;
            }
            if (minSelectDate.day != selectedDay) {
                DateTime lastDay = _lastDayInMonth(selectedYear, selectedMonth);
                _scrollToDate(lastDay);
            }
        }

        void _scrollToDate(DateTime newDate) {
            D.assert(newDate != null);
            SchedulerBinding.instance.addPostFrameCallback((Duration timestamp) {
            if (selectedYear != newDate.year) {
                _animateColumnControllerToItem(yearController, newDate.year);
            }

            if (selectedMonth != newDate.month) {
                _animateColumnControllerToItem(monthController, newDate.month - 1);
            }

            if (selectedDay != newDate.day) {
                _animateColumnControllerToItem(dayController, newDate.day - 1);
            }
            });
        }

        public override Widget build(BuildContext context) {
            List<_ColumnBuilder> pickerBuilders = new List<_ColumnBuilder>();
            List<float> columnWidths = new List<float>();
            switch (localizations.datePickerDateOrder) {
                case DatePickerDateOrder.mdy:
                    pickerBuilders = new List<_ColumnBuilder>()
                        {_buildMonthPicker, _buildDayPicker, _buildYearPicker};
                    columnWidths = new List<float>() {
                        estimatedColumnWidths[(int) _PickerColumnType.month],
                        estimatedColumnWidths[(int) _PickerColumnType.dayOfMonth],
                        estimatedColumnWidths[(int) _PickerColumnType.year]
                    };
                    break;
                case DatePickerDateOrder.dmy:
                    pickerBuilders = new List<_ColumnBuilder>
                        {_buildDayPicker, _buildMonthPicker, _buildYearPicker};
                    columnWidths = new List<float> {
                        estimatedColumnWidths[(int) _PickerColumnType.dayOfMonth],
                        estimatedColumnWidths[(int) _PickerColumnType.month],
                        estimatedColumnWidths[(int) _PickerColumnType.year]
                    };
                    break;
                case DatePickerDateOrder.ymd:
                    pickerBuilders = new List<_ColumnBuilder>
                        {_buildYearPicker, _buildMonthPicker, _buildDayPicker};
                    columnWidths = new List<float>() {
                        estimatedColumnWidths[(int) _PickerColumnType.year],
                        estimatedColumnWidths[(int) _PickerColumnType.month],
                        estimatedColumnWidths[(int) _PickerColumnType.dayOfMonth]
                    };
                    break;
                case DatePickerDateOrder.ydm:
                    pickerBuilders = new List<_ColumnBuilder>
                        {_buildYearPicker, _buildDayPicker, _buildMonthPicker};
                    columnWidths = new List<float> {
                        estimatedColumnWidths[(int) _PickerColumnType.year],
                        estimatedColumnWidths[(int) _PickerColumnType.dayOfMonth],
                        estimatedColumnWidths[(int) _PickerColumnType.month]
                    };
                    break;
                default:
                    D.assert(false, () => "date order is not specified");
                    break;
            }

            List<Widget> pickers = new List<Widget>();
            for (int i = 0; i < columnWidths.Count; i++) {
                //var _i = i;
                float offAxisFraction = (i - 1) * 0.3f * textDirectionFactor;
                EdgeInsets padding = EdgeInsets.only(right: CupertinoDatePickerUtils._kDatePickerPadSize);
                if (textDirectionFactor == -1)
                    padding = EdgeInsets.only(left: CupertinoDatePickerUtils._kDatePickerPadSize);


                pickers.Add(new LayoutId(
                    id: i,
                    child: pickerBuilders[i](
                        offAxisFraction,
                        (BuildContext _context, Widget child) => {
                            return new Container(
                                alignment: i == columnWidths.Count - 1
                                    ? alignCenterLeft
                                    : alignCenterRight,
                                padding: i == 0 ? null : padding,
                                child: new Container(
                                    alignment: i == 0 ? alignCenterLeft : alignCenterRight,
                                    width: columnWidths[i] + CupertinoDatePickerUtils._kDatePickerPadSize,
                                    child: child
                                )
                            );
                        }
                    )
                ));
            }

            return new MediaQuery(
                data: new MediaQueryData(textScaleFactor: 1.0f),
                child: new NotificationListener<ScrollEndNotification>(
                    //onNotification: _keepInValidRange,
                    child: DefaultTextStyle.merge(
                        style: CupertinoDatePickerUtils._kDefaultPickerTextStyle,
                        child: new CustomMultiChildLayout(
                            layoutDelegate: new _DatePickerLayoutDelegate(
                                columnWidths: columnWidths,
                                 textDirectionFactor: textDirectionFactor
                            ),
                            children:
                            pickers
                        )
                    )
                )
            );
        }
    }

    public enum CupertinoTimerPickerMode {
        hm,
        ms,
        hms,
    }

    public class CupertinoTimerPicker : StatefulWidget {
        public CupertinoTimerPicker(
            Key key,
            CupertinoTimerPickerMode mode = CupertinoTimerPickerMode.hms,
            TimeSpan initialTimerDuration = new TimeSpan(),
            int minuteInterval = 1,
            int secondInterval = 1,
            AlignmentGeometry alignment = Alignment.center,
            Color backgroundColor = null,
            ValueChanged<TimeSpan> onTimerDurationChanged = null
        ):base(key : key) {
            D.assert(mode != null);
            D.assert(onTimerDurationChanged != null);
            D.assert(initialTimerDuration >= TimeSpan.Zero);
            D.assert(initialTimerDuration < new TimeSpan(1, 0, 0, 0));
            D.assert(minuteInterval > 0 && 60 % minuteInterval == 0);
            D.assert(secondInterval > 0 && 60 % secondInterval == 0);
            D.assert((int) initialTimerDuration.TotalMinutes % minuteInterval == 0);
            D.assert((int) initialTimerDuration.TotalSeconds % secondInterval == 0);
            D.assert(alignment != null);  

            this.onTimerDurationChanged = onTimerDurationChanged;
            this.mode = mode;
            this.initialTimerDuration = initialTimerDuration;
            this.minuteInterval = minuteInterval;
            this.secondInterval = secondInterval;
            this.alignment = alignment;
            this.backgroundColor = backgroundColor;
        }

        public readonly CupertinoTimerPickerMode mode;
        public readonly TimeSpan initialTimerDuration;
        public readonly int minuteInterval;
        public readonly int secondInterval;
        public readonly ValueChanged<TimeSpan> onTimerDurationChanged;
        public readonly AlignmentGeometry alignment;
        public readonly Color backgroundColor;

        public override State createState() {
            return new _CupertinoTimerPickerState();
        }
    }

    class _CupertinoTimerPickerState : State<CupertinoTimerPicker> {
        TextDirection textDirection;
        CupertinoLocalizations localizations;

        int  textDirectionFactor {
            get{
                switch (textDirection) {
                    case TextDirection.ltr:
                        return 1;
                    case TextDirection.rtl:
                        return -1;
                    }
                    return 1;
            }
        }
        //Alignment alignCenterLeft;
        //Alignment alignCenterRight;
        int selectedHour;
        int selectedMinute;
        int selectedSecond;

        int lastSelectedHour;
        int lastSelectedMinute;
        int lastSelectedSecond;

        TextPainter textPainter = new TextPainter();
        List<string> numbers = CupertinoDatePickerUtils.listGenerate(10, (int i) => 
            int minusValue = 9 -i;
            return minus + " "
        );///tbc ??? 
        float numberLabelWidth;
        float numberLabelHeight;
        float numberLabelBaseline;

        public override void initState() {
            base.initState();
            selectedMinute = (int) widget.initialTimerDuration.TotalMinutes % 60;
            if (widget.mode != CupertinoTimerPickerMode.ms) {
                selectedHour = (int) widget.initialTimerDuration.TotalHours;
            }

            if (widget.mode != CupertinoTimerPickerMode.hm) {
                selectedSecond = (int) widget.initialTimerDuration.TotalSeconds % 60;
            }
            PaintingBinding.instance.systemFonts.addListener(_handleSystemFontsChange);/// tbc ???
        }

        

        void _handleSystemFontsChange() {
            setState(()=> {
            // System fonts change might cause the text layout width to change.
            textPainter.markNeedsLayout();
            _measureLabelMetrics();
            });
        }

        
        public override void dispose() {
            PaintingBinding.instance.systemFonts.removeListener(_handleSystemFontsChange);
            base.dispose();
        }

        public override void didUpdateWidget(CupertinoTimerPicker oldWidget) {
            base.didUpdateWidget(oldWidget);

            D.assert(
            oldWidget.mode == widget.mode, ()=>"The CupertinoTimerPicker's mode cannot change once it's built" );
        }


        public override void didChangeDependencies() {
            base.didChangeDependencies();
            //localizations = CupertinoLocalizations.of(context);
            //alignCenterLeft = Alignment.centerLeft;
            //alignCenterRight = Alignment.centerRight;
            
            textDirection = Directionality.of(context);
            localizations = CupertinoLocalizations.of(context);

            _measureLabelMetrics();
        }
        
        void _measureLabelMetrics() {
            textPainter.textDirection = textDirection;
            TextStyle textStyle =  _textStyleFrom(context);

            float maxWidth = float.negativeInfinity;
            string widestNumber;
            foreach (string input in numbers) {
            textPainter.text = new TextSpan(
                text: input,
                style: textStyle
            );
            textPainter.layout();

            if (textPainter.maxIntrinsicWidth > maxWidth) {
                maxWidth = textPainter.maxIntrinsicWidth;
                widestNumber = input;
            }
            }

            textPainter.text = new TextSpan(
            text: '$widestNumber$widestNumber',
            style: textStyle
            );

            textPainter.layout();
            numberLabelWidth = textPainter.maxIntrinsicWidth;
            numberLabelHeight = textPainter.height;
            numberLabelBaseline = textPainter.computeDistanceToActualBaseline(TextBaseline.alphabetic);
        }
        Widget _buildLabel(string text, EdgeInsetsDirectional pickerPadding) {
            EdgeInsetsDirectional padding = EdgeInsetsDirectional.only(
            start: numberLabelWidth
                + CupertinoDatePickerUtils._kTimerPickerLabelPadSize
                + pickerPadding.start
            );

            return new IgnorePointer(
            child: new Container(
                alignment: AlignmentDirectional.centerStart.resolve(textDirection),
                padding: padding.resolve(textDirection),
                child: new SizedBox(
                height: numberLabelHeight,
                child: new Baseline(
                    baseline: numberLabelBaseline,
                    baselineType: TextBaseline.alphabetic,
                    child: new Text(
                    text,
                    style: new  TextStyle(
                        fontSize: CupertinoDatePickerUtils._kTimerPickerLabelFontSize,
                        fontWeight: FontWeight.w600
                    ),
                    maxLines: 1,
                    softWrap: false
                    )
                )
                )
            )
            );
        }

        Widget _buildPickerNumberLabel(string text, EdgeInsetsDirectional padding) {
            return new Container(
            width: _kTimerPickerColumnIntrinsicWidth + padding.horizontal,
            padding: padding.resolve(textDirection),
            alignment: AlignmentDirectional.centerStart.resolve(textDirection),
            child: new Container(
                width: numberLabelWidth,
                alignment: AlignmentDirectional.centerEnd.resolve(textDirection),
                child: new Text(text, softWrap: false, maxLines: 1, overflow: TextOverflow.visible)
            )
            );
        }        

        Widget _buildHourPicker() {
            return new CupertinoPicker(
                scrollController: new FixedExtentScrollController(initialItem: selectedHour),
                offAxisFraction: -0.5f * textDirectionFactor,
                itemExtent: CupertinoDatePickerUtils._kItemExtent,
               backgroundColor: widget.backgroundColor,
                squeeze:  CupertinoDatePickerUtils._kSqueeze,
                onSelectedItemChanged: (int index) => {
                    setState(() => {
                        selectedHour = index;
                        widget.onTimerDurationChanged(
                            new TimeSpan(
                                hours: selectedHour,
                                minutes: selectedMinute,
                                seconds: selectedSecond ?? 0));
                    });
                },
                children: CupertinoDatePickerUtils.listGenerate(24, (int index) => {
                    string semanticsLabel = textDirectionFactor == 1
                    ? localizations.timerPickerHour(index) + localizations.timerPickerHourLabel(index)
                    : localizations.timerPickerHourLabel(index) + localizations.timerPickerHour(index);

                    return new Semantics(
                        label: semanticsLabel,
                        excludeSemantics: true,
                        child: _buildPickerNumberLabel(localizations.timerPickerHour(index), additionalPadding)
                    );
                })
            );
        }

        Widget _buildHourColumn(EdgeInsetsDirectional additionalPadding) {
            return new Stack(
            children: new List<Widget>[
                new NotificationListener<ScrollEndNotification>(
                onNotification: (ScrollEndNotification notification) => {
                    setState(()=> { lastSelectedHour = selectedHour; });
                    return false;
                },
                child: _buildHourPicker(additionalPadding)
                ),
                _buildLabel(
                localizations.timerPickerHourLabel(lastSelectedHour ?? selectedHour),
                additionalPadding
                )
            ]
            );
        }///tbc

        Widget _buildMinutePicker() {
            float offAxisFraction;
            switch (widget.mode) {
                case CupertinoTimerPickerMode.hm:
                    offAxisFraction = 0.5f * textDirectionFactor;
                    break;
                case CupertinoTimerPickerMode.hms:
                    offAxisFraction = 0.0f;
                    break;
                case CupertinoTimerPickerMode.ms:
                    offAxisFraction = -0.5f * textDirectionFactor;
            }

            return new CupertinoPicker(
                scrollController: new FixedExtentScrollController(
                    initialItem: (int)(selectedMinute / widget.minuteInterval)
                ),
                offAxisFraction: offAxisFraction,
                itemExtent: CupertinoDatePickerUtils._kItemExtent,
                backgroundColor: widget.backgroundColor,
                squeeze:  CupertinoDatePickerUtils._kSqueeze,
                looping: true,
                onSelectedItemChanged: (int index) => {
                    setState(() => {
                        selectedMinute = index * widget.minuteInterval;
                        widget.onTimerDurationChanged(
                            new TimeSpan(
                                hours: selectedHour ?? 0,
                                minutes: selectedMinute,
                                seconds: selectedSecond ?? 0));
                    });
                },
                children: CupertinoDatePickerUtils.listGenerate((int)(60 / widget.minuteInterval), (int index) => {
                    int minute = index * widget.minuteInterval;
                    string semanticsLabel = textDirectionFactor == 1
                    ? localizations.timerPickerMinute(minute) + localizations.timerPickerMinuteLabel(minute)
                    : localizations.timerPickerMinuteLabel(minute) + localizations.timerPickerMinute(minute);

                    return Semantics(
                    label: semanticsLabel,
                    excludeSemantics: true,
                    child: _buildPickerNumberLabel(localizations.timerPickerMinute(minute), additionalPadding),
                    );
                   
                })
            );
        }

        Widget _buildMinuteColumn(EdgeInsetsDirectional additionalPadding) {
            return new Stack(
            children: new List<Widget>[
                new NotificationListener<ScrollEndNotification>(
                onNotification: (ScrollEndNotification notification) => {
                    setState(() => { lastSelectedMinute = selectedMinute; });
                    return false;
                },
                child: _buildMinutePicker(additionalPadding)
                ),
                _buildLabel(
                localizations.timerPickerMinuteLabel(lastSelectedMinute ?? selectedMinute),
                additionalPadding
                )
            ]
            );
        }
//////////////////////////////////////////////////////////////////////////////////////////
        //Widget _buildSecondPicker() {
        Widget _buildSecondPicker(EdgeInsetsDirectional additionalPadding) {
            float offAxisFraction = 0.5 * textDirectionFactor; //0.5f ;
            float secondPickerWidth = widget.mode == CupertinoTimerPickerMode.ms
                ? CupertinoDatePickerUtils._kPickerWidth / 10
                : CupertinoDatePickerUtils._kPickerWidth / 6;

            return new CupertinoPicker(
                scrollController: new FixedExtentScrollController(
                    initialItem: (int)(selectedSecond / widget.secondInterval)
                ),
                offAxisFraction: offAxisFraction,
                itemExtent:  CupertinoDatePickerUtils._kItemExtent,
                backgroundColor: widget.backgroundColor,
                squeeze:  CupertinoDatePickerUtils._kSqueeze,
                looping: true,
                onSelectedItemChanged: (int index) => {
                    setState(() => {
                    selectedSecond = index * widget.secondInterval;
                    widget.onTimerDurationChanged(
                        Duration(
                        hours: selectedHour ?? 0,
                        minutes: selectedMinute,
                        seconds: selectedSecond));
                    });
                },
                children: List<Widget>.generate((int)(60 / widget.secondInterval), (int index) => {
                    int second = index * widget.secondInterval;

                    string semanticsLabel = textDirectionFactor == 1
                    ? localizations.timerPickerSecond(second) + localizations.timerPickerSecondLabel(second)
                    : localizations.timerPickerSecondLabel(second) + localizations.timerPickerSecond(second);

                    return Semantics(
                    label: semanticsLabel,
                    excludeSemantics: true,
                    child: _buildPickerNumberLabel(localizations.timerPickerSecond(second), additionalPadding),
                    );
                }),
                );
                
        }

        Widget _buildSecondColumn() {
            float secondPickerWidth = widget.mode == CupertinoTimerPickerMode.ms
                ? CupertinoDatePickerUtils._kPickerWidth / 10
                : CupertinoDatePickerUtils._kPickerWidth / 6;
            Widget secondLabel = new IgnorePointer(
                child: new Container(
                    alignment: alignCenterLeft,
                    padding: EdgeInsets.only(left: secondPickerWidth),
                    child: new Container(
                        alignment: alignCenterLeft,
                        padding: EdgeInsets.symmetric(horizontal: 2.0f),
                        child: _buildLabel(localizations.timerPickerSecondLabel(selectedSecond))
                    )
                )
            );
            return new Stack(
                children: new List<Widget> {
                    _buildSecondPicker(),
                    secondLabel
                }
            );
        }

        public override Widget build(BuildContext context) {
            Widget picker;
            if (widget.mode == CupertinoTimerPickerMode.hm) {
                picker = new Row(
                    children: new List<Widget> {
                        new Expanded(child: _buildHourColumn()),
                        new Expanded(child: _buildMinuteColumn()),
                    }
                );
            }
            else if (widget.mode == CupertinoTimerPickerMode.ms) {
                picker = new Row(
                    children: new List<Widget> {
                        new Expanded(child: _buildMinuteColumn()),
                        new Expanded(child: _buildSecondColumn()),
                    }
                );
            }
            else {
                picker = new Row(
                    children: new List<Widget> {
                        new Expanded(child: _buildHourColumn()),
                        new Container(
                            width: CupertinoDatePickerUtils._kPickerWidth / 3,
                            child: _buildMinuteColumn()
                        ),
                        new Expanded(child: _buildSecondColumn()),
                    }
                );
            }

            return new MediaQuery(
                data: new MediaQueryData(
                    textScaleFactor: 1.0f
                ),
                child: picker
            );
        }
    }
}*/