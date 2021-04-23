using System;
using System.Collections.Generic;
using Unity.UIWidgets.async;
using Unity.UIWidgets.external.simplejson;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.services;
using Unity.UIWidgets.ui;
using UnityEngine;

namespace Unity.UIWidgets.service {
    public delegate Future<object> Handler(MethodCall call);
    public class MethodChannel {
        public MethodChannel(string name, MethodCodec codec, BinaryMessenger binaryMessenger = null) {
            codec = new StandardMethodCodec();
            D.assert(name != null);
            D.assert(codec != null);
            _binaryMessenger = binaryMessenger;
        }

        public readonly string name;
        public readonly MethodCodec codec;

        BinaryMessenger binaryMessenger {
            get { return _binaryMessenger ?? ServicesBinding.instance.defaultBinaryMessenger; }
        }

        public readonly BinaryMessenger _binaryMessenger;
        
        /*public void setMethodCallHandler( Handler handler) {
            binaryMessenger.setMessageHandler(
                name,
                handler == null ? null : (byte[] message) => _handleAsMethodCall(message, handler)
            );
        }*/
        /*
        public Future<T> _invokeMethod<T>(string method,  bool missingOk, object arguments )  {
            // async
            D.assert(method != null); 
            // await
            ///???
            var result =  binaryMessenger.send(
              name,
              codec.encodeMethodCall(new MethodCall(method, arguments))
            );
            if (result == null) {
              if (missingOk) {
                return null;
              }
              throw new MissingPluginException($"No implementation found for method $method on channel {name}");
            }
            return (Future<T>)codec.decodeEnvelope(result);
        }
        public Future _invokeMethod(string method,  bool missingOk, object arguments )  {
            // async
            D.assert(method != null); 
            // await
            byte[] result =  binaryMessenger.send(
                name,
                codec.encodeMethodCall(new MethodCall(method, arguments))
            );
            if (result == null) {
                if (missingOk) {
                    return null;
                }
                throw new MissingPluginException($"No implementation found for method $method on channel {name}");
            }
            return (Future)codec.decodeEnvelope(result);
        }
        public virtual Future<T> invokeMethod<T>(string method,  object arguments  = null) {
            return _invokeMethod<T>(method, missingOk: false, arguments: arguments);
        }
        public virtual Future invokeMethod(string method,  object arguments  = null) {
            return _invokeMethod(method, missingOk: false, arguments: arguments);
        }*/
        
    }
    /*public class OptionalMethodChannel : MethodChannel {
        public OptionalMethodChannel(string name, MethodCodec codec, BinaryMessenger binaryMessenger = null) : base(name, codec)
        {
        }

        public override Future<T> invokeMethod<T>(string method,  object arguments  = null)  {
            return base._invokeMethod<T>(method, missingOk: true, arguments: arguments);
        }
        public override Future invokeMethod(string method,  object arguments  = null)  {
            return base._invokeMethod(method, missingOk: true, arguments: arguments);
        }

    }*/

}