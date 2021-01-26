using System;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.ui;

namespace Unity.UIWidgets.service {
    public class TextSelection : TextRange, IEquatable<TextSelection> {
        public readonly int baseOffset;
        public readonly int extentOffset;
        public readonly TextAffinity affinity;
        public readonly bool isDirectional;
        public TextPosition _base {
            get{
                return new TextPosition(offset: baseOffset, affinity: affinity);
            }
        }

        public TextSelection(int baseOffset, int extentOffset, TextAffinity affinity = TextAffinity.downstream,
            bool isDirectional = false) : base(baseOffset < extentOffset ? baseOffset : extentOffset,
            baseOffset < extentOffset ? extentOffset : baseOffset) {
            this.baseOffset = baseOffset;
            this.extentOffset = extentOffset;
            this.affinity = affinity;
            this.isDirectional = isDirectional;
        }

        public static TextSelection collapsed(int offset, TextAffinity affinity = TextAffinity.downstream) {
            return new TextSelection(offset, offset, affinity, false);
        }

        public static TextSelection fromPosition(TextPosition position) {
            return collapsed(position.offset, position.affinity);
        }

        public TextPosition basePos {
            get { return new TextPosition(offset: baseOffset, affinity: affinity); }
        }

        public TextPosition extendPos {
            get { return new TextPosition(offset: extentOffset, affinity: affinity); }
        }

        public TextPosition startPos {
            get { return new TextPosition(offset: start, affinity: affinity); }
        }

        public TextPosition endPos {
            get { return new TextPosition(offset: end, affinity: affinity); }
        }
        
        public TextPosition  extent {
            get { return new TextPosition(offset: extentOffset, affinity: affinity); }
        }

        public bool Equals(TextSelection other) {
            if (ReferenceEquals(null, other)) {
                return false;
            }

            if (ReferenceEquals(this, other)) {
                return true;
            }

            return baseOffset == other.baseOffset && extentOffset == other.extentOffset &&
                   affinity == other.affinity && isDirectional == other.isDirectional;
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

            return Equals((TextSelection) obj);
        }

        public override int GetHashCode() {
            unchecked {
                var hashCode = base.GetHashCode();
                hashCode = (hashCode * 397) ^ baseOffset;
                hashCode = (hashCode * 397) ^ extentOffset;
                hashCode = (hashCode * 397) ^ (int) affinity;
                hashCode = (hashCode * 397) ^ isDirectional.GetHashCode();
                return hashCode;
            }
        }

        public static bool operator ==(TextSelection left, TextSelection right) {
            return Equals(left, right);
        }

        public static bool operator !=(TextSelection left, TextSelection right) {
            return !Equals(left, right);
        }

        public TextSelection copyWith(int? baseOffset = null, int? extentOffset = null, TextAffinity? affinity = null,
            bool? isDirectional = null) {
            return new TextSelection(
                baseOffset ?? this.baseOffset, extentOffset ?? this.extentOffset, affinity ?? this.affinity,
                isDirectional ?? this.isDirectional
            );
        }

        public override string ToString() {
            return
                $"{base.ToString()}, BaseOffset: {baseOffset}, ExtentOffset: {extentOffset}, Affinity: {affinity}, IsDirectional: {isDirectional}";
        }
    }
}