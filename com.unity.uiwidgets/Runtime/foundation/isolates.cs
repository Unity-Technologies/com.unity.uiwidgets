using System.ComponentModel;
using Unity.UIWidgets.async;
using Unity.UIWidgets.ui;

namespace Unity.UIWidgets.foundation {
    public delegate R ComputeCallback<Q, R>(Q message);

    public static partial class foundation_ {
        public static Future<R> compute<Q, R>(ComputeCallback<Q, R> callback, Q message, string debugLabel = null) {
            var completer = Completer.create();
            var isolate = Isolate.current;
            
            var backgroundWorker = new BackgroundWorker();
            backgroundWorker.DoWork += (sender, args) => { args.Result = callback((Q) args.Argument); };
            backgroundWorker.RunWorkerCompleted += (o, a) => {
                if (!isolate.isValid) {
                    return;
                }
                
                using (Isolate.getScope(isolate)) {
                    if (a.Error != null) {
                        completer.completeError(a.Error);
                    }
                    else {
                        completer.complete(FutureOr.value((R) a.Result));
                    }
                }
            };
            backgroundWorker.RunWorkerAsync(message);
            return completer.future.to<R>();
        }
    }
}