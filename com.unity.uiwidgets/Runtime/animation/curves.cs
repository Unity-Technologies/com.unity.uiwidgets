using System;
using System.Collections.Generic;
using System.Linq;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.ui;
using UnityEngine;
using Random = System.Random;

namespace Unity.UIWidgets.animation {
    public abstract class ParametricCurve<T> {
        public virtual T transform(float t) {
            D.assert(t >= 0.0f && t <= 1.0f, () => $"parametric value {t} is outside of [0, 1] range.");
            return transformInternal(t);
        }
        
        protected virtual T transformInternal(float t) {
            throw new NotImplementedException();
        }

        public override string ToString() {
            return $"{GetType()}";
        }
    }
    
    public abstract class Curve : ParametricCurve<float> {
        public override float transform(float t) {
            if (t == 0.0f || t == 1.0f) {
                return t;
            }

            return base.transform(t);
        }

        public Curve flipped {
            get { return new FlippedCurve(this); }
        }
    }

    class _Linear : Curve {
        protected override float transformInternal(float t) {
            return t;
        }
    }

    public class SawTooth : Curve {
        public SawTooth(int count) {
            this.count = count;
        }

        public readonly int count;

        protected override float transformInternal(float t) {
            t *= count;
            return t - (int) t;
        }

        public override string ToString() {
            return $"{GetType()}({count})";
        }
    }

    public class Interval : Curve {
        public Interval(float begin, float end, Curve curve = null) {
            this.begin = begin;
            this.end = end;
            this.curve = curve ?? Curves.linear;
        }

        public readonly float begin;

        public readonly float end;

        public readonly Curve curve;

        protected override float transformInternal(float t) {
            D.assert(t >= 0.0 && t <= 1.0);
            D.assert(begin >= 0.0);
            D.assert(begin <= 1.0);
            D.assert(end >= 0.0);
            D.assert(end <= 1.0);
            D.assert(end >= begin);
            t = ((t - begin) / (end - begin)).clamp(0.0f, 1.0f);
            if (t == 0.0 || t == 1.0) {
                return t;
            }

            return curve.transform(t);
        }

        public override string ToString() {
            if (!(curve is _Linear)) {
                return $"{GetType()}({begin}\u22EF{end}\u27A9{curve}";
            }

            return $"{GetType()}({begin}\u22EF{end})";
        }
    }

    public class Threshold : Curve {
        public Threshold(float threshold) {
            this.threshold = threshold;
        }

        public readonly float threshold;

        protected override float transformInternal(float t) {
            D.assert(threshold >= 0.0);
            D.assert(threshold <= 1.0);
            return t < threshold ? 0.0f : 1.0f;
        }
    }

    public class Cubic : Curve {
        public Cubic(float a, float b, float c, float d) {
            this.a = a;
            this.b = b;
            this.c = c;
            this.d = d;
        }

        public readonly float a;

        public readonly float b;

        public readonly float c;

        public readonly float d;

        const float _cubicErrorBound = 0.001f;

        float _evaluateCubic(float a, float b, float m) {
            return 3 * a * (1 - m) * (1 - m) * m +
                   3 * b * (1 - m) * m * m +
                   m * m * m;
        }

        protected override float transformInternal(float t) {
            float start = 0.0f;
            float end = 1.0f;
            while (true) {
                float midpoint = (start + end) / 2;
                float estimate = _evaluateCubic(a, c, midpoint);
                if ((t - estimate).abs() < _cubicErrorBound) {
                    return _evaluateCubic(b, d, midpoint);
                }

                if (estimate < t) {
                    start = midpoint;
                }
                else {
                    end = midpoint;
                }
            }
        }

        public override string ToString() {
            return $"{GetType()}({a:F2}, {b:F2}, {c:F2}, {d:F2})";
        }
    }

