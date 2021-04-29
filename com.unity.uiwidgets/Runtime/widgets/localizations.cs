using System;
using System.Collections.Generic;
using System.Linq;
using Unity.UIWidgets.async;
using Unity.UIWidgets.external;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.rendering;
using Unity.UIWidgets.ui;

namespace Unity.UIWidgets.widgets {
    class _Pending {
        public _Pending(
            LocalizationsDelegate del, 
            Future<object> futureValue) {
            this.del = del;
            this.futureValue = futureValue;
        }

        public readonly LocalizationsDelegate del;
        public readonly Future<object> futureValue;

        internal static Future<Dictionary<Type, object>> _loadAll(
            Locale locale,
            IEnumerable<LocalizationsDelegate> allDelegates) {
            Dictionary<Type, object> output = new Dictionary<Type, object>();
            List<_Pending> pendingList = null;

            HashSet<Type> types = new HashSet<Type>();
            List<LocalizationsDelegate> delegates = new List<LocalizationsDelegate>();
            foreach (LocalizationsDelegate del in allDelegates) {
                if (!types.Contains(del.type) && del.isSupported(locale)) {
                    types.Add(del.type);
                    delegates.Add(del);
                }
            }

            foreach (LocalizationsDelegate del in delegates) {
                
                Future<object> inputValue = del.load(locale).to<object>();
                object completedValue = null;
                Future<object> futureValue = inputValue.then_(value => {
                     completedValue = value;
                     return FutureOr.value(completedValue);
                }).to<object>();
                
                if (completedValue != null) {
                    Type type = del.type;
                    D.assert(!output.ContainsKey(type));
                    output[type] = completedValue;
                }
                else {
                    pendingList = pendingList ?? new List<_Pending>();
                    pendingList.Add(new _Pending(del, futureValue));
                }
            }

            if (pendingList == null) {
                return new SynchronousFuture<Dictionary<Type, object>>(output);
            }
            return Future.wait<object>(LinqUtils<Future<object>, _Pending>.SelectList(pendingList, (p => p.futureValue)))
                .then(values => {
                    
                    var list = (List<object>)values;
                    D.assert(list.Count == pendingList.Count);
                    for (int i = 0; i < list.Count; i += 1) {
                        Type type = pendingList[i].del.type;
                        D.assert(!output.ContainsKey(type));
                        output[type] = list[i];
                    }

                    return output;
                }).to<Dictionary<Type, object>>();
        }
    }

    public abstract class LocalizationsDelegate {
        protected LocalizationsDelegate() {
        }

        public abstract bool isSupported(Locale locale);

        public abstract Future<WidgetsLocalizations> load(Locale locale);

        public abstract bool shouldReload(LocalizationsDelegate old);

        public abstract Type type { get; }

        public override string ToString() {
            return $"{GetType()}[{type}]";
        }
    }

    public abstract class LocalizationsDelegate<T> : LocalizationsDelegate {
        public override Type type {
            get { return typeof(T); }
        }
    }

    public abstract class WidgetsLocalizations {
        public TextDirection textDirection { get; }

        static WidgetsLocalizations of(BuildContext context) {
            return Localizations.of<WidgetsLocalizations>(context, typeof(WidgetsLocalizations));
        }
    }

    class _WidgetsLocalizationsDelegate : LocalizationsDelegate<WidgetsLocalizations> {
        public _WidgetsLocalizationsDelegate() {
        }

        public override bool isSupported(Locale locale) {
            return true;
        }

        public override Future<WidgetsLocalizations> load(Locale locale) {
            return DefaultWidgetsLocalizations.load(locale);
        }

        public override bool shouldReload(LocalizationsDelegate old) {
            return false;
        }

        public override string ToString() {
            return "DefaultWidgetsLocalizations.delegate(en_US)";
        }
    }

    public class DefaultWidgetsLocalizations : WidgetsLocalizations {
        public DefaultWidgetsLocalizations() {
        }

        public static Future<WidgetsLocalizations> load(Locale locale) {
            return new SynchronousFuture<WidgetsLocalizations>(new DefaultWidgetsLocalizations());
        }

        public static readonly LocalizationsDelegate<WidgetsLocalizations> del = new _WidgetsLocalizationsDelegate();
    }

    class _LocalizationsScope : InheritedWidget {
        public _LocalizationsScope(
            Key key = null,
            Locale locale = null,
            _LocalizationsState localizationsState = null,
            Dictionary<Type, object> typeToResources = null,
            Widget child = null
        ) : base(key: key, child: child) {
            D.assert(locale != null);
            D.assert(localizationsState != null);
            D.assert(typeToResources != null);
            this.locale = locale;
            this.localizationsState = localizationsState;
            this.typeToResources = typeToResources;
        }

        public readonly Locale locale;

        public readonly _LocalizationsState localizationsState;

        public readonly Dictionary<Type, object> typeToResources;

        public override bool updateShouldNotify(InheritedWidget old) {
            return typeToResources != ((_LocalizationsScope) old).typeToResources;
        }
    }

    public class Localizations : StatefulWidget {
        public Localizations(
            Key key = null,
            Locale locale = null,
            List<LocalizationsDelegate> delegates = null,
            Widget child = null
        ) : base(key: key) {
            D.assert(locale != null);
            D.assert(delegates != null);
            D.assert(delegates.Any(del => del is LocalizationsDelegate<WidgetsLocalizations>));
            this.locale = locale;
            this.delegates = delegates;
            this.child = child;
        }

