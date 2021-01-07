using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using Unity.UIWidgets.async2;
using Unity.UIWidgets.engine2;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.ui;
using UnityEngine;
using UnityEngine.Networking;
using Codec = Unity.UIWidgets.ui.Codec;
using Image = Unity.UIWidgets.ui.Image;
using Locale = Unity.UIWidgets.ui.Locale;
using Object = UnityEngine.Object;
using Path = System.IO.Path;
using TextDirection = Unity.UIWidgets.ui.TextDirection;
using Window = Unity.UIWidgets.ui.Window;

namespace Unity.UIWidgets.painting {
    public static partial class painting_ {
        internal delegate void _KeyAndErrorHandlerCallback<T>(T key, Action<Exception> handleError);

        internal delegate Future _AsyncKeyErrorHandler<T>(T key, Exception exception);
    }

    public class ImageConfiguration : IEquatable<ImageConfiguration> {
        public ImageConfiguration(
            AssetBundle bundle = null,
            float? devicePixelRatio = null,
            Locale locale = null,
            Size size = null,
            RuntimePlatform? platform = null
        ) {
            this.bundle = bundle;
            this.devicePixelRatio = devicePixelRatio;
            this.locale = locale;
            this.size = size;
            this.platform = platform;
        }

        public ImageConfiguration copyWith(
            AssetBundle bundle = null,
            float? devicePixelRatio = null,
            Locale locale = null,
            Size size = null,
            RuntimePlatform? platform = null
        ) {
            return new ImageConfiguration(
                bundle: bundle ? bundle : this.bundle,
                devicePixelRatio: devicePixelRatio ?? this.devicePixelRatio,
                locale: locale ?? this.locale,
                size: size ?? this.size,
                platform: platform ?? this.platform
            );
        }

        public readonly AssetBundle bundle;

        public readonly float? devicePixelRatio;

        public readonly Locale locale;

        public readonly TextDirection textDirection;

        public readonly Size size;

        public readonly RuntimePlatform? platform;

        public static readonly ImageConfiguration empty = new ImageConfiguration();

        public bool Equals(ImageConfiguration other) {
            if (ReferenceEquals(null, other)) {
                return false;
            }

            if (ReferenceEquals(this, other)) {
                return true;
            }

            return Equals(bundle, other.bundle) && devicePixelRatio.Equals(other.devicePixelRatio) &&
                   Equals(locale, other.locale) && Equals(size, other.size) &&
                   platform == other.platform;
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

            return Equals((ImageConfiguration) obj);
        }

