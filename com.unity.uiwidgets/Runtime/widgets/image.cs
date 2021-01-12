using System;
using System.Collections.Generic;
using System.Diagnostics;
using Unity.UIWidgets.async2;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.scheduler2;
using Unity.UIWidgets.ui;
using UnityEngine;
using Color = Unity.UIWidgets.ui.Color;
using Object = System.Object;
using Rect = Unity.UIWidgets.ui.Rect;

namespace Unity.UIWidgets.widgets {
    public class ImageUtils {
        public static ImageConfiguration createLocalImageConfiguration(BuildContext context, Size size = null) {
            return new ImageConfiguration(
                bundle: DefaultAssetBundle.of(context),
                devicePixelRatio: MediaQuery.of(context, nullOk: true)?.devicePixelRatio ?? 1.0f,
                //locale: Localizations.localeOf(context, nullOk: true),
                size: size,
                platform: Application.platform
            );
        }

        public Future precacheImage(
            ImageProvider provider,
            BuildContext context,
            Size size = null,
            ImageErrorListener onError = null
        ) {
            ImageConfiguration config = createLocalImageConfiguration(context, size: size);
            var completer = Completer.create();
            ImageStream stream = provider.resolve(config);

            void listener(ImageInfo image, bool sync) {
                if (!completer.isCompleted) {
                    stream.removeListener(listener);
                }

                SchedulerBinding.instance.addPostFrameCallback(timeStamp => { stream.removeListener(listener); });
            }

            void errorListener(Exception exception) {
                if (!completer.isCompleted) {
                    completer.complete();
                }

                stream.removeListener(listener);
                if (onError != null) {
                    onError(exception);
                }
                else {
                    UIWidgetsError.reportError(new UIWidgetsErrorDetails(
                        context: new ErrorDescription("image failed to precache"),
                        library: "image resource service",
                        exception: exception,
                        silent: true
                    ));
                }
            }

            stream.addListener(listener: listener, onError: errorListener);
            return completer.future;
        }
    }

    public delegate Widget ImageFrameBuilder (
    BuildContext context,
    Widget child,
    int frame,
    bool wasSynchronouslyLoaded
    );
    
    // [!!!] class ImageChunkEvent is missing
    public delegate Widget ImageLoadingBuilder(
    BuildContext context,
    Widget child//,
    //ImageChunkEvent loadingProgress
    );
    
    public delegate Widget ImageErrorWidgetBuilder(
        BuildContext context,
        Object error,
        StackTrace stackTrace
    );
    
    public class Image : StatefulWidget {
        public Image(
            Key key = null,
            ImageProvider image = null,
            ImageFrameBuilder frameBuilder = null,
            ImageLoadingBuilder loadingBuilder = null,
            ImageErrorWidgetBuilder errorBuilder = null,
            float? width = null,
            float? height = null,
            Color color = null,
            BlendMode colorBlendMode = BlendMode.srcIn,
            BoxFit? fit = null,
            Alignment alignment = null,
            ImageRepeat repeat = ImageRepeat.noRepeat,
            Rect centerSlice = null,
            bool gaplessPlayback = false,
            FilterQuality filterQuality = FilterQuality.low
        ) : base(key: key) {
            D.assert(image != null);
            this.image = image;
            this.frameBuilder = frameBuilder;
            this.loadingBuilder = loadingBuilder;
            this.errorBuilder = errorBuilder;
            this.width = width;
            this.height = height;
            this.color = color;
            this.colorBlendMode = colorBlendMode;
            this.fit = fit;
            this.alignment = alignment ?? Alignment.center;
            this.repeat = repeat;
            this.centerSlice = centerSlice;
            this.gaplessPlayback = gaplessPlayback;
            this.filterQuality = filterQuality;
        }

        public static Image network(
            string src,
            Key key = null,
            float scale = 1.0f,
            ImageFrameBuilder frameBuilder = null,
            ImageLoadingBuilder loadingBuilder = null,
            ImageErrorWidgetBuilder errorBuilder = null,
            float? width = null,
            float? height = null,
            Color color = null,
            BlendMode colorBlendMode = BlendMode.srcIn,
            BoxFit? fit = null,
            Alignment alignment = null,
            ImageRepeat repeat = ImageRepeat.noRepeat,
            Rect centerSlice = null,
            bool gaplessPlayback = false,
            FilterQuality filterQuality = FilterQuality.low,
            IDictionary<string, string> headers = null,
            int cacheWidth = default,
            int cacheHeight = default
        ) {
            var networkImage = new NetworkImage(src, scale, headers);//image = ResizeImage.resizeIfNeeded(cacheWidth, cacheHeight, NetworkImage(src, scale: scale, headers: headers));
            return new Image(
                key: key,
                image: networkImage,
                frameBuilder: frameBuilder,
                loadingBuilder: loadingBuilder,
                errorBuilder: errorBuilder,
                width: width,
                height: height,
                color: color,
                colorBlendMode: colorBlendMode,
                fit: fit,
                alignment: alignment,
                repeat: repeat,
                centerSlice: centerSlice,
                gaplessPlayback: gaplessPlayback,
                filterQuality: filterQuality
            );
        }

