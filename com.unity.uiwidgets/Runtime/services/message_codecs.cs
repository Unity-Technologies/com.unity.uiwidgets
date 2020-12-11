using System;
using System.Collections;
using System.Globalization;
using System.Text;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.services;

namespace Unity.UIWidgets.services {
    public class BinaryCodec : MessageCodec<byte[]> {
        public BinaryCodec() {
        }

        public static readonly BinaryCodec instance = new BinaryCodec();

        public byte[] decodeMessage(byte[] message) => message;

        public byte[] encodeMessage(byte[] message) => message;
    }

    public class StringCodec : MessageCodec<string> {
        public StringCodec() {
        }

        public static readonly StringCodec instance = new StringCodec();

        public string decodeMessage(byte[] message) {
            if (message == null)
                return null;
            return Encoding.UTF8.GetString(message);
        }

        public byte[] encodeMessage(string message) {
            if (message == null)
                return null;
            return Encoding.UTF8.GetBytes(message);
        }
    }

    public class JSONMessageCodec : MessageCodec<object> {
        public JSONMessageCodec() {
        }

        public static readonly JSONMessageCodec instance = new JSONMessageCodec();

        public byte[] encodeMessage(object message) {
            var json = toJson(message);
            return StringCodec.instance.encodeMessage(json);
        }

        public string toJson(object message) {
            if (message == null)
                return null;

            var sb = new StringBuilder();
            _writeToStringBuilder(message, sb);
            return sb.ToString();
        }

        public object decodeMessage(byte[] message) {
            if (message == null)
                return null;

            return _parseJson(StringCodec.instance.decodeMessage(message));
        }

        [ThreadStatic] static StringBuilder _escapeBuilder;

        static StringBuilder _getEscapeBuilder() {
            return _escapeBuilder ?? (_escapeBuilder = new StringBuilder());
        }

        static string _escape(string aText) {
            var sb = _getEscapeBuilder();
            sb.Length = 0;
            if (sb.Capacity < aText.Length + aText.Length / 10)
                sb.Capacity = aText.Length + aText.Length / 10;

            foreach (char c in aText) {
                switch (c) {
                    case '\\':
                        sb.Append("\\\\");
                        break;
                    case '\"':
                        sb.Append("\\\"");
                        break;
                    case '\n':
                        sb.Append("\\n");
                        break;
                    case '\r':
                        sb.Append("\\r");
                        break;
                    case '\t':
                        sb.Append("\\t");
                        break;
                    case '\b':
                        sb.Append("\\b");
                        break;
                    case '\f':
                        sb.Append("\\f");
                        break;
                    default:
                        if (c < ' ') {
                            ushort val = c;
                            sb.Append("\\u").Append(val.ToString("X4"));
                        }
                        else
                            sb.Append(c);

                        break;
                }
            }

            string result = sb.ToString();
            sb.Length = 0;
            return result;
        }

        static void _writeToStringBuilder(object obj, StringBuilder sb) {
            if (obj is IDictionary dict) {
                sb.Append('{');
                bool first = true;
                foreach (DictionaryEntry k in dict) {
                    if (!first)
                        sb.Append(',');
                    first = false;
                    sb.Append('\"').Append(_escape(k.Key.ToString())).Append('\"');
                    sb.Append(':');
                    _writeToStringBuilder(k.Value, sb);
                }

                sb.Append('}');
            }
            else if (obj is IList list) {
                sb.Append('[');
                int count = list.Count;
                for (int i = 0; i < count; i++) {
                    if (i > 0)
                        sb.Append(',');
                    _writeToStringBuilder(list[i], sb);
                }

                sb.Append(']');
            }
            else if (obj is string str) {
                sb.Append('\"').Append(_escape(str)).Append('\"');
            }
            else if (obj is double d) {
                sb.Append(d.ToString(CultureInfo.InvariantCulture));
            }
            else if (obj is float f) {
                sb.Append(f.ToString(CultureInfo.InvariantCulture));
            }
            else if (obj is int i) {
                sb.Append(i.ToString(CultureInfo.InvariantCulture));
            }
            else if (obj is long l) {
                sb.Append(l.ToString(CultureInfo.InvariantCulture));
            }
            else if (obj is bool b) {
                sb.Append(b ? "true" : "false");
            }
            else if (obj == null) {
                sb.Append("null");
            }
            else {
                throw new Exception("Unsupported object: " + obj);
            }
        }

        static object _parseElement(string token, bool quoted) {
            if (quoted)
                return token;

            string tmp = token.ToLower();
            if (tmp == "false" || tmp == "true")
                return tmp == "true";
            if (tmp == "null")
                return null;

