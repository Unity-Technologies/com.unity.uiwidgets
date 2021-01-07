using System;
using System.Collections.Generic;
using System.Linq;
using Unity.UIWidgets.async2;
using Unity.UIWidgets.cupertino;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.gestures;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.rendering;
using Unity.UIWidgets.service;
using Unity.UIWidgets.ui;
using Unity.UIWidgets.widgets;

namespace Unity.UIWidgets.widgets {
    public enum FocusHighlightMode {
        touch,
        traditional,
    }
    enum FocusHighlightStrategy {
        automatic,
        alwaysTouch,
        alwaysTraditional,
    }

    public enum UnfocusDisposition {
  
        scope,

        previouslyFocusedChild,
    }
    public delegate bool FocusOnKeyCallback(FocusNode node, RawKeyEvent Event);

    public class FocusManagerUtils {
        public static readonly bool _kDebugFocus = false;

        public static bool _focusDebug(string message, List<string> details = null) {
            if (_kDebugFocus) {
                //UnityEngine.Debug.Log();
                UnityEngine.Debug.Log($"FOCUS: {message}");
                if (details != null && details.Count() != 0) {
                    foreach (string detail in details) {
                        UnityEngine.Debug.Log($"    {detail}");
                    }
                }
            }

            return true;
        }

        public static FocusNode primaryFocus {
            get { return WidgetsBinding.instance.focusManager.primaryFocus; }
        }


        /*string debugDescribeFocusTree() {
            D.assert(WidgetsBinding.instance != null);
            string result = null;
            D.assert(() => {
                result = FocusManager.instance.toStringDeep();
                return true;
            }());
            return result ?? "";
        }*/


        /*void debugDumpFocusTree() {
            D.assert(() => {
                UnityEngine.Debug.Log(debugDescribeFocusTree());
                return true;
            });
        }*/
    }

    public class FocusAttachment {
        public FocusAttachment(FocusNode _node) {
            D.assert(_node != null);
            this._node = _node;
        }

        public readonly FocusNode _node;

        bool isAttached {
            get {
                return _node._attachment == this;
            }
        }

        public void detach() {
            D.assert(_node != null);
            D.assert(FocusManagerUtils._focusDebug("Detaching node:", new List<string>{_node.ToString(), $"With enclosing scope {_node.enclosingScope}"}));
            if (isAttached) {
                if (_node.hasPrimaryFocus || (_node._manager != null && _node._manager._markedForFocus == _node)) {
                    _node.unfocus(disposition: UnfocusDisposition.previouslyFocusedChild);
                }
                
                _node._manager?._markDetached(_node);
                _node._parent?._removeChild(_node);
                _node._attachment = null;
                D.assert(!_node.hasPrimaryFocus);
                D.assert(_node._manager?._markedForFocus != _node);
            }
            D.assert(!isAttached);
        }

        public void reparent(FocusNode parent = null) {
            D.assert(_node != null);
            if (isAttached) {
                D.assert(_node.context != null);
                parent = parent ?? Focus.of(_node.context, nullOk: true, scopeOk: true);
                parent = parent ?? _node.context.owner.focusManager.rootScope;
                D.assert(parent != null);
                parent._reparent(_node);
            }
        }
    }

    public class FocusNode : ChangeNotifier {
        //DiagnosticableTreeMixin,
        public FocusNode(
            string debugLabel = "",
            FocusOnKeyCallback onKey = null,
            bool skipTraversal = false,
            bool canRequestFocus = true
        ) {
            D.assert(skipTraversal != null);
            D.assert(canRequestFocus != null);
            _skipTraversal = skipTraversal;
            _canRequestFocus = canRequestFocus;
            /*_onKey = onKey {
                this.debugLabel = debugLabel; ///????
            }*/
        }

        public bool skipTraversal {
            get { return _skipTraversal; }
            set {
                if (value != _skipTraversal) {
                    _skipTraversal = value;
                    _manager?._markPropertiesChanged(this);
                }
            }
        }

        bool _skipTraversal;


        public bool canRequestFocus {
            get {
                FocusScopeNode scope = enclosingScope;
                return _canRequestFocus && (scope == null || scope.canRequestFocus);
            }
            set {
                if (value != _canRequestFocus) {
                    if (!value) {
                        unfocus(disposition: UnfocusDisposition.previouslyFocusedChild);
                    }

                    _canRequestFocus = value;
                    _manager?._markPropertiesChanged(this);
                }
            }

        }

        bool _canRequestFocus;


        public BuildContext context {
            get { return _context; }

        }

        BuildContext _context;

        FocusOnKeyCallback onKey {
            get { return _onKey; }

        }

        FocusOnKeyCallback _onKey;

        public FocusManager _manager;
        List<FocusNode> _ancestors;
        List<FocusNode> _descendants;
        bool _hasKeyboardToken = false;

        FocusNode parent {
            get { return _parent; }

        }

        public FocusNode _parent;


        IEnumerable<FocusNode> children {
            get { return _children; }

        }

        public readonly List<FocusNode> _children = new List<FocusNode>();

