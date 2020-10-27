using System;
using System.Collections.Generic;
using System.Linq;
using Unity.UIWidgets.async2;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.scheduler2;
using Unity.UIWidgets.ui;
using SchedulerBinding = Unity.UIWidgets.scheduler2.SchedulerBinding;

namespace Unity.UIWidgets.painting {
    public class ImageInfo : IEquatable<ImageInfo> {
        public ImageInfo(Image image, float scale = 1.0f) {
            D.assert(image != null);

            this.image = image;
            this.scale = scale;
        }

        public readonly Image image;
        public readonly float scale;

        public bool Equals(ImageInfo other) {
            if (ReferenceEquals(null, other)) {
                return false;
            }

            if (ReferenceEquals(this, other)) {
                return true;
            }

            return Equals(image, other.image) && scale.Equals(other.scale);
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

            return Equals((ImageInfo) obj);
        }

        public override int GetHashCode() {
            unchecked {
                return ((image != null ? image.GetHashCode() : 0) * 397) ^ scale.GetHashCode();
            }
        }

        public static bool operator ==(ImageInfo left, ImageInfo right) {
            return Equals(left, right);
        }

        public static bool operator !=(ImageInfo left, ImageInfo right) {
            return !Equals(left, right);
        }

        public override string ToString() {
            return $"{image} @ {scale}x";
        }
    }

    public delegate void ImageListener(ImageInfo image, bool synchronousCall);

    public delegate void ImageErrorListener(Exception exception);

    class _ImageListenerPair {
        public ImageListener listener;
        public ImageErrorListener errorListener;
    }

    public class ImageStream : Diagnosticable {
        public ImageStream() {
        }

        ImageStreamCompleter _completer;

        public ImageStreamCompleter completer {
            get { return _completer; }
        }

        List<_ImageListenerPair> _listeners;

        public void setCompleter(ImageStreamCompleter value) {
            D.assert(_completer == null);

            _completer = value;
            if (_listeners != null) {
                var initialListeners = _listeners;
                _listeners = null;
                foreach (_ImageListenerPair listenerPair in initialListeners) {
                    _completer.addListener(
                        listenerPair.listener,
                        listenerPair.errorListener
                    );
                }
            }
        }

        public void addListener(ImageListener listener, ImageErrorListener onError = null) {
            if (_completer != null) {
                _completer.addListener(listener, onError);
                return;
            }

            if (_listeners == null) {
                _listeners = new List<_ImageListenerPair>();
            }

            _listeners.Add(new _ImageListenerPair {listener = listener, errorListener = onError});
        }

        public void removeListener(ImageListener listener) {
            if (_completer != null) {
                _completer.removeListener(listener);
                return;
            }

            D.assert(_listeners != null);
            for (int i = 0; i < _listeners.Count; i++) {
                if (_listeners[i].listener == listener) {
                    _listeners.RemoveAt(i);
                    break;
                }
            }
        }

        public object key {
            get { return _completer != null ? (object) _completer : this; }
        }

        public override void debugFillProperties(DiagnosticPropertiesBuilder properties) {
            base.debugFillProperties(properties);
            properties.add(new ObjectFlagProperty<ImageStreamCompleter>(
                "completer",
                _completer,
                ifPresent: _completer?.toStringShort(),
                ifNull: "unresolved"
            ));
            properties.add(new ObjectFlagProperty<List<_ImageListenerPair>>(
                "listeners",
                _listeners,
                ifPresent: $"{_listeners?.Count} listener{(_listeners?.Count == 1 ? "" : "s")}",
                ifNull: "no listeners",
                level: _completer != null ? DiagnosticLevel.hidden : DiagnosticLevel.info
            ));
            _completer?.debugFillProperties(properties);
        }
    }

    public abstract class ImageStreamCompleter : Diagnosticable {
        internal readonly List<_ImageListenerPair> _listeners = new List<_ImageListenerPair>();
        public ImageInfo currentImage;
        public UIWidgetsErrorDetails currentError;

        protected bool hasListeners {
            get { return _listeners.isNotEmpty(); }
        }

        public virtual void addListener(ImageListener listener, ImageErrorListener onError = null) {
            _listeners.Add(new _ImageListenerPair {listener = listener, errorListener = onError});

            if (currentImage != null) {
                try {
                    listener(currentImage, true);
                }
                catch (Exception ex) {
                    reportError(
                        context: "by a synchronously-called image listener",
                        exception: ex
                    );
                }
            }

            if (currentError != null && onError != null) {
                try {
                    onError(currentError.exception);
                }
                catch (Exception ex) {
                    UIWidgetsError.reportError(
                        new UIWidgetsErrorDetails(
                            exception: ex,
                            library: "image resource service",
                            context: "when reporting an error to an image listener"
                        )
                    );
                }
            }
        }

        public virtual void removeListener(ImageListener listener) {
            for (int i = 0; i < _listeners.Count; i++) {
                if (_listeners[i].listener == listener) {
                    _listeners.RemoveAt(i);
                    break;
                }
            }
        }

        protected void setImage(ImageInfo image) {
            currentImage = image;
            if (_listeners.isEmpty()) {
                return;
            }

            var localListeners = _listeners.Select(l => l.listener).ToList();
            foreach (var listener in localListeners) {
                try {
                    listener(image, false);
                }
                catch (Exception ex) {
                    reportError(
                        context: "by an image listener",
                        exception: ex
                    );
                }
            }
        }

