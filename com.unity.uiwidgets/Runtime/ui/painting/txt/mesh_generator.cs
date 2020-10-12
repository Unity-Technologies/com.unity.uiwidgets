using System;
using System.Collections.Generic;
using Unity.UIWidgets.foundation;
using UnityEngine;

namespace Unity.UIWidgets.uiOld{
    class MeshKey : PoolObject, IEquatable<MeshKey> {
        public long textBlobId;
        public float scale;

        public MeshKey() {
        }

        public static MeshKey create(long textBlobId, float scale) {
            var newKey = ObjectPool<MeshKey>.alloc();
            newKey.textBlobId = textBlobId;
            newKey.scale = scale;
            return newKey;
        }

        public bool Equals(MeshKey other) {
            if (ReferenceEquals(null, other)) {
                return false;
            }

            if (ReferenceEquals(this, other)) {
                return true;
            }

            return textBlobId == other.textBlobId && scale.Equals(other.scale);
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

            return Equals((MeshKey) obj);
        }

        public override int GetHashCode() {
            unchecked {
                return (textBlobId.GetHashCode() * 397) ^ scale.GetHashCode();
            }
        }

        public static bool operator ==(MeshKey left, MeshKey right) {
            return Equals(left, right);
        }

        public static bool operator !=(MeshKey left, MeshKey right) {
            return !Equals(left, right);
        }

        public override string ToString() {
            return $"{nameof(textBlobId)}: {textBlobId}, {nameof(scale)}: {scale}";
        }
    }

    class MeshInfo : PoolObject {
        public MeshKey key;
        public long textureVersion;
        public uiMeshMesh mesh;
        long _timeToLive;

        public MeshInfo() {
        }

        public static MeshInfo create(MeshKey key, uiMeshMesh mesh, long textureVersion, int timeToLive = 5) {
            var meshInfo = ObjectPool<MeshInfo>.alloc();
            meshInfo.mesh = mesh;
            meshInfo.key = key;
            meshInfo.textureVersion = textureVersion;
            meshInfo.touch(timeToLive);
            return meshInfo;
        }

        public override void clear() {
            ObjectPool<MeshKey>.release(key);
            ObjectPool<uiMeshMesh>.release(mesh);
        }

        public long timeToLive {
            get { return _timeToLive; }
        }

        public void touch(long timeTolive = 5) {
            _timeToLive = timeTolive + TextBlobMesh.frameCount;
        }
    }


    class TextBlobMesh : PoolObject {
        static readonly Dictionary<MeshKey, MeshInfo> _meshes = new Dictionary<MeshKey, MeshInfo>();
        static long _frameCount = 0;

        public TextBlob? textBlob;
        public float scale;
        public uiMatrix3 matrix;

        uiMeshMesh _mesh;
        bool _resolved;

        public TextBlobMesh() {
        }

        public override void clear() {
            ObjectPool<uiMeshMesh>.release(_mesh);
            _mesh = null;
            _resolved = false;
            textBlob = null;
        }

        public static TextBlobMesh create(TextBlob textBlob, float scale, uiMatrix3 matrix) {
            TextBlobMesh newMesh = ObjectPool<TextBlobMesh>.alloc();
            newMesh.textBlob = textBlob;
            newMesh.scale = scale;
            newMesh.matrix = matrix;
            return newMesh;
        }

        public static long frameCount {
            get { return _frameCount; }
        }

        public static int meshCount {
            get { return _meshes.Count; }
        }

        static List<MeshKey> _keysToRemove = new List<MeshKey>();

        public static void tickNextFrame() {
            _frameCount++;
            D.assert(_keysToRemove.Count == 0);
            foreach (var key in _meshes.Keys) {
                if (_meshes[key].timeToLive < _frameCount) {
                    _keysToRemove.Add(key);
                }
            }

            foreach (var key in _keysToRemove) {
                ObjectPool<MeshInfo>.release(_meshes[key]);
                _meshes.Remove(key);
            }

            _keysToRemove.Clear();
        }

