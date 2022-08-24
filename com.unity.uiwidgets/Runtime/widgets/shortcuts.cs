using System;
using System.Collections.Generic;
using System.Linq;
using Unity.UIWidgets.external;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.service;
using Unity.UIWidgets.services;

namespace Unity.UIWidgets.widgets {
    public class KeySet<T> : KeyboardKey, IEquatable<KeySet<T>> {
        public static readonly List<int> _tempHashStore3 = new List<int> {0, 0, 0}; // used to sort exactly 3 keys
        public static readonly List<int> _tempHashStore4 = new List<int> {0, 0, 0, 0}; // used to sort exactly 4 keys
        public readonly HashSet<T> _keys;

        public KeySet(
            T key1,
            T key2 = default,
            T key3 = default,
            T key4 = default
        ) {
            D.assert(key1 != null);
            _keys = new HashSet<T>();
            _keys.Add(item: key1);
            var count = 1;
            if (key2 != null) {
                _keys.Add(item: key2);
                D.assert(() => {
                    count++;
                    return true;
                });
            }

            if (key3 != null) {
                _keys.Add(item: key3);
                D.assert(() => {
                    count++;
                    return true;
                });
            }

            if (key4 != null) {
                _keys.Add(item: key4);
                D.assert(() => {
                    count++;
                    return true;
                });
            }

            D.assert(_keys.Count == count,
                () => "Two or more provided keys are identical. Each key must appear only once.");
        }

        public KeySet(HashSet<T> keys) {
            D.assert(keys != null);
            D.assert(result: keys.isNotEmpty);
            D.assert(!keys.Contains(default));
            foreach (var key in keys) {
                _keys.Add(item: key);
            }
        }

        public HashSet<T> keys {
            get { return new HashSet<T>(collection: _keys); }
        }


        public bool Equals(KeySet<T> other) {
            if (ReferenceEquals(null, objB: other)) {
                return false;
            }

            if (ReferenceEquals(this, objB: other)) {
                return true;
            }

            return _keys.SetEquals(other._keys);
        }

        public override bool Equals(object obj) {
            if (ReferenceEquals(null, objB: obj)) {
                return false;
            }

            if (ReferenceEquals(this, objB: obj)) {
                return true;
            }

            if (obj.GetType() != GetType()) {
                return false;
            }

            return Equals((KeySet<T>) obj);
        }

        public override int GetHashCode() {
            unchecked {
                var length = _keys.Count;
                IEnumerator<T> iterator = _keys.GetEnumerator();

                // There's always at least one key. Just extract it.
                iterator.MoveNext();
                var h1 = iterator.Current.GetHashCode();

                if (length == 1) {
                    // Don't do anything fancy if there's exactly one key.
                    return h1;
                }

                iterator.MoveNext();
                var h2 = iterator.Current.GetHashCode();
                if (length == 2) {
                    // No need to sort if there's two keys, just compare them.
                    return h1 < h2
                        ? ((h1.GetHashCode()) * 397) ^ h2.GetHashCode()
                        : ((h2.GetHashCode()) * 397) ^ h1.GetHashCode();
                }

                // Sort key hash codes and feed to hashList to ensure the aggregate
                // hash code does not depend on the key order.
                var sortedHashes = length == 3
                    ? _tempHashStore3
                    : _tempHashStore4;
                sortedHashes[0] = h1;
                sortedHashes[1] = h2;
                iterator.MoveNext();
                sortedHashes[2] = iterator.Current.GetHashCode();
                if (length == 4) {
                    iterator.MoveNext();
                    sortedHashes[3] = iterator.Current.GetHashCode();
                }

                sortedHashes.Sort();
                var _hashCode = sortedHashes[0].GetHashCode();
                for (var i = 1; i < sortedHashes.Count; i++) {
                    _hashCode = (_hashCode * 397) ^ (sortedHashes[i].GetHashCode());
                }
                return _hashCode;
            }
        }

        public static bool operator ==(KeySet<T> left, KeySet<T> right) {
            return Equals(objA: left, objB: right);
        }

        public static bool operator !=(KeySet<T> left, KeySet<T> right) {
            return !Equals(objA: left, objB: right);
        }
    }

    public class LogicalKeySet : KeySet<LogicalKeyboardKey> {
        public static readonly HashSet<LogicalKeyboardKey> _modifiers = new HashSet<LogicalKeyboardKey> {
            LogicalKeyboardKey.alt,
            LogicalKeyboardKey.control,
            LogicalKeyboardKey.meta,
            LogicalKeyboardKey.shift
        };

