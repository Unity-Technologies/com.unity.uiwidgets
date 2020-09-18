using Unity.UIWidgets.foundation;

namespace Unity.UIWidgets.widgets {
    public class WillPopScope : StatefulWidget {
        public WillPopScope(
            Key key = null,
            Widget child = null,
            WillPopCallback onWillPop = null
        ) : base(key: key) {
            D.assert(child != null);
            this.onWillPop = onWillPop;
            this.child = child;
        }

        public readonly Widget child;

        public readonly WillPopCallback onWillPop;

        public override State createState() {
            return new _WillPopScopeState();
        }
    }

    class _WillPopScopeState : State<WillPopScope> {
        ModalRoute _route;

        public _WillPopScopeState() {
        }

        public override void didChangeDependencies() {
            base.didChangeDependencies();
            if (widget.onWillPop != null) {
                _route?.removeScopedWillPopCallback(widget.onWillPop);
            }

            _route = ModalRoute.of(context);
            if (widget.onWillPop != null) {
                _route?.addScopedWillPopCallback(widget.onWillPop);
            }
        }

        public override void didUpdateWidget(StatefulWidget _oldWidget) {
            base.didUpdateWidget(_oldWidget);
            D.assert(_route == ModalRoute.of(context));
            WillPopScope oldWidget = _oldWidget as WillPopScope;
            if (widget.onWillPop != oldWidget.onWillPop && _route != null) {
                if (oldWidget.onWillPop != null) {
                    _route.removeScopedWillPopCallback(oldWidget.onWillPop);
                }

                if (widget.onWillPop != null) {
                    _route.addScopedWillPopCallback(widget.onWillPop);
                }
            }
        }

        public override void dispose() {
            if (widget.onWillPop != null) {
                _route?.removeScopedWillPopCallback(widget.onWillPop);
            }

            base.dispose();
        }

        public override Widget build(BuildContext context) {
            return widget.child;
        }
    }
}