    public abstract class Curve2D : ParametricCurve<Offset> {
        internal IEnumerable<Curve2DSample> generateSamples(
            float start = 0.0f,
            float end = 1.0f,
            float tolerance = 1e-10f) {
            D.assert(end > start);

            Random rand = new Random(samplingSeed);
            bool isFlat(Offset p, Offset q, Offset r) {
                Offset pr = p - r;
                Offset qr = q - r;
                float z = pr.dx * qr.dy - qr.dx * pr.dy;
                return z * z < tolerance;
            }
            
            Curve2DSample first = new Curve2DSample(start, transform(start));
            Curve2DSample last = new Curve2DSample(end, transform(end));
            List<Curve2DSample> samples = new List<Curve2DSample>(){first};

            void sample(Curve2DSample p, Curve2DSample q, bool forceSubdivide = false) {
                float t = p.t + (0.45f + 0.1f * (float)rand.NextDouble() * (q.t - p.t));
                Curve2DSample r = new Curve2DSample(t, transform(t));

                if (!forceSubdivide && isFlat(p.value, q.value, r.value)) {
                    samples.Add(q);
                }
                else {
                    sample(p, r);
                    sample(r, q);
                }
            }
            
            sample(first, last,
                forceSubdivide: (first.value.dx - last.value.dx).abs() < tolerance &&
                                (first.value.dy - last.value.dy).abs() < tolerance);

            return samples;
        }

        protected virtual int samplingSeed {
            get { return 0; }
        }

        public float findInverse(float x) {
            float start = 0.0f;
            float end = 1.0f;
            float mid = 0f;

            float offsetToOrigin(float pos) {
                return x - transform(pos).dx;
            }

            const float errorLimit = 1e-6f;
            int count = 100;
            float startValue = offsetToOrigin(start);
            while ((end - start / 2.0f) > errorLimit && count > 0) {
                mid = (end + start) / 2.0f;
                float value = offsetToOrigin(mid);
                if (value.sign() == startValue.sign()) {
                    start = mid;
                }
                else {
                    end = mid;
                }

                count--;
            }

            return mid;
        }
    }

    internal class Curve2DSample {
        public Curve2DSample(float t, Offset value) {
            this.t = t;
            this.value = value;
        }
        
        public readonly float t;
        public readonly Offset value;

        public override string ToString() {
            return $"[{value.dx:F2}, {value.dy:F2}, {t:F2}]";
        }
    }

    class CatmullRomSpline : Curve2D {
        CatmullRomSpline(
            List<Offset> controlPoints,
            float? tension = 0.0f,
            Offset startHandle = null,
            Offset endHandle = null
        ) {
            D.assert(controlPoints != null);
            D.assert(tension != null);
            D.assert(tension <= 1.0f, () => $"tension {tension} must not be greater than 1.0.");
            D.assert(tension >= 0.0f, () => $"tension {tension} must not be negative.");
            D.assert(controlPoints.Count > 3, () => "There must be at least four control points to create a CatmullRomSpline.");
            _controlPoints = controlPoints;
            _startHandle = startHandle;
            _endHandle = endHandle;
            _tension = tension;
            _cubicSegments = new List<List<Offset>>();
        }
        
        internal CatmullRomSpline(
            List<Offset> controlPoints,
            float? tension = 0.0f,
            Offset startHandle = null,
            Offset endHandle = null,
            List<List<Offset>> cubicSegments = null
        ) {
            D.assert(cubicSegments != null);
            _controlPoints = controlPoints;
            _startHandle = startHandle;
            _endHandle = endHandle;
            _tension = tension;
            _cubicSegments = new List<List<Offset>>();
        }

        public static CatmullRomSpline precompute(
            List<Offset> controlPoints,
            float? tension = 0.0f,
            Offset startHandle = null,
            Offset endHandle = null
        ) {
            D.assert(controlPoints != null);
            D.assert(tension != null);
            D.assert(tension <= 1.0f, () => $"tension {tension} must not be greater than 1.0.");
            D.assert(tension >= 0.0f, () => $"tension {tension} must not be negative.");
            D.assert(controlPoints.Count > 3, () => "There must be at least four control points to create a CatmullRomSpline.");
            return new CatmullRomSpline(
                controlPoints: null,
                tension: null,
                startHandle: null,
                endHandle: null,
                cubicSegments: _computeSegments(controlPoints, tension, startHandle: startHandle, endHandle: endHandle)
            );
        }