        IEnumerable<FocusNode> traversalChildren {
            get {
                if (!canRequestFocus) {
                    return new List<FocusNode>();
                }

                List<FocusNode> nodes = new List<FocusNode>();
                foreach (FocusNode node in children) {
                    if (!node.skipTraversal && node.canRequestFocus)
                        nodes.Add(node);
                }

                return nodes;
            }

        }

        public string debugLabel {
            get { return _debugLabel; }
            set {
                D.assert(() => {
                    // Only set the value in debug builds.
                    _debugLabel = value;
                    return true;
                });
            }

        }

        string _debugLabel;


        public FocusAttachment _attachment;


        public IEnumerable<FocusNode> descendants {

            get {
                if (_descendants == null) {
                    List<FocusNode> result = new List<FocusNode>();
                    foreach (FocusNode child in _children) {
                        foreach (var childDescendant in child.descendants) {
                            result.Add(childDescendant);
                        }

                        //result.addAll(child.descendants);
                        result.Add(child);
                    }

                    _descendants = result;
                }

                return _descendants;
            }

        }

        public IEnumerable<FocusNode> traversalDescendants {
            get {
                List<FocusNode> nodes = new List<FocusNode>();
                foreach (FocusNode node in descendants) {
                    if (!node.skipTraversal && node.canRequestFocus) {
                        nodes.Add(node);
                    }

                }

                // descendants.where((FocusNode node) => !node.skipTraversal && node.canRequestFocus);
                return nodes;
            }

        }

        public IEnumerable<FocusNode> ancestors {
            get {
                if (_ancestors == null) {
                    List<FocusNode> result = new List<FocusNode>();
                    FocusNode parent = _parent;
                    while (parent != null) {
                        result.Add(parent);
                        parent = parent._parent;
                    }

                    _ancestors = result;
                }

                return _ancestors;
            }

        }


        public bool hasFocus {
            get { return hasPrimaryFocus || (_manager?.primaryFocus?.ancestors?.Contains(this) ?? false); }
        }

        public bool hasPrimaryFocus {
            get { return _manager?.primaryFocus == this; }
        }

        /// Returns the [FocusHighlightMode] that is currently in effect for this node.
        FocusHighlightMode highlightMode {
            get { return FocusManager.instance.highlightMode; }
        }

        FocusScopeNode nearestScope {
            get { return enclosingScope; }
        }


        public FocusScopeNode enclosingScope {
            get {
                foreach (FocusNode node in ancestors) {
                    if (node is FocusScopeNode) {
                        return (FocusScopeNode) node;
                    }
                }

                return null;
            }

        }


        Size size {
            get {
                D.assert(
                    context != null, () =>
                        "Tried to get the size of a focus node that didn't have its context set yet.\n" +
                        "The context needs to be set before trying to evaluate traversal policies. This " +
                        "is typically done with the attach method."
                );
                return context.findRenderObject().semanticBounds.size;
            }
        }


        Offset offset {
            get {
                D.assert(
                    context != null, () =>
                        "Tried to get the offset of a focus node that didn't have its context set yet.\n" +
                        "The context needs to be set before trying to evaluate traversal policies. This " +
                        "is typically done with the attach method.");
                RenderObject renderObject = context.findRenderObject();
                return MatrixUtils.transformPoint(renderObject.getTransformTo(null),
                    renderObject.semanticBounds.topLeft);
            }
        }

        Rect rect {
            get {
                D.assert(
                    context != null, () =>
                        "Tried to get the bounds of a focus node that didn't have its context set yet.\n" +
                        "The context needs to be set before trying to evaluate traversal policies. This " +
                        "is typically done with the attach method.");
                RenderObject renderObject = context.findRenderObject();
                Offset globalOffset = MatrixUtils.transformPoint(renderObject.getTransformTo(null),
                    renderObject.semanticBounds.topLeft);
                return globalOffset & renderObject.semanticBounds.size;
            }
        }

        public void unfocus(
            UnfocusDisposition disposition = UnfocusDisposition.scope
        ) {
            D.assert(disposition != null);
            if (!hasFocus && (_manager == null || _manager._markedForFocus != this)) {
                return;
            }

            FocusScopeNode scope = enclosingScope;
            if (scope == null) {

                return;
            }

            switch (disposition) {
                case UnfocusDisposition.scope:
                    if (scope.canRequestFocus) {
                        scope._focusedChildren.Clear();
                    }

                    while (!scope.canRequestFocus) {
                        scope = scope.enclosingScope ?? _manager?.rootScope;
                    }

                    scope?._doRequestFocus(findFirstFocus: false);
                    break;
                case UnfocusDisposition.previouslyFocusedChild:

                    if (scope.canRequestFocus) {
                        scope?._focusedChildren?.Remove(this);
                    }

                    while (!scope.canRequestFocus) {
                        scope.enclosingScope?._focusedChildren?.Remove(scope);
                        scope = scope.enclosingScope ?? _manager?.rootScope;
                    }

                    scope?._doRequestFocus(findFirstFocus: true);
                    break;
            }

            D.assert(FocusManagerUtils._focusDebug("Unfocused node:",
                new List<string> {$"primary focus was {this}", $"next focus will be {_manager?._markedForFocus}"}));
        }


