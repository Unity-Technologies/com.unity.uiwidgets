using System;
using System.Collections.Generic;
using System.Linq;
using Unity.UIWidgets.animation;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.external;
using Unity.UIWidgets.ui;
using UnityEngine;
using Color = Unity.UIWidgets.ui.Color;
using Rect = Unity.UIWidgets.ui.Rect;
using Shader = Unity.UIWidgets.ui.Shader;

namespace Unity.UIWidgets.painting {
    class _ColorsAndStops {
        public _ColorsAndStops(List<Color> colors, List<float> stops) {
            this.colors = colors;
            this.stops = stops;
        }

        public readonly List<Color> colors;
        public readonly List<float> stops;

        static Color _sample(List<Color> colors, List<float> stops, float t) {
            D.assert(colors != null);
            D.assert(colors.isNotEmpty);
            D.assert(stops != null);
            D.assert(stops.isNotEmpty);

            if (t < stops.first()) {
                return colors.first();
            }

            if (t > stops.last()) {
                return colors.last();
            }

            int index = stops.FindLastIndex((float s) => { return s <= t; });
            D.assert(index != -1);
            return Color.lerp(colors[index], colors[index + 1],
                (t - stops[index]) / (stops[index + 1] - stops[index]));
        }

        internal static _ColorsAndStops _interpolateColorsAndStops(
            List<Color> aColors,
            List<float> aStops,
            List<Color> bColors,
            List<float> bStops,
            float t) {
            D.assert(aColors.Count >= 2);
            D.assert(bColors.Count >= 2);
            D.assert(aStops.Count == aColors.Count);
            D.assert(bStops.Count == bColors.Count);

            SplayTree<float, bool> stops = new SplayTree<float, bool>();
            stops.AddAll(aStops);
            stops.AddAll(bStops);

            List<float> interpolatedStops = stops.Keys.ToList();
            List<Color> interpolatedColors = interpolatedStops.Select<float, Color>((float stop) => {
                return Color.lerp(_sample(aColors, aStops, stop), _sample(bColors, bStops, stop), t);
            }).ToList();

            return new _ColorsAndStops(interpolatedColors, interpolatedStops);
        }
    }

    public abstract class GradientTransform {
        public GradientTransform() {
        }

        public abstract Matrix4 transform(Rect bounds, TextDirection? textDirection = null);
    }

    public class GradientRotation : GradientTransform {
        public GradientRotation(float radians) {
            this.radians = radians;
        }

        public readonly float radians;

        public override
            Matrix4 transform(Rect bounds,
                TextDirection? textDirection = null
            ) {
            D.assert(bounds != null);
            float sinRadians = Mathf.Sin(radians);
            float oneMinusCosRadians = 1 - Mathf.Cos(radians);
            Offset center = bounds.center;
            float originX = sinRadians * center.dy + oneMinusCosRadians * center.dx;
            float originY = -sinRadians * center.dx + oneMinusCosRadians * center.dy;
            var result = Matrix4.identity();
            result.translate(originX, originY);
            result.rotateZ(radians);
            return result;
        }
    }

    public abstract class Gradient {
        public Gradient(
            List<Color> colors = null,
            List<float> stops = null,
            GradientTransform transform = null
        ) {
            D.assert(colors != null);
            this.colors = colors;
            this.stops = stops;
            this.transform = transform;
        }

        public readonly List<Color> colors;

        public readonly List<float> stops;

        public readonly GradientTransform transform;

        protected List<float> _impliedStops() {
            if (stops != null) {
                return stops;
            }

            D.assert(colors.Count >= 2, () => "colors list must have at least two colors");
            float separation = 1.0f / (colors.Count - 1);
            return LinqUtils<float, int>.SelectList(Enumerable.Range(0, colors.Count), (i => i * separation));
        }

        public abstract Shader createShader(Rect rect, TextDirection? textDirection = null);

        public abstract Gradient scale(float factor);

        protected virtual Gradient lerpFrom(Gradient a, float t) {
            if (a == null) {
                return scale(t);
            }

            return null;
        }

        protected virtual Gradient lerpTo(Gradient b, float t) {
            if (b == null) {
                return scale(1.0f - t);
            }

            return null;
        }


