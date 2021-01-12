using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.UIWidgets.animation;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.rendering;
using Unity.UIWidgets.ui;
using UnityEngine;
using Color = Unity.UIWidgets.ui.Color;
using Debug = System.Diagnostics.Debug;
using Object = System.Object;

namespace Unity.UIWidgets.widgets {
    public class FadeInImage : StatelessWidget {
        public FadeInImage(
            ImageProvider placeholder,
            ImageErrorWidgetBuilder placeholderErrorBuilder,
            ImageProvider image,
            ImageErrorWidgetBuilder imageErrorBuilder,
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
            this.placeholderErrorBuilder = placeholderErrorBuilder;
            this.image = image;
            this.imageErrorBuilder = imageErrorBuilder;
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
            ImageErrorWidgetBuilder placeholderErrorBuilder,
            string image,
            ImageErrorWidgetBuilder imageErrorBuilder,
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
            Key key = null,
            int placeholderCacheWidth = default,
            int placeholderCacheHeight = default,
            int imageCacheWidth = default,
            int imageCacheHeight = default
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
            /*placeholder = ResizeImage.resizeIfNeeded(placeholderCacheWidth, placeholderCacheHeight,
                new MemoryImage(placeholder, scale: placeholderScale));
            image = ResizeImage.resizeIfNeeded(imageCacheWidth, imageCacheHeight,
                new NetworkImage(image, scale: imageScale));*/
            return new FadeInImage(
                placeholder: memoryImage,
                placeholderErrorBuilder: placeholderErrorBuilder,
                image: networkImage,
                imageErrorBuilder: imageErrorBuilder,
                fadeOutDuration: fadeOutDuration,
                fadeOutCurve: fadeOutCurve,
                fadeInDuration: fadeInDuration,
                fadeInCurve: fadeInCurve,
                width: width, height: height,
                fit: fit,
                alignment: alignment,
                repeat: repeat,
                key: key
            );
        }

        public static FadeInImage assetNetwork(
            string placeholder,
            ImageErrorWidgetBuilder placeholderErrorBuilder,
            string image,
            ImageErrorWidgetBuilder imageErrorBuilder,
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
            Key key = null,
            int placeholderCacheWidth = default,
            int placeholderCacheHeight = default,
            int imageCacheWidth = default,
            int imageCacheHeight = default
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
            /*placeholder = placeholderScale != null
                ? ResizeImage.resizeIfNeeded(placeholderCacheWidth, placeholderCacheHeight, ExactAssetImage(placeholder, bundle: bundle, scale: placeholderScale))
                : ResizeImage.resizeIfNeeded(placeholderCacheWidth, placeholderCacheHeight, AssetImage(placeholder, bundle: bundle));*/
            var networkImage = new NetworkImage(image, imageScale);
            return new FadeInImage(
                placeholder: imageProvider,
                placeholderErrorBuilder: placeholderErrorBuilder,
                image: networkImage,
                imageErrorBuilder: imageErrorBuilder,
                fadeOutDuration: fadeOutDuration,
                fadeOutCurve: fadeOutCurve,
                fadeInDuration: fadeInDuration,
                fadeInCurve: fadeInCurve,
                width: width, height: height,
                fit: fit,
                alignment: alignment,
                repeat: repeat,
                key: key
            );
        }

        public readonly ImageProvider placeholder;
        public readonly ImageErrorWidgetBuilder placeholderErrorBuilder;
        public readonly ImageProvider image;
        public readonly ImageErrorWidgetBuilder imageErrorBuilder;
        public readonly TimeSpan fadeOutDuration;
        public readonly Curve fadeOutCurve;
        public readonly TimeSpan fadeInDuration;
        public readonly Curve fadeInCurve;
        public readonly float? width;
        public readonly float? height;
        public readonly BoxFit? fit;
        public readonly Alignment alignment;
        public readonly ImageRepeat repeat;

        public Image _image(
            ImageProvider image = null,
            ImageErrorWidgetBuilder errorBuilder = null,
            ImageFrameBuilder frameBuilder = null
        ) {
            D.assert(image != null);
            return new Image(
                image: image,
                //errorBuilder: errorBuilder,
                //frameBuilder: frameBuilder,
                width: width,
                height: height,
                fit: fit,
                alignment: alignment,
                repeat: repeat,
                //matchTextDirection: matchTextDirection,
                gaplessPlayback: true
            );
        }

        public override Widget build(BuildContext context) {
            Widget result = _image(
                image: image,
                errorBuilder: imageErrorBuilder,
                frameBuilder: (BuildContext context, Widget child, int frame, bool wasSynchronouslyLoaded) => {
                    if (wasSynchronouslyLoaded)
                        return child;
                    return new _AnimatedFadeOutFadeIn(
                        key: key,
                        target: child,
                        placeholder: _image(
                            image: placeholder,
                            errorBuilder: placeholderErrorBuilder
                        ),
                        isTargetLoaded: frame != null,
                        fadeInDuration: fadeInDuration,
                        fadeOutDuration: fadeOutDuration,
                        fadeInCurve: fadeInCurve,
                        fadeOutCurve: fadeOutCurve
                    );
                }
            );
            return result;
        }
    }