        public bool consumeKeyboardToken() {
            if (!_hasKeyboardToken) {
                return false;
            }

            _hasKeyboardToken = false;
            return true;
        }

        public void _markNextFocus(FocusNode newFocus) {
            if (_manager != null) {
                // If we have a manager, then let it handle the focus change.
                _manager._markNextFocus(this);
                return;
            }

            newFocus?._setAsFocusedChildForScope();
            newFocus?._notify();
            if (newFocus != this) {
                _notify();
            }
        }


        public void _removeChild(FocusNode node, bool removeScopeFocus = true) {
            D.assert(node != null);
            D.assert(_children.Contains(node), () => "Tried to remove a node that wasn't a child.");
            D.assert(node._parent == this);
            D.assert(node._manager == _manager);

            if (removeScopeFocus) {
                node.enclosingScope?._focusedChildren?.Remove(node);
            }

            node._parent = null;
            _children.Remove(node);
            foreach (FocusNode ancestor in ancestors) {
                ancestor._descendants = null;
            }

            _descendants = null;
            D.assert(_manager == null || !_manager.rootScope.descendants.Contains(node));
        }

        void _updateManager(FocusManager manager) {
            _manager = manager;
            foreach (FocusNode descendant in descendants) {
                descendant._manager = manager;
                descendant._ancestors = null;
            }
        }

        public void _reparent(FocusNode child) {
            D.assert(child != null);
            D.assert(child != this, () => "Tried to make a child into a parent of itself.");
            if (child._parent == this) {
                D.assert(_children.Contains(child),
                    () => "Found a node that says it's a child, but doesn't appear in the child list.");
                // The child is already a child of this parent.
                return;
            }

            D.assert(_manager == null || child != _manager.rootScope, () => "Reparenting the root node isn't allowed.");
            D.assert(!ancestors.Contains(child),
                () => "The supplied child is already an ancestor of this node. Loops are not allowed.");
            FocusScopeNode oldScope = child.enclosingScope;
            bool hadFocus = child.hasFocus;
            child._parent?._removeChild(child, removeScopeFocus: oldScope != nearestScope);
            _children.Add(child);
            child._parent = this;
            child._ancestors = null;
            child._updateManager(_manager);
            foreach (FocusNode ancestor in child.ancestors) {
                ancestor._descendants = null;
            }

            if (hadFocus) {
                _manager?.primaryFocus?._setAsFocusedChildForScope();
            }

            if (oldScope != null && child.context != null && child.enclosingScope != oldScope) {
                UnityEngine.Debug.Log(
                    "FocusTraversalGroup.of(child.context, nullOk: true)?.changedScope(node: child, oldScope: oldScope);");
                //FocusTraversalGroup.of(child.context, nullOk: true)?.changedScope(node: child, oldScope: oldScope);
            }

            if (child._requestFocusWhenReparented) {
                child._doRequestFocus(findFirstFocus: true);
                child._requestFocusWhenReparented = false;
            }
        }

        public FocusAttachment attach(BuildContext context, FocusOnKeyCallback onKey = null) {
            _context = context;
            _onKey = onKey ?? _onKey;
            _attachment = new FocusAttachment(this);
            return _attachment;
        }


        public override void dispose() {
            _attachment?.detach();
            base.dispose();
        }


        public void _notify() {
            if (_parent == null) {

                return;
            }

            if (hasPrimaryFocus) {
                _setAsFocusedChildForScope();
            }

            notifyListeners();
        }

        public void requestFocus(FocusNode node = null) {
            if (node != null) {
                if (node._parent == null) {
                    _reparent(node);
                }

                D.assert(node.ancestors.Contains(this),
                    () =>
                        "Focus was requested for a node that is not a descendant of the scope from which it was requested.");
                node._doRequestFocus(findFirstFocus: true);
                return;
            }

            _doRequestFocus(findFirstFocus: true);
        }

        // Note that this is overridden in FocusScopeNode.
        public virtual void _doRequestFocus(bool findFirstFocus = false) {
            D.assert(findFirstFocus != null);
            if (!canRequestFocus) {
                D.assert(FocusManagerUtils._focusDebug(
                    "Node NOT requesting focus because canRequestFocus is false: $this"));
                return;
            }

            if (_parent == null) {
                _requestFocusWhenReparented = true;
                return;
            }

            _setAsFocusedChildForScope();
            if (hasPrimaryFocus && (_manager._markedForFocus == null || _manager._markedForFocus == this)) {
                return;
            }

            _hasKeyboardToken = true;
            D.assert(FocusManagerUtils._focusDebug($"Node requesting focus: {this}"));
            _markNextFocus(this);
        }


        bool _requestFocusWhenReparented = false;


        public void _setAsFocusedChildForScope() {
            FocusNode scopeFocus = this;
            foreach (FocusScopeNode ancestor in ancestors) {
                if (ancestor is FocusScopeNode) {
                    D.assert(scopeFocus != ancestor, () => "Somehow made a loop by setting focusedChild to its scope.");
                    D.assert(FocusManagerUtils._focusDebug($"Setting {scopeFocus} as focused child for scope:",
                        new List<string> {ancestor.ToString()}));
                    ancestor._focusedChildren.Remove(scopeFocus);

                    ancestor._focusedChildren.Add(scopeFocus);
                    scopeFocus = ancestor;
                }
            }
        }


