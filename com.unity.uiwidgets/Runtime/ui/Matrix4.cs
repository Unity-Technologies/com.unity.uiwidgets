using System;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.painting;
using UnityEngine;

namespace Unity.UIWidgets.ui {
    public class Matrix4 : IEquatable<Matrix4> {
        internal readonly float[] _m4storage;

        int dimension {
            get { return 4; }
        }

        public Matrix4() {
            _m4storage = new float[16];
        }

        public Matrix4(Matrix4 other) {
            _m4storage = new float[16];
            setFrom(other);
        }

        public static Matrix4 tryInvert(Matrix4 other) {
            Matrix4 r = new Matrix4();
            float determinant = r.copyInverse(other);
            if (determinant == 0) {
                return null;
            }

            return r;
        }

        public Matrix4(
            float arg0,
            float arg1,
            float arg2,
            float arg3,
            float arg4,
            float arg5,
            float arg6,
            float arg7,
            float arg8,
            float arg9,
            float arg10,
            float arg11,
            float arg12,
            float arg13,
            float arg14,
            float arg15) {
            _m4storage = new float[16];
            setValues(arg0, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9,
                arg10, arg11, arg12, arg13, arg14, arg15);
        }

        public Matrix4 fromList(
            float arg0,
            float arg1,
            float arg2,
            float arg3,
            float arg4,
            float arg5,
            float arg6,
            float arg7,
            float arg8,
            float arg9,
            float arg10,
            float arg11,
            float arg12,
            float arg13,
            float arg14,
            float arg15) {
            setValues(arg0, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9,
                arg10, arg11, arg12, arg13, arg14, arg15);
            return this;
        }

        public Matrix4 identity() {
            setIdentity();
            return this;
        }

        public Matrix4 inverted(Matrix4 other) {
            double determinant = copyInverse(other);
            if (determinant == 0) {
                // throw new Exception("input matrix cannot be inverted");
                return null;
            }

            return this;
        }

        public Matrix4 copy(Matrix4 other) {
            setFrom(other);
            return this;
        }

        public Matrix4 compose(Vector3 translation, Quaternion rotation, Vector3 scale) {
            setFromTranslationRotationScale(translation, rotation, scale);
            return this;
        }

        public void setColumn(int column, Vector4 arg) {
            int entry = column * 4;
            _m4storage[entry + 3] = arg[3];
            _m4storage[entry + 2] = arg[2];
            _m4storage[entry + 1] = arg[1];
            _m4storage[entry + 0] = arg[0];
        }
        
        public Vector4 getColumn(int column) {
            Vector4 r = new Vector4();
            int entry = column * 4;
            r[3] = _m4storage[entry + 3];
            r[2] = _m4storage[entry + 2];
            r[1] = _m4storage[entry + 1];
            r[0] = _m4storage[entry + 0];
            return r;
        }
        
        public void setRow(int row, Vector4 arg) {
            _m4storage[index(row, 0)] = arg[0];
            _m4storage[index(row, 1)] = arg[1];
            _m4storage[index(row, 2)] = arg[2];
            _m4storage[index(row, 3)] = arg[3];
        }
        
        public Vector4 getRow(int row) {
            Vector4 r = new Vector4();
            r[0] = _m4storage[index(row, 0)];
            r[1] = _m4storage[index(row, 1)];
            r[2] = _m4storage[index(row, 2)];
            r[3] = _m4storage[index(row, 3)];
            return r;
        }
        
        public Matrix4 clone() => new Matrix4().copy(this);

        public static Matrix4 operator *(Matrix4 a, Matrix4 b) {
            var result = a.clone();
            result.multiply(b);
            return result;
        }

        void setFromTranslationRotationScale(
            Vector3 translation, Quaternion rotation, Vector3 scale) {
            setFromTranslationRotation(translation, rotation);
            this.scale(scale);
        }
        
