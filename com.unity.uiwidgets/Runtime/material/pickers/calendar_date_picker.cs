using System;
using System.Collections.Generic;
using com.unity.uiwidgets.Runtime.rendering;
using Unity.UIWidgets.animation;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.rendering;
using Unity.UIWidgets.ui;
using Unity.UIWidgets.widgets;
using UnityEngine;
using Color = Unity.UIWidgets.ui.Color;
using TextStyle = Unity.UIWidgets.painting.TextStyle;

namespace Unity.UIWidgets.material {
    public partial class material_ {
        public static readonly TimeSpan _monthScrollDuration = new TimeSpan(0, 0, 0, 0, 200);

        public const float _dayPickerRowHeight = 42.0f;

        public const int _maxDayPickerRowCount = 6; // A 31 day month that starts on Saturday.

        public const float _maxDayPickerHeight = _dayPickerRowHeight * (_maxDayPickerRowCount + 1);
        public const float _monthPickerHorizontalPadding = 8.0f;

        public const int _yearPickerColumnCount = 3;
        public const float _yearPickerPadding = 16.0f;
        public const float _yearPickerRowHeight = 52.0f;
        public const float _yearPickerRowSpacing = 8.0f;

        public const float _subHeaderHeight = 52.0f;
        public const float _monthNavButtonsWidth = 108.0f;
    }

    public class CalendarDatePicker : StatefulWidget {
        public CalendarDatePicker(
            Key key = null,
            DateTime? initialDate = null,
            DateTime? firstDate = null,
            DateTime? lastDate = null,
            ValueChanged<DateTime> onDateChanged = null,
            ValueChanged<DateTime> onDisplayedMonthChanged = null,
            DatePickerMode initialCalendarMode = DatePickerMode.day,
            material_.SelectableDayPredicate selectableDayPredicate = null
        ) : base(key: key) {
            D.assert(initialDate != null);
            D.assert(firstDate != null);
            D.assert(lastDate != null);
            this.initialDate = utils.dateOnly(initialDate.Value);
            this.firstDate = utils.dateOnly(firstDate.Value);
            this.lastDate = utils.dateOnly(lastDate.Value);
            D.assert(onDateChanged != null);

            this.onDateChanged = onDateChanged;
            this.onDisplayedMonthChanged = onDisplayedMonthChanged;
            this.initialCalendarMode = initialCalendarMode;
            this.selectableDayPredicate = selectableDayPredicate;
            D.assert(
                !(this.lastDate < this.firstDate),
                () => $"lastDate {lastDate} must be on or after firstDate {firstDate}."
            );
            D.assert(
                !(this.initialDate < this.firstDate),
                () => $"initialDate {initialDate} must be on or after firstDate {firstDate}."
            );
            D.assert(
                !(this.initialDate > this.lastDate),
                () => $"initialDate {initialDate} must be on or before lastDate {lastDate}."
            );
            D.assert(
                selectableDayPredicate == null || selectableDayPredicate(this.initialDate),
                () => $"Provided initialDate {initialDate} must satisfy provided selectableDayPredicate."
            );
        }

        public readonly DateTime initialDate;

        public readonly DateTime firstDate;

        public readonly DateTime lastDate;

        public readonly ValueChanged<DateTime> onDateChanged;

        public readonly ValueChanged<DateTime> onDisplayedMonthChanged;

        public readonly DatePickerMode initialCalendarMode;

        public readonly material_.SelectableDayPredicate selectableDayPredicate;

        public override State createState() => new UIWidgets.material._CalendarDatePickerState();
    }

    internal class _CalendarDatePickerState : State<CalendarDatePicker> {
        bool _announcedInitialDate = false;
        DatePickerMode _mode;
        DateTime _currentDisplayedMonthDate;
        DateTime _selectedDate;
        public readonly GlobalKey _monthPickerKey = GlobalKey.key();
        public readonly GlobalKey _yearPickerKey = GlobalKey.key();
        MaterialLocalizations _localizations;
        TextDirection _textDirection;

