using System;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.physics;
using Unity.UIWidgets.ui;
using UnityEngine;

namespace Unity.UIWidgets.widgets {
    public class BouncingScrollSimulation : Simulation {
        public BouncingScrollSimulation(
            float position,
            float velocity,
            float leadingExtent,
            float trailingExtent,
            SpringDescription spring,
            Tolerance tolerance = null
        ) : base(tolerance: tolerance) {
            D.assert(leadingExtent <= trailingExtent);
            D.assert(spring != null);

            this.leadingExtent = leadingExtent;
            this.trailingExtent = trailingExtent;
            this.spring = spring;

            if (position < leadingExtent) {
                _springSimulation = _underscrollSimulation(position, velocity);
                _springTime = float.NegativeInfinity;
            }
            else if (position > trailingExtent) {
                _springSimulation = _overscrollSimulation(position, velocity);
                _springTime = float.NegativeInfinity;
            }
            else {
                _frictionSimulation = new FrictionSimulation(0.135f, position, velocity);
                float finalX = _frictionSimulation.finalX;
                if (velocity > 0.0f && finalX > trailingExtent) {
                    _springTime = _frictionSimulation.timeAtX(trailingExtent);
                    _springSimulation = _overscrollSimulation(
                        trailingExtent,
                        Mathf.Min(_frictionSimulation.dx(_springTime),
                            maxSpringTransferVelocity)
                    );
                    D.assert(_springTime.isFinite());
                }
                else if (velocity < 0.0f && finalX < leadingExtent) {
                    _springTime = _frictionSimulation.timeAtX(leadingExtent);
                    _springSimulation = _underscrollSimulation(
                        leadingExtent,
                        Mathf.Min(_frictionSimulation.dx(_springTime),
                            maxSpringTransferVelocity)
                    );
                    D.assert(_springTime.isFinite());
                }
                else {
                    _springTime = float.PositiveInfinity;
                }
            }
        }

        const float maxSpringTransferVelocity = 5000.0f;

        public readonly float leadingExtent;

        public readonly float trailingExtent;

        public readonly SpringDescription spring;

        readonly FrictionSimulation _frictionSimulation;
        readonly Simulation _springSimulation;
        readonly float _springTime;
        float _timeOffset = 0.0f;

        Simulation _underscrollSimulation(float x, float dx) {
            return new ScrollSpringSimulation(spring, x, leadingExtent, dx);
        }

        Simulation _overscrollSimulation(float x, float dx) {
            return new ScrollSpringSimulation(spring, x, trailingExtent, dx);
        }

        Simulation _simulation(float time) {
            Simulation simulation;
            if (time > _springTime) {
                _timeOffset = _springTime.isFinite() ? _springTime : 0.0f;
                simulation = _springSimulation;
            }
            else {
                _timeOffset = 0.0f;
                simulation = _frictionSimulation;
            }

            simulation.tolerance = tolerance;
            return simulation;
        }

        public override float x(float time) {
            return _simulation(time).x(time - _timeOffset);
        }

        public override float dx(float time) {
            return _simulation(time).dx(time - _timeOffset);
        }

        public override bool isDone(float time) {
            return _simulation(time).isDone(time - _timeOffset);
        }

        public override string ToString() {
            return $"{GetType()}(leadingExtent: {leadingExtent}, trailingExtent: {trailingExtent})";
        }
    }

    public class ClampingScrollSimulation : Simulation {
        public ClampingScrollSimulation(
            float position,
            float velocity,
            float friction = 0.015f,
            Tolerance tolerance = null
        ) : base(tolerance: tolerance) {
            D.assert(_flingVelocityPenetration(0.0f) == _initialVelocityPenetration);
            this.position = position;
            this.velocity = velocity;
            this.friction = friction;

            _duration = _flingDuration(velocity);
            _distance = (velocity * _duration / _initialVelocityPenetration).abs();
        }

        public readonly float position;

        public readonly float velocity;

        public readonly float friction;

        readonly float _duration;

        readonly float _distance;

        static readonly float _kDecelerationRate = Mathf.Log(0.78f) / Mathf.Log(0.9f);

        static float _decelerationForFriction(float friction) {
            return friction * 61774.04968f;
        }

        float _flingDuration(float velocity) {
            float scaledFriction = friction * _decelerationForFriction(0.84f);

            float deceleration = Mathf.Log(0.35f * velocity.abs() / scaledFriction);

            return Mathf.Exp(deceleration / (_kDecelerationRate - 1.0f));
        }

        const float _initialVelocityPenetration = 3.065f;

        static float _flingDistancePenetration(float t) {
            return (1.2f * t * t * t) - (3.27f * t * t) + (_initialVelocityPenetration * t);
        }

        static float _flingVelocityPenetration(float t) {
            return (3.6f * t * t) - (6.54f * t) + _initialVelocityPenetration;
        }

        public override float x(float time) {
            float t = (time / _duration).clamp(0.0f, 1.0f);
            return (position + _distance * _flingDistancePenetration(t) *
                            velocity.sign());
        }

        public override float dx(float time) {
            float t = (time / _duration).clamp(0.0f, 1.0f);
            return (_distance * _flingVelocityPenetration(t) * velocity.sign() /
                            _duration);
        }

        public override bool isDone(float time) {
            return time >= _duration;
        }
    }
}