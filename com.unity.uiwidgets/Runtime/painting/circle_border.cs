using System;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.ui;
using UnityEngine;
using Canvas = Unity.UIWidgets.ui.Canvas;
using Rect = Unity.UIWidgets.ui.Rect;

namespace Unity.UIWidgets.painting {
    public class CircleBorder : ShapeBorder, IEquatable<CircleBorder> {
        public CircleBorder(BorderSide side = null) {
            this.side = side ?? BorderSide.none;
        }

        public readonly BorderSide side;

        public override EdgeInsetsGeometry dimensions {
            get { return EdgeInsets.all(side.width); }
        }

        public override ShapeBorder scale(float t) {
            return new CircleBorder(side: side.scale(t));
        }

        public override ShapeBorder lerpFrom(ShapeBorder a, float t) {
            if (a is CircleBorder border) {
                return new CircleBorder(side: BorderSide.lerp(border.side, side, t));
            }

            return base.lerpFrom(a, t);
        }

        public override ShapeBorder lerpTo(ShapeBorder b, float t) {
            if (b is CircleBorder border) {
                return new CircleBorder(side: BorderSide.lerp(side, border.side, t));
            }

            return base.lerpTo(b, t);
        }

        public override Path getInnerPath(Rect rect, TextDirection? textDirection = null) {
            var path = new Path();
            path.addOval(Rect.fromCircle(
                center: rect.center,
                radius: Mathf.Max(0.0f, rect.shortestSide / 2.0f - side.width)
            ));
            return path;
        }

        public override Path getOuterPath(Rect rect, TextDirection? textDirection = null) {
            var path = new Path();
            path.addOval(Rect.fromCircle(
                center: rect.center,
                radius: rect.shortestSide / 2.0f
            ));
            return path;
        }

        public override void paint(Canvas canvas, Rect rect, TextDirection? textDirection = null) {
            switch (side.style) {
                case BorderStyle.none:
                    break;
                case BorderStyle.solid:
                    canvas.drawCircle(rect.center, (rect.shortestSide - side.width) / 2.0f, side.toPaint());
                    break;
            }
        }

        public bool Equals(CircleBorder other) {
            if (ReferenceEquals(null, other)) {
                return false;
            }

            if (ReferenceEquals(this, other)) {
                return true;
            }

            return Equals(side, other.side);
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

            return Equals((CircleBorder) obj);
        }

        public override int GetHashCode() {
            return (side != null ? side.GetHashCode() : 0);
        }

        public static bool operator ==(CircleBorder left, CircleBorder right) {
            return Equals(left, right);
        }

        public static bool operator !=(CircleBorder left, CircleBorder right) {
            return !Equals(left, right);
        }

        public override string ToString() {
            return $"{foundation_.objectRuntimeType(this, "CircleBorder")}({side})";
        }
    }
}