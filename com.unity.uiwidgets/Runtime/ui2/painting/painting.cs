using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using AOT;
using RSG;
using Unity.UIWidgets.foundation;
using UnityEngine;
using Unity.UIWidgets.ui;
using Rect = Unity.UIWidgets.ui.Rect;

namespace Unity.UIWidgets.ui2 {
    static class PaintingUtils {
        internal static bool _rectIsValid(Rect rect) {
            D.assert(rect != null, () => "Rect argument was null.");
            D.assert(!rect.hasNaN, () => "Rect argument contained a NaN value.");
            return true;
        }

        internal static bool _offsetIsValid(Offset offset) {
            D.assert(offset != null, () => "Offset argument was null.");
            D.assert(!offset.dx.isNaN() && !offset.dy.isNaN(), () => "Offset argument contained a NaN value.");
            return true;
        }

        internal static bool _matrix4IsValid(float[] matrix4) {
            D.assert(matrix4 != null, () => "Matrix4 argument was null.");
            D.assert(matrix4.Length == 16, () => "Matrix4 must have 16 entries.");
            D.assert(matrix4.All((float value) => value.isFinite()), () => "Matrix4 entries must be finite.");
            return true;
        }

        internal static bool _radiusIsValid(Radius radius) {
            D.assert(radius != null, () => "Radius argument was null.");
            D.assert(!radius.x.isNaN() && !radius.y.isNaN(), () => "Radius argument contained a NaN value.");
            return true;
        }

        internal static Color _scaleAlpha(Color a, float factor) {
            return a.withAlpha((a.alpha * factor).round().clamp(0, 255));
        }

        internal static unsafe int getInt32(this byte[] bytes, int byteOffset) {
            D.assert(byteOffset % 4 == 0);
            fixed (byte* b = &bytes[byteOffset]) {
                return *(int*) b;
            }
        }

        internal static unsafe void setInt32(this byte[] bytes, int byteOffset, int value) {
            D.assert(byteOffset % 4 == 0);
            fixed (byte* b = &bytes[byteOffset]) {
                *(int*) b = value;
            }
        }

        internal static unsafe float getFloat32(this byte[] bytes, int byteOffset) {
            D.assert(byteOffset % 4 == 0);
            fixed (byte* b = &bytes[byteOffset]) {
                return *(float*) b;
            }
        }

        internal static unsafe void setFloat32(this byte[] bytes, int byteOffset, float value) {
            D.assert(byteOffset % 4 == 0);
            fixed (byte* b = &bytes[byteOffset]) {
                *(float*) b = value;
            }
        }

        internal static uint[] _encodeColorList(List<Color> colors) {
            int colorCount = colors.Count;
            var result = new uint[colorCount];
            for (int i = 0; i < colorCount; ++i)
                result[i] = colors[i].value;
            return result;
        }

        internal static float[] _encodePointList(List<Offset> points) {
            D.assert(points != null);
            int pointCount = points.Count;
            var result = new float[pointCount * 2];
            for (int i = 0; i < pointCount; ++i) {
                int xIndex = i * 2;
                int yIndex = xIndex + 1;
                Offset point = points[i];
                D.assert(_offsetIsValid(point));
                result[xIndex] = point.dx;
                result[yIndex] = point.dy;
            }

            return result;
        }

        internal static float[] _encodeTwoPoints(Offset pointA, Offset pointB) {
            D.assert(_offsetIsValid(pointA));
            D.assert(_offsetIsValid(pointB));
            var result = new float[4];
            result[0] = pointA.dx;
            result[1] = pointA.dy;
            result[2] = pointB.dx;
            result[3] = pointB.dy;
            return result;
        }
    }

    public class Color : IEquatable<Color> {
        public Color(uint value) {
            this.value = value & 0xFFFFFFFF;
        }

        public static readonly Color clear = new Color(0x00000000);

        public static readonly Color black = new Color(0xFF000000);

        public static readonly Color white = new Color(0xFFFFFFFF);

        public static Color fromARGB(int a, int r, int g, int b) {
            return new Color(
                (uint) (((a & 0xff) << 24) |
                        ((r & 0xff) << 16) |
                        ((g & 0xff) << 8) |
                        (b & 0xff)));
        }

        public static Color fromRGBO(int r, int g, int b, float opacity) {
            return new Color(
                (uint) ((((int) (opacity * 0xff) & 0xff) << 24) |
                        ((r & 0xff) << 16) |
                        ((g & 0xff) << 8) |
                        (b & 0xff)));
        }

        public readonly uint value;

        public int alpha {
            get { return (int) ((0xff000000 & value) >> 24); }
        }

        public float opacity {
            get { return alpha / 255.0f; }
        }

        public int red {
            get { return (int) ((0x00ff0000 & value) >> 16); }
        }

        public int green {
            get { return (int) ((0x0000ff00 & value) >> 8); }
        }

        public int blue {
            get { return (int) ((0x000000ff & value) >> 0); }
        }

        public Color withAlpha(int a) {
            return fromARGB(a, red, green, blue);
        }

        public Color withOpacity(float opacity) {
            return withAlpha((int) (opacity * 255));
        }

        public Color withRed(int r) {
            return fromARGB(alpha, r, green, blue);
        }

        public Color withGreen(int g) {
            return fromARGB(alpha, red, g, blue);
        }

