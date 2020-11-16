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