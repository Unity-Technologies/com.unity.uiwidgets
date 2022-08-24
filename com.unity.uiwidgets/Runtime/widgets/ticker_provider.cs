using System;
using System.Collections.Generic;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.scheduler;

namespace Unity.UIWidgets.widgets {
    public class TickerMode : StatelessWidget {
        public TickerMode(
            Key key = null,
            bool? enabled = true,
            Widget child = null)
            : base(key:key) {
            D.assert(enabled != null);
            this.enabled = enabled.Value;
            this.child = child;
        }

        public readonly bool enabled;
        public readonly Widget child;
        public static bool of(BuildContext context) {
            _EffectiveTickerMode widget = context.dependOnInheritedWidgetOfExactType<_EffectiveTickerMode>();
            return widget?.enabled ?? true;
        }

        public override Widget build(BuildContext context) {
            return new _EffectiveTickerMode(
                enabled: enabled && TickerMode.of(context),
                child: child
            );
        }
        public override void debugFillProperties(DiagnosticPropertiesBuilder properties) {
            base.debugFillProperties(properties);
            properties.add(new FlagProperty("requested mode", value: enabled, ifTrue: "enabled", ifFalse: "disabled", showName: true));
        }
    }
    public class _EffectiveTickerMode : InheritedWidget { 
        public _EffectiveTickerMode(
            bool enabled = false,
            Key key = null,
            Widget child = null) : 
            base(key: key, child: child) {
            this.enabled = enabled;
        }
        public readonly bool enabled;

        public override void debugFillProperties(DiagnosticPropertiesBuilder properties) {
            base.debugFillProperties(properties);
            properties.add(new FlagProperty("effective mode", value: enabled, ifTrue: "enabled", ifFalse: "disabled", showName: true));
        }

        public override bool updateShouldNotify(InheritedWidget oldWidget) {
            oldWidget = (_EffectiveTickerMode) oldWidget;
            return enabled != ((_EffectiveTickerMode) oldWidget).enabled;
        }
    }


    public abstract class SingleTickerProviderStateMixin<T> : State<T>, TickerProvider where T : StatefulWidget {
        Ticker _ticker;

        public Ticker createTicker(TickerCallback onTick) {
            D.assert(() => {
                if (_ticker == null) {
                    return true;
                }

                throw new UIWidgetsError(new List<DiagnosticsNode>{
                    new ErrorSummary($"{GetType()} is a SingleTickerProviderStateMixin but multiple tickers were created."),
                    new ErrorDescription("A SingleTickerProviderStateMixin can only be used as a TickerProvider once."),
                    new ErrorHint(
                        "If a State is used for multiple AnimationController objects, or if it is passed to other " +
                        "objects and those objects might use it more than one time in total, then instead of " +
                        "mixing in a SingleTickerProviderStateMixin, use a regular TickerProviderStateMixin."
                    )
                });
            });

            _ticker = new Ticker(onTick, debugLabel: foundation_.kDebugMode ? $"created by {this}" : null);
            return _ticker;
        }

        public override void dispose() {
            D.assert(() => {
                if (_ticker == null || !_ticker.isActive) {
                    return true;
                }

                throw new UIWidgetsError(new List<DiagnosticsNode>{
                    new ErrorSummary($"{this} was disposed with an active Ticker."),
                    new ErrorDescription(
                        $"{GetType()} created a Ticker via its SingleTickerProviderStateMixin, but at the time " +
                        "dispose() was called on the mixin, that Ticker was still active. The Ticker must " +
                        "be disposed before calling super.dispose()."
                    ),
                    new ErrorHint(
                        "Tickers used by AnimationControllers " +
                        "should be disposed by calling dispose() on the AnimationController itself. " +
                        "Otherwise, the ticker will leak."
                    ),
                    _ticker.describeForError("The offending ticker was")
                    }
                );
            });
            base.dispose();
        }

        public override void didChangeDependencies() {
            if (_ticker != null) {
                _ticker.muted = !TickerMode.of(context);
            }

            base.didChangeDependencies();
        }

        public override void debugFillProperties(DiagnosticPropertiesBuilder properties) {
            base.debugFillProperties(properties);
            string tickerDescription = null;
            if (_ticker != null) {
                if (_ticker.isActive && _ticker.muted) {
                    tickerDescription = "active but muted";
                }
                else if (_ticker.isActive) {
                    tickerDescription = "active";
                }
                else if (_ticker.muted) {
                    tickerDescription = "inactive and muted";
                }
                else {
                    tickerDescription = "inactive";
                }
            }

            properties.add(new DiagnosticsProperty<Ticker>("ticker", _ticker, description: tickerDescription,
                showSeparator: false, defaultValue: foundation_.kNullDefaultValue));
        }
    }

    // There is a copy of the implementation of this mixin at widgets/automatic_keep_alive.cs,
    // in AutomaticKeepAliveClientWithTickerProviderStateMixin, remember to keep the copy up to date
    public abstract class TickerProviderStateMixin<T> : State<T>, TickerProvider where T : StatefulWidget {
        HashSet<Ticker> _tickers;

        public Ticker createTicker(TickerCallback onTick) {
            _tickers = _tickers ?? new HashSet<Ticker>();
            
            var result = new _WidgetTicker<T>(onTick, this,
                debugLabel: foundation_.kDebugMode ? "created by " + this : null);
            _tickers.Add(result);
            return result;
        }

        internal void _removeTicker(_WidgetTicker<T> ticker) {
            D.assert(_tickers != null);
            D.assert(_tickers.Contains(ticker));
            _tickers.Remove(ticker);
        }

        public override void dispose() {
            D.assert(() => {
                if (_tickers != null) {
                    foreach (Ticker ticker in _tickers) {
                        if (ticker.isActive) {
                            throw new UIWidgetsError(new List<DiagnosticsNode>{
                                new ErrorSummary($"{this} was disposed with an active Ticker."),
                                new ErrorDescription(
                                    $"{GetType()} created a Ticker via its TickerProviderStateMixin, but at the time " +
                                    "dispose() was called on the mixin, that Ticker was still active. All Tickers must " +
                                    "be disposed before calling super.dispose()."
                                ),
                                new ErrorHint(
                                    "Tickers used by AnimationControllers " +
                                    "should be disposed by calling dispose() on the AnimationController itself. " +
                                    "Otherwise, the ticker will leak."
                                ),
                                ticker.describeForError("The offending ticker was"),
                            });
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
    }

    class _WidgetTicker<T> : Ticker where T : StatefulWidget {
        internal _WidgetTicker(
            TickerCallback onTick,
            TickerProviderStateMixin<T> creator,
            string debugLabel = null) :
            base(onTick: onTick, debugLabel: debugLabel) {
            _creator = creator;
        }

        readonly TickerProviderStateMixin<T> _creator;

        public override void Dispose() {
            _creator._removeTicker(this);
            base.Dispose();
        }
    }
}