        public Color withBlue(int b) {
            return fromARGB(alpha, red, green, b);
        }

        static float _linearizeColorComponent(float component) {
            if (component <= 0.03928f) {
                return component / 12.92f;
            }

            return Mathf.Pow((component + 0.055f) / 1.055f, 2.4f);
        }

        public float computeLuminance() {
            float R = _linearizeColorComponent((float) red / 0xFF);
            float G = _linearizeColorComponent((float) green / 0xFF);
            float B = _linearizeColorComponent((float) blue / 0xFF);
            return 0.2126f * R + 0.7152f * G + 0.0722f * B;
        }

        public static Color lerp(Color a, Color b, float t) {
            if (a == null && b == null) {
                return null;
            }

            if (a == null) {
                return b._scaleAlpha(t);
            }

            if (b == null) {
                return a._scaleAlpha(1.0f - t);
            }

            return fromARGB(
                ((int) MathUtils.lerpFloat(a.alpha, b.alpha, t)).clamp(0, 255),
                ((int) MathUtils.lerpFloat(a.red, b.red, t)).clamp(0, 255),
                ((int) MathUtils.lerpFloat(a.green, b.green, t)).clamp(0, 255),
                ((int) MathUtils.lerpFloat(a.blue, b.blue, t)).clamp(0, 255)
            );
        }

        public bool Equals(Color other) {
            if (ReferenceEquals(null, other)) {
                return false;
            }

            if (ReferenceEquals(this, other)) {
                return true;
            }

            return value == other.value;
        }

        public override bool Equals(object obj) {
            if (ReferenceEquals(null, obj)) {
                return false;
            }

            if (ReferenceEquals(this, obj)) {
                return true;
            }

            if (obj.GetType() != GetType()) {
                return false;
            }

            return Equals((Color) obj);
        }

        public override int GetHashCode() {
            return value.GetHashCode();
        }

        public static bool operator ==(Color a, Color b) {
            return ReferenceEquals(a, null) ? ReferenceEquals(b, null) : a.Equals(b);
        }

        public static bool operator !=(Color a, Color b) {
            return !(a == b);
        }

        public override string ToString() {
            return $"Color(0x{value:X8})";
        }
    }

    public enum Clip {
        none,
        hardEdge,
        antiAlias,
        antiAliasWithSaveLayer,
    }

    public enum PaintingStyle {
        fill,
        stroke,
    }

    public enum FilterQuality {
        none,
        low,
        medium,
        high,
    }

    public enum StrokeCap {
        butt,
        round,
        square,
    }

    public enum StrokeJoin {
        miter,
        round,
        bevel,
    }

    public enum BlurStyle {
        normal, // only normal for now.
        solid,
        outer,
        inner,
    }

    class _ImageFilter : NativeWrapper {
        _ImageFilter(IntPtr ptr, ImageFilter creator) : base(ptr) {
            this.creator = creator;
        }

        protected override void DisposePtr(IntPtr ptr) {
            NativeBindings.ImageFilter_dispose(ptr);
        }

        public static _ImageFilter blur(ImageFilter creator) {
            D.assert(creator != null);
            D.assert(creator._type == ImageFilter._kTypeMatrix);
            if (creator._data.Length != 16) {
                throw new ArgumentException("\"matrix4\" must have 16 entries.");
            }

            var filter = new _ImageFilter(NativeBindings.ImageFilter_constructor(), creator);
            NativeBindings.ImageFilter_initBlur(filter._ptr, creator._data[0], creator._data[1]);

            return filter;
        }

        public static unsafe _ImageFilter matrix(ImageFilter creator) {
            D.assert(creator != null);
            D.assert(creator._type == ImageFilter._kTypeMatrix);
            if (creator._data.Length != 16) {
                throw new ArgumentException("\"matrix4\" must have 16 entries.");
            }

            var filter = new _ImageFilter(NativeBindings.ImageFilter_constructor(), creator);
            fixed (float* data = &creator._data[0]) {
                NativeBindings.ImageFilter_initMatrix(filter._ptr, data, (int) creator._filterQuality);
            }

            return filter;
        }

        public readonly ImageFilter creator;
    }

    class _ColorFilter : NativeWrapper {
        _ColorFilter(IntPtr ptr, ColorFilter creator) : base(ptr) {
            this.creator = creator;
        }

        protected override void DisposePtr(IntPtr ptr) {
            NativeBindings.ColorFilter_dispose(ptr);
        }

        public static _ColorFilter mode(ColorFilter creator) {
            D.assert(creator != null);
            D.assert(creator._type == ColorFilter._TypeMode);

            var filter = new _ColorFilter(NativeBindings.ColorFilter_constructor(), creator);
            NativeBindings.ColorFilter_initMode(filter._ptr, creator._color.value, (int) creator._blendMode);

            return filter;
        }

        public static unsafe _ColorFilter matrix(ColorFilter creator) {
            D.assert(creator != null);
            D.assert(creator._type == ColorFilter._TypeMatrix);

            var filter = new _ColorFilter(NativeBindings.ColorFilter_constructor(), creator);
            fixed (float* matrix = &creator._matrix[0]) {
                NativeBindings.ColorFilter_initMatrix(filter._ptr, matrix);
            }

            return filter;
        }

