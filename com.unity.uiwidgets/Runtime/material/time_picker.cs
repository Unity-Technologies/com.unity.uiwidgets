using System;
using System.Collections.Generic;
using System.Linq;
using uiwidgets;
using Unity.UIWidgets.animation;
using Unity.UIWidgets.async;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.gestures;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.rendering;
using Unity.UIWidgets.ui;
using Unity.UIWidgets.widgets;
using UnityEngine;
using Canvas = Unity.UIWidgets.ui.Canvas;
using Color = Unity.UIWidgets.ui.Color;
using Rect = Unity.UIWidgets.ui.Rect;
using TextStyle = Unity.UIWidgets.painting.TextStyle;

namespace Unity.UIWidgets.material {
    public static class TimePickerUtils {
        public static readonly TimeSpan _kDialAnimateDuration = new TimeSpan(0, 0, 0, 0, 200);
        public const float _kTwoPi = 2 * Mathf.PI;
        public static readonly TimeSpan _kVibrateCommitDelay = new TimeSpan(0, 0, 0, 0, milliseconds: 100);

        public const float _kTimePickerHeaderPortraitHeight = 96.0f;
        public const float _kTimePickerHeaderLandscapeWidth = 168.0f;


        public const float _kTimePickerWidthPortrait = 328.0f;
        public const float _kTimePickerWidthLandscape = 512.0f;

        public const float _kTimePickerHeightPortrait = 496.0f;
        public const float _kTimePickerHeightLandscape = 316.0f;

        public const float _kTimePickerHeightPortraitCollapsed = 484.0f;
        public const float _kTimePickerHeightLandscapeCollapsed = 304.0f;

        public static readonly BoxConstraints _kMinTappableRegion = new BoxConstraints(minWidth: 48, minHeight: 48);

        public static _TimePickerHeaderFormat _buildHeaderFormat(
            TimeOfDayFormat timeOfDayFormat,
            _TimePickerFragmentContext context,
            Orientation orientation
        ) {
            _TimePickerHeaderFragment hour() {
                return new _TimePickerHeaderFragment(
                    layoutId: _TimePickerHeaderId.hour,
                    widget: new _HourControl(fragmentContext: context)
                );
            }

            _TimePickerHeaderFragment minute() {
                return new _TimePickerHeaderFragment(
                    layoutId: _TimePickerHeaderId.minute,
                    widget: new _MinuteControl(fragmentContext: context)
                );
            }

            _TimePickerHeaderFragment getString(_TimePickerHeaderId layoutId, string value) {
                return new _TimePickerHeaderFragment(
                    layoutId: layoutId,
                    widget: new _StringFragment(
                        fragmentContext: context,
                        value: value
                    )
                );
            }

            _TimePickerHeaderFragment dayPeriod() {
                return new _TimePickerHeaderFragment(
                    layoutId: _TimePickerHeaderId.period,
                    widget: new _DayPeriodControl(fragmentContext: context, orientation: orientation)
                );
            }

            _TimePickerHeaderFormat format(
                _TimePickerHeaderPiece piece1,
                _TimePickerHeaderPiece piece2 = null
            ) {
                List<_TimePickerHeaderPiece> pieces = new List<_TimePickerHeaderPiece>();
                switch (context.textDirection) {
                    case TextDirection.ltr:
                        pieces.Add(piece1);
                        if (piece2 != null) {
                            pieces.Add(piece2);
                        }

                        break;
                    case TextDirection.rtl:
                        if (piece2 != null) {
                            pieces.Add(piece2);
                        }

                        pieces.Add(piece1);
                        break;
                }

                int? centerpieceIndex = null;
                for (int i = 0; i < pieces.Count; i += 1) {
                    if (pieces[i].pivotIndex >= 0) {
                        centerpieceIndex = i;
                    }
                }

                D.assert(centerpieceIndex != null);
                return new _TimePickerHeaderFormat(centerpieceIndex.Value, pieces);
            }

            _TimePickerHeaderPiece piece(
                int pivotIndex = -1,
                float bottomMargin = 0.0f,
                _TimePickerHeaderFragment fragment1 = null,
                _TimePickerHeaderFragment fragment2 = null,
                _TimePickerHeaderFragment fragment3 = null
            ) {
                List<_TimePickerHeaderFragment> fragments = new List<_TimePickerHeaderFragment> {fragment1};
                if (fragment2 != null) {
                    fragments.Add(fragment2);
                    if (fragment3 != null) {
                        fragments.Add(fragment3);
                    }
                }

                return new _TimePickerHeaderPiece(pivotIndex, fragments, bottomMargin: bottomMargin);
            }

            switch (timeOfDayFormat) {
                case TimeOfDayFormat.h_colon_mm_space_a:
                    return format(
                        piece(
                            pivotIndex: 1,
                            fragment1: hour(),
                            fragment2: getString(_TimePickerHeaderId.colon, ":"),
                            fragment3: minute()
                        ),
                        piece(
                            fragment1: dayPeriod()
                        )
                    );
                case TimeOfDayFormat.H_colon_mm:
                    return format(piece(
                        pivotIndex: 1,
                        fragment1: hour(),
                        fragment2: getString(_TimePickerHeaderId.colon, ":"),
                        fragment3: minute()
                    ));
                case TimeOfDayFormat.HH_dot_mm:
                    return format(piece(
                        pivotIndex: 1,
                        fragment1: hour(),
                        fragment2: getString(_TimePickerHeaderId.dot, "."),
                        fragment3: minute()
                    ));
                case TimeOfDayFormat.a_space_h_colon_mm:
                    return format(
                        piece(
                            fragment1: dayPeriod()
                        ),
                        piece(
                            pivotIndex: 1,
                            fragment1: hour(),
                            fragment2: getString(_TimePickerHeaderId.colon, ":"),
                            fragment3: minute()
                        )
                    );
                case TimeOfDayFormat.frenchCanadian:
                    return format(piece(
                        pivotIndex: 1,
                        fragment1: hour(),
                        fragment2: getString(_TimePickerHeaderId.hString, "h"),
                        fragment3: minute()
                    ));
                case TimeOfDayFormat.HH_colon_mm:
                    return format(piece(
                        pivotIndex: 1,
                        fragment1: hour(),
                        fragment2: getString(_TimePickerHeaderId.colon, ":"),
                        fragment3: minute()
                    ));
            }

            return null;
        }


        public static Future<TimeOfDay> showTimePicker(
            BuildContext context,
            TimeOfDay initialTime,
            TransitionBuilder builder = null,
            bool useRootNavigator = true,
            RouteSettings routeSettings = null
        ) {
            D.assert(context != null);
            D.assert(initialTime != null);
            D.assert(material_.debugCheckHasMaterialLocalizations(context));

            Widget dialog = new _TimePickerDialog(initialTime: initialTime);
            return material_.showDialog<TimeOfDay>(
                context: context,
                useRootNavigator: useRootNavigator,
                builder: (BuildContext subContext) => {
                    return builder == null ? dialog : builder(subContext, dialog);
                },
                routeSettings: routeSettings
            );
        }
    }

    public enum _TimePickerMode {
        hour,
        minute
    }