        void setFromTranslationRotation(Vector3 arg0, Quaternion arg1) {
            float x = arg1[0];
            float y = arg1[1];
            float z = arg1[2];
            float w = arg1[3];
            float x2 = x + x;
            float y2 = y + y;
            float z2 = z + z;
            float xx = x * x2;
            float xy = x * y2;
            float xz = x * z2;
            float yy = y * y2;
            float yz = y * z2;
            float zz = z * z2;
            float wx = w * x2;
            float wy = w * y2;
            float wz = w * z2;

            _m4storage[0] = 1.0f - (yy + zz);
            _m4storage[1] = xy + wz;
            _m4storage[2] = xz - wy;
            _m4storage[3] = 0;
            _m4storage[4] = xy - wz;
            _m4storage[5] = 1.0f - (xx + zz);
            _m4storage[6] = yz + wx;
            _m4storage[7] = 0;
            _m4storage[8] = xz + wy;
            _m4storage[9] = yz - wx;
            _m4storage[10] = 1.0f - (xx + yy);
            _m4storage[11] = 0;
            _m4storage[12] = arg0[0];
            _m4storage[13] = arg0[1];
            _m4storage[14] = arg0[2];
            _m4storage[15] = 1.0f;
        }
        
        void setFrom(Matrix4 arg) {
            _m4storage[15] = arg[15];
            _m4storage[14] = arg[14];
            _m4storage[13] = arg[13];
            _m4storage[12] = arg[12];
            _m4storage[11] = arg[11];
            _m4storage[10] = arg[10];
            _m4storage[9] = arg[9];
            _m4storage[8] = arg[8];
            _m4storage[7] = arg[7];
            _m4storage[6] = arg[6];
            _m4storage[5] = arg[5];
            _m4storage[4] = arg[4];
            _m4storage[3] = arg[3];
            _m4storage[2] = arg[2];
            _m4storage[1] = arg[1];
            _m4storage[0] = arg[0];
        }

        void setValues(
            float arg0,
            float arg1,
            float arg2,
            float arg3,
            float arg4,
            float arg5,
            float arg6,
            float arg7,
            float arg8,
            float arg9,
            float arg10,
            float arg11,
            float arg12,
            float arg13,
            float arg14,
            float arg15) {
            _m4storage[15] = arg15;
            _m4storage[14] = arg14;
            _m4storage[13] = arg13;
            _m4storage[12] = arg12;
            _m4storage[11] = arg11;
            _m4storage[10] = arg10;
            _m4storage[9] = arg9;
            _m4storage[8] = arg8;
            _m4storage[7] = arg7;
            _m4storage[6] = arg6;
            _m4storage[5] = arg5;
            _m4storage[4] = arg4;
            _m4storage[3] = arg3;
            _m4storage[2] = arg2;
            _m4storage[1] = arg1;
            _m4storage[0] = arg0;
        }

        public void setZero() {
            _m4storage[0] = 0;
            _m4storage[1] = 0;
            _m4storage[2] = 0;
            _m4storage[3] = 0;
            _m4storage[4] = 0;
            _m4storage[5] = 0;
            _m4storage[6] = 0;
            _m4storage[7] = 0;
            _m4storage[8] = 0;
            _m4storage[9] = 0;
            _m4storage[10] = 0;
            _m4storage[11] = 0;
            _m4storage[12] = 0;
            _m4storage[13] = 0;
            _m4storage[14] = 0;
            _m4storage[15] = 0;
        }

        void setIdentity() {
            _m4storage[0] = 1;
            _m4storage[1] = 0;
            _m4storage[2] = 0;
            _m4storage[3] = 0;
            _m4storage[4] = 0;
            _m4storage[5] = 1;
            _m4storage[6] = 0;
            _m4storage[7] = 0;
            _m4storage[8] = 0;
            _m4storage[9] = 0;
            _m4storage[10] = 1;
            _m4storage[11] = 0;
            _m4storage[12] = 0;
            _m4storage[13] = 0;
            _m4storage[14] = 0;
            _m4storage[15] = 1;
        }