        public static _ColorFilter linearToSrgbGamma(ColorFilter creator) {
            D.assert(creator != null);
            D.assert(creator._type == ColorFilter._TypeLinearToSrgbGamma);

            var filter = new _ColorFilter(NativeBindings.ColorFilter_constructor(), creator);
            NativeBindings.ColorFilter_initLinearToSrgbGamma(filter._ptr);

            return filter;
        }

        public static _ColorFilter srgbToLinearGamma(ColorFilter creator) {
            D.assert(creator != null);
            D.assert(creator._type == ColorFilter._TypeSrgbToLinearGamma);

            var filter = new _ColorFilter(NativeBindings.ColorFilter_constructor(), creator);
            NativeBindings.ColorFilter_initSrgbToLinearGamma(filter._ptr);

            return filter;
        }

        public readonly ColorFilter creator;
    }

    public abstract class Shader : NativeWrapper {
        protected Shader(IntPtr ptr) : base(ptr) {
        }
    }

    public enum TileMode {
        clamp,
        repeated,
        mirror,
    }

    public class Gradient : Shader {
        Gradient(IntPtr ptr) : base(ptr) {
        }

        protected override void DisposePtr(IntPtr ptr) {
            NativeBindings.Gradient_dispose(ptr);
        }

        public static unsafe Gradient linear(
            Offset from,
            Offset to,
            List<Color> colors,
            List<float> colorStops = null,
            TileMode tileMode = TileMode.clamp,
            float[] matrix4 = null
        ) {
            D.assert(PaintingUtils._offsetIsValid(from));
            D.assert(PaintingUtils._offsetIsValid(to));
            D.assert(colors != null);
            D.assert(matrix4 == null || PaintingUtils._matrix4IsValid(matrix4));

            _validateColorStops(colors, colorStops);
            var endPointsArray = PaintingUtils._encodeTwoPoints(from, to);
            var colorsArray = PaintingUtils._encodeColorList(colors);
            var colorStopsArray = colorStops?.ToArray() ?? new float[] { };

            fixed (float* endPointsBuffer = &endPointsArray[0])
            fixed (uint* colorsBuffer = &colorsArray[0])
            fixed (float* colorStopsBuffer = &colorStopsArray[0])
            fixed (float* matrix4Buffer = &matrix4[0]) {
                var gradient = new Gradient(NativeBindings.Gradient_constructor());
                NativeBindings.Gradient_initLinear(
                    gradient._ptr, endPointsBuffer, endPointsArray.Length,
                    colorsBuffer, colorsArray.Length,
                    colorStopsBuffer, colorStopsArray.Length,
                    (int) tileMode,
                    matrix4Buffer
                );

                return gradient;
            }
        }

        public static unsafe Gradient radial(
            Offset center,
            float radius,
            List<Color> colors, List<float> colorStops = null,
            TileMode tileMode = TileMode.clamp,
            float[] matrix4 = null,
            Offset focal = null,
            float focalRadius = 0.0f) {
            D.assert(PaintingUtils._offsetIsValid(center));
            D.assert(colors != null);
            D.assert(matrix4 == null || PaintingUtils._matrix4IsValid(matrix4));

            _validateColorStops(colors, colorStops);
            var colorsArray = PaintingUtils._encodeColorList(colors);
            var colorStopsArray = colorStops?.ToArray() ?? new float[] { };

            fixed (uint* colorsBuffer = &colorsArray[0])
            fixed (float* colorStopsBuffer = &colorStopsArray[0])
            fixed (float* matrix4Buffer = &matrix4[0]) {
                var gradient = new Gradient(NativeBindings.Gradient_constructor());

                if (focal == null || (focal == center && focalRadius == 0.0f)) {
                    NativeBindings.Gradient_initRadial(gradient._ptr,
                        center.dx, center.dy, radius,
                        colorsBuffer, colorsArray.Length,
                        colorStopsBuffer, colorStopsArray.Length,
                        (int) tileMode, matrix4Buffer);
                }
                else {
                    D.assert(center != Offset.zero || focal != Offset.zero);
                    NativeBindings.Gradient_initConical(gradient._ptr,
                        focal.dx, focal.dy, focalRadius,
                        center.dx, center.dy, radius,
                        colorsBuffer, colorsArray.Length,
                        colorStopsBuffer, colorStopsArray.Length,
                        (int) tileMode, matrix4Buffer);
                }

                return gradient;
            }
        }

        public static unsafe Gradient sweep(
            Offset center,
            List<Color> colors,
            List<float> colorStops = null,
            TileMode tileMode = TileMode.clamp,
            float startAngle = 0.0f,
            float endAngle = Mathf.PI * 2,
            float[] matrix4 = null
        ) {
            D.assert(PaintingUtils._offsetIsValid(center));
            D.assert(colors != null);
            D.assert(startAngle < endAngle);
            D.assert(matrix4 == null || PaintingUtils._matrix4IsValid(matrix4));

            _validateColorStops(colors, colorStops);

            var colorsArray = PaintingUtils._encodeColorList(colors);
            var colorStopsArray = colorStops?.ToArray() ?? new float[] { };

            fixed (uint* colorsBuffer = &colorsArray[0])
            fixed (float* colorStopsBuffer = &colorStopsArray[0])
            fixed (float* matrix4Buffer = &matrix4[0]) {
                var gradient = new Gradient(NativeBindings.Gradient_constructor());
                NativeBindings.Gradient_initSweep(gradient._ptr,
                    center.dx, center.dy,
                    colorsBuffer, colorsArray.Length,
                    colorStopsBuffer, colorStopsArray.Length,
                    (int) tileMode, startAngle, endAngle, matrix4Buffer);
                return gradient;
            }
        }