        public override void initState() {
            base.initState();
            _mode = widget.initialCalendarMode;
            _currentDisplayedMonthDate = new DateTime(widget.initialDate.Year, widget.initialDate.Month, 1);
            _selectedDate = widget.initialDate;
        }

        public override void didChangeDependencies() {
            base.didChangeDependencies();
            _localizations = MaterialLocalizations.of(context);
            _textDirection = Directionality.of(context);
            if (!_announcedInitialDate) {
                _announcedInitialDate = true;
                // SemanticsService.announce(
                //   _localizations.formatFullDate(_selectedDate),
                //   _textDirection,
                // );
            }
        }

        void _vibrate() {
            // switch (Theme.of(context).platform) {
            //   case TargetPlatform.android:
            //   case TargetPlatform.fuchsia:
            //   case TargetPlatform.linux:
            //   case TargetPlatform.windows:
            //     // HapticFeedback.vibrate();
            //     break;
            //   case TargetPlatform.iOS:
            //   case TargetPlatform.macOS:
            //     break;
            // }
        }

        void _handleModeChanged(DatePickerMode mode) {
            _vibrate();
            setState(() => {
                _mode = mode;
                // if (_mode == DatePickerMode.day) {
                //   SemanticsService.announce(
                //     _localizations.formatMonthYear(_selectedDate),
                //     _textDirection,
                //   );
                // } else {
                //   SemanticsService.announce(
                //     _localizations.formatYear(_selectedDate),
                //     _textDirection,
                //   );
                // }
            });
        }

        void _handleMonthChanged(DateTime date) {
            setState(() => {
                if (_currentDisplayedMonthDate.Year != date.Year || _currentDisplayedMonthDate.Month != date.Month) {
                    _currentDisplayedMonthDate = new DateTime(date.Year, date.Month, 1);
                    widget.onDisplayedMonthChanged?.Invoke(_currentDisplayedMonthDate);
                }
            });
        }

        void _handleYearChanged(DateTime value) {
            _vibrate();

            if (value < widget.firstDate) {
                value = widget.firstDate;
            }
            else if (value > widget.lastDate) {
                value = widget.lastDate;
            }

            setState(() => {
                _mode = DatePickerMode.day;
                _handleMonthChanged(value);
            });
        }

        void _handleDayChanged(DateTime value) {
            _vibrate();
            setState(() => {
                _selectedDate = value;
                widget.onDateChanged?.Invoke(_selectedDate);
            });
        }

        Widget _buildPicker() {
            switch (_mode) {
                case DatePickerMode.day:
                    return new _MonthPicker(
                        key: _monthPickerKey,
                        initialMonth: _currentDisplayedMonthDate,
                        currentDate: DateTime.Now,
                        firstDate: widget.firstDate,
                        lastDate: widget.lastDate,
                        selectedDate: _selectedDate,
                        onChanged: _handleDayChanged,
                        onDisplayedMonthChanged: _handleMonthChanged,
                        selectableDayPredicate: widget.selectableDayPredicate
                    );
                case DatePickerMode.year:
                    return new Padding(
                        padding: EdgeInsets.only(top: material_._subHeaderHeight),
                        child: new _YearPicker(
                            key: _yearPickerKey,
                            currentDate: DateTime.Now,
                            firstDate: widget.firstDate,
                            lastDate: widget.lastDate,
                            initialDate: _currentDisplayedMonthDate,
                            selectedDate: _selectedDate,
                            onChanged: _handleYearChanged
                        )
                    );
            }

            return null;
        }