        Matrix4 absolute() {
            Matrix4 r = new Matrix4();
            this[0] = _m4storage[0].abs();
            this[1] = _m4storage[1].abs();
            this[2] = _m4storage[2].abs();
            this[3] = _m4storage[3].abs();
            this[4] = _m4storage[4].abs();
            this[5] = _m4storage[5].abs();
            this[6] = _m4storage[6].abs();
            this[7] = _m4storage[7].abs();
            this[8] = _m4storage[8].abs();
            this[9] = _m4storage[9].abs();
            this[10] = _m4storage[10].abs();
            this[11] = _m4storage[11].abs();
            this[12] = _m4storage[12].abs();
            this[13] = _m4storage[13].abs();
            this[14] = _m4storage[14].abs();
            this[15] = _m4storage[15].abs();
            return r;
        }

        public float determinant() {
            float det2_01_01 =
                _m4storage[0] * _m4storage[5] - _m4storage[1] * _m4storage[4];
            float det2_01_02 =
                _m4storage[0] * _m4storage[6] - _m4storage[2] * _m4storage[4];
            float det2_01_03 =
                _m4storage[0] * _m4storage[7] - _m4storage[3] * _m4storage[4];
            float det2_01_12 =
                _m4storage[1] * _m4storage[6] - _m4storage[2] * _m4storage[5];
            float det2_01_13 =
                _m4storage[1] * _m4storage[7] - _m4storage[3] * _m4storage[5];
            float det2_01_23 =
                _m4storage[2] * _m4storage[7] - _m4storage[3] * _m4storage[6];
            float det3_201_012 = _m4storage[8] * det2_01_12 -
                                 _m4storage[9] * det2_01_02 +
                                 _m4storage[10] * det2_01_01;
            float det3_201_013 = _m4storage[8] * det2_01_13 -
                                 _m4storage[9] * det2_01_03 +
                                 _m4storage[11] * det2_01_01;
            float det3_201_023 = _m4storage[8] * det2_01_23 -
                                 _m4storage[10] * det2_01_03 +
                                 _m4storage[11] * det2_01_02;
            float det3_201_123 = _m4storage[9] * det2_01_23 -
                                 _m4storage[10] * det2_01_13 +
                                 _m4storage[11] * det2_01_12;
            return -det3_201_123 * _m4storage[12] +
                   det3_201_023 * _m4storage[13] -
                   det3_201_013 * _m4storage[14] +
                   det3_201_012 * _m4storage[15];
        }

        float invertRotation() {
            float det = determinant();
            if (det == 0) {
                return 0;
            }

            float invDet = 1.0f / det;
            float ix;
            float iy;
            float iz;
            float jx;
            float jy;
            float jz;
            float kx;
            float ky;
            float kz;
            ix = invDet *
                 (_m4storage[5] * _m4storage[10] - _m4storage[6] * _m4storage[9]);
            iy = invDet *
                 (_m4storage[2] * _m4storage[9] - _m4storage[1] * _m4storage[10]);
            iz = invDet *
                 (_m4storage[1] * _m4storage[6] - _m4storage[2] * _m4storage[5]);
            jx = invDet *
                 (_m4storage[6] * _m4storage[8] - _m4storage[4] * _m4storage[10]);
            jy = invDet *
                 (_m4storage[0] * _m4storage[10] - _m4storage[2] * _m4storage[8]);
            jz = invDet *
                 (_m4storage[2] * _m4storage[4] - _m4storage[0] * _m4storage[6]);
            kx = invDet *
                 (_m4storage[4] * _m4storage[9] - _m4storage[5] * _m4storage[8]);
            ky = invDet *
                 (_m4storage[1] * _m4storage[8] - _m4storage[0] * _m4storage[9]);
            kz = invDet *
                 (_m4storage[0] * _m4storage[5] - _m4storage[1] * _m4storage[4]);
            _m4storage[0] = ix;
            _m4storage[1] = iy;
            _m4storage[2] = iz;
            _m4storage[4] = jx;
            _m4storage[5] = jy;
            _m4storage[6] = jz;
            _m4storage[8] = kx;
            _m4storage[9] = ky;
            _m4storage[10] = kz;
            return det;
        }

