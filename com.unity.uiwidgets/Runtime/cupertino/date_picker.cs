using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Unity.UIWidgets.animation;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.rendering;
using Unity.UIWidgets.scheduler2;
using Unity.UIWidgets.ui;
using Unity.UIWidgets.widgets;
using UnityEngine;
using Color = Unity.UIWidgets.ui.Color;
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
        public static TextStyle _kDefaultPickerTextStyle = new TextStyle(
            letterSpacing: -0.83f
        );
        public const float _kTimerPickerHalfColumnPadding = 2f;
        public const float _kTimerPickerLabelPadSize = 4.5f;
        public const float _kTimerPickerLabelFontSize = 17.0f;
        public const float _kTimerPickerColumnIntrinsicWidth = 106f;
        public const float _kTimerPickerNumberLabelFontSize = 23f;

        public static TextStyle _themeTextStyle(BuildContext context,  bool isValid = true ) {
            TextStyle style = CupertinoTheme.of(context).textTheme.dateTimePickerTextStyle;
            return isValid ? style : style.copyWith(color: CupertinoDynamicColor.resolve(CupertinoColors.inactiveGray, context));
        }

        public static void _animateColumnControllerToItem(FixedExtentScrollController controller, int targetItem) {
            controller.animateToItem(
                targetItem,
                curve: Curves.easeInOut,
                duration: TimeSpan.FromMilliseconds(200)
                );
        }

        public static List<string> CreateNumbers() {
            //List<String>.generate(10, (int i) => '${9 - i}');
            List<string> numbers = new List<string>();
            for (int i = 0; i < 10; i++) {
                numbers.Add($"{9-i}");
            }

            return numbers;
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
            this.columnWidths = columnWidths;
            this.textDirectionFactor = textDirectionFactor;
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
                if (index == 0 || index == columnWidths.Count - 1)
                    childWidth += remainingWidth / 2;

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
                layoutChild(index, BoxConstraints.tight(new Size(Mathf.Max(0.0f, childWidth), size.height)));
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
            CupertinoDatePickerMode mode = cupertino.CupertinoDatePickerMode.dateAndTime,
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
            this.minimumYear = minimumYear;
            this.maximumYear = maximumYear;
            this.minuteInterval = minuteInterval;
            this.use24hFormat = use24hFormat;
            this.backgroundColor = backgroundColor;
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
        public readonly Color backgroundColor;
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
                    style: CupertinoDatePickerUtils._themeTextStyle(context),
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
        
        int textDirectionFactor;
        public CupertinoLocalizations localizations;
        
        public Alignment alignCenterLeft;
        public Alignment alignCenterRight;
        
        public DateTime initialDateTime;
        
       
        public int previousHourIndex;
        
        
        public FixedExtentScrollController amPmController;
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
                    false,()=>
                    $"{GetType()} is only meant for dateAndTime mode or time mode"
                );
                return 0;
            }
            set { }
        }
        // The controller of the date column.
        FixedExtentScrollController dateController;

        int selectedHour {
            get { return _selectedHour(selectedAmPm, _selectedHourIndex);}
        } 
        int _selectedHourIndex {
            get { return  hourController.hasClients ? hourController.selectedItem % 24 : initialDateTime.Hour;
            }
        }
        int _selectedHour(int selectedAmPm, int selectedHour) {
            return _isHourRegionFlipped(selectedAmPm) ? (selectedHour + 12) % 24 : selectedHour;
        }
        FixedExtentScrollController hourController = null;

        
        int selectedMinute {
            get {
                return minuteController.hasClients
                    ? minuteController.selectedItem * widget.minuteInterval % 60
                    : initialDateTime.Minute;
            }
        }
        FixedExtentScrollController minuteController;
        
        int selectedAmPm;

        bool isHourRegionFlipped {
            get { return  _isHourRegionFlipped(selectedAmPm);}
        }
        bool _isHourRegionFlipped(int selectedAmPm) => selectedAmPm != meridiemRegion;
        int meridiemRegion;
        FixedExtentScrollController meridiemController;
        
        bool isDatePickerScrolling = false;
        bool isHourPickerScrolling = false;
        bool isMinutePickerScrolling = false;
        bool isMeridiemPickerScrolling = false;

        bool  isScrolling {
            get {
                return isDatePickerScrolling
                              || isHourPickerScrolling
                              || isMinutePickerScrolling
                              || isMeridiemPickerScrolling;
            }
        }

        public static Dictionary<int, float> estimatedColumnWidths = new Dictionary<int, float>();

        public override void initState() {
            base.initState();
            initialDateTime = widget.initialDateTime;

           
            selectedAmPm = (int)(initialDateTime.Hour / 12);
            meridiemRegion = selectedAmPm;

            meridiemController = new FixedExtentScrollController(initialItem: selectedAmPm);
            hourController = new FixedExtentScrollController(initialItem: initialDateTime.Hour);
            minuteController = new FixedExtentScrollController(initialItem: (int)(initialDateTime.Minute / widget.minuteInterval));
            dateController = new FixedExtentScrollController(initialItem: 0);

            PaintingBinding.instance.systemFonts.addListener(_handleSystemFontsChange);
        }
        void _handleSystemFontsChange () {
            setState(()=> {
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
            oldWidget = (CupertinoDatePicker) oldWidget;
            base.didUpdateWidget(oldWidget);

            D.assert(
                ((CupertinoDatePicker) oldWidget).mode == widget.mode,()=>
                $"The {GetType()}'s mode cannot change once it's built."
            );

            if (!widget.use24hFormat && ((CupertinoDatePicker) oldWidget).use24hFormat) {
                
                meridiemController.dispose();
                meridiemController = new FixedExtentScrollController(initialItem: selectedAmPm);
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
        DateTime  selectedDateTime {
            get {
                return new DateTime(
                    initialDateTime.Year,
                    initialDateTime.Month,
                    initialDateTime.Day + selectedDayFromInitial,
                    selectedHour,
                    selectedMinute,
                    0
                );
            }
        }
        void _onSelectedItemChange(int index) {
            DateTime selected = selectedDateTime;
            bool isDateInvalid = widget.minimumDate?.CompareTo(selected) < 0
                                 || widget.maximumDate?.CompareTo(selected) > 0;
            if (isDateInvalid)
                return;
            widget.onDateTimeChanged(selected);
        }
        Widget _buildMediumDatePicker(float offAxisFraction, TransitionBuilder itemPositioningBuilder) {
            return new NotificationListener<ScrollNotification>(
                onNotification: (ScrollNotification notification)=> { 
                    if (notification is ScrollStartNotification) { 
                        isDatePickerScrolling = true; 
                    } else if (notification is ScrollEndNotification) { 
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
                onSelectedItemChanged: (int index)=> { 
                    _onSelectedItemChange(index); 
                },
                itemBuilder: (BuildContext context, int index) => {
                    var rangeStart = new DateTime(
                        year:initialDateTime.Year, 
                        month:initialDateTime.Month, 
                        day:initialDateTime.Day
                    );
                    rangeStart = rangeStart.AddDays(index);
                    
                    var rangeEnd  = rangeStart.AddDays(1);
                    
                    DateTime now = DateTime.Now;
                    
                    if (widget.minimumDate?.CompareTo(rangeEnd) > 0 ) 
                        return null; 
                    if (widget.maximumDate?.CompareTo(rangeStart) < 0) 
                        return null;
                    
                    string dateText = rangeStart == new DateTime(now.Year, now.Month, now.Day)
                        ? localizations.todayLabel
                        : localizations.datePickerMediumDate(rangeStart);
                    return itemPositioningBuilder(
                        context, 
                        new Text(dateText, style: CupertinoDatePickerUtils._themeTextStyle(context))
                        ); 
                }
            )
            );
        }
        bool _isValidHour(int meridiemIndex, int hourIndex) {
            DateTime rangeStart = new DateTime(
                initialDateTime.Year, 
                initialDateTime.Month, 
                initialDateTime.Day + selectedDayFromInitial,
                _selectedHour(meridiemIndex, hourIndex),
                0,
                0
            );

            // The end value of the range is exclusive, i.e. [rangeStart, rangeEnd).
            DateTime rangeEnd = rangeStart.Add(new TimeSpan(0,1,0,0));

            return (widget.minimumDate?.CompareTo(rangeEnd) < 0 )
                   && !(widget.maximumDate?.CompareTo(rangeStart) > 0);
        }
        Widget _buildHourPicker(float offAxisFraction, TransitionBuilder itemPositioningBuilder) { 
           
            List<Widget> widgets = new List<Widget>();
            for (int index = 0; index < 24; index++) {
                int hour = isHourRegionFlipped ? (index + 12) % 24 : index;
                int displayHour = widget.use24hFormat ? hour : (hour + 11) % 12 + 1;
                widgets.Add(itemPositioningBuilder(
                    context,
                    new Text(
                        localizations.datePickerHour(displayHour),
                        //semanticsLabel: localizations.datePickerHourSemanticsLabel(displayHour),
                        style: CupertinoDatePickerUtils._themeTextStyle(context,
                            isValid: _isValidHour(selectedAmPm, index))
                    )
                ));
            }

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
                child: new CupertinoPicker(
                    scrollController: hourController,
                    offAxisFraction: offAxisFraction,
                    itemExtent: CupertinoDatePickerUtils._kItemExtent,
                    useMagnifier: CupertinoDatePickerUtils._kUseMagnifier,
                    magnification: CupertinoDatePickerUtils._kMagnification,
                    backgroundColor: widget.backgroundColor,
                    squeeze: CupertinoDatePickerUtils._kSqueeze,
                    onSelectedItemChanged: (int index) =>{
                      bool regionChanged = meridiemRegion != (int)(index / 12);
                      bool debugIsFlipped = isHourRegionFlipped;

                      if (regionChanged) {
                        meridiemRegion = (int)(index / 12);
                        selectedAmPm = 1 - selectedAmPm;
                      }

                      if (!widget.use24hFormat && regionChanged) {
                        
                        meridiemController.animateToItem(
                          selectedAmPm,
                          duration: TimeSpan.FromMilliseconds(300),
                          curve: Curves.easeOut
                        );
                      } else {
                        _onSelectedItemChange(index);
                      }

                      D.assert(debugIsFlipped == isHourRegionFlipped);
                    }, 
                    children: widgets,
                    looping: true
                )
            );
        }
        Widget _buildMinutePicker(float offAxisFraction, TransitionBuilder itemPositioningBuilder) { 
            List<Widget> widgets = new List<Widget>();
            for (int index = 0; index < 24; index++) {
                int minute = index * widget.minuteInterval;

                DateTime date = new DateTime(
                    initialDateTime.Year,
                    initialDateTime.Month,
                    initialDateTime.Day + selectedDayFromInitial,
                    selectedHour,
                    minute,
                    0
                );

                bool isInvalidMinute = (widget.minimumDate?.CompareTo(date) < 0 )
                                       || (widget.maximumDate?.CompareTo(date) > 0);

                widgets.Add( itemPositioningBuilder(
                    context,
                    new  Text(
                        localizations.datePickerMinute(minute),
                        //semanticsLabel: localizations.datePickerMinuteSemanticsLabel(minute),
                        style: CupertinoDatePickerUtils._themeTextStyle(context, isValid: !isInvalidMinute)
                    ))
                );
            }
            return new NotificationListener<ScrollNotification>(
                onNotification: (ScrollNotification notification) =>{ 
                    if (notification is ScrollStartNotification) { 
                        isMinutePickerScrolling = true; 
                    } else if (notification is ScrollEndNotification) { 
                        isMinutePickerScrolling = false; 
                        _pickerDidStopScrolling(); 
                    } 
                    return false; }, 
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
            List<Widget> widgets = new List<Widget>();
            for(int index = 0; index < 2; index ++ ){
                widgets.Add(
                    itemPositioningBuilder(
                        context,
                        new Text(
                            index == 0
                                ? localizations.anteMeridiemAbbreviation
                                : localizations.postMeridiemAbbreviation,
                            style: CupertinoDatePickerUtils._themeTextStyle(context, isValid: _isValidHour(index, _selectedHourIndex))
                        )));
            }
            return new NotificationListener<ScrollNotification>(
                onNotification: (ScrollNotification notification) =>{
                if (notification is ScrollStartNotification) {
                    isMeridiemPickerScrolling = true;
                } else if (notification is ScrollEndNotification) {
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
                onSelectedItemChanged: (int index) =>{
                selectedAmPm = index;
                D.assert(selectedAmPm == 0 || selectedAmPm == 1);
                _onSelectedItemChange(index);
            },
            children: widgets
            )
            );
        }
        void _pickerDidStopScrolling() {
           
            setState(()=> { });

            if (isScrolling)
                return;

            DateTime selectedDate = selectedDateTime;

            bool minCheck = widget.minimumDate?.CompareTo(selectedDate) < 0 ;
            bool maxCheck = widget.maximumDate?.CompareTo(selectedDate) > 0 ;

            if (minCheck || maxCheck) {
                // We have minCheck === !maxCheck.
                DateTime targetDate = (DateTime)(minCheck == true ? widget.minimumDate : widget.maximumDate) ;
                _scrollToDate(targetDate, selectedDate);
            }
        }
        void _scrollToDate(DateTime newDate, DateTime fromDate) { 
            D.assert(newDate != null);
            SchedulerBinding.instance.addPostFrameCallback((TimeSpan _)=> {
                if (fromDate.Year != newDate.Year || fromDate.Month != newDate.Month || fromDate.Day != newDate.Day) {
                    CupertinoDatePickerUtils._animateColumnControllerToItem(dateController, selectedDayFromInitial);
                }

                if (fromDate.Hour != newDate.Hour) {
                    bool needsMeridiemChange = !widget.use24hFormat
                                                     && fromDate.Hour / 12 != newDate.Hour / 12;
                    // In AM/PM mode, the pickers should not scroll all the way to the other hour region.
                    if (needsMeridiemChange) {
                        CupertinoDatePickerUtils._animateColumnControllerToItem(meridiemController, 1 - meridiemController.selectedItem);

                        // Keep the target item index in the current 12-h region.
                        int newItem = ((int)(hourController.selectedItem / 12)) * 12
                            + (hourController.selectedItem + newDate.Hour - fromDate.Hour) % 12;
                        CupertinoDatePickerUtils._animateColumnControllerToItem(hourController, newItem);
                    } else {
                        CupertinoDatePickerUtils._animateColumnControllerToItem(
                            hourController,
                            hourController.selectedItem + (int)newDate.Hour - (int)fromDate.Hour
                        );
                    }
                }

                if (fromDate.Minute != newDate.Minute) {
                    CupertinoDatePickerUtils._animateColumnControllerToItem(minuteController, newDate.Minute);
                }
            });
        }
        public override Widget build(BuildContext context) {
            List<float> columnWidths = new List<float>(){
                _getEstimatedColumnWidth(_PickerColumnType.hour), 
                _getEstimatedColumnWidth(_PickerColumnType.minute),
            }; 
            List<_ColumnBuilder> pickerBuilders = new List<_ColumnBuilder>(){
                _buildHourPicker,
                _buildMinutePicker
            };

            if (!widget.use24hFormat) { 
                if (localizations.datePickerDateTimeOrder == DatePickerDateTimeOrder.date_time_dayPeriod
                                            || localizations.datePickerDateTimeOrder == DatePickerDateTimeOrder.time_dayPeriod_date) { 
                    pickerBuilders.Add(_buildAmPmPicker);
                    columnWidths.Add(_getEstimatedColumnWidth(_PickerColumnType.dayPeriod));
                } else {
                    pickerBuilders.Insert(0, _buildAmPmPicker);
                    columnWidths.Insert(0, _getEstimatedColumnWidth(_PickerColumnType.dayPeriod));
                }
            } 
            if (widget.mode == CupertinoDatePickerMode.dateAndTime) { 
                if (localizations.datePickerDateTimeOrder == DatePickerDateTimeOrder.time_dayPeriod_date
                                                                          || localizations.datePickerDateTimeOrder == DatePickerDateTimeOrder.dayPeriod_time_date) { 
                    pickerBuilders.Add(_buildMediumDatePicker);
                    columnWidths.Add(_getEstimatedColumnWidth(_PickerColumnType.date)); 
                } else { 
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
                EdgeInsets padding = EdgeInsets.only(right:CupertinoDatePickerUtils._kDatePickerPadSize); 
                if (i == columnWidths.Count - 1) 
                    padding = padding.flipped;
                if (textDirectionFactor == -1) 
                    padding = padding.flipped;

                float width = columnWidths[i];
                pickers.Add(
                    new LayoutId(
                            id: i,
                            child: pickerBuilders[i](
                            offAxisFraction,
                          (BuildContext context1, Widget child) =>{
                            return new Container(
                                alignment: i == columnWidths.Count - 1
                                    ? alignCenterLeft
                                    : alignCenterRight,
                                padding: padding,
                                child: new Container(
                                    alignment: i == columnWidths.Count - 1 ? alignCenterLeft : alignCenterRight,
                                    width:  i == 0 || i == columnWidths.Count - 1
                                        ? (float?) null : (float)(width + CupertinoDatePickerUtils._kDatePickerPadSize),
                                    child: child
                                    )
                                ); 
                        }
                    )
                  ));
            }
            return new MediaQuery(
                data: MediaQuery.of(context).copyWith(textScaleFactor: 1.0f), 
                child: DefaultTextStyle.merge(
                    style:CupertinoDatePickerUtils._kDefaultPickerTextStyle, 
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

        bool isScrolling {
            get { return isDayPickerScrolling || isMonthPickerScrolling || isYearPickerScrolling; }
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

        void _refreshEstimatedColumnWidths() {
            estimatedColumnWidths[(int) _PickerColumnType.dayOfMonth] =
                CupertinoDatePicker._getColumnWidth(_PickerColumnType.dayOfMonth, localizations, context);
            estimatedColumnWidths[(int) _PickerColumnType.month] =
                CupertinoDatePicker._getColumnWidth(_PickerColumnType.month, localizations, context);
            estimatedColumnWidths[(int) _PickerColumnType.year] =
                CupertinoDatePicker._getColumnWidth(_PickerColumnType.year, localizations, context);
        }

        DateTime _lastDayInMonth(int year, int month) {
            //new DateTime(year, month + 1, 0);
            var date = new DateTime(year,month,1);
            date = date.AddMonths(1);
            date = date.Subtract(new TimeSpan(1, 0, 0, 0));
            return date;
        }

        Widget _buildDayPicker(float offAxisFraction, TransitionBuilder itemPositioningBuilder) {
            int daysInCurrentMonth = _lastDayInMonth(selectedYear, selectedMonth).Day;
            List<Widget> widgets = new List<Widget>();
            for (int index = 0; index < 31; index++) {
                int day = index + 1;
                widgets.Add(itemPositioningBuilder(
                    context,
                    new Text(
                        localizations.datePickerDayOfMonth(day),
                        style: CupertinoDatePickerUtils._themeTextStyle(context, isValid: day <= daysInCurrentMonth)
                    )
                ));
            }

            return new NotificationListener<ScrollNotification>(
                onNotification: (ScrollNotification notification) => {
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
                    onSelectedItemChanged: (int index) => {
                        selectedDay = index + 1;
                        if (_isCurrentDateValid)
                            widget.onDateTimeChanged(new DateTime(selectedYear, selectedMonth, selectedDay));
                    },
                    children: widgets,
                    looping: true
                )
            );
        }

        Widget _buildMonthPicker(float offAxisFraction, TransitionBuilder itemPositioningBuilder) {
            List<Widget> widgets = new List<Widget>();
            for (int index = 0; index < 12; index++) {
                int month = index + 1;
                bool isInvalidMonth = (widget.minimumDate?.Year == selectedYear && widget.minimumDate?.Month > month)
                                      || (widget.maximumDate?.Year == selectedYear && widget.maximumDate?.Month < month);

                widgets.Add(itemPositioningBuilder(
                    context,
                    new Text(
                        localizations.datePickerMonth(month),
                        style: CupertinoDatePickerUtils._themeTextStyle(context, isValid: !isInvalidMonth)
                    )
                ));
            }

            return new NotificationListener<ScrollNotification>(
                onNotification: (ScrollNotification notification) => {
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
                    onSelectedItemChanged: (int index) => {
                        selectedMonth = index + 1;
                        if (_isCurrentDateValid)
                            widget.onDateTimeChanged(new DateTime(selectedYear, selectedMonth, selectedDay));
                    },
                    children: widgets,
                    looping: true
                )
            );
        }

        Widget _buildYearPicker(float offAxisFraction, TransitionBuilder itemPositioningBuilder) {
            return new NotificationListener<ScrollNotification>(
                onNotification: (ScrollNotification notification) => {
                    if (notification is ScrollStartNotification) {
                        isYearPickerScrolling = true;
                    }
                    else if (notification is ScrollEndNotification) {
                        isYearPickerScrolling = false;
                        _pickerDidStopScrolling();
                    }

                    return false;
                },
                child: new CupertinoPicker (
                    scrollController: yearController,
                    itemExtent: CupertinoDatePickerUtils._kItemExtent,
                    offAxisFraction: offAxisFraction,
                    useMagnifier: CupertinoDatePickerUtils._kUseMagnifier,
                    magnification: CupertinoDatePickerUtils._kMagnification,
                    backgroundColor: widget.backgroundColor,
                    onSelectedItemChanged: (int index) => {
                        selectedYear = index;
                        if (_isCurrentDateValid)
                            widget.onDateTimeChanged(new DateTime(selectedYear, selectedMonth, selectedDay));
                    },
                    itemBuilder: (BuildContext _context, int year) => {
                        if (year < widget.minimumYear)
                            return null;

                        if (widget.maximumYear != null && year > widget.maximumYear)
                            return null;

                        bool isValidYear = (widget.minimumDate == null || widget.minimumDate?.Year <= year)
                                           && (widget.maximumDate == null || widget.maximumDate?.Year >= year);

                        return itemPositioningBuilder(
                            context,
                            new Text(
                                localizations.datePickerYear(year),
                                style: CupertinoDatePickerUtils._themeTextStyle(_context, isValid: isValidYear))
                        );

                    }
                ));
        }

        bool _isCurrentDateValid {
            // The current date selection represents a range [minSelectedData, maxSelectDate].
            get {
                DateTime minSelectedDate = new DateTime(selectedYear, selectedMonth, selectedDay);
                DateTime maxSelectedDate = new DateTime(selectedYear, selectedMonth, selectedDay);
                maxSelectedDate = maxSelectedDate.AddDays(1);

                bool minCheck = widget.minimumDate == null ? true : widget.minimumDate?.CompareTo(maxSelectedDate) < 0;
                bool maxCheck = widget.maximumDate == null ? false : widget.maximumDate?.CompareTo(minSelectedDate) > 0;

                return minCheck && !maxCheck && minSelectedDate.Day == selectedDay;
            }
        }

        void _pickerDidStopScrolling() {
            setState(() => { });

            if (isScrolling) {
                return;
            }

            DateTime minSelectDate = new DateTime(selectedYear, selectedMonth, selectedDay);
            DateTime maxSelectDate = new DateTime(selectedYear, selectedMonth, selectedDay + 1);

            bool minCheck = widget.minimumDate == null ? true : widget.minimumDate?.CompareTo(maxSelectDate) < 0 ;
           
            bool maxCheck =  widget.maximumDate == null ? false :widget.maximumDate?.CompareTo(minSelectDate) > 0;

            if (!minCheck || maxCheck) {
                DateTime targetDate = minCheck ? (DateTime) widget.maximumDate : (DateTime) widget.minimumDate;
                _scrollToDate(targetDate);
                return;
            }


            if (minSelectDate.Day != selectedDay) {
                DateTime lastDay = _lastDayInMonth(selectedYear, selectedMonth);
                _scrollToDate(lastDay);
            }
        }

        void _scrollToDate(DateTime newDate) {
            D.assert(newDate != null);
            SchedulerBinding.instance.addPostFrameCallback((TimeSpan timespan) => {
                if (selectedYear != newDate.Year) {
                    CupertinoDatePickerUtils._animateColumnControllerToItem(yearController, newDate.Year);
                }

                if (selectedMonth != newDate.Month) {
                    CupertinoDatePickerUtils._animateColumnControllerToItem(monthController, newDate.Month - 1);
                }

                if (selectedDay != newDate.Day) {
                    CupertinoDatePickerUtils._animateColumnControllerToItem(dayController, newDate.Day - 1);
                }
            });
        }

        public override Widget build(BuildContext context) {
            List<_ColumnBuilder> pickerBuilders = new List<_ColumnBuilder>();
            List<float> columnWidths = new List<float>();
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

            List<Widget> pickers = new List<Widget>();
            for (int i = 0; i < columnWidths.Count; i++) {
                int index = i;
                float offAxisFraction = (index - 1) * 0.3f * textDirectionFactor;
                EdgeInsets padding = EdgeInsets.only(right: CupertinoDatePickerUtils._kDatePickerPadSize);
                if (textDirectionFactor == -1)
                    padding = EdgeInsets.only(left: CupertinoDatePickerUtils._kDatePickerPadSize);

                Widget transitionBuilder(BuildContext _context, Widget child) {
                    var columnWidth = columnWidths.Count == 0 ? 0 : columnWidths[index];
                    var result = new Container(
                        alignment: index == (columnWidths.Count - 1)
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

                Widget childWidget =  pickerBuilders[index](
                    offAxisFraction: offAxisFraction,
                    itemPositioningBuilder :  builder
                ); 
                pickers.Add(new LayoutId(
                    id: index,
                    child: childWidget
                    )
                );
            }

            return new MediaQuery(
                data: MediaQuery.of(context).copyWith(textScaleFactor: 1.0f),
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
        hms,
    }

    public class CupertinoTimerPicker : StatefulWidget {
        public CupertinoTimerPicker(
            Key key = null,
            CupertinoTimerPickerMode mode = cupertino.CupertinoTimerPickerMode.hms,
            TimeSpan? initialTimerDuration = null,
            int minuteInterval = 1,
            int secondInterval = 1,
            AlignmentGeometry alignment = null,
            Color backgroundColor = null,
            ValueChanged<TimeSpan> onTimerDurationChanged = null
            ):base(key : key) {
            
            initialTimerDuration = initialTimerDuration ?? TimeSpan.Zero; 
            alignment = alignment ?? Alignment.center;
            
            D.assert(onTimerDurationChanged != null);
            D.assert(initialTimerDuration >= TimeSpan.Zero);
            D.assert(initialTimerDuration < new TimeSpan(1,0,0,0));
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
        CupertinoLocalizations localizations;
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
        Alignment alignCenterLeft;
        Alignment alignCenterRight;
        
        int selectedHour;
        int selectedMinute;
        int selectedSecond;
        
        int lastSelectedHour;
        int lastSelectedMinute;
        int lastSelectedSecond;
        
        public readonly TextPainter textPainter = new TextPainter();
        public readonly List<string> numbers = CupertinoDatePickerUtils.CreateNumbers();
        
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
            PaintingBinding.instance.systemFonts.addListener(_handleSystemFontsChange);
        }
        void _handleSystemFontsChange() {
            setState(() =>{
                textPainter.markNeedsLayout();
                _measureLabelMetrics();
            });
        }

        public override void dispose() {
            PaintingBinding.instance.systemFonts.removeListener(_handleSystemFontsChange);
            base.dispose();
        }

        public override void didUpdateWidget(StatefulWidget oldWidget) {
            oldWidget = (CupertinoTimerPicker) oldWidget;
            base.didUpdateWidget(oldWidget);

            D.assert(
                ((CupertinoTimerPicker) oldWidget).mode == widget.mode,()=>
                "The CupertinoTimerPicker's mode cannot change once it's built"
            );
        }
        public override void didChangeDependencies() {
            base.didChangeDependencies();

            textDirection = Directionality.of(context);
            localizations = CupertinoLocalizations.of(context);

            _measureLabelMetrics();
        }
        void _measureLabelMetrics() {
            textPainter.textDirection = textDirection;
            TextStyle textStyle = _textStyleFrom(context);

            float maxWidth = float.NegativeInfinity;
            string widestNumber = "";

           
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
                text: $"{widestNumber}{widestNumber}",
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
                    child: new  Text(
                      text,
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
              padding: padding.resolve(textDirection),
              alignment: AlignmentDirectional.centerStart.resolve(textDirection),
              child: new Container(
                width: numberLabelWidth,
                alignment: AlignmentDirectional.centerEnd.resolve(textDirection),
                child: new Text(text, softWrap: false, maxLines: 1, overflow: TextOverflow.visible)
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
              onSelectedItemChanged: (int index)=> {
                setState(() =>{
                  selectedHour = index;
                  widget.onTimerDurationChanged(
                    new TimeSpan(
                      hours: selectedHour,
                      minutes: selectedMinute,
                      seconds: selectedSecond == 0 ? 0 : selectedHour));
                });
              },
              children: CupertinoDatePickerUtils.listGenerate(24, (int index) => {
                 string semanticsLabel = textDirectionFactor == 1
                  ? localizations.timerPickerHour(index) + localizations.timerPickerHourLabel(index)
                  : localizations.timerPickerHourLabel(index) + localizations.timerPickerHour(index);

                 return _buildPickerNumberLabel(localizations.timerPickerHour(index), additionalPadding);
              })
            );
          }

          Widget _buildHourColumn(EdgeInsetsDirectional additionalPadding) {
            return new Stack(
              children: new List<Widget>{
                new NotificationListener<ScrollEndNotification>(
                  onNotification: (ScrollEndNotification notification)=> {
                    setState(()=> { lastSelectedHour = selectedHour; });
                    return false;
                  },
                  child: _buildHourPicker(additionalPadding)
                ),
                _buildLabel(
                  localizations.timerPickerHourLabel(lastSelectedHour == 0  ? selectedHour :  lastSelectedHour),
                  additionalPadding
                ),
              }
            );
          }

          Widget _buildMinutePicker(EdgeInsetsDirectional additionalPadding) {
            float offAxisFraction = 0f;
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
                initialItem: (int)selectedMinute / widget.minuteInterval
              ),
              offAxisFraction: offAxisFraction,
              itemExtent: CupertinoDatePickerUtils._kItemExtent,
              backgroundColor: widget.backgroundColor,
              squeeze: CupertinoDatePickerUtils._kSqueeze,
              looping: true,
              onSelectedItemChanged: (int index) => {
                setState(() =>{
                  selectedMinute = index * widget.minuteInterval;
                  widget.onTimerDurationChanged(
                    new TimeSpan(
                      hours: selectedHour == 0 ? 0 : selectedHour,
                      minutes: selectedMinute,
                      seconds: selectedSecond == 0 ? 0 : selectedSecond ));
                });
              },
              children: CupertinoDatePickerUtils.listGenerate((int)(60 / widget.minuteInterval), (int index) => {
                 int minute = index * widget.minuteInterval;
                 string semanticsLabel = textDirectionFactor == 1
                  ? localizations.timerPickerMinute(minute) + localizations.timerPickerMinuteLabel(minute)
                  : localizations.timerPickerMinuteLabel(minute) + localizations.timerPickerMinute(minute);
                 return _buildPickerNumberLabel(localizations.timerPickerMinute(minute), additionalPadding);
              })
            );
          }

          Widget _buildMinuteColumn(EdgeInsetsDirectional additionalPadding) {
            return new Stack(
              children: new List<Widget>{
                new NotificationListener<ScrollEndNotification>(
                  onNotification: (ScrollEndNotification notification)=> {
                    setState(() => { lastSelectedMinute = selectedMinute; });
                    return false;
                  },
                  child: _buildMinutePicker(additionalPadding)
                ),
                _buildLabel(
                  localizations.timerPickerMinuteLabel(lastSelectedMinute == 0 ? selectedMinute : lastSelectedMinute),
                  additionalPadding
                ),
              }
            );
          }

          Widget _buildSecondPicker(EdgeInsetsDirectional additionalPadding) {
             float offAxisFraction = 0.5f * textDirectionFactor;

            return new CupertinoPicker(
              scrollController: new FixedExtentScrollController(
                initialItem: (int) selectedSecond / widget.secondInterval
              ),
              offAxisFraction: offAxisFraction,
              itemExtent: CupertinoDatePickerUtils._kItemExtent,
              backgroundColor: widget.backgroundColor,
              squeeze: CupertinoDatePickerUtils._kSqueeze,
              looping: true,
              onSelectedItemChanged: (int index)=> {
                setState(() => {
                  selectedSecond = index * widget.secondInterval;
                  widget.onTimerDurationChanged(
                    new TimeSpan(
                      hours: selectedHour == 0 ? 0 : selectedHour,
                      minutes: selectedMinute,
                      seconds: selectedSecond));
                });
              },
              children: CupertinoDatePickerUtils.listGenerate((int) (60 / widget.secondInterval), (int index)=> {
                 int second = index * widget.secondInterval;

                 string semanticsLabel = textDirectionFactor == 1
                  ? localizations.timerPickerSecond(second) + localizations.timerPickerSecondLabel(second)
                  : localizations.timerPickerSecondLabel(second) + localizations.timerPickerSecond(second);
                 return _buildPickerNumberLabel(localizations.timerPickerSecond(second), additionalPadding); 
              })
            );
          }

          Widget _buildSecondColumn(EdgeInsetsDirectional additionalPadding) {
            return new Stack(
              children: new List<Widget>{
                new NotificationListener<ScrollEndNotification>(
                  onNotification: (ScrollEndNotification notification)=> {
                    setState(() => { lastSelectedSecond = selectedSecond; });
                    return false;
                  },
                  child: _buildSecondPicker(additionalPadding)
                ),
                _buildLabel(
                  localizations.timerPickerSecondLabel(lastSelectedSecond == 0 ? selectedSecond : lastSelectedSecond),
                  additionalPadding
                )
              }
            );
          }

          TextStyle _textStyleFrom(BuildContext context) {
            return CupertinoTheme.of(context).textTheme
              .pickerTextStyle.merge(
                new TextStyle(
                  fontSize: CupertinoDatePickerUtils._kTimerPickerNumberLabelFontSize
                )
              );
          }
          public override Widget build(BuildContext context) {
              List<Widget> columns = new List<Widget>();
            float paddingValue = CupertinoDatePickerUtils._kPickerWidth - 2 * CupertinoDatePickerUtils._kTimerPickerColumnIntrinsicWidth - 2 * CupertinoDatePickerUtils._kTimerPickerHalfColumnPadding;
          
            float totalWidth = CupertinoDatePickerUtils._kPickerWidth;
            D.assert(paddingValue >= 0);

            switch (widget.mode) {
              case CupertinoTimerPickerMode.hm:
                // Pad the widget to make it as wide as `_kPickerWidth`.
                columns = new List<Widget>{
                  _buildHourColumn( EdgeInsetsDirectional.only(start: paddingValue / 2, end: CupertinoDatePickerUtils._kTimerPickerHalfColumnPadding)),
                  _buildMinuteColumn( EdgeInsetsDirectional.only(start: CupertinoDatePickerUtils._kTimerPickerHalfColumnPadding, end: paddingValue / 2)),
                };
                break;
              case CupertinoTimerPickerMode.ms:
                // Pad the widget to make it as wide as `_kPickerWidth`.
                columns = new List<Widget>{
                  _buildMinuteColumn( EdgeInsetsDirectional.only(start: paddingValue / 2, end: CupertinoDatePickerUtils._kTimerPickerHalfColumnPadding)),
                  _buildSecondColumn( EdgeInsetsDirectional.only(start: CupertinoDatePickerUtils._kTimerPickerHalfColumnPadding, end: paddingValue / 2)),
                };
                break;
              case CupertinoTimerPickerMode.hms:
                 float _paddingValue = CupertinoDatePickerUtils._kTimerPickerHalfColumnPadding * 2;
                totalWidth = CupertinoDatePickerUtils._kTimerPickerColumnIntrinsicWidth * 3 + 4 * CupertinoDatePickerUtils._kTimerPickerHalfColumnPadding + _paddingValue;
                columns = new List<Widget>{
                  _buildHourColumn( EdgeInsetsDirectional.only(start: _paddingValue / 2, end: CupertinoDatePickerUtils._kTimerPickerHalfColumnPadding)),
                  _buildMinuteColumn( EdgeInsetsDirectional.only(start: CupertinoDatePickerUtils._kTimerPickerHalfColumnPadding, end: CupertinoDatePickerUtils._kTimerPickerHalfColumnPadding)),
                  _buildSecondColumn( EdgeInsetsDirectional.only(start: CupertinoDatePickerUtils._kTimerPickerHalfColumnPadding, end: _paddingValue / 2)),
                };
                break;
            }
             CupertinoThemeData themeData = CupertinoTheme.of(context);
            return new MediaQuery(
              // The native iOS picker's text scaling is fixed, so we will also fix it
              // as well in our picker.
              data: MediaQuery.of(context).copyWith(textScaleFactor: 1.0f),
              child: new CupertinoTheme(
                data: themeData.copyWith(
                  textTheme: themeData.textTheme.copyWith(
                    pickerTextStyle: _textStyleFrom(context)
                  )
                ),
                child: new Align(
                  alignment: widget.alignment,
                  child: new Container(
                    color: CupertinoDynamicColor.resolve(widget.backgroundColor, context),
                    width: totalWidth,
                    height: CupertinoDatePickerUtils._kPickerHeight,
                    child: new DefaultTextStyle(
                      style: _textStyleFrom(context),
                      child: new Row(
                          children:
                              columns.Select((Widget child) => {
                                      var result = new Expanded(child: child);
                                      return (Widget) result;
                              }).ToList()
                          )
                    )
                  )
                )
              )
            );
          }


    }
}