        public static Image file(
            string file,
            Key key = null,
            float scale = 1.0f,
            ImageFrameBuilder frameBuilder = null,
            ImageErrorWidgetBuilder errorBuilder = null,
            float? width = null,
            float? height = null,
            Color color = null,
            BlendMode colorBlendMode = BlendMode.srcIn,
            BoxFit? fit = null,
            Alignment alignment = null,
            ImageRepeat repeat = ImageRepeat.noRepeat,
            Rect centerSlice = null,
            bool gaplessPlayback = false,
            FilterQuality filterQuality = FilterQuality.low,
            int cacheWidth = default,
            int cacheHeight = default
        ) {
            
            var fileImage = new FileImage(file, scale);//ResizeImage.resizeIfNeeded(cacheWidth, cacheHeight, FileImage(file, scale: scale));
            return new Image(
                key: key,
                image: fileImage,
                frameBuilder: frameBuilder,
                loadingBuilder: null,
                errorBuilder: errorBuilder,
                width: width,
                height: height,
                color: color,
                colorBlendMode: colorBlendMode,
                fit: fit,
                alignment: alignment,
                repeat: repeat,
                centerSlice: centerSlice,
                gaplessPlayback: gaplessPlayback,
                filterQuality: filterQuality
            );
        }

        public static Image asset(
            string name,
            Key key = null,
            AssetBundle bundle = null,
            ImageFrameBuilder frameBuilder = null,
            ImageErrorWidgetBuilder errorBuilder = null,
            float? scale = null,
            float? width = null,
            float? height = null,
            Color color = null,
            BlendMode colorBlendMode = BlendMode.srcIn,
            BoxFit? fit = null,
            Alignment alignment = null,
            ImageRepeat repeat = ImageRepeat.noRepeat,
            Rect centerSlice = null,
            bool gaplessPlayback = false,
            FilterQuality filterQuality = FilterQuality.low,
            int cacheWidth = default,
            int cacheHeight = default
        ) {
            /*image = ResizeImage.resizeIfNeeded(cacheWidth, cacheHeight, scale != null
                ? ExactAssetImage(name, bundle: bundle, scale: scale, package: package)
                : AssetImage(name, bundle: bundle, package: package)
            );*/
            var image = scale != null
                ? (AssetBundleImageProvider) new ExactAssetImage(name, bundle: bundle, scale: scale.Value)
                : new AssetImage(name, bundle: bundle);

            return new Image(
                key: key,
                image: image,
                frameBuilder: frameBuilder,
                loadingBuilder: null,
                errorBuilder: errorBuilder,
                width: width,
                height: height,
                color: color,
                colorBlendMode: colorBlendMode,
                fit: fit,
                alignment: alignment,
                repeat: repeat,
                centerSlice: centerSlice,
                gaplessPlayback: gaplessPlayback,
                filterQuality: filterQuality
            );
        }

        public static Image memory(
            byte[] bytes,
            Key key = null,
            float scale = 1.0f,
            ImageFrameBuilder frameBuilder = null,
            ImageErrorWidgetBuilder errorBuilder = null,
            float? width = null,
            float? height = null,
            Color color = null,
            BlendMode colorBlendMode = BlendMode.srcIn,
            BoxFit? fit = null,
            Alignment alignment = null,
            ImageRepeat repeat = ImageRepeat.noRepeat,
            Rect centerSlice = null,
            bool gaplessPlayback = false,
            FilterQuality filterQuality = FilterQuality.low,
            int cacheWidth = default,
            int cacheHeight = default
        ) {
            // ResizeImage.resizeIfNeeded(cacheWidth, cacheHeight, MemoryImage(bytes, scale: scale));
            var memoryImage = new MemoryImage(bytes, scale);
            return new Image(
                key: key,
                image: memoryImage,
                frameBuilder: frameBuilder,
                loadingBuilder: null,
                errorBuilder: errorBuilder,
                width: width,
                height: height,
                color: color,
                colorBlendMode: colorBlendMode,
                fit: fit,
                alignment: alignment,
                repeat: repeat,
                centerSlice: centerSlice,
                gaplessPlayback: gaplessPlayback,
                filterQuality: filterQuality
            );
        }

