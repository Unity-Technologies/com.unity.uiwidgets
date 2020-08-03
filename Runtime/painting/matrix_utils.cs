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
            q.w = Mathf.Sqrt( Mathf.Max( 0, 1 + m.entry(0,0) + m.entry(1,1) + m.entry(2,2) ) ) / 2; 
            q.x = Mathf.Sqrt( Mathf.Max( 0, 1 + m.entry(0,0) - m.entry(1,1) - m.entry(2,2) ) ) / 2; 
            q.y = Mathf.Sqrt( Mathf.Max( 0, 1 - m.entry(0,0) + m.entry(1,1) - m.entry(2,2) ) ) / 2; 
            q.z = Mathf.Sqrt( Mathf.Max( 0, 1 - m.entry(0,0) - m.entry(1,1) + m.entry(2,2) ) ) / 2; 
            q.x *= Mathf.Sign( q.x * ( m.entry(2,1) - m.entry(1,2) ) );
            q.y *= Mathf.Sign( q.y * ( m.entry(0,2) - m.entry(2,0) ) );
            q.z *= Mathf.Sign( q.z * ( m.entry(1,0) - m.entry(0,1) ) );
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

            List<string> result = new List<string>(3);
            for (int i = 0; i < 3; i++) {
                result.Add($"[{i}] {transform[i * 3]}, {transform[i * 3 + 1]}, {transform[i * 3 + 2]}");
            }

            return result;
        }

        public static Offset transformPoint(Matrix4 transform, Offset point) {
            Vector3 position3 = new Vector3(point.dx, point.dy, 0);
            Vector3 transformed3 = transform.perspectiveTransform(position3);
            return new Offset(transformed3.x, transformed3.y);
        }

        public static Rect transformRect(Matrix4 transform, Rect rect) {
            Offset point1 = transformPoint(transform, rect.topLeft);
            Offset point2 = transformPoint(transform, rect.topRight);
            Offset point3 = transformPoint(transform, rect.bottomLeft);
            Offset point4 = transformPoint(transform, rect.bottomRight);
            return Rect.fromLTRB(
                _min4(point1.dx, point2.dx, point3.dx, point4.dx),
                _min4(point1.dy, point2.dy, point3.dy, point4.dy),
                _max4(point1.dx, point2.dx, point3.dx, point4.dx),
                _max4(point1.dy, point2.dy, point3.dy, point4.dy)
            );
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
            D.assert(transform.determinant() != 0.0);
            if (isIdentity(transform))
                return rect;
            transform = Matrix4.tryInvert(transform);
            return transformRect(transform, rect);
        }
        
        static float _min4(float a, float b, float c, float d) {
            return Mathf.Min(a, Mathf.Min(b, Mathf.Min(c, d)));
        }

        static float _max4(float a, float b, float c, float d) {
            return Mathf.Max(a, Mathf.Max(b, Mathf.Max(c, d)));
        }

        public static Matrix4x4 toMatrix4x4(this Matrix3 matrix3) {
            var matrix = Matrix4x4.identity;

            matrix[0, 0] = matrix3[0]; // row 0
            matrix[0, 1] = matrix3[1];
            matrix[0, 3] = matrix3[2];

            matrix[1, 0] = matrix3[3]; // row 1
            matrix[1, 1] = matrix3[4];
            matrix[1, 3] = matrix3[5];

            matrix[3, 0] = matrix3[6]; // row 2
            matrix[3, 1] = matrix3[7];
            matrix[3, 3] = matrix3[8];

            return matrix;
        }
        
        public static Matrix3 toMatrix3(this Matrix4 matrix4) {
            var matrix = Matrix3.I();

            matrix[0] = matrix4.entry(0, 0);
            matrix[1] = matrix4.entry(0, 1);
            matrix[2] = matrix4.entry(0, 3);

            matrix[3] = matrix4.entry(1, 0);
            matrix[4] = matrix4.entry(1, 1);
            matrix[5] = matrix4.entry(1, 3);

            matrix[6] = matrix4.entry(3, 0);
            matrix[7] = matrix4.entry(3, 1);
            matrix[8] = matrix4.entry(3, 3);

            return matrix;
        }
    }

    public class TransformProperty : DiagnosticsProperty<Matrix4> {
        public TransformProperty(string name, Matrix4 value,
            bool showName = true,
            object defaultValue = null,
            DiagnosticLevel level = DiagnosticLevel.info
        ) : base(name, value, showName: showName, defaultValue: defaultValue ?? Diagnostics.kNoDefaultValue,
            level: level) { }

        protected override string valueToString(TextTreeConfiguration parentConfiguration = null) {
            if (parentConfiguration != null && !parentConfiguration.lineBreakProperties) {
                return this.value == null ? "null" : this.value.ToString();
            }

            return string.Join("\n", MatrixUtils.debugDescribeTransform(this.value));
        }
    }
}