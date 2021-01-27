using System;
using System.Collections.Generic;
using System.Linq;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.ui;

namespace Unity.UIWidgets.rendering {
    public class TableBorder : IEquatable<TableBorder> {
        public TableBorder(
            BorderSide top = null,
            BorderSide right = null,
            BorderSide bottom = null,
            BorderSide left = null,
            BorderSide horizontalInside = null,
            BorderSide verticalInside = null
        ) {
            this.top = top ?? BorderSide.none;
            this.right = right ?? BorderSide.none;
            this.bottom = bottom ?? BorderSide.none;
            this.left = left ?? BorderSide.none;
            this.horizontalInside = horizontalInside ?? BorderSide.none;
            this.verticalInside = verticalInside ?? BorderSide.none;
        }

        public static TableBorder all(
            Color color = null,
            float width = 1.0f,
            BorderStyle style = BorderStyle.solid
        ) {
            color = color ?? new Color(0xFF000000);
            BorderSide side = new BorderSide(color: color, width: width, style: style);
            return new TableBorder(
                top: side, right: side, bottom: side, left: side, horizontalInside: side, verticalInside: side);
        }

        public static TableBorder symmetric(
            BorderSide inside = null,
            BorderSide outside = null
        ) {
            inside = inside ?? BorderSide.none;
            outside = outside ?? BorderSide.none;
            return new TableBorder(
                top: outside,
                right: outside,
                bottom: outside,
                left: outside,
                horizontalInside: inside,
                verticalInside: inside
            );
        }

        public readonly BorderSide top;

        public readonly BorderSide right;

        public readonly BorderSide bottom;

        public readonly BorderSide left;

        public readonly BorderSide horizontalInside;

        public readonly BorderSide verticalInside;

        public EdgeInsets dimensions {
            get {
                return EdgeInsets.fromLTRB(
                    left.width,
                    top.width,
                    right.width,
                    bottom.width
                );
            }
        }

        public bool isUniform {
            get {
                D.assert(top != null);
                D.assert(right != null);
                D.assert(bottom != null);
                D.assert(left != null);
                D.assert(horizontalInside != null);
                D.assert(verticalInside != null);

                Color topColor = top.color;
                if (right.color != topColor ||
                    bottom.color != topColor ||
                    left.color != topColor ||
                    horizontalInside.color != topColor ||
                    verticalInside.color != topColor) {
                    return false;
                }

                float topWidth = top.width;
                if (right.width != topWidth ||
                    bottom.width != topWidth ||
                    left.width != topWidth ||
                    horizontalInside.width != topWidth ||
                    verticalInside.width != topWidth) {
                    return false;
                }

                BorderStyle topStyle = top.style;
                if (right.style != topStyle ||
                    bottom.style != topStyle ||
                    left.style != topStyle ||
                    horizontalInside.style != topStyle ||
                    verticalInside.style != topStyle) {
                    return false;
                }

                return true;
            }
        }

        TableBorder scale(float t) {
            return new TableBorder(
                top: top.scale(t),
                right: right.scale(t),
                bottom: bottom.scale(t),
                left: left.scale(t),
                horizontalInside: horizontalInside.scale(t),
                verticalInside: verticalInside.scale(t)
            );
        }

        public static TableBorder lerp(TableBorder a, TableBorder b, float t) {
            if (a == null && b == null) {
                return null;
            }

            if (a == null) {
                return b.scale(t);
            }

            if (b == null) {
                return a.scale(1.0f - t);
            }

            return new TableBorder(
                top: BorderSide.lerp(a.top, b.top, t),
                right: BorderSide.lerp(a.right, b.right, t),
                bottom: BorderSide.lerp(a.bottom, b.bottom, t),
                left: BorderSide.lerp(a.left, b.left, t),
                horizontalInside: BorderSide.lerp(a.horizontalInside, b.horizontalInside, t),
                verticalInside: BorderSide.lerp(a.verticalInside, b.verticalInside, t)
            );
        }

        public void paint(Canvas canvas, Rect rect, List<float> rows, List<float> columns) {
            D.assert(top != null);
            D.assert(right != null);
            D.assert(bottom != null);
            D.assert(left != null);
            D.assert(horizontalInside != null);
            D.assert(verticalInside != null);

            D.assert(canvas != null);
            D.assert(rect != null);
            D.assert(rows != null);
            D.assert(rows.isEmpty() || (rows.First() >= 0.0f && rows.Last() <= rect.height));
            D.assert(columns != null);
            D.assert(columns.isEmpty() || (columns.First() >= 0.0f && columns.Last() <= rect.width));

            if (columns.isNotEmpty() || rows.isNotEmpty()) {
                Paint paint = new Paint();

                if (columns.isNotEmpty()) {
                    switch (verticalInside.style) {
                        case BorderStyle.solid: {
                            paint.color = verticalInside.color;
                            paint.strokeWidth = verticalInside.width;
                            paint.style = PaintingStyle.stroke;
                            Path path = new Path();

                            foreach (float x in columns) {
                                path.moveTo(rect.left + x, rect.top);
                                path.lineTo(rect.left + x, rect.bottom);
                            }

                            canvas.drawPath(path, paint);
                            break;
                        }
                        case BorderStyle.none: {
                            break;
                        }
                    }
                }

                if (rows.isNotEmpty()) {
                    switch (horizontalInside.style) {
                        case BorderStyle.solid: {
                            paint.color = horizontalInside.color;
                            paint.strokeWidth = horizontalInside.width;
                            paint.style = PaintingStyle.stroke;
                            Path path = new Path();

                            foreach (float y in rows) {
                                path.moveTo(rect.left, rect.top + y);
                                path.lineTo(rect.right, rect.top + y);
                            }

                            canvas.drawPath(path, paint);
                            break;
                        }
                        case BorderStyle.none: {
                            break;
                        }
                    }
                }

                BorderUtils.paintBorder(canvas, rect, top: top, right: right, bottom: bottom,
                    left: left);
            }
        }

        public bool Equals(TableBorder other) {
            if (ReferenceEquals(null, other)) {
                return false;
            }

            if (ReferenceEquals(this, other)) {
                return true;
            }

            return other.top == top &&
                   other.right == right &&
                   other.bottom == bottom &&
                   other.left == left &&
                   other.horizontalInside == horizontalInside &&
                   other.verticalInside == verticalInside;
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

            return Equals((TableBorder) obj);
        }

        public static bool operator ==(TableBorder left, TableBorder right) {
            return Equals(left, right);
        }

        public static bool operator !=(TableBorder left, TableBorder right) {
            return !Equals(left, right);
        }

        public override int GetHashCode() {
            unchecked {
                var hashCode = top.GetHashCode();
                hashCode = (hashCode * 397) ^ right.GetHashCode();
                hashCode = (hashCode * 397) ^ bottom.GetHashCode();
                hashCode = (hashCode * 397) ^ left.GetHashCode();
                hashCode = (hashCode * 397) ^ horizontalInside.GetHashCode();
                hashCode = (hashCode * 397) ^ verticalInside.GetHashCode();
                return hashCode;
            }
        }

        public override string ToString() {
            return
                $"TableBorder({top}, {right}, {bottom}, {left}, {horizontalInside}, {verticalInside})";
        }
    }
}