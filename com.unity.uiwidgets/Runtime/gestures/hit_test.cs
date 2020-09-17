using System.Collections.Generic;
using System.Linq;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.ui;

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
    }

    public class HitTestResult {
        public HitTestResult(List<HitTestEntry> path = null) {
            _path = path ?? new List<HitTestEntry>();
        }

        public IList<HitTestEntry> path {
            get { return _path.AsReadOnly(); }
        }

        readonly List<HitTestEntry> _path;

        public void add(HitTestEntry entry) {
            _path.Add(entry);
        }

        public override string ToString() {
            return
                $"HitTestResult({(_path.isEmpty() ? "<empty path>" : string.Join(", ", _path.Select(x => x.ToString()).ToArray()))})";
        }
    }
}