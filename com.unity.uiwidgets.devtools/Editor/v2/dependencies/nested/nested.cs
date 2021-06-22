using System;
using System.Collections.Generic;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.widgets;

namespace Unity.UIWidgets.DevTools
{
    public class Nested : StatelessWidget
    {
        public Nested(
            Key key = null,
            List<SingleChildWidgetMixinStatelessWidget> children = null,
            Widget child = null
        ) : base(key: key)
        {
            // D.assert(children != null && children.isNotEmpty());
            this._children = children;
            this._child = child;
        }
        
        public readonly List<SingleChildWidgetMixinStatelessWidget> _children;
        public readonly Widget _child;

        public override Widget build(BuildContext context)
        {
            throw new Exception("implemented internally");
        }

        public override Element createElement()
        {
            return new _NestedElement(this);
        }
    }
    
    public class _NestedElement : StatelessElement {
        public _NestedElement(Nested widget) : base(widget)
        {
        }
        
        //SingleChildWidgetElementMixin
        _NestedHookElement _parent;

        public override void mount(Element parent, object newSlot)
        {
            if (parent is _NestedHookElement) {
                _parent = parent as _NestedHookElement;
            }
            base.mount(parent, newSlot);
        }

        public override void activate() {
            base.activate();
            visitAncestorElements((parent) => {
                if (parent is _NestedHookElement) {
                    _parent = parent as _NestedHookElement;
                }
                return false;
            });
        }
        //end of SingleChildWidgetElementMixin

        public readonly List<_NestedHookElement> nodes = new List<_NestedHookElement>();
        
        Nested nested_widget => base.widget as Nested;

        protected override Widget build()
        {
            _NestedHook nestedHook = null;
            var nextNode = _parent?.injectedChild ?? nested_widget._child;

            for (int i= nested_widget._children.Count - 1; i >=0 ; i--)
            {
                var child = nested_widget._children[i];
                nextNode = nestedHook = new _NestedHook(
                    owner: this,
                    wrappedWidget: child,
                    injectedChild: nextNode
                );
            }

            if (nestedHook != null) {
                foreach (var node in nodes) { 
                    node.wrappedChild = nestedHook.wrappedWidget;
                    node.injectedChild = nestedHook.injectedChild;

                    var next = nestedHook.injectedChild;
                    if (next is _NestedHook) {
                        nestedHook = next as _NestedHook;
                    } else {
                        break;
                    }
                }
            }

            return nextNode;
        }
    }

    public class _NestedHook : StatelessWidget
    {
        public _NestedHook(
            Widget injectedChild = null,
            SingleChildWidgetMixinStatelessWidget wrappedWidget = null,
            _NestedElement owner = null
            )
        {
            D.assert(wrappedWidget != null);
            D.assert(owner != null);
            this.injectedChild = injectedChild;
            this.wrappedWidget = wrappedWidget;
            this.owner = owner;
        }
        
        public readonly SingleChildWidgetMixinStatelessWidget wrappedWidget;
        public readonly Widget injectedChild;
        public readonly _NestedElement owner;

        public override Element createElement() => new _NestedHookElement(this);

        public override Widget build(BuildContext context) => throw new Exception("handled internally");
    }
    
    public class _NestedHookElement : StatelessElement {
    public _NestedHookElement(_NestedHook widget) : base(widget){}
    
    _NestedHook nested_widget => base.widget as _NestedHook;

    Widget _injectedChild;

    internal Widget injectedChild
    {
        get { return _injectedChild; }
        set
        {
            var previous = _injectedChild;
            if (value is _NestedHook &&
                previous is _NestedHook &&
                Widget.canUpdate(((_NestedHook)value).wrappedWidget, ((_NestedHook)previous).wrappedWidget)) {
                return;
            }
            if (previous != value) {
                _injectedChild = value;
                visitChildren((e) => e.markNeedsBuild());
            }
        }
    }

    SingleChildWidgetMixinStatelessWidget _wrappedChild;

