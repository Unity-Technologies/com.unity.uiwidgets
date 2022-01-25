using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using AOT;
using Unity.UIWidgets.animation;
using Unity.UIWidgets.async;
using Unity.UIWidgets.external;
using Unity.UIWidgets.foundation;
using UnityEngine;
using Unity.UIWidgets.ui;
using Rect = Unity.UIWidgets.ui.Rect;

namespace Unity.UIWidgets.ui {
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

        internal static bool _rrectIsValid(RRect rrect) {
            D.assert(rrect != null, () => "RRect argument was null.");
            D.assert(!rrect.hasNaN, () => "RRect argument contained a NaN value.");
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

        internal static T[] range<T>(this T[] data, int index, int length)
        {
            T[] result = new T[length];
            Array.Copy(data, index, result, 0, length);
            return result;
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

        public uint value;

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
                ((int) MathUtils.lerpNullableFloat(a.alpha, b.alpha, t)).clamp(0, 255),
                ((int) MathUtils.lerpNullableFloat(a.red, b.red, t)).clamp(0, 255),
                ((int) MathUtils.lerpNullableFloat(a.green, b.green, t)).clamp(0, 255),
                ((int) MathUtils.lerpNullableFloat(a.blue, b.blue, t)).clamp(0, 255)
            );
        }

        public static Color alphaBlend(Color foreground, Color background) {
            int alpha = foreground.alpha;
            if (alpha == 0x00) {
                // Foreground completely transparent.
                return background;
            }

            int invAlpha = 0xff - alpha;
            int backAlpha = background.alpha;
            if (backAlpha == 0xff) {
                // Opaque background case
                return fromARGB(
                    0xff,
                    (alpha * foreground.red + invAlpha * background.red) / 0xff,
                    (alpha * foreground.green + invAlpha * background.green) / 0xff,
                    (alpha * foreground.blue + invAlpha * background.blue) / 0xff
                );
            }
            else {
                // General case
                backAlpha = (backAlpha * invAlpha) / 0xff;
                int outAlpha = alpha + backAlpha;
                D.assert(outAlpha != 0x00);
                return fromARGB(
                    outAlpha,
                    (foreground.red * alpha + background.red * backAlpha) / outAlpha,
                    (foreground.green * alpha + background.green * backAlpha) / outAlpha,
                    (foreground.blue * alpha + background.blue * backAlpha) / outAlpha);
            }
        }

