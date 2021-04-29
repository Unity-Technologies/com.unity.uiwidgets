using Unity.UIWidgets.animation;

namespace Unity.UIWidgets.material {
    public partial class material_ {
        public static readonly Curve standardEasing = Curves.fastOutSlowIn;

        public static readonly Curve accelerateEasing = new Cubic(0.4f, 0.0f, 1.0f, 1.0f);

        public static readonly Curve decelerateEasing = new Cubic(0.0f, 0.0f, 0.2f, 1.0f);
    }
}