        /*bool nextFocus() {
            return FocusTraversalGroup.of(context).next(this);
        }*
    
    
        bool previousFocus() {
          return FocusTraversalGroup.of(context).previous(this);
        }
    
    
        bool focusInDirection(TraversalDirection direction) {
          return  FocusTraversalGroup.of(context).inDirection(this, direction);
        }*/


        /*public override void debugFillProperties(DiagnosticPropertiesBuilder properties) {
            base.debugFillProperties(properties);
            properties.add(new DiagnosticsProperty<BuildContext>("context", context, defaultValue: null));
            properties.add(new FlagProperty("canRequestFocus", value: canRequestFocus, ifFalse: "NOT FOCUSABLE", defaultValue: true));
            properties.add(new FlagProperty("hasFocus", value: hasFocus && !hasPrimaryFocus, ifTrue: "IN FOCUS PATH", defaultValue: false));
            properties.add(new FlagProperty("hasPrimaryFocus", value: hasPrimaryFocus, ifTrue: "PRIMARY FOCUS", defaultValue: false));
        }*/


        /*public override List<DiagnosticsNode> debugDescribeChildren() {
            int count = 1;
            return _children.map<DiagnosticsNode>((FocusNode child) {
              return child.toDiagnosticsNode(name: "Child ${count++}");
            }).toList();
        }*/


        /*public string toStringShort() {//override
            bool hasDebugLabel = debugLabel != null && debugLabel.isNotEmpty();
            string nullStr = "";
            string extraData = $"{(hasDebugLabel ? debugLabel : nullStr)} " +
                $"{(hasFocus && hasDebugLabel ? nullStr : nullStr)}" + 
                $"{(hasFocus && !hasPrimaryFocus ? "[IN FOCUS PATH]" : nullStr)}"+
                $"{(hasPrimaryFocus ? "[PRIMARY FOCUS]" : nullStr)}";
            return $"{describeIdentity(this)}" + $"{(extraData.isNotEmpty() ? extraData : nullStr)}";
            }
        }*/
    }

    public class FocusScopeNode : FocusNode {
        public FocusScopeNode(
            string debugLabel = null,
            FocusOnKeyCallback onKey = null,
            bool skipTraversal = false,
            bool canRequestFocus = true
        ) : base(
            debugLabel: debugLabel,
            onKey: onKey,
            canRequestFocus: canRequestFocus,
            skipTraversal: skipTraversal
        ) {
            D.assert(skipTraversal != null);
            D.assert(canRequestFocus != null);
            
        }

        public FocusScopeNode nearestScope {
            get { return enclosingScope; }
        }

        public bool isFirstFocus {
            get {
                return enclosingScope.focusedChild == this;
            }
        }


        public FocusNode focusedChild {
            get {
                D.assert(_focusedChildren.isEmpty() || _focusedChildren.Last().enclosingScope == this,()=> "Focused child does not have the same idea of its enclosing scope as the scope does.");
                return _focusedChildren.isNotEmpty() ? _focusedChildren.Last() : null;
            }
        }

        public readonly  List<FocusNode> _focusedChildren = new List<FocusNode>();

        public void setFirstFocus(FocusScopeNode scope) {
            D.assert(scope != null);
            D.assert(scope != this, ()=>"Unexpected self-reference in setFirstFocus.");
            D.assert(FocusManagerUtils._focusDebug("Setting scope as first focus in $this to node:", new List<string>{scope.ToString()}));
            if (scope._parent == null) {
              _reparent(scope);
            }
            D.assert(scope.ancestors.Contains(this), ()=>$"{typeof(FocusScopeNode)}" + $"{scope} must be a child of"+ $" {this} to set it as first focus.");
            if (hasFocus) {
              scope._doRequestFocus(findFirstFocus: true);
            } else {
              scope._setAsFocusedChildForScope();
            }
        }

        public void autofocus(FocusNode node) { 
        D.assert(FocusManagerUtils._focusDebug("Node autofocusing: $node"));
            if (focusedChild == null) {
              if (node._parent == null) {
                _reparent(node);
              }
              D.assert(node.ancestors.Contains(this), ()=>"Autofocus was requested for a node that is not a descendant of the scope from which it was requested.");
              node._doRequestFocus(findFirstFocus: true);
            }
        }

        public override void _doRequestFocus( bool findFirstFocus = false) { 
            D.assert(findFirstFocus != null);

            // It is possible that a previously focused child is no longer focusable.
            while (focusedChild != null && !focusedChild.canRequestFocus)
              _focusedChildren.removeLast();


            if (!findFirstFocus) {
              if (canRequestFocus) {
                _setAsFocusedChildForScope();
                _markNextFocus(this);
              }
              return;
            }

            FocusNode primaryFocus = focusedChild ?? this;

            while (primaryFocus is FocusScopeNode && (( FocusScopeNode)primaryFocus).focusedChild != null) {
              FocusScopeNode scope = primaryFocus as FocusScopeNode;
              primaryFocus = scope.focusedChild;
            }
            if (primaryFocus == this) {
              
              if (primaryFocus.canRequestFocus) {
                _setAsFocusedChildForScope();
                _markNextFocus(this);
              }
            } else {
              
              primaryFocus._doRequestFocus(findFirstFocus: findFirstFocus);
            }
        }