        public Vector4 transform(Vector4 arg) {
            float x_ = (_m4storage[0] * arg[0]) +
                       (_m4storage[4] * arg[1]) +
                       (_m4storage[8] * arg[2]) +
                       (_m4storage[12] * arg[3]);
            float y_ = (_m4storage[1] * arg[0]) +
                       (_m4storage[5] * arg[1]) +
                       (_m4storage[9] * arg[2]) +
                       (_m4storage[13] * arg[3]);
            float z_ = (_m4storage[2] * arg[0]) +
                       (_m4storage[6] * arg[1]) +
                       (_m4storage[10] * arg[2]) +
                       (_m4storage[14] * arg[3]);
            float w_ = (_m4storage[3] * arg[0]) +
                       (_m4storage[7] * arg[1]) +
                       (_m4storage[11] * arg[2]) +
                       (_m4storage[15] * arg[3]);
            arg[0] = x_;
            arg[1] = y_;
            arg[2] = z_;
            arg[3] = w_;
            return arg;
        }

        public Vector3 perspectiveTransform(Vector3 arg) {
            float x_ = (_m4storage[0] * arg[0]) +
                       (_m4storage[4] * arg[1]) +
                       (_m4storage[8] * arg[2]) +
                       _m4storage[12];
            float y_ = (_m4storage[1] * arg[0]) +
                       (_m4storage[5] * arg[1]) +
                       (_m4storage[9] * arg[2]) +
                       _m4storage[13];
            float z_ = (_m4storage[2] * arg[0]) +
                       (_m4storage[6] * arg[1]) +
                       (_m4storage[10] * arg[2]) +
                       _m4storage[14];
            float w_ = 1.0f /
                       ((_m4storage[3] * arg[0]) +
                        (_m4storage[7] * arg[1]) +
                        (_m4storage[11] * arg[2]) +
                        _m4storage[15]);
            arg[0] = x_ * w_;
            arg[1] = y_ * w_;
            arg[2] = z_ * w_;
            return arg;
        }

        public void translate(float tx, float ty = 0, float tz = 0, float tw = 1) {
            float t1 = _m4storage[0] * tx +
                       _m4storage[4] * ty +
                       _m4storage[8] * tz +
                       _m4storage[12] * tw;
            float t2 = _m4storage[1] * tx +
                       _m4storage[5] * ty +
                       _m4storage[9] * tz +
                       _m4storage[13] * tw;
            float t3 = _m4storage[2] * tx +
                       _m4storage[6] * ty +
                       _m4storage[10] * tz +
                       _m4storage[14] * tw;
            float t4 = _m4storage[3] * tx +
                       _m4storage[7] * ty +
                       _m4storage[11] * tz +
                       _m4storage[15] * tw;
            _m4storage[12] = t1;
            _m4storage[13] = t2;
            _m4storage[14] = t3;
            _m4storage[15] = t4;
        }

        public Matrix4 rotationX(float radians) {
            _m4storage[15] = 1.0f;
            setRotationX(radians);
            return this;
        }

        public Matrix4 rotationY(float radians) {
            _m4storage[15] = 1.0f;
            setRotationY(radians);
            return this;
        }

        public Matrix4 rotationZ(float radians) {
            _m4storage[15] = 1.0f;
            setRotationZ(radians);
            return this;
        }
        
        public Matrix4 rotationZ(float radians, float px, float py) {
            _m4storage[15] = 1.0f;
            setRotationZ(radians, px, py);
            return this;
        }

        public Matrix4 diagonal3Values(float x, float y, float z) {
            _m4storage[15] = 1;
            _m4storage[10] = z;
            _m4storage[5] = y;
            _m4storage[0] = x;
            return this;
        }

        public void scale(float sx, float sy, float sz, float sw = 1) {
            _m4storage[0] *= sx;
            _m4storage[1] *= sx;
            _m4storage[2] *= sx;
            _m4storage[3] *= sx;
            _m4storage[4] *= sy;
            _m4storage[5] *= sy;
            _m4storage[6] *= sy;
            _m4storage[7] *= sy;
            _m4storage[8] *= sz;
            _m4storage[9] *= sz;
            _m4storage[10] *= sz;
            _m4storage[11] *= sz;
            _m4storage[12] *= sw;
            _m4storage[13] *= sw;
            _m4storage[14] *= sw;
            _m4storage[15] *= sw;
        }
        
