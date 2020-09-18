using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using RSG;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.ui;
using UnityEngine;

namespace Unity.UIWidgets.async {
    public class UIWidgetsCoroutine {
        _WaitForSecondsProcessor _waitProcessor;
        _WaitForCoroutineProcessor _waitForCoroutine;
        _WaitForAsyncOPProcessor _waitForAsyncOPProcessor;

        readonly IEnumerator _routine;
        readonly Window _window;
        readonly IDisposable _unhook;
        readonly bool _isBackground;

        internal volatile object lastResult;
        internal volatile Exception lastError;
        internal bool isDone;

        readonly Promise<object> _promise = new Promise<object>(isSync: true);

        public IPromise<object> promise {
            get { return _promise; }
        }

        internal UIWidgetsCoroutine(IEnumerator routine, Window window, bool isBackground = false) {
            D.assert(routine != null);
            D.assert(window != null);

            _routine = routine;
            _window = window;
            _isBackground = isBackground;

            if (isBackground && BackgroundCallbacks.getInstance() != null) {
                _unhook = BackgroundCallbacks.getInstance().addCallback(_moveNext);
            }
            else {
                _unhook = _window.run(TimeSpan.Zero, _moveNext, periodic: true);
                _moveNext(true); // try to run the first enumeration in the current loop.
            }
        }

        void _moveNext() {
            _moveNext(false);
        }

        void _moveNext(bool firstTime) {
            D.assert(!isDone);

            var lastError = this.lastError;
            if (lastError != null) {
                _unhook.Dispose();

                isDone = true;
                lastResult = null;
                if (_isBackground) {
                    _window.runInMain(() => { _promise.Reject(lastError); });
                }
                else {
                    _promise.Reject(lastError);
                }

                return;
            }

            bool hasNext = true;
            try {
                if (firstTime) {
                    hasNext = _routine.MoveNext();
                }
                if (hasNext) {
                    hasNext = _processIEnumeratorRecursive(_routine);
                }
            }
            catch (Exception ex) {
                stop(ex);
                return;
            }

            if (!hasNext && !isDone) {
                _unhook.Dispose();

                isDone = true;
                D.assert(this.lastError == null);
                if (_isBackground) {
                    _window.runInMain(() => { _promise.Resolve(lastResult); });
                }
                else {
                    _promise.Resolve(lastResult);
                }
            }
        }

        bool _processIEnumeratorRecursive(IEnumerator child) {
            D.assert(child != null);

            if (child.Current is IEnumerator nestedEnumerator) {
                return _processIEnumeratorRecursive(nestedEnumerator) || child.MoveNext();
            }

            if (child.Current is UIWidgetsCoroutine nestedCoroutine) {
                if (_isBackground) {
                    throw new Exception("nestedCoroutine is not supported in Background Coroutine");
                }

                _waitForCoroutine.set(nestedCoroutine);
                return _waitForCoroutine.moveNext(child, this);
            }

            if (child.Current is UIWidgetsWaitForSeconds waitForSeconds) {
                if (_isBackground) {
                    throw new Exception("waitForSeconds is not supported in Background Coroutine");
                }

                _waitProcessor.set(waitForSeconds);
                return _waitProcessor.moveNext(child);
            }

            if (child.Current is AsyncOperation waitForAsyncOP) {
                if (_isBackground) {
                    throw new Exception("asyncOperation is not supported in Background Coroutine");
                }

                _waitForAsyncOPProcessor.set(waitForAsyncOP);
                return _waitForAsyncOPProcessor.moveNext(child);
            }

            lastResult = child.Current;
            return child.MoveNext();
        }

        public void stop() {
            stop(null);
        }

        internal void stop(Exception ex) {
            if (lastError == null) {
                lastError = ex ?? new CoroutineCanceledException();
            }
        }
    }

    struct _WaitForSecondsProcessor {
        UIWidgetsWaitForSeconds _current;
        float _targetTime;

        public void set(UIWidgetsWaitForSeconds yieldStatement) {
            if (_current != yieldStatement) {
                _current = yieldStatement;
                _targetTime = Timer.timeSinceStartup + yieldStatement.waitTime;
            }
        }