        static void _validateColorStops(List<Color> colors, List<float> colorStops) {
            if (colorStops == null) {
                if (colors.Count != 2) {
                    throw new ArgumentException("\"colors\" must have length 2 if \"colorStops\" is omitted.");
                }
            }
            else {
                if (colors.Count != colorStops.Count) {
                    throw new ArgumentException("\"colors\" and \"colorStops\" arguments must have equal length.");
                }
            }
        }
    }

    public class ImageShader : Shader {
        unsafe ImageShader(Image image, TileMode tmx, TileMode tmy, float[] matrix4)
            : base(NativeBindings.ImageShader_constructor()) {
            D.assert(image != null);
            D.assert(matrix4 != null);
            if (matrix4.Length != 16) {
                throw new ArgumentException("\"matrix4\" must have 16 entries.");
            }

            fixed (float* matrix4Buffer = &matrix4[0]) {
                NativeBindings.ImageShader_initWithImage(_ptr, image._ptr, (int) tmx, (int) tmy, matrix4Buffer);
            }
        }

        protected override void DisposePtr(IntPtr ptr) {
            NativeBindings.ImageShader_dispose(ptr);
        }
    }

    public class MaskFilter : IEquatable<MaskFilter> {
        MaskFilter(BlurStyle style, float sigma) {
            _style = style;
            _sigma = sigma;
        }

        public static MaskFilter blur(BlurStyle style, float sigma) {
            return new MaskFilter(style, sigma);
        }

        internal readonly BlurStyle _style;
        internal readonly float _sigma;

        internal const int _TypeNone = 0; // null
        internal const int _TypeBlur = 1; // SkBlurMaskFilter

        public bool Equals(MaskFilter other) {
            if (ReferenceEquals(null, other)) {
                return false;
            }

            if (ReferenceEquals(this, other)) {
                return true;
            }

            return _style == other._style && _sigma == other._sigma;
        }

        public override bool Equals(object obj) {
            if (ReferenceEquals(null, obj)) {
                return false;
            }

            if (ReferenceEquals(this, obj)) {
                return true;
            }

            if (obj.GetType() != GetType()) {
                return false;
            }

            return Equals((MaskFilter) obj);
        }

        public override int GetHashCode() {
            unchecked {
                return ((int) _style * 397) ^ _sigma.GetHashCode();
            }
        }

        public static bool operator ==(MaskFilter left, MaskFilter right) {
            return Equals(left, right);
        }

        public static bool operator !=(MaskFilter left, MaskFilter right) {
            return !Equals(left, right);
        }

        public override string ToString() {
            return $"MaskFilter.blur(${_style}, ${_sigma:F1})";
        }
    }

    public class ColorFilter : IEquatable<ColorFilter> {
        ColorFilter(Color color, BlendMode? blendMode, float[] matrix, int type) {
            _color = color;
            _blendMode = blendMode;
            _matrix = matrix;
            _type = type;
        }

        public static ColorFilter mode(Color color, BlendMode blendMode) {
            return new ColorFilter(color, blendMode, null, _TypeMode);
        }

        public static ColorFilter matrix(float[] matrix) {
            return new ColorFilter(null, null, matrix, _TypeMatrix);
        }

        public static ColorFilter linearToSrgbGamma() {
            return new ColorFilter(null, null, null, _TypeLinearToSrgbGamma);
        }

        public static ColorFilter srgbToLinearGamma() {
            return new ColorFilter(null, null, null, _TypeSrgbToLinearGamma);
        }

        internal readonly Color _color;
        internal readonly BlendMode? _blendMode;
        internal readonly float[] _matrix;
        internal readonly int _type;

        internal const int _TypeMode = 1; // MakeModeFilter
        internal const int _TypeMatrix = 2; // MakeMatrixFilterRowMajor255
        internal const int _TypeLinearToSrgbGamma = 3; // MakeLinearToSRGBGamma
        internal const int _TypeSrgbToLinearGamma = 4; // MakeSRGBToLinearGamma

        internal _ColorFilter _toNativeColorFilter() {
            switch (_type) {
                case _TypeMode:
                    if (_color == null || _blendMode == null) {
                        return null;
                    }

                    return _ColorFilter.mode(this);
                case _TypeMatrix:
                    if (_matrix == null) {
                        return null;
                    }

                    D.assert(_matrix.Length == 20, () => "Color Matrix must have 20 entries.");
                    return _ColorFilter.matrix(this);
                case _TypeLinearToSrgbGamma:
                    return _ColorFilter.linearToSrgbGamma(this);
                case _TypeSrgbToLinearGamma:
                    return _ColorFilter.srgbToLinearGamma(this);
                default:
                    throw new Exception($"Unknown mode {_type} for ColorFilter.");
            }
        }