    public enum _TimePickerHeaderId {
        hour,
        colon,
        minute,
        period, // AM/PM picker
        dot,
        hString, // French Canadian "h" literal
    }

    public class _TimePickerFragmentContext {
        public _TimePickerFragmentContext(
            TextTheme headerTextTheme = null,
            TextDirection? textDirection = null,
            TimeOfDay selectedTime = null,
            _TimePickerMode? mode = null,
            Color activeColor = null,
            TextStyle activeStyle = null,
            Color inactiveColor = null,
            TextStyle inactiveStyle = null,
            ValueChanged<TimeOfDay> onTimeChange = null,
            ValueChanged<_TimePickerMode> onModeChange = null,
            RuntimePlatform? targetPlatform = null,
            bool? use24HourDials = null
        ) {
            D.assert(headerTextTheme != null);
            D.assert(textDirection != null);
            D.assert(selectedTime != null);
            D.assert(mode != null);
            D.assert(activeColor != null);
            D.assert(activeStyle != null);
            D.assert(inactiveColor != null);
            D.assert(inactiveStyle != null);
            D.assert(onTimeChange != null);
            D.assert(onModeChange != null);
            D.assert(targetPlatform != null);
            D.assert(use24HourDials != null);

            this.headerTextTheme = headerTextTheme;
            this.textDirection = textDirection.Value;
            this.selectedTime = selectedTime;
            this.mode = mode.Value;
            this.activeColor = activeColor;
            this.activeStyle = activeStyle;
            this.inactiveColor = inactiveColor;
            this.inactiveStyle = inactiveStyle;
            this.onTimeChange = onTimeChange;
            this.onModeChange = onModeChange;
            this.targetPlatform = targetPlatform.Value;
            this.use24HourDials = use24HourDials.Value;
        }

        public readonly TextTheme headerTextTheme;
        public readonly TextDirection textDirection;
        public readonly TimeOfDay selectedTime;
        public readonly _TimePickerMode mode;
        public readonly Color activeColor;
        public readonly TextStyle activeStyle;
        public readonly Color inactiveColor;
        public readonly TextStyle inactiveStyle;
        public readonly ValueChanged<TimeOfDay> onTimeChange;
        public readonly ValueChanged<_TimePickerMode> onModeChange;
        public readonly RuntimePlatform targetPlatform;
        public readonly bool use24HourDials;
    }

    public class _TimePickerHeaderFragment {
        public _TimePickerHeaderFragment(
            _TimePickerHeaderId? layoutId = null,
            Widget widget = null,
            float startMargin = 0.0f
        ) {
            D.assert(layoutId != null);
            D.assert(widget != null);
            this.layoutId = layoutId.Value;
            this.widget = widget;
            this.startMargin = startMargin;
        }

        public readonly _TimePickerHeaderId layoutId;

        public readonly Widget widget;

        public readonly float startMargin;
    }

    public class _TimePickerHeaderPiece {
        public _TimePickerHeaderPiece(int pivotIndex, List<_TimePickerHeaderFragment> fragments,
            float bottomMargin = 0.0f) {
            D.assert(fragments != null);
            this.pivotIndex = pivotIndex;
            this.fragments = fragments;
            this.bottomMargin = bottomMargin;
        }

        public readonly int pivotIndex;

        public readonly List<_TimePickerHeaderFragment> fragments;

        public readonly float bottomMargin;
    }

    public class _TimePickerHeaderFormat {
        public _TimePickerHeaderFormat(int centerpieceIndex, List<_TimePickerHeaderPiece> pieces) {
            D.assert(pieces != null);
            this.centerpieceIndex = centerpieceIndex;
            this.pieces = pieces;
        }

        public readonly int centerpieceIndex;

        public readonly List<_TimePickerHeaderPiece> pieces;
    }

    class _DayPeriodControl : StatelessWidget {
        public _DayPeriodControl(
            _TimePickerFragmentContext fragmentContext,
            Orientation orientation
        ) {
            this.fragmentContext = fragmentContext;
            this.orientation = orientation;
        }

        public readonly _TimePickerFragmentContext fragmentContext;
        public readonly Orientation orientation;

        void _togglePeriod() {
            int newHour = (fragmentContext.selectedTime.hour + TimeOfDay.hoursPerPeriod) % TimeOfDay.hoursPerDay;
            TimeOfDay newTime = fragmentContext.selectedTime.replacing(hour: newHour);
            fragmentContext.onTimeChange(newTime);
        }

        void _setAm(BuildContext context) {
            if (fragmentContext.selectedTime.period == DayPeriod.am) {
                return;
            }

            _togglePeriod();
        }

        void _setPm(BuildContext context) {
            if (fragmentContext.selectedTime.period == DayPeriod.pm) {
                return;
            }

            _togglePeriod();
        }

        public override Widget build(BuildContext context) {
            MaterialLocalizations materialLocalizations = MaterialLocalizations.of(context);
            TextTheme headerTextTheme = fragmentContext.headerTextTheme;
            TimeOfDay selectedTime = fragmentContext.selectedTime;
            Color activeColor = fragmentContext.activeColor;
            Color inactiveColor = fragmentContext.inactiveColor;
            bool amSelected = selectedTime.period == DayPeriod.am;
            TextStyle amStyle = headerTextTheme.subtitle1.copyWith(
                color: amSelected ? activeColor : inactiveColor
            );
            TextStyle pmStyle = headerTextTheme.subtitle1.copyWith(
                color: !amSelected ? activeColor : inactiveColor
            );
            bool layoutPortrait = orientation == Orientation.portrait;

            float buttonTextScaleFactor = Mathf.Min(MediaQuery.of(context).textScaleFactor, 2.0f);

            Widget amButton = new ConstrainedBox(
                constraints: TimePickerUtils._kMinTappableRegion,
                child: new Material(
                    type: MaterialType.transparency,
                    child: new InkWell(
                        onTap: Feedback.wrapForTap(() => _setAm(context), context),
                        child: new Padding(
                            padding: layoutPortrait ? EdgeInsets.only(bottom: 2.0f) : EdgeInsets.only(right: 4.0f),
                            child: new Align(
                                alignment: layoutPortrait ? Alignment.bottomCenter : Alignment.centerRight,
                                widthFactor: 1,
                                heightFactor: 1,
                                child: new Text(
                                    materialLocalizations.anteMeridiemAbbreviation,
                                    style: amStyle,
                                    textScaleFactor: buttonTextScaleFactor
                                )
                            )
                        )
                    )
                )
            );

            Widget pmButton = new ConstrainedBox(
                constraints: TimePickerUtils._kMinTappableRegion,
                child: new Material(
                    type: MaterialType.transparency,
                    textStyle: pmStyle,
                    child: new InkWell(
                        onTap: Feedback.wrapForTap(() => _setPm(context), context),
                        child: new Padding(
                            padding: layoutPortrait ? EdgeInsets.only(top: 2.0f) : EdgeInsets.only(left: 4.0f),
                            child: new Align(
                                alignment: orientation == Orientation.portrait
                                    ? Alignment.topCenter
                                    : Alignment.centerLeft,
                                widthFactor: 1,
                                heightFactor: 1,
                                child: new Text(
                                    materialLocalizations.postMeridiemAbbreviation,
                                    style: pmStyle,
                                    textScaleFactor: buttonTextScaleFactor
                                )
                            )
                        )
                    )
                )
            );

            switch (orientation) {
                case Orientation.portrait:
                    return new Column(
                        mainAxisSize: MainAxisSize.min,
                        children: new List<Widget> {
                            amButton,
                            pmButton
                        }
                    );

                case Orientation.landscape:
                    return new Row(
                        mainAxisSize: MainAxisSize.min,
                        children: new List<Widget> {
                            amButton,
                            pmButton
                        }
                    );
            }

            return null;
        }
    }

