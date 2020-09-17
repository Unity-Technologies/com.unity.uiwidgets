using System;
using Unity.UIWidgets.foundation;
using UnityEngine;

namespace Unity.UIWidgets.ui {
    public class Matrix3 : IEquatable<Matrix3> {
        public static Matrix3 makeScale(float sx, float sy) {
            var m = new Matrix3();
            m.setScale(sx, sy);
            return m;
        }

        public static Matrix3 makeScale(float scale) {
            var m = new Matrix3();
            m.setScale(scale, scale);
            return m;
        }

        public static Matrix3 makeTrans(float dx, float dy) {
            var m = new Matrix3();
            m.setTranslate(dx, dy);
            return m;
        }

        public static Matrix3 makeRotate(float radians) {
            var m = new Matrix3();
            m.setRotate(radians);
            return m;
        }

        public static Matrix3 makeRotate(float radians, float px, float py) {
            var m = new Matrix3();
            m.setRotate(radians, px, py);
            return m;
        }

        public static Matrix3 makeTrans(Offset offset) {
            var m = new Matrix3();
            m.setTranslate(offset.dx, offset.dy);
            return m;
        }

        public static Matrix3 makeSkew(float dx, float dy) {
            var m = new Matrix3();
            m.setSkew(dx, dy);
            return m;
        }

        public static Matrix3 makeAll(
            float scaleX, float skewX, float transX,
            float skewY, float scaleY, float transY,
            float pers0, float pers1, float pers2) {
            var m = new Matrix3();
            m.setAll(scaleX, skewX, transX, skewY, scaleY, transY, pers0, pers1, pers2);
            return m;
        }

        public Matrix3(Matrix3 other) {
            D.assert(other != null);
            fMat = (float[]) other.fMat.Clone();
            fTypeMask = other.fTypeMask;
        }

        public void copyFrom(Matrix3 other) {
            Array.Copy(other.fMat, fMat, 9);
            fTypeMask = other.fTypeMask;
        }

        [Flags]
        public enum TypeMask {
            kIdentity_Mask = 0, //!< identity SkMatrix; all bits clear
            kTranslate_Mask = 0x01, //!< translation SkMatrix
            kScale_Mask = 0x02, //!< scale SkMatrix
            kAffine_Mask = 0x04, //!< skew or rotate SkMatrix
            kPerspective_Mask = 0x08, //!< perspective SkMatrix
        }

        public TypeMask getType() {
            if ((fTypeMask & kUnknown_Mask) != 0) {
                fTypeMask = computeTypeMask();
            }

            // only return the public masks
            return (TypeMask) (fTypeMask & 0xF);
        }

        public bool isIdentity() {
            return getType() == 0;
        }

        public bool isScaleTranslate() {
            return (getType() & ~(TypeMask.kScale_Mask | TypeMask.kTranslate_Mask)) == 0;
        }

        public bool isTranslate() {
            return (getType() & ~(TypeMask.kTranslate_Mask)) == 0;
        }

        public bool rectStaysRect() {
            if ((fTypeMask & kUnknown_Mask) != 0) {
                fTypeMask = computeTypeMask();
            }

            return (fTypeMask & kRectStaysRect_Mask) != 0;
        }

        public bool preservesAxisAlignment() {
            return rectStaysRect();
        }

        public bool hasPerspective() {
            return (getPerspectiveTypeMaskOnly() & TypeMask.kPerspective_Mask) != 0;
        }

        public bool isSimilarity(float tol = ScalarUtils.kScalarNearlyZero) {
            TypeMask mask = getType();
            if (mask <= TypeMask.kTranslate_Mask) {
                return true;
            }

            if ((mask & TypeMask.kPerspective_Mask) != 0) {
                return false;
            }

            float mx = fMat[kMScaleX];
            float my = fMat[kMScaleY];
            // if no skew, can just compare scale factors
            if ((mask & TypeMask.kAffine_Mask) == 0) {
                return !ScalarUtils.ScalarNearlyZero(mx) &&
                       ScalarUtils.ScalarNearlyEqual(Mathf.Abs(mx), Mathf.Abs(my));
            }

            float sx = fMat[kMSkewX];
            float sy = fMat[kMSkewY];

            if (ScalarUtils.is_degenerate_2x2(mx, sx, sy, my)) {
                return false;
            }

            // upper 2x2 is rotation/reflection + uniform scale if basis vectors
            // are 90 degree rotations of each other
            return (ScalarUtils.ScalarNearlyEqual(mx, my, tol) && ScalarUtils.ScalarNearlyEqual(sx, -sy, tol))
                   || (ScalarUtils.ScalarNearlyEqual(mx, -my, tol) && ScalarUtils.ScalarNearlyEqual(sx, sy, tol));
        }

        public bool preservesRightAngles(float tol = ScalarUtils.kScalarNearlyZero) {
            TypeMask mask = getType();

            if (mask <= TypeMask.kTranslate_Mask) {
                // identity, translate and/or scale
                return true;
            }

            if ((mask & TypeMask.kPerspective_Mask) != 0) {
                return false;
            }

            D.assert((mask & (TypeMask.kAffine_Mask | TypeMask.kScale_Mask)) != 0);

            float mx = fMat[kMScaleX];
            float my = fMat[kMScaleY];
            float sx = fMat[kMSkewX];
            float sy = fMat[kMSkewY];

            if (ScalarUtils.is_degenerate_2x2(mx, sx, sy, my)) {
                return false;
            }

            // upper 2x2 is scale + rotation/reflection if basis vectors are orthogonal
            var dot = mx * sx + sy * my;
            return ScalarUtils.ScalarNearlyZero(dot, tol * tol);
        }

        public const int kMScaleX = 0; //!< horizontal scale factor
        public const int kMSkewX = 1; //!< horizontal skew factor
        public const int kMTransX = 2; //!< horizontal translation
        public const int kMSkewY = 3; //!< vertical skew factor
        public const int kMScaleY = 4; //!< vertical scale factor
        public const int kMTransY = 5; //!< vertical translation
        public const int kMPersp0 = 6; //!< input x perspective factor
        public const int kMPersp1 = 7; //!< input y perspective factor
        public const int kMPersp2 = 8; //!< perspective bias

        public float this[int index] {
            get {
                D.assert((uint) index < 9);
                return fMat[index];
            }

            set {
                D.assert((uint) index < 9);
                fMat[index] = value;
                setTypeMask(kUnknown_Mask);
            }
        }