        public override Widget build(BuildContext context) {
            return new Stack(
                children: new List<Widget> {
                    new SingleChildScrollView(
                        child: new SizedBox(
                            height: material_._maxDayPickerHeight,
                            child: _buildPicker()
                        )
                    ),
                    // Put the mode toggle button on top so that it won"t be covered up by the _MonthPicker
                    new _DatePickerModeToggleButton(
                        mode: _mode,
                        title: _localizations.formatMonthYear(_currentDisplayedMonthDate),
                        onTitlePressed: () => {
                            // Toggle the day/year mode.
                            _handleModeChanged(_mode == DatePickerMode.day ? DatePickerMode.year : DatePickerMode.day);
                        }
                    )
                }
            );
        }
    }

    class _DatePickerModeToggleButton : StatefulWidget {
        internal _DatePickerModeToggleButton(
            DatePickerMode mode,
            string title,
            VoidCallback onTitlePressed
        ) {
            this.mode = mode;
            this.title = title;
            this.onTitlePressed = onTitlePressed;
        }

        public readonly DatePickerMode mode;

        public readonly string title;

        public readonly VoidCallback onTitlePressed;

        public override State createState() => new _DatePickerModeToggleButtonState();
    }

    class _DatePickerModeToggleButtonState : SingleTickerProviderStateMixin<_DatePickerModeToggleButton> {
        AnimationController _controller;

        public override void initState() {
            base.initState();
            _controller = new AnimationController(
                value: widget.mode == DatePickerMode.year ? 0.5f : 0,
                upperBound: 0.5f,
                duration: new TimeSpan(0, 0, 0, 0, 200),
                vsync: this
            );
        }

        public override void didUpdateWidget(StatefulWidget statefulWidget) {
            base.didUpdateWidget(statefulWidget);
            if (statefulWidget is _DatePickerModeToggleButton oldWidget) {
                if (oldWidget.mode == widget.mode) {
                    return;
                }

                if (widget.mode == DatePickerMode.year) {
                    _controller.forward();
                }
                else {
                    _controller.reverse();
                }
            }
        }

        public override Widget build(BuildContext context) {
            ColorScheme colorScheme = Theme.of(context).colorScheme;
            TextTheme textTheme = Theme.of(context).textTheme;
            Color controlColor = colorScheme.onSurface.withOpacity(0.60f);

            var rowChildren = new List<Widget> {
                new Flexible(
                    child: new Container(
                        height: material_._subHeaderHeight,
                        child: new InkWell(
                            onTap: () => widget.onTitlePressed(),
                            child: new Padding(
                                padding: EdgeInsets.symmetric(horizontal: 8),
                                child: new Row(
                                    children: new List<Widget> {
                                        new Flexible(
                                            child: new Text(
                                                widget.title,
                                                overflow: TextOverflow.ellipsis,
                                                style: textTheme.subtitle2?.copyWith(
                                                    color: controlColor
                                                )
                                            )
                                        ),
                                        new RotationTransition(
                                            turns: _controller,
                                            child: new Icon(
                                                Icons.arrow_drop_down,
                                                color: controlColor
                                            )
                                        ),
                                    }
                                )
                            )
                        )
                    )
                )
            };
            if (widget.mode == DatePickerMode.day) {
                // Give space for the prev/next month buttons that are underneath this row
                rowChildren.Add(new SizedBox(width: material_._monthNavButtonsWidth));
            }

            return new Container(
                padding: EdgeInsetsDirectional.only(start: 16, end: 4),
                height: material_._subHeaderHeight,
                child: new Row(
                    children: rowChildren
                )
            );
        }

        public override void dispose() {
            _controller.dispose();
            base.dispose();
        }
    }

