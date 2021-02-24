using System;
using System.Collections.Generic;
using Unity.UIWidgets.ui;
using UnityEngine;
using Color = Unity.UIWidgets.ui.Color;

namespace Unity.UIWidgets.painting {
    public class BoxShadow : IEquatable<BoxShadow> {
        public BoxShadow(
            Color color = null,
            Offset offset = null,
            float blurRadius = 0.0f,
            float spreadRadius = 0.0f
        ) {
            this.color = color ?? Color.black;
            this.offset = offset ?? Offset.zero;
            this.blurRadius = blurRadius;
            this.spreadRadius = spreadRadius;
        }

        public readonly Color color;
        public readonly Offset offset;
        public readonly float blurRadius;
        public readonly float spreadRadius;

        public static float convertRadiusToSigma(float radius) {
            return radius * 0.57735f + 0.5f;
        }

        public float blurSigma {
            get { return convertRadiusToSigma(blurRadius); }
        }

        public Paint toPaint() {
            return new Paint {
                color = color,
                maskFilter = MaskFilter.blur(BlurStyle.normal, blurSigma)
            };
        }

        public BoxShadow scale(float factor) {
            return new BoxShadow(
                color: color,
                offset: offset * factor,
                blurRadius: blurRadius * factor,
                spreadRadius: spreadRadius * factor
            );
        }

        public static BoxShadow lerp(BoxShadow a, BoxShadow b, float t) {
            if (a == null && b == null) {
                return null;
            }

            if (a == null) {
                return b.scale(t);
            }

            if (b == null) {
                return a.scale(1.0f - t);
            }

            return new BoxShadow(
                color: Color.lerp(a.color, b.color, t),
                offset: Offset.lerp(a.offset, b.offset, t),
                blurRadius: MathUtils.lerpNullableFloat(a.blurRadius, b.blurRadius, t),
                spreadRadius: MathUtils.lerpNullableFloat(a.spreadRadius, b.spreadRadius, t)
            );
        }

        public static List<BoxShadow> lerpList(List<BoxShadow> a, List<BoxShadow> b, float t) {
            if (a == null && b == null) {
                return null;
            }

            a = a ?? new List<BoxShadow>();
            b = b ?? new List<BoxShadow>();
            List<BoxShadow> result = new List<BoxShadow>();
            int commonLength = Mathf.Min(a.Count, b.Count);
            for (int i = 0; i < commonLength; i += 1) {
                result.Add(lerp(a[i], b[i], t));
            }

            for (int i = commonLength; i < a.Count; i += 1) {
                result.Add(a[i].scale(1.0f - t));
            }

            for (int i = commonLength; i < b.Count; i += 1) {
                result.Add(b[i].scale(t));
            }

            return result;
        }

        public bool Equals(BoxShadow other) {
            if (ReferenceEquals(null, other)) {
                return false;
            }

            if (ReferenceEquals(this, other)) {
                return true;
            }

            return Equals(color, other.color)
                   && Equals(offset, other.offset)
                   && blurRadius.Equals(other.blurRadius)
                   && spreadRadius.Equals(other.spreadRadius);
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

            return Equals((BoxShadow) obj);
        }

        public override int GetHashCode() {
            unchecked {
                var hashCode = (color != null ? color.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (offset != null ? offset.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ blurRadius.GetHashCode();
                hashCode = (hashCode * 397) ^ spreadRadius.GetHashCode();
                return hashCode;
            }
        }

        public static bool operator ==(BoxShadow a, BoxShadow b) {
            return Equals(a, b);
        }

        public static bool operator !=(BoxShadow a, BoxShadow b) {
            return !(a == b);
        }

        public override string ToString() {
            return $"BoxShadow({color}, {offset}, {blurRadius}, {spreadRadius})";
        }
    }
}