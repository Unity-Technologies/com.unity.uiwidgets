using System;
using System.Collections.Generic;
using Unity.UIWidgets.animation;
using Unity.UIWidgets.external;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.rendering;
using Unity.UIWidgets.scheduler;
using Unity.UIWidgets.ui;
using Unity.UIWidgets.widgets;
using UnityEngine;
using Color = Unity.UIWidgets.ui.Color;
using TextStyle = Unity.UIWidgets.painting.TextStyle;

namespace Unity.UIWidgets.cupertino {
    static class CupertinoDatePickerUtils {
        public delegate Widget listGenerateDelegate(int index);

        public const float _kItemExtent = 32.0f;
        public const float _kPickerWidth = 320.0f;
        public const float _kPickerHeight = 216.0f;
        public const bool _kUseMagnifier = true;
        public const float _kMagnification = 1.05f;
        public const float _kDatePickerPadSize = 5f;
        public const float _kSqueeze = 1.25f;
        public const float _kTimerPickerHalfColumnPadding = 2f;
        public const float _kTimerPickerLabelPadSize = 4.5f;
        public const float _kTimerPickerLabelFontSize = 17.0f;
        public const float _kTimerPickerColumnIntrinsicWidth = 106f;
        public const float _kTimerPickerNumberLabelFontSize = 23f;

        public static TextStyle _kDefaultPickerTextStyle = new TextStyle(
            letterSpacing: -0.83f
        );

        public static TextStyle _themeTextStyle(BuildContext context, bool isValid = true) {
            var style = CupertinoTheme.of(context: context).textTheme.dateTimePickerTextStyle;
            return isValid
                ? style
                : style.copyWith(
                    color: CupertinoDynamicColor.resolve(resolvable: CupertinoColors.inactiveGray, context: context)
                );
        }

        public static void _animateColumnControllerToItem(FixedExtentScrollController controller, int targetItem) {
            controller.animateToItem(
                itemIndex: targetItem,
                curve: Curves.easeInOut,
                duration: TimeSpan.FromMilliseconds(200)
            );
        }

        public static List<string> CreateNumbers() {
            //List<String>.generate(10, (int i) => '${9 - i}');
            var numbers = new List<string>();
            for (var i = 0; i < 10; i++) {
                numbers.Add($"{9 - i}");
            }

            return numbers;
        }

        public static List<Widget> listGenerate(int count, listGenerateDelegate func) {
            var list = new List<Widget>();
            for (var i = 0; i < count; i++) {
                list.Add(func(index: i));
            }

            return list;
        }
    }

    public class _DatePickerLayoutDelegate : MultiChildLayoutDelegate {
        public readonly List<float> columnWidths;
        public readonly int textDirectionFactor;

        public _DatePickerLayoutDelegate(
            List<float> columnWidths,
            int textDirectionFactor
        ) {
            this.columnWidths = columnWidths;
            this.textDirectionFactor = textDirectionFactor;
        }

