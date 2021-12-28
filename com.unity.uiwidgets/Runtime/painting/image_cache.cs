using System;
using System.Collections.Generic;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.ui;

namespace Unity.UIWidgets.painting {
    //TODO: TimeLineTask
    public class ImageCache {
        const int _kDefaultSize = 1000;
        const int _kDefaultSizeBytes = 100 << 20; // 100 MiB

        readonly Dictionary<object, _PendingImage> _pendingImages =
            new Dictionary<object, _PendingImage>();

        readonly Dictionary<object, _CachedImage> _cache = new Dictionary<object, _CachedImage>();
        readonly Dictionary<object, _LiveImage> _liveImages = new Dictionary<object, _LiveImage>();

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

        public bool evict(object key, bool includeLive = true) {
            if (includeLive) {
                _LiveImage liveImage = _liveImages.getOrDefault(key);
                _liveImages.Remove(key);
                liveImage?.removeListener();
            }

            D.assert(key != null);

            if (_pendingImages.TryGetValue(key, out var pendingImage)) {
                pendingImage.removeListener();
                _pendingImages.Remove(key);
                return true;
            }

            if (_cache.TryGetValue(key, out var image)) {
                _currentSizeBytes -= image.sizeBytes ?? 0;
                _cache.Remove(key);
                _lruKeys.Remove(image.node);
                return true;
            }

            return false;
        }

        void _touch(Object key, _CachedImage image) {
            // D.assert(foundation_.kReleaseMode);
            if (image.sizeBytes != null && image.sizeBytes <= maximumSizeBytes) {
                _currentSizeBytes += image.sizeBytes ?? 0;
                _cache[key] = image;
                // _checkCacheSize(timelineTask);
            }
        }

        void _trackLiveImage(Object key, _LiveImage image, bool debugPutOk = true) {
            var imageOut = _liveImages.putIfAbsent(key, () => {
                D.assert(debugPutOk);
                image.completer.addOnLastListenerRemovedCallback(image.handleRemove);
                return image;
            });
            imageOut.sizeBytes = image.sizeBytes ?? image.sizeBytes;
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
                if (image.node != null) {
                    _lruKeys.Remove(image.node);
                }
                _trackLiveImage(key, new _LiveImage(image.completer, image.sizeBytes, () => _liveImages.Remove(key)));
                image.node = _lruKeys.AddLast(key);
                return image.completer;
            }

            _liveImages.TryGetValue(key, out var liveImage);
            if (liveImage != null) {
                _touch(key, liveImage);
                return liveImage.completer;
            }

            try {
                result = loader();
                _trackLiveImage(key, new _LiveImage(result, null, () => _liveImages.Remove(key)));
            }
            catch (Exception ex) {
                if (onError != null) {
                    onError(ex);
                }
                else {
                    throw;
                }
            }
            
            _PendingImage untrackedPendingImage = null;

            void listener(ImageInfo info, bool syncCall) {
                int imageSize = info?.image == null ? 0 : info.image.height * info.image.width * 4;
                _CachedImage cachedImage = new _CachedImage(result, imageSize);

                _trackLiveImage(
                    key,
                    new _LiveImage(
                        result,
                        imageSize,
                        () => _liveImages.Remove(key)
                    ),
                    debugPutOk: syncCall
                );
                _PendingImage _pendingImage = untrackedPendingImage ?? _pendingImages.getOrDefault(key);
                _pendingImages.Remove(key);
                if (_pendingImage != null) {
                    _pendingImage.removeListener();
                }

                if (untrackedPendingImage == null) {
                    _touch(key, cachedImage);
                }
            }

            ImageStreamListener streamListener = new ImageStreamListener(listener);
            if (maximumSize > 0 && maximumSizeBytes > 0) {
                _pendingImages[key] = new _PendingImage(result, streamListener);
            }
            else {
                untrackedPendingImage = new _PendingImage(result, streamListener);
            }

            result.addListener(streamListener);

            return result;
        }

        public ImageCacheStatus statusForKey(Object key) {
            return new ImageCacheStatus(
                pending: _pendingImages.ContainsKey(key),
                keepAlive: _cache.ContainsKey(key),
                live: _liveImages.ContainsKey(key)
            );
        }

        public bool containsKey(Object key) {
            return _pendingImages[key] != null || _cache[key] != null;
        }

