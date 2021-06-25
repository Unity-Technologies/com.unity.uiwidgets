using System;
using System.Collections.Generic;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.rendering;
using Unity.UIWidgets.ui;
using Unity.UIWidgets.widgets;

namespace Unity.UIWidgets.DevTools
{
    public delegate bool UpdateShouldNotify<T>(T previous, T current);

    public delegate T Create<T>(BuildContext context);

    public delegate T Update<T>(BuildContext context, T value);
    public delegate void Dispose<T>(BuildContext context, T value);

    public delegate void DebugCheckInvalidValueType<T>(T value);
    public delegate VoidCallback StartListening<T>(InheritedContext<T> element, T value);
    
    public class InheritedProvider<T> : SingleChildStatelessWidget
    {
        public InheritedProvider(
            Key key = null,
            Create<T> create = null,
            Update<T> update = null,
            UpdateShouldNotify<T> updateShouldNotify = null,
            DebugCheckInvalidValueType<T> debugCheckInvalidValueType = null,
            StartListening<T> startListening = null, 
            Dispose<T> dispose = null,
            TransitionBuilder builder = null,
            bool? lazy = null, 
            Widget child = null,
            _Delegate<T> _delegate = null
        ) :
        base(key: key, child: child)
        {
            this._lazy = lazy;
            this._delegate = _delegate?? new _CreateInheritedProvider<T>(
                create: create,
                update: update,
                updateShouldNotify: updateShouldNotify,
                debugCheckInvalidValueType: debugCheckInvalidValueType,
                startListening: startListening,
                dispose: dispose
            );
            this.builder = builder;
        }
        
        public readonly _Delegate<T> _delegate;
        public readonly bool? _lazy;
        public readonly TransitionBuilder builder;

        public InheritedProvider(
            Key key,
            _Delegate<T> _delegate,
            bool? lazy,
            TransitionBuilder builder,
            Widget child
        ):
            base(key: key, child: child)
        {
            this._lazy = lazy;
            this._delegate = _delegate;
            this.builder = builder;
        }
        
        
        protected internal override Widget buildWithChild(BuildContext context, Widget child)
        {
            D.assert(
                builder != null || child != null,
                () => $"runtimeType used outside of MultiProvider must specify a child"
            );
            return new _InheritedProviderScope<T>(
                owner: this,
                child: builder != null
                    ? new Container(
                        child:new Builder(
                            builder: (context2) => builder(context2, child)
                        )
                    )
                    : child
            );
        }
    }
    
    class _CreateInheritedProvider<T> : _Delegate<T> {
        public _CreateInheritedProvider(
            Create<T> create,
            Update<T> update,
            UpdateShouldNotify<T> updateShouldNotify = null,
            DebugCheckInvalidValueType<T> debugCheckInvalidValueType = null,
            StartListening<T> startListening = null,
            Dispose<T> dispose = null
        )
        {
            // D.assert(create != null || update != null);
            this.create = create;
            this.update = update;
            _updateShouldNotify = updateShouldNotify;
            this.debugCheckInvalidValueType = debugCheckInvalidValueType;
            this.startListening = startListening;
            this.dispose = dispose;
        }

        public readonly Create<T> create;
        public readonly Update<T> update;
        public readonly UpdateShouldNotify<T> _updateShouldNotify;
        public readonly DebugCheckInvalidValueType<T> debugCheckInvalidValueType;
        public readonly StartListening<T> startListening;
        public readonly Dispose<T> dispose;


        public override _DelegateState<T, _Delegate<T>> createState()
        {
            throw new System.NotImplementedException();
        }
    }
    
    
    public abstract class _Delegate<T> {
        public abstract _DelegateState<T, _Delegate<T>> createState();

        public virtual void debugFillProperties(DiagnosticPropertiesBuilder properties) {}
    }
    
    public abstract class _DelegateState<T, D> where D : _Delegate<T>{
        public _InheritedProviderScopeElement<T> element;

        public T value
        {
            get;
        }

        public D  _delegate
        {
            get
            {
                return element.widget.owner._delegate as D;
            }
        }

        private bool hasValue
        {
            get;
        }

        bool debugSetInheritedLock(bool value) {
            return element._debugSetInheritedLock(value);
        }

        public virtual bool willUpdateDelegate(D newDelegate) => false;

        void dispose() {}

        public virtual void debugFillProperties(DiagnosticPropertiesBuilder properties) {}

        void build(bool isBuildFromExternalSources) {}
    }

    public class _InheritedProviderScopeElement<T> : InheritedElement
    {
        public _InheritedProviderScopeElement(Widget widget) : base(widget)
        {
        }
        