        public readonly ImageProvider image;
        public readonly ImageFrameBuilder frameBuilder;
        public readonly ImageLoadingBuilder loadingBuilder;
        public readonly ImageErrorWidgetBuilder errorBuilder;
        public readonly float? width;
        public readonly float? height;
        public readonly Color color;
        public readonly FilterQuality filterQuality;
        public readonly BlendMode colorBlendMode;
        public readonly BoxFit? fit;
        public readonly Alignment alignment;
        public readonly ImageRepeat repeat;
        public readonly Rect centerSlice;
        public readonly bool gaplessPlayback;

        public override State createState() {
            return new _ImageState();
        }

        public override void debugFillProperties(DiagnosticPropertiesBuilder properties) {
            base.debugFillProperties(properties);

            properties.add(new DiagnosticsProperty<ImageProvider>("image", image));
            properties.add(new DiagnosticsProperty<ImageFrameBuilder>("frameBuilder", frameBuilder));
            properties.add(new DiagnosticsProperty<ImageLoadingBuilder>("loadingBuilder", loadingBuilder));
            properties.add(new FloatProperty("width", width, defaultValue: foundation_.kNullDefaultValue));
            properties.add(new FloatProperty("height", height, defaultValue: foundation_.kNullDefaultValue));
            properties.add(new ColorProperty("color", color,
                defaultValue: foundation_.kNullDefaultValue));
            properties.add(new EnumProperty<BlendMode>("colorBlendMode", colorBlendMode,
                defaultValue: foundation_.kNullDefaultValue));
            properties.add(new EnumProperty<BoxFit?>("fit", fit, defaultValue: foundation_.kNullDefaultValue));
            properties.add(new DiagnosticsProperty<Alignment>("alignment", alignment,
                defaultValue: foundation_.kNullDefaultValue));
            properties.add(new EnumProperty<ImageRepeat>("repeat", repeat, defaultValue: ImageRepeat.noRepeat));
            properties.add(new DiagnosticsProperty<Rect>("centerSlice", centerSlice,
                defaultValue: foundation_.kNullDefaultValue));
            properties.add(new EnumProperty<FilterQuality>("filterQuality", filterQuality, foundation_.kNullDefaultValue));
        }
    }

    public class _ImageState : State<Image> ,WidgetsBindingObserver{
        ImageStream _imageStream;
        ImageInfo _imageInfo;
        //ImageChunkEvent _loadingProgress;
        bool _isListeningToStream = false;
        bool _invertColors;
        int _frameNumber;
        bool _wasSynchronouslyLoaded;
        DisposableBuildContext<State<Image>> _scrollAwareContext;
        Object _lastException;
        StackTrace _lastStack;
        
        public override void initState() {
            base.initState();
            WidgetsBinding.instance.addObserver(this);
            _scrollAwareContext = new DisposableBuildContext<State<Image>>(this);
        }

        public override void dispose() {
            D.assert(_imageStream != null);
            WidgetsBinding.instance.removeObserver(this);
            _stopListeningToStream();
            _scrollAwareContext.dispose();
            base.dispose();
        }
        
        public override void didChangeDependencies() {
            _updateInvertColors();
            _resolveImage();

            if (TickerMode.of(context)) {
                _listenToStream();
            }
            else {
                _stopListeningToStream();
            }

            base.didChangeDependencies();
        }

        public override void didUpdateWidget(StatefulWidget oldWidget) {
            base.didUpdateWidget(oldWidget);
            Image image = (Image) oldWidget;
            if (_isListeningToStream &&
                (widget.loadingBuilder == null) != (image.loadingBuilder == null)) {
                /*_imageStream.removeListener(_getListener(image.loadingBuilder));
                _imageStream.addListener(_getListener());*/
            }
            if (widget.image != ((Image) oldWidget).image) {
                _resolveImage();
            }
        }
        
        
        /*public override void didChangeAccessibilityFeatures() {
            base.didChangeAccessibilityFeatures();
            setState(() => {
                _updateInvertColors();
            });
        }*/
        
        void _updateInvertColors() {
            _invertColors = MediaQuery.of(context, nullOk: true)?.invertColors
                            ?? false;
        }

        void _resolveImage() {
            /*ScrollAwareImageProvider provider = new ScrollAwareImageProvider<dynamic>(
                context: _scrollAwareContext,
                imageProvider: widget.image
            );
            ImageStream newStream =
                provider.resolve(ImageUtils.createLocalImageConfiguration(
                    context,
                    size: widget.width != null && widget.height != null
                        ? new Size(widget.width.Value, widget.height.Value)
                        : null
                ));*/
            ImageStream newStream =
                widget.image.resolve(ImageUtils.createLocalImageConfiguration(
                    context,
                    size: widget.width != null && widget.height != null
                        ? new Size(widget.width.Value, widget.height.Value)
                        : null
                ));
            D.assert(newStream != null);
            _updateSourceStream(newStream);
        }
        