    class _HourControl : StatelessWidget {
        public _HourControl(
            _TimePickerFragmentContext fragmentContext
        ) {
            this.fragmentContext = fragmentContext;
        }

        public readonly _TimePickerFragmentContext fragmentContext;

        public override Widget build(BuildContext context) {
            D.assert(WidgetsD.debugCheckHasMediaQuery(context));
            bool alwaysUse24HourFormat = MediaQuery.of(context).alwaysUse24HourFormat;
            MaterialLocalizations localizations = MaterialLocalizations.of(context);
            TextStyle hourStyle = fragmentContext.mode == _TimePickerMode.hour
                ? fragmentContext.activeStyle
                : fragmentContext.inactiveStyle;
            string formattedHour = localizations.formatHour(
                fragmentContext.selectedTime,
                alwaysUse24HourFormat: alwaysUse24HourFormat
            );

            TimeOfDay hoursFromSelected(int hoursToAdd) {
                if (fragmentContext.use24HourDials) {
                    int selectedHour = fragmentContext.selectedTime.hour;
                    return fragmentContext.selectedTime.replacing(
                        hour: (selectedHour + hoursToAdd + TimeOfDay.hoursPerDay) % TimeOfDay.hoursPerDay
                    );
                }
                else {
                    int periodOffset = fragmentContext.selectedTime.periodOffset;
                    int hours = fragmentContext.selectedTime.hourOfPeriod;
                    return fragmentContext.selectedTime.replacing(
                        hour: (periodOffset + (hours + hoursToAdd) + TimeOfDay.hoursPerPeriod) % TimeOfDay.hoursPerPeriod
                    );
                }
            }

            TimeOfDay nextHour = hoursFromSelected(1);
            string formattedNextHour = localizations.formatHour(
                nextHour,
                alwaysUse24HourFormat: alwaysUse24HourFormat
            );
            TimeOfDay previousHour = hoursFromSelected(-1);
            string formattedPreviousHour = localizations.formatHour(
                previousHour,
                alwaysUse24HourFormat: alwaysUse24HourFormat
            );

            return new ConstrainedBox(
                constraints: TimePickerUtils._kMinTappableRegion,
                child: new Material(
                    type: MaterialType.transparency,
                    child: new InkWell(
                        onTap: Feedback.wrapForTap(() => fragmentContext.onModeChange(_TimePickerMode.hour), context),
                        child: new Text(
                            formattedHour,
                            style: hourStyle,
                            textAlign: TextAlign.end,
                            textScaleFactor: 1.0f
                        )
                    )
                )
            );
        }
    }

    class _StringFragment : StatelessWidget {
        public _StringFragment(
            _TimePickerFragmentContext fragmentContext,
            string value
        ) {
            this.fragmentContext = fragmentContext;
            this.value = value;
        }

        public readonly _TimePickerFragmentContext fragmentContext;
        public readonly string value;

        public override Widget build(BuildContext context) {
            return new Text(value, style: fragmentContext.inactiveStyle, textScaleFactor: 1.0f);
        }
    }

    class _MinuteControl : StatelessWidget {
        public _MinuteControl(
            _TimePickerFragmentContext fragmentContext
        ) {
            this.fragmentContext = fragmentContext;
        }

        public readonly _TimePickerFragmentContext fragmentContext;

        public override Widget build(BuildContext context) {
            MaterialLocalizations localizations = MaterialLocalizations.of(context);
            TextStyle minuteStyle = fragmentContext.mode == _TimePickerMode.minute
                ? fragmentContext.activeStyle
                : fragmentContext.inactiveStyle;
            string formattedMinute = localizations.formatMinute(fragmentContext.selectedTime);
            TimeOfDay nextMinute = fragmentContext.selectedTime.replacing(
                minute: (fragmentContext.selectedTime.minute + 1) % TimeOfDay.minutesPerHour
            );
            string formattedNextMinute = localizations.formatMinute(nextMinute);
            TimeOfDay previousMinute = fragmentContext.selectedTime.replacing(
                minute: (fragmentContext.selectedTime.minute - 1 + TimeOfDay.minutesPerHour) % TimeOfDay.minutesPerHour
            );
            string formattedPreviousMinute = localizations.formatMinute(previousMinute);

            return new ConstrainedBox(
                constraints: TimePickerUtils._kMinTappableRegion,
                child: new Material(
                    type: MaterialType.transparency,
                    child: new InkWell(
                        onTap: Feedback.wrapForTap(() => fragmentContext.onModeChange(_TimePickerMode.minute), context),
                        child: new Text(formattedMinute, style: minuteStyle, textAlign: TextAlign.start,
                            textScaleFactor: 1.0f)
                    )
                )
            );
        }
    }

    class _TimePickerHeaderLayout : MultiChildLayoutDelegate {
        public _TimePickerHeaderLayout(
            Orientation orientation,
            _TimePickerHeaderFormat format) {
            this.orientation = orientation;
            this.format = format;
        }

        public readonly Orientation orientation;
        public readonly _TimePickerHeaderFormat format;

        public override void performLayout(Size size) {
            BoxConstraints constraints = BoxConstraints.loose(size);

            switch (orientation) {
                case Orientation.portrait:
                    _layoutHorizontally(size, constraints);
                    break;
                case Orientation.landscape:
                    _layoutVertically(size, constraints);
                    break;
            }
        }

        void _layoutHorizontally(Size size, BoxConstraints constraints) {
            List<_TimePickerHeaderFragment> fragmentsFlattened = new List<_TimePickerHeaderFragment>();
            Dictionary<_TimePickerHeaderId, Size> childSizes = new Dictionary<_TimePickerHeaderId, Size>();
            int pivotIndex = 0;
            for (int pieceIndex = 0; pieceIndex < format.pieces.Count; pieceIndex += 1) {
                _TimePickerHeaderPiece piece = format.pieces[pieceIndex];
                foreach (_TimePickerHeaderFragment fragment in piece.fragments) {
                    childSizes[fragment.layoutId] = layoutChild(fragment.layoutId, constraints);
                    fragmentsFlattened.Add(fragment);
                }

                if (pieceIndex == format.centerpieceIndex) {
                    pivotIndex += format.pieces[format.centerpieceIndex].pivotIndex;
                }
                else if (pieceIndex < format.centerpieceIndex) {
                    pivotIndex += piece.fragments.Count;
                }
            }

            _positionPivoted(size.width, size.height / 2.0f, childSizes, fragmentsFlattened, pivotIndex);
        }

