using System;
using System.Collections.Generic;
using System.Linq;
using Unity.UIWidgets.async;
using Unity.UIWidgets.cupertino;
using Unity.UIWidgets.external;
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


        public static string debugDescribeFocusTree() {
            D.assert(WidgetsBinding.instance != null);
            string result = null;
            D.assert(() => {
                result = FocusManager.instance.toStringDeep();
                return true;
            });
            return result ?? "";
        }


        public static void debugDumpFocusTree() {
            D.assert(() => {
                UnityEngine.Debug.Log(debugDescribeFocusTree());
                return true;
            });
        }
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

    public class FocusNode : DiagnosticableTreeMixinChangeNotifier{
        public FocusNode(
            string debugLabel = "",
            FocusOnKeyCallback onKey = null,
            bool skipTraversal = false,
            bool canRequestFocus = true
        ) {
            _skipTraversal = skipTraversal;
            _canRequestFocus = canRequestFocus;
            _onKey = onKey;
            this.debugLabel = debugLabel;
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

        public FocusOnKeyCallback onKey {
            get { return _onKey; }

        }

        FocusOnKeyCallback _onKey;

        public FocusManager _manager;
        List<FocusNode> _ancestors;
        List<FocusNode> _descendants;
        bool _hasKeyboardToken = false;

        public FocusNode parent {
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
                        result.AddRange(child.descendants);
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

        public virtual FocusScopeNode nearestScope {
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

        public Rect rect {
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
                //UnityEngine.Debug.Log("FocusTraversalGroup.of(child.context, nullOk: true)?.changedScope(node: child, oldScope: oldScope);");
                FocusTraversalGroup.of(child.context, nullOk: true)?.changedScope(node: child, oldScope: oldScope);
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
            if (!canRequestFocus) {
                D.assert(FocusManagerUtils._focusDebug(
                    $"Node NOT requesting focus because canRequestFocus is false: {this}"));
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
            foreach ( var ancestor in ancestors) {
                if (ancestor is FocusScopeNode) {
                    D.assert(scopeFocus != ancestor, () => "Somehow made a loop by setting focusedChild to its scope.");
                    D.assert(FocusManagerUtils._focusDebug($"Setting {scopeFocus} as focused child for scope:",
                        new List<string> {ancestor.ToString()}));
                    ((FocusScopeNode)ancestor)._focusedChildren.Remove(scopeFocus);

                    ((FocusScopeNode)ancestor)._focusedChildren.Add(scopeFocus);
                    scopeFocus = ancestor;
                }
            }
        }
        
        public bool nextFocus() {
            return FocusTraversalGroup.of(context).next(this);
        }
        
        public bool previousFocus() {
          return FocusTraversalGroup.of(context).previous(this);
        }
        
        public bool focusInDirection(TraversalDirection direction) {
          return  FocusTraversalGroup.of(context).inDirection(this, direction);
        }
        
        public override void debugFillProperties(DiagnosticPropertiesBuilder properties) {
            base.debugFillProperties(properties);
            properties.add(new DiagnosticsProperty<BuildContext>("context", context, defaultValue: null));
            properties.add(new FlagProperty("canRequestFocus", value: canRequestFocus, ifFalse: "NOT FOCUSABLE", defaultValue: true));
            properties.add(new FlagProperty("hasFocus", value: hasFocus && !hasPrimaryFocus, ifTrue: "IN FOCUS PATH", defaultValue: false));
            properties.add(new FlagProperty("hasPrimaryFocus", value: hasPrimaryFocus, ifTrue: "PRIMARY FOCUS", defaultValue: false));
        }
        
        public override List<DiagnosticsNode> debugDescribeChildren() {
            int count = 1;
            return LinqUtils<DiagnosticsNode, FocusNode>.SelectList(_children, (FocusNode child) => { 
                return child.toDiagnosticsNode(name: $"Child {count++}");
            });
        }
        
        public override string toStringShort() {//override
            bool hasDebugLabel = debugLabel != null && debugLabel.isNotEmpty();
            string nullStr = "";
            string extraData = $"{(hasDebugLabel ? debugLabel : nullStr)} " +
                $"{(hasFocus && hasDebugLabel ? nullStr : nullStr)}" + 
                $"{(hasFocus && !hasPrimaryFocus ? "[IN FOCUS PATH]" : nullStr)}"+
                $"{(hasPrimaryFocus ? "[PRIMARY FOCUS]" : nullStr)}";
            return $"{foundation_.describeIdentity(this)}" + $"{(extraData.isNotEmpty() ? extraData : nullStr)}";
            }
        }
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
        ) {}

        public override FocusScopeNode nearestScope {
            get { return this; }
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
            D.assert(FocusManagerUtils._focusDebug($"Setting scope as first focus in {this} to node:", new List<string>{scope.ToString()}));
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
        D.assert(FocusManagerUtils._focusDebug($"Node autofocusing: {node}"));
            if (focusedChild == null) {
              if (node._parent == null) {
                _reparent(node);
              }
              D.assert(node.ancestors.Contains(this), ()=>"Autofocus was requested for a node that is not a descendant of the scope from which it was requested.");
              node._doRequestFocus(findFirstFocus: true);
            }
        }

        public override void _doRequestFocus( bool findFirstFocus = false) {
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
        public override void debugFillProperties(DiagnosticPropertiesBuilder properties) {
            base.debugFillProperties(properties);
            if (_focusedChildren.isEmpty()) {
              return;
            } 
            List<string> childList = new List<string>();
            _focusedChildren.Reverse();
            childList = LinqUtils<string,FocusNode>.SelectList(_focusedChildren, (FocusNode child) => { 
                return child.toStringShort();
            });
            properties.add(new EnumerableProperty<string>("focusedChildren", childList, defaultValue: new List<string>()));
        }
    }

    public class FocusManager : DiagnosticableTreeMixinChangeNotifier{
        public FocusManager() 
        { 
            rootScope._manager = this;
            RawKeyboard.instance.addListener(_handleRawKeyEvent);
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
              _notifyHighlightModeListeners();
            }
        }

        // The list of listeners for [highlightMode] state changes.
        public  readonly  List<ValueChanged<FocusHighlightMode>> _listeners = new List<ValueChanged<FocusHighlightMode>>();

        public void addHighlightModeListener(ValueChanged<FocusHighlightMode> listener) => _listeners.Add(listener);

        public void removeHighlightModeListener(ValueChanged<FocusHighlightMode> listener) => _listeners?.Remove(listener);

        void _notifyHighlightModeListeners() { 
            if (_listeners.isEmpty()) {
              return;
            }
            List<ValueChanged<FocusHighlightMode>> localListeners = new List<ValueChanged<FocusHighlightMode>>();
            foreach (var listener in _listeners) {
                localListeners.Add(listener);
            }
            foreach( ValueChanged<FocusHighlightMode> listener in localListeners) {
                if (_listeners.Contains(listener)) {
                    listener(_highlightMode);
                }
              
            }
        }

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

        void _handleRawKeyEvent(RawKeyEvent Event) {

          if (_lastInteractionWasTouch) {
              _lastInteractionWasTouch = false;
              _updateHighlightMode();
          }

          //D.assert(FocusManagerUtils._focusDebug($"Received key event {Event.logicalKey}"));

          if (_primaryFocus == null) {
              D.assert(FocusManagerUtils._focusDebug($"No primary focus for key event, ignored: {Event}"));
              return;
          }

          bool handled = false;
          List<FocusNode> nodes = new List<FocusNode>();
          nodes.Add(_primaryFocus);
          foreach (var node in _primaryFocus.ancestors) {
              nodes.Add(node);
              
          }
          foreach (FocusNode node in nodes) {
              if (node.onKey != null && node.onKey(node, Event)) {
                  D.assert(FocusManagerUtils._focusDebug($"Node {node} handled key event {Event}."));
                  handled = true;
                  break;
              }
          }
          if (!handled) {
              D.assert(FocusManagerUtils._focusDebug($"Key event not handled by anyone: {Event}."));
          }
        }


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
            async_.scheduleMicrotask(()=> {
                _applyFocusChange();
                return null;
            });
        }
        void _applyFocusChange() {
            _haveScheduledUpdate = false;
            FocusNode previousFocus = _primaryFocus;
            if (_primaryFocus == null && _markedForFocus == null) {
              
                _markedForFocus = rootScope;
            }
            D.assert(FocusManagerUtils._focusDebug($"Refreshing focus state. Next focus will be {_markedForFocus}"));
            
            if (_markedForFocus != null && _markedForFocus != _primaryFocus) {
                HashSet<FocusNode> previousPath = previousFocus?.ancestors != null ? new HashSet<FocusNode>(previousFocus.ancestors) : new HashSet<FocusNode>();
                HashSet<FocusNode> nextPath = new HashSet<FocusNode>(_markedForFocus.ancestors);
                foreach(FocusNode node in FocusTravesalUtils.difference(nextPath,previousPath)) {
                    _dirtyNodes.Add(node);
                }
                foreach(FocusNode node in FocusTravesalUtils.difference(previousPath,nextPath)) {
                    _dirtyNodes.Add(node);
                }

                _primaryFocus = _markedForFocus;
                _markedForFocus = null;
            }
            if (previousFocus != _primaryFocus) {
                D.assert(FocusManagerUtils._focusDebug($"Updating focus from {previousFocus} to {_primaryFocus}"));
                if (previousFocus != null) {
                    _dirtyNodes.Add(previousFocus);
                }
                if (_primaryFocus != null) {
                    _dirtyNodes.Add(_primaryFocus);
                }
            }
            D.assert(FocusManagerUtils._focusDebug($"Notifying {_dirtyNodes.Count} dirty nodes:",
                LinqUtils<string,FocusNode>.SelectList(_dirtyNodes, ((FocusNode node) => {
                    return node.toString();
                }))
            ));
            foreach ( FocusNode node in _dirtyNodes) {
                node._notify();
            }
            _dirtyNodes.Clear();
            if (previousFocus != _primaryFocus) {
                notifyListeners();
            }
            D.assert(()=> {
                if (FocusManagerUtils._kDebugFocus) {
                    FocusManagerUtils.debugDumpFocusTree();
                }
                return true;
            });
        }
        


        public override List<DiagnosticsNode> debugDescribeChildren() {
            return new List<DiagnosticsNode>{
              rootScope.toDiagnosticsNode(name: "rootScope")
              };
        }


        public override void debugFillProperties(DiagnosticPropertiesBuilder properties) {
            properties.add(new FlagProperty("haveScheduledUpdate", value: _haveScheduledUpdate, ifTrue: "UPDATE SCHEDULED"));
            properties.add(new DiagnosticsProperty<FocusNode>("primaryFocus", primaryFocus, defaultValue: null));
            properties.add(new DiagnosticsProperty<FocusNode>("nextFocus", _markedForFocus, defaultValue: null));
            Element element = primaryFocus?.context as Element;
            if (element != null) {
              properties.add(new DiagnosticsProperty<string>("primaryFocusCreator", element.debugGetCreatorChain(20)));
            }
        }
    }

