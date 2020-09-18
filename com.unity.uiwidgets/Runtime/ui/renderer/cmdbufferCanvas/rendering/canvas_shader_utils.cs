using UnityEngine;

namespace Unity.UIWidgets.ui {
    public class MaterialPropertyBlockWrapper : PoolObject {
        public readonly MaterialPropertyBlock mpb;

        public MaterialPropertyBlockWrapper() {
            mpb = new MaterialPropertyBlock();
        }

        public override void clear() {
            mpb.Clear();
        }

        public void SetVector(int mid, Vector4 vec) {
            mpb.SetVector(mid, vec);
        }

        public void SetFloat(int mid, float value) {
            mpb.SetFloat(mid, value);
        }

        public void SetMatrix(int mid, Matrix4x4 mat) {
            mpb.SetMatrix(mid, mat);
        }

        public void SetTexture(int mid, Texture texture) {
            mpb.SetTexture(mid, texture);
        }

        public void SetInt(int mid, int value) {
            mpb.SetInt(mid, value);
        }

        public void SetFloatArray(int mid, float[] array) {
            mpb.SetFloatArray(mid, array);
        }

        public void SetBuffer(int mid, ComputeBuffer buffer) {
            mpb.SetBuffer(mid, buffer);
        }
    }
}