        public float getScaleX() {
            return fMat[kMScaleX];
        }

        public float getScaleY() {
            return fMat[kMScaleY];
        }

        public float getSkewY() {
            return fMat[kMSkewY];
        }

        public float getSkewX() {
            return fMat[kMSkewX];
        }

        public float getTranslateX() {
            return fMat[kMTransX];
        }

        public float getTranslateY() {
            return fMat[kMTransY];
        }

        public float getPerspX() {
            return fMat[kMPersp0];
        }

        public float getPerspY() {
            return fMat[kMPersp1];
        }

        public void setScaleX(float v) {
            this[kMScaleX] = v;
        }

        public void setScaleY(float v) {
            this[kMScaleY] = v;
        }

        public void setSkewY(float v) {
            this[kMSkewY] = v;
        }

        public void setSkewX(float v) {
            this[kMSkewX] = v;
        }

        public void setTranslateX(float v) {
            this[kMTransX] = v;
        }

        public void setTranslateY(float v) {
            this[kMTransY] = v;
        }

        public void setPerspX(float v) {
            this[kMPersp0] = v;
        }

        public void setPerspY(float v) {
            this[kMPersp1] = v;
        }

        public void setAll(
            float scaleX, float skewX, float transX,
            float skewY, float scaleY, float transY,
            float persp0, float persp1, float persp2) {
            fMat[kMScaleX] = scaleX;
            fMat[kMSkewX] = skewX;
            fMat[kMTransX] = transX;
            fMat[kMSkewY] = skewY;
            fMat[kMScaleY] = scaleY;
            fMat[kMTransY] = transY;
            fMat[kMPersp0] = persp0;
            fMat[kMPersp1] = persp1;
            fMat[kMPersp2] = persp2;
            setTypeMask(kUnknown_Mask);
        }

        public void get9(float[] buffer) {
            Array.Copy(fMat, buffer, 9);
        }

        public void set9(float[] buffer) {
            Array.Copy(buffer, fMat, 9);
            setTypeMask(kUnknown_Mask);
        }

        public void reset() {
            fMat[kMScaleX] = fMat[kMScaleY] = fMat[kMPersp2] = 1;
            fMat[kMSkewX] = fMat[kMSkewY] = fMat[kMTransX] =
                fMat[kMTransY] = fMat[kMPersp0] = fMat[kMPersp1] = 0;
            setTypeMask((int) TypeMask.kIdentity_Mask | kRectStaysRect_Mask);
        }

        public void setIdentity() {
            reset();
        }

        public void setTranslate(float dx, float dy) {
            if ((dx != 0) | (dy != 0)) {
                fMat[kMTransX] = dx;
                fMat[kMTransY] = dy;

                fMat[kMScaleX] = fMat[kMScaleY] = fMat[kMPersp2] = 1;
                fMat[kMSkewX] = fMat[kMSkewY] =
                    fMat[kMPersp0] = fMat[kMPersp1] = 0;

                setTypeMask((int) TypeMask.kTranslate_Mask | kRectStaysRect_Mask);
            }
            else {
                reset();
            }
        }

        public void setScale(float sx, float sy, float px, float py) {
            if (1 == sx && 1 == sy) {
                reset();
            }
            else {
                setScaleTranslate(sx, sy, px - sx * px, py - sy * py);
            }
        }

        public void setScale(float sx, float sy) {
            if (1 == sx && 1 == sy) {
                reset();
            }
            else {
                fMat[kMScaleX] = sx;
                fMat[kMScaleY] = sy;
                fMat[kMPersp2] = 1;

                fMat[kMTransX] = fMat[kMTransY] = fMat[kMSkewX] =
                    fMat[kMSkewY] = fMat[kMPersp0] = fMat[kMPersp1] = 0;

                setTypeMask((int) TypeMask.kScale_Mask | kRectStaysRect_Mask);
            }
        }

        public void setRotate(float radians, float px, float py) {
            float sinV, cosV;
            sinV = ScalarUtils.ScalarSinCos(radians, out cosV);
            setSinCos(sinV, cosV, px, py);
        }

        public void setRotate(float radians) {
            float sinV, cosV;
            sinV = ScalarUtils.ScalarSinCos(radians, out cosV);
            setSinCos(sinV, cosV);
        }

        public void setSinCos(float sinV, float cosV, float px, float py) {
            var oneMinusCosV = 1 - cosV;

            fMat[kMScaleX] = cosV;
            fMat[kMSkewX] = -sinV;
            fMat[kMTransX] = ScalarUtils.sdot(sinV, py, oneMinusCosV, px);

            fMat[kMSkewY] = sinV;
            fMat[kMScaleY] = cosV;
            fMat[kMTransY] = ScalarUtils.sdot(-sinV, px, oneMinusCosV, py);

            fMat[kMPersp0] = fMat[kMPersp1] = 0;
            fMat[kMPersp2] = 1;

            setTypeMask(kUnknown_Mask | kOnlyPerspectiveValid_Mask);
        }

        public void setSinCos(float sinV, float cosV) {
            fMat[kMScaleX] = cosV;
            fMat[kMSkewX] = -sinV;
            fMat[kMTransX] = 0;

            fMat[kMSkewY] = sinV;
            fMat[kMScaleY] = cosV;
            fMat[kMTransY] = 0;

            fMat[kMPersp0] = fMat[kMPersp1] = 0;
            fMat[kMPersp2] = 1;

            setTypeMask(kUnknown_Mask | kOnlyPerspectiveValid_Mask);
        }

        public void setSkew(float kx, float ky, float px, float py) {
            fMat[kMScaleX] = 1;
            fMat[kMSkewX] = kx;
            fMat[kMTransX] = -kx * py;

            fMat[kMSkewY] = ky;
            fMat[kMScaleY] = 1;
            fMat[kMTransY] = -ky * px;

            fMat[kMPersp0] = fMat[kMPersp1] = 0;
            fMat[kMPersp2] = 1;

            setTypeMask(kUnknown_Mask | kOnlyPerspectiveValid_Mask);
        }

