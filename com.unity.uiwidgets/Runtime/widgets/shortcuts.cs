using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.service;
using Unity.UIWidgets.services;

namespace Unity.UIWidgets.widgets {

    public class KeySet<T> : KeyboardKey, IEquatable<KeySet<T>> 
    {  
        public KeySet(
            T key1,
            T key2 = default(T),
            T key3 = default(T),
            T key4 = default(T)
        ) {
            D.assert(key1 != null);
            _keys = new HashSet<T>();
            _keys.Add(key1);
            int count = 1;
            if (key2 != null) {
                _keys.Add(key2);
                D.assert(() => {
                    count++;
                    return true;
                });
            }

            if (key3 != null) {
                _keys.Add(key3);
                D.assert(() => {
                    count++;
                    return true;
                });
            }

            if (key4 != null) {
                _keys.Add(key4);
                D.assert(() => {
                    count++;
                    return true;
                });
            }

            D.assert(_keys.Count == count,
                () => "Two or more provided keys are identical. Each key must appear only once.");
        }

        public KeySet (HashSet<T> keys) 
        {
            D.assert(keys != null);
            D.assert(keys.isNotEmpty);
            D.assert(!keys.Contains(default(T)));
            foreach (var key in keys) {
                _keys.Add(key);
            }
            
        }

        public HashSet<T> keys 
        {
            get { return new HashSet<T>(_keys); }
        }
        public readonly HashSet<T> _keys;
        
        
        public static readonly List<int> _tempHashStore3 = new List<int>() {0, 0, 0}; // used to sort exactly 3 keys
        public static readonly List<int> _tempHashStore4 = new List<int>() {0, 0, 0, 0}; // used to sort exactly 4 keys
        
        public int _hashCode;


        public bool Equals(KeySet<T> other)
        {
            if (ReferenceEquals(null, other)) {
                return false;
            }

            if (ReferenceEquals(this, other)) {
                return true;
            }

            return Equals(_keys, other._keys) && _hashCode == other._hashCode;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) {
                return false;
            }

            if (ReferenceEquals(this, obj)) {
                return true;
            }

            if (obj.GetType() != GetType()) {
                return false;
            }

            return Equals((KeySet<T>) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                if (_hashCode != 0) {
                    return _hashCode;
                }

                int length = _keys.Count;
                IEnumerator<T> iterator = _keys.GetEnumerator();

                // There's always at least one key. Just extract it.
                iterator.MoveNext();
                int h1 = iterator.GetHashCode();

                if (length == 1) {
                    // Don't do anything fancy if there's exactly one key.
                    return _hashCode = h1;
                }

                iterator.MoveNext();
                int h2 = iterator.GetHashCode();
                if (length == 2) {
                    // No need to sort if there's two keys, just compare them.
                    return _hashCode = h1 < h2
                        ? ((h1 != null ? h1.GetHashCode() : 0) * 397) ^ h2.GetHashCode()
                        : ((h2 != null ? h2.GetHashCode() : 0) * 397) ^ h1.GetHashCode();
                }

                // Sort key hash codes and feed to hashList to ensure the aggregate
                // hash code does not depend on the key order.
                List<int> sortedHashes = length == 3
                    ? _tempHashStore3
                    : _tempHashStore4;
                sortedHashes[0] = h1;
                sortedHashes[1] = h2;
                iterator.MoveNext();
                sortedHashes[2] = iterator.GetHashCode();
                if (length == 4) {
                    iterator.MoveNext();
                    sortedHashes[3] = iterator.GetHashCode();
                }
                sortedHashes.Sort();
                return _hashCode  = (_hashCode * 397) ^ (sortedHashes != null ? sortedHashes.GetHashCode() : 0);
                //return ((_keys != null ? _keys.GetHashCode() : 0) * 397) ^ _hashCode;
            }
        }
        public static bool operator ==(KeySet<T> left, KeySet<T> right) {
            return Equals(left, right);
        }