        public LogicalKeySet(
            LogicalKeyboardKey key1,
            LogicalKeyboardKey key2 = null,
            LogicalKeyboardKey key3 = null,
            LogicalKeyboardKey key4 = null
        ) : base(key1: key1, key2: key2, key3: key3, key4: key4) {
        }

        public LogicalKeySet(HashSet<LogicalKeyboardKey> keys) : base(keys: keys) {
        }

        public string debugDescribeKeys() {
            var sortedKeys = keys.ToList();
            sortedKeys.Sort(
                (a, b) => {
                    var aIsModifier = a.synonyms.isNotEmpty() || _modifiers.Contains(item: a);
                    var bIsModifier = b.synonyms.isNotEmpty() || _modifiers.Contains(item: b);
                    if (aIsModifier && !bIsModifier) {
                        return -1;
                    }

                    if (bIsModifier && !aIsModifier) {
                        return 1;
                    }

                    return a.debugName.CompareTo(strB: b.debugName);
                }
            );
            var results = LinqUtils<string, LogicalKeyboardKey>.SelectList(sortedKeys, (key => key.debugName));
            return string.Join(" + ", values: results);
        }


        public override void debugFillProperties(DiagnosticPropertiesBuilder properties) {
            base.debugFillProperties(properties: properties);
            properties.add(
                new DiagnosticsProperty<HashSet<LogicalKeyboardKey>>("keys", value: _keys, debugDescribeKeys()));
        }
    }

    public class ShortcutMapProperty : DiagnosticsProperty<Dictionary<LogicalKeySet, Intent>> {
        public ShortcutMapProperty(
            string name,
            Dictionary<LogicalKeySet, Intent> value,
            bool showName = true,
            object defaultValue = null,
            DiagnosticLevel level = DiagnosticLevel.info,
            string description = null
        ) : base(
            name: name,
            value: value,
            showName: showName,
            defaultValue: defaultValue ?? foundation_.kNoDefaultValue,
            level: level,
            description: description
        ) {
        }


        protected override string valueToString(TextTreeConfiguration parentConfiguration = null) {
            var res = new List<string>();
            foreach (var key in valueT.Keys) {
                var temp = "{" + key.debugDescribeKeys() + "}:" + valueT[key: key];
                res.Add(item: temp);
            }

            return string.Join(", ", values: res);
        }
    }

    public class ShortcutManager : DiagnosticableMixinChangeNotifier {
        public readonly bool modal;


        Dictionary<LogicalKeySet, Intent> _shortcuts;

        public ShortcutManager(
            Dictionary<LogicalKeySet, Intent> shortcuts = null,
            bool modal = false
        ) {
            shortcuts = shortcuts ?? new Dictionary<LogicalKeySet, Intent>();
            this.modal = modal;
            this.shortcuts = shortcuts;
        }

        public Dictionary<LogicalKeySet, Intent> shortcuts {
            get { return _shortcuts; }
            set {
                _shortcuts = value;
                notifyListeners();
            }
        }
        
        public virtual bool handleKeypress(
            BuildContext context,
            RawKeyEvent rawKeyEvent,
            LogicalKeySet keysPressed = null
        ) {
            if (!(rawKeyEvent is RawKeyDownEvent)) {
                return false;
            }

            D.assert(context != null);
            //FIX ME !
            //Since we process key event produced by Unity instead of raw input info (physical key)  from os directly,
            //we cannot handle the shortcut key press as in original flutter code
            //TODO: however, we need find out a way to make this work in another way
            return false;

            /*
            LogicalKeySet keySet = keysPressed ?? new LogicalKeySet(RawKeyboard.instance.keysPressed);
            Intent matchedIntent = _shortcuts[keySet];
            if (matchedIntent == null) {
             
              HashSet<LogicalKeyboardKey> pseudoKeys = new HashSet<LogicalKeyboardKey>{};
              foreach (LogicalKeyboardKey setKey in keySet.keys) {
                HashSet<LogicalKeyboardKey> synonyms = setKey.synonyms;
                if (synonyms.isNotEmpty()) {
                 
                  pseudoKeys.Add(synonyms.First());
                } else {
                  pseudoKeys.Add(setKey);
                }
              }
              matchedIntent = _shortcuts[new LogicalKeySet(pseudoKeys)];
            }
            if (matchedIntent != null) {
              BuildContext primaryContext = FocusManagerUtils.primaryFocus?.context;
              if (primaryContext == null) {
                return false;
              }
              return Actions.invoke(primaryContext, matchedIntent, nullOk: true);
            }
            
            return false;
            */
        }

