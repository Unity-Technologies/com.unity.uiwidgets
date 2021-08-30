using Unity.UIWidgets.animation;
using Unity.UIWidgets.painting;

namespace Unity.UIWidgets.rendering {
    
    public class FractionalOffsetTween : Tween<FractionalOffset> {
        public FractionalOffsetTween(FractionalOffset begin = null, FractionalOffset end = null)
            : base(begin: begin, end: end) {
        }

        public override FractionalOffset lerp(float t) {
            return FractionalOffset.lerp(begin, end, t);
        }
    }
    
    public class AlignmentTween : Tween<Alignment> {
        public AlignmentTween(
            Alignment begin = null,
            Alignment end = null)
            : base(begin: begin, end: end) {
        }

        public override Alignment lerp(float t) {
            return Alignment.lerpAlignment(begin, end, t);
        }
    }
    public class AlignmentGeometryTween : Tween<AlignmentGeometry> {
        public AlignmentGeometryTween(
            AlignmentGeometry begin = null,
            AlignmentGeometry end = null
        ) : base(begin: begin, end: end) {
        }
        public override AlignmentGeometry lerp(float t) {
            return AlignmentGeometry.lerp(begin, end, t);
        }
    }
    
}