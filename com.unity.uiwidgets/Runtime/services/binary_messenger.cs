using System;
using Unity.UIWidgets.async2;
using Unity.UIWidgets.ui2;

namespace Unity.UIWidgets.services {
    public delegate Future<byte[]> MessageHandler(byte[] message);

    public interface BinaryMessenger {
        Future handlePlatformMessage(string channel, byte[] data, PlatformMessageResponseCallback callback);

        Future<byte[]> send(string channel, byte[] message);

        void setMessageHandler(string channel, MessageHandler handler);
    }
}