        bool _debugInheritLocked = false;
        _DelegateState<T, _Delegate<T>> _delegateState;
        
        public new  _InheritedProviderScope<T> widget
        {
            get
            {
                return base.widget as _InheritedProviderScope<T>;
            }
        }
        
        public bool _debugSetInheritedLock(bool value) {
            D.assert(() => {
                _debugInheritLocked = value;
                return true;
            });
            return true;
        }
        
        public new T value
        {
            get
            {
                if (_delegateState != null)
                {
                    return _delegateState.value;
                }

                return default;
            }   
        }
        
    }
    
    public class _InheritedProviderScope<T> : InheritedWidget {
        public _InheritedProviderScope(
            InheritedProvider<T> owner = null,
            Widget child = null
        ) : base(child: child)
        {
            this.owner = owner;
        }

        public readonly InheritedProvider<T> owner;
        
        public override bool updateShouldNotify(InheritedWidget oldWidget) {
            return false;
        }
        public override Element createElement() {
            return new _InheritedProviderScopeElement<T>(this);
        }
    }
    
    public abstract class InheritedContext<T> : BuildContext {
    /// The current value exposed by [InheritedProvider].
    ///
    /// This property is lazy loaded, and reading it the first time may trigger
    /// some side-effects such as creating a [T] instance or start a subscription.
    private T value { get; }

    /// Marks the [InheritedProvider] as needing to update dependents.
    ///
    /// This bypass [InheritedWidget.updateShouldNotify] and will force widgets
    /// that depends on [T] to rebuild.
    void markNeedsNotifyDependents(){}

    /// Wether `setState` was called at least once or not.
    ///
    /// It can be used by [DeferredStartListening] to differentiate between the
    /// very first listening, and a rebuild after `controller` changed.
    private bool hasValue { get; }

    public Widget widget { get; }
    public BuildOwner owner { get; }
    public bool debugDoingBuild { get; }
    public RenderObject findRenderObject()
    {
        throw new NotImplementedException();
    }

    public Size size { get; }
    public InheritedWidget inheritFromElement(InheritedElement ancestor, object aspect = null)
    {
        throw new NotImplementedException();
    }

    public InheritedWidget dependOnInheritedElement(InheritedElement ancestor, object aspect = null)
    {
        throw new NotImplementedException();
    }

    public InheritedWidget inheritFromWidgetOfExactType(Type targetType, object aspect = null)
    {
        throw new NotImplementedException();
    }

    public T1 dependOnInheritedWidgetOfExactType<T1>(object aspect = null) where T1 : InheritedWidget
    {
        throw new NotImplementedException();
    }

    public InheritedElement ancestorInheritedElementForWidgetOfExactType(Type targetType)
    {
        throw new NotImplementedException();
    }

    public InheritedElement getElementForInheritedWidgetOfExactType<T1>() where T1 : InheritedWidget
    {
        throw new NotImplementedException();
    }

    public Widget ancestorWidgetOfExactType(Type targetType)
    {
        throw new NotImplementedException();
    }

    public T1 findAncestorWidgetOfExactType<T1>() where T1 : Widget
    {
        throw new NotImplementedException();
    }

    public State ancestorStateOfType(TypeMatcher matcher)
    {
        throw new NotImplementedException();
    }

    public T1 findAncestorStateOfType<T1>() where T1 : State
    {
        throw new NotImplementedException();
    }

    public State rootAncestorStateOfType(TypeMatcher matcher)
    {
        throw new NotImplementedException();
    }

    public T1 findRootAncestorStateOfType<T1>() where T1 : State
    {
        throw new NotImplementedException();
    }

    public RenderObject ancestorRenderObjectOfType(TypeMatcher matcher)
    {
        throw new NotImplementedException();
    }

    public T1 findAncestorRenderObjectOfType<T1>() where T1 : RenderObject
    {
        throw new NotImplementedException();
    }

    public void visitAncestorElements(ElementVisitorBool visitor)
    {
        throw new NotImplementedException();
    }

    public void visitChildElements(ElementVisitor visitor)
    {
        throw new NotImplementedException();
    }

    public DiagnosticsNode describeElement(string name, DiagnosticsTreeStyle style = DiagnosticsTreeStyle.errorProperty)
    {
        throw new NotImplementedException();
    }

    public DiagnosticsNode describeWidget(string name, DiagnosticsTreeStyle style = DiagnosticsTreeStyle.errorProperty)
    {
        throw new NotImplementedException();
    }

    public List<DiagnosticsNode> describeMissingAncestor(Type expectedAncestorType)
    {
        throw new NotImplementedException();
    }

    public DiagnosticsNode describeOwnershipChain(string name)
    {
        throw new NotImplementedException();
    }
    }
    
}