        public bool Equals(ColorFilter other) {
            if (ReferenceEquals(null, other)) {
                return false;
            }

            if (ReferenceEquals(this, other)) {
                return true;
            }

            return Equals(_color, other._color) && _blendMode == other._blendMode &&
                   _matrix.equalsList(other._matrix) && _type == other._type;
        }

        public override bool Equals(object obj) {
            if (ReferenceEquals(null, obj)) {
                return false;
            }

            if (ReferenceEquals(this, obj)) {
                return true;
            }

            if (obj.GetType() != GetType()) {
                return false;
            }

            return Equals((ColorFilter) obj);
        }

        public override int GetHashCode() {
            unchecked {
                var hashCode = (_color != null ? _color.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ _blendMode.GetHashCode();
                hashCode = (hashCode * 397) ^ _matrix.hashList();
                hashCode = (hashCode * 397) ^ _type;
                return hashCode;
            }
        }

        public static bool operator ==(ColorFilter left, ColorFilter right) {
            return Equals(left, right);
        }

        public static bool operator !=(ColorFilter left, ColorFilter right) {
            return !Equals(left, right);
        }

        public override string ToString() {
            switch (_type) {
                case _TypeMode:
                    return $"ColorFilter.mode({_color}, {_blendMode})";
                case _TypeMatrix:
                    return $"ColorFilter.matrix({_matrix.toStringList()})";
                case _TypeLinearToSrgbGamma:
                    return "ColorFilter.linearToSrgbGamma()";
                case _TypeSrgbToLinearGamma:
                    return "ColorFilter.srgbToLinearGamma()";
                default:
                    return "Unknown ColorFilter type.";
            }
        }
    }

    public class ImageFilter : IEquatable<ImageFilter> {
        public static ImageFilter blur(float sigmaX = 0.0f, float sigmaY = 0.0f) {
            return new ImageFilter(_makeList(sigmaX, sigmaY), FilterQuality.none, _kTypeBlur);
        }

        public static ImageFilter matrix(float[] transform, FilterQuality filterQuality = FilterQuality.low) {
            var data = (float[]) transform.Clone();
            return new ImageFilter(data, filterQuality, _kTypeMatrix);
        }

        ImageFilter(float[] data, FilterQuality filterQuality, int type) {
            _data = data;
            _filterQuality = filterQuality;
            _type = type;
        }

        static float[] _makeList(float a, float b) {
            return new[] {a, b};
        }

        internal readonly float[] _data;
        internal readonly FilterQuality _filterQuality;
        internal readonly int _type;
        _ImageFilter _nativeFilter;

        internal const int _kTypeBlur = 0;
        internal const int _kTypeMatrix = 1;

        internal _ImageFilter _toNativeImageFilter() => _nativeFilter ??= _makeNativeImageFilter();

        _ImageFilter _makeNativeImageFilter() {
            if (_data == null) {
                return null;
            }

            switch (_type) {
                case _kTypeBlur:
                    return _ImageFilter.blur(this);
                case _kTypeMatrix:
                    return _ImageFilter.matrix(this);
                default:
                    throw new Exception($"Unknown mode {_type} for ImageFilter.");
            }
        }

        public bool Equals(ImageFilter other) {
            if (ReferenceEquals(null, other)) {
                return false;
            }

            if (ReferenceEquals(this, other)) {
                return true;
            }

            return _type == other._type && _data.equalsList(other._data) && _filterQuality == other._filterQuality;
        }

        public override bool Equals(object obj) {
            if (ReferenceEquals(null, obj)) {
                return false;
            }

            if (ReferenceEquals(this, obj)) {
                return true;
            }

            if (obj.GetType() != GetType()) {
                return false;
            }

            return Equals((ImageFilter) obj);
        }

        public override int GetHashCode() {
            unchecked {
                var hashCode = (int) _filterQuality;
                hashCode = (hashCode * 397) ^ _data.hashList();
                hashCode = (hashCode * 397) ^ _type;
                return hashCode;
            }
        }

        public static bool operator ==(ImageFilter left, ImageFilter right) {
            return Equals(left, right);
        }

        public static bool operator !=(ImageFilter left, ImageFilter right) {
            return !Equals(left, right);
        }

        public override string ToString() {
            switch (_type) {
                case _kTypeBlur:
                    return $"ImageFilter.blur({_data[0]}, {_data[1]})";
                case _kTypeMatrix:
                    return $"ImageFilter.matrix({_data.toStringList()}, {_filterQuality})";
                default:
                    return "Unknown ImageFilter type.";
            }
        }
    }

    public class Paint {
        readonly byte[] _data = new byte[_kDataByteCount];

        const int _kIsAntiAliasIndex = 0;
        const int _kColorIndex = 1;
        const int _kBlendModeIndex = 2;
        const int _kStyleIndex = 3;
        const int _kStrokeWidthIndex = 4;
        const int _kStrokeCapIndex = 5;
        const int _kStrokeJoinIndex = 6;
        const int _kStrokeMiterLimitIndex = 7;
        const int _kFilterQualityIndex = 8;
        const int _kMaskFilterIndex = 9;
        const int _kMaskFilterBlurStyleIndex = 10;
        const int _kMaskFilterSigmaIndex = 11;
        const int _kInvertColorIndex = 12;
        const int _kDitherIndex = 13;

