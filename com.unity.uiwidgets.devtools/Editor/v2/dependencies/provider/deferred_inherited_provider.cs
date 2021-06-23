using Unity.UIWidgets.foundation;
using Unity.UIWidgets.ui;
using Unity.UIWidgets.widgets;

namespace Unity.UIWidgets.DevTools
{

    public delegate void SetState<R>(R value);
    public delegate VoidCallback DeferredStartListening<T, R>(
        InheritedContext<R> context,
        SetState<R> setState,
        T controller,
        R value
    );
    
    
    class DeferredInheritedProvider<T, R> : InheritedProvider<R> {

        public DeferredInheritedProvider(
            Key key = null,
            Create<T> create = null,
            Dispose<T> dispose = null,
            DeferredStartListening<T, R> startListening = null,
            UpdateShouldNotify<R> updateShouldNotify = null,
            bool? lazy = null,
            TransitionBuilder builder = null,
            Widget child = null
        )
        {
            base._constructor(
                key: key,
                child: child,
                lazy: lazy,
                builder: builder,
                _delegate: new _CreateDeferredInheritedProvider<T,R>(
                    create: create,
                    dispose: dispose,
                    updateShouldNotify: updateShouldNotify,
                    startListening: startListening
                )
            );
        }

        /// Listens to `value` and expose its content to `child` and its descendants.
        public DeferredInheritedProvider(
            Key key = null,
            T value = default,
            DeferredStartListening<T, R> startListening = null,
            UpdateShouldNotify<R> updateShouldNotify = null,
            bool? lazy = null,
            TransitionBuilder builder = null,
            Widget child = null
        )
        {
            base._constructor(
                key: key,
                lazy: lazy,
                builder: builder,
                _delegate: new _ValueDeferredInheritedProvider<T, R>(
                    value,
                    updateShouldNotify,
                    startListening
                ),
                child: child
            );
        }
        
    }
    
    class _CreateDeferredInheritedProvider<T, R> : _DeferredDelegate<T, R> {
        public _CreateDeferredInheritedProvider(
            Create<T> create = null,
            Dispose<T> dispose = null,
            UpdateShouldNotify<R> updateShouldNotify = null,
            DeferredStartListening<T, R> startListening = null
        ) : base(updateShouldNotify, startListening)
        {
            this.create = create;
            this.dispose = dispose;
        }

        public readonly Create<T> create;
        public readonly Dispose<T> dispose;
    
        public _CreateDeferredInheritedProviderElement<T, R> createState() { // [attention] override
            return new _CreateDeferredInheritedProviderElement<T, R>();
        }
    }

    class _CreateDeferredInheritedProviderElement<T, R>
        : _DeferredDelegateState<T, R, _CreateDeferredInheritedProvider<T, R>>
    {
        
    }


    public abstract class _DeferredDelegate<T, R> : _Delegate<R> {
        public _DeferredDelegate(UpdateShouldNotify<R> updateShouldNotify, DeferredStartListening<T, R> startListening)
        {
            this.updateShouldNotify = updateShouldNotify;
            this.startListening = startListening;
        }

        public readonly UpdateShouldNotify<R> updateShouldNotify;
        public readonly DeferredStartListening<T, R> startListening;

        public override _DelegateState<R, _Delegate<R>> createState()
        {
            return null; // [attention] this function has no body in flutter
        }
    }
    
    public abstract class _DeferredDelegateState<T, R, W > : _DelegateState<R, W> where W : _DeferredDelegate<T, R>
    {
        public VoidCallback _removeListener;
        R _value;
        
        public new R value {
            get
            {
                // element._isNotifyDependentsEnabled = false;
                // _removeListener ??= delegate.startListening(
                //     element,
                //     setState,
                //     controller,
                //     _value,
                // );
                // element._isNotifyDependentsEnabled = true;
                // D.assert(element.hasValue, '''
                // The callback "startListening" was called, but it left DeferredInhertitedProviderElement<$T, $R>
                //     in an unitialized state.
                //
                //     It is necessary for "startListening" to call "setState" at least once the very
                //     first time "value" is requested.
                //
                //     To fix, consider:
                //
                // DeferredInheritedProvider(
                //     ...,
                // startListening: (element, setState, controller, value) {
                //     if (!element.hasValue) {
                //         setState(myInitialValue); // TODO replace myInitialValue with your own
                //     }
                //     ...
                // }
                // )
                // ''');
                // D.assert(_removeListener != null);
                return _value;
            }
            
    }
    }
    
    public class _ValueDeferredInheritedProvider<T, R> : _DeferredDelegate<T, R> {
        public _ValueDeferredInheritedProvider(
            T value,
            UpdateShouldNotify<R> updateShouldNotify,
            DeferredStartListening<T, R> startListening
        ) : base(updateShouldNotify, startListening)
        {
            this.value = value;
        }

        public readonly T value;
        
        public  _ValueDeferredInheritedProviderState<T, R> createState() { // [attention] override
            return new _ValueDeferredInheritedProviderState<T,R>();
        }

        
        public override void debugFillProperties(DiagnosticPropertiesBuilder properties) {
            base.debugFillProperties(properties);
            properties.add(new DiagnosticsProperty<T>("controller", value));
        }
    }
    
    public class _ValueDeferredInheritedProviderState<T, R> : _DeferredDelegateState<
    T, R, _ValueDeferredInheritedProvider<T, R>> {
    
    public override bool willUpdateDelegate(_ValueDeferredInheritedProvider<T, R> oldDelegate) {
        if (true) { // [attention] _delegate.value != oldDelegate.value
            if (_removeListener != null) {
                _removeListener();
                _removeListener = null;
            }
            return true;
        }
        return false;
    }

    
    public new T  controller
    {
        get
        {
            return _delegate.value;
        }
    }

    
    public override void debugFillProperties(DiagnosticPropertiesBuilder properties) {
        base.debugFillProperties(properties);
        if (_removeListener != null) {
            properties.add(new DiagnosticsProperty<R>("value", value));
        } else {
            properties.add(
                new FlagProperty(
                    "value",
                    value: true,
                    showName: true,
                    ifTrue: "<not yet loaded>"
                )
            );
        }
    }
    }
    
}