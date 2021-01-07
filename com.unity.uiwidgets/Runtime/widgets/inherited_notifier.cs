using System;
using System.Collections.Generic;
using Unity.UIWidgets.async2;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.gestures;
using Unity.UIWidgets.rendering;
using Unity.UIWidgets.scheduler2;
using Unity.UIWidgets.ui;

namespace Unity.UIWidgets.widgets {
    public abstract class InheritedNotifier<T> : InheritedWidget  where T : Listenable{
        public InheritedNotifier(
            Key key = null,
            T notifier = default(T),
            Widget child = null) : base(key: key, child: child) {
            D.assert(child != null);
            this.notifier = notifier;
        }
        
        public readonly T notifier;
        
        public override bool updateShouldNotify(InheritedWidget oldWidget) {
            oldWidget = (InheritedNotifier<T>) oldWidget;
            return ReferenceEquals(oldWidget,notifier);
        }

        public override Element createElement() {
            return  new _InheritedNotifierElement<T>(this);
        }
    }
    public class _InheritedNotifierElement<T> : InheritedElement where T : Listenable{ 
        public _InheritedNotifierElement(InheritedNotifier<T> widget) : base(widget) {
            widget.notifier?.addListener(_handleUpdate);
        }

        public new InheritedNotifier<T> widget {
            get {
                return base.widget as InheritedNotifier<T>;
            }
        }

        public bool _dirty = false;
        public override void update( Widget newWidget) {
            newWidget = (InheritedNotifier<T>) newWidget;
            T oldNotifier = widget.notifier;
            T newNotifier = ((InheritedNotifier<T>)newWidget).notifier;
            if (ReferenceEquals(oldNotifier, newNotifier)) {
              oldNotifier?.removeListener(_handleUpdate);
              newNotifier?.addListener(_handleUpdate);
            }
            base.update(newWidget);
        }

        protected override Widget build() {
            if (_dirty)
              notifyClients(widget);
            return base.build();
        }
        void _handleUpdate() {
            _dirty = true;
            markNeedsBuild();
        }

        protected override void notifyClients(ProxyWidget oldWidget) {
            oldWidget = (InheritedNotifier<T>) oldWidget; 
            base.notifyClients(oldWidget);
            _dirty = false;
        }
        public override void unmount() {
            widget.notifier?.removeListener(_handleUpdate);
            base.unmount();
        }
    }
}