using System;
using System.Collections.Generic;
using Unity.UIWidgets.async;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.gestures;
using Unity.UIWidgets.ui;

namespace Unity.UIWidgets.services {
    public class ServicesBinding : GestureBinding {
        protected override void initInstances() {
            base.initInstances();
            instance = this;

            _defaultBinaryMessenger = createBinaryMessenger();
            window.onPlatformMessage = defaultBinaryMessenger.handlePlatformMessage;
            //SystemChannels.system.setMessageHandler(handleSystemMessage);
        }
        
        public new static ServicesBinding instance {
            get { return (ServicesBinding) Window.instance._binding; }
            private set { Window.instance._binding = value; }
        }

        public BinaryMessenger defaultBinaryMessenger => _defaultBinaryMessenger;
        BinaryMessenger _defaultBinaryMessenger;

        protected BinaryMessenger createBinaryMessenger() {
            return new _DefaultBinaryMessenger();
        }

        protected virtual Future handleSystemMessage(Object systemMessage) {
            return Future.value();
        }

        protected virtual void evict(string asset) {
            services_.rootBundle.evict(asset);
        }
    }

    class _DefaultBinaryMessenger : BinaryMessenger {
        internal _DefaultBinaryMessenger() {
        }

        readonly Dictionary<string, MessageHandler> _handlers = new Dictionary<string, MessageHandler>();

        Future<byte[]> _sendPlatformMessage(string channel, byte[] message) {
            Completer completer = Completer.create();

            Window.instance.sendPlatformMessage(channel, message, (reply) => {
                try {
                    completer.complete(FutureOr.value(reply));
                }
                catch (Exception exception) {
                    UIWidgetsError.reportError(new UIWidgetsErrorDetails(
                        exception: exception,
                        library: "services library",
                        context: new ErrorDescription("during a platform message response callback")
                    ));
                }
            });

            return completer.future.to<byte[]>();
        }

        public Future handlePlatformMessage(
            string channel, byte[] data,
            PlatformMessageResponseCallback callback) {
            MessageHandler handler = _handlers[channel];
            if (handler == null) {
                ui_.channelBuffers.push(channel, data, callback);
                return Future.value();
            }

            return handler(data).then(bytes => {
                var response = (byte[]) bytes;
                callback(response);
                return FutureOr.nil;
            }, onError: exception => {
                UIWidgetsError.reportError(new UIWidgetsErrorDetails(
                    exception: exception,
                    library: "services library",
                    context: new ErrorDescription("during a platform message callback"))
                );
                callback(null);
                return FutureOr.nil;
            });
        }

        public Future<byte[]> send(string channel, byte[] message) {
            return _sendPlatformMessage(channel, message);
        }

        public void setMessageHandler(string channel, MessageHandler handler) {
            if (handler == null)
                _handlers.Remove(channel);
            else
                _handlers[channel] = handler;
            ui_.channelBuffers.drain(channel,
                (byte[] data, PlatformMessageResponseCallback callback) =>
                    handlePlatformMessage(channel, data, callback));
        }
    }
}