        void _layoutVertically(Size size, BoxConstraints constraints) {
            Dictionary<_TimePickerHeaderId, Size> childSizes = new Dictionary<_TimePickerHeaderId, Size>();
            List<float> pieceHeights = new List<float>();
            float height = 0.0f;
            float margin = 0.0f;
            foreach (_TimePickerHeaderPiece piece in format.pieces) {
                float pieceHeight = 0.0f;
                foreach (_TimePickerHeaderFragment fragment in piece.fragments) {
                    Size childSize = childSizes[fragment.layoutId] = layoutChild(fragment.layoutId, constraints);
                    pieceHeight = Mathf.Max(pieceHeight, childSize.height);
                }

                pieceHeights.Add(pieceHeight);
                height += pieceHeight + margin;
                margin = piece.bottomMargin;
            }

            _TimePickerHeaderPiece centerpiece = format.pieces[format.centerpieceIndex];
            float y = (size.height - height) / 2.0f;
            for (int pieceIndex = 0; pieceIndex < format.pieces.Count; pieceIndex += 1) {
                float pieceVerticalCenter = y + pieceHeights[pieceIndex] / 2.0f;
                if (pieceIndex != format.centerpieceIndex) {
                    _positionPiece(size.width, pieceVerticalCenter, childSizes, format.pieces[pieceIndex].fragments);
                }
                else {
                    _positionPivoted(size.width, pieceVerticalCenter, childSizes, centerpiece.fragments,
                        centerpiece.pivotIndex);
                }

                y += pieceHeights[pieceIndex] + format.pieces[pieceIndex].bottomMargin;
            }
        }

        void _positionPivoted(float width, float y, Dictionary<_TimePickerHeaderId, Size> childSizes,
            List<_TimePickerHeaderFragment> fragments, int pivotIndex) {
            float tailWidth = childSizes[fragments[pivotIndex].layoutId].width / 2.0f;
            foreach (_TimePickerHeaderFragment fragment in fragments.Skip(pivotIndex + 1)) {
                tailWidth += childSizes[fragment.layoutId].width + fragment.startMargin;
            }

            float x = width / 2.0f + tailWidth;
            x = Mathf.Min(x, width);
            for (int i = fragments.Count - 1; i >= 0; i -= 1) {
                _TimePickerHeaderFragment fragment = fragments[i];
                Size childSize = childSizes[fragment.layoutId];
                x -= childSize.width;
                positionChild(fragment.layoutId, new Offset(x, y - childSize.height / 2.0f));
                x -= fragment.startMargin;
            }
        }

        void _positionPiece(float width, float centeredAroundY, Dictionary<_TimePickerHeaderId, Size> childSizes,
            List<_TimePickerHeaderFragment> fragments) {
            float pieceWidth = 0.0f;
            float nextMargin = 0.0f;
            foreach (_TimePickerHeaderFragment fragment in fragments) {
                Size childSize = childSizes[fragment.layoutId];
                pieceWidth += childSize.width + nextMargin;
                nextMargin = fragment.startMargin;
            }

            float x = (width + pieceWidth) / 2.0f;
            for (int i = fragments.Count - 1; i >= 0; i -= 1) {
                _TimePickerHeaderFragment fragment = fragments[i];
                Size childSize = childSizes[fragment.layoutId];
                x -= childSize.width;
                positionChild(fragment.layoutId, new Offset(x, centeredAroundY - childSize.height / 2.0f));
                x -= fragment.startMargin;
            }
        }

        public override bool shouldRelayout(MultiChildLayoutDelegate oldDelegate) {
            var _oldDelegate = (_TimePickerHeaderLayout) oldDelegate;
            return orientation != _oldDelegate.orientation || format != _oldDelegate.format;
        }
    }

    class _TimePickerHeader : StatelessWidget {
        public _TimePickerHeader(
            TimeOfDay selectedTime,
            _TimePickerMode mode,
            Orientation orientation,
            ValueChanged<_TimePickerMode> onModeChanged,
            ValueChanged<TimeOfDay> onChanged,
            bool use24HourDials
        ) {
            D.assert(selectedTime != null);
            this.selectedTime = selectedTime;
            this.mode = mode;
            this.orientation = orientation;
            this.onModeChanged = onModeChanged;
            this.onChanged = onChanged;
            this.use24HourDials = use24HourDials;
        }

        public readonly TimeOfDay selectedTime;
        public readonly _TimePickerMode mode;
        public readonly Orientation orientation;
        public readonly ValueChanged<_TimePickerMode> onModeChanged;
        public readonly ValueChanged<TimeOfDay> onChanged;
        public readonly bool use24HourDials;

        void _handleChangeMode(_TimePickerMode value) {
            if (value != mode) {
                onModeChanged(value);
            }
        }

        TextStyle _getBaseHeaderStyle(TextTheme headerTextTheme) {
            switch (orientation) {
                case Orientation.portrait:
                    return headerTextTheme.headline2.copyWith(fontSize: 60.0f);
                case Orientation.landscape:
                    return headerTextTheme.headline3.copyWith(fontSize: 50.0f);
            }

            return null;
        }

        public override Widget build(BuildContext context) {
            D.assert(WidgetsD.debugCheckHasMediaQuery(context));
            ThemeData themeData = Theme.of(context);
            MediaQueryData media = MediaQuery.of(context);
            TimeOfDayFormat timeOfDayFormat = MaterialLocalizations.of(context)
                .timeOfDayFormat(alwaysUse24HourFormat: media.alwaysUse24HourFormat);

            EdgeInsets padding = null;
            float height = 0f;
            float width = 0f;

            switch (orientation) {
                case Orientation.portrait:
                    height = TimePickerUtils._kTimePickerHeaderPortraitHeight;
                    padding = EdgeInsets.symmetric(horizontal: 24.0f);
                    break;
                case Orientation.landscape:
                    width = TimePickerUtils._kTimePickerHeaderLandscapeWidth;
                    padding = EdgeInsets.symmetric(horizontal: 16.0f);
                    break;
            }

            Color backgroundColor = null;
            switch (themeData.brightness) {
                case Brightness.light:
                    backgroundColor = themeData.primaryColor;
                    break;
                case Brightness.dark:
                    backgroundColor = themeData.backgroundColor;
                    break;
            }

            Color activeColor = null;
            Color inactiveColor = null;
            switch (themeData.primaryColorBrightness) {
                case Brightness.light:
                    activeColor = Colors.black87;
                    inactiveColor = Colors.black54;
                    break;
                case Brightness.dark:
                    activeColor = Colors.white;
                    inactiveColor = Colors.white70;
                    break;
            }

            TextTheme headerTextTheme = themeData.primaryTextTheme;
            TextStyle baseHeaderStyle = _getBaseHeaderStyle(headerTextTheme);
            _TimePickerFragmentContext fragmentContext = new _TimePickerFragmentContext(
                headerTextTheme: headerTextTheme,
                textDirection: Directionality.of(context),
                selectedTime: selectedTime,
                mode: mode,
                activeColor: activeColor,
                activeStyle: baseHeaderStyle.copyWith(color: activeColor),
                inactiveColor: inactiveColor,
                inactiveStyle: baseHeaderStyle.copyWith(color: inactiveColor),
                onTimeChange: onChanged,
                onModeChange: _handleChangeMode,
                targetPlatform: themeData.platform,
                use24HourDials: use24HourDials
            );
            _TimePickerHeaderFormat format =
                TimePickerUtils._buildHeaderFormat(timeOfDayFormat, fragmentContext, orientation);

            var childrenPieces = new List<_TimePickerHeaderFragment>();
            foreach (var piece in format.pieces) {
                childrenPieces.AddRange(piece.fragments);
            }

            var children = childrenPieces.Select<_TimePickerHeaderFragment, Widget>(
                (_TimePickerHeaderFragment fragment) => {
                    return new LayoutId(
                        id: fragment.layoutId,
                        child: fragment.widget
                    );
                });

            return new Container(
                width: width,
                height: height,
                padding: padding,
                color: backgroundColor,
                child: new CustomMultiChildLayout(
                    layoutDelegate: new _TimePickerHeaderLayout(orientation, format),
                    children: children.ToList()
                )
            );
        }
    }

