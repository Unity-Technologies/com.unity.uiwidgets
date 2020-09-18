using System;
using Unity.UIWidgets.async;
using Unity.UIWidgets.foundation;
using UnityEngine;

namespace Unity.UIWidgets.ui {
    public class Image : IEquatable<Image>, IDisposable {
        Texture _texture;
        readonly bool _noDispose;
        readonly bool _isAsset;
        readonly bool _isDynamic;
        AssetBundle _bundle;

        public Image(Texture texture, bool noDispose = false, bool isAsset = false, AssetBundle bundle = null,
            bool isDynamic = false) {
            D.assert(!noDispose || !isAsset && bundle == null);
            D.assert(isAsset || bundle == null);

            _texture = texture;
            _noDispose = noDispose;
            _isAsset = isAsset;
            _bundle = bundle;
            _isDynamic = isDynamic;
        }

        public bool valid {
            get { return _texture != null; }
        }

        public int width {
            get { return _texture != null ? _texture.width : 0; }
        }

        public int height {
            get { return _texture != null ? _texture.height : 0; }
        }

        public Texture texture {
            get { return _texture; }
        }

        public bool isDynamic {
            get { return _isDynamic; }
        }

        ~Image() {
            _dispose(true);
        }

        void _dispose(bool finalizer) {
            if (_noDispose) {
                _texture = null;
                _bundle = null;
                return;
            }

            if (_isAsset) {
                var t = _texture;
                _texture = null;
                var b = _bundle;
                _bundle = null;

                if (b == null) {
                    if (finalizer) {
                        // make sure no ref back to this in finalizer
                        Timer.runInMainFromFinalizer(() => { Resources.UnloadAsset(t); });
                    }
                    else {
                        Resources.UnloadAsset(t);
                    }
                }
                else {
                    if (finalizer) {
                        // make sure no ref back to this in finalizer
                        Timer.runInMainFromFinalizer(() => { b.Unload(t); });
                    }
                    else {
                        b.Unload(t);
                    }
                }
            }
            else {
                var t = _texture;
                _texture = null;

                if (finalizer) {
                    // make sure no ref back to this in finalizer
                    Timer.runInMainFromFinalizer(() => { ObjectUtils.SafeDestroy(t); });
                }
                else {
                    ObjectUtils.SafeDestroy(t);
                }
            }
        }

        public void Dispose() {
            _dispose(false);
            GC.SuppressFinalize(this);
        }

        public bool Equals(Image other) {
            if (ReferenceEquals(null, other)) {
                return false;
            }

            if (ReferenceEquals(this, other)) {
                return true;
            }

            return Equals(_texture, other._texture);
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

            return Equals((Image) obj);
        }

        public override int GetHashCode() {
            return (_texture != null ? _texture.GetHashCode() : 0);
        }

        public static bool operator ==(Image left, Image right) {
            return Equals(left, right);
        }

        public static bool operator !=(Image left, Image right) {
            return !Equals(left, right);
        }

        public override string ToString() {
            return $"[{width}\u00D7{height}]";
        }
    }
}