        static List<List<Offset>> _computeSegments(
            List<Offset> controlPoints,
            float? tension,
            Offset startHandle,
            Offset endHandle) {
            startHandle = startHandle ?? controlPoints[0] * 2.0f - controlPoints[1];
            endHandle = endHandle ?? controlPoints.last() * 2.0f - controlPoints[controlPoints.Count - 2];

            List<Offset> allPoints = new List<Offset>();
            allPoints.Add(startHandle);
            allPoints.AddRange(controlPoints);
            allPoints.Add(endHandle);
            
            const float alpha = 0.5f;
            float reverseTension = 1.0f - tension.Value;
            List<List<Offset>> result = new List<List<Offset>>();
            for (int i = 0; i < allPoints.Count - 3; ++i) {
                List<Offset> curve = new List<Offset>{allPoints[i], allPoints[i + 1], allPoints[i + 2], allPoints[i + 3]};
                Offset diffCurve10 = curve[1] - curve[0];
                Offset diffCurve21 = curve[2] - curve[1];
                Offset diffCurve32 = curve[3] - curve[2];
                float t01 = Mathf.Pow(diffCurve10.distance, alpha);
                float t12 = Mathf.Pow(diffCurve21.distance, alpha);
                float t23 = Mathf.Pow(diffCurve32.distance, alpha);
                
                Offset m1 = (diffCurve21 + (diffCurve10 / t01 - (curve[2] - curve[0]) / (t01 + t12)) * t12) * reverseTension;
                Offset m2 = (diffCurve21 + (diffCurve32 / t23 - (curve[3] - curve[1]) / (t12 + t23)) * t12) * reverseTension;
                Offset sumM12 = m1 + m2;

                List<Offset> segment = new List<Offset> {
                    diffCurve21 * -2.0f + sumM12,
                    diffCurve21 * 3.0f - m1 - sumM12,
                    m1,
                    curve[1]
                };
                result.Add(segment);
            }

            return result;
        }

        readonly List<List<Offset>> _cubicSegments;

        readonly List<Offset> _controlPoints;
        readonly Offset _startHandle;
        readonly Offset _endHandle;
        readonly float? _tension;

        void _initializeIfNeeded() {
            if (_cubicSegments.isNotEmpty()) {
                return;
            }
            _cubicSegments.AddRange(_computeSegments(_controlPoints, _tension, startHandle: _startHandle, endHandle: _endHandle));
        }

        protected override int samplingSeed {
            get {
                _initializeIfNeeded();
                Offset seedPoint = _cubicSegments[0][1];
                return ((seedPoint.dx + seedPoint.dy) * 10000).round();
            }
        }

        protected override Offset transformInternal(float t) {
            _initializeIfNeeded();
            float length = _cubicSegments.Count;
            float position;
            float localT;
            int index;
            
            if (t < 1.0f) {
                position = t * length;
                localT = position - position.floor();
                index = position.floor();
            } else {
                position = length;
                localT = 1.0f;
                index = _cubicSegments.Count - 1;
            }
            List<Offset> cubicControlPoints = _cubicSegments[index];
            float localT2 = localT * localT;
            return cubicControlPoints[0] * localT2 * localT
                   + cubicControlPoints[1] * localT2
                   + cubicControlPoints[2] * localT
                   + cubicControlPoints[3];
        }
    }

    public class CatmullRomCurve : Curve {
        public CatmullRomCurve(
            List<Offset> controlPoints,
            float? tension = 0.0f) {
            D.assert(tension != null);
            
            this.controlPoints = controlPoints;
            this.tension = tension;
            
            D.assert(() => {
                _debugAssertReasons.Clear();
                return validateControlPoints(controlPoints,
                    tension: tension,
                    reasons: _debugAssertReasons);
            }, () => $"control points {controlPoints} could not be validated:\n  {string.Join("\n ", _debugAssertReasons)}");
            _precomputedSamples = new List<Curve2DSample>();
        }
        
        internal CatmullRomCurve(
            List<Offset> controlPoints,
            float? tension = 0.0f,
            List<Curve2DSample> precomputedSamples = null) {
            D.assert(precomputedSamples != null);
            D.assert(tension != null);
            
            this.controlPoints = controlPoints;
            this.tension = tension;
            
            D.assert(() => {
                _debugAssertReasons.Clear();
                return validateControlPoints(controlPoints,
                    tension: tension,
                    reasons: _debugAssertReasons);
            }, () => $"control points {controlPoints} could not be validated:\n  {string.Join("\n ", _debugAssertReasons)}");

            _precomputedSamples = precomputedSamples;
        }

        public static CatmullRomCurve precompute(
            List<Offset> controlPoints, float? tension = 0.0f) {
            return new CatmullRomCurve(
                controlPoints,
                tension,
                _computeSamples(controlPoints, tension));
        }

        static List<Curve2DSample> _computeSamples(List<Offset> controlPoints, float? tension) {
            List<Offset> _controlPoints = new List<Offset>();
            _controlPoints.Add(Offset.zero);
            _controlPoints.AddRange(controlPoints);
            _controlPoints.Add(new Offset(1.0f, 1.0f));
            
            return CatmullRomSpline.precompute(_controlPoints, tension: tension).generateSamples(
                start: 0.0f, end: 1.0f, tolerance: 1e-12f).ToList();
        }
        