        const int _kIsAntiAliasOffset = _kIsAntiAliasIndex << 2;
        const int _kColorOffset = _kColorIndex << 2;
        const int _kBlendModeOffset = _kBlendModeIndex << 2;
        const int _kStyleOffset = _kStyleIndex << 2;
        const int _kStrokeWidthOffset = _kStrokeWidthIndex << 2;
        const int _kStrokeCapOffset = _kStrokeCapIndex << 2;
        const int _kStrokeJoinOffset = _kStrokeJoinIndex << 2;
        const int _kStrokeMiterLimitOffset = _kStrokeMiterLimitIndex << 2;
        const int _kFilterQualityOffset = _kFilterQualityIndex << 2;
        const int _kMaskFilterOffset = _kMaskFilterIndex << 2;
        const int _kMaskFilterBlurStyleOffset = _kMaskFilterBlurStyleIndex << 2;
        const int _kMaskFilterSigmaOffset = _kMaskFilterSigmaIndex << 2;
        const int _kInvertColorOffset = _kInvertColorIndex << 2;

        const int _kDitherOffset = _kDitherIndex << 2;

        // If you add more fields, remember to update _kDataByteCount.
        const int _kDataByteCount = 56;

        object[] _objects;
        const int _kShaderIndex = 0;
        const int _kColorFilterIndex = 1;
        const int _kImageFilterIndex = 2;
        const int _kObjectCount = 3; // Must be one larger than the largest index.

        public Paint() {
            if (enableDithering) {
                _dither = true;
            }
        }

        public bool isAntiAlias {
            get { return _data.getInt32(_kIsAntiAliasOffset) == 0; }
            set {
                int encoded = value ? 0 : 1;
                _data.setInt32(_kIsAntiAliasOffset, encoded);
            }
        }

        const uint _kColorDefault = 0xFF000000;

        public Color color {
            get {
                int encoded = _data.getInt32(_kColorOffset);
                return new Color((uint) (encoded ^ _kColorDefault));
            }

            set {
                D.assert(value != null);
                int encoded = (int) (value.value ^ _kColorDefault);
                _data.setInt32(_kColorOffset, encoded);
            }
        }

        const int _kBlendModeDefault = (int) BlendMode.srcOver;

        public BlendMode blendMode {
            get {
                int encoded = _data.getInt32(_kBlendModeOffset);
                return (BlendMode) (encoded ^ _kBlendModeDefault);
            }
            set {
                int encoded = (int) value ^ _kBlendModeDefault;
                _data.setInt32(_kBlendModeOffset, encoded);
            }
        }

        public PaintingStyle style {
            get { return (PaintingStyle) _data.getInt32(_kStyleOffset); }
            set {
                int encoded = (int) value;
                _data.setInt32(_kStyleOffset, encoded);
            }
        }

        float strokeWidth {
            get { return _data.getFloat32(_kStrokeWidthOffset); }
            set {
                float encoded = value;
                _data.setFloat32(_kStrokeWidthOffset, encoded);
            }
        }

        public StrokeCap strokeCap {
            get { return (StrokeCap) (_data.getInt32(_kStrokeCapOffset)); }
            set {
                int encoded = (int) value;
                _data.setInt32(_kStrokeCapOffset, encoded);
            }
        }

        public StrokeJoin strokeJoin {
            get { return (StrokeJoin) (_data.getInt32(_kStrokeJoinOffset)); }
            set {
                int encoded = (int) value;
                _data.setInt32(_kStrokeJoinOffset, encoded);
            }
        }

        const float _kStrokeMiterLimitDefault = 4.0f;

        public float strokeMiterLimit {
            get { return _data.getFloat32(_kStrokeMiterLimitOffset) + _kStrokeMiterLimitDefault; }
            set {
                float encoded = value - _kStrokeMiterLimitDefault;
                _data.setFloat32(_kStrokeMiterLimitOffset, encoded);
            }
        }

        public MaskFilter maskFilter {
            get {
                switch (_data.getInt32(_kMaskFilterOffset)) {
                    case MaskFilter._TypeNone:
                        return null;
                    case MaskFilter._TypeBlur:
                        return MaskFilter.blur(
                            (BlurStyle) (_data.getInt32(_kMaskFilterBlurStyleOffset)),
                            _data.getFloat32(_kMaskFilterSigmaOffset)
                        );
                }

                return null;
            }

            set {
                if (value == null) {
                    _data.setInt32(_kMaskFilterOffset, MaskFilter._TypeNone);
                    _data.setInt32(_kMaskFilterBlurStyleOffset, 0);
                    _data.setFloat32(_kMaskFilterSigmaOffset, 0.0f);
                }
                else {
                    // For now we only support one kind of MaskFilter, so we don"t need to
                    // check what the type is if it"s not null.
                    _data.setInt32(_kMaskFilterOffset, MaskFilter._TypeBlur);
                    _data.setInt32(_kMaskFilterBlurStyleOffset, (int) value._style);
                    _data.setFloat32(_kMaskFilterSigmaOffset, value._sigma);
                }
            }
        }

        public FilterQuality filterQuality {
            get { return (FilterQuality) (_data.getInt32(_kFilterQualityOffset)); }
            set {
                int encoded = (int) value;
                _data.setInt32(_kFilterQualityOffset, encoded);
            }
        }

