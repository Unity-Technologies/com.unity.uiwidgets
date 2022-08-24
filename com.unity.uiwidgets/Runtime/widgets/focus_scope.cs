using System.Collections.Generic;
using Unity.UIWidgets.foundation;

namespace Unity.UIWidgets.widgets {

    public class FocusScope : Focus {
        public FocusScope(
            Key key = null,
            FocusScopeNode node = null,
            Widget child = null,
            bool autofocus = false,
            ValueChanged<bool> onFocusChange = null,
            bool? canRequestFocus = null,
            bool? skipTraversal = null,
            FocusOnKeyCallback onKey = null, 
            string debugLabel = null
        ) : base(
            key: key,
            child: child,
            focusNode: node,
            autofocus: autofocus,
            onFocusChange: onFocusChange,
            canRequestFocus: canRequestFocus,
            skipTraversal: skipTraversal,
            onKey: onKey,
            debugLabel: debugLabel) {
            D.assert(child != null);
        }
        public static FocusScopeNode of(BuildContext context) {
            D.assert(context != null);
            _FocusMarker marker = context.dependOnInheritedWidgetOfExactType<_FocusMarker>();
            return marker?.notifier?.nearestScope ?? context.owner.focusManager.rootScope;
        }
        public override State createState() {
            return new _FocusScopeState();
        }
    }
    class _FocusScopeState : _FocusState {
        protected override FocusNode _createNode() {
            return new FocusScopeNode(
                debugLabel: widget.debugLabel,
                canRequestFocus: widget.canRequestFocus ?? true,
                skipTraversal: widget.skipTraversal ?? false
            );
        }
        public override Widget build(BuildContext context) {
            _focusAttachment.reparent();
            return new _FocusMarker(
                node: focusNode,
                child: widget.child);
        }
    }
    class _FocusMarker : InheritedNotifier<FocusNode> {
        public _FocusMarker(
            Key key = null,
            FocusNode node = null,
            Widget child = null
        ) : base(key: key, notifier: node, child: child) {
            D.assert(node != null);
            D.assert(child != null);
        }
    }
    
    public class Focus : StatefulWidget { 
        public Focus(
            Key key = null,
            Widget child = null,
            FocusNode focusNode = null,
            bool autofocus = false,
            ValueChanged<bool> onFocusChange = null,
            FocusOnKeyCallback onKey = null,
            string debugLabel = null,
            bool? canRequestFocus= null,
            bool? skipTraversal = null,
            bool includeSemantics = true
          )  :base(key:key) {
            D.assert(child != null);
            this.child = child;
            this.focusNode = focusNode;
            this.autofocus = autofocus;
            this.onFocusChange = onFocusChange;
            this.onKey = onKey;
            this.debugLabel = debugLabel;
            this.canRequestFocus = canRequestFocus;
            this.skipTraversal = skipTraversal;
            this.includeSemantics = includeSemantics;
        }

        public readonly string debugLabel;
        public readonly Widget child;
        public readonly FocusOnKeyCallback onKey; 
        public readonly ValueChanged<bool> onFocusChange;
        public readonly bool autofocus;
        public readonly FocusNode focusNode;
        public readonly bool? skipTraversal;
        public readonly bool includeSemantics;
        public readonly bool? canRequestFocus;

        public static FocusNode of(BuildContext context,  bool nullOk = false, bool scopeOk = false ) {
            D.assert(context != null);
            _FocusMarker marker = context.dependOnInheritedWidgetOfExactType<_FocusMarker>();
            FocusNode node = marker?.notifier;
            if (node == null) {
              if (!nullOk) {
                  throw new UIWidgetsError(
                  "Focus.of() was called with a context that does not contain a Focus widget.\n"+
                  "No Focus widget ancestor could be found starting from the context that was passed to "+
                  "Focus.of(). This can happen because you are using a widget that looks for a Focus "+
                  "ancestor, and do not have a Focus widget descendant in the nearest FocusScope.\n"+
                  "The context used was:\n"+
                  $"  {context}"
                );
              }
              return null;
            }
            if (!scopeOk && node is FocusScopeNode) {
              if (!nullOk) {
                  throw new UIWidgetsError(
                  "Focus.of() was called with a context that does not contain a Focus between the given "+
                  "context and the nearest FocusScope widget.\n"+
                  "No Focus ancestor could be found starting from the context that was passed to "+
                  "Focus.of() to the point where it found the nearest FocusScope widget. This can happen "+
                  "because you are using a widget that looks for a Focus ancestor, and do not have a "+
                  "Focus widget ancestor in the current FocusScope.\n"+
                  "The context used was:\n"+
                  $"  {context}"
                );
              }
              return null;
            }
            return node;
          }
        public static bool isAt(BuildContext context) => Focus.of(context, nullOk: true)?.hasFocus ?? false;
        public override void debugFillProperties(DiagnosticPropertiesBuilder properties) {
            base.debugFillProperties(properties);
            properties.add(new StringProperty("debugLabel", debugLabel, defaultValue: null));
            properties.add(new FlagProperty("autofocus", value: autofocus, ifTrue: "AUTOFOCUS", defaultValue: false));
            properties.add(new DiagnosticsProperty<FocusNode>("node", focusNode, defaultValue: null));
        }