    class _MonthPicker : StatefulWidget {
        internal _MonthPicker(
            Key key = null,
            DateTime? initialMonth = null,
            DateTime? currentDate = null,
            DateTime? firstDate = null,
            DateTime? lastDate = null,
            DateTime? selectedDate = null,
            ValueChanged<DateTime> onChanged = null,
            ValueChanged<DateTime> onDisplayedMonthChanged = null,
            material_.SelectableDayPredicate selectableDayPredicate = null
        ) : base(key: key) {
            D.assert(selectedDate != null);
            D.assert(currentDate != null);
            D.assert(onChanged != null);
            D.assert(firstDate != null);
            D.assert(lastDate != null);
            D.assert(!(firstDate > lastDate));
            D.assert(!(selectedDate < firstDate));
            D.assert(!(selectedDate > lastDate));
            this.initialMonth = initialMonth.Value;
            this.currentDate = currentDate.Value;
            this.firstDate = firstDate.Value;
            this.lastDate = lastDate.Value;
            this.selectedDate = selectedDate.Value;
            this.onChanged = onChanged;
            this.onDisplayedMonthChanged = onDisplayedMonthChanged;
            this.selectableDayPredicate = selectableDayPredicate;
        }

        public readonly DateTime initialMonth;

        public readonly DateTime currentDate;

        public readonly DateTime firstDate;

        public readonly DateTime lastDate;

        public readonly DateTime selectedDate;

        public readonly ValueChanged<DateTime> onChanged;

        public readonly ValueChanged<DateTime> onDisplayedMonthChanged;

        public readonly material_.SelectableDayPredicate selectableDayPredicate;

        public override State createState() => new _MonthPickerState();
    }

    class _MonthPickerState : State<_MonthPicker> {
        DateTime _currentMonth;
        DateTime _nextMonthDate;
        DateTime _previousMonthDate;
        PageController _pageController;
        MaterialLocalizations _localizations;
        TextDirection _textDirection;

        public override void initState() {
            base.initState();
            _currentMonth = widget.initialMonth;
            _previousMonthDate = utils.addMonthsToMonthDate(_currentMonth, -1);
            _nextMonthDate = utils.addMonthsToMonthDate(_currentMonth, 1);
            _pageController = new PageController(initialPage: utils.monthDelta(widget.firstDate, _currentMonth));
        }

        public override void didChangeDependencies() {
            base.didChangeDependencies();
            _localizations = MaterialLocalizations.of(context);
            _textDirection = Directionality.of(context);
        }

        public override void dispose() {
            _pageController?.dispose();
            base.dispose();
        }

        void _handleMonthPageChanged(int monthPage) {
            DateTime monthDate = utils.addMonthsToMonthDate(widget.firstDate, monthPage);
            if (_currentMonth.Year != monthDate.Year || _currentMonth.Month != monthDate.Month) {
                _currentMonth = new DateTime(monthDate.Year, monthDate.Month, 1);
                _previousMonthDate = utils.addMonthsToMonthDate(_currentMonth, -1);
                _nextMonthDate = utils.addMonthsToMonthDate(_currentMonth, 1);
                widget.onDisplayedMonthChanged?.Invoke(_currentMonth);
            }
        }

        void _handleNextMonth() {
            if (!_isDisplayingLastMonth) {
                // SemanticsService.announce(
                //     _localizations.formatMonthYear(_nextMonthDate),
                //     _textDirection,
                // );
                _pageController.nextPage(
                    duration: material_._monthScrollDuration,
                    curve: Curves.ease
                );
            }
        }

        void _handlePreviousMonth() {
            if (!_isDisplayingFirstMonth) {
                // SemanticsService.announce(
                //     _localizations.formatMonthYear(_previousMonthDate),
                //     _textDirection,
                // );
                _pageController.previousPage(
                    duration: material_._monthScrollDuration,
                    curve: Curves.ease
                );
            }
        }

        bool _isDisplayingFirstMonth {
            get { return !(_currentMonth > new DateTime(widget.firstDate.Year, widget.firstDate.Month, 1)); }
        }

        bool _isDisplayingLastMonth {
            get {
                return !(_currentMonth < new DateTime(widget.lastDate.Year, widget.lastDate.Month, 1)
                    );
            }
        }