        /*ImageStreamListener _getListener(ImageLoadingBuilder loadingBuilder = null) {
            loadingBuilder ??= widget.loadingBuilder;
            _lastException = null;
            _lastStack = null;
            return new ImageStreamListener(
                _handleImageFrame,
                onChunk: loadingBuilder == null ? null : _handleImageChunk,
                onError: widget.errorBuilder != null
                    ? (dynamic error, StackTrace stackTrace) => {
                setState(() => {
                    _lastException = error;
                    _lastStack = stackTrace;
                });
            }
            : null
                );
        }*/

        
        /*void _handleImageFrame(ImageInfo imageInfo, bool synchronousCall) {
            setState(() =>{
                _imageInfo = imageInfo;
                _loadingProgress = null;
                _frameNumber = _frameNumber == null ? 0 : _frameNumber + 1;
                _wasSynchronouslyLoaded |= synchronousCall;
            });
        }

        void _handleImageChunk(ImageChunkEvent _event) {
            D.assert(widget.loadingBuilder != null);
            setState(() => {
                _loadingProgress = _event;
            });
        }*/
        void _handleImageChanged(ImageInfo imageInfo, bool synchronousCall) {
            setState(() => { _imageInfo = imageInfo; });
        }
        
        void _updateSourceStream(ImageStream newStream) {
            if (_imageStream?.key == newStream?.key) {
                return;
            }

            if (_isListeningToStream) {
                //_imageStream.removeListener(_getListener());
                _imageStream.removeListener(_handleImageChanged);
            }

            if (!widget.gaplessPlayback) {
                setState(() => { _imageInfo = null; });
            }

            /*setState(() => {
                _loadingProgress = null;
                _frameNumber = null;
                _wasSynchronouslyLoaded = false;
            });*/
            
            _imageStream = newStream;
            if (_isListeningToStream) {
                //_imageStream.addListener(_getListener());
                _imageStream.addListener(_handleImageChanged);
            }
        }

        void _listenToStream() {
            if (_isListeningToStream) {
                return;
            }
            //_imageStream.addListener(_getListener());
            _imageStream.addListener(_handleImageChanged);
            _isListeningToStream = true;
        }

        void _stopListeningToStream() {
            if (!_isListeningToStream) {
                return;
            }
            //_imageStream.removeListener(_getListener());
            _imageStream.removeListener(_handleImageChanged);
            _isListeningToStream = false;
        }
        
        public override Widget build(BuildContext context) {
            if (_lastException  != null) {
                D.assert(widget.errorBuilder != null);
                return widget.errorBuilder(context, _lastException, _lastStack);
            }
            RawImage image = new RawImage(
                image: _imageInfo?.image,
                width: widget.width,
                height: widget.height,
                scale: _imageInfo?.scale ?? 1.0f,
                color: widget.color,
                colorBlendMode: widget.colorBlendMode,
                fit: widget.fit,
                alignment: widget.alignment,
                repeat: widget.repeat,
                centerSlice: widget.centerSlice,
                invertColors: _invertColors,
                filterQuality: widget.filterQuality
            );
            /*if (widget.frameBuilder != null)
                image = widget.frameBuilder(context, image, _frameNumber, _wasSynchronouslyLoaded);

            if (widget.loadingBuilder != null)
                image = widget.loadingBuilder(context, image, _loadingProgress);*/
            
            return image;
        }

        public override void debugFillProperties(DiagnosticPropertiesBuilder description) {
            base.debugFillProperties(description);
            description.add(new DiagnosticsProperty<ImageStream>("stream", _imageStream));
            description.add(new DiagnosticsProperty<ImageInfo>("pixels", _imageInfo));
            //description.add(new DiagnosticsProperty<ImageChunkEvent>("loadingProgress", _loadingProgress));
            description.add(new DiagnosticsProperty<int>("frameNumber", _frameNumber));
            description.add(new DiagnosticsProperty<bool>("wasSynchronouslyLoaded", _wasSynchronouslyLoaded));
        }

        
        public void didChangeMetrics() {
            setState();
        }

        public void didChangeTextScaleFactor() {
            setState();
        }

        public void didChangePlatformBrightness() {
            setState();
        }

        public void didChangeLocales(List<Locale> locale) {
            setState();
        }

        public Future<bool> didPopRoute() {
            return Future.value(false).to<bool>();
        }

        public Future<bool> didPushRoute(string route) {
            return Future.value(false).to<bool>();
        }
    }
}