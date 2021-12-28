using System;
using System.Collections.Generic;
using Unity.UIWidgets.animation;
using Unity.UIWidgets.async;
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
        public static readonly Size _calendarPortraitDialogSize = new Size(330.0f, 518.0f);
        public static readonly Size _calendarLandscapeDialogSize = new Size(496.0f, 346.0f);
        public static readonly Size _inputPortraitDialogSize = new Size(330.0f, 270.0f);
        public static readonly Size _inputLandscapeDialogSize = new Size(496f, 160.0f);
        public static readonly TimeSpan _dialogSizeAnimationDuration = new TimeSpan(0, 0, 0, 0, 200);

        public static Future<DateTime> showDatePicker(
            BuildContext context,
            DateTime initialDate,
            DateTime firstDate,
            DateTime lastDate,
            DatePickerEntryMode initialEntryMode = DatePickerEntryMode.calendar,
            material_.SelectableDayPredicate selectableDayPredicate = null,
            string helpText = null,
            string cancelText = null,
            string confirmText = null,
            Locale locale = null,
            bool useRootNavigator = true,
            RouteSettings routeSettings = null,
            TextDirection? textDirection = null,
            TransitionBuilder builder = null,
            DatePickerMode initialDatePickerMode = DatePickerMode.day,
            string errorFormatText = null,
            string errorInvalidText = null,
            string fieldHintText = null,
            string fieldLabelText = null
        ) {
            D.assert(context != null);
            initialDate = utils.dateOnly(initialDate);
            firstDate = utils.dateOnly(firstDate);
            lastDate = utils.dateOnly(lastDate);

            D.assert(
                !lastDate.isBefore(firstDate),
                () => $"lastDate {lastDate} must be on or after firstDate {firstDate}."
            );
            D.assert(
                !initialDate.isBefore(firstDate),
                () => $"initialDate {initialDate} must be on or after firstDate {firstDate}."
            );
            D.assert(
                !initialDate.isAfter(lastDate),
                () => $"initialDate {initialDate} must be on or before lastDate {lastDate}."
            );
            D.assert(
                selectableDayPredicate == null || selectableDayPredicate(initialDate),
                () => $"Provided initialDate {initialDate} must satisfy provided selectableDayPredicate."
            );
            
            D.assert(material_.debugCheckHasMaterialLocalizations(context));

            Widget dialog = new _DatePickerDialog(
                initialDate: initialDate,
                firstDate: firstDate,
                lastDate: lastDate,
                initialEntryMode: initialEntryMode,
                selectableDayPredicate: selectableDayPredicate,
                helpText: helpText,
                cancelText: cancelText,
                confirmText: confirmText,
                initialCalendarMode: initialDatePickerMode,
                errorFormatText: errorFormatText,
                errorInvalidText: errorInvalidText,
                fieldHintText: fieldHintText,
                fieldLabelText: fieldLabelText
            );

            if (textDirection != null) {
                dialog = new Directionality(
                    textDirection: textDirection.Value,
                    child: dialog
                );
            }

            if (locale != null) {
                dialog = Localizations.overrides(
                    context: context,
                    locale: locale,
                    child: dialog
                );
            }

            return material_.showDialog<DateTime>(
                context: context,
                useRootNavigator: useRootNavigator,
                routeSettings: routeSettings,
                builder: (BuildContext subContext) => { return builder == null ? dialog : builder(subContext, dialog); }
            );
        }
    }

    class _DatePickerDialog : StatefulWidget {
        public _DatePickerDialog(
            Key key = null,
            DateTime? initialDate = null,
            DateTime? firstDate = null,
            DateTime? lastDate = null,
            DatePickerEntryMode initialEntryMode = DatePickerEntryMode.calendar,
            material_.SelectableDayPredicate selectableDayPredicate = null,
            string cancelText = null,
            string confirmText = null,
            string helpText = null,
            DatePickerMode initialCalendarMode = DatePickerMode.day,
            string errorFormatText = null,
            string errorInvalidText = null,
            string fieldHintText = null,
            string fieldLabelText = null
        ) : base(key: key) {
            D.assert(initialDate != null);
            D.assert(firstDate != null);
            D.assert(lastDate != null);

            initialDate = utils.dateOnly(initialDate.Value);
            firstDate = utils.dateOnly(firstDate.Value);
            lastDate = utils.dateOnly(lastDate.Value);

            D.assert(
                !lastDate.Value.isBefore(firstDate.Value),
                () => $"lastDate {lastDate} must be on or after firstDate {firstDate}."
            );
            D.assert(
                initialDate == null || !initialDate.Value.isBefore(firstDate.Value),
                () => $"initialDate {initialDate} must be on or after firstDate {firstDate}."
            );
            D.assert(
                initialDate == null || !initialDate.Value.isAfter(lastDate.Value),
                () => $"initialDate {initialDate} must be on or before lastDate {lastDate}."
            );
            D.assert(
                selectableDayPredicate == null || initialDate == null || selectableDayPredicate(initialDate.Value),
                () => $"Provided initialDate {initialDate} must satisfy provided selectableDayPredicate."
            );

            this.initialDate = initialDate.Value;
            this.firstDate = firstDate.Value;
            this.lastDate = lastDate.Value;
            this.initialEntryMode = initialEntryMode;
            this.selectableDayPredicate = selectableDayPredicate;
            this.cancelText = cancelText;
            this.confirmText = confirmText;
            this.helpText = helpText;
            this.initialCalendarMode = initialCalendarMode;
            this.errorFormatText = errorFormatText;
            this.errorInvalidText = errorInvalidText;
            this.fieldHintText = fieldHintText;
            this.fieldLabelText = fieldLabelText;
        }

        public readonly DateTime initialDate;

        public readonly DateTime firstDate;

        public readonly DateTime lastDate;

        public readonly DatePickerEntryMode initialEntryMode;

        public readonly material_.SelectableDayPredicate selectableDayPredicate;

        public readonly string cancelText;

        public readonly string confirmText;

        public readonly string helpText;

        public readonly DatePickerMode initialCalendarMode;

        public readonly string errorFormatText;

        public readonly string errorInvalidText;

        public readonly string fieldHintText;

        public readonly string fieldLabelText;

        public override State createState() {
            return new _DatePickerDialogState();
        }
    }

    class _DatePickerDialogState : State<_DatePickerDialog> {
        DatePickerEntryMode _entryMode;
        DateTime _selectedDate;
        bool _autoValidate;
        public readonly GlobalKey _calendarPickerKey = GlobalKey.key();
        public readonly GlobalKey<FormState> _formKey = GlobalKey<FormState>.key();

        public override void initState() {
            base.initState();
            _entryMode = widget.initialEntryMode;
            _selectedDate = widget.initialDate;
            _autoValidate = false;
        }

        void _handleOk() {
            if (_entryMode == DatePickerEntryMode.input) {
                FormState form = _formKey.currentState;
                if (!form.validate()) {
                    setState(() => _autoValidate = true);
                    return;
                }

                form.save();
            }

            Navigator.pop(context, _selectedDate);
        }

        void _handleCancel() {
            Navigator.pop<object>(context);
        }

        void _handelEntryModeToggle() {
            setState(() => {
                switch (_entryMode) {
                    case DatePickerEntryMode.calendar:
                        _autoValidate = false;
                        _entryMode = DatePickerEntryMode.input;
                        break;
                    case DatePickerEntryMode.input:
                        _formKey.currentState.save();
                        _entryMode = DatePickerEntryMode.calendar;
                        break;
                }
            });
        }

        void _handleDateChanged(DateTime date) {
            setState(() => _selectedDate = date);
        }

        Size _dialogSize(BuildContext context) {
            Orientation? orientation = MediaQuery.of(context).orientation;
            switch (_entryMode) {
                case DatePickerEntryMode.calendar:
                    switch (orientation) {
                        case Orientation.portrait:
                            return material_._calendarPortraitDialogSize;
                        case Orientation.landscape:
                            return material_._calendarLandscapeDialogSize;
                    }

                    break;
                case DatePickerEntryMode.input:
                    switch (orientation) {
                        case Orientation.portrait:
                            return material_._inputPortraitDialogSize;
                        case Orientation.landscape:
                            return material_._inputLandscapeDialogSize;
                    }

                    break;
            }

            return null;
        }

        public override Widget build(BuildContext context) {
            ThemeData theme = Theme.of(context);
            ColorScheme colorScheme = theme.colorScheme;
            MaterialLocalizations localizations = MaterialLocalizations.of(context);
            Orientation? orientation = MediaQuery.of(context).orientation;
            TextTheme textTheme = theme.textTheme;

            float textScaleFactor = Mathf.Min(MediaQuery.of(context).textScaleFactor, 1.3f);

            string dateText = _selectedDate != null
                ? localizations.formatMediumDate(_selectedDate)
                : "Date";
            Color dateColor = colorScheme.brightness == Brightness.light
                ? colorScheme.onPrimary
                : colorScheme.onSurface;
            TextStyle dateStyle = orientation == Orientation.landscape
                ? textTheme.headline5?.copyWith(color: dateColor)
                : textTheme.headline4?.copyWith(color: dateColor);

            Widget actions = new ButtonBar(
                buttonTextTheme: ButtonTextTheme.primary,
                layoutBehavior: ButtonBarLayoutBehavior.constrained,
                children: new List<Widget> {
                    new FlatButton(
                        child: new Text(widget.cancelText ?? localizations.cancelButtonLabel),
                        onPressed: _handleCancel
                    ),
                    new FlatButton(
                        child: new Text(widget.confirmText ?? localizations.okButtonLabel),
                        onPressed: _handleOk
                    ),
                }
            );

            Widget picker = null;
            IconData entryModeIcon = null;
            string entryModeTooltip = null;
            switch (_entryMode) {
                case DatePickerEntryMode.calendar:
                    picker = new CalendarDatePicker(
                        key: _calendarPickerKey,
                        initialDate: _selectedDate,
                        firstDate: widget.firstDate,
                        lastDate: widget.lastDate,
                        onDateChanged: _handleDateChanged,
                        selectableDayPredicate: widget.selectableDayPredicate,
                        initialCalendarMode: widget.initialCalendarMode
                    );
                    entryModeIcon = Icons.edit;
                    entryModeTooltip = "Switch to input";
                    break;

                case DatePickerEntryMode.input:
                    picker = new Form(
                        key: _formKey,
                        autovalidate: _autoValidate,
                        child: new InputDatePickerFormField(
                            initialDate: _selectedDate,
                            firstDate: widget.firstDate,
                            lastDate: widget.lastDate,
                            onDateSubmitted: _handleDateChanged,
                            onDateSaved: _handleDateChanged,
                            selectableDayPredicate: widget.selectableDayPredicate,
                            errorFormatText: widget.errorFormatText,
                            errorInvalidText: widget.errorInvalidText,
                            fieldHintText: widget.fieldHintText,
                            fieldLabelText: widget.fieldLabelText,
                            autofocus: true
                        )
                    );
                    entryModeIcon = Icons.calendar_today;
                    entryModeTooltip = "Switch to calendar";
                    break;
            }

            Widget header = new DatePickerHeader(
                helpText: widget.helpText ?? "SELECT DATE",
                titleText: dateText,
                titleStyle: dateStyle,
                orientation: orientation,
                isShort: orientation == Orientation.landscape,
                icon: entryModeIcon,
                iconTooltip: entryModeTooltip,
                onIconPressed: _handelEntryModeToggle
            );

            Size dialogSize = _dialogSize(context) * textScaleFactor;
            DialogTheme dialogTheme = Theme.of(context).dialogTheme;
            return new Dialog(
                child: new AnimatedContainer(
                    width: dialogSize.width,
                    height: dialogSize.height,
                    duration: material_._dialogSizeAnimationDuration,
                    curve: Curves.easeIn,
                    child: new MediaQuery(
                        data: MediaQuery.of(context).copyWith(
                            textScaleFactor: textScaleFactor
                        ),
                        child: new Builder(builder: (BuildContext subContext) => {
                            switch (orientation) {
                                case Orientation.portrait:
                                    return new Column(
                                        mainAxisSize: MainAxisSize.min,
                                        crossAxisAlignment: CrossAxisAlignment.stretch,
                                        children: new List<Widget> {
                                            header,
                                            new Expanded(child: picker),
                                            actions,
                                        }
                                    );
                                case Orientation.landscape:
                                    return new Row(
                                        mainAxisSize: MainAxisSize.min,
                                        crossAxisAlignment: CrossAxisAlignment.stretch,
                                        children: new List<Widget> {
                                            header,
                                            new Flexible(
                                                child: new Column(
                                                    mainAxisSize: MainAxisSize.min,
                                                    crossAxisAlignment: CrossAxisAlignment.stretch,
                                                    children: new List<Widget> {
                                                        new Expanded(child: picker),
                                                        actions,
                                                    }
                                                )
                                            ),
                                        }
                                    );
                            }

                            return null;
                        })
                    )
                ),
                insetPadding: EdgeInsets.symmetric(horizontal: 16.0f, vertical: 24.0f),
                shape: dialogTheme.shape ?? new RoundedRectangleBorder(
                           borderRadius: BorderRadius.all(Radius.circular(4.0f))
                       ),
                clipBehavior: Clip.antiAlias
            );
        }
    }
}