        public void setSkew(float kx, float ky) {
            fMat[kMScaleX] = 1;
            fMat[kMSkewX] = kx;
            fMat[kMTransX] = 0;

            fMat[kMSkewY] = ky;
            fMat[kMScaleY] = 1;
            fMat[kMTransY] = 0;

            fMat[kMPersp0] = fMat[kMPersp1] = 0;
            fMat[kMPersp2] = 1;

            setTypeMask(kUnknown_Mask | kOnlyPerspectiveValid_Mask);
        }

        static bool only_scale_and_translate(int mask) {
            return 0 == (mask & (int) (TypeMask.kAffine_Mask | TypeMask.kPerspective_Mask));
        }

        static float rowcol3(float[] row, int i, float[] col, int j) {
            return row[i] * col[j] + row[i + 1] * col[j + 3] + row[i + 2] * col[j + 6];
        }

        static float muladdmul(float a, float b, float c, float d) {
            return (float) ((double) a * b + (double) c * d);
        }

        public void setConcat(Matrix3 a, Matrix3 b) {
            TypeMask aType = a.getType();
            TypeMask bType = b.getType();

            if (a.isTriviallyIdentity()) {
                copyFrom(b);
            }
            else if (b.isTriviallyIdentity()) {
                copyFrom(a);
            }
            else if (only_scale_and_translate((int) aType | (int) bType)) {
                setScaleTranslate(a.fMat[kMScaleX] * b.fMat[kMScaleX],
                    a.fMat[kMScaleY] * b.fMat[kMScaleY],
                    a.fMat[kMScaleX] * b.fMat[kMTransX] + a.fMat[kMTransX],
                    a.fMat[kMScaleY] * b.fMat[kMTransY] + a.fMat[kMTransY]);
            }
            else {
                Matrix3 tmp = new Matrix3();

                if (((aType | bType) & TypeMask.kPerspective_Mask) != 0) {
                    tmp.fMat[kMScaleX] = rowcol3(a.fMat, 0, b.fMat, 0);
                    tmp.fMat[kMSkewX] = rowcol3(a.fMat, 0, b.fMat, 1);
                    tmp.fMat[kMTransX] = rowcol3(a.fMat, 0, b.fMat, 2);
                    tmp.fMat[kMSkewY] = rowcol3(a.fMat, 3, b.fMat, 0);
                    tmp.fMat[kMScaleY] = rowcol3(a.fMat, 3, b.fMat, 1);
                    tmp.fMat[kMTransY] = rowcol3(a.fMat, 3, b.fMat, 2);
                    tmp.fMat[kMPersp0] = rowcol3(a.fMat, 6, b.fMat, 0);
                    tmp.fMat[kMPersp1] = rowcol3(a.fMat, 6, b.fMat, 1);
                    tmp.fMat[kMPersp2] = rowcol3(a.fMat, 6, b.fMat, 2);

                    tmp.setTypeMask(kUnknown_Mask);
                }
                else {
                    tmp.fMat[kMScaleX] = muladdmul(a.fMat[kMScaleX],
                        b.fMat[kMScaleX],
                        a.fMat[kMSkewX],
                        b.fMat[kMSkewY]);

                    tmp.fMat[kMSkewX] = muladdmul(a.fMat[kMScaleX],
                        b.fMat[kMSkewX],
                        a.fMat[kMSkewX],
                        b.fMat[kMScaleY]);

                    tmp.fMat[kMTransX] = muladdmul(a.fMat[kMScaleX],
                                             b.fMat[kMTransX],
                                             a.fMat[kMSkewX],
                                             b.fMat[kMTransY]) + a.fMat[kMTransX];

                    tmp.fMat[kMSkewY] = muladdmul(a.fMat[kMSkewY],
                        b.fMat[kMScaleX],
                        a.fMat[kMScaleY],
                        b.fMat[kMSkewY]);

                    tmp.fMat[kMScaleY] = muladdmul(a.fMat[kMSkewY],
                        b.fMat[kMSkewX],
                        a.fMat[kMScaleY],
                        b.fMat[kMScaleY]);

                    tmp.fMat[kMTransY] = muladdmul(a.fMat[kMSkewY],
                                             b.fMat[kMTransX],
                                             a.fMat[kMScaleY],
                                             b.fMat[kMTransY]) + a.fMat[kMTransY];

                    tmp.fMat[kMPersp0] = 0;
                    tmp.fMat[kMPersp1] = 0;
                    tmp.fMat[kMPersp2] = 1;
                    tmp.setTypeMask(kUnknown_Mask | kOnlyPerspectiveValid_Mask);
                }

                copyFrom(tmp);
            }
        }

        public void preTranslate(float dx, float dy) {
            var mask = getType();

            if (mask <= TypeMask.kTranslate_Mask) {
                fMat[kMTransX] += dx;
                fMat[kMTransY] += dy;
            }
            else if ((mask & TypeMask.kPerspective_Mask) != 0) {
                var m = new Matrix3();
                m.setTranslate(dx, dy);
                preConcat(m);
                return;
            }
            else {
                fMat[kMTransX] += fMat[kMScaleX] * dx + fMat[kMSkewX] * dy;
                fMat[kMTransY] += fMat[kMSkewY] * dx + fMat[kMScaleY] * dy;
            }

            updateTranslateMask();
        }

        public void preScale(float sx, float sy, float px, float py) {
            if (1 == sx && 1 == sy) {
                return;
            }

            var m = new Matrix3();
            m.setScale(sx, sy, px, py);
            preConcat(m);
        }

        public void preScale(float sx, float sy) {
            if (1 == sx && 1 == sy) {
                return;
            }

            // the assumption is that these multiplies are very cheap, and that
            // a full concat and/or just computing the matrix type is more expensive.
            // Also, the fixed-point case checks for overflow, but the float doesn't,
            // so we can get away with these blind multiplies.

            fMat[kMScaleX] *= sx;
            fMat[kMSkewY] *= sx;
            fMat[kMPersp0] *= sx;

            fMat[kMSkewX] *= sy;
            fMat[kMScaleY] *= sy;
            fMat[kMPersp1] *= sy;

            // Attempt to simplify our type when applying an inverse scale.
            // TODO: The persp/affine preconditions are in place to keep the mask consistent with
            //       what computeTypeMask() would produce (persp/skew always implies kScale).
            //       We should investigate whether these flag dependencies are truly needed.
            if (fMat[kMScaleX] == 1 && fMat[kMScaleY] == 1
                                         && (fTypeMask &
                                             (int) (TypeMask.kPerspective_Mask | TypeMask.kAffine_Mask)) == 0) {
                clearTypeMask((int) TypeMask.kScale_Mask);
            }
            else {
                orTypeMask((int) TypeMask.kScale_Mask);
            }
        }