            if (double.TryParse(token, NumberStyles.Float, CultureInfo.InvariantCulture, out var val))
                return val;
            else
                return token;
        }

        static void _addElement(object ctx, string tokenName, object element) {
            if (ctx is IDictionary dict) {
                dict.Add(tokenName, element);
            }
            else if (ctx is IList list) {
                D.assert(tokenName.isEmpty);
                list.Add(element);
            }
            else {
                D.assert(ctx == null);
            }
        }

        static object _parseJson(string jsonStr) {
            Stack stack = new Stack();
            object ctx = null;
            int i = 0;
            StringBuilder token = new StringBuilder();
            string tokenName = "";
            bool quoteMode = false;
            bool tokenIsQuoted = false;
            while (i < jsonStr.Length) {
                switch (jsonStr[i]) {
                    case '{':
                        if (quoteMode) {
                            token.Append(jsonStr[i]);
                            break;
                        }

                        stack.Push(new Hashtable());
                        _addElement(ctx, tokenName, stack.Peek());

                        tokenName = "";
                        token.Length = 0;
                        ctx = stack.Peek();
                        break;

                    case '[':
                        if (quoteMode) {
                            token.Append(jsonStr[i]);
                            break;
                        }

                        stack.Push(new ArrayList());
                        _addElement(ctx, tokenName, stack.Peek());

                        tokenName = "";
                        token.Length = 0;
                        ctx = stack.Peek();
                        break;

                    case '}':
                    case ']':
                        if (quoteMode) {
                            token.Append(jsonStr[i]);
                            break;
                        }

                        if (stack.Count == 0)
                            throw new Exception("JSON Parse: Too many closing brackets");

                        stack.Pop();
                        if (token.Length > 0 || tokenIsQuoted)
                            _addElement(ctx, tokenName, _parseElement(token.ToString(), tokenIsQuoted));

                        tokenIsQuoted = false;
                        tokenName = "";
                        token.Length = 0;

                        if (stack.Count > 0)
                            ctx = stack.Peek();
                        break;

                    case ':':
                        if (quoteMode) {
                            token.Append(jsonStr[i]);
                            break;
                        }

                        tokenIsQuoted = false;
                        tokenName = token.ToString();
                        token.Length = 0;
                        break;

                    case '"':
                        quoteMode ^= true;
                        tokenIsQuoted |= quoteMode;
                        break;

                    case ',':
                        if (quoteMode) {
                            token.Append(jsonStr[i]);
                            break;
                        }

                        if (token.Length > 0 || tokenIsQuoted)
                            _addElement(ctx, tokenName, _parseElement(token.ToString(), tokenIsQuoted));

                        tokenIsQuoted = false;
                        tokenName = "";
                        token.Length = 0;
                        break;

                    case '\r':
                    case '\n':
                        break;

                    case ' ':
                    case '\t':
                        if (quoteMode)
                            token.Append(jsonStr[i]);
                        break;

                    case '\\':
                        ++i;
                        if (quoteMode) {
                            char c = jsonStr[i];
                            switch (c) {
                                case 't':
                                    token.Append('\t');
                                    break;
                                case 'r':
                                    token.Append('\r');
                                    break;
                                case 'n':
                                    token.Append('\n');
                                    break;
                                case 'b':
                                    token.Append('\b');
                                    break;
                                case 'f':
                                    token.Append('\f');
                                    break;
                                case 'u': {
                                    string s = jsonStr.Substring(i + 1, 4);
                                    token.Append((char) int.Parse(
                                        s,
                                        NumberStyles.AllowHexSpecifier));
                                    i += 4;
                                    break;
                                }
                                default:
                                    token.Append(c);
                                    break;
                            }
                        }

                        break;

                    default:
                        token.Append(jsonStr[i]);
                        break;
                }

                ++i;
            }

            if (quoteMode) {
                throw new Exception("JSON Parse: Quotation marks seems to be messed up.");
            }

            if (ctx == null)
                return _parseElement(token.ToString(), tokenIsQuoted);
            return ctx;
        }
    }
}

public class JSONMethodCodec : MethodCodec {
    public JSONMethodCodec() {
    }

    public static readonly JSONMethodCodec instance = new JSONMethodCodec();

    public byte[] encodeMethodCall(MethodCall call) {
        var obj = new Hashtable {{"method", call.method}, {"args", call.arguments}};
        return JSONMessageCodec.instance.encodeMessage(obj);
    }