        public Shader shader {
            get {
                if (_objects == null)
                    return null;
                return _objects[_kShaderIndex] as Shader;
            }
            set {
                _objects ??= new object[_kObjectCount];
                _objects[_kShaderIndex] = value;
            }
        }

        public ColorFilter colorFilter {
            get {
                if (_objects == null || _objects[_kColorFilterIndex] == null) {
                    return null;
                }

                return ((_ColorFilter) _objects[_kColorFilterIndex]).creator;
            }

            set {
                _ColorFilter nativeFilter = value?._toNativeColorFilter();
                if (nativeFilter == null) {
                    if (_objects != null) {
                        _objects[_kColorFilterIndex] = null;
                    }
                }
                else {
                    if (_objects == null) {
                        _objects = new object[_kObjectCount];
                        _objects[_kColorFilterIndex] = nativeFilter;
                    }
                    else if (((_ColorFilter) _objects[_kColorFilterIndex])?.creator != value) {
                        _objects[_kColorFilterIndex] = nativeFilter;
                    }
                }
            }
        }

        public ImageFilter imageFilter {
            get {
                if (_objects == null || _objects[_kImageFilterIndex] == null) {
                    return null;
                }

                return ((_ImageFilter) _objects[_kImageFilterIndex]).creator;
            }
            set {
                if (value == null) {
                    if (_objects != null) {
                        _objects[_kImageFilterIndex] = null;
                    }
                }
                else {
                    _objects ??= new object[_kObjectCount];
                    if (((_ImageFilter) _objects[_kImageFilterIndex])?.creator != value) {
                        _objects[_kImageFilterIndex] = value._toNativeImageFilter();
                    }
                }
            }
        }

        public bool invertColors {
            get { return _data.getInt32(_kInvertColorOffset) == 1; }
            set { _data.setInt32(_kInvertColorOffset, value ? 1 : 0); }
        }

        bool _dither {
            get { return _data.getInt32(_kDitherOffset) == 1; }
            set { _data.setInt32(_kDitherOffset, value ? 1 : 0); }
        }

        static bool enableDithering = false;

        public override string ToString() {
            var result = new StringBuilder();
            string semicolon = "";
            result.Append("Paint(");
            if (style == PaintingStyle.stroke) {
                result.Append($"{style}");
                if (strokeWidth != 0.0f) {
                    result.Append($" {strokeWidth:F1}");
                }
                else {
                    result.Append(" hairline");
                }

                if (strokeCap != StrokeCap.butt) {
                    result.Append($" {strokeCap}");
                }

                if (strokeJoin == StrokeJoin.miter) {
                    if (strokeMiterLimit != _kStrokeMiterLimitDefault) {
                        result.Append($" {strokeJoin} up to {strokeMiterLimit:F1}");
                    }
                }
                else {
                    result.Append($" {strokeJoin}");
                }

                semicolon = "; ";
            }

            if (isAntiAlias != true) {
                result.Append($"{semicolon}antialias off");
                semicolon = "; ";
            }

            if (color?.value != _kColorDefault) {
                if (color != null) {
                    result.Append($"{semicolon}{color}");
                }
                else {
                    result.Append($"{semicolon}no color");
                }

                semicolon = "; ";
            }

            if ((int) blendMode != _kBlendModeDefault) {
                result.Append($"{semicolon}{blendMode}");
                semicolon = "; ";
            }

            if (colorFilter != null) {
                result.Append($"{semicolon}colorFilter: {colorFilter}");
                semicolon = "; ";
            }

            if (maskFilter != null) {
                result.Append($"{semicolon}maskFilter: {maskFilter}");
                semicolon = "; ";
            }

            if (filterQuality != FilterQuality.none) {
                result.Append($"{semicolon}filterQuality: {filterQuality}");
                semicolon = "; ";
            }

            if (shader != null) {
                result.Append($"{semicolon}shader: {shader}");
                semicolon = "; ";
            }

            if (imageFilter != null) {
                result.Append($"{semicolon}imageFilter: {imageFilter}");
                semicolon = "; ";
            }

            if (invertColors) {
                result.Append($"{semicolon}invert: {invertColors}");
            }

            if (_dither) {
                result.Append($"{semicolon}dither: {_dither}");
            }

            result.Append(")");
            return result.ToString();
        }
    }

    public static class Conversions {
        public static UnityEngine.Color toColor(this Color color) {
            return new UnityEngine.Color(
                color.red / 255f, color.green / 255f, color.blue / 255f, color.alpha / 255f);
        }

        public static Color32 toColor32(this Color color) {
            return new Color32(
                (byte) color.red, (byte) color.green, (byte) color.blue, (byte) color.alpha);
        }

        public static Vector2 toVector(this Offset offset) {
            return new Vector2(offset.dx, offset.dy);
        }

        public static UnityEngine.Rect toRect(this Rect rect) {
            return new UnityEngine.Rect(rect.left, rect.top, rect.width, rect.height);
        }

        public static float alignToPixel(this float v, float devicePixelRatio) {
            return Mathf.Round(v * devicePixelRatio) / devicePixelRatio;
        }

        internal static Color _scaleAlpha(this Color a, float factor) {
            return a.withAlpha((a.alpha * factor).round().clamp(0, 255));
        }
    }

    public enum BlendMode {
        clear,
        src,
        dst,
        srcOver,
        dstOver,
        srcIn,
        dstIn,
        srcOut,
        dstOut,
        srcATop,
        dstATop,
        xor,
        plus,

