using System.Collections.Generic;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.ui;

namespace Unity.UIWidgets.widgets {
    public class Form : StatefulWidget {
        public Form(
            Key key = null,
            Widget child = null,
            bool autovalidate = false,
            WillPopCallback onWillPop = null,
            VoidCallback onChanged = null
        ) : base(key: key) {
            D.assert(child != null);
            this.child = child;
            this.autovalidate = autovalidate;
            this.onWillPop = onWillPop;
            this.onChanged = onChanged;
        }

        public static FormState of(BuildContext context) {
            _FormScope scope = context.dependOnInheritedWidgetOfExactType<_FormScope>();
            return scope?._formState;
        }

        public readonly Widget child;

        public readonly bool autovalidate;

        public readonly WillPopCallback onWillPop;

        public readonly VoidCallback onChanged;

        public override State createState() {
            return new FormState();
        }
    }

    public class FormState : State<Form> {
        int _generation = 0;
        readonly HashSet<FormFieldState> _fields = new HashSet<FormFieldState>();

        public FormState() {
        }

        public void _fieldDidChange() {
            if (widget.onChanged != null) {
                widget.onChanged();
            }

            _forceRebuild();
        }

        void _forceRebuild() {
            setState(() => { ++_generation; });
        }

        public void _register(FormFieldState field) {
            _fields.Add(field);
        }

        public void _unregister(FormFieldState field) {
            _fields.Remove(field);
        }

        public override Widget build(BuildContext context) {
            if (widget.autovalidate) {
                _validate();
            }

            return new WillPopScope(
                onWillPop: widget.onWillPop,
                child: new _FormScope(
                    formState: this,
                    generation: _generation,
                    child: widget.child
                )
            );
        }

        public void save() {
            foreach (FormFieldState field in _fields) {
                field.save();
            }
        }

        public void reset() {
            foreach (FormFieldState field in _fields) {
                field.reset();
            }

            _fieldDidChange();
        }

        public bool validate() {
            _forceRebuild();
            return _validate();
        }

        bool _validate() {
            bool hasError = false;
            foreach (FormFieldState field in _fields) {
                hasError = !field.validate() || hasError;
            }

            return !hasError;
        }
    }

    class _FormScope : InheritedWidget {
        public _FormScope(
            Key key = null,
            Widget child = null,
            FormState formState = null,
            int? generation = null
        ) : base(key: key, child: child) {
            _formState = formState;
            _generation = generation;
        }

        public readonly FormState _formState;

        public readonly int? _generation;

        public Form form {
            get { return _formState.widget; }
        }

        public override bool updateShouldNotify(InheritedWidget _old) {
            _FormScope old = _old as _FormScope;
            return _generation != old._generation;
        }
    }

    public delegate string FormFieldValidator<T>(T value);

    public delegate void FormFieldSetter<T>(T newValue);

    public delegate Widget FormFieldBuilder<T>(FormFieldState<T> field);

    public class FormField<T> : StatefulWidget {
        public FormField(
            Key key = null,
            FormFieldBuilder<T> builder = null,
            FormFieldSetter<T> onSaved = null,
            FormFieldValidator<T> validator = null,
            T initialValue = default,
            bool autovalidate = false,
            bool enabled = true
        ) : base(key: key) {
            D.assert(builder != null);
            this.onSaved = onSaved;
            this.validator = validator;
            this.builder = builder;
            this.initialValue = initialValue;
            this.autovalidate = autovalidate;
            this.enabled = enabled;
        }

        public readonly FormFieldSetter<T> onSaved;

        public readonly FormFieldValidator<T> validator;

        public readonly FormFieldBuilder<T> builder;

        public readonly T initialValue;

        public readonly bool autovalidate;

        public readonly bool enabled;

        public override State createState() {
            return new FormFieldState<T>();
        }
    }

    public interface FormFieldState {
        void save();

        bool validate();

        void reset();
    }

    public class FormFieldState<T> : State<FormField<T>>, FormFieldState {
        T _value;
        string _errorText;

        public T value {
            get { return _value; }
        }

        public string errorText {
            get { return _errorText; }
        }

        public bool hasError {
            get { return _errorText != null; }
        }

        public bool isValid {
            get {
                return widget.validator?.Invoke(_value) == null;
            }
        }

        public void save() {
            if (widget.onSaved != null) {
                widget.onSaved(value);
            }
        }

        public virtual void reset() {
            setState(() => {
                _value = widget.initialValue;
                _errorText = null;
            });
        }

        public bool validate() {
            setState(() => { _validate(); });
            return !hasError;
        }

        void _validate() {
            if (widget.validator != null) {
                _errorText = widget.validator(_value);
            }
        }

        public virtual void didChange(T value) {
            setState(() => { _value = value; });
            Form.of(context)?._fieldDidChange();
        }

        protected void setValue(T value) {
            _value = value;
        }

        public override void initState() {
            base.initState();
            _value = widget.initialValue;
        }

        public override void deactivate() {
            Form.of(context)?._unregister(this);
            base.deactivate();
        }

        public override Widget build(BuildContext context) {
            if (widget.autovalidate && widget.enabled) {
                _validate();
            }

            Form.of(context)?._register(this);
            return widget.builder(this);
        }
    }
}