        public void scale(Vector3 s) {
            float sx = s.x;
            float sy = s.y;
            float sz = s.z;
            float sw = 1;
            _m4storage[0] *= sx;
            _m4storage[1] *= sx;
            _m4storage[2] *= sx;
            _m4storage[3] *= sx;
            _m4storage[4] *= sy;
            _m4storage[5] *= sy;
            _m4storage[6] *= sy;
            _m4storage[7] *= sy;
            _m4storage[8] *= sz;
            _m4storage[9] *= sz;
            _m4storage[10] *= sz;
            _m4storage[11] *= sz;
            _m4storage[12] *= sw;
            _m4storage[13] *= sw;
            _m4storage[14] *= sw;
            _m4storage[15] *= sw;
        }

        Matrix4 scaled(float sx, float sy, float sz, float sw = 1) {
            var result = clone();
            result.scale(sx, sy, sz, sw);
            return result;
        }

        public Matrix4 translationValues(float x, float y, float z) {
            identity();
            setTranslationRaw(x, y, z);
            return this;
        }

        public void rotateX(float angle) {
            float cosAngle = Mathf.Cos(angle);
            float sinAngle = Mathf.Sin(angle);
            float t1 = _m4storage[4] * cosAngle + _m4storage[8] * sinAngle;
            float t2 = _m4storage[5] * cosAngle + _m4storage[9] * sinAngle;
            float t3 = _m4storage[6] * cosAngle + _m4storage[10] * sinAngle;
            float t4 = _m4storage[7] * cosAngle + _m4storage[11] * sinAngle;
            float t5 = _m4storage[4] * -sinAngle + _m4storage[8] * cosAngle;
            float t6 = _m4storage[5] * -sinAngle + _m4storage[9] * cosAngle;
            float t7 = _m4storage[6] * -sinAngle + _m4storage[10] * cosAngle;
            float t8 = _m4storage[7] * -sinAngle + _m4storage[11] * cosAngle;
            _m4storage[4] = t1;
            _m4storage[5] = t2;
            _m4storage[6] = t3;
            _m4storage[7] = t4;
            _m4storage[8] = t5;
            _m4storage[9] = t6;
            _m4storage[10] = t7;
            _m4storage[11] = t8;
        }

        public void rotateY(float angle) {
            float cosAngle = Mathf.Cos(angle);
            float sinAngle = Mathf.Sin(angle);
            float t1 = _m4storage[0] * cosAngle + _m4storage[8] * -sinAngle;
            float t2 = _m4storage[1] * cosAngle + _m4storage[9] * -sinAngle;
            float t3 = _m4storage[2] * cosAngle + _m4storage[10] * -sinAngle;
            float t4 = _m4storage[3] * cosAngle + _m4storage[11] * -sinAngle;
            float t5 = _m4storage[0] * sinAngle + _m4storage[8] * cosAngle;
            float t6 = _m4storage[1] * sinAngle + _m4storage[9] * cosAngle;
            float t7 = _m4storage[2] * sinAngle + _m4storage[10] * cosAngle;
            float t8 = _m4storage[3] * sinAngle + _m4storage[11] * cosAngle;
            _m4storage[0] = t1;
            _m4storage[1] = t2;
            _m4storage[2] = t3;
            _m4storage[3] = t4;
            _m4storage[8] = t5;
            _m4storage[9] = t6;
            _m4storage[10] = t7;
            _m4storage[11] = t8;
        }

        public void rotateZ(float angle) {
            float cosAngle = Mathf.Cos(angle);
            float sinAngle = Mathf.Sin(angle);
            float t1 = _m4storage[0] * cosAngle + _m4storage[4] * sinAngle;
            float t2 = _m4storage[1] * cosAngle + _m4storage[5] * sinAngle;
            float t3 = _m4storage[2] * cosAngle + _m4storage[6] * sinAngle;
            float t4 = _m4storage[3] * cosAngle + _m4storage[7] * sinAngle;
            float t5 = _m4storage[0] * -sinAngle + _m4storage[4] * cosAngle;
            float t6 = _m4storage[1] * -sinAngle + _m4storage[5] * cosAngle;
            float t7 = _m4storage[2] * -sinAngle + _m4storage[6] * cosAngle;
            float t8 = _m4storage[3] * -sinAngle + _m4storage[7] * cosAngle;
            _m4storage[0] = t1;
            _m4storage[1] = t2;
            _m4storage[2] = t3;
            _m4storage[3] = t4;
            _m4storage[4] = t5;
            _m4storage[5] = t6;
            _m4storage[6] = t7;
            _m4storage[7] = t8;
        }

