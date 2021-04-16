using System;
using System.Collections.Generic;
using System.Linq;
using Unity.UIWidgets.animation;
using Unity.UIWidgets.async;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.ui;

namespace Unity.UIWidgets.widgets {
    public class ScrollController : ChangeNotifier {
        public ScrollController(
            float initialScrollOffset = 0.0f,
            bool keepScrollOffset = true,
            string debugLabel = null
        ) {
            _initialScrollOffset = initialScrollOffset;
            this.keepScrollOffset = keepScrollOffset;
            this.debugLabel = debugLabel;
        }

        public virtual float initialScrollOffset {
            get { return _initialScrollOffset; }
        }

        readonly float _initialScrollOffset;

        public readonly bool keepScrollOffset;

        public readonly string debugLabel;

        public ICollection<ScrollPosition> positions {
            get { return _positions; }
        }

        readonly List<ScrollPosition> _positions = new List<ScrollPosition>();

        public bool hasClients {
            get { return _positions.isNotEmpty(); }
        }

        public ScrollPosition position {
            get {
                D.assert(_positions.isNotEmpty(), () => "ScrollController not attached to any scroll views.");
                D.assert(_positions.Count == 1, () => "ScrollController attached to multiple scroll views.");
                return _positions.Single();
            }
        }

        public float offset {
            get { return position.pixels; }
        }


        public Future animateTo(float to,
            TimeSpan duration,
            Curve curve
        ) {
            D.assert(_positions.isNotEmpty(), () => "ScrollController not attached to any scroll views.");
            List<Future> animations = CollectionUtils.CreateRepeatedList<Future>(null, _positions.Count);
            for (int i = 0; i < _positions.Count; i += 1) {
                animations[i] = _positions[i].animateTo(to, duration: duration, curve: curve);
            }

            return Future.wait<object>(animations);
        }

        public void jumpTo(float value) {
            D.assert(_positions.isNotEmpty(), () => "ScrollController not attached to any scroll views.");
            foreach (ScrollPosition position in new List<ScrollPosition>(_positions)) {
                position.jumpTo(value);
            }
        }

        public virtual void attach(ScrollPosition position) {
            D.assert(!_positions.Contains(position));
            _positions.Add(position);
            position.addListener(notifyListeners);
        }

        public virtual void detach(ScrollPosition position) {
            D.assert(_positions.Contains(position));
            position.removeListener(notifyListeners);
            _positions.Remove(position);
        }

        public override void dispose() {
            foreach (ScrollPosition position in _positions) {
                position.removeListener(notifyListeners);
            }

            base.dispose();
        }

        public virtual ScrollPosition createScrollPosition(
            ScrollPhysics physics,
            ScrollContext context,
            ScrollPosition oldPosition
        ) {
            return new ScrollPositionWithSingleContext(
                physics: physics,
                context: context,
                initialPixels: initialScrollOffset,
                keepScrollOffset: keepScrollOffset,
                oldPosition: oldPosition,
                debugLabel: debugLabel
            );
        }

        public override string ToString() {
            List<string> description = new List<string>();
            debugFillDescription(description);
            return $"{foundation_.describeIdentity(this)}({string.Join(", ", description.ToArray())})";
        }

        protected virtual void debugFillDescription(List<string> description) {
            if (debugLabel != null) {
                description.Add(debugLabel);
            }

            if (initialScrollOffset != 0.0) {
                description.Add($"initialScrollOffset: {initialScrollOffset:F1}, ");
            }

            if (_positions.isEmpty()) {
                description.Add("no clients");
            }
            else if (_positions.Count == 1) {
                description.Add($"one client, offset {offset:F1}");
            }
            else {
                description.Add(_positions.Count + " clients");
            }
        }
    }

    public class TrackingScrollController : ScrollController {
        public TrackingScrollController(
            float initialScrollOffset = 0.0f,
            bool keepScrollOffset = true,
            string debugLabel = null
        ) : base(initialScrollOffset: initialScrollOffset,
            keepScrollOffset: keepScrollOffset,
            debugLabel: debugLabel) {
        }

        readonly Dictionary<ScrollPosition, VoidCallback> _positionToListener =
            new Dictionary<ScrollPosition, VoidCallback>();

        ScrollPosition _lastUpdated;
        float? _lastUpdatedOffset;

        public ScrollPosition mostRecentlyUpdatedPosition {
            get { return _lastUpdated; }
        }

        public override float initialScrollOffset {
            get { return _lastUpdatedOffset ?? base.initialScrollOffset; }
        }

        public override void attach(ScrollPosition position) {
            base.attach(position);
            D.assert(!_positionToListener.ContainsKey(position));
            _positionToListener[position] = () => {
                _lastUpdated = position;
                _lastUpdatedOffset = position.pixels;
            };
            position.addListener(_positionToListener[position]);
        }

        public override void detach(ScrollPosition position) {
            base.detach(position);
            D.assert(_positionToListener.ContainsKey(position));
            position.removeListener(_positionToListener[position]);
            _positionToListener.Remove(position);
            if (_lastUpdated == position) {
                _lastUpdated = null;
            }

            if (_positionToListener.isEmpty()) {
                _lastUpdatedOffset = null;
            }
        }

        public override void dispose() {
            foreach (ScrollPosition position in positions) {
                D.assert(_positionToListener.ContainsKey(position));
                position.removeListener(_positionToListener[position]);
            }

            base.dispose();
        }
    }
}