using System;
using System.Collections.Generic;
using System.Text;
using Unity.UIWidgets.async;
using UnityEngine;

namespace Unity.UIWidgets.ui {
    readonly struct _StoredMessage {
        internal _StoredMessage(byte[] data, PlatformMessageResponseCallback callback) {
            _data = data;
            _callback = callback;
        }

        readonly byte[] _data;
        public byte[] data => _data;

        readonly PlatformMessageResponseCallback _callback;
        public PlatformMessageResponseCallback callback => _callback;
    }

    class _RingBuffer<T> where T : struct {
        readonly Queue<T> _queue;

        internal _RingBuffer(int capacity) {
            _capacity = capacity;
            _queue = new Queue<T>(_capacity);
        }

        public int length => _queue.Count;

        int _capacity;
        public int capacity => _capacity;

        public bool isEmpty => _queue.Count == 0;

        Action<T> _dropItemCallback;

        public Action<T> dropItemCallback {
            set { _dropItemCallback = value; }
        }

        public bool push(in T val) {
            if (_capacity <= 0)
                return true;

            int overflowCount = _dropOverflowItems(_capacity - 1);
            _queue.Enqueue(val);
            return overflowCount > 0;
        }

        public T? pop() {
            return _queue.Count == 0 ? (T?) null : _queue.Dequeue();
        }

        int _dropOverflowItems(int lengthLimit) {
            int result = 0;
            while (_queue.Count > lengthLimit) {
                T item = _queue.Dequeue();
                _dropItemCallback?.Invoke(item);

                result += 1;
            }

            return result;
        }

        public int resize(int newSize) {
            _capacity = newSize;
            return _dropOverflowItems(newSize);
        }
    }

    public delegate Future DrainChannelCallback(byte[] data, PlatformMessageResponseCallback callback);

    public class ChannelBuffers {
        public const int kDefaultBufferSize = 1;

        public const string kControlChannelName = "dev.uiwidgets/channel-buffers";

        readonly Dictionary<string, _RingBuffer<_StoredMessage>> _messages =
            new Dictionary<string, _RingBuffer<_StoredMessage>>();

        _RingBuffer<_StoredMessage> _makeRingBuffer(int size) =>
            new _RingBuffer<_StoredMessage>(size) {dropItemCallback = _onDropItem};

        void _onDropItem(_StoredMessage message) {
            message.callback(null);
        }

        public bool push(string channel, byte[] data, PlatformMessageResponseCallback callback) {
            _RingBuffer<_StoredMessage> queue = _messages[channel];
            if (queue == null) {
                queue = _makeRingBuffer(kDefaultBufferSize);
                _messages[channel] = queue;
            }

            bool didOverflow = queue.push(new _StoredMessage(data, callback));
            if (didOverflow) {
                Debug.LogWarning($"Overflow on channel: {channel}. " +
                                 "Messages on this channel are being discarded in FIFO fashion. " +
                                 "The engine may not be running or you need to adjust " +
                                 "the buffer size of the channel.");
            }

            return didOverflow;
        }

        _StoredMessage? _pop(string channel) {
            _RingBuffer<_StoredMessage> queue = _messages[channel];
            _StoredMessage? result = queue?.pop();
            return result;
        }

        void _resize(string channel, int newSize) {
            _RingBuffer<_StoredMessage> queue = _messages[channel];
            if (queue == null) {
                queue = _makeRingBuffer(newSize);
                _messages[channel] = queue;
            }
            else {
                int numberOfDroppedMessages = queue.resize(newSize);
                if (numberOfDroppedMessages > 0) {
                    Debug.LogWarning(
                        $"Dropping messages on channel \"{channel}\" as a result of shrinking the buffer size.");
                }
            }
        }

        public Future drain(string channel, DrainChannelCallback callback) {
            return Future.doWhile(() => {
                _StoredMessage? message = _pop(channel);
                if (!message.HasValue) {
                    return false;
                }

                return callback(message.Value.data, message.Value.callback);
            });
        }

        string _getString(byte[] data) {
            return Encoding.UTF8.GetString(data);
        }

        public void handleMessage(byte[] data) {
            var command = _getString(data).Split('\r');
            if (command.Length == /*arity=*/2 + 1 && command[0] == "resize") {
                _resize(command[1], int.Parse(command[2]));
            }
            else {
                throw new Exception($"Unrecognized command {command} sent to {kControlChannelName}.");
            }
        }
    }

    public static partial class ui_ {
        public static readonly ChannelBuffers channelBuffers = new ChannelBuffers();
    }
}