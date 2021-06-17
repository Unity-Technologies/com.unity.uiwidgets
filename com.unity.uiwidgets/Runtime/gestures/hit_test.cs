using System.Collections.Generic;
using System.Linq;
using Unity.UIWidgets.external;
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
            get { return _transform; }
        }

        internal Matrix4 _transform;
    }

    public class HitTestResult {
        public HitTestResult() {
            _path = new List<HitTestEntry>();
            _transforms = new LinkedList<Matrix4>();
        }

        public HitTestResult(HitTestResult result) {
            _path = result._path;
            _transforms = result._transforms;
        }

        public IList<HitTestEntry> path {
            get { return _path.AsReadOnly(); }
        }

        readonly List<HitTestEntry> _path;
        readonly LinkedList<Matrix4> _transforms;

        public void add(HitTestEntry entry) {
            D.assert(entry.transform == null);
            entry._transform = _transforms.isEmpty() ? null : _transforms.Last();
            _path.Add(entry);
        }

        protected void pushTransform(Matrix4 transform) {
            D.assert(transform != null);
            D.assert(
                _debugVectorMoreOrLessEquals(transform.getRow(2), new Vector4(0, 0, 1, 0)) &&
                _debugVectorMoreOrLessEquals(transform.getColumn(2), new Vector4(0, 0, 1, 0))
            );
            _transforms.AddLast(_transforms.isEmpty()
                ? transform
                : (transform * _transforms.Last() as Matrix4));
        }

        protected void popTransform() {
            D.assert(_transforms.isNotEmpty);
            _transforms.RemoveLast();
        }

        bool _debugVectorMoreOrLessEquals(Vector4 a, Vector4 b,
            double epsilon = foundation_.precisionErrorTolerance) {
            bool result = true;
            D.assert(() => {
                Vector4 difference = a - b;
                return difference.SqrMagnitude() < epsilon;
            });
            return result;
        }

        public override string ToString() {
            return
                $"HitTestResult({(_path.isEmpty() ? "<empty path>" : string.Join(", ", LinqUtils<string,HitTestEntry>.SelectList(_path, (x => x.ToString())) ))})";
           }
    }
}