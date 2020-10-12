namespace Unity.UIWidgets.uiOld{
    public partial class uiPath {

        public enum uiPathShapeHint {
            Rect,
            Circle,
            NaiveRRect,
            Other
        }

        public bool isRect {
            get { return _shapeHint == uiPathShapeHint.Rect; }
        }

        public bool isCircle {
            get { return _shapeHint == uiPathShapeHint.Circle; }
        }
    }
}