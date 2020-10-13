using System;
using System.Collections.Generic;
using Unity.UIWidgets.async2;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.scheduler2;
using Unity.UIWidgets.ui;
using UnityEngine;
using Color = Unity.UIWidgets.ui.Color;
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
                        context: "image failed to precache",
                        library: "image resource service",
                        exception: exception,
                        silent: true
                    ));
                }
            }

            stream.addListener(listener, onError: errorListener);
            return completer.future;
        }
    }

    public class Image : StatefulWidget {
        public Image(
            Key key = null,
            ImageProvider image = null,
            float? width = null,
            float? height = null,
            Color color = null,
            BlendMode colorBlendMode = BlendMode.srcIn,
            BoxFit? fit = null,
            Alignment alignment = null,
            ImageRepeat repeat = ImageRepeat.noRepeat,
            Rect centerSlice = null,
            bool gaplessPlayback = false,
            FilterMode filterMode = FilterMode.Bilinear
        ) : base(key) {
            D.assert(image != null);
            this.image = image;
            this.width = width;
            this.height = height;
            this.color = color;
            this.colorBlendMode = colorBlendMode;
            this.fit = fit;
            this.alignment = alignment ?? Alignment.center;
            this.repeat = repeat;
            this.centerSlice = centerSlice;
            this.gaplessPlayback = gaplessPlayback;
            this.filterMode = filterMode;
        }

        public static Image network(
            string src,
            Key key = null,
            float scale = 1.0f,
            float? width = null,
            float? height = null,
            Color color = null,
            BlendMode colorBlendMode = BlendMode.srcIn,
            BoxFit? fit = null,
            Alignment alignment = null,
            ImageRepeat repeat = ImageRepeat.noRepeat,
            Rect centerSlice = null,
            bool gaplessPlayback = false,
            FilterMode filterMode = FilterMode.Bilinear,
            IDictionary<string, string> headers = null
        ) {
            var networkImage = new NetworkImage(src, scale, headers);
            return new Image(
                key,
                networkImage,
                width,
                height,
                color,
                colorBlendMode,
                fit,
                alignment,
                repeat,
                centerSlice,
                gaplessPlayback,
                filterMode
            );
        }

        public static Image file(
            string file,
            Key key = null,
            float scale = 1.0f,
            float? width = null,
            float? height = null,
            Color color = null,
            BlendMode colorBlendMode = BlendMode.srcIn,
            BoxFit? fit = null,
            Alignment alignment = null,
            ImageRepeat repeat = ImageRepeat.noRepeat,
            Rect centerSlice = null,
            bool gaplessPlayback = false,
            FilterMode filterMode = FilterMode.Bilinear
        ) {
            var fileImage = new FileImage(file, scale);
            return new Image(
                key,
                fileImage,
                width,
                height,
                color,
                colorBlendMode,
                fit,
                alignment,
                repeat,
                centerSlice,
                gaplessPlayback,
                filterMode
            );
        }

        public static Image asset(
            string name,
            Key key = null,
            AssetBundle bundle = null,
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
            FilterMode filterMode = FilterMode.Bilinear
        ) {
            var image = scale != null
                ? (AssetBundleImageProvider) new ExactAssetImage(name, bundle: bundle, scale: scale.Value)
                : new AssetImage(name, bundle: bundle);

            return new Image(
                key,
                image,
                width,
                height,
                color,
                colorBlendMode,
                fit,
                alignment,
                repeat,
                centerSlice,
                gaplessPlayback,
                filterMode
            );
        }

        public static Image memory(
            byte[] bytes,
            Key key = null,
            float scale = 1.0f,
            float? width = null,
            float? height = null,
            Color color = null,
            BlendMode colorBlendMode = BlendMode.srcIn,
            BoxFit? fit = null,
            Alignment alignment = null,
            ImageRepeat repeat = ImageRepeat.noRepeat,
            Rect centerSlice = null,
            bool gaplessPlayback = false,
            FilterMode filterMode = FilterMode.Bilinear
        ) {
            var memoryImage = new MemoryImage(bytes, scale);
            return new Image(
                key,
                memoryImage,
                width,
                height,
                color,
                colorBlendMode,
                fit,
                alignment,
                repeat,
                centerSlice,
                gaplessPlayback,
                filterMode
            );
        }

        public readonly ImageProvider image;
        public readonly float? width;
        public readonly float? height;
        public readonly Color color;
        public readonly FilterMode filterMode;
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
            properties.add(new FloatProperty("width", width, defaultValue: foundation_.kNullDefaultValue));
            properties.add(new FloatProperty("height", height, defaultValue: foundation_.kNullDefaultValue));
            properties.add(new DiagnosticsProperty<Color>("color", color,
                defaultValue: foundation_.kNullDefaultValue));
            properties.add(new EnumProperty<BlendMode>("colorBlendMode", colorBlendMode,
                defaultValue: foundation_.kNullDefaultValue));
            properties.add(new EnumProperty<BoxFit?>("fit", fit, defaultValue: foundation_.kNullDefaultValue));
            properties.add(new DiagnosticsProperty<Alignment>("alignment", alignment,
                defaultValue: foundation_.kNullDefaultValue));
            properties.add(new EnumProperty<ImageRepeat>("repeat", repeat, defaultValue: ImageRepeat.noRepeat));
            properties.add(new DiagnosticsProperty<Rect>("centerSlice", centerSlice,
                defaultValue: foundation_.kNullDefaultValue));
            properties.add(new EnumProperty<FilterMode>("filterMode", filterMode, foundation_.kNullDefaultValue));
        }
    }

    public class _ImageState : State<Image> {
        ImageStream _imageStream;
        ImageInfo _imageInfo;
        bool _isListeningToStream = false;
        bool _invertColors;

        public override void didChangeDependencies() {
            _invertColors = false;

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

            if (widget.image != ((Image) oldWidget).image) {
                _resolveImage();
            }
        }

        void _resolveImage() {
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

        void _handleImageChanged(ImageInfo imageInfo, bool synchronousCall) {
            setState(() => { _imageInfo = imageInfo; });
        }

        void _updateSourceStream(ImageStream newStream) {
            if (_imageStream?.key == newStream?.key) {
                return;
            }

            if (_isListeningToStream) {
                _imageStream.removeListener(_handleImageChanged);
            }

            if (!widget.gaplessPlayback) {
                setState(() => { _imageInfo = null; });
            }

            _imageStream = newStream;
            if (_isListeningToStream) {
                _imageStream.addListener(_handleImageChanged);
            }
        }

        void _listenToStream() {
            if (_isListeningToStream) {
                return;
            }

            _imageStream.addListener(_handleImageChanged);
            _isListeningToStream = true;
        }

        void _stopListeningToStream() {
            if (!_isListeningToStream) {
                return;
            }

            _imageStream.removeListener(_handleImageChanged);
            _isListeningToStream = false;
        }

        public override void dispose() {
            D.assert(_imageStream != null);
            _stopListeningToStream();
            base.dispose();
        }


        public override Widget build(BuildContext context) {
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
                filterMode: widget.filterMode
            );

            return image;
        }

        public override void debugFillProperties(DiagnosticPropertiesBuilder description) {
            base.debugFillProperties(description);
            description.add(new DiagnosticsProperty<ImageStream>("stream", _imageStream));
            description.add(new DiagnosticsProperty<ImageInfo>("pixels", _imageInfo));
        }
    }
}