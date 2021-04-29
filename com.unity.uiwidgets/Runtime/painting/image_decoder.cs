using Unity.UIWidgets.async;
using Unity.UIWidgets.ui;

namespace Unity.UIWidgets.painting {
    public partial class painting_ {
        public static Future<ui.Image> decodeImageFromList(byte[] bytes) {
            Future<Codec> codec = PaintingBinding.instance.instantiateImageCodec(bytes);
            Future<FrameInfo> frameInfo = codec.then_<FrameInfo>(code => code.getNextFrame());
            var result = frameInfo.then_<Image>(frame => FutureOr.value(frame.image));
            return result;
        }
    }
}