    enum _DialRing {
        outer,
        inner,
    }

    class _TappableLabel {
        public _TappableLabel(
            int value,
            TextPainter painter,
            VoidCallback onTap
        ) {
            this.value = value;
            this.painter = painter;
            this.onTap = onTap;
        }

        public readonly int value;

        public readonly TextPainter painter;

        public readonly VoidCallback onTap;
    }

    class _DialPainter : AbstractCustomPainter {
        public _DialPainter(
            List<_TappableLabel> primaryOuterLabels,
            List<_TappableLabel> primaryInnerLabels,
            List<_TappableLabel> secondaryOuterLabels,
            List<_TappableLabel> secondaryInnerLabels,
            Color backgroundColor,
            Color accentColor,
            float theta,
            _DialRing activeRing,
            TextDirection textDirection,
            int selectedValue
        ) : base(repaint: PaintingBinding.instance.systemFonts) {
            this.primaryOuterLabels = primaryOuterLabels;
            this.primaryInnerLabels = primaryInnerLabels;
            this.secondaryOuterLabels = secondaryOuterLabels;
            this.secondaryInnerLabels = secondaryInnerLabels;
            this.backgroundColor = backgroundColor;
            this.accentColor = accentColor;
            this.theta = theta;
            this.activeRing = activeRing;
            this.textDirection = textDirection;
            this.selectedValue = selectedValue;
        }

        public readonly List<_TappableLabel> primaryOuterLabels;
        public readonly List<_TappableLabel> primaryInnerLabels;
        public readonly List<_TappableLabel> secondaryOuterLabels;
        public readonly List<_TappableLabel> secondaryInnerLabels;
        public readonly Color backgroundColor;
        public readonly Color accentColor;
        public readonly float theta;
        public readonly _DialRing activeRing;
        public readonly TextDirection textDirection;
        public readonly int selectedValue;

        public override void paint(Canvas canvas, Size size) {
            float radius = size.shortestSide / 2.0f;
            Offset center = new Offset(size.width / 2.0f, size.height / 2.0f);
            Offset centerPoint = center;
            canvas.drawCircle(centerPoint, radius, new Paint {color = backgroundColor});

            float labelPadding = 24.0f;
            float outerLabelRadius = radius - labelPadding;
            float innerLabelRadius = radius - labelPadding * 2.5f;

            Offset getOffsetForTheta(float theta, _DialRing ring) {
                float labelRadius = 0f;
                switch (ring) {
                    case _DialRing.outer:
                        labelRadius = outerLabelRadius;
                        break;
                    case _DialRing.inner:
                        labelRadius = innerLabelRadius;
                        break;
                }

                return center + new Offset(labelRadius * Mathf.Cos(theta),
                           -labelRadius * Mathf.Sin(theta));
            }

            void paintLabels(List<_TappableLabel> labels, _DialRing ring) {
                if (labels == null) {
                    return;
                }

                float labelThetaIncrement = -TimePickerUtils._kTwoPi / labels.Count;
                float labelTheta = Mathf.PI / 2.0f;

                foreach (_TappableLabel label in labels) {
                    TextPainter labelPainter = label.painter;
                    Offset labelOffset = new Offset(-labelPainter.width / 2.0f, -labelPainter.height / 2.0f);
                    labelPainter.paint(canvas, getOffsetForTheta(labelTheta, ring) + labelOffset);
                    labelTheta += labelThetaIncrement;
                }
            }

            paintLabels(primaryOuterLabels, _DialRing.outer);
            paintLabels(primaryInnerLabels, _DialRing.inner);

            Paint selectorPaint = new Paint {color = accentColor};
            Offset focusedPoint = getOffsetForTheta(theta, activeRing);
            float focusedRadius = labelPadding - 4.0f;
            canvas.drawCircle(centerPoint, 4.0f, selectorPaint);
            canvas.drawCircle(focusedPoint, focusedRadius, selectorPaint);
            selectorPaint.strokeWidth = 2.0f;
            canvas.drawLine(centerPoint, focusedPoint, selectorPaint);

            Rect focusedRect = Rect.fromCircle(
                center: focusedPoint, radius: focusedRadius
            );
            canvas.save();

            var path = new Path();
            path.addOval(focusedRect);
            canvas.clipPath(path);

            paintLabels(secondaryOuterLabels, _DialRing.outer);
            paintLabels(secondaryInnerLabels, _DialRing.inner);
            canvas.restore();
        }

        public override bool shouldRepaint(CustomPainter oldPainter) {
            var _oldPainter = (_DialPainter) oldPainter;
            return _oldPainter.primaryOuterLabels != primaryOuterLabels
                   || _oldPainter.primaryInnerLabels != primaryInnerLabels
                   || _oldPainter.secondaryOuterLabels != secondaryOuterLabels
                   || _oldPainter.secondaryInnerLabels != secondaryInnerLabels
                   || _oldPainter.backgroundColor != backgroundColor
                   || _oldPainter.accentColor != accentColor
                   || _oldPainter.theta != theta
                   || _oldPainter.activeRing != activeRing;
        }
    }

    class _Dial : StatefulWidget {
        public _Dial(
            TimeOfDay selectedTime,
            _TimePickerMode mode,
            bool use24HourDials,
            ValueChanged<TimeOfDay> onChanged,
            VoidCallback onHourSelected
        ) {
            D.assert(selectedTime != null);
            this.selectedTime = selectedTime;
            this.mode = mode;
            this.use24HourDials = use24HourDials;
            this.onChanged = onChanged;
            this.onHourSelected = onHourSelected;
        }

        public readonly TimeOfDay selectedTime;
        public readonly _TimePickerMode mode;
        public readonly bool use24HourDials;
        public readonly ValueChanged<TimeOfDay> onChanged;
        public readonly VoidCallback onHourSelected;

        public override State createState() {
            return new _DialState();
        }
    }