        public void multiply(Matrix4 arg) {
            float m00 = _m4storage[0];
            float m01 = _m4storage[4];
            float m02 = _m4storage[8];
            float m03 = _m4storage[12];
            float m10 = _m4storage[1];
            float m11 = _m4storage[5];
            float m12 = _m4storage[9];
            float m13 = _m4storage[13];
            float m20 = _m4storage[2];
            float m21 = _m4storage[6];
            float m22 = _m4storage[10];
            float m23 = _m4storage[14];
            float m30 = _m4storage[3];
            float m31 = _m4storage[7];
            float m32 = _m4storage[11];
            float m33 = _m4storage[15];
            float[] argStorage = arg._m4storage;
            float n00 = argStorage[0];
            float n01 = argStorage[4];
            float n02 = argStorage[8];
            float n03 = argStorage[12];
            float n10 = argStorage[1];
            float n11 = argStorage[5];
            float n12 = argStorage[9];
            float n13 = argStorage[13];
            float n20 = argStorage[2];
            float n21 = argStorage[6];
            float n22 = argStorage[10];
            float n23 = argStorage[14];
            float n30 = argStorage[3];
            float n31 = argStorage[7];
            float n32 = argStorage[11];
            float n33 = argStorage[15];
            _m4storage[0] = (m00 * n00) + (m01 * n10) + (m02 * n20) + (m03 * n30);
            _m4storage[4] = (m00 * n01) + (m01 * n11) + (m02 * n21) + (m03 * n31);
            _m4storage[8] = (m00 * n02) + (m01 * n12) + (m02 * n22) + (m03 * n32);
            _m4storage[12] = (m00 * n03) + (m01 * n13) + (m02 * n23) + (m03 * n33);
            _m4storage[1] = (m10 * n00) + (m11 * n10) + (m12 * n20) + (m13 * n30);
            _m4storage[5] = (m10 * n01) + (m11 * n11) + (m12 * n21) + (m13 * n31);
            _m4storage[9] = (m10 * n02) + (m11 * n12) + (m12 * n22) + (m13 * n32);
            _m4storage[13] = (m10 * n03) + (m11 * n13) + (m12 * n23) + (m13 * n33);
            _m4storage[2] = (m20 * n00) + (m21 * n10) + (m22 * n20) + (m23 * n30);
            _m4storage[6] = (m20 * n01) + (m21 * n11) + (m22 * n21) + (m23 * n31);
            _m4storage[10] = (m20 * n02) + (m21 * n12) + (m22 * n22) + (m23 * n32);
            _m4storage[14] = (m20 * n03) + (m21 * n13) + (m22 * n23) + (m23 * n33);
            _m4storage[3] = (m30 * n00) + (m31 * n10) + (m32 * n20) + (m33 * n30);
            _m4storage[7] = (m30 * n01) + (m31 * n11) + (m32 * n21) + (m33 * n31);
            _m4storage[11] = (m30 * n02) + (m31 * n12) + (m32 * n22) + (m33 * n32);
            _m4storage[15] = (m30 * n03) + (m31 * n13) + (m32 * n23) + (m33 * n33);
        }

