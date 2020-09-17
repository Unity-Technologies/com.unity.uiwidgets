using UnityEngine;

namespace Unity.UIWidgets.flow {
    public class MatrixDecomposition {
        public MatrixDecomposition(Matrix4x4 matrix) {
            if (matrix[3, 3] == 0) {
                return;
            }

            for (int i = 0; i < 4; i++) {
                for (int j = 0; j < 4; j++) {
                    matrix[j, i] /= matrix[3, 3];
                }
            }

            Matrix4x4 perpectiveMatrix = matrix;
            for (int i = 0; i < 3; i++) {
                perpectiveMatrix[3, i] = 0;
            }

            perpectiveMatrix[3, 3] = 1;


            if (perpectiveMatrix.determinant == 0) {
                return;
            }

            if (matrix[3, 0] != 0 || matrix[3, 1] != 0 || matrix[3, 2] != 0) {
                Vector4 rightHandSide = new Vector4(matrix[3, 0], matrix[3, 1], matrix[3, 2], matrix[3, 3]);
                perspective = perpectiveMatrix.inverse.transpose * rightHandSide;

                matrix[3, 0] = 0;
                matrix[3, 1] = 0;
                matrix[3, 2] = 0;
                matrix[3, 3] = 1;
            }

            translation = new Vector3(matrix[0, 3], matrix[1, 3], matrix[2, 3]);

            matrix[0, 3] = 0;
            matrix[1, 3] = 0;
            matrix[2, 3] = 0;

            Vector3[] row = new Vector3[3];
            for (int i = 0; i < 3; i++) {
                row[i] = matrix.GetRow(i);
            }

            scale.x = row[0].magnitude;
            row[0] = row[0].normalized;

            shear.x = Vector3.Dot(row[0], row[1]);
            row[1] += row[0] * -shear.x;

            scale.y = row[1].magnitude;
            row[1] = row[1].normalized;
            shear.x /= scale.y;

            shear.y = Vector3.Dot(row[0], row[2]);
            row[2] += row[0] * -shear.y;

            shear.z = Vector3.Dot(row[1], row[2]);
            row[2] += row[1] * -shear.z;

            scale.z = row[2].magnitude;
            row[2] = row[2].normalized;

            shear.y /= scale.z;
            shear.z /= scale.z;

            if (Vector3.Dot(row[0], Vector3.Cross(row[1], row[2])) < 0) {
                scale.x *= -1;
                scale.y *= -1;
                scale.z *= -1;

                for (int i = 0; i < 3; i++) {
                    row[i].x *= -1;
                    row[i].y *= -1;
                    row[i].z *= -1;
                }
            }

            rotation = new Vector4(
                0.5f * Mathf.Sqrt(Mathf.Max(1.0f + row[0].x - row[1].y - row[2].z, 0.0f)),
                0.5f * Mathf.Sqrt(Mathf.Max(1.0f - row[0].x + row[1].y - row[2].z, 0.0f)),
                0.5f * Mathf.Sqrt(Mathf.Max(1.0f - row[0].x - row[1].y + row[2].z, 0.0f)),
                0.5f * Mathf.Sqrt(Mathf.Max(1.0f + row[0].x + row[1].y + row[2].z, 0.0f)));

            if (row[2].y > row[1].z) {
                rotation.x = -rotation.x;
            }

            if (row[0].z > row[2].x) {
                rotation.y = -rotation.y;
            }

            if (row[1].x > row[0].y) {
                rotation.z = -rotation.z;
            }

            valid = true;
        }


        public readonly bool valid;
        public readonly Vector3 translation;
        public readonly Vector3 scale;
        public readonly Vector3 shear;
        public readonly Vector4 perspective;
        public readonly Vector4 rotation;
    }
}