    /*public override void debugFillProperties(DiagnosticPropertiesBuilder properties) {
        base.debugFillProperties(properties);
        if (_focusedChildren.isEmpty()) {
          return;
        } 
        List<string> childList = _focusedChildren.reversed.map<string>((FocusNode child) {
          return child.toStringShort();
        }).toList();
        properties.add(new IEnumerableProperty<string>("focusedChildren", childList, defaultValue: new List<string>()));
    }*/
    }

    public class FocusManager : ChangeNotifier {//DiagnosticableTreeMixin,
        public FocusManager() 
        {
            rootScope._manager = this;
            //RawKeyboard.instance.addListener(_handleRawKeyEvent);
            GestureBinding.instance.pointerRouter.addGlobalRoute(_handlePointerEvent);
        } 
        public static FocusManager instance {
          get { 
              return WidgetsBinding.instance.focusManager;
          } 
        }

        bool _lastInteractionWasTouch = true;


        FocusHighlightStrategy highlightStrategy {
          get {
              return  _highlightStrategy;
          }
          set {
              _highlightStrategy = highlightStrategy;
              _updateHighlightMode();
          }
        }
        FocusHighlightStrategy _highlightStrategy = FocusHighlightStrategy.automatic;

        public FocusHighlightMode highlightMode {
          get {
              return  _highlightMode;
          }
        }
        FocusHighlightMode _highlightMode = FocusHighlightMode.touch;


        void _updateHighlightMode() {
            _lastInteractionWasTouch = false;//(Platform.isAndroid || Platform.isIOS || !WidgetsBinding.instance.mouseTracker.mouseIsConnected);
            FocusHighlightMode newMode = FocusHighlightMode.touch;
            switch (highlightStrategy) {
              case FocusHighlightStrategy.automatic:
                if (_lastInteractionWasTouch) {
                  newMode = FocusHighlightMode.touch;
                } else {
                  newMode = FocusHighlightMode.traditional;
                }
                break;
              case FocusHighlightStrategy.alwaysTouch:
                newMode = FocusHighlightMode.touch;
                break;
              case FocusHighlightStrategy.alwaysTraditional:
                newMode = FocusHighlightMode.traditional;
                break;
            }
            if (newMode != _highlightMode) {
              _highlightMode = newMode;
              //_notifyHighlightModeListeners();
            }
        }

        // The list of listeners for [highlightMode] state changes.
        public  readonly  List<ValueChanged<FocusHighlightMode>> _listeners = new List<ValueChanged<FocusHighlightMode>>();

        public void addHighlightModeListener(ValueChanged<FocusHighlightMode> listener) => _listeners.Add(listener);

        public void removeHighlightModeListener(ValueChanged<FocusHighlightMode> listener) => _listeners?.Remove(listener);

        /*void _notifyHighlightModeListeners() {
            if (_listeners.isEmpty()) {
              return;
            }
            List<ValueChanged<FocusHighlightMode>> localListeners = List<ValueChanged<FocusHighlightMode>>.from(_listeners);
            foreach( ValueChanged<FocusHighlightMode> listener in localListeners) {
              try {
                if (_listeners.Contains(listener)) {
                  listener(_highlightMode);
                }
              } catch (exception, stack) {
                InformationCollector collector;
                D.assert(() =>{
                  collector = () sync* {
                    yield DiagnosticsProperty<FocusManager>(
                      "The $runtimeType sending notification was",
                      this,
                      style: DiagnosticsTreeStyle.errorProperty,
                    );
                  };
                  return true;
                }());
                FlutterError.reportError(FlutterErrorDetails(
                  exception: exception,
                  stack: stack,
                  library: "widgets library",
                  context: ErrorDescription("while dispatching notifications for $runtimeType"),
                  informationCollector: collector,
                ));
              }
            }
        }*/

        public readonly FocusScopeNode rootScope = new FocusScopeNode(debugLabel: "Root Focus Scope");

        void _handlePointerEvent(PointerEvent Event) {
            bool currentInteractionIsTouch = false;
            switch (Event.kind) {
              case PointerDeviceKind.touch:
              case PointerDeviceKind.stylus:
              case PointerDeviceKind.invertedStylus:
                currentInteractionIsTouch = true;
                break;
              case PointerDeviceKind.mouse:
              case PointerDeviceKind.unknown:
                currentInteractionIsTouch = false;
                break;
            }
            if (_lastInteractionWasTouch != currentInteractionIsTouch) {
              _lastInteractionWasTouch = currentInteractionIsTouch;
              _updateHighlightMode();
            }
        }