        static readonly List<string> _debugAssertReasons = new List<string>();

        readonly List<Curve2DSample> _precomputedSamples;

        public readonly List<Offset> controlPoints;

        public readonly float? tension;

        static bool validateControlPoints(
            List<Offset> controlPoints,
            float? tension = 0.0f,
            List<string> reasons = null) {
            D.assert(tension != null);
            if (controlPoints == null) {
                D.assert(() => {
                    reasons?.Add("Supplied control points cannot be null");
                    return true;
                });
                return false;
            }

            if (controlPoints.Count < 2) {
                D.assert(() => {
                    reasons?.Add("There must be at least two points supplied to create a valid curve.");
                    return true;
                });
                return false;
            }

            List<Offset> _controlPoints = new List<Offset>();
            _controlPoints.AddRange(controlPoints);
            
            _controlPoints.Insert(0, Offset.zero);
            _controlPoints.Add(new Offset(1.0f, 1.0f));
            
            Offset startHandle = _controlPoints[0] * 2.0f - _controlPoints[1];
            Offset endHandle = _controlPoints.last() * 2.0f - _controlPoints[_controlPoints.Count - 2];
            _controlPoints.Insert(0, startHandle);
            _controlPoints.Add(endHandle);

            float lastX = -float.PositiveInfinity;
            for (int i = 0; i < _controlPoints.Count; ++i) {
                if (i > 1 &&
                    i < _controlPoints.Count - 2 &&
                    (_controlPoints[i].dx <= 0.0f || _controlPoints[i].dx >= 1.0f)) {
                    D.assert(() => {
                        reasons?.Add("Control points must have X values between 0.0 and 1.0, exclusive. " +
                        $"Point {i} has an x value ({_controlPoints[i].dx}) which is outside the range.");
                        return true;
                    });
                    return false;
                }
                if (_controlPoints[i].dx <= lastX) {
                    D.assert(() => {
                        reasons?.Add("Each X coordinate must be greater than the preceding X coordinate " +
                        $"(i.e. must be monotonically increasing in X). Point {i} has an x value of " + 
                        $"{_controlPoints[i].dx}, which is not greater than {lastX}");
                        return true;
                    });
                    return false;
                }
                lastX = _controlPoints[i].dx;
            }
            
            bool success = true;
            lastX = -float.PositiveInfinity;
            const float tolerance = 1e-3f;
            CatmullRomSpline testSpline = new CatmullRomSpline(_controlPoints, tension: tension);
            float start = testSpline.findInverse(0.0f);
            float end = testSpline.findInverse(1.0f);
            IEnumerable<Curve2DSample> samplePoints = testSpline.generateSamples(start: start, end: end);

            if (samplePoints.First().value.dy.abs() > tolerance ||
                (1.0f - samplePoints.Last().value.dy).abs() > tolerance) {
                bool bail = true;
                success = false;
                D.assert(() => {
                    reasons?.Add($"The curve has more than one Y value at X = {samplePoints.First().value.dx}. " +
                    "Try moving some control points further away from this value of X, or increasing " + 
                    "the tension.");
                    bail = reasons == null;
                    return true;
                });
                if (bail) {
                    return false;
                }
            }
            foreach (Curve2DSample sample in samplePoints) {
                Offset point = sample.value;
                float t = sample.t;
                float x = point.dx;
                if (t >= start && t <= end && (x < -1e-3f || x > 1.0f + 1e-3f)) {
                    bool bail = true;
                    success = false;
                    D.assert(() => {
                        reasons?.Add($"The resulting curve has an X value ({x}) which is outside " +
                        "the range [0.0, 1.0], inclusive.");
                        bail = reasons == null;
                        return true;
                    });
                    if (bail) {
                        return false;
                    }
                }
                if (x < lastX) {
                    bool bail = true;
                    success = false;
                    D.assert(() => {
                        reasons?.Add($"The curve has more than one Y value at x = {x}. Try moving " +
                        "some control points further apart in X, or increasing the tension.");
                        bail = reasons == null;
                        return true;
                    });
                    if (bail) {
                        return false;
                    }
                }
                lastX = x;
            }
            return success;
        }

