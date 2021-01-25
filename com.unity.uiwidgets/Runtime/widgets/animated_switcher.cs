using System;
using System.Collections.Generic;
using System.Linq;
using Unity.UIWidgets.animation;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.painting;

namespace Unity.UIWidgets.widgets {
    class _ChildEntry {
        public _ChildEntry(
            AnimationController controller = null,
            Animation<float> animation = null,
            Widget transition = null,
            Widget widgetChild = null
        ) {
            D.assert(animation != null);
            D.assert(transition != null);
            D.assert(controller != null);
            this.controller = controller;
            this.animation = animation;
            this.transition = transition;
            this.widgetChild = widgetChild;
        }

        public readonly AnimationController controller;

        public readonly Animation<float> animation;

        public Widget transition;

        public Widget widgetChild;

        public override string ToString() {
            return $"Entry#{foundation_.shortHash(this)}({widgetChild})";
        }
    }

    public delegate Widget AnimatedSwitcherTransitionBuilder(Widget child, Animation<float> animation);

    public delegate Widget AnimatedSwitcherLayoutBuilder(Widget currentChild, List<Widget> previousChildren);

    public class AnimatedSwitcher : StatefulWidget {
        public AnimatedSwitcher(
            Key key = null,
            Widget child = null,
            TimeSpan? duration = null,
            TimeSpan? reverseDuration = null,
            Curve switchInCurve = null,
            Curve switchOutCurve = null,
            AnimatedSwitcherTransitionBuilder transitionBuilder = null,
            AnimatedSwitcherLayoutBuilder layoutBuilder = null
        ) : base(key: key) {
            D.assert(duration != null);
            this.child = child;
            this.duration = duration;
            this.reverseDuration = reverseDuration;
            this.switchInCurve = switchInCurve ?? Curves.linear;
            this.switchOutCurve = switchOutCurve ?? Curves.linear;
            this.transitionBuilder = transitionBuilder ?? defaultTransitionBuilder;
            this.layoutBuilder = layoutBuilder ?? defaultLayoutBuilder;
        }

        public readonly Widget child;

        public readonly TimeSpan? duration;
        
        public readonly TimeSpan? reverseDuration;

        public readonly Curve switchInCurve;

        public readonly Curve switchOutCurve;

        public readonly AnimatedSwitcherTransitionBuilder transitionBuilder;

        public readonly AnimatedSwitcherLayoutBuilder layoutBuilder;

        public override State createState() {
            return new _AnimatedSwitcherState();
        }

        public static Widget defaultTransitionBuilder(Widget child, Animation<float> animation) {
            return new FadeTransition(
                opacity: animation,
                child: child
            );
        }

        public static Widget defaultLayoutBuilder(Widget currentChild, List<Widget> previousChildren) {
            List<Widget> children = previousChildren;
            if (currentChild != null) {
                children = children.ToList();
                children.Add(currentChild);
            }

            return new Stack(
                children: children,
                alignment: Alignment.center
            );
        }
        
        public override void debugFillProperties(DiagnosticPropertiesBuilder properties) {
            base.debugFillProperties(properties);
            properties.add(new IntProperty("duration", duration?.Milliseconds, unit: "ms"));
            properties.add(new IntProperty("reverseDuration", reverseDuration?.Milliseconds, unit: "ms", defaultValue: null));
        }
    }

    class _AnimatedSwitcherState : TickerProviderStateMixin<AnimatedSwitcher> {
        _ChildEntry _currentEntry;
        HashSet<_ChildEntry> _outgoingEntries = new HashSet<_ChildEntry>();
        List<Widget> _outgoingWidgets = new List<Widget>();
        int _childNumber = 0;

        public override void initState() {
            base.initState();
            _addEntryForNewChild(animate: false);
        }

