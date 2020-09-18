using System;
using System.Linq;
using Unity.UIWidgets.foundation;
using UnityEngine;

namespace Unity.UIWidgets.ui {
    class PaintingUtils {
        internal static bool _offsetIsValid(Offset offset) {
            D.assert(offset != null, () => "Offset argument was null.");
            D.assert(!offset.dx.isNaN() && !offset.dy.isNaN(), () => "Offset argument contained a NaN value.");
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
        
        internal static bool _matrix4IsValid(float[] matrix4) {
            D.assert(matrix4 != null, () => "Matrix4 argument was null.");
            D.assert(matrix4.Length == 16, () => "Matrix4 must have 16 entries.");
            D.assert(matrix4.All((float value) => value.isFinite()), () => "Matrix4 entries must be finite.");
            return true;
        }
    }

    public class Color : IEquatable<Color> {
        public Color(long value) {
            this.value = value & 0xFFFFFFFF;
        }

        public static readonly Color clear = new Color(0x00000000);

        public static readonly Color black = new Color(0xFF000000);

        public static readonly Color white = new Color(0xFFFFFFFF);

        public static Color fromARGB(int a, int r, int g, int b) {
            return new Color(
                (((a & 0xff) << 24) |
                 ((r & 0xff) << 16) |
                 ((g & 0xff) << 8) |
                 ((b & 0xff) << 0)) & 0xFFFFFFFF);
        }

        public static Color fromRGBO(int r, int g, int b, float opacity) {
            return new Color(
                ((((int) (opacity * 0xff) & 0xff) << 24) |
                 ((r & 0xff) << 16) |
                 ((g & 0xff) << 8) |
                 ((b & 0xff) << 0)) & 0xFFFFFFFF);
        }

        public readonly long value;

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
            float R = _linearizeColorComponent(red / 0xFF);
            float G = _linearizeColorComponent(green / 0xFF);
            float B = _linearizeColorComponent(blue / 0xFF);
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
        fast_shadow
    }

    public class MaskFilter : IEquatable<MaskFilter> {
        MaskFilter(BlurStyle style, float sigma) {
            this.style = style;
            this.sigma = sigma;
        }

        public static MaskFilter blur(BlurStyle style, float sigma) {
            return new MaskFilter(style, sigma);
        }

        public static MaskFilter fastShadow(float sigma) {
            return new MaskFilter(BlurStyle.fast_shadow, sigma);
        }

        public readonly BlurStyle style;
        public readonly float sigma;

        public bool Equals(MaskFilter other) {
            if (ReferenceEquals(null, other)) {
                return false;
            }

            if (ReferenceEquals(this, other)) {
                return true;
            }

            return style == other.style && sigma.Equals(other.sigma);
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
                return ((int) style * 397) ^ sigma.GetHashCode();
            }
        }

        public static bool operator ==(MaskFilter left, MaskFilter right) {
            return Equals(left, right);
        }

        public static bool operator !=(MaskFilter left, MaskFilter right) {
            return !Equals(left, right);
        }

        public override string ToString() {
            return $"MaskFilter.blur(${style}, ${sigma:F1})";
        }
    }

    public class ColorFilter : IEquatable<ColorFilter> {
        ColorFilter(Color color, BlendMode blendMode) {
            D.assert(color != null);
            this.color = color;
            this.blendMode = blendMode;
        }

        public static ColorFilter mode(Color color, BlendMode blendMode) {
            return new ColorFilter(color, blendMode);
        }

        public readonly Color color;

        public readonly BlendMode blendMode;

        public bool Equals(ColorFilter other) {
            if (ReferenceEquals(null, other)) {
                return false;
            }

            if (ReferenceEquals(this, other)) {
                return true;
            }

            return Equals(color, other.color) && blendMode == other.blendMode;
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
                return ((color != null ? color.GetHashCode() : 0) * 397) ^ (int) blendMode;
            }
        }

        public static bool operator ==(ColorFilter left, ColorFilter right) {
            return Equals(left, right);
        }

        public static bool operator !=(ColorFilter left, ColorFilter right) {
            return !Equals(left, right);
        }

        public override string ToString() {
            return $"ColorFilter({color}, {blendMode})";
        }
    }

    public abstract class ImageFilter {
        public static ImageFilter blur(float sigmaX = 0.0f, float sigmaY = 0.0f) {
            return new _BlurImageFilter(sigmaX, sigmaY);
        }

        public static ImageFilter matrix(Matrix3 transform, FilterMode filterMode = FilterMode.Bilinear) {
            return new _MatrixImageFilter(transform, filterMode);
        }
    }

    class _BlurImageFilter : ImageFilter {
        public _BlurImageFilter(float sigmaX, float sigmaY) {
            this.sigmaX = sigmaX;
            this.sigmaY = sigmaY;
        }
        
        public readonly float sigmaX;
        public readonly float sigmaY;
    }

    class _MatrixImageFilter : ImageFilter {
        public _MatrixImageFilter(Matrix3 transform, FilterMode filterMode) {
            D.assert(transform != null);
            this.transform = transform;
            this.filterMode = filterMode;
        }

        public readonly Matrix3 transform;
        public readonly FilterMode filterMode;
    }

    public class Paint {
        static readonly Color _kColorDefault = new Color(0xFFFFFFFF);

        public Color color = _kColorDefault;

        public BlendMode blendMode = BlendMode.srcOver;

        public PaintingStyle style = PaintingStyle.fill;

        public float strokeWidth = 0;

        public StrokeCap strokeCap = StrokeCap.butt;

        public StrokeJoin strokeJoin = StrokeJoin.miter;

        public float strokeMiterLimit = 4.0f;

        public FilterMode filterMode = FilterMode.Bilinear;

        public ColorFilter colorFilter = null;

        public MaskFilter maskFilter = null;

        public ImageFilter backdrop = null;

        public PaintShader shader = null;

        public bool invertColors = false;

        public Paint() {
        }

        public Paint(Paint paint) {
            D.assert(paint != null);

            color = paint.color;
            blendMode = paint.blendMode;
            style = paint.style;
            strokeWidth = paint.strokeWidth;
            strokeCap = paint.strokeCap;
            strokeJoin = paint.strokeJoin;
            strokeMiterLimit = paint.strokeMiterLimit;
            filterMode = paint.filterMode;
            colorFilter = paint.colorFilter;
            maskFilter = paint.maskFilter;
            backdrop = paint.backdrop;
            shader = paint.shader;
            invertColors = paint.invertColors;
        }

        public static Paint shapeOnly(Paint paint) {
            return new Paint {
                style = paint.style,
                strokeWidth = paint.strokeWidth,
                strokeCap = paint.strokeCap,
                strokeJoin = paint.strokeJoin,
                strokeMiterLimit = paint.strokeMiterLimit,
            };
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
}