        // REF: https://www.w3.org/TR/compositing-1/#blendingseparable
        modulate,
        screen,
        overlay,
        darken,
        lighten,
        colorDodge,
        colorBurn,
        hardLight,
        softLight,
        difference,
        exclusion,
        multiply,

        // REF: https://www.w3.org/TR/compositing-1/#blendingnonseparable
        hue,
        saturation,
        color,
        luminosity,
    }

    public enum ImageByteFormat {
        rawRgba,
        rawUnmodified,
        png,
    }

    public class Image : NativeWrapperDisposable {
        internal Image(IntPtr ptr) : base(ptr) {
        }

        protected override void DisposePtr(IntPtr ptr) {
            NativeBindings.Image_dispose(_ptr);
        }

        public int width => NativeBindings.Image_width(_ptr);

        public int height => NativeBindings.Image_height(_ptr);

        public Promise<byte[]> toByteData(
            ImageByteFormat format = ImageByteFormat.rawRgba
        ) {
            var completer = new Promise<byte[]>(true);
            GCHandle completerHandle = GCHandle.Alloc(completer);

            IntPtr error =
                NativeBindings.Image_toByteData(_ptr, (int) format, _toByteDataCallback,
                    (IntPtr) completerHandle);
            if (error != null) {
                completerHandle.Free();
                throw new Exception(Marshal.PtrToStringAnsi(error));
            }

            return completer;
        }

        [MonoPInvokeCallback(typeof(NativeBindings.Image_toByteDataCallback))]
        static void _toByteDataCallback(IntPtr callbackHandle, IntPtr data, int length) {
            GCHandle completerHandle = (GCHandle) callbackHandle;
            var completer = (Promise<byte[]>) completerHandle.Target;
            completerHandle.Free();

            if (data == IntPtr.Zero || length == 0) {
                completer.Resolve(new byte[0]);
            }
            else {
                var bytes = new byte[length];
                Marshal.Copy(data, bytes, 0, length);
                completer.Resolve(bytes);
            }
        }

        public override string ToString() => $"[{width}\u00D7{height}]";
    }

    public class Canvas : NativeWrapper {
        public Canvas(PictureRecorder recorder, Rect cullRect = null) {
            D.assert(recorder != null);
            if (recorder.isRecording) {
                throw new ArgumentException("\"recorder\" must not already be associated with another Canvas.");
            }

            cullRect ??= Rect.largest;
            _ptr = NativeBindings.Canvas_constructor(recorder._ptr, cullRect.left, cullRect.top,
                cullRect.right,
                cullRect.bottom);
        }

        protected override void DisposePtr(IntPtr ptr) {
            NativeBindings.Canvas_dispose(ptr);
        }

        public void save() {
            NativeBindings.Canvas_save(_ptr);
        }

        public void saveLayer(Rect bounds, Paint paint) {
            D.assert(paint != null);
            if (bounds == null) {
                //_saveLayerWithoutBounds(paint._objects, paint._data);
            }
            else {
                D.assert(PaintingUtils._rectIsValid(bounds));
                // _saveLayer(bounds.left, bounds.top, bounds.right, bounds.bottom,
                //     paint._objects, paint._data);
            }
        }

        public void restore() {
            NativeBindings.Canvas_restore(_ptr);
        }
    }

    public class Picture : NativeWrapperDisposable {
        internal Picture(IntPtr ptr) : base(ptr) {
        }

        protected override void DisposePtr(IntPtr ptr) {
            NativeBindings.Picture_dispose(ptr);
        }

        public Promise<Image> toImage(int width, int height) {
            if (width <= 0 || height <= 0) {
                throw new ArgumentException("Invalid image dimensions.");
            }

            var completer = new Promise<Image>(true);
            GCHandle completerHandle = GCHandle.Alloc(completer);

            IntPtr error =
                NativeBindings.Picture_toImage(_ptr, width, height, _toImageCallback,
                    (IntPtr) completerHandle);
            if (error != null) {
                completerHandle.Free();
                throw new Exception(Marshal.PtrToStringAnsi(error));
            }

            return completer;
        }

        [MonoPInvokeCallback(typeof(NativeBindings.Picture_toImageCallback))]
        static void _toImageCallback(IntPtr callbackHandle, IntPtr result) {
            GCHandle completerHandle = (GCHandle) callbackHandle;
            var completer = (Promise<Image>) completerHandle.Target;
            completerHandle.Free();

            if (result == IntPtr.Zero) {
                completer.Reject(new Exception("operation failed"));
            }
            else {
                var image = new Image(result);
                completer.Resolve(image);
            }
        }

        public int approximateBytesUsed => NativeBindings.Picture_GetAllocationSize(_ptr);
    }

    public class PictureRecorder : NativeWrapper {
        public PictureRecorder() : base(NativeBindings.PictureRecorder_constructor()) {
        }

        protected override void DisposePtr(IntPtr ptr) {
            NativeBindings.PictureRecorder_dispose(ptr);
        }

        public bool isRecording => NativeBindings.PictureRecorder_isRecording(_ptr);

        public Picture endRecording() {
            return new Picture(NativeBindings.PictureRecorder_endRecording(_ptr));
        }
    }

    public class Shadow {
    }
}