    class _DialState : SingleTickerProviderStateMixin<_Dial> {
        public override void initState() {
            base.initState();
            _updateDialRingFromWidget();
            _thetaController = new AnimationController(
                duration: TimePickerUtils._kDialAnimateDuration,
                vsync: this
            );

            //the end will be set to be valid value later, we just make it 1f by default here
            _thetaTween = new FloatTween(begin: _getThetaForTime(widget.selectedTime), end: 1f);
            _theta = _thetaController
                .drive(new CurveTween(curve: Curves.fastOutSlowIn))
                .drive(_thetaTween);
            _theta.addListener(() => setState(() => {
            }));
        }

        ThemeData themeData;
        MaterialLocalizations localizations;
        MediaQueryData media;

        public override void didChangeDependencies() {
            base.didChangeDependencies();
            D.assert(WidgetsD.debugCheckHasMediaQuery(context));
            themeData = Theme.of(context);
            localizations = MaterialLocalizations.of(context);
            media = MediaQuery.of(context);
        }

        public override void didUpdateWidget(StatefulWidget oldWidget) {
            var _oldWidget = (_Dial) oldWidget;
            base.didUpdateWidget(oldWidget);
            _updateDialRingFromWidget();
            if (widget.mode != _oldWidget.mode || widget.selectedTime != _oldWidget.selectedTime) {
                if (!_dragging) {
                    _animateTo(_getThetaForTime(widget.selectedTime));
                }
            }
        }

        void _updateDialRingFromWidget() {
            if (widget.mode == _TimePickerMode.hour && widget.use24HourDials) {
                _activeRing = widget.selectedTime.hour >= 1 && widget.selectedTime.hour <= 12
                    ? _DialRing.inner
                    : _DialRing.outer;
            }
            else {
                _activeRing = _DialRing.outer;
            }
        }

        public override void dispose() {
            _thetaController.dispose();
            base.dispose();
        }

        Tween<float> _thetaTween;
        Animation<float> _theta;
        AnimationController _thetaController;
        bool _dragging = false;

        static float _nearest(float target, float a, float b) {
            return ((target - a).abs() < (target - b).abs()) ? a : b;
        }

        void _animateTo(float targetTheta) {
            float currentTheta = _theta.value;
            float beginTheta = _nearest(targetTheta, currentTheta, currentTheta + TimePickerUtils._kTwoPi);
            beginTheta = _nearest(targetTheta, beginTheta, currentTheta - TimePickerUtils._kTwoPi);
            _thetaTween.begin = beginTheta;
            _thetaTween.end = targetTheta;
            _thetaController.setValue(0.0f);
            _thetaController.forward();
        }

        float _getThetaForTime(TimeOfDay time) {
            float fraction = widget.mode == _TimePickerMode.hour
                ? (time.hour / TimeOfDay.hoursPerPeriod) % TimeOfDay.hoursPerPeriod
                : (time.minute / TimeOfDay.minutesPerHour) % TimeOfDay.minutesPerHour;
            return (Mathf.PI / 2.0f - fraction * TimePickerUtils._kTwoPi) % TimePickerUtils._kTwoPi;
        }

        TimeOfDay _getTimeForTheta(float theta) {
            float fraction = (0.25f - (theta % TimePickerUtils._kTwoPi) / TimePickerUtils._kTwoPi) % 1.0f;
            if (widget.mode == _TimePickerMode.hour) {
                int newHour = (fraction * TimeOfDay.hoursPerPeriod).round() % TimeOfDay.hoursPerPeriod;
                if (widget.use24HourDials) {
                    if (_activeRing == _DialRing.outer) {
                        if (newHour != 0) {
                            newHour = (newHour + TimeOfDay.hoursPerPeriod) % TimeOfDay.hoursPerDay;
                        }
                    }
                    else if (newHour == 0) {
                        newHour = TimeOfDay.hoursPerPeriod;
                    }
                }
                else {
                    newHour = newHour + widget.selectedTime.periodOffset;
                }

                return widget.selectedTime.replacing(hour: newHour);
            }
            else {
                return widget.selectedTime.replacing(
                    minute: (fraction * TimeOfDay.minutesPerHour).round() % TimeOfDay.minutesPerHour
                );
            }
        }

        TimeOfDay _notifyOnChangedIfNeeded() {
            TimeOfDay current = _getTimeForTheta(_theta.value);
            if (widget.onChanged == null) {
                return current;
            }

            if (current != widget.selectedTime) {
                widget.onChanged(current);
            }

            return current;
        }

        void _updateThetaForPan() {
            setState(() => {
                Offset offset = _position - _center;
                float angle = (Mathf.Atan2(offset.dx, offset.dy) - Mathf.PI / 2.0f) % TimePickerUtils._kTwoPi;
                _thetaTween.begin = angle;
                _thetaTween.end = angle;
                RenderBox box = context.findRenderObject() as RenderBox;
                float radius = box.size.shortestSide / 2.0f;
                if (widget.mode == _TimePickerMode.hour && widget.use24HourDials) {
                    if (offset.distance * 1.5f < radius) {
                        _activeRing = _DialRing.inner;
                    }
                    else {
                        _activeRing = _DialRing.outer;
                    }
                }
            });
        }

        Offset _position;
        Offset _center;
        _DialRing _activeRing = _DialRing.outer;

        void _handlePanStart(DragStartDetails details) {
            D.assert(!_dragging);
            _dragging = true;
            RenderBox box = context.findRenderObject() as RenderBox;
            _position = box.globalToLocal(details.globalPosition);
            _center = box.size.center(Offset.zero);
            _updateThetaForPan();
            _notifyOnChangedIfNeeded();
        }

        void _handlePanUpdate(DragUpdateDetails details) {
            _position += details.delta;
            _updateThetaForPan();
            _notifyOnChangedIfNeeded();
        }

        void _handlePanEnd(DragEndDetails details) {
            D.assert(_dragging);
            _dragging = false;
            _position = null;
            _center = null;
            _animateTo(_getThetaForTime(widget.selectedTime));
            if (widget.mode == _TimePickerMode.hour) {
                if (widget.onHourSelected != null) {
                    widget.onHourSelected();
                }
            }
        }

        void _handleTapUp(TapUpDetails details) {
            RenderBox box = context.findRenderObject() as RenderBox;
            _position = box.globalToLocal(details.globalPosition);
            _center = box.size.center(Offset.zero);
            _updateThetaForPan();
            TimeOfDay newTime = _notifyOnChangedIfNeeded();
            if (widget.mode == _TimePickerMode.hour) {
                if (widget.onHourSelected != null) {
                    widget.onHourSelected();
                }
            }

            _animateTo(_getThetaForTime(_getTimeForTheta(_theta.value)));
            _dragging = false;
            _position = null;
            _center = null;
        }

        void _selectHour(int hour) {
            TimeOfDay time;
            if (widget.mode == _TimePickerMode.hour && widget.use24HourDials) {
                _activeRing = hour >= 1 && hour <= 12
                    ? _DialRing.inner
                    : _DialRing.outer;
                time = new TimeOfDay(hour: hour, minute: widget.selectedTime.minute);
            }
            else {
                _activeRing = _DialRing.outer;
                if (widget.selectedTime.period == DayPeriod.am) {
                    time = new TimeOfDay(hour: hour, minute: widget.selectedTime.minute);
                }
                else {
                    time = new TimeOfDay(hour: hour + TimeOfDay.hoursPerPeriod, minute: widget.selectedTime.minute);
                }
            }

            float angle = _getThetaForTime(time);
            _thetaTween.begin = angle;
            _thetaTween.end = angle;
            _notifyOnChangedIfNeeded();
        }