        public override int GetHashCode() {
            unchecked {
                var hashCode = (bundle != null ? bundle.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ devicePixelRatio.GetHashCode();
                hashCode = (hashCode * 397) ^ (locale != null ? locale.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (size != null ? size.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ platform.GetHashCode();
                return hashCode;
            }
        }

        public static bool operator ==(ImageConfiguration left, ImageConfiguration right) {
            return Equals(left, right);
        }

        public static bool operator !=(ImageConfiguration left, ImageConfiguration right) {
            return !Equals(left, right);
        }

        public override string ToString() {
            var result = new StringBuilder();
            result.Append("ImageConfiguration(");
            bool hasArguments = false;
            if (bundle != null) {
                if (hasArguments) {
                    result.Append(", ");
                }

                result.Append($"bundle: {bundle}");
                hasArguments = true;
            }

            if (devicePixelRatio != null) {
                if (hasArguments) {
                    result.Append(", ");
                }

                result.Append($"devicePixelRatio: {devicePixelRatio:F1}");
                hasArguments = true;
            }

            if (locale != null) {
                if (hasArguments) {
                    result.Append(", ");
                }

                result.Append($"locale: {locale}");
                hasArguments = true;
            }

            if (size != null) {
                if (hasArguments) {
                    result.Append(", ");
                }

                result.Append($"size: {size}");
                hasArguments = true;
            }

            if (platform != null) {
                if (hasArguments) {
                    result.Append(", ");
                }

                result.Append($"platform: {platform}");
                hasArguments = true;
            }

            result.Append(")");
            return result.ToString();
        }
    }

    public delegate Future<Codec> DecoderCallback(byte[] bytes, int? cacheWidth = 0, int? cacheHeight = 0);

    public abstract class ImageProvider {
        public abstract ImageStream resolve(ImageConfiguration configuration);
        
        public static bool operator ==(ImageProvider left, ImageProvider right) {
            return Equals(left, right);
        }
        
        public static bool operator !=(ImageProvider left, ImageProvider right) {
            return !Equals(left, right);
        }
    }

    public abstract class ImageProvider<T> : ImageProvider {
        public override ImageStream resolve(ImageConfiguration configuration) {
            D.assert(configuration != null);

            ImageStream stream = new ImageStream();
            _createErrorHandlerAndKey(
                configuration,
                (T successKey, Action<Exception> errorHandler) => {
                    resolveStreamForKey(configuration, stream, successKey, (Exception e) => errorHandler(e));
                },
                (T key, Exception exception) => {
                    // await null; // wait an event turn in case a listener has been added to the image stream.
                    _ErrorImageCompleter imageCompleter = new _ErrorImageCompleter();
                    stream.setCompleter(imageCompleter);
                    InformationCollector collector = null;
                    D.assert(() => {
                        IEnumerable<DiagnosticsNode> infoCollector() {
                            yield return new DiagnosticsProperty<ImageProvider>("Image provider", this);
                            yield return new DiagnosticsProperty<ImageConfiguration>("Image configuration",
                                configuration);
                            yield return new DiagnosticsProperty<T>("Image key", key, defaultValue: null);
                        }

                        collector = infoCollector;
                        return true;
                    });
                    imageCompleter.setError(
                        exception: exception,
                        stack: exception.StackTrace,
                        context: new ErrorDescription("while resolving an image"),
                        silent: true, // could be a network error or whatnot
                        informationCollector: collector
                    );
                   return null;
                }
            );

            return stream;
        }

        void resolveStreamForKey(ImageConfiguration configuration, ImageStream stream, T key,
            ImageErrorListener handleError) {
            
            if (stream.completer != null) {
                ImageStreamCompleter completerEdge = PaintingBinding.instance.imageCache.putIfAbsent(
                    key,
                    () => stream.completer,
                    onError: handleError
                );
                D.assert(Equals(completerEdge, stream.completer));
                return;
            }

            ImageStreamCompleter completer = PaintingBinding.instance.imageCache.putIfAbsent(
                key,
                () => load(key, ui_.instantiateImageCodec),
                onError: handleError
            );
            if (completer != null) {
                stream.setCompleter(completer);
            }
        }

        public Future<bool> evict(ImageCache cache = null, ImageConfiguration configuration = null) {
            configuration = configuration ?? ImageConfiguration.empty;
            cache = cache ?? PaintingBinding.instance.imageCache;

            return obtainKey(configuration).then(key => cache.evict(key)).to<bool>();
        }

        protected abstract ImageStreamCompleter load(T assetBundleImageKey, DecoderCallback decode);

        protected abstract Future<T> obtainKey(ImageConfiguration configuration);

        private void _createErrorHandlerAndKey(
            ImageConfiguration configuration,
            painting_._KeyAndErrorHandlerCallback<T> successCallback,
            painting_._AsyncKeyErrorHandler<T> errorCallback
        ) {
            T obtainedKey = default;
            bool didError = false;

            Action<Exception> handleError = (Exception exception) => {
                if (didError) {
                    return;
                }

                if (!didError) {
                    errorCallback(obtainedKey, exception);
                }

                didError = true;
            };

            Zone dangerZone = Zone.current.fork(
                specification: new ZoneSpecification(
                    handleUncaughtError: (Zone self, ZoneDelegate parent, Zone zone, Exception error) => {
                        handleError(error);
                    }
                )
            );
            dangerZone.runGuarded(() => {
                Future<T> key;
                try {
                    key = obtainKey(configuration);
                }
                catch (Exception error) {
                    handleError(error);
                    return null;
                }

                key.then_((T reusltKey) => {
                    obtainedKey = reusltKey;
                    try {
                        successCallback(reusltKey, handleError);
                    }
                    catch (Exception error) {
                        handleError(error);
                    }
                }).catchError(handleError);
                return null;
            });
        }
    }

    public class AssetBundleImageKey : IEquatable<AssetBundleImageKey> {
        public AssetBundleImageKey(
            AssetBundle bundle,
            string name,
            float scale
        ) {
            D.assert(name != null);
            D.assert(scale >= 0.0);

            this.bundle = bundle;
            this.name = name;
            this.scale = scale;
        }

        public readonly AssetBundle bundle;

        public readonly string name;

        public readonly float scale;

        public bool Equals(AssetBundleImageKey other) {
            if (ReferenceEquals(null, other)) {
                return false;
            }

            if (ReferenceEquals(this, other)) {
                return true;
            }

            return Equals(bundle, other.bundle) && string.Equals(name, other.name) &&
                   scale.Equals(other.scale);
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

            return Equals((AssetBundleImageKey) obj);
        }

        public override int GetHashCode() {
            unchecked {
                var hashCode = (bundle != null ? bundle.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (name != null ? name.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ scale.GetHashCode();
                return hashCode;
            }
        }

        public static bool operator ==(AssetBundleImageKey left, AssetBundleImageKey right) {
            return Equals(left, right);
        }

        public static bool operator !=(AssetBundleImageKey left, AssetBundleImageKey right) {
            return !Equals(left, right);
        }

        public override string ToString() {
            return $"{GetType()}(bundle: {bundle}, name: \"{name}\", scale: {scale})";
        }
    }

    public abstract class AssetBundleImageProvider : ImageProvider<AssetBundleImageKey> {
        protected AssetBundleImageProvider() {
        }

        protected override ImageStreamCompleter load(AssetBundleImageKey key, DecoderCallback decode) {
            IEnumerable<DiagnosticsNode> infoCollector() {
                yield return new DiagnosticsProperty<ImageProvider>("Image provider", this);
                yield return new DiagnosticsProperty<AssetBundleImageKey>("Image key", key);
            }
            
            return new MultiFrameImageStreamCompleter(
                codec: _loadAsync(key, decode),
                scale: key.scale,
                informationCollector: infoCollector
            );
        }

        Future<Codec> _loadAsync(AssetBundleImageKey key, DecoderCallback decode) {
            Object data;
            // Hot reload/restart could change whether an asset bundle or key in a
            // bundle are available, or if it is a network backed bundle.
            try {
                data = key.bundle.LoadAsset(key.name);
            }
            catch (Exception e) {
                PaintingBinding.instance.imageCache.evict(key);
                throw e;
            }

            if (data != null && data is Texture2D textureData) {
                return decode(textureData.EncodeToPNG());
            }
            else {
                PaintingBinding.instance.imageCache.evict(key);
                throw new Exception("Unable to read data");
            }
        }

        IEnumerator _loadAssetAsync(AssetBundleImageKey key) {
            if (key.bundle == null) {
                ResourceRequest request = Resources.LoadAsync(key.name);
                if (request.asset) {
                    yield return request.asset;
                }
                else {
                    yield return request;
                    yield return request.asset;
                }
            }
            else {
                AssetBundleRequest request = key.bundle.LoadAssetAsync(key.name);
                if (request.asset) {
                    yield return request.asset;
                }
                else {
                    yield return request.asset;
                }
            }
        }
    }

    public class NetworkImage : ImageProvider<NetworkImage>, IEquatable<NetworkImage> {
        public NetworkImage(string url,
            float scale = 1.0f,
            IDictionary<string, string> headers = null) {
            D.assert(url != null);
            this.url = url;
            this.scale = scale;
            this.headers = headers;
        }

        public readonly string url;

        public readonly float scale;

        public readonly IDictionary<string, string> headers;

        protected override Future<NetworkImage> obtainKey(ImageConfiguration configuration) {
            return new SynchronousFuture<NetworkImage>(this);
        }

        protected override ImageStreamCompleter load(NetworkImage key, DecoderCallback decode) {
            IEnumerable<DiagnosticsNode> infoCollector() {
                yield return new ErrorDescription($"url: {url}");
            }
            
            return new MultiFrameImageStreamCompleter(
                codec: _loadAsync(key, decode),
                scale: key.scale,
                informationCollector: infoCollector
            );
        }

        Future<Codec> _loadAsync(NetworkImage key, DecoderCallback decode) {
            var completer = Completer.create();
            var isolate = Isolate.current;
            var panel = UIWidgetsPanel.current;
            panel.StartCoroutine(_loadCoroutine(key.url, completer, isolate)); 
            return completer.future.to<byte[]>().then_<byte[]>(data => {
                if (data != null && data.Length > 0) {
                    return decode(data);
                }
                throw new Exception("not loaded");
            }).to<Codec>();
        }
        
        IEnumerator _loadCoroutine(string key, Completer completer, Isolate isolate) {
            var url = new Uri(key);
            using (var www = UnityWebRequest.Get(url)) {
                if (headers != null) {
                    foreach (var header in headers) {
                        www.SetRequestHeader(header.Key, header.Value);
                    }
                }

                yield return www.SendWebRequest();

                if (www.isNetworkError || www.isHttpError) {
                    completer.completeError(new Exception($"Failed to load from url \"{url}\": {www.error}"));
                    yield break;
                }

                var data = www.downloadHandler.data;

                using (Isolate.getScope(isolate)) {
                    completer.complete(data);
                }
            }
        }

        IEnumerator _loadBytes(NetworkImage key) {
            D.assert(key == this);
            var uri = new Uri(key.url);

            if (uri.LocalPath.EndsWith(".gif")) {
                using (var www = UnityWebRequest.Get(uri)) {
                    if (headers != null) {
                        foreach (var header in headers) {
                            www.SetRequestHeader(header.Key, header.Value);
                        }
                    }

                    yield return www.SendWebRequest();

                    if (www.isNetworkError || www.isHttpError) {
                        throw new Exception($"Failed to load from url \"{uri}\": {www.error}");
                    }

                    var data = www.downloadHandler.data;
                    yield return data;
                }

                yield break;
            }

            using (var www = UnityWebRequestTexture.GetTexture(uri)) {
                if (headers != null) {
                    foreach (var header in headers) {
                        www.SetRequestHeader(header.Key, header.Value);
                    }
                }

                yield return www.SendWebRequest();

                if (www.isNetworkError || www.isHttpError) {
                    throw new Exception($"Failed to load from url \"{uri}\": {www.error}");
                }

                var data = ((DownloadHandlerTexture) www.downloadHandler).texture;
                yield return data;
            }
        }

        public bool Equals(NetworkImage other) {
            if (ReferenceEquals(null, other)) {
                return false;
            }

            if (ReferenceEquals(this, other)) {
                return true;
            }

            return string.Equals(url, other.url) && scale.Equals(other.scale);
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

            return Equals((NetworkImage) obj);
        }

        public override int GetHashCode() {
            unchecked {
                return ((url != null ? url.GetHashCode() : 0) * 397) ^ scale.GetHashCode();
            }
        }

        public static bool operator ==(NetworkImage left, NetworkImage right) {
            return Equals(left, right);
        }

        public static bool operator !=(NetworkImage left, NetworkImage right) {
            return !Equals(left, right);
        }

        public override string ToString() {
            return $"runtimeType(\"{url}\", scale: {scale})";
        }
    }

    public class FileImage : ImageProvider<FileImage>, IEquatable<FileImage> {
        public FileImage(string file, float scale = 1.0f) {
            D.assert(file != null);
            this.file = file;
            this.scale = scale;
        }

        public readonly string file;

        public readonly float scale;

        protected override Future<FileImage> obtainKey(ImageConfiguration configuration) {
            return Future.value(FutureOr.value(this)).to<FileImage>();
        }

        protected override ImageStreamCompleter load(FileImage key, DecoderCallback decode) {
            IEnumerable<DiagnosticsNode> infoCollector() {
                yield return new ErrorDescription($"Path: {file}");
            }
            
            return new MultiFrameImageStreamCompleter(_loadAsync(key, decode),
                scale: key.scale,
                informationCollector: infoCollector);
        }

        Future<Codec> _loadAsync(FileImage key, DecoderCallback decode) {
            byte[] bytes = File.ReadAllBytes(Path.Combine(Application.streamingAssetsPath, key.file));
            if (bytes != null && bytes.Length > 0 ) {
                return decode(bytes);
            }
            throw new Exception("not loaded");
        }

        IEnumerator _loadBytes(FileImage key) {
            D.assert(key == this);
            var uri = "file://" + key.file;

            if (uri.EndsWith(".gif")) {
                using (var www = UnityWebRequest.Get(uri)) {
                    yield return www.SendWebRequest();

                    if (www.isNetworkError || www.isHttpError) {
                        throw new Exception($"Failed to get file \"{uri}\": {www.error}");
                    }

                    var data = www.downloadHandler.data;
                    yield return data;
                }

                yield break;
            }

            using (var www = UnityWebRequestTexture.GetTexture(uri)) {
                yield return www.SendWebRequest();

                if (www.isNetworkError || www.isHttpError) {
                    throw new Exception($"Failed to get file \"{uri}\": {www.error}");
                }

                var data = ((DownloadHandlerTexture) www.downloadHandler).texture;
                yield return data;
            }
        }

        public bool Equals(FileImage other) {
            if (ReferenceEquals(null, other)) {
                return false;
            }

            if (ReferenceEquals(this, other)) {
                return true;
            }

            return string.Equals(file, other.file) && scale.Equals(other.scale);
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

            return Equals((FileImage) obj);
        }

        public override int GetHashCode() {
            unchecked {
                return ((file != null ? file.GetHashCode() : 0) * 397) ^ scale.GetHashCode();
            }
        }

        public static bool operator ==(FileImage left, FileImage right) {
            return Equals(left, right);
        }

        public static bool operator !=(FileImage left, FileImage right) {
            return !Equals(left, right);
        }

        public override string ToString() {
            return $"{GetType()}(\"{file}\", scale: {scale})";
        }
    }

    public class MemoryImage : ImageProvider<MemoryImage>, IEquatable<MemoryImage> {
        public MemoryImage(byte[] bytes, float scale = 1.0f) {
            D.assert(bytes != null);
            this.bytes = bytes;
            this.scale = scale;
        }

        public readonly byte[] bytes;

        public readonly float scale;

        protected override Future<MemoryImage> obtainKey(ImageConfiguration configuration) {
            return Future.value(FutureOr.value(this)).to<MemoryImage>();
        }

        protected override ImageStreamCompleter load(MemoryImage key, DecoderCallback decode) {
            return new MultiFrameImageStreamCompleter(
                _loadAsync(key, decode),
                scale: key.scale);
        }

        Future<Codec> _loadAsync(MemoryImage key, DecoderCallback decode) {
            D.assert(key == this);

            return decode(bytes);
        }

        public bool Equals(MemoryImage other) {
            if (ReferenceEquals(null, other)) {
                return false;
            }

            if (ReferenceEquals(this, other)) {
                return true;
            }

            return Equals(bytes, other.bytes) && scale.Equals(other.scale);
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

            return Equals((MemoryImage) obj);
        }

        public override int GetHashCode() {
            unchecked {
                return ((bytes != null ? bytes.GetHashCode() : 0) * 397) ^ scale.GetHashCode();
            }
        }

        public static bool operator ==(MemoryImage left, MemoryImage right) {
            return Equals(left, right);
        }

        public static bool operator !=(MemoryImage left, MemoryImage right) {
            return !Equals(left, right);
        }

        public override string ToString() {
            return $"{GetType()}({foundation_.describeIdentity(bytes)}), scale: {scale}";
        }
    }

    public class ExactAssetImage : AssetBundleImageProvider, IEquatable<ExactAssetImage> {
        public ExactAssetImage(
            string assetName,
            float scale = 1.0f,
            AssetBundle bundle = null
        ) {
            D.assert(assetName != null);
            this.assetName = assetName;
            this.scale = scale;
            this.bundle = bundle;
        }

        public readonly string assetName;

        public readonly float scale;

        public readonly AssetBundle bundle;

        protected override Future<AssetBundleImageKey> obtainKey(ImageConfiguration configuration) {
            return Future.value(FutureOr.value(new AssetBundleImageKey(
                bundle: bundle ? bundle : configuration.bundle,
                name: assetName,
                scale: scale
            ))).to<AssetBundleImageKey>();
        }

        public bool Equals(ExactAssetImage other) {
            if (ReferenceEquals(null, other)) {
                return false;
            }

            if (ReferenceEquals(this, other)) {
                return true;
            }

            return string.Equals(assetName, other.assetName) && scale.Equals(other.scale) &&
                   Equals(bundle, other.bundle);
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

            return Equals((ExactAssetImage) obj);
        }

        public override int GetHashCode() {
            unchecked {
                var hashCode = (assetName != null ? assetName.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ scale.GetHashCode();
                hashCode = (hashCode * 397) ^ (bundle != null ? bundle.GetHashCode() : 0);
                return hashCode;
            }
        }

        public static bool operator ==(ExactAssetImage left, ExactAssetImage right) {
            return Equals(left, right);
        }

        public static bool operator !=(ExactAssetImage left, ExactAssetImage right) {
            return !Equals(left, right);
        }

        public override string ToString() {
            return $"{GetType()}(name: \"{assetName}\", scale: {scale}, bundle: {bundle})";
        }
    }

    internal class _ErrorImageCompleter : ImageStreamCompleter {
        internal _ErrorImageCompleter() {
        }

        public void setError(
            DiagnosticsNode context,
            Exception exception,
            string stack,
            InformationCollector informationCollector,
            bool silent = false
        ) {
            reportError(
                context: context,
                exception: exception,
                informationCollector: informationCollector,
                silent: silent
            );
        }
    }
}