        public static Localizations overrides(
            Key key = null,
            BuildContext context = null,
            Locale locale = null,
            List<LocalizationsDelegate> delegates = null,
            Widget child = null
        ) {
            List<LocalizationsDelegate> mergedDelegates = _delegatesOf(context);
            if (delegates != null) {
                mergedDelegates.InsertRange(0, delegates);
            }

            return new Localizations(
                key: key,
                locale: locale ?? localeOf(context),
                delegates: mergedDelegates,
                child: child
            );
        }

        public readonly Locale locale;

        public readonly List<LocalizationsDelegate> delegates;

        public readonly Widget child;

        public static Locale localeOf(BuildContext context, bool nullOk = false) {
            D.assert(context != null);
            _LocalizationsScope scope =
                (_LocalizationsScope) context.dependOnInheritedWidgetOfExactType<_LocalizationsScope>();
            if (nullOk && scope == null) {
                return null;
            }

            D.assert((bool) (scope != null), () => "a Localizations ancestor was not found");
            return scope.localizationsState.locale;
        }

        public static List<LocalizationsDelegate> _delegatesOf(BuildContext context) {
            D.assert(context != null);
            _LocalizationsScope scope =
                (_LocalizationsScope) context.dependOnInheritedWidgetOfExactType<_LocalizationsScope>();
            D.assert(scope != null, () => "a Localizations ancestor was not found");
            return new List<LocalizationsDelegate>(scope.localizationsState.widget.delegates);
        }

        public static T of<T>(BuildContext context, Type type) {
            D.assert(context != null);
            D.assert(type != null);
            _LocalizationsScope scope =
                (_LocalizationsScope) context.dependOnInheritedWidgetOfExactType<_LocalizationsScope>();
            if (scope != null && scope.localizationsState != null) {
                return scope.localizationsState.resourcesFor<T>(type);
            }

            return default;
        }

        public override State createState() {
            return new _LocalizationsState();
        }

        public override void debugFillProperties(DiagnosticPropertiesBuilder properties) {
            base.debugFillProperties(properties);
            properties.add(new DiagnosticsProperty<Locale>("locale", locale));
            properties.add(new EnumerableProperty<LocalizationsDelegate>("delegates", delegates));
        }
    }


    class _LocalizationsState : State<Localizations> {
        readonly GlobalKey _localizedResourcesScopeKey = GlobalKey.key();
        Dictionary<Type, object> _typeToResources = new Dictionary<Type, object>();

        public Locale locale {
            get { return _locale; }
        }
        Locale _locale;

        public override void initState() {
            base.initState();
            load(widget.locale);
        }

        bool _anyDelegatesShouldReload(Localizations old) {
            if (widget.delegates.Count != old.delegates.Count) {
                return true;
            }

            List<LocalizationsDelegate> delegates = widget.delegates.ToList();
            List<LocalizationsDelegate> oldDelegates = old.delegates.ToList();
            for (int i = 0; i < delegates.Count; i += 1) {
                LocalizationsDelegate del = delegates[i];
                LocalizationsDelegate oldDelegate = oldDelegates[i];
                if (del.GetType() != oldDelegate.GetType() || del.shouldReload(oldDelegate)) {
                    return true;
                }
            }

            return false;
        }

        public override void didUpdateWidget(StatefulWidget oldWidget) {
            Localizations old = (Localizations) oldWidget;
            base.didUpdateWidget(old);
            if (widget.locale != old.locale
                || (widget.delegates == null && old.delegates != null)
                || (widget.delegates != null && old.delegates == null)
                || (widget.delegates != null && _anyDelegatesShouldReload(old))) {
                load(widget.locale);
            }
        }

        void load(Locale locale) {
            var delegates = widget.delegates;
            if (delegates == null || delegates.isEmpty()) {
                _locale = locale;
                return;
            }

            Dictionary<Type, object> typeToResources = null;
            Future<Dictionary<Type, object>> typeToResourcesFuture = _Pending._loadAll(locale, delegates)
                .then(value => { return FutureOr.value(typeToResources = (Dictionary<Type, object>)value); }).to<Dictionary<Type, object>>();

            if (typeToResources != null) {
                _typeToResources = typeToResources;
                _locale = locale;
            }
            else {
                typeToResourcesFuture.then(value => {
                    if (mounted) {
                        setState(() => {
                            _typeToResources = (Dictionary<Type, object>) value;
                            _locale = locale;
                        });
                    }
                });
            }
        }

        public T resourcesFor<T>(Type type) {
            D.assert(type != null);
            T resources = (T) _typeToResources.getOrDefault(type);
            return resources;
        }

        TextDirection _textDirection {
            get {
                WidgetsLocalizations resources = (WidgetsLocalizations)_typeToResources.getOrDefault(typeof(WidgetsLocalizations)) ;
                D.assert(resources != null);
                return resources.textDirection;
            }
        }
        public override Widget build(BuildContext context) {
            if (_locale == null) {
                return new Container();
            }

            return new _LocalizationsScope(
                key: _localizedResourcesScopeKey,
                locale: _locale,
                localizationsState: this,
                typeToResources: _typeToResources,
                child: new Directionality(
                    textDirection: _textDirection,
                    child: widget.child
                )
            );
        }
    }
}