using System;
using System.Collections.Generic;
using Unity.UIWidgets.async;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.gestures;
using Unity.UIWidgets.rendering;
using Unity.UIWidgets.scheduler;
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
    public class _InheritedNotifierElement<T> : InheritedElement where T : Listenable
    { 
        public _InheritedNotifierElement(InheritedNotifier<T> widget) : base(widget) {
            widget.notifier?.addListener(_handleUpdate);
        }
        
        public new InheritedNotifier<T> widget {
            get {
                return base.widget as InheritedNotifier<T>;
            }
        }

        //In flutter this variable is named as _dirty and hides the property of its parent with the name variable name
        //We give it a new name, i.e., _notifier_dirty in UIWidgets so that the code looks more clear
        bool _notifier_dirty = false;

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
            if (_notifier_dirty)
              notifyClients(widget);
            return base.build();
        }
        void _handleUpdate() {
            _notifier_dirty = true;
            markNeedsBuild();
        }

        public override void notifyClients(ProxyWidget oldWidget) {
            oldWidget = (InheritedNotifier<T>) oldWidget; 
            base.notifyClients(oldWidget);
            _notifier_dirty = false;
        }
        public override void unmount() {
            widget.notifier?.removeListener(_handleUpdate);
            base.unmount();
        }
    }
}