        public override State createState() {
            return new _FocusState();
        }
    }
    
    public class _FocusState : State<Focus> { 
        FocusNode _internalNode;

        public FocusNode focusNode { 
            get { 
                return widget.focusNode ?? _internalNode;
            }
        }
        bool _hasPrimaryFocus;
        bool _canRequestFocus;
        bool _didAutofocus = false;
        public FocusAttachment _focusAttachment;
        public override void initState() {
            base.initState();
            _initNode();
        }
        public void _initNode() { 
            if (widget.focusNode == null) { 
                _internalNode = _internalNode ?? _createNode();
            }
            if (widget.skipTraversal != null) { 
                focusNode.skipTraversal = widget.skipTraversal.Value;
            }
            if (widget.canRequestFocus != null) { 
                focusNode.canRequestFocus =  widget.canRequestFocus.Value;
            }
            _canRequestFocus = focusNode.canRequestFocus;
            _hasPrimaryFocus = focusNode.hasPrimaryFocus;
            _focusAttachment = focusNode.attach(context, onKey: widget.onKey);
            focusNode.addListener(_handleFocusChanged);
         }
        protected virtual FocusNode _createNode() { 
            return new FocusNode(
            debugLabel: widget.debugLabel,
            canRequestFocus: widget.canRequestFocus ?? true,
            skipTraversal: widget.skipTraversal ?? false
            );
        }
        public override void dispose() {
            focusNode.removeListener(_handleFocusChanged); 
            _focusAttachment.detach();
            _internalNode?.dispose();
            base.dispose();
        }
        public override void didChangeDependencies() {
            base.didChangeDependencies();
            _focusAttachment?.reparent();
            _handleAutofocus();
        }
        void _handleAutofocus() {
            if (!_didAutofocus && widget.autofocus) {
              FocusScope.of(context).autofocus(focusNode);
              _didAutofocus = true;
            }
        }

        public override void deactivate() {
            base.deactivate();
            
            _focusAttachment?.reparent();
            _didAutofocus = false;
        }

        public override void didUpdateWidget(StatefulWidget oldWidget) {
            oldWidget = (Focus) oldWidget;
            base.didUpdateWidget(oldWidget);
            D.assert(()=> {
                if (((Focus)oldWidget).debugLabel != widget.debugLabel && _internalNode != null) {
                _internalNode.debugLabel = widget.debugLabel;
              }
              return true;
            });

            if (((Focus)oldWidget).focusNode == widget.focusNode) {
              if (widget.skipTraversal != null) {
                focusNode.skipTraversal = widget.skipTraversal.Value;
              }
              if (widget.canRequestFocus != null) {
                focusNode.canRequestFocus = widget.canRequestFocus.Value;
              }
            } else {
              _focusAttachment.detach();
              focusNode.removeListener(_handleFocusChanged);
              _initNode();
            }

            if (((Focus)oldWidget).autofocus != widget.autofocus) {
              _handleAutofocus();
            }
        }
        void _handleFocusChanged() { 
            bool hasPrimaryFocus = focusNode.hasPrimaryFocus;
            bool canRequestFocus = focusNode.canRequestFocus;
            if (widget.onFocusChange != null) {
              widget.onFocusChange(focusNode.hasFocus);
            }
            if (_hasPrimaryFocus != hasPrimaryFocus) {
              setState(() =>{
                _hasPrimaryFocus = hasPrimaryFocus;
              });
            }
            if (_canRequestFocus != canRequestFocus) {
              setState(() =>{
                _canRequestFocus = canRequestFocus;
              });
            }
        }
        public override Widget build(BuildContext context) {
            _focusAttachment.reparent();
            Widget child = widget.child;
            return new _FocusMarker(
              node: focusNode,
              child: child
            );
        }
    }

    

    
}