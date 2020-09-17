using System;
using Unity.UIWidgets.animation;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.ui;
using UnityEngine;
using Color = Unity.UIWidgets.ui.Color;

namespace Unity.UIWidgets.widgets {
    public class FadeInImage : StatefulWidget {
        public FadeInImage(
            ImageProvider placeholder,
            ImageProvider image,
            TimeSpan? fadeOutDuration = null,
            Curve fadeOutCurve = null,
            TimeSpan? fadeInDuration = null,
            Curve fadeInCurve = null,
            float? width = null,
            float? height = null,
            BoxFit? fit = null,
            Alignment alignment = null,
            ImageRepeat repeat = ImageRepeat.noRepeat,
            Key key = null
        ) : base(key) {
            D.assert(placeholder != null);
            D.assert(image != null);
            D.assert(fadeOutDuration != null);
            D.assert(fadeOutCurve != null);
            D.assert(fadeInDuration != null);
            D.assert(fadeInCurve != null);
            D.assert(alignment != null);
            this.placeholder = placeholder;
            this.image = image;
            this.width = width;
            this.height = height;
            this.fit = fit;
            this.fadeOutDuration = fadeOutDuration ?? TimeSpan.FromMilliseconds(300);
            this.fadeOutCurve = fadeOutCurve ?? Curves.easeOut;
            this.fadeInDuration = fadeInDuration ?? TimeSpan.FromMilliseconds(700);
            this.fadeInCurve = fadeInCurve ?? Curves.easeIn;
            this.alignment = alignment ?? Alignment.center;
            this.repeat = repeat;
        }

        public static FadeInImage memoryNetwork(
            byte[] placeholder,
            string image,
            float placeholderScale = 1.0f,
            float imageScale = 1.0f,
            TimeSpan? fadeOutDuration = null,
            Curve fadeOutCurve = null,
            TimeSpan? fadeInDuration = null,
            Curve fadeInCurve = null,
            float? width = null,
            float? height = null,
            BoxFit? fit = null,
            Alignment alignment = null,
            ImageRepeat repeat = ImageRepeat.noRepeat,
            Key key = null
        ) {
            D.assert(placeholder != null);
            D.assert(image != null);
            D.assert(fadeOutDuration != null);
            D.assert(fadeOutCurve != null);
            D.assert(fadeInDuration != null);
            D.assert(fadeInCurve != null);
            D.assert(alignment != null);
            var memoryImage = new MemoryImage(placeholder, placeholderScale);
            var networkImage = new NetworkImage(image, imageScale);
            return new FadeInImage(
                memoryImage,
                networkImage,
                fadeOutDuration,
                fadeOutCurve,
                fadeInDuration,
                fadeInCurve,
                width, height,
                fit,
                alignment,
                repeat,
                key
            );
        }

        public static FadeInImage assetNetwork(
            string placeholder,
            string image,
            AssetBundle bundle = null,
            float? placeholderScale = null,
            float imageScale = 1.0f,
            TimeSpan? fadeOutDuration = null,
            Curve fadeOutCurve = null,
            TimeSpan? fadeInDuration = null,
            Curve fadeInCurve = null,
            float? width = null,
            float? height = null,
            BoxFit? fit = null,
            Alignment alignment = null,
            ImageRepeat repeat = ImageRepeat.noRepeat,
            Key key = null
        ) {
            D.assert(placeholder != null);
            D.assert(image != null);
            fadeOutDuration = fadeOutDuration ?? new TimeSpan(0, 0, 0, 0, 300);
            fadeOutCurve = fadeOutCurve ?? Curves.easeOut;
            fadeInDuration = fadeInDuration ?? new TimeSpan(0, 0, 0, 0, 700);
            fadeInCurve = Curves.easeIn;
            alignment = alignment ?? Alignment.center;
            var imageProvider = placeholderScale != null
                ? new ExactAssetImage(placeholder, bundle: bundle, scale: placeholderScale ?? 1.0f)
                : (ImageProvider) new AssetImage(placeholder, bundle: bundle);

            var networkImage = new NetworkImage(image, imageScale);
            return new FadeInImage(
                imageProvider,
                networkImage,
                fadeOutDuration,
                fadeOutCurve,
                fadeInDuration,
                fadeInCurve,
                width, height,
                fit,
                alignment,
                repeat,
                key
            );
        }

        public readonly ImageProvider placeholder;
        public readonly ImageProvider image;
        public readonly TimeSpan fadeOutDuration;
        public readonly Curve fadeOutCurve;
        public readonly TimeSpan fadeInDuration;
        public readonly Curve fadeInCurve;
        public readonly float? width;
        public readonly float? height;
        public readonly BoxFit? fit;
        public readonly Alignment alignment;
        public readonly ImageRepeat repeat;

        public override State createState() {
            return new _FadeInImageState();
        }
    }

    enum FadeInImagePhase {
        start,
        waiting,
        fadeOut,
        fadeIn,
        completed
    }

    delegate void _ImageProviderResolverListener();

    class _ImageProviderResolver {
        public _ImageProviderResolver(
            _FadeInImageState state,
            _ImageProviderResolverListener listener
        ) {
            this.state = state;
            this.listener = listener;
        }

        readonly _FadeInImageState state;
        readonly _ImageProviderResolverListener listener;

        FadeInImage widget {
            get { return state.widget; }
        }

        public ImageStream _imageStream;
        public ImageInfo _imageInfo;

