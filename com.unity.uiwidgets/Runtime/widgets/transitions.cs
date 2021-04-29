using Unity.UIWidgets.animation;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.rendering;
using Unity.UIWidgets.ui;
using UnityEngine;
using Rect = Unity.UIWidgets.ui.Rect;
using TextStyle = Unity.UIWidgets.painting.TextStyle;

namespace Unity.UIWidgets.widgets {
    public abstract class AnimatedWidget : StatefulWidget {
        public readonly Listenable listenable;
        protected AnimatedWidget(
            Key key = null,
            Listenable listenable = null
            ) : base(key) {
            D.assert(listenable != null);
            this.listenable = listenable;
        }

        protected internal abstract Widget build(BuildContext context);

        public override State createState() {
            return new _AnimatedState();
        }

        public override void debugFillProperties(DiagnosticPropertiesBuilder properties) {
            base.debugFillProperties(properties);
            properties.add(new DiagnosticsProperty<Listenable>("animation", listenable));
        }
    }

    public class _AnimatedState : State<AnimatedWidget> {
        public override void initState() {
            base.initState();
            widget.listenable.addListener(_handleChange);
        }

        public override void didUpdateWidget(StatefulWidget oldWidget) {
            base.didUpdateWidget(oldWidget);
            if (widget.listenable != ((AnimatedWidget) oldWidget).listenable) {
                ((AnimatedWidget) oldWidget).listenable.removeListener(_handleChange);
                widget.listenable.addListener(_handleChange);
            }
        }

        public override void dispose() {
            widget.listenable.removeListener(_handleChange);
            base.dispose();
        }

        void _handleChange() {
            setState(() => {
                // The listenable's state is our build state, and it changed already.
            });
        }

        public override Widget build(BuildContext context) {
            return widget.build(context);
        }
    }

    public class SlideTransition : AnimatedWidget {
        public SlideTransition(
            Key key = null,
            Animation<Offset> position = null,
            bool transformHitTests = true,
            TextDirection? textDirection = null,
            Widget child = null
            ) : base(key: key, listenable: position) {
            D.assert(position != null);
            this.transformHitTests = transformHitTests;
            this.textDirection = textDirection;
            this.child = child;
        }

        public Animation<Offset> position {
            get { return (Animation<Offset>) listenable; }
        }

        public readonly TextDirection? textDirection;

        public readonly bool transformHitTests;

        public readonly Widget child;

        protected internal override Widget build(BuildContext context) {
            var offset = position.value;
            if (textDirection == TextDirection.rtl) {
                offset = new Offset(-offset.dx, offset.dy);
            }

            return new FractionalTranslation(
                translation: offset,
                transformHitTests: transformHitTests,
                child: child
            );
        }
    }


    public class ScaleTransition : AnimatedWidget {
        public ScaleTransition(
            Key key = null,
            Animation<float> scale = null,
            Alignment alignment = null,
            Widget child = null
        ) : base(key: key, listenable: scale) {
            alignment = alignment ?? Alignment.center;
            D.assert(scale != null);
            this.alignment = alignment;
            this.child = child;
        }

        public Animation<float> scale {
            get { return (Animation<float>) listenable; }
        }

        public readonly Alignment alignment;

        public readonly Widget child;

        protected internal override Widget build(BuildContext context) {
            float scaleValue = scale.value;
            Matrix4 transform = Matrix4.identity();
            transform.scale(scaleValue, scaleValue, 1.0f);
            return new Transform(
                transform: transform,
                alignment: alignment,
                child: child
            );
        }
    }


    public class RotationTransition : AnimatedWidget {
        public RotationTransition(
            Key key = null,
            Animation<float> turns = null,
            Alignment alignment = null,
            Widget child = null) : 
            base(key: key, listenable: turns) {
            D.assert(turns != null);
            this.alignment = alignment ?? Alignment.center;
            this.child = child;
        }

        public Animation<float> turns {
            get { return (Animation<float>) listenable; }
        }

        public readonly Alignment alignment;

        public readonly Widget child;

        protected internal override Widget build(BuildContext context) {
            float turnsValue = turns.value;
            Matrix4 transform = Matrix4.rotationZ((turnsValue * Mathf.PI * 2.0f));
            return new Transform(
                transform: transform,
                alignment: alignment,
                child: child);
        }
    }