        public void preRotate(float radians, float px, float py) {
            var m = new Matrix3();
            m.setRotate(radians, px, py);
            preConcat(m);
        }

        public void preRotate(float radians) {
            var m = new Matrix3();
            m.setRotate(radians);
            preConcat(m);
        }

        public void preSkew(float kx, float ky, float px, float py) {
            var m = new Matrix3();
            m.setSkew(kx, ky, px, py);
            preConcat(m);
        }

        public void preSkew(float kx, float ky) {
            var m = new Matrix3();
            m.setSkew(kx, ky);
            preConcat(m);
        }

        public void preConcat(Matrix3 other) {
            // check for identity first, so we don't do a needless copy of ourselves
            // to ourselves inside setConcat()
            if (!other.isIdentity()) {
                setConcat(this, other);
            }
        }

        public void postTranslate(float dx, float dy) {
            if (hasPerspective()) {
                var m = new Matrix3();
                m.setTranslate(dx, dy);
                postConcat(m);
            }
            else {
                fMat[kMTransX] += dx;
                fMat[kMTransY] += dy;
                updateTranslateMask();
            }
        }

        public void postScale(float sx, float sy, float px, float py) {
            if (1 == sx && 1 == sy) {
                return;
            }

            var m = new Matrix3();
            m.setScale(sx, sy, px, py);
            postConcat(m);
        }

        public void postScale(float sx, float sy) {
            if (1 == sx && 1 == sy) {
                return;
            }

            var m = new Matrix3();
            m.setScale(sx, sy);
            postConcat(m);
        }

        public void postRotate(float radians, float px, float py) {
            var m = new Matrix3();
            m.setRotate(radians, px, py);
            postConcat(m);
        }

        public void postRotate(float radians) {
            var m = new Matrix3();
            m.setRotate(radians);
            postConcat(m);
        }

        public void postSkew(float kx, float ky, float px, float py) {
            var m = new Matrix3();
            m.setSkew(kx, ky, px, py);
            postConcat(m);
        }

        public void postSkew(float kx, float ky) {
            var m = new Matrix3();
            m.setSkew(kx, ky);
            postConcat(m);
        }

        public void postConcat(Matrix3 mat) {
            if (!mat.isIdentity()) {
                setConcat(mat, this);
            }
        }

        public bool invert(Matrix3 inverse) {
            if (isIdentity()) {
                if (inverse != null) {
                    inverse.reset();
                }

                return true;
            }

            return invertNonIdentity(inverse);
        }

        public void mapPoints(Offset[] dst, Offset[] src) {
            D.assert(dst != null && src != null && dst.Length == src.Length);
            getMapPtsProc()(this, dst, src, src.Length);
        }

        public void mapPoints(Offset[] pts) {
            mapPoints(pts, pts);
        }

        public Offset mapXY(float x, float y) {
            getMapXYProc()(this, x, y, out var x1, out var y1);
            return new Offset(x1, y1);
        }
        
        public void mapXY(float x, float y, out float x1, out float y1) {
            getMapXYProc()(this, x, y, out x1, out y1);
        }

        public bool mapRect(out Rect dst, Rect src) {
            if (getType() <= TypeMask.kTranslate_Mask) {
                var tx = fMat[kMTransX];
                var ty = fMat[kMTransY];

                dst = Rect.fromLTRB(
                    src.left + tx,
                    src.top + ty,
                    src.right + tx,
                    src.bottom + ty
                ).normalize();

                return true;
            }

            if (isScaleTranslate()) {
                mapRectScaleTranslate(out dst, src);
                return true;
            }
            else {
                float x1, y1, x2, y2, x3, y3, x4, y4;
                mapXY(src.left, src.top, out x1, out y1);
                mapXY(src.right, src.top, out x2, out y2);
                mapXY(src.right, src.bottom, out x3, out y3);
                mapXY(src.left, src.bottom, out x4, out y4);

                var minX = x1;
                var minY = y1;
                var maxX = x1;
                var maxY = y1;

                if (x2 < minX) {
                    minX = x2;
                }
                if (x2 > maxX) {
                    maxX = x2;
                }
                if (y2 < minY) {
                    minY = y2;
                }
                if (y2 > maxY) {
                    maxY = y2;
                }

                if (x3 < minX) {
                    minX = x3;
                }
                if (x3 > maxX) {
                    maxX = x3;
                }
                if (y3 < minY) {
                    minY = y3;
                }
                if (y3 > maxY) {
                    maxY = y3;
                }

                if (x4 < minX) {
                    minX = x4;
                }
                if (x4 > maxX) {
                    maxX = x4;
                }
                if (y4 < minY) {
                    minY = y4;
                }
                if (y4 > maxY) {
                    maxY = y4;
                }

                dst = Rect.fromLTRB(minX, minY, maxX, maxY);
                return rectStaysRect(); // might still return true if rotated by 90, etc.
            }
        }

        public bool mapRect(ref Rect rect) {
            return mapRect(out rect, rect);
        }

        public Rect mapRect(Rect src) {
            Rect dst;
            mapRect(out dst, src);
            return dst;
        }

        public void mapRectScaleTranslate(out Rect dst, Rect src) {
            D.assert(isScaleTranslate());

            var sx = fMat[kMScaleX];
            var sy = fMat[kMScaleY];
            var tx = fMat[kMTransX];
            var ty = fMat[kMTransY];

            dst = Rect.fromLTRB(
                src.left * sx + tx,
                src.top * sy + ty,
                src.right * sx + tx,
                src.bottom * sy + ty
            ).normalize();
        }

        public Offset[] mapRectToQuad(Rect rect) {
            Offset[] dst = rect.toQuad();
            mapPoints(dst, dst);
            return dst;
        }

        public static bool operator ==(Matrix3 a, Matrix3 b) {
            if (ReferenceEquals(a, null) && ReferenceEquals(b, null)) {
                return true;
            }

            if (ReferenceEquals(a, b)) {
                return true;
            }

            if (ReferenceEquals(a, null) || ReferenceEquals(b, null)) {
                return false;
            }

            var ma = a.fMat;
            var mb = b.fMat;

            return ma[0] == mb[0] && ma[1] == mb[1] && ma[2] == mb[2] &&
                   ma[3] == mb[3] && ma[4] == mb[4] && ma[5] == mb[5] &&
                   ma[6] == mb[6] && ma[7] == mb[7] && ma[8] == mb[8];
        }