        public void decompose(ref Vector3 translation, ref Quaternion rotation, ref Vector3 scale) {
            Vector3 v = Vector3.zero;

            v.Set(_m4storage[0], _m4storage[1], _m4storage[2]);
            float sx = v.sqrMagnitude;

            v.Set(_m4storage[4], _m4storage[5], _m4storage[6]);
            float sy = v.sqrMagnitude;

            v.Set(_m4storage[8], _m4storage[9], _m4storage[10]);
            float sz = v.sqrMagnitude;

            if (determinant() < 0) {
                sx = -sx;
            }

            translation[0] = _m4storage[12];
            translation[1] = _m4storage[13];
            translation[2] = _m4storage[14];

            float invSX = 1.0f / sx;
            float invSY = 1.0f / sy;
            float invSZ = 1.0f / sz;

            Matrix4 m = new Matrix4().copy(this);
            m._m4storage[0] *= invSX;
            m._m4storage[1] *= invSX;
            m._m4storage[2] *= invSX;
            m._m4storage[4] *= invSY;
            m._m4storage[5] *= invSY;
            m._m4storage[6] *= invSY;
            m._m4storage[8] *= invSZ;
            m._m4storage[9] *= invSZ;
            m._m4storage[10] *= invSZ;

            m.QuaternionFromMatrix(ref rotation);

            scale[0] = sx;
            scale[1] = sy;
            scale[2] = sz;
        }

        void setTranslationRaw(float x, float y, float z) {
            _m4storage[14] = z;
            _m4storage[13] = y;
            _m4storage[12] = x;
        }

        void setRotationX(float radians) {
            float c = Mathf.Cos(radians);
            float s = Mathf.Sin(radians);
            _m4storage[0] = 1.0f;
            _m4storage[1] = 0;
            _m4storage[2] = 0;
            _m4storage[4] = 0;
            _m4storage[5] = c;
            _m4storage[6] = s;
            _m4storage[8] = 0;
            _m4storage[9] = -s;
            _m4storage[10] = c;
            _m4storage[3] = 0;
            _m4storage[7] = 0;
            _m4storage[11] = 0;
        }

        void setRotationY(float radians) {
            float c = Mathf.Cos(radians);
            float s = Mathf.Sin(radians);
            _m4storage[0] = c;
            _m4storage[1] = 0;
            _m4storage[2] = -s;
            _m4storage[4] = 0;
            _m4storage[5] = 1.0f;
            _m4storage[6] = 0;
            _m4storage[8] = s;
            _m4storage[9] = 0;
            _m4storage[10] = c;
            _m4storage[3] = 0;
            _m4storage[7] = 0;
            _m4storage[11] = 0;
        }

        void setRotationZ(float radians) {
            float c = Mathf.Cos(radians);
            float s = Mathf.Sin(radians);
            _m4storage[0] = c;
            _m4storage[1] = s;
            _m4storage[2] = 0;
            _m4storage[4] = -s;
            _m4storage[5] = c;
            _m4storage[6] = 0;
            _m4storage[8] = 0;
            _m4storage[9] = 0;
            _m4storage[10] = 1.0f;
            _m4storage[3] = 0;
            _m4storage[7] = 0;
            _m4storage[11] = 0;
        }
        
        void setRotationZ(float radians, float px, float py) {
            float c = Mathf.Cos(radians);
            float s = Mathf.Sin(radians);
            _m4storage[0] = c;
            _m4storage[1] = s;
            _m4storage[2] = 0;
            _m4storage[4] = -s;
            _m4storage[5] = c;
            _m4storage[6] = 0;
            _m4storage[8] = 0;
            _m4storage[9] = 0;
            _m4storage[10] = 1.0f;
            _m4storage[3] = 0;
            _m4storage[7] = 0;
            _m4storage[11] = 0;
            _m4storage[12] = s * py + (1 - c) * px;
            _m4storage[13] = -s * px + (1 - c) * py;
        }

        public float invert() => copyInverse(this);

