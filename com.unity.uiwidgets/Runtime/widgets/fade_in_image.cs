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
            ImageErrorWidgetBuilder imageErrorBuilder = null,
            TimeSpan? fadeOutDuration = null,
            Curve fadeOutCurve = null,
            TimeSpan? fadeInDuration = null,
            Curve fadeInCurve = null,
            float? width = null,
            float? height = null,
            BoxFit? fit = null,
            AlignmentGeometry alignment = null,
            ImageRepeat repeat = ImageRepeat.noRepeat,
            Key key = null,
            bool matchTextDirection = false
        ) : base(key) {
           
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
            this.matchTextDirection = matchTextDirection;
        }

        public static FadeInImage memoryNetwork(
            byte[] placeholder ,
            string image ,
            Key key = null,
            ImageErrorWidgetBuilder placeholderErrorBuilder = null,
            ImageErrorWidgetBuilder imageErrorBuilder = null,
            float placeholderScale = 1.0f,
            float imageScale = 1.0f,
            TimeSpan? fadeOutDuration = null,
            Curve fadeOutCurve = null,
            TimeSpan? fadeInDuration = null,
            Curve fadeInCurve = null,
            float? width = null,
            float? height = null,
            BoxFit? fit = null,
            AlignmentGeometry alignment = null,
            ImageRepeat repeat = ImageRepeat.noRepeat,
            bool matchTextDirection = false,
            int? placeholderCacheWidth = null,
            int? placeholderCacheHeight = null,
            int? imageCacheWidth = null,
            int? imageCacheHeight = null
        ) {
            alignment = alignment ?? Alignment.center;
            fadeOutDuration = fadeOutDuration  ?? TimeSpan.FromMilliseconds( 300 );
            fadeOutCurve = fadeOutCurve ?? Curves.easeOut;
            fadeInDuration = fadeInDuration ?? TimeSpan.FromMilliseconds( 700 );
            fadeInCurve = fadeInCurve ?? Curves.easeIn;
            ImageProvider memoryImage = new MemoryImage(placeholder, placeholderScale);
            ImageProvider networkImage = new NetworkImage(image, imageScale);
            
            //placeholder = ResizeImage.resizeIfNeeded(placeholderCacheWidth, placeholderCacheHeight, memoryImage);
            //image = ResizeImage.resizeIfNeeded(imageCacheWidth, imageCacheHeight, networkImage);
            return new FadeInImage(
                placeholder: ResizeImage.resizeIfNeeded(placeholderCacheWidth, placeholderCacheHeight, memoryImage),
                placeholderErrorBuilder: placeholderErrorBuilder,
                image: ResizeImage.resizeIfNeeded(imageCacheWidth, imageCacheHeight, networkImage),
                imageErrorBuilder: imageErrorBuilder,
                fadeOutDuration: fadeOutDuration,
                fadeOutCurve: fadeOutCurve,
                fadeInDuration: fadeInDuration,
                fadeInCurve: fadeInCurve,
                width: width, height: height,
                fit: fit,
                alignment: alignment,
                repeat: repeat,
                matchTextDirection: matchTextDirection,
                key: key
            );
        }

        public static FadeInImage assetNetwork(
            string placeholder,
            string image,
            ImageErrorWidgetBuilder placeholderErrorBuilder = null,
            ImageErrorWidgetBuilder imageErrorBuilder = null,
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
            AlignmentGeometry alignment = null,
            ImageRepeat repeat = ImageRepeat.noRepeat,
            bool matchTextDirection = false,
            Key key = null,
            int? placeholderCacheWidth = null,
            int? placeholderCacheHeight = null,
            int? imageCacheWidth = null,
            int? imageCacheHeight = null
        ) {
           
            fadeOutDuration = fadeOutDuration ?? new TimeSpan(0, 0, 0, 0, 300);
            fadeOutCurve = fadeOutCurve ?? Curves.easeOut;
            fadeInDuration = fadeInDuration ?? new TimeSpan(0, 0, 0, 0, 700);
            fadeInCurve = Curves.easeIn;
            alignment = alignment ?? Alignment.center;
            var holder = placeholderScale ?? 1.0f;
            var _placeholder = placeholderScale != null
                ? ResizeImage.resizeIfNeeded(placeholderCacheWidth, placeholderCacheHeight,
                    new ExactAssetImage(placeholder, bundle: bundle, scale: holder))
                : ResizeImage.resizeIfNeeded(placeholderCacheWidth, placeholderCacheHeight,
                    new AssetImage(placeholder, bundle: bundle));
            return new FadeInImage(
                placeholder: _placeholder,
                placeholderErrorBuilder: placeholderErrorBuilder,
                image: ResizeImage.resizeIfNeeded(imageCacheWidth, imageCacheHeight, new NetworkImage(image, scale: imageScale)),
                imageErrorBuilder: imageErrorBuilder,
                fadeOutDuration: fadeOutDuration,
                fadeOutCurve: fadeOutCurve,
                fadeInDuration: fadeInDuration,
                fadeInCurve: fadeInCurve,
                width: width, height: height,
                fit: fit,
                alignment: alignment,
                repeat: repeat,
                matchTextDirection: matchTextDirection,
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
        public readonly AlignmentGeometry alignment;
        public readonly ImageRepeat repeat;
        public readonly bool matchTextDirection;

        public Image _image(
            ImageProvider image = null,
            ImageErrorWidgetBuilder errorBuilder = null,
            ImageFrameBuilder frameBuilder = null
        ) {
            D.assert(image != null);
            return new Image(
                image: image,
                errorBuilder: errorBuilder,
                frameBuilder: frameBuilder,
                width: width,
                height: height,
                fit: fit,
                alignment: alignment,
                repeat: repeat,
                matchTextDirection: matchTextDirection,
                gaplessPlayback: true
                
            );
        }

        public override Widget build(BuildContext context) {
            Widget result = _image(
                image: image,
                errorBuilder: imageErrorBuilder,
                frameBuilder: (BuildContext context1, Widget child, int? frame, bool wasSynchronouslyLoaded)=> {
                if (wasSynchronouslyLoaded)
                    return child;
                return new _AnimatedFadeOutFadeIn(
                    target: child,
                    placeholder: _image(image: placeholder, errorBuilder: placeholderErrorBuilder),
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
            Widget target ,
            Widget placeholder ,
            bool isTargetLoaded ,
            TimeSpan fadeOutDuration,
            TimeSpan fadeInDuration,
            Curve fadeOutCurve,
            Curve fadeInCurve,
            Key key = null
        ) : base(key: key, duration: fadeInDuration + fadeOutDuration) {
            
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

                
                _placeholderOpacityAnimation = animation.drive(new TweenSequence<float>(list));
                _placeholderOpacityAnimation.addStatusListener((AnimationStatus status) =>{
                    if (_placeholderOpacityAnimation.isCompleted) {
                        setState(() => {});
                    }
                });

                List<TweenSequenceItem<float>> list2 = new List<TweenSequenceItem<float>>();
                list2.Add(new TweenSequenceItem<float>(
                    tween: new ConstantTween<float>(0),
                    weight: (float) widget.fadeOutDuration?.Milliseconds
                ));
                list2.Add(new TweenSequenceItem<float>(
                    tween: _targetOpacity.chain(new CurveTween(curve: widget.fadeInCurve)),
                    weight: (float) widget.fadeInDuration?.Milliseconds
                ));
                _targetOpacityAnimation = animation.drive(new TweenSequence<float>(list2));
                if (!widget.isTargetLoaded && _isValid(_placeholderOpacity) && _isValid(_targetOpacity)) {
                    controller.setValue(controller.upperBound);
                }
            }

            bool _isValid(Tween<float> tween) {
                return tween?.begin != null && tween?.end != null;
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
        
    }
}