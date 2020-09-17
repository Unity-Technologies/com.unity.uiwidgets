using System;

namespace Unity.UIWidgets.foundation {
    public class WriteBuffer {
        public WriteBuffer() {
            _buffer = new byte[0];
            _position = 0;
        }

        byte[] _buffer;
        int _position;

        void _ensureCapacity(int value) {
            if (value <= _buffer.Length)
                return;
            int capacity = value;
            if (capacity < 256)
                capacity = 256;
            if (capacity < _buffer.Length * 2)
                capacity = _buffer.Length * 2;
            if ((uint) (_buffer.Length * 2) > 2147483591U)
                capacity = value > 2147483591 ? value : 2147483591;

            byte[] buffer = new byte[capacity];
            if (_buffer.Length > 0)
                Buffer.BlockCopy(_buffer, 0, buffer, 0, _buffer.Length);
            _buffer = buffer;
        }

        public void putUint8(byte value) {
            _ensureCapacity(_position + 1);
            _buffer[_position] = value;
            _position++;
        }

        public unsafe void putUint16(ushort value) {
            _ensureCapacity(_position + 2);
            fixed (byte* ptr = &_buffer[_position])
                *(ushort*) ptr = value;
            _position += 2;
        }

        public unsafe void putUint32(uint value) {
            _ensureCapacity(_position + 4);
            fixed (byte* ptr = &_buffer[_position])
                *(uint*) ptr = value;
            _position += 4;
        }

        public unsafe void putInt32(int value) {
            _ensureCapacity(_position + 4);
            fixed (byte* ptr = &_buffer[_position])
                *(int*) ptr = value;
            _position += 4;
        }

        public unsafe void putInt64(long value) {
            _ensureCapacity(_position + 8);
            fixed (byte* ptr = &_buffer[_position])
                *(long*) ptr = value;
            _position += 8;
        }

        public unsafe void putFloat32(float value) {
            _alignTo(4);
            _ensureCapacity(_position + 4);
            fixed (byte* ptr = &_buffer[_position])
                *(float*) ptr = value;
            _position += 4;
        }

        public void putUint8List(byte[] list) {
            int lengthInBytes = list.Length;
            _ensureCapacity(_position + lengthInBytes);
            Buffer.BlockCopy(list, 0, _buffer, _position, lengthInBytes);
            _position += lengthInBytes;
        }

        public void putInt32List(int[] list) {
            _alignTo(4);
            int lengthInBytes = list.Length * 4;
            _ensureCapacity(_position + lengthInBytes);
            Buffer.BlockCopy(list, 0, _buffer, _position, lengthInBytes);
            _position += lengthInBytes;
        }

        public void putInt64List(long[] list) {
            _alignTo(8);
            int lengthInBytes = list.Length * 8;
            _ensureCapacity(_position + lengthInBytes);
            Buffer.BlockCopy(list, 0, _buffer, _position, lengthInBytes);
            _position += lengthInBytes;
        }

        public void putFloat32List(float[] list) {
            _alignTo(4);
            int lengthInBytes = list.Length * 4;
            _ensureCapacity(_position + lengthInBytes);
            Buffer.BlockCopy(list, 0, _buffer, _position, lengthInBytes);
            _position += lengthInBytes;
        }

        void _alignTo(int alignment) {
            var mod = _position % alignment;
            if (mod != 0) {
                for (int i = 0; i < alignment - mod; i++)
                    putUint8(0);
            }
        }

        public byte[] done() {
            var bytes = new byte[_position];
            Buffer.BlockCopy(_buffer, 0, bytes, 0, _position);
            _buffer = null;
            return bytes;
        }
    }

    public class ReadBuffer {
        public ReadBuffer(byte[] data) {
            D.assert(data != null);
            this.data = data;
        }

        public readonly byte[] data;

        int _position = 0;

        public bool hasRemaining => _position < data.Length;

        public byte getUint8() {
            return data[_position++];
        }

        public unsafe ushort getUint16() {
            fixed (byte* ptr = &data[_position += 2])
                return *(ushort*) ptr;
        }

        public unsafe uint getUint32() {
            fixed (byte* ptr = &data[_position += 4])
                return *(uint*) ptr;
        }

        public unsafe int getInt32() {
            fixed (byte* ptr = &data[_position += 4])
                return *(int*) ptr;
        }

        public unsafe long getInt64() {
            fixed (byte* ptr = &data[_position += 8])
                return *(long*) ptr;
        }

        public unsafe float getFloat32() {
            _alignTo(4);
            fixed (byte* ptr = &data[_position += 4])
                return *(float*) ptr;
        }

        public byte[] getUint8List(int length) {
            byte[] list = new byte[length];
            int lengthInBytes = length;
            Buffer.BlockCopy(data, _position, list, 0, lengthInBytes);
            _position += lengthInBytes;
            return list;
        }

        public int[] getInt32List(int length) {
            _alignTo(4);
            int[] list = new int[length];
            int lengthInBytes = length * 4;
            Buffer.BlockCopy(data, _position, list, 0, lengthInBytes);
            _position += lengthInBytes;
            return list;
        }

        public long[] getInt64List(int length) {
            _alignTo(8);
            long[] list = new long[length];
            int lengthInBytes = length * 8;
            Buffer.BlockCopy(data, _position, list, 0, lengthInBytes);
            _position += lengthInBytes;
            return list;
        }

        public float[] getFloat32List(int length) {
            _alignTo(4);
            float[] list = new float[length];
            int lengthInBytes = length * 4;
            Buffer.BlockCopy(data, _position, list, 0, lengthInBytes);
            _position += lengthInBytes;
            return list;
        }

        void _alignTo(int alignment) {
            int mod = _position % alignment;
            if (mod != 0)
                _position += alignment - mod;
        }
    }
}