using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.ui;
using UnityEngine;

namespace Unity.UIWidgets.gestures {
    public enum GestureDisposition {
        accepted,
        rejected,
    }

    public interface GestureArenaMember {
        void acceptGesture(int pointer);
        void rejectGesture(int pointer);
    }

    public class GestureArenaEntry {
        public GestureArenaEntry(
            GestureArenaManager arena = null,
            int pointer = 0,
            GestureArenaMember member = null) {
            _arena = arena;
            _pointer = pointer;
            _member = member;
        }

        readonly GestureArenaManager _arena;
        readonly int _pointer;
        readonly GestureArenaMember _member;

        public virtual void resolve(GestureDisposition disposition) {
            _arena._resolve(_pointer, _member, disposition);
        }
    }

    class _GestureArena {
        public readonly List<GestureArenaMember> members = new List<GestureArenaMember>();
        public bool isOpen = true;
        public bool isHeld = false;
        public bool hasPendingSweep = false;

        public GestureArenaMember eagerWinner;

        public void add(GestureArenaMember member) {
            D.assert(isOpen);
            members.Add(member);
        }

        public override string ToString() {
            StringBuilder buffer = new StringBuilder();
            if (members.isEmpty()) {
                buffer.Append("<empty>");
            }
            else {
                buffer.Append(string.Join(", ", members.Select(
                    member => member == eagerWinner
                        ? $"{member} (eager winner)"
                        : member.ToString()).ToArray()));
            }

            if (isOpen) {
                buffer.Append(" [open]");
            }

            if (isHeld) {
                buffer.Append(" [held]");
            }

            if (hasPendingSweep) {
                buffer.Append(" [hasPendingSweep]");
            }

            return buffer.ToString();
        }
    }

    public class GestureArenaManager {
        readonly Dictionary<int, _GestureArena> _arenas = new Dictionary<int, _GestureArena>();

        public GestureArenaEntry add(int pointer, GestureArenaMember member) {
            _GestureArena state = _arenas.putIfAbsent(pointer, () => {
                D.assert(_debugLogDiagnostic(pointer, () => "★ Opening new gesture arena."));
                return state = new _GestureArena();
            });

            state.add(member);

            D.assert(_debugLogDiagnostic(pointer, () => $"Adding: {member}"));
            return new GestureArenaEntry(this, pointer, member);
        }

        public void close(int pointer) {
            _GestureArena state;
            if (!_arenas.TryGetValue(pointer, out state)) {
                return;
            }

            state.isOpen = false;
            D.assert(_debugLogDiagnostic(pointer, () => "Closing", state));
            _tryToResolveArena(pointer, state);
        }

        public void sweep(int pointer) {
            _GestureArena state;
            if (!_arenas.TryGetValue(pointer, out state)) {
                return;
            }

            D.assert(!state.isOpen);
            if (state.isHeld) {
                state.hasPendingSweep = true;
                D.assert(_debugLogDiagnostic(pointer, () => "Delaying sweep", state));
                return;
            }

            D.assert(_debugLogDiagnostic(pointer, () => "Sweeping", state));
            _arenas.Remove(pointer);
            if (state.members.isNotEmpty()) {
                D.assert(_debugLogDiagnostic(
                    pointer, () => $"Winner: {state.members.First()}"));

                state.members[0].acceptGesture(pointer);
                for (int i = 1; i < state.members.Count; i++) {
                    state.members[i].rejectGesture(pointer);
                }
            }
        }

        public void hold(int pointer) {
            _GestureArena state;
            if (!_arenas.TryGetValue(pointer, out state)) {
                return;
            }

            state.isHeld = true;
            D.assert(_debugLogDiagnostic(pointer, () => "Holding", state));
        }

        public void release(int pointer) {
            _GestureArena state;
            if (!_arenas.TryGetValue(pointer, out state)) {
                return;
            }

            state.isHeld = false;
            D.assert(_debugLogDiagnostic(pointer, () => "Releasing", state));
            if (state.hasPendingSweep) {
                sweep(pointer);
            }
        }

        internal void _resolve(int pointer, GestureArenaMember member, GestureDisposition disposition) {
            _GestureArena state;
            if (!_arenas.TryGetValue(pointer, out state)) {
                return;
            }

            D.assert(_debugLogDiagnostic(pointer,
                () => $"{(disposition == GestureDisposition.accepted ? "Accepting" : "Rejecting")}: {member}"));

            D.assert(state.members.Contains(member));
            if (disposition == GestureDisposition.rejected) {
                state.members.Remove(member);
                member.rejectGesture(pointer);
                if (!state.isOpen) {
                    _tryToResolveArena(pointer, state);
                }
            }
            else {
                if (state.isOpen) {
                    state.eagerWinner = state.eagerWinner ?? member;
                }
                else {
                    D.assert(_debugLogDiagnostic(pointer,
                        () => $"Self-declared winner: {member}"));
                    _resolveInFavorOf(pointer, state, member);
                }
            }
        }

        void _tryToResolveArena(int pointer, _GestureArena state) {
            D.assert(_arenas[pointer] == state);
            D.assert(!state.isOpen);

            if (state.members.Count == 1) {
                Window.instance.scheduleMicrotask(() => _resolveByDefault(pointer, state));
            }
            else if (state.members.isEmpty()) {
                _arenas.Remove(pointer);
                D.assert(_debugLogDiagnostic(pointer, () => "Arena empty."));
            }
            else if (state.eagerWinner != null) {
                D.assert(_debugLogDiagnostic(pointer,
                    () => $"Eager winner: {state.eagerWinner}"));
                _resolveInFavorOf(pointer, state, state.eagerWinner);
            }
        }

        void _resolveByDefault(int pointer, _GestureArena state) {
            if (!_arenas.ContainsKey(pointer)) {
                return;
            }

            D.assert(_arenas[pointer] == state);
            D.assert(!state.isOpen);
            D.assert(state.members.Count == 1);
            _arenas.Remove(pointer);
            D.assert(_debugLogDiagnostic(pointer,
                () => $"Default winner: {state.members.First()}"));
            state.members[0].acceptGesture(pointer);
        }

        void _resolveInFavorOf(int pointer, _GestureArena state, GestureArenaMember member) {
            D.assert(state == _arenas[pointer]);
            D.assert(state != null);
            D.assert(state.eagerWinner == null || state.eagerWinner == member);
            D.assert(!state.isOpen);

            _arenas.Remove(pointer);
            foreach (GestureArenaMember rejectedMember in state.members) {
                if (rejectedMember != member) {
                    rejectedMember.rejectGesture(pointer);
                }
            }

            member.acceptGesture(pointer);
        }

        bool _debugLogDiagnostic(int pointer, Func<string> message, _GestureArena state = null) {
            D.assert(() => {
                if (D.debugPrintGestureArenaDiagnostics) {
                    int? count = state?.members.Count;
                    string s = count != 1 ? "s" : "";
                    Debug.LogFormat("Gesture arena {0} ❙ {1}{2}",
                        pointer.ToString().PadRight(4),
                        message(),
                        count != null ? $" with {count} member{s}." : "");
                }

                return true;
            });

            return true;
        }
    }
}