        public static bool operator !=(KeySet<T> left, KeySet<T> right) {
            return !Equals(left, right);
        }

    }
    
    public class LogicalKeySet : KeySet<LogicalKeyboardKey> {

        public LogicalKeySet(
            LogicalKeyboardKey key1,
            LogicalKeyboardKey key2 = null,
            LogicalKeyboardKey key3 = null,
            LogicalKeyboardKey key4 = null
        ) : base(key1: key1, key2: key2, key3: key3, key4: key4){ }
        
        public LogicalKeySet(HashSet<LogicalKeyboardKey> keys) : base(keys) { }

        public static readonly HashSet<LogicalKeyboardKey> _modifiers = new HashSet<LogicalKeyboardKey>(){
            LogicalKeyboardKey.alt,
            LogicalKeyboardKey.control,
            LogicalKeyboardKey.meta,
            LogicalKeyboardKey.shift,
        };

        public string debugDescribeKeys() { 
            List<LogicalKeyboardKey> sortedKeys = keys.ToList();
            sortedKeys.Sort(
             (LogicalKeyboardKey a, LogicalKeyboardKey b) => { 
                 bool aIsModifier = a.synonyms.isNotEmpty() || _modifiers.Contains(a);
                 bool bIsModifier = b.synonyms.isNotEmpty() || _modifiers.Contains(b);
                 if (aIsModifier && !bIsModifier) {
                     return -1;
                 } else if (bIsModifier && !aIsModifier) {
                     return 1;
                 }
                 return a.debugName.CompareTo(b.debugName); 
             }
            ); 
            var results = sortedKeys.Select((LogicalKeyboardKey key) => key.debugName.ToString());
            return string.Join(" + ",results);
         
      }
      
      
      public override void debugFillProperties(DiagnosticPropertiesBuilder properties) {
        base.debugFillProperties(properties);
        properties.add(new DiagnosticsProperty<HashSet<LogicalKeyboardKey>>("keys", _keys, description: debugDescribeKeys()));
      }
    }
    
    public class ShortcutMapProperty : DiagnosticsProperty<Dictionary<LogicalKeySet, Intent>> {

        public ShortcutMapProperty(
            string name,
            Dictionary<LogicalKeySet, Intent> value,
            bool showName = true,
            object defaultValue =  null,
            DiagnosticLevel level = DiagnosticLevel.info,
            string description = null
        ) : base(
                name: name,
                value: value,
                showName: showName,
                defaultValue: defaultValue ?? foundation_.kNoDefaultValue,
                level: level,
                description: description
            ) 
            {
               
            }

    
        protected override string valueToString( TextTreeConfiguration parentConfiguration = null) {
            
            List<string> res = new List<string>();
            foreach (var key in value.Keys) {
                string temp = "{" + key.debugDescribeKeys() + "}:" + value[key];
                res.Add(temp);
            }
            return string.Join(", ", res);
        }
    }
    
    public class ShortcutManager : DiagnosticableMixinChangeNotifier {

      public ShortcutManager(
        Dictionary<LogicalKeySet, Intent> shortcuts = null,
        bool modal = false
      ) {
          shortcuts = shortcuts ?? new Dictionary<LogicalKeySet, Intent>();
          this.modal = modal;
          this.shortcuts = shortcuts;
        }

      public readonly bool modal;


      Dictionary<LogicalKeySet, Intent> _shortcuts;
    
      public Dictionary<LogicalKeySet, Intent>  shortcuts {
          get { return _shortcuts; }
          set {
              _shortcuts = value;
              notifyListeners();
          }
      }

      
      public bool handleKeypress(
        BuildContext context,
        RawKeyEvent rawKeyEvent,
        LogicalKeySet keysPressed = null
      ) {
        if (!(rawKeyEvent is RawKeyDownEvent)) {
          return false;
        }
        D.assert(context != null);
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
      }

      public override void debugFillProperties(DiagnosticPropertiesBuilder properties) {
          base.debugFillProperties(properties: properties);
          properties.add(new DiagnosticsProperty<Dictionary<LogicalKeySet, Intent>>("shortcuts", _shortcuts));
          properties.add(new FlagProperty("modal", value: modal, ifTrue: "modal", defaultValue: false));
      }
    }
      
    public class Shortcuts : StatefulWidget {
      
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
      
      public readonly ShortcutManager manager;
      
      public readonly Dictionary<LogicalKeySet, Intent> shortcuts;
      
      public readonly Widget child;
      
      public readonly string debugLabel;

      public static ShortcutManager of(BuildContext context, bool nullOk = false) {
        D.assert(context != null);
        _ShortcutsMarker inherited = context.dependOnInheritedWidgetOfExactType<_ShortcutsMarker>();
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
        base.debugFillProperties(properties);
        properties.add(new DiagnosticsProperty<ShortcutManager>("manager", manager, defaultValue: null));
        properties.add(new ShortcutMapProperty("shortcuts", shortcuts, description: debugLabel?.isNotEmpty() ?? false ? debugLabel : null));
      }
    }

    public class _ShortcutsState : State<Shortcuts> {
        
      private ShortcutManager _internalManager;
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
        base.didUpdateWidget((Shortcuts)oldWidget);
        if (widget.manager != ((Shortcuts)oldWidget).manager) {
          if (widget.manager != null) {
            _internalManager?.dispose();
            _internalManager = null;
          } else {
            _internalManager = _internalManager?? new ShortcutManager();
          }
        }
        manager.shortcuts = widget.shortcuts;
      }

      public bool _handleOnKey(FocusNode node, RawKeyEvent _event)
      {
        if (node.context == null) {
          return false;
        }
        return manager.handleKeypress(node.context, _event) || manager.modal;
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