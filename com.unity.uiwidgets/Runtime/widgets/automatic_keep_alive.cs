using System;
using System.Collections.Generic;
using Unity.UIWidgets.async;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.rendering;
using Unity.UIWidgets.scheduler;
using Unity.UIWidgets.ui;

namespace Unity.UIWidgets.widgets {
    public class AutomaticKeepAlive : StatefulWidget {
        public AutomaticKeepAlive(
            Key key = null,
            Widget child = null
        ) : base(key: key) {
            this.child = child;
        }

        public readonly Widget child;

        public override State createState() {
            return new _AutomaticKeepAliveState();
        }
    }

    class _AutomaticKeepAliveState : State<AutomaticKeepAlive> {
        Dictionary<Listenable, VoidCallback> _handles;
        Widget _child;
        bool _keepingAlive = false;

        public override void initState() {
            base.initState();
            _updateChild();
        }

        public override void didUpdateWidget(StatefulWidget oldWidget) {
            oldWidget = (AutomaticKeepAlive)oldWidget;
            base.didUpdateWidget(oldWidget);
            _updateChild();
        }

        void _updateChild() {
            _child = new NotificationListener<KeepAliveNotification>(
                onNotification: _addClient,
                child: widget.child
            );
        }

        public override void dispose() {
            if (_handles != null) {
                foreach (Listenable handle in _handles.Keys) {
                    handle.removeListener(_handles[handle]);
                }
            }
            base.dispose();
        }

        bool _addClient(KeepAliveNotification notification) {
            Listenable handle = notification.handle;
            _handles = _handles ?? new Dictionary<Listenable, VoidCallback>();
            D.assert(!_handles.ContainsKey(handle));
            _handles[handle] = _createCallback(handle);
            handle.addListener(_handles[handle]);
            if (!_keepingAlive) {
                _keepingAlive = true;
                ParentDataElement childElement = _getChildElement();
                if (childElement != null) {
                    _updateParentDataOfChild(childElement);
                }
                else {
                    SchedulerBinding.instance.addPostFrameCallback(timeStamp => {
                        if (!mounted) {
                            return;
                        }
                        ParentDataElement childElement1 = _getChildElement();
                        D.assert(childElement1 != null);
                        _updateParentDataOfChild(childElement1);
                    });
                }
            }

            return false;
        }
        
        ParentDataElement _getChildElement() {
            D.assert(mounted);
            Element element = (Element) context;
            Element childElement = null;
            element.visitChildren((Element child) => { childElement = child; });

            D.assert(childElement == null || childElement is ParentDataElement);
            return (ParentDataElement) childElement;
        }
        
       void _updateParentDataOfChild(ParentDataElement childElement) {
           childElement.applyWidgetOutOfTurn((ParentDataWidget) build(context));
       }

        VoidCallback _createCallback(Listenable handle) {
            return () => {
                D.assert(() => {
                    if (!mounted) {
                        throw new UIWidgetsError(
                            "AutomaticKeepAlive handle triggered after AutomaticKeepAlive was disposed." +
                            "Widgets should always trigger their KeepAliveNotification handle when they are " +
                            "deactivated, so that they (or their handle) do not send spurious events later " +
                            "when they are no longer in the tree."
                        );
                    }

                    return true;
                });
                _handles.Remove(handle);
                if (_handles.isEmpty()) {
                    if (SchedulerBinding.instance.schedulerPhase < SchedulerPhase.persistentCallbacks) {
                        setState(() => { _keepingAlive = false; });
                    }
                    else {
                        _keepingAlive = false;
                        async_.scheduleMicrotask(() => {
                            if (mounted && _handles.isEmpty()) {
                                setState(() => { D.assert(!_keepingAlive); });
                            }

                            return null;
                        });
                    }
                }
            };
        }

        public override Widget build(BuildContext context) {
            D.assert(_child != null);
            return new KeepAlive(
                keepAlive: _keepingAlive,
                child: _child
            );
        }

        public override void debugFillProperties(DiagnosticPropertiesBuilder description) {
            base.debugFillProperties(description);
            description.add(new FlagProperty("_keepingAlive", value: _keepingAlive,
                ifTrue: "keeping subtree alive"));
            description.add(new DiagnosticsProperty<Dictionary<Listenable, VoidCallback>>(
                "handles",
                _handles,
                description: _handles != null ? _handles.Count + " active clients" : null,
                ifNull: "no notifications ever received"
            ));
        }
    }

    public class KeepAliveNotification : Notification {
        public KeepAliveNotification(Listenable handle) {
            D.assert(handle != null);
            this.handle = handle;
        }

        public readonly Listenable handle;
    }

    public class KeepAliveHandle : ChangeNotifier {
        public void release() {
            notifyListeners();
        }
    }

