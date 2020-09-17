using System.Collections.Generic;
using Unity.UIWidgets.foundation;

namespace Unity.UIWidgets.gestures {
    class _CombiningGestureArenaEntry : GestureArenaEntry {
        public _CombiningGestureArenaEntry(_CombiningGestureArenaMember _combiner, GestureArenaMember _member) {
            this._combiner = _combiner;
            this._member = _member;
        }

        readonly _CombiningGestureArenaMember _combiner;
        readonly GestureArenaMember _member;

        public override void resolve(GestureDisposition disposition) {
            _combiner._resolve(_member, disposition);
        }
    }

    class _CombiningGestureArenaMember : GestureArenaMember {
        public _CombiningGestureArenaMember(GestureArenaTeam _owner, int _pointer) {
            this._owner = _owner;
            this._pointer = _pointer;
        }

        readonly GestureArenaTeam _owner;
        readonly List<GestureArenaMember> _members = new List<GestureArenaMember>();
        readonly int _pointer;

        bool _resolved = false;
        GestureArenaMember _winner;
        GestureArenaEntry _entry;

        public void acceptGesture(int pointer) {
            D.assert(_pointer == pointer);
            D.assert(_winner != null || _members.isNotEmpty());

            _close();
            _winner = _winner ?? _owner.captain ?? _members[0];

            foreach (GestureArenaMember member in _members) {
                if (member != _winner) {
                    member.rejectGesture(pointer);
                }
            }

            _winner.acceptGesture(pointer);
        }

        public void rejectGesture(int pointer) {
            D.assert(_pointer == pointer);

            _close();
            foreach (GestureArenaMember member in _members) {
                member.rejectGesture(pointer);
            }
        }

        void _close() {
            D.assert(!_resolved);
            _resolved = true;

            var combiner = _owner._combiners[_pointer];
            D.assert(combiner == this);

            _owner._combiners.Remove(_pointer);
        }

        internal GestureArenaEntry _add(int pointer, GestureArenaMember member) {
            D.assert(!_resolved);
            D.assert(_pointer == pointer);

            _members.Add(member);
            _entry = _entry ?? GestureBinding.instance.gestureArena.add(pointer, this);
            return new _CombiningGestureArenaEntry(this, member);
        }

        internal void _resolve(GestureArenaMember member, GestureDisposition disposition) {
            if (_resolved) {
                return;
            }

            if (disposition == GestureDisposition.rejected) {
                _members.Remove(member);
                member.rejectGesture(_pointer);
                if (_members.isEmpty()) {
                    _entry.resolve(disposition);
                }
            }
            else {
                D.assert(disposition == GestureDisposition.accepted);
                _winner = _winner ?? _owner.captain ?? member;
                _entry.resolve(disposition);
            }
        }
    }

    public class GestureArenaTeam {
        internal readonly Dictionary<int, _CombiningGestureArenaMember> _combiners =
            new Dictionary<int, _CombiningGestureArenaMember>();

        public GestureArenaMember captain;

        public GestureArenaEntry add(int pointer, GestureArenaMember member) {
            _CombiningGestureArenaMember combiner;

            if (!_combiners.TryGetValue(pointer, out combiner)) {
                combiner = new _CombiningGestureArenaMember(this, pointer);
                _combiners[pointer] = combiner;
            }

            return combiner._add(pointer, member);
        }
    }
}