        public void resolve(ImageProvider provider) {
            ImageStream oldImageStream = _imageStream;
            Size size = null;
            if (widget.width != null && widget.height != null) {
                size = new Size((int) widget.width, (int) widget.height);
            }

            _imageStream = provider.resolve(ImageUtils.createLocalImageConfiguration(state.context, size));
            D.assert(_imageStream != null);

            if (_imageStream.key != oldImageStream?.key) {
                oldImageStream?.removeListener(_handleImageChanged);
                _imageStream.addListener(_handleImageChanged);
            }
        }

        void _handleImageChanged(ImageInfo imageInfo, bool synchronousCall) {
            _imageInfo = imageInfo;
            listener();
        }

        public void stopListening() {
            _imageStream?.removeListener(_handleImageChanged);
        }
    }


    class _FadeInImageState : TickerProviderStateMixin<FadeInImage> {
        _ImageProviderResolver _imageResolver;
        _ImageProviderResolver _placeholderResolver;

        AnimationController _controller;
        Animation<float> _animation;

        FadeInImagePhase _phase = FadeInImagePhase.start;

        public override void initState() {
            _imageResolver = new _ImageProviderResolver(state: this, _updatePhase);
            _placeholderResolver = new _ImageProviderResolver(state: this, listener: () => {
                setState(() => {
                    // Trigger rebuild to display the placeholder image
                });
            });
            _controller = new AnimationController(
                value: 1.0f,
                vsync: this
            );
            _controller.addListener(() => {
                setState(() => {
                    // Trigger rebuild to update opacity value.
                });
            });
            _controller.addStatusListener(status => { _updatePhase(); });
            base.initState();
        }

        public override void didChangeDependencies() {
            _resolveImage();
            base.didChangeDependencies();
        }

        public override void didUpdateWidget(StatefulWidget oldWidget) {
            base.didUpdateWidget(oldWidget);
            FadeInImage fadeInImage = oldWidget as FadeInImage;
            if (widget.image != fadeInImage.image || widget.placeholder != fadeInImage.placeholder) {
                _resolveImage();
            }
        }

        void _resolveImage() {
            _imageResolver.resolve(widget.image);

            if (_isShowingPlaceholder) {
                _placeholderResolver.resolve(widget.placeholder);
            }

            if (_phase == FadeInImagePhase.start) {
                _updatePhase();
            }
        }

        void _updatePhase() {
            setState(() => {
                switch (_phase) {
                    case FadeInImagePhase.start:
                        if (_imageResolver._imageInfo != null) {
                            _phase = FadeInImagePhase.completed;
                        }
                        else {
                            _phase = FadeInImagePhase.waiting;
                        }

                        break;
                    case FadeInImagePhase.waiting:
                        if (_imageResolver._imageInfo != null) {
                            _controller.duration = widget.fadeOutDuration;
                            _animation = new CurvedAnimation(
                                parent: _controller,
                                curve: widget.fadeOutCurve
                            );
                            _phase = FadeInImagePhase.fadeOut;
                            _controller.reverse(1.0f);
                        }

                        break;
                    case FadeInImagePhase.fadeOut:
                        if (_controller.status == AnimationStatus.dismissed) {
                            // Done fading out placeholder. Begin target image fade-in.
                            _controller.duration = widget.fadeInDuration;
                            _animation = new CurvedAnimation(
                                parent: _controller,
                                curve: widget.fadeInCurve
                            );
                            _phase = FadeInImagePhase.fadeIn;
                            _placeholderResolver.stopListening();
                            _controller.forward(0.0f);
                        }

                        break;
                    case FadeInImagePhase.fadeIn:
                        if (_controller.status == AnimationStatus.completed) {
                            // Done finding in new image.
                            _phase = FadeInImagePhase.completed;
                        }

                        break;
                    case FadeInImagePhase.completed:
                        // Nothing to do.
                        break;
                }
            });
        }

        public override void dispose() {
            _imageResolver.stopListening();
            _placeholderResolver.stopListening();
            _controller.dispose();
            base.dispose();
        }

        bool _isShowingPlaceholder {
            get {
                switch (_phase) {
                    case FadeInImagePhase.start:
                    case FadeInImagePhase.waiting:
                    case FadeInImagePhase.fadeOut:
                        return true;
                    case FadeInImagePhase.fadeIn:
                    case FadeInImagePhase.completed:
                        return false;
                }

                return true;
            }
        }

        ImageInfo _imageInfo {
            get {
                return _isShowingPlaceholder
                    ? _placeholderResolver._imageInfo
                    : _imageResolver._imageInfo;
            }
        }

        public override Widget build(BuildContext context) {
            D.assert(_phase != FadeInImagePhase.start);
            ImageInfo imageInfo = _imageInfo;
            return new RawImage(
                image: imageInfo?.image,
                width: widget.width,
                height: widget.height,
                scale: imageInfo?.scale ?? 1.0f,
                color: Color.fromRGBO(255, 255, 255, _animation?.value ?? 1.0f),
                colorBlendMode: BlendMode.modulate,
                fit: widget.fit,
                alignment: widget.alignment,
                repeat: widget.repeat
            );
        }

        public override void debugFillProperties(DiagnosticPropertiesBuilder properties) {
            base.debugFillProperties(properties);
            properties.add(new EnumProperty<FadeInImagePhase>("phase", _phase));
            properties.add(new DiagnosticsProperty<ImageInfo>("pixels", _imageInfo));
            properties.add(new DiagnosticsProperty<ImageStream>("image stream", _imageResolver._imageStream));
            properties.add(new DiagnosticsProperty<ImageStream>("placeholder stream",
                _placeholderResolver._imageStream));
        }
    }
}