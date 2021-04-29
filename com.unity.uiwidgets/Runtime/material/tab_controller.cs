using System;
using Unity.UIWidgets.animation;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.scheduler;
using Unity.UIWidgets.widgets;
using UnityEngine;

namespace Unity.UIWidgets.material {
    public class TabController : ChangeNotifier {
        public TabController(
            int initialIndex = 0,
            int? length = null,
            TickerProvider vsync = null) {
            D.assert(length != null && length >= 0);
            D.assert(initialIndex >= 0 && (length == 0 || initialIndex < length));
            D.assert(vsync != null);
            this.length = length.Value;

            _index = initialIndex;
            _previousIndex = initialIndex;
            _animationController = AnimationController.unbounded(value: initialIndex, vsync: vsync);
        }

        internal TabController(
            int index = 0,
            int previousIndex = 0,
            AnimationController animationController = null,
            int? length = null
        ) {
            D.assert(length != null);
            _index = index;
            _previousIndex = index;
            this.length = length.Value;
            _animationController = animationController;
        }

        internal TabController _copyWith(
            int? index = null,
            int? length = null,
            int? previousIndex = null
        ) {
            return new TabController(
                index: index ?? _index,
                length: length ?? this.length,
                animationController: _animationController,
                previousIndex: previousIndex ?? _previousIndex
            );
        }

        public Animation<float> animation {
            get { return _animationController?.view; }
        }

        AnimationController _animationController;

        public readonly int length;

        void _changeIndex(int value, TimeSpan? duration = null, Curve curve = null) {
            D.assert(value >= 0 && (value < length || length == 0));
            D.assert(duration != null || curve == null);
            D.assert(_indexIsChangingCount >= 0);

            if (value == _index || length < 2) {
                return;
            }

            _previousIndex = index;
            _index = value;
            if (duration != null) {
                _indexIsChangingCount++;
                notifyListeners();
                _animationController.animateTo(
                    _index, duration: duration, curve: curve).whenCompleteOrCancel(() => {
                    _indexIsChangingCount--;
                    notifyListeners();
                });
            }
            else {
                _indexIsChangingCount++;
                _animationController.setValue(_index);
                _indexIsChangingCount--;
                notifyListeners();
            }
        }

        public int index {
            get { return _index; }
            set { _changeIndex(value); }
        }

        int _index;

        public int previousIndex {
            get { return _previousIndex; }
        }

        int _previousIndex;

        public bool indexIsChanging {
            get { return _indexIsChangingCount != 0; }
        }

        int _indexIsChangingCount = 0;

        public void animateTo(int value, TimeSpan? duration = null, Curve curve = null) {
            duration = duration ?? material_.kTabScrollDuration;
            curve = curve ?? Curves.ease;
            _changeIndex(value, duration: duration, curve: curve);
        }

        public float offset {
            get { return _animationController.value - _index; }
            set {
                D.assert(value >= -1.0f && value <= 1.0f);
                D.assert(!indexIsChanging);
                if (value == offset) {
                    return;
                }

                _animationController.setValue(value + _index);
            }
        }

        public override void dispose() {
            _animationController = null;
            base.dispose();
        }
    }

    class _TabControllerScope : InheritedWidget {
        public _TabControllerScope(
            Key key = null,
            TabController controller = null,
            bool? enabled = null,
            Widget child = null
        ) : base(key: key, child: child) {
            this.controller = controller;
            this.enabled = enabled;
        }

        public readonly TabController controller;

        public readonly bool? enabled;

        public override bool updateShouldNotify(InheritedWidget old) {
            _TabControllerScope _old = (_TabControllerScope) old;
            return enabled != _old.enabled
                   || controller != _old.controller;
        }
    }

    public class DefaultTabController : StatefulWidget {
        public DefaultTabController(
            Key key = null,
            int? length = null,
            int initialIndex = 0,
            Widget child = null
        ) : base(key: key) {
            D.assert(length != null);
            D.assert(length >= 0);
            D.assert(length == 0 || (initialIndex >= 0 && initialIndex < length));
            D.assert(child != null);
            this.length = length;
            this.initialIndex = initialIndex;
            this.child = child;
        }

        public readonly int? length;

        public readonly int initialIndex;

        public readonly Widget child;

        public static TabController of(BuildContext context) {
            _TabControllerScope scope = context.dependOnInheritedWidgetOfExactType<_TabControllerScope>();
            return scope?.controller;
        }

        public override State createState() {
            return new _DefaultTabControllerState();
        }
    }

    class _DefaultTabControllerState : SingleTickerProviderStateMixin<DefaultTabController> {
        TabController _controller;

        public override void initState() {
            base.initState();
            _controller = new TabController(
                vsync: this,
                length: widget.length,
                initialIndex: widget.initialIndex
            );
        }

        public override void dispose() {
            _controller.dispose();
            base.dispose();
        }

        public override Widget build(BuildContext context) {
            return new _TabControllerScope(
                controller: _controller,
                enabled: TickerMode.of(context),
                child: widget.child
            );
        }

        public override void didUpdateWidget(StatefulWidget oldWidget) {
            var _oldWidget = (DefaultTabController) oldWidget;
            base.didUpdateWidget(oldWidget);
            if (_oldWidget.length != widget.length) {
                int newIndex = 0;
                int previousIndex = _controller.previousIndex;
                if (_controller.index >= widget.length) {
                    newIndex = Mathf.Max(0, widget.length.Value - 1);
                    previousIndex = _controller.index;
                }

                _controller = _controller._copyWith(
                    length: widget.length,
                    index: newIndex,
                    previousIndex: previousIndex
                );
            }
        }
    }
}