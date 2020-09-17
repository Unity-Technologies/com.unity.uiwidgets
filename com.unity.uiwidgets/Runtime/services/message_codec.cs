using System;
using Unity.UIWidgets.external.simplejson;
using Unity.UIWidgets.foundation;

namespace Unity.UIWidgets.services {
    public interface MessageCodec<T> {
        byte[] encodeMessage(T message);

        T decodeMessage(byte[] message);
    }

    public readonly struct MethodCall {
        public MethodCall(string method, object arguments) {
            D.assert(method != null);
            this.method = method;
            this.arguments = arguments;
        }

        public readonly string method;

        public readonly object arguments;

        public override string ToString() =>
            $"{foundation_.objectRuntimeType(this, "MethodCall")}({method}, {arguments})";
    }

    public interface MethodCodec {
        byte[] encodeMethodCall(MethodCall methodCall);

        MethodCall decodeMethodCall(byte[] methodCall);

        object decodeEnvelope(byte[] envelope);

        byte[] encodeSuccessEnvelope(object result);

        byte[] encodeErrorEnvelope(string code, string message = null, object details = null);
    }

    public class PlatformException : Exception {
        public PlatformException(string code,
            string message = null,
            object details = null) {
            D.assert(code != null);
            this.code = code;
            this.message = message;
            this.details = details;
        }

        public readonly string code;

        public readonly string message;

        public readonly object details;

        public override string ToString() => $"PlatformException({code}, {message}, {details})";
    }

    public class MissingPluginException : Exception {
        public MissingPluginException(string message = null) {
            this.message = message;
        }

        public readonly string message;

        public override string ToString() => $"MissingPluginException({message})";
    }
}