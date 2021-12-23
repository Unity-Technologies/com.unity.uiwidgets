using System.Collections.Generic;
using Unity.UIWidgets.ui;

namespace Unity.UIWidgets.material {
    public enum MaterialState {
        hovered,

        focused,

        pressed,

        dragged,

        selected,

        disabled,

        error,
    }

    public partial class material_ {
        public delegate T MaterialPropertyResolver<T>(HashSet<MaterialState> states);
    }

    abstract class MaterialStateColor : Color {
        public MaterialStateColor(uint defaultValue) : base(defaultValue) {
        }

        static MaterialStateColor resolveWith(material_.MaterialPropertyResolver<Color> callback) =>
            new _MaterialStateColor(callback);

        public abstract Color resolve(HashSet<MaterialState> states);


        public static Color resolveAs<Color>(Color value, HashSet<MaterialState> states) {
            if (value is MaterialStateProperty<Color> materialStateProperty) {
                MaterialStateProperty<Color> property = materialStateProperty;
                return property.resolve(states);
            }

            return value;
        }

        public static MaterialStateProperty<Color> resolveWith<Color>(
            material_.MaterialPropertyResolver<Color> callback) => new _MaterialStateProperty<Color>(callback);
    }

    class _MaterialStateColor : MaterialStateColor {
        internal _MaterialStateColor(material_.MaterialPropertyResolver<Color> _resolve) : base(_resolve(_defaultStates)
            .value) {
            this._resolve = _resolve;
        }

        readonly material_.MaterialPropertyResolver<Color> _resolve;

        public static readonly HashSet<MaterialState> _defaultStates = new HashSet<MaterialState>();

        public override Color resolve(HashSet<MaterialState> states) => _resolve(states);
    }

    interface IMaterialStateProperty<T> {
        T resolve(HashSet<MaterialState> states);
    }

    abstract class MaterialStateProperty<T> : IMaterialStateProperty<T> {
        public abstract T resolve(HashSet<MaterialState> states);

        public static S resolveAsMaterialStateProperty<S>(S value, HashSet<MaterialState> states) {
            if (value is MaterialStateProperty<S> materialStateProperty) {
                MaterialStateProperty<S> property = materialStateProperty;
                return property.resolve(states);
            }

            return value;
        }

        public static MaterialStateProperty<S> resolveWithMaterialStateProperty<S>(material_.MaterialPropertyResolver<S> callback) =>
            new _MaterialStateProperty<S>(callback);
    }


    class _MaterialStateProperty<T> : MaterialStateProperty<T> {
        internal _MaterialStateProperty(material_.MaterialPropertyResolver<T> _resolve) {
            this._resolve = _resolve;
        }

        readonly material_.MaterialPropertyResolver<T> _resolve;

        public override T resolve(HashSet<MaterialState> states) => _resolve(states);

        public static S resolveAs_MaterialStateProperty<S>(S value, HashSet<MaterialState> states) {
            if (value is MaterialStateProperty<S> materialStateProperty) {
                MaterialStateProperty<S> property = materialStateProperty;
                return property.resolve(states);
            }
            return value;
        }

        public static MaterialStateProperty<S> resolveWith_MaterialStateProperty<S>(material_.MaterialPropertyResolver<S> callback) =>
            new _MaterialStateProperty<S>(callback);
    }
}