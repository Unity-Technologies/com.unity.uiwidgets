using System.Collections.Generic;
using System.Linq;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.rendering;
using Unity.UIWidgets.ui;
using UnityEngine;

namespace Unity.UIWidgets.gestures {
    public interface HitTestable {
        void hitTest(HitTestResult result, Offset position);
    }

    public interface HitTestDispatcher {
        void dispatchEvent(PointerEvent evt, HitTestResult hitTestResult);
    }

    public interface HitTestTarget {
        void handleEvent(PointerEvent evt, HitTestEntry entry);
    }

    public class HitTestEntry {
        public HitTestEntry(HitTestTarget target) {
            _target = target;
        }

        public virtual HitTestTarget target {
            get { return _target; }
        }

        readonly HitTestTarget _target;

        public override string ToString() {
            return _target.ToString();
        }

        public Matrix4 transform {
            get { return this._transform; }
        }

        internal Matrix4 _transform;
    }

    public class HitTestResult {
        public HitTestResult() {
            this._path = new List<HitTestEntry>();
            this._transforms = new LinkedList<Matrix4>();
        }

        public HitTestResult(HitTestResult result) {
            this._path = result._path;
            this._transforms = result._transforms;
        }

        public IList<HitTestEntry> path {
            get { return _path.AsReadOnly(); }
        }

        readonly List<HitTestEntry> _path;
        readonly LinkedList<Matrix4> _transforms;

        public void add(HitTestEntry entry) {
            D.assert(entry.transform == null);
            entry._transform = this._transforms.isEmpty() ? null : this._transforms.Last();
            this._path.Add(entry);
        }

        protected void pushTransform(Matrix4 transform) {
            D.assert(transform != null);
            D.assert(
                this._debugVectorMoreOrLessEquals(transform.getRow(2), new Vector4(0, 0, 1, 0)) &&
                this._debugVectorMoreOrLessEquals(transform.getColumn(2), new Vector4(0, 0, 1, 0))
            );
            this._transforms.AddLast(this._transforms.isEmpty()
                ? transform
                : (transform * this._transforms.Last() as Matrix4));
        }

        protected void popTransform() {
            D.assert(this._transforms.isNotEmpty);
            this._transforms.RemoveLast();
        }

        bool _debugVectorMoreOrLessEquals(Vector4 a, Vector4 b,
            double epsilon = SliverGeometry.precisionErrorTolerance) {
            bool result = true;
            D.assert(() => {
                Vector4 difference = a - b;
                return difference.SqrMagnitude() < epsilon;
            });
            return result;
        }

        public override string ToString() {
            return
                $"HitTestResult({(_path.isEmpty() ? "<empty path>" : string.Join(", ", _path.Select(x => x.ToString()).ToArray()))})";
        }
    }
}