        public static bool operator !=(Matrix3 a, Matrix3 b) {
            return !(a == b);
        }

        public static Matrix3 I() {
            var m = new Matrix3();
            m.reset();
            return m;
        }

        public static Matrix3 concat(Matrix3 a, Matrix3 b) {
            Matrix3 result = new Matrix3();
            result.setConcat(a, b);
            return result;
        }

        public void dirtyMatrixTypeCache() {
            setTypeMask(kUnknown_Mask);
        }

        public void setScaleTranslate(float sx, float sy, float tx, float ty) {
            fMat[kMScaleX] = sx;
            fMat[kMSkewX] = 0;
            fMat[kMTransX] = tx;

            fMat[kMSkewY] = 0;
            fMat[kMScaleY] = sy;
            fMat[kMTransY] = ty;

            fMat[kMPersp0] = 0;
            fMat[kMPersp1] = 0;
            fMat[kMPersp2] = 1;

            int mask = 0;
            if (sx != 1 || sy != 1) {
                mask |= (int) TypeMask.kScale_Mask;
            }

            if (tx != 0 || ty != 0) {
                mask |= (int) TypeMask.kTranslate_Mask;
            }

            setTypeMask(mask | kRectStaysRect_Mask);
        }

        public bool isFinite() {
            return ScalarUtils.ScalarsAreFinite(fMat, 9);
        }

        // PRIVATE

        const int kRectStaysRect_Mask = 0x10;

        const int kOnlyPerspectiveValid_Mask = 0x40;

        const int kUnknown_Mask = 0x80;

        const int kORableMasks =
            (int)
            (TypeMask.kTranslate_Mask |
             TypeMask.kScale_Mask |
             TypeMask.kAffine_Mask |
             TypeMask.kPerspective_Mask);

        const int kAllMasks =
            (int)
            (TypeMask.kTranslate_Mask |
             TypeMask.kScale_Mask |
             TypeMask.kAffine_Mask |
             TypeMask.kPerspective_Mask) |
            kRectStaysRect_Mask;

        internal readonly float[] fMat = new float[9];
        int fTypeMask = 0;

        Matrix3() {
        }

        enum TypeShift {
            kTranslate_Shift,
            kScale_Shift,
            kAffine_Shift,
            kPerspective_Shift,
            kRectStaysRect_Shift
        }

        static void ComputeInv(float[] dst, float[] src, float invDet, bool isPersp) {
            D.assert(src != dst);
            D.assert(src != null && dst != null);

            if (isPersp) {
                dst[kMScaleX] =
                    ScalarUtils.scross_dscale(src[kMScaleY], src[kMPersp2], src[kMTransY],
                        src[kMPersp1], invDet);
                dst[kMSkewX] =
                    ScalarUtils.scross_dscale(src[kMTransX], src[kMPersp1], src[kMSkewX],
                        src[kMPersp2], invDet);
                dst[kMTransX] =
                    ScalarUtils.scross_dscale(src[kMSkewX], src[kMTransY], src[kMTransX],
                        src[kMScaleY], invDet);

                dst[kMSkewY] =
                    ScalarUtils.scross_dscale(src[kMTransY], src[kMPersp0], src[kMSkewY],
                        src[kMPersp2], invDet);
                dst[kMScaleY] =
                    ScalarUtils.scross_dscale(src[kMScaleX], src[kMPersp2], src[kMTransX],
                        src[kMPersp0], invDet);
                dst[kMTransY] =
                    ScalarUtils.scross_dscale(src[kMTransX], src[kMSkewY], src[kMScaleX],
                        src[kMTransY], invDet);

                dst[kMPersp0] =
                    ScalarUtils.scross_dscale(src[kMSkewY], src[kMPersp1], src[kMScaleY],
                        src[kMPersp0], invDet);
                dst[kMPersp1] =
                    ScalarUtils.scross_dscale(src[kMSkewX], src[kMPersp0], src[kMScaleX],
                        src[kMPersp1], invDet);
                dst[kMPersp2] =
                    ScalarUtils.scross_dscale(src[kMScaleX], src[kMScaleY], src[kMSkewX],
                        src[kMSkewY], invDet);
            }
            else {
                // not perspective
                dst[kMScaleX] = src[kMScaleY] * invDet;
                dst[kMSkewX] = -src[kMSkewX] * invDet;
                dst[kMTransX] =
                    ScalarUtils.dcross_dscale(src[kMSkewX], src[kMTransY], src[kMScaleY],
                        src[kMTransX], invDet);

                dst[kMSkewY] = -src[kMSkewY] * invDet;
                dst[kMScaleY] = src[kMScaleX] * invDet;
                dst[kMTransY] =
                    ScalarUtils.dcross_dscale(src[kMSkewY], src[kMTransX], src[kMScaleX],
                        src[kMTransY], invDet);

                dst[kMPersp0] = 0;
                dst[kMPersp1] = 0;
                dst[kMPersp2] = 1;
            }
        }

        const int kScalar1Int = 0x3f800000;