        public override void didUpdateWidget(StatefulWidget _oldWidget) {
            base.didUpdateWidget(_oldWidget);
            AnimatedSwitcher oldWidget = _oldWidget as AnimatedSwitcher;

            if (widget.transitionBuilder != oldWidget.transitionBuilder) {
                _outgoingEntries.Each(_updateTransitionForEntry);
                if (_currentEntry != null) {
                    _updateTransitionForEntry(_currentEntry);
                }

                _markChildWidgetCacheAsDirty();
            }

            bool hasNewChild = widget.child != null;
            bool hasOldChild = _currentEntry != null;
            if (hasNewChild != hasOldChild ||
                hasNewChild && !Widget.canUpdate(widget.child, _currentEntry.widgetChild)) {
                _childNumber += 1;
                _addEntryForNewChild(animate: true);
            }
            else if (_currentEntry != null) {
                D.assert(hasOldChild && hasNewChild);
                D.assert(Widget.canUpdate(widget.child, _currentEntry.widgetChild));
                _currentEntry.widgetChild = widget.child;
                _updateTransitionForEntry(_currentEntry);
                _markChildWidgetCacheAsDirty();
            }
        }

        void _addEntryForNewChild(bool animate) {
            D.assert(animate || _currentEntry == null);
            if (_currentEntry != null) {
                D.assert(animate);
                D.assert(!_outgoingEntries.Contains(_currentEntry));
                _outgoingEntries.Add(_currentEntry);
                _currentEntry.controller.reverse();
                _markChildWidgetCacheAsDirty();
                _currentEntry = null;
            }

            if (widget.child == null) {
                return;
            }

            AnimationController controller = new AnimationController(
                duration: widget.duration,
                reverseDuration:widget.reverseDuration,
                vsync: this
            );
            Animation<float> animation = new CurvedAnimation(
                parent: controller,
                curve: widget.switchInCurve,
                reverseCurve: widget.switchOutCurve
            );
            _currentEntry = _newEntry(
                child: widget.child,
                controller: controller,
                animation: animation,
                builder: widget.transitionBuilder
            );
            if (animate) {
                controller.forward();
            }
            else {
                D.assert(_outgoingEntries.isEmpty);
                controller.setValue(1.0f);
            }
        }

        _ChildEntry _newEntry(
            Widget child = null,
            AnimatedSwitcherTransitionBuilder builder = null,
            AnimationController controller = null,
            Animation<float> animation = null
        ) {
            _ChildEntry entry = new _ChildEntry(
                widgetChild: child,
                transition: KeyedSubtree.wrap(builder(child, animation), _childNumber),
                animation: animation,
                controller: controller
            );
            animation.addStatusListener((AnimationStatus status) => {
                if (status == AnimationStatus.dismissed) {
                    setState(() => {
                        D.assert(mounted);
                        D.assert(_outgoingEntries.Contains(entry));
                        _outgoingEntries.Remove(entry);
                        _markChildWidgetCacheAsDirty();
                    });
                    controller.dispose();
                }
            });
            return entry;
        }

        void _markChildWidgetCacheAsDirty() {
            _outgoingWidgets = null;
        }

        void _updateTransitionForEntry(_ChildEntry entry) {
            entry.transition = new KeyedSubtree(
                key: entry.transition.key,
                child: widget.transitionBuilder(entry.widgetChild, entry.animation)
            );
        }

        void _rebuildOutgoingWidgetsIfNeeded() {
            if (_outgoingWidgets == null) {
                _outgoingWidgets = new List<Widget>(_outgoingEntries.Count);
                foreach (_ChildEntry entry in _outgoingEntries) {
                    _outgoingWidgets.Add(entry.transition);
                }
            }
            
            D.assert(_outgoingEntries.Count == _outgoingWidgets.Count);
            D.assert(_outgoingEntries.isEmpty() ||
                     _outgoingEntries.Last().transition == _outgoingWidgets.Last());
        }

        public override void dispose() {
            if (_currentEntry != null) {
                _currentEntry.controller.dispose();
            }

            foreach (_ChildEntry entry in _outgoingEntries) {
                entry.controller.dispose();
            }

            base.dispose();
        }

        public override Widget build(BuildContext context) {
            _rebuildOutgoingWidgetsIfNeeded();
            return widget.layoutBuilder(_currentEntry?.transition, _outgoingWidgets);
        }
    }
}