        protected void reportError(
            string context = null,
            Exception exception = null,
            InformationCollector informationCollector = null,
            bool silent = false) {
            currentError = new UIWidgetsErrorDetails(
                exception: exception,
                library: "image resource service",
                context: context,
                informationCollector: informationCollector,
                silent: silent
            );

            var localErrorListeners = _listeners.Select(l => l.errorListener).Where(l => l != null).ToList();

            if (localErrorListeners.isEmpty()) {
                UIWidgetsError.reportError(currentError);
            }
            else {
                foreach (var errorListener in localErrorListeners) {
                    try {
                        errorListener(exception);
                    }
                    catch (Exception ex) {
                        UIWidgetsError.reportError(
                            new UIWidgetsErrorDetails(
                                context: "when reporting an error to an image listener",
                                library: "image resource service",
                                exception: ex
                            )
                        );
                    }
                }
            }
        }

        public override void debugFillProperties(DiagnosticPropertiesBuilder description) {
            base.debugFillProperties(description);
            description.add(new DiagnosticsProperty<ImageInfo>(
                "current", currentImage, ifNull: "unresolved", showName: false));
            description.add(new ObjectFlagProperty<List<_ImageListenerPair>>(
                "listeners",
                _listeners,
                ifPresent: $"{_listeners.Count} listener{(_listeners.Count == 1 ? "" : "s")}"
            ));
        }
    }

    public class OneFrameImageStreamCompleter : ImageStreamCompleter {
        public OneFrameImageStreamCompleter(Future<ImageInfo> image,
            InformationCollector informationCollector = null) {
            D.assert(image != null);

            image.then_(result => { setImage(result); }).catchError(err => {
                reportError(
                    context: "resolving a single-frame image stream",
                    exception: err,
                    informationCollector: informationCollector,
                    silent: true
                );
            });
        }
    }

    public class MultiFrameImageStreamCompleter : ImageStreamCompleter {
        public MultiFrameImageStreamCompleter(
            Future<Codec> codec,
            float scale,
            InformationCollector informationCollector = null
        ) {
            D.assert(codec != null);

            _scale = scale;
            _informationCollector = informationCollector;

            codec.then_((Action<Codec>) _handleCodecReady, ex => {
                reportError(
                    context: "resolving an image codec",
                    exception: ex,
                    informationCollector: informationCollector,
                    silent: true
                );
                return FutureOr.nil;
            });
        }

        Codec _codec;
        readonly float _scale;
        readonly InformationCollector _informationCollector;

        FrameInfo _nextFrame;
        TimeSpan? _shownTimestamp;
        TimeSpan? _frameDuration;
        int _framesEmitted = 0;
        Timer _timer;

        bool _frameCallbackScheduled = false;

        void _handleCodecReady(Codec codec) {
            _codec = codec;
            D.assert(_codec != null);

            if (hasListeners) {
                _decodeNextFrameAndSchedule();
            }
        }

        void _handleAppFrame(TimeSpan timestamp) {
            _frameCallbackScheduled = false;
            if (!hasListeners) {
                return;
            }

            if (_isFirstFrame() || _hasFrameDurationPassed(timestamp)) {
                _emitFrame(new ImageInfo(image: _nextFrame.image, scale: _scale));
                _shownTimestamp = timestamp;
                _frameDuration = _nextFrame.duration;
                _nextFrame = null;
                int completedCycles = _codec.frameCount == 0 ? 0 : _framesEmitted / _codec.frameCount;

                if (_codec.repetitionCount == -1 || completedCycles <= _codec.repetitionCount) {
                    _decodeNextFrameAndSchedule();
                }

                return;
            }

            TimeSpan delay = _frameDuration.Value - (timestamp - _shownTimestamp.Value);
            delay = new TimeSpan((long) (delay.Ticks * scheduler_.timeDilation));
            // TODO: time dilation 
            _timer = Timer.create(delay , ()=>  _scheduleAppFrame());
        }

        bool _isFirstFrame() {
            return _frameDuration == null;
        }

        bool _hasFrameDurationPassed(TimeSpan timestamp) {
            D.assert(_shownTimestamp != null);
            return timestamp - _shownTimestamp >= _frameDuration;
        }

        Future _decodeNextFrameAndSchedule() {
            var frame = _codec.getNextFrame();
            return frame.then_(info => {
                _nextFrame = info;
                if (_codec.frameCount == 1) {
                    _emitFrame(new ImageInfo(image: _nextFrame.image, scale: _scale));
                    return;
                }

                _scheduleAppFrame();
            });
        }

        void _scheduleAppFrame() {
            if (_frameCallbackScheduled) {
                return;
            }

            _frameCallbackScheduled = true;
            SchedulerBinding.instance.scheduleFrameCallback(_handleAppFrame);
        }

        void _emitFrame(ImageInfo imageInfo) {
            setImage(imageInfo);
            _framesEmitted += 1;
        }

        public override void addListener(ImageListener listener, ImageErrorListener onError = null) {
            if (!hasListeners && _codec != null) {
                _decodeNextFrameAndSchedule();
            }

            base.addListener(listener, onError: onError);
        }

        public override void removeListener(ImageListener listener) {
            base.removeListener(listener);
            if (!hasListeners) {
                _timer?.cancel();
                _timer = null;
            }
        }
    }
}