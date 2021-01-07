using System;
using System.Collections.Generic;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.ui;

namespace Unity.UIWidgets.animation {
    public abstract class AnimationLazyListenerMixinAnimation<T> : Animation<T> {
        int _listenerCounter = 0;

        protected void didRegisterListener() {
            D.assert(_listenerCounter >= 0);
            if (_listenerCounter == 0) {
                didStartListening();
            }

            _listenerCounter += 1;
        }

        protected void didUnregisterListener() {
            D.assert(_listenerCounter >= 1);
            _listenerCounter -= 1;
            if (_listenerCounter == 0) {
                didStopListening();
            }
        }

        protected abstract void didStartListening();

        protected abstract void didStopListening();

        public bool isListening {
            get { return _listenerCounter > 0; }
        }
    }


    public abstract class AnimationEagerListenerMixinAnimation<T> : Animation<T> {
        protected void didRegisterListener() {
        }

        protected void didUnregisterListener() {
        }

        public virtual void dispose() {
        }
    }


    public abstract class
        AnimationLocalListenersMixinAnimationLazyListenerMixinAnimation<T> : AnimationLazyListenerMixinAnimation<T> {
        readonly ObserverList<VoidCallback> _listeners = new ObserverList<VoidCallback>();

        public override void addListener(VoidCallback listener) {
            didRegisterListener();
            _listeners.Add(listener);
        }

        public override void removeListener(VoidCallback listener) {
            bool removed = _listeners.Remove(listener);
            if (removed) {
                didUnregisterListener();
            }
        }

        public void notifyListeners() {
            var localListeners = new List<VoidCallback>(_listeners);
            foreach (VoidCallback listener in localListeners) {
                try {
                    if (_listeners.Contains(listener)) {
                        listener();
                    }
                }
                catch (Exception exception) {
                    IEnumerable<DiagnosticsNode> infoCollector() {
                        yield return new DiagnosticsProperty<
                            AnimationLocalListenersMixinAnimationLazyListenerMixinAnimation<T>>(
                            "The " + GetType() + " notifying listeners was:",
                            this,
                            style: DiagnosticsTreeStyle.errorProperty
                        );
                    }

                    UIWidgetsError.reportError(new UIWidgetsErrorDetails(
                        exception: exception,
                        library: "animation library",
                        context: new ErrorDescription("while notifying listeners for " + GetType()),
                        informationCollector: infoCollector
                    ));
                }
            }
        }
    }


    public abstract class
        AnimationLocalListenersMixinAnimationEagerListenerMixinAnimation<T> : AnimationEagerListenerMixinAnimation<T> {
        readonly ObserverList<VoidCallback> _listeners = new ObserverList<VoidCallback>();

        public override void addListener(VoidCallback listener) {
            didRegisterListener();
            _listeners.Add(listener);
        }

        public override void removeListener(VoidCallback listener) {
            bool removed = _listeners.Remove(listener);
            if (removed) {
                didUnregisterListener();
            }
        }

        public void notifyListeners() {
            var localListeners = new List<VoidCallback>(_listeners);
            foreach (VoidCallback listener in localListeners) {
                try {
                    if (_listeners.Contains(listener)) {
                        listener();
                    }
                }
                catch (Exception exception) {
                    IEnumerable<DiagnosticsNode> infoCollector() {
                        yield return new DiagnosticsProperty<
                            AnimationLocalListenersMixinAnimationEagerListenerMixinAnimation<T>>(
                            "The " + GetType() + " notifying listeners was:",
                            this,
                            style: DiagnosticsTreeStyle.errorProperty
                        );
                    }

                    UIWidgetsError.reportError(new UIWidgetsErrorDetails(
                        exception: exception,
                        library: "animation library",
                        context: new ErrorDescription("while notifying listeners for " + GetType()),
                        informationCollector: infoCollector
                    ));
                }
            }
        }
    }


