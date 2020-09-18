using UnityEngine;

namespace Unity.UIWidgets.ui {
    public class CommandBufferCanvas : uiRecorderCanvas {
        readonly PictureFlusher _flusher;

        public CommandBufferCanvas(RenderTexture renderTexture, float devicePixelRatio, MeshPool meshPool)
            : base(new uiPictureRecorder()) {
            _flusher = new PictureFlusher(renderTexture, devicePixelRatio, meshPool);
        }

        public override float getDevicePixelRatio() {
            return _flusher.getDevicePixelRatio();
        }

        public override void flush() {
            var picture = _recorder.endRecording();
            _flusher.flush(picture);
            _recorder.reset();
            ObjectPool<uiPicture>.release(picture);
        }

        public void dispose() {
            _flusher.dispose();
        }
    }
}