        protected override float transformInternal(float t) {
            if (_precomputedSamples.isEmpty()) {
                // Compute the samples now if we were constructed lazily.
                _precomputedSamples.AddRange(_computeSamples(controlPoints, tension));
            }
            int start = 0;
            int end = _precomputedSamples.Count - 1;
            int mid;
            Offset value;
            Offset startValue = _precomputedSamples[start].value;
            Offset endValue = _precomputedSamples[end].value;
            while (end - start > 1) {
                mid = (end + start) / 2;
                value = _precomputedSamples[mid].value;
                if (t >= value.dx) {
                    start = mid;
                    startValue = value;
                } else {
                    end = mid;
                    endValue = value;
                }
            }
            float t2 = (t - startValue.dx) / (endValue.dx - startValue.dx);
            return MathUtils.lerpNullableFloat(startValue.dy, endValue.dy, t2);
        }
    }

    public class FlippedCurve : Curve {
        public FlippedCurve(Curve curve) {
            D.assert(curve != null);
            this.curve = curve;
        }

        public readonly Curve curve;

        protected override float transformInternal(float t) {
            return 1.0f - curve.transform(1.0f - t);
        }

        public override string ToString() {
            return $"{GetType()}({curve})";
        }
    }

    class _DecelerateCurve : Curve {
        internal _DecelerateCurve() {
        }

        protected override float transformInternal(float t) {
            t = 1.0f - t;
            return 1.0f - t * t;
        }
    }

    class _BounceInCurve : Curve {
        internal _BounceInCurve() {
        }

        protected override float transformInternal(float t) {
            return 1.0f - Curves._bounce(1.0f - t);
        }
    }

    class _BounceOutCurve : Curve {
        internal _BounceOutCurve() {
        }

        protected override float transformInternal(float t) {
            return Curves._bounce(t);
        }
    }

    class _BounceInOutCurve : Curve {
        internal _BounceInOutCurve() {
        }

        protected override float transformInternal(float t) {
            if (t < 0.5f) {
                return (1.0f - Curves._bounce(1.0f - t)) * 0.5f;
            }
            else {
                return Curves._bounce(t * 2.0f - 1.0f) * 0.5f + 0.5f;
            }
        }
    }

    public class ElasticInCurve : Curve {
        public ElasticInCurve(float period = 0.4f) {
            this.period = period;
        }

        public readonly float period;

        protected override float transformInternal(float t) {
            float s = period / 4.0f;
            t = t - 1.0f;
            return -Mathf.Pow(2.0f, 10.0f * t) * Mathf.Sin((t - s) * (Mathf.PI * 2.0f) / period);
        }

        public override string ToString() {
            return $"{GetType()}({period})";
        }
    }

    public class ElasticOutCurve : Curve {
        public ElasticOutCurve(float period = 0.4f) {
            this.period = period;
        }

        public readonly float period;

        protected override float transformInternal(float t) {
            float s = period / 4.0f;
            return Mathf.Pow(2.0f, -10.0f * t) * Mathf.Sin((t - s) * (Mathf.PI * 2.0f) / period) + 1.0f;
        }

        public override string ToString() {
            return $"{GetType()}({period})";
        }
    }

    public class ElasticInOutCurve : Curve {
        public ElasticInOutCurve(float period = 0.4f) {
            this.period = period;
        }

        public readonly float period;

        protected override float transformInternal(float t) {
            float s = period / 4.0f;
            t = 2.0f * t - 1.0f;
            if (t < 0.0) {
                return -0.5f * Mathf.Pow(2.0f, 10.0f * t) * Mathf.Sin((t - s) * (Mathf.PI * 2.0f) / period);
            }
            else {
                return Mathf.Pow(2.0f, -10.0f * t) * Mathf.Sin((t - s) * (Mathf.PI * 2.0f) / period) * 0.5f +
                       1.0f;
            }
        }

        public override string ToString() {
            return $"{GetType()}({period})";
        }
    }

    public static class Curves {
        public static readonly Curve linear = new _Linear();

        public static readonly Curve decelerate = new _DecelerateCurve();

        public static readonly Cubic fastLinearToSlowEaseIn = new Cubic(0.18f, 1.0f, 0.04f, 1.0f);

        public static readonly Curve ease = new Cubic(0.25f, 0.1f, 0.25f, 1.0f);

        public static readonly Curve easeIn = new Cubic(0.42f, 0.0f, 1.0f, 1.0f);

        public static readonly Cubic easeInToLinear = new Cubic(0.67f, 0.03f, 0.65f, 0.09f);

