using System;
using System.Linq;

namespace Unity.UIWidgets {
    public interface Dispatcher {
        T dispatch<T>(object action);

        object dispatch(object action);
    }

    public class DispatcherImpl : Dispatcher {
        readonly Func<object, object> _impl;

        public DispatcherImpl(Func<object, object> impl) {
            _impl = impl;
        }
        
        public T dispatch<T>(object action) {
            if (_impl == null) {
                return default;
            }

            return (T) _impl(action);
        }

        public object dispatch(object action) {
            if (_impl == null) {
                return default;
            }

            return _impl(action);
        }
    }

    public delegate State Reducer<State>(State previousState, object action);

    public delegate Func<Dispatcher, Dispatcher> Middleware<State>(Store<State> store);

    public delegate void StateChangedHandler<State>(State action);

    public class Store<State> {
        public StateChangedHandler<State> stateChanged;
        
        readonly Dispatcher _dispatcher;
        readonly Reducer<State> _reducer;
        State _state;

        public Store(
            Reducer<State> reducer,
            State initialState = default,
            params Middleware<State>[] middleware) {
            _reducer = reducer;
            _dispatcher = _applyMiddleware(middleware);
            _state = initialState;
        }

        public Dispatcher dispatcher {
            get { return _dispatcher; }
        }

        public State getState() {
            return _state;
        }

        Dispatcher _applyMiddleware(params Middleware<State>[] middleware) {
            return middleware.Reverse().Aggregate<Middleware<State>, Dispatcher>(
                new DispatcherImpl(_innerDispatch),
                (current, middlewareItem) => middlewareItem(this)(current));
        }

        object _innerDispatch(object action) {
            _state = _reducer(_state, action);

            if (stateChanged != null) {
                stateChanged(_state);
            }

            return action;
        }
    }
}