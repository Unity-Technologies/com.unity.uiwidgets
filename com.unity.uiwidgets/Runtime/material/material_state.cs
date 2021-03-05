using System.Collections.Generic;
using Unity.UIWidgets.material;
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

    abstract class MaterialStateProperty<T> {
        public abstract T resolve(HashSet<MaterialState> states);


        public static T resolveAs<T>(T value, HashSet<MaterialState> states) {
            if (value is MaterialStateProperty<T> materialStateProperty) {
                MaterialStateProperty<T> property = materialStateProperty;
                return property.resolve(states);
            }

            return value;
        }

        public static MaterialStateProperty<T> resolveWith<T>(material_.MaterialPropertyResolver<T> callback) =>
            new _MaterialStateProperty<T>(callback);
    }


    class _MaterialStateProperty<T> : MaterialStateProperty<T> {
        internal _MaterialStateProperty(material_.MaterialPropertyResolver<T> _resolve) {
            this._resolve = _resolve;
        }

        readonly material_.MaterialPropertyResolver<T> _resolve;

        public override T resolve(HashSet<MaterialState> states) => _resolve(states);

        public static T resolveAs<T>(T value, HashSet<MaterialState> states) {
            if (value is MaterialStateProperty<T> materialStateProperty) {
                MaterialStateProperty<T> property = materialStateProperty;
                return property.resolve(states);
            }

            return value;
        }

        public static MaterialStateProperty<T> resolveWith<T>(material_.MaterialPropertyResolver<T> callback) =>
            new _MaterialStateProperty<T>(callback);
    }
}