    public MethodCall decodeMethodCall(byte[] methodCall) {
        object decoded = JSONMessageCodec.instance.decodeMessage(methodCall);
        if (!(decoded is IDictionary dict))
            throw new Exception($"Expected method call JSONObject, got {decoded}");

        object methodRaw = dict["method"];
        object arguments = dict["args"];

        if (methodRaw is string method)
            return new MethodCall(method, arguments);

        throw new Exception($"Invalid method call: {decoded}");
    }

    public object decodeEnvelope(byte[] envelope) {
        object decoded = JSONMessageCodec.instance.decodeMessage(envelope);
        if (!(decoded is IList list))
            throw new Exception($"Expected envelope JSONArray, got {decoded}");

        if (list.Count == 1)
            return list[0];
        if (list.Count == 3
            && list[0] is string code
            && (list[1] == null || list[1] is string))
            throw new PlatformException(
                code: code,
                message: list[1] as string,
                details: list[2]
            );
        throw new Exception($"Invalid envelope: {decoded}");
    }

    public byte[] encodeSuccessEnvelope(object result) {
        var array = new ArrayList {result};
        return JSONMessageCodec.instance.encodeMessage(array);
    }

    public byte[] encodeErrorEnvelope(string code, string message = null, object details = null) {
        D.assert(code != null);
        var array = new ArrayList {code, message, details};
        return JSONMessageCodec.instance.encodeMessage(array);
    }
}

public class StandardMessageCodec : MessageCodec<object> {
    public StandardMessageCodec() {
    }

    public static readonly StandardMessageCodec instance = new StandardMessageCodec();

    const byte _valueNull = 0;
    const byte _valueTrue = 1;
    const byte _valueFalse = 2;
    const byte _valueInt32 = 3;
    const byte _valueInt64 = 4;
    const byte _valueFloat32 = 6;
    const byte _valueString = 7;
    const byte _valueUint8List = 8;
    const byte _valueInt32List = 9;
    const byte _valueInt64List = 10;
    const byte _valueFloat32List = 11;
    const byte _valueList = 12;
    const byte _valueMap = 13;

    public byte[] encodeMessage(object message) {
        if (message == null)
            return null;
        WriteBuffer buffer = new WriteBuffer();
        writeValue(buffer, message);
        return buffer.done();
    }

    public object decodeMessage(byte[] message) {
        if (message == null)
            return null;
        ReadBuffer buffer = new ReadBuffer(message);
        object result = readValue(buffer);
        if (buffer.hasRemaining)
            throw new Exception("Message corrupted");
        return result;
    }

    public void writeValue(WriteBuffer buffer, object value) {
        if (value == null) {
            buffer.putUint8(_valueNull);
        }
        else if (value is bool b) {
            buffer.putUint8(b ? _valueTrue : _valueFalse);
        }
        else if (value is float f) {
            buffer.putUint8(_valueFloat32);
            buffer.putFloat32(f);
        }
        else if (value is int i) {
            buffer.putUint8(_valueInt32);
            buffer.putInt32(i);
        }
        else if (value is long l) {
            buffer.putUint8(_valueInt64);
            buffer.putInt64(l);
        }
        else if (value is string s) {
            buffer.putUint8(_valueString);
            byte[] bytes = Encoding.UTF8.GetBytes(s);
            writeSize(buffer, bytes.Length);
            buffer.putUint8List(bytes);
        }
        else if (value is byte[] bytes) {
            buffer.putUint8(_valueUint8List);
            writeSize(buffer, bytes.Length);
            buffer.putUint8List(bytes);
        }
        else if (value is int[] ints) {
            buffer.putUint8(_valueInt32List);
            writeSize(buffer, ints.Length);
            buffer.putInt32List(ints);
        }
        else if (value is long[] longs) {
            buffer.putUint8(_valueInt64List);
            writeSize(buffer, longs.Length);
            buffer.putInt64List(longs);
        }
        else if (value is float[] floats) {
            buffer.putUint8(_valueFloat32List);
            writeSize(buffer, floats.Length);
            buffer.putFloat32List(floats);
        }
        else if (value is IList list) {
            buffer.putUint8(_valueList);
            writeSize(buffer, list.Count);
            foreach (object item in list) {
                writeValue(buffer, item);
            }
        }
        else if (value is IDictionary dict) {
            buffer.putUint8(_valueMap);
            writeSize(buffer, dict.Count);
            foreach (DictionaryEntry entry in dict) {
                writeValue(buffer, entry.Key);
                writeValue(buffer, entry.Value);
            }
        }
        else {
            throw new ArgumentException(value.ToString());
        }
    }