        int liveImageCount {
            get => _liveImages.Count;
        }

        int pendingImageCount {
            get => _pendingImages.Count;
        }

        void clearLiveImages() {
            foreach (_LiveImage
                image in _liveImages.Values) {
                image.removeListener();
            }

            _liveImages.Clear();
        }

        void _checkCacheSize() {
            Dictionary<string, object> finishArgs = new Dictionary<string, object>();
            // TimelineTask checkCacheTask;
            if (!foundation_.kReleaseMode) {
                // checkCacheTask = TimelineTask(parent: timelineTask)..start('checkCacheSize');
                finishArgs["evictedKeys"] = new List<string>();
                finishArgs["currentSize"] = currentSize;
                finishArgs["currentSizeBytes"] = currentSizeBytes;
            }

            while (_currentSizeBytes > _maximumSizeBytes || _cache.Count > _maximumSize) {
                object key = _cache.Keys.GetEnumerator().Current;
                _CachedImage image = _cache[key];
                _currentSizeBytes -= image.sizeBytes ?? 0;
                _cache.Remove(key);
                if (!foundation_.kReleaseMode) {
                    ((List<string>) finishArgs["evictedKeys"]).Add(key.ToString());
                }
            }

            if (!foundation_.kReleaseMode) {
                finishArgs["endSize"] = currentSize;
                finishArgs["endSizeBytes"] = currentSizeBytes;
                // checkCacheTask.finish(arguments: finishArgs);
            }

            D.assert(_currentSizeBytes >= 0);
            D.assert(_cache.Count <= maximumSize);
            D.assert(_currentSizeBytes <= maximumSizeBytes);
        }
    }

    public class ImageCacheStatus : IEquatable<ImageCacheStatus> {
        internal ImageCacheStatus(
            bool pending = false,
            bool keepAlive = false,
            bool live = false
        ) {
            D.assert(!pending || !keepAlive);
            this.pending = pending;
            this.keepAlive = keepAlive;
            this.live = live;
        }

        public bool pending;

        public bool keepAlive;

        public bool live;

        public bool tracked {
            get => pending || keepAlive || live;
        }

        public bool untracked {
            get => !pending && !keepAlive && !live;
        }

        public override string ToString() =>
            $"{foundation_.objectRuntimeType(this, "ImageCacheStatus")}(pending: {pending}, live: {live}, keepAlive: {keepAlive})";

        public bool Equals(ImageCacheStatus other) {
            if (ReferenceEquals(null, other)) {
                return false;
            }

            if (ReferenceEquals(this, other)) {
                return true;
            }

            return pending == other.pending && keepAlive == other.keepAlive && live == other.live;
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

            return Equals((ImageCacheStatus) obj);
        }

        public static bool operator ==(ImageCacheStatus left, object right) {
            return Equals(left, right);
        }

        public static bool operator !=(ImageCacheStatus left, object right) {
            return !Equals(left, right);
        }

        public override int GetHashCode() {
            unchecked {
                var hashCode = pending.GetHashCode();
                hashCode = (hashCode * 397) ^ keepAlive.GetHashCode();
                hashCode = (hashCode * 397) ^ live.GetHashCode();
                return hashCode;
            }
        }
    }

    class _CachedImage {
        public _CachedImage(ImageStreamCompleter completer, int? sizeBytes) {
            this.completer = completer;
            this.sizeBytes = sizeBytes;
        }

        public ImageStreamCompleter completer;
        public int? sizeBytes;
        public LinkedListNode<object> node;
    }

    class _LiveImage : _CachedImage {
        internal _LiveImage(ImageStreamCompleter completer, int? sizeBytes, VoidCallback handleRemove)
            : base(completer, sizeBytes) {
            this.handleRemove = handleRemove;
        }

        public readonly VoidCallback handleRemove;

        public void removeListener() {
            completer.removeOnLastListenerRemovedCallback(handleRemove);
        }
    }

    class _PendingImage {
        public _PendingImage(
            ImageStreamCompleter completer,
            ImageStreamListener listener
        ) {
            this.completer = completer;
            this.listener = listener;
        }

        public readonly ImageStreamCompleter completer;

        public readonly ImageStreamListener listener;

        public void removeListener() {
            completer.removeListener(listener);
        }
    }
}