        int computeTypeMask() {
            int mask = 0;

            if (fMat[kMPersp0] != 0 || fMat[kMPersp1] != 0 ||
                fMat[kMPersp2] != 1) {
                // Once it is determined that that this is a perspective transform,
                // all other flags are moot as far as optimizations are concerned.
                return kORableMasks;
            }

            if (fMat[kMTransX] != 0 || fMat[kMTransY] != 0) {
                mask |= (int) TypeMask.kTranslate_Mask;
            }

            int m00 = ScalarUtils.ScalarAs2sCompliment(fMat[kMScaleX]);
            int m01 = ScalarUtils.ScalarAs2sCompliment(fMat[kMSkewX]);
            int m10 = ScalarUtils.ScalarAs2sCompliment(fMat[kMSkewY]);
            int m11 = ScalarUtils.ScalarAs2sCompliment(fMat[kMScaleY]);

            if ((m01 != 0) | (m10 != 0)) {
                // The skew components may be scale-inducing, unless we are dealing
                // with a pure rotation.  Testing for a pure rotation is expensive,
                // so we opt for being conservative by always setting the scale bit.
                // along with affine.
                // By doing this, we are also ensuring that matrices have the same
                // type masks as their inverses.
                mask |= (int) TypeMask.kAffine_Mask | (int) TypeMask.kScale_Mask;

                // For rectStaysRect, in the affine case, we only need check that
                // the primary diagonal is all zeros and that the secondary diagonal
                // is all non-zero.

                // map non-zero to 1
                m01 = m01 != 0 ? 1 : 0;
                m10 = m10 != 0 ? 1 : 0;

                int dp0 = 0 == (m00 | m11) ? 1 : 0; // true if both are 0
                int ds1 = m01 & m10; // true if both are 1

                mask |= (dp0 & ds1) << (int) TypeShift.kRectStaysRect_Shift;
            }
            else {
                // Only test for scale explicitly if not affine, since affine sets the
                // scale bit.
                if (((m00 ^ kScalar1Int) | (m11 ^ kScalar1Int)) != 0) {
                    mask |= (int) TypeMask.kScale_Mask;
                }

                // Not affine, therefore we already know secondary diagonal is
                // all zeros, so we just need to check that primary diagonal is
                // all non-zero.

                // map non-zero to 1
                m00 = m00 != 0 ? 1 : 0;
                m11 = m11 != 0 ? 1 : 0;

                // record if the (p)rimary diagonal is all non-zero
                mask |= (m00 & m11) << (int) TypeShift.kRectStaysRect_Shift;
            }

            return mask;
        }

        int computePerspectiveTypeMask() {
            // Benchmarking suggests that replacing this set of floatAs2sCompliment
            // is a win, but replacing those below is not. We don't yet understand
            // that result.
            if (fMat[kMPersp0] != 0 || fMat[kMPersp1] != 0 ||
                fMat[kMPersp2] != 1) {
                // If this is a perspective transform, we return true for all other
                // transform flags - this does not disable any optimizations, respects
                // the rule that the type mask must be conservative, and speeds up
                // type mask computation.
                return kORableMasks;
            }

            return kOnlyPerspectiveValid_Mask | kUnknown_Mask;
        }


        void setTypeMask(int mask) {
            D.assert(kUnknown_Mask == mask || (mask & kAllMasks) == mask ||
                     ((kUnknown_Mask | kOnlyPerspectiveValid_Mask) & mask)
                     == (kUnknown_Mask | kOnlyPerspectiveValid_Mask));
            fTypeMask = mask;
        }

        void orTypeMask(int mask) {
            D.assert((mask & kORableMasks) == mask);
            fTypeMask |= mask;
        }

        void clearTypeMask(int mask) {
            // only allow a valid mask
            D.assert((mask & kAllMasks) == mask);
            fTypeMask &= ~mask;
        }

        TypeMask getPerspectiveTypeMaskOnly() {
            if ((fTypeMask & kUnknown_Mask) != 0 &&
                (fTypeMask & kOnlyPerspectiveValid_Mask) == 0) {
                fTypeMask = computePerspectiveTypeMask();
            }

            return (TypeMask) (fTypeMask & 0xF);
        }

        bool isTriviallyIdentity() {
            if ((fTypeMask & kUnknown_Mask) != 0) {
                return false;
            }

            return (fTypeMask & 0xF) == 0;
        }

        void updateTranslateMask() {
            if ((fMat[kMTransX] != 0) | (fMat[kMTransY] != 0)) {
                fTypeMask |= (int) TypeMask.kTranslate_Mask;
            }
            else {
                fTypeMask &= ~(int) TypeMask.kTranslate_Mask;
            }
        }

        delegate void MapXYProc(Matrix3 mat, float x, float y, out float x1, out float y1);

        static readonly MapXYProc[] gMapXYProcs = {
            Identity_xy, Trans_xy,
            Scale_xy, ScaleTrans_xy,
            Rot_xy, RotTrans_xy,
            Rot_xy, RotTrans_xy,
            // repeat the persp proc 8 times
            Persp_xy, Persp_xy,
            Persp_xy, Persp_xy,
            Persp_xy, Persp_xy,
            Persp_xy, Persp_xy
        };

        static MapXYProc GetMapXYProc(TypeMask mask) {
            D.assert(((int) mask & ~kAllMasks) == 0);
            return gMapXYProcs[(int) mask & kAllMasks];
        }

        MapXYProc getMapXYProc() {
            return GetMapXYProc(getType());
        }

        static void Identity_xy(Matrix3 m, float sx, float sy, out float resX, out float resY) {
            D.assert(0 == m.getType());

            resX = sx;
            resY = sy;
        }

        static void Trans_xy(Matrix3 m, float sx, float sy, out float resX, out float resY) {
            D.assert(m.getType() == TypeMask.kTranslate_Mask);

            resX = sx + m.fMat[kMTransX];
            resY = sy + m.fMat[kMTransY];
        }

        static void Scale_xy(Matrix3 m, float sx, float sy, out float resX, out float resY) {
            D.assert((m.getType() & (TypeMask.kScale_Mask | TypeMask.kAffine_Mask | TypeMask.kPerspective_Mask))
                     == TypeMask.kScale_Mask);
            D.assert(0 == m.fMat[kMTransX]);
            D.assert(0 == m.fMat[kMTransY]);

            resX = sx * m.fMat[kMScaleX];
            resY = sy * m.fMat[kMScaleY];
        }

        static void ScaleTrans_xy(Matrix3 m, float sx, float sy, out float resX, out float resY) {
            D.assert((m.getType() & (TypeMask.kScale_Mask | TypeMask.kAffine_Mask | TypeMask.kPerspective_Mask))
                     == TypeMask.kScale_Mask);

            resX = sx * m.fMat[kMScaleX] + m.fMat[kMTransX];
            resY = sy * m.fMat[kMScaleY] + m.fMat[kMTransY];
        }

        static void Rot_xy(Matrix3 m, float sx, float sy, out float resX, out float resY) {
            D.assert((m.getType() & (TypeMask.kAffine_Mask | TypeMask.kPerspective_Mask)) == TypeMask.kAffine_Mask);
            D.assert(0 == m.fMat[kMTransX]);
            D.assert(0 == m.fMat[kMTransY]);

            resX = ScalarUtils.sdot(sx, m.fMat[kMScaleX], sy, m.fMat[kMSkewX]);
            resY = ScalarUtils.sdot(sx, m.fMat[kMSkewY], sy, m.fMat[kMScaleY]);
        }