        void _selectMinute(int minute) {
            TimeOfDay time = new TimeOfDay(
                hour: widget.selectedTime.hour,
                minute: minute
            );
            float angle = _getThetaForTime(time);
            _thetaTween.begin = angle;
            _thetaTween.end = angle;
            _notifyOnChangedIfNeeded();
        }

        static readonly List<TimeOfDay> _amHours = new List<TimeOfDay> {
            new TimeOfDay(hour: 12, minute: 0),
            new TimeOfDay(hour: 1, minute: 0),
            new TimeOfDay(hour: 2, minute: 0),
            new TimeOfDay(hour: 3, minute: 0),
            new TimeOfDay(hour: 4, minute: 0),
            new TimeOfDay(hour: 5, minute: 0),
            new TimeOfDay(hour: 6, minute: 0),
            new TimeOfDay(hour: 7, minute: 0),
            new TimeOfDay(hour: 8, minute: 0),
            new TimeOfDay(hour: 9, minute: 0),
            new TimeOfDay(hour: 10, minute: 0),
            new TimeOfDay(hour: 11, minute: 0)
        };

        static readonly List<TimeOfDay> _pmHours = new List<TimeOfDay> {
            new TimeOfDay(hour: 0, minute: 0),
            new TimeOfDay(hour: 13, minute: 0),
            new TimeOfDay(hour: 14, minute: 0),
            new TimeOfDay(hour: 15, minute: 0),
            new TimeOfDay(hour: 16, minute: 0),
            new TimeOfDay(hour: 17, minute: 0),
            new TimeOfDay(hour: 18, minute: 0),
            new TimeOfDay(hour: 19, minute: 0),
            new TimeOfDay(hour: 20, minute: 0),
            new TimeOfDay(hour: 21, minute: 0),
            new TimeOfDay(hour: 22, minute: 0),
            new TimeOfDay(hour: 23, minute: 0)
        };

        _TappableLabel _buildTappableLabel(TextTheme textTheme, int value, string label, VoidCallback onTap) {
            TextStyle style = textTheme.subtitle1;
            float labelScaleFactor = Mathf.Min(MediaQuery.of(context).textScaleFactor, 2.0f);

            var painter = new TextPainter(
                text: new TextSpan(style: style, text: label),
                textDirection: TextDirection.ltr,
                textScaleFactor: labelScaleFactor
            );

            painter.layout();

            return new _TappableLabel(
                value: value,
                painter: painter,
                onTap: onTap
            );
        }

        List<_TappableLabel> _build24HourInnerRing(TextTheme textTheme) {
            var labels = new List<_TappableLabel>();
            foreach (TimeOfDay timeOfDay in _amHours) {
                labels.Add(_buildTappableLabel(
                    textTheme,
                    timeOfDay.hour,
                    localizations.formatHour(timeOfDay, alwaysUse24HourFormat: media.alwaysUse24HourFormat),
                    () => { _selectHour(timeOfDay.hour); }
                ));
            }

            return labels;
        }

        List<_TappableLabel> _build24HourOuterRing(TextTheme textTheme) {
            var labels = new List<_TappableLabel>();
            foreach (TimeOfDay timeOfDay in _pmHours) {
                labels.Add(_buildTappableLabel(
                    textTheme,
                    timeOfDay.hour,
                    localizations.formatHour(timeOfDay, alwaysUse24HourFormat: media.alwaysUse24HourFormat),
                    () => { _selectHour(timeOfDay.hour); }
                ));
            }

            return labels;
        }

        List<_TappableLabel> _build12HourOuterRing(TextTheme textTheme) {
            var labels = new List<_TappableLabel>();
            foreach (TimeOfDay timeOfDay in _amHours) {
                labels.Add(_buildTappableLabel(
                    textTheme,
                    timeOfDay.hour,
                    localizations.formatHour(timeOfDay, alwaysUse24HourFormat: media.alwaysUse24HourFormat),
                    () => { _selectHour(timeOfDay.hour); }
                ));
            }

            return labels;
        }

        static readonly List<TimeOfDay> _minuteMarkerValues = new List<TimeOfDay> {
            new TimeOfDay(hour: 0, minute: 0),
            new TimeOfDay(hour: 0, minute: 5),
            new TimeOfDay(hour: 0, minute: 10),
            new TimeOfDay(hour: 0, minute: 15),
            new TimeOfDay(hour: 0, minute: 20),
            new TimeOfDay(hour: 0, minute: 25),
            new TimeOfDay(hour: 0, minute: 30),
            new TimeOfDay(hour: 0, minute: 35),
            new TimeOfDay(hour: 0, minute: 40),
            new TimeOfDay(hour: 0, minute: 45),
            new TimeOfDay(hour: 0, minute: 50),
            new TimeOfDay(hour: 0, minute: 55)
        };

        List<_TappableLabel> _buildMinutes(TextTheme textTheme) {
            var labels = new List<_TappableLabel>();

            foreach (TimeOfDay timeOfDay in _minuteMarkerValues) {
                labels.Add(_buildTappableLabel(
                    textTheme,
                    timeOfDay.minute,
                    localizations.formatMinute(timeOfDay),
                    () => { _selectMinute(timeOfDay.minute); }
                ));
            }

            return labels;
        }

        public override Widget build(BuildContext context) {
            Color backgroundColor = null;
            switch (themeData.brightness) {
                case Brightness.light:
                    backgroundColor = Colors.grey[200];
                    break;
                case Brightness.dark:
                    backgroundColor = themeData.backgroundColor;
                    break;
            }

            ThemeData theme = Theme.of(context);
            List<_TappableLabel> primaryOuterLabels = null;
            List<_TappableLabel> primaryInnerLabels = null;
            List<_TappableLabel> secondaryOuterLabels = null;
            List<_TappableLabel> secondaryInnerLabels = null;
            int selectedDialValue = 0;
            switch (widget.mode) {
                case _TimePickerMode.hour:
                    if (widget.use24HourDials) {
                        selectedDialValue = widget.selectedTime.hour;
                        primaryOuterLabels = _build24HourOuterRing(theme.textTheme);
                        secondaryOuterLabels = _build24HourOuterRing(theme.accentTextTheme);
                        primaryInnerLabels = _build24HourInnerRing(theme.textTheme);
                        secondaryInnerLabels = _build24HourInnerRing(theme.accentTextTheme);
                    }
                    else {
                        selectedDialValue = widget.selectedTime.hourOfPeriod;
                        primaryOuterLabels = _build12HourOuterRing(theme.textTheme);
                        secondaryOuterLabels = _build12HourOuterRing(theme.accentTextTheme);
                    }

                    break;
                case _TimePickerMode.minute:
                    selectedDialValue = widget.selectedTime.minute;
                    primaryOuterLabels = _buildMinutes(theme.textTheme);
                    primaryInnerLabels = null;
                    secondaryOuterLabels = _buildMinutes(theme.accentTextTheme);
                    secondaryInnerLabels = null;
                    break;
            }

            return new GestureDetector(
                onPanStart: _handlePanStart,
                onPanUpdate: _handlePanUpdate,
                onPanEnd: _handlePanEnd,
                onTapUp: _handleTapUp,
                child: new CustomPaint(
                    key: new ValueKey<string>("time-picker-dial"),
                    painter: new _DialPainter(
                        selectedValue: selectedDialValue,
                        primaryOuterLabels: primaryOuterLabels,
                        primaryInnerLabels: primaryInnerLabels,
                        secondaryOuterLabels: secondaryOuterLabels,
                        secondaryInnerLabels: secondaryInnerLabels,
                        backgroundColor: backgroundColor,
                        accentColor: themeData.accentColor,
                        theta: _theta.value,
                        activeRing: _activeRing,
                        textDirection: Directionality.of(context)
                    )
                )
            );
        }
    }