        /*void _handleRawKeyEvent(RawKeyEvent Event) {

          if (_lastInteractionWasTouch) {
              _lastInteractionWasTouch = false;
              _updateHighlightMode();
          }

          D.assert(FocusManagerUtils._focusDebug($"Received key event {Event.logicalKey}"));

          if (_primaryFocus == null) {
              D.assert(FocusManagerUtils._focusDebug($"No primary focus for key event, ignored: {Event}"));
              return;
          }

          bool handled = false;
          foreach (FocusNode node in List < FocusNode >{
              _primaryFocus, ..._primaryFocus.ancestors
          }) {
              if (node.onKey != null && node.onKey(node, Event)) {
                  D.assert(FocusManagerUtils._focusDebug($"Node {node} handled key event {Event}."));
                  handled = true;
                  break;
              }
          }
          if (!handled) {
              D.assert(FocusManagerUtils._focusDebug($"Key event not handled by anyone: {Event}."));
          }
        }*/


        public FocusNode primaryFocus { 
            get{return _primaryFocus;}
        }
        FocusNode _primaryFocus;


        public readonly HashSet<FocusNode> _dirtyNodes = new HashSet<FocusNode>();

        public FocusNode _markedForFocus;

        public void _markDetached(FocusNode node) {

        D.assert(FocusManagerUtils._focusDebug($"Node was detached: {node}"));
        if (_primaryFocus == node) {
          _primaryFocus = null;
        }
        _dirtyNodes?.Remove(node);
        }

        public void _markPropertiesChanged(FocusNode node) {
            _markNeedsUpdate();
            D.assert(FocusManagerUtils._focusDebug($"Properties changed for node {node}."));
            _dirtyNodes?.Add(node);
            }

            public void _markNextFocus(FocusNode node) {
            if (_primaryFocus == node) {
              
              _markedForFocus = null;
            } else {
              _markedForFocus = node;
              _markNeedsUpdate();
            }
        }

        // True indicates that there is an update pending.
        bool _haveScheduledUpdate = false;

        void _markNeedsUpdate() {
            D.assert(FocusManagerUtils._focusDebug($"Scheduling update, current focus is {_primaryFocus}, next focus will be {_markedForFocus}"));
            if (_haveScheduledUpdate) {
              return;
            }
            _haveScheduledUpdate = true;
            //scheduleMicrotask(_applyFocusChange);
        }
        /*public void reparentIfNeeded(FocusNode node) {
            D.assert(node != null);
            if (node._parent == null || node._parent == this) {
                return;
            }

            node.unfocus();
            D.assert(node._parent == null);
            if (_focus == null) {
                _setFocus(node);
            }
        }*/

        /*void _applyFocusChange() {
            _haveScheduledUpdate = false;
            FocusNode previousFocus = _primaryFocus;
            if (_primaryFocus == null && _markedForFocus == null) {
              
              _markedForFocus = rootScope;
            }
            D.assert(FocusManagerUtils._focusDebug($"Refreshing focus state. Next focus will be {_markedForFocus}"));
            
            if (_markedForFocus != null && _markedForFocus != _primaryFocus) {
                HashSet<FocusNode> previousPath = previousFocus?.ancestors?.toSet() ?? new HashSet<FocusNode>();
              HashSet<FocusNode> nextPath = _markedForFocus.ancestors.toSet();
              // Notify nodes that are newly focused.
              _dirtyNodes.addAll(nextPath.difference(previousPath));
              // Notify nodes that are no longer focused
              _dirtyNodes.addAll(previousPath.difference(nextPath));

              _primaryFocus = _markedForFocus;
              _markedForFocus = null;
            }
            if (previousFocus != _primaryFocus) {
              D.assert(FocusManagerUtils._focusDebug("Updating focus from $previousFocus to $_primaryFocus"));
              if (previousFocus != null) {
                _dirtyNodes.Add(previousFocus);
              }
              if (_primaryFocus != null) {
                _dirtyNodes.Add(_primaryFocus);
              }
            }
            D.assert(FocusManagerUtils._focusDebug("Notifying ${_dirtyNodes.length} dirty nodes:", _dirtyNodes.toList().map<String>((FocusNode node) => node.toString())));
            foreach ( FocusNode node in _dirtyNodes) {
              node._notify();
            }
            _dirtyNodes.Clear();
            if (previousFocus != _primaryFocus) {
              notifyListeners();
            }
            D.assert(()=> {
              if (_kDebugFocus) {
                debugDumpFocusTree();
              }
              return true;
            });
        }*/


        /*public override List<DiagnosticsNode> debugDescribeChildren() {
            return new List<DiagnosticsNode>{
              rootScope.toDiagnosticsNode(name: "rootScope")
              };
        }*/


