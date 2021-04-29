using System;
using Unity.UIWidgets.async;

namespace Unity.UIWidgets.foundation {
    public class SynchronousFuture : Future {
        public SynchronousFuture(object value) {
            _value = value;
        }

        readonly object _value;

        // @override
        //     Stream<T> asStream() {
        //     final StreamController<T> controller = StreamController<T>();
        //     controller.add(_value);
        //     controller.close();
        //     return controller.stream;
        // }

        public override Future catchError(Func<Exception, FutureOr> onError, Func<Exception, bool> test = null) {
            return Completer.create().future;
        }

        public override Future then(Func<object, FutureOr> f, Func<Exception, FutureOr> onError = null) {
            FutureOr result = f(_value);
            if (result.isFuture)
                return result.f;

            return new SynchronousFuture(result.v);
        }

        public override Future timeout(TimeSpan timeLimit, Func<FutureOr> onTimeout = null) {
            return value(FutureOr.value(_value)).timeout(timeLimit, onTimeout: onTimeout);
        }

        public override Future whenComplete(Func<FutureOr> action) {
            try {
                FutureOr result = action();
                if (result.isFuture)
                    return result.f.then((value) => FutureOr.value(_value));
                return this;
            }
            catch (Exception e) {
                return error(e);
            }
        }
    }

    public class SynchronousFuture<T> : Future<T> {
        public SynchronousFuture(T value) : base(new SynchronousFuture(value)) {
        }
    }
}