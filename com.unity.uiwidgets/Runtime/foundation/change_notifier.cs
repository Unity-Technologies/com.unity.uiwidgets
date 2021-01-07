using System;
using System.Collections.Generic;
using System.Linq;
using Unity.UIWidgets.ui;

namespace Unity.UIWidgets.foundation {
    public interface Listenable {
        void addListener(VoidCallback listener);

        void removeListener(VoidCallback listener);
    }

    public static class ListenableUtils {
        public static Listenable merge(this List<Listenable> listenables) {
            return new _MergingListenable(listenables);
        }
    }

    public interface ValueListenable<T> : Listenable {
        T value { get; }
    }

    public class ChangeNotifier : Listenable {
        ObserverList<VoidCallback> _listeners = new ObserverList<VoidCallback>();

        bool _debugAssertNotDisposed() {
            D.assert(() => {
                if (_listeners == null) {
                    throw new UIWidgetsError($"A {GetType()} was used after being disposed.\n" +
                                             "Once you have called dispose() on a {GetType()}, it can no longer be used.");
                }

                return true;
            });

            return true;
        }

        protected bool hasListeners {
            get {
                D.assert(_debugAssertNotDisposed());
                return _listeners.isNotEmpty();
            }
        }

        public void addListener(VoidCallback listener) {
            D.assert(_debugAssertNotDisposed());
            _listeners.Add(listener);
        }

        public void removeListener(VoidCallback listener) {
            D.assert(_debugAssertNotDisposed());
            _listeners.Remove(listener);
        }

        public virtual void dispose() {
            D.assert(_debugAssertNotDisposed());
            _listeners = null;
        }

        protected virtual void notifyListeners() {
            D.assert(_debugAssertNotDisposed());
            if (_listeners != null) {
                var localListeners = new List<VoidCallback>(_listeners);
                foreach (VoidCallback listener in localListeners) {
                    try {
                        if (_listeners.Contains(listener)) {
                            listener();
                        }
                    }
                    catch (Exception ex) {
                        IEnumerable<DiagnosticsNode> infoCollector() {
                            yield return new DiagnosticsProperty<ChangeNotifier>(
                                $"The {GetType()} sending notification was",
                                this,
                                style: DiagnosticsTreeStyle.errorProperty
                            );
                        }
                        
                        UIWidgetsError.reportError(new UIWidgetsErrorDetails(
                            exception: ex,
                            library: "foundation library",
                            context: new ErrorDescription($"while dispatching notifications for {GetType()}"),
                            informationCollector: infoCollector
                        ));
                    }
                }
            }
        }
    }

    class _MergingListenable : Listenable {
        internal _MergingListenable(List<Listenable> _children) {
            this._children = _children;
        }

        readonly List<Listenable> _children;

        public void addListener(VoidCallback listener) {
            foreach (Listenable child in _children) {
                child?.addListener(listener);
            }
        }

        public void removeListener(VoidCallback listener) {
            foreach (Listenable child in _children) {
                child?.removeListener(listener);
            }
        }

        public override string ToString() {
            return "Listenable.merge([" + _children.toStringList() + "])";
        }
    }

    public class ValueNotifier<T> : ChangeNotifier, ValueListenable<T> {
        public ValueNotifier(T value) {
            _value = value;
        }

        public virtual T value {
            get { return _value; }
            set {
                if (Equals(value, _value)) {
                    return;
                }

                _value = value;
                notifyListeners();
            }
        }

        T _value;

        public override string ToString() {
            return $"{foundation_.describeIdentity(this)}({_value})";
        }
    }
}