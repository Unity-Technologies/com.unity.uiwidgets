using System;
using System.Collections;
using System.Collections.Generic;
using developer;
using Unity.UIWidgets.async;
using Unity.UIWidgets.ui;

namespace Unity.UIWidgets.foundation {
    public delegate Future<IDictionary<string, object>>
        ServiceExtensionCallback(IDictionary<string, string> parameters);

    public abstract class BindingBase {
        protected BindingBase() {
            Timeline.startSync("Framework initialization");
            
            D.assert(!_debugInitialized);
            initInstances();
            D.assert(_debugInitialized);
            
            initServiceExtensions();
            
            developer_.postEvent("Flutter.FrameworkInitialization", new Hashtable());

            Timeline.finishSync();
        }

        bool _debugInitialized = false;

        public Window window => Window.instance;

        protected virtual void initInstances() {
            D.assert(!_debugInitialized);
            D.assert(() => {
                _debugInitialized = true;
                return true;
            });
        }
        
        protected virtual void initServiceExtensions() {
        }

        protected bool locked => _lockCount > 0;
        int _lockCount = 0;

        protected Future lockEvents(Func<Future> callback) {
            Timeline.startSync("Lock events");

            D.assert(callback != null);
            _lockCount += 1;
            Future future = callback();
            D.assert(future != null,
                () =>
                    "The lockEvents() callback returned null; " +
                    "it should return a Promise that completes when the lock is to expire.");
            future.whenComplete(() => {
                _lockCount -= 1;
                if (!locked) {
                    Timeline.finishSync();
                    unlocked();
                }

                return FutureOr.nil;
            });
            return future;
        }

        protected virtual void unlocked() {
            D.assert(!locked);
        }
    }
}