    // There is a copy of the implementation of this mixin
    // in AutomaticKeepAliveClientWithTickerProviderStateMixin, remember to keep the copy up to date
    public abstract class AutomaticKeepAliveClientMixin<T> : State<T> where T : StatefulWidget {
        KeepAliveHandle _keepAliveHandle;

        void _ensureKeepAlive() {
            D.assert(_keepAliveHandle == null);
            _keepAliveHandle = new KeepAliveHandle();
            new KeepAliveNotification(_keepAliveHandle).dispatch(context);
        }

        void _releaseKeepAlive() {
            _keepAliveHandle.release();
            _keepAliveHandle = null;
        }

        protected abstract bool wantKeepAlive { get; }

        protected void updateKeepAlive() {
            if (wantKeepAlive) {
                if (_keepAliveHandle == null) {
                    _ensureKeepAlive();
                }
            }
            else {
                if (_keepAliveHandle != null) {
                    _releaseKeepAlive();
                }
            }
        }

        public override void initState() {
            base.initState();
            if (wantKeepAlive) {
                _ensureKeepAlive();
            }
        }

        public override void deactivate() {
            if (_keepAliveHandle != null) {
                _releaseKeepAlive();
            }

            base.deactivate();
        }

        public override Widget build(BuildContext context) {
            if (wantKeepAlive && _keepAliveHandle == null) {
                _ensureKeepAlive();
            }

            return null;
        }
    }


    public abstract class AutomaticKeepAliveClientWithTickerProviderStateMixin<T> : State<T>, TickerProvider
        where T : StatefulWidget {
        HashSet<Ticker> _tickers;

        public Ticker createTicker(TickerCallback onTick) {
            _tickers = _tickers ?? new HashSet<Ticker>();

            var result = new _AutomaticWidgetTicker<T>(onTick, this,
                debugLabel: foundation_.kDebugMode ? "created by " + this : null);
            _tickers.Add(result);
            return result;
        }

        internal void _removeTicker(_AutomaticWidgetTicker<T> ticker) {
            D.assert(_tickers != null);
            D.assert(_tickers.Contains(ticker));
            _tickers.Remove(ticker);
        }

        public override void dispose() {
            D.assert(() => {
                if (_tickers != null) {
                    foreach (Ticker ticker in _tickers) {
                        if (ticker.isActive) {
                            throw new UIWidgetsError(
                                this + " was disposed with an active Ticker.\n" +
                                GetType() +
                                " created a Ticker via its TickerProviderStateMixin, but at the time " +
                                "dispose() was called on the mixin, that Ticker was still active. All Tickers must " +
                                "be disposed before calling base.dispose(). Tickers used by AnimationControllers " +
                                "should be disposed by calling dispose() on the AnimationController itself. " +
                                "Otherwise, the ticker will leak.\n" +
                                "The offending ticker was: " + ticker.toString(debugIncludeStack: true)
                            );
                        }
                    }
                }

                return true;
            });
            base.dispose();
        }

        public override void didChangeDependencies() {
            bool muted = !TickerMode.of(context);
            if (_tickers != null) {
                foreach (Ticker ticker in _tickers) {
                    ticker.muted = muted;
                }
            }

            base.didChangeDependencies();
        }

        public override void debugFillProperties(DiagnosticPropertiesBuilder properties) {
            base.debugFillProperties(properties);
            properties.add(new DiagnosticsProperty<HashSet<Ticker>>(
                "tickers",
                _tickers,
                description: _tickers != null ? "tracking " + _tickers.Count + " tickers" : null,
                defaultValue: foundation_.kNullDefaultValue
            ));
        }

        KeepAliveHandle _keepAliveHandle;

        void _ensureKeepAlive() {
            D.assert(_keepAliveHandle == null);
            _keepAliveHandle = new KeepAliveHandle();
            new KeepAliveNotification(_keepAliveHandle).dispatch(context);
        }

        void _releaseKeepAlive() {
            _keepAliveHandle.release();
            _keepAliveHandle = null;
        }

        protected abstract bool wantKeepAlive { get; }

        protected void updateKeepAlive() {
            if (wantKeepAlive) {
                if (_keepAliveHandle == null) {
                    _ensureKeepAlive();
                }
            }
            else {
                if (_keepAliveHandle != null) {
                    _releaseKeepAlive();
                }
            }
        }

        public override void initState() {
            base.initState();
            if (wantKeepAlive) {
                _ensureKeepAlive();
            }
        }

        public override void deactivate() {
            if (_keepAliveHandle != null) {
                _releaseKeepAlive();
            }

            base.deactivate();
        }

        public override Widget build(BuildContext context) {
            if (wantKeepAlive && _keepAliveHandle == null) {
                _ensureKeepAlive();
            }

            return null;
        }
    }
}