    internal SingleChildWidgetMixinStatelessWidget wrappedChild
    {
        get { return _wrappedChild; }
        set
        {
            if (_wrappedChild != value) {
                _wrappedChild = value;
                markNeedsBuild();
            }
        }
    }

    public override void mount(Element parent, object newSlot) {
        nested_widget.owner.nodes.Add(this);
        _wrappedChild = nested_widget.wrappedWidget;
        _injectedChild = nested_widget.injectedChild;
        base.mount(parent, newSlot);
    }

    public override void unmount() {
        nested_widget.owner.nodes.Remove(this);
        base.unmount();
    }

    protected override Widget build() {
        return wrappedChild;
    }
    }
    

    public abstract class SingleChildWidget : Widget {
        public abstract override Element createElement();
    }

    public abstract class SingleChildStatelessWidget : SingleChildWidgetMixinStatelessWidget
    {
        public SingleChildStatelessWidget(Key key = null, Widget child = null) : base(key : key)
        {
            _child = child;
        }
        
        public readonly Widget _child;
        
        protected internal abstract Widget buildWithChild(BuildContext context, Widget child);

        public override Widget build(BuildContext context) => buildWithChild(context, _child);

        public override Element createElement() {
            return new SingleChildStatelessElement(this);
        }
    }
    
    class SingleChildStatelessElement : StatelessElement {
        public SingleChildStatelessElement(SingleChildStatelessWidget widget)
            : base(widget)
        {
            
        }
        
        //SingleChildWidgetElementMixin
        _NestedHookElement _parent;

        public override void mount(Element parent, object newSlot)
        {
            if (parent is _NestedHookElement) {
                _parent = parent as _NestedHookElement;
            }
            base.mount(parent, newSlot);
        }

        public override void activate() {
            base.activate();
            visitAncestorElements((parent) => {
                if (parent is _NestedHookElement) {
                    _parent = parent as _NestedHookElement;
                }
                return false;
            });
        }
        //end of SingleChildWidgetElementMixin

        protected override Widget build() {
        if (_parent != null) {
            return nested_widget.buildWithChild(this, _parent.injectedChild);
        }
        return base.build();
    }
        
        SingleChildStatelessWidget nested_widget =>
        base.widget as SingleChildStatelessWidget;
    }

    public abstract class SingleChildStatefulWidget : StatefulWidget {
        public SingleChildStatefulWidget(Key key = null, Widget child = null) : base(key: key)
        {
            _child = child;
        }

        public readonly Widget _child;

        public override Element createElement() {
        return new SingleChildStatefulElement(this);
    }
    }

    public abstract class SingleChildState<T> : State<T> where T : SingleChildStatefulWidget
    {
        protected internal abstract Widget buildWithChild(BuildContext context, Widget child);

        public override Widget build(BuildContext context) => buildWithChild(context, widget._child);
    }

    public class SingleChildStatefulElement : StatefulElement
    {
        public SingleChildStatefulElement(SingleChildStatefulWidget widget) : base(widget)
        {
        }
        
        //SingleChildWidgetElementMixin
        _NestedHookElement _parent;

        public override void mount(Element parent, object newSlot)
        {
            if (parent is _NestedHookElement) {
                _parent = parent as _NestedHookElement;
            }
            base.mount(parent, newSlot);
        }

        public override void activate() {
            base.activate();
            visitAncestorElements((parent) => {
                if (parent is _NestedHookElement) {
                    _parent = parent as _NestedHookElement;
                }
                return false;
            });
        }
        //end of SingleChildWidgetElementMixin
        
        SingleChildStatefulWidget nested_widget =>
        base.widget as SingleChildStatefulWidget;
        
            SingleChildState<SingleChildStatefulWidget> nested_state =>
            base.state as SingleChildState<SingleChildStatefulWidget>;

            protected override Widget build() {
            if (_parent != null) {
                return nested_state.buildWithChild(this, _parent.injectedChild);
            }
            return base.build();
        }
    }
}