        Widget _buildItems(BuildContext context, int index) {
            DateTime month = utils.addMonthsToMonthDate(widget.firstDate, index);
            return new _DayPicker(
                key: new ValueKey<DateTime>(month),
                selectedDate: widget.selectedDate,
                currentDate: widget.currentDate,
                onChanged: widget.onChanged,
                firstDate: widget.firstDate,
                lastDate: widget.lastDate,
                displayedMonth: month,
                selectableDayPredicate: widget.selectableDayPredicate
            );
        }

        public override Widget build(BuildContext context) {
            string previousTooltipText =
                $"{_localizations.previousMonthTooltip} {_localizations.formatMonthYear(_previousMonthDate)}";
            string nextTooltipText =
                $"{_localizations.nextMonthTooltip} {_localizations.formatMonthYear(_nextMonthDate)}";
            Color controlColor = Theme.of(context).colorScheme.onSurface.withOpacity(0.60f);

            return new Column(
                children: new List<Widget> {
                    new Container(
                        padding: EdgeInsetsDirectional.only(start: 16, end: 4),
                        height: material_._subHeaderHeight,
                        child:
                        new Row(
                            children: new List<Widget> {
                                new Spacer(),
                                new IconButton(
                                    icon: new Icon(Icons.chevron_left),
                                    color: controlColor,
                                    tooltip: _isDisplayingFirstMonth ? null : previousTooltipText,
                                    onPressed: _isDisplayingFirstMonth ? (VoidCallback) null : _handlePreviousMonth
                                ),
                                new IconButton(icon: new Icon(Icons.chevron_right),
                                    color: controlColor,
                                    tooltip: _isDisplayingLastMonth ? null : nextTooltipText,
                                    onPressed: _isDisplayingLastMonth ? (VoidCallback) null : _handleNextMonth
                                )
                            }
                        )
                    ),
                    new _DayHeaders(),
                    new Expanded(
                        child: PageView.builder(
                            controller: _pageController,
                            itemBuilder: _buildItems,
                            itemCount: utils.monthDelta(widget.firstDate, widget.lastDate) + 1,
                            scrollDirection: Axis.horizontal,
                            onPageChanged: _handleMonthPageChanged
                        )
                    )
                }
            );
        }
    }

    class _DayPicker : StatelessWidget {
        internal _DayPicker(
            Key key = null,
            DateTime? currentDate = null,
            DateTime? displayedMonth = null,
            DateTime? firstDate = null,
            DateTime? lastDate = null,
            DateTime? selectedDate = null,
            ValueChanged<DateTime> onChanged = null,
            material_.SelectableDayPredicate selectableDayPredicate = null
        ) : base(key: key) {
            D.assert(currentDate != null);
            D.assert(displayedMonth != null);
            D.assert(firstDate != null);
            D.assert(lastDate != null);
            D.assert(selectedDate != null);
            D.assert(onChanged != null);
            D.assert(!(firstDate > lastDate));
            D.assert(!(selectedDate < firstDate));
            D.assert(!(selectedDate > lastDate));
            this.currentDate = currentDate.Value;
            this.displayedMonth = displayedMonth.Value;
            this.firstDate = firstDate.Value;
            this.lastDate = lastDate.Value;
            this.selectedDate = selectedDate.Value;
            this.onChanged = onChanged;
            this.selectableDayPredicate = selectableDayPredicate;
        }

        public readonly DateTime selectedDate;

        public readonly DateTime currentDate;

        public readonly ValueChanged<DateTime> onChanged;

        public readonly DateTime firstDate;

        public readonly DateTime lastDate;

        public readonly DateTime displayedMonth;

        public readonly material_.SelectableDayPredicate selectableDayPredicate;

