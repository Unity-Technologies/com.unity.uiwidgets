using Unity.UIWidgets.foundation;

namespace Unity.UIWidgets.widgets {
    public delegate Widget ValueWidgetBuilder<T>(BuildContext context, T value, Widget child);

    public class ValueListenableBuilder<T> : StatefulWidget {
        public ValueListenableBuilder(
            ValueListenable<T> valueListenable ,
            ValueWidgetBuilder<T> builder ,
            Key key = null,
            Widget child = null
        ) : base(key: key) {
            D.assert(valueListenable != null);
            D.assert(builder != null);
            this.valueListenable = valueListenable;
            this.builder = builder;
            this.child = child;
        }

        public readonly ValueListenable<T> valueListenable;

        public readonly ValueWidgetBuilder<T> builder;

        public readonly Widget child;

        public override State createState() {
            return new _ValueListenableBuilderState<T>();
        }
    }

    class _ValueListenableBuilderState<T> : State<ValueListenableBuilder<T>> {
        T value;

        public override void initState() {
            base.initState();
            value = widget.valueListenable.value;
            widget.valueListenable.addListener(_valueChanged);
        }

        public override void didUpdateWidget(StatefulWidget _oldWidget) {
            ValueListenableBuilder<T> oldWidget = _oldWidget as ValueListenableBuilder<T>;
            if (oldWidget.valueListenable != widget.valueListenable) {
                oldWidget.valueListenable.removeListener(_valueChanged);
                value = widget.valueListenable.value;
                widget.valueListenable.addListener(_valueChanged);
            }

            base.didUpdateWidget(oldWidget);
        }

        public override void dispose() {
            widget.valueListenable.removeListener(_valueChanged);
            base.dispose();
        }

        void _valueChanged() {
            setState(() => { value = widget.valueListenable.value; });
        }

        public override Widget build(BuildContext context) {
            return widget.builder(context, value, widget.child);
        }
    }
}