    public class _AnimatedFadeOutFadeIn : ImplicitlyAnimatedWidget {
        public _AnimatedFadeOutFadeIn(
            Key key,
            Widget target,
            Widget placeholder,
            bool isTargetLoaded,
            TimeSpan fadeOutDuration,
            TimeSpan fadeInDuration,
            Curve fadeOutCurve,
            Curve fadeInCurve
        ) : base(key: key, duration: fadeInDuration + fadeOutDuration) {
            D.assert(target != null);
            D.assert(placeholder != null);
            D.assert(isTargetLoaded != null);
            D.assert(fadeOutDuration != null);
            D.assert(fadeOutCurve != null);
            D.assert(fadeInDuration != null);
            D.assert(fadeInCurve != null);
            this.target = target;
            this.placeholder = placeholder;
            this.isTargetLoaded = isTargetLoaded;
            this.fadeInDuration = fadeInDuration;
            this.fadeOutDuration = fadeOutDuration;
            this.fadeInCurve = fadeInCurve;
            this.fadeOutCurve = fadeOutCurve;
        }

        public readonly Widget target;
        public readonly Widget placeholder;
        public readonly bool isTargetLoaded;
        public readonly TimeSpan? fadeInDuration;
        public readonly TimeSpan? fadeOutDuration;
        public readonly Curve fadeInCurve;
        public readonly Curve fadeOutCurve;


        public override State createState() => new _AnimatedFadeOutFadeInState();


        public class _AnimatedFadeOutFadeInState : ImplicitlyAnimatedWidgetState<_AnimatedFadeOutFadeIn> {
            FloatTween _targetOpacity;
            FloatTween _placeholderOpacity;
            Animation<float> _targetOpacityAnimation;
            Animation<float> _placeholderOpacityAnimation;

            /*@override
            void forEachTween(TweenVisitor<dynamic> visitor) {
                _targetOpacity = visitor(
                    _targetOpacity,
                    widget.isTargetLoaded ? 1.0 : 0.0,
                    (dynamic value) => Tween<double>(begin: value as double),
                ) as Tween<double>;
                _placeholderOpacity = visitor(
                    _placeholderOpacity,
                    widget.isTargetLoaded ? 0.0 : 1.0,
                    (dynamic value) => Tween<double>(begin: value as double),
                ) as Tween<double>;
            }*/

            protected override void forEachTween(TweenVisitor visitor) {
                _targetOpacity = (FloatTween) visitor.visit(
                    state: this,
                    tween: _targetOpacity,
                    targetValue: widget.isTargetLoaded ? 1.0f : 0.0f,
                    constructor: (float value) => new FloatTween(begin: value, 0));

                _placeholderOpacity = (FloatTween) visitor.visit(
                    state: this,
                    tween: _placeholderOpacity,
                    targetValue: widget.isTargetLoaded ? 0.0f : 1.0f,
                    constructor: (float value) => new FloatTween(begin: value, 0));
            }

            protected override void didUpdateTweens() {
                List<TweenSequenceItem<float>> list = new List<TweenSequenceItem<float>>();

                Debug.Assert(widget.fadeOutDuration?.Milliseconds != null,
                    "widget.fadeOutDuration?.Milliseconds != null");
                list.Add(new TweenSequenceItem<float>(
                    tween: _placeholderOpacity.chain(new CurveTween(curve: widget.fadeOutCurve)),
                    weight: (float) widget.fadeOutDuration?.Milliseconds
                ));

                Debug.Assert(widget.fadeInDuration?.Milliseconds != null,
                    "widget.fadeInDuration?.Milliseconds != null");
                list.Add(new TweenSequenceItem<float>(
                    tween: new ConstantTween<float>(0),
                    weight: (float) widget.fadeInDuration?.Milliseconds
                ));

                //[!!!] drive
                /*_placeholderOpacityAnimation = animation.drive(list).addStatusListener((AnimationStatus status) =>{
                    if (_placeholderOpacityAnimation.isCompleted) {
                        // Need to rebuild to remove placeholder now that it is invisibile.
                        setState(() => {});
                    }
                });*/

                List<TweenSequenceItem<float>> list2 = new List<TweenSequenceItem<float>>();
                list2.Add(new TweenSequenceItem<float>(
                    tween: new ConstantTween<float>(0),
                    weight: (float) widget.fadeOutDuration?.Milliseconds
                ));
                list2.Add(new TweenSequenceItem<float>(
                    tween: _targetOpacity.chain(new CurveTween(curve: widget.fadeInCurve)),
                    weight: (float) widget.fadeInDuration?.Milliseconds
                ));
                //_targetOpacityAnimation = animation.drive(list2);[!!!] animation.cs drive
                if (!widget.isTargetLoaded && _isValid(_placeholderOpacity) && _isValid(_targetOpacity)) {
                    //controller.value = controller.upperBound;[!!!] animation_controller.cs value set
                }
            }

            bool _isValid(Tween<float> tween) {
                return tween.begin != null && tween.end != null;
            }


            public override Widget build(BuildContext context) {
                Widget target = new FadeTransition(
                    opacity: _targetOpacityAnimation,
                    child: widget.target
                );

                if (_placeholderOpacityAnimation.isCompleted) {
                    return target;
                }

                return new Stack(
                    fit: StackFit.passthrough,
                    alignment: AlignmentDirectional.center,
                    textDirection: TextDirection.ltr,
                    children: new List<Widget>() {
                        target,
                        new FadeTransition(
                            opacity: _placeholderOpacityAnimation,
                            child: widget.placeholder
                        )
                    }
                );
            }

            public override void debugFillProperties(DiagnosticPropertiesBuilder properties) {
                base.debugFillProperties(properties);
                properties.add(new DiagnosticsProperty<Animation<float>>("targetOpacity", _targetOpacityAnimation));
                properties.add(
                    new DiagnosticsProperty<Animation<float>>("placeholderOpacity", _placeholderOpacityAnimation));
            }
        }

        /*enum FadeInImagePhase {
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
        }*/
    }
}