        public override Widget build(BuildContext context) {
            ColorScheme colorScheme = Theme.of(context).colorScheme;
            MaterialLocalizations localizations = MaterialLocalizations.of(context);
            TextTheme textTheme = Theme.of(context).textTheme;
            TextStyle dayStyle = textTheme.caption;
            Color enabledDayColor = colorScheme.onSurface.withOpacity(0.87f);
            Color disabledDayColor = colorScheme.onSurface.withOpacity(0.38f);
            Color selectedDayColor = colorScheme.onPrimary;
            Color selectedDayBackground = colorScheme.primary;
            Color todayColor = colorScheme.primary;

            int year = displayedMonth.Year;
            int month = displayedMonth.Month;

            int daysInMonth = utils.getDaysInMonth(year, month);
            int dayOffset = utils.firstDayOffset(year, month, localizations);

            List<Widget> dayItems = new List<Widget>();
            // 1-based day of month, e.g. 1-31 for January, and 1-29 for February on
            // a leap year.
            int day = -dayOffset;
            while (day < daysInMonth) {
                day++;
                if (day < 1) {
                    dayItems.Add(new Container());
                }
                else {
                    DateTime dayToBuild = new DateTime(year, month, day);
                    bool isDisabled = dayToBuild > lastDate ||
                                      dayToBuild < firstDate ||
                                      (selectableDayPredicate != null && !selectableDayPredicate(dayToBuild));

                    BoxDecoration decoration = null;
                    Color dayColor = enabledDayColor;
                    bool isSelectedDay = utils.isSameDay(selectedDate, dayToBuild);
                    if (isSelectedDay) {
                        // The selected day gets a circle background highlight, and a
                        // contrasting text color.
                        dayColor = selectedDayColor;
                        decoration = new BoxDecoration(
                            color: selectedDayBackground,
                            shape: BoxShape.circle
                        );
                    }
                    else if (isDisabled) {
                        dayColor = disabledDayColor;
                    }
                    else if (utils.isSameDay(currentDate, dayToBuild)) {
                        // The current day gets a different text color and a circle stroke
                        // border.
                        dayColor = todayColor;
                        decoration = new BoxDecoration(
                            border: Border.all(color: todayColor, width: 1),
                            shape: BoxShape.circle
                        );
                    }

                    Widget dayWidget = new Container(
                        decoration: decoration,
                        child: new Center(
                            child: new Text(localizations.formatDecimal(day), style: dayStyle.apply(color: dayColor))
                        )
                    );
                    
                    if (!isDisabled) {
                        dayWidget = new GestureDetector(
                            behavior: HitTestBehavior.opaque,
                            onTap: () => onChanged(dayToBuild),
                            child: dayWidget
                        );
                    }

                    dayItems.Add(dayWidget);
                }
            }

            return new Padding(
                padding: EdgeInsets.symmetric(
                    horizontal: material_._monthPickerHorizontalPadding
                ),
                child: GridView.custom(
                    physics: new ClampingScrollPhysics(),
                    gridDelegate: material_._dayPickerGridDelegate,
                    childrenDelegate: new SliverChildListDelegate(
                        dayItems,
                        addRepaintBoundaries: false
                    )
                )
            );
        }
    }

    class _DayPickerGridDelegate : SliverGridDelegate {
        internal _DayPickerGridDelegate() {
        }

        const int daysPerWeek = 7;

        public override SliverGridLayout getLayout(SliverConstraints constraints) {
            const int columnCount = daysPerWeek;
            float tileWidth = constraints.crossAxisExtent / columnCount;
            float tileHeight = Mathf.Min(material_._dayPickerRowHeight,
                constraints.viewportMainAxisExtent / material_._maxDayPickerRowCount);
            return new SliverGridRegularTileLayout(
                childCrossAxisExtent: tileWidth,
                childMainAxisExtent: tileHeight,
                crossAxisCount: columnCount,
                crossAxisStride: tileWidth,
                mainAxisStride: tileHeight,
                reverseCrossAxis: AxisUtils.axisDirectionIsReversed(constraints.crossAxisDirection)
            );
        }

