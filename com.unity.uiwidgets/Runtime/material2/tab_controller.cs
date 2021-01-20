using System;
using Unity.UIWidgets.animation;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.scheduler;
using Unity.UIWidgets.widgets;

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
            _animationController = length < 2
                ? null
                : new AnimationController(
                    value: initialIndex,
                    upperBound: length.Value - 1,
                    vsync: vsync);
        }

        public Animation<float> animation {
            get { return _animationController?.view ?? Animations.kAlwaysCompleteAnimation; }
        }

        readonly AnimationController _animationController;

        public readonly int length;

        void _changeIndex(int value, TimeSpan? duration = null, Curve curve = null) {
            D.assert(value >= 0 && (value < length || length == 0));
            D.assert(duration == null ? curve == null : true);
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
            duration = duration ?? Constants.kTabScrollDuration;
            curve = curve ?? Curves.ease;
            _changeIndex(value, duration: duration, curve: curve);
        }

        public float offset {
            get { return length > 1 ? _animationController.value - _index : 0.0f; }
            set {
                D.assert(length > 1);
                D.assert(value >= -1.0f && value <= 1.0f);
                D.assert(!indexIsChanging);
                if (value == offset) {
                    return;
                }

                _animationController.setValue(value + _index);
            }
        }

        public override void dispose() {
            _animationController?.dispose();
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
            D.assert(child != null);
            this.length = length;
            this.initialIndex = initialIndex;
            this.child = child;
        }

        public readonly int? length;

        public readonly int initialIndex;

        public readonly Widget child;

        public static TabController of(BuildContext context) {
            _TabControllerScope scope =
                (_TabControllerScope) context.inheritFromWidgetOfExactType(typeof(_TabControllerScope));
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
    }
}