        public static readonly Cubic easeInSine = new Cubic(0.47f, 0, 0.745f, 0.715f);

        public static readonly Cubic easeInQuad = new Cubic(0.55f, 0.085f, 0.68f, 0.53f);

        public static readonly Cubic easeInCubic = new Cubic(0.55f, 0.055f, 0.675f, 0.19f);

        public static readonly Cubic easeInQuart = new Cubic(0.895f, 0.03f, 0.685f, 0.22f);

        public static readonly Cubic easeInQuint = new Cubic(0.755f, 0.05f, 0.855f, 0.06f);

        public static readonly Cubic easeInExpo = new Cubic(0.95f, 0.05f, 0.795f, 0.035f);

        public static readonly Cubic easeInCirc = new Cubic(0.6f, 0.04f, 0.98f, 0.335f);

        public static readonly Cubic easeInBack = new Cubic(0.6f, -0.28f, 0.735f, 0.045f);

        public static readonly Curve easeOut = new Cubic(0.0f, 0.0f, 0.58f, 1.0f);

        public static readonly Cubic linearToEaseOut = new Cubic(0.35f, 0.91f, 0.33f, 0.97f);

        public static readonly Cubic easeOutSine = new Cubic(0.39f, 0.575f, 0.565f, 1.0f);

        public static readonly Cubic easeOutQuad = new Cubic(0.25f, 0.46f, 0.45f, 0.94f);

        public static readonly Cubic easeOutCubic = new Cubic(0.215f, 0.61f, 0.355f, 1.0f);

        public static readonly Cubic easeOutQuart = new Cubic(0.165f, 0.84f, 0.44f, 1.0f);

        public static readonly Cubic easeOutQuint = new Cubic(0.23f, 1.0f, 0.32f, 1.0f);

        public static readonly Cubic easeOutExpo = new Cubic(0.19f, 1.0f, 0.22f, 1.0f);

        public static readonly Cubic easeOutCirc = new Cubic(0.075f, 0.82f, 0.165f, 1.0f);

        public static readonly Cubic easeOutBack = new Cubic(0.175f, 0.885f, 0.32f, 1.275f);

        public static readonly Curve easeInOut = new Cubic(0.42f, 0.0f, 0.58f, 1.0f);

        public static readonly Cubic easeInOutSine = new Cubic(0.445f, 0.05f, 0.55f, 0.95f);

        public static readonly Cubic easeInOutQuad = new Cubic(0.455f, 0.03f, 0.515f, 0.955f);

        public static readonly Cubic easeInOutCubic = new Cubic(0.645f, 0.045f, 0.355f, 1.0f);

        public static readonly Cubic easeInOutQuart = new Cubic(0.77f, 0, 0.175f, 1.0f);

        public static readonly Cubic easeInOutQuint = new Cubic(0.86f, 0, 0.07f, 1.0f);

        public static readonly Cubic easeInOutExpo = new Cubic(1.0f, 0, 0, 1.0f);

        public static readonly Cubic easeInOutCirc = new Cubic(0.785f, 0.135f, 0.15f, 0.86f);

        public static readonly Cubic easeInOutBack = new Cubic(0.68f, -0.55f, 0.265f, 1.55f);

        public static readonly Cubic fastOutSlowIn = new Cubic(0.4f, 0.0f, 0.2f, 1.0f);
        
        public static readonly Cubic slowMiddle = new Cubic(0.15f, 0.85f, 0.85f, 0.15f);

        public static readonly Curve bounceIn = new _BounceInCurve();

        public static readonly Curve bounceOut = new _BounceOutCurve();

        public static readonly Curve bounceInOut = new _BounceInOutCurve();

        public static readonly Curve elasticIn = new ElasticInCurve();

        public static readonly Curve elasticOut = new ElasticOutCurve();

        public static readonly Curve elasticInOut = new ElasticInOutCurve();

        internal static float _bounce(float t) {
            if (t < 1.0f / 2.75f) {
                return 7.5625f * t * t;
            }
            else if (t < 2 / 2.75f) {
                t -= 1.5f / 2.75f;
                return 7.5625f * t * t + 0.75f;
            }
            else if (t < 2.5f / 2.75f) {
                t -= 2.25f / 2.75f;
                return 7.5625f * t * t + 0.9375f;
            }

            t -= 2.625f / 2.75f;
            return 7.5625f * t * t + 0.984375f;
        }
    }
}