    public abstract class
        AnimationLocalStatusListenersMixinAnimationLocalListenersMixinAnimationLazyListenerMixinAnimation<T> :
            AnimationLocalListenersMixinAnimationLazyListenerMixinAnimation<T> {
        readonly ObserverList<AnimationStatusListener> _statusListeners = new ObserverList<AnimationStatusListener>();

        public override void addStatusListener(AnimationStatusListener listener) {
            didRegisterListener();
            _statusListeners.Add(listener);
        }

        public override void removeStatusListener(AnimationStatusListener listener) {
            bool removed = _statusListeners.Remove(listener);
            if (removed) {
                didUnregisterListener();
            }
        }

        public void notifyStatusListeners(AnimationStatus status) {
            var localListeners = new List<AnimationStatusListener>(_statusListeners);
            foreach (AnimationStatusListener listener in localListeners) {
                try {
                    if (_statusListeners.Contains(listener)) {
                        listener(status);
                    }
                }
                catch (Exception exception) {
                    IEnumerable<DiagnosticsNode> infoCollector() {
                        yield return new DiagnosticsProperty<
                            AnimationLocalStatusListenersMixinAnimationLocalListenersMixinAnimationLazyListenerMixinAnimation
                            <T>>(
                            "The " + GetType() + " notifying listeners was:",
                            this,
                            style: DiagnosticsTreeStyle.errorProperty
                        );
                    }

                    UIWidgetsError.reportError(new UIWidgetsErrorDetails(
                        exception: exception,
                        library: "animation library",
                        context: new ErrorDescription("while notifying status listeners for " + GetType()),
                        informationCollector: infoCollector
                    ));
                }
            }
        }
    }


    public abstract class
        AnimationLocalStatusListenersMixinAnimationLazyListenerMixinAnimation<T> : AnimationLazyListenerMixinAnimation<T
        > {
        readonly ObserverList<AnimationStatusListener> _statusListeners = new ObserverList<AnimationStatusListener>();

        public override void addStatusListener(AnimationStatusListener listener) {
            didRegisterListener();
            _statusListeners.Add(listener);
        }

        public override void removeStatusListener(AnimationStatusListener listener) {
            bool removed = _statusListeners.Remove(listener);
            if (removed) {
                didUnregisterListener();
            }
        }

        public void notifyStatusListeners(AnimationStatus status) {
            var localListeners = new List<AnimationStatusListener>(_statusListeners);
            foreach (AnimationStatusListener listener in localListeners) {
                try {
                    if (_statusListeners.Contains(listener)) {
                        listener(status);
                    }
                }
                catch (Exception exception) {
                    IEnumerable<DiagnosticsNode> infoCollector() {
                        yield return new DiagnosticsProperty<
                            AnimationLocalStatusListenersMixinAnimationLazyListenerMixinAnimation<T>>(
                            "The " + GetType() + " notifying listeners was:",
                            this,
                            style: DiagnosticsTreeStyle.errorProperty
                        );
                    }

                    UIWidgetsError.reportError(new UIWidgetsErrorDetails(
                        exception: exception,
                        library: "animation library",
                        context: new ErrorDescription("while notifying status listeners for " + GetType()),
                        informationCollector: infoCollector
                    ));
                }
            }
        }
    }


    public abstract class
        AnimationLocalStatusListenersMixinAnimationLocalListenersMixinAnimationEagerListenerMixinAnimation<T> :
            AnimationLocalListenersMixinAnimationEagerListenerMixinAnimation<T> {
        readonly ObserverList<AnimationStatusListener> _statusListeners = new ObserverList<AnimationStatusListener>();

        public override void addStatusListener(AnimationStatusListener listener) {
            didRegisterListener();
            _statusListeners.Add(listener);
        }

        public override void removeStatusListener(AnimationStatusListener listener) {
            bool removed = _statusListeners.Remove(listener);
            if (removed) {
                didUnregisterListener();
            }
        }

        public void notifyStatusListeners(AnimationStatus status) {
            var localListeners = new List<AnimationStatusListener>(_statusListeners);
            foreach (AnimationStatusListener listener in localListeners) {
                try {
                    if (_statusListeners.Contains(listener)) {
                        listener(status);
                    }
                }
                catch (Exception exception) {
                    IEnumerable<DiagnosticsNode> infoCollector() {
                        yield return new DiagnosticsProperty<
                            AnimationLocalStatusListenersMixinAnimationLocalListenersMixinAnimationEagerListenerMixinAnimation
                            <T>>(
                            "The " + GetType() + " notifying listeners was:",
                            this,
                            style: DiagnosticsTreeStyle.errorProperty
                        );
                    }

                    UIWidgetsError.reportError(new UIWidgetsErrorDetails(
                        exception: exception,
                        library: "animation library",
                        context: new ErrorDescription("while notifying status listeners for " + GetType()),
                        informationCollector: infoCollector
                    ));
                }
            }
        }
    }
}