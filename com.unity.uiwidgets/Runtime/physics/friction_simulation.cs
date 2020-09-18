using System;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.ui;
using UnityEngine;

namespace Unity.UIWidgets.physics {
    public class FrictionSimulation : Simulation {
        public FrictionSimulation(
            float drag, float position, float velocity,
            Tolerance tolerance = null
        ) : base(tolerance: tolerance) {
            _drag = drag;
            _dragLog = Mathf.Log(drag);
            _x = position;
            _v = velocity;
        }

        public static FrictionSimulation through(float startPosition, float endPosition, float startVelocity,
            float endVelocity) {
            D.assert(startVelocity == 0.0 || endVelocity == 0.0 || startVelocity.sign() == endVelocity.sign());
            D.assert(startVelocity.abs() >= endVelocity.abs());
            D.assert((endPosition - startPosition).sign() == startVelocity.sign());

            return new FrictionSimulation(
                _dragFor(startPosition, endPosition, startVelocity, endVelocity),
                startPosition,
                startVelocity,
                tolerance: new Tolerance(velocity: endVelocity.abs())
            );
        }

        readonly float _drag;
        readonly float _dragLog;
        readonly float _x;
        readonly float _v;

        static float _dragFor(float startPosition, float endPosition, float startVelocity, float endVelocity) {
            return Mathf.Pow((float) Math.E, (startVelocity - endVelocity) / (startPosition - endPosition));
        }

        public override float x(float time) {
            return _x + _v * Mathf.Pow(_drag, time) / _dragLog - _v / _dragLog;
        }

        public override float dx(float time) {
            return _v * Mathf.Pow(_drag, time);
        }

        public float finalX {
            get { return _x - _v / _dragLog; }
        }

        public float timeAtX(float x) {
            if (x == _x) {
                return 0.0f;
            }

            if (_v == 0.0 || (_v > 0 ? (x < _x || x > finalX) : (x > _x || x < finalX))) {
                return float.PositiveInfinity;
            }

            return Mathf.Log(_dragLog * (x - _x) / _v + 1.0f) / _dragLog;
        }

        public override bool isDone(float time) {
            return dx(time).abs() < tolerance.velocity;
        }
    }

    public class BoundedFrictionSimulation : FrictionSimulation {
        BoundedFrictionSimulation(
            float drag,
            float position,
            float velocity,
            float _minX,
            float _maxX
        ) : base(drag, position, velocity) {
            D.assert(position.clamp(_minX, _maxX) == position);
            this._minX = _minX;
            this._maxX = _maxX;
        }

        readonly float _minX;

        readonly float _maxX;

        public override float x(float time) {
            return base.x(time).clamp(_minX, _maxX);
        }

        public override bool isDone(float time) {
            return base.isDone(time) ||
                   (x(time) - _minX).abs() < tolerance.distance ||
                   (x(time) - _maxX).abs() < tolerance.distance;
        }
    }
}