        static void RotTrans_xy(Matrix3 m, float sx, float sy, out float resX, out float resY) {
            D.assert((m.getType() & (TypeMask.kAffine_Mask | TypeMask.kPerspective_Mask)) == TypeMask.kAffine_Mask);

            resX = ScalarUtils.sdot(sx, m.fMat[kMScaleX], sy, m.fMat[kMSkewX]) + m.fMat[kMTransX];
            resY = ScalarUtils.sdot(sx, m.fMat[kMSkewY], sy, m.fMat[kMScaleY]) + m.fMat[kMTransY];
        }

        static void Persp_xy(Matrix3 m, float sx, float sy, out float resX, out float resY) {
            D.assert(m.hasPerspective());

            float x = ScalarUtils.sdot(sx, m.fMat[kMScaleX], sy, m.fMat[kMSkewX]) +
                      m.fMat[kMTransX];
            float y = ScalarUtils.sdot(sx, m.fMat[kMSkewY], sy, m.fMat[kMScaleY]) +
                      m.fMat[kMTransY];
            float z = ScalarUtils.sdot(sx, m.fMat[kMPersp0], sy, m.fMat[kMPersp1]) +
                      m.fMat[kMPersp2];
            if (z != 0) {
                z = 1 / z;
            }

            resX = x * z;
            resY = y * z;
        }

        delegate void MapPtsProc(Matrix3 mat, Offset[] dst, Offset[] src, int count);

        static readonly MapPtsProc[] gMapPtsProcs = {
            Identity_pts, Trans_pts,
            Scale_pts, Scale_pts,
            Affine_pts, Affine_pts,
            Affine_pts, Affine_pts,
            // repeat the persp proc 8 times
            Persp_pts, Persp_pts,
            Persp_pts, Persp_pts,
            Persp_pts, Persp_pts,
            Persp_pts, Persp_pts
        };

        static MapPtsProc GetMapPtsProc(TypeMask mask) {
            D.assert(((int) mask & ~kAllMasks) == 0);
            return gMapPtsProcs[(int) mask & kAllMasks];
        }

        MapPtsProc getMapPtsProc() {
            return GetMapPtsProc(getType());
        }

        static void Identity_pts(Matrix3 m, Offset[] dst, Offset[] src, int count) {
            D.assert(m.getType() == 0);

            if (dst != src && count > 0) {
                Array.Copy(src, dst, count);
            }
        }

        static void Trans_pts(Matrix3 m, Offset[] dst, Offset[] src, int count) {
            D.assert(m.getType() <= TypeMask.kTranslate_Mask);
            if (count > 0) {
                var tx = m.getTranslateX();
                var ty = m.getTranslateY();
                for (int i = 0; i < count; ++i) {
                    dst[i] = new Offset(src[i].dx + tx, src[i].dy + ty);
                }
            }
        }

        static void Scale_pts(Matrix3 m, Offset[] dst, Offset[] src, int count) {
            D.assert(m.getType() <= (TypeMask.kScale_Mask | TypeMask.kTranslate_Mask));
            if (count > 0) {
                var tx = m.getTranslateX();
                var ty = m.getTranslateY();
                var sx = m.getScaleX();
                var sy = m.getScaleY();

                for (int i = 0; i < count; ++i) {
                    dst[i] = new Offset(src[i].dx * sx + tx, src[i].dy * sy + ty);
                }
            }
        }

        static void Persp_pts(Matrix3 m, Offset[] dst, Offset[] src, int count) {
            D.assert(m.hasPerspective());

            if (count > 0) {
                for (int i = 0; i < count; ++i) {
                    var sy = src[i].dy;
                    var sx = src[i].dx;
                    var x = ScalarUtils.sdot(sx, m.fMat[kMScaleX], sy, m.fMat[kMSkewX]) +
                            m.fMat[kMTransX];
                    var y = ScalarUtils.sdot(sx, m.fMat[kMSkewY], sy, m.fMat[kMScaleY]) +
                            m.fMat[kMTransY];
                    var z = ScalarUtils.sdot(sx, m.fMat[kMPersp0], sy, m.fMat[kMPersp1]) +
                            m.fMat[kMPersp2];
                    if (z != 0) {
                        z = 1 / z;
                    }

                    dst[i] = new Offset(x * z, y * z);
                }
            }
        }

        static void Affine_pts(Matrix3 m, Offset[] dst, Offset[] src, int count) {
            D.assert(m.getType() != TypeMask.kPerspective_Mask);
            if (count > 0) {
                var tx = m.getTranslateX();
                var ty = m.getTranslateY();
                var sx = m.getScaleX();
                var sy = m.getScaleY();
                var kx = m.getSkewX();
                var ky = m.getSkewY();

                for (int i = 0; i < count; ++i) {
                    dst[i] = new Offset(
                        src[i].dx * sx + src[i].dy * kx + tx,
                        src[i].dx * ky + src[i].dy * sy + ty);
                }
            }
        }

        bool invertNonIdentity(Matrix3 inv) {
            D.assert(!isIdentity());

            TypeMask mask = getType();

            if (0 == (mask & ~(TypeMask.kScale_Mask | TypeMask.kTranslate_Mask))) {
                bool invertible = true;
                if (inv != null) {
                    if ((mask & TypeMask.kScale_Mask) != 0) {
                        var invX = fMat[kMScaleX];
                        var invY = fMat[kMScaleY];
                        if (0 == invX || 0 == invY) {
                            return false;
                        }

                        invX = 1f / invX;
                        invY = 1f / invY;

                        // Must be careful when writing to inv, since it may be the
                        // same memory as this.

                        inv.fMat[kMSkewX] = inv.fMat[kMSkewY] =
                            inv.fMat[kMPersp0] = inv.fMat[kMPersp1] = 0;

                        inv.fMat[kMScaleX] = invX;
                        inv.fMat[kMScaleY] = invY;
                        inv.fMat[kMPersp2] = 1;
                        inv.fMat[kMTransX] = -fMat[kMTransX] * invX;
                        inv.fMat[kMTransY] = -fMat[kMTransY] * invY;

                        inv.setTypeMask((int) mask | kRectStaysRect_Mask);
                    }
                    else {
                        // translate only
                        inv.setTranslate(-fMat[kMTransX], -fMat[kMTransY]);
                    }
                }
                else {
                    // inv is nullptr, just check if we're invertible
                    if (fMat[kMScaleX] == 0 || fMat[kMScaleY] == 0) {
                        invertible = false;
                    }
                }

                return invertible;
            }

            int isPersp = (int) (mask & TypeMask.kPerspective_Mask);
            float invDet = ScalarUtils.inv_determinant(fMat, isPersp);

            if (invDet == 0) {
                // underflow
                return false;
            }

            bool applyingInPlace = (inv == this);

            var tmp = inv;

            if (applyingInPlace || null == tmp) {
                tmp = new Matrix3(); // we either need to avoid trampling memory or have no memory
            }

            ComputeInv(tmp.fMat, fMat, invDet, isPersp != 0);
            if (!tmp.isFinite()) {
                return false;
            }

            tmp.setTypeMask(fTypeMask);

            if (applyingInPlace) {
                Array.Copy(tmp.fMat, inv.fMat, 9); // need to copy answer back
                inv.fTypeMask = tmp.fTypeMask;
            }

            return true;
        }

