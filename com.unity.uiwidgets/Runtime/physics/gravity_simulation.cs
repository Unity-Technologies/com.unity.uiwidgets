using Unity.UIWidgets.foundation;
using Unity.UIWidgets.ui;

namespace Unity.UIWidgets.physics {
    public class GravitySimulation : Simulation {
        public GravitySimulation(
            float acceleration,
            float distance,
            float endDistance,
            float velocity
        ) {
            D.assert(endDistance >= 0);
            _a = acceleration;
            _x = distance;
            _v = velocity;
            _end = endDistance;
        }

        readonly float _x;
        readonly float _v;
        readonly float _a;
        readonly float _end;

        public override float x(float time) {
            return _x + _v * time + 0.5f * _a * time * time;
        }

        public override float dx(float time) {
            return _v + time * _a;
        }

        public override bool isDone(float time) {
            return x(time).abs() >= _end;
        }
    }
}