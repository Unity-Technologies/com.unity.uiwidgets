using System;
using System.Collections.Generic;
using Unity.UIWidgets.async2;
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

        // public Future<T> _invokeMethod<T>(string method,  bool missingOk, object arguments )  {
        //     // async
        //     D.assert(method != null); 
        //     // await
        //     ///???
        //     var result =  binaryMessenger.send(
        //       name,
        //       codec.encodeMethodCall(new MethodCall(method, arguments))
        //     );
        //     return result.then_<byte[]>(r => {
        //         if (result == null) {
        //             if (missingOk) {
        //                 return FutureOr.nil;
        //             }
        //             throw new MissingPluginException($"No implementation found for method $method on channel {name}");
        //         }
        //         return FutureOr.value(r);
        //     }).to<T>();

        // }
        public Future _invokeMethod(string method, bool missingOk, object arguments) {
            D.assert(method != null);
            return binaryMessenger.send(
                name,
                codec.encodeMethodCall(new MethodCall(method, arguments))
            );
        }

        // public virtual Future<T> invokeMethod<T>(string method,  object arguments  = null) {
        //     return _invokeMethod<T>(method, missingOk: false, arguments: arguments);
        // }
        public virtual Future invokeMethod(string method, object arguments = null) {
            return _invokeMethod(method, missingOk: false, arguments: arguments);
        }

        // Future<List<T>> invokeListMethod<T>(String method, object arguments ) {
        //     var result =  invokeMethod<List<dynamic>>(method, arguments).to<List<T>>();
        //     return result;
        // }
    }

    public class OptionalMethodChannel : MethodChannel {
        public OptionalMethodChannel(String name, MethodCodec codec = null)
            : base(name, codec) {
        }

        public static OptionalMethodChannel create(String name, MethodCodec codec = null) {
            codec = codec ?? new StandardMethodCodec();
            return new OptionalMethodChannel(name, codec);
        }

        // public override Future<T> invokeMethod<T>(String method, object arguments =null)  {
        //     return base._invokeMethod<T>(method, missingOk: true, arguments: arguments);
        // }

        public override Future invokeMethod(String method, object arguments = null) {
            return base._invokeMethod(method, missingOk: true, arguments: arguments);
        }

        // public override Future<List<T>> invokeListMethod<T>(String method,  object arguments ) {
        //      List<dynamic> result = await invokeMethod<List<T>>(method, arguments);
        //     return result.cast<T>();
        // }
        //
        // public override Future<Dictionary<K, V>> invokeMapMethod<K, V>(String method,  object arguments = null) {
        //     Dictionary<object, object> result = await invokeMethod<Dictionary<object, dynamic>>(method, arguments);
        //     return result.cast<K, V>();
        // }
    }

    // class BasicMessageChannel<T> {
    //     /// Creates a [BasicMessageChannel] with the specified [name], [codec] and [binaryMessenger].
    //     ///
    //     /// The [name] and [codec] arguments cannot be null. The default [ServicesBinding.defaultBinaryMessenger]
    //     /// instance is used if [binaryMessenger] is null.
    //     const BasicMessageChannel(this.name, this.codec, { BinaryMessenger binaryMessenger })
    //     : assert(name != null),
    //     assert(codec != null),
    //     _binaryMessenger = binaryMessenger;
    //
    //     /// The logical channel on which communication happens, not null.
    //     final String name;
    //
    //     /// The message codec used by this channel, not null.
    //     final MessageCodec<T> codec;
    //
    //     /// The messenger which sends the bytes for this channel, not null.
    //     BinaryMessenger get binaryMessenger => _binaryMessenger ?? defaultBinaryMessenger; // ignore: deprecated_member_use_from_same_package
    //     final BinaryMessenger _binaryMessenger;
    //
    //     /// Sends the specified [message] to the platform plugins on this channel.
    //     ///
    //     /// Returns a [Future] which completes to the received response, which may
    //     /// be null.
    //     Future<T> send(T message) async {
    //         return codec.decodeMessage(await binaryMessenger.send(name, codec.encodeMessage(message)));
    //     }
}