        public bool Equals(Matrix3 other) {
            if (ReferenceEquals(null, other)) {
                return false;
            }

            if (ReferenceEquals(this, other)) {
                return true;
            }

            return this == other;
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

            return Equals((Matrix3) obj);
        }

        public override int GetHashCode() {
            unchecked {
                int hash = 17;
                foreach (var element in fMat) {
                    hash = hash * 31 + element.GetHashCode();
                }

                return hash;
            }
        }

        public override string ToString() {
            return "Matrix3(" + string.Join(",", Array.ConvertAll(fMat, i => i.ToString())) + ")";
        }


        public Offset mapPoint(Offset point) {
            return mapXY(point.dx, point.dy);
        }
    }

    class ScalarUtils {
        public const float kScalarNearlyZero = 1f / (1 << 12);

        public static bool ScalarNearlyZero(float x, float tolerance = kScalarNearlyZero) {
            D.assert(tolerance >= 0);
            return Mathf.Abs(x) <= tolerance;
        }

        public static bool ScalarNearlyEqual(float x, float y, float tolerance = kScalarNearlyZero) {
            D.assert(tolerance >= 0);
            return Mathf.Abs(x - y) <= tolerance;
        }
        
        public static bool ScalarIsInteger(float scalar) {
            return scalar == Mathf.FloorToInt(scalar);
        }

        public static float DegreesToRadians(float degrees) {
            return degrees * (Mathf.PI / 180);
        }

        public static float RadiansToDegrees(float radians) {
            return radians * (180 / Mathf.PI);
        }

        public static float ScalarSinCos(float radians, out float cosValue) {
            float sinValue = Mathf.Sin(radians);

            cosValue = Mathf.Cos(radians);
            if (ScalarNearlyZero(cosValue)) {
                cosValue = 0;
            }

            if (ScalarNearlyZero(sinValue)) {
                sinValue = 0;
            }

            return sinValue;
        }

        public static bool ScalarsAreFinite(float[] array, int count) {
            float prod = 0;
            for (int i = 0; i < count; ++i) {
                prod *= array[i];
            }

            // At this point, prod will either be NaN or 0
            return prod == 0; // if prod is NaN, this check will return false
        }

        static byte[] _scalar_as_2s_compliment_vars = new byte[4];


        static unsafe int GetBytesToInt32(float value) {
            var intVal = *(int*) &value;
            fixed (byte* b = _scalar_as_2s_compliment_vars) {
                *((int*) b) = intVal;
            }

            fixed (byte* pbyte = &_scalar_as_2s_compliment_vars[0]) {
                return *((int*) pbyte);
            }
        }

        public static int ScalarAs2sCompliment(float x) {
            var result = GetBytesToInt32(x);
            if (result < 0) {
                result &= 0x7FFFFFFF;
                result = -result;
            }

            return result;
        }

        public static float sdot(float a, float b, float c, float d) {
            return a * b + c * d;
        }

        public static float sdot(float a, float b, float c, float d, float e, float f) {
            return a * b + c * d + e * f;
        }

        public static float scross(float a, float b, float c, float d) {
            return a * b - c * d;
        }

        public static double dcross(double a, double b, double c, double d) {
            return a * b - c * d;
        }

        public static float scross_dscale(float a, float b,
            float c, float d, double scale) {
            return (float) (scross(a, b, c, d) * scale);
        }

        public static float dcross_dscale(double a, double b,
            double c, double d, double scale) {
            return (float) (dcross(a, b, c, d) * scale);
        }

        public static bool is_degenerate_2x2(
            float scaleX, float skewX,
            float skewY, float scaleY) {
            float perp_dot = scaleX * scaleY - skewX * skewY;
            return ScalarNearlyZero(perp_dot,
                kScalarNearlyZero * kScalarNearlyZero);
        }

        public static float inv_determinant(float[] mat, int isPerspective) {
            double det;

            if (isPerspective != 0) {
                det = mat[Matrix3.kMScaleX] *
                      dcross(mat[Matrix3.kMScaleY], mat[Matrix3.kMPersp2],
                          mat[Matrix3.kMTransY], mat[Matrix3.kMPersp1])
                      +
                      mat[Matrix3.kMSkewX] *
                      dcross(mat[Matrix3.kMTransY], mat[Matrix3.kMPersp0],
                          mat[Matrix3.kMSkewY], mat[Matrix3.kMPersp2])
                      +
                      mat[Matrix3.kMTransX] *
                      dcross(mat[Matrix3.kMSkewY], mat[Matrix3.kMPersp1],
                          mat[Matrix3.kMScaleY], mat[Matrix3.kMPersp0]);
            }
            else {
                det = dcross(mat[Matrix3.kMScaleX], mat[Matrix3.kMScaleY],
                    mat[Matrix3.kMSkewX], mat[Matrix3.kMSkewY]);
            }

            // Since the determinant is on the order of the cube of the matrix members,
            // compare to the cube of the default nearly-zero constant (although an
            // estimate of the condition number would be better if it wasn't so expensive).
            if (ScalarNearlyZero((float) det,
                kScalarNearlyZero * kScalarNearlyZero * kScalarNearlyZero)) {
                return 0;
            }

            return 1.0f / (float) det;
        }
    }
}