        public uiMeshMesh resolveMesh() {
            if (_resolved) {
                return _mesh;
            }

            _resolved = true;

            var style = textBlob.Value.style;

            var text = textBlob.Value.text;
            var key = MeshKey.create(textBlob.Value.instanceId, scale);
            var fontInfo = FontManager.instance.getOrCreate(style.fontFamily, style.fontWeight, style.fontStyle);
            var font = fontInfo.font;

            _meshes.TryGetValue(key, out var meshInfo);
            if (meshInfo != null && meshInfo.textureVersion == fontInfo.textureVersion) {
                ObjectPool<MeshKey>.release(key);
                meshInfo.touch();
                _mesh = meshInfo.mesh.transform(matrix);
                return _mesh;
            }

            // Handling Emoji
            char startingChar = text[textBlob.Value.textOffset];
            if (char.IsHighSurrogate(startingChar) || EmojiUtils.isSingleCharEmoji(startingChar)) {
                var vert = ObjectPool<uiList<Vector3>>.alloc();
                var tri = ObjectPool<uiList<int>>.alloc();
                var uvCoord = ObjectPool<uiList<Vector2>>.alloc();

                var metrics = FontMetrics.fromFont(font, style.UnityFontSize);
                var minMaxRect = EmojiUtils.getMinMaxRect(style.fontSize, metrics.ascent, metrics.descent);
                var minX = minMaxRect.left;
                var maxX = minMaxRect.right;
                var minY = minMaxRect.top;
                var maxY = minMaxRect.bottom;

                for (int i = 0; i < textBlob.Value.textSize; i++) {
                    char a = text[textBlob.Value.textOffset + i];
                    int code = a;
                    if (char.IsHighSurrogate(a)) {
                        D.assert(i + 1 < textBlob.Value.textSize);
                        D.assert(textBlob.Value.textOffset + i + 1 < textBlob.Value.text.Length);
                        char b = text[textBlob.Value.textOffset + i + 1];
                        D.assert(char.IsLowSurrogate(b));
                        code = char.ConvertToUtf32(a, b);
                    }
                    else if (char.IsLowSurrogate(a) || EmojiUtils.isEmptyEmoji(a)) {
                        continue;
                    }

                    var uvRect = EmojiUtils.getUVRect(code);

                    var positionX = textBlob.Value.getPositionX(i);

                    int baseIndex = vert.Count;
                    vert.Add(new Vector3(positionX + minX, minY, 0));
                    vert.Add(new Vector3(positionX + maxX, minY, 0));
                    vert.Add(new Vector3(positionX + maxX, maxY, 0));
                    vert.Add(new Vector3(positionX + minX, maxY, 0));
                    tri.Add(baseIndex);
                    tri.Add(baseIndex + 1);
                    tri.Add(baseIndex + 2);
                    tri.Add(baseIndex);
                    tri.Add(baseIndex + 2);
                    tri.Add(baseIndex + 3);
                    uvCoord.Add(uvRect.bottomLeft.toVector());
                    uvCoord.Add(uvRect.bottomRight.toVector());
                    uvCoord.Add(uvRect.topRight.toVector());
                    uvCoord.Add(uvRect.topLeft.toVector());

                    if (char.IsHighSurrogate(a)) {
                        i++;
                    }
                }

                uiMeshMesh meshMesh = uiMeshMesh.create(null, vert, tri, uvCoord);

                if (_meshes.ContainsKey(key)) {
                    ObjectPool<MeshInfo>.release(_meshes[key]);
                    _meshes.Remove(key);
                }

                _meshes[key] = MeshInfo.create(key, meshMesh, 0);

                _mesh = meshMesh.transform(matrix);
                return _mesh;
            }

            var length = textBlob.Value.textSize;
            var fontSizeToLoad = Mathf.CeilToInt(style.UnityFontSize * scale);

            var vertices = ObjectPool<uiList<Vector3>>.alloc();
            vertices.SetCapacity(length * 4);

            var triangles = ObjectPool<uiList<int>>.alloc();
            triangles.SetCapacity(length * 6);

            var uv = ObjectPool<uiList<Vector2>>.alloc();
            uv.SetCapacity(length * 4);

            for (int charIndex = 0; charIndex < length; ++charIndex) {
                var ch = text[charIndex + textBlob.Value.textOffset];
                // first char as origin for mesh position 
                var positionX = textBlob.Value.getPositionX(charIndex);
                if (LayoutUtils.isWordSpace(ch) || LayoutUtils.isLineEndSpace(ch) || ch == '\t') {
                    continue;
                }

                if (fontSizeToLoad == 0) {
                    continue;
                }

                font.getGlyphInfo(ch, out var glyphInfo, fontSizeToLoad, style.UnityFontStyle);

                var minX = glyphInfo.minX / scale;
                var maxX = glyphInfo.maxX / scale;
                var minY = -glyphInfo.maxY / scale;
                var maxY = -glyphInfo.minY / scale;

                var baseIndex = vertices.Count;

                vertices.Add(new Vector3(positionX + minX, minY, 0));
                vertices.Add(new Vector3(positionX + maxX, minY, 0));
                vertices.Add(new Vector3(positionX + maxX, maxY, 0));
                vertices.Add(new Vector3(positionX + minX, maxY, 0));

                triangles.Add(baseIndex);
                triangles.Add(baseIndex + 1);
                triangles.Add(baseIndex + 2);
                triangles.Add(baseIndex);
                triangles.Add(baseIndex + 2);
                triangles.Add(baseIndex + 3);

                uv.Add(glyphInfo.uvTopLeft);
                uv.Add(glyphInfo.uvTopRight);
                uv.Add(glyphInfo.uvBottomRight);
                uv.Add(glyphInfo.uvBottomLeft);
            }

            if (vertices.Count == 0) {
                _mesh = null;
                ObjectPool<uiList<Vector3>>.release(vertices);
                ObjectPool<uiList<Vector2>>.release(uv);
                ObjectPool<uiList<int>>.release(triangles);
                ObjectPool<MeshKey>.release(key);
                return null;
            }

            uiMeshMesh mesh = vertices.Count > 0 ? uiMeshMesh.create(null, vertices, triangles, uv) : null;

            if (_meshes.ContainsKey(key)) {
                ObjectPool<MeshInfo>.release(_meshes[key]);
                _meshes.Remove(key);
            }

            _meshes[key] = MeshInfo.create(key, mesh, fontInfo.textureVersion);

            _mesh = mesh.transform(matrix);
            return _mesh;
        }
    }
}