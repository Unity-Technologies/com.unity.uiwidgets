using Unity.UIWidgets.foundation;
using Unity.UIWidgets.service;

namespace Unity.UIWidgets.widgets {
    public class RawKeyboardListener : StatefulWidget {
        public RawKeyboardListener(
            Key key = null,
            FocusNode focusNode = null,
            bool autofocus = false,
            ValueChanged<RawKeyEvent> onKey = null,
            Widget child = null
        ) : base(key: key) {
            D.assert(focusNode != null);
            D.assert(child != null);
            this.focusNode = focusNode;
            this.autofocus = autofocus;
            this.onKey = onKey;
            this.child = child;
        }

        public readonly FocusNode focusNode;

        public readonly bool autofocus;

        public readonly ValueChanged<RawKeyEvent> onKey;

        public readonly Widget child;

        public override State createState() => new _RawKeyboardListenerState();

        public override void debugFillProperties(DiagnosticPropertiesBuilder properties) {
            base.debugFillProperties(properties);
            properties.add(new DiagnosticsProperty<FocusNode>("focusNode", focusNode));
        }
    }

    public class _RawKeyboardListenerState : State<RawKeyboardListener> {
        public override void initState() {
            base.initState();
            widget.focusNode.addListener(_handleFocusChanged);
        }

        public override void didUpdateWidget(StatefulWidget oldWidget) {
            var _oldWidgt = oldWidget as RawKeyboardListener;
            base.didUpdateWidget(oldWidget);
            if (widget.focusNode != _oldWidgt.focusNode) {
                _oldWidgt.focusNode.removeListener(_handleFocusChanged);
                widget.focusNode.addListener(_handleFocusChanged);
            }
        }


        public override void dispose() {
            widget.focusNode.removeListener(_handleFocusChanged);
            _detachKeyboardIfAttached();
            base.dispose();
        }

        void _handleFocusChanged() {
            if (widget.focusNode.hasFocus)
                _attachKeyboardIfDetached();
            else
                _detachKeyboardIfAttached();
        }

        bool _listening = false;

        void _attachKeyboardIfDetached() {
            if (_listening)
                return;
            RawKeyboard.instance.addListener(_handleRawKeyEvent);
            _listening = true;
        }

        void _detachKeyboardIfAttached() {
            if (!_listening)
                return;
            RawKeyboard.instance.removeListener(_handleRawKeyEvent);
            _listening = false;
        }

        void _handleRawKeyEvent(RawKeyEvent evt) {
            if (widget.onKey != null)
                widget.onKey(evt);
        }

        public override Widget build(BuildContext context) {
            return new Focus(
                focusNode: widget.focusNode,
                autofocus: widget.autofocus,
                child: widget.child
            );
        }
    }
}