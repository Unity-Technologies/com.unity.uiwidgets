using System;
using System.Collections.Generic;
using Unity.UIWidgets.ui;
using UnityEngine;
using Canvas = Unity.UIWidgets.ui.Canvas;
using Rect = Unity.UIWidgets.ui.Rect;

namespace Unity.UIWidgets.painting {
    public class BeveledRectangleBorder : ShapeBorder, IEquatable<BeveledRectangleBorder> {
        public BeveledRectangleBorder(
            BorderSide side = null,
            BorderRadius borderRadius = null
        ) {
            this.side = side ?? BorderSide.none;
            this.borderRadius = borderRadius ?? BorderRadius.zero;
        }

        public readonly BorderSide side;

        public readonly BorderRadius borderRadius;

        public override EdgeInsetsGeometry dimensions {
            get { return EdgeInsets.all(side.width); }
        }

        public override ShapeBorder scale(float t) {
            return new BeveledRectangleBorder(
                side: side.scale(t),
                borderRadius: (BorderRadius) (borderRadius * t)
            );
        }

        public override ShapeBorder lerpFrom(ShapeBorder a, float t) {
            if (a is BeveledRectangleBorder border) {
                return new BeveledRectangleBorder(
                    side: BorderSide.lerp(border.side, side, t),
                    borderRadius: BorderRadius.lerp(border.borderRadius, borderRadius, t)
                );
            }

            return base.lerpFrom(a, t);
        }

        public override ShapeBorder lerpTo(ShapeBorder b, float t) {
            if (b is BeveledRectangleBorder border) {
                return new BeveledRectangleBorder(
                    side: BorderSide.lerp(side, border.side, t),
                    borderRadius: BorderRadius.lerp(borderRadius, border.borderRadius, t)
                );
            }

            return base.lerpTo(b, t);
        }

        Path _getPath(RRect rrect) {
            Offset centerLeft = new Offset(rrect.left, rrect.center.dy);
            Offset centerRight = new Offset(rrect.right, rrect.center.dy);
            Offset centerTop = new Offset(rrect.center.dx, rrect.top);
            Offset centerBottom = new Offset(rrect.center.dx, rrect.bottom);

            float tlRadiusX = Mathf.Max(0.0f, rrect.tlRadiusX);
            float tlRadiusY = Mathf.Max(0.0f, rrect.tlRadiusY);
            float trRadiusX = Mathf.Max(0.0f, rrect.trRadiusX);
            float trRadiusY = Mathf.Max(0.0f, rrect.trRadiusY);
            float blRadiusX = Mathf.Max(0.0f, rrect.blRadiusX);
            float blRadiusY = Mathf.Max(0.0f, rrect.blRadiusY);
            float brRadiusX = Mathf.Max(0.0f, rrect.brRadiusX);
            float brRadiusY = Mathf.Max(0.0f, rrect.brRadiusY);

            List<Offset> vertices = new List<Offset> {
                new Offset(rrect.left, Mathf.Min(centerLeft.dy, rrect.top + tlRadiusY)),
                new Offset(Mathf.Min(centerTop.dx, rrect.left + tlRadiusX), rrect.top),
                new Offset(Mathf.Max(centerTop.dx, rrect.right - trRadiusX), rrect.top),
                new Offset(rrect.right, Mathf.Min(centerRight.dy, rrect.top + trRadiusY)),
                new Offset(rrect.right, Mathf.Max(centerRight.dy, rrect.bottom - brRadiusY)),
                new Offset(Mathf.Max(centerBottom.dx, rrect.right - brRadiusX), rrect.bottom),
                new Offset(Mathf.Min(centerBottom.dx, rrect.left + blRadiusX), rrect.bottom),
                new Offset(rrect.left, Mathf.Max(centerLeft.dy, rrect.bottom - blRadiusY)),
            };

            var path = new Path();
            path.addPolygon(vertices, true);
            return path;
        }

        public override Path getInnerPath(Rect rect, TextDirection? textDirection = null) {
            return _getPath(borderRadius.resolve(textDirection).toRRect(rect).deflate(side.width));
        }

        public override Path getOuterPath(Rect rect, TextDirection? textDirection = null) {
            return _getPath(borderRadius.resolve(textDirection).toRRect(rect));
        }

        public override void paint(Canvas canvas, Rect rect, TextDirection? textDirection = null) {
            if (rect.isEmpty) {
                return;
            }

            switch (side.style) {
                case BorderStyle.none:
                    break;
                case BorderStyle.solid:
                    Path path = getOuterPath(rect, textDirection);
                    path.addPath(getInnerPath(rect, textDirection), Offset.zero);
                    canvas.drawPath(path, side.toPaint());
                    break;
            }
        }

        public bool Equals(BeveledRectangleBorder other) {
            if (ReferenceEquals(null, other)) {
                return false;
            }

            if (ReferenceEquals(this, other)) {
                return true;
            }

            return Equals(side, other.side) && Equals(borderRadius, other.borderRadius);
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

            return Equals((BeveledRectangleBorder) obj);
        }

        public override int GetHashCode() {
            unchecked {
                return ((side != null ? side.GetHashCode() : 0) * 397) ^
                       (borderRadius != null ? borderRadius.GetHashCode() : 0);
            }
        }

        public static bool operator ==(BeveledRectangleBorder left, BeveledRectangleBorder right) {
            return Equals(left, right);
        }

        public static bool operator !=(BeveledRectangleBorder left, BeveledRectangleBorder right) {
            return !Equals(left, right);
        }

        public override string ToString() {
            return $"{GetType()}({side}, {borderRadius})";
        }
    }
}