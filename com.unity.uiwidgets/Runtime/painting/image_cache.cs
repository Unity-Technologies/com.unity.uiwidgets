using System;
using System.Collections.Generic;
using Unity.UIWidgets.foundation;

namespace Unity.UIWidgets.painting {
    public class ImageCache {
        const int _kDefaultSize = 1000;
        const int _kDefaultSizeBytes = 100 << 20; // 100 MiB

        readonly Dictionary<object, _PendingImage> _pendingImages =
            new Dictionary<object, _PendingImage>();

        readonly Dictionary<object, _CachedImage> _cache = new Dictionary<object, _CachedImage>();
        readonly LinkedList<object> _lruKeys = new LinkedList<object>();

        int _maximumSize = _kDefaultSize;

        public int maximumSize {
            get { return _maximumSize; }
            set {
                D.assert(value >= 0);
                if (value == _maximumSize) {
                    return;
                }

                _maximumSize = value;
                if (_maximumSize == 0) {
                    clear();
                }
                else {
                    _checkCacheSize();
                }
            }
        }

        public int currentSize {
            get { return _cache.Count; }
        }

        int _maximumSizeBytes = _kDefaultSizeBytes;

        public int maximumSizeBytes {
            get { return _maximumSizeBytes; }
            set {
                D.assert(value >= 0);
                if (value == _maximumSizeBytes) {
                    return;
                }

                _maximumSizeBytes = value;
                if (_maximumSizeBytes == 0) {
                    clear();
                }
                else {
                    _checkCacheSize();
                }
            }
        }

        int _currentSizeBytes;

        public int currentSizeBytes {
            get { return _currentSizeBytes; }
        }

        public void clear() {
            _cache.Clear();
            _pendingImages.Clear();
            _currentSizeBytes = 0;

            _lruKeys.Clear();
        }

        public bool evict(object key) {
            D.assert(key != null);

            if (_pendingImages.TryGetValue(key, out var pendingImage)) {
                pendingImage.removeListener();
                _pendingImages.Remove(key);
                return true;
            }

            if (_cache.TryGetValue(key, out var image)) {
                _currentSizeBytes -= image.sizeBytes;
                _cache.Remove(key);
                _lruKeys.Remove(image.node);
                return true;
            }

            return false;
        }

        public ImageStreamCompleter putIfAbsent(object key, Func<ImageStreamCompleter> loader,
            ImageErrorListener onError = null) {
            D.assert(key != null);
            D.assert(loader != null);

            ImageStreamCompleter result = null;
            if (_pendingImages.TryGetValue(key, out var pendingImage)) {
                result = pendingImage.completer;
                return result;
            }

            if (_cache.TryGetValue(key, out var image)) {
                // put to the MRU position
                _lruKeys.Remove(image.node);
                image.node = _lruKeys.AddLast(key);
                return image.completer;
            }

            try {
                result = loader();
            }
            catch (Exception ex) {
                if (onError != null) {
                    onError(ex);
                }
                else {
                    throw;
                }
            }

            void listener(ImageInfo info, bool syncCall) {
                int imageSize = info?.image == null ? 0 : info.image.height * info.image.width * 4;
                _CachedImage cachedImage = new _CachedImage(result, imageSize);

                if (maximumSizeBytes > 0 && imageSize > maximumSizeBytes) {
                    _maximumSizeBytes = imageSize + 1000;
                }

                _currentSizeBytes += imageSize;

                if (_pendingImages.TryGetValue(key, out var loadedPendingImage)) {
                    loadedPendingImage.removeListener();
                    _pendingImages.Remove(key);
                }

                D.assert(!_cache.ContainsKey(key));
                _cache[key] = cachedImage;
                cachedImage.node = _lruKeys.AddLast(key);
                _checkCacheSize();
            }

            if (maximumSize > 0 && maximumSizeBytes > 0) {
                _pendingImages[key] = new _PendingImage(result, listener);
                result.addListener(listener);
            }

            return result;
        }

        void _checkCacheSize() {
            while (_currentSizeBytes > _maximumSizeBytes || _cache.Count > _maximumSize) {
                var node = _lruKeys.First;
                var key = node.Value; // get the LRU item

                D.assert(_cache.ContainsKey(key));
                _CachedImage image = _cache[key];

                D.assert(node == image.node);
                _currentSizeBytes -= image.sizeBytes;
                _cache.Remove(key);
                _lruKeys.Remove(image.node);
            }

            D.assert(_currentSizeBytes >= 0);
            D.assert(_cache.Count <= maximumSize);
            D.assert(_currentSizeBytes <= maximumSizeBytes);
        }
    }

    class _CachedImage {
        public _CachedImage(ImageStreamCompleter completer, int sizeBytes) {
            this.completer = completer;
            this.sizeBytes = sizeBytes;
        }

        public ImageStreamCompleter completer;
        public int sizeBytes;
        public LinkedListNode<object> node;
    }

    class _PendingImage {
        public _PendingImage(
            ImageStreamCompleter completer,
            ImageListener listener
        ) {
            this.completer = completer;
            this.listener = listener;
        }

        public readonly ImageStreamCompleter completer;

        public readonly ImageListener listener;

        public void removeListener() {
            completer.removeListener(listener);
        }
    }
}