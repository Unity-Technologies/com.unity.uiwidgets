using System;
using RSG;
using Unity.UIWidgets.foundation;
using UnityEngine;

namespace Unity.UIWidgets.uiOld{
    public class FrameInfo {
        public Image image;
        public TimeSpan duration;
    }

    public interface Codec : IDisposable {
        int frameCount { get; }
        int repetitionCount { get; }
        FrameInfo getNextFrame();
    }

    public class ImageCodec : Codec {
        Image _image;

        public ImageCodec(Image image) {
            D.assert(image != null);
            _image = image;
        }

        public int frameCount {
            get { return 1; }
        }

        public int repetitionCount {
            get { return 0; }
        }

        public FrameInfo getNextFrame() {
            D.assert(_image != null);

            return new FrameInfo {
                duration = TimeSpan.Zero,
                image = _image
            };
        }

        public void Dispose() {
            if (_image != null) {
                _image.Dispose();
                _image = null;
            }
        }
    }


    public static class CodecUtils {
        public static Future<Codec> getCodec(byte[] bytes) {
            if (GifCodec.isGif(bytes)) {
                return Promise<Codec>.Resolved(new GifCodec(bytes));
            }

            var texture = new Texture2D(2, 2);
            texture.hideFlags = HideFlags.HideAndDontSave;
            texture.LoadImage(bytes);
            return Promise<Codec>.Resolved(new ImageCodec(new Image(texture)));
        }

        public static Future<Codec> getCodec(Image image) {
            return Promise<Codec>.Resolved(new ImageCodec(image));
        }
    }
}