        public static Gradient lerp(Gradient a, Gradient b, float t) {
            Gradient result = null;
            if (b != null) {
                result = b.lerpFrom(a, t); // if a is null, this must return non-null
            }

            if (result == null && a != null) {
                result = a.lerpTo(b, t); // if b is null, this must return non-null
            }

            if (result != null) {
                return result;
            }

            if (a == null && b == null) {
                return null;
            }

            D.assert(a != null && b != null);
            return t < 0.5 ? a.scale(1.0f - (t * 2.0f)) : b.scale((t - 0.5f) * 2.0f);
        }

        protected float[] _resolveTransform(Rect bounds, TextDirection? textDirection) {
            return transform?.transform(bounds, textDirection: textDirection)?.storage;
        }
    }


    public class LinearGradient : Gradient, IEquatable<LinearGradient> {
        public LinearGradient(
            AlignmentGeometry begin = null,
            AlignmentGeometry end = null,
            List<Color> colors = null,
            List<float> stops = null,
            TileMode tileMode = TileMode.clamp,
            GradientTransform transform = null
        ) : base(colors: colors, stops: stops, transform: transform) {
            this.begin = begin ?? Alignment.centerLeft;
            this.end = end ?? Alignment.centerRight;
            this.tileMode = tileMode;
        }

        public readonly AlignmentGeometry begin;

        public readonly AlignmentGeometry end;

        public readonly TileMode tileMode;

        public override Shader createShader(Rect rect, TextDirection? textDirection = null) {
            return ui.Gradient.linear(
                begin.resolve(textDirection).withinRect(rect),
                end.resolve(textDirection).withinRect(rect),
                colors, _impliedStops(),
                tileMode, _resolveTransform(rect, textDirection)
            );
        }

        public override Gradient scale(float factor) {
            return new LinearGradient(
                begin: begin,
                end: end,
                colors: LinqUtils<Color>.SelectList(colors,(color => Color.lerp(null, color, factor))),
                stops: stops,
                tileMode: tileMode
            );
        }

        protected override Gradient lerpFrom(Gradient a, float t) {
            if (a == null || (a is LinearGradient)) {
                return lerp((LinearGradient) a, this, t);
            }

            return base.lerpFrom(a, t);
        }

        protected override Gradient lerpTo(Gradient b, float t) {
            if (b == null || (b is LinearGradient)) {
                return lerp(this, (LinearGradient) b, t);
            }

            return base.lerpTo(b, t);
        }

        public static LinearGradient lerp(LinearGradient a, LinearGradient b, float t) {
            if (a == null && b == null) {
                return null;
            }

            if (a == null) {
                return (LinearGradient) b.scale(t);
            }

            if (b == null) {
                return (LinearGradient) a.scale(1.0f - t);
            }

            _ColorsAndStops interpolated = _ColorsAndStops._interpolateColorsAndStops(
                a.colors,
                a._impliedStops(),
                b.colors,
                b._impliedStops(),
                t);
            return new LinearGradient(
                begin: AlignmentGeometry.lerp(a.begin, b.begin, t),
                end: AlignmentGeometry.lerp(a.end, b.end, t),
                colors: interpolated.colors,
                stops: interpolated.stops,
                tileMode: t < 0.5 ? a.tileMode : b.tileMode
            );
        }

        public bool Equals(LinearGradient other) {
            if (ReferenceEquals(null, other)) {
                return false;
            }

            if (ReferenceEquals(this, other)) {
                return true;
            }

            return
                colors.equalsList(other.colors) &&
                stops.equalsList(other.stops) &&
                Equals(begin, other.begin) &&
                Equals(end, other.end) &&
                tileMode == other.tileMode;
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

            return Equals((LinearGradient) obj);
        }

