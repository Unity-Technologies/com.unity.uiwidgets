using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Unity.UIWidgets.async;
using Unity.UIWidgets.engine;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.ui;
using UnityEngine.Networking;

namespace Unity.UIWidgets.services {
    public abstract class AssetBundle {
        public abstract Future<byte[]> load(string key);

        public virtual Future<string> loadString(string key, bool cache = true) {
            return load(key).then_<string>(data => {
                if (data == null)
                    throw new UIWidgetsError($"Unable to load asset: {key}");

                if (data.Length < 10 * 1024) {
                    return Encoding.UTF8.GetString(data);
                }

                return foundation_.compute(Encoding.UTF8.GetString, data, debugLabel: $"UTF8 decode for \"{key}\"");
            });
        }

        public abstract Future<T> loadStructuredData<T>(string key, Func<string, Future<T>> parser);

        public virtual void evict(string key) {
        }

        public override string ToString() => $"{foundation_.describeIdentity(this)}()";
    }

    public class NetworkAssetBundle : AssetBundle {
        public NetworkAssetBundle(Uri baseUrl, IDictionary<string, string> headers = null) {
            _baseUrl = baseUrl;
            this.headers = headers;
        }

        public readonly IDictionary<string, string> headers;

        readonly Uri _baseUrl;

        Uri _urlFromKey(string key) => new Uri(_baseUrl, key);

        IEnumerator _loadCoroutine(string key, Completer completer, Isolate isolate) {
            var url = _urlFromKey(key);
            using (var www = UnityWebRequest.Get(url)) {
                if (headers != null) {
                    foreach (var header in headers) {
                        www.SetRequestHeader(header.Key, header.Value);
                    }
                }

                yield return www.SendWebRequest();

                using (Isolate.getScope(isolate)) {
                    if (www.isNetworkError || www.isHttpError) {
                        completer.completeError(new UIWidgetsError(new List<DiagnosticsNode>() {
                            new ErrorSummary($"Unable to load asset: {key}"),
                            new StringProperty("HTTP status code", www.error)
                        }));
                        yield break;
                    }

                    var data = www.downloadHandler.data;
                    completer.complete(data);
                }
            }
        }

        public override Future<byte[]> load(string key) {
            var completer = Completer.create();
            var isolate = Isolate.current;
            var panel =UIWidgetsPanelWrapper.current.window;
            if (panel.isActive()) {
                panel.startCoroutine(_loadCoroutine(key, completer, isolate));
            }
            return completer.future.to<byte[]>();
        }

        public override Future<T> loadStructuredData<T>(string key, Func<string, Future<T>> parser) {
            D.assert(key != null);
            D.assert(parser != null);
            return loadString(key).then_<T>(value => parser(value));
        }

        public override string ToString() => $"{foundation_.describeIdentity(this)}({_baseUrl})";
    }


    public abstract class CachingAssetBundle : AssetBundle {
        readonly Dictionary<string, Future<string>> _stringCache = new Dictionary<string, Future<string>>();
        readonly Dictionary<string, Future> _structuredDataCache = new Dictionary<string, Future>();

        public override Future<string> loadString(string key, bool cache = true) {
            if (cache)
                return _stringCache.putIfAbsent(key, () => base.loadString(key));
            return base.loadString(key);
        }

        public override Future<T> loadStructuredData<T>(string key, Func<string, Future<T>> parser) {
            D.assert(key != null);
            D.assert(parser != null);
            if (_structuredDataCache.ContainsKey(key))
                return _structuredDataCache[key].to<T>();

            Completer completer = null;
            Future<T> result = null;
            loadString(key, cache: false).then_<T>(value => parser(value)).then_<object>((T value) => {
                result = new SynchronousFuture<T>(value);
                _structuredDataCache[key] = result;
                if (completer != null) {
                    // We already returned from the loadStructuredData function, which means
                    // we are in the asynchronous mode. Pass the value to the completer. The
                    // completer's future is what we returned.
                    completer.complete(FutureOr.value(value));
                }

                return FutureOr.nil;
            });

            if (result != null) {
                // The code above ran synchronously, and came up with an answer.
                // Return the SynchronousFuture that we created above.
                return result;
            }

            // The code above hasn't yet run its "then" handler yet. Let's prepare a
            // completer for it to use when it does run.
            completer = Completer.create();
            _structuredDataCache[key] = result = completer.future.to<T>();
            return result;
        }

        public override void evict(string key) {
            _stringCache.Remove(key);
            _structuredDataCache.Remove(key);
        }
    }

    public class PlatformAssetBundle : CachingAssetBundle {
        public override Future<byte[]> load(string key) {
            byte[] encoded = Encoding.UTF8.GetBytes(key);
            return ServicesBinding.instance.defaultBinaryMessenger.send(
                "uiwidgets/assets", encoded).then_<byte[]>(asset => {
                if (asset == null)
                    throw new UIWidgetsError($"Unable to load asset: {key}");
                return asset;
            });
        }
    }

    public static partial class services_ {
        static AssetBundle _initRootBundle() {
            return new PlatformAssetBundle();
        }

        public static readonly AssetBundle rootBundle = _initRootBundle();
    }
}