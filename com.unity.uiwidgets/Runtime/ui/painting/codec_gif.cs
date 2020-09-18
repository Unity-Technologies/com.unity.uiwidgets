using System;
using System.Collections;
using System.IO;
using Unity.UIWidgets.foundation;
using UnityEngine;

namespace Unity.UIWidgets.ui {
    public class GifCodec : Codec {
        public static bool isGif(byte[] bytes) {
            return bytes != null && bytes.Length >= 3 && bytes[0] == 'G' && bytes[1] == 'I' && bytes[2] == 'F';
        }

        public class FrameData {
            public FrameInfo frameInfo;
            public GifDecoder.GifFrame gifFrame;
        }

        volatile byte[] _bytes;
        volatile int _width;
        volatile int _height;
        volatile int _frameCount;
        volatile int _repetitionCount;
        volatile bool _isDone;
        volatile int _frameIndex;
        volatile Texture2D _texture;
        volatile FrameData _frameData;
        volatile Image _image;
        volatile GifDecoder _decoder;
        volatile MemoryStream _stream;
        IEnumerator _coroutine;

        public GifCodec(byte[] bytes) {
            D.assert(bytes != null);
            D.assert(isGif(bytes));

            _frameCount = 0;
            _repetitionCount = 0;
            _isDone = false;
            _frameIndex = 0;
            _bytes = bytes;
            _coroutine = _startDecoding();
            _decoder = new GifDecoder();
            _stream = new MemoryStream(_bytes);
            _frameData = new FrameData() {
                frameInfo = new FrameInfo()
            };
        }

        IEnumerator _startDecoding() {
            _stream.Seek(0, SeekOrigin.Begin);

            if (_decoder.read(_stream) != GifDecoder.STATUS_OK) {
                throw new Exception("Failed to decode gif.");
            }

            _width = _decoder.frameWidth;
            _height = _decoder.frameHeight;

            if (_texture == null) {
                _texture = new Texture2D(_width, _height, TextureFormat.BGRA32, false);
                _texture.hideFlags = HideFlags.HideAndDontSave;
                _image = new Image(_texture, isDynamic: true);
                _frameData.frameInfo.image = _image;
            }

            _frameData.gifFrame = _decoder.currentFrame;
            D.assert(_frameData.gifFrame != null);

            int i = 0;
            while (true) {
                if (_decoder.nextFrame() != GifDecoder.STATUS_OK) {
                    throw new Exception("Failed to decode gif.");
                }

                if (_decoder.done) {
                    break;
                }


                i++;

                yield return null;
            }

            D.assert(_decoder.frameCount == i);
            _frameCount = _decoder.frameCount;
            _repetitionCount = _decoder.loopCount;
            _isDone = true;
        }

        public int frameCount {
            get { return _frameCount; }
        }

        public int repetitionCount {
            get { return _repetitionCount - 1; }
        }


        void _nextFrame() {
            _frameIndex++;

            _coroutine.MoveNext();

            if (_isDone && _frameIndex >= _frameCount) {
                _frameIndex = 0;
                _isDone = false;
                _coroutine = _startDecoding();
                _coroutine.MoveNext();
            }
        }

        public FrameInfo getNextFrame() {
            _nextFrame();
            _texture.LoadRawTextureData(_frameData.gifFrame.bytes);
            _texture.Apply();
            _frameData.frameInfo.duration = TimeSpan.FromMilliseconds(_frameData.gifFrame.delay);
            return _frameData.frameInfo;
        }

        public void Dispose() {
            _decoder.Dispose();
        }
    }
}