        public bool moveNext(IEnumerator enumerator) {
            if (_targetTime <= Timer.timeSinceStartup) {
                _current = null;
                _targetTime = 0;
                return enumerator.MoveNext();
            }

            return true;
        }
    }

    struct _WaitForCoroutineProcessor {
        UIWidgetsCoroutine _current;

        public void set(UIWidgetsCoroutine routine) {
            if (_current != routine) {
                _current = routine;
            }
        }

        public bool moveNext(IEnumerator enumerator, UIWidgetsCoroutine parent) {
            if (_current.isDone) {
                var current = _current;
                _current = null;

                if (current.lastError != null) {
                    parent.stop(current.lastError);
                    return false;
                }

                parent.lastResult = current.lastResult;
                return enumerator.MoveNext();
            }

            return true;
        }
    }

    struct _WaitForAsyncOPProcessor {
        AsyncOperation _current;

        public void set(AsyncOperation operation) {
            if (_current != operation) {
                _current = operation;
            }
        }

        public bool moveNext(IEnumerator enumerator) {
            if (_current.isDone) {
                _current = null;
                return enumerator.MoveNext();
            }

            return true;
        }
    }

    public static class Coroutine {
        public static UIWidgetsCoroutine startCoroutine(this Window owner, IEnumerator routine) {
            return new UIWidgetsCoroutine(routine, owner);
        }

        public static UIWidgetsCoroutine startBackgroundCoroutine(this Window owner, IEnumerator routine) {
            return new UIWidgetsCoroutine(routine, owner, isBackground: true);
        }
    }

    public class CoroutineCanceledException : Exception {
    }

    public class UIWidgetsWaitForSeconds {
        public float waitTime { get; }

        public UIWidgetsWaitForSeconds(float time) {
            waitTime = time;
        }
    }

    class BackgroundCallbacks : IDisposable {
        static BackgroundCallbacks _instance;

        public static BackgroundCallbacks getInstance() {
#if UNITY_WEBGL
            return null;
#else
            if (_instance == null) {
                _instance = new BackgroundCallbacks(2);
            }

            return _instance;
#endif
        }

        readonly LinkedList<_CallbackNode> _callbackList;
        readonly ManualResetEvent _event;

        readonly Thread[] _threads;
        volatile bool _aborted = false;

        public BackgroundCallbacks(int threadCount = 1) {
            _callbackList = new LinkedList<_CallbackNode>();
            _event = new ManualResetEvent(false);

            _threads = new Thread[threadCount];
            for (var i = 0; i < _threads.Length; i++) {
                _threads[i] = new Thread(_threadLoop);
                _threads[i].Start();
            }
        }

        public void Dispose() {
            foreach (var t in _threads) {
                _aborted = true;
                _event.Set();
                t.Join();
            }

            _callbackList.Clear();
        }

        void _threadLoop() {
            while (true) {
                if (_aborted) {
                    break;
                }

                LinkedListNode<_CallbackNode> node;
                lock (_callbackList) {
                    node = _callbackList.First;
                    if (node != null) {
                        _callbackList.Remove(node);
                    }
                }

                if (node == null) {
                    _event.WaitOne();
                    _event.Reset();
                    continue;
                }

                var callbackNode = node.Value;
                D.assert(!callbackNode.isDone);

                try {
                    callbackNode.callback();
                }
                catch (Exception ex) {
                    D.logError("Failed to execute callback in BackgroundCallbacks: ", ex);
                }

                if (!callbackNode.isDone) {
                    lock (_callbackList) {
                        _callbackList.AddLast(node);
                    }
                }
            }
        }

        public IDisposable addCallback(VoidCallback callback) {
            var node = new _CallbackNode {callback = callback};
            lock (_callbackList) {
                _callbackList.AddLast(node);
            }

            _event.Set();

            return new _CallbackDisposable(node);
        }

        class _CallbackDisposable : IDisposable {
            readonly _CallbackNode _node;

            public _CallbackDisposable(_CallbackNode node) {
                _node = node;
            }

            public void Dispose() {
                _node.isDone = true;
            }
        }

        class _CallbackNode {
            public VoidCallback callback;
            public volatile bool isDone;
        }
    }
}