    public class SizeTransition : AnimatedWidget {
        public SizeTransition(
            Key key = null,
            Axis axis = Axis.vertical,
            Animation<float> sizeFactor = null,
            float axisAlignment = 0.0f,
            Widget child = null) 
            : base(key: key, listenable: sizeFactor) {
            D.assert(sizeFactor != null);
            this.axis = axis;
            this.axisAlignment = axisAlignment;
            this.child = child;
        }

        public readonly Axis axis;

        public readonly float axisAlignment;

        Animation<float> sizeFactor {
            get { return (Animation<float>) listenable; }
        }

        public readonly Widget child;

        protected internal override Widget build(BuildContext context) {
            Alignment alignment;
            if (axis == Axis.vertical) {
                alignment = new Alignment(-1.0f, axisAlignment);
            }
            else {
                alignment = new Alignment(axisAlignment, -1.0f);
            }

            return new ClipRect(
                child: new Align(
                    alignment: alignment,
                    widthFactor: axis == Axis.horizontal ? (float?) Mathf.Max(sizeFactor.value, 0.0f) : null,
                    heightFactor: axis == Axis.vertical ? (float?) Mathf.Max(sizeFactor.value, 0.0f) : null,
                    child: child
                )
            );
        }
    }

    public class FadeTransition : SingleChildRenderObjectWidget {
        public FadeTransition(
            Key key = null, 
            Animation<float> opacity = null,
            bool alwaysIncludeSemantics = false,
            Widget child = null
            ) : base(key: key, child: child) {
            D.assert(opacity != null);
            this.opacity = opacity;
            this.alwaysIncludeSemantics = alwaysIncludeSemantics;
        }

        public readonly Animation<float> opacity;
        public readonly bool alwaysIncludeSemantics;
        public override RenderObject createRenderObject(BuildContext context) {
            return new RenderAnimatedOpacity(
                opacity: opacity
                //alwaysIncludeSemantics: alwaysIncludeSemantics
            );
        }

        public override void updateRenderObject(BuildContext context, RenderObject renderObject) {
            ((RenderAnimatedOpacity) renderObject).opacity = opacity;
            //((RenderAnimatedOpacity) renderObject).alwaysIncludeSemantics = alwaysIncludeSemantics;
        }

        public override void debugFillProperties(DiagnosticPropertiesBuilder properties) {
            base.debugFillProperties(properties);
            properties.add(new DiagnosticsProperty<Animation<float>>("opacity", opacity));
            properties.add(new FlagProperty("alwaysIncludeSemantics", value: alwaysIncludeSemantics, ifTrue: "alwaysIncludeSemantics"));
        }
    }

    public class SliverFadeTransition : SingleChildRenderObjectWidget {
        public SliverFadeTransition(
            Animation<float> opacity = null,
            Key key = null,
            Widget sliver = null
        ) : base(key: key, child: sliver) {
            D.assert(opacity != null);
            this.opacity = opacity;
        }
        public readonly Animation<float> opacity;
        public override RenderObject createRenderObject(BuildContext context) {
            return new RenderSliverAnimatedOpacity(
                opacity: opacity
            );
        }
        
        public override void updateRenderObject(BuildContext context, RenderObject renderObject) {
            renderObject = (RenderSliverAnimatedOpacity)renderObject;
            ((RenderSliverAnimatedOpacity)renderObject).opacity = opacity;

        }
        
        public override void debugFillProperties(DiagnosticPropertiesBuilder properties) {
            base.debugFillProperties(properties);
            properties.add(new DiagnosticsProperty<Animation<float>>("opacity", opacity));
        }
    }
    public class RelativeRectTween : Tween<RelativeRect> {
        public RelativeRectTween(
            RelativeRect begin = null, 
            RelativeRect end = null) : base(begin: begin, end: end) {
        }

        public override RelativeRect lerp(float t) {
            return RelativeRect.lerp(begin, end, t);
        }
    }


    public class PositionedTransition : AnimatedWidget {
        public PositionedTransition(
            Animation<RelativeRect> rect ,
            Key key = null,
            Widget child = null
        ) : base(key: key, listenable: rect) {
            D.assert(rect != null);
            this.child = child;
        }