    class _TimePickerDialog : StatefulWidget {
        public _TimePickerDialog(
            Key key = null,
            TimeOfDay initialTime = null) : base(key: key) {
            D.assert(initialTime != null);
            this.initialTime = initialTime;
        }

        public readonly TimeOfDay initialTime;

        public override State createState() {
            return new _TimePickerDialogState();
        }
    }

    class _TimePickerDialogState : State<_TimePickerDialog> {
        public override void initState() {
            base.initState();
            _selectedTime = widget.initialTime;
        }

        public override void didChangeDependencies() {
            base.didChangeDependencies();
            localizations = MaterialLocalizations.of(context);
            _announceInitialTimeOnce();
            _announceModeOnce();
        }

        _TimePickerMode _mode = _TimePickerMode.hour;
        _TimePickerMode _lastModeAnnounced;

        TimeOfDay selectedTime {
            get { return _selectedTime; }
        }

        TimeOfDay _selectedTime;

        Timer _vibrateTimer;
        MaterialLocalizations localizations;

        void _vibrate() {
        }

        void _handleModeChanged(_TimePickerMode mode) {
            _vibrate();
            setState(() => {
                _mode = mode;
                _announceModeOnce();
            });
        }

        void _announceModeOnce() {
            if (_lastModeAnnounced == _mode) {
                return;
            }

            _lastModeAnnounced = _mode;
        }

        bool _announcedInitialTime = false;

        void _announceInitialTimeOnce() {
            if (_announcedInitialTime) {
                return;
            }

            MediaQueryData media = MediaQuery.of(context);
            MaterialLocalizations localizations = MaterialLocalizations.of(context);
            _announcedInitialTime = true;
        }

        void _handleTimeChanged(TimeOfDay value) {
            _vibrate();
            setState(() => { _selectedTime = value; });
        }

        void _handleHourSelected() {
            setState(() => { _mode = _TimePickerMode.minute; });
        }

        void _handleCancel() {
            Navigator.pop<object>(context);
        }

        void _handleOk() {
            Navigator.pop(context, _selectedTime);
        }

        public override Widget build(BuildContext context) {
            D.assert(WidgetsD.debugCheckHasMediaQuery(context));
            MediaQueryData media = MediaQuery.of(context);
            TimeOfDayFormat timeOfDayFormat =
                localizations.timeOfDayFormat(alwaysUse24HourFormat: media.alwaysUse24HourFormat);
            bool use24HourDials = HourFormatUtils.hourFormat(of: timeOfDayFormat) != HourFormat.h;
            ThemeData theme = Theme.of(context);

            Widget picker = new Padding(
                padding: EdgeInsets.all(16.0f),
                child: new AspectRatio(
                    aspectRatio: 1.0f,
                    child: new _Dial(
                        mode: _mode,
                        use24HourDials: use24HourDials,
                        selectedTime: _selectedTime,
                        onChanged: _handleTimeChanged,
                        onHourSelected: _handleHourSelected
                    )
                )
            );
            Widget actions = new ButtonBar(
                children: new List<Widget> {
                    new FlatButton(
                        child: new Text(localizations.cancelButtonLabel),
                        onPressed: _handleCancel
                    ),
                    new FlatButton(
                        child: new Text(localizations.okButtonLabel),
                        onPressed: _handleOk
                    ),
                }
            );

            Dialog dialog = new Dialog(
                child: new OrientationBuilder(
                    builder: (BuildContext subContext, Orientation subOrientation) => {
                        Widget header = new _TimePickerHeader(
                            selectedTime: _selectedTime,
                            mode: _mode,
                            orientation: subOrientation,
                            onModeChanged: _handleModeChanged,
                            onChanged: _handleTimeChanged,
                            use24HourDials: use24HourDials
                        );

                        Widget pickerAndActions = new Container(
                            color: theme.dialogBackgroundColor,
                            child: new Column(
                                mainAxisSize: MainAxisSize.min,
                                children: new List<Widget> {
                                    new Expanded(child: picker), // picker grows and shrinks with the available space
                                    actions,
                                }
                            )
                        );

                        float timePickerHeightPortrait = 0f;
                        float timePickerHeightLandscape = 0f;
                        switch (theme.materialTapTargetSize) {
                            case MaterialTapTargetSize.padded:
                                timePickerHeightPortrait = TimePickerUtils._kTimePickerHeightPortrait;
                                timePickerHeightLandscape = TimePickerUtils._kTimePickerHeightLandscape;
                                break;
                            case MaterialTapTargetSize.shrinkWrap:
                                timePickerHeightPortrait = TimePickerUtils._kTimePickerHeightPortraitCollapsed;
                                timePickerHeightLandscape = TimePickerUtils._kTimePickerHeightLandscapeCollapsed;
                                break;
                        }

                        switch (subOrientation) {
                            case Orientation.portrait:
                                return new SizedBox(
                                    width: TimePickerUtils._kTimePickerWidthPortrait,
                                    height: timePickerHeightPortrait,
                                    child: new Column(
                                        mainAxisSize: MainAxisSize.min,
                                        crossAxisAlignment: CrossAxisAlignment.stretch,
                                        children: new List<Widget> {
                                            header,
                                            new Expanded(
                                                child: pickerAndActions
                                            ),
                                        }
                                    )
                                );
                            case Orientation.landscape:
                                return new SizedBox(
                                    width: TimePickerUtils._kTimePickerWidthLandscape,
                                    height: timePickerHeightLandscape,
                                    child: new Row(
                                        mainAxisSize: MainAxisSize.min,
                                        crossAxisAlignment: CrossAxisAlignment.stretch,
                                        children: new List<Widget> {
                                            header,
                                            new Flexible(
                                                child: pickerAndActions
                                            ),
                                        }
                                    )
                                );
                        }

                        return null;
                    }
                )
            );

            return new Theme(
                data: theme.copyWith(
                    dialogBackgroundColor: Colors.transparent
                ),
                child: dialog
            );
        }

        public override void dispose() {
            _vibrateTimer?.cancel();
            _vibrateTimer = null;
            base.dispose();
        }
    }
}