        public override bool shouldRelayout(SliverGridDelegate sliverGridDelegate) => false;
    }

    public partial class material_ {
        internal static readonly _DayPickerGridDelegate _dayPickerGridDelegate = new _DayPickerGridDelegate();
    }

    class _DayHeaders : StatelessWidget {
        List<Widget> _getDayHeaders(TextStyle headerStyle, MaterialLocalizations localizations) {
            List<Widget> result = new List<Widget>();
            for (int i = localizations.firstDayOfWeekIndex; true; i = (i + 1) % 7) {
                string weekday = localizations.narrowWeekdays[i];
                result.Add(new Center(child: new Text(weekday, style: headerStyle)));
                if (i == (localizations.firstDayOfWeekIndex - 1 + 7) % 7)
                    break;
            }

            return result;
        }

        public override Widget build(BuildContext context) {
            ThemeData theme = Theme.of(context);
            ColorScheme colorScheme = theme.colorScheme;
            TextStyle dayHeaderStyle = theme.textTheme.caption?.apply(
                color: colorScheme.onSurface.withOpacity(0.60f)
            );
            MaterialLocalizations localizations = MaterialLocalizations.of(context);
            List<Widget>
                labels = _getDayHeaders(dayHeaderStyle, localizations);

            return new Padding(
                padding: EdgeInsets.symmetric(horizontal: material_._monthPickerHorizontalPadding),
                child:
                GridView.custom(
                    shrinkWrap: true,
                    gridDelegate: material_._dayPickerGridDelegate,
                    childrenDelegate: new SliverChildListDelegate(
                        labels,
                        addRepaintBoundaries: false
                    )
                )
            );
        }
    }

    class _YearPicker : StatefulWidget {
        internal _YearPicker(
            Key key = null,
            DateTime? currentDate = null,
            DateTime? firstDate = null,
            DateTime? lastDate = null,
            DateTime? initialDate = null,
            DateTime? selectedDate = null,
            ValueChanged<DateTime> onChanged = null
        ) : base(key: key) {
            D.assert(currentDate != null);
            D.assert(firstDate != null);
            D.assert(lastDate != null);
            D.assert(initialDate != null);
            D.assert(selectedDate != null);
            D.assert(onChanged != null);
            D.assert(!(firstDate > lastDate));
            this.currentDate = currentDate.Value;
            this.firstDate = firstDate.Value;
            this.lastDate = lastDate.Value;
            this.initialDate = initialDate.Value;
            this.selectedDate = selectedDate.Value;
            this.onChanged = onChanged;
        }

        public readonly DateTime currentDate;

        public readonly DateTime firstDate;

        public readonly DateTime lastDate;

        public readonly DateTime initialDate;

        public readonly DateTime selectedDate;

        public readonly ValueChanged<DateTime> onChanged;

        public override
            State createState() => new _YearPickerState();
    }

    class _YearPickerState : State<_YearPicker> {
        ScrollController scrollController;

        // The approximate number of years necessary to fill the available space.
        public const int minYears = 18;

        public override void initState() {
            base.initState();

            // Set the scroll position to approximately center the initial year.
            int initialYearIndex = widget.selectedDate.Year - widget.firstDate.Year;
            int initialYearRow = (int) (1.0f * initialYearIndex / material_._yearPickerColumnCount);
            // Move the offset down by 2 rows to approximately center it.
            int centeredYearRow = initialYearRow - 2;
            float scrollOffset = _itemCount < minYears ? 0 : centeredYearRow * material_._yearPickerRowHeight;
            scrollController = new ScrollController(initialScrollOffset: scrollOffset);
        }