        public override int GetHashCode() {
            unchecked {
                var hashCode = colors.hashList();
                hashCode = (hashCode * 397) ^ stops.hashList();
                hashCode = (hashCode * 397) ^ (begin != null ? begin.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (end != null ? end.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (int) tileMode;
                return hashCode;
            }
        }

        public static bool operator ==(LinearGradient left, LinearGradient right) {
            return Equals(left, right);
        }

        public static bool operator !=(LinearGradient left, LinearGradient right) {
            return !Equals(left, right);
        }

        public override string ToString() {
            return $"{foundation_.objectRuntimeType(this, "LinearGradient")}({begin}, {end}," +
                   $"{colors.toStringList()}, {stops.toStringList()}, {tileMode})";
        }
    }

    public class RadialGradient : Gradient, IEquatable<RadialGradient> {
        public RadialGradient(
            AlignmentGeometry center = null,
            float radius = 0.5f,
            List<Color> colors = null,
            List<float> stops = null,
            TileMode tileMode = TileMode.clamp,
            AlignmentGeometry focal = null,
            float focalRadius = 0.0f,
            GradientTransform transform = null
        ) : base(colors: colors, stops: stops, transform: transform) {
            this.center = center ?? Alignment.center;
            this.radius = radius;
            this.tileMode = tileMode;
            this.focal = focal;
            this.focalRadius = focalRadius;
        }

        public readonly AlignmentGeometry center;

        public readonly float radius;

        public readonly TileMode tileMode;

        public readonly AlignmentGeometry focal;

        public readonly float focalRadius;

        public override Shader createShader(Rect rect, TextDirection? textDirection = null) {
            return ui.Gradient.radial(
                center.resolve(textDirection).withinRect(rect),
                radius * rect.shortestSide,
                colors, _impliedStops(),
                tileMode,
                _resolveTransform(rect, textDirection),
                focal == null ? null : focal.resolve(textDirection).withinRect(rect),
                focalRadius * rect.shortestSide
            );
        }

        public override Gradient scale(float factor) {
            return new RadialGradient(
                center: center,
                radius: radius,
                colors: LinqUtils<Color>.SelectList(colors,(color => Color.lerp(null, color, factor))),
                stops: stops,
                tileMode: tileMode
            );
        }

        protected override Gradient lerpFrom(Gradient a, float t) {
            if (a == null || (a is RadialGradient)) {
                return lerp((RadialGradient) a, this, t);
            }

            return base.lerpFrom(a, t);
        }

        protected override Gradient lerpTo(Gradient b, float t) {
            if (b == null || (b is RadialGradient)) {
                return lerp(this, (RadialGradient) b, t);
            }

            return base.lerpTo(b, t);
        }

        public static RadialGradient lerp(RadialGradient a, RadialGradient b, float t) {
            if (a == null && b == null) {
                return null;
            }

            if (a == null) {
                return (RadialGradient) b.scale(t);
            }

            if (b == null) {
                return (RadialGradient) a.scale(1.0f - t);
            }

            _ColorsAndStops interpolated = _ColorsAndStops._interpolateColorsAndStops(
                a.colors,
                a._impliedStops(),
                b.colors,
                b._impliedStops(),
                t);
            return new RadialGradient(
                center: AlignmentGeometry.lerp(a.center, b.center, t),
                radius: Mathf.Max(0.0f, MathUtils.lerpNullableFloat(a.radius, b.radius, t)),
                colors: interpolated.colors,
                stops: interpolated.stops,
                tileMode: t < 0.5 ? a.tileMode : b.tileMode,
                focal: AlignmentGeometry.lerp(a.focal, b.focal, t),
                focalRadius: Mathf.Max(0.0f, MathUtils.lerpNullableFloat(a.focalRadius, b.focalRadius, t))
            );
        }

        public bool Equals(RadialGradient other) {
            if (ReferenceEquals(null, other)) {
                return false;
            }

            if (ReferenceEquals(this, other)) {
                return true;
            }

            return
                colors.equalsList(other.colors) &&
                stops.equalsList(other.stops) &&
                Equals(center, other.center) &&
                Equals(radius, other.radius) &&
                tileMode == other.tileMode;
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

            return Equals((RadialGradient) obj);
        }

        public override int GetHashCode() {
            unchecked {
                var hashCode = colors.hashList();
                hashCode = (hashCode * 397) ^ stops.hashList();
                hashCode = (hashCode * 397) ^ (center != null ? center.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ radius.GetHashCode();
                hashCode = (hashCode * 397) ^ (int) tileMode;
                return hashCode;
            }
        }

        public static bool operator ==(RadialGradient left, RadialGradient right) {
            return Equals(left, right);
        }

        public static bool operator !=(RadialGradient left, RadialGradient right) {
            return !Equals(left, right);
        }

        public override string ToString() {
            return $"{foundation_.objectRuntimeType(this, "RadialGradient")}({center}, {radius}," +
                   $"{colors.toStringList()}, {stops.toStringList()}, {tileMode})";
        }
    }

    public class SweepGradient : Gradient, IEquatable<SweepGradient> {
        public SweepGradient(
            AlignmentGeometry center = null,
            float startAngle = 0.0f,
            float endAngle = Mathf.PI * 2,
            List<Color> colors = null,
            List<float> stops = null,
            TileMode tileMode = TileMode.clamp,
            GradientTransform transform = null
        ) : base(colors: colors, stops: stops, transform: transform) {
            this.center = center ?? Alignment.center;
            this.startAngle = startAngle;
            this.endAngle = endAngle;
            this.tileMode = tileMode;
        }

        public readonly AlignmentGeometry center;

        public readonly float startAngle;

        public readonly float endAngle;

        public readonly TileMode tileMode;


        public override Shader createShader(Rect rect, TextDirection? textDirection = null) {
            return ui.Gradient.sweep(
                center.resolve(textDirection).withinRect(rect),
                colors, _impliedStops(),
                tileMode,
                startAngle, endAngle, _resolveTransform(rect, textDirection)
            );
        }

        public override Gradient scale(float factor) {
            return new SweepGradient(
                center: center,
                startAngle: startAngle,
                endAngle: endAngle,
                colors: LinqUtils<Color>.SelectList(colors,(color => Color.lerp(null, color, factor))),
                stops: stops,
                tileMode: tileMode
            );
        }

        protected override Gradient lerpFrom(Gradient a, float t) {
            if (a == null || (a is SweepGradient && a.colors.Count == colors.Count)) {
                return lerp((SweepGradient) a, this, t);
            }

            return base.lerpFrom(a, t);
        }

        protected override Gradient lerpTo(Gradient b, float t) {
            if (b == null || (b is SweepGradient && b.colors.Count == colors.Count)) {
                return lerp(this, (SweepGradient) b, t);
            }

            return base.lerpTo(b, t);
        }

        public static SweepGradient lerp(SweepGradient a, SweepGradient b, float t) {
            if (a == null && b == null) {
                return null;
            }

            if (a == null) {
                return (SweepGradient) b.scale(t);
            }

            if (b == null) {
                return (SweepGradient) a.scale(1.0f - t);
            }

            _ColorsAndStops interpolated =
                _ColorsAndStops._interpolateColorsAndStops(a.colors, a.stops, b.colors, b.stops, t);
            return new SweepGradient(
                center: Alignment.lerp(a.center, b.center, t),
                startAngle: Mathf.Max(0.0f, MathUtils.lerpNullableFloat(a.startAngle, b.startAngle, t)),
                endAngle: Mathf.Max(0.0f, MathUtils.lerpNullableFloat(a.endAngle, b.endAngle, t)),
                colors: interpolated.colors,
                stops: interpolated.stops,
                tileMode: t < 0.5 ? a.tileMode : b.tileMode
            );
        }

        public bool Equals(SweepGradient other) {
            if (ReferenceEquals(null, other)) {
                return false;
            }

            if (ReferenceEquals(this, other)) {
                return true;
            }

            return
                colors.equalsList(other.colors) &&
                stops.equalsList(other.stops) &&
                Equals(center, other.center) &&
                Equals(startAngle, other.startAngle) &&
                Equals(endAngle, other.endAngle) &&
                tileMode == other.tileMode;
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

            return Equals((SweepGradient) obj);
        }

        public override int GetHashCode() {
            unchecked {
                var hashCode = colors.hashList();
                hashCode = (hashCode * 397) ^ stops.hashList();
                hashCode = (hashCode * 397) ^ (center != null ? center.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ startAngle.GetHashCode();
                hashCode = (hashCode * 397) ^ endAngle.GetHashCode();
                hashCode = (hashCode * 397) ^ (int) tileMode;
                return hashCode;
            }
        }

        public static bool operator ==(SweepGradient left, SweepGradient right) {
            return Equals(left, right);
        }

        public static bool operator !=(SweepGradient left, SweepGradient right) {
            return !Equals(left, right);
        }

        public override string ToString() {
            return $"{foundation_.objectRuntimeType(this, "SweepGradient")}({center}, {startAngle}," +
                   $" {endAngle}, {colors.toStringList()}, {stops.toStringList()}, {tileMode})";
        }
    }
}