        Animation<RelativeRect> rect {
            get { return (Animation<RelativeRect>) listenable; }
        }
        public readonly Widget child;
        protected internal override Widget build(BuildContext context) {
            return Positioned.fromRelativeRect(
                rect: rect.value,
                child: child
            );
        }
    }

    public class RelativePositionedTransition : AnimatedWidget {
        public RelativePositionedTransition(
            Animation<Rect> rect,
            Size size,
            Widget child ,
            Key key = null
        ) : base(key: key, listenable: rect) {
            D.assert(rect != null);
            D.assert(size != null);
            D.assert(child != null);
            this.size = size;
            this.child = child;
        }

        Animation<Rect> rect {
            get { return (Animation<Rect>) listenable; }
        }

        public readonly Size size;

        public readonly Widget child;

        protected internal override Widget build(BuildContext context) {
            RelativeRect offsets = RelativeRect.fromSize(rect.value, size);
            return new Positioned(
                top: offsets.top,
                right: offsets.right,
                bottom: offsets.bottom,
                left: offsets.left,
                child: child
            );
        }
    }


    public class DecoratedBoxTransition : AnimatedWidget {
        public DecoratedBoxTransition(
            Animation<Decoration> decoration ,
            Widget child ,
            Key key = null, 
            DecorationPosition position = DecorationPosition.background
        ) : base(key: key, listenable: decoration) {
            D.assert(decoration != null);
            D.assert(child != null);
            this.decoration = decoration;
            this.position = position;
            this.child = child;
        }

        public readonly Animation<Decoration> decoration;

        public readonly DecorationPosition position;

        public readonly Widget child;

        protected internal override Widget build(BuildContext context) {
            return new DecoratedBox(
                decoration: decoration.value,
                position: position,
                child: child
            );
        }
    }

    public class AlignTransition : AnimatedWidget {
        public AlignTransition(
            Animation<AlignmentGeometry> alignment,
            Widget child,
            Key key = null,
            float? widthFactor = null,
            float? heightFactor = null
        ) : base(key: key, listenable: alignment) {
            D.assert(alignment != null);
            D.assert(child != null);
            this.child = child;
            this.widthFactor = widthFactor;
            this.heightFactor = heightFactor;
        }

        Animation<AlignmentGeometry> alignment {
            get { return (Animation<AlignmentGeometry>) listenable; }
        }

        public readonly float? widthFactor;

        public readonly float? heightFactor;

        public readonly Widget child;


        protected internal override Widget build(BuildContext context) {
            return new Align(
                alignment: alignment.value,
                widthFactor: widthFactor,
                heightFactor: heightFactor,
                child: child
            );
        }
    }

    public class DefaultTextStyleTransition : AnimatedWidget {
        public DefaultTextStyleTransition(
            Animation<TextStyle> style,
            Widget child,
            Key key = null,
            TextAlign? textAlign = null,
            bool softWrap = true,
            TextOverflow overflow = TextOverflow.clip,
            int? maxLines = null
        ) : base(key: key, listenable: style) {
            D.assert(style != null);
            D.assert(child != null);
            this.textAlign = textAlign;
            this.softWrap = softWrap;
            this.overflow = overflow;
            this.maxLines = maxLines;
            this.child = child;
        }

        Animation<TextStyle> style {
            get { return (Animation<TextStyle>) listenable; }
        }

        public readonly TextAlign? textAlign;

        public readonly bool softWrap;
        public readonly TextOverflow overflow;
        public readonly int? maxLines;
        public readonly Widget child;

        protected internal override Widget build(BuildContext context) {
            return new DefaultTextStyle(
                style: style.value,
                textAlign: textAlign,
                softWrap: softWrap,
                overflow: overflow,
                maxLines: maxLines,
                child: child
            );
        }
    }


    public class AnimatedBuilder : AnimatedWidget {
        public readonly TransitionBuilder builder;
        public readonly Widget child;
        public AnimatedBuilder(
            Listenable animation, 
            TransitionBuilder builder,
            Key key = null, 
            Widget child = null) : base(key, animation) {
            D.assert(builder != null);
            D.assert(animation != null);
            this.builder = builder;
            this.child = child;
        }

        protected internal override Widget build(BuildContext context) {
            return builder(context, child);
        }
    }
}