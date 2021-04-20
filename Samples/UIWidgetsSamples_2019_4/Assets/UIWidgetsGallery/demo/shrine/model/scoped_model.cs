using System;
using System.Collections.Generic;
using System.Linq;
using Unity.UIWidgets.async;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.ui;
using Unity.UIWidgets.widgets;
using UnityEngine;

namespace UIWidgetsGallery.demo.shrine.model
{
    public abstract class Model : Listenable
    {
        readonly HashSet<VoidCallback> _listeners = new HashSet<VoidCallback>();
        internal int _version = 0;
        int _microtaskVersion = 0;
        
        public void addListener(VoidCallback listener) {
            _listeners.Add(listener);
        }
        
        public void removeListener(VoidCallback listener) {
            _listeners.Remove(listener);
        }
        
        int listenerCount => _listeners.Count;
        
        protected void notifyListeners() {
            // We schedule a microtask to debounce multiple changes that can occur
            // all at once.
            if (_microtaskVersion == _version) {
                _microtaskVersion++;
                async_.scheduleMicrotask(() => {
                    _version++;
                    _microtaskVersion = _version;

                    // Convert the Set to a List before executing each listener. This
                    // prevents errors that can arise if a listener removes itself during
                    // invocation!
                    _listeners.ToList().ForEach((VoidCallback listener) => listener());
                    return null;
                });
            }
        }
    }

    public class ScopedModel<T> : StatelessWidget where T : Model
    {
        public ScopedModel(T model, Widget child)
        {
            D.assert(model != null);
            D.assert(child != null);

            this.model = model;
            this.child = child;
        }
        
        public readonly T model;
        public readonly Widget child;

        public override Widget build(BuildContext context) {
            return new AnimatedBuilder(
                animation: model,
                builder: (subContext, _) => new _InheritedModel<T>(model: model, child: child)
            );
        }
        
        public static T of(
            BuildContext context, 
            bool rebuildOnChange = false
        ) {
            Type type = typeof(_InheritedModel<T>);

            Widget widget = rebuildOnChange
            ? context.dependOnInheritedWidgetOfExactType<_InheritedModel<T>>()
            : context.findAncestorWidgetOfExactType<_InheritedModel<T>>();

            if (widget == null) {
                throw new ScopedModelError();
            } else {
                return (widget as _InheritedModel<T>).model;
            }
        }
    }

    public class _InheritedModel<T> : InheritedWidget where T : Model
    {
        public readonly T model;
        public readonly int version;

        public _InheritedModel(Key key = null, Widget child = null, T model = null) :
            base(key: key, child: child)
        {
            this.model = model;
            this.version = model._version;
        }

        public override bool updateShouldNotify(InheritedWidget oldWidget)
        {
            var _oldWidget = (_InheritedModel<T>) oldWidget;
            return _oldWidget.version != version;
        }
    }

    public delegate Widget ScopedModelDescendantBuilder<T>(
        BuildContext context,
        Widget child,
        T model) where T : Model;

    public class ScopedModelDescendant<T> : StatelessWidget where T : Model
    {
        public readonly ScopedModelDescendantBuilder<T> builder;
        
        public readonly  Widget child;
        
        public readonly  bool rebuildOnChange;

        public ScopedModelDescendant(
            ScopedModelDescendantBuilder<T> builder = null,
            Widget child = null,
            bool rebuildOnChange = true)
        {
            D.assert(builder != null);
            this.builder = builder;
            this.child = child;
            this.rebuildOnChange = rebuildOnChange;
        }

        public override Widget build(BuildContext context) {
            return builder(
                context,
                child,
                ScopedModel<T>.of(context, rebuildOnChange: rebuildOnChange)
            );
        }
    }

    public class ScopedModelError : UIWidgetsError
    {
        public ScopedModelError() : base("ScopedModelError")
        {
        }

        public override string ToString()
        {
            return "Error: Could not find the correct ScopedModel.";
        }
    }
}