        float copyInverse(Matrix4 arg) {
            float a00 = arg[0];
            float a01 = arg[1];
            float a02 = arg[2];
            float a03 = arg[3];
            float a10 = arg[4];
            float a11 = arg[5];
            float a12 = arg[6];
            float a13 = arg[7];
            float a20 = arg[8];
            float a21 = arg[9];
            float a22 = arg[10];
            float a23 = arg[11];
            float a30 = arg[12];
            float a31 = arg[13];
            float a32 = arg[14];
            float a33 = arg[15];
            float b00 = a00 * a11 - a01 * a10;
            float b01 = a00 * a12 - a02 * a10;
            float b02 = a00 * a13 - a03 * a10;
            float b03 = a01 * a12 - a02 * a11;
            float b04 = a01 * a13 - a03 * a11;
            float b05 = a02 * a13 - a03 * a12;
            float b06 = a20 * a31 - a21 * a30;
            float b07 = a20 * a32 - a22 * a30;
            float b08 = a20 * a33 - a23 * a30;
            float b09 = a21 * a32 - a22 * a31;
            float b10 = a21 * a33 - a23 * a31;
            float b11 = a22 * a33 - a23 * a32;
            float det =
                (b00 * b11 - b01 * b10 + b02 * b09 + b03 * b08 - b04 * b07 + b05 * b06);
            if (det == 0) {
                setFrom(arg);
                return 0;
            }

            float invDet = 1.0f / det;
            _m4storage[0] = (a11 * b11 - a12 * b10 + a13 * b09) * invDet;
            _m4storage[1] = (-a01 * b11 + a02 * b10 - a03 * b09) * invDet;
            _m4storage[2] = (a31 * b05 - a32 * b04 + a33 * b03) * invDet;
            _m4storage[3] = (-a21 * b05 + a22 * b04 - a23 * b03) * invDet;
            _m4storage[4] = (-a10 * b11 + a12 * b08 - a13 * b07) * invDet;
            _m4storage[5] = (a00 * b11 - a02 * b08 + a03 * b07) * invDet;
            _m4storage[6] = (-a30 * b05 + a32 * b02 - a33 * b01) * invDet;
            _m4storage[7] = (a20 * b05 - a22 * b02 + a23 * b01) * invDet;
            _m4storage[8] = (a10 * b10 - a11 * b08 + a13 * b06) * invDet;
            _m4storage[9] = (-a00 * b10 + a01 * b08 - a03 * b06) * invDet;
            _m4storage[10] = (a30 * b04 - a31 * b02 + a33 * b00) * invDet;
            _m4storage[11] = (-a20 * b04 + a21 * b02 - a23 * b00) * invDet;
            _m4storage[12] = (-a10 * b09 + a11 * b07 - a12 * b06) * invDet;
            _m4storage[13] = (a00 * b09 - a01 * b07 + a02 * b06) * invDet;
            _m4storage[14] = (-a30 * b03 + a31 * b01 - a32 * b00) * invDet;
            _m4storage[15] = (a20 * b03 - a21 * b01 + a22 * b00) * invDet;
            return det;
        }

        public int index(int row, int col) => (col * 4) + row;

        public float entry(int row, int col) {
            D.assert((row >= 0) && (row < dimension));
            D.assert((col >= 0) && (col < dimension));

            return _m4storage[index(row, col)];
        }

        public float this[int index] {
            get {
                D.assert((uint) index < 16);
                return _m4storage[index];
            }

            set {
                D.assert((uint) index < 16);
                _m4storage[index] = value;
            }
        }

        public bool Equals(Matrix4 other) {
            if (ReferenceEquals(null, other)) {
                return false;
            }

            if (ReferenceEquals(this, other)) {
                return true;
            }

            return (_m4storage[0] == other._m4storage[0]) &&
                   (_m4storage[1] == other._m4storage[1]) &&
                   (_m4storage[2] == other._m4storage[2]) &&
                   (_m4storage[3] == other._m4storage[3]) &&
                   (_m4storage[4] == other._m4storage[4]) &&
                   (_m4storage[5] == other._m4storage[5]) &&
                   (_m4storage[6] == other._m4storage[6]) &&
                   (_m4storage[7] == other._m4storage[7]) &&
                   (_m4storage[8] == other._m4storage[8]) &&
                   (_m4storage[9] == other._m4storage[9]) &&
                   (_m4storage[10] == other._m4storage[10]) &&
                   (_m4storage[11] == other._m4storage[11]) &&
                   (_m4storage[12] == other._m4storage[12]) &&
                   (_m4storage[13] == other._m4storage[13]) &&
                   (_m4storage[14] == other._m4storage[14]) &&
                   (_m4storage[15] == other._m4storage[15]);
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

            return Equals((Matrix4) obj);
        }

        public override int GetHashCode() {
            return (_m4storage != null ? _m4storage.GetHashCode() : 0);
        }
    }
}