using System;

namespace Unity.UIWidgets.widgets {
    public class IconData : IEquatable<IconData> {
        public IconData(
            int codePoint,
            string fontFamily = null
        ) {
            this.codePoint = codePoint;
            this.fontFamily = fontFamily;
        }

        public readonly int codePoint;

        public readonly string fontFamily;

        public bool Equals(IconData other) {
            if (ReferenceEquals(null, other)) {
                return false;
            }

            if (ReferenceEquals(this, other)) {
                return true;
            }

            return codePoint == other.codePoint &&
                   string.Equals(fontFamily, other.fontFamily);
        }

        public override bool Equals(object obj) {
            if (ReferenceEquals(null, obj)) {
                return false;
            }

            if (ReferenceEquals(this, obj)) {
                return true;
            }

            if (obj.GetType() != GetType()) {
                return false;
            }

            return Equals((IconData) obj);
        }

        public override int GetHashCode() {
            unchecked {
                return (codePoint * 397) ^ (fontFamily != null ? fontFamily.GetHashCode() : 0);
            }
        }

        public static bool operator ==(IconData left, IconData right) {
            return Equals(left, right);
        }

        public static bool operator !=(IconData left, IconData right) {
            return !Equals(left, right);
        }

        public override string ToString() {
            return "IconData(U+" + codePoint.ToString("X5") + ")";
        }
    }
}