using UnityEngine;

namespace Unity.UIWidgets.scheduler {
    public class Priority {
        Priority(int value) {
            _value = value;
        }

        public int value => _value;
        int _value;

        public static readonly Priority idle = new Priority(0);

        public static readonly Priority animation = new Priority(100000);

        public static readonly Priority touch = new Priority(200000);

        public static readonly int kMaxOffset = 10000;

        public static Priority operator +(Priority it, int offset) {
            if (Mathf.Abs(offset) > kMaxOffset) {
                offset = kMaxOffset * (int) Mathf.Sign(offset);
            }

            return new Priority(it._value + offset);
        }

        public static Priority operator -(Priority it, int offset) => it + (-offset);
    }
}