        Widget _buildYearItem(BuildContext context, int index) {
            ColorScheme colorScheme = Theme.of(context).colorScheme;
            TextTheme textTheme = Theme.of(context).textTheme;

            // Backfill the _YearPicker with disabled years if necessary.
            int offset = _itemCount < minYears ? (int) (1.0f * (minYears - _itemCount) / 2) : 0;
            int year = widget.firstDate.Year + index - offset;
            bool isSelected = year == widget.selectedDate.Year;
            bool isCurrentYear = year == widget.currentDate.Year;
            bool isDisabled = year < widget.firstDate.Year || year > widget.lastDate.Year;
            const float decorationHeight = 36.0f;
            const float decorationWidth = 72.0f;

            Color textColor;
            if (isSelected) {
                textColor = colorScheme.onPrimary;
            }
            else if (isDisabled) {
                textColor = colorScheme.onSurface.withOpacity(0.38f);
            }
            else if (isCurrentYear) {
                textColor = colorScheme.primary;
            }
            else {
                textColor = colorScheme.onSurface.withOpacity(0.87f);
            }

            TextStyle itemStyle = textTheme.bodyText1?.apply(color: textColor);

            BoxDecoration decoration = null;
            if (isSelected) {
                decoration = new BoxDecoration(
                    color: colorScheme.primary,
                    borderRadius: BorderRadius.circular(decorationHeight / 2),
                    shape: BoxShape.rectangle
                );
            }
            else if (isCurrentYear && !isDisabled) {
                decoration = new BoxDecoration(
                    border: Border.all(
                        color: colorScheme.primary,
                        width: 1
                    ),
                    borderRadius: BorderRadius.circular(decorationHeight / 2),
                    shape: BoxShape.rectangle
                );
            }

            Widget yearItem = new Center(
                child: new Container(
                    decoration: decoration,
                    height: decorationHeight,
                    width: decorationWidth,
                    child: new Center(
                        child: new Text(year.ToString(), style: itemStyle)
                    )
                )
            );
            
            if (!isDisabled) {
                yearItem = new InkWell(
                    key: new ValueKey<int>(year),
                    onTap: () => {
                        widget.onChanged(
                            new DateTime(
                                year,
                                widget.initialDate.Month,
                                widget.initialDate.Day
                            )
                        );
                    },
                    child: yearItem
                );
            }

            return yearItem;
        }

        int _itemCount {
            get { return widget.lastDate.Year - widget.firstDate.Year + 1; }
        }

        public override Widget build(BuildContext context) {
            return new Column(
                children: new List<Widget> {
                    new Divider(),
                    new Expanded(
                        child: GridView.builder(
                            controller: scrollController,
                            gridDelegate: material_._yearPickerGridDelegate,
                            itemBuilder: _buildYearItem,
                            itemCount: Math.Max(_itemCount, minYears),
                            padding: EdgeInsets.symmetric(horizontal: material_._yearPickerPadding)
                        )
                    ),
                    new Divider(),
                }
            );
        }
    }

    class _YearPickerGridDelegate : SliverGridDelegate {
        internal _YearPickerGridDelegate() {
        }

        public override SliverGridLayout getLayout(SliverConstraints constraints) {
            float tileWidth =
                (constraints.crossAxisExtent -
                 (material_._yearPickerColumnCount - 1) * material_._yearPickerRowSpacing) /
                material_._yearPickerColumnCount;
            return new SliverGridRegularTileLayout(
                childCrossAxisExtent: tileWidth,
                childMainAxisExtent: material_._yearPickerRowHeight,
                crossAxisCount: material_._yearPickerColumnCount,
                crossAxisStride: tileWidth + material_._yearPickerRowSpacing,
                mainAxisStride: material_._yearPickerRowHeight,
                reverseCrossAxis: AxisUtils.axisDirectionIsReversed(constraints.crossAxisDirection)
            );
        }

        public override bool shouldRelayout(SliverGridDelegate sliverGridDelegate) => false;
    }

    public partial class material_ {
        internal static readonly _YearPickerGridDelegate _yearPickerGridDelegate = new _YearPickerGridDelegate();
    }
}