    public object readValue(ReadBuffer buffer) {
        if (!buffer.hasRemaining)
            throw new Exception("Message corrupted");
        int type = buffer.getUint8();
        return readValueOfType(type, buffer);
    }

    object readValueOfType(int type, ReadBuffer buffer) {
        switch (type) {
            case _valueNull:
                return null;
            case _valueTrue:
                return true;
            case _valueFalse:
                return false;
            case _valueInt32:
                return buffer.getInt32();
            case _valueInt64:
                return buffer.getInt64();
            case _valueFloat32:
                return buffer.getFloat32();
            case _valueString: {
                int length = (int) readSize(buffer);
                return Encoding.UTF8.GetString(buffer.getUint8List(length));
            }
            case _valueUint8List: {
                int length = (int) readSize(buffer);
                return buffer.getUint8List(length);
            }
            case _valueInt32List: {
                int length = (int) readSize(buffer);
                return buffer.getInt32List(length);
            }
            case _valueInt64List: {
                int length = (int) readSize(buffer);
                return buffer.getInt64List(length);
            }
            case _valueFloat32List: {
                int length = (int) readSize(buffer);
                return buffer.getFloat32List(length);
            }
            case _valueList: {
                int length = (int) readSize(buffer);
                var result = new ArrayList(length);
                for (int i = 0; i < length; i++)
                    result.Add(readValue(buffer));
                return result;
            }
            case _valueMap: {
                int length = (int) readSize(buffer);
                Hashtable result = new Hashtable();
                for (int i = 0; i < length; i++)
                    result[readValue(buffer)] = readValue(buffer);
                return result;
            }
            default:
                throw new Exception("Message corrupted");
        }
    }

    void writeSize(WriteBuffer buffer, long value) {
        D.assert(0 <= value && value <= 0x7fffffff);
        if (value < 254) {
            buffer.putUint8((byte) value);
        }
        else if (value <= 0xffff) {
            buffer.putUint8(254);
            buffer.putUint16((ushort) value);
        }
        else {
            buffer.putUint8(255);
            buffer.putUint32((uint) value);
        }
    }

    long readSize(ReadBuffer buffer) {
        int value = buffer.getUint8();
        switch (value) {
            case 254:
                return buffer.getUint16();
            case 255:
                return buffer.getUint32();
            default:
                return value;
        }
    }
}

public class StandardMethodCodec : MethodCodec {
    public StandardMethodCodec(StandardMessageCodec messageCodec = null) {
        this.messageCodec = messageCodec ?? StandardMessageCodec.instance;
    }

    readonly StandardMessageCodec messageCodec;

    public byte[] encodeMethodCall(MethodCall call) {
        WriteBuffer buffer = new WriteBuffer();
        messageCodec.writeValue(buffer, call.method);
        messageCodec.writeValue(buffer, call.arguments);
        return buffer.done();
    }

    public MethodCall decodeMethodCall(byte[] methodCall) {
        ReadBuffer buffer = new ReadBuffer(methodCall);
        object methodRaw = messageCodec.readValue(buffer);
        object arguments = messageCodec.readValue(buffer);
        if (methodRaw is string method && !buffer.hasRemaining)
            return new MethodCall(method, arguments);
        else
            throw new Exception("Invalid method call");
    }

    public byte[] encodeSuccessEnvelope(object result) {
        WriteBuffer buffer = new WriteBuffer();
        buffer.putUint8(0);
        messageCodec.writeValue(buffer, result);
        return buffer.done();
    }

    public byte[] encodeErrorEnvelope(string code, string message = null, object details = null) {
        WriteBuffer buffer = new WriteBuffer();
        buffer.putUint8(1);
        messageCodec.writeValue(buffer, code);
        messageCodec.writeValue(buffer, message);
        messageCodec.writeValue(buffer, details);
        return buffer.done();
    }

    public object decodeEnvelope(byte[] envelope) {
        if (envelope.Length == 0)
            throw new Exception("Expected envelope, got nothing");
        ReadBuffer buffer = new ReadBuffer(envelope);
        if (buffer.getUint8() == 0)
            return messageCodec.readValue(buffer);
        object errorCodeRaw = messageCodec.readValue(buffer);
        object errorMessage = messageCodec.readValue(buffer);
        object errorDetails = messageCodec.readValue(buffer);
        if (errorCodeRaw is string errorCode && (errorMessage == null || errorMessage is string) &&
            !buffer.hasRemaining)
            throw new PlatformException(code: errorCode, message: errorMessage as string, details: errorDetails);
        else
            throw new Exception("Invalid envelope");
    }
}