        public override void debugFillProperties(DiagnosticPropertiesBuilder properties) {
            base.debugFillProperties(properties: properties);
            properties.add(new DiagnosticsProperty<Dictionary<LogicalKeySet, Intent>>("shortcuts", value: _shortcuts));
            properties.add(new FlagProperty("modal", value: modal, "modal", defaultValue: false));
        }
    }

    public class Shortcuts : StatefulWidget {
        public readonly Widget child;

        public readonly string debugLabel;

        public readonly ShortcutManager manager;

        public readonly Dictionary<LogicalKeySet, Intent> shortcuts;

        public Shortcuts(
            Key key = null,
            ShortcutManager manager = null,
            Dictionary<LogicalKeySet, Intent> shortcuts = null,
            Widget child = null,
            string debugLabel = null
        ) : base(key: key) {
            this.manager = manager;
            this.shortcuts = shortcuts;
            this.child = child;
            this.debugLabel = debugLabel;
        }

        public static ShortcutManager of(BuildContext context, bool nullOk = false) {
            D.assert(context != null);
            var inherited = context.dependOnInheritedWidgetOfExactType<_ShortcutsMarker>();
            D.assert(() => {
                if (nullOk) {
                    return true;
                }

                if (inherited == null) {
                    throw new UIWidgetsError($"Unable to find a {typeof(Shortcuts)} widget in the context.\n" +
                                             $"{typeof(Shortcuts)}.of()was called with a context that does not contain a " +
                                             $"{typeof(Shortcuts)} widget.\n" +
                                             $"No {typeof(Shortcuts)} ancestor could be found starting from the context that was " +
                                             $"passed to {typeof(Shortcuts)}.of().\n" +
                                             "The context used was:\n" +
                                             $"  {context}");
                }

                return true;
            });
            return inherited?.notifier;
        }


        public override State createState() {
            return new _ShortcutsState();
        }


        public override void debugFillProperties(DiagnosticPropertiesBuilder properties) {
            base.debugFillProperties(properties: properties);
            properties.add(new DiagnosticsProperty<ShortcutManager>("manager", value: manager, defaultValue: null));
            properties.add(new ShortcutMapProperty("shortcuts", value: shortcuts,
                description: debugLabel?.isNotEmpty() ?? false ? debugLabel : null));
        }
    }

    public class _ShortcutsState : State<Shortcuts> {
        ShortcutManager _internalManager;

        public ShortcutManager manager {
            get { return widget.manager ?? _internalManager; }
        }


        public override void dispose() {
            _internalManager?.dispose();
            base.dispose();
        }


        public override void initState() {
            base.initState();
            if (widget.manager == null) {
                _internalManager = new ShortcutManager();
            }

            manager.shortcuts = widget.shortcuts;
        }


        public override void didUpdateWidget(StatefulWidget oldWidget) {
            base.didUpdateWidget((Shortcuts) oldWidget);
            if (widget.manager != ((Shortcuts) oldWidget).manager) {
                if (widget.manager != null) {
                    _internalManager?.dispose();
                    _internalManager = null;
                }
                else {
                    _internalManager = _internalManager ?? new ShortcutManager();
                }
            }

            manager.shortcuts = widget.shortcuts;
        }

        public bool _handleOnKey(FocusNode node, RawKeyEvent _event) {
            if (node.context == null) {
                return false;
            }

            return manager.handleKeypress(context: node.context, rawKeyEvent: _event) || manager.modal;
        }

        public override Widget build(BuildContext context) {
            return new Focus(
                debugLabel: typeof(Shortcuts).ToString(),
                canRequestFocus: false,
                onKey: _handleOnKey,
                child: new _ShortcutsMarker(
                    manager: manager,
                    child: widget.child
                )
            );
        }
    }

    public class _ShortcutsMarker : InheritedNotifier<ShortcutManager> {
        public _ShortcutsMarker(
            ShortcutManager manager = null,
            Widget child = null
        ) : base(notifier: manager, child: child) {
            D.assert(manager != null);
            D.assert(child != null);
        }
    }
}