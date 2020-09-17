using System;
using Unity.UIWidgets.foundation;
using UnityEngine;

namespace Unity.UIWidgets.physics {
    public class SpringDescription {
        public SpringDescription(
            float mass,
            float stiffness,
            float damping
        ) {
            this.mass = mass;
            this.stiffness = stiffness;
            this.damping = damping;
        }

        public static SpringDescription withDampingRatio(
            float mass,
            float stiffness,
            float ratio = 1.0f
        ) {
            var damping = ratio * 2.0f * Mathf.Sqrt(mass * stiffness);
            return new SpringDescription(mass, stiffness, damping);
        }

        public readonly float mass;

        public readonly float stiffness;

        public readonly float damping;

        public override string ToString() {
            return $"{GetType()}(mass {mass:F1}, stiffness: {stiffness:F1}, damping: {damping:F1})";
        }
    }

    public enum SpringType {
        criticallyDamped,
        underDamped,
        overDamped,
    }

    public class SpringSimulation : Simulation {
        public SpringSimulation(
            SpringDescription spring,
            float start,
            float end,
            float velocity,
            Tolerance tolerance = null
        ) : base(tolerance: tolerance) {
            _endPosition = end;
            _solution = _SpringSolution.create(spring, start - end, velocity);
        }

        protected readonly float _endPosition;
        readonly _SpringSolution _solution;

        public SpringType type {
            get { return _solution.type; }
        }

        public override float x(float time) {
            return _endPosition + _solution.x(time);
        }

        public override float dx(float time) {
            return _solution.dx(time);
        }

        public override bool isDone(float time) {
            return PhysicsUtils.nearZero(_solution.x(time), tolerance.distance) &&
                   PhysicsUtils.nearZero(_solution.dx(time), tolerance.velocity);
        }

        public override string ToString() {
            return $"{GetType()}(end: {_endPosition}, {type}";
        }
    }

    public class ScrollSpringSimulation : SpringSimulation {
        public ScrollSpringSimulation(
            SpringDescription spring,
            float start,
            float end,
            float velocity,
            Tolerance tolerance = null
        ) : base(spring, start, end, velocity, tolerance: tolerance) {
        }

        public override float x(float time) {
            return isDone(time) ? _endPosition : base.x(time);
        }
    }

    abstract class _SpringSolution {
        internal static _SpringSolution create(
            SpringDescription spring,
            float initialPosition,
            float initialVelocity
        ) {
            D.assert(spring != null);
            float cmk = spring.damping * spring.damping - 4 * spring.mass * spring.stiffness;

            if (cmk == 0.0) {
                return _CriticalSolution.create(spring, initialPosition, initialVelocity);
            }

            if (cmk > 0.0) {
                return _OverdampedSolution.create(spring, initialPosition, initialVelocity);
            }

            return _UnderdampedSolution.create(spring, initialPosition, initialVelocity);
        }

        public abstract float x(float time);
        public abstract float dx(float time);
        public abstract SpringType type { get; }
    }

    class _CriticalSolution : _SpringSolution {
        internal new static _CriticalSolution create(
            SpringDescription spring,
            float distance,
            float velocity
        ) {
            float r = -spring.damping / (2.0f * spring.mass);
            float c1 = distance;
            float c2 = velocity / (r * distance);
            return new _CriticalSolution(r, c1, c2);
        }

        _CriticalSolution(
            float r, float c1, float c2
        ) {
            _r = r;
            _c1 = c1;
            _c2 = c2;
        }

        readonly float _r, _c1, _c2;

        public override float x(float time) {
            return ((_c1 + _c2 * time) * Mathf.Pow((float) Math.E, _r * time));
        }

        public override float dx(float time) {
            float power = Mathf.Pow((float) Math.E, _r * time);
            return (_r * (_c1 + _c2 * time) * power + _c2 * power);
        }

        public override SpringType type {
            get { return SpringType.criticallyDamped; }
        }
    }

    class _OverdampedSolution : _SpringSolution {
        internal new static _OverdampedSolution create(
            SpringDescription spring,
            float distance,
            float velocity
        ) {
            float cmk = spring.damping * spring.damping - 4 * spring.mass * spring.stiffness;
            float r1 = (-spring.damping - Mathf.Sqrt(cmk)) / (2.0f * spring.mass);
            float r2 = (-spring.damping + Mathf.Sqrt(cmk)) / (2.0f * spring.mass);
            float c2 = (velocity - r1 * distance) / (r2 - r1);
            float c1 = distance - c2;
            return new _OverdampedSolution(r1, r2, c1, c2);
        }

        _OverdampedSolution(
            float r1, float r2, float c1, float c2
        ) {
            _r1 = r1;
            _r2 = r2;
            _c1 = c1;
            _c2 = c2;
        }

        readonly float _r1, _r2, _c1, _c2;

        public override float x(float time) {
            return (_c1 * Mathf.Pow((float) Math.E, _r1 * time) +
                    _c2 * Mathf.Pow((float) Math.E, _r2 * time));
        }

        public override float dx(float time) {
            return (_c1 * _r1 * Mathf.Pow((float) Math.E, _r1 * time) +
                    _c2 * _r2 * Mathf.Pow((float) Math.E, _r2 * time));
        }

        public override SpringType type {
            get { return SpringType.overDamped; }
        }
    }

    class _UnderdampedSolution : _SpringSolution {
        internal new static _UnderdampedSolution create(
            SpringDescription spring,
            float distance,
            float velocity
        ) {
            float w = Mathf.Sqrt(4.0f * spring.mass * spring.stiffness -
                                 spring.damping * spring.damping) / (2.0f * spring.mass);
            float r = -(spring.damping / 2.0f * spring.mass);
            float c1 = distance;
            float c2 = (velocity - r * distance) / w;
            return new _UnderdampedSolution(w, r, c1, c2);
        }

        _UnderdampedSolution(
            float w, float r, float c1, float c2
        ) {
            _w = w;
            _r = r;
            _c1 = c1;
            _c2 = c2;
        }

        readonly float _w, _r, _c1, _c2;

        public override float x(float time) {
            return (Mathf.Pow((float) Math.E, _r * time) *
                    (_c1 * Mathf.Cos(_w * time) + _c2 * Mathf.Sin(_w * time)));
        }

        public override float dx(float time) {
            float power = Mathf.Pow((float) Math.E, _r * time);
            float cosine = Mathf.Cos(_w * time);
            float sine = Mathf.Sin(_w * time);
            return (power * (_c2 * _w * cosine - _c1 * _w * sine) +
                    _r * power * (_c2 * sine + _c1 * cosine));
        }

        public override SpringType type {
            get { return SpringType.underDamped; }
        }
    }
}