        public override void performLayout(Size size) {
            var remainingWidth = size.width;
            for (var i = 0; i < columnWidths.Count; i++) {
                remainingWidth -= columnWidths[index: i] + CupertinoDatePickerUtils._kDatePickerPadSize * 2;
            }

            var currentHorizontalOffset = 0.0f;
            for (var i = 0; i < columnWidths.Count; i++) {
                var index = textDirectionFactor == 1 ? i : columnWidths.Count - i - 1;

                var childWidth = columnWidths[index: index] + CupertinoDatePickerUtils._kDatePickerPadSize * 2;
                if (index == 0 || index == columnWidths.Count - 1) {
                    childWidth += remainingWidth / 2;
                }

                D.assert(() => {
                    if (childWidth < 0) {
                        throw new UIWidgetsError("Insufficient horizontal space to render the " +
                                                 "CupertinoDatePicker because the parent is too narrow at " +
                                                 $"{size.width}px.\n" +
                                                 $"An additional {-remainingWidth}px is needed to avoid " +
                                                 "overlapping columns.");
                    }

                    return true;
                });
                layoutChild(childId: index,
                    BoxConstraints.tight(new Size(Mathf.Max(0.0f, b: childWidth), height: size.height)));
                positionChild(childId: index, new Offset(dx: currentHorizontalOffset, 0.0f));

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
        dateAndTime
    }

    enum _PickerColumnType {
        dayOfMonth,
        month,
        year,
        date,
        hour,
        minute,
        dayPeriod
    }

    public class CupertinoDatePicker : StatefulWidget {
        public readonly Color backgroundColor;
        public readonly DateTime initialDateTime;
        public readonly DateTime? maximumDate;
        public readonly int? maximumYear;
        public readonly DateTime? minimumDate;
        public readonly int minimumYear;
        public readonly int minuteInterval;

        public readonly CupertinoDatePickerMode mode;
        public readonly ValueChanged<DateTime> onDateTimeChanged;
        public readonly bool use24hFormat;

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
                mode != CupertinoDatePickerMode.date || minimumYear >= 1 && this.initialDateTime.Year >= minimumYear,
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
            this.backgroundColor = backgroundColor;
        }

        public override State createState() {
            switch (mode) {
                case CupertinoDatePickerMode.time:
                case CupertinoDatePickerMode.dateAndTime:
                    return new _CupertinoDatePickerDateTimeState();
                case CupertinoDatePickerMode.date:
                    return new _CupertinoDatePickerDateState();
            }

            D.assert(false);
            return new _CupertinoDatePickerDateTimeState();
        }

        internal static float _getColumnWidth(
            _PickerColumnType columnType,
            CupertinoLocalizations localizations,
            BuildContext context
        ) {
            var longestText = "";
            switch (columnType) {
                case _PickerColumnType.date:

                    for (var i = 1; i <= 12; i++) {
                        var date =
                            localizations.datePickerMediumDate(new DateTime(2018, month: i, 25));
                        if (longestText.Length < date.Length) {
                            longestText = date;
                        }
                    }

                    break;
                case _PickerColumnType.hour:
                    for (var i = 0; i < 24; i++) {
                        var hour = localizations.datePickerHour(hour: i);
                        if (longestText.Length < hour.Length) {
                            longestText = hour;
                        }
                    }

                    break;
                case _PickerColumnType.minute:
                    for (var i = 0; i < 60; i++) {
                        var minute = localizations.datePickerMinute(minute: i);
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
                    for (var i = 1; i <= 31; i++) {
                        var dayOfMonth = localizations.datePickerDayOfMonth(dayIndex: i);
                        if (longestText.Length < dayOfMonth.Length) {
                            longestText = dayOfMonth;
                        }
                    }

                    break;
                case _PickerColumnType.month:
                    for (var i = 1; i <= 12; i++) {
                        var month = localizations.datePickerMonth(monthIndex: i);
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
            var painter = new TextPainter(
                new TextSpan(
                    style: CupertinoDatePickerUtils._themeTextStyle(context: context),
                    text: longestText
                )
            );


            painter.layout();
            return painter.maxIntrinsicWidth;
        }
    }

    delegate Widget _ColumnBuilder(float offAxisFraction, TransitionBuilder itemPositioningBuilder);

    class _CupertinoDatePickerDateTimeState : State<CupertinoDatePicker> {
        public static float _kMaximumOffAxisFraction = 0.45f;

        public static Dictionary<int, float> estimatedColumnWidths = new Dictionary<int, float>();

        public Alignment alignCenterLeft;
        public Alignment alignCenterRight;


        public FixedExtentScrollController amPmController;

        // The controller of the date column.
        FixedExtentScrollController dateController;
        FixedExtentScrollController hourController;

        public DateTime initialDateTime;

        bool isDatePickerScrolling;
        bool isHourPickerScrolling;
        bool isMeridiemPickerScrolling;
        bool isMinutePickerScrolling;
        public CupertinoLocalizations localizations;
        FixedExtentScrollController meridiemController;
        int meridiemRegion;
        FixedExtentScrollController minuteController;


        public int previousHourIndex;

        int selectedAmPm;

        int textDirectionFactor;

        int selectedDayFromInitial {
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
                    false, () =>
                        $"{GetType()} is only meant for dateAndTime mode or time mode"
                );
                return 0;
            }
            set { }
        }

        int selectedHour {
            get { return _selectedHour(selectedAmPm: selectedAmPm, selectedHour: _selectedHourIndex); }
        }

        int _selectedHourIndex {
            get { return hourController.hasClients ? hourController.selectedItem % 24 : initialDateTime.Hour; }
        }


        int selectedMinute {
            get {
                return minuteController.hasClients
                    ? minuteController.selectedItem * widget.minuteInterval % 60
                    : initialDateTime.Minute;
            }
        }

        bool isHourRegionFlipped {
            get { return _isHourRegionFlipped(selectedAmPm: selectedAmPm); }
        }

        bool isScrolling {
            get {
                return isDatePickerScrolling
                       || isHourPickerScrolling
                       || isMinutePickerScrolling
                       || isMeridiemPickerScrolling;
            }
        }

        DateTime selectedDateTime {
            get {
                var hours = selectedHour;
                var minutes = selectedMinute;
                var initialDate = new DateTime(
                    year: initialDateTime.Year,
                    month: initialDateTime.Month,
                    day: initialDateTime.Day
                );
                return initialDate.AddMinutes(hours * 60 + minutes).AddDays(value: selectedDayFromInitial);
            }
        }

        int _selectedHour(int selectedAmPm, int selectedHour) {
            return _isHourRegionFlipped(selectedAmPm: selectedAmPm) ? (selectedHour + 12) % 24 : selectedHour;
        }

        bool _isHourRegionFlipped(int selectedAmPm) {
            return selectedAmPm != meridiemRegion;
        }

        public override void initState() {
            base.initState();
            initialDateTime = widget.initialDateTime;


            selectedAmPm = initialDateTime.Hour / 12;
            meridiemRegion = selectedAmPm;

            meridiemController = new FixedExtentScrollController(initialItem: selectedAmPm);
            hourController = new FixedExtentScrollController(initialItem: initialDateTime.Hour);
            minuteController = new FixedExtentScrollController(initialDateTime.Minute / widget.minuteInterval);
            dateController = new FixedExtentScrollController();

            PaintingBinding.instance.systemFonts.addListener(listener: _handleSystemFontsChange);
        }

        void _handleSystemFontsChange() {
            setState(() => { estimatedColumnWidths.Clear(); });
        }

        public override void dispose() {
            dateController.dispose();
            hourController.dispose();
            minuteController.dispose();
            meridiemController.dispose();
            PaintingBinding.instance.systemFonts.removeListener(listener: _handleSystemFontsChange);
            base.dispose();
        }

        public override void didUpdateWidget(StatefulWidget oldWidget) {
            oldWidget = (CupertinoDatePicker) oldWidget;
            base.didUpdateWidget(oldWidget: oldWidget);

            D.assert(
                ((CupertinoDatePicker) oldWidget).mode == widget.mode, () =>
                    $"The {GetType()}'s mode cannot change once it's built."
            );

            if (!widget.use24hFormat && ((CupertinoDatePicker) oldWidget).use24hFormat) {
                meridiemController.dispose();
                meridiemController = new FixedExtentScrollController(initialItem: selectedAmPm);
            }
        }

        public override void didChangeDependencies() {
            base.didChangeDependencies();

            textDirectionFactor = Directionality.of(context: context) == TextDirection.ltr ? 1 : -1;
            localizations = CupertinoLocalizations.of(context: context);

            alignCenterLeft = textDirectionFactor == 1 ? Alignment.centerLeft : Alignment.centerRight;
            alignCenterRight = textDirectionFactor == 1 ? Alignment.centerRight : Alignment.centerLeft;

            estimatedColumnWidths.Clear();
        }

        float _getEstimatedColumnWidth(_PickerColumnType columnType) {
            if (!estimatedColumnWidths.TryGetValue((int) columnType, out var _)) {
                estimatedColumnWidths[(int) columnType] =
                    CupertinoDatePicker._getColumnWidth(columnType: columnType, localizations: localizations,
                        context: context);
            }

            return estimatedColumnWidths[(int) columnType];
        }

        void _onSelectedItemChange(int index) {
            var selected = selectedDateTime;
            var isDateInvalid = widget.minimumDate?.CompareTo(value: selected) > 0
                                || widget.maximumDate?.CompareTo(value: selected) < 0;
            if (isDateInvalid) {
                return;
            }

            widget.onDateTimeChanged(value: selected);
        }

        Widget _buildMediumDatePicker(float offAxisFraction, TransitionBuilder itemPositioningBuilder) {
            return new NotificationListener<ScrollNotification>(
                onNotification: notification => {
                    if (notification is ScrollStartNotification) {
                        isDatePickerScrolling = true;
                    }
                    else if (notification is ScrollEndNotification) {
                        isDatePickerScrolling = false;
                        _pickerDidStopScrolling();
                    }

                    return false;
                },
                child: new CupertinoPicker(
                    scrollController: dateController,
                    offAxisFraction: offAxisFraction,
                    itemExtent: CupertinoDatePickerUtils._kItemExtent,
                    useMagnifier: CupertinoDatePickerUtils._kUseMagnifier,
                    magnification: CupertinoDatePickerUtils._kMagnification,
                    backgroundColor: widget.backgroundColor,
                    squeeze: CupertinoDatePickerUtils._kSqueeze,
                    onSelectedItemChanged: index => { _onSelectedItemChange(index: index); },
                    itemBuilder: (context, index) => {
                        var rangeStart = new DateTime(
                            year: initialDateTime.Year,
                            month: initialDateTime.Month,
                            day: initialDateTime.Day
                        );
                        rangeStart = rangeStart.AddDays(value: index);

                        var rangeEnd = rangeStart.AddDays(1);

                        var now = DateTime.Now;

                        if (widget.minimumDate?.CompareTo(value: rangeEnd) > 0) {
                            return null;
                        }

                        if (widget.maximumDate?.CompareTo(value: rangeStart) < 0) {
                            return null;
                        }

                        var dateText = rangeStart == new DateTime(year: now.Year, month: now.Month, day: now.Day)
                            ? localizations.todayLabel
                            : localizations.datePickerMediumDate(date: rangeStart);
                        return itemPositioningBuilder(
                            context: context,
                            new Text(data: dateText, style: CupertinoDatePickerUtils._themeTextStyle(context: context))
                        );
                    }
                )
            );
        }

        bool _isValidHour(int meridiemIndex, int hourIndex) {
            var rangeStart = new DateTime(
                    year: initialDateTime.Year,
                    month: initialDateTime.Month,
                    day: initialDateTime.Day
                ).AddHours(_selectedHour(selectedAmPm: meridiemIndex, selectedHour: hourIndex))
                .AddDays(value: selectedDayFromInitial);

            // The end value of the range is exclusive, i.e. [rangeStart, rangeEnd).
            var rangeEnd = rangeStart.Add(new TimeSpan(0, 1, 0, 0));

            var minimum = widget.minimumDate == null ? true : widget.minimumDate?.CompareTo(value: rangeEnd) < 0;
            var maxmum = widget.maximumDate == null ? false : widget.maximumDate?.CompareTo(value: rangeStart) < 0;
            return minimum && !maxmum;
        }

        Widget _buildHourPicker(float offAxisFraction, TransitionBuilder itemPositioningBuilder) {
            var widgets = new List<Widget>();
            for (var index = 0; index < 24; index++) {
                var hour = isHourRegionFlipped ? (index + 12) % 24 : index;
                var displayHour = widget.use24hFormat ? hour : (hour + 11) % 12 + 1;
                widgets.Add(itemPositioningBuilder(
                    context: context,
                    new Text(
                        localizations.datePickerHour(hour: displayHour),
                        //semanticsLabel: localizations.datePickerHourSemanticsLabel(displayHour),
                        style: CupertinoDatePickerUtils._themeTextStyle(context: context,
                            _isValidHour(meridiemIndex: selectedAmPm, hourIndex: index))
                    )
                ));
            }

            return new NotificationListener<ScrollNotification>(
                onNotification: notification => {
                    if (notification is ScrollStartNotification) {
                        isHourPickerScrolling = true;
                    }
                    else if (notification is ScrollEndNotification) {
                        isHourPickerScrolling = false;
                        _pickerDidStopScrolling();
                    }

                    return false;
                },
                child: new CupertinoPicker(
                    scrollController: hourController,
                    offAxisFraction: offAxisFraction,
                    itemExtent: CupertinoDatePickerUtils._kItemExtent,
                    useMagnifier: CupertinoDatePickerUtils._kUseMagnifier,
                    magnification: CupertinoDatePickerUtils._kMagnification,
                    backgroundColor: widget.backgroundColor,
                    squeeze: CupertinoDatePickerUtils._kSqueeze,
                    onSelectedItemChanged: index => {
                        var regionChanged = meridiemRegion != index / 12;
                        var debugIsFlipped = isHourRegionFlipped;

                        if (regionChanged) {
                            meridiemRegion = index / 12;
                            selectedAmPm = 1 - selectedAmPm;
                        }

                        if (!widget.use24hFormat && regionChanged) {
                            meridiemController.animateToItem(
                                itemIndex: selectedAmPm,
                                TimeSpan.FromMilliseconds(300),
                                curve: Curves.easeOut
                            );
                        }
                        else {
                            _onSelectedItemChange(index: index);
                        }

                        D.assert(debugIsFlipped == isHourRegionFlipped);
                    },
                    children: widgets,
                    looping: true
                )
            );
        }

        Widget _buildMinutePicker(float offAxisFraction, TransitionBuilder itemPositioningBuilder) {
            var widgets = new List<Widget>();
            for (var index = 0; index < 60; index++) {
                var minute = index * widget.minuteInterval;

                var date = new DateTime(
                    year: initialDateTime.Year,
                    month: initialDateTime.Month,
                    day: initialDateTime.Day
                ).AddMinutes(selectedHour * 60 + minute).AddDays(value: selectedDayFromInitial);

                var miniDate = widget.minimumDate == null ? false : widget.minimumDate?.CompareTo(value: date) > 0;
                var maxDate = widget.maximumDate == null ? false : widget.maximumDate?.CompareTo(value: date) < 0;
                var isInvalidMinute = miniDate || maxDate;

                widgets.Add(itemPositioningBuilder(
                    context: context,
                    new Text(
                        localizations.datePickerMinute(minute: minute),
                        //semanticsLabel: localizations.datePickerMinuteSemanticsLabel(minute),
                        style: CupertinoDatePickerUtils._themeTextStyle(context: context, isValid: !isInvalidMinute)
                    ))
                );
            }

            return new NotificationListener<ScrollNotification>(
                onNotification: notification => {
                    if (notification is ScrollStartNotification) {
                        isMinutePickerScrolling = true;
                    }
                    else if (notification is ScrollEndNotification) {
                        isMinutePickerScrolling = false;
                        _pickerDidStopScrolling();
                    }

                    return false;
                },
                child: new CupertinoPicker(
                    scrollController: minuteController,
                    offAxisFraction: offAxisFraction,
                    itemExtent: CupertinoDatePickerUtils._kItemExtent,
                    useMagnifier: CupertinoDatePickerUtils._kUseMagnifier,
                    magnification: CupertinoDatePickerUtils._kMagnification,
                    backgroundColor: widget.backgroundColor,
                    squeeze: CupertinoDatePickerUtils._kSqueeze,
                    onSelectedItemChanged: _onSelectedItemChange,
                    children: widgets,
                    looping: true)
            );
        }

        Widget _buildAmPmPicker(float offAxisFraction, TransitionBuilder itemPositioningBuilder) {
            var widgets = new List<Widget>();
            for (var index = 0; index < 2; index++) {
                widgets.Add(
                    itemPositioningBuilder(
                        context: context,
                        new Text(
                            index == 0
                                ? localizations.anteMeridiemAbbreviation
                                : localizations.postMeridiemAbbreviation,
                            style: CupertinoDatePickerUtils._themeTextStyle(context: context,
                                _isValidHour(meridiemIndex: index, hourIndex: _selectedHourIndex))
                        )));
            }

            return new NotificationListener<ScrollNotification>(
                onNotification: notification => {
                    if (notification is ScrollStartNotification) {
                        isMeridiemPickerScrolling = true;
                    }
                    else if (notification is ScrollEndNotification) {
                        isMeridiemPickerScrolling = false;
                        _pickerDidStopScrolling();
                    }

                    return false;
                },
                child: new CupertinoPicker(
                    scrollController: meridiemController,
                    offAxisFraction: offAxisFraction,
                    itemExtent: CupertinoDatePickerUtils._kItemExtent,
                    useMagnifier: CupertinoDatePickerUtils._kUseMagnifier,
                    magnification: CupertinoDatePickerUtils._kMagnification,
                    backgroundColor: widget.backgroundColor,
                    squeeze: CupertinoDatePickerUtils._kSqueeze,
                    onSelectedItemChanged: index => {
                        selectedAmPm = index;
                        D.assert(selectedAmPm == 0 || selectedAmPm == 1);
                        _onSelectedItemChange(index: index);
                    },
                    children: widgets
                )
            );
        }

        void _pickerDidStopScrolling() {
            setState(() => { });

            if (isScrolling) {
                return;
            }

            var selectedDate = selectedDateTime;

            var minCheck = widget.minimumDate == null ? false : widget.minimumDate?.CompareTo(value: selectedDate) > 0;
            var maxCheck = widget.maximumDate == null ? false : widget.maximumDate?.CompareTo(value: selectedDate) < 0;

            if (minCheck || maxCheck) {
                // We have minCheck === !maxCheck.
                var targetDate = (DateTime) (minCheck ? widget.minimumDate : widget.maximumDate);
                _scrollToDate(newDate: targetDate, fromDate: selectedDate);
            }
        }

        void _scrollToDate(DateTime newDate, DateTime fromDate) {
            D.assert(newDate != null);
            SchedulerBinding.instance.addPostFrameCallback(_ => {
                if (fromDate.Year != newDate.Year || fromDate.Month != newDate.Month || fromDate.Day != newDate.Day) {
                    CupertinoDatePickerUtils._animateColumnControllerToItem(controller: dateController,
                        targetItem: selectedDayFromInitial);
                }

                if (fromDate.Hour != newDate.Hour) {
                    var needsMeridiemChange = !widget.use24hFormat
                                              && fromDate.Hour / 12 != newDate.Hour / 12;
                    // In AM/PM mode, the pickers should not scroll all the way to the other hour region.
                    if (needsMeridiemChange) {
                        CupertinoDatePickerUtils._animateColumnControllerToItem(controller: meridiemController,
                            1 - meridiemController.selectedItem);

                        // Keep the target item index in the current 12-h region.
                        var newItem = hourController.selectedItem / 12 * 12
                                      + (hourController.selectedItem + newDate.Hour - fromDate.Hour) % 12;
                        CupertinoDatePickerUtils._animateColumnControllerToItem(controller: hourController,
                            targetItem: newItem);
                    }
                    else {
                        CupertinoDatePickerUtils._animateColumnControllerToItem(
                            controller: hourController,
                            hourController.selectedItem + newDate.Hour - fromDate.Hour
                        );
                    }
                }

                if (fromDate.Minute != newDate.Minute) {
                    CupertinoDatePickerUtils._animateColumnControllerToItem(controller: minuteController,
                        targetItem: newDate.Minute);
                }
            });
        }

        public override Widget build(BuildContext context) {
            var columnWidths = new List<float> {
                _getEstimatedColumnWidth(columnType: _PickerColumnType.hour),
                _getEstimatedColumnWidth(columnType: _PickerColumnType.minute)
            };
            var pickerBuilders = new List<_ColumnBuilder> {
                _buildHourPicker,
                _buildMinutePicker
            };
            if (!widget.use24hFormat) {
                if (localizations.datePickerDateTimeOrder == DatePickerDateTimeOrder.date_time_dayPeriod
                    || localizations.datePickerDateTimeOrder == DatePickerDateTimeOrder.time_dayPeriod_date) {
                    pickerBuilders.Add(item: _buildAmPmPicker);
                    columnWidths.Add(_getEstimatedColumnWidth(columnType: _PickerColumnType.dayPeriod));
                }
                else {
                    pickerBuilders.Insert(0, item: _buildAmPmPicker);
                    columnWidths.Insert(0, _getEstimatedColumnWidth(columnType: _PickerColumnType.dayPeriod));
                }
            }

            if (widget.mode == CupertinoDatePickerMode.dateAndTime) {
                if (localizations.datePickerDateTimeOrder == DatePickerDateTimeOrder.time_dayPeriod_date
                    || localizations.datePickerDateTimeOrder == DatePickerDateTimeOrder.dayPeriod_time_date) {
                    pickerBuilders.Add(item: _buildMediumDatePicker);
                    columnWidths.Add(_getEstimatedColumnWidth(columnType: _PickerColumnType.date));
                }
                else {
                    pickerBuilders.Insert(0, item: _buildMediumDatePicker);
                    columnWidths.Insert(0, _getEstimatedColumnWidth(columnType: _PickerColumnType.date));
                }
            }

            var pickers = new List<Widget>();
            for (var i = 0; i < columnWidths.Count; i++) {
                var offAxisFraction = 0.0f;
                if (i == 0) {
                    offAxisFraction = -_kMaximumOffAxisFraction * textDirectionFactor;
                }
                else if (i >= 2 || columnWidths.Count == 2) {
                    offAxisFraction = _kMaximumOffAxisFraction * textDirectionFactor;
                }

                var padding = EdgeInsets.only(right: CupertinoDatePickerUtils._kDatePickerPadSize);
                if (i == columnWidths.Count - 1) {
                    padding = padding.flipped;
                }

                if (textDirectionFactor == -1) {
                    padding = padding.flipped;
                }

                var width = columnWidths[index: i];
                pickers.Add(
                    new LayoutId(
                        id: i,
                        child: pickerBuilders[index: i](
                            offAxisFraction: offAxisFraction,
                            (context1, child) => {
                                return new Container(
                                    alignment: i == columnWidths.Count - 1
                                        ? alignCenterLeft
                                        : alignCenterRight,
                                    padding: padding,
                                    child: new Container(
                                        alignment: i == columnWidths.Count - 1 ? alignCenterLeft : alignCenterRight,
                                        width: i == 0 || i == columnWidths.Count - 1
                                            ? (float?) null
                                            : width + CupertinoDatePickerUtils._kDatePickerPadSize,
                                        child: child
                                    )
                                );
                            }
                        )
                    ));
            }

            return new MediaQuery(
                data: MediaQuery.of(context: context).copyWith(textScaleFactor: 1.0f),
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
        readonly Dictionary<int, float> estimatedColumnWidths = new Dictionary<int, float>();
        Alignment alignCenterLeft;
        Alignment alignCenterRight;

        FixedExtentScrollController dayController;

        bool isDayPickerScrolling;
        bool isMonthPickerScrolling;
        bool isYearPickerScrolling;
        CupertinoLocalizations localizations;
        FixedExtentScrollController monthController;

        int selectedDay;
        int selectedMonth;
        int selectedYear;
        int textDirectionFactor;
        FixedExtentScrollController yearController;

        bool isScrolling {
            get { return isDayPickerScrolling || isMonthPickerScrolling || isYearPickerScrolling; }
        }

        bool _isCurrentDateValid {
            // The current date selection represents a range [minSelectedData, maxSelectDate].
            get {
                var minSelectedDate = new DateTime(year: selectedYear, month: selectedMonth, 1);
                minSelectedDate = minSelectedDate.AddDays(selectedDay - 1);
                var maxSelectedDate = minSelectedDate.AddDays(1);
                var minCheck = widget.minimumDate == null
                    ? true
                    : widget.minimumDate?.CompareTo(value: maxSelectedDate) < 0;
                var maxCheck = widget.maximumDate == null
                    ? false
                    : widget.maximumDate?.CompareTo(value: minSelectedDate) < 0;
                return minCheck && !maxCheck && minSelectedDate.Day == selectedDay;
            }
        }

        public override void initState() {
            base.initState();
            selectedDay = widget.initialDateTime.Day;
            selectedMonth = widget.initialDateTime.Month;
            selectedYear = widget.initialDateTime.Year;

            dayController = new FixedExtentScrollController(selectedDay - 1);
            monthController = new FixedExtentScrollController(selectedMonth - 1);
            yearController = new FixedExtentScrollController(initialItem: selectedYear);

            PaintingBinding.instance.systemFonts.addListener(listener: _handleSystemFontsChange);
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

            PaintingBinding.instance.systemFonts.removeListener(listener: _handleSystemFontsChange);
            base.dispose();
        }


        public override void didChangeDependencies() {
            base.didChangeDependencies();

            textDirectionFactor = Directionality.of(context: context) == TextDirection.ltr ? 1 : -1;
            localizations = CupertinoLocalizations.of(context: context);

            alignCenterLeft = textDirectionFactor == 1 ? Alignment.centerLeft : Alignment.centerRight;
            alignCenterRight = textDirectionFactor == 1 ? Alignment.centerRight : Alignment.centerLeft;

            _refreshEstimatedColumnWidths();
        }

        void _refreshEstimatedColumnWidths() {
            estimatedColumnWidths[(int) _PickerColumnType.dayOfMonth] =
                CupertinoDatePicker._getColumnWidth(columnType: _PickerColumnType.dayOfMonth,
                    localizations: localizations, context: context);
            estimatedColumnWidths[(int) _PickerColumnType.month] =
                CupertinoDatePicker._getColumnWidth(columnType: _PickerColumnType.month, localizations: localizations,
                    context: context);
            estimatedColumnWidths[(int) _PickerColumnType.year] =
                CupertinoDatePicker._getColumnWidth(columnType: _PickerColumnType.year, localizations: localizations,
                    context: context);
        }

        DateTime _lastDayInMonth(int year, int month) {
            var date = new DateTime(year: year, month: month, 1);
            date = date.AddMonths(1);
            date = date.Subtract(new TimeSpan(1, 0, 0, 0));
            return date;
        }

        Widget _buildDayPicker(float offAxisFraction, TransitionBuilder itemPositioningBuilder) {
            var daysInCurrentMonth = _lastDayInMonth(year: selectedYear, month: selectedMonth).Day;
            var widgets = new List<Widget>();
            for (var index = 0; index < 31; index++) {
                var day = index + 1;
                widgets.Add(itemPositioningBuilder(
                    context: context,
                    new Text(
                        localizations.datePickerDayOfMonth(dayIndex: day),
                        style: CupertinoDatePickerUtils._themeTextStyle(context: context, day <= daysInCurrentMonth)
                    )
                ));
            }

            return new NotificationListener<ScrollNotification>(
                onNotification: notification => {
                    if (notification is ScrollStartNotification) {
                        isDayPickerScrolling = true;
                    }
                    else if (notification is ScrollEndNotification) {
                        isDayPickerScrolling = false;
                        _pickerDidStopScrolling();
                    }

                    return false;
                },
                child: new CupertinoPicker(
                    scrollController: dayController,
                    offAxisFraction: offAxisFraction,
                    itemExtent: CupertinoDatePickerUtils._kItemExtent,
                    useMagnifier: CupertinoDatePickerUtils._kUseMagnifier,
                    magnification: CupertinoDatePickerUtils._kMagnification,
                    backgroundColor: widget.backgroundColor,
                    squeeze: CupertinoDatePickerUtils._kSqueeze,
                    onSelectedItemChanged: index => {
                        selectedDay = index + 1;
                        if (_isCurrentDateValid) {
                            var date = new DateTime(year: selectedYear, month: selectedMonth, 1);
                            date.AddDays(selectedDay - 1);
                            widget.onDateTimeChanged(value: date);
                        }
                    },
                    children: widgets,
                    looping: true
                )
            );
        }

        Widget _buildMonthPicker(float offAxisFraction, TransitionBuilder itemPositioningBuilder) {
            var widgets = new List<Widget>();
            for (var index = 0; index < 12; index++) {
                var month = index + 1;
                var isInvalidMonth = widget.minimumDate?.Year == selectedYear && widget.minimumDate?.Month > month
                                     || widget.maximumDate?.Year == selectedYear && widget.maximumDate?.Month < month;

                widgets.Add(itemPositioningBuilder(
                    context: context,
                    new Text(
                        localizations.datePickerMonth(monthIndex: month),
                        style: CupertinoDatePickerUtils._themeTextStyle(context: context, isValid: !isInvalidMonth)
                    )
                ));
            }

            return new NotificationListener<ScrollNotification>(
                onNotification: notification => {
                    if (notification is ScrollStartNotification) {
                        isMonthPickerScrolling = true;
                    }
                    else if (notification is ScrollEndNotification) {
                        isMonthPickerScrolling = false;
                        _pickerDidStopScrolling();
                    }

                    return false;
                },
                child: new CupertinoPicker(
                    scrollController: monthController,
                    offAxisFraction: offAxisFraction,
                    itemExtent: CupertinoDatePickerUtils._kItemExtent,
                    useMagnifier: CupertinoDatePickerUtils._kUseMagnifier,
                    magnification: CupertinoDatePickerUtils._kMagnification,
                    backgroundColor: widget.backgroundColor,
                    squeeze: CupertinoDatePickerUtils._kSqueeze,
                    onSelectedItemChanged: index => {
                        selectedMonth = index + 1;
                        if (_isCurrentDateValid) {
                            var date = new DateTime(year: selectedYear, month: selectedMonth, 1);
                            date.AddDays(selectedDay - 1);
                            widget.onDateTimeChanged(value: date);
                        }
                    },
                    children: widgets,
                    looping: true
                )
            );
        }

        Widget _buildYearPicker(float offAxisFraction, TransitionBuilder itemPositioningBuilder) {
            return new NotificationListener<ScrollNotification>(
                onNotification: notification => {
                    if (notification is ScrollStartNotification) {
                        isYearPickerScrolling = true;
                    }
                    else if (notification is ScrollEndNotification) {
                        isYearPickerScrolling = false;
                        _pickerDidStopScrolling();
                    }

                    return false;
                },
                child: new CupertinoPicker(
                    scrollController: yearController,
                    itemExtent: CupertinoDatePickerUtils._kItemExtent,
                    offAxisFraction: offAxisFraction,
                    useMagnifier: CupertinoDatePickerUtils._kUseMagnifier,
                    magnification: CupertinoDatePickerUtils._kMagnification,
                    backgroundColor: widget.backgroundColor,
                    onSelectedItemChanged: index => {
                        selectedYear = index;
                        if (_isCurrentDateValid) {
                            var date = new DateTime(year: selectedYear, month: selectedMonth, 1);
                            date.AddDays(selectedDay - 1);
                            widget.onDateTimeChanged(value: date);
                        }
                    },
                    itemBuilder: (_context, year) => {
                        if (year < widget.minimumYear) {
                            return null;
                        }

                        if (widget.maximumYear != null && year > widget.maximumYear) {
                            return null;
                        }

                        var isValidYear = (widget.minimumDate == null || widget.minimumDate?.Year <= year)
                                          && (widget.maximumDate == null || widget.maximumDate?.Year >= year);

                        return itemPositioningBuilder(
                            context: context,
                            new Text(
                                localizations.datePickerYear(yearIndex: year),
                                style: CupertinoDatePickerUtils._themeTextStyle(context: _context,
                                    isValid: isValidYear))
                        );
                    }
                ));
        }

        void _pickerDidStopScrolling() {
            setState(() => { });

            if (isScrolling) {
                return;
            }

            var minSelectDate = new DateTime(year: selectedYear, month: selectedMonth, 1);
            minSelectDate = minSelectDate.AddDays(selectedDay - 1);
            var maxSelectDate = minSelectDate.AddDays(1);

            var minCheck = widget.minimumDate == null ? false : widget.minimumDate?.CompareTo(value: maxSelectDate) > 0;
            var maxCheck = widget.maximumDate == null ? false : widget.maximumDate?.CompareTo(value: minSelectDate) < 0;

            if (minCheck || maxCheck) {
                var targetDate = minCheck ? (DateTime) widget.maximumDate : (DateTime) widget.minimumDate;
                _scrollToDate(newDate: targetDate);
                return;
            }


            if (minSelectDate.Day != selectedDay) {
                var lastDay = _lastDayInMonth(year: selectedYear, month: selectedMonth);
                _scrollToDate(newDate: lastDay);
            }
        }

        void _scrollToDate(DateTime newDate) {
            D.assert(newDate != null);
            SchedulerBinding.instance.addPostFrameCallback(timespan => {
                if (selectedYear != newDate.Year) {
                    CupertinoDatePickerUtils._animateColumnControllerToItem(controller: yearController,
                        targetItem: newDate.Year);
                }

                if (selectedMonth != newDate.Month) {
                    CupertinoDatePickerUtils._animateColumnControllerToItem(controller: monthController,
                        newDate.Month - 1);
                }

                if (selectedDay != newDate.Day) {
                    CupertinoDatePickerUtils._animateColumnControllerToItem(controller: dayController, newDate.Day - 1);
                }
            });
        }

        public override Widget build(BuildContext context) {
            var pickerBuilders = new List<_ColumnBuilder>();
            var columnWidths = new List<float>();
            switch (localizations.datePickerDateOrder) {
                case DatePickerDateOrder.mdy:
                    pickerBuilders = new List<_ColumnBuilder> {_buildMonthPicker, _buildDayPicker, _buildYearPicker};
                    columnWidths = new List<float> {
                        estimatedColumnWidths[(int) _PickerColumnType.month],
                        estimatedColumnWidths[(int) _PickerColumnType.dayOfMonth],
                        estimatedColumnWidths[(int) _PickerColumnType.year]
                    };
                    break;
                case DatePickerDateOrder.dmy:
                    pickerBuilders = new List<_ColumnBuilder> {_buildDayPicker, _buildMonthPicker, _buildYearPicker};
                    columnWidths = new List<float> {
                        estimatedColumnWidths[(int) _PickerColumnType.dayOfMonth],
                        estimatedColumnWidths[(int) _PickerColumnType.month],
                        estimatedColumnWidths[(int) _PickerColumnType.year]
                    };
                    break;
                case DatePickerDateOrder.ymd:
                    pickerBuilders = new List<_ColumnBuilder> {_buildYearPicker, _buildMonthPicker, _buildDayPicker};
                    columnWidths = new List<float> {
                        estimatedColumnWidths[(int) _PickerColumnType.year],
                        estimatedColumnWidths[(int) _PickerColumnType.month],
                        estimatedColumnWidths[(int) _PickerColumnType.dayOfMonth]
                    };
                    break;
                case DatePickerDateOrder.ydm:
                    pickerBuilders = new List<_ColumnBuilder> {_buildYearPicker, _buildDayPicker, _buildMonthPicker};
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

            var pickers = new List<Widget>();
            for (var i = 0; i < columnWidths.Count; i++) {
                var index = i;
                var offAxisFraction = (index - 1) * 0.3f * textDirectionFactor;
                var padding = EdgeInsets.only(right: CupertinoDatePickerUtils._kDatePickerPadSize);
                if (textDirectionFactor == -1) {
                    padding = EdgeInsets.only(left: CupertinoDatePickerUtils._kDatePickerPadSize);
                }

                Widget transitionBuilder(BuildContext _context, Widget child) {
                    var columnWidth = columnWidths.Count == 0 ? 0 : columnWidths[index: index];
                    var result = new Container(
                        alignment: index == columnWidths.Count - 1
                            ? alignCenterLeft
                            : alignCenterRight,
                        padding: index == 0 ? null : padding,
                        child: new Container(
                            alignment: index == 0 ? alignCenterLeft : alignCenterRight,
                            width: columnWidth + CupertinoDatePickerUtils._kDatePickerPadSize,
                            child: child
                        )
                    );
                    return result;
                }

                TransitionBuilder builder = transitionBuilder;

                var childWidget = pickerBuilders[index: index](
                    offAxisFraction: offAxisFraction,
                    itemPositioningBuilder: builder
                );
                pickers.Add(new LayoutId(
                        id: index,
                        child: childWidget
                    )
                );
            }

            return new MediaQuery(
                data: MediaQuery.of(context: context).copyWith(textScaleFactor: 1.0f),
                child: DefaultTextStyle.merge(
                    style: CupertinoDatePickerUtils._kDefaultPickerTextStyle,
                    child: new CustomMultiChildLayout(
                        layoutDelegate: new _DatePickerLayoutDelegate(
                            columnWidths: columnWidths,
                            textDirectionFactor: textDirectionFactor
                        ),
                        children: pickers
                    ))
            );
        }
    }

    public enum CupertinoTimerPickerMode {
        hm,
        ms,
        hms
    }

    public class CupertinoTimerPicker : StatefulWidget {
        public readonly AlignmentGeometry alignment;
        public readonly Color backgroundColor;
        public readonly TimeSpan initialTimerDuration;
        public readonly int minuteInterval;

        public readonly CupertinoTimerPickerMode mode;
        public readonly ValueChanged<TimeSpan> onTimerDurationChanged;
        public readonly int secondInterval;

        public CupertinoTimerPicker(
            Key key = null,
            CupertinoTimerPickerMode mode = CupertinoTimerPickerMode.hms,
            TimeSpan? initialTimerDuration = null,
            int minuteInterval = 1,
            int secondInterval = 1,
            AlignmentGeometry alignment = null,
            Color backgroundColor = null,
            ValueChanged<TimeSpan> onTimerDurationChanged = null
        ) : base(key: key) {
            initialTimerDuration = initialTimerDuration ?? TimeSpan.Zero;
            alignment = alignment ?? Alignment.center;

            D.assert(onTimerDurationChanged != null);
            D.assert(initialTimerDuration >= TimeSpan.Zero);
            D.assert(initialTimerDuration < new TimeSpan(1, 0, 0, 0));
            D.assert(minuteInterval > 0 && 60 % minuteInterval == 0);
            D.assert(secondInterval > 0 && 60 % secondInterval == 0);
            D.assert(alignment != null);

            this.mode = mode;
            this.initialTimerDuration = initialTimerDuration ?? TimeSpan.Zero;
            this.minuteInterval = minuteInterval;
            this.secondInterval = secondInterval;
            this.alignment = alignment;
            this.backgroundColor = backgroundColor;
            this.onTimerDurationChanged = onTimerDurationChanged;
        }

        public override State createState() {
            return new _CupertinoTimerPickerState();
        }
    }

    class _CupertinoTimerPickerState : State<CupertinoTimerPicker> {
        public readonly List<string> numbers = CupertinoDatePickerUtils.CreateNumbers();

        public readonly TextPainter textPainter = new TextPainter();
        Alignment alignCenterLeft;
        Alignment alignCenterRight;

        int lastSelectedHour;
        int lastSelectedMinute;
        int lastSelectedSecond;
        CupertinoLocalizations localizations;
        float numberLabelBaseline;
        float numberLabelHeight;

        float numberLabelWidth;

        int selectedHour;
        int selectedMinute;
        int selectedSecond;
        TextDirection textDirection;

        int textDirectionFactor {
            get {
                switch (textDirection) {
                    case TextDirection.ltr:
                        return 1;
                    case TextDirection.rtl:
                        return -1;
                }

                return 1;
            }
        }

        public override void initState() {
            base.initState();
            selectedMinute = (int) widget.initialTimerDuration.TotalMinutes % 60;

            if (widget.mode != CupertinoTimerPickerMode.ms) {
                selectedHour = (int) widget.initialTimerDuration.TotalHours;
            }

            if (widget.mode != CupertinoTimerPickerMode.hm) {
                selectedSecond = (int) widget.initialTimerDuration.TotalSeconds % 60;
            }

            PaintingBinding.instance.systemFonts.addListener(listener: _handleSystemFontsChange);
        }

        void _handleSystemFontsChange() {
            setState(() => {
                textPainter.markNeedsLayout();
                _measureLabelMetrics();
            });
        }

        public override void dispose() {
            PaintingBinding.instance.systemFonts.removeListener(listener: _handleSystemFontsChange);
            base.dispose();
        }

        public override void didUpdateWidget(StatefulWidget oldWidget) {
            oldWidget = (CupertinoTimerPicker) oldWidget;
            base.didUpdateWidget(oldWidget: oldWidget);

            D.assert(
                ((CupertinoTimerPicker) oldWidget).mode == widget.mode, () =>
                    "The CupertinoTimerPicker's mode cannot change once it's built"
            );
        }

        public override void didChangeDependencies() {
            base.didChangeDependencies();

            textDirection = Directionality.of(context: context);
            localizations = CupertinoLocalizations.of(context: context);

            _measureLabelMetrics();
        }

        void _measureLabelMetrics() {
            textPainter.textDirection = textDirection;
            var textStyle = _textStyleFrom(context: context);

            var maxWidth = float.NegativeInfinity;
            var widestNumber = "";


            foreach (var input in numbers) {
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
                $"{widestNumber}{widestNumber}",
                style: textStyle
            );

            textPainter.layout();
            numberLabelWidth = textPainter.maxIntrinsicWidth;
            numberLabelHeight = textPainter.height;
            numberLabelBaseline = textPainter.computeDistanceToActualBaseline(baseline: TextBaseline.alphabetic);
        }

        Widget _buildLabel(string text, EdgeInsetsDirectional pickerPadding) {
            var padding = EdgeInsetsDirectional.only(
                numberLabelWidth
                + CupertinoDatePickerUtils._kTimerPickerLabelPadSize
                + pickerPadding.start
            );

            return new IgnorePointer(
                child: new Container(
                    alignment: AlignmentDirectional.centerStart.resolve(direction: textDirection),
                    padding: padding.resolve(direction: textDirection),
                    child: new SizedBox(
                        height: numberLabelHeight,
                        child: new Baseline(
                            baseline: numberLabelBaseline,
                            baselineType: TextBaseline.alphabetic,
                            child: new Text(
                                data: text,
                                style: new TextStyle(
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
                width: CupertinoDatePickerUtils._kTimerPickerColumnIntrinsicWidth + padding.horizontal,
                padding: padding.resolve(direction: textDirection),
                alignment: AlignmentDirectional.centerStart.resolve(direction: textDirection),
                child: new Container(
                    width: numberLabelWidth,
                    alignment: AlignmentDirectional.centerEnd.resolve(direction: textDirection),
                    child: new Text(data: text, softWrap: false, maxLines: 1, overflow: TextOverflow.visible)
                )
            );
        }

        Widget _buildHourPicker(EdgeInsetsDirectional additionalPadding) {
            return new CupertinoPicker(
                scrollController: new FixedExtentScrollController(initialItem: selectedHour),
                offAxisFraction: -0.5f * textDirectionFactor,
                itemExtent: CupertinoDatePickerUtils._kItemExtent,
                backgroundColor: widget.backgroundColor,
                squeeze: CupertinoDatePickerUtils._kSqueeze,
                onSelectedItemChanged: index => {
                    setState(() => {
                        selectedHour = index;
                        widget.onTimerDurationChanged(
                            new TimeSpan(
                                hours: selectedHour,
                                minutes: selectedMinute,
                                selectedSecond == 0 ? 0 : selectedHour));
                    });
                },
                children: CupertinoDatePickerUtils.listGenerate(24, index => {
                    var semanticsLabel = textDirectionFactor == 1
                        ? localizations.timerPickerHour(hour: index) + localizations.timerPickerHourLabel(hour: index)
                        : localizations.timerPickerHourLabel(hour: index) + localizations.timerPickerHour(hour: index);

                    return _buildPickerNumberLabel(localizations.timerPickerHour(hour: index),
                        padding: additionalPadding);
                })
            );
        }

        Widget _buildHourColumn(EdgeInsetsDirectional additionalPadding) {
            return new Stack(
                children: new List<Widget> {
                    new NotificationListener<ScrollEndNotification>(
                        onNotification: notification => {
                            setState(() => { lastSelectedHour = selectedHour; });
                            return false;
                        },
                        child: _buildHourPicker(additionalPadding: additionalPadding)
                    ),
                    _buildLabel(
                        localizations.timerPickerHourLabel(lastSelectedHour == 0 ? selectedHour : lastSelectedHour),
                        pickerPadding: additionalPadding
                    )
                }
            );
        }

        Widget _buildMinutePicker(EdgeInsetsDirectional additionalPadding) {
            var offAxisFraction = 0f;
            switch (widget.mode) {
                case CupertinoTimerPickerMode.hm:
                    offAxisFraction = 0.5f * textDirectionFactor;
                    break;
                case CupertinoTimerPickerMode.hms:
                    offAxisFraction = 0.0f;
                    break;
                case CupertinoTimerPickerMode.ms:
                    offAxisFraction = -0.5f * textDirectionFactor;
                    break;
            }

            return new CupertinoPicker(
                scrollController: new FixedExtentScrollController(
                    selectedMinute / widget.minuteInterval
                ),
                offAxisFraction: offAxisFraction,
                itemExtent: CupertinoDatePickerUtils._kItemExtent,
                backgroundColor: widget.backgroundColor,
                squeeze: CupertinoDatePickerUtils._kSqueeze,
                looping: true,
                onSelectedItemChanged: index => {
                    setState(() => {
                        selectedMinute = index * widget.minuteInterval;
                        widget.onTimerDurationChanged(
                            new TimeSpan(
                                selectedHour == 0 ? 0 : selectedHour,
                                minutes: selectedMinute,
                                selectedSecond == 0 ? 0 : selectedSecond));
                    });
                },
                children: CupertinoDatePickerUtils.listGenerate(60 / widget.minuteInterval, index => {
                    var minute = index * widget.minuteInterval;
                    var semanticsLabel = textDirectionFactor == 1
                        ? localizations.timerPickerMinute(minute: minute) +
                          localizations.timerPickerMinuteLabel(minute: minute)
                        : localizations.timerPickerMinuteLabel(minute: minute) +
                          localizations.timerPickerMinute(minute: minute);
                    return _buildPickerNumberLabel(localizations.timerPickerMinute(minute: minute),
                        padding: additionalPadding);
                })
            );
        }

        Widget _buildMinuteColumn(EdgeInsetsDirectional additionalPadding) {
            return new Stack(
                children: new List<Widget> {
                    new NotificationListener<ScrollEndNotification>(
                        onNotification: notification => {
                            setState(() => { lastSelectedMinute = selectedMinute; });
                            return false;
                        },
                        child: _buildMinutePicker(additionalPadding: additionalPadding)
                    ),
                    _buildLabel(
                        localizations.timerPickerMinuteLabel(lastSelectedMinute == 0
                            ? selectedMinute
                            : lastSelectedMinute),
                        pickerPadding: additionalPadding
                    )
                }
            );
        }

        Widget _buildSecondPicker(EdgeInsetsDirectional additionalPadding) {
            var offAxisFraction = 0.5f * textDirectionFactor;

            return new CupertinoPicker(
                scrollController: new FixedExtentScrollController(
                    selectedSecond / widget.secondInterval
                ),
                offAxisFraction: offAxisFraction,
                itemExtent: CupertinoDatePickerUtils._kItemExtent,
                backgroundColor: widget.backgroundColor,
                squeeze: CupertinoDatePickerUtils._kSqueeze,
                looping: true,
                onSelectedItemChanged: index => {
                    setState(() => {
                        selectedSecond = index * widget.secondInterval;
                        widget.onTimerDurationChanged(
                            new TimeSpan(
                                selectedHour == 0 ? 0 : selectedHour,
                                minutes: selectedMinute,
                                seconds: selectedSecond));
                    });
                },
                children: CupertinoDatePickerUtils.listGenerate(60 / widget.secondInterval, index => {
                    var second = index * widget.secondInterval;

                    var semanticsLabel = textDirectionFactor == 1
                        ? localizations.timerPickerSecond(second: second) +
                          localizations.timerPickerSecondLabel(second: second)
                        : localizations.timerPickerSecondLabel(second: second) +
                          localizations.timerPickerSecond(second: second);
                    return _buildPickerNumberLabel(localizations.timerPickerSecond(second: second),
                        padding: additionalPadding);
                })
            );
        }

        Widget _buildSecondColumn(EdgeInsetsDirectional additionalPadding) {
            return new Stack(
                children: new List<Widget> {
                    new NotificationListener<ScrollEndNotification>(
                        onNotification: notification => {
                            setState(() => { lastSelectedSecond = selectedSecond; });
                            return false;
                        },
                        child: _buildSecondPicker(additionalPadding: additionalPadding)
                    ),
                    _buildLabel(
                        localizations.timerPickerSecondLabel(lastSelectedSecond == 0
                            ? selectedSecond
                            : lastSelectedSecond),
                        pickerPadding: additionalPadding
                    )
                }
            );
        }

        TextStyle _textStyleFrom(BuildContext context) {
            return CupertinoTheme.of(context: context).textTheme
                .pickerTextStyle.merge(
                    new TextStyle(
                        fontSize: CupertinoDatePickerUtils._kTimerPickerNumberLabelFontSize
                    )
                );
        }

        public override Widget build(BuildContext context) {
            var columns = new List<Widget>();
            var paddingValue = CupertinoDatePickerUtils._kPickerWidth -
                               2 * CupertinoDatePickerUtils._kTimerPickerColumnIntrinsicWidth -
                               2 * CupertinoDatePickerUtils._kTimerPickerHalfColumnPadding;

            var totalWidth = CupertinoDatePickerUtils._kPickerWidth;
            D.assert(paddingValue >= 0);
            switch (widget.mode) {
                case CupertinoTimerPickerMode.hm:
                    // Pad the widget to make it as wide as `_kPickerWidth`.
                    columns = new List<Widget> {
                        _buildHourColumn(EdgeInsetsDirectional.only(paddingValue / 2,
                            end: CupertinoDatePickerUtils._kTimerPickerHalfColumnPadding)),
                        _buildMinuteColumn(EdgeInsetsDirectional.only(
                            start: CupertinoDatePickerUtils._kTimerPickerHalfColumnPadding, end: paddingValue / 2))
                    };
                    break;
                case CupertinoTimerPickerMode.ms:
                    // Pad the widget to make it as wide as `_kPickerWidth`.
                    columns = new List<Widget> {
                        _buildMinuteColumn(EdgeInsetsDirectional.only(paddingValue / 2,
                            end: CupertinoDatePickerUtils._kTimerPickerHalfColumnPadding)),
                        _buildSecondColumn(EdgeInsetsDirectional.only(
                            start: CupertinoDatePickerUtils._kTimerPickerHalfColumnPadding, end: paddingValue / 2))
                    };
                    break;
                case CupertinoTimerPickerMode.hms:
                    var _paddingValue = CupertinoDatePickerUtils._kTimerPickerHalfColumnPadding * 2;
                    totalWidth = CupertinoDatePickerUtils._kTimerPickerColumnIntrinsicWidth * 3 +
                                 4 * CupertinoDatePickerUtils._kTimerPickerHalfColumnPadding + _paddingValue;
                    columns = new List<Widget> {
                        _buildHourColumn(EdgeInsetsDirectional.only(_paddingValue / 2,
                            end: CupertinoDatePickerUtils._kTimerPickerHalfColumnPadding)),
                        _buildMinuteColumn(EdgeInsetsDirectional.only(
                            start: CupertinoDatePickerUtils._kTimerPickerHalfColumnPadding,
                            end: CupertinoDatePickerUtils._kTimerPickerHalfColumnPadding)),
                        _buildSecondColumn(EdgeInsetsDirectional.only(
                            start: CupertinoDatePickerUtils._kTimerPickerHalfColumnPadding, end: _paddingValue / 2))
                    };
                    break;
            }

            var themeData = CupertinoTheme.of(context: context);
            return new MediaQuery(
                // The native iOS picker's text scaling is fixed, so we will also fix it
                // as well in our picker.
                data: MediaQuery.of(context: context).copyWith(textScaleFactor: 1.0f),
                child: new CupertinoTheme(
                    data: themeData.copyWith(
                        textTheme: themeData.textTheme.copyWith(
                            pickerTextStyle: _textStyleFrom(context: context)
                        )
                    ),
                    child: new Align(
                        alignment: widget.alignment,
                        child: new Container(
                            color: CupertinoDynamicColor.resolve(resolvable: widget.backgroundColor, context: context),
                            width: totalWidth,
                            height: CupertinoDatePickerUtils._kPickerHeight,
                            child: new DefaultTextStyle(
                                style: _textStyleFrom(context: context),
                                child: new Row(
                                    children: LinqUtils<Widget>.SelectList(items: columns, child => {
                                        var result = new Expanded(child: child);
                                        return (Widget) result;
                                    })
                                )
                            )
                        )
                    )
                )
            );
        }
    }
}