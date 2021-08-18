using System;
using System.Diagnostics;

namespace Unity.UIWidgets.async  {
    public static partial class _stream {
        /** Runs user code and takes actions depending on success or failure. */
        internal static void _runUserCode<T>(
            Func<T> userCode, Action<T> onSuccess, Action<Exception> onError){ 
            try {
                onSuccess(userCode());
            } catch (Exception e) {
                AsyncError replacement = Zone.current.errorCallback(e);
                if (replacement == null) {
                    onError(e);
                } else {
                    var error = async_._nonNullError(replacement);
                    onError(error);
                }
            }
        }
        
        internal static void _cancelAndErrorWithReplacement<T>(StreamSubscription<T> subscription,
            _Future future, Exception error) {
            AsyncError replacement = Zone.current.errorCallback(error);
            if (replacement != null) {
                error = (Exception) _async._nonNullError(replacement);
            }
            _cancelAndError(subscription, future, error);
        }
        
        internal delegate void _ErrorCallback(Exception error);


        internal static _ErrorCallback _cancelAndErrorClosure<T>(
            StreamSubscription<T> subscription, _Future future) {
            return (error) => {
                _cancelAndError(subscription, future, error);
            };
        }
        
        internal static void _cancelAndValue<T>(StreamSubscription<T> subscription, _Future future, object value) {
            var cancelFuture = subscription.cancel();
            if (cancelFuture != null && !Equals(cancelFuture, Future._nullFuture)) {
                cancelFuture.whenComplete(() => future._complete(FutureOr.value(value)));
            } else {
                future._complete(FutureOr.value(value));
            }
        }
        
        static void _cancelAndError<T>(StreamSubscription<T> subscription, _Future future, Exception error
            ) {
            var cancelFuture = subscription.cancel();
            if (cancelFuture != null && !Equals(cancelFuture, Future._nullFuture)) {
                cancelFuture.whenComplete(() => future._completeError(error));
            } else {
                future._completeError(error);
            }
        }

        internal static void _cancelAndValue<T>(StreamSubscription<T> subscription, _Future future, FutureOr value) {
            var cancelFuture = subscription.cancel();
            if (cancelFuture != null && !Equals(cancelFuture, Future._nullFuture)) {
                cancelFuture.whenComplete(() => future._complete(value));
            } else {
                future._complete(value);
            }
        }

    }
}