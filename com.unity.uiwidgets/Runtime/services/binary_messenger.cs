using System;
using Unity.UIWidgets.async;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.ui;


namespace Unity.UIWidgets.services {
    public delegate Future<byte[]> MessageHandler(byte[] message);

    public interface BinaryMessenger {
        Future handlePlatformMessage(string channel, byte[] data, PlatformMessageResponseCallback callback);

        Future<byte[]> send(string channel, byte[] message);

        void setMessageHandler(string channel, MessageHandler handler);
        
        
    }
}