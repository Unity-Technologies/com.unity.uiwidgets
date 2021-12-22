using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UIWidgets.Runtime.material;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.service;
using Unity.UIWidgets.widgets;

namespace Unity.UIWidgets.material {
    public static class InputDatePickerUtils {
        public const float _inputPortraitHeight = 98.0f;
        public const float _inputLandscapeHeight = 108.0f;

        public static bool isBefore(this DateTime t1, DateTime t2) {
            return t1.CompareTo(t2) < 0;
        }

        public static bool isAfter(this DateTime t1, DateTime t2) {
            return t1.CompareTo(t2) > 0;
        }
    }

    class InputDatePickerFormField : StatefulWidget {
        public InputDatePickerFormField(
            Key key = null,
            DateTime? initialDate = null,
            DateTime? firstDate = null,
            DateTime? lastDate = null,
            ValueChanged<DateTime> onDateSubmitted = null,
            ValueChanged<DateTime> onDateSaved = null,
            material_.SelectableDayPredicate selectableDayPredicate = null,
            string errorFormatText = null,
            string errorInvalidText = null,
            string fieldHintText = null,
            string fieldLabelText = null,
            bool autofocus = false
        ) : base(key: key) {
            D.assert(firstDate != null);
            D.assert(lastDate != null);

            initialDate = initialDate != null ? utils.dateOnly(initialDate.Value) : (DateTime?)null;
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

            this.initialDate = initialDate;
            this.firstDate = firstDate.Value;
            this.lastDate = lastDate.Value;
            this.onDateSubmitted = onDateSubmitted;
            this.onDateSaved = onDateSaved;
            this.selectableDayPredicate = selectableDayPredicate;
            this.errorFormatText = errorFormatText;
            this.errorInvalidText = errorInvalidText;
            this.fieldHintText = fieldHintText;
            this.fieldLabelText = fieldLabelText;
            this.autofocus = autofocus;
        }

        public readonly DateTime? initialDate;

        public readonly DateTime firstDate;

        public readonly DateTime lastDate;

        public readonly ValueChanged<DateTime> onDateSubmitted;

        public readonly ValueChanged<DateTime> onDateSaved;

        public readonly material_.SelectableDayPredicate selectableDayPredicate;

        public readonly string errorFormatText;

        public readonly string errorInvalidText;

        public readonly string fieldHintText;

        public readonly string fieldLabelText;

        public readonly bool autofocus;

        public override State createState() {
            return new _InputDatePickerFormFieldState();
        }
    }

    class _InputDatePickerFormFieldState : State<InputDatePickerFormField> {
        public readonly TextEditingController _controller = new TextEditingController();
        DateTime? _selectedDate;
        string _inputText;
        bool _autoSelected = false;

        public override void initState() {
            base.initState();
            _selectedDate = widget.initialDate;
        }

        public override void dispose() {
            _controller.dispose();
            base.dispose();
        }

        public override void didChangeDependencies() {
            base.didChangeDependencies();
            if (_selectedDate != null) {
                MaterialLocalizations localizations = MaterialLocalizations.of(context);
                _inputText = localizations.formatCompactDate(_selectedDate.Value);
                TextEditingValue textEditingValue = _controller.value.copyWith(text: _inputText);

                if (widget.autofocus && !_autoSelected) {
                    textEditingValue = textEditingValue.copyWith(selection: new TextSelection(
                        baseOffset: 0,
                        extentOffset: _inputText.Length
                    ));
                    _autoSelected = true;
                }

                _controller.value = textEditingValue;
            }
        }

        DateTime? _parseDate(string text) {
            MaterialLocalizations localizations = MaterialLocalizations.of(context);
            return localizations.parseCompactDate(text);
        }

        bool _isValidAcceptableDate(DateTime? date) {
            return
                date != null &&
                !date.Value.isBefore(widget.firstDate) &&
                !date.Value.isAfter(widget.lastDate) &&
                (widget.selectableDayPredicate == null || widget.selectableDayPredicate(date));
        }

        string _validateDate(string text) {
            DateTime? date = _parseDate(text);
            if (date == null) {
                return widget.errorFormatText ?? "Invalid format.";
            }
            else if (!_isValidAcceptableDate(date)) {
                return widget.errorInvalidText ?? "Out of range.";
            }

            return null;
        }

        void _handleSaved(string text) {
            if (widget.onDateSaved != null) {
                DateTime? date = _parseDate(text);
                if (_isValidAcceptableDate(date)) {
                    _selectedDate = date;
                    _inputText = text;
                    widget.onDateSaved(date.Value);
                }
            }
        }

        void _handleSubmitted(string text) {
            if (widget.onDateSubmitted != null) {
                DateTime? date = _parseDate(text);
                if (_isValidAcceptableDate(date)) {
                    _selectedDate = date;
                    _inputText = text;
                    widget.onDateSubmitted(date.Value);
                }
            }
        }

        public override Widget build(BuildContext context) {
            return new OrientationBuilder(builder: (BuildContext subContext, Orientation orientation) => {
                return new Container(
                    padding: EdgeInsets.symmetric(horizontal: 24f),
                    height: orientation == Orientation.portrait
                        ? InputDatePickerUtils._inputPortraitHeight
                        : InputDatePickerUtils._inputLandscapeHeight,
                    child: new Column(
                        children: new List<Widget> {
                            new Spacer(),
                            new TextFormField(
                                decoration: new InputDecoration(
                                    border: new UnderlineInputBorder(),
                                    filled: true,
                                    hintText: widget.fieldHintText ?? "mm/dd/yyyy",
                                    labelText: widget.fieldLabelText ?? "Enter Date"
                                ),
                                validator: _validateDate,
                                inputFormatters: new List<TextInputFormatter> {
                                    new _DateTextInputFormatter("/")
                                },
                                keyboardType: TextInputType.datetime,
                                onSaved: _handleSaved,
                                onFieldSubmitted: _handleSubmitted,
                                autofocus: widget.autofocus,
                                controller: _controller
                            ),
                            new Spacer(),
                        }
                    )
                );
            });
        }
    }

    class _DateTextInputFormatter : TextInputFormatter {
        public _DateTextInputFormatter(string separator) {
            this.separator = separator;
        }

        public readonly string separator;

        public readonly WhitelistingTextInputFormatter _filterFormatter =
            // Only allow digits and separators (slash, dot, comma, hyphen, space).
            new WhitelistingTextInputFormatter(new Regex(@"[\d\/\.,\- ]+"));

        public override TextEditingValue formatEditUpdate(TextEditingValue oldValue, TextEditingValue newValue) {
            TextEditingValue filteredValue = _filterFormatter.formatEditUpdate(oldValue, newValue);
            return filteredValue.copyWith(
                text: Regex.Replace(filteredValue.text, @"[\D]", separator)
            );
        }
    }
}