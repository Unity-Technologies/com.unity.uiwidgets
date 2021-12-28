using System.Collections.Generic;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.ui;
using UnityEngine;
using Rect = Unity.UIWidgets.ui.Rect;

namespace Unity.UIWidgets.painting {
    public static class MatrixUtils {
        public static float dot(this Vector3 left, Vector3 right) {
            return left.x * right.x + left.y * right.y + left.z * right.z;
        }

        public static Offset getAsTranslation(this Matrix4 values) {
            if (values[0] == 1 && // col 1
                values[1] == 0 &&
                values[2] == 0 &&
                values[3] == 0 &&
                values[4] == 0 && // col 2
                values[5] == 1 &&
                values[6] == 0 &&
                values[7] == 0 &&
                values[8] == 0 && // col 3
                values[9] == 0 &&
                values[10] == 1 &&
                values[11] == 0 &&
                values[14] == 0 && // bottom of col 4 (values 12 and 13 are the x and y offsets)
                values[15] == 1) {
                return new Offset(values[12], values[13]);
            }
            else {
                return null;
            }
        }

        public static void QuaternionFromMatrix(this Matrix4 m, ref Quaternion q) {
            q.w = Mathf.Sqrt(Mathf.Max(0, 1 + m.entry(0, 0) + m.entry(1, 1) + m.entry(2, 2))) / 2;
            q.x = Mathf.Sqrt(Mathf.Max(0, 1 + m.entry(0, 0) - m.entry(1, 1) - m.entry(2, 2))) / 2;
            q.y = Mathf.Sqrt(Mathf.Max(0, 1 - m.entry(0, 0) + m.entry(1, 1) - m.entry(2, 2))) / 2;
            q.z = Mathf.Sqrt(Mathf.Max(0, 1 - m.entry(0, 0) - m.entry(1, 1) + m.entry(2, 2))) / 2;
            q.x *= Mathf.Sign(q.x * (m.entry(2, 1) - m.entry(1, 2)));
            q.y *= Mathf.Sign(q.y * (m.entry(0, 2) - m.entry(2, 0)));
            q.z *= Mathf.Sign(q.z * (m.entry(1, 0) - m.entry(0, 1)));
        }

        public static Quaternion scaled(this Quaternion lhs, float scale) {
            lhs.x *= scale;
            lhs.y *= scale;
            lhs.z *= scale;
            lhs.w *= scale;
            return lhs;
        }

        public static Quaternion add(this Quaternion lhs, Quaternion rhs) {
            lhs.x += rhs.x;
            lhs.y += rhs.y;
            lhs.z += rhs.z;
            lhs.w += rhs.w;
            return lhs;
        }

        public static List<string> debugDescribeTransform(Matrix4 transform) {
            if (transform == null) {
                return new List<string> {"null"};
            }

            List<string> result = new List<string>(){
                $"[0] ${D.debugFormatFloat(transform.entry(0, 0))},${D.debugFormatFloat(transform.entry(0, 1))},${D.debugFormatFloat(transform.entry(0, 2))},${D.debugFormatFloat(transform.entry(0, 3))}",
                $"[1] ${D.debugFormatFloat(transform.entry(1, 0))},${D.debugFormatFloat(transform.entry(1, 1))},${D.debugFormatFloat(transform.entry(1, 2))},${D.debugFormatFloat(transform.entry(1, 3))}",
                $"[2] ${D.debugFormatFloat(transform.entry(2, 0))},${D.debugFormatFloat(transform.entry(2, 1))},${D.debugFormatFloat(transform.entry(2, 2))},${D.debugFormatFloat(transform.entry(2, 3))}",
                $"[3] ${D.debugFormatFloat(transform.entry(3, 0))},${D.debugFormatFloat(transform.entry(3, 1))},${D.debugFormatFloat(transform.entry(3, 2))},${D.debugFormatFloat(transform.entry(3, 3))}"
            };
            for (int i = 0; i < 3; i++) {
                result.Add($"[{i}] {transform[i * 3]}, {transform[i * 3 + 1]}, {transform[i * 3 + 2]}");
            }

            return result;
        }

        public static Offset transformPoint(Matrix4 transform, Offset point) {
            float[] storage = transform.storage;
            float x = point.dx;
            float y = point.dy;

            float rx = storage[0] * x + storage[4] * y + storage[12];
            float ry = storage[1] * x + storage[5] * y + storage[13];
            float rw = storage[3] * x + storage[7] * y + storage[15];
            if (rw == 1.0f) {
                return new Offset(rx, ry);
            }
            else {
                return new Offset(rx / rw, ry / rw);
            }
        }

        internal static Rect _safeTransformRect(Matrix4 transform, Rect rect) {
            float[] storage = transform.storage;
            bool isAffine = storage[3] == 0.0 &&
                            storage[7] == 0.0 &&
                            storage[15] == 1.0;

            _minMax = _minMax ?? new float[4];

            _accumulate(storage, rect.left, rect.top, true, isAffine);
            _accumulate(storage, rect.right, rect.top, false, isAffine);
            _accumulate(storage, rect.left, rect.bottom, false, isAffine);
            _accumulate(storage, rect.right, rect.bottom, false, isAffine);

            return Rect.fromLTRB(_minMax[0], _minMax[1], _minMax[2], _minMax[3]);
        }

        static float[] _minMax;

