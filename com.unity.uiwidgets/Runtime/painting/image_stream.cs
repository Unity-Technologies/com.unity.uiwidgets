using System;
using System.Collections.Generic;
using System.Linq;
using Unity.UIWidgets.async;
using Unity.UIWidgets.external;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.scheduler;
using Unity.UIWidgets.ui;
using SchedulerBinding = Unity.UIWidgets.scheduler.SchedulerBinding;

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

            return image.Equals(other.image) && scale.Equals(other.scale);
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


    public class ImageStreamListener : IEquatable<ImageStreamListener> {
        public ImageStreamListener(
            ImageListener onImage,
            ImageChunkListener onChunk = null,
            ImageErrorListener onError = null
        ) {
            D.assert(onImage != null);
            this.onImage = onImage;
            this.onChunk = onChunk;
            this.onError = onError;
        }

        public readonly ImageListener onImage;

        public readonly ImageChunkListener onChunk;

        public readonly ImageErrorListener onError;

        public bool Equals(ImageStreamListener other) {
            if (ReferenceEquals(null, other)) {
                return false;
            }

            if (ReferenceEquals(this, other)) {
                return true;
            }

            return Equals(onImage, other.onImage) && Equals(onChunk, other.onChunk) && Equals(onError, other.onError);
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

            return Equals((ImageStreamListener) obj);
        }

        public static bool operator ==(ImageStreamListener left, ImageStreamListener right) {
            return Equals(left, right);
        }

        public static bool operator !=(ImageStreamListener left, ImageStreamListener right) {
            return !Equals(left, right);
        }

        public override int GetHashCode() {
            unchecked {
                var hashCode = (onImage != null ? onImage.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (onChunk != null ? onChunk.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (onError != null ? onError.GetHashCode() : 0);
                return hashCode;
            }
        }
    }

    public delegate void ImageListener(ImageInfo image, bool synchronousCall);

    public delegate void ImageChunkListener(ImageChunkEvent evt);

    public delegate void ImageErrorListener(Exception exception);

    public class ImageChunkEvent : Diagnosticable {
        public ImageChunkEvent(
            int cumulativeBytesLoaded,
            int expectedTotalBytes
        ) {
            D.assert(cumulativeBytesLoaded >= 0);
            D.assert(expectedTotalBytes >= 0);
            this.cumulativeBytesLoaded = cumulativeBytesLoaded;
            this.expectedTotalBytes = expectedTotalBytes;
        }

        public readonly int cumulativeBytesLoaded;

        public readonly int expectedTotalBytes;

        public override
            void debugFillProperties(DiagnosticPropertiesBuilder properties) {
            base.debugFillProperties(properties);
            properties.add(new IntProperty("cumulativeBytesLoaded", cumulativeBytesLoaded));
            properties.add(new IntProperty("expectedTotalBytes", expectedTotalBytes));
        }
    }

    public class ImageStream : Diagnosticable {
        public ImageStream() {
        }

        ImageStreamCompleter _completer;

        public ImageStreamCompleter completer {
            get { return _completer; }
        }

        List<ImageStreamListener> _listeners;

        public void setCompleter(ImageStreamCompleter value) {
            D.assert(_completer == null);

            _completer = value;
            if (_listeners != null) {
                var initialListeners = _listeners;
                _listeners = null;
                initialListeners.ForEach(_completer.addListener);
            }
        }

        public void addListener(ImageStreamListener listener) {
            if (_completer != null) {
                _completer.addListener(listener);
                return;
            }

            if (_listeners == null) {
                _listeners = new List<ImageStreamListener>();
            }

            _listeners.Add(listener);
        }

        public void removeListener(ImageStreamListener listener) {
            if (_completer != null) {
                _completer.removeListener(listener);
                return;
            }

            D.assert(_listeners != null);
            for (int i = 0; i < _listeners.Count; i++) {
                if (_listeners[i] == listener) {
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
            properties.add(new ObjectFlagProperty<List<ImageStreamListener>>(
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
        internal readonly List<ImageStreamListener> _listeners = new List<ImageStreamListener>();
        internal ImageInfo _currentImage;
        internal UIWidgetsErrorDetails _currentError;

        protected bool hasListeners {
            get { return _listeners.isNotEmpty(); }
        }

        public virtual void addListener(ImageStreamListener listener) {
            _listeners.Add(listener);
            if (_currentImage != null) {
                try {
                    listener.onImage(_currentImage, true);
                }
                catch (Exception exception) {
                    reportError(
                        context: new ErrorDescription("by a synchronously-called image listener"),
                        exception: exception
                    );
                }
            }

            if (_currentError != null && listener.onError != null) {
                try {
                    listener.onError(_currentError.exception);
                }
                catch (Exception exception) {
                    UIWidgetsError.reportError(
                        new UIWidgetsErrorDetails(
                            exception: exception,
                            library: "image resource service",
                            context: new ErrorDescription("by a synchronously-called image error listener")
                        )
                    );
                }
            }
        }

        public virtual void removeListener(ImageStreamListener listener) {
            for (int i = 0; i < _listeners.Count; i += 1) {
                if (_listeners[i] == listener) {
                    _listeners.RemoveAt(i);
                    break;
                }
            }

            if (_listeners.isEmpty()) {
                foreach (VoidCallback callback in _onLastListenerRemovedCallbacks) {
                    callback();
                }

                _onLastListenerRemovedCallbacks.Clear();
            }
        }

        readonly List<VoidCallback> _onLastListenerRemovedCallbacks = new List<VoidCallback>();

        public void addOnLastListenerRemovedCallback(VoidCallback callback) {
            D.assert(callback != null);
            _onLastListenerRemovedCallbacks.Add(callback);
        }
        
        public void removeOnLastListenerRemovedCallback(VoidCallback callback) {
            D.assert(callback != null);
            _onLastListenerRemovedCallbacks.Remove(callback);
        }


        protected void setImage(ImageInfo image) {
            _currentImage = image;
            if (_listeners.isEmpty()) {
                return;
            }

            var localListeners = LinqUtils<ImageStreamListener>.SelectList(_listeners,(l => l));
            foreach (var listener in localListeners) {
                try {
                    listener.onImage(image, false);
                }
                catch (Exception ex) {
                    reportError(
                        context: new ErrorDescription("by an image listener"),
                        exception: ex
                    );
                }
            }
        }

        protected void reportError(
            DiagnosticsNode context = null,
            Exception exception = null,
            InformationCollector informationCollector = null,
            bool silent = false) {
            _currentError = new UIWidgetsErrorDetails(
                exception: exception,
                library: "image resource service",
                context: context,
                informationCollector: informationCollector,
                silent: silent
            );
            
            var localErrorListeners = LinqUtils<ImageErrorListener>.WhereList(
                LinqUtils<ImageErrorListener,ImageStreamListener>.SelectList(_listeners, (l => l.onError)),
                (l => l != null));
            
            if (localErrorListeners.isEmpty()) {
                UIWidgetsError.reportError(_currentError);
            }
            else {
                foreach (var errorListener in localErrorListeners) {
                    try {
                        errorListener(exception);
                    }
                    catch (Exception ex) {
                        UIWidgetsError.reportError(
                            new UIWidgetsErrorDetails(
                                context: new ErrorDescription("when reporting an error to an image listener"),
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
                "current", _currentImage, ifNull: "unresolved", showName: false));
            description.add(new ObjectFlagProperty<List<ImageStreamListener>>(
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
                    context: new ErrorDescription("resolving a single-frame image stream"),
                    exception: err,
                    informationCollector: informationCollector,
                    silent: true
                );
            });
        }
    }

    // TODO: update stream
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
                    context: new ErrorDescription("resolving an image codec"),
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
            _timer = Timer.create(delay, () => _scheduleAppFrame());
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

        public override void addListener(ImageStreamListener listener) {
            if (!hasListeners && _codec != null) {
                _decodeNextFrameAndSchedule();
            }

            base.addListener(listener);
        }

        public override void removeListener(ImageStreamListener listener) {
            base.removeListener(listener);
            if (!hasListeners) {
                _timer?.cancel();
                _timer = null;
            }
        }
    }
}