        /*public  void debugFillProperties(DiagnosticPropertiesBuilder properties) {
            properties.add(new FlagProperty("haveScheduledUpdate", value: _haveScheduledUpdate, ifTrue: "UPDATE SCHEDULED"));
            properties.add(new DiagnosticsProperty<FocusNode>("primaryFocus", primaryFocus, defaultValue: null));
            properties.add(new DiagnosticsProperty<FocusNode>("nextFocus", _markedForFocus, defaultValue: null));
            Element element = primaryFocus?.context as Element;
            if (element != null) {
              properties.add(new DiagnosticsProperty<String>("primaryFocusCreator", element.debugGetCreatorChain(20)));
            }
        }*/
    }

}
/*
    public class FocusNode : ChangeNotifier {
        internal FocusScopeNode _parent;
        internal FocusManager _manager;
        internal bool _hasKeyboardToken = false;

        public bool hasFocus {
            get {
                FocusNode node = null;
                if (_manager != null) {
                    node = _manager._currentFocus;
                }

                return node == this;
            }
        }

        public bool consumeKeyboardToken() {
            if (!_hasKeyboardToken) {
                return false;
            }

            _hasKeyboardToken = false;
            return true;
        }

        public void unfocus() {
            if (_parent != null) {
                _parent._resignFocus(this);
            }

            D.assert(_parent == null);
            D.assert(_manager == null);
        }

        public override void dispose() {
            if (_manager != null) {
                _manager._willDisposeFocusNode(this);
            }

            if (_parent != null) {
                _parent._resignFocus(this);
            }

            D.assert(_parent == null);
            D.assert(_manager == null);
            base.dispose();
        }

        internal void _notify() {
            notifyListeners();
        }

        public override string ToString() {
            return $"{foundation_.describeIdentity(this)} hasFocus: {hasFocus}";
        }
    }

    public class FocusScopeNode : DiagnosticableTree {
        internal FocusManager _manager;
        internal FocusScopeNode _parent;

        internal FocusScopeNode _nextSibling;
        internal FocusScopeNode _previousSibling;

        internal FocusScopeNode _firstChild;
        internal FocusScopeNode _lastChild;

        internal FocusNode _focus;
        internal List<FocusScopeNode> _focusPath;

        public bool isFirstFocus {
            get { return _parent == null || _parent._firstChild == this; }
        }

        internal List<FocusScopeNode> _getFocusPath() {
            List<FocusScopeNode> nodes = new List<FocusScopeNode> {this};
            FocusScopeNode node = _parent;
            while (node != null && node != _manager?.rootScope) {
                nodes.Add(node);
                node = node._parent;
            }

            return nodes;
        }

        internal void _prepend(FocusScopeNode child) {
            D.assert(child != this);
            D.assert(child != _firstChild);
            D.assert(child != _lastChild);
            D.assert(child._parent == null);
            D.assert(child._manager == null);
            D.assert(child._nextSibling == null);
            D.assert(child._previousSibling == null);
            D.assert(() => {
                var node = this;
                while (node._parent != null) {
                    node = node._parent;
                }

                D.assert(node != child);
                return true;
            });
            child._parent = this;
            child._nextSibling = _firstChild;
            if (_firstChild != null) {
                _firstChild._previousSibling = child;
            }

            _firstChild = child;
            _lastChild = _lastChild ?? child;
            child._updateManager(_manager);
        }

        void _updateManager(FocusManager manager) {
            Action<FocusScopeNode> update = null;
            update = (child) => {
                if (child._manager == manager) {
                    return;
                }

                child._manager = manager;
                // We don"t proactively null out the manager for FocusNodes because the
                // manager holds the currently active focus node until the end of the
                // microtask, even if that node is detached from the focus tree.
                if (manager != null && child._focus != null) {
                    child._focus._manager = manager;
                }

                child._visitChildren(update);
            };
            update(this);
        }

        void _visitChildren(Action<FocusScopeNode> vistor) {
            FocusScopeNode child = _firstChild;
            while (child != null) {
                vistor.Invoke(child);
                child = child._nextSibling;
            }
        }

        bool _debugUltimatePreviousSiblingOf(FocusScopeNode child, FocusScopeNode equals) {
            while (child._previousSibling != null) {
                D.assert(child._previousSibling != child);
                child = child._previousSibling;
            }

            return child == equals;
        }

        bool _debugUltimateNextSiblingOf(FocusScopeNode child, FocusScopeNode equals) {
            while (child._nextSibling != null) {
                D.assert(child._nextSibling != child);
                child = child._nextSibling;
            }

            return child == equals;
        }

        internal void _remove(FocusScopeNode child) {
            D.assert(child._parent == this);
            D.assert(child._manager == _manager);
            D.assert(_debugUltimatePreviousSiblingOf(child, equals: _firstChild));
            D.assert(_debugUltimateNextSiblingOf(child, equals: _lastChild));
            if (child._previousSibling == null) {
                D.assert(_firstChild == child);
                _firstChild = child._nextSibling;
            }
            else {
                child._previousSibling._nextSibling = child._nextSibling;
            }

            if (child._nextSibling == null) {
                D.assert(_lastChild == child);
                _lastChild = child._previousSibling;
            }
            else {
                child._nextSibling._previousSibling = child._previousSibling;
            }

            child._previousSibling = null;
            child._nextSibling = null;
            child._parent = null;
            child._updateManager(null);
        }

        internal void _didChangeFocusChain() {
            if (isFirstFocus && _manager != null) {
                _manager._markNeedsUpdate();
            }
        }

        // TODO: need update
        public void requestFocus(FocusNode node = null) {
            // D.assert(node != null);
            var focusPath = _manager?._getCurrentFocusPath();
            if (_focus == node &&
                (_focusPath == focusPath || (focusPath != null && _focusPath != null &&
                                             _focusPath.SequenceEqual(focusPath)))) {
                return;
            }

            if (_focus != null) {
                _focus.unfocus();
            }

            node._hasKeyboardToken = true;
            _setFocus(node);
        }
        
        public void autofocus(FocusNode node) {
            D.assert(node != null);
            if (_focus == null) {
                node._hasKeyboardToken = true;
                _setFocus(node);
            }
        }

        public void reparentIfNeeded(FocusNode node) {
            D.assert(node != null);
            if (node._parent == null || node._parent == this) {
                return;
            }

            node.unfocus();
            D.assert(node._parent == null);
            if (_focus == null) {
                _setFocus(node);
            }
        }

        internal void _setFocus(FocusNode node) {
            D.assert(node != null);
            D.assert(node._parent == null);
            D.assert(_focus == null);
            _focus = node;
            _focus._parent = this;
            _focus._manager = _manager;
            _focus._hasKeyboardToken = true;
            _didChangeFocusChain();
            _focusPath = _getFocusPath();
        }

        internal void _resignFocus(FocusNode node) {
            D.assert(node != null);
            if (_focus != node) {
                return;
            }

            _focus._parent = null;
            _focus._manager = null;
            _focus = null;
            _didChangeFocusChain();
        }

        public void setFirstFocus(FocusScopeNode child) {
            D.assert(child != null);
            D.assert(child._parent == null || child._parent == this);
            if (_firstChild == child) {
                return;
            }

            child.detach();
            _prepend(child);
            D.assert(child._parent == this);
            _didChangeFocusChain();
        }

        public void reparentScopeIfNeeded(FocusScopeNode child) {
            D.assert(child != null);
            if (child._parent == null || child._parent == this) {
                return;
            }

            if (child.isFirstFocus) {
                setFirstFocus(child);
            }
            else {
                child.detach();
            }
        }

        public void detach() {
            _didChangeFocusChain();
            if (_parent != null) {
                _parent._remove(this);
            }

            D.assert(_parent == null);
        }

        public override void debugFillProperties(DiagnosticPropertiesBuilder properties) {
            base.debugFillProperties(properties);
            if (_focus != null) {
                properties.add(new DiagnosticsProperty<FocusNode>("focus", _focus));
            }
        }

        public override List<DiagnosticsNode> debugDescribeChildren() {
            var children = new List<DiagnosticsNode>();
            if (_firstChild != null) {
                FocusScopeNode child = _firstChild;
                int count = 1;
                while (true) {
                    children.Add(child.toDiagnosticsNode(name: $"child {count}"));
                    if (child == _lastChild) {
                        break;
                    }

                    child = child._nextSibling;
                    count += 1;
                }
            }

            return children;
        }
    }

    public class FocusManager {
        public FocusManager() {
            rootScope._manager = this;
            D.assert(rootScope._firstChild == null);
            D.assert(rootScope._lastChild == null);
        }

        public readonly FocusScopeNode rootScope = new FocusScopeNode();
        internal readonly FocusScopeNode _noneScope = new FocusScopeNode();

        public FocusNode currentFocus {
            get { return _currentFocus; }
        }

        internal FocusNode _currentFocus;

        internal void _willDisposeFocusNode(FocusNode node) {
            D.assert(node != null);
            if (_currentFocus == node) {
                _currentFocus = null;
            }
        }

        bool _haveScheduledUpdate = false;

        internal void _markNeedsUpdate() {
            if (_haveScheduledUpdate) {
                return;
            }

            _haveScheduledUpdate = true;
            async_.scheduleMicrotask(() => {
                _update();
                return null;
            });
        }

        internal FocusNode _findNextFocus() {
            FocusScopeNode scope = rootScope;
            while (scope._firstChild != null) {
                scope = scope._firstChild;
            }

            return scope._focus;
        }

        internal void _update() {
            _haveScheduledUpdate = false;
            var nextFocus = _findNextFocus();
            if (_currentFocus == nextFocus) {
                return;
            }

            var previousFocus = _currentFocus;
            _currentFocus = nextFocus;
            if (previousFocus != null) {
                previousFocus._notify();
            }

            if (_currentFocus != null) {
                _currentFocus._notify();
            }
        }

        internal List<FocusScopeNode> _getCurrentFocusPath() {
            return _currentFocus?._parent?._getFocusPath();
        }

        public void focusNone(bool focus) {
            if (focus) {
                if (_noneScope._parent != null && _noneScope.isFirstFocus) {
                    return;
                }

                rootScope.setFirstFocus(_noneScope);
            }
            else {
                if (_noneScope._parent == null) {
                    return;
                }

                _noneScope.detach();
            }
        }

        public override string ToString() {
            var status = _haveScheduledUpdate ? " UPDATE SCHEDULED" : "";
            var indent = "    ";
            return string.Format("{1}{2}\n{0}currentFocus: {3}\n{4}", indent, foundation_.describeIdentity(this),
                status, _currentFocus,
                rootScope.toStringDeep(prefixLineOne: indent, prefixOtherLines: indent));
        }
    }*/