        static void _accumulate(float[] m, float x, float y, bool first, bool isAffine) {
            float w = isAffine ? 1.0f : 1.0f / (m[3] * x + m[7] * y + m[15]);
            float tx = (m[0] * x + m[4] * y + m[12]) * w;
            float ty = (m[1] * x + m[5] * y + m[13]) * w;
            if (first) {
                _minMax[0] = _minMax[2] = tx;
                _minMax[1] = _minMax[3] = ty;
            }
            else {
                if (tx < _minMax[0]) {
                    _minMax[0] = tx;
                }

                if (ty < _minMax[1]) {
                    _minMax[1] = ty;
                }

                if (tx > _minMax[2]) {
                    _minMax[2] = tx;
                }

                if (ty > _minMax[3]) {
                    _minMax[3] = ty;
                }
            }
        }

        public static Rect transformRect(Matrix4 transform, Rect rect) {
            float[] storage = transform.storage;
            float x = rect.left;
            float y = rect.top;
            float w = rect.right - x;
            float h = rect.bottom - y;

            if (!w.isFinite() || !h.isFinite()) {
                return _safeTransformRect(transform, rect);
            }


            float wx = storage[0] * w;
            float hx = storage[4] * h;
            float rx = storage[0] * x + storage[4] * y + storage[12];

            float wy = storage[1] * w;
            float hy = storage[5] * h;
            float ry = storage[1] * x + storage[5] * y + storage[13];

            if (storage[3] == 0.0f && storage[7] == 0.0f && storage[15] == 1.0f) {
                float left = rx;
                float right = rx;
                if (wx < 0) {
                    left += wx;
                }
                else {
                    right += wx;
                }

                if (hx < 0) {
                    left += hx;
                }
                else {
                    right += hx;
                }

                float top = ry;
                float bottom = ry;
                if (wy < 0) {
                    top += wy;
                }
                else {
                    bottom += wy;
                }

                if (hy < 0) {
                    top += hy;
                }
                else {
                    bottom += hy;
                }

                return Rect.fromLTRB(left, top, right, bottom);
            }
            else {
                float ww = storage[3] * w;
                float hw = storage[7] * h;
                float rw = storage[3] * x + storage[7] * y + storage[15];

                float ulx = rx / rw;
                float uly = ry / rw;
                float urx = (rx + wx) / (rw + ww);
                float ury = (ry + wy) / (rw + ww);
                float llx = (rx + hx) / (rw + hw);
                float lly = (ry + hy) / (rw + hw);
                float lrx = (rx + wx + hx) / (rw + ww + hw);
                float lry = (ry + wy + hy) / (rw + ww + hw);

                return Rect.fromLTRB(
                    _min4(ulx, urx, llx, lrx),
                    _min4(uly, ury, lly, lry),
                    _max4(ulx, urx, llx, lrx),
                    _max4(uly, ury, lly, lry)
                );
            }
        }

        static bool isIdentity(Matrix4 a) {
            D.assert(a != null);
            return a[0] == 1.0 // col 1
                   && a[1] == 0.0
                   && a[2] == 0.0
                   && a[3] == 0.0
                   && a[4] == 0.0 // col 2
                   && a[5] == 1.0
                   && a[6] == 0.0
                   && a[7] == 0.0
                   && a[8] == 0.0 // col 3
                   && a[9] == 0.0
                   && a[10] == 1.0
                   && a[11] == 0.0
                   && a[12] == 0.0 // col 4
                   && a[13] == 0.0
                   && a[14] == 0.0
                   && a[15] == 1.0;
        }

        public static Rect inverseTransformRect(Matrix4 transform, Rect rect) {
            D.assert(rect != null);
            
            //FIXME: there is a bug here
            //In flutter this assertion has been commented, but we cannot do it since the call of Matrix4.tryInvert will fail
            //we need to find a better way to fix this issue here
            // D.assert(transform.determinant() != 0.0);
            if (isIdentity(transform))
                return rect;
            transform = Matrix4.copy(transform);
            transform.invert();

            return transformRect(transform, rect);
        }

        static float _min4(float a, float b, float c, float d) {
            float e = (a < b) ? a : b;
            float f = (c < d) ? c : d;
            return (e < f) ? e : f;
        }

        static float _max4(float a, float b, float c, float d) {
            float e = (a > b) ? a : b;
            float f = (c > d) ? c : d;
            return (e > f) ? e : f;
        }

        public static Matrix4 createCylindricalProjectionTransform(
            float radius,
            float angle,
            float perspective = 0.001f,
            Axis? orientation = null
        ) {
            D.assert(perspective >= 0 && perspective <= 1.0);
            if (orientation == null) {
                orientation = Axis.vertical;
            }

            Matrix4 result = Matrix4.identity();
            result.setEntry(3, 2, -perspective);
            result.setEntry(2, 3, -radius);
            result.setEntry(3, 3, perspective * radius + 1.0f);

            result = result * ((
                orientation == Axis.horizontal
                    ? Matrix4.rotationY(angle)
                    : Matrix4.rotationX(angle)
            ) * Matrix4.translationValues(0.0f, 0.0f, radius)) as Matrix4;

            return result;
        }

        internal static Matrix4 forceToPoint(Offset offset) {
            var result = Matrix4.identity();
            result.setRow(0, new Vector4(0, 0, 0, offset.dx));
            result.setRow(1, new Vector4(0, 0, 0, offset.dy));
            return result;
        }
    }

    public class TransformProperty : DiagnosticsProperty<Matrix4> {
        public TransformProperty(string name, Matrix4 value,
            bool showName = true,
            object defaultValue = null,
            DiagnosticLevel level = DiagnosticLevel.info
        ) : base(name, value, showName: showName, defaultValue: defaultValue ?? foundation_.kNoDefaultValue,
            level: level) {
        }

        protected override string valueToString(TextTreeConfiguration parentConfiguration = null) {
            if (parentConfiguration != null && !parentConfiguration.lineBreakProperties) {
                return value == null ? "null" : value.ToString();
            }

            return string.Join("\n", MatrixUtils.debugDescribeTransform(valueT));
        }
    }
}