        public static int getAlphaFromOpacity(float opacity) {
            return (opacity.clamp(0.0f, 1.0f) * 255).round();
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

    public enum PaintingStyle {
        fill,
        stroke,
    }

    public enum Clip {
        none,
        hardEdge,
        antiAlias,
        antiAliasWithSaveLayer,
    }

    public static partial class ui_ {
        internal const int _kDoNotResizeDimension = -1;
    }

    public class Paint {
        internal readonly byte[] _data = new byte[_kDataByteCount];

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
        internal IntPtr[] _objectPtrs;
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

        public float strokeWidth {
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
            get { return _objects?[_kShaderIndex] as Shader; }
            set {
                if (value == null) {
                    if (_objects != null) {
                        _objects[_kShaderIndex] = null;
                        _objectPtrs[_kShaderIndex] = IntPtr.Zero;
                    }
                }
                else {
                    _objects = _objects ?? new object[_kObjectCount];
                    _objectPtrs = _objectPtrs ?? new IntPtr[_kObjectCount];

                    _objects[_kShaderIndex] = value;
                    _objectPtrs[_kShaderIndex] = value._ptr;
                }
            }
        }

        public ColorFilter colorFilter {
            get { return ((_ColorFilter) _objects?[_kColorFilterIndex])?.creator; }

            set {
                _ColorFilter nativeFilter = value?._toNativeColorFilter();
                if (nativeFilter == null) {
                    if (_objects != null) {
                        _objects[_kColorFilterIndex] = null;
                        _objectPtrs[_kColorFilterIndex] = IntPtr.Zero;
                    }
                }
                else {
                    _objects = _objects ?? new object[_kObjectCount];
                    _objectPtrs = _objectPtrs ?? new IntPtr[_kObjectCount];

                    if (((_ColorFilter) _objects[_kColorFilterIndex])?.creator != value) {
                        _objects[_kColorFilterIndex] = nativeFilter;
                        _objectPtrs[_kColorFilterIndex] = nativeFilter._ptr;
                    }
                }
            }
        }

        public ImageFilter imageFilter {
            get { return ((_ImageFilter) _objects?[_kImageFilterIndex])?.creator; }
            set {
                _ImageFilter nativeFilter = value?._toNativeImageFilter();
                if (nativeFilter == null) {
                    if (_objects != null) {
                        _objects[_kImageFilterIndex] = null;
                        _objectPtrs[_kImageFilterIndex] = IntPtr.Zero;
                    }
                }
                else {
                    _objects = _objects ?? new object[_kObjectCount];
                    _objectPtrs = _objectPtrs ?? new IntPtr[_kObjectCount];

                    if (((_ImageFilter) _objects[_kImageFilterIndex])?.creator != value) {
                        _objects[_kImageFilterIndex] = nativeFilter;
                        _objectPtrs[_kImageFilterIndex] = nativeFilter._ptr;
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


    public enum ImageByteFormat {
        rawRgba,
        rawUnmodified,
        png,
    }

    public enum PixelFormat {
        rgba8888,
        bgra8888,
    }

    [StructLayout(LayoutKind.Sequential)]
    struct _ImageInfo {
        internal _ImageInfo(int width, int height, int format, int? rowBytes = null) {
            this.width = width;
            this.height = height;
            this.format = format;
            this.rowBytes = rowBytes ?? width * 4;
        }

        public int width;
        public int height;
        public int format;
        public int rowBytes;
    }

    public class Image : NativeWrapperDisposable, IEquatable<Image> {
        internal Image(IntPtr ptr) : base(ptr) {
        }

        public override void DisposePtr(IntPtr ptr) {
            Image_dispose(ptr);
        }

        public int width => Image_width(_ptr);

        public int height => Image_height(_ptr);

        public Future<byte[]> toByteData(
            ImageByteFormat format = ImageByteFormat.rawRgba
        ) {
            return ui_._futurize(
                (_Callback<byte[]> callback) => {
                    GCHandle callbackHandle = GCHandle.Alloc(callback);

                    IntPtr error = Image_toByteData(_ptr,
                        (int) format, _toByteDataCallback, (IntPtr) callbackHandle);
                    if (error != IntPtr.Zero) {
                        callbackHandle.Free();
                        return Marshal.PtrToStringAnsi(error);
                    }

                    return null;
                });
        }

        [MonoPInvokeCallback(typeof(Image_toByteDataCallback))]
        static void _toByteDataCallback(IntPtr callbackHandle, IntPtr data, int length) {
            GCHandle handle = (GCHandle) callbackHandle;
            var callback = (_Callback<byte[]>) handle.Target;
            handle.Free();

            if (!Isolate.checkExists()) {
                return;
            }

            try {
                if (data == IntPtr.Zero || length == 0) {
                    callback(new byte[0]);
                }
                else {
                    var bytes = new byte[length];
                    Marshal.Copy(data, bytes, 0, length);
                    callback(bytes);
                }
            }
            catch (Exception ex) {
                Debug.LogException(ex);
            }
        }
        
        public override string ToString() => $"[{width}\u00D7{height}]";

        [DllImport(NativeBindings.dllName)]
        static extern void Image_dispose(IntPtr ptr);

        [DllImport(NativeBindings.dllName)]
        static extern int Image_width(IntPtr ptr);

        [DllImport(NativeBindings.dllName)]
        static extern int Image_height(IntPtr ptr);

        delegate void Image_toByteDataCallback(IntPtr callbackHandle, IntPtr data, int length);

        [DllImport(NativeBindings.dllName)]
        static extern IntPtr Image_toByteData(IntPtr ptr, int format, Image_toByteDataCallback callback,
            IntPtr callbackHandle);

        public bool Equals(Image other) {
            return other != null && width == other.width && height == other.height && _ptr.Equals(other._ptr);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) {
                return false;
            }

            if (ReferenceEquals(this, obj)) {
                return true;
            }

            if (obj.GetType() != GetType()) {
                return false;
            }

            return Equals((Image) obj);
        }

        public static bool operator ==(Image left, Image right) {
            return Equals(left, right);
        }
        
        public static bool operator !=(Image left, Image right) {
            return !Equals(left, right);
        }

        public override int GetHashCode() {
            unchecked {
                var hashCode = (width.GetHashCode());
                hashCode = (hashCode * 397) ^(height.GetHashCode());
                hashCode = (hashCode * 397) ^ (_ptr.GetHashCode());
                return hashCode;
            }
        }
    }

    public delegate void ImageDecoderCallback(Image result);

    public class FrameInfo : NativeWrapper {
        internal FrameInfo(IntPtr ptr) : base(ptr) {
        }

        public override void DisposePtr(IntPtr ptr) {
            FrameInfo_dispose(ptr);
        }

        public TimeSpan duration => TimeSpan.FromMilliseconds(_durationMillis);
        int _durationMillis => FrameInfo_durationMillis(_ptr);

        public Image image => new Image(FrameInfo_image(_ptr));

        [DllImport(NativeBindings.dllName)]
        static extern void FrameInfo_dispose(IntPtr ptr);

        [DllImport(NativeBindings.dllName)]
        static extern int FrameInfo_durationMillis(IntPtr ptr);

        [DllImport(NativeBindings.dllName)]
        static extern IntPtr FrameInfo_image(IntPtr ptr);
    }

    public class Codec : NativeWrapperDisposable {
        internal Codec(IntPtr ptr) : base(ptr) {
        }

        public override void DisposePtr(IntPtr ptr) {
            Codec_dispose(ptr);
        }

        public int frameCount => Codec_frameCount(_ptr);

        public int repetitionCount => Codec_repetitionCount(_ptr);

        public Future<FrameInfo> getNextFrame() {
            return ui_._futurize<FrameInfo>(_getNextFrame);
        }

        string _getNextFrame(_Callback<FrameInfo> callback) {
            GCHandle callbackHandle = GCHandle.Alloc(callback);

            IntPtr error = Codec_getNextFrame(_ptr, _getNextFrameCallback, (IntPtr) callbackHandle);
            if (error != IntPtr.Zero) {
                callbackHandle.Free();
                return Marshal.PtrToStringAnsi(error);
            }

            return null;
        }

        [MonoPInvokeCallback(typeof(Codec_getNextFrameCallback))]
        static void _getNextFrameCallback(IntPtr callbackHandle, IntPtr ptr) {
            GCHandle handle = (GCHandle) callbackHandle;
            var callback = (_Callback<FrameInfo>) handle.Target;
            handle.Free();

            if (!Isolate.checkExists()) {
                return;
            }

            try {
                callback(ptr == IntPtr.Zero ? null : new FrameInfo(ptr));
            }
            catch (Exception ex) {
                Debug.LogException(ex);
            }
        }

        internal static unsafe string _instantiateImageCodec(byte[] list, _Callback<Codec> callback,
            _ImageInfo? imageInfo, int targetWidth, int targetHeight) {
            GCHandle callbackHandle = GCHandle.Alloc(callback);

            fixed (byte* bytes = list) {
                IntPtr error = Codec_instantiateImageCodec(bytes, list?.Length ?? 0,
                    _instantiateImageCodecCallback, (IntPtr) callbackHandle,
                    imageInfo ?? default, imageInfo.HasValue, targetWidth, targetHeight);
                if (error != IntPtr.Zero) {
                    callbackHandle.Free();
                    return Marshal.PtrToStringAnsi(error);
                }
            }

            return null;
        }


        [MonoPInvokeCallback(typeof(Codec_instantiateImageCodecCallback))]
        static void _instantiateImageCodecCallback(IntPtr callbackHandle, IntPtr ptr) {
            GCHandle handle = (GCHandle) callbackHandle;
            var callback = (_Callback<Codec>) handle.Target;
            handle.Free();

            try {
                D.assert(ptr != IntPtr.Zero);
                callback(new Codec(ptr));
            }
            catch (Exception ex) {
                Debug.LogException(ex);
            }
        }


        [DllImport(NativeBindings.dllName)]
        static extern void Codec_dispose(IntPtr ptr);

        [DllImport(NativeBindings.dllName)]
        static extern int Codec_frameCount(IntPtr ptr);

        [DllImport(NativeBindings.dllName)]
        static extern int Codec_repetitionCount(IntPtr ptr);

        delegate void Codec_getNextFrameCallback(IntPtr callbackHandle, IntPtr ptr);

        [DllImport(NativeBindings.dllName)]
        static extern IntPtr Codec_getNextFrame(IntPtr ptr, Codec_getNextFrameCallback callback, IntPtr callbackHandle);

        delegate void Codec_instantiateImageCodecCallback(IntPtr callbackHandle, IntPtr ptr);

        [DllImport(NativeBindings.dllName)]
        static extern unsafe IntPtr Codec_instantiateImageCodec(byte* list, int listLength,
            Codec_instantiateImageCodecCallback callback,
            IntPtr callbackHandle, _ImageInfo imageInfo, bool hasImageInfo, int targetWidth, int targetHeight);
    }

    public static partial class ui_ {
        public static Future<Codec> instantiateImageCodec(byte[] list, int? targetWidth = null,
            int? targetHeight = null) {
            return _futurize(
                (_Callback<Codec> callback) => Codec._instantiateImageCodec(list, callback, null,
                    targetWidth ?? _kDoNotResizeDimension, targetHeight ?? _kDoNotResizeDimension)
            );
        }

        public static void decodeImageFromList(byte[] list, ImageDecoderCallback callback) {
            _decodeImageFromListAsync(list, callback);
        }

        static Future _decodeImageFromListAsync(byte[] list, ImageDecoderCallback callback) {
            return instantiateImageCodec(list).then_(codec => {
                return codec.getNextFrame().then_(frameInfo => { callback(frameInfo.image); });
            });
        }

        public static void decodeImageFromPixels(
            byte[] pixels,
            int width,
            int height,
            PixelFormat format,
            ImageDecoderCallback callback,
            int? rowBytes = null, int? targetWidth = null, int? targetHeight = null
        ) {
            _ImageInfo imageInfo = new _ImageInfo(width, height, (int) format, rowBytes);
            Future<Codec> codecFuture = _futurize(
                (_Callback<Codec> callback1) => Codec._instantiateImageCodec(pixels, callback1, imageInfo,
                    targetWidth ?? _kDoNotResizeDimension, targetHeight ?? _kDoNotResizeDimension)
            );
            codecFuture.then_<FrameInfo>(codec => codec.getNextFrame())
                .then_(frameInfo => callback(frameInfo.image));
        }
    }

    public enum PathFillType {
        nonZero,
        evenOdd,
    }

    public enum PathOperation {
        difference,
        intersect,
        union,
        xor,
        reverseDifference,
    }

    public abstract class EngineLayer : NativeWrapper {
        protected EngineLayer(IntPtr ptr) : base(ptr) {
        }

        public override void DisposePtr(IntPtr ptr) {
            EngineLayer_dispose(ptr);
        }

        [DllImport(NativeBindings.dllName)]
        static extern void EngineLayer_dispose(IntPtr ptr);
    }

    public class Path : NativeWrapper {
        public Path() : base(Path_constructor()) {
        }

        public Path(IntPtr ptr) : base(ptr) {
        }

        public override void DisposePtr(IntPtr ptr) {
            Path_dispose(ptr);
        }

        public static Path from(Path source) {
            return new Path(Path_clone(source._ptr));
        }

        public PathFillType fillType {
            get { return (PathFillType) Path_getFillType(_ptr); }
            set { Path_setFillType(_ptr, (int) value); }
        }

        public void moveTo(float x, float y) {
            Path_moveTo(_ptr, x, y);
        }

        public void relativeMoveTo(float dx, float dy) {
            Path_relativeMoveTo(_ptr, dx, dy);
        }

        public void lineTo(float x, float y) {
            Path_lineTo(_ptr, x, y);
        }

        public void relativeLineTo(float dx, float dy) {
            Path_relativeLineTo(_ptr, dx, dy);
        }

        public void quadraticBezierTo(float x1, float y1, float x2, float y2) {
            Path_quadraticBezierTo(_ptr, x1, y1, x2, y2);
        }

        public void relativeQuadraticBezierTo(float x1, float y1, float x2, float y2) {
            Path_relativeQuadraticBezierTo(_ptr, x1, y1, x2, y2);
        }

        public void cubicTo(float x1, float y1, float x2, float y2, float x3, float y3) {
            Path_cubicTo(_ptr, x1, y1, x2, y2, x3, y3);
        }

        public void relativeCubicTo(float x1, float y1, float x2, float y2, float x3, float y3) {
            Path_relativeCubicTo(_ptr, x1, y1, x2, y2, x3, y3);
        }

        public void conicTo(float x1, float y1, float x2, float y2, float w) {
            Path_conicTo(_ptr, x1, y1, x2, y2, w);
        }

        public void relativeConicTo(float x1, float y1, float x2, float y2, float w) {
            Path_relativeConicTo(_ptr, x1, y1, x2, y2, w);
        }

        public void arcTo(Rect rect, float startAngle, float sweepAngle, bool forceMoveTo) {
            D.assert(PaintingUtils._rectIsValid(rect));
            Path_arcTo(_ptr, rect.left, rect.top, rect.right, rect.bottom, startAngle, sweepAngle,
                forceMoveTo);
        }

        public void arcToPoint(Offset arcEnd,
            Radius radius = null,
            float rotation = 0.0f,
            bool largeArc = false,
            bool clockwise = true) {
            radius = radius ?? Radius.zero;

            D.assert(PaintingUtils._offsetIsValid(arcEnd));
            D.assert(PaintingUtils._radiusIsValid(radius));

            Path_arcToPoint(_ptr, arcEnd.dx, arcEnd.dy, radius.x, radius.y, rotation,
                largeArc, clockwise);
        }

        public void relativeArcToPoint(Offset arcEndDelta,
            Radius radius = null,
            float rotation = 0.0f,
            bool largeArc = false,
            bool clockwise = true) {
            radius = radius ?? Radius.zero;

            D.assert(PaintingUtils._offsetIsValid(arcEndDelta));
            D.assert(PaintingUtils._radiusIsValid(radius));

            Path_relativeArcToPoint(_ptr, arcEndDelta.dx, arcEndDelta.dy, radius.x, radius.y, rotation,
                largeArc, clockwise);
        }

        public void addRect(Rect rect) {
            D.assert(PaintingUtils._rectIsValid(rect));
            Path_addRect(_ptr, rect.left, rect.top, rect.right, rect.bottom);
        }

        public void addOval(Rect oval) {
            D.assert(PaintingUtils._rectIsValid(oval));
            Path_addOval(_ptr, oval.left, oval.top, oval.right, oval.bottom);
        }

        public void addArc(Rect oval, float startAngle, float sweepAngle) {
            D.assert(PaintingUtils._rectIsValid(oval));
            Path_addArc(_ptr, oval.left, oval.top, oval.right, oval.bottom, startAngle, sweepAngle);
        }

        public unsafe void addPolygon(List<Offset> points, bool close) {
            D.assert(points != null);
            float[] list = PaintingUtils._encodePointList(points);
            fixed (float* listPtr = list) {
                Path_addPolygon(_ptr, listPtr, list.Length, close);
            }
        }

        public unsafe void addRRect(RRect rrect) {
            D.assert(PaintingUtils._rrectIsValid(rrect));
            float[] value32 = rrect._value32;
            fixed (float* value32Ptr = value32) {
                Path_addRRect(_ptr, value32Ptr);
            }
        }

        public unsafe void addPath(Path path, Offset offset, float[] matrix4 = null) {
            D.assert(path != null); // path is checked on the engine side
            D.assert(PaintingUtils._offsetIsValid(offset));

            if (matrix4 != null) {
                D.assert(PaintingUtils._matrix4IsValid(matrix4));
                fixed (float* matrix4Ptr = matrix4) {
                    Path_addPathWithMatrix(_ptr, path._ptr, offset.dx, offset.dy, matrix4Ptr);
                }
            }
            else {
                Path_addPath(_ptr, path._ptr, offset.dx, offset.dy);
            }
        }

        public unsafe void extendWithPath(Path path, Offset offset, float[] matrix4 = null) {
            D.assert(path != null);
            D.assert(PaintingUtils._offsetIsValid(offset));

            if (matrix4 != null) {
                D.assert(PaintingUtils._matrix4IsValid(matrix4));
                fixed (float* matrix4Ptr = matrix4) {
                    Path_extendWithPathAndMatrix(_ptr, path._ptr, offset.dx, offset.dy, matrix4Ptr);
                }
            }
            else {
                Path_extendWithPath(_ptr, path._ptr, offset.dx, offset.dy);
            }
        }

        public void close() {
            Path_close(_ptr);
        }

        public void reset() {
            Path_reset(_ptr);
        }

        public bool contains(Offset point) {
            D.assert(PaintingUtils._offsetIsValid(point));
            return Path_contains(_ptr, point.dx, point.dy);
        }

        public Path shift(Offset offset) {
            D.assert(PaintingUtils._offsetIsValid(offset));
            return new Path(Path_shift(_ptr, offset.dx, offset.dy));
        }

        public unsafe Path transform(float[] matrix4) {
            D.assert(PaintingUtils._matrix4IsValid(matrix4));
            fixed (float* matrixPtr = matrix4) {
                return new Path(Path_transform(_ptr, matrixPtr));
            }
        }

        public unsafe Rect getBounds() {
            float[] rect = new float[4];
            fixed (float* rectPtr = rect) {
                Path_getBounds(_ptr, rectPtr);
            }

            return Rect.fromLTRB(rect[0], rect[1], rect[2], rect[3]);
        }

        public static Path combine(PathOperation operation, Path path1, Path path2) {
            D.assert(path1 != null);
            D.assert(path2 != null);
            Path path = new Path();
            if (Path_op(path._ptr, path1._ptr, path2._ptr, (int) operation)) {
                return path;
            }

            throw new Exception(
                "Path.combine() failed.  This may be due an invalid path; in particular, check for NaN values.");
        }

        public PathMetrics computeMetrics(bool forceClose = false) {
            return new PathMetrics(this, forceClose);
        }


        [DllImport(NativeBindings.dllName)]
        static extern IntPtr Path_constructor();

        [DllImport(NativeBindings.dllName)]
        static extern void Path_dispose(IntPtr ptr);

        [DllImport(NativeBindings.dllName)]
        static extern IntPtr Path_clone(IntPtr ptr);

        [DllImport(NativeBindings.dllName)]
        static extern int Path_getFillType(IntPtr ptr);

        [DllImport(NativeBindings.dllName)]
        static extern void Path_setFillType(IntPtr ptr, int fillType);

        [DllImport(NativeBindings.dllName)]
        static extern void Path_moveTo(IntPtr ptr, float x, float y);

        [DllImport(NativeBindings.dllName)]
        static extern void Path_relativeMoveTo(IntPtr ptr, float dx, float dy);

        [DllImport(NativeBindings.dllName)]
        static extern void Path_lineTo(IntPtr ptr, float x, float y);

        [DllImport(NativeBindings.dllName)]
        static extern void Path_relativeLineTo(IntPtr ptr, float dx, float dy);

        [DllImport(NativeBindings.dllName)]
        static extern void Path_quadraticBezierTo(IntPtr ptr, float x1, float y1, float x2, float y2);

        [DllImport(NativeBindings.dllName)]
        static extern void Path_relativeQuadraticBezierTo(IntPtr ptr, float x1, float y1, float x2, float y2);

        [DllImport(NativeBindings.dllName)]
        static extern void Path_cubicTo(IntPtr ptr, float x1, float y1, float x2, float y2, float x3, float y3);

        [DllImport(NativeBindings.dllName)]
        static extern void Path_relativeCubicTo(IntPtr ptr, float x1, float y1, float x2, float y2, float x3,
            float y3);

        [DllImport(NativeBindings.dllName)]
        static extern void Path_conicTo(IntPtr ptr, float x1, float y1, float x2, float y2, float w);

        [DllImport(NativeBindings.dllName)]
        static extern void Path_relativeConicTo(IntPtr ptr, float x1, float y1, float x2, float y2, float w);

        [DllImport(NativeBindings.dllName)]
        static extern void Path_arcTo(IntPtr ptr, float left, float top, float right, float bottom,
            float startAngle, float sweepAngle, bool forceMoveTo);

        [DllImport(NativeBindings.dllName)]
        static extern void Path_arcToPoint(IntPtr ptr, float arcEndX, float arcEndY, float radiusX,
            float radiusY, float rotation, bool largeArc, bool clockwise);

        [DllImport(NativeBindings.dllName)]
        static extern void Path_relativeArcToPoint(IntPtr ptr, float arcEndX, float arcEndY, float radiusX,
            float radiusY, float rotation, bool largeArc, bool clockwise);

        [DllImport(NativeBindings.dllName)]
        static extern void Path_addRect(IntPtr ptr, float left, float top, float right, float bottom);

        [DllImport(NativeBindings.dllName)]
        static extern void Path_addOval(IntPtr ptr, float left, float top, float right, float bottom);

        [DllImport(NativeBindings.dllName)]
        static extern void Path_addArc(IntPtr ptr, float left, float top, float right, float bottom,
            float startAngle, float sweepAngle);

        [DllImport(NativeBindings.dllName)]
        static extern unsafe void Path_addPolygon(IntPtr ptr, float* points, int pointsLength, bool close);

        [DllImport(NativeBindings.dllName)]
        static extern unsafe void Path_addRRect(IntPtr ptr, float* value32);

        [DllImport(NativeBindings.dllName)]
        static extern unsafe void Path_addPathWithMatrix(IntPtr ptr, IntPtr path, float dx, float dy,
            float* matrix4);

        [DllImport(NativeBindings.dllName)]
        static extern unsafe void Path_addPath(IntPtr ptr, IntPtr path, float dx, float dy);

        [DllImport(NativeBindings.dllName)]
        static extern unsafe void Path_extendWithPathAndMatrix(IntPtr ptr, IntPtr path, float dx, float dy,
            float* matrix4);

        [DllImport(NativeBindings.dllName)]
        static extern unsafe void Path_extendWithPath(IntPtr ptr, IntPtr path, float dx, float dy);

        [DllImport(NativeBindings.dllName)]
        static extern void Path_close(IntPtr ptr);

        [DllImport(NativeBindings.dllName)]
        static extern void Path_reset(IntPtr ptr);

        [DllImport(NativeBindings.dllName)]
        [return: MarshalAs(UnmanagedType.U1)]
        static extern bool Path_contains(IntPtr ptr, float x, float y);

        [DllImport(NativeBindings.dllName)]
        static extern IntPtr Path_shift(IntPtr ptr, float dx, float dy);

        [DllImport(NativeBindings.dllName)]
        static extern unsafe IntPtr Path_transform(IntPtr ptr, float* matrix4);

        [DllImport(NativeBindings.dllName)]
        static extern unsafe void Path_getBounds(IntPtr ptr, float* rect);

        [DllImport(NativeBindings.dllName)]
        [return: MarshalAs(UnmanagedType.U1)]
        static extern bool Path_op(IntPtr ptr, IntPtr path1, IntPtr path2, int operation);
    }

    /*
     * TODO: path metric/measure
     * PathMetrics / PathMetricIterator / Tangent
     * public class PathMetric {}
     * class _PathMeasure {}
     */
    public enum BlurStyle {
        normal, // only normal for now.
        solid,
        outer,
        inner,
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

    public class PathMetrics : IEnumerable<PathMetric> {
        public PathMetrics(Path path, bool forceClosed) {
            _iterator = new PathMetricIterator(new _PathMeasure(path, forceClosed));
        }

        readonly IEnumerator<PathMetric> _iterator;

        public IEnumerator<PathMetric> GetEnumerator() {
            return _iterator;
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }
    }

    public class PathMetricIterator : IEnumerator<PathMetric> {
        public PathMetricIterator(_PathMeasure pathMeasure) {
            D.assert(pathMeasure != null);
            _pathMeasure = pathMeasure;
        }

        PathMetric _pathMetric;
        readonly _PathMeasure _pathMeasure;
        
        public bool MoveNext() {
            if (_pathMeasure._nextContour()) {
                _pathMetric = new PathMetric(_pathMeasure);
                return true;
            }

            _pathMetric = null;
            return false;
        }

        public void Reset() {
            D.assert(false, () => "PathMetricIterator.Reset is not implemented yet !");
        }

        public PathMetric Current {
            get {
                PathMetric currentMetric = _pathMetric;
                if (currentMetric == null) {
                    throw new Exception("PathMetricIterator is not pointing to a PathMetric. This can happen in two situations:\n" +
                    "- The iteration has not started yet. If so, call \"moveNext\" to start iteration." + 
                    "- The iterator ran out of elements. If so, check that \"moveNext\" returns true prior to calling \"current\".");
                }

                return currentMetric;
            }
        }

        object IEnumerator.Current {
            get { return Current; }
        }

        public void Dispose() {
            
        }
    }

    public class Tangent {
        public Tangent(Offset position, Offset vector) {
            D.assert(position != null);
            D.assert(vector != null);

            this.position = position;
            this.vector = vector;
        }

        public static Tangent fromAngle(Offset position, float angle) {
            return new Tangent(position, new Offset(Mathf.Cos(angle), Mathf.Sin(angle)));
        }

        public readonly Offset position;

        public readonly Offset vector;

        public float angle => -Mathf.Atan2(vector.dy, vector.dx);
    }

    public class PathMetric {
        public PathMetric(_PathMeasure measure) {
            D.assert(measure != null);
            _measure = measure;
            length = _measure.length(_measure.currentContourIndex);
            isClosed = _measure.isClosed(_measure.currentContourIndex);
            contourIndex = _measure.currentContourIndex;
        }

        public readonly float length;

        public readonly bool isClosed;

        public readonly int contourIndex;

        readonly _PathMeasure _measure;

        public Tangent getTangentForOffset(float distance) {
            return _measure.getTangentForOffset(contourIndex, distance);
        }

        public Path extractPath(float start, float end, bool startWithMoveTo = true) {
            return _measure.extractPath(contourIndex, start, end, startWithMoveTo);
        }

        public override string ToString() {
            return $"{GetType()}: length: {length}, isClosed: {isClosed}, contourIndex: {contourIndex}";
        }
    }


    public class _PathMeasure : NativeWrapper {
        public _PathMeasure(Path path, bool forceClosed) : base(PathMeasure_constructor(path._ptr, forceClosed)) {
        }

        public override void DisposePtr(IntPtr ptr) {
            PathMeasure_dispose(ptr);
        }

        public int currentContourIndex = -1;

        public float length(int contourIndex) {
            D.assert(contourIndex <= currentContourIndex, () => $"Iterator must be advanced before index {contourIndex} can be used.");
            return PathMeasure_length(_ptr, contourIndex);
        }

        public unsafe Tangent getTangentForOffset(int contourIndex, float distance) {
            D.assert(contourIndex <= currentContourIndex, () => $"Iterator must be advanced before index {contourIndex} can be used.");
            float[] posTan = new float[5];
            fixed (float* posTanPtr = posTan) {
                PathMeasure_getPosTan(_ptr, contourIndex, distance, posTanPtr);
            }

            if (posTan[0] == 0.0f) {
                return null;
            }
            else {
                return new Tangent(
                    new Offset(posTan[1], posTan[2]),
                    new Offset(posTan[3], posTan[4])
                    );
            }
        }

        public Path extractPath(int contourIndex, float start, float end, bool startWithMoveTo = true) {
            D.assert(contourIndex <= currentContourIndex,
                () => $"Iterator must be advanced before index {contourIndex} can be used.");
            IntPtr pathPtr = PathMeasure_getSegment(_ptr, contourIndex, start, end, startWithMoveTo);
            return new Path(pathPtr);
        }

        public bool isClosed(int contourIndex) {
            D.assert(contourIndex <= currentContourIndex, () => $"Iterator must be advanced before index {contourIndex} can be used.");
            return PathMeasure_isClosed(_ptr, contourIndex);
        }

        public bool _nextContour() {
            bool next = PathMeasure_nativeNextContour(_ptr);
            if (next) {
                currentContourIndex++;
            }
            return next;
        }

        
        [DllImport(NativeBindings.dllName)]
        static extern IntPtr PathMeasure_constructor(IntPtr path, bool forcedClosed);

        [DllImport(NativeBindings.dllName)]
        static extern void PathMeasure_dispose(IntPtr ptr);

        [DllImport(NativeBindings.dllName)]
        static extern float PathMeasure_length(IntPtr ptr, int contourIndex);

        [DllImport(NativeBindings.dllName)]
        static extern unsafe void PathMeasure_getPosTan(IntPtr ptr, int contourIndex, float distance, float* posTan);

        [DllImport(NativeBindings.dllName)]
        static extern IntPtr PathMeasure_getSegment(IntPtr ptr, int contourIndex, float start, float end,
            bool startWithMoveTo);

        
        [DllImport(NativeBindings.dllName)]
        [return: MarshalAs(UnmanagedType.U1)]
        static extern bool PathMeasure_isClosed(IntPtr ptr, int contourIndex);
        
        [DllImport(NativeBindings.dllName)]
        [return: MarshalAs(UnmanagedType.U1)]
        static extern bool PathMeasure_nativeNextContour(IntPtr ptr);
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

    class _ColorFilter : NativeWrapper {
        _ColorFilter(IntPtr ptr, ColorFilter creator) : base(ptr) {
            this.creator = creator;
        }

        public override void DisposePtr(IntPtr ptr) {
            ColorFilter_dispose(ptr);
        }

        public static _ColorFilter mode(ColorFilter creator) {
            D.assert(creator != null);
            D.assert(creator._type == ColorFilter._TypeMode);

            var filter = new _ColorFilter(ColorFilter_constructor(), creator);
            ColorFilter_initMode(filter._ptr, creator._color.value, (int) creator._blendMode);

            return filter;
        }

        public static unsafe _ColorFilter matrix(ColorFilter creator) {
            D.assert(creator != null);
            D.assert(creator._type == ColorFilter._TypeMatrix);

            var filter = new _ColorFilter(ColorFilter_constructor(), creator);
            fixed (float* matrix = creator._matrix) {
                ColorFilter_initMatrix(filter._ptr, matrix);
            }

            return filter;
        }

        public static _ColorFilter linearToSrgbGamma(ColorFilter creator) {
            D.assert(creator != null);
            D.assert(creator._type == ColorFilter._TypeLinearToSrgbGamma);

            var filter = new _ColorFilter(ColorFilter_constructor(), creator);
            ColorFilter_initLinearToSrgbGamma(filter._ptr);

            return filter;
        }

        public static _ColorFilter srgbToLinearGamma(ColorFilter creator) {
            D.assert(creator != null);
            D.assert(creator._type == ColorFilter._TypeSrgbToLinearGamma);

            var filter = new _ColorFilter(ColorFilter_constructor(), creator);
            ColorFilter_initSrgbToLinearGamma(filter._ptr);

            return filter;
        }

        public readonly ColorFilter creator;

        [DllImport(NativeBindings.dllName)]
        static extern IntPtr ColorFilter_constructor();

        [DllImport(NativeBindings.dllName)]
        static extern void ColorFilter_dispose(IntPtr ptr);

        [DllImport(NativeBindings.dllName)]
        static extern void ColorFilter_initMode(IntPtr ptr, uint color, int blendMode);

        [DllImport(NativeBindings.dllName)]
        static extern unsafe void ColorFilter_initMatrix(IntPtr ptr, float* matrix4);

        [DllImport(NativeBindings.dllName)]
        static extern unsafe void ColorFilter_initLinearToSrgbGamma(IntPtr ptr);

        [DllImport(NativeBindings.dllName)]
        static extern unsafe void ColorFilter_initSrgbToLinearGamma(IntPtr ptr);
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

        internal _ImageFilter _toNativeImageFilter() => _nativeFilter = _nativeFilter ?? _makeNativeImageFilter();

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

    internal class _ImageFilter : NativeWrapper {
        _ImageFilter(IntPtr ptr, ImageFilter creator) : base(ptr) {
            this.creator = creator;
        }

        public override void DisposePtr(IntPtr ptr) {
            ImageFilter_dispose(ptr);
        }

        public static _ImageFilter blur(ImageFilter creator) {
            D.assert(creator != null);
            D.assert(creator._type == ImageFilter._kTypeBlur);
            /*if (creator._data.Length != 16) {
                throw new ArgumentException("\"matrix4\" must have 16 entries.");
            }*/

            var filter = new _ImageFilter(ImageFilter_constructor(), creator);
            ImageFilter_initBlur(filter._ptr, creator._data[0], creator._data[1]);

            return filter;
        }

        public static unsafe _ImageFilter matrix(ImageFilter creator) {
            D.assert(creator != null);
            D.assert(creator._type == ImageFilter._kTypeMatrix);
            if (creator._data.Length != 16) {
                throw new ArgumentException("\"matrix4\" must have 16 entries.");
            }

            var filter = new _ImageFilter(ImageFilter_constructor(), creator);
            fixed (float* data = creator._data) {
                ImageFilter_initMatrix(filter._ptr, data, (int) creator._filterQuality);
            }

            return filter;
        }

        public readonly ImageFilter creator;

        [DllImport(NativeBindings.dllName)]
        static extern IntPtr ImageFilter_constructor();

        [DllImport(NativeBindings.dllName)]
        static extern void ImageFilter_dispose(IntPtr ptr);

        [DllImport(NativeBindings.dllName)]
        static extern void ImageFilter_initBlur(IntPtr ptr, float sigmaX, float sigmaY);

        [DllImport(NativeBindings.dllName)]
        static extern unsafe void ImageFilter_initMatrix(IntPtr ptr, float* matrix4, int filterQuality);
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

        public override void DisposePtr(IntPtr ptr) {
            Gradient_dispose(ptr);
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

            fixed (float* endPointsBuffer = endPointsArray)
            fixed (uint* colorsBuffer = colorsArray)
            fixed (float* colorStopsBuffer = colorStopsArray)
            fixed (float* matrix4Buffer = matrix4) {
                var gradient = new Gradient(Gradient_constructor());
                Gradient_initLinear(
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

            fixed (uint* colorsBuffer = colorsArray)
            fixed (float* colorStopsBuffer = colorStopsArray)
            fixed (float* matrix4Buffer = matrix4) {
                var gradient = new Gradient(Gradient_constructor());

                if (focal == null || (focal == center && focalRadius == 0.0f)) {
                    Gradient_initRadial(gradient._ptr,
                        center.dx, center.dy, radius,
                        colorsBuffer, colorsArray.Length,
                        colorStopsBuffer, colorStopsArray.Length,
                        (int) tileMode, matrix4Buffer);
                }
                else {
                    D.assert(center != Offset.zero || focal != Offset.zero);
                    Gradient_initConical(gradient._ptr,
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

            fixed (uint* colorsBuffer = colorsArray)
            fixed (float* colorStopsBuffer = colorStopsArray)
            fixed (float* matrix4Buffer = matrix4) {
                var gradient = new Gradient(Gradient_constructor());
                Gradient_initSweep(gradient._ptr,
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

        [DllImport(NativeBindings.dllName)]
        static extern IntPtr Gradient_constructor();

        [DllImport(NativeBindings.dllName)]
        static extern void Gradient_dispose(IntPtr ptr);

        [DllImport(NativeBindings.dllName)]
        static extern unsafe void Gradient_initLinear(IntPtr ptr,
            float* endPoints, int endPointsLength,
            uint* colors, int colorsLength,
            float* colorStops, int colorStopsLength,
            int tileMode, float* matrix4);

        [DllImport(NativeBindings.dllName)]
        static extern unsafe void Gradient_initRadial(IntPtr ptr,
            float centerX, float centerY, float radius,
            uint* colors, int colorsLength,
            float* colorStops, int colorStopsLength,
            int tileMode, float* matrix4);

        [DllImport(NativeBindings.dllName)]
        static extern unsafe void Gradient_initConical(IntPtr ptr,
            float startX, float startY, float startRadius,
            float endX, float endY, float endRadius,
            uint* colors, int colorsLength,
            float* colorStops, int colorStopsLength,
            int tileMode, float* matrix4);

        [DllImport(NativeBindings.dllName)]
        static extern unsafe void Gradient_initSweep(IntPtr ptr,
            float centerX, float centerY,
            uint* colors, int colorsLength,
            float* colorStops, int colorStopsLength,
            int tileMode, float startAngle, float endAngle, float* matrix4);
    }

    public class ImageShader : Shader {
        unsafe ImageShader(Image image, TileMode tmx, TileMode tmy, float[] matrix4)
            : base(ImageShader_constructor()) {
            D.assert(image != null);
            D.assert(matrix4 != null);
            if (matrix4.Length != 16) {
                throw new ArgumentException("\"matrix4\" must have 16 entries.");
            }

            fixed (float* matrix4Buffer = matrix4) {
                ImageShader_initWithImage(_ptr, image._ptr, (int) tmx, (int) tmy, matrix4Buffer);
            }
        }

        public override void DisposePtr(IntPtr ptr) {
            ImageShader_dispose(ptr);
        }

        [DllImport(NativeBindings.dllName)]
        static extern IntPtr ImageShader_constructor();

        [DllImport(NativeBindings.dllName)]
        static extern void ImageShader_dispose(IntPtr ptr);

        [DllImport(NativeBindings.dllName)]
        static extern unsafe void ImageShader_initWithImage(IntPtr ptr,
            IntPtr image, int tmx, int tmy, float* matrix4);
    }


    public enum VertexMode {
        triangles,
        triangleStrip,
        triangleFan,
    }

    public class Vertices : NativeWrapper {
        Vertices() : base(Vertices_constructor()) {
        }

        public override void DisposePtr(IntPtr ptr) {
            Vertices_dispose(ptr);
        }

        public unsafe Vertices(
            VertexMode mode,
            List<Offset> positions,
            List<Offset> textureCoordinates = null,
            List<Color> colors = null,
            List<int> indices = null
        ) : base(Vertices_constructor()) {
            D.assert(positions != null);
            if (textureCoordinates != null && textureCoordinates.Count != positions.Count)
                throw new ArgumentException("\"positions\" and \"textureCoordinates\" lengths must match.");
            if (colors != null && colors.Count != positions.Count)
                throw new ArgumentException("\"positions\" and \"colors\" lengths must match.");
            if (indices != null && indices.Any((i) => i < 0 || i >= positions.Count))
                throw new ArgumentException("\"indices\" values must be valid indices in the positions list.");

            float[] encodedPositions = PaintingUtils._encodePointList(positions);
            float[] encodedTextureCoordinates = (textureCoordinates != null)
                ? PaintingUtils._encodePointList(textureCoordinates)
                : null;
            uint[] encodedColors = colors != null
                ? PaintingUtils._encodeColorList(colors)
                : null;
            ushort[] encodedIndices = indices != null
                ? LinqUtils<ushort, int>.SelectArray(indices, (i => (ushort) i))
                : null;

            fixed (float* encodedPositionsPtr = encodedPositions, encodedTextureCoordinatesPtr =
                encodedTextureCoordinates)
            fixed (uint* encodedColorsPtr = encodedColors)
            fixed (ushort* encodedIndicesPtr = encodedIndices)
                if (!Vertices_init(_ptr, (int) mode,
                    encodedPositionsPtr, encodedPositions.Length,
                    encodedTextureCoordinatesPtr, encodedTextureCoordinates.Length,
                    encodedColorsPtr, encodedColors.Length,
                    encodedIndicesPtr, encodedIndices.Length))
                    throw new ArgumentException("Invalid configuration for vertices.");
        }


        public static unsafe Vertices raw(
            VertexMode mode,
            float[] positions,
            float[] textureCoordinates = null,
            uint[] colors = null,
            ushort[] indices = null
        ) {
            D.assert(positions != null);

            if (textureCoordinates != null && textureCoordinates.Length != positions.Length)
                throw new ArgumentException("\"positions\" and \"textureCoordinates\" lengths must match.");
            if (colors != null && colors.Length != positions.Length)
                throw new ArgumentException("\"positions\" and \"colors\" lengths must match.");
            if (indices != null && indices.Any((i) => i < 0 || i >= positions.Length))
                throw new ArgumentException("\"indices\" values must be valid indices in the positions list.");

            var vertices = new Vertices();

            fixed (float* encodedPositionsPtr = positions, encodedTextureCoordinatesPtr = textureCoordinates)
            fixed (uint* encodedColorsPtr = colors)
            fixed (ushort* encodedIndicesPtr = indices)
                if (!Vertices_init(vertices._ptr, (int) mode,
                    encodedPositionsPtr, positions.Length,
                    encodedTextureCoordinatesPtr, textureCoordinates.Length,
                    encodedColorsPtr, colors.Length,
                    encodedIndicesPtr, indices.Length))
                    throw new ArgumentException("Invalid configuration for vertices.");

            return vertices;
        }


        [DllImport(NativeBindings.dllName)]
        public static extern IntPtr Vertices_constructor();

        [DllImport(NativeBindings.dllName)]
        public static extern void Vertices_dispose(IntPtr ptr);

        [DllImport(NativeBindings.dllName)]
        [return: MarshalAs(UnmanagedType.U1)]
        public static extern unsafe bool Vertices_init(IntPtr ptr,
            int mode,
            float* positions,
            int positionsLength,
            float* textureCoordinates,
            int textureCoordinatesLength,
            uint* colors,
            int colorsLength,
            ushort* indices,
            int indicesLength);
    }


    public enum PointMode {
        points,
        lines,
        polygon,
    }

    public enum ClipOp {
        difference,
        intersect,
    }

    public class Canvas : NativeWrapper {
        public Canvas(PictureRecorder recorder, Rect cullRect = null) {
            D.assert(recorder != null);
            if (recorder.isRecording) {
                throw new ArgumentException("\"recorder\" must not already be associated with another Canvas.");
            }

            cullRect = cullRect ?? Rect.largest;
            _setPtr(Canvas_constructor(recorder._ptr, cullRect.left, cullRect.top,
                cullRect.right,
                cullRect.bottom));
        }

        public override void DisposePtr(IntPtr ptr) {
            Canvas_dispose(ptr);
        }

        public virtual void save() {
            Canvas_save(_ptr);
        }

        public virtual unsafe void saveLayer(Rect bounds, Paint paint) {
            D.assert(paint != null);
            if (bounds == null) {
                fixed (IntPtr* objectsPtr = paint._objectPtrs)
                fixed (byte* dataPtr = paint._data)
                    Canvas_saveLayerWithoutBounds(_ptr, objectsPtr, dataPtr);
            }
            else {
                D.assert(PaintingUtils._rectIsValid(bounds));
                fixed (IntPtr* objectsPtr = paint._objectPtrs)
                fixed (byte* dataPtr = paint._data)
                    Canvas_saveLayer(_ptr, bounds.left, bounds.top, bounds.right, bounds.bottom,
                        objectsPtr, dataPtr);
            }
        }

        public virtual void restore() {
            Canvas_restore(_ptr);
        }

        public virtual int getSaveCount() {
            return Canvas_getSaveCount(_ptr);
        }

        public virtual void translate(float dx, float dy) {
            Canvas_translate(_ptr, dx, dy);
        }

        public virtual void scale(float sx, float? sy = null) {
            Canvas_scale(_ptr, sx, sy ?? sx);
        }

        public virtual void rotate(float radians) {
            Canvas_rotate(_ptr, radians);
        }

        public virtual void skew(float sx, float sy) {
            Canvas_skew(_ptr, sx, sy);
        }

        public virtual unsafe void transform(float[] matrix4) {
            D.assert(matrix4 != null);
            if (matrix4.Length != 16)
                throw new ArgumentException("\"matrix4\" must have 16 entries.");
            fixed (float* matrx4Ptr = matrix4)
                Canvas_transform(_ptr, matrx4Ptr);
        }

        public virtual void clipRect(Rect rect, ClipOp clipOp = ClipOp.intersect, bool doAntiAlias = true) {
            D.assert(PaintingUtils._rectIsValid(rect));
            Canvas_clipRect(_ptr, rect.left, rect.top, rect.right, rect.bottom, (int) clipOp, doAntiAlias);
        }

        public virtual unsafe void clipRRect(RRect rrect, bool doAntiAlias = true) {
            D.assert(PaintingUtils._rrectIsValid(rrect));
            fixed (float* rrectPtr = rrect._value32)
                Canvas_clipRRect(_ptr, rrectPtr, doAntiAlias);
        }

        public virtual void clipPath(Path path, bool doAntiAlias = true) {
            D.assert(path != null);
            Canvas_clipPath(_ptr, path._ptr, doAntiAlias);
        }

        public virtual void drawColor(Color color, BlendMode blendMode) {
            D.assert(color != null);

            Canvas_drawColor(_ptr, color.value, (int) blendMode);
        }

        public virtual unsafe void drawLine(Offset p1, Offset p2, Paint paint) {
            D.assert(PaintingUtils._offsetIsValid(p1));
            D.assert(PaintingUtils._offsetIsValid(p2));
            D.assert(paint != null);

            fixed (IntPtr* objectsPtr = paint._objectPtrs)
            fixed (byte* dataPtr = paint._data)
                Canvas_drawLine(_ptr, p1.dx, p1.dy, p2.dx, p2.dy, objectsPtr, dataPtr);
        }

        public virtual unsafe void drawPaint(Paint paint) {
            D.assert(paint != null);

            fixed (IntPtr* objectsPtr = paint._objectPtrs)
            fixed (byte* dataPtr = paint._data)
                Canvas_drawPaint(_ptr, objectsPtr, dataPtr);
        }

        public virtual unsafe void drawRect(Rect rect, Paint paint) {
            D.assert(PaintingUtils._rectIsValid(rect));
            D.assert(paint != null);
            fixed (IntPtr* objectsPtr = paint._objectPtrs)
            fixed (byte* dataPtr = paint._data)
                Canvas_drawRect(_ptr, rect.left, rect.top, rect.right, rect.bottom,
                    objectsPtr, dataPtr);
        }

        public virtual unsafe void drawRRect(RRect rrect, Paint paint) {
            D.assert(PaintingUtils._rrectIsValid(rrect));
            D.assert(paint != null);

            fixed (float* rrectPtr = rrect._value32)
            fixed (IntPtr* objectsPtr = paint._objectPtrs)
            fixed (byte* dataPtr = paint._data)
                Canvas_drawRRect(_ptr, rrectPtr, objectsPtr,
                    dataPtr);
        }

        public virtual unsafe void drawDRRect(RRect outer, RRect inner, Paint paint) {
            D.assert(PaintingUtils._rrectIsValid(outer));
            D.assert(PaintingUtils._rrectIsValid(inner));
            D.assert(paint != null);

            fixed (float* outerPtr = outer._value32)
            fixed (float* innerPtr = inner._value32)
            fixed (IntPtr* objectsPtr = paint._objectPtrs)
            fixed (byte* dataPtr = paint._data)
                Canvas_drawDRRect(_ptr, outerPtr, innerPtr, objectsPtr,
                    dataPtr);
        }

        public virtual unsafe void drawOval(Rect rect, Paint paint) {
            D.assert(PaintingUtils._rectIsValid(rect));
            D.assert(paint != null);

            fixed (IntPtr* objectsPtr = paint._objectPtrs)
            fixed (byte* dataPtr = paint._data)
                Canvas_drawOval(_ptr, rect.left, rect.top, rect.right, rect.bottom,
                    objectsPtr,
                    dataPtr);
        }

        public virtual unsafe void drawCircle(Offset c, float radius, Paint paint) {
            D.assert(PaintingUtils._offsetIsValid(c));
            D.assert(paint != null);

            fixed (IntPtr* objectsPtr = paint._objectPtrs)
            fixed (byte* dataPtr = paint._data)
                Canvas_drawCircle(_ptr, c.dx, c.dy, radius, objectsPtr,
                    dataPtr);
        }

        public virtual unsafe void drawArc(Rect rect, float startAngle, float sweepAngle, bool useCenter, Paint paint) {
            D.assert(PaintingUtils._rectIsValid(rect));
            D.assert(paint != null);

            fixed (IntPtr* objectsPtr = paint._objectPtrs)
            fixed (byte* dataPtr = paint._data)
                Canvas_drawArc(_ptr, rect.left, rect.top, rect.right, rect.bottom, startAngle,
                    sweepAngle, useCenter, objectsPtr,
                    dataPtr);
        }

        public virtual unsafe void drawPath(Path path, Paint paint) {
            D.assert(path != null);
            D.assert(paint != null);

            fixed (IntPtr* objectsPtr = paint._objectPtrs)
            fixed (byte* dataPtr = paint._data)
                Canvas_drawPath(_ptr, path._ptr, objectsPtr,
                    dataPtr);
        }

        public virtual unsafe void drawImage(Image image, Offset p, Paint paint) {
            D.assert(image != null);
            D.assert(PaintingUtils._offsetIsValid(p));
            D.assert(paint != null);

            fixed (IntPtr* objectsPtr = paint._objectPtrs)
            fixed (byte* dataPtr = paint._data)
                Canvas_drawImage(_ptr, image._ptr, p.dx, p.dy, objectsPtr,
                    dataPtr);
        }

        public virtual unsafe void drawImageRect(Image image, Rect src, Rect dst, Paint paint) {
            D.assert(image != null);
            D.assert(PaintingUtils._rectIsValid(src));
            D.assert(PaintingUtils._rectIsValid(dst));
            D.assert(paint != null);

            fixed (IntPtr* objectsPtr = paint._objectPtrs)
            fixed (byte* dataPtr = paint._data)
                Canvas_drawImageRect(_ptr, image._ptr,
                    src.left,
                    src.top,
                    src.right,
                    src.bottom,
                    dst.left,
                    dst.top,
                    dst.right,
                    dst.bottom,
                    objectsPtr,
                    dataPtr);
        }

        public virtual unsafe void drawImageNine(Image image, Rect center, Rect dst, Paint paint) {
            D.assert(image != null);
            D.assert(PaintingUtils._rectIsValid(center));
            D.assert(PaintingUtils._rectIsValid(dst));
            D.assert(paint != null);

            fixed (IntPtr* objectsPtr = paint._objectPtrs)
            fixed (byte* dataPtr = paint._data)
                Canvas_drawImageNine(_ptr, image._ptr,
                    center.left,
                    center.top,
                    center.right,
                    center.bottom,
                    dst.left,
                    dst.top,
                    dst.right,
                    dst.bottom,
                    objectsPtr,
                    dataPtr);
        }

        public virtual void drawPicture(Picture picture) {
            D.assert(picture != null);
            Canvas_drawPicture(_ptr, picture._ptr);
        }


        public virtual void drawParagraph(Paragraph paragraph, Offset offset) {
            D.assert(paragraph != null);
            D.assert(PaintingUtils._offsetIsValid(offset));
            paragraph._paint(this, offset.dx, offset.dy);
        }
        public virtual void drawPoints(PointMode pointMode, List<Offset> points, Paint paint) {
            unsafe
            {
                D.assert(points != null);
                D.assert(paint != null);
                float[] list = PaintingUtils._encodePointList(points);
                fixed (IntPtr* objectPtrs = paint._objectPtrs)
                fixed (byte* dataPtr = paint._data)
                fixed (float* listPtr = list) {
                    Canvas_drawPoints(_ptr, objectPtrs, dataPtr, (int) pointMode, listPtr, list.Length);
                }
            }
        }
        public virtual unsafe void drawRawPoints(PointMode pointMode, float[] points, Paint paint) {
            D.assert(points != null);
            D.assert(paint != null);
            if (points.Length % 2 != 0)
                throw new ArgumentException("\"points\" must have an even number of values.");
            fixed (IntPtr* objectPtrs = paint._objectPtrs)
            fixed (byte* dataPtr = paint._data)
            fixed (float* pointsPtr = points) {
                Canvas_drawPoints(_ptr, objectPtrs, dataPtr, (int) pointMode, pointsPtr, points.Length);
            }
        }

        public virtual unsafe void drawVertices(Vertices vertices, BlendMode blendMode, Paint paint) {
            D.assert(vertices != null);
            D.assert(paint != null);

            fixed (IntPtr* objectPtrs = paint._objectPtrs)
            fixed (byte* dataPtr = paint._data) {
                Canvas_drawVertices(_ptr, vertices._ptr, (int) blendMode,
                    objectPtrs, dataPtr);
            }
        }

        public virtual unsafe void drawAtlas(Image atlas,
            List<RSTransform> transforms,
            List<Rect> rects,
            List<Color> colors,
            BlendMode blendMode,
            Rect cullRect,
            Paint paint) {
            D.assert(atlas != null);
            D.assert(transforms != null);
            D.assert(rects != null);
            D.assert(colors != null);
            D.assert(paint != null);

            int rectCount = rects.Count;
            if (transforms.Count != rectCount)
                throw new ArgumentException("\"transforms\" and \"rects\" lengths must match.");
            if (colors.isNotEmpty() && colors.Count != rectCount)
                throw new ArgumentException(
                    "If non-null, \"colors\" length must match that of \"transforms\" and \"rects\".");

            var rstTransformBuffer = new float[rectCount * 4];
            var rectBuffer = new float [rectCount * 4];

            for (int i = 0; i < rectCount; ++i) {
                int index0 = i * 4;
                int index1 = index0 + 1;
                int index2 = index0 + 2;
                int index3 = index0 + 3;
                RSTransform rstTransform = transforms[i];
                Rect rect = rects[i];
                D.assert(PaintingUtils._rectIsValid(rect));
                rstTransformBuffer[index0] = rstTransform.scos;
                rstTransformBuffer[index1] = rstTransform.ssin;
                rstTransformBuffer[index2] = rstTransform.tx;
                rstTransformBuffer[index3] = rstTransform.ty;
                rectBuffer[index0] = rect.left;
                rectBuffer[index1] = rect.top;
                rectBuffer[index2] = rect.right;
                rectBuffer[index3] = rect.bottom;
            }

            uint[] colorBuffer = colors.isEmpty() ? null : PaintingUtils._encodeColorList(colors);

            fixed (IntPtr* objectPtrs = paint._objectPtrs)
            fixed (byte* paintDataPtr = paint._data)
            fixed (float* rstTransformsPtr = rstTransformBuffer,
                rectsPtr = rectBuffer,
                cullRectPtr = cullRect?._value32)
            fixed (uint* colorsPtr = colorBuffer) {
                Canvas_drawAtlas(_ptr,
                    objectPtrs, paintDataPtr, atlas._ptr,
                    rstTransformsPtr, transforms.Count,
                    rectsPtr, rects.Count,
                    colorsPtr, colors.Count,
                    (int) blendMode, cullRectPtr
                );
            }
        }

        public virtual unsafe void drawRawAtlas(Image atlas,
            float[] rstTransforms,
            float[] rects,
            uint[] colors,
            BlendMode blendMode,
            Rect cullRect,
            Paint paint) {
            D.assert(atlas != null);
            D.assert(rstTransforms != null);
            D.assert(rects != null);
            D.assert(colors != null);
            D.assert(paint != null);

            int rectCount = rects.Length;
            if (rstTransforms.Length != rectCount)
                throw new ArgumentException("\"rstTransforms\" and \"rects\" lengths must match.");
            if (rectCount % 4 != 0)
                throw new ArgumentException("\"rstTransforms\" and \"rects\" lengths must be a multiple of four.");
            if (colors.Length * 4 != rectCount)
                throw new ArgumentException(
                    "If non-null, \"colors\" length must be one fourth the length of \"rstTransforms\" and \"rects\".");

            fixed (IntPtr* objectPtrs = paint._objectPtrs)
            fixed (byte* paintDataPtr = paint._data)
            fixed (float* rstTransformsPtr = rstTransforms,
                rectsPtr = rects,
                cullRectPtr = cullRect?._value32)
            fixed (uint* colorsPtr = colors) {
                Canvas_drawAtlas(_ptr,
                    objectPtrs, paintDataPtr, atlas._ptr,
                    rstTransformsPtr, rstTransforms.Length,
                    rectsPtr, rects.Length,
                    colorsPtr, colors.Length,
                    (int) blendMode, cullRectPtr
                );
            }
        }

        public virtual void drawShadow(Path path, Color color, float elevation, bool transparentOccluder) {
            D.assert(path != null);
            D.assert(color != null);
            Canvas_drawShadow(_ptr, path._ptr, color.value, elevation, transparentOccluder);
        }

        [DllImport(NativeBindings.dllName)]
        static extern IntPtr Canvas_constructor(IntPtr recorder,
            float left,
            float top,
            float right,
            float bottom);

        [DllImport(NativeBindings.dllName)]
        static extern void Canvas_dispose(IntPtr ptr);

        [DllImport(NativeBindings.dllName)]
        static extern void Canvas_save(IntPtr ptr);

        [DllImport(NativeBindings.dllName)]
        static extern unsafe void
            Canvas_saveLayerWithoutBounds(IntPtr ptr, IntPtr* paintObject, byte* paintData);

        [DllImport(NativeBindings.dllName)]
        static extern unsafe void Canvas_saveLayer(IntPtr ptr,
            float left, float top, float right, float bottom, IntPtr* paintObject, byte* paintData);

        [DllImport(NativeBindings.dllName)]
        static extern void Canvas_restore(IntPtr ptr);

        [DllImport(NativeBindings.dllName)]
        static extern int Canvas_getSaveCount(IntPtr ptr);

        [DllImport(NativeBindings.dllName)]
        static extern void Canvas_translate(IntPtr ptr,
            float x, float y);

        [DllImport(NativeBindings.dllName)]
        static extern void Canvas_scale(IntPtr ptr,
            float sx, float sy);

        [DllImport(NativeBindings.dllName)]
        static extern void Canvas_rotate(IntPtr ptr,
            float radians);

        [DllImport(NativeBindings.dllName)]
        static extern void Canvas_skew(IntPtr ptr,
            float sx, float sy);

        [DllImport(NativeBindings.dllName)]
        static extern unsafe void Canvas_transform(IntPtr ptr,
            float* matrix4);

        [DllImport(NativeBindings.dllName)]
        static extern void Canvas_clipRect(IntPtr ptr,
            float left, float top, float right, float bottom, int clipOp, bool doAntiAlias);

        [DllImport(NativeBindings.dllName)]
        static extern unsafe void Canvas_clipRRect(IntPtr ptr,
            float* rrect, bool doAntiAlias);

        [DllImport(NativeBindings.dllName)]
        static extern void Canvas_clipPath(IntPtr ptr,
            IntPtr path, bool doAntiAlias);

        [DllImport(NativeBindings.dllName)]
        static extern void Canvas_drawColor(IntPtr ptr,
            uint color, int blendMode);

        [DllImport(NativeBindings.dllName)]
        static extern unsafe void Canvas_drawLine(IntPtr ptr,
            float x1, float y1, float x2, float y2, IntPtr* paintObject, byte* paintData);

        [DllImport(NativeBindings.dllName)]
        static extern unsafe void Canvas_drawPaint(IntPtr ptr,
            IntPtr* paintObject, byte* paintData);

        [DllImport(NativeBindings.dllName)]
        static extern unsafe void Canvas_drawRect(IntPtr ptr,
            float left, float top, float right, float bottom, IntPtr* paintObject, byte* paintData);

        [DllImport(NativeBindings.dllName)]
        static extern unsafe void Canvas_drawRRect(IntPtr ptr,
            float* rrect, IntPtr* paintObject, byte* paintData);

        [DllImport(NativeBindings.dllName)]
        static extern unsafe void Canvas_drawDRRect(IntPtr ptr,
            float* outer, float* inner, IntPtr* paintObject, byte* paintData);

        [DllImport(NativeBindings.dllName)]
        static extern unsafe void Canvas_drawOval(IntPtr ptr,
            float left, float top, float right, float bottom, IntPtr* paintObject, byte* paintData);

        [DllImport(NativeBindings.dllName)]
        static extern unsafe void Canvas_drawCircle(IntPtr ptr,
            float x, float y, float radius, IntPtr* paintObject, byte* paintData);

        [DllImport(NativeBindings.dllName)]
        static extern unsafe void Canvas_drawArc(IntPtr ptr,
            float left, float top, float right, float bottom,
            float startAngle, float sweepAngle, bool useCenter, IntPtr* paintObject, byte* paintData);

        [DllImport(NativeBindings.dllName)]
        static extern unsafe void Canvas_drawPath(IntPtr ptr,
            IntPtr path, IntPtr* paintObject, byte* paintData);

        [DllImport(NativeBindings.dllName)]
        static extern unsafe void Canvas_drawImage(IntPtr ptr,
            IntPtr image, float x, float y, IntPtr* paintObject, byte* paintData);

        [DllImport(NativeBindings.dllName)]
        static extern unsafe void Canvas_drawImageRect(IntPtr ptr,
            IntPtr image, float srcLeft, float srcTop, float srcRight, float srcBottom,
            float dstLeft, float dstTop, float dstRight, float dstBottom, IntPtr* paintObject, byte* paintData);

        [DllImport(NativeBindings.dllName)]
        static extern unsafe void Canvas_drawImageNine(IntPtr ptr,
            IntPtr image, float centerLeft, float centerTop, float centerRight, float centerBottom,
            float dstLeft, float dstTop, float dstRight, float dstBottom, IntPtr* paintObject, byte* paintData);

        [DllImport(NativeBindings.dllName)]
        static extern void Canvas_drawPicture(IntPtr ptr, IntPtr picture);

        [DllImport(NativeBindings.dllName)]
        static extern unsafe void Canvas_drawPoints(IntPtr ptr,
            IntPtr* paintObject, byte* paintData, int pointMode, float* points, int pointsLength);

        [DllImport(NativeBindings.dllName)]
        static extern unsafe void Canvas_drawVertices(IntPtr ptr, IntPtr vertices, int blendMode,
            IntPtr* paintObject, byte* paintData);

        [DllImport(NativeBindings.dllName)]
        static extern unsafe void Canvas_drawAtlas(IntPtr ptr, IntPtr* paintObjects, byte* paintData,
            IntPtr atlas, float* rstTransforms, int rstTransformsLength,
            float* rects, int rectsLength, uint* colors, int colorsLength, int blendMode, float* cullRect);

        [DllImport(NativeBindings.dllName)]
        static extern void Canvas_drawShadow(IntPtr ptr, IntPtr path, uint color, float elevation,
            bool transparentOccluder);
    }

    public class Picture : NativeWrapperDisposable {
        internal Picture(IntPtr ptr) : base(ptr) {
        }

        public override void DisposePtr(IntPtr ptr) {
            Picture_dispose(ptr);
        }

        public Future<Image> toImage(int width, int height) {
            if (width <= 0 || height <= 0) {
                throw new ArgumentException("Invalid image dimensions.");
            }

            return ui_._futurize(
                (_Callback<Image> callback) => {
                    GCHandle callbackHandle = GCHandle.Alloc(callback);
                    IntPtr error =
                        Picture_toImage(_ptr, width, height, _toImageCallback,
                            (IntPtr) callbackHandle);

                    if (error != IntPtr.Zero) {
                        callbackHandle.Free();
                        return Marshal.PtrToStringAnsi(error);
                    }

                    return null;
                });
        }

        [MonoPInvokeCallback(typeof(Picture_toImageCallback))]
        static void _toImageCallback(IntPtr callbackHandle, IntPtr result) {
            GCHandle handle = (GCHandle) callbackHandle;
            var callback = (_Callback<Image>) handle.Target;
            handle.Free();

            if (!Isolate.checkExists()) {
                return;
            }

            try {
                callback(result == IntPtr.Zero ? null : new Image(result));
            }
            catch (Exception ex) {
                Debug.LogException(ex);
            }
        }

        public ulong approximateBytesUsed => Picture_GetAllocationSize(_ptr);

        [DllImport(NativeBindings.dllName)]
        static extern void Picture_dispose(IntPtr ptr);

        [DllImport(NativeBindings.dllName)]
        [return: MarshalAs(UnmanagedType.SysUInt)]
        static extern ulong Picture_GetAllocationSize(IntPtr ptr);

        delegate void Picture_toImageCallback(IntPtr callbackHandle, IntPtr result);

        [DllImport(NativeBindings.dllName)]
        static extern IntPtr Picture_toImage(IntPtr ptr, int width, int height, Picture_toImageCallback callback,
            IntPtr callbackHandle);
    }

    public class PictureRecorder : NativeWrapper {
        public PictureRecorder() : base(PictureRecorder_constructor()) {
        }

        public override void DisposePtr(IntPtr ptr) {
            PictureRecorder_dispose(ptr);
        }

        public bool isRecording => PictureRecorder_isRecording(_ptr);

        public Picture endRecording() {
            return new Picture(PictureRecorder_endRecording(_ptr));
        }

        [DllImport(NativeBindings.dllName)]
        static extern IntPtr PictureRecorder_constructor();

        [DllImport(NativeBindings.dllName)]
        static extern void PictureRecorder_dispose(IntPtr ptr);

        [DllImport(NativeBindings.dllName)]
        [return: MarshalAs(UnmanagedType.U1)]
        static extern bool PictureRecorder_isRecording(IntPtr ptr);

        [DllImport(NativeBindings.dllName)]
        static extern IntPtr PictureRecorder_endRecording(IntPtr ptr);
    }

    public class Shadow : IEquatable<Shadow> {
        public Shadow(
            Color color,
            Offset offset,
            float blurRadius = 0) {
            D.assert(color != null);
            D.assert(offset != null);
            D.assert(blurRadius >= 0.0);
            this.color = color ?? new Color(_kColorDefault);
            this.offset = offset ?? Offset.zero;
            this.blurRadius = blurRadius;
        }

        static readonly uint _kColorDefault = 0xFF000000;

        // Constants for shadow encoding.
        static readonly int _kBytesPerShadow = 16;
        static readonly int _kColorOffset = 0 << 2;
        static readonly int _kXOffset = 1 << 2;
        static readonly int _kYOffset = 2 << 2;
        static readonly int _kBlurOffset = 3 << 2;

        public readonly Color color;

        public readonly Offset offset;

        public readonly float blurRadius;

        public static float convertRadiusToSigma(float radius) {
            return radius * 0.57735f + 0.5f;
        }

        public float blurSigma {
            get { return convertRadiusToSigma(blurRadius); }
        }

        public Paint toPaint() {
            return new Paint() {
                color = color,
                maskFilter = MaskFilter.blur(BlurStyle.normal, blurSigma)
            };
        }

        public Shadow scale(float factor) {
            return new Shadow(
                color: color,
                offset: offset * factor,
                blurRadius: blurRadius * factor
            );
        }

        public static Shadow lerp(Shadow a, Shadow b, float t) {
            if (a == null && b == null)
                return null;
            if (a == null)
                return b.scale(t);
            if (b == null)
                return a.scale(1.0f - t);
            return new Shadow(
                color: Color.lerp(a.color, b.color, t),
                offset: Offset.lerp(a.offset, b.offset, t),
                blurRadius: MathUtils.lerpNullableFloat(a.blurRadius, b.blurRadius, t)
            );
        }

        public static List<Shadow> lerpList(List<Shadow> a, List<Shadow> b, float t) {
            if (a == null && b == null)
                return null;
            a = a ?? new List<Shadow>();
            b = b ?? new List<Shadow>();
            List<Shadow> result = new List<Shadow>();
            int commonLength = Math.Min(a.Count, b.Count);
            for (int i = 0; i < commonLength; i += 1)
                result.Add(lerp(a[i], b[i], t));
            for (int i = commonLength; i < a.Count; i += 1)
                result.Add(a[i].scale(1.0f - t));
            for (int i = commonLength; i < b.Count; i += 1)
                result.Add(b[i].scale(t));
            return result;
        }
        
        internal static byte[] _encodeShadows(List<Shadow> shadows) {
            if (shadows == null)
                return new byte[0];

            int byteCount = shadows.Count * _kBytesPerShadow;
            byte[] shadowsData = new byte[byteCount];

            int shadowOffset = 0;
            for (int shadowIndex = 0; shadowIndex < shadows.Count; ++shadowIndex) {
                Shadow shadow = shadows[shadowIndex];
                if (shadow == null)
                    continue;
                shadowOffset = shadowIndex * _kBytesPerShadow;

                shadowsData.setInt32(_kColorOffset + shadowOffset,
                    (int) (shadow.color.value ^ _kColorDefault));

                shadowsData.setFloat32(_kXOffset + shadowOffset,
                    shadow.offset.dx);

                shadowsData.setFloat32(_kYOffset + shadowOffset,
                    shadow.offset.dy);

                shadowsData.setFloat32(_kBlurOffset + shadowOffset,
                    shadow.blurRadius);
            }

            return shadowsData;
        }

        public override string ToString() => $"TextShadow({color}, {offset}, {blurRadius})";

        public bool Equals(Shadow other)
        {
            if (ReferenceEquals(null, other)) {
                return false;
            }

            if (ReferenceEquals(this, other)) {
                return true;
            }

            return Equals(color, other.color) && Equals(offset, other.offset) && blurRadius.Equals(other.blurRadius);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) {
                return false;
            }

            if (ReferenceEquals(this, obj)) {
                return true;
            }

            if (obj.GetType() != GetType()) {
                return false;
            }

            return Equals((Shadow) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (color != null ? color.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (offset != null ? offset.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ blurRadius.GetHashCode();
                return hashCode;
            }
        }
    }
    
    public class Skottie : NativeWrapper {
        public Skottie(string path) {
            var Id = Skottie_Construct(path);
            if(Id == IntPtr.Zero){
                Debug.Log($"cannot load lottie from {path}, please check file exist and valid");
            }else {
                _setPtr(Id);
            }
        }

        public override void DisposePtr(IntPtr ptr) {
            Skottie_Dispose(ptr);
        }

        public void paint(Canvas canvas, Offset offset, float width, float height, float frame) {
            Skottie_Paint(_ptr, canvas._ptr, offset.dx, offset.dy, width, height, frame);
        }
        
        public float duration() {
            return Skottie_Duration(_ptr);
        }

        [DllImport(NativeBindings.dllName)]
        static extern IntPtr Skottie_Construct(string path);

        [DllImport(NativeBindings.dllName)]
        static extern void Skottie_Dispose(IntPtr skottie);

        [DllImport(NativeBindings.dllName)]
        static extern void Skottie_Paint(IntPtr skottie, IntPtr canvas, float x, float y, float width, float height,
            float frame);
        
        [DllImport(NativeBindings.dllName)]
        static extern float Skottie_Duration(IntPtr skottie);
    }

    delegate void _Callback<T>(T result);

    delegate string _Callbacker<T>(_Callback<T> callback);

    public static partial class ui_ {
        internal static Future<T> _futurize<T>(_Callbacker<T> callbacker) {
            Completer completer = Completer.sync();
            string error = callbacker(t => {
                if (t == null) {
                    completer.completeError(new Exception("operation failed"));
                }
                else {
                    completer.complete(FutureOr.value(t));
